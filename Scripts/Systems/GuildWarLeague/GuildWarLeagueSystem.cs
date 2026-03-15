using Godot;
using System;
using System.Collections.Generic;

public class GuildWarLeagueSystem : BaseSystem {
    
    private GuildWarLeagueData _data;
    private GuildWarLeagueDatabase _database;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    
    // Signals
    public static string SignalMatchCompleted = "match_completed";
    public static string SignalSeasonEnded = "season_ended";
    public static string SignalDivisionPromoted = "division_promoted";
    public static string SignalDivisionDemoted = "division_demoted";
    
    public override void _Ready() {
        base._Ready();
        _rng.Randomize();
        _database = new GuildWarLeagueDatabase();
        LoadData();
        
        // Initialize season if needed
        if (_data.SeasonStartTimestamp == 0) {
            StartNewSeason();
        }
    }
    
    private void LoadData() {
        _data = new GuildWarLeagueData();
        // Try to load from file
        string savePath = "user://guild_war_league_save.json";
        if (FileAccess.FileExists(savePath)) {
            using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
            string json = file.GetAsText();
            // Parse JSON would go here - for now using default data
        }
    }
    
    private void SaveData() {
        string savePath = "user://guild_war_league_save.json";
        using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);
        // Serialize would go here
    }
    
    public void StartNewSeason() {
        _data.CurrentSeason++;
        _data.SeasonStartTimestamp = OS.GetUnixTime();
        _data.SeasonMatches.Clear();
        
        // Reset all guild records for new season
        foreach (var record in _data.GuildRecords.Values) {
            record.Points = 0;
            record.Wins = 0;
            record.Losses = 0;
            record.Draws = 0;
            record.TotalMatches = 0;
            record.WinStreak = 0;
            record.LoseStreak = 0;
            record.Season = _data.CurrentSeason;
            record.TotalDamageDealt = 0;
            record.TotalDamageTaken = 0;
            record.TotalKills = 0;
        }
        
        SaveData();
        EmitSignal(SignalSeasonEnded);
    }
    
    public void RegisterGuild(string guildId, string guildName) {
        if (!_data.GuildRecords.ContainsKey(guildId)) {
            _data.GuildRecords[guildId] = new GuildWarLeagueRecord {
                GuildId = guildId,
                GuildName = guildName,
                Division = "Bronze",
                Rank = _data.GuildRecords.Count + 1,
                Season = _data.CurrentSeason
            };
            _data.DivisionStandings[guildId] = _data.GuildRecords.Count;
            _data.Statistics.TotalGuildsParticipated = _data.GuildRecords.Count;
            SaveData();
        }
    }
    
    public GuildWarMatch CreateMatch(string guildAId, string guildBId, string matchType = "Standard") {
        if (!_data.GuildRecords.ContainsKey(guildAId) || !_data.GuildRecords.ContainsKey(guildBId)) {
            return null;
        }
        
        var guildA = _data.GuildRecords[guildAId];
        var guildB = _data.GuildRecords[guildBId];
        
        var match = new GuildWarMatch {
            MatchId = Guid.NewGuid().ToString(),
            GuildAId = guildAId,
            GuildAName = guildA.GuildName,
            GuildBId = guildBId,
            GuildBName = guildB.GuildName,
            MatchState = "Scheduled",
            StartTimestamp = OS.GetUnixTime() + 300, // 5 minutes from now
            Season = _data.CurrentSeason.ToString()
        };
        
        _data.SeasonMatches.Add(match);
        return match;
    }
    
    public void StartMatch(string matchId) {
        var match = FindMatch(matchId);
        if (match != null && match.MatchState == "Scheduled") {
            match.MatchState = "InProgress";
            match.StartTimestamp = OS.GetUnixTime();
            SaveData();
        }
    }
    
    public void CompleteMatch(string matchId, int guildAScore, int guildBScore, int guildADamage, int guildBDamage, int guildAKills, int guildBKills) {
        var match = FindMatch(matchId);
        if (match == null || match.MatchState != "InProgress") return;
        
        match.MatchState = "Completed";
        match.EndTimestamp = OS.GetUnixTime();
        match.GuildAScore = guildAScore;
        match.GuildBScore = guildBScore;
        
        var guildA = _data.GuildRecords[match.GuildAId];
        var guildB = _data.GuildRecords[match.GuildBId];
        
        // Update statistics
        guildA.TotalDamageDealt += guildADamage;
        guildA.TotalDamageTaken += guildBDamage;
        guildA.TotalKills += guildAKills;
        
        guildB.TotalDamageDealt += guildBDamage;
        guildB.TotalDamageTaken += guildADamage;
        guildB.TotalKills += guildBKills;
        
        // Calculate points based on match type
        var config = _database.MatchTypes.GetValueOrDefault("Standard");
        
        if (guildAScore > guildBScore) {
            // Guild A wins
            match.WinnerId = match.GuildAId;
            int points = config.WinPoints;
            if (guildA.WinStreak >= 3) points += config.BonusWinPoints;
            
            guildA.Points += points;
            guildA.Wins++;
            guildA.WinStreak++;
            guildA.LoseStreak = 0;
            
            guildB.Losses++;
            guildB.WinStreak = 0;
            guildB.LoseStreak++;
        } else if (guildBScore > guildAScore) {
            // Guild B wins
            match.WinnerId = match.GuildBId;
            int points = config.WinPoints;
            if (guildB.WinStreak >= 3) points += config.BonusWinPoints;
            
            guildB.Points += points;
            guildB.Wins++;
            guildB.WinStreak++;
            guildB.LoseStreak = 0;
            
            guildA.Losses++;
            guildA.WinStreak = 0;
            guildA.LoseStreak++;
        } else {
            // Draw
            guildA.Points += config.DrawPoints;
            guildB.Points += config.DrawPoints;
            guildA.Draws++;
            guildB.Draws++;
            guildA.WinStreak = 0;
            guildB.WinStreak = 0;
            guildA.LoseStreak = 0;
            guildB.LoseStreak = 0;
        }
        
        guildA.TotalMatches++;
        guildB.TotalMatches++;
        
        // Update ranks
        UpdateRanks();
        
        // Check for division changes
        CheckDivisionChanges(guildA);
        CheckDivisionChanges(guildB);
        
        // Update statistics
        _data.Statistics.TotalMatchesPlayed++;
        
        // Check for record updates
        if (guildA.WinStreak > _data.Statistics.LongestWinStreak) {
            _data.Statistics.LongestWinStreak = guildA.WinStreak;
        }
        if (guildB.WinStreak > _data.Statistics.LongestWinStreak) {
            _data.Statistics.LongestWinStreak = guildB.WinStreak;
        }
        if (guildA.Points > _data.Statistics.HighestPoints) {
            _data.Statistics.HighestPoints = guildA.Points;
        }
        
        SaveData();
        EmitSignal(SignalMatchCompleted, match);
    }
    
    private GuildWarMatch FindMatch(string matchId) {
        foreach (var match in _data.SeasonMatches) {
            if (match.MatchId == matchId) return match;
        }
        return null;
    }
    
    private void UpdateRanks() {
        // Sort guilds by points
        var sortedGuilds = new List<GuildWarLeagueRecord>(_data.GuildRecords.Values);
        sortedGuilds.Sort((a, b) => b.Points.CompareTo(a.Points));
        
        for (int i = 0; i < sortedGuilds.Count; i++) {
            sortedGuilds[i].Rank = i + 1;
            if (sortedGuilds[i].HighestRank == 0 || sortedGuilds[i].HighestRank > i + 1) {
                sortedGuilds[i].HighestRank = i + 1;
            }
            _data.DivisionStandings[sortedGuilds[i].GuildId] = i + 1;
        }
    }
    
    private void CheckDivisionChanges(GuildWarLeagueRecord guild) {
        string oldDivision = guild.Division;
        
        if (guild.Points >= 15000 && guild.Division != "Legendary") {
            guild.Division = "Legendary";
            EmitSignal(SignalDivisionPromoted, guild.GuildId, "Legendary");
        } else if (guild.Points >= 7000 && guild.Division == "Bronze") {
            guild.Division = "Diamond";
            EmitSignal(SignalDivisionPromoted, guild.GuildId, "Diamond");
        } else if (guild.Points >= 3500 && guild.Division == "Silver") {
            guild.Division = "Platinum";
            EmitSignal(SignalDivisionPromoted, guild.GuildId, "Platinum");
        } else if (guild.Points >= 1500 && guild.Division == "Bronze") {
            guild.Division = "Gold";
            EmitSignal(SignalDivisionPromoted, guild.GuildId, "Gold");
        } else if (guild.Points >= 500 && guild.Division == "Bronze") {
            guild.Division = "Silver";
            EmitSignal(SignalDivisionPromoted, guild.GuildId, "Silver");
        }
        // Demotion logic could be added here
    }
    
    public Dictionary<string, GuildWarLeagueRecord> GetDivisionGuilds(string division) {
        var result = new Dictionary<string, GuildWarLeagueRecord>();
        foreach (var guild in _data.GuildRecords.Values) {
            if (guild.Division == division) {
                result[guild.GuildId] = guild;
            }
        }
        return result;
    }
    
    public List<GuildWarMatch> GetGuildMatches(string guildId) {
        var result = new List<GuildWarMatch>();
        foreach (var match in _data.SeasonMatches) {
            if (match.GuildAId == guildId || match.GuildBId == guildId) {
                result.Add(match);
            }
        }
        return result;
    }
    
    public GuildWarLeagueRecord GetGuildRecord(string guildId) {
        return _data.GuildRecords.GetValueOrDefault(guildId);
    }
    
    public Dictionary<string, GuildWarLeagueRecord> GetTopGuilds(int count) {
        var sorted = new List<GuildWarLeagueRecord>(_data.GuildRecords.Values);
        sorted.Sort((a, b) => b.Points.CompareTo(a.Points));
        
        var result = new Dictionary<string, GuildWarLeagueRecord>();
        for (int i = 0; i < Mathf.Min(count, sorted.Count); i++) {
            result[sorted[i].GuildId] = sorted[i];
        }
        return result;
    }
    
    public int GetSeasonDaysRemaining() {
        int elapsed = OS.GetUnixTime() - _data.SeasonStartTimestamp;
        int total = _data.SeasonDurationDays * 86400;
        return Mathf.Max(0, (total - elapsed) / 86400);
    }
    
    public SeasonReward GetSeasonReward(string division) {
        return _data.SeasonRewards.GetValueOrDefault(division);
    }
    
    public GuildWarLeagueStatistics GetStatistics() {
        return _data.Statistics;
    }
    
    public int GetCurrentSeason() {
        return _data.CurrentSeason;
    }
    
    // Generate random matches for testing
    public void GenerateRandomMatches(int count) {
        var guildIds = new List<string>(_data.GuildRecords.Keys);
        if (guildIds.Count < 2) return;
        
        for (int i = 0; i < count; i++) {
            int idxA = _rng.RandiRange(0, guildIds.Count - 1);
            int idxB = _rng.RandiRange(0, guildIds.Count - 1);
            while (idxA == idxB) {
                idxB = _rng.RandiRange(0, guildIds.Count - 1);
            }
            
            var match = CreateMatch(guildIds[idxA], guildIds[idxB]);
            if (match != null) {
                // Simulate match results
                int scoreA = _rng.RandiRange(0, 100);
                int scoreB = _rng.RandiRange(0, 100);
                int damageA = _rng.RandiRange(1000, 10000);
                int damageB = _rng.RandiRange(1000, 10000);
                int killsA = _rng.RandiRange(5, 50);
                int killsB = _rng.RandiRange(5, 50);
                
                StartMatch(match.MatchId);
                CompleteMatch(match.MatchId, scoreA, scoreB, damageA, damageB, killsA, killsB);
            }
        }
    }
}
