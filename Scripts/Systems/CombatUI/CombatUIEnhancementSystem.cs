using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Systems.CombatUI;

namespace ClawRPG.Systems
{
    /// <summary>
    /// Combat UI Enhancement System - 战斗UI增强系统核心
    /// 动态血条、技能冷却动画、战斗状态指示器、连击计数器
    /// 应用 Advanced Shader Effects (trauma-based shake, visual feedback)
    /// </summary>
    public class CombatUIEnhancementSystem : Node
    {
        private static CombatUIEnhancementSystem _instance;
        public static CombatUIEnhancementSystem Instance => _instance;
        
        private CombatUIEnhancementData _data;
        private Dictionary<string, SkillCooldownData> _skillCooldowns = new Dictionary<string, SkillCooldownData>();
        private List<StatusEffectData> _activeStatusEffects = new List<StatusEffectData>();
        private CombatStateData _combatState = new CombatStateData();
        
        // Screen effect integration references
        private Node _screenEffectSystem;
        
        // Trauma-based shake system
        private float _trauma = 0f;
        private Vector2 _shakeOffset = Vector2.Zero;
        
        // Combo system
        private Timer _comboTimer;
        private int _currentCombo = 0;
        
        // Health bar interpolation
        private float _displayedHealth = 100f;
        private float _targetHealth = 100f;
        private float _healthLerpSpeed = 5f;
        
        // Critical hit screen flash
        private float _criticalFlashIntensity = 0f;
        private Color _criticalFlashColor = new Color(1f, 0.8f, 0f, 0f);
        
        public override void _Ready()
        {
            _instance = this;
            _data = new CombatUIEnhancementData();
            
            _comboTimer = new Timer();
            _comboTimer.WaitTime = _data.ComboTimeout;
            _comboTimer.OneShot = true;
            _comboTimer.Connect("timeout", this, nameof(_OnComboTimeout));
            AddChild(_comboTimer);
            
            // Try to get screen effect system for integration
            _screenEffectSystem = GetNode("/root/Main/ScreenEffectSystem");
            
            GD.Print("[CombatUIEnhancement] System initialized");
        }
        
        public override void _Process(float delta)
        {
            // Process trauma-based shake
            if (_trauma > 0f)
            {
                _trauma = Mathf.Max(0f, _trauma - delta * 0.5f);
                float shakeIntensity = _trauma * _trauma * _data.ScreenShakeIntensity;
                _shakeOffset = new Vector2(
                    (float)GD.Randf() * 2f - 1f,
                    (float)GD.Randf() * 2f - 1f
                ) * shakeIntensity * 10f;
            }
            
            // Health bar smooth interpolation
            if (Mathf.Abs(_displayedHealth - _targetHealth) > 0.1f)
            {
                _displayedHealth = Mathf.Lerp(_displayedHealth, _targetHealth, delta * _healthLerpSpeed);
            }
            
            // Critical flash fade
            if (_criticalFlashIntensity > 0f)
            {
                _criticalFlashIntensity = Mathf.Max(0f, _criticalFlashIntensity - delta * 4f);
            }
            
            // Update skill cooldowns
            foreach (var skill in _skillCooldowns.Values)
            {
                if (skill.CurrentCooldown > 0f)
                {
                    skill.CurrentCooldown = Mathf.Max(0f, skill.CurrentCooldown - delta);
                }
            }
            
            // Update status effects
            for (int i = _activeStatusEffects.Count - 1; i >= 0; i--)
            {
                _activeStatusEffects[i].RemainingTime -= delta;
                if (_activeStatusEffects[i].RemainingTime <= 0f)
                {
                    _activeStatusEffects.RemoveAt(i);
                }
            }
        }
        
        #region Health Bar Methods
        
        public void UpdateHealth(float current, float max)
        {
            float healthPercent = max > 0 ? current / max : 0f;
            _targetHealth = healthPercent * 100f;
            
            // Trigger screen effects on low health
            if (healthPercent < 0.25f && _data.ScreenFlashOnCritical)
            {
                _criticalFlashIntensity = 0.3f;
                _criticalFlashColor = new Color(1f, 0f, 0f, _criticalFlashIntensity);
            }
        }
        
        public float GetDisplayedHealthPercent() => _displayedHealth / 100f;
        
        public Color GetHealthBarColor()
        {
            float healthPercent = GetDisplayedHealthPercent();
            if (healthPercent < 0.25f)
                return _data.HealthBarCriticalColor;
            else if (healthPercent < 0.5f)
                return _data.HealthBarLowColor;
            return _data.HealthBarColor;
        }
        
        #endregion
        
        #region Skill Cooldown Methods
        
        public void RegisterSkill(string skillId, string skillName, float cooldown)
        {
            if (!_skillCooldowns.ContainsKey(skillId))
            {
                _skillCooldowns[skillId] = new SkillCooldownData
                {
                    SkillId = skillId,
                    SkillName = skillName,
                    MaxCooldown = cooldown,
                    CurrentCooldown = 0f
                };
            }
        }
        
        public void StartSkillCooldown(string skillId)
        {
            if (_skillCooldowns.ContainsKey(skillId))
            {
                _skillCooldowns[skillId].CurrentCooldown = _skillCooldowns[skillId].MaxCooldown;
            }
        }
        
        public float GetSkillCooldownPercent(string skillId)
        {
            if (_skillCooldowns.ContainsKey(skillId))
            {
                return _skillCooldowns[skillId].GetCooldownPercent();
            }
            return 0f;
        }
        
        public bool IsSkillReady(string skillId)
        {
            if (_skillCooldowns.ContainsKey(skillId))
            {
                return _skillCooldowns[skillId].IsReady;
            }
            return true;
        }
        
        public List<SkillCooldownData> GetAllCooldowns()
        {
            return new List<SkillCooldownData>(_skillCooldowns.Values);
        }
        
        #endregion
        
        #region Combat State Methods
        
        public void SetCombatState(CombatStateData.CombatState newState)
        {
            _combatState.CurrentState = newState;
            _combatState.StateTimer = 0f;
            
            // Apply screen shake on state change
            if (newState == CombatStateData.CombatState.Fighting && _data.ScreenShakeOnHit)
            {
                ApplyShake(0.3f);
            }
        }
        
        public CombatStateData.CombatState GetCurrentState() => _combatState.CurrentState;
        
        public void ProcessCombatState(float delta)
        {
            _combatState.StateTimer += delta;
        }
        
        #endregion
        
        #region Combo System
        
        public void RegisterHit(bool isCritical = false)
        {
            _currentCombo++;
            _comboTimer.Start();
            
            if (_currentCombo > _data.HighestComboCount)
            {
                _data.HighestComboCount = _currentCombo;
            }
            
            _data.TotalCombosTriggered++;
            
            // Critical hit effects
            if (isCritical)
            {
                _data.TotalCriticals++;
                if (_data.ScreenFlashOnCritical)
                {
                    _criticalFlashIntensity = 0.6f;
                    _criticalFlashColor = new Color(1f, 0.8f, 0f, _criticalFlashIntensity);
                }
                if (_data.ScreenShakeOnHit)
                {
                    ApplyShake(0.5f);
                }
            }
            
            // Combo shake effect
            if (_data.ComboShakeEnabled && _currentCombo > 3)
            {
                ApplyShake(0.2f * Mathf.Min(_currentCombo * 0.1f, 1f));
            }
            
            _combatState.IsCritical = isCritical;
            _combatState.LastHitTime = Time.GetTicksMsec() / 1000f;
        }
        
        public int GetCurrentCombo() => _currentCombo;
        
        private void _OnComboTimeout()
        {
            _currentCombo = 0;
        }
        
        #endregion
        
        #region Status Effects
        
        public void AddStatusEffect(StatusEffectData.EffectType type, float duration, float intensity = 1.0f)
        {
            var effect = new StatusEffectData
            {
                Type = type,
                Duration = duration,
                RemainingTime = duration,
                Intensity = intensity,
                DisplayName = GetStatusEffectName(type),
                EffectColor = GetStatusEffectColor(type)
            };
            
            _activeStatusEffects.Add(effect);
        }
        
        public void RemoveStatusEffect(StatusEffectData.EffectType type)
        {
            _activeStatusEffects.RemoveAll(e => e.Type == type);
        }
        
        public List<StatusEffectData> GetActiveEffects() => new List<StatusEffectData>(_activeStatusEffects);
        
        private string GetStatusEffectName(StatusEffectData.EffectType type)
        {
            switch (type)
            {
                case StatusEffectData.EffectType.Poison: return "Poison";
                case StatusEffectData.EffectType.Burn: return "Burn";
                case StatusEffectData.EffectType.Freeze: return "Freeze";
                case StatusEffectData.EffectType.Stun: return "Stun";
                case StatusEffectData.EffectType.Slow: return "Slow";
                case StatusEffectData.EffectType.Bleed: return "Bleed";
                case StatusEffectData.EffectType.Blind: return "Blind";
                case StatusEffectData.EffectType.Silence: return "Silence";
                case StatusEffectData.EffectType.Taunt: return "Taunt";
                case StatusEffectData.EffectType.Shield: return "Shield";
                default: return "Unknown";
            }
        }
        
        private Color GetStatusEffectColor(StatusEffectData.EffectType type)
        {
            switch (type)
            {
                case StatusEffectData.EffectType.Poison: return new Color(0.4f, 0.8f, 0.2f);
                case StatusEffectData.EffectType.Burn: return new Color(1f, 0.4f, 0f);
                case StatusEffectData.EffectType.Freeze: return new Color(0.4f, 0.8f, 1f);
                case StatusEffectData.EffectType.Stun: return new Color(0.8f, 0.8f, 0.2f);
                case StatusEffectData.EffectType.Slow: return new Color(0.6f, 0.6f, 0.8f);
                case StatusEffectData.EffectType.Bleed: return new Color(0.8f, 0.1f, 0.1f);
                case StatusEffectData.EffectType.Blind: return new Color(0.4f, 0.4f, 0.4f);
                case StatusEffectData.EffectType.Silence: return new Color(0.6f, 0.3f, 0.8f);
                case StatusEffectData.EffectType.Taunt: return new Color(0.9f, 0.5f, 0.2f);
                case StatusEffectData.EffectType.Shield: return new Color(0.3f, 0.5f, 1f);
                default: return Colors.White;
            }
        }
        
        #endregion
        
        #region Screen Effects (Trauma-based Shake)
        
        public void ApplyShake(float amount)
        {
            _trauma = Mathf.Clamp01(_trauma + amount);
        }
        
        public Vector2 GetShakeOffset() => _shakeOffset;
        
        public float GetTrauma() => _trauma;
        
        #endregion
        
        #region Statistics
        
        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                { "total_combos", _data.TotalCombosTriggered },
                { "highest_combo", _data.HighestComboCount },
                { "total_criticals", _data.TotalCriticals },
                { "damage_mitigated", _data.TotalDamageMitigated }
            };
        }
        
        public void RecordDamageMitigated(float amount)
        {
            _data.TotalDamageMitigated += amount;
        }
        
        #endregion
        
        #region Settings
        
        public void SetEnabled(bool enabled)
        {
            _data.Enabled = enabled;
        }
        
        public bool IsEnabled() => _data.Enabled;
        
        public void SetComboTimeout(float timeout)
        {
            _data.ComboTimeout = timeout;
            _comboTimer.WaitTime = timeout;
        }
        
        public void SetShakeIntensity(float intensity)
        {
            _data.ScreenShakeIntensity = intensity;
        }
        
        #endregion
        
        #region Save/Load
        
        public Dictionary<string, object> Save()
        {
            return new Dictionary<string, object>
            {
                { "enabled", _data.Enabled },
                { "total_combos", _data.TotalCombosTriggered },
                { "highest_combo", _data.HighestComboCount },
                { "total_criticals", _data.TotalCriticals },
                { "damage_mitigated", _data.TotalDamageMitigated }
            };
        }
        
        public void Load(Dictionary<string, object> saveData)
        {
            if (saveData.ContainsKey("enabled"))
                _data.Enabled = (bool)saveData["enabled"];
            if (saveData.ContainsKey("total_combos"))
                _data.TotalCombosTriggered = (int)saveData["total_combos"];
            if (saveData.ContainsKey("highest_combo"))
                _data.HighestComboCount = (int)saveData["highest_combo"];
            if (saveData.ContainsKey("total_criticals"))
                _data.TotalCriticals = (int)saveData["total_criticals"];
            if (saveData.ContainsKey("damage_mitigated"))
                _data.TotalDamageMitigated = (float)saveData["damage_mitigated"];
        }
        
        #endregion
    }
}
