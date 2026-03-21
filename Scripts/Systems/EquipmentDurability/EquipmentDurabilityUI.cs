using Godot;
using System;
using System.Collections.Generic;

public class EquipmentDurabilityUI : Control
{
    private PanelContainer _mainPanel;
    private VBoxContainer _content;
    private Label _titleLabel;
    private Button _closeButton;
    private ScrollContainer _scrollContainer;
    private VBoxContainer _equipmentList;
    private Label _statisticsLabel;
    private Label _totalCostLabel;
    private Button _repairAllButton;

    private bool _isVisible = false;

    // REQ-058-11: Migrated from Godot 3 .Connect() to C# event
    public event Action<string, int, int> OnDurabilityChangedUI;
    public event Action<string, int> OnEquipmentRepairedUI;

    public override void _Ready()
    {
        Visible = false;
        _mainPanel = new PanelContainer();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(500, 400);
        AddChild(_mainPanel);

        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(8);
        _mainPanel.AddThemeStyleboxOverride("panel", style);

        _content = new VBoxContainer();
        _content.SetAnchorRight(1);
        _content.SetAnchorBottom(1);
        _content.AddThemeConstantOverride("separation", 10);
        _mainPanel.AddChild(_content);

        // 标题栏
        var titleBar = new HBoxContainer();
        titleBar.AddThemeConstantOverride("separation", 10);
        _content.AddChild(titleBar);

        _titleLabel = new Label();
        _titleLabel.Text = "  ⚙ 装备耐久度";
        _titleLabel.AddThemeFontSizeOverride("font_size", 20);
        _titleLabel.AddThemeColorOverride("font_color", new Color(1, 0.9f, 0.7f));
        titleBar.AddChild(_titleLabel);

        titleBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });

        _closeButton = new Button();
        _closeButton.Text = "✕";
        _closeButton.TooltipText = "关闭 (ESC)";
        _closeButton.Pressed += () => ToggleUI();
        titleBar.AddChild(_closeButton);

        // 统计信息
        _statisticsLabel = new Label();
        _statisticsLabel.AddThemeFontSizeOverride("font_size", 14);
        _statisticsLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
        _content.AddChild(_statisticsLabel);

        // 总修理费用
        _totalCostLabel = new Label();
        _totalCostLabel.AddThemeFontSizeOverride("font_size", 16);
        _totalCostLabel.AddThemeColorOverride("font_color", new Color(1, 0.8f, 0.4f));
        _content.AddChild(_totalCostLabel);

        // 装备列表
        _scrollContainer = new ScrollContainer();
        _scrollContainer.SetCustomMinimumSize(new Vector2(0, 250));
        _content.AddChild(_scrollContainer);

        _equipmentList = new VBoxContainer();
        _equipmentList.AddThemeConstantOverride("separation", 5);
        _scrollContainer.AddChild(_equipmentList);

        // 一键修理按钮
        _repairAllButton = new Button();
        _repairAllButton.Text = " 修理所有装备 ";
        _repairAllButton.Pressed += OnRepairAllPressed;
        _content.AddChild(_repairAllButton);

        // 底部说明
        var helpLabel = new Label();
        helpLabel.Text = " 攻击敌人会消耗武器耐久度 • 受到攻击会消耗防具耐久度";
        helpLabel.AddThemeFontSizeOverride("font_size", 12);
        helpLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.6f));
        _content.AddChild(helpLabel);

        // 连接到信号 (REQ-058-11: migrated from Godot 3 .Connect() to C# event +=)
        if (EquipmentDurabilitySystem.Instance != null)
        {
            EquipmentDurabilitySystem.Instance.DurabilityChanged += OnDurabilityChanged; // NEW
            EquipmentDurabilitySystem.Instance.Connect("DurabilityChanged", this, nameof(OnDurabilityChanged)); // TODO: Remove after migration
            EquipmentDurabilitySystem.Instance.EquipmentRepaired += OnEquipmentRepaired; // NEW
            EquipmentDurabilitySystem.Instance.Connect("EquipmentRepaired", this, nameof(OnEquipmentRepaired)); // TODO: Remove after migration
        }

        UpdateUI();
    }

    public void ToggleUI()
    {
        _isVisible = !_isVisible;
        Visible = _isVisible;

        if (_isVisible)
        {
            UpdateUI();
            PlayOpenAnimation();
        }
        else
        {
            PlayCloseAnimation();
        }
    }

    private void PlayOpenAnimation()
    {
        var tween = CreateTween();
        _mainPanel.Modulate = new Color(1, 1, 1, 0);
        _mainPanel.Scale = new Vector2(0.9f, 0.9f);

        tween.SetParallel(true);
        tween.TweenProperty(_mainPanel, "modulate:a", 1.0f, 0.2f);
        tween.TweenProperty(_mainPanel, "scale", new Vector2(1.0f, 1.0f), 0.2f).SetTrans(Tween.TransitionType.Back).SetEasing(Tween.EasingFunction.EaseOut);
    }

    private void PlayCloseAnimation()
    {
        var tween = CreateTween();
        tween.TweenProperty(_mainPanel, "modulate:a", 0.0f, 0.15f);
        tween.TweenProperty(_mainPanel, "scale", new Vector2(0.95f, 0.95f), 0.15f);
    }

    private void UpdateUI()
    {
        // 清空列表
        foreach (Node child in _equipmentList.GetChildren())
        {
            child.QueueFree();
        }

        if (EquipmentDurabilitySystem.Instance == null)
        {
            _statisticsLabel.Text = " 耐久度系统未初始化";
            return;
        }

        var allDurability = EquipmentDurabilitySystem.Instance.GetAllDurability();
        var stats = EquipmentDurabilitySystem.Instance.GetStatistics();

        _statisticsLabel.Text = $" 总修理次数: {stats["total_repairs"]} | 总修理花费: {stats["total_repair_cost"]} 金币";

        int totalCost = EquipmentDurabilitySystem.Instance.GetTotalRepairCost();
        _totalCostLabel.Text = $" 需要修理费用: {totalCost} 金币";
        _repairAllButton.Disabled = totalCost == 0 || (Player.Instance != null && Player.Instance.Gold < totalCost);

        if (allDurability.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = " 当前没有装备耐久度数据";
            emptyLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.7f));
            _equipmentList.AddChild(emptyLabel);
            return;
        }

        foreach (var kvp in allDurability)
        {
            var item = CreateDurabilityItem(kvp.Key, kvp.Value);
            _equipmentList.AddChild(item);
        }
    }

    private Control CreateDurabilityItem(string itemId, EquipmentDurabilityData.EquipmentDurability durability)
    {
        var container = new HBoxContainer();
        container.AddThemeConstantOverride("separation", 10);

        // 耐久度颜色
        Color stateColor = durability.State switch
        {
            EquipmentDurabilityData.DurabilityState.Excellent => new Color(0.4f, 1f, 0.4f),
            EquipmentDurabilityData.DurabilityState.Good => new Color(0.4f, 0.9f, 0.4f),
            EquipmentDurabilityData.DurabilityState.Worn => new Color(1f, 0.9f, 0.4f),
            EquipmentDurabilityData.DurabilityState.Damaged => new Color(1f, 0.6f, 0.4f),
            EquipmentDurabilityData.DurabilityState.Broken => new Color(1f, 0.3f, 0.3f),
            _ => new Color(0.7f, 0.7f, 0.8f)
        };

        // 装备名称
        var nameLabel = new Label();
        nameLabel.Text = $" {itemId}";
        nameLabel.AddThemeFontSizeOverride("font_size", 14);
        nameLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1f));
        nameLabel.CustomMinimumSize = new Vector2(150, 0);
        container.AddChild(nameLabel);

        // 进度条
        var progressBar = new ProgressBar();
        progressBar.CustomMinimumSize = new Vector2(200, 20);
        progressBar.Value = durability.DurabilityPercent * 100;
        progressBar.MaxValue = 100;

        var progressStyle = new StyleBoxFlat();
        progressStyle.BgColor = new Color(0.2f, 0.2f, 0.25f);
        progressStyle.SetCornerRadiusAll(4);
        progressBar.AddThemeStyleboxOverride("background", progressStyle);

        var fillStyle = new StyleBoxFlat();
        fillStyle.BgColor = stateColor;
        fillStyle.SetCornerRadiusAll(4);
        progressBar.AddThemeStyleboxOverride("fill", fillStyle);

        container.AddChild(progressBar);

        // 耐久度数值
        var valueLabel = new Label();
        valueLabel.Text = $"{durability.CurrentDurability}/{durability.MaxDurability}";
        valueLabel.AddThemeFontSizeOverride("font_size", 14);
        valueLabel.AddThemeColorOverride("font_color", stateColor);
        valueLabel.CustomMinimumSize = new Vector2(80, 0);
        container.AddChild(valueLabel);

        // 修理按钮
        var repairButton = new Button();
        repairButton.Text = "修理";
        repairButton.Disabled = durability.CurrentDurability >= durability.MaxDurability;

        int cost = EquipmentDurabilitySystem.Instance.GetRepairCost(itemId);
        repairButton.Disabled = repairButton.Disabled || (Player.Instance != null && Player.Instance.Gold < cost);

        repairButton.Pressed += () => OnRepairButtonPressed(itemId);
        container.AddChild(repairButton);

        // 状态标签
        var stateLabel = new Label();
        string stateText = durability.State switch
        {
            EquipmentDurabilityData.DurabilityState.Excellent => "优秀",
            EquipmentDurabilityData.DurabilityState.Good => "良好",
            EquipmentDurabilityData.DurabilityState.Worn => "磨损",
            EquipmentDurabilityData.DurabilityState.Damaged => "损坏",
            EquipmentDurabilityData.DurabilityState.Broken => "已坏",
            _ => ""
        };
        stateLabel.Text = stateText;
        stateLabel.AddThemeFontSizeOverride("font_size", 12);
        stateLabel.AddThemeColorOverride("font_color", stateColor);
        container.AddChild(stateLabel);

        return container;
    }

    private void OnRepairButtonPressed(string itemId)
    {
        if (EquipmentDurabilitySystem.Instance == null) return;

        int cost = EquipmentDurabilitySystem.Instance.GetRepairCost(itemId);
        bool success = EquipmentDurabilitySystem.Instance.RepairEquipment(itemId, cost);

        if (success)
        {
            UpdateUI();
        }
    }

    private void OnRepairAllPressed()
    {
        if (EquipmentDurabilitySystem.Instance == null) return;

        int totalCost = EquipmentDurabilitySystem.Instance.GetTotalRepairCost();
        bool success = EquipmentDurabilitySystem.Instance.RepairAllEquipment(100);

        if (success)
        {
            UpdateUI();
        }
    }

    private void OnDurabilityChanged(string itemId, int current, int max)
    {
        // REQ-058-11: Invoke new event
        OnDurabilityChangedUI?.Invoke(itemId, current, max);
        UpdateUI();
    }
    
    private void OnEquipmentRepaired(string itemId, int cost)
    {
        // REQ-058-11: Invoke new event
        OnEquipmentRepairedUI?.Invoke(itemId, cost);
        UpdateUI();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Scancode == KeyList.Escape)
        {
            if (_isVisible)
            {
                ToggleUI();
            }
        }
    }
}
