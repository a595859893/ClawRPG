using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Counter attack system - allows players to perform counter attacks after perfect blocks
    /// </summary>
    public partial class CounterAttackSystem : Node
    {
        public static CounterAttackSystem Instance { get; private set; }
        
        // Counter attack data
        public class CounterAttackData
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public float DamageMultiplier { get; set; }
            public float StaminaCost { get; set; }
            public float Cooldown { get; set; }
            public float ExecutionWindow { get; set; } // Time window after perfect block to counter
            public StatusEffectType? ApplyStatus { get; set; }
            public float StatusChance { get; set; }
        }
        
        // Counter attack types
        public enum CounterType
        {
            Riposte,           // Basic counter - quick counter after perfect block
            ShieldBash,        // Counter with shield - knocks back enemy
            BladeDance,        // Multi-hit counter attack
            IronWill,          // Defensive counter - grants temporary invincibility
            BloodRevenge,     // Offensive counter - deals damage based on lost HP
            MagicCounter       // Counter with magic damage
        }
        
        // Player counter attack state
        public bool IsCounterAttacking { get; private set; }
        public CounterType CurrentCounterType { get; private set; }
        public float CounterCooldownTimer { get; private set; }
        public float ExecutionWindowTimer { get; private set; }
        public bool CanCounter { get; private set; }
        
        // Counter attack database
        private Dictionary<CounterType, CounterAttackData> _counterAttacks;
        
        // Tutorial tracking
        private bool _hasTriggeredFirstCounter = false;
        
        // Signals
        [Signal]
        public delegate void CounterAttackPerformedEventHandler(CounterType type, float damage);
        
        [Signal]
        public delegate void CounterAttack窗口EventHandler(bool isActive);
        
        [Signal]
        public delegate void CounterAttackReadyEventHandler();
        
        public override void _Ready()
        {
            Instance = this;
            _InitializeCounterAttacks();
            CanCounter = true;
            CounterCooldownTimer = 0f;
            ExecutionWindowTimer = 0f;
        }
        
        private void _InitializeCounterAttacks()
        {
            _counterAttacks = new Dictionary<CounterType, CounterAttackData>
            {
                { CounterType.Riposte, new CounterAttackData
                    {
                        Name = "弹反",
                        Description = "在完美格挡后的快速反击，造成额外伤害",
                        DamageMultiplier = 1.5f,
                        StaminaCost = 20f,
                        Cooldown = 3f,
                        ExecutionWindow = 0.5f,
                        ApplyStatus = null,
                        StatusChance = 0f
                    }
                },
                { CounterType.ShieldBash, new CounterAttackData
                    {
                        Name = "盾击",
                        Description = "用盾牌猛击敌人，造成眩晕",
                        DamageMultiplier = 1.2f,
                        StaminaCost = 30f,
                        Cooldown = 4f,
                        ExecutionWindow = 0.6f,
                        ApplyStatus = StatusEffectType.Stunned,
                        StatusChance = 0.5f
                    }
                },
                { CounterType.BladeDance, new CounterAttackData
                    {
                        Name = "刀舞",
                        Description = "快速多段反击，造成大量伤害",
                        DamageMultiplier = 2.5f,
                        StaminaCost = 40f,
                        Cooldown = 5f,
                        ExecutionWindow = 0.4f,
                        ApplyStatus = StatusEffectType.Bleeding,
                        StatusChance = 0.3f
                    }
                },
                { CounterType.IronWill, new CounterAttackData
                    {
                        Name = "铁意志",
                        Description = "防御性反击，获得短暂无敌",
                        DamageMultiplier = 0.8f,
                        StaminaCost = 25f,
                        Cooldown = 6f,
                        ExecutionWindow = 0.7f,
                        ApplyStatus = StatusEffectType.Invincibility,
                        StatusChance = 1f
                    }
                },
                { CounterType.BloodRevenge, new CounterAttackData
                    {
                        Name = "血之复仇",
                        Description = "根据损失的生命值造成伤害",
                        DamageMultiplier = 2.0f,
                        StaminaCost = 35f,
                        Cooldown = 8f,
                        ExecutionWindow = 0.5f,
                        ApplyStatus = StatusEffectType.Bleeding,
                        StatusChance = 0.4f
                    }
                },
                { CounterType.MagicCounter, new CounterAttackData
                    {
                        Name = "魔法反击",
                        Description = "使用魔法能量进行反击",
                        DamageMultiplier = 1.8f,
                        StaminaCost = 30f,
                        Cooldown = 4f,
                        ExecutionWindow = 0.5f,
                        ApplyStatus = StatusEffectType.Slowed,
                        StatusChance = 0.6f
                    }
                }
            };
        }
        
        public override void _Process(double delta)
        {
            float deltaF = (float)delta;
            
            // Update cooldown timer
            if (CounterCooldownTimer > 0)
            {
                CounterCooldownTimer -= deltaF;
                if (CounterCooldownTimer <= 0)
                {
                    CounterCooldownTimer = 0;
                    CanCounter = true;
                    EmitSignal(SignalName.CounterAttackReady);
                }
            }
            
            // Update execution window timer
            if (ExecutionWindowTimer > 0)
            {
                ExecutionWindowTimer -= deltaF;
                if (ExecutionWindowTimer <= 0)
                {
                    ExecutionWindowTimer = 0;
                    IsCounterAttacking = false;
                    EmitSignal(SignalName.CounterAttack窗口, false);
                }
            }
        }
        
        /// <summary>
        /// Called when player performs a perfect block - starts the counter window
        /// </summary>
        public void OnPerfectBlock()
        {
            if (!CanCounter) return;
            
            // Start the counter execution window
            var counterData = _counterAttacks[CurrentCounterType];
            ExecutionWindowTimer = counterData.ExecutionWindow;
            IsCounterAttacking = true;
            EmitSignal(SignalName.CounterAttack窗口, true);
        }
        
        /// <summary>
        /// Attempt to perform a counter attack
        /// </summary>
        public bool TryCounterAttack(Characters.Player player, Characters.Enemy target)
        {
            if (!IsCounterAttacking || !CanCounter)
            {
                return false;
            }
            
            var counterData = _counterAttacks[CurrentCounterType];
            
            // Check stamina
            if (player.Stamina < counterData.StaminaCost)
            {
                return false;
            }
            
            // Consume stamina
            player.Stamina -= counterData.StaminaCost;
            
            // Calculate damage
            float baseDamage = player.TotalAttack;
            float damage = baseDamage * counterData.DamageMultiplier;
            
            // Special calculation for Blood Revenge
            if (CurrentCounterType == CounterType.BloodRevenge)
            {
                float missingHpPercent = 1f - ((float)player.Health / player.MaxHealth);
                damage *= (1f + missingHpPercent * 2f);
            }
            
            // Apply damage to target
            if (target != null)
            {
                target.TakeDamage((int)damage, isCritical: false);
                
                // Apply status effect if any
                if (counterData.ApplyStatus.HasValue && GD.Randf() < counterData.StatusChance)
                {
                    target.ApplyStatusEffect(counterData.ApplyStatus.Value, 3f, 1f);
                }
            }
            
            // Start cooldown
            CounterCooldownTimer = counterData.Cooldown;
            CanCounter = false;
            IsCounterAttacking = false;
            ExecutionWindowTimer = 0;
            
            // Emit signals
            EmitSignal(SignalName.CounterAttackPerformed, CurrentCounterType, damage);
            EmitSignal(SignalName.CounterAttack窗口, false);
            
            // Track counter attack achievements
            AchievementManager.Instance?.TrackCounterAttack(1);
            
            // Trigger tutorial for first counter attack
            if (!_hasTriggeredFirstCounter)
            {
                _hasTriggeredFirstCounter = true;
                TutorialSystem.Trigger(TutorialTrigger.FirstCounter);
            }
            
            return true;
        }
        
        /// <summary>
        /// Set the current counter attack type
        /// </summary>
        public void SetCounterType(CounterType type)
        {
            if (_counterAttacks.ContainsKey(type))
            {
                CurrentCounterType = type;
            }
        }
        
        /// <summary>
        /// Get counter attack data
        /// </summary>
        public CounterAttackData GetCounterAttackData(CounterType type)
        {
            return _counterAttacks.ContainsKey(type) ? _counterAttacks[type] : null;
        }
        
        /// <summary>
        /// Get current counter attack data
        /// </summary>
        public CounterAttackData GetCurrentCounterData()
        {
            return _counterAttacks[CurrentCounterType];
        }
        
        /// <summary>
        /// Get all counter attack types
        /// </summary>
        public CounterType[] GetAllCounterTypes()
        {
            return (CounterType[])Enum.GetValues(typeof(CounterType));
        }
        
        /// <summary>
        /// Get cooldown progress (0-1)
        /// </summary>
        public float GetCooldownProgress()
        {
            if (CanCounter) return 1f;
            var counterData = _counterAttacks[CurrentCounterType];
            return 1f - (CounterCooldownTimer / counterData.Cooldown);
        }
        
        /// <summary>
        /// Get execution window progress (0-1)
        /// </summary>
        public float GetExecutionWindowProgress()
        {
            if (!IsCounterAttacking) return 0f;
            var counterData = _counterAttacks[CurrentCounterType];
            return ExecutionWindowTimer / counterData.ExecutionWindow;
        }
        
        /// <summary>
        /// Cancel counter attack window
        /// </summary>
        public void CancelCounterWindow()
        {
            ExecutionWindowTimer = 0;
            IsCounterAttacking = false;
            EmitSignal(SignalName.CounterAttack窗口, false);
        }
    }
}
