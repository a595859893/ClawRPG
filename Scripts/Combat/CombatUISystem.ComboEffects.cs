using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Combat;

namespace ClawRPG.Scripts.Combat
{
    public partial class CombatUISystem
    {
        #region Combo System
        
        /// <summary>
        /// Add hit to combo chain
        /// </summary>
        public void AddComboHit(float damage)
        {
            _currentCombo.CurrentCombo++;
            _currentCombo.ComboHits++;
            _currentCombo.ComboDamage += damage;
            _currentCombo.ComboTimer = _currentCombo.MaxComboTime;
            
            if (_currentCombo.CurrentCombo > _currentCombo.MaxCombo)
            {
                _currentCombo.MaxCombo = _currentCombo.CurrentCombo;
            }
            
            // Check for milestone
            var milestone = _database.GetComboMilestone(_currentCombo.CurrentCombo);
            if (milestone != null)
            {
                ShowCombatIndicator(CombatIndicatorType.Combo, milestone.Message.Replace("{N}", _currentCombo.CurrentCombo.ToString()));
                EmitSignal(SignalComboMilestone, _currentCombo.CurrentCombo, milestone.Message);
                
                if (_currentCombo.CurrentCombo >= 10)
                {
                    TriggerScreenEffect("kill_streak");
                }
            }
        }
        
        /// <summary>
        /// Reset combo chain
        /// </summary>
        public void ResetCombo()
        {
            if (_currentCombo.CurrentCombo > 0)
            {
                GD.Print($"[CombatUI] Combo ended: {_currentCombo.MaxCombo} max hits, {_currentCombo.ComboDamage} total damage");
            }
            
            _currentCombo.CurrentCombo = 0;
            _currentCombo.ComboHits = 0;
            _currentCombo.ComboDamage = 0;
            _currentCombo.ComboTimer = 0;
        }
        
        /// <summary>
        /// Get current combo data
        /// </summary>
        public ComboChainData GetCurrentCombo()
        {
            return _currentCombo;
        }
        
        #endregion
        
        #region Screen Effects
        
        /// <summary>
        /// Trigger screen effect
        /// </summary>
        public void TriggerScreenEffect(string effectName, float intensity = 1.0f)
        {
            var effect = new ScreenEffectTrigger
            {
                EffectName = effectName,
                Intensity = intensity,
                Duration = 0.5f
            };
            
            _screenEffectQueue.Enqueue(effect);
            EmitSignal(SignalScreenEffect, effectName, intensity);
        }
        
        /// <summary>
        /// Process queued screen effects
        /// </summary>
        private void ProcessScreenEffects()
        {
            // Process one effect per frame
            if (_screenEffectQueue.Count > 0)
            {
                var effect = _screenEffectQueue.Dequeue();
                var config = _database.GetScreenEffect(effect.EffectName);
                
                if (config != null)
                {
                    ApplyScreenEffect(config, effect.Intensity);
                }
            }
        }
        
        private void ApplyScreenEffect(ScreenEffectConfig config, float intensity)
        {
            if (config.ScreenShake)
            {
                // Apply screen shake
                float shakeIntensity = config.ShakeIntensity * intensity;
                // Screen shake implementation would go here
            }
            
            if (config.FlashColor != null)
            {
                // Apply screen flash
                float flashAlpha = config.FlashAlpha * intensity;
                // Screen flash implementation would go here
            }
            
            if (config.SlowMotion)
            {
                // Apply slow motion
                float timeScale = config.SlowMotionScale;
                // Engine.time_scale = timeScale would be applied
            }
        }
        
        #endregion
    }
}
