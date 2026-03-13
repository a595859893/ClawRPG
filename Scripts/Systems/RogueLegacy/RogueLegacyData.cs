using Godot;
using System.Collections.Generic;

public class RogueLegacyData : GodotObject
{
    // 继承资源追踪
    private int _goldInheritancePercent = 25;
    private int _experienceInheritancePercent = 50;
    private int _itemInheritancePercent = 30;
    
    // 传承点数系统
    private int _legacyPoints = 0;
    private int _totalLegacyPointsEarned = 0;
    private int _totalLegacyPointsSpent = 0;
    
    // 死亡次数追踪
    private int _totalDeaths = 0;
    private int _runsCompleted = 0;
    
    // 已解锁传承升级
    private List<string> _unlockedUpgrades = new List<string>();
    
    // 传承历史记录
    private List<LegacyRunRecord> _runHistory = new List<LegacyRunRecord>();
    
    // 统计追踪
    private int _highestGoldInherited = 0;
    private int _highestExpInherited = 0;
    private int _longestRun = 0;
    private int _bestFloor = 0;
    
    // 属性加成
    private int _bonusAttack = 0;
    private int _bonusDefense = 0;
    private int _bonusHealth = 0;
    private int _bonusSpeed = 0;
    private int _bonusCritical = 0;
    
    public int GoldInheritancePercent
    {
        get => _goldInheritancePercent;
        set => _goldInheritancePercent = value;
    }
    
    public int ExperienceInheritancePercent
    {
        get => _experienceInheritancePercent;
        set => _experienceInheritancePercent = value;
    }
    
    public int ItemInheritancePercent
    {
        get => _itemInheritancePercent;
        set => _itemInheritancePercent = value;
    }
    
    public int LegacyPoints
    {
        get => _legacyPoints;
        set => _legacyPoints = value;
    }
    
    public int TotalLegacyPointsEarned
    {
        get => _totalLegacyPointsEarned;
        set => _totalLegacyPointsEarned = value;
    }
    
    public int TotalLegacyPointsSpent
    {
        get => _totalLegacyPointsSpent;
        set => _totalLegacyPointsSpent = value;
    }
    
    public int TotalDeaths
    {
        get => _totalDeaths;
        set => _totalDeaths = value;
    }
    
    public int RunsCompleted
    {
        get => _runsCompleted;
        set => _runsCompleted = value;
    }
    
    public List<string> UnlockedUpgrades
    {
        get => _unlockedUpgrades;
        set => _unlockedUpgrades = value;
    }
    
    public List<LegacyRunRecord> RunHistory
    {
        get => _runHistory;
        set => _runHistory = value;
    }
    
    public int HighestGoldInherited
    {
        get => _highestGoldInherited;
        set => _highestGoldInherited = value;
    }
    
    public int HighestExpInherited
    {
        get => _highestExpInherited;
        set => _highestExpInherited = value;
    }
    
    public int LongestRun
    {
        get => _longestRun;
        set => _longestRun = value;
    }
    
    public int BestFloor
    {
        get => _bestFloor;
        set => _bestFloor = value;
    }
    
    public int BonusAttack
    {
        get => _bonusAttack;
        set => _bonusAttack = value;
    }
    
    public int BonusDefense
    {
        get => _bonusDefense;
        set => _bonusDefense = value;
    }
    
    public int BonusHealth
    {
        get => _bonusHealth;
        set => _bonusHealth = value;
    }
    
    public int BonusSpeed
    {
        get => _bonusSpeed;
        set => _bonusSpeed = value;
    }
    
    public int BonusCritical
    {
        get => _bonusCritical;
        set => _bonusCritical = value;
    }
    
    // 存档数据转换
    public Dictionary<string, object> ToDict()
    {
        return new Dictionary<string, object>
        {
        };
    }
    
    public void FromDict(Dictionary<string, object> data)
    {
    }
}

public class LegacyRunRecord
{
    public int RunNumber { get; set; }
    public int FloorReached { get; set; }
    public int GoldEarned { get; set; }
    public int ExperienceGained { get; set; }
    public int GoldInherited { get; set; }
    public int ExpInherited { get; set; }
    public int LegacyPointsEarned { get; set; }
    public bool Completed { get; set; }
    public long Timestamp { get; set; }
}

public class RogueLegacyInheritanceBonus
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Cost { get; set; }
    public int MaxLevel { get; set; }
    public int CurrentLevel { get; set; }
    public string BonusType { get; set; }
    public float BonusValue { get; set; }
}
