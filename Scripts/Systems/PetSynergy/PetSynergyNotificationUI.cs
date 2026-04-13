using Godot;
using System;
using ClawRPG.Systems.PetFormation;

/// <summary>
/// 宠物默契技能通知UI (REQ-163-04)
/// 显示配合动画触发时的纯视觉金标提示。
/// 不同阵型显示不同颜色的通知边框（前锋=红/铁桶=蓝/均衡=绿等）。
/// </summary>
public partial class PetSynergyNotificationUI : Control
{
    // Config
    [Export] private float _displayDuration = 2.5f;
    [Export] private float _fadeOutDuration = 0.5f;

    // UI Elements
    private PanelContainer _panel;
    private Label _skillNameLabel;
    private Label _formationLabel;
    private Label _animLabel;
    private TextureRect _icon;

    // State
    private bool _isShowing;
    private Timer _displayTimer;

    public override void _Ready()
    {
        SetupUI();
        SubscribeToSignals();
        Hide();
    }

    private void SetupUI()
    {
        Name = "PetSynergyNotificationUI";
        AnchorsPreset = AnchorsPreset.Custom;
        CustomMinimumSize = new Vector2(320, 80);

        // Position: right side of screen, vertically centered
        AnchorLeft = 1.0f;
        AnchorTop = 0.5f;
        AnchorRight = 1.0f;
        AnchorBottom = 0.5f;
        OffsetLeft = -340;
        OffsetTop = -60;
        OffsetRight = -20;
        OffsetBottom = 20;

        // Main panel
        _panel = new PanelContainer();
        _panel.CustomMinimumSize = new Vector2(320, 80);
        var panelStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.05f, 0.05f, 0.1f, 0.92f),
            BorderWidthLeft = 3,
            BorderWidthTop = 3,
            BorderWidthRight = 3,
            BorderWidthBottom = 3,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8
        };
        // Default: gold border (friendship tier)
        panelStyle.BorderColor = new Color(1.0f, 0.85f, 0.2f, 1.0f);
        _panel.AddThemeStyleboxOverride("panel", panelStyle);
        AddChild(_panel);

        var vbox = new VBoxContainer();
        vbox.Alignment = BoxContainer.AlignmentMode.Center;
        vbox.AddThemeConstantOverride("separation", 4);
        _panel.AddChild(vbox);

        // Header row: icon + skill name
        var headerHbox = new HBoxContainer();
        headerHbox.Alignment = BoxContainer.AlignmentMode.Center;
        vbox.AddChild(headerHbox);

        _icon = new TextureRect
        {
            CustomMinimumSize = new Vector2(24, 24),
            Expand = true,
            StretchMode = TextureRect.StretchModeEnum.KeepSizeCentered
        };
        // Use a simple colored square as placeholder icon
        var iconTex = new AtlasTexture
        {
            Atlas = null, // Would be set to a spritesheet in production
        };
        _icon.Texture = iconTex;
        headerHbox.AddChild(_icon);

        var spacer = new Control { CustomMinimumSize = new Vector2(8, 0) };
        headerHbox.AddChild(spacer);

        _skillNameLabel = new Label
        {
            Text = "⚡ 默契技能",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _skillNameLabel.AddThemeFontSizeOverride("font_size", 16);
        headerHbox.AddChild(_skillNameLabel);

        // Formation label
        _formationLabel = new Label
        {
            Text = "",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _formationLabel.AddThemeFontSizeOverride("font_size", 12);
        _formationLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f, 1f));
        vbox.AddChild(_formationLabel);

        // Animation label
        _animLabel = new Label
        {
            Text = "",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _animLabel.AddThemeFontSizeOverride("font_size", 13);
        _animLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f, 1f));
        vbox.AddChild(_animLabel);

        Hide();
    }

    private void SubscribeToSignals()
    {
        var trigger = PetSynergySkillTrigger.Instance;
        if (trigger != null)
        {
            trigger.OnSynergyAnimationRequested += OnSynergyAnimationRequested;
        }
    }

    private void OnSynergyAnimationRequested(string petId, string animation, string skillId, FormationType formation)
    {
        ShowNotification(petId, animation, skillId, formation);
    }

    private void ShowNotification(string petId, string animation, string skillId, FormationType formation)
    {
        if (_isShowing)
        {
            // Interrupt current animation, start new one
            _displayTimer?.Stop();
            _displayTimer?.QueueFree();
        }

        _isShowing = true;

        // Update labels
        _skillNameLabel.Text = $"⚡ {skillId}";
        _formationLabel.Text = GetFormationDisplayName(formation);
        _animLabel.Text = $"{animation}";

        // Color-code by formation (REQ-176-05 extension)
        var panelStyle = _panel.GetThemeStylebox("panel") as StyleBoxFlat;
        if (panelStyle != null)
        {
            panelStyle.BorderColor = GetFormationColor(formation);
        }

        // Scale-in animation
        Modulate = new Color(1f, 1f, 1f, 1f);
        Scale = new Vector2(0.8f, 0.8f);
        var tweenIn = CreateTween();
        tweenIn.TweenProperty(this, "scale", new Vector2(1.05f, 1.05f), 0.15f);
        tweenIn.TweenProperty(this, "scale", new Vector2(1.0f, 1.0f), 0.08f);
        tweenIn.Parallel().TweenProperty(this, "modulate:a", 1f, 0.1f);

        Show();

        // Auto-hide after duration
        _displayTimer = new Timer { OneShot = true, WaitTime = _displayDuration };
        _displayTimer.Timeout += OnDisplayTimerExpired;
        AddChild(_displayTimer);
        _displayTimer.Start();
    }

    private void OnDisplayTimerExpired()
    {
        if (!IsInsideTree()) return;

        // Fade out
        var tweenOut = CreateTween();
        tweenOut.TweenProperty(this, "modulate:a", 0f, _fadeOutDuration);
        tweenOut.TweenCallback(Callable.From(HideAndReset));

        _displayTimer?.QueueFree();
        _displayTimer = null;
        _isShowing = false;
    }

    private void HideAndReset()
    {
        Hide();
        Scale = new Vector2(1f, 1f);
    }

    private Color GetFormationColor(FormationType formation)
    {
        return formation switch
        {
            FormationType.AggressiveRush => new Color(1.0f, 0.3f, 0.3f, 1f),    // Red - aggressive
            FormationType.GuardFormation => new Color(0.3f, 0.6f, 1.0f, 1f),    // Blue - defensive
            FormationType.Balanced => new Color(0.3f, 1.0f, 0.3f, 1f),          // Green - balanced
            FormationType.PincerSetup => new Color(1.0f, 0.6f, 0.1f, 1f),     // Orange - flanking
            FormationType.FlexibleAssault => new Color(0.8f, 0.4f, 1.0f, 1f), // Purple - flexible
            FormationType.Solo => new Color(0.6f, 0.6f, 0.6f, 1f),            // Gray - solo
            _ => new Color(1.0f, 0.85f, 0.2f, 1f)                              // Gold - default
        };
    }

    private string GetFormationDisplayName(FormationType formation)
    {
        return formation switch
        {
            FormationType.AggressiveRush => "🏃 全力突击阵型",
            FormationType.GuardFormation => "🛡 铁桶阵型",
            FormationType.Balanced => "⚖ 攻守平衡阵型",
            FormationType.PincerSetup => "✂ 钳形攻势阵型",
            FormationType.FlexibleAssault => "🔄 灵活突击阵型",
            FormationType.Solo => "🐾 单独作战",
            _ => ""
        };
    }

    public override void _Notification(int what)
    {
        if (what == NotificationExitTree)
        {
            var trigger = PetSynergySkillTrigger.Instance;
            if (trigger != null)
            {
                trigger.OnSynergyAnimationRequested -= OnSynergyAnimationRequested;
            }
            _displayTimer?.QueueFree();
        }
    }
}
