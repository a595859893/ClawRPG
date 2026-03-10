using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

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
        
        public override void _Ready()
        {
            CurrentHealth = MaxHealth;
            
            _sprite = GetNode<Sprite2D>("Sprite2D");
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _detectionArea = GetNode<Area2D>("DetectionArea");
            _attackArea = GetNode<Area2D>("AttackArea");
            
            // Find player
            _target = GetTree().GetFirstNodeInGroup("player") as Player;
            
            ChangeState(new EnemyStateIdle(this));
            
            GD.Print("Enemy spawned: " + EnemyName + " HP: " + CurrentHealth);
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
            if (IsDead) return;
            
            CurrentHealth -= damage;
            
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
            IsDead = true;
            
            GD.Print(EnemyName + " defeated!");
            
            // Track kill achievement
            AchievementManager.Instance.TrackKill();
            
            // Check if this is a boss
            if (this is Boss)
            {
                AchievementManager.Instance.TrackBossKill();
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
            var popup = new DamagePopup();
            popup.Initialize(damage, isCrit, GlobalPosition);
            GetTree().CurrentScene.AddChild(popup);
        }
        
        private void FlashDamage()
        {
            var tween = CreateTween();
            _sprite.Modulate = new Color(1f, 0f, 0f);
            tween.TweenProperty(_sprite, "modulate", Color.White, 0.1f);
        }
        
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
