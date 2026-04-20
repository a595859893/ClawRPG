using Godot;
using System;
using System.Collections.Generic;

public partial class TutorialUI : Control
{
    private TutorialSystem _tutorialSystem;
    private Control _tutorialPanel;
    private Label _titleLabel;
    private Label _stepLabel;
    private Label _descriptionLabel;
    private ProgressBar _progressBar;
    private Button _skipButton;
    private Button _completeButton;
    private Button _closeButton;
    private VBoxContainer _tutorialListContainer;
    private TabContainer _tabContainer;

    private bool _isVisible = false;

    public override void _Ready()
    {
        _tutorialSystem = GetNode<TutorialSystem>("/root/TutorialSystem");
        SetupUI();
        GD.Print("[TutorialUI] 游戏教程UI已初始化");
    }

    private void SetupUI()
    {
        // Main panel
        _tutorialPanel = new Control();
        _tutorialPanel.Name = "TutorialPanel";
        _tutorialPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _tutorialPanel.CustomMinimumSize = new Vector2(600, 500);
        AddChild(_tutorialPanel);

        // Background
        var bgPanel = new Panel();
        bgPanel.Name = "Background";
        bgPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        bgPanel.Modulate = new Color(0, 0, 0, 0.8f);
        _tutorialPanel.AddChild(bgPanel);

        // Content container
        var contentContainer = new VBoxContainer();
        contentContainer.Name = "Content";
        contentContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        contentContainer.AddThemeConstantOverride("separation", 20);
        _tutorialPanel.AddChild(contentContainer);

        // Header
        var header = new HBoxContainer();
        header.Name = "Header";
        header.AddThemeConstantOverride("separation", 10);
        contentContainer.AddChild(header);

        _titleLabel = new Label();
        _titleLabel.Name = "Title";
        _titleLabel.Text = "游戏教程";
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        header.AddChild(_titleLabel);

        header.AddChild(new Control() { Name = "Spacer", SizeFlagsHorizontal = Control.SizeFlags.Expand });

        _closeButton = new Button();
        _closeButton.Name = "CloseButton";
        _closeButton.Text = "X";
        _closeButton.CustomMinimumSize = new Vector2(30, 30);
        _closeButton.Pressed += OnClosePressed;
        header.AddChild(_closeButton);

        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.Name = "Tabs";
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
        contentContainer.AddChild(_tabContainer);

        // Current Tutorial Tab
        var currentTab = new VBoxContainer();
        currentTab.Name = "CurrentTutorial";
        _tabContainer.AddChild(currentTab);
        _tabContainer.SetTabTitle(0, "当前教程");

        var currentTitle = new Label();
        currentTitle.Name = "CurrentTitle";
        currentTitle.Text = "当前教程";
        currentTitle.AddThemeFontSizeOverride("font_size", 18);
        currentTitle.HorizontalAlignment = HorizontalAlignment.Center;
        currentTab.AddChild(currentTitle);

        _stepLabel = new Label();
        _stepLabel.Name = "StepLabel";
        _stepLabel.Text = "步骤 0/0";
        _stepLabel.HorizontalAlignment = HorizontalAlignment.Center;
        currentTab.AddChild(_stepLabel);

        _progressBar = new ProgressBar();
        _progressBar.Name = "Progress";
        _progressBar.MinValue = 0;
        _progressBar.MaxValue = 100;
        _progressBar.Value = 0;
        currentTab.AddChild(_progressBar);

        _descriptionLabel = new Label();
        _descriptionLabel.Name = "Description";
        _descriptionLabel.Text = "暂无进行中的教程";
        _descriptionLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _descriptionLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        currentTab.AddChild(_descriptionLabel);

        var buttonContainer = new HBoxContainer();
        buttonContainer.Name = "Buttons";
        buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
        buttonContainer.AddThemeConstantOverride("separation", 20);
        currentTab.AddChild(buttonContainer);

        _skipButton = new Button();
        _skipButton.Name = "SkipButton";
        _skipButton.Text = "跳过此步骤";
        _skipButton.Pressed += OnSkipPressed;
        buttonContainer.AddChild(_skipButton);

        _completeButton = new Button();
        _completeButton.Name = "CompleteButton";
        _completeButton.Text = "完成步骤";
        _completeButton.Pressed += OnCompletePressed;
        buttonContainer.AddChild(_completeButton);

        // All Tutorials Tab
        var allTab = new ScrollContainer();
        allTab.Name = "AllTutorials";
        _tabContainer.AddChild(allTab);
        _tabContainer.SetTabTitle(1, "所有教程");

        _tutorialListContainer = new VBoxContainer();
        _tutorialListContainer.Name = "TutorialList";
        _tutorialListContainer.AddThemeConstantOverride("separation", 10);
        allTab.AddChild(_tutorialListContainer);

        RefreshTutorialList();

        // Statistics Tab
        var statsTab = new VBoxContainer();
        statsTab.Name = "Statistics";
        _tabContainer.AddChild(statsTab);
        _tabContainer.SetTabTitle(2, "统计");

        RefreshStatistics(statsTab);

        _tutorialPanel.Visible = false;
    }

    private void RefreshTutorialList()
    {
        foreach (var child in _tutorialListContainer.GetChildren())
        {
            child.QueueFree();
        }

        var tutorials = _tutorialSystem.GetAllTutorials();
        foreach (var tutorial in tutorials)
        {
            var itemContainer = new HBoxContainer();
            itemContainer.Name = "Tutorial_" + tutorial.TutorialId;

            var iconLabel = new Label();
            iconLabel.Text = tutorial.Icon + " ";
            iconLabel.AddThemeFontSizeOverride("font_size", 20);
            itemContainer.AddChild(iconLabel);

            var titleLabel = new Label();
            titleLabel.Text = tutorial.Title;
            titleLabel.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            itemContainer.AddChild(titleLabel);

            var statusLabel = new Label();
            bool completed = _tutorialSystem.IsTutorialCompleted(tutorial.TutorialId);
            bool inProgress = _tutorialSystem.IsTutorialInProgress(tutorial.TutorialId);
            
            if (completed)
                statusLabel.Text = "✓ 已完成";
            else if (inProgress)
                statusLabel.Text = "🔄 进行中";
            else
                statusLabel.Text = "○ 未开始";
                
            itemContainer.AddChild(statusLabel);

            var startButton = new Button();
            startButton.Text = "开始";
            startButton.Disabled = completed || inProgress;
            startButton.Pressed += () => OnStartTutorialPressed(tutorial.TutorialId);
            itemContainer.AddChild(startButton);

            _tutorialListContainer.AddChild(itemContainer);
        }
    }

    private void RefreshStatistics(VBoxContainer statsTab)
    {
        foreach (var child in statsTab.GetChildren())
        {
            child.QueueFree();
        }

        var stats = _tutorialSystem.GetStatistics();
        
        var titleLabel = new Label();
        titleLabel.Name = "StatsTitle";
        titleLabel.Text = "教程统计";
        titleLabel.AddThemeFontSizeOverride("font_size", 18);
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        statsTab.AddChild(titleLabel);

        var statsContainer = new GridContainer();
        statsContainer.Name = "StatsGrid";
        statsContainer.Columns = 2;
        statsContainer.AddThemeConstantOverride("h_separation", 20);
        statsContainer.AddThemeConstantOverride("v_separation", 10);
        statsTab.AddChild(statsContainer);

        AddStatRow(statsContainer, "已完成教程:", stats.GetValueOrDefault("TotalCompleted", 0).ToString() + "/" + stats.GetValueOrDefault("TotalAvailable", 0));
        AddStatRow(statsContainer, "进行中:", stats.GetValueOrDefault("InProgress", 0).ToString());
        AddStatRow(statsContainer, "使用提示:", stats.GetValueOrDefault("HintsUsed", 0).ToString());

        var categories = _tutorialSystem.GetCategories();
        foreach (var category in categories)
        {
            int completed = stats.GetValueOrDefault("Category_" + category + "_Completed", 0);
            int total = stats.GetValueOrDefault("Category_" + category + "_Total", 0);
            AddStatRow(statsContainer, category + ":", completed + "/" + total);
        }

        var resetButton = new Button();
        resetButton.Name = "ResetButton";
        resetButton.Text = "重置所有教程";
        resetButton.Pressed += OnResetAllPressed;
        statsTab.AddChild(resetButton);
    }

    private void AddStatRow(GridContainer container, string label, string value)
    {
        var labelNode = new Label();
        labelNode.Text = label;
        container.AddChild(labelNode);

        var valueNode = new Label();
        valueNode.Text = value;
        valueNode.HorizontalAlignment = HorizontalAlignment.Right;
        container.AddChild(valueNode);
    }

    public void ToggleTutorialUI()
    {
        _isVisible = !_isVisible;
        _tutorialPanel.Visible = _isVisible;
        
        if (_isVisible)
        {
            UpdateCurrentTutorial();
            RefreshTutorialList();
            var statsTab = _tabContainer.GetNode<VBoxContainer>("Statistics");
            if (statsTab != null)
                RefreshStatistics(statsTab);
        }
    }

    private void UpdateCurrentTutorial()
    {
        if (_tutorialSystem.IsAnyTutorialActive())
        {
            var step = _tutorialSystem.GetCurrentStep();
            var tutorial = _tutorialSystem.GetTutorial(_tutorialSystem.GetCurrentTutorialId());
            
            if (tutorial != null)
            {
                _titleLabel.Text = tutorial.Title;
                _stepLabel.Text = "步骤 " + (_tutorialSystem.GetCurrentStepIndex() + 1) + "/" + _tutorialSystem.GetTotalSteps();
                _progressBar.Value = _tutorialSystem.GetStepProgress() * 100;
                
                if (step != null)
                {
                    _descriptionLabel.Text = step.Title + "\n\n" + step.Description;
                }
            }
        }
        else
        {
            _titleLabel.Text = "游戏教程";
            _stepLabel.Text = "无进行中的教程";
            _progressBar.Value = 0;
            _descriptionLabel.Text = "从下方选择一个教程开始学习，或等待系统自动触发教程。";
        }

        _skipButton.Disabled = !_tutorialSystem.IsAnyTutorialActive();
        _completeButton.Disabled = !_tutorialSystem.IsAnyTutorialActive();
    }

    private void OnClosePressed()
    {
        ToggleTutorialUI();
    }

    private void OnSkipPressed()
    {
        _tutorialSystem.SkipStep();
        UpdateCurrentTutorial();
    }

    private void OnCompletePressed()
    {
        _tutorialSystem.CompleteCurrentStep();
        UpdateCurrentTutorial();
        RefreshTutorialList();
        
        var statsTab = _tabContainer.GetNode<VBoxContainer>("Statistics");
        if (statsTab != null)
            RefreshStatistics(statsTab);
    }

    private void OnStartTutorialPressed(string tutorialId)
    {
        _tutorialSystem.StartTutorial(tutorialId);
        UpdateCurrentTutorial();
        RefreshTutorialList();
        _tabContainer.CurrentTab = 0;
    }

    private void OnResetAllPressed()
    {
        _tutorialSystem.ResetAllTutorials();
        UpdateCurrentTutorial();
        RefreshTutorialList();
        
        var statsTab = _tabContainer.GetNode<VBoxContainer>("Statistics");
        if (statsTab != null)
            RefreshStatistics(statsTab);
    }

    public override void _Process(double delta)
    {
        if (_isVisible && _tutorialSystem.IsAnyTutorialActive())
        {
            UpdateCurrentTutorial();
        }
    }
}
