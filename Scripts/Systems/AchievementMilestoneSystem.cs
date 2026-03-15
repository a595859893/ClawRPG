using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// 成就里程碑系统 - 核心系统
    /// </summary>
    public partial class AchievementMilestoneSystem : BaseSystem
    {
        public static AchievementMilestoneSystem Instance { get; private set; }
        
        // 数据库
        private AchievementMilestoneDatabase _database;
        
        // 玩家数据
        private PlayerAchievementData _playerData;
        
        // 统计数据
        private AchievementStatistics _statistics;
        
        // 信号
        [Signal] public delegate void AchievementUnlockedEventHandler(string achievementId, Achievement achievement);
        [Signal] public delegate void MilestoneCompletedEventHandler(string milestoneId, Milestone milestone);
        [Signal] public delegate void RewardClaimedEventHandler(string rewardId, int gold, int exp);
        [Signal] public delegate void ProgressUpdatedEventHandler(string id, int current, int target);
        
        public override void _Ready()
        {
            Instance = this;
            _database = new AchievementMilestoneDatabase();
            _playerData = new PlayerAchievementData();
            _statistics = new AchievementStatistics();
            
            InitializePlayerData();
            LoadSaveData();
        }
        
        /// <summary>
        /// 系统名称
        /// </summary>
        protected override string SystemName => "AchievementMilestone";
        
        private void InitializePlayerData()
        {
            // 初始化成就数据
            foreach (var template in _database.Achievements.Values)
            {
                var achievement = new Achievement
                {
                    ID = template.ID,
                    Name = template.Name,
                    Description = template.Description,
                    Type = template.Type,
                    Rarity = template.Rarity,
                    RequiredValue = template.RequiredValue,
                    IconName = template.IconName,
                    Rewards = new List<string>(template.Rewards)
                };
                _playerData.Achievements[template.ID] = achievement;
            }
            
            // 初始化里程碑数据
            foreach (var template in _database.Milestones.Values)
            {
                var milestone = new Milestone
                {
                    ID = template.ID,
                    Name = template.Name,
                    Description = template.Description,
                    RequiredCount = template.RequiredCount,
                    IconName = template.IconName,
                    Rewards = new List<string>(template.Rewards)
                };
                _playerData.Milestones[template.ID] = milestone;
            }
            
            _playerData.TotalAchievements = _playerData.Achievements.Count;
            _playerData.TotalMilestones = _playerData.Milestones.Count;
        }
        
        #region 进度更新
        
        /// <summary>
        /// 更新成就进度
        /// </summary>
        public void UpdateAchievementProgress(string achievementId, int newValue)
        {
            if (!_playerData.Achievements.ContainsKey(achievementId))
                return;
                
            var achievement = _playerData.Achievements[achievementId];
            int oldValue = achievement.CurrentValue;
            achievement.CurrentValue = newValue;
            
            // 检查是否解锁
            if (!achievement.IsUnlocked && newValue >= achievement.RequiredValue)
            {
                achievement.IsUnlocked = true;
                achievement.UnlockedTime = DateTime.Now;
                _playerData.UnlockedAchievements++;
                _playerData.RecentlyUnlocked.Add(achievementId);
                
                // 更新统计
                UpdateStatistics(achievement);
                
                // 发送信号
                EmitSignal(SignalName.AchievementUnlocked, achievementId, achievement);
            }
            
            // 发送进度更新信号
            EmitSignal(SignalName.ProgressUpdated, achievementId, newValue, achievement.RequiredValue);
            
            SaveSaveData();
        }
        
        /// <summary>
        /// 增加成就进度
        /// </summary>
        public void IncrementAchievementProgress(string achievementId, int amount = 1)
        {
            if (!_playerData.Achievements.ContainsKey(achievementId))
                return;
                
            var achievement = _playerData.Achievements[achievementId];
            UpdateAchievementProgress(achievementId, achievement.CurrentValue + amount);
        }
        
        /// <summary>
        /// 更新里程碑进度
        /// </summary>
        public void UpdateMilestoneProgress(string milestoneId, int newCount)
        {
            if (!_playerData.Milestones.ContainsKey(milestoneId))
                return;
                
            var milestone = _playerData.Milestones[milestoneId];
            int oldCount = milestone.CurrentCount;
            milestone.CurrentCount = newCount;
            
            // 检查是否完成
            if (!milestone.IsCompleted && newCount >= milestone.RequiredCount)
            {
                milestone.IsCompleted = true;
                milestone.CompletedTime = DateTime.Now;
                _playerData.CompletedMilestones++;
                
                // 发送信号
                EmitSignal(SignalName.MilestoneCompleted, milestoneId, milestone);
            }
            
            // 发送进度更新信号
            EmitSignal(SignalName.ProgressUpdated, milestoneId, newCount, milestone.RequiredCount);
            
            SaveSaveData();
        }
        
        /// <summary>
        /// 增加里程碑进度
        /// </summary>
        public void IncrementMilestoneProgress(string milestoneId, int amount = 1)
        {
            if (!_playerData.Milestones.ContainsKey(milestoneId))
                return;
                
            var milestone = _playerData.Milestones[milestoneId];
            UpdateMilestoneProgress(milestoneId, milestone.CurrentCount + amount);
        }
        
        /// <summary>
        /// 批量更新进度 - 击杀
        /// </summary>
        public void OnEnemyKilled(int enemyLevel = 1)
        {
            // 更新击杀成就
            UpdateKillAchievements(1);
            
            // 更新击杀里程碑
            UpdateKillMilestones(1);
        }
        
        /// <summary>
        /// 批量更新进度 - Boss击杀
        /// </summary>
        public void OnBossKilled()
        {
            IncrementAchievementProgress("ach_boss_1");
            IncrementAchievementProgress("ach_boss_10");
            IncrementAchievementProgress("ach_boss_50");
            IncrementAchievementProgress("ach_boss_100");
            
            // 更新里程碑
            IncrementMilestoneProgress("ms_boss_10");
            IncrementMilestoneProgress("ms_boss_50");
            IncrementMilestoneProgress("ms_boss_100");
        }
        
        /// <summary>
        /// 批量更新进度 - 地下城完成
        /// </summary>
        public void OnDungeonCompleted()
        {
            IncrementAchievementProgress("ach_dungeon_1");
            IncrementAchievementProgress("ach_dungeon_25");
            IncrementAchievementProgress("ach_dungeon_100");
            
            // 更新里程碑
            IncrementMilestoneProgress("ms_dungeon_10");
            IncrementMilestoneProgress("ms_dungeon_50");
            IncrementMilestoneProgress("ms_dungeon_100");
        }
        
        /// <summary>
        /// 批量更新进度 - 制作
        /// </summary>
        public void OnItemCrafted()
        {
            IncrementAchievementProgress("ach_craft_10");
            IncrementAchievementProgress("ach_craft_50");
            IncrementAchievementProgress("ach_craft_200");
            IncrementAchievementProgress("ach_craft_500");
            IncrementAchievementProgress("ach_craft_1000");
            
            // 更新里程碑
            IncrementMilestoneProgress("ms_craft_100");
            IncrementMilestoneProgress("ms_craft_500");
            IncrementMilestoneProgress("ms_craft_1000");
        }
        
        /// <summary>
        /// 批量更新进度 - 探索
        /// </summary>
        public void OnAreaExplored()
        {
            // 这里需要跟踪已探索区域
            // 简化版本直接增加进度
            IncrementAchievementProgress("ach_explore_5");
            IncrementAchievementProgress("ach_explore_15");
            IncrementAchievementProgress("ach_explore_30");
        }
        
        /// <summary>
        /// 批量更新进度 - 收集
        /// </summary>
        public void OnItemCollected()
        {
            IncrementAchievementProgress("ach_collect_50");
            IncrementAchievementProgress("ach_collect_200");
            IncrementAchievementProgress("ach_collect_500");
            IncrementAchievementProgress("ach_collect_1000");
        }
        
        /// <summary>
        /// 批量更新进度 - 等级提升
        /// </summary>
        public void OnLevelUp(int newLevel)
        {
            UpdateAchievementProgress("ach_level_10", newLevel);
            UpdateAchievementProgress("ach_level_30", newLevel);
            UpdateAchievementProgress("ach_level_50", newLevel);
            UpdateAchievementProgress("ach_level_80", newLevel);
            UpdateAchievementProgress("ach_level_100", newLevel);
            
            // 更新里程碑
            UpdateMilestoneProgress("ms_level_25", newLevel);
            UpdateMilestoneProgress("ms_level_50", newLevel);
            UpdateMilestoneProgress("ms_level_75", newLevel);
            UpdateMilestoneProgress("ms_level_100", newLevel);
        }
        
        /// <summary>
        /// 批量更新进度 - 金币变化
        /// </summary>
        public void OnGoldChanged(int newGoldAmount)
        {
            UpdateAchievementProgress("ach_gold_10000", newGoldAmount);
            UpdateAchievementProgress("ach_gold_100000", newGoldAmount);
            UpdateAchievementProgress("ach_gold_1000000", newGoldAmount);
            UpdateAchievementProgress("ach_gold_10000000", newGoldAmount);
            
            // 更新里程碑
            UpdateMilestoneProgress("ms_gold_50000", newGoldAmount);
            UpdateMilestoneProgress("ms_gold_500000", newGoldAmount);
            UpdateMilestoneProgress("ms_gold_5000000", newGoldAmount);
        }
        
        private void UpdateKillAchievements(int killCount)
        {
            // 需要累计击杀总数，这里简化处理
            // 实际应该从玩家数据获取总击杀数
            foreach (var ach in _playerData.Achievements.Values)
            {
                if (ach.ID.StartsWith("ach_kill_") && !ach.IsUnlocked)
                {
                    ach.CurrentValue += killCount;
                    if (ach.CurrentValue >= ach.RequiredValue)
                    {
                        ach.IsUnlocked = true;
                        ach.UnlockedTime = DateTime.Now;
                        _playerData.UnlockedAchievements++;
                        _playerData.RecentlyUnlocked.Add(ach.ID);
                        UpdateStatistics(ach);
                        EmitSignal(SignalName.AchievementUnlocked, ach.ID, ach);
                    }
                }
            }
        }
        
        private void UpdateKillMilestones(int killCount)
        {
            foreach (var ms in _playerData.Milestones.Values)
            {
                if (ms.ID.StartsWith("ms_kill_") && !ms.IsCompleted)
                {
                    ms.CurrentCount += killCount;
                    if (ms.CurrentCount >= ms.RequiredCount)
                    {
                        ms.IsCompleted = true;
                        ms.CompletedTime = DateTime.Now;
                        _playerData.CompletedMilestones++;
                        EmitSignal(SignalName.MilestoneCompleted, ms.ID, ms);
                    }
                }
            }
        }
        
        #endregion
        
        #region 奖励领取
        
        /// <summary>
        /// 领取成就奖励
        /// </summary>
        public bool ClaimAchievementReward(string achievementId)
        {
            if (!_playerData.Achievements.ContainsKey(achievementId))
                return false;
                
            var achievement = _playerData.Achievements[achievementId];
            if (!achievement.IsUnlocked || achievement.RewardsClaimed)
                return false;
            
            // 发放奖励
            int gold = 0;
            int exp = 0;
            
            foreach (var rewardId in achievement.Rewards)
            {
                var reward = _database.GetRewardConfig(rewardId);
                if (reward != null)
                {
                    if (reward.Type == RewardType.Gold)
                        gold += reward.Amount;
                    else if (reward.Type == RewardType.Experience)
                        exp += reward.Amount;
                }
            }
            
            // 标记奖励已领取
            achievement.RewardsClaimed = true;
            _playerData.ClaimedRewards++;
            _statistics.TotalRewardsClaimed++;
            _statistics.GoldEarnedFromRewards += gold;
            _statistics.ExpEarnedFromRewards += exp;
            
            // 发送信号
            EmitSignal(SignalName.RewardClaimed, achievementId, gold, exp);
            
            SaveSaveData();
            return true;
        }
        
        /// <summary>
        /// 领取里程碑奖励
        /// </summary>
        public bool ClaimMilestoneReward(string milestoneId)
        {
            if (!_playerData.Milestones.ContainsKey(milestoneId))
                return false;
                
            var milestone = _playerData.Milestones[milestoneId];
            if (!milestone.IsCompleted || milestone.RewardsClaimed)
                return false;
            
            // 发放奖励
            int gold = 0;
            int exp = 0;
            
            foreach (var rewardId in milestone.Rewards)
            {
                var reward = _database.GetRewardConfig(rewardId);
                if (reward != null)
                {
                    if (reward.Type == RewardType.Gold)
                        gold += reward.Amount;
                    else if (reward.Type == RewardType.Experience)
                        exp += reward.Amount;
                }
            }
            
            // 标记奖励已领取
            milestone.RewardsClaimed = true;
            _playerData.ClaimedRewards++;
            _statistics.TotalRewardsClaimed++;
            _statistics.GoldEarnedFromRewards += gold;
            _statistics.ExpEarnedFromRewards += exp;
            
            // 发送信号
            EmitSignal(SignalName.RewardClaimed, milestoneId, gold, exp);
            
            SaveSaveData();
            return true;
        }
        
        #endregion
        
        #region 数据查询
        
        /// <summary>
        /// 获取成就
        /// </summary>
        public Achievement GetAchievement(string achievementId)
        {
            return _playerData.Achievements.ContainsKey(achievementId) ? _playerData.Achievements[achievementId] : null;
        }
        
        /// <summary>
        /// 获取里程碑
        /// </summary>
        public Milestone GetMilestone(string milestoneId)
        {
            return _playerData.Milestones.ContainsKey(milestoneId) ? _playerData.Milestones[milestoneId] : null;
        }
        
        /// <summary>
        /// 获取所有成就
        /// </summary>
        public Dictionary<string, Achievement> GetAllAchievements()
        {
            return new Dictionary<string, Achievement>(_playerData.Achievements);
        }
        
        /// <summary>
        /// 获取所有里程碑
        /// </summary>
        public Dictionary<string, Milestone> GetAllMilestones()
        {
            return new Dictionary<string, Milestone>(_playerData.Milestones);
        }
        
        /// <summary>
        /// 获取按类型分类的成就
        /// </summary>
        public List<Achievement> GetAchievementsByType(AchievementType type)
        {
            var result = new List<Achievement>();
            foreach (var ach in _playerData.Achievements.Values)
            {
                if (ach.Type == type)
                    result.Add(ach);
            }
            return result;
        }
        
        /// <summary>
        /// 获取最近解锁的成就
        /// </summary>
        public List<string> GetRecentlyUnlockedAchievements(int count = 10)
        {
            var result = new List<string>();
            int take = Math.Min(count, _playerData.RecentlyUnlocked.Count);
            for (int i = _playerData.RecentlyUnlocked.Count - take; i < _playerData.RecentlyUnlocked.Count; i++)
            {
                result.Add(_playerData.RecentlyUnlocked[i]);
            }
            return result;
        }
        
        /// <summary>
        /// 获取未领取奖励的成就
        /// </summary>
        public List<Achievement> GetUnclaimedAchievements()
        {
            var result = new List<Achievement>();
            foreach (var ach in _playerData.Achievements.Values)
            {
                if (ach.IsUnlocked && !ach.RewardsClaimed && ach.Rewards.Count > 0)
                    result.Add(ach);
            }
            return result;
        }
        
        /// <summary>
        /// 获取未领取奖励的里程碑
        /// </summary>
        public List<Milestone> GetUnclaimedMilestones()
        {
            var result = new List<Milestone>();
            foreach (var ms in _playerData.Milestones.Values)
            {
                if (ms.IsCompleted && !ms.RewardsClaimed && ms.Rewards.Count > 0)
                    result.Add(ms);
            }
            return result;
        }
        
        /// <summary>
        /// 获取统计数据
        /// </summary>
        public AchievementStatistics GetStatistics()
        {
            return _statistics;
        }
        
        /// <summary>
        /// 获取玩家数据
        /// </summary>
        public PlayerAchievementData GetPlayerData()
        {
            return _playerData;
        }
        
        /// <summary>
        /// 获取总体进度百分比
        /// </summary>
        public float GetOverallProgress()
        {
            int total = _playerData.TotalAchievements + _playerData.TotalMilestones;
            int completed = _playerData.UnlockedAchievements + _playerData.CompletedMilestones;
            return total > 0 ? (float)completed / total : 0f;
        }
        
        #endregion
        
        #region 统计
        
        private void UpdateStatistics(Achievement achievement)
        {
            _statistics.TotalAchievementsUnlocked++;
            
            // 稀有度统计
            if (_statistics.RarityBreakdown.ContainsKey(achievement.Rarity))
                _statistics.RarityBreakdown[achievement.Rarity]++;
            else
                _statistics.RarityBreakdown[achievement.Rarity] = 1;
            
            // 类型统计
            if (_statistics.TypeBreakdown.ContainsKey(achievement.Type))
                _statistics.TypeBreakdown[achievement.Type]++;
            else
                _statistics.TypeBreakdown[achievement.Type] = 1;
        }
        
        #endregion
        
        #region 存档
        
        public Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            // 成就数据
            var achievements = new Array();
            foreach (var ach in _playerData.Achievements.Values)
            {
                var achData = new Dictionary
                {
                    { "id", ach.ID },
                    { "current_value", ach.CurrentValue },
                    { "is_unlocked", ach.IsUnlocked },
                    { "unlocked_time", ach.UnlockedTime?.ToString("o") ?? "" },
                    { "rewards_claimed", ach.RewardsClaimed }
                };
                achievements.Add(achData);
            }
            data["achievements"] = achievements;
            
            // 里程碑数据
            var milestones = new Array();
            foreach (var ms in _playerData.Milestones.Values)
            {
                var msData = new Dictionary
                {
                    { "id", ms.ID },
                    { "current_count", ms.CurrentCount },
                    { "is_completed", ms.IsCompleted },
                    { "completed_time", ms.CompletedTime?.ToString("o") ?? "" },
                    { "rewards_claimed", ms.RewardsClaimed }
                };
                milestones.Add(msData);
            }
            data["milestones"] = milestones;
            
            // 统计数据
            var stats = new Dictionary
            {
                { "total_play_time", _statistics.TotalPlayTime },
                { "total_rewards_claimed", _statistics.TotalRewardsClaimed },
                { "gold_earned", _statistics.GoldEarnedFromRewards },
                { "exp_earned", _statistics.ExpEarnedFromRewards }
            };
            data["statistics"] = stats;
            
            return data;
        }
        
        public void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            // 成就数据
            if (data.Contains("achievements"))
            {
                var achievements = (Array)data["achievements"];
                foreach (Dictionary achData in achievements)
                {
                    string id = (string)achData["id"];
                    if (_playerData.Achievements.ContainsKey(id))
                    {
                        var ach = _playerData.Achievements[id];
                        ach.CurrentValue = (int)achData["current_value"];
                        ach.IsUnlocked = (bool)achData["is_unlocked"];
                        string timeStr = (string)achData["unlocked_time"];
                        if (!string.IsNullOrEmpty(timeStr))
                            ach.UnlockedTime = DateTime.Parse(timeStr);
                        ach.RewardsClaimed = (bool)achData["rewards_claimed"];
                        
                        if (ach.IsUnlocked)
                            _playerData.UnlockedAchievements++;
                    }
                }
            }
            
            // 里程碑数据
            if (data.Contains("milestones"))
            {
                var milestones = (Array)data["milestones"];
                foreach (Dictionary msData in milestones)
                {
                    string id = (string)msData["id"];
                    if (_playerData.Milestones.ContainsKey(id))
                    {
                        var ms = _playerData.Milestones[id];
                        ms.CurrentCount = (int)msData["current_count"];
                        ms.IsCompleted = (bool)msData["is_completed"];
                        string timeStr = (string)msData["completed_time"];
                        if (!string.IsNullOrEmpty(timeStr))
                            ms.CompletedTime = DateTime.Parse(timeStr);
                        ms.RewardsClaimed = (bool)msData["rewards_claimed"];
                        
                        if (ms.IsCompleted)
                            _playerData.CompletedMilestones++;
                    }
                }
            }
            
            // 统计数据
            if (data.Contains("statistics"))
            {
                var stats = (Dictionary)data["statistics"];
                _statistics.TotalPlayTime = (int)stats["total_play_time"];
                _statistics.TotalRewardsClaimed = (int)stats["total_rewards_claimed"];
                _statistics.GoldEarnedFromRewards = (int)stats["gold_earned"];
                _statistics.ExpEarnedFromRewards = (int)stats["exp_earned"];
            }
        }
        
        private void LoadSaveData()
        {
            // 这里应该从游戏存档加载数据
            // 简化版本使用默认数据
        }
        
        private void SaveSaveData()
        {
            // 这里应该保存到游戏存档
            // 简化版本不实际保存
        }
        
        #endregion
    }
}
