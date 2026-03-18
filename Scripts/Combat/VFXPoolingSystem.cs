using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Combat {
    /// <summary>
    /// VFX Pooling System - 负责特效对象池
    /// 单一职责：管理VFX对象的复用，减少GC压力
    /// </summary>
    public partial class VFXPoolingSystem : BaseSystem {
        public static VFXPoolingSystem Instance { get; private set; }
        
        // 对象池配置
        [Export] private int initialDamageNumberPoolSize = 30;
        [Export] private int initialVFXPoolSize = 20;
        [Export] private int initialScreenEffectPoolSize = 10;
        [Export] private int initialComboEffectPoolSize = 10;
        [Export] private int initialCriticalGlowPoolSize = 5;
        [Export] private bool enablePoolAutoExpansion = true;
        [Export] private int maxPoolSize = 100;
        
        // 对象池
        private Pool<DamageNumber> damageNumberPool;
        private Pool<VFXInstance> vfxPool;
        private Pool<ScreenEffect> screenEffectPool;
        private Pool<ComboEffect> comboEffectPool;
        private Pool<CriticalGlow> criticalGlowPool;
        
        // 活跃实例追踪（用于更新）
        private List<DamageNumber> activeDamageNumbers = new List<DamageNumber>();
        private List<VFXInstance> activeVFX = new List<VFXInstance>();
        private List<ScreenEffect> activeScreenEffects = new List<ScreenEffect>();
        private List<ComboEffect> activeComboEffects = new List<ComboEffect>();
        private List<CriticalGlow> activeCriticalGlows = new List<CriticalGlow>();
        
        // 统计
        public int TotalPoolRequests { get; private set; }
        public int PoolHits { get; private set; }
        public int PoolMisses { get; private set; }
        
        public override void _Ready() {
            base._Ready();
            Instance = this;
            InitializePools();
            GD.Print("[VFXPoolingSystem] Initialized");
        }
        
        private void InitializePools() {
            damageNumberPool = new Pool<DamageNumber>(initialDamageNumberPoolSize, () => new DamageNumber());
            vfxPool = new Pool<VFXInstance>(initialVFXPoolSize, () => new VFXInstance());
            screenEffectPool = new Pool<ScreenEffect>(initialScreenEffectPoolSize, () => new ScreenEffect());
            comboEffectPool = new Pool<ComboEffect>(initialComboEffectPoolSize, () => new ComboEffect());
            criticalGlowPool = new Pool<CriticalGlow>(initialCriticalGlowPoolSize, () => new CriticalGlow());
        }
        
        #region Pool Operations
        
        /// <summary>
        /// 从池中获取伤害数字实例
        /// </summary>
        public DamageNumber GetDamageNumber() {
            TotalPoolRequests++;
            var item = damageNumberPool.Get();
            if (item != null) {
                PoolHits++;
                activeDamageNumbers.Add(item);
                return item;
            }
            
            PoolMisses++;
            if (enableAutoExpansion && damageNumberPool.TotalSize < maxPoolSize) {
                damageNumberPool.Expand(5);
                item = damageNumberPool.Get();
                if (item != null) {
                    activeDamageNumbers.Add(item);
                    return item;
                }
            }
            
            // 如果池已满，创建新实例但不加入池
            var newItem = new DamageNumber();
            activeDamageNumbers.Add(newItem);
            return newItem;
        }
        
        /// <summary>
        /// 回收伤害数字实例
        /// </summary>
        public void ReleaseDamageNumber(DamageNumber item) {
            if (item == null) return;
            activeDamageNumbers.Remove(item);
            damageNumberPool.Release(item);
        }
        
        /// <summary>
        /// 从池中获取VFX实例
        /// </summary>
        public VFXInstance GetVFXInstance() {
            TotalPoolRequests++;
            var item = vfxPool.Get();
            if (item != null) {
                PoolHits++;
                activeVFX.Add(item);
                return item;
            }
            
            PoolMisses++;
            if (enableAutoExpansion && vfxPool.TotalSize < maxPoolSize) {
                vfxPool.Expand(5);
                item = vfxPool.Get();
                if (item != null) {
                    activeVFX.Add(item);
                    return item;
                }
            }
            
            var newItem = new VFXInstance();
            activeVFX.Add(newItem);
            return newItem;
        }
        
        /// <summary>
        /// 回收VFX实例
        /// </summary>
        public void ReleaseVFXInstance(VFXInstance item) {
            if (item == null) return;
            activeVFX.Remove(item);
            vfxPool.Release(item);
        }
        
        /// <summary>
        /// 从池中获取屏幕特效实例
        /// </summary>
        public ScreenEffect GetScreenEffect() {
            TotalPoolRequests++;
            var item = screenEffectPool.Get();
            if (item != null) {
                PoolHits++;
                activeScreenEffects.Add(item);
                return item;
            }
            
            PoolMisses++;
            if (enableAutoExpansion && screenEffectPool.TotalSize < maxPoolSize) {
                screenEffectPool.Expand(2);
                item = screenEffectPool.Get();
                if (item != null) {
                    activeScreenEffects.Add(item);
                    return item;
                }
            }
            
            var newItem = new ScreenEffect();
            activeScreenEffects.Add(newItem);
            return newItem;
        }
        
        /// <summary>
        /// 回收屏幕特效实例
        /// </summary>
        public void ReleaseScreenEffect(ScreenEffect item) {
            if (item == null) return;
            activeScreenEffects.Remove(item);
            screenEffectPool.Release(item);
        }
        
        /// <summary>
        /// 从池中获取连击特效实例
        /// </summary>
        public ComboEffect GetComboEffect() {
            TotalPoolRequests++;
            var item = comboEffectPool.Get();
            if (item != null) {
                PoolHits++;
                activeComboEffects.Add(item);
                return item;
            }
            
            PoolMisses++;
            if (enableAutoExpansion && comboEffectPool.TotalSize < maxPoolSize) {
                comboEffectPool.Expand(2);
                item = comboEffectPool.Get();
                if (item != null) {
                    activeComboEffects.Add(item);
                    return item;
                }
            }
            
            var newItem = new ComboEffect();
            activeComboEffects.Add(newItem);
            return newItem;
        }
        
        /// <summary>
        /// 回收连击特效实例
        /// </summary>
        public void ReleaseComboEffect(ComboEffect item) {
            if (item == null) return;
            activeComboEffects.Remove(item);
            comboEffectPool.Release(item);
        }
        
        /// <summary>
        /// 从池中获取暴击光效实例
        /// </summary>
        public CriticalGlow GetCriticalGlow() {
            TotalPoolRequests++;
            var item = criticalGlowPool.Get();
            if (item != null) {
                PoolHits++;
                activeCriticalGlows.Add(item);
                return item;
            }
            
            PoolMisses++;
            if (enableAutoExpansion && criticalGlowPool.TotalSize < maxPoolSize) {
                criticalGlowPool.Expand(1);
                item = criticalGlowPool.Get();
                if (item != null) {
                    activeCriticalGlows.Add(item);
                    return item;
                }
            }
            
            var newItem = new CriticalGlow();
            activeCriticalGlows.Add(newItem);
            return newItem;
        }
        
        /// <summary>
        /// 回收暴击光效实例
        /// </summary>
        public void ReleaseCriticalGlow(CriticalGlow item) {
            if (item == null) return;
            activeCriticalGlows.Remove(item);
            criticalGlowPool.Release(item);
        }
        
        #endregion
        
        #region Update Management
        
        /// <summary>
        /// 更新所有活跃的伤害数字
        /// </summary>
        public void UpdateDamageNumbers(float dt, Action<DamageNumber> onExpire = null) {
            for (int i = activeDamageNumbers.Count - 1; i >= 0; i--) {
                var item = activeDamageNumbers[i];
                item.CurrentTime += dt;
                
                if (item.CurrentTime >= item.LifeTime) {
                    ReleaseDamageNumber(item);
                    onExpire?.Invoke(item);
                }
            }
        }
        
        /// <summary>
        /// 更新所有活跃的VFX
        /// </summary>
        public void UpdateVFX(float dt, Action<VFXInstance> onExpire = null) {
            for (int i = activeVFX.Count - 1; i >= 0; i--) {
                var item = activeVFX[i];
                item.CurrentTime += dt;
                
                if (item.CurrentTime >= item.LifeTime) {
                    ReleaseVFXInstance(item);
                    onExpire?.Invoke(item);
                }
            }
        }
        
        /// <summary>
        /// 更新所有活跃的屏幕特效
        /// </summary>
        public void UpdateScreenEffects(float dt, Action<ScreenEffect> onExpire = null) {
            for (int i = activeScreenEffects.Count - 1; i >= 0; i--) {
                var item = activeScreenEffects[i];
                item.CurrentTime += dt;
                
                if (item.CurrentTime >= item.Duration) {
                    ReleaseScreenEffect(item);
                    onExpire?.Invoke(item);
                }
            }
        }
        
        /// <summary>
        /// 更新所有活跃的连击特效
        /// </summary>
        public void UpdateComboEffects(float dt, Action<ComboEffect> onExpire = null) {
            for (int i = activeComboEffects.Count - 1; i >= 0; i--) {
                var item = activeComboEffects[i];
                item.CurrentTime += dt;
                
                if (item.CurrentTime >= item.LifeTime) {
                    ReleaseComboEffect(item);
                    onExpire?.Invoke(item);
                }
            }
        }
        
        /// <summary>
        /// 更新所有活跃的暴击光效
        /// </summary>
        public void UpdateCriticalGlows(float dt, Action<CriticalGlow> onExpire = null) {
            for (int i = activeCriticalGlows.Count - 1; i >= 0; i--) {
                var item = activeCriticalGlows[i];
                item.CurrentTime += dt;
                
                if (item.CurrentTime >= item.Duration) {
                    ReleaseCriticalGlow(item);
                    onExpire?.Invoke(item);
                }
            }
        }
        
        #endregion
        
        #region Statistics
        
        /// <summary>
        /// 获取池命中率
        /// </summary>
        public float GetPoolHitRate() {
            if (TotalPoolRequests == 0) return 0f;
            return (float)PoolHits / TotalPoolRequests * 100f;
        }
        
        /// <summary>
        /// 获取统计信息
        /// </summary>
        public Dictionary<string, int> GetStatistics() {
            return new Dictionary<string, int> {
                { "TotalPoolRequests", TotalPoolRequests },
                { "PoolHits", PoolHits },
                { "PoolMisses", PoolMisses },
                { "DamageNumberPoolSize", damageNumberPool.ActiveCount },
                { "VFXPoolSize", vfxPool.ActiveCount },
                { "ScreenEffectPoolSize", screenEffectPool.ActiveCount },
                { "ComboEffectPoolSize", comboEffectPool.ActiveCount },
                { "CriticalGlowPoolSize", criticalGlowPool.ActiveCount }
            };
        }
        
        /// <summary>
        /// 重置统计
        /// </summary>
        public void ResetStatistics() {
            TotalPoolRequests = 0;
            PoolHits = 0;
            PoolMisses = 0;
        }
        
        #endregion
        
        #region Save/Load
        
        public override Dictionary ExportSaveData() {
            return new Dictionary {
                { "statistics", GetStatistics() }
            };
        }
        
        public override void ImportSaveData(Dictionary data) {
            // Pool不需要持久化运行时数据
        }
        
        #endregion
        
        // 别名属性用于GDScript导出
        private bool enableAutoExpansion {
            get => enablePoolAutoExpansion;
        }
    }
    
    /// <summary>
    /// 通用对象池类
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public class Pool<T> where T : class, new() {
        private List<T> available = new List<T>();
        private List<T> used = new List<T>();
        private Func<T> factory;
        
        public int TotalSize => available.Count + used.Count;
        public int AvailableCount => available.Count;
        public int ActiveCount => used.Count;
        
        public Pool(int initialSize, Func<T> factory) {
            this.factory = factory;
            for (int i = 0; i < initialSize; i++) {
                available.Add(factory());
            }
        }
        
        /// <summary>
        /// 从池中获取对象
        /// </summary>
        public T Get() {
            T item;
            if (available.Count > 0) {
                item = available[available.Count - 1];
                available.RemoveAt(available.Count - 1);
            } else {
                item = factory();
            }
            used.Add(item);
            return item;
        }
        
        /// <summary>
        /// 回收对象到池
        /// </summary>
        public void Release(T item) {
            if (item == null) return;
            used.Remove(item);
            ResetItem(item);
            available.Add(item);
        }
        
        /// <summary>
        /// 扩展池大小
        /// </summary>
        public void Expand(int count) {
            for (int i = 0; i < count; i++) {
                available.Add(factory());
            }
        }
        
        /// <summary>
        /// 重置对象状态
        /// </summary>
        private void ResetItem(T item) {
            // 对于值类型需要特殊处理
            if (item is DamageNumber dn) {
                dn.CurrentTime = 0;
                dn.LifeTime = 0;
                dn.Value = 0;
            } else if (item is VFXInstance vfx) {
                vfx.CurrentTime = 0;
                vfx.LifeTime = 0;
                vfx.Target = null;
            } else if (item is ScreenEffect se) {
                se.CurrentTime = 0;
                se.Duration = 0;
            } else if (item is ComboEffect ce) {
                ce.CurrentTime = 0;
                ce.LifeTime = 0;
                ce.ComboCount = 0;
            } else if (item is CriticalGlow cg) {
                cg.CurrentTime = 0;
                cg.Duration = 0;
                cg.Target = null;
            }
        }
    }
}
