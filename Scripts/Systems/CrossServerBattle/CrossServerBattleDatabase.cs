using Godot;
using System;
using System.Collections.Generic;

public class MatchTypeConfig
{
    public CrossServerMatchType Type { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public int TeamSize { get; set; }
    public int MatchDuration { get; set; }
    public int WinPoints { get; set; }
    public int DrawPoints { get; set; }
    public int LossPoints { get; set; }
    public int StreakBonus { get; set; }
    public Color Team1Color { get; set; }
    public Color Team2Color { get; set; }
}

public class ServerLevelConfig
{
    public CrossServerServerLevel Level { get; set; }
    public string Name { get; set; } = "";
    public Color DisplayColor { get; set; }
    public int RequiredAverageLevel { get; set; }
    public int RequiredPlayerCount { get; set; }
    public int MaxPointSpread { get; set; }
    public int RewardMultiplier { get; set; }
}

public class SeasonConfig
{
    public int SeasonNumber { get; set; }
    public string Name { get; set; } = "";
    public int DurationDays { get; set; }
    public int WinRewardPoints { get; set; }
    public int DrawRewardPoints { get; set; }
    public int LossRewardPoints { get; set; }
    public int StreakBonusPerWin { get; set; }
    public Dictionary<int, int> RankingRewards { get; set; } = new Dictionary<int, int>();
}

public class CrossServerBattleDatabase
{
    private static CrossServerBattleDatabase _instance;
    public static CrossServerBattleDatabase Instance
    {
        get
        {
            if (_instance == null)
                _instance = new CrossServerBattleDatabase();
            return _instance;
        }
    }

    public List<MatchTypeConfig> MatchTypes { get; private set; } = new List<MatchTypeConfig>();
    public List<ServerLevelConfig> ServerLevels { get; private set; } = new List<ServerLevelConfig>();
    public List<SeasonConfig> Seasons { get; private set; } = new List<SeasonConfig>();

    public CrossServerBattleDatabase()
    {
        InitializeMatchTypes();
        InitializeServerLevels();
        InitializeSeasons();
    }

    private void InitializeMatchTypes()
    {
        MatchTypes.Add(new MatchTypeConfig
        {
            Type = CrossServerMatchType.OneVOne,
            Name = "1v1 Duel",
            Description = "Single player combat arena",
            MinPlayers = 2,
            MaxPlayers = 2,
            TeamSize = 1,
            MatchDuration = 300,
            WinPoints = 100,
            DrawPoints = 25,
            LossPoints = 0,
            StreakBonus = 10,
            Team1Color = new Color(0.2f, 0.6f, 1.0f),
            Team2Color = new Color(1.0f, 0.3f, 0.3f)
        });

        MatchTypes.Add(new MatchTypeConfig
        {
            Type = CrossServerMatchType.ThreeVThree,
            Name = "3v3 Battle",
            Description = "Team of three players",
            MinPlayers = 6,
            MaxPlayers = 6,
            TeamSize = 3,
            MatchDuration = 480,
            WinPoints = 150,
            DrawPoints = 40,
            LossPoints = 10,
            StreakBonus = 15,
            Team1Color = new Color(0.2f, 0.6f, 1.0f),
            Team2Color = new Color(1.0f, 0.3f, 0.3f)
        });

        MatchTypes.Add(new MatchTypeConfig
        {
            Type = CrossServerMatchType.FiveVFive,
            Name = "5v5 War",
            Description = "Full team warfare",
            MinPlayers = 10,
            MaxPlayers = 10,
            TeamSize = 5,
            MatchDuration = 600,
            WinPoints = 200,
            DrawPoints = 50,
            LossPoints = 15,
            StreakBonus = 20,
            Team1Color = new Color(0.2f, 0.6f, 1.0f),
            Team2Color = new Color(1.0f, 0.3f, 0.3f)
        });

        MatchTypes.Add(new MatchTypeConfig
        {
            Type = CrossServerMatchType.TenVTen,
            Name = "10v10 Arena",
            Description = "Large scale battle",
            MinPlayers = 20,
            MaxPlayers = 20,
            TeamSize = 10,
            MatchDuration = 900,
            WinPoints = 300,
            DrawPoints = 75,
            LossPoints = 20,
            StreakBonus = 30,
            Team1Color = new Color(0.2f, 0.6f, 1.0f),
            Team2Color = new Color(1.0f, 0.3f, 0.3f)
        });

        MatchTypes.Add(new MatchTypeConfig
        {
            Type = CrossServerMatchType.BossRush,
            Name = "Boss Rush",
            Description = "Team vs Boss competition",
            MinPlayers = 4,
            MaxPlayers = 20,
            TeamSize = 10,
            MatchDuration = 720,
            WinPoints = 250,
            DrawPoints = 50,
            LossPoints = 10,
            StreakBonus = 25,
            Team1Color = new Color(0.8f, 0.4f, 0.0f),
            Team2Color = new Color(0.4f, 0.0f, 0.8f)
        });

        MatchTypes.Add(new MatchTypeConfig
        {
            Type = CrossServerMatchType.TeamDeathmatch,
            Name = "Team Deathmatch",
            Description = "First team to elimination wins",
            MinPlayers = 4,
            MaxPlayers = 20,
            TeamSize = 10,
            MatchDuration = 600,
            WinPoints = 200,
            DrawPoints = 50,
            LossPoints = 15,
            StreakBonus = 20,
            Team1Color = new Color(0.2f, 0.6f, 1.0f),
            Team2Color = new Color(1.0f, 0.3f, 0.3f)
        });
    }

    private void InitializeServerLevels()
    {
        ServerLevels.Add(new ServerLevelConfig
        {
            Level = CrossServerServerLevel.Bronze,
            Name = "Bronze",
            DisplayColor = new Color(0.8f, 0.5f, 0.2f),
            RequiredAverageLevel = 1,
            RequiredPlayerCount = 10,
            MaxPointSpread = 500,
            RewardMultiplier = 1
        });

        ServerLevels.Add(new ServerLevelConfig
        {
            Level = CrossServerServerLevel.Silver,
            Name = "Silver",
            DisplayColor = new Color(0.7f, 0.7f, 0.8f),
            RequiredAverageLevel = 30,
            RequiredPlayerCount = 50,
            MaxPointSpread = 1000,
            RewardMultiplier = 2
        });

        ServerLevels.Add(new ServerLevelConfig
        {
            Level = CrossServerServerLevel.Gold,
            Name = "Gold",
            DisplayColor = new Color(1.0f, 0.8f, 0.0f),
            RequiredAverageLevel = 50,
            RequiredPlayerCount = 100,
            MaxPointSpread = 2000,
            RewardMultiplier = 3
        });

        ServerLevels.Add(new ServerLevelConfig
        {
            Level = CrossServerServerLevel.Platinum,
            Name = "Platinum",
            DisplayColor = new Color(0.4f, 0.8f, 0.9f),
            RequiredAverageLevel = 70,
            RequiredPlayerCount = 200,
            MaxPointSpread = 5000,
            RewardMultiplier = 4
        });

        ServerLevels.Add(new ServerLevelConfig
        {
            Level = CrossServerServerLevel.Diamond,
            Name = "Diamond",
            DisplayColor = new Color(0.6f, 0.8f, 1.0f),
            RequiredAverageLevel = 80,
            RequiredPlayerCount = 500,
            MaxPointSpread = 10000,
            RewardMultiplier = 5
        });
    }

    private void InitializeSeasons()
    {
        // Season 1
        var season1 = new SeasonConfig
        {
            SeasonNumber = 1,
            Name = "Season 1",
            DurationDays = 30,
            WinRewardPoints = 100,
            DrawRewardPoints = 25,
            LossRewardPoints = 0,
            StreakBonusPerWin = 10
        };
        season1.RankingRewards.Add(1, 5000);
        season1.RankingRewards.Add(3, 3000);
        season1.RankingRewards.Add(10, 1500);
        season1.RankingRewards.Add(50, 500);
        season1.RankingRewards.Add(100, 200);
        Seasons.Add(season1);

        // Season 2
        var season2 = new SeasonConfig
        {
            SeasonNumber = 2,
            Name = "Season 2",
            DurationDays = 30,
            WinRewardPoints = 120,
            DrawRewardPoints = 30,
            LossRewardPoints = 5,
            StreakBonusPerWin = 12
        };
        season2.RankingRewards.Add(1, 6000);
        season2.RankingRewards.Add(3, 3500);
        season2.RankingRewards.Add(10, 1800);
        season2.RankingRewards.Add(50, 600);
        season2.RankingRewards.Add(100, 250);
        Seasons.Add(season2);

        // Season 3
        var season3 = new SeasonConfig
        {
            SeasonNumber = 3,
            Name = "Season 3",
            DurationDays = 30,
            WinRewardPoints = 150,
            DrawRewardPoints = 35,
            LossRewardPoints = 10,
            StreakBonusPerWin = 15
        };
        season3.RankingRewards.Add(1, 7000);
        season3.RankingRewards.Add(3, 4000);
        season3.RankingRewards.Add(10, 2000);
        season3.RankingRewards.Add(50, 700);
        season3.RankingRewards.Add(100, 300);
        Seasons.Add(season3);
    }

    public MatchTypeConfig GetMatchTypeConfig(CrossServerMatchType type)
    {
        foreach (var config in MatchTypes)
        {
            if (config.Type == type)
                return config;
        }
        return MatchTypes[0];
    }

    public ServerLevelConfig GetServerLevelConfig(CrossServerServerLevel level)
    {
        foreach (var config in ServerLevels)
        {
            if (config.Level == level)
                return config;
        }
        return ServerLevels[0];
    }

    public SeasonConfig GetCurrentSeasonConfig(int seasonNumber)
    {
        foreach (var config in Seasons)
        {
            if (config.SeasonNumber == seasonNumber)
                return config;
        }
        return Seasons[Seasons.Count - 1];
    }
}
