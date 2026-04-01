using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.WorldHeritage
{
    /// <summary>
    /// 世界遗产系统 — 记录并展示跨 run 的探索/征服痕迹
    /// 纯视觉/叙事层，不影响数值平衡
    /// </summary>
    public partial class WorldHeritageSystem : BaseSystem
    {
        private static WorldHeritageSystem _instance;
        public static WorldHeritageSystem Instance => _instance;

        private WorldHeritageDatabase _database;
        private int _totalRunsCompleted = 0;
        private int _totalVictories = 0;

        // 当前 run 的快照（运行时收集，run 结束时提交）
        private RunHeritageSnapshot _currentSnapshot;
        private int _currentRunIndex = 0;

        // 已激活的遗产 ID 集合（内存缓存）
        private HashSet<string> _activeHeritageIds = new HashSet<string>();

        // Boss 击杀计数（用于多次击杀同一 boss 的情况）
        private Dictionary<string, int> _bossKillCounts = new Dictionary<string, int>();
        private Dictionary<string, int> _secretDiscoveryCounts = new Dictionary<string, int>();

        // Signals
        public delegate void HeritageActivatedEventHandler(string recordId, HeritageRecord record);
        public delegate void HeritageReadyEventHandler(List<HeritageRecord> activeHeritages);

        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            _database = WorldHeritageDatabase.Instance;

            SubscribeToGameEvents();
            GD.Print("[WorldHeritage] 系统初始化完成");
        }

        protected override string SystemName => "WorldHeritageSystem";

        #region Event Subscription

        private void SubscribeToGameEvents()
        {
            // 订阅 Boss 被击杀事件
            if (Scripts.Systems.BossMechanics.BossMechanicsSystem.Instance != null)
            {
                Scripts.Systems.BossMechanics.BossMechanicsSystem.BossDefeated += OnBossDefeated;
            }

            // 订阅 Run 结束事件 (SealedTowerManager)
            var sealedTower = GetNodeOrNull("/root/Main/SealedTowerManager");
            if (sealedTower != null)
            {
                sealedTower.Connect("RunEnded", new Callable(this, nameof(OnRunEnded)));
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Boss 被击杀时调用 — 记录到当前 run 快照
        /// </summary>
        private void OnBossDefeated(string bossConfigId, string bossName, bool isFirstBlood, List<string> rewards)
        {
            if (_currentSnapshot == null)
                StartNewSnapshot();

            if (!_currentSnapshot.BossesDefeated.Contains(bossConfigId))
            {
                _currentSnapshot.BossesDefeated.Add(bossConfigId);
            }

            // 更新击杀计数
            if (!_bossKillCounts.ContainsKey(bossConfigId))
                _bossKillCounts[bossConfigId] = 0;
            _bossKillCounts[bossConfigId]++;

            GD.Print($"[WorldHeritage] Boss 击杀记录: {bossConfigId} (击杀次数: {_bossKillCounts[bossConfigId]})");
        }

        /// <summary>
        /// Run 结束时调用 — 提交快照并激活相应遗产
        /// </summary>
        private void OnRunEnded(bool victory, int floorsCleared, int totalEnemiesDefeated)
        {
            if (_currentSnapshot == null)
                return;

            _currentSnapshot.Victory = victory;
            _currentSnapshot.FloorsCleared = floorsCleared;
            _currentSnapshot.TotalEnemiesDefeated = totalEnemiesDefeated;

            CommitSnapshot(_currentSnapshot);
            _currentSnapshot = null;
        }

        /// <summary>
        /// 当需要记录秘密发现时由其他系统调用
        /// </summary>
        public void RecordSecretDiscovery(string secretEventId)
        {
            if (_currentSnapshot == null)
                StartNewSnapshot();

            if (!_currentSnapshot.SecretsDiscovered.Contains(secretEventId))
            {
                _currentSnapshot.SecretsDiscovered.Add(secretEventId);
            }

            if (!_secretDiscoveryCounts.ContainsKey(secretEventId))
                _secretDiscoveryCounts[secretEventId] = 0;
            _secretDiscoveryCounts[secretEventId]++;

            GD.Print($"[WorldHeritage] 秘密发现记录: {secretEventId}");
        }

        /// <summary>
        /// 当需要记录成就解锁时由其他系统调用
        /// </summary>
        public void RecordAchievement(string achievementId)
        {
            if (_currentSnapshot == null)
                StartNewSnapshot();

            if (!_currentSnapshot.AchievementsUnlocked.Contains(achievementId))
            {
                _currentSnapshot.AchievementsUnlocked.Add(achievementId);
            }

            GD.Print($"[WorldHeritage] 成就记录: {achievementId}");
        }

        #endregion

        #region Snapshot Management

        private void StartNewSnapshot()
        {
            _currentRunIndex++;
            _currentSnapshot = new RunHeritageSnapshot
            {
                RunIndex = _currentRunIndex,
                Victory = false,
                BossesDefeated = new List<string>(),
                SecretsDiscovered = new List<string>(),
                AchievementsUnlocked = new List<string>(),
                TotalEnemiesDefeated = 0,
                FloorsCleared = 0
            };
        }

        /// <summary>
        /// 提交 run 快照 — 将事件转化为遗产激活
        /// </summary>
        private void CommitSnapshot(RunHeritageSnapshot snapshot)
        {
            _totalRunsCompleted++;
            if (snapshot.Victory)
                _totalVictories++;

            var newlyActivated = new List<HeritageRecord>();

            // 1. 处理 Boss 征服痕迹
            foreach (var bossId in snapshot.BossesDefeated)
            {
                string recordId = BossIdToRecordId(bossId);
                if (recordId != null && !_activeHeritageIds.Contains(recordId))
                {
                    ActivateHeritage(recordId, snapshot.RunIndex);
                    var record = _database.GetRecord(recordId);
                    if (record != null)
                        newlyActivated.Add(record);
                }
            }

            // 2. 处理秘密发现
            foreach (var secretId in snapshot.SecretsDiscovered)
            {
                string recordId = SecretIdToRecordId(secretId);
                if (recordId != null && !_activeHeritageIds.Contains(recordId))
                {
                    ActivateHeritage(recordId, snapshot.RunIndex);
                    var record = _database.GetRecord(recordId);
                    if (record != null)
                        newlyActivated.Add(record);
                }
            }

            // 3. 处理成就铭刻
            foreach (var achieveId in snapshot.AchievementsUnlocked)
            {
                string recordId = AchieveIdToRecordId(achieveId);
                if (recordId != null && !_activeHeritageIds.Contains(recordId))
                {
                    ActivateHeritage(recordId, snapshot.RunIndex);
                    var record = _database.GetRecord(recordId);
                    if (record != null)
                        newlyActivated.Add(record);
                }
            }

            // 4. 处理"首次"特殊标记
            if (snapshot.Victory && _totalVictories == 1)
            {
                // 首次胜利
                TryActivateHeritage("achieve_first_blood", snapshot.RunIndex);
            }

            // 5. 特殊成就：100 连击
            // ComboSystem 应在达到 100 连击时调用 RecordAchievement("combo_100")

            if (newlyActivated.Count > 0)
            {
                OnHeritageReady?.Invoke(GetActiveHeritages());
                GD.Print($"[WorldHeritage] Run #{snapshot.RunIndex} 结束，激活了 {newlyActivated.Count} 个新遗产");
            }
            else
            {
                GD.Print($"[WorldHeritage] Run #{snapshot.RunIndex} 结束，无新遗产激活");
            }
        }

        private void ActivateHeritage(string recordId, int runIndex)
        {
            if (_activeHeritageIds.Contains(recordId))
                return;

            _activeHeritageIds.Add(recordId);
            _database.SetRecordActive(recordId, runIndex);

            var record = _database.GetRecord(recordId);
            if (record != null)
            {
                GD.Print($"[WorldHeritage] 激活遗产: {record.DisplayName}");
                OnHeritageActivated?.Invoke(recordId, record);
            }
        }

        private bool TryActivateHeritage(string recordId, int runIndex)
        {
            if (_activeHeritageIds.Contains(recordId))
                return false;

            ActivateHeritage(recordId, runIndex);
            return true;
        }

        #endregion

        #region ID Mapping

        /// <summary>
        /// 将 Boss config ID 映射到遗产记录 ID
        /// </summary>
        private string BossIdToRecordId(string bossConfigId)
        {
            // 已知 boss ID → 遗产 recordId 映射
            switch (bossConfigId?.ToLowerInvariant())
            {
                case "forest_guardian":
                case "boss_forest_guardian":
                    return "boss_forest_guardian";
                case "dungeon_lord":
                case "boss_dungeon_lord":
                    return "boss_dungeon_lord";
                case "fire_dragon":
                case "boss_fire_dragon":
                    return "boss_fire_dragon";
                case "tower_guardian":
                case "boss_tower_guardian":
                    return "boss_tower_guardian";
                case "king_of_shadows":
                case "boss_king_of_shadows":
                case "shadow_king":
                    return "boss_king_of_shadows";
                default:
                    // 动态生成通用 boss 征服记录（最多记录 3 个未知 boss）
                    if (bossConfigId != null && !bossConfigId.StartsWith("boss_generic_"))
                    {
                        return $"boss_conquest_{bossConfigId.ToLowerInvariant()}";
                    }
                    return null;
            }
        }

        private string SecretIdToRecordId(string secretEventId)
        {
            switch (secretEventId?.ToLowerInvariant())
            {
                case "treasure_room":
                case "secret_treasure_room":
                case "hidden_treasure":
                    return "secret_treasure_room";
                case "healing_shrine":
                case "secret_healing_shrine":
                case "shrines":
                    return "secret_healing_shrine";
                case "mysterious_merchant":
                case "merchant":
                case "secret_merchant":
                    return "secret_mysterious_merchant";
                default:
                    return null;
            }
        }

        private string AchieveIdToRecordId(string achievementId)
        {
            switch (achievementId?.ToLowerInvariant())
            {
                case "first_blood":
                case "first_kill":
                case "first_blood_achievement":
                    return "achieve_first_blood";
                case "no_hit_run":
                case "perfect_run":
                case "no_damage_run":
                    return "achieve_no_hit_run";
                case "combo_100":
                case "hundred_combo":
                case "combo_100_achievement":
                    return "achieve_100_combo";
                default:
                    return null;
            }
        }

        #endregion

        #region Public Queries

        /// <summary>
        /// 获取所有已激活的遗产
        /// </summary>
        public List<HeritageRecord> GetActiveHeritages()
        {
            return _database.GetActiveRecords();
        }

        /// <summary>
        /// 获取指定区域的已激活遗产
        /// </summary>
        public List<HeritageRecord> GetHeritagesForRegion(RegionId region)
        {
            return _database.GetRecordsByRegion(region);
        }

        /// <summary>
        /// 获取当前 run 已收集的遗产快照（运行时）
        /// </summary>
        public RunHeritageSnapshot GetCurrentSnapshot()
        {
            return _currentSnapshot;
        }

        /// <summary>
        /// 获取已激活遗产的数量
        /// </summary>
        public int GetActiveHeritageCount()
        {
            return _activeHeritageIds.Count;
        }

        /// <summary>
        /// 检查某个遗产是否已激活
        /// </summary>
        public bool IsHeritageActive(string recordId)
        {
            return _activeHeritageIds.Contains(recordId);
        }

        /// <summary>
        /// 获取运行总次数
        /// </summary>
        public int GetTotalRunsCompleted() => _totalRunsCompleted;

        /// <summary>
        /// 获取总胜利次数
        /// </summary>
        public int GetTotalVictories() => _totalVictories;

        #endregion

        #region Persistence

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["active_heritage_ids"] = new List<string>(_activeHeritageIds);
            data["total_runs"] = _totalRunsCompleted;
            data["total_victories"] = _totalVictories;
            data["boss_kill_counts"] = new Dictionary<string, int>(_bossKillCounts);
            data["secret_discovery_counts"] = new Dictionary<string, int>(_secretDiscoveryCounts);
            data["last_snapshot"] = _currentSnapshot;
            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            _activeHeritageIds.Clear();

            if (data.TryGetValue("active_heritage_ids", out var idsObj) && idsObj is List<object> ids)
            {
                foreach (var id in ids)
                {
                    if (id is string recordId)
                    {
                        _activeHeritageIds.Add(recordId);
                        _database.SetRecordActive(recordId, 0); // 从存档加载，run index 未知
                    }
                }
            }

            if (data.TryGetValue("total_runs", out var runsObj) && runsObj is System.Int64 runs)
                _totalRunsCompleted = (int)runs;

            if (data.TryGetValue("total_victories", out var victoriesObj) && victoriesObj is System.Int64 victories)
                _totalVictories = (int)victories;

            if (data.TryGetValue("boss_kill_counts", out var bkcObj) && bkcObj is Dictionary<object, object> bkc)
            {
                _bossKillCounts.Clear();
                foreach (var kv in bkc)
                {
                    if (kv.Key is string k && kv.Value is System.Int64 v)
                        _bossKillCounts[k] = (int)v;
                }
            }

            if (data.TryGetValue("secret_discovery_counts", out var sdcObj) && sdcObj is Dictionary<object, object> sdc)
            {
                _secretDiscoveryCounts.Clear();
                foreach (var kv in sdc)
                {
                    if (kv.Key is string k && kv.Value is System.Int64 v)
                        _secretDiscoveryCounts[k] = (int)v;
                }
            }

            GD.Print($"[WorldHeritage] 从存档加载了 {_activeHeritageIds.Count} 个遗产 (总 run: {_totalRunsCompleted}, 胜利: {_totalVictories})");
        }

        #endregion

        #region Events

        public event HeritageActivatedEventHandler OnHeritageActivated;
        public event HeritageReadyEventHandler OnHeritageReady;

        #endregion
    }
}
