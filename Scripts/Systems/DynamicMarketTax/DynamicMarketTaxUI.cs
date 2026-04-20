using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 动态市场税率系统 UI
/// </summary>
public partial class DynamicMarketTaxUI : Control
{
    private DynamicMarketTaxSystem _taxSystem;
    private Label _titleLabel;
    private Label _taxRateLabel;
    private Label _marketActivityLabel;
    private Label _marketTrendLabel;
    private Label _statisticsLabel;
    private HSlider _simulationSlider;
    private Button _simulateButton;
    private Button _resetButton;
    private Button _closeButton;
    private VBoxContainer _mainContainer;
    
    private bool _isVisible = false;
    
    public override void _Ready()
    {
        // 查找系统
        _taxSystem = GetNode<DynamicMarketTaxSystem>("/root/Main/Systems/DynamicMarketTaxSystem");
        
        if (_taxSystem == null)
        {
            GD.PrintErr("DynamicMarketTaxUI: 未找到 DynamicMarketTaxSystem");
            return;
        }
        
        SetupUI();
        Hide();
        
        GD.Print("DynamicMarketTaxUI: 动态市场税率 UI 已初始化");
    }
    
    private void SetupUI()
    {
        // 主容器
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchor(AnchorPresets.Center);
        _mainContainer.SetOffset(new Rect2(-300, -250, 600, 500));
        _mainContainer.AddThemeConstantOverride("separation", 15);
        AddChild(_mainContainer);
        
        // 标题
        _titleLabel = new Label();
        _titleLabel.Text = "  📊 动态市场税率系统  ";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeColorOverride("font_color", new Color(1, 0.84f, 0));
        _mainContainer.AddChild(_titleLabel);
        
        // 分隔线
        AddSeparator(_mainContainer);
        
        // 税率显示
        _taxRateLabel = new Label();
        _taxRateLabel.Text = "当前税率: 5.0%";
        _taxRateLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _mainContainer.AddChild(_taxRateLabel);
        
        // 市场热度
        _marketActivityLabel = new Label();
        _marketActivityLabel.Text = "市场热度: 50%";
        _marketActivityLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _mainContainer.AddChild(_marketActivityLabel);
        
        // 市场趋势
        _marketTrendLabel = new Label();
        _marketTrendLabel.Text = "市场趋势: Stable";
        _marketTrendLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _mainContainer.AddChild(_marketTrendLabel);
        
        // 分隔线
        AddSeparator(_mainContainer);
        
        // 统计信息
        _statisticsLabel = new Label();
        _statisticsLabel.Text = "统计信息:\n总交易: 0\n总税收: 0\n总交易额: 0\n平均交易额: 0\n峰值交易额: 0";
        _statisticsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _mainContainer.AddChild(_statisticsLabel);
        
        // 分隔线
        AddSeparator(_mainContainer);
        
        // 模拟交易区域
        var simContainer = new HBoxContainer();
        simContainer.AddThemeConstantOverride("separation", 10);
        _mainContainer.AddChild(simContainer);
        
        var simLabel = new Label();
        simLabel.Text = "模拟交易量:";
        simContainer.AddChild(simLabel);
        
        _simulationSlider = new HSlider();
        _simulationSlider.MinValue = 100;
        _simulationSlider.MaxValue = 10000;
        _simulationSlider.Value = 1000;
        _simulationSlider.CustomMinimumSize = new Vector2(200, 0);
        simContainer.AddChild(_simulationSlider);
        
        var simValueLabel = new Label();
        simValueLabel.Text = "1000";
        simValueLabel.CustomMinimumSize = new Vector2(60, 0);
        simContainer.AddChild(simValueLabel);
        
        _simulationSlider.ValueChanged += (value) => {
            simValueLabel.Text = ((int)value).ToString();
        };
        
        // 模拟按钮
        _simulateButton = new Button();
        _simulateButton.Text = "模拟交易";
        _simulateButton.CustomMinimumSize = new Vector2(120, 30);
        _simulateButton.Pressed += _OnSimulateButtonPressed;
        _mainContainer.AddChild(_simulateButton);
        
        // 重置按钮
        _resetButton = new Button();
        _resetButton.Text = "重置统计";
        _resetButton.CustomMinimumSize = new Vector2(120, 30);
        _resetButton.Pressed += _OnResetButtonPressed;
        _mainContainer.AddChild(_resetButton);
        
        // 关闭按钮
        _closeButton = new Button();
        _closeButton.Text = "关闭 (ESC)";
        _closeButton.CustomMinimumSize = new Vector2(120, 30);
        _closeButton.Pressed += _OnCloseButtonPressed;
        _mainContainer.AddChild(_closeButton);
    }
    
    private void AddSeparator(VBoxContainer container)
    {
        var separator = new HSeparator();
        separator.AddThemeConstantOverride("separation", 10);
        container.AddChild(separator);
    }
    
    private void _OnSimulateButtonPressed()
    {
        long volume = (long)_simulationSlider.Value;
        _taxSystem.SimulateTransaction(1, volume);
        UpdateDisplay();
    }
    
    private void _OnResetButtonPressed()
    {
        _taxSystem.ResetStatistics();
        UpdateDisplay();
    }
    
    private void _OnCloseButtonPressed()
    {
        Toggle();
    }
    
    public void Toggle()
    {
        if (_isVisible)
        {
            Hide();
            _isVisible = false;
        }
        else
        {
            Show();
            _isVisible = true;
            UpdateDisplay();
        }
    }
    
    private void UpdateDisplay()
    {
        var stats = _taxSystem.GetTaxStatistics();
        
        _taxRateLabel.Text = $"当前税率: {stats["currentTaxRate"]}%";
        
        float activity = (float)stats["marketActivity"];
        string activityColor = activity > 70 ? "#FF6B6B" : (activity < 30 ? "#4ECDC4" : "#FFFFFF");
        _marketActivityLabel.Text = $"市场热度: {activity:F1}%";
        _marketActivityLabel.AddThemeColorOverride("font_color", new Color(activityColor));
        
        string trend = (string)stats["marketTrend"];
        string trendEmoji = trend == "Rising" ? "📈" : (trend == "Falling" ? "📉" : "➡️");
        _marketTrendLabel.Text = $"市场趋势: {trendEmoji} {trend}";
        
        _statisticsLabel.Text = $"统计信息:\n" +
            $"总交易: {stats["totalTransactions"]}\n" +
            $"总税收: {stats["totalTaxCollected"]}\n" +
            $"总交易额: {stats["totalVolume"]}\n" +
            $"平均交易额: {stats["averageTransactionValue"]:F1}\n" +
            $"峰值交易额: {stats["peakVolume"]}";
    }
    
    public override void _Input(InputEvent eventArgs)
    {
        if (eventArgs is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Scancode == KeyList.Escape)
        {
            if (_isVisible)
            {
                Toggle();
            }
        }
    }
}
