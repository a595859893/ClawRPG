using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Data;

public class MountTrainingDatabase
{
    public static MountTrainingDatabase Instance { get; private set; }
    
    private Dictionary<string, TrainingProject> trainingProjects = new Dictionary<string, TrainingProject>();
    private Dictionary<string, List<string>> projectsByCategory = new Dictionary<string, List<string>>();
    private Dictionary<string, List<string>> projectsByLevel = new Dictionary<string, List<string>>();
    
    // Bond level rewards
    public Dictionary<int, BondLevelReward> BondLevelRewards = new Dictionary<int, BondLevelReward>();
    
    public MountTrainingDatabase()
    {
        Instance = this;
        InitializeTrainingProjects();
        InitializeBondRewards();
    }
    
    private void InitializeTrainingProjects()
    {
        // Combat Training Projects
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "combat_basic",
            ProjectName = "Basic Combat Training",
            Description = "Basic combat exercises to improve battle performance",
            Category = TrainingCategory.Combat,
            RequiredLevel = 1,
            DurationMinutes = 30,
            ExperienceReward = 100,
            BondPointsReward = 10,
            AttributeRewards = new Dictionary<string, int>
            {
                { "Attack", 5 },
                { "Defense", 3 }
            },
            DailyLimit = 3
        });
        
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "combat_advanced",
            ProjectName = "Advanced Combat Training",
            Description = "Advanced combat techniques for experienced mounts",
            Category = TrainingCategory.Combat,
            RequiredLevel = 10,
            DurationMinutes = 60,
            ExperienceReward = 250,
            BondPointsReward = 25,
            AttributeRewards = new Dictionary<string, int>
            {
                { "Attack", 12 },
                { "Defense", 8 },
                { "Critical", 2 }
            },
            DailyLimit = 2
        });
        
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "combat_master",
            ProjectName = "Master Combat Training",
            Description = "Master-level combat training unlocking ultimate abilities",
            Category = TrainingCategory.Combat,
            RequiredLevel = 25,
            DurationMinutes = 120,
            ExperienceReward = 500,
            BondPointsReward = 50,
            AttributeRewards = new Dictionary<string, int>
            {
                { "Attack", 25 },
                { "Defense", 15 },
                { "Critical", 5 },
                { "LifeSteal", 3 }
            },
            DailyLimit = 1
        });
        
        // Speed Training Projects
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "speed_basic",
            ProjectName = "Basic Speed Training",
            Description = "Improve running speed and agility",
            Category = TrainingCategory.Speed,
            RequiredLevel = 1,
            DurationMinutes = 20,
            ExperienceReward = 80,
            BondPointsReward = 8,
            AttributeRewards = new Dictionary<string, int>
            {
                { "Speed", 8 }
            },
            DailyLimit = 3
        });
        
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "speed_advanced",
            ProjectName = "Advanced Speed Training",
            Description = "Advanced speed and evasion training",
            Category = TrainingCategory.Speed,
            RequiredLevel = 15,
            DurationMinutes = 45,
            ExperienceReward = 200,
            BondPointsReward = 20,
            AttributeRewards = new Dictionary<string, int>
            {
                { "Speed", 15 },
                { "Evasion", 5 }
            },
            DailyLimit = 2
        });
        
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "speed_master",
            ProjectName = "Lightning Dash",
            Description = "Master speed training achieving light-speed movement",
            Category = TrainingCategory.Speed,
            RequiredLevel = 30,
            DurationMinutes = 90,
            ExperienceReward = 450,
            BondPointsReward = 45,
            AttributeRewards = new Dictionary<string, int>
            {
                { "Speed", 30 },
                { "Evasion", 10 },
                { "AttackSpeed", 5 }
            },
            DailyLimit = 1
        });
        
        // Stamina Training Projects
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "stamina_basic",
            ProjectName = "Basic Endurance Training",
            Description = "Build up stamina and health",
            Category = TrainingCategory.Stamina,
            RequiredLevel = 1,
            DurationMinutes = 25,
            ExperienceReward = 90,
            BondPointsReward = 9,
            AttributeRewards = new Dictionary<string, int>
            {
                { "Health", 20 }
            },
            DailyLimit = 3
        });
        
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "stamina_advanced",
            ProjectName = "Advanced Endurance Training",
            Description = "Advanced stamina conditioning",
            Category = TrainingCategory.Stamina,
            RequiredLevel = 12,
            DurationMinutes = 50,
            ExperienceReward = 220,
            BondPointsReward = 22,
            AttributeRewards = new Dictionary<string, int>
            {
                { "Health", 45 },
                { "Defense", 8 }
            },
            DailyLimit = 2
        });
        
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "stamina_master",
            ProjectName = "Iron Body Training",
            Description = "Master-level stamina training for indomitable spirit",
            Category = TrainingCategory.Stamina,
            RequiredLevel = 28,
            DurationMinutes = 100,
            ExperienceReward = 480,
            BondPointsReward = 48,
            AttributeRewards = new Dictionary<string, int>
            {
                { "Health", 100 },
                { "Defense", 20 },
                { "MagicDefense", 15 }
            },
            DailyLimit = 1
        });
        
        // Intelligence Training Projects
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "intelligence_basic",
            ProjectName = "Basic Intelligence Training",
            Description = "Improve mount's understanding and learning ability",
            Category = TrainingCategory.Intelligence,
            RequiredLevel = 5,
            DurationMinutes = 35,
            ExperienceReward = 120,
            BondPointsReward = 12,
            AttributeRewards = new Dictionary<string, int>
            {
                { "Magic", 10 }
            },
            DailyLimit = 2
        });
        
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "intelligence_advanced",
            ProjectName = "Advanced Intelligence Training",
            Description = "Unlock hidden potential and special abilities",
            Category = TrainingCategory.Intelligence,
            RequiredLevel = 18,
            DurationMinutes = 70,
            ExperienceReward = 280,
            BondPointsReward = 28,
            AttributeRewards = new Dictionary<string, int>
            {
                { "Magic", 25 },
                { "MagicDefense", 10 }
            },
            DailyLimit = 1
        });
        
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "intelligence_master",
            ProjectName = "Wisdom of the Ancients",
            Description = "Master intelligence training unlocking ancient wisdom",
            Category = TrainingCategory.Intelligence,
            RequiredLevel = 32,
            DurationMinutes = 140,
            ExperienceReward = 550,
            BondPointsReward = 55,
            AttributeRewards = new Dictionary<string, int>
            {
                { "Magic", 40 },
                { "MagicDefense", 20 },
                { "Luck", 15 }
            },
            DailyLimit = 1
        });
        
        // Bonding Training Projects
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "bonding_walk",
            ProjectName = "Evening Walk",
            Description = "A peaceful walk to strengthen your bond",
            Category = TrainingCategory.Bonding,
            RequiredLevel = 1,
            DurationMinutes = 15,
            ExperienceReward = 50,
            BondPointsReward = 15,
            DailyLimit = 5
        });
        
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "bonding_play",
            ProjectName = "Playtime",
            Description = "Play together to increase trust",
            Category = TrainingCategory.Bonding,
            RequiredLevel = 3,
            DurationMinutes = 20,
            ExperienceReward = 70,
            BondPointsReward = 20,
            DailyLimit = 4
        });
        
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "bonding_feast",
            ProjectName = "Shared Feast",
            Description = "Share a meal to deepen your bond",
            Category = TrainingCategory.Bonding,
            RequiredLevel = 8,
            DurationMinutes = 30,
            ExperienceReward = 100,
            BondPointsReward = 30,
            DailyLimit = 2
        });
        
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "bonding_adventure",
            ProjectName = "Adventure Together",
            Description = "Face challenges together to forge an unbreakable bond",
            Category = TrainingCategory.Bonding,
            RequiredLevel = 20,
            DurationMinutes = 60,
            ExperienceReward = 200,
            BondPointsReward = 50,
            DailyLimit = 1
        });
        
        // Special Training Projects
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "special_fire",
            ProjectName = "Fire Resistance Training",
            Description = "Train to resist fire damage",
            Category = TrainingCategory.Special,
            RequiredLevel = 10,
            DurationMinutes = 45,
            ExperienceReward = 180,
            BondPointsReward = 18,
            AttributeRewards = new Dictionary<string, int>
            {
                { "FireResistance", 15 }
            },
            DailyLimit = 2
        });
        
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "special_ice",
            ProjectName = "Ice Resistance Training",
            Description = "Train to resist ice damage",
            Category = TrainingCategory.Special,
            RequiredLevel = 10,
            DurationMinutes = 45,
            ExperienceReward = 180,
            BondPointsReward = 18,
            AttributeRewards = new Dictionary<string, int>
            {
                { "IceResistance", 15 }
            },
            DailyLimit = 2
        });
        
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "special_lightning",
            ProjectName = "Lightning Agility",
            Description = "Train to react with lightning speed",
            Category = TrainingCategory.Special,
            RequiredLevel = 15,
            DurationMinutes = 55,
            ExperienceReward = 220,
            BondPointsReward = 22,
            AttributeRewards = new Dictionary<string, int>
            {
                { "LightningResistance", 15 },
                { "Speed", 10 }
            },
            DailyLimit = 2
        });
        
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "special_holy",
            ProjectName = "Holy Blessing",
            Description = "Receive holy blessing for divine protection",
            Category = TrainingCategory.Special,
            RequiredLevel = 25,
            DurationMinutes = 80,
            ExperienceReward = 350,
            BondPointsReward = 35,
            AttributeRewards = new Dictionary<string, int>
            {
                { "HolyResistance", 20 },
                { "Health", 30 }
            },
            DailyLimit = 1
        });
        
        AddTrainingProject(new TrainingProject
        {
            ProjectId = "special_shadow",
            ProjectName = "Shadow Mastery",
            Description = "Master shadow abilities for stealth and power",
            Category = TrainingCategory.Special,
            RequiredLevel = 30,
            DurationMinutes = 100,
            ExperienceReward = 420,
            BondPointsReward = 42,
            AttributeRewards = new Dictionary<string, int>
            {
                { "DarkResistance", 20 },
                { "Critical", 8 },
                { "Evasion", 8 }
            },
            DailyLimit = 1
        });
    }
    
    private void AddTrainingProject(TrainingProject project)
    {
        trainingProjects[project.ProjectId] = project;
        
        string categoryKey = project.Category.ToString();
        if (!projectsByCategory.ContainsKey(categoryKey))
            projectsByCategory[categoryKey] = new List<string>();
        projectsByCategory[categoryKey].Add(project.ProjectId);
        
        string levelKey = "Level" + project.RequiredLevel;
        if (!projectsByLevel.ContainsKey(levelKey))
            projectsByLevel[levelKey] = new List<string>();
        projectsByLevel[levelKey].Add(project.ProjectId);
    }
    
    private void InitializeBondRewards()
    {
        BondLevelRewards[1] = new BondLevelReward { Level = 1, BonusMultiplier = 1.0f, UnlocksSpecial = false };
        BondLevelRewards[2] = new BondLevelReward { Level = 2, BonusMultiplier = 1.05f, UnlocksSpecial = false };
        BondLevelRewards[3] = new BondLevelReward { Level = 3, BonusMultiplier = 1.1f, UnlocksSpecial = false };
        BondLevelRewards[4] = new BondLevelReward { Level = 4, BonusMultiplier = 1.15f, UnlocksSpecial = false };
        BondLevelRewards[5] = new BondLevelReward { Level = 5, BonusMultiplier = 1.2f, UnlocksSpecial = true, SpecialUnlock = "BondSkill_HealthRegen" };
        BondLevelRewards[6] = new BondLevelReward { Level = 6, BonusMultiplier = 1.25f, UnlocksSpecial = false };
        BondLevelRewards[7] = new BondLevelReward { Level = 7, BonusMultiplier = 1.3f, UnlocksSpecial = false };
        BondLevelRewards[8] = new BondLevelReward { Level = 8, BonusMultiplier = 1.35f, UnlocksSpecial = false };
        BondLevelRewards[9] = new BondLevelReward { Level = 9, BonusMultiplier = 1.4f, UnlocksSpecial = false };
        BondLevelRewards[10] = new BondLevelReward { Level = 10, BonusMultiplier = 1.5f, UnlocksSpecial = true, SpecialUnlock = "BondSkill_Sprint" };
    }
    
    public TrainingProject GetTrainingProject(string projectId)
    {
        return trainingProjects.ContainsKey(projectId) ? trainingProjects[projectId] : null;
    }
    
    public List<TrainingProject> GetProjectsByCategory(TrainingCategory category)
    {
        string key = category.ToString();
        if (!projectsByCategory.ContainsKey(key)) return new List<TrainingProject>();
        
        List<TrainingProject> result = new List<TrainingProject>();
        foreach (string id in projectsByCategory[key])
            result.Add(trainingProjects[id]);
        return result;
    }
    
    public List<TrainingProject> GetAvailableProjects(int mountLevel)
    {
        List<TrainingProject> result = new List<TrainingProject>();
        foreach (var project in trainingProjects.Values)
        {
            if (project.RequiredLevel <= mountLevel)
                result.Add(project);
        }
        return result;
    }
    
    public BondLevelReward GetBondReward(int bondLevel)
    {
        return BondLevelRewards.ContainsKey(bondLevel) ? BondLevelRewards[bondLevel] : null;
    }
    
    public int GetRequiredBondPoints(int level)
    {
        return level * 100 + (level - 1) * 50;
    }
    
    public int GetRequiredExperience(int level)
    {
        return level * 100 + (level - 1) * 50;
    }
}

public class BondLevelReward
{
    public int Level;
    public float BonusMultiplier;
    public bool UnlocksSpecial;
    public string SpecialUnlock;
}
