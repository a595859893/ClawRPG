using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Achievement
{
    /// <summary>
    /// 成就进度管理 - 追踪和管理成就进度
    /// </summary>
    public partial class AchievementProgress : BaseSystem
    {
        /// <summary>
        /// 进度数据
        /// </summary>
        public class ProgressData
        {
            public string AchievementId { get; set; }
            public int CurrentValue { get; set; }
            public int TargetValue { get; set; }
            public bool IsUnlocked { get; set; }
            public int UnlockTime { get; set; }
        }
        
        private Dictionary<string, ProgressData> _progressData = new Dictionary<string, ProgressData>();
        private AchievementSystem _achievementSystem;
        
        public override void _Ready()
        {
            base._Ready();
            InitializeProgressData();
        }
        
        /// <summary>
        /// 设置成就系统引用
        /// </summary>
        public void SetAchievementSystem(AchievementSystem system)
        {
            _achievementSystem = system;
        }
        
        /// <summary>
        /// 初始化进度数据
        /// </summary>
        private void InitializeProgressData()
        {
            // 击杀相关
            _progressData["first_blood"] = new ProgressData { TargetValue = 1 };
            _progressData["killer_novice"] = new ProgressData { TargetValue = 100 };
            _progressData["killer_master"] = new ProgressData { TargetValue = 1000 };
            _progressData["killer_legend"] = new ProgressData { TargetValue = 10000 };
            
            // Boss击杀
            _progressData["boss_slayer"] = new ProgressData { TargetValue = 10 };
            _progressData["boss_legend"] = new ProgressData { TargetValue = 100 };
            
            // PvP
            _progressData["pvp_novice"] = new ProgressData { TargetValue = 10 };
            _progressData["pvp_champion"] = new ProgressData { TargetValue = 100 };
            
            // 探索
            _progressData["explorer_novice"] = new ProgressData { TargetValue = 5 };
            _progressData["explorer_master"] = new ProgressData { TargetValue = 20 };
            _progressData["explorer_legend"] = new ProgressData { TargetValue = 50 };
            
            // 爬塔
            _progressData["tower_climber"] = new ProgressData { TargetValue = 50 };
            _progressData["tower_master"] = new ProgressData { TargetValue = 100 };
            
            // 收集
            _progressData["pet_collector_novice"] = new ProgressData { TargetValue = 5 };
            _progressData["pet_collector_master"] = new ProgressData { TargetValue = 20 };
            _progressData["pet_collector_legend"] = new ProgressData { TargetValue = 50 };
            
            // 社交
            _progressData["social_novice"] = new ProgressData { TargetValue = 10 };
            _progressData["social_person"] = new ProgressData { TargetValue = 50 };
            _progressData["social_butterfly"] = new ProgressData { TargetValue = 100 };
            
            // 经济
            _progressData["shopaholic_novice"] = new ProgressData { TargetValue = 10000 };
            _progressData["shopaholic_master"] = new ProgressData { TargetValue = 100000 };
            _progressData["shopaholic_legend"] = new ProgressData { TargetValue = 1000000 };
        }
        
        /// <summary>
        /// 更新进度
        /// </summary>
        public void UpdateProgress(string achievementId, int newValue)
        {
            if (!_progressData.ContainsKey(achievementId))
            {
                _progressData[achievementId] = new ProgressData
                {
                    AchievementId = achievementId,
                    TargetValue = 100  // 默认目标
                };
            }
            
            var data = _progressData[achievementId];
            data.CurrentValue = newValue;
            
            // 检查是否解锁
            if (!data.IsUnlocked && newValue >= data.TargetValue)
            {
                data.IsUnlocked = true;
                data.UnlockTime = OS.GetUnixTime();
                
                // 通知成就系统
                GD.Print($"[AchievementProgress] Achievement unlocked: {achievementId}");
            }
        }
        
        /// <summary>
        /// 获取进度百分比
        /// </summary>
        public float GetProgressPercent(string achievementId)
        {
            if (!_progressData.ContainsKey(achievementId))
                return 0f;
            
            var data = _progressData[achievementId];
            if (data.TargetValue == 0)
                return 0f;
            
            return (float)data.CurrentValue / data.TargetValue * 100f;
        }
        
        /// <summary>
        /// 获取进度数据
        /// </summary>
        public ProgressData GetProgressData(string achievementId)
        {
            return _progressData.ContainsKey(achievementId) ? _progressData[achievementId] : null;
        }
        
        /// <summary>
        /// 获取所有进度数据
        /// </summary>
        public Dictionary<string, ProgressData> GetAllProgressData()
        {
            return new Dictionary<string, ProgressData>(_progressData);
        }
        
        /// <summary>
        /// 检查是否已解锁
        /// </summary>
        public bool IsUnlocked(string achievementId)
        {
            return _progressData.ContainsKey(achievementId) && _progressData[achievementId].IsUnlocked;
        }
        
        /// <summary>
        /// 获取已解锁成就数量
        /// </summary>
        public int GetUnlockedCount()
        {
            int count = 0;
            foreach (var data in _progressData.Values)
            {
                if (data.IsUnlocked)
                    count++;
            }
            return count;
        }
        
        /// <summary>
        /// 获取总成就数量
        /// </summary>
        public int GetTotalCount()
        {
            return _progressData.Count;
        }
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            foreach (var kvp in _progressData)
            {
                data[kvp.Key] = new Dictionary
                {
                    { "current", kvp.Value.CurrentValue },
                    { "unlocked", kvp.Value.IsUnlocked },
                    { "unlockTime", kvp.Value.UnlockTime }
                };
            }
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            foreach (string key in data.Keys)
            {
                var progressDict = data[key] as Dictionary;
                if (progressDict == null) continue;
                
                if (!_progressData.ContainsKey(key))
                {
                    _progressData[key] = new ProgressData { AchievementId = key };
                }
                
                _progressData[key].CurrentValue = (int)progressDict["current"];
                _progressData[key].IsUnlocked = (bool)progressDict["unlocked"];
                _progressData[key].UnlockTime = (int)progressDict["unlockTime"];
            }
        }
    }
}
