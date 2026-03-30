using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Roguelike Deposit System — persistent cross-run data.
/// Tracks deposit slots that accumulate from player actions and convert into deposit cards.
/// </summary>
public class DepositData : Node
{
    // ── Deposit Types ──────────────────────────────────────────────────────
    public enum DepositType
    {
        Ember,      // Fire damage dealt
        Sediment,    // Damage taken
        Echo,        // Frequent combo usage
        Debt,        // Actions while low HP
        Synergy      // Pet companion assists
    }

    // ── Per-Slot Data ─────────────────────────────────────────────────────
    [System.Serializable]
    public class DepositSlot
    {
        public DepositType Type = DepositType.Ember;
        public int Level = 0;           // 0–5 (0 = inactive)
        public float Xp = 0f;           // Progress toward next level
        public float LastUsedTimestamp = 0f;  // Game time of last deposit event
        public int TotalDeposits = 0;   // Total events accumulated (for decay calc)

        public DepositSlot() { }

        public DepositSlot(DepositType type)
        {
            Type = type;
        }

        /// <summary>XP required to advance from current level to next.</summary>
        public float XpForNextLevel()
        {
            // Logarithmic curve: higher levels cost more
            return 10f * Mathf.Pow(2f, Level);
        }

        /// <summary>Add XP and level up if threshold reached. Returns true if levelled up.</summary>
        public bool AddXp(float amount)
        {
            Xp += amount;
            bool leveledUp = false;
            while (Level < 5 && Xp >= XpForNextLevel())
            {
                Xp -= XpForNextLevel();
                Level++;
                leveledUp = true;
            }
            TotalDeposits++;
            LastUsedTimestamp = Time.GetTicksMsec() / 1000f;
            return leveledUp;
        }

        /// <summary>Decay slot over time (called on game end).</summary>
        public void ApplyDecay(float decayRate = 0.1f)
        {
            if (Level > 0)
            {
                // Each full game away reduces level by 1
                Level = Mathf.Max(0, Level - 1);
                Xp = 0f;
            }
        }

        /// <summary>Effective intensity for card generation (0.0–1.0 across levels 1–5).</summary>
        public float GetIntensity()
        {
            return Level / 5f;
        }
    }

    // ── State ─────────────────────────────────────────────────────────────
    private Dictionary<DepositType, DepositSlot> _slots = new Dictionary<DepositType, DepositSlot>();

    // ── Singleton ──────────────────────────────────────────────────────────
    public static DepositData Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        // Initialize all slot types
        foreach (DepositType type in Enum.GetValues(typeof(DepositType)))
        {
            _slots[type] = new DepositSlot(type);
        }
        SaveSystem.Instance.RegisterSaveData(this);
    }

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>Record a deposit event (e.g., fire damage dealt → Ember).</summary>
    public void RecordDeposit(DepositType type, float xpAmount = 1f)
    {
        if (!_slots.ContainsKey(type)) return;
        bool leveledUp = _slots[type].AddXp(xpAmount);
        if (leveledUp)
        {
            EmitSignal(nameof(DepositLevelChanged), type, _slots[type].Level);
        }
        EmitSignal(nameof(DepositUpdated), type);
    }

    /// <summary>Get a deposit slot.</summary>
    public DepositSlot GetSlot(DepositType type)
    {
        return _slots.GetValueOrDefault(type, null);
    }

    /// <summary>Get level for a deposit type.</summary>
    public int GetLevel(DepositType type)
    {
        return _slots.GetValueOrDefault(type)?.Level ?? 0;
    }

    /// <summary>Get all non-zero slots (for card generation).</summary>
    public List<DepositSlot> GetActiveSlots()
    {
        var result = new List<DepositSlot>();
        foreach (var slot in _slots.Values)
        {
            if (slot.Level > 0) result.Add(slot);
        }
        return result;
    }

    /// <summary>Apply decay to all slots (called on game end).</summary>
    public void ApplyGlobalDecay()
    {
        foreach (var slot in _slots.Values)
        {
            slot.ApplyDecay();
        }
    }

    /// <summary>Reset all slots (for new game start).</summary>
    public void ResetForNewGame()
    {
        // Slots persist across games; only decay (not reset) on new game
    }

    // ── Signals ────────────────────────────────────────────────────────────
public delegate void DepositUpdated(DepositType type);
public delegate void DepositLevelChanged(DepositType type, int newLevel);

    // ── Save System ────────────────────────────────────────────────────────

    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        var slotsList = new Godot.Collections.Array();
        foreach (var kvp in _slots)
        {
            var slotData = new Dictionary
            {
                ["type"] = (int)kvp.Key,
                ["level"] = kvp.Value.Level,
                ["xp"] = kvp.Value.Xp,
                ["lastUsed"] = kvp.Value.LastUsedTimestamp,
                ["totalDeposits"] = kvp.Value.TotalDeposits
            };
            slotsList.Add(slotData);
        }
        data["depositSlots"] = slotsList;
        return data;
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null || !data.Contains("depositSlots")) return;

        foreach (Dictionary slotData in (Godot.Collections.Array)data["depositSlots"])
        {
            int typeInt = (int)slotData["type"];
            var type = (DepositType)typeInt;
            if (!_slots.ContainsKey(type)) continue;

            var slot = _slots[type];
            slot.Level = (int)slotData["level"];
            slot.Xp = (float)slotData["xp"];
            slot.LastUsedTimestamp = (float)slotData["lastUsed"];
            slot.TotalDeposits = (int)slotData["totalDeposits"];
        }
    }
}
