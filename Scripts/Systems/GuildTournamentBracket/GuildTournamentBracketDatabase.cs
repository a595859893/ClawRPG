using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.GuildTournamentBracket {
    /// <summary>
    /// 公会锦标赛赛程配置数据库
    /// </summary>
    public static class GuildTournamentBracketDatabase {
        // 赛制配置
        public static Dictionary<TournamentFormat, TournamentFormatConfig> FormatConfigs { get; private set; }
        
        // 赛制类型
        public enum TournamentFormat {
            SingleElimination,  // 单败淘汰
            DoubleElimination,   // 双败淘汰
            RoundRobin,         // 循环赛
            Swiss               // 瑞士制
        }
        
        // 赛制配置
        public class TournamentFormatConfig {
            public string Name { get; set; }
            public string Description { get; set; }
            public int MinTeams { get; set; }
            public int MaxTeams { get; set; }
            public int RoundsForTeams(int teamCount) {
                switch (Name) {
                    case "Single Elimination":
                        return (int)Math.Ceiling(Math.Log2(teamCount));
                    case "Double Elimination":
                        return (int)Math.Ceiling(Math.Log2(teamCount)) * 2 - 1;
                    case "Round Robin":
                        return teamCount - 1;
                    case "Swiss":
                        return (int)Math.Ceiling(Math.Log2(teamCount));
                    default:
                        return teamCount - 1;
                }
            }
        }
        
        // 种子配置
        public static Dictionary<int, int> SeedPowers { get; private set; }
        
        // 奖励配置
        public static Dictionary<int, TournamentRewardConfig> RankingRewards { get; private set; }
        
        static GuildTournamentBracketDatabase() {
            InitializeFormatConfigs();
            InitializeSeedPowers();
            InitializeRankingRewards();
        }
        
        private static void InitializeFormatConfigs() {
            FormatConfigs = new Dictionary<TournamentFormat, TournamentFormatConfig>();
            
            FormatConfigs[TournamentFormat.SingleElimination] = new TournamentFormatConfig {
                Name = "Single Elimination",
                Description = "单败淘汰赛，输一场即被淘汰",
                MinTeams = 4,
                MaxTeams = 32
            };
            
            FormatConfigs[TournamentFormat.DoubleElimination] = new TournamentFormatConfig {
                Name = "Double Elimination",
                Description = "双败淘汰赛，输两场被淘汰",
                MinTeams = 4,
                MaxTeams = 32
            };
            
            FormatConfigs[TournamentFormat.RoundRobin] = new TournamentFormatConfig {
                Name = "Round Robin",
                Description = "循环赛，每队与其他队各交手一次",
                MinTeams = 3,
                MaxTeams = 16
            };
            
            FormatConfigs[TournamentFormat.Swiss] = new TournamentFormatConfig {
                Name = "Swiss",
                Description = "瑞士制，每轮根据战绩匹配对手",
                MinTeams = 4,
                MaxTeams = 32
            };
        }
        
        private static void InitializeSeedPowers() {
            SeedPowers = new Dictionary<int, int>();
            // 种子排位对应的实力加成
            SeedPowers[1] = 100;   // 1号种子
            SeedPowers[2] = 95;
            SeedPowers[3] = 90;
            SeedPowers[4] = 88;
            SeedPowers[5] = 85;
            SeedPowers[6] = 82;
            SeedPowers[7] = 80;
            SeedPowers[8] = 78;
            SeedPowers[9] = 75;
            SeedPowers[10] = 72;
            SeedPowers[11] = 70;
            SeedPowers[12] = 68;
            SeedPowers[13] = 65;
            SeedPowers[14] = 62;
            SeedPowers[15] = 60;
            SeedPowers[16] = 58;
            // 16名之后统一为55
            for (int i = 17; i <= 32; i++) {
                SeedPowers[i] = 55;
            }
        }
        
        private static void InitializeRankingRewards() {
            RankingRewards = new Dictionary<int, TournamentRewardConfig>();
            
            // 第1名奖励
            RankingRewards[1] = new TournamentRewardConfig {
                Rank = 1,
                GoldReward = 50000,
                ExperienceReward = 10000,
                TournamentPoints = 1000,
                Title = "Champion"
            };
            
            // 第2名奖励
            RankingRewards[2] = new TournamentRewardConfig {
                Rank = 2,
                GoldReward = 30000,
                ExperienceReward = 7500,
                TournamentPoints = 750,
                Title = "Runner-up"
            };
            
            // 第3名奖励
            RankingRewards[3] = new TournamentRewardConfig {
                Rank = 3,
                GoldReward = 20000,
                ExperienceReward = 5000,
                TournamentPoints = 500,
                Title = "Third Place"
            };
            
            // 第4名奖励
            RankingRewards[4] = new TournamentRewardConfig {
                Rank = 4,
                GoldReward = 15000,
                ExperienceReward = 3500,
                TournamentPoints = 350,
                Title = ""
            };
            
            // 5-8名奖励
            for (int i = 5; i <= 8; i++) {
                RankingRewards[i] = new TournamentRewardConfig {
                    Rank = i,
                    GoldReward = 10000 - (i - 5) * 1500,
                    ExperienceReward = 2500 - (i - 5) * 400,
                    TournamentPoints = 250 - (i - 5) * 40,
                    Title = ""
                };
            }
            
            // 9-16名奖励
            for (int i = 9; i <= 16; i++) {
                RankingRewards[i] = new TournamentRewardConfig {
                    Rank = i,
                    GoldReward = 5000 - (i - 9) * 600,
                    ExperienceReward = 1500 - (i - 9) * 150,
                    TournamentPoints = 150 - (i - 9) * 15,
                    Title = ""
                };
            }
        }
        
        /// <summary>
        /// 获取赛制配置
        /// </summary>
        public static TournamentFormatConfig GetFormatConfig(TournamentFormat format) {
            if (FormatConfigs.ContainsKey(format)) {
                return FormatConfigs[format];
            }
            return FormatConfigs[TournamentFormat.SingleElimination];
        }
        
        /// <summary>
        /// 获取排名奖励
        /// </summary>
        public static TournamentRewardConfig GetRankReward(int rank) {
            if (RankingRewards.ContainsKey(rank)) {
                return RankingRewards[rank];
            }
            return new TournamentRewardConfig {
                Rank = rank,
                GoldReward = Math.Max(1000, 5000 - rank * 300),
                ExperienceReward = Math.Max(500, 1500 - rank * 80),
                TournamentPoints = Math.Max(50, 150 - rank * 8),
                Title = ""
            };
        }
        
        /// <summary>
        /// 获取种子加成
        /// </summary>
        public static int GetSeedPower(int seed) {
            if (SeedPowers.ContainsKey(seed)) {
                return SeedPowers[seed];
            }
            return 50;
        }
    }
    
    /// <summary>
    /// 赛制配置
    /// </summary>
    public class TournamentFormatConfig {
        public string Name { get; set; }
        public string Description { get; set; }
        public int MinTeams { get; set; }
        public int MaxTeams { get; set; }
    }
    
    /// <summary>
    /// 锦标赛奖励配置
    /// </summary>
    public class TournamentRewardConfig {
        public int Rank { get; set; }
        public int GoldReward { get; set; }
        public int ExperienceReward { get; set; }
        public int TournamentPoints { get; set; }
        public string Title { get; set; }
    }
}
