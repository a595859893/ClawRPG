using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// World Boss System UI - Display and control world boss events
    /// </summary>
    [GlobalClass]
    public partial class WorldBossUI : Control {
        
        private Control container;
        private VBoxContainer mainVBox;
        private TabContainer tabContainer;
        
        // References
        private WorldBossSystem worldBossSystem;
        
        // UI Elements
        private Label titleLabel;
        private Label statusLabel;
        private Control activeBossContainer;
        private Control bossListContainer;
        private Control statsContainer;
        
        // State
        private bool isVisible = false;
        
        public override void _Ready() {
            SetupUI();
            SetupInput();
            
            worldBossSystem = WorldBossSystem.Instance;
            if (worldBossSystem == null) {
                GD.Print("[WorldBossUI] Warning: WorldBossSystem not found");
            }
            
            Visible = false;
        }
        
        private void SetupUI() {
            // Main container
            container = new Control {
                Name = "WorldBossUI",
                AnchorRight = 1f,
                AnchorBottom = 1f,
                MouseFilter = Control.MouseFilterEnum.Stop
            };
            AddChild(container);
            
            // Background panel
            var bgPanel = new Panel {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                Modulate = new Color(0, 0, 0, 0.7f)
            };
            container.AddChild(bgPanel);
            
            // Main panel
            var mainPanel = new Panel {
                AnchorLeft = 0.5f,
                AnchorTop = 0.5f,
                AnchorRight = 0.5f,
                AnchorBottom = 0.5f,
                OffsetLeft = -400,
                OffsetTop = -300,
                OffsetRight = 400,
                OffsetBottom = 300,
                GrowHorizontal = Control.GrowDirection.Center,
                GrowVertical = Control.GrowDirection.Center
            };
            container.AddChild(mainPanel);
            
            // Style
            var style = new StyleBoxFlat {
                BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f),
                BorderColor = new Color(0.3f, 0.3f, 0.4f),
                BorderWidthBottom = 2,
                BorderWidthLeft = 2,
                BorderWidthRight = 2,
                BorderWidthTop = 2,
                CornerRadiusTopLeft = 8,
                CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8,
                CornerRadiusBottomRight = 8
            };
            mainPanel.AddThemeStyleboxOverride("panel", style);
            
            // Main VBox
            mainVBox = new VBoxContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = -10,
                OffsetBottom = -10
            };
            mainPanel.AddChild(mainVBox);
            
            // Title
            titleLabel = new Label {
                Text = "🌍 World Boss Events",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0.2f));
            mainVBox.AddChild(titleLabel);
            
            // Status
            statusLabel = new Label {
                Text = "No active world bosses",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            statusLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            mainVBox.AddChild(statusLabel);
            
            // Tab container
            tabContainer = new TabContainer {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                TabAlign = TabsAlignEnum.Left
            };
            mainVBox.AddChild(tabContainer);
            
            // Active Bosses tab
            var activeTab = new Control {
                Name = "Active Bosses"
            };
            tabContainer.AddChild(activeTab);
            SetupActiveBossTab(activeTab);
            
            // Boss List tab
            var listTab = new Control {
                Name = "All Bosses"
            };
            tabContainer.AddChild(listTab);
            SetupBossListTab(listTab);
            
            // Statistics tab
            var statsTab = new Control {
                Name = "Statistics"
            };
            tabContainer.AddChild(statsTab);
            SetupStatsTab(statsTab);
            
            // Close button
            var closeButton = new Button {
                Text = "Close (ESC)",
                CustomMinimumSize = new Vector2(0, 40)
            };
            closeButton.Pressed += () => ToggleVisibility();
            mainVBox.AddChild(closeButton);
        }
        
        private void SetupActiveBossTab(Control parent) {
            var scroll = new ScrollContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 5,
                OffsetTop = 5,
                OffsetRight = -5,
                OffsetBottom = -5
            };
            parent.AddChild(scroll);
            
            activeBossContainer = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            scroll.AddChild(activeBossContainer);
        }
        
        private void SetupBossListTab(Control parent) {
            var scroll = new ScrollContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 5,
                OffsetTop = 5,
                OffsetRight = -5,
                OffsetBottom = -5
            };
            parent.AddChild(scroll);
            
            bossListContainer = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            scroll.AddChild(bossListContainer);
        }
        
        private void SetupStatsTab(Control parent) {
            var scroll = new ScrollContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 5,
                OffsetTop = 5,
                OffsetRight = -5,
                OffsetBottom = -5
            };
            parent.AddChild(scroll);
            
            statsContainer = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            scroll.AddChild(statsContainer);
        }
        
        private void SetupInput() {
            // Input action will be handled by project.godot
        }
        
        public override void _Input(InputEvent e) {
            if (e is InputEventKey keyEvent && keyEvent.Pressed) {
                if (keyEvent.Keycode == Key.W) {
                    ToggleVisibility();
                }
            }
        }
        
        public void ToggleVisibility() {
            isVisible = !isVisible;
            Visible = isVisible;
            
            if (isVisible) {
                RefreshUI();
            }
        }
        
        private void RefreshUI() {
            if (worldBossSystem == null) {
                statusLabel.Text = "World Boss System not initialized";
                return;
            }
            
            RefreshActiveBosses();
            RefreshBossList();
            RefreshStatistics();
        }
        
        private void RefreshActiveBosses() {
            // Clear existing
            foreach (var child in activeBossContainer.GetChildren()) {
                child.QueueFree();
            }
            
            var activeBosses = worldBossSystem.GetActiveBosses();
            
            if (activeBosses.Count == 0) {
                var emptyLabel = new Label {
                    Text = "No active world bosses",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                emptyLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
                activeBossContainer.AddChild(emptyLabel);
                statusLabel.Text = "No active world bosses";
                return;
            }
            
            statusLabel.Text = $"{activeBosses.Count} active world boss(es)";
            
            foreach (var kvp in activeBosses) {
                var boss = kvp.Value;
                var bossCard = CreateBossCard(boss);
                activeBossContainer.AddChild(bossCard);
            }
        }
        
        private void RefreshBossList() {
            // Clear existing
            foreach (var child in bossListContainer.GetChildren()) {
                child.QueueFree();
            }
            
            var allBosses = worldBossSystem.GetAllBosses();
            
            foreach (var kvp in allBosses) {
                var boss = kvp.Value;
                var bossCard = CreateBossListCard(boss);
                bossListContainer.AddChild(bossCard);
            }
        }
        
        private void RefreshStatistics() {
            // Clear existing
            foreach (var child in statsContainer.GetChildren()) {
                child.QueueFree();
            }
            
            var stats = worldBossSystem.GetStatistics();
            
            // Stats grid
            var statsGrid = new GridContainer {
                Columns = 2
            };
            statsContainer.AddChild(statsGrid);
            
            AddStatRow(statsGrid, "Total World Boss Events", stats.TotalEvents.ToString());
            AddStatRow(statsGrid, "Successful Defeats", stats.SuccessfulDefeats.ToString());
            AddStatRow(statsGrid, "Total Damage Dealt", stats.TotalDamageDealt.ToString("N0"));
            AddStatRow(statsGrid, "Total Participants", stats.TotalPlayers参与.ToString());
            AddStatRow(statsGrid, "Success Rate", $"{stats.SuccessRate:P1}");
            
            // Boss history
            if (stats.BossHistory.Count > 0) {
                var historyLabel = new Label {
                    Text = "Recent Defeats:",
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                historyLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.2f));
                historyLabel.AddThemeFontSizeOverride("font_size", 16);
                statsContainer.AddChild(historyLabel);
                
                foreach (var entry in stats.BossHistory) {
                    var entryLabel = new Label {
                        Text = "• " + entry,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    entryLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
                    statsContainer.AddChild(entryLabel);
                }
            }
        }
        
        private Control CreateBossCard(WorldBossInstance boss) {
            var card = new Panel {
                CustomMinimumSize = new Vector2(0, 120)
            };
            
            var cardStyle = new StyleBoxFlat {
                BgColor = new Color(0.2f, 0.15f, 0.1f),
                BorderColor = new Color(0.8f, 0.4f, 0f),
                BorderWidthLeft = 3,
                BorderWidthRight = 3,
                BorderWidthTop = 3,
                BorderWidthBottom = 3,
                CornerRadiusTopLeft = 6,
                CornerRadiusTopRight = 6,
                CornerRadiusBottomLeft = 6,
                CornerRadiusBottomRight = 6
            };
            card.AddThemeStyleboxOverride("panel", cardStyle);
            
            var vbox = new VBoxContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 10,
                OffsetTop = 8,
                OffsetRight = -10,
                OffsetBottom = -8
            };
            card.AddChild(vbox);
            
            // Boss name and type
            var nameLabel = new Label {
                Text = $"⚔️ {boss.Data.Name} (Phase {boss.Phase})",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 18);
            nameLabel.AddThemeColorOverride("font_color", new Color(1f, 0.7f, 0.2f));
            vbox.AddChild(nameLabel);
            
            // Type and difficulty
            var infoLabel = new Label {
                Text = $"Type: {boss.Data.Type} | Difficulty: {boss.Data.Difficulty}/5 | Element: {boss.Data.Element}",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            infoLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            vbox.AddChild(infoLabel);
            
            // Health bar
            var healthContainer = new HBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            vbox.AddChild(healthContainer);
            
            var healthLabel = new Label {
                Text = "HP: "
            };
            healthLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.3f, 0.3f));
            healthContainer.AddChild(healthLabel);
            
            var healthBar = new ProgressBar {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                Value = (float)boss.CurrentHealth / boss.MaxHealth * 100,
                MaxValue = 100
            };
            healthBar.AddThemeStyleboxOverride("background", new StyleBoxFlat { BgColor = new Color(0.3f, 0.1f, 0.1f) });
            var fillStyle = new StyleBoxFlat { BgColor = new Color(0.9f, 0.2f, 0.2f) };
            healthBar.AddThemeStyleboxOverride("fill", fillStyle);
            healthContainer.AddChild(healthBar);
            
            var healthValueLabel = new Label {
                Text = $"{boss.CurrentHealth:N0}/{boss.MaxHealth:N0}"
            };
            healthValueLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.3f, 0.3f));
            healthContainer.AddChild(healthValueLabel);
            
            // Participants
            var participantLabel = new Label {
                Text = $"Participants: {boss.DamageContributors.Count} | Time: {(DateTime.Now - boss.SpawnTime).TotalMinutes:F1} min",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            participantLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            vbox.AddChild(participantLabel);
            
            return card;
        }
        
        private Control CreateBossListCard(WorldBossData boss) {
            var card = new Panel {
                CustomMinimumSize = new Vector2(0, 80)
            };
            
            var cardStyle = new StyleBoxFlat {
                BgColor = new Color(0.15f, 0.15f, 0.2f),
                BorderColor = GetDifficultyColor(boss.Difficulty),
                BorderWidthLeft = 2,
                CornerRadiusTopLeft = 4,
                CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4,
                CornerRadiusBottomRight = 4
            };
            card.AddThemeStyleboxOverride("panel", cardStyle);
            
            var hbox = new HBoxContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 10,
                OffsetTop = 8,
                OffsetRight = -10,
                OffsetBottom = -8
            };
            card.AddChild(hbox);
            
            // Boss icon
            var iconLabel = new Label {
                Text = GetBossEmoji(boss.Type),
                CustomMinimumSize = new Vector2(40, 0)
            };
            iconLabel.AddThemeFontSizeOverride("font_size", 24);
            hbox.AddChild(iconLabel);
            
            // Boss info
            var infoVBox = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            hbox.AddChild(infoVBox);
            
            var nameLabel = new Label {
                Text = boss.Name,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            nameLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.5f));
            infoVBox.AddChild(nameLabel);
            
            var descLabel = new Label {
                Text = boss.Description,
                HorizontalAlignment = HorizontalAlignment.Left,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            descLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            infoVBox.AddChild(descLabel);
            
            // Difficulty
            var diffLabel = new Label {
                Text = $"⭐{boss.Difficulty}/5",
                CustomMinimumSize = new Vector2(60, 0)
            };
            diffLabel.AddThemeFontSizeOverride("font_size", 14);
            diffLabel.AddThemeColorOverride("font_color", GetDifficultyColor(boss.Difficulty));
            hbox.AddChild(diffLabel);
            
            return card;
        }
        
        private void AddStatRow(GridContainer grid, string label, string value) {
            var labelNode = new Label {
                Text = label + ":",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            labelNode.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            grid.AddChild(labelNode);
            
            var valueNode = new Label {
                Text = value,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            valueNode.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.5f));
            grid.AddChild(valueNode);
        }
        
        private Color GetDifficultyColor(int difficulty) {
            return difficulty switch {
                1 => new Color(0.5f, 0.5f, 0.5f),
                2 => new Color(0.3f, 0.7f, 0.3f),
                3 => new Color(0.3f, 0.5f, 0.9f),
                4 => new Color(0.7f, 0.4f, 0.8f),
                5 => new Color(1f, 0.6f, 0f),
                _ => new Color(0.5f, 0.5f, 0.5f)
            };
        }
        
        private string GetBossEmoji(WorldBossType type) {
            return type switch {
                WorldBossType.Elite => "🐉",
                WorldBossType.Cosmic => "🌌",
                WorldBossType.Divine => "✨",
                WorldBossType.Corrupted => "☠️",
                WorldBossType.Construct => "🗿",
                WorldBossType.Assassin => "🗡️",
                _ => "👹"
            };
        }
        
        public bool IsUIVisible() => isVisible;
    }
}
