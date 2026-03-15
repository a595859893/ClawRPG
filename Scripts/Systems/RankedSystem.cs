using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 排位系统 - 管理玩家排位赛段位和积分
/// </summary>
public enum RankTier
{
    Bronze,
    Silver,
    Gold,
    Diamond,
    Master,
    GrandMaster
}

public enum RankDivision
{
    IV,
    III,
    II,
    I
}

public class RankData
{
    public string playerName;
    public RankTier tier;
    public RankDivision division;
    public int points;
    public int wins;
    public int losses;
    public int currentStreak;
    public int bestStreak;
    public DateTime lastMatchTime;
    public int seasonWins;
    public int seasonLosses;
    public int seasonPoints;
}

public class RankMatch
{
    public string matchId;
    public string playerName;
    public string opponentName;
    public bool playerWon;
    public int pointsChange;
    public RankTier playerTierBefore;
    public RankTier playerTierAfter;
    public DateTime matchTime;
}

public partial class RankedSystem : BaseSystem
{
    private static RankedSystem instance;
    public static RankedSystem Instance => instance;

    private Dictionary<string, RankData> playerRanks = new Dictionary<string, RankData>();
    private List<RankMatch> matchHistory = new List<RankMatch>();
    private Dictionary<string, string> activeMatches = new Dictionary<string, string>();

    // Rank configuration
    private readonly Dictionary<RankTier, int[]> TierPoints = new Dictionary<RankTier, int[]>
    {
        { RankTier.Bronze, new int[] { 0, 400 } },
        { RankTier.Silver, new int[] { 400, 800 } },
        { RankTier.Gold, new int[] { 800, 1200 } },
        { RankTier.Diamond, new int[] { 1200, 1800 } },
        { RankTier.Master, new int[] { 1800, 2400 } },
        { RankTier.GrandMaster, new int[] { 2400, 99999 } }
    };

    private readonly Dictionary<RankTier, Color> TierColors = new Dictionary<RankTier, Color>
    {
        { RankTier.Bronze, new Color(0.8f, 0.5f, 0.3f) },
        { RankTier.Silver, new Color(0.7f, 0.7f, 0.8f) },
        { RankTier.Gold, new Color(1f, 0.85f, 0.3f) },
        { RankTier.Diamond, new Color(0.4f, 0.9f, 1f) },
        { RankTier.Master, new Color(0.9f, 0.3f, 0.9f) },
        { RankTier.GrandMaster, new Color(1f, 0.3f, 0.3f) }
    };

    private string currentSeasonId;
    private DateTime seasonStartTime;
    private DateTime seasonEndTime;

    // UI References
    private Control rankedUI;
    private Label rankLabel;
    private Label pointsLabel;
    private Label streakLabel;
    private Label winsLabel;
    private Label lossesLabel;

    public override void _Ready()
    {
        instance = this;
        InitializeSeason();
        LoadData();
    }

    private void InitializeSeason()
    {
        currentSeasonId = "Season_" + DateTime.Now.ToString("yyyyMM");
        seasonStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        seasonEndTime = seasonStartTime.AddMonths(1).AddDays(-1);
    }

    public void LoadData()
    {
        // Load player rank data
        var gameState = GetNode<Main>("/root/Main")?.GameState;
        if (gameState != null && gameState.Contains("ranked_data"))
        {
            // Parse saved data
        }
    }

    public void SaveData()
    {
        var gameState = GetNode<Main>("/root/Main")?.GameState;
        if (gameState != null)
        {
            // Save rank data
        }
    }

    public RankData GetOrCreateRank(string playerName)
    {
        if (!playerRanks.ContainsKey(playerName))
        {
            playerRanks[playerName] = new RankData
            {
                playerName = playerName,
                tier = RankTier.Bronze,
                division = RankDivision.IV,
                points = 0,
                wins = 0,
                losses = 0,
                currentStreak = 0,
                bestStreak = 0,
                lastMatchTime = DateTime.MinValue,
                seasonWins = 0,
                seasonLosses = 0,
                seasonPoints = 0
            };
        }
        return playerRanks[playerName];
    }

    public int CalculatePointsGain(RankTier opponentTier, bool won, int currentStreak)
    {
        int basePoints = won ? 25 : -15;
        
        // Tier difference bonus
        int tierDiff = (int)opponentTier - (int)GetCurrentTier();
        basePoints += tierDiff * 5;
        
        // Streak bonus
        if (won && currentStreak >= 3)
        {
            basePoints += Math.Min(currentStreak - 2, 10) * 2;
        }
        
        // Divison protection
        var currentRank = GetOrCreateRank(Player.Name);
        if (!won && currentRank.points < 100)
        {
            basePoints = Math.Max(basePoints, -5);
        }
        
        return basePoints;
    }

    public RankTier GetCurrentTier()
    {
        return GetOrCreateRank(Player.Name).tier;
    }

    public string GetRankedTitle()
    {
        var rank = GetOrCreateRank(Player.Name);
        return $"{rank.tier} {rank.division}";
    }

    public Color GetTierColor()
    {
        return TierColors[GetCurrentTier()];
    }

    public int GetPointsInTier()
    {
        var rank = GetOrCreateRank(Player.Name);
        var tierRange = TierPoints[rank.tier];
        return rank.points - tierRange[0];
    }

    public int GetPointsToNextTier()
    {
        var rank = GetOrCreateRank(Player.Name);
        var tierRange = TierPoints[rank.tier];
        return tierRange[1] - rank.points;
    }

    public bool CanPromote()
    {
        var rank = GetOrCreateRank(Player.Name);
        return rank.points >= TierPoints[rank.tier][1] && rank.tier != RankTier.GrandMaster;
    }

    public bool CanDemote()
    {
        var rank = GetOrCreateRank(Player.Name);
        return rank.points < TierPoints[rank.tier][0] - 200 && rank.tier != RankTier.Bronze;
    }

    public void RecordMatch(bool won, string opponentName, RankTier opponentTier)
    {
        var rank = GetOrCreateRank(Player.Name);
        
        int pointsChange = CalculatePointsGain(opponentTier, won, rank.currentStreak);
        RankTier tierBefore = rank.tier;
        
        // Update stats
        rank.points = Math.Max(0, rank.points + pointsChange);
        
        if (won)
        {
            rank.wins++;
            rank.seasonWins++;
            rank.currentStreak++;
            rank.bestStreak = Math.Max(rank.bestStreak, rank.currentStreak);
        }
        else
        {
            rank.losses++;
            rank.seasonLosses++;
            rank.currentStreak = 0;
        }
        
        rank.lastMatchTime = DateTime.Now;
        
        // Check promotion/demotion
        CheckTierChange(rank);
        
        // Record match
        var match = new RankMatch
        {
            matchId = Guid.NewGuid().ToString(),
            playerName = Player.Name,
            opponentName = opponentName,
            playerWon = won,
            pointsChange = pointsChange,
            playerTierBefore = tierBefore,
            playerTierAfter = rank.tier,
            matchTime = DateTime.Now
        };
        matchHistory.Add(match);
        
        // Create match record for opponent
        if (playerRanks.ContainsKey(opponentName))
        {
            var opponentRank = playerRanks[opponentName];
            int opponentPointsChange = CalculatePointsGain(tierBefore, !won, opponentRank.currentStreak);
            opponentRank.points = Math.Max(0, opponentRank.points + opponentPointsChange);
            
            if (!won)
            {
                opponentRank.wins++;
                opponentRank.currentStreak++;
            }
            else
            {
                opponentRank.losses++;
                opponentRank.currentStreak = 0;
            }
            CheckTierChange(opponentRank);
        }
        
        SaveData();
    }

    private void CheckTierChange(RankData rank)
    {
        // Promotion
        while (rank.tier != RankTier.GrandMaster && rank.points >= TierPoints[rank.tier][1])
        {
            rank.tier++;
            rank.points = TierPoints[rank.tier][0];
            rank.division = RankDivision.IV;
        }
        
        // Demotion
        while (rank.tier != RankTier.Bronze && rank.points < TierPoints[rank.tier][0] - 200)
        {
            rank.tier--;
            rank.points = TierPoints[rank.tier][1] - 1;
            rank.division = RankDivision.I;
        }
        
        // Update division based on points
        int tierPoints = rank.points - TierPoints[rank.tier][0];
        int tierSize = TierPoints[rank.tier][1] - TierPoints[rank.tier][0];
        
        if (tierPoints >= tierSize * 0.75f)
            rank.division = RankDivision.I;
        else if (tierPoints >= tierSize * 0.5f)
            rank.division = RankDivision.II;
        else if (tierPoints >= tierSize * 0.25f)
            rank.division = RankDivision.III;
        else
            rank.division = RankDivision.IV;
    }

    public List<RankMatch> GetMatchHistory(int limit = 20)
    {
        int count = Math.Min(limit, matchHistory.Count);
        return matchHistory.GetRange(matchHistory.Count - count, count);
    }

    public Dictionary<string, int> GetLeaderboard(int limit = 100)
    {
        var leaderboard = new Dictionary<string, int>();
        
        foreach (var kvp in playerRanks)
        {
            leaderboard[kvp.Key] = kvp.Value.points;
        }
        
        // Sort by points
        var sorted = new List<KeyValuePair<string, int>>(leaderboard);
        sorted.Sort((a, b) => b.Value.CompareTo(a.Value));
        
        var result = new Dictionary<string, int>();
        for (int i = 0; i < Math.Min(limit, sorted.Count); i++)
        {
            result[sorted[i].Key] = sorted[i].Value;
        }
        
        return result;
    }

    public Dictionary<string, object> GetPlayerStats()
    {
        var rank = GetOrCreateRank(Player.Name);
        
        return new Dictionary<string, object>
        {
            { "tier", rank.tier.ToString() },
            { "division", rank.division.ToString() },
            { "points", rank.points },
            { "wins", rank.wins },
            { "losses", rank.losses },
            { "winRate", rank.wins + rank.losses > 0 ? (float)rank.wins / (rank.wins + rank.losses) * 100 : 0 },
            { "currentStreak", rank.currentStreak },
            { "bestStreak", rank.bestStreak },
            { "seasonWins", rank.seasonWins },
            { "seasonLosses", rank.seasonLosses },
            { "seasonPoints", rank.seasonPoints }
        };
    }

    public string GetSeasonInfo()
    {
        return $"Season: {currentSeasonId}\nEnds: {seasonEndTime:MMM dd}";
    }

    public float GetSeasonProgress()
    {
        TimeSpan total = seasonEndTime - seasonStartTime;
        TimeSpan elapsed = DateTime.Now - seasonStartTime;
        return (float)(elapsed.TotalSeconds / total.TotalSeconds);
    }

    public int GetPlacementMatchesRemaining()
    {
        var rank = GetOrCreateRank(Player.Name);
        return Math.Max(0, 10 - rank.wins - rank.losses);
    }

    public bool IsInPlacement()
    {
        return GetPlacementMatchesRemaining() > 0;
    }

    // Called when UI is toggled
    public void ToggleUI()
    {
        // Will be called from keybinding
    }

    // ===== 持久化方法 =====

    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        // 玩家排名数据
        var ranksData = new List<Dictionary>();
        foreach (var kvp in playerRanks)
        {
            var rankDict = new Dictionary();
            rankDict["playerName"] = kvp.Key;
            rankDict["tier"] = (int)kvp.Value.tier;
            rankDict["division"] = (int)kvp.Value.division;
            rankDict["points"] = kvp.Value.points;
            rankDict["wins"] = kvp.Value.wins;
            rankDict["losses"] = kvp.Value.losses;
            rankDict["currentStreak"] = kvp.Value.currentStreak;
            rankDict["bestStreak"] = kvp.Value.bestStreak;
            rankDict["seasonWins"] = kvp.Value.seasonWins;
            rankDict["seasonLosses"] = kvp.Value.seasonLosses;
            rankDict["seasonPoints"] = kvp.Value.seasonPoints;
            ranksData.Add(rankDict);
        }
        data["playerRanks"] = ranksData;
        
        // 比赛历史
        var historyData = new List<Dictionary>();
        foreach (var match in matchHistory)
        {
            var matchDict = new Dictionary();
            matchDict["matchId"] = match.matchId;
            matchDict["playerName"] = match.playerName;
            matchDict["opponentName"] = match.opponentName;
            matchDict["playerWon"] = match.playerWon;
            matchDict["pointsChange"] = match.pointsChange;
            matchDict["playerTierBefore"] = (int)match.playerTierBefore;
            matchDict["playerTierAfter"] = (int)match.playerTierAfter;
            historyData.Add(matchDict);
        }
        data["matchHistory"] = historyData;
        
        // 赛季信息
        data["currentSeasonId"] = currentSeasonId;
        data["seasonStartTime"] = seasonStartTime.ToString("o");
        data["seasonEndTime"] = seasonEndTime.ToString("o");
        
        return data;
    }

    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        // 加载玩家排名数据
        if (data.Contains("playerRanks"))
        {
            var ranksData = (Array)data["playerRanks"];
            playerRanks.Clear();
            foreach (Dictionary rankDict in ranksData)
            {
                var rank = new RankData
                {
                    playerName = rankDict["playerName"].ToString(),
                    tier = (RankTier)(int)rankDict["tier"],
                    division = (RankDivision)(int)rankDict["division"],
                    points = (int)rankDict["points"],
                    wins = (int)rankDict["wins"],
                    losses = (int)rankDict["losses"],
                    currentStreak = (int)rankDict["currentStreak"],
                    bestStreak = (int)rankDict["bestStreak"],
                    seasonWins = (int)rankDict["seasonWins"],
                    seasonLosses = (int)rankDict["seasonLosses"],
                    seasonPoints = (int)rankDict["seasonPoints"]
                };
                playerRanks[rank.playerName] = rank;
            }
        }
        
        // 加载比赛历史
        if (data.Contains("matchHistory"))
        {
            var historyData = (Array)data["matchHistory"];
            matchHistory.Clear();
            foreach (Dictionary matchDict in historyData)
            {
                var match = new RankMatch
                {
                    matchId = matchDict["matchId"].ToString(),
                    playerName = matchDict["playerName"].ToString(),
                    opponentName = matchDict["opponentName"].ToString(),
                    playerWon = (bool)matchDict["playerWon"],
                    pointsChange = (int)matchDict["pointsChange"],
                    playerTierBefore = (RankTier)(int)matchDict["playerTierBefore"],
                    playerTierAfter = (RankTier)(int)matchDict["playerTierAfter"]
                };
                matchHistory.Add(match);
            }
        }
        
        // 加载赛季信息
        if (data.Contains("currentSeasonId"))
        {
            currentSeasonId = data["currentSeasonId"].ToString();
        }
        if (data.Contains("seasonStartTime"))
        {
            DateTime.TryParse(data["seasonStartTime"].ToString(), out seasonStartTime);
        }
        if (data.Contains("seasonEndTime"))
        {
            DateTime.TryParse(data["seasonEndTime"].ToString(), out seasonEndTime);
        }
    }
}
