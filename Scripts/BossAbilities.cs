using Godot;
using System;
using System.Collections.Generic;

using ClawRPG.Scripts.Combat;

namespace ClawRPG.Scripts.Characters {
    /// <summary>
    /// Boss Abilities - handles ability execution
    /// </summary>
    public class BossAbilities
    {
        private Boss _boss;
        private BossData _data;
        
        // Visual effects
        private BossAbilityVisualizer _visualizer;
        
        // Events
        public event Action<string> OnAbilityUsed;
        public event Action<string> OnAbilityWarmingUp;
        
        public BossAbilities(Boss boss, BossData data)
        {
            _boss = boss;
            _data = data;
        }
        
        /// <summary>
        /// Initialize abilities system
        /// </summary>
        public void Initialize()
        {
            _visualizer = BossAbilityVisualizer.Instance;
        }
        
        /// <summary>
        /// Try to use a random special ability
        /// </summary>
        public void TryUseSpecialAbility()
        {
            var readyAbilities = _data.GetReadyAbilities();
            if (readyAbilities.Count == 0) return;
            
            // Pick random ready ability
            string ability = readyAbilities[GD.Randi() % readyAbilities.Count];
            
            GD.Print($"{_data.BossTitle} uses special ability: {ability}");
            
            // Fire warming up event for UI warning
            OnAbilityWarmingUp?.Invoke(ability);
            
            // Set cooldown
            _data.SetAbilityOnCooldown(ability);
            
            _boss.ForceSetAIState(BossAIState.UsingAbility);
            
            ExecuteAbility(ability);
            
            OnAbilityUsed?.Invoke(ability);
        }
        
        /// <summary>
        /// Try to use a specific ability (called by decision maker)
        /// </summary>
        public void TryUseAbility(string abilityId)
        {
            if (string.IsNullOrEmpty(abilityId))
            {
                _boss.TryAttack();
                return;
            }
            
            // Check cooldown
            if (_data.AbilityCurrentCooldowns.ContainsKey(abilityId) && _data.AbilityCurrentCooldowns[abilityId] > 0)
            {
                return;
            }
            
            // Check if ability is available
            if (!_data.AvailableAbilities.Contains(abilityId))
            {
                return;
            }
            
            GD.Print($"{_data.BossTitle} uses ability: {abilityId}");
            
            // Fire warning event for UI
            OnAbilityWarmingUp?.Invoke(abilityId);
            
            // Set cooldown
            _data.SetAbilityOnCooldown(abilityId);
            
            _boss.ForceSetAIState(BossAIState.UsingAbility);
            
            ExecuteAbility(abilityId);
            
            OnAbilityUsed?.Invoke(abilityId);
        }
        
        /// <summary>
        /// Execute ability by ID
        /// </summary>
        private void ExecuteAbility(string abilityId)
        {
            switch (abilityId)
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
        }
        
        private void UseFireBreath()
        {
            var ability = _data.GetAbility("fire_breath");
            Vector2 targetPos = _boss.GetTarget()?.GlobalPosition ?? _boss.GlobalPosition + new Vector2(100, 0);
            Vector2 direction = (targetPos - _boss.GlobalPosition).Normalized();
            float facingAngle = Mathf.Atan2(direction.Y, direction.X);
            
            SoundEffectSystem.Instance?.PlayBossAbilityFireBreath();
            
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("fire_breath", _boss.GlobalPosition, targetPos, facingAngle);
            }
            
            if (ability.IsAoE)
            {
                ApplyAoEDamage(ability);
            }
            
            if (_boss.GetTarget() != null && ApplyStatusEffect(ability))
            {
                ApplyStatusToTarget(_boss.GetTarget(), ability.ApplyStatus.Value, ability.Duration);
            }
            
            GD.Print($"Fire breath! Direction: {direction}, AoE: {ability.AoERadius}");
        }
        
        private void UseLightningChain()
        {
            var ability = _data.GetAbility("lightning_chain");
            Vector2 targetPos = _boss.GetTarget()?.GlobalPosition ?? _boss.GlobalPosition + new Vector2(100, 0);
            
            SoundEffectSystem.Instance?.PlayBossAbilityLightningChain();
            
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("lightning_chain", _boss.GlobalPosition, targetPos);
            }
            
            ApplyAoEDamage(ability);
            GD.Print("Lightning chain attack!");
        }
        
        private void UsePoisonCloud()
        {
            var ability = _data.GetAbility("poison_cloud");
            Vector2 targetPos = _boss.GetTarget()?.GlobalPosition ?? _boss.GlobalPosition;
            
            SoundEffectSystem.Instance?.PlayBossAbilityPoisonCloud();
            
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("poison_cloud", _boss.GlobalPosition, targetPos);
            }
            
            ApplyAoEDamage(ability);
            
            if (_boss.GetTarget() != null && ApplyStatusEffect(ability))
            {
                ApplyStatusToTarget(_boss.GetTarget(), ability.ApplyStatus.Value, ability.Duration);
            }
            GD.Print("Poison cloud released!");
        }
        
        private void UseIceLance()
        {
            var ability = _data.GetAbility("ice_lance");
            Vector2 targetPos = _boss.GetTarget()?.GlobalPosition ?? _boss.GlobalPosition + new Vector2(100, 0);
            Vector2 direction = (targetPos - _boss.GlobalPosition).Normalized();
            float facingAngle = Mathf.Atan2(direction.Y, direction.X);
            
            SoundEffectSystem.Instance?.PlayBossAbilityIceLance();
            
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("ice_lance", _boss.GlobalPosition, targetPos, facingAngle);
            }
            
            if (_boss.GetTarget() != null)
            {
                float dist = _boss.GlobalPosition.DistanceTo(_boss.GetTarget().GlobalPosition);
                if (dist <= ability.Range)
                {
                    int damage = (int)(_boss.AttackDamage * ability.DamageMultiplier);
                    _boss.GetTarget().TakeDamage(damage);
                    
                    if (ApplyStatusEffect(ability))
                    {
                        ApplyStatusToTarget(_boss.GetTarget(), ability.ApplyStatus.Value, ability.Duration);
                    }
                }
            }
            GD.Print("Ice lance fired!");
        }
        
        private void UseDarkBolt()
        {
            var ability = _data.GetAbility("dark_bolt");
            Vector2 targetPos = _boss.GetTarget()?.GlobalPosition ?? _boss.GlobalPosition + new Vector2(100, 0);
            
            SoundEffectSystem.Instance?.PlayBossAbilityShadowBolt();
            
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("shadow_bolt", _boss.GlobalPosition, targetPos);
            }
            
            if (_boss.GetTarget() != null)
            {
                float dist = _boss.GlobalPosition.DistanceTo(_boss.GetTarget().GlobalPosition);
                if (dist <= ability.Range)
                {
                    int damage = (int)(_boss.AttackDamage * ability.DamageMultiplier);
                    _boss.GetTarget().TakeDamage(damage);
                    
                    if (ApplyStatusEffect(ability))
                    {
                        ApplyStatusToTarget(_boss.GetTarget(), ability.ApplyStatus.Value, ability.Duration);
                    }
                }
            }
            GD.Print("Dark bolt fired!");
        }
        
        private void UseGroundSlam()
        {
            var ability = _data.GetAbility("ground_slam");
            Vector2 targetPos = _boss.GetTarget()?.GlobalPosition ?? _boss.GlobalPosition;
            
            // Show AOE indicator
            if (Systems.AOEIndicatorManager.Instance != null)
            {
                var worldPos = new Vector3(_boss.GlobalPosition.X, _boss.GlobalPosition.Y, 0);
                Systems.AOEIndicatorManager.Instance.ShowAtWorldPosition(worldPos, ability.AoERadius, true, 1.5f);
            }
            
            SoundEffectSystem.Instance?.PlayBossAbilityGroundSlam();
            ScreenEffectManager.Instance?.TriggerShockwave(0.6f, 1.2f, 0.03f);
            
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("ground_slam", _boss.GlobalPosition, targetPos);
            }
            
            ApplyAoEDamage(ability);
            
            if (ApplyStatusEffect(ability))
            {
                var enemies = _boss.GetTree().GetNodesInGroup("enemy");
                foreach (Node node in enemies)
                {
                    if (node is Enemy enemy && _boss.GlobalPosition.DistanceTo(enemy.GlobalPosition) <= ability.AoERadius)
                    {
                        ApplyStatusToTarget(enemy, StatusEffectType.Stunned, ability.Duration);
                    }
                }
            }
            
            GD.Print("Ground slam! AoE damage and stun.");
        }
        
        private void UseFearShout()
        {
            var ability = _data.GetAbility("fear_shout");
            
            if (Systems.AOEIndicatorManager.Instance != null)
            {
                var worldPos = new Vector3(_boss.GlobalPosition.X, _boss.GlobalPosition.Y, 0);
                Systems.AOEIndicatorManager.Instance.ShowAtWorldPosition(worldPos, ability.AoERadius, true, 1.2f);
            }
            
            SoundEffectSystem.Instance?.PlayBossAbilityFearRoar();
            ScreenEffectManager.Instance?.TriggerShockwave(0.4f, 1.0f, 0.02f);
            
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("fear_roar", _boss.GlobalPosition, _boss.GlobalPosition);
            }
            
            ApplyAoEDamage(ability);
            GD.Print("Fear shout! Enemies terrified.");
        }
        
        private void UseBleedWave()
        {
            var ability = _data.GetAbility("bleed_wave");
            
            SoundEffectSystem.Instance?.PlayBossAbilityBloodRipple();
            
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("blood_ripple", _boss.GlobalPosition, _boss.GlobalPosition);
            }
            
            ApplyAoEDamage(ability);
            
            if (ApplyStatusEffect(ability) && _boss.GetTarget() != null)
            {
                ApplyStatusToTarget(_boss.GetTarget(), StatusEffectType.Bleeding, ability.Duration);
            }
            GD.Print("Bleed wave!");
        }
        
        private void UseMagicMissile()
        {
            var ability = _data.GetAbility("magic_missile");
            Vector2 targetPos = _boss.GetTarget()?.GlobalPosition ?? _boss.GlobalPosition + new Vector2(100, 0);
            
            SoundEffectSystem.Instance?.PlayBossAbilityArcaneMissile();
            
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("arcane_missile", _boss.GlobalPosition, targetPos);
            }
            
            if (_boss.GetTarget() != null)
            {
                float dist = _boss.GlobalPosition.DistanceTo(_boss.GetTarget().GlobalPosition);
                if (dist <= ability.Range)
                {
                    int damage = (int)(_boss.AttackDamage * ability.DamageMultiplier);
                    _boss.GetTarget().TakeDamage(damage);
                }
            }
            GD.Print("Magic missile fired!");
        }
        
        private void UseBossHeal()
        {
            SoundEffectSystem.Instance?.PlayBossAbilitySelfHeal();
            
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("self_heal", _boss.GlobalPosition, _boss.GlobalPosition);
            }
            
            _boss.Heal(_boss.MaxHealth / 4);
            
            var tween = _boss.CreateTween();
            _boss.GetSprite().Modulate = new Color(0f, 1f, 0f);
            tween.TweenProperty(_boss.GetSprite(), "modulate", Color.White, 0.5f);
            GD.Print("Boss healed!");
        }
        
        private void UseTeleport()
        {
            SoundEffectSystem.Instance?.PlayBossAbilityTeleport();
            
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("teleport", _boss.GlobalPosition, _boss.GlobalPosition);
            }
            
            if (_boss.GetTarget() != null)
            {
                float angle = (float)GD.RandRange(0, Mathf.PI * 2);
                float distance = (float)GD.RandRange(100, 200);
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                Vector2 newPos = _boss.GetTarget().GlobalPosition + offset;
                
                if (_visualizer != null)
                {
                    _visualizer.TriggerAbilityVisual("teleport", newPos, newPos);
                }
                
                _boss.GlobalPosition = newPos;
                
                var tween = _boss.CreateTween();
                _boss.GetSprite().Modulate = new Color(0.5f, 0.5f, 1f);
                tween.TweenProperty(_boss.GetSprite(), "modulate", Color.White, 0.3f);
            }
            GD.Print("Boss teleported!");
        }
        
        private void UseSummonMinions()
        {
            SoundEffectSystem.Instance?.PlayBossAbilitySummonMinions();
            
            if (_visualizer != null)
            {
                _visualizer.TriggerAbilityVisual("summon_minions", _boss.GlobalPosition, _boss.GlobalPosition);
            }
            
            for (int i = 0; i < 3; i++)
            {
                float angle = (float)(i * Mathf.PI * 2 / 3);
                float distance = 100f;
                Vector2 spawnPos = _boss.GlobalPosition + new Vector2(
                    Mathf.Cos(angle) * distance,
                    Mathf.Sin(angle) * distance
                );
                
                GD.Print($"Minion {i+1} summoned at {spawnPos}");
            }
            GD.Print("Minions summoned!");
        }
        
        private void UseAreaAttack()
        {
            var ability = _data.GetAbility("ground_slam");
            ApplyAoEDamage(ability);
            GD.Print("Area attack!");
        }
        
        private void ApplyAoEDamage(BossAbility ability)
        {
            if (!ability.IsAoE) return;
            
            var enemies = _boss.GetTree().GetNodesInGroup("player");
            foreach (Node node in enemies)
            {
                if (node is Player player)
                {
                    float dist = _boss.GlobalPosition.DistanceTo(player.GlobalPosition);
                    if (dist <= ability.AoERadius)
                    {
                        int damage = (int)(_boss.AttackDamage * ability.DamageMultiplier);
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
            GD.Print($"Applied {status} for {duration}s to target");
        }
        
        /// <summary>
        /// Get ability cooldown
        /// </summary>
        public float GetAbilityCooldown(string abilityId)
        {
            if (_data.AbilityCurrentCooldowns.ContainsKey(abilityId))
                return _data.AbilityCurrentCooldowns[abilityId];
            return 0f;
        }
    }
}
