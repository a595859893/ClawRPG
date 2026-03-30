using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Achievement
{
    /// <summary>
    /// 成就系统主控制器 - 协调各子系统
    /// </summary>
    public partial class AchievementSystem : BaseSystem
    {
        private static AchievementSystem _instance;
        public static AchievementSystem Instance => _instance;

        // 子系统
        private AchievementChecker _checker;
        private AchievementProgress _progress;
        private AchievementRewards _rewards;
        
        // 统计数据
        private int _totalKills;
        private int _bossKills;
        private int _pvpWins;
        private int _zonesDiscovered;
        private int _sealedTowerFloor;
        private int _petsCollected;
        private int _mountsCollected;
        private int _equipmentCollected;
        private int _friendsMade;
        private int _goldSpent;
        
        // Signals
        public delegate void AchievementUnlockedEventHandler(string achievementId);
        public delegate void AchievementProgressUpdatedEventHandler(string achievementId, int progress);
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            
            // 初始化子系统
            _checker = new AchievementChecker();
            _progress = new AchievementProgress();
            _rewards = new AchievementRewards();
            
            _checker.SetAchievementSystem(this);
            _progress.SetAchievementSystem(this);
            
            InitializeAchievements();
        }
        
        protected override string SystemName => "AchievementSystem";
        
        #region Initialization
        
        /// <summary>
        /// 初始化成就
        /// </summary>
        private void InitializeAchievements()
        {
            GD.Print("[AchievementSystem] Initialized");
        }
        
        #endregion
        
        #region Stats Tracking
        
        /// <summary>
        /// 增加击杀数
        /// </summary>
        public void AddKill(bool isBoss = false)
        {
            _totalKills++;
            if (isBoss)
            {
                _bossKills++;
                UpdateProgress("boss_slayer", _bossKills);
            }
            
            UpdateProgress("first_blood", _totalKills);
            UpdateProgress("killer_novice", _totalKills);
            UpdateProgress("killer_master", _totalKills);
            UpdateProgress("killer_legend", _totalKills);
        }
        
        /// <summary>
        /// 增加PvP胜利
        /// </summary>
        public void AddPvpWin()
        {
            _pvpWins++;
            UpdateProgress("pvp_novice", _pvpWins);
            UpdateProgress("pvp_champion", _pvpWins);
        }
        
        /// <summary>
        /// 发现区域
        /// </summary>
        public void DiscoverZone(int zoneCount)
        {
            _zonesDiscovered = zoneCount;
            UpdateProgress("explorer_novice", zoneCount);
            UpdateProgress("explorer_master", zoneCount);
            UpdateProgress("explorer_legend", zoneCount);
        }
        
        /// <summary>
        /// 更新爬塔进度
        /// </summary>
        public void UpdateSealedTower(int floor)
        {
            _sealedTowerFloor = floor;
            UpdateProgress("tower_climber", floor);
            UpdateProgress("tower_master", floor);
        }
        
        /// <summary>
        /// 增加宠物收集
        /// </summary>
        public void AddPet()
        {
            _petsCollected++;
            UpdateProgress("pet_collector_novice", _petsCollected);
            UpdateProgress("pet_collector_master", _petsCollected);
            UpdateProgress("pet_collector_legend", _petsCollected);
        }
        
        /// <summary>
        /// 增加坐骑收集
        /// </summary>
        public void AddMount()
        {
            _mountsCollected++;
        }
        
        /// <summary>
        /// 增加装备收集
        /// </summary>
        public void AddEquipment(int count = 1)
        {
            _equipmentCollected += count;
        }
        
        /// <summary>
        /// 增加好友
        /// </summary>
        public void AddFriend()
        {
            _friendsMade++;
            UpdateProgress("social_novice", _friendsMade);
            UpdateProgress("social_person", _friendsMade);
            UpdateProgress("social_butterfly", _friendsMade);
        }
        
        /// <summary>
        /// 更新金币
        /// </summary>
        public void UpdateGold(int currentGold)
        {
            // 可以添加金币相关成就
        }
        
        /// <summary>
        /// 增加金币消费
        /// </summary>
        public void AddGoldSpent(int amount)
        {
            _goldSpent += amount;
            UpdateProgress("shopaholic_novice", _goldSpent);
            UpdateProgress("shopaholic_master", _goldSpent);
            UpdateProgress("shopaholic_legend", _goldSpent);
        }
        
        #endregion
        
        #region Progress Management
        
        /// <summary>
        /// 更新成就进度
        /// </summary>
        private void UpdateProgress(string achievementId, int value)
        {
            _progress.UpdateProgress(value, value);
            
            // 检查是否解锁
            if (_progress.IsUnlocked(achievementId))
            {
                UnlockAchievement(achievementId);
            }
            
            EmitSignal(SignalName.AchievementProgressUpdated, achievementId, value);
        }
        
        /// <summary>
        /// 解锁成就
        /// </summary>
        private void UnlockAchievement(string achievementId)
        {
            // 发放奖励
            _rewards.GrantRewards(achievementId);
            
            EmitSignal(SignalName.AchievementUnlocked, achievementId);
            GD.Print($"[AchievementSystem] Achievement unlocked: {achievementId}");
        }
        
        #endregion
        
        #region Data Access
        
        public int GetTotalKills() => _totalKills;
        public int GetBossKills() => _bossKills;
        public int GetPvpWins() => _pvpWins;
        public int GetZonesDiscovered() => _zonesDiscovered;
        public int GetSealedTowerFloor() => _sealedTowerFloor;
        public int GetPetsCollected() => _petsCollected;
        public int GetMountsCollected() => _mountsCollected;
        public int GetEquipmentCollected() => _equipmentCollected;
        public int GetFriendsMade() => _friendsMade;
        public int GetGoldSpent() => _goldSpent;
        
        #endregion
        
        #region Save/Load
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // 导出统计数据
            data["totalKills"] = _totalKills;
            data["bossKills"] = _bossKills;
            data["pvpWins"] = _pvpWins;
            data["zonesDiscovered"] = _zonesDiscovered;
            data["sealedTowerFloor"] = _sealedTowerFloor;
            data["petsCollected"] = _petsCollected;
            data["mountsCollected"] = _mountsCollected;
            data["equipmentCollected"] = _equipmentCollected;
            data["friendsMade"] = _friendsMade;
            data["goldSpent"] = _goldSpent;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            _totalKills = data.Contains("totalKills") ? (int)data["totalKills"] : 0;
            _bossKills = data.Contains("bossKills") ? (int)data["bossKills"] : 0;
            _pvpWins = data.Contains("pvpWins") ? (int)data["pvpWins"] : 0;
            _zonesDiscovered = data.Contains("zonesDiscovered") ? (int)data["zonesDiscovered"] : 0;
            _sealedTowerFloor = data.Contains("sealedTowerFloor") ? (int)data["sealedTowerFloor"] : 0;
            _petsCollected = data.Contains("petsCollected") ? (int)data["petsCollected"] : 0;
            _mountsCollected = data.Contains("mountsCollected") ? (int)data["mountsCollected"] : 0;
            _equipmentCollected = data.Contains("equipmentCollected") ? (int)data["equipmentCollected"] : 0;
            _friendsMade = data.Contains("friendsMade") ? (int)data["friendsMade"] : 0;
            _goldSpent = data.Contains("goldSpent") ? (int)data["goldSpent"] : 0;
        }
        
        #endregion
    }
}
