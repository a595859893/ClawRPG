using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Data;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// 锦标赛数据库配置
    /// </summary>
    public static class ArenaTournamentDatabase
    {
        // 预定义锦标赛模板
        public static Dictionary<string, TournamentTemplate> Templates = new Dictionary<string, TournamentTemplate>();
        
        // 奖励配置
        public static Dictionary<string, List<TournamentReward>> RewardPools = new Dictionary<string, List<TournamentReward>>();
        
        // 赛制配置
        public static Dictionary<TournamentFormat, FormatConfig> FormatConfigs = new Dictionary<TournamentFormat, FormatConfig>();
        
        // 阶段配置
        public static Dictionary<TournamentStage, StageConfig> StageConfigs = new Dictionary<TournamentStage, StageConfig>();

        static ArenaTournamentDatabase()
        {
            InitializeTemplates();
            InitializeRewardPools();
            InitializeFormatConfigs();
            InitializeStageConfigs();
        }

        private static void InitializeTemplates()
        {
            // 每日锦标赛
            Templates["daily_arena"] = new TournamentTemplate
            {
                templateId = "daily_arena",
                name = "每日竞技场",
                description = "每日举办的竞技场比赛，所有玩家均可参加",
                format = TournamentFormat.SingleElimination,
                maxPlayers = 16,
                minPlayers = 4,
                rounds = 4,
                registrationDuration = 3600, // 1小时
                matchDuration = 600,        // 10分钟
                prizePool = 1000,
                entryFee = 100,
                allowLateJoin = false
            };

            // 周锦标赛
            Templates["weekly_championship"] = new TournamentTemplate
            {
                templateId = "weekly_championship",
                name = "周冠军赛",
                description = "每周举办的锦标赛，冠军可获得专属称号",
                format = TournamentFormat.DoubleElimination,
                maxPlayers = 32,
                minPlayers = 8,
                rounds = 6,
                registrationDuration = 7200, // 2小时
                matchDuration = 900,        // 15分钟
                prizePool = 5000,
                entryFee = 500,
                allowLateJoin = false
            };

            // 大师赛
            Templates["master_series"] = new TournamentTemplate
            {
                templateId = "master_series",
                name = "大师系列赛",
                description = "高水平玩家专用锦标赛",
                format = TournamentFormat.DoubleElimination,
                maxPlayers = 64,
                minPlayers = 16,
                rounds = 7,
                registrationDuration = 14400, // 4小时
                matchDuration = 1200,         // 20分钟
                prizePool = 20000,
                entryFee = 2000,
                allowLateJoin = false,
                requiredRank = 5 // 需要一定排名
            };

            // 练习赛
            Templates["practice_arena"] = new TournamentTemplate
            {
                templateId = "practice_arena",
                name = "练习赛场",
                description = "免费练习赛，无奖励",
                format = TournamentFormat.SingleElimination,
                maxPlayers = 8,
                minPlayers = 2,
                rounds = 3,
                registrationDuration = 1800, // 30分钟
                matchDuration = 300,        // 5分钟
                prizePool = 0,
                entryFee = 0,
                allowLateJoin = true
            };

            // 瑞士制锦标赛
            Templates["swiss_system"] = new TournamentTemplate
            {
                templateId = "swiss_system",
                name = "瑞士制公开赛",
                description = "采用瑞士制的公平竞技锦标赛",
                format = TournamentFormat.SwissSystem,
                maxPlayers = 32,
                minPlayers = 8,
                rounds = 5,
                registrationDuration = 3600,
                matchDuration = 600,
                prizePool = 3000,
                entryFee = 300,
                allowLateJoin = false
            };
        }

        private static void InitializeRewardPools()
        {
            // 小型奖励池 (8人)
            RewardPools["small"] = new List<TournamentReward>
            {
                new TournamentReward { rankStart = 1, rankEnd = 1, rewardType = "gold", rewardId = "gold", rewardAmount = 500 },
                new TournamentReward { rankStart = 2, rankEnd = 2, rewardType = "gold", rewardId = "gold", rewardAmount = 300 },
                new TournamentReward { rankStart = 3, rankEnd = 3, rewardType = "gold", rewardId = "gold", rewardAmount = 150 },
                new TournamentReward { rankStart = 4, rankEnd = 4, rewardType = "gold", rewardId = "gold", rewardAmount = 50 }
            };

            // 中型奖励池 (16人)
            RewardPools["medium"] = new List<TournamentReward>
            {
                new TournamentReward { rankStart = 1, rankEnd = 1, rewardType = "gold", rewardId = "gold", rewardAmount = 1000 },
                new TournamentReward { rankStart = 2, rankEnd = 2, rewardType = "gold", rewardId = "gold", rewardAmount = 600 },
                new TournamentReward { rankStart = 3, rankEnd = 3, rewardType = "gold", rewardId = "gold", rewardAmount = 400 },
                new TournamentReward { rankStart = 4, rankEnd = 4, rewardType = "gold", rewardId = "gold", rewardAmount = 200 },
                new TournamentReward { rankStart = 5, rankEnd = 8, rewardType = "gold", rewardId = "gold", rewardAmount = 100 }
            };

            // 大型奖励池 (32人以上)
            RewardPools["large"] = new List<TournamentReward>
            {
                new TournamentReward { rankStart = 1, rankEnd = 1, rewardType = "gold", rewardId = "gold", rewardAmount = 5000 },
                new TournamentReward { rankStart = 1, rankEnd = 1, rewardType = "title", rewardId = "champion", rewardAmount = 1 },
                new TournamentReward { rankStart = 2, rankEnd = 2, rewardType = "gold", rewardId = "gold", rewardAmount = 2500 },
                new TournamentReward { rankStart = 3, rankEnd = 3, rewardType = "gold", rewardId = "gold", rewardAmount = 1500 },
                new TournamentReward { rankStart = 4, rankEnd = 4, rewardType = "gold", rewardId = "gold", rewardAmount = 800 },
                new TournamentReward { rankStart = 5, rankEnd = 8, rewardType = "gold", rewardId = "gold", rewardAmount = 400 },
                new TournamentReward { rankStart = 9, rankEnd = 16, rewardType = "gold", rewardId = "gold", rewardAmount = 200 }
            };
        }

        private static void InitializeFormatConfigs()
        {
            FormatConfigs[TournamentFormat.SingleElimination] = new FormatConfig
            {
                format = TournamentFormat.SingleElimination,
                name = "单败淘汰",
                description = "输一场即被淘汰",
                maxRoundsMultiplier = 4, // log2(maxPlayers)
                requiresThirdPlaceMatch = true,
                supportsByes = true
            };

            FormatConfigs[TournamentFormat.DoubleElimination] = new FormatConfig
            {
                format = TournamentFormat.DoubleElimination,
                name = "双败淘汰",
                description = "输两场被淘汰",
                maxRoundsMultiplier = 7, // 2 * log2(maxPlayers) - 1
                requiresThirdPlaceMatch = false,
                supportsByes = true
            };

            FormatConfigs[TournamentFormat.RoundRobin] = new FormatConfig
            {
                format = TournamentFormat.RoundRobin,
                name = "循环赛",
                description = "每人都要交手",
                maxRoundsMultiplier = 10, // maxPlayers - 1
                requiresThirdPlaceMatch = false,
                supportsByes = false
            };

            FormatConfigs[TournamentFormat.SwissSystem] = new FormatConfig
            {
                format = TournamentFormat.SwissSystem,
                name = "瑞士制",
                description = "每轮按成绩匹配对手",
                maxRoundsMultiplier = 5, // 通常5-7轮
                requiresThirdPlaceMatch = false,
                supportsByes = false
            };
        }

        private static void InitializeStageConfigs()
        {
            StageConfigs[TournamentStage.Registration] = new StageConfig
            {
                stage = TournamentStage.Registration,
                displayName = "报名中",
                allowRegistration = true,
                allowMatches = false
            };

            StageConfigs[TournamentStage.GroupStage] = new StageConfig
            {
                stage = TournamentStage.GroupStage,
                displayName = "小组赛",
                allowRegistration = false,
                allowMatches = true
            };

            StageConfigs[TournamentStage.QuarterFinals] = new StageConfig
            {
                stage = TournamentStage.QuarterFinals,
                displayName = "四分之一决赛",
                allowRegistration = false,
                allowMatches = true
            };

            StageConfigs[TournamentStage.SemiFinals] = new StageConfig
            {
                stage = TournamentStage.SemiFinals,
                displayName = "半决赛",
                allowRegistration = false,
                allowMatches = true
            };

            StageConfigs[TournamentStage.Finals] = new StageConfig
            {
                stage = TournamentStage.Finals,
                displayName = "决赛",
                allowRegistration = false,
                allowMatches = true
            };

            StageConfigs[TournamentStage.Completed] = new StageConfig
            {
                stage = TournamentStage.Completed,
                displayName = "已结束",
                allowRegistration = false,
                allowMatches = false
            };
        }

        /// <summary>
        /// 获取模板
        /// </summary>
        public static TournamentTemplate GetTemplate(string templateId)
        {
            return Templates.ContainsKey(templateId) ? Templates[templateId] : null;
        }

        /// <summary>
        /// 获取所有模板
        /// </summary>
        public static List<TournamentTemplate> GetAllTemplates()
        {
            return new List<TournamentTemplate>(Templates.Values);
        }

        /// <summary>
        /// 根据玩家数获取合适的奖励池
        /// </summary>
        public static List<TournamentReward> GetRewardPool(int playerCount)
        {
            if (playerCount <= 8) return RewardPools["small"];
            if (playerCount <= 16) return RewardPools["medium"];
            return RewardPools["large"];
        }

        /// <summary>
        /// 获取赛制配置
        /// </summary>
        public static FormatConfig GetFormatConfig(TournamentFormat format)
        {
            return FormatConfigs.ContainsKey(format) ? FormatConfigs[format] : null;
        }

        /// <summary>
        /// 获取阶段配置
        /// </summary>
        public static StageConfig GetStageConfig(TournamentStage stage)
        {
            return StageConfigs.ContainsKey(stage) ? StageConfigs[stage] : null;
        }
    }

    /// <summary>
    /// 锦标赛模板
    /// </summary>
    public class TournamentTemplate
    {
        public string templateId;
        public string name;
        public string description;
        public TournamentFormat format;
        public int maxPlayers;
        public int minPlayers;
        public int rounds;
        public int registrationDuration;   // 报名持续时间(秒)
        public int matchDuration;          // 单场比赛时间(秒)
        public int prizePool;
        public int entryFee;
        public bool allowLateJoin;
        public int? requiredRank;
    }

    /// <summary>
    /// 赛制配置
    /// </summary>
    public class FormatConfig
    {
        public TournamentFormat format;
        public string name;
        public string description;
        public int maxRoundsMultiplier;
        public bool requiresThirdPlaceMatch;
        public bool supportsByes;
    }

    /// <summary>
    /// 阶段配置
    /// </summary>
    public class StageConfig
    {
        public TournamentStage stage;
        public string displayName;
        public bool allowRegistration;
        public bool allowMatches;
    }
}
