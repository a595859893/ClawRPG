using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

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
            // This would integrate with SaveSystem in a full implementation
            return null;
        }
        
        private void SaveDailyChallenges(string key) {
            // This would integrate with SaveSystem in a full implementation
        }
        
        private void StartSurvivalTimer() {
            // Survival time is tracked in _PhysicsProcess
        }
        
        public void Update() {
            if (_player == null) return;
            
            // Update survival time challenge
            _survivalTimeSeconds++;
            UpdateChallengeProgress(ChallengeType.SurvivalTime, _survivalTimeSeconds);
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
        }
    }
}
