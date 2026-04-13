using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 宠物友谊数据 - 存储宠物之间友谊等级和经验
/// </summary>
public partial class PetFriendshipData : Resource
{
    public int PetId { get; set; }
    public int FriendPetId { get; set; }
    public int FriendshipLevel { get; set; }
    public int Experience { get; set; }
    public DateTime LastInteraction { get; set; }
    public bool IsBondsOfWar { get; set; }

    /// <summary>
    /// 历史最高友谊等级（跨所有游戏局次）
    /// </summary>
    public int MaxFriendship { get; set; }

    /// <summary>
    /// 共同战斗总次数（跨所有游戏局次）
    /// </summary>
    public int TotalBattles { get; set; }
}

public class PetFriendshipDatabase
{
    private static readonly Dictionary<string, Dictionary<int, int>> FriendshipExperienceCurve = new Dictionary<string, Dictionary<int, int>>
    {
        { "Stranger", new Dictionary<int, int> { { 1, 0 }, { 2, 100 } } },
        { "Acquaintance", new Dictionary<int, int> { { 1, 0 }, { 2, 250 } } },
        { "Friend", new Dictionary<int, int> { { 1, 0 }, { 2, 500 } } },
        { "CloseFriend", new Dictionary<int, int> { { 1, 0 }, { 2, 1000 } } },
        { "BestFriend", new Dictionary<int, int> { { 1, 0 }, { 2, 2000 } } },
        { "Soulmate", new Dictionary<int, int> { { 1, 0 }, { 2, 5000 } } }
    };

    private static readonly Dictionary<string, float> FriendshipBonuses = new Dictionary<string, float>
    {
        { "Stranger", 1.0f },
        { "Acquaintance", 1.05f },
        { "Friend", 1.1f },
        { "CloseFriend", 1.15f },
        { "BestFriend", 1.25f },
        { "Soulmate", 1.5f }
    };

    public static string GetFriendshipTier(int level)
    {
        if (level <= 1) return "Stranger";
        if (level <= 3) return "Acquaintance";
        if (level <= 6) return "Friend";
        if (level <= 10) return "CloseFriend";
        if (level <= 15) return "BestFriend";
        return "Soulmate";
    }

    public static int GetExpForLevel(int level)
    {
        string tier = GetFriendshipTier(level);
        if (FriendshipExperienceCurve.ContainsKey(tier))
        {
            if (FriendshipExperienceCurve[tier].ContainsKey(2))
                return FriendshipExperienceCurve[tier][2];
        }
        return 5000;
    }

    public static float GetBonusMultiplier(int level)
    {
        string tier = GetFriendshipTier(level);
        if (FriendshipBonuses.ContainsKey(tier))
            return FriendshipBonuses[tier];
        return 1.0f;
    }

    public static string[] GetAllTiers()
    {
        return new string[] { "Stranger", "Acquaintance", "Friend", "CloseFriend", "BestFriend", "Soulmate" };
    }

    public static int[] GetTierThresholds()
    {
        return new int[] { 1, 4, 7, 11, 16, 20 };
    }

    public static string[] GetFriendshipSkills()
    {
        return new string[] 
        { 
            "Combined Attack",
            "Defensive Bond",
            "Healing Aura",
            "Experience Boost",
            "Critical Strike",
            "Life Steal",
            "Speed Boost",
            " Dodge Mastery"
        };
    }

    public static Dictionary<string, float> GetSkillBonuses(string skill)
    {
        var bonuses = new Dictionary<string, float>();
        switch (skill)
        {
            case "Combined Attack":
                bonuses["attack"] = 0.15f;
                break;
            case "Defensive Bond":
                bonuses["defense"] = 0.15f;
                break;
            case "Healing Aura":
                bonuses["health"] = 0.1f;
                bonuses["regen"] = 0.05f;
                break;
            case "Experience Boost":
                bonuses["exp"] = 0.2f;
                break;
            case "Critical Strike":
                bonuses["crit_rate"] = 0.1f;
                break;
            case "Life Steal":
                bonuses["lifesteal"] = 0.08f;
                break;
            case "Speed Boost":
                bonuses["speed"] = 0.12f;
                break;
            case "Dodge Mastery":
                bonuses["dodge"] = 0.1f;
                break;
        }
        return bonuses;
    }
}
