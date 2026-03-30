using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Combat {
    /// <summary>
    /// VFX Player - 负责特效播放控制
    /// 单一职责：创建和管理VFX的视觉表现，处理动画和生命周期
    /// </summary>
    public partial class VFXPlayer : BaseSystem {
        public static VFXPlayer Instance { get; private set; }
        
        // 引用
        private VFXLibrary _library;
        private Control _damageNumbersContainer;
        private Control _effectsContainer;
        private Camera3D _mainCamera;
        private CanvasLayer _canvasLayer;
        private Node _sceneRoot;
        
        // 配置
        [Export] private float defaultVFXLifetime = 1.5f;
        [Export] private float defaultDamageNumberLifetime = 1.5f;
        [Export] private float screenShakeIntensity = 10f;
        
        // 活跃的视觉效果节点追踪
        private List<Node> activeVFXNodes = new List<Node>();
        
        public override void _Ready() {
            base._Ready();
            Instance = this;
            Initialize();
        }
        
        public override void _Process(double delta) {
            // VFX Player不负责更新，由协调者处理
        }
        
        private void Initialize() {
            _library = VFXLibrary.Instance;
            SetupContainers();
            GetReferences();
            GD.Print("[VFXPlayer] Initialized");
        }
        
        private void SetupContainers() {
            // 创建伤害数字容器
            _damageNumbersContainer = new Control();
            _damageNumbersContainer.Name = "DamageNumbersContainer";
            _damageNumbersContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _damageNumbersContainer.MouseFilter = Control.MouseFilterEnum.Ignore;
            
            // 创建特效容器
            _effectsContainer = new Control();
            _effectsContainer.Name = "EffectsContainer";
            _effectsContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _effectsContainer.MouseFilter = Control.MouseFilterEnum.Ignore;
            
            // 创建CanvasLayer
            _canvasLayer = new CanvasLayer();
            _canvasLayer.Name = "CombatVFXLayer";
            _canvasLayer.Layer = 100;
            _canvasLayer.AddChild(_damageNumbersContainer);
            _canvasLayer.AddChild(_effectsContainer);
            
            // 获取场景根节点
            _sceneRoot = GetTree().CurrentScene;
            if (_sceneRoot != null) {
                _sceneRoot.AddChild(_canvasLayer);
            }
        }
        
        private void GetReferences() {
            // 获取主相机
            _mainCamera = GetViewport().GetCamera3D();
        }
        
        #region Container Access
        
        /// <summary>
        /// 获取伤害数字容器
        /// </summary>
        public Control GetDamageNumbersContainer() => _damageNumbersContainer;
        
        /// <summary>
        /// 获取特效容器
        /// </summary>
        public Control GetEffectsContainer() => _effectsContainer;
        
        /// <summary>
        /// 获取主相机
        /// </summary>
        public Camera3D GetMainCamera() => _mainCamera;
        
        /// <summary>
        /// 获取场景根节点
        /// </summary>
        public Node GetSceneRoot() => _sceneRoot;
        
        #endregion
        
        #region Damage Number
        
        /// <summary>
        /// 播放伤害数字
        /// </summary>
        public void PlayDamageNumber(DamageNumber dn) {
            var label = CreateDamageNumberUI(dn);
            _damageNumbersContainer.AddChild(label);
            
            CreateDamageNumberAnimation(label, dn);
        }
        
        private Label CreateDamageNumberUI(DamageNumber dn) {
            var label = new Label();
            string text;
            
            if (dn.Type == DamageNumberType.Miss || dn.Type == DamageNumberType.Dodge) {
                text = "MISS";
            } else {
                text = Mathf.RoundToInt(dn.Value).ToString();
            }
            
            label.Text = text;
            
            var style = _library?.GetDamageStyle(dn.Type);
            if (style != null) {
                label.AddThemeFontSizeOverride("font_size", (int)style.Size);
                label.Modulate = style.Color;
            } else {
                label.AddThemeFontSizeOverride("font_size", 24);
                label.Modulate = Colors.White;
            }
            
            // 设置位置
            Vector2 screenPos = WorldToScreen(dn.Position);
            label.Position = screenPos;
            
            // 添加阴影效果
            label.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.5f));
            label.AddThemeConstantOverride("font_shadow_size", 2);
            
            return label;
        }
        
        private void CreateDamageNumberAnimation(Label label, DamageNumber dn) {
            var lifetime = dn.LifeTime > 0 ? dn.LifeTime : defaultDamageNumberLifetime;
            
            var tween = CreateTween();
            tween.SetParallel(true);
            
            // 向上移动
            tween.TweenProperty(label, "position:y", label.Position.y + dn.Velocity.Y * lifetime * 0.5f, lifetime);
            
            // 水平移动（暴击有特殊效果）
            if (dn.Type == DamageNumberType.Critical) {
                tween.TweenProperty(label, "position:x", label.Position.X + dn.Velocity.X, lifetime);
            }
            
            // 淡出
            tween.TweenProperty(label, "modulate:a", 0f, lifetime);
            
            // 完成后移除
            tween.TweenCallback(() => {
                if (IsInstanceValid(label)) {
                    label.QueueFree();
                    activeVFXNodes.Remove(label);
                }
            });
            
            // 缩放动画（暴击有弹跳效果）
            if (dn.Type == DamageNumberType.Critical) {
                var scaleTween = CreateTween();
                scaleTween.TweenProperty(label, "scale", new Vector2(1.5f, 1.5f), 0.1f);
                scaleTween.TweenProperty(label, "scale", new Vector2(1f, 1f), 0.2f);
            }
        }
        
        #endregion
        
        #region VFX
        
        /// <summary>
        /// 播放VFX
        /// </summary>
        public void PlayVFX(VFXInstance vfx) {
            var meshInstance = CreateVFXVisual(vfx);
            if (meshInstance == null) return;
            
            _sceneRoot.AddChild(meshInstance);
            activeVFXNodes.Add(meshInstance);
            
            CreateVFXAnimation(meshInstance, vfx);
        }
        
        private MeshInstance3D CreateVFXVisual(VFXInstance vfx) {
            var meshInstance = new MeshInstance3D();
            
            var sphere = new SphereMesh();
            sphere.Radius = 0.3f * vfx.Scale;
            sphere.Height = 0.6f * vfx.Scale;
            meshInstance.Mesh = sphere;
            
            // 使用库中的材质或创建新的
            var material = new StandardMaterial3D();
            material.AlbedoColor = vfx.Color;
            material.EmissionEnabled = true;
            material.Emission = vfx.Color;
            material.EmissionEnergyMultiplier = 2f;
            material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            material.AlbedoColor = new Color(vfx.Color.R, vfx.Color.G, vfx.Color.B, 0.8f);
            meshInstance.MaterialOverride = material;
            
            meshInstance.Position = vfx.Position;
            
            return meshInstance;
        }
        
        private void CreateVFXAnimation(MeshInstance3D meshInstance, VFXInstance vfx) {
            var lifetime = vfx.LifeTime;
            
            var tween = CreateTween();
            tween.SetParallel(true);
            
            // 放大
            tween.TweenProperty(meshInstance, "scale", new Vector3(1.5f, 1.5f, 1.5f) * vfx.Scale, lifetime * 0.3f);
            
            // 缩小并消失
            tween.TweenProperty(meshInstance, "scale", Vector3.Zero, lifetime * 0.7f).SetDelay(lifetime * 0.3f);
            
            // 淡出
            if (meshInstance.MaterialOverride is StandardMaterial3D mat) {
                tween.TweenProperty(mat, "albedo_color:a", 0f, lifetime * 0.7f).SetDelay(lifetime * 0.3f);
            }
            
            // 完成后移除
            tween.TweenCallback(() => {
                if (IsInstanceValid(meshInstance)) {
                    meshInstance.QueueFree();
                    activeVFXNodes.Remove(meshInstance);
                }
            });
        }
        
        #endregion
        
        #region Screen Effect
        
        /// <summary>
        /// 播放屏幕特效
        /// </summary>
        public void PlayScreenEffect(ScreenEffect effect) {
            var colorRect = CreateScreenEffectOverlay(effect);
            _effectsContainer.AddChild(colorRect);
            activeVFXNodes.Add(colorRect);
            
            CreateScreenEffectAnimation(colorRect, effect);
        }
        
        private ColorRect CreateScreenEffectOverlay(ScreenEffect effect) {
            var colorRect = new ColorRect();
            colorRect.Color = effect.Color;
            colorRect.Color.A = effect.Intensity;
            colorRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            colorRect.MouseFilter = Control.MouseFilterEnum.Ignore;
            
            return colorRect;
        }
        
        private void CreateScreenEffectAnimation(ColorRect colorRect, ScreenEffect effect) {
            var tween = CreateTween();
            tween.TweenProperty(colorRect, "color:a", 0f, effect.Duration);
            tween.TweenCallback(() => {
                if (IsInstanceValid(colorRect)) {
                    colorRect.QueueFree();
                    activeVFXNodes.Remove(colorRect);
                }
            });
            
            // 屏幕震动
            if (effect.Type == ScreenEffectType.Shake) {
                PlayScreenShake(effect.Intensity);
            }
            
            // 慢动作
            if (effect.Type == ScreenEffectType.SlowMo) {
                PlaySlowMotion(effect.Intensity, effect.Duration);
            }
        }
        
        private void PlayScreenShake(float intensity) {
            if (_mainCamera == null) return;
            
            var shakeTween = CreateTween();
            Vector3 originalPos = _mainCamera.Position;
            
            for (int i = 0; i < 5; i++) {
                shakeTween.TweenProperty(_mainCamera, "position", 
                    originalPos + new Vector3(
                        GD.Randf() * intensity - intensity / 2,
                        GD.Randf() * intensity - intensity / 2,
                        0
                    ), 0.04f);
            }
            shakeTween.TweenProperty(_mainCamera, "position", originalPos, 0.04f);
        }
        
        private void PlaySlowMotion(float intensity, float duration) {
            Engine.TimeScale = intensity;
            GetTree().CreateTimer(duration).Timeout += () => {
                Engine.TimeScale = 1f;
            };
        }
        
        #endregion
        
        #region Combo Effect
        
        /// <summary>
        /// 播放连击特效
        /// </summary>
        public void PlayComboEffect(ComboEffect effect) {
            var label = CreateComboUI(effect);
            _effectsContainer.AddChild(label);
            activeVFXNodes.Add(label);
            
            CreateComboAnimation(label, effect);
        }
        
        private Label CreateComboUI(ComboEffect effect) {
            var label = new Label();
            label.Text = $"{effect.ComboCount} COMBO!";
            label.AddThemeFontSizeOverride("font_size", (int)CombatVFXDatabase.GetComboSize(effect.ComboCount));
            label.Modulate = CombatVFXDatabase.GetComboColor(effect.ComboCount);
            
            // 设置位置（屏幕中心上方）
            Vector2 screenPos = WorldToScreen(effect.Position);
            screenPos.y -= 100;
            label.Position = screenPos;
            
            // 添加阴影
            label.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.7f));
            label.AddThemeConstantOverride("font_shadow_size", 3);
            
            return label;
        }
        
        private void CreateComboAnimation(Label label, ComboEffect effect) {
            var tween = CreateTween();
            tween.SetParallel(true);
            
            // 放大出现
            label.Scale = new Vector2(0.5f, 0.5f);
            tween.TweenProperty(label, "scale", new Vector2(1.2f, 1.2f), 0.2f);
            tween.TweenProperty(label, "scale", new Vector2(1f, 1f), 0.1f);
            
            // 上浮
            tween.TweenProperty(label, "position:y", label.Position.y - 50f, effect.LifeTime);
            
            // 淡出
            tween.TweenProperty(label, "modulate:a", 0f, effect.LifeTime);
            
            tween.TweenCallback(() => {
                if (IsInstanceValid(label)) {
                    label.QueueFree();
                    activeVFXNodes.Remove(label);
                }
            });
        }
        
        #endregion
        
        #region Critical Glow
        
        /// <summary>
        /// 播放暴击光效
        /// </summary>
        public void PlayCriticalGlow(CriticalGlow glow) {
            var meshInstance = CreateCriticalGlowVisual(glow);
            if (meshInstance == null) return;
            
            var tempParent = glow.Target?.GetParent();
            if (tempParent != null) {
                tempParent.AddChild(meshInstance);
                activeVFXNodes.Add(meshInstance);
            }
            
            CreateCriticalGlowAnimation(meshInstance, glow);
        }
        
        private MeshInstance3D CreateCriticalGlowVisual(CriticalGlow glow) {
            if (glow.Target == null || !IsInstanceValid(glow.Target)) return null;
            
            var meshInstance = new MeshInstance3D();
            var box = new BoxMesh();
            box.Size = new Vector3(1.5f, 1.5f, 1.5f);
            meshInstance.Mesh = box;
            
            var material = new StandardMaterial3D();
            material.AlbedoColor = glow.GlowColor;
            material.EmissionEnabled = true;
            material.Emission = glow.GlowColor;
            material.EmissionEnergyMultiplier = glow.Intensity;
            material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            material.AlbedoColor = new Color(glow.GlowColor.R, glow.GlowColor.G, glow.GlowColor.B, 0.3f);
            meshInstance.MaterialOverride = material;
            
            meshInstance.Position = glow.Target.Position;
            
            return meshInstance;
        }
        
        private void CreateCriticalGlowAnimation(MeshInstance3D meshInstance, CriticalGlow glow) {
            var tween = CreateTween();
            tween.SetParallel(true);
            
            // 脉动效果
            tween.TweenProperty(meshInstance, "scale", new Vector3(2f, 2f, 2f), glow.Duration * 0.5f);
            tween.TweenProperty(meshInstance, "scale", new Vector3(1.5f, 1.5f, 1.5f), glow.Duration * 0.5f).SetDelay(glow.Duration * 0.5f);
            
            // 淡出
            if (meshInstance.MaterialOverride is StandardMaterial3D mat) {
                tween.TweenProperty(mat, "albedo_color:a", 0f, glow.Duration);
            }
            
            // 跟随目标
            tween.TweenCallback(() => {
                if (IsInstanceValid(meshInstance)) {
                    meshInstance.QueueFree();
                    activeVFXNodes.Remove(meshInstance);
                }
            });
            
            // 开始跟随
            _ = FollowTargetAsync(glow.Target, meshInstance, glow.Duration);
        }
        
        private async System.Threading.Tasks.Task FollowTargetAsync(Node3D target, Node3D follower, float duration) {
            float elapsed = 0;
            while (elapsed < duration && IsInstanceValid(target) && IsInstanceValid(follower)) {
                follower.Position = target.Position;
                elapsed += 0.016f;
                await System.Threading.Tasks.Task.Delay(16);
            }
        }
        
        #endregion
        
        #region Utility
        
        /// <summary>
        /// 世界坐标转屏幕坐标
        /// </summary>
        public Vector2 WorldToScreen(Vector3 worldPos) {
            if (_mainCamera == null) return Vector2.Zero;
            
            var screenPos = _mainCamera.UnprojectPosition(worldPos);
            return new Vector2(screenPos.x, screenPos.y);
        }
        
        /// <summary>
        /// 获取活跃VFX节点数量
        /// </summary>
        public int GetActiveVFXCount() => activeVFXNodes.Count;
        
        /// <summary>
        /// 清理所有活跃VFX
        /// </summary>
        public void ClearAll() {
            foreach (var node in activeVFXNodes) {
                if (IsInstanceValid(node)) {
                    node.QueueFree();
                }
            }
            activeVFXNodes.Clear();
        }
        
        #endregion
        
        #region Save/Load
        
        public override Dictionary<string, object> ExportSaveData() {
            return new Dictionary {
                { "active_vfx_count", activeVFXNodes.Count }
            };
        }
        
        public override void ImportSaveData(Dictionary<string, object> data) {
            // VFX Player不需要持久化
        }
        
        #endregion
    }
}
