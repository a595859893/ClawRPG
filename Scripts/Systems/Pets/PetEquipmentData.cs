using Godot;
using System;
using System.Collections.Generic;

namespace Game.Systems.Pets
{
    /// <summary>
    /// 宠物装备数据结构
    /// </summary>
    public class PetEquipment
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public PetEquipmentType Type { get; set; }
        public ItemRarity Rarity { get; set; }
        public int Price { get; set; }
        
        // 属性加成
        public int AttackBonus { get; set; }
        public int DefenseBonus { get; set; }
        public int HealthBonus { get; set; }
        public int SpeedBonus { get; set; }
        public float CritRateBonus { get; set; }
        public float CritDamageBonus { get; set; }
        public int LifeStealBonus { get; set; }
        
        public string Description { get; set; }
        
        public PetEquipment()
        {
            Type = PetEquipmentType.Accessory;
            Rarity = ItemRarity.Common;
        }
    }
    
    public enum PetEquipmentType
    {
        Collar,      // 项圈
        Harness,     // 马具
        Armor,       // 护甲
        Accessory,   // 配饰
        Toy          // 玩具
    }
    
    public enum ItemRarity
    {
        Common,     // 普通
        Uncommon,   // 优秀
        Rare,       // 稀有
        Epic,       // 史诗
        Legendary   // 传说
    }
    
    /// <summary>
    /// 玩家宠物装备数据
    /// </summary>
    public class PlayerPetEquipmentData
    {
        public Dictionary<string, List<string>> OwnedEquipment { get; set; } = new Dictionary<string, List<string>>();
        public Dictionary<string, string> EquippedEquipment { get; set; } = new Dictionary<string, string>(); // pet_id -> equipment_id
        
        public PlayerPetEquipmentData()
        {
            // 初始化每个装备槽位
            foreach (PetEquipmentType type in Enum.GetValues(typeof(PetEquipmentType)))
            {
                if (!OwnedEquipment.ContainsKey(type.ToString()))
                {
                    OwnedEquipment[type.ToString()] = new List<string>();
                }
            }
        }
    }
}
