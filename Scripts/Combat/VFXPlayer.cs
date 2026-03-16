using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Combat {
    /// <summary>
    /// VFX Player - Plays and animates visual effects
    /// Part of CombatVFXSystem refactoring
    /// </summary>
    public partial class VFXPlayer : BaseSystem
    {
        private CombatVFXSystem _vfxSystem;
        private VFXFactory _factory;
        
        // Animation settings
        private float _defaultVFXLifetime = 1.5f;
        private float _defaultDamageNumberLifetime = 1.5f;
        
        public VFXPlayer(CombatVFXSystem vfxSystem, VFXFactory factory)
        {
            _vfxSystem = vfxSystem;
            _factory = factory;
        }
        
        /// <summary>
        /// Play damage number with animation
        /// </summary>
        public void PlayDamageNumber(DamageNumber dn, Control container)
        {
            var label = _factory.CreateDamageNumberUI(dn);
            container.AddChild(label);
            
            // Create animation
            var tween = _vfxSystem.CreateTween();
            tween.SetParallel(true);
            
            // Move up
            tween.TweenProperty(label, "position:y", label.Position.y + dn.Velocity.Y * _defaultDamageNumberLifetime * 0.5f, _defaultDamageNumberLifetime);
            
            // Horizontal movement (special for critical)
            if (dn.Type == DamageNumberType.Critical) {
                tween.TweenProperty(label, "position:x", label.Position.X + dn.Velocity.X, _defaultDamageNumberLifetime);
            }
            
            // Fade out
            tween.TweenProperty(label, "modulate:a", 0f, _defaultDamageNumberLifetime);
            
            // Remove on complete
            tween.TweenCallback(() => {
                if (IsInstanceValid(label)) {
                    label.QueueFree();
                }
            });
            
            // Scale animation (bounce for critical)
            if (dn.Type == DamageNumberType.Critical) {
                var scaleTween = _vfxSystem.CreateTween();
                scaleTween.TweenProperty(label, "scale", new Vector2(1.5f, 1.5f), 0.1f);
                scaleTween.TweenProperty(label, "scale", new Vector2(1f, 1f), 0.2f);
            }
        }
        
        /// <summary>
        /// Play VFX with animation
        /// </summary>
        public void PlayVFX(VFXInstance vfx, Node scene)
        {
            var meshInstance = _factory.CreateVFXVisual(vfx);
            if (meshInstance == null) return;
            
            scene.AddChild(meshInstance);
            
            // Create animation
            var tween = _vfxSystem.CreateTween();
            tween.SetParallel(true);
            
            // Scale up
            tween.TweenProperty(meshInstance, "scale", new Vector3(1.5f, 1.5f, 1.5f) * vfx.Scale, vfx.LifeTime * 0.3f);
            
            // Scale down and disappear
            tween.TweenProperty(meshInstance, "scale", Vector3.Zero, vfx.LifeTime * 0.7f).SetDelay(vfx.LifeTime * 0.3f);
            
            // Fade out
            if (meshInstance.MaterialOverride is StandardMaterial3D mat) {
                tween.TweenProperty(mat, "albedo_color:a", 0f, vfx.LifeTime * 0.7f).SetDelay(vfx.LifeTime * 0.3f);
            }
            
            // Remove on complete
            tween.TweenCallback(() => {
                if (IsInstanceValid(meshInstance)) {
                    meshInstance.QueueFree();
                }
            });
        }
        
        /// <summary>
        /// Play screen effect with animation
        /// </summary>
        public void PlayScreenEffect(ScreenEffect effect, Control container)
        {
            var colorRect = _factory.CreateScreenEffectOverlay(effect);
            container.AddChild(colorRect);
            
            // Animation
            var tween = _vfxSystem.CreateTween();
            tween.TweenProperty(colorRect, "color:a", 0f, effect.Duration);
            tween.TweenCallback(() => {
                if (IsInstanceValid(colorRect)) {
                    colorRect.QueueFree();
                }
            });
            
            // Screen shake
            if (effect.Type == ScreenEffectType.Shake) {
                PlayScreenShake(effect.Intensity);
            }
            
            // Slow motion
            if (effect.Type == ScreenEffectType.SlowMo) {
                PlaySlowMotion(effect.Intensity, effect.Duration);
            }
        }
        
        /// <summary>
        /// Play combo effect with animation
        /// </summary>
        public void PlayComboEffect(ComboEffect effect, Control container)
        {
            var label = _factory.CreateComboUI(effect);
            container.AddChild(label);
            
            // Animation
            var tween = _vfxSystem.CreateTween();
            tween.SetParallel(true);
            
            // Scale in with bounce
            label.Scale = new Vector2(0.5f, 0.5f);
            tween.TweenProperty(label, "scale", new Vector2(1.2f, 1.2f), 0.2f);
            tween.TweenProperty(label, "scale", new Vector2(1f, 1f), 0.1f);
            
            // Float up
            tween.TweenProperty(label, "position:y", label.Position.y - 50f, effect.LifeTime);
            
            // Fade out
            tween.TweenProperty(label, "modulate:a", 0f, effect.LifeTime);
            
            tween.TweenCallback(() => {
                if (IsInstanceValid(label)) {
                    label.QueueFree();
                }
            });
        }
        
        /// <summary>
        /// Play critical glow effect with animation
        /// </summary>
        public void PlayCriticalGlow(CriticalGlow glow)
        {
            var meshInstance = _factory.CreateCriticalGlowVisual(glow);
            if (meshInstance == null) return;
            
            var tempParent = glow.Target.GetParent();
            if (tempParent != null) {
                tempParent.AddChild(meshInstance);
            }
            
            // Animation
            var tween = _vfxSystem.CreateTween();
            tween.SetParallel(true);
            
            // Pulse effect
            tween.TweenProperty(meshInstance, "scale", new Vector3(2f, 2f, 2f), glow.Duration * 0.5f);
            tween.TweenProperty(meshInstance, "scale", new Vector3(1.5f, 1.5f, 1.5f), glow.Duration * 0.5f).SetDelay(glow.Duration * 0.5f);
            
            // Fade out
            if (meshInstance.MaterialOverride is StandardMaterial3D mat) {
                tween.TweenProperty(mat, "albedo_color:a", 0f, glow.Duration);
            }
            
            // Follow target
            tween.TweenCallback(() => {
                if (IsInstanceValid(meshInstance)) {
                    meshInstance.QueueFree();
                }
            });
            
            // Start following
            _ = FollowTargetAsync(glow.Target, meshInstance, glow.Duration);
        }
        
        /// <summary>
        /// Screen shake effect
        /// </summary>
        private void PlayScreenShake(float intensity)
        {
            var camera = _vfxSystem?.GetMainCamera();
            if (camera == null) return;
            
            var shakeTween = _vfxSystem.CreateTween();
            Vector3 originalPos = camera.Position;
            
            for (int i = 0; i < 5; i++) {
                shakeTween.TweenProperty(camera, "position", 
                    originalPos + new Vector3(
                        GD.Randf() * intensity - intensity / 2,
                        GD.Randf() * intensity - intensity / 2,
                        0
                    ), 0.04f);
            }
            shakeTween.TweenProperty(camera, "position", originalPos, 0.04f);
        }
        
        /// <summary>
        /// Slow motion effect
        /// </summary>
        private void PlaySlowMotion(float intensity, float duration)
        {
            Engine.TimeScale = intensity;
            _vfxSystem.GetTree().CreateTimer(duration).Timeout += () => {
                Engine.TimeScale = 1f;
            };
        }
        
        /// <summary>
        /// Follow target async
        /// </summary>
        private async System.Threading.Tasks.Task FollowTargetAsync(Node3D target, Node3D follower, float duration)
        {
            float elapsed = 0;
            while (elapsed < duration && IsInstanceValid(target) && IsInstanceValid(follower)) {
                follower.Position = target.Position;
                elapsed += 0.016f;
                await System.Threading.Tasks.Task.Delay(16);
            }
        }
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            // No persistent data needed
        }
    }
}
