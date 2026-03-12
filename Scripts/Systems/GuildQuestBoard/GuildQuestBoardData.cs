using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.GuildQuestBoard {
    /**
     * GuildQuestBoardData - 公会任务布告栏数据
     * 存储公会发布的任务和玩家接受的任务
     */
    public class GuildQuestBoardData : Resource {
        // 任务ID -> 任务数据
        public Dictionary<int, QuestBoardQuest> availableQuests = new Dictionary<int, QuestBoardQuest>();
        
        // 玩家已接受的任务
        public List<int> acceptedQuestIds = new List<int>();
        
        // 任务计数器
        public int nextQuestId = 1;
        
        // 任务发布权限等级 (1=普通成员, 2=官员, 3=副会长, 4=会长)
        public int publishPermissionLevel = 2;
        
        // 每日发布限制
        public int dailyPublishLimit = 5;
        public int todayPublishedCount = 0;
        
        // 最后重置时间
        public long lastResetTime = 0;
        
        public GuildQuestBoardData() {
            InitializeDefaultQuests();
        }
        
        private void InitializeDefaultQuests() {
            // 添加一些默认任务
            var defaultQuests = new[] {
                new QuestBoardQuest {
                    Id = 1,
                    Title = "讨伐森林巨狼",
                    Description = "击败10只森林巨狼",
                    QuestType = QuestType.Combat,
                    Difficulty = Difficulty.Normal,
                    RequiredCount = 10,
                    CurrentProgress = 0,
                    RewardGold = 500,
                    RewardExp = 200,
                    RewardGuildPoints = 50,
                    PublisherName = "公会",
                    PublishTime = DateTime.Now.Ticks,
                    IsCompleted = false,
                    IsDaily = true
                },
                new QuestBoardQuest {
                    Id = 2,
                    Title = "收集草药",
                    Description = "收集20份草药",
                    QuestType = QuestType.Gathering,
                    Difficulty = Difficulty.Easy,
                    RequiredCount = 20,
                    CurrentProgress = 0,
                    RewardGold = 300,
                    RewardExp = 100,
                    RewardGuildPoints = 30,
                    PublisherName = "公会",
                    PublishTime = DateTime.Now.Ticks,
                    IsCompleted = false,
                    IsDaily = true
                },
                new QuestBoardQuest {
                    Id = 3,
                    Title = "Boss挑战 - 冰霜巨龙",
                    Description = "击败冰霜巨龙",
                    QuestType = QuestType.Boss,
                    Difficulty = Difficulty.Epic,
                    RequiredCount = 1,
                    CurrentProgress = 0,
                    RewardGold = 5000,
                    RewardExp = 2000,
                    RewardGuildPoints = 500,
                    PublisherName = "公会",
                    PublishTime = DateTime.Now.Ticks,
                    IsCompleted = false,
                    IsDaily = false
                }
            };
            
            foreach (var quest in defaultQuests) {
                availableQuests[quest.Id] = quest;
            }
            nextQuestId = 4;
        }
    }
    
    /**
     * QuestBoardQuest - 任务布告栏任务
     */
    public class QuestBoardQuest {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public QuestType QuestType { get; set; }
        public Difficulty Difficulty { get; set; }
        public int RequiredCount { get; set; }
        public int CurrentProgress { get; set; }
        public int RewardGold { get; set; }
        public int RewardExp { get; set; }
        public int RewardGuildPoints { get; set; }
        public string PublisherName { get; set; }
        public long PublishTime { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDaily { get; set; }
        
        // 接受该任务的玩家
        public List<string> AcceptedPlayers = new List<string>();
        
        // 完成任务统计
        public int CompletionCount { get; set; }
    }
    
    /**
     * QuestType - 任务类型
     */
    public enum QuestType {
        Combat,       // 战斗任务
        Gathering,    // 采集任务
        Crafting,     // 制作任务
        Delivery,     // 送货任务
        Rescue,       // 救援任务
        Hunt,         // 狩猎任务
        Boss,         // Boss任务
        Escort,       // 护送任务
        Exploration,  // 探索任务
        Timed         // 时限任务
    }
    
    /**
     * Difficulty - 难度等级
     */
    public enum Difficulty {
        Easy,       // 简单
        Normal,     // 普通
        Hard,       // 困难
        Epic,       // 史诗
        Legendary   // 传说
    }
}
