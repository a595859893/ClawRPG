using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;
using ClawRPG.Scripts.Items;
using ClawRPG.Scripts.Database;
using ClawRPG.Scripts.UI;
using ClawRPG.Scripts.Framework;

namespace ClawRPG.Scripts.Characters {
    /// <summary>
    /// Enemy AI character with state machine
    /// </summary>
    public partial class Enemy : CharacterBody2D
    {
        [Export] public string EnemyName = "Goblin";
        [Export] public int MaxHealth = 50;
        [Export] public float MoveSpeed = 100f;
        [Export] public float AttackDamage = 10f;
        [Export] public float AttackRange = 50f;
        [Export] public float AttackCooldown = 1f;
        [Export] public float ChaseRange = 200f;
        [Export] public float DetectionRange = 300f;
        
        // Combat stats
        [Export] public float CriticalChance = 0.05f;
        [Export] public float CriticalDamage = 1.5f;
        
        public int CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }
        
        // State
        private EnemyState _currentState;
        private Player _target;
        private float _attackTimer;
        
        // Components
        private Sprite2D _sprite;
        private AnimationPlayer _animationPlayer;
        private Area2D _detectionArea;
        private Area2D _attackArea;
        
        // Status effects
        private List<StatusEffect> _statusEffects = new();
        
        // Loot
        [Export] public int ExperienceReward = 20;
        [Export] public string[] DropItems;
        [Export] public string EnemyTypeId = "";  // Database enemy type ID for drop table lookup
        
        public override void _Ready()
        {
            // Initialize from database if EnemyTypeId is set (Flyweight Pattern)
            InitializeFromDatabase();
            
            CurrentHealth = MaxHealth;
            
            _sprite = GetNode<Sprite2D>("Sprite2D");
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _detectionArea = GetNode<Area2D>("DetectionArea");
            _attackArea = GetNode<Area2D>("AttackArea");
            
            // Find player
            _target = GetTree().GetFirstNodeInGroup("player") as Player;
            
            // REQ-129: Auto-attach EnemyPatternTracker if not already present
            if (GetNodeOrNull<EnemyPatternTracker>("EnemyPatternTracker") == null)
            {
                var tracker = new EnemyPatternTracker();
                tracker.Name = "EnemyPatternTracker";
                AddChild(tracker);
            }
            
            ChangeState(new EnemyStateIdle(this));
            
            GD.Print("Enemy spawned: " + EnemyName + " HP: " + CurrentHealth);
        }
        
        /// <summary>
        /// Initialize enemy properties from database using Flyweight Pattern
        /// Multiple enemy instances can share the same EnemyType data from cache
        /// </summary>
        private void InitializeFromDatabase()
        {
            if (string.IsNullOrEmpty(EnemyTypeId)) return;
            
            var enemyType = EnemyDatabase.Instance.GetEnemyType(EnemyTypeId);
            if (enemyType == null)
            {
                GD.PrintErr("Enemy type not found in database: " + EnemyTypeId);
                return;
            }
            
            // Apply database values (shared intrinsic state)
            EnemyName = enemyType.Name;
            MaxHealth = enemyType.MaxHealth;
            MoveSpeed = enemyType.MoveSpeed;
            AttackDamage = enemyType.AttackDamage;
            AttackRange = enemyType.AttackRange;
            AttackCooldown = enemyType.AttackCooldown;
            ChaseRange = enemyType.ChaseRange;
            DetectionRange = enemyType.DetectionRange;
            CriticalChance = enemyType.CriticalChance;
            CriticalDamage = enemyType.CriticalDamage;
            ExperienceReward = enemyType.ExperienceReward;
            
            GD.Print("Enemy initialized from database: " + EnemyTypeId);
        }
        
        public override void _PhysicsProcess(double delta)
        {
            float dt = (float)delta;
            
            if (IsDead) return;
            
            // Update status effects
            UpdateStatusEffects(dt);
            
            // Update state
            if (_currentState != null)
            {
                _currentState.Update(dt);
            }
            
            // Update attack timer
            if (_attackTimer > 0) _attackTimer -= dt;
            
            // Apply velocity
            MoveAndSlide();
        }
        
        public void ChangeState(EnemyState newState)
        {
            if (_currentState != null)
            {
                _currentState.Exit();
            }
            
            _currentState = newState;
            _currentState.Enter();
        }
        
        public void TakeDamage(int damage, bool isCrit = false, Vector2 fromDirection = default)
        {
            Invariant.Assert(!IsDead, "TakeDamage called on dead enemy: {0}", EnemyName);
            Invariant.Assert(damage >= 0, "TakeDamage negative damage for {0}: {1}", EnemyName, damage);
            if (IsDead) return;
            
            // REQ-129: Apply counter defense if enemy has recognized our combo
            int actualDamage = damage;
            var tracker = GetNodeOrNull<EnemyPatternTracker>("EnemyPatternTracker");
            if (tracker != null && tracker.IsInCounterMode)
            {
                actualDamage = (int)tracker.ApplyCounterDefense(damage);
                // Visual feedback is handled by SetCounterModeActive (orange pulse)
            }
            
            CurrentHealth -= actualDamage;
            Invariant.Assert(CurrentHealth >= 0, "Health went negative after TakeDamage for {0}: {1}", EnemyName, CurrentHealth);
            Invariant.Assert(CurrentHealth <= MaxHealth, "Health exceeded MaxHealth after TakeDamage for {0}: {1} > {2}", EnemyName, CurrentHealth, MaxHealth);
            
            // Track statistics
            StatisticsManager.Instance.RecordDamageDealt(damage);
            if (isCrit)
            {
                StatisticsManager.Instance.RecordCriticalHit();
            }
            
            // Show damage popup
            ShowDamageNumber(damage, isCrit);
            
            // Knockback
            if (fromDirection != default)
            {
                Velocity = fromDirection * 150f;
            }
            
            // Flash effect
            FlashDamage();
            
            GD.Print(EnemyName + " took " + damage + (isCrit ? " CRITICAL!" : ""));
            
            if (CurrentHealth <= 0)
            {
                Die();
            }
        }
        
        private void Die()
        {
            Invariant.Assert(CurrentHealth <= 0, "Die called but health is {0} for {1}", CurrentHealth, EnemyName);
            IsDead = true;
            
            GD.Print(EnemyName + " defeated!");
            
            // Track kill achievement
            AchievementManager.Instance.TrackKill();
            
            // Track bounty progress
            BountyManager.Instance.UpdateKillProgress(EnemyTypeId);
            
            // Track statistics
            StatisticsManager.Instance.RecordKill();
            
            // Track combat status - enemy killed
            bool isBoss = this is Boss;
            CombatStatusSystem.Instance.RecordEnemyKilled(isBoss);
            
            // Check if this is a boss
            if (this is Boss)
            {
                AchievementManager.Instance.TrackBossKill();
                StatisticsManager.Instance.RecordBossDefeated();
            }
            
            // Give experience to player
            if (_target != null)
            {
                _target.GainExperience(ExperienceReward);
            }
            
            // Drop items
            DropLoot();
            
            // Death animation
            if (_animationPlayer != null)
            {
                _animationPlayer.Play("death");
            }
            
            // Remove after delay
            GetTree().CreateTimer(1f).Timeout += () => QueueFree();
        }
        
        private void DropLoot()
        {
            // First, try to use database drop table
            if (!string.IsNullOrEmpty(EnemyTypeId))
            {
                var enemyType = Database.EnemyDatabase.Instance.GetEnemy(EnemyTypeId);
                if (enemyType != null && enemyType.DropTable != null && enemyType.DropTable.Count > 0)
                {
                    foreach (var dropEntry in enemyType.DropTable)
                    {
                        string itemId = dropEntry.Key;
                        float dropChance = dropEntry.Value;
                        
                        if (GD.Randf() < dropChance)
                        {
                            // Try to parse as integer ID first
                            if (int.TryParse(itemId, out int itemIdInt))
                            {
                                // Add to player's inventory using InventoryManager
                                var inventoryManager = Items.InventoryManager.Instance;
                                if (inventoryManager != null)
                                {
                                    if (inventoryManager.AddItem(itemIdInt, 1))
                                    {
                                        var itemData = Items.ItemDatabase.Instance.GetItem(itemIdInt);
                                        string itemName = itemData?.Name ?? $"Item {itemIdInt}";
                                        GD.Print($"[Loot] Player received: {itemName}");
                                    }
                                }
                            }
                            else
                            {
                                // It's a string key, spawn as item drop in world
                                GD.Print("Dropped item: " + itemId);
                            }
                        }
                    }
                    return;  // Use database drops, skip editor DropItems
                }
            }
            
            // Fallback to editor-configured DropItems
            if (DropItems == null || DropItems.Length == 0) return;
            
            foreach (var itemId in DropItems)
            {
                if (GD.Randf() > 0.5f) // 50% drop chance per item
                {
                    GD.Print("Dropped item: " + itemId);
                    // Would spawn item pickup here
                }
            }
        }
        
        public void Attack()
        {
            if (_attackTimer > 0) return;
            
            _attackTimer = AttackCooldown;
            
            // Check if player in range
            if (_target != null && GlobalPosition.DistanceTo(_target.GlobalPosition) <= AttackRange)
            {
                // Calculate damage
                float damage = AttackDamage;
                bool isCrit = GD.Randf() < CriticalChance;
                if (isCrit) damage *= CriticalDamage;
                
                _target.TakeDamage((int)damage, isCrit, (GlobalPosition - _target.GlobalPosition).Normalized());
                
                GD.Print(EnemyName + " attacks! Damage: " + damage);
            }
        }
        
        public void ApplyStatusEffect(StatusEffect effect)
        {
            _statusEffects.Add(effect);
            GD.Print(EnemyName + " affected by: " + effect.Type);
        }

        /// <summary>
        /// Apply a status effect from the skill system (EffectType variant).
        /// </summary>
        public void ApplyStatusEffect(StatusEffect.EffectType effectType, float value, float duration)
        {
            var effect = new StatusEffect(effectType, value, duration);
            _statusEffects.Add(effect);
            GD.Print(EnemyName + " affected by: " + effectType);
        }

        /// <summary>
        /// Apply a status effect from boss abilities (StatusEffectType variant).
        /// </summary>
        public void ApplyStatusEffect(StatusEffectType effectType, float value, float duration)
        {
            var effect = StatusEffect.FromEffectType(effectType, value, duration);
            _statusEffects.Add(effect);
            GD.Print(EnemyName + " affected by: " + effectType);
        }
        
        private void UpdateStatusEffects(float dt)
        {
            for (int i = _statusEffects.Count - 1; i >= 0; i--)
            {
                var effect = _statusEffects[i];
                effect.Update(this, dt);
                
                if (effect.IsExpired)
                {
                    _statusEffects.RemoveAt(i);
                }
            }
        }
        
        public Player GetTarget() => _target;
        
        public bool IsTargetInRange(float range)
        {
            return _target != null && GlobalPosition.DistanceTo(_target.GlobalPosition) <= range;
        }
        
        public void MoveToTarget(float speed)
        {
            if (_target == null) return;
            
            Vector2 direction = (_target.GlobalPosition - GlobalPosition).Normalized();
            Velocity = direction * speed;
            
            // Flip sprite
            if (direction.x < 0) _sprite.FlipH = true;
            else if (direction.x > 0) _sprite.FlipH = false; 
        }
        
        public void StopMovement()
        {
            Velocity = Vector2.Zero;
        }
        
        private void ShowDamageNumber(int damage, bool isCrit)
        {
            if (DamageNumberSystem.Instance != null)
            {
                var type = isCrit ? DamageNumberSystem.DamageType.Critical : DamageNumberSystem.DamageType.Normal;
                DamageNumberSystem.Instance.ShowDamageOnEntity2D(this, damage, type);
            }
        }
        
        private void ShowCounterText()
        {
            // Show "识破!" text when counter defense reduces damage
            if (DamageNumberSystem.Instance != null)
            {
                DamageNumberSystem.Instance.ShowDamageOnEntity2D(this, 0, DamageNumberSystem.DamageType.Perfect);
            }
        }
        
        private void FlashDamage()
        {
            var tween = CreateTween();
            _sprite.Modulate = new Color(1f, 0f, 0f);
            tween.TweenProperty(_sprite, "modulate", Color.White, 0.1f);
        }
        
        // === Counter Mode (REQ-129: Enemy Observer AI) ===
        
        private bool _isInCounterMode = false;
        private Color _originalSpriteModulate;
        private Tween _counterModeTween;
        
        /// <summary>
        /// Called by EnemyPatternTracker when this enemy's counter mode activates/deactivates.
        /// </summary>
        public void SetCounterModeActive(bool active)
        {
            if (active == _isInCounterMode) return;
            _isInCounterMode = active;
            
            if (active)
            {
                // Orange glow to indicate "pattern recognized"
                _counterModeTween = CreateTween();
                _counterModeTween.TweenProperty(_sprite, "modulate", new Color(1f, 0.6f, 0.1f), 0.2f);
                _counterModeTween.SetLoops(-1);
                _counterModeTween.TweenProperty(_sprite, "modulate", new Color(1f, 0.9f, 0.3f), 0.4f);
            }
            else
            {
                if (_counterModeTween != null)
                {
                    _counterModeTween.Kill();
                    _counterModeTween = null;
                }
                _sprite.Modulate = Color.White;
            }
        }
        
        /// <summary>
        /// Called by EnemyPatternTracker when this enemy recognizes a player combo pattern.
        /// </summary>
        public void NotifyPatternRecognized(string comboId, float threatLevel)
        {
            // Emit a signal that UI can listen to
            EmitSignal(nameof(PatternRecognized), comboId, threatLevel);
            GD.Print($"{EnemyName} recognized combo pattern: {comboId} (threat: {threatLevel:F2})");
        }
public delegate void PatternRecognizedEventHandler(string comboId, float threatLevel);
        
        // === End Counter Mode ===
        
        public void Heal(int amount)
        {
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        }
    }
    
    /// <summary>
    /// Enemy AI State base class
    /// </summary>
    public abstract class EnemyState
    {
        protected Enemy Enemy { get; }
        
        public EnemyState(Enemy enemy)
        {
            Enemy = enemy;
        }
        
        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update(float dt) { }
    }
    
    /// <summary>
    /// Enemy idle state - waiting
    /// </summary>
    public class EnemyStateIdle : EnemyState
    {
        public EnemyStateIdle(Enemy enemy) : base(enemy) { }
        
        public override void Enter()
        {
            Enemy.StopMovement();
        }
        
        public override void Update(float dt)
        {
            // Check for player
            if (Enemy.GetTarget() != null && Enemy.IsTargetInRange(Enemy.DetectionRange))
            {
                Enemy.ChangeState(new EnemyStateChase(Enemy));
            }
        }
    }
    
    /// <summary>
    /// Enemy chase state - running towards player
    /// </summary>
    public class EnemyStateChase : EnemyState
    {
        public EnemyStateChase(Enemy enemy) : base(enemy) { }
        
        public override void Update(float dt)
        {
            if (Enemy.GetTarget() == null)
            {
                Enemy.ChangeState(new EnemyStateIdle(Enemy));
                return;
            }
            
            float distance = Enemy.GlobalPosition.DistanceTo(Enemy.GetTarget().GlobalPosition);
            
            if (distance > Enemy.DetectionRange)
            {
                // Lost player
                Enemy.ChangeState(new EnemyStateIdle(Enemy));
            }
            else if (distance <= Enemy.AttackRange)
            {
                // In attack range
                Enemy.ChangeState(new EnemyStateAttack(Enemy));
            }
            else
            {
                // Chase
                Enemy.MoveToTarget(Enemy.MoveSpeed);
            }
        }
    }
    
    /// <summary>
    /// Enemy attack state
    /// </summary>
    public class EnemyStateAttack : EnemyState
    {
        public EnemyStateAttack(Enemy enemy) : base(enemy) { }
        
        public override void Enter()
        {
            Enemy.StopMovement();
            Enemy.Attack();
        }
        
        public override void Update(float dt)
        {
            if (Enemy.GetTarget() == null || !Enemy.IsTargetInRange(Enemy.DetectionRange))
            {
                Enemy.ChangeState(new EnemyStateIdle(Enemy));
            }
            else if (!Enemy.IsTargetInRange(Enemy.AttackRange))
            {
                Enemy.ChangeState(new EnemyStateChase(Enemy));
            }
        }
    }
}
