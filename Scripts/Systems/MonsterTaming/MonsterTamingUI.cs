using Godot;
using System;
using System.Collections.Generic;

public partial class MonsterTamingUI : Control
{
    private MonsterTamingSystem _system;
    private Label _titleLabel;
    private Label _statsLabel;
    private VBoxContainer _monsterList;
    private Button _captureButton;
    private Button _trainButton;
    private Button _releaseButton;
    private Label _captureResultLabel;
    
    private int _selectedMonsterId = -1;
    
    public override void _Ready()
    {
        _system = GetNode<MonsterTamingSystem>("/root/Main/MonsterTamingSystem");
        
        // Create main panel
        var panel = new PanelContainer();
        panel.SetAnchor(AnchorPresets.Center);
        panel.CustomMinimumSize = new Vector2(600, 500);
        AddChild(panel);
        
        var mainVBox = new VBoxContainer();
        panel.AddChild(mainVBox);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "Monster Taming System";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        mainVBox.AddChild(_titleLabel);
        
        // Stats
        _statsLabel = new Label();
        _statsLabel.Text = "Loading...";
        mainVBox.AddChild(_statsLabel);
        
        // Monster List
        var scrollContainer = new ScrollContainer();
        scrollContainer.CustomMinimumSize = new Vector2(580, 250);
        mainVBox.AddChild(scrollContainer);
        
        _monsterList = new VBoxContainer();
        scrollContainer.AddChild(_monsterList);
        
        // Buttons
        var buttonBox = new HBoxContainer();
        mainVBox.AddChild(buttonBox);
        
        _captureButton = new Button();
        _captureButton.Text = "Find Monster";
        _captureButton.Pressed += OnCapturePressed;
        buttonBox.AddChild(_captureButton);
        
        _trainButton = new Button();
        _trainButton.Text = "Train";
        _trainButton.Pressed += OnTrainPressed;
        _trainButton.Disabled = true;
        buttonBox.AddChild(_trainButton);
        
        _releaseButton = new Button();
        _releaseButton.Text = "Release";
        _releaseButton.Pressed += OnReleasePressed;
        _releaseButton.Disabled = true;
        buttonBox.AddChild(_releaseButton);
        
        // Result
        _captureResultLabel = new Label();
        _captureResultLabel.HorizontalAlignment = HorizontalAlignment.Center;
        mainVBox.AddChild(_captureResultLabel);
        
        // Close button
        var closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.Pressed += () => Visible = false;
        mainVBox.AddChild(closeButton);
        
        // Initial update
        UpdateUI();
        
        // Hide by default
        Visible = false;
    }
    
    public void UpdateUI()
    {
        var stats = _system.GetStatistics();
        
        _statsLabel.Text = $"Total Tamed: {stats["total_tamed"]} | " +
            $"Capture Rate: {(float)stats["capture_rate"] * 100:F1}% | " +
            $"Legendary: {stats["legendary"]} | Epic: {stats["epic"]}";
        
        // Update monster list
        foreach (Node child in _monsterList.GetChildren())
            child.QueueFree();
        
        var data = _system.GetData();
        foreach (var kvp in data.TamedMonsters)
        {
            var monster = kvp.Value;
            var monsterBtn = new Button();
            monsterBtn.Text = $"#{monster.Id} {monster.Name} [{monster.Rarity}] Lv.{monster.Level} Bond:{monster.BondLevel}";
            monsterBtn.Pressed += () => OnMonsterSelected(monster.Id);
            _monsterList.AddChild(monsterBtn);
        }
    }
    
    private void OnMonsterSelected(int id)
    {
        _selectedMonsterId = id;
        _trainButton.Disabled = false;
        _releaseButton.Disabled = false;
    }
    
    private void OnCapturePressed()
    {
        // Simulate finding and capturing a monster
        var wildMonster = _system.GenerateWildMonster(10); // Player level 10
        
        // Simulate capture at 50% health
        int maxHP = (wildMonster["stats"] as Dictionary<string, int>)["HP"];
        bool success = _system.AttemptCapture(wildMonster, maxHP * 0.5f, maxHP);
        
        if (success)
        {
            _captureResultLabel.Text = $"Success! Captured {wildMonster["rarity"]} {wildMonster["type"]}!";
            _captureResultLabel.Modulate = Colors.Green;
        }
        else
        {
            _captureResultLabel.Text = "Capture failed! The monster escaped.";
            _captureResultLabel.Modulate = Colors.Red;
        }
        
        UpdateUI();
    }
    
    private void OnTrainPressed()
    {
        if (_selectedMonsterId > 0)
        {
            _system.TrainMonster(_selectedMonsterId, 25);
            _system.IncreaseBond(_selectedMonsterId, 1);
            _captureResultLabel.Text = "Monster trained! +25 XP, Bond +1";
            _captureResultLabel.Modulate = Colors.Yellow;
            UpdateUI();
        }
    }
    
    private void OnReleasePressed()
    {
        if (_selectedMonsterId > 0)
        {
            _system.ReleaseMonster(_selectedMonsterId);
            _captureResultLabel.Text = "Monster released.";
            _captureResultLabel.Modulate = Colors.Orange;
            _selectedMonsterId = -1;
            _trainButton.Disabled = true;
            _releaseButton.Disabled = true;
            UpdateUI();
        }
    }
    
    public void Toggle()
    {
        Visible = !Visible;
        if (Visible) UpdateUI();
    }
}
