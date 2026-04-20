using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Bookmark UI - allows players to view, add, and manage bookmarks
    /// </summary>
    public partial class BookmarkUI : Control {
        [Export] private Color _panelColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        [Export] private Color _bookmarkPlayerColor = new Color(0.2f, 0.6f, 1.0f);
        [Export] private Color _bookmarkBossColor = new Color(1.0f, 0.3f, 0.3f);
        [Export] private Color _bookmarkShopColor = new Color(0.3f, 0.8f, 0.4f);
        [Export] private Color _bookmarkQuestColor = new Color(1.0f, 0.8f, 0.2f);
        [Export] private Color _bookmarkWaypointColor = new Color(0.6f, 0.4f, 1.0f);
        
        private VBoxContainer _mainContainer;
        private HBoxContainer _headerContainer;
        private Label _titleLabel;
        private Button _closeButton;
        private HBoxContainer _filterContainer;
        private Button _allFilter;
        private Button _playerFilter;
        private Button _bossFilter;
        private Button _shopFilter;
        private Button _questFilter;
        private Button _waypointFilter;
        private ScrollContainer _bookmarkList;
        private VBoxContainer _bookmarkItemsContainer;
        private HBoxContainer _addContainer;
        private LineEdit _nameInput;
        private Button _addButton;
        private Label _countLabel;
        
        private bool _isVisible = false; 
        private BookmarkCategory? _currentFilter = null;
        
        // Signal for when player wants to travel to a bookmark
        [Signal] public delegate void OnTravelToBookmarkEventHandlerEventHandler(string bookmarkId);
        
        public override void _Ready() {
            SetupUI();
            SetupInput();
            
            // Subscribe to bookmark events
            if (BookmarkSystem.Instance != null) {
                BookmarkSystem.Instance.OnBookmarkAdded += OnBookmarkAdded;
                BookmarkSystem.Instance.OnBookmarkRemoved += OnBookmarkRemoved;
            }
            
            Hide();
        }
        
        private void SetupUI() {
            // Main panel
            var panel = new Panel {
                AnchorsPreset = Control.LayoutPreset.Center,
                Size = new Vector2(600, 500),
                Position = new Vector2(340, 110)
            };
            panel.Modulate = _panelColor;
            AddChild(panel);
            
            _mainContainer = new VBoxContainer {
                AnchorsPreset = Control.LayoutPreset.FullRect,
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = -10,
                OffsetBottom = -10
            };
            panel.AddChild(_mainContainer);
            
            // Header
            _headerContainer = new HBoxContainer {
                Alignment = BoxContainer.AlignmentMode.Center
            };
            _mainContainer.AddChild(_headerContainer);
            
            _titleLabel = new Label {
                Text = "⭐ 收藏点",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _headerContainer.AddChild(_titleLabel);
            
            _headerContainer.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });
            
            _closeButton = new Button {
                Text = "✕",
                CustomMinimumSize = new Vector2(30, 30)
            };
            _closeButton.Pressed += () => ToggleVisibility();
            _headerContainer.AddChild(_closeButton);
            
            // Filter buttons
            _filterContainer = new HBoxContainer {
                Alignment = BoxContainer.AlignmentMode.Center
            };
            _mainContainer.AddChild(_filterContainer);
            
            _allFilter = CreateFilterButton("全部", null);
            _playerFilter = CreateFilterButton("玩家", BookmarkCategory.Player);
            _bossFilter = CreateFilterButton("Boss", BookmarkCategory.Boss);
            _shopFilter = CreateFilterButton("商店", BookmarkCategory.Shop);
            _questFilter = CreateFilterButton("任务", BookmarkCategory.Quest);
            _waypointFilter = CreateFilterButton("传送点", BookmarkCategory.Waypoint);
            
            // Bookmark list
            _bookmarkList = new ScrollContainer {
                SizeFlagsVertical = Control.SizeFlags.Expand | Control.SizeFlags.Fill
            };
            _mainContainer.AddChild(_bookmarkList);
            
            _bookmarkItemsContainer = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.Fill
            };
            _bookmarkList.AddChild(_bookmarkItemsContainer);
            
            // Add new bookmark section
            _addContainer = new HBoxContainer {
                Alignment = BoxContainer.AlignmentMode.Center
            };
            _mainContainer.AddChild(_addContainer);
            
            var nameLabel = new Label { Text = "名称:" };
            _addContainer.AddChild(nameLabel);
            
            _nameInput = new LineEdit {
                PlaceholderText = "输入收藏点名称...",
                CustomMinimumSize = new Vector2(200, 30)
            };
            _addContainer.AddChild(_nameInput);
            
            _addButton = new Button {
                Text = "+ 添加当前位置",
                CustomMinimumSize = new Vector2(150, 30)
            };
            _addButton.Pressed += OnAddBookmarkPressed;
            _addContainer.AddChild(_addButton);
            
            // Count label
            _countLabel = new Label {
                Text = "0 / 50 收藏点"
            };
            _countLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _mainContainer.AddChild(_countLabel);
            
            RefreshBookmarkList();
        }
        
        private Button CreateFilterButton(string text, BookmarkCategory? category) {
            var button = new Button {
                Text = text,
                ToggleMode = true,
                ButtonPressed = category == null
            };
            button.Pressed += () => OnFilterPressed(category);
            _filterContainer.AddChild(button);
            return button;
        }
        
        private void SetupInput() {
            // Toggle with N key
        }
        
        public override void _Input(InputEvent evt) {
            if (evt is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.N) {
                ToggleVisibility();
            }
        }
        
        public void ToggleVisibility() {
            if (_isVisible) {
                Hide();
                _isVisible = false; 
            } else {
                Show();
                _isVisible = true;
                RefreshBookmarkList();
            }
        }
        
        private void OnFilterPressed(BookmarkCategory? category) {
            _currentFilter = category;
            
            // Update button states
            _allFilter.ButtonPressed = category == null;
            _playerFilter.ButtonPressed = category == BookmarkCategory.Player;
            _bossFilter.ButtonPressed = category == BookmarkCategory.Boss;
            _shopFilter.ButtonPressed = category == BookmarkCategory.Shop;
            _questFilter.ButtonPressed = category == BookmarkCategory.Quest;
            _waypointFilter.ButtonPressed = category == BookmarkCategory.Waypoint;
            
            RefreshBookmarkList();
        }
        
        private void RefreshBookmarkList() {
            // Clear existing items
            foreach (Node child in _bookmarkItemsContainer.GetChildren()) {
                child.QueueFree();
            }
            
            var bookmarks = BookmarkSystem.Instance.GetAllBookmarks();
            
            // Apply filter
            if (_currentFilter.HasValue) {
                bookmarks = bookmarks.FindAll(b => b.Category == _currentFilter.Value);
            }
            
            // Update count
            var allBookmarks = BookmarkSystem.Instance.GetAllBookmarks();
            _countLabel.Text = $"{allBookmarks.Count} / 50 收藏点";
            
            // Sort by creation date (newest first)
            bookmarks.Sort((a, b) => b.CreatedAt.CompareTo(a.CreatedAt));
            
            // Add bookmark items
            foreach (var bookmark in bookmarks) {
                var item = CreateBookmarkItem(bookmark);
                _bookmarkItemsContainer.AddChild(item);
            }
            
            if (bookmarks.Count == 0) {
                var emptyLabel = new Label {
                    Text = _currentFilter.HasValue ? "没有收藏点" : "按 N 打开收藏界面，点击下方按钮添加当前位置",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                _bookmarkItemsContainer.AddChild(emptyLabel);
            }
        }
        
        private Control CreateBookmarkItem(Bookmark bookmark) {
            var container = new HBoxContainer {
                CustomMinimumSize = new Vector2(0, 50)
            };
            
            // Color indicator based on category
            var colorRect = new ColorRect {
                Color = GetCategoryColor(bookmark.Category),
                CustomMinimumSize = new Vector2(8, 40)
            };
            container.AddChild(colorRect);
            
            // Bookmark info
            var infoContainer = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            container.AddChild(infoContainer);
            
            var nameLabel = new Label {
                Text = $"⭐ {bookmark.Name}",
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            infoContainer.AddChild(nameLabel);
            
            var detailsLabel = new Label {
                Text = $"{GetCategoryText(bookmark.Category)} • {bookmark.CreatedAt:yyyy-MM-dd HH:mm}",
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            detailsLabel.AddThemeFontSizeOverride("font_size", 12);
            infoContainer.AddChild(detailsLabel);
            
            if (!string.IsNullOrEmpty(bookmark.Note)) {
                var noteLabel = new Label {
                    Text = $"📝 {bookmark.Note}",
                    SizeFlagsHorizontal = Control.SizeFlags.Expand
                };
                noteLabel.AddThemeFontSizeOverride("font_size", 11);
                infoContainer.AddChild(noteLabel);
            }
            
            // Travel button
            var travelButton = new Button {
                Text = "传送",
                CustomMinimumSize = new Vector2(60, 30)
            };
            travelButton.Pressed += () => OnTravelToBookmarkEventHandlerEventHandlerPressed(bookmark.Id.ToString());
            container.AddChild(travelButton);
            
            // Delete button
            var deleteButton = new Button {
                Text = "✕",
                CustomMinimumSize = new Vector2(30, 30)
            };
            deleteButton.Pressed += () => OnDeleteBookmarkPressed(bookmark);
            container.AddChild(deleteButton);
            
            return container;
        }
        
        private Color GetCategoryColor(BookmarkCategory category) {
            return category switch {
                BookmarkCategory.Player => _bookmarkPlayerColor,
                BookmarkCategory.Boss => _bookmarkBossColor,
                BookmarkCategory.Shop => _bookmarkShopColor,
                BookmarkCategory.Quest => _bookmarkQuestColor,
                BookmarkCategory.Waypoint => _bookmarkWaypointColor,
                _ => Colors.Gray
            };
        }
        
        private string GetCategoryText(BookmarkCategory category) {
            return category switch {
                BookmarkCategory.Player => "玩家标记",
                BookmarkCategory.Boss => "Boss位置",
                BookmarkCategory.Shop => "商店/NPC",
                BookmarkCategory.Quest => "任务目标",
                BookmarkCategory.Region => "区域入口",
                BookmarkCategory.Danger => "危险区域",
                BookmarkCategory.Treasure => "宝藏位置",
                BookmarkCategory.Waypoint => "传送点",
                _ => "其他"
            };
        }
        
        private void OnAddBookmarkPressed() {
            var name = _nameInput.Text.Trim();
            if (string.IsNullOrEmpty(name)) {
                GD.Warn("[BookmarkUI] Please enter a name for the bookmark");
                return;
            }
            
            // Get player's current position and region
            var player = GetTree().GetFirstNodeInGroup("Player") as CharacterBody2D;
            if (player == null) {
                GD.Warn("[BookmarkUI] Cannot find player node");
                return;
            }
            
            int regionId = 0;
            if (RegionManager.Instance != null) {
                regionId = RegionManager.Instance.CurrentRegionId;
            }
            
            // Add bookmark at player's current position
            bool success = BookmarkSystem.Instance.AddBookmark(
                name,
                BookmarkType.Custom,
                BookmarkCategory.Player,
                player.GlobalPosition,
                regionId
            );
            
            if (success) {
                _nameInput.Text = "";
                RefreshBookmarkList();
            }
        }
        
        private void OnTravelToBookmarkEventHandlerEventHandlerPressed(string bookmarkId) {
            GD.Print($"[BookmarkUI] Traveling to bookmark: {bookmarkId}");
            OnTravelToBookmarkEventHandlerEventHandler?.Invoke(bookmarkId);
            
            // Hide UI after traveling
            if (_isVisible) {
                ToggleVisibility();
            }
        }
        
        private void OnDeleteBookmarkPressed(Bookmark bookmark) {
            BookmarkSystem.Instance.RemoveBookmark(bookmark.Id);
            RefreshBookmarkList();
        }
        
        private void OnBookmarkAdded(Bookmark bookmark) {
            if (_isVisible) {
                RefreshBookmarkList();
            }
        }
        
        private void OnBookmarkRemoved(int bookmarkId) {
            if (_isVisible) {
                RefreshBookmarkList();
            }
        }
        
        public override void _ExitTree() {
            if (BookmarkSystem.Instance != null) {
                BookmarkSystem.Instance.OnBookmarkAdded -= OnBookmarkAdded;
                BookmarkSystem.Instance.OnBookmarkRemoved -= OnBookmarkRemoved;
            }
        }
    }
}
