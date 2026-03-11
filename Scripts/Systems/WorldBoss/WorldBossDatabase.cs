using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.WorldBoss;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// World boss database configuration
    /// </summary>
    public static class WorldBossDatabase
    {
        private static Dictionary<string, WorldBossData.WorldBoss> _bosses;
        private static bool _initialized = false; 
        
        public static void Initialize()
        {
            if (_initialized) return;
            
            _bosses = new Dictionary<string, WorldBossData.WorldBoss>();
            
            // Elite bosses
            AddBoss(new WorldBossData.WorldBoss
            {
                Id = "forest_troll",
                Name = "森林巨魔",
                Description = "居住在森林深处的强大生物，攻击力极强",
                Rarity = WorldBossData.BossRarity.Elite,
                Level = 15,
                Health = 5000,
                Attack = 150,
                Defense = 30,
                MoveSpeed = 2.5f,
                Skills = new List<string> { "ground_slam", "enrage" },
                SpawnType = WorldBossData.SpawnCondition.Timer,
                SpawnIntervalMinutes = 30,
                MinPlayers = 1,
                GoldReward = 500,
                ExpReward = 300,
                ItemRewards = new List<string> { "troll_hammer", "forest_token" },
                SpawnRadius = 800f,
                AttackRange = 120,
                AttackCooldown = 2.5f
            });
            
            AddBoss(new WorldBossData.WorldBoss
            {
                Id = "cave_spider",
                Name = "洞穴蜘蛛女王",
                Description = "地下洞穴中的恐怖生物，能够召唤小蜘蛛",
                Rarity = WorldBossData.BossRarity.Elite,
                Level = 20,
                Health = 8000,
                Attack = 120,
                Defense = 40,
                MoveSpeed = 2.2f,
                Skills = new List<string> { "web_shot", "summon_minions", "poison_bite" },
                SpawnType = WorldBossData.SpawnCondition.Timer,
                SpawnIntervalMinutes = 45,
                MinPlayers = 1,
                GoldReward = 800,
                ExpReward = 500,
                ItemRewards = new List<string> { "spider_silk", "poison_gland" },
                SpawnRadius = 600f,
                AttackRange = 200,
                AttackCooldown = 1.8f
            });
            
            // Rare bosses
            AddBoss(new WorldBossData.WorldBoss
            {
                Id = "desert_scorpion",
                Name = "沙漠蝎子王",
                Description = "沙漠中的致命猎手，毒性猛烈",
                Rarity = WorldBossData.BossRarity.Rare,
                Level = 25,
                Health = 15000,
                Attack = 200,
                Defense = 50,
                MoveSpeed = 3.0f,
                Skills = new List<string> { "sting", "sandstorm", "armor_shed" },
                SpawnType = WorldBossData.SpawnCondition.Timer,
                SpawnIntervalMinutes = 60,
                MinPlayers = 2,
                GoldReward = 1500,
                ExpReward = 1000,
                ItemRewards = new List<string> { "scorpion_claw", "desert_gem" },
                SpawnRadius = 1000f,
                AttackRange = 150,
                AttackCooldown = 1.5f
            });
            
            AddBoss(new WorldBossData.WorldBoss
            {
                Id = "mountain_golem",
                Name = "山岭巨人",
                Description = "由岩石构成的远古守卫，防御力极高",
                Rarity = WorldBossData.BossRarity.Rare,
                Level = 30,
                Health = 25000,
                Attack = 180,
                Defense = 100,
                MoveSpeed = 1.5f,
                Skills = new List<string> { "rock_throw", "earthquake", "stone_skin" },
                SpawnType = WorldBossData.SpawnCondition.Timer,
                SpawnIntervalMinutes = 90,
                MinPlayers = 2,
                GoldReward = 2000,
                ExpReward = 1500,
                ItemRewards = new List<string> { "golem_core", "mountain_crystal" },
                SpawnRadius = 1200f,
                AttackRange = 180,
                AttackCooldown = 3.0f
            });
            
            // Epic bosses
            AddBoss(new WorldBossData.WorldBoss
            {
                Id = "fire_drake",
                Name = "火焰翼龙",
                Description = "拥有喷吐火焰能力的强大龙类",
                Rarity = WorldBossData.BossRarity.Epic,
                Level = 35,
                Health = 40000,
                Attack = 300,
                Defense = 80,
                MoveSpeed = 4.0f,
                Skills = new List<string> { "fire_breath", "fly_attack", "flame_shield" },
                SpawnType = WorldBossData.SpawnCondition.Timer,
                SpawnIntervalMinutes = 120,
                MinPlayers = 3,
                GoldReward = 5000,
                ExpReward = 3000,
                ItemRewards = new List<string> { "drake_scale", "fire_essence", "dragon_heart" },
                SpawnRadius = 1500f,
                AttackRange = 300,
                AttackCooldown = 4.0f
            });
            
            AddBoss(new WorldBossData.WorldBoss
            {
                Id = "frost_wraith",
                Name = "冰霜幽灵",
                Description = "极寒之地游荡的怨灵，能够冰冻敌人",
                Rarity = WorldBossData.BossRarity.Epic,
                Level = 38,
                Health = 35000,
                Attack = 280,
                Defense = 60,
                MoveSpeed = 3.5f,
                Skills = new List<string> { "ice_lance", "frozen_field", "soul_drain" },
                SpawnType = WorldBossData.SpawnCondition.Timer,
                SpawnIntervalMinutes = 150,
                MinPlayers = 3,
                GoldReward = 4500,
                ExpReward = 2800,
                ItemRewards = new List<string> { "frost_crystal", "soul_shard", "wraith_cloak" },
                SpawnRadius = 1400f,
                AttackRange = 250,
                AttackCooldown = 2.0f
            });
            
            // Legendary bosses
            AddBoss(new WorldBossData.WorldBoss
            {
                Id = "thunder_titan",
                Name = "雷电泰坦",
                Description = "掌控雷电之力的远古巨人",
                Rarity = WorldBossData.BossRarity.Legendary,
                Level = 42,
                Health = 80000,
                Attack = 400,
                Defense = 120,
                MoveSpeed = 2.8f,
                Skills = new List<string> { "thunder_strike", "lightning_storm", "static_field", "divine_wrath" },
                SpawnType = WorldBossData.SpawnCondition.Timer,
                SpawnIntervalMinutes = 180,
                MinPlayers = 5,
                GoldReward = 15000,
                ExpReward = 10000,
                ItemRewards = new List<string> { "titan_core", "thunder_orb", "lightning_hammer", "divine_token" },
                SpawnRadius = 2000f,
                AttackRange = 350,
                AttackCooldown = 5.0f
            });
            
            AddBoss(new WorldBossData.WorldBoss
            {
                Id = "dark_shadowlord",
                Name = "暗影领主",
                Description = "统领暗影军团的邪恶存在",
                Rarity = WorldBossData.BossRarity.Legendary,
                Level = 45,
                Health = 100000,
                Attack = 450,
                Defense = 100,
                MoveSpeed = 3.2f,
                Skills = new List<string> { "shadow_blade", "dark_void", "soul_harvest", "immortal_form" },
                SpawnType = WorldBossData.SpawnCondition.Timer,
                SpawnIntervalMinutes = 240,
                MinPlayers = 5,
                GoldReward = 20000,
                ExpReward = 15000,
                ItemRewards = new List<string> { "shadow_crown", "void_essence", "soul_gem", "dark_blade" },
                SpawnRadius = 1800f,
                AttackRange = 200,
                AttackCooldown = 3.5f
            });
            
            // Mythic bosses
            AddBoss(new WorldBossData.WorldBoss
            {
                Id = "world_dragon",
                Name = "世界之龙",
                Description = "创世之初便存在的超级巨龙，掌握所有元素之力",
                Rarity = WorldBossData.BossRarity.Mythic,
                Level = 50,
                Health = 500000,
                Attack = 600,
                Defense = 200,
                MoveSpeed = 5.0f,
                Skills = new List<string> { "elemental_breath", "meteor_strike", "time_stop", "world_ender" },
                SpawnType = WorldBossData.SpawnCondition.EventTrigger,
                SpawnIntervalMinutes = 480,
                MinPlayers = 10,
                GoldReward = 100000,
                ExpReward = 50000,
                ItemRewards = new List<string> { "dragon_egg", "world_core", "eternal_crystal", "god_slayer", "legendary_mount" },
                SpawnRadius = 3000f,
                AttackRange = 500,
                AttackCooldown = 8.0f
            });
            
            AddBoss(new WorldBossData.WorldBoss
            {
                Id = "chaos_beast",
                Name = "混沌巨兽",
                Description = "来自异界的恐怖生物，能够扭曲现实",
                Rarity = WorldBossData.BossRarity.Mythic,
                Level = 50,
                Health = 400000,
                Attack = 550,
                Defense = 150,
                MoveSpeed = 4.5f,
                Skills = new List<string> { "chaos_bolt", "reality_break", "dark_matter", "chaos_avatar" },
                SpawnType = WorldBossData.SpawnCondition.Random,
                SpawnIntervalMinutes = 360,
                MinPlayers = 8,
                GoldReward = 80000,
                ExpReward = 40000,
                ItemRewards = new List<string> { "chaos_orb", "reality_shard", "abyss_key", "corrupted_artifact" },
                SpawnRadius = 2500f,
                AttackRange = 400,
                AttackCooldown = 6.0f
            });
            
            _initialized = true;
        }
        
        private static void AddBoss(WorldBossData.WorldBoss boss)
        {
            _bosses[boss.Id] = boss;
        }
        
        public static WorldBossData.WorldBoss GetBoss(string bossId)
        {
            Initialize();
            return _bosses.ContainsKey(bossId) ? _bosses[bossId] : null;
        }
        
        public static List<WorldBossData.WorldBoss> GetAllBosses()
        {
            Initialize();
            return new List<WorldBossData.WorldBoss>(_bosses.Values);
        }
        
        public static List<WorldBossData.WorldBoss> GetBossesByRarity(WorldBossData.BossRarity rarity)
        {
            Initialize();
            List<WorldBossData.WorldBoss> result = new List<WorldBossData.WorldBoss>();
            foreach (var boss in _bosses.Values)
            {
                if (boss.Rarity == rarity)
                    result.Add(boss);
            }
            return result;
        }
        
        public static List<WorldBossData.WorldBoss> GetAvailableBosses(int playerLevel)
        {
            Initialize();
            List<WorldBossData.WorldBoss> result = new List<WorldBossData.WorldBoss>();
            foreach (var boss in _bosses.Values)
            {
                if (boss.Level <= playerLevel + 10)
                    result.Add(boss);
            }
            return result;
        }
        
        public static WorldBossData.WorldBoss GetRandomBoss(WorldBossData.BossRarity minRarity)
        {
            Initialize();
            List<WorldBossData.WorldBoss> candidates = new List<WorldBossData.WorldBoss>();
            foreach (var boss in _bosses.Values)
            {
                if (boss.Rarity >= minRarity)
                    candidates.Add(boss);
            }
            if (candidates.Count == 0) return null;
            
            int index = new Random().Next(candidates.Count);
            return candidates[index];
        }
        
        public static int GetRarityColor(WorldBossData.BossRarity rarity)
        {
            switch (rarity)
            {
                case WorldBossData.BossRarity.Elite: return 0x808080;     // Gray
                case WorldBossData.BossRarity.Rare: return 0x0080FF;      // Blue
                case WorldBossData.BossRarity.Epic: return 0x8000FF;       // Purple
                case WorldBossData.BossRarity.Legendary: return 0xFF8000;  // Orange
                case WorldBossData.BossRarity.Mythic: return 0xFF0000;      // Red
                default: return 0xFFFFFF;
            }
        }
        
        public static float GetRarityMultiplier(WorldBossData.BossRarity rarity)
        {
            switch (rarity)
            {
                case WorldBossData.BossRarity.Elite: return 1.0f;
                case WorldBossData.BossRarity.Rare: return 1.5f;
                case WorldBossData.BossRarity.Epic: return 2.5f;
                case WorldBossData.BossRarity.Legendary: return 5.0f;
                case WorldBossData.BossRarity.Mythic: return 10.0f;
                default: return 1.0f;
            }
        }
    }
}
