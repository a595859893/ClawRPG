using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 竞技场锦标赛数据结构
/// 包含赛事信息、参赛者状态、比赛结果等
/// </summary>
public class ArenaTournamentData
{
    public enum TournamentState
    {
        Registration,
        InProgress,
        Completed,
        Cancelled
    }

    public enum TournamentType
    {
        SoloDuel,
        TeamBattle,
        FreeForAll,
        MountCombat,
        PetBattle
    }

    public enum ParticipantStatus
    {
        Registered,
        Eliminated,
        Champion,
        RunnerUp,
        ThirdPlace
    }

    [Serializable]
    public class Tournament
    {
        public string Id;
        public string Name;
        public string Description;
        public TournamentType Type;
        public int MaxParticipants;
        public int MinLevel;
        public int EntryFee;
        public int PrizePool;
        public int RoundCount;
        public int ParticipantsPerMatch;
        public float MatchDuration;
        public DateTime RegistrationStart;
        public DateTime RegistrationEnd;
        public DateTime StartTime;
        public TournamentState State;
        public List<string> RegisteredPlayerIds = new List<string>();
        public List<string> EliminatedPlayerIds = new List<string>();
        public List<string> ActiveMatchIds = new List<string>();
    }

    [Serializable]
    public class TournamentMatch
    {
        public string Id;
        public string TournamentId;
        public int Round;
        public int MatchNumber;
        public List<string> ParticipantIds = new List<string>();
        public string WinnerId;
        public bool IsCompleted;
        public DateTime StartTime;
        public DateTime EndTime;
    }

    [Serializable]
    public class PlayerTournamentData
    {
        public string PlayerId;
        public List<string> RegisteredTournamentIds = new List<string>();
        public int Wins;
        public int Losses;
        public int Championships;
        public int TotalEarnings;
        public Dictionary<string, ParticipantStatus> TournamentStatuses = new Dictionary<string, ParticipantStatus>();
    }
}
