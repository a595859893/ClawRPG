using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Auto bookmark system - automatically adds bookmarks for game content
    /// Listens to game events and adds bookmarks automatically
    /// </summary>
    public partial class AutoBookmarkSystem : BaseSystem {
        public static AutoBookmarkSystem Instance { get; private set; }
        
        // Signals
        [Signal] [Signal]
    public delegate void OnAutoBookmarkAdded(Bookmark bookmark, string trigger);
        
        // Settings
        [Export] public bool AutoBookmarkBoss = true;
        [Export] public bool AutoBookmarkShop = true;
        [Export] public bool AutoBookmarkQuest = true;
        [Export] public bool AutoBookmarkWaypoint = true;
        
        // Track discovered locations to avoid duplicates
        private HashSet<string> _discoveredLocations = new();
        
        public override void _Ready() {
            Instance = this;
            ConnectSignals();
            GD.Print("[AutoBookmarkSystem] Initialized");
        }
        
        /// <summary>
        /// 系统名称
        /// </summary>
        protected override string SystemName => "AutoBookmark";
        
        private void ConnectSignals() {
            // Connect to BossManager for boss defeat events
            var bossManager = GetNodeOrNull("/root/Main/BossManager");
            if (bossManager != null) {
                // Boss defeated signal would be connected here
            }
            
            // Connect to RegionManager for region entry
            var regionManager = GetNodeOrNull("/root/Main/RegionManager");
            if (regionManager != null) {
                // Region changed signal would be connected here
            }
        }
        
        /// <summary>
        /// Add bookmark for a defeated boss
        /// </summary>
        public void AddBossBookmark(string bossName, Vector2 position, int regionId) {
            if (!AutoBookmarkBoss) return;
            
            string locationKey = $"boss_{regionId}_{bossName}";
            if (_discoveredLocations.Contains(locationKey)) {
                return;
            }
            
            _discoveredLocations.Add(locationKey);
            
            var bookmarkSystem = BookmarkSystem.Instance;
            if (bookmarkSystem != null) {
                bool added = bookmarkSystem.AddBookmark(
                    $"⚔️ {bossName}",
                    BookmarkType.Auto,
                    BookmarkCategory.Boss,
                    position,
                    regionId,
                    $"Boss location - {bossName}"
                );
                
                if (added) {
                    OnAutoBookmarkAdded?.Invoke(
                        bookmarkSystem.PlayerBookmarks[bookmarkSystem.PlayerBookmarks.Count - 1],
                        "boss_defeat"
                    );
                    GD.Print($"[AutoBookmarkSystem] Added boss bookmark: {bossName}");
                }
            }
        }
        
        /// <summary>
        /// Add bookmark for a discovered shop
        /// </summary>
        public void AddShopBookmark(string shopName, Vector2 position, int regionId) {
            if (!AutoBookmarkShop) return;
            
            string locationKey = $"shop_{regionId}_{shopName}";
            if (_discoveredLocations.Contains(locationKey)) {
                return;
            }
            
            _discoveredLocations.Add(locationKey);
            
            var bookmarkSystem = BookmarkSystem.Instance;
            if (bookmarkSystem != null) {
                bool added = bookmarkSystem.AddBookmark(
                    $"🛒 {shopName}",
                    BookmarkType.Auto,
                    BookmarkCategory.Shop,
                    position,
                    regionId,
                    $"Shop location - {shopName}"
                );
                
                if (added) {
                    OnAutoBookmarkAdded?.Invoke(
                        bookmarkSystem.PlayerBookmarks[bookmarkSystem.PlayerBookmarks.Count - 1],
                        "shop_discovered"
                    );
                    GD.Print($"[AutoBookmarkSystem] Added shop bookmark: {shopName}");
                }
            }
        }
        
        /// <summary>
        /// Add bookmark for a quest location
        /// </summary>
        public void AddQuestBookmark(string questName, Vector2 position, int regionId, string targetName) {
            if (!AutoBookmarkQuest) return;
            
            string locationKey = $"quest_{questName}_{targetName}";
            if (_discoveredLocations.Contains(locationKey)) {
                return;
            }
            
            _discoveredLocations.Add(locationKey);
            
            var bookmarkSystem = BookmarkSystem.Instance;
            if (bookmarkSystem != null) {
                bool added = bookmarkSystem.AddBookmark(
                    $"📋 {questName} - {targetName}",
                    BookmarkType.Quest,
                    BookmarkCategory.Quest,
                    position,
                    regionId,
                    $"Quest: {questName}"
                );
                
                if (added) {
                    OnAutoBookmarkAdded?.Invoke(
                        bookmarkSystem.PlayerBookmarks[bookmarkSystem.PlayerBookmarks.Count - 1],
                        "quest_updated"
                    );
                    GD.Print($"[AutoBookmarkSystem] Added quest bookmark: {questName}");
                }
            }
        }
        
        /// <summary>
        /// Add bookmark for a fast travel waypoint
        /// </summary>
        public void AddWaypointBookmark(string waypointName, Vector2 position, int regionId) {
            if (!AutoBookmarkWaypoint) return;
            
            string locationKey = $"waypoint_{regionId}_{waypointName}";
            if (_discoveredLocations.Contains(locationKey)) {
                return;
            }
            
            _discoveredLocations.Add(locationKey);
            
            var bookmarkSystem = BookmarkSystem.Instance;
            if (bookmarkSystem != null) {
                bool added = bookmarkSystem.AddBookmark(
                    $"✨ {waypointName}",
                    BookmarkType.FastTravel,
                    BookmarkCategory.Waypoint,
                    position,
                    regionId,
                    $"Fast travel point - {waypointName}"
                );
                
                if (added) {
                    OnAutoBookmarkAdded?.Invoke(
                        bookmarkSystem.PlayerBookmarks[bookmarkSystem.PlayerBookmarks.Count - 1],
                        "waypoint_discovered"
                    );
                    GD.Print($"[AutoBookmarkSystem] Added waypoint bookmark: {waypointName}");
                }
            }
        }
        
        /// <summary>
        /// Add bookmark for region entrance
        /// </summary>
        public void AddRegionBookmark(string regionName, Vector2 position, int regionId) {
            string locationKey = $"region_{regionId}_{regionName}";
            if (_discoveredLocations.Contains(locationKey)) {
                return;
            }
            
            _discoveredLocations.Add(locationKey);
            
            var bookmarkSystem = BookmarkSystem.Instance;
            if (bookmarkSystem != null) {
                bool added = bookmarkSystem.AddBookmark(
                    $"🗺️ {regionName}",
                    BookmarkType.Auto,
                    BookmarkCategory.Region,
                    position,
                    regionId,
                    $"Region entrance - {regionName}"
                );
                
                if (added) {
                    GD.Print($"[AutoBookmarkSystem] Added region bookmark: {regionName}");
                }
            }
        }
        
        /// <summary>
        /// Clear discovered locations (for new game)
        /// </summary>
        public void ClearDiscoveredLocations() {
            _discoveredLocations.Clear();
            GD.Print("[AutoBookmarkSystem] Cleared discovered locations");
        }
        
        /// <summary>
        /// Serialize auto bookmark data
        /// </summary>
        public Dictionary<string, object> Serialize() {
            var data = new Dictionary<string, object>();
            data["discovered_locations"] = new List<string>(_discoveredLocations);
            data["auto_bookmark_boss"] = AutoBookmarkBoss;
            data["auto_bookmark_shop"] = AutoBookmarkShop;
            data["auto_bookmark_quest"] = AutoBookmarkQuest;
            data["auto_bookmark_waypoint"] = AutoBookmarkWaypoint;
            return data;
        }
        
        /// <summary>
        /// Deserialize auto bookmark data
        /// </summary>
        public void Deserialize(Dictionary<string, object> data) {
            if (data == null) return;
            
            _discoveredLocations.Clear();
            
            if (data.ContainsKey("discovered_locations") && data["discovered_locations"] is List<object> locations) {
                foreach (var loc in locations) {
                    _discoveredLocations.Add(loc.ToString());
                }
            }
            
            if (data.ContainsKey("auto_bookmark_boss")) {
                AutoBookmarkBoss = Convert.ToBoolean(data["auto_bookmark_boss"]);
            }
            if (data.ContainsKey("auto_bookmark_shop")) {
                AutoBookmarkShop = Convert.ToBoolean(data["auto_bookmark_shop"]);
            }
            if (data.ContainsKey("auto_bookmark_quest")) {
                AutoBookmarkQuest = Convert.ToBoolean(data["auto_bookmark_quest"]);
            }
            if (data.ContainsKey("auto_bookmark_waypoint")) {
                AutoBookmarkWaypoint = Convert.ToBoolean(data["auto_bookmark_waypoint"]);
            }
            
            GD.Print($"[AutoBookmarkSystem] Loaded {_discoveredLocations.Count} discovered locations");
        }
    }
}
