using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Combat {
    /// <summary>
    /// 战斗视觉特效系统 - 管理战斗中的各种视觉反馈效果
    /// </summary>
    public partial class CombatVFXSystem : BaseSystem {
        public static CombatVFXSystem Instance { get; private set; }
        
        // 数据
        public PlayerCombatVFXData PlayerData { get; private set; } = new PlayerCombatVFXData();
        
        // 活跃的特效实例
        private List<DamageNumber> activeDamageNumbers = new List<DamageNumber>();
        private List<VFXInstance> activeVFX = new List<VFXInstance>();
        private List<ScreenEffect> activeScreenEffects = new List<ScreenEffect>();
        private List<ComboEffect> activeComboEffects = new List<ComboEffect>();
        private List<CriticalGlow> activeCriticalGlows = new List<CriticalGlow>();
        
        // 引用
        private Control damageNumbersContainer;
        private Control effectsContainer;
        private Camera3D mainCamera;
        private Player player;
        
        // 配置
        [Export] private int maxDamageNumbers = 50;
        [Export] private float damageNumberLifetime = 1.5f;
        [Export] private bool enableScreenEffects = true;
        [Export] private bool enableCriticalGlow = true;
        [Export] private bool enableComboEffects = true;
        
        // 信号
        [Signal] public delegate void DamageNumberCreatedEventHandler(DamageNumberType type, float value);
        [Signal] public delegate void VFXPlayedEventHandler(VFXType type, Vector3 position);
        [Signal] public delegate void ScreenEffectTriggeredEventHandler(ScreenEffectType type);
        [Signal] public delegate void ComboMilestoneReachedEventHandler(int comboCount, string milestone);
        
        protected override void Initialize() {
            Instance = this;
            SetupContainers();
            GetReferences();
            base.Initialize();
        }
        
        private void SetupContainers() {
            // 创建伤害数字容器
            damageNumbersContainer = new Control();
            damageNumbersContainer.Name = "DamageNumbersContainer";
            damageNumbersContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            damageNumbersContainer.MouseFilter = Control.MouseFilterEnum.Ignore;
            
            // 创建特效容器
            effectsContainer = new Control();
            effectsContainer.Name = "EffectsContainer";
            effectsContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            effectsContainer.MouseFilter = Control.MouseFilterEnum.Ignore;
            
            // 添加到CanvasLayer
            var canvasLayer = new CanvasLayer();
            canvasLayer.Name = "CombatVFXLayer";
            canvasLayer.Layer = 100;
            canvasLayer.AddChild(damageNumbersContainer);
            canvasLayer.AddChild(effectsContainer);
            
            GetTree().CurrentScene.AddChild(canvasLayer);
        }
        
        private void GetReferences() {
            // 获取主相机
            mainCamera = GetViewport().GetCamera3D();
            
            // 获取玩家
            var playerNode = GetTree().GetFirstNodeInGroup("Player");
            if (playerNode is Player p) {
                player = p;
            }
        }
        
        public override void _Process(double delta) {
            float dt = (float)delta;
            
            // 更新伤害数字
            UpdateDamageNumbers(dt);
            
            // 更新特效
            UpdateVFX(dt);
            
            // 更新屏幕特效
            UpdateScreenEffects(dt);
            
            // 更新连击特效
            UpdateComboEffects(dt);
            
            // 更新暴击光效
            UpdateCriticalGlows(dt);
        }
        
        #region 伤害数字
        
        /// <summary>
        /// 显示伤害数字
        /// </summary>
        public void ShowDamageNumber(float damage, Vector3 worldPosition, DamageNumberType type, bool isEnemy = false) {
            if (activeDamageNumbers.Count >= maxDamageNumbers) {
                // 移除最旧的伤害数字
                var oldest = activeDamageNumbers[0];
                if (oldest.LifeTime > 0) {
                    oldest.CurrentTime = oldest.LifeTime + 1; // 强制移除
                }
            }
            
            var damageNumber = new DamageNumber {
                Value = damage,
                Type = type,
                Position = worldPosition,
                Velocity = CombatVFXDatabase.DamageNumberVelocities[type],
                LifeTime = damageNumberLifetime,
                CurrentTime = 0,
                IsEnemy = isEnemy
            };
            
            activeDamageNumbers.Add(damageNumber);
            PlayerData.TotalDamageNumbers++;
            
            if (type == DamageNumberType.Critical) {
                PlayerData.CriticalHits++;
            }
            
            // 创建UI显示
            CreateDamageNumberUI(damageNumber);
            
            EmitSignal(SignalName.DamageNumberCreated, type, damage);
        }
        
        private void CreateDamageNumberUI(DamageNumber dn) {
            var label = new Label();
            string text;
            
            if (dn.Type == DamageNumberType.Miss || dn.Type == DamageNumberType.Dodge) {
                text = "MISS";
            } else {
                text = Mathf.RoundToInt(dn.Value).ToString();
            }
            
            label.Text = text;
            label.AddThemeFontSizeOverride("font_size", (int)CombatVFXDatabase.DamageNumberSizes[dn.Type]);
            label.Modulate = CombatVFXDatabase.DamageNumberColors[dn.Type];
            
            // 设置位置
            Vector2 screenPos = WorldToScreen(dn.Position);
            label.Position = screenPos;
            
            // 添加阴影效果
            label.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.5f));
            label.AddThemeConstantOverride("font_shadow_size", 2);
            
            damageNumbersContainer.AddChild(label);
            
            // 创建动画
            var tween = CreateTween();
            tween.SetParallel(true);
            
            // 向上移动
            tween.TweenProperty(label, "position:y", label.Position.y + dn.Velocity.Y * damageNumberLifetime * 0.5f, damageNumberLifetime);
            
            // 水平移动（暴击有特殊效果）
            if (dn.Type == DamageNumberType.Critical) {
                tween.TweenProperty(label, "position:x", label.Position.X + dn.Velocity.X, damageNumberLifetime);
            }
            
            // 淡出
            tween.TweenProperty(label, "modulate:a", 0f, damageNumberLifetime);
            
            // 完成后移除
            tween.TweenCallback(() => {
                if (IsInstanceValid(label)) {
                    label.QueueFree();
                }
            });
            
            // 缩放动画（暴击有弹跳效果）
            if (dn.Type == DamageNumberType.Critical) {
                var scaleTween = CreateTween();
                scaleTween.TweenProperty(label, "scale", new Vector2(1.5f, 1.5f), 0.1f);
                scaleTween.TweenProperty(label, "scale", new Vector2(1f, 1f), 0.2f);
            }
        }
        
        private void UpdateDamageNumbers(float dt) {
            for (int i = activeDamageNumbers.Count - 1; i >= 0; i--) {
                var dn = activeDamageNumbers[i];
                dn.CurrentTime += dt;
                
                if (dn.CurrentTime >= dn.LifeTime) {
                    activeDamageNumbers.RemoveAt(i);
                }
            }
        }
        
        #endregion
        
        #region 特效
        
        /// <summary>
        /// 播放特效
        /// </summary>
        public void PlayVFX(VFXType type, Vector3 worldPosition, Node3D target = null) {
            var config = CombatVFXDatabase.VFXConfigs[type];
            
            var vfx = new VFXInstance {
                ID = config.ID,
                Type = type,
                Duration = config.Duration,
                Position = worldPosition,
                Color = config.Color,
                Scale = config.Scale,
                LifeTime = config.Lifetime,
                CurrentTime = 0,
                Target = target
            };
            
            activeVFX.Add(vfx);
            PlayerData.VFXPlayed++;
            
            // 创建视觉表现
            CreateVFXVisual(vfx);
            
            EmitSignal(SignalName.VFXPlayed, type, worldPosition);
        }
        
        private void CreateVFXVisual(VFXInstance vfx) {
            // 创建简单的粒子效果（使用Sprite3D或MeshInstance3D）
            // 这里创建一个简单的发光球体来表示特效
            
            var meshInstance = new MeshInstance3D();
            var sphere = new SphereMesh();
            sphere.Radius = 0.3f * vfx.Scale;
            sphere.Height = 0.6f * vfx.Scale;
            meshInstance.Mesh = sphere;
            
            // 创建发光材质
            var material = new StandardMaterial3D();
            material.AlbedoColor = vfx.Color;
            material.EmissionEnabled = true;
            material.Emission = vfx.Color;
            material.EmissionEnergyMultiplier = 2f;
            material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            material.AlbedoColor = new Color(vfx.Color.R, vfx.Color.G, vfx.Color.B, 0.8f);
            meshInstance.MaterialOverride = material;
            
            meshInstance.Position = vfx.Position;
            GetTree().CurrentScene.AddChild(meshInstance);
            
            // 创建动画
            var tween = CreateTween();
            tween.SetParallel(true);
            
            // 放大
            tween.TweenProperty(meshInstance, "scale", new Vector3(1.5f, 1.5f, 1.5f) * vfx.Scale, vfx.LifeTime * 0.3f);
            
            // 缩小并消失
            tween.TweenProperty(meshInstance, "scale", Vector3.Zero, vfx.LifeTime * 0.7f).SetDelay(vfx.LifeTime * 0.3f);
            
            // 淡出
            tween.TweenProperty(material, "albedo_color:a", 0f, vfx.LifeTime * 0.7f).SetDelay(vfx.LifeTime * 0.3f);
            
            // 完成后移除
            tween.TweenCallback(() => {
                if (IsInstanceValid(meshInstance)) {
                    meshInstance.QueueFree();
                }
            });
        }
        
        private void UpdateVFX(float dt) {
            for (int i = activeVFX.Count - 1; i >= 0; i--) {
                var vfx = activeVFX[i];
                vfx.CurrentTime += dt;
                
                if (vfx.CurrentTime >= vfx.LifeTime) {
                    activeVFX.RemoveAt(i);
                }
            }
        }
        
        #endregion
        
        #region 屏幕特效
        
        /// <summary>
        /// 触发屏幕特效
        /// </summary>
        public void TriggerScreenEffect(ScreenEffectType type, float customIntensity = -1f) {
            if (!enableScreenEffects) return;
            
            var effect = new ScreenEffect {
                ID = type.ToString(),
                Type = type,
                Intensity = customIntensity > 0 ? customIntensity : CombatVFXDatabase.ScreenEffectIntensities[type],
                Duration = CombatVFXDatabase.ScreenEffectDurations[type],
                CurrentTime = 0,
                Color = GetScreenEffectColor(type)
            };
            
            activeScreenEffects.Add(effect);
            PlayerData.ScreenEffects++;
            
            // 应用特效
            ApplyScreenEffect(effect);
            
            EmitSignal(SignalName.ScreenEffectTriggered, type);
        }
        
        private Color GetScreenEffectColor(ScreenEffectType type) {
            switch (type) {
                case ScreenEffectType.Flash:
                    return new Color(1f, 1f, 1f);
                case ScreenEffectType.RedTint:
                    return new Color(1f, 0f, 0f);
                case ScreenEffectType.Chromatic:
                    return new Color(1f, 1f, 1f);
                default:
                    return new Color(0f, 0f, 0f);
            }
        }
        
        private void ApplyScreenEffect(ScreenEffect effect) {
            // 这里可以集成到现有的CameraEffectSystem或ScreenEffectManager
            // 简化实现：使用颜色叠加
            
            var colorRect = new ColorRect();
            colorRect.Color = effect.Color;
            colorRect.Color.A = effect.Intensity;
            colorRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            colorRect.MouseFilter = Control.MouseFilterEnum.Ignore;
            
            effectsContainer.AddChild(colorRect);
            
            // 动画
            var tween = CreateTween();
            tween.TweenProperty(colorRect, "color:a", 0f, effect.Duration);
            tween.TweenCallback(() => {
                if (IsInstanceValid(colorRect)) {
                    colorRect.QueueFree();
                }
            });
            
            // 屏幕震动
            if (effect.Type == ScreenEffectType.Shake && mainCamera != null) {
                var shakeTween = CreateTween();
                Vector3 originalPos = mainCamera.Position;
                for (int i = 0; i < 5; i++) {
                    shakeTween.TweenProperty(mainCamera, "position", 
                        originalPos + new Vector3(
                            GD.Randf() * effect.Intensity - effect.Intensity / 2,
                            GD.Randf() * effect.Intensity - effect.Intensity / 2,
                            0
                        ), 0.04f);
                }
                shakeTween.TweenProperty(mainCamera, "position", originalPos, 0.04f);
            }
            
            // 慢动作
            if (effect.Type == ScreenEffectType.SlowMo) {
                Engine.TimeScale = effect.Intensity;
                GetTree().CreateTimer(effect.Duration).Timeout += () => {
                    Engine.TimeScale = 1f;
                };
            }
        }
        
        private void UpdateScreenEffects(float dt) {
            for (int i = activeScreenEffects.Count - 1; i >= 0; i--) {
                var effect = activeScreenEffects[i];
                effect.CurrentTime += dt;
                
                if (effect.CurrentTime >= effect.Duration) {
                    activeScreenEffects.RemoveAt(i);
                }
            }
        }
        
        #endregion
        
        #region 连击特效
        
        /// <summary>
        /// 显示连击特效
        /// </summary>
        public void ShowComboEffect(int comboCount, Vector3 worldPosition) {
            if (!enableComboEffects) return;
            if (comboCount < 2) return;
            
            var effect = new ComboEffect {
                ComboCount = comboCount,
                Position = worldPosition,
                LifeTime = 1f,
                CurrentTime = 0
            };
            
            activeComboEffects.Add(effect);
            
            if (comboCount > PlayerData.MaxCombo) {
                PlayerData.MaxCombo = comboCount;
            }
            
            // 检查连击里程碑
            CheckComboMilestone(comboCount);
            
            // 创建连击显示
            CreateComboUI(effect);
        }
        
        private void CheckComboMilestone(int comboCount) {
            foreach (var milestone in CombatVFXDatabase.ComboMilestones) {
                if (comboCount == milestone.Key) {
                    EmitSignal(SignalName.ComboMilestoneReached, comboCount, milestone.Value);
                    break;
                }
            }
        }
        
        private void CreateComboUI(ComboEffect effect) {
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
            
            effectsContainer.AddChild(label);
            
            // 动画
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
                }
            });
        }
        
        private void UpdateComboEffects(float dt) {
            for (int i = activeComboEffects.Count - 1; i >= 0; i--) {
                var effect = activeComboEffects[i];
                effect.CurrentTime += dt;
                
                if (effect.CurrentTime >= effect.LifeTime) {
                    activeComboEffects.RemoveAt(i);
                }
            }
        }
        
        #endregion
        
        #region 暴击光效
        
        /// <summary>
        /// 显示暴击光效
        /// </summary>
        public void ShowCriticalGlow(Node3D target) {
            if (!enableCriticalGlow || target == null) return;
            
            var glow = new CriticalGlow {
                Target = target,
                GlowColor = CombatVFXDatabase.GetCriticalGlowColor(),
                Intensity = CombatVFXDatabase.GetCriticalGlowIntensity(),
                Duration = CombatVFXDatabase.GetCriticalGlowDuration(),
                CurrentTime = 0
            };
            
            activeCriticalGlows.Add(glow);
            
            // 创建光效（简单的发光覆盖）
            CreateCriticalGlowVisual(glow);
        }
        
        private void CreateCriticalGlowVisual(CriticalGlow glow) {
            if (glow.Target == null || !IsInstanceValid(glow.Target)) return;
            
            // 创建一个发光效果的Mesh
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
            
            // 临时添加到场景
            var tempParent = glow.Target.GetParent();
            if (tempParent != null) {
                tempParent.AddChild(meshInstance);
            }
            
            // 动画
            var tween = CreateTween();
            tween.SetParallel(true);
            
            // 脉动效果
            tween.TweenProperty(meshInstance, "scale", new Vector3(2f, 2f, 2f), glow.Duration * 0.5f);
            tween.TweenProperty(meshInstance, "scale", new Vector3(1.5f, 1.5f, 1.5f), glow.Duration * 0.5f).SetDelay(glow.Duration * 0.5f);
            
            // 淡出
            tween.TweenProperty(material, "albedo_color:a", 0f, glow.Duration);
            
            // 跟随目标
            tween.TweenCallback(() => {
                if (IsInstanceValid(meshInstance)) {
                    meshInstance.QueueFree();
                }
            });
            
            // 更新跟随
            _ = FollowTarget(glow.Target, meshInstance, glow.Duration);
        }
        
        private async System.Threading.Tasks.Task FollowTarget(Node3D target, Node3D follower, float duration) {
            float elapsed = 0;
            while (elapsed < duration && IsInstanceValid(target) && IsInstanceValid(follower)) {
                follower.Position = target.Position;
                elapsed += 0.016f;
                await System.Threading.Tasks.Task.Delay(16);
            }
        }
        
        private void UpdateCriticalGlows(float dt) {
            for (int i = activeCriticalGlows.Count - 1; i >= 0; i--) {
                var glow = activeCriticalGlows[i];
                glow.CurrentTime += dt;
                
                if (glow.CurrentTime >= glow.Duration) {
                    activeCriticalGlows.RemoveAt(i);
                }
            }
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 世界坐标转屏幕坐标
        /// </summary>
        private Vector2 WorldToScreen(Vector3 worldPos) {
            if (mainCamera == null) return Vector2.Zero;
            
            var screenPos = mainCamera.UnprojectPosition(worldPos);
            return new Vector2(screenPos.x, screenPos.y);
        }
        
        /// <summary>
        /// 便捷方法：显示伤害
        /// </summary>
        public void ShowDamage(float damage, Vector3 worldPosition, bool isCritical, bool isEnemy = false) {
            var type = isCritical ? DamageNumberType.Critical : DamageNumberType.Normal;
            ShowDamageNumber(damage, worldPosition, type, isEnemy);
            
            if (isCritical) {
                TriggerScreenEffect(ScreenEffectType.Chromatic);
                if (player != null) {
                    ShowCriticalGlow(player);
                }
            }
        }
        
        /// <summary>
        /// 便捷方法：显示治疗
        /// </summary>
        public void ShowHeal(float amount, Vector3 worldPosition) {
            ShowDamageNumber(amount, worldPosition, DamageNumberType.Heal);
            PlayerData.Heals++;
            PlayVFX(VFXType.Heal, worldPosition);
        }
        
        /// <summary>
        /// 便捷方法：显示格挡
        /// </summary>
        public void ShowBlock(Vector3 worldPosition) {
            ShowDamageNumber(0, worldPosition, DamageNumberType.Block);
            PlayerData.Blocks++;
            PlayVFX(VFXType.Block, worldPosition);
            TriggerScreenEffect(ScreenEffectType.Flash, 0.2f);
        }
        
        /// <summary>
        /// 便捷方法：显示闪避
        /// </summary>
        public void ShowDodge(Vector3 worldPosition) {
            ShowDamageNumber(0, worldPosition, DamageNumberType.Dodge);
            PlayerData.Dodges++;
            PlayVFX(VFXType.Dodge, worldPosition);
        }
        
        /// <summary>
        /// 便捷方法：显示Miss
        /// </summary>
        public void ShowMiss(Vector3 worldPosition) {
            ShowDamageNumber(0, worldPosition, DamageNumberType.Miss);
        }
        
        /// <summary>
        /// 便捷方法：显示吸收
        /// </summary>
        public void ShowAbsorb(float amount, Vector3 worldPosition) {
            ShowDamageNumber(amount, worldPosition, DamageNumberType.Absorb);
        }
        
        /// <summary>
        /// 便捷方法：显示反射
        /// </summary>
        public void ShowReflect(float amount, Vector3 worldPosition) {
            ShowDamageNumber(amount, worldPosition, DamageNumberType.Reflect);
        }
        
        /// <summary>
        /// 获取统计数据
        /// </summary>
        public Dictionary<string, int> GetStatistics() {
            return new Dictionary<string, int> {
                { "TotalDamageNumbers", PlayerData.TotalDamageNumbers },
                { "CriticalHits", PlayerData.CriticalHits },
                { "Heals", PlayerData.Heals },
                { "Blocks", PlayerData.Blocks },
                { "Dodges", PlayerData.Dodges },
                { "MaxCombo", PlayerData.MaxCombo },
                { "ScreenEffects", PlayerData.ScreenEffects },
                { "VFXPlayed", PlayerData.VFXPlayed }
            };
        }
        
        #endregion
        
        #region 保存/加载
        
        public override Dictionary ExportSaveData() {
            return new Dictionary {
                { "player_data", new Dictionary<string, int> {
                    { "total_damage_numbers", PlayerData.TotalDamageNumbers },
                    { "critical_hits", PlayerData.CriticalHits },
                    { "heals", PlayerData.Heals },
                    { "blocks", PlayerData.Blocks },
                    { "dodges", PlayerData.Dodges },
                    { "max_combo", PlayerData.MaxCombo },
                    { "screen_effects", PlayerData.ScreenEffects },
                    { "vfx_played", PlayerData.VFXPlayed }
                }}
            };
        }
        
        public override void ImportSaveData(Dictionary data) {
            if (data == null || !data.ContainsKey("player_data")) return;
            
            var playerData = (Dictionary)data["player_data"];
            PlayerData.TotalDamageNumbers = Convert.ToInt32(playerData.GetValueOrDefault("total_damage_numbers", 0));
            PlayerData.CriticalHits = Convert.ToInt32(playerData.GetValueOrDefault("critical_hits", 0));
            PlayerData.Heals = Convert.ToInt32(playerData.GetValueOrDefault("heals", 0));
            PlayerData.Blocks = Convert.ToInt32(playerData.GetValueOrDefault("blocks", 0));
            PlayerData.Dodges = Convert.ToInt32(playerData.GetValueOrDefault("dodges", 0));
            PlayerData.MaxCombo = Convert.ToInt32(playerData.GetValueOrDefault("max_combo", 0));
            PlayerData.ScreenEffects = Convert.ToInt32(playerData.GetValueOrDefault("screen_effects", 0));
            PlayerData.VFXPlayed = Convert.ToInt32(playerData.GetValueOrDefault("vfx_played", 0));
        }
        
        #endregion
    }
}
