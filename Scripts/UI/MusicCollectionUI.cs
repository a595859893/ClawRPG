using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Music Collection UI - Display and manage player's music collection
    /// </summary>
    public class MusicCollectionUI : Control {
        private static MusicCollectionUI _instance;
        public static MusicCollectionUI Instance => _instance;
        
        // UI Components
        private VBoxContainer _mainContainer;
        private HBoxContainer _categoryTabs;
        private GridContainer _trackGrid;
        private Label _titleLabel;
        private Label _statsLabel;
        private Label _progressLabel;
        
        // Category buttons
        private Button _btnAll;
        private Button _btnExploration;
        private Button _btnBattle;
        private Button _btnEvent;
        private Button _btnMenu;
        private Button _btnSpecial;
        
        // Current filter
        private MusicCategory? _currentCategory = null;
        private TrackRarity? _currentRarity = null;
        
        // Track item scene
        private PackedScene _trackItemScene;
        
        // Collection system reference
        private MusicCollectionSystem _collectionSystem;
        
        public override void _Ready() {
            _instance = this;
            _collectionSystem = GetNode<MusicCollectionSystem>("/root/MusicCollectionSystem");
            
            SetupUI();
            ConnectSignals();
            RefreshDisplay();
            
            GD.Print("MusicCollectionUI initialized");
        }
        
        private void SetupUI() {
            // Main container
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _mainContainer.AddThemeConstantOverride("separation", 10);
            AddChild(_mainContainer);
            
            // Title
            _titleLabel = new Label();
            _titleLabel.Text = "🎵 Music Collection";
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _mainContainer.AddChild(_titleLabel);
            
            // Progress label
            _progressLabel = new Label();
            _progressLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _mainContainer.AddChild(_progressLabel);
            
            // Category tabs
            _categoryTabs = new HBoxContainer();
            _categoryTabs.Alignment = BoxContainer.AlignmentMode.Center;
            _categoryTabs.AddThemeConstantOverride("separation", 5);
            _mainContainer.AddChild(_categoryTabs);
            
            _btnAll = CreateCategoryButton("All", null);
            _btnExploration = CreateCategoryButton("Exploration", MusicCategory.Exploration);
            _btnBattle = CreateCategoryButton("Battle", MusicCategory.Battle);
            _btnEvent = CreateCategoryButton("Event", MusicCategory.Event);
            _btnMenu = CreateCategoryButton("Menu", MusicCategory.Menu);
            _btnSpecial = CreateCategoryButton("Special", MusicCategory.Special);
            
            // Stats label
            _statsLabel = new Label();
            _statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _mainContainer.AddChild(_statsLabel);
            
            // Track grid
            _trackGrid = new GridContainer();
            _trackGrid.Columns = 4;
            _trackGrid.AddThemeConstantOverride("h_separation", 10);
            _trackGrid.AddThemeConstantOverride("v_separation", 10);
            _trackGrid.SizeFlagsVertical = Control.SizeFlags.ExpandAndFill;
            
            var scrollContainer = new ScrollContainer();
            scrollContainer.AddThemeConstantOverride("h_separation", 10);
            scrollContainer.AddThemeConstantOverride("v_separation", 10);
            scrollContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            scrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandAndFill;
            scrollContainer.AddChild(_trackGrid);
            _mainContainer.AddChild(scrollContainer);
            
            // Close button
            var closeButton = new Button();
            closeButton.Text = "Close (M)";
            closeButton.Alignment = HorizontalAlignment.Center;
            closeButton.Pressed += OnClosePressed;
            _mainContainer.AddChild(closeButton);
        }
        
        private Button CreateCategoryButton(string text, MusicCategory? category) {
            var button = new Button();
            button.Text = text;
            button.ToggleMode = true;
            button.Pressed += () => OnCategorySelected(category);
            _categoryTabs.AddChild(button);
            return button;
        }
        
        private void ConnectSignals() {
            if (_collectionSystem != null) {
                _collectionSystem.Connect(nameof(MusicCollectionSystem.TrackUnlocked), this, nameof(OnTrackUnlocked));
                _collectionSystem.Connect(nameof(MusicCollectionSystem.FavoriteAdded), this, nameof(OnFavoriteChanged));
                _collectionSystem.Connect(nameof(MusicCollectionSystem.FavoriteRemoved), this, nameof(OnFavoriteChanged));
            }
        }
        
        private void RefreshDisplay() {
            // Update progress
            var (unlocked, total) = _collectionSystem.GetCollectionProgress();
            _progressLabel.Text = $"Collection: {unlocked}/{total} tracks unlocked";
            
            // Update stats
            var mostPlayed = _collectionSystem.GetMostPlayedTrack();
            var stats = $"Total Play Time: {_collectionSystem.GetTotalPlayTime()}s | " +
                       $"Times Played: {_collectionSystem.GetPlayCount()} | " +
                       $"Favorites: {_collectionSystem.GetFavoriteCount()}";
            if (mostPlayed != null) {
                stats += $" | Most Played: {mostPlayed.Name}";
            }
            _statsLabel.Text = stats;
            
            // Update grid
            UpdateTrackGrid();
        }
        
        private void UpdateTrackGrid() {
            // Clear existing items
            foreach (Node child in _trackGrid.GetChildren()) {
                child.QueueFree();
            }
            
            // Get tracks to display
            List<MusicCollectionEntry> tracks;
            
            if (_currentCategory.HasValue) {
                tracks = _collectionSystem.GetTracksByCategory(_currentCategory.Value);
            } else {
                tracks = new List<MusicCollectionEntry>(_collectionSystem.GetUnlockedTracks().Values);
            }
            
            // Filter by rarity if set
            if (_currentRarity.HasValue) {
                tracks.RemoveAll(t => t.Rarity != _currentRarity.Value);
            }
            
            // Sort by category then name
            tracks.Sort((a, b) => {
                int catCompare = a.Category.CompareTo(b.Category);
                if (catCompare != 0) return catCompare;
                return a.Name.CompareTo(b.Name);
            });
            
            // Add track items
            foreach (var track in tracks) {
                var trackItem = CreateTrackItem(track);
                _trackGrid.AddChild(trackItem);
            }
        }
        
        private Control CreateTrackItem(MusicCollectionEntry track) {
            var container = new VBoxContainer();
            container.CustomMinimumSize = new Vector2(150, 100);
            
            // Name label
            var nameLabel = new Label();
            nameLabel.Text = track.Name;
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            nameLabel.AddThemeFontSizeOverride("font_size", 14);
            container.AddChild(nameLabel);
            
            // Category label
            var categoryLabel = new Label();
            categoryLabel.Text = track.Category.ToString();
            categoryLabel.HorizontalAlignment = HorizontalAlignment.Center;
            categoryLabel.AddThemeFontSizeOverride("font_size", 10);
            container.AddChild(categoryLabel);
            
            // Rarity label
            var rarityLabel = new Label();
            rarityLabel.Text = GetRarityIcon(track.Rarity) + " " + track.Rarity;
            rarityLabel.HorizontalAlignment = HorizontalAlignment.Center;
            rarityLabel.AddThemeFontSizeOverride("font_size", 10);
            container.AddChild(rarityLabel);
            
            // Favorite button
            var favButton = new Button();
            favButton.Text = _collectionSystem.IsFavorite(track.Id) ? "❤️" : "🤍";
            favButton.Pressed += () => OnFavoriteToggle(track.Id);
            container.AddChild(favButton);
            
            // Play count
            var playCountLabel = new Label();
            playCountLabel.Text = $"▶ {track.PlayCount}x";
            playCountLabel.HorizontalAlignment = HorizontalAlignment.Center;
            playCountLabel.AddThemeFontSizeOverride("font_size", 10);
            container.AddChild(playCountLabel);
            
            // Description
            var descLabel = new Label();
            descLabel.Text = track.Description;
            descLabel.HorizontalAlignment = HorizontalAlignment.Center;
            descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            descLabel.AddThemeFontSizeOverride("font_size", 9);
            container.AddChild(descLabel);
            
            return container;
        }
        
        private string GetRarityIcon(TrackRarity rarity) {
            switch (rarity) {
                case TrackRarity.Common: return "⚪";
                case TrackRarity.Uncommon: return "🟢";
                case TrackRarity.Rare: return "🔵";
                case TrackRarity.Epic: return "🟣";
                case TrackRarity.Legendary: return "🟡";
                default: return "⚪";
            }
        }
        
        #region Event Handlers
        
        private void OnCategorySelected(MusicCategory? category) {
            _currentCategory = category;
            
            // Update button states
            _btnAll.ButtonPressed = category == null;
            _btnExploration.ButtonPressed = category == MusicCategory.Exploration;
            _btnBattle.ButtonPressed = category == MusicCategory.Battle;
            _btnEvent.ButtonPressed = category == MusicCategory.Event;
            _btnMenu.ButtonPressed = category == MusicCategory.Menu;
            _btnSpecial.ButtonPressed = category == MusicCategory.Special;
            
            UpdateTrackGrid();
        }
        
        private void OnFavoriteToggle(string trackId) {
            if (_collectionSystem.IsFavorite(trackId)) {
                _collectionSystem.RemoveFavorite(trackId);
            } else {
                _collectionSystem.AddFavorite(trackId);
            }
        }
        
        private void OnTrackUnlocked(string trackId) {
            RefreshDisplay();
        }
        
        private void OnFavoriteChanged(string trackId) {
            RefreshDisplay();
        }
        
        private void OnClosePressed() {
            Hide();
        }
        
        #endregion
        
        public override void _Input(InputEvent evt) {
            if (evt is InputEventKey keyEvent && keyEvent.Pressed) {
                if (keyEvent.Keycode == Key.M || keyEvent.Keycode == Key.Escape) {
                    Hide();
                }
            }
        }
        
        public override void _ExitTree() {
            if (_instance == this) {
                _instance = null;
            }
        }
    }
}
