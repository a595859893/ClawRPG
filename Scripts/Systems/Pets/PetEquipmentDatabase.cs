using Godot;
using System;
using System.Collections.Generic;

namespace Game.Systems.Pets
{
    /// <summary>
    /// 宠物装备数据库
    /// </summary>
    public static class PetEquipmentDatabase
    {
        private static Dictionary<string, PetEquipment> _equipment = new Dictionary<string, PetEquipment>();
        
        public static void Initialize()
        {
            if (_equipment.Count > 0) return;
            
            // 项圈 (Collar)
            AddEquipment(new PetEquipment
            {
                Id = "collar_common_1",
                Name = "皮质项圈",
                Type = PetEquipmentType.Collar,
                Rarity = ItemRarity.Common,
                Price = 50,
                AttackBonus = 2,
                Description = "基础的皮质项圈"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "collar_uncommon_1",
                Name = "银质项圈",
                Type = PetEquipmentType.Collar,
                Rarity = ItemRarity.Uncommon,
                Price = 150,
                AttackBonus = 5,
                SpeedBonus = 1,
                Description = "带有银色装饰的项圈"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "collar_rare_1",
                Name = "魔法项圈",
                Type = PetEquipmentType.Collar,
                Rarity = ItemRarity.Rare,
                Price = 500,
                AttackBonus = 10,
                SpeedBonus = 3,
                CritRateBonus = 0.02f,
                Description = "附有魔法的项圈，提升攻击和速度"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "collar_epic_1",
                Name = "龙鳞项圈",
                Type = PetEquipmentType.Collar,
                Rarity = ItemRarity.Epic,
                Price = 1500,
                AttackBonus = 20,
                SpeedBonus = 5,
                CritRateBonus = 0.05f,
                CritDamageBonus = 0.1f,
                Description = "用龙鳞制作的稀有项圈"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "collar_legendary_1",
                Name = "传说龙皇项圈",
                Type = PetEquipmentType.Collar,
                Rarity = ItemRarity.Legendary,
                Price = 5000,
                AttackBonus = 35,
                SpeedBonus = 10,
                CritRateBonus = 0.1f,
                CritDamageBonus = 0.2f,
                LifeStealBonus = 5,
                Description = "传说中的至宝，传说级别的力量"
            });
            
            // 马具 (Harness)
            AddEquipment(new PetEquipment
            {
                Id = "harness_common_1",
                Name = "基础马具",
                Type = PetEquipmentType.Harness,
                Rarity = ItemRarity.Common,
                Price = 60,
                DefenseBonus = 3,
                Description = "基础的保护马具"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "harness_uncommon_1",
                Name = "强化马具",
                Type = PetEquipmentType.Harness,
                Rarity = ItemRarity.Uncommon,
                Price = 180,
                DefenseBonus = 8,
                HealthBonus = 20,
                Description = "经过强化的马具"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "harness_rare_1",
                Name = "符文马具",
                Type = PetEquipmentType.Harness,
                Rarity = ItemRarity.Rare,
                Price = 600,
                DefenseBonus = 15,
                HealthBonus = 50,
                SpeedBonus = 2,
                Description = "刻有符文的马具，提升防御和生命"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "harness_epic_1",
                Name = "泰坦马具",
                Type = PetEquipmentType.Harness,
                Rarity = ItemRarity.Epic,
                Price = 1800,
                DefenseBonus = 25,
                HealthBonus = 100,
                SpeedBonus = 4,
                CritRateBonus = 0.03f,
                Description = "用泰坦神铁打造的马具"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "harness_legendary_1",
                Name = "神圣守护马具",
                Type = PetEquipmentType.Harness,
                Rarity = ItemRarity.Legendary,
                Price = 5500,
                DefenseBonus = 40,
                HealthBonus = 200,
                SpeedBonus = 8,
                LifeStealBonus = 3,
                Description = "神圣祝福的守护马具"
            });
            
            // 护甲 (Armor)
            AddEquipment(new PetEquipment
            {
                Id = "armor_common_1",
                Name = "皮制护甲",
                Type = PetEquipmentType.Armor,
                Rarity = ItemRarity.Common,
                Price = 80,
                DefenseBonus = 5,
                HealthBonus = 15,
                Description = "基础宠物护甲"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "armor_uncommon_1",
                Name = "锁甲",
                Type = PetEquipmentType.Armor,
                Rarity = ItemRarity.Uncommon,
                Price = 200,
                DefenseBonus = 12,
                HealthBonus = 40,
                Description = "锁子甲，提供更好防护"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "armor_rare_1",
                Name = "魔法护甲",
                Type = PetEquipmentType.Armor,
                Rarity = ItemRarity.Rare,
                Price = 700,
                DefenseBonus = 20,
                HealthBonus = 80,
                CritDamageBonus = 0.05f,
                Description = "附有魔法的护甲"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "armor_epic_1",
                Name = "龙鳞护甲",
                Type = PetEquipmentType.Armor,
                Rarity = ItemRarity.Epic,
                Price = 2000,
                DefenseBonus = 30,
                HealthBonus = 150,
                CritRateBonus = 0.05f,
                CritDamageBonus = 0.1f,
                Description = "用龙鳞制作的顶级护甲"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "armor_legendary_1",
                Name = "不灭守护甲",
                Type = PetEquipmentType.Armor,
                Rarity = ItemRarity.Legendary,
                Price = 6000,
                DefenseBonus = 50,
                HealthBonus = 300,
                CritRateBonus = 0.08f,
                CritDamageBonus = 0.15f,
                LifeStealBonus = 5,
                Description = "永恒不灭的守护护甲"
            });
            
            // 配饰 (Accessory)
            AddEquipment(new PetEquipment
            {
                Id = "accessory_common_1",
                Name = "小铃铛",
                Type = PetEquipmentType.Accessory,
                Rarity = ItemRarity.Common,
                Price = 40,
                SpeedBonus = 1,
                Description = "可爱的小铃铛"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "accessory_uncommon_1",
                Name = "幸运符",
                Type = PetEquipmentType.Accessory,
                Rarity = ItemRarity.Uncommon,
                Price = 160,
                SpeedBonus = 3,
                CritRateBonus = 0.02f,
                Description = "带来幸运的护符"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "accessory_rare_1",
                Name = "智慧之眼",
                Type = PetEquipmentType.Accessory,
                Rarity = ItemRarity.Rare,
                Price = 550,
                AttackBonus = 8,
                SpeedBonus = 5,
                CritRateBonus = 0.05f,
                Description = "赋予宠物智慧的神秘饰品"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "accessory_epic_1",
                Name = "元素之心",
                Type = PetEquipmentType.Accessory,
                Rarity = ItemRarity.Epic,
                Price = 1600,
                AttackBonus = 15,
                SpeedBonus = 8,
                CritRateBonus = 0.08f,
                CritDamageBonus = 0.1f,
                Description = "蕴含元素力量的宝石"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "accessory_legendary_1",
                Name = "命运项链",
                Type = PetEquipmentType.Accessory,
                Rarity = ItemRarity.Legendary,
                Price = 4500,
                AttackBonus = 25,
                SpeedBonus = 12,
                CritRateBonus = 0.12f,
                CritDamageBonus = 0.2f,
                LifeStealBonus = 8,
                Description = "掌控命运的传奇饰品"
            });
            
            // 玩具 (Toy)
            AddEquipment(new PetEquipment
            {
                Id = "toy_common_1",
                Name = "毛线球",
                Type = PetEquipmentType.Toy,
                Rarity = ItemRarity.Common,
                Price = 30,
                AttackBonus = 1,
                SpeedBonus = 1,
                Description = "宠物喜欢的玩具"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "toy_uncommon_1",
                Name = "橡胶球",
                Type = PetEquipmentType.Toy,
                Rarity = ItemRarity.Uncommon,
                Price = 120,
                AttackBonus = 3,
                SpeedBonus = 2,
                Description = "弹力十足的橡胶球"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "toy_rare_1",
                Name = "发光飞盘",
                Type = PetEquipmentType.Toy,
                Rarity = ItemRarity.Rare,
                Price = 450,
                AttackBonus = 6,
                SpeedBonus = 4,
                CritRateBonus = 0.03f,
                Description = "夜间也能玩耍的发光飞盘"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "toy_epic_1",
                Name = "雷电法杖",
                Type = PetEquipmentType.Toy,
                Rarity = ItemRarity.Epic,
                Price = 1400,
                AttackBonus = 12,
                SpeedBonus = 7,
                CritRateBonus = 0.06f,
                CritDamageBonus = 0.08f,
                Description = "蕴含雷电力量的玩具"
            });
            
            AddEquipment(new PetEquipment
            {
                Id = "toy_legendary_1",
                Name = "创世神印",
                Type = PetEquipmentType.Toy,
                Rarity = ItemRarity.Legendary,
                Price = 4000,
                AttackBonus = 20,
                SpeedBonus = 10,
                CritRateBonus = 0.1f,
                CritDamageBonus = 0.15f,
                LifeStealBonus = 6,
                Description = "传说中创世神的玩具"
            });
        }
        
        private static void AddEquipment(PetEquipment equipment)
        {
            _equipment[equipment.Id] = equipment;
        }
        
        public static PetEquipment GetEquipment(string id)
        {
            if (_equipment.ContainsKey(id))
                return _equipment[id];
            return null;
        }
        
        public static List<PetEquipment> GetAllEquipment()
        {
            return new List<PetEquipment>(_equipment.Values);
        }
        
        public static List<PetEquipment> GetEquipmentByType(PetEquipmentType type)
        {
            List<PetEquipment> result = new List<PetEquipment>();
            foreach (var equip in _equipment.Values)
            {
                if (equip.Type == type)
                    result.Add(equip);
            }
            return result;
        }
        
        public static List<PetEquipment> GetEquipmentByRarity(ItemRarity rarity)
        {
            List<PetEquipment> result = new List<PetEquipment>();
            foreach (var equip in _equipment.Values)
            {
                if (equip.Rarity == rarity)
                    result.Add(equip);
            }
            return result;
        }
        
        public static List<PetEquipment> GetShopEquipment()
        {
            List<PetEquipment> result = new List<PetEquipment>();
            foreach (var equip in _equipment.Values)
            {
                // 出售所有装备
                result.Add(equip);
            }
            return result;
        }
    }
}
