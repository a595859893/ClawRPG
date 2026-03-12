using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.GuildQuestBoard {
    /**
     * GuildQuestBoardSystem - 公会任务布告栏系统
     * 允许公会发布任务，玩家接受并完成任务获得奖励
     */
    public class GuildQuestBoardSystem : Node {
        // 单例
        private static GuildQuestBoardSystem _instance;
        public static GuildQuestBoardSystem Instance => _instance;
        
        // 数据
        private GuildQuestBoardData _data;
        
        // 信号
        public static string QuestAcceptedSignal = "quest_accepted";
        public static string QuestCompletedSignal = "quest_completed";
        public static string QuestPublishedSignal = "quest_published";
        public static string QuestCancelledSignal = "quest_cancelled";
        public static string ProgressUpdatedSignal = "progress_updated";
        
        public override void _Ready() {
            _instance = this;
            _data = new GuildQuestBoardData();
            LoadData();
        }
        
        #region Data Management
        
        private void LoadData() {
            // 从存档加载数据
            var saveSystem = GetTree().Root.GetNode<ClawRPG.Scripts.Systems.SaveLoad.SaveLoadSystem>("SaveLoadSystem");
            if (saveSystem != null) {
                var loadedData = saveSystem.LoadGuildQuestBoardData();
                if (loadedData != null) {
                    _data = loadedData;
                }
            }
        }
        
        public void SaveData() {
            var saveSystem = GetTree().Root.GetNode<ClawRPG.Scripts.Systems.SaveLoad.SaveLoadSystem>("SaveLoadSystem");
            if (saveSystem != null) {
                saveSystem.SaveGuildQuestBoardData(_data);
            }
        }
        
        #endregion
        
        #region Quest Management
        
        /**
         * 发布新任务
         */
        public int PublishQuest(string title, string description, QuestType type, Difficulty difficulty, 
                               int requiredCount, int rewardGold, int rewardExp, int rewardGuildPoints,
                               string publisherName, bool isDaily = false) {
            // 检查权限
            if (!CheckPublishPermission()) {
                GD.Print("你没有权限发布任务");
                return -1;
            }
            
            // 检查每日限制
            if (!CheckDailyLimit()) {
                GD.Print("今日发布任务数量已达上限");
                return -1;
            }
            
            var quest = new QuestBoardQuest {
                Id = _data.nextQuestId++,
                Title = title,
                Description = description,
                QuestType = type,
                Difficulty = difficulty,
                RequiredCount = requiredCount,
                CurrentProgress = 0,
                RewardGold = rewardGold,
                RewardExp = rewardExp,
                RewardGuildPoints = rewardGuildPoints,
                PublisherName = publisherName,
                PublishTime = DateTime.Now.Ticks,
                IsCompleted = false,
                IsDaily = isDaily
            };
            
            _data.availableQuests[quest.Id] = quest;
            _data.todayPublishedCount++;
            
            // 触发信号
            EmitSignal(QuestPublishedSignal, quest.Id);
            
            // 保存数据
            SaveData();
            
            return quest.Id;
        }
        
        /**
         * 接受任务
         */
        public bool AcceptQuest(int questId, string playerName) {
            if (!_data.availableQuests.ContainsKey(questId)) {
                GD.Print("任务不存在");
                return false;
            }
            
            var quest = _data.availableQuests[questId];
            
            // 检查是否已完成
            if (quest.IsCompleted) {
                GD.Print("任务已完成");
                return false;
            }
            
            // 检查是否已接受
            if (quest.AcceptedPlayers.Contains(playerName)) {
                GD.Print("你已接受此任务");
                return false;
            }
            
            // 添加到已接受列表
            quest.AcceptedPlayers.Add(playerName);
            _data.acceptedQuestIds.Add(questId);
            
            // 触发信号
            EmitSignal(QuestAcceptedSignal, questId, playerName);
            
            // 保存数据
            SaveData();
            
            return true;
        }
        
        /**
         * 放弃任务
         */
        public bool AbandonQuest(int questId, string playerName) {
            if (!_data.availableQuests.ContainsKey(questId)) {
                return false;
            }
            
            var quest = _data.availableQuests[questId];
            
            if (quest.AcceptedPlayers.Contains(playerName)) {
                quest.AcceptedPlayers.Remove(playerName);
                _data.acceptedQuestIds.Remove(questId);
                
                // 触发信号
                EmitSignal(QuestCancelledSignal, questId, playerName);
                
                SaveData();
                return true;
            }
            
            return false;
        }
        
        /**
         * 更新任务进度
         */
        public void UpdateProgress(string playerName, QuestType questType, int count = 1) {
            foreach (var kvp in _data.availableQuests) {
                var quest = kvp.Value;
                
                // 检查是否接受此任务
                if (!quest.AcceptedPlayers.Contains(playerName)) continue;
                
                // 检查任务类型
                if (quest.QuestType != questType) continue;
                
                // 检查是否已完成
                if (quest.IsCompleted) continue;
                
                // 更新进度
                quest.CurrentProgress += count;
                
                // 检查是否完成
                if (quest.CurrentProgress >= quest.RequiredCount) {
                    CompleteQuest(quest.Id, playerName);
                }
                
                // 触发信号
                EmitSignal(ProgressUpdatedSignal, quest.Id, playerName, quest.CurrentProgress);
            }
            
            SaveData();
        }
        
        /**
         * 完成任务
         */
        private bool CompleteQuest(int questId, string playerName) {
            if (!_data.availableQuests.ContainsKey(questId)) {
                return false;
            }
            
            var quest = _data.availableQuests[questId];
            
            if (!quest.AcceptedPlayers.Contains(playerName)) {
                return false;
            }
            
            // 标记完成
            quest.IsCompleted = true;
            quest.CompletionCount++;
            
            // 发放奖励
            // 这里应该调用经济系统发放金币和经验
            // var economySystem = GetTree().Root.GetNode<...>();
            
            // 触发信号
            EmitSignal(QuestCompletedSignal, questId, playerName);
            
            // 从已接受列表中移除
            quest.AcceptedPlayers.Remove(playerName);
            _data.acceptedQuestIds.Remove(questId);
            
            // 如果不是日常任务，可以考虑移除或保留
            if (!quest.IsDaily) {
                // 可以保留历史记录或移除
            }
            
            SaveData();
            
            return true;
        }
        
        /**
         * 取消任务（发布者）
         */
        public bool CancelQuest(int questId) {
            if (!_data.availableQuests.ContainsKey(questId)) {
                return false;
            }
            
            var quest = _data.availableQuests[questId];
            
            // 通知所有接受的玩家
            foreach (var player in quest.AcceptedPlayers) {
                EmitSignal(QuestCancelledSignal, questId, player);
            }
            
            // 移除任务
            _data.availableQuests.Remove(questId);
            
            SaveData();
            
            return true;
        }
        
        #endregion
        
        #region Query Methods
        
        /**
         * 获取所有可用任务
         */
        public List<QuestBoardQuest> GetAvailableQuests() {
            var quests = new List<QuestBoardQuest>();
            foreach (var quest in _data.availableQuests.Values) {
                if (!quest.IsCompleted) {
                    quests.Add(quest);
                }
            }
            return quests;
        }
        
        /**
         * 按类型获取任务
         */
        public List<QuestBoardQuest> GetQuestsByType(QuestType type) {
            var quests = new List<QuestBoardQuest>();
            foreach (var quest in _data.availableQuests.Values) {
                if (!quest.IsCompleted && quest.QuestType == type) {
                    quests.Add(quest);
                }
            }
            return quests;
        }
        
        /**
         * 按难度获取任务
         */
        public List<QuestBoardQuest> GetQuestsByDifficulty(Difficulty difficulty) {
            var quests = new List<QuestBoardQuest>();
            foreach (var quest in _data.availableQuests.Values) {
                if (!quest.IsCompleted && quest.Difficulty == difficulty) {
                    quests.Add(quest);
                }
            }
            return quests;
        }
        
        /**
         * 获取玩家已接受的任务
         */
        public List<QuestBoardQuest> GetAcceptedQuests(string playerName) {
            var quests = new List<QuestBoardQuest>();
            foreach (var quest in _data.availableQuests.Values) {
                if (quest.AcceptedPlayers.Contains(playerName) && !quest.IsCompleted) {
                    quests.Add(quest);
                }
            }
            return quests;
        }
        
        /**
         * 获取任务详情
         */
        public QuestBoardQuest GetQuest(int questId) {
            if (_data.availableQuests.ContainsKey(questId)) {
                return _data.availableQuests[questId];
            }
            return null;
        }
        
        /**
         * 获取每日任务
         */
        public List<QuestBoardQuest> GetDailyQuests() {
            var quests = new List<QuestBoardQuest>();
            foreach (var quest in _data.availableQuests.Values) {
                if (!quest.IsCompleted && quest.IsDaily) {
                    quests.Add(quest);
                }
            }
            return quests;
        }
        
        #endregion
        
        #region Permission & Limits
        
        /**
         * 检查发布权限
         */
        private bool CheckPublishPermission() {
            // 简化版本：任何人都可以发布
            // 实际版本应该检查玩家在公会中的职位
            return true;
        }
        
        /**
         * 检查每日限制
         */
        private bool CheckDailyLimit() {
            // 检查是否需要重置每日计数
            var now = DateTime.Now;
            var lastReset = new DateTime(_data.lastResetTime);
            
            if (now.Date != lastReset.Date) {
                _data.todayPublishedCount = 0;
                _data.lastResetTime = now.Ticks;
            }
            
            return _data.todayPublishedCount < _data.dailyPublishLimit;
        }
        
        /**
         * 设置发布权限等级
         */
        public void SetPublishPermissionLevel(int level) {
            _data.publishPermissionLevel = Mathf.Clamp(level, 1, 4);
            SaveData();
        }
        
        /**
         * 设置每日发布限制
         */
        public void SetDailyPublishLimit(int limit) {
            _data.dailyPublishLimit = Mathf.Max(1, limit);
            SaveData();
        }
        
        #endregion
        
        #region Statistics
        
        /**
         * 获取统计数据
         */
        public Dictionary<string, object> GetStatistics() {
            var stats = new Dictionary<string, object>();
            
            int totalQuests = 0;
            int completedQuests = 0;
            int dailyQuests = 0;
            
            foreach (var quest in _data.availableQuests.Values) {
                totalQuests++;
                if (quest.IsCompleted) completedQuests++;
                if (quest.IsDaily) dailyQuests++;
            }
            
            stats["total_quests"] = totalQuests;
            stats["completed_quests"] = completedQuests;
            stats["daily_quests"] = dailyQuests;
            stats["total_completions"] = completedQuests; // 简化
            stats["today_published"] = _data.todayPublishedCount;
            
            return stats;
        }
        
        /**
         * 获取玩家统计
         */
        public Dictionary<string, object> GetPlayerStatistics(string playerName) {
            var stats = new Dictionary<string, object>();
            
            int acceptedCount = 0;
            int completedCount = 0;
            
            foreach (var quest in _data.availableQuests.Values) {
                if (quest.AcceptedPlayers.Contains(playerName)) {
                    acceptedCount++;
                }
                if (quest.IsCompleted && quest.CompletionCount > 0) {
                    // 检查是否这个玩家完成了任务
                    completedCount++;
                }
            }
            
            stats["accepted_quests"] = acceptedCount;
            stats["completed_quests"] = completedCount;
            
            return stats;
        }
        
        #endregion
    }
}
