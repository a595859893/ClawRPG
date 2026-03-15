using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Boss技能视觉特效系统 - 为Boss技能提供可视化效果
    /// </summary>
    public partial class BossAbilityVisualizer : BaseSystem {
        public static BossAbilityVisualizer Instance { get; private set; }

        // 技能视觉类型
        public enum BossAbilityVisualType {
            Circle,      // 圆形
            Rectangle,   // 矩形
            Cone,        // 锥形/扇形
            Line,        // 直线
            Target,      // 目标圈
            Self         // 自身周围
        }

        // 效果样式
        public enum BossAbilityEffectStyle {
            Warning,     // 预警阶段
            Active,     // 激活阶段
            Persistent, // 持续存在
            Pulse,      // 脉冲效果
            Follow      // 跟随目标
        }

        // 技能视觉配置数据结构
        public class BossAbilityVisual {
            public string AbilityId { get; set; }
            public BossAbilityVisualType VisualType { get; set; }
            public BossAbilityEffectStyle EffectStyle { get; set; }
            public Color WarningColor { get; set; }
            public Color ActiveColor { get; set; }
            public float WarningDuration { get; set; }
            public float ActiveDuration { get; set; }
            public float Radius { get; set; }
            public float Width { get; set; }
            public float Angle { get; set; }
            public float LineLength { get; set; }
            public int ParticleCount { get; set; }
            public string ParticleType { get; set; }
            public bool UseGradient { get; set; }
            public float PulseSpeed { get; set; }
        }

        // 活跃的视觉效果实例
        private class ActiveVisualInstance {
            public Node2D RootNode { get; set; }
            public string AbilityId { get; set; }
            public BossAbilityVisualType VisualType { get; set; }
            public BossAbilityEffectStyle EffectStyle { get; set; }
            public Color BaseColor { get; set; }
            public Vector2 StartPosition { get; set; }
            public Vector2 TargetPosition { get; set; }
            public float CurrentTime { get; set; }
            public float Duration { get; set; }
            public float Radius { get; set; }
            public float Angle { get; set; }
            public bool IsWarning { get; set; }
            public List<Node> ChildNodes { get; set; }
        }

        private Dictionary<string, BossAbilityVisual> _abilityVisuals;
        private List<ActiveVisualInstance> _activeVisuals;
        private int _maxConcurrentVisuals = 20;

        public override void _Ready() {
            Instance = this;
            _activeVisuals = new List<ActiveVisualInstance>();
            InitializeAbilityVisuals();
        }

        public override void _Process(double delta) {
            float deltaF = (float)delta;
            UpdateActiveVisuals(deltaF);
        }

        /// <summary>
        /// 初始化技能视觉配置
        /// </summary>
        private void InitializeAbilityVisuals() {
            _abilityVisuals = new Dictionary<string, BossAbilityVisual>();

            // 火焰吐息 - 锥形
            _abilityVisuals["fire_breath"] = new BossAbilityVisual {
                AbilityId = "fire_breath",
                VisualType = BossAbilityVisualType.Cone,
                EffectStyle = BossAbilityEffectStyle.Warning,
                WarningColor = new Color(1f, 0.3f, 0f, 0.4f),
                ActiveColor = new Color(1f, 0.5f, 0f, 0.7f),
                WarningDuration = 1.5f,
                ActiveDuration = 2f,
                Angle = 60f,
                Radius = 200f,
                ParticleCount = 30,
                ParticleType = "fire",
                UseGradient = true,
                PulseSpeed = 3f
            };

            // 闪电链 - 直线
            _abilityVisuals["lightning_chain"] = new BossAbilityVisual {
                AbilityId = "lightning_chain",
                VisualType = BossAbilityVisualType.Line,
                EffectStyle = BossAbilityEffectStyle.Instant,
                WarningColor = new Color(0.8f, 0.9f, 1f, 0.3f),
                ActiveColor = new Color(0.6f, 0.8f, 1f, 0.9f),
                WarningDuration = 0.5f,
                ActiveDuration = 0.3f,
                LineLength = 300f,
                Width = 30f,
                ParticleCount = 15,
                ParticleType = "lightning",
                UseGradient = true
            };

            // 毒云 - 圆形持续
            _abilityVisuals["poison_cloud"] = new BossAbilityVisual {
                AbilityId = "poison_cloud",
                VisualType = BossAbilityVisualType.Circle,
                EffectStyle = BossAbilityEffectStyle.Persistent,
                WarningColor = new Color(0.2f, 0.8f, 0.2f, 0.3f),
                ActiveColor = new Color(0.1f, 0.6f, 0.1f, 0.5f),
                WarningDuration = 1f,
                ActiveDuration = 4f,
                Radius = 120f,
                ParticleCount = 25,
                ParticleType = "poison",
                UseGradient = true,
                PulseSpeed = 2f
            };

            // 寒冰长矛 - 锥形
            _abilityVisuals["ice_lance"] = new BossAbilityVisual {
                AbilityId = "ice_lance",
                VisualType = BossAbilityVisualType.Cone,
                EffectStyle = BossAbilityEffectStyle.Warning,
                WarningColor = new Color(0.5f, 0.8f, 1f, 0.4f),
                ActiveColor = new Color(0.7f, 0.9f, 1f, 0.8f),
                WarningDuration = 1f,
                ActiveDuration = 1.5f,
                Angle = 45f,
                Radius = 250f,
                ParticleCount = 20,
                ParticleType = "ice",
                UseGradient = true
            };

            // 暗影箭 - 直线
            _abilityVisuals["shadow_bolt"] = new BossAbilityVisual {
                AbilityId = "shadow_bolt",
                VisualType = BossAbilityVisualType.Target,
                EffectStyle = BossAbilityEffectStyle.Warning,
                WarningColor = new Color(0.3f, 0f, 0.5f, 0.4f),
                ActiveColor = new Color(0.5f, 0f, 0.8f, 0.7f),
                WarningDuration = 0.8f,
                ActiveDuration = 1f,
                Radius = 50f,
                ParticleCount = 12,
                ParticleType = "shadow",
                UseGradient = true
            };

            // 地震猛击 - 圆形脉冲
            _abilityVisuals["ground_slam"] = new BossAbilityVisual {
                AbilityId = "ground_slam",
                VisualType = BossAbilityVisualType.Circle,
                EffectStyle = BossAbilityEffectStyle.Pulse,
                WarningColor = new Color(0.6f, 0.4f, 0.2f, 0.3f),
                ActiveColor = new Color(0.8f, 0.5f, 0.2f, 0.6f),
                WarningDuration = 0.5f,
                ActiveDuration = 1.5f,
                Radius = 180f,
                ParticleCount = 35,
                ParticleType = "earth",
                UseGradient = true,
                PulseSpeed = 4f
            };

            // 恐惧咆哮 - 自身脉冲
            _abilityVisuals["fear_roar"] = new BossAbilityVisual {
                AbilityId = "fear_roar",
                VisualType = BossAbilityVisualType.Self,
                EffectStyle = BossAbilityEffectStyle.Pulse,
                WarningColor = new Color(0.8f, 0f, 0f, 0.3f),
                ActiveColor = new Color(1f, 0f, 0f, 0.5f),
                WarningDuration = 0.3f,
                ActiveDuration = 1f,
                Radius = 150f,
                ParticleCount = 20,
                ParticleType = "fear",
                UseGradient = true,
                PulseSpeed = 5f
            };

            // 鲜血波纹 - 圆形脉冲
            _abilityVisuals["blood_ripple"] = new BossAbilityVisual {
                AbilityId = "blood_ripple",
                VisualType = BossAbilityVisualType.Circle,
                EffectStyle = BossAbilityEffectStyle.Pulse,
                WarningColor = new Color(0.6f, 0f, 0f, 0.3f),
                ActiveColor = new Color(0.8f, 0.1f, 0.1f, 0.6f),
                WarningDuration = 0.5f,
                ActiveDuration = 2f,
                Radius = 160f,
                ParticleCount = 25,
                ParticleType = "blood",
                UseGradient = true,
                PulseSpeed = 3f
            };

            // 奥术飞弹 - 目标
            _abilityVisuals["arcane_missile"] = new BossAbilityVisual {
                AbilityId = "arcane_missile",
                VisualType = BossAbilityVisualType.Target,
                EffectStyle = BossAbilityEffectStyle.Warning,
                WarningColor = new Color(0.6f, 0.3f, 1f, 0.4f),
                ActiveColor = new Color(0.8f, 0.5f, 1f, 0.7f),
                WarningDuration = 0.8f,
                ActiveDuration = 1.2f,
                Radius = 40f,
                ParticleCount = 10,
                ParticleType = "arcane",
                UseGradient = true
            };

            // 自我治疗 - 自身
            _abilityVisuals["self_heal"] = new BossAbilityVisual {
                AbilityId = "self_heal",
                VisualType = BossAbilityVisualType.Self,
                EffectStyle = BossAbilityEffectStyle.Pulse,
                WarningColor = new Color(0f, 0.8f, 0.2f, 0.3f),
                ActiveColor = new Color(0.2f, 1f, 0.4f, 0.6f),
                WarningDuration = 0.2f,
                ActiveDuration = 2f,
                Radius = 100f,
                ParticleCount = 30,
                ParticleType = "heal",
                UseGradient = true,
                PulseSpeed = 2.5f
            };

            // 闪现 - 瞬移
            _abilityVisuals["teleport"] = new BossAbilityVisual {
                AbilityId = "teleport",
                VisualType = BossAbilityVisualType.Self,
                EffectStyle = BossAbilityEffectStyle.Instant,
                WarningColor = new Color(0.5f, 0.5f, 1f, 0.3f),
                ActiveColor = new Color(0.7f, 0.7f, 1f, 0.8f),
                WarningDuration = 0.1f,
                ActiveDuration = 0.3f,
                Radius = 60f,
                ParticleCount = 20,
                ParticleType = "arcane",
                UseGradient = true
            };

            // 召唤小怪 - 自身
            _abilityVisuals["summon_minions"] = new BossAbilityVisual {
                AbilityId = "summon_minions",
                VisualType = BossAbilityVisualType.Circle,
                EffectStyle = BossAbilityEffectStyle.Persistent,
                WarningColor = new Color(0.4f, 0f, 0.6f, 0.3f),
                ActiveColor = new Color(0.6f, 0.2f, 0.8f, 0.6f),
                WarningDuration = 0.5f,
                ActiveDuration = 1.5f,
                Radius = 120f,
                ParticleCount = 40,
                ParticleType = "shadow",
                UseGradient = true,
                PulseSpeed = 4f
            };

            // 额外别名映射
            _abilityVisuals["flame_breath"] = _abilityVisuals["fire_breath"];
            _abilityVisuals["toxic_gas"] = _abilityVisuals["poison_cloud"];
            _abilityVisuals["dark_bolt"] = _abilityVisuals["shadow_bolt"];
            _abilityVisuals["shadow_burst"] = _abilityVisuals["shadow_bolt"];
        }

        /// <summary>
        /// 触发技能视觉特效
        /// </summary>
        public void TriggerAbilityVisual(string abilityId, Vector2 startPos, Vector2 targetPos, float facingAngle = 0f) {
            if (!_abilityVisuals.ContainsKey(abilityId)) {
                GD.Print($"[BossAbilityVisualizer] Unknown ability: {abilityId}");
                return;
            }

            // 清理过多的活跃视觉效果
            while (_activeVisuals.Count >= _maxConcurrentVisuals) {
                var oldest = _activeVisuals[0];
                if (oldest.RootNode != null && oldest.RootNode.IsInsideTree()) {
                    oldest.RootNode.QueueFree();
                }
                _activeVisuals.RemoveAt(0);
            }

            var config = _abilityVisuals[abilityId];
            CreateVisualInstance(config, startPos, targetPos, facingAngle);
        }

        /// <summary>
        /// 创建视觉效果实例
        /// </summary>
        private void CreateVisualInstance(BossAbilityVisual config, Vector2 startPos, Vector2 targetPos, float facingAngle) {
            // 创建根节点
            var rootNode = new Node2D();
            rootNode.Position = startPos;
            GetTree().CurrentScene.AddChild(rootNode);

            var instance = new ActiveVisualInstance {
                RootNode = rootNode,
                AbilityId = config.AbilityId,
                VisualType = config.VisualType,
                EffectStyle = config.EffectStyle,
                BaseColor = config.WarningColor,
                StartPosition = startPos,
                TargetPosition = targetPos,
                CurrentTime = 0f,
                Duration = config.WarningDuration,
                Radius = config.Radius,
                Angle = facingAngle,
                IsWarning = true,
                ChildNodes = new List<Node>()
            };

            // 根据视觉类型创建节点
            switch (config.VisualType) {
                case BossAbilityVisualType.Circle:
                case BossAbilityVisualType.Self:
                    CreateCircleVisual(instance, config, false);
                    break;
                case BossAbilityVisualType.Cone:
                    CreateConeVisual(instance, config, facingAngle);
                    break;
                case BossAbilityVisualType.Line:
                case BossAbilityVisualType.Target:
                    CreateLineVisual(instance, config, targetPos);
                    break;
                case BossAbilityVisualType.Rectangle:
                    CreateRectangleVisual(instance, config);
                    break;
            }

            _activeVisuals.Add(instance);

            // 延迟切换到激活阶段
            var timer = GetTree().CreateTimer(config.WarningDuration);
            timer.timeout += () => {
                if (instance.RootNode != null && instance.RootNode.IsInsideTree()) {
                    instance.IsWarning = false; 
                    instance.CurrentTime = 0f;
                    instance.Duration = config.ActiveDuration;
                    instance.BaseColor = config.ActiveColor;
                    UpdateVisualColor(instance, config.ActiveColor);
                    
                    // 添加激活阶段粒子
                    if (config.ActiveDuration > 0.5f) {
                        CreateParticles(instance, config, true);
                    }
                }
            };

            // 延迟销毁
            var destroyTimer = GetTree().CreateTimer(config.WarningDuration + config.ActiveDuration);
            destroyTimer.timeout += () => {
                if (instance.RootNode != null && instance.RootNode.IsInsideTree()) {
                    instance.RootNode.QueueFree();
                }
                _activeVisuals.Remove(instance);
            };
        }

        /// <summary>
        /// 创建圆形视觉效果
        /// </summary>
        private void CreateCircleVisual(ActiveVisualInstance instance, BossAbilityVisual config, bool isActive) {
            var color = isActive ? config.ActiveColor : config.WarningColor;
            
            // 外圈
            var outerCircle = new Node2D();
            outerCircle.SetScript(GD.Load<Script>("res://Scripts/UI/BossAbilityVisualizer.cs"));
            instance.RootNode.AddChild(outerCircle);
            instance.ChildNodes.Add(outerCircle);

            // 内圈
            var innerCircle = new Node2D();
            innerCircle.Position = Vector2.Zero;
            instance.RootNode.AddChild(innerCircle);
            instance.ChildNodes.Add(innerCircle);

            // 动态更新
            outerCircle.SetMeta("type", "circle_outer");
            outerCircle.SetMeta("config", config);
            outerCircle.SetMeta("instance", instance);
            innerCircle.SetMeta("type", "circle_inner");
            innerCircle.SetMeta("config", config);
            innerCircle.SetMeta("instance", instance);
        }

        /// <summary>
        /// 创建锥形视觉效果
        /// </summary>
        private void CreateConeVisual(ActiveVisualInstance instance, BossAbilityVisual config, float facingAngle) {
            instance.RootNode.Rotation = facingAngle;

            var cone = new Node2D();
            instance.RootNode.AddChild(cone);
            instance.ChildNodes.Add(cone);
            cone.SetMeta("type", "cone");
            cone.SetMeta("config", config);
            cone.SetMeta("instance", instance);
        }

        /// <summary>
        /// 创建直线视觉效果
        /// </summary>
        private void CreateLineVisual(ActiveVisualInstance instance, BossAbilityVisual config, Vector2 targetPos) {
            var direction = (targetPos - instance.StartPosition).Normalized();
            var length = instance.StartPos.DistanceTo(targetPos);
            
            instance.RootNode.Rotation = direction.Angle();

            var line = new Node2D();
            instance.RootNode.AddChild(line);
            instance.ChildNodes.Add(line);
            line.SetMeta("type", "line");
            line.SetMeta("config", config);
            line.SetMeta("instance", instance);
            line.SetMeta("length", length);
        }

        /// <summary>
        /// 创建矩形视觉效果
        /// </summary>
        private void CreateRectangleVisual(ActiveVisualInstance instance, BossAbilityVisual config) {
            var rect = new Node2D();
            instance.RootNode.AddChild(rect);
            instance.ChildNodes.Add(rect);
            rect.SetMeta("type", "rectangle");
            rect.SetMeta("config", config);
            rect.SetMeta("instance", instance);
        }

        /// <summary>
        /// 创建粒子效果
        /// </summary>
        private void CreateParticles(ActiveVisualInstance instance, BossAbilityVisual config, bool isActive) {
            var particleCount = config.ParticleCount / 2;
            var particleColor = isActive ? config.ActiveColor : config.WarningColor;
            
            for (int i = 0; i < particleCount; i++) {
                var particle = new Node2D();
                instance.RootNode.AddChild(particle);
                instance.ChildNodes.Add(particle);
                
                // 随机位置
                float angle = (float)GD.RandRange(0, Mathf.Tau);
                float dist = (float)GD.RandRange(0, config.Radius);
                particle.Position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
                
                particle.SetMeta("type", "particle");
                particle.SetMeta("particle_type", config.ParticleType);
                particle.SetMeta("color", particleColor);
                particle.SetMeta("life", isActive ? config.ActiveDuration : config.WarningDuration);
            }
        }

        /// <summary>
        /// 更新视觉效果颜色
        /// </summary>
        private void UpdateVisualColor(ActiveVisualInstance instance, Color color) {
            // 更新所有子节点的颜色
            foreach (var child in instance.ChildNodes) {
                if (child is Node2D node2d) {
                    // 颜色更新由 _Process 处理
                }
            }
        }

        /// <summary>
        /// 更新所有活跃的视觉效果
        /// </summary>
        private void UpdateActiveVisuals(float delta) {
            foreach (var instance in _activeVisuals) {
                if (instance.RootNode == null || !instance.RootNode.IsInsideTree()) continue;

                instance.CurrentTime += delta;
                
                // 根据效果样式更新
                switch (instance.EffectStyle) {
                    case BossAbilityEffectStyle.Pulse:
                        UpdatePulseEffect(instance, delta);
                        break;
                    case BossAbilityEffectStyle.Follow:
                        UpdateFollowEffect(instance);
                        break;
                }

                // 绘制效果
                QueueRedrawInstance(instance);
            }
        }

        /// <summary>
        /// 更新脉冲效果
        /// </summary>
        private void UpdatePulseEffect(ActiveVisualInstance instance, float delta) {
            if (!_abilityVisuals.TryGetValue(instance.AbilityId, out var config)) return;
            
            float pulse = Mathf.Sin(instance.CurrentTime * config.PulseSpeed) * 0.5f + 0.5f;
            float scale = 1f + pulse * 0.3f;
            
            if (instance.RootNode != null) {
                instance.RootNode.Scale = new Vector2(scale, scale);
            }
        }

        /// <summary>
        /// 更新跟随效果
        /// </summary>
        private void UpdateFollowEffect(ActiveVisualInstance instance) {
            if (instance.RootNode != null && instance.TargetPosition != Vector2.Zero) {
                var direction = (instance.TargetPosition - instance.RootNode.Position).Normalized();
                instance.RootNode.Position += direction * 100f * (float)GetTree().ProcessTime;
            }
        }

        /// <summary>
        /// 队列重绘实例
        /// </summary>
        private void QueueRedrawInstance(ActiveVisualInstance instance) {
            // 触发重绘
            foreach (var child in instance.ChildNodes) {
                if (child is Node2D node) {
                    node.QueueRedraw();
                }
            }
        }

        /// <summary>
        /// 清除所有视觉效果
        /// </summary>
        public void ClearAllVisuals() {
            foreach (var instance in _activeVisuals) {
                if (instance.RootNode != null && instance.RootNode.IsInsideTree()) {
                    instance.RootNode.QueueFree();
                }
            }
            _activeVisuals.Clear();
        }

        /// <summary>
        /// 获取技能视觉配置
        /// </summary>
        public BossAbilityVisual GetAbilityVisual(string abilityId) {
            if (_abilityVisuals.TryGetValue(abilityId, out var visual)) {
                return visual;
            }
            return null;
        }

        /// <summary>
        /// 是否有指定技能的视觉配置
        /// </summary>
        public bool HasAbilityVisual(string abilityId) {
            return _abilityVisuals.ContainsKey(abilityId);
        }

        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData() {
            var data = new Dictionary();
            data["maxConcurrentVisuals"] = _maxConcurrentVisuals;
            return data;
        }

        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data) {
            if (data == null) return;
            
            if (data.Contains("maxConcurrentVisuals")) {
                _maxConcurrentVisuals = (int)data["maxConcurrentVisuals"];
            }
        }
    }
}
