using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Multiplayer Leaderboard System
    /// Tracks and displays player rankings in various categories
    /// </summary>
    public partial class MultiplayerLeaderboard : BaseSystem
    {
        public static MultiplayerLeaderboard Instance { get; private set; }

        // Leaderboard entry
        public class LeaderboardEntry
        {
            public int Rank;
            public string PlayerId;
            public string PlayerName;
            public int Value;
            public DateTime LastUpdated;
        }

        // Leaderboard categories
        public enum LeaderboardCategory
        {
            Kills,
            DamageDealt,
            GoldEarned,
            BossesDefeated,
            DungeonsCleared,
            PvPWins,
            SurvivalTime,
            ComboMaster
        }

        // Leaderboard data
        private Dictionary<LeaderboardCategory, List<LeaderboardEntry>> _leaderboards = 
            new Dictionary<LeaderboardCategory, List<LeaderboardEntry>>();
        
        // Current session stats
        private int _sessionKills = 0;
        private int _sessionDamageDealt = 0;
        private int _sessionGoldEarned = 0;
        private int _sessionBossesDefeated = 0;
        private int _sessionDungeonsCleared = 0;
        private int _sessionPvPWins = 0;
        private float _sessionSurvivalTime = 0;
        private int _sessionMaxCombo = 0;

        // Signals
        public delegate void LeaderboardUpdatedEvent(LeaderboardCategory category);
        public event LeaderboardUpdatedEvent OnLeaderboardUpdated;

        public override void _Ready()
        {
            Instance = this;
            InitializeLeaderboards();
        }

        private void InitializeLeaderboards()
        {
            foreach (LeaderboardCategory category in Enum.GetValues(typeof(LeaderboardCategory)))
            {
                _leaderboards[category] = new List<LeaderboardEntry>();
            }
        }

        // Session stat tracking
        public void AddKill()
        {
            _sessionKills++;
            UpdateLocalEntry(LeaderboardCategory.Kills, _sessionKills);
        }

        public void AddDamage(int damage)
        {
            _sessionDamageDealt += damage;
            UpdateLocalEntry(LeaderboardCategory.DamageDealt, _sessionDamageDealt);
        }

        public void AddGold(int gold)
        {
            _sessionGoldEarned += gold;
            UpdateLocalEntry(LeaderboardCategory.GoldEarned, _sessionGoldEarned);
        }

        public void AddBossDefeated()
        {
            _sessionBossesDefeated++;
            UpdateLocalEntry(LeaderboardCategory.BossesDefeated, _sessionBossesDefeated);
        }

        public void AddDungeonCleared()
        {
            _sessionDungeonsCleared++;
            UpdateLocalEntry(LeaderboardCategory.DungeonsCleared, _sessionDungeonsCleared);
        }

        public void AddPvPWin()
        {
            _sessionPvPWins++;
            UpdateLocalEntry(LeaderboardCategory.PvPWins, _sessionPvPWins);
        }

        public void UpdateSurvivalTime(float time)
        {
            _sessionSurvivalTime = time;
            UpdateLocalEntry(LeaderboardCategory.SurvivalTime, (int)_sessionSurvivalTime);
        }

        public void UpdateMaxCombo(int combo)
        {
            if (combo > _sessionMaxCombo)
            {
                _sessionMaxCombo = combo;
                UpdateLocalEntry(LeaderboardCategory.ComboMaster, _sessionMaxCombo);
            }
        }

        private void UpdateLocalEntry(LeaderboardCategory category, int value)
        {
            var playerId = GetPlayerId();
            var playerName = GetPlayerName();
            
            var entries = _leaderboards[category];
            var existingEntry = entries.Find(e => e.PlayerId == playerId);
            
            if (existingEntry != null)
            {
                existingEntry.Value = value;
                existingEntry.LastUpdated = DateTime.Now;
            }
            else
            {
                entries.Add(new LeaderboardEntry
                {
                    Rank = entries.Count + 1,
                    PlayerId = playerId,
                    PlayerName = playerName,
                    Value = value,
                    LastUpdated = DateTime.Now
                });
            }
            
            // Sort and update ranks
            entries.Sort((a, b) => b.Value.CompareTo(a.Value));
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Rank = i + 1;
            }
            
            OnLeaderboardUpdated?.Invoke(category);
        }

        // Add NPC/AI entries for single-player leaderboard
        public void AddAIEntry(LeaderboardCategory category, string name, int value)
        {
            var entries = _leaderboards[category];
            entries.Add(new LeaderboardEntry
            {
                Rank = entries.Count + 1,
                PlayerId = "ai_" + name.ToLower().Replace(" ", "_"),
                PlayerName = name,
                Value = value,
                LastUpdated = DateTime.Now
            });
            
            entries.Sort((a, b) => b.Value.CompareTo(a.Value));
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Rank = i + 1;
            }
            
            OnLeaderboardUpdated?.Invoke(category);
        }

        // Get leaderboard data
        public List<LeaderboardEntry> GetLeaderboard(LeaderboardCategory category, int limit = 10)
        {
            if (_leaderboards.TryGetValue(category, out var entries))
            {
                if (limit > 0 && entries.Count > limit)
                {
                    return entries.GetRange(0, limit);
                }
                return new List<LeaderboardEntry>(entries);
            }
            return new List<LeaderboardEntry>();
        }

        // Get player rank in category
        public int GetPlayerRank(LeaderboardCategory category)
        {
            var playerId = GetPlayerId();
            if (_leaderboards.TryGetValue(category, out var entries))
            {
                var entry = entries.Find(e => e.PlayerId == playerId);
                return entry?.Rank ?? 0;
            }
            return 0;
        }

        // Get player value in category
        public int GetPlayerValue(LeaderboardCategory category)
        {
            var playerId = GetPlayerId();
            if (_leaderboards.TryGetValue(category, out var entries))
            {
                var entry = entries.Find(e => e.PlayerId == playerId);
                return entry?.Value ?? 0;
            }
            return 0;
        }

        private string GetPlayerId()
        {
            if (MultiplayerManager.Instance != null)
            {
                return "player_" + MultiplayerManager.Instance.LocalPlayerId;
            }
            return "player_local";
        }

        private string GetPlayerName()
        {
            if (MultiplayerManager.Instance != null)
            {
                return MultiplayerManager.Instance.PlayerName;
            }
            return "You";
        }

        // Get category display name
        public static string GetCategoryName(LeaderboardCategory category)
        {
            return category switch
            {
                LeaderboardCategory.Kills => "击杀榜",
                LeaderboardCategory.DamageDealt => "伤害榜",
                LeaderboardCategory.GoldEarned => "财富榜",
                LeaderboardCategory.BossesDefeated => "Boss榜",
                LeaderboardCategory.DungeonsCleared => "副本榜",
                LeaderboardCategory.PvPWins => "PVP榜",
                LeaderboardCategory.SurvivalTime => "生存榜",
                LeaderboardCategory.ComboMaster => "连击榜",
                _ => category.ToString()
            };
        }

        // Reset session stats
        public void ResetSessionStats()
        {
            _sessionKills = 0;
            _sessionDamageDealt = 0;
            _sessionGoldEarned = 0;
            _sessionBossesDefeated = 0;
            _sessionDungeonsCleared = 0;
            _sessionPvPWins = 0;
            _sessionSurvivalTime = 0;
            _sessionMaxCombo = 0;
        }

        // Load/Save for single-player mode
        public Dictionary<string, object> GetSaveData()
        {
            var data = new Dictionary<string, object>();
            
            foreach (var kvp in _leaderboards)
            {
                var entries = new List<Dictionary<string, object>>();
                foreach (var entry in kvp.Value)
                {
                    entries.Add(new Dictionary<string, object>
                    {
                        { "player_id", entry.PlayerId },
                        { "player_name", entry.PlayerName },
                        { "value", entry.Value },
                        { "last_updated", entry.LastUpdated.ToString("o") }
                    });
                }
                data[kvp.Key.ToString()] = entries;
            }
            
            return data;
        }

        public void LoadSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            InitializeLeaderboards();
            
            foreach (var kvp in data)
            {
                if (Enum.TryParse<LeaderboardCategory>(kvp.Key, out var category))
                {
                    var entriesList = kvp.Value as List<object>;
                    if (entriesList != null)
                    {
                        foreach (var entryObj in entriesList)
                        {
                            var entryDict = entryObj as Dictionary<string, object>;
                            if (entryDict != null)
                            {
                                var entry = new LeaderboardEntry
                                {
                                    PlayerId = entryDict.GetValueOrDefault("player_id", "").ToString(),
                                    PlayerName = entryDict.GetValueOrDefault("player_name", "").ToString(),
                                    Value = Convert.ToInt32(entryDict.GetValueOrDefault("value", 0)),
                                    LastUpdated = DateTime.TryParse(
                                        entryDict.GetValueOrDefault("last_updated", "").ToString(), 
                                        out var dt) ? dt : DateTime.Now
                                };
                                _leaderboards[category].Add(entry);
                            }
                        }
                        
                        // Sort and rank
                        var entries = _leaderboards[category];
                        entries.Sort((a, b) => b.Value.CompareTo(a.Value));
                        for (int i = 0; i < entries.Count; i++)
                        {
                            entries[i].Rank = i + 1;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Export save data for persistence
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            foreach (var kvp in _leaderboards)
            {
                var entries = new Godot.Collections.Array();
                foreach (var entry in kvp.Value)
                {
                    var entryData = new Dictionary
                    {
                        { "player_id", entry.PlayerId },
                        { "player_name", entry.PlayerName },
                        { "value", entry.Value },
                        { "last_updated", entry.LastUpdated.ToString("o") }
                    };
                    entries.Add(entryData);
                }
                data[kvp.Key.ToString()] = entries;
            }
            
            return data;
        }
        
        /// <summary>
        /// Import save data from persistence
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            InitializeLeaderboards();
            
            foreach (var kvp in data)
            {
                if (Enum.TryParse<LeaderboardCategory>(kvp.Key, out var category))
                {
                    var entriesList = kvp.Value as Array;
                    if (entriesList != null)
                    {
                        foreach (var entryObj in entriesList)
                        {
                            var entryDict = entryObj as Dictionary;
                            if (entryDict != null)
                            {
                                var entry = new LeaderboardEntry
                                {
                                    PlayerId = entryDict.Get("player_id", "").ToString(),
                                    PlayerName = entryDict.Get("player_name", "").ToString(),
                                    Value = (int)entryDict.Get("value", 0),
                                    LastUpdated = DateTime.TryParse(
                                        entryDict.Get("last_updated", "").ToString(), 
                                        out var dt) ? dt : DateTime.Now
                                };
                                _leaderboards[category].Add(entry);
                            }
                        }
                        
                        // Sort and rank
                        var entries = _leaderboards[category];
                        entries.Sort((a, b) => b.Value.CompareTo(a.Value));
                        for (int i = 0; i < entries.Count; i++)
                        {
                            entries[i].Rank = i + 1;
                        }
                    }
                }
            }
        }
    }
}
