using Godot;
using System;
using System.Collections.Generic;
using Framework;

public partial class ConstellationDatabase : BaseSystem
{
    // All constellation configurations
    private Dictionary<string, ConstellationSystem.Constellation> _constellations = new Dictionary<string, ConstellationSystem.Constellation>();
    
    // 已解锁的星座（用于持久化）
    private HashSet<string> _unlockedConstellations = new HashSet<string>();
    
    public override void _Ready()
    {
        InitializeConstellations();
    }
    
    private void InitializeConstellations()
    {
        // Fire Constellations (Aries, Leo, Sagittarius)
        AddConstellation(new ConstellationSystem.Constellation
        {
            Id = "aries",
            Name = "Aries",
            Description = "The Ram - Grants fiery passion and combat prowess",
            Type = ConstellationSystem.ConstellationType.Fire,
            Rarity = ConstellationSystem.ConstellationRarity.Common,
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
        
        AddConstellation(new ConstellationSystem.Constellation
        {
            Id = "leo",
            Name = "Leo",
            Description = "The Lion - The king of beasts grants royal strength",
            Type = ConstellationSystem.ConstellationType.Fire,
            Rarity = ConstellationSystem.ConstellationRarity.Rare,
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
        
        AddConstellation(new ConstellationSystem.Constellation
        {
            Id = "sagittarius",
            Name = "Sagittarius",
            Description = "The Archer - Seeks truth and grants accuracy",
            Type = ConstellationSystem.ConstellationType.Fire,
            Rarity = ConstellationSystem.ConstellationRarity.Epic,
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
        AddConstellation(new ConstellationSystem.Constellation
        {
            Id = "cancer",
            Name = "Cancer",
            Description = "The Crab - Grants defensive resilience",
            Type = ConstellationSystem.ConstellationType.Water,
            Rarity = ConstellationSystem.ConstellationRarity.Common,
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
        
        AddConstellation(new ConstellationSystem.Constellation
        {
            Id = "scorpio",
            Name = "Scorpio",
            Description = "The Scorpion - Grants deadly precision",
            Type = ConstellationSystem.ConstellationType.Water,
            Rarity = ConstellationSystem.ConstellationRarity.Rare,
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
        
        AddConstellation(new ConstellationSystem.Constellation
        {
            Id = "pisces",
            Name = "Pisces",
            Description = "The Fish - Grants mystical wisdom",
            Type = ConstellationSystem.ConstellationType.Water,
            Rarity = ConstellationSystem.ConstellationRarity.Epic,
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
        AddConstellation(new ConstellationSystem.Constellation
        {
            Id = "taurus",
            Name = "Taurus",
            Description = "The Bull - Grants unyielding strength",
            Type = ConstellationSystem.ConstellationType.Earth,
            Rarity = ConstellationSystem.ConstellationRarity.Common,
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
        
        AddConstellation(new ConstellationSystem.Constellation
        {
            Id = "virgo",
            Name = "Virgo",
            Description = "The Maiden - Grants analytical precision",
            Type = ConstellationSystem.ConstellationType.Earth,
            Rarity = ConstellationSystem.ConstellationRarity.Rare,
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
        
        AddConstellation(new ConstellationSystem.Constellation
        {
            Id = "capricorn",
            Name = "Capricorn",
            Description = "The Goat - Grants ambitious determination",
            Type = ConstellationSystem.ConstellationType.Earth,
            Rarity = ConstellationSystem.ConstellationRarity.Epic,
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
        AddConstellation(new ConstellationSystem.Constellation
        {
            Id = "gemini",
            Name = "Gemini",
            Description = "The Twins - Grants versatile agility",
            Type = ConstellationSystem.ConstellationType.Air,
            Rarity = ConstellationSystem.ConstellationRarity.Common,
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
        
        AddConstellation(new ConstellationSystem.Constellation
        {
            Id = "libra",
            Name = "Libra",
            Description = "The Scales - Grants balance and harmony",
            Type = ConstellationSystem.ConstellationType.Air,
            Rarity = ConstellationSystem.ConstellationRarity.Rare,
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
        
        AddConstellation(new ConstellationSystem.Constellation
        {
            Id = "aquarius",
            Name = "Aquarius",
            Description = "The Water Bearer - Grants innovative wisdom",
            Type = ConstellationSystem.ConstellationType.Air,
            Rarity = ConstellationSystem.ConstellationRarity.Epic,
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
        AddConstellation(new ConstellationSystem.Constellation
        {
            Id = "orion",
            Name = "Orion",
            Description = "The Hunter - Grants legendary combat mastery",
            Type = ConstellationSystem.ConstellationType.Light,
            Rarity = ConstellationSystem.ConstellationRarity.Legendary,
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
        
        AddConstellation(new ConstellationSystem.Constellation
        {
            Id = "phoenix",
            Name = "Phoenix",
            Description = "The Immortal Bird - Grants rebirth and resilience",
            Type = ConstellationSystem.ConstellationType.Light,
            Rarity = ConstellationSystem.ConstellationRarity.Legendary,
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
        
        AddConstellation(new ConstellationSystem.Constellation
        {
            Id = "sirius",
            Name = "Sirius",
            Description = "The Bright Star - Grants celestial power",
            Type = ConstellationSystem.ConstellationType.Light,
            Rarity = ConstellationSystem.ConstellationRarity.Legendary,
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
        AddConstellation(new ConstellationSystem.Constellation
        {
            Id = "shadow",
            Name = "Shadow",
            Description = "The Dark Realm - Grants stealth and cunning",
            Type = ConstellationSystem.ConstellationType.Dark,
            Rarity = ConstellationSystem.ConstellationRarity.Legendary,
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
        
        AddConstellation(new ConstellationSystem.Constellation
        {
            Id = "void",
            Name = "Void",
            Description = "The Empty Abyss - Grants mysterious power",
            Type = ConstellationSystem.ConstellationType.Dark,
            Rarity = ConstellationSystem.ConstellationRarity.Legendary,
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
        
        AddConstellation(new ConstellationSystem.Constellation
        {
            Id = "eclipse",
            Name = "Eclipse",
            Description = "The Celestial Event - Grants ultimate power",
            Type = ConstellationSystem.ConstellationType.Dark,
            Rarity = ConstellationSystem.ConstellationRarity.Legendary,
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
    
    private void AddConstellation(ConstellationSystem.Constellation constellation)
    {
        _constellations[constellation.Id] = constellation;
    }
    
    public ConstellationSystem.Constellation GetConstellation(string id)
    {
        if (_constellations.ContainsKey(id))
            return _constellations[id];
        return null;
    }
    
    public Dictionary<string, ConstellationSystem.Constellation> GetAllConstellations()
    {
        return new Dictionary<string, ConstellationSystem.Constellation>(_constellations);
    }
    
    public Dictionary<string, ConstellationSystem.Constellation> GetConstellationsByType(ConstellationSystem.ConstellationType type)
    {
        Dictionary<string, ConstellationSystem.Constellation> result = new Dictionary<string, ConstellationSystem.Constellation>();
        foreach (var kvp in _constellations)
        {
            if (kvp.Value.Type == type)
                result[kvp.Key] = kvp.Value;
        }
        return result;
    }
    
    public Dictionary<string, ConstellationSystem.Constellation> GetConstellationsByRarity(ConstellationSystem.ConstellationRarity rarity)
    {
        Dictionary<string, ConstellationSystem.Constellation> result = new Dictionary<string, ConstellationSystem.Constellation>();
        foreach (var kvp in _constellations)
        {
            if (kvp.Value.Rarity == rarity)
                result[kvp.Key] = kvp.Value;
        }
        return result;
    }
    
    public List<ConstellationSystem.Constellation> GetAvailableConstellations(int playerLevel)
    {
        List<ConstellationSystem.Constellation> result = new List<ConstellationSystem.Constellation>();
        foreach (var kvp in _constellations)
        {
            if (kvp.Value.RequiredLevel <= playerLevel)
                result.Add(kvp.Value);
        }
        return result;
    }

    // 持久化方法
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        // ConstellationDatabase 主要存储静态配置数据，不需要持久化玩家状态
        // 如需持久化玩家解锁状态，需要添加额外字段
        data["constellations"] = _constellations;
        return data;
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        // 加载配置数据
        if (data.ContainsKey("constellations"))
        {
            // 如有需要可在此处理玩家解锁状态
        }
    }
}
