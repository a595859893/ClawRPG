using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Systems.PetMimicry
{
    /// <summary>
    /// 房间环境类型标签 — 支持多标签组合（flags）
    /// 用于宠物行为印记系统：宠物根据房间环境类型记录主人的行为
    /// </summary>
    [Flags]
    public enum RoomEnvironmentType
    {
        /// <summary>无特殊环境（默认普通房间）</summary>
        None = 0,

        /// <summary>火系环境 — 厨房、火源、熔岩等</summary>
        Fire = 1 << 0,

        /// <summary>冰系环境 — 冰霜、寒冷区域</summary>
        Ice = 1 << 1,

        /// <summary>陷阱密集区 — 机关密集的房间</summary>
        TrapDense = 1 << 2,

        /// <summary>Boss房间</summary>
        Boss = 1 << 3,

        /// <summary>撤退/逃生通道区</summary>
        Escape = 1 << 4,

        /// <summary>宝藏房间 — 高价值战利品区</summary>
        Treasure = 1 << 5,

        /// <summary>休息/恢复区 — 篝火、治疗泉</summary>
        Rest = 1 << 6,

        /// <summary>谜题房间</summary>
        Puzzle = 1 << 7,

        /// <summary>精英敌人区域</summary>
        Elite = 1 << 8,

        /// <summary>普通战斗区（无特殊环境）</summary>
        Combat = 1 << 9,

        /// <summary>入口/出生区域</summary>
        Entrance = 1 << 10,

        /// <summary>毒系环境</summary>
        Poison = 1 << 11,

        /// <summary>电系/机械环境</summary>
        Electric = 1 << 12,

        /// <summary>暗系/虚空环境</summary>
        Shadow = 1 << 13,

        /// <summary>神圣环境</summary>
        Holy = 1 << 14,

        /// <summary>自然/藤蔓环境</summary>
        Nature = 1 << 15
    }

    /// <summary>
    /// 玩家行为类型 — 宠物记录的主人在房间内的行为
    /// </summary>
    public enum PlayerBehaviorType
    {
        /// <summary>使用火系技能/法术</summary>
        UseFireSkill,

        /// <summary>使用冰系技能/法术</summary>
        UseIceSkill,

        /// <summary>使用电系技能</summary>
        UseElectricSkill,

        /// <summary>使用暗系技能</summary>
        UseShadowSkill,

        /// <summary>使用神圣技能</summary>
        UseHolySkill,

        /// <summary>使用自然/藤蔓技能</summary>
        UseNatureSkill,

        /// <summary>高频率位移/闪避</summary>
        FrequentDodge,

        /// <summary>积极进攻（高攻击频率）</summary>
        AggressiveAttack,

        /// <summary>防守姿态（高防御/护盾）</summary>
        DefensiveStance,

        /// <summary>低血量时激进出击（背水一战）</summary>
        LowHPAggression,

        /// <summary>快速撤退/脱离战斗</summary>
        QuickRetreat,

        /// <summary>优先击杀精英/Boss</summary>
        FocusElite,

        /// <summary>绕路避开敌人</summary>
        AvoidCombat,

        /// <summary>触发陷阱</summary>
        TriggerTrap,

        /// <summary>解决谜题</summary>
        SolvePuzzle,

        /// <summary>收集战利品</summary>
        CollectLoot,

        /// <summary>使用恢复/治疗</summary>
        UseHealing,

        /// <summary>宠物协战</summary>
        PetSynergy,

        /// <summary>触发特殊互动</summary>
        SpecialInteraction
    }

    /// <summary>
    /// 行为印记记录 — 将环境类型与行为关联
    /// </summary>
    public class BehaviorImprint
    {
        public RoomEnvironmentType EnvironmentType { get; set; }
        public PlayerBehaviorType BehaviorType { get; set; }
        public int ImprintLevel { get; set; }       // 0-5 模仿等级
        public float Xp { get; set; }
        public DateTime LastRecordedAt { get; set; }
        public int TotalTriggers { get; set; }

        // REQ-144: 行为保真度 — 宠物模仿的精确程度 (0.0-1.0)
        // 初始值 0.3-0.6 随机，每次重复行为 +0.05，上限 1.0
        public float Fidelity { get; set; }

        // REQ-143: 衰减计时器 — 距离上次触发的时间（秒）
        // 超过 DECAY_GRACE_SECONDS 后开始衰减
        public float DecayTimer { get; set; }

        /// <summary>
        /// 获取下次升级所需 XP（对数曲线）
        /// </summary>
        public float GetXpForNextLevel()
        {
            return 10f * Mathf.Pow(2f, ImprintLevel);
        }

        /// <summary>
        /// 添加 XP，达到阈值时升级
        /// </summary>
        public bool AddXp(float amount)
        {
            Xp += amount;
            if (Xp >= GetXpForNextLevel() && ImprintLevel < 5)
            {
                ImprintLevel++;
                Xp = 0f;
                return true; // 升级了
            }
            return false;
        }

        /// <summary>
        /// REQ-144: 增加保真度（每次重复相同行为时调用）
        /// </summary>
        public void ImproveFidelity()
        {
            Fidelity = Mathf.Clamp(Fidelity + 0.05f, 0f, 1f);
        }

        /// <summary>
        /// REQ-143: 衰减检查 — 当 DecayTimer 超过阈值时降低等级
        /// 返回是否发生了衰减
        /// </summary>
        public bool CheckDecay(float graceSeconds, float decayIntervalSeconds)
        {
            if (DecayTimer >= graceSeconds)
            {
                // 每 decayIntervalSeconds 秒衰减一次
                float decayTicks = (DecayTimer - graceSeconds) / decayIntervalSeconds;
                if (decayTicks >= 1f)
                {
                    ImprintLevel = Mathf.Max(1, ImprintLevel - 1);
                    DecayTimer = graceSeconds; // 重置到宽限期边界
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// REQ-143: 重置衰减计时器（当印记被刷新时调用）
        /// </summary>
        public void ResetDecayTimer()
        {
            DecayTimer = 0f;
        }

        /// <summary>
        /// 获取保真度等级描述
        /// </summary>
        public string GetFidelityLabel()
        {
            if (Fidelity >= 0.7f) return "高保真";
            if (Fidelity >= 0.4f) return "中保真";
            return "低保真";
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // REQ-149: 性格机制层 — 条件触发系统
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 性格触发器类型 — 决定宠物性格的条件来源
    /// </summary>
    public enum PersonalityTriggerType
    {
        /// <summary>历史最多行为（原有统计逻辑）</summary>
        MostFrequent,

        /// <summary>HP状态相关触发</summary>
        HPRelated,

        /// <summary>当前环境专精触发</summary>
        EnvironmentSpecialist,

        /// <summary>近期事件驱动触发</summary>
        EventDriven,

        /// <summary>近因偏好（近期行为权重更高）</summary>
        RecentBias,

        /// <summary>某行为长期未触发被抑制</summary>
        Suppressed
    }

    /// <summary>
    /// 性格触发器 — 带条件权重计算的行为触发器
    /// </summary>
    public struct PersonalityTrigger
    {
        /// <summary>触发器类型</summary>
        public PersonalityTriggerType Type;

        /// <summary>该触发器关联的行为类型</summary>
        public PlayerBehaviorType Behavior;

        /// <summary>权重贡献（可配置）</summary>
        public float Weight;

        /// <summary>触发条件是否满足</summary>
        public bool IsActive;

        /// <summary>触发原因的简短描述（用于UI）</summary>
        public string Reason;

        public PersonalityTrigger(PersonalityTriggerType type, PlayerBehaviorType behavior, float weight, bool isActive, string reason = "")
        {
            Type = type;
            Behavior = behavior;
            Weight = weight;
            IsActive = isActive;
            Reason = reason;
        }
    }

    /// <summary>
    /// 性格分析结果 — GetDominantBehaviorEx() 返回的扩展结果
    /// </summary>
    public class PersonalityAnalysisResult
    {
        public PlayerBehaviorType? DominantBehavior;
        public float DominantScore;
        public float HistoricalScore;
        public float TriggerScore;
        public List<PersonalityTrigger> ActiveTriggers = new List<PersonalityTrigger>();
        public string Description = "";

        /// <summary>各行为类型的加权得分</summary>
        public Dictionary<PlayerBehaviorType, float> AllScores = new Dictionary<PlayerBehaviorType, float>();
    }

    /// <summary>
    /// 宠物行为模仿数据 — 全局单例，存储所有行为印记
    /// 跨游戏持久化：存档时保存，进游戏时加载
    ///
    /// REQ-149: 增加条件触发性格机制层
    /// - 不再只返回"历史最多行为"作为性格
    /// - 结合HP状态、环境专精、事件驱动等条件触发器加权计算
    /// </summary>
    public class PetMimicryData : Node
    {
        public static PetMimicryData Instance { get; private set; }

        /// <summary>
        /// 所有行为印记列表 (environment × behavior)
        /// </summary>
        private List<BehaviorImprint> _imprints = new List<BehaviorImprint>();

        /// <summary>
        /// 每种行为类型的最高等级（用于宠物个性卡）
        /// </summary>
        private Dictionary<PlayerBehaviorType, int> _highestBehaviorLevel = new Dictionary<PlayerBehaviorType, int>();

        // ══════════════════════════════════════════════════════════════════════
        // REQ-149: 条件触发状态
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>当前玩家HP百分比 (0.0-1.0)，由外部系统更新</summary>
        private float _currentHpPercent = 1.0f;

        /// <summary>当前房间环境类型，由外部系统更新</summary>
        private RoomEnvironmentType _currentEnvironment = RoomEnvironmentType.None;

        /// <summary>近期事件触发的临时性格加成（会随时间衰减）</summary>
        private Dictionary<PlayerBehaviorType, float> _eventDrivenBonus = new Dictionary<PlayerBehaviorType, float>();

        /// <summary>各触发器的默认权重配置</summary>
        private const float HP_TRIGGER_WEIGHT = 2.0f;      // HP触发权重
        private const float ENV_TRIGGER_WEIGHT = 1.5f;     // 环境专精权重
        private const float EVENT_TRIGGER_WEIGHT = 1.8f;   // 事件驱动权重
        private const float RECENT_TRIGGER_WEIGHT = 1.3f;  // 近因偏好权重
        private const float SUPPRESSED_TRIGGER_WEIGHT = -0.5f; // 抑制惩罚

        /// <summary>HP低阈值（低于此值触发谨慎性格）</summary>
        private const float HP_LOW_THRESHOLD = 0.3f;

        /// <summary>HP极低阈值（触发背水一战性格）</summary>
        private const float HP_CRITICAL_THRESHOLD = 0.15f;

        /// <summary>HP相关触发的有效窗口（秒）</summary>
        private const float HP_TRIGGER_WINDOW_SECONDS = 10f;

        /// <summary>事件驱动加成衰减时间（秒）</summary>
        private const float EVENT_BONUS_DECAY_SECONDS = 30f;

        /// <summary>某行为超过此秒数未触发开始计算抑制（秒）</summary>
        private const float SUPPRESSION_THRESHOLD_SECONDS = 120f;

        /// <summary>最近记录的印记时间（用于近因偏好）</summary>
        private DateTime _mostRecentRecordTime = DateTime.MinValue;

        // ══════════════════════════════════════════════════════════════════════
        // 公开 API — 外部系统调用
        // ══════════════════════════════════════════════════════════════════════

        public override void _Ready()
        {
            Instance = this;
            GD.Print("[PetMimicryData] Initialized");
        }

        /// <summary>
        /// 设置当前玩家HP百分比（由 PetBehaviorLogger 或其他系统调用）
        /// </summary>
        public void SetCurrentHpPercent(float hpPercent)
        {
            _currentHpPercent = Mathf.Clamp(hpPercent, 0f, 1f);
        }

        /// <summary>
        /// 设置当前房间环境类型（由 PetBehaviorLogger 调用）
        /// </summary>
        public void SetCurrentEnvironment(RoomEnvironmentType envType)
        {
            _currentEnvironment = envType;
        }

        /// <summary>
        /// 触发事件驱动的性格加成（由 PetBehaviorLogger 在特定事件时调用）
        /// 例如：救了玩家 → 勇敢性格临时提升
        /// </summary>
        public void TriggerEventDrivenBonus(PlayerBehaviorType behavior, float bonusAmount = 1.0f)
        {
            if (!_eventDrivenBonus.ContainsKey(behavior))
                _eventDrivenBonus[behavior] = 0f;
            _eventDrivenBonus[behavior] += bonusAmount;
        }

        // ══════════════════════════════════════════════════════════════════════
        // Imprint Access
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// 获取指定环境+行为类型的印记
        /// </summary>
        public BehaviorImprint GetImprint(RoomEnvironmentType envType, PlayerBehaviorType behavior)
        {
            return _imprints.Find(i =>
                i.EnvironmentType == envType && i.BehaviorType == behavior);
        }

        /// <summary>
        /// 获取指定行为类型在所有环境中的最高等级
        /// </summary>
        public int GetHighestLevel(PlayerBehaviorType behavior)
        {
            return _highestBehaviorLevel.TryGetValue(behavior, out var level) ? level : 0;
        }

        /// <summary>
        /// 获取某环境类型下的所有印记
        /// </summary>
        public List<BehaviorImprint> GetImprintsForEnvironment(RoomEnvironmentType envType)
        {
            return _imprints.FindAll(i => i.EnvironmentType == envType);
        }

        /// <summary>
        /// 获取所有印记
        /// </summary>
        public List<BehaviorImprint> GetAllImprints()
        {
            return new List<BehaviorImprint>(_imprints);
        }

        /// <summary>
        /// 添加新印记（REQ-144: 随机初始保真度 0.3-0.6）
        /// </summary>
        public void AddImprint(BehaviorImprint imprint)
        {
            // 初始化保真度为 0.3-0.6 随机
            if (imprint.Fidelity <= 0f)
                imprint.Fidelity = 0.3f + (float)GD.RandDouble() * 0.3f;
            _imprints.Add(imprint);
            UpdateHighestLevel(imprint.BehaviorType, imprint.ImprintLevel);

            if (imprint.LastRecordedAt > _mostRecentRecordTime)
                _mostRecentRecordTime = imprint.LastRecordedAt;
        }

        private void UpdateHighestLevel(PlayerBehaviorType behavior, int level)
        {
            if (!_highestBehaviorLevel.ContainsKey(behavior) ||
                _highestBehaviorLevel[behavior] < level)
            {
                _highestBehaviorLevel[behavior] = level;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // REQ-149: 条件触发性格分析
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// 获取宠物的个性类型（原有兼容方法，纯统计）
        /// </summary>
        public PlayerBehaviorType? GetDominantBehavior()
        {
            return GetDominantBehaviorEx().DominantBehavior;
        }

        /// <summary>
        /// REQ-149: 获取宠物的扩展性格分析结果
        /// 结合条件触发器（HP状态、环境专精、事件驱动等）加权计算
        /// </summary>
        public PersonalityAnalysisResult GetDominantBehaviorEx()
        {
            var result = new PersonalityAnalysisResult();

            // 1. 计算每种行为的历史得分
            var scores = new Dictionary<PlayerBehaviorType, float>();
            foreach (PlayerBehaviorType behavior in Enum.GetValues(typeof(PlayerBehaviorType)))
            {
                int level = GetHighestLevel(behavior);
                float historicalScore = level * 1.0f; // 基础：每级 1.0 分
                scores[behavior] = historicalScore;
            }

            // 2. HP 相关触发器
            EvaluateHpTriggers(scores, result.ActiveTriggers);

            // 3. 环境专精触发器
            EvaluateEnvironmentTriggers(scores, result.ActiveTriggers);

            // 4. 事件驱动触发器
            EvaluateEventDrivenTriggers(scores, result.ActiveTriggers);

            // 5. 近因偏好
            EvaluateRecentBias(scores, result.ActiveTriggers);

            // 6. 抑制触发器
            EvaluateSuppressedTriggers(scores, result.ActiveTriggers);

            // 7. 找到最高分
            float maxScore = 0f;
            PlayerBehaviorType? dominant = null;
            foreach (var kvp in scores)
            {
                if (kvp.Value > maxScore)
                {
                    maxScore = kvp.Value;
                    dominant = kvp.Key;
                }
            }

            result.DominantBehavior = dominant;
            result.DominantScore = maxScore;
            result.AllScores = scores;
            result.Description = BuildPersonalityDescription(result);

            return result;
        }

        /// <summary>
        /// HP相关触发器：低HP时倾向谨慎或背水一战
        /// </summary>
        private void EvaluateHpTriggers(Dictionary<PlayerBehaviorType, float> scores, List<PersonalityTrigger> triggers)
        {
            if (_currentHpPercent <= HP_LOW_THRESHOLD)
            {
                float intensity = 1f - (_currentHpPercent / HP_LOW_THRESHOLD); // 越低越强

                if (_currentHpPercent <= HP_CRITICAL_THRESHOLD)
                {
                    // 极低HP：触发背水一战
                    float bonus = HP_TRIGGER_WEIGHT * intensity * 2f;
                    AddTriggerScore(scores, PlayerBehaviorType.LowHPAggression, bonus, triggers,
                        new PersonalityTrigger(PersonalityTriggerType.HPRelated, PlayerBehaviorType.LowHPAggression,
                            bonus, true, $"HP危急({_currentHpPercent:P0})"));
                }

                // 低HP：倾向谨慎/防守
                float cautionBonus = HP_TRIGGER_WEIGHT * intensity;
                AddTriggerScore(scores, PlayerBehaviorType.QuickRetreat, cautionBonus, triggers,
                    new PersonalityTrigger(PersonalityTriggerType.HPRelated, PlayerBehaviorType.QuickRetreat,
                        cautionBonus, true, $"HP低({_currentHpPercent:P0})"));
                AddTriggerScore(scores, PlayerBehaviorType.DefensiveStance, cautionBonus * 0.7f, triggers,
                    new PersonalityTrigger(PersonalityTriggerType.HPRelated, PlayerBehaviorType.DefensiveStance,
                        cautionBonus * 0.7f, true, $"HP低({_currentHpPercent:P0})"));
            }
        }

        /// <summary>
        /// 环境专精触发器：当前环境匹配时强化对应行为
        /// </summary>
        private void EvaluateEnvironmentTriggers(Dictionary<PlayerBehaviorType, float> scores, List<PersonalityTrigger> triggers)
        {
            if (_currentEnvironment == RoomEnvironmentType.None) return;

            // 查找当前环境下已有的印记
            var envImprints = GetImprintsForEnvironment(_currentEnvironment);
            foreach (var imprint in envImprints)
            {
                if (imprint.ImprintLevel > 0)
                {
                    float bonus = ENV_TRIGGER_WEIGHT * (imprint.ImprintLevel / 5f); // 最高级时加成最大
                    AddTriggerScore(scores, imprint.BehaviorType, bonus, triggers,
                        new PersonalityTrigger(PersonalityTriggerType.EnvironmentSpecialist, imprint.BehaviorType,
                            bonus, true, $"当前环境专精(Lv.{imprint.ImprintLevel})"));
                }
            }

            // Boss房间：倾向优先击杀
            if (_currentEnvironment.HasFlag(RoomEnvironmentType.Boss))
            {
                float bossBonus = ENV_TRIGGER_WEIGHT * 1.2f;
                AddTriggerScore(scores, PlayerBehaviorType.FocusElite, bossBonus, triggers,
                    new PersonalityTrigger(PersonalityTriggerType.EnvironmentSpecialist, PlayerBehaviorType.FocusElite,
                        bossBonus, true, "Boss房间"));
            }

            // 宝藏房间：倾向收集
            if (_currentEnvironment.HasFlag(RoomEnvironmentType.Treasure))
            {
                float treasureBonus = ENV_TRIGGER_WEIGHT * 1.0f;
                AddTriggerScore(scores, PlayerBehaviorType.CollectLoot, treasureBonus, triggers,
                    new PersonalityTrigger(PersonalityTriggerType.EnvironmentSpecialist, PlayerBehaviorType.CollectLoot,
                        treasureBonus, true, "宝藏房间"));
            }

            // 陷阱密集区：倾向躲避
            if (_currentEnvironment.HasFlag(RoomEnvironmentType.TrapDense))
            {
                float trapBonus = ENV_TRIGGER_WEIGHT * 0.8f;
                AddTriggerScore(scores, PlayerBehaviorType.AvoidCombat, trapBonus, triggers,
                    new PersonalityTrigger(PersonalityTriggerType.EnvironmentSpecialist, PlayerBehaviorType.AvoidCombat,
                        trapBonus, true, "陷阱密集区"));
            }
        }

        /// <summary>
        /// 事件驱动触发器：特定事件临时提升相关性格
        /// </summary>
        private void EvaluateEventDrivenTriggers(Dictionary<PlayerBehaviorType, float> scores, List<PersonalityTrigger> triggers)
        {
            foreach (var kvp in _eventDrivenBonus)
            {
                if (kvp.Value > 0.01f)
                {
                    AddTriggerScore(scores, kvp.Key, kvp.Value * EVENT_TRIGGER_WEIGHT, triggers,
                        new PersonalityTrigger(PersonalityTriggerType.EventDriven, kvp.Key,
                            kvp.Value * EVENT_TRIGGER_WEIGHT, true, $"事件加成({kvp.Value:F1})"));
                }
            }
        }

        /// <summary>
        /// 近因偏好：最近记录的行为获得额外权重
        /// </summary>
        private void EvaluateRecentBias(Dictionary<PlayerBehaviorType, float> scores, List<PersonalityTrigger> triggers)
        {
            if (_mostRecentRecordTime == DateTime.MinValue) return;

            TimeSpan elapsed = DateTime.Now - _mostRecentRecordTime;
            if (elapsed.TotalSeconds > 300) return; // 超过5分钟不触发

            // 查找最近记录的行为
            BehaviorImprint recent = null;
            foreach (var imprint in _imprints)
            {
                if (recent == null || imprint.LastRecordedAt > recent.LastRecordedAt)
                    recent = imprint;
            }

            if (recent != null && recent.ImprintLevel > 0)
            {
                float recencyFactor = Mathf.Max(0f, 1f - (float)elapsed.TotalSeconds / 300f);
                float bonus = RECENT_TRIGGER_WEIGHT * recencyFactor * (recent.ImprintLevel / 5f);
                AddTriggerScore(scores, recent.BehaviorType, bonus, triggers,
                    new PersonalityTrigger(PersonalityTriggerType.RecentBias, recent.BehaviorType,
                        bonus, true, $"近期行为({elapsed.TotalSeconds:F0}s前)"));
            }
        }

        /// <summary>
        /// 抑制触发器：某行为长期未触发时降低其竞争力
        /// </summary>
        private void EvaluateSuppressedTriggers(Dictionary<PlayerBehaviorType, float> scores, List<PersonalityTrigger> triggers)
        {
            foreach (var imprint in _imprints)
            {
                if (imprint.ImprintLevel > 0 && imprint.LastRecordedAt != default)
                {
                    TimeSpan elapsed = DateTime.Now - imprint.LastRecordedAt;
                    if (elapsed.TotalSeconds > SUPPRESSION_THRESHOLD_SECONDS)
                    {
                        float suppressionFactor = Mathf.Min(1f, (float)(elapsed.TotalSeconds - SUPPRESSION_THRESHOLD_SECONDS) / 120f);
                        float penalty = SUPPRESSED_TRIGGER_WEIGHT * suppressionFactor * imprint.ImprintLevel;
                        AddTriggerScore(scores, imprint.BehaviorType, penalty, triggers,
                            new PersonalityTrigger(PersonalityTriggerType.Suppressed, imprint.BehaviorType,
                                penalty, true, $"久未使用({elapsed.TotalMinutes:F0}min)"));
                    }
                }
            }
        }

        private void AddTriggerScore(Dictionary<PlayerBehaviorType, float> scores, PlayerBehaviorType behavior, float delta, List<PersonalityTrigger> triggers, PersonalityTrigger trigger)
        {
            if (!scores.ContainsKey(behavior))
                scores[behavior] = 0f;
            scores[behavior] += delta;
            if (trigger.IsActive)
                triggers.Add(trigger);
        }

        /// <summary>
        /// 构建性格描述字符串（用于UI显示）
        /// </summary>
        private string BuildPersonalityDescription(PersonalityAnalysisResult result)
        {
            if (result.DominantBehavior == null) return "无记录";

            var parts = new List<string>();
            parts.Add($"核心性格: {GetBehaviorDisplayName(result.DominantBehavior.Value)}");

            // 按权重排序触发器
            var sortedTriggers = result.ActiveTriggers.FindAll(t => t.IsActive && t.Weight > 0.1f);
            sortedTriggers.Sort((a, b) => b.Weight.CompareTo(a.Weight));

            if (sortedTriggers.Count > 0)
            {
                var activeReasons = new List<string>();
                foreach (var t in sortedTriggers.Take(3))
                {
                    if (!string.IsNullOrEmpty(t.Reason))
                        activeReasons.Add($"{GetBehaviorDisplayName(t.Behavior)}↑({t.Reason})");
                }
                if (activeReasons.Count > 0)
                    parts.Add("触发中: " + string.Join(", ", activeReasons));
            }

            return string.Join(" | ", parts);
        }

        /// <summary>
        /// 获取行为类型的中文显示名称
        /// </summary>
        public static string GetBehaviorDisplayName(PlayerBehaviorType behavior)
        {
            switch (behavior)
            {
                case PlayerBehaviorType.UseFireSkill: return "火系";
                case PlayerBehaviorType.UseIceSkill: return "冰系";
                case PlayerBehaviorType.UseElectricSkill: return "电系";
                case PlayerBehaviorType.UseShadowSkill: return "暗系";
                case PlayerBehaviorType.UseHolySkill: return "神圣";
                case PlayerBehaviorType.UseNatureSkill: return "自然";
                case PlayerBehaviorType.FrequentDodge: return "闪避";
                case PlayerBehaviorType.AggressiveAttack: return "进攻";
                case PlayerBehaviorType.DefensiveStance: return "防守";
                case PlayerBehaviorType.LowHPAggression: return "背水一战";
                case PlayerBehaviorType.QuickRetreat: return "撤退";
                case PlayerBehaviorType.FocusElite: return "精英杀手";
                case PlayerBehaviorType.AvoidCombat: return "回避";
                case PlayerBehaviorType.TriggerTrap: return "触发陷阱";
                case PlayerBehaviorType.SolvePuzzle: return "解谜";
                case PlayerBehaviorType.CollectLoot: return "收藏家";
                case PlayerBehaviorType.UseHealing: return "治疗";
                case PlayerBehaviorType.PetSynergy: return "协战";
                case PlayerBehaviorType.SpecialInteraction: return "特殊互动";
                default: return behavior.ToString();
            }
        }

        /// <summary>
        /// REQ-149: 获取主导性格的中文描述
        /// </summary>
        public string GetPersonalityDescription()
        {
            return GetDominantBehaviorEx().Description;
        }

        // ══════════════════════════════════════════════════════════════════════
        // 其他行为类型的等级排名
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// 获取所有行为类型的等级排名
        /// </summary>
        public List<(PlayerBehaviorType Behavior, int Level)> GetBehaviorRanking()
        {
            var result = new List<(PlayerBehaviorType, int)>();
            foreach (PlayerBehaviorType behavior in Enum.GetValues(typeof(PlayerBehaviorType)))
            {
                int level = GetHighestLevel(behavior);
                if (level > 0)
                    result.Add((behavior, level));
            }
            result.Sort((a, b) => b.Item2.CompareTo(a.Item2));
            return result;
        }

        // ══════════════════════════════════════════════════════════════════════
        // Persistence
        // ══════════════════════════════════════════════════════════════════════

        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            var imprintList = new List<Dictionary>();
            foreach (var imprint in _imprints)
            {
                imprintList.Add(new Dictionary
                {
                    { "envType", (int)imprint.EnvironmentType },
                    { "behaviorType", (int)imprint.BehaviorType },
                    { "level", imprint.ImprintLevel },
                    { "xp", imprint.Xp },
                    { "lastRecorded", imprint.LastRecordedAt.ToString("o") },
                    { "totalTriggers", imprint.TotalTriggers },
                    { "fidelity", imprint.Fidelity },
                    { "decayTimer", imprint.DecayTimer }
                });
            }
            data["imprints"] = imprintList;
            return data;
        }

        public override void ImportSaveData(Dictionary data)
        {
            _imprints.Clear();
            _highestBehaviorLevel.Clear();
            _eventDrivenBonus.Clear();

            if (data == null || !data.Contains("imprints")) return;

            var imprintList = (Godot.Collections.Array)data["imprints"];
            foreach (Dictionary imprintData in imprintList)
            {
                var imprint = new BehaviorImprint
                {
                    EnvironmentType = (RoomEnvironmentType)(int)imprintData["envType"],
                    BehaviorType = (PlayerBehaviorType)(int)imprintData["behaviorType"],
                    ImprintLevel = (int)imprintData["level"],
                    Xp = (float)(double)imprintData["xp"],
                    LastRecordedAt = DateTime.Parse((string)imprintData["lastRecorded"]),
                    TotalTriggers = (int)imprintData["totalTriggers"],
                    // REQ-144/REQ-143: 新字段，向后兼容
                    Fidelity = imprintData.Contains("fidelity") ? (float)(double)imprintData["fidelity"] : 0.5f,
                    DecayTimer = imprintData.Contains("decayTimer") ? (float)(double)imprintData["decayTimer"] : 0f
                };
                _imprints.Add(imprint);
                UpdateHighestLevel(imprint.BehaviorType, imprint.ImprintLevel);

                if (imprint.LastRecordedAt > _mostRecentRecordTime)
                    _mostRecentRecordTime = imprint.LastRecordedAt;
            }

            GD.Print($"[PetMimicryData] Loaded {_imprints.Count} behavior imprints from save");
        }

        /// <summary>
        /// 重置所有印记（用于测试或新游戏）
        /// </summary>
        public void ResetAll()
        {
            _imprints.Clear();
            _highestBehaviorLevel.Clear();
            _eventDrivenBonus.Clear();
            _mostRecentRecordTime = DateTime.MinValue;
            GD.Print("[PetMimicryData] All imprints reset");
        }
    }
}
