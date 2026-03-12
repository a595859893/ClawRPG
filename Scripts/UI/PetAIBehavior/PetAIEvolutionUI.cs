using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Database;
using ClawRPG.Scripts.Data;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// Pet AI Evolution UI - displays pet learning progress
    /// </summary>
    public partial class PetAIEvolutionUI : Control
    {
        private Control _mainContainer;
        private VBoxContainer _content;
        private Label _titleLabel;
        private PetAIEvolutionSystem _evolutionSystem;
        
        // Pet selection
        private OptionButton _petSelector;
        private string _selectedPetId = "";
        
        // Stats display
        private Label _battlesLabel;
        private Label _winRateLabel;
        private Label _damageDealtLabel;
        private Label _damageTakenLabel;
        private Label _comboLabel;
        
        // Evolution display
        private GridContainer _evolutionGrid;
        private Label _unlockedLabel;
        
        // Current bonuses
        private Label _activeBonusLabel;
        
        private bool _isVisible = false;

        public override void _Ready()
        {
            _evolutionSystem = PetAIEvolutionSystem.Instance;
            SetupUI();
            UpdatePetList();
            
            // Connect signals
            if (_evolutionSystem != null)
            {
                _evolutionSystem.EvolutionUnlocked += OnEvolutionUnlocked;
                _evolutionSystem.ProgressUpdated += OnProgressUpdated;
                _evolutionSystem.ComboUpdated += OnComboUpdated;
            }
        }

        private void SetupUI()
        {
            // Main container
            _mainContainer = new Control();
            _mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainContainer.CustomMinimumSize = new Vector2(600, 500);
            AddChild(_mainContainer);
            
            // Background panel
            var bg = new PanelContainer();
            bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            style.CornerRadiusTopLeft = 10;
            style.CornerRadiusTopRight = 10;
            style.CornerRadiusBottomLeft = 10;
            style.CornerRadiusBottomRight = 10;
            style.BorderWidthLeft = 2;
            style.BorderWidthTop = 2;
            style.BorderWidthRight = 2;
            style.BorderWidthBottom = 2;
            style.BorderColor = new Color(0.3f, 0.3f, 0.5f);
            bg.AddThemeStyleboxOverride("panel", style);
            _mainContainer.AddChild(bg);
            
            // Content
            _content = new VBoxContainer();
            _content.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _content.AddThemeConstantOverride("separation", 15);
            bg.AddChild(_content);
            
            // Title
            _titleLabel = new Label();
            _titleLabel.Text = "  🧬 宠物 AI 进化系统  ";
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _content.AddChild(_titleLabel);
            
            // Pet selector
            var selectorContainer = new HBoxContainer();
            selectorContainer.AddThemeConstantOverride("separation", 10);
            _content.AddChild(selectorContainer);
            
            var petLabel = new Label();
            petLabel.Text = "选择宠物:";
            selectorContainer.AddChild(petLabel);
            
            _petSelector = new OptionButton();
            _petSelector.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _petSelector.ItemSelected += OnPetSelected;
            selectorContainer.AddChild(_petSelector);
            
            // Stats section
            var statsContainer = new VBoxContainer();
            _content.AddChild(statsContainer);
            
            var statsTitle = new Label();
            statsTitle.Text = "=== 战斗统计 ===";
            statsTitle.HorizontalAlignment = HorizontalAlignment.Center;
            statsTitle.AddThemeFontSizeOverride("font_size", 18);
            statsContainer.AddChild(statsTitle);
            
            _battlesLabel = new Label();
            _battlesLabel.Text = "战斗次数: 0";
            statsContainer.AddChild(_battlesLabel);
            
            _winRateLabel = new Label();
            _winRateLabel.Text = "胜率: 0%";
            statsContainer.AddChild(_winRateLabel);
            
            _damageDealtLabel = new Label();
            _damageDealtLabel.Text = "造成伤害: 0";
            statsContainer.AddChild(_damageDealtLabel);
            
            _damageTakenLabel = new Label();
            _damageTakenLabel.Text = "受到伤害: 0";
            statsContainer.AddChild(_damageTakenLabel);
            
            _comboLabel = new Label();
            _comboLabel.Text = "最高连击: 0";
            statsContainer.AddChild(_comboLabel);
            
            // Unlocked evolutions
            var evolutionTitle = new Label();
            evolutionTitle.Text = "=== 已解锁进化 ===";
            evolutionTitle.HorizontalAlignment = HorizontalAlignment.Center;
            evolutionTitle.AddThemeFontSizeOverride("font_size", 18);
            _content.AddChild(evolutionTitle);
            
            _unlockedLabel = new Label();
            _unlockedLabel.Text = "暂无";
            _unlockedLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _content.AddChild(_unlockedLabel);
            
            // Evolution progress grid
            _evolutionGrid = new GridContainer();
            _evolutionGrid.Columns = 2;
            _evolutionGrid.AddThemeConstantOverride("h_separation", 20);
            _evolutionGrid.AddThemeConstantOverride("v_separation", 10);
            _content.AddChild(_evolutionGrid);
            
            // Active bonus
            var bonusTitle = new Label();
            bonusTitle.Text = "=== 当前激活 ===";
            bonusTitle.HorizontalAlignment = HorizontalAlignment.Center;
            bonusTitle.AddThemeFontSizeOverride("font_size", 18);
            _content.AddChild(bonusTitle);
            
            _activeBonusLabel = new Label();
            _activeBonusLabel.Text = "无激活效果";
            _activeBonusLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _content.AddChild(_activeBonusLabel);
            
            // Instructions
            var hint = new Label();
            hint.Text = "按 [V] 键切换显示 | 按 [ESC] 关闭";
            hint.HorizontalAlignment = HorizontalAlignment.Center;
            hint.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            _content.AddChild(hint);
            
            // Initial state
            _mainContainer.Visible = false;
        }

        private void UpdatePetList()
        {
            _petSelector.Clear();
            
            // Get list of pets with evolution data
            var petSystem = PetSystem.Instance;
            if (petSystem != null)
            {
                var pets = petSystem.GetAllPets();
                int index = 0;
                foreach (var pet in pets)
                {
                    _petSelector.AddItem(pet.PetId, index++);
                }
            }
            
            if (_petSelector.ItemCount > 0)
            {
                _petSelector.Selected = 0;
                _selectedPetId = _petSelector.GetItemText(0);
                UpdateDisplay();
            }
        }

        private void OnPetSelected(long index)
        {
            if (index >= 0 && index < _petSelector.ItemCount)
            {
                _selectedPetId = _petSelector.GetItemText((int)index);
                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            if (string.IsNullOrEmpty(_selectedPetId))
                return;
            
            _evolutionSystem.InitializePetEvolution(_selectedPetId);
            
            // Update stats
            var stats = _evolutionSystem.GetStatistics(_selectedPetId);
            
            _battlesLabel.Text = $"战斗次数: {stats.GetValueOrDefault("battles_fought", 0)}";
            
            float winRate = Convert.ToSingle(stats.GetValueOrDefault("win_rate", 0f));
            _winRateLabel.Text = $"胜率: {winRate * 100f:F1}%";
            
            _damageDealtLabel.Text = $"造成伤害: {stats.GetValueOrDefault("total_damage_dealt", 0)}";
            _damageTakenLabel.Text = $"受到伤害: {stats.GetValueOrDefault("total_damage_taken", 0)}";
            _comboLabel.Text = $"最高连击: {stats.GetValueOrDefault("highest_combo", 0)}";
            
            // Update unlocked evolutions
            var unlocked = _evolutionSystem.GetUnlockedEvolutions(_selectedPetId);
            if (unlocked.Count > 0)
            {
                var names = new List<string>();
                foreach (var type in unlocked)
                {
                    var bonus = PetAIEvolutionDatabase.GetEvolution(type);
                    if (bonus != null)
                        names.Add(bonus.EvolutionName);
                }
                _unlockedLabel.Text = string.Join(" / ", names);
            }
            else
            {
                _unlockedLabel.Text = "暂无 - 继续战斗解锁";
            }
            
            // Update progress grid
            UpdateProgressGrid();
        }

        private void UpdateProgressGrid()
        {
            // Clear existing
            foreach (var child in _evolutionGrid.GetChildren())
            {
                child.QueueFree();
            }
            
            // Add progress for each evolution type
            var allEvolutions = PetAIEvolutionDatabase.GetAllEvolutions();
            
            foreach (var evolution in allEvolutions)
            {
                var container = new VBoxContainer();
                
                var nameLabel = new Label();
                nameLabel.Text = evolution.EvolutionName;
                nameLabel.AddThemeFontSizeOverride("font_size", 14);
                container.AddChild(nameLabel);
                
                var descLabel = new Label();
                descLabel.Text = evolution.Description;
                descLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
                descLabel.AddThemeFontSizeOverride("font_size", 12);
                container.AddChild(descLabel);
                
                float progress = _evolutionSystem.GetEvolutionProgress(_selectedPetId, evolution.EvolutionType);
                var unlocked = _evolutionSystem.GetUnlockedEvolutions(_selectedPetId).Contains(evolution.EvolutionType);
                
                var progressLabel = new Label();
                if (unlocked)
                {
                    progressLabel.Text = "✓ 已解锁";
                    progressLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.9f, 0.3f));
                }
                else
                {
                    progressLabel.Text = $"进度: {progress:F0}/{PetAIEvolutionData.EvolutionThreshold}";
                }
                container.AddChild(progressLabel);
                
                _evolutionGrid.AddChild(container);
            }
        }

        private void OnEvolutionUnlocked(string petId, PetAIEvolutionType type)
        {
            if (petId == _selectedPetId)
            {
                var bonus = PetAIEvolutionDatabase.GetEvolution(type);
                if (bonus != null)
                {
                    _activeBonusLabel.Text = $"✨ {bonus.EvolutionName} 已解锁!";
                    _activeBonusLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.3f));
                }
                UpdateDisplay();
            }
        }

        private void OnProgressUpdated(string petId, float progress)
        {
            if (petId == _selectedPetId)
            {
                UpdateDisplay();
            }
        }

        private void OnComboUpdated(string petId, int combo)
        {
            if (petId == _selectedPetId)
            {
                _comboLabel.Text = $"当前连击: {combo}";
            }
        }

        public void Toggle()
        {
            _isVisible = !_isVisible;
            _mainContainer.Visible = _isVisible;
            
            if (_isVisible)
            {
                UpdatePetList();
            }
        }

        public override void _Input(InputEvent e)
        {
            if (e is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.V && !keyEvent.Echo)
                {
                    Toggle();
                }
            }
        }
    }
}
