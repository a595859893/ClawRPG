using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Characters;

namespace ClawRPG.Scripts.AI {
    /// <summary>
    /// Enemy Perception System - simulates enemy senses
    /// Based on Advanced Game AI Patterns - Perception Systems
    /// </summary>
    public class EnemyPerceptionSystem {
        // Perception types
        public enum PerceptionType {
            Visual,    // Vision cone
            Auditory,  // Hearing player attacks
            Range      // Proximity detection
        }
        
        // Detection result
        public struct PerceptionResult {
            public bool CanSeePlayer;
            public bool CanHearPlayer;
            public bool IsPlayerInRange;
            public float DistanceToPlayer;
            public Vector2 DirectionToPlayer;
            public float VisionAngle; // Angle from enemy forward
        }
        
        // Configuration
        private float _visionRange = 300f;
        private float _visionAngle = 90f; // degrees
        private float _hearingRange = 200f;
        private float _detectionRange = 350f;
        
        // References
        private Enemy _enemy;
        private Player _player;
        
        public EnemyPerceptionSystem(Enemy enemy) {
            _enemy = enemy;
            _player = enemy.GetTarget();
        }
        
        /// <summary>
        /// Update perception ranges based on enemy stats
        /// </summary>
        public void Configure(float visionRange, float visionAngle, float hearingRange, float detectionRange) {
            _visionRange = visionRange;
            _visionAngle = visionAngle;
            _hearingRange = hearingRange;
            _detectionRange = detectionRange;
        }
        
        /// <summary>
        /// Update all perceptions and return combined result
        /// </summary>
        public PerceptionResult UpdatePerceptions(float deltaTime) {
            var result = new PerceptionResult();
            
            if (_player == null || _player.IsDead) {
                return result;
            }
            
            // Calculate distance and direction
            result.DistanceToPlayer = _enemy.GlobalPosition.DistanceTo(_player.GlobalPosition);
            result.DirectionToPlayer = (_player.GlobalPosition - _enemy.GlobalPosition).Normalized();
            
            // Range perception (always active)
            result.IsPlayerInRange = result.DistanceToPlayer <= _detectionRange;
            
            // Visual perception
            result.CanSeePlayer = CheckVisualPerception(result.DistanceToPlayer, result.DirectionToPlayer);
            
            // Auditory perception
            result.CanHearPlayer = CheckAuditoryPerception(result.DistanceToPlayer);
            
            // Calculate vision angle relative to enemy facing direction
            Vector2 enemyFacing = _enemy.GetNode<Sprite2D>("Sprite2D").FlipH ? Vector2.Left : Vector2.Right;
            result.VisionAngle = result.DirectionToPlayer.AngleTo(enemyFacing) * Mathf.RadToDeg;
            
            return result;
        }
        
        /// <summary>
        /// Check if player is visible (within vision cone)
        /// </summary>
        private bool CheckVisualPerception(float distance, Vector2 direction) {
            if (distance > _visionRange) return false;
            
            // Check if player is behind a wall (raycast)
            var spaceState = _enemy.GetWorld2d().DirectSpaceState;
            var query = PhysicsRayQueryParameters2D.Create(_enemy.GlobalPosition, _player.GlobalPosition);
            var result = spaceState.IntersectRay(query);
            
            // If ray hits something before player, vision blocked
            if (result.Count > 0) {
                var collider = result["collider"];
                if (collider is StaticBody2D wall) {
                    // Check if wall is between enemy and player
                    float wallDist = _enemy.GlobalPosition.DistanceTo((Vector2)result["position"]);
                    if (wallDist < distance - 10f) { // 10f tolerance
                        return false;
                    }
                }
            }
            
            // Check angle
            Vector2 enemyFacing = _enemy.GetNode<Sprite2D>("Sprite2D").FlipH ? Vector2.Left : Vector2.Right;
            float angle = direction.AngleTo(enemyFacing) * Mathf.RadToDeg;
            
            return Mathf.Abs(angle) <= _visionAngle / 2f;
        }
        
        /// <summary>
        /// Check if player can be heard
        /// </summary>
        private bool CheckAuditoryPerception(float distance) {
            if (distance > _hearingRange) return false;
            
            // Check if player is attacking (making noise)
            // For simplicity, assume player is always potentially audible within range
            // In a full implementation, would check if player is attacking/casting
            
            return true;
        }
    }
    
    /// <summary>
    /// Enemy Tactical AI Decision
    /// </summary>
    public enum TacticalDecision {
        None,
        Chase,           // Move towards player
        Attack,          // Attack player
        Retreat,         // Fall back
        Dodge,           // Evade player attack
        Block,           // Defend against attack
        MaintainDistance, // Keep optimal distance
        Flank,           // Move to side
        Circle,          // Circle around player
        Wait             // Pause briefly
    }
    
    /// <summary>
    /// Enemy Tactical AI Context - information for decision making
    /// </summary>
    public class TacticalContext {
        public EnemyPerceptionSystem.PerceptionResult Perception;
        public float HealthPercent;
        public float TimeSinceLastAttack;
        public float TimeSinceLastDodge;
        public int EnemiesNearby; // Number of allied enemies
        public bool IsPlayerAttacking;
        public float PlayerAttackCharge; // 0-1, how close player is to attacking
        public Vector2 EnemyPosition;
        public Vector2 PlayerPosition;
        public Vector2 BestRetreatPosition;
    }
    
    /// <summary>
    /// Enemy Tactical AI System
    /// Based on Advanced Game AI Patterns - Decision Systems (Utility-Based)
    /// </summary>
    public class EnemyTacticalAI {
        private Enemy _enemy;
        private EnemyPerceptionSystem _perception;
        
        // Tactical parameters
        private float _optimalAttackRange = 60f;
        private float _dodgeThreshold = 0.7f; // Dodge when player attack charge > this
        private float _retreatHealthThreshold = 0.25f; // Retreat when health < 25%
        private float _dodgeCooldown = 2f;
        
        // Decision weights
        private float _aggression = 0.5f; // 0 = defensive, 1 = aggressive
        private float _caution = 0.5f;    // 0 = reckless, 1 = cautious
        
        private float _lastDodgeTime = -10f;
        
        public EnemyTacticalAI(Enemy enemy) {
            _enemy = enemy;
            _perception = new EnemyPerceptionSystem(enemy);
            
            // Configure based on enemy stats
            ConfigureTactics();
        }
        
        /// <summary>
        /// Configure tactical parameters based on enemy type
        /// </summary>
        private void ConfigureTactics() {
            // Adjust based on enemy difficulty
            string enemyName = _enemy.EnemyName.ToLower();
            
            if (enemyName.ContainsKey("boss") || enemyName.ContainsKey("elite")) {
                _aggression = 0.7f;
                _caution = 0.6f;
                _optimalAttackRange = 80f;
            } else if (enemyName.ContainsKey("tank") || enemyName.ContainsKey("guardian")) {
                _aggression = 0.3f;
                _caution = 0.8f;
                _optimalAttackRange = 50f;
            } else if (enemyName.ContainsKey("assassin") || enemyName.ContainsKey("ranger")) {
                _aggression = 0.8f;
                _caution = 0.3f;
                _optimalAttackRange = 150f;
            }
        }
        
        /// <summary>
        /// Update perception and make tactical decisions
        /// </summary>
        public TacticalDecision Update(float deltaTime) {
            var perception = _perception.UpdatePerceptions(deltaTime);
            
            // Build tactical context
            var context = new TacticalContext {
                Perception = perception,
                HealthPercent = (float)_enemy.CurrentHealth / _enemy.MaxHealth,
                TimeSinceLastAttack = 0, // Would track this
                TimeSinceLastDodge = Time.GetTicksMsec() / 1000f - _lastDodgeTime,
                IsPlayerAttacking = false, // Would check player state
                PlayerAttackCharge = 0f, // Would check player charge
                EnemyPosition = _enemy.GlobalPosition,
                PlayerPosition = perception.IsPlayerInRange && _enemy.GetTarget() != null 
                    ? _enemy.GetTarget().GlobalPosition 
                    : Vector2.Zero
            };
            
            // Make decision using utility-based system
            return MakeDecision(context);
        }
        
        /// <summary>
        /// Make tactical decision based on context (Utility-Based AI)
        /// </summary>
        private TacticalDecision MakeDecision(TacticalContext context) {
            if (!context.Perception.IsPlayerInRange) {
                return TacticalDecision.None;
            }
            
            float distance = context.Perception.DistanceToPlayer;
            
            // Check if should retreat (low health)
            if (context.HealthPercent < _retreatHealthThreshold && _caution > 0.3f) {
                // Find retreat position
                Vector2 retreatDir = (context.EnemyPosition - context.PlayerPosition).Normalized();
                context.BestRetreatPosition = context.EnemyPosition + retreatDir * 100f;
                return TacticalDecision.Retreat;
            }
            
            // Check if should dodge (player attacking)
            if (context.PlayerAttackCharge > _dodgeThreshold && 
                context.TimeSinceLastDodge > _dodgeCooldown &&
                distance < _optimalAttackRange * 2f) {
                _lastDodgeTime = Time.GetTicksMsec() / 1000f;
                return TacticalDecision.Dodge;
            }
            
            // Decision based on distance
            if (distance > _optimalAttackRange * 2f) {
                // Too far - chase
                return TacticalDecision.Chase;
            } else if (distance < _optimalAttackRange * 0.5f) {
                // Too close - back off or circle
                if (_aggression > 0.6f) {
                    return TacticalDecision.Circle;
                } else {
                    return TacticalDecision.Retreat;
                }
            } else if (distance <= _optimalAttackRange) {
                // In attack range
                if (_aggression > 0.4f) {
                    return TacticalDecision.Attack;
                } else {
                    return TacticalDecision.MaintainDistance;
                }
            } else {
                // Optimal range - choose tactic based on aggression
                float roll = GD.Randf();
                if (roll < _aggression * 0.3f) {
                    return TacticalDecision.Flank;
                } else if (roll < _aggression * 0.5f) {
                    return TacticalDecision.Circle;
                } else {
                    return TacticalDecision.MaintainDistance;
                }
            }
        }
        
        /// <summary>
        /// Get the perception system for external use
        /// </summary>
        public EnemyPerceptionSystem GetPerception() => _perception;
        
        /// <summary>
        /// Set tactical personality
        /// </summary>
        public void SetPersonality(float aggression, float caution) {
            _aggression = Mathf.Clamp(aggression, 0f, 1f);
            _caution = Mathf.Clamp(caution, 0f, 1f);
        }
    }
    
    /// <summary>
    /// Enemy State Extended - adds tactical behaviors
    /// Based on Advanced Game AI Patterns - FSM + Behavior Trees
    /// </summary>
    public class EnemyStateTacticalChase : EnemyState {
        private EnemyTacticalAI _tacticalAI;
        
        public EnemyStateTacticalChase(Enemy enemy, EnemyTacticalAI tacticalAI) : base(enemy) {
            _tacticalAI = tacticalAI;
        }
        
        public override void Update(float dt) {
            if (Enemy.GetTarget() == null) {
                Enemy.ChangeState(new EnemyStateIdle(Enemy));
                return;
            }
            
            // Get tactical decision
            var decision = _tacticalAI.Update(dt);
            
            float distance = Enemy.GlobalPosition.DistanceTo(Enemy.GetTarget().GlobalPosition);
            
            switch (decision) {
                case TacticalDecision.Attack:
                    if (distance <= Enemy.AttackRange) {
                        Enemy.ChangeState(new EnemyStateAttack(Enemy));
                    }
                    break;
                    
                case TacticalDecision.Retreat:
                    // Move away from player
                    Vector2 retreatDir = (Enemy.GlobalPosition - Enemy.GetTarget().GlobalPosition).Normalized();
                    Enemy.Velocity = retreatDir * Enemy.MoveSpeed * 0.7f;
                    break;
                    
                case TacticalDecision.Circle:
                    // Circle around player
                    Vector2 toPlayer = (Enemy.GetTarget().GlobalPosition - Enemy.GlobalPosition).Normalized();
                    Vector2 perpendicular = new Vector2(-toPlayer.y, toPlayer.x);
                    // Alternate direction based on time
                    float circleDir = Mathf.Sin(Time.GetTicksMsec() / 1000f) > 0 ? 1f : -1f;
                    Enemy.Velocity = perpendicular * circleDir * Enemy.MoveSpeed * 0.8f;
                    break;
                    
                case TacticalDecision.Flank:
                    // Move to side of player
                    Vector2 playerDir = (Enemy.GetTarget().GlobalPosition - Enemy.GlobalPosition).Normalized();
                    Vector2 flankDir = new Vector2(-playerDir.y, playerDir.x) * (GD.Randf() > 0.5f ? 1f : -1f);
                    Enemy.Velocity = (playerDir * 0.5f + flankDir * 0.5f).Normalized() * Enemy.MoveSpeed;
                    break;
                    
                case TacticalDecision.Dodge:
                    // Quick dodge in random direction
                    Vector2 dodgeDir = new Vector2(GD.Randf() - 0.5f, GD.Randf() - 0.5f).Normalized();
                    Enemy.Velocity = dodgeDir * Enemy.MoveSpeed * 1.5f;
                    // Would trigger dodge animation/effect
                    break;
                    
                case TacticalDecision.Chase:
                default:
                    // Standard chase
                    Enemy.MoveToTarget(Enemy.MoveSpeed);
                    break;
            }
            
            // Check if player escaped
            if (distance > Enemy.DetectionRange * 1.5f) {
                Enemy.ChangeState(new EnemyStateIdle(Enemy));
            } else if (distance <= Enemy.AttackRange) {
                Enemy.ChangeState(new EnemyStateAttack(Enemy));
            }
        }
    }
    
    /// <summary>
    /// Enemy Ability System - context-aware ability usage
    /// </summary>
    public class EnemyAbilitySystem {
        // Ability types
        public enum AbilityType {
            BasicAttack,
            SpecialAttack,
            Defensive,
            Movement,
            Debuff,
            Heal
        }
        
        // Ability info
        public class EnemyAbility {
            public string Name;
            public AbilityType Type;
            public float Cooldown;
            public float Range;
            public float DamageMultiplier;
            public string RequiredTag; // e.g., "fire", "ice"
        }
        
        private List<EnemyAbility> _abilities = new();
        private Dictionary<string, float> _cooldowns = new();
        private Random _rand = new Random();
        
        public EnemyAbilitySystem() {
            InitializeDefaultAbilities();
        }
        
        /// <summary>
        /// Initialize default enemy abilities
        /// </summary>
        private void InitializeDefaultAbilities() {
            _abilities.Add(new EnemyAbility {
                Name = "Basic Attack",
                Type = AbilityType.BasicAttack,
                Cooldown = 1f,
                Range = 50f,
                DamageMultiplier = 1f
            });
            
            _abilities.Add(new EnemyAbility {
                Name = "Heavy Strike",
                Type = AbilityType.SpecialAttack,
                Cooldown = 3f,
                Range = 60f,
                DamageMultiplier = 1.5f
            });
            
            _abilities.Add(new EnemyAbility {
                Name = "Power Slash",
                Type = AbilityType.SpecialAttack,
                Cooldown = 4f,
                Range = 80f,
                DamageMultiplier = 2f
            });
            
            _abilities.Add(new EnemyAbility {
                Name = "Shield Block",
                Type = AbilityType.Defensive,
                Cooldown = 5f,
                Range = 0f,
                DamageMultiplier = 0f
            });
            
            _abilities.Add(new EnemyAbility {
                Name = "Quick Dash",
                Type = AbilityType.Movement,
                Cooldown = 3f,
                Range = 100f,
                DamageMultiplier = 0.3f
            });
            
            _abilities.Add(new EnemyAbility {
                Name = "Poison Cloud",
                Type = AbilityType.Debuff,
                Cooldown = 8f,
                Range = 100f,
                DamageMultiplier = 0.8f
            });
            
            _abilities.Add(new EnemyAbility {
                Name = "Regenerate",
                Type = AbilityType.Heal,
                Cooldown = 10f,
                Range = 0f,
                DamageMultiplier = 0f
            });
        }
        
        /// <summary>
        /// Select best ability based on tactical context
        /// </summary>
        public EnemyAbility SelectAbility(float healthPercent, float distanceToPlayer, bool playerAttacking) {
            float currentTime = Time.GetTicksMsec() / 1000f;
            
            // Filter abilities that are off cooldown and in range
            var availableAbilities = _abilities.FindAll(a => {
                if (_cooldowns.TryGetValue(a.Name, out float cooldownEnd)) {
                    if (currentTime < cooldownEnd) return false;
                }
                return distanceToPlayer <= a.Range;
            });
            
            if (availableAbilities.Count == 0) {
                return _abilities[0]; // Default to basic attack
            }
            
            // Score each ability
            float bestScore = float.MinValue;
            EnemyAbility bestAbility = availableAbilities[0];
            
            foreach (var ability in availableAbilities) {
                float score = ScoreAbility(ability, healthPercent, distanceToPlayer, playerAttacking);
                
                if (score > bestScore) {
                    bestScore = score;
                    bestAbility = ability;
                }
            }
            
            // Apply cooldown
            _cooldowns[bestAbility.Name] = currentTime + bestAbility.Cooldown;
            
            return bestAbility;
        }
        
        /// <summary>
        /// Score an ability based on tactical context
        /// </summary>
        private float ScoreAbility(EnemyAbility ability, float healthPercent, float distanceToPlayer, bool playerAttacking) {
            float score = 0f;
            
            switch (ability.Type) {
                case AbilityType.BasicAttack:
                    score = 50f; // Always useful when in range
                    break;
                    
                case AbilityType.SpecialAttack:
                    // Prefer when player is close and not attacking
                    score = 30f + (distanceToPlayer < 70f ? 20f : 0f) + (playerAttacking ? -10f : 10f);
                    break;
                    
                case AbilityType.Defensive:
                    // Prefer when low health or player attacking
                    score = healthPercent < 0.3f ? 60f : (playerAttacking ? 40f : 10f);
                    break;
                    
                case AbilityType.Movement:
                    // Prefer when need to reposition
                    score = distanceToPlayer > 100f ? 30f : 15f;
                    break;
                    
                case AbilityType.Debuff:
                    // Prefer in extended fights
                    score = 25f;
                    break;
                    
                case AbilityType.Heal:
                    // Only when low health
                    score = healthPercent < 0.4f ? 70f : 5f;
                    break;
            }
            
            // Add randomness
            score += (_rand.NextDouble() * 10f - 5f);
            
            return score;
        }
        
        /// <summary>
        /// Add custom ability
        /// </summary>
        public void AddAbility(EnemyAbility ability) {
            _abilities.Add(ability);
        }
    }
}
