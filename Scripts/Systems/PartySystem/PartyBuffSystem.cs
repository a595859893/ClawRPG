using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 队伍Buff系统 - 负责队伍buff、协同效果、增益管理等
    /// </summary>
    public partial class PartyBuffSystem : BaseSystem
    {
        private static PartyBuffSystem _instance;
        public static PartyBuffSystem Instance => _instance;
        
        // 队伍Buff存储 (stat bonuses)
        private Dictionary<string, List<PartyBuff>> _partyBuffs = new Dictionary<string, List<PartyBuff>>();
        
        // 资源型Buff存储 (LuckBoost, ExpBoost, etc.) - 兼容 PartyLootSystem
        private Dictionary<PartyData.PartyBuffType, float> _resourceBuffs = new Dictionary<PartyData.PartyBuffType, float>();
        
        // 协同效果存储
        private Dictionary<string, SynergyEffect> _synergyEffects = new Dictionary<string, SynergyEffect>();
        
        // Buff ID 计数器
        private int _nextBuffId = 1;
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
        }
        
        protected override string SystemName => "PartyBuff";
        
        #region Buff Management
        
        /// <summary>
        /// 添加队伍Buff
        /// </summary>
        public int AddBuff(string partyId, PartyBuff buff)
        {
            var buffId = _nextBuffId++;
            buff.Id = buffId;
            
            if (!_partyBuffs.ContainsKey(partyId))
            {
                _partyBuffs[partyId] = new List<PartyBuff>();
            }
            
            _partyBuffs[partyId].Add(buff);
            return buffId;
        }
        
        /// <summary>
        /// 移除队伍Buff
        /// </summary>
        public bool RemoveBuff(string partyId, int buffId)
        {
            if (!_partyBuffs.ContainsKey(partyId))
                return false;
            
            var buff = _partyBuffs[partyId].FirstOrDefault(b => b.Id == buffId);
            if (buff == null)
                return false;
            
            _partyBuffs[partyId].Remove(buff);
            return true;
        }
        
        /// <summary>
        /// 获取队伍所有Buff
        /// </summary>
        public List<PartyBuff> GetBuffs(string partyId)
        {
            return _partyBuffs.ContainsKey(partyId) ? new List<PartyBuff>(_partyBuffs[partyId]) : new List<PartyBuff>();
        }
        
        /// <summary>
        /// 更新Buff (每帧调用)
        /// </summary>
        public void UpdateBuffs(string partyId, float delta)
        {
            if (!_partyBuffs.ContainsKey(partyId))
                return;
            
            var expiredBuffs = new List<PartyBuff>();
            
            foreach (var buff in _partyBuffs[partyId])
            {
                if (buff.IsPermanent)
                    continue;
                
                buff.RemainingTime -= delta;
                if (buff.RemainingTime <= 0)
                {
                    expiredBuffs.Add(buff);
                }
            }
            
            // Remove expired buffs
            foreach (var buff in expiredBuffs)
            {
                _partyBuffs[partyId].Remove(buff);
            }
        }
        
        /// <summary>
        /// 清除队伍所有Buff
        /// </summary>
        public void ClearBuffs(string partyId)
        {
            if (_partyBuffs.ContainsKey(partyId))
            {
                _partyBuffs[partyId].Clear();
            }
        }
        
        #endregion
        
        #region Resource Buffs (Compatibility)
        
        /// <summary>
        /// 获取资源型Buff值 (LuckBoost, ExpBoost, GoldBoost, DropRateBoost)
        /// </summary>
        public float GetBuffValue(PartyData.PartyBuffType type)
        {
            return _resourceBuffs.ContainsKey(type) ? _resourceBuffs[type] : 0f;
        }
        
        /// <summary>
        /// 添加资源型Buff (兼容 PartyLootSystem)
        /// </summary>
        public void AddResourceBuff(PartyData.PartyBuffType type, float value, float duration, int providerId)
        {
            _resourceBuffs[type] = value;
        }
        
        /// <summary>
        /// 移除资源型Buff
        /// </summary>
        public void RemoveResourceBuff(PartyData.PartyBuffType type)
        {
            if (_resourceBuffs.ContainsKey(type))
            {
                _resourceBuffs.Remove(type);
            }
        }
        
        #endregion
        
        #region Synergy Effects
        
        /// <summary>
        /// 计算协同效果
        /// </summary>
        public SynergyEffect CalculateSynergy(string partyId, List<string> memberClasses)
        {
            var effect = new SynergyEffect();
            
            // Count class combinations
            var classCounts = memberClasses.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
            
            // Apply synergy rules
            foreach (var kvp in classCounts)
            {
                var count = kvp.Value;
                var className = kvp.Key;
                
                // Example: 3+ same class = strong bonus
                if (count >= 3)
                {
                    effect.DamageBonus += 0.15f;
                    effect.DefenseBonus += 0.10f;
                }
                // 2 same class = minor bonus
                else if (count == 2)
                {
                    effect.DamageBonus += 0.05f;
                    effect.DefenseBonus += 0.03f;
                }
            }
            
            // Store effect
            _synergyEffects[partyId] = effect;
            return effect;
        }
        
        /// <summary>
        /// 获取队伍协同效果
        /// </summary>
        public SynergyEffect GetSynergyEffect(string partyId)
        {
            return _synergyEffects.ContainsKey(partyId) ? _synergyEffects[partyId] : null;
        }
        
        #endregion
        
        #region Buff Calculations
        
        /// <summary>
        /// 计算最终属性加成
        /// </summary>
        public Dictionary<string, float> CalculateFinalStats(string partyId, Dictionary<string, float> baseStats)
        {
            var result = new Dictionary<string, float>(baseStats);
            
            if (!_partyBuffs.ContainsKey(partyId))
                return result;
            
            float damageMult = 1.0f;
            float defenseMult = 1.0f;
            float hpMult = 1.0f;
            float speedMult = 1.0f;
            
            foreach (var buff in _partyBuffs[partyId])
            {
                damageMult += buff.DamageBonus;
                defenseMult += buff.DefenseBonus;
                hpMult += buff.HpBonus;
                speedMult += buff.SpeedBonus;
            }
            
            // Apply synergy
            var synergy = GetSynergyEffect(partyId);
            if (synergy != null)
            {
                damageMult += synergy.DamageBonus;
                defenseMult += synergy.DefenseBonus;
            }
            
            result["damage"] *= damageMult;
            result["defense"] *= defenseMult;
            result["hp"] *= hpMult;
            result["speed"] *= speedMult;
            
            return result;
        }
        
        #endregion
        
        #region Persistence
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // Export party buffs
            var buffsArray = new Array();
            foreach (var kvp in _partyBuffs)
            {
                var entry = new Dictionary
                {
                    ["partyId"] = kvp.Key,
                    ["buffs"] = JsonSerializer.Serialize(kvp.Value)
                };
                buffsArray.Add(entry);
            }
            data["partyBuffs"] = buffsArray;
            
            // Export synergy effects
            var synergyArray = new Array();
            foreach (var kvp in _synergyEffects)
            {
                var entry = new Dictionary
                {
                    ["partyId"] = kvp.Key,
                    ["effect"] = JsonSerializer.Serialize(kvp.Value)
                };
                synergyArray.Add(entry);
            }
            data["synergyEffects"] = synergyArray;
            
            data["nextBuffId"] = _nextBuffId;
            
            // Export resource buffs
            var resourceBuffs = new Dictionary<string, object>();
            foreach (var kvp in _resourceBuffs)
            {
                resourceBuffs[(int)kvp.Key] = kvp.Value;
            }
            data["resourceBuffs"] = JsonSerializer.Serialize(resourceBuffs);
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            _partyBuffs.Clear();
            _synergyEffects.Clear();
            _resourceBuffs.Clear();
            
            // Import party buffs
            if (data.Contains("partyBuffs"))
            {
                var buffsArray = (Array)data["partyBuffs"];
                foreach (Dictionary entry in buffsArray)
                {
                    var partyId = entry["partyId"].ToString();
                    var buffs = JsonSerializer.Deserialize<List<PartyBuff>>(entry["buffs"].ToString());
                    if (buffs != null)
                    {
                        _partyBuffs[partyId] = buffs;
                    }
                }
            }
            
            // Import synergy effects
            if (data.Contains("synergyEffects"))
            {
                var synergyArray = (Array)data["synergyEffects"];
                foreach (Dictionary entry in synergyArray)
                {
                    var partyId = entry["partyId"].ToString();
                    var effect = JsonSerializer.Deserialize<SynergyEffect>(entry["effect"].ToString());
                    if (effect != null)
                    {
                        _synergyEffects[partyId] = effect;
                    }
                }
            }
            
            if (data.Contains("nextBuffId"))
            {
                _nextBuffId = Convert.ToInt32(data["nextBuffId"]);
            }
            
            // Import resource buffs
            if (data.Contains("resourceBuffs"))
            {
                var resourceData = JsonSerializer.Deserialize<Dictionary<int, float>>(data["resourceBuffs"].ToString());
                if (resourceData != null)
                {
                    foreach (var kvp in resourceData)
                    {
                        _resourceBuffs[(PartyData.PartyBuffType)kvp.Key] = kvp.Value;
                    }
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 队伍Buff
    /// </summary>
    public class PartyBuff
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float DamageBonus { get; set; }
        public float DefenseBonus { get; set; }
        public float HpBonus { get; set; }
        public float SpeedBonus { get; set; }
        public float RemainingTime { get; set; }
        public bool IsPermanent { get; set; }
        public string Source { get; set; }
    }
    
    /// <summary>
    /// 协同效果
    /// </summary>
    public class SynergyEffect
    {
        public float DamageBonus { get; set; }
        public float DefenseBonus { get; set; }
        public float HpBonus { get; set; }
        public float SpeedBonus { get; set; }
        public float CritBonus { get; set; }
    }
}
