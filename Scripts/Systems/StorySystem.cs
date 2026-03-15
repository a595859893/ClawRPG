using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 故事系统 - 管理游戏剧情和章节
    /// </summary>
    public class StoryChapter {
        public int ChapterId;
        public string Title;
        public string Description;
        public int RequiredLevel;
        public int RequiredRegionId;
        public List<StoryObjective> Objectives;
        public List<StoryReward> Rewards;
        public bool IsUnlocked;
        public bool IsCompleted;
    }

    public class StoryObjective {
        public int ObjectiveId;
        public string Description;
        public StoryObjectiveType Type;
        public int TargetId;
        public int TargetCount;
        public int CurrentCount;
    }

    public enum StoryObjectiveType {
        TalkToNPC,
        KillEnemy,
        CollectItem,
        ReachLocation,
        CompleteQuest,
        DefeatBoss,
        ReachLevel
    }

    public class StoryReward {
        public RewardType Type;
        public int ItemId;
        public int Amount;
        public int Gold;
        public int Experience;
        public int SkillPoints;
    }

    public enum RewardType {
        Item,
        Gold,
        Experience,
        SkillPoints
    }

    public class StoryDatabase {
        public static StoryDatabase Instance { get; private set; }
        
        private List<StoryChapter> chapters = new List<StoryChapter>();
        
        public StoryDatabase() {
            Instance = this;
            InitializeChapters();
        }
        
        private void InitializeChapters() {
            // Chapter 1: 初出茅庐
            chapters.Add(new StoryChapter {
                ChapterId = 1,
                Title = "初出茅庐",
                Description = "作为一名新晋冒险者，你来到暮光森林寻找传说中的宝藏。",
                RequiredLevel = 1,
                RequiredRegionId = 0,
                IsUnlocked = true,
                Objectives = new List<StoryObjective> {
                    new StoryObjective {
                        ObjectiveId = 1,
                        Description = "与铁匠对话",
                        Type = StoryObjectiveType.TalkToNPC,
                        TargetId = 1,
                        TargetCount = 1
                    },
                    new StoryObjective {
                        ObjectiveId = 2,
                        Description = "击败5只哥布林",
                        Type = StoryObjectiveType.KillEnemy,
                        TargetId = 1,
                        TargetCount = 5
                    },
                    new StoryObjective {
                        ObjectiveId = 3,
                        Description = "等级达到3级",
                        Type = StoryObjectiveType.ReachLevel,
                        TargetId = 3,
                        TargetCount = 1
                    }
                },
                Rewards = new List<StoryReward> {
                    new StoryReward { Type = RewardType.Gold, Amount = 100 },
                    new StoryReward { Type = RewardType.Experience, Amount = 200 }
                }
            });

            // Chapter 2: 森林试炼
            chapters.Add(new StoryChapter {
                ChapterId = 2,
                Title = "森林试炼",
                Description = "通过试炼后，你需要深入森林寻找古老的树精。",
                RequiredLevel = 5,
                RequiredRegionId = 1,
                Objectives = new List<StoryObjective> {
                    new StoryObjective {
                        ObjectiveId = 4,
                        Description = "与贤者对话",
                        Type = StoryObjectiveType.TalkToNPC,
                        TargetId = 2,
                        TargetCount = 1
                    },
                    new StoryObjective {
                        ObjectiveId = 5,
                        Description = "击败古老树精",
                        Type = StoryObjectiveType.DefeatBoss,
                        TargetId = 1,
                        TargetCount = 1
                    },
                    new StoryObjective {
                        ObjectiveId = 6,
                        Description = "收集10个森林之证",
                        Type = StoryObjectiveType.CollectItem,
                        TargetId = 101,
                        TargetCount = 10
                    }
                },
                Rewards = new List<StoryReward> {
                    new StoryReward { Type = RewardType.Gold, Amount = 300 },
                    new StoryReward { Type = RewardType.Experience, Amount = 500 },
                    new StoryReward { Type = RewardType.SkillPoints, Amount = 2 }
                }
            });

            // Chapter 3: 洞穴探秘
            chapters.Add(new StoryChapter {
                ChapterId = 3,
                Title = "洞穴探秘",
                Description = "击败树精后，你发现了通往幽暗洞穴的入口。",
                RequiredLevel = 10,
                RequiredRegionId = 2,
                Objectives = new List<StoryObjective> {
                    new StoryObjective {
                        ObjectiveId = 7,
                        Description = "探索幽暗洞穴",
                        Type = StoryObjectiveType.ReachLocation,
                        TargetId = 2,
                        TargetCount = 1
                    },
                    new StoryObjective {
                        ObjectiveId = 8,
                        Description = "击败水晶傀儡",
                        Type = StoryObjectiveType.DefeatBoss,
                        TargetId = 2,
                        TargetCount = 1
                    },
                    new StoryObjective {
                        ObjectiveId = 9,
                        Description = "等级达到15级",
                        Type = StoryObjectiveType.ReachLevel,
                        TargetId = 15,
                        TargetCount = 1
                    }
                },
                Rewards = new List<StoryReward> {
                    new StoryReward { Type = RewardType.Gold, Amount = 500 },
                    new StoryReward { Type = RewardType.Experience, Amount = 800 },
                    new StoryReward { Type = RewardType.SkillPoints, Amount = 3 }
                }
            });

            // Chapter 4: 火焰试炼
            chapters.Add(new StoryChapter {
                ChapterId = 4,
                Title = "火焰试炼",
                Description = "穿过洞穴，你来到了烈焰地牢。",
                RequiredLevel = 20,
                RequiredRegionId = 3,
                Objectives = new List<StoryObjective> {
                    new StoryObjective {
                        ObjectiveId = 10,
                        Description = "击败炼狱巨龙",
                        Type = StoryObjectiveType.DefeatBoss,
                        TargetId = 3,
                        TargetCount = 1
                    },
                    new StoryObjective {
                        ObjectiveId = 11,
                        Description = "收集5块火焰精华",
                        Type = StoryObjectiveType.CollectItem,
                        TargetId = 201,
                        TargetCount = 5
                    }
                },
                Rewards = new List<StoryReward> {
                    new StoryReward { Type = RewardType.Gold, Amount = 1000 },
                    new StoryReward { Type = RewardType.Experience, Amount = 1500 },
                    new StoryReward { Type = RewardType.SkillPoints, Amount = 5 }
                }
            });

            // Chapter 5: 冰霜之旅
            chapters.Add(new StoryChapter {
                ChapterId = 5,
                Title = "冰霜之旅",
                Description = "火焰地牢的考验结束后，你来到了冰霜地牢。",
                RequiredLevel = 25,
                RequiredRegionId = 4,
                Objectives = new List<StoryObjective> {
                    new StoryObjective {
                        ObjectiveId = 12,
                        Description = "击败霜翼龙",
                        Type = StoryObjectiveType.DefeatBoss,
                        TargetId = 5,
                        TargetCount = 1
                    },
                    new StoryObjective {
                        ObjectiveId = 13,
                        Description = "等级达到30级",
                        Type = StoryObjectiveType.ReachLevel,
                        TargetId = 30,
                        TargetCount = 1
                    }
                },
                Rewards = new List<StoryReward> {
                    new StoryReward { Type = RewardType.Gold, Amount = 1500 },
                    new StoryReward { Type = RewardType.Experience, Amount = 2000 },
                    new StoryReward { Type = RewardType.SkillPoints, Amount = 5 }
                }
            });

            // Chapter 6: 暗影决战
            chapters.Add(new StoryChapter {
                ChapterId = 6,
                Title = "暗影决战",
                Description = "最终的暗影地牢，击败暗夜刺客。",
                RequiredLevel = 35,
                RequiredRegionId = 5,
                Objectives = new List<StoryObjective> {
                    new StoryObjective {
                        ObjectiveId = 14,
                        Description = "击败暗夜刺客",
                        Type = StoryObjectiveType.DefeatBoss,
                        TargetId = 4,
                        TargetCount = 1
                    }
                },
                Rewards = new List<StoryReward> {
                    new StoryReward { Type = RewardType.Gold, Amount = 2000 },
                    new StoryReward { Type = RewardType.Experience, Amount = 3000 },
                    new StoryReward { Type = RewardType.SkillPoints, Amount = 10 }
                }
            });

            // Chapter 7: 最终决战
            chapters.Add(new StoryChapter {
                ChapterId = 7,
                Title = "最终决战",
                Description = "挑战恶魔领主，成为真正的英雄！",
                RequiredLevel = 40,
                RequiredRegionId = 6,
                Objectives = new List<StoryObjective> {
                    new StoryObjective {
                        ObjectiveId = 15,
                        Description = "击败恶魔领主",
                        Type = StoryObjectiveType.DefeatBoss,
                        TargetId = 6,
                        TargetCount = 1
                    }
                },
                Rewards = new List<StoryReward> {
                    new StoryReward { Type = RewardType.Gold, Amount = 5000 },
                    new StoryReward { Type = RewardType.Experience, Amount = 5000 },
                    new StoryReward { Type = RewardType.SkillPoints, Amount = 20 }
                }
            });
        }
        
        public List<StoryChapter> GetAllChapters() => chapters;
        
        public StoryChapter GetChapter(int chapterId) {
            return chapters.Find(c => c.ChapterId == chapterId);
        }
        
        public StoryChapter GetCurrentChapter() {
            foreach (var chapter in chapters) {
                if (chapter.IsUnlocked && !chapter.IsCompleted) {
                    return chapter;
                }
            }
            return null;
        }
    }

    public class StoryManager : BaseSystem {
        public static StoryManager Instance { get; private set; }
        
        [Signal]
        public delegate void ChapterUnlocked(StoryChapter chapter);
        
        [Signal]
        public delegate void ChapterCompleted(StoryChapter chapter);
        
        [Signal]
        public delegate void ObjectiveProgressUpdated(StoryObjective objective);
        
        [Signal]
        public delegate void ObjectiveCompleted(StoryObjective objective);
        
        [Signal]
        public delegate void RewardClaimed(StoryReward reward);
        
        private StoryDatabase database;
        private int currentChapterId = 1;
        
        public override void _Ready() {
            Instance = this;
            database = new StoryDatabase();
            
            // Connect to player signals
            var player = GetNode<Player>("/root/Main/Player");
            if (player != null) {
                player.Connect("LevelUp", this, nameof(OnPlayerLevelUp));
            }
        }
        
        public void LoadProgress(int chapterId) {
            currentChapterId = chapterId;
            var chapters = database.GetAllChapters();
            for (int i = 0; i < chapters.Count; i++) {
                chapters[i].IsCompleted = i < chapterId - 1;
                chapters[i].IsUnlocked = i < chapterId;
            }
        }
        
        public int GetCurrentChapterId() => currentChapterId;
        
        public List<StoryChapter> GetAllChapters() => database.GetAllChapters();
        
        public StoryChapter GetCurrentChapter() => database.GetCurrentChapter();
        
        public void UpdateObjective(StoryObjectiveType type, int targetId, int amount = 1) {
            var currentChapter = GetCurrentChapter();
            if (currentChapter == null) return;
            
            foreach (var objective in currentChapter.Objectives) {
                if (objective.Type == type && objective.TargetId == targetId) {
                    objective.CurrentCount = Mathf.Min(objective.CurrentCount + amount, objective.TargetCount);
                    EmitSignal(nameof(ObjectiveProgressUpdated), objective);
                    
                    if (objective.CurrentCount >= objective.TargetCount) {
                        EmitSignal(nameof(ObjectiveCompleted), objective);
                    }
                    
                    CheckChapterCompletion();
                }
            }
        }
        
        private void OnPlayerLevelUp(int newLevel) {
            UpdateObjective(StoryObjectiveType.ReachLevel, newLevel);
        }
        
        public void OnEnemyKilled(int enemyTypeId, bool isBoss) {
            if (isBoss) {
                UpdateObjective(StoryObjectiveType.DefeatBoss, enemyTypeId);
            } else {
                UpdateObjective(StoryObjectiveType.KillEnemy, enemyTypeId);
            }
        }
        
        public void OnItemCollected(int itemId) {
            UpdateObjective(StoryObjectiveType.CollectItem, itemId);
        }
        
        public void OnNPCtalked(int npcId) {
            UpdateObjective(StoryObjectiveType.TalkToNPC, npcId);
        }
        
        public void OnQuestCompleted(int questId) {
            UpdateObjective(StoryObjectiveType.CompleteQuest, questId);
        }
        
        private void CheckChapterCompletion() {
            var currentChapter = GetCurrentChapter();
            if (currentChapter == null) return;
            
            bool allComplete = true;
            foreach (var objective in currentChapter.Objectives) {
                if (objective.CurrentCount < objective.TargetCount) {
                    allComplete = false; 
                    break;
                }
            }
            
            if (allComplete) {
                CompleteChapter(currentChapter);
            }
        }
        
        private void CompleteChapter(StoryChapter chapter) {
            chapter.IsCompleted = true;
            currentChapterId = chapter.ChapterId + 1;
            
            // Unlock next chapter
            var chapters = database.GetAllChapters();
            if (currentChapterId <= chapters.Count) {
                chapters[currentChapterId - 1].IsUnlocked = true;
                EmitSignal(nameof(ChapterUnlocked), chapters[currentChapterId - 1]);
            }
            
            // Grant rewards
            foreach (var reward in chapter.Rewards) {
                GrantReward(reward);
            }
            
            EmitSignal(nameof(ChapterCompleted), chapter);
        }
        
        private void GrantReward(StoryReward reward) {
            var player = GetNode<Player>("/root/Main/Player");
            if (player == null) return;
            
            switch (reward.Type) {
                case RewardType.Gold:
                    player.AddGold(reward.Amount);
                    break;
                case RewardType.Experience:
                    player.AddExperience(reward.Amount);
                    break;
                case RewardType.SkillPoints:
                    player.SkillPoints += reward.Amount;
                    break;
                case RewardType.Item:
                    // Add item to inventory
                    break;
            }
            
            EmitSignal(nameof(RewardClaimed), reward);
        }
        
        public Dictionary<string, object> Serialize() {
            var data = new Dictionary<string, object>();
            data["currentChapterId"] = currentChapterId;
            
            var chapters = database.GetAllChapters();
            var objectivesData = new List<Dictionary<string, object>>();
            
            foreach (var chapter in chapters) {
                foreach (var obj in chapter.Objectives) {
                    objectivesData.Add(new Dictionary<string, object> {
                        ["objectiveId"] = obj.ObjectiveId,
                        ["currentCount"] = obj.CurrentCount
                    });
                }
            }
            
            data["objectives"] = objectivesData;
            return data;
        }
        
        public void Deserialize(Dictionary<string, object> data) {
            if (data.ContainsKey("currentChapterId")) {
                LoadProgress((int)data["currentChapterId"]);
            }
            
            if (data.ContainsKey("objectives")) {
                var objectivesData = (List<object>)data["objectives"];
                var chapters = database.GetAllChapters();
                
                foreach (var objData in objectivesData) {
                    var objDict = (Dictionary<string, object>)objData;
                    int objectiveId = (int)objDict["objectiveId"];
                    int currentCount = (int)objDict["currentCount"];
                    
                    foreach (var chapter in chapters) {
                        foreach (var objective in chapter.Objectives) {
                            if (objective.ObjectiveId == objectiveId) {
                                objective.CurrentCount = currentCount;
                            }
                        }
                    }
                }
            }
        }
    }
}
