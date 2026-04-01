using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Characters;
using ClawRPG.Scripts.AI;
using ClawRPG.Scripts.Systems.BossMechanics;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Boss AI Decision Maker using Behavior Tree
    /// Provides intelligent skill selection based on combat context
    /// REQ-164: Adds charge mechanism for high-threat abilities.
    /// </summary>
    public class BossDecisionMaker {
        private Boss _boss;
        private BTContext _context;
        private BTSelector _root;

        // Ability evaluation cache
        private Dictionary<string, float> _abilityScores = new Dictionary<string, float>();

        // REQ-164: Charge mechanism state
        private bool _isCharging = false;
        private float _chargeTimer = 0f;
        private float _chargeDuration = 0f;
        private string _chargingAbilityId = null;

        /// <summary>
        /// Fired when the boss selects an ability — payload carries the full intent data for UI display (REQ-160).
        /// </summary>
        public event Action<BossIntentData> OnIntentSelected;
        
        /// <summary>
        /// REQ-164: Fired when boss enters charge state. String = ability ID being charged.
        /// </summary>
        public event Action<string, float> OnBossCharging;
        
        /// <summary>
        /// REQ-164: Fired when boss charge is interrupted (player interrupt skill).
        /// </summary>
        public event Action<string> OnBossChargeInterrupted;
        
        public BossDecisionMaker(Boss boss) {
            _boss = boss;
            _context = new BTContext { Boss = boss };
            BuildBehaviorTree();

            // REQ-156-04: 订阅模式切换信号，模式变化时重置行为树强制重新决策
            BossEnrageManager.OnBossModeChanged += OnBossModeChanged;
        }

        /// <summary>
        /// REQ-156-04: 模式切换时更新上下文并重置行为树
        /// </summary>
        private void OnBossModeChanged(string battleInstanceId, int oldMode, int newMode)
        {
            _context.BossMode = newMode;
            // 重置行为树状态，强制下次 MakeDecision 重新评估
            _root?.Reset();
            GD.Print($"[BossDecisionMaker] Mode changed to {(newMode == 1 ? "Enraged" : "Strategic")}, tree reset for re-evaluation");
        }
        
        /// <summary>
        /// Build the behavior tree for Boss AI decision making
        /// </summary>
        private void BuildBehaviorTree() {
            // Root selector: Priority-based decision making
            _root = new BTSelector(
                // Priority 0: ENRAGED MODE - BossMode == 1 covers both HP-rage and time-enrage (REQ-156)
                // ModeChanged signal fires for both BossPhaseSystem.BossEnraged and BossRageTriggered
                new BTSequence(
                    new BTCondition(ctx => ctx.BossMode == 1),
                    CreateEnragedModeEvaluation()
                ),

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
        /// Evaluate and make decision.
        /// REQ-164: If selected ability needs charging, enters charge state instead of immediate execution.
        /// </summary>
        public void MakeDecision(Character target, float deltaTime) {
            // Update context
            _context.Target = target;
            _context.DeltaTime = deltaTime;
            _context.SelectedAbility = null;
            _context.DesiredState = BossAIState.Idle;
            _context.Score = 0;
            _context.TimeSinceLastAbility += deltaTime;
            
            // Execute behavior tree (REQ-165: now scores first, then executes)
            _root.Reset();
            _root.Execute(_context);
            
            // REQ-164: Check if selected ability needs charging
            string selectedAbility = _context.SelectedAbility;
            if (selectedAbility != null && NeedsCharge(selectedAbility)) {
                // Enter charge state — do NOT call ApplyDecision() immediately
                _isCharging = true;
                _chargeDuration = GetChargeDuration(selectedAbility);
                _chargeTimer = 0f;
                _chargingAbilityId = selectedAbility;
                
                // Emit charging signal so UI can show charge animation + countdown
                OnBossCharging?.Invoke(selectedAbility, _chargeDuration);
                
                // Emit intent (REQ-160) for the UI to display what the boss is charging
                var intentData = BuildIntentData(selectedAbility);
                OnIntentSelected?.Invoke(intentData);
            } else {
                // No charge needed — execute immediately
                ApplyDecision();
            }
        }
        
        /// <summary>
        /// REQ-164: Called from boss's _Process() to update charge timer.
        /// </summary>
        public void Update(float delta) {
            if (!_isCharging) return;
            
            _chargeTimer += delta;
            
            if (_chargeTimer >= _chargeDuration) {
                // Charge complete — execute the ability
                _isCharging = false;
                _chargeTimer = 0f;
                ApplyDecision();
            }
        }
        
        /// <summary>
        /// REQ-164: Interrupt the current charge. Called by player interrupt skills.
        /// Returns true if there was an active charge to interrupt.
        /// </summary>
        public bool InterruptCharge() {
            if (!_isCharging) return false;
            
            string interruptedAbility = _chargingAbilityId;
            _isCharging = false;
            _chargeTimer = 0f;
            _chargingAbilityId = null;
            
            OnBossChargeInterrupted?.Invoke(interruptedAbility);
            GD.Print($"[BossDecisionMaker] Charge interrupted: {interruptedAbility}");
            return true;
        }
        
        /// <summary>
        /// REQ-164: Returns true if the ability needs a charge window.
        /// High-threat abilities (AoE, Heal, Shield, Summon) require charge.
        /// </summary>
        private bool NeedsCharge(string abilityId) {
            if (string.IsNullOrEmpty(abilityId)) return false;
            
            string lower = abilityId.ToLowerInvariant();
            
            // Abilities that always need charge (high threat, player can counter)
            string[] chargeKeywords = { "aoe", "fireball", "heal", "shield", "summon", "enrage", "mass", "ultimate" };
            foreach (var kw in chargeKeywords) {
                if (lower.Contains(kw)) return true;
            }
            
            // Check if the ability has AoE flag
            var abilityDb = _boss.GetAbilityDatabase();
            if (abilityDb.TryGetValue(abilityId, out var ability)) {
                if (ability.IsAoE) return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// REQ-164: Get charge duration for an ability.
        /// </summary>
        private float GetChargeDuration(string abilityId) {
            if (string.IsNullOrEmpty(abilityId)) return 0f;
            
            string lower = abilityId.ToLowerInvariant();
            
            // Different charge durations based on threat level
            if (lower.Contains("ultimate") || lower.Contains("enrage")) return 3.0f;  // Longest for ultimate abilities
            if (lower.Contains("heal")) return 2.0f;                                  // 2s for healing
            if (lower.Contains("shield")) return 1.5f;                               // 1.5s for shielding
            if (lower.Contains("summon")) return 1.5f;                               // 1.5s for summoning
            if (lower.Contains("aoe") || lower.Contains("fireball") || lower.Contains("mass")) return 1.0f; // 1s for AoE
            
            // Check AoE flag for default
            var abilityDb = _boss.GetAbilityDatabase();
            if (abilityDb.TryGetValue(abilityId, out var ability)) {
                if (ability.IsAoE) return 1.0f;
            }
            
            return 0f; // No charge for normal abilities
        }
        
        /// <summary>
        /// Apply the decision to the Boss.
        /// Note: Intent is now emitted in BTScoreSelector.OnExecute() during the score phase (REQ-165).
        /// ApplyDecision only handles the actual ability execution.
        /// </summary>
        private void ApplyDecision() {
            if (_context.SelectedAbility != null) {
                // REQ-165: Intent is emitted in OnExecute() score phase.
                // Only execute the ability here — no duplicate intent emission.
                _boss.TryUseAbility(_context.SelectedAbility);
                _context.TimeSinceLastAbility = 0;
            }

            // Apply state change
            if (_context.DesiredState != BossAIState.Idle) {
                _boss.ForceSetAIState(_context.DesiredState);
            }
        }

        /// <summary>
        /// REQ-160: Construct BossIntentData from a selected ability ID.
        /// </summary>
        private BossIntentData BuildIntentData(string abilityId) {
            var data = new BossIntentData {
                AbilityId = abilityId,
                IntentType = BossIntentType.Damage
            };

            var abilityDb = _boss.GetAbilityDatabase();
            if (abilityDb != null && abilityDb.TryGetValue(abilityId, out var ability)) {
                data.AbilityName = ability.AbilityName;

                // Try to classify via BossSkillType if available on the ability
                if (ability is BossAbility ba) {
                    // Infer intent type from ability characteristics
                    if (ba.IsAoE || ba.AoERadius > 0) {
                        data.IsAoE = true;
                    }
                    // Classify based on ability name heuristics (covers all current abilities)
                    data.IntentType = ClassifyIntentFromAbility(ba);
                    data.MinDamage = (int)(ba.DamageMultiplier * _boss.AttackDamage);
                    data.MaxDamage = (int)(ba.DamageMultiplier * _boss.AttackDamage * 1.3f); // +30% variance
                } else {
                    data.AbilityName = ability.AbilityName ?? abilityId;
                    data.MinDamage = 0;
                    data.MaxDamage = 0;
                }
            } else {
                data.AbilityName = abilityId;
                data.MinDamage = 0;
                data.MaxDamage = 0;
            }

            // Check enrage state
            data.IsEnraged = _boss.IsEnraged();

            return data;
        }

        private BossIntentType ClassifyIntentFromAbility(BossAbility ability) {
            var name = ability.AbilityName.ToLowerInvariant();

            if (name.Contains("heal") || name.Contains("shield") || name.Contains("buff"))
                return BossIntentType.Buff;
            if (name.Contains("curse") || name.Contains("poison") || name.Contains("debuff") || name.Contains("slow"))
                return BossIntentType.Debuff;
            if (name.Contains("teleport") || name.Contains("retreat"))
                return BossIntentType.Defend;
            if (name.Contains("summon") || name.Contains("enrage"))
                return BossIntentType.Special;
            return BossIntentType.Damage;
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
        /// Create Rage mode evaluation - aggressive rapid attacks (REQ-127)
        /// </summary>
        /// <summary>
        /// REQ-156: Enraged 模式行为评估
        /// - 攻击间隔缩短50%（BossAI层面处理）
        /// - 攻击完全随机化（BossAI.SelectSkill 处理）
        /// - 行为树层面：高风险高回报动作优先
        /// </summary>
        private BTNode CreateEnragedModeEvaluation() {
            return new BTScoreSelector()
                .AddScoringNode(new BTScoringAction(ctx => {
                    // Enraged专属攻击优先（enraged_burst, rapid_fire, desperate_strike）
                    var enragedAbilities = new[] { "enraged_burst", "rapid_fire", "desperate_strike" };

                    foreach (var ability in enragedAbilities) {
                        if (!IsAbilityReady(ability)) continue;

                        float score = 400f; // 最高优先级

                        var abilityData = _boss.GetAbilityDatabase().GetValueOrDefault(ability);
                        if (abilityData != null) {
                            score += abilityData.DamageMultiplier * 100f;
                        }

                        if (score > ctx.Score) {
                            ctx.Score = score;
                            ctx.SelectedAbility = ability;
                        }
                    }
                }))
                .AddScoringNode(new BTScoringAction(ctx => {
                    // In rage mode, prioritize devastating abilities
                    var rageAbilities = new[] { "fire_breath", "dark_bolt", "lightning_chain", "ground_slam" };

                    foreach (var ability in rageAbilities) {
                        if (!IsAbilityReady(ability)) continue;

                        float score = 300f; // High base priority

                        var abilityData = _boss.GetAbilityDatabase().GetValueOrDefault(ability);
                        if (abilityData != null) {
                            score += abilityData.DamageMultiplier * 80f;
                            // Prefer AoE in rage
                            if (abilityData.IsAoE) score += 100f;
                            // Prefer high damage
                            if (abilityData.DamageMultiplier >= 2.0f) score += 80f;
                        }

                        // Closer = better for melee
                        if (ctx.DistanceToTarget < 200f) score += 50f;

                        if (score > ctx.Score) {
                            ctx.Score = score;
                            ctx.SelectedAbility = ability;
                        }
                    }
                }))
                .AddScoringNode(new BTScoringAction(ctx => {
                    // Fallback: rapid basic attacks (乱拍感觉)
                    var basicAbilities = new[] { "magic_missile", "ice_lance", "basic_attack" };
                    foreach (var ability in basicAbilities) {
                        if (!IsAbilityReady(ability)) continue;

                        float score = 200f;
                        if (score > ctx.Score) {
                            ctx.Score = score;
                            ctx.SelectedAbility = ability;
                        }
                    }
                }));
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
