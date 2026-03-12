using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.SecretBoss {
    /// <summary>
    /// Secret Boss System - 隐藏Boss核心系统
    /// 管理隐藏Boss的出现条件检测、生成和战斗
    /// </summary>
    public class SecretBossSystem : Node {
        // 单例
        private static SecretBossSystem _instance;
        public static SecretBossSystem Instance => _instance;
        
        // 活跃的隐藏Boss
        private Dictionary<string, SecretBossSpawnInfo> _activeBosses = new Dictionary<string, SecretBossSpawnInfo>();
        
        // 玩家击杀统计
        private Dictionary<string, int> _killCount = new Dictionary<string, int>();
        
        // 玩家连击数
        private int _currentCombo = 0;
        
        // Boss出现冷却
        private Dictionary<string, float> _bossCooldowns = new Dictionary<string, float>();
        
        // 当前区域
        private string _currentArea = "";
        
        // 当前天气
        private WeatherType _currentWeather = WeatherType.Clear;
        
        // 当前时间
        private int _currentHour = 12;
        
        // 当前月相
        private int _currentMoonPhase = 0;
        
        // 世界状态标志
        private Dictionary<string, bool> _worldFlags = new Dictionary<string, bool>();
        
        // 统计
        private int _totalBossSpawns = 0;
        private int _totalBossDefeats = 0;
        private int _totalDropsCollected = 0;
        
        // 信号
        [Signal]
        public delegate void BossSpawned(SecretBossData boss, Vector3 position);
        
        [Signal]
        public delegate void BossDefeated(SecretBossData boss);
        
        [Signal]
        public delegate void BossConditionMet(SecretBossData boss);
        
        [Signal]
        public delegate void DropCollected(string itemId, int quantity);
        
        public override void _Ready() {
            base._Ready();
            _instance = this;
            LoadData();
            GD.Print("[SecretBossSystem] 隐藏Boss系统已初始化");
        }
        
        public override void _Process(float delta) {
            base._Process(delta);
            CheckBossConditions();
            UpdateBossTimers(delta);
        }
        
        /// <summary>
        /// 检查Boss出现条件
        /// </summary>
        private void CheckBossConditions() {
            foreach (var boss in SecretBossDatabase.GetAllBosses()) {
                if (IsBossConditionMet(boss)) {
                    // 检查冷却
                    if (_bossCooldowns.ContainsKey(boss.BossId) && _bossCooldowns[boss.BossId] > 0) {
                        continue;
                    }
                    
                    // 检查是否已经有该Boss激活
                    if (_activeBosses.ContainsKey(boss.BossId) && _activeBosses[boss.BossId].IsActive) {
                        continue;
                    }
                    
                    // 生成Boss
                    SpawnBoss(boss);
                }
            }
        }
        
        /// <summary>
        /// 判断Boss出现条件是否满足
        /// </summary>
        private bool IsBossConditionMet(SecretBossData boss) {
            var condition = boss.Condition;
            
            // 检查区域要求
            if (!string.IsNullOrEmpty(condition.RequiredArea) && _currentArea != condition.RequiredArea) {
                return false;
            }
            
            // 根据条件类型检查
            switch (condition.Type) {
                case ConditionType.TimeOfDay:
                    if (condition.RequiredHourStart.HasValue && condition.RequiredHourEnd.HasValue) {
                        int start = condition.RequiredHourStart.Value;
                        int end = condition.RequiredHourEnd.Value;
                        if (start > end) {
                            // 跨午夜
                            if (_currentHour < start && _currentHour >= end) return false;
                        } else {
                            if (_currentHour < start || _currentHour >= end) return false;
                        }
                    }
                    break;
                    
                case ConditionType.Weather:
                    if (condition.RequiredWeather.HasValue && _currentWeather != condition.RequiredWeather.Value) {
                        return false;
                    }
                    break;
                    
                case ConditionType.KillCount:
                    if (!string.IsNullOrEmpty(condition.RequiredKillCount)) {
                        int killCount = _killCount.ContainsKey(condition.RequiredKillCount) 
                            ? _killCount[condition.RequiredKillCount] : 0;
                        if (killCount < condition.RequiredKillAmount) {
                            return false;
                        }
                    }
                    break;
                    
                case ConditionType.PlayerLevel:
                    if (condition.RequiredPlayerLevel.HasValue) {
                        // 需要获取玩家等级
                        int playerLevel = GetPlayerLevel();
                        if (playerLevel < condition.RequiredPlayerLevel.Value) {
                            return false;
                        }
                    }
                    break;
                    
                case ConditionType.Luck:
                    if (condition.RequiredLuck.HasValue) {
                        int playerLuck = GetPlayerLuck();
                        if (playerLuck < condition.RequiredLuck.Value) {
                            return false;
                        }
                    }
                    break;
                    
                case ConditionType.MoonPhase:
                    if (_currentMoonPhase != condition.RequiredValue) {
                        return false;
                    }
                    break;
                    
                case ConditionType.ComboCount:
                    if (_currentCombo < condition.RequiredValue) {
                        return false;
                    }
                    break;
                    
                case ConditionType.BossDefeated:
                    if (!string.IsNullOrEmpty(condition.RequiredBossDefeated)) {
                        var requiredBoss = SecretBossDatabase.GetBoss(condition.RequiredBossDefeated);
                        if (requiredBoss == null || !requiredBoss.IsDefeated) {
                            return false;
                        }
                    }
                    break;
                    
                case ConditionType.WorldFlag:
                    if (!string.IsNullOrEmpty(condition.RequiredWorldFlag)) {
                        bool flag = _worldFlags.ContainsKey(condition.RequiredWorldFlag) 
                            ? _worldFlags[condition.RequiredWorldFlag] : false;
                        if (!flag) {
                            return false;
                        }
                    }
                    break;
                    
                case ConditionType.Location:
                    // 位置检查已经在开始处处理
                    break;
            }
            
            return true;
        }
        
        /// <summary>
        /// 生成隐藏Boss
        /// </summary>
        private void SpawnBoss(SecretBossData boss) {
            // 随机位置
            Vector3 spawnPos = GetRandomSpawnPosition();
            
            var spawnInfo = new SecretBossSpawnInfo {
                BossId = boss.BossId,
                Position = spawnPos,
                SpawnTime = OS.GetTicksMsec() / 1000f,
                Duration = 300f, // 5分钟持续时间
                IsActive = true
            };
            
            _activeBosses[boss.BossId] = spawnInfo;
            _totalBossSpawns++;
            
            // 设置冷却 (24小时)
            _bossCooldowns[boss.BossId] = 86400f;
            
            // 标记为已发现
            boss.IsDiscovered = true;
            
            // 发送信号
            EmitSignal(nameof(BossSpawned), boss, spawnPos);
            
            // 显示出现消息
            ShowSpawnMessage(boss.SpawnMessage);
            
            GD.Print($"[SecretBossSystem] 隐藏Boss已生成: {boss.BossName} ({boss.BossId})");
        }
        
        /// <summary>
        /// 更新Boss计时器
        /// </summary>
        private void UpdateBossTimers(float delta) {
            List<string> expiredBosses = new List<string>();
            
            foreach (var kvp in _activeBosses) {
                if (kvp.Value.IsActive) {
                    float elapsed = OS.GetTicksMsec() / 1000f - kvp.Value.SpawnTime;
                    if (elapsed >= kvp.Value.Duration) {
                        // Boss消失
                        expiredBosses.Add(kvp.Key);
                        kvp.Value.IsActive = false;
                        GD.Print($"[SecretBossSystem] Boss超时消失: {kvp.Key}");
                    }
                }
            }
            
            // 更新冷却
            foreach (var kvp in _bossCooldowns) {
                if (kvp.Value > 0) {
                    _bossCooldowns[kvp.Key] -= delta;
                }
            }
        }
        
        /// <summary>
        /// 击败Boss
        /// </summary>
        public void DefeatBoss(string bossId) {
            var boss = SecretBossDatabase.GetBoss(bossId);
            if (boss == null) return;
            
            boss.IsDefeated = true;
            boss.DefeatCount++;
            _totalBossDefeats++;
            
            // 生成掉落
            GenerateDrops(boss);
            
            // 停用Boss
            if (_activeBosses.ContainsKey(bossId)) {
                _activeBosses[bossId].IsActive = false;
            }
            
            // 发送信号
            EmitSignal(nameof(BossDefeated), boss);
            
            GD.Print($"[SecretBossSystem] Boss被击败: {boss.BossName}, 累计击败次数: {boss.DefeatCount}");
        }
        
        /// <summary>
        /// 生成掉落物品
        /// </summary>
        private void GenerateDrops(SecretBossData boss) {
            foreach (var drop in boss.Drops) {
                bool canDrop = drop.IsGuaranteed || GD.Randf() < drop.DropRate;
                
                if (canDrop) {
                    int quantity = GD.RandRange(drop.MinQuantity, drop.MaxQuantity);
                    _totalDropsCollected += quantity;
                    
                    // 添加到玩家背包
                    AddItemToPlayer(drop.ItemId, quantity);
                    
                    // 发送信号
                    EmitSignal(nameof(DropCollected), drop.ItemId, quantity);
                    
                    GD.Print($"[SecretBossSystem] 掉落: {drop.ItemName} x{quantity}");
                }
            }
        }
        
        /// <summary>
        /// 获取玩家等级
        /// </summary>
        private int GetPlayerLevel() {
            // 从玩家数据获取等级
            // 这里简化处理，实际应该从玩家系统获取
            return 1;
        }
        
        /// <summary>
        /// 获取玩家幸运值
        /// </summary>
        private int GetPlayerLuck() {
            // 从玩家数据获取幸运值
            // 这里简化处理
            return 50;
        }
        
        /// <summary>
        /// 获取随机生成位置
        /// </summary>
        private Vector3 GetRandomSpawnPosition() {
            // 实际应该基于当前区域计算
            return new Vector3(GD.Randf() * 100 - 50, 0, GD.Randf() * 100 - 50);
        }
        
        /// <summary>
        /// 显示出现消息
        /// </summary>
        private void ShowSpawnMessage(string message) {
            // 可以通过UI系统显示
            GD.Print($"[SecretBossSystem] {message}");
        }
        
        /// <summary>
        /// 添加物品到玩家背包
        /// </summary>
        private void AddItemToPlayer(string itemId, int quantity) {
            // 实际应该调用背包系统
        }
        
        /// <summary>
        /// 增加击杀计数
        /// </summary>
        public void AddKill(string enemyType) {
            if (!_killCount.ContainsKey(enemyType)) {
                _killCount[enemyType] = 0;
            }
            _killCount[enemyType]++;
        }
        
        /// <summary>
        /// 设置当前区域
        /// </summary>
        public void SetCurrentArea(string areaId) {
            _currentArea = areaId;
        }
        
        /// <summary>
        /// 设置当前天气
        /// </summary>
        public void SetCurrentWeather(WeatherType weather) {
            _currentWeather = weather;
        }
        
        /// <summary>
        /// 设置当前时间
        /// </summary>
        public void SetCurrentTime(int hour) {
            _currentHour = hour;
        }
        
        /// <summary>
        /// 设置当前月相
        /// </summary>
        public void SetMoonPhase(int phase) {
            _currentMoonPhase = phase;
        }
        
        /// <summary>
        /// 设置连击数
        /// </summary>
        public void SetComboCount(int combo) {
            _currentCombo = combo;
        }
        
        /// <summary>
        /// 设置世界状态标志
        /// </summary>
        public void SetWorldFlag(string flag, bool value) {
            _worldFlags[flag] = value;
        }
        
        /// <summary>
        /// 获取Boss状态
        /// </summary>
        public bool IsBossActive(string bossId) {
            return _activeBosses.ContainsKey(bossId) && _activeBosses[bossId].IsActive;
        }
        
        /// <summary>
        /// 获取所有活跃的Boss
        /// </summary>
        public List<SecretBossSpawnInfo> GetActiveBosses() {
            List<SecretBossSpawnInfo> result = new List<SecretBossSpawnInfo>();
            foreach (var kvp in _activeBosses) {
                if (kvp.Value.IsActive) {
                    result.Add(kvp.Value);
                }
            }
            return result;
        }
        
        /// <summary>
        /// 获取统计信息
        /// </summary>
        public Dictionary<string, int> GetStatistics() {
            return new Dictionary<string, int> {
                { "TotalSpawns", _totalBossSpawns },
                { "TotalDefeats", _totalBossDefeats },
                { "TotalDrops", _totalDropsCollected },
                { "Discovered", SecretBossDatabase.GetDiscoveredCount() },
                { "Defeated", SecretBossDatabase.GetDefeatedCount() },
                { "TotalBosses", SecretBossDatabase.GetTotalBossCount() }
            };
        }
        
        /// <summary>
        /// 存档数据
        /// </summary>
        public Dictionary<string, object> SaveData() {
            return new Dictionary<string, object> {
                { "killCount", _killCount },
                { "worldFlags", _worldFlags },
                { "totalBossSpawns", _totalBossSpawns },
                { "totalBossDefeats", _totalBossDefeats },
                { "totalDropsCollected", _totalDropsCollected }
            };
        }
        
        /// <summary>
        /// 加载数据
        /// </summary>
        public void LoadData() {
            // 实际应该从存档加载
            // 暂时跳过
        }
    }
}
