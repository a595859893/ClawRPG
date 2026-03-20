using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Data;
using ClawRPG.Scripts.Database.Loaders;
using ClawRPG.Scripts.Framework;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// Achievement database - stores all achievement templates
    /// </summary>
    public class AchievementDatabase : BaseSystem, IDatabase
    {
        private Dictionary<string, Achievement> _achievements;

        /// <summary>
        /// 静态实例引用（兼容原有访问模式）
        /// </summary>
        public static AchievementDatabase Instance { get; private set; }

        public AchievementDatabase()
        {
            Instance = this;
            _achievements = new Dictionary<string, Achievement>();
        }

        protected override void Initialize()
        {
            base.Initialize();
            LoadAchievements();
        }

        private void LoadAchievements()
        {
            // 尝试从配置加载成就数据
            var configPath = "res://Resources/Config/achievements_config.json";
            var loader = AchievementConfigLoader.Instance;

            if (loader.Load(configPath))
            {
                var achievementsList = loader.GetAllAchievements();
                foreach (var achievement in achievementsList)
                {
                    AddAchievement(achievement);
                }
                GD.Print($"[AchievementDatabase] 从配置文件加载了 {_achievements.Count} 个成就");
            }
            else
            {
                GD.PrintErr($"[AchievementDatabase] 加载成就配置失败: {loader.LastError}");
            }
        }

        private void AddAchievement(Achievement achievement)
        {
            _achievements[achievement.Id] = achievement;
            _dataStore[achievement.Id] = achievement;
        }

        /// <summary>
        /// 通过 ID 获取成就
        /// </summary>
        public Achievement GetAchievement(string id)
        {
            return _achievements.ContainsKey(id) ? _achievements[id] : null;
        }

        /// <summary>
        /// 获取所有成就
        /// </summary>
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

        /// <summary>
        /// 验证数据完整性
        /// </summary>
        public bool ValidateData()
        {
            return _achievements != null && _achievements.Count > 0;
        }

        // IDatabase implementation
        public object Instance => AchievementDatabase.Instance;

        public new T GetData<T>(string key) where T : class
        {
            if (_achievements.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return null;
        }

        public new int GetDataCount() => _achievements.Count;

        public new IEnumerable<string> GetAllKeys() => _achievements.Keys;

        // BaseSystem overrides for save/load
        public override Dictionary ExportSaveData()
        {
            var saveData = new Dictionary();
            ExportSaveDataInternal(saveData);
            return saveData;
        }

        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            ImportSaveDataInternal(data);
        }

        private void ExportSaveDataInternal(Godot.Collections.Dictionary saveData)
        {
            var achievementsArray = new Godot.Collections.Array();
            foreach (var kvp in _achievements)
            {
                var ach = kvp.Value;
                achievementsArray.Add(new Godot.Collections.Dictionary
                {
                    ["id"] = ach.Id,
                    ["name"] = ach.Name,
                    ["description"] = ach.Description,
                    ["type"] = (int)ach.Type,
                    ["difficulty"] = (int)ach.Difficulty,
                    ["requiredValue"] = ach.RequiredValue,
                    ["rewardGold"] = ach.RewardGold,
                    ["rewardExp"] = ach.RewardExp
                });
            }
            saveData["achievements"] = achievementsArray;
        }

        private void ImportSaveDataInternal(Godot.Collections.Dictionary saveData)
        {
            if (!saveData.ContainsKey("achievements"))
                return;

            var achievementsArray = (Godot.Collections.Array)saveData["achievements"];
            foreach (Godot.Collections.Dictionary achDict in achievementsArray)
            {
                var achievement = new Achievement
                {
                    Id = (string)achDict["id"],
                    Name = (string)achDict["name"],
                    Description = (string)achDict["description"],
                    Type = (AchievementType)(int)achDict["type"],
                    Difficulty = (AchievementDifficulty)(int)achDict["difficulty"],
                    RequiredValue = (int)achDict["requiredValue"],
                    RewardGold = (int)achDict["rewardGold"],
                    RewardExp = (int)achDict["rewardExp"]
                };
                _achievements[achievement.Id] = achievement;
            }
        }
    }
}
