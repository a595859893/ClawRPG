using Godot;
using System;
using System.Collections.Generic;

public class BossMechanicsDatabase
{
    private static Dictionary<string, BossConfig> _bossConfigs;
    private static Dictionary<string, BossPhaseConfig> _phaseConfigs;
    private static Dictionary<string, BossSkillConfig> _skillConfigs;
    private static bool _initialized = false;

    public static void Initialize()
    {
        if (_initialized) return;

        _bossConfigs = new Dictionary<string, BossConfig>();
        _phaseConfigs = new Dictionary<string, BossPhaseConfig>();
        _skillConfigs = new Dictionary<string, BossSkillConfig>();

        InitializeSkills();
        InitializeBossConfigs();

        _initialized = true;
    }

    private static void InitializeSkills()
    {
        // 近战技能
        _skillConfigs["boss_melee_slash"] = new BossSkillConfig
        {
            Id = "boss_melee_slash",
            Name = "致命斩击",
            Description = "Boss挥动武器进行一次强力斩击",
            SkillType = BossSkillType.MeleeAttack,
            Damage = 150f,
            Range = 3f,
            Cooldown = 3f,
            CastTime = 0.5f,
            KnockbackForce = 5f,
            ExecuteProbability = 0.4f
        };

        _skillConfigs["boss_spin_attack"] = new BossSkillConfig
        {
            Id = "boss_spin_attack",
            Name = "旋转攻击",
            Description = "Boss原地旋转攻击周围所有目标",
            SkillType = BossSkillType.SpinAttack,
            Damage = 200f,
            AreaRadius = 5f,
            Cooldown = 8f,
            CastTime = 1f,
            Duration = 2f,
            ExecuteProbability = 0.2f
        };

        _skillConfigs["boss_charge"] = new BossSkillConfig
        {
            Id = "boss_charge",
            Name = "冲锋",
            Description = "Boss向目标冲锋",
            SkillType = BossSkillType.Charge,
            Damage = 150f,
            Range = 15f,
            Cooldown = 10f,
            CastTime = 0.8f,
            KnockbackForce = 10f,
            ExecuteProbability = 0.15f
        };

        // 远程技能
        _skillConfigs["boss_ice_lance"] = new BossSkillConfig
        {
            Id = "boss_ice_lance",
            Name = "寒冰之矛",
            Description = "Boss发射寒冰投射物",
            SkillType = BossSkillType.Projectile,
            Damage = 120f,
            Range = 20f,
            Cooldown = 4f,
            CastTime = 0.6f,
            ExecuteProbability = 0.3f
        };

        _skillConfigs["boss_fire_breath"] = new BossSkillConfig
        {
            Id = "boss_fire_breath",
            Name = "火焰吐息",
            Description = "Boss喷出火焰",
            SkillType = BossSkillType.AreaOfEffect,
            Damage = 250f,
            AreaRadius = 8f,
            Range = 12f,
            Cooldown = 12f,
            CastTime = 1.5f,
            Duration = 3f,
            ExecuteProbability = 0.15f
        };

        _skillConfigs["boss_laser_beam"] = new BossSkillConfig
        {
            Id = "boss_laser_beam",
            Name = "激光射线",
            Description = "Boss发射致命的激光束",
            SkillType = BossSkillType.LaserBeam,
            Damage = 400f,
            AreaRadius = 3f,
            Range = 25f,
            Cooldown = 20f,
            CastTime = 2f,
            Duration = 2f,
            StunDuration = 1f,
            ExecuteProbability = 0.1f
        };

        // 范围技能
        _skillConfigs["boss_explosion"] = new BossSkillConfig
        {
            Id = "boss_explosion",
            Name = "爆炸",
            Description = "Boss引发剧烈爆炸",
            SkillType = BossSkillType.AreaOfEffect,
            Damage = 300f,
            AreaRadius = 10f,
            Cooldown = 15f,
            CastTime = 1f,
            KnockbackForce = 15f,
            ExecuteProbability = 0.15f
        };

        _skillConfigs["boss_meteor_strike"] = new BossSkillConfig
        {
            Id = "boss_meteor_strike",
            Name = "陨石打击",
            Description = "Boss召唤陨石砸向目标区域",
            SkillType = BossSkillType.AreaOfEffect,
            Damage = 500f,
            AreaRadius = 6f,
            Range = 20f,
            Cooldown = 25f,
            CastTime = 3f,
            ExecuteProbability = 0.08f
        };

        // 召唤技能
        _skillConfigs["boss_summon_minions"] = new BossSkillConfig
        {
            Id = "boss_summon_minions",
            Name = "召唤随从",
            Description = "Boss召唤小怪助战",
            SkillType = BossSkillType.Summon,
            SummonMonsterId = "dark_skeleton",
            SummonCount = 4,
            Cooldown = 30f,
            CastTime = 2f,
            ExecuteProbability = 0.12f
        };

        _skillConfigs["boss_summon_elemental"] = new BossSkillConfig
        {
            Id = "boss_summon_elemental",
            Name = "召唤元素",
            Description = "Boss召唤强大的元素生物",
            SkillType = BossSkillType.Summon,
            SummonMonsterId = "fire_elemental",
            SummonCount = 1,
            Cooldown = 45f,
            CastTime = 3f,
            ExecuteProbability = 0.08f
        };

        // 减益技能
        _skillConfigs["boss_curse"] = new BossSkillConfig
        {
            Id = "boss_curse",
            Name = "诅咒",
            Description = "Boss施加致命诅咒",
            SkillType = BossSkillType.Debuff,
            Damage = 50f,
            Cooldown = 20f,
            CastTime = 1f,
            Duration = 10f,
            DebuffIds = new List<string> { "curse_armor", "curse_damage" },
            ExecuteProbability = 0.15f
        };

        _skillConfigs["boss_poison_cloud"] = new BossSkillConfig
        {
            Id = "boss_poison_cloud",
            Name = "毒云",
            Description = "Boss释放毒云",
            SkillType = BossSkillType.AreaOfEffect,
            Damage = 80f,
            AreaRadius = 8f,
            Cooldown = 18f,
            CastTime = 1.5f,
            Duration = 8f,
            DebuffIds = new List<string> { "poison" },
            ExecuteProbability = 0.2f
        };

        // 控制技能
        _skillConfigs["boss_stun_smash"] = new BossSkillConfig
        {
            Id = "boss_stun_smash",
            Name = "震晕打击",
            Description = "Boss重击并眩晕目标",
            SkillType = BossSkillType.Stun,
            Damage = 200f,
            Range = 4f,
            Cooldown = 12f,
            CastTime = 0.8f,
            StunDuration = 3f,
            KnockbackForce = 8f,
            ExecuteProbability = 0.15f
        };

        // 辅助技能
        _skillConfigs["boss_shield"] = new BossSkillConfig
        {
            Id = "boss_shield",
            Name = "护盾",
            Description = "Boss获得护盾",
            SkillType = BossSkillType.Shield,
            ShieldAmount = 500f,
            Cooldown = 25f,
            CastTime = 0.5f,
            ExecuteProbability = 0.1f
        };

        _skillConfigs["boss_heal"] = new BossSkillConfig
        {
            Id = "boss_heal",
            Name = "自我治疗",
            Description = "Boss治疗自己",
            SkillType = BossSkillType.Heal,
            HealAmount = 300f,
            Cooldown = 30f,
            CastTime = 1.5f,
            ExecuteProbability = 0.08f
        };

        // 狂暴技能
        _skillConfigs["boss_enrage"] = new BossSkillConfig
        {
            Id = "boss_enrage",
            Name = "狂暴",
            Description = "Boss进入狂暴状态",
            SkillType = BossSkillType.Enrage,
            DamageMultiplier = 2.0f,
            Cooldown = 999999f,
            CastTime = 2f,
            ExecuteProbability = 1.0f,
            IsEnragedOnly = true
        };

        // 特殊技能
        _skillConfigs["boss_teleport"] = new BossSkillConfig
        {
            Id = "boss_teleport",
            Name = "传送",
            Description = "Boss传送到随机位置",
            SkillType = BossSkillType.Teleport,
            Cooldown = 15f,
            CastTime = 0.3f,
            ExecuteProbability = 0.2f
        };

        _skillConfigs["boss_mind_control"] = new BossSkillConfig
        {
            Id = "boss_mind_control",
            Name = "精神控制",
            Description = "Boss控制目标心智",
            SkillType = BossSkillType.Debuff,
            Damage = 100f,
            Range = 15f,
            Cooldown = 35f,
            CastTime = 2f,
            Duration = 5f,
            ExecuteProbability = 0.08f
        };
    }

    private static void InitializeBossConfigs()
    {
        // 森林之王 - 精英Boss
        var forestKing = new BossConfig
        {
            Id = "forest_king",
            Name = "森林之王",
            Description = "古老森林的统治者，拥有操控自然的力量",
            Type = BossType.Elite,
            Difficulty = DifficultyLevel.Normal,
            MaxHealth = 50000f,
            AttackPower = 150f,
            Defense = 50f,
            MoveSpeed = 4f,
            AttackSpeed = 1.2f,
            CriticalChance = 0.15f,
            CriticalDamage = 1.5f,
            Level = 25,
            PhaseCount = 2,
            EnrageThreshold = 0.3f,
            EnrageTimer = 180f,
            DefaultPattern = AttackPattern.Balanced,
            GoldReward = 5000f,
            ExpReward = 5000f,
            PointReward = 100,
            RespawnTime = 3600f
        };
        forestKing.Skills.Add(_skillConfigs["boss_melee_slash"]);
        forestKing.Skills.Add(_skillConfigs["boss_summon_minions"]);
        forestKing.Skills.Add(_skillConfigs["boss_poison_cloud"]);
        forestKing.Skills.Add(_skillConfigs["boss_heal"]);
        forestKing.DropTable.Add(new DropTableEntry { ItemId = "forest_king_sword", DropChance = 0.1f, MinQuantity = 1, MaxQuantity = 1 });
        _bossConfigs["forest_king"] = forestKing;

        // 火焰领主 - 精英Boss
        var fireLord = new BossConfig
        {
            Id = "fire_lord",
            Name = "火焰领主",
            Description = "来自深渊的火焰恶魔，掌控一切火焰之力",
            Type = BossType.Elite,
            Difficulty = DifficultyLevel.Hard,
            MaxHealth = 75000f,
            AttackPower = 200f,
            Defense = 40f,
            MoveSpeed = 4.5f,
            AttackSpeed = 1.0f,
            CriticalChance = 0.2f,
            CriticalDamage = 1.8f,
            Level = 35,
            PhaseCount = 3,
            EnrageThreshold = 0.25f,
            EnrageTimer = 150f,
            DefaultPattern = AttackPattern.Aggressive,
            GoldReward = 8000f,
            ExpReward = 8000f,
            PointReward = 150,
            RespawnTime = 7200f
        };
        fireLord.Skills.Add(_skillConfigs["boss_melee_slash"]);
        fireLord.Skills.Add(_skillConfigs["boss_fire_breath"]);
        fireLord.Skills.Add(_skillConfigs["boss_explosion"]);
        fireLord.Skills.Add(_skillConfigs["boss_summon_elemental"]);
        fireLord.Skills.Add(_skillConfigs["boss_charge"]);
        fireLord.Skills.Add(_skillConfigs["boss_enrage"]);
        fireLord.DropTable.Add(new DropTableEntry { ItemId = "infernal_axe", DropChance = 0.08f, MinQuantity = 1, MaxQuantity = 1 });
        fireLord.DropTable.Add(new DropTableEntry { ItemId = "fire_essence", DropChance = 0.3f, MinQuantity = 3, MaxQuantity = 10 });
        _bossConfigs["fire_lord"] = fireLord;

        // 冰霜巨龙 - 世界Boss
        var iceDragon = new BossConfig
        {
            Id = "ice_dragon",
            Name = "冰霜巨龙",
            Description = "古老的冰霜巨龙，曾统治北境的天空",
            Type = BossType.World,
            Difficulty = DifficultyLevel.Nightmare,
            MaxHealth = 500000f,
            AttackPower = 350f,
            Defense = 100f,
            MoveSpeed = 6f,
            AttackSpeed = 0.8f,
            CriticalChance = 0.25f,
            CriticalDamage = 2.0f,
            Level = 50,
            PhaseCount = 3,
            EnrageThreshold = 0.2f,
            EnrageTimer = 300f,
            DefaultPattern = AttackPattern.Phased,
            GoldReward = 50000f,
            ExpReward = 50000f,
            PointReward = 500,
            TitleReward = "冰霜屠龙者",
            RespawnTime = 14400f
        };
        iceDragon.Skills.Add(_skillConfigs["boss_melee_slash"]);
        iceDragon.Skills.Add(_skillConfigs["boss_ice_lance"]);
        iceDragon.Skills.Add(_skillConfigs["boss_frost_breath"]);
        iceDragon.Skills.Add(_skillConfigs["boss_teleport"]);
        iceDragon.Skills.Add(_skillConfigs["boss_freeze"]);
        iceDragon.Skills.Add(_skillConfigs["boss_summon_minions"]);
        iceDragon.Skills.Add(_skillConfigs["boss_enrage"]);
        iceDragon.DropTable.Add(new DropTableEntry { ItemId = "ice_dragon_scale", DropChance = 0.5f, MinQuantity = 1, MaxQuantity = 3 });
        iceDragon.DropTable.Add(new DropTableEntry { ItemId = "frozen_heart", DropChance = 0.1f, MinQuantity = 1, MaxQuantity = 1 });
        _bossConfigs["ice_dragon"] = iceDragon;

        // 暗影君主 - 传说Boss
        var shadowLord = new BossConfig
        {
            Id = "shadow_lord",
            Name = "暗影君主",
            Description = "穿梭于虚实之间的暗影王者",
            Type = BossType.Legendary,
            Difficulty = DifficultyLevel.Legendary,
            MaxHealth = 1000000f,
            AttackPower = 500f,
            Defense = 150f,
            MoveSpeed = 7f,
            AttackSpeed = 1.5f,
            CriticalChance = 0.35f,
            CriticalDamage = 2.5f,
            Level = 60,
            PhaseCount = 4,
            EnrageThreshold = 0.15f,
            EnrageTimer = 360f,
            DefaultPattern = AttackPattern.Erratic,
            IsRaidBoss = true,
            RequiredPartySize = 10,
            GoldReward = 100000f,
            ExpReward = 100000f,
            PointReward = 1000,
            TitleReward = "暗影终结者",
            RespawnTime = 28800f
        };
        shadowLord.Skills.Add(_skillConfigs["boss_melee_slash"]);
        shadowLord.Skills.Add(_skillConfigs["boss_shadow_strike"]);
        shadowLord.Skills.Add(_skillConfigs["boss_teleport"]);
        shadowLord.Skills.Add(_skillConfigs["boss_mind_control"]);
        shadowLord.Skills.Add(_skillConfigs["boss_summon_shadows"]);
        shadowLord.Skills.Add(_skillConfigs["boss_dark_void"]);
        shadowLord.Skills.Add(_skillConfigs["boss_shield"]);
        shadowLord.Skills.Add(_skillConfigs["boss_enrage"]);
        shadowLord.DropTable.Add(new DropTableEntry { ItemId = "shadow_crown", DropChance = 0.05f, MinQuantity = 1, MaxQuantity = 1 });
        shadowLord.DropTable.Add(new DropTableEntry { ItemId = "void_shard", DropChance = 0.2f, MinQuantity = 5, MaxQuantity = 15 });
        _bossConfigs["shadow_lord"] = shadowLord;

        // 深渊恶魔 - 团本Boss
        var abyssDemon = new BossConfig
        {
            Id = "abyss_demon",
            Name = "深渊恶魔",
            Description = "来自深渊的终极恶魔",
            Type = BossType.Raid,
            Difficulty = DifficultyLevel.Legendary,
            MaxHealth = 2000000f,
            AttackPower = 600f,
            Defense = 200f,
            MoveSpeed = 5f,
            AttackSpeed = 0.7f,
            CriticalChance = 0.3f,
            CriticalDamage = 2.2f,
            Level = 70,
            PhaseCount = 5,
            EnrageThreshold = 0.1f,
            EnrageTimer = 420f,
            DefaultPattern = AttackPattern.Phased,
            IsRaidBoss = true,
            RequiredPartySize = 20,
            GoldReward = 200000f,
            ExpReward = 200000f,
            PointReward = 2000,
            TitleReward = "深渊征服者",
            RespawnTime = 43200f
        };
        abyssDemon.Skills.Add(_skillConfigs["boss_melee_slash"]);
        abyssDemon.Skills.Add(_skillConfigs["boss_spin_attack"]);
        abyssDemon.Skills.Add(_skillConfigs["boss_charge"]);
        abyssDemon.Skills.Add(_skillConfigs["boss_explosion"]);
        abyssDemon.Skills.Add(_skillConfigs["boss_meteor_strike"]);
        abyssDemon.Skills.Add(_skillConfigs["boss_summon_minions"]);
        abyssDemon.Skills.Add(_skillConfigs["boss_heal"]);
        abyssDemon.Skills.Add(_skillConfigs["boss_enrage"]);
        abyssDemon.DropTable.Add(new DropTableEntry { ItemId = "demon_heart", DropChance = 0.3f, MinQuantity = 1, MaxQuantity = 2 });
        abyssDemon.DropTable.Add(new DropTableEntry { ItemId = "abyss_artifact", DropChance = 0.05f, MinQuantity = 1, MaxQuantity = 1 });
        _bossConfigs["abyss_demon"] = abyssDemon;

        // 元素之王 - 地下城Boss
        var elementKing = new BossConfig
        {
            Id = "element_king",
            Name = "元素之王",
            Description = "四种元素的主宰者",
            Type = BossType.Dungeon,
            Difficulty = DifficultyLevel.Hard,
            MaxHealth = 100000f,
            AttackPower = 250f,
            Defense = 80f,
            MoveSpeed = 5f,
            AttackSpeed = 1.3f,
            CriticalChance = 0.2f,
            CriticalDamage = 1.6f,
            Level = 40,
            PhaseCount = 4,
            EnrageThreshold = 0.25f,
            EnrageTimer = 180f,
            DefaultPattern = AttackPattern.Balanced,
            GoldReward = 10000f,
            ExpReward = 10000f,
            PointReward = 200,
            RespawnTime = 3600f
        };
        elementKing.Skills.Add(_skillConfigs["boss_melee_slash"]);
        elementKing.Skills.Add(_skillConfigs["boss_elemental_burst"]);
        elementKing.Skills.Add(_skillConfigs["boss_teleport"]);
        elementKing.Skills.Add(_skillConfigs["boss_shield"]);
        elementKing.Skills.Add(_skillConfigs["boss_enrage"]);
        elementKing.DropTable.Add(new DropTableEntry { ItemId = "element_crystal", DropChance = 0.25f, MinQuantity = 2, MaxQuantity = 5 });
        _bossConfigs["element_king"] = elementKing;
    }

    public static BossConfig GetBossConfig(string bossId)
    {
        Initialize();
        return _bossConfigs.ContainsKey(bossId) ? _bossConfigs[bossId] : null;
    }

    public static Dictionary<string, BossConfig> GetAllBossConfigs()
    {
        Initialize();
        return new Dictionary<string, BossConfig>(_bossConfigs);
    }

    public static BossSkillConfig GetSkillConfig(string skillId)
    {
        Initialize();
        return _skillConfigs.ContainsKey(skillId) ? _skillConfigs[skillId] : null;
    }

    public static Dictionary<string, BossSkillConfig> GetAllSkillConfigs()
    {
        Initialize();
        return new Dictionary<string, BossSkillConfig>(_skillConfigs);
    }

    public static List<BossConfig> GetBossConfigsByType(BossType type)
    {
        Initialize();
        List<BossConfig> result = new List<BossConfig>();
        foreach (var config in _bossConfigs.Values)
        {
            if (config.Type == type)
                result.Add(config);
        }
        return result;
    }

    public static List<BossConfig> GetBossConfigsByDifficulty(DifficultyLevel difficulty)
    {
        Initialize();
        List<BossConfig> result = new List<BossConfig>();
        foreach (var config in _bossConfigs.Values)
        {
            if (config.Difficulty == difficulty)
                result.Add(config);
        }
        return result;
    }
}
