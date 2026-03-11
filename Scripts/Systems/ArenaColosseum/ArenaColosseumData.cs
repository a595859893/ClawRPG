using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 角斗场数据结构 - 玩家实时对战
    /// </summary>
    public class ArenaColosseumData
    {
        // 竞技场类型
        public enum ColosseumType
        {
            SoloDuel,      // 1v1 单挑
            TeamArena,    // 3v3 团队战
            FreeForAll,   // 大乱斗
            MountCombat,  // 坐骑战
            PetBattle     // 宠物战
        }

        // 竞技场状态
        public enum ColosseumState
        {
            Waiting,      // 等待对手
            Matching,     // 匹配中
            Countdown,    // 倒计时开始
            InProgress,   // 战斗中
            Completed,    // 战斗结束
            Cancelled     // 已取消
        }

        // 竞技场配置
        public class Colosseum
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public ColosseumType Type { get; set; }
            public int MinLevel { get; set; }
            public int MaxPlayers { get; set; }
            public int EntryFee { get; set; }
            public int PrizePool { get; set; }
            public float Duration { get; set; } // 战斗时长（秒）
            public int WinnerReward { get; set; }
            public int LoserReward { get; set; }
        }

        // 参与者数据
        public class Participant
        {
            public int PlayerId { get; set; }
            public string PlayerName { get; set; }
            public int Level { get; set; }
            public int Health { get; set; }
            public int MaxHealth { get; set; }
            public int Damage { get; set; }
            public int Wins { get; set; }
            public int Losses { get; set; }
            public bool IsReady { get; set; }
            public Vector2 Position { get; set; }
            public bool IsAlive { get; set; }
            public int Score { get; set; }
        }

        // 活跃竞技场实例
        public class ActiveColosseum
        {
            public int InstanceId { get; set; }
            public int ColosseumId { get; set; }
            public ColosseumState State { get; set; }
            public List<Participant> Participants { get; set; } = new List<Participant>();
            public float TimeRemaining { get; set; }
            public float CountdownTime { get; set; }
            public int Round { get; set; }
            public int WinnerId { get; set; }
            public DateTime StartTime { get; set; }
        }

        // 玩家角斗场数据
        public class PlayerColosseumData
        {
            public int PlayerId { get; set; }
            public int TotalMatches { get; set; }
            public int Wins { get; set; }
            public int Losses { get; set; }
            public int TotalPrizeEarned { get; set; }
            public int TotalEntryFees { get; set; }
            public int HighestStreak { get; set; }
            public int CurrentStreak { get; set; }
            public int HighestDamage { get; set; }
            public int TotalKills { get; set; }
            public List<ColosseumRecord> History { get; set; } = new List<ColosseumRecord>();
            public int Rating { get; set; } // 竞技积分
        }

        // 角斗场记录
        public class ColosseumRecord
        {
            public int ColosseumId { get; set; }
            public ColosseumType Type { get; set; }
            public bool IsWinner { get; set; }
            public int DamageDealt { get; set; }
            public int Kills { get; set; }
            public int PrizeEarned { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }

    /// <summary>
    /// 角斗场数据库
    /// </summary>
    public class ArenaColosseumDatabase
    {
        public static List<ArenaColosseumData.Colosseum> GetDefaultColosseums()
        {
            return new List<ArenaColosseumData.Colosseum>
            {
                // Solo Duel - 1v1
                new ArenaColosseumData.Colosseum
                {
                    Id = 1,
                    Name = "练习场",
                    Description = "1v1单挑练习战",
                    Type = ArenaColosseumData.ColosseumType.SoloDuel,
                    MinLevel = 1,
                    MaxPlayers = 2,
                    EntryFee = 0,
                    PrizePool = 0,
                    Duration = 120f,
                    WinnerReward = 0,
                    LoserReward = 0
                },
                new ArenaColosseumData.Colosseum
                {
                    Id = 2,
                    Name = "青铜角斗场",
                    Description = "1v1单挑青铜段位赛",
                    Type = ArenaColosseumData.ColosseumType.SoloDuel,
                    MinLevel = 10,
                    MaxPlayers = 2,
                    EntryFee = 100,
                    PrizePool = 200,
                    Duration = 180f,
                    WinnerReward = 180,
                    LoserReward = 20
                },
                new ArenaColosseumData.Colosseum
                {
                    Id = 3,
                    Name = "白银角斗场",
                    Description = "1v1单挑白银段位赛",
                    Type = ArenaColosseumData.ColosseumType.SoloDuel,
                    MinLevel = 20,
                    MaxPlayers = 2,
                    EntryFee = 500,
                    PrizePool = 1000,
                    Duration = 180f,
                    WinnerReward = 900,
                    LoserReward = 50
                },
                new ArenaColosseumData.Colosseum
                {
                    Id = 4,
                    Name = "黄金角斗场",
                    Description = "1v1单挑黄金段位赛",
                    Type = ArenaColosseumData.ColosseumType.SoloDuel,
                    MinLevel = 30,
                    MaxPlayers = 2,
                    EntryFee = 2000,
                    PrizePool = 4000,
                    Duration = 180f,
                    WinnerReward = 3600,
                    LoserReward = 200
                },
                // Team Arena - 3v3
                new ArenaColosseumData.Colosseum
                {
                    Id = 5,
                    Name = "团队练习赛",
                    Description = "3v3团队练习战",
                    Type = ArenaColosseumData.ColosseumType.TeamArena,
                    MinLevel = 5,
                    MaxPlayers = 6,
                    EntryFee = 0,
                    PrizePool = 0,
                    Duration = 300f,
                    WinnerReward = 0,
                    LoserReward = 0
                },
                new ArenaColosseumData.Colosseum
                {
                    Id = 6,
                    Name = "公会团战",
                    Description = "3v3公会团队战",
                    Type = ArenaColosseumData.ColosseumType.TeamArena,
                    MinLevel = 15,
                    MaxPlayers = 6,
                    EntryFee = 300,
                    PrizePool = 1800,
                    Duration = 300f,
                    WinnerReward = 600,
                    LoserReward = 100
                },
                // Free For All
                new ArenaColosseumData.Colosseum
                {
                    Id = 7,
                    Name = "大乱斗",
                    Description = "8人混战，最后存活者获胜",
                    Type = ArenaColosseumData.ColosseumType.FreeForAll,
                    MinLevel = 10,
                    MaxPlayers = 8,
                    EntryFee = 200,
                    PrizePool = 1600,
                    Duration = 240f,
                    WinnerReward = 1000,
                    LoserReward = 0
                },
                new ArenaColosseumData.Colosseum
                {
                    Id = 8,
                    Name = "死亡竞技场",
                    Description = "12人混战，一击必杀",
                    Type = ArenaColosseumData.ColosseumType.FreeForAll,
                    MinLevel = 25,
                    MaxPlayers = 12,
                    EntryFee = 500,
                    PrizePool = 6000,
                    Duration = 180f,
                    WinnerReward = 4000,
                    LoserReward = 0
                },
                // Mount Combat
                new ArenaColosseumData.Colosseum
                {
                    Id = 9,
                    Name = "骑战练习场",
                    Description = "坐骑对战练习",
                    Type = ArenaColosseumData.ColosseumType.MountCombat,
                    MinLevel = 5,
                    MaxPlayers = 4,
                    EntryFee = 0,
                    PrizePool = 0,
                    Duration = 180f,
                    WinnerReward = 0,
                    LoserReward = 0
                },
                new ArenaColosseumData.Colosseum
                {
                    Id = 10,
                    Name = "骑战锦标赛",
                    Description = "坐骑对战锦标赛",
                    Type = ArenaColosseumData.ColosseumType.MountCombat,
                    MinLevel = 20,
                    MaxPlayers = 4,
                    EntryFee = 800,
                    PrizePool = 3200,
                    Duration = 180f,
                    WinnerReward = 2400,
                    LoserReward = 100
                },
                // Pet Battle
                new ArenaColosseumData.Colosseum
                {
                    Id = 11,
                    Name = "宠物对战练习",
                    Description = "宠物对战练习赛",
                    Type = ArenaColosseumData.ColosseumType.PetBattle,
                    MinLevel = 1,
                    MaxPlayers = 4,
                    EntryFee = 0,
                    PrizePool = 0,
                    Duration = 120f,
                    WinnerReward = 0,
                    LoserReward = 0
                },
                new ArenaColosseumData.Colosseum
                {
                    Id = 12,
                    Name = "宠物大师赛",
                    Description = "宠物对战大师赛",
                    Type = ArenaColosseumData.ColosseumType.PetBattle,
                    MinLevel = 15,
                    MaxPlayers = 4,
                    EntryFee = 600,
                    PrizePool = 2400,
                    Duration = 180f,
                    WinnerReward = 1800,
                    LoserReward = 50
                }
            };
        }

        public static ArenaColosseumData.Colosseum GetColosseum(int id)
        {
            var colosseums = GetDefaultColosseums();
            foreach (var c in colosseums)
            {
                if (c.Id == id) return c;
            }
            return null;
        }

        public static List<ArenaColosseumData.Colosseum> GetColosseumsByType(ArenaColosseumData.ColosseumType type)
        {
            var result = new List<ArenaColosseumData.Colosseum>();
            var colosseums = GetDefaultColosseums();
            foreach (var c in colosseums)
            {
                if (c.Type == type) result.Add(c);
            }
            return result;
        }
    }
}
