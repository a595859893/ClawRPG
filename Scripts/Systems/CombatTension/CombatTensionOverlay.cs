using Godot;
using System;

namespace ClawRPG.Systems.CombatTension;

/// <summary>
/// 战斗紧张度氛围叠加层
/// 在屏幕四边显示颜色渐变表示紧张度
/// </summary>
public partial class CombatTensionOverlay : CanvasLayer
{
    // 边缘色块
    [Export] private ColorRect _topEdge;
    [Export] private ColorRect _bottomEdge;
    [Export] private ColorRect _leftEdge;
    [Export] private ColorRect _rightEdge;

    // 粒子背景
    [Export] private Node2D _particleBackground;

    [Export] private bool _enabled = true;
    [Export] private float _transitionDuration = 0.5f;
    [Export] private float _edgeThickness = 60.0f;

    private Godot.Color _currentColor = new Godot.Color(0.1f, 0.3f, 0.6f, 0.0f);
    private Godot.Color _targetColor = new Godot.Color(0.1f, 0.3f, 0.6f, 0.0f);
    private float _currentParticleSpeed = 20.0f;
    private float _targetParticleSpeed = 20.0f;

    private Tween _colorTween;
    private Tween _particleTween;

    public override void _Ready()
    {
        // 默认透明
        SetEdgesColor(new Godot.Color(0.1f, 0.3f, 0.6f, 0.0f));

        // 订阅 CombatTensionSystem 信号
        var tensionSystem = GetNodeOrNull<Godot.Node>("/root/CombatTensionSystem");
        if (tensionSystem != null)
        {
            CombatTensionSystem.OnTensionValueChanged += _OnTensionValueChanged;
            CombatTensionSystem.OnTensionLevelChanged += _OnTensionLevelChanged;
        }

        // 设置初始粒子速度
        if (_particleBackground != null)
        {
            // Particles2D 的 exploded 速度通过 EmissionORectScale 或 process_material 调整
            // 这里用可见性比例模拟速度变化
            _particleBackground.Visible = false; // 默认隐藏，有粒子才显示
        }

        // 确保边缘尺寸正确
        SetupEdgeSizes();
    }

    private void SetupEdgeSizes()
    {
        var viewportSize = GetViewportRect().Size;

        if (_topEdge != null)
        {
            _topEdge.Size = new Godot.Vector2(viewportSize.x, _edgeThickness);
            _topEdge.Position = new Godot.Vector2(0, 0);
        }
        if (_bottomEdge != null)
        {
            _bottomEdge.Size = new Godot.Vector2(viewportSize.x, _edgeThickness);
            _bottomEdge.Position = new Godot.Vector2(0, viewportSize.y - _edgeThickness);
        }
        if (_leftEdge != null)
        {
            _leftEdge.Size = new Godot.Vector2(_edgeThickness, viewportSize.y);
            _leftEdge.Position = new Godot.Vector2(0, 0);
        }
        if (_rightEdge != null)
        {
            _rightEdge.Size = new Godot.Vector2(_edgeThickness, viewportSize.y);
            _rightEdge.Position = new Godot.Vector2(viewportSize.x - _edgeThickness, 0);
        }
    }

    public override void _Process(double delta)
    {
        // 窗口大小变化时重新设置边缘尺寸
        // 这里用简单的固定值，实际项目可能需要动态调整
    }

    private void _OnTensionValueChanged(float normalizedValue)
    {
        if (!_enabled) return;

        // 计算目标颜色
        TensionLevel level = CombatTensionDatabase.GetTensionLevel(normalizedValue);
        _targetColor = CombatTensionDatabase.GetTensionColor(level);
        _targetParticleSpeed = CombatTensionDatabase.GetParticleSpeed(level);

        // 透明度根据紧张度调整
        _targetColor.a = normalizedValue * 0.7f; // 最大 70% 透明度

        // 使用 Tween 平滑过渡
        TransitionColor(_targetColor);
        TransitionParticleSpeed(_targetParticleSpeed);
    }

    private void _OnTensionLevelChanged(TensionLevel level)
    {
        if (!_enabled) return;

        // 当等级变化时，可能需要触发额外的视觉效果
        // 例如：等级跳跃时短暂闪烁
        if (level == TensionLevel.Enraged)
        {
            FlashEnraged();
        }
    }

    private void TransitionColor(Godot.Color toColor)
    {
        _colorTween?.Kill();

        _colorTween = CreateTween();
        _colorTween.TweenProperty(this, "_currentColor", toColor, _transitionDuration)
            .SetTrans(Tween.TransitionType.EaseInOut);

        // 直接设置颜色（因为 Tween 不能直接动画 Color 属性）
        // 使用替代方案：手动插值
        _colorTween = CreateTween();
        var tweenCallback = new Godot.TweenCallback(this, new Callable(this, nameof(_InterpolateColor)));
        // 由于 Godot 4 的 Tween API 变化，使用颜色插值更简单的方式
        SetEdgesColorAnimated(_currentColor, toColor, _transitionDuration);
    }

    private void SetEdgesColorAnimated(Godot.Color from, Godot.Color to, float duration)
    {
        var tween = CreateTween();
        tween.SetParallel(true);

        float elapsed = 0.0f;
        float step = 0.016f; // ~60fps

        tween.TweenCallback(new Godot.Callable(this, new Callable(this, "QueueFree"))).SetDelay(duration);

        // 使用 Tween 的增量模式
        // Godot 4 方式：通过 CustomAction 或手动更新
        // 这里简化处理，直接设置目标颜色
        SetEdgesColor(toColor);
    }

    private void _InterpolateColor()
    {
        // 颜色插值回调（由 Tween 调用）
    }

    private void SetEdgesColor(Godot.Color color)
    {
        if (_topEdge != null) _topEdge.Color = color;
        if (_bottomEdge != null) _bottomEdge.Color = color;
        if (_leftEdge != null) _leftEdge.Color = color;
        if (_rightEdge != null) _rightEdge.Color = color;
    }

    private void TransitionParticleSpeed(float targetSpeed)
    {
        if (_particleBackground == null) return;

        // 显示粒子背景
        if (!_particleBackground.Visible && targetSpeed > CombatTensionDatabase.ParticlePresets.CalmSpeed)
        {
            _particleBackground.Visible = true;
        }

        // 调整粒子速度（通过缩放粒子数量模拟速度变化）
        // 实际项目中应该通过 process_material 的 initial_velocity 或 similar property
        float speedRatio = targetSpeed / CombatTensionDatabase.ParticlePresets.CalmSpeed;
        _particleBackground.EmitCount = (int)(20 * speedRatio); // 基础 20 个粒子，速度越快粒子越多
    }

    private void FlashEnraged()
    {
        // Enraged 等级时短暂闪烁
        var flashColor = new Godot.Color(0.6f, 0.0f, 0.0f, 0.8f);
        SetEdgesColor(flashColor);

        var tween = CreateTween();
        tween.SetDelay(0.1f);
        tween.TweenCallback(new Godot.Callable(this, new Callable(this, nameof(_OnFlashComplete))));
    }

    private void _OnFlashComplete()
    {
        // 闪烁完成后恢复到当前目标颜色
        SetEdgesColor(_targetColor);
    }

    /// <summary>
    /// 启用/禁用氛围叠加层
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
        if (!enabled)
        {
            SetEdgesColor(new Godot.Color(0, 0, 0, 0));
            if (_particleBackground != null)
                _particleBackground.Visible = false;
        }
    }

    public bool IsEnabled() => _enabled;

    public override void _ExitTree()
    {
        // 取消订阅
        CombatTensionSystem.OnTensionValueChanged -= _OnTensionValueChanged;
        CombatTensionSystem.OnTensionLevelChanged -= _OnTensionLevelChanged;

        _colorTween?.Kill();
        _particleTween?.Kill();

        base._ExitTree();
    }
}
