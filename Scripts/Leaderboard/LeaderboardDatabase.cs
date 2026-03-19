using Godot;
using System;
using System.Collections.Generic;
using Framework;

namespace ClawRPG.Scripts.Leaderboard {
    /// <summary>
    /// 排行榜配置数据库
    /// </summary>
    public class LeaderboardDatabase : BaseSystem {
        // 排行榜配置
        public Dictionary<LeaderboardType, LeaderboardConfig> Configs = new Dictionary<LeaderboardType, LeaderboardConfig>();
        
        // 排名奖励配置
        public Dictionary<LeaderboardType, List<RankReward>> RankRewards = new Dictionary<LeaderboardType, List<RankReward>>();
        
        public override void _Ready() {
            InitializeConfigs();
            InitializeRankRewards();
        }

        private void InitializeConfigs() {
            // 玩家等级排行榜
            Configs[LeaderboardType.PlayerLevel] = new LeaderboardConfig {
                Type = LeaderboardType.PlayerLevel,
                DisplayName = "Level Ranking",
                Description = "Player level leaderboard",
                Icon = "res://icons/level.png",
                MaxEntries = 1000,
                UpdateFrequency = 300f, // 5分钟
                SortDescending = true,
                ScoreFormat = "Lv.{0}"
            };

            // 金币排行榜
            Configs[LeaderboardType.Gold] = new LeaderboardConfig {
                Type = LeaderboardType.Gold,
                DisplayName = "Gold Ranking",
                Description = "Gold accumulation leaderboard",
                Icon = "res://icons/gold.png",
                MaxEntries = 1000,
                UpdateFrequency = 60f,
                SortDescending = true,
                ScoreFormat = "{0} G"
            };

            // 成就排行榜
            Configs[LeaderboardType.Achievements] = new LeaderboardConfig {
                Type = LeaderboardType.Achievements,
                DisplayName = "Achievements Ranking",
                Description = "Achievements count leaderboard",
                Icon = "res://icons/achievement.png",
                MaxEntries = 500,
                UpdateFrequency = 300f,
                SortDescending = true,
                ScoreFormat = "{0} achievements"
            };

            // 竞技场胜利排行榜
            Configs[LeaderboardType.ArenaWins] = new LeaderboardConfig {
                Type = LeaderboardType.ArenaWins,
                DisplayName = "Arena Wins",
                Description = "Arena victory leaderboard",
                Icon = "res://icons/arena.png",
                MaxEntries = 500,
                UpdateFrequency = 120f,
                SortDescending = true,
                ScoreFormat = "{0} wins"
            };

            // 地下城通关排行榜
            Configs[LeaderboardType.DungeonCompleted] = new LeaderboardConfig {
                Type = LeaderboardType.DungeonCompleted,
                DisplayName = "Dungeon Completion",
                Description = "Dungeons completed leaderboard",
                Icon = "res://icons/dungeon.png",
                MaxEntries = 500,
                UpdateFrequency = 300f,
                SortDescending = true,
                ScoreFormat = "{0} dungeons"
            };

            // Boss击杀排行榜
            Configs[LeaderboardType.BossKills] = new LeaderboardConfig {
                Type = LeaderboardType.BossKills,
                DisplayName = "Boss Kills",
                Description = "Boss elimination leaderboard",
                Icon = "res://icons/boss.png",
                MaxEntries = 500,
                UpdateFrequency = 120f,
                SortDescending = true,
                ScoreFormat = "{0} kills"
            };

            // 宠物强度排行榜
            Configs[LeaderboardType.PetStrength] = new LeaderboardConfig {
                Type = LeaderboardType.PetStrength,
                DisplayName = "Pet Power",
                Description = "Pet strength leaderboard",
                Icon = "res://icons/pet.png",
                MaxEntries = 500,
                UpdateFrequency = 600f,
                SortDescending = true,
                ScoreFormat = "Power {0}"
            };

            // 制作精通排行榜
            Configs[LeaderboardType.CraftingMastery] = new LeaderboardConfig {
                Type = LeaderboardType.CraftingMastery,
                DisplayName = "Crafting Mastery",
                Description = "Crafting mastery leaderboard",
                Icon = "res://icons/crafting.png",
                MaxEntries = 300,
                UpdateFrequency = 600f,
                SortDescending = true,
                ScoreFormat = "Mastery Lv.{0}"
            };

            // 公会积分排行榜
            Configs[LeaderboardType.GuildPoints] = new LeaderboardConfig {
                Type = LeaderboardType.GuildPoints,
                DisplayName = "Guild Points",
                Description = "Guild points leaderboard",
                Icon = "res://icons/guild.png",
                MaxEntries = 100,
                UpdateFrequency = 300f,
                SortDescending = true,
                ScoreFormat = "{0} pts"
            };

            // 跨服评分排行榜
            Configs[LeaderboardType.CrossServerRating] = new LeaderboardConfig {
                Type = LeaderboardType.CrossServerRating,
                DisplayName = "Cross-Server Rating",
                Description = "Cross-server rating leaderboard",
                Icon = "res://icons/crossserver.png",
                MaxEntries = 500,
                UpdateFrequency = 300f,
                SortDescending = true,
                ScoreFormat = "Rating {0}"
            };

            // 大秘境分数排行榜
            Configs[LeaderboardType.MythicPlusScore] = new LeaderboardConfig {
                Type = LeaderboardType.MythicPlusScore,
                DisplayName = "Mythic+ Score",
                Description = "Mythic+ score leaderboard",
                Icon = "res://icons/mythic.png",
                MaxEntries = 500,
                UpdateFrequency = 300f,
                SortDescending = true,
                ScoreFormat = "M+{0}"
            };

            // 连击链排行榜
            Configs[LeaderboardType.ComboChain] = new LeaderboardConfig {
                Type = LeaderboardType.ComboChain,
                DisplayName = "Combo Chain",
                Description = "Highest combo chain leaderboard",
                Icon = "res://icons/combo.png",
                MaxEntries = 500,
                UpdateFrequency = 60f,
                SortDescending = true,
                ScoreFormat = "{0} hits"
            };

            // 总伤害排行榜
            Configs[LeaderboardType.TotalDamage] = new LeaderboardConfig {
                Type = LeaderboardType.TotalDamage,
                DisplayName = "Total Damage",
                Description = "Total damage dealt leaderboard",
                Icon = "res://icons/damage.png",
                MaxEntries = 500,
                UpdateFrequency = 300f,
                SortDescending = true,
                ScoreFormat = "{0} DMG"
            };

            // 总治疗排行榜
            Configs[LeaderboardType.TotalHealing] = new LeaderboardConfig {
                Type = LeaderboardType.TotalHealing,
                DisplayName = "Total Healing",
                Description = "Total healing done leaderboard",
                Icon = "res://icons/healing.png",
                MaxEntries = 500,
                UpdateFrequency = 300f,
                SortDescending = true,
                ScoreFormat = "{0} HP"
            };
        }

        private void InitializeRankRewards() {
            // 竞技场胜利排行榜奖励
            RankRewards[LeaderboardType.ArenaWins] = new List<RankReward> {
                new RankReward { MinRank = 1, MaxRank = 1, GoldReward = 10000, ExpReward = 5000, Title = "Arena Champion" },
                new RankReward { MinRank = 2, MaxRank = 2, GoldReward = 7500, ExpReward = 3750, Title = "Arena Grandmaster" },
                new RankReward { MinRank = 3, MaxRank = 3, GoldReward = 5000, ExpReward = 2500, Title = "Arena Master" },
                new RankReward { MinRank = 4, MaxRank = 10, GoldReward = 2500, ExpReward = 1250, Title = "Arena Expert" },
                new RankReward { MinRank = 11, MaxRank = 50, GoldReward = 1000, ExpReward = 500, Title = "Arena Veteran" },
                new RankReward { MinRank = 51, MaxRank = 100, GoldReward = 500, ExpReward = 250, Title = "Arena Fighter" }
            };

            // 金币排行榜奖励
            RankRewards[LeaderboardType.Gold] = new List<RankReward> {
                new RankReward { MinRank = 1, MaxRank = 1, GoldReward = 50000, ExpReward = 10000, Title = "Wealthy Tycoon" },
                new RankReward { MinRank = 2, MaxRank = 3, GoldReward = 25000, ExpReward = 5000, Title = "Rich Noble" },
                new RankReward { MinRank = 4, MaxRank = 10, GoldReward = 10000, ExpReward = 2500, Title = "Merchant Prince" },
                new RankReward { MinRank = 11, MaxRank = 50, GoldReward = 5000, ExpReward = 1000, Title = "Wealthy Trader" }
            };

            // 大秘境分数排行榜奖励
            RankRewards[LeaderboardType.MythicPlusScore] = new List<RankReward> {
                new RankReward { MinRank = 1, MaxRank = 1, GoldReward = 20000, ExpReward = 10000, Title = "Mythic+ Legend" },
                new RankReward { MinRank = 2, MaxRank = 3, GoldReward = 15000, ExpReward = 7500, Title = "Mythic+ Hero" },
                new RankReward { MinRank = 4, MaxRank = 10, GoldReward = 10000, ExpReward = 5000, Title = "Mythic+ Champion" },
                new RankReward { MinRank = 11, MaxRank = 50, GoldReward = 5000, ExpReward = 2500, Title = "Mythic+ Veteran" }
            };

            // 成就排行榜奖励
            RankRewards[LeaderboardType.Achievements] = new List<RankReward> {
                new RankReward { MinRank = 1, MaxRank = 1, GoldReward = 15000, ExpReward = 7500, Title = "Achievement Master" },
                new RankReward { MinRank = 2, MaxRank = 5, GoldReward = 10000, ExpReward = 5000, Title = "Achievement Expert" },
                new RankReward { MinRank = 6, MaxRank = 20, GoldReward = 5000, ExpReward = 2500, Title = "Achievement Hunter" }
            };
        }

        public LeaderboardConfig GetConfig(LeaderboardType type) {
            return Configs.ContainsKey(type) ? Configs[type] : null;
        }

        public List<RankReward> GetRankRewards(LeaderboardType type) {
            return RankRewards.ContainsKey(type) ? RankRewards[type] : new List<RankReward>();
        }

        public RankReward GetRewardForRank(LeaderboardType type, int rank) {
            var rewards = GetRankRewards(type);
            foreach (var reward in rewards) {
                if (rank >= reward.MinRank && rank <= reward.MaxRank) {
                    return reward;
                }
            }
            return null;
        }

        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // 导出排行榜配置（如果有任何运行时修改）
            var configsData = new List<Dictionary<string, object>>();
            foreach (var kvp in Configs)
            {
                configsData.Add(new Dictionary<string, object>
                {
                    { "type", (int)kvp.Key },
                    { "max_entries", kvp.Value.MaxEntries },
                    { "update_frequency", kvp.Value.UpdateFrequency }
                });
            }
            data["configs"] = configsData;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            // 导入排行榜配置
            if (data.ContainsKey("configs") && data["configs"] is List<object> configsList)
            {
                foreach (var item in configsList)
                {
                    if (item is Dictionary<string, object> configDict)
                    {
                        var type = (LeaderboardType)(int)configDict["type"];
                        if (Configs.ContainsKey(type))
                        {
                            var config = Configs[type];
                            config.MaxEntries = (int)configDict.GetValueOrDefault("max_entries", config.MaxEntries);
                            config.UpdateFrequency = (float)configDict.GetValueOrDefault("update_frequency", config.UpdateFrequency);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 排行榜配置
    /// </summary>
    public class LeaderboardConfig {
        public LeaderboardType Type;
        public string DisplayName;
        public string Description;
        public string Icon;
        public int MaxEntries;
        public float UpdateFrequency;
        public bool SortDescending;
        public string ScoreFormat;
    }

    /// <summary>
    /// 排名奖励
    /// </summary>
    public class RankReward {
        public int MinRank;
        public int MaxRank;
        public int GoldReward;
        public int ExpReward;
        public string Title;
    }
}
