using Godot;
/// <summary>
/// 元素试炼数据。
/// </summary>
using System;
using System.Collections.Generic;

/// <summary>
/// 元素试炼数据 - 存储试炼关卡配置
/// </summary>
public class ElementalTrialData
{
    public enum TrialType
    {
        FireTrial,
        IceTrial,
        LightningTrial,
        DarkTrial,
        HolyTrial,
        NatureTrial,
        MixedTrial
    }

    public enum TrialDifficulty
    {
        Easy,
        Normal,
        Hard,
        Epic,
        Legendary
    }

    public string TrialId { get; set; }
    public string TrialName { get; set; }
    public string Description { get; set; }
    public TrialType Type { get; set; }
    public TrialDifficulty Difficulty { get; set; }
    public int RecommendedLevel { get; set; }
    public int WaveCount { get; set; }
    public int GoldReward { get; set; }
    public int ExpReward { get; set; }
    public List<string> ItemRewards { get; set; }
    public List<string> EnemyIds { get; set; }
    public float EnemyHealthMultiplier { get; set; }
    public float EnemyDamageMultiplier { get; set; }
    public float TimeLimit { get; set; }
    public bool IsUnlocked { get; set; }
    public int BestWave { get; set; }
    public bool IsCompleted { get; set; }

    public ElementalTrialData()
    {
        ItemRewards = new List<string>();
        EnemyIds = new List<string>();
    }
}

public class ElementalTrialDatabase
{
    private static ElementalTrialDatabase _instance;
    public static ElementalTrialDatabase Instance
    {
        get
        {
            if (_instance == null) _instance = new ElementalTrialDatabase();
            return _instance;
        }
    }

    private List<ElementalTrialData> _trials;

    public ElementalTrialDatabase()
    {
        _trials = new List<ElementalTrialData>();
        InitializeTrials();
    }

    private void InitializeTrials()
    {
        // Fire Trial - 火焰试炼
        AddTrial(new ElementalTrialData
        {
            TrialId = "fire_trial_easy",
            TrialName = "火焰试炼·入门",
            Description = "面对火焰元素的考验",
            Type = ElementalTrialData.TrialType.FireTrial,
            Difficulty = ElementalTrialData.TrialDifficulty.Easy,
            RecommendedLevel = 10,
            WaveCount = 3,
            GoldReward = 500,
            ExpReward = 200,
            ItemRewards = new List<string> { "fire_orb", "fire_scroll" },
            EnemyIds = new List<string> { "fire_sprite", "magma_golem" },
            EnemyHealthMultiplier = 1.0f,
            EnemyDamageMultiplier = 1.0f,
            TimeLimit = 180,
            IsUnlocked = true
        });

        AddTrial(new ElementalTrialData
        {
            TrialId = "fire_trial_normal",
            TrialName = "火焰试炼·进阶",
            Description = "更强大的火焰敌人",
            Type = ElementalTrialData.TrialType.FireTrial,
            Difficulty = ElementalTrialData.TrialDifficulty.Normal,
            RecommendedLevel = 25,
            WaveCount = 5,
            GoldReward = 1500,
            ExpReward = 800,
            ItemRewards = new List<string> { "fire_orb", "flame_sword" },
            EnemyIds = new List<string> { "fire_sprite", "magma_golem", "fire_elemental" },
            EnemyHealthMultiplier = 1.5f,
            EnemyDamageMultiplier = 1.5f,
            TimeLimit = 300,
            IsUnlocked = false
        });

        AddTrial(new ElementalTrialData
        {
            TrialId = "fire_trial_hard",
            TrialName = "火焰试炼·大师",
            Description = "火焰王者等你挑战",
            Type = ElementalTrialData.TrialType.FireTrial,
            Difficulty = ElementalTrialData.TrialDifficulty.Hard,
            RecommendedLevel = 45,
            WaveCount = 7,
            GoldReward = 5000,
            ExpReward = 3000,
            ItemRewards = new List<string> { "fire_orb", "flame_armor", "phoenix_feather" },
            EnemyIds = new List<string> { "fire_elemental", "fire_dragon", "inferno_lord" },
            EnemyHealthMultiplier = 2.5f,
            EnemyDamageMultiplier = 2.5f,
            TimeLimit = 420,
            IsUnlocked = false
        });

        // Ice Trial - 冰霜试炼
        AddTrial(new ElementalTrialData
        {
            TrialId = "ice_trial_easy",
            TrialName = "冰霜试炼·入门",
            Description = "面对冰霜元素的考验",
            Type = ElementalTrialData.TrialType.IceTrial,
            Difficulty = ElementalTrialData.TrialDifficulty.Easy,
            RecommendedLevel = 10,
            WaveCount = 3,
            GoldReward = 500,
            ExpReward = 200,
            ItemRewards = new List<string> { "ice_crystal", "frost_scroll" },
            EnemyIds = new List<string> { "ice_sprite", "frost_golem" },
            EnemyHealthMultiplier = 1.0f,
            EnemyDamageMultiplier = 1.0f,
            TimeLimit = 180,
            IsUnlocked = true
        });

        AddTrial(new ElementalTrialData
        {
            TrialId = "ice_trial_normal",
            TrialName = "冰霜试炼·进阶",
            Description = "更强大的冰霜敌人",
            Type = ElementalTrialData.TrialType.IceTrial,
            Difficulty = ElementalTrialData.TrialDifficulty.Normal,
            RecommendedLevel = 25,
            WaveCount = 5,
            GoldReward = 1500,
            ExpReward = 800,
            ItemRewards = new List<string> { "ice_crystal", "frost_blade" },
            EnemyIds = new List<string> { "ice_sprite", "frost_golem", "ice_elemental" },
            EnemyHealthMultiplier = 1.5f,
            EnemyDamageMultiplier = 1.5f,
            TimeLimit = 300,
            IsUnlocked = false
        });

        AddTrial(new ElementalTrialData
        {
            TrialId = "ice_trial_hard",
            TrialName = "冰霜试炼·大师",
            Description = "冰霜之王等你挑战",
            Type = ElementalTrialData.TrialType.IceTrial,
            Difficulty = ElementalTrialData.TrialDifficulty.Hard,
            RecommendedLevel = 45,
            WaveCount = 7,
            GoldReward = 5000,
            ExpReward = 3000,
            ItemRewards = new List<string> { "ice_crystal", "frost_armor", "winter_crown" },
            EnemyIds = new List<string> { "ice_elemental", "ice_dragon", "frost_king" },
            EnemyHealthMultiplier = 2.5f,
            EnemyDamageMultiplier = 2.5f,
            TimeLimit = 420,
            IsUnlocked = false
        });

        // Lightning Trial - 雷电试炼
        AddTrial(new ElementalTrialData
        {
            TrialId = "lightning_trial_easy",
            TrialName = "雷电试炼·入门",
            Description = "面对雷电元素的考验",
            Type = ElementalTrialData.TrialType.LightningTrial,
            Difficulty = ElementalTrialData.TrialDifficulty.Easy,
            RecommendedLevel = 15,
            WaveCount = 3,
            GoldReward = 600,
            ExpReward = 250,
            ItemRewards = new List<string> { "lightning_orb", "thunder_scroll" },
            EnemyIds = new List<string> { "lightning_sprite", "storm_golem" },
            EnemyHealthMultiplier = 1.0f,
            EnemyDamageMultiplier = 1.2f,
            TimeLimit = 150,
            IsUnlocked = true
        });

        AddTrial(new ElementalTrialData
        {
            TrialId = "lightning_trial_normal",
            TrialName = "雷电试炼·进阶",
            Description = "更强大的雷电敌人",
            Type = ElementalTrialData.TrialType.LightningTrial,
            Difficulty = ElementalTrialData.TrialDifficulty.Normal,
            RecommendedLevel = 30,
            WaveCount = 5,
            GoldReward = 2000,
            ExpReward = 1000,
            ItemRewards = new List<string> { "lightning_orb", "thunder_hammer" },
            EnemyIds = new List<string> { "lightning_sprite", "storm_golem", "thunder_elemental" },
            EnemyHealthMultiplier = 1.5f,
            EnemyDamageMultiplier = 1.8f,
            TimeLimit = 280,
            IsUnlocked = false
        });

        // Dark Trial - 黑暗试炼
        AddTrial(new ElementalTrialData
        {
            TrialId = "dark_trial_easy",
            TrialName = "黑暗试炼·入门",
            Description = "面对黑暗元素的考验",
            Type = ElementalTrialData.TrialType.DarkTrial,
            Difficulty = ElementalTrialData.TrialDifficulty.Easy,
            RecommendedLevel = 20,
            WaveCount = 3,
            GoldReward = 700,
            ExpReward = 300,
            ItemRewards = new List<string> { "dark_orb", "shadow_scroll" },
            EnemyIds = new List<string> { "shadow_sprite", "dark_golem" },
            EnemyHealthMultiplier = 1.2f,
            EnemyDamageMultiplier = 1.2f,
            TimeLimit = 180,
            IsUnlocked = true
        });

        // Holy Trial - 神圣试炼
        AddTrial(new ElementalTrialData
        {
            TrialId = "holy_trial_easy",
            TrialName = "神圣试炼·入门",
            Description = "面对神圣元素的考验",
            Type = ElementalTrialData.TrialType.HolyTrial,
            Difficulty = ElementalTrialData.TrialDifficulty.Easy,
            RecommendedLevel = 20,
            WaveCount = 3,
            GoldReward = 700,
            ExpReward = 300,
            ItemRewards = new List<string> { "holy_orb", "blessing_scroll" },
            EnemyIds = new List<string> { "corrupted_sprite", "evil_golem" },
            EnemyHealthMultiplier = 1.2f,
            EnemyDamageMultiplier = 1.2f,
            TimeLimit = 180,
            IsUnlocked = true
        });

        // Nature Trial - 自然试炼
        AddTrial(new ElementalTrialData
        {
            TrialId = "nature_trial_easy",
            TrialName = "自然试炼·入门",
            Description = "面对自然元素的考验",
            Type = ElementalTrialData.TrialType.NatureTrial,
            Difficulty = ElementalTrialData.TrialDifficulty.Easy,
            RecommendedLevel = 15,
            WaveCount = 3,
            GoldReward = 600,
            ExpReward = 250,
            ItemRewards = new List<string> { "nature_orb", "growth_scroll" },
            EnemyIds = new List<string> { "nature_sprite", "vine_golem" },
            EnemyHealthMultiplier = 1.0f,
            EnemyDamageMultiplier = 1.1f,
            TimeLimit = 160,
            IsUnlocked = true
        });

        // Mixed Trial - 混合试炼
        AddTrial(new ElementalTrialData
        {
            TrialId = "mixed_trial_epic",
            TrialName = "元素融合试炼",
            Description = "同时面对多种元素的考验",
            Type = ElementalTrialData.TrialType.MixedTrial,
            Difficulty = ElementalTrialData.TrialDifficulty.Epic,
            RecommendedLevel = 50,
            WaveCount = 10,
            GoldReward = 10000,
            ExpReward = 5000,
            ItemRewards = new List<string> { "element_essence", "legendary_armor" },
            EnemyIds = new List<string> { "fire_elemental", "ice_elemental", "lightning_elemental", "thunder_dragon" },
            EnemyHealthMultiplier = 3.0f,
            EnemyDamageMultiplier = 3.0f,
            TimeLimit = 600,
            IsUnlocked = false
        });

        AddTrial(new ElementalTrialData
        {
            TrialId = "mixed_trial_legendary",
            TrialName = "终极元素试炼",
            Description = "只有真正的强者才能通过",
            Type = ElementalTrialData.TrialType.MixedTrial,
            Difficulty = ElementalTrialData.TrialDifficulty.Legendary,
            RecommendedLevel = 60,
            WaveCount = 15,
            GoldReward = 50000,
            ExpReward = 20000,
            ItemRewards = new List<string> { "element_essence", "god_weapon", "legendary_armor", "ancient_relic" },
            EnemyIds = new List<string> { "inferno_lord", "frost_king", "thunder_dragon", "shadow_king", "light_avatar", "element_chaos" },
            EnemyHealthMultiplier = 5.0f,
            EnemyDamageMultiplier = 5.0f,
            TimeLimit = 900,
            IsUnlocked = false
        });
    }

    private void AddTrial(ElementalTrialData trial)
    {
        _trials.Add(trial);
    }

    public List<ElementalTrialData> GetAllTrials()
    {
        return new List<ElementalTrialData>(_trials);
    }

    public List<ElementalTrialData> GetUnlockedTrials()
    {
        return _trials.FindAll(t => t.IsUnlocked);
    }

    public List<ElementalTrialData> GetTrialsByType(ElementalTrialData.TrialType type)
    {
        return _trials.FindAll(t => t.Type == type);
    }

    public ElementalTrialData GetTrial(string trialId)
    {
        return _trials.Find(t => t.TrialId == trialId);
    }

    public void UnlockTrial(string trialId)
    {
        var trial = GetTrial(trialId);
        if (trial != null)
        {
            trial.IsUnlocked = true;
        }
    }
}
