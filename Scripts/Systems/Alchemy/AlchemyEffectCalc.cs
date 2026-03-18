using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Alchemy
{
    /// <summary>
    /// 炼金效果计算器 - 处理药水效果的计算和应用
    /// 基于 AlchemyEffects 扩展计算功能
    /// </summary>
    public partial class AlchemyEffectCalc : BaseSystem
    {
        private static AlchemyEffectCalc _instance;
        public static AlchemyEffectCalc Instance => _instance;
        
        // 活跃效果存储
        private Dictionary<string, List<ActiveEffect>> _activeEffects = new Dictionary<string, List<ActiveEffect>>();
        
        // 效果配置
        private float _tickRate = 1.0f;
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
        }
        
        protected override string SystemName => "AlchemyEffectCalc";
        
        #region Effect Types
        
        /// <summary>
        /// 效果类型
        /// </summary>
        public enum EffectType
        {
            HealthRestore,     // 生命恢复
            ManaRestore,       // 法力恢复
            HealthRegen,       // 生命持续恢复
            ManaRegen,         // 法力持续恢复
            AttackBoost,       // 攻击提升
            DefenseBoost,      // 防御提升
            SpeedBoost,        // 速度提升
            CritRateBoost,     // 暴击率提升
            CritDamageBoost,   // 暴击伤害提升
            Invisible,         // 隐形
            Cleansing,         // 清除负面状态
            Invulnerable      // 无敌
        }
        
        /// <summary>
        /// 药水效果
        /// </summary>
        public class PotionEffect
        {
            public EffectType Type { get; set; }
            public float Value { get; set; }
            public float Duration { get; set; }  // 持续时间（秒），0表示即时
        }
        
        /// <summary>
        /// 活跃效果
        /// </summary>
        public class ActiveEffect
        {
            public string TargetId { get; set; }
            public EffectType Type { get; set; }
            public float Value { get; set; }
            public float RemainingTime { get; set; }
            public float TickInterval { get; set; }  // 周期效果间隔
            public float TimeSinceLastTick { get; set; }
        }
        
        #endregion
        
        #region Effect Application
        
        /// <summary>
        /// 应用即时效果
        /// </summary>
        public void ApplyInstantEffect(string targetId, EffectType type, float value)
        {
            switch (type)
            {
                case EffectType.HealthRestore:
                    ApplyHealthRestore(targetId, value);
                    break;
                case EffectType.ManaRestore:
                    ApplyManaRestore(targetId, value);
                    break;
                case EffectType.Cleansing:
                    ApplyCleansing(targetId);
                    break;
            }
        }
        
        /// <summary>
        /// 应用持续效果
        /// </summary>
        public void ApplyDurationEffect(string targetId, EffectType type, float value, float duration, float tickInterval = 0)
        {
            if (!_activeEffects.ContainsKey(targetId))
            {
                _activeEffects[targetId] = new List<ActiveEffect>();
            }
            
            var effect = new ActiveEffect
            {
                TargetId = targetId,
                Type = type,
                Value = value,
                RemainingTime = duration,
                TickInterval = tickInterval > 0 ? tickInterval : duration,
                TimeSinceLastTick = 0
            };
            
            _activeEffects[targetId].Add(effect);
            GD.Print($"[AlchemyEffectCalc] Applied duration effect {type} to {targetId} for {duration}s");
        }
        
        /// <summary>
        /// 移除效果
        /// </summary>
        public void RemoveEffect(string targetId, EffectType type)
        {
            if (!_activeEffects.ContainsKey(targetId))
                return;
            
            _activeEffects[targetId].RemoveAll(e => e.Type == type);
            GD.Print($"[AlchemyEffectCalc] Removed effect {type} from {targetId}");
        }
        
        /// <summary>
        /// 清除所有效果
        /// </summary>
        public void ClearAllEffects(string targetId)
        {
            if (_activeEffects.ContainsKey(targetId))
            {
                _activeEffects[targetId].Clear();
            }
        }
        
        /// <summary>
        /// 获取活跃效果
        /// </summary>
        public List<ActiveEffect> GetActiveEffects(string targetId)
        {
            if (!_activeEffects.ContainsKey(targetId))
                return new List<ActiveEffect>();
            
            return _activeEffects[targetId];
        }
        
        /// <summary>
        /// 是否有特定类型的效果
        /// </summary>
        public bool HasEffect(string targetId, EffectType type)
        {
            if (!_activeEffects.ContainsKey(targetId))
                return false;
            
            return _activeEffects[targetId].Exists(e => e.Type == type);
        }
        
        /// <summary>
        /// 获取活跃效果数量
        /// </summary>
        public int GetActiveEffectCount(string targetId)
        {
            if (!_activeEffects.ContainsKey(targetId))
                return 0;
            return _activeEffects[targetId].Count;
        }
        
        #endregion
        
        #region Effect Processing
        
        /// <summary>
        /// 处理生命恢复
        /// </summary>
        private void ApplyHealthRestore(string targetId, float value)
        {
            GD.Print($"[AlchemyEffectCalc] 恢复目标 {targetId} 生命值 {value}");
            // 可以通过事件系统通知其他系统
        }
        
        /// <summary>
        /// 处理法力恢复
        /// </summary>
        private void ApplyManaRestore(string targetId, float value)
        {
            GD.Print($"[AlchemyEffectCalc] 恢复目标 {targetId} 法力值 {value}");
        }
        
        /// <summary>
        /// 处理净化效果
        /// </summary>
        private void ApplyCleansing(string targetId)
        {
            GD.Print($"[AlchemyEffectCalc] 清除目标 {targetId} 的负面状态");
            ClearAllEffects(targetId);
        }
        
        /// <summary>
        /// 处理周期效果
        /// </summary>
        public override void _Process(double delta)
        {
            float deltaF = (float)delta;
            
            List<string> emptyTargets = new List<string>();
            
            foreach (var targetEffects in _activeEffects)
            {
                for (int i = targetEffects.Value.Count - 1; i >= 0; i--)
                {
                    var effect = targetEffects.Value[i];
                    
                    // 更新剩余时间
                    effect.RemainingTime -= deltaF;
                    effect.TimeSinceLastTick += deltaF;
                    
                    // 处理周期效果
                    if (effect.TimeSinceLastTick >= effect.TickInterval)
                    {
                        ApplyTickEffect(effect);
                        effect.TimeSinceLastTick = 0;
                    }
                    
                    // 效果结束
                    if (effect.RemainingTime <= 0)
                    {
                        OnEffectExpired(effect);
                        targetEffects.Value.RemoveAt(i);
                    }
                }
                
                // 检查是否需要清理
                if (targetEffects.Value.Count == 0)
                {
                    emptyTargets.Add(targetEffects.Key);
                }
            }
            
            // 清理空列表
            foreach (var target in emptyTargets)
            {
                _activeEffects.Remove(target);
            }
        }
        
        /// <summary>
        /// 应用周期效果
        /// </summary>
        private void ApplyTickEffect(ActiveEffect effect)
        {
            switch (effect.Type)
            {
                case EffectType.HealthRegen:
                    ApplyHealthRestore(effect.TargetId, effect.Value);
                    break;
                case EffectType.ManaRegen:
                    ApplyManaRestore(effect.TargetId, effect.Value);
                    break;
            }
        }
        
        /// <summary>
        /// 效果结束时调用
        /// </summary>
        private void OnEffectExpired(ActiveEffect effect)
        {
            GD.Print($"[AlchemyEffectCalc] 效果 {effect.Type} 在目标 {effect.TargetId} 上结束");
        }
        
        #endregion
        
        #region Item Effects
        
        /// <summary>
        /// 根据物品ID获取效果列表
        /// </summary>
        public List<PotionEffect> GetEffectsFromItemId(int itemId)
        {
            var effects = new List<PotionEffect>();
            
            // 根据物品ID映射效果
            switch (itemId)
            {
                case 501: // 体力药水
                    effects.Add(new PotionEffect { Type = EffectType.HealthRestore, Value = 50 });
                    break;
                case 511: // 法力药水
                    effects.Add(new PotionEffect { Type = EffectType.ManaRestore, Value = 30 });
                    break;
                case 531: // 力量药水
                    effects.Add(new PotionEffect { Type = EffectType.AttackBoost, Value = 10, Duration = 60 });
                    break;
                case 541: // 敏捷药水
                    effects.Add(new PotionEffect { Type = EffectType.SpeedBoost, Value = 15, Duration = 60 });
                    break;
                case 551: // 防御药水
                    effects.Add(new PotionEffect { Type = EffectType.DefenseBoost, Value = 10, Duration = 60 });
                    break;
                case 561: // 暴击药水
                    effects.Add(new PotionEffect { Type = EffectType.CritRateBoost, Value = 5, Duration = 60 });
                    break;
                case 571: // 生命再生药水
                    effects.Add(new PotionEffect { Type = EffectType.HealthRegen, Value = 5, Duration = 30, TickInterval = 1 });
                    break;
                case 572: // 法力再生药水
                    effects.Add(new PotionEffect { Type = EffectType.ManaRegen, Value = 3, Duration = 30, TickInterval = 1 });
                    break;
                case 581: // 解毒药水
                    effects.Add(new PotionEffect { Type = EffectType.Cleansing, Value = 0 });
                    break;
                case 591: // 隐形药水
                    effects.Add(new PotionEffect { Type = EffectType.Invisible, Value = 1, Duration = 30 });
                    break;
            }
            
            return effects;
        }
        
        /// <summary>
        /// 使用物品并应用效果
        /// </summary>
        public void UseItem(int itemId, string targetId)
        {
            var effects = GetEffectsFromItemId(itemId);
            foreach (var effect in effects)
            {
                if (effect.Duration > 0)
                {
                    ApplyDurationEffect(targetId, effect.Type, effect.Value, effect.Duration);
                }
                else
                {
                    ApplyInstantEffect(targetId, effect.Type, effect.Value);
                }
            }
        }
        
        /// <summary>
        /// 计算效果对属性的修改值
        /// </summary>
        public float CalculateEffectValue(EffectType type, float baseValue)
        {
            return type switch
            {
                EffectType.AttackBoost => baseValue * 1.1f,
                EffectType.DefenseBoost => baseValue * 1.1f,
                EffectType.SpeedBoost => baseValue * 1.15f,
                EffectType.CritRateBoost => baseValue + 5f,
                EffectType.CritDamageBoost => baseValue * 1.25f,
                _ => baseValue
            };
        }
        
        #endregion
        
        #region Effect Buff/Debuff Helpers
        
        /// <summary>
        /// 检查是否是增益效果
        /// </summary>
        public bool IsBuffEffect(EffectType type)
        {
            return type switch
            {
                EffectType.HealthRestore or EffectType.ManaRestore or 
                EffectType.HealthRegen or EffectType.ManaRegen or
                EffectType.AttackBoost or EffectType.DefenseBoost or
                EffectType.SpeedBoost or EffectType.CritRateBoost or
                EffectType.CritDamageBoost or EffectType.Invisible or
                EffectType.Invulnerable => true,
                _ => false
            };
        }
        
        /// <summary>
        /// 检查是否是debuff效果
        /// </summary>
        public bool IsDebuffEffect(EffectType type)
        {
            return type == EffectType.Cleansing;
        }
        
        #endregion
        
        #region Persistence
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            // 加载数据
        }
        
        #endregion
    }
}
