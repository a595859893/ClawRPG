using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.Quests {
    /// <summary>
    /// Quest class
    /// </summary>
    public class Quest
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public QuestType Type { get; set; }
        public QuestDifficulty Difficulty { get; set; }
        
        // Requirements
        public int RequiredLevel { get; set; } = 1;
        public int[] RequiredQuests { get; set; } = new int[0]; // Quest IDs that must be completed first
        
        // Objectives
        public List<QuestObjective> Objectives { get; set; } = new();
        
        // Rewards
        public int ExperienceReward { get; set; }
        public int GoldReward { get; set; }
        public int[] ItemRewards { get; set; } = new int[0]; // Item IDs
        
        public bool IsMainQuest => Type == QuestType.Main;
        
        public enum QuestType { Main, Side, Daily }
        public enum QuestDifficulty { Easy, Normal, Hard, Boss }
    }
    
    /// <summary>
    /// Quest objective
    /// </summary>
    public class QuestObjective
    {
        public string Description { get; set; } = "";
        public ObjectiveType Type { get; set; }
        public string TargetId { get; set; } // Enemy ID, item ID, or location
        public int RequiredAmount { get; set; } = 1;
        public int CurrentAmount { get; set; }
        
        public bool IsComplete => CurrentAmount >= RequiredAmount;
        
        public enum ObjectiveType { Kill, Collect, Talk, Reach, Use }
    }
    
    /// <summary>
    /// Quest status
    /// </summary>
    public enum QuestStatus { NotStarted, Active, Completed, TurnedIn }
    
    /// <summary>
    /// Quest database
    /// </summary>
    public class QuestDatabase
    {
        private static QuestDatabase _instance;
        public static QuestDatabase Instance => _instance ??= new QuestDatabase();
        
        private Dictionary<int, Quest> _quests = new();
        
        public QuestDatabase()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            // Main Quests
            AddQuest(new Quest 
            { 
                Id = 1, Name = "初入冒险", Description = "与村长对话，了解情况",
                Type = Quest.QuestType.Main, Difficulty = Quest.QuestDifficulty.Easy,
                RequiredLevel = 1,
                Objectives = new List<QuestObjective> 
                { 
                    new QuestObjective { Description = "与村长对话", Type = QuestObjective.ObjectiveType.Talk, TargetId = "village_chief" }
                },
                ExperienceReward = 50, GoldReward = 10
            });
            
            AddQuest(new Quest 
            { 
                Id = 2, Name = "消灭哥布林", Description = "清除村庄附近的哥布林威胁",
                Type = Quest.QuestType.Main, Difficulty = Quest.QuestDifficulty.Normal,
                RequiredLevel = 1,
                Objectives = new List<QuestObjective> 
                { 
                    new QuestObjective { Description = "击败5只哥布林", Type = QuestObjective.ObjectiveType.Kill, TargetId = "goblin", RequiredAmount = 5 }
                },
                ExperienceReward = 100, GoldReward = 30
            });
            
            AddQuest(new Quest 
            { 
                Id = 3, Name = "森林探索", Description = "探索森林区域",
                Type = Quest.QuestType.Main, Difficulty = Quest.QuestDifficulty.Normal,
                RequiredLevel = 2,
                RequiredQuests = new[] { 2 },
                Objectives = new List<QuestObjective> 
                { 
                    new QuestObjective { Description = "探索森林深处", Type = QuestObjective.ObjectiveType.Reach, TargetId = "deep_forest" }
                },
                ExperienceReward = 150, GoldReward = 50
            });
            
            AddQuest(new Quest 
            { 
                Id = 4, Name = "暗影法师", Description = "击败暗影法师",
                Type = Quest.QuestType.Main, Difficulty = Quest.QuestDifficulty.Hard,
                RequiredLevel = 4,
                RequiredQuests = new[] { 3 },
                Objectives = new List<QuestObjective> 
                { 
                    new QuestObjective { Description = "击败暗影法师", Type = QuestObjective.ObjectiveType.Kill, TargetId = "shadow_mage" }
                },
                ExperienceReward = 500, GoldReward = 200
            });
            
            AddQuest(new Quest 
            { 
                Id = 5, Name = "龙的挑战", Description = "挑战并击败巨龙",
                Type = Quest.QuestType.Main, Difficulty = Quest.QuestDifficulty.Boss,
                RequiredLevel = 5,
                RequiredQuests = new[] { 4 },
                Objectives = new List<QuestObjective> 
                { 
                    new QuestObjective { Description = "击败巨龙", Type = QuestObjective.ObjectiveType.Kill, TargetId = "dragon" }
                },
                ExperienceReward = 1000, GoldReward = 500, ItemRewards = new[] { 8 } // Legendary sword
            });
            
            // Side Quests
            AddQuest(new Quest 
            { 
                Id = 101, Name = "怪物猎人", Description = "消灭各种怪物",
                Type = Quest.QuestType.Side, Difficulty = Quest.QuestDifficulty.Normal,
                RequiredLevel = 2,
                Objectives = new List<QuestObjective> 
                { 
                    new QuestObjective { Description = "击败10只怪物", Type = QuestObjective.ObjectiveType.Kill, TargetId = "any", RequiredAmount = 10 }
                },
                ExperienceReward = 200, GoldReward = 100
            });
            
            AddQuest(new Quest 
            { 
                Id = 102, Name = "古老宝藏", Description = "寻找古老宝藏",
                Type = Quest.QuestType.Side, Difficulty = Quest.QuestDifficulty.Normal,
                RequiredLevel = 2,
                Objectives = new List<QuestObjective> 
                { 
                    new QuestObjective { Description = "收集5个古钱币", Type = QuestObjective.ObjectiveType.Collect, TargetId = "ancient_coin", RequiredAmount = 5 }
                },
                ExperienceReward = 150, GoldReward = 50
            });
            
            AddQuest(new Quest 
            { 
                Id = 103, Name = "铁匠的请求", Description = "收集铁匠需要的材料",
                Type = Quest.QuestType.Side, Difficulty = Quest.QuestDifficulty.Easy,
                RequiredLevel = 1,
                Objectives = new List<QuestObjective> 
                { 
                    new QuestObjective { Description = "收集10个怪物精华", Type = QuestObjective.ObjectiveType.Collect, TargetId = "monster_essence", RequiredAmount = 10 }
                },
                ExperienceReward = 80, GoldReward = 30
            });
            
            AddQuest(new Quest 
            { 
                Id = 104, Name = "炼金材料", Description = "收集炼金所需材料",
                Type = Quest.QuestType.Side, Difficulty = Quest.QuestDifficulty.Normal,
                RequiredLevel = 2,
                Objectives = new List<QuestObjective> 
                { 
                    new QuestObjective { Description = "收集3个史莱姆凝胶", Type = QuestObjective.ObjectiveType.Collect, TargetId = "slime_gel", RequiredAmount = 3 }
                },
                ExperienceReward = 100, GoldReward = 40
            });
            
            AddQuest(new Quest 
            { 
                Id = 105, Name = "死灵法师的请求", Description = "收集骷髅骨头",
                Type = Quest.QuestType.Side, Difficulty = Quest.QuestDifficulty.Normal,
                RequiredLevel = 3,
                Objectives = new List<QuestObjective> 
                { 
                    new QuestObjective { Description = "收集5个骷髅骨头", Type = QuestObjective.ObjectiveType.Collect, TargetId = "skeleton_bone", RequiredAmount = 5 }
                },
                ExperienceReward = 180, GoldReward = 70
            });
            
            AddQuest(new Quest 
            { 
                Id = 106, Name = "神圣使命", Description = "收集神圣物品",
                Type = Quest.QuestType.Side, Difficulty = Quest.QuestDifficulty.Hard,
                RequiredLevel = 4,
                Objectives = new List<QuestObjective> 
                { 
                    new QuestObjective { Description = "收集3个神圣宝珠", Type = QuestObjective.ObjectiveType.Collect, TargetId = "holy_orb", RequiredAmount = 3 }
                },
                ExperienceReward = 300, GoldReward = 150
            });
            
            AddQuest(new Quest 
            { 
                Id = 107, Name = "龙鳞收集", Description = "收集龙鳞用于装备",
                Type = Quest.QuestType.Side, Difficulty = Quest.QuestDifficulty.Hard,
                RequiredLevel = 5,
                Objectives = new List<QuestObjective> 
                { 
                    new QuestObjective { Description = "收集5个龙鳞", Type = QuestObjective.ObjectiveType.Collect, TargetId = "dragon_scale", RequiredAmount = 5 }
                },
                ExperienceReward = 400, GoldReward = 200, ItemRewards = new[] { 105 } // Dragon scale armor
            });
            
            AddQuest(new Quest 
            { 
                Id = 108, Name = "凤凰羽毛", Description = "寻找传说中的凤凰羽毛",
                Type = Quest.QuestType.Side, Difficulty = Quest.QuestDifficulty.Hard,
                RequiredLevel = 4,
                Objectives = new List<QuestObjective> 
                { 
                    new QuestObjective { Description = "获取凤凰羽毛", Type = QuestObjective.ObjectiveType.Collect, TargetId = "phoenix_feather", RequiredAmount = 1 }
                },
                ExperienceReward = 500, GoldReward = 250
            });
        }
        
        private void AddQuest(Quest quest)
        {
            _quests[quest.Id] = quest;
        }
        
        public Quest GetQuest(int id)
        {
            return _quests.ContainsKey(id) ? _quests[id] : null;
        }
        
        public List<Quest> GetAllQuests()
        {
            return new List<Quest>(_quests.Values);
        }
        
        public List<Quest> GetMainQuests()
        {
            var result = new List<Quest>();
            foreach (var quest in _quests.Values)
            {
                if (quest.IsMainQuest) result.Add(quest);
            }
            return result;
        }
        
        public List<Quest> GetSideQuests()
        {
            var result = new List<Quest>();
            foreach (var quest in _quests.Values)
            {
                if (quest.Type == Quest.QuestType.Side) result.Add(quest);
            }
            return result;
        }
    }
    
    /// <summary>
    /// Quest manager - handles quest progress
    /// </summary>
    public class QuestManager
    {
        private Dictionary<int, QuestStatus> _questStatus = new();
        
        // Signals for UI updates
        public static event Action<Quest> OnQuestAccepted;
        public static event Action<Quest> OnQuestCompleted;
        public static event Action<Quest, QuestObjective> OnQuestObjectiveUpdated;
        public static event Action<Quest> OnQuestTurnedIn;
        
        public QuestStatus GetQuestStatus(int questId)
        {
            return _questStatus.ContainsKey(questId) ? _questStatus[questId] : QuestStatus.NotStarted;
        }
        
        public void StartQuest(int questId)
        {
            var quest = QuestDatabase.Instance.GetQuest(questId);
            if (quest == null) return;
            
            // Check requirements
            foreach (var req in quest.RequiredQuests)
            {
                if (GetQuestStatus(req) != QuestStatus.TurnedIn)
                {
                    GD.Print("Cannot start quest - required quest not completed");
                    return;
                }
            }
            
            _questStatus[questId] = QuestStatus.Active;
            GD.Print("Started quest: " + quest.Name);
            
            // Trigger signal
            OnQuestAccepted?.Invoke(quest);
        }
        
        public void UpdateObjective(string targetId, int amount = 1)
        {
            foreach (var kvp in _questStatus)
            {
                if (kvp.Value != QuestStatus.Active) continue;
                
                var quest = QuestDatabase.Instance.GetQuest(kvp.Key);
                foreach (var obj in quest.Objectives)
                {
                    if (obj.TargetId == targetId || targetId == "any")
                    {
                        obj.CurrentAmount += amount;
                        GD.Print("Objective progress: " + obj.Description + " (" + obj.CurrentAmount + "/" + obj.RequiredAmount + ")");
                        
                        // Trigger signal
                        OnQuestObjectiveUpdated?.Invoke(quest, obj);
                        
                        if (obj.IsComplete)
                        {
                            CheckQuestCompletion(kvp.Key);
                        }
                    }
                }
            }
        }
        
        public void CheckQuestCompletion(int questId)
        {
            var quest = QuestDatabase.Instance.GetQuest(questId);
            bool allComplete = true;
            
            foreach (var obj in quest.Objectives)
            {
                if (!obj.IsComplete)
                {
                    allComplete = false;
                    break;
                }
            }
            
            if (allComplete)
            {
                GD.Print("Quest ready to turn in: " + quest.Name);
            }
        }
        
        public void TurnInQuest(int questId, CharacterBody2D player)
        {
            var quest = QuestDatabase.Instance.GetQuest(questId);
            if (quest == null || GetQuestStatus(questId) != QuestStatus.Active) return;
            
            // Check completion
            foreach (var obj in quest.Objectives)
            {
                if (!obj.IsComplete)
                {
                    GD.Print("Quest objectives not complete!");
                    return;
                }
            }
            
            // Give rewards
            player.GainExperience(quest.ExperienceReward);
            // Give gold
            if (player is Player p)
            {
                p.AddGold(quest.GoldReward);
            }
            
            // Track achievement progress
            AchievementManager.Instance.TrackQuestComplete();
            
            _questStatus[questId] = QuestStatus.TurnedIn;
            GD.Print("Quest completed: " + quest.Name + "! Rewards: " + quest.ExperienceReward + " XP, " + quest.GoldReward + " Gold");
            
            // Trigger signals
            OnQuestCompleted?.Invoke(quest);
            OnQuestTurnedIn?.Invoke(quest);
        }
        
        public List<Quest> GetActiveQuests()
        {
            var result = new List<Quest>();
            foreach (var kvp in _questStatus)
            {
                if (kvp.Value == QuestStatus.Active)
                {
                    result.Add(QuestDatabase.Instance.GetQuest(kvp.Key));
                }
            }
            return result;
        }
    }
}
