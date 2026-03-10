using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.Characters {
    /// <summary>
    /// Boss special ability data
    /// </summary>
    [System.Serializable]
    public class BossAbility
    {
        public string AbilityId;
        public string AbilityName;
        public string Description;
        public float Cooldown;
        public float DamageMultiplier;
        public float Range;
        public float Duration;
        public StatusEffectType? ApplyStatus;
        public float StatusChance;
        public bool IsAoE;
        public float AoERadius;
        
        public BossAbility(string id, string name, string desc, float cooldown, float dmgMult, float range = 150f)
        {
            AbilityId = id;
            AbilityName = name;
            Description = desc;
            Cooldown = cooldown;
            DamageMultiplier = dmgMult;
            Range = range;
            Duration = 0f;
            IsAoE = false;
            AoERadius = 0f;
            StatusChance = 0f;
        }
    }
    
    /// <summary>
    /// Boss AI behavior state
    /// </summary>
    public enum BossAIState
    {
        Idle,
        Chasing,
        Attacking,
        UsingAbility,
        retreating,
        Stunned
    }
    
    /// <summary>
    /// Boss enemy with multiple phases and special abilities - Enhanced version
    /// </summary>
    public partial class Boss : Enemy
    {
        [ExportGroup("Boss Properties")]
        [Export] public string BossTitle = "Ancient Dragon";
        [Export] public int PhaseCount = 3;
        [Export] public float EnrageTime = 120f;
        
        [ExportGroup("Phase Settings")]
        [Export] public int[] PhaseHealthThresholds = { 66, 33 };
        
        [ExportGroup("Special Abilities")]
        [Export] public float AbilityCooldown = 10f;
        [Export] public string[] SpecialAbilities;
        
        // Enhanced ability system
        private Dictionary<string, BossAbility> _abilityDatabase;
        private Dictionary<string, float> _abilityCurrentCooldowns;
        private List<string> _availableAbilities;
        
        // AI State
        private BossAIState _aiState = BossAIState.Idle;
        private float _stateTimer;
        private float _retreatThreshold = 0.3f; // Retreat when health below 30%
        private float _wanderRadius = 50f;
        private Vector2 _wanderTarget;
        
        // Enhanced combat
        private float _attackRange = 80f;
        private float _chaseRange = 400f;
        private float _predictTargetTime = 0.2f;
        
        // Visual effects
        private BossAbilityVisualizer _visualizer;
        
        // State
        private int _currentPhase = 1;
        private float _abilityTimer;
        private float _enrageTimer;
        private bool _isEnraged;
        private bool _phaseTransitioning;
        
        // Events
        public event Action<int> OnPhaseChange;
        public event Action OnEnrage;
        public event Action<string> OnSpecialAbility;
        public event Action<BossAIState> OnAIStateChanged;
        
        public override void _Ready()
        {
            base._Ready();
            
            InitializeAbilityDatabase();
            
            // Initialize visualizer
            _visualizer = BossAbilityVisualizer.Instance;
            
            _abilityTimer = 5f;
            _enrageTimer = EnrageTime;
            _stateTimer = 0f;
            _availableAbilities = new List<string>();
            
            if (SpecialAbilities != null)
            {
                _availableAbilities.AddRange(SpecialAbilities);
            }
            
            GD.Print($"Boss {BossTitle} spawned! Phase: {_currentPhase}, Enrage: {EnrageTime}s");
        }
        
        private void InitializeAbilityDatabase()
        {
            _abilityDatabase = new Dictionary<string, BossAbility>();
            _abilityCurrentCooldowns = new Dictionary<string, float>();
            
            // Offensive abilities
            _abilityDatabase["fire_breath"] = new BossAbility("fire_breath", "火焰吐息", "喷射火焰造成持续伤害", 12f, 1.5f, 250f)
            {
                ApplyStatus = StatusEffectType.Burning,
                StatusChance = 0.8f,
                IsAoE = true,
                AoERadius = 100f,
                Duration = 5f
            };
            
            _abilityDatabase["lightning_chain"] = new BossAbility("lightning_chain", "闪电链", "连锁攻击多个目标", 15f, 1.2f, 200f)
            {
                IsAoE = true,
                AoERadius = 150f
            };
            
            _abilityDatabase["poison_cloud"] = new BossAbility("poison_cloud", "毒云", "释放毒云持续伤害", 18f, 0.8f, 180f)
            {
                ApplyStatus = StatusEffectType.Poisoned,
                StatusChance = 1f,
                IsAoE = true,
                AoERadius = 120f,
                Duration = 8f
            };
            
            _abilityDatabase["ice_lance"] = new BossAbility("ice_lance", "寒冰长矛", "快速冰冻攻击", 8f, 1.0f, 300f)
            {
                ApplyStatus = StatusEffectType.Frozen,
                StatusChance = 0.6f,
                Duration = 2f
            };
            
            _abilityDatabase["dark_bolt"] = new BossAbility("dark_bolt", "暗影箭", "暗影属性强力攻击", 10f, 1.8f, 200f)
            {
                ApplyStatus = StatusEffectType.Cursed,
                StatusChance = 0.5f,
                Duration = 5f
            };
            
            _abilityDatabase["ground_slam"] = new BossAbility("ground_slam", "地震猛击", "强力范围攻击", 14f, 2.0f, 100f)
            {
                IsAoE = true,
                AoERadius = 200f,
                StatusChance = 0.3f,
                ApplyStatus = StatusEffectType.Stunned,
                Duration = 1f
            };
            
            _abilityDatabase["fear_shout"] = new BossAbility("fear_shout", "恐惧咆哮", "使敌人恐惧逃跑", 20f, 0.5f, 250f)
            {
                IsAoE = true,
                AoERadius = 250f,
                ApplyStatus = StatusEffectType.Frozen, // Using Frozen as fear equivalent
                StatusChance = 0.7f,
                Duration = 3f
            };
            
            _abilityDatabase["bleed_wave"] = new BossAbility("bleed_wave", "鲜血波纹", "造成出血效果", 16f, 1.3f, 150f)
            {
                ApplyStatus = StatusEffectType.Bleeding,
                StatusChance = 0.9f,
                Duration = 6f,
                IsAoE = true,
                AoERadius = 180f
            };
            
            _abilityDatabase["magic_missile"] = new BossAbility("magic_missile", "奥术飞弹", "追踪魔法攻击", 6f, 0.9f, 250f);
            
            _abilityDatabase["heal"] = new BossAbility("heal", "自我治疗", "恢复生命值", 25f, 0f);
            
            _abilityDatabase["teleport"] = new BossAbility("teleport", "闪现", "瞬间移动位置", 18f, 0f);
            
            _abilityDatabase["summon_minions"] = new BossAbility("summon_minions", "召唤小怪", "召唤助手作战", 30f, 0f)
            {
                IsAoE = true,
                AoERadius = 100f
            };
            
            // Initialize cooldowns
            foreach (var ability in _abilityDatabase.Keys)
            {
                _abilityCurrentCooldowns[ability] = 0f;
            }
        }
        
        public override void _PhysicsProcess(double delta)
        {
            float dt = (float)delta;
            
            if (IsDead) return;
            
            // Update ability cooldowns
            UpdateAbilityCooldowns(dt);
            
            // Update enrage timer
            if (!_isEnraged)
            {
                _enrageTimer -= dt;
                if (_enrageTimer <= 0)
                {
                    TriggerEnrage();
                }
            }
            
            // Update AI state
            UpdateAIState(dt);
            
            // Update ability timer (for random abilities)
            _abilityTimer -= dt;
            if (_abilityTimer <= 0)
            {
                TryUseSpecialAbility();
                _abilityTimer = AbilityCooldown;
            }
            
            // Check for phase transition
            CheckPhaseTransition();
            
            base._PhysicsProcess(delta);
        }
        
        private void UpdateAbilityCooldowns(float dt)
        {
            foreach (var ability in _abilityCurrentCooldowns.Keys)
            {
                if (_abilityCurrentCooldowns[ability] > 0)
                {
                    _abilityCurrentCooldowns[ability] -= dt;
                }
            }
        }
        
        private void UpdateAIState(float dt)
        {
            _stateTimer -= dt;
            
            if (_aiState == BossAIState.Stunned && _stateTimer <= 0)
            {
                SetAIState(BossAIState.Idle);
                return;
            }
            
            if (_aiState == BossAIState.Retreating && _stateTimer <= 0)
            {
                SetAIState(BossAIState.Idle);
                return;
            }
            
            var target = GetTarget();
            if (target == null)
            {
                SetAIState(BossAIState.Idle);
                Wander(dt);
                return;
            }
            
            float distanceToTarget = GlobalPosition.DistanceTo(target.GlobalPosition);
            float healthPercent = (float)CurrentHealth / MaxHealth;
            
            // State machine
            switch (_aiState)
            {
                case BossAIState.Idle:
                    if (distanceToTarget > _chaseRange)
                    {
                        SetAIState(BossAIState.Chasing);
                    }
                    else if (distanceToTarget <= _attackRange)
                    {
                        SetAIState(BossAIState.Attacking);
                    }
                    break;
                    
                case BossAIState.Chasing:
                    if (distanceToTarget <= _attackRange)
                    {
                        SetAIState(BossAIState.Attacking);
                    }
                    else if (distanceToTarget > _chaseRange * 1.5f)
                    {
                        // Lost target, wander
                        SetAIState(BossAIState.Idle);
                    }
                    break;
                    
                case BossAIState.Attacking:
                    if (distanceToTarget > _attackRange * 1.5f)
                    {
                        SetAIState(BossAIState.Chasing);
                    }
                    break;
                    
                case BossAIState.UsingAbility:
                    if (_stateTimer <= 0)
                    {
                        SetAIState(BossAIState.Idle);
                    }
                    return; // Don't move while using ability
            }
            
            // Check for retreat at low health
            if (healthPercent < _retreatThreshold && _aiState != BossAIState.Retreating && _aiState != BossAIState.UsingAbility)
            {
                TryRetreat();
            }
            
            // Execute state behavior
            ExecuteStateBehavior(dt, target);
        }
        
        private void SetAIState(BossAIState newState)
        {
            if (_aiState == newState) return;
            
            _aiState = newState;
            _stateTimer = GetStateDuration(newState);
            OnAIStateChanged?.Invoke(newState);
        }
        
        private float GetStateDuration(BossAIState state)
        {
            switch (state)
            {
                case BossAIState.Idle: return 1f;
                case BossAIState.Chasing: return 2f;
                case BossAIState.Attacking: return 1.5f;
                case BossAIState.UsingAbility: return 3f;
                case BossAIState.Retreating: return 2f;
                case BossAIState.Stunned: return 2f;
                default: return 1f;
            }
        }
        
        private void ExecuteStateBehavior(float dt, Character target)
        {
            switch (_aiState)
            {
                case BossAIState.Idle:
                    Wander(dt);
                    break;
                    
                case BossAIState.Chasing:
                    ChaseTarget(target);
                    break;
                    
                case BossAIState.Attacking:
                    AttackTarget(target);
                    break;
                    
                case BossAIState.Retreating:
                    RetreatFromTarget(target);
                    break;
            }
        }
        
        private void Wander(float dt)
        {
            // Random movement when idle
            if (_stateTimer <= 0 || GlobalPosition.DistanceTo(_wanderTarget) < 10f)
            {
                _wanderTarget = GlobalPosition + new Vector2(
                    (float)GD.RandRange(-_wanderRadius, _wanderRadius),
                    (float)GD.RandRange(-_wanderRadius, _wanderRadius)
                );
            }
            
            MoveTo(_wanderTarget, MoveSpeed * 0.3f);
        }
        
        private void ChaseTarget(Character target)
        {
            // Predict target position for better chasing
            Vector2 predictedPos = target.GlobalPosition;
            if (target is Player player)
            {
                predictedPos += player.Velocity * _predictTargetTime;
            }
            
            MoveTo(predictedPos, MoveSpeed);
        }
        
        private void AttackTarget(Character target)
        {
            // Face target
            if (target.GlobalPosition.x < GlobalPosition.x)
                FaceDirection(-1);
            else
                FaceDirection(1);
            
            // Basic attack
            TryAttack();
        }
        
        private void RetreatFromTarget(Character target)
        {
            Vector2 retreatDir = (GlobalPosition - target.GlobalPosition).Normalized();
            Vector2 retreatPos = GlobalPosition + retreatDir * 200f;
            MoveTo(retreatPos, MoveSpeed * 0.8f);
        }
        
        private void TryRetreat()
        {
            SetAIState(BossAIState.Retreating);
            GD.Print($"{BossTitle} is retreating to recover!");
        }
        
        private void CheckPhaseTransition()
        {
            if (_phaseTransitioning) return;
            
            int healthPercent = (CurrentHealth * 100) / MaxHealth;
            
            for (int i = 0; i < PhaseHealthThresholds.Length; i++)
            {
                if (healthPercent <= PhaseHealthThresholds[i] && _currentPhase < i + 2)
                {
                    TransitionToPhase(i + 2);
                    break;
                }
            }
        }
        
        private void TransitionToPhase(int newPhase)
        {
            _phaseTransitioning = true;
            _currentPhase = newPhase;
            
            GD.Print($"{BossTitle} transitions to Phase {_currentPhase}!");
            
            // Visual feedback
            ShowPhaseTransitionEffect();
            
            // Increase difficulty
            MoveSpeed *= 1.2f;
            AttackDamage *= 1.3f;
            AttackCooldown *= 0.9f;
            
            // Unlock more abilities in later phases
            if (newPhase >= 2 && !_availableAbilities.Contains("lightning_chain"))
                _availableAbilities.Add("lightning_chain");
            if (newPhase >= 3 && !_availableAbilities.Contains("fear_shout"))
                _availableAbilities.Add("fear_shout");
            
            OnPhaseChange?.Invoke(_currentPhase);
            
            GetTree().CreateTimer(2f).Timeout += () => _phaseTransitioning = false;
        }
        
        private void TriggerEnrage()
        {
            _isEnraged = true;
            MoveSpeed *= 1.5f;
            AttackDamage *= 2f;
            AttackCooldown *= 0.7f;
            
            // Reduce ability cooldowns when enraged
            foreach (var ability in _abilityCurrentCooldowns.Keys)
            {
                _abilityCurrentCooldowns[ability] *= 0.5f;
            }
            
            GD.Print($"{BossTitle} is ENRAGED!");
            ShowEnrageEffect();
            
            OnEnrage?.Invoke();
        }
        
        private void TryUseSpecialAbility()
        {
            if (_availableAbilities == null || _availableAbilities.Count == 0) return;
            
            // Find available abilities (not on cooldown)
            List<string> readyAbilities = new List<string>();
            foreach (string ability in _availableAbilities)
            {
                if (_abilityCurrentCooldowns.ContainsKey(ability) && _abilityCurrentCooldowns[ability] <= 0)
                {
                    readyAbilities.Add(ability);
                }
            }
            
            if (readyAbilities.Count == 0) return;
            
            // Pick random ready ability
            string ability = readyAbilities[GD.Randi() % readyAbilities.Count];
            
            GD.Print($"{BossTitle} uses special ability: {ability}");
            
            // Set cooldown
            if (_abilityDatabase.ContainsKey(ability))
            {
                _abilityCurrentCooldowns[ability] = _abilityDatabase[ability].Cooldown;
            }
            
            SetAIState(BossAIState.UsingAbility);
            
            switch (ability)
            {
                case "fire_breath":
                    UseFireBreath();
                    break;
                case "lightning_chain":
                    UseLightningChain();
                    break;
                case "poison_cloud":
                    UsePoisonCloud();
                    break;
                case "ice_lance":
                    UseIceLance();
                    break;
                case "dark_bolt":
                    UseDarkBolt();
                    break;
                case "ground_slam":
                    UseGroundSlam();
                    break;
                case "fear_shout":
                    UseFearShout();
                    break;
                case "bleed_wave":
                    UseBleedWave();
                    break;
                case "magic_missile":
                    UseMagicMissile();
                    break;
                case "heal":
                    UseBossHeal();
                    break;
                case "teleport":
                    UseTeleport();
                    break;
                case "summon_minions":
                    UseSummonMinions();
                    break;
                default:
                    UseAreaAttack();
                    break;
            }
            
            OnSpecialAbility?.Invoke(ability);
        }
        
        // Enhanced ability implementations
        private void UseFireBreath()
        {
            var ability = _abilityDatabase["fire_breath"];
            Vector2 targetPos = GetTarget()?.GlobalPosition ?? GlobalPosition + new Vector2(100, 0);
            Vector2 direction = (targetPos - GlobalPosition).Normalized();
            float facingAngle = Mathf.Atan2(direction.Y, direction.X);
            
            // Play ability sound
            SoundEffectSystem.Instance?.PlayBossAbilityFireBreath();
            
            // Trigger visual effect
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("fire_breath", GlobalPosition, targetPos, facingAngle);
            }
            
            // Create area effect
            if (ability.IsAoE)
            {
                ApplyAoEDamage(ability);
            }
            
            // Apply burning status
            if (GetTarget() != null && ApplyStatusEffect(ability))
            {
                ApplyStatusToTarget(GetTarget(), ability.ApplyStatus.Value, ability.Duration);
            }
            
            GD.Print($"Fire breath! Direction: {direction}, AoE: {ability.AoERadius}");
        }
        
        private void UseLightningChain()
        {
            var ability = _abilityDatabase["lightning_chain"];
            Vector2 targetPos = GetTarget()?.GlobalPosition ?? GlobalPosition + new Vector2(100, 0);
            
            // Play ability sound
            SoundEffectSystem.Instance?.PlayBossAbilityLightningChain();
            
            // Trigger visual effect
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("lightning_chain", GlobalPosition, targetPos);
            }
            
            ApplyAoEDamage(ability);
            GD.Print("Lightning chain attack!");
        }
        
        private void UsePoisonCloud()
        {
            var ability = _abilityDatabase["poison_cloud"];
            Vector2 targetPos = GetTarget()?.GlobalPosition ?? GlobalPosition;
            
            // Play ability sound
            SoundEffectSystem.Instance?.PlayBossAbilityPoisonCloud();
            
            // Trigger visual effect
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("poison_cloud", GlobalPosition, targetPos);
            }
            
            ApplyAoEDamage(ability);
            
            if (GetTarget() != null && ApplyStatusEffect(ability))
            {
                ApplyStatusToTarget(GetTarget(), ability.ApplyStatus.Value, ability.Duration);
            }
            GD.Print("Poison cloud released!");
        }
        
        private void UseIceLance()
        {
            var ability = _abilityDatabase["ice_lance"];
            Vector2 targetPos = GetTarget()?.GlobalPosition ?? GlobalPosition + new Vector2(100, 0);
            Vector2 direction = (targetPos - GlobalPosition).Normalized();
            float facingAngle = Mathf.Atan2(direction.Y, direction.X);
            
            // Play ability sound
            SoundEffectSystem.Instance?.PlayBossAbilityIceLance();
            
            // Trigger visual effect
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("ice_lance", GlobalPosition, targetPos, facingAngle);
            }
            
            if (GetTarget() != null)
            {
                float dist = GlobalPosition.DistanceTo(GetTarget().GlobalPosition);
                if (dist <= ability.Range)
                {
                    int damage = (int)(AttackDamage * ability.DamageMultiplier);
                    GetTarget().TakeDamage(damage);
                    
                    if (ApplyStatusEffect(ability))
                    {
                        ApplyStatusToTarget(GetTarget(), ability.ApplyStatus.Value, ability.Duration);
                    }
                }
            }
            GD.Print("Ice lance fired!");
        }
        
        private void UseDarkBolt()
        {
            var ability = _abilityDatabase["dark_bolt"];
            Vector2 targetPos = GetTarget()?.GlobalPosition ?? GlobalPosition + new Vector2(100, 0);
            
            // Play ability sound
            SoundEffectSystem.Instance?.PlayBossAbilityShadowBolt();
            
            // Trigger visual effect
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("shadow_bolt", GlobalPosition, targetPos);
            }
            
            if (GetTarget() != null)
            {
                float dist = GlobalPosition.DistanceTo(GetTarget().GlobalPosition);
                if (dist <= ability.Range)
                {
                    int damage = (int)(AttackDamage * ability.DamageMultiplier);
                    GetTarget().TakeDamage(damage);
                    
                    if (ApplyStatusEffect(ability))
                    {
                        ApplyStatusToTarget(GetTarget(), ability.ApplyStatus.Value, ability.Duration);
                    }
                }
            }
            GD.Print("Dark bolt fired!");
        }
        
        private void UseGroundSlam()
        {
            var ability = _abilityDatabase["ground_slam"];
            Vector2 targetPos = GetTarget()?.GlobalPosition ?? GlobalPosition;
            
            // Play ability sound
            SoundEffectSystem.Instance?.PlayBossAbilityGroundSlam();
            
            // Trigger visual effect
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("ground_slam", GlobalPosition, targetPos);
            }
            
            ApplyAoEDamage(ability);
            
            // Add stun effect
            if (ApplyStatusEffect(ability))
            {
                // Stun nearby enemies
                var enemies = GetTree().GetNodesInGroup("enemy");
                foreach (Node node in enemies)
                {
                    if (node is Enemy enemy && GlobalPosition.DistanceTo(enemy.GlobalPosition) <= ability.AoERadius)
                    {
                        ApplyStatusToTarget(enemy, StatusEffectType.Stunned, ability.Duration);
                    }
                }
            }
            
            GD.Print("Ground slam! AoE damage and stun.");
        }
        
        private void UseFearShout()
        {
            var ability = _abilityDatabase["fear_shout"];
            
            // Play ability sound
            SoundEffectSystem.Instance?.PlayBossAbilityFearRoar();
            
            // Trigger visual effect
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("fear_roar", GlobalPosition, GlobalPosition);
            }
            
            ApplyAoEDamage(ability);
            GD.Print("Fear shout! Enemies terrified.");
        }
        
        private void UseBleedWave()
        {
            var ability = _abilityDatabase["bleed_wave"];
            
            // Play ability sound
            SoundEffectSystem.Instance?.PlayBossAbilityBloodRipple();
            
            // Trigger visual effect
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("blood_ripple", GlobalPosition, GlobalPosition);
            }
            
            ApplyAoEDamage(ability);
            
            if (ApplyStatusEffect(ability) && GetTarget() != null)
            {
                ApplyStatusToTarget(GetTarget(), StatusEffectType.Bleeding, ability.Duration);
            }
            GD.Print("Bleed wave!");
        }
        
        private void UseMagicMissile()
        {
            var ability = _abilityDatabase["magic_missile"];
            Vector2 targetPos = GetTarget()?.GlobalPosition ?? GlobalPosition + new Vector2(100, 0);
            
            // Play ability sound
            SoundEffectSystem.Instance?.PlayBossAbilityArcaneMissile();
            
            // Trigger visual effect
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("arcane_missile", GlobalPosition, targetPos);
            }
            
            if (GetTarget() != null)
            {
                float dist = GlobalPosition.DistanceTo(GetTarget().GlobalPosition);
                if (dist <= ability.Range)
                {
                    int damage = (int)(AttackDamage * ability.DamageMultiplier);
                    GetTarget().TakeDamage(damage);
                }
            }
            GD.Print("Magic missile fired!");
        }
        
        private void UseBossHeal()
        {
            // Play ability sound
            SoundEffectSystem.Instance?.PlayBossAbilitySelfHeal();
            
            // Trigger visual effect
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("self_heal", GlobalPosition, GlobalPosition);
            }
            
            Heal(MaxHealth / 4);
            // Visual feedback
            var tween = CreateTween();
            _sprite.Modulate = new Color(0f, 1f, 0f);
            tween.TweenProperty(_sprite, "modulate", Color.White, 0.5f);
            GD.Print("Boss healed!");
        }
        
        private void UseTeleport()
        {
            // Play ability sound
            SoundEffectSystem.Instance?.PlayBossAbilityTeleport();
            
            // Trigger visual effect at current position first
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("teleport", GlobalPosition, GlobalPosition);
            }
            
            if (GetTarget() != null)
            {
                // Teleport to random position around player
                float angle = (float)GD.RandRange(0, Mathf.PI * 2);
                float distance = (float)GD.RandRange(100, 200);
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                Vector2 newPos = GetTarget().GlobalPosition + offset;
                
                // Trigger visual effect at new position
                if (_visualizer != null)
                {
                    _visualizer.TriggerAbilityVisual("teleport", newPos, newPos);
                }
                
                GlobalPosition = newPos;
                
                // Visual effect
                var tween = CreateTween();
                _sprite.Modulate = new Color(0.5f, 0.5f, 1f);
                tween.TweenProperty(_sprite, "modulate", Color.White, 0.3f);
            }
            GD.Print("Boss teleported!");
        }
        
        private void UseSummonMinions()
        {
            // Play ability sound
            SoundEffectSystem.Instance?.PlayBossAbilitySummonMinions();
            
            // Trigger visual effect
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("summon_minions", GlobalPosition, GlobalPosition);
            }
            
            // Spawn 3 minions around boss
            for (int i = 0; i < 3; i++)
            {
                float angle = (float)(i * Mathf.PI * 2 / 3);
                float distance = 100f;
                Vector2 spawnPos = GlobalPosition + new Vector2(
                    Mathf.Cos(angle) * distance,
                    Mathf.Sin(angle) * distance
                );
                
                // Would spawn enemy here - simplified for now
                GD.Print($"Minion {i+1} summoned at {spawnPos}");
            }
            GD.Print("Minions summoned!");
        }
        
        private void UseAreaAttack()
        {
            var ability = _abilityDatabase["ground_slam"]; // Use as fallback
            ApplyAoEDamage(ability);
            GD.Print("Area attack!");
        }
        
        private void ApplyAoEDamage(BossAbility ability)
        {
            if (!ability.IsAoE) return;
            
            var enemies = GetTree().GetNodesInGroup("player");
            foreach (Node node in enemies)
            {
                if (node is Player player)
                {
                    float dist = GlobalPosition.DistanceTo(player.GlobalPosition);
                    if (dist <= ability.AoERadius)
                    {
                        int damage = (int)(AttackDamage * ability.DamageMultiplier);
                        player.TakeDamage(damage);
                    }
                }
            }
        }
        
        private bool ApplyStatusEffect(BossAbility ability)
        {
            return ability.ApplyStatus.HasValue && (float)GD.RandRange(0, 1) <= ability.StatusChance;
        }
        
        private void ApplyStatusToTarget(Character target, StatusEffectType status, float duration)
        {
            // Status effect application would integrate with StatusEffectSystem
            GD.Print($"Applied {status} for {duration}s to target");
        }
        
        private void ShowPhaseTransitionEffect()
        {
            var tween = CreateTween();
            _sprite.Modulate = new Color(1f, 0f, 1f);
            tween.TweenProperty(_sprite, "modulate", Color.White, 2f);
            
            // Screen shake effect
            var main = GetTree().CurrentScene;
            main.Call("AddScreenShake", 10);
        }
        
        private void ShowEnrageEffect()
        {
            var tween = CreateTween();
            _sprite.Modulate = new Color(1f, 0.3f, 0f);
            tween.SetLoops();
            tween.TweenProperty(_sprite, "modulate", new Color(1f, 0f, 0f), 0.5f);
            tween.TweenProperty(_sprite, "modulate", new Color(1f, 0.3f, 0f), 0.5f);
        }
        
        // Public methods
        public int GetCurrentPhase() => _currentPhase;
        public bool IsEnraged() => _isEnraged;
        public float GetEnrageTimeRemaining() => _enrageTimer;
        public float GetEnragePercentage() => (_enrageTimer / EnrageTime) * 100f;
        public BossAIState GetAIState() => _aiState;
        
        public float GetAbilityCooldown(string abilityId)
        {
            if (_abilityCurrentCooldowns.ContainsKey(abilityId))
                return _abilityCurrentCooldowns[abilityId];
            return 0f;
        }
        
        public Dictionary<string, BossAbility> GetAbilityDatabase() => _abilityDatabase;
        
        public override void Die()
        {
            GD.Print($"*** BOSS DEFEATED: {BossTitle} ***");
            OnPhaseChange = null;
            OnEnrage = null;
            OnSpecialAbility = null;
            OnAIStateChanged = null;
            
            // Track boss bounty progress
            BountyManager.Instance?.UpdateBossKillProgress(BossId);
            
            base.Die();
        }
    }
}
