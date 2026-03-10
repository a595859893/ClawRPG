using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Characters {
    /// <summary>
    /// Player character controller with combat system
    /// </summary>
    public partial class Player : CharacterBody2D
    {
        // Movement
        [Export] public float MoveSpeed = 200f;
        [Export] public float DodgeSpeed = 400f;
        
        // Combat stats
        [Export] public int MaxHealth = 100;
        [Export] public int MaxMana = 50;
        [Export] public float AttackDamage = 15f;
        [Export] public float AttackSpeed = 0.5f;
        [Export] public float CriticalChance = 0.1f;
        [Export] public float CriticalDamage = 1.5f;
        
        // Stamina
        [Export] public float MaxStamina = 100f;
        [Export] public float StaminaRegen = 20f;
        
        // Block system
        [Export] public float BlockStaminaCost = 15f;
        [Export] public float BlockDamageReduction = 0.5f;
        [Export] public float PerfectBlockWindow = 0.2f;
        
        // Dodge system
        [Export] public float DodgeDuration = 0.3f;
        [Export] public float DodgeCooldown = 1.0f;
        
        // State
        public int CurrentHealth { get; private set; }
        public int CurrentMana { get; private set; }
        public float CurrentStamina { get; private set; }
        public int Level { get; private set; } = 1;
        public int Experience { get; private set; }
        
        // Combat state
        public bool IsAttacking { get; private set; }
        public bool IsBlocking { get; private set; }
        public bool IsDodging { get; private set; }
        public bool IsInvincible { get; private set; }
        public bool IsPerfectBlock { get; private set; }
        public Vector2 AttackDirection { get; private set; } = Vector2.Right;
        
        // Timers
        private float _attackTimer;
        private float _dodgeCooldownTimer;
        private float _perfectBlockTimer;
        private float _staminaRegenTimer;
        
        // Status effects
        private List<StatusEffect> _statusEffects = new();
        
        // Components
        private AnimationPlayer _animationPlayer;
        private Sprite2D _sprite;
        private Area2D _attackArea;
        private CollisionShape2D _hitbox;
        
        public override void _Ready()
        {
            CurrentHealth = MaxHealth;
            CurrentMana = MaxMana;
            CurrentStamina = MaxStamina;
            
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _sprite = GetNode<Sprite2D>("Sprite2D");
            _attackArea = GetNode<Area2D>("AttackArea");
            _hitbox = GetNode<CollisionShape2D>("Hitbox/CollisionShape2D");
            
            GD.Print("Player initialized - HP: " + CurrentHealth + "/" + MaxHealth);
        }
        
        public override void _PhysicsProcess(double delta)
        {
            float dt = (float)delta;
            
            // Update timers
            UpdateTimers(dt);
            
            // Handle movement
            HandleMovement(dt);
            
            // Update status effects
            UpdateStatusEffects(dt);
            
            // Regenerate stamina
            RegenerateStamina(dt);
            
            // Apply velocity
            Velocity = Velocity.MoveToward(Vector2.Zero, 500f * (float)delta);
            MoveAndSlide();
        }
        
        private void UpdateTimers(float dt)
        {
            if (_attackTimer > 0) _attackTimer -= dt;
            if (_dodgeCooldownTimer > 0) _dodgeCooldownTimer -= dt;
            if (_perfectBlockTimer > 0) _perfectBlockTimer -= dt;
            
            if (_perfectBlockTimer <= 0) IsPerfectBlock = false;
        }
        
        private void HandleMovement(float dt)
        {
            if (IsDodging || IsAttacking) return;
            
            Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
            
            // Block movement speed reduction
            float speedMod = IsBlocking ? 0.5f : 1f;
            
            if (inputDir != Vector2.Zero)
            {
                Velocity = inputDir * MoveSpeed * speedMod;
                AttackDirection = inputDir.Normalized();
                
                // Flip sprite
                if (inputDir.x < 0) _sprite.FlipH = true;
                else if (inputDir.x > 0) _sprite.FlipH = false;
            }
            else
            {
                Velocity = Vector2.Zero;
            }
        }
        
        private void RegenerateStamina(float dt)
        {
            if (!IsBlocking && CurrentStamina < MaxStamina)
            {
                CurrentStamina = Mathf.Min(MaxStamina, CurrentStamina + StaminaRegen * dt);
            }
        }
        
        public override void _Process(double delta)
        {
            float dt = (float)delta;
            
            // Handle combat input
            HandleCombatInput(dt);
        }
        
        private void HandleCombatInput(float dt)
        {
            // Attack
            if (Input.IsActionPressed("attack") && !IsAttacking && !IsBlocking && _attackTimer <= 0)
            {
                PerformAttack();
            }
            
            // Dodge
            if (Input.IsActionPressed("dodge") && !IsDodging && _dodgeCooldownTimer <= 0 && CurrentStamina >= 20f)
            {
                PerformDodge();
            }
            
            // Block
            HandleBlockInput(dt);
        }
        
        private void HandleBlockInput(float dt)
        {
            bool wantsBlock = Input.IsActionPressed("block");
            
            if (wantsBlock && !IsBlocking && CurrentStamina >= BlockStaminaCost && !IsDodging && !IsAttacking)
            {
                StartBlock();
            }
            else if (!wantsBlock && IsBlocking)
            {
                EndBlock();
            }
            
            if (IsBlocking)
            {
                CurrentStamina -= BlockStaminaCost * dt;
                if (CurrentStamina <= 0)
                {
                    EndBlock();
                }
            }
        }
        
        private void StartBlock()
        {
            IsBlocking = true;
            GD.Print("Block started");
        }
        
        private void EndBlock()
        {
            IsBlocking = false;
            IsPerfectBlock = false;
            _perfectBlockTimer = 0;
            GD.Print("Block ended");
        }
        
        public void TriggerPerfectBlock()
        {
            IsPerfectBlock = true;
            _perfectBlockTimer = PerfectBlockWindow;
            GD.Print("PERFECT BLOCK!");
        }
        
        private void PerformAttack()
        {
            IsAttacking = true;
            _attackTimer = AttackSpeed;
            
            // Calculate damage with critical
            float damage = AttackDamage;
            bool isCrit = GD.Randf() < CriticalChance;
            if (isCrit) damage *= CriticalDamage;
            
            // Deal damage to enemies in range
            var enemies = GetEnemiesInAttackRange();
            foreach (var enemy in enemies)
            {
                enemy.TakeDamage((int)damage, isCrit, AttackDirection);
            }
            
            GD.Print("Attack! Damage: " + damage + (isCrit ? " CRITICAL!" : ""));
            
            // Attack animation/sound would go here
            
            // Reset attack state after animation
            GetTree().CreateTimer(0.3f).Timeout += () => IsAttacking = false;
        }
        
        private void PerformDodge()
        {
            IsDodging = true;
            IsInvincible = true;
            CurrentStamina -= 20f;
            _dodgeCooldownTimer = DodgeCooldown;
            
            Vector2 dodgeDir = AttackDirection;
            if (Input.GetVector("move_left", "move_right", "move_up", "move_down") != Vector2.Zero)
            {
                dodgeDir = Input.GetVector("move_left", "move_right", "move_up", "move_down").Normalized();
            }
            
            // Tween velocity for smooth dodge
            var tween = CreateTween();
            tween.TweenProperty(this, "velocity", dodgeDir * DodgeSpeed, DodgeDuration);
            
            GetTree().CreateTimer(DodgeDuration).Timeout += () => {
                IsDodging = false;
                IsInvincible = false;
                Velocity = Vector2.Zero;
            };
            
            GD.Print("Dodge performed!");
        }
        
        private List<Enemy> GetEnemiesInAttackRange()
        {
            var enemies = new List<Enemy>();
            if (_attackArea == null) return enemies;
            
            var bodies = _attackArea.GetOverlappingBodies();
            foreach (var body in bodies)
            {
                if (body is Enemy enemy)
                {
                    enemies.Add(enemy);
                }
            }
            return enemies;
        }
        
        public void TakeDamage(int damage, bool isCrit = false, Vector2 fromDirection = default)
        {
            if (IsInvincible) return;
            
            float finalDamage = damage;
            
            // Apply block reduction
            if (IsBlocking)
            {
                if (IsPerfectBlock)
                {
                    finalDamage = 0;
                    TriggerPerfectBlock();
                    // Counter attack could trigger here
                    GD.Print("Perfect block! No damage taken!");
                }
                else
                {
                    finalDamage *= (1 - BlockDamageReduction);
                    GD.Print("Blocked! Damage reduced to: " + finalDamage);
                }
            }
            
            CurrentHealth -= (int)finalDamage;
            
            if (CurrentHealth <= 0)
            {
                Die();
            }
            
            // Knockback
            if (fromDirection != default)
            {
                Velocity = -fromDirection * 100f;
            }
            
            // Visual feedback
            ShowDamageNumber((int)finalDamage, isCrit);
            
            GD.Print("Player took " + finalDamage + " damage. HP: " + CurrentHealth + "/" + MaxHealth);
        }
        
        public void Heal(int amount)
        {
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            GD.Print("Healed " + amount + " HP. HP: " + CurrentHealth + "/" + MaxHealth);
        }
        
        public void UseMana(int amount)
        {
            CurrentMana = Mathf.Max(0, CurrentMana - amount);
        }
        
        public void GainExperience(int amount)
        {
            Experience += amount;
            GD.Print("Gained " + amount + " XP. Total: " + Experience);
            
            // Check for level up (100 XP per level)
            int expToLevel = Level * 100;
            if (Experience >= expToLevel)
            {
                LevelUp();
            }
        }
        
        private void LevelUp()
        {
            Level++;
            Experience -= Level * 100;
            
            // Increase stats
            MaxHealth += 20;
            MaxMana += 10;
            AttackDamage += 5;
            CurrentHealth = MaxHealth;
            CurrentMana = MaxMana;
            
            GD.Print("LEVEL UP! Now level " + Level);
            
            // Level up effect
            var effect = new LevelUpEffect();
            GetTree().CurrentScene.AddChild(effect);
            effect.GlobalPosition = GlobalPosition;
        }
        
        private void Die()
        {
            GD.Print("Player died!");
            // Handle death (game over, respawn, etc.)
            QueueFree();
        }
        
        public void ApplyStatusEffect(StatusEffect effect)
        {
            _statusEffects.Add(effect);
            GD.Print("Applied status effect: " + effect.Type);
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
        
        public float GetBlockDamageReduction()
        {
            return IsPerfectBlock ? 1.0f : BlockDamageReduction;
        }
        
        private void ShowDamageNumber(int damage, bool isCrit)
        {
            var popup = new DamagePopup();
            popup.Initialize(damage, isCrit, GlobalPosition);
            GetTree().CurrentScene.AddChild(popup);
        }
    }
    
    /// <summary>
    /// Status effect base class
    /// </summary>
    public class StatusEffect
    {
        public enum EffectType { Poison, Burn, Freeze, Stun, Slow, Bleed, Sleep, Paralyze, Confusion, Shield, Regeneration }
        
        public EffectType Type { get; set; }
        public float Duration { get; set; }
        public float TickInterval { get; set; } = 1f;
        public float DamagePerTick { get; set; }
        public float SpeedMultiplier { get; set; } = 1f;
        
        private float _timer;
        private float _tickTimer;
        
        public bool IsExpired => Duration <= 0;
        
        public virtual void Update(CharacterBody2D target, float dt)
        {
            Duration -= dt;
            _tickTimer += dt;
            
            if (_tickTimer >= TickInterval)
            {
                _tickTimer = 0;
                ApplyTickEffect(target);
            }
        }
        
        protected virtual void ApplyTickEffect(CharacterBody2D target)
        {
            // Override in subclasses
        }
    }
    
    /// <summary>
    /// Damage number popup effect
    /// </summary>
    public partial class DamagePopup : Node2D
    {
        private Label _label;
        private float _lifetime = 1f;
        private float _speed = 50f;
        
        public void Initialize(int damage, bool isCritical, Vector2 position)
        {
            GlobalPosition = position;
            
            _label = new Label();
            _label.Text = damage.ToString();
            _label.Modulate = isCritical ? new Color(1f, 0.3f, 0.3f) : new Color(1f, 1f, 1f);
            _label.AddThemeFontSizeOverride("font_size", isCritical ? 32 : 24);
            AddChild(_label);
            
            // Animate
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(_label, "position:y", -50f, _lifetime);
            tween.TweenProperty(_label, "modulate:a", 0f, _lifetime);
            
            QueueFree();
        }
    }
    
    /// <summary>
    /// Level up visual effect
    /// </summary>
    public partial class LevelUpEffect : Node2D
    {
        public override void _Ready()
        {
            var sprite = new Sprite2D();
            // Would load particle texture here
            AddChild(sprite);
            
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0f, 1f);
            QueueFree();
        }
    }
}
