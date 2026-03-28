using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Combo 发现通知系统
/// 订阅 ComboSystem.NewComboDiscovered 事件，显示华丽的 combo 发现通知
/// </summary>
public partial class ComboDiscoveryNotification : Control
{
    private CanvasLayer _canvasLayer;
    private VBoxContainer _container;
    private Queue<ComboData> _pendingNotifications = new();
    private bool _isShowing;
    private Timer _displayTimer;

    [Export] private float _displayDuration = 3.5f;
    [Export] private float _slideInDuration = 0.35f;
    [Export] private float _slideOutDuration = 0.4f;

    public override void _Ready()
    {
        Visible = false;

        _canvasLayer = new CanvasLayer();
        _canvasLayer.Layer = 200; // 高层级，确保在其他UI之上
        AddChild(_canvasLayer);

        _container = new VBoxContainer();
        _container.SetAnchorsPreset(Control.LayoutPreset.Center);
        _container.Position = new Vector2(-200, -120);
        _container.CustomMinimumSize = new Vector2(400, 0);
        _container.AddThemeConstantOverride("separation", 12);
        _canvasLayer.AddChild(_container);

        _displayTimer = new Timer();
        _displayTimer.OneShot = true;
        _displayTimer.Timeout += OnDisplayTimerTimeout;
        _canvasLayer.AddChild(_displayTimer);

        // 订阅 combo 发现事件
        ComboSystem.NewComboDiscovered += OnNewComboDiscovered;
    }

    public override void _ExitTree()
    {
        ComboSystem.NewComboDiscovered -= OnNewComboDiscovered;
    }

    private void OnNewComboDiscovered(ComboData combo)
    {
        _pendingNotifications.Enqueue(combo);
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
        var combo = _pendingNotifications.Dequeue();

        // 清除旧内容
        foreach (var child in _container.GetChildren())
        {
            child.QueueFree();
        }

        var notification = CreateComboDiscoveryPanel(combo);
        _container.AddChild(notification);

        // 入场动画：从下方滑入 + 淡入
        var tween = CreateTween();
        notification.Modulate = new Color(1, 1, 1, 0);
        notification.Position = new Vector2(0, 60);
        tween.SetParallel(true);
        tween.TweenProperty(notification, "modulate:a", 1f, _slideInDuration);
        tween.TweenProperty(notification, "position:y", 0f, _slideInDuration).SetTrans(Tween.TransitionType.Back);

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
            tween.TweenCallback(Callable.From(() =>
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

    private Control CreateComboDiscoveryPanel(ComboData combo)
    {
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(400, 0);

        // 根据稀有度设置边框颜色
        Color rarityColor = GetRarityColor(combo.comboRarity);

        var style = new StyleBoxFlat();
        style.BorderWidthLeft = 3;
        style.BorderWidthRight = 3;
        style.BorderWidthTop = 3;
        style.BorderWidthBottom = 3;
        style.BorderColor = rarityColor;
        style.BgColor = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        style.CornerRadiusTopLeft = 12;
        style.CornerRadiusTopRight = 12;
        style.CornerRadiusBottomLeft = 12;
        style.CornerRadiusBottomRight = 12;
        style.ContentMarginLeft = 20;
        style.ContentMarginTop = 16;
        style.ContentMarginRight = 20;
        style.ContentMarginBottom = 16;
        panel.AddThemeStyleboxOverride("panel", style);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 8);
        panel.AddChild(vbox);

        // 顶部标题行
        var headerRow = new HBoxContainer();
        headerRow.Alignment = BoxContainer.AlignmentMode.Center;
        vbox.AddChild(headerRow);

        var sparkleLabel = new Label();
        sparkleLabel.Text = "✨";
        sparkleLabel.AddThemeFontSizeOverride("font_size", 24);
        sparkleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        headerRow.AddChild(sparkleLabel);

        var titleLabel = new Label();
        titleLabel.Text = "NEW COMBO DISCOVERED";
        titleLabel.AddThemeFontSizeOverride("font_size", 13);
        titleLabel.Modulate = new Color(0.8f, 0.8f, 0.8f);
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        headerRow.AddChild(titleLabel);

        var sparkleLabel2 = new Label();
        sparkleLabel2.Text = "✨";
        sparkleLabel2.AddThemeFontSizeOverride("font_size", 24);
        sparkleLabel2.HorizontalAlignment = HorizontalAlignment.Center;
        headerRow.AddChild(sparkleLabel2);

        // Combo 名称（稀有度颜色）
        var comboNameLabel = new Label();
        comboNameLabel.Text = combo.comboName ?? combo.comboId;
        comboNameLabel.AddThemeFontSizeOverride("font_size", 28);
        comboNameLabel.Modulate = rarityColor;
        comboNameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        comboNameLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        vbox.AddChild(comboNameLabel);

        // 稀有度标签
        var rarityRow = new HBoxContainer();
        rarityRow.Alignment = BoxContainer.AlignmentMode.Center;
        vbox.AddChild(rarityRow);

        var rarityBadge = new Label();
        rarityBadge.Text = GetRarityLabel(combo.comboRarity);
        rarityBadge.AddThemeFontSizeOverride("font_size", 14);
        rarityBadge.Modulate = rarityColor;
        rarityRow.AddChild(rarityBadge);

        // 分隔线
        var separator = new HSeparator();
        separator.Modulate = new Color(rarityColor.R, rarityColor.G, rarityColor.B, 0.4f);
        vbox.AddChild(separator);

        // 描述
        if (!string.IsNullOrEmpty(combo.description))
        {
            var descLabel = new Label();
            descLabel.Text = combo.description;
            descLabel.AddThemeFontSizeOverride("font_size", 13);
            descLabel.Modulate = new Color(0.75f, 0.75f, 0.75f);
            descLabel.HorizontalAlignment = HorizontalAlignment.Center;
            descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            vbox.AddChild(descLabel);
        }

        // 属性行
        var statsRow = new HBoxContainer();
        statsRow.Alignment = BoxContainer.AlignmentMode.Center;
        statsRow.AddThemeConstantOverride("separation", 20);
        vbox.AddChild(statsRow);

        // 伤害倍率
        if (combo.damageMultiplier > 0)
        {
            var dmgLabel = new Label();
            dmgLabel.Text = $"⚔️ {combo.damageMultiplier:F1}x DMG";
            dmgLabel.AddThemeFontSizeOverride("font_size", 14);
            dmgLabel.Modulate = new Color(1f, 0.5f, 0.5f);
            statsRow.AddChild(dmgLabel);
        }

        // 冷却缩减
        if (combo.cooldownReduction > 0)
        {
            var cdLabel = new Label();
            cdLabel.Text = $"⏱️ -{combo.cooldownReduction * 100:F0}% CD";
            cdLabel.AddThemeFontSizeOverride("font_size", 14);
            cdLabel.Modulate = new Color(0.5f, 0.8f, 1f);
            statsRow.AddChild(cdLabel);
        }

        // 连击点奖励
        if (combo.comboPointReward > 0)
        {
            var cpLabel = new Label();
            cpLabel.Text = $"⭐ +{combo.comboPointReward} CP";
            cpLabel.AddThemeFontSizeOverride("font_size", 14);
            cpLabel.Modulate = new Color(1f, 0.85f, 0.4f);
            statsRow.AddChild(cpLabel);
        }

        // 特效名称（如果有）
        if (!string.IsNullOrEmpty(combo.effectName))
        {
            var effectLabel = new Label();
            effectLabel.Text = $"▶ {combo.effectName}";
            effectLabel.AddThemeFontSizeOverride("font_size", 12);
            effectLabel.Modulate = new Color(0.6f, 0.9f, 0.6f);
            effectLabel.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(effectLabel);
        }

        return panel;
    }

    private Color GetRarityColor(ComboData.Rarity rarity)
    {
        return rarity switch
        {
            ComboData.Rarity.Common => new Color(0.6f, 0.6f, 0.6f),      // 灰色
            ComboData.Rarity.Uncommon => new Color(0.2f, 0.8f, 0.3f),    // 绿色
            ComboData.Rarity.Rare => new Color(0.2f, 0.5f, 1f),          // 蓝色
            ComboData.Rarity.Epic => new Color(0.6f, 0.2f, 0.9f),       // 紫色
            ComboData.Rarity.Legendary => new Color(1f, 0.6f, 0.1f),    // 橙色
            _ => new Color(0.6f, 0.6f, 0.6f)
        };
    }

    private string GetRarityLabel(ComboData.Rarity rarity)
    {
        return rarity switch
        {
            ComboData.Rarity.Common => "⬜ COMMON",
            ComboData.Rarity.Uncommon => "🟢 UNCOMMON",
            ComboData.Rarity.Rare => "🔵 RARE",
            ComboData.Rarity.Epic => "🟣 EPIC",
            ComboData.Rarity.Legendary => "🟠 LEGENDARY",
            _ => "⚪ COMMON"
        };
    }
}
