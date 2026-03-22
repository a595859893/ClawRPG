using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.UI
{
    /// <summary>
    /// TreasureHuntUI - 寻宝系统UI
    /// 提供寻宝系统的图形界面
    /// </summary>
    public partial class TreasureHuntUI : Control
    {
        /// <summary>
        /// Currently selected region
        /// </summary>
        private object _selectedRegion;

        /// <summary>
        /// UI Elements
        /// </summary>
        private ItemList _regionList;
        private PanelContainer _infoPanel;
        private PanelContainer _statsPanel;
        private Label _energyLabel;
        private Label _successRateLabel;
        private Button _huntButton;
        private Button _closeButton;

        public override void _Ready()
        {
            // Get UI element references
            _regionList = GetNodeOrNull<ItemList>("VBoxContainer/RegionList");
            _infoPanel = GetNodeOrNull<PanelContainer>("VBoxContainer/HBoxContainer/InfoPanel");
            _statsPanel = GetNodeOrNull<PanelContainer>("VBoxContainer/HBoxContainer/StatsPanel");
            _energyLabel = GetNodeOrNull<Label>("VBoxContainer/HBoxContainer/StatsPanel/EnergyLabel");
            _successRateLabel = GetNodeOrNull<Label>("VBoxContainer/HBoxContainer/InfoPanel/SuccessRateLabel");
            _huntButton = GetNodeOrNull<Button>("VBoxContainer/HBoxContainer/InfoPanel/HuntButton");
            _closeButton = GetNodeOrNull<Button>("VBoxContainer/CloseButton");

            // Connect signals
            if (_closeButton != null)
            {
                _closeButton.Pressed += OnClosePressed;
            }

            if (_huntButton != null)
            {
                _huntButton.Pressed += OnHuntPressed;
            }

            if (_regionList != null)
            {
                _regionList.ItemSelected += OnRegionSelected;
            }
        }

        /// <summary>
        /// Export save data (UI class - no data to save)
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            return new Dictionary();
        }

        /// <summary>
        /// Import save data (UI class - no data to load)
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            // UI class - no data to import
        }

        /// <summary>
        /// Load regions from TreasureHuntManager
        /// </summary>
        public void LoadRegions()
        {
            // Note: TreasureHuntManager is still GDScript - this will need to be updated
            // when TreasureHuntManager is converted to C#
            // For now, this is a placeholder implementation

            if (_regionList == null)
            {
                return;
            }

            _regionList.Clear();

            // TODO: When TreasureHuntManager is converted to C#, implement region loading
            // var regions = TreasureHuntManager.Instance.GetRegions();
        }

        /// <summary>
        /// Handle region selection
        /// </summary>
        private void OnRegionSelected(long index)
        {
            if (_regionList == null)
            {
                return;
            }

            var itemData = _regionList.GetItemMetadata((int)index);
            if (itemData == null)
            {
                return;
            }

            // Assuming itemData contains region info
            // This will need to be updated when TreasureHuntManager is converted

            // Update info panel
            if (_infoPanel != null)
            {
                var nameLabel = _infoPanel.GetNodeOrNull<Label>("RegionNameLabel");
                var descLabel = _infoPanel.GetNodeOrNull<Label>("DescriptionLabel");
                var levelLabel = _infoPanel.GetNodeOrNull<Label>("LevelLabel");
                var energyCostLabel = _infoPanel.GetNodeOrNull<Label>("EnergyCostLabel");

                // Update labels with region data
                // These will be populated when region selection is fully implemented
            }

            UpdateTreasurePreview();
        }

        /// <summary>
        /// Update treasure preview for selected region
        /// </summary>
        private void UpdateTreasurePreview()
        {
            if (_selectedRegion == null)
            {
                return;
            }

            var treasureContainer = GetNodeOrNull<ItemList>("VBoxContainer/HBoxContainer/InfoPanel/TreasureContainer");
            if (treasureContainer == null)
            {
                return;
            }

            treasureContainer.Clear();

            // TODO: When TreasureHuntManager is converted, populate treasures
            // foreach (var treasure in selectedRegion.Treasures)
            // {
            //     treasureContainer.AddItem($"{treasure.Name} - {treasure.GoldReward} Gold");
            // }
        }

        /// <summary>
        /// Handle hunt button pressed
        /// </summary>
        private void OnHuntPressed()
        {
            if (_selectedRegion == null)
            {
                return;
            }

            // Note: TreasureHuntManager is still GDScript
            // var success = TreasureHuntManager.Instance.StartHunt(playerId, selectedRegion.Id);

            // For now, show placeholder result
            ShowResult(true);
        }

        /// <summary>
        /// Show hunt result
        /// </summary>
        private void ShowResult(bool success)
        {
            var resultPanel = GetNodeOrNull<PanelContainer>("VBoxContainer/HBoxContainer/ResultPanel");
            if (resultPanel == null)
            {
                return;
            }

            resultPanel.Visible = true;

            var resultLabel = resultPanel.GetNodeOrNull<Label>("ResultLabel");
            if (resultLabel != null)
            {
                if (success)
                {
                    resultLabel.Text = "Treasure Found!";
                    resultLabel.Modulate = Colors.Green;
                }
                else
                {
                    resultLabel.Text = "Hunt Failed...";
                    resultLabel.Modulate = Colors.Red;
                }
            }

            // Auto-hide after 2 seconds
            GetTree().CreateTimer(2.0f).Timeout += () =>
            {
                resultPanel.Visible = false;
            };
        }

        /// <summary>
        /// Update stats display
        /// </summary>
        private void UpdateStats()
        {
            // Note: TreasureHuntManager is still GDScript
            // This will need to be updated when TreasureHuntManager is converted

            if (_energyLabel != null)
            {
                // _energyLabel.Text = $"Energy: {data.CurrentEnergy}/{data.MaxEnergy}";
            }

            var statsLabel = GetNodeOrNull<Label>("VBoxContainer/HBoxContainer/StatsPanel/StatsLabel");
            if (statsLabel != null)
            {
                // Populate stats when TreasureHuntManager is available
            }
        }

        /// <summary>
        /// Handle close button pressed
        /// </summary>
        private void OnClosePressed()
        {
            Visible = false;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel") && Visible)
            {
                Visible = false;
            }
        }
    }
}
