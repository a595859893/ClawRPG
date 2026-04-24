using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Combat {

    /// <summary>
    /// Node2D-based renderer for drawing ability effects.
    /// Owned by BossAbilityVisualizer (composition pattern).
    /// </summary>
    public partial class EffectRenderer : Node2D
    {
        private BossAbilityVisualizer _parent;

        public void SetParent(BossAbilityVisualizer parent)
        {
            _parent = parent;
        }

        public void _Draw()
        {
            if (_parent == null) return;
            _parent.DrawEffects();
        }
    }
    /// <summary>
    /// Boss ability visual effect types
    /// </summary>
    public enum BossAbilityVisualType
    {
        Circle,          // Circular area effect (e.g., flame breath, poison cloud)
        Rectangle,      // Rectangular area (e.g., ground slam)
        Cone,           // Cone shaped area (e.g., breath attacks)
        Line,           // Linear area (e.g., lightning chain)
        Target,         // Single target effect
        Self            // Self-centered effect (e.g., self-heal)
    }
    
    /// <summary>
    /// Visual effect style for boss abilities
    /// </summary>
    public enum BossAbilityEffectStyle
    {
        Warning,        // Warning indicator before damage
        Instant,        // Instant damage effect
        Persistent,     // Persistent damage zone
        Pulse,          // Pulsing area effect
        Follow          // Following the target
    }
    
    /// <summary>
    /// Boss ability visual data
    /// </summary>
    [System.Serializable]
    public class BossAbilityVisual
    {
        public string AbilityId;
        public BossAbilityVisualType VisualType;
        public BossAbilityEffectStyle EffectStyle;
        public Color WarningColor = new Color(1f, 0f, 0f, 0.3f);
        public Color ActiveColor = new Color(1f, 0.3f, 0f, 0.6f);
        public float WarningDuration = 1.5f;
        public float ActiveDuration = 0.8f;
        public float Radius = 150f;
        public Vector2 Size = new Vector2(200f, 100f);
        public float ConeAngle = 60f;
        public int Segments = 32;
        public bool ShowParticles = true;
        public string ParticleType = "fire";
        
        public BossAbilityVisual(string abilityId, BossAbilityVisualType visualType, BossAbilityEffectStyle effectStyle)
        {
            AbilityId = abilityId;
            VisualType = visualType;
            EffectStyle = effectStyle;
        }
    }
    
    /// <summary>
    /// Manages visual effects for boss abilities
    /// </summary>
    public partial class BossAbilityVisualizer : BaseSystem
    {
        private static BossAbilityVisualizer _instance;
        public static BossAbilityVisualizer Instance => _instance;
        
        // Active visual effects
        private Dictionary<string, BossAbilityVisual> _visualDatabase;
        private List<AbilityEffectInstance> _activeEffects;
        
        // Drawing
        private Dictionary<string, List<Vector2>> _particlePositions;
        
        // Particles
        private Random _random = new Random();
        
        // Effect renderer (composition: Node2D child for drawing)
        private EffectRenderer _renderer;
        
        protected override void Initialize()
        {
            base.Initialize();
            _instance = this;
            _visualDatabase = new Dictionary<string, BossAbilityVisual>();
            _activeEffects = new List<AbilityEffectInstance>();
            _particlePositions = new Dictionary<string, List<Vector2>>();
            
            InitializeVisualDatabase();
            
            // Create EffectRenderer child for drawing
            _renderer = new EffectRenderer();
            _renderer.SetParent(this);
            AddChild(_renderer);
        }
        
        public override void _Process(double delta)
        {
            UpdateEffects(delta);
            if (_renderer != null)
            {
                _renderer.QueueRedraw();
            }
        }
        
        private void InitializeVisualDatabase()
        {
            // Fire abilities
            AddVisual("fire_breath", new BossAbilityVisual("fire_breath", BossAbilityVisualType.Cone, BossAbilityEffectStyle.Warning)
            {
                WarningColor = new Color(1f, 0.3f, 0f, 0.25f),
                ActiveColor = new Color(1f, 0.5f, 0f, 0.7f),
                ConeAngle = 45f,
                Radius = 200f,
                WarningDuration = 1.2f,
                ActiveDuration = 1.0f,
                ParticleType = "fire"
            });
            
            AddVisual("flame_breath", new BossAbilityVisual("flame_breath", BossAbilityVisualType.Cone, BossAbilityEffectStyle.Warning)
            {
                WarningColor = new Color(1f, 0.2f, 0f, 0.25f),
                ActiveColor = new Color(1f, 0.4f, 0f, 0.7f),
                ConeAngle = 50f,
                Radius = 180f,
                WarningDuration = 1.0f,
                ActiveDuration = 0.8f,
                ParticleType = "fire"
            });
            
            // Lightning abilities
            AddVisual("lightning_chain", new BossAbilityVisual("lightning_chain", BossAbilityVisualType.Line, BossAbilityEffectStyle.Instant)
            {
                WarningColor = new Color(0.5f, 0.5f, 1f, 0.3f),
                ActiveColor = new Color(0.7f, 0.7f, 1f, 0.8f),
                Radius = 250f,
                WarningDuration = 0.5f,
                ActiveDuration = 0.3f,
                ParticleType = "lightning"
            });
            
            // Poison abilities
            AddVisual("poison_cloud", new BossAbilityVisual("poison_cloud", BossAbilityVisualType.Circle, BossAbilityEffectStyle.Persistent)
            {
                WarningColor = new Color(0.2f, 0.8f, 0.2f, 0.2f),
                ActiveColor = new Color(0.3f, 0.6f, 0.3f, 0.5f),
                Radius = 120f,
                WarningDuration = 1.0f,
                ActiveDuration = 3.0f,
                ShowParticles = true,
                ParticleType = "poison"
            });
            
            AddVisual("toxic_gas", new BossAbilityVisual("toxic_gas", BossAbilityVisualType.Circle, BossAbilityEffectStyle.Persistent)
            {
                WarningColor = new Color(0.3f, 0.6f, 0.2f, 0.2f),
                ActiveColor = new Color(0.4f, 0.8f, 0.3f, 0.5f),
                Radius = 150f,
                WarningDuration = 1.5f,
                ActiveDuration = 4.0f,
                ShowParticles = true,
                ParticleType = "poison"
            });
            
            // Ice abilities
            AddVisual("ice_lance", new BossAbilityVisual("ice_lance", BossAbilityVisualType.Cone, BossAbilityEffectStyle.Warning)
            {
                WarningColor = new Color(0.5f, 0.8f, 1f, 0.25f),
                ActiveColor = new Color(0.7f, 0.9f, 1f, 0.7f),
                ConeAngle = 30f,
                Radius = 220f,
                WarningDuration = 0.8f,
                ActiveDuration = 0.5f,
                ParticleType = "ice"
            });
            
            // Shadow abilities
            AddVisual("shadow_bolt", new BossAbilityVisual("shadow_bolt", BossAbilityVisualType.Target, BossAbilityEffectStyle.Instant)
            {
                WarningColor = new Color(0.3f, 0f, 0.5f, 0.4f),
                ActiveColor = new Color(0.5f, 0f, 0.8f, 0.8f),
                WarningDuration = 0.6f,
                ActiveDuration = 0.3f,
                ParticleType = "shadow"
            });
            
            AddVisual("shadow_burst", new BossAbilityVisual("shadow_burst", BossAbilityVisualType.Circle, BossAbilityEffectStyle.Warning)
            {
                WarningColor = new Color(0.2f, 0f, 0.4f, 0.25f),
                ActiveColor = new Color(0.4f, 0f, 0.6f, 0.6f),
                Radius = 100f,
                WarningDuration = 1.0f,
                ActiveDuration = 0.6f,
                ParticleType = "shadow"
            });
            
            // Ground slam
            AddVisual("ground_slam", new BossAbilityVisual("ground_slam", BossAbilityVisualType.Circle, BossAbilityEffectStyle.Warning)
            {
                WarningColor = new Color(0.6f, 0.4f, 0.2f, 0.25f),
                ActiveColor = new Color(0.8f, 0.5f, 0.2f, 0.7f),
                Radius = 130f,
                WarningDuration = 1.2f,
                ActiveDuration = 0.5f,
                ShowParticles = true,
                ParticleType = "earth"
            });
            
            // Fear roar
            AddVisual("fear_roar", new BossAbilityVisual("fear_roar", BossAbilityVisualType.Circle, BossAbilityEffectStyle.Pulse)
            {
                WarningColor = new Color(0.5f, 0f, 0f, 0.2f),
                ActiveColor = new Color(0.8f, 0f, 0f, 0.5f),
                Radius = 180f,
                WarningDuration = 0.8f,
                ActiveDuration = 1.5f,
                Segments = 64,
                ShowParticles = true,
                ParticleType = "fear"
            });
            
            // Blood ripple
            AddVisual("blood_ripple", new BossAbilityVisual("blood_ripple", BossAbilityVisualType.Circle, BossAbilityEffectStyle.Pulse)
            {
                WarningColor = new Color(0.6f, 0f, 0f, 0.2f),
                ActiveColor = new Color(0.8f, 0.1f, 0.1f, 0.6f),
                Radius = 140f,
                WarningDuration = 0.5f,
                ActiveDuration = 2.0f,
                ShowParticles = true,
                ParticleType = "blood"
            });
            
            // Arcane missiles
            AddVisual("arcane_missile", new BossAbilityVisual("arcane_missile", BossAbilityVisualType.Target, BossAbilityEffectStyle.Persistent)
            {
                WarningColor = new Color(0.6f, 0.3f, 1f, 0.3f),
                ActiveColor = new Color(0.8f, 0.5f, 1f, 0.7f),
                WarningDuration = 1.0f,
                ActiveDuration = 2.0f,
                ShowParticles = true,
                ParticleType = "arcane"
            });
            
            // Self heal visual
            AddVisual("self_heal", new BossAbilityVisual("self_heal", BossAbilityVisualType.Self, BossAbilityEffectStyle.Pulse)
            {
                WarningColor = new Color(0.2f, 0.8f, 0.4f, 0.2f),
                ActiveColor = new Color(0.4f, 1f, 0.6f, 0.6f),
                Radius = 80f,
                WarningDuration = 0.5f,
                ActiveDuration = 1.5f,
                ShowParticles = true,
                ParticleType = "heal"
            });
            
            // Teleport/Blink
            AddVisual("teleport", new BossAbilityVisual("teleport", BossAbilityVisualType.Self, BossAbilityEffectStyle.Instant)
            {
                WarningColor = new Color(0.5f, 0.5f, 1f, 0.3f),
                ActiveColor = new Color(0.8f, 0.8f, 1f, 0.8f),
                Radius = 50f,
                WarningDuration = 0.3f,
                ActiveDuration = 0.2f,
                ParticleType = "arcane"
            });
            
            // Summon minions
            AddVisual("summon_minions", new BossAbilityVisual("summon_minions", BossAbilityVisualType.Circle, BossAbilityEffectStyle.Warning)
            {
                WarningColor = new Color(0.4f, 0f, 0.6f, 0.2f),
                ActiveColor = new Color(0.6f, 0.2f, 0.8f, 0.6f),
                Radius = 160f,
                WarningDuration = 1.5f,
                ActiveDuration = 0.8f,
                ShowParticles = true,
                ParticleType = "shadow"
            });
        }
        
        private void AddVisual(string abilityId, BossAbilityVisual visual)
        {
            _visualDatabase[abilityId] = visual;
        }
        
        /// <summary>
        /// Trigger a visual effect for a boss ability
        /// </summary>
        public void TriggerAbilityVisual(string abilityId, Vector2 bossPosition, Vector2 targetPosition, float bossFacingAngle = 0)
        {
            if (!_visualDatabase.TryGetValue(abilityId, out var visual))
            {
                // Create default visual if not found
                visual = new BossAbilityVisual(abilityId, BossAbilityVisualType.Circle, BossAbilityEffectStyle.Warning);
                _visualDatabase[abilityId] = visual;
            }
            
            var instance = new AbilityEffectInstance
            {
                Visual = visual,
                BossPosition = bossPosition,
                TargetPosition = targetPosition,
                BossFacingAngle = bossFacingAngle,
                TimeElapsed = 0f,
                Phase = EffectPhase.Warning
            };
            
            _activeEffects.Add(instance);
            
            // Initialize particles for this effect
            if (visual.ShowParticles)
            {
                InitializeParticles(instance);
            }
        }
        
        private void InitializeParticles(AbilityEffectInstance instance)
        {
            var key = instance.GetHashCode().ToString();
            _particlePositions[key] = new List<Vector2>();
            
            int particleCount = (int)(instance.Visual.Radius / 10f);
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 offset = GetRandomOffsetInArea(instance.Visual);
                _particlePositions[key].Add(offset);
            }
        }
        
        private Vector2 GetRandomOffsetInArea(BossAbilityVisual visual)
        {
            switch (visual.VisualType)
            {
                case BossAbilityVisualType.Circle:
                    float angle = (float)(_random.NextDouble() * Math.PI * 2);
                    float dist = (float)(_random.NextDouble() * visual.Radius);
                    return new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
                    
                case BossAbilityVisualType.Cone:
                    float coneAngle = (float)((_random.NextDouble() - 0.5) * visual.ConeAngle * Mathf.DegToRad);
                    float coneDist = (float)(_random.NextDouble() * visual.Radius);
                    Vector2 dir = new Vector2(1f, 0f).Rotated(coneAngle);
                    return dir * coneDist;
                    
                case BossAbilityVisualType.Rectangle:
                    return new Vector2(
                        (float)((_random.NextDouble() - 0.5) * visual.Size.X),
                        (float)((_random.NextDouble() - 0.5) * visual.Size.Y)
                    );
                    
                default:
                    return Vector2.Zero;
            }
        }
        
        private void UpdateEffects(float delta)
        {
            List<AbilityEffectInstance> toRemove = new List<AbilityEffectInstance>();
            
            foreach (var effect in _activeEffects)
            {
                effect.TimeElapsed += delta;
                
                // Phase transition
                if (effect.Phase == EffectPhase.Warning && effect.TimeElapsed >= effect.Visual.WarningDuration)
                {
                    effect.Phase = EffectPhase.Active;
                    effect.TimeElapsed = 0f;
                }
                else if (effect.Phase == EffectPhase.Active && effect.TimeElapsed >= effect.Visual.ActiveDuration)
                {
                    toRemove.Add(effect);
                }
                
                // Update particles
                if (effect.Visual.ShowParticles)
                {
                    UpdateParticles(effect, delta);
                }
            }
            
            foreach (var effect in toRemove)
            {
                _activeEffects.Remove(effect);
                var key = effect.GetHashCode().ToString();
                _particlePositions.Remove(key);
            }
        }
        
        private void UpdateParticles(AbilityEffectInstance effect, float delta)
        {
            var key = effect.GetHashCode().ToString();
            if (!_particlePositions.TryGetValue(key, out var positions))
                return;
                
            float moveSpeed = 20f * delta;
            for (int i = 0; i < positions.Count; i++)
            {
                Vector2 offset = positions[i];
                
                // Slight random movement
                offset.X += (float)((_random.NextDouble() - 0.5) * moveSpeed);
                offset.Y += (float)((_random.NextDouble() - 0.5) * moveSpeed);
                
                // Keep within bounds
                float maxDist = effect.Visual.Radius;
                if (offset.Length() > maxDist)
                {
                    offset = offset.Normalized() * maxDist;
                }
                
                positions[i] = offset;
            }
        }
        
        public void _Draw()
        {
            // Drawing is handled by EffectRenderer child
        }
        
        /// <summary>
        /// Called by EffectRenderer to draw all active effects.
        /// </summary>
        public void DrawEffects()
        {
            foreach (var effect in _activeEffects)
            {
                DrawAbilityEffect(effect);
            }
        }
        
        private void DrawAbilityEffect(AbilityEffectInstance effect)
        {
            Vector2 drawPos = effect.BossPosition;
            Color color = effect.Phase == EffectPhase.Warning ? 
                effect.Visual.WarningColor : effect.Visual.ActiveColor;
            
            // Pulse effect for active phase
            if (effect.Phase == EffectPhase.Active && effect.Visual.EffectStyle == BossAbilityEffectStyle.Pulse)
            {
                float pulse = Mathf.Sin(effect.TimeElapsed * 8f) * 0.3f + 0.7f;
                color.A *= pulse;
            }
            
            // Animate warning to active transition
            if (effect.Phase == EffectPhase.Active)
            {
                float transition = effect.TimeElapsed / effect.Visual.ActiveDuration;
                color.A = Mathf.Lerp(effect.Visual.ActiveColor.A, color.A * 0.3f, transition);
            }
            
            switch (effect.Visual.VisualType)
            {
                case BossAbilityVisualType.Circle:
                    DrawCircleEffect(effect, color);
                    break;
                    
                case BossAbilityVisualType.Cone:
                    DrawConeEffect(effect, color);
                    break;
                    
                case BossAbilityVisualType.Rectangle:
                    DrawRectangleEffect(effect, color);
                    break;
                    
                case BossAbilityVisualType.Line:
                    DrawLineEffect(effect, color);
                    break;
                    
                case BossAbilityVisualType.Target:
                    DrawTargetEffect(effect, color);
                    break;
                    
                case BossAbilityVisualType.Self:
                    DrawSelfEffect(effect, color);
                    break;
            }
            
            // Draw particles
            if (effect.Visual.ShowParticles && effect.Phase == EffectPhase.Active)
            {
                DrawParticles(effect, color);
            }
        }
        
        private void DrawCircleEffect(AbilityEffectInstance effect, Color color)
        {
            // Draw outer circle
            DrawArc(effect.TargetPosition, effect.Visual.Radius, 0, Mathf.Tau, 
                effect.Visual.Segments, color, 2f);
            
            // Draw inner fill (warning phase only)
            if (effect.Phase == EffectPhase.Warning)
            {
                Color fillColor = color;
                fillColor.A *= 0.5f;
                DrawCircle(effect.TargetPosition, effect.Visual.Radius * 0.9f, fillColor);
            }
        }
        
        private void DrawConeEffect(AbilityEffectInstance effect, Color color)
        {
            float angle = effect.BossFacingAngle;
            float halfAngle = effect.Visual.ConeAngle * Mathf.DegToRad / 2f;
            float radius = effect.Visual.Radius;
            
            Vector2 startDir = new Vector2(Mathf.Cos(angle - halfAngle), Mathf.Sin(angle - halfAngle));
            Vector2 endDir = new Vector2(Mathf.Cos(angle + halfAngle), Mathf.Sin(angle + halfAngle));
            
            Vector2 start = effect.BossPosition + startDir * radius;
            Vector2 end = effect.BossPosition + endDir * radius;
            
            // Draw arc
            int segments = 16;
            Vector2 prevPoint = effect.BossPosition;
            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                float segAngle = angle - halfAngle + halfAngle * 2 * t;
                Vector2 point = effect.BossPosition + new Vector2(Mathf.Cos(segAngle), Mathf.Sin(segAngle)) * radius;
                DrawLine(prevPoint, point, color, 2f);
                prevPoint = point;
            }
            
            // Draw lines to center
            DrawLine(effect.BossPosition, start, color, 1.5f);
            DrawLine(effect.BossPosition, end, color, 1.5f);
            
            // Fill during warning
            if (effect.Phase == EffectPhase.Warning)
            {
                Color fillColor = color;
                fillColor.A *= 0.3f;
                // Simple triangle approximation
                Vector2[] points = new Vector2[] { effect.BossPosition, start, end };
                // Note: Godot 2D drawing doesn't have FillPolygon, so we use circles
                for (int i = 0; i < 5; i++)
                {
                    float t = (float)i / 4;
                    Vector2 pos = effect.BossPosition + (startDir + (endDir - startDir) * t) * radius * 0.5f;
                    DrawCircle(pos, 30f, fillColor);
                }
            }
        }
        
        private void DrawRectangleEffect(AbilityEffectInstance effect, Color color)
        {
            Vector2 size = effect.Visual.Size;
            Rect2 rect = new Rect2(
                effect.TargetPosition - size / 2,
                size
            );
            
            // Draw rectangle outline
            DrawRect(rect, color, false, 2f);
            
            // Fill during warning
            if (effect.Phase == EffectPhase.Warning)
            {
                Color fillColor = color;
                fillColor.A *= 0.4f;
                DrawRect(rect, fillColor, true);
            }
        }
        
        private void DrawLineEffect(AbilityEffectInstance effect, Color color)
        {
            Vector2 direction = (effect.TargetPosition - effect.BossPosition).Normalized();
            float length = effect.Visual.Radius;
            
            Vector2 endPoint = effect.BossPosition + direction * length;
            
            // Draw main line
            DrawLine(effect.BossPosition, endPoint, color, 3f);
            
            // Draw branches for lightning effect
            if (effect.Visual.ParticleType == "lightning" && effect.Phase == EffectPhase.Active)
            {
                int branches = 3;
                for (int i = 0; i < branches; i++)
                {
                    float branchOffset = (i - branches / 2f) * 20f;
                    Vector2 perpendicular = new Vector2(-direction.Y, direction.X) * branchOffset;
                    Vector2 branchStart = effect.BossPosition + perpendicular * 0.5f;
                    Vector2 branchMid = branchStart + direction * length * 0.6f;
                    Vector2 branchEnd = branchStart + direction * length;
                    
                    DrawLine(branchStart, branchMid, color, 1.5f);
                    DrawLine(branchMid, branchEnd, color, 1f);
                }
            }
        }
        
        private void DrawTargetEffect(AbilityEffectInstance effect, Color color)
        {
            float radius = 40f;
            
            // Draw targeting reticle
            DrawCircle(effect.TargetPosition, radius, color, 2f);
            DrawCircle(effect.TargetPosition, radius * 0.7f, color, 1.5f);
            
            // Draw cross
            float crossSize = radius * 1.3f;
            DrawLine(
                effect.TargetPosition + new Vector2(-crossSize, 0),
                effect.TargetPosition + new Vector2(crossSize, 0),
                color, 1.5f
            );
            DrawLine(
                effect.TargetPosition + new Vector2(0, -crossSize),
                effect.TargetPosition + new Vector2(0, crossSize),
                color, 1.5f
            );
            
            // Warning indicator
            if (effect.Phase == EffectPhase.Warning)
            {
                Color fillColor = color;
                fillColor.A *= 0.3f;
                DrawCircle(effect.TargetPosition, radius * 0.5f, fillColor);
            }
        }
        
        private void DrawSelfEffect(AbilityEffectInstance effect, Color color)
        {
            float radius = effect.Visual.Radius;
            
            // Expanding rings
            if (effect.Phase == EffectPhase.Active)
            {
                float progress = effect.TimeElapsed / effect.Visual.ActiveDuration;
                float expandRadius = radius * (1f - progress);
                float alpha = 1f - progress;
                
                Color ringColor = color;
                ringColor.A *= alpha;
                DrawCircle(effect.BossPosition, expandRadius, ringColor, 2f);
            }
            else
            {
                DrawCircle(effect.BossPosition, radius, color, 2f);
            }
        }
        
        private void DrawParticles(AbilityEffectInstance effect, Color color)
        {
            var key = effect.GetHashCode().ToString();
            if (!_particlePositions.TryGetValue(key, out var positions))
                return;
                
            Color particleColor = color;
            particleColor.A *= 0.6f;
            
            float particleSize = 4f;
            if (effect.Visual.ParticleType == "fire")
                particleColor = new Color(1f, 0.6f, 0.1f, particleColor.A);
            else if (effect.Visual.ParticleType == "ice")
                particleColor = new Color(0.7f, 0.9f, 1f, particleColor.A);
            else if (effect.Visual.ParticleType == "poison")
                particleColor = new Color(0.3f, 0.8f, 0.3f, particleColor.A);
            else if (effect.Visual.ParticleType == "lightning")
                particleColor = new Color(0.8f, 0.8f, 1f, particleColor.A);
            else if (effect.Visual.ParticleType == "shadow")
                particleColor = new Color(0.4f, 0.2f, 0.6f, particleColor.A);
            else if (effect.Visual.ParticleType == "heal")
                particleColor = new Color(0.4f, 1f, 0.6f, particleColor.A);
            else if (effect.Visual.ParticleType == "arcane")
                particleColor = new Color(0.7f, 0.5f, 1f, particleColor.A);
                
            foreach (var offset in positions)
            {
                Vector2 pos = effect.TargetPosition + offset;
                DrawCircle(pos, particleSize, particleColor);
            }
        }
        
        /// <summary>
        /// Clear all active effects
        /// </summary>
        public void ClearAllEffects()
        {
            _activeEffects.Clear();
            _particlePositions.Clear();
        }
        
        public override Dictionary<string, object> ExportSaveData()
        {
            // Visualizer is stateless for saves — no persistent state to export
            return new Dictionary<string, object>();
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            // No persistent state to restore
        }
        
        private enum EffectPhase
        {
            Warning,
            Active
        }
        
        private class AbilityEffectInstance
        {
            public BossAbilityVisual Visual;
            public Vector2 BossPosition;
            public Vector2 TargetPosition;
            public float BossFacingAngle;
            public float TimeElapsed;
            public EffectPhase Phase;
        }
    }
}
