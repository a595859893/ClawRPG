using Godot;
using System;
using System.Collections.Generic;

public class PetTalentData
{
    // Talent points available for each pet
    public int AvailablePoints { get; set; }
    
    // Allocated talent points by category
    public Dictionary<string, int> AllocatedPoints { get; set; }
    
    // Unlocked talents (talent_id -> level)
    public Dictionary<string, int> UnlockedTalents { get; set; }
    
    // Total talent points earned (from leveling)
    public int TotalPointsEarned { get; set; }
    
    // Total talent points spent
    public int TotalPointsSpent { get; set; }
    
    public PetTalentData()
    {
        AvailablePoints = 0;
        AllocatedPoints = new Dictionary<string, int>();
        UnlockedTalents = new Dictionary<string, int>();
        TotalPointsEarned = 0;
        TotalPointsSpent = 0;
    }
}

public class PetTalentCategory
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public List<PetTalent> Talents { get; set; }
    
    public PetTalentCategory()
    {
        Talents = new List<PetTalent>();
    }
}

public class PetTalent
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int MaxLevel { get; set; }
    public int PointsPerLevel { get; set; }
    
    // Attribute bonuses per level
    public float AttackBonus { get; set; }
    public float DefenseBonus { get; set; }
    public float HealthBonus { get; set; }
    public float SpeedBonus { get; set; }
    public float CritRateBonus { get; set; }
    public float CritDamageBonus { get; set; }
    public float LifeStealBonus { get; set; }
    public float DodgeBonus { get; set; }
    
    public string Category { get; set; }
    
    public PetTalent()
    {
        MaxLevel = 5;
        PointsPerLevel = 1;
    }
}
