using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Alchemy
{
    /// <summary>
    /// 炼金效果 - 处理药水效果的施加和计算
    /// </summary>
    public partial class AlchemyEffects : BaseSystem
    {
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
        
        private Dictionary<string, List<ActiveEffect>> _activeEffects = new Dictionary<string, List<ActiveEffect>>();
        
        public override void _Ready()
        {
            base._Ready();
        }
        
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
        }
        
        /// <summary>
        /// 移除效果
        /// </summary>
        public void RemoveEffect(string targetId, EffectType type)
        {
            if (!_activeEffects.ContainsKey(targetId))
                return;
            
            _activeEffects[targetId].RemoveAll(e => e.Type == type);
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
        /// 处理生命恢复
        /// </summary>
        private void ApplyHealthRestore(string targetId, float value)
        {
            GD.Print($"[AlchemyEffects] 恢复目标 {targetId} 生命值 {value}");
            // 可以通过事件系统通知其他系统
        }
        
        /// <summary>
        /// 处理法力恢复
        /// </summary>
        private void ApplyManaRestore(string targetId, float value)
        {
            GD.Print($"[AlchemyEffects] 恢复目标 {targetId} 法力值 {value}");
        }
        
        /// <summary>
        /// 处理净化效果
        /// </summary>
        private void ApplyCleansing(string targetId)
        {
            GD.Print($"[AlchemyEffects] 清除目标 {targetId} 的负面状态");
            ClearAllEffects(targetId);
        }
        
        /// <summary>
        /// 处理周期效果
        /// </summary>
        public override void _Process(double delta)
        {
            float deltaF = (float)delta;
            
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
            }
            
            // 清理空列表
            var emptyTargets = new List<string>();
            foreach (var kvp in _activeEffects)
            {
                if (kvp.Value.Count == 0)
                {
                    emptyTargets.Add(kvp.Key);
                }
            }
            
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
            GD.Print($"[AlchemyEffects] 效果 {effect.Type} 在目标 {effect.TargetId} 上结束");
        }
        
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
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            // 加载数据
        }
    }
}
