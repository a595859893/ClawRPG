using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Managers;
using ClawRPG.Scripts.Systems.ProceduralDungeon;
using ClawRPG.Scripts.Systems;
using ClawRPG.Scripts.Systems.Pets;

/// <summary>
/// 宠物行为日志器 — 在每个房间记录玩家的核心行为
/// 
/// 职责：
/// 1. 订阅游戏事件（技能使用、受伤、击杀等）
/// 2. 识别当前房间环境类型
/// 3. 将行为映射为 PlayerBehaviorType 并记录到 PetMimicryData
/// 4. 支持行为印记跨游戏持久化
/// </summary>
public partial class PetBehaviorLogger : Node
{
    // ── Singleton ──────────────────────────────────────────────────────────
    public static PetBehaviorLogger Instance { get; private set; }

    // ── State ─────────────────────────────────────────────────────────────
    private RoomEnvironmentType _currentEnvironment = RoomEnvironmentType.None;
    private bool _isLowHpState = false;
    private int _attackCountThisRoom = 0;
    private int _dodgeCountThisRoom = 0;
    private float _lowHpActionTimer = 0f;
    private const float LOW_HP_THRESHOLD = 0.3f;  // HP below 30%
    private const float LOW_HP_WINDOW_SECONDS = 3f; // Actions within 3s of low-HP state count

    // Per-room event cooldown to avoid flooding
    private HashSet<string> _cooldownCache = new HashSet<string>();
    private float _cooldownAccumulator = 0f;
    private const float COOLDOWN_DURATION = 2f; // Same event type in same room, min 2s apart

    // Reference to PetMimicryData (where imprints are stored)
    private PetMimicryData _mimicryData;

    public override void _Ready()
    {
        Instance = this;
        SubscribeToEvents();
        _mimicryData = PetMimicryData.Instance;
        GD.Print("[PetBehaviorLogger] Initialized");
    }

    public override void _Process(double delta)
    {
        _cooldownAccumulator += delta;
        if (_cooldownAccumulator >= COOLDOWN_DURATION)
        {
            _cooldownAccumulator = 0f;
            _cooldownCache.Clear();
        }

        // Low-HP action window tracking
        if (_isLowHpState)
        {
            _lowHpActionTimer += delta;
            if (_lowHpActionTimer > LOW_HP_WINDOW_SECONDS)
            {
                _isLowHpState = false;
                _lowHpActionTimer = 0f;
            }
        }
    }

    private void SubscribeToEvents()
    {
        var bus = EventBusManager.Instance;
        if (bus == null) return;

        // Enemy damaged — detect fire/ice/electric/shadow/holy/nature skill usage
        bus.Subscribe<EnemyDamagedEventData>(EventBusManager.Events.EnemyDamaged, OnEnemyDamaged);

        // Player health changed — detect low-HP aggression
        bus.Subscribe<PlayerHealthChangedEventData>(EventBusManager.Events.PlayerHealthChanged, OnPlayerHealthChanged);

        // Player died — detect retreat failure (quick death)
        bus.Subscribe<PlayerDiedEventData>(EventBusManager.Events.PlayerDied, OnPlayerDied);

        // Enemy died — detect focus-elite / aggressive-attack
        bus.Subscribe<EnemyDiedEventData>(EventBusManager.Events.EnemyDied, OnEnemyDied);

        // Pet synergy — detect pet synergy behavior (Godot signal from PetCombatCompanionSystem)
        if (PetCombatCompanionSystem.Instance != null)
        {
            PetCombatCompanionSystem.Instance.SynergyAttackTriggered += OnSynergyTriggeredDirect;
        }

        // Scene / room changed — update current environment
        bus.Subscribe<string>(EventBusManager.Events.SceneChanged, OnSceneChanged);

        // Dungeon room cleared — flush per-room counters
        bus.Subscribe<DungeonRoomClearedEventData>("DungeonRoomCleared", OnRoomCleared);
    }

    // ── Event Handlers ─────────────────────────────────────────────────────

    private void OnEnemyDamaged(EnemyDamagedEventData data)
    {
        if (data?.Enemy == null || data.Attacker == null) return;

        string dmgType = data.DamageType?.ToLowerInvariant() ?? "physical";
        PlayerBehaviorType? behavior = null;

        // Map damage type to behavior
        switch (dmgType)
        {
            case "fire":
            case "flame":
                behavior = PlayerBehaviorType.UseFireSkill;
                break;
            case "ice":
            case "frost":
                behavior = PlayerBehaviorType.UseIceSkill;
                break;
            case "electric":
            case "lightning":
                behavior = PlayerBehaviorType.UseElectricSkill;
                break;
            case "shadow":
            case "dark":
            case "void":
                behavior = PlayerBehaviorType.UseShadowSkill;
                break;
            case "holy":
            case "divine":
                behavior = PlayerBehaviorType.UseHolySkill;
                break;
            case "nature":
            case "poison":
                behavior = PlayerBehaviorType.UseNatureSkill;
                break;
        }

        if (behavior.HasValue)
        {
            RecordImprint(behavior.Value);

            // Track attack frequency
            _attackCountThisRoom++;
            if (_attackCountThisRoom >= 5)
            {
                // Aggressive attack pattern detected
                RecordImprint(PlayerBehaviorType.AggressiveAttack);
            }
        }
    }

    private void OnPlayerHealthChanged(PlayerHealthChangedEventData data)
    {
        if (data == null || data.Player == null) return;

        float hpPercent = data.HealthPercentage;

        // REQ-149: 同步HP状态到PetMimicryData（用于性格触发器）
        _mimicryData?.SetCurrentHpPercent(hpPercent);

        if (hpPercent <= LOW_HP_THRESHOLD && !_isLowHpState)
        {
            _isLowHpState = true;
            _lowHpActionTimer = 0f;
        }

        // REQ-149: 从低HP恢复 → 事件驱动勇敢性格加成
        // （玩家在低HP状态存活后变得勇敢）
        if (hpPercent > LOW_HP_THRESHOLD && _isLowHpState && _lowHpActionTimer > 5f)
        {
            _mimicryData?.TriggerEventDrivenBonus(PlayerBehaviorType.LowHPAggression, 1.0f);
        }
    }

    private void OnPlayerDied(PlayerDiedEventData data)
    {
        // Quick retreat failure — player died near room entrance or quickly after combat start
        if (data != null)
        {
            // Flag retreat behavior when death happens in escape-type environment
            if (_currentEnvironment.HasFlag(RoomEnvironmentType.Escape))
            {
                RecordImprint(PlayerBehaviorType.QuickRetreat);
            }
        }
    }

    private void OnEnemyDied(EnemyDiedEventData data)
    {
        if (data?.Enemy == null) return;

        // Check if it's an elite/boss death → FocusElite
        string enemyType = data.EnemyType?.ToLowerInvariant() ?? "";
        if (enemyType.ContainsKey("elite") || enemyType.ContainsKey("boss"))
        {
            RecordImprint(PlayerBehaviorType.FocusElite);

            // REQ-149: 击杀精英/Boss → 事件驱动进攻性格加成
            _mimicryData?.TriggerEventDrivenBonus(PlayerBehaviorType.AggressiveAttack, 1.5f);
        }
    }

    private void OnSynergyTriggeredDirect(string petId, string attackType, float syncLevel)
    {
        RecordImprint(PlayerBehaviorType.PetSynergy);

        // REQ-149: 协同攻击触发 → 事件驱动加成（宠物协战行为强化）
        _mimicryData?.TriggerEventDrivenBonus(PlayerBehaviorType.PetSynergy, syncLevel * 0.5f);
    }

    private void OnSceneChanged(string scenePath)
    {
        UpdateCurrentEnvironment();
    }

    private void OnRoomCleared(DungeonRoomClearedEventData data)
    {
        // Analyze room behavior summary before clearing
        AnalyzeAndFlushRoom();
    }

    // ── Core Logic ─────────────────────────────────────────────────────────

    /// <summary>
    /// Update current room environment type from dungeon system
    /// </summary>
    private void UpdateCurrentEnvironment()
    {
        try
        {
            var dungeon = ProceduralDungeonSystem.Instance?.CurrentDungeon;
            var room = dungeon?.CurrentRoom;
            _currentEnvironment = RoomEnvironmentClassifier.Classify(room);

            // Reset per-room counters
            _attackCountThisRoom = 0;
            _dodgeCountThisRoom = 0;
            _cooldownCache.Clear();
            _cooldownAccumulator = 0f;

            // REQ-149: 同步环境到PetMimicryData（用于环境专精触发器）
            _mimicryData?.SetCurrentEnvironment(_currentEnvironment);

            GD.Print($"[PetBehaviorLogger] Room environment: {RoomEnvironmentClassifier.GetDisplayName(_currentEnvironment)}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[PetBehaviorLogger] Failed to update environment: {ex.Message}");
            _currentEnvironment = RoomEnvironmentType.None;
        }
    }

    /// <summary>
    /// Record a player behavior as an imprint for the current environment
    /// </summary>
    private void RecordImprint(PlayerBehaviorType behavior)
    {
        if (_mimicryData == null) return;

        // Apply cooldown
        string cooldownKey = $"{_currentEnvironment}_{behavior}";
        if (_cooldownCache.Contains(cooldownKey))
            return;
        _cooldownCache.Add(cooldownKey);

        // Low-HP aggression check
        if (_isLowHpState && behavior == PlayerBehaviorType.AggressiveAttack)
        {
            RecordImprintInternal(PlayerBehaviorType.LowHPAggression);
        }

        RecordImprintInternal(behavior);
    }

    private void RecordImprintInternal(PlayerBehaviorType behavior)
    {
        if (_mimicryData == null) return;

        // Find existing imprint for this (environment, behavior) pair
        BehaviorImprint imprint = _mimicryData.GetImprint(_currentEnvironment, behavior);
        if (imprint == null)
        {
            imprint = new BehaviorImprint
            {
                EnvironmentType = _currentEnvironment,
                BehaviorType = behavior,
                ImprintLevel = 0,
                Xp = 0f,
                LastRecordedAt = DateTime.Now,
                TotalTriggers = 0
            };
            _mimicryData.AddImprint(imprint);
        }

        imprint.LastRecordedAt = DateTime.Now;
        imprint.TotalTriggers++;
        float xpGained = GetXpForBehavior(behavior);
        bool leveledUp = imprint.AddXp(xpGained);

        // Notify level tracker for decay management and UI updates
        if (MimicryLevelTracker.Instance != null)
        {
            MimicryLevelTracker.Instance.OnImprintXpGained(imprint, xpGained);
        }

        if (leveledUp)
        {
            GD.Print($"[PetBehaviorLogger] Imprint leveled up! {behavior} in {RoomEnvironmentClassifier.GetDisplayName(_currentEnvironment)} → Level {imprint.ImprintLevel}");
        }
    }

    /// <summary>
    /// Get XP amount based on behavior significance
    /// </summary>
    private float GetXpForBehavior(PlayerBehaviorType behavior)
    {
        return behavior switch
        {
            PlayerBehaviorType.LowHPAggression => 1.2f,  // High risk = high XP
            PlayerBehaviorType.FocusElite => 1.0f,
            PlayerBehaviorType.PetSynergy => 0.8f,
            PlayerBehaviorType.AggressiveAttack => 0.7f,
            PlayerBehaviorType.UseFireSkill or PlayerBehaviorType.UseIceSkill
                or PlayerBehaviorType.UseElectricSkill => 0.6f,
            PlayerBehaviorType.QuickRetreat => 0.5f,
            PlayerBehaviorType.DefensiveStance => 0.5f,
            _ => 0.3f
        };
    }

    /// <summary>
    /// Flush per-room counters when room is cleared
    /// </summary>
    private void AnalyzeAndFlushRoom()
    {
        if (_dodgeCountThisRoom >= 3)
        {
            RecordImprint(PlayerBehaviorType.FrequentDodge);
        }

        if (_currentEnvironment.HasFlag(RoomEnvironmentType.Treasure))
        {
            // Player collected loot in treasure room
            RecordImprint(PlayerBehaviorType.CollectLoot);
        }

        if (_currentEnvironment.HasFlag(RoomEnvironmentType.Boss))
        {
            // Analyze boss room behavior
            if (_isLowHpState)
            {
                RecordImprint(PlayerBehaviorType.LowHPAggression);
            }
        }

        // Reset
        _attackCountThisRoom = 0;
        _dodgeCountThisRoom = 0;
        _isLowHpState = false;
        _lowHpActionTimer = 0f;
    }

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>
    /// Manually log a behavior (for systems that don't fire events)
    /// </summary>
    public void LogBehavior(PlayerBehaviorType behavior)
    {
        UpdateCurrentEnvironment();
        RecordImprint(behavior);
    }

    /// <summary>
    /// Log dodge event (called by player movement system)
    /// </summary>
    public void LogDodge()
    {
        _dodgeCountThisRoom++;
        UpdateCurrentEnvironment();
        RecordImprint(PlayerBehaviorType.FrequentDodge);
    }

    /// <summary>
    /// Log healing item usage
    /// </summary>
    public void LogHealing()
    {
        UpdateCurrentEnvironment();
        RecordImprint(PlayerBehaviorType.UseHealing);

        // REQ-149: 使用治疗 → 事件驱动防守性格加成
        _mimicryData?.TriggerEventDrivenBonus(PlayerBehaviorType.DefensiveStance, 0.8f);
    }

    /// <summary>
    /// Log trap trigger
    /// </summary>
    public void LogTrapTrigger()
    {
        UpdateCurrentEnvironment();
        RecordImprint(PlayerBehaviorType.TriggerTrap);
    }

    /// <summary>
    /// Force refresh current environment (called when entering a new room)
    /// </summary>
    public void RefreshEnvironment()
    {
        UpdateCurrentEnvironment();
    }
}

/// <summary>
/// 协同攻击触发事件数据
/// </summary>
public class SynergyAttackTriggeredEventData
{
    public Player Player { get; set; }
    public Pet Pet { get; set; }
    public Vector3 Position { get; set; }
    public float SynergyLevel { get; set; }

    public SynergyAttackTriggeredEventData() { }

    public SynergyAttackTriggeredEventData(Player player, Pet pet, Vector3 position, float synergyLevel)
    {
        Player = player;
        Pet = pet;
        Position = position;
        SynergyLevel = synergyLevel;
    }
}

/// <summary>
/// 房间清除事件数据
/// </summary>
public class DungeonRoomClearedEventData
{
    public string RoomId { get; set; }
    public RoomType RoomType { get; set; }
    public float ClearTime { get; set; }
    public int EnemiesDefeated { get; set; }

    public DungeonRoomClearedEventData() { }

    public DungeonRoomClearedEventData(string roomId, RoomType roomType, float clearTime, int enemiesDefeated)
    {
        RoomId = roomId;
        RoomType = roomType;
        ClearTime = clearTime;
        EnemiesDefeated = enemiesDefeated;
    }
}
