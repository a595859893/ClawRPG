using System;
using System.Collections.Generic;

namespace ClawRPG.Modules.MultiplayerVote
{
    /// <summary>
    /// Vote type for multiplayer decisions
    /// </summary>
    public enum VoteType
    {
        KickPlayer,          // 踢出玩家
        StartGame,           // 开始游戏
        PauseGame,           // 暂停游戏
        Surrender,           // 投降
        MapVote,            // 地图投票
        DifficultyVote,      // 难度投票
        ReadyCheck,          // 准备确认
        InvitePlayer,        // 邀请玩家
        PromoteLeader,       // 提升队长
        CancelMatch         // 取消匹配
    }

    /// <summary>
    /// Vote status
    /// </summary>
    public enum VoteStatus
    {
        Pending,    // 待投票
        Passed,     // 通过
        Failed,     // 失败
        Cancelled,  // 取消
        Expired     // 过期
    }

    /// <summary>
    /// Individual vote record
    /// </summary>
    public class VoteRecord
    {
        public string PlayerId { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public bool VotedYes { get; set; }
        public int VoteTime { get; set; }
    }

    /// <summary>
    /// Vote configuration
    /// </summary>
    public class VoteConfig
    {
        public VoteType Type { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = ""
        public int DurationSeconds { get; set; } = 30;
        public float PassThreshold { get; set; } = 0.5f;  // 50% needed to pass
        public bool RequireMajority { get; set; } = false;
        public bool AutoCancelOnLeaver { get; set; } = true;
    }

    /// <summary>
    /// Active vote instance
    /// </summary>
    public class ActiveVote
    {
        public string VoteId { get; set; } = Guid.NewGuid().ToString();
        public VoteType Type { get; set; }
        public string InitiatorId { get; set; } = "";
        public string InitiatorName { get; set; } = "";
        public string TargetId { get; set; } = "";  // For kick/promote
        public string TargetName { get; set; } = "";
        public string Reason { get; set; } = "";
        public VoteStatus Status { get; set; } = VoteStatus.Pending;
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public List<VoteRecord> Votes { get; set; } = new List<VoteRecord>();
        public int YesCount => Votes.FindAll(v => v.VotedYes).Count;
        public int NoCount => Votes.FindAll(v => !v.VotedYes).Count;
        public int TotalVotes => Votes.Count;
        public float YesPercentage => TotalVotes > 0 ? (float)YesCount / TotalVotes : 0f;
    }

    /// <summary>
    /// Party member data
    /// </summary>
    public class PartyMember
    {
        public string PlayerId { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public int Level { get; set; }
        public int Power { get; set; }
        public bool IsLeader { get; set; }
        public bool IsReady { get; set; }
        public int JoinTime { get; set; }
        public string Role { get; set; } = "Member";  // Member/Officer/Leader
    }

    /// <summary>
    /// Party data
    /// </summary>
    public class Party
    {
        public string PartyId { get; set; } = Guid.NewGuid().ToString();
        public string PartyName { get; set; } = "";
        public string LeaderId { get; set; } = "";
        public List<PartyMember> Members { get; set; } = new List<PartyMember>();
        public int MaxMembers { get; set; } = 4;
        public bool IsPublic { get; set; } = true;
        public string Password { get; set; } = "";
        public string GameMode { get; set; } = "";
        public int MinLevel { get; set; } = 1;
        public int MaxLevel { get; set; } = 100;
        public int CreateTime { get; set; }
    }

    /// <summary>
    /// Player party data
    /// </summary>
    public class PlayerPartyData
    {
        public string PlayerId { get; set; } = "";
        public string CurrentPartyId { get; set; } = "";
        public List<string> PendingInvites { get; set; } = new List<string>();
        public List<string> PastPartyIds { get; set; } = new List<string>();
        public int TotalPartiesJoined { get; set; }
        public int TotalPartiesCreated { get; set; }
        public int VotesCast { get; set; }
        public int VotesInitiated { get; set; }
    }

    /// <summary>
    /// Party statistics
    /// </summary>
    public class PartyStatistics
    {
        public int TotalVotes { get; set; }
        public int VotesPassed { get; set; }
        public int VotesFailed { get; set; }
        public int PartiesCreated { get; set; }
        public int PartiesJoined { get; set; }
        public int TimesKicked { get; set; }
        public int TimesKickedOthers { get; set; }
    }

    /// <summary>
    /// Root multiplayer vote data
    /// </summary>
    public class MultiplayerVoteData
    {
        public Dictionary<string, Party> ActiveParties { get; set; } = new Dictionary<string, Party>();
        public Dictionary<string, ActiveVote> ActiveVotes { get; set; } = new Dictionary<string, ActiveVote>();
        public Dictionary<string, PlayerPartyData> PlayerPartyData { get; set; } = new Dictionary<string, PlayerPartyData>();
        public Dictionary<string, PartyStatistics> PlayerStatistics { get; set; } = new Dictionary<string, PartyStatistics>();
    }
}
