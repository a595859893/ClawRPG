using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Database;
using ClawRPG.Scripts.Data;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Pet AI Behavior System - intelligent pet combat behavior
    /// </summary>
    public partial class PetAIBehaviorSystem : BaseSystem
    {
        public static PetAIBehaviorSystem Instance { get; private set; }

        private PlayerPetAIData _playerPetAIData = new PlayerPetAIData();
        private Dictionary<string, PetAIData> _activePetAI = new Dictionary<string, PetAIData>();
        
        // Signals
        public Signal<string> BehaviorChanged { get; }
        public Signal<string, string> StateChanged { get; }
        public Signal<string, string> TargetSelected { get; }
        public Signal<string, float> DamageAvoided { get; }
        public Signal<string, Vector2> PositionUpdated { get; }

        public override void _Ready()
        {
            Instance = this;
            LoadData();
        }

        public void InitializePetAI(string petId)
        {
            if (!_playerPetAIData.PetAIStates.ContainsKey(petId))
            {
                _playerPetAIData.PetAIStates[petId] = new PetAIData
                {
                    PetId = petId,
                    CurrentBehavior = PetAIBehavior.Aggressive,
                    CurrentState = PetAIState.Idle
                };
            }
            
            _activePetAI[petId] = _playerPetAIData.PetAIStates[petId];
        }

        public void SetBehavior(string petId, PetAIBehavior behavior)
        {
            if (_activePetAI.TryGetValue(petId, out var aiData))
            {
                aiData.CurrentBehavior = behavior;
                BehaviorChanged?.Emit(petId);
            }
        }

        public void SetBehaviorById(string petId, string behaviorId)
        {
            var config = PetAIDatabase.GetBehavior(behaviorId);
            if (_activePetAI.TryGetValue(petId, out var aiData))
            {
                aiData.CurrentBehavior = config.BehaviorType;
                BehaviorChanged?.Emit(petId);
            }
        }

        public AIBehaviorConfig GetBehaviorConfig(string petId)
        {
            if (_activePetAI.TryGetValue(petId, out var aiData))
            {
                return PetAIDatabase.GetBehaviorByType(aiData.CurrentBehavior);
            }
            return PetAIDatabase.GetBehaviorByType(PetAIBehavior.Aggressive);
        }

        public void UpdatePetState(string petId, Vector2 position, float currentHp, float maxHp, 
            List<string> nearbyEnemies, Vector2? ownerPosition = null)
        {
            if (!_activePetAI.TryGetValue(petId, out var aiData))
                return;

            var config = GetBehaviorConfig(petId);
            float hpPercent = currentHp / Mathf.Max(maxHp, 1f);

            // Update position
            aiData.LastPosition = aiData.TargetPosition;
            aiData.TargetPosition = position;

            // State machine
            switch (config.BehaviorType)
            {
                case PetAIBehavior.Passive:
                    UpdatePassiveBehavior(aiData, config, currentHp, maxHp, nearbyEnemies);
                    break;
                case PetAIBehavior.Defensive:
                    UpdateDefensiveBehavior(aiData, config, currentHp, maxHp, nearbyEnemies, ownerPosition);
                    break;
                case PetAIBehavior.Aggressive:
                    UpdateAggressiveBehavior(aiData, config, currentHp, maxHp, nearbyEnemies);
                    break;
                case PetAIBehavior.Tactical:
                    UpdateTacticalBehavior(aiData, config, currentHp, maxHp, nearbyEnemies, ownerPosition);
                    break;
                case PetAIBehavior.Support:
                    UpdateSupportBehavior(aiData, config, currentHp, maxHp, nearbyEnemies, ownerPosition);
                    break;
            }

            StateChanged?.Emit(petId, aiData.CurrentState.ToString());
        }

        private void UpdatePassiveBehavior(PetAIData aiData, AIBehaviorConfig config, float currentHp, float maxHp, 
            List<string> nearbyEnemies)
        {
            // Passive: Only attack when very close
            if (nearbyEnemies.Count > 0 && nearbyEnemies.Count <= 2)
            {
                aiData.CurrentState = PetAIState.Attacking;
                if (string.IsNullOrEmpty(aiData.TargetEnemyId))
                    aiData.TargetEnemyId = nearbyEnemies[0];
            }
            else
            {
                aiData.CurrentState = PetAIState.Idle;
                aiData.TargetEnemyId = null;
            }
        }

        private void UpdateDefensiveBehavior(PetAIData aiData, AIBehaviorConfig config, float currentHp, float maxHp, 
            List<string> nearbyEnemies, Vector2? ownerPosition)
        {
            float hpPercent = currentHp / Mathf.Max(maxHp, 1f);

            // Check if should flee
            if (hpPercent < config.FleeThreshold && nearbyEnemies.Count > 0)
            {
                aiData.CurrentState = PetAIState.Fleeing;
                return;
            }

            // Prioritize enemies near owner
            if (nearbyEnemies.Count > 0)
            {
                aiData.CurrentState = PetAIState.Attacking;
                if (string.IsNullOrEmpty(aiData.TargetEnemyId) || !nearbyEnemies.Contains(aiData.TargetEnemyId))
                    aiData.TargetEnemyId = nearbyEnemies[0];
            }
            else if (ownerPosition.HasValue)
            {
                aiData.CurrentState = PetAIState.Returning;
            }
            else
            {
                aiData.CurrentState = PetAIState.Idle;
            }
        }

        private void UpdateAggressiveBehavior(PetAIData aiData, AIBehaviorConfig config, float currentHp, float maxHp, 
            List<string> nearbyEnemies)
        {
            // Aggressive: Always attack nearest enemy
            aiData.TargetSwitchTimer += (float)GetProcessDeltaTime();
            
            if (nearbyEnemies.Count > 0)
            {
                // Switch target periodically
                if (string.IsNullOrEmpty(aiData.TargetEnemyId) || 
                    !nearbyEnemies.Contains(aiData.TargetEnemyId) ||
                    aiData.TargetSwitchTimer >= config.TargetSwitchTime)
                {
                    aiData.TargetEnemyId = nearbyEnemies[0]; // Nearest
                    aiData.TargetSwitchTimer = 0;
                    TargetSelected?.Emit(aiData.PetId, aiData.TargetEnemyId);
                }
                aiData.CurrentState = PetAIState.Attacking;
            }
            else
            {
                aiData.CurrentState = PetAIState.Chasing;
                aiData.TargetEnemyId = null;
            }
        }

        private void UpdateTacticalBehavior(PetAIData aiData, AIBehaviorConfig config, float currentHp, float maxHp, 
            List<string> nearbyEnemies, Vector2? ownerPosition)
        {
            float hpPercent = currentHp / Mathf.Max(maxHp, 1f);

            // Smart target selection: prioritize low HP enemies
            if (nearbyEnemies.Count > 0)
            {
                aiData.CurrentState = PetAIState.Attacking;
                
                // Check if current target should be switched (enemy HP too high)
                if (aiData.TargetSwitchTimer >= config.TargetSwitchTime)
                {
                    // Find lowest HP enemy
                    aiData.TargetEnemyId = nearbyEnemies[0];
                    aiData.TargetSwitchTimer = 0;
                    TargetSelected?.Emit(aiData.PetId, aiData.TargetEnemyId);
                }
                else
                {
                    aiData.TargetSwitchTimer += (float)GetProcessDeltaTime();
                }
            }
            else
            {
                // Keep distance from enemies, return to owner
                aiData.CurrentState = ownerPosition.HasValue ? PetAIState.Returning : PetAIState.Idle;
            }

            // Dodge and block calculation
            if (config.DodgeChance > 0 && Random.Shared.NextFloat() < config.DodgeChance)
            {
                aiData.DodgesSuccessful++;
                _playerPetAIData.TotalDodges++;
                aiData.TotalDamageAvoided += currentHp * 0.1f;
                _playerPetAIData.TotalDamageAvoided += currentHp * 0.1f;
                DamageAvoided?.Emit(aiData.PetId, currentHp * 0.1f);
            }
        }

        private void UpdateSupportBehavior(PetAIData aiData, AIBehaviorConfig config, float currentHp, float maxHp, 
            List<string> nearbyEnemies, Vector2? ownerPosition)
        {
            float hpPercent = currentHp / Mathf.Max(maxHp, 1f);

            // Check if should support owner or other pets
            if (ownerPosition.HasValue)
            {
                float distToOwner = config.SupportRange;
                if (distToOwner > config.SupportRange)
                {
                    aiData.CurrentState = PetAIState.Returning;
                    return;
                }
            }

            // Only attack if no enemies threatening owner
            if (nearbyEnemies.Count == 0)
            {
                aiData.CurrentState = PetAIState.Supporting;
                aiData.TargetEnemyId = null;
            }
            else
            {
                aiData.CurrentState = PetAIState.Attacking;
                if (string.IsNullOrEmpty(aiData.TargetEnemyId))
                    aiData.TargetEnemyId = nearbyEnemies[0];
            }
        }

        public bool ShouldDodge(string petId)
        {
            var config = GetBehaviorConfig(petId);
            return Random.Shared.NextFloat() < config.DodgeChance;
        }

        public bool ShouldBlock(string petId)
        {
            var config = GetBehaviorConfig(petId);
            return Random.Shared.NextFloat() < config.BlockChance;
        }

        public bool ShouldUseSkill(string petId, float currentHp, float maxHp)
        {
            var config = GetBehaviorConfig(petId);
            if (!config.UseSkills) return false;
            
            float hpPercent = currentHp / Mathf.Max(maxHp, 1f);
            return hpPercent < config.SkillCooldownThreshold;
        }

        public Vector2 GetTargetPosition(string petId)
        {
            if (_activePetAI.TryGetValue(petId, out var aiData))
            {
                return aiData.TargetPosition;
            }
            return Vector2.Zero;
        }

        public string GetCurrentTarget(string petId)
        {
            if (_activePetAI.TryGetValue(petId, out var aiData))
            {
                return aiData.TargetEnemyId;
            }
            return null;
        }

        public PetAIState GetCurrentState(string petId)
        {
            if (_activePetAI.TryGetValue(petId, out var aiData))
            {
                return aiData.CurrentState;
            }
            return PetAIState.Idle;
        }

        public void RecordAttack(string petId, float damage)
        {
            if (_activePetAI.TryGetValue(petId, out var aiData))
            {
                aiData.EnemiesAttacked++;
                aiData.TotalDamageDealt += damage;
            }
        }

        public Dictionary<string, object> GetStatistics(string petId)
        {
            var stats = new Dictionary<string, object>();
            
            if (_playerPetAIData.PetAIStates.TryGetValue(petId, out var aiData))
            {
                stats["enemies_attacked"] = aiData.EnemiesAttacked;
                stats["dodges_successful"] = aiData.DodgesSuccessful;
                stats["blocks_successful"] = aiData.BlocksSuccessful;
                stats["total_damage_dealt"] = aiData.TotalDamageDealt;
                stats["total_damage_avoided"] = aiData.TotalDamageAvoided;
            }
            
            stats["total_dodges"] = _playerPetAIData.TotalDodges;
            stats["total_blocks"] = _playerPetAIData.TotalBlocks;
            stats["total_damage_avoided"] = _playerPetAIData.TotalDamageAvoided;
            
            return stats;
        }

        public void SaveData()
        {
            // Save to player data
            GameDataManager.SetData("pet_ai_data", _playerPetAIData);
        }

        public void LoadData()
        {
            if (GameDataManager.HasData("pet_ai_data"))
            {
                _playerPetAIData = GameDataManager.GetData<PlayerPetAIData>("pet_ai_data");
            }
        }

        public void ResetPetAI(string petId)
        {
            if (_playerPetAIData.PetAIStates.ContainsKey(petId))
            {
                _playerPetAIData.PetAIStates[petId] = new PetAIData
                {
                    PetId = petId,
                    CurrentBehavior = PetAIBehavior.Aggressive,
                    CurrentState = PetAIState.Idle
                };
            }
        }
    }
}
