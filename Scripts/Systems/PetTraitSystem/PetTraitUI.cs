using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Data;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// UI for managing pet traits.
    /// </summary>
    public class PetTraitUI : Control
    {
        private TabContainer _tabContainer;
        private VBoxContainer _overviewTab;
        private VBoxContainer _traitsTab;
        private VBoxContainer _statisticsTab;
        
        // References
        private PetTraitSystem _traitSystem;
        
        // Colors for rarity
        private Color _commonColor = new Color(0.7f, 0.7f, 0.7f);
        private Color _uncommonColor = new Color(0.2f, 0.8f, 0.2f);
        private Color _rareColor = new Color(0.2f, 0.5f, 1.0f);
        private Color _epicColor = new Color(0.6f, 0.3f, 0.8f);
        private Color _legendaryColor = new Color(1.0f, 0.6f, 0.1f);
        
        public override void _Ready()
        {
            _traitSystem = PetTraitSystem.Instance;
            SetupUI();
            
            // Connect input
            Input.SetMouseMode(Input.MouseModeEnum.Visible);
        }
        
        private void SetupUI()
        {
            // Main panel
            var panel = new PanelContainer();
            panel.SetAnchor(AnchorPreset.FullRect);
            panel.MouseFilter = Control.MouseFilterEnum.Stop;
            AddChild(panel);
            
            var mainVBox = new VBoxContainer();
            mainVBox.SetAnchor(AnchorPreset.FullRect);
            panel.AddChild(mainVBox);
            
            // Header
            var header = new HBoxContainer();
            mainVBox.AddChild(header);
            
            var titleLabel = new Label();
            titleLabel.Text = "  🐾 Pet Trait System";
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            header.AddChild(titleLabel);
            
            header.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
            
            var closeBtn = new Button();
            closeBtn.Text = "✕";
            closeBtn.TooltipText = "Close (ESC)";
            closeBtn.Pressed += () => Hide();
            header.AddChild(closeBtn);
            
            // Tab container
            _tabContainer = new TabContainer();
            _tabContainer.SetVSizeFlags(Control.SizeFlags.ExpandFill);
            mainVBox.AddChild(_tabContainer);
            
            // Overview tab
            _overviewTab = new VBoxContainer();
            _overviewTab.Name = "Overview";
            _tabContainer.AddChild(_overviewTab);
            SetupOverviewTab();
            
            // Traits tab
            _traitsTab = new VBoxContainer();
            _traitsTab.Name = "All Traits";
            _tabContainer.AddChild(_traitsTab);
            SetupTraitsTab();
            
            // Statistics tab
            _statisticsTab = new VBoxContainer();
            _statisticsTab.Name = "Statistics";
            _tabContainer.AddChild(_statisticsTab);
            SetupStatisticsTab();
        }
        
        private void SetupOverviewTab()
        {
            // Summary section
            var summaryBox = new VBoxContainer();
            _overviewTab.AddChild(summaryBox);
            
            var summaryLabel = new Label();
            summaryLabel.Text = "Active Bonuses";
            summaryLabel.AddThemeFontSizeOverride("font_size", 18);
            summaryBox.AddChild(summaryLabel);
            
            var separator = new HSeparator();
            summaryBox.AddChild(separator);
            
            // Bonus display
            var bonuses = new GridContainer();
            bonuses.Columns = 2;
            summaryBox.AddChild(bonuses);
            
            AddBonusRow(bonuses, "Attack Bonus", "+" + String.Format("{0:P0}", _traitSystem.GetAttackBonus()));
            AddBonusRow(bonuses, "Defense Bonus", "+" + String.Format("{0:P0}", _traitSystem.GetDefenseBonus()));
            AddBonusRow(bonuses, "Health Bonus", "+" + String.Format("{0:P0}", _traitSystem.GetHealthBonus()));
            AddBonusRow(bonuses, "Speed Bonus", "+" + String.Format("{0:P0}", _traitSystem.GetSpeedBonus()));
            AddBonusRow(bonuses, "Critical Bonus", "+" + String.Format("{0:P0}", _traitSystem.GetCriticalBonus()));
            AddBonusRow(bonuses, "Evasion Bonus", "+" + String.Format("{0:P0}", _traitSystem.GetEvasionBonus()));
            AddBonusRow(bonuses, "Exp Bonus", "+" + String.Format("{0:P0}", _traitSystem.GetExpBonus()));
            AddBonusRow(bonuses, "Gold Bonus", "+" + String.Format("{0:P0}", _traitSystem.GetGoldBonus()));
            AddBonusRow(bonuses, "Drop Rate Bonus", "+" + String.Format("{0:P0}", _traitSystem.GetDropRateBonus()));
            
            // Active traits section
            var activeSection = new VBoxContainer();
            _overviewTab.AddChild(activeSection);
            
            var activeLabel = new Label();
            activeLabel.Text = "\nActive Traits";
            activeLabel.AddThemeFontSizeOverride("font_size", 18);
            activeSection.AddChild(activeLabel);
            
            var activeSeparator = new HSeparator();
            activeSection.AddChild(activeSeparator);
            
            var activeList = new VBoxContainer();
            activeSection.AddChild(activeList);
            
            var activeTraits = _traitSystem.GetActiveTraits();
            if (activeTraits.Count == 0)
            {
                var noTraits = new Label();
                noTraits.Text = "No active traits. Unlock and activate traits in the 'All Traits' tab.";
                activeList.AddChild(noTraits);
            }
            else
            {
                foreach (var trait in activeTraits)
                {
                    var traitPanel = CreateTraitPanel(trait, true);
                    activeList.AddChild(traitPanel);
                }
            }
        }
        
        private void SetupTraitsTab()
        {
            var scroll = new ScrollContainer();
            scroll.SetVSizeFlags(Control.SizeFlags.ExpandFill);
            _traitsTab.AddChild(scroll);
            
            var traitsList = new VBoxContainer();
            traitsList.SetAnchor(AnchorPreset.FullRect);
            scroll.AddChild(traitsList);
            
            // Group by type
            var types = new TraitType[] { TraitType.Battle, TraitType.Economic, TraitType.Exploration, TraitType.Social, TraitType.Special };
            string[] typeNames = { "⚔️ Battle Traits", "💰 Economic Traits", "📚 Exploration Traits", "🤝 Social Traits", "⭐ Special Traits" };
            
            for (int i = 0; i < types.Length; i++)
            {
                var typeLabel = new Label();
                typeLabel.Text = typeNames[i];
                typeLabel.AddThemeFontSizeOverride("font_size", 16);
                typeLabel.MarginTop = 10;
                traitsList.AddChild(typeLabel);
                
                var traits = PetTraitDatabase.GetTraitsByType(types[i]);
                foreach (var trait in traits)
                {
                    var traitPanel = CreateTraitPanel(trait, _traitSystem.IsTraitUnlocked(trait.Id));
                    traitsList.AddChild(traitPanel);
                }
            }
        }
        
        private void SetupStatisticsTab()
        {
            var statsBox = new VBoxContainer();
            _statisticsTab.AddChild(statsBox);
            
            var title = new Label();
            title.Text = "Trait Statistics";
            title.AddThemeFontSizeOverride("font_size", 18);
            statsBox.AddChild(title);
            
            var sep = new HSeparator();
            statsBox.AddChild(sep);
            
            // Total unlocked
            var totalUnlocked = new Label();
            totalUnlocked.Text = $"Total Traits Unlocked: {_traitSystem.GetUnlockedCount()} / {_traitSystem.GetTotalTraitCount()}";
            statsBox.AddChild(totalUnlocked);
            
            // Active count
            var activeCount = new Label();
            activeCount.Text = $"Active Traits: {_traitSystem.GetActiveCount()}";
            statsBox.AddChild(activeCount);
            
            // Rarity breakdown
            var rarityLabel = new Label();
            rarityLabel.Text = "\nRarity Breakdown:";
            rarityLabel.AddThemeFontSizeOverride("font_size", 14);
            statsBox.AddChild(rarityLabel);
            
            var rarities = new TraitRarity[] { TraitRarity.Common, TraitRarity.Uncommon, TraitRarity.Rare, TraitRarity.Epic, TraitRarity.Legendary };
            string[] rarityNames = { "Common", "Uncommon", "Rare", "Epic", "Legendary" };
            Color[] rarityColors = { _commonColor, _uncommonColor, _rareColor, _epicColor, _legendaryColor };
            
            for (int i = 0; i < rarities.Length; i++)
            {
                var count = PetTraitDatabase.GetTraitCountByRarity(rarities[i]);
                var rarityText = new Label();
                rarityText.Text = $"{rarityNames[i]}: {count}";
                rarityText.Modulate = rarityColors[i];
                statsBox.AddChild(rarityText);
            }
            
            // Reset button
            var resetBtn = new Button();
            resetBtn.Text = "Reset All Traits";
            resetBtn.Pressed += () =>
            {
                _traitSystem.ResetData();
                RefreshUI();
            };
            statsBox.AddChild(resetBtn);
        }
        
        private void AddBonusRow(GridContainer grid, string label, string value)
        {
            var labelControl = new Label();
            labelControl.Text = label + ":";
            grid.AddChild(labelControl);
            
            var valueControl = new Label();
            valueControl.Text = value;
            valueControl.HorizontalAlignment = HorizontalAlignment.Right;
            grid.AddChild(valueControl);
        }
        
        private Control CreateTraitPanel(PetTrait trait, bool isUnlocked)
        {
            var panel = new PanelContainer();
            panel.MarginBottom = 60;
            
            var hbox = new HBoxContainer();
            panel.AddChild(hbox);
            
            // Info section
            var infoVBox = new VBoxContainer();
            infoVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            hbox.AddChild(infoVBox);
            
            // Name and rarity
            var nameRow = new HBoxContainer();
            infoVBox.AddChild(nameRow);
            
            var nameLabel = new Label();
            nameLabel.Text = trait.Name;
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            nameLabel.Modulate = GetRarityColor(trait.Rarity);
            nameRow.AddChild(nameLabel);
            
            nameRow.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
            
            var rarityLabel = new Label();
            rarityLabel.Text = trait.Rarity.ToString();
            rarityLabel.Modulate = GetRarityColor(trait.Rarity);
            nameRow.AddChild(rarityLabel);
            
            // Description
            var descLabel = new Label();
            descLabel.Text = trait.Description;
            descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            infoVBox.AddChild(descLabel);
            
            // Requirements
            var reqLabel = new Label();
            reqLabel.Text = $"Min Level: {trait.MinPetLevel}";
            reqLabel.AddThemeFontSizeOverride("font_size", 12);
            infoVBox.AddChild(reqLabel);
            
            // Button section
            var btnVBox = new VBoxContainer();
            hbox.AddChild(btnVBox);
            
            if (!isUnlocked)
            {
                var unlockBtn = new Button();
                unlockBtn.Text = "Unlock";
                unlockBtn.Pressed += () =>
                {
                    // For demo, use level 50 and empty pet type
                    if (_traitSystem.UnlockTrait(trait.Id))
                    {
                        RefreshUI();
                    }
                };
                btnVBox.AddChild(unlockBtn);
            }
            else
            {
                var isActive = _traitSystem.IsTraitActive(trait.Id);
                var toggleBtn = new Button();
                toggleBtn.Text = isActive ? "Deactivate" : "Activate";
                toggleBtn.Pressed += () =>
                {
                    _traitSystem.ToggleTrait(trait.Id);
                    RefreshUI();
                };
                btnVBox.AddChild(toggleBtn);
            }
            
            return panel;
        }
        
        private Color GetRarityColor(TraitRarity rarity)
        {
            switch (rarity)
            {
                case TraitRarity.Common: return _commonColor;
                case TraitRarity.Uncommon: return _uncommonColor;
                case TraitRarity.Rare: return _rareColor;
                case TraitRarity.Epic: return _epicColor;
                case TraitRarity.Legendary: return _legendaryColor;
                default: return Colors.White;
            }
        }
        
        private void RefreshUI()
        {
            // Clear and rebuild
            foreach (var child in _overviewTab.GetChildren())
                child.QueueFree();
            foreach (var child in _traitsTab.GetChildren())
                child.QueueFree();
            foreach (var child in _statisticsTab.GetChildren())
                child.QueueFree();
                
            SetupOverviewTab();
            SetupTraitsTab();
            SetupStatisticsTab();
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel"))
            {
                Hide();
            }
        }
    }
}
