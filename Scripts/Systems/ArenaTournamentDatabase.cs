using Godot;
using System;
using System.Collections.Generic;

public class ArenaTournamentDatabase
{
    private static ArenaTournamentDatabase _instance;
    public static ArenaTournamentDatabase Instance
    {
        get
        {
            if (_instance == null) _instance = new ArenaTournamentDatabase();
            return _instance;
        }
    }

    public List<ArenaTournamentData.Tournament> GetDefaultTournaments()
    {
        return new List<ArenaTournamentData.Tournament>
        {
            // Solo Duel Tournaments
            new ArenaTournamentData.Tournament
            {
                Id = "tournament_solo_bronze",
                Name = "青铜杯个人赛",
                Description = "青铜段位玩家的专属竞技场",
                Type = ArenaTournamentData.TournamentType.SoloDuel,
                MaxParticipants = 16,
                MinLevel = 1,
                EntryFee = 100,
                PrizePool = 1000,
                RoundCount = 4,
                ParticipantsPerMatch = 2,
                MatchDuration = 180f
            },
            new ArenaTournamentData.Tournament
            {
                Id = "tournament_solo_silver",
                Name = "白银杯个人赛",
                Description = "白银段位玩家的竞技对决",
                Type = ArenaTournamentData.TournamentType.SoloDuel,
                MaxParticipants = 16,
                MinLevel = 20,
                EntryFee = 500,
                PrizePool = 5000,
                RoundCount = 4,
                ParticipantsPerMatch = 2,
                MatchDuration = 180f
            },
            new ArenaTournamentData.Tournament
            {
                Id = "tournament_solo_gold",
                Name = "黄金杯个人赛",
                Description = "黄金段位强者的巅峰对决",
                Type = ArenaTournamentData.TournamentType.SoloDuel,
                MaxParticipants = 32,
                MinLevel = 40,
                EntryFee = 2000,
                PrizePool = 30000,
                RoundCount = 5,
                ParticipantsPerMatch = 2,
                MatchDuration = 180f
            },
            new ArenaTournamentData.Tournament
            {
                Id = "tournament_solo_diamond",
                Name = "钻石杯冠军赛",
                Description = "顶尖玩家的王者之战",
                Type = ArenaTournamentData.TournamentType.SoloDuel,
                MaxParticipants = 64,
                MinLevel = 60,
                EntryFee = 10000,
                PrizePool = 200000,
                RoundCount = 6,
                ParticipantsPerMatch = 2,
                MatchDuration = 240f
            },
            
            // Team Battle Tournaments
            new ArenaTournamentData.Tournament
            {
                Id = "tournament_team_3v3",
                Name = "3v3团队赛",
                Description = "三人小队的团队竞技",
                Type = ArenaTournamentData.TournamentType.TeamBattle,
                MaxParticipants = 24,
                MinLevel = 30,
                EntryFee = 1500,
                PrizePool = 20000,
                RoundCount = 4,
                ParticipantsPerMatch = 6,
                MatchDuration = 300f
            },
            new ArenaTournamentData.Tournament
            {
                Id = "tournament_team_5v5",
                Name = "5v5公会战",
                Description = "公会之间的巅峰对决",
                Type = ArenaTournamentData.TournamentType.TeamBattle,
                MaxParticipants = 40,
                MinLevel = 50,
                EntryFee = 5000,
                PrizePool = 100000,
                RoundCount = 4,
                ParticipantsPerMatch = 10,
                MatchDuration = 360f
            },

            // Free For All
            new ArenaTournamentData.Tournament
            {
                Id = "tournament_ffa_8",
                Name = "大乱斗(8人)",
                Description = "八人混战，最后存活者获胜",
                Type = ArenaTournamentData.TournamentType.FreeForAll,
                MaxParticipants = 8,
                MinLevel = 25,
                EntryFee = 800,
                PrizePool = 5000,
                RoundCount = 1,
                ParticipantsPerMatch = 8,
                MatchDuration = 300f
            },
            new ArenaTournamentData.Tournament
            {
                Id = "tournament_ffa_16",
                Name = "大乱斗(16人)",
                Description = "十六人混战，胜者为王",
                Type = ArenaTournamentData.TournamentType.FreeForAll,
                MaxParticipants = 16,
                MinLevel = 45,
                EntryFee = 3000,
                PrizePool = 25000,
                RoundCount = 1,
                ParticipantsPerMatch = 16,
                MatchDuration = 360f
            },

            // Mount Combat Tournaments
            new ArenaTournamentData.Tournament
            {
                Id = "tournament_mount_race",
                Name = "坐骑竞速赛",
                Description = "骑乘坐骑的速度对决",
                Type = ArenaTournamentData.TournamentType.MountCombat,
                MaxParticipants = 20,
                MinLevel = 20,
                EntryFee = 500,
                PrizePool = 8000,
                RoundCount = 4,
                ParticipantsPerMatch = 4,
                MatchDuration = 120f
            },
            new ArenaTournamentData.Tournament
            {
                Id = "tournament_mount_combat",
                Name = "坐骑战斗赛",
                Description = "骑乘坐骑进行战斗",
                Type = ArenaTournamentData.TournamentType.MountCombat,
                MaxParticipants = 16,
                MinLevel = 35,
                EntryFee = 2000,
                PrizePool = 20000,
                RoundCount = 4,
                ParticipantsPerMatch = 2,
                MatchDuration = 180f
            },

            // Pet Battle Tournaments
            new ArenaTournamentData.Tournament
            {
                Id = "tournament_pet_1v1",
                Name = "宠物1v1排位赛",
                Description = "宠物单独作战",
                Type = ArenaTournamentData.TournamentType.PetBattle,
                MaxParticipants = 16,
                MinLevel = 15,
                EntryFee = 300,
                PrizePool = 3000,
                RoundCount = 4,
                ParticipantsPerMatch = 2,
                MatchDuration = 180f
            },
            new ArenaTournamentData.Tournament
            {
                Id = "tournament_pet_3v3",
                Name = "宠物3v3团队赛",
                Description = "三只宠物协同作战",
                Type = ArenaTournamentData.TournamentType.PetBattle,
                MaxParticipants = 24,
                MinLevel = 30,
                EntryFee = 1500,
                PrizePool = 18000,
                RoundCount = 4,
                ParticipantsPerMatch = 6,
                MatchDuration = 300f
            }
        };
    }

    public string GetTournamentTypeName(ArenaTournamentData.TournamentType type)
    {
        switch (type)
        {
            case ArenaTournamentData.TournamentType.SoloDuel:
                return "个人赛";
            case ArenaTournamentData.TournamentType.TeamBattle:
                return "团队赛";
            case ArenaTournamentData.TournamentType.FreeForAll:
                return "大乱斗";
            case ArenaTournamentData.TournamentType.MountCombat:
                return "坐骑战";
            case ArenaTournamentData.TournamentType.PetBattle:
                return "宠物战";
            default:
                return "未知";
        }
    }

    public string GetStateName(ArenaTournamentData.TournamentState state)
    {
        switch (state)
        {
            case ArenaTournamentData.TournamentState.Registration:
                return "报名中";
            case ArenaTournamentData.TournamentState.InProgress:
                return "进行中";
            case ArenaTournamentData.TournamentState.Completed:
                return "已结束";
            case ArenaTournamentData.TournamentState.Cancelled:
                return "已取消";
            default:
                return "未知";
        }
    }
}
