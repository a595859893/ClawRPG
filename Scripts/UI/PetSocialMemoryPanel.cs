using Godot;
using System;

namespace ClawRPG.Scripts.UI
{
/// <summary>
/// 宠物社交记忆面板 - 显示各宠物"上次一起战斗"时间
/// REQ-178: 数据已存在于 PetSocialMemoryDatabase，消费端 UI 缺失
/// </summary>
public partial class PetSocialMemoryPanel : PanelContainer
{
    // 7天阈值（毫秒）
    private const long SEVEN_DAYS_MS = 7L * 24 * 60 * 60 * 1000;

    private VBoxContainer _content;
    private Label _titleLabel;
    private Label _noPetsLabel;

    public override void _Ready()
    {
        // 面板样式
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.06f, 0.04f, 0.12f, 0.88f);
        style.BorderColorLeft = new Color(0.45f, 0.3f, 0.7f, 0.6f);
        style.BorderColorRight = new Color(0.45f, 0.3f, 0.7f, 0.6f);
        style.BorderColorTop = new Color(0.45f, 0.3f, 0.7f, 0.6f);
        style.BorderColorBottom = new Color(0.45f, 0.3f, 0.7f, 0.6f);
        style.BorderWidthLeft = 1;
        style.BorderWidthRight = 1;
        style.BorderWidthTop = 1;
        style.BorderWidthBottom = 1;
        style.CornerRadiusTopLeft = 6;
        style.CornerRadiusTopRight = 6;
        style.CornerRadiusBottomLeft = 6;
        style.CornerRadiusBottomRight = 6;
        style.ContentMarginLeft = 12;
        style.ContentMarginTop = 10;
        style.ContentMarginRight = 12;
        style.ContentMarginBottom = 10;
        AddThemeStyleboxOverride("panel", style);

        var scroll = new ScrollContainer
        {
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled
        };
        AddChild(scroll);

        _content = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(0, 0)
        };
        scroll.AddChild(_content);

        _titleLabel = new Label
        {
            Text = "🐾 社交记忆",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        _titleLabel.AddThemeFontSizeOverride("font_size", 14);
        _titleLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.65f, 1.0f, 1.0f));
        _content.AddChild(_titleLabel);

        _noPetsLabel = new Label
        {
            Text = "暂无同伴记录",
            HorizontalAlignment = HorizontalAlignment.Left,
            Modulate = new Color(0.6f, 0.6f, 0.65f, 0.8f)
        };
        _noPetsLabel.AddThemeFontSizeOverride("font_size", 12);
        _content.AddChild(_noPetsLabel);

        // 订阅 PetFriendshipSystem 重逢信号（战斗结束后触发刷新）
        if (PetFriendshipSystem.Instance != null)
        {
            PetFriendshipSystem.Instance.PetReunion += _ => RefreshDisplay();
        }

        Visible = false;
    }

    /// <summary>
    /// 显示社交记忆面板（由 PetCombatCompanionUI 调用）
    /// </summary>
    public void ShowPanel()
    {
        RefreshDisplay();
        Visible = true;

        // 淡入动画
        var tween = CreateTween();
        Modulate = new Color(1, 1, 1, 0);
        tween.TweenProperty(this, "modulate:a", 1.0f, 0.3f);
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    public void HidePanel()
    {
        Visible = false;
    }

    /// <summary>
    /// 刷新显示（每次战斗结束/社交互动后调用）
    /// </summary>
    public void RefreshDisplay()
    {
        if (!IsInstanceValid(_content)) return;

        // 清除旧内容（保留标题和noPetsLabel）
        foreach (var child in _content.GetChildren())
        {
            if (child == _titleLabel || child == _noPetsLabel) continue;
            child.QueueFree();
        }

        var petIds = GetActivePetIds();
        if (petIds.Count == 0)
        {
            _noPetsLabel.Visible = true;
            return;
        }

        _noPetsLabel.Visible = false;

        foreach (var petId in petIds)
        {
            var entry = CreatePetMemoryEntry(petId);
            _content.AddChild(entry);
        }
    }

    private System.Collections.Generic.List<int> GetActivePetIds()
    {
        var ids = new System.Collections.Generic.List<int>();

        // 从 PetCombatCompanionSystem 获取当前宠物列表
        var companion = GetNodeOrNull("/root/PetCombatCompanionSystem");
        if (companion == null) return ids;

        // 反射获取宠物列表（避免强类型依赖）
        var getPetsMethod = companion.GetType().GetMethod("GetActivePetIds");
        if (getPetsMethod != null)
        {
            var result = getPetsMethod.Invoke(companion, null);
            if (result is System.Collections.Generic.List<int> list)
                return list;
            if (result is int[] arr)
            {
                foreach (var id in arr) ids.Add(id);
                return ids;
            }
        }

        return ids;
    }

    private Control CreatePetMemoryEntry(int petId)
    {
        var hbox = new HBoxContainer();
        hbox.CustomMinimumSize = new Vector2(0, 28);

        // 宠物名标签
        var nameLabel = new Label
        {
            Text = $"宠物 #{petId}",
            HorizontalAlignment = HorizontalAlignment.Left,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        nameLabel.AddThemeFontSizeOverride("font_size", 12);
        nameLabel.AddThemeColorOverride("font_color", new Color(0.88f, 0.88f, 0.95f, 1.0f));
        hbox.AddChild(nameLabel);

        // 时间标签
        var timeLabel = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            SizeFlagsHorizontal = SizeFlags.ShrinkEnd
        };
        timeLabel.AddThemeFontSizeOverride("font_size", 12);

        // 获取最近战斗时间
        var lastBattle = PetSocialMemoryDatabase.Instance.GetLastBattleTimeForPet(petId);

        if (lastBattle == null)
        {
            // 从未一起战斗
            timeLabel.Text = "——";
            timeLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.55f, 0.7f));
        }
        else
        {
            var elapsed = DateTime.Now - lastBattle.Value;
            var (text, color, isLong) = FormatElapsedTime(elapsed);

            timeLabel.Text = text;
            timeLabel.AddThemeColorOverride("font_color", color);

            // 超7天显示想念标记
            if (isLong)
            {
                var sadLabel = new Label
                {
                    Text = " 想念你了 🥺",
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                sadLabel.AddThemeFontSizeOverride("font_size", 11);
                sadLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.55f, 0.55f, 0.9f));
                hbox.AddChild(sadLabel);
            }
        }

        hbox.AddChild(timeLabel);
        return hbox;
    }

    /// <summary>
    /// 将时间差转为自然语言
    /// 返回 (显示文本, 文字颜色, 是否超过7天)
    /// </summary>
    private (string text, Color color, bool isLong) FormatElapsedTime(TimeSpan elapsed)
    {
        if (elapsed.TotalMilliseconds < 0)
            elapsed = TimeSpan.Zero;

        if (elapsed.TotalDays < 1)
        {
            // 今天
            return ("今天", new Color(0.6f, 0.85f, 0.6f, 1.0f), false);
        }
        else if (elapsed.TotalDays < 7)
        {
            // X天前
            int days = (int)elapsed.TotalDays;
            var color = days <= 2
                ? new Color(0.7f, 0.85f, 0.7f, 1.0f)
                : new Color(0.85f, 0.75f, 0.55f, 1.0f);
            return ($"{days}天前", color, false);
        }
        else
        {
            // 7天前+
            return ("7天前+", new Color(0.95f, 0.5f, 0.5f, 1.0f), true);
        }
    }

    public override void _ExitTree()
    {
        if (PetFriendshipSystem.Instance != null)
        {
            PetFriendshipSystem.Instance.PetReunion -= _ => RefreshDisplay();
        }
    }
}
}
