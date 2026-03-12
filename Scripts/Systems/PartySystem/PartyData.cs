using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{

public class PartyData
{
    public enum PartyState
    {
        None,
        Forming,
        Ready,
        InBattle,
        Disbanded
    }

    public enum PartyType
    {
        Solo,
        Duo,
        Squad,
        Raid
    }

    public enum MemberRole
    {
        Leader,
        Tank,
        Healer,
        Damage,
        Support
    }

    public class PartyMember
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = "";
        public int Level { get; set; }
        public int ClassId { get; set; }
        public MemberRole Role { get; set; }
        public bool IsReady { get; set; }
        public bool IsOnline { get; set; }
        public float HealthPercent { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    public class Party
    {
        public string PartyId { get; set; } = "";
        public string PartyName { get; set; } = "";
        public PartyType Type { get; set; }
        public PartyState State { get; set; }
        public int LeaderId { get; set; }
        public List<PartyMember> Members { get; set; } = new List<PartyMember>();
        public int MaxMembers { get; set; }
        public float ExpShareBonus { get; set; }
        public float DropRateBonus { get; set; }
        public float DamageBonus { get; set; }
        public float DefenseBonus { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PartyInvite
    {
        public int FromPlayerId { get; set; }
        public string FromPlayerName { get; set; } = "";
        public int ToPlayerId { get; set; }
        public PartyType PartyType { get; set; }
        public DateTime SentAt { get; set; }
    }

    public class PlayerPartyData
    {
        public string CurrentPartyId { get; set; } = "";
        public int TotalPartiesJoined { get; set; }
        public int TotalPartiesWon { get; set; }
        public int TotalPartyMembersInvited { get; set; }
        public List<PartyRecord> History { get; set; } = new List<PartyRecord>();
    }

    public class PartyRecord
    {
        public string PartyId { get; set; } = "";
        public string PartyName { get; set; } = "";
        public PartyType Type { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime LeftAt { get; set; }
        public bool WasLeader { get; set; }
        public bool WasVictory { get; set; }
    }
}

public class PartyDatabase
{
    public static Dictionary<PartyData.PartyType, int> MaxMembersByType = new Dictionary<PartyData.PartyType, int>
    {
        { PartyData.PartyType.Solo, 1 },
        { PartyData.PartyType.Duo, 2 },
        { PartyData.PartyType.Squad, 4 },
        { PartyData.PartyType.Raid, 8 }
    };

    public static Dictionary<PartyData.PartyType, float> ExpShareBonusByType = new Dictionary<PartyData.PartyType, float>
    {
        { PartyData.PartyType.Solo, 0f },
        { PartyData.PartyType.Duo, 0.1f },
        { PartyData.PartyType.Squad, 0.25f },
        { PartyData.PartyType.Raid, 0.5f }
    };

    public static Dictionary<PartyData.PartyType, float> DropRateBonusByType = new Dictionary<PartyData.PartyType, float>
    {
        { PartyData.PartyType.Solo, 0f },
        { PartyData.PartyType.Duo, 0.05f },
        { PartyData.PartyType.Squad, 0.15f },
        { PartyData.PartyType.Raid, 0.3f }
    };

    public static Dictionary<PartyData.PartyType, float> DamageBonusPerMember = new Dictionary<PartyData.PartyType, float>
    {
        { PartyData.PartyType.Solo, 0f },
        { PartyData.PartyType.Duo, 0.02f },
        { PartyData.PartyType.Squad, 0.03f },
        { PartyData.PartyType.Raid, 0.04f }
    };

    public static Dictionary<PartyData.PartyType, float> DefenseBonusPerMember = new Dictionary<PartyData.PartyType, float>
    {
        { PartyData.PartyType.Solo, 0f },
        { PartyData.PartyType.Duo, 0.03f },
        { PartyData.PartyType.Squad, 0.05f },
        { PartyData.PartyType.Raid, 0.07f }
    };

    public static string[] PartyTypeNames = { "单人", "双人", "小队", "团队" };
    public static string[] RoleNames = { "队长", "坦克", "治疗", "输出", "辅助" };

    public static int GetMaxMembers(PartyData.PartyType type)
    {
        return MaxMembersByType.ContainsKey(type) ? MaxMembersByType[type] : 1;
    }

    public static float GetExpShareBonus(PartyData.PartyType type)
    {
        return ExpShareBonusByType.ContainsKey(type) ? ExpShareBonusByType[type] : 0f;
    }

    public static float GetDropRateBonus(PartyData.PartyType type)
    {
        return DropRateBonusByType.ContainsKey(type) ? DropRateBonusByType[type] : 0f;
    }

    public static float GetDamageBonus(PartyData.PartyType type, int memberCount)
    {
        float baseBonus = DamageBonusPerMember.ContainsKey(type) ? DamageBonusPerMember[type] : 0f;
        return baseBonus * memberCount;
    }

    public static float GetDefenseBonus(PartyData.PartyType type, int memberCount)
    {
        float baseBonus = DefenseBonusPerMember.ContainsKey(type) ? DefenseBonusPerMember[type] : 0f;
        return baseBonus * memberCount;
    }
}
}
