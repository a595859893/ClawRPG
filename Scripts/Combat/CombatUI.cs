using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Combat;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// Combat UI - Godot UI display for combat interface
    /// </summary>
    public partial class CombatUI : Control
    {
        private static CombatUI _instance;
        public static CombatUI Instance => _instance;
        
        // Main containers
        private VBoxContainer _mainContainer;
        private HBoxContainer _topBar;
        private HBoxContainer _bottomBar;
        
        // Player info panel
        private PanelContainer _playerPanel;
        private ProgressBar _healthBar;
        private ProgressBar _energyBar;
        private ProgressBar _shieldBar;
        private Label _healthLabel;
        private Label _energyLabel;
        
        // Combo display
        private PanelContainer _comboPanel;
        private Label _comboLabel;
        private Label _comboHitLabel;
        
        // Statistics panel
        private PanelContainer _statsPanel;
        private VBoxContainer _statsContainer;
        private Label _dpsLabel;
        private Label _damageLabel;
        private Label _healingLabel;
        private Label _killsLabel;
        
        // Settings panel
        private PanelContainer _settingsPanel;
        private CheckBox _showDamageNumbersCheck;
        private CheckBox _showHealthBarsCheck;
        private CheckBox _showComboCheck;
        private CheckBox _showDPSCheck;
        private HSlider _uiScaleSlider;
        
        // Animation timers
        private float _comboPulseTimer = 0f;
        private bool _isComboPulsing = false;
        
        // Toggle state
        private bool _isVisible = true;
        private bool _settingsOpen = false;
        
        public override void _Ready()
        {
            _instance = this;
            
            SetupUI();
            ConnectSignals();
            
            GD.Print("[CombatUI] Combat UI initialized");
        }
        
        private void SetupUI()
        {
            // Main container
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _mainContainer.MouseFilter = Control.MouseFilterEnum.Stop;
            AddChild(_mainContainer);
            
            // Top bar - Player info
            SetupTopBar();
            
            // Bottom bar - Combo & Settings
            SetupBottomBar();
            
            // Stats panel (hidden by default)
            SetupStatsPanel();
            
            // Settings panel (hidden by default)
            SetupSettingsPanel();
            
            // Initially hide settings
            _settingsPanel.Visible = false;
        }
        
        private void SetupTopBar()
        {
            _topBar = new HBoxContainer();
            _topBar.Alignment = HBoxContainer.AlignmentMode.Center;
            _topBar.CustomMinimumSize = new Vector2(0, 100);
            _mainContainer.AddChild(_topBar);
            
            // Player health panel
            _playerPanel = new PanelContainer();
            _playerPanel.CustomMinimumSize = new Vector2(400, 80);
            _topBar.AddChild(_playerPanel);
            
            var playerVBox = new VBoxContainer();
            _playerPanel.AddChild(playerVBox);
            
            // Health bar
            var healthContainer = new HBoxContainer();
            playerVBox.AddChild(healthContainer);
            
            var healthLabel = new Label();
            healthLabel.Text = "HP: ";
            healthLabel.AddThemeFontSizeOverride("font_size", 16);
            healthContainer.AddChild(healthLabel);
            
            _healthBar = new ProgressBar();
            _healthBar.CustomMinimumSize = new Vector2(300, 25);
            _healthBar.MinValue = 0;
            _healthBar.MaxValue = 100;
            _healthBar.Value = 100;
            _healthBar.ShowPercentage = false;
            healthContainer.AddChild(_healthBar);
            
            _healthLabel = new Label();
            _healthLabel.Text = "100/100";
            _healthLabel.AddThemeFontSizeOverride("font_size", 14);
            healthContainer.AddChild(_healthLabel);
            
            // Shield bar
            var shieldContainer = new HBoxContainer();
            playerVBox.AddChild(shieldContainer);
            
            var shieldLabel = new Label();
            shieldLabel.Text = "🛡️ ";
            shieldLabel.AddThemeFontSizeOverride("font_size", 14);
            shieldContainer.AddChild(shieldLabel);
            
            _shieldBar = new ProgressBar();
            _shieldBar.CustomMinimumSize = new Vector2(200, 15);
            _shieldBar.MinValue = 0;
            _shieldBar.MaxValue = 100;
            _shieldBar.Value = 0;
            _shieldBar.ShowPercentage = false;
            shieldContainer.AddChild(_shieldBar);
            
            // Energy bar
            var energyContainer = new HBoxContainer();
            playerVBox.AddChild(energyContainer);
            
            var energyLabel = new Label();
            energyLabel.Text = "⚡ ";
            energyLabel.AddThemeFontSizeOverride("font_size", 14);
            energyContainer.AddChild(energyLabel);
            
            _energyBar = new ProgressBar();
            _energyBar.CustomMinimumSize = new Vector2(200, 15);
            _energyBar.MinValue = 0;
            _energyBar.MaxValue = 100;
            _energyBar.Value = 100;
            _energyBar.ShowPercentage = false;
            energyContainer.AddChild(_energyBar);
            
            _energyLabel = new Label();
            _energyLabel.Text = "100/100";
            _energyLabel.AddThemeFontSizeOverride("font_size", 12);
            energyContainer.AddChild(_energyLabel);
        }
        
        private void SetupBottomBar()
        {
            var spacer = new Control();
            spacer.CustomMinimumSize = new Vector2(0, 50);
            _mainContainer.AddChild(spacer);
            
            _bottomBar = new HBoxContainer();
            _bottomBar.Alignment = HBoxContainer.AlignmentMode.Center;
            _bottomBar.CustomMinimumSize = new Vector2(0, 60);
            _mainContainer.AddChild(_bottomBar);
            
            // Combo panel
            _comboPanel = new PanelContainer();
            _comboPanel.CustomMinimumSize = new Vector2(200, 60);
            _bottomBar.AddChild(_comboPanel);
            
            var comboVBox = new VBoxContainer();
            _comboPanel.AddChild(comboVBox);
            
            _comboLabel = new Label();
            _comboLabel.Text = "COMBO";
            _comboLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _comboLabel.AddThemeFontSizeOverride("font_size", 24);
            _comboLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f)); // Gold
            comboVBox.AddChild(_comboLabel);
            
            _comboHitLabel = new Label();
            _comboHitLabel.Text = "0 Hits";
            _comboHitLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _comboHitLabel.AddThemeFontSizeOverride("font_size", 14);
            comboVBox.AddChild(_comboHitLabel);
            
            // Stats button
            var statsButton = new Button();
            statsButton.Text = "📊 Stats";
            statsButton.Pressed += OnStatsButtonPressed;
            _bottomBar.AddChild(statsButton);
            
            // Settings button
            var settingsButton = new Button();
            settingsButton.Text = "⚙️ Settings";
            settingsButton.Pressed += OnSettingsButtonPressed;
            _bottomBar.AddChild(settingsButton);
        }
        
        private void SetupStatsPanel()
        {
            _statsPanel = new PanelContainer();
            _statsPanel.Position = new Vector2(20, 150);
            _statsPanel.CustomMinimumSize = new Vector2(200, 150);
            AddChild(_statsPanel);
            
            _statsContainer = new VBoxContainer();
            _statsPanel.AddChild(_statsContainer);
            
            var titleLabel = new Label();
            titleLabel.Text = "⚔️ Combat Stats";
            titleLabel.AddThemeFontSizeOverride("font_size", 16);
            _statsContainer.AddChild(titleLabel);
            
            _statsContainer.AddChild(new HSeparator());
            
            _dpsLabel = new Label();
            _dpsLabel.Text = "DPS: 0";
            _statsContainer.AddChild(_dpsLabel);
            
            _damageLabel = new Label();
            _damageLabel.Text = "Damage: 0";
            _statsContainer.AddChild(_damageLabel);
            
            _healingLabel = new Label();
            _healingLabel.Text = "Healing: 0";
            _statsContainer.AddChild(_healingLabel);
            
            _killsLabel = new Label();
            _killsLabel.Text = "Kills: 0";
            _statsContainer.AddChild(_killsLabel);
            
            var critLabel = new Label();
            critLabel.Name = "crit_label";
            critLabel.Text = "Crits: 0";
            _statsContainer.AddChild(critLabel);
            
            var closeButton = new Button();
            closeButton.Text = "Close";
            closeButton.Pressed += () => _statsPanel.Visible = false;
            _statsContainer.AddChild(closeButton);
            
            _statsPanel.Visible = false;
        }
        
        private void SetupSettingsPanel()
        {
            _settingsPanel = new PanelContainer();
            _settingsPanel.Position = new Vector2(50, 200);
            _settingsPanel.CustomMinimumSize = new Vector2(250, 200);
            AddChild(_settingsPanel);
            
            var settingsVBox = new VBoxContainer();
            _settingsPanel.AddChild(settingsVBox);
            
            var titleLabel = new Label();
            titleLabel.Text = "⚙️ Combat UI Settings";
            titleLabel.AddThemeFontSizeOverride("font_size", 16);
            settingsVBox.AddChild(titleLabel);
            
            settingsVBox.AddChild(new HSeparator());
            
            _showDamageNumbersCheck = new CheckBox();
            _showDamageNumbersCheck.Text = "Show Damage Numbers";
            _showDamageNumbersCheck.ButtonPressed = true;
            _showDamageNumbersCheck.Toggled += OnDamageNumbersToggled;
            settingsVBox.AddChild(_showDamageNumbersCheck);
            
            _showHealthBarsCheck = new CheckBox();
            _showHealthBarsCheck.Text = "Show Health Bars";
            _showHealthBarsCheck.ButtonPressed = true;
            _showHealthBarsCheck.Toggled += OnHealthBarsToggled;
            settingsVBox.AddChild(_showHealthBarsCheck);
            
            _showComboCheck = new CheckBox();
            _showComboCheck.Text = "Show Combo Counter";
            _showComboCheck.ButtonPressed = true;
            _showComboCheck.Toggled += OnComboToggled;
            settingsVBox.AddChild(_showComboCheck);
            
            _showDPSCheck = new CheckBox();
            _showDPSCheck.Text = "Show DPS";
            _showDPSCheck.ButtonPressed = false;
            _showDPSCheck.Toggled += OnDPSToggled;
            settingsVBox.AddChild(_showDPSCheck);
            
            var scaleLabel = new Label();
            scaleLabel.Text = "UI Scale:";
            settingsVBox.AddChild(scaleLabel);
            
            _uiScaleSlider = new HSlider();
            _uiScaleSlider.CustomMinimumSize = new Vector2(200, 20);
            _uiScaleSlider.MinValue = 0.5f;
            _uiScaleSlider.MaxValue = 2.0f;
            _uiScaleSlider.Step = 0.1f;
            _uiScaleSlider.Value = 1.0f;
            _uiScaleSlider.ValueChanged += OnUIScaleChanged;
            settingsVBox.AddChild(_uiScaleSlider);
            
            var closeButton = new Button();
            closeButton.Text = "Close Settings";
            closeButton.Pressed += OnSettingsButtonPressed;
            settingsVBox.AddChild(closeButton);
        }
        
        private void ConnectSignals()
        {
            // Connect to combat system signals
            // CombatUISystem.Instance.Connect(CombatUISystem.SignalDamageDealt, this, nameof(OnDamageDealt));
            // CombatUISystem.Instance.Connect(CombatUISystem.SignalHealing, this, nameof(OnHealing));
            // CombatUISystem.Instance.Connect(CombatUISystem.SignalKill, this, nameof(OnKill));
        }
        
        public override void _Process(double delta)
        {
            UpdatePlayerInfo();
            UpdateComboDisplay();
            UpdateStatistics();
            ProcessComboPulse(delta);
        }
        
        private void UpdatePlayerInfo()
        {
            if (CombatUISystem.Instance == null) return;
            
            var playerState = CombatUISystem.Instance.GetPlayerState();
            
            if (playerState.MaxHealth > 0)
            {
                _healthBar.MaxValue = playerState.MaxHealth;
                _healthBar.Value = playerState.CurrentHealth;
                _healthLabel.Text = $"{(int)playerState.CurrentHealth}/{(int)playerState.MaxHealth}";
                
                // Update health bar color based on percentage
                float healthPercent = playerState.CurrentHealth / playerState.MaxHealth;
                var healthColor = GetHealthColor(healthPercent);
                _healthBar.AddThemeColorOverride("fill_color", healthColor);
            }
            
            if (playerState.MaxEnergy > 0)
            {
                _energyBar.MaxValue = playerState.MaxEnergy;
                _energyBar.Value = playerState.CurrentEnergy;
                _energyLabel.Text = $"{(int)playerState.CurrentEnergy}/{(int)playerState.MaxEnergy}";
            }
            
            if (playerState.CurrentShield > 0)
            {
                _shieldBar.MaxValue = playerState.MaxHealth;
                _shieldBar.Value = playerState.CurrentShield;
                _shieldBar.Visible = true;
            }
            else
            {
                _shieldBar.Visible = false;
            }
        }
        
        private Color GetHealthColor(float percent)
        {
            if (percent > 0.75f) return new Color(0f, 1f, 0f);           // Green
            if (percent > 0.5f) return new Color(0.5f, 1f, 0f);          // Light Green
            if (percent > 0.25f) return new Color(1f, 1f, 0f);           // Yellow
            if (percent > 0.1f) return new Color(1f, 0.5f, 0f);          // Orange
            return new Color(1f, 0f, 0f);                                // Red
        }
        
        private void UpdateComboDisplay()
        {
            if (CombatUISystem.Instance == null) return;
            
            var combo = CombatUISystem.Instance.GetCurrentCombo();
            
            if (combo.CurrentCombo > 0)
            {
                _comboLabel.Text = $"x{combo.CurrentCombo}";
                _comboHitLabel.Text = $"{combo.ComboHits} Hits";
                _comboPanel.Visible = true;
                
                // Scale based on combo size
                float scale = 1.0f + (combo.CurrentCombo * 0.02f);
                scale = Mathf.Min(scale, 2.0f);
                _comboLabel.AddThemeFontSizeOverride("font_size", (int)(24 * scale));
                
                // Color based on combo milestones
                if (combo.CurrentCombo >= 30)
                {
                    _comboLabel.AddThemeColorOverride("font_color", new Color(0f, 1f, 1f)); // Cyan
                }
                else if (combo.CurrentCombo >= 20)
                {
                    _comboLabel.AddThemeColorOverride("font_color", new Color(0.58f, 0f, 0.83f)); // Purple
                }
                else if (combo.CurrentCombo >= 10)
                {
                    _comboLabel.AddThemeColorOverride("font_color", new Color(1f, 0.27f, 0f)); // Orange
                }
                else if (combo.CurrentCombo >= 5)
                {
                    _comboLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f)); // Gold
                }
                
                // Trigger pulse animation
                if (!_isComboPulsing)
                {
                    _isComboPulsing = true;
                    _comboPulseTimer = 0.2f;
                }
            }
            else
            {
                _comboPanel.Visible = false;
            }
        }
        
        private void ProcessComboPulse(float delta)
        {
            if (_isComboPulsing)
            {
                _comboPulseTimer -= delta;
                if (_comboPulseTimer <= 0)
                {
                    _isComboPulsing = false;
                }
            }
        }
        
        private void UpdateStatistics()
        {
            if (CombatUISystem.Instance == null) return;
            
            var stats = CombatUISystem.Instance.GetStatistics();
            
            _dpsLabel.Text = $"DPS: {stats.DPS:F1}";
            _damageLabel.Text = $"Damage: {stats.TotalDamageDealt}";
            _healingLabel.Text = $"Healing: {stats.TotalHealing}";
            _killsLabel.Text = $"Kills: {stats.EnemiesKilled}";
        }
        
        #region UI Event Handlers
        
        private void OnStatsButtonPressed()
        {
            _statsPanel.Visible = !_statsPanel.Visible;
        }
        
        private void OnSettingsButtonPressed()
        {
            _settingsOpen = !_settingsOpen;
            _settingsPanel.Visible = _settingsOpen;
        }
        
        private void OnDamageNumbersToggled(bool toggled)
        {
            if (CombatUISystem.Instance != null)
            {
                CombatUISystem.Instance.GetPreferences().ShowDamageNumbers = toggled;
            }
        }
        
        private void OnHealthBarsToggled(bool toggled)
        {
            if (CombatUISystem.Instance != null)
            {
                CombatUISystem.Instance.GetPreferences().ShowHealthBars = toggled;
            }
        }
        
        private void OnComboToggled(bool toggled)
        {
            if (CombatUISystem.Instance != null)
            {
                CombatUISystem.Instance.GetPreferences().ShowComboCounter = toggled;
            }
            _comboPanel.Visible = toggled;
        }
        
        private void OnDPSToggled(bool toggled)
        {
            if (CombatUISystem.Instance != null)
            {
                CombatUISystem.Instance.GetPreferences().ShowDPS = toggled;
            }
        }
        
        private void OnUIScaleChanged(float value)
        {
            if (CombatUISystem.Instance != null)
            {
                CombatUISystem.Instance.GetPreferences().UIScale = value;
            }
            Scale = new Vector2(value, value);
        }
        
        #endregion
        
        #region Toggle Visibility
        
        /// <summary>
        /// Toggle combat UI visibility
        /// </summary>
        public void Toggle()
        {
            _isVisible = !_isVisible;
            _mainContainer.Visible = _isVisible;
        }
        
        /// <summary>
        /// Show combat UI
        /// </summary>
        public void Show()
        {
            _isVisible = true;
            _mainContainer.Visible = true;
        }
        
        /// <summary>
        /// Hide combat UI
        /// </summary>
        public void Hide()
        {
            _isVisible = false;
            _mainContainer.Visible = false;
        }
        
        #endregion
    }
}
