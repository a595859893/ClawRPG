using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Pet Combat Companion System - coordinates pet behavior with player combat
    /// Features: Combo system, learning, sync with player
    /// </summary>
    public partial class PetCombatCompanionSystem : BaseSystem
    {
        public static PetCombatCompanionSystem Instance { get; private set; }

        private PetCombatCompanionData _companionData = new PetCombatCompanionData();
        
        // Signals for UI and game integration
        public Signal<string, int> ComboChainChanged { get; }
        public Signal<string, string> RoleChanged { get; }
        public Signal<string, float> SyncLevelChanged { get; }
        public Signal<string, ComboType, float> ComboExecuted { get; }
        public Signal<string, string> LearningUpdated { get; }
        public Signal<string, Vector2> PositionRecommendation { get; }

        public override void _Ready()
        {
            Instance = this;
            LoadData();
        }

        public override void _Process(double delta)
        {
            UpdateComboChains((float)delta);
        }

        #region Initialization

        public void InitializePetCompanion(string petId)
        {
            if (!_companionData.PetStates.ContainsKey(petId))
            {
                _companionData.PetStates[petId] = new PetCompanionState
                {
                    PetId = petId,
                    CurrentRole = "Attacker",
                    ComboChain = 0,
                    ComboWindow = 2.0f,
                    SyncLevel = 0.5f
                };
            }

            if (!_companionData.LearningData.ContainsKey(petId))
            {
                _companionData.LearningData[petId] = new PetLearningData
                {
                    PetId = petId,
                    AdaptationLevel = 0f,
                    LastLearningUpdate = DateTime.Now
                };
            }

            if (!_companionData.ComboHistory.ContainsKey(petId))
            {
                _companionData.ComboHistory[petId] = new List<CombatComboRecord>();
            }
        }

        #endregion

        #region Role Management

        public void SetPetRole(string petId, string role)
        {
            if (_companionData.PetStates.TryGetValue(petId, out var state))
            {
                string oldRole = state.CurrentRole;
                state.CurrentRole = role;
                
                // Update learning data
                if (_companionData.LearningData.TryGetValue(petId, out var learning))
                {
                    if (!learning.PreferredBehaviors.Contains(role))
                    {
                        learning.PreferredBehaviors.Add(role);
                    }
                }

                RoleChanged?.Emit(petId, role);
            }
        }

        public string GetPetRole(string petId)
        {
            if (_companionData.PetStates.TryGetValue(petId, out var state))
            {
                return state.CurrentRole;
            }
            return "Attacker";
        }

        public string GetRecommendedRole(string petId, float petHealthPercent, int nearbyEnemies, bool ownerNeedsHealing)
        {
            if (ownerNeedsHealing)
                return "Support";
            
            if (petHealthPercent < 0.3f)
                return "Scout";
            
            if (nearbyEnemies > 3)
                return "Tank";
            
            return "Attacker";
        }

        #endregion

        #region Combo System

        public void RecordPlayerAttack(string petId, string attackType, Vector2 playerPosition)
        {
            if (!_companionData.PetStates.TryGetValue(petId, out var state))
                return;

            float currentTime = (float)GetTimeSinceStart();
            float timeSinceLastAction = currentTime - state.LastAttackTime;

            // Update player's attack pattern for learning
            if (_companionData.LearningData.TryGetValue(petId, out var learning))
            {
                learning.PlayerAttackPattern[attackType] = timeSinceLastAction;
                
                // Update average interval
                float total = 0;
                int count = 0;
                foreach (var interval in learning.PlayerAttackPattern.Values)
                {
                    total += interval;
                    count++;
                }
                if (count > 0)
                    learning.AveragePlayerAttackInterval = total / count;
            }

            // Check if within combo window
            if (timeSinceLastAction <= state.ComboWindow && state.IsInCombo)
            {
                state.ComboChain++;
                _companionData.TotalCombos++;
                
                if (state.ComboChain > _companionData.HighestComboChain)
                    _companionData.HighestComboChain = state.ComboChain;

                ComboChainChanged?.Emit(petId, state.ComboChain);
            }
            else
            {
                // Reset combo if window expired
                if (state.ComboChain > 0)
                {
                    ExecuteCombo(petId, state.ComboChain);
                }
                state.ComboChain = 1;
                state.IsInCombo = true;
            }

            state.LastAttackTime = currentTime;
            state.LastPlayerAction = attackType;
            state.LastPlayerPosition = playerPosition;

            // Update sync level based on timing
            UpdateSyncLevel(petId, timeSinceLastAction);
        }

        private void ExecuteCombo(string petId, int chainLength)
        {
            if (!_companionData.PetStates.TryGetValue(petId, out var state))
                return;

            ComboType comboType = DetermineComboType(chainLength, state.CurrentRole);
            float damage = CalculateComboDamage(chainLength, state.SyncLevel);
            int hitCount = Mathf.Min(chainLength, 10);

            // Record combo
            var record = new CombatComboRecord
            {
                PetId = petId,
                ComboType = comboType.ToString(),
                Damage = damage,
                Duration = state.ComboWindow,
                HitCount = hitCount,
                Timestamp = DateTime.Now
            };

            if (_companionData.ComboHistory.ContainsKey(petId))
            {
                _companionData.ComboHistory[petId].Add(record);
                
                // Keep only last 100 combos
                if (_companionData.ComboHistory[petId].Count > 100)
                    _companionData.ComboHistory[petId].RemoveAt(0);
            }

            _companionData.TotalComboDamage += damage;
            
            ComboExecuted?.Emit(petId, comboType, damage);
        }

        private ComboType DetermineComboType(int chainLength, string role)
        {
            if (chainLength >= 8)
                return ComboType.Ultimate;
            if (chainLength >= 5)
                return ComboType.Support;
            if (role == "Support")
                return ComboType.Support;
            if (chainLength >= 3)
                return ComboType.Chain;
            return ComboType.Basic;
        }

        private float CalculateComboDamage(int chainLength, float syncLevel)
        {
            float baseDamage = 10f;
            float chainMultiplier = 1f + (chainLength - 1) * 0.2f;
            float syncMultiplier = 1f + syncLevel * 0.5f;
            return baseDamage * chainMultiplier * syncMultiplier;
        }

        private void UpdateComboChains(float delta)
        {
            float currentTime = (float)GetTimeSinceStart();

            foreach (var state in _companionData.PetStates.Values)
            {
                if (state.IsInCombo)
                {
                    float timeSinceLastAttack = currentTime - state.LastAttackTime;
                    
                    if (timeSinceLastAttack > state.ComboWindow)
                    {
                        // Combo expired
                        if (state.ComboChain > 0)
                        {
                            ExecuteCombo(state.PetId, state.ComboChain);
                        }
                        state.ComboChain = 0;
                        state.IsInCombo = false;
                    }
                }
            }
        }

        #endregion

        #region Sync Level

        private void UpdateSyncLevel(string petId, float timeSinceLastAction)
        {
            if (!_companionData.PetStates.TryGetValue(petId, out var state))
                return;

            if (!_companionData.LearningData.TryGetValue(petId, out var learning))
                return;

            // Sync improves when pet attacks close to player's attack timing
            float idealInterval = learning.AveragePlayerAttackInterval;
            float deviation = Mathf.Abs(timeSinceLastAction - idealInterval);
            
            float syncChange = 0.01f;
            if (deviation < 0.3f)
            {
                state.SyncLevel = Mathf.Min(1f, state.SyncLevel + syncChange);
            }
            else
            {
                state.SyncLevel = Mathf.Max(0f, state.SyncLevel - syncChange * 0.5f);
            }

            SyncLevelChanged?.Emit(petId, state.SyncLevel);
        }

        public float GetSyncLevel(string petId)
        {
            if (_companionData.PetStates.TryGetValue(petId, out var state))
            {
                return state.SyncLevel;
            }
            return 0.5f;
        }

        #endregion

        #region Learning System

        public void RecordEnemyKill(string petId, string enemyType)
        {
            if (_companionData.LearningData.TryGetValue(petId, out var learning))
            {
                if (learning.EnemyTypeKills.ContainsKey(enemyType))
                    learning.EnemyTypeKills[enemyType]++;
                else
                    learning.EnemyTypeKills[enemyType] = 1;

                // Increase adaptation level
                learning.AdaptationLevel = Mathf.Min(100f, learning.AdaptationLevel + 0.5f);
                learning.LastLearningUpdate = DateTime.Now;

                LearningUpdated?.Emit(petId, "enemy_kill");
            }
        }

        public void RecordDodgeResult(string petId, bool success)
        {
            if (_companionData.LearningData.TryGetValue(petId, out var learning))
            {
                if (success)
                    learning.SuccessfulDodges++;
                else
                    learning.FailedDodges++;

                int total = learning.SuccessfulDodges + learning.FailedDodges;
                if (total > 0)
                    learning.DodgeSuccessRate = (float)learning.SuccessfulDodges / total;

                LearningUpdated?.Emit(petId, "dodge");
            }
        }

        public string GetMostKilledEnemyType(string petId)
        {
            if (_companionData.LearningData.TryGetValue(petId, out var learning))
            {
                string maxEnemy = "";
                int maxKills = 0;
                
                foreach (var kvp in learning.EnemyTypeKills)
                {
                    if (kvp.Value > maxKills)
                    {
                        maxKills = kvp.Value;
                        maxEnemy = kvp.Key;
                    }
                }
                
                return maxEnemy;
            }
            return "";
        }

        public Dictionary<string, object> GetLearningReport(string petId)
        {
            var report = new Dictionary<string, object>();
            
            if (_companionData.LearningData.TryGetValue(petId, out var learning))
            {
                report["most_killed_enemy"] = GetMostKilledEnemyType(petId);
                report["total_enemies_killed"] = learning.SuccessfulDodges + learning.FailedDodges; // Approximation
                report["dodge_success_rate"] = learning.DodgeSuccessRate;
                report["adaptation_level"] = learning.AdaptationLevel;
                report["preferred_role"] = learning.PreferredBehaviors.Count > 0 ? learning.PreferredBehaviors[0] : "Attacker";
                report["avg_attack_interval"] = learning.AveragePlayerAttackInterval;
            }

            return report;
        }

        #endregion

        #region Position Recommendations

        public Vector2 GetRecommendedPosition(string petId, Vector2 playerPosition, Vector2 enemyPosition, int enemyCount)
        {
            if (!_companionData.PetStates.TryGetValue(petId, out var state))
                return playerPosition;

            Vector2 directionToEnemy = (enemyPosition - playerPosition).Normalized();
            float optimalDistance = state.CurrentRole switch
            {
                "Tank" => 80f,      // Close to protect
                "Attacker" => 150f, // Medium range
                "Support" => 200f,   // Back line
                "Scout" => 300f,    // Far to flank
                _ => 150f
            };

            // Add some randomness based on sync level
            float randomOffset = (1f - state.SyncLevel) * 50f * (Random.Shared.NextFloat() - 0.5f);
            
            Vector2 recommended = playerPosition + directionToEnemy * optimalDistance;
            recommended.x += randomOffset;
            
            PositionRecommendation?.Emit(petId, recommended);
            return recommended;
        }

        #endregion

        #region Statistics

        public Dictionary<string, object> GetStatistics()
        {
            var stats = new Dictionary<string, object>();
            
            stats["total_combos"] = _companionData.TotalCombos;
            stats["total_combo_damage"] = _companionData.TotalComboDamage;
            stats["highest_combo_chain"] = _companionData.HighestComboChain;
            stats["pet_count"] = _companionData.PetStates.Count;

            return stats;
        }

        public Dictionary<string, object> GetPetStatistics(string petId)
        {
            var stats = new Dictionary<string, object>();
            
            if (_companionData.PetStates.TryGetValue(petId, out var state))
            {
                stats["role"] = state.CurrentRole;
                stats["current_combo_chain"] = state.ComboChain;
                stats["sync_level"] = state.SyncLevel;
            }

            if (_companionData.ComboHistory.TryGetValue(petId, out var history))
            {
                float totalDamage = 0;
                int totalHits = 0;
                foreach (var record in history)
                {
                    totalDamage += record.Damage;
                    totalHits += record.HitCount;
                }
                stats["total_combos"] = history.Count;
                stats["total_combo_damage"] = totalDamage;
                stats["total_combo_hits"] = totalHits;
            }

            // Add learning data
            foreach (var kvp in GetLearningReport(petId))
            {
                stats[kvp.Key] = kvp.Value;
            }

            return stats;
        }

        #endregion

        #region Save/Load

        public void SaveData()
        {
            GameDataManager.SetData("pet_combat_companion", _companionData);
        }

        public void LoadData()
        {
            if (GameDataManager.HasData("pet_combat_companion"))
            {
                _companionData = GameDataManager.GetData<PetCombatCompanionData>("pet_combat_companion");
            }
        }

        #endregion

        private double GetTimeSinceStart()
        {
            return Time.GetTicksMsec() / 1000.0;
        }
    }
}
