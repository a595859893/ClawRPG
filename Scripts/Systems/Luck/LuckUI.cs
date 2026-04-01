using Godot;
using System;
using System.Collections.Generic;

public partial class LuckUI : Control
{
    private Label _luckValueLabel;
    private Label _baseLuckLabel;
    private VBoxContainer _modifiersContainer;
    private VBoxContainer _historyContainer;
    private Label _statsLabel;
    private TabContainer _tabContainer;
    
    // 幸运值显示颜色
    private Color _lowLuckColor = new Color(1f, 0.3f, 0.3f);
    private Color _normalLuckColor = new Color(1f, 1f, 1f);
    private Color _highLuckColor = new Color(0.3f, 1f, 0.3f);
    
    public override void _Ready()
    {
        SetupUI();
        
        // 监听幸运变化事件
        LuckSystem.Instance.Initialize();
        LuckSystem.OnLuckChanged += UpdateLuckDisplay;
        LuckSystem.OnModifierAdded += OnModifierAdded;
        LuckSystem.OnModifierRemoved += OnModifierRemoved;
        
        UpdateAll();
    }
    
    private void SetupUI()
    {
        // 主容器
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchor(AnchorPresets.FullRect);
        mainContainer.AddConstantOverride("separation", 10);
        AddChild(mainContainer);
        
        // 标题
        var title = new Label();
        title.Text = "🎲 运气系统";
        title.Align = Label.AlignEnum.Center;
        title.AddFontOverride("font", GD.Load<DynamicFont>("res://fonts/title_font.tres"));
        mainContainer.AddChild(title);
        
        // 当前幸运值显示
        var luckContainer = new HBoxContainer();
        mainContainer.AddChild(luckContainer);
        
        var luckTitle = new Label();
        luckTitle.Text = "当前幸运: ";
        luckContainer.AddChild(luckTitle);
        
        _luckValueLabel = new Label();
        _luckValueLabel.Text = "50";
        _luckValueLabel.AddFontOverride("font", GD.Load<DynamicFont>("res://fonts/bold_font.tres"));
        luckContainer.AddChild(_luckValueLabel);
        
        // 基础幸运
        var baseContainer = new HBoxContainer();
        mainContainer.AddChild(baseContainer);
        
        var baseLabel = new Label();
        baseLabel.Text = "基础幸运: ";
        baseContainer.AddChild(baseLabel);
        
        _baseLuckLabel = new Label();
        _baseLuckLabel.Text = "50";
        baseContainer.AddChild(_baseLuckLabel);
        
        // 测试按钮
        var buttonContainer = new HBoxContainer();
        mainContainer.AddChild(buttonContainer);
        
        var rollBtn = new Button();
        rollBtn.Text = "🎲 投掷 (难度50)";
        rollBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        rollBtn.Pressed += OnRollPressed;
        buttonContainer.AddChild(rollBtn);
        
        var simpleRollBtn = new Button();
        simpleRollBtn.Text = "🎯 简单投掷 (50%)";
        simpleRollBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        simpleRollBtn.Pressed += OnSimpleRollPressed;
        buttonContainer.AddChild(simpleRollBtn);
        
        // 标签页容器
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(_tabContainer);
        
        // 修饰器标签页
        var modifiersTab = new VBoxContainer();
        modifiersTab.Name = "活跃效果";
        _tabContainer.AddChild(modifiersTab);
        
        _modifiersContainer = new VBoxContainer();
        modifiersTab.AddChild(_modifiersContainer);
        
        // 历史记录标签页
        var historyTab = new VBoxContainer();
        historyTab.Name = "历史记录";
        _tabContainer.AddChild(historyTab);
        
        _historyContainer = new VBoxContainer();
        historyTab.AddChild(_historyContainer);
        
        // 统计标签页
        var statsTab = new VBoxContainer();
        statsTab.Name = "统计数据";
        _tabContainer.AddChild(statsTab);
        
        var statsScroll = new ScrollContainer();
        statsScroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        statsTab.AddChild(statsScroll);
        
        _statsLabel = new Label();
        _statsLabel.Text = "统计数据";
        statsScroll.AddChild(_statsLabel);
        
        // 底部按钮
        var bottomContainer = new HBoxContainer();
        mainContainer.AddChild(bottomContainer);
        
        var closeBtn = new Button();
        closeBtn.Text = "关闭 (ESC)";
        closeBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        closeBtn.Pressed += OnClosePressed;
        bottomContainer.AddChild(closeBtn);
        
        // 快捷键
        var shortcutLabel = new Label();
        shortcutLabel.Text = "快捷键: L 切换显示";
        shortcutLabel.Align = Label.AlignEnum.Center;
        mainContainer.AddChild(shortcutLabel);
    }
    
    private void UpdateAll()
    {
        UpdateLuckDisplay(LuckSystem.Instance.GetCurrentLuck());
        UpdateModifiersList();
        UpdateHistoryList();
        UpdateStats();
    }
    
    private void UpdateLuckDisplay(int luck)
    {
        _luckValueLabel.Text = luck.ToString();
        _baseLuckLabel.Text = LuckSystem.Instance.GetBaseLuck().ToString();
        
        // 根据幸运值设置颜色
        if (luck < 30)
            _luckValueLabel.Modulate = _lowLuckColor;
        else if (luck > 70)
            _luckValueLabel.Modulate = _highLuckColor;
        else
            _luckValueLabel.Modulate = _normalLuckColor;
    }
    
    private void UpdateModifiersList()
    {
        // 清空现有
        foreach (var child in _modifiersContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        var modifiers = LuckSystem.Instance.GetActiveModifiers();
        
        if (modifiers.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "无活跃效果";
            emptyLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
            _modifiersContainer.AddChild(emptyLabel);
            return;
        }
        
        foreach (var mod in modifiers)
        {
            var modContainer = new HBoxContainer();
            _modifiersContainer.AddChild(modContainer);
            
            var nameLabel = new Label();
            nameLabel.Text = mod.Name;
            modContainer.AddChild(nameLabel);
            
            var valueLabel = new Label();
            string valueText = mod.Value > 0 ? $"+{mod.Value}" : mod.Value.ToString();
            valueLabel.Text = valueText;
            valueLabel.Modulate = mod.Value > 0 ? _highLuckColor : _lowLuckColor;
            modContainer.AddChild(valueLabel);
            
            if (mod.Duration > 0)
            {
                var durationLabel = new Label();
                durationLabel.Text = $" ({mod.Duration}s)";
                durationLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
                modContainer.AddChild(durationLabel);
            }
        }
    }
    
    private void UpdateHistoryList()
    {
        // 清空现有
        foreach (var child in _historyContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        var history = LuckSystem.Instance.GetHistory(10);
        
        if (history.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "暂无历史记录";
            emptyLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
            _historyContainer.AddChild(emptyLabel);
            return;
        }
        
        // 反序显示（最新的在前）
        history.Reverse();
        
        foreach (var evt in history)
        {
            var label = new Label();
            
            string resultIcon = "";
            switch (evt.Type)
            {
                case "roll":
                    resultIcon = "🎲";
                    break;
                case "bonus":
                    resultIcon = "✨";
                    break;
                default:
                    resultIcon = "📜";
                    break;
            }
            
            label.Text = $"{resultIcon} 投掷: {evt.Value} -> {evt.Result} ({evt.Source})";
            
            // 根据结果着色
            if (evt.Result >= 80)
                label.Modulate = _highLuckColor;
            else if (evt.Result <= 20)
                label.Modulate = _lowLuckColor;
            
            _historyContainer.AddChild(label);
        }
    }
    
    private void UpdateStats()
    {
        int total = LuckSystem.Instance.GetTotalRolls();
        int critical = LuckSystem.Instance.GetCriticalRolls();
        int failed = LuckSystem.Instance.GetFailedRolls();
        float successRate = LuckSystem.Instance.GetSuccessRate();
        float dropBonus = LuckSystem.Instance.GetDropRateBonus();
        float critBonus = LuckSystem.Instance.GetCriticalRateBonus();
        
        string stats = $"📊 运气统计\n\n";
        stats += $"总投掷次数: {total}\n";
        stats += $"暴击次数: {critical}\n";
        stats += $"失败次数: {failed}\n";
        stats += $"成功率: {successRate:P1}\n\n";
        stats += $"🎁 掉落率加成: {dropBonus:F2}x\n";
        stats += $"⚔️ 暴击率加成: {critBonus:P1}\n";
        
        _statsLabel.Text = stats;
    }
    
    private void OnRollPressed()
    {
        var result = LuckSystem.Instance.Roll(50);
        string resultText = GetResultText(result);
        
        GD.Print($"🎲 投掷结果: {resultText}");
        UpdateAll();
    }
    
    private void OnSimpleRollPressed()
    {
        bool success = LuckSystem.Instance.SimpleRoll(50);
        string resultText = success ? "✅ 成功!" : "❌ 失败!";
        
        GD.Print($"🎯 简单投掷结果: {resultText}");
        UpdateAll();
    }
    
    private string GetResultText(LuckResult result)
    {
        switch (result)
        {
            case LuckResult.CriticalFailure: return "❌ 大失败!";
            case LuckResult.Failure: return "❌ 失败";
            case LuckResult.LowSuccess: return "🔸 小成功";
            case LuckResult.Success: return "✅ 成功";
            case LuckResult.HighSuccess: return "✨ 大成功";
            case LuckResult.CriticalSuccess: return "🌟 暴击大成功!";
            default: return "未知";
        }
    }
    
    private void OnModifierAdded(string name, int value)
    {
        UpdateModifiersList();
    }
    
    private void OnModifierRemoved(string name)
    {
        UpdateModifiersList();
    }
    
    private void OnClosePressed()
    {
        QueueFree();
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Scancode == Godot.KeyList.Escape)
            {
                OnClosePressed();
            }
        }
    }
}
