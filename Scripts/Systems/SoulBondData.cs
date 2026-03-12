using Godot;
using System;
using System.Collections.Generic;

public enum BondType
{
    Weapon,
    Armor,
    Accessory,
    Pet
}

public enum BondLevel
{
    None,
    Awakening,
    Manifestation,
    Convergence,
    Transcendence,
    Nirvana
}

public class SoulBondData
{
    public string BondId { get; set; }
    public string ItemOrPetId { get; set; }
    public BondType BondType { get; set; }
    public BondLevel CurrentLevel { get; set; }
    public int TotalBondPoints { get; set; }
    public int BondPointsToNextLevel { get; set; }
    public List<string> UnlockedAbilities { get; set; }
    public List<BondMilestone> Milestones { get; set; }
    public DateTime BondedAt { get; set; }
    public DateTime LastInteractionTime { get; set; }
    public int InteractionCount { get; set; }

    public SoulBondData()
    {
        UnlockedAbilities = new List<string>();
        Milestones = new List<BondMilestone>();
        CurrentLevel = BondLevel.None;
    }
}

public class BondMilestone
{
    public BondLevel Level { get; set; }
    public string AbilityId { get; set; }
    public string Description { get; set; }
    public bool Unlocked { get; set; }
}

public class SoulBondRecord
{
    public string BondId { get; set; }
    public DateTime Timestamp { get; set; }
    public BondLevel PreviousLevel { get; set; }
    public BondLevel NewLevel { get; set; }
    public string AbilityUnlocked { get; set; }
    public int PointsSpent { get; set; }
}
