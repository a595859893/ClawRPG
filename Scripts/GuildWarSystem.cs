using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Core.Systems.GuildWar
{
    /// <summary>
    /// Guild War System - Core system for guild battles
    /// </summary>
    public partial class GuildWarSystem : Node
    {
        public static GuildWarSystem Instance { get; private set; }

        // Active wars
        private Dictionary<string, GuildWar> _activeWars = new Dictionary<string, GuildWar>();
        
        // War history
        private List<GuildWar> _warHistory = new List<GuildWar>();
        
        // Territory control
        private List<TerritoryControl> _territories = new List<TerritoryControl>();
        
        // Guild statistics
        private Dictionary<string, GuildWarStatistics> _guildStatistics = new Dictionary<string, GuildWarStatistics>();
        
        // Player progress
        private Dictionary<string, GuildWarProgress> _playerProgress = new Dictionary<string, GuildWarProgress>();

        // Signals
        public signal void WarStarted(string warId, string warName);
        public signal void WarEnded(string warId, string winnerId, List<GuildWarParticipant> rankings);
        public signal void BattleOccurred(string battleId, string warId, string attackerId, string defenderId, BattleResult result);
        public signal void TerritoryCaptured(string territoryId, string guildId, string guildName);
        public signal void ScoreUpdated(string warId, string guildId, int newScore);

        public override void _Ready()
        {
            Instance = this;
            InitializeTerritories();
            LoadData();
        }

        private void InitializeTerritories()
        {
            foreach (var config in GuildWarDatabase.TerritoryConfigs)
            {
                _territories.Add(new TerritoryControl
                {
                    TerritoryId = config.TerritoryId,
                    TerritoryName = config.Name,
                    ControllingGuildId = "neutral",
                    ControllingGuildName = "Unclaimed",
                    CaptureTime = DateTime.MinValue,
                    DefenseLevel = config.DefenseLevel,
                    ResourceGeneration = config.ResourceGeneration
                });
            }
        }

        #region War Management

        /// <summary>
        /// Create a new guild war
        /// </summary>
        public GuildWar CreateWar(string name, string description, GuildWarType type, int duration, int maxGuilds, int minGuildLevel, int entryFee, string mapId)
        {
            var config = GuildWarDatabase.GetConfig(type);
            var warId = GenerateWarId();
            
            var war = new GuildWar
            {
                WarId = warId,
                Name = name,
                Description = description,
                Type = type,
                State = GuildWarState.Preparation,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddMinutes(duration),
                Duration = duration,
                MaxGuilds = maxGuilds > 0 ? maxGuilds : config.MaxGuilds,
                MinGuildLevel = minGuildLevel,
                EntryFee = entryFee,
                MapId = mapId,
                PrizePool = 0
            };

            _activeWars[warId] = war;
            SaveData();
            
            GD.Print($"[GuildWar] Created war: {name} ({type}) - {warId}");
            return war;
        }

        /// <summary>
        /// Start registration for a war
        /// </summary>
        public bool StartRegistration(string warId)
        {
            if (!_activeWars.ContainsKey(warId)) return false;
            
            var war = _activeWars[warId];
            if (war.State != GuildWarState.Preparation) return false;
            
            war.State = GuildWarState.Registration;
            SaveData();
            
            GD.Print($"[GuildWar] Registration started for war: {warId}");
            return true;
        }

        /// <summary>
        /// Register a guild for war
        /// </summary>
        public bool RegisterGuild(string warId, string guildId, string guildName, int powerLevel, int membersOnline)
        {
            if (!_activeWars.ContainsKey(warId)) return false;
            
            var war = _activeWars[warId];
            if (war.State != GuildWarState.Registration) return false;
            if (war.Participants.Count >= war.MaxGuilds) return false;
            if (war.Participants.Any(p => p.GuildId == guildId)) return false;

            var participant = new GuildWarParticipant
            {
                GuildId = guildId,
                GuildName = guildName,
                PowerLevel = powerLevel,
                MembersOnline = membersOnline,
                Score = 0,
                Kills = 0,
                Deaths = 0,
                ResourcesCaptured = 0,
                Result = BattleResult.Pending,
                Rank = war.Participants.Count + 1
            };

            war.Participants.Add(participant);
            war.PrizePool += war.EntryFee;
            
            // Initialize guild stats if not exists
            if (!_guildStatistics.ContainsKey(guildId))
            {
                _guildStatistics[guildId] = new GuildWarStatistics
                {
                    GuildId = guildId,
                    TotalWars = 0,
                    Wins = 0,
                    Losses = 0,
                    Draws = 0
                };
            }
            
            SaveData();
            GD.Print($"[GuildWar] Guild {guildName} registered for war {warId}");
            return true;
        }

        /// <summary>
        /// Start the war
        /// </summary>
        public bool StartWar(string warId)
        {
            if (!_activeWars.ContainsKey(warId)) return false;
            
            var war = _activeWars[warId];
            if (war.State != GuildWarState.Registration) return false;
            if (war.Participants.Count < 2) return false;
            
            war.State = GuildWarState.Active;
            war.StartTime = DateTime.Now;
            war.EndTime = DateTime.Now.AddMinutes(war.Duration);
            
            // Sort by power level for seeding
            war.Participants = war.Participants.OrderByDescending(p => p.PowerLevel).ToList();
            for (int i = 0; i < war.Participants.Count; i++)
            {
                war.Participants[i].Rank = i + 1;
            }
            
            SaveData();
            WarStarted.Invoke(warId, war.Name);
            GD.Print($"[GuildWar] War started: {warId} with {war.Participants.Count} guilds");
            return true;
        }

        /// <summary>
        /// End the war and calculate results
        /// </summary>
        public bool EndWar(string warId)
        {
            if (!_activeWars.ContainsKey(warId)) return false;
            
            var war = _activeWars[warId];
            if (war.State != GuildWarState.Active) return false;
            
            war.State = GuildWarState.Completed;
            
            // Sort by score
            war.Participants = war.Participants.OrderByDescending(p => p.Score).ToList();
            for (int i = 0; i < war.Participants.Count; i++)
            {
                war.Participants[i].Rank = i + 1;
                
                // Determine result
                if (i == 0)
                {
                    war.Participants[i].Result = BattleResult.Victory;
                    war.WinnerId = war.Participants[i].GuildId;
                }
                else
                {
                    war.Participants[i].Result = BattleResult.Defeat;
                }
                
                // Update guild statistics
                UpdateGuildStats(war.Participants[i], i == 0);
            }
            
            // Distribute rewards
            DistributeRewards(war);
            
            _warHistory.Add(war);
            _activeWars.Remove(warId);
            
            SaveData();
            WarEnded.Invoke(warId, war.WinnerId, war.Participants);
            GD.Print($"[GuildWar] War ended: {warId}, Winner: {war.WinnerId}");
            return true;
        }

        #endregion

        #region Battle System

        /// <summary>
        /// Record a battle between guilds
        /// </summary>
        public GuildWarBattle RecordBattle(string warId, string attackerGuildId, string defenderGuildId, 
            int attackerScore, int defenderScore, BattleResult result, int duration)
        {
            if (!_activeWars.ContainsKey(warId)) return null;
            
            var war = _activeWars[warId];
            var attacker = war.Participants.FirstOrDefault(p => p.GuildId == attackerGuildId);
            var defender = war.Participants.FirstOrDefault(p => p.GuildId == defenderGuildId);
            
            if (attacker == null || defender == null) return null;
            
            var battle = new GuildWarBattle
            {
                BattleId = GenerateBattleId(),
                WarId = warId,
                AttackerGuildId = attackerGuildId,
                DefenderGuildId = defenderGuildId,
                AttackerName = attacker.GuildName,
                DefenderName = defender.GuildName,
                BattleTime = DateTime.Now,
                AttackerScore = attackerScore,
                DefenderScore = defenderScore,
                Result = result,
                Duration = duration
            };
            
            // Update scores
            attacker.Score += attackerScore;
            defender.Score += defenderScore;
            
            // Update kills/deaths
            if (result == BattleResult.Victory)
            {
                attacker.Kills++;
                defender.Deaths++;
            }
            else if (result == BattleResult.Defeat)
            {
                defender.Kills++;
                attacker.Deaths++;
            }
            
            SaveData();
            BattleOccurred.Invoke(battle.BattleId, warId, attackerGuildId, defenderGuildId, result);
            ScoreUpdated.Invoke(warId, attackerGuildId, attacker.Score);
            ScoreUpdated.Invoke(warId, defenderGuildId, defender.Score);
            
            GD.Print($"[GuildWar] Battle: {attacker.GuildName} vs {defender.GuildName}, Result: {result}");
            return battle;
        }

        /// <summary>
        /// Update guild contribution
        /// </summary>
        public void UpdateContribution(string warId, string playerId, string guildId, int contribution)
        {
            if (!_activeWars.ContainsKey(warId)) return;
            
            var war = _activeWars[warId];
            var participant = war.Participants.FirstOrDefault(p => p.GuildId == guildId);
            
            if (participant == null) return;
            
            // Update player progress
            if (!_playerProgress.ContainsKey(playerId))
            {
                _playerProgress[playerId] = new GuildWarProgress { PlayerId = playerId };
            }
            
            var progress = _playerProgress[playerId];
            progress.TotalContributions += contribution;
            progress.PersonalScore += contribution;
            
            if (progress.WarContributions.ContainsKey(warId))
                progress.WarContributions[warId] += contribution;
            else
                progress.WarContributions[warId] = contribution;
            
            SaveData();
        }

        #endregion

        #region Territory Control

        /// <summary>
        /// Capture a territory
        /// </summary>
        public bool CaptureTerritory(string territoryId, string guildId, string guildName)
        {
            var territory = _territories.FirstOrDefault(t => t.TerritoryId == territoryId);
            if (territory == null) return false;
            
            territory.ControllingGuildId = guildId;
            territory.ControllingGuildName = guildName;
            territory.CaptureTime = DateTime.Now;
            
            SaveData();
            TerritoryCaptured.Invoke(territoryId, guildId, guildName);
            GD.Print($"[GuildWar] Territory {territoryId} captured by {guildName}");
            return true;
        }

        /// <summary>
        /// Get territory info
        /// </summary>
        public TerritoryControl GetTerritory(string territoryId)
        {
            return _territories.FirstOrDefault(t => t.TerritoryId == territoryId);
        }

        /// <summary>
        /// Get all territories
        /// </summary>
        public List<TerritoryControl> GetAllTerritories()
        {
            return new List<TerritoryControl>(_territories);
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Update guild statistics
        /// </summary>
        private void UpdateGuildStats(GuildWarParticipant participant, bool isWinner)
        {
            if (!_guildStatistics.ContainsKey(participant.GuildId))
            {
                _guildStatistics[participant.GuildId] = new GuildWarStatistics { GuildId = participant.GuildId };
            }
            
            var stats = _guildStatistics[participant.GuildId];
            stats.TotalWars++;
            stats.TotalScore += participant.Score;
            stats.TotalKills += participant.Kills;
            stats.TotalDeaths += participant.Deaths;
            
            if (isWinner)
            {
                stats.Wins++;
                stats.CurrentWinStreak++;
                if (stats.CurrentWinStreak > stats.LongestWinStreak)
                    stats.LongestWinStreak = stats.CurrentWinStreak;
            }
            else
            {
                stats.Losses++;
                stats.CurrentWinStreak = 0;
            }
            
            if (participant.Rank < stats.HighestRank || stats.HighestRank == 0)
                stats.HighestRank = participant.Rank;
            
            stats.WarIds.Add(participant.GuildId);
        }

        /// <summary>
        /// Get guild statistics
        /// </summary>
        public GuildWarStatistics GetGuildStats(string guildId)
        {
            return _guildStatistics.ContainsKey(guildId) ? _guildStatistics[guildId] : null;
        }

        /// <summary>
        /// Get player progress
        /// </summary>
        public GuildWarProgress GetPlayerProgress(string playerId)
        {
            return _playerProgress.ContainsKey(playerId) ? _playerProgress[playerId] : null;
        }

        /// <summary>
        /// Get war rankings
        /// </summary>
        public List<GuildWarParticipant> GetWarRankings(string warId)
        {
            if (_warHistory.Any(w => w.WarId == warId))
            {
                var war = _warHistory.First(w => w.WarId == warId);
                return war.Participants.OrderByDescending(p => p.Score).ToList();
            }
            if (_activeWars.ContainsKey(warId))
            {
                return _activeWars[warId].Participants.OrderByDescending(p => p.Score).ToList();
            }
            return new List<GuildWarParticipant>();
        }

        #endregion

        #region Helpers

        private string GenerateWarId()
        {
            return $"war_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString()[..8]}";
        }

        private string GenerateBattleId()
        {
            return $"battle_{Guid.NewGuid().ToString()[..8]}";
        }

        private void DistributeRewards(GuildWar war)
        {
            for (int i = 0; i < Math.Min(war.Participants.Count, 10); i++)
            {
                var participant = war.Participants[i];
                var reward = GuildWarDatabase.GetReward(i + 1);
                if (reward == null) continue;
                
                var stats = _guildStatistics[participant.GuildId];
                stats.TotalPrizeEarned += reward.Gold;
                
                GD.Print($"[GuildWar] Guild {participant.GuildName} (Rank {i + 1}) received: {reward.Gold} gold, {reward.Experience} exp");
            }
        }

        #endregion

        #region Data Management

        private void LoadData()
        {
            // Load from save system (placeholder)
            GD.Print("[GuildWar] Data loaded");
        }

        private void SaveData()
        {
            // Save to save system (placeholder)
            // SaveSystem.Save("guild_war_data", this);
        }

        /// <summary>
        /// Get all active wars
        /// </summary>
        public Dictionary<string, GuildWar> GetActiveWars()
        {
            return new Dictionary<string, GuildWar>(_activeWars);
        }

        /// <summary>
        /// Get war history
        /// </summary>
        public List<GuildWar> GetWarHistory()
        {
            return new List<GuildWar>(_warHistory);
        }

        /// <summary>
        /// Get current war by ID
        /// </summary>
        public GuildWar GetWar(string warId)
        {
            if (_activeWars.ContainsKey(warId))
                return _activeWars[warId];
            return _warHistory.FirstOrDefault(w => w.WarId == warId);
        }

        #endregion

        #region Weekly Events

        /// <summary>
        /// Get scheduled wars for today
        /// </summary>
        public List<WarScheduleConfig> GetTodaysScheduledWars()
        {
            int dayOfWeek = (int)DateTime.Now.DayOfWeek;
            return GuildWarDatabase.WeeklySchedule.Where(s => s.DayOfWeek == dayOfWeek).ToList();
        }

        /// <summary>
        /// Auto-create scheduled wars
        /// </summary>
        public void CreateScheduledWars()
        {
            var scheduled = GetTodaysScheduledWars();
            foreach (var schedule in scheduled)
            {
                var config = GuildWarDatabase.GetConfig(schedule.WarType);
                var name = $"{config.Name} - {DateTime.Now:MM/dd}";
                CreateWar(name, config.Description, schedule.WarType, schedule.Duration, 0, 0, 0, "default");
            }
        }

        #endregion
    }
}
