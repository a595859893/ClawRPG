using Godot;
using System;
using System.Collections.Generic;
using Framework;

/// <summary>
/// 骰子大师系统 - 骰子小游戏管理
/// </summary>
public partial class DiceMasterSystem : BaseSystem
{
    public static DiceMasterSystem Instance { get; private set; }
    
    // Dice types
    public enum DiceType { D4, D6, D8, D10, D12, D20, D100 }
    
    // Buff types
    public enum BuffType { Attack, Defense, Health, Speed, Critical, LifeSteal, Dodge, Magic }
    
    // Dice roll record
    public struct DiceRoll
    {
        public int roll;
        public int max;
        public DateTime timestamp;
    }
    
    // Player data
    public int TotalRolls { get; set; } = 0;
    public int TotalWins { get; set; } = 0;
    public int HighestRoll { get; set; } = 0;
    public int LuckyStreak { get; set; } = 0;
    public int UnluckyStreak { get; set; } = 0;
    public int Diamonds { get; set; } = 10; // Free daily rolls
    
    private List<DiceRoll> rollHistory = new List<DiceRoll>();
    private Random rng = new Random();
    
    // Buffs from rolling
    private Dictionary<BuffType, float> activeBuffs = new Dictionary<BuffType, float>();
    private float buffDuration = 30.0f; // seconds
    private float buffTimer = 0.0f;
    
    public override void _Ready()
    {
        Instance = this;
    }
    
    public override void _Process(double delta)
    {
        if (buffTimer > 0)
        {
            buffTimer -= delta;
            if (buffTimer <= 0)
            {
                ClearBuffs();
            }
        }
    }
    
    // Roll a dice
    public int Roll(DiceType diceType)
    {
        int maxValue = GetMaxValue(diceType);
        int roll = rng.Next(1, maxValue + 1);
        
        // Record roll
        DiceRoll record = new DiceRoll
        {
            roll = roll,
            max = maxValue,
            timestamp = DateTime.Now
        };
        rollHistory.Add(record);
        if (rollHistory.Count > 100) rollHistory.RemoveAt(0);
        
        TotalRolls++;
        
        // Track highest roll
        if (roll > HighestRoll) HighestRoll = roll;
        
        // Track streaks
        if (roll >= maxValue * 0.8f)
        {
            LuckyStreak++;
            UnluckyStreak = 0;
            TotalWins++;
        }
        else if (roll <= maxValue * 0.2f)
        {
            UnluckyStreak++;
            LuckyStreak = 0;
        }
        else
        {
            LuckyStreak = 0;
            UnluckyStreak = 0;
        }
        
        // Apply buff based on roll
        ApplyBuffFromRoll(roll, maxValue);
        
        // Signal
        EmitSignal(nameof(DiceRolled), roll, maxValue);
        
        return roll;
    }
    
    // Get max value for dice type
    public int GetMaxValue(DiceType diceType)
    {
        switch (diceType)
        {
            case DiceType.D4: return 4;
            case DiceType.D6: return 6;
            case DiceType.D8: return 8;
            case DiceType.D10: return 10;
            case DiceType.D12: return 12;
            case DiceType.D20: return 20;
            case DiceType.D100: return 100;
            default: return 6;
        }
    }
    
    // Apply buff based on roll result
    private void ApplyBuffFromRoll(int roll, int max)
    {
        float ratio = (float)roll / max;
        
        // Clear old buffs
        ClearBuffs();
        
        // Apply new buffs based on roll quality
        if (ratio >= 0.9f) // Critical success (80-100%)
        {
            // Triple buff
            BuffType type1 = (BuffType)rng.Next(8);
            BuffType type2 = (BuffType)rng.Next(8);
            BuffType type3 = (BuffType)rng.Next(8);
            
            activeBuffs[type1] = 0.30f;
            activeBuffs[type2] = 0.25f;
            activeBuffs[type3] = 0.20f;
            buffDuration = 60.0f;
        }
        else if (ratio >= 0.7f) // Great success (70-89%)
        {
            // Double buff
            BuffType type1 = (BuffType)rng.Next(8);
            BuffType type2 = (BuffType)rng.Next(8);
            
            activeBuffs[type1] = 0.25f;
            activeBuffs[type2] = 0.15f;
            buffDuration = 45.0f;
        }
        else if (ratio >= 0.5f) // Normal success (50-69%)
        {
            // Single buff
            BuffType type = (BuffType)rng.Next(8);
            activeBuffs[type] = 0.15f;
            buffDuration = 30.0f;
        }
        else if (ratio <= 0.2f) // Curse (0-20%)
        {
            // Negative buff
            BuffType type = (BuffType)rng.Next(8);
            activeBuffs[type] = -0.20f;
            buffDuration = 20.0f;
        }
        
        buffTimer = buffDuration;
    }
    
    // Get buff value
    public float GetBuff(BuffType type)
    {
        if (activeBuffs.ContainsKey(type))
            return activeBuffs[type];
        return 0.0f;
    }
    
    // Clear all buffs
    public void ClearBuffs()
    {
        activeBuffs.Clear();
    }
    
    // Check if has active buffs
    public bool HasActiveBuffs()
    {
        return activeBuffs.Count > 0;
    }
    
    // Get buff info for UI
    public Dictionary<string, float> GetActiveBuffsInfo()
    {
        Dictionary<string, float> info = new Dictionary<string, float>();
        string[] buffNames = { "Attack", "Defense", "Health", "Speed", "Critical", "LifeSteal", "Dodge", "Magic" };
        
        foreach (var kvp in activeBuffs)
        {
            info[buffNames[(int)kvp.Key]] = kvp.Value;
        }
        
        return info;
    }
    
    // Spend diamond for extra roll
    public bool SpendDiamond()
    {
        if (Diamonds > 0)
        {
            Diamonds--;
            return true;
        }
        return false;
    }
    
    // Get roll history for UI
    public List<DiceRoll> GetRollHistory(int count = 10)
    {
        if (rollHistory.Count <= count)
            return new List<DiceRoll>(rollHistory);
        
        List<DiceRoll> result = new List<DiceRoll>();
        for (int i = rollHistory.Count - count; i < rollHistory.Count; i++)
            result.Add(rollHistory[i]);
        return result;
    }
    
    // Get statistics
    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            { "total_rolls", TotalRolls },
            { "total_wins", TotalWins },
            { "highest_roll", HighestRoll },
            { "lucky_streak", LuckyStreak },
            { "unlucky_streak", UnluckyStreak },
            { "diamonds", Diamonds }
        };
    }
    
    // 持久化支持
    public override Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary
        {
            { "total_rolls", TotalRolls },
            { "total_wins", TotalWins },
            { "highest_roll", HighestRoll },
            { "lucky_streak", LuckyStreak },
            { "unlucky_streak", UnluckyStreak },
            { "diamonds", Diamonds }
        };
    }
    
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        TotalRolls = data.ContainsKey("total_rolls") ? Convert.ToInt32(data["total_rolls"]) : 0;
        TotalWins = data.ContainsKey("total_wins") ? Convert.ToInt32(data["total_wins"]) : 0;
        HighestRoll = data.ContainsKey("highest_roll") ? Convert.ToInt32(data["highest_roll"]) : 0;
        LuckyStreak = data.ContainsKey("lucky_streak") ? Convert.ToInt32(data["lucky_streak"]) : 0;
        UnluckyStreak = data.ContainsKey("unlucky_streak") ? Convert.ToInt32(data["unlucky_streak"]) : 0;
        Diamonds = data.ContainsKey("diamonds") ? Convert.ToInt32(data["diamonds"]) : 10;
    }
    
    // 旧的存档方法（保留兼容性）
    public Dictionary<string, object> SaveData()
    {
        return ExportSaveData();
    }
    
    public void LoadData(Dictionary<string, object> data)
    {
        ImportSaveData(new Dictionary(data));
    }
    
    // Signal
public delegate void DiceRolled(int roll, int maxValue);
}
