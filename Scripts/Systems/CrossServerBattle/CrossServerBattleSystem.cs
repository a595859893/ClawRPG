using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class CrossServerBattleSystem : BaseSystem
{
    private CrossServerBattleData _data = new CrossServerBattleData();
    private CrossServerBattleDatabase _database;
    private Random _random = new Random();

    public CrossServerBattleData Data => _data;
    public CrossServerBattleDatabase Database => _database;

    public override void _Ready()
    {
        _database = CrossServerBattleDatabase.Instance;
        InitializeSeason();
        GD.Print("[CrossServerBattleSystem] Initialized");
    }

    private void InitializeSeason()
    {
        if (_data.CurrentSeason == null || !_data.CurrentSeason.IsActive)
        {
            _data.CurrentSeason = new CrossServerSeason
            {
                SeasonNumber = 1,
                StartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                EndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (30 * 24 * 60 * 60),
                IsActive = true,
                RewardPoints = 100
            };
        }
    }

    // Server Management
    public void RegisterServer(string serverId, string serverName, int playerCount, int averageLevel)
    {
        var existingServer = _data.RegisteredServers.FirstOrDefault(s => s.ServerId == serverId);
        if (existingServer != null)
        {
            existingServer.PlayerCount = playerCount;
            existingServer.AverageLevel = averageLevel;
            existingServer.LastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        else
        {
            var serverLevel = CalculateServerLevel(averageLevel, playerCount);
            var serverRank = _data.RegisteredServers.Count + 1;
            
            var newServer = new ServerInfo
            {
                ServerId = serverId,
                ServerName = serverName,
                PlayerCount = playerCount,
                AverageLevel = averageLevel,
                ServerRank = serverRank,
                ServerLevel = serverLevel,
                LastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            _data.RegisteredServers.Add(newServer);
        }
        
        GD.Print($"[CrossServerBattle] Server registered: {serverName} ({serverId})");
    }

    private CrossServerServerLevel CalculateServerLevel(int averageLevel, int playerCount)
    {
        if (averageLevel >= 80 && playerCount >= 500)
            return CrossServerServerLevel.Diamond;
        if (averageLevel >= 70 && playerCount >= 200)
            return CrossServerServerLevel.Platinum;
        if (averageLevel >= 50 && playerCount >= 100)
            return CrossServerServerLevel.Gold;
        if (averageLevel >= 30 && playerCount >= 50)
            return CrossServerServerLevel.Silver;
        return CrossServerServerLevel.Bronze;
    }

    // Player Registration
    public void RegisterPlayer(string playerId, string playerName, string serverId)
    {
        var existingRecord = _data.PlayerRecords.FirstOrDefault(p => p.PlayerId == playerId);
        if (existingRecord != null)
        {
            existingRecord.ServerId = serverId;
            existingRecord.LastMatchTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        else
        {
            var newRecord = new CrossServerBattleRecord
            {
                PlayerId = playerId,
                PlayerName = playerName,
                ServerId = serverId,
                PersonalRank = _data.PlayerRecords.Count + 1,
                TotalPoints = 1000,
                Wins = 0,
                Losses = 0,
                Draws = 0,
                CurrentStreak = 0,
                BestStreak = 0,
                LastMatchTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                SeasonNumber = _data.CurrentSeason.SeasonNumber
            };
            _data.PlayerRecords.Add(newRecord);
        }
        
        GD.Print($"[CrossServerBattle] Player registered: {playerName} ({playerId}) on server {serverId}");
    }

    // Match Creation
    public CrossServerMatch CreateMatch(CrossServerMatchType matchType, List<string> team1Players, List<string> team2Players, string team1ServerId, string team2ServerId)
    {
        var matchConfig = _database.GetMatchTypeConfig(matchType);
        
        var match = new CrossServerMatch
        {
            MatchId = GenerateMatchId(),
            MatchType = matchType,
            State = CrossServerMatchState.InProgress,
            Team1Players = team1Players,
            Team2Players = team2Players,
            Team1Score = 0,
            Team2Score = 0,
            Team1ServerId = team1ServerId,
            Team2ServerId = team2ServerId,
            StartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            EndTime = 0,
            Duration = matchConfig.MatchDuration
        };
        
        _data.ActiveMatches.Add(match);
        
        GD.Print($"[CrossServerBattle] Match created: {match.MatchId} ({matchConfig.Name})");
        
        return match;
    }

    private string GenerateMatchId()
    {
        return $"CSM_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{_random.Next(1000, 9999)}";
    }

    // Match Updates
    public void UpdateMatchScore(string matchId, int team1Score, int team2Score)
    {
        var match = _data.ActiveMatches.FirstOrDefault(m => m.MatchId == matchId);
        if (match != null)
        {
            match.Team1Score = team1Score;
            match.Team2Score = team2Score;
        }
    }

    public void CompleteMatch(string matchId, CrossServerPlayerResult result)
    {
        var match = _data.ActiveMatches.FirstOrDefault(m => m.MatchId == matchId);
        if (match == null) return;

        match.State = CrossServerMatchState.Completed;
        match.EndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        match.Duration = (int)(match.EndTime - match.StartTime);

        // Update player records
        var matchConfig = _database.GetMatchTypeConfig(match.MatchType);
        var seasonConfig = _database.GetCurrentSeasonConfig(_data.CurrentSeason.SeasonNumber);

        // Get player ID (simplified - in real game would be from match data)
        var playerId = match.Team1Players.FirstOrDefault();
        if (string.IsNullOrEmpty(playerId)) return;

        var playerRecord = _data.PlayerRecords.FirstOrDefault(p => p.PlayerId == playerId);
        if (playerRecord == null) return;

        // Update statistics
        _data.TotalMatches++;
        
        int pointsEarned = 0;
        switch (result)
        {
            case CrossServerPlayerResult.Win:
                playerRecord.Wins++;
                playerRecord.CurrentStreak++;
                if (playerRecord.CurrentStreak > playerRecord.BestStreak)
                    playerRecord.BestStreak = playerRecord.CurrentStreak;
                pointsEarned = matchConfig.WinPoints + (playerRecord.CurrentStreak * matchConfig.StreakBonus);
                _data.TotalWins++;
                break;
                
            case CrossServerPlayerResult.Loss:
                playerRecord.Losses++;
                playerRecord.CurrentStreak = 0;
                pointsEarned = matchConfig.LossPoints;
                _data.TotalLosses++;
                break;
                
            case CrossServerPlayerResult.Draw:
                playerRecord.Draws++;
                playerRecord.CurrentStreak = 0;
                pointsEarned = matchConfig.DrawPoints;
                _data.TotalDraws++;
                break;
        }

        // Apply season bonus
        pointsEarned = (int)(pointsEarned * (1.0 + (_data.CurrentSeason.SeasonNumber * 0.1)));
        
        playerRecord.TotalPoints += pointsEarned;
        playerRecord.LastMatchTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Update high scores
        if (playerRecord.TotalPoints > _data.HighestPoints)
            _data.HighestPoints = playerRecord.TotalPoints;
        if (playerRecord.BestStreak > _data.BestStreak)
            _data.BestStreak = playerRecord.BestStreak;

        // Move to history
        _data.ActiveMatches.Remove(match);
        _data.MatchHistory.Insert(0, match);
        
        // Keep only last 100 matches
        if (_data.MatchHistory.Count > 100)
            _data.MatchHistory.RemoveAt(_data.MatchHistory.Count - 1);

        // Update rankings
        UpdateRankings();

        GD.Print($"[CrossServerBattle] Match completed: {matchId}, Result: {result}, Points: {pointsEarned}");
    }

    // Ranking System
    private void UpdateRankings()
    {
        // Sort players by total points
        var sortedPlayers = _data.PlayerRecords.OrderByDescending(p => p.TotalPoints).ToList();
        
        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            sortedPlayers[i].PersonalRank = i + 1;
        }

        // Sort servers by average points
        var serverPoints = _data.RegisteredServers
            .GroupBy(s => s.ServerId)
            .Select(g => new 
            { 
                Server = g.First(), 
                AvgPoints = _data.PlayerRecords
                    .Where(p => p.ServerId == g.Key)
                    .Sum(p => p.TotalPoints)
            })
            .OrderByDescending(x => x.AvgPoints)
            .ToList();

        for (int i = 0; i < serverPoints.Count; i++)
        {
            serverPoints[i].Server.ServerRank = i + 1;
        }
    }

    public List<CrossServerBattleRecord> GetTopPlayers(int count = 10)
    {
        return _data.PlayerRecords
            .OrderByDescending(p => p.TotalPoints)
            .Take(count)
            .ToList();
    }

    public List<ServerInfo> GetTopServers(int count = 10)
    {
        return _data.RegisteredServers
            .OrderBy(s => s.ServerRank)
            .Take(count)
            .ToList();
    }

    // Season Management
    public void StartNewSeason()
    {
        var newSeasonNumber = _data.CurrentSeason.SeasonNumber + 1;
        
        // Calculate rewards for top players
        var topPlayers = GetTopPlayers(100);
        var seasonConfig = _database.GetCurrentSeasonConfig(newSeasonNumber);
        
        foreach (var kvp in seasonConfig.RankingRewards)
        {
            if (kvp.Key <= topPlayers.Count)
            {
                var player = topPlayers[kvp.Key - 1];
                // Reward would be applied to player's account
                GD.Print($"[CrossServerBattle] Season {_data.CurrentSeason.SeasonNumber} rewards: Player {player.PlayerName} ranked #{kvp.Key} receives {kvp.Value} reward points");
            }
        }

        // Start new season
        _data.CurrentSeason = new CrossServerSeason
        {
            SeasonNumber = newSeasonNumber,
            StartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            EndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (seasonConfig.DurationDays * 24 * 60 * 60),
            IsActive = true,
            RewardPoints = seasonConfig.WinRewardPoints
        };

        // Reset player streaks but keep points
        foreach (var player in _data.PlayerRecords)
        {
            player.CurrentStreak = 0;
            player.SeasonNumber = newSeasonNumber;
        }

        GD.Print($"[CrossServerBattle] New season started: Season {newSeasonNumber}");
    }

    // Matching System
    public List<string> FindMatchingPlayers(string playerId, CrossServerMatchType matchType, int maxDistance = 1000)
    {
        var matchConfig = _database.GetMatchTypeConfig(matchType);
        var player = _data.PlayerRecords.FirstOrDefault(p => p.PlayerId == playerId);
        
        if (player == null) return new List<string>();

        return _data.PlayerRecords
            .Where(p => p.PlayerId != playerId)
            .Where(p => Math.Abs(p.TotalPoints - player.TotalPoints) <= maxDistance)
            .Where(p => p.ServerId != player.ServerId)
            .OrderBy(p => Math.Abs(p.TotalPoints - player.TotalPoints))
            .Take(matchConfig.TeamSize)
            .Select(p => p.PlayerId)
            .ToList();
    }

    // Statistics
    public Dictionary<string, object> GetStatistics()
    {
        var stats = new Dictionary<string, object>();
        
        stats["TotalMatches"] = _data.TotalMatches;
        stats["TotalWins"] = _data.TotalWins;
        stats["TotalLosses"] = _data.TotalLosses;
        stats["TotalDraws"] = _data.TotalDraws;
        stats["WinRate"] = _data.TotalMatches > 0 ? (float)_data.TotalWins / _data.TotalMatches * 100 : 0;
        stats["BestRanking"] = _data.BestRanking;
        stats["HighestPoints"] = _data.HighestPoints;
        stats["BestStreak"] = _data.BestStreak;
        stats["CurrentSeason"] = _data.CurrentSeason?.SeasonNumber ?? 1;
        stats["ActiveServers"] = _data.RegisteredServers.Count;
        stats["RegisteredPlayers"] = _data.PlayerRecords.Count;

        return stats;
    }

    public CrossServerBattleRecord GetPlayerRecord(string playerId)
    {
        return _data.PlayerRecords.FirstOrDefault(p => p.PlayerId == playerId);
    }

    public ServerInfo GetServerInfo(string serverId)
    {
        return _data.RegisteredServers.FirstOrDefault(s => s.ServerId == serverId);
    }

    // Save/Load
    public Dictionary<string, object> SaveData()
    {
        var saveData = new Dictionary<string, object>();
        
        saveData["player_records"] = _data.PlayerRecords;
        saveData["servers"] = _data.RegisteredServers;
        saveData["match_history"] = _data.MatchHistory;
        saveData["current_season"] = _data.CurrentSeason;
        saveData["total_matches"] = _data.TotalMatches;
        saveData["total_wins"] = _data.TotalWins;
        saveData["total_losses"] = _data.TotalLosses;
        saveData["total_draws"] = _data.TotalDraws;
        saveData["best_ranking"] = _data.BestRanking;
        saveData["highest_points"] = _data.HighestPoints;
        saveData["best_streak"] = _data.BestStreak;
        saveData["total_play_time"] = _data.TotalPlayTime;

        return saveData;
    }

    public void LoadData(Dictionary<string, object> saveData)
    {
        if (saveData == null) return;

        if (saveData.ContainsKey("player_records"))
            _data.PlayerRecords = JsonUtils.DeserializeList<CrossServerBattleRecord>(saveData["player_records"]);
        
        if (saveData.ContainsKey("servers"))
            _data.RegisteredServers = JsonUtils.DeserializeList<ServerInfo>(saveData["servers"]);
        
        if (saveData.ContainsKey("match_history"))
            _data.MatchHistory = JsonUtils.DeserializeList<CrossServerMatch>(saveData["match_history"]);
        
        if (saveData.ContainsKey("current_season"))
            _data.CurrentSeason = JsonUtils.DeserializeObject<CrossServerSeason>(saveData["current_season"]);
        
        if (saveData.ContainsKey("total_matches"))
            _data.TotalMatches = Convert.ToInt32(saveData["total_matches"]);
        
        if (saveData.ContainsKey("total_wins"))
            _data.TotalWins = Convert.ToInt32(saveData["total_wins"]);
        
        if (saveData.ContainsKey("total_losses"))
            _data.TotalLosses = Convert.ToInt32(saveData["total_losses"]);
        
        if (saveData.ContainsKey("total_draws"))
            _data.TotalDraws = Convert.ToInt32(saveData["total_draws"]);
        
        if (saveData.ContainsKey("best_ranking"))
            _data.BestRanking = Convert.ToInt32(saveData["best_ranking"]);
        
        if (saveData.ContainsKey("highest_points"))
            _data.HighestPoints = Convert.ToInt32(saveData["highest_points"]);
        
        if (saveData.ContainsKey("best_streak"))
            _data.BestStreak = Convert.ToInt32(saveData["best_streak"]);
        
        if (saveData.ContainsKey("total_play_time"))
            _data.TotalPlayTime = Convert.ToInt64(saveData["total_play_time"]);

        GD.Print("[CrossServerBattle] Data loaded");
    }
}
