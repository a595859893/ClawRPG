using Godot;
using System;
using System.Collections.Generic;

public class BossRushDatabase
{
    // Boss configurations for each stage
    public static Dictionary<int, List<BossRushBoss>> StageBosses = new Dictionary<int, List<BossRushBoss>>
    {
        { 1, new List<BossRushBoss> {
            new BossRushBoss { Name = "Slime King", Health = 5000, Attack = 150, Defense = 50, Speed = 80, Experience = 500, Gold = 200 },
            new BossRushBoss { Name = "Goblin Warlord", Health = 6000, Attack = 180, Defense = 60, Speed = 90, Experience = 600, Gold = 250 }
        }},
        { 2, new List<BossRushBoss> {
            new BossRushBoss { Name = "Skeleton Lord", Health = 8000, Attack = 220, Defense = 80, Speed = 100, Experience = 800, Gold = 350 },
            new BossRushBoss { Name = "Orc Chieftain", Health = 9000, Attack = 250, Defense = 90, Speed = 95, Experience = 900, Gold = 400 }
        }},
        { 3, new List<BossRushBoss> {
            new BossRushBoss { Name = "Troll Brute", Health = 12000, Attack = 300, Defense = 120, Speed = 85, Experience = 1200, Gold = 550 },
            new BossRushBoss { Name = "Dark Mage", Health = 10000, Attack = 350, Defense = 70, Speed = 110, Experience = 1300, Gold = 600 }
        }},
        { 4, new List<BossRushBoss> {
            new BossRushBoss { Name = "Frost Giant", Health = 15000, Attack = 320, Defense = 150, Speed = 75, Experience = 1500, Gold = 700 },
            new BossRushBoss { Name = "Flame Demon", Health = 14000, Attack = 380, Defense = 100, Speed = 105, Experience = 1600, Gold = 750 }
        }},
        { 5, new List<BossRushBoss> {
            new BossRushBoss { Name = "Thunder Lord", Health = 18000, Attack = 400, Defense = 130, Speed = 120, Experience = 2000, Gold = 900 },
            new BossRushBoss { Name = "Shadow Assassin", Health = 16000, Attack = 450, Defense = 110, Speed = 140, Experience = 2200, Gold = 1000 }
        }},
        { 6, new List<BossRushBoss> {
            new BossRushBoss { Name = "Ancient Golem", Health = 25000, Attack = 380, Defense = 200, Speed = 60, Experience = 3000, Gold = 1500 },
            new BossRushBoss { Name = "Lich King", Health = 22000, Attack = 480, Defense = 140, Speed = 100, Experience = 3200, Gold = 1600 }
        }},
        { 7, new List<BossRushBoss> {
            new BossRushBoss { Name = "Dragon Warlord", Health = 30000, Attack = 500, Defense = 180, Speed = 110, Experience = 4000, Gold = 2000 },
            new BossRushBoss { Name = "Phoenix Lord", Health = 28000, Attack = 520, Defense = 160, Speed = 130, Experience = 4200, Gold = 2100 }
        }},
        { 8, new List<BossRushBoss> {
            new BossRushBoss { Name = "Void Titan", Health = 40000, Attack = 550, Defense = 220, Speed = 100, Experience = 5500, Gold = 2800 },
            new BossRushBoss { Name = "Celestial Guardian", Health = 38000, Attack = 580, Defense = 200, Speed = 120, Experience = 5800, Gold = 3000 }
        }},
        { 9, new List<BossRushBoss> {
            new BossRushBoss { Name = "Demon Emperor", Health = 50000, Attack = 620, Defense = 250, Speed = 115, Experience = 7500, Gold = 4000 },
            BossRushBoss { Name = "Abyss Walker", Health = 48000, Attack = 650, Defense = 230, Speed = 135, Experience = 8000, Gold = 4200 }
        }},
        { 10, new List<BossRushBoss> {
            new BossRushBoss { Name = "World Eater", Health = 80000, Attack = 700, Defense = 300, Speed = 120, Experience = 15000, Gold = 10000 },
            new BossRushBoss { Name = "Chaos Incarnate", Health = 75000, Attack = 750, Defense = 280, Speed = 140, Experience = 18000, Gold = 12000 }
        }}
    };
    
    // Difficulty settings
    public static Dictionary<string, DifficultySetting> DifficultySettings = new Dictionary<string, DifficultySetting>
    {
        { "Easy", new DifficultySetting { HealthMultiplier = 0.7f, AttackMultiplier = 0.7f, RewardMultiplier = 1.0f } },
        { "Normal", new DifficultySetting { HealthMultiplier = 1.0f, AttackMultiplier = 1.0f, RewardMultiplier = 1.0f } },
        { "Hard", new DifficultySetting { HealthMultiplier = 1.5f, AttackMultiplier = 1.3f, RewardMultiplier = 1.5f } },
        { "Nightmare", new DifficultySetting { HealthMultiplier = 2.0f, AttackMultiplier = 1.6f, RewardMultiplier = 2.0f } },
        { "Legendary", new DifficultySetting { HealthMultiplier = 3.0f, AttackMultiplier = 2.0f, RewardMultiplier = 3.0f } }
    };
    
    // Stage rewards
    public static Dictionary<int, StageReward> StageRewards = new Dictionary<int, StageReward>
    {
        { 1, new StageReward { Gold = 500, Experience = 1000, ItemDropChance = 0.1f } },
        { 2, new StageReward { Gold = 750, Experience = 1500, ItemDropChance = 0.15f } },
        { 3, new StageReward { Gold = 1000, Experience = 2000, ItemDropChance = 0.2f } },
        { 4, new StageReward { Gold = 1500, Experience = 3000, ItemDropChance = 0.25f } },
        { 5, new StageReward { Gold = 2000, Experience = 4000, ItemDropChance = 0.3f } },
        { 6, new StageReward { Gold = 3000, Experience = 6000, ItemDropChance = 0.35f } },
        { 7, new StageReward { Gold = 4500, Experience = 9000, ItemDropChance = 0.4f } },
        { 8, new StageReward { Gold = 6000, Experience = 12000, ItemDropChance = 0.5f } },
        { 9, new StageReward { Gold = 8000, Experience = 16000, ItemDropChance = 0.6f } },
        { 10, new StageReward { Gold = 15000, Experience = 30000, ItemDropChance = 0.8f } }
    };
}

public class BossRushBoss
{
    public string Name { get; set; }
    public float Health { get; set; }
    public float Attack { get; set; }
    public float Defense { get; set; }
    public float Speed { get; set; }
    public int Experience { get; set; }
    public int Gold { get; set; }
    public string Element { get; set; } = "Physical";
    public List<string> Abilities { get; set; } = new List<string>();
}

public class DifficultySetting
{
    public float HealthMultiplier { get; set; } = 1.0f;
    public float AttackMultiplier { get; set; } = 1.0f;
    public float RewardMultiplier { get; set; } = 1.0f;
}

public class StageReward
{
    public int Gold { get; set; }
    public int Experience { get; set; }
    public float ItemDropChance { get; set; }
}
