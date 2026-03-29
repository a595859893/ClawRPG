using Godot;
using System;

/// <summary>
/// Boss 狂暴 Combo 掉落通知
/// REQ-155: 狂暴触发后显示金色通知 "传说Combo已准备！"
/// </summary>
public partial class BossEnrageComboDropNotification : Control
{
    private CanvasLayer _canvasLayer;
    private Label _notificationLabel;
    private Timer _displayTimer;
    private Timer _fadeTimer;
    private bool _isShowing = false;

    [Export] private float _displayDuration = 2.5f;
    [Export] private float _fadeOutDuration = 0.5f;

    public override void _Ready()
    {
        SetupUI();

        // 订阅 Combo 掉落事件
        ComboDropSystem.OnComboDropGranted += OnComboDropGranted;
    }

    public override void _ExitTree()
    {
        ComboDropSystem.OnComboDropGranted -= OnComboDropGranted;
    }

    private void SetupUI()
    {
        _canvasLayer = new CanvasLayer();
        _canvasLayer.Layer = 150;
        AddChild(_canvasLayer);

        _notificationLabel = new Label();
        _notificationLabel.Text = "";
        _notificationLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _notificationLabel.VerticalAlignment = VerticalAlignment.Center;
        _notificationLabel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _notificationLabel.OffsetLeft = -300;
        _notificationLabel.OffsetTop = -40;
        _notificationLabel.OffsetRight = 300;
        _notificationLabel.OffsetBottom = 40;
        _notificationLabel.AddThemeFontSizeOverride("font_size", 28);
        _notificationLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.2f, 1f));
        _notificationLabel.Visible = false;

        // 添加黑色阴影背景
        var shadowLabel = new Label();
        shadowLabel.Text = "";
        shadowLabel.HorizontalAlignment = HorizontalAlignment.Center;
        shadowLabel.VerticalAlignment = VerticalAlignment.Center;
        shadowLabel.SetAnchorsPreset(Control.LayoutPreset.Center);
        shadowLabel.OffsetLeft = -300 + 2;
        shadowLabel.OffsetTop = -40 + 2;
        shadowLabel.OffsetRight = 300 + 2;
        shadowLabel.OffsetBottom = 40 + 2;
        shadowLabel.AddThemeFontSizeOverride("font_size", 28);
        shadowLabel.AddThemeColorOverride("font_color", new Color(0f, 0f, 0f, 0.8f));
        shadowLabel.Visible = false;
        shadowLabel.Name = "ShadowLabel";
        _canvasLayer.AddChild(shadowLabel);

        _canvasLayer.AddChild(_notificationLabel);

        _displayTimer = new Timer();
        _displayTimer.OneShot = true;
        _displayTimer.Timeout += OnDisplayTimerTimeout;
        _canvasLayer.AddChild(_displayTimer);

        _fadeTimer = new Timer();
        _fadeTimer.OneShot = true;
        _fadeTimer.Timeout += OnFadeTimerTimeout;
        _canvasLayer.AddChild(_fadeTimer);
    }

    private void OnComboDropGranted(string comboId, ComboData combo)
    {
        if (_isShowing) return;
        ShowNotification(combo);
    }

    private void ShowNotification(ComboData combo)
    {
        _isShowing = true;

        // 格式化通知文本
        string rarityText = combo.comboRarity == ComboData.Rarity.Legendary ? "传说" : "史诗";
        string notificationText = $"🎴 {rarityText}Combo 已准备！\n{combo.comboName}";

        _notificationLabel.Text = notificationText;
        _notificationLabel.Visible = true;

        var shadowLabel = _canvasLayer.GetNodeOrNull<Label>("ShadowLabel");
        if (shadowLabel != null)
        {
            shadowLabel.Text = notificationText;
            shadowLabel.Visible = true;
        }

        // 初始透明度
        Modulate = new Color(1f, 1f, 1f, 0f);
        _notificationLabel.Modulate = new Color(1f, 1f, 1f, 0f);
        if (shadowLabel != null) shadowLabel.Modulate = new Color(1f, 1f, 1f, 0f);

        // 淡入动画
        Tween fadeInTween = CreateTween();
        fadeInTween.TweenProperty(this, "modulate", new Color(1f, 1f, 1f, 1f), _fadeOutDuration);
        fadeInTween.Parallel().TweenProperty(_notificationLabel, "modulate", new Color(1f, 1f, 1f, 1f), _fadeOutDuration);
        if (shadowLabel != null)
            fadeInTween.Parallel().TweenProperty(shadowLabel, "modulate", new Color(1f, 1f, 1f, 1f), _fadeOutDuration);

        // 开始显示计时
        _displayTimer.Start(_displayDuration);
    }

    private void OnDisplayTimerTimeout()
    {
        // 开始淡出
        _fadeTimer.Start(_fadeOutDuration);
        Tween fadeOutTween = CreateTween();
        fadeOutTween.TweenProperty(this, "modulate", new Color(1f, 1f, 1f, 0f), _fadeOutDuration);
        fadeOutTween.Parallel().TweenProperty(_notificationLabel, "modulate", new Color(1f, 1f, 1f, 0f), _fadeOutDuration);
        var shadowLabel = _canvasLayer.GetNodeOrNull<Label>("ShadowLabel");
        if (shadowLabel != null)
            fadeOutTween.Parallel().TweenProperty(shadowLabel, "modulate", new Color(1f, 1f, 1f, 0f), _fadeOutDuration);
    }

    private void OnFadeTimerTimeout()
    {
        _notificationLabel.Visible = false;
        var shadowLabel = _canvasLayer.GetNodeOrNull<Label>("ShadowLabel");
        if (shadowLabel != null) shadowLabel.Visible = false;
        _isShowing = false;
    }
}
