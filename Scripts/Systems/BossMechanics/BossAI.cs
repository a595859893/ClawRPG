using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.BossMechanics {
    /// <summary>
    /// Boss AI - Handles boss decision making and behavior
    /// Part of BossMechanicsSystem refactoring
    /// </summary>
    public partial class BossAI : BaseSystem
    {
        private BossMechanicsSystem _bossSystem;
        private BossAbilityDatabase _abilityDb;
        private BossPhaseManager _phaseManager;
        
        // AI configuration
        private float _decisionInterval = 1.0f;  // Make decisions every second
        private float _decisionTimer = 0f;
        
        // Random for AI decisions
        private Random _random = new Random();
        
        public BossAI(BossMechanicsSystem bossSystem, BossAbilityDatabase abilityDb, BossPhaseManager phaseManager)
        {
            _bossSystem = bossSystem;
            _abilityDb = abilityDb;
            _phaseManager = phaseManager;
        }
        
        /// <summary>
        /// Set AI decision interval
        /// </summary>
        public void SetDecisionInterval(float interval)
        {
            _decisionInterval = interval;
        }
        
        /// <summary>
        /// Update AI decision making
        /// </summary>
        public void Update(BossBattleState state, BossMechanicsData bossData, float delta)
        {
            if (state == null || bossData == null) return;
            
            _decisionTimer += delta;
            
            if (_decisionTimer >= _decisionInterval)
            {
                _decisionTimer = 0f;
                MakeDecision(state, bossData);
            }
        }
        
        /// <summary>
        /// Make AI decision
        /// </summary>
        private void MakeDecision(BossBattleState state, BossMechanicsData bossData)
        {
            // Check phase transitions
            _phaseManager.CheckPhaseTransition(state, bossData);
            
            // Check enrage
            CheckEnrage(state, bossData);
            
            // Decide whether to use a skill
            if (bossData.Skills != null && bossData.Skills.Count > 0)
            {
                TryUseSkill(state, bossData);
            }
            
            // Check minion spawning
            TrySpawnMinions(state, bossData);
        }
        
        /// <summary>
        /// Try to use a skill
        /// </summary>
        private void TryUseSkill(BossBattleState state, BossMechanicsData bossData)
        {
            // Get available skills
            var availableSkills = _abilityDb.GetAvailableSkills(state, bossData);
            if (availableSkills.Count == 0) return;
            
            // Select skill based on AI personality
            string selectedSkill = SelectSkill(state, bossData, availableSkills);
            
            if (!string.IsNullOrEmpty(selectedSkill))
            {
                _abilityDb.UseSkill(state, bossData, selectedSkill);
            }
        }
        
        /// <summary>
        /// Select skill based on boss AI personality
        /// </summary>
        private string SelectSkill(BossBattleState state, BossMechanicsData bossData, List<string> availableSkills)
        {
            if (availableSkills.Count == 0) return null;
            
            // Simple random selection with bias towards less used skills
            // Could be enhanced with boss-specific AI personalities
            
            float healthPercent = state.CurrentHealth / state.MaxHealth;
            
            // In low health, prefer defensive/healing skills if available
            if (healthPercent < 0.3f)
            {
                foreach (var skillId in availableSkills)
                {
                    var skill = _abilityDb.GetSkillData(bossData, skillId);
                    if (skill != null && skill.SkillName.ToLower().Contains("heal"))
                    {
                        return skillId;
                    }
                }
            }
            
            // Use ability database's selection
            return _abilityDb.SelectBestSkill(state, bossData);
        }
        
        /// <summary>
        /// Check and trigger enrage mechanic
        /// </summary>
        private void CheckEnrage(BossBattleState state, BossMechanicsData bossData)
        {
            if (bossData == null || !bossData.HasEnrageMechanic) return;
            
            if (!state.IsEnraged && state.BattleTime >= bossData.EnrageTime)
            {
                state.IsEnraged = true;
                GD.Print($"[BossAI] Boss {bossData.BossName} is ENRAGED!");
            }
        }
        
        /// <summary>
        /// Try to spawn minions
        /// </summary>
        private void TrySpawnMinions(BossBattleState state, BossMechanicsData bossData)
        {
            if (bossData == null || !bossData.CanSummonMinions) return;
            
            float healthPercent = state.CurrentHealth / state.MaxHealth;
            
            // Spawn based on health threshold
            if (bossData.MinionSpawnHealthPercent > 0 && healthPercent <= bossData.MinionSpawnHealthPercent)
            {
                if (state.ActiveMinionCount < bossData.MaxMinionCount)
                {
                    // Random chance per update when in range
                    if (_random.NextDouble() < 0.01) // 1% chance per decision interval
                    {
                        _bossSystem.SpawnMinions(state.BossId, bossData.MinionTypes, 1);
                    }
                }
            }
        }
        
        /// <summary>
        /// Calculate attack multiplier based on current state
        /// </summary>
        public float GetAttackMultiplier(BossBattleState state, BossMechanicsData bossData)
        {
            if (state == null || bossData == null) return 1.0f;
            
            float multiplier = 1.0f;
            
            // Phase multiplier
            float phaseMultiplier = _phaseManager.GetPhaseAttackMultiplier(state, bossData);
            multiplier *= phaseMultiplier;
            
            // Enrage multiplier
            if (state.IsEnraged && bossData.EnrageTimers != null && bossData.EnrageTimers.Count > 0)
            {
                multiplier *= bossData.EnrageTimers[0].AttackMultiplier;
            }
            
            return multiplier;
        }
        
        /// <summary>
        /// Generate loot on boss defeat
        /// </summary>
        public List<string> GenerateLoot(BossMechanicsData bossData)
        {
            List<string> loot = new List<string>();
            
            if (bossData.LootTable == null || bossData.LootTable.Length == 0)
                return loot;
            
            int lootCount = bossData.MinLootCount + _random.Next(bossData.MaxLootCount - bossData.MinLootCount + 1);
            
            for (int i = 0; i < lootCount; i++)
            {
                float roll = (float)_random.NextDouble() * 100f;
                float cumulative = 0f;
                
                for (int j = 0; j < bossData.LootTable.Length && j < bossData.LootWeights.Length; j++)
                {
                    cumulative += bossData.LootWeights[j];
                    if (roll <= cumulative)
                    {
                        loot.Add(bossData.LootTable[j]);
                        break;
                    }
                }
            }
            
            return loot;
        }
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            data["decisionInterval"] = _decisionInterval;
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            if (data.Contains("decisionInterval")) {
                _decisionInterval = Convert.ToSingle(data["decisionInterval"]);
            }
        }
    }
}
