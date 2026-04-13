using Godot;
using System;

/// <summary>
/// 战斗节拍感知 UI（REQ-131-04）
/// 战斗内显示当前节奏等级图标：Calm / Normal / Intense / Frenzied
/// </summary>
public partial class CombatRhythmUI : CanvasLayer
{
    private static CombatRhythmUI _instance;
    public static CombatRhythmUI Instance => _instance;

    // 节奏等级对应的 emoji/图标
    private static readonly string[] LevelEmojis = { "💤", "🎵", "🔥", "⚡" };
    private static readonly string[] LevelNames   = { "Calm", "Normal", "Intense", "Frenzied" };
    private static readonly Color[] LevelColors   = {
        new Color(0.5f, 0.8f, 0.5f),   // Calm — 绿色
        new Color(0.9f, 0.9f, 0.4f),   // Normal — 黄色
        new Color(1.0f, 0.6f, 0.2f),   // Intense — 橙色
        new Color(1.0f, 0.2f, 0.2f),   // Frenzied — 红色
    };

    // UI 组件
    private PanelContainer _panel;
    private HBoxContainer  _hbox;
    private Label          _iconLabel;
    private Label          _nameLabel;
    private PanelContainer _pulseOverlay;

    // 动画
    private Tween _fadeTween;
    private Tween _pulseTween;
    private bool  _visible = false;

    // 状态
    private bool _subscriptionActive = false;

    public override void _Ready()
    {
        _instance = this;
        SetupUI();
        SubscribeToSignals();
        // 初始状态：战斗外隐藏
        HideRhythmUI(false);
    }

    private void SetupUI()
    {
        // 面板 — 屏幕右上角
        _panel = new PanelContainer();
        _panel.SetAnchorsPreset(Control.LayoutPreset.TopRight);
        _panel.MarginTop   = 80;
        _panel.MarginRight = 20;
        _panel.CustomMinimumSize = new Vector2(140, 44);
        AddChild(_panel);

        // 背景样式
        var style = new StyleBoxFlat();
        style.BgColor       = new Color(0.08f, 0.08f, 0.12f, 0.82f);
        style.CornerRadiusTopLeft     = 10;
        style.CornerRadiusTopRight     = 10;
        style.CornerRadiusBottomLeft   = 10;
        style.CornerRadiusBottomRight  = 10;
        style.ContentMarginLeft   = 14;
        style.ContentMarginTop    = 8;
        style.ContentMarginRight  = 14;
        style.ContentMarginBottom = 8;
        style.BorderWidthBottom = 2;
        _panel.AddThemeStyleboxOverride("panel", style);

        // HBox
        _hbox = new HBoxContainer();
        _hbox.Alignment = BoxContainer.AlignmentMode.Center;
        _hbox.CustomMinimumSize = new Vector2(120, 28);
        _panel.AddChild(_hbox);

        // 图标标签（emoji）
        _iconLabel = new Label();
        _iconLabel.Text        = "💤";
        _iconLabel.AddThemeFontSizeOverride("font_size", 22);
        _iconLabel.ExpandIcon  = false;
        _hbox.AddChild(_iconLabel);

        // 分隔
        var sep = new Label();
        sep.Text = " ";
        sep.CustomMinimumSize = new Vector2(6, 0);
        _hbox.AddChild(sep);

        // 等级名称标签
        _nameLabel = new Label();
        _nameLabel.Text        = "Calm";
        _nameLabel.AddThemeFontSizeOverride("font_size", 15);
        _nameLabel.AddThemeColorOverride("font_color", LevelColors[0]);
        _nameLabel.Valign     = Label.Valign.Center;
        _hbox.AddChild(_nameLabel);

        // 脉冲覆盖层（用于 Frenzied 时的红色脉冲效果）
        _pulseOverlay = new PanelContainer();
        _pulseOverlay.ZIndex = 10;
        _pulseOverlay.MouseFilter = Control.MouseFilterEnum.Ignore;
        _pulseOverlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        var pulseStyle = new StyleBoxFlat();
        pulseStyle.BgColor       = new Color(1f, 0f, 0f, 0.0f);
        pulseStyle.CornerRadiusTopLeft     = 10;
        pulseStyle.CornerRadiusTopRight    = 10;
        pulseStyle.CornerRadiusBottomLeft  = 10;
        pulseStyle.CornerRadiusBottomRight = 10;
        _pulseOverlay.AddThemeStyleboxOverride("panel", pulseStyle);
        _panel.AddChild(_pulseOverlay);
    }

    private void SubscribeToSignals()
    {
        if (_subscriptionActive) return;

        var rhythmData = CombatRhythmData.Instance;
        if (rhythmData == null)
        {
            GD.Print("[CombatRhythmUI] CombatRhythmData not ready — will retry next frame");
            return;
        }

        rhythmData.RhythmLevelChanged += OnRhythmLevelChanged;
        EventBusManager.Instance?.Subscribe(EventBusManager.Events.CombatStarted, OnCombatStarted);
        EventBusManager.Instance?.Subscribe(EventBusManager.Events.CombatEnded,   OnCombatEnded);
        _subscriptionActive = true;

        GD.Print("[CombatRhythmUI] Subscribed to rhythm + combat events");
    }

    public override void _Process(double delta)
    {
        // 延迟订阅（等待 CombatRhythmData 就绪）
        if (!_subscriptionActive)
            SubscribeToSignals();
    }

    private void OnCombatStarted()
    {
        ShowRhythmUI();
    }

    private void OnCombatEnded()
    {
        HideRhythmUI(true);
    }

    private void OnRhythmLevelChanged(CombatRhythmData.RhythmLevel newLevel, CombatRhythmData.RhythmLevel oldLevel)
    {
        int idx = (int)newLevel;

        _iconLabel.Text = LevelEmojis[idx];
        _nameLabel.Text = LevelNames[idx];
        _nameLabel.AddThemeColorOverride("font_color", LevelColors[idx]);

        // 更新边框颜色
        var style = _panel.GetThemeStylebox("panel") as StyleBoxFlat;
        if (style != null)
            style.BorderColor = LevelColors[idx];

        // Frenzied 时开始脉冲动画
        if (newLevel == CombatRhythmData.RhythmLevel.Frenzied)
            StartPulseAnimation();
        else
            StopPulseAnimation();

        GD.Print($"[CombatRhythmUI] Level changed: {oldLevel} → {newLevel}");
    }

    private void ShowRhythmUI()
    {
        if (_visible) return;
        _visible = true;

        // 淡入
        var tween = CreateTween().SetParallel(true);
        tween.TweenProperty(_panel, "modulate:a", 1f, 0.3f);
        _panel.Modulate = new Color(1, 1, 1, 0);

        // 立即显示
        _panel.Show();
    }

    private void HideRhythmUI(bool animate)
    {
        if (!_visible && !_panel.Visible) return;
        _visible = false;

        if (!animate)
        {
            _panel.Hide();
            _panel.Modulate = new Color(1, 1, 1, 0);
            return;
        }

        // 淡出
        var tween = CreateTween().SetParallel(true);
        tween.TweenProperty(_panel, "modulate:a", 0f, 0.4f);
        tween.TweenCallback(new Callable(this, nameof(OnFadeOutComplete)));
    }

    private void OnFadeOutComplete()
    {
        _panel.Hide();
        _panel.Modulate = new Color(1, 1, 1, 1);
    }

    private void StartPulseAnimation()
    {
        StopPulseAnimation();

        _pulseTween = CreateTween().SetLoops(true);
        var pulseStyle = _pulseOverlay.GetThemeStylebox("panel") as StyleBoxFlat;

        // 红色脉冲：从 0 → 0.15 → 0 alpha 循环
        _pulseTween.TweenProperty(pulseStyle, "bg_color:a", 0.18f, 0.4f)
                   .SetTrans(Tween.TransitionType.Sine)
                   .SetEase(Tween.EaseType.InOut);
        _pulseTween.TweenProperty(pulseStyle, "bg_color:a", 0.0f, 0.4f)
                   .SetTrans(Tween.TransitionType.Sine)
                   .SetEase(Tween.EaseType.InOut);
    }

    private void StopPulseAnimation()
    {
        if (_pulseTween != null)
        {
            _pulseTween.Stop();
            _pulseTween = null;
        }
        var pulseStyle = _pulseOverlay.GetThemeStylebox("panel") as StyleBoxFlat;
        if (pulseStyle != null)
            pulseStyle.BgColor = new Color(1f, 0f, 0f, 0f);
    }

    public override void _ExitTree()
    {
        if (_instance == this)
            _instance = null;
    }
}
