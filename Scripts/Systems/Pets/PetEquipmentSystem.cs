using Godot;
using System;
using System.Collections.Generic;

namespace Game.Systems.Pets
{
    /// <summary>
    /// 宠物装备系统管理器
    /// </summary>
    public class PetEquipmentSystem : BaseSystem
    {
        public static PetEquipmentSystem Instance { get; private set; }
        
        private PlayerPetEquipmentData _playerData = new PlayerPetEquipmentData();
        
        // 信号
        [Signal] public delegate void EquipmentPurchased(string equipmentId);
        [Signal] public delegate void EquipmentEquipped(string petId, string equipmentId);
        [Signal] public delegate void EquipmentUnequipped(string petId, string equipmentId);
        [Signal] public delegate void DataLoaded();
        
        public override void _Ready()
        {
            Instance = this;
            PetEquipmentDatabase.Initialize();
        }
        
        /// <summary>
        /// 购买宠物装备
        /// </summary>
        public bool PurchaseEquipment(string equipmentId, int playerGold)
        {
            var equipment = PetEquipmentDatabase.GetEquipment(equipmentId);
            if (equipment == null)
            {
                GD.PrintErr($"[PetEquipmentSystem] Equipment not found: {equipmentId}");
                return false;
            }
            
            if (playerGold < equipment.Price)
            {
                GD.PrintErr($"[PetEquipmentSystem] Not enough gold. Required: {equipment.Price}, Have: {playerGold}");
                return false;
            }
            
            string typeKey = equipment.Type.ToString();
            if (!_playerData.OwnedEquipment.ContainsKey(typeKey))
            {
                _playerData.OwnedEquipment[typeKey] = new List<string>();
            }
            
            // 检查是否已拥有
            if (_playerData.OwnedEquipment[typeKey].Contains(equipmentId))
            {
                GD.PrintErr($"[PetEquipmentSystem] Already owned: {equipmentId}");
                return false;
            }
            
            // 添加到已拥有列表
            _playerData.OwnedEquipment[typeKey].Add(equipmentId);
            
            EmitSignal(nameof(EquipmentPurchased), equipmentId);
            GD.Print($"[PetEquipmentSystem] Purchased: {equipment.Name} for {equipment.Price} gold");
            return true;
        }
        
        /// <summary>
        /// 检查是否拥有装备
        /// </summary>
        public bool HasEquipment(string equipmentId)
        {
            foreach (var list in _playerData.OwnedEquipment.Values)
            {
                if (list.Contains(equipmentId))
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// 获取玩家拥有的所有装备
        /// </summary>
        public List<string> GetOwnedEquipment()
        {
            List<string> result = new List<string>();
            foreach (var list in _playerData.OwnedEquipment.Values)
            {
                result.AddRange(list);
            }
            return result;
        }
        
        /// <summary>
        /// 获取玩家拥有的指定类型装备
        /// </summary>
        public List<string> GetOwnedEquipmentByType(PetEquipmentType type)
        {
            string typeKey = type.ToString();
            if (_playerData.OwnedEquipment.ContainsKey(typeKey))
            {
                return new List<string>(_playerData.OwnedEquipment[typeKey]);
            }
            return new List<string>();
        }
        
        /// <summary>
        /// 为宠物装备装备
        /// </summary>
        public bool EquipToPet(string petId, string equipmentId)
        {
            var equipment = PetEquipmentDatabase.GetEquipment(equipmentId);
            if (equipment == null)
            {
                GD.PrintErr($"[PetEquipmentSystem] Equipment not found: {equipmentId}");
                return false;
            }
            
            // 检查是否拥有
            if (!HasEquipment(equipmentId))
            {
                GD.PrintErr($"[PetEquipmentSystem] Do not own equipment: {equipmentId}");
                return false;
            }
            
            string typeKey = equipment.Type.ToString();
            
            // 卸下当前装备
            if (_playerData.EquippedEquipment.ContainsKey(petId))
            {
                string currentEquip = _playerData.EquippedEquipment[petId];
                if (!string.IsNullOrEmpty(currentEquip))
                {
                    var current = PetEquipmentDatabase.GetEquipment(currentEquip);
                    if (current != null && current.Type == equipment.Type)
                    {
                        // 同类型装备，直接替换
                    }
                }
            }
            
            _playerData.EquippedEquipment[petId] = equipmentId;
            EmitSignal(nameof(EquipmentEquipped), petId, equipmentId);
            GD.Print($"[PetEquipmentSystem] Equipped {equipment.Name} to pet {petId}");
            return true;
        }
        
        /// <summary>
        /// 卸下宠物装备
        /// </summary>
        public bool UnequipFromPet(string petId)
        {
            if (!_playerData.EquippedEquipment.ContainsKey(petId))
            {
                return false;
            }
            
            string equipmentId = _playerData.EquippedEquipment[petId];
            _playerData.EquippedEquipment.Remove(petId);
            
            if (!string.IsNullOrEmpty(equipmentId))
            {
                var equipment = PetEquipmentDatabase.GetEquipment(equipmentId);
                if (equipment != null)
                {
                    GD.Print($"[PetEquipmentSystem] Unequipped {equipment.Name} from pet {petId}");
                }
            }
            
            EmitSignal(nameof(EquipmentUnequipped), petId, equipmentId);
            return true;
        }
        
        /// <summary>
        /// 获取宠物当前装备
        /// </summary>
        public string GetEquippedEquipment(string petId)
        {
            if (_playerData.EquippedEquipment.ContainsKey(petId))
                return _playerData.EquippedEquipment[petId];
            return null;
        }
        
        /// <summary>
        /// 获取宠物装备加成
        /// </summary>
        public Dictionary<string, int> GetPetEquipmentBonuses(string petId)
        {
            Dictionary<string, int> bonuses = new Dictionary<string, int>
            {
                { "attack", 0 },
                { "defense", 0 },
                { "health", 0 },
                { "speed", 0 },
                { "lifesteal", 0 }
            };
            
            Dictionary<string, float> floatBonuses = new Dictionary<string, float>
            {
                { "crit_rate", 0f },
                { "crit_damage", 0f }
            };
            
            string equipmentId = GetEquippedEquipment(petId);
            if (string.IsNullOrEmpty(equipmentId))
                return bonuses;
            
            var equipment = PetEquipmentDatabase.GetEquipment(equipmentId);
            if (equipment == null)
                return bonuses;
            
            bonuses["attack"] = equipment.AttackBonus;
            bonuses["defense"] = equipment.DefenseBonus;
            bonuses["health"] = equipment.HealthBonus;
            bonuses["speed"] = equipment.SpeedBonus;
            bonuses["lifesteal"] = equipment.LifeStealBonus;
            floatBonuses["crit_rate"] = equipment.CritRateBonus;
            floatBonuses["crit_damage"] = equipment.CritDamageBonus;
            
            return bonuses;
        }
        
        /// <summary>
        /// 获取浮点型装备加成（用于显示）
        /// </summary>
        public Dictionary<string, float> GetPetEquipmentFloatBonuses(string petId)
        {
            Dictionary<string, float> bonuses = new Dictionary<string, float>
            {
                { "crit_rate", 0f },
                { "crit_damage", 0f }
            };
            
            string equipmentId = GetEquippedEquipment(petId);
            if (string.IsNullOrEmpty(equipmentId))
                return bonuses;
            
            var equipment = PetEquipmentDatabase.GetEquipment(equipmentId);
            if (equipment == null)
                return bonuses;
            
            bonuses["crit_rate"] = equipment.CritRateBonus;
            bonuses["crit_damage"] = equipment.CritDamageBonus;
            
            return bonuses;
        }
        
        /// <summary>
        /// 保存数据
        /// </summary>
        public Dictionary Save()
        {
            Dictionary data = new Dictionary();
            
            // 保存已拥有装备
            Dictionary ownedData = new Dictionary();
            foreach (var kvp in _playerData.OwnedEquipment)
            {
                ownedData[kvp.Key] = new Godot.Collections.Array(kvp.Value);
            }
            data["owned"] = ownedData;
            
            // 保存已装备
            Godot.Collections.Array equippedList = new Godot.Collections.Array();
            foreach (var kvp in _playerData.EquippedEquipment)
            {
                Dictionary equipEntry = new Dictionary();
                equipEntry["pet_id"] = kvp.Key;
                equipEntry["equipment_id"] = kvp.Value;
                equippedList.Add(equipEntry);
            }
            data["equipped"] = equippedList;
            
            return data;
        }
        
        /// <summary>
        /// 加载数据
        /// </summary>
        public void Load(Dictionary data)
        {
            if (data == null) return;
            
            _playerData = new PlayerPetEquipmentData();
            
            // 加载已拥有装备
            if (data.Contains("owned"))
            {
                Dictionary ownedData = (Dictionary)data["owned"];
                foreach (string key in ownedData.Keys)
                {
                    Godot.Collections.Array list = (Godot.Collections.Array)ownedData[key];
                    List<string> equipmentList = new List<string>();
                    foreach (var item in list)
                    {
                        equipmentList.Add(item.ToString());
                    }
                    _playerData.OwnedEquipment[key] = equipmentList;
                }
            }
            
            // 加载已装备
            if (data.Contains("equipped"))
            {
                Godot.Collections.Array equippedList = (Godot.Collections.Array)data["equipped"];
                foreach (Dictionary entry in equippedList)
                {
                    string petId = entry["pet_id"].ToString();
                    string equipmentId = entry["equipment_id"].ToString();
                    _playerData.EquippedEquipment[petId] = equipmentId;
                }
            }
            
            EmitSignal(nameof(DataLoaded));
            GD.Print("[PetEquipmentSystem] Data loaded");
        }
    }

}
