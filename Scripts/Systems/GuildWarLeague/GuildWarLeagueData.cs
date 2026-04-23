using Godot;
using System;
using System.Collections.Generic;

public partial class GuildWarLeagueData : Resource {
    [Export] public int CurrentSeason { get; set; } = 1;
    [Export] public int SeasonStartTimestamp { get; set; }
    [Export] public int SeasonDurationDays { get; set; } = 30;
    public Dictionary<string, GuildWarLeagueRecord> GuildRecords { get; set; } = new Dictionary<string, GuildWarLeagueRecord>();
    public List<GuildWarMatch> SeasonMatches { get; set; } = new List<GuildWarMatch>();
    public Dictionary<string, int> DivisionStandings { get; set; } = new Dictionary<string, int>();
    public GuildWarLeagueStatistics Statistics { get; set; } = new GuildWarLeagueStatistics();
}

public class GuildWarLeagueRecord {
    public string GuildId { get; set; } = "";
    public string GuildName { get; set; } = "";
    public string Division { get; set; } = "Bronze";
    public int Rank { get; set; } = 0;
    public int Points { get; set; } = 0;
    public int Wins { get; set; } = 0;
    public int Losses { get; set; } = 0;
    public int Draws { get; set; } = 0;
    public int TotalMatches { get; set; } = 0;
    public int WinStreak { get; set; } = 0;
    public int LoseStreak { get; set; } = 0;
    public int HighestRank { get; set; } = 0;
    public int Season { get; set; } = 1;
    public int TotalDamageDealt { get; set; } = 0;
    public int TotalDamageTaken { get; set; } = 0;
    public int TotalKills { get; set; } = 0;
}

public class GuildWarMatch {
    public string MatchId { get; set; } = "";
    public string GuildAId { get; set; } = "";
    public string GuildAName { get; set; } = "";
    public string GuildBId { get; set; } = "";
    public string GuildBName { get; set; } = "";
    public int GuildAScore { get; set; } = 0;
    public int GuildBScore { get; set; } = 0;
    public string WinnerId { get; set; } = "";
    public string MatchState { get; set; } = "Scheduled"; // Scheduled/InProgress/Completed
    public int StartTimestamp { get; set; }
    public int EndTimestamp { get; set; }
    public string Season { get; set; } = "";
}

public class GuildWarLeagueStatistics {
    public int TotalMatchesPlayed { get; set; } = 0;
    public int TotalGuildsParticipated { get; set; } = 0;
    public int TotalSeasons { get; set; } = 0;
    public int MostWinsGuildId { get; set; } = 0;
    public int LongestWinStreak { get; set; } = 0;
    public int HighestPoints { get; set; } = 0;
}

public partial class GuildWarLeagueDatabase : Resource {
    public Dictionary<string, DivisionConfig> Divisions { get; set; } = new Dictionary<string, DivisionConfig>();
    public Dictionary<string, SeasonReward> SeasonRewards { get; set; } = new Dictionary<string, SeasonReward>();
    public Dictionary<string, MatchConfig> MatchTypes { get; set; } = new Dictionary<string, MatchConfig>();
    
    public GuildWarLeagueDatabase() {
        InitializeDivisions();
        InitializeSeasonRewards();
        InitializeMatchTypes();
    }
    
    private void InitializeDivisions() {
        Divisions["Bronze"] = new DivisionConfig { DivisionId = "Bronze", RequiredPoints = 0, MaxRank = 100, RewardMultiplier = 1.0f };
        Divisions["Silver"] = new DivisionConfig { DivisionId = "Silver", RequiredPoints = 500, MaxRank = 80, RewardMultiplier = 1.25f };
        Divisions["Gold"] = new DivisionConfig { DivisionId = "Gold", RequiredPoints = 1500, MaxRank = 60, RewardMultiplier = 1.5f };
        Divisions["Platinum"] = new DivisionConfig { DivisionId = "Platinum", RequiredPoints = 3500, MaxRank = 40, RewardMultiplier = 2.0f };
        Divisions["Diamond"] = new DivisionConfig { DivisionId = "Diamond", RequiredPoints = 7000, MaxRank = 20, RewardMultiplier = 2.5f };
        Divisions["Legendary"] = new DivisionConfig { DivisionId = "Legendary", RequiredPoints = 15000, MaxRank = 10, RewardMultiplier = 3.0f };
    }
    
    private void InitializeSeasonRewards() {
        SeasonRewards["Bronze"] = new SeasonReward { Division = "Bronze", GoldReward = 1000, ReputationReward = 50, ItemRewardPool = new string[] { "common_chest" } };
        SeasonRewards["Silver"] = new SeasonReward { Division = "Silver", GoldReward = 2500, ReputationReward = 100, ItemRewardPool = new string[] { "uncommon_chest", "silver_coin" } };
        SeasonRewards["Gold"] = new SeasonReward { Division = "Gold", GoldReward = 5000, ReputationReward = 200, ItemRewardPool = new string[] { "rare_chest", "gold_coin", "ruby" } };
        SeasonRewards["Platinum"] = new SeasonReward { Division = "Platinum", GoldReward = 10000, ReputationReward = 400, ItemRewardPool = new string[] { "epic_chest", "diamond", "legendary_fragment" } };
        SeasonRewards["Diamond"] = new SeasonReward { Division = "Diamond", GoldReward = 25000, ReputationReward = 800, ItemRewardPool = new string[] { "epic_chest", "legendary_weapon_shard", "ancient_coin" } };
        SeasonRewards["Legendary"] = new SeasonReward { Division = "Legendary", GoldReward = 50000, ReputationReward = 1500, ItemRewardPool = new string[] { "legendary_chest", "ancient_relic", "mythic_essence" } };
    }
    
    private void InitializeMatchTypes() {
        MatchTypes["Standard"] = new MatchConfig { MatchTypeId = "Standard", WinPoints = 30, DrawPoints = 10, LossPoints = 0, BonusWinPoints = 5 };
        MatchTypes["Championship"] = new MatchConfig { MatchTypeId = "Championship", WinPoints = 50, DrawPoints = 15, LossPoints = 5, BonusWinPoints = 10 };
        MatchTypes["Tournament"] = new MatchConfig { MatchTypeId = "Tournament", WinPoints = 75, DrawPoints = 20, LossPoints = 10, BonusWinPoints = 15 };
    }
}

public class DivisionConfig {
    public string DivisionId { get; set; } = "";
    public int RequiredPoints { get; set; } = 0;
    public int MaxRank { get; set; } = 0;
    public float RewardMultiplier { get; set; } = 1.0f;
}

public class SeasonReward {
    public string Division { get; set; } = "";
    public int GoldReward { get; set; } = 0;
    public int ReputationReward { get; set; } = 0;
    public string[] ItemRewardPool { get; set; } = Array.Empty<string>();
}

public class MatchConfig {
    public string MatchTypeId { get; set; } = "";
    public int WinPoints { get; set; } = 0;
    public int DrawPoints { get; set; } = 0;
    public int LossPoints { get; set; } = 0;
    public int BonusWinPoints { get; set; } = 0;
}
