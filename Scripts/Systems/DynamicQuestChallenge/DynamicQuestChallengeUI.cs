using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.DynamicQuestChallenge
{
    /// <summary>
    /// DynamicQuestChallengeUI - 动态任务挑战系统UI
    /// 提供挑战系统的图形界面
    /// </summary>
    public partial class DynamicQuestChallengeUI : Control
    {
        /// <summary>
        /// Reference to the challenge system
        /// </summary>
        private DynamicQuestChallengeSystem _system;

        /// <summary>
        /// UI Elements
        /// </summary>
        private Label _titleLabel;
        private Button _activeTab;
        private Button _completedTab;
        private Button _statsTab;
        private ScrollContainer _challengeContainer;
        private VBoxContainer _challengeVBox;
        private VBoxContainer _statsContainer;

        /// <summary>
        /// Colors for difficulty badges
        /// </summary>
        private Color _colorActive = new Color(0.2f, 0.8f, 0.2f);
        private Color _colorCompleted = new Color(0.2f, 0.5f, 0.9f);
        private Color _colorStats = new Color(0.9f, 0.7f, 0.2f);
        private Color _colorEasy = new Color(0.5f, 0.8f, 0.5f);
        private Color _colorMedium = new Color(0.8f, 0.8f, 0.2f);
        private Color _colorHard = new Color(0.9f, 0.6f, 0.2f);
        private Color _colorEpic = new Color(0.8f, 0.3f, 0.8f);
        private Color _colorLegendary = new Color(1.0f, 0.8f, 0.0f);

        /// <summary>
        /// Export save data (UI class - no data to save)
        /// </summary>
        public Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary<string, object>();
        }

        /// <summary>
        /// Import save data (UI class - no data to load)
        /// </summary>
        public void ImportSaveData(Dictionary<string, object> data)
        {
            // UI class - no data to import
        }

        public override void _Ready()
        {
            // Try to get system reference
            var mainNode = GetNodeOrNull("/root/Main");
            if (mainNode != null)
            {
                _system = mainNode.GetNodeOrNull<DynamicQuestChallengeSystem>("DynamicQuestChallengeSystem");
            }

            if (_system == null)
            {
                // Create system if not found
                GD.Print("[DynamicQuestChallengeUI] System not found, creating new instance");
            }

            SetupUi();
        }

        /// <summary>
        /// Setup the UI elements
        /// </summary>
        private void SetupUi()
        {
            // Main panel
            var panel = new PanelContainer();
            panel.AnchorsPreset = (Control.LayoutPreset)5; // FullRect
            panel.OffsetLeft = 200;
            panel.OffsetTop = 100;
            panel.OffsetRight = -200;
            panel.OffsetBottom = -100;
            AddChild(panel);

            var vbox = new VBoxContainer();
            panel.AddChild(vbox);

            // Title
            _titleLabel = new Label();
            _titleLabel.Text = "Dynamic Quest Challenge";
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            vbox.AddChild(_titleLabel);

            // Tab buttons
            var tabHbox = new HBoxContainer();
            vbox.AddChild(tabHbox);

            _activeTab = new Button();
            _activeTab.Text = "Active";
            _activeTab.Pressed += OnActiveTabPressed;
            tabHbox.AddChild(_activeTab);

            _completedTab = new Button();
            _completedTab.Text = "Completed";
            _completedTab.Pressed += OnCompletedTabPressed;
            tabHbox.AddChild(_completedTab);

            _statsTab = new Button();
            _statsTab.Text = "Statistics";
            _statsTab.Pressed += OnStatsTabPressed;
            tabHbox.AddChild(_statsTab);

            // Challenge container
            _challengeContainer = new ScrollContainer();
            _challengeContainer.SizeFlagsVertical = (SizeFlags)3;
            _challengeContainer.VerticalScrollFree = true;
            vbox.AddChild(_challengeContainer);

            _challengeVBox = new VBoxContainer();
            _challengeVBox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _challengeContainer.AddChild(_challengeVBox);

            // Stats container
            _statsContainer = new VBoxContainer();
            _statsContainer.Visible = false;
            vbox.AddChild(_statsContainer);

            // Generate button
            var generateBtn = new Button();
            generateBtn.Text = "Generate New Challenge";
            generateBtn.Pressed += OnGeneratePressed;
            vbox.AddChild(generateBtn);

            // Close button
            var closeBtn = new Button();
            closeBtn.Text = "Close (ESC)";
            closeBtn.Pressed += OnClosePressed;
            vbox.AddChild(closeBtn);

            ShowActiveChallenges();
        }

        public override void _Process(double delta)
        {
            if (_system != null)
            {
                _system.CheckExpired();
            }
        }

        /// <summary>
        /// Show active challenges
        /// </summary>
        private void ShowActiveChallenges()
        {
            ClearChallengeContainer();
            _statsContainer.Visible = false;
            _challengeContainer.Visible = true;

            if (_system == null)
            {
                return;
            }

            var challenges = _system.GetActiveChallenges();

            if (challenges.Count == 0)
            {
                var emptyLabel = new Label();
                emptyLabel.Text = "No active challenges";
                emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
                _challengeVBox.AddChild(emptyLabel);
                return;
            }

            foreach (Dictionary challenge in challenges)
            {
                var card = CreateChallengeCard(challenge, true);
                _challengeVBox.AddChild(card);
            }
        }

        /// <summary>
        /// Show completed challenges
        /// </summary>
        private void ShowCompletedChallenges()
        {
            ClearChallengeContainer();
            _statsContainer.Visible = false;
            _challengeContainer.Visible = true;

            if (_system == null)
            {
                return;
            }

            var challenges = _system.GetCompletedChallenges();

            if (challenges.Count == 0)
            {
                var emptyLabel = new Label();
                emptyLabel.Text = "No completed challenges";
                emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
                _challengeVBox.AddChild(emptyLabel);
                return;
            }

            foreach (Dictionary challenge in challenges)
            {
                var card = CreateChallengeCard(challenge, false);
                _challengeVBox.AddChild(card);
            }
        }

        /// <summary>
        /// Show statistics
        /// </summary>
        private void ShowStatistics()
        {
            ClearChallengeContainer();
            _challengeContainer.Visible = false;
            _statsContainer.Visible = true;

            if (_system == null)
            {
                return;
            }

            var stats = _system.GetStatistics();

            // Clear existing stats
            foreach (var child in _statsContainer.GetChildren())
            {
                child.QueueFree();
            }

            var statsTitle = new Label();
            statsTitle.Text = "Challenge Statistics";
            statsTitle.AddThemeFontSizeOverride("font_size", 20);
            _statsContainer.AddChild(statsTitle);

            var statsItems = new List<string[]>
            {
                new[] { "Total Generated", GetStatValue(stats, "total_generated").ToString() },
                new[] { "Total Completed", GetStatValue(stats, "total_completed").ToString() },
                new[] { "Total Abandoned", GetStatValue(stats, "total_abandoned").ToString() },
                new[] { "Current Streak", GetStatValue(stats, "current_streak").ToString() },
                new[] { "Longest Streak", GetStatValue(stats, "longest_streak").ToString() },
                new[] { "Total Gold Earned", GetStatValue(stats, "total_gold_earned").ToString() },
                new[] { "Total Experience Earned", GetStatValue(stats, "total_experience_earned").ToString() }
            };

            foreach (var item in statsItems)
            {
                var label = new Label();
                label.Text = item[0] + ": " + item[1];
                _statsContainer.AddChild(label);
            }
        }

        /// <summary>
        /// Get stat value safely
        /// </summary>
        private object GetStatValue(Dictionary<string, object> stats, string key)
        {
            if (stats.ContainsKey(key))
            {
                return stats[key];
            }
            return 0;
        }

        /// <summary>
        /// Create a challenge card
        /// </summary>
        private Control CreateChallengeCard(Dictionary challenge, bool isActive)
        {
            var card = new PanelContainer();
            card.CustomMinimumSize = new Vector2(0, 80);

            var hbox = new HBoxContainer();
            card.AddChild(hbox);

            // Info section
            var infoVbox = new VBoxContainer();
            infoVbox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            hbox.AddChild(infoVbox);

            var nameLabel = new Label();
            nameLabel.Text = challenge.ContainsKey("name") ? (string)challenge["name"] : "Unknown";
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            infoVbox.AddChild(nameLabel);

            var descLabel = new Label();
            descLabel.Text = challenge.ContainsKey("description") ? (string)challenge["description"] : "";
            infoVbox.AddChild(descLabel);

            // Progress
            var progressLabel = new Label();
            if (isActive)
            {
                var progress = challenge.ContainsKey("progress") ? (int)challenge["progress"] : 0;
                var target = challenge.ContainsKey("target_amount") ? (int)challenge["target_amount"] : 0;
                progressLabel.Text = $"Progress: {progress}/{target}";
            }
            else
            {
                progressLabel.Text = "Completed!";
            }
            infoVbox.AddChild(progressLabel);

            // Difficulty badge
            var difficulty = challenge.ContainsKey("difficulty") ? (string)challenge["difficulty"] : "Easy";
            var difficultyLabel = new Label();
            difficultyLabel.Text = $"[{difficulty}]";
            difficultyLabel.Modulate = GetDifficultyColor(difficulty);
            hbox.AddChild(difficultyLabel);

            // Abandon button for active challenges
            if (isActive)
            {
                var abandonBtn = new Button();
                abandonBtn.Text = "Abandon";
                var challengeId = challenge.ContainsKey("id") ? (string)challenge["id"] : "";
                abandonBtn.Pressed += () => OnAbandonPressed(challengeId);
                hbox.AddChild(abandonBtn);
            }

            return card;
        }

        /// <summary>
        /// Get color for difficulty
        /// </summary>
        private Color GetDifficultyColor(string difficulty)
        {
            return difficulty switch
            {
                "Easy" => _colorEasy,
                "Medium" => _colorMedium,
                "Hard" => _colorHard,
                "Epic" => _colorEpic,
                "Legendary" => _colorLegendary,
                _ => Colors.White
            };
        }

        /// <summary>
        /// Clear challenge container
        /// </summary>
        private void ClearChallengeContainer()
        {
            foreach (var child in _challengeVBox.GetChildren())
            {
                child.QueueFree();
            }
        }

        /// <summary>
        /// Signal handler: Active tab pressed
        /// </summary>
        private void OnActiveTabPressed()
        {
            ShowActiveChallenges();
        }

        /// <summary>
        /// Signal handler: Completed tab pressed
        /// </summary>
        private void OnCompletedTabPressed()
        {
            ShowCompletedChallenges();
        }

        /// <summary>
        /// Signal handler: Stats tab pressed
        /// </summary>
        private void OnStatsTabPressed()
        {
            ShowStatistics();
        }

        /// <summary>
        /// Signal handler: Generate pressed
        /// </summary>
        private void OnGeneratePressed()
        {
            if (_system != null)
            {
                var challenge = _system.GenerateChallenge(10, "Warrior", new List<object>());
                GD.Print($"[DynamicQuestChallengeUI] Generated challenge: {(challenge.ContainsKey("name") ? challenge["name"] : "")}");
                ShowActiveChallenges();
            }
        }

        /// <summary>
        /// Signal handler: Abandon pressed
        /// </summary>
        private void OnAbandonPressed(string challengeId)
        {
            if (_system != null)
            {
                _system.AbandonChallenge(challengeId);
                ShowActiveChallenges();
            }
        }

        /// <summary>
        /// Signal handler: Close pressed
        /// </summary>
        private void OnClosePressed()
        {
            QueueFree();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel"))
            {
                OnClosePressed();
            }
        }
    }
}
