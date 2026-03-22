using Godot;
using System.Collections.Generic;

public class RogueLegacyDatabase : Object
{
    // 继承百分比配置
    private Dictionary<string, int> _baseInheritanceConfig = new Dictionary<string, int>
    {
        { "GoldInheritance", 25 },
        { "ExperienceInheritance", 50 },
        { "ItemInheritance", 30 }
    };
    
    // 传承升级配置
    private List<InheritanceUpgrade> _upgrades = new List<InheritanceUpgrade>();
    
    // 传承点数获取配置
    private int _basePointsPerRun = 10;
    private int _pointsPerFloor = 1;
    private int _pointsPerBoss = 5;
    private int _completionBonus = 25;
    
    // 传承加成配置
    private Dictionary<string, float> _attributeBonusPerLevel = new Dictionary<string, float>
    {
        { "Attack", 2.0f },
        { "Defense", 2.0f },
        { "Health", 10.0f },
        { "Speed", 1.0f },
        { "Critical", 0.5f }
    };
    
    public Dictionary<string, int> BaseInheritanceConfig => _baseInheritanceConfig;
    public List<InheritanceUpgrade> Upgrades => _upgrades;
    public int BasePointsPerRun => _basePointsPerRun;
    public int PointsPerFloor => _pointsPerFloor;
    public int PointsPerBoss => _pointsPerBoss;
    public int CompletionBonus => _completionBonus;
    public Dictionary<string, float> AttributeBonusPerLevel => _attributeBonusPerLevel;
    
    public RogueLegacyDatabase()
    {
        InitializeUpgrades();
    }
    
    private void InitializeUpgrades()
    {
        // 继承百分比升级
        _upgrades.Add(new InheritanceUpgrade
        {
            Id = "gold_inheritance_1",
            Name = "Gold Hoarder I",
            Description = "Increase gold inheritance by 10%",
            Category = "Inheritance",
            MaxLevel = 5,
            BaseCost = 50,
            CostScaling = 1.5f,
            BonusType = "GoldInheritance",
            BonusValue = 10.0f
        });
        
        _upgrades.Add(new InheritanceUpgrade
        {
            Id = "exp_inheritance_1",
            Name = "Wisdom Keeper I",
            Description = "Increase experience inheritance by 10%",
            Category = "Inheritance",
            MaxLevel = 5,
            BaseCost = 50,
            CostScaling = 1.5f,
            BonusType = "ExperienceInheritance",
            BonusValue = 10.0f
        });
        
        _upgrades.Add(new InheritanceUpgrade
        {
            Id = "item_inheritance_1",
            Name = "Treasure Hunter I",
            Description = "Increase item inheritance by 10%",
            Category = "Inheritance",
            MaxLevel = 5,
            BaseCost = 75,
            CostScaling = 1.5f,
            BonusType = "ItemInheritance",
            BonusValue = 10.0f
        });
        
        // 属性加成升级
        _upgrades.Add(new InheritanceUpgrade
        {
            Id = "attack_boost",
            Name = "Warrior's Strength",
            Description = "Permanent attack bonus from legacy points",
            Category = "Attribute",
            MaxLevel = 10,
            BaseCost = 100,
            CostScaling = 2.0f,
            BonusType = "Attack",
            BonusValue = 5.0f
        });
        
        _upgrades.Add(new InheritanceUpgrade
        {
            Id = "defense_boost",
            Name = "Guardian's Shield",
            Description = "Permanent defense bonus from legacy points",
            Category = "Attribute",
            MaxLevel = 10,
            BaseCost = 100,
            CostScaling = 2.0f,
            BonusType = "Defense",
            BonusValue = 5.0f
        });
        
        _upgrades.Add(new InheritanceUpgrade
        {
            Id = "health_boost",
            Name = "Vitality Master",
            Description = "Permanent health bonus from legacy points",
            Category = "Attribute",
            MaxLevel = 10,
            BaseCost = 100,
            CostScaling = 2.0f,
            BonusType = "Health",
            BonusValue = 25.0f
        });
        
        _upgrades.Add(new InheritanceUpgrade
        {
            Id = "speed_boost",
            Name = "Swift Legacy",
            Description = "Permanent speed bonus from legacy points",
            Category = "Attribute",
            MaxLevel = 10,
            BaseCost = 100,
            CostScaling = 2.0f,
            BonusType = "Speed",
            BonusValue = 2.0f
        });
        
        _upgrades.Add(new InheritanceUpgrade
        {
            Id = "critical_boost",
            Name = "Critical Fate",
            Description = "Permanent critical chance bonus from legacy points",
            Category = "Attribute",
            MaxLevel = 10,
            BaseCost = 150,
            CostScaling = 2.0f,
            BonusType = "Critical",
            BonusValue = 1.0f
        });
        
        // 特殊能力升级
        _upgrades.Add(new InheritanceUpgrade
        {
            Id = "starting_gold",
            Name = "Golden Start",
            Description = "Start each run with bonus gold",
            Category = "Special",
            MaxLevel = 5,
            BaseCost = 100,
            CostScaling = 2.0f,
            BonusType = "StartingGold",
            BonusValue = 100.0f
        });
        
        _upgrades.Add(new InheritanceUpgrade
        {
            Id = "enemy_scaling",
            Name = "Monster Weakness",
            Description = "Enemies start with reduced health",
            Category = "Special",
            MaxLevel = 3,
            BaseCost = 200,
            CostScaling = 2.5f,
            BonusType = "EnemyScaling",
            BonusValue = -5.0f
        });
        
        _upgrades.Add(new InheritanceUpgrade
        {
            Id = "luck_boost",
            Name = "Fortune's Blessing",
            Description = "Increased luck in runs",
            Category = "Special",
            MaxLevel = 5,
            BaseCost = 150,
            CostScaling = 2.0f,
            BonusType = "Luck",
            BonusValue = 5.0f
        });
    }
    
    public InheritanceUpgrade GetUpgrade(string id)
    {
        foreach (var upgrade in _upgrades)
        {
            if (upgrade.Id == id)
                return upgrade;
        }
        return null;
    }
    
    public List<InheritanceUpgrade> GetUpgradesByCategory(string category)
    {
        List<InheritanceUpgrade> result = new List<InheritanceUpgrade>();
        foreach (var upgrade in _upgrades)
        {
            if (upgrade.Category == category)
                result.Add(upgrade);
        }
        return result;
    }
    
    public int GetUpgradeCost(InheritanceUpgrade upgrade, int currentLevel)
    {
        return (int)(upgrade.BaseCost * Mathf.Pow(upgrade.CostScaling, currentLevel));
    }
}

public class InheritanceUpgrade
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public int MaxLevel { get; set; }
    public int BaseCost { get; set; }
    public float CostScaling { get; set; }
    public string BonusType { get; set; }
    public float BonusValue { get; set; }
}
