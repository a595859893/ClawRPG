using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Combat;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// Combat Log UI - Godot UI display for combat log
    /// </summary>
    public class CombatLogUI : Control
    {
        private static CombatLogUI _instance;
        public static CombatLogUI Instance => _instance;
        
        // Main containers
        private PanelContainer _mainPanel;
        private VBoxContainer _mainContainer;
        
        // Header
        private HBoxContainer _header;
        private Label _titleLabel;
        private Button _clearButton;
        private Button _filterButton;
        private Button _settingsButton;
        
        // Log scroll container
        private ScrollContainer _scrollContainer;
        private VBoxContainer _logContainer;
        
        // Filter panel (hidden by default)
        private PanelContainer _filterPanel;
        private VBoxContainer _filterContainer;
        private CheckBox _damageCheck;
        private CheckBox _healingCheck;
        private CheckBox _buffsCheck;
        private CheckBox _skillsCheck;
        private CheckBox _combatCheck;
        private CheckBox _infoCheck;
        private CheckBox _playerOnlyCheck;
        private CheckBox _enemyOnlyCheck;
        
        // Settings panel (hidden by default)
        private PanelContainer _settingsPanel;
        private VBoxContainer _settingsContainer;
        private CheckBox _autoScrollCheck;
        private CheckBox _timestampCheck;
        private CheckBox _coloredTextCheck;
        private CheckBox _soundEffectsCheck;
        private SpinBox _maxEntriesSpin;
        private SpinBox _fontSizeSpin;
        private HSlider _opacitySlider;
        
        // Statistics panel
        private PanelContainer _statsPanel;
        private VBoxContainer _statsContainer;
        private Label _sessionTimeLabel;
        private Label _damageDealtLabel;
        private Label _damageTakenLabel;
        private Label _healingLabel;
        private Label _killsLabel;
        private Label _comboLabel;
        
        // State
        private bool _isVisible = false;
        private bool _filterOpen = false;
        private bool _settingsOpen = false;
        private bool _statsOpen = true;
        private bool _autoScroll = true;
        private bool _showTimestamp = true;
        private bool _coloredText = true;
        
        // Styling
        private Color _playerColor = new Color(0.4f, 0.8f, 1f);
        private Color _enemyColor = new Color(1f, 0.4f, 0.4f);
        private Color _healColor = new Color(0.4f, 1f, 0.4f);
        private Color _skillColor = new Color(1f, 0.8f, 0.2f);
        private Color _infoColor = new Color(0.8f, 0.8f, 0.8f);
        private Color _warningColor = new Color(1f, 0.6f, 0f);
        private Color _criticalColor = new Color(1f, 0.2f, 0.2f);
        
        // Entry limit
        private int _maxDisplayEntries = 100;
        
        public override void _Ready()
        {
            _instance = this;
            
            SetupUI();
            ConnectSignals();
            
            // Connect to combat log system
            if (CombatLogSystem.Instance != null)
            {
                CombatLogSystem.Instance.SignalNewEntry += OnNewLogEntry;
                CombatLogSystem.Instance.SignalComboMilestone += OnComboMilestone;
            }
            
            GD.Print("[CombatLogUI] Combat Log UI initialized");
        }
        
        private void SetupUI()
        {
            // Main panel
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.TopRight);
            _mainPanel.Position = new Vector2(850, 100);
            _mainPanel.CustomMinimumSize = new Vector2(400, 500);
            _mainPanel.Visible = _isVisible;
            AddChild(_mainPanel);
            
            // Main container
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _mainContainer.AddThemeConstantOverride("separation", 5);
            _mainPanel.AddChild(_mainContainer);
            
            // Setup sections
            SetupHeader();
            SetupLogContainer();
            SetupFilterPanel();
            SetupSettingsPanel();
            SetupStatsPanel();
            
            // Initially hide panels
            _filterPanel.Visible = false;
            _settingsPanel.Visible = false;
        }
        
        private void SetupHeader()
        {
            _header = new HBoxContainer();
            _header.Alignment = HBoxContainer.AlignmentMode.Center;
            _header.CustomMinimumSize = new Vector2(0, 40);
            _mainContainer.AddChild(_header);
            
            // Title
            _titleLabel = new Label();
            _titleLabel.Text = "⚔️ Combat Log";
            _titleLabel.AddThemeFontSizeOverride("font_size", 18);
            _header.AddChild(_titleLabel);
            
            // Spacer
            var spacer = new Control();
            spacer.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            _header.AddChild(spacer);
            
            // Stats button
            var statsButton = new Button();
            statsButton.Text = "📊";
            statsButton.TooltipText = "Toggle Statistics";
            statsButton.Pressed += () => ToggleStats();
            _header.AddChild(statsButton);
            
            // Filter button
            _filterButton = new Button();
            _filterButton.Text = "🔍";
            _filterButton.TooltipText = "Toggle Filters";
            _filterButton.Pressed += () => ToggleFilter();
            _header.AddChild(_filterButton);
            
            // Settings button
            _settingsButton = new Button();
            _settingsButton.Text = "⚙️";
            _settingsButton.TooltipText = "Settings";
            _settingsButton.Pressed += () => ToggleSettings();
            _header.AddChild(_settingsButton);
            
            // Clear button
            _clearButton = new Button();
            _clearButton.Text = "🗑️";
            _clearButton.TooltipText = "Clear Log";
            _clearButton.Pressed += () => ClearLog();
            _header.AddChild(_clearButton);
            
            // Toggle button
            var toggleButton = new Button();
            toggleButton.Text = _isVisible ? "▼" : "▲";
            toggleButton.TooltipText = _isVisible ? "Minimize" : "Expand";
            toggleButton.Pressed += () => ToggleVisibility();
            _header.AddChild(toggleButton);
        }
        
        private void SetupLogContainer()
        {
            _scrollContainer = new ScrollContainer();
            _scrollContainer.SetHScrollEnabled(false);
            _scrollContainer.SetVScrollEnabled(true);
            _scrollContainer.CustomMinimumSize = new Vector2(380, 300);
            _mainContainer.AddChild(_scrollContainer);
            
            _logContainer = new VBoxContainer();
            _logContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _logContainer.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            _logContainer.AddThemeConstantOverride("separation", 2);
            _scrollContainer.AddChild(_logContainer);
        }
        
        private void SetupFilterPanel()
        {
            _filterPanel = new PanelContainer();
            _filterPanel.CustomMinimumSize = new Vector2(380, 200);
            _mainContainer.AddChild(_filterPanel);
            
            _filterContainer = new VBoxContainer();
            _filterContainer.AddThemeConstantOverride("separation", 5);
            _filterPanel.AddChild(_filterContainer);
            
            var filterTitle = new Label();
            filterTitle.Text = "Filters";
            filterTitle.AddThemeFontSizeOverride("font_size", 14);
            _filterContainer.AddChild(filterTitle);
            
            // Damage filter
            _damageCheck = new CheckBox();
            _damageCheck.Text = "Show Damage";
            _damageCheck.ButtonPressed = true;
            _damageCheck.Toggled += (pressed) => {
                if (CombatLogSystem.Instance != null)
                    CombatLogSystem.Instance.SetShowDamage(pressed);
            };
            _filterContainer.AddChild(_damageCheck);
            
            // Healing filter
            _healingCheck = new CheckBox();
            _healingCheck.Text = "Show Healing";
            _healingCheck.ButtonPressed = true;
            _healingCheck.Toggled += (pressed) => {
                if (CombatLogSystem.Instance != null)
                    CombatLogSystem.Instance.SetShowHealing(pressed);
            };
            _filterContainer.AddChild(_healingCheck);
            
            // Buffs filter
            _buffsCheck = new CheckBox();
            _buffsCheck.Text = "Show Buffs/Debuffs";
            _buffsCheck.ButtonPressed = true;
            _buffsCheck.Toggled += (pressed) => {
                if (CombatLogSystem.Instance != null)
                    CombatLogSystem.Instance.SetShowBuffs(pressed);
            };
            _filterContainer.AddChild(_buffsCheck);
            
            // Skills filter
            _skillsCheck = new CheckBox();
            _skillsCheck.Text = "Show Skills/Items";
            _skillsCheck.ButtonPressed = true;
            _skillsCheck.Toggled += (pressed) => {
                if (CombatLogSystem.Instance != null)
                    CombatLogSystem.Instance.SetShowSkills(pressed);
            };
            _filterContainer.AddChild(_skillsCheck);
            
            // Combat filter
            _combatCheck = new CheckBox();
            _combatCheck.Text = "Show Combat Events";
            _combatCheck.ButtonPressed = true;
            _combatCheck.Toggled += (pressed) => {
                if (CombatLogSystem.Instance != null)
                    CombatLogSystem.Instance.SetShowCombat(pressed);
            };
            _filterContainer.AddChild(_combatCheck);
            
            // Info filter
            _infoCheck = new CheckBox();
            _infoCheck.Text = "Show Info/EXP";
            _infoCheck.ButtonPressed = true;
            _infoCheck.Toggled += (pressed) => {
                if (CombatLogSystem.Instance != null)
                    CombatLogSystem.Instance.SetShowInfo(pressed);
            };
            _filterContainer.AddChild(_infoCheck);
            
            // Player only
            _playerOnlyCheck = new CheckBox();
            _playerOnlyCheck.Text = "Player Actions Only";
            _playerOnlyCheck.Toggled += (pressed) => {
                if (CombatLogSystem.Instance != null)
                    CombatLogSystem.Instance.SetPlayerOnly(pressed);
            };
            _filterContainer.AddChild(_playerOnlyCheck);
            
            // Enemy only
            _enemyOnlyCheck = new CheckBox();
            _enemyOnlyCheck.Text = "Enemy Actions Only";
            _enemyOnlyCheck.Toggled += (pressed) => {
                if (CombatLogSystem.Instance != null)
                    CombatLogSystem.Instance.SetEnemyOnly(pressed);
            };
            _filterContainer.AddChild(_enemyOnlyCheck);
        }
        
        private void SetupSettingsPanel()
        {
            _settingsPanel = new PanelContainer();
            _settingsPanel.CustomMinimumSize = new Vector2(380, 180);
            _mainContainer.AddChild(_settingsPanel);
            
            _settingsContainer = new VBoxContainer();
            _settingsContainer.AddThemeConstantOverride("separation", 5);
            _settingsPanel.AddChild(_settingsContainer);
            
            var settingsTitle = new Label();
            settingsTitle.Text = "Settings";
            settingsTitle.AddThemeFontSizeOverride("font_size", 14);
            _settingsContainer.AddChild(settingsTitle);
            
            // Auto scroll
            _autoScrollCheck = new CheckBox();
            _autoScrollCheck.Text = "Auto Scroll to Bottom";
            _autoScrollCheck.ButtonPressed = true;
            _autoScrollCheck.Toggled += (pressed) => _autoScroll = pressed;
            _settingsContainer.AddChild(_autoScrollCheck);
            
            // Timestamp
            _timestampCheck = new CheckBox();
            _timestampCheck.Text = "Show Timestamp";
            _timestampCheck.ButtonPressed = true;
            _timestampCheck.Toggled += (pressed) => _showTimestamp = pressed;
            _settingsContainer.AddChild(_timestampCheck);
            
            // Colored text
            _coloredTextCheck = new CheckBox();
            _coloredTextCheck.Text = "Colored Text";
            _coloredTextCheck.ButtonPressed = true;
            _coloredTextCheck.Toggled += (pressed) => _coloredText = pressed;
            _settingsContainer.AddChild(_coloredTextCheck);
            
            // Sound effects
            _soundEffectsCheck = new CheckBox();
            _soundEffectsCheck.Text = "Sound Effects";
            _soundEffectsCheck.ButtonPressed = false;
            _settingsContainer.AddChild(_soundEffectsCheck);
            
            // Font size
            var fontSizeContainer = new HBoxContainer();
            var fontSizeLabel = new Label();
            fontSizeLabel.Text = "Font Size:";
            fontSizeContainer.AddChild(fontSizeLabel);
            
            _fontSizeSpin = new SpinBox();
            _fontSizeSpin.MinValue = 10;
            _fontSizeSpin.MaxValue = 24;
            _fontSizeSpin.Value = 14;
            _fontSizeSpin.ValueChanged += (value) => UpdateFontSize((int)value);
            fontSizeContainer.AddChild(_fontSizeSpin);
            _settingsContainer.AddChild(fontSizeContainer);
            
            // Opacity
            var opacityContainer = new HBoxContainer();
            var opacityLabel = new Label();
            opacityLabel.Text = "Opacity:";
            opacityContainer.AddChild(opacityLabel);
            
            _opacitySlider = new HSlider();
            _opacitySlider.MinValue = 0.3;
            _opacitySlider.MaxValue = 1.0;
            _opacitySlider.Step = 0.1;
            _opacitySlider.Value = 0.9;
            _opacitySlider.ValueChanged += (value) => UpdateOpacity((float)value);
            opacityContainer.AddChild(_opacitySlider);
            _settingsContainer.AddChild(opacityContainer);
        }
        
        private void SetupStatsPanel()
        {
            _statsPanel = new PanelContainer();
            _statsPanel.CustomMinimumSize = new Vector2(380, 150);
            _mainContainer.AddChild(_statsPanel);
            
            _statsContainer = new VBoxContainer();
            _statsContainer.AddThemeConstantOverride("separation", 3);
            _statsPanel.AddChild(_statsContainer);
            
            var statsTitle = new Label();
            statsTitle.Text = "📊 Session Statistics";
            statsTitle.AddThemeFontSizeOverride("font_size", 14);
            _statsContainer.AddChild(statsTitle);
            
            // Session time
            _sessionTimeLabel = new Label();
            _sessionTimeLabel.Text = "Time: 00:00";
            _statsContainer.AddChild(_sessionTimeLabel);
            
            // Damage dealt
            _damageDealtLabel = new Label();
            _damageDealtLabel.Text = "Damage Dealt: 0";
            _damageDealtLabel.Modulate = _playerColor;
            _statsContainer.AddChild(_damageDealtLabel);
            
            // Damage taken
            _damageTakenLabel = new Label();
            _damageTakenLabel.Text = "Damage Taken: 0";
            _damageTakenLabel.Modulate = _enemyColor;
            _statsContainer.AddChild(_damageTakenLabel);
            
            // Healing
            _healingLabel = new Label();
            _healingLabel.Text = "Healing: 0";
            _healingLabel.Modulate = _healColor;
            _statsContainer.AddChild(_healingLabel);
            
            // Kills
            _killsLabel = new Label();
            _killsLabel.Text = "Kills: 0";
            _statsContainer.AddChild(_killsLabel);
            
            // Combo
            _comboLabel = new Label();
            _comboLabel.Text = "Current Combo: 0";
            _comboLabel.Modulate = _skillColor;
            _statsContainer.AddChild(_comboLabel);
        }
        
        private void ConnectSignals()
        {
            // Handle input for toggling
        }
        
        #region Public Methods
        
        /// <summary>
        /// Toggle visibility
        /// </summary>
        public void ToggleVisibility()
        {
            _isVisible = !_isVisible;
            _mainPanel.Visible = _isVisible;
        }
        
        /// <summary>
        /// Show/hide combat log
        /// </summary>
        public void Show()
        {
            _isVisible = true;
            _mainPanel.Visible = true;
        }
        
        /// <summary>
        /// Hide combat log
        /// </summary>
        public void Hide()
        {
            _isVisible = false;
            _mainPanel.Visible = false;
        }
        
        /// <summary>
        /// Clear log entries
        /// </summary>
        public void ClearLog()
        {
            foreach (Node child in _logContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            if (CombatLogSystem.Instance != null)
            {
                CombatLogSystem.Instance.ClearLog();
            }
            
            GD.Print("[CombatLogUI] Log cleared");
        }
        
        #endregion
        
        #region Toggle Methods
        
        private void ToggleFilter()
        {
            _filterOpen = !_filterOpen;
            _filterPanel.Visible = _filterOpen;
        }
        
        private void ToggleSettings()
        {
            _settingsOpen = !_settingsOpen;
            _settingsPanel.Visible = _settingsOpen;
        }
        
        private void ToggleStats()
        {
            _statsOpen = !_statsOpen;
            _statsPanel.Visible = _statsOpen;
        }
        
        #endregion
        
        #region Update Methods
        
        private void OnNewLogEntry(CombatLogEntry entry)
        {
            AddLogEntry(entry);
            UpdateStatistics();
            
            if (_autoScroll)
            {
                _scrollContainer.ScrollVertical = (int)_scrollContainer.GetVScroll().MaxValue;
            }
        }
        
        private void AddLogEntry(CombatLogEntry entry)
        {
            // Limit entries
            if (_logContainer.GetChildCount() >= _maxDisplayEntries)
            {
                var firstChild = _logContainer.GetChild(0);
                if (firstChild != null)
                {
                    firstChild.QueueFree();
                }
            }
            
            var label = new Label();
            label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            label.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            
            // Format message
            string message = entry.Message;
            
            if (_showTimestamp)
            {
                int minutes = (int)(entry.Timestamp / 60);
                int seconds = (int)(entry.Timestamp % 60);
                message = $"[{minutes:D2}:{seconds:D2}] {message}";
            }
            
            label.Text = message;
            
            // Apply color
            if (_coloredText)
            {
                label.Modulate = GetColorForType(entry.Type, entry.IsPlayerAction);
            }
            else
            {
                label.Modulate = _infoColor;
            }
            
            _logContainer.AddChild(label);
        }
        
        private Color GetColorForType(CombatLogType type, bool isPlayerAction)
        {
            switch (type)
            {
                case CombatLogType.Damage:
                case CombatLogType.Kill:
                case CombatLogType.Death:
                    return isPlayerAction ? _playerColor : _enemyColor;
                    
                case CombatLogType.Critical:
                    return _criticalColor;
                    
                case CombatLogType.Healing:
                    return _healColor;
                    
                case CombatLogType.SkillUsed:
                case CombatLogType.ItemUsed:
                    return _skillColor;
                    
                case CombatLogType.Buff:
                    return new Color(0.4f, 1f, 0.8f);
                    
                case CombatLogType.Debuff:
                    return new Color(1f, 0.5f, 0.8f);
                    
                case CombatLogType.Warning:
                    return _warningColor;
                    
                case CombatLogType.Combo:
                    return _skillColor;
                    
                default:
                    return isPlayerAction ? _playerColor : _enemyColor;
            }
        }
        
        private void UpdateStatistics()
        {
            if (CombatLogSystem.Instance == null) return;
            
            var stats = CombatLogSystem.Instance.GetStatistics();
            float sessionTime = CombatLogSystem.Instance.GetSessionTime();
            int currentCombo = CombatLogSystem.Instance.GetCurrentCombo();
            
            // Session time
            int minutes = (int)(sessionTime / 60);
            int seconds = (int)(sessionTime % 60);
            _sessionTimeLabel.Text = $"Time: {minutes:D2}:{seconds:D2}";
            
            // Damage dealt
            _damageDealtLabel.Text = $"Damage Dealt: {stats.TotalDamageDealt:F0}";
            
            // Damage taken
            _damageTakenLabel.Text = $"Damage Taken: {stats.TotalDamageTaken:F0}";
            
            // Healing
            _healingLabel.Text = $"Healing: {stats.TotalHealing:F0}";
            
            // Kills
            _killsLabel.Text = $"Kills: {stats.KillEntries}";
            
            // Combo
            _comboLabel.Text = $"Current Combo: {currentCombo}";
            
            // Pulse combo if active
            if (currentCombo > 0)
            {
                _comboLabel.AddThemeFontSizeOverride("font_size", 18);
            }
            else
            {
                _comboLabel.AddThemeFontSizeOverride("font_size", 14);
            }
        }
        
        private void OnComboMilestone(int milestone)
        {
            // Visual feedback for combo milestone
            var tween = CreateTween();
            _comboLabel.Scale = new Vector2(1.5f, 1.5f);
            tween.TweenProperty(_comboLabel, "scale", Vector2.One, 0.3f);
        }
        
        private void UpdateFontSize(int size)
        {
            foreach (Node child in _logContainer.GetChildren())
            {
                if (child is Label label)
                {
                    label.AddThemeFontSizeOverride("font_size", size);
                }
            }
        }
        
        private void UpdateOpacity(float opacity)
        {
            _mainPanel.Modulate = new Color(1f, 1f, 1f, opacity);
        }
        
        #endregion
        
        public override void _Process(double delta)
        {
            // Update statistics periodically
            if (Engine.GetFramesDrawn() % 30 == 0)
            {
                UpdateStatistics();
            }
        }
    }
}
