using Godot;
using System;
using System.Collections.Generic;

public class ClassUI : Control
{
    private static ClassUI _instance;
    public static ClassUI Instance => _instance;

    // UI 组件
    private Label _titleLabel;
    private Label _classNameLabel;
    private Label _tierLabel;
    private Label _levelLabel;
    private Label _expLabel;
    private ProgressBar _expProgressBar;
    private Label _statsLabel;
    private GridContainer _classGrid;
    private VBoxContainer _detailPanel;
    private Label _descriptionLabel;
    private Label _bonusStatsLabel;
    private Button _switchClassButton;
    private Button _closeButton;
    
    // 状态
    private bool _isVisible = false;
    private ClassData _selectedClass;

    public override void _Ready()
    {
        _instance = this;
        SetupUI();
        Hide();
    }

    private void SetupUI()
    {
        // 主容器
        var mainPanel = new PanelContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetLeft = 100,
            OffsetTop = 50,
            OffsetRight = -100,
            OffsetBottom = -50,
            CustomMinimumSize = new Vector2(800, 600)
        };
        AddChild(mainPanel);
        
        var mainVBox = new VBoxContainer();
        mainPanel.AddChild(mainVBox);
        
        // 标题栏
        var titleBar = new HBoxContainer();
        mainVBox.AddChild(titleBar);
        
        _titleLabel = new Label
        {
            Text = "职业系统",
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        titleBar.AddChild(_titleLabel);
        
        _closeButton = new Button
        {
            Text = "X",
            CustomMinimumSize = new Vector2(40, 40)
        };
        _closeButton.Pressed += () => ToggleUI();
        titleBar.AddChild(_closeButton);
        
        // 内容区域
        var contentHBox = new HBoxContainer();
        contentHBox.SizeFlagsVertical = SizeFlags.ExpandFill;
        mainVBox.AddChild(contentHBox);
        
        // 左侧 - 职业列表
        var leftPanel = new VBoxContainer();
        leftPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        leftPanel.CustomMinimumSize = new Vector2(300, 0);
        contentHBox.AddChild(leftPanel);
        
        var listLabel = new Label { Text = "选择职业:" };
        listLabel.AddThemeFontSizeOverride("font_size", 18);
        leftPanel.AddChild(listLabel);
        
        _classGrid = new GridContainer();
        _classGrid.Columns = 1;
        _classGrid.SizeFlagsVertical = SizeFlags.ExpandFill;
        leftPanel.AddChild(_classGrid);
        
        // 右侧 - 详情面板
        _detailPanel = new VBoxContainer();
        _detailPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        contentHBox.AddChild(_detailPanel);
        
        _classNameLabel = new Label();
        _classNameLabel.AddThemeFontSizeOverride("font_size", 28);
        _detailPanel.AddChild(_classNameLabel);
        
        _tierLabel = new Label();
        _tierLabel.AddThemeFontSizeOverride("font_size", 18);
        _detailPanel.AddChild(_tierLabel);
        
        _descriptionLabel = new Label();
        _descriptionLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        _descriptionLabel.SizeFlagsVertical = SizeFlags.ExpandFill;
        _detailPanel.AddChild(_descriptionLabel);
        
        var statsTitleLabel = new Label { Text = "属性加成:" };
        statsTitleLabel.AddThemeFontSizeOverride("font_size", 16);
        _detailPanel.AddChild(statsTitleLabel);
        
        _bonusStatsLabel = new Label();
        _detailPanel.AddChild(_bonusStatsLabel);
        
        _switchClassButton = new Button
        {
            Text = "选择此职业",
            CustomMinimumSize = new Vector2(200, 50)
        };
        _switchClassButton.Pressed += OnSwitchClassPressed;
        _detailPanel.AddChild(_switchClassButton);
        
        // 底部 - 当前职业信息
        var bottomPanel = new VBoxContainer();
        mainVBox.AddChild(bottomPanel);
        
        var currentClassTitle = new Label { Text = "当前职业:" };
        currentClassTitle.AddThemeFontSizeOverride("font_size", 16);
        bottomPanel.AddChild(currentClassTitle);
        
        var currentInfoHBox = new HBoxContainer();
        bottomPanel.AddChild(currentInfoHBox);
        
        _levelLabel = new Label { Text = "等级: 1" };
        currentInfoHBox.AddChild(_levelLabel);
        
        _expLabel = new Label { Text = "经验: 0/100" };
        currentInfoHBox.AddChild(_expLabel);
        
        _expProgressBar = new ProgressBar { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _expProgressBar.MinValue = 0;
        _expProgressBar.MaxValue = 100;
        _expProgressBar.Value = 0;
        bottomPanel.AddChild(_expProgressBar);
        
        _statsLabel = new Label { Text = "属性加成: 生命+0 攻击+0 防御+0 魔法+0 速度+0 幸运+0" };
        bottomPanel.AddChild(_statsLabel);
        
        RefreshClassList();
        UpdateCurrentClassInfo();
    }

    private void RefreshClassList()
    {
        // 清除现有项
        foreach (var child in _classGrid.GetChildren())
        {
            child.QueueFree();
        }
        
        if (ClassSystem.Instance == null) return;
        
        var classes = ClassSystem.Instance.GetAllClasses();
        foreach (var kvp in classes)
        {
            var classData = kvp.Value;
            var button = new Button
            {
                Text = $"[{GetTierName(classData.Tier)}] {classData.Name}",
                CustomMinimumSize = new Vector2(0, 45),
                Tag = classData.Type
            };
            button.Pressed += () => OnClassSelected(classData);
            _classGrid.AddChild(button);
        }
    }

    private void OnClassSelected(ClassData classData)
    {
        _selectedClass = classData;
        
        _classNameLabel.Text = classData.Name;
        _tierLabel.Text = $"阶级: {GetTierName(classData.Tier)} (需求等级: {classData.LevelRequired})";
        _descriptionLabel.Text = classData.Description;
        
        var stats = $"生命+{classData.BaseHealthBonus} 攻击+{classData.BaseAttackBonus} 防御+{classData.BaseDefenseBonus}\n" +
                     $"魔法+{classData.BaseMagicBonus} 速度+{classData.BaseSpeedBonus} 幸运+{classData.BaseLuckBonus}";
        
        if (classData.AdvancedClass.HasValue)
        {
            var advancedData = ClassSystem.Instance.GetClassData(classData.AdvancedClass.Value);
            if (advancedData != null)
                stats += $"\n\n进阶职业: {advancedData.Name}";
        }
        
        if (classData.PassiveSkills.Count > 0)
            stats += $"\n\n被动技能: {classData.PassiveSkills.Count}个";
        if (classData.ActiveSkills.Count > 0)
            stats += $"\n主动技能: {classData.ActiveSkills.Count}个";
        
        _bonusStatsLabel.Text = stats;
    }

    private void OnSwitchClassPressed()
    {
        if (_selectedClass == null || ClassSystem.Instance == null) return;
        
        ClassSystem.Instance.SetClass(_selectedClass.Type);
        UpdateCurrentClassInfo();
        RefreshClassList();
    }

    private void UpdateCurrentClassInfo()
    {
        if (ClassSystem.Instance == null) return;
        
        var currentClass = ClassSystem.Instance.GetCurrentClassData();
        if (currentClass == null) return;
        
        _levelLabel.Text = $"等级: {ClassSystem.Instance.ClassLevel}";
        
        int expToNext = ClassSystem.Instance.ExperienceToNextLevel;
        if (expToNext > 0)
            _expLabel.Text = $"经验: {ClassSystem.Instance.ClassExperience}/{ClassSystem.Instance.ExperienceToNextLevel}";
        else
            _expLabel.Text = "经验: 满级";
        
        int maxExp = ClassSystem.Instance.ExperienceToNextLevel;
        if (maxExp > 0)
        {
            _expProgressBar.MaxValue = maxExp;
            _expProgressBar.Value = ClassSystem.Instance.ClassExperience;
        }
        
        _statsLabel.Text = $"属性加成: 生命+{ClassSystem.Instance.HealthBonus} 攻击+{ClassSystem.Instance.AttackBonus} " +
                          $"防御+{ClassSystem.Instance.DefenseBonus} 魔法+{ClassSystem.Instance.MagicBonus} " +
                          $"速度+{ClassSystem.Instance.SpeedBonus} 幸运+{ClassSystem.Instance.LuckBonus}";
    }

    private string GetTierName(ClassData.ClassTier tier)
    {
        switch (tier)
        {
            case ClassData.ClassTier.Novice: return "初级";
            case ClassData.ClassTier.Adept: return "熟练";
            case ClassData.ClassTier.Master: return "大师";
            case ClassData.ClassTier.Legend: return "传奇";
            default: return "未知";
        }
    }

    public void ToggleUI()
    {
        _isVisible = !_isVisible;
        
        if (_isVisible)
        {
            Show();
            RefreshClassList();
            UpdateCurrentClassInfo();
        }
        else
        {
            Hide();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel") && _isVisible)
        {
            ToggleUI();
            GetTree().SetInputAsHandled();
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationReady && ClassSystem.Instance != null)
        {
            UpdateCurrentClassInfo();
        }
    }
}
