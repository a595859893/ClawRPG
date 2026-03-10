using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Characters;
using ClawRPG.Scripts.AI;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Boss AI Decision Maker using Behavior Tree
    /// Provides intelligent skill selection based on combat context
    /// </summary>
    public class BossDecisionMaker {
        private Boss _boss;
        private BTContext _context;
        private BTSelector _root;
        
        // Ability evaluation cache
        private Dictionary<string, float> _abilityScores = new Dictionary<string, float>();
        
        public BossDecisionMaker(Boss boss) {
            _boss = boss;
            _context = new BTContext { Boss = boss };
            BuildBehaviorTree();
        }
        
        /// <summary>
        /// Build the behavior tree for Boss AI decision making
        /// </summary>
        private void BuildBehaviorTree() {
            // Root selector: Priority-based decision making
            _root = new BTSelector(
                // Priority 1: Emergency healing when low health
                new BTSequence(
                    new BTCondition(ctx => ctx.HealthPercent < 0.3f),
                    new BTScoreSelector()
                        .AddScoringNode(CreateHealEvaluation())
                ),
                
                // Priority 2: Retreat when very low health
                new BTSequence(
                    new BTCondition(ctx => ctx.HealthPercent < 0.2f),
                    new BTAction(ctx => ctx.DesiredState = BossAIState.Retreating)
                ),
                
                // Priority 3: Use AoE abilities when multiple targets
                new BTSequence(
                    new BTCondition(ctx => HasMultipleTargets()),
                    new BTScoreSelector()
                        .AddScoringNode(CreateAoEEvaluation())
                ),
                
                // Priority 4: Use single-target abilities
                new BTScoreSelector()
                    .AddScoringNode(CreateDamageEvaluation())
                    .AddScoringNode(CreateControlEvaluation())
                    .AddScoringNode(CreateDebuffEvaluation()),
                
                // Priority 5: Movement abilities
                new BTScoreSelector()
                    .AddScoringNode(CreateTeleportEvaluation())
                    .AddScoringNode(CreateSummonEvaluation()),
                
                // Priority 6: Default attack
                new BTAction(ctx => ctx.SelectedAbility = null)
            );
        }
        
        /// <summary>
        /// Evaluate and make decision
        /// </summary>
        public void MakeDecision(Character target, float deltaTime) {
            // Update context
            _context.Target = target;
            _context.DeltaTime = deltaTime;
            _context.SelectedAbility = null;
            _context.DesiredState = BossAIState.Idle;
            _context.Score = 0;
            _context.TimeSinceLastAbility += deltaTime;
            
            // Execute behavior tree
            _root.Reset();
            _root.Execute(_context);
            
            // Apply decision
            ApplyDecision();
        }
        
        /// <summary>
        /// Apply the decision to the Boss
        /// </summary>
        private void ApplyDecision() {
            if (_context.SelectedAbility != null) {
                _boss.TryUseAbility(_context.SelectedAbility);
                _context.TimeSinceLastAbility = 0;
            }
            
            // Apply state change
            if (_context.DesiredState != BossAIState.Idle) {
                _boss.ForceSetState(_context.DesiredState);
            }
        }
        
        private bool HasMultipleTargets() {
            var players = _boss.GetTree().GetNodesInGroup("player");
            return players.Count > 1;
        }
        
        #region Ability Evaluations
        
        /// <summary>
        /// Create healing ability evaluation
        /// </summary>
        private BTNode CreateHealEvaluation() {
            return new BTScoringAction(ctx => {
                if (!IsAbilityReady("heal")) return;
                
                // High score if health is very low
                float score = (0.3f - ctx.HealthPercent) * 1000f;
                score += 200f; // Base priority for healing
                
                // Prefer healing in early phases
                if (ctx.CurrentPhase < 3) score += 100f;
                
                // Don't heal if just used ability recently
                if (ctx.TimeSinceLastAbility < 5f) score -= 300f;
                
                if (score > ctx.Score) {
                    ctx.Score = score;
                    ctx.SelectedAbility = "heal";
                }
            });
        }
        
        /// <summary>
        /// Create AoE ability evaluation
        /// </summary>
        private BTNode CreateAoEEvaluation() {
            return new BTScoringAction(ctx => {
                var aoeAbilities = new[] { "fire_breath", "lightning_chain", "poison_cloud", 
                                           "ground_slam", "fear_shout", "bleed_wave" };
                
                foreach (var ability in aoeAbilities) {
                    if (!IsAbilityReady(ability)) continue;
                    
                    float score = 150f;
                    
                    // Higher score for phase 2+
                    if (ctx.CurrentPhase >= 2) score += 50f;
                    
                    // Enraged increases AoE preference
                    if (ctx.IsEnraged) score += 100f;
                    
                    // Prefer fire/lightning in later phases
                    if (ctx.CurrentPhase >= 3) {
                        if (ability == "fire_breath" || ability == "lightning_chain") {
                            score += 50f;
                        }
                    }
                    
                    if (score > ctx.Score) {
                        ctx.Score = score;
                        ctx.SelectedAbility = ability;
                    }
                }
            });
        }
        
        /// <summary>
        /// Create damage-focused ability evaluation
        /// </summary>
        private BTNode CreateDamageEvaluation() {
            return new BTScoringAction(ctx => {
                var damageAbilities = new[] { "dark_bolt", "magic_missile", "ice_lance" };
                
                foreach (var ability in damageAbilities) {
                    if (!IsAbilityReady(ability)) continue;
                    
                    float score = 100f;
                    var abilityData = _boss.GetAbilityDatabase().GetValueOrDefault(ability);
                    
                    // Higher damage multiplier = higher score
                    if (abilityData != null) {
                        score += abilityData.DamageMultiplier * 50f;
                    }
                    
                    // Prefer long-range abilities when far from target
                    if (ctx.DistanceToTarget > 200f && abilityData != null) {
                        score += abilityData.Range / 10f;
                    }
                    
                    // Prefer high damage in enraged state
                    if (ctx.IsEnraged) score += 50f;
                    
                    // Prefer specific abilities based on phase
                    if (ctx.CurrentPhase == 1 && ability == "magic_missile") {
                        score += 30f;
                    }
                    if (ctx.CurrentPhase == 3 && ability == "dark_bolt") {
                        score += 50f;
                    }
                    
                    if (score > ctx.Score) {
                        ctx.Score = score;
                        ctx.SelectedAbility = ability;
                    }
                }
            });
        }
        
        /// <summary>
        /// Create control/stun ability evaluation
        /// </summary>
        private BTNode CreateControlEvaluation() {
            return new BTScoringAction(ctx => {
                var controlAbilities = new[] { "ground_slam", "fear_shout", "ice_lance" };
                
                foreach (var ability in controlAbilities) {
                    if (!IsAbilityReady(ability)) continue;
                    
                    float score = 80f;
                    
                    // Higher score if target is close (more likely to hit)
                    if (ctx.DistanceToTarget < 150f) {
                        score += 60f;
                    }
                    
                    // Ground slam better when multiple targets
                    if (ability == "ground_slam" && HasMultipleTargets()) {
                        score += 80f;
                    }
                    
                    // Fear better in phase 3
                    if (ability == "fear_shout" && ctx.CurrentPhase >= 3) {
                        score += 50f;
                    }
                    
                    if (score > ctx.Score) {
                        ctx.Score = score;
                        ctx.SelectedAbility = ability;
                    }
                }
            });
        }
        
        /// <summary>
        /// Create debuff ability evaluation
        /// </summary>
        private BTNode CreateDebuffEvaluation() {
            return new BTScoringAction(ctx => {
                var debuffAbilities = new[] { "poison_cloud", "bleed_wave" };
                
                foreach (var ability in debuffAbilities) {
                    if (!IsAbilityReady(ability)) continue;
                    
                    float score = 60f;
                    
                    // Prefer poison for sustained damage
                    if (ability == "poison_cloud") {
                        score += 40f;
                    }
                    
                    // Prefer bleed for high-damage phases
                    if (ability == "bleed_wave" && (ctx.CurrentPhase >= 2 || ctx.IsEnraged)) {
                        score += 50f;
                    }
                    
                    if (score > ctx.Score) {
                        ctx.Score = score;
                        ctx.SelectedAbility = ability;
                    }
                }
            });
        }
        
        /// <summary>
        /// Create teleport ability evaluation
        /// </summary>
        private BTNode CreateTeleportEvaluation() {
            return new BTScoringAction(ctx => {
                if (!IsAbilityReady("teleport")) return;
                
                float score = 30f;
                
                // Teleport if target is too close (melee threat)
                if (ctx.DistanceToTarget < 80f) {
                    score += 100f;
                }
                
                // Teleport if stuck (no movement for a while)
                // Would need additional tracking for this
                
                if (score > ctx.Score) {
                    ctx.Score = score;
                    ctx.SelectedAbility = "teleport";
                }
            });
        }
        
        /// <summary>
        /// Create summon ability evaluation
        /// </summary>
        private BTNode CreateSummonEvaluation() {
            return new BTScoringAction(ctx => {
                if (!IsAbilityReady("summon_minions")) return;
                
                float score = 20f;
                
                // Summon more in later phases
                score += ctx.CurrentPhase * 20f;
                
                // Summon when low on minions
                // Would need to track minion count
                
                // Don't summon too often
                if (ctx.TimeSinceLastAbility < 15f) {
                    score -= 50f;
                }
                
                if (score > ctx.Score) {
                    ctx.Score = score;
                    ctx.SelectedAbility = "summon_minions";
                }
            });
        }
        
        #endregion
        
        /// <summary>
        /// Check if an ability is ready to use
        /// </summary>
        private bool IsAbilityReady(string abilityId) {
            float cooldown = _boss.GetAbilityCooldown(abilityId);
            return cooldown <= 0;
        }
    }
}
