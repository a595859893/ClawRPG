using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Systems;

namespace ClawRPG.Systems {
    /// <summary>
    /// Database of all fate relics (roguelike-style collectible relics)
    /// </summary>
    public static class FateRelicDatabase {
        private static Dictionary<string, FateRelic> _relics;
        
        static FateRelicDatabase() {
            _relics = new Dictionary<string, FateRelic>();
            InitializeRelics();
        }
        
        private static void InitializeRelic(string id, string name, string description, 
            FateRelicRarity rarity, FateRelicType type, params FateRelicEffect[] effects) {
            var relic = new FateRelic {
                Id = id,
                Name = name,
                Description = description,
                Rarity = rarity,
                Type = type,
                Effects = new List<FateRelicEffect>(effects)
            };
            _relics[id] = relic;
        }
        
        private static void InitializeRelics() {
            // Combat Relics - Common
            InitializeRelic("combat_001", "Rusty Blade", "Increases attack damage by 5%",
                FateRelicRarity.Common, FateRelicType.Combat,
                new FateRelicEffect { Stat = "attack_bonus", Value = 0.05f, Description = "+5% Attack Damage" });
            
            InitializeRelic("combat_002", "Sharp Stone", "Increases critical hit chance by 3%",
                FateRelicRarity.Common, FateRelicType.Combat,
                new FateRelicEffect { Stat = "crit_rate", Value = 0.03f, Description = "+3% Critical Chance" });
            
            InitializeRelic("combat_003", "Warrior's Mark", "Increases damage against elite enemies by 10%",
                FateRelicRarity.Common, FateRelicType.Combat,
                new FateRelicEffect { Stat = "elite_damage", Value = 0.10f, Description = "+10% Elite Damage" });
            
            // Combat Relics - Uncommon
            InitializeRelic("combat_004", "Berserker's Fury", "Attack speed +8%, but defense -5%",
                FateRelicRarity.Uncommon, FateRelicType.Combat,
                new FateRelicEffect { Stat = "attack_speed", Value = 0.08f, Description = "+8% Attack Speed" },
                new FateRelicEffect { Stat = "defense", Value = -0.05f, Description = "-5% Defense" });
            
            InitializeRelic("combat_005", "Vampiric Amulet", "Life steal +5% on hit",
                FateRelicRarity.Uncommon, FateRelicType.Combat,
                new FateRelicEffect { Stat = "lifesteal", Value = 0.05f, Description = "+5% Life Steal" });
            
            InitializeRelic("combat_006", "Chain Breaker", "Combo damage +15% per combo point",
                FateRelicRarity.Uncommon, FateRelicType.Combat,
                new FateRelicEffect { Stat = "combo_damage", Value = 0.15f, Description = "+15% Combo Damage" });
            
            // Combat Relics - Rare
            InitializeRelic("combat_007", "Dragon's Breath", "Fire damage +20%, chance to burn enemies",
                FateRelicRarity.Rare, FateRelicType.Combat,
                new FateRelicEffect { Stat = "fire_damage", Value = 0.20f, Description = "+20% Fire Damage" },
                new FateRelicEffect { Stat = "burn_chance", Value = 0.10f, Description = "10% Burn Chance" });
            
            InitializeRelic("combat_008", "Thunder God's Blessing", "Lightning damage +25%, chance to stun",
                FateRelicRarity.Rare, FateRelicType.Combat,
                new FateRelicEffect { Stat = "lightning_damage", Value = 0.25f, Description = "+25% Lightning Damage" },
                new FateRelicEffect { Stat = "stun_chance", Value = 0.08f, Description = "8% Stun Chance" });
            
            InitializeRelic("combat_009", "Assassin's Shadow", "Backstab damage +40%",
                FateRelicRarity.Rare, FateRelicType.Combat,
                new FateRelicEffect { Stat = "backstab_damage", Value = 0.40f, Description = "+40% Backstab Damage" });
            
            // Combat Relics - Epic
            InitializeRelic("combat_010", "War God's Wrath", "All damage +15%, but health -10%",
                FateRelicRarity.Epic, FateRelicType.Combat,
                new FateRelicEffect { Stat = "all_damage", Value = 0.15f, Description = "+15% All Damage" },
                new FateRelicEffect { Stat = "max_health", Value = -0.10f, Description = "-10% Max Health" });
            
            InitializeRelic("combat_011", "Blood Moon Pendant", "Life steal +10%, damage increases when low HP",
                FateRelicRarity.Epic, FateRelicType.Combat,
                new FateRelicEffect { Stat = "lifesteal", Value = 0.10f, Description = "+10% Life Steal" },
                new FateRelicEffect { Stat = "low_hp_damage", Value = 0.25f, Description = "+25% Damage Below 30% HP" });
            
            // Combat Relics - Legendary
            InitializeRelic("combat_012", "Godslayer", "Boss damage +50%, but combat takes longer",
                FateRelicRarity.Legendary, FateRelicType.Combat,
                new FateRelicEffect { Stat = "boss_damage", Value = 0.50f, Description = "+50% Boss Damage" },
                new FateRelicEffect { Stat = "enemy_health", Value = 0.10f, Description = "+10% Enemy Health" });
            
            // Defense Relics - Common
            InitializeRelic("defense_001", "Iron Shield", "Defense +5%",
                FateRelicRarity.Common, FateRelicType.Defense,
                new FateRelicEffect { Stat = "defense_bonus", Value = 0.05f, Description = "+5% Defense" });
            
            InitializeRelic("defense_002", "Stone Skin", "Take 3% less damage from all sources",
                FateRelicRarity.Common, FateRelicType.Defense,
                new FateRelicEffect { Stat = "damage_reduction", Value = 0.03f, Description = "-3% Damage Taken" });
            
            // Defense Relics - Uncommon
            InitializeRelic("defense_003", "Thorn Mail", "Reflect 5% damage to attackers",
                FateRelicRarity.Uncommon, FateRelicType.Defense,
                new FateRelicEffect { Stat = "damage_reflect", Value = 0.05f, Description = "5% Damage Reflect" });
            
            InitializeRelic("defense_004", "Guardian Angel", "One free death per dungeon floor",
                FateRelicRarity.Uncommon, FateRelicType.Defense,
                new FateRelicEffect { Stat = "free_death", Value = 1f, Description = "1 Free Death/Floor" });
            
            // Defense Relics - Rare
            InitializeRelic("defense_005", "Frost Barrier", "Ice damage +15%, chance to freeze attackers",
                FateRelicRarity.Rare, FateRelicType.Defense,
                new FateRelicEffect { Stat = "ice_damage", Value = 0.15f, Description = "+15% Ice Damage" },
                new FateRelicEffect { Stat = "freeze_reflect", Value = 0.08f, Description = "8% Freeze Reflect" });
            
            // Defense Relics - Epic
            InitializeRelic("defense_006", "Divine Shield", "Invincible for 3 seconds every 30 seconds",
                FateRelicRarity.Epic, FateRelicType.Defense,
                new FateRelicEffect { Stat = "invincibility", Value = 3f, Description = "3s Invincibility/30s" });
            
            // Defense Relics - Legendary
            InitializeRelic("defense_007", "World Tree's Blessing", "Health +25%, Regen +50%",
                FateRelicRarity.Legendary, FateRelicType.Defense,
                new FateRelicEffect { Stat = "max_health", Value = 0.25f, Description = "+25% Max Health" },
                new FateRelicEffect { Stat = "health_regen", Value = 0.50f, Description = "+50% Health Regen" });
            
            // Utility Relics - Common
            InitializeRelic("utility_001", "Quick Boots", "Movement speed +5%",
                FateRelicRarity.Common, FateRelicType.Utility,
                new FateRelicEffect { Stat = "movement_speed", Value = 0.05f, Description = "+5% Movement Speed" });
            
            InitializeRelic("utility_002", "Mana Crystal", "Max mana +8%",
                FateRelicRarity.Common, FateRelicType.Utility,
                new FateRelicEffect { Stat = "max_mana", Value = 0.08f, Description = "+8% Max Mana" });
            
            // Utility Relics - Uncommon
            InitializeRelic("utility_003", "Time Watch", "Cooldown reduction +10%",
                FateRelicRarity.Uncommon, FateRelicType.Utility,
                new FateRelicEffect { Stat = "cooldown_reduction", Value = 0.10f, Description = "-10% Cooldowns" });
            
            InitializeRelic("utility_004", "XP Crystal", "Experience gain +10%",
                FateRelicRarity.Uncommon, FateRelicType.Utility,
                new FateRelicEffect { Stat = "exp_gain", Value = 0.10f, Description = "+10% XP Gain" });
            
            // Utility Relics - Rare
            InitializeRelic("utility_005", "Dimensional Ring", "Extra inventory slot +2",
                FateRelicRarity.Rare, FateRelicType.Utility,
                new FateRelicEffect { Stat = "inventory_slots", Value = 2f, Description = "+2 Inventory Slots" });
            
            InitializeRelic("utility_006", "Ancient Compass", "Reveal hidden passages and secrets",
                FateRelicRarity.Rare, FateRelicType.Utility,
                new FateRelicEffect { Stat = "secret_detection", Value = 1f, Description = "Reveal Secrets" });
            
            // Utility Relics - Epic
            InitializeRelic("utility_007", "Phoenix Feather", "Revive once with 50% HP on death",
                FateRelicRarity.Epic, FateRelicType.Utility,
                new FateRelicEffect { Stat = "revive", Value = 0.50f, Description = "1 Revive @ 50% HP" });
            
            // Utility Relics - Legendary
            InitializeRelic("utility_008", "Time Stopper", "Use any skill once without cooldown",
                FateRelicRarity.Legendary, FateRelicType.Utility,
                new FateRelicEffect { Stat = "time_stop", Value = 1f, Description = "1 Skill No Cooldown" });
            
            // Economic Relics - Common
            InitializeRelic("economy_001", "Lucky Coin", "Gold find +5%",
                FateRelicRarity.Common, FateRelicType.Economic,
                new FateRelicEffect { Stat = "gold_find", Value = 0.05f, Description = "+5% Gold Find" });
            
            InitializeRelic("economy_002", "Merchant's Scale", "Shop prices -3%",
                FateRelicRarity.Common, FateRelicType.Economic,
                new FateRelicEffect { Stat = "shop_discount", Value = 0.03f, Description = "-3% Shop Prices" });
            
            // Economic Relics - Uncommon
            InitializeRelic("economic_003", "Treasure Hunter", "Rare item drop rate +15%",
                FateRelicRarity.Uncommon, FateRelicType.Economic,
                new FateRelicEffect { Stat = "rare_drop", Value = 0.15f, Description = "+15% Rare Drops" });
            
            InitializeRelic("economy_004", "Golden Touch", "Gold from enemies +10%",
                FateRelicRarity.Uncommon, FateRelicType.Economic,
                new FateRelicEffect { Stat = "enemy_gold", Value = 0.10f, Description = "+10% Enemy Gold" });
            
            // Economic Relics - Rare
            InitializeRelic("economy_005", "Investment Master", "Investment returns +25%",
                FateRelicRarity.Rare, FateRelicType.Economic,
                new FateRelicEffect { Stat = "investment_return", Value = 0.25f, Description = "+25% Investment" });
            
            // Economic Relics - Epic
            InitializeRelic("economy_006", "Dragon's Hoard", "Gold find +30%, but attract more enemies",
                FateRelicRarity.Epic, FateRelicType.Economic,
                new FateRelicEffect { Stat = "gold_find", Value = 0.30f, Description = "+30% Gold Find" },
                new FateRelicEffect { Stat = "enemy_spawn", Value = 0.15f, Description = "+15% Enemy Spawn" });
            
            // Economic Relics - Legendary
            InitializeRelic("economy_007", "King's Fortune", "All currency gains +40%",
                FateRelicRarity.Legendary, FateRelicType.Economic,
                new FateRelicEffect { Stat = "all_currency", Value = 0.40f, Description = "+40% All Currency" });
            
            // Special Relics - Rare
            InitializeRelic("special_001", "Mystery Box", "Random effect each floor",
                FateRelicRarity.Rare, FateRelicType.Special,
                new FateRelicEffect { Stat = "random_effect", Value = 1f, Description = "Random Floor Effect" });
            
            // Special Relics - Epic
            InitializeRelic("special_002", "Chaos Gem", "All stats +5% but random negative effect",
                FateRelicRarity.Epic, FateRelicType.Special,
                new FateRelicEffect { Stat = "all_stats", Value = 0.05f, Description = "+5% All Stats" },
                new FateRelicEffect { Stat = "random_negative", Value = -0.10f, Description = "Random -10% Stat" });
            
            // Special Relics - Legendary
            InitializeRelic("special_003", "Wish Dragon", "One wish granted at critical moment",
                FateRelicRarity.Legendary, FateRelicType.Special,
                new FateRelicEffect { Stat = "wish_grant", Value = 1f, Description = "1 Wish Grant" });
            
            InitializeRelic("special_004", "Fate Weave", "All relic effects +20%",
                FateRelicRarity.Legendary, FateRelicType.Special,
                new FateRelicEffect { Stat = "relic_boost", Value = 0.20f, Description = "+20% All Relic Effects" });
        }
        
        public static FateRelic GetRelic(string id) {
            if (_relics.ContainsKey(id)) {
                return _relics[id];
            }
            return null;
        }
        
        public static List<FateRelic> GetAllRelics() {
            return new List<FateRelic>(_relics.Values);
        }
        
        public static List<FateRelic> GetRelicsByRarity(RelicRarity rarity) {
            var result = new List<FateRelic>();
            foreach (var relic in _relics.Values) {
                if (relic.Rarity == rarity) {
                    result.Add(relic);
                }
            }
            return result;
        }
        
        public static List<FateRelic> GetRelicsByType(RelicType type) {
            var result = new List<FateRelic>();
            foreach (var relic in _relics.Values) {
                if (relic.Type == type) {
                    result.Add(relic);
                }
            }
            return result;
        }
        
        public static FateRelic GetRandomRelic(RelicRarity rarity) {
            var relics = GetRelicsByRarity(rarity);
            if (relics.Count == 0) return null;
            
            var random = new Random();
            return relics[random.Next(relics.Count)];
        }
        
        public static FateRelic GetRandomRelicByWeight() {
            var random = new Random();
            float roll = (float)random.NextDouble();
            
            float cumulative = 0f;
            foreach (var rarity in FateRelicRarity.All) {
                cumulative += rarity.DropRate;
                if (roll <= cumulative) {
                    return GetRandomRelic(rarity);
                }
            }
            
            return GetRandomRelic(FateRelicRarity.Common);
        }
        
        public static int GetTotalRelicCount() {
            return _relics.Count;
        }
        
        public static Dictionary<string, int> GetRelicCountByRarity() {
            var counts = new Dictionary<string, int>();
            foreach (var rarity in FateRelicRarity.All) {
                counts[rarity.Name] = GetRelicsByRarity(rarity).Count;
            }
            return counts;
        }
    }
}
