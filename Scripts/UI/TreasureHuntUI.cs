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
        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary<string, object>();
        }

        /// <summary>
        /// Import save data (UI class - no data to load)
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            // UI class - no data to import
        }

        /// <summary>
        /// Load regions from TreasureHuntManager
        /// </summary>
        public void LoadRegions()
        {
            if (_regionList == null)
            {
                return;
            }

            _regionList.Clear();

            var regions = TreasureHuntManager.Instance.GetRegions();
            foreach (var region in regions)
            {
                _regionList.AddItem($"{region.name} (Lv.{region.requiredLevel})");
                _regionList.SetItemMetadata(_regionList.ItemCount - 1, region);
            }

            UpdateTreasurePreview();
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

            _selectedRegion = itemData as TreasureHuntManager.HuntRegion;
            if (_selectedRegion == null)
            {
                return;
            }

            // Update info panel
            if (_infoPanel != null)
            {
                var nameLabel = _infoPanel.GetNodeOrNull<Label>("RegionNameLabel");
                var descLabel = _infoPanel.GetNodeOrNull<Label>("DescriptionLabel");
                var levelLabel = _infoPanel.GetNodeOrNull<Label>("LevelLabel");
                var energyCostLabel = _infoPanel.GetNodeOrNull<Label>("EnergyCostLabel");

                if (nameLabel != null) nameLabel.Text = _selectedRegion.name;
                if (descLabel != null) descLabel.Text = _selectedRegion.description;
                if (levelLabel != null) levelLabel.Text = $"Required Level: {_selectedRegion.requiredLevel}";
                if (energyCostLabel != null) energyCostLabel.Text = $"Energy Cost: {_selectedRegion.energyCost}";
                if (_successRateLabel != null) _successRateLabel.Text = $"Success Rate: {_selectedRegion.successRate:P0}";
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

            var region = _selectedRegion as TreasureHuntManager.HuntRegion;
            if (region == null || region.treasures == null)
            {
                return;
            }

            var treasureContainer = GetNodeOrNull<ItemList>("VBoxContainer/HBoxContainer/InfoPanel/TreasureContainer");
            if (treasureContainer == null)
            {
                return;
            }

            treasureContainer.Clear();

            foreach (var treasure in region.treasures)
            {
                treasureContainer.AddItem($"{treasure.name} - {treasure.goldReward} Gold");
            }
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

            var region = _selectedRegion as TreasureHuntManager.HuntRegion;
            if (region == null)
            {
                return;
            }

            int playerId = Player.Instance != null ? Player.Instance.playerId : 0;
            bool success = TreasureHuntManager.Instance.StartHunt(playerId, region.id);
            ShowResult(success);
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
