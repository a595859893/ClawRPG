using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// 粒子效果类型
    /// </summary>
    public enum ParticleEffectType {
        None,
        FireExplosion,
        IceShatter,
        LightningStrike,
        HolyLight,
        DarkVoid,
        PoisonCloud,
        Smoke,
        Sparks,
        Blood,
        Heal,
        Buff,
        Debuff,
        Footsteps,
        Leaves,
        Snow,
        Rain,
        Dust,
        WaterSplash,
        CritSparkle,
        LevelUp
    }

    /// <summary>
    /// 粒子效果系统 - 管理游戏中所有粒子特效
    /// </summary>
    public partial class ParticleEffectManager : Node
    {
        public static ParticleEffectManager Instance { get; private set; }

        // 预加载粒子场景
        private PackedScene particleScene;

        // 粒子效果配置
        private Dictionary<ParticleEffectType, ParticleConfig> particleConfigs;

        // 活跃的粒子节点
        private List<Node> activeParticles = new List<Node>();
        private const int MaxActiveParticles = 50;

        public override void _Ready()
        {
            Instance = this;
            InitializeParticleConfigs();
        }

        private void InitializeParticleConfigs()
        {
            particleConfigs = new Dictionary<ParticleEffectType, ParticleConfig>();

            // 火焰爆炸
            particleConfigs[ParticleEffectType.FireExplosion] = new ParticleConfig
            {
                Amount = 30,
                Lifetime = 0.8f,
                Spread = 180f,
                Gravity = Vector3.Down * 200,
                InitialVelocity = 300,
                VelocityRandom = 100,
                Scale = new Curve(),
                Color = new Gradient(
                    new GradientPoint[] {
                        new GradientPoint(Colors.Orange, 0f),
                        new GradientPoint(Colors.Red, 0.3f),
                        new GradientPoint(new Color(0.3f, 0f, 0f), 0.7f),
                        new GradientPoint(Colors.Transparent, 1f)
                    }
                ),
                EmissionShape = EmissionShape.Sphere,
                EmissionRadius = 0.5f
            };

            // 冰霜破碎
            particleConfigs[ParticleEffectType.IceShatter] = new ParticleConfig
            {
                Amount = 20,
                Lifetime = 1.0f,
                Spread = 360f,
                Gravity = Vector3.Down * 300,
                InitialVelocity = 200,
                VelocityRandom = 80,
                Scale = new Curve(),
                Color = new Gradient(
                    new GradientPoint[] {
                        new GradientPoint(Colors.LightBlue, 0f),
                        new GradientPoint(Colors.Cyan, 0.4f),
                        new GradientPoint(Colors.White, 0.7f),
                        new GradientPoint(Colors.Transparent, 1f)
                    }
                ),
                EmissionShape = EmissionShape.Sphere,
                EmissionRadius = 0.3f
            };

            // 闪电打击
            particleConfigs[ParticleEffectType.LightningStrike] = new ParticleConfig
            {
                Amount = 40,
                Lifetime = 0.5f,
                Spread = 30f,
                Gravity = Vector3.Zero,
                InitialVelocity = 0,
                VelocityRandom = 0,
                Scale = new Curve(),
                Color = new Gradient(
                    new GradientPoint[] {
                        new GradientPoint(Colors.Yellow, 0f),
                        new GradientPoint(Colors.LightYellow, 0.2f),
                        new GradientPoint(Colors.White, 0.5f),
                        new GradientPoint(Colors.Transparent, 1f)
                    }
                ),
                EmissionShape = EmissionShape.Point,
                EmissionRadius = 0f
            };

            // 圣光
            particleConfigs[ParticleEffectType.HolyLight] = new ParticleConfig
            {
                Amount = 25,
                Lifetime = 1.2f,
                Spread = 90f,
                Gravity = Vector3.Down * 50,
                InitialVelocity = 100,
                VelocityRandom = 30,
                Scale = new Curve(),
                Color = new Gradient(
                    new GradientPoint[] {
                        new GradientPoint(Colors.White, 0f),
                        new GradientPoint(Colors.LightYellow, 0.3f),
                        new GradientPoint(new Color(1f, 1f, 0.8f), 0.7f),
                        new GradientPoint(Colors.Transparent, 1f)
                    }
                ),
                EmissionShape = EmissionShape.Sphere,
                EmissionRadius = 0.5f
            };

            // 暗影
            particleConfigs[ParticleEffectType.DarkVoid] = new ParticleConfig
            {
                Amount = 30,
                Lifetime = 1.0f,
                Spread = 180f,
                Gravity = Vector3.Down * 20,
                InitialVelocity = 80,
                VelocityRandom = 40,
                Scale = new Curve(),
                Color = new Gradient(
                    new GradientPoint[] {
                        new GradientPoint(new Color(0.3f, 0f, 0.5f), 0f),
                        new GradientPoint(new Color(0.2f, 0f, 0.3f), 0.5f),
                        new GradientPoint(Colors.Black, 0.8f),
                        new GradientPoint(Colors.Transparent, 1f)
                    }
                ),
                EmissionShape = EmissionShape.Sphere,
                EmissionRadius = 0.5f
            };

            // 毒雾
            particleConfigs[ParticleEffectType.PoisonCloud] = new ParticleConfig
            {
                Amount = 35,
                Lifetime = 2.0f,
                Spread = 360f,
                Gravity = Vector3.Up * 30,
                InitialVelocity = 50,
                VelocityRandom = 20,
                Scale = new Curve(),
                Color = new Gradient(
                    new GradientPoint[] {
                        new GradientPoint(new Color(0.2f, 0.8f, 0.2f, 0.8f), 0f),
                        new GradientPoint(new Color(0.1f, 0.5f, 0.1f, 0.6f), 0.5f),
                        new GradientPoint(new Color(0f, 0.3f, 0f, 0.3f), 1f)
                    }
                ),
                EmissionShape = EmissionShape.Sphere,
                EmissionRadius = 1.0f
            };

            // 治疗
            particleConfigs[ParticleEffectType.Heal] = new ParticleConfig
            {
                Amount = 20,
                Lifetime = 1.5f,
                Spread = 45f,
                Gravity = Vector3.Up * 100,
                InitialVelocity = 80,
                VelocityRandom = 20,
                Scale = new Curve(),
                Color = new Gradient(
                    new GradientPoint[] {
                        new GradientPoint(Colors.White, 0f),
                        new GradientPoint(Colors.LightGreen, 0.3f),
                        new GradientPoint(Colors.Green, 0.7f),
                        new GradientPoint(Colors.Transparent, 1f)
                    }
                ),
                EmissionShape = EmissionShape.Sphere,
                EmissionRadius = 0.3f
            };

            // 暴击闪光
            particleConfigs[ParticleEffectType.CritSparkle] = new ParticleConfig
            {
                Amount = 15,
                Lifetime = 0.6f,
                Spread = 180f,
                Gravity = Vector3.Zero,
                InitialVelocity = 150,
                VelocityRandom = 50,
                Scale = new Curve(),
                Color = new Gradient(
                    new GradientPoint[] {
                        new GradientPoint(Colors.Gold, 0f),
                        new GradientPoint(Colors.Yellow, 0.3f),
                        new GradientPoint(Colors.White, 0.6f),
                        new GradientPoint(Colors.Transparent, 1f)
                    }
                ),
                EmissionShape = EmissionShape.Point,
                EmissionRadius = 0f
            };

            // 升级
            particleConfigs[ParticleEffectType.LevelUp] = new ParticleConfig
            {
                Amount = 50,
                Lifetime = 2.0f,
                Spread = 90f,
                Gravity = Vector3.Up * 50,
                InitialVelocity = 100,
                VelocityRandom = 30,
                Scale = new Curve(),
                Color = new Gradient(
                    new GradientPoint[] {
                        new GradientPoint(Colors.Gold, 0f),
                        new GradientPoint(Colors.Yellow, 0.3f),
                        new GradientPoint(new Color(1f, 0.9f, 0.5f), 0.7f),
                        new GradientPoint(Colors.Transparent, 1f)
                    }
                ),
                EmissionShape = EmissionShape.Sphere,
                EmissionRadius = 0.8f
            };
        }

        /// <summary>
        /// 在指定位置播放粒子效果
        /// </summary>
        public void PlayParticleEffect(ParticleEffectType effectType, Vector3 position, float scale = 1.0f)
        {
            if (effectType == ParticleEffectType.None || !particleConfigs.ContainsKey(effectType))
                return;

            // 清理过多粒子
            CleanupOldParticles();

            var config = particleConfigs[effectType];
            CreateParticleNode(effectType, config, position, scale);
        }

        /// <summary>
        /// 在2D位置播放粒子效果
        /// </summary>
        public void PlayParticleEffect2D(ParticleEffectType effectType, Vector2 position, float scale = 1.0f)
        {
            PlayParticleEffect(effectType, new Vector3(position.x, position.y, 0), scale);
        }

        /// <summary>
        /// 在节点位置播放粒子效果
        /// </summary>
        public void PlayParticleEffectAtNode(ParticleEffectType effectType, Node3D node, float scale = 1.0f)
        {
            if (node != null)
            {
                PlayParticleEffect(effectType, node.GlobalPosition, scale);
            }
        }

        /// <summary>
        /// 在2D节点位置播放粒子效果
        /// </summary>
        public void PlayParticleEffectAtNode2D(ParticleEffectType effectType, Node2D node, float scale = 1.0f)
        {
            if (node != null)
            {
                PlayParticleEffect2D(effectType, node.GlobalPosition, scale);
            }
        }

        private void CreateParticleNode(ParticleEffectType effectType, ParticleConfig config, Vector3 position, float scale)
        {
            // 使用GPUParticles3D创建3D粒子
            var particles = new GPUParticles3D();
            
            // 设置粒子数量
            particles.Amount = config.Amount;
            particles.Lifetime = config.Lifetime;
            
            // 创建材质
            var material = new ParticleProcessMaterial();
            material.Spread = config.Spread;
            material.Gravity = config.Gravity;
            material.InitialVelocityMin = config.InitialVelocity - config.VelocityRandom;
            material.InitialVelocityMax = config.InitialVelocity + config.VelocityRandom;
            material.ScaleMin = scale * 0.5f;
            material.ScaleMax = scale * 1.5f;
            
            // 设置颜色渐变
            if (config.Color != null)
            {
                material.Color = config.Color;
            }
            
            // 设置发射形状
            switch (config.EmissionShape)
            {
                case EmissionShape.Sphere:
                    var sphereShape = new SphereShape3D();
                    sphereShape.Radius = config.EmissionRadius;
                    material.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Sphere;
                    material.EmissionSphereRadius = config.EmissionRadius;
                    break;
                case EmissionShape.Point:
                    material.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Point;
                    break;
                case EmissionShape.Box:
                    material.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box;
                    material.EmissionBoxExtents = new Vector3(config.EmissionRadius, config.EmissionRadius, config.EmissionRadius);
                    break;
            }
            
            particles.ProcessMaterial = material;
            
            // 设置位置
            particles.Position = position;
            
            // 设置一次性发射
            particles.OneShot = true;
            particles.Explosiveness = 1.0f;
            
            // 添加到场景
            GetTree().CurrentScene.AddChild(particles);
            activeParticles.Add(particles);
            
            // 播放并自动清理
            particles.Emitting = true;
            
            // 延迟删除
            var timer = GetTree().CreateTimer(config.Lifetime + 0.5f);
            timer.Timeout += () => {
                if (IsInstanceValid(particles))
                {
                    particles.QueueFree();
                    activeParticles.Remove(particles);
                }
            };
        }

        private void CleanupOldParticles()
        {
            while (activeParticles.Count >= MaxActiveParticles)
            {
                var oldest = activeParticles[0];
                if (IsInstanceValid(oldest))
                {
                    oldest.QueueFree();
                }
                activeParticles.RemoveAt(0);
            }
        }

        /// <summary>
        /// 清除所有粒子效果
        /// </summary>
        public void ClearAllParticles()
        {
            foreach (var particle in activeParticles)
            {
                if (IsInstanceValid(particle))
                {
                    particle.QueueFree();
                }
            }
            activeParticles.Clear();
        }

        /// <summary>
        /// 获取粒子效果的颜色
        /// </summary>
        public Color GetParticleColor(ParticleEffectType effectType)
        {
            return effectType switch
            {
                ParticleEffectType.FireExplosion => Colors.Orange,
                ParticleEffectType.IceShatter => Colors.Cyan,
                ParticleEffectType.LightningStrike => Colors.Yellow,
                ParticleEffectType.HolyLight => Colors.White,
                ParticleEffectType.DarkVoid => new Color(0.3f, 0f, 0.5f),
                ParticleEffectType.PoisonCloud => Colors.Green,
                ParticleEffectType.Heal => Colors.LightGreen,
                ParticleEffectType.CritSparkle => Colors.Gold,
                ParticleEffectType.LevelUp => Colors.Gold,
                _ => Colors.White
            };
        }

        /// <summary>
        /// 获取粒子效果名称
        /// </summary>
        public string GetParticleName(ParticleEffectType effectType)
        {
            return effectType switch
            {
                ParticleEffectType.FireExplosion => "火焰爆炸",
                ParticleEffectType.IceShatter => "冰霜破碎",
                ParticleEffectType.LightningStrike => "闪电打击",
                ParticleEffectType.HolyLight => "圣光",
                ParticleEffectType.DarkVoid => "暗影",
                ParticleEffectType.PoisonCloud => "毒雾",
                ParticleEffectType.Heal => "治疗",
                ParticleEffectType.CritSparkle => "暴击闪光",
                ParticleEffectType.LevelUp => "升级",
                _ => "未知"
            };
        }
    }

    /// <summary>
    /// 粒子效果配置
    /// </summary>
    public class ParticleConfig
    {
        public int Amount { get; set; } = 20;
        public float Lifetime { get; set; } = 1.0f;
        public float Spread { get; set; } = 180f;
        public Vector3 Gravity { get; set; } = Vector3.Down * 100;
        public float InitialVelocity { get; set; } = 100;
        public float VelocityRandom { get; set; } = 30;
        public Curve Scale { get; set; }
        public Gradient Color { get; set; }
        public EmissionShape EmissionShape { get; set; } = EmissionShape.Point;
        public float EmissionRadius { get; set; } = 0f;
    }

    public enum EmissionShape
    {
        Point,
        Sphere,
        Box
    }
}
