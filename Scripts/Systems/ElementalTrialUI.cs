using Godot;
using System;
using System.Collections.Generic;

public class ElementalTrialUI : Control
{
    private VBoxContainer _mainContainer;
    private HBoxContainer _headerContainer;
    private Label _titleLabel;
    private Button _closeButton;
    
    private TabContainer _tabContainer;
    private VBoxContainer _trialsListContainer;
    private VBoxContainer _infoContainer;
    
    private Label _trialNameLabel;
    private Label _trialDescriptionLabel;
    private Label _trialDifficultyLabel;
    private Label _trialWaveLabel;
    private Label _trialRewardLabel;
    private Label _trialTimeLabel;
    private Label _bestWaveLabel;
    private Button _startTrialButton;
    
    private Label _currentTrialLabel;
    private Label _waveLabel;
    private Label _timeLabel;
    private ProgressBar _timeProgressBar;
    private Button _retreatButton;
    
    private ElementalTrialData _selectedTrial;
    private bool _isInTrial;

    public override void _Ready()
    {
        Visible = false; 
        _isInTrial = false; 
        
        CreateUI();
        ConnectSignals();
        
        ElementalTrialSystem.Instance.TrialStarted.Connect(OnTrialStarted);
        ElementalTrialSystem.Instance.TrialCompleted.Connect(OnTrialCompleted);
        ElementalTrialSystem.Instance.TrialFailed.Connect(OnTrialFailed);
        ElementalTrialSystem.Instance.WaveCompleted.Connect(OnWaveCompleted);
        ElementalTrialSystem.Instance.TrialUnlocked.Connect(OnTrialUnlocked);
    }

    private void CreateUI()
    {
        // Main panel
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainContainer.CustomMinimumSize = new Vector2(800, 600);
        AddChild(_mainContainer);

        // Header
        _headerContainer = new HBoxContainer();
        _mainContainer.AddChild(_headerContainer);
        
        _titleLabel = new Label();
        _titleLabel.Text = "元素试炼";
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _headerContainer.AddChild(_titleLabel);
        
        _headerContainer.AddChild(new Control() { CustomMinimumSize = new Vector2(400, 0) }); // Spacer
        
        _closeButton = new Button();
        _closeButton.Text = "×";
        _closeButton.CustomMinimumSize = new Vector2(40, 40);
        _headerContainer.AddChild(_closeButton);

        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SetHExpand(ExpandMode.Fill);
        _tabContainer.SetVExpand(ExpandMode.Fill);
        _mainContainer.AddChild(_tabContainer);

        // Trials list tab
        _trialsListContainer = new VBoxContainer();
        _trialsListContainer.SetName("试炼列表");
        _tabContainer.AddChild(_trialsListContainer);
        
        CreateTrialsList();

        // Trial info tab
        _infoContainer = new VBoxContainer();
        _infoContainer.SetName("试炼详情");
        _tabContainer.AddChild(_infoContainer);
        
        CreateTrialInfoPanel();

        // Active trial panel (hidden by default)
        CreateActiveTrialPanel();
    }

    private void CreateTrialsList()
    {
        var scrollContainer = new ScrollContainer();
        scrollContainer.SetHExpand(ExpandMode.Fill);
        scrollContainer.SetVExpand(ExpandMode.Fill);
        _trialsListContainer.AddChild(scrollContainer);
        
        var listContainer = new VBoxContainer();
        listContainer.SetHExpand(ExpandMode.Fill);
        scrollContainer.AddChild(listContainer);
        
        // Add trial buttons
        var trials = ElementalTrialSystem.Instance.GetAllTrials();
        foreach (var trial in trials)
        {
            var trialButton = new Button();
            trialButton.Text = $"{GetTrialTypeIcon(trial.Type)} {trial.TrialName} {(trial.IsCompleted ? "✓" : "")}";
            trialButton.TooltipText = $"{trial.Description}\n推荐等级: {trial.RecommendedLevel}\n波次: {trial.WaveCount}";
            
            if (!trial.IsUnlocked)
            {
                trialButton.Disabled = true;
                trialButton.Text = "🔒 " + trial.TrialName;
            }
            else
            {
                trialButton.Modulate = GetDifficultyColor(trial.Difficulty);
            }
            
            trialButton.Connect("pressed", this, nameof(OnTrialButtonPressed), new Godot.Collections.Array { trial.TrialId });
            listContainer.AddChild(trialButton);
        }
    }

    private void CreateTrialInfoPanel()
    {
        _trialNameLabel = new Label();
        _trialNameLabel.AddThemeFontSizeOverride("font_size", 20);
        _infoContainer.AddChild(_trialNameLabel);
        
        _trialDescriptionLabel = new Label();
        _infoContainer.AddChild(_trialDescriptionLabel);
        
        _trialDifficultyLabel = new Label();
        _infoContainer.AddChild(_trialDifficultyLabel);
        
        _trialWaveLabel = new Label();
        _infoContainer.AddChild(_trialWaveLabel);
        
        _trialRewardLabel = new Label();
        _infoContainer.AddChild(_trialRewardLabel);
        
        _trialTimeLabel = new Label();
        _infoContainer.AddChild(_trialTimeLabel);
        
        _bestWaveLabel = new Label();
        _bestWaveLabel.AddThemeFontSizeOverride("font_size", 18);
        _infoContainer.AddChild(_bestWaveLabel);
        
        _startTrialButton = new Button();
        _startTrialButton.Text = "开始试炼";
        _startTrialButton.CustomMinimumSize = new Vector2(200, 50);
        _startTrialButton.Connect("pressed", this, nameof(OnStartTrialPressed));
        _infoContainer.AddChild(_startTrialButton);
    }

    private void CreateActiveTrialPanel()
    {
        // This panel shows during active trial
        var activePanel = new PanelContainer();
        activePanel.SetName("战斗中");
        activePanel.Visible = false; 
        _tabContainer.AddChild(activePanel);
        
        var activeContainer = new VBoxContainer();
        activePanel.AddChild(activeContainer);
        
        _currentTrialLabel = new Label();
        _currentTrialLabel.AddThemeFontSizeOverride("font_size", 24);
        _currentTrialLabel.Align = Label.AlignEnum.Center;
        activeContainer.AddChild(_currentTrialLabel);
        
        _waveLabel = new Label();
        _waveLabel.AddThemeFontSizeOverride("font_size", 18);
        _waveLabel.Align = Label.AlignEnum.Center;
        activeContainer.AddChild(_waveLabel);
        
        _timeLabel = new Label();
        _timeLabel.AddThemeFontSizeOverride("font_size", 16);
        _timeLabel.Align = Label.AlignEnum.Center;
        activeContainer.AddChild(_timeLabel);
        
        _timeProgressBar = new ProgressBar();
        _timeProgressBar.SetHExpand(ExpandMode.Fill);
        _timeProgressBar.MinValue = 0;
        _timeProgressBar.MaxValue = 100;
        _timeProgressBar.Value = 100;
        activeContainer.AddChild(_timeProgressBar);
        
        _retreatButton = new Button();
        _retreatButton.Text = "撤退 (放弃当前试炼)";
        _retreatButton.Connect("pressed", this, nameof(OnRetreatPressed));
        activeContainer.AddChild(_retreatButton);
    }

    private void ConnectSignals()
    {
        _closeButton.Connect("pressed", this, nameof(OnClosePressed));
    }

    private void OnTrialButtonPressed(string trialId)
    {
        _selectedTrial = ElementalTrialSystem.Instance.GetTrial(trialId);
        if (_selectedTrial == null) return;
        
        UpdateTrialInfo();
        _tabContainer.CurrentTab = 1; // Switch to info tab
    }

    private void UpdateTrialInfo()
    {
        if (_selectedTrial == null) return;
        
        _trialNameLabel.Text = $"{GetTrialTypeIcon(_selectedTrial.Type)} {_selectedTrial.TrialName}";
        _trialDescriptionLabel.Text = _selectedTrial.Description;
        _trialDifficultyLabel.Text = $"难度: {GetDifficultyText(_selectedTrial.Difficulty)}";
        _trialWaveLabel.Text = $"波次: {_selectedTrial.WaveCount} 波";
        _trialRewardLabel.Text = $"奖励: {_selectedTrial.GoldReward} 金币, {_selectedTrial.ExpReward} 经验";
        _trialRewardLabel.Text += $"\n物品: {string.Join(", ", _selectedTrial.ItemRewards)}";
        _trialTimeLabel.Text = $"时间限制: {_selectedTrial.TimeLimit} 秒";
        
        if (_selectedTrial.IsCompleted)
        {
            _bestWaveLabel.Text = $"✓ 已完成! 最佳波次: {_selectedTrial.BestWave}";
            _bestWaveLabel.Modulate = Colors.Green;
        }
        else if (_selectedTrial.BestWave > 0)
        {
            _bestWaveLabel.Text = $"最佳波次: {_selectedTrial.BestWave}";
            _bestWaveLabel.Modulate = Colors.Yellow;
        }
        else
        {
            _bestWaveLabel.Text = "尚未挑战";
            _bestWaveLabel.Modulate = Colors.Gray;
        }
    }

    private void OnStartTrialPressed()
    {
        if (_selectedTrial == null) return;
        
        if (ElementalTrialSystem.Instance.StartTrial(_selectedTrial.TrialId))
        {
            Visible = false;  // Hide during trial
        }
    }

    private void OnTrialStarted(string trialId, int wave, int timeRemaining)
    {
        _isInTrial = true;
        var trial = ElementalTrialSystem.Instance.GetTrial(trialId);
        if (trial == null) return;
        
        _currentTrialLabel.Text = trial.TrialName;
    }

    private void OnTrialCompleted(string trialId, int waves, int timeRemaining)
    {
        _isInTrial = false; 
        RefreshTrialsList();
        
        var trial = ElementalTrialSystem.Instance.GetTrial(trialId);
        if (trial != null)
        {
            GD.Print($"Trial completed: {trial.TrialName}, Waves: {waves}, Time remaining: {timeRemaining}s");
        }
    }

    private void OnTrialFailed(string trialId, int wave, string reason)
    {
        _isInTrial = false; 
        GD.Print($"Trial failed: {trialId}, Wave: {wave}, Reason: {reason}");
    }

    private void OnWaveCompleted(int currentWave, int totalWaves)
    {
        _waveLabel.Text = $"波次: {currentWave} / {totalWaves}";
    }

    private void OnTrialUnlocked(string trialId)
    {
        RefreshTrialsList();
    }

    private void OnRetreatPressed()
    {
        // Player retreats - this counts as failure
        ElementalTrialSystem.Instance.OnPlayerDefeated();
    }

    private void OnClosePressed()
    {
        Visible = false; 
    }

    public override void _Process(float delta)
    {
        if (_isInTrial)
        {
            var timeRemaining = ElementalTrialSystem.Instance.GetTimeRemaining();
            var currentWave = ElementalTrialSystem.Instance.GetCurrentWave();
            var trialId = ElementalTrialSystem.Instance.GetCurrentTrialId();
            var trial = ElementalTrialSystem.Instance.GetTrial(trialId);
            
            if (trial != null)
            {
                _timeLabel.Text = $"剩余时间: {timeRemaining:F0} 秒";
                _timeProgressBar.MaxValue = trial.TimeLimit;
                _timeProgressBar.Value = timeRemaining;
                _waveLabel.Text = $"波次: {currentWave} / {trial.WaveCount}";
            }
        }
    }

    private void RefreshTrialsList()
    {
        // Remove old list and recreate
        foreach (var child in _trialsListContainer.GetChildren())
        {
            child.QueueFree();
        }
        CreateTrialsList();
    }

    private string GetTrialTypeIcon(ElementalTrialData.TrialType type)
    {
        switch (type)
        {
            case ElementalTrialData.TrialType.FireTrial: return "🔥";
            case ElementalTrialData.TrialType.IceTrial: return "❄️";
            case ElementalTrialData.TrialType.LightningTrial: return "⚡";
            case ElementalTrialData.TrialType.DarkTrial: return "🌑";
            case ElementalTrialData.TrialType.HolyTrial: return "✨";
            case ElementalTrialData.TrialType.NatureTrial: return "🌿";
            case ElementalTrialData.TrialType.MixedTrial: return "🌈";
            default: return "❓";
        }
    }

    private Color GetDifficultyColor(ElementalTrialData.TrialDifficulty difficulty)
    {
        switch (difficulty)
        {
            case ElementalTrialData.TrialDifficulty.Easy: return Colors.Green;
            case ElementalTrialData.TrialDifficulty.Normal: return Colors.Blue;
            case ElementalTrialData.TrialDifficulty.Hard: return Colors.Orange;
            case ElementalTrialData.TrialDifficulty.Epic: return Colors.Purple;
            case ElementalTrialData.TrialDifficulty.Legendary: return Colors.Red;
            default: return Colors.White;
        }
    }

    private string GetDifficultyText(ElementalTrialData.TrialDifficulty difficulty)
    {
        switch (difficulty)
        {
            case ElementalTrialData.TrialDifficulty.Easy: return "简单";
            case ElementalTrialData.TrialDifficulty.Normal: return "普通";
            case ElementalTrialData.TrialDifficulty.Hard: return "困难";
            case ElementalTrialData.TrialDifficulty.Epic: return "史诗";
            case ElementalTrialData.TrialDifficulty.Legendary: return "传说";
            default: return "未知";
        }
    }

    public void Toggle()
    {
        Visible = !Visible;
        if (Visible)
        {
            RefreshTrialsList();
        }
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("ui_cancel") && Visible)
        {
            Visible = false; 
        }
    }
}
