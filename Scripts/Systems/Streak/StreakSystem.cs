using Godot;
using System;
using System.Collections.Generic;

public class StreakSystem : Node
{
    private StreakData _data;
    private StreakDatabase _database;
    
    // Signals
    public static event Action<StreakType, int> OnStreakUpdated;
    public static event Action<StreakType, int, StreakReward> OnRewardClaimed;
    public static event Action<StreakType> OnStreakBroken;
    public static event Action<StreakType, int> OnMilestoneReached;
    
    public override void _Ready()
    {
        _database = StreakDatabase.Instance;
        LoadData();
    }
    
    private void LoadData()
    {
        _data = new StreakData();
        
        var saveSystem = SaveSystem.Instance;
        if (saveSystem == null) return;

        var data = saveSystem.LoadGame();
        if (data == null || data.Count == 0) return;

        // Load streak data
        if (data.Contains("streak_login")) _data.LoginStreak = (int)data["streak_login"];
        if (data.Contains("streak_battle")) _data.BattleStreak = (int)data["streak_battle"];
        if (data.Contains("streak_quest")) _data.QuestStreak = (int)data["streak_quest"];
        if (data.Contains("streak_dungeon")) _data.DungeonStreak = (int)data["streak_dungeon"];
        if (data.Contains("streak_pet")) _data.PetInteractionStreak = (int)data["streak_pet"];

        if (data.Contains("best_streak_login")) _data.BestLoginStreak = (int)data["best_streak_login"];
        if (data.Contains("best_streak_battle")) _data.BestBattleStreak = (int)data["best_streak_battle"];
        if (data.Contains("best_streak_quest")) _data.BestQuestStreak = (int)data["best_streak_quest"];
        if (data.Contains("best_streak_dungeon")) _data.BestDungeonStreak = (int)data["best_streak_dungeon"];
        if (data.Contains("best_streak_pet")) _data.BestPetInteractionStreak = (int)data["best_streak_pet"];

        if (data.Contains("total_login_days")) _data.TotalLoginDays = (int)data["total_login_days"];
        if (data.Contains("total_battle_days")) _data.TotalBattleDays = (int)data["total_battle_days"];
        if (data.Contains("total_quest_days")) _data.TotalQuestDays = (int)data["total_quest_days"];
        if (data.Contains("total_dungeon_days")) _data.TotalDungeonDays = (int)data["total_dungeon_days"];
        if (data.Contains("total_pet_days")) _data.TotalPetInteractionDays = (int)data["total_pet_days"];

        if (data.Contains("streak_freeze_tokens")) _data.StreakFreezeTokens = (int)data["streak_freeze_tokens"];
        if (data.Contains("total_freeze_used")) _data.TotalStreakFreezeUsed = (int)data["total_freeze_used"];

        if (data.Contains("last_login_time")) _data.LastLoginTime = (long)data["last_login_time"];
        if (data.Contains("last_battle_time")) _data.LastBattleTime = (long)data["last_battle_time"];
        if (data.Contains("last_quest_time")) _data.LastQuestTime = (long)data["last_quest_time"];
        if (data.Contains("last_dungeon_time")) _data.LastDungeonTime = (long)data["last_dungeon_time"];
        if (data.Contains("last_pet_time")) _data.LastPetInteractionTime = (long)data["last_pet_time"];

        if (data.Contains("total_rewards_claimed")) _data.TotalRewardsClaimed = (int)data["total_rewards_claimed"];
        if (data.Contains("total_gold_streaks")) _data.TotalGoldFromStreaks = (int)data["total_gold_streaks"];
        if (data.Contains("total_exp_streaks")) _data.TotalExpFromStreaks = (int)data["total_exp_streaks"];
    }
    
    public void SaveData()
    {
        var saveSystem = SaveSystem.Instance;
        if (saveSystem == null) return;

        var data = saveSystem.LoadGame();
        if (data == null) data = new Godot.Dictionary();

        // Save streak data
        data["streak_login"] = _data.LoginStreak;
        data["streak_battle"] = _data.BattleStreak;
        data["streak_quest"] = _data.QuestStreak;
        data["streak_dungeon"] = _data.DungeonStreak;
        data["streak_pet"] = _data.PetInteractionStreak;

        data["best_streak_login"] = _data.BestLoginStreak;
        data["best_streak_battle"] = _data.BestBattleStreak;
        data["best_streak_quest"] = _data.BestQuestStreak;
        data["best_streak_dungeon"] = _data.BestDungeonStreak;
        data["best_streak_pet"] = _data.BestPetInteractionStreak;

        data["total_login_days"] = _data.TotalLoginDays;
        data["total_battle_days"] = _data.TotalBattleDays;
        data["total_quest_days"] = _data.TotalQuestDays;
        data["total_dungeon_days"] = _data.TotalDungeonDays;
        data["total_pet_days"] = _data.TotalPetInteractionDays;

        data["streak_freeze_tokens"] = _data.StreakFreezeTokens;
        data["total_freeze_used"] = _data.TotalStreakFreezeUsed;

        data["last_login_time"] = _data.LastLoginTime;
        data["last_battle_time"] = _data.LastBattleTime;
        data["last_quest_time"] = _data.LastQuestTime;
        data["last_dungeon_time"] = _data.LastDungeonTime;
        data["last_pet_time"] = _data.LastPetInteractionTime;

        data["total_rewards_claimed"] = _data.TotalRewardsClaimed;
        data["total_gold_streaks"] = _data.TotalGoldFromStreaks;
        data["total_exp_streaks"] = _data.TotalExpFromStreaks;

        saveSystem.SaveGame(data);
    }
    
    public StreakData GetData() => _data;
    
    public int GetStreak(StreakType type) => type switch
    {
        StreakType.Login => _data.LoginStreak,
        StreakType.Battle => _data.BattleStreak,
        StreakType.Quest => _data.QuestStreak,
        StreakType.Dungeon => _data.DungeonStreak,
        StreakType.PetInteraction => _data.PetInteractionStreak,
        _ => 0
    };
    
    public int GetBestStreak(StreakType type) => type switch
    {
        StreakType.Login => _data.BestLoginStreak,
        StreakType.Battle => _data.BestBattleStreak,
        StreakType.Quest => _data.BestQuestStreak,
        StreakType.Dungeon => _data.BestDungeonStreak,
        StreakType.PetInteraction => _data.BestPetInteractionStreak,
        _ => 0
    };
    
    public long GetLastTime(StreakType type) => type switch
    {
        StreakType.Login => _data.LastLoginTime,
        StreakType.Battle => _data.LastBattleTime,
        StreakType.Quest => _data.LastQuestTime,
        StreakType.Dungeon => _data.LastDungeonTime,
        StreakType.PetInteraction => _data.LastPetInteractionTime,
        _ => 0
    };
    
    public bool ShouldDecayStreak(StreakType type)
    {
        long lastTime = GetLastTime(type);
        if (lastTime == 0) return false;
        
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long hoursSince = (now - lastTime) / 3600;
        
        return hoursSince >= _database.StreakDecayHours;
    }
    
    public void RecordActivity(StreakType type)
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long lastTime = GetLastTime(type);
        
        // Check if streak should decay
        if (ShouldDecayStreak(type))
        {
            BreakStreak(type);
            return;
        }
        
        // Check if already claimed today (for daily rewards)
        if (IsToday(lastTime))
        {
            // Already activity today, just update time
            UpdateLastTime(type, now);
            return;
        }
        
        // Check if this is a new day (increment streak)
        if (IsYesterday(lastTime) || lastTime == 0)
        {
            IncrementStreak(type);
        }
        
        UpdateLastTime(type, now);
        OnStreakUpdated?.Invoke(type, GetStreak(type));
    }
    
    private void IncrementStreak(StreakType type)
    {
        int current = GetStreak(type);
        current++;
        
        switch (type)
        {
            case StreakType.Login:
                _data.LoginStreak = current;
                _data.TotalLoginDays++;
                if (current > _data.BestLoginStreak)
                    _data.BestLoginStreak = current;
                break;
            case StreakType.Battle:
                _data.BattleStreak = current;
                _data.TotalBattleDays++;
                if (current > _data.BestBattleStreak)
                    _data.BestBattleStreak = current;
                break;
            case StreakType.Quest:
                _data.QuestStreak = current;
                _data.TotalQuestDays++;
                if (current > _data.BestQuestStreak)
                    _data.BestQuestStreak = current;
                break;
            case StreakType.Dungeon:
                _data.DungeonStreak = current;
                _data.TotalDungeonDays++;
                if (current > _data.BestDungeonStreak)
                    _data.BestDungeonStreak = current;
                break;
            case StreakType.PetInteraction:
                _data.PetInteractionStreak = current;
                _data.TotalPetInteractionDays++;
                if (current > _data.BestPetInteractionStreak)
                    _data.BestPetInteractionStreak = current;
                break;
        }
        
        // Check for milestone
        CheckMilestone(type, current);
    }
    
    private void BreakStreak(StreakType type)
    {
        switch (type)
        {
            case StreakType.Login:
                _data.LoginStreak = 0;
                break;
            case StreakType.Battle:
                _data.BattleStreak = 0;
                break;
            case StreakType.Quest:
                _data.QuestStreak = 0;
                break;
            case StreakType.Dungeon:
                _data.DungeonStreak = 0;
                break;
            case StreakType.PetInteraction:
                _data.PetInteractionStreak = 0;
                break;
        }
        
        OnStreakBroken?.Invoke(type);
    }
    
    private void UpdateLastTime(StreakType type, long time)
    {
        switch (type)
        {
            case StreakType.Login:
                _data.LastLoginTime = time;
                break;
            case StreakType.Battle:
                _data.LastBattleTime = time;
                break;
            case StreakType.Quest:
                _data.LastQuestTime = time;
                break;
            case StreakType.Dungeon:
                _data.LastDungeonTime = time;
                break;
            case StreakType.PetInteraction:
                _data.LastPetInteractionTime = time;
                break;
        }
    }
    
    private void CheckMilestone(StreakType type, int streak)
    {
        int[] milestones = { 7, 14, 30, 60, 100 };
        foreach (var m in milestones)
        {
            if (streak == m)
            {
                OnMilestoneReached?.Invoke(type, streak);
                break;
            }
        }
    }
    
    public StreakReward ClaimReward(StreakType type)
    {
        int streak = GetStreak(type);
        if (streak <= 0) return new StreakReward();
        
        StreakReward reward = _database.GetReward(type, streak);
        
        // Apply reward
        if (reward.Gold > 0)
        {
            // Add gold (through gold system)
            _data.TotalGoldFromStreaks += reward.Gold;
        }
        
        if (reward.Exp > 0)
        {
            // Add exp (through exp system)
            _data.TotalExpFromStreaks += reward.Exp;
        }
        
        // Add items if any
        if (!string.IsNullOrEmpty(reward.ItemId) && reward.ItemCount > 0)
        {
            // Add items (through inventory system)
        }
        
        _data.TotalRewardsClaimed++;
        
        OnRewardClaimed?.Invoke(type, streak, reward);
        
        return reward;
    }
    
    public bool CanUseStreakFreeze()
    {
        return _data.StreakFreezeTokens > 0;
    }
    
    public bool UseStreakFreeze(StreakType type)
    {
        if (!CanUseStreakFreeze()) return false;
        
        _data.StreakFreezeTokens--;
        _data.TotalStreakFreezeUsed++;
        
        // Reset decay timer
        UpdateLastTime(type, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        
        return true;
    }
    
    public void PurchaseStreakFreeze()
    {
        // Can be purchased from shop
        _data.StreakFreezeTokens = Mathf.Min(
            _data.StreakFreezeTokens + 1,
            _database.MaxFreezeTokens
        );
    }
    
    public Dictionary<StreakType, StreakRecord> GetAllStreaks()
    {
        return new Dictionary<StreakType, StreakRecord>
        {
            { StreakType.Login, new StreakRecord
                {
                    Type = StreakType.Login,
                    CurrentStreak = _data.LoginStreak,
                    BestStreak = _data.BestLoginStreak,
                    TotalDays = _data.TotalLoginDays,
                    LastTime = _data.LastLoginTime,
                    ClaimedToday = IsToday(_data.LastLoginTime)
                }
            },
            { StreakType.Battle, new StreakRecord
                {
                    Type = StreakType.Battle,
                    CurrentStreak = _data.BattleStreak,
                    BestStreak = _data.BestBattleStreak,
                    TotalDays = _data.TotalBattleDays,
                    LastTime = _data.LastBattleTime,
                    ClaimedToday = IsToday(_data.LastBattleTime)
                }
            },
            { StreakType.Quest, new StreakRecord
                {
                    Type = StreakType.Quest,
                    CurrentStreak = _data.QuestStreak,
                    BestStreak = _data.BestQuestStreak,
                    TotalDays = _data.TotalQuestDays,
                    LastTime = _data.LastQuestTime,
                    ClaimedToday = IsToday(_data.LastQuestTime)
                }
            },
            { StreakType.Dungeon, new StreakRecord
                {
                    Type = StreakType.Dungeon,
                    CurrentStreak = _data.DungeonStreak,
                    BestStreak = _data.BestDungeonStreak,
                    TotalDays = _data.TotalDungeonDays,
                    LastTime = _data.LastDungeonTime,
                    ClaimedToday = IsToday(_data.LastDungeonTime)
                }
            },
            { StreakType.PetInteraction, new StreakRecord
                {
                    Type = StreakType.PetInteraction,
                    CurrentStreak = _data.PetInteractionStreak,
                    BestStreak = _data.BestPetInteractionStreak,
                    TotalDays = _data.TotalPetInteractionDays,
                    LastTime = _data.LastPetInteractionTime,
                    ClaimedToday = IsToday(_data.LastPetInteractionTime)
                }
            }
        };
    }
    
    public int GetTotalGoldFromStreaks() => _data.TotalGoldFromStreaks;
    public int GetTotalExpFromStreaks() => _data.TotalExpFromStreaks;
    public int GetTotalRewardsClaimed() => _data.TotalRewardsClaimed;
    public int GetStreakFreezeTokens() => _data.StreakFreezeTokens;
    
    // Quick activity methods
    public void OnPlayerLogin() => RecordActivity(StreakType.Login);
    public void OnBattleComplete() => RecordActivity(StreakType.Battle);
    public void OnQuestComplete() => RecordActivity(StreakType.Quest);
    public void OnDungeonComplete() => RecordActivity(StreakType.Dungeon);
    public void OnPetInteraction() => RecordActivity(StreakType.PetInteraction);
    
    // Helper methods
    private bool IsToday(long timestamp)
    {
        if (timestamp == 0) return false;
        
        var dateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        var now = DateTime.UtcNow;
        
        return dateTime.Date == now.Date;
    }
    
    private bool IsYesterday(long timestamp)
    {
        if (timestamp == 0) return true;
        
        var dateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        var yesterday = DateTime.UtcNow.AddDays(-1);
        
        return dateTime.Date == yesterday.Date;
    }
    
    public void CheckAndUpdateAllStreaks()
    {
        foreach (StreakType type in Enum.GetValues(typeof(StreakType)))
        {
            if (ShouldDecayStreak(type))
            {
                BreakStreak(type);
            }
        }
    }
}
