using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物数据库 - 存储所有宠物模板
    /// </summary>
    public class PetDatabase
    {
        private static PetDatabase _instance;
        public static PetDatabase Instance => _instance ??= new PetDatabase();

        private Dictionary<string, Pet> _pets = new Dictionary<string, Pet>();

        public PetDatabase()
        {
            InitializePets();
        }

        private void InitializePets()
        {
            // ===== 普通宠物 (Common) =====
            AddPet(new Pet
            {
                PetId = "pet_slime",
                PetName = "史莱姆",
                Type = PetType.Companion,
                Rarity = PetRarity.Common,
                HealthBonus = 10,
                AttackBonus = 2,
                DefenseBonus = 1,
                SpeedBonus = 0,
                CriticalBonus = 0,
                SpecialAbility = "",
                SpecialValue = 0
            });

            AddPet(new Pet
            {
                PetId = "pet_hamster",
                PetName = "仓鼠",
                Type = PetType.Collector,
                Rarity = PetRarity.Common,
                HealthBonus = 5,
                AttackBonus = 1,
                DefenseBonus = 1,
                SpeedBonus = 2,
                CriticalBonus = 0,
                SpecialAbility = "auto_pickup",
                SpecialValue = 0.1f  // 10% 自动拾取范围
            });

            AddPet(new Pet
            {
                PetId = "pet_bird",
                PetName = "小鸟",
                Type = PetType.Explorer,
                Rarity = PetRarity.Common,
                HealthBonus = 5,
                AttackBonus = 2,
                DefenseBonus = 0,
                SpeedBonus = 3,
                CriticalBonus = 1,
                SpecialAbility = "exp_boost",
                SpecialValue = 0.05f  // 5% 经验加成
            });

            // ===== 优秀宠物 (Uncommon) =====
            AddPet(new Pet
            {
                PetId = "pet_wolf",
                PetName = "小狼",
                Type = PetType.Companion,
                Rarity = PetRarity.Uncommon,
                HealthBonus = 25,
                AttackBonus = 5,
                DefenseBonus = 3,
                SpeedBonus = 2,
                CriticalBonus = 2,
                SpecialAbility = "",
                SpecialValue = 0
            });

            AddPet(new Pet
            {
                PetId = "pet_cat",
                PetName = "猫咪",
                Type = PetType.Collector,
                Rarity = PetRarity.Uncommon,
                HealthBonus = 15,
                AttackBonus = 3,
                DefenseBonus = 2,
                SpeedBonus = 5,
                CriticalBonus = 3,
                SpecialAbility = "auto_pickup",
                SpecialValue = 0.2f  // 20% 自动拾取范围
            });

            AddPet(new Pet
            {
                PetId = "pet_fox",
                PetName = "狐狸",
                Type = PetType.Guardian,
                Rarity = PetRarity.Uncommon,
                HealthBonus = 20,
                AttackBonus = 4,
                DefenseBonus = 5,
                SpeedBonus = 3,
                CriticalBonus = 2,
                SpecialAbility = "damage_reduction",
                SpecialValue = 0.05f  // 5% 伤害减免
            });

            // ===== 稀有宠物 (Rare) =====
            AddPet(new Pet
            {
                PetId = "pet_owl",
                PetName = "猫头鹰",
                Type = PetType.Explorer,
                Rarity = PetRarity.Rare,
                HealthBonus = 30,
                AttackBonus = 5,
                DefenseBonus = 3,
                SpeedBonus = 4,
                CriticalBonus = 5,
                SpecialAbility = "exp_boost",
                SpecialValue = 0.1f  // 10% 经验加成
            });

            AddPet(new Pet
            {
                PetId = "pet_tiger",
                PetName = "小虎",
                Type = PetType.Companion,
                Rarity = PetRarity.Rare,
                HealthBonus = 50,
                AttackBonus = 10,
                DefenseBonus = 5,
                SpeedBonus = 5,
                CriticalBonus = 5,
                SpecialAbility = "",
                SpecialValue = 0
            });

            AddPet(new Pet
            {
                PetId = "pet_turtle",
                PetName = "乌龟",
                Type = PetType.Guardian,
                Rarity = PetRarity.Rare,
                HealthBonus = 80,
                AttackBonus = 3,
                DefenseBonus = 15,
                SpeedBonus = 1,
                CriticalBonus = 0,
                SpecialAbility = "shield",
                SpecialValue = 0.1f  // 10% 最大生命值护盾
            });

            // ===== 史诗宠物 (Epic) =====
            AddPet(new Pet
            {
                PetId = "pet_dragon_whelp",
                PetName = "龙宝宝",
                Type = PetType.Companion,
                Rarity = PetRarity.Epic,
                HealthBonus = 100,
                AttackBonus = 20,
                DefenseBonus = 10,
                SpeedBonus = 8,
                CriticalBonus = 10,
                SpecialAbility = "fire_breath",
                SpecialValue = 0.2f  // 20% 额外火焰伤害
            });

            AddPet(new Pet
            {
                PetId = "pet_phoenix",
                PetName = "凤凰",
                Type = PetType.Guardian,
                Rarity = PetRarity.Epic,
                HealthBonus = 80,
                AttackBonus = 15,
                DefenseBonus = 15,
                SpeedBonus = 10,
                CriticalBonus = 8,
                SpecialAbility = "resurrect",
                SpecialValue = 0.3f  // 30% 生命值复活
            });

            AddPet(new Pet
            {
                PetId = "pet_griffin",
                PetName = "狮鹫",
                Type = PetType.Explorer,
                Rarity = PetRarity.Epic,
                HealthBonus = 60,
                AttackBonus = 12,
                DefenseBonus = 8,
                SpeedBonus = 15,
                CriticalBonus = 10,
                SpecialAbility = "drop_boost",
                SpecialValue = 0.15f  // 15% 掉落率加成
            });

            // ===== 传说宠物 (Legendary) =====
            AddPet(new Pet
            {
                PetId = "pet_celestial_dragon",
                PetName = "神龙",
                Type = PetType.Companion,
                Rarity = PetRarity.Legendary,
                HealthBonus = 200,
                AttackBonus = 30,
                DefenseBonus = 20,
                SpeedBonus = 15,
                CriticalBonus = 20,
                SpecialAbility = "all_stats",
                SpecialValue = 0.1f  // 全属性10%加成
            });

            AddPet(new Pet
            {
                PetId = "pet_angel",
                PetName = "天使",
                Type = PetType.Guardian,
                Rarity = PetRarity.Legendary,
                HealthBonus = 300,
                AttackBonus = 20,
                DefenseBonus = 30,
                SpeedBonus = 10,
                CriticalBonus = 15,
                SpecialAbility = "holy_protection",
                SpecialValue = 0.25f  // 25% 伤害减免 + 净化
            });

            AddPet(new Pet
            {
                PetId = "pet_lucky_cat",
                PetName = "幸运猫",
                Type = PetType.Collector,
                Rarity = PetRarity.Legendary,
                HealthBonus = 100,
                AttackBonus = 15,
                DefenseBonus = 10,
                SpeedBonus = 20,
                CriticalBonus = 25,
                SpecialAbility = "lucky",
                SpecialValue = 0.2f  // 20% 暴击率 + 稀有掉落
            });
        }

        private void AddPet(Pet pet)
        {
            _pets[pet.PetId] = pet;
        }

        public Pet GetPet(string petId)
        {
            if (_pets.TryGetValue(petId, out var pet))
            {
                return pet;
            }
            return null;
        }

        public List<Pet> GetAllPets()
        {
            return new List<Pet>(_pets.Values);
        }

        public List<Pet> GetPetsByType(PetType type)
        {
            List<Pet> result = new List<Pet>();
            foreach (var pet in _pets.Values)
            {
                if (pet.Type == type)
                    result.Add(pet);
            }
            return result;
        }

        public List<Pet> GetPetsByRarity(PetRarity rarity)
        {
            List<Pet> result = new List<Pet>();
            foreach (var pet in _pets.Values)
            {
                if (pet.Rarity == rarity)
                    result.Add(pet);
            }
            return result;
        }

        public Pet GetRandomPet(PetRarity minRarity = PetRarity.Common)
        {
            List<Pet> availablePets = new List<Pet>();
            foreach (var pet in _pets.Values)
            {
                if (pet.Rarity >= minRarity)
                    availablePets.Add(pet);
            }
            
            if (availablePets.Count == 0) return null;
            
            // 根据稀有度加权随机
            int totalWeight = 0;
            foreach (var pet in availablePets)
            {
                totalWeight += GetRarityWeight(pet.Rarity);
            }
            
            int randomWeight = GD.Randi() % totalWeight;
            int currentWeight = 0;
            
            foreach (var pet in availablePets)
            {
                currentWeight += GetRarityWeight(pet.Rarity);
                if (currentWeight > randomWeight)
                    return pet;
            }
            
            return availablePets[0];
        }

        private int GetRarityWeight(PetRarity rarity)
        {
            return rarity switch
            {
                PetRarity.Common => 50,
                PetRarity.Uncommon => 30,
                PetRarity.Rare => 15,
                PetRarity.Epic => 4,
                PetRarity.Legendary => 1,
                _ => 10
            };
        }
    }
}
