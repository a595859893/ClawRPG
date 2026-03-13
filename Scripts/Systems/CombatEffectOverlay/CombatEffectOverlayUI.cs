using Godot;
using System;
using System.Collections.Generic;

public class CombatEffectOverlayUI : Control
{
    // Singleton instance
    private static CombatEffectOverlayUI _instance;
    public static CombatEffectOverlayUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GetNode<CombatEffectOverlayUI>("/root/CombatEffectOverlayUI");
            }
            return _instance;
        }
    }
    
    // Reference to system
    private CombatEffectOverlaySystem _system;
    
    // UI Elements
    private PanelContainer _mainPanel;
    private VBoxContainer _mainBox;
    
    // Tab containers
    private TabContainer _tabContainer;
    private Control _overviewTab;
    private Control _effectsTab;
    private Control _statisticsTab;
    private Control _testTab;
    
    // Labels for real-time display
    private Label _redOverlayLabel;
    private Label _screenFlashLabel;
    private Label _cameraShakeLabel;
    private Label _slowMotionLabel;
    private Label _chromaticLabel;
    private Label _vignetteLabel;
    private Label _floatingTextCountLabel;
    
    // Statistics labels
    private Label _totalFlashesLabel;
    private Label _totalShakesLabel;
    private Label _totalSlowMotionsLabel;
    private Label _totalFloatingTextsLabel;
    private Label _totalShakeIntensityLabel;
    
    // Color for active effects
    private Color _activeColor = new Color(0.3f, 1f, 0.5f);
    private Color _inactiveColor = new Color(0.5f, 0.5f, 0.5f);
    
    public override void _Ready()
    {
        _system = CombatEffectOverlaySystem.Instance;
        
        SetupUI();
        Hide();
    }
    
    public override void _Process(float delta)
    {
        if (Visible)
        {
            UpdateRealTimeDisplay();
        }
    }
    
    private void SetupUI()
    {
        // Create main panel
        _mainPanel = new PanelContainer();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(600, 500);
        AddChild(_mainPanel);
        
        // Create main box
        _mainBox = new VBoxContainer();
        _mainBox.Setanchorspreset(Control.LayoutPreset.FullRect);
        _mainBox.AddThemeConstantOverride("separation", 10);
        _mainPanel.AddChild(_mainBox);
        
        // Title
        var titleLabel = new Label();
        titleLabel.Text = "⚔️ Combat Effect Overlay System ⚔️";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _mainBox.AddChild(titleLabel);
        
        // Create tab container
        _tabContainer = new TabContainer();
        _tabContainer.SetHExpand(true);
        _tabContainer.SetVExpand(true);
        _mainBox.AddChild(_tabContainer);
        
        // Create tabs
        CreateOverviewTab();
        CreateEffectsTab();
        CreateStatisticsTab();
        CreateTestTab();
        
        // Close button
        var closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.Pressed += () => Hide();
        _mainBox.AddChild(closeButton);
    }
    
    private void CreateOverviewTab()
    {
        _overviewTab = new Control();
        _overviewTab.SetHExpand(true);
        _overviewTab.SetVExpand(true);
        _tabContainer.AddChild(_overviewTab);
        _tabContainer.SetTabTitle(_overviewTab, "Overview");
        
        var vbox = new VBoxContainer();
        vbox.Setanchorspreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 15);
        _overviewTab.AddChild(vbox);
        
        // Header
        var header = new Label();
        header.Text = "🎮 Real-Time Effect Status";
        header.HorizontalAlignment = HorizontalAlignment.Center;
        header.AddThemeFontSizeOverride("font_size", 20);
        vbox.AddChild(header);
        
        // Status grid
        var grid = new GridContainer();
        grid.Columns = 2;
        grid.SetHExpand(true);
        vbox.AddChild(grid);
        
        // Red Overlay
        grid.AddChild(CreateStatusRow("Red Overlay:", out _redOverlayLabel));
        
        // Screen Flash
        grid.AddChild(CreateStatusRow("Screen Flash:", out _screenFlashLabel));
        
        // Camera Shake
        grid.AddChild(CreateStatusRow("Camera Shake:", out _cameraShakeLabel));
        
        // Slow Motion
        grid.AddChild(CreateStatusRow("Slow Motion:", out _slowMotionLabel));
        
        // Chromatic Aberration
        grid.AddChild(CreateStatusRow("Chromatic Aberration:", out _chromaticLabel));
        
        // Vignette
        grid.AddChild(CreateStatusRow("Vignette:", out _vignetteLabel));
        
        // Floating Texts
        grid.AddChild(CreateStatusRow("Active Floating Texts:", out _floatingTextCountLabel));
        
        // Info section
        var infoLabel = new Label();
        infoLabel.Text = "\n📖 This system provides visual feedback for combat events:\n" +
                        "• Damage numbers with critical hit effects\n" +
                        "• Screen shake based on impact intensity\n" +
                        "• Slow motion for dramatic moments\n" +
                        "• Chromatic aberration on critical hits\n" +
                        "• Red overlay for low health warnings\n" +
                        "• Floating text for all combat events";
        infoLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        vbox.AddChild(infoLabel);
    }
    
    private Control CreateStatusRow(string labelText, out Label valueLabel)
    {
        var container = new HBoxContainer();
        
        var label = new Label();
        label.Text = labelText;
        label.CustomMinimumSize = new Vector2(200, 0);
        container.AddChild(label);
        
        valueLabel = new Label();
        valueLabel.Text = "Inactive";
        valueLabel.Modulate = _inactiveColor;
        container.AddChild(valueLabel);
        
        return container;
    }
    
    private void CreateEffectsTab()
    {
        _effectsTab = new Control();
        _effectsTab.SetHExpand(true);
        _effectsTab.SetVExpand(true);
        _tabContainer.AddChild(_effectsTab);
        _tabContainer.SetTabTitle(_effectsTab, "Effects Config");
        
        var vbox = new VBoxContainer();
        vbox.Setanchorspreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 15);
        _effectsTab.AddChild(vbox);
        
        var header = new Label();
        header.Text = "⚙️ Available Screen Effects";
        header.HorizontalAlignment = HorizontalAlignment.Center;
        header.AddThemeFontSizeOverride("font_size", 20);
        vbox.AddChild(header);
        
        // Effects list
        var effectsList = new VBoxContainer();
        effectsList.SetHExpand(true);
        vbox.AddChild(effectsList);
        
        AddEffectInfo(effectsList, "Red Overlay", "Low health warning - red tint on screen");
        AddEffectInfo(effectsList, "Screen Flash", "Quick white/colored flash on impacts");
        AddEffectInfo(effectsList, "Camera Shake", "Screen shake intensity based on damage");
        AddEffectInfo(effectsList, "Slow Motion", "Time dilation for dramatic moments");
        AddEffectInfo(effectsList, "Chromatic Aberration", "RGB channel separation on critical hits");
        AddEffectInfo(effectsList, "Vignette", "Dark edges for emphasis");
        
        var noteLabel = new Label();
        noteLabel.Text = "\n💡 These effects are triggered automatically during combat.";
        noteLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
        vbox.AddChild(noteLabel);
    }
    
    private void AddEffectInfo(VBoxContainer parent, string name, string description)
    {
        var hbox = new HBoxContainer();
        
        var nameLabel = new Label();
        nameLabel.Text = "• " + name + ":";
        nameLabel.CustomMinimumSize = new Vector2(180, 0);
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        hbox.AddChild(nameLabel);
        
        var descLabel = new Label();
        descLabel.Text = description;
        descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        hbox.AddChild(descLabel);
        
        parent.AddChild(hbox);
    }
    
    private void CreateStatisticsTab()
    {
        _statisticsTab = new Control();
        _statisticsTab.SetHExpand(true);
        _statisticsTab.SetVExpand(true);
        _tabContainer.AddChild(_statisticsTab);
        _tabContainer.SetTabTitle(_statisticsTab, "Statistics");
        
        var vbox = new VBoxContainer();
        vbox.Setanchorspreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 15);
        _statisticsTab.AddChild(vbox);
        
        var header = new Label();
        header.Text = "📊 Combat Effect Statistics";
        header.HorizontalAlignment = HorizontalAlignment.Center;
        header.AddThemeFontSizeOverride("font_size", 20);
        vbox.AddChild(header);
        
        // Statistics grid
        var grid = new GridContainer();
        grid.Columns = 2;
        grid.SetHExpand(true);
        vbox.AddChild(grid);
        
        grid.AddChild(CreateStatRow("Total Screen Flashes:", out _totalFlashesLabel));
        grid.AddChild(CreateStatRow("Total Camera Shakes:", out _totalShakesLabel));
        grid.AddChild(CreateStatRow("Total Slow Motions:", out _totalSlowMotionsLabel));
        grid.AddChild(CreateStatRow("Total Floating Texts:", out _totalFloatingTextsLabel));
        grid.AddChild(CreateStatRow("Total Shake Intensity:", out _totalShakeIntensityLabel));
        
        // Reset button
        var resetButton = new Button();
        resetButton.Text = "Reset Statistics";
        resetButton.Pressed += () =>
        {
            _system.ResetStatistics();
            UpdateStatistics();
        };
        vbox.AddChild(resetButton);
    }
    
    private Control CreateStatRow(string labelText, out Label valueLabel)
    {
        var container = new HBoxContainer();
        
        var label = new Label();
        label.Text = labelText;
        label.CustomMinimumSize = new Vector2(220, 0);
        container.AddChild(label);
        
        valueLabel = new Label();
        valueLabel.Text = "0";
        valueLabel.AddThemeFontSizeOverride("font_size", 18);
        container.AddChild(valueLabel);
        
        return container;
    }
    
    private void CreateTestTab()
    {
        _testTab = new Control();
        _testTab.SetHExpand(true);
        _testTab.SetVExpand(true);
        _tabContainer.AddChild(_testTab);
        _tabContainer.SetTabTitle(_testTab, "Test Effects");
        
        var vbox = new VBoxContainer();
        vbox.Setanchorspreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 15);
        _testTab.AddChild(vbox);
        
        var header = new Label();
        header.Text = "🧪 Test Combat Effects";
        header.HorizontalAlignment = HorizontalAlignment.Center;
        header.AddThemeFontSizeOverride("font_size", 20);
        vbox.AddChild(header);
        
        // Test buttons grid
        var buttonGrid = new GridContainer();
        buttonGrid.Columns = 3;
        buttonGrid.SetHExpand(true);
        vbox.AddChild(buttonGrid);
        
        // Screen effects
        AddTestButton(buttonGrid, "Red Overlay", () => _system.TriggerRedOverlay());
        AddTestButton(buttonGrid, "Screen Flash", () => _system.TriggerScreenFlash());
        AddTestButton(buttonGrid, "Chromatic Aberration", () => _system.TriggerChromaticAberration());
        AddTestButton(buttonGrid, "Vignette", () => _system.TriggerVignette());
        
        // Camera shake
        AddTestButton(buttonGrid, "Shake Light", () => _system.TriggerCameraShake("light"));
        AddTestButton(buttonGrid, "Shake Medium", () => _system.TriggerCameraShake("medium"));
        AddTestButton(buttonGrid, "Shake Heavy", () => _system.TriggerCameraShake("heavy"));
        AddTestButton(buttonGrid, "Shake Extreme", () => _system.TriggerCameraShake("extreme"));
        
        // Slow motion
        AddTestButton(buttonGrid, "Slow Quick", () => _system.TriggerSlowMotion("quick"));
        AddTestButton(buttonGrid, "Slow Normal", () => _system.TriggerSlowMotion("normal"));
        AddTestButton(buttonGrid, "Slow Dramatic", () => _system.TriggerSlowMotion("dramatic"));
        
        // Floating text
        AddTestButton(buttonGrid, "Damage", () => SpawnTestDamage(false));
        AddTestButton(buttonGrid, "Critical", () => SpawnTestDamage(true));
        AddTestButton(buttonGrid, "Heal", () => _system.SpawnHealText(50, GetRandomScreenPosition()));
        AddTestButton(buttonGrid, "Miss", () => _system.SpawnMissText(GetRandomScreenPosition()));
        
        // Combat simulation
        var combatLabel = new Label();
        combatLabel.Text = "\n⚔️ Combat Simulation:";
        combatLabel.AddThemeFontSizeOverride("font_size", 16);
        vbox.AddChild(combatLabel);
        
        var combatGrid = new HBoxContainer();
        combatGrid.SetHExpand(true);
        vbox.AddChild(combatGrid);
        
        var normalAttackBtn = new Button();
        normalAttackBtn.Text = "Normal Attack";
        normalAttackBtn.Pressed += () => _system.OnDamageDealt(25, GetRandomScreenPosition(), false);
        combatGrid.AddChild(normalAttackBtn);
        
        var criticalAttackBtn = new Button();
        criticalAttackBtn.Text = "Critical Hit";
        criticalAttackBtn.Pressed += () => _system.OnDamageDealt(150, GetRandomScreenPosition(), true);
        combatGrid.AddChild(criticalAttackBtn);
        
        var bossAttackBtn = new Button();
        bossAttackBtn.Text = "Boss Attack";
        bossAttackBtn.Pressed += () => _system.OnDamageDealt(300, GetRandomScreenPosition(), true, true);
        combatGrid.AddChild(bossAttackBtn);
        
        var healBtn = new Button();
        healBtn.Text = "Heal";
        healBtn.Pressed += () => _system.OnHeal(100, GetRandomScreenPosition());
        combatGrid.AddChild(healBtn);
    }
    
    private void AddTestButton(GridContainer parent, string text, Action callback)
    {
        var button = new Button();
        button.Text = text;
        button.Pressed += callback;
        button.CustomMinimumSize = new Vector2(150, 40);
        parent.AddChild(button);
    }
    
    private Vector2 GetRandomScreenPosition()
    {
        var random = new Random();
        var viewportSize = GetViewportRect().Size;
        float x = (float)(random.NextDouble() * (viewportSize.x - 200) + 100);
        float y = (float)(random.NextDouble() * (viewportSize.y - 300) + 100);
        return new Vector2(x, y);
    }
    
    private void SpawnTestDamage(bool isCritical)
    {
        float damage = isCritical ? 100 + (float)(new Random().NextDouble() * 100) : 20 + (float)(new Random().NextDouble() * 30);
        _system.OnDamageDealt(damage, GetRandomScreenPosition(), isCritical);
    }
    
    private void UpdateRealTimeDisplay()
    {
        var data = _system.Data;
        
        // Update status labels
        UpdateStatusLabel(_redOverlayLabel, data.RedOverlayActive, data.RedOverlayIntensity);
        UpdateStatusLabel(_screenFlashLabel, data.ScreenFlashActive, data.ScreenFlashIntensity);
        UpdateStatusLabel(_cameraShakeLabel, data.CameraShakeActive, data.CameraShakeIntensity);
        UpdateStatusLabel(_slowMotionLabel, data.SlowMotionActive, 1f - data.SlowMotionScale);
        UpdateStatusLabel(_chromaticLabel, data.ChromaticAberrationActive, data.ChromaticAberrationIntensity / 10f);
        UpdateStatusLabel(_vignetteLabel, data.VignetteActive, data.VignetteIntensity);
        
        _floatingTextCountLabel.Text = data.ActiveFloatingTexts.Count.ToString();
        _floatingTextCountLabel.Modulate = data.ActiveFloatingTexts.Count > 0 ? _activeColor : _inactiveColor;
        
        // Update statistics
        UpdateStatistics();
    }
    
    private void UpdateStatusLabel(Label label, bool isActive, float intensity)
    {
        if (label == null) return;
        
        if (isActive)
        {
            label.Text = $"Active ({intensity:F2})";
            label.Modulate = _activeColor;
        }
        else
        {
            label.Text = "Inactive";
            label.Modulate = _inactiveColor;
        }
    }
    
    private void UpdateStatistics()
    {
        if (_totalFlashesLabel != null)
            _totalFlashesLabel.Text = _system.GetTotalScreenFlashes().ToString();
        if (_totalShakesLabel != null)
            _totalShakesLabel.Text = _system.GetTotalCameraShakes().ToString();
        if (_totalSlowMotionsLabel != null)
            _totalSlowMotionsLabel.Text = _system.GetTotalSlowMotions().ToString();
        if (_totalFloatingTextsLabel != null)
            _totalFloatingTextsLabel.Text = _system.GetTotalFloatingTexts().ToString();
        if (_totalShakeIntensityLabel != null)
            _totalShakeIntensityLabel.Text = _system.GetTotalShakeIntensity().ToString("F1");
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            if (Visible)
            {
                Hide();
                GetTree().SetInputAsHandled();
            }
        }
    }
}
