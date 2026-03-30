using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Combat {
    /// <summary>
    /// 战斗视觉特效系统 - 协调者
    /// 负责协调 VFXLibrary、VFXPlayer、VFXPoolingSystem 三个子系统
    /// 保持向后兼容，保留原有 public API
    /// </summary>
    public partial class CombatVFXSystem : BaseSystem {
        public static CombatVFXSystem Instance { get; private set; }
        
        // 子系统引用
        private VFXLibrary _library;
        private VFXPlayer _player;
        private VFXPoolingSystem _pool;
        
        // 数据
        public PlayerCombatVFXData PlayerData { get; private set; } = new PlayerCombatVFXData();
        
        // 引用
        private Camera3D mainCamera;
        private Player player;
        
        // 配置
        [Export] private int maxDamageNumbers = 50;
        [Export] private float damageNumberLifetime = 1.5f;
        [Export] private bool enableScreenEffects = true;
        [Export] private bool enableCriticalGlow = true;
        [Export] private bool enableComboEffects = true;
        
        // 信号
        public delegate void DamageNumberCreatedEventHandler(DamageNumberType type, float value);
        public delegate void VFXPlayedEventHandler(VFXType type, Vector3 position);
        public delegate void ScreenEffectTriggeredEventHandler(ScreenEffectType type);
        public delegate void ComboMilestoneReachedEventHandler(int comboCount, string milestone);
        
        protected override void Initialize() {
            Instance = this;
            
            // 初始化子系统
            InitializeSubsystems();
            
            // 获取引用
            GetReferences();
            
            base.Initialize();
            GD.Print("[CombatVFXSystem] Coordinated system initialized");
        }
        
        private void InitializeSubsystems() {
            // 获取或创建子系统
            _library = VFXLibrary.Instance;
            _player = VFXPlayer.Instance;
            _pool = VFXPoolingSystem.Instance;
            
            // 如果子系统尚未初始化，在这里确保它们存在
            if (_library == null) {
                _library = new VFXLibrary();
                AddChild(_library);
            }
            
            if (_player == null) {
                _player = new VFXPlayer();
                AddChild(_player);
            }
            
            if (_pool == null) {
                _pool = new VFXPoolingSystem();
                AddChild(_pool);
            }
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
            
            // 通过池系统更新活跃实例
            UpdateThroughPools(dt);
        }
        
        /// <summary>
        /// 通过池系统更新所有活跃实例
        /// </summary>
        private void UpdateThroughPools(float dt) {
            if (_pool != null) {
                _pool.UpdateDamageNumbers(dt);
                _pool.UpdateVFX(dt);
                _pool.UpdateScreenEffects(dt);
                _pool.UpdateComboEffects(dt);
                _pool.UpdateCriticalGlows(dt);
            }
        }
        
        #region 伤害数字
        
        /// <summary>
        /// 显示伤害数字
        /// </summary>
        public void ShowDamageNumber(float damage, Vector3 worldPosition, DamageNumberType type, bool isEnemy = false) {
            // 检查数量限制
            if (_pool != null && _pool.activeDamageNumbers?.Count >= maxDamageNumbers) {
                // 池系统会处理
            }
            
            // 使用库创建实例
            var damageNumber = _library?.CreateDamageNumber(damage, worldPosition, type, isEnemy);
            if (damageNumber == null) {
                damageNumber = new DamageNumber {
                    Value = damage,
                    Type = type,
                    Position = worldPosition,
                    Velocity = CombatVFXDatabase.DamageNumberVelocities[type],
                    LifeTime = damageNumberLifetime,
                    CurrentTime = 0,
                    IsEnemy = isEnemy
                };
            }
            
            // 统计
            PlayerData.TotalDamageNumbers++;
            
            if (type == DamageNumberType.Critical) {
                PlayerData.CriticalHits++;
            }
            
            // 使用播放系统显示
            _player?.PlayDamageNumber(damageNumber);
            
            EmitSignal(SignalName.DamageNumberCreated, type, damage);
        }
        
        #endregion
        
        #region 特效
        
        /// <summary>
        /// 播放特效
        /// </summary>
        public void PlayVFX(VFXType type, Vector3 worldPosition, Node3D target = null) {
            // 使用库创建实例
            var vfx = _library?.CreateVFXInstance(type, worldPosition, target);
            if (vfx == null) {
                var config = CombatVFXDatabase.VFXConfigs[type];
                vfx = new VFXInstance {
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
            }
            
            // 统计
            PlayerData.VFXPlayed++;
            
            // 使用播放系统播放
            _player?.PlayVFX(vfx);
            
            EmitSignal(SignalName.VFXPlayed, type, worldPosition);
        }
        
        #endregion
        
        #region 屏幕特效
        
        /// <summary>
        /// 触发屏幕特效
        /// </summary>
        public void TriggerScreenEffect(ScreenEffectType type, float customIntensity = -1f) {
            if (!enableScreenEffects) return;
            
            // 使用库创建实例
            var effect = _library?.CreateScreenEffect(type, customIntensity);
            if (effect == null) {
                effect = new ScreenEffect {
                    ID = type.ToString(),
                    Type = type,
                    Intensity = customIntensity > 0 ? customIntensity : CombatVFXDatabase.ScreenEffectIntensities[type],
                    Duration = CombatVFXDatabase.ScreenEffectDurations[type],
                    CurrentTime = 0,
                    Color = GetScreenEffectColor(type)
                };
            }
            
            // 统计
            PlayerData.ScreenEffects++;
            
            // 使用播放系统播放
            _player?.PlayScreenEffect(effect);
            
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
        
        #endregion
        
        #region 连击特效
        
        /// <summary>
        /// 显示连击特效
        /// </summary>
        public void ShowComboEffect(int comboCount, Vector3 worldPosition) {
            if (!enableComboEffects) return;
            if (comboCount < 2) return;
            
            // 使用库创建实例
            var effect = _library?.CreateComboEffect(comboCount, worldPosition);
            if (effect == null) {
                effect = new ComboEffect {
                    ComboCount = comboCount,
                    Position = worldPosition,
                    LifeTime = 1f,
                    CurrentTime = 0
                };
            }
            
            // 统计
            if (comboCount > PlayerData.MaxCombo) {
                PlayerData.MaxCombo = comboCount;
            }
            
            // 检查连击里程碑
            CheckComboMilestone(comboCount);
            
            // 使用播放系统播放
            _player?.PlayComboEffect(effect);
        }
        
        private void CheckComboMilestone(int comboCount) {
            foreach (var milestone in CombatVFXDatabase.ComboMilestones) {
                if (comboCount == milestone.Key) {
                    EmitSignal(SignalName.ComboMilestoneReached, comboCount, milestone.Value);
                    break;
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
            
            // 使用库创建实例
            var glow = _library?.CreateCriticalGlow(target);
            if (glow == null) {
                glow = new CriticalGlow {
                    Target = target,
                    GlowColor = CombatVFXDatabase.GetCriticalGlowColor(),
                    Intensity = CombatVFXDatabase.GetCriticalGlowIntensity(),
                    Duration = CombatVFXDatabase.GetCriticalGlowDuration(),
                    CurrentTime = 0
                };
            }
            
            // 使用播放系统播放
            _player?.PlayCriticalGlow(glow);
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 获取主相机（兼容旧API）
        /// </summary>
        public Camera3D GetMainCamera() {
            if (mainCamera != null) return mainCamera;
            return _player?.GetMainCamera();
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
            var stats = new Dictionary<string, int> {
                { "TotalDamageNumbers", PlayerData.TotalDamageNumbers },
                { "CriticalHits", PlayerData.CriticalHits },
                { "Heals", PlayerData.Heals },
                { "Blocks", PlayerData.Blocks },
                { "Dodges", PlayerData.Dodges },
                { "MaxCombo", PlayerData.MaxCombo },
                { "ScreenEffects", PlayerData.ScreenEffects },
                { "VFXPlayed", PlayerData.VFXPlayed }
            };
            
            // 添加池统计
            if (_pool != null) {
                var poolStats = _pool.GetStatistics();
                foreach (var kvp in poolStats) {
                    stats[$"Pool_{kvp.Key}"] = kvp.Value;
                }
            }
            
            return stats;
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
