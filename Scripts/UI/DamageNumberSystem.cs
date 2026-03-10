using Godot;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI;

/// <summary>
/// 浮动伤害数字系统
/// 当实体受到伤害或恢复时显示浮动数字
/// 支持暴击、格挡、闪避、治疗等不同类型
/// 支持2D和3D模式
/// </summary>
public partial class DamageNumberSystem : CanvasLayer
{
    public static DamageNumberSystem Instance { get; private set; }

    [Export] private float _defaultLifetime = 1.2f;
    [Export] private float _critLifetime = 1.8f;
    [Export] private float _defaultSpeed = 80f;
    [Export] private float _critSpeed = 100f;
    [Export] private int _maxSimultaneous = 30;
    [Export] private bool _use3D = false; // 2D游戏设为false

    private List<Label> _activeLabels = new();
    private Dictionary<int, float> _entityLastSpawnTime = new();

    public enum DamageType
    {
        Normal,       // 普通伤害
        Critical,     // 暴击伤害
        Blocked,      // 被格挡
        Dodged,       // 闪避
        Heal,         // 治疗
        ManaRestore,  // 魔法恢复
        TrueDamage,   // 真实伤害
        Miss,         // 未命中
        Poison,       // 中毒
        Fire,         // 火焰
        Ice,          // 冰霜
        Lightning,    // 雷电
    }

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(float delta)
    {
        // 清理过期标签
        for (int i = _activeLabels.Count - 1; i >= 0; i--)
        {
            if (!IsInstanceValid(_activeLabels[i]))
            {
                _activeLabels.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 在2D位置显示伤害数字 (用于2D游戏)
    /// </summary>
    public void ShowDamage2D(Vector2 screenPosition, int amount, DamageType type = DamageType.Normal)
    {
        if (amount <= 0 && type != DamageType.Heal && type != DamageType.ManaRestore) return;
        
        // 限制同时显示的数量
        if (_activeLabels.Count >= _maxSimultaneous)
        {
            var oldest = _activeLabels[0];
            if (IsInstanceValid(oldest))
            {
                oldest.QueueFree();
            }
            _activeLabels.RemoveAt(0);
        }

        // 添加随机偏移避免重叠
        var position = screenPosition + new Vector2(
            GD.Randf() * 40 - 20,
            GD.Randf() * 20 - 10
        );

        ShowDamageAtPosition(position, amount, type);
    }

    /// <summary>
    /// 在屏幕指定位置显示伤害数字 (内部方法)
    /// </summary>
    private void ShowDamageAtPosition(Vector2 screenPos, int amount, DamageType type)
    {
        // 创建标签
        var label = new Label();
        label.Text = FormatNumber(amount, type);
        label.Position = screenPos;
        label.ZIndex = 100;
        
        // 根据类型设置样式
        ApplyStyle(label, type, amount > 0);

        AddChild(label);
        _activeLabels.Add(label);

        // 创建动画
        var tween = CreateTween();
        bool isCrit = type == DamageType.Critical;
        float lifetime = isCrit ? _critLifetime : _defaultLifetime;
        float speed = isCrit ? _critSpeed : _defaultSpeed;
        
        // 向上飘动
        float distance = speed * lifetime;
        tween.TweenProperty(label, "position:y", screenPos.y - distance, lifetime);
        
        // 淡出
        tween.SetParallel(true);
        var endTime = lifetime - 0.3f;
        if (endTime > 0)
        {
            tween.TweenInterval(endTime);
            tween.TweenProperty(label, "modulate:a", 0f, 0.3f);
        }
        
        // 结束时删除
        tween.TweenCallback(Callable.From(() =>
        {
            if (IsInstanceValid(label))
            {
                label.QueueFree();
                _activeLabels.Remove(label);
            }
        }));

        // 暴击时添加缩放动画
        if (isCrit)
        {
            var scaleTween = CreateTween();
            label.Scale = new Vector2(0.5f, 0.5f);
            scaleTween.TweenProperty(label, "scale", new Vector2(1.3f, 1.3f), 0.15f);
            scaleTween.TweenProperty(label, "scale", Vector2.One, 0.15f);
        }
    }

    /// <summary>
    /// 在3D位置显示伤害数字 (用于3D游戏)
    /// </summary>
    public void ShowDamage(Vector3 worldPosition, int amount, DamageType type = DamageType.Normal, bool isPlayerDamage = false)
    {
        if (amount <= 0 && type != DamageType.Heal && type != DamageType.ManaRestore) return;
        
        // 限制同时显示的数量
        if (_activeLabels.Count >= _maxSimultaneous)
        {
            var oldest = _activeLabels[0];
            if (IsInstanceValid(oldest))
            {
                oldest.QueueFree();
            }
            _activeLabels.RemoveAt(0);
        }

        // 转换世界坐标到屏幕坐标
        var camera = GetViewport().GetCamera3D();
        if (camera == null) return;

        var screenPos = camera.UnprojectPosition(worldPosition);
        
        // 添加随机偏移避免重叠
        var randomOffset = new Vector2(
            GD.Randf() * 40 - 20,
            GD.Randf() * 20 - 10
        );
        screenPos += randomOffset;

        // 创建标签
        var label = new Label();
        label.Text = FormatNumber(amount, type);
        label.Position = screenPos;
        label.ZIndex = 100;
        
        // 根据类型设置样式
        ApplyStyle(label, type, amount > 0);

        AddChild(label);
        _activeLabels.Add(label);

        // 创建动画
        var tween = CreateTween();
        bool isCrit = type == DamageType.Critical;
        float lifetime = isCrit ? _critLifetime : _defaultLifetime;
        float speed = isCrit ? _critSpeed : _defaultSpeed;
        
        // 向上飘动
        float distance = speed * lifetime;
        tween.TweenProperty(label, "position:y", screenPos.y - distance, lifetime);
        
        // 淡出
        tween.SetParallel(true);
        var endTime = lifetime - 0.3f;
        if (endTime > 0)
        {
            tween.TweenInterval(endTime);
            tween.TweenProperty(label, "modulate:a", 0f, 0.3f);
        }
        
        // 结束时删除
        tween.TweenCallback(Callable.From(() =>
        {
            if (IsInstanceValid(label))
            {
                label.QueueFree();
                _activeLabels.Remove(label);
            }
        }));

        // 暴击时添加缩放动画
        if (isCrit)
        {
            var scaleTween = CreateTween();
            label.Scale = new Vector2(0.5f, 0.5f);
            scaleTween.TweenProperty(label, "scale", new Vector2(1.3f, 1.3f), 0.15f);
            scaleTween.TweenProperty(label, "scale", Vector2.One, 0.15f);
        }
    }

    /// <summary>
    /// 在实体位置显示伤害数字
    /// </summary>
    public void ShowDamageOnEntity(Node3D entity, int amount, DamageType type = DamageType.Normal, Vector3? overridePosition = null)
    {
        if (entity == null || !IsInstanceValid(entity)) return;

        Vector3 position = overridePosition ?? entity.GlobalPosition;
        // 偏移到头顶
        position += new Vector3(0, entity.GetBoundingBox().Size.y * 0.6f + 0.5f, 0);
        
        ShowDamage(position, amount, type);
    }

    /// <summary>
    /// 在2D实体位置显示伤害数字 (用于2D游戏)
    /// </summary>
    public void ShowDamageOnEntity2D(Node2D entity, int amount, DamageType type = DamageType.Normal, Vector2? overridePosition = null)
    {
        if (entity == null || !IsInstanceValid(entity)) return;

        Vector2 position = overridePosition ?? entity.GlobalPosition;
        // 偏移到头顶
        position += new Vector2(0, -40);

        ShowDamage2D(position, amount, type);
    }

    /// <summary>
    /// 显示文本效果 (2D版本)
    /// </summary>
    public void ShowTextEffect2D(Vector2 screenPosition, string text, DamageType type = DamageType.Dodged)
    {
        if (_activeLabels.Count >= _maxSimultaneous)
        {
            var oldest = _activeLabels[0];
            if (IsInstanceValid(oldest)) oldest.QueueFree();
            _activeLabels.RemoveAt(0);
        }

        var label = new Label();
        label.Text = text;
        label.Position = screenPosition + new Vector2(GD.Randf() * 30 - 15, -20);
        label.ZIndex = 100;

        var fontSize = 20;
        var color = type switch
        {
            DamageType.Dodged => new Color(0.6f, 0.8f, 1f, 1f),
            DamageType.Blocked => new Color(0.8f, 0.7f, 0.4f, 1f),
            DamageType.Miss => new Color(0.7f, 0.7f, 0.7f, 1f),
            _ => new Color(1f, 1f, 1f, 1f)
        };

        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.Modulate = color;
        label.Alignment = HorizontalAlignment.Center;

        AddChild(label);
        _activeLabels.Add(label);

        var tween = CreateTween();
        float lifetime = _defaultLifetime * 0.8f;
        
        tween.TweenProperty(label, "position:y", screenPosition.y - 60, lifetime);
        
        var endTime = lifetime - 0.2f;
        if (endTime > 0)
        {
            tween.TweenInterval(endTime);
            tween.TweenProperty(label, "modulate:a", 0f, 0.2f);
        }
        
        tween.TweenCallback(Callable.From(() =>
        {
            if (IsInstanceValid(label))
            {
                label.QueueFree();
                _activeLabels.Remove(label);
            }
        }));
    }

    /// <summary>
    /// 显示闪避/格挡等文字效果
    /// </summary>
    public void ShowTextEffect(Vector3 worldPosition, string text, DamageType type = DamageType.Dodged)
    {
        var camera = GetViewport().GetCamera3D();
        if (camera == null) return;

        var screenPos = camera.UnprojectPosition(worldPosition);
        screenPos += new Vector2(GD.Randf() * 30 - 15, -20);

        var label = new Label();
        label.Text = text;
        label.Position = screenPos;
        label.ZIndex = 100;

        // 样式
        var fontSize = 20;
        var color = type switch
        {
            DamageType.Dodged => new Color(0.6f, 0.8f, 1f, 1f),
            DamageType.Blocked => new Color(0.8f, 0.7f, 0.4f, 1f),
            DamageType.Miss => new Color(0.7f, 0.7f, 0.7f, 1f),
            _ => new Color(1f, 1f, 1f, 1f)
        };

        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.Modulate = color;
        label.Alignment = HorizontalAlignment.Center;

        AddChild(label);
        _activeLabels.Add(label);

        // 动画
        var tween = CreateTween();
        float lifetime = _defaultLifetime * 0.8f;
        
        tween.TweenProperty(label, "position:y", screenPos.y - 40, lifetime);
        
        var endTime = lifetime - 0.2f;
        if (endTime > 0)
        {
            tween.TweenInterval(endTime);
            tween.TweenProperty(label, "modulate:a", 0f, 0.2f);
        }
        
        tween.TweenCallback(Callable.From(() =>
        {
            if (IsInstanceValid(label))
            {
                label.QueueFree();
                _activeLabels.Remove(label);
            }
        }));
    }

    private string FormatNumber(int amount, DamageType type)
    {
        if (type == DamageType.Heal || type == DamageType.ManaRestore)
        {
            return "+" + amount;
        }
        
        if (type == DamageType.Miss || type == DamageType.Dodged || type == DamageType.Blocked)
        {
            return type.ToString();
        }

        return "-" + amount;
    }

    private void ApplyStyle(Label label, DamageType type, bool isNegative)
    {
        Color color;
        int fontSize;
        bool outline = false;
        Color outlineColor = Colors.Black;

        switch (type)
        {
            case DamageType.Critical:
                color = new Color(1f, 0.3f, 0.3f, 1f); // 红色暴击
                fontSize = 32;
                outline = true;
                outlineColor = new Color(1f, 0.8f, 0f, 1f); // 金色描边
                break;
            case DamageType.Heal:
                color = new Color(0.3f, 1f, 0.5f, 1f); // 绿色治疗
                fontSize = 26;
                break;
            case DamageType.ManaRestore:
                color = new Color(0.4f, 0.6f, 1f, 1f); // 蓝色魔法
                fontSize = 24;
                break;
            case DamageType.Blocked:
                color = new Color(0.9f, 0.8f, 0.4f, 1f); // 金色格挡
                fontSize = 22;
                break;
            case DamageType.Dodged:
                color = new Color(0.6f, 0.8f, 1f, 1f); // 蓝色闪避
                fontSize = 20;
                break;
            case DamageType.Miss:
                color = new Color(0.7f, 0.7f, 0.7f, 1f); // 灰色未命中
                fontSize = 20;
                break;
            case DamageType.TrueDamage:
                color = new Color(1f, 0.4f, 0.8f, 1f); // 粉色真实伤害
                fontSize = 26;
                outline = true;
                break;
            case DamageType.Poison:
                color = new Color(0.5f, 0.9f, 0.3f, 1f); // 绿色中毒
                fontSize = 22;
                break;
            case DamageType.Fire:
                color = new Color(1f, 0.5f, 0.2f, 1f); // 橙色火焰
                fontSize = 24;
                break;
            case DamageType.Ice:
                color = new Color(0.5f, 0.9f, 1f, 1f); // 浅蓝冰霜
                fontSize = 24;
                break;
            case DamageType.Lightning:
                color = new Color(0.9f, 0.9f, 0.4f, 1f); // 黄色雷电
                fontSize = 24;
                break;
            default:
                color = isNegative ? new Color(1f, 1f, 1f, 1f) : new Color(0.8f, 0.8f, 0.8f, 1f);
                fontSize = 24;
                break;
        }

        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.Modulate = color;
        label.Alignment = HorizontalAlignment.Center;

        // 添加阴影效果
        if (outline)
        {
            label.AddThemeColorOverride("font_shadow_color", outlineColor);
            label.AddThemeConstantOverride("font_shadow_offset_x", 2);
            label.AddThemeConstantOverride("font_shadow_offset_y", 2);
        }
    }

    /// <summary>
    /// 清除所有浮动数字
    /// </summary>
    public void ClearAll()
    {
        foreach (var label in _activeLabels)
        {
            if (IsInstanceValid(label))
            {
                label.QueueFree();
            }
        }
        _activeLabels.Clear();
    }
}
