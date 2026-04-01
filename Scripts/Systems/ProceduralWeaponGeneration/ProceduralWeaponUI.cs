using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ProceduralWeaponGeneration {
    /// <summary>
    /// UI for procedural weapon generation system
    /// </summary>
    public partial class ProceduralWeaponUI : Control {
        
        private ProceduralWeaponSystem _system;
        private ProceduralWeaponData _data;
        private ProceduralWeaponDatabase _database;
        
        // UI Elements
        private Button _generateButton;
        private Button _rerollButton;
        private Button _closeButton;
        private OptionButton _weaponTypeSelector;
        private OptionButton _raritySelector;
        
        private Label _weaponNameLabel;
        private Label _weaponTypeLabel;
        private Label _rarityLabel;
        private Label _attackLabel;
        private Label _defenseLabel;
        private Label _speedLabel;
        private Label _effectsLabel;
        private Label _costLabel;
        
        private Label _statsTotalLabel;
        private Label _statsLegendaryLabel;
        private Label _statsEpicLabel;
        private Label _statsRareLabel;
        private Label _statsGoldSpentLabel;
        
        private ItemList _historyList;
        private TabContainer _tabContainer;
        
        // Current weapon
        private WeaponGenerationRecord _currentWeapon;
        
        public void Initialize(ProceduralWeaponSystem system, ProceduralWeaponData data, ProceduralWeaponDatabase database) {
            _system = system;
            _data = data;
            _database = database;
            
            SetupUI();
            PopulateWeaponTypes();
            PopulateRarities();
            UpdateStatistics();
            
            GD.Print("[ProceduralWeaponUI] Initialized");
        }
        
        private void SetupUI() {
            // Create main container
            var mainContainer = new VBoxContainer();
            mainContainer.SetAnchor(AnchorPresets.FullRect);
            mainContainer.MarginLeft = 100;
            mainContainer.MarginTop = 50;
            mainContainer.MarginRight = -100;
            mainContainer.MarginBottom = -50;
            mainContainer.AddThemeConstantOverride("separation", 10);
            AddChild(mainContainer);
            
            // Title
            var titleLabel = new Label();
            titleLabel.Text = "⚔️ Procedural Weapon Generator ⚔️";
            titleLabel.Align = Label.AlignEnum.Center;
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            mainContainer.AddChild(titleLabel);
            
            // Tab container
            _tabContainer = new TabContainer();
            _tabContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            mainContainer.AddChild(_tabContainer);
            
            // Generator tab
            var generatorTab = new Control();
            generatorTab.Name = "Generator";
            _tabContainer.AddChild(generatorTab);
            SetupGeneratorTab(generatorTab);
            
            // History tab
            var historyTab = new Control();
            historyTab.Name = "History";
            _tabContainer.AddChild(historyTab);
            SetupHistoryTab(historyTab);
            
            // Statistics tab
            var statsTab = new Control();
            statsTab.Name = "Statistics";
            _tabContainer.AddChild(statsTab);
            SetupStatisticsTab(statsTab);
            
            // Close button
            _closeButton = new Button();
            _closeButton.Text = "Close (ESC)";
            _closeButton.Align = Button.AlignEnum.Center;
            _closeButton.Pressed += OnClosePressed;
            mainContainer.AddChild(_closeButton);
        }
        
        private void SetupGeneratorTab(Control tab) {
            var container = new VBoxContainer();
            container.SetAnchor(AnchorPresets.FullRect);
            container.MarginLeft = 20;
            container.MarginTop = 20;
            container.MarginRight = -20;
            container.MarginBottom = -20;
            container.AddThemeConstantOverride("separation", 15);
            tab.AddChild(container);
            
            // Weapon type selector
            var typeLabel = new Label();
            typeLabel.Text = "Weapon Type:";
            container.AddChild(typeLabel);
            
            _weaponTypeSelector = new OptionButton();
            _weaponTypeSelector.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _weaponTypeSelector.ItemSelected += OnWeaponTypeSelected;
            container.AddChild(_weaponTypeSelector);
            
            // Rarity selector
            var rarityLabel = new Label();
            rarityLabel.Text = "Force Rarity (Optional):";
            container.AddChild(rarityLabel);
            
            _raritySelector = new OptionButton();
            _raritySelector.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            container.AddChild(_raritySelector);
            
            // Generate button
            _generateButton = new Button();
            _generateButton.Text = "Generate Weapon (100g)";
            _generateButton.Align = Button.AlignEnum.Center;
            _generateButton.Pressed += OnGeneratePressed;
            container.AddChild(_generateButton);
            
            // Current weapon display
            var displayPanel = new PanelContainer();
            displayPanel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            container.AddChild(displayPanel);
            
            var displayVBox = new VBoxContainer();
            displayVBox.AddThemeConstantOverride("separation", 8);
            displayPanel.AddChild(displayVBox);
            
            // Weapon name
            _weaponNameLabel = new Label();
            _weaponNameLabel.Text = "No weapon generated yet";
            _weaponNameLabel.Align = Label.AlignEnum.Center;
            _weaponNameLabel.AddThemeFontSizeOverride("font_size", 20);
            displayVBox.AddChild(_weaponNameLabel);
            
            // Weapon type and rarity
            var typeRarityBox = new HBoxContainer();
            typeRarityBox.Align = BoxContainer.AlignMode.Center;
            typeRarityBox.AddThemeConstantOverride("separation", 20);
            displayVBox.AddChild(typeRarityBox);
            
            _weaponTypeLabel = new Label();
            _weaponTypeLabel.Text = "Type: -";
            typeRarityBox.AddChild(_weaponTypeLabel);
            
            _rarityLabel = new Label();
            _rarityLabel.Text = "Rarity: -";
            typeRarityBox.AddChild(_rarityLabel);
            
            // Stats
            var statsBox = new HBoxContainer();
            statsBox.Align = BoxContainer.AlignMode.Center;
            statsBox.AddThemeConstantOverride("separation", 30);
            displayVBox.AddChild(statsBox);
            
            _attackLabel = new Label();
            _attackLabel.Text = "ATK: 0";
            statsBox.AddChild(_attackLabel);
            
            _defenseLabel = new Label();
            _defenseLabel.Text = "DEF: 0";
            statsBox.AddChild(_defenseLabel);
            
            _speedLabel = new Label();
            _speedLabel.Text = "SPD: 0";
            statsBox.AddChild(_speedLabel);
            
            // Effects
            _effectsLabel = new Label();
            _effectsLabel.Text = "Effects: None";
            _effectsLabel.Align = Label.AlignEnum.Center;
            displayVBox.AddChild(_effectsLabel);
            
            // Reroll button
            _rerollButton = new Button();
            _rerollButton.Text = "Reroll (50g)";
            _rerollButton.Align = Button.AlignEnum.Center;
            _rerollButton.Pressed += OnRerollPressed;
            _rerollButton.Disabled = true;
            container.AddChild(_rerollButton);
            
            // Cost
            _costLabel = new Label();
            _costLabel.Text = "Cost: 100g";
            _costLabel.Align = Label.AlignEnum.Center;
            container.AddChild(_costLabel);
        }
        
        private void SetupHistoryTab(Control tab) {
            var container = new VBoxContainer();
            container.SetAnchor(AnchorPresets.FullRect);
            container.MarginLeft = 20;
            container.MarginTop = 20;
            container.MarginRight = -20;
            container.MarginBottom = -20;
            tab.AddChild(container);
            
            var historyLabel = new Label();
            historyLabel.Text = "Generation History (Last 50)";
            historyLabel.AddThemeFontSizeOverride("font_size", 18);
            container.AddChild(historyLabel);
            
            _historyList = new ItemList();
            _historyList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            container.AddChild(_historyList);
        }
        
        private void SetupStatisticsTab(Control tab) {
            var container = new VBoxContainer();
            container.SetAnchor(AnchorPresets.FullRect);
            container.MarginLeft = 20;
            container.MarginTop = 20;
            container.MarginRight = -20;
            container.MarginBottom = -20;
            container.AddThemeConstantOverride("separation", 10);
            tab.AddChild(container);
            
            var titleLabel = new Label();
            titleLabel.Text = "Generation Statistics";
            titleLabel.AddThemeFontSizeOverride("font_size", 20);
            container.AddChild(titleLabel);
            
            _statsTotalLabel = new Label();
            _statsTotalLabel.Text = "Total Generated: 0";
            container.AddChild(_statsTotalLabel);
            
            _statsLegendaryLabel = new Label();
            _statsLegendaryLabel.Text = "Legendary: 0";
            container.AddChild(_statsLegendaryLabel);
            
            _statsEpicLabel = new Label();
            _statsEpicLabel.Text = "Epic: 0";
            container.AddChild(_statsEpicLabel);
            
            _statsRareLabel = new Label();
            _statsRareLabel.Text = "Rare: 0";
            container.AddChild(_statsRareLabel);
            
            _statsGoldSpentLabel = new Label();
            _statsGoldSpentLabel.Text = "Total Gold Spent: 0";
            container.AddChild(_statsGoldSpentLabel);
        }
        
        private void PopulateWeaponTypes() {
            _weaponTypeSelector.Clear();
            _weaponTypeSelector.AddItem("Random", 0);
            
            var types = _system.GetAvailableWeaponTypes();
            for (int i = 0; i < types.Count; i++) {
                _weaponTypeSelector.AddItem(types[i], i + 1);
            }
        }
        
        private void PopulateRarities() {
            _raritySelector.Clear();
            _raritySelector.AddItem("Random", 0);
            _raritySelector.AddItem("Common", 1);
            _raritySelector.AddItem("Uncommon", 2);
            _raritySelector.AddItem("Rare", 3);
            _raritySelector.AddItem("Epic", 4);
            _raritySelector.AddItem("Legendary", 5);
        }
        
        private void OnWeaponTypeSelected(long index) {
            // Update cost based on selection
            UpdateCostLabel();
        }
        
        private void OnGeneratePressed() {
            string weaponType = "";
            string rarity = "";
            
            // Get selected weapon type
            if (_weaponTypeSelector.Selected > 0) {
                weaponType = _weaponTypeSelector.GetItemText(_weaponTypeSelector.Selected);
            }
            
            // Get selected rarity
            if (_raritySelector.Selected > 0) {
                rarity = _raritySelector.GetItemText(_raritySelector.Selected);
            }
            
            // Generate weapon
            _currentWeapon = _system.GenerateWeapon(weaponType, rarity);
            
            // Update display
            UpdateWeaponDisplay();
            UpdateHistory();
            UpdateStatistics();
            
            // Enable reroll
            _rerollButton.Disabled = false;
            UpdateRerollButton();
        }
        
        private void OnRerollPressed() {
            if (_currentWeapon == null) return;
            
            _currentWeapon = _system.RerollWeapon(_currentWeapon);
            UpdateWeaponDisplay();
            UpdateHistory();
            UpdateStatistics();
        }
        
        private void OnClosePressed() {
            Visible = false;
        }
        
        private void UpdateWeaponDisplay() {
            if (_currentWeapon == null) return;
            
            _weaponNameLabel.Text = _currentWeapon.WeaponName;
            _weaponTypeLabel.Text = "Type: " + _currentWeapon.WeaponType;
            _rarityLabel.Text = "Rarity: " + _currentWeapon.Rarity;
            
            // Color based on rarity
            var rarityConfig = _system.GetRarityConfig(_currentWeapon.Rarity);
            if (rarityConfig != null) {
                Color rarityColor = new Color(rarityConfig.Color);
                _rarityLabel.Modulate = rarityColor;
            }
            
            _attackLabel.Text = "ATK: " + _currentWeapon.Attack;
            _defenseLabel.Text = "DEF: " + _currentWeapon.Defense;
            _speedLabel.Text = "SPD: " + _currentWeapon.Speed;
            
            if (_currentWeapon.SpecialEffects.Count > 0) {
                _effectsLabel.Text = "Effects: " + string.Join(", ", _currentWeapon.SpecialEffects);
            } else {
                _effectsLabel.Text = "Effects: None";
            }
        }
        
        private void UpdateHistory() {
            _historyList.Clear();
            
            if (_data.GenerationHistory == null) return;
            
            for (int i = 0; i < Math.Min(_data.GenerationHistory.Count, 50); i++) {
                var record = _data.GenerationHistory[i];
                string displayText = $"{record.Rarity} {record.WeaponName}";
                _historyList.AddItem(displayText);
            }
        }
        
        private void UpdateStatistics() {
            var stats = _system.GetStatistics();
            
            _statsTotalLabel.Text = $"Total Generated: {stats["TotalWeaponsGenerated"]}";
            _statsLegendaryLabel.Text = $"Legendary: {stats["LegendaryWeapons"]}";
            _statsEpicLabel.Text = $"Epic: {stats["EpicWeapons"]}";
            _statsRareLabel.Text = $"Rare: {stats["RareWeapons"]}";
            _statsGoldSpentLabel.Text = $"Total Gold Spent: {stats["TotalGoldSpent"]}";
        }
        
        private void UpdateCostLabel() {
            string rarity = "";
            if (_raritySelector.Selected > 0) {
                rarity = _raritySelector.GetItemText(_raritySelector.Selected);
            }
            
            int cost = _system.GetGenerationCost(rarity);
            _costLabel.Text = $"Cost: {cost}g";
            _generateButton.Text = $"Generate Weapon ({cost}g)";
        }
        
        private void UpdateRerollButton() {
            if (_currentWeapon == null) return;
            
            int cost = _system.GetRerollCost(_currentWeapon);
            _rerollButton.Text = $"Reroll ({cost}g)";
        }
        
        public override void _Input(InputEvent @event) {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
                if (keyEvent.Keycode == Key.Escape) {
                    Visible = false;
                }
            }
        }
    }
}
