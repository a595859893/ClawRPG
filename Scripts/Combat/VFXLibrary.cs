using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Combat {
    /// <summary>
    /// VFX Library - 负责特效资源加载和管理
    /// 单一职责：管理VFX资源的加载、缓存和配置
    /// </summary>
    public partial class VFXLibrary : BaseSystem {
        public static VFXLibrary Instance { get; private set; }
        
        // 资源缓存
        private Dictionary<string, Mesh> cachedMeshes = new Dictionary<string, Mesh>();
        private Dictionary<string, Material> cachedMaterials = new Dictionary<string, Material>();
        private Dictionary<VFXType, VFXConfig> vfxConfigs = new Dictionary<VFXType, VFXConfig>();
        private Dictionary<DamageNumberType, DamageNumberStyle> damageStyles = new Dictionary<DamageNumberType, DamageNumberStyle>();
        
        // 预加载资源
        [Export] private bool preloadResources = true;
        [Export] private int maxCachedMeshes = 20;
        [Export] private int maxCachedMaterials = 30;
        
        public override void _Ready() {
            base._Ready();
            Instance = this;
            InitializeLibrary();
        }
        
        private void InitializeLibrary() {
            LoadVFXConfigs();
            LoadDamageNumberStyles();
            if (preloadResources) {
                PreloadCommonResources();
            }
            GD.Print("[VFXLibrary] Initialized");
        }
        
        /// <summary>
        /// 加载VFX配置
        /// </summary>
        private void LoadVFXConfigs() {
            foreach (var kvp in CombatVFXDatabase.VFXConfigs) {
                vfxConfigs[kvp.Key] = kvp.Value;
            }
        }
        
        /// <summary>
        /// 加载伤害数字样式
        /// </summary>
        private void LoadDamageNumberStyles() {
            foreach (DamageNumberType type in Enum.GetValues(typeof(DamageNumberType))) {
                damageStyles[type] = new DamageNumberStyle {
                    Color = CombatVFXDatabase.DamageNumberColors.GetValueOrDefault(type, Colors.White),
                    Size = CombatVFXDatabase.DamageNumberSizes.GetValueOrDefault(type, 24f),
                    Velocity = CombatVFXDatabase.DamageNumberVelocities.GetValueOrDefault(type, new Vector2(0, -80f))
                };
            }
        }
        
        /// <summary>
        /// 预加载常用资源
        /// </summary>
        private void PreloadCommonResources() {
            // 预创建基础Mesh
            var sphere = new SphereMesh();
            sphere.Radius = 0.3f;
            sphere.Height = 0.6f;
            cachedMeshes["sphere"] = sphere;
            
            var box = new BoxMesh();
            box.Size = new Vector3(1f, 1f, 1f);
            cachedMeshes["box"] = box;
            
            var cylinder = new CylinderMesh();
            cylinder.TopRadius = 0.3f;
            cylinder.BottomRadius = 0.3f;
            cylinder.Height = 1f;
            cachedMeshes["cylinder"] = cylinder;
            
            // 预创建基础材质
            cachedMaterials["default_emissive"] = CreateEmissiveMaterial(Colors.White);
            cachedMaterials["hit"] = CreateEmissiveMaterial(new Color(1f, 1f, 1f));
            cachedMaterials["critical"] = CreateEmissiveMaterial(new Color(1f, 0.84f, 0f));
            cachedMaterials["heal"] = CreateEmissiveMaterial(new Color(0f, 1f, 0.5f));
            cachedMaterials["block"] = CreateEmissiveMaterial(new Color(0.5f, 0.5f, 1f));
        }
        
        /// <summary>
        /// 创建发光材质
        /// </summary>
        private Material CreateEmissiveMaterial(Color color) {
            var material = new StandardMaterial3D();
            material.AlbedoColor = color;
            material.EmissionEnabled = true;
            material.Emission = color;
            material.EmissionEnergyMultiplier = 2f;
            material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            material.AlbedoColor = new Color(color.R, color.G, color.B, 0.8f);
            return material;
        }
        
        #region Public API
        
        /// <summary>
        /// 获取VFX配置
        /// </summary>
        public VFXConfig GetVFXConfig(VFXType type) {
            if (vfxConfigs.TryGetValue(type, out var config)) {
                return config;
            }
            return null;
        }
        
        /// <summary>
        /// 获取伤害数字样式
        /// </summary>
        public DamageNumberStyle GetDamageStyle(DamageNumberType type) {
            if (damageStyles.TryGetValue(type, out var style)) {
                return style;
            }
            return damageStyles[DamageNumberType.Normal];
        }
        
        /// <summary>
        /// 获取缓存的Mesh
        /// </summary>
        public Mesh GetMesh(string key) {
            if (cachedMeshes.TryGetValue(key, out var mesh)) {
                return mesh;
            }
            return null;
        }
        
        /// <summary>
        /// 获取缓存的材质
        /// </summary>
        public Material GetMaterial(string key) {
            if (cachedMaterials.TryGetValue(key, out var material)) {
                return material;
            }
            return null;
        }
        
        /// <summary>
        /// 缓存自定义Mesh
        /// </summary>
        public void CacheMesh(string key, Mesh mesh) {
            if (cachedMeshes.Count < maxCachedMeshes) {
                cachedMeshes[key] = mesh;
            }
        }
        
        /// <summary>
        /// 缓存自定义材质
        /// </summary>
        public void CacheMaterial(string key, Material material) {
            if (cachedMaterials.Count < maxCachedMaterials) {
                cachedMaterials[key] = material;
            }
        }
        
        /// <summary>
        /// 创建VFX实例数据
        /// </summary>
        public VFXInstance CreateVFXInstance(VFXType type, Vector3 position, Node3D target = null) {
            var config = GetVFXConfig(type);
            if (config == null) return null;
            
            return new VFXInstance {
                ID = config.ID,
                Type = type,
                Duration = config.Duration,
                Position = position,
                Color = config.Color,
                Scale = config.Scale,
                LifeTime = config.Lifetime,
                CurrentTime = 0,
                Target = target
            };
        }
        
        /// <summary>
        /// 创建伤害数字实例数据
        /// </summary>
        public DamageNumber CreateDamageNumber(float value, Vector3 position, DamageNumberType type, bool isEnemy = false) {
            var style = GetDamageStyle(type);
            
            return new DamageNumber {
                Value = value,
                Type = type,
                Position = position,
                Velocity = style.Velocity,
                LifeTime = 1.5f,
                CurrentTime = 0,
                IsEnemy = isEnemy
            };
        }
        
        /// <summary>
        /// 创建屏幕特效实例数据
        /// </summary>
        public ScreenEffect CreateScreenEffect(ScreenEffectType type, float customIntensity = -1f) {
            float intensity = customIntensity > 0 ? customIntensity : 
                CombatVFXDatabase.ScreenEffectIntensities.GetValueOrDefault(type, 0.5f);
            float duration = CombatVFXDatabase.ScreenEffectDurations.GetValueOrDefault(type, 0.3f);
            
            return new ScreenEffect {
                ID = type.ToString(),
                Type = type,
                Intensity = intensity,
                Duration = duration,
                CurrentTime = 0,
                Color = GetScreenEffectColor(type)
            };
        }
        
        /// <summary>
        /// 创建连击特效实例数据
        /// </summary>
        public ComboEffect CreateComboEffect(int comboCount, Vector3 position) {
            return new ComboEffect {
                ComboCount = comboCount,
                Position = position,
                LifeTime = 1f,
                CurrentTime = 0
            };
        }
        
        /// <summary>
        /// 创建暴击光效实例数据
        /// </summary>
        public CriticalGlow CreateCriticalGlow(Node3D target) {
            return new CriticalGlow {
                Target = target,
                GlowColor = CombatVFXDatabase.GetCriticalGlowColor(),
                Intensity = CombatVFXDatabase.GetCriticalGlowIntensity(),
                Duration = CombatVFXDatabase.GetCriticalGlowDuration(),
                CurrentTime = 0
            };
        }
        
        /// <summary>
        /// 获取屏幕特效颜色
        /// </summary>
        public Color GetScreenEffectColor(ScreenEffectType type) {
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
        
        /// <summary>
        /// 获取所有VFX类型
        /// </summary>
        public VFXType[] GetAllVFXTypes() {
            return (VFXType[])Enum.GetValues(typeof(VFXType));
        }
        
        /// <summary>
        /// 清理未使用的资源
        /// </summary>
        public void CleanupUnusedResources() {
            // 可以实现引用计数来清理不常用的资源
            GD.Print("[VFXLibrary] Cleanup called");
        }
        
        #endregion
        
        #region Save/Load
        
        public override Dictionary<string, object> ExportSaveData() {
            return new Dictionary {
                { "cached_meshes_count", cachedMeshes.Count },
                { "cached_materials_count", cachedMaterials.Count }
            };
        }
        
        public override void ImportSaveData(Dictionary<string, object> data) {
            // Library不需要持久化运行时缓存
        }
        
        #endregion
    }
    
    /// <summary>
    /// 伤害数字样式数据
    /// </summary>
    public class DamageNumberStyle {
        public Color Color { get; set; }
        public float Size { get; set; }
        public Vector2 Velocity { get; set; }
    }
}
