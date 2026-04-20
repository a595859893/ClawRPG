using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// UI for procedural name generation system
    /// </summary>
    public partial class ProceduralNameUI : Control {
        private ProceduralNameSystem _system;
        private ProceduralNameDatabase _database;
        
        // UI Elements
        private Label _titleLabel;
        private OptionButton _typeOption;
        private OptionButton _rarityOption;
        private OptionButton _styleOption;
        private Label _resultLabel;
        private Button _generateButton;
        private Button _generateMultipleButton;
        private LineEdit _seedInput;
        private Label _historyLabel;
        private Label _statsLabel;
        
        private TabContainer _tabContainer;
        
        // Current selections
        private string _selectedType = "";
        private string _selectedRarity = "";
        private string _selectedStyle = "";
        
        public ProceduralNameUI() {
            _system = new ProceduralNameSystem();
            _database = new ProceduralNameDatabase();
        }
        
        public override void _Ready() {
            SetupUI();
            UpdateOptions();
        }
        
        private void SetupUI() {
            // Main container
            var mainContainer = new VBoxContainer();
            mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
            mainContainer.CustomMinimumSize = new Vector2(600, 500);
            AddChild(mainContainer);
            
            // Title
            _titleLabel = new Label();
            _titleLabel.Text = "Procedural Item Name Generator";
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            mainContainer.AddChild(_titleLabel);
            
            mainContainer.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });
            
            // Tab container
            _tabContainer = new TabContainer();
            _tabContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            mainContainer.AddChild(_tabContainer);
            
            // Generator tab
            var generatorTab = new VBoxContainer();
            generatorTab.Name = "Generator";
            _tabContainer.AddChild(generatorTab);
            
            // Type selector
            var typeHBox = new HBoxContainer();
            var typeLabel = new Label();
            typeLabel.Text = "Item Type:";
            typeLabel.CustomMinimumSize = new Vector2(100, 0);
            typeHBox.AddChild(typeLabel);
            
            _typeOption = new OptionButton();
            _typeOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _typeOption.ItemSelected += (index) => {
                _selectedType = _typeOption.GetItemText((int)index);
            };
            typeHBox.AddChild(_typeOption);
            generatorTab.AddChild(typeHBox);
            
            // Rarity selector
            var rarityHBox = new HBoxContainer();
            var rarityLabel = new Label();
            rarityLabel.Text = "Rarity:";
            rarityLabel.CustomMinimumSize = new Vector2(100, 0);
            rarityHBox.AddChild(rarityLabel);
            
            _rarityOption = new OptionButton();
            _rarityOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _rarityOption.ItemSelected += (index) => {
                _selectedRarity = _rarityOption.GetItemText((int)index);
            };
            rarityHBox.AddChild(_rarityOption);
            generatorTab.AddChild(rarityHBox);
            
            // Style selector
            var styleHBox = new HBoxContainer();
            var styleLabel = new Label();
            styleLabel.Text = "Style:";
            styleLabel.CustomMinimumSize = new Vector2(100, 0);
            styleHBox.AddChild(styleLabel);
            
            _styleOption = new OptionButton();
            _styleOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _styleOption.ItemSelected += (index) => {
                _selectedStyle = _styleOption.GetItemText((int)index);
            };
            styleHBox.AddChild(_styleOption);
            generatorTab.AddChild(styleHBox);
            
            // Seed input
            var seedHBox = new HBoxContainer();
            var seedLabel = new Label();
            seedLabel.Text = "Seed (optional):";
            seedLabel.CustomMinimumSize = new Vector2(100, 0);
            seedHBox.AddChild(seedLabel);
            
            _seedInput = new LineEdit();
            _seedInput.PlaceholderText = "Enter seed for reproducible names";
            _seedInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            seedHBox.AddChild(_seedInput);
            generatorTab.AddChild(seedHBox);
            
            generatorTab.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });
            
            // Generate buttons
            var buttonHBox = new HBoxContainer();
            
            _generateButton = new Button();
            _generateButton.Text = "Generate Name";
            _generateButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _generateButton.Pressed += OnGeneratePressed;
            buttonHBox.AddChild(_generateButton);
            
            _generateMultipleButton = new Button();
            _generateMultipleButton.Text = "Generate 5";
            _generateMultipleButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _generateMultipleButton.Pressed += OnGenerateMultiplePressed;
            buttonHBox.AddChild(_generateMultipleButton);
            
            generatorTab.AddChild(buttonHBox);
            
            // Result display
            _resultLabel = new Label();
            _resultLabel.Text = "Click Generate to create a name";
            _resultLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _resultLabel.AddThemeFontSizeOverride("font_size", 20);
            _resultLabel.CustomMinimumSize = new Vector2(0, 60);
            generatorTab.AddChild(_resultLabel);
            
            generatorTab.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });
            
            // History tab
            var historyTab = new VBoxContainer();
            historyTab.Name = "History";
            _tabContainer.AddChild(historyTab);
            
            _historyLabel = new Label();
            _historyLabel.Text = "No names generated yet";
            _historyLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            historyTab.AddChild(_historyLabel);
            
            // Statistics tab
            var statsTab = new VBoxContainer();
            statsTab.Name = "Statistics";
            _tabContainer.AddChild(statsTab);
            
            _statsLabel = new Label();
            _statsLabel.Text = "No statistics available";
            _statsLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            statsTab.AddChild(_statsLabel);
            
            // Close button
            var closeButton = new Button();
            closeButton.Text = "Close (ESC)";
            closeButton.Pressed += () => Visible = false;
            mainContainer.AddChild(closeButton);
        }
        
        private void UpdateOptions() {
            // Update type options
            _typeOption.Clear();
            _typeOption.AddItem("Random");
            foreach (var type in ProceduralNameDatabase.GetAllTypes()) {
                _typeOption.AddItem(type);
            }
            _typeOption.Select(0);
            
            // Update rarity options
            _rarityOption.Clear();
            _rarityOption.AddItem("Random");
            foreach (var rarity in ProceduralNameDatabase.GetAllRarities()) {
                _rarityOption.AddItem(rarity);
            }
            _rarityOption.Select(0);
            
            // Update style options
            _styleOption.Clear();
            _styleOption.AddItem("Random");
            foreach (var style in ProceduralNameDatabase.GetAllStyles()) {
                _styleOption.AddItem(style);
            }
            _styleOption.Select(0);
        }
        
        private void OnGeneratePressed() {
            int? seed = null;
            if (!string.IsNullOrEmpty(_seedInput.Text)) {
                if (int.TryParse(_seedInput.Text, out int seedValue)) {
                    seed = seedValue;
                }
            }
            
            string type = _selectedType == "Random" ? "" : _selectedType;
            string rarity = _selectedRarity == "Random" ? "" : _selectedRarity;
            string style = _selectedStyle == "Random" ? "" : _selectedStyle;
            
            string name = _system.GenerateName(type, rarity, style, seed);
            _resultLabel.Text = name;
            
            // Update rarity color
            if (_rarityOption.Selected > 0) {
                string rarityText = _rarityOption.GetItemText(_rarityOption.Selected);
                if (ProceduralNameDatabase.RarityColors.ContainsKey(rarityText)) {
                    _resultLabel.Modulate = ProceduralNameDatabase.RarityColors[rarityText];
                }
            } else {
                _resultLabel.Modulate = Colors.White;
            }
            
            UpdateHistoryDisplay();
            UpdateStatsDisplay();
        }
        
        private void OnGenerateMultiplePressed() {
            int? seed = null;
            if (!string.IsNullOrEmpty(_seedInput.Text)) {
                if (int.TryParse(_seedInput.Text, out int seedValue)) {
                    seed = seedValue;
                }
            }
            
            string type = _selectedType == "Random" ? "" : _selectedType;
            string rarity = _selectedRarity == "Random" ? "" : _selectedRarity;
            string style = _selectedStyle == "Random" ? "" : _selectedStyle;
            
            var names = _system.GenerateMultiple(5, type, rarity, style, seed);
            _resultLabel.Text = string.Join("\n", names);
            _resultLabel.Modulate = Colors.White;
            
            UpdateHistoryDisplay();
            UpdateStatsDisplay();
        }
        
        private void UpdateHistoryDisplay() {
            var history = _system.GetHistory();
            if (history.Count == 0) {
                _historyLabel.Text = "No names generated yet";
                return;
            }
            
            var text = "";
            foreach (var record in history) {
                text += $"{record.Name} ({record.Rarity} {record.Type})\n";
            }
            _historyLabel.Text = text;
        }
        
        private void UpdateStatsDisplay() {
            var stats = _system.GetStatistics();
            var data = _system.GetData();
            
            var text = $"Total Names Generated: {stats["TotalGenerated"]}\n\n";
            text += "Style Distribution:\n";
            text += $"  Fantasy: {stats["FantasyStyle"]}\n";
            text += $"  Modern: {stats["ModernStyle"]}\n";
            text += $"  Mythical: {stats["MythicalStyle"]}\n";
            text += $"  Ancient: {stats["AncientStyle"]}\n\n";
            
            text += "Rarity Distribution:\n";
            foreach (var kvp in data.RarityUsageCount) {
                text += $"  {kvp.Key}: {kvp.Value}\n";
            }
            
            text += "\nType Distribution:\n";
            foreach (var kvp in data.TypeUsageCount) {
                text += $"  {kvp.Key}: {kvp.Value}\n";
            }
            
            _statsLabel.Text = text;
        }
        
        public void SetSystem(ProceduralNameSystem system) {
            _system = system;
            UpdateHistoryDisplay();
            UpdateStatsDisplay();
        }
        
        public override void _Input(InputEvent e) {
            if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape) {
                Visible = false;
            }
        }
    }
}
