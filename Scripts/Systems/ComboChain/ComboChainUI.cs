using Godot;
using System;
using System.Collections.Generic;

public class ComboChainUI : Control
{
    // UI 组件
    private Label _chainCountLabel;
    private Label _comboLevelLabel;
    private Label _timerLabel;
    private ProgressBar _chainProgressBar;
    private ProgressBar _timerProgressBar;
    private VBoxContainer _bonusContainer;
    private VBoxContainer _historyContainer;
    private Label _statsLabel;
    
    // 样式
    private Godot.Color _normalColor = new Godot.Color(1f, 1f, 1f);
    private Godot.Color _goodColor = new Godot.Color(0.3f, 0.9f, 0.3f);
    private Godot.Color _greatColor = new Godot.Color(0.3f, 0.6f, 1f);
    private Godot.Color _amazingColor = new Godot.Color(0.9f, 0.7f, 0.2f);
    private Godot.Color _legendaryColor = new Godot.Color(1f, 0.4f, 0.4f);
    private Godot.Color _mythicColor = new Godot.Color(0.2f, 1f, 0.9f);
    
    // 动画
    private Tween _chainTween;
    private float _lastChainCount = 0;
    
    public override void _Ready()
    {
        SetupUI();
        ConnectSignals();
    }
    
    private void SetupUI()
    {
        // 主容器
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainContainer.Position = new Vector2(500, 300);
        mainContainer.CustomMinimumSize = new Vector2(400, 500);
        AddChild(mainContainer);
        
        // 标题
        var titleLabel = new Label();
        titleLabel.Text = "⚔️ Combo Chain System ⚔️";
        titleLabel.Align = Label.AlignEnum.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainContainer.AddChild(titleLabel);
        
        // 连击数显示
        _chainCountLabel = new Label();
        _chainCountLabel.Text = "0";
        _chainCountLabel.Align = Label.AlignEnum.Center;
        _chainCountLabel.AddThemeFontSizeOverride("font_size", 72);
        _chainCountLabel.AddThemeColorOverride("font_color", _normalColor);
        mainContainer.AddChild(_chainCountLabel);
        
        // 连击等级
        _comboLevelLabel = new Label();
        _comboLevelLabel.Text = "Novice";
        _comboLevelLabel.Align = Label.AlignEnum.Center;
        _comboLevelLabel.AddThemeFontSizeOverride("font_size", 18);
        mainContainer.AddChild(_comboLevelLabel);
        
        // 连击进度条
        _chainProgressBar = new ProgressBar();
        _chainProgressBar.CustomMinimumSize = new Vector2(300, 20);
        _chainProgressBar.ShowPercentage = false;
        mainContainer.AddChild(_chainProgressBar);
        
        // 计时器标签
        _timerLabel = new Label();
        _timerLabel.Text = "Time: 0.0s";
        _timerLabel.Align = Label.AlignEnum.Center;
        mainContainer.AddChild(_timerLabel);
        
        // 计时器进度条
        _timerProgressBar = new ProgressBar();
        _timerProgressBar.CustomMinimumSize = new Vector2(300, 10);
        _timerProgressBar.ShowPercentage = false;
        mainContainer.AddChild(_timerProgressBar);
        
        // 分隔
        var separator = new HSeparator();
        mainContainer.AddChild(separator);
        
        // 活跃加成标题
        var bonusTitle = new Label();
        bonusTitle.Text = "Active Bonuses:";
        bonusTitle.AddThemeFontSizeOverride("font_size", 16);
        mainContainer.AddChild(bonusTitle);
        
        // 活跃加成容器
        _bonusContainer = new VBoxContainer();
        mainContainer.AddChild(_bonusContainer);
        
        // 分隔
        var separator2 = new HSeparator();
        mainContainer.AddChild(separator2);
        
        // 历史记录标题
        var historyTitle = new Label();
        historyTitle.Text = "Recent Chains:";
        historyTitle.AddThemeFontSizeOverride("font_size", 16);
        mainContainer.AddChild(historyTitle);
        
        // 历史记录容器
        _historyContainer = new VBoxContainer();
        _historyContainer.CustomMinimumSize = new Vector2(300, 150);
        mainContainer.AddChild(_historyContainer);
        
        // 分隔
        var separator3 = new HSeparator();
        mainContainer.AddChild(separator3);
        
        // 统计信息
        _statsLabel = new Label();
        _statsLabel.Text = "Statistics:";
        _statsLabel.AddThemeFontSizeOverride("font_size", 16);
        mainContainer.AddChild(_statsLabel);
        
        var statsContainer = new VBoxContainer();
        mainContainer.AddChild(statsContainer);
        
        // 测试按钮容器
        var buttonContainer = new HBoxContainer();
        mainContainer.AddChild(buttonContainer);
        
        // 测试轻攻击按钮
        var lightButton = new Button();
        lightButton.Text = "Light Attack";
        lightButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        lightButton.Pressed += _OnLightAttackPressed;
        buttonContainer.AddChild(lightButton);
        
        // 测试重攻击按钮
        var heavyButton = new Button();
        heavyButton.Text = "Heavy Attack";
        heavyButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        heavyButton.Pressed += _OnHeavyAttackPressed;
        buttonContainer.AddChild(heavyButton);
        
        // 测试技能按钮
        var skillButton = new Button();
        skillButton.Text = "Skill Attack";
        skillButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        skillButton.Pressed += _OnSkillAttackPressed;
        buttonContainer.AddChild(skillButton);
        
        // 结束连击按钮
        var endButton = new Button();
        endButton.Text = "End Chain";
        endButton.Pressed += _OnEndChainPressed;
        mainContainer.AddChild(endButton);
        
        // 重置统计按钮
        var resetButton = new Button();
        resetButton.Text = "Reset Statistics";
        resetButton.Pressed += _OnResetPressed;
        mainContainer.AddChild(resetButton);
        
        // 关闭按钮
        var closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.Pressed += _OnClosePressed;
        mainContainer.AddChild(closeButton);
        
        // 初始更新
        UpdateUI();
    }
    
    private void ConnectSignals()
    {
        var system = ComboChainSystem.Instance;
        if (system != null)
        {
            system.ChainStarted += _OnChainStarted;
            system.ChainEnded += _OnChainEnded;
            system.ChainBonusActivated += _OnChainBonusActivated;
            system.ComboLevelUp += _OnComboLevelUp;
        }
    }
    
    public override void _Process(float delta)
    {
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        var system = ComboChainSystem.Instance;
        if (system == null) return;
        
        // 更新连击数
        int currentChain = system.GetCurrentChain();
        _chainCountLabel.Text = currentChain.ToString();
        
        // 更新连击数颜色
        Godot.Color chainColor = GetChainColor(currentChain);
        _chainCountLabel.AddThemeColorOverride("font_color", chainColor);
        
        // 更新连击等级
        _comboLevelLabel.Text = system.GetCurrentComboLevelName();
        
        // 更新进度条
        int nextLevel = system.GetCurrentComboLevel() + 1;
        var currentConfig = system.GetCurrentComboLevelConfig();
        if (currentConfig != null)
        {
            float progress = (float)(currentChain - currentConfig.MinHits) / (currentConfig.MaxHits - currentConfig.MinHits + 1);
            _chainProgressBar.Value = progress * 100;
        }
        
        // 更新计时器
        float remainingTime = system.GetRemainingTime();
        float timeLimit = system.GetChainTimeLimit();
        _timerLabel.Text = $"Time: {remainingTime:F1}s / {timeLimit:F1}s";
        _timerProgressBar.Value = (remainingTime / timeLimit) * 100;
        
        // 更新加成显示
        UpdateBonuses();
        
        // 更新历史记录
        UpdateHistory();
        
        // 更新统计
        UpdateStats();
        
        // 连击动画
        if (currentChain != _lastChainCount && currentChain > 0)
        {
            PlayChainAnimation();
            _lastChainCount = currentChain;
        }
    }
    
    private Godot.Color GetChainColor(int chain)
    {
        if (chain >= 100) return _mythicColor;
        if (chain >= 50) return _legendaryColor;
        if (chain >= 25) return _amazingColor;
        if (chain >= 10) return _greatColor;
        if (chain >= 5) return _goodColor;
        return _normalColor;
    }
    
    private void UpdateBonuses()
    {
        var system = ComboChainSystem.Instance;
        if (system == null) return;
        
        // 清空现有显示
        foreach (var child in _bonusContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // 显示活跃加成
        var activeBonuses = system.GetActiveBonuses();
        foreach (var bonus in activeBonuses)
        {
            var bonusLabel = new Label();
            bonusLabel.Text = $"✦ {bonus.EffectName}: +{(bonus.DamageBonus * 100):F0}% DMG, +{(bonus.SpeedBonus * 100):F0}% SPD, +{(bonus.CritBonus * 100):F0}% CRIT";
            bonusLabel.AddThemeColorOverride("font_color", bonus.EffectColor);
            _bonusContainer.AddChild(bonusLabel);
        }
        
        if (activeBonuses.Count == 0)
        {
            var noBonusLabel = new Label();
            noBonusLabel.Text = "No active bonuses";
            noBonusLabel.AddThemeColorOverride("font_color", new Godot.Color(0.5f, 0.5f, 0.5f));
            _bonusContainer.AddChild(noBonusLabel);
        }
    }
    
    private void UpdateHistory()
    {
        var system = ComboChainSystem.Instance;
        if (system == null) return;
        
        // 清空现有显示
        foreach (var child in _historyContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // 显示历史记录
        var history = system.GetHistory(5);
        foreach (var record in history)
        {
            var historyLabel = new Label();
            historyLabel.Text = $"L{record.ChainLevel}: {record.Damage:F0} DMG (+{record.BonusDamage:F0})";
            _historyContainer.AddChild(historyLabel);
        }
        
        if (history.Count == 0)
        {
            var noHistoryLabel = new Label();
            noHistoryLabel.Text = "No chain history";
            noHistoryLabel.AddThemeColorOverride("font_color", new Godot.Color(0.5f, 0.5f, 0.5f));
            _historyContainer.AddChild(noHistoryLabel);
        }
    }
    
    private void UpdateStats()
    {
        var system = ComboChainSystem.Instance;
        if (system == null) return;
        
        var stats = system.GetStatistics();
        
        string statsText = $"Total Chains: {stats["totalChains"]}\n";
        statsText += $"Total Hits: {stats["totalChainHits"]}\n";
        statsText += $"Max Chain: {stats["maxChainEver"]}\n";
        statsText += $"Chain 10+: {stats["chain10Count"]}\n";
        statsText += $"Chain 25+: {stats["chain25Count"]}\n";
        statsText += $"Chain 50+: {stats["chain50Count"]}\n";
        statsText += $"Chain 100+: {stats["chain100Count"]}\n";
        statsText += $"Bonus Damage: {stats["chainDamageBonus"]:F0}";
        
        _statsLabel.Text = statsText;
    }
    
    private void PlayChainAnimation()
    {
        if (_chainTween != null)
        {
            _chainTween.Kill();
        }
        
        _chainTween = CreateTween();
        _chainTween.TweenProperty(_chainCountLabel, "scale", new Vector2(1.3f, 1.3f), 0.1f);
        _chainTween.TweenProperty(_chainCountLabel, "scale", new Vector2(1f, 1f), 0.1f);
    }
    
    // 信号处理
    private void _OnChainStarted(int chainCount)
    {
        GD.Print($"Chain Started: {chainCount}");
    }
    
    private void _OnChainEnded(int maxChain, float totalDamage)
    {
        GD.Print($"Chain Ended - Max: {maxChain}, Total Bonus Damage: {totalDamage}");
    }
    
    private void _OnChainBonusActivated(int chainRequired, string effectName)
    {
        GD.Print($"Bonus Activated: {effectName} at {chainRequired} chain!");
    }
    
    private void _OnComboLevelUp(int newLevel, string levelName)
    {
        GD.Print($"Combo Level Up: {newLevel} - {levelName}");
    }
    
    // 按钮处理
    private void _OnLightAttackPressed()
    {
        var system = ComboChainSystem.Instance;
        if (system != null)
        {
            system.TriggerChain(100f, ComboChainDatabase.ComboType.Light);
        }
    }
    
    private void _OnHeavyAttackPressed()
    {
        var system = ComboChainSystem.Instance;
        if (system != null)
        {
            system.TriggerChain(150f, ComboChainDatabase.ComboType.Heavy);
        }
    }
    
    private void _OnSkillAttackPressed()
    {
        var system = ComboChainSystem.Instance;
        if (system != null)
        {
            system.TriggerChain(200f, ComboChainDatabase.ComboType.Skill);
        }
    }
    
    private void _OnEndChainPressed()
    {
        var system = ComboChainSystem.Instance;
        if (system != null)
        {
            system.ForceEndChain();
        }
    }
    
    private void _OnResetPressed()
    {
        var system = ComboChainSystem.Instance;
        if (system != null)
        {
            system.ResetStatistics();
        }
    }
    
    private void _OnClosePressed()
    {
        QueueFree();
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            _OnClosePressed();
        }
    }
    
    /// <summary>
    /// 导出保存数据 - UI组件无持久化状态
    /// </summary>
    public override Dictionary ExportSaveData() {
        return new Dictionary();
    }

    /// <summary>
    /// 导入保存数据 - UI组件无持久化状态
    /// </summary>
    public override void ImportSaveData(Dictionary data) {
    }
}

// 便捷函数
public partial class ComboChainMain : BaseSystem
{
    public static ComboChainUI CurrentUI { get; private set; }
    
    public static void ToggleComboChainUI()
    {
        if (CurrentUI != null)
        {
            CurrentUI.QueueFree();
            CurrentUI = null;
            return;
        }
        
        var ui = new ComboChainUI();
        AddChild(ui);
        CurrentUI = ui;
    }
}
