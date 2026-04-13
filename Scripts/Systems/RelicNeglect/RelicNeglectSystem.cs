using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.RelicNeglect
{
    /// <summary>
    /// 遗物「被遗弃感」系统 — REQ-192
    /// 遗物连续10+场未使用开始呈现被冷落视觉，被使用时痕迹消失。
    /// 零数值影响，纯视觉叙事。
    /// </summary>
    public partial class RelicNeglectSystem : BaseSystem
    {
        public static RelicNeglectSystem Instance { get; private set; }

        // Signals
        public delegate void RelicReactivatedEventHandler(string relicId, RelicNeglectLevel previousLevel);
        public delegate void VisualStateChangedEventHandler(string relicId, RelicNeglectLevel newLevel, RelicNeglectLevel oldLevel);
        public delegate void SorrowfulNarrativeTriggeredEventHandler(string relicId, string narrativeText);

        public event RelicReactivatedEventHandler OnRelicReactivated;
        public event VisualStateChangedEventHandler OnVisualStateChanged;
        public event SorrowfulNarrativeTriggeredEventHandler OnSorrowfulNarrativeTriggered;

        // 阈值常量
        private const int WORRIED_THRESHOLD = 5;
        private const int NEGLECT_THRESHOLD = 10;
        private const int SORROW_THRESHOLD = 20;
        private const int DESPAIR_THRESHOLD = 30;

        // 已追踪的遗物ID集合（用于知道哪些遗物"拥有"但未装备）
        private HashSet<string> _ownedRelicIds = new();
        // 上一次的视觉状态（用于检测变化）
        private Dictionary<string, RelicNeglectLevel> _previousLevel = new();

        public override void _Ready()
        {
            Instance = this;
            RelicNeglectDatabase.Initialize();
            SubscribeToSignals();
            GD.Print("[RelicNeglectSystem] Initialized");
        }

        #region 信号订阅

        private void SubscribeToSignals()
        {
            // 订阅 RelicSystem 信号
            if (RelicSystem.Instance != null)
            {
                if (RelicSystem.Instance.HasSignal("RelicEquipped"))
                    RelicSystem.Instance.Connect("RelicEquipped", Callable.From<string>(OnRelicEquipped));
                if (RelicSystem.Instance.HasSignal("RelicUnequipped"))
                    RelicSystem.Instance.Connect("RelicUnequipped", Callable.From<string>(OnRelicUnequipped));

                // 尝试获取已装备遗物列表初始化 _ownedRelicIds
                TryInitializeOwnedRelics();
                GD.Print("[RelicNeglectSystem] Subscribed to RelicSystem signals");
            }
            else
            {
                // RelicSystem 还没准备好，延迟重试
                GD.Print("[RelicNeglectSystem] RelicSystem not ready, retrying in 1 second...");
                var timer = new Godot.Timer { OneShot = true, WaitTime = 1.0f };
                AddChild(timer);
                timer.Timeout += () => {
                    timer.QueueFree();
                    SubscribeToSignals();
                };
                timer.Start();
            }

            // 订阅战斗结束信号（来自 CombatManager）
            var combatManager = GetNodeOrNull<Godot.Node>("/root/CombatManager");
            if (combatManager != null)
            {
                if (combatManager.HasSignal("CombatEnded"))
                    combatManager.Connect("CombatEnded", Callable.From<Godot.Collections.Dictionary>(OnCombatEnded));
                GD.Print("[RelicNeglectSystem] Subscribed to CombatManager signals");
            }
        }

        private void TryInitializeOwnedRelics()
        {
            try
            {
                if (RelicSystem.Instance == null) return;
                var equipped = RelicSystem.Instance.GetEquippedRelics();
                foreach (var relic in equipped)
                {
                    _ownedRelicIds.Add(relic.Id);
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[RelicNeglectSystem] Failed to initialize owned relics: {ex.Message}");
            }
        }

        #endregion

        #region 信号处理

        /// <summary>
        /// 遗物被装备时调用 — 重置未使用计数，触发视觉恢复
        /// </summary>
        private void OnRelicEquipped(string relicId)
        {
            if (string.IsNullOrEmpty(relicId)) return;

            var entry = RelicNeglectDatabase.GetOrCreateEntry(relicId);
            RelicNeglectLevel previousLevel = entry.GetVisualLevel();

            // 重置计数
            entry.ConsecutiveBattlesUnused = 0;
            entry.TotalTimesActivated++;
            entry.LastActivatedTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            entry.HasShownSorrowfulNarrative = false;

            // 如果之前不是 Active 状态，触发视觉恢复
            if (previousLevel > RelicNeglectLevel.Active)
            {
                GD.Print($"[RelicNeglectSystem] Relic {relicId} reactivated, visual state: {previousLevel} -> Active");
                OnRelicReactivated?.Invoke(relicId, previousLevel);
            }

            OnVisualStateChanged?.Invoke(relicId, RelicNeglectLevel.Active, previousLevel);
            _previousLevel[relicId] = RelicNeglectLevel.Active;
        }

        /// <summary>
        /// 遗物被卸下时调用
        /// </summary>
        private void OnRelicUnequipped(string relicId)
        {
            if (string.IsNullOrEmpty(relicId)) return;
            _ownedRelicIds.Add(relicId);
        }

        /// <summary>
        /// 战斗结束时批量更新计数 — 对所有携带的遗物累加未使用场次
        /// </summary>
        private void OnCombatEnded(Godot.Collections.Dictionary battleData)
        {
            if (RelicSystem.Instance == null) return;

            try
            {
                // 获取当前装备的遗物
                var equipped = RelicSystem.Instance.GetEquippedRelics();
                var equippedIds = new HashSet<string>();
                foreach (var r in equipped)
                {
                    equippedIds.Add(r.Id);
                    _ownedRelicIds.Add(r.Id);
                }

                // 更新所有已拥有的遗物（装备的和不装备的）
                foreach (var relicId in _ownedRelicIds)
                {
                    var entry = RelicNeglectDatabase.GetOrCreateEntry(relicId);
                    entry.TotalBattlesCarried++;

                    // 只有未装备的遗物才累加"未使用"计数
                    if (!equippedIds.Contains(relicId))
                    {
                        entry.ConsecutiveBattlesUnused++;
                    }
                    // 装备的遗物在 OnRelicEquipped 里已经重置了

                    // 检查是否需要升级视觉状态
                    RelicNeglectLevel oldLevel = _previousLevel.TryGetValue(relicId, out var prev) ? prev : RelicNeglectLevel.Active;
                    RelicNeglectLevel newLevel = entry.GetVisualLevel();

                    if (newLevel != oldLevel)
                    {
                        _previousLevel[relicId] = newLevel;
                        OnVisualStateChanged?.Invoke(relicId, newLevel, oldLevel);

                        // 首次进入 Sorrowful 状态时触发叙事文字
                        if (newLevel == RelicNeglectLevel.Sorrowful && !entry.HasShownSorrowfulNarrative)
                        {
                            TriggerSorrowfulNarrative(relicId, entry);
                        }

                        GD.Print($"[RelicNeglectSystem] Relic {relicId} visual state changed: {oldLevel} -> {newLevel}");
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[RelicNeglectSystem] Error in OnCombatEnded: {ex.Message}");
            }
        }

        #endregion

        #region 叙事触发

        private void TriggerSorrowfulNarrative(string relicId, RelicNeglectEntry entry)
        {
            entry.HasShownSorrowfulNarrative = true;

            // 获取遗物名称（如果有的话）
            string relicName = relicId;
            try
            {
                if (RelicSystem.Instance != null)
                {
                    var allRelics = RelicSystem.Instance.GetAllRelics();
                    if (allRelics != null)
                    {
                        foreach (var r in allRelics)
                        {
                            if (r.Id == relicId && !string.IsNullOrEmpty(r.Name))
                            {
                                relicName = r.Name;
                                break;
                            }
                        }
                    }
                }
            }
            catch { /* ignore */ }

            string[] messages = new string[]
            {
                $"{relicName} 已经很久没有被使用了...",
                $"{relicName} 似乎在等待什么",
                $"{relicName} 的光芒黯淡了些许"
            };

            string message = messages[Math.abs(relicId.GetHashCode()) % messages.Length];
            GD.Print($"[RelicNeglectSystem] Sorrowful narrative for {relicId}: {message}");
            OnSorrowfulNarrativeTriggered?.Invoke(relicId, message);
        }

        #endregion

        #region 公共 API

        /// <summary>
        /// 获取遗物的当前视觉状态等级
        /// </summary>
        public RelicNeglectLevel GetVisualState(string relicId)
        {
            return RelicNeglectDatabase.GetVisualState(relicId);
        }

        /// <summary>
        /// 获取遗物的被遗弃感数据
        /// </summary>
        public RelicNeglectEntry GetNeglectEntry(string relicId)
        {
            return RelicNeglectDatabase.GetNeglectEntry(relicId);
        }

        /// <summary>
        /// 注册拥有的遗物（用于追踪）
        /// </summary>
        public void RegisterOwnedRelic(string relicId)
        {
            _ownedRelicIds.Add(relicId);
        }

        #endregion

        #region 持久化

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            var entries = RelicNeglectDatabase.ExportAll();
            var saveList = new List<RelicNeglectEntrySaveData>();

            foreach (var entry in entries)
            {
                saveList.Add(new RelicNeglectEntrySaveData
                {
                    RelicId = entry.RelicId,
                    TotalBattlesCarried = entry.TotalBattlesCarried,
                    ConsecutiveBattlesUnused = entry.ConsecutiveBattlesUnused,
                    TotalTimesActivated = entry.TotalTimesActivated,
                    LastActivatedTimestamp = entry.LastActivatedTimestamp,
                    HasShownSorrowfulNarrative = entry.HasShownSorrowfulNarrative
                });
            }

            data["OwnedRelicIds"] = new List<string>(_ownedRelicIds);
            data["Entries"] = saveList;
            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null || data.Count == 0) return;

            // 恢复拥有列表
            if (data.TryGetValue("OwnedRelicIds", out var ownedObj) && ownedObj is List<object> ownedList)
            {
                _ownedRelicIds.Clear();
                foreach (var item in ownedList)
                {
                    _ownedRelicIds.Add(item?.ToString() ?? "");
                }
            }

            // 恢复被遗弃感数据
            if (data.TryGetValue("Entries", out var entriesObj) && entriesObj is List<object> entriesList)
            {
                var saveEntries = new List<RelicNeglectEntry>();
                foreach (var obj in entriesList)
                {
                    if (obj is Dictionary<string, object> dict)
                    {
                        var saveEntry = new RelicNeglectEntry
                        {
                            RelicId = dict.TryGetValue("RelicId", out var rid) ? rid?.ToString() ?? "" : "",
                            TotalBattlesCarried = dict.TryGetValue("TotalBattlesCarried", out var tbc) ? Convert.ToInt32(tbc) : 0,
                            ConsecutiveBattlesUnused = dict.TryGetValue("ConsecutiveBattlesUnused", out var cbu) ? Convert.ToInt32(cbu) : 0,
                            TotalTimesActivated = dict.TryGetValue("TotalTimesActivated", out var tta) ? Convert.ToInt32(tta) : 0,
                            LastActivatedTimestamp = dict.TryGetValue("LastActivatedTimestamp", out var lat) ? Convert.ToInt64(lat) : 0,
                            HasShownSorrowfulNarrative = dict.TryGetValue("HasShownSorrowfulNarrative", out var hssn) && hssn is true
                        };
                        saveEntries.Add(saveEntry);
                    }
                }
                RelicNeglectDatabase.LoadFromData(saveEntries);
            }

            // 重建 previousLevel 缓存
            _previousLevel.Clear();
            var allEntries = RelicNeglectDatabase.GetAllEntries();
            foreach (var kvp in allEntries)
            {
                _previousLevel[kvp.Key] = kvp.Value.GetVisualLevel();
            }

            GD.Print($"[RelicNeglectSystem] Imported {_ownedRelicIds.Count} owned relics, {allEntries.Count} neglect entries");
        }

        #endregion
    }
}
