using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// Achievement database - stores all achievement templates
    /// </summary>
    public class AchievementDatabase
    {
        private static AchievementDatabase _instance;
        public static AchievementDatabase Instance => _instance ??= new AchievementDatabase();
        
        private Dictionary<string, Achievement> _achievements;
        
        private AchievementDatabase()
        {
            _achievements = new Dictionary<string, Achievement>();
            InitializeAchievements();
        }
        
        private void InitializeAchievements()
        {
            // Kill achievements
            AddAchievement(new Achievement
            {
                Id = "kill_10",
                Name = "初出茅庐",
                Description = "击杀10个敌人",
                Type = AchievementType.Kill,
                Difficulty = AchievementDifficulty.Easy,
                RequiredValue = 10,
                RewardGold = 50,
                RewardExp = 100
            });
            
            AddAchievement(new Achievement
            {
                Id = "kill_100",
                Name = "战士",
                Description = "击杀100个敌人",
                Type = AchievementType.Kill,
                Difficulty = AchievementDifficulty.Normal,
                RequiredValue = 100,
                RewardGold = 200,
                RewardExp = 500
            });
            
            AddAchievement(new Achievement
            {
                Id = "kill_500",
                Name = "战斗大师",
                Description = "击杀500个敌人",
                Type = AchievementType.Kill,
                Difficulty = AchievementDifficulty.Hard,
                RequiredValue = 500,
                RewardGold = 1000,
                RewardExp = 2000
            });
            
            AddAchievement(new Achievement
            {
                Id = "kill_1000",
                Name = "传奇杀手",
                Description = "击杀1000个敌人",
                Type = AchievementType.Kill,
                Difficulty = AchievementDifficulty.Epic,
                RequiredValue = 1000,
                RewardGold = 5000,
                RewardExp = 10000
            });
            
            // Level achievements
            AddAchievement(new Achievement
            {
                Id = "level_5",
                Name = "初学者",
                Description = "达到5级",
                Type = AchievementType.LevelUp,
                Difficulty = AchievementDifficulty.Easy,
                RequiredValue = 5,
                RewardGold = 100,
                RewardExp = 0
            });
            
            AddAchievement(new Achievement
            {
                Id = "level_10",
                Name = "进阶冒险者",
                Description = "达到10级",
                Type = AchievementType.LevelUp,
                Difficulty = AchievementDifficulty.Normal,
                RequiredValue = 10,
                RewardGold = 300,
                RewardExp = 0
            });
            
            AddAchievement(new Achievement
            {
                Id = "level_20",
                Name = "资深探险家",
                Description = "达到20级",
                Type = AchievementType.LevelUp,
                Difficulty = AchievementDifficulty.Hard,
                RequiredValue = 20,
                RewardGold = 1000,
                RewardExp = 0
            });
            
            AddAchievement(new Achievement
            {
                Id = "level_50",
                Name = "传奇英雄",
                Description = "达到50级",
                Type = AchievementType.LevelUp,
                Difficulty = AchievementDifficulty.Legendary,
                RequiredValue = 50,
                RewardGold = 10000,
                RewardExp = 0
            });
            
            // Gold achievements
            AddAchievement(new Achievement
            {
                Id = "gold_1000",
                Name = "小有积蓄",
                Description = "累计获得1000金币",
                Type = AchievementType.Gold,
                Difficulty = AchievementDifficulty.Easy,
                RequiredValue = 1000,
                RewardExp = 200
            });
            
            AddAchievement(new Achievement
            {
                Id = "gold_10000",
                Name = "富甲一方",
                Description = "累计获得10000金币",
                Type = AchievementType.Gold,
                Difficulty = AchievementDifficulty.Normal,
                RequiredValue = 10000,
                RewardExp = 1000
            });
            
            AddAchievement(new Achievement
            {
                Id = "gold_100000",
                Name = "金币大亨",
                Description = "累计获得100000金币",
                Type = AchievementType.Gold,
                Difficulty = AchievementDifficulty.Epic,
                RequiredValue = 100000,
                RewardExp = 5000
            });
            
            // Boss achievements
            AddAchievement(new Achievement
            {
                Id = "boss_1",
                Name = "首胜",
                Description = "击败第一个Boss",
                Type = AchievementType.Boss,
                Difficulty = AchievementDifficulty.Normal,
                RequiredValue = 1,
                RewardGold = 500,
                RewardExp = 1000
            });
            
            AddAchievement(new Achievement
            {
                Id = "boss_5",
                Name = "Boss猎人",
                Description = "击败5个Boss",
                Type = AchievementType.Boss,
                Difficulty = AchievementDifficulty.Hard,
                RequiredValue = 5,
                RewardGold = 2000,
                RewardExp = 3000
            });
            
            AddAchievement(new Achievement
            {
                Id = "boss_all",
                Name = "Boss克星",
                Description = "击败所有Boss",
                Type = AchievementType.Boss,
                Difficulty = AchievementDifficulty.Legendary,
                RequiredValue = 9,
                RewardGold = 10000,
                RewardExp = 20000
            });
            
            // Craft achievements
            AddAchievement(new Achievement
            {
                Id = "craft_1",
                Name = "新手匠人",
                Description = "合成第一件装备",
                Type = AchievementType.Craft,
                Difficulty = AchievementDifficulty.Easy,
                RequiredValue = 1,
                RewardGold = 100,
                RewardExp = 200
            });
            
            AddAchievement(new Achievement
            {
                Id = "craft_10",
                Name = "熟练工匠",
                Description = "合成10件装备",
                Type = AchievementType.Craft,
                Difficulty = AchievementDifficulty.Normal,
                RequiredValue = 10,
                RewardGold = 500,
                RewardExp = 1000
            });
            
            AddAchievement(new Achievement
            {
                Id = "craft_50",
                Name = "大师级铁匠",
                Description = "合成50件装备",
                Type = AchievementType.Craft,
                Difficulty = AchievementDifficulty.Hard,
                RequiredValue = 50,
                RewardGold = 3000,
                RewardExp = 5000
            });
            
            // Quest achievements
            AddAchievement(new Achievement
            {
                Id = "quest_1",
                Name = "初试任务",
                Description = "完成第一个任务",
                Type = AchievementType.Quest,
                Difficulty = AchievementDifficulty.Easy,
                RequiredValue = 1,
                RewardGold = 100,
                RewardExp = 200
            });
            
            AddAchievement(new Achievement
            {
                Id = "quest_10",
                Name = "任务达人",
                Description = "完成10个任务",
                Type = AchievementType.Quest,
                Difficulty = AchievementDifficulty.Normal,
                RequiredValue = 10,
                RewardGold = 500,
                RewardExp = 1500
            });
            
            AddAchievement(new Achievement
            {
                Id = "quest_all",
                Name = "任务大师",
                Description = "完成所有任务",
                Type = AchievementType.Quest,
                Difficulty = AchievementDifficulty.Epic,
                RequiredValue = 15,
                RewardGold = 5000,
                RewardExp = 10000
            });
            
            // Combo achievements
            AddAchievement(new Achievement
            {
                Id = "combo_10",
                Name = "连击初学者",
                Description = "达成10连击",
                Type = AchievementType.Combo,
                Difficulty = AchievementDifficulty.Easy,
                RequiredValue = 10,
                RewardGold = 100,
                RewardExp = 300
            });
            
            AddAchievement(new Achievement
            {
                Id = "combo_50",
                Name = "连击达人",
                Description = "达成50连击",
                Type = AchievementType.Combo,
                Difficulty = AchievementDifficulty.Normal,
                RequiredValue = 50,
                RewardGold = 500,
                RewardExp = 1000
            });
            
            AddAchievement(new Achievement
            {
                Id = "combo_100",
                Name = "连击王者",
                Description = "达成100连击",
                Type = AchievementType.Combo,
                Difficulty = AchievementDifficulty.Hard,
                RequiredValue = 100,
                RewardGold = 2000,
                RewardExp = 3000
            });
            
            // Survival achievements
            AddAchievement(new Achievement
            {
                Id = "survive_1min",
                Name = "生存新手",
                Description = "累计生存1分钟",
                Type = AchievementType.Survival,
                Difficulty = AchievementDifficulty.Easy,
                RequiredValue = 60,
                RewardGold = 50,
                RewardExp = 100
            });
            
            AddAchievement(new Achievement
            {
                Id = "survive_10min",
                Name = "生存专家",
                Description = "累计生存10分钟",
                Type = AchievementType.Survival,
                Difficulty = AchievementDifficulty.Normal,
                RequiredValue = 600,
                RewardGold = 300,
                RewardExp = 500
            });
            
            AddAchievement(new Achievement
            {
                Id = "survive_1hour",
                Name = "生存大师",
                Description = "累计生存1小时",
                Type = AchievementType.Survival,
                Difficulty = AchievementDifficulty.Hard,
                RequiredValue = 3600,
                RewardGold = 2000,
                RewardExp = 3000
            });
            
            // Damage achievements
            AddAchievement(new Achievement
            {
                Id = "damage_1000",
                Name = "初露锋芒",
                Description = "单次造成1000伤害",
                Type = AchievementType.Damage,
                Difficulty = AchievementDifficulty.Easy,
                RequiredValue = 1000,
                RewardGold = 200,
                RewardExp = 300
            });
            
            AddAchievement(new Achievement
            {
                Id = "damage_10000",
                Name = "伤害输出者",
                Description = "单次造成10000伤害",
                Type = AchievementType.Damage,
                Difficulty = AchievementDifficulty.Normal,
                RequiredValue = 10000,
                RewardGold = 1000,
                RewardExp = 2000
            });
            
            AddAchievement(new Achievement
            {
                Id = "damage_100000",
                Name = "毁灭者",
                Description = "单次造成100000伤害",
                Type = AchievementType.Damage,
                Difficulty = AchievementDifficulty.Epic,
                RequiredValue = 100000,
                RewardGold = 5000,
                RewardExp = 10000
            });
            
            // Skill achievements
            AddAchievement(new Achievement
            {
                Id = "skill_learn_5",
                Name = "技能学徒",
                Description = "学习5个技能",
                Type = AchievementType.Skill,
                Difficulty = AchievementDifficulty.Easy,
                RequiredValue = 5,
                RewardGold = 200,
                RewardExp = 500
            });
            
            AddAchievement(new Achievement
            {
                Id = "skill_learn_15",
                Name = "技能大师",
                Description = "学习15个技能",
                Type = AchievementType.Skill,
                Difficulty = AchievementDifficulty.Normal,
                RequiredValue = 15,
                RewardGold = 1000,
                RewardExp = 2000
            });
            
            // Explore achievements
            AddAchievement(new Achievement
            {
                Id = "explore_3",
                Name = "初探世界",
                Description = "探索3个区域",
                Type = AchievementType.Explore,
                Difficulty = AchievementDifficulty.Easy,
                RequiredValue = 3,
                RewardGold = 100,
                RewardExp = 200
            });
            
            AddAchievement(new Achievement
            {
                Id = "explore_all",
                Name = "世界探索者",
                Description = "探索所有区域",
                Type = AchievementType.Explore,
                Difficulty = AchievementDifficulty.Hard,
                RequiredValue = 7,
                RewardGold = 2000,
                RewardExp = 5000
            });
            
            // Enrage kill achievements - Boss狂暴击杀成就
            AddAchievement(new Achievement
            {
                Id = "enrage_kill_1",
                Name = "狂暴杀手",
                Description = "在Boss狂暴状态下击杀1个Boss",
                Type = AchievementType.EnrageKill,
                Difficulty = AchievementDifficulty.Normal,
                RequiredValue = 1,
                RewardGold = 1000,
                RewardExp = 2000
            });
            
            AddAchievement(new Achievement
            {
                Id = "enrage_kill_5",
                Name = "狂暴猎手",
                Description = "在Boss狂暴状态下击杀5个Boss",
                Type = AchievementType.EnrageKill,
                Difficulty = AchievementDifficulty.Hard,
                RequiredValue = 5,
                RewardGold = 3000,
                RewardExp = 5000
            });
            
            AddAchievement(new Achievement
            {
                Id = "enrage_kill_10",
                Name = "狂暴终结者",
                Description = "在Boss狂暴状态下击杀10个Boss",
                Type = AchievementType.EnrageKill,
                Difficulty = AchievementDifficulty.Epic,
                RequiredValue = 10,
                RewardGold = 8000,
                RewardExp = 15000
            });
            
            // Perfect block achievements - 完美格挡成就
            AddAchievement(new Achievement
            {
                Id = "perfect_block_10",
                Name = "格挡新手",
                Description = "完成10次完美格挡",
                Type = AchievementType.PerfectBlock,
                Difficulty = AchievementDifficulty.Easy,
                RequiredValue = 10,
                RewardGold = 200,
                RewardExp = 500
            });
            
            AddAchievement(new Achievement
            {
                Id = "perfect_block_50",
                Name = "格挡大师",
                Description = "完成50次完美格挡",
                Type = AchievementType.PerfectBlock,
                Difficulty = AchievementDifficulty.Normal,
                RequiredValue = 50,
                RewardGold = 800,
                RewardExp = 1500
            });
            
            AddAchievement(new Achievement
            {
                Id = "perfect_block_100",
                Name = "铁壁防御者",
                Description = "完成100次完美格挡",
                Type = AchievementType.PerfectBlock,
                Difficulty = AchievementDifficulty.Hard,
                RequiredValue = 100,
                RewardGold = 2000,
                RewardExp = 3000
            });
            
            // Counter attack achievements - 反击成就
            AddAchievement(new Achievement
            {
                Id = "counter_5",
                Name = "反击新手",
                Description = "成功完成5次反击",
                Type = AchievementType.CounterAttack,
                Difficulty = AchievementDifficulty.Easy,
                RequiredValue = 5,
                RewardGold = 300,
                RewardExp = 500
            });
            
            AddAchievement(new Achievement
            {
                Id = "counter_25",
                Name = "反击达人",
                Description = "成功完成25次反击",
                Type = AchievementType.CounterAttack,
                Difficulty = AchievementDifficulty.Normal,
                RequiredValue = 25,
                RewardGold = 1000,
                RewardExp = 2000
            });
            
            AddAchievement(new Achievement
            {
                Id = "counter_50",
                Name = "反击大师",
                Description = "成功完成50次反击",
                Type = AchievementType.CounterAttack,
                Difficulty = AchievementDifficulty.Hard,
                RequiredValue = 50,
                RewardGold = 2500,
                RewardExp = 5000
            });
            
            // No-hit boss achievements - 无伤Boss成就
            AddAchievement(new Achievement
            {
                Id = "nohit_boss_1",
                Name = "无伤初战",
                Description = "无伤击败1个Boss",
                Type = AchievementType.NoHitBoss,
                Difficulty = AchievementDifficulty.Hard,
                RequiredValue = 1,
                RewardGold = 2000,
                RewardExp = 3000
            });
            
            AddAchievement(new Achievement
            {
                Id = "nohit_boss_3",
                Name = "无伤猎手",
                Description = "无伤击败3个Boss",
                Type = AchievementType.NoHitBoss,
                Difficulty = AchievementDifficulty.Epic,
                RequiredValue = 3,
                RewardGold = 5000,
                RewardExp = 8000
            });
        }
        
        private void AddAchievement(Achievement achievement)
        {
            _achievements[achievement.Id] = achievement;
        }
        
        public Achievement GetAchievement(string id)
        {
            return _achievements.ContainsKey(id) ? _achievements[id] : null;
        }
        
        public List<Achievement> GetAllAchievements()
        {
            return new List<Achievement>(_achievements.Values);
        }
        
        public List<Achievement> GetAchievementsByType(AchievementType type)
        {
            List<Achievement> result = new List<Achievement>();
            foreach (var achievement in _achievements.Values)
            {
                if (achievement.Type == type)
                {
                    result.Add(achievement);
                }
            }
            return result;
        }
        
        public int GetTotalCount() => _achievements.Count;
        
        public int GetUnlockedCount(List<Achievement> unlockedAchievements)
        {
            return unlockedAchievements.Count;
        }
    }
}
