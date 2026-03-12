using Godot;
using System;
using System.Collections.Generic;

public class ConstellationDatabase : Node
{
    // All constellation configurations
    private Dictionary<string, ConstellationData.Constellation> _constellations = new Dictionary<string, ConstellationData.Constellation>();
    
    public override void _Ready()
    {
        InitializeConstellations();
    }
    
    private void InitializeConstellations()
    {
        // Fire Constellations (Aries, Leo, Sagittarius)
        AddConstellation(new ConstellationData.Constellation
        {
            Id = "aries",
            Name = "Aries",
            Description = "The Ram - Grants fiery passion and combat prowess",
            Type = ConstellationData.ConstellationType.Fire,
            Rarity = ConstellationData.ConstellationRarity.Common,
            Stars = 3,
            AttackBonus = 0.10f,
            DefenseBonus = 0.05f,
            HealthBonus = 0.05f,
            SpeedBonus = 0.08f,
            CriticalBonus = 0.05f,
            EvasionBonus = 0.0f,
            GoldBonus = 0.0f,
            ExpBonus = 0.05f,
            UnlockCost = 100,
            RequiredLevel = 1
        });
        
        AddConstellation(new ConstellationData.Constellation
        {
            Id = "leo",
            Name = "Leo",
            Description = "The Lion - The king of beasts grants royal strength",
            Type = ConstellationData.ConstellationType.Fire,
            Rarity = ConstellationData.ConstellationRarity.Rare,
            Stars = 5,
            AttackBonus = 0.20f,
            DefenseBonus = 0.10f,
            HealthBonus = 0.15f,
            SpeedBonus = 0.05f,
            CriticalBonus = 0.10f,
            EvasionBonus = 0.05f,
            GoldBonus = 0.10f,
            ExpBonus = 0.08f,
            UnlockCost = 500,
            RequiredLevel = 15
        });
        
        AddConstellation(new ConstellationData.Constellation
        {
            Id = "sagittarius",
            Name = "Sagittarius",
            Description = "The Archer - Seeks truth and grants accuracy",
            Type = ConstellationData.ConstellationType.Fire,
            Rarity = ConstellationData.ConstellationRarity.Epic,
            Stars = 7,
            AttackBonus = 0.25f,
            DefenseBonus = 0.12f,
            HealthBonus = 0.10f,
            SpeedBonus = 0.15f,
            CriticalBonus = 0.15f,
            EvasionBonus = 0.08f,
            GoldBonus = 0.12f,
            ExpBonus = 0.12f,
            UnlockCost = 1000,
            RequiredLevel = 30
        });
        
        // Water Constellations (Cancer, Scorpio, Pisces)
        AddConstellation(new ConstellationData.Constellation
        {
            Id = "cancer",
            Name = "Cancer",
            Description = "The Crab - Grants defensive resilience",
            Type = ConstellationData.ConstellationType.Water,
            Rarity = ConstellationData.ConstellationRarity.Common,
            Stars = 3,
            AttackBonus = 0.05f,
            DefenseBonus = 0.12f,
            HealthBonus = 0.10f,
            SpeedBonus = 0.05f,
            CriticalBonus = 0.0f,
            EvasionBonus = 0.05f,
            GoldBonus = 0.0f,
            ExpBonus = 0.05f,
            UnlockCost = 100,
            RequiredLevel = 1
        });
        
        AddConstellation(new ConstellationData.Constellation
        {
            Id = "scorpio",
            Name = "Scorpio",
            Description = "The Scorpion - Grants deadly precision",
            Type = ConstellationData.ConstellationType.Water,
            Rarity = ConstellationData.ConstellationRarity.Rare,
            Stars = 5,
            AttackBonus = 0.18f,
            DefenseBonus = 0.08f,
            HealthBonus = 0.12f,
            SpeedBonus = 0.10f,
            CriticalBonus = 0.18f,
            EvasionBonus = 0.10f,
            GoldBonus = 0.08f,
            ExpBonus = 0.10f,
            UnlockCost = 500,
            RequiredLevel = 15
        });
        
        AddConstellation(new ConstellationData.Constellation
        {
            Id = "pisces",
            Name = "Pisces",
            Description = "The Fish - Grants mystical wisdom",
            Type = ConstellationData.ConstellationType.Water,
            Rarity = ConstellationData.ConstellationRarity.Epic,
            Stars = 7,
            AttackBonus = 0.12f,
            DefenseBonus = 0.20f,
            HealthBonus = 0.20f,
            SpeedBonus = 0.08f,
            CriticalBonus = 0.10f,
            EvasionBonus = 0.15f,
            GoldBonus = 0.15f,
            ExpBonus = 0.15f,
            UnlockCost = 1000,
            RequiredLevel = 30
        });
        
        // Earth Constellations (Taurus, Virgo, Capricorn)
        AddConstellation(new ConstellationData.Constellation
        {
            Id = "taurus",
            Name = "Taurus",
            Description = "The Bull - Grants unyielding strength",
            Type = ConstellationData.ConstellationType.Earth,
            Rarity = ConstellationData.ConstellationRarity.Common,
            Stars = 3,
            AttackBonus = 0.08f,
            DefenseBonus = 0.15f,
            HealthBonus = 0.12f,
            SpeedBonus = 0.03f,
            CriticalBonus = 0.03f,
            EvasionBonus = 0.0f,
            GoldBonus = 0.05f,
            ExpBonus = 0.03f,
            UnlockCost = 100,
            RequiredLevel = 1
        });
        
        AddConstellation(new ConstellationData.Constellation
        {
            Id = "virgo",
            Name = "Virgo",
            Description = "The Maiden - Grants analytical precision",
            Type = ConstellationData.ConstellationType.Earth,
            Rarity = ConstellationData.ConstellationRarity.Rare,
            Stars = 5,
            AttackBonus = 0.12f,
            DefenseBonus = 0.18f,
            HealthBonus = 0.15f,
            SpeedBonus = 0.08f,
            CriticalBonus = 0.12f,
            EvasionBonus = 0.08f,
            GoldBonus = 0.15f,
            ExpBonus = 0.12f,
            UnlockCost = 500,
            RequiredLevel = 15
        });
        
        AddConstellation(new ConstellationData.Constellation
        {
            Id = "capricorn",
            Name = "Capricorn",
            Description = "The Goat - Grants ambitious determination",
            Type = ConstellationData.ConstellationType.Earth,
            Rarity = ConstellationData.ConstellationRarity.Epic,
            Stars = 7,
            AttackBonus = 0.15f,
            DefenseBonus = 0.25f,
            HealthBonus = 0.25f,
            SpeedBonus = 0.10f,
            CriticalBonus = 0.08f,
            EvasionBonus = 0.05f,
            GoldBonus = 0.20f,
            ExpBonus = 0.18f,
            UnlockCost = 1000,
            RequiredLevel = 30
        });
        
        // Air Constellations (Gemini, Libra, Aquarius)
        AddConstellation(new ConstellationData.Constellation
        {
            Id = "gemini",
            Name = "Gemini",
            Description = "The Twins - Grants versatile agility",
            Type = ConstellationData.ConstellationType.Air,
            Rarity = ConstellationData.ConstellationRarity.Common,
            Stars = 3,
            AttackBonus = 0.07f,
            DefenseBonus = 0.05f,
            HealthBonus = 0.05f,
            SpeedBonus = 0.15f,
            CriticalBonus = 0.08f,
            EvasionBonus = 0.08f,
            GoldBonus = 0.03f,
            ExpBonus = 0.08f,
            UnlockCost = 100,
            RequiredLevel = 1
        });
        
        AddConstellation(new ConstellationData.Constellation
        {
            Id = "libra",
            Name = "Libra",
            Description = "The Scales - Grants balance and harmony",
            Type = ConstellationData.ConstellationType.Air,
            Rarity = ConstellationData.ConstellationRarity.Rare,
            Stars = 5,
            AttackBonus = 0.10f,
            DefenseBonus = 0.15f,
            HealthBonus = 0.10f,
            SpeedBonus = 0.12f,
            CriticalBonus = 0.10f,
            EvasionBonus = 0.15f,
            GoldBonus = 0.12f,
            ExpBonus = 0.10f,
            UnlockCost = 500,
            RequiredLevel = 15
        });
        
        AddConstellation(new ConstellationData.Constellation
        {
            Id = "aquarius",
            Name = "Aquarius",
            Description = "The Water Bearer - Grants innovative wisdom",
            Type = ConstellationData.ConstellationType.Air,
            Rarity = ConstellationData.ConstellationRarity.Epic,
            Stars = 7,
            AttackBonus = 0.18f,
            DefenseBonus = 0.12f,
            HealthBonus = 0.12f,
            SpeedBonus = 0.20f,
            CriticalBonus = 0.12f,
            EvasionBonus = 0.18f,
            GoldBonus = 0.18f,
            ExpBonus = 0.20f,
            UnlockCost = 1000,
            RequiredLevel = 30
        });
        
        // Light Constellations (Orion, Phoenix, Sirius)
        AddConstellation(new ConstellationData.Constellation
        {
            Id = "orion",
            Name = "Orion",
            Description = "The Hunter - Grants legendary combat mastery",
            Type = ConstellationData.ConstellationType.Light,
            Rarity = ConstellationData.ConstellationRarity.Legendary,
            Stars = 9,
            AttackBonus = 0.35f,
            DefenseBonus = 0.20f,
            HealthBonus = 0.20f,
            SpeedBonus = 0.18f,
            CriticalBonus = 0.25f,
            EvasionBonus = 0.15f,
            GoldBonus = 0.20f,
            ExpBonus = 0.25f,
            UnlockCost = 2500,
            RequiredLevel = 45
        });
        
        AddConstellation(new ConstellationData.Constellation
        {
            Id = "phoenix",
            Name = "Phoenix",
            Description = "The Immortal Bird - Grants rebirth and resilience",
            Type = ConstellationData.ConstellationType.Light,
            Rarity = ConstellationData.ConstellationRarity.Legendary,
            Stars = 9,
            AttackBonus = 0.20f,
            DefenseBonus = 0.25f,
            HealthBonus = 0.35f,
            SpeedBonus = 0.15f,
            CriticalBonus = 0.15f,
            EvasionBonus = 0.20f,
            GoldBonus = 0.15f,
            ExpBonus = 0.30f,
            UnlockCost = 2500,
            RequiredLevel = 45
        });
        
        AddConstellation(new ConstellationData.Constellation
        {
            Id = "sirius",
            Name = "Sirius",
            Description = "The Bright Star - Grants celestial power",
            Type = ConstellationData.ConstellationType.Light,
            Rarity = ConstellationData.ConstellationRarity.Legendary,
            Stars = 9,
            AttackBonus = 0.25f,
            DefenseBonus = 0.18f,
            HealthBonus = 0.18f,
            SpeedBonus = 0.25f,
            CriticalBonus = 0.20f,
            EvasionBonus = 0.25f,
            GoldBonus = 0.25f,
            ExpBonus = 0.28f,
            UnlockCost = 2500,
            RequiredLevel = 45
        });
        
        // Dark Constellations (Shadow, Void, Eclipse)
        AddConstellation(new ConstellationData.Constellation
        {
            Id = "shadow",
            Name = "Shadow",
            Description = "The Dark Realm - Grants stealth and cunning",
            Type = ConstellationData.ConstellationType.Dark,
            Rarity = ConstellationData.ConstellationRarity.Legendary,
            Stars = 9,
            AttackBonus = 0.30f,
            DefenseBonus = 0.10f,
            HealthBonus = 0.15f,
            SpeedBonus = 0.25f,
            CriticalBonus = 0.30f,
            EvasionBonus = 0.25f,
            GoldBonus = 0.25f,
            ExpBonus = 0.20f,
            UnlockCost = 2500,
            RequiredLevel = 45
        });
        
        AddConstellation(new ConstellationData.Constellation
        {
            Id = "void",
            Name = "Void",
            Description = "The Empty Abyss - Grants mysterious power",
            Type = ConstellationData.ConstellationType.Dark,
            Rarity = ConstellationData.ConstellationRarity.Legendary,
            Stars = 9,
            AttackBonus = 0.25f,
            DefenseBonus = 0.25f,
            HealthBonus = 0.25f,
            SpeedBonus = 0.10f,
            CriticalBonus = 0.20f,
            EvasionBonus = 0.10f,
            GoldBonus = 0.30f,
            ExpBonus = 0.25f,
            UnlockCost = 2500,
            RequiredLevel = 45
        });
        
        AddConstellation(new ConstellationData.Constellation
        {
            Id = "eclipse",
            Name = "Eclipse",
            Description = "The Celestial Event - Grants ultimate power",
            Type = ConstellationData.ConstellationType.Dark,
            Rarity = ConstellationData.ConstellationRarity.Legendary,
            Stars = 9,
            AttackBonus = 0.28f,
            DefenseBonus = 0.28f,
            HealthBonus = 0.28f,
            SpeedBonus = 0.20f,
            CriticalBonus = 0.22f,
            EvasionBonus = 0.18f,
            GoldBonus = 0.22f,
            ExpBonus = 0.30f,
            UnlockCost = 3000,
            RequiredLevel = 50
        });
    }
    
    private void AddConstellation(ConstellationData.Constellation constellation)
    {
        _constellations[constellation.Id] = constellation;
    }
    
    public ConstellationData.Constellation GetConstellation(string id)
    {
        if (_constellations.ContainsKey(id))
            return _constellations[id];
        return null;
    }
    
    public Dictionary<string, ConstellationData.Constellation> GetAllConstellations()
    {
        return new Dictionary<string, ConstellationData.Constellation>(_constellations);
    }
    
    public Dictionary<string, ConstellationData.Constellation> GetConstellationsByType(ConstellationData.ConstellationType type)
    {
        Dictionary<string, ConstellationData.Constellation> result = new Dictionary<string, ConstellationData.Constellation>();
        foreach (var kvp in _constellations)
        {
            if (kvp.Value.Type == type)
                result[kvp.Key] = kvp.Value;
        }
        return result;
    }
    
    public Dictionary<string, ConstellationData.Constellation> GetConstellationsByRarity(ConstellationData.ConstellationRarity rarity)
    {
        Dictionary<string, ConstellationData.Constellation> result = new Dictionary<string, ConstellationData.Constellation>();
        foreach (var kvp in _constellations)
        {
            if (kvp.Value.Rarity == rarity)
                result[kvp.Key] = kvp.Value;
        }
        return result;
    }
    
    public List<ConstellationData.Constellation> GetAvailableConstellations(int playerLevel)
    {
        List<ConstellationData.Constellation> result = new List<ConstellationData.Constellation>();
        foreach (var kvp in _constellations)
        {
            if (kvp.Value.RequiredLevel <= playerLevel)
                result.Add(kvp.Value);
        }
        return result;
    }
}
