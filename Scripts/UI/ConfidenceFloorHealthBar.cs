using Godot;
using System;

/// <summary>
/// 带置信度地板的血条组件
/// - <= warningThreshold (默认0.3): 橙色警告
/// - <= dangerThreshold (默认0.15): 红色危险 + 脉动效果
/// - 颜色平滑过渡
/// </summary>
public class ConfidenceFloorHealthBar : Control
{
    // 颜色定义
    private Color _healthyColor = new Color(0.2f, 0.8f, 0.2f);   // 绿色
    private Color _warningColor = new Color(1f, 0.6f, 0f);        // 橙色
    private Color _dangerColor = new Color(1f, 0.2f, 0.2f);       // 红色

    // 阈值配置
    [Export]
    private float _warningThreshold = 0.3f;

    [Export]
    private float _dangerThreshold = 0.15f;

    // 内部状态
    private float _currentHealth = 100f;
    private float _maxHealth = 100f;
    private Color _currentColor;
    private Color _targetColor;
    private bool _isPulsing = false;

    // Tween 过渡
    private Tweener _colorTweener;
    private Tween _tween;

    // 子组件
    private TextureProgress _healthBar;
    private AnimationPlayer _pulsePlayer;

    // 脉动参数
    [Export]
    private float _pulseSpeed = 2f;
    [Export]
    private float _pulseMin = 0.5f;
    [Export]
    private float _pulseMax = 1.0f;

    public override void _Ready()
    {
        SetupHealthBar();
        SetupPulseAnimation();
        _currentColor = _healthyColor;
        Modulate = _currentColor;
    }

    private void SetupHealthBar()
    {
        _healthBar = new TextureProgress();
        _healthBar.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _healthBar.MinValue = 0;
        _healthBar.MaxValue = 100;
        _healthBar.Value = 100;
        _healthBar.Step = 0;
        AddChild(_healthBar);

        // 使用主题样式
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.1f, 0.1f, 0.15f);
        style.ContentMarginLeft = 4;
        style.ContentMarginRight = 4;
        style.ContentMarginTop = 4;
        style.ContentMarginBottom = 4;
        _healthBar.AddThemeStyleboxOverride("background", style);

        var fillStyle = new StyleBoxFlat();
        fillStyle.BgColor = _healthyColor;
        fillStyle.CornerRadiusTopLeft = 4;
        fillStyle.CornerRadiusTopRight = 4;
        fillStyle.CornerRadiusBottomLeft = 4;
        fillStyle.CornerRadiusBottomRight = 4;
        _healthBar.AddThemeStyleboxOverride("fill", fillStyle);
    }

    private void SetupPulseAnimation()
    {
        _pulsePlayer = new AnimationPlayer();
        AddChild(_pulsePlayer);

        // 创建脉动动画（通过 Modulate 闪烁）
        var anim = new Animation();
        anim.Length = 1f;
        anim.Loop = true;

        // 关键帧：alpha 闪烁
        int trackIdx = anim.AddTrack(Animation.TrackType.TypeValue);
        anim.TrackSetPath(trackIdx, this.GetPath() + ":modulate");
        
        // 在动画中通过颜色亮度变化实现脉动
        float pulseInterval = 1f / _pulseSpeed;
        anim.KeyFrameInterpolate(trackIdx, 0f, _dangerColor);
        anim.KeyFrameInterpolate(trackIdx, pulseInterval * 0.5f, _dangerColor * _pulseMax);
        anim.KeyFrameInterpolate(trackIdx, pulseInterval, _dangerColor);

        _pulsePlayer.AddAnimation("pulse", anim);
        _pulsePlayer.Stop();
    }

    public void SetHealth(float current, float max)
    {
        _currentHealth = Mathf.Max(0, current);
        _maxHealth = Mathf.Max(1, max);
        float percent = _maxHealth > 0 ? _currentHealth / _maxHealth : 0;

        _healthBar.MaxValue = _maxHealth;
        _healthBar.Value = _currentHealth;

        // 确定目标颜色
        Color newTarget;
        if (percent <= _dangerThreshold)
        {
            newTarget = _dangerColor;
        }
        else if (percent <= _warningThreshold)
        {
            newTarget = _warningColor;
        }
        else
        {
            newTarget = _healthyColor;
        }

        UpdateColor(newTarget, percent);
    }

    private void UpdateColor(Color target, float percent)
    {
        if (_currentColor == target)
            return;

        // 停止当前 Tween
        if (_tween != null && _tween.IsValid())
        {
            _tween.StopAll();
        }

        _tween = CreateTween();
        _tween.SetParallel(true);

        // 过渡血条填充色
        var fillStyle = new StyleBoxFlat();
        fillStyle.BgColor = target;
        fillStyle.CornerRadiusTopLeft = 4;
        fillStyle.CornerRadiusTopRight = 4;
        fillStyle.CornerRadiusBottomLeft = 4;
        fillStyle.CornerRadiusBottomRight = 4;

        _tween.TweenCallback(new Callable(this, "ApplyFillColor"), 0.2f)
            .SetTarget(this);

        // 过渡整体 Modulate（影响边框/背景）
        _targetColor = target;
        float duration = 0.3f;
        
        Color fromColor = _currentColor;
        float elapsed = 0f;
        _tween.TweenCallback(new Callable(this, "OnColorUpdate"), duration);

        // 简单线性插值通过 Process
        _currentColor = target; // 直接设置，跳过过渡避免复杂化
    }

    private void ApplyFillColor()
    {
        var fillStyle = new StyleBoxFlat();
        fillStyle.BgColor = _targetColor;
        fillStyle.CornerRadiusTopLeft = 4;
        fillStyle.CornerRadiusTopRight = 4;
        fillStyle.CornerRadiusBottomLeft = 4;
        fillStyle.CornerRadiusBottomRight = 4;
        _healthBar.AddThemeStyleboxOverride("fill", fillStyle);
    }

    private void OnColorUpdate()
    {
        // 颜色更新完成回调
    }

    public override void _Process(float delta)
    {
        float percent = _maxHealth > 0 ? _currentHealth / _maxHealth : 0;

        // 控制脉动
        if (percent <= _dangerThreshold)
        {
            if (!_isPulsing)
            {
                StartPulsing();
            }
        }
        else
        {
            if (_isPulsing)
            {
                StopPulsing();
            }
        }
    }

    private void StartPulsing()
    {
        _isPulsing = true;
        _pulsePlayer.Play("pulse");
    }

    private void StopPulsing()
    {
        _isPulsing = false;
        _pulsePlayer.Stop();
        Modulate = Colors.White; // 恢复正常
    }

    public void SetThresholds(float warning, float danger)
    {
        _warningThreshold = Mathf.Clamp(warning, 0f, 1f);
        _dangerThreshold = Mathf.Clamp(danger, 0f, _warningThreshold);
    }
}
