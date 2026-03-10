using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Items
{
    /// <summary>
    /// 药水数据库 - 数据驱动设计
    /// </summary>
    public class PotionDatabase
    {
        private static PotionDatabase _instance;
        public static PotionDatabase Instance => _instance ??= new PotionDatabase();

        private Dictionary<int, Potion> _potions = new Dictionary<int, Potion>();

        public PotionDatabase()
        {
            InitializePotions();
        }

        private void InitializePotions()
        {
            // 生命药水 (ID: 501-510)
            AddPotion(new Potion
            {
                Id = 501,
                Name = "小生命药水",
                Description = "恢复少量生命值",
                Type = PotionType.Health,
                Rarity = PotionRarity.Common,
                Value = 10,
                HealthRestore = 25,
                MaxStack = 99,
                Cooldown = 5f
            });

            AddPotion(new Potion
            {
                Id = 502,
                Name = "中生命药水",
                Description = "恢复中等生命值",
                Type = PotionType.Health,
                Rarity = PotionRarity.Uncommon,
                Value = 25,
                HealthRestore = 75,
                MaxStack = 99,
                Cooldown = 5f
            });

            AddPotion(new Potion
            {
                Id = 503,
                Name = "大生命药水",
                Description = "恢复大量生命值",
                Type = PotionType.Health,
                Rarity = PotionRarity.Rare,
                Value = 50,
                HealthRestore = 150,
                MaxStack = 99,
                Cooldown = 5f
            });

            AddPotion(new Potion
            {
                Id = 504,
                Name = "超级生命药水",
                Description = "恢复大量生命值并持续恢复",
                Type = PotionType.Health,
                Rarity = PotionRarity.Epic,
                Value = 100,
                HealthRestore = 250,
                HealthRegen = 5,
                Duration = 30f,
                MaxStack = 50,
                Cooldown = 10f
            });

            AddPotion(new Potion
            {
                Id = 505,
                Name = "传说生命药水",
                Description = "恢复全部生命值",
                Type = PotionType.Health,
                Rarity = PotionRarity.Legendary,
                Value = 500,
                HealthRestore = 9999,
                MaxStack = 10,
                Cooldown = 60f
            });

            // 法力药水 (ID: 511-520)
            AddPotion(new Potion
            {
                Id = 511,
                Name = "小法力药水",
                Description = "恢复少量法力值",
                Type = PotionType.Mana,
                Rarity = PotionRarity.Common,
                Value = 10,
                ManaRestore = 25,
                MaxStack = 99,
                Cooldown = 5f
            });

            AddPotion(new Potion
            {
                Id = 512,
                Name = "中法力药水",
                Description = "恢复中等法力值",
                Type = PotionType.Mana,
                Rarity = PotionRarity.Uncommon,
                Value = 25,
                ManaRestore = 75,
                MaxStack = 99,
                Cooldown = 5f
            });

            AddPotion(new Potion
            {
                Id = 513,
                Name = "大法力药水",
                Description = "恢复大量法力值",
                Type = PotionType.Mana,
                Rarity = PotionRarity.Rare,
                Value = 50,
                ManaRestore = 150,
                MaxStack = 99,
                Cooldown = 5f
            });

            AddPotion(new Potion
            {
                Id = 514,
                Name = "超级法力药水",
                Description = "恢复大量法力值并持续恢复",
                Type = PotionType.Mana,
                Rarity = PotionRarity.Epic,
                Value = 100,
                ManaRestore = 250,
                ManaRegen = 5,
                Duration = 30f,
                MaxStack = 50,
                Cooldown = 10f
            });

            // 体力药水 (ID: 521-525)
            AddPotion(new Potion
            {
                Id = 521,
                Name = "体力药水",
                Description = "恢复体力值",
                Type = PotionType.Stamina,
                Rarity = PotionRarity.Common,
                Value = 15,
                ManaRestore = 50,
                MaxStack = 99,
                Cooldown = 3f
            });

            AddPotion(new Potion
            {
                Id = 522,
                Name = "超级体力药水",
                Description = "恢复大量体力值",
                Type = PotionType.Stamina,
                Rarity = PotionRarity.Uncommon,
                Value = 35,
                ManaRestore = 150,
                MaxStack = 50,
                Cooldown = 3f
            });

            // 增益药水 (ID: 531-545)
            AddPotion(new Potion
            {
                Id = 531,
                Name = "力量药水",
                Description = "增加攻击力",
                Type = PotionType.Damage,
                Rarity = PotionRarity.Uncommon,
                Value = 30,
                DamageBoost = 0.15f,
                Duration = 60f,
                MaxStack = 50,
                Cooldown = 30f
            });

            AddPotion(new Potion
            {
                Id = 532,
                Name = "强效力量药水",
                Description = "大幅增加攻击力",
                Type = PotionType.Damage,
                Rarity = PotionRarity.Rare,
                Value = 60,
                DamageBoost = 0.30f,
                Duration = 120f,
                MaxStack = 30,
                Cooldown = 30f
            });

            AddPotion(new Potion
            {
                Id = 533,
                Name = "传说力量药水",
                Description = "大幅增加攻击力并提高暴击率",
                Type = PotionType.Damage,
                Rarity = PotionRarity.Legendary,
                Value = 300,
                DamageBoost = 0.50f,
                CriticalBoost = 0.15f,
                Duration = 180f,
                MaxStack = 10,
                Cooldown = 60f
            });

            AddPotion(new Potion
            {
                Id = 541,
                Name = "防御药水",
                Description = "增加防御力",
                Type = PotionType.Defense,
                Rarity = PotionRarity.Uncommon,
                Value = 30,
                DefenseBoost = 0.15f,
                Duration = 60f,
                MaxStack = 50,
                Cooldown = 30f
            });

            AddPotion(new Potion
            {
                Id = 542,
                Name = "强效防御药水",
                Description = "大幅增加防御力",
                Type = PotionType.Defense,
                Rarity = PotionRarity.Rare,
                Value = 60,
                DefenseBoost = 0.30f,
                Duration = 120f,
                MaxStack = 30,
                Cooldown = 30f
            });

            AddPotion(new Potion
            {
                Id = 551,
                Name = "速度药水",
                Description = "增加移动速度",
                Type = PotionType.Speed,
                Rarity = PotionRarity.Uncommon,
                Value = 25,
                SpeedBoost = 0.20f,
                Duration = 60f,
                MaxStack = 50,
                Cooldown = 30f
            });

            AddPotion(new Potion
            {
                Id = 552,
                Name = "超级速度药水",
                Description = "大幅增加移动速度",
                Type = PotionType.Speed,
                Rarity = PotionRarity.Rare,
                Value = 55,
                SpeedBoost = 0.40f,
                Duration = 120f,
                MaxStack = 30,
                Cooldown = 30f
            });

            AddPotion(new Potion
            {
                Id = 561,
                Name = "暴击药水",
                Description = "增加暴击率",
                Type = PotionType.Critical,
                Rarity = PotionRarity.Rare,
                Value = 80,
                CriticalBoost = 0.20f,
                Duration = 60f,
                MaxStack = 30,
                Cooldown = 45f
            });

            // 再生药水 (ID: 571-575)
            AddPotion(new Potion
            {
                Id = 571,
                Name = "生命再生药水",
                Description = "持续恢复生命值",
                Type = PotionType.Regeneration,
                Rarity = PotionRarity.Uncommon,
                Value = 40,
                HealthRegen = 10,
                Duration = 60f,
                MaxStack = 50,
                Cooldown = 15f
            });

            AddPotion(new Potion
            {
                Id = 572,
                Name = "法力再生药水",
                Description = "持续恢复法力值",
                Type = PotionType.Regeneration,
                Rarity = PotionRarity.Uncommon,
                Value = 40,
                ManaRegen = 10,
                Duration = 60f,
                MaxStack = 50,
                Cooldown = 15f
            });

            // 解毒药水 (ID: 581-582)
            AddPotion(new Potion
            {
                Id = 581,
                Name = "解毒药水",
                Description = "清除所有负面状态效果",
                Type = PotionType.Antidote,
                Rarity = PotionRarity.Uncommon,
                Value = 50,
                MaxStack = 50,
                Cooldown = 30f
            });

            // 隐形药水 (ID: 591-592)
            AddPotion(new Potion
            {
                Id = 591,
                Name = "隐形药水",
                Description = "使敌人无法发现你",
                Type = PotionType.Invisibility,
                Rarity = PotionRarity.Rare,
                Value = 100,
                Duration = 30f,
                MaxStack = 20,
                Cooldown = 60f
            });
        }

        private void AddPotion(Potion potion)
        {
            _potions[potion.Id] = potion;
        }

        public Potion GetPotion(int id)
        {
            return _potions.ContainsKey(id) ? _potions[id] : null;
        }

        public List<Potion> GetAllPotions()
        {
            return new List<Potion>(_potions.Values);
        }

        public List<Potion> GetPotionsByType(PotionType type)
        {
            List<Potion> result = new List<Potion>();
            foreach (var potion in _potions.Values)
            {
                if (potion.Type == type)
                    result.Add(potion);
            }
            return result;
        }

        public List<Potion> GetPotionsByRarity(PotionRarity rarity)
        {
            List<Potion> result = new List<Potion>();
            foreach (var potion in _potions.Values)
            {
                if (potion.Rarity == rarity)
                    result.Add(potion);
            }
            return result;
        }

        public List<Potion> GetConsumablePotions()
        {
            List<Potion> result = new List<Potion>();
            foreach (var potion in _potions.Values)
            {
                // 直接恢复的的药水
                if (potion.HealthRestore > 0 || potion.ManaRestore > 0 || potion.Duration == 0)
                    result.Add(potion);
            }
            return result;
        }

        public List<Potion> GetBuffPotions()
        {
            List<Potion> result = new List<Potion>();
            foreach (var potion in _potions.Values)
            {
                // 有持续效果的药水
                if (potion.Duration > 0)
                    result.Add(potion);
            }
            return result;
        }
    }
}
