using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.DynamicQuestChallenge
{
    /// <summary>
    /// DynamicQuestChallengeSystem - 动态任务挑战系统
    /// 基于玩家状态动态生成个性化挑战任务
    /// </summary>
    public partial class DynamicQuestChallengeSystem : BaseSystem
    {
        /// <summary>
        /// Challenge data container
        /// </summary>
        private DynamicQuestChallengeData _data;

        /// <summary>
        /// Challenge template database
        /// </summary>
        private DynamicQuestChallengeDatabase _database;

        public override void _Ready()
        {
            base._Ready();
            _data = new DynamicQuestChallengeData();
            _database = new DynamicQuestChallengeDatabase();
            LoadData();
            GD.Print($"[DynamicQuestChallengeSystem] Initialized");
        }

        public override void _ExitTree()
        {
            SaveData();
            Shutdown();
        }

        protected override void Initialize()
        {
            base.Initialize();
            IsInitialized = true;
        }

        /// <summary>
        /// Shutdown the system and save data
        /// </summary>
        public void Shutdown()
        {
            SaveData();
            base.Shutdown();
        }

        /// <summary>
        /// Export save data
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            if (_data != null)
            {
                return new Dictionary
                {
                    { "data", _data.ToDict() },
                    { "system_name", SystemName }
                };
            }
            return new Dictionary
            {
                { "data", new Dictionary<string, object>() },
                { "system_name", SystemName }
            };
        }

        /// <summary>
        /// Import save data
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            base.ImportSaveData(data);
            if (data.Contains("data") && data["data"] is Dictionary dictData)
            {
                _data.FromDict(dictData);
            }
        }

        /// <summary>
        /// Generate a new challenge for the player
        /// </summary>
        public Dictionary GenerateChallenge(int playerLevel, string playerClass, List<object> currentQuests)
        {
            var challengeTypes = _database.GetChallengeTypes();
            var random = new Random();
            var selectedType = challengeTypes[random.Next(challengeTypes.Count)];

            var difficulty = CalculateDifficulty(playerLevel);
            var challenge = _database.GenerateChallenge(selectedType, difficulty, playerLevel, playerClass);

            // Add to active challenges
            _data.ActiveChallenges.Add(challenge);

            // Generate unique ID
            challenge["id"] = GenerateUniqueId();
            challenge["generated_time"] = Time.GetUnixTimeFromSystem();
            challenge["expires_time"] = (double)challenge["generated_time"] + (int)challenge["duration"];
            challenge["progress"] = 0;
            challenge["completed"] = false;

            SaveData();
            return challenge;
        }

        /// <summary>
        /// Calculate difficulty based on player level
        /// </summary>
        private string CalculateDifficulty(int playerLevel)
        {
            var random = new Random();
            var roll = random.Next(100);

            if (playerLevel < 10)
            {
                return roll < 70 ? "Easy" : "Medium";
            }
            else if (playerLevel < 30)
            {
                if (roll < 30) return "Easy";
                if (roll < 70) return "Medium";
                return "Hard";
            }
            else if (playerLevel < 50)
            {
                if (roll < 40) return "Medium";
                if (roll < 80) return "Hard";
                return "Epic";
            }
            else
            {
                if (roll < 30) return "Hard";
                if (roll < 70) return "Epic";
                return "Legendary";
            }
        }

        /// <summary>
        /// Update challenge progress
        /// </summary>
        public Dictionary UpdateProgress(string challengeId, int progressDelta)
        {
            foreach (Dictionary challenge in _data.ActiveChallenges)
            {
                if ((string)challenge["id"] == challengeId)
                {
                    challenge["progress"] = (int)challenge["progress"] + progressDelta;

                    // Check completion
                    if ((int)challenge["progress"] >= (int)challenge["target_amount"])
                    {
                        challenge["completed"] = true;
                        challenge["completion_time"] = Time.GetUnixTimeFromSystem();
                        _data.CompletedChallenges.Add(challenge);
                        _data.ActiveChallenges.Remove(challenge);

                        // Update statistics
                        _data.Statistics["total_completed"] = (int)_data.Statistics["total_completed"] + 1;
                        _data.Statistics["current_streak"] = (int)_data.Statistics["current_streak"] + 1;

                        var currentStreak = (int)_data.Statistics["current_streak"];
                        var longestStreak = (int)_data.Statistics["longest_streak"];
                        if (currentStreak > longestStreak)
                        {
                            _data.Statistics["longest_streak"] = currentStreak;
                        }
                    }

                    SaveData();
                    return challenge;
                }
            }

            return new Dictionary<string, object>();
        }

        /// <summary>
        /// Abandon a challenge
        /// </summary>
        public bool AbandonChallenge(string challengeId)
        {
            foreach (Dictionary challenge in _data.ActiveChallenges)
            {
                if ((string)challenge["id"] == challengeId)
                {
                    _data.ActiveChallenges.Remove(challenge);
                    _data.Statistics["total_abandoned"] = (int)_data.Statistics["total_abandoned"] + 1;
                    _data.Statistics["current_streak"] = 0;
                    SaveData();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get active challenges
        /// </summary>
        public List<object> GetActiveChallenges()
        {
            return _data.ActiveChallenges;
        }

        /// <summary>
        /// Get completed challenges
        /// </summary>
        public List<object> GetCompletedChallenges()
        {
            return _data.CompletedChallenges;
        }

        /// <summary>
        /// Get statistics
        /// </summary>
        public Dictionary<string, object> GetStatistics()
        {
            return _data.Statistics;
        }

        /// <summary>
        /// Check and expire old challenges
        /// </summary>
        public void CheckExpired()
        {
            var currentTime = Time.GetUnixTimeFromSystem();
            var expired = new List<string>();

            foreach (Dictionary challenge in _data.ActiveChallenges)
            {
                if (currentTime > (double)challenge["expires_time"])
                {
                    expired.Add((string)challenge["id"]);
                }
            }

            foreach (var challengeId in expired)
            {
                AbandonChallenge(challengeId);
            }
        }

        /// <summary>
        /// Generate a unique challenge ID
        /// </summary>
        private string GenerateUniqueId()
        {
            var random = new Random();
            return $"challenge_{Time.GetUnixTimeFromSystem()}_{random.Next(10000)}";
        }

        /// <summary>
        /// Save data to file
        /// </summary>
        public void SaveData()
        {
            var savePath = "user://dynamic_quest_challenge.save";
            using (var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write))
            {
                if (file != null)
                {
                    var jsonString = Json.Stringify(_data.ToDict());
                    file.StoreString(jsonString);
                    file.Close();
                }
            }
        }

        /// <summary>
        /// Load data from file
        /// </summary>
        private void LoadData()
        {
            var savePath = "user://dynamic_quest_challenge.save";
            if (FileAccess.FileExists(savePath))
            {
                using (var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Read))
                {
                    if (file != null)
                    {
                        var jsonString = file.GetAsText();
                        file.Close();
                        var json = new Json();
                        var parseResult = json.Parse(jsonString);
                        if (parseResult == Error.Ok)
                        {
                            _data.FromDict(json.Data.AsGodotDictionary());
                        }
                    }
                }
            }
        }
    }
}
