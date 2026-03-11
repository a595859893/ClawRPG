using Godot;
using System;
using System.Collections.Generic;

public class ArenaTournamentSystem : Node
{
    private static ArenaTournamentSystem _instance;
    public static ArenaTournamentSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ArenaTournamentSystem();
            }
            return _instance;
        }
    }

    private Dictionary<string, ArenaTournamentData.Tournament> _tournaments = new Dictionary<string, ArenaTournamentData.Tournament>();
    private Dictionary<string, ArenaTournamentData.TournamentMatch> _matches = new Dictionary<string, ArenaTournamentData.TournamentMatch>();
    private Dictionary<string, ArenaTournamentData.PlayerTournamentData> _playerData = new Dictionary<string, ArenaTournamentData.PlayerTournamentData>();
    private List<string> _activeTournamentOrder = new List<string>();
    private int _matchIdCounter = 0;

    public Signal<string> TournamentRegistered { get; } = new Signal<string>();
    public Signal<string> TournamentStarted { get; } = new Signal<string>();
    public Signal<string, string> MatchStarted { get; } = new Signal<string, string>();
    public Signal<string, string> MatchCompleted { get; } = new Signal<string, string>();
    public Signal<string, string> TournamentCompleted { get; } = new Signal<string, string>();
    public Signal<string, string> PlayerEliminated { get; } = new Signal<string, string>();
    public Signal<string, string> PlayerChampion { get; } = new Signal<string, string>();

    public override void _Ready()
    {
        _instance = this;
        LoadDefaultTournaments();
    }

    private void LoadDefaultTournaments()
    {
        var defaultTournaments = ArenaTournamentDatabase.Instance.GetDefaultTournaments();
        foreach (var tournament in defaultTournaments)
        {
            _tournaments[tournament.Id] = tournament;
        }
    }

    public List<ArenaTournamentData.Tournament> GetAllTournaments()
    {
        return new List<ArenaTournamentData.Tournament>(_tournaments.Values);
    }

    public List<ArenaTournamentData.Tournament> GetAvailableTournaments()
    {
        var result = new List<ArenaTournamentData.Tournament>();
        var now = DateTime.Now;
        
        foreach (var tournament in _tournaments.Values)
        {
            if (tournament.State == ArenaTournamentData.TournamentState.Registration)
            {
                result.Add(tournament);
            }
        }
        
        return result;
    }

    public List<ArenaTournamentData.Tournament> GetTournamentsByType(ArenaTournamentData.TournamentType type)
    {
        var result = new List<ArenaTournamentData.Tournament>();
        
        foreach (var tournament in _tournaments.Values)
        {
            if (tournament.Type == type)
            {
                result.Add(tournament);
            }
        }
        
        return result;
    }

    public ArenaTournamentData.Tournament GetTournament(string tournamentId)
    {
        if (_tournaments.ContainsKey(tournamentId))
        {
            return _tournaments[tournamentId];
        }
        return null;
    }

    public bool CanRegister(string tournamentId, string playerId, int playerLevel, int playerGold)
    {
        var tournament = GetTournament(tournamentId);
        if (tournament == null) return false;
        
        if (tournament.State != ArenaTournamentData.TournamentState.Registration)
        {
            GD.Print($"[ArenaTournament] Tournament {tournamentId} is not accepting registrations");
            return false;
        }
        
        if (tournament.MinLevel > playerLevel)
        {
            GD.Print($"[ArenaTournament] Player level {playerLevel} is below minimum {tournament.MinLevel}");
            return false;
        }
        
        if (playerGold < tournament.EntryFee)
        {
            GD.Print($"[ArenaTournament] Player gold {playerGold} is below entry fee {tournament.EntryFee}");
            return false;
        }
        
        if (tournament.RegisteredPlayerIds.Count >= tournament.MaxParticipants)
        {
            GD.Print($"[ArenaTournament] Tournament {tournamentId} is full");
            return false;
        }
        
        if (tournament.RegisteredPlayerIds.Contains(playerId))
        {
            GD.Print($"[ArenaTournament] Player {playerId} already registered");
            return false;
        }
        
        return true;
    }

    public bool RegisterPlayer(string tournamentId, string playerId, ref int playerGold)
    {
        var tournament = GetTournament(tournamentId);
        if (tournament == null) return false;
        
        if (!CanRegister(tournamentId, playerId, 1, playerGold))
        {
            return false;
        }
        
        // Deduct entry fee
        playerGold -= tournament.EntryFee;
        tournament.PrizePool += tournament.EntryFee;
        tournament.RegisteredPlayerIds.Add(playerId);
        
        // Initialize player data if needed
        if (!_playerData.ContainsKey(playerId))
        {
            _playerData[playerId] = new ArenaTournamentData.PlayerTournamentData
            {
                PlayerId = playerId
            };
        }
        
        var playerData = _playerData[playerId];
        playerData.RegisteredTournamentIds.Add(tournamentId);
        playerData.TournamentStatuses[tournamentId] = ArenaTournamentData.ParticipantStatus.Registered;
        
        TournamentRegistered.Emit(tournamentId);
        
        GD.Print($"[ArenaTournament] Player {playerId} registered for {tournamentId}");
        return true;
    }

    public bool StartTournament(string tournamentId)
    {
        var tournament = GetTournament(tournamentId);
        if (tournament == null) return false;
        
        if (tournament.RegisteredPlayerIds.Count < 2)
        {
            GD.Print($"[ArenaTournament] Not enough participants to start {tournamentId}");
            return false;
        }
        
        tournament.State = ArenaTournamentData.TournamentState.InProgress;
        _activeTournamentOrder.Add(tournamentId);
        
        // Generate first round matches
        GenerateRoundMatches(tournament, 1);
        
        TournamentStarted.Emit(tournamentId);
        GD.Print($"[ArenaTournament] Tournament {tournamentId} started with {tournament.RegisteredPlayerIds.Count} participants");
        return true;
    }

    private void GenerateRoundMatches(ArenaTournamentData.Tournament tournament, int round)
    {
        var participants = new List<string>(tournament.RegisteredPlayerIds);
        
        // Remove already eliminated players
        foreach (var eliminatedId in tournament.EliminatedPlayerIds)
        {
            participants.Remove(eliminatedId);
        }
        
        // Shuffle participants
        var random = new Random();
        for (int i = participants.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            var temp = participants[i];
            participants[i] = participants[j];
            participants[j] = temp;
        }
        
        if (tournament.Type == ArenaTournamentData.TournamentType.FreeForAll)
        {
            // Single match for all participants
            var match = new ArenaTournamentData.TournamentMatch
            {
                Id = $"match_{tournamentId}_{_matchIdCounter++}",
                TournamentId = tournament.Id,
                Round = round,
                MatchNumber = 1,
                ParticipantIds = new List<string>(participants),
                IsCompleted = false,
                StartTime = DateTime.Now
            };
            
            _matches[match.Id] = match;
            tournament.ActiveMatchIds.Add(match.Id);
            MatchStarted.Emit(tournament.Id, match.Id);
        }
        else
        {
            // Create pair matches
            int matchNumber = 1;
            for (int i = 0; i < participants.Count; i += tournament.ParticipantsPerMatch)
            {
                var matchParticipants = new List<string>();
                for (int j = 0; j < tournament.ParticipantsPerMatch && i + j < participants.Count; j++)
                {
                    matchParticipants.Add(participants[i + j]);
                }
                
                var match = new ArenaTournamentData.TournamentMatch
                {
                    Id = $"match_{tournamentId}_{_matchIdCounter++}",
                    TournamentId = tournament.Id,
                    Round = round,
                    MatchNumber = matchNumber++,
                    ParticipantIds = matchParticipants,
                    IsCompleted = false,
                    StartTime = DateTime.Now
                };
                
                _matches[match.Id] = match;
                tournament.ActiveMatchIds.Add(match.Id);
                MatchStarted.Emit(tournament.Id, match.Id);
            }
        }
    }

    public void CompleteMatch(string matchId, string winnerId)
    {
        if (!_matches.ContainsKey(matchId)) return;
        
        var match = _matches[matchId];
        match.WinnerId = winnerId;
        match.IsCompleted = true;
        match.EndTime = DateTime.Now;
        
        var tournament = GetTournament(match.TournamentId);
        if (tournament == null) return;
        
        // Remove winner from active, add losers to eliminated
        foreach (var participantId in match.ParticipantIds)
        {
            if (participantId != winnerId)
            {
                tournament.EliminatedPlayerIds.Add(participantId);
                
                // Update player status
                if (_playerData.ContainsKey(participantId))
                {
                    _playerData[participantId].TournamentStatuses[tournament.Id] = ArenaTournamentData.ParticipantStatus.Eliminated;
                    _playerData[participantId].Losses++;
                }
                
                PlayerEliminated.Emit(tournament.Id, participantId);
            }
            else
            {
                if (_playerData.ContainsKey(participantId))
                {
                    _playerData[participantId].Wins++;
                }
            }
        }
        
        tournament.ActiveMatchIds.Remove(matchId);
        
        // Check if round is complete
        bool roundComplete = true;
        foreach (var activeMatchId in tournament.ActiveMatchIds)
        {
            if (_matches.ContainsKey(activeMatchId) && _matches[activeMatchId].Round == match.Round)
            {
                roundComplete = false; 
                break;
            }
        }
        
        if (roundComplete)
        {
            // Check if tournament is complete
            var remainingParticipants = new List<string>();
            foreach (var pid in tournament.RegisteredPlayerIds)
            {
                if (!tournament.EliminatedPlayerIds.Contains(pid))
                {
                    remainingParticipants.Add(pid);
                }
            }
            
            if (remainingParticipants.Count <= 1 || match.Round >= tournament.RoundCount)
            {
                CompleteTournament(tournament, remainingParticipants);
            }
            else
            {
                // Start next round
                GenerateRoundMatches(tournament, match.Round + 1);
            }
        }
        
        MatchCompleted.Emit(tournament.Id, matchId);
    }

    private void CompleteTournament(ArenaTournamentData.Tournament tournament, List<string> winners)
    {
        tournament.State = ArenaTournamentData.TournamentState.Completed;
        
        if (winners.Count > 0)
        {
            var championId = winners[0];
            
            // Update champion data
            if (_playerData.ContainsKey(championId))
            {
                _playerData[championId].TournamentStatuses[tournament.Id] = ArenaTournamentData.ParticipantStatus.Champion;
                _playerData[championId].Championships++;
                _playerData[championId].TotalEarnings += tournament.PrizePool;
            }
            
            PlayerChampion.Emit(tournament.Id, championId);
            GD.Print($"[ArenaTournament] Tournament {tournament.Id} completed. Champion: {championId}");
        }
        
        if (winners.Count > 1)
        {
            var runnerUpId = winners[1];
            if (_playerData.ContainsKey(runnerUpId))
            {
                _playerData[runnerUpId].TournamentStatuses[tournament.Id] = ArenaTournamentData.ParticipantStatus.RunnerUp;
            }
        }
        
        TournamentCompleted.Emit(tournament.Id);
    }

    public ArenaTournamentData.PlayerTournamentData GetPlayerData(string playerId)
    {
        if (_playerData.ContainsKey(playerId))
        {
            return _playerData[playerId];
        }
        
        return new ArenaTournamentData.PlayerTournamentData { PlayerId = playerId };
    }

    public List<ArenaTournamentData.TournamentMatch> GetTournamentMatches(string tournamentId)
    {
        var result = new List<ArenaTournamentData.TournamentMatch>();
        
        foreach (var match in _matches.Values)
        {
            if (match.TournamentId == tournamentId)
            {
                result.Add(match);
            }
        }
        
        return result;
    }

    public Dictionary<string, object> GetSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // Save tournaments
        var tournamentsData = new List<Dictionary<string, object>>();
        foreach (var tournament in _tournaments.Values)
        {
            var tData = new Dictionary<string, object>
            {
                { "Id", tournament.Id },
                { "State", (int)tournament.State },
                { "RegisteredPlayerIds", tournament.RegisteredPlayerIds },
                { "EliminatedPlayerIds", tournament.EliminatedPlayerIds },
                { "ActiveMatchIds", tournament.ActiveMatchIds },
                { "PrizePool", tournament.PrizePool }
            };
            tournamentsData.Add(tData);
        }
        data["tournaments"] = tournamentsData;
        
        // Save matches
        var matchesData = new List<Dictionary<string, object>>();
        foreach (var match in _matches.Values)
        {
            var mData = new Dictionary<string, object>
            {
                { "Id", match.Id },
                { "TournamentId", match.TournamentId },
                { "Round", match.Round },
                { "MatchNumber", match.MatchNumber },
                { "ParticipantIds", match.ParticipantIds },
                { "WinnerId", match.WinnerId ?? "" },
                { "IsCompleted", match.IsCompleted }
            };
            matchesData.Add(mData);
        }
        data["matches"] = matchesData;
        
        // Save player data
        var playerDataList = new List<Dictionary<string, object>>();
        foreach (var playerData in _playerData.Values)
        {
            var pData = new Dictionary<string, object>
            {
                { "PlayerId", playerData.PlayerId },
                { "Wins", playerData.Wins },
                { "Losses", playerData.Losses },
                { "Championships", playerData.Championships },
                { "TotalEarnings", playerData.TotalEarnings }
            };
            playerDataList.Add(pData);
        }
        data["playerData"] = playerDataList;
        
        return data;
    }

    public void LoadSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        // Load tournaments
        if (data.ContainsKey("tournaments"))
        {
            var tournamentsData = (List<object>)data["tournaments"];
            foreach (var tData in tournamentsData)
            {
                var dict = (Dictionary<string, object>)tData;
                var id = (string)dict["Id"];
                if (_tournaments.ContainsKey(id))
                {
                    _tournaments[id].State = (ArenaTournamentData.TournamentState)(int)dict["State"];
                    _tournaments[id].RegisteredPlayerIds = new List<string>((List<object>)dict["RegisteredPlayerIds"]);
                    _tournaments[id].EliminatedPlayerIds = new List<string>((List<object>)dict["EliminatedPlayerIds"]);
                    _tournaments[id].ActiveMatchIds = new List<string>((List<object>)dict["ActiveMatchIds"]);
                    _tournaments[id].PrizePool = (int)dict["PrizePool"];
                }
            }
        }
        
        // Load matches
        if (data.ContainsKey("matches"))
        {
            var matchesData = (List<object>)data["matches"];
            foreach (var mData in matchesData)
            {
                var dict = (Dictionary<string, object>)mData;
                var match = new ArenaTournamentData.TournamentMatch
                {
                    Id = (string)dict["Id"],
                    TournamentId = (string)dict["TournamentId"],
                    Round = (int)dict["Round"],
                    MatchNumber = (int)dict["MatchNumber"],
                    ParticipantIds = new List<string>((List<object>)dict["ParticipantIds"]),
                    WinnerId = (string)dict["WinnerId"],
                    IsCompleted = (bool)dict["IsCompleted"]
                };
                _matches[match.Id] = match;
            }
        }
        
        // Load player data
        if (data.ContainsKey("playerData"))
        {
            var playerDataList = (List<object>)data["playerData"];
            foreach (var pData in playerDataList)
            {
                var dict = (Dictionary<string, object>)pData;
                var playerData = new ArenaTournamentData.PlayerTournamentData
                {
                    PlayerId = (string)dict["PlayerId"],
                    Wins = (int)dict["Wins"],
                    Losses = (int)dict["Losses"],
                    Championships = (int)dict["Championships"],
                    TotalEarnings = (int)dict["TotalEarnings"]
                };
                _playerData[playerData.PlayerId] = playerData;
            }
        }
    }
}
