using Godot;
using System;

/// <summary>
/// 动画效果管理器 - 负责玩家攻击动画、特效和屏幕反馈
/// 集成 AnimationPlayer 概念用于攻击序列控制
/// </summary>
public partial class AnimationEffectManager : BaseSystem
{
    public static AnimationEffectManager Instance { get; private set; }
    
    // 动画状态
    private enum AnimationState { Idle, Attacking, Dodging, Casting, Hit }
    private AnimationState _currentState = AnimationState.Idle;
    
    // 攻击动画序列
    private float _attackAnimationTime = 0f;
    private float _attackAnimationDuration = 0.3f;
    private Vector2 _attackOrigin = Vector2.Zero;
    private bool _isPlayingAttackAnimation = false; 
    
    // 打击效果
    private float _hitStopTime = 0f;
    private float _screenShakeIntensity = 0f;
    private Vector2 _screenShakeOffset = Vector2.Zero;
    private double _shakeDuration = 0.0;
    private double _shakeTimer = 0.0;
    
    // 攻击拖尾
    private bool _enableAttackTrail = false; 
    private Color _attackTrailColor = new Color(1f, 0.8f, 0.3f, 0.6f);
    private float _trailWidth = 20f;
    
    // 慢动作效果
    private bool _isSlowMotion = false; 
    private float _slowMotionScale = 1.0f;
    private double _slowMotionDuration = 0.0;
    private double _slowMotionTimer = 0.0;
    
    // 节点引用
    private Node2D _effectContainer;
    
    public override void _Ready()
    {
        Instance = this;
        SetupEffectContainer();
    }
    
    /// <summary>
    /// 系统名称
    /// </summary>
    protected override string SystemName => "AnimationEffect";
    
    private void SetupEffectContainer()
    {
        _effectContainer = new Node2D();
        _effectContainer.Name = "AnimationEffectContainer";
        GetTree().CurrentScene.AddChild(_effectContainer);
    }
    
    public override void _Process(double delta)
    {
        // 更新攻击动画
        if (_isPlayingAttackAnimation)
        {
            _attackAnimationTime += (float)delta;
            
            // 攻击回弹效果
            float progress = _attackAnimationTime / _attackAnimationDuration;
            if (progress < 0.3f)
            {
                // 向前冲
                float t = progress / 0.3f;
                float scale = 1f + 0.1f * Mathf.Sin(t * Mathf.Pi);
                // 效果可以在此处应用到玩家节点
            }
            else if (progress < 1.0f)
            {
                // 回弹
                float t = (progress - 0.3f) / 0.7f;
                float scale = 1f + 0.1f * Mathf.Sin((1f - t) * Mathf.Pi);
            }
            
            if (_attackAnimationTime >= _attackAnimationDuration)
            {
                _isPlayingAttackAnimation = false; 
                _currentState = AnimationState.Idle;
            }
        }
        
        // 更新屏幕震动
        if (_shakeTimer > 0)
        {
            _shakeTimer -= delta;
            if (_shakeTimer <= 0)
            {
                _screenShakeOffset = Vector2.Zero;
            }
            else
            {
                float intensity = _screenShakeIntensity * (float)(_shakeTimer / _shakeDuration);
                _screenShakeOffset = new Vector2(
                    (float)GD.Randf() * 2f - 1f,
                    (float)GD.Randf() * 2f - 1f
                ) * intensity;
            }
        }
        
        // 更新慢动作
        if (_isSlowMotion)
        {
            _slowMotionTimer -= delta;
            if (_slowMotionTimer <= 0)
            {
                _isSlowMotion = false; 
                Engine.TimeScale = 1.0f;
            }
        }
        
        // 更新打击停顿
        if (_hitStopTime > 0)
        {
            _hitStopTime -= (float)delta;
        }
    }
    
    /// <summary>
    /// 播放攻击动画序列
    /// </summary>
    public void PlayAttackAnimation(Vector2 attackPosition, float duration = 0.3f)
    {
        if (_currentState == AnimationState.Attacking) return;
        
        _currentState = AnimationState.Attacking;
        _attackOrigin = attackPosition;
        _attackAnimationTime = 0f;
        _attackAnimationDuration = duration;
        _isPlayingAttackAnimation = true;
        
        // 播放攻击音效
        SoundEffectSystem.Instance?.PlayAttackSound();
    }
    
    /// <summary>
    /// 播放打击特效
    /// </summary>
    public void PlayHitEffect(Vector2 position, bool isCritical, float intensity = 1.0f)
    {
        // 屏幕震动
        float shakeAmount = isCritical ? 8f * intensity : 4f * intensity;
        TriggerScreenShake(shakeAmount, 0.15);
        
        // 慢动作 (暴击时)
        if (isCritical)
        {
            TriggerSlowMotion(0.2f, 0.15);
        }
        
        // 打击停顿 (Hit Stop)
        _hitStopTime = isCritical ? 0.08f : 0.04f;
        
        // 粒子效果 (如果存在粒子系统)
        // 可以在这里添加粒子发射
        
        GD.Print($"Hit effect at {position}, Critical: {isCritical}");
    }
    
    /// <summary>
    /// 触发屏幕震动
    /// </summary>
    public void TriggerScreenShake(float intensity, double duration)
    {
        _screenShakeIntensity = intensity;
        _shakeDuration = duration;
        _shakeTimer = duration;
    }
    
    /// <summary>
    /// 触发慢动作效果
    /// </summary>
    public void TriggerSlowMotion(float timeScale, double duration)
    {
        _slowMotionScale = timeScale;
        _slowMotionDuration = duration;
        _slowMotionTimer = duration;
        _isSlowMotion = true;
        Engine.TimeScale = timeScale;
    }
    
    /// <summary>
    /// 触发打击停顿 (Hit Stop) - 暂时停止游戏时间
    /// </summary>
    public void TriggerHitStop(float duration)
    {
        _hitStopTime = duration;
    }
    
    /// <summary>
    /// 播放闪避动画
    /// </summary>
    public void PlayDodgeAnimation(Vector2 direction, float duration = 0.25f)
    {
        if (_currentState == AnimationState.Dodging) return;
        
        _currentState = AnimationState.Dodging;
        
        // 闪避时的屏幕效果
        TriggerScreenShake(2f, 0.1);
        
        // 延迟恢复状态
        GetTree().CreateTimer(duration).Timeout += () => 
        {
            if (_currentState == AnimationState.Dodging)
                _currentState = AnimationState.Idle;
        };
    }
    
    /// <summary>
    /// 播放受击动画
    /// </summary>
    public void PlayHitAnimation(Vector2 fromDirection)
    {
        if (_currentState == AnimationState.Hit) return;
        
        _currentState = AnimationState.Hit;
        
        // 受击时强烈震动
        TriggerScreenShake(10f, 0.2);
        
        // 短暂慢动作
        TriggerSlowMotion(0.3f, 0.1);
        
        // 延迟恢复
        GetTree().CreateTimer(0.2).Timeout += () =>
        {
            if (_currentState == AnimationState.Hit)
                _currentState = AnimationState.Idle;
        };
    }
    
    /// <summary>
    /// 播放技能释放动画
    /// </summary>
    public void PlaySkillAnimation(Vector2 targetPosition, string skillName, float duration = 0.5f)
    {
        _currentState = AnimationState.Casting;
        
        // 技能释放屏幕效果
        TriggerScreenShake(3f, duration * 0.5);
        
        GetTree().CreateTimer(duration).Timeout += () =>
        {
            if (_currentState == AnimationState.Casting)
                _currentState = AnimationState.Idle;
        };
    }
    
    /// <summary>
    /// 获取当前屏幕震动偏移
    /// </summary>
    public Vector2 GetScreenShakeOffset() => _screenShakeOffset;
    
    /// <summary>
    /// 获取当前状态
    /// </summary>
    public AnimationState GetCurrentState() => _currentState;
    
    /// <summary>
    /// 是否处于打击停顿中
    /// </summary>
    public bool IsInHitStop() => _hitStopTime > 0;
}
