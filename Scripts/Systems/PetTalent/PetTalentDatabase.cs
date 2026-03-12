using Godot;
using System;
using System.Collections.Generic;

public class PetTalentDatabase
{
    private static PetTalentDatabase _instance;
    public static PetTalentDatabase Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PetTalentDatabase();
            return _instance;
        }
    }
    
    public List<PetTalentCategory> Categories { get; private set; }
    public Dictionary<string, PetTalent> Talents { get; private set; }
    
    public PetTalentDatabase()
    {
        Categories = new List<PetTalentCategory>();
        Talents = new Dictionary<string, PetTalent>();
        InitializeTalents();
    }
    
    private void InitializeTalents()
    {
        // Combat Talents
        var combatCategory = new PetTalentCategory
        {
            Id = "combat",
            Name = "Combat",
            Description = "Offensive and defensive abilities",
            Icon = "⚔️"
        };
        
        combatCategory.Talents.Add(new PetTalent
        {
            Id = "combat_power",
            Name = "Combat Power",
            Description = "Increases pet attack damage",
            MaxLevel = 5,
            PointsPerLevel = 1,
            AttackBonus = 10f,
            Category = "combat"
        });
        
        combatCategory.Talents.Add(new PetTalent
        {
            Id = "combat_endurance",
            Name = "Combat Endurance",
            Description = "Increases pet max health",
            MaxLevel = 5,
            PointsPerLevel = 1,
            HealthBonus = 50f,
            Category = "combat"
        });
        
        combatCategory.Talents.Add(new PetTalent
        {
            Id = "combat_armor",
            Name = "Combat Armor",
            Description = "Increases pet defense",
            MaxLevel = 5,
            PointsPerLevel = 1,
            DefenseBonus = 8f,
            Category = "combat"
        });
        
        combatCategory.Talents.Add(new PetTalent
        {
            Id = "critical_strike",
            Name = "Critical Strike",
            Description = "Increases critical hit rate",
            MaxLevel = 5,
            PointsPerLevel = 1,
            CritRateBonus = 2f,
            Category = "combat"
        });
        
        combatCategory.Talents.Add(new PetTalent
        {
            Id = "deadly_precision",
            Name = "Deadly Precision",
            Description = "Increases critical hit damage",
            MaxLevel = 5,
            PointsPerLevel = 1,
            CritDamageBonus = 10f,
            Category = "combat"
        });
        
        Categories.Add(combatCategory);
        
        // Speed Talents
        var speedCategory = new PetTalentCategory
        {
            Id = "speed",
            Name = "Speed",
            Description = "Agility and movement abilities",
            Icon = "⚡"
        };
        
        speedCategory.Talents.Add(new PetTalent
        {
            Id = "swiftness",
            Name = "Swiftness",
            Description = "Increases pet speed",
            MaxLevel = 5,
            PointsPerLevel = 1,
            SpeedBonus = 5f,
            Category = "speed"
        });
        
        speedCategory.Talents.Add(new PetTalent
        {
            Id = "evasion",
            Name = "Evasion",
            Description = "Increases dodge chance",
            MaxLevel = 5,
            PointsPerLevel = 1,
            DodgeBonus = 2f,
            Category = "speed"
        });
        
        speedCategory.Talents.Add(new PetTalent
        {
            Id = "quick_attack",
            Name = "Quick Attack",
            Description = "Increases attack speed",
            MaxLevel = 5,
            PointsPerLevel = 1,
            SpeedBonus = 3f,
            Category = "speed"
        });
        
        Categories.Add(speedCategory);
        
        // Survival Talents
        var survivalCategory = new PetTalentCategory
        {
            Id = "survival",
            Name = "Survival",
            Description = "Defensive and healing abilities",
            Icon = "🛡️"
        };
        
        survivalCategory.Talents.Add(new PetTalent
        {
            Id = "iron_skin",
            Name = "Iron Skin",
            Description = "Increases defense significantly",
            MaxLevel = 5,
            PointsPerLevel = 1,
            DefenseBonus = 12f,
            Category = "survival"
        });
        
        survivalCategory.Talents.Add(new PetTalent
        {
            Id = "regeneration",
            Name = "Regeneration",
            Description = "Health regeneration over time",
            MaxLevel = 5,
            PointsPerLevel = 1,
            HealthBonus = 30f,
            Category = "survival"
        });
        
        survivalCategory.Talents.Add(new PetTalent
        {
            Id = "vampiric",
            Name = "Vampiric",
            Description = "Life steal from attacks",
            MaxLevel = 5,
            PointsPerLevel = 1,
            LifeStealBonus = 3f,
            Category = "survival"
        });
        
        Categories.Add(survivalCategory);
        
        // Special Talents
        var specialCategory = new PetTalentCategory
        {
            Id = "special",
            Name = "Special",
            Description = "Unique and powerful abilities",
            Icon = "✨"
        };
        
        specialCategory.Talents.Add(new PetTalent
        {
            Id = "elemental_mastery",
            Name = "Elemental Mastery",
            Description = "Mastery over elements",
            MaxLevel = 3,
            PointsPerLevel = 2,
            AttackBonus = 15f,
            Category = "special"
        });
        
        specialCategory.Talents.Add(new PetTalent
        {
            Id = "battle_instinct",
            Name = "Battle Instinct",
            Description = "Passive combat awareness",
            MaxLevel = 3,
            PointsPerLevel = 2,
            CritRateBonus = 3f,
            CritDamageBonus = 15f,
            Category = "special"
        });
        
        specialCategory.Talents.Add(new PetTalent
        {
            Id = "legendary_potential",
            Name = "Legendary Potential",
            Description = "Unlock ultimate potential",
            MaxLevel = 1,
            PointsPerLevel = 5,
            AttackBonus = 25f,
            DefenseBonus = 20f,
            HealthBonus = 100f,
            SpeedBonus = 10f,
            CritRateBonus = 5f,
            Category = "special"
        });
        
        Categories.Add(specialCategory);
        
        // Build talent dictionary
        foreach (var category in Categories)
        {
            foreach (var talent in category.Talents)
            {
                Talents[talent.Id] = talent;
            }
        }
    }
    
    public PetTalent GetTalent(string talentId)
    {
        return Talents.ContainsKey(talentId) ? Talents[talentId] : null;
    }
    
    public PetTalentCategory GetCategory(string categoryId)
    {
        foreach (var category in Categories)
        {
            if (category.Id == categoryId)
                return category;
        }
        return null;
    }
    
    public int GetTotalTalentPointsForLevel(int petLevel)
    {
        // 1 point per level
        return petLevel;
    }
}
