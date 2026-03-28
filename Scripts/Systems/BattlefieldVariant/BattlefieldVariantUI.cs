using Godot;
using System;

/// <summary>
/// 战场变体 UI（REQ-115-05）
/// 战斗开始时显示变体名称+图标+效果描述，战斗中实时显示效果触发
/// </summary>
public class BattlefieldVariantUI : CanvasLayer
{
    // 变体提示面板
    private PanelContainer _variantPanel;
    private Label _variantNameLabel;
    private Label _variantDescLabel;
    private TextureRect _variantIcon;
    private PanelContainer _effectLabel; // 实时效果触发提示

    // 淡出动画
    private Tween _fadeTween;
    private float _introDisplayDuration = 3.0f;

    // 效果提示队列
    private float _effectLabelTimer = 0f;
    private float _effectLabelDuration = 2.0f;

    public override void _Ready()
    {
        _instance = this;

        SetupUI();
        SubscribeToSignals();
        HideVariantPanel();
    }

    private static BattlefieldVariantUI _instance;
    public static BattlefieldVariantUI Instance => _instance;

    private void SetupUI()
    {
        // 主面板（屏幕顶部中央）
        _variantPanel = new PanelContainer();
        _variantPanel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        _variantPanel.MarginTop = 20;
        _variantPanel.MarginLeft = 100;
        _variantPanel.MarginRight = -100;
        _variantPanel.CustomMinimumSize = new Vector2(0, 60);
        AddChild(_variantPanel);

        // 背景样式
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.85f);
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
        style.ContentMarginLeft = 20;
        style.ContentMarginTop = 12;
        style.ContentMarginRight = 20;
        style.ContentMarginBottom = 12;
        _variantPanel.AddThemeStyleboxOverride("panel", style);

        // HBox 容器
        var hbox = new HBoxContainer();
        hbox.Alignment = BoxContainer.AlignmentMode.Center;
        hbox.CustomMinimumSize = new Vector2(0, 40);
        _variantPanel.AddChild(hbox);

        // 图标占位
        _variantIcon = new TextureRect();
        _variantIcon.CustomMinimumSize = new Vector2(32, 32);
        _variantIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        hbox.AddChild(_variantIcon);

        // 名称标签
        _variantNameLabel = new Label();
        _variantNameLabel.Text = "";
        _variantNameLabel.AddThemeFontSizeOverride("font_size", 20);
        _variantNameLabel.AddThemeColorOverride("font_color", Colors.White);
        hbox.AddChild(_variantNameLabel);

        // 分隔
        var sep = new Label();
        sep.Text = "  |  ";
        sep.AddThemeFontSizeOverride("font_size", 18);
        sep.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        hbox.AddChild(sep);

        // 描述标签
        _variantDescLabel = new Label();
        _variantDescLabel.Text = "";
        _variantDescLabel.AddThemeFontSizeOverride("font_size", 16);
        _variantDescLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.85f, 0.85f));
        hbox.AddChild(_variantDescLabel);

        // 效果触发提示标签（屏幕底部中央）
        _effectLabel = new PanelContainer();
        _effectLabel.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
        _effectLabel.MarginBottom = 120;
        _effectLabel.MarginLeft = 200;
        _effectLabel.MarginRight = -200;
        _effectLabel.Hide();
        AddChild(_effectLabel);

        var effectStyle = new StyleBoxFlat();
        effectStyle.BgColor = new Color(0.05f, 0.05f, 0.1f, 0.9f);
        effectStyle.CornerRadiusTopLeft = 6;
        effectStyle.CornerRadiusTopRight = 6;
        effectStyle.CornerRadiusBottomLeft = 6;
        effectStyle.CornerRadiusBottomRight = 6;
        effectStyle.ContentMarginLeft = 16;
        effectStyle.ContentMarginTop = 8;
        effectStyle.ContentMarginRight = 16;
        effectStyle.ContentMarginBottom = 8;
        _effectLabel.AddThemeStyleboxOverride("panel", effectStyle);

        var effectText = new Label();
        effectText.Name = "EffectText";
        effectText.AddThemeFontSizeOverride("font_size", 15);
        effectText.HorizontalAlignment = HorizontalAlignment.Center;
        _effectLabel.AddChild(effectText);
    }

    private void SubscribeToSignals()
    {
        if (BattlefieldVariantSystem.Instance != null)
        {
            BattlefieldVariantSystem.Instance.Connect("VariantSelected", this, nameof(OnVariantSelected));
            BattlefieldVariantSystem.Instance.Connect("VariantEffectTriggered", this, nameof(OnVariantEffectTriggered));
            BattlefieldVariantSystem.Instance.Connect("VariantExited", this, nameof(OnVariantExited));
        }
    }

    private void OnVariantSelected(BattlefieldVariantType variant)
    {
        var config = BattlefieldVariantSystem.Instance.GetCurrentVariantConfig();
        if (config == null) return;

        // 更新面板内容
        _variantNameLabel.Text = $"【{config.DisplayName}】";
        _variantDescLabel.Text = config.Description;

        // 根据变体类型设置颜色
        Color variantColor = config.IconColor;
        _variantNameLabel.AddThemeColorOverride("font_color", variantColor);

        // 淡入显示
        ShowVariantPanel();
        PlayIntroAnimation();
    }

    private void OnVariantEffectTriggered(BattlefieldVariantType variant, string effectDesc)
    {
        // 显示效果触发提示
        ShowEffectLabel(effectDesc, variant);
    }

    private void OnVariantExited(BattlefieldVariantType variant)
    {
        HideVariantPanel();
        _effectLabel.Hide();
    }

    private void ShowVariantPanel()
    {
        _variantPanel.Show();
        Modulate = new Color(1, 1, 1, 0);
    }

    private void PlayIntroAnimation()
    {
        // 淡入 + 3秒后淡出
        _fadeTween = CreateTween();
        _fadeTween.TweenProperty(this, "modulate:a", 1.0f, 0.3f);
        _fadeTween.TweenInterval(_introDisplayDuration);
        _fadeTween.TweenProperty(this, "modulate:a", 0.0f, 0.5f);
        _fadeTween.Connect("finished", this, nameof(OnIntroFadeFinished));
    }

    private void OnIntroFadeFinished()
    {
        // 变体介绍淡出后，保持面板隐藏但系统仍然运行
        _variantPanel.Hide();
    }

    private void ShowEffectLabel(string text, BattlefieldVariantType variant)
    {
        var config = BattlefieldVariantSystem.Instance.GetCurrentVariantConfig();
        Color variantColor = config?.IconColor ?? Colors.Yellow;

        var effectText = _effectLabel.GetNode<Label>("EffectText");
        effectText.Text = text;
        effectText.AddThemeColorOverride("font_color", variantColor);

        _effectLabel.Show();
        _effectLabel.Modulate = new Color(1, 1, 1, 0);
        _effectLabel.SelfModulate = new Color(1, 1, 1, 0);

        _fadeTween = CreateTween();
        _fadeTween.TweenProperty(_effectLabel, "modulate:a", 1.0f, 0.2f);
        _fadeTween.TweenInterval(_effectLabelDuration);
        _fadeTween.TweenProperty(_effectLabel, "modulate:a", 0.0f, 0.4f);
        _fadeTween.Connect("finished", this, nameof(OnEffectLabelFadeFinished));
    }

    private void OnEffectLabelFadeFinished()
    {
        _effectLabel.Hide();
    }

    public override void _Process(double delta)
    {
        // 如果变体面板正在显示，更新位置（适应不同屏幕）
    }
}
