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
        // TODO: Load from file
    }
    
    public void SaveData()
    {
        // TODO: Save to file
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
