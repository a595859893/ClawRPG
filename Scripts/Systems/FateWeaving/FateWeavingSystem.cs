using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.FateWeaving {
    public class FateWeavingSystem : BaseSystem {
        private static FateWeavingSystem _instance;
        public static FateWeavingSystem Instance {
            get {
                if (_instance == null) {
                    _instance = new FateWeavingSystem();
                }
                return _instance;
            }
        }

        public FateWeavingData Data { get; private set; } = new FateWeavingData();
        public FateWeavingStatistics Statistics { get; private set; } = new FateWeavingStatistics();
        
        private FateWeavingDatabase _database;
        
        public event Action<FateChoice> OnChoiceMade;
        public event Action<FatePathType> OnPathAffinityChanged;
        public event Action<int> OnWeaveLevelChanged;

        public override void _Ready() {
            _instance = this;
            _database = FateWeavingDatabase.Instance;
            LoadData();
        }

        public void LoadData() {
            var saveSystem = GetNode("/root/SaveSystem") as ClawRPG.Scripts.Systems.SaveSystem;
            if (saveSystem != null) {
                // Try to load saved data
                // For now, initialize with defaults
            }
            UpdateDominantPath();
        }

        public void MakeChoice(FateChoice choice) {
            if (choice == null || Data.MadeChoices.Contains(choice.Id)) return;
            
            Data.MadeChoices.Add(choice.Id);
            Data.TotalWeaves++;
            
            // Apply path influence
            foreach (var influence in choice.PathInfluence) {
                if (Data.PathAffinity.ContainsKey(influence.Key)) {
                    Data.PathAffinity[influence.Key] += influence.Value;
                }
            }
            
            // Apply stat bonuses
            foreach (var stat in choice.StatBonuses) {
                if (Data.PlayerStats.ContainsKey(stat.Key)) {
                    Data.PlayerStats[stat.Key] += stat.Value;
                }
            }
            
            // Update choice type count
            var choiceTypeName = choice.ChoiceType.ToString();
            if (Data.ChoiceTypeCount.ContainsKey(choiceTypeName)) {
                Data.ChoiceTypeCount[choiceTypeName]++;
            } else {
                Data.ChoiceTypeCount[choiceTypeName] = 1;
            }
            
            // Update statistics
            UpdateStatistics(choice);
            
            // Check for level up
            CheckWeaveLevelUp();
            
            // Update dominant path
            UpdateDominantPath();
            
            // Apply path bonuses
            ApplyPathBonuses();
            
            OnChoiceMade?.Invoke(choice);
            
            SaveData();
        }

        private void UpdateStatistics(FateChoice choice) {
            Statistics.TotalChoicesMade++;
            
            switch (choice.ChoiceType) {
                case FateChoiceType.Moral:
                    Statistics.MoralChoices++;
                    break;
                case FateChoiceType.Combat:
                    Statistics.CombatChoices++;
                    break;
                case FateChoiceType.Social:
                    Statistics.SocialChoices++;
                    break;
                case FateChoiceType.Economic:
                    Statistics.EconomicChoices++;
                    break;
                case FateChoiceType.Exploration:
                    Statistics.ExplorationChoices++;
                    break;
                case FateChoiceType.Mystery:
                    Statistics.MysteryChoices++;
                    break;
            }
            
            foreach (var influence in choice.PathInfluence) {
                if (Statistics.PathChoiceCount.ContainsKey(influence.Key)) {
                    Statistics.PathChoiceCount[influence.Key]++;
                } else {
                    Statistics.PathChoiceCount[influence.Key] = 1;
                }
            }
            
            // Update highest affinity
            float highest = 0f;
            foreach (var affinity in Data.PathAffinity) {
                if (affinity.Value > highest) {
                    highest = affinity.Value;
                }
            }
            Statistics.HighestPathAffinity = highest;
        }

        private void CheckWeaveLevelUp() {
            int oldLevel = Data.WeaveLevel;
            int newLevel = CalculateWeaveLevel();
            
            if (newLevel > oldLevel) {
                Data.WeaveLevel = newLevel;
                OnWeaveLevelChanged?.Invoke(newLevel);
            }
        }

        private int CalculateWeaveLevel() {
            return Math.Min(20, 1 + Data.TotalWeaves / 5);
        }

        private void UpdateDominantPath() {
            float highestAffinity = 0f;
            FatePathType dominant = FatePathType.Hero;
            
            foreach (var affinity in Data.PathAffinity) {
                if (affinity.Value > highestAffinity) {
                    highestAffinity = affinity.Value;
                    dominant = affinity.Key;
                }
            }
            
            if (Data.DominantPath != dominant) {
                Data.DominantPath = dominant;
                OnPathAffinityChanged?.Invoke(dominant);
            }
        }

        private void ApplyPathBonuses() {
            var dominantPathData = _database.GetPath(Data.DominantPath);
            if (dominantPathData != null) {
                foreach (var bonus in dominantPathData.PathBonuses) {
                    GD.Print($"[FateWeaving] Path bonus applied: {bonus.Key} = {bonus.Value}");
                }
            }
        }

        public FateChoice GetRandomChoice() {
            return _database.GetRandomChoice(Data.WeaveLevel);
        }

        public List<FateChoice> GetAvailableChoices() {
            return _database.GetAvailableChoices(Data.WeaveLevel);
        }

        public FatePathData GetDominantPathData() {
            return _database.GetPath(Data.DominantPath);
        }

        public Dictionary<FatePathType, float> GetAllPathAffinities() {
            return new Dictionary<FatePathType, float>(Data.PathAffinity);
        }

        public float GetPathAffinity(FatePathType path) {
            return Data.PathAffinity.ContainsKey(path) ? Data.PathAffinity[path] : 0f;
        }

        public float GetStatBonus(string statName) {
            return Data.PlayerStats.ContainsKey(statName) ? Data.PlayerStats[statName] : 0f;
        }

        public float GetTotalStatBonus() {
            float total = 0f;
            foreach (var stat in Data.PlayerStats.Values) {
                total += stat;
            }
            return total;
        }

        public Dictionary<string, float> GetAllStatBonuses() {
            return new Dictionary<string, float>(Data.PlayerStats);
        }

        public bool HasChosen(string choiceId) {
            return Data.MadeChoices.Contains(choiceId);
        }

        public int GetChoiceCount() {
            return Data.MadeChoices.Count;
        }

        public int GetWeaveLevel() {
            return Data.WeaveLevel;
        }

        public float GetExperienceProgress() {
            int expNeeded = Data.WeaveLevel * 5;
            return (float)(Data.TotalWeaves % 5) / expNeeded;
        }

        public void ResetProgress() {
            Data = new FateWeavingData();
            Statistics = new FateWeavingStatistics();
            SaveData();
        }

        private void SaveData() {
            var saveSystem = GetNode("/root/SaveSystem") as ClawRPG.Scripts.Systems.SaveSystem;
            if (saveSystem != null) {
                // Save fate weaving data
                var saveData = new Godot.Dictionary();
                // Serialization would go here
            }
        }

        public Dictionary<string, object> GetSaveData() {
            var saveData = new Godot.Dictionary();
            
            var pathAffinity = new Godot.Dictionary();
            foreach (var kvp in Data.PathAffinity) {
                pathAffinity[kvp.Key.ToString()] = kvp.Value;
            }
            saveData["path_affinity"] = pathAffinity;
            
            var playerStats = new Godot.Dictionary();
            foreach (var kvp in Data.PlayerStats) {
                playerStats[kvp.Key] = kvp.Value;
            }
            saveData["player_stats"] = playerStats;
            
            saveData["made_choices"] = new Godot.Array(Data.MadeChoices);
            saveData["dominant_path"] = Data.DominantPath.ToString();
            saveData["weave_level"] = Data.WeaveLevel;
            saveData["total_weaves"] = Data.TotalWeaves;
            
            var choiceTypeCount = new Godot.Dictionary();
            foreach (var kvp in Data.ChoiceTypeCount) {
                choiceTypeCount[kvp.Key] = kvp.Value;
            }
            saveData["choice_type_count"] = choiceTypeCount;
            
            return saveData;
        }

        public void LoadFromData(Dictionary<string, object> saveData) {
            if (saveData == null) return;
            
            if (saveData.Contains("path_affinity")) {
                var pathAffinity = saveData["path_affinity"] as Godot.Dictionary;
                foreach (var key in pathAffinity.Keys) {
                    if (Enum.TryParse<FatePathType>(key.ToString(), out var pathType)) {
                        Data.PathAffinity[pathType] = Convert.ToSingle(pathAffinity[key]);
                    }
                }
            }
            
            if (saveData.Contains("player_stats")) {
                var playerStats = saveData["player_stats"] as Godot.Dictionary;
                foreach (var key in playerStats.Keys) {
                    Data.PlayerStats[key.ToString()] = Convert.ToSingle(playerStats[key]);
                }
            }
            
            if (saveData.Contains("made_choices")) {
                var madeChoices = saveData["made_choices"] as Godot.Array;
                Data.MadeChoices.Clear();
                foreach (var choice in madeChoices) {
                    Data.MadeChoices.Add(choice.ToString());
                }
            }
            
            if (saveData.Contains("dominant_path")) {
                if (Enum.TryParse<FatePathType>(saveData["dominant_path"].ToString(), out var path)) {
                    Data.DominantPath = path;
                }
            }
            
            if (saveData.Contains("weave_level")) {
                Data.WeaveLevel = Convert.ToInt32(saveData["weave_level"]);
            }
            
            if (saveData.Contains("total_weaves")) {
                Data.TotalWeaves = Convert.ToInt32(saveData["total_weaves"]);
            }
            
            if (saveData.Contains("choice_type_count")) {
                var choiceTypeCount = saveData["choice_type_count"] as Godot.Dictionary;
                Data.ChoiceTypeCount.Clear();
                foreach (var key in choiceTypeCount.Keys) {
                    Data.ChoiceTypeCount[key.ToString()] = Convert.ToInt32(choiceTypeCount[key]);
                }
            }
        }
        
        /// <summary>
        /// 导出保存数据 (BaseSystem 接口)
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            // 保存玩家属性
            var playerStats = new Godot.Collections.Dictionary();
            foreach (var kvp in Data.PlayerStats)
            {
                playerStats[kvp.Key] = kvp.Value;
            }
            data["player_stats"] = playerStats;
            
            // 保存已做选择
            var madeChoices = new Godot.Collections.Array();
            foreach (var choice in Data.MadeChoices)
            {
                madeChoices.Add(choice);
            }
            data["made_choices"] = madeChoices;
            
            data["dominant_path"] = Data.DominantPath.ToString();
            data["weave_level"] = Data.WeaveLevel;
            data["total_weaves"] = Data.TotalWeaves;
            
            // 保存选择类型计数
            var choiceTypeCount = new Godot.Collections.Dictionary();
            foreach (var kvp in Data.ChoiceTypeCount)
            {
                choiceTypeCount[kvp.Key] = kvp.Value;
            }
            data["choice_type_count"] = choiceTypeCount;
            
            return data;
        }
        
        /// <summary>
        /// 导入保存数据 (BaseSystem 接口)
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            if (data.Contains("player_stats")) {
                var playerStats = data["player_stats"] as Godot.Collections.Dictionary;
                Data.PlayerStats.Clear();
                foreach (var key in playerStats.Keys) {
                    Data.PlayerStats[key.ToString()] = Convert.ToSingle(playerStats[key]);
                }
            }
            
            if (data.Contains("made_choices")) {
                var madeChoices = data["made_choices"] as Godot.Array;
                Data.MadeChoices.Clear();
                foreach (var choice in madeChoices) {
                    Data.MadeChoices.Add(choice.ToString());
                }
            }
            
            if (data.Contains("dominant_path")) {
                if (Enum.TryParse<FatePathType>(data["dominant_path"].ToString(), out var path)) {
                    Data.DominantPath = path;
                }
            }
            
            if (data.Contains("weave_level")) {
                Data.WeaveLevel = Convert.ToInt32(data["weave_level"]);
            }
            
            if (data.Contains("total_weaves")) {
                Data.TotalWeaves = Convert.ToInt32(data["total_weaves"]);
            }
            
            if (data.Contains("choice_type_count")) {
                var choiceTypeCount = data["choice_type_count"] as Godot.Collections.Dictionary;
                Data.ChoiceTypeCount.Clear();
                foreach (var key in choiceTypeCount.Keys) {
                    Data.ChoiceTypeCount[key.ToString()] = Convert.ToInt32(choiceTypeCount[key]);
                }
            }
            
            GD.Print($"[FateWeaving] Imported: {Data.MadeChoices.Count} choices, level {Data.WeaveLevel}");
        }
    }
}
