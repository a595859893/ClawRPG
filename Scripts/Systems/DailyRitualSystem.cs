using Godot;
using System;
using System.Collections.Generic;
using SaveSystem = ClawRPG.Scripts.Framework.SaveSystem;

/// <summary>
/// 每日仪式系统 - 管理每日仪式任务
/// </summary>
public partial class DailyRitualSystem : BaseSystem
{
    public static DailyRitualSystem Instance { get; private set; }

    // Active ritual state
    public string CurrentRitualId { get; private set; } = "";
    public float RitualProgress { get; private set; } = 0f;
    public bool IsRitualActive => !string.IsNullOrEmpty(CurrentRitualId);
    public DateTime RitualStartTime { get; private set; }

    // Ritual history and stats
    public int TotalRitualsPerformed { get; private set; }
    public Dictionary<RitualType, int> RitualsByType { get; private set; }
    public Dictionary<RitualTier, int> RitualsByTier { get; private set; }
    public int TotalGoldSpent { get; private set; }
    public int TotalReputationGained { get; private set; }
    public List<string> UnlockedRitualIds { get; private set; }

    // Daily reset
    private long _lastResetTime;
    private int _dailyRitualsRemaining;

    // Current bonuses from rituals
    public Dictionary<string, float> ActiveBonuses { get; private set; }

    // ===== UI 桥接（REQ-075 解耦） =====
    private DailyRitualUI _ui;

    public void RegisterUI(DailyRitualUI ui)
    {
        _ui = ui;
        if (_ui == null) return;

        // 订阅 UI 事件
        _ui.OnRefreshRequested += HandleRefreshRequested;
        _ui.OnStartRitualRequested += HandleStartRitualRequested;
        _ui.OnClearBonusesRequested += HandleClearBonusesRequested;
    }

    public void UnregisterUI(DailyRitualUI ui)
    {
        if (ui == null) return;
        ui.OnRefreshRequested -= HandleRefreshRequested;
        ui.OnStartRitualRequested -= HandleStartRitualRequested;
        ui.OnClearBonusesRequested -= HandleClearBonusesRequested;
        if (_ui == ui) _ui = null;
    }

    private void HandleRefreshRequested()
    {
        if (_ui == null) return;

        // 获取所有仪式数据
        var allRituals = DailyRitualDatabase.Instance.GetAllRituals();
        _ui.UpdateRitualList(allRituals, UnlockedRitualIds, CurrentRitualId);
        _ui.UpdateStats(TotalRitualsPerformed, TotalGoldSpent, TotalReputationGained);
        _ui.UpdateDailyCount(_dailyRitualsRemaining);
    }

    private void HandleStartRitualRequested(string ritualId)
    {
        if (_ui == null) return;

        var ritual = DailyRitualDatabase.Instance.GetRitual(ritualId);
        if (ritual == null) return;

        // 获取玩家金币（通过场景树）
        var player = GetTree().CurrentScene?.GetNode<Player>("Player");
        if (player == null)
        {
            GD.Print("Player not found");
            return;
        }

        // 检查金币（使用反射获取 gold 属性）
        var goldProperty = player.GetType().GetProperty("Gold");
        int playerGold = goldProperty != null ? (int)goldProperty.GetValue(player) : 0;

        if (playerGold < ritual.GoldCost)
        {
            GD.Print("Not enough gold");
            return;
        }

        // 扣除金币
        if (goldProperty != null)
        {
            goldProperty.SetValue(player, playerGold - ritual.GoldCost);
        }

        // 启动仪式
        if (StartRitual(ritualId, playerGold - ritual.GoldCost))
        {
            _ui.NotifyRitualStarted(ritualId);
            _ui.UpdateDailyCount(_dailyRitualsRemaining);
        }
    }

    private void HandleClearBonusesRequested()
    {
        ClearBonuses();
    }

    public override void _Ready()
    {
        Instance = this;
        RitualsByType = new Dictionary<RitualType, int>();
        RitualsByTier = new Dictionary<RitualTier, int>();
        UnlockedRitualIds = new List<string>();
        ActiveBonuses = new Dictionary<string, float>();

        foreach (RitualType type in Enum.GetValues(typeof(RitualType)))
            RitualsByType[type] = 0;
        foreach (RitualTier tier in Enum.GetValues(typeof(RitualTier)))
            RitualsByTier[tier] = 0;

        LoadData();
        CheckDailyReset();
    }

    public override void _Process(double delta)
    {
        if (IsRitualActive)
        {
            UpdateRitualProgress((float)delta);
        }
    }

    private void UpdateRitualProgress(float delta)
    {
        var ritual = DailyRitualDatabase.Instance.GetRitual(CurrentRitualId);
        if (ritual == null) return;

        RitualProgress += delta;
        
        // Update progress percentage
        float progressPercent = Mathf.Min(RitualProgress / ritual.Duration, 1.0f);
        
        // Apply partial bonuses as ritual progresses
        UpdateActiveBonuses(ritual, progressPercent);

        // Complete ritual when finished
        if (RitualProgress >= ritual.Duration)
        {
            CompleteRitual(ritual);
        }
    }

    private void UpdateActiveBonuses(RitualData ritual, float progressPercent)
    {
        // Apply bonuses based on progress (linear progression)
        foreach (var bonus in ritual.AttributeBonuses)
        {
            if (ActiveBonuses.ContainsKey(bonus.Key))
                ActiveBonuses[bonus.Key] = bonus.Value * progressPercent;
            else
                ActiveBonuses[bonus.Key] = bonus.Value * progressPercent;
        }
    }

    private void CompleteRitual(RitualData ritual)
    {
        // Finalize bonuses
        foreach (var bonus in ritual.AttributeBonuses)
        {
            if (ActiveBonuses.ContainsKey(bonus.Key))
                ActiveBonuses[bonus.Key] = bonus.Value;
            else
                ActiveBonuses[bonus.Key] = bonus.Value;
        }

        // Update statistics
        TotalRitualsPerformed++;
        RitualsByType[ritual.Type]++;
        RitualsByTier[ritual.Tier]++;
        TotalGoldSpent += ritual.GoldCost;
        TotalReputationGained += ritual.ReputationGain;

        // Unlock higher tier rituals
        UnlockHigherTierRituals(ritual);

        // Decrease daily count
        _dailyRitualsRemaining--;

        // Notify completion
        GD.Print($"Ritual completed: {ritual.Name} ({ritual.Tier})");
        
        // Emit signal
        EmitSignal(nameof(RitualCompleted), ritual.Id);

        // Clear current ritual (bonuses remain active until next ritual or manual clear)
        var oldRitualId = CurrentRitualId;
        CurrentRitualId = "";
        RitualProgress = 0f;

        // REQ-075 桥接通知 UI
        _ui?.NotifyRitualCompleted(oldRitualId);
        _ui?.UpdateDailyCount(_dailyRitualsRemaining);
        _ui?.UpdateStats(TotalRitualsPerformed, TotalGoldSpent, TotalReputationGained);

        SaveData();
    }

    private void UnlockHigherTierRituals(RitualData completedRitual)
    {
        var allRituals = DailyRitualDatabase.Instance.GetAllRituals();
        foreach (var ritual in allRituals)
        {
            if (ritual.Type == completedRitual.Type && 
                (int)ritual.Tier > (int)completedRitual.Tier &&
                !UnlockedRitualIds.Contains(ritual.Id))
            {
                // Unlock if player has performed enough rituals of this type
                if (RitualsByType[completedRitual.Type] >= GetTierUnlockRequirement(ritual.Tier))
                {
                    UnlockedRitualIds.Add(ritual.Id);
                    EmitSignal(nameof(RitualUnlocked), ritual.Id);
                    // REQ-075 桥接通知 UI
                    _ui?.NotifyRitualUnlocked(ritual.Id);
                }
            }
        }
    }

    private int GetTierUnlockRequirement(RitualTier tier)
    {
        return tier switch
        {
            RitualTier.Adept => 5,
            RitualTier.Master => 15,
            RitualTier.Legendary => 30,
            _ => 0
        };
    }

    public bool StartRitual(string ritualId, int playerGold)
    {
        var ritual = DailyRitualDatabase.Instance.GetRitual(ritualId);
        if (ritual == null)
        {
            GD.PrintErr($"Ritual not found: {ritualId}");
            return false;
        }

        if (IsRitualActive)
        {
            GD.Print("Cannot start new ritual while one is active");
            return false;
        }

        if (_dailyRitualsRemaining <= 0)
        {
            GD.Print("No daily rituals remaining");
            return false;
        }

        if (!UnlockedRitualIds.Contains(ritualId) && ritual.Tier != RitualTier.Novice)
        {
            GD.Print("Ritual not yet unlocked");
            return false;
        }

        if (playerGold < ritual.GoldCost)
        {
            GD.Print("Not enough gold for ritual");
            return false;
        }

        // Clear previous bonuses
        ActiveBonuses.Clear();

        // Start new ritual
        CurrentRitualId = ritualId;
        RitualProgress = 0f;
        RitualStartTime = DateTime.Now;

        // Deduct gold (handled by caller)
        
        EmitSignal(nameof(RitualStarted), ritualId);
        // REQ-075 桥接通知 UI
        _ui?.NotifyRitualStarted(ritualId);
        SaveData();

        return true;
    }

    public void CancelRitual()
    {
        if (!IsRitualActive) return;

        var ritual = DailyRitualDatabase.Instance.GetRitual(CurrentRitualId);
        
        // Clear bonuses
        ActiveBonuses.Clear();
        
        CurrentRitualId = "";
        RitualProgress = 0f;
        
        EmitSignal(nameof(RitualCancelled));
        // REQ-075 桥接通知 UI
        _ui?.NotifyRitualCompleted("");
        SaveData();
    }

    public void ClearBonuses()
    {
        ActiveBonuses.Clear();
        EmitSignal(nameof(BonusesCleared));
        // REQ-075 桥接通知 UI
        _ui?.UpdateStats(TotalRitualsPerformed, TotalGoldSpent, TotalReputationGained);
    }

    private void CheckDailyReset()
    {
        var now = DateTime.Now;
        var today = new DateTime(now.Year, now.Month, now.Day);
        long todayTicks = today.Ticks / 10000; // Convert to milliseconds

        if (_lastResetTime < todayTicks)
        {
            _lastResetTime = todayTicks;
            _dailyRitualsRemaining = 3; // 3 rituals per day
            // REQ-075 桥接通知 UI
            _ui?.UpdateDailyCount(_dailyRitualsRemaining);
            SaveData();
        }
    }

    public int GetDailyRitualsRemaining() => _dailyRitualsRemaining;

    public Dictionary<string, float> GetTotalBonuses()
    {
        var total = new Dictionary<string, float>(ActiveBonuses);
        
        // Add reputation bonuses if any
        // (This would integrate with a ReputationSystem if present)
        
        return total;
    }

    public Dictionary<string, float> GetPlayerBonuses()
    {
        return new Dictionary<string, float>(ActiveBonuses);
    }

    // Signal definitions
    [Signal]
    public delegate void RitualCompletedEventHandler(string ritualId);

    [Signal]
    public delegate void RitualUnlockedEventHandler(string ritualId);

    [Signal]
    public delegate void RitualStartedEventHandler(string ritualId);

    [Signal]
    public delegate void RitualCancelledEventHandler();

    [Signal]
    public delegate void BonusesClearedEventHandler();

    // 数据持久化接口
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>
        {
            { "total_rituals", TotalRitualsPerformed },
            { "total_gold_spent", TotalGoldSpent },
            { "total_reputation", TotalReputationGained },
            { "last_reset_time", _lastResetTime },
            { "daily_rituals_remaining", _dailyRitualsRemaining },
            { "unlocked_rituals", UnlockedRitualIds },
            { "current_ritual", CurrentRitualId },
            { "ritual_progress", RitualProgress }
        };

        // Save rituals by type and tier
        var ritualsByType = new Dictionary<string, int>();
        foreach (var kvp in RitualsByType)
            ritualsByType[kvp.Key.ToString()] = kvp.Value;
        data["rituals_by_type"] = ritualsByType;

        var ritualsByTier = new Dictionary<string, int>();
        foreach (var kvp in RitualsByTier)
            ritualsByTier[kvp.Key.ToString()] = kvp.Value;
        data["rituals_by_tier"] = ritualsByTier;

        return new Dictionary(data);
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        if (data.ContainsKey("total_rituals"))
            TotalRitualsPerformed = Convert.ToInt32(data["total_rituals"]);
        if (data.ContainsKey("total_gold_spent"))
            TotalGoldSpent = Convert.ToInt32(data["total_gold_spent"]);
        if (data.ContainsKey("total_reputation"))
            TotalReputationGained = Convert.ToInt32(data["total_reputation"]);
        if (data.ContainsKey("last_reset_time"))
            _lastResetTime = Convert.ToInt64(data["last_reset_time"]);
        if (data.ContainsKey("daily_rituals_remaining"))
            _dailyRitualsRemaining = Convert.ToInt32(data["daily_rituals_remaining"]);
        if (data.ContainsKey("unlocked_rituals"))
            UnlockedRitualIds = new List<string>((List<string>)data["unlocked_rituals"]);
        if (data.ContainsKey("current_ritual"))
            CurrentRitualId = data["current_ritual"].ToString();
        if (data.ContainsKey("ritual_progress"))
            RitualProgress = Convert.ToSingle(data["ritual_progress"]);

        // Load rituals by type
        if (data.ContainsKey("rituals_by_type"))
        {
            var dict = (Dictionary<string, object>)data["rituals_by_type"];
            foreach (var kvp in dict)
            {
                if (Enum.TryParse<RitualType>(kvp.Key, out var type))
                    RitualsByType[type] = Convert.ToInt32(kvp.Value);
            }
        }

        // Load rituals by tier
        if (data.ContainsKey("rituals_by_tier"))
        {
            var dict = (Dictionary<string, object>)data["rituals_by_tier"];
            foreach (var kvp in dict)
            {
                if (Enum.TryParse<RitualTier>(kvp.Key, out var tier))
                    RitualsByTier[tier] = Convert.ToInt32(kvp.Value);
            }
        }
    }

    // Save/Load
    public void SaveData()
    {
        var data = new Dictionary<string, object>
        {
            { "total_rituals", TotalRitualsPerformed },
            { "total_gold_spent", TotalGoldSpent },
            { "total_reputation", TotalReputationGained },
            { "last_reset_time", _lastResetTime },
            { "daily_rituals_remaining", _dailyRitualsRemaining },
            { "unlocked_rituals", UnlockedRitualIds },
            { "current_ritual", CurrentRitualId },
            { "ritual_progress", RitualProgress }
        };

        // Save rituals by type and tier
        var ritualsByType = new Dictionary<string, int>();
        foreach (var kvp in RitualsByType)
            ritualsByType[kvp.Key.ToString()] = kvp.Value;
        data["rituals_by_type"] = ritualsByType;

        var ritualsByTier = new Dictionary<string, int>();
        foreach (var kvp in RitualsByTier)
            ritualsByTier[kvp.Key.ToString()] = kvp.Value;
        data["rituals_by_tier"] = ritualsByTier;

        SaveSystem.Save("daily_ritual", data);
    }

    private void LoadData()
    {
        var data = SaveSystem.Load("daily_ritual");
        if (data == null) return;

        if (data.ContainsKey("total_rituals"))
            TotalRitualsPerformed = Convert.ToInt32(data["total_rituals"]);
        if (data.ContainsKey("total_gold_spent"))
            TotalGoldSpent = Convert.ToInt32(data["total_gold_spent"]);
        if (data.ContainsKey("total_reputation"))
            TotalReputationGained = Convert.ToInt32(data["total_reputation"]);
        if (data.ContainsKey("last_reset_time"))
            _lastResetTime = Convert.ToInt64(data["last_reset_time"]);
        if (data.ContainsKey("daily_rituals_remaining"))
            _dailyRitualsRemaining = Convert.ToInt32(data["daily_rituals_remaining"]);
        if (data.ContainsKey("unlocked_rituals"))
            UnlockedRitualIds = new List<string>((List<string>)data["unlocked_rituals"]);
        if (data.ContainsKey("current_ritual"))
            CurrentRitualId = data["current_ritual"].ToString();
        if (data.ContainsKey("ritual_progress"))
            RitualProgress = Convert.ToSingle(data["ritual_progress"]);

        // Load rituals by type
        if (data.ContainsKey("rituals_by_type"))
        {
            var dict = (Dictionary<string, object>)data["rituals_by_type"];
            foreach (var kvp in dict)
            {
                if (Enum.TryParse<RitualType>(kvp.Key, out var type))
                    RitualsByType[type] = Convert.ToInt32(kvp.Value);
            }
        }

        // Load rituals by tier
        if (data.ContainsKey("rituals_by_tier"))
        {
            var dict = (Dictionary<string, object>)data["rituals_by_tier"];
            foreach (var kvp in dict)
            {
                if (Enum.TryParse<RitualTier>(kvp.Key, out var tier))
                    RitualsByTier[tier] = Convert.ToInt32(kvp.Value);
            }
        }

        // Restore active bonuses if ritual was in progress
        if (IsRitualActive)
        {
            var ritual = DailyRitualDatabase.Instance.GetRitual(CurrentRitualId);
            if (ritual != null)
            {
                float progressPercent = Mathf.Min(RitualProgress / ritual.Duration, 1.0f);
                UpdateActiveBonuses(ritual, progressPercent);
            }
        }
    }

    // Integration with player stats
    public void ApplyBonusesToPlayer(Player player)
    {
        if (player == null) return;

        // Apply active ritual bonuses to player
        foreach (var bonus in ActiveBonuses)
        {
            switch (bonus.Key)
            {
                case "attack":
                    player.attack *= (1 + bonus.Value);
                    break;
                case "defense":
                    player.defense *= (1 + bonus.Value);
                    break;
                case "health":
                    player.maxHealth *= (1 + bonus.Value);
                    break;
                case "speed":
                    player.speed *= (1 + bonus.Value);
                    break;
                case "crit_rate":
                    player.critRate += bonus.Value;
                    break;
                case "crit_damage":
                    player.critDamage += bonus.Value;
                    break;
                case "lifesteal":
                    player.lifeSteal += bonus.Value;
                    break;
                case "dodge":
                    player.dodge += bonus.Value;
                    break;
                case "exp":
                    // Applied elsewhere
                    break;
                case "luck":
                    // Applied elsewhere
                    break;
                case "regen":
                    // Applied elsewhere
                    break;
                case "fire_damage":
                case "water_damage":
                case "ice_damage":
                case "thunder_damage":
                case "dark_damage":
                case "holy_damage":
                case "earth_damage":
                case "wind_damage":
                case "poison_damage":
                    // Elemental damage bonuses - would need ElementalSystem integration
                    break;
            }
        }
    }
}
