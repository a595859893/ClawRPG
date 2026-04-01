using Godot;
using System;
using System.Collections.Generic;
using Framework;

namespace ClawRPG.Scripts.Items {
    /// <summary>
    /// Equipment set manager - calculates and manages active set bonuses
    /// </summary>
    public partial class EquipmentSetManager : BaseSystem
    {
        private static EquipmentSetManager _instance;
        public static EquipmentSetManager Instance => _instance;
        
        // Track equipped items by set
        private Dictionary<int, List<int>> _equippedSets = new();
        
        // Active set bonuses
        private List<ActiveSetBonus> _activeSetBonuses = new();
        
        // Signals
        public delegate void SetBonusActivatedEvent(EquipmentSet set, SetBonusEffect bonus);
        public event SetBonusActivatedEvent OnSetBonusActivated;
        
        public delegate void SetBonusChangedEvent();
        public event SetBonusChangedEvent OnSetBonusChanged;
        
        public override void _Ready()
        {
            _instance = this;
            GD.Print("EquipmentSetManager initialized");
        }
        
        /// <summary>
        /// Update equipped items and recalculate bonuses
        /// </summary>
        public void UpdateEquippedSets(List<int> equippedEquipmentIds)
        {
            // Clear current sets
            _equippedSets.Clear();
            
            // Group equipped items by set
            foreach (int equipId in equippedEquipmentIds)
            {
                var set = EquipmentSetDatabase.Instance.GetSetByEquipmentId(equipId);
                if (set != null)
                {
                    if (!_equippedSets.ContainsKey(set.SetId))
                    {
                        _equippedSets[set.SetId] = new List<int>();
                    }
                    _equippedSets[set.SetId].Add(equipId);
                }
            }
            
            // Recalculate active bonuses
            RecalculateBonuses();
        }
        
        /// <summary>
        /// Recalculate all active set bonuses
        /// </summary>
        private void RecalculateBonuses()
        {
            var previousBonuses = new List<ActiveSetBonus>(_activeSetBonuses);
            _activeSetBonuses.Clear();
            
            foreach (var kvp in _equippedSets)
            {
                var set = EquipmentSetDatabase.Instance.GetSet(kvp.Key);
                if (set != null)
                {
                    var activeBonus = new ActiveSetBonus(set, kvp.Value.Count);
                    if (activeBonus.IsActive)
                    {
                        _activeSetBonuses.Add(activeBonus);
                    }
                }
            }
            
            // Check if bonuses changed
            if (previousBonuses.Count != _activeSetBonuses.Count)
            {
                OnSetBonusChanged?.Invoke();
                return;
            }
            
            for (int i = 0; i < Math.Min(previousBonuses.Count, _activeSetBonuses.Count); i++)
            {
                if (previousBonuses[i].Set.SetId != _activeSetBonuses[i].Set.SetId ||
                    previousBonuses[i].EquippedPieces != _activeSetBonuses[i].EquippedPieces)
                {
                    OnSetBonusChanged?.Invoke();
                    return;
                }
            }
        }
        
        /// <summary>
        /// Get total set bonus stats
        /// </summary>
        public void GetTotalSetBonusStats(out float damageBonus, out float defenseBonus, 
            out float healthBonus, out float manaBonus, out float critChance, 
            out float critDamage, out float attackSpeed, out float moveSpeed)
        {
            damageBonus = 0;
            defenseBonus = 0;
            healthBonus = 0;
            manaBonus = 0;
            critChance = 0;
            critDamage = 0;
            attackSpeed = 0;
            moveSpeed = 0;
            
            foreach (var activeSet in _activeSetBonuses)
            {
                if (activeSet.ActiveBonus == null) continue;
                
                damageBonus += activeSet.ActiveBonus.DamageBonus;
                defenseBonus += activeSet.ActiveBonus.DefenseBonus;
                healthBonus += activeSet.ActiveBonus.HealthBonus;
                manaBonus += activeSet.ActiveBonus.ManaBonus;
                critChance += activeSet.ActiveBonus.CriticalChanceBonus;
                critDamage += activeSet.ActiveBonus.CriticalDamageBonus;
                attackSpeed += activeSet.ActiveBonus.AttackSpeedBonus;
                moveSpeed += activeSet.ActiveBonus.MoveSpeedBonus;
            }
        }
        
        /// <summary>
        /// Get elemental resistances
        /// </summary>
        public void GetElementalResistances(out float fire, out float ice, 
            out float lightning, out float poison, out float dark, out float holy)
        {
            fire = 0;
            ice = 0;
            lightning = 0;
            poison = 0;
            dark = 0;
            holy = 0;
            
            foreach (var activeSet in _activeSetBonuses)
            {
                if (activeSet.ActiveBonus == null) continue;
                
                fire += activeSet.ActiveBonus.FireResistance;
                ice += activeSet.ActiveBonus.IceResistance;
                lightning += activeSet.ActiveBonus.LightningResistance;
                poison += activeSet.ActiveBonus.PoisonResistance;
                dark += activeSet.ActiveBonus.DarkResistance;
                holy += activeSet.ActiveBonus.HolyResistance;
            }
        }
        
        /// <summary>
        /// Get all active set bonuses
        /// </summary>
        public List<ActiveSetBonus> GetActiveSetBonuses()
        {
            return new List<ActiveSetBonus>(_activeSetBonuses);
        }
        
        /// <summary>
        /// Get set progress for a specific set
        /// </summary>
        public int GetSetProgress(int setId)
        {
            return _equippedSets.ContainsKey(setId) ? _equippedSets[setId].Count : 0;
        }
        
        /// <summary>
        /// Get set info for an equipment ID
        /// </summary>
        public EquipmentSet GetSetForEquipment(int equipmentId)
        {
            return EquipmentSetDatabase.Instance.GetSetByEquipmentId(equipmentId);
        }
        
        /// <summary>
        /// Check if equipment is part of a set
        /// </summary>
        public bool IsPartOfSet(int equipmentId)
        {
            return EquipmentSetDatabase.Instance.GetSetByEquipmentId(equipmentId) != null;
        }
        
        /// <summary>
        /// Get active bonus count
        /// </summary>
        public int GetActiveSetCount()
        {
            return _activeSetBonuses.Count;
        }
        
        /// <summary>
        /// Get total equipped set pieces
        /// </summary>
        public int GetTotalEquippedPieces()
        {
            int total = 0;
            foreach (var kvp in _equippedSets)
            {
                total += kvp.Value.Count;
            }
            return total;
        }

        /// <summary>
        /// 导出保存数据 - 实现 BaseSystem 持久化接口
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var equippedSetsData = new Dictionary<string, object>();
            foreach (var kvp in _equippedSets)
            {
                var equipmentList = new Godot.Collections.Array();
                foreach (var equipId in kvp.Value)
                {
                    equipmentList.Add(equipId);
                }
                equippedSetsData[kvp.Key] = equipmentList;
            }

            var activeBonusesData = new Godot.Collections.Array();
            foreach (var bonus in _activeSetBonuses)
            {
                activeBonusesData.Add(new Dictionary
                {
                    { "setId", bonus.SetId },
                    { "setName", bonus.SetName },
                    { "pieceCount", bonus.PieceCount },
                    { "bonusIndex", bonus.BonusIndex }
                });
            }

            return new Dictionary
            {
                { "equippedSets", equippedSetsData },
                { "activeSetBonuses", activeBonusesData }
            };
        }

        /// <summary>
        /// 导入保存数据 - 实现 BaseSystem 持久化接口
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            _equippedSets.Clear();
            if (data.Contains("equippedSets"))
            {
                var equippedSetsData = data["equippedSets"] as Dictionary;
                foreach (string setIdStr in equippedSetsData.Keys)
                {
                    if (int.TryParse(setIdStr, out var setId))
                    {
                        var equipmentList = equippedSetsData[setIdStr] as Godot.Collections.Array;
                        var equipIds = new List<int>();
                        foreach (var equipId in equipmentList)
                        {
                            equipIds.Add(Convert.ToInt32(equipId));
                        }
                        _equippedSets[setId] = equipIds;
                    }
                }
            }

            _activeSetBonuses.Clear();
            if (data.Contains("activeSetBonuses"))
            {
                var activeBonusesData = data["activeSetBonuses"] as Godot.Collections.Array;
                foreach (Dictionary bonusDict in activeBonusesData)
                {
                    var bonus = new ActiveSetBonus
                    {
                        SetId = Convert.ToInt32(bonusDict["setId"]),
                        SetName = bonusDict["setName"].ToString(),
                        PieceCount = Convert.ToInt32(bonusDict["pieceCount"]),
                        BonusIndex = Convert.ToInt32(bonusDict["bonusIndex"])
                    };
                    _activeSetBonuses.Add(bonus);
                }
            }
        }
    }
}
