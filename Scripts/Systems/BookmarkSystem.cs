using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Bookmark data class - represents a saved location marker
    /// </summary>
    public class Bookmark {
        public int Id { get; set; }
        public string Name { get; set; }
        public Vector2 Position { get; set; }
        public int RegionId { get; set; }
        public BookmarkType Type { get; set; }
        public BookmarkCategory Category { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Note { get; set; }
        
        public Bookmark() {
            CreatedAt = DateTime.Now;
            Note = "";
        }
        
        public Bookmark(int id, string name, Vector2 position, int regionId, BookmarkType type, BookmarkCategory category) {
            Id = id;
            Name = name;
            Position = position;
            RegionId = regionId;
            Type = type;
            Category = category;
            CreatedAt = DateTime.Now;
            Note = "";
        }
    }
    
    public enum BookmarkType {
        Custom,      // Player created
        Auto,        // System auto-added (boss, shop, etc.)
        Quest,       // Quest related
        FastTravel   // Fast travel point
    }
    
    public enum BookmarkCategory {
        Player,      // Player marked
        Boss,        // Boss location
        Shop,        // Shop/NPC
        Quest,       // Quest objective
        Region,      // Region entrance
        Danger,     // Dangerous area
        Treasure,   // Treasure location
        Waypoint    // Fast travel waypoint
    }
    
    /// <summary>
    /// Bookmark database - stores bookmark templates
    /// </summary>
    public class BookmarkDatabase {
        private static BookmarkDatabase _instance;
        public static BookmarkDatabase Instance => _instance ??= new BookmarkDatabase();
        
        // Auto-generated bookmarks based on game content
        private List<Bookmark> _autoBookmarks = new();
        
        private BookmarkDatabase() {
            InitializeAutoBookmarks();
        }
        
        private void InitializeAutoBookmarks() {
            // These would be populated based on game content
            // For now, we'll generate them dynamically based on regions
        }
        
        public void AddAutoBookmark(Bookmark bookmark) {
            _autoBookmarks.Add(bookmark);
        }
        
        public List<Bookmark> GetAutoBookmarks() {
            return new List<Bookmark>(_autoBookmarks);
        }
        
        public List<Bookmark> GetBookmarksByCategory(BookmarkCategory category) {
            return _autoBookmarks.FindAll(b => b.Category == category);
        }
    }
    
    /// <summary>
    /// Bookmark manager - handles bookmark creation, storage, and retrieval
    /// </summary>
    public partial class BookmarkSystem : Node {
        public static BookmarkSystem Instance { get; private set; }
        
        // Signals for bookmark events
        [Signal] public signal void OnBookmarkAdded(Bookmark bookmark);
        [Signal] public signal void OnBookmarkRemoved(int bookmarkId);
        [Signal] public signal void OnBookmarkUpdated(Bookmark bookmark);
        
        // Player's bookmarks
        public List<Bookmark> PlayerBookmarks { get; private set; } = new();
        
        // Next available bookmark ID
        private int _nextBookmarkId = 1;
        
        // Maximum bookmarks
        private const int MaxBookmarks = 50;
        
        public override void _Ready() {
            Instance = this;
        }
        
        /// <summary>
        /// Add a new bookmark at player's current position
        /// </summary>
        public bool AddBookmark(string name, BookmarkType type, BookmarkCategory category, Vector2 position, int regionId, string note = "") {
            if (PlayerBookmarks.Count >= MaxBookmarks) {
                GD.Warn($"[BookmarkSystem] Maximum bookmarks reached ({MaxBookmarks})");
                return false;
            }
            
            var bookmark = new Bookmark(_nextBookmarkId++, name, position, regionId, type, category) {
                Note = note
            };
            
            PlayerBookmarks.Add(bookmark);
            OnBookmarkAdded?.Invoke(bookmark);
            
            GD.Print($"[BookmarkSystem] Added bookmark: {name} at {position}");
            return true;
        }
        
        /// <summary>
        /// Remove a bookmark by ID
        /// </summary>
        public bool RemoveBookmark(int bookmarkId) {
            var bookmark = PlayerBookmarks.Find(b => b.Id == bookmarkId);
            if (bookmark != null) {
                PlayerBookmarks.Remove(bookmark);
                OnBookmarkRemoved?.Invoke(bookmarkId);
                GD.Print($"[BookmarkSystem] Removed bookmark: {bookmark.Name}");
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Update bookmark details
        /// </summary>
        public bool UpdateBookmark(int bookmarkId, string newName = null, string newNote = null) {
            var bookmark = PlayerBookmarks.Find(b => b.Id == bookmarkId);
            if (bookmark != null) {
                if (!string.IsNullOrEmpty(newName)) bookmark.Name = newName;
                if (newNote != null) bookmark.Note = newNote;
                OnBookmarkUpdated?.Invoke(bookmark);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Get all bookmarks
        /// </summary>
        public List<Bookmark> GetAllBookmarks() {
            return new List<Bookmark>(PlayerBookmarks);
        }
        
        /// <summary>
        /// Get bookmarks for a specific region
        /// </summary>
        public List<Bookmark> GetBookmarksInRegion(int regionId) {
            return PlayerBookmarks.FindAll(b => b.RegionId == regionId);
        }
        
        /// <summary>
        /// Get bookmarks by category
        /// </summary>
        public List<Bookmark> GetBookmarksByCategory(BookmarkCategory category) {
            return PlayerBookmarks.FindAll(b => b.Category == category);
        }
        
        /// <summary>
        /// Find nearest bookmark to a position
        /// </summary>
        public Bookmark FindNearestBookmark(Vector2 position, float maxDistance = 200f) {
            Bookmark nearest = null;
            float nearestDist = maxDistance;
            
            foreach (var bookmark in PlayerBookmarks) {
                float dist = position.DistanceTo(bookmark.Position);
                if (dist < nearestDist) {
                    nearestDist = dist;
                    nearest = bookmark;
                }
            }
            
            return nearest;
        }
        
        /// <summary>
        /// Serialize bookmarks for saving
        /// </summary>
        public Dictionary<string, object> Serialize() {
            var data = new Dictionary<string, object>();
            var bookmarkList = new List<Dictionary<string, object>>();
            
            foreach (var bookmark in PlayerBookmarks) {
                bookmarkList.Add(new Dictionary<string, object> {
                    { "id", bookmark.Id },
                    { "name", bookmark.Name },
                    { "position_x", bookmark.Position.X },
                    { "position_y", bookmark.Position.Y },
                    { "region_id", bookmark.RegionId },
                    { "type", (int)bookmark.Type },
                    { "category", (int)bookmark.Category },
                    { "note", bookmark.Note },
                    { "created_at", bookmark.CreatedAt.ToString("o") }
                });
            }
            
            data["bookmarks"] = bookmarkList;
            data["next_id"] = _nextBookmarkId;
            
            return data;
        }
        
        /// <summary>
        /// Deserialize bookmarks from save
        /// </summary>
        public void Deserialize(Dictionary<string, object> data) {
            if (data == null) return;
            
            PlayerBookmarks.Clear();
            
            if (data.ContainsKey("bookmarks") && data["bookmarks"] is List<object> bookmarkList) {
                foreach (var obj in bookmarkList) {
                    if (obj is Dictionary<string, object> bookmarkData) {
                        var bookmark = new Bookmark(
                            Convert.ToInt32(bookmarkData["id"]),
                            bookmarkData["name"].ToString(),
                            new Vector2(
                                Convert.ToSingle(bookmarkData["position_x"]),
                                Convert.ToSingle(bookmarkData["position_y"])
                            ),
                            Convert.ToInt32(bookmarkData["region_id"]),
                            (BookmarkType)Convert.ToInt32(bookmarkData["type"]),
                            (BookmarkCategory)Convert.ToInt32(bookmarkData["category"])
                        );
                        
                        if (bookmarkData.ContainsKey("note")) {
                            bookmark.Note = bookmarkData["note"].ToString();
                        }
                        
                        if (bookmarkData.ContainsKey("created_at")) {
                            DateTime.TryParse(bookmarkData["created_at"].ToString(), out var createdAt);
                            bookmark.CreatedAt = createdAt;
                        }
                        
                        PlayerBookmarks.Add(bookmark);
                    }
                }
            }
            
            if (data.ContainsKey("next_id")) {
                _nextBookmarkId = Convert.ToInt32(data["next_id"]);
            }
            
            GD.Print($"[BookmarkSystem] Loaded {PlayerBookmarks.Count} bookmarks");
        }
    }
}
