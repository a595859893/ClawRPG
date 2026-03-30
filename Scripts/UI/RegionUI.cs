using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// Region UI - Shows current region and allows teleportation
    /// </summary>
    
    public class RegionUI : Control
    {
        [Export] public Color LockedColor { get; set; } = new Color(0.3f, 0.3f, 0.3f, 0.7f);
        [Export] public Color UnlockedColor { get; set; } = new Color(0.2f, 0.8f, 0.2f, 0.9f);
        [Export] public Color CurrentRegionColor { get; set; } = new Color(1f, 0.9f, 0.3f, 1f);

        private Label _regionNameLabel;
        private Label _regionDescLabel;
        private Label _levelReqLabel;
        private VBoxContainer _regionListContainer;
        private Player _player;
        private bool _isVisible = false; 

        public override void _Ready()
        {
            Visible = false; 
            SetupUI();
            
            // Get player reference
            _player = GetTree().GetFirstNodeInGroup("player") as Player;
            
            // Connect to region manager
            if (RegionManager.Instance != null)
            {
                RegionManager.Instance.RegionChanged += OnRegionChanged;
            }
        }

        private void SetupUI()
        {
            // Main container
            var mainContainer = new VBoxContainer
            {
                AnchorLeft = 0.5f,
                AnchorTop = 0.5f,
                AnchorRight = 0.5f,
                AnchorBottom = 0.5f,
                OffsetLeft = -200,
                OffsetTop = -250,
                OffsetRight = 200,
                OffsetBottom = 250
            };
            AddChild(mainContainer);

            // Title
            var titleLabel = new Label
            {
                Text = "🗺️ 区域选择",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            mainContainer.AddChild(titleLabel);

            // Current region info
            _regionNameLabel = new Label
            {
                Text = "当前区域: 暮光森林",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            mainContainer.AddChild(_regionNameLabel);

            _regionDescLabel = new Label
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                Modulate = new Color(0.7f, 0.7f, 0.7f)
            };
            mainContainer.AddChild(_regionDescLabel);

            _levelReqLabel = new Label
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                Modulate = new Color(1f, 0.8f, 0.4f)
            };
            mainContainer.AddChild(_levelReqLabel);

            // Separator
            var separator = new HSeparator();
            mainContainer.AddChild(separator);

            // Region list
            _regionListContainer = new VBoxContainer();
            mainContainer.AddChild(_regionListContainer);

            // Close button
            var closeButton = new Button
            {
                Text = "关闭 (ESC)",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            closeButton.Pressed += () => ToggleVisibility();
            mainContainer.AddChild(closeButton);

            // Style
            var style = new StyleBoxFlat
            {
                BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f),
                BorderColor = new Color(0.3f, 0.3f, 0.4f),
                BorderWidthLeft = 2,
                BorderWidthTop = 2,
                BorderWidthRight = 2,
                BorderWidthBottom = 2,
                CornerRadiusTopLeft = 8,
                CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8,
                CornerRadiusBottomRight = 8
            };
            mainContainer.AddChild(style);
            mainContainer.SelfModulate = new Color(1f, 1f, 1f, 0.98f);
        }

        public override void _Process(double delta)
        {
            if (!_isVisible)
                return;

            UpdateRegionList();
        }

        private void UpdateRegionList()
        {
            if (_player == null || RegionDatabase.Instance == null)
                return;

            // Clear existing buttons
            foreach (Node child in _regionListContainer.GetChildren())
            {
                child.QueueFree();
            }

            var regions = RegionDatabase.Instance.GetAllRegions();
            int playerLevel = _player?.Level ?? 1;
            string currentRegion = RegionManager.Instance?.CurrentRegionId ?? "forest";

            // Sort regions by required level
            var sortedRegions = new List<KeyValuePair<string, RegionType>>(regions);
            sortedRegions.Sort((a, b) => a.Value.RequiredLevel.CompareTo(b.Value.RequiredLevel));

            foreach (var kvp in sortedRegions)
            {
                var region = kvp.Value;
                bool isUnlocked = region.RequiredLevel <= playerLevel;
                bool isCurrent = region.RegionId == currentRegion;

                var button = new Button
                {
                    Text = $"{(isCurrent ? "⭐ " : "")}{region.RegionName} (Lv.{region.RequiredLevel})",
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                if (isCurrent)
                {
                    button.Modulate = CurrentRegionColor;
                    button.Disabled = true;
                }
                else if (!isUnlocked)
                {
                    button.Modulate = LockedColor;
                    button.Disabled = true;
                    button.Text = $"🔒 {region.RegionName} (需要 Lv.{region.RequiredLevel})";
                }
                else
                {
                    button.Modulate = UnlockedColor;
                    button.Pressed += () => OnRegionButtonPressed(region.RegionId);
                }

                _regionListContainer.AddChild(button);
            }
        }

        private void OnRegionButtonPressed(string regionId)
        {
            if (RegionManager.Instance != null)
            {
                RegionManager.Instance.ChangeRegion(regionId);
                GD.Print($"[RegionUI] Teleporting to region: {regionId}");
            }
        }

        private void OnRegionChanged(string regionId, string regionName)
        {
            UpdateCurrentRegionInfo(regionName);
        }

        private void UpdateCurrentRegionInfo(string regionName)
        {
            if (_regionNameLabel != null)
            {
                _regionNameLabel.Text = $"当前区域: {regionName}";
            }

            var region = RegionManager.Instance?.CurrentRegion;
            if (region != null)
            {
                if (_regionDescLabel != null)
                {
                    _regionDescLabel.Text = region.Description;
                }
                
                if (_levelReqLabel != null)
                {
                    string multipliers = $"经验 x{region.ExpMultiplier} | 掉落 x{region.DropRateMultiplier}";
                    if (region.EnvironmentalDamagePerSecond > 0)
                    {
                        multipliers += " | 环境伤害!";
                    }
                    _levelReqLabel.Text = multipliers;
                }
            }
        }

        public void ToggleVisibility()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;

            if (_isVisible)
            {
                UpdateCurrentRegionInfo(RegionManager.Instance?.CurrentRegion?.RegionName ?? "未知");
            }
            else
            {
                // Resume game if paused
                GetTree().Paused = false; 
            }
        }

        public override void _Input(InputEvent e)
        {
            if (e.IsActionPressed("ui_cancel")) // ESC key
            {
                if (Visible)
                {
                    ToggleVisibility();
                    GetTree().SetInputAsHandled();
                }
            }
            
            if (e.IsActionPressed("region_map")) // R key for region map
            {
                ToggleVisibility();
                GetTree().SetInputAsHandled();
            }
        }
    }
}
