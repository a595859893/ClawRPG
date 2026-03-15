using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 成就里程碑系统 - 追踪玩家成就进度里程碑
    /// </summary>
    public class AchievementMilestoneSystem : BaseSystem
    {
        private static AchievementMilestoneSystem _instance;
        public static AchievementMilestoneSystem Instance => _instance ??= new AchievementMilestoneSystem();
        
        public AchievementMilestoneData Data { get; private set; }
        public AchievementMilestoneDatabase Database { get; private set; }
        
        // 信号
        public Action<string, int> OnMilestoneReached;
        
        private AchievementMilestoneSystem()
        {
            Database = AchievementMilestoneDatabase.Instance;
            Data = new AchievementMilestoneData();
        }
        
        protected override void Initialize()
        {
            GD.Print("[AchievementMilestone] System initialized");
            IsInitialized = true;
        }
        
        /// <summary>
        /// 更新成就进度并检查里程碑
        /// </summary>
        public void UpdateProgress(string achievementId, int currentValue, string achievementName = "")
        {
            var milestones = Database.GetMilestones(achievementId);
            if (milestones.Count == 0)
                return;
            
            // 获取当前已达成最高里程碑
            int currentMilestone = Data.Milestones.ContainsKey(achievementId) ? Data.Milestones[achievementId] : 0;
            
            // 检查是否达成新里程碑
            foreach (var milestone in milestones)
            {
                if (milestone.Level > currentMilestone && currentValue >= milestone.Threshold)
                {
                    // 达成新里程碑
                    Data.Milestones[achievementId] = milestone.Level;
                    Data.TotalMilestonesReached++;
                    
                    if (milestone.Level > Data.HighestMilestoneLevel)
                        Data.HighestMilestoneLevel = milestone.Level;
                    
                    // 添加历史记录
                    var historyEntry = new MilestoneHistoryEntry
                    {
                        AchievementId = achievementId,
                        AchievementName = achievementName.IsEmpty() ? achievementId : achievementName,
                        MilestoneLevel = milestone.Level,
                        Timestamp = OS.GetUnixTime()
                    };
                    Data.History.Insert(0, historyEntry);
                    
                    // 保持历史记录不超过100条
                    if (Data.History.Count > 100)
                        Data.History.RemoveAt(Data.History.Count - 1);
                    
                    // 发放奖励
                    GrantReward(milestone.Reward);
                    
                    // 触发信号
                    OnMilestoneReached?.Invoke(achievementId, milestone.Level);
                    
                    GD.Print($"[AchievementMilestone] Milestone reached: {achievementName} Level {milestone.Level} ({currentValue}/{milestone.Threshold})");
                }
            }
        }
        
        private void GrantReward(string rewardId)
        {
            var reward = Database.GetReward(rewardId);
            if (reward == null)
            {
                GD.Print($"[AchievementMilestone] Warning: Reward not found: {rewardId}");
                return;
            }
            
            switch (reward.Type)
            {
                case "gold":
                    GD.Print($"[AchievementMilestone] Reward: {reward.Value} gold");
                    break;
                case "exp":
                    GD.Print($"[AchievementMilestone] Reward: {reward.Value} experience");
                    break;
                case "gem":
                    GD.Print($"[AchievementMilestone] Reward: {reward.Value} gems");
                    break;
                default:
                    GD.Print($"[AchievementMilestone] Reward: {reward.Type} x {reward.Value}");
                    break;
            }
        }
        
        /// <summary>
        /// 获取成就的当前里程碑等级
        /// </summary>
        public int GetMilestoneLevel(string achievementId)
        {
            return Data.Milestones.ContainsKey(achievementId) ? Data.Milestones[achievementId] : 0;
        }
        
        /// <summary>
        /// 获取成就的里程碑进度
        /// </summary>
        public (int current, int max) GetProgress(string achievementId, int currentValue)
        {
            int currentMilestone = GetMilestoneLevel(achievementId);
            var milestones = Database.GetMilestones(achievementId);
            
            if (milestones.Count == 0)
                return (0, 0);
            
            int nextThreshold = 0;
            foreach (var milestone in milestones)
            {
                if (milestone.Level > currentMilestone)
                {
                    nextThreshold = milestone.Threshold;
                    break;
                }
            }
            
            if (nextThreshold == 0)
                return (100, 100); // 已达成最高里程碑
            
            int prevThreshold = 0;
            if (currentMilestone > 0)
            {
                foreach (var milestone in milestones)
                {
                    if (milestone.Level == currentMilestone)
                    {
                        prevThreshold = milestone.Threshold;
                        break;
                    }
                }
            }
            
            int progress = nextThreshold > prevThreshold ? 
                (currentValue - prevThreshold) * 100 / (nextThreshold - prevThreshold) : 100;
            
            return (Math.Clamp(progress, 0, 100), 100);
        }
        
        /// <summary>
        /// 获取里程碑历史
        /// </summary>
        public List<MilestoneHistoryEntry> GetHistory(int count = 10)
        {
            int limit = Math.Min(count, Data.History.Count);
            return Data.History.GetRange(0, limit);
        }
        
        /// <summary>
        /// 获取统计信息
        /// </summary>
        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                ["total_milestones"] = Data.TotalMilestonesReached,
                ["highest_level"] = Data.HighestMilestoneLevel,
                ["achievements_with_milestones"] = Data.Milestones.Count,
                ["history_count"] = Data.History.Count
            };
        }
        
        /// <summary>
        /// 保存数据
        /// </summary>
        public void Save(Dictionary<string, object> data)
        {
            var milestoneData = new Dictionary<string, object>();
            
            // 保存里程碑
            var milestones = new Dictionary<string, int>();
            foreach (var kvp in Data.Milestones)
            {
                milestones[kvp.Key] = kvp.Value;
            }
            milestoneData["milestones"] = milestones;
            
            // 保存历史
            var history = new List<Dictionary<string, object>>();
            foreach (var entry in Data.History)
            {
                history.Add(new Dictionary<string, object>
                {
                    ["achievement_id"] = entry.AchievementId,
                    ["achievement_name"] = entry.AchievementName,
                    ["milestone_level"] = entry.MilestoneLevel,
                    ["timestamp"] = entry.Timestamp
                });
            }
            milestoneData["history"] = history;
            
            milestoneData["total_milestones"] = Data.TotalMilestonesReached;
            milestoneData["highest_level"] = Data.HighestMilestoneLevel;
            
            data["achievement_milestone_data"] = milestoneData;
        }
        
        /// <summary>
        /// 加载数据
        /// </summary>
        public void Load(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("achievement_milestone_data"))
                return;
            
            var milestoneData = (Dictionary<string, object>)data["achievement_milestone_data"];
            
            // 加载里程碑
            Data.Milestones = new Dictionary<string, int>();
            if (milestoneData.ContainsKey("milestones"))
            {
                var milestones = (Dictionary<string, object>)milestoneData["milestones"];
                foreach (var kvp in milestones)
                {
                    Data.Milestones[kvp.Key] = Convert.ToInt32(kvp.Value);
                }
            }
            
            // 加载历史
            Data.History = new List<MilestoneHistoryEntry>();
            if (milestoneData.ContainsKey("history"))
            {
                var history = (List<object>)milestoneData["history"];
                foreach (var entry in history)
                {
                    var dict = (Dictionary<string, object>)entry;
                    Data.History.Add(new MilestoneHistoryEntry
                    {
                        AchievementId = dict["achievement_id"].ToString(),
                        AchievementName = dict["achievement_name"].ToString(),
                        MilestoneLevel = Convert.ToInt32(dict["milestone_level"]),
                        Timestamp = Convert.ToInt32(dict["timestamp"])
                    });
                }
            }
            
            if (milestoneData.ContainsKey("total_milestones"))
                Data.TotalMilestonesReached = Convert.ToInt32(milestoneData["total_milestones"]);
            
            if (milestoneData.ContainsKey("highest_level"))
                Data.HighestMilestoneLevel = Convert.ToInt32(milestoneData["highest_level"]);
            
            GD.Print($"[AchievementMilestone] Loaded: {Data.Milestones.Count} milestones, {Data.History.Count} history entries");
        }
        
        /// <summary>
        /// 重置数据
        /// </summary>
        public void Reset()
        {
            Data = new AchievementMilestoneData();
            GD.Print("[AchievementMilestone] Data reset");
        }
        
        /// <summary>
        /// 导出保存数据 (BaseSystem 接口) - 复用 Save 方法
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            // 复用已有的 Save 方法，转换为 Godot Dictionary
            var internalData = new Dictionary<string, object>();
            Save(internalData);
            
            // 将 System.Collections.Generic.Dictionary 转换为 Godot Dictionary
            return ConvertToGodotDictionary(internalData);
        }
        
        /// <summary>
        /// 导入保存数据 (BaseSystem 接口) - 复用 Load 方法
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null || data.Count == 0)
                return;
            
            // 将 Godot Dictionary 转换为 System.Collections.Generic.Dictionary
            var internalData = ConvertFromGodotDictionary(data);
            
            // 复用已有的 Load 方法
            Load(internalData);
        }
        
        /// <summary>
        /// 将 System Dictionary 转换为 Godot Dictionary
        /// </summary>
        private Dictionary ConvertToGodotDictionary(Dictionary<string, object> internalData)
        {
            var godotDict = new Dictionary();
            foreach (var kvp in internalData)
            {
                godotDict[kvp.Key] = ConvertValueToGodot(kvp.Value);
            }
            return godotDict;
        }
        
        /// <summary>
        /// 将 Godot Dictionary 转换为 System Dictionary
        /// </summary>
        private Dictionary<string, object> ConvertFromGodotDictionary(Dictionary godotData)
        {
            var result = new Dictionary<string, object>();
            foreach (var key in godotData.Keys)
            {
                result[key.ToString()] = ConvertValueFromGodot(godotData[key]);
            }
            return result;
        }
        
        /// <summary>
        /// 转换值为 Godot 兼容类型
        /// </summary>
        private object ConvertValueToGodot(object value)
        {
            if (value is Dictionary<string, object> dict)
            {
                return ConvertToGodotDictionary(dict);
            }
            if (value is List<Dictionary<string, object>> list)
            {
                var godotArray = new Godot.Collections.Array();
                foreach (var item in list)
                {
                    godotArray.Add(ConvertToGodotDictionary(item));
                }
                return godotArray;
            }
            return value;
        }
        
        /// <summary>
        /// 从 Godot 类型转换回 .NET 类型
        /// </summary>
        private object ConvertValueFromGodot(object value)
        {
            if (value is Dictionary godotDict)
            {
                return ConvertFromGodotDictionary(godotDict);
            }
            if (value is Godot.Collections.Array godotArray)
            {
                var list = new List<Dictionary<string, object>>();
                foreach (var item in godotArray)
                {
                    if (item is Dictionary itemDict)
                    {
                        list.Add(ConvertFromGodotDictionary(itemDict));
                    }
                }
                return list;
            }
            return value;
        }
    }
}
