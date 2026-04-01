using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Skill Mastery UI - Display and manage skill mastery
    /// </summary>
    public partial class SkillMasteryUI : Control
    {
        private VBoxContainer _mainContainer;
        private VBoxContainer _skillListContainer;
        private Label _titleLabel;
        private Label _statsLabel;
        private ScrollContainer _scrollContainer;
        
        // Theme colors
        private Color _noviceColor = new Color(0.7f, 0.7f, 0.7f);
        private Color _apprenticeColor = new Color(0.3f, 0.7f, 0.3f);
        private Color _journeymanColor = new Color(0.3f, 0.5f, 0.9f);
        private Color _expertColor = new Color(0.6f, 0.3f, 0.9f);
        private Color _masterColor = new Color(0.9f, 0.6f, 0.1f);
        private Color _grandMasterColor = new Color(1f, 0.8f, 0.2f);

        private bool _isVisible = false;

        public override void _Ready()
        {
            Visible = false;
            SetupUI();
        }

        private void SetupUI()
        {
            // Main panel
            var panel = new PanelContainer
            {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 200,
                OffsetTop = 100,
                OffsetRight = -200,
                OffsetBottom = -100
            };
            AddChild(panel);

            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(8);
            panel.AddThemeStyleboxOverride("panel", style);

            // Main container
            _mainContainer = new VBoxContainer();
            _mainContainer.AddThemeConstantOverride("separation", 10);
            panel.AddChild(_mainContainer);

            // Title
            _titleLabel = new Label
            {
                Text = "⚔️ Skill Mastery",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _mainContainer.AddChild(_titleLabel);

            // Stats header
            _statsLabel = new Label
            {
                Text = "Loading statistics...",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _statsLabel.AddThemeFontSizeOverride("font_size", 14);
            _mainContainer.AddChild(_statsLabel);

            // Separator
            var hsep = new HSeparator();
            _mainContainer.AddChild(hsep);

            // Scroll container for skill list
            _scrollContainer = new ScrollContainer();
            _scrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _mainContainer.AddChild(_scrollContainer);

            _skillListContainer = new VBoxContainer();
            _skillListContainer.AddThemeConstantOverride("separation", 5);
            _scrollContainer.AddChild(_skillListContainer);

            // Close hint
            var closeHint = new Label
            {
                Text = "Press [M] or ESC to close",
                HorizontalAlignment = HorizontalAlignment.Center,
                Modulate = new Color(0.6f, 0.6f, 0.6f)
            };
            closeHint.AddThemeFontSizeOverride("font_size", 12);
            _mainContainer.AddChild(closeHint);

            // Update data
            RefreshData();
        }

        public override void _Input(InputEvent eventArgs)
        {
            if (eventArgs is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.M || keyEvent.Keycode == Key.Escape)
                {
                    ToggleVisibility();
                    GetTree().SetInputAsHandled();
                }
            }
        }

        public void ToggleVisibility()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;

            if (_isVisible)
            {
                RefreshData();
            }
        }

        private void RefreshData()
        {
            // Clear existing items
            foreach (var child in _skillListContainer.GetChildren())
            {
                child.QueueFree();
            }

            var masterySystem = SkillMasterySystem.Instance;
            var stats = masterySystem.GetStatistics();
            var skillsData = masterySystem.GetAllSkillsMastery();

            // Update stats
            _statsLabel.Text = $"Total Points: {stats["totalMasteryPoints"]} | " +
                             $"Skills: {stats["totalSkills"]} | " +
                             $"Grand Masters: {stats["grandMasterCount"]}";

            // Add skill items
            if (skillsData.Count == 0)
            {
                var noDataLabel = new Label
                {
                    Text = "No skills mastered yet. Use skills to gain mastery!",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Modulate = new Color(0.7f, 0.7f, 0.7f)
                };
                _skillListContainer.AddChild(noDataLabel);
            }
            else
            {
                foreach (var skill in skillsData)
                {
                    var item = CreateSkillItem(skill);
                    _skillListContainer.AddChild(item);
                }
            }
        }

        private Control CreateSkillItem(Dictionary<string, object> skill)
        {
            var container = new HBoxContainer();
            container.CustomMinimumSize = new Vector2(0, 60);

            // Skill icon placeholder (colored box based on type)
            var typeBox = new ColorRect
            {
                CustomMinimumSize = new Vector2(50, 50),
                Color = GetTypeColor(skill["type"].ToString())
            };
            container.AddChild(typeBox);

            // Skill info
            var infoContainer = new VBoxContainer();
            infoContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            container.AddChild(infoContainer);

            // Skill name and tier
            var nameLabel = new Label
            {
                Text = $"{skill["skillName"]} ({skill["tier"]})",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            nameLabel.Modulate = GetTierColor(skill["tier"].ToString());
            infoContainer.AddChild(nameLabel);

            // Stats
            var statsText = $"Uses: {skill["totalUses"]} | Points: {skill["masteryPoints"]} | " +
                          $"DMG: +{(float)skill["damageBonus"] * 100:F0}% | " +
                          $"CDR: -{(float)skill["cooldownReduction"] * 100:F0}%";
            var statsLabel = new Label
            {
                Text = statsText,
                HorizontalAlignment = HorizontalAlignment.Left,
                Modulate = new Color(0.8f, 0.8f, 0.8f)
            };
            statsLabel.AddThemeFontSizeOverride("font_size", 12);
            infoContainer.AddChild(statsLabel);

            // Progress bar
            var progressBar = new ProgressBar();
            progressBar.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            progressBar.CustomMinimumSize = new Vector2(0, 10);
            progressBar.Value = (float)skill["progressToNextTier"] * 100;
            progressBar.MaxValue = 100;
            
            var progressStyle = new StyleBoxFlat();
            progressStyle.BgColor = new Color(0.2f, 0.2f, 0.3f);
            progressStyle.SetCornerRadiusAll(4);
            progressBar.AddThemeStyleboxOverride("background", progressStyle);
            
            var fillStyle = new StyleBoxFlat();
            fillStyle.BgColor = GetTierColor(skill["tier"].ToString());
            fillStyle.SetCornerRadiusAll(4);
            progressBar.AddThemeStyleboxOverride("fill", fillStyle);
            
            infoContainer.AddChild(progressBar);

            // Bonuses count
            var bonusLabel = new Label
            {
                Text = $"🎁 Bonuses: {skill["unlockedBonuses"]}",
                HorizontalAlignment = HorizontalAlignment.Right,
                Modulate = new Color(1f, 0.9f, 0.5f)
            };
            bonusLabel.AddThemeFontSizeOverride("font_size", 12);
            container.AddChild(bonusLabel);

            return container;
        }

        private Color GetTierColor(string tier)
        {
            switch (tier)
            {
                case "Novice": return _noviceColor;
                case "Apprentice": return _apprenticeColor;
                case "Journeyman": return _journeymanColor;
                case "Expert": return _expertColor;
                case "Master": return _masterColor;
                case "GrandMaster": return _grandMasterColor;
                default: return _noviceColor;
            }
        }

        private Color GetTypeColor(string type)
        {
            switch (type)
            {
                case "Attack": return new Color(0.9f, 0.3f, 0.3f);
                case "Defense": return new Color(0.3f, 0.5f, 0.9f);
                case "Support": return new Color(0.3f, 0.8f, 0.4f);
                case "Magic": return new Color(0.6f, 0.3f, 0.9f);
                case "Healing": return new Color(0.4f, 0.9f, 0.6f);
                case "Utility": return new Color(0.8f, 0.7f, 0.4f);
                default: return new Color(0.5f, 0.5f, 0.5f);
            }
        }

        public void Refresh()
        {
            if (_isVisible)
            {
                RefreshData();
            }
        }
    }
}
