using System;
using System.Collections.Generic;

public class MomentumDatabase
{
    private static MomentumDatabase _instance;
    public static MomentumDatabase Instance => _instance ?? (_instance = new MomentumDatabase());
    
    public Dictionary<MomentumData.MomentumType, MomentumData.MomentumConfig> MomentumConfigs { get; private set; }
    
    public MomentumDatabase()
    {
        MomentumConfigs = new Dictionary<MomentumData.MomentumType, MomentumData.MomentumConfig>();
        InitializeMomentumConfigs();
    }
    
    private void InitializeMomentumConfigs()
    {
        // Attack Momentum - Increases damage with consecutive kills
        var attackMomentum = new MomentumData.MomentumConfig
        {
            Type = MomentumData.MomentumType.Attack,
            Name = "战斗 momentum",
            Description = "连续击杀敌人来积累攻击力 momentum",
            MaxCharge = 100f,
            DecayRate = 5f,
            ChargePerKill = 25f,
            ChargePerHit = 2f,
            ChargePerSecond = 1f,
            MaxLevel = 5,
            StateMultipliers = new Dictionary<MomentumData.MomentumState, Dictionary<string, float>>
            {
                { MomentumData.MomentumState.Neutral, new Dictionary<string, float> { { "damage", 1.0f } } },
                { MomentumData.MomentumState.Building, new Dictionary<string, float> { { "damage", 1.1f } } },
                { MomentumData.MomentumState.Charged, new Dictionary<string, float> { { "damage", 1.25f } } },
                { MomentumData.MomentumState.Overcharged, new Dictionary<string, float> { { "damage", 1.5f } } },
                { MomentumData.MomentumState.Fading, new Dictionary<string, float> { { "damage", 1.05f } } }
            },
            AttributeBonuses = new Dictionary<string, float>
            {
                { "damage", 0.1f }
            }
        };
        MomentumConfigs[MomentumData.MomentumType.Attack] = attackMomentum;
        
        // Defense Momentum - Increases defense with consecutive hits taken
        var defenseMomentum = new MomentumData.MomentumConfig
        {
            Type = MomentumData.MomentumType.Defense,
            Name = "防御 momentum",
            Description = "承受攻击来积累防御力 momentum",
            MaxCharge = 100f,
            DecayRate = 3f,
            ChargePerKill = 15f,
            ChargePerHit = 10f,
            ChargePerSecond = 0.8f,
            MaxLevel = 5,
            StateMultipliers = new Dictionary<MomentumData.MomentumState, Dictionary<string, float>>
            {
                { MomentumData.MomentumState.Neutral, new Dictionary<string, float> { { "defense", 1.0f } } },
                { MomentumData.MomentumState.Building, new Dictionary<string, float> { { "defense", 1.1f } } },
                { MomentumData.MomentumState.Charged, new Dictionary<string, float> { { "defense", 1.2f } } },
                { MomentumData.MomentumState.Overcharged, new Dictionary<string, float> { { "defense", 1.4f } } },
                { MomentumData.MomentumState.Fading, new Dictionary<string, float> { { "defense", 1.05f } } }
            },
            AttributeBonuses = new Dictionary<string, float>
            {
                { "defense", 0.08f }
            }
        };
        MomentumConfigs[MomentumData.MomentumType.Defense] = defenseMomentum;
        
        // Speed Momentum - Increases attack speed with consecutive actions
        var speedMomentum = new MomentumData.MomentumConfig
        {
            Type = MomentumData.MomentumType.Speed,
            Name = "速度 momentum",
            Description = "快速行动来积累速度 momentum",
            MaxCharge = 100f,
            DecayRate = 4f,
            ChargePerKill = 20f,
            ChargePerHit = 3f,
            ChargePerSecond = 1.5f,
            MaxLevel = 5,
            StateMultipliers = new Dictionary<MomentumData.MomentumState, Dictionary<string, float>>
            {
                { MomentumData.MomentumState.Neutral, new Dictionary<string, float> { { "attackSpeed", 1.0f } } },
                { MomentumData.MomentumState.Building, new Dictionary<string, float> { { "attackSpeed", 1.05f } } },
                { MomentumData.MomentumState.Charged, new Dictionary<string, float> { { "attackSpeed", 1.15f } } },
                { MomentumData.MomentumState.Overcharged, new Dictionary<string, float> { { "attackSpeed", 1.3f } } },
                { MomentumData.MomentumState.Fading, new Dictionary<string, float> { { "attackSpeed", 1.02f } } }
            },
            AttributeBonuses = new Dictionary<string, float>
            {
                { "attackSpeed", 0.06f }
            }
        };
        MomentumConfigs[MomentumData.MomentumType.Speed] = speedMomentum;
        
        // Luck Momentum - Increases drop rate and critical chance
        var luckMomentum = new MomentumData.MomentumConfig
        {
            Type = MomentumData.MomentumType.Luck,
            Name = "幸运 momentum",
            Description = "击杀敌人来积累幸运 momentum",
            MaxCharge = 100f,
            DecayRate = 2f,
            ChargePerKill = 30f,
            ChargePerHit = 1f,
            ChargePerSecond = 0.5f,
            MaxLevel = 5,
            StateMultipliers = new Dictionary<MomentumData.MomentumState, Dictionary<string, float>>
            {
                { MomentumData.MomentumState.Neutral, new Dictionary<string, float> { { "luck", 1.0f } } },
                { MomentumData.MomentumState.Building, new Dictionary<string, float> { { "luck", 1.15f } } },
                { MomentumData.MomentumState.Charged, new Dictionary<string, float> { { "luck", 1.3f } } },
                { MomentumData.MomentumState.Overcharged, new Dictionary<string, float> { { "luck", 1.5f } } },
                { MomentumData.MomentumState.Fading, new Dictionary<string, float> { { "luck", 1.08f } } }
            },
            AttributeBonuses = new Dictionary<string, float>
            {
                { "dropRate", 0.1f },
                { "critChance", 0.02f }
            }
        };
        MomentumConfigs[MomentumData.MomentumType.Luck] = luckMomentum;
        
        // Critical Momentum - Increases critical damage
        var criticalMomentum = new MomentumData.MomentumConfig
        {
            Type = MomentumData.MomentumType.Critical,
            Name = "暴击 momentum",
            Description = "暴击敌人来积累暴击 momentum",
            MaxCharge = 100f,
            DecayRate = 3f,
            ChargePerKill = 20f,
            ChargePerHit = 5f,
            ChargePerSecond = 0.6f,
            MaxLevel = 5,
            StateMultipliers = new Dictionary<MomentumData.MomentumState, Dictionary<string, float>>
            {
                { MomentumData.MomentumState.Neutral, new Dictionary<string, float> { { "critDamage", 1.0f } } },
                { MomentumData.MomentumState.Building, new Dictionary<string, float> { { "critDamage", 1.1f } } },
                { MomentumData.MomentumState.Charged, new Dictionary<string, float> { { "critDamage", 1.25f } } },
                { MomentumData.MomentumState.Overcharged, new Dictionary<string, float> { { "critDamage", 1.5f } } },
                { MomentumData.MomentumState.Fading, new Dictionary<string, float> { { "critDamage", 1.05f } } }
            },
            AttributeBonuses = new Dictionary<string, float>
            {
                { "critDamage", 0.1f }
            }
        };
        MomentumConfigs[MomentumData.MomentumType.Critical] = criticalMomentum;
    }
    
    public MomentumData.MomentumConfig GetConfig(MomentumData.MomentumType type)
    {
        return MomentumConfigs.ContainsKey(type) ? MomentumConfigs[type] : null;
    }
    
    public float GetStateMultiplier(MomentumData.MomentumType type, MomentumData.MomentumState state, string attribute)
    {
        var config = GetConfig(type);
        if (config == null || !config.StateMultipliers.ContainsKey(state))
            return 1.0f;
            
        return config.StateMultipliers[state].ContainsKey(attribute) 
            ? config.StateMultipliers[state][attribute] 
            : 1.0f;
    }
}
