using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Secret Achievement System - hidden achievements discovered through gameplay
    /// </summary>
    public partial class SecretAchievementSystem : Node
    {
        private static SecretAchievementSystem _instance;
        public static SecretAchievementSystem Instance => _instance;

        // Player progress data
        private Dictionary<string, PlayerSecretAchievementData> _playerAchievements = new();
        
        // Statistics
        private int _totalDiscovered = 0;
        private int _totalGoldEarned = 0;
        private int _totalExpEarned = 0;
        
        // Signals
        [Signal]
    public delegate void OnAchievementProgress(string achievementId, int progress, int condition);
        [Signal]
    public delegate void OnAchievementDiscovered(string achievementId, int goldReward, int expReward);
        [Signal]
    public delegate void OnSecretCategoryComplete(SecretAchievementCategory category, int discovered, int total);

        public int TotalDiscovered => _totalDiscovered;
        public int TotalGoldEarned => _totalGoldEarned;
        public int TotalExpEarned => _totalExpEarned;

        public override void _Ready()
        {
            _instance = this;
            AddToGroup("SecretAchievementSystem");
            InitializePlayerData();
        }

        private void InitializePlayerData()
        {
            var allAchievements = SecretAchievementDatabase.GetAllAchievements();
            foreach (var achievement in allAchievements)
            {
                if (!_playerAchievements.ContainsKey(achievement.AchievementId))
                {
                    _playerAchievements[achievement.AchievementId] = new PlayerSecretAchievementData
                    {
                        AchievementId = achievement.AchievementId,
                        IsDiscovered = false,
                        Progress = 0,
                        DiscoveredAt = DateTime.MinValue
                    };
                }
            }
        }

        /// <summary>
        /// 报告成就进度
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        /// <param name="amount">进度增量</param>
        public void ReportProgress(string achievementId, int amount = 1)
        {
            if (!_playerAchievements.ContainsKey(achievementId))
                return;

            var playerData = _playerAchievements[achievementId];
            var achievement = SecretAchievementDatabase.GetAchievement(achievementId);
            
            if (playerData.IsDiscovered)
                return;

            playerData.Progress += amount;
            OnAchievementProgress?.Invoke(achievementId, playerData.Progress, achievement.DiscoveryCondition);

            // Check for discovery
            if (playerData.Progress >= achievement.DiscoveryCondition)
            {
                DiscoverAchievement(achievementId);
            }
        }

        /// <summary>
        /// 强制发现成就（用于特殊事件）
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        public void ForceDiscover(string achievementId)
        {
            if (_playerAchievements.ContainsKey(achievementId) && !_playerAchievements[achievementId].IsDiscovered)
            {
                DiscoverAchievement(achievementId);
            }
        }

        private void DiscoverAchievement(string achievementId)
        {
            var playerData = _playerAchievements[achievementId];
            var achievement = SecretAchievementDatabase.GetAchievement(achievementId);
            
            playerData.IsDiscovered = true;
            playerData.DiscoveredAt = DateTime.Now;
            playerData.Progress = achievement.DiscoveryCondition;
            
            _totalDiscovered++;
            _totalGoldEarned += achievement.GoldReward;
            _totalExpEarned += achievement.ExpReward;

            // Grant rewards
            if (achievement.GoldReward > 0 || achievement.ExpReward > 0)
            {
                GrantRewards(achievement.GoldReward, achievement.ExpReward);
            }

            OnAchievementDiscovered?.Invoke(achievementId, achievement.GoldReward, achievement.ExpReward);
            
            // Check category completion
            CheckCategoryCompletion(achievement.Category);
        }

        private void GrantRewards(int gold, int exp)
        {
            var player = GetTree().GetFirstNodeInGroup("Player") as Node;
            if (player != null)
            {
                // Grant gold
                var inventoryManager = player.GetNode("InventoryManager");
                if (inventoryManager != null)
                {
                    var addGoldMethod = inventoryManager.GetType().GetMethod("AddGold");
                    addGoldMethod?.Invoke(inventoryManager, new object[] { gold });
                }
                
                // Grant exp - would need player level system
                GD.Print($"[SecretAchievement] Granted rewards: {gold} gold, {exp} exp");
            }
        }

        private void CheckCategoryCompletion(SecretAchievementCategory category)
        {
            var categoryAchievements = SecretAchievementDatabase.GetAchievementsByCategory(category);
            int discovered = 0;
            
            foreach (var achievement in categoryAchievements)
            {
                if (_playerAchievements.ContainsKey(achievement.AchievementId) && 
                    _playerAchievements[achievement.AchievementId].IsDiscovered)
                {
                    discovered++;
                }
            }
            
            if (discovered == categoryAchievements.Count)
            {
                OnSecretCategoryComplete?.Invoke(category, discovered, categoryAchievements.Count);
            }
        }

        /// <summary>
        /// 获取发现进度 (0.0 - 1.0)
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        /// <returns>发现进度百分比</returns>
        public float GetDiscoveryProgress(string achievementId)
        {
            if (!_playerAchievements.ContainsKey(achievementId))
                return 0f;
                
            var playerData = _playerAchievements[achievementId];
            var achievement = SecretAchievementDatabase.GetAchievement(achievementId);
            
            if (achievement == null)
                return 0f;
                
            return Mathf.Clamp((float)playerData.Progress / achievement.DiscoveryCondition, 0f, 1f);
        }

        /// <summary>
        /// 检查成就是否已发现
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        /// <returns>是否已发现</returns>
        public bool IsDiscovered(string achievementId)
        {
            return _playerAchievements.ContainsKey(achievementId) && 
                   _playerAchievements[achievementId].IsDiscovered;
        }

        /// <summary>
        /// 获取成就进度
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        /// <returns>当前进度</returns>
        public int GetProgress(string achievementId)
        {
            return _playerAchievements.ContainsKey(achievementId) ? 
                   _playerAchievements[achievementId].Progress : 0;
        }

        /// <summary>
        /// 获取所有玩家成就数据
        /// </summary>
        /// <returns>玩家成就数据字典</returns>
        public Dictionary<string, PlayerSecretAchievementData> GetAllPlayerData()
        {
            return new Dictionary<string, PlayerSecretAchievementData>(_playerAchievements);
        }

        /// <summary>
        /// 获取已发现成就列表
        /// </summary>
        /// <returns>已发现成就ID列表</returns>
        public List<string> GetDiscoveredAchievements()
        {
            List<string> discovered = new();
            foreach (var kvp in _playerAchievements)
            {
                if (kvp.Value.IsDiscovered)
                    discovered.Add(kvp.Key);
            }
            return discovered;
        }

        /// <summary>
        /// 获取未发现成就及进度
        /// </summary>
        /// <returns>未发现成就进度字典</returns>
        public Dictionary<string, float> GetUndiscoveredWithProgress()
        {
            Dictionary<string, float> result = new();
            foreach (var kvp in _playerAchievements)
            {
                if (!kvp.Value.IsDiscovered)
                {
                    result[kvp.Key] = GetDiscoveryProgress(kvp.Key);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取发现百分比
        /// </summary>
        /// <returns>发现百分比</returns>
        public float GetTotalDiscoveryPercentage()
        {
            int total = SecretAchievementDatabase.GetTotalCount();
            if (total == 0) return 0f;
            return (float)_totalDiscovered / total;
        }

        // Convenience methods for common achievement triggers
        public void OnBossKilled() => ReportProgress("kill_100_bosses");
        public void OnDungeonCompleteNoDamage() => ReportProgress("no_damage_run");
        public void OnComboReached(int combo) 
        { 
            if (combo >= 100) ReportProgress("combo_100", combo); 
        }
        public void OnTreasureFound(string treasureType) => ReportProgress("find_all_treasures");
        public void OnRegionVisited(string region) => ReportProgress("visit_all_regions");
        public void OnPetObtained() => ReportProgress("collect_all_pets");
        public void OnMountMaxed() => ReportProgress("max_all_mounts");
        public void OnCriticalHit() => ReportProgress("critical_rain");
        public void OnLegendaryItemDrop() => ReportProgress("legendary_drop");
        public void OnEliteDungeonSoloComplete() => ReportProgress("solo_elite_dungeon");
        public void OnSpeedRunComplete(float time) 
        { 
            if (time < 180) ReportProgress("speed_run"); 
        }
        public void OnGuildCreated() => ReportProgress("guild_founder");
        public void OnTradeComplete() => ReportProgress("trade_master");
        public void OnPlayerLevelUp(int level) => ReportProgress("the_chosen_one", level);
        public void OnGoldChanged(int totalGold) => ReportProgress("millionaire", totalGold);

        // Save/Load support
        public Dictionary<string, Dictionary<string, object>> GetSaveData()
        {
            Dictionary<string, Dictionary<string, object>> saveData = new();
            
            foreach (var kvp in _playerAchievements)
            {
                saveData[kvp.Key] = new Dictionary<string, object>
                {
                    { "isDiscovered", kvp.Value.IsDiscovered },
                    { "progress", kvp.Value.Progress },
                    { "discoveredAt", kvp.Value.DiscoveredAt.ToString("o") }
                };
            }
            
            return saveData;
        }

        public void LoadSaveData(Dictionary<string, Dictionary<string, object>> saveData)
        {
            if (saveData == null) return;
            
            foreach (var kvp in saveData)
            {
                if (_playerAchievements.ContainsKey(kvp.Key))
                {
                    _playerAchievements[kvp.Value.IsDiscovered] = kvp.Value.IsDiscovered;
                    _playerAchievements[kvp.Key].Progress = kvp.Value.Progress;
                    
                    if (kvp.Value.ContainsKey("discoveredAt") && kvp.Value["discoveredAt"] is string dateStr)
                    {
                        DateTime.TryParse(dateStr, out DateTime discoveredAt);
                        _playerAchievements[kvp.Key].DiscoveredAt = discoveredAt;
                    }
                }
            }
            
            // Recalculate stats
            _totalDiscovered = 0;
            _totalGoldEarned = 0;
            _totalExpEarned = 0;
            
            foreach (var kvp in _playerAchievements)
            {
                if (kvp.Value.IsDiscovered)
                {
                    _totalDiscovered++;
                    var achievement = SecretAchievementDatabase.GetAchievement(kvp.Key);
                    if (achievement != null)
                    {
                        _totalGoldEarned += achievement.GoldReward;
                        _totalExpEarned += achievement.ExpReward;
                    }
                }
            }
        }
    }
}
