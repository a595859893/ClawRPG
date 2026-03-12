using Godot;
using System;
using System.Collections.Generic;

public class GuildTowerDefenseUI : Control
{
    private GuildTowerDefenseSystem _system;
    private Control _mainPanel;
    private TabContainer _tabContainer;
    
    // Info labels
    private Label _waveLabel;
    private Label _livesLabel;
    private Label _goldLabel;
    private Label _pointsLabel;
    private Label _enemiesLabel;
    private Label _towersLabel;
    
    // Tower selection
    private OptionButton _towerTypeOption;
    private Button _buildButton;
    private Button _upgradeButton;
    private Button _startWaveButton;
    
    // Tower list
    private ItemList _towerList;
    
    // Stats panel
    private Label _statsLabel;
    
    // Visibility
    private bool _visible = false;
    
    public override void _Ready()
    {
        _system = GetNode<GuildTowerDefenseSystem>("/root/Main/GuildTowerDefenseSystem");
        
        SetupUI();
        ConnectSignals();
        
        // Initial state - hidden
        Hide();
    }
    
    private void SetupUI()
    {
        // Main panel
        _mainPanel = new Panel
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            AnchorTop = 0.1f,
            AnchorLeft = 0.1f,
            AnchorBottom = 0.9f
        };
        AddChild(_mainPanel);
        
        var mainBg = new ColorRect
        {
            Color = new Color(0.1f, 0.1f, 0.15f, 0.95f),
            AnchorRight = 1f,
            AnchorBottom = 1f
        };
        _mainPanel.AddChild(mainBg);
        
        // Title
        var titleLabel = new Label
        {
            Text = "🏰 Guild Tower Defense",
            AnchorTop = 0.02f,
            AnchorLeft = 0.35f,
            AnchorRight = 0.65f,
            AnchorTop = 0.02f,
            Align = Label.AlignEnum.Center
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.5f));
        mainBg.AddChild(titleLabel);
        
        // Tab container
        _tabContainer = new TabContainer
        {
            AnchorTop = 0.08f,
            AnchorLeft = 0.02f,
            AnchorRight = 0.98f,
            AnchorBottom = 0.92f
        };
        mainBg.AddChild(_tabContainer);
        
        // Setup tabs
        SetupDefenseTab();
        SetupowersTab();
        SetupStatsTab();
        // Close button
        var closeBtn = new Button
        {
            Text = "✕",
            AnchorLeft = 0.92f,
            AnchorTop = 0.02f,
            AnchorRight = 0.98f,
            AnchorBottom = 0.06f
        };
        closeBtn.Connect("pressed", this, nameof(OnClosePressed));
        mainBg.AddChild(closeBtn);
        
        // Info bar at bottom
        SetupInfoBar(mainBg);
    }
    
    private void SetupDefenseTab()
    {
        var defensePanel = new Control();
        defensePanel.Name = "Defense";
        _tabContainer.AddChild(defensePanel);
        
        // Wave info
        _waveLabel = new Label
        {
            Text = "Wave: 0/20",
            AnchorLeft = 0.05f,
            AnchorTop = 0.05f,
            AnchorRight = 0.45f,
            Align = Label.AlignEnum.Left
        };
        _waveLabel.AddThemeFontSizeOverride("font_size", 18);
        defensePanel.AddChild(_waveLabel);
        
        // Lives
        _livesLabel = new Label
        {
            Text = "❤️ Lives: 20",
            AnchorLeft = 0.55f,
            AnchorTop = 0.05f,
            AnchorRight = 0.95f,
            Align = Label.AlignEnum.Right
        };
        _livesLabel.AddThemeFontSizeOverride("font_size", 18);
        _livesLabel.AddThemeColorOverride("font_color", new Color(1f, 0.3f, 0.3f));
        defensePanel.AddChild(_livesLabel);
        
        // Gold
        _goldLabel = new Label
        {
            Text = "💰 Gold: 0",
            AnchorLeft = 0.05f,
            AnchorTop = 0.12f,
            AnchorRight = 0.45f,
            Align = Label.AlignEnum.Left
        };
        _goldLabel.AddThemeFontSizeOverride("font_size", 16);
        defensePanel.AddChild(_goldLabel);
        
        // Points
        _pointsLabel = new Label
        {
            Text = "⭐ Points: 0",
            AnchorLeft = 0.55f,
            AnchorTop = 0.12f,
            AnchorRight = 0.95f,
            Align = Label.AlignEnum.Right
        };
        _pointsLabel.AddThemeFontSizeOverride("font_size", 16);
        defensePanel.AddChild(_pointsLabel);
        
        // Tower type selector
        var towerTypeLabel = new Label
        {
            Text = "Tower Type:",
            AnchorLeft = 0.05f,
            AnchorTop = 0.22f,
            AnchorRight = 0.25f
        };
        defensePanel.AddChild(towerTypeLabel);
        
        _towerTypeOption = new OptionButton
        {
            AnchorLeft = 0.25f,
            AnchorTop = 0.22f,
            AnchorRight = 0.55f
        };
        
        // Add tower types
        var towerTypes = new[] { "Arrow Tower", "Cannon Tower", "Ice Tower", "Fire Tower", 
                                  "Lightning Tower", "Poison Tower", "Support Tower", "Ultimate Tower" };
        foreach (var type in towerTypes)
        {
            _towerTypeOption.AddItem(type);
        }
        
        defensePanel.AddChild(_towerTypeOption);
        
        // Build button
        _buildButton = new Button
        {
            Text = "🔨 Build Tower (100g)",
            AnchorLeft = 0.60f,
            AnchorTop = 0.22f,
            AnchorRight = 0.95f
        };
        _buildButton.Connect("pressed", this, nameof(OnBuildPressed));
        defensePanel.AddChild(_buildButton);
        
        // Upgrade button
        _upgradeButton = new Button
        {
            Text = "⬆️ Upgrade Selected",
            AnchorLeft = 0.60f,
            AnchorTop = 0.30f,
            AnchorRight = 0.95f
        };
        _upgradeButton.Connect("pressed", this, nameof(OnUpgradePressed));
        defensePanel.AddChild(_upgradeButton);
        
        // Start wave button
        _startWaveButton = new Button
        {
            Text = "⚔️ Start Wave",
            AnchorLeft = 0.05f,
            AnchorTop = 0.40f,
            AnchorRight = 0.35f
        };
        _startWaveButton.Connect("pressed", this, nameof(OnStartWavePressed));
        defensePanel.AddChild(_startWaveButton);
        
        // Enemies defeated
        _enemiesLabel = new Label
        {
            Text = "👹 Enemies Defeated: 0",
            AnchorLeft = 0.40f,
            AnchorTop = 0.40f,
            AnchorRight = 0.95f
        };
        defensePanel.AddChild(_enemiesLabel);
        
        // Towers built
        _towersLabel = new Label
        {
            Text = "🏗️ Towers Built: 0",
            AnchorLeft = 0.40f,
            AnchorTop = 0.48f,
            AnchorRight = 0.95f
        };
        defensePanel.AddChild(_towersLabel);
        
        // Tower list
        var towerListLabel = new Label
        {
            Text = "Built Towers:",
            AnchorLeft = 0.05f,
            AnchorTop = 0.58f,
            AnchorRight = 0.25f
        };
        defensePanel.AddChild(towerListLabel);
        
        _towerList = new ItemList
        {
            AnchorLeft = 0.05f,
            AnchorTop = 0.65f,
            AnchorRight = 0.95f,
            AnchorBottom = 0.95f
        };
        defensePanel.AddChild(_towerList);
    }
    
    private void SetupTowersTab()
    {
        var towersPanel = new Control();
        towersPanel.Name = "Towers";
        _tabContainer.AddChild(towersPanel);
        
        var infoLabel = new Label
        {
            Text = "🏰 Tower Information\n\n" +
                   "🔹 Arrow Tower - Fast attack, single target\n" +
                   "🔹 Cannon Tower - High damage, slow\n" +
                   "🔹 Ice Tower - Slows enemies\n" +
                   "🔹 Fire Tower - Area damage\n" +
                   "🔹 Lightning Tower - Chain damage\n" +
                   "🔹 Poison Tower - Damage over time\n" +
                   "🔹 Support Tower - Buffs nearby towers\n" +
                   "🔹 Ultimate Tower - Massive damage\n\n" +
                   "Towers can be upgraded to level 3 for increased stats.",
            AnchorLeft = 0.05f,
            AnchorTop = 0.05f,
            AnchorRight = 0.95f,
            Align = Label.AlignEnum.Center
        };
        infoLabel.AddThemeFontSizeOverride("font_size", 14);
        towersPanel.AddChild(infoLabel);
    }
    
    private void SetupStatsTab()
    {
        var statsPanel = new Control();
        statsPanel.Name = "Stats";
        _tabContainer.AddChild(statsPanel);
        
        _statsLabel = new Label
        {
            Text = "📊 Statistics\n\n" +
                   "Total Gold Earned: 0\n" +
                   "Total Points: 0\n" +
                   "Enemies Defeated: 0\n" +
                   "Towers Built: 0\n\n" +
                   "Complete waves to earn rewards!\n" +
                   "Build towers strategically to defend against enemy waves.",
            AnchorLeft = 0.05f,
            AnchorTop = 0.05f,
            AnchorRight = 0.95f,
            Align = Label.AlignEnum.Center
        };
        _statsLabel.AddThemeFontSizeOverride("font_size", 16);
        statsPanel.AddChild(_statsLabel);
    }
    
    private void SetupInfoBar(ColorRect parent)
    {
        var infoPanel = new Panel
        {
            AnchorLeft = 0f,
            AnchorRight = 1f,
            AnchorBottom = 1f,
            AnchorTop = 0.92f
        };
        parent.AddChild(infoPanel);
        
        var infoBg = new ColorRect
        {
            Color = new Color(0.05f, 0.05f, 0.1f, 0.9f),
            AnchorRight = 1f,
            AnchorBottom = 1f
        };
        infoPanel.AddChild(infoBg);
        
        var hintLabel = new Label
        {
            Text = "Press T to toggle | Build towers to defend against enemy waves!",
            AnchorLeft = 0.05f,
            AnchorRight = 0.95f,
            Align = Label.AlignEnum.Center
        };
        infoBg.AddChild(hintLabel);
    }
    
    private void ConnectSignals()
    {
        // Connect input
    }
    
    public override void _Input(InputEvent eventEvent)
    {
        if (eventEvent is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.T)
            {
                ToggleVisibility();
            }
            else if (keyEvent.Keycode == Key.Escape && _visible)
            {
                Hide();
                _visible = false;
            }
        }
    }
    
    public void ToggleVisibility()
    {
        if (_visible)
        {
            Hide();
            _visible = false;
        }
        else
        {
            Show();
            _visible = true;
            RefreshUI();
        }
    }
    
    public void RefreshUI()
    {
        if (_system == null) return;
        
        // Update wave info
        var currentWave = _system.GetCurrentWave();
        int waveNum = _system.GetCurrentWaveNumber();
        _waveLabel.Text = $"Wave: {waveNum}/20";
        
        // Lives
        int lives = _system.GetLives();
        _livesLabel.Text = $"❤️ Lives: {lives}";
        
        // Gold and points
        _goldLabel.Text = $"💰 Gold: {_system.GetTotalGold()}";
        _pointsLabel.Text = $"⭐ Points: {_system.GetTotalPoints()}";
        
        // Enemies and towers
        _enemiesLabel.Text = $"👹 Enemies Defeated: {_system.GetEnemiesDefeated()}";
        _towersLabel.Text = $"🏗️ Towers Built: {_system.GetTowersBuilt()}";
        
        // Tower list
        _towerList.Clear();
        var towers = _system.GetAllTowers();
        for (int i = 0; i < towers.Count; i++)
        {
            var tower = towers[i];
            string towerInfo = $"{GuildTowerDefenseSystem.GetTowerTypeName(tower.Type)} Lv.{tower.Level}";
            if (tower.Level < 3)
                towerInfo += " [Upgradable]";
            _towerList.AddItem(towerInfo);
        }
        
        // Stats
        _statsLabel.Text = "📊 Statistics\n\n" +
                   $"Total Gold Earned: {_system.GetTotalGold()}\n" +
                   $"Total Points: {_system.GetTotalPoints()}\n" +
                   $"Enemies Defeated: {_system.GetEnemiesDefeated()}\n" +
                   $"Towers Built: {_system.GetTowersBuilt()}\n\n" +
                   "Complete waves to earn rewards!\n" +
                   "Build towers strategically to defend against enemy waves.";
        
        // Update button states
        _startWaveButton.Disabled = _system.GetWaveState() == GuildTowerDefenseSystem.WaveState.InProgress;
    }
    
    private void OnBuildPressed()
    {
        if (_system == null) return;
        
        int selectedIndex = _towerTypeOption.Selected;
        if (selectedIndex >= 0 && selectedIndex < 8)
        {
            var towerType = (GuildTowerDefenseSystem.TowerType)selectedIndex;
            // Build at center of defense area
            Vector2 buildPos = new Vector2(500, 300);
            _system.BuildTower(towerType, buildPos);
            RefreshUI();
        }
    }
    
    private void OnUpgradePressed()
    {
        if (_system == null) return;
        
        int selectedIndex = _towerList.GetSelectedItems().Length > 0 ? _towerList.GetSelectedItems()[0] : -1;
        if (selectedIndex >= 0)
        {
            _system.UpgradeTower(selectedIndex);
            RefreshUI();
        }
    }
    
    private void OnStartWavePressed()
    {
        if (_system == null) return;
        
        _system.StartNextWave();
        RefreshUI();
    }
    
    private void OnClosePressed()
    {
        Hide();
        _visible = false;
    }
}
