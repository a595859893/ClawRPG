using Godot;
using System;
using System.Collections.Generic;

public partial class CombatStanceUI : Control
{
    private static CombatStanceUI Instance { get; set; }
    
    private bool isVisible = false;
    private Control panel;
    private VBoxContainer stanceListContainer;
    private Label currentStanceLabel;
    private Label stanceInfoLabel;
    private Label durationLabel;
    private Label levelLabel;
    private ProgressBar durationBar;
    private Button closeButton;
    
    // 按钮数组
    private Dictionary<CombatStanceSystem.StanceType, Button> stanceButtons = new Dictionary<CombatStanceSystem.StanceType, Button>();
    
    public override void _Ready()
    {
        Instance = this;
        SetupUI();
        VisibilityChanged += OnVisibilityChanged;
    }
    
    private void SetupUI()
    {
        // 主面板
        panel = new Control();
        panel.SetAnchorsPreset(Control.LayoutPreset.Center);
        AddChild(panel);
        
        var panelBg = new Panel();
        panelBg.SetAnchorsPreset(Control.LayoutPreset.Center);
        panelBg.CustomMinimumSize = new Vector2(500, 400);
        panelBg.Position = new Vector2(-250, -200);
        panel.AddChild(panelBg);
        
        var marginContainer = new MarginContainer();
        marginContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        marginContainer.AddThemeConstantOverride("margin_left", 20);
        marginContainer.AddThemeConstantOverride("margin_right", 20);
        marginContainer.AddThemeConstantOverride("margin_top", 20);
        marginContainer.AddThemeConstantOverride("margin_bottom", 20);
        panelBg.AddChild(marginContainer);
        
        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        marginContainer.AddChild(vbox);
        
        // 标题栏
        var titleLabel = new Label();
        titleLabel.Text = "⚔ 战斗姿态";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        vbox.AddChild(titleLabel);
        
        // 当前姿态显示
        currentStanceLabel = new Label();
        currentStanceLabel.Text = "当前姿态: 平衡姿态";
        currentStanceLabel.HorizontalAlignment = HorizontalAlignment.Center;
        currentStanceLabel.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(currentStanceLabel);
        
        // 等级显示
        levelLabel = new Label();
        levelLabel.Text = "姿态等级: 1";
        levelLabel.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(levelLabel);
        
        // 持续时间条
        durationBar = new ProgressBar();
        durationBar.CustomMinimumSize = new Vector2(0, 20);
        vbox.AddChild(durationBar);
        
        durationLabel = new Label();
        durationLabel.Text = "持续时间: --";
        durationLabel.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(durationLabel);
        
        // 分割线
        var separator = new HSeparator();
        vbox.AddChild(separator);
        
        // 姿态列表
        stanceListContainer = new VBoxContainer();
        stanceListContainer.CustomMinimumSize = new Vector2(0, 200);
        vbox.AddChild(stanceListContainer);
        
        // 创建姿态按钮
        CreateStanceButtons();
        
        // 姿态信息显示
        stanceInfoLabel = new Label();
        stanceInfoLabel.Text = "选择一个姿态查看详情";
        stanceInfoLabel.HorizontalAlignment = HorizontalAlignment.Center;
        stanceInfoLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        vbox.AddChild(stanceInfoLabel);
        
        // 关闭按钮
        closeButton = new Button();
        closeButton.Text = "关闭 (K)";
        closeButton.Pressed += () => ToggleVisibility();
        vbox.AddChild(closeButton);
        
        // 初始隐藏
        Hide();
    }
    
    private void CreateStanceButtons()
    {
        var stances = Enum.GetValues(typeof(CombatStanceSystem.StanceType));
        
        foreach (CombatStanceSystem.StanceType stance in stances)
        {
            var button = new Button();
            button.Text = CombatStanceSystem.Instance.GetStanceName(stance);
            button.CustomMinimumSize = new Vector2(0, 40);
            
            // 设置按钮颜色
            var color = CombatStanceSystem.Instance.GetStanceIconColor(stance);
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(color.R, color.G, color.B, 0.3f);
            styleBox.BorderWidthLeft = 2;
            styleBox.BorderWidthRight = 2;
            styleBox.BorderWidthTop = 2;
            styleBox.BorderWidthBottom = 2;
            styleBox.BorderColor = color;
            button.AddThemeStyleboxOverride("normal", styleBox);
            
            var hoverStyle = new StyleBoxFlat();
            hoverStyle.BgColor = new Color(color.R, color.G, color.B, 0.5f);
            hoverStyle.BorderWidthLeft = 2;
            hoverStyle.BorderWidthRight = 2;
            hoverStyle.BorderWidthTop = 2;
            hoverStyle.BorderWidthBottom = 2;
            hoverStyle.BorderColor = color;
            button.AddThemeStyleboxOverride("hover", hoverStyle);
            
            button.Pressed += () => OnStanceButtonPressed(stance);
            
            stanceListContainer.AddChild(button);
            stanceButtons[stance] = button;
        }
    }
    
    private void OnStanceButtonPressed(CombatStanceSystem.StanceType stance)
    {
        bool success = CombatStanceSystem.Instance.SwitchStance(stance);
        if (success)
        {
            UpdateDisplay();
        }
        else
        {
            GD.Print("[CombatStanceUI] Failed to switch stance - not enough stamina?");
        }
    }
    
    private void OnVisibilityChanged()
    {
        if (Visible)
        {
            UpdateDisplay();
        }
    }
    
    public void UpdateDisplay()
    {
        if (CombatStanceSystem.Instance == null) return;
        
        var currentStance = CombatStanceSystem.Instance.GetCurrentStance();
        var config = CombatStanceSystem.Instance.GetCurrentStanceConfig();
        
        // 更新当前姿态显示
        currentStanceLabel.Text = $"当前姿态: {CombatStanceSystem.Instance.GetStanceName(currentStance)}";
        
        // 更新等级
        levelLabel.Text = $"姿态等级: {CombatStanceSystem.Instance.GetStanceLevel()}";
        
        // 更新持续时间
        if (config.maxDuration > 0)
        {
            var duration = CombatStanceSystem.Instance.GetStanceDurationRatio();
            durationBar.Value = duration * 100;
            durationBar.Show();
            
            float remainingTime = config.maxDuration * duration;
            durationLabel.Text = $"持续时间: {remainingTime:F1}秒";
            durationLabel.Show();
        }
        else
        {
            durationBar.Hide();
            durationLabel.Text = "持续时间: 无限";
            durationLabel.Show();
        }
        
        // 更新信息
        stanceInfoLabel.Text = config.description;
        
        // 更新按钮状态
        foreach (var kvp in stanceButtons)
        {
            if (kvp.Key == currentStance)
            {
                var activeStyle = new StyleBoxFlat();
                activeStyle.BgColor = new Color(1f, 1f, 0f, 0.5f);
                kvp.Value.AddThemeStyleboxOverride("pressed", activeStyle);
            }
        }
    }
    
    public void ToggleVisibility()
    {
        if (isVisible)
        {
            Hide();
            isVisible = false;
        }
        else
        {
            Show();
            isVisible = true;
            UpdateDisplay();
        }
    }
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.K || keyEvent.Keycode == Key.Period)
            {
                ToggleVisibility();
            }
        }
    }
    
    public static void ShowUI()
    {
        if (Instance != null)
        {
            Instance.Show();
            Instance.isVisible = true;
            Instance.UpdateDisplay();
        }
    }
    
    public static void HideUI()
    {
        if (Instance != null)
        {
            Instance.Hide();
            Instance.isVisible = false;
        }
    }
    
    public static bool IsVisible()
    {
        return Instance != null && Instance.Visible;
    }
}
