using Godot;
using System;
using System.Collections.Generic;

public class TattooDatabase
{
    // Tattoo categories
    public enum TattooCategory
    {
        Battle,
        Mythical,
        Nature,
        Spiritual,
        Decorative,
        Legendary
    }
    
    // Tattoo rarity
    public enum TattooRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    // Single tattoo configuration
    public class TattooConfig
    {
        public string Id;
        public string Name;
        public string Description;
        public TattooCategory Category;
        public TattooRarity Rarity;
        public int Cost;
        public float AttackBonus;
        public float DefenseBonus;
        public float HealthBonus;
        public float SpeedBonus;
        public float CriticalBonus;
        public float EvasionBonus;
        public string IconPath;
    }
    
    // All tattoo configurations
    public Dictionary<string, TattooConfig> Tattoos = new Dictionary<string, TattooConfig>();
    
    // Slot configurations
    public Dictionary<string, List<string>> BodySlots = new Dictionary<string, List<string>>
    {
        { "Head", new List<string> { "tattoo_head_1", "tattoo_head_2", "tattoo_head_3" } },
        { "Arm", new List<string> { "tattoo_arm_1", "tattoo_arm_2", "tattoo_arm_3" } },
        { "Back", new List<string> { "tattoo_back_1", "tattoo_back_2", "tattoo_back_3" } },
        { "Chest", new List<string> { "tattoo_chest_1", "tattoo_chest_2", "tattoo_chest_3" } },
        { "Leg", new List<string> { "tattoo_leg_1", "tattoo_leg_2", "tattoo_leg_3" } }
    };
    
    public TattooDatabase()
    {
        // Battle tattoos
        RegisterTattoo(new TattooConfig
        {
            Id = "tattoo_battle_1",
            Name = "Warrior's Mark",
            Description = "A symbol of battle prowess",
            Category = TattooCategory.Battle,
            Rarity = TattooRarity.Common,
            Cost = 100,
            AttackBonus = 5f
        });
        
        RegisterTattoo(new TattooConfig
        {
            Id = "tattoo_battle_2",
            Name = "Blade Scar",
            Description = "Marks of countless victories",
            Category = TattooCategory.Battle,
            Rarity = TattooRarity.Uncommon,
            Cost = 250,
            AttackBonus = 10f,
            CriticalBonus = 2f
        });
        
        RegisterTattoo(new TattooConfig
        {
            Id = "tattoo_battle_3",
            Name = "Champion's Crest",
            Description = "Emblem of a true champion",
            Category = TattooCategory.Battle,
            Rarity = TattooRarity.Rare,
            Cost = 500,
            AttackBonus = 15f,
            DefenseBonus = 10f,
            CriticalBonus = 5f
        });
        
        // Mythical tattoos
        RegisterTattoo(new TattooConfig
        {
            Id = "tattoo_myth_1",
            Name = "Dragon Fang",
            Description = "The power of an ancient dragon",
            Category = TattooCategory.Mythical,
            Rarity = TattooRarity.Rare,
            Cost = 600,
            AttackBonus = 20f,
            HealthBonus = 50f
        });
        
        RegisterTattoo(new TattooConfig
        {
            Id = "tattoo_myth_2",
            Name = "Phoenix Wing",
            Description = "Rises from the ashes",
            Category = TattooCategory.Mythical,
            Rarity = TattooRarity.Epic,
            Cost = 1200,
            HealthBonus = 100f,
            DefenseBonus = 15f,
            SpeedBonus = 5f
        });
        
        RegisterTattoo(new TattooConfig
        {
            Id = "tattoo_myth_3",
            Name = "Celestial Guardian",
            Description = "Blessed by the heavens",
            Category = TattooCategory.Mythical,
            Rarity = TattooRarity.Legendary,
            Cost = 3000,
            AttackBonus = 25f,
            DefenseBonus = 25f,
            HealthBonus = 150f,
            SpeedBonus = 10f
        });
        
        // Nature tattoos
        RegisterTattoo(new TattooConfig
        {
            Id = "tattoo_nature_1",
            Name = "Vine Mark",
            Description = "One with nature",
            Category = TattooCategory.Nature,
            Rarity = TattooRarity.Common,
            Cost = 100,
            HealthBonus = 20f
        });
        
        RegisterTattoo(new TattooConfig
        {
            Id = "tattoo_nature_2",
            Name = "Oak Shield",
            Description = "Strong as ancient oak",
            Category = TattooCategory.Nature,
            Rarity = TattooRarity.Uncommon,
            Cost = 200,
            HealthBonus = 40f,
            DefenseBonus = 5f
        });
        
        RegisterTattoo(new TattooConfig
        {
            Id = "tattoo_nature_3",
            Name = "Forest Spirit",
            Description = "Guardian of the woods",
            Category = TattooCategory.Nature,
            Rarity = TattooRarity.Rare,
            Cost = 550,
            HealthBonus = 80f,
            DefenseBonus = 15f,
            EvasionBonus = 3f
        });
        
        // Spiritual tattoos
        RegisterTattoo(new TattooConfig
        {
            Id = "tattoo_spirit_1",
            Name = "Third Eye",
            Description = "Sees beyond the veil",
            Category = TattooCategory.Spiritual,
            Rarity = TattooRarity.Uncommon,
            Cost = 300,
            CriticalBonus = 5f,
            EvasionBonus = 3f
        });
        
        RegisterTattoo(new TattooConfig
        {
            Id = "tattoo_spirit_2",
            Name = "Soul Anchor",
            Description = "Anchors the soul",
            Category = TattooCategory.Spiritual,
            Rarity = TattooRarity.Rare,
            Cost = 600,
            HealthBonus = 60f,
            DefenseBonus = 10f,
            SpeedBonus = 3f
        });
        
        // Decorative tattoos
        RegisterTattoo(new TattooConfig
        {
            Id = "tattoo_deco_1",
            Name = "Rose Pattern",
            Description = "Beautiful rose design",
            Category = TattooCategory.Decorative,
            Rarity = TattooRarity.Common,
            Cost = 50,
            SpeedBonus = 2f
        });
        
        RegisterTattoo(new TattooConfig
        {
            Id = "tattoo_deco_2",
            Name = "Tribal Art",
            Description = "Ancient tribal design",
            Category = TattooCategory.Decorative,
            Rarity = TattooRarity.Uncommon,
            Cost = 150,
            AttackBonus = 5f,
            DefenseBonus = 5f
        });
        
        RegisterTattoo(new TattooConfig
        {
            Id = "tattoo_deco_3",
            Name = "Celtic Knot",
            Description = "Eternal intertwined design",
            Category = TattooCategory.Decorative,
            Rarity = TattooRarity.Rare,
            Cost = 400,
            HealthBonus = 30f,
            AttackBonus = 8f,
            DefenseBonus = 8f
        });
        
        // Legendary tattoos
        RegisterTattoo(new TattooConfig
        {
            Id = "tattoo_legend_1",
            Name = "Demon King's Mark",
            Description = "Power beyond mortal comprehension",
            Category = TattooCategory.Legendary,
            Rarity = TattooRarity.Legendary,
            Cost = 5000,
            AttackBonus = 40f,
            HealthBonus = 200f,
            CriticalBonus = 10f
        });
        
        RegisterTattoo(new TattooConfig
        {
            Id = "tattoo_legend_2",
            Name = "Goddess's Blessing",
            Description = "Divine protection",
            Category = TattooCategory.Legendary,
            Rarity = TattooRarity.Legendary,
            Cost = 5000,
            DefenseBonus = 40f,
            HealthBonus = 250f,
            EvasionBonus = 8f
        });
    }
    
    private void RegisterTattoo(TattooConfig config)
    {
        Tattoos[config.Id] = config;
    }
    
    public TattooConfig GetTattoo(string id)
    {
        if (Tattoos.ContainsKey(id))
            return Tattoos[id];
        return null;
    }
    
    public List<TattooConfig> GetTattoosByCategory(TattooCategory category)
    {
        List<TattooConfig> result = new List<TattooConfig>();
        foreach (var tattoo in Tattoos.Values)
        {
            if (tattoo.Category == category)
                result.Add(tattoo);
        }
        return result;
    }
    
    public List<TattooConfig> GetTattoosByRarity(TattooRarity rarity)
    {
        List<TattooConfig> result = new List<TattooConfig>();
        foreach (var tattoo in Tattoos.Values)
        {
            if (tattoo.Rarity == rarity)
                result.Add(tattoo);
        }
        return result;
    }
    
    public bool IsSlotAvailable(string slot)
    {
        return BodySlots.ContainsKey(slot);
    }
    
    public List<string> GetAvailableSlots()
    {
        return new List<string>(BodySlots.Keys);
    }
}
