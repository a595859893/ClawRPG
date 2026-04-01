using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Combat;

namespace ClawRPG.Scripts.Combat
{
    public partial class CombatUISystem
    {
        #region Damage Display
        
        /// <summary>
        /// Display damage number at target position
        /// </summary>
        public void ShowDamageNumber(float damage, DamageDisplayType type, Vector3 worldPosition, bool isPlayerSource = true)
        {
            if (!_uiPreferences.ShowDamageNumbers) return;
            
            var damageText = new DamageTextData
            {
                Amount = damage,
                DisplayType = type,
                Position = worldPosition,
                Lifetime = 1.0f,
                Scale = type == DamageDisplayType.Critical ? 1.5f : 1.0f,
                IsPlayerSource = isPlayerSource
            };
            
            _activeDamageTexts.Add(damageText);
            
            // Update statistics
            if (isPlayerSource)
            {
                _currentSessionStats.TotalDamageDealt += (int)damage;
                if (damage > _currentSessionStats.HighestDamage)
                {
                    _currentSessionStats.HighestDamage = damage;
                }
                
                if (type == DamageDisplayType.Critical)
                {
                    _currentSessionStats.CriticalHits++;
                    TriggerScreenEffect("critical_hit");
                }
                else
                {
                    TriggerScreenEffect("light_damage");
                }
                
                // Update combo
                if (damage > 0)
                {
                    AddComboHit(damage);
                }
            }
            else
            {
                _currentSessionStats.TotalDamageTaken += (int)damage;
                
                if (type == DamageDisplayType.Blocked)
                {
                    _currentSessionStats.Blocks++;
                    TriggerScreenEffect("perfect_block");
                }
                else if (damage > _playerState.CurrentHealth * 0.3f)
                {
                    TriggerScreenEffect("heavy_damage");
                }
                else
                {
                    TriggerScreenEffect("light_damage");
                }
            }
            
            EmitSignal(SignalDamageDealt, damage, isPlayerSource);
        }
        
        /// <summary>
        /// Display healing number
        /// </summary>
        public void ShowHealing(float amount, Vector3 worldPosition)
        {
            var healText = new DamageTextData
            {
                Amount = amount,
                DisplayType = DamageDisplayType.Healing,
                Position = worldPosition,
                Lifetime = 1.2f,
                Scale = 1.2f,
                IsPlayerSource = true
            };
            
            _activeDamageTexts.Add(healText);
            _currentSessionStats.TotalHealing += (int)amount;
            
            EmitSignal(SignalHealing, amount);
        }
        
        /// <summary>
        /// Display miss indicator
        /// </summary>
        public void ShowMiss(Vector3 worldPosition, bool isPlayerSource)
        {
            if (!_uiPreferences.ShowDamageNumbers) return;
            
            var missText = new DamageTextData
            {
                Amount = 0,
                DisplayType = DamageDisplayType.Miss,
                Position = worldPosition,
                Lifetime = 0.8f,
                Scale = 1.0f,
                IsPlayerSource = isPlayerSource
            };
            
            _activeDamageTexts.Add(missText);
        }
        
        /// <summary>
        /// Show combat indicator message
        /// </summary>
        public void ShowCombatIndicator(CombatIndicatorType type, string message = "")
        {
            if (!_uiPreferences.ShowCombatIndicators) return;
            
            // Implementation would create UI indicator
            // Different types: Combo, Buff, Debuff, BossPhase, etc.
        }
        
        /// <summary>
        /// Show buff/debuff applied
        /// </summary>
        public void ShowBuff(string buffName, bool isPositive = true)
        {
            if (!_uiPreferences.ShowCombatIndicators) return;
            ShowCombatIndicator(isPositive ? CombatIndicatorType.Buff : CombatIndicatorType.Debuff, buffName);
        }
        
        /// <summary>
        /// Show enemy kill notification
        /// </summary>
        public void ShowKill(string enemyName)
        {
            _currentSessionStats.EnemiesKilled++;
            ShowCombatIndicator(CombatIndicatorType.Kill, enemyName);
            EmitSignal(SignalKill, enemyName);
        }
        
        /// <summary>
        /// Show boss phase change
        /// </summary>
        public void ShowBossPhase(int newPhase, int totalPhases)
        {
            ShowCombatIndicator(CombatIndicatorType.BossPhase, $"{newPhase}/{totalPhases}");
        }
        
        /// <summary>
        /// Get priority for combat indicator
        /// </summary>
        private int GetIndicatorPriority(CombatIndicatorType type)
        {
            switch (type)
            {
                case CombatIndicatorType.BossPhase:
                    return 5;
                case CombatIndicatorType.Combo:
                    return 4;
                case CombatIndicatorType.Buff:
                case CombatIndicatorType.Debuff:
                    return 3;
                case CombatIndicatorType.Kill:
                    return 2;
                default:
                    return 0;
            }
        }
        
        #endregion
    }
}
