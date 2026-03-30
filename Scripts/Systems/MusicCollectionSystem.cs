using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Music Collection System - Allows players to unlock and collect music tracks
    /// </summary>
    public class MusicCollectionSystem : BaseSystem {
        private static MusicCollectionSystem _instance;
        public static MusicCollectionSystem Instance => _instance;
        
        // Player music collection data
        private Dictionary<string, MusicCollectionEntry> _unlockedTracks;
        private HashSet<string> _favoriteTracks;
        private string _currentlyPlaying = "";
        
        // Track database
        private Dictionary<string, MusicCollectionEntry> _trackDatabase;
        
        // Statistics
        private int _totalPlayTime = 0;
        private int _timesPlayed = 0;
        
        public override void _Ready() {
            _instance = this;
            _unlockedTracks = new Dictionary<string, MusicCollectionEntry>();
            _favoriteTracks = new HashSet<string>();
            _trackDatabase = new Dictionary<string, MusicCollectionEntry>();
            
            InitializeTrackDatabase();
            LoadData();
            
            GD.Print("MusicCollectionSystem initialized");
        }
        
        private void InitializeTrackDatabase() {
            // Exploration tracks
            AddTrack("bgm_default", "Default Theme", MusicCategory.Exploration, true, "Default exploration theme");
            AddTrack("bgm_forest", "Forest Theme", MusicCategory.Exploration, true, "Peaceful forest ambience");
            AddTrack("bgm_town", "Town Theme", MusicCategory.Exploration, true, "Busy town atmosphere");
            AddTrack("bgm_mountain", "Mountain Theme", MusicCategory.Exploration, false, "Rocky mountain trails");
            AddTrack("bgm_desert", "Desert Theme", MusicCategory.Exploration, false, "Hot desert winds");
            AddTrack("bgm_ocean", "Ocean Theme", MusicCategory.Exploration, false, "Calm ocean waves");
            AddTrack("bgm_volcano", "Volcano Theme", MusicCategory.Exploration, false, "Volcanic danger");
            AddTrack("bgm_ice", "Ice Theme", MusicCategory.Exploration, false, "Frozen tundra");
            AddTrack("bgm_swamp", "Swamp Theme", MusicCategory.Exploration, false, "Mysterious swamp");
            AddTrack("bgm_ruins", "Ancient Ruins", MusicCategory.Exploration, false, "Forgotten ruins");
            AddTrack("bgm_castle", "Castle Theme", MusicCategory.Exploration, false, "Majestic castle");
            AddTrack("bgm_underwater", "Underwater Theme", MusicCategory.Exploration, false, "Deep ocean depths");
            
            // Battle tracks
            AddTrack("battle_normal", "Normal Battle", MusicCategory.Battle, true, "Standard combat music");
            AddTrack("battle_boss", "Boss Battle", MusicCategory.Battle, true, "Epic boss fight");
            AddTrack("battle_miniboss", "Mini Boss", MusicCategory.Battle, false, "Mini boss encounter");
            AddTrack("battle_final", "Final Battle", MusicCategory.Battle, false, "Final confrontation");
            AddTrack("battle_pvp", "PvP Arena", MusicCategory.Battle, false, "Player vs player");
            AddTrack("battle_siege", "Siege Battle", MusicCategory.Battle, false, "Castle siege");
            
            // Event tracks
            AddTrack("event_victory", "Victory Fanfare", MusicCategory.Event, true, "Celebration music");
            AddTrack("event_defeat", "Defeat Theme", MusicCategory.Event, true, "Sad reflection");
            AddTrack("event_shop", "Shop Theme", MusicCategory.Event, true, "Merchant tunes");
            AddTrack("event_inn", "Inn Theme", MusicCategory.Event, true, "Cozy tavern");
            AddTrack("event_festival", "Festival Theme", MusicCategory.Event, false, "Grand festival");
            AddTrack("event_wedding", "Wedding March", MusicCategory.Event, false, "Celebration of love");
            AddTrack("event_funeral", "Memorial Theme", MusicCategory.Event, false, "Honoring the fallen");
            
            // Menu tracks
            AddTrack("menu_main", "Main Menu", MusicCategory.Menu, true, "Title screen music");
            AddTrack("menu_options", "Options Theme", MusicCategory.Menu, true, "Settings music");
            AddTrack("menu_credits", "Credits Theme", MusicCategory.Menu, false, "End credits");
            
            // Special tracks
            AddTrack("special_secret", "Secret Theme", MusicCategory.Special, false, "Hidden discovery");
            AddTrack("special_dream", "Dream Sequence", MusicCategory.Special, false, "Mysterious dreams");
            AddTrack("special_memory", "Memory Lane", MusicCategory.Special, false, "Nostalgic moments");
            AddTrack("special_final", "Final Farewell", MusicCategory.Special, false, "Emotional goodbye");
        }
        
        private void AddTrack(string id, string name, MusicCategory category, bool defaultUnlocked, string description) {
            _trackDatabase[id] = new MusicCollectionEntry {
                Id = id,
                Name = name,
                Category = category,
                IsUnlocked = defaultUnlocked,
                Description = description,
                UnlockCondition = GetUnlockCondition(category),
                Rarity = GetTrackRarity(id)
            };
            
            if (defaultUnlocked) {
                _unlockedTracks[id] = _trackDatabase[id];
            }
        }
        
        private string GetUnlockCondition(MusicCategory category) {
            switch (category) {
                case MusicCategory.Exploration:
                    return "Default";
                case MusicCategory.Battle:
                    return "Default";
                case MusicCategory.Event:
                    return "Default";
                case MusicCategory.Menu:
                    return "Default";
                case MusicCategory.Special:
                    return "Complete the game";
                default:
                    return "Unknown";
            }
        }
        
        private TrackRarity GetTrackRarity(string id) {
            if (id.Contains("secret") || id.Contains("final")) return TrackRarity.Legendary;
            if (id.Contains("boss") || id.Contains("siege")) return TrackRarity.Epic;
            if (id.Contains("mini") || id.Contains("pvp")) return TrackRarity.Rare;
            if (id.Contains("festival") || id.Contains("wedding")) return TrackRarity.Uncommon;
            return TrackRarity.Common;
        }
        
        #region Public Methods
        
        /// <summary>
        /// Get all tracks in the database
        /// </summary>
        public Dictionary<string, MusicCollectionEntry> GetAllTracks() => _trackDatabase;
        
        /// <summary>
        /// Get all unlocked tracks
        /// </summary>
        public Dictionary<string, MusicCollectionEntry> GetUnlockedTracks() => _unlockedTracks;
        
        /// <summary>
        /// Get tracks by category
        /// </summary>
        public List<MusicCollectionEntry> GetTracksByCategory(MusicCategory category) {
            var result = new List<MusicCollectionEntry>();
            foreach (var track in _unlockedTracks.Values) {
                if (track.Category == category) {
                    result.Add(track);
                }
            }
            return result;
        }
        
        /// <summary>
        /// Unlock a track
        /// </summary>
        public bool UnlockTrack(string trackId) {
            if (_trackDatabase.TryGetValue(trackId, out var track) && !_unlockedTracks.ContainsKey(trackId)) {
                _unlockedTracks[trackId] = track;
                track.IsUnlocked = true;
                SaveData();
                EmitSignal(nameof(TrackUnlocked), trackId);
                GD.Print($"[MusicCollection] Unlocked: {track.Name}");
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Check if track is unlocked
        /// </summary>
        public bool IsTrackUnlocked(string trackId) {
            return _unlockedTracks.ContainsKey(trackId);
        }
        
        /// <summary>
        /// Add track to favorites
        /// </summary>
        public bool AddFavorite(string trackId) {
            if (_unlockedTracks.ContainsKey(trackId) && !_favoriteTracks.Contains(trackId)) {
                _favoriteTracks.Add(trackId);
                SaveData();
                EmitSignal(nameof(FavoriteAdded), trackId);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Remove track from favorites
        /// </summary>
        public bool RemoveFavorite(string trackId) {
            if (_favoriteTracks.Contains(trackId)) {
                _favoriteTracks.Remove(trackId);
                SaveData();
                EmitSignal(nameof(FavoriteRemoved), trackId);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Check if track is favorite
        /// </summary>
        public bool IsFavorite(string trackId) {
            return _favoriteTracks.Contains(trackId);
        }
        
        /// <summary>
        /// Get favorite tracks
        /// </summary>
        public List<MusicCollectionEntry> GetFavorites() {
            var result = new List<MusicCollectionEntry>();
            foreach (var id in _favoriteTracks) {
                if (_unlockedTracks.TryGetValue(id, out var track)) {
                    result.Add(track);
                }
            }
            return result;
        }
        
        /// <summary>
        /// Record track play
        /// </summary>
        public void RecordPlay(string trackId, int durationSeconds) {
            _currentlyPlaying = trackId;
            _timesPlayed++;
            _totalPlayTime += durationSeconds;
            
            if (_unlockedTracks.TryGetValue(trackId, out var track)) {
                track.PlayCount++;
                track.TotalPlayTime += durationSeconds;
            }
            
            SaveData();
        }
        
        /// <summary>
        /// Get collection progress
        /// </summary>
        public (int unlocked, int total) GetCollectionProgress() {
            return (_unlockedTracks.Count, _trackDatabase.Count);
        }
        
        /// <summary>
        /// Get favorite count
        /// </summary>
        public int GetFavoriteCount() => _favoriteTracks.Count;
        
        /// <summary>
        /// Get most played track
        /// </summary>
        public MusicCollectionEntry GetMostPlayedTrack() {
            MusicCollectionEntry result = null;
            int maxPlays = 0;
            
            foreach (var track in _unlockedTracks.Values) {
                if (track.PlayCount > maxPlays) {
                    maxPlays = track.PlayCount;
                    result = track;
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Get total play time
        /// </summary>
        public int GetTotalPlayTime() => _totalPlayTime;
        
        /// <summary>
        /// Get play count
        /// </summary>
        public int GetPlayCount() => _timesPlayed;
        
        /// <summary>
        /// Get currently playing track
        /// </summary>
        public string GetCurrentlyPlaying() => _currentlyPlaying;
        
        /// <summary>
        /// Unlock all tracks (for testing)
        /// </summary>
        public void UnlockAllTracks() {
            foreach (var track in _trackDatabase.Values) {
                if (!_unlockedTracks.ContainsKey(track.Id)) {
                    _unlockedTracks[track.Id] = track;
                    track.IsUnlocked = true;
                }
            }
            SaveData();
            EmitSignal(nameof(AllTracksUnlocked));
        }
        
        /// <summary>
        /// Get tracks by rarity
        /// </summary>
        public List<MusicCollectionEntry> GetTracksByRarity(TrackRarity rarity) {
            var result = new List<MusicCollectionEntry>();
            foreach (var track in _unlockedTracks.Values) {
                if (track.Rarity == rarity) {
                    result.Add(track);
                }
            }
            return result;
        }
        
        #endregion
        
        #region Save/Load
        
        private void LoadData() {
            var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
            if (saveSystem == null) return;
            
            var data = saveSystem.GetPlayerData();
            if (data == null) return;
            
            if (data.ContainsKey("music_collection")) {
                var collection = (Godot.Collections.Dictionary)data["music_collection"];
                
                // Load unlocked tracks
                if (collection.Contains("unlocked")) {
                    var unlocked = (Godot.Collections.Array)collection["unlocked"];
                    foreach (string trackId in unlocked) {
                        if (_trackDatabase.TryGetValue(trackId, out var track)) {
                            _unlockedTracks[trackId] = track;
                            track.IsUnlocked = true;
                        }
                    }
                }
                
                // Load favorites
                if (collection.Contains("favorites")) {
                    var favorites = (Godot.Collections.Array)collection["favorites"];
                    foreach (string trackId in favorites) {
                        _favoriteTracks.Add(trackId);
                    }
                }
                
                // Load stats
                if (collection.Contains("total_play_time")) {
                    _totalPlayTime = (int)collection["total_play_time"];
                }
                if (collection.Contains("times_played")) {
                    _timesPlayed = (int)collection["times_played"];
                }
                
                // Load track-specific data
                if (collection.Contains("track_data")) {
                    var trackData = (Godot.Collections.Dictionary)collection["track_data"];
                    foreach (string trackId in trackData.Keys) {
                        if (_unlockedTracks.TryGetValue(trackId, out var track)) {
                            var trackInfo = (Godot.Collections.Dictionary)trackData[trackId];
                            if (trackInfo.Contains("play_count")) {
                                track.PlayCount = (int)trackInfo["play_count"];
                            }
                            if (trackInfo.Contains("total_play_time")) {
                                track.TotalPlayTime = (int)trackInfo["total_play_time"];
                            }
                        }
                    }
                }
            }
        }
        
        private void SaveData() {
            var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
            if (saveSystem == null) return;
            
            var data = saveSystem.GetPlayerData();
            
            var collection = new Godot.Collections.Dictionary();
            
            // Save unlocked tracks
            var unlocked = new Godot.Collections.Array();
            foreach (var trackId in _unlockedTracks.Keys) {
                unlocked.Add(trackId);
            }
            collection["unlocked"] = unlocked;
            
            // Save favorites
            var favorites = new Godot.Collections.Array();
            foreach (var trackId in _favoriteTracks) {
                favorites.Add(trackId);
            }
            collection["favorites"] = favorites;
            
            // Save stats
            collection["total_play_time"] = _totalPlayTime;
            collection["times_played"] = _timesPlayed;
            
            // Save track-specific data
            var trackData = new Godot.Collections.Dictionary();
            foreach (var track in _unlockedTracks.Values) {
                var trackInfo = new Godot.Collections.Dictionary();
                trackInfo["play_count"] = track.PlayCount;
                trackInfo["total_play_time"] = track.TotalPlayTime;
                trackData[track.Id] = trackInfo;
            }
            collection["track_data"] = trackData;
            
            data["music_collection"] = collection;
            saveSystem.SavePlayerData();
        }
        
        #endregion
        
        #region Signals
public delegate void TrackUnlocked(string trackId);
public delegate void FavoriteAdded(string trackId);
public delegate void FavoriteRemoved(string trackId);
public delegate void AllTracksUnlocked();
        
        #endregion
        
        public override void _ExitTree() {
            if (_instance == this) {
                _instance = null;
            }
        }
        
        /// <summary>
        /// Export save data for persistence
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            // 已解锁曲目
            var unlocked = new Array();
            foreach (var trackId in _unlockedTracks.Keys) {
                unlocked.Add(trackId);
            }
            data["unlocked"] = unlocked;
            
            // 收藏
            var favorites = new Array();
            foreach (var trackId in _favoriteTracks) {
                favorites.Add(trackId);
            }
            data["favorites"] = favorites;
            
            // 统计
            data["total_play_time"] = _totalPlayTime;
            data["times_played"] = _timesPlayed;
            
            // 曲目数据
            var trackData = new Dictionary();
            foreach (var track in _unlockedTracks.Values) {
                var trackInfo = new Dictionary
                {
                    { "play_count", track.PlayCount },
                    { "total_play_time", track.TotalPlayTime }
                };
                trackData[track.Id] = trackInfo;
            }
            data["track_data"] = trackData;
            
            return data;
        }
        
        /// <summary>
        /// Import save data from persistence
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            // Load unlocked tracks
            if (data.Contains("unlocked")) {
                var unlocked = (Array)data["unlocked"];
                foreach (string trackId in unlocked) {
                    if (_trackDatabase.TryGetValue(trackId, out var track)) {
                        _unlockedTracks[trackId] = track;
                        track.IsUnlocked = true;
                    }
                }
            }
            
            // Load favorites
            if (data.Contains("favorites")) {
                var favorites = (Array)data["favorites"];
                foreach (string trackId in favorites) {
                    _favoriteTracks.Add(trackId);
                }
            }
            
            // Load stats
            if (data.Contains("total_play_time")) {
                _totalPlayTime = (int)data["total_play_time"];
            }
            if (data.Contains("times_played")) {
                _timesPlayed = (int)data["times_played"];
            }
            
            // Load track-specific data
            if (data.Contains("track_data")) {
                var trackData = (Dictionary)data["track_data"];
                foreach (string trackId in trackData.Keys) {
                    if (_unlockedTracks.TryGetValue(trackId, out var track)) {
                        var trackInfo = (Dictionary)trackData[trackId];
                        if (trackInfo.Contains("play_count")) {
                            track.PlayCount = (int)trackInfo["play_count"];
                        }
                        if (trackInfo.Contains("total_play_time")) {
                            track.TotalPlayTime = (int)trackInfo["total_play_time"];
                        }
                    }
                }
            }
        }
    }
    
    #region Data Classes
    
    public class MusicCollectionEntry {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public MusicCategory Category { get; set; } = MusicCategory.Exploration;
        public bool IsUnlocked { get; set; } = false;
        public string Description { get; set; } = "";
        public string UnlockCondition { get; set; } = "";
        public TrackRarity Rarity { get; set; } = TrackRarity.Common;
        public int PlayCount { get; set; } = 0;
        public int TotalPlayTime { get; set; } = 0;
    }
    
    public enum TrackRarity {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    #endregion
}
