using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Combo 遗忘等级（可视化层）
/// </summary>
public enum ComboForgetLevel
{
    /// <summary>熟练 — 近期使用过</summary>
    Proficient,
    /// <summary>生疏 — 超过 DORMANT_AFTER_GAMES 但未永久遗忘</summary>
    Rusty,
    /// <summary>遗忘 — 已永久遗忘（休眠）或长期未使用</summary>
    Forgotten
}

/// <summary>
/// Combo 遗忘可视化 UI — 集成到 SkillComboUI 的遗忘等级显示
/// 订阅 ComboForgetSystem 信号，驱动 SkillComboUI 面板的遗忘 UI 更新
/// </summary>
public partial class ComboForgetUI : Node
{
    public static ComboForgetUI Instance { get; private set; }

    // SkillComboUI reference (延迟解析，避免循环依赖)
    private SkillComboUI _comboUI;

    public override void _Ready()
    {
        Instance = this;
        // 订阅遗忘状态变化信号
        Framework.ComboForgetData.ComboForgetStateChanged += OnForgetStateChanged;
        Framework.ComboForgetData.ComboRediscovered += OnRediscovered;
        Framework.ComboForgetData.ComboLocked += OnLocked;
        Framework.ComboForgetData.ComboUnlocked += OnUnlocked;
    }

    public override void _ExitTree()
    {
        Framework.ComboForgetData.ComboForgetStateChanged -= OnForgetStateChanged;
        Framework.ComboForgetData.ComboRediscovered -= OnRediscovered;
        Framework.ComboForgetData.ComboLocked -= OnLocked;
        Framework.ComboForgetData.ComboUnlocked -= OnUnlocked;
    }

    /// <summary>
    /// 获取某个 combo 的遗忘等级
    /// </summary>
    public ComboForgetLevel GetForgetLevel(string comboId)
    {
        if (Framework.ComboForgetSystem.Instance == null)
            return ComboForgetLevel.Proficient;

        var (games, isLocked, isDormant, totalUse) = Framework.ComboForgetSystem.Instance.GetForgetInfo(comboId);

        if (isDormant || games >= Framework.ComboForgetData.DORMANT_AFTER_GAMES + 2)
            return ComboForgetLevel.Forgotten;

        if (games >= Framework.ComboForgetData.DORMANT_AFTER_GAMES)
            return ComboForgetLevel.Rusty;

        return ComboForgetLevel.Proficient;
    }

    /// <summary>
    /// 获取某个 combo 的有效成功率（基础 - 遗忘惩罚）
    /// </summary>
    public float GetEffectiveSuccessRate(string comboId)
    {
        var level = GetForgetLevel(comboId);
        return level switch
        {
            ComboForgetLevel.Proficient => 1.0f,
            ComboForgetLevel.Rusty => 0.85f,    // -15% 生疏惩罚
            ComboForgetLevel.Forgotten => 0.70f, // -30% 遗忘惩罚
            _ => 1.0f
        };
    }

    /// <summary>
    /// 获取遗忘等级对应的颜色
    /// </summary>
    public Color GetForgetLevelColor(ComboForgetLevel level)
    {
        return level switch
        {
            ComboForgetLevel.Proficient => new Color(0.2f, 0.9f, 0.3f),   // 绿色
            ComboForgetLevel.Rusty => new Color(0.95f, 0.75f, 0.2f),       // 黄色
            ComboForgetLevel.Forgotten => new Color(0.9f, 0.2f, 0.2f),    // 红色
            _ => Colors.White
        };
    }

    /// <summary>
    /// 获取遗忘等级对应的图标字符
    /// </summary>
    public string GetForgetLevelIcon(ComboForgetLevel level)
    {
        return level switch
        {
            ComboForgetLevel.Proficient => "✓",  // 实心勾
            ComboForgetLevel.Rusty => "~",        // 波浪 ~ 表示生疏
            ComboForgetLevel.Forgotten => "?",   // 问号表示遗忘
            _ => ""
        };
    }

    /// <summary>
    /// 获取某 combo 的完整遗忘信息（用于 UI 显示）
    /// </summary>
    public (ComboForgetLevel level, Color color, string icon, float successRate, int games) GetDisplayInfo(string comboId)
    {
        var level = GetForgetLevel(comboId);
        return (
            level,
            GetForgetLevelColor(level),
            GetForgetLevelIcon(level),
            GetEffectiveSuccessRate(comboId),
            Framework.ComboForgetSystem.Instance?.GetForgetInfo(comboId).games ?? 0
        );
    }

    // ========== 信号处理 ==========

    private void OnForgetStateChanged(string comboId, bool isNowDormant)
    {
        GD.Print($"[ComboForgetUI] State changed: {comboId}, dormant={isNowDormant}");
        RefreshComboUI(comboId);
    }

    private void OnRediscovered(string comboId)
    {
        GD.Print($"[ComboForgetUI] Rediscovered: {comboId}");
        RefreshComboUI(comboId);
        // 可叠加一个"重发现"特效动画信号
    }

    private void OnLocked(string comboId)
    {
        RefreshComboUI(comboId);
    }

    private void OnUnlocked(string comboId)
    {
        RefreshComboUI(comboId);
    }

    /// <summary>
    /// 通知 SkillComboUI 刷新指定 combo 的遗忘 UI
    /// </summary>
    private void RefreshComboUI(string comboId)
    {
        if (_comboUI == null)
            _comboUI = SkillComboUI.Instance;

        _comboUI?.RefreshForgetIndicator(comboId);
    }
}
