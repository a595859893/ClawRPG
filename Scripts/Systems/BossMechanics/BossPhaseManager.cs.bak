using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.BossMechanics {
    /// <summary>
    /// Boss Phase Manager - Handles phase transitions and phase-specific behaviors
    /// Part of BossMechanicsSystem refactoring
    /// </summary>
    public partial class BossPhaseManager : BaseSystem
    {
        private BossMechanicsSystem _bossSystem;
        
        // Phase transition callbacks
        public event Action<BossBattleState, BossPhaseData> OnPhaseTransition;
        
        public BossPhaseManager(BossMechanicsSystem bossSystem)
        {
            _bossSystem = bossSystem;
        }
        
        /// <summary>
        /// Check and handle phase transitions
        /// </summary>
        public bool CheckPhaseTransition(BossBattleState state, BossMechanicsData bossData)
        {
            if (bossData == null || bossData.Phases == null) return false;
            
            float healthPercent = state.CurrentHealth / state.MaxHealth;
            
            for (int i = bossData.Phases.Count - 1; i >= 0; i--)
            {
                var phase = bossData.Phases[i];
                if (healthPercent <= phase.HealthPercentage && state.CurrentPhase < phase.PhaseNumber)
                {
                    // Transition to new phase
                    state.CurrentPhase = phase.PhaseNumber;
                    state.PhaseChanged = true;
                    
                    // Notify listeners
                    OnPhaseTransition?.Invoke(state, phase);
                    
                    GD.Print($"[BossPhaseManager] Boss entered phase {phase.PhaseNumber}: {phase.PhaseName}");
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Get current phase data
        /// </summary>
        public BossPhaseData GetCurrentPhase(BossBattleState state, BossMechanicsData bossData)
        {
            if (bossData == null || bossData.Phases == null) return null;
            
            int phaseIndex = Mathf.Clamp(state.CurrentPhase - 1, 0, bossData.Phases.Count - 1);
            return bossData.Phases[phaseIndex];
        }
        
        /// <summary>
        /// Get phase attack multiplier
        /// </summary>
        public float GetPhaseAttackMultiplier(BossBattleState state, BossMechanicsData bossData)
        {
            var phase = GetCurrentPhase(state, bossData);
            if (phase == null) return 1.0f;
            
            return phase.AttackMultiplier;
        }
        
        /// <summary>
        /// Check if minions should spawn for current phase
        /// </summary>
        public List<string> GetPhaseMinionSpawns(BossBattleState state, BossMechanicsData bossData)
        {
            var phase = GetCurrentPhase(state, bossData);
            if (phase == null || phase.SpawnEnemies == null) return null;
            
            return phase.SpawnEnemies;
        }
        
        /// <summary>
        /// Apply phase-specific effects
        /// </summary>
        public void ApplyPhaseEffects(BossBattleState state, BossMechanicsData bossData)
        {
            var phase = GetCurrentPhase(state, bossData);
            if (phase == null) return;
            
            // Apply phase-specific stat modifications here
            // This could include attack speed, defense, special abilities, etc.
        }
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            // No persistent data needed
        }
    }
}
