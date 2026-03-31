using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Chaos Combo 通知系统 (REQ-167-05)
/// 订阅 ComboSystem.ChaosComboExecuted 事件，显示混沌 combo 随机选中的技能列表
/// </summary>
public partial class ChaosComboNotification : Control
{
    private CanvasLayer _canvasLayer;
    private VBoxContainer _container;
    private Queue<(string comboId, List<string> skills)> _pendingNotifications = new();
    private bool _isShowing;
    private Timer _displayTimer;

    [Export] private float _displayDuration = 2.5f;
    [Export] private float _slideInDuration = 0.3f;
    [Export] private float _slideOutDuration = 0.4f;

    public override void _Ready()
    {
        Visible = false;

        _canvasLayer = new CanvasLayer();
        _canvasLayer.Layer = 200;
        AddChild(_canvasLayer);

        _container = new VBoxContainer();
        _container.SetAnchorsPreset(Control.LayoutPreset.Center);
        _container.Position = new Vector2(-220, -100);
        _container.CustomMinimumSize = new Vector2(440, 0);
        _container.AddThemeConstantOverride("separation", 10);
        _canvasLayer.AddChild(_container);

        _displayTimer = new Timer();
        _displayTimer.OneShot = true;
        _displayTimer.Timeout += OnDisplayTimerTimeout;
        _canvasLayer.AddChild(_displayTimer);

        // 订阅 chaos combo 执行事件
        ComboSystem.ChaosComboExecuted += OnChaosComboExecuted;
    }

    public override void _ExitTree()
    {
        ComboSystem.ChaosComboExecuted -= OnChaosComboExecuted;
    }

    private void OnChaosComboExecuted(string comboId, List<string> selectedSkills)
    {
        _pendingNotifications.Enqueue((comboId, selectedSkills));
        if (!_isShowing)
        {
            ShowNextNotification();
        }
    }

    private void ShowNextNotification()
    {
        if (_pendingNotifications.Count == 0)
        {
            _isShowing = false;
            Visible = false;
            return;
        }

        _isShowing = true;
        Visible = true;
        var (comboId, skills) = _pendingNotifications.Dequeue();

        // 清除旧内容
        foreach (var child in _container.GetChildren())
        {
            child.QueueFree();
        }

        ComboSystem.Instance.GetAllCombos().TryGetValue(comboId, out var combo);
        var notification = CreateChaosComboPanel(comboId, skills, combo);
        _container.AddChild(notification);

        // 入场动画：滑入 + 淡入
        var tween = CreateTween();
        notification.Modulate = new Color(1, 1, 1, 0);
        notification.Position = new Vector2(0, 50);
        tween.SetParallel(true);
        tween.TweenProperty(notification, "modulate:a", 1f, _slideInDuration);
        tween.TweenProperty(notification, "position:y", 0f, _slideInDuration)
            .SetTrans(Tween.TransitionType.Back);

        // 启动显示计时器
        _displayTimer.Stop();
        _displayTimer.Start(_displayDuration);
    }

    private void OnDisplayTimerTimeout()
    {
        if (_container.GetChildCount() > 0)
        {
            var notification = _container.GetChild(0);
            var tween = CreateTween();
            tween.TweenProperty(notification, "modulate:a", 0f, _slideOutDuration);
            tween.Callback(Callable.From(() =>
            {
                foreach (var child in _container.GetChildren())
                {
                    child.QueueFree();
                }
                ShowNextNotification();
            }));
        }
        else
        {
            ShowNextNotification();
        }
    }

    private Control CreateChaosComboPanel(string comboId, List<string> skills, ComboData combo)
    {
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(440, 0);

        // 混沌主题：紫色渐变边框
        var style = new StyleBoxFlat();
        style.BorderWidthLeft = 3;
        style.BorderWidthRight = 3;
        style.BorderWidthTop = 3;
        style.BorderWidthBottom = 3;
        style.BorderColor = new Color(0.7f, 0.3f, 1f, 1f);  // 紫色
        style.BgColor = new Color(0.08f, 0.04f, 0.15f, 0.95f);
        style.CornerRadiusTopLeft = 12;
        style.CornerRadiusTopRight = 12;
        style.CornerRadiusBottomLeft = 12;
        style.CornerRadiusBottomRight = 12;
        style.ContentMarginLeft = 20;
        style.ContentMarginTop = 14;
        style.ContentMarginRight = 20;
        style.ContentMarginBottom = 14;
        panel.AddThemeStyleboxOverride("panel", style);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 8);
        panel.AddChild(vbox);

        // 🎲 混沌标题
        var titleLabel = new Label();
        titleLabel.Text = $"🎲 混沌连击！";
        titleLabel.AddThemeFontSizeOverride("font_size", 22);
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.6f, 1f));
        vbox.AddChild(titleLabel);

        // 连击名称
        var nameLabel = new Label();
        nameLabel.Text = combo?.comboName ?? comboId;
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        nameLabel.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 0.9f));
        vbox.AddChild(nameLabel);

        // 分隔线
        var separator = new HSeparator();
        separator.AddThemeConstantOverride("separation", 4);
        vbox.AddChild(separator);

        // 技能列表
        var skillsLabel = new Label();
        skillsLabel.Text = string.Join(" + ", skills);
        skillsLabel.AddThemeFontSizeOverride("font_size", 15);
        skillsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        skillsLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.4f));  // 金色
        vbox.AddChild(skillsLabel);

        // 底部装饰
        var hintLabel = new Label();
        hintLabel.Text = "[ 混沌之力 ]";
        hintLabel.AddThemeFontSizeOverride("font_size", 11);
        hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
        hintLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.4f, 0.8f, 0.7f));
        vbox.AddChild(hintLabel);

        return panel;
    }
}
