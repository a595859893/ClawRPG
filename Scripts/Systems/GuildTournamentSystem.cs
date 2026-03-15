using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Guild Tournament System - Competitive guild tournaments
    /// </summary>
    public partial class GuildTournamentSystem : BaseSystem
    {
        public static GuildTournamentSystem Instance { get; private set; }

        public override void _Ready()
        {
            base._Ready();
            Instance = this;
            Initialize();
        }

        protected override void Initialize()
        {
            GD.Print("GuildTournamentSystem initialized");
            _currentTournament = new TournamentData
            {
                State = TournamentState.Registration,
                RegisteredGuilds = new List<int>()
            };
            IsInitialized = true;
        }

        // Tournament states
        public enum TournamentState
        {
            Registration,    // Guilds can register
            Preparation,    // Tournament setup
            InProgress,     // Tournament active
            Completed       // Tournament finished
        }

        // Tournament types
        public enum TournamentType
        {
            Deathmatch,     // Total kills
            CaptureFlag,    // Control points
            Survival,       // Last guild standing
            BossRush,      // Combined boss damage
            TreasureHunt,   // Most treasure collected
            PuzzleChallenge // Guild puzzle solving
        }

        // Tournament data
        private TournamentData _currentTournament;
        private List<TournamentData> _tournamentHistory = new List<TournamentData>();
        private Dictionary<int, GuildTournamentScore> _guildScores = new Dictionary<int, GuildTournamentScore>();
        
        // Configuration
        private const int REGISTRATION_DURATION = 300; // 5 minutes
        private const int TOURNAMENT_DURATION = 1800; // 30 minutes
        private const int MAX_GUILDS = 16;
        
        // Timer
        private float _stateTimer = 0f;
        
        public event Action<TournamentState> OnStateChanged;
        public event Action<int, GuildTournamentScore> OnScoreUpdated;
        public event Action<TournamentData> OnTournamentComplete;
        
        public GuildTournamentSystem()
        {
            _currentTournament = new TournamentData
            {
                State = TournamentState.Registration,
                RegisteredGuilds = new List<int>()
            };
        }

        public override void _Ready()
        {
            base._Ready();
            Initialize();
        }

        protected override void Initialize()
        {
            GD.Print("[GuildTournamentSystem] initialized");
            IsInitialized = true;
        }
        
        /// <summary>
        /// Start a new tournament
        /// </summary>
        public void StartTournament(TournamentType type, string name)
        {
            if (_currentTournament.State != TournamentState.Completed && 
                _currentTournament.RegisteredGuilds.Count > 0)
            {
                GD.Print("[GuildTournament] Cannot start new tournament - current one still active");
                return;
            }
            
            _currentTournament = new TournamentData
            {
                Id = (int)DateTime.Now.Ticks,
                Name = name,
                Type = type,
                State = TournamentState.Registration,
                StartTime = DateTime.Now,
                RegisteredGuilds = new List<int>(),
                Matches = new List<TournamentMatch>()
            };
            
            _guildScores.Clear();
            _stateTimer = 0f;
            
            // Create bracket
            CreateBracket();
            
            GD.Print($"[GuildTournament] Started tournament: {name}, Type: {type}");
            OnStateChanged?.Invoke(TournamentState.Registration);
        }
        
        /// <summary>
        /// Register a guild for the tournament
        /// </summary>
        public bool RegisterGuild(int guildId, string guildName)
        {
            if (_currentTournament.State != TournamentState.Registration)
            {
                GD.Print($"[GuildTournament] Registration closed for guild {guildName}");
                return false;
            }
            
            if (_currentTournament.RegisteredGuilds.Count >= MAX_GUILDS)
            {
                GD.Print($"[GuildTournament] Tournament full for guild {guildName}");
                return false;
            }
            
            if (_currentTournament.RegisteredGuilds.Contains(guildId))
            {
                GD.Print($"[GuildTournament] Guild {guildName} already registered");
                return false;
            }
            
            _currentTournament.RegisteredGuilds.Add(guildId);
            _guildScores[guildId] = new GuildTournamentScore
            {
                GuildId = guildId,
                GuildName = guildName,
                Kills = 0,
                Deaths = 0,
                Damage = 0,
                Healing = 0,
                Score = 0,
                Rank = _currentTournament.RegisteredGuilds.Count
            };
            
            GD.Print($"[GuildTournament] Guild {guildName} registered for tournament");
            return true;
        }
        
        /// <summary>
        /// Update tournament state
        /// </summary>
        public void Update(float delta)
        {
            if (_currentTournament.State == TournamentState.Completed)
                return;
                
            _stateTimer += delta;
            
            switch (_currentTournament.State)
            {
                case TournamentState.Registration:
                    if (_stateTimer >= REGISTRATION_DURATION)
                    {
                        _currentTournament.State = TournamentState.Preparation;
                        _stateTimer = 0f;
                        OnStateChanged?.Invoke(TournamentState.Preparation);
                        GD.Print("[GuildTournament] Registration ended, preparation started");
                    }
                    break;
                    
                case TournamentState.Preparation:
                    if (_stateTimer >= 60f && _currentTournament.RegisteredGuilds.Count >= 2)
                    {
                        _currentTournament.State = TournamentState.InProgress;
                        _stateTimer = 0f;
                        _currentTournament.MatchStartTime = DateTime.Now;
                        OnStateChanged?.Invoke(TournamentState.InProgress);
                        GD.Print("[GuildTournament] Tournament started!");
                    }
                    else if (_stateTimer >= 60f)
                    {
                        // Not enough guilds - cancel
                        _currentTournament.State = TournamentState.Completed;
                        _stateTimer = 0f;
                        OnStateChanged?.Invoke(TournamentState.Completed);
                        GD.Print("[GuildTournament] Cancelled - not enough guilds");
                    }
                    break;
                    
                case TournamentState.InProgress:
                    if (_stateTimer >= TOURNAMENT_DURATION)
                    {
                        _currentTournament.State = TournamentState.Completed;
                        _stateTimer = 0f;
                        CompleteTournament();
                        OnStateChanged?.Invoke(TournamentState.Completed);
                        GD.Print("[GuildTournament] Tournament completed!");
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Update guild score
        /// </summary>
        public void UpdateGuildScore(int guildId, int kills, int deaths, float damage, float healing)
        {
            if (_currentTournament.State != TournamentState.InProgress)
                return;
                
            if (!_guildScores.ContainsKey(guildId))
                return;
                
            var score = _guildScores[guildId];
            score.Kills += kills;
            score.Deaths += deaths;
            score.Damage += damage;
            score.Healing += healing;
            
            // Calculate total score based on tournament type
            switch (_currentTournament.Type)
            {
                case TournamentType.Deathmatch:
                    score.Score = kills * 100 - deaths * 25;
                    break;
                case TournamentType.CaptureFlag:
                    score.Score = kills * 50 + (int)damage / 10;
                    break;
                case TournamentType.Survival:
                    score.Score = kills * 100 + (int)healing / 5;
                    break;
                case TournamentType.BossRush:
                    score.Score = (int)damage;
                    break;
                case TournamentType.TreasureHunt:
                    score.Score = kills * 20 + (int)damage / 5;
                    break;
                default:
                    score.Score = kills * 100 - deaths * 25;
                    break;
            }
            
            OnScoreUpdated?.Invoke(guildId, score);
        }
        
        /// <summary>
        /// Get current tournament data
        /// </summary>
        public TournamentData GetCurrentTournament()
        {
            return _currentTournament;
        }
        
        /// <summary>
        /// Get guild scores sorted by rank
        /// </summary>
        public List<GuildTournamentScore> GetLeaderboard()
        {
            List<GuildTournamentScore> scores = new List<GuildTournamentScore>(_guildScores.Values);
            scores.Sort((a, b) => b.Score.CompareTo(a.Score));
            
            // Update ranks
            for (int i = 0; i < scores.Count; i++)
            {
                scores[i].Rank = i + 1;
            }
            
            return scores;
        }
        
        /// <summary>
        /// Get tournament history
        /// </summary>
        public List<TournamentData> GetHistory()
        {
            return _tournamentHistory;
        }
        
        /// <summary>
        /// Create tournament bracket
        /// </summary>
        private void CreateBracket()
        {
            int guildCount = _currentTournament.RegisteredGuilds.Count;
            if (guildCount < 2)
                return;
                
            // Simple single elimination bracket
            _currentTournament.Matches = new List<TournamentMatch>();
            
            int rounds = (int)Mathf.Ceil(Mathf.Log(guildCount, 2));
            int matchId = 0;
            
            for (int round = 1; round <= rounds; round++)
            {
                int matchesInRound = guildCount / (int)Mathf.Pow(2, round);
                if (matchesInRound < 1) matchesInRound = 1;
                
                for (int i = 0; i < matchesInRound; i++)
                {
                    var match = new TournamentMatch
                    {
                        Id = matchId++,
                        Round = round,
                        Guild1Id = -1,
                        Guild2Id = -1,
                        Guild1Score = 0,
                        Guild2Score = 0,
                        WinnerId = -1,
                        IsComplete = false
                    };
                    _currentTournament.Matches.Add(match);
                }
            }
        }
        
        /// <summary>
        /// Complete tournament and calculate results
        /// </summary>
        private void CompleteTournament()
        {
            var leaderboard = GetLeaderboard();
            
            if (leaderboard.Count > 0)
            {
                _currentTournament.WinnerGuildId = leaderboard[0].GuildId;
                _currentTournament.WinnerGuildName = leaderboard[0].GuildName;
                _currentTournament.PrizePool = _currentTournament.RegisteredGuilds.Count * 1000;
            }
            
            // Add to history
            _tournamentHistory.Add(_currentTournament);
            
            // Keep only last 50 tournaments
            if (_tournamentHistory.Count > 50)
            {
                _tournamentHistory.RemoveAt(0);
            }
            
            OnTournamentComplete?.Invoke(_currentTournament);
        }
        
        /// <summary>
        /// Get time remaining in current state
        /// </summary>
        public float GetTimeRemaining()
        {
            float duration = 0f;
            
            switch (_currentTournament.State)
            {
                case TournamentState.Registration:
                    duration = REGISTRATION_DURATION;
                    break;
                case TournamentState.Preparation:
                    duration = 60f;
                    break;
                case TournamentState.InProgress:
                    duration = TOURNAMENT_DURATION;
                    break;
            }
            
            return Mathf.Max(0, duration - _stateTimer);
        }
        
        /// <summary>
        /// Get current state
        /// </summary>
        public TournamentState GetState()
        {
            return _currentTournament.State;
        }
        
        /// <summary>
        /// Save tournament data
        /// </summary>
        protected override Dictionary ExportSaveData()
        {
            Dictionary data = new Dictionary();
            
            // Save current tournament
            if (_currentTournament != null)
            {
                data["current_tournament"] = new Dictionary
                {
                    { "id", _currentTournament.Id },
                    { "name", _currentTournament.Name },
                    { "type", (int)_currentTournament.Type },
                    { "state", (int)_currentTournament.State },
                    { "state_timer", _stateTimer },
                    { "registered_guilds", _currentTournament.RegisteredGuilds }
                };
            }
            
            // Save guild scores
            Godot.Collections.Array scoresList = new Godot.Collections.Array();
            foreach (var kvp in _guildScores)
            {
                scoresList.Add(new Godot.Collections.Dictionary
                {
                    { "guild_id", kvp.Key },
                    { "guild_name", kvp.Value.GuildName },
                    { "kills", kvp.Value.Kills },
                    { "deaths", kvp.Value.Deaths },
                    { "damage", kvp.Value.Damage },
                    { "healing", kvp.Value.Healing },
                    { "score", kvp.Value.Score }
                });
            }
            data["guild_scores"] = scoresList;
            
            // Save history
            Godot.Collections.Array historyList = new Godot.Collections.Array();
            foreach (var tournament in _tournamentHistory)
            {
                historyList.Add(new Godot.Collections.Dictionary
                {
                    { "id", tournament.Id },
                    { "name", tournament.Name },
                    { "type", (int)tournament.Type },
                    { "winner_guild_id", tournament.WinnerGuildId },
                    { "winner_guild_name", tournament.WinnerGuildName }
                });
            }
            data["tournament_history"] = historyList;
            
            return data;
        }
        
        /// <summary>
        /// Load tournament data
        /// </summary>
        protected override void ImportSaveData(Dictionary data)
        {
            if (data == null)
                return;
                
            _guildScores.Clear();
            
            // Load scores
            if (data.ContainsKey("guild_scores"))
            {
                var scoresList = data["guild_scores"] as Godot.Collections.Array;
                if (scoresList != null)
                {
                    foreach (var scoreData in scoresList)
                    {
                        var dict = scoreData as Godot.Collections.Dictionary;
                        if (dict == null) continue;
                        
                        var score = new GuildTournamentScore
                        {
                            GuildId = Convert.ToInt32(dict["guild_id"]),
                            GuildName = dict["guild_name"].ToString(),
                            Kills = Convert.ToInt32(dict["kills"]),
                            Deaths = Convert.ToInt32(dict["deaths"]),
                            Damage = Convert.ToSingle(dict["damage"]),
                            Healing = Convert.ToSingle(dict["healing"]),
                            Score = Convert.ToInt32(dict["score"])
                        };
                        _guildScores[score.GuildId] = score;
                    }
                }
            }
            
            GD.Print($"[GuildTournament] Loaded {_guildScores.Count} guild scores");
        }
    }
    
    // Data classes
    public class TournamentData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public GuildTournamentSystem.TournamentType Type { get; set; }
        public GuildTournamentSystem.TournamentState State { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime MatchStartTime { get; set; }
        public List<int> RegisteredGuilds { get; set; }
        public List<TournamentMatch> Matches { get; set; }
        public int WinnerGuildId { get; set; }
        public string WinnerGuildName { get; set; }
        public int PrizePool { get; set; }
    }
    
    public class TournamentMatch
    {
        public int Id { get; set; }
        public int Round { get; set; }
        public int Guild1Id { get; set; }
        public int Guild2Id { get; set; }
        public int Guild1Score { get; set; }
        public int Guild2Score { get; set; }
        public int WinnerId { get; set; }
        public bool IsComplete { get; set; }
    }
    
    public class GuildTournamentScore
    {
        public int GuildId { get; set; }
        public string GuildName { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public float Damage { get; set; }
        public float Healing { get; set; }
        public int Score { get; set; }
        public int Rank { get; set; }
    }
}
