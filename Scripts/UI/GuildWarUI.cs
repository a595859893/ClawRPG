using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Core.Systems.GuildWar
{
    /// <summary>
    /// Guild War UI - Display and interaction
    /// </summary>
    public partial class GuildWarUI : Control
    {
        private GuildWarSystem _system;
        
        // UI Elements
        private TabContainer _tabContainer;
        private VBoxContainer _activeWarsTab;
        private VBoxContainer _territoryTab;
        private VBoxContainer _historyTab;
        private VBoxContainer _statisticsTab;
        
        // State
        private bool _isVisible = false;
        private string _selectedWarId = "";

        public override void _Ready()
        {
            _system = GuildWarSystem.Instance;
            SetupUI();
            ConnectSignals();
            Hide();
        }

        private void SetupUI()
        {
            // Main container
            var mainPanel = new Panel
            {
                Name = "MainPanel",
                Size = new Vector2(900, 600),
                Position = new Vector2(50, 50)
            };
            AddChild(mainPanel);

            // Title
            var title = new Label
            {
                Text = "⚔️ Guild War System",
                Position = new Vector2(20, 10),
                Size = new Vector2(860, 40)
            };
            title.AddThemeFontSizeOverride("font_size", 24);
            mainPanel.AddChild(title);

            // Tab container
            _tabContainer = new TabContainer
            {
                Position = new Vector2(20, 60),
                Size = new Vector2(860, 520)
            };
            mainPanel.AddChild(_tabContainer);

            // Create tabs
            _activeWarsTab = CreateActiveWarsTab();
            _territoryTab = CreateTerritoryTab();
            _historyTab = CreateHistoryTab();
            _statisticsTab = CreateStatisticsTab();

            _tabContainer.AddChild(_activeWarsTab);
            _tabContainer.AddChild(_territoryTab);
            _tabContainer.AddChild(_historyTab);
            _tabContainer.AddChild(_statisticsTab);

            _tabContainer.SetTabTitle(0, "Active Wars");
            _tabContainer.SetTabTitle(1, "Territories");
            _tabContainer.SetTabTitle(2, "History");
            _tabContainer.SetTabTitle(3, "Statistics");

            // Close button
            var closeButton = new Button
            {
                Text = "✕ Close",
                Position = new Vector2(780, 10),
                Size = new Vector2(100, 30)
            };
            closeButton.Pressed += () => ToggleUI();
            mainPanel.AddChild(closeButton);
        }

        private VBoxContainer CreateActiveWarsTab()
        {
            var container = new VBoxContainer();
            container.Name = "ActiveWars";
            container.AddThemeConstantOverride("separation", 10);

            // Header
            var header = new Label
            {
                Text = "⚔️ Active Guild Wars",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            header.AddThemeFontSizeOverride("font_size", 20);
            container.AddChild(header);

            // Scroll container for wars list
            var scroll = new ScrollContainer
            {
                Size = new Vector2(840, 450),
                VerticalScrollMode = ScrollContainer.ScrollModeEnabled
            };
            
            var warsList = new VBoxContainer();
            warsList.Name = "WarsList";
            warsList.AddThemeConstantOverride("separation", 5);
            scroll.AddChild(warsList);
            container.AddChild(scroll);

            return container;
        }

        private VBoxContainer CreateTerritoryTab()
        {
            var container = new VBoxContainer();
            container.Name = "Territory";
            container.AddThemeConstantOverride("separation", 10);

            // Header
            var header = new Label
            {
                Text = "🏰 Territory Control",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            header.AddThemeFontSizeOverride("font_size", 20);
            container.AddChild(header);

            // Territories list
            var scroll = new ScrollContainer
            {
                Size = new Vector2(840, 450),
                VerticalScrollMode = ScrollContainer.ScrollModeEnabled
            };
            
            var territoryList = new VBoxContainer();
            territoryList.Name = "TerritoryList";
            territoryList.AddThemeConstantOverride("separation", 5);
            scroll.AddChild(territoryList);
            container.AddChild(scroll);

            return container;
        }

        private VBoxContainer CreateHistoryTab()
        {
            var container = new VBoxContainer();
            container.Name = "History";
            container.AddThemeConstantOverride("separation", 10);

            // Header
            var header = new Label
            {
                Text = "📜 War History",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            header.AddThemeFontSizeOverride("font_size", 20);
            container.AddChild(header);

            // Scroll container
            var scroll = new ScrollContainer
            {
                Size = new Vector2(840, 450),
                VerticalScrollMode = ScrollContainer.ScrollModeEnabled
            };
            
            var historyList = new VBoxContainer();
            historyList.Name = "HistoryList";
            historyList.AddThemeConstantOverride("separation", 5);
            scroll.AddChild(historyList);
            container.AddChild(scroll);

            return container;
        }

        private VBoxContainer CreateStatisticsTab()
        {
            var container = new VBoxContainer();
            container.Name = "Statistics";
            container.AddThemeConstantOverride("separation", 10);

            // Header
            var header = new Label
            {
                Text = "📊 Guild Statistics",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            header.AddThemeFontSizeOverride("font_size", 20);
            container.AddChild(header);

            // Stats display
            var statsContainer = new VBoxContainer();
            statsContainer.Name = "StatsContainer";
            statsContainer.AddThemeConstantOverride("separation", 5);
            container.AddChild(statsContainer);

            // Note about guild stats
            var note = new Label
            {
                Text = "Enter your Guild ID to view statistics",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            container.AddChild(note);

            // Input for guild ID
            var inputBox = new LineEdit
            {
                PlaceholderText = "Enter Guild ID...",
                Size = new Vector2(400, 30)
            };
            container.AddChild(inputBox);

            var queryButton = new Button
            {
                Text = "Query Statistics"
            };
            queryButton.Pressed += () => QueryGuildStats(inputBox.Text);
            container.AddChild(queryButton);

            return container;
        }

        private void ConnectSignals()
        {
            if (_system != null)
            {
                _system.WarStarted += OnWarStarted;
                _system.WarEnded += OnWarEnded;
                _system.TerritoryCaptured += OnTerritoryCaptured;
            }
        }

        private void OnWarStarted(string warId, string warName)
        {
            RefreshActiveWars();
        }

        private void OnWarEnded(string warId, string winnerId, List<GuildWarParticipant> rankings)
        {
            RefreshActiveWars();
            RefreshHistory();
        }

        private void OnTerritoryCaptured(string territoryId, string guildId, string guildName)
        {
            RefreshTerritories();
        }

        public override void _Process(double delta)
        {
            // Update active war timers if visible
            if (_isVisible && _system != null)
            {
                RefreshActiveWars();
            }
        }

        #region Refresh Methods

        private void RefreshActiveWars()
        {
            var warsList = _activeWarsTab.GetNode<VBoxContainer>("WarsList");
            if (warsList == null) return;

            // Clear existing
            foreach (var child in warsList.GetChildren())
            {
                child.QueueFree();
            }

            var activeWars = _system.GetActiveWars();

            if (activeWars.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "No active wars at the moment",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                warsList.AddChild(emptyLabel);
                return;
            }

            foreach (var kvp in activeWars)
            {
                var war = kvp.Value;
                var warPanel = CreateWarPanel(war);
                warsList.AddChild(warPanel);
            }
        }

        private Control CreateWarPanel(GuildWar war)
        {
            var panel = new Panel
            {
                Size = new Vector2(820, 120)
            };

            var vbox = new VBoxContainer
            {
                Position = new Vector2(10, 10),
                Size = new Vector2(800, 100)
            };
            vbox.AddThemeConstantOverride("separation", 5);
            panel.AddChild(vbox);

            // War info
            var titleLabel = new Label
            {
                Text = $"⚔️ {war.Name} ({war.Type})"
            };
            titleLabel.AddThemeFontSizeOverride("font_size", 16);
            vbox.AddChild(titleLabel);

            var statusLabel = new Label
            {
                Text = $"Status: {war.State} | Guilds: {war.Participants.Count}/{war.MaxGuilds} | Prize: {war.PrizePool}"
            };
            vbox.AddChild(statusLabel);

            // Time remaining
            TimeSpan remaining = war.EndTime - DateTime.Now;
            var timeLabel = new Label
            {
                Text = remaining.TotalSeconds > 0 ? $"Time: {remaining.Hours}h {remaining.Minutes}m remaining" : "Time: Ended"
            };
            vbox.AddChild(timeLabel);

            // Rankings if active
            if (war.State == GuildWarState.Active && war.Participants.Count > 0)
            {
                var top3 = war.Participants.OrderByDescending(p => p.Score).Take(3).ToList();
                var rankingText = "Top: " + string.Join(" > ", top3.Select(p => $"{p.GuildName}({p.Score})"));
                var rankingLabel = new Label
                {
                    Text = rankingText
                };
                vbox.AddChild(rankingLabel);
            }

            return panel;
        }

        private void RefreshTerritories()
        {
            var territoryList = _territoryTab.GetNode<VBoxContainer>("TerritoryList");
            if (territoryList == null) return;

            // Clear existing
            foreach (var child in territoryList.GetChildren())
            {
                child.QueueFree();
            }

            var territories = _system.GetAllTerritories();

            foreach (var territory in territories)
            {
                var panel = new Panel
                {
                    Size = new Vector2(820, 80)
                };

                var hbox = new HBoxContainer
                {
                    Position = new Vector2(10, 10),
                    Size = new Vector2(800, 60)
                };
                hbox.AddThemeConstantOverride("separation", 20);
                panel.AddChild(hbox);

                var nameLabel = new Label
                {
                    Text = $"🏰 {territory.TerritoryName}",
                    Size = new Vector2(200, 60)
                };
                nameLabel.AddThemeFontSizeOverride("font_size", 16);
                hbox.AddChild(nameLabel);

                var ownerLabel = new Label
                {
                    Text = $"Owner: {territory.ControllingGuildName}",
                    Size = new Vector2(250, 60)
                };
                hbox.AddChild(ownerLabel);

                var resourceLabel = new Label
                {
                    Text = $"Resource: {territory.ResourceGeneration}/hr",
                    Size = new Vector2(200, 60)
                };
                hbox.AddChild(resourceLabel);

                var defenseLabel = new Label
                {
                    Text = $"Defense: {territory.DefenseLevel}",
                    Size = new Vector2(150, 60)
                };
                hbox.AddChild(defenseLabel);

                territoryList.AddChild(panel);
            }
        }

        private void RefreshHistory()
        {
            var historyList = _historyTab.GetNode<VBoxContainer>("HistoryList");
            if (historyList == null) return;

            // Clear existing
            foreach (var child in historyList.GetChildren())
            {
                child.QueueFree();
            }

            var history = _system.GetWarHistory();

            if (history.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "No war history yet",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                historyList.AddChild(emptyLabel);
                return;
            }

            foreach (var war in history.OrderByDescending(w => w.EndTime).Take(20))
            {
                var panel = new Panel
                {
                    Size = new Vector2(820, 100)
                };

                var vbox = new VBoxContainer
                {
                    Position = new Vector2(10, 10),
                    Size = new Vector2(800, 80)
                };
                vbox.AddThemeConstantOverride("separation", 5);
                panel.AddChild(vbox);

                var titleLabel = new Label
                {
                    Text = $"⚔️ {war.Name} - Winner: {war.WinnerId}"
                };
                titleLabel.AddThemeFontSizeOverride("font_size", 14);
                vbox.AddChild(titleLabel);

                var infoLabel = new Label
                {
                    Text = $"{war.Type} | {war.Participants.Count} guilds | Prize: {war.PrizePool} | {war.EndTime:yyyy-MM-dd HH:mm}"
                };
                vbox.AddChild(infoLabel);

                var rankingLabel = new Label
                {
                    Text = "Top 3: " + string.Join(" | ", war.Participants.OrderByDescending(p => p.Score).Take(3).Select(p => $"{p.GuildName}:{p.Score}"))
                };
                vbox.AddChild(rankingLabel);

                historyList.AddChild(panel);
            }
        }

        private void QueryGuildStats(string guildId)
        {
            var statsContainer = _statisticsTab.GetNode<VBoxContainer>("StatsContainer");
            if (statsContainer == null) return;

            // Clear existing stats
            foreach (var child in statsContainer.GetChildren())
            {
                child.QueueFree();
            }

            if (string.IsNullOrEmpty(guildId))
            {
                var errorLabel = new Label { Text = "Please enter a Guild ID" };
                statsContainer.AddChild(errorLabel);
                return;
            }

            var stats = _system.GetGuildStats(guildId);

            if (stats == null)
            {
                var notFoundLabel = new Label { Text = $"No statistics found for guild: {guildId}" };
                statsContainer.AddChild(notFoundLabel);
                return;
            }

            // Display stats
            statsContainer.AddChild(new Label { Text = $"📊 Statistics for: {guildId}" });
            statsContainer.AddChild(new Label { Text = $"Total Wars: {stats.TotalWars}" });
            statsContainer.AddChild(new Label { Text = $"Wins: {stats.Wins} | Losses: {stats.Losses} | Draws: {stats.Draws}" });
            
            double winRate = stats.TotalWars > 0 ? (double)stats.Wins / stats.TotalWars * 100 : 0;
            statsContainer.AddChild(new Label { Text = $"Win Rate: {winRate:F1}%" });
            
            statsContainer.AddChild(new Label { Text = $"Total Score: {stats.TotalScore}" });
            statsContainer.AddChild(new Label { Text = $"Kills: {stats.TotalKills} | Deaths: {stats.TotalDeaths}" });
            statsContainer.AddChild(new Label { Text = $"Current Win Streak: {stats.CurrentWinStreak}" });
            statsContainer.AddChild(new Label { Text = $"Longest Win Streak: {stats.LongestWinStreak}" });
            statsContainer.AddChild(new Label { Text = $"Highest Rank: {stats.HighestRank}" });
            statsContainer.AddChild(new Label { Text = $"Total Prize Earned: {stats.TotalPrizeEarned}" });
        }

        #endregion

        #region Toggle

        public void ToggleUI()
        {
            _isVisible = !_isVisible;
            
            if (_isVisible)
            {
                Show();
                RefreshAll();
            }
            else
            {
                Hide();
            }
        }

        private void RefreshAll()
        {
            RefreshActiveWars();
            RefreshTerritories();
            RefreshHistory();
        }

        #endregion

        #region Input Handling

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // Ctrl+Shift+G - Toggle Guild War UI
                if (keyEvent.Keycode == Key.G && 
                    keyEvent.ModifierMask.HasFlag(KeyModifierMask Ctrl) &&
                    keyEvent.ModifierMask.HasFlag(KeyModifierMask Shift))
                {
                    ToggleUI();
                    GetViewport().SetInputAsHandled();
                }
            }
        }

        #endregion
    }
}
