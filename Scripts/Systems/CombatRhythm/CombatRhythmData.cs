using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 战斗节拍数据 — 记录玩家受伤频率，计算战斗节奏等级
/// </summary>
public partial class CombatRhythmData : BaseSystem
{
    private static CombatRhythmData _instance;
    public static CombatRhythmData Instance => _instance;

    /// <summary>
    /// 节奏等级（从平静到狂热）
    /// </summary>
    public enum RhythmLevel
    {
        Calm = 0,      // 5秒内受伤 ≤ 1次
        Normal = 1,    // 5秒内受伤 2-3次
        Intense = 2,   // 5秒内受伤 4-5次
        Frenzied = 3   // 5秒内受伤 > 5次
    }

    /// <summary>
    /// 单次受伤记录
    /// </summary>
    private struct DamageRecord
    {
        public float Timestamp;   // 游戏时间（秒）
        public int DamageAmount;
        public string Source;     // 伤害来源标识

        public DamageRecord(float timestamp, int damageAmount, string source)
        {
            Timestamp = timestamp;
            DamageAmount = damageAmount;
            Source = source;
        }
    }

    // ===== 运行时数据 =====

    /// <summary>
    /// 滑动窗口内的受伤记录（仅保留5秒内的）
    /// </summary>
    private List<DamageRecord> _recentDamageRecords = new List<DamageRecord>();

    /// <summary>
    /// 当前节奏等级
    /// </summary>
    private RhythmLevel _currentLevel = RhythmLevel.Calm;

    /// <summary>
    /// 上次战斗的节奏等级（用于跨战斗比较）
    /// </summary>
    private RhythmLevel _lastBattleLevel = RhythmLevel.Calm;

    /// <summary>
    /// 滑动窗口秒数
    /// </summary>
    private const float WINDOW_SECONDS = 5f;

    /// <summary>
    /// 当前战斗开始时间
    /// </summary>
    private float _currentCombatStartTime = 0f;

    /// <summary>
    /// 是否处于战斗中
    /// </summary>
    private bool _inCombat = false;

    // ===== 事件信号 =====
public delegate void RhythmLevelChangedEventHandler(RhythmLevel newLevel, RhythmLevel oldLevel);
public delegate void RhythmIntensityUpdatedEventHandler(RhythmLevel level, int recentDamageCount, float windowSeconds);

    // ===== 公开 API =====

    public override void _Ready()
    {
        _instance = this;
        base._Ready();
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        var bus = EventBusManager.Instance;
        if (bus == null) return;
        bus.Subscribe<PlayerHealthChangedEventData>(EventBusManager.Events.PlayerHealthChanged, OnPlayerHealthChanged);
        bus.Subscribe(EventBusManager.Events.CombatStarted, OnCombatStarted);
        bus.Subscribe(EventBusManager.Events.CombatEnded, OnCombatEnded);
    }

    private void OnPlayerHealthChanged(PlayerHealthChangedEventData data)
    {
        if (!_inCombat) return;
        if (data.Delta >= 0) return; // 只记录受伤，不记录治疗

        float currentTime = GetGameTime();
        int damageAmount = Mathf.Abs(data.Delta);

        // 记录伤害事件
        _recentDamageRecords.Add(new DamageRecord(currentTime, damageAmount, "player"));

        // 清理过期记录
        CleanExpiredRecords(currentTime);

        // 计算并更新节奏等级
        UpdateRhythmLevel(currentTime);
    }

    private void OnCombatStarted()
    {
        _inCombat = true;
        _currentCombatStartTime = GetGameTime();
        _recentDamageRecords.Clear();
        _currentLevel = RhythmLevel.Calm;
    }

    private void OnCombatEnded()
    {
        _lastBattleLevel = _currentLevel;
        _inCombat = false;
        _recentDamageRecords.Clear();
    }

    /// <summary>
    /// 清理超过滑动窗口的旧记录
    /// </summary>
    private void CleanExpiredRecords(float currentTime)
    {
        float cutoff = currentTime - WINDOW_SECONDS;
        _recentDamageRecords.RemoveAll(r => r.Timestamp < cutoff);
    }

    /// <summary>
    /// 根据当前受伤频率更新节奏等级
    /// </summary>
    private void UpdateRhythmLevel(float currentTime)
    {
        CleanExpiredRecords(currentTime);
        int count = _recentDamageRecords.Count;
        RhythmLevel oldLevel = _currentLevel;

        if (count <= 1)
            _currentLevel = RhythmLevel.Calm;
        else if (count <= 3)
            _currentLevel = RhythmLevel.Normal;
        else if (count <= 5)
            _currentLevel = RhythmLevel.Intense;
        else
            _currentLevel = RhythmLevel.Frenzied;

        if (_currentLevel != oldLevel)
        {
            EmitSignal(nameof(RhythmLevelChanged), _currentLevel, oldLevel);
        }

        EmitSignal(nameof(RhythmIntensityUpdated), _currentLevel, count, WINDOW_SECONDS);
    }

    /// <summary>
    /// 每帧驱动：清理过期记录并检查等级变化
    /// </summary>
    public override void _Process(double delta)
    {
        if (!_inCombat) return;

        float currentTime = GetGameTime();
        int prevCount = _recentDamageRecords.Count;
        CleanExpiredRecords(currentTime);

        // 如果有记录过期，可能需要降级
        if (_recentDamageRecords.Count != prevCount)
        {
            UpdateRhythmLevel(currentTime);
        }
    }

    // ===== 公开查询 API =====

    /// <summary>
    /// 获取当前节奏等级
    /// </summary>
    public RhythmLevel GetCurrentLevel() => _currentLevel;

    /// <summary>
    /// 获取当前窗口内受伤次数
    /// </summary>
    public int GetRecentDamageCount()
    {
        CleanExpiredRecords(GetGameTime());
        return _recentDamageRecords.Count;
    }

    /// <summary>
    /// 获取滑动窗口秒数
    /// </summary>
    public float GetWindowSeconds() => WINDOW_SECONDS;

    /// <summary>
    /// 是否处于战斗中
    /// </summary>
    public bool IsInCombat() => _inCombat;

    /// <summary>
    /// 获取当前战斗持续时间（秒）
    /// </summary>
    public float GetCurrentCombatDuration()
    {
        if (!_inCombat) return 0f;
        return GetGameTime() - _currentCombatStartTime;
    }

    /// <summary>
    /// 获取上次战斗的节奏等级
    /// </summary>
    public RhythmLevel GetLastBattleLevel() => _lastBattleLevel;

    // ===== 工具方法 =====

    private float GetGameTime()
    {
        return OS.GetTicksMsec() / 1000f;
    }

    // ===== 持久化 =====

    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        data["last_battle_level"] = (int)_lastBattleLevel;
        return data;
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        if (data.Contains("last_battle_level"))
            _lastBattleLevel = (RhythmLevel)(int)(float)data["last_battle_level"];
    }

    public override void _ExitTree()
    {
        if (_instance == this)
            _instance = null;
    }
}
