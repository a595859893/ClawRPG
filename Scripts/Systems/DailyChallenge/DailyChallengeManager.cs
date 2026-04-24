using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;
using ClawRPG.Scripts.Characters;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Daily challenge manager - handles challenge generation, tracking, and rewards
    /// </summary>
    public class DailyChallengeManager {
        private static DailyChallengeManager _instance;
        public static DailyChallengeManager Instance {
            get {
                if (_instance == null) {
                    _instance = new DailyChallengeManager();
                }
                return _instance;
            }
        }
        
        // Current day's challenges
        private List<DailyChallenge> _dailyChallenges = new();
        
        // Statistics tracking
        private int _totalKills;
        private int _totalDamageDealt;
        private int _totalGoldEarned;
        private int _totalSkillsUsed;
        private int _totalQuestsCompleted;
        private int _regionsExplored;
        private HashSet<string> _exploredRegionIds = new();
        private int _survivalTimeSeconds;
        private int _autoSaveTimer;
        private const int AutoSaveInterval = 60; // Save every 60 seconds
        
        // New challenge tracking
        private int _totalFishCaught;
        private int _totalAlchemyCrafted;
        private int _totalMountKills;
        private int _totalMountSkillsUsed;
        private int _totalPetKills;
        private int _totalItemsSold;
        private int _totalItemsCrafted;
        private int _totalReputationGained;
        private int _totalBossesKilled;
        private int _totalCriticalHits;
        private int _totalDodges;
        private int _totalHealed;
        
        // Events
        public static string ChallengeCompletedSignal = "challenge_completed";
        public static string ChallengeUpdatedSignal = "challenge_updated";
        public static string AllChallengesCompletedSignal = "all_challenges_completed";
        
        private Player _player;
        
        public DailyChallengeManager() {
            _instance = this;
        }
        
        public void Initialize(Player player) {
            _player = player;
            LoadDailyChallenges();
            StartSurvivalTimer();
        }
        
        public void LoadDailyChallenges() {
            var today = DateTime.Today;
            var saveKey = $"daily_challenges_{today:yyyyMMdd}";
            
            // Check if we already have today's challenges
            var saved = GetStoredChallenges(saveKey);
            if (saved != null && saved.Count > 0) {
                // Check if expired
                if (saved[0].IsExpired()) {
                    // Generate new challenges
                    _dailyChallenges = DailyChallengeDatabase.Instance.GetRandomChallenges(3);
                    SaveDailyChallenges(saveKey);
                } else {
                    _dailyChallenges = saved;
                }
            } else {
                // Generate new challenges
                _dailyChallenges = DailyChallengeDatabase.Instance.GetRandomChallenges(3);
                SaveDailyChallenges(saveKey);
            }
            
            GD.Print($"[DailyChallengeManager] Loaded {_dailyChallenges.Count} daily challenges");
        }
        
        private List<DailyChallenge> GetStoredChallenges(string key) {
            try {
                var file = new File();
                var path = $"user://{key}.json";
                
                if (!file.FileExists(path)) {
                    return null;
                }
                
                file.Open(path, File.ModeFlags.Read);
                var json = file.GetAsText();
                file.Close();
                
                if (string.IsNullOrEmpty(json)) {
                    return null;
                }
                
                var result = JSON.Parse(json);
                if (result.Error != Error.Ok) {
                    GD.PrintErr($"[DailyChallengeManager] Failed to parse stored challenges: {result.ErrorString}");
                    return null;
                }
                
                var data = result.Result as Dictionary;
                if (data == null) {
                    return null;
                }
                
                var challenges = new List<DailyChallenge>();
                
                if (data.ContainsKey("challenges")) {
                    var challengeList = (Array)data["challenges"];
                    foreach (var item in challengeList) {
                        var challengeDict = item as Dictionary;
                        if (challengeDict != null) {
                            var challenge = new DailyChallenge {
                                Id = challengeDict.GetValueOrDefault("Id", "").ToString(),
                                Name = challengeDict.GetValueOrDefault("Name", "").ToString(),
                                Description = challengeDict.GetValueOrDefault("Description", "").ToString(),
                                Type = (ChallengeType)(int)challengeDict.GetValueOrDefault("Type", 0),
                                Difficulty = (ChallengeDifficulty)(int)challengeDict.GetValueOrDefault("Difficulty", 0),
                                TargetCount = (int)challengeDict.GetValueOrDefault("TargetCount", 0),
                                CurrentProgress = (int)challengeDict.GetValueOrDefault("CurrentProgress", 0),
                                IsCompleted = (bool)challengeDict.GetValueOrDefault("IsCompleted", false),
                                GoldReward = (int)challengeDict.GetValueOrDefault("GoldReward", 0),
                                ExpReward = (int)challengeDict.GetValueOrDefault("ExpReward", 0),
                                ExpireTime = DateTime.Parse(challengeDict.GetValueOrDefault("ExpireTime", DateTime.Now.ToString()).ToString())
                            };
                            
                            // Load item rewards
                            if (challengeDict.ContainsKey("ItemRewardIds")) {
                                var itemRewards = (Array)challengeDict["ItemRewardIds"];
                                challenge.ItemRewardIds = new List<int>();
                                foreach (var itemId in itemRewards) {
                                    challenge.ItemRewardIds.Add(Convert.ToInt32(itemId));
                                }
                            }
                            
                            challenges.Add(challenge);
                        }
                    }
                }
                
                // Restore statistics
                if (data.ContainsKey("totalKills")) _totalKills = (int)data["totalKills"];
                if (data.ContainsKey("totalDamageDealt")) _totalDamageDealt = (int)data["totalDamageDealt"];
                if (data.ContainsKey("totalGoldEarned")) _totalGoldEarned = (int)data["totalGoldEarned"];
                if (data.ContainsKey("totalSkillsUsed")) _totalSkillsUsed = (int)data["totalSkillsUsed"];
                if (data.ContainsKey("totalQuestsCompleted")) _totalQuestsCompleted = (int)data["totalQuestsCompleted"];
                if (data.ContainsKey("regionsExplored")) _regionsExplored = (int)data["regionsExplored"];
                if (data.ContainsKey("survivalTimeSeconds")) _survivalTimeSeconds = (int)data["survivalTimeSeconds"];
                
                // Restore explored region IDs
                if (data.ContainsKey("exploredRegionIds")) {
                    var regionIds = (Array)data["exploredRegionIds"];
                    _exploredRegionIds = new HashSet<string>();
                    foreach (var regionId in regionIds) {
                        _exploredRegionIds.Add(regionId.ToString());
                    }
                }
                
                // Restore new challenge tracking stats
                if (data.ContainsKey("totalFishCaught")) _totalFishCaught = (int)data["totalFishCaught"];
                if (data.ContainsKey("totalAlchemyCrafted")) _totalAlchemyCrafted = (int)data["totalAlchemyCrafted"];
                if (data.ContainsKey("totalMountKills")) _totalMountKills = (int)data["totalMountKills"];
                if (data.ContainsKey("totalMountSkillsUsed")) _totalMountSkillsUsed = (int)data["totalMountSkillsUsed"];
                if (data.ContainsKey("totalPetKills")) _totalPetKills = (int)data["totalPetKills"];
                if (data.ContainsKey("totalItemsSold")) _totalItemsSold = (int)data["totalItemsSold"];
                if (data.ContainsKey("totalItemsCrafted")) _totalItemsCrafted = (int)data["totalItemsCrafted"];
                if (data.ContainsKey("totalReputationGained")) _totalReputationGained = (int)data["totalReputationGained"];
                if (data.ContainsKey("totalBossesKilled")) _totalBossesKilled = (int)data["totalBossesKilled"];
                if (data.ContainsKey("totalCriticalHits")) _totalCriticalHits = (int)data["totalCriticalHits"];
                if (data.ContainsKey("totalDodges")) _totalDodges = (int)data["totalDodges"];
                if (data.ContainsKey("totalHealed")) _totalHealed = (int)data["totalHealed"];
                
                GD.Print($"[DailyChallengeManager] Loaded {challenges.Count} challenges from storage");
                return challenges;
            }
            catch (Exception e) {
                GD.PrintErr($"[DailyChallengeManager] Failed to load stored challenges: {e.Message}");
                return null;
            }
        }
        
        private void SaveDailyChallenges(string key) {
            try {
                var file = new File();
                var path = $"user://{key}.json";
                
                var data = new Dictionary {
                    ["challenges"] = BuildChallengeArray(),
                    ["totalKills"] = _totalKills,
                    ["totalDamageDealt"] = _totalDamageDealt,
                    ["totalGoldEarned"] = _totalGoldEarned,
                    ["totalSkillsUsed"] = _totalSkillsUsed,
                    ["totalQuestsCompleted"] = _totalQuestsCompleted,
                    ["regionsExplored"] = _regionsExplored,
                    ["survivalTimeSeconds"] = _survivalTimeSeconds,
                    ["exploredRegionIds"] = new Godot.Collections.Array(_exploredRegionIds),
                    ["totalFishCaught"] = _totalFishCaught,
                    ["totalAlchemyCrafted"] = _totalAlchemyCrafted,
                    ["totalMountKills"] = _totalMountKills,
                    ["totalMountSkillsUsed"] = _totalMountSkillsUsed,
                    ["totalPetKills"] = _totalPetKills,
                    ["totalItemsSold"] = _totalItemsSold,
                    ["totalItemsCrafted"] = _totalItemsCrafted,
                    ["totalReputationGained"] = _totalReputationGained,
                    ["totalBossesKilled"] = _totalBossesKilled,
                    ["totalCriticalHits"] = _totalCriticalHits,
                    ["totalDodges"] = _totalDodges,
                    ["totalHealed"] = _totalHealed,
                    ["savedAt"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                
                var json = JSON.Print(data);
                file.Open(path, File.ModeFlags.Write);
                file.StoreString(json);
                file.Close();
                
                GD.Print($"[DailyChallengeManager] Saved {_dailyChallenges.Count} challenges to storage");
            }
            catch (Exception e) {
                GD.PrintErr($"[DailyChallengeManager] Failed to save challenges: {e.Message}");
            }
        }
        
        private Array BuildChallengeArray() {
            var challengeArray = new Godot.Collections.Array();
            foreach (var challenge in _dailyChallenges) {
                challengeArray.Add(new Dictionary {
                    ["Id"] = challenge.Id,
                    ["Name"] = challenge.Name,
                    ["Description"] = challenge.Description,
                    ["Type"] = (int)challenge.Type,
                    ["Difficulty"] = (int)challenge.Difficulty,
                    ["TargetCount"] = challenge.TargetCount,
                    ["CurrentProgress"] = challenge.CurrentProgress,
                    ["IsCompleted"] = challenge.IsCompleted,
                    ["GoldReward"] = challenge.GoldReward,
                    ["ExpReward"] = challenge.ExpReward,
                    ["ItemRewardIds"] = new Godot.Collections.Array(challenge.ItemRewardIds),
                    ["ExpireTime"] = challenge.ExpireTime.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            return challengeArray;
        }
        
        private void StartSurvivalTimer() {
            // Survival time is tracked in _PhysicsProcess
        }
        
        public void Update() {
            if (_player == null) return;
            
            // Update survival time challenge
            _survivalTimeSeconds++;
            UpdateChallengeProgress(ChallengeType.SurvivalTime, _survivalTimeSeconds);
            
            // Auto-save periodically
            _autoSaveTimer++;
            if (_autoSaveTimer >= AutoSaveInterval * 60) { // Assuming 60 FPS
                _autoSaveTimer = 0;
                var today = DateTime.Today;
                var saveKey = $"daily_challenges_{today:yyyyMMdd}";
                SaveDailyChallenges(saveKey);
            }
        }
        
        /// <summary>
        /// Called when an enemy is killed
        /// </summary>
        public void OnEnemyKilled(Enemy enemy) {
            _totalKills++;
            UpdateChallengeProgress(ChallengeType.KillEnemies, _totalKills);
            
            // Also track by enemy type if needed
        }
        
        /// <summary>
        /// Called when damage is dealt
        /// </summary>
        public void OnDamageDealt(float damage) {
            _totalDamageDealt += (int)damage;
            UpdateChallengeProgress(ChallengeType.DealDamage, _totalDamageDealt);
        }
        
        /// <summary>
        /// Called when gold is earned
        /// </summary>
        public void OnGoldEarned(int gold) {
            _totalGoldEarned += gold;
            UpdateChallengeProgress(ChallengeType.EarnGold, _totalGoldEarned);
        }
        
        /// <summary>
        /// Called when a skill is used
        /// </summary>
        public void OnSkillUsed() {
            _totalSkillsUsed++;
            UpdateChallengeProgress(ChallengeType.UseSkills, _totalSkillsUsed);
        }
        
        /// <summary>
        /// Called when a quest is completed
        /// </summary>
        public void OnQuestCompleted() {
            _totalQuestsCompleted++;
            UpdateChallengeProgress(ChallengeType.CompleteQuests, _totalQuestsCompleted);
        }
        
        /// <summary>
        /// Called when a region is explored
        /// </summary>
        public void OnRegionExplored(string regionId) {
            if (_exploredRegionIds.Contains(regionId)) return;
            
            _exploredRegionIds.Add(regionId);
            _regionsExplored = _exploredRegionIds.Count;
            UpdateChallengeProgress(ChallengeType.ExploreRegions, _regionsExplored);
        }
        
        /// <summary>
        /// Called when an item is collected
        /// </summary>
        public void OnItemCollected(int itemId) {
            // For collect items challenges, we need to track specific items
            // This is a simplified implementation
            UpdateChallengeProgress(ChallengeType.CollectItems, 1);
        }
        
        /// <summary>
        /// Called when a fish is caught
        /// </summary>
        public void OnFishCaught() {
            _totalFishCaught++;
            UpdateChallengeProgress(ChallengeType.Fishing, _totalFishCaught);
        }
        
        /// <summary>
        /// Called when alchemy is crafted
        /// </summary>
        public void OnAlchemyCrafted() {
            _totalAlchemyCrafted++;
            UpdateChallengeProgress(ChallengeType.Alchemy, _totalAlchemyCrafted);
        }
        
        /// <summary>
        /// Called when enemy is killed while mounted
        /// </summary>
        public void OnMountKill() {
            _totalMountKills++;
            UpdateChallengeProgress(ChallengeType.MountCombat, _totalMountKills);
        }
        
        /// <summary>
        /// Called when a mount skill is used
        /// </summary>
        public void OnMountSkillUsed() {
            _totalMountSkillsUsed++;
            UpdateChallengeProgress(ChallengeType.MountCombat, _totalMountSkillsUsed);
        }
        
        /// <summary>
        /// Called when pet kills an enemy
        /// </summary>
        public void OnPetKill() {
            _totalPetKills++;
            UpdateChallengeProgress(ChallengeType.PetBattle, _totalPetKills);
        }
        
        /// <summary>
        /// Called when an item is sold
        /// </summary>
        public void OnItemSold() {
            _totalItemsSold++;
            UpdateChallengeProgress(ChallengeType.Trade, _totalItemsSold);
        }
        
        /// <summary>
        /// Called when an item is crafted
        /// </summary>
        public void OnItemCrafted() {
            _totalItemsCrafted++;
            UpdateChallengeProgress(ChallengeType.CraftItem, _totalItemsCrafted);
        }
        
        /// <summary>
        /// Called when reputation is gained
        /// </summary>
        public void OnReputationGained(int amount) {
            _totalReputationGained += amount;
            UpdateChallengeProgress(ChallengeType.Reputation, _totalReputationGained);
        }
        
        /// <summary>
        /// Called when a boss is killed
        /// </summary>
        public void OnBossKilled() {
            _totalBossesKilled++;
            UpdateChallengeProgress(ChallengeType.KillBoss, _totalBossesKilled);
            UpdateChallengeProgress(ChallengeType.KillEnemies, _totalKills);
        }
        
        /// <summary>
        /// Called when a critical hit occurs
        /// </summary>
        public void OnCriticalHit() {
            _totalCriticalHits++;
            UpdateChallengeProgress(ChallengeType.CriticalHits, _totalCriticalHits);
        }
        
        /// <summary>
        /// Called when an attack is dodged
        /// </summary>
        public void OnDodge() {
            _totalDodges++;
            UpdateChallengeProgress(ChallengeType.Dodge, _totalDodges);
        }
        
        /// <summary>
        /// Called when player is healed
        /// </summary>
        public void OnHealed(int amount) {
            _totalHealed += amount;
            UpdateChallengeProgress(ChallengeType.Heal, _totalHealed);
        }
        
        /// <summary>
        /// Called when pet levels up
        /// </summary>
        public void OnPetLevelUp() {
            UpdateChallengeProgress(ChallengeType.PetBattle, 1);
        }
        
        private void UpdateChallengeProgress(ChallengeType type, int newValue) {
            bool anyCompleted = false; 
            
            foreach (var challenge in _dailyChallenges) {
                if (challenge.Type == type && !challenge.IsCompleted) {
                    int oldProgress = challenge.CurrentProgress;
                    challenge.CurrentProgress = newValue;
                    
                    if (challenge.CurrentProgress >= challenge.TargetCount && oldProgress < challenge.TargetCount) {
                        challenge.IsCompleted = true;
                        GrantRewards(challenge);
                        anyCompleted = true;
                        
                        GD.Print($"[DailyChallengeManager] Challenge completed: {challenge.Name}");
                    }
                    
                    // Emit update signal
                    EmitSignal(ChallengeUpdatedSignal, challenge);
                }
            }
            
            // Check if all challenges are completed
            if (IsAllCompleted() && !anyCompleted) {
                EmitSignal(AllChallengesCompletedSignal);
            }
        }
        
        private void GrantRewards(DailyChallenge challenge) {
            if (_player == null) return;
            
            // Grant gold
            if (challenge.GoldReward > 0) {
                _player.AddGold(challenge.GoldReward);
            }
            
            // Grant experience
            if (challenge.ExpReward > 0) {
                _player.AddExperience(challenge.ExpReward);
            }
            
            // Grant items
            foreach (var itemId in challenge.ItemRewardIds) {
                _player.Inventory.AddItem(itemId, 1);
            }
            
            // Emit completion signal
            EmitSignal(ChallengeCompletedSignal, challenge);
        }
        
        private void EmitSignal(string signalName, DailyChallenge challenge = null) {
            // This would emit a Godot signal in a full implementation
            // For now, we'll use GD.Print for debugging
            if (challenge != null) {
                GD.Print($"[DailyChallengeManager] Signal: {signalName} - {challenge.Name}");
            } else {
                GD.Print($"[DailyChallengeManager] Signal: {signalName}");
            }
        }
        
        public bool IsAllCompleted() {
            foreach (var challenge in _dailyChallenges) {
                if (!challenge.IsCompleted) return false;
            }
            return _dailyChallenges.Count > 0;
        }
        
        public int GetCompletedCount() {
            int count = 0;
            foreach (var challenge in _dailyChallenges) {
                if (challenge.IsCompleted) count++;
            }
            return count;
        }
        
        public float GetOverallProgress() {
            if (_dailyChallenges.Count == 0) return 0f;
            
            float total = 0f;
            foreach (var challenge in _dailyChallenges) {
                total += challenge.GetProgressPercentage();
            }
            return total / _dailyChallenges.Count;
        }
        
        // Getters
        public List<DailyChallenge> GetDailyChallenges() => _dailyChallenges;
        public int GetTotalKills() => _totalKills;
        public int GetTotalDamageDealt() => _totalDamageDealt;
        public int GetTotalGoldEarned() => _totalGoldEarned;
        public int GetTotalSkillsUsed() => _totalSkillsUsed;
        public int GetTotalQuestsCompleted() => _totalQuestsCompleted;
        public int GetRegionsExplored() => _regionsExplored;
        public int GetSurvivalTimeSeconds() => _survivalTimeSeconds;
        
        public void ResetDailyChallenges() {
            _dailyChallenges = DailyChallengeDatabase.Instance.GetRandomChallenges(3);
            _totalKills = 0;
            _totalDamageDealt = 0;
            _totalGoldEarned = 0;
            _totalSkillsUsed = 0;
            _totalQuestsCompleted = 0;
            _regionsExplored = 0;
            _exploredRegionIds.Clear();
            _survivalTimeSeconds = 0;
            _autoSaveTimer = 0;
            
            // Reset new challenge tracking
            _totalFishCaught = 0;
            _totalAlchemyCrafted = 0;
            _totalMountKills = 0;
            _totalMountSkillsUsed = 0;
            _totalPetKills = 0;
            _totalItemsSold = 0;
            _totalItemsCrafted = 0;
            _totalReputationGained = 0;
            _totalBossesKilled = 0;
            _totalCriticalHits = 0;
            _totalDodges = 0;
            _totalHealed = 0;
        }
    }
}
