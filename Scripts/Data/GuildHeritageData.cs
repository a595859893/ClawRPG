using System;
using System.Collections.Generic;
using Godot;

public enum HeritageType
{
    BattleCry,      // Battle bonuses
    Arcane secrets, // Magic bonuses
    Crafting Mastery,
    Trade Prosperity,
    Defense Fortification,
    Exploration,
    Diplomacy,
    Legendary Heroes
}

public enum HeritageTier
{
    None,
    Bronze,
    Silver,
    Gold,
    Platinum,
    Diamond
}

public class HeritageBonus
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public HeritageType Type { get; set; }
    public HeritageTier Tier { get; set; }
    public int RequiredPoints { get; set; }
    public float DamageBonus { get; set; }
    public float DefenseBonus { get; set; }
    public float MagicBonus { get; set; }
    public float GoldBonus { get; set; }
    public float ExpBonus { get; set; }
    public float DropRateBonus { get; set; }
    public Dictionary<string, int> Requirements { get; set; }
}

public class GuildHeritage
{
    public string GuildId { get; set; }
    public string GuildName { get; set; }
    public Dictionary<string, int> UnlockedHeritages { get; set; }
    public int TotalHeritagePoints { get; set; }
    public Dictionary<string, int> ContributionHistory { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<string> ActiveEffects { get; set; }
}

public class PlayerHeritageData
{
    public string PlayerId { get; set; }
    public int PersonalContribution { get; set; }
    public Dictionary<string, int> PersonalUnlocks { get; set; }
    public DateTime LastContributionTime { get; set; }
}

public class GuildHeritageStatistics
{
    public int TotalGuildsWithHeritages { get; set; }
    public int TotalHeritagesUnlocked { get; set; }
    public string MostPopularHeritage { get; set; }
    public Dictionary<HeritageType, int> HeritagesByType { get; set; }
}
