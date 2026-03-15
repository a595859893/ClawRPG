using Godot;
using System;
using System.Collections.Generic;

public class SkillTreeResetData
{
    // Singleton
    private static SkillTreeResetData _instance;
    public static SkillTreeResetData Instance => _instance ??= new SkillTreeResetData();
    
    // Data
    public int TotalResets { get; set; }
    public int FreeResetsRemaining { get; set; }
    public List<SkillTreeResetRecord> ResetHistory { get; set; }
    public Dictionary<string, int> SkillPointsSpentAtReset { get; set; }
    public DateTime LastResetTime { get; set; }
    
    // Statistics
    public int TotalPointsRecovered { get; set; }
    public int TotalGoldSpent { get; set; }
    public int MaxPointsInSingleReset { get; set; }
    
    public SkillTreeResetData()
    {
        ResetHistory = new List<SkillTreeResetRecord>();
        SkillPointsSpentAtReset = new Dictionary<string, int>();
        FreeResetsRemaining = 1; // First reset is free
        TotalResets = 0;
        TotalPointsRecovered = 0;
        TotalGoldSpent = 0;
        MaxPointsInSingleReset = 0;
        LastResetTime = DateTime.MinValue;
    }
    
    public void Reset()
    {
        TotalResets = 0;
        FreeResetsRemaining = 1;
        ResetHistory.Clear();
        SkillPointsSpentAtReset.Clear();
        TotalPointsRecovered = 0;
        TotalGoldSpent = 0;
        MaxPointsInSingleReset = 0;
        LastResetTime = DateTime.MinValue;
    }

    public override Dictionary ExportSaveData() => new();
    public override void ImportSaveData(Dictionary data) { }

}

public class SkillTreeResetRecord
{
    public DateTime ResetTime { get; set; }
    public int PointsReset { get; set; }
    public int PointsRecovered { get; set; }
    public int GoldSpent { get; set; }
    public string ResetType { get; set; } // "Free" or "Paid"
}

public class SkillTreeResetDatabase
{
    private static SkillTreeResetDatabase _instance;
    public static SkillTreeResetDatabase Instance => _instance ??= new SkillTreeResetDatabase();
    
    // Configuration
    public int FreeResetsPerLevel { get; private set; }
    public float RefundPercentage { get; private set; }
    public Dictionary<string, int> BaseCostByLevel { get; private set; }
    public Dictionary<string, float> CostMultiplierByRarity { get; private set; }
    public int MaxPaidResetsPerDay { get; private set; }
    public int BonusFreeResetsOnLevelUp { get; private set; }
    
    public SkillTreeResetDatabase()
    {
        FreeResetsPerLevel = 1;
        RefundPercentage = 0.75f; // 75% refund
        MaxPaidResetsPerDay = 3;
        BonusFreeResetsOnLevelUp = 1;
        
        BaseCostByLevel = new Dictionary<string, int>
        {
            ["first"] = 0,      // First reset is free
            ["second"] = 100,   // 100 gold
            ["third"] = 250,    // 250 gold
            ["fourth"] = 500,   // 500 gold
            ["fifth"] = 1000,   // 1000 gold
            ["sixth"] = 2000,   // 2000 gold
            ["seventh"] = 4000, // 4000 gold
            ["eighth"] = 8000,  // 8000 gold
            ["ninth"] = 16000,  // 16000 gold
            ["tenth_plus"] = 32000  // 32000 gold and beyond
        };
        
        CostMultiplierByRarity = new Dictionary<string, float>
        {
            ["Common"] = 1.0f,
            ["Uncommon"] = 1.5f,
            ["Rare"] = 2.0f,
            ["Epic"] = 3.0f,
            ["Legendary"] = 5.0f
        };
    }
    
    public int GetResetCost(int resetNumber)
    {
        if (resetNumber <= 0) return 0;
        
        if (resetNumber == 1) return BaseCostByLevel["first"];
        if (resetNumber == 2) return BaseCostByLevel["second"];
        if (resetNumber == 3) return BaseCostByLevel["third"];
        if (resetNumber == 4) return BaseCostByLevel["fourth"];
        if (resetNumber == 5) return BaseCostByLevel["fifth"];
        if (resetNumber == 6) return BaseCostByLevel["sixth"];
        if (resetNumber == 7) return BaseCostByLevel["seventh"];
        if (resetNumber == 8) return BaseCostByLevel["eighth"];
        if (resetNumber == 9) return BaseCostByLevel["ninth"];
        return BaseCostByLevel["tenth_plus"];
    }
    
    public int CalculateRefundPoints(int totalPointsSpent)
    {
        return (int)(totalPointsSpent * RefundPercentage);
    }
}

public partial class SkillTreeResetSystem : BaseSystem
{
    private static SkillTreeResetSystem _instance;
    public static SkillTreeResetSystem Instance => _instance;
    
    [Export] private int _playerGold = 10000;
    [Export] private int _playerLevel = 1;
    [Export] private int _availableSkillPoints = 10;
    [Export] private int _totalSkillPointsEarned = 10;
    
    // For demo purposes - tracks spent points in each category
    private Dictionary<string, int> _categoryPointsSpent;
    
    public override void _Ready()
    {
        _instance = this;
        _categoryPointsSpent = new Dictionary<string, int>
        {
            ["combat"] = 3,
            ["defense"] = 2,
            ["magic"] = 1,
            ["utility"] = 2,
            ["special"] = 2
        };
        
        GD.Print("=== Skill Tree Reset System Ready ===");
        GD.Print($"Player Gold: {_playerGold}");
        GD.Print($"Player Level: {_playerLevel}");
        GD.Print($"Available Points: {_availableSkillPoints}");
        GD.Print($"Category Points: {string.Join(", ", _categoryPointsSpent)}");
    }
    
    public bool CanResetSkillTree()
    {
        var data = SkillTreeResetData.Instance;
        
        // Check free resets
        if (data.FreeResetsRemaining > 0) return true;
        
        // Check paid resets
        int paidResetsToday = GetPaidResetsToday();
        return paidResetsToday < SkillTreeResetDatabase.Instance.MaxPaidResetsPerDay;
    }
    
    public int GetResetCost()
    {
        var data = SkillTreeResetData.Instance;
        
        // Free reset available
        if (data.FreeResetsRemaining > 0) return 0;
        
        // Calculate cost based on total resets
        return SkillTreeResetDatabase.Instance.GetResetCost(data.TotalResets + 1);
    }
    
    public SkillTreeResetResult ResetSkillTree(bool useFreeReset = true)
    {
        var data = SkillTreeResetData.Instance;
        var db = SkillTreeResetDatabase.Instance;
        
        var result = new SkillTreeResetResult();
        
        // Check if reset is possible
        if (!CanResetSkillTree())
        {
            result.Success = false;
            result.ErrorMessage = "No resets available today";
            return result;
        }
        
        // Calculate cost
        int cost = GetResetCost();
        
        // Check gold for paid reset
        if (cost > 0 && _playerGold < cost)
        {
            result.Success = false;
            result.ErrorMessage = $"Not enough gold. Need {cost} gold, have {_playerGold}";
            return result;
        }
        
        // Calculate points to recover
        int totalSpent = GetTotalPointsSpent();
        int pointsToRecover = db.CalculateRefundPoints(totalSpent);
        
        // Execute reset
        if (useFreeReset && data.FreeResetsRemaining > 0)
        {
            data.FreeResetsRemaining--;
            result.ResetType = "Free";
        }
        else
        {
            _playerGold -= cost;
            data.TotalGoldSpent += cost;
            result.ResetType = "Paid";
            result.GoldSpent = cost;
        }
        
        // Update data
        data.TotalResets++;
        data.TotalPointsRecovered += pointsToRecover;
        data.LastResetTime = DateTime.Now;
        
        if (totalSpent > data.MaxPointsInSingleReset)
        {
            data.MaxPointsInSingleReset = totalSpent;
        }
        
        // Record history
        var record = new SkillTreeResetRecord
        {
            ResetTime = DateTime.Now,
            PointsReset = totalSpent,
            PointsRecovered = pointsToRecover,
            GoldSpent = cost,
            ResetType = result.ResetType
        };
        data.ResetHistory.Add(record);
        
        // Reset skill points
        _availableSkillPoints += pointsToRecover;
        
        // Reset category points (for demo)
        foreach (var key in _categoryPointsSpent.Keys)
        {
            _categoryPointsSpent[key] = 0;
        }
        
        // Set result
        result.Success = true;
        result.PointsRecovered = pointsToRecover;
        result.TotalResets = data.TotalResets;
        result.FreeResetsRemaining = data.FreeResetsRemaining;
        
        GD.Print($"=== Skill Tree Reset ===");
        GD.Print($"Type: {result.ResetType}");
        GD.Print($"Points Recovered: {pointsToRecover}");
        GD.Print($"Remaining Free Resets: {data.FreeResetsRemaining}");
        
        return result;
    }
    
    public void OnPlayerLevelUp(int newLevel)
    {
        _playerLevel = newLevel;
        
        // Grant free reset on level up
        if (newLevel % 5 == 0) // Every 5 levels
        {
            SkillTreeResetData.Instance.FreeResetsRemaining += SkillTreeResetDatabase.Instance.BonusFreeResetsOnLevelUp;
            GD.Print($"Level up! Granted free skill tree reset. Total free resets: {SkillTreeResetData.Instance.FreeResetsRemaining}");
        }
    }
    
    public SkillTreeResetStatistics GetStatistics()
    {
        var data = SkillTreeResetData.Instance;
        
        return new SkillTreeResetStatistics
        {
            TotalResets = data.TotalResets,
            FreeResetsUsed = data.TotalResets - GetPaidResetsTotal(),
            PaidResetsUsed = GetPaidResetsTotal(),
            FreeResetsRemaining = data.FreeResetsRemaining,
            TotalPointsRecovered = data.TotalPointsRecovered,
            TotalGoldSpent = data.TotalGoldSpent,
            MaxPointsInSingleReset = data.MaxPointsInSingleReset,
            AveragePointsPerReset = data.TotalResets > 0 ? data.TotalPointsRecovered / data.TotalResets : 0,
            RefundPercentage = SkillTreeResetDatabase.Instance.RefundPercentage * 100
        };
    }
    
    public Dictionary<string, int> GetCategoryPointsSpent()
    {
        return new Dictionary<string, int>(_categoryPointsSpent);
    }
    
    public void SpendPoints(string category, int points)
    {
        if (_categoryPointsSpent.ContainsKey(category))
        {
            _categoryPointsSpent[category] += points;
            _availableSkillPoints -= points;
        }
    }
    
    public bool CanAffordReset()
    {
        return _playerGold >= GetResetCost() || SkillTreeResetData.Instance.FreeResetsRemaining > 0;
    }
    
    private int GetTotalPointsSpent()
    {
        int total = 0;
        foreach (var points in _categoryPointsSpent.Values)
        {
            total += points;
        }
        return total;
    }
    
    private int GetPaidResetsToday()
    {
        var today = DateTime.Today;
        int count = 0;
        
        foreach (var record in SkillTreeResetData.Instance.ResetHistory)
        {
            if (record.ResetTime.Date == today && record.ResetType == "Paid")
            {
                count++;
            }
        }
        
        return count;
    }
    
    private int GetPaidResetsTotal()
    {
        int count = 0;
        
        foreach (var record in SkillTreeResetData.Instance.ResetHistory)
        {
            if (record.ResetType == "Paid")
            {
                count++;
            }
        }
        
        return count;
    }
}

public class SkillTreeResetResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public string ResetType { get; set; } // "Free" or "Paid"
    public int PointsRecovered { get; set; }
    public int GoldSpent { get; set; }
    public int TotalResets { get; set; }
    public int FreeResetsRemaining { get; set; }
}

public class SkillTreeResetStatistics
{
    public int TotalResets { get; set; }
    public int FreeResetsUsed { get; set; }
    public int PaidResetsUsed { get; set; }
    public int FreeResetsRemaining { get; set; }
    public int TotalPointsRecovered { get; set; }
    public int TotalGoldSpent { get; set; }
    public int MaxPointsInSingleReset { get; set; }
    public int AveragePointsPerReset { get; set; }
    public float RefundPercentage { get; set; }
}

public partial class SkillTreeResetSystem
{
    public override Dictionary ExportSaveData() => new();
    public override void ImportSaveData(Dictionary data) { }
}
