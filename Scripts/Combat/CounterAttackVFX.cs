using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// Counter attack visual effects system - provides visual feedback for counter attacks
    /// </summary>
    public partial class CounterAttackVFX : Node
    {
        public static CounterAttackVFX Instance { get; private set; }
        
        // VFX types
        public enum CounterVFXType
        {
            Slash,          // 刀光斩击
            ShieldImpact,   // 盾牌冲击
            BladeStorm,     // 刀刃风暴
            EnergyShield,   // 能量护盾
            BloodBurst,     // 鲜血爆发
            MagicBurst      // 魔法爆发
        }
        
        // VFX data
        public class CounterVFXData
        {
            public string Name { get; set; }
            public Color PrimaryColor { get; set; }
            public Color SecondaryColor { get; set; }
            public float Duration { get; set; }
            public float Scale { get; set; }
            public int ParticleCount { get; set; }
            public bool UseGlow { get; set; }
            public float RotationSpeed { get; set; }
        }
        
        // Active VFX instances
        private class ActiveVFX
        {
            public Node2D RootNode { get; set; }
            public CounterVFXType Type { get; set; }
            public float CurrentTime { get; set; }
            public float Duration { get; set; }
            public Vector2 Position { get; set; }
            public float Rotation { get; set; }
            public bool IsAttacker { get; set; } // true = player, false = enemy
        }
        
        private Dictionary<CounterVFXType, CounterVFXData> _vfxDatabase;
        private List<ActiveVFX> _activeVFX;
        private int _maxConcurrentVFX = 30;
        
        public override void _Ready()
        {
            Instance = this;
            _activeVFX = new List<ActiveVFX>();
            _InitializeVFXDatabase();
        }
        
        public override void _Process(double delta)
        {
            float deltaF = (float)delta;
            _UpdateVFX(deltaF);
        }
        
        private void _InitializeVFXDatabase()
        {
            _vfxDatabase = new Dictionary<CounterVFXType, CounterVFXData>
            {
                { CounterVFXType.Slash, new CounterVFXData
                    {
                        Name = "刀光斩击",
                        PrimaryColor = new Color(1f, 0.9f, 0.7f, 0.9f),
                        SecondaryColor = new Color(1f, 0.5f, 0.2f, 0.6f),
                        Duration = 0.6f,
                        Scale = 1.5f,
                        ParticleCount = 15,
                        UseGlow = true,
                        RotationSpeed = 2f
                    }
                },
                { CounterVFXType.ShieldImpact, new CounterVFXData
                    {
                        Name = "盾牌冲击",
                        PrimaryColor = new Color(0.6f, 0.7f, 1f, 0.8f),
                        SecondaryColor = new Color(0.3f, 0.5f, 1f, 0.5f),
                        Duration = 0.5f,
                        Scale = 2f,
                        ParticleCount = 20,
                        UseGlow = true,
                        RotationSpeed = 0f
                    }
                },
                { CounterVFXType.BladeStorm, new CounterVFXData
                    {
                        Name = "刀刃风暴",
                        PrimaryColor = new Color(0.9f, 0.9f, 1f, 0.9f),
                        SecondaryColor = new Color(0.7f, 0.8f, 1f, 0.6f),
                        Duration = 1f,
                        Scale = 2.5f,
                        ParticleCount = 30,
                        UseGlow = true,
                        RotationSpeed = 5f
                    }
                },
                { CounterVFXType.EnergyShield, new CounterVFXData
                    {
                        Name = "能量护盾",
                        PrimaryColor = new Color(0.3f, 0.8f, 1f, 0.7f),
                        SecondaryColor = new Color(0.5f, 0.9f, 1f, 0.4f),
                        Duration = 0.8f,
                        Scale = 1.8f,
                        ParticleCount = 25,
                        UseGlow = true,
                        RotationSpeed = 1f
                    }
                },
                { CounterVFXType.BloodBurst, new CounterVFXData
                    {
                        Name = "鲜血爆发",
                        PrimaryColor = new Color(0.8f, 0.1f, 0.1f, 0.9f),
                        SecondaryColor = new Color(0.5f, 0f, 0f, 0.6f),
                        Duration = 0.7f,
                        Scale = 1.6f,
                        ParticleCount = 20,
                        UseGlow = false,
                        RotationSpeed = 3f
                    }
                },
                { CounterVFXType.MagicBurst, new CounterVFXData
                    {
                        Name = "魔法爆发",
                        PrimaryColor = new Color(0.7f, 0.3f, 1f, 0.9f),
                        SecondaryColor = new Color(0.5f, 0.2f, 0.8f, 0.6f),
                        Duration = 0.8f,
                        Scale = 2f,
                        ParticleCount = 25,
                        UseGlow = true,
                        RotationSpeed = 2f
                    }
                }
            };
        }
        
        /// <summary>
        /// Trigger counter attack VFX
        /// </summary>
        public void TriggerCounterVFX(CounterVFXType vfxType, Vector2 position, bool isAttacker = true, float rotation = 0f)
        {
            if (!_vfxDatabase.ContainsKey(vfxType))
            {
                GD.Print($"[CounterAttackVFX] Unknown VFX type: {vfxType}");
                return;
            }
            
            // Clean up old VFX if too many
            while (_activeVFX.Count >= _maxConcurrentVFX)
            {
                var oldest = _activeVFX[0];
                if (oldest.RootNode != null && oldest.RootNode.IsInsideTree())
                {
                    oldest.RootNode.QueueFree();
                }
                _activeVFX.RemoveAt(0);
            }
            
            var vfxData = _vfxDatabase[vfxType];
            
            // Create root node
            var rootNode = new Node2D();
            rootNode.Position = position;
            rootNode.Rotation = rotation;
            GetTree().CurrentScene.AddChild(rootNode);
            
            var vfx = new ActiveVFX
            {
                RootNode = rootNode,
                Type = vfxType,
                CurrentTime = 0f,
                Duration = vfxData.Duration,
                Position = position,
                Rotation = rotation,
                IsAttacker = isAttacker
            };
            
            // Create visual elements
            _CreateVFXVisuals(vfx, vfxData);
            
            _activeVFX.Add(vfx);
            
            // Schedule cleanup
            var timer = GetTree().CreateTimer(vfxData.Duration);
            timer.timeout += () =>
            {
                if (vfx.RootNode != null && vfx.RootNode.IsInsideTree())
                {
                    vfx.RootNode.QueueFree();
                }
                _activeVFX.Remove(vfx);
            };
        }
        
        /// <summary>
        /// Create VFX visual elements
        /// </summary>
        private void _CreateVFXVisuals(ActiveVFX vfx, CounterVFXData data)
        {
            var root = vfx.RootNode;
            
            // Create main effect node
            var effectNode = new Node2D();
            effectNode.SetMeta("is_counter_vfx", true);
            effectNode.SetMeta("vfx_type", vfx.Type);
            effectNode.SetMeta("duration", data.Duration);
            effectNode.SetMeta("color", data.PrimaryColor);
            effectNode.SetMeta("secondary_color", data.SecondaryColor);
            effectNode.SetMeta("particle_count", data.ParticleCount);
            effectNode.SetMeta("use_glow", data.UseGlow);
            effectNode.SetMeta("rotation_speed", data.RotationSpeed);
            root.AddChild(effectNode);
            
            // Create particles
            for (int i = 0; i < data.ParticleCount; i++)
            {
                var particle = new Node2D();
                particle.SetMeta("is_counter_particle", true);
                
                float angle = (float)GD.RandRange(0, Mathf.Tau);
                float dist = (float)GD.RandRange(10, data.Scale * 50);
                particle.Position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
                
                particle.SetMeta("particle_type", vfx.Type);
                particle.SetMeta("base_position", particle.Position);
                particle.SetMeta("angle", angle);
                particle.SetMeta("speed", (float)GD.RandRange(0.5f, 2f));
                
                root.AddChild(particle);
            }
            
            // Create glow ring for certain VFX types
            if (data.UseGlow)
            {
                var glowRing = new Node2D();
                glowRing.SetMeta("is_glow_ring", true);
                glowRing.SetMeta("vfx_type", vfx.Type);
                glowRing.SetMeta("max_scale", data.Scale);
                root.AddChild(glowRing);
            }
        }
        
        /// <summary>
        /// Update all active VFX
        /// </summary>
        private void _UpdateVFX(float delta)
        {
            foreach (var vfx in _activeVFX)
            {
                if (vfx.RootNode == null || !vfx.RootNode.IsInsideTree()) continue;
                
                vfx.CurrentTime += delta;
                
                float progress = vfx.CurrentTime / vfx.Duration;
                var data = _vfxDatabase[vfx.Type];
                
                // Update rotation
                if (data.RotationSpeed > 0)
                {
                    vfx.RootNode.Rotation += data.RotationSpeed * delta;
                }
                
                // Update scale (grow then shrink)
                float scale = Mathf.Sin(progress * Mathf.Pi) * data.Scale;
                vfx.RootNode.Scale = new Vector2(scale, scale);
                
                // Update all children
                foreach (Node child in vfx.RootNode.GetChildren())
                {
                    if (child is Node2D node)
                    {
                        // Update particles
                        if (node.HasMeta("is_counter_particle"))
                        {
                            float angle = (float)node.GetMeta("angle");
                            float speed = (float)node.GetMeta("speed");
                            float dist = ((Vector2)node.GetMeta("base_position")).Length();
                            
                            // Particles expand outward
                            float expandFactor = 1f + progress * 0.5f;
                            node.Position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist * expandFactor;
                            
                            // Fade out
                            float alpha = 1f - progress;
                            node.Modulate = new Color(1f, 1f, 1f, alpha);
                        }
                        
                        // Update glow ring
                        if (node.HasMeta("is_glow_ring"))
                        {
                            float maxScale = (float)node.GetMeta("max_scale");
                            float ringScale = progress * maxScale;
                            node.Scale = new Vector2(ringScale, ringScale);
                            
                            float alpha = 1f - progress * 0.5f;
                            node.Modulate = new Color(1f, 1f, 1f, alpha);
                        }
                    }
                }
                
                // Trigger redraw
                vfx.RootNode.QueueRedraw();
            }
        }
        
        /// <summary>
        /// Map counter attack type to VFX type
        /// </summary>
        public CounterVFXType GetVFXForCounterType(Systems.CounterAttackSystem.CounterType counterType)
        {
            switch (counterType)
            {
                case Systems.CounterAttackSystem.CounterType.Riposte:
                    return CounterVFXType.Slash;
                case Systems.CounterAttackSystem.CounterType.ShieldBash:
                    return CounterVFXType.ShieldImpact;
                case Systems.CounterAttackSystem.CounterType.BladeDance:
                    return CounterVFXType.BladeStorm;
                case Systems.CounterAttackSystem.CounterType.IronWill:
                    return CounterVFXType.EnergyShield;
                case Systems.CounterAttackSystem.CounterType.BloodRevenge:
                    return CounterVFXType.BloodBurst;
                case Systems.CounterAttackSystem.CounterType.MagicCounter:
                    return CounterVFXType.MagicBurst;
                default:
                    return CounterVFXType.Slash;
            }
        }
        
        /// <summary>
        /// Clear all active VFX
        /// </summary>
        public void ClearAllVFX()
        {
            foreach (var vfx in _activeVFX)
            {
                if (vfx.RootNode != null && vfx.RootNode.IsInsideTree())
                {
                    vfx.RootNode.QueueFree();
                }
            }
            _activeVFX.Clear();
        }
    }
}
