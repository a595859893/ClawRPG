using Godot;
using System;
using System.Collections.Generic;

public partial class SealedTowerUI : Control
{
    private Label _titleLabel;
    private Label _floorLabel;
    private Label _statsLabel;
    private Label _boonsLabel;
    private Label _cursesLabel;
    private Label _healthBar;
    private Button _startButton;
    private Button _exitButton;
    private Button _closeButton;
    private VBoxContainer _infoPanel;
    private bool _isVisible = false;
    
    public override void _Ready()
    {
        Visible = false;
        _isVisible = false;
        
        // Create main panel
        var mainPanel = new PanelContainer
        {
            AnchorLeft = 0.5f,
            AnchorRight = 0.5f,
            AnchorTop = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = -300,
            OffsetRight = 300,
            OffsetTop = -250,
            OffsetBottom = 250
        };
        AddChild(mainPanel);
        
        var mainVBox = new VBoxContainer { CustomMinimumSize = new Vector2(600, 500) };
        mainPanel.AddChild(mainVBox);
        
        // Title
        _titleLabel = new Label
        {
            Text = "🏰 Sealed Tower",
            Align = Label.AlignEnum.Center,
            CustomMinimumSize = new Vector2(0, 50)
        };
        _titleLabel.AddThemeFontSizeOverride("font_size", 28);
        mainVBox.AddChild(_titleLabel);
        
        // Separator
        var hsep1 = new HSeparator();
        mainVBox.AddChild(hsep1);
        
        // Info panel
        _infoPanel = new VBoxContainer { CustomMinimumSize = new Vector2(0, 300) };
        mainVBox.AddChild(_infoPanel);
        
        // Floor info
        _floorLabel = new Label
        {
            Text = "Current Floor: --",
            Align = Label.AlignEnum.Center
        };
        _floorLabel.AddThemeFontSizeOverride("font_size", 20);
        _infoPanel.AddChild(_floorLabel);
        
        // Stats
        _statsLabel = new Label
        {
            Text = "Stats will appear here",
            Align = Label.AlignEnum.Center
        };
        _statsLabel.AddThemeFontSizeOverride("font_size", 16);
        _infoPanel.AddChild(_statsLabel);
        
        // Boons
        _boonsLabel = new Label
        {
            Text = "Active Boons: None",
            Align = Label.AlignEnum.Center
        };
        _infoPanel.AddChild(_boonsLabel);
        
        // Curses
        _cursesLabel = new Label
        {
            Text = "Active Curses: None",
            Align = Label.AlignEnum.Center
        };
        _infoPanel.AddChild(_cursesLabel);
        
        // Health bar
        _healthBar = new Label
        {
            Text = "Health: --/--",
            Align = Label.AlignEnum.Center
        };
        _healthBar.AddThemeFontSizeOverride("font_size", 18);
        _infoPanel.AddChild(_healthBar);
        
        // Separator
        var hsep2 = new HSeparator();
        mainVBox.AddChild(hsep2);
        
        // Buttons
        var buttonPanel = new HBoxContainer { CustomMinimumSize = new Vector2(0, 60) };
        mainVBox.AddChild(buttonPanel);
        
        _startButton = new Button
        {
            Text = "Start Run",
            CustomMinimumSize = new Vector2(150, 50)
        };
        _startButton.Pressed += OnStartPressed;
        buttonPanel.AddChild(_startButton);
        
        buttonPanel.AddChild(new Control { CustomMinimumSize = new Vector2(20, 0) });
        
        _exitButton = new Button
        {
            Text = "Exit Tower",
            CustomMinimumSize = new Vector2(150, 50)
        };
        _exitButton.Pressed += OnExitPressed;
        _exitButton.Disabled = true;
        buttonPanel.AddChild(_exitButton);
        
        buttonPanel.AddChild(new Control { CustomMinimumSize = new Vector2(20, 0) });
        
        _closeButton = new Button
        {
            Text = "Close",
            CustomMinimumSize = new Vector2(150, 50)
        };
        _closeButton.Pressed += OnClosePressed;
        buttonPanel.AddChild(_closeButton);
        
        // Connect signals
        if (SealedTowerManager.Instance != null)
        {
            SealedTowerManager.Instance.RunStarted += OnRunStarted;
            SealedTowerManager.Instance.RunEnded += OnRunEnded;
            SealedTowerManager.Instance.FloorChanged += OnFloorChanged;
            SealedTowerManager.Instance.BoonAcquired += OnBoonAcquired;
            SealedTowerManager.Instance.CurseAcquired += OnCurseAcquired;
        }
        
        UpdateDisplay();
    }
    
    public void Toggle()
    {
        _isVisible = !_isVisible;
        Visible = _isVisible;
        
        if (_isVisible)
        {
            UpdateDisplay();
        }
    }
    
    private void UpdateDisplay()
    {
        if (SealedTowerManager.Instance == null) return;
        
        var stats = SealedTowerManager.Instance.GetTowerStats();
        
        // Floor info
        if (SealedTowerManager.Instance.IsInTower)
        {
            var floorData = SealedTowerManager.Instance.GetCurrentFloorData();
            _floorLabel.Text = $"�Floor {SealedTowerManager.Instance.CurrentFloor}: {floorData?.Name ?? "Unknown"}";
            
            _statsLabel.Text = $"Enemies: {stats["enemies_defeated"]} | Gold: {stats["gold_earned"]} | EXP: {stats["exp_earned"]}";
            
            _healthBar.Text = $"❤️ Health: {SealedTowerManager.Instance.CurrentHealth}/{SealedTowerManager.Instance.MaxHealth}";
            
            _startButton.Disabled = true;
            _exitButton.Disabled = false;
        }
        else
        {
            _floorLabel.Text = "Ready to Enter";
            
            float winRate = stats["total_runs"] > 0 ? (float)stats["wins"] / stats["total_runs"] * 100 : 0;
            _statsLabel.Text = $"Total Runs: {stats["total_runs"]} | Wins: {stats["wins"]} | Win Rate: {winRate:F1}%\nMax Floor: {stats["max_floor"]}";
            
            _healthBar.Text = "❤️ Health: --/--";
            
            _startButton.Disabled = false;
            _exitButton.Disabled = true;
        }
        
        // Boons
        var boons = SealedTowerManager.Instance.AcquiredBoons;
        if (boons.Count > 0)
        {
            _boonsLabel.Text = $"✨ Active Boons ({boons.Count}): {string.Join(", ", boons)}";
        }
        else
        {
            _boonsLabel.Text = "✨ Active Boons: None";
        }
        
        // Curses
        var curses = SealedTowerManager.Instance.ActiveCurses;
        if (curses.Count > 0)
        {
            _cursesLabel.Text = $"💀 Active Curses ({curses.Count}): {string.Join(", ", curses)}";
        }
        else
        {
            _cursesLabel.Text = "💀 Active Curses: None";
        }
    }
    
    private void OnStartPressed()
    {
        if (SealedTowerManager.Instance != null)
        {
            SealedTowerManager.Instance.StartRun();
            UpdateDisplay();
        }
    }
    
    private void OnExitPressed()
    {
        if (SealedTowerManager.Instance != null)
        {
            SealedTowerManager.Instance.ExitTower(false);
            UpdateDisplay();
        }
    }
    
    private void OnClosePressed()
    {
        Toggle();
    }
    
    private void OnRunStarted()
    {
        UpdateDisplay();
    }
    
    private void OnRunEnded(bool victory, int floorsCleared, int enemiesDefeated)
    {
        string result = victory ? "🎉 Victory!" : "💀 Defeat";
        GD.Print($"[SealedTowerUI] Run ended: {result}, Floors: {floorsCleared}, Enemies: {enemiesDefeated}");
        UpdateDisplay();
    }
    
    private void OnFloorChanged(int floor)
    {
        UpdateDisplay();
    }
    
    private void OnBoonAcquired(string boonId)
    {
        UpdateDisplay();
    }
    
    private void OnCurseAcquired(string curseId)
    {
        UpdateDisplay();
    }
    
    public override void _Process(double delta)
    {
        if (_isVisible && SealedTowerManager.Instance?.IsInTower == true)
        {
            _healthBar.Text = $"❤️ Health: {SealedTowerManager.Instance.CurrentHealth}/{SealedTowerManager.Instance.MaxHealth}";
        }
    }
}
