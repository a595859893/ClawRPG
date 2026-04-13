using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Managers;

/// <summary>
/// Tracks player actions during a run and records them as deposit events.
/// Subscribes to combat/game events and maps them to deposit types.
/// </summary>
public partial class DepositTracker : Node
{
    // ── Singleton ──────────────────────────────────────────────────────────
    public static DepositTracker Instance { get; private set; }

    // ── Per-Run Action Counters ─────────────────────────────────────────────
    private int _fireDamageDealt = 0;
    private int _damageTaken = 0;
    private int _comboUses = 0;
    private int _lowHpActions = 0;  // Actions taken while HP < 30%
    private int _petAssists = 0;

    // Track low-HP state
    private bool _isLowHp = false;

    // ── XP amounts per event ───────────────────────────────────────────────
    private const float XP_PER_FIRE_DAMAGE = 0.5f;    // Fire damage dealt
    private const float XP_PER_DAMAGE_TAKEN = 0.3f;    // Damage received
    private const float XP_PER_COMBO_USE = 0.4f;       // Each combo executed
    private const float XP_PER_LOW_HP_ACTION = 0.8f;   // Action while low HP (high risk = high reward)
    private const float XP_PER_PET_ASSIST = 0.5f;     // Pet helped in combat

    public override void _Ready()
    {
        Instance = this;
        SubscribeToEvents();
        GD.Print("[DepositTracker] Initialized");
    }

    private void SubscribeToEvents()
    {
        var bus = EventBusManager.Instance;
        if (bus == null) return;

        // Fire damage — via EnemyDamaged with fire damage type
        bus.Subscribe<EnemyDamagedEventData>(EventBusManager.Events.EnemyDamaged, OnEnemyDamaged);

        // Damage taken
        bus.Subscribe<PlayerHealthChangedEventData>(EventBusManager.Events.PlayerHealthChanged, OnPlayerHealthChanged);

        // Combo usage — via SkillComboSystem signal (if available)
        // Pet assist — via SynergyAttackTriggered signal (REQ-136)

        // Game over — apply global decay
        bus.Subscribe<GameOverEventData>(EventBusManager.Events.GameOver, OnGameOver);

        // Combat end — not needed for now; game-over handles flush
    }

    private void OnEnemyDamaged(EnemyDamagedEventData data)
    {
        if (data.Attacker is Player)
        {
            // Check damage type - fire if DamageType == "fire"
            // For now, record all player-dealt damage as potential Ember
            // A more refined system would check the actual damage type from the skill used
            _fireDamageDealt += data.Damage;

            if (_fireDamageDealt > 0)
            {
                // Record 1 deposit per threshold (every 10 damage = 1 deposit event)
                int depositEvents = _fireDamageDealt / 10;
                for (int i = 0; i < depositEvents; i++)
                {
                    DepositData.Instance?.RecordDeposit(DepositData.DepositType.Ember, XP_PER_FIRE_DAMAGE);
                }
                _fireDamageDealt %= 10; // Carry remainder
            }
        }
    }

    private void OnPlayerHealthChanged(PlayerHealthChangedEventData data)
    {
        if (data.MaxHealth <= 0) return;

        // Track damage taken (HP going down)
        if (data.NewHealth < data.OldHealth)
        {
            int damageAmount = data.OldHealth - data.NewHealth;
            _damageTaken += damageAmount;

            int depositEvents = _damageTaken / 15;
            for (int i = 0; i < depositEvents; i++)
            {
                DepositData.Instance?.RecordDeposit(DepositData.DepositType.Sediment, XP_PER_DAMAGE_TAKEN * damageAmount);
            }
            _damageTaken %= 15;
        }

        // Track entering low HP state (< 30% max HP)
        float hpPercent = data.NewHealth / (float)data.MaxHealth;
        bool nowLowHp = hpPercent < 0.30f;
        if (nowLowHp && !_isLowHp)
        {
            _isLowHp = true; // Just transitioned to low HP
        }
        else if (!nowLowHp && _isLowHp)
        {
            _isLowHp = false; // Recovered from low HP
        }
    }

    private void OnGameOver(GameOverEventData data)
    {
        // Apply decay to all slots (called here for game-over triggered decay)
        DepositData.Instance?.ApplyGlobalDecay();

        // Flush remaining small counts (rounding)
        if (_fireDamageDealt > 0)
            DepositData.Instance?.RecordDeposit(DepositData.DepositType.Ember, XP_PER_FIRE_DAMAGE * _fireDamageDealt / 10f);
        if (_damageTaken > 0)
            DepositData.Instance?.RecordDeposit(DepositData.DepositType.Sediment, _damageTaken * XP_PER_DAMAGE_TAKEN / 15f);
        if (_comboUses > 0)
            DepositData.Instance?.RecordDeposit(DepositData.DepositType.Echo, XP_PER_COMBO_USE * _comboUses);
        if (_lowHpActions > 0)
            DepositData.Instance?.RecordDeposit(DepositData.DepositType.Debt, XP_PER_LOW_HP_ACTION * _lowHpActions);
        if (_petAssists > 0)
            DepositData.Instance?.RecordDeposit(DepositData.DepositType.Synergy, XP_PER_PET_ASSIST * _petAssists);

        ResetRunData();
    }

    // ── Public API for other systems to call ───────────────────────────────

    /// <summary>Call when a combo is successfully executed.</summary>
    public void RecordComboUse(int comboLength)
    {
        _comboUses += comboLength;
        DepositData.Instance?.RecordDeposit(DepositData.DepositType.Echo, XP_PER_COMBO_USE * comboLength);
    }

    /// <summary>Call when player takes an action while below 30% HP.</summary>
    public void RecordLowHPAction()
    {
        _lowHpActions++;
        DepositData.Instance?.RecordDeposit(DepositData.DepositType.Debt, XP_PER_LOW_HP_ACTION);
    }

    /// <summary>Call when pet assists in combat.</summary>
    public void RecordPetAssist()
    {
        _petAssists++;
        DepositData.Instance?.RecordDeposit(DepositData.DepositType.Synergy, XP_PER_PET_ASSIST);
    }

    /// <summary>Reset counters for a new run.</summary>
    public void ResetRunData()
    {
        _fireDamageDealt = 0;
        _damageTaken = 0;
        _comboUses = 0;
        _lowHpActions = 0;
        _petAssists = 0;
        _isLowHp = false;
    }
}
