using Godot;
using System;

// TODO: Uncomment when BaseSystem is confirmed to exist
// using ClawRPG.Framework;

namespace ClawRPG.Systems.CombatTension;

/// <summary>
/// 战斗紧张度核心系统
/// 管理战斗场景的氛围梯度：combo 连续数、宠物血量、Boss 蓄力状态
/// </summary>
public partial class CombatTensionSystem : Godot.Node//, BaseSystem
{
    // TODO: Uncomment when BaseSystem is available
    // public static CombatTensionSystem Instance { get; private set; }

    // [Signal]
    // public delegate void OnTensionValueChangedEventHandler(float normalizedValue);
    // [Signal]
    // public delegate void OnTensionLevelChangedEventHandler(TensionLevel level);

    // Temp signal definitions (will merge with BaseSystem signals when available)
    private static event Action<float> OnTensionValueChanged;
    private static event Action<TensionLevel> OnTensionLevelChanged;

    [Export] private bool _enabled = true;
    [Export] private float _transitionDuration = 0.5f;

    private TensionParams _params;
    private TensionState _state;

    // 战斗状态
    private int _currentComboCount = 0;
    private float _petHpRatio = 1.0f;   // 0.0~1.0，越低越紧张
    private float _bossChargeLevel = 0.0f; // 0.0~1.0
    private bool _inCombat = false;

    // Tween 引用
    private Tween _colorTween;
    private Tween _particleTween;

    public override void _Ready()
    {
        // TODO: Uncomment when BaseSystem is available
        // Initialize singleton
        // Instance = this;

        _params = CombatTensionDatabase.DefaultParams;
        _state = new TensionState
        {
            CurrentLevel = TensionLevel.Calm,
            NormalizedValue = 0.0f,
            Transitioning = false
        };

        SubscribeSignals();
    }

    private void SubscribeSignals()
    {
        // 订阅 Combo 系统信号
        // Note: 这些信号需要确认存在，使用 HasSignal 检查
        var comboSystem = GetNodeOrNull<Godot.Node>("/root/SkillComboSystem");
        if (comboSystem != null)
        {
            if (comboSystem.HasSignal("ComboStarted"))
                comboSystem.Connect("ComboStarted", new Godot.Callable(this, "_OnComboStarted"));
            if (comboSystem.HasSignal("ComboCompleted"))
                comboSystem.Connect("ComboCompleted", new Godot.Callable(this, "_OnComboCompleted"));
            if (comboSystem.HasSignal("ComboFailed"))
                comboSystem.Connect("ComboFailed", new Godot.Callable(this, "_OnComboFailed"));
        }

        // 订阅宠物伤害信号
        var petSystem = GetNodeOrNull<Godot.Node>("/root/PetCombatCompanionSystem");
        if (petSystem != null)
        {
            if (petSystem.HasSignal("PetDamaged"))
                petSystem.Connect("PetDamaged", new Godot.Callable(this, "_OnPetDamaged"));
        }

        // 订阅 Boss 蓄力信号
        var bossSystem = GetNodeOrNull<Godot.Node>("/root/BossMechanicsSystem");
        if (bossSystem != null)
        {
            if (bossSystem.HasSignal("BossCharging"))
                bossSystem.Connect("BossCharging", new Godot.Callable(this, "_OnBossCharging"));
            if (bossSystem.HasSignal("BossChargeFinished"))
                bossSystem.Connect("BossChargeFinished", new Godot.Callable(this, "_OnBossChargeFinished"));
        }

        // 订阅战斗开始/结束信号
        var combatSystem = GetNodeOrNull<Godot.Node>("/root/CombatSystem");
        if (combatSystem != null)
        {
            if (combatSystem.HasSignal("CombatStarted"))
                combatSystem.Connect("CombatStarted", new Godot.Callable(this, "_OnCombatStarted"));
            if (combatSystem.HasSignal("CombatEnded"))
                combatSystem.Connect("CombatEnded", new Godot.Callable(this, "_OnCombatEnded"));
        }
    }

    private void _OnComboStarted()
    {
        if (!_enabled || !_inCombat) return;
        _currentComboCount++;
        EvaluateAndBroadcast();
    }

    private void _OnComboCompleted()
    {
        if (!_enabled || !_inCombat) return;
        // combo 完成后短暂保持计数，然后重置
        var timer = new Godot.Timer { OneShot = true, WaitTime = 3.0f };
        AddChild(timer);
        timer.Timeout += () =>
        {
            _currentComboCount = 0;
            EvaluateAndBroadcast();
            timer.QueueFree();
        };
        timer.Start();
    }

    private void _OnComboFailed()
    {
        if (!_enabled || !_inCombat) return;
        _currentComboCount = 0;
        EvaluateAndBroadcast();
    }

    private void _OnPetDamaged(int petId, float damage, float newHp, float maxHp)
    {
        if (!_enabled || !_inCombat) return;
        _petHpRatio = (maxHp > 0) ? newHp / maxHp : 1.0f;
        EvaluateAndBroadcast();
    }

    private void _OnBossCharging(float chargePercent)
    {
        if (!_enabled || !_inCombat) return;
        _bossChargeLevel = Mathf.Clamp(chargePercent, 0.0f, 1.0f);
        EvaluateAndBroadcast();
    }

    private void _OnBossChargeFinished()
    {
        if (!_enabled) return;
        _bossChargeLevel = 0.0f;
        EvaluateAndBroadcast();
    }

    private void _OnCombatStarted()
    {
        if (!_enabled) return;
        _inCombat = true;
        _currentComboCount = 0;
        _petHpRatio = 1.0f;
        _bossChargeLevel = 0.0f;
        EvaluateAndBroadcast();
    }

    private void _OnCombatEnded()
    {
        if (!_enabled) return;
        _inCombat = false;
        // 重置到 Calm
        float oldValue = _state.NormalizedValue;
        _state.NormalizedValue = 0.0f;
        _state.CurrentLevel = TensionLevel.Calm;
        _state.Transitioning = false;

        if (Mathf.Abs(oldValue - 0.0f) > 0.01f)
        {
            OnTensionValueChanged?.Invoke(0.0f);
            OnTensionLevelChanged?.Invoke(TensionLevel.Calm);
        }
    }

    /// <summary>
    /// 评估当前紧张度并广播变化
    /// </summary>
    private void EvaluateAndBroadcast()
    {
        float oldValue = _state.NormalizedValue;
        TensionLevel oldLevel = _state.CurrentLevel;

        float comboFactor = EvaluateComboFactor();
        float petHpFactor = EvaluatePetHpFactor();
        float bossFactor = EvaluateBossChargeFactor();

        float newValue = (_params.ComboWeight * comboFactor) +
                        (_params.PetHpWeight * petHpFactor) +
                        (_params.BossChargeWeight * bossFactor);

        newValue = Mathf.Clamp(newValue, 0.0f, 1.0f);
        _state.NormalizedValue = newValue;
        _state.CurrentLevel = CombatTensionDatabase.GetTensionLevel(newValue);

        // 广播变化（仅当值有显著变化时）
        if (Mathf.Abs(newValue - oldValue) > 0.01f)
        {
            OnTensionValueChanged?.Invoke(newValue);
        }

        if (_state.CurrentLevel != oldLevel)
        {
            OnTensionLevelChanged?.Invoke(_state.CurrentLevel);
        }
    }

    private float EvaluateComboFactor()
    {
        if (_currentComboCount <= 0) return 0.0f;
        if (_currentComboCount >= _params.ComboCriticalThreshold) return 1.0f;
        if (_currentComboCount >= _params.ComboIntenseThreshold) return 0.7f;
        if (_currentComboCount >= _params.ComboRisingThreshold) return 0.4f;
        return 0.2f;
    }

    private float EvaluatePetHpFactor()
    {
        // HP 越低越紧张：1.0 = 满血 = 0.0 紧张度
        // 0.0 = 空血 = 1.0 紧张度
        return 1.0f - _petHpRatio;
    }

    private float EvaluateBossChargeFactor()
    {
        return _bossChargeLevel;
    }

    /// <summary>
    /// 获取当前紧张度值 (0.0~1.0)
    /// </summary>
    public float GetTensionValue() => _state.NormalizedValue;

    /// <summary>
    /// 获取当前紧张度等级
    /// </summary>
    public TensionLevel GetTensionLevel() => _state.CurrentLevel;

    /// <summary>
    /// 获取目标紧张度颜色
    /// </summary>
    public Godot.Color GetTensionColor() => CombatTensionDatabase.GetTensionColor(_state.CurrentLevel);

    /// <summary>
    /// 获取粒子速度倍率
    /// </summary>
    public float GetParticleSpeedMultiplier() => CombatTensionDatabase.GetParticleSpeed(_state.CurrentLevel);

    /// <summary>
    /// 获取 BGM 层索引
    /// </summary>
    public int GetBgmLayer() => CombatTensionDatabase.GetBgmLayer(_state.CurrentLevel);

    /// <summary>
    /// 启用/禁用紧张度系统
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
        if (!enabled)
        {
            // 禁用时立即重置
            _state.NormalizedValue = 0.0f;
            _state.CurrentLevel = TensionLevel.Calm;
            OnTensionValueChanged?.Invoke(0.0f);
            OnTensionLevelChanged?.Invoke(TensionLevel.Calm);
        }
    }

    public bool IsEnabled() => _enabled;

    // ========== 持久化 ==========

    // /// <summary>
    // /// 导出保存数据 (BaseSystem 要求)
    // /// </summary>
    // public override Godot.Collections.Dictionary ExportSaveData()
    // {
    //     return new Godot.Collections.Dictionary
    //     {
    //         ["enabled"] = _enabled,
    //         ["currentComboCount"] = _currentComboCount,
    //         ["petHpRatio"] = _petHpRatio,
    //         ["bossChargeLevel"] = _bossChargeLevel
    //     };
    // }
    //
    // /// <summary>
    // /// 导入保存数据 (BaseSystem 要求)
    // /// </summary>
    // public override void ImportSaveData(Godot.Collections.Dictionary data)
    // {
    //     if (data == null) return;
    //     if (data.ContainsKey("enabled")) _enabled = (bool)data["enabled"];
    //     if (data.ContainsKey("currentComboCount")) _currentComboCount = (int)data["currentComboCount"];
    //     if (data.ContainsKey("petHpRatio")) _petHpRatio = (float)data["petHpRatio"];
    //     if (data.ContainsKey("bossChargeLevel")) _bossChargeLevel = (float)data["bossChargeLevel"];
    // }
}
