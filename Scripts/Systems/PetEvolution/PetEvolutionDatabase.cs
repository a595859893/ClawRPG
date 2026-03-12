using System;
using System.Collections.Generic;

public class PetEvolutionDatabase
{
    // Evolution chains for each pet type
    // Key: pet type, Value: list of evolution forms in order
    public static Dictionary<string, List<EvolutionForm>> EvolutionChains = new Dictionary<string, List<EvolutionForm>>
    {
        // Dog evolution chain
        {
            "Dog", new List<EvolutionForm>
            {
                new EvolutionForm { FormName = "Puppy", Tier = 1, Rarity = "Common", RequiredLevel = 1, RequiredPoints = 0 },
                new EvolutionForm { FormName = "Adult Dog", Tier = 2, Rarity = "Uncommon", RequiredLevel = 10, RequiredPoints = 50 },
                new EvolutionForm { FormName = "War Dog", Tier = 3, Rarity = "Rare", RequiredLevel = 25, RequiredPoints = 150 },
                new EvolutionForm { FormName = "Noble Hound", Tier = 4, Rarity = "Epic", RequiredLevel = 40, RequiredPoints = 300 },
                new EvolutionForm { FormName = "Celestial Wolf", Tier = 5, Rarity = "Legendary", RequiredLevel = 60, RequiredPoints = 500 }
            }
        },
        // Cat evolution chain
        {
            "Cat", new List<EvolutionForm>
            {
                new EvolutionForm { FormName = "Kitten", Tier = 1, Rarity = "Common", RequiredLevel = 1, RequiredPoints = 0 },
                new EvolutionForm { FormName = "House Cat", Tier = 2, Rarity = "Uncommon", RequiredLevel = 10, RequiredPoints = 50 },
                new EvolutionForm { FormName = "Wild Cat", Tier = 3, Rarity = "Rare", RequiredLevel = 25, RequiredPoints = 150 },
                new EvolutionForm { FormName = "Shadow Leopard", Tier = 4, Rarity = "Epic", RequiredLevel = 40, RequiredPoints = 300 },
                new EvolutionForm { FormName = "Void Panther", Tier = 5, Rarity = "Legendary", RequiredLevel = 60, RequiredPoints = 500 }
            }
        },
        // Bird evolution chain
        {
            "Bird", new List<EvolutionForm>
            {
                new EvolutionForm { FormName = "Chick", Tier = 1, Rarity = "Common", RequiredLevel = 1, RequiredPoints = 0 },
                new EvolutionForm { FormName = "Young Bird", Tier = 2, Rarity = "Uncommon", RequiredLevel = 10, RequiredPoints = 50 },
                new EvolutionForm { FormName = "Hawk", Tier = 3, Rarity = "Rare", RequiredLevel = 25, RequiredPoints = 150 },
                new EvolutionForm { FormName = "Phoenix Hatchling", Tier = 4, Rarity = "Epic", RequiredLevel = 40, RequiredPoints = 300 },
                new EvolutionForm { FormName = "Solar Phoenix", Tier = 5, Rarity = "Legendary", RequiredLevel = 60, RequiredPoints = 500 }
            }
        },
        // Rabbit evolution chain
        {
            "Rabbit", new List<EvolutionForm>
            {
                new EvolutionForm { FormName = "Bunny", Tier = 1, Rarity = "Common", RequiredLevel = 1, RequiredPoints = 0 },
                new EvolutionForm { FormName = "Forest Rabbit", Tier = 2, Rarity = "Uncommon", RequiredLevel = 10, RequiredPoints = 50 },
                new EvolutionForm { FormName = "Thunder Hare", Tier = 3, Rarity = "Rare", RequiredLevel = 25, RequiredPoints = 150 },
                new EvolutionForm { FormName = "Moon Rabbit", Tier = 4, Rarity = "Epic", RequiredLevel = 40, RequiredPoints = 300 },
                new EvolutionForm { FormName = "Lunar Sage", Tier = 5, Rarity = "Legendary", RequiredLevel = 60, RequiredPoints = 500 }
            }
        },
        // Dragon evolution chain
        {
            "Dragon", new List<EvolutionForm>
            {
                new EvolutionForm { FormName = "Dragon Egg", Tier = 1, Rarity = "Common", RequiredLevel = 1, RequiredPoints = 0 },
                new EvolutionForm { FormName = "Hatchling", Tier = 2, Rarity = "Uncommon", RequiredLevel = 10, RequiredPoints = 50 },
                new EvolutionForm { FormName = "Young Dragon", Tier = 3, Rarity = "Rare", RequiredLevel = 25, RequiredPoints = 150 },
                new EvolutionForm { FormName = "Elder Dragon", Tier = 4, Rarity = "Epic", RequiredLevel = 40, RequiredPoints = 300 },
                new EvolutionForm { FormName = "Ancient Dragon God", Tier = 5, Rarity = "Legendary", RequiredLevel = 60, RequiredPoints = 500 }
            }
        },
        // Slime evolution chain
        {
            "Slime", new List<EvolutionForm>
            {
                new EvolutionForm { FormName = "Small Slime", Tier = 1, Rarity = "Common", RequiredLevel = 1, RequiredPoints = 0 },
                new EvolutionForm { FormName = "Gel Slime", Tier = 2, Rarity = "Uncommon", RequiredLevel = 10, RequiredPoints = 50 },
                new EvolutionForm { FormName = "Crystal Slime", Tier = 3, Rarity = "Rare", RequiredLevel = 25, RequiredPoints = 150 },
                new EvolutionForm { FormName = "Mega Slime", Tier = 4, Rarity = "Epic", RequiredLevel = 40, RequiredPoints = 300 },
                new EvolutionForm { FormName = "Slime King", Tier = 5, Rarity = "Legendary", RequiredLevel = 60, RequiredPoints = 500 }
            }
        },
        // Skeleton evolution chain
        {
            "Skeleton", new List<EvolutionForm>
            {
                new EvolutionForm { FormName = "Bone Sprite", Tier = 1, Rarity = "Common", RequiredLevel = 1, RequiredPoints = 0 },
                new EvolutionForm { FormName = "Skeleton Warrior", Tier = 2, Rarity = "Uncommon", RequiredLevel = 10, RequiredPoints = 50 },
                new EvolutionForm { FormName = "Death Knight", Tier = 3, Rarity = "Rare", RequiredLevel = 25, RequiredPoints = 150 },
                new EvolutionForm { FormName = "Lich Lord", Tier = 4, Rarity = "Epic", RequiredLevel = 40, RequiredPoints = 300 },
                new EvolutionForm { FormName = "Soul Reaper", Tier = 5, Rarity = "Legendary", RequiredLevel = 60, RequiredPoints = 500 }
            }
        },
        // Elemental evolution chain
        {
            "Elemental", new List<EvolutionForm>
            {
                new EvolutionForm { FormName = "Spark", Tier = 1, Rarity = "Common", RequiredLevel = 1, RequiredPoints = 0 },
                new EvolutionForm { FormName = "Flame Spirit", Tier = 2, Rarity = "Uncommon", RequiredLevel = 10, RequiredPoints = 50 },
                new EvolutionForm { FormName = "Inferno Wisp", Tier = 3, Rarity = "Rare", RequiredLevel = 25, RequiredPoints = 150 },
                new EvolutionForm { FormName = "Elemental Lord", Tier = 4, Rarity = "Epic", RequiredLevel = 40, RequiredPoints = 300 },
                new EvolutionForm { FormName = "Primordial Flame", Tier = 5, Rarity = "Legendary", RequiredLevel = 60, RequiredPoints = 500 }
            }
        }
    };

    // Evolution type bonuses
    public static Dictionary<string, EvolutionTypeBonus> EvolutionTypeBonuses = new Dictionary<string, EvolutionTypeBonus>
    {
        {
            "Battle", new EvolutionTypeBonus
            {
                TypeName = "Battle",
                AttackBonus = 20,
                DefenseBonus = 10,
                HealthBonus = 100,
                SpeedBonus = 5
            }
        },
        {
            "Economic", new EvolutionTypeBonus
            {
                TypeName = "Economic",
                GoldBonus = 25,
                DropRateBonus = 10,
                CriticalRateBonus = 5
            }
        },
        {
            "Support", new EvolutionTypeBonus
            {
                TypeName = "Support",
                HealingBonus = 20,
                DefenseBonus = 15,
                EvasionBonus = 10
            }
        },
        {
            "Balanced", new EvolutionTypeBonus
            {
                TypeName = "Balanced",
                AttackBonus = 10,
                DefenseBonus = 10,
                HealthBonus = 50,
                SpeedBonus = 5
            }
        }
    };

    // Evolution method definitions
    public static List<EvolutionMethod> EvolutionMethods = new List<EvolutionMethod>
    {
        new EvolutionMethod { MethodName = "Level Up", PointsPerAction = 10, Description = "Gain experience through battles" },
        new EvolutionMethod { MethodName = "Battle Victory", PointsPerAction = 15, Description = "Win battles" },
        new EvolutionMethod { MethodName = "Feeding", PointsPerAction = 5, Description = "Feed your pet" },
        new EvolutionMethod { MethodName = "Playing", PointsPerAction = 3, Description = "Play with your pet" },
        new EvolutionMethod { MethodName = "Training", PointsPerAction = 8, Description = "Train your pet" },
        new EvolutionMethod { MethodName = "Grooming", PointsPerAction = 4, Description = "Groom your pet" }
    };
}

public class EvolutionForm
{
    public string FormName { get; set; } = "";
    public int Tier { get; set; }
    public string Rarity { get; set; } = "";
    public int RequiredLevel { get; set; }
    public int RequiredPoints { get; set; }
    public string Description { get; set; } = "";
}

public class EvolutionTypeBonus
{
    public string TypeName { get; set; } = "";
    public int AttackBonus { get; set; }
    public int DefenseBonus { get; set; }
    public int HealthBonus { get; set; }
    public int SpeedBonus { get; set; }
    public int GoldBonus { get; set; }
    public int DropRateBonus { get; set; }
    public int CriticalRateBonus { get; set; }
    public int EvasionBonus { get; set; }
    public int HealingBonus { get; set; }
}

public class EvolutionMethod
{
    public string MethodName { get; set; } = "";
    public int PointsPerAction { get; set; }
    public string Description { get; set; } = "";
}
