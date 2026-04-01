using Godot;
using System;

/// <summary>
/// 宠物重逢叙事UI - 监听 PetFriendshipSystem.PetReunion 信号
/// 当两只宠物带着历史友谊重逢时显示叙事面板
/// </summary>
public partial class PetReunionNarrativeUI : CanvasLayer
{
    private PanelContainer _panel;
    private Label _titleLabel;
    private Label _bodyLabel;
    private Tween _tween;

    /// <summary>
    /// 叙事文本模板
    /// </summary>
    private static readonly string[] REUNION_TITLES = {
        "老朋友重逢！",
        "久别重逢！",
        "熟悉的伙伴！"
    };

    public override void _Ready()
    {
        Visible = false;

        // 创建UI结构
        _panel = new PanelContainer();
        _panel.SetAnchor(AnchorsPreset.CenterRight, true);
        _panel.SetAnchor(AnchorsPreset.CenterRight, true);
        _panel.SetAnchor(AnchorsPreset.CenterRight, true);
        _panel.SetAnchor(AnchorsPreset.CenterRight, true);
        _panel.OffsetLeft = -400;
        _panel.OffsetTop = 100;
        _panel.OffsetRight = -20;
        _panel.OffsetBottom = 280;
        AddChild(_panel);

        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.08f, 0.05f, 0.15f, 0.92f);
        style.BorderColorLeft = new Color(0.6f, 0.4f, 0.9f, 0.8f);
        style.BorderColorRight = new Color(0.6f, 0.4f, 0.9f, 0.8f);
        style.BorderColorTop = new Color(0.6f, 0.4f, 0.9f, 0.8f);
        style.BorderColorBottom = new Color(0.6f, 0.4f, 0.9f, 0.8f);
        style.BorderWidthLeft = 2;
        style.BorderWidthRight = 2;
        style.BorderWidthTop = 2;
        style.BorderWidthBottom = 2;
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
        style.ContentMarginLeft = 16;
        style.ContentMarginTop = 12;
        style.ContentMarginRight = 16;
        style.ContentMarginBottom = 12;
        _panel.AddThemeStyleboxOverride("panel", style);

        var vbox = new VBoxContainer();
        _panel.AddChild(vbox);

        _titleLabel = new Label();
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.65f, 1.0f, 1.0f));
        vbox.AddChild(_titleLabel);

        _bodyLabel = new Label();
        _bodyLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _bodyLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        _bodyLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.95f, 0.9f));
        vbox.AddChild(_bodyLabel);

        // 订阅重逢信号
        if (PetFriendshipSystem.Instance != null)
        {
            PetFriendshipSystem.Instance.PetReunion += OnPetReunion;
        }
    }

    private void OnPetReunion(int petIdA, int petIdB, int historicalFriendship, int totalBattles)
    {
        string petNameA = GetPetDisplayName(petIdA);
        string petNameB = GetPetDisplayName(petIdB);

        string title = REUNION_TITLES[GD.Randi() % REUNION_TITLES.Length];
        string tierDesc = GetTierDescription(historicalFriendship);
        string battleDesc = totalBattles > 10 ? $"{totalBattles}次并肩作战" : $"{totalBattles}次共同战斗";

        _titleLabel.Text = title;
        _bodyLabel.Text = $"{petNameA} 和 {petNameB}\n{ tierDesc } · {battleDesc}";

        ShowWithAnimation();
    }

    private string GetPetDisplayName(int petId)
    {
        // 尝试从宠物数据库获取名字
        // fallback: 显示 ID
        return $"宠物 #{petId}";
    }

    private string GetTierDescription(int level)
    {
        if (level >= 16) return "灵魂伴侣";
        if (level >= 11) return "挚友";
        if (level >= 7) return "好友";
        if (level >= 4) return "熟人";
        return "相识";
    }

    private void ShowWithAnimation()
    {
        if (_tween != null && _tween.IsValid())
            _tween.Kill();

        Visible = true;
        Modulate = new Color(1, 1, 1, 0);
        _panel.Position = new Vector2(50, _panel.Position.Y);

        _tween = CreateTween();
        _tween.SetParallel(true);
        _tween.TweenProperty(this, "modulate:a", 1.0f, 0.4f);
        _tween.TweenProperty(_panel, "position:x", 0, 0.4f)
            .From(50);
        _tween.TweenInterval(2.5f);
        _tween.TweenProperty(this, "modulate:a", 0.0f, 0.5f);
        _tween.TweenCallback(new Callable(this, nameof(OnFadeOutComplete)));
    }

    private void OnFadeOutComplete()
    {
        Visible = false;
    }

    public override void _ExitTree()
    {
        if (PetFriendshipSystem.Instance != null)
        {
            PetFriendshipSystem.Instance.PetReunion -= OnPetReunion;
        }
    }
}
