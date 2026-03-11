using Godot;
using System;
using System.Collections.Generic;
using MountEvolutionDataSpace;

public class MountEvolutionDatabase
{
    private static MountEvolutionDatabase _instance;
    public static MountEvolutionDatabase Instance => _instance ??= new MountEvolutionDatabase();

    // 所有进化配置按坐骑ID索引
    private Dictionary<string, List<MountEvolutionData.MountEvolutionConfig>> _evolutionConfigs;
    
    // 快速查询：坐骑ID -> 可用的进化阶段
    private Dictionary<string, List<MountEvolutionData.EvolutionStage>> _availableStages;

    public MountEvolutionDatabase()
    {
        _evolutionConfigs = new Dictionary<string, List<MountEvolutionData.MountEvolutionConfig>>();
        _availableStages = new Dictionary<string, List<MountEvolutionData.EvolutionStage>>();
        InitializeEvolutions();
    }

    private void InitializeEvolutions()
    {
        // === 白马进化链 ===
        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "white_horse",
            BaseMountName = "白马",
            Stage = MountEvolutionData.EvolutionStage.Advanced,
            Type = MountEvolutionData.EvolutionType.Holy,
            EvolutionName = "圣光白马",
            Description = "沐浴圣光的白马进化形态，擅长辅助与治疗",
            RequiredLevel = 20,
            RequiredBattleExp = 1000,
            RequiredItemId = 1001, // 圣光之羽
            RequiredItemCount = 5,
            GoldCost = 500,
            HealthBonus = 0.15f,
            AttackBonus = 0.10f,
            DefenseBonus = 0.10f,
            SpeedBonus = 0.05f,
            UnlockSkills = new List<string> { "holy_charge" },
            TintColor = new Color(1f, 0.95f, 0.8f)
        });

        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "white_horse",
            BaseMountName = "白马",
            Stage = MountEvolutionData.EvolutionStage.Elite,
            Type = MountEvolutionData.EvolutionType.Holy,
            EvolutionName = "光辉天马",
            Description = "拥有翅膀的光辉天马，能够飞翔作战",
            RequiredLevel = 40,
            RequiredBattleExp = 5000,
            RequiredItemId = 1002, // 天使之羽
            RequiredItemCount = 10,
            GoldCost = 2000,
            HealthBonus = 0.25f,
            AttackBonus = 0.20f,
            DefenseBonus = 0.15f,
            SpeedBonus = 0.15f,
            UnlockSkills = new List<string> { "holy_charge", "divine_shield" },
            TintColor = new Color(1f, 0.9f, 0.7f)
        });

        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "white_horse",
            BaseMountName = "白马",
            Stage = MountEvolutionData.EvolutionStage.Epic,
            Type = MountEvolutionData.EvolutionType.Holy,
            EvolutionName = "光明神驹",
            Description = "光明之神降临的坐骑，散发神圣光芒",
            RequiredLevel = 60,
            RequiredBattleExp = 15000,
            RequiredItemId = 1003, // 光明神印
            RequiredItemCount = 5,
            GoldCost = 10000,
            HealthBonus = 0.40f,
            AttackBonus = 0.30f,
            DefenseBonus = 0.25f,
            SpeedBonus = 0.20f,
            UnlockSkills = new List<string> { "holy_charge", "divine_shield", "light_burst" },
            TintColor = new Color(1f, 1f, 0.9f)
        });

        // === 黑马进化链 ===
        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "black_horse",
            BaseMountName = "黑马",
            Stage = MountEvolutionData.EvolutionStage.Advanced,
            Type = MountEvolutionData.EvolutionType.Dark,
            EvolutionName = "暗影黑马",
            Description = "穿梭于暗影中的黑马，擅长隐匿与爆发",
            RequiredLevel = 20,
            RequiredBattleExp = 1000,
            RequiredItemId = 1011, // 暗影之石
            RequiredItemCount = 5,
            GoldCost = 500,
            HealthBonus = 0.10f,
            AttackBonus = 0.20f,
            DefenseBonus = 0.10f,
            SpeedBonus = 0.10f,
            UnlockSkills = new List<string> { "shadow_strike" },
            TintColor = new Color(0.6f, 0.6f, 0.7f)
        });

        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "black_horse",
            BaseMountName = "黑马",
            Stage = MountEvolutionData.EvolutionStage.Elite,
            Type = MountEvolutionData.EvolutionType.Dark,
            EvolutionName = "深渊梦魇",
            Description = "来自深渊的梦魇之马，掌控暗影之力",
            RequiredLevel = 40,
            RequiredBattleExp = 5000,
            RequiredItemId = 1012, // 深渊魔晶
            RequiredItemCount = 10,
            GoldCost = 2000,
            HealthBonus = 0.20f,
            AttackBonus = 0.30f,
            DefenseBonus = 0.15f,
            SpeedBonus = 0.15f,
            UnlockSkills = new List<string> { "shadow_strike", "dark_aura" },
            TintColor = new Color(0.4f, 0.4f, 0.5f)
        });

        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "black_horse",
            BaseMountName = "黑马",
            Stage = MountEvolutionData.EvolutionStage.Epic,
            Type = MountEvolutionData.EvolutionType.Dark,
            EvolutionName = "毁灭之源",
            Description = "蕴含毁灭之力的终极暗影坐骑",
            RequiredLevel = 60,
            RequiredBattleExp = 15000,
            RequiredItemId = 1013, // 毁灭本源
            RequiredItemCount = 5,
            GoldCost = 10000,
            HealthBonus = 0.35f,
            AttackBonus = 0.45f,
            DefenseBonus = 0.20f,
            SpeedBonus = 0.25f,
            UnlockSkills = new List<string> { "shadow_strike", "dark_aura", "annihilation" },
            TintColor = new Color(0.3f, 0.2f, 0.3f)
        });

        // === 雪狼进化链 ===
        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "snow_wolf",
            BaseMountName = "雪狼",
            Stage = MountEvolutionData.EvolutionStage.Advanced,
            Type = MountEvolutionData.EvolutionType.Ice,
            EvolutionName = "寒霜冰狼",
            Description = "掌控寒冰之力的冰狼，减缓敌人速度",
            RequiredLevel = 20,
            RequiredBattleExp = 1000,
            RequiredItemId = 1021, // 冰晶
            RequiredItemCount = 5,
            GoldCost = 500,
            HealthBonus = 0.12f,
            AttackBonus = 0.12f,
            DefenseBonus = 0.15f,
            SpeedBonus = 0.08f,
            UnlockSkills = new List<string> { "frost_bite" },
            TintColor = new Color(0.8f, 0.9f, 1f)
        });

        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "snow_wolf",
            BaseMountName = "雪狼",
            Stage = MountEvolutionData.EvolutionStage.Elite,
            Type = MountEvolutionData.EvolutionType.Ice,
            EvolutionName = "极地冰原狼",
            Description = "冰原上的绝对王者，冰雪的主宰",
            RequiredLevel = 40,
            RequiredBattleExp = 5000,
            RequiredItemId = 1022, // 永恒冰晶
            RequiredItemCount = 10,
            GoldCost = 2000,
            HealthBonus = 0.22f,
            AttackBonus = 0.22f,
            DefenseBonus = 0.25f,
            SpeedBonus = 0.12f,
            UnlockSkills = new List<string> { "frost_bite", "ice_barrier" },
            TintColor = new Color(0.7f, 0.85f, 1f)
        });

        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "snow_wolf",
            BaseMountName = "雪狼",
            Stage = MountEvolutionData.EvolutionStage.Epic,
            Type = MountEvolutionData.EvolutionType.Ice,
            EvolutionName = "冰封之主",
            Description = "冻结一切的冰雪帝王",
            RequiredLevel = 60,
            RequiredBattleExp = 15000,
            RequiredItemId = 1023, // 冰封王座
            RequiredItemCount = 5,
            GoldCost = 10000,
            HealthBonus = 0.35f,
            AttackBonus = 0.35f,
            DefenseBonus = 0.35f,
            SpeedBonus = 0.18f,
            UnlockSkills = new List<string> { "frost_bite", "ice_barrier", "blizzard" },
            TintColor = new Color(0.6f, 0.8f, 1f)
        });

        // === 棕熊进化链 ===
        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "brown_bear",
            BaseMountName = "棕熊",
            Stage = MountEvolutionData.EvolutionStage.Advanced,
            Type = MountEvolutionData.EvolutionType.Nature,
            EvolutionName = "钢甲棕熊",
            Description = "披上钢铁盔甲的棕熊，防御力极强",
            RequiredLevel = 20,
            RequiredBattleExp = 1000,
            RequiredItemId = 1031, // 强化钢板
            RequiredItemCount = 5,
            GoldCost = 500,
            HealthBonus = 0.25f,
            AttackBonus = 0.08f,
            DefenseBonus = 0.20f,
            SpeedBonus = -0.05f,
            UnlockSkills = new List<string> { "iron_skin" },
            TintColor = new Color(0.6f, 0.5f, 0.4f)
        });

        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "brown_bear",
            BaseMountName = "棕熊",
            Stage = MountEvolutionData.EvolutionStage.Elite,
            Type = MountEvolutionData.EvolutionType.Nature,
            EvolutionName = "山岭巨熊",
            Description = "如山岳般巨大的熊王，撼动大地",
            RequiredLevel = 40,
            RequiredBattleExp = 5000,
            RequiredItemId = 1032, // 山岳之心
            RequiredItemCount = 10,
            GoldCost = 2000,
            HealthBonus = 0.40f,
            AttackBonus = 0.15f,
            DefenseBonus = 0.30f,
            SpeedBonus = -0.08f,
            UnlockSkills = new List<string> { "iron_skin", "earthquake" },
            TintColor = new Color(0.5f, 0.45f, 0.35f)
        });

        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "brown_bear",
            BaseMountName = "棕熊",
            Stage = MountEvolutionData.EvolutionStage.Epic,
            Type = MountEvolutionData.EvolutionType.Nature,
            EvolutionName = "自然守护者",
            Description = "自然之力的化身，大地的守护者",
            RequiredLevel = 60,
            RequiredBattleExp = 15000,
            RequiredItemId = 1033, // 自然之源
            RequiredItemCount = 5,
            GoldCost = 10000,
            HealthBonus = 0.55f,
            AttackBonus = 0.25f,
            DefenseBonus = 0.45f,
            SpeedBonus = -0.10f,
            UnlockSkills = new List<string> { "iron_skin", "earthquake", "nature_wrath" },
            TintColor = new Color(0.4f, 0.6f, 0.3f)
        });

        // === 金鹰进化链 ===
        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "golden_eagle",
            BaseMountName = "金鹰",
            Stage = MountEvolutionData.EvolutionStage.Advanced,
            Type = MountEvolutionData.EvolutionType.Lightning,
            EvolutionName = "雷鸣金鹰",
            Description = "掌控雷电的金鹰，闪电般的速度",
            RequiredLevel = 20,
            RequiredBattleExp = 1000,
            RequiredItemId = 1041, // 雷电精华
            RequiredItemCount = 5,
            GoldCost = 500,
            HealthBonus = 0.08f,
            AttackBonus = 0.15f,
            DefenseBonus = 0.08f,
            SpeedBonus = 0.25f,
            UnlockSkills = new List<string> { "thunder_bolt" },
            TintColor = new Color(1f, 0.95f, 0.4f)
        });

        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "golden_eagle",
            BaseMountName = "金鹰",
            Stage = MountEvolutionData.EvolutionStage.Elite,
            Type = MountEvolutionData.EvolutionType.Lightning,
            EvolutionName = "苍穹雷鹰",
            Description = "翱翔于苍穹的雷鹰，召唤天雷",
            RequiredLevel = 40,
            RequiredBattleExp = 5000,
            RequiredItemId = 1042, // 苍穹之雷
            RequiredItemCount = 10,
            GoldCost = 2000,
            HealthBonus = 0.15f,
            AttackBonus = 0.25f,
            DefenseBonus = 0.15f,
            SpeedBonus = 0.35f,
            UnlockSkills = new List<string> { "thunder_bolt", "lightning_storm" },
            TintColor = new Color(1f, 0.9f, 0.3f)
        });

        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "golden_eagle",
            BaseMountName = "金鹰",
            Stage = MountEvolutionData.EvolutionStage.Epic,
            Type = MountEvolutionData.EvolutionType.Lightning,
            EvolutionName = "雷霆之主",
            Description = "雷霆之力的终极形态，天空的霸主",
            RequiredLevel = 60,
            RequiredBattleExp = 15000,
            RequiredItemId = 1043, // 雷霆本源
            RequiredItemCount = 5,
            GoldCost = 10000,
            HealthBonus = 0.25f,
            AttackBonus = 0.40f,
            DefenseBonus = 0.20f,
            SpeedBonus = 0.50f,
            UnlockSkills = new List<string> { "thunder_bolt", "lightning_storm", "thunder_wrath" },
            TintColor = new Color(1f, 0.85f, 0.2f)
        });

        // === 红龙进化链 ===
        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "red_dragon",
            BaseMountName = "红龙",
            Stage = MountEvolutionData.EvolutionStage.Advanced,
            Type = MountEvolutionData.EvolutionType.Fire,
            EvolutionName = "烈焰红龙",
            Description = "喷吐烈焰的红龙，焚烧一切",
            RequiredLevel = 25,
            RequiredBattleExp = 1500,
            RequiredItemId = 1051, // 火焰之心
            RequiredItemCount = 5,
            GoldCost = 800,
            HealthBonus = 0.20f,
            AttackBonus = 0.25f,
            DefenseBonus = 0.15f,
            SpeedBonus = 0.05f,
            UnlockSkills = new List<string> { "fire_breath" },
            TintColor = new Color(1f, 0.5f, 0.2f)
        });

        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "red_dragon",
            BaseMountName = "红龙",
            Stage = MountEvolutionData.EvolutionStage.Elite,
            Type = MountEvolutionData.EvolutionType.Fire,
            EvolutionName = "熔岩巨龙",
            Description = "由熔岩构成的巨龙，掌控大地之火",
            RequiredLevel = 45,
            RequiredBattleExp = 7000,
            RequiredItemId = 1052, // 熔岩核心
            RequiredItemCount = 10,
            GoldCost = 3000,
            HealthBonus = 0.30f,
            AttackBonus = 0.35f,
            DefenseBonus = 0.25f,
            SpeedBonus = 0.08f,
            UnlockSkills = new List<string> { "fire_breath", "lava_armor" },
            TintColor = new Color(0.9f, 0.4f, 0.1f)
        });

        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "red_dragon",
            BaseMountName = "红龙",
            Stage = MountEvolutionData.EvolutionStage.Epic,
            Type = MountEvolutionData.EvolutionType.Fire,
            EvolutionName = "炎帝巨龙",
            Description = "火焰帝国的帝王，毁灭的象征",
            RequiredLevel = 65,
            RequiredBattleExp = 20000,
            RequiredItemId = 1053, // 炎帝之印
            RequiredItemCount = 5,
            GoldCost = 15000,
            HealthBonus = 0.45f,
            AttackBonus = 0.50f,
            DefenseBonus = 0.35f,
            SpeedBonus = 0.12f,
            UnlockSkills = new List<string> { "fire_breath", "lava_armor", "inferno" },
            TintColor = new Color(1f, 0.3f, 0f)
        });

        // 传奇阶段 (Legendary) - 最终进化
        RegisterLegendaryEvolutions();
    }

    private void RegisterLegendaryEvolutions()
    {
        // 白马传奇进化
        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "white_horse",
            BaseMountName = "白马",
            Stage = MountEvolutionData.EvolutionStage.Legendary,
            Type = MountEvolutionData.EvolutionType.Holy,
            EvolutionName = "创世神驹",
            Description = "创造之神降临的终极坐骑，超越凡间的存在",
            RequiredLevel = 80,
            RequiredBattleExp = 50000,
            RequiredItemId = 1099, // 创世神晶
            RequiredItemCount = 1,
            GoldCost = 50000,
            HealthBonus = 0.60f,
            AttackBonus = 0.45f,
            DefenseBonus = 0.40f,
            SpeedBonus = 0.30f,
            UnlockSkills = new List<string> { "holy_charge", "divine_shield", "light_burst", "creation_blessing" },
            TintColor = new Color(1f, 1f, 1f)
        });

        // 黑马传奇进化
        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "black_horse",
            BaseMountName = "黑马",
            Stage = MountEvolutionData.EvolutionStage.Legendary,
            Type = MountEvolutionData.EvolutionType.Dark,
            EvolutionName = "灭世魔驹",
            Description = "毁灭之力的终极形态，黑暗的代言人",
            RequiredLevel = 80,
            RequiredBattleExp = 50000,
            RequiredItemId = 1098, // 灭世之源
            RequiredItemCount = 1,
            GoldCost = 50000,
            HealthBonus = 0.50f,
            AttackBonus = 0.60f,
            DefenseBonus = 0.30f,
            SpeedBonus = 0.35f,
            UnlockSkills = new List<string> { "shadow_strike", "dark_aura", "annihilation", "void_consumption" },
            TintColor = new Color(0.1f, 0.1f, 0.15f)
        });

        // 红龙传奇进化
        RegisterEvolution(new MountEvolutionData.MountEvolutionConfig
        {
            MountId = "red_dragon",
            BaseMountName = "红龙",
            Stage = MountEvolutionData.EvolutionStage.Legendary,
            Type = MountEvolutionData.EvolutionType.Fire,
            EvolutionName = "炎狱魔龙",
            Description = "来自炎狱的终极巨龙，毁灭与重生的化身",
            RequiredLevel = 80,
            RequiredBattleExp = 50000,
            RequiredItemId = 1097, // 炎狱核心
            RequiredItemCount = 1,
            GoldCost = 50000,
            HealthBonus = 0.60f,
            AttackBonus = 0.65f,
            DefenseBonus = 0.45f,
            SpeedBonus = 0.20f,
            UnlockSkills = new List<string> { "fire_breath", "lava_armor", "inferno", "apocalypse" },
            TintColor = new Color(0.8f, 0.1f, 0f)
        });
    }

    private void RegisterEvolution(MountEvolutionData.MountEvolutionConfig config)
    {
        if (!_evolutionConfigs.ContainsKey(config.MountId))
        {
            _evolutionConfigs[config.MountId] = new List<MountEvolutionData.MountEvolutionConfig>();
            _availableStages[config.MountId] = new List<MountEvolutionData.EvolutionStage>();
        }

        _evolutionConfigs[config.MountId].Add(config);
        
        // 排序：按阶段排序
        _evolutionConfigs[config.MountId].Sort((a, b) => a.Stage.CompareTo(b.Stage));
        
        if (!_availableStages[config.MountId].Contains(config.Stage))
        {
            _availableStages[config.MountId].Add(config.Stage);
            _availableStages[config.MountId].Sort();
        }
    }

    /// <summary>
    /// 获取坐骑的所有进化配置
    /// </summary>
    public List<MountEvolutionData.MountEvolutionConfig> GetEvolutions(string mountId)
    {
        if (_evolutionConfigs.ContainsKey(mountId))
            return new List<MountEvolutionData.MountEvolutionConfig>(_evolutionConfigs[mountId]);
        return new List<MountEvolutionData.MountEvolutionConfig>();
    }

    /// <summary>
    /// 获取坐骑可用的下一个进化阶段
    /// </summary>
    public MountEvolutionData.MountEvolutionConfig GetNextEvolution(string mountId, MountEvolutionData.EvolutionStage currentStage)
    {
        if (!_evolutionConfigs.ContainsKey(mountId)) return null;

        foreach (var config in _evolutionConfigs[mountId])
        {
            if (config.Stage > currentStage)
                return config;
        }
        return null;
    }

    /// <summary>
    /// 获取特定阶段的进化配置
    /// </summary>
    public MountEvolutionData.MountEvolutionConfig GetEvolutionByStage(string mountId, MountEvolutionData.EvolutionStage stage)
    {
        if (!_evolutionConfigs.ContainsKey(mountId)) return null;

        foreach (var config in _evolutionConfigs[mountId])
        {
            if (config.Stage == stage)
                return config;
        }
        return null;
    }

    /// <summary>
    /// 检查是否有可用的进化
    /// </summary>
    public bool HasEvolution(string mountId, MountEvolutionData.EvolutionStage currentStage)
    {
        return GetNextEvolution(mountId, currentStage) != null;
    }

    /// <summary>
    /// 获取所有可进化的坐骑ID列表
    /// </summary>
    public List<string> GetEvolvableMounts()
    {
        return new List<string>(_evolutionConfigs.Keys);
    }
}
