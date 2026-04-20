using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.BuildHistory
{
    /// <summary>
    /// Build 历史系统 — 记录每场战斗的高光/低谷时刻，run 结束时生成叙事卡
    /// 纯叙事层，不影响数值平衡
    /// </summary>
    public partial class BuildHistorySystem : BaseSystem
    {
        private static BuildHistorySystem _instance;
        public static BuildHistorySystem Instance => _instance;

        private BuildHistoryDatabase _database;

        // 当前 run 的数据（运行时收集）
        private BuildHistoryEntry _currentEntry;
        private int _totalRunsRecorded = 0;
        private int _allTimeMaxCombo = 0;
        private int _allTimeBestWinStreak = 0;
        private int _currentWinStreak = 0;
        private int _currentLossStreak = 0;

        // 历史记录（持久化）
        private List<BuildHistoryEntry> _historyEntries = new List<BuildHistoryEntry>();

        // Signals
        [Signal]
        public delegate void HistoryEntryCreatedDelegateEventHandler(string entryRunIndex);

        [Signal]
        public delegate void HighlightRecordedDelegateEventHandler(string momentTimestamp);

        [Signal]
        public delegate void LowlightRecordedDelegateEventHandler(string momentTimestamp);

        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            _database = BuildHistoryDatabase.Instance;

            SubscribeToGameEvents();
            GD.Print("[BuildHistory] 系统初始化完成");
        }

        protected override string SystemName => "BuildHistorySystem";

        #region Event Subscription

        private void SubscribeToGameEvents()
        {
            // Combat 事件 — 使用弱引用避免循环依赖
            var combatSys = GetNodeOrNull("/root/Main/Combat");
            if (combatSys != null)
            {
                if (combatSys.HasSignal("CombatStarted"))
                    combatSys.Connect("CombatStarted", new Callable(this, nameof(OnCombatStarted)), (uint)ConnectFlags.Deferred);
                if (combatSys.HasSignal("CombatEnded"))
                    combatSys.Connect("CombatEnded", new Callable(this, nameof(OnCombatEnded)), (uint)ConnectFlags.Deferred);
            }

            // SkillComboSystem — 连击完成事件
            var skillCombo = GetNodeOrNull("/root/Main/SkillCombo");
            if (skillCombo != null)
            {
                if (skillCombo.HasSignal("ComboCompleted"))
                    skillCombo.Connect("ComboCompleted", new Callable(this, nameof(OnComboCompleted)), (uint)ConnectFlags.Deferred);
                if (skillCombo.HasSignal("ComboFailed"))
                    skillCombo.Connect("ComboFailed", new Callable(this, nameof(OnComboFailed)), (uint)ConnectFlags.Deferred);
            }

            // Boss 击杀事件
            var bossSys = GetNodeOrNull("/root/Main/BossMechanics");
            if (bossSys != null)
            {
                if (bossSys.HasSignal("BossDefeated"))
                    bossSys.Connect("BossDefeated", new Callable(this, nameof(OnBossDefeated)), (uint)ConnectFlags.Deferred);
            }

            // Run 结束事件 (SealedTowerManager)
            var sealedTower = GetNodeOrNull("/root/Main/SealedTowerManager");
            if (sealedTower != null)
            {
                if (sealedTower.HasSignal("RunEnded"))
                    sealedTower.Connect("RunEnded", new Callable(this, nameof(OnRunEnded)), (uint)ConnectFlags.Deferred);
            }

            // NarrativeLogSystem 集成 — 关键时刻节点
            var narrativeLog = GetNodeOrNull("/root/Main/NarrativeLog");
            if (narrativeLog != null)
            {
                if (narrativeLog.HasSignal("KeyMomentRecorded"))
                    narrativeLog.Connect("KeyMomentRecorded", new Callable(this, nameof(OnKeyMomentRecorded)), (uint)ConnectFlags.Deferred);
            }
        }

        #endregion

        #region Event Handlers

        private void OnCombatStarted()
        {
            if (_currentEntry == null)
                StartNewEntry();
        }

        private void OnCombatEnded(bool victory, int floorsCleared)
        {
            if (_currentEntry == null)
                return;

            _currentEntry.Victory = victory;
            _currentEntry.EndTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (victory)
            {
                _currentWinStreak++;
                _currentLossStreak = 0;
                if (_currentWinStreak > _allTimeBestWinStreak)
                    _allTimeBestWinStreak = _currentWinStreak;
            }
            else
            {
                _currentLossStreak++;
                _currentWinStreak = 0;
            }

            _currentEntry.CurrentWinStreak = _currentWinStreak;
            _currentEntry.CurrentLossStreak = _currentLossStreak;

            // 更新历史高光/低谷
            UpdateHighlightAndLowlight();
        }

        private void OnComboCompleted(string comboId, int comboScore, int comboLevel)
        {
            if (_currentEntry == null)
                StartNewEntry();

            // 记录最大连击
            if (comboScore > _currentEntry.MaxComboAchieved)
                _currentEntry.MaxComboAchieved = comboScore;

            if (comboScore > _allTimeMaxCombo)
                _allTimeMaxCombo = comboScore;

            // 首次使用该 combo
            if (!_currentEntry.SeenComboIds.Contains(comboId))
            {
                _currentEntry.SeenComboIds.Add(comboId);

                // 检查是否是"常用" combo（使用了多次的）
                // 这里只记录真正新的 combo
                var moment = new HighlightMoment
                {
                    Type = HighlightType.FirstComboUse,
                    Title = "新 combo 发现",
                    Tag = comboId,
                    Value = comboScore,
                    Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                };
                moment.NarrativeText = _database.GenerateHighlightNarrative(moment, _currentEntry.RunIndex);
                AddHighlightMoment(moment);
            }
        }

        private void OnComboFailed(string comboId)
        {
            if (_currentEntry == null)
                return;

            _currentEntry.ComboFailures++;

            // 只有高频失败的 combo 才记录为低谷
            if (_currentEntry.ComboFailures >= 3)
            {
                var moment = new LowlightMoment
                {
                    Type = LowlightType.ComboFailure,
                    Title = "Combo 反复失败",
                    Tag = comboId,
                    Value = _currentEntry.ComboFailures,
                    Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                };
                moment.NarrativeText = _database.GenerateLowlightNarrative(moment, _currentEntry.RunIndex);
                AddLowlightMoment(moment);
            }
        }

        private void OnBossDefeated(string bossConfigId, string bossName, bool isFirstBlood, List<string> rewards)
        {
            if (_currentEntry == null)
                StartNewEntry();

            if (!_currentEntry.SeenBossIds.Contains(bossConfigId))
            {
                _currentEntry.SeenBossIds.Add(bossConfigId);
                _currentEntry.BossesKilled++;

                var moment = new HighlightMoment
                {
                    Type = HighlightType.BossKill,
                    Title = "Boss 击杀",
                    Tag = bossName ?? bossConfigId,
                    Value = _currentEntry.BossesKilled,
                    Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                };
                moment.NarrativeText = _database.GenerateHighlightNarrative(moment, _currentEntry.RunIndex);
                AddHighlightMoment(moment);
            }
        }

        private void OnKeyMomentRecorded(string momentType, string description, int value)
        {
            if (_currentEntry == null)
                return;

            // 从 NarrativeLogSystem 接收关键时刻
            switch (momentType)
            {
                case "clutch":
                    var clutch = new HighlightMoment
                    {
                        Type = HighlightType.Clutch,
                        Title = "极限翻盘",
                        Tag = description,
                        Value = value,
                        Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                    };
                    clutch.NarrativeText = _database.GenerateHighlightNarrative(clutch, _currentEntry.RunIndex);
                    AddHighlightMoment(clutch);
                    break;

                case "near_death":
                    var nearDeath = new LowlightMoment
                    {
                        Type = LowlightType.NearDeath,
                        Title = "险死还生",
                        Tag = description,
                        Value = value,
                        Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                    };
                    nearDeath.NarrativeText = _database.GenerateLowlightNarrative(nearDeath, _currentEntry.RunIndex);
                    AddLowlightMoment(nearDeath);
                    break;
            }
        }

        private void OnRunEnded(bool victory, int floorsCleared, int totalEnemiesDefeated)
        {
            if (_currentEntry == null)
                return;

            _currentEntry.Victory = victory;
            _currentEntry.TotalEnemiesDefeated = totalEnemiesDefeated;
            _currentEntry.EndTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // 最终评估高光/低谷
            UpdateHighlightAndLowlight();

            // 提交记录
            CommitEntry(_currentEntry);
            _currentEntry = null;
        }

        #endregion

        #region Core Logic

        private void StartNewEntry()
        {
            _totalRunsRecorded++;
            _currentEntry = new BuildHistoryEntry
            {
                RunIndex = _totalRunsRecorded,
                Victory = false,
                StartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                EndTime = 0,
                HighlightMoments = new List<HighlightMoment>(),
                LowlightMoments = new List<LowlightMoment>(),
                SeenComboIds = new HashSet<string>(),
                SeenBossIds = new HashSet<string>()
            };

            GD.Print($"[BuildHistory] 开始新记录: Run #{_totalRunsRecorded}");
        }

        private void AddHighlightMoment(HighlightMoment moment)
        {
            if (_currentEntry == null)
                return;

            _currentEntry.HighlightMoments.Add(moment);
            OnHighlightRecorded?.Invoke(moment.Timestamp.ToString());
            GD.Print($"[BuildHistory] 高光时刻: {moment.Type} - {moment.NarrativeText}");
        }

        private void AddLowlightMoment(LowlightMoment moment)
        {
            if (_currentEntry == null)
                return;

            // 去重：同类低谷只保留最严重的一个
            for (int i = 0; i < _currentEntry.LowlightMoments.Count; i++)
            {
                if (_currentEntry.LowlightMoments[i].Type == moment.Type)
                {
                    // 保留更严重的
                    if (moment.Value > _currentEntry.LowlightMoments[i].Value)
                        _currentEntry.LowlightMoments[i] = moment;
                    return;
                }
            }

            _currentEntry.LowlightMoments.Add(moment);
            OnLowlightRecorded?.Invoke(moment.Timestamp.ToString());
            GD.Print($"[BuildHistory] 低谷时刻: {moment.Type} - {moment.NarrativeText}");
        }

        private void UpdateHighlightAndLowlight()
        {
            if (_currentEntry == null)
                return;

            // 检查最大连击是否为历史级
            if (_currentEntry.MaxComboAchieved >= 20 && _currentEntry.MaxComboAchieved >= _allTimeMaxCombo * 0.8)
            {
                bool alreadyHas = false;
                foreach (var h in _currentEntry.HighlightMoments)
                {
                    if (h.Type == HighlightType.MaxCombo)
                    {
                        alreadyHas = true;
                        break;
                    }
                }
                if (!alreadyHas)
                {
                    var moment = new HighlightMoment
                    {
                        Type = HighlightType.MaxCombo,
                        Title = "历史级连击",
                        Tag = "",
                        Value = _currentEntry.MaxComboAchieved,
                        Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                    };
                    moment.NarrativeText = _database.GenerateHighlightNarrative(moment, _currentEntry.RunIndex);
                    AddHighlightMoment(moment);
                }
            }

            // 检查连败
            if (_currentLossStreak >= 3)
            {
                bool alreadyHas = false;
                foreach (var l in _currentEntry.LowlightMoments)
                {
                    if (l.Type == LowlightType.LossStreak)
                    {
                        alreadyHas = true;
                        break;
                    }
                }
                if (!alreadyHas)
                {
                    var moment = new LowlightMoment
                    {
                        Type = LowlightType.LossStreak,
                        Title = "连败低谷",
                        Tag = "",
                        Value = _currentLossStreak,
                        Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                    };
                    moment.NarrativeText = _database.GenerateLowlightNarrative(moment, _currentEntry.RunIndex);
                    AddLowlightMoment(moment);
                }
            }
        }

        private void CommitEntry(BuildHistoryEntry entry)
        {
            _historyEntries.Add(entry);

            // 限制历史记录数量（保留最近 20 局）
            while (_historyEntries.Count > 20)
                _historyEntries.RemoveAt(0);

            OnHistoryEntryCreated?.Invoke(entry.RunIndex.ToString());
            GD.Print($"[BuildHistory] Run #{entry.RunIndex} 记录已提交 (高光:{entry.HighlightMoments.Count}, 低谷:{entry.LowlightMoments.Count})");
        }

        #endregion

        #region Public API

        /// <summary>
        /// 供其他系统调用，记录自定义高光时刻
        /// </summary>
        public void RecordHighlight(HighlightType type, string title, string tag, int value)
        {
            if (_currentEntry == null)
                StartNewEntry();

            var moment = new HighlightMoment
            {
                Type = type,
                Title = title,
                Tag = tag,
                Value = value,
                Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };
            moment.NarrativeText = _database.GenerateHighlightNarrative(moment, _currentEntry.RunIndex);
            AddHighlightMoment(moment);
        }

        /// <summary>
        /// 供其他系统调用，记录自定义低谷时刻
        /// </summary>
        public void RecordLowlight(LowlightType type, string title, string tag, int value)
        {
            if (_currentEntry == null)
                StartNewEntry();

            var moment = new LowlightMoment
            {
                Type = type,
                Title = title,
                Tag = tag,
                Value = value,
                Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };
            moment.NarrativeText = _database.GenerateLowlightNarrative(moment, _currentEntry.RunIndex);
            AddLowlightMoment(moment);
        }

        /// <summary>
        /// 获取最近 N 条历史记录
        /// </summary>
        public List<BuildHistoryEntry> GetRecentHistory(int count = 10)
        {
            var result = new List<BuildHistoryEntry>();
            int start = Math.Max(0, _historyEntries.Count - count);
            for (int i = start; i < _historyEntries.Count; i++)
                result.Add(_historyEntries[i]);
            return result;
        }

        /// <summary>
        /// 获取所有历史记录
        /// </summary>
        public List<BuildHistoryEntry> GetAllHistory()
        {
            return new List<BuildHistoryEntry>(_historyEntries);
        }

        /// <summary>
        /// 获取当前 run 的运行中记录（run 结束时不要调用）
        /// </summary>
        public BuildHistoryEntry GetCurrentEntry()
        {
            return _currentEntry;
        }

        /// <summary>
        /// 获取 run 总结叙事
        /// </summary>
        public string GetRunSummaryNarrative(BuildHistoryEntry entry)
        {
            return _database.GenerateRunSummaryNarrative(entry);
        }

        /// <summary>
        /// 获取历史最高连击
        /// </summary>
        public int GetAllTimeMaxCombo() => _allTimeMaxCombo;

        /// <summary>
        /// 获取历史最佳连胜
        /// </summary>
        public int GetAllTimeBestWinStreak() => _allTimeBestWinStreak;

        #endregion

        #region Persistence

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["history_entries"] = _historyEntries;
            data["total_runs"] = _totalRunsRecorded;
            data["all_time_max_combo"] = _allTimeMaxCombo;
            data["all_time_best_win_streak"] = _allTimeBestWinStreak;
            data["current_win_streak"] = _currentWinStreak;
            data["current_loss_streak"] = _currentLossStreak;
            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            _historyEntries.Clear();

            if (data.TryGetValue("history_entries", out var entriesObj) && entriesObj is List<object> entries)
            {
                foreach (var e in entries)
                {
                    if (e is BuildHistoryEntry entry)
                        _historyEntries.Add(entry);
                }
            }

            if (data.TryGetValue("total_runs", out var runsObj) && runsObj is System.Int64 runs)
                _totalRunsRecorded = (int)runs;

            if (data.TryGetValue("all_time_max_combo", out var maxComboObj) && maxComboObj is System.Int64 maxCombo)
                _allTimeMaxCombo = (int)maxCombo;

            if (data.TryGetValue("all_time_best_win_streak", out var streakObj) && streakObj is System.Int64 streak)
                _allTimeBestWinStreak = (int)streak;

            if (data.TryGetValue("current_win_streak", out var wsObj) && wsObj is System.Int64 ws)
                _currentWinStreak = (int)ws;

            if (data.TryGetValue("current_loss_streak", out var lsObj) && lsObj is System.Int64 ls)
                _currentLossStreak = (int)ls;

            GD.Print($"[BuildHistory] 从存档加载了 {_historyEntries.Count} 条历史记录 (总run: {_totalRunsRecorded}, 历史最高连击: {_allTimeMaxCombo})");
        }

        #endregion

        #region Events

        public event Action<string> OnHistoryEntryCreated;
        public event Action<string> OnHighlightRecorded;
        public event Action<string> OnLowlightRecorded;

        #endregion
    }
}
