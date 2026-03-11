using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI;

/// <summary>
/// 成就解锁通知弹窗系统
/// 应用信号系统模式：当成就解锁时显示弹窗通知
/// </summary>
public partial class AchievementNotification : Control
{
    [Export] private float _displayDuration = 4.0f;
    [Export] private float _slideInDuration = 0.3f;
    [Export] private float _slideOutDuration = 0.3f;
    [Export] private int _maxQueue = 3;

    private VBoxContainer _container;
    private Queue<AchievementPopupData> _popupQueue = new();
    private bool _isShowing;
    private List<Control> _activePopups = new();

    private class AchievementPopupData
    {
        public string Title;
        public string Description;
        public string Difficulty;
        public int GoldReward;
        public int XpReward;
    }

    public override void _Ready()
    {
        Visible = false; 
        
        // 创建容器
        _container = new VBoxContainer();
        _container.SetAnchorsPreset(Control.LayoutPreset.TopRight);
        _container.Position = new Vector2(30, 30);
        _container.AddThemeConstantOverride("separation", 10);
        AddChild(_container);

        // 直接连接到成就管理器信号（使用单例）
        AchievementManager.Instance.OnAchievementUnlocked += OnAchievementUnlockedHandler;
    }

    public override void _ExitTree()
    {
        // 断开信号连接
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnAchievementUnlocked -= OnAchievementUnlockedHandler;
        }
    }

    private void OnAchievementUnlockedHandler(Achievement achievement)
    {
        string title = achievement.Name ?? "成就解锁";
        string description = achievement.Description ?? "";
        string difficulty = achievement.Difficulty.ToString();
        int goldReward = achievement.GoldReward;
        int xpReward = achievement.XpReward;

        var data = new AchievementPopupData
        {
            Title = title,
            Description = description,
            Difficulty = difficulty,
            GoldReward = goldReward,
            XpReward = xpReward
        };

        _popupQueue.Enqueue(data);
        
        if (!_isShowing)
        {
            ShowNextPopup();
        }
    }

    private void ShowNextPopup()
    {
        if (_popupQueue.Count == 0)
        {
            _isShowing = false; 
            Visible = false; 
            return;
        }

        _isShowing = true;
        Visible = true;
        var data = _popupQueue.Dequeue();
        var popup = CreatePopup(data);
        _container.AddChild(popup);
        _activePopups.Add(popup);

        // 限制同时显示的数量
        while (_activePopups.Count > _maxQueue)
        {
            var oldest = _activePopups[0];
            _activePopups.RemoveAt(0);
            oldest.QueueFree();
        }

        // 动画进入
        var tween = CreateTween();
        tween.SetParallel(true);
        popup.Modulate = new Color(1, 1, 1, 0);
        popup.Position = new Vector2(300, 0);
        tween.TweenProperty(popup, "modulate:a", 1f, _slideInDuration);
        tween.TweenProperty(popup, "position:x", 0f, _slideInDuration);
        
        // 显示一段时间后消失
        tween = CreateTween();
        tween.TweenInterval(_displayDuration);
        tween.TweenCallback(Callable.From(() => RemovePopup(popup)));
    }

    private void RemovePopup(Control popup)
    {
        if (!IsInstanceValid(popup)) return;
        
        var tween = CreateTween();
        tween.TweenProperty(popup, "modulate:a", 0f, _slideOutDuration);
        tween.TweenCallback(Callable.From(() =>
        {
            if (IsInstanceValid(popup))
            {
                popup.QueueFree();
                _activePopups.Remove(popup);
            }
            ShowNextPopup();
        }));
    }

    private Control CreatePopup(AchievementPopupData data)
    {
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(280, 0);
        
        // 根据难度设置颜色
        Color borderColor = data.Difficulty switch
        {
            "简单" => new Color(0.5f, 0.5f, 0.5f),
            "普通" => new Color(0.2f, 0.6f, 0.2f),
            "困难" => new Color(0.2f, 0.4f, 0.8f),
            "史诗" => new Color(0.5f, 0.2f, 0.8f),
            "传说" => new Color(1f, 0.6f, 0f),
            _ => new Color(0.5f, 0.5f, 0.5f)
        };
        
        var style = new StyleBoxFlat();
        style.BorderWidthLeft = 3;
        style.BorderWidthRight = 3;
        style.BorderWidthTop = 3;
        style.BorderWidthBottom = 3;
        style.BorderColor = borderColor;
        style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
        panel.AddThemeStyleboxOverride("panel", style);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 5);
        panel.AddChild(vbox);

        // 标题行
        var titleRow = new HBoxContainer();
        titleRow.AddThemeConstantOverride("separation", 8);
        vbox.AddChild(titleRow);

        var icon = new TextureRect();
        icon.CustomMinimumSize = new Vector2(24, 24);
        // 使用星星图标
        var starTexture = CreateStarTexture();
        icon.Texture = starTexture;
        titleRow.AddChild(icon);

        var titleLabel = new Label();
        titleLabel.Text = "🎉 " + data.Title;
        titleLabel.AddThemeFontSizeOverride("font_size", 16);
        titleLabel.Modulate = borderColor;
        titleRow.AddChild(titleLabel);

        // 难度标签
        var difficultyLabel = new Label();
        difficultyLabel.Text = data.Difficulty;
        difficultyLabel.AddThemeFontSizeOverride("font_size", 12);
        difficultyLabel.Modulate = borderColor;
        difficultyLabel.HorizontalAlignment = HorizontalAlignment.Right;
        titleRow.AddChild(difficultyLabel);

        // 描述
        if (!string.IsNullOrEmpty(data.Description))
        {
            var descLabel = new Label();
            descLabel.Text = data.Description;
            descLabel.AddThemeFontSizeOverride("font_size", 12);
            descLabel.Modulate = new Color(0.8f, 0.8f, 0.8f);
            descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            vbox.AddChild(descLabel);
        }

        // 奖励行
        if (data.GoldReward > 0 || data.XpReward > 0)
        {
            var rewardRow = new HBoxContainer();
            rewardRow.Alignment = BoxContainer.AlignmentMode.Center;
            rewardRow.AddThemeConstantOverride("separation", 15);
            vbox.AddChild(rewardRow);

            if (data.GoldReward > 0)
            {
                var goldLabel = new Label();
                goldLabel.Text = $"💰 +{data.GoldReward}";
                goldLabel.AddThemeFontSizeOverride("font_size", 14);
                goldLabel.Modulate = new Color(1f, 0.85f, 0.3f);
                rewardRow.AddChild(goldLabel);
            }

            if (data.XpReward > 0)
            {
                var xpLabel = new Label();
                xpLabel.Text = $"⭐ +{data.XpReward}";
                xpLabel.AddThemeFontSizeOverride("font_size", 14);
                xpLabel.Modulate = new Color(0.3f, 0.7f, 1f);
                rewardRow.AddChild(xpLabel);
            }
        }

        return panel;
    }

    private Texture2D CreateStarTexture()
    {
        // 创建简单的星星纹理
        var image = Image.Create(24, 24, false, Image.Format.Rgba8);
        image.Fill(new Color(0, 0, 0, 0));
        
        // 绘制简单的星星形状
        var gold = new Color(1f, 0.85f, 0.3f, 1f);
        int[] starPixels = {
            11, 2, 12, 2,
            7, 5, 8, 5, 15, 5, 16, 5,
            9, 7, 10, 7, 14, 7, 15, 7,
            6, 9, 7, 9, 8, 9, 15, 9, 16, 9, 17, 9,
            4, 11, 5, 11, 8, 11, 9, 11, 14, 11, 15, 11, 18, 11, 19, 11,
            3, 13, 4, 13, 9, 13, 10, 13, 13, 13, 14, 13, 19, 13, 20, 13,
            3, 15, 4, 15, 10, 15, 11, 15, 12, 15, 13, 15, 19, 15, 20, 15,
            4, 17, 5, 17, 11, 17, 12, 17, 18, 17, 19, 17,
            6, 19, 7, 19, 16, 19, 17, 19,
            9, 21, 10, 21, 13, 21, 14, 21,
            11, 22, 12, 22
        };
        
        for (int i = 0; i < starPixels.Length; i += 2)
        {
            if (starPixels[i] < 24 && starPixels[i + 1] < 24)
            {
                image.SetPixel(starPixels[i], starPixels[i + 1], gold);
            }
        }
        
        var texture = ImageTexture.CreateFromImage(image);
        return texture;
    }
}
