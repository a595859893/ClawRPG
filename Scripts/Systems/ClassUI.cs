using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// ClassUI - 职业选择界面
/// 重构自 REQ-075: 移除对 ClassSystem 的直接引用，改为事件驱动解耦
/// </summary>
public partial class ClassUI : Control
{
    // ===== 事件接口（UI → System 通信） =====

    /// <summary>请求刷新职业列表（System 收到后调用 UpdateClassList）</summary>
    public Action OnClassListRefreshRequested;

    /// <summary>请求切换职业（System 收到后处理）</summary>
    public Action<ClassData.ClassType> OnSwitchClassRequested;

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

        GD.Print("[ClassUI] Initialized (event-driven mode)");
    }

    // ===== 公开更新接口（System → UI 通信） =====
    // REQ-075 解耦：UI 不再主动拉取数据，而是等待外部推送

    /// <summary>
    /// 更新职业列表显示（由外部/System 调用）
    /// </summary>
    public void UpdateClassList(Dictionary<ClassData.ClassType, ClassData> classes)
    {
        // 清除现有项
        foreach (var child in _classGrid.GetChildren())
        {
            child.QueueFree();
        }

        if (classes == null) return;

        foreach (var kvp in classes)
        {
            var classData = kvp.Value;
            var button = new Button
            {
                Text = $"[{GetTierName(classData.Tier)}] {classData.Name}",
                CustomMinimumSize = new Vector2(0, 45)
            };
            button.Pressed += () => OnClassSelected(classData);
            _classGrid.AddChild(button);
        }
    }

    /// <summary>
    /// 更新选中职业详情（由外部/System 调用）
    /// </summary>
    public void UpdateSelectedClassDetails(ClassData classData, ClassData advancedData)
    {
        _selectedClass = classData;

        _classNameLabel.Text = classData.Name;
        _tierLabel.Text = $"阶级: {GetTierName(classData.Tier)} (需求等级: {classData.LevelRequired})";
        _descriptionLabel.Text = classData.Description;

        var stats = $"生命+{classData.BaseHealthBonus} 攻击+{classData.BaseAttackBonus} 防御+{classData.BaseDefenseBonus}\n" +
                     $"魔法+{classData.BaseMagicBonus} 速度+{classData.BaseSpeedBonus} 幸运+{classData.BaseLuckBonus}";

        if (classData.AdvancedClass.HasValue && advancedData != null)
        {
            stats += $"\n\n进阶职业: {advancedData.Name}";
        }

        if (classData.PassiveSkills.Count > 0)
            stats += $"\n\n被动技能: {classData.PassiveSkills.Count}个";
        if (classData.ActiveSkills.Count > 0)
            stats += $"\n主动技能: {classData.ActiveSkills.Count}个";

        _bonusStatsLabel.Text = stats;
    }

    /// <summary>
    /// 更新当前职业信息（由外部/System 调用）
    /// </summary>
    public void UpdateCurrentClassDisplay(ClassData currentClass, int level, int exp, int expToNext,
        int healthBonus, int attackBonus, int defenseBonus, int magicBonus, int speedBonus, int luckBonus)
    {
        if (currentClass == null) return;

        _levelLabel.Text = $"等级: {level}";

        if (expToNext > 0)
            _expLabel.Text = $"经验: {exp}/{exp + expToNext}";
        else
            _expLabel.Text = "经验: 满级";

        if (expToNext > 0)
        {
            _expProgressBar.MaxValue = exp + expToNext;
            _expProgressBar.Value = exp;
        }

        _statsLabel.Text = $"属性加成: 生命+{healthBonus} 攻击+{attackBonus} " +
                          $"防御+{defenseBonus} 魔法+{magicBonus} " +
                          $"速度+{speedBonus} 幸运+{luckBonus}";
    }

    // ===== 事件处理（转发到外部） =====

    /// <summary>
    /// Handle class selected — 请求外部提供详情数据
    /// </summary>
    private void OnClassSelected(ClassData classData)
    {
        // 通过事件请求，而不是直接调用 System
        OnClassListRefreshRequested?.Invoke();
    }

    /// <summary>
    /// Handle switch class button pressed
    /// </summary>
    private void OnSwitchClassPressed()
    {
        if (_selectedClass == null) return;

        // 通过事件请求，而不是直接调用 System
        OnSwitchClassRequested?.Invoke(_selectedClass.Type);
    }

    public void ToggleUI()
    {
        _isVisible = !_isVisible;

        if (_isVisible)
        {
            Show();
            // 通过事件请求刷新数据
            OnClassListRefreshRequested?.Invoke();
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
            // Input already handled by this callback
        }
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
}
