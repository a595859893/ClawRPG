using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Achievement manager - tracks progress and handles unlocking
    /// </summary>
    public class AchievementManager
    {
        private static AchievementManager _instance;
        public static AchievementManager Instance => _instance ??= new AchievementManager();
        
        private List<Achievement> _unlockedAchievements;
        private Dictionary<string, Achievement> _trackedAchievements;
        
        // Tutorial tracking
        private bool _hasTriggeredFirstAchievement = false; 
        
        // Statistics tracking
        private int _totalKills;
        private int _totalBossesKilled;
        private int _totalGoldEarned;
        private int _totalCrafts;
        private int _totalQuestsCompleted;
        private int _maxCombo;
        private int _maxDamage;
        private int _regionsExplored;
        private int _skillsLearned;
        private float _totalSurvivalTime;
        private int _enrageKills;
        private int _perfectBlocks;
        private int _counterAttacks;
        private int _noHitBosses;
        private int _totalBossDamageTaken;
        
        // Signals
        public Action<Achievement> OnAchievementUnlocked;
        public Action<Achievement> OnAchievementProgressUpdated;
        
        private AchievementManager()
        {
            _unlockedAchievements = new List<Achievement>();
            _trackedAchievements = new Dictionary<string, Achievement>();
            LoadAchievements();
        }
        
        private void LoadAchievements()
        {
            var allAchievements = AchievementDatabase.Instance.GetAllAchievements();
            foreach (var achievement in allAchievements)
            {
                _trackedAchievements[achievement.Id] = achievement;
            }
        }
        
        /// <summary>
        /// 追踪击杀数
        /// </summary>
        /// <param name="count">击杀数量</param>
        public void TrackKill(int count = 1)
        {
            _totalKills += count;
            UpdateAchievement("kill_10", _totalKills);
            UpdateAchievement("kill_100", _totalKills);
            UpdateAchievement("kill_500", _totalKills);
            UpdateAchievement("kill_1000", _totalKills);
        }
        
        /// <summary>
        /// 追踪Boss击杀
        /// </summary>
        public void TrackBossKill()
        {
            _totalBossesKilled++;
            UpdateAchievement("boss_1", _totalBossesKilled);
            UpdateAchievement("boss_5", _totalBossesKilled);
            UpdateAchievement("boss_all", _totalBossesKilled);
        }
        
        /// <summary>
        /// 追踪获得金币
        /// </summary>
        /// <param name="amount">金币数量</param>
        public void TrackGoldEarned(int amount)
        {
            _totalGoldEarned += amount;
            UpdateAchievement("gold_1000", _totalGoldEarned);
            UpdateAchievement("gold_10000", _totalGoldEarned);
            UpdateAchievement("gold_100000", _totalGoldEarned);
        }
        
        /// <summary>
        /// 追踪制造
        /// </summary>
        public void TrackCraft()
        {
            _totalCrafts++;
            UpdateAchievement("craft_1", _totalCrafts);
            UpdateAchievement("craft_10", _totalCrafts);
            UpdateAchievement("craft_50", _totalCrafts);
        }
        
        /// <summary>
        /// 追踪任务完成
        /// </summary>
        public void TrackQuestComplete()
        {
            _totalQuestsCompleted++;
            UpdateAchievement("quest_1", _totalQuestsCompleted);
            UpdateAchievement("quest_10", _totalQuestsCompleted);
            UpdateAchievement("quest_all", _totalQuestsCompleted);
        }
        
        /// <summary>
        /// 追踪连击数
        /// </summary>
        /// <param name="combo">连击数</param>
        public void TrackCombo(int combo)
        {
            if (combo > _maxCombo)
            {
                _maxCombo = combo;
                UpdateAchievement("combo_10", _maxCombo);
                UpdateAchievement("combo_50", _maxCombo);
                UpdateAchievement("combo_100", _maxCombo);
            }
        }
        
        /// <summary>
        /// 追踪伤害
        /// </summary>
        /// <param name="damage">伤害值</param>
        public void TrackDamage(int damage)
        {
            if (damage > _maxDamage)
            {
                _maxDamage = damage;
                UpdateAchievement("damage_1000", _maxDamage);
                UpdateAchievement("damage_10000", _maxDamage);
                UpdateAchievement("damage_100000", _maxDamage);
            }
        }
        
        /// <summary>
        /// 追踪生存时间
        /// </summary>
        /// <param name="seconds">生存秒数</param>
        public void TrackSurvivalTime(float seconds)
        {
            _totalSurvivalTime += seconds;
            UpdateAchievement("survive_1min", (int)_totalSurvivalTime);
            UpdateAchievement("survive_10min", (int)_totalSurvivalTime);
            UpdateAchievement("survive_1hour", (int)_totalSurvivalTime);
        }
        
        /// <summary>
        /// 追踪等级
        /// </summary>
        /// <param name="level">等级</param>
        public void TrackLevel(int level)
        {
            UpdateAchievement("level_5", level);
            UpdateAchievement("level_10", level);
            UpdateAchievement("level_20", level);
            UpdateAchievement("level_50", level);
        }
        
        /// <summary>
        /// 追踪区域探索
        /// </summary>
        /// <param name="regionCount">已探索区域数</param>
        public void TrackRegionExplore(int regionCount)
        {
            _regionsExplored = regionCount;
            UpdateAchievement("explore_3", _regionsExplored);
            UpdateAchievement("explore_all", _regionsExplored);
        }
        
        /// <summary>
        /// 追踪技能学习
        /// </summary>
        /// <param name="skillCount">已学习技能数</param>
        public void TrackSkillLearn(int skillCount)
        {
            _skillsLearned = skillCount;
            UpdateAchievement("skill_learn_5", _skillsLearned);
            UpdateAchievement("skill_learn_15", _skillsLearned);
        }
        
        /// <summary>
        /// 追踪愤怒击杀
        /// </summary>
        public void TrackEnrageKill()
        {
            _enrageKills++;
            UpdateAchievement("enrage_kill_1", _enrageKills);
            UpdateAchievement("enrage_kill_5", _enrageKills);
            UpdateAchievement("enrage_kill_10", _enrageKills);
        }
        
        /// <summary>
        /// 追踪完美格挡
        /// </summary>
        /// <param name="count">完美格挡次数</param>
        public void TrackPerfectBlock(int count = 1)
        {
            _perfectBlocks += count;
            UpdateAchievement("perfect_block_10", _perfectBlocks);
            UpdateAchievement("perfect_block_50", _perfectBlocks);
            UpdateAchievement("perfect_block_100", _perfectBlocks);
        }
        
        /// <summary>
        /// 追踪反击
        /// </summary>
        /// <param name="count">反击次数</param>
        public void TrackCounterAttack(int count = 1)
        {
            _counterAttacks += count;
            UpdateAchievement("counter_5", _counterAttacks);
            UpdateAchievement("counter_25", _counterAttacks);
            UpdateAchievement("counter_50", _counterAttacks);
        }
        
        /// <summary>
        /// 追踪无伤Boss战
        /// </summary>
        /// <param name="success">是否成功</param>
        public void TrackNoHitBoss(bool success)
        {
            if (success)
            {
                _noHitBosses++;
                UpdateAchievement("nohit_boss_1", _noHitBosses);
                UpdateAchievement("nohit_boss_3", _noHitBosses);
            }
        }
        
        public void TrackBossDamageTaken(int damage)
        {
            _totalBossDamageTaken += damage;
        }
        
        public int GetBossDamageTaken() => _totalBossDamageTaken;
        
        public void ResetBossDamageTaken()
        {
            _totalBossDamageTaken = 0;
        }
        
        private void UpdateAchievement(string id, int value)
        {
            if (_trackedAchievements.TryGetValue(id, out var achievement))
            {
                int oldValue = achievement.CurrentValue;
                achievement.CurrentValue = value;
                
                if (achievement.CanUnlock)
                {
                    achievement.Unlock();
                    if (!_unlockedAchievements.Contains(achievement))
                    {
                        _unlockedAchievements.Add(achievement);
                        OnAchievementUnlocked?.Invoke(achievement);
                        
                        // Trigger tutorial for first achievement
                        if (!_hasTriggeredFirstAchievement)
                        {
                            _hasTriggeredFirstAchievement = true;
                            TutorialSystem.Trigger(TutorialTrigger.FirstAchievement);
                        }
                        
                        // Grant rewards
                        GrantRewards(achievement);
                    }
                }
                else if (achievement.CurrentValue != oldValue)
                {
                    OnAchievementProgressUpdated?.Invoke(achievement);
                }
            }
        }
        
        private void GrantRewards(Achievement achievement)
        {
            var player = GetPlayer();
            if (player != null)
            {
                if (achievement.RewardGold > 0)
                {
                    player.AddGold(achievement.RewardGold);
                }
                if (achievement.RewardExp > 0)
                {
                    player.AddExperience(achievement.RewardExp);
                }
            }
        }
        
        private Node GetPlayer()
        {
            return GetTree()?.CurrentScene?.GetNodeOrNull<Node>("%Player");
        }
        
        private Tree GetTree()
        {
            return Engine.GetMainLoop() as Tree;
        }
        
        /// <summary>
        /// 获取已解锁成就列表
        /// </summary>
        /// <returns>已解锁成就列表</returns>
        public List<Achievement> GetUnlockedAchievements()
        {
            return new List<Achievement>(_unlockedAchievements);
        }
        
        /// <summary>
        /// 获取所有追踪的成就
        /// </summary>
        /// <returns>成就列表</returns>
        public List<Achievement> GetAllTrackedAchievements()
        {
            return AchievementDatabase.Instance.GetAllAchievements();
        }
        
        /// <summary>
        /// 获取指定成就
        /// </summary>
        /// <param name="id">成就ID</param>
        /// <returns>成就对象</returns>
        public Achievement GetAchievement(string id)
        {
            return _trackedAchievements.TryGetValue(id, out var achievement) ? achievement : null;
        }
        
        /// <summary>
        /// 检查成就是否已解锁
        /// </summary>
        /// <param name="id">成就ID</param>
        /// <returns>是否已解锁</returns>
        public bool IsAchievementUnlocked(string id)
        {
            return _trackedAchievements.TryGetValue(id, out var achievement) && achievement.IsUnlocked;
        }
        
        /// <summary>
        /// 获取总体进度百分比
        /// </summary>
        /// <returns>进度百分比</returns>
        public float GetOverallProgress()
        {
            var all = GetAllTrackedAchievements();
            if (all.Count == 0) return 0f;
            
            int unlocked = 0;
            foreach (var a in all)
            {
                if (a.IsUnlocked) unlocked++;
            }
            
            return (float)unlocked / all.Count;
        }
        
        /// <summary>
        /// 获取统计数据
        /// </summary>
        /// <returns>统计数据字典</returns>
        public Dictionary<string, int> GetStatistics()
        {
            return new Dictionary<string, int>
            {
                { "totalKills", _totalKills },
                { "totalBossesKilled", _totalBossesKilled },
                { "totalGoldEarned", _totalGoldEarned },
                { "totalCrafts", _totalCrafts },
                { "totalQuestsCompleted", _totalQuestsCompleted },
                { "maxCombo", _maxCombo },
                { "maxDamage", _maxDamage },
                { "regionsExplored", _regionsExplored },
                { "skillsLearned", _skillsLearned },
                { "totalSurvivalTime", (int)_totalSurvivalTime },
                { "enrageKills", _enrageKills },
                { "perfectBlocks", _perfectBlocks },
                { "counterAttacks", _counterAttacks },
                { "noHitBosses", _noHitBosses },
                { "unlockedAchievements", _unlockedAchievements.Count },
                { "totalAchievements", _trackedAchievements.Count }
            };
        }
        
        public void ResetProgress()
        {
            _totalKills = 0;
            _totalBossesKilled = 0;
            _totalGoldEarned = 0;
            _totalCrafts = 0;
            _totalQuestsCompleted = 0;
            _maxCombo = 0;
            _maxDamage = 0;
            _regionsExplored = 0;
            _skillsLearned = 0;
            _totalSurvivalTime = 0;
            _unlockedAchievements.Clear();
            
            LoadAchievements();
        }
        
        public Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                { "totalKills", _totalKills },
                { "totalBossesKilled", _totalBossesKilled },
                { "totalGoldEarned", _totalGoldEarned },
                { "totalCrafts", _totalCrafts },
                { "totalQuestsCompleted", _totalQuestsCompleted },
                { "maxCombo", _maxCombo },
                { "maxDamage", _maxDamage },
                { "regionsExplored", _regionsExplored },
                { "skillsLearned", _skillsLearned },
                { "totalSurvivalTime", _totalSurvivalTime },
                { "unlockedIds", _unlockedAchievements.ConvertAll(a => a.Id) }
            };
        }
        
        public void Deserialize(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            _totalKills = data.GetValueOrDefault("totalKills", 0);
            _totalBossesKilled = data.GetValueOrDefault("totalBossesKilled", 0);
            _totalGoldEarned = data.GetValueOrDefault("totalGoldEarned", 0);
            _totalCrafts = data.GetValueOrDefault("totalCrafts", 0);
            _totalQuestsCompleted = data.GetValueOrDefault("totalQuestsCompleted", 0);
            _maxCombo = data.GetValueOrDefault("maxCombo", 0);
            _maxDamage = data.GetValueOrDefault("maxDamage", 0);
            _regionsExplored = data.GetValueOrDefault("regionsExplored", 0);
            _skillsLearned = data.GetValueOrDefault("skillsLearned", 0);
            _totalSurvivalTime = data.GetValueOrDefault("totalSurvivalTime", 0f);
            
            var unlockedIds = data.GetValueOrDefault("unlockedIds", new List<string>());
            if (unlockedIds is List<string> ids)
            {
                foreach (var id in ids)
                {
                    if (_trackedAchievements.TryGetValue(id, out var achievement))
                    {
                        achievement.IsUnlocked = true;
                        achievement.CurrentValue = achievement.RequiredValue;
                        if (!_unlockedAchievements.Contains(achievement))
                        {
                            _unlockedAchievements.Add(achievement);
                        }
                    }
                }
            }
        }
    }
}
