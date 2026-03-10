using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Characters {
    /// <summary>
    /// Boss enemy with multiple phases and special abilities
    /// </summary>
    public partial class Boss : Enemy
    {
        [ExportGroup("Boss Properties")]
        [Export] public string BossTitle = "Ancient Dragon";
        [Export] public int PhaseCount = 3;
        [Export] public float EnrageTime = 120f; // seconds
        
        [ExportGroup("Phase Settings")]
        [Export] public int[] PhaseHealthThresholds = { 66, 33 }; // Percentage thresholds for each phase
        
        [ExportGroup("Special Abilities")]
        [Export] public float AbilityCooldown = 10f;
        [Export] public string[] SpecialAbilities;
        
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
        
        public override void _Ready()
        {
            base._Ready();
            
            _abilityTimer = 5f; // First ability after 5 seconds
            _enrageTimer = EnrageTime;
            
            GD.Print($"Boss {BossTitle} spawned! Phase: {_currentPhase}, Enrage: {EnrageTime}s");
        }
        
        public override void _PhysicsProcess(double delta)
        {
            float dt = (float)delta;
            
            if (IsDead) return;
            
            // Update enrage timer
            if (!_isEnraged)
            {
                _enrageTimer -= dt;
                if (_enrageTimer <= 0)
                {
                    TriggerEnrage();
                }
            }
            
            // Update ability timer
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
            
            OnPhaseChange?.Invoke(_currentPhase);
            
            GetTree().CreateTimer(2f).Timeout += () => _phaseTransitioning = false;
        }
        
        private void TriggerEnrage()
        {
            _isEnraged = true;
            MoveSpeed *= 1.5f;
            AttackDamage *= 2f;
            AttackCooldown *= 0.7f;
            
            GD.Print($"{BossTitle} is ENRAGED!");
            ShowEnrageEffect();
            
            OnEnrage?.Invoke();
        }
        
        private void TryUseSpecialAbility()
        {
            if (SpecialAbilities == null || SpecialAbilities.Length == 0) return;
            
            // Pick random ability
            string ability = SpecialAbilities[GD.Randi() % SpecialAbilities.Length];
            
            GD.Print($"{BossTitle} uses special ability: {ability}");
            
            switch (ability)
            {
                case "fire_breath":
                    UseFireBreath();
                    break;
                case "ground_slam":
                    UseGroundSlam();
                    break;
                case "teleport":
                    UseTeleport();
                    break;
                case "summon_minions":
                    UseSummonMinions();
                    break;
                case "heal":
                    UseBossHeal();
                    break;
                case "area_attack":
                    UseAreaAttack();
                    break;
            }
            
            OnSpecialAbility?.Invoke(ability);
        }
        
        private void UseFireBreath()
        {
            // Create fire breath area effect
            Vector2 direction = (GetTarget().GlobalPosition - GlobalPosition).Normalized();
            // Visual effect would be added here
            GD.Print("Fire breath attack!");
        }
        
        private void UseGroundSlam()
        {
            // AoE damage around boss
            float slamRadius = 200f;
            if (GetTarget() != null && GlobalPosition.DistanceTo(GetTarget().GlobalPosition) <= slamRadius)
            {
                GetTarget().TakeDamage((int)(AttackDamage * 1.5f));
            }
            GD.Print("Ground slam!");
        }
        
        private void UseTeleport()
        {
            // Teleport behind player
            if (GetTarget() != null)
            {
                Vector2 newPos = GetTarget().GlobalPosition + new Vector2(100, 0);
                GlobalPosition = newPos;
            }
            GD.Print("Boss teleported!");
        }
        
        private void UseSummonMinions()
        {
            // Spawn 3 minions
            GD.Print("Summoning minions...");
            // Minion spawning would be implemented here
        }
        
        private void UseBossHeal()
        {
            Heal(MaxHealth / 4);
            GD.Print("Boss healed!");
        }
        
        private void UseAreaAttack()
        {
            // Multiple target attack
            float attackRadius = 300f;
            if (GetTarget() != null && GlobalPosition.DistanceTo(GetTarget().GlobalPosition) <= attackRadius)
            {
                GetTarget().TakeDamage((int)(AttackDamage * 2f));
            }
            GD.Print("Area attack!");
        }
        
        private void ShowPhaseTransitionEffect()
        {
            var tween = CreateTween();
            _sprite.Modulate = new Color(1f, 0f, 1f); // Purple flash
            tween.TweenProperty(_sprite, "modulate", Color.White, 2f);
            
            // Screen shake effect
            var main = GetTree().CurrentScene;
            main.Call("AddScreenShake", 10);
        }
        
        private void ShowEnrageEffect()
        {
            var tween = CreateTween();
            _sprite.Modulate = new Color(1f, 0.3f, 0f); // Orange/red
            tween.SetLoops();
            tween.TweenProperty(_sprite, "modulate", new Color(1f, 0f, 0f), 0.5f);
            tween.TweenProperty(_sprite, "modulate", new Color(1f, 0.3f, 0f), 0.5f);
        }
        
        public int GetCurrentPhase() => _currentPhase;
        public bool IsEnraged() => _isEnraged;
        public float GetEnrageTimeRemaining() => _enrageTimer;
        public float GetEnragePercentage() => (_enrageTimer / EnrageTime) * 100f;
        
        public override void Die()
        {
            GD.Print($"*** BOSS DEFEATED: {BossTitle} ***");
            // Boss defeated celebration
            OnPhaseChange = null;
            OnEnrage = null;
            OnSpecialAbility = null;
            
            // Track boss bounty progress
            BountyManager.Instance.UpdateBossKillProgress(BossId);
            
            base.Die();
        }
    }
}
