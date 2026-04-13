// WorldEventConsequenceSystem.cs
// REQ-197: WorldEvent因果事件链 — 核心系统
// 读取 EventHistory，决定触发概率/类型/强度
// 最小侵入：只读取 WorldEventSystem 数据，不修改其核心逻辑

using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Core.Systems
{
    /// <summary>
    /// WorldEvent 因果事件链系统
    /// 三类因果规则：
    /// - 失败事件 → 怨念（下次同类型事件难度叠加）
    /// - 成功事件 → 印记（SafeHouse 视觉层）
    /// - 跳过事件 → 债务（低等级时追债）
    /// </summary>
    public partial class WorldEventConsequenceSystem : BaseSystem
    {
        // ============ Singleton ============
        public static new WorldEventConsequenceSystem Instance { get; protected set; }

        public override void _Ready()
        {
            Instance = this;
            base._Ready();
            InitializeConsequenceState();
        }

        // ============ 状态 ============
        /// <summary>每个事件类型的因果状态</summary>
        private Dictionary<WorldEventType, EventConsequenceState> _consequenceStates;

        /// <summary>活跃债务记录</summary>
        private List<DebtRecord> _activeDebts;

        /// <summary>活跃印记记录</summary>
        private List<MarkRecord> _activeMarks;

        // ============ 配置常量 ============
        private const int GRUDGE_THRESHOLD_1 = 1;   // 触发概率+10%，奖励不变
        private const int GRUDGE_THRESHOLD_3 = 3;   // 触发概率+30%，奖励-10%
        private const int GRUDGE_THRESHOLD_5 = 5;   // 触发概率+50%，奖励-20%，叙事升级

        private const float GRUDGE_BONUS_1 = 0.10f;
        private const float GRUDGE_BONUS_3 = 0.30f;
        private const float GRUDGE_BONUS_5 = 0.50f;
        private const float REWARD_PENALTY_3 = 0.10f;
        private const float REWARD_PENALTY_5 = 0.20f;

        private const float MERCHANT_SUCCESS_SPAWN_BONUS = 0.15f;   // 印记：商人来访概率+15%
        private const float BLESSING_SUCCESS_SPAWN_BONUS = 0.10f;  // 印记：Blessing触发概率+10%
        private const int REPUTATION_BONUS = 5;                    // 印记：声望+5

        private const int DEBT_GRADE_THRESHOLD = 5;  // 债务：玩家比事件等级低5级以上触发追债

        // ============ 信号 ============
        public Action<WorldEventType, int> OnGrudgeEscalated;
        public Action<WorldEventType, int> OnMarkEarned;
        public Action<WorldEventType, int> OnDebtTriggered;
        public Action<WorldEventType, int> OnDebtResolved;
        public Action<WorldEventType, string> OnConsequenceNarrativeNeeded;

        // ============ 初始化 ============
        private void InitializeConsequenceState()
        {
            _consequenceStates = new Dictionary<WorldEventType, EventConsequenceState>();
            _activeDebts = new List<DebtRecord>();
            _activeMarks = new List<MarkRecord>();

            // 初始化所有事件类型
            foreach (WorldEventType eventType in Enum.GetValues(typeof(WorldEventType)))
            {
                _consequenceStates[eventType] = new EventConsequenceState
                {
                    EventType = eventType,
                    GrudgeLevel = 0,
                    MarkCount = 0,
                    DebtCount = 0,
                    LastOutcome = EventOutcome.None,
                    LastOutcomeTimestamp = 0,
                    IsGrudgeEscalated = false,
                    DebtTriggered = false
                };
            }
        }

        // ============ 核心 API ============

        /// <summary>
        /// 获取怨念等级（用于调整事件触发概率）
        /// </summary>
        public int GetGrudgeLevel(WorldEventType eventType)
        {
            if (_consequenceStates.TryGetValue(eventType, out var state))
                return state.GrudgeLevel;
            return 0;
        }

        /// <summary>
        /// 获取调整后的触发概率（怨念加成）
        /// </summary>
        public float GetAdjustedSpawnChance(WorldEventType eventType, float baseChance)
        {
            var grudgeLevel = GetGrudgeLevel(eventType);
            float bonus = grudgeLevel switch
            {
                >= GRUDGE_THRESHOLD_5 => GRUDGE_BONUS_5,
                >= GRUDGE_THRESHOLD_3 => GRUDGE_BONUS_3,
                >= GRUDGE_THRESHOLD_1 => GRUDGE_BONUS_1,
                _ => 0f
            };
            return baseChance + bonus;
        }

        /// <summary>
        /// 获取奖励惩罚倍率（怨念导致奖励减少）
        /// </summary>
        public float GetRewardMultiplier(WorldEventType eventType)
        {
            var grudgeLevel = GetGrudgeLevel(eventType);
            return grudgeLevel switch
            {
                >= GRUDGE_THRESHOLD_5 => 1f - REWARD_PENALTY_5,
                >= GRUDGE_THRESHOLD_3 => 1f - REWARD_PENALTY_3,
                _ => 1f
            };
        }

        /// <summary>
        /// 记录一个事件结果（由 WorldEventSystem 在 CompleteEvent/FailEvent/Skip 时调用）
        /// </summary>
        public void RecordOutcome(WorldEventType eventType, EventOutcome outcome)
        {
            if (!_consequenceStates.TryGetValue(eventType, out var state))
                return;

            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            state.LastOutcome = outcome;
            state.LastOutcomeTimestamp = now;

            switch (outcome)
            {
                case EventOutcome.Success:
                    HandleSuccess(eventType, state);
                    break;
                case EventOutcome.Failed:
                    HandleFailure(eventType, state);
                    break;
                case EventOutcome.Skipped:
                    HandleSkip(eventType, state);
                    break;
            }
        }

        /// <summary>
        /// 检查是否有活跃债务需要触发（玩家低等级时）
        /// </summary>
        public List<DebtRecord> CheckActiveDebts(int currentPlayerLevel)
        {
            var triggerDebts = new List<DebtRecord>();
            foreach (var debt in _activeDebts)
            {
                if (!debt.IsResolved && currentPlayerLevel < debt.PlayerLevelAtDebt - DEBT_GRADE_THRESHOLD)
                {
                    triggerDebts.Add(debt);
                }
            }
            return triggerDebts;
        }

        /// <summary>
        /// 触发债务（生成叙事文字）
        /// </summary>
        public string GetDebtNarrative(WorldEventType eventType, int currentPlayerLevel)
        {
            if (!_consequenceStates.TryGetValue(eventType, out var state))
                return string.Empty;

            var debt = _activeDebts.Find(d => d.EventType == eventType && !d.IsResolved);
            if (debt == null)
                return string.Empty;

            return GenerateDebtNarrative(eventType, debt);
        }

        /// <summary>
        /// 结算债务（完成一次债务追踪的事件后调用）
        /// </summary>
        public void ResolveDebt(WorldEventType eventType)
        {
            var debt = _activeDebts.Find(d => d.EventType == eventType && !d.IsResolved);
            if (debt != null)
            {
                debt.IsResolved = true;
                if (_consequenceStates.TryGetValue(eventType, out var state))
                {
                    state.DebtCount = Mathf.Max(0, state.DebtCount - 1);
                }
                OnDebtResolved?.Invoke(eventType, state?.DebtCount ?? 0);
            }
        }

        /// <summary>
        /// 获取 SafeHouse 印记状态
        /// </summary>
        public List<MarkRecord> GetActiveMarks()
        {
            return _activeMarks;
        }

        /// <summary>
        /// 获取印记强度（用于 SafeHouse 视觉叠加）
        /// </summary>
        public int GetMarkIntensity(WorldEventType eventType)
        {
            var mark = _activeMarks.Find(m => m.EventType == eventType);
            return mark?.Intensity ?? 0;
        }

        /// <summary>
        /// 获取因果叙事文字（事件触发时调用，用于事件描述）
        /// </summary>
        public string GetConsequenceNarrative(WorldEventType eventType)
        {
            if (!_consequenceStates.TryGetValue(eventType, out var state))
                return string.Empty;

            // 优先检查债务叙事
            var debt = _activeDebts.Find(d => d.EventType == eventType && !d.IsResolved);
            if (debt != null)
            {
                return GenerateDebtNarrative(eventType, debt);
            }

            // 其次检查怨念叙事
            if (state.GrudgeLevel >= GRUDGE_THRESHOLD_5 && state.IsGrudgeEscalated)
            {
                return GenerateEscalatedGrudgeNarrative(eventType, state);
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取当前所有有因果状态的事件类型列表
        /// </summary>
        public List<WorldEventType> GetTypesWithConsequences()
        {
            var result = new List<WorldEventType>();
            foreach (var kvp in _consequenceStates)
            {
                if (kvp.Value.GrudgeLevel > 0 || kvp.Value.MarkCount > 0 || kvp.Value.DebtCount > 0)
                {
                    result.Add(kvp.Key);
                }
            }
            return result;
        }

        // ============ 内部处理方法 ============

        private void HandleSuccess(WorldEventType eventType, EventConsequenceState state)
        {
            state.MarkCount++;

            // 记录印记
            var existingMark = _activeMarks.Find(m => m.EventType == eventType);
            if (existingMark != null)
            {
                existingMark.Intensity++;
                existingMark.LastEarnedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
            else
            {
                _activeMarks.Add(new MarkRecord
                {
                    EventType = eventType,
                    Intensity = 1,
                    FirstEarnedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    LastEarnedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                });
            }

            // 重置怨念（成功可以减轻怨念）
            state.GrudgeLevel = Mathf.Max(0, state.GrudgeLevel - 1);

            OnMarkEarned?.Invoke(eventType, state.MarkCount);
        }

        private void HandleFailure(WorldEventType eventType, EventConsequenceState state)
        {
            state.GrudgeLevel++;

            if (state.GrudgeLevel >= GRUDGE_THRESHOLD_5)
            {
                state.IsGrudgeEscalated = true;
                OnGrudgeEscalated?.Invoke(eventType, state.GrudgeLevel);
            }
            else
            {
                OnGrudgeEscalated?.Invoke(eventType, state.GrudgeLevel);
            }
        }

        private void HandleSkip(WorldEventType eventType, EventConsequenceState state)
        {
            state.DebtCount++;

            _activeDebts.Add(new DebtRecord
            {
                EventType = eventType,
                PlayerLevelAtDebt = GetCurrentPlayerLevel(),
                DebtTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                IsResolved = false
            });

            OnDebtTriggered?.Invoke(eventType, state.DebtCount);
        }

        private int GetCurrentPlayerLevel()
        {
            // 尝试从 GameState 获取玩家等级
            // 如果没有 GameState，假设为 1（最低等级，最容易触发债务）
            try
            {
                var gameState = GetNodeOrNull("/root/GameState");
                if (gameState != null)
                {
                    var levelProperty = gameState.Get("Level");
                    if (levelProperty is int level)
                        return level;
                }
            }
            catch
            {
                // Ignore errors, return minimum level
            }
            return 1;
        }

        // ============ 叙事文字生成 ============

        private string GenerateDebtNarrative(WorldEventType eventType, DebtRecord debt)
        {
            var templates = GetDebtTemplates(eventType);
            var template = templates.Count > 0 ? templates[GD.Rand() % templates.Count] : string.Empty;
            return string.Format(template, GetEventTypeName(eventType));
        }

        private string GenerateEscalatedGrudgeNarrative(WorldEventType eventType, EventConsequenceState state)
        {
            var templates = GetEscalatedGrudgeTemplates(eventType);
            var template = templates.Count > 0 ? templates[GD.Rand() % templates.Count] : string.Empty;
            return string.Format(template, GetEventTypeName(eventType), state.GrudgeLevel);
        }

        private List<string> GetDebtTemplates(WorldEventType eventType)
        {
            return eventType switch
            {
                WorldEventType.MerchantVisit => new List<string>
                {
                    "你上次跑掉了...这次你必须买一件。",
                    "商人还记得你欠他的。",
                    "你欠商人一份交易。这次不能再逃了。"
                },
                WorldEventType.Blessing => new List<string>
                {
                    "你拒绝了祝福...诅咒会加倍。",
                    "神圣的光芒还记得你的拒绝。",
                    "你逃得过一时，逃不过一世的债。"
                },
                WorldEventType.MonsterSurge => new List<string>
                {
                    "你上次跑出了怪物群...它们追上来了。",
                    "Invasion的债务，现在来还。",
                    "怪物们还记得你的逃跑。"
                },
                WorldEventType.Curse => new List<string>
                {
                    "你上次逃过了诅咒...它追上来了。",
                    "黑暗还记得你的逃避。",
                    "有些东西，逃不掉。"
                },
                _ => new List<string>
                {
                    "这个世界记得你上次的逃避。",
                    "你欠它一个交代。",
                    "跑不掉的。"
                }
            };
        }

        private List<string> GetEscalatedGrudgeTemplates(WorldEventType eventType)
        {
            return eventType switch
            {
                WorldEventType.MerchantVisit => new List<string>
                {
                    "你第{1}次输给了商人...这次交易，你付双倍。",
                    "商人已经盯上你了。第{1}次失败，条件更苛刻。"
                },
                WorldEventType.MonsterSurge => new List<string>
                {
                    "你被这群怪物击退了{1}次...它们更强了。",
                    "你的失败已经传遍了整个地牢。第{1}次，敌人更凶残。"
                },
                WorldEventType.TreasureSpawn => new List<string>
                {
                    "第{1}次空手而归...宝藏在嘲笑你。",
                    "你已经错过{1}次机会了。这次还有吗？"
                },
                _ => new List<string>
                {
                    "你已经在这个事件上失败了{1}次。",
                    "第{1}次失败...它会更强。",
                    "输得越多，它就越不放过你。"
                }
            };
        }

        private string GetEventTypeName(WorldEventType eventType)
        {
            return eventType switch
            {
                WorldEventType.TreasureSpawn => "宝藏事件",
                WorldEventType.MonsterSurge => "怪物袭击",
                WorldEventType.MerchantVisit => "商人来访",
                WorldEventType.WeatherChange => "天气变化",
                WorldEventType.Blessing => "祝福",
                WorldEventType.Curse => "诅咒",
                WorldEventType.RareSpawn => "稀有生物",
                WorldEventType.ResourceBurst => "资源爆发",
                WorldEventType.Portal => "神秘传送门",
                WorldEventType.NPCrescue => "NPC营救",
                _ => "世界事件"
            };
        }

        // ============ 持久化 ============

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();

            // 序列化因果状态
            var stateList = new List<Dictionary<string, object>>();
            foreach (var kvp in _consequenceStates)
            {
                var state = kvp.Value;
                stateList.Add(new Dictionary<string, object>
                {
                    ["eventType"] = state.EventType.ToString(),
                    ["grudgeLevel"] = state.GrudgeLevel,
                    ["markCount"] = state.MarkCount,
                    ["debtCount"] = state.DebtCount,
                    ["lastOutcome"] = state.LastOutcome.ToString(),
                    ["lastOutcomeTimestamp"] = state.LastOutcomeTimestamp,
                    ["isGrudgeEscalated"] = state.IsGrudgeEscalated,
                    ["debtTriggered"] = state.DebtTriggered
                });
            }
            data["consequenceStates"] = stateList;

            // 序列化债务记录
            var debtList = new List<Dictionary<string, object>>();
            foreach (var debt in _activeDebts)
            {
                debtList.Add(new Dictionary<string, object>
                {
                    ["eventType"] = debt.EventType.ToString(),
                    ["playerLevelAtDebt"] = debt.PlayerLevelAtDebt,
                    ["debtTimestamp"] = debt.DebtTimestamp,
                    ["isResolved"] = debt.IsResolved
                });
            }
            data["activeDebts"] = debtList;

            // 序列化印记记录
            var markList = new List<Dictionary<string, object>>();
            foreach (var mark in _activeMarks)
            {
                markList.Add(new Dictionary<string, object>
                {
                    ["eventType"] = mark.EventType.ToString(),
                    ["intensity"] = mark.Intensity,
                    ["firstEarnedTimestamp"] = mark.FirstEarnedTimestamp,
                    ["lastEarnedTimestamp"] = mark.LastEarnedTimestamp
                });
            }
            data["activeMarks"] = markList;

            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            InitializeConsequenceState();

            // 反序列化因果状态
            if (data.TryGetValue("consequenceStates", out var stateObj) && stateObj is List<object> stateList)
            {
                foreach (var item in stateList)
                {
                    if (item is Dictionary<string, object> stateDict)
                    {
                        var eventTypeStr = stateDict.TryGetValue("eventType", out var et) ? et?.ToString() : string.Empty;
                        if (Enum.TryParse<WorldEventType>(eventTypeStr, out var eventType) &&
                            _consequenceStates.TryGetValue(eventType, out var state))
                        {
                            if (stateDict.TryGetValue("grudgeLevel", out var gl))
                                state.GrudgeLevel = Convert.ToInt32(gl);
                            if (stateDict.TryGetValue("markCount", out var mc))
                                state.MarkCount = Convert.ToInt32(mc);
                            if (stateDict.TryGetValue("debtCount", out var dc))
                                state.DebtCount = Convert.ToInt32(dc);
                            if (stateDict.TryGetValue("lastOutcome", out var lo))
                                Enum.TryParse<EventOutcome>(lo?.ToString(), out state.LastOutcome);
                            if (stateDict.TryGetValue("lastOutcomeTimestamp", out var lt))
                                state.LastOutcomeTimestamp = Convert.ToInt64(lt);
                            if (stateDict.TryGetValue("isGrudgeEscalated", out var ge))
                                state.IsGrudgeEscalated = Convert.ToBoolean(ge);
                            if (stateDict.TryGetValue("debtTriggered", out var dt))
                                state.DebtTriggered = Convert.ToBoolean(dt);
                        }
                    }
                }
            }

            // 反序列化债务记录
            _activeDebts.Clear();
            if (data.TryGetValue("activeDebts", out var debtObj) && debtObj is List<object> debtList)
            {
                foreach (var item in debtList)
                {
                    if (item is Dictionary<string, object> debtDict)
                    {
                        var eventTypeStr = debtDict.TryGetValue("eventType", out var et) ? et?.ToString() : string.Empty;
                        if (Enum.TryParse<WorldEventType>(eventTypeStr, out var eventType))
                        {
                            _activeDebts.Add(new DebtRecord
                            {
                                EventType = eventType,
                                PlayerLevelAtDebt = debtDict.TryGetValue("playerLevelAtDebt", out var pl) ? Convert.ToInt32(pl) : 1,
                                DebtTimestamp = debtDict.TryGetValue("debtTimestamp", out var dt2) ? Convert.ToInt64(dt2) : 0,
                                IsResolved = debtDict.TryGetValue("isResolved", out var ir) ? Convert.ToBoolean(ir) : false
                            });
                        }
                    }
                }
            }

            // 反序列化印记记录
            _activeMarks.Clear();
            if (data.TryGetValue("activeMarks", out var markObj) && markObj is List<object> markList)
            {
                foreach (var item in markList)
                {
                    if (item is Dictionary<string, object> markDict)
                    {
                        var eventTypeStr = markDict.TryGetValue("eventType", out var et) ? et?.ToString() : string.Empty;
                        if (Enum.TryParse<WorldEventType>(eventTypeStr, out var eventType))
                        {
                            _activeMarks.Add(new MarkRecord
                            {
                                EventType = eventType,
                                Intensity = markDict.TryGetValue("intensity", out var i) ? Convert.ToInt32(i) : 1,
                                FirstEarnedTimestamp = markDict.TryGetValue("firstEarnedTimestamp", out var ft) ? Convert.ToInt64(ft) : 0,
                                LastEarnedTimestamp = markDict.TryGetValue("lastEarnedTimestamp", out var lt2) ? Convert.ToInt64(lt2) : 0
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取印记加成（商人来访概率、Blessing触发概率）
        /// </summary>
        public float GetMarkSpawnBonus(WorldEventType eventType)
        {
            if (!_consequenceStates.TryGetValue(eventType, out var state))
                return 0f;

            if (state.MarkCount <= 0)
                return 0f;

            return eventType switch
            {
                WorldEventType.MerchantVisit => MERCHANT_SUCCESS_SPAWN_BONUS,
                WorldEventType.Blessing => BLESSING_SUCCESS_SPAWN_BONUS,
                WorldEventType.MonsterSurge => 0f, // 印记不加成袭击概率
                _ => 0f
            };
        }
    }
}
