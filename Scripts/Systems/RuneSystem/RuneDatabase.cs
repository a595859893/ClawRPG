using Godot;
using System.Collections.Generic;
using ClawRPG.Scripts.Database;

public class RuneDatabase : IDatabase
{
    private static RuneDatabase _instance;
    public static RuneDatabase Instance
    {
        get
        {
            if (_instance == null) _instance = new RuneDatabase();
            return _instance;
        }
    }

    object IDatabase.Instance => Instance;

    public void Initialize() { }

    public bool ValidateData() => Runes.Count > 0;
    
    // Rune Types
    public enum RuneType { Power, Defense, Support, Special }
    
    // Rune Slots
    public enum RuneSlot { Helmet, Chest, Legs, Weapon, Accessory }
    
    // Rune definition
    public class RuneDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public RuneType Type { get; set; }
        public RuneSlot Slot { get; set; }
        public int BaseCost { get; set; }
        public int EnhanceCost { get; set; }
        public Dictionary<string, float> Attributes { get; set; }
        public string SpecialEffect { get; set; }
        public int RequiredLevel { get; set; }
    }
    
    // All rune definitions
    public Dictionary<string, RuneDefinition> Runes = new Dictionary<string, RuneDefinition>();
    
    // Slot colors
    public Dictionary<RuneSlot, Color> SlotColors = new Dictionary<RuneSlot, Color>()
    {
        { RuneSlot.Helmet, new Color(1f, 0.8f, 0.4f) },
        { RuneSlot.Chest, new Color(0.4f, 0.8f, 1f) },
        { RuneSlot.Legs, new Color(0.4f, 1f, 0.6f) },
        { RuneSlot.Weapon, new Color(1f, 0.4f, 0.4f) },
        { RuneSlot.Accessory, new Color(0.8f, 0.6f, 1f) }
    };
    
    // Rune type colors
    public Dictionary<RuneType, Color> TypeColors = new Dictionary<RuneType, Color>()
    {
        { RuneType.Power, new Color(1f, 0.3f, 0.3f) },
        { RuneType.Defense, new Color(0.3f, 0.5f, 1f) },
        { RuneType.Support, new Color(0.3f, 1f, 0.5f) },
        { RuneType.Special, new Color(0.8f, 0.5f, 1f) }
    };
    
    public RuneDatabase()
    {
        InitializeRunes();
    }
    
    private void InitializeRunes()
    {
        // Power Runes - Helmet
        AddRune(new RuneDefinition
        {
            Id = "power_helmet_1",
            Name = "Battle Crown",
            Description = "A crown forged in battle, increasing attack power.",
            Type = RuneType.Power,
            Slot = RuneSlot.Helmet,
            BaseCost = 100,
            EnhanceCost = 50,
            RequiredLevel = 1,
            Attributes = new Dictionary<string, float> { { "attack", 5f } }
        });
        
        AddRune(new RuneDefinition
        {
            Id = "power_helmet_2",
            Name = "War Helmet",
            Description = "A helmet worn by ancient warriors.",
            Type = RuneType.Power,
            Slot = RuneSlot.Helmet,
            BaseCost = 250,
            EnhanceCost = 100,
            RequiredLevel = 15,
            Attributes = new Dictionary<string, float> { { "attack", 12f }, { "critical", 2f } }
        });
        
        AddRune(new RuneDefinition
        {
            Id = "power_helmet_3",
            Name = "Berserker Crown",
            Description = "A crown that fuels rage and fury.",
            Type = RuneType.Power,
            Slot = RuneSlot.Helmet,
            BaseCost = 500,
            EnhanceCost = 200,
            RequiredLevel = 30,
            Attributes = new Dictionary<string, float> { { "attack", 25f }, { "critical", 5f }, { "life_steal", 3f } },
            SpecialEffect = " berserker_fury"
        });
        
        // Power Runes - Weapon
        AddRune(new RuneDefinition
        {
            Id = "power_weapon_1",
            Name = "Sharp Edge",
            Description = "A rune that sharpens weapons.",
            Type = RuneType.Power,
            Slot = RuneSlot.Weapon,
            BaseCost = 150,
            EnhanceCost = 75,
            RequiredLevel = 5,
            Attributes = new Dictionary<string, float> { { "attack", 8f } }
        });
        
        AddRune(new RuneDefinition
        {
            Id = "power_weapon_2",
            Name = "Flame Blade",
            Description = "A blade engulfed in eternal flames.",
            Type = RuneType.Power,
            Slot = RuneSlot.Weapon,
            BaseCost = 350,
            EnhanceCost = 150,
            RequiredLevel = 20,
            Attributes = new Dictionary<string, float> { { "attack", 18f }, { "fire_damage", 10f } },
            SpecialEffect = " flame_enchant"
        });
        
        AddRune(new RuneDefinition
        {
            Id = "power_weapon_3",
            Name = "Thunder Strike",
            Description = "A weapon crackling with lightning.",
            Type = RuneType.Power,
            Slot = RuneSlot.Weapon,
            BaseCost = 700,
            EnhanceCost = 300,
            RequiredLevel = 35,
            Attributes = new Dictionary<string, float> { { "attack", 30f }, { "lightning_damage", 15f }, { "speed", 5f } },
            SpecialEffect = " thunder_enchant"
        });
        
        // Defense Runes - Chest
        AddRune(new RuneDefinition
        {
            Id = "defense_chest_1",
            Name = "Iron Plate",
            Description = "A sturdy plate that enhances defense.",
            Type = RuneType.Defense,
            Slot = RuneSlot.Chest,
            BaseCost = 120,
            EnhanceCost = 60,
            RequiredLevel = 3,
            Attributes = new Dictionary<string, float> { { "defense", 10f } }
        });
        
        AddRune(new RuneDefinition
        {
            Id = "defense_chest_2",
            Name = "Dragon Scale",
            Description = "Armor made from dragon scales.",
            Type = RuneType.Defense,
            Slot = RuneSlot.Chest,
            BaseCost = 300,
            EnhanceCost = 120,
            RequiredLevel = 18,
            Attributes = new Dictionary<string, float> { { "defense", 22f }, { "fire_resist", 15f } }
        });
        
        AddRune(new RuneDefinition
        {
            Id = "defense_chest_3",
            Name = "Divine Shield",
            Description = "A shield blessed by the gods.",
            Type = RuneType.Defense,
            Slot = RuneSlot.Chest,
            BaseCost = 600,
            EnhanceCost = 250,
            RequiredLevel = 32,
            Attributes = new Dictionary<string, float> { { "defense", 35f }, { "all_resist", 10f }, { "health", 50f } },
            SpecialEffect = " divine_aura"
        });
        
        // Defense Runes - Legs
        AddRune(new RuneDefinition
        {
            Id = "defense_legs_1",
            Name = "Steel Greaves",
            Description = "Leg armor that provides stability.",
            Type = RuneType.Defense,
            Slot = RuneSlot.Legs,
            BaseCost = 100,
            EnhanceCost = 50,
            RequiredLevel = 2,
            Attributes = new Dictionary<string, float> { { "defense", 6f }, { "health", 20f } }
        });
        
        AddRune(new RuneDefinition
        {
            Id = "defense_legs_2",
            Name = "Titan Leggings",
            Description = "Leggings forged from titan steel.",
            Type = RuneType.Defense,
            Slot = RuneSlot.Legs,
            BaseCost = 280,
            EnhanceCost = 110,
            RequiredLevel = 16,
            Attributes = new Dictionary<string, float> { { "defense", 15f }, { "health", 45f }, { "evasion", 3f } }
        });
        
        // Support Runes - Accessory
        AddRune(new RuneDefinition
        {
            Id = "support_accessory_1",
            Name = "Life Pendant",
            Description = "A pendant that channels life energy.",
            Type = RuneType.Support,
            Slot = RuneSlot.Accessory,
            BaseCost = 130,
            EnhanceCost = 65,
            RequiredLevel = 4,
            Attributes = new Dictionary<string, float> { { "health", 30f }, { "health_regen", 1f } }
        });
        
        AddRune(new RuneDefinition
        {
            Id = "support_accessory_2",
            Name = "Sage's Wisdom",
            Description = "An amulet worn by ancient sages.",
            Type = RuneType.Support,
            Slot = RuneSlot.Accessory,
            BaseCost = 320,
            EnhanceCost = 130,
            RequiredLevel = 22,
            Attributes = new Dictionary<string, float> { { "magic", 15f }, { "mana", 40f }, { "mana_regen", 2f } }
        });
        
        AddRune(new RuneDefinition
        {
            Id = "support_accessory_3",
            Name = "Angel's Blessing",
            Description = "A blessing from the celestial angels.",
            Type = RuneType.Support,
            Slot = RuneSlot.Accessory,
            BaseCost = 650,
            EnhanceCost = 280,
            RequiredLevel = 38,
            Attributes = new Dictionary<string, float> { { "health", 60f }, { "defense", 15f }, { "all_resist", 8f } },
            SpecialEffect = " angel_blessing"
        });
        
        // Special Runes
        AddRune(new RuneDefinition
        {
            Id = "special_weapon_1",
            Name = "Vampire Fang",
            Description = "A cursed fang that drains life.",
            Type = RuneType.Special,
            Slot = RuneSlot.Weapon,
            BaseCost = 400,
            EnhanceCost = 180,
            RequiredLevel = 25,
            Attributes = new Dictionary<string, float> { { "attack", 15f }, { "life_steal", 8f } },
            SpecialEffect = " life_drain"
        });
        
        AddRune(new RuneDefinition
        {
            Id = "special_accessory_1",
            Name = "Shadow Cloak",
            Description = "A cloak that hides the wearer in shadows.",
            Type = RuneType.Special,
            Slot = RuneSlot.Accessory,
            BaseCost = 450,
            EnhanceCost = 200,
            RequiredLevel = 28,
            Attributes = new Dictionary<string, float> { { "evasion", 8f }, { "critical", 5f }, { "speed", 3f } },
            SpecialEffect = " shadow_step"
        });
    }
    
    private void AddRune(RuneDefinition rune)
    {
        Runes[rune.Id] = rune;
    }
    
    public RuneDefinition GetRune(string id)
    {
        return Runes.ContainsKey(id) ? Runes[id] : null;
    }
    
    public List<RuneDefinition> GetRunesBySlot(RuneSlot slot)
    {
        List<RuneDefinition> result = new List<RuneDefinition>();
        foreach (var rune in Runes.Values)
        {
            if (rune.Slot == slot) result.Add(rune);
        }
        return result;
    }
    
    public List<RuneDefinition> GetRunesByType(RuneType type)
    {
        List<RuneDefinition> result = new List<RuneDefinition>();
        foreach (var rune in Runes.Values)
        {
            if (rune.Type == type) result.Add(rune);
        }
        return result;
    }
}
