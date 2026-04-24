using Godot;
using System;

namespace ClawRPG.Scripts.Characters {
    /// <summary>
    /// Boss Phase - handles phase transitions and enrage mechanics
    /// </summary>
    public class BossPhase
    {
        private Boss _boss;
        private BossData _data;
        
        // Events
        public event Action<int> OnPhaseChange;
        public event Action OnEnrage;
        
        public BossPhase(Boss boss, BossData data)
        {
            _boss = boss;
            _data = data;
        }
        
        /// <summary>
        /// Get current phase
        /// </summary>
        public int GetCurrentPhase() => _data.CurrentPhase;
        
        /// <summary>
        /// Check and handle phase transition
        /// </summary>
        public void CheckPhaseTransition()
        {
            if (_data.PhaseTransitioning) return;
            
            int healthPercent = (_boss.CurrentHealth * 100) / _boss.MaxHealth;
            
            for (int i = 0; i < _data.PhaseHealthThresholds.Length; i++)
            {
                if (healthPercent <= _data.PhaseHealthThresholds[i] && _data.CurrentPhase < i + 2)
                {
                    TransitionToPhase(i + 2);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Transition to new phase
        /// </summary>
        private void TransitionToPhase(int newPhase)
        {
            _data.PhaseTransitioning = true;
            _data.CurrentPhase = newPhase;
            
            GD.Print($"{_data.BossTitle} transitions to Phase {_data.CurrentPhase}!");
            
            // Visual feedback
            ShowPhaseTransitionEffect();
            
            // Increase difficulty
            _boss.MoveSpeed *= 1.2f;
            _boss.AttackDamage *= 1.3f;
            _boss.AttackCooldown *= 0.9f;
            
            // Unlock more abilities in later phases
            if (newPhase >= 2 && !_data.AvailableAbilities.ContainsKey("lightning_chain"))
                _data.AvailableAbilities.Add("lightning_chain");
            if (newPhase >= 3 && !_data.AvailableAbilities.ContainsKey("fear_shout"))
                _data.AvailableAbilities.Add("fear_shout");
            
            OnPhaseChange?.Invoke(_data.CurrentPhase);
            
            _boss.GetTree().CreateTimer(2f).Timeout += () => _data.PhaseTransitioning = false;
        }
        
        /// <summary>
        /// Show phase transition visual effect
        /// </summary>
        private void ShowPhaseTransitionEffect()
        {
            var tween = _boss.CreateTween();
            _boss.GetSprite().Modulate = new Color(1f, 0f, 1f);
            tween.TweenProperty(_boss.GetSprite(), "modulate", Color.White, 2f);
            
            // Screen shake effect
            var main = _boss.GetTree().CurrentScene;
            main.Call("AddScreenShake", 10);
        }
        
        /// <summary>
        /// Update enrage timer
        /// </summary>
        public void UpdateEnrage(float dt)
        {
            if (_data.IsEnraged) return;
            
            _data.EnrageTimer -= dt;
            if (_data.EnrageTimer <= 0)
            {
                TriggerEnrage();
            }
        }
        
        /// <summary>
        /// Trigger enrage mode
        /// </summary>
        private void TriggerEnrage()
        {
            _data.IsEnraged = true;
            _boss.MoveSpeed *= 1.5f;
            _boss.AttackDamage *= 2f;
            _boss.AttackCooldown *= 0.7f;
            
            // Reduce ability cooldowns when enraged
            foreach (var ability in _data.AbilityCurrentCooldowns.Keys)
            {
                _data.AbilityCurrentCooldowns[ability] *= 0.5f;
            }
            
            GD.Print($"{_data.BossTitle} is ENRAGED!");
            ShowEnrageEffect();
            
            OnEnrage?.Invoke();
        }
        
        /// <summary>
        /// Show enrage visual effect
        /// </summary>
        private void ShowEnrageEffect()
        {
            // Apply rage shader effect
            if (_data.RageMaterial != null && _boss.GetSprite() != null)
            {
                _boss.GetSprite().Material = _data.RageMaterial;
                AnimateRageShader();
            }
            
            // Also keep the original modulate effect for compatibility
            var tween = _boss.CreateTween();
            _boss.GetSprite().Modulate = new Color(1f, 0.3f, 0f);
            tween.SetLoops();
            tween.TweenProperty(_boss.GetSprite(), "modulate", new Color(1f, 0f, 0f), 0.5f);
            tween.TweenProperty(_boss.GetSprite(), "modulate", new Color(1f, 0.3f, 0f), 0.5f);
        }
        
        /// <summary>
        /// Animate rage shader
        /// </summary>
        private void AnimateRageShader()
        {
            if (_data.RageMaterial == null) return;
            
            var tween = _boss.CreateTween();
            tween.SetLoops();
            
            tween.TweenCallback(Callable.From(() => {
                _data.RageMaterial.SetShaderParameter("rage_amount", 0.5f);
            }));
            tween.TweenInterval(0.5f);
            tween.TweenCallback(Callable.From(() => {
                _data.RageMaterial.SetShaderParameter("rage_amount", 1.0f);
            }));
            tween.TweenInterval(0.5f);
        }
        
        /// <summary>
        /// Initialize rage shader
        /// </summary>
        public void InitializeRageShader()
        {
            var shader = GD.Load<Shader>("res://Shaders/boss_rage.gdshader");
            if (shader != null)
            {
                _data.RageMaterial = new ShaderMaterial();
                _data.RageMaterial.Shader = shader;
                _data.RageMaterial.SetShaderParameter("rage_amount", 0.0f);
                GD.Print($"Boss {_data.BossTitle} initialized rage shader");
            }
            else
            {
                GD.PrintErr($"Failed to load boss_rage.gdshader for {_data.BossTitle}");
            }
        }
        
        /// <summary>
        /// Check if boss is enraged
        /// </summary>
        public bool IsEnraged() => _data.IsEnraged;

        /// <summary>
        /// Check if boss has triggered HP-based rage (REQ-127)
        /// </summary>
        public bool IsRageTriggered() => _data.IsRageTriggered;

        /// <summary>
        /// Check HP-based rage trigger (REQ-127: HP < 5%)
        /// </summary>
        public void CheckRageTrigger()
        {
            if (_data.IsRageTriggered) return;

            float healthPercent = _boss.CurrentHealth / _boss.MaxHealth;
            float rageThreshold = 0.05f; // 5%

            if (healthPercent <= rageThreshold)
            {
                _data.IsRageTriggered = true;
                TriggerRageEffects();
                GD.Print($"{_data.BossTitle} entered RAGE MODE at {healthPercent * 100:F1}% HP!");
            }
        }

        /// <summary>
        /// Trigger rage mode effects (speed +50%)
        /// </summary>
        private void TriggerRageEffects()
        {
            _boss.MoveSpeed *= 1.5f;
            _boss.AttackCooldown *= 0.67f; // ~50% faster attacks
            ShowRageEffect();
        }

        /// <summary>
        /// Show rage visual effect
        /// </summary>
        private void ShowRageEffect()
        {
            // Apply rage shader effect
            if (_data.RageMaterial != null && _boss.GetSprite() != null)
            {
                _boss.GetSprite().Material = _data.RageMaterial;
                AnimateRageShader();
            }

            // Red pulsing modulate
            var tween = _boss.CreateTween();
            _boss.GetSprite().Modulate = new Color(1f, 0.2f, 0f);
            tween.SetLoops();
            tween.TweenProperty(_boss.GetSprite(), "modulate", new Color(1f, 0f, 0f), 0.3f);
            tween.TweenProperty(_boss.GetSprite(), "modulate", new Color(1f, 0.2f, 0f), 0.3f);
        }

        /// <summary>
        /// Animate rage shader (looping version)
        /// </summary>
        private void AnimateRageShaderLoop()
        {
            if (_data.RageMaterial == null) return;

            var tween = _boss.CreateTween();
            tween.SetLoops();

            tween.TweenCallback(Callable.From(() => {
                _data.RageMaterial.SetShaderParameter("rage_amount", 0.7f);
            }));
            tween.TweenInterval(0.3f);
            tween.TweenCallback(Callable.From(() => {
                _data.RageMaterial.SetShaderParameter("rage_amount", 1.0f);
            }));
            tween.TweenInterval(0.3f);
        }
        
        /// <summary>
        /// Get enrage time remaining
        /// </summary>
        public float GetEnrageTimeRemaining() => _data.EnrageTimer;
        
        /// <summary>
        /// Get enrage percentage
        /// </summary>
        public float GetEnragePercentage() => (_data.EnrageTimer / _data.EnrageTime) * 100f;
    }
}
