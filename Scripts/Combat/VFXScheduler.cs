using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Combat {
    /// <summary>
    /// VFX Scheduler - Manages timing and updates of active effects
    /// Part of CombatVFXSystem refactoring
    /// </summary>
    public partial class VFXScheduler : BaseSystem
    {
        private CombatVFXSystem _vfxSystem;
        
        // Active effect lists managed by scheduler
        private List<DamageNumber> _damageNumbers = new List<DamageNumber>();
        private List<VFXInstance> _vfxInstances = new List<VFXInstance>();
        private List<ScreenEffect> _screenEffects = new List<ScreenEffect>();
        private List<ComboEffect> _comboEffects = new List<ComboEffect>();
        private List<CriticalGlow> _criticalGlows = new List<CriticalGlow>();
        
        // Configuration
        private int _maxDamageNumbers = 50;
        
        public VFXScheduler(CombatVFXSystem vfxSystem)
        {
            _vfxSystem = vfxSystem;
        }
        
        /// <summary>
        /// Set maximum damage numbers
        /// </summary>
        public void SetMaxDamageNumbers(int max)
        {
            _maxDamageNumbers = max;
        }
        
        #region Damage Numbers Management
        
        /// <summary>
        /// Schedule a new damage number
        /// </summary>
        public void ScheduleDamageNumber(DamageNumber dn)
        {
            // Remove oldest if at capacity
            if (_damageNumbers.Count >= _maxDamageNumbers) {
                var oldest = _damageNumbers[0];
                if (oldest.LifeTime > 0) {
                    oldest.CurrentTime = oldest.LifeTime + 1; // Force removal
                }
            }
            
            _damageNumbers.Add(dn);
        }
        
        /// <summary>
        /// Get active damage numbers
        /// </summary>
        public List<DamageNumber> GetActiveDamageNumbers()
        {
            return _damageNumbers;
        }
        
        /// <summary>
        /// Update all damage numbers
        /// </summary>
        public void UpdateDamageNumbers(float delta)
        {
            for (int i = _damageNumbers.Count - 1; i >= 0; i--) {
                var dn = _damageNumbers[i];
                dn.CurrentTime += delta;
                
                if (dn.CurrentTime >= dn.LifeTime) {
                    _damageNumbers.RemoveAt(i);
                }
            }
        }
        
        #endregion
        
        #region VFX Management
        
        /// <summary>
        /// Schedule a new VFX instance
        /// </summary>
        public void ScheduleVFX(VFXInstance vfx)
        {
            _vfxInstances.Add(vfx);
        }
        
        /// <summary>
        /// Get active VFX instances
        /// </summary>
        public List<VFXInstance> GetActiveVFX()
        {
            return _vfxInstances;
        }
        
        /// <summary>
        /// Update all VFX instances
        /// </summary>
        public void UpdateVFX(float delta)
        {
            for (int i = _vfxInstances.Count - 1; i >= 0; i--) {
                var vfx = _vfxInstances[i];
                vfx.CurrentTime += delta;
                
                if (vfx.CurrentTime >= vfx.LifeTime) {
                    _vfxInstances.RemoveAt(i);
                }
            }
        }
        
        #endregion
        
        #region Screen Effects Management
        
        /// <summary>
        /// Schedule a new screen effect
        /// </summary>
        public void ScheduleScreenEffect(ScreenEffect effect)
        {
            _screenEffects.Add(effect);
        }
        
        /// <summary>
        /// Get active screen effects
        /// </summary>
        public List<ScreenEffect> GetActiveScreenEffects()
        {
            return _screenEffects;
        }
        
        /// <summary>
        /// Update all screen effects
        /// </summary>
        public void UpdateScreenEffects(float delta)
        {
            for (int i = _screenEffects.Count - 1; i >= 0; i--) {
                var effect = _screenEffects[i];
                effect.CurrentTime += delta;
                
                if (effect.CurrentTime >= effect.Duration) {
                    _screenEffects.RemoveAt(i);
                }
            }
        }
        
        #endregion
        
        #region Combo Effects Management
        
        /// <summary>
        /// Schedule a new combo effect
        /// </summary>
        public void ScheduleComboEffect(ComboEffect effect)
        {
            _comboEffects.Add(effect);
        }
        
        /// <summary>
        /// Get active combo effects
        /// </summary>
        public List<ComboEffect> GetActiveComboEffects()
        {
            return _comboEffects;
        }
        
        /// <summary>
        /// Update all combo effects
        /// </summary>
        public void UpdateComboEffects(float delta)
        {
            for (int i = _comboEffects.Count - 1; i >= 0; i--) {
                var effect = _comboEffects[i];
                effect.CurrentTime += delta;
                
                if (effect.CurrentTime >= effect.LifeTime) {
                    _comboEffects.RemoveAt(i);
                }
            }
        }
        
        #endregion
        
        #region Critical Glow Management
        
        /// <summary>
        /// Schedule a new critical glow
        /// </summary>
        public void ScheduleCriticalGlow(CriticalGlow glow)
        {
            _criticalGlows.Add(glow);
        }
        
        /// <summary>
        /// Get active critical glows
        /// </summary>
        public List<CriticalGlow> GetActiveCriticalGlows()
        {
            return _criticalGlows;
        }
        
        /// <summary>
        /// Update all critical glows
        /// </summary>
        public void UpdateCriticalGlows(float delta)
        {
            for (int i = _criticalGlows.Count - 1; i >= 0; i--) {
                var glow = _criticalGlows[i];
                glow.CurrentTime += delta;
                
                if (glow.CurrentTime >= glow.Duration) {
                    _criticalGlows.RemoveAt(i);
                }
            }
        }
        
        #endregion
        
        /// <summary>
        /// Update all scheduled effects
        /// </summary>
        public void UpdateAll(float delta)
        {
            UpdateDamageNumbers(delta);
            UpdateVFX(delta);
            UpdateScreenEffects(delta);
            UpdateComboEffects(delta);
            UpdateCriticalGlows(delta);
        }
        
        /// <summary>
        /// Clear all scheduled effects
        /// </summary>
        public void ClearAll()
        {
            _damageNumbers.Clear();
            _vfxInstances.Clear();
            _screenEffects.Clear();
            _comboEffects.Clear();
            _criticalGlows.Clear();
        }
        
        /// <summary>
        /// Get total active effects count
        /// </summary>
        public int GetTotalActiveEffects()
        {
            return _damageNumbers.Count + _vfxInstances.Count + _screenEffects.Count + 
                   _comboEffects.Count + _criticalGlows.Count;
        }
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["maxDamageNumbers"] = _maxDamageNumbers;
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data.Contains("maxDamageNumbers")) {
                _maxDamageNumbers = Convert.ToInt32(data["maxDamageNumbers"]);
            }
        }
    }
}
