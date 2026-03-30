using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 反击系统 - 允许玩家在完美格挡后进行反击
    /// </summary>
    public partial class CounterAttackSystem : BaseSystem
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        public static CounterAttackSystem Instance { get; private set; }
        
        // Counter attack data
        
        /// <summary>
        /// 反击数据 - 定义一种反击方式的配置
        /// </summary>
        public class CounterAttackData
        {
            /// <summary>
            /// 反击名称
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 反击描述
            /// </summary>
            public string Description { get; set; }
            /// <summary>
            /// 伤害倍率
            /// </summary>
            public float DamageMultiplier { get; set; }
            /// <summary>
            /// 体力消耗
            /// </summary>
            public float StaminaCost { get; set; }
            /// <summary>
            /// 冷却时间（秒）
            /// </summary>
            public float Cooldown { get; set; }
            /// <summary>
            /// 反击时间窗口（秒）- 完美格挡后可以进行反击的时间
            /// </summary>
            public float ExecutionWindow { get; set; }
            /// <summary>
            /// 附加的状态效果
            /// </summary>
            public StatusEffectType? ApplyStatus { get; set; }
            /// <summary>
            /// 状态效果触发几率
            /// </summary>
            public float StatusChance { get; set; }
        }
        
        // Counter attack types
        
        /// <summary>
        /// 反击类型枚举
        /// </summary>
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
        
        /// <summary>
        /// 是否正在进行反击
        /// </summary>
        public bool IsCounterAttacking { get; private set; }
        
        /// <summary>
        /// 当前选择的反击类型
        /// </summary>
        public CounterType CurrentCounterType { get; private set; }
        
        /// <summary>
        /// 反击冷却计时器
        /// </summary>
        public float CounterCooldownTimer { get; private set; }
        
        /// <summary>
        /// 反击时间窗口计时器
        /// </summary>
        public float ExecutionWindowTimer { get; private set; }
        
        /// <summary>
        /// 是否可以进行反击
        /// </summary>
        public bool CanCounter { get; private set; }
        
        // Counter attack database
        private Dictionary<CounterType, CounterAttackData> _counterAttacks;
        
        // Tutorial tracking
        private bool _hasTriggeredFirstCounter = false; 
        
        // Signals
public delegate void CounterAttackPerformedEventHandler(CounterType type, float damage);
public delegate void CounterAttack窗口EventHandler(bool isActive);
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
        
        // ===== 持久化 =====
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            data["has_triggered_first_counter"] = _hasTriggeredFirstCounter;
            data["current_counter_type"] = (int)CurrentCounterType;
            data["counter_cooldown_timer"] = CounterCooldownTimer;
            data["execution_window_timer"] = ExecutionWindowTimer;
            data["can_counter"] = CanCounter;
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            if (data.ContainsKey("has_triggered_first_counter"))
                _hasTriggeredFirstCounter = Convert.ToBoolean(data["has_triggered_first_counter"]);
            if (data.ContainsKey("current_counter_type"))
                CurrentCounterType = (CounterType)Convert.ToInt32(data["current_counter_type"]);
            if (data.ContainsKey("counter_cooldown_timer"))
                CounterCooldownTimer = Convert.ToSingle(data["counter_cooldown_timer"]);
            if (data.ContainsKey("execution_window_timer"))
                ExecutionWindowTimer = Convert.ToSingle(data["execution_window_timer"]);
            if (data.ContainsKey("can_counter"))
                CanCounter = Convert.ToBoolean(data["can_counter"]);
        }
    }
}
