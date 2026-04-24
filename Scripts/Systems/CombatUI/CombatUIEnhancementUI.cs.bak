using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Systems.CombatUI;

namespace ClawRPG.Systems
{
    /// <summary>
    /// Combat UI Enhancement UI - 战斗UI增强系统控制面板
    /// 显示动态血条、技能冷却、战斗状态、连击计数
    /// </summary>
    public partial class CombatUIEnhancementUI : Control
    {
        private CombatUIEnhancementSystem _system;
        
        // UI Components
        private VBoxContainer _mainContainer;
        private CheckBox _enabledCheckBox;
        private Label _titleLabel;
        
        // Health Bar Section
        private HBoxContainer _healthBarContainer;
        private ProgressBar _healthBar;
        private Label _healthLabel;
        
        // Combat State Section
        private Label _combatStateLabel;
        private Label _comboLabel;
        
        // Skill Cooldowns Section
        private GridContainer _skillCooldownGrid;
        private List<TextureRect> _skillIcons = new List<TextureRect>();
        
        // Status Effects Section
        private FlowContainer _statusEffectContainer;
        
        // Statistics Section
        private Label _statsLabel;
        
        private bool _isVisible = false;
        
        public override void _Ready()
        {
            _system = GetNode<CombatUIEnhancementSystem>("/root/Main/CombatUIEnhancementSystem");
            
            _CreateUI();
            
            // Connect input
            VisibilityChanged += _OnVisibilityChanged;
            
            GD.Print("[CombatUIEnhancementUI] Initialized - Press Ctrl+Shift+U to toggle");
        }
        
        private void _CreateUI()
        {
            // Main container
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorsPreset(Control.LayoutPreset.TopRight);
            _mainContainer.Position = new Vector2(50, 50);
            _mainContainer.CustomMinimumSize = new Vector2(300, 400);
            AddChild(_mainContainer);
            
            // Title
            _titleLabel = new Label();
            _titleLabel.Text = "⚔️ Combat UI Enhancement";
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _mainContainer.AddChild(_titleLabel);
            
            // Enabled checkbox
            _enabledCheckBox = new CheckBox();
            _enabledCheckBox.Text = "Enabled";
            _enabledCheckBox.Pressed += () => _system.SetEnabled(true);
            _enabledCheckBox.Toggled += (pressed) => _system.SetEnabled(pressed);
            _mainContainer.AddChild(_enabledCheckBox);
            
            // Separator
            _mainContainer.AddChild(_CreateHSeparator());
            
            // Health Bar Section
            var healthSectionLabel = new Label();
            healthSectionLabel.Text = "❤️ Health";
            _mainContainer.AddChild(healthSectionLabel);
            
            _healthBarContainer = new HBoxContainer();
            _mainContainer.AddChild(_healthBarContainer);
            
            _healthBar = new ProgressBar();
            _healthBar.CustomMinimumSize = new Vector2(200, 20);
            _healthBar.Value = 100;
            _healthBar.MaxValue = 100;
            _healthBarContainer.AddChild(_healthBar);
            
            _healthLabel = new Label();
            _healthLabel.Text = "100/100";
            _healthLabel.CustomMinimumSize = new Vector2(80, 0);
            _healthBarContainer.AddChild(_healthLabel);
            
            // Separator
            _mainContainer.AddChild(_CreateHSeparator());
            
            // Combat State
            var stateLabel = new Label();
            stateLabel.Text = "🎯 Combat State";
            _mainContainer.AddChild(stateLabel);
            
            _combatStateLabel = new Label();
            _combatStateLabel.Text = "Idle";
            _mainContainer.AddChild(_combatStateLabel);
            
            _comboLabel = new Label();
            _comboLabel.Text = "Combo: 0";
            _comboLabel.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0f));
            _mainContainer.AddChild(_comboLabel);
            
            // Separator
            _mainContainer.AddChild(_CreateHSeparator());
            
            // Skill Cooldowns
            var skillLabel = new Label();
            skillLabel.Text = "⚡ Skill Cooldowns";
            _mainContainer.AddChild(skillLabel);
            
            _skillCooldownGrid = new GridContainer();
            _skillCooldownGrid.Columns = 4;
            _skillCooldownGrid.CustomMinimumSize = new Vector2(280, 100);
            _mainContainer.AddChild(_skillCooldownGrid);
            
            // Initialize skill cooldown slots
            for (int i = 0; i < 8; i++)
            {
                var skillIcon = new TextureRect();
                skillIcon.CustomMinimumSize = new Vector2(60, 60);
                skillIcon.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                _skillCooldownGrid.AddChild(skillIcon);
                _skillIcons.AddChild(skillIcon);
            }
            
            // Separator
            _mainContainer.AddChild(_CreateHSeparator());
            
            // Status Effects
            var statusLabel = new Label();
            statusLabel.Text = "✨ Status Effects";
            _mainContainer.AddChild(statusLabel);
            
            _statusEffectContainer = new FlowContainer();
            _statusEffectContainer.CustomMinimumSize = new Vector2(280, 60);
            _mainContainer.AddChild(_statusEffectContainer);
            
            // Separator
            _mainContainer.AddChild(_CreateHSeparator());
            
            // Statistics
            var statsHeaderLabel = new Label();
            statsHeaderLabel.Text = "📊 Statistics";
            _mainContainer.AddChild(statsHeaderLabel);
            
            _statsLabel = new Label();
            _statsLabel.Text = "Loading...";
            _mainContainer.AddChild(_statsLabel);
            
            // Separator
            _mainContainer.AddChild(_CreateHSeparator());
            
            // Close hint
            var closeHint = new Label();
            closeHint.Text = "Press ESC to close";
            closeHint.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            _mainContainer.AddChild(closeHint);
            
            // Initial state
            Hide();
        }
        
        private HSeparator _CreateHSeparator()
        {
            var sep = new HSeparator();
            sep.CustomMinimumSize = new Vector2(0, 5);
            return sep;
        }
        
        public override void _Process(double delta)
        {
            if (!_isVisible || _system == null)
                return;
            
            // Update health bar
            float healthPercent = _system.GetDisplayedHealthPercent() * 100;
            _healthBar.Value = healthPercent;
            _healthLabel.Text = $"{healthPercent:F0}%";
            
            // Health bar color based on percentage
            Color healthColor = _system.GetHealthBarColor();
            _healthBar.Modulate = healthColor;
            
            // Update combat state
            var state = _system.GetCurrentState();
            _combatStateLabel.Text = state.ToString();
            
            // Update combo
            int combo = _system.GetCurrentCombo();
            _comboLabel.Text = $"⚔️ Combo: {combo}";
            
            // Combo color scaling
            if (combo > 10)
                _comboLabel.AddThemeColorOverride("font_color", new Color(1f, 0.3f, 0f));
            else if (combo > 5)
                _comboLabel.AddThemeColorOverride("font_color", new Color(1f, 0.6f, 0f));
            else
                _comboLabel.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0f));
            
            // Update skill cooldowns
            var cooldowns = _system.GetAllCooldowns();
            for (int i = 0; i < _skillIcons.Count; i++)
            {
                if (i < cooldowns.Count)
                {
                    float cooldownPercent = cooldowns[i].GetCooldownPercent();
                    _skillIcons[i].Modulate = new Color(
                        1f - cooldownPercent,
                        cooldownPercent,
                        0f,
                        1f
                    );
                }
            }
            
            // Update status effects
            _UpdateStatusEffects();
            
            // Update statistics
            _UpdateStatistics();
        }
        
        private void _UpdateStatusEffects()
        {
            // Clear existing effect icons
            foreach (var child in _statusEffectContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            var effects = _system.GetActiveEffects();
            foreach (var effect in effects)
            {
                var effectContainer = new VBoxContainer();
                effectContainer.CustomMinimumSize = new Vector2(50, 50);
                
                var effectIcon = new TextureRect();
                effectIcon.CustomMinimumSize = new Vector2(40, 40);
                effectIcon.Modulate = effect.EffectColor;
                effectContainer.AddChild(effectIcon);
                
                var effectLabel = new Label();
                effectLabel.Text = effect.DisplayName;
                effectLabel.HorizontalAlignment = HorizontalAlignment.Center;
                effectLabel.AddThemeColorOverride("font_color", effect.EffectColor);
                effectContainer.AddChild(effectLabel);
                
                var progressBar = new ProgressBar();
                progressBar.CustomMinimumSize = new Vector2(40, 5);
                progressBar.Value = effect.GetRemainingPercent() * 100;
                progressBar.MaxValue = 100;
                progressBar.Modulate = effect.EffectColor;
                effectContainer.AddChild(progressBar);
                
                _statusEffectContainer.AddChild(effectContainer);
            }
        }
        
        private void _UpdateStatistics()
        {
            var stats = _system.GetStatistics();
            _statsLabel.Text = $"Combos: {stats["total_combos"]}\n" +
                              $"Best Combo: {stats["highest_combo"]}\n" +
                              $"Criticals: {stats["total_criticals"]}\n" +
                              $"Damage Mitigated: {stats["damage_mitigated"]:F0}";
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent)
            {
                // Toggle visibility with Ctrl+Shift+U
                if (keyEvent.Pressed && keyEvent.Control && keyEvent.Shift && keyEvent.Scancode == Godot.KeyList.U)
                {
                    ToggleVisibility();
                }
                
                // Close with Escape
                if (keyEvent.Pressed && keyEvent.Scancode == Godot.KeyList.Escape && _isVisible)
                {
                    Hide();
                }
            }
        }
        
        public void ToggleVisibility()
        {
            if (_isVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
        
        private void _OnVisibilityChanged()
        {
            _isVisible = Visible;
        }
        
        // Public methods for external control
        public void ShowPanel()
        {
            Show();
        }
        
        public void HidePanel()
        {
            Hide();
        }
    }
}
