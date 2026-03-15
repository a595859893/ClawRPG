using Godot;
using System.Collections.Generic;

/// <summary>
/// 每日仪式数据库 - 配置仪式数据
/// </summary>
public class DailyRitualDatabase
{
    private static DailyRitualDatabase _instance;
    public static DailyRitualDatabase Instance => _instance ??= new DailyRitualDatabase();

    public Dictionary<string, RitualData> Rituals { get; private set; }

    public DailyRitualDatabase()
    {
        Rituals = new Dictionary<string, RitualData>();
        InitializeRituals();
    }

    private void InitializeRituals()
    {
        // Morning Prayer
        AddRitual(new RitualData
        {
            Id = "morning_prayer_novice",
            Name = "Morning Prayer",
            Description = "Start your day with divine blessings",
            Type = RitualType.MorningPrayer,
            Tier = RitualTier.Novice,
            GoldCost = 100,
            AttributeBonuses = new Dictionary<string, float>
            {
                { "attack", 0.02f },
                { "defense", 0.02f }
            },
            Duration = 60f,
            ReputationGain = 10
        });

        AddRitual(new RitualData
        {
            Id = "morning_prayer_adept",
            Name = "Morning Prayer",
            Description = "Enhanced morning devotion",
            Type = RitualType.MorningPrayer,
            Tier = RitualTier.Adept,
            GoldCost = 500,
            AttributeBonuses = new Dictionary<string, float>
            {
                { "attack", 0.05f },
                { "defense", 0.05f },
                { "exp", 0.03f }
            },
            Duration = 120f,
            ReputationGain = 25
        });

        AddRitual(new RitualData
        {
            Id = "morning_prayer_master",
            Name = "Morning Prayer",
            Description = "Mastery of morning devotion",
            Type = RitualType.MorningPrayer,
            Tier = RitualTier.Master,
            GoldCost = 2000,
            AttributeBonuses = new Dictionary<string, float>
            {
                { "attack", 0.08f },
                { "defense", 0.08f },
                { "exp", 0.05f },
                { "luck", 0.03f }
            },
            Duration = 180f,
            ReputationGain = 50
        });

        // Evening Meditation
        AddRitual(new RitualData
        {
            Id = "evening_meditation_novice",
            Name = "Evening Meditation",
            Description = "Reflect on the day's battles",
            Type = RitualType.EveningMeditation,
            Tier = RitualTier.Novice,
            GoldCost = 100,
            AttributeBonuses = new Dictionary<string, float>
            {
                { "health", 0.03f },
                { "defense", 0.02f }
            },
            Duration = 60f,
            ReputationGain = 10
        });

        AddRitual(new RitualData
        {
            Id = "evening_meditation_adept",
            Name = "Evening Meditation",
            Description = "Deep evening contemplation",
            Type = RitualType.EveningMeditation,
            Tier = RitualTier.Adept,
            GoldCost = 500,
            AttributeBonuses = new Dictionary<string, float>
            {
                { "health", 0.06f },
                { "defense", 0.05f },
                { "regen", 0.03f }
            },
            Duration = 120f,
            ReputationGain = 25
        });

        // Blessing of Fire
        AddRitual(new RitualData
        {
            Id = "blessing_fire_novice",
            Name = "Blessing of Fire",
            Description = "Invoke the power of flames",
            Type = RitualType.BlessingOfFire,
            Tier = RitualTier.Novice,
            GoldCost = 200,
            AttributeBonuses = new Dictionary<string, float>
            {
                { "attack", 0.03f },
                { "fire_damage", 0.05f }
            },
            Duration = 90f,
            ReputationGain = 15
        });

        AddRitual(new RitualData
        {
            Id = "blessing_fire_master",
            Name = "Blessing of Fire",
            Description = "Mastery over flames",
            Type = RitualType.BlessingOfFire,
            Tier = RitualTier.Master,
            GoldCost = 2500,
            AttributeBonuses = new Dictionary<string, float>
            {
                { "attack", 0.10f },
                { "fire_damage", 0.15f },
                { "crit_damage", 0.08f }
            },
            Duration = 240f,
            ReputationGain = 75
        });

        // Offering to Water
        AddRitual(new RitualData
        {
            Id = "offering_water_novice",
            Name = "Offering to Water",
            Description = "Honour the healing waters",
            Type = RitualType.OfferingToWater,
            Tier = RitualTier.Novice,
            GoldCost = 200,
            AttributeBonuses = new Dictionary<string, float>
            {
                { "health", 0.05f },
                { "water_damage", 0.03f }
            },
            Duration = 90f,
            ReputationGain = 15
        });

        // Tribute to Earth
        AddRitual(new RitualData
        {
            Id = "tribute_earth_novice",
            Name = "Tribute to Earth",
            Description = "Draw strength from the ground",
            Type = RitualType.TributeToEarth,
            Tier = RitualTier.Novice,
            GoldCost = 200,
            AttributeBonuses = new Dictionary<string, float>
            {
                { "defense", 0.05f },
                { "health", 0.03f }
            },
            Duration = 90f,
            ReputationGain = 15
        });

        // Wind Whisper
        AddRitual(new RitualData
        {
            Id = "wind_whisper_novice",
            Name = "Wind Whisper",
            Description = "Listen to the wind's secrets",
            Type = RitualType.WindWhisper,
            Tier = RitualTier.Novice,
            GoldCost = 200,
            AttributeBonuses = new Dictionary<string, float>
            {
                { "speed", 0.05f },
                { "dodge", 0.03f }
            },
            Duration = 90f,
            ReputationGain = 15
        });

        // Light Ceremony
        AddRitual(new RitualData
        {
            Id = "light_ceremony_novice",
            Name = "Light Ceremony",
            Description = "Embrace the power of light",
            Type = RitualType.LightCeremony,
            Tier = RitualTier.Novice,
            GoldCost = 300,
            AttributeBonuses = new Dictionary<string, float>
            {
                { "holy_damage", 0.05f },
                { "health", 0.03f }
            },
            Duration = 120f,
            ReputationGain = 20
        });

        AddRitual(new RitualData
        {
            Id = "light_ceremony_legendary",
            Name = "Light Ceremony",
            Description = "Transcend into pure light",
            Type = RitualType.LightCeremony,
            Tier = RitualTier.Legendary,
            GoldCost = 10000,
            AttributeBonuses = new Dictionary<string, float>
            {
                { "holy_damage", 0.20f },
                { "health", 0.15f },
                { "attack", 0.10f },
                { "exp", 0.10f }
            },
            Duration = 360f,
            ReputationGain = 200
        });

        // Shadow Ritual
        AddRitual(new RitualData
        {
            Id = "shadow_ritual_novice",
            Name = "Shadow Ritual",
            Description = "Embrace the darkness within",
            Type = RitualType.ShadowRitual,
            Tier = RitualTier.Novice,
            GoldCost = 300,
            AttributeBonuses = new Dictionary<string, float>
            {
                { "dark_damage", 0.05f },
                { "crit_rate", 0.03f }
            },
            Duration = 120f,
            ReputationGain = 20
        });

        // Blood Pact
        AddRitual(new RitualData
        {
            Id = "blood_pact_master",
            Name = "Blood Pact",
            Description = "A dark bargain for power",
            Type = RitualType.BloodPact,
            Tier = RitualTier.Master,
            GoldCost = 5000,
            AttributeBonuses = new Dictionary<string, float>
            {
                { "attack", 0.15f },
                { "lifesteal", 0.10f },
                { "crit_rate", 0.05f }
            },
            Duration = 300f,
            ReputationGain = 100
        });

        // Spirit Summon
        AddRitual(new RitualData
        {
            Id = "spirit_summon_legendary",
            Name = "Spirit Summon",
            Description = "Call upon ancient spirits",
            Type = RitualType.SpiritSummon,
            Tier = RitualTier.Legendary,
            GoldCost = 15000,
            AttributeBonuses = new Dictionary<string, float>
            {
                { "attack", 0.12f },
                { "defense", 0.12f },
                { "magic", 0.12f },
                { "speed", 0.08f },
                { "all_attributes", 0.05f }
            },
            Duration = 480f,
            ReputationGain = 300
        });
    }

    private void AddRitual(RitualData ritual)
    {
        Rituals[ritual.Id] = ritual;
    }

    public List<RitualData> GetRitualsByType(RitualType type)
    {
        var result = new List<RitualData>();
        foreach (var ritual in Rituals.Values)
        {
            if (ritual.Type == type)
                result.Add(ritual);
        }
        return result;
    }

    public List<RitualData> GetRitualsByTier(RitualTier tier)
    {
        var result = new List<RitualData>();
        foreach (var ritual in Rituals.Values)
        {
            if (ritual.Tier == tier)
                result.Add(ritual);
        }
        return result;
    }

    public RitualData GetRitual(string id)
    {
        return Rituals.ContainsKey(id) ? Rituals[id] : null;
    }

    public List<RitualData> GetAllRituals()
    {
        return new List<RitualData>(Rituals.Values);
    }
}
