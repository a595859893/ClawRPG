using Godot;
using System;
using System.Collections.Generic;

public class BossMechanicsDatabase
{
    private static BossMechanicsDatabase _instance;
    public static BossMechanicsDatabase Instance
    {
        get
        {
            if (_instance == null) _instance = new BossMechanicsDatabase();
            return _instance;
        }
    }
    
    public Dictionary<string, BossMechanicsData> BossDatabase { get; private set; } = new Dictionary<string, BossMechanicsData>();
    
    public BossMechanicsDatabase()
    {
        InitializeBosses();
    }
    
    private void InitializeBosses()
    {
        // Dragon Lord - 龙之领主
        var dragonLord = new BossMechanicsData
        {
            BossId = "dragon_lord",
            BossName = "龙之领主",
            BossLevel = 50,
            MaxHealth = 50000,
            Attack = 500,
            Defense = 300,
            Speed = 80,
            CanSummonMinions = true,
            MaxMinionCount = 6,
            MinionTypes = new string[] { "dragon_soldier", "fire_dragon" },
            MinionSpawnHealthPercent = 0.6f,
            HasEnrageMechanic = true,
            EnrageTime = 180f,
            WeaknessElement = "Ice",
            WeaknessMultiplier = 2.0f,
            LootTable = new string[] { "dragon_scale", "fire_breath", "dragon_heart", "ancient_sword", "legendary_armor" },
            LootWeights = new float[] { 40, 30, 15, 10, 5 },
            MinLootCount = 2,
            MaxLootCount = 5
        };
        dragonLord.Phases = new List<BossPhaseData>
        {
            new BossPhaseData { PhaseName = "火焰吐息", PhaseNumber = 1, HealthPercentage = 1.0f, AttackMultiplier = 1.0f, NewSkills = new List<string> { "fire_breath", "wing_slash" } },
            new BossPhaseData { PhaseName = "龙之狂暴", PhaseNumber = 2, HealthPercentage = 0.6f, AttackMultiplier = 1.5f, SpeedMultiplier = 1.3f, SpawnEnemies = new List<string> { "dragon_soldier" }, SpawnCount = 3, PhaseEffect = "攻击速度提升" },
            new BossPhaseData { PhaseName = "深渊之力", PhaseNumber = 3, HealthPercentage = 0.3f, AttackMultiplier = 2.0f, DefenseMultiplier = 0.7f, SpawnEnemies = new List<string> { "fire_dragon" }, SpawnCount = 2, PhaseEffect = "防御下降但攻击大幅提升" },
            new BossPhaseData { PhaseName = "最终形态", PhaseNumber = 4, HealthPercentage = 0.1f, AttackMultiplier = 2.5f, SpeedMultiplier = 1.5f, IsEnragePhase = true, PhaseEffect = "全属性大幅提升" }
        };
        dragonLord.Skills = new List<BossSkillData>
        {
            new BossSkillData { SkillName = "火焰吐息", SkillId = "fire_breath", Cooldown = 15f, Range = 30f, Damage = 800, TargetType = "Area", CastTime = 2f, IsInterruptible = false, EffectType = "Fire" },
            new BossSkillData { SkillName = "龙翼斩", SkillId = "wing_slash", Cooldown = 8f, Range = 15f, Damage = 500, TargetType = "Front", CastTime = 1f, IsInterruptible = true, EffectType = "Physical" },
            new BossSkillData { SkillName = "召唤龙裔", SkillId = "summon_dragon", Cooldown = 45f, Range = 0f, Damage = 0, TargetType = "Self", CastTime = 3f, IsInterruptible = true, EffectType = "Summon" },
            new BossSkillData { SkillName = "龙息爆发", SkillId = "dragon_explosion", Cooldown = 60f, Range = 50f, Damage = 1500, TargetType = "Area", CastTime = 3f, IsInterruptible = false, EffectType = "Fire" }
        };
        dragonLord.EnrageTimers = new List<BossEnrageData>
        {
            new BossEnrageData { EnrageName = "狂暴", TriggerTime = 180f, AttackMultiplier = 2.0f, SpeedMultiplier = 1.5f, EnrageEffect = "攻击和速度大幅提升", VisualEffect = "全身燃烧火焰" }
        };
        BossDatabase["dragon_lord"] = dragonLord;
        
        // Shadow Assassin - 暗影刺客
        var shadowAssassin = new BossMechanicsData
        {
            BossId = "shadow_assassin",
            BossName = "暗影刺客",
            BossLevel = 45,
            MaxHealth = 30000,
            Attack = 700,
            Defense = 150,
            Speed = 150,
            CanSummonMinions = true,
            MaxMinionCount = 4,
            MinionTypes = new string[] { "shadow_clone", "night_stalker" },
            MinionSpawnHealthPercent = 0.5f,
            HasEnrageMechanic = true,
            EnrageTime = 120f,
            WeaknessElement = "Holy",
            WeaknessMultiplier = 1.8f,
            LootTable = new string[] { "shadow_cloak", "assassin_dagger", "poison_vial", "speed_boots" },
            LootWeights = new float[] { 35, 30, 20, 15 },
            MinLootCount = 1,
            MaxLootCount = 3
        };
        shadowAssassin.Phases = new List<BossPhaseData>
        {
            new BossPhaseData { PhaseName = "影之舞", PhaseNumber = 1, HealthPercentage = 1.0f, AttackMultiplier = 1.0f, SpeedMultiplier = 1.0f },
            new BossPhaseData { PhaseName = "分身术", PhaseNumber = 2, HealthPercentage = 0.6f, AttackMultiplier = 1.3f, SpeedMultiplier = 1.5f, SpawnEnemies = new List<string> { "shadow_clone" }, SpawnCount = 2, PhaseEffect = "召唤两个分身" },
            new BossPhaseData { PhaseName = "暗影打击", PhaseNumber = 3, HealthPercentage = 0.3f, AttackMultiplier = 2.0f, SpeedMultiplier = 2.0f, IsEnragePhase = true, PhaseEffect = "速度极快，难以命中" }
        };
        shadowAssassin.Skills = new List<BossSkillData>
        {
            new BossSkillData { SkillName = "背刺", SkillId = "backstab", Cooldown = 5f, Range = 10f, Damage = 600, TargetType = "Single", CastTime = 0.5f, IsInterruptible = false, EffectType = "Physical" },
            new BossSkillData { SkillName = "烟雾弹", SkillId = "smoke_bomb", Cooldown = 20f, Range = 0f, Damage = 0, TargetType = "Self", CastTime = 1f, IsInterruptible = true, EffectType = "Debuff" },
            new BossSkillData { SkillName = "暗影突袭", SkillId = "shadow_assault", Cooldown = 12f, Range = 25f, Damage = 900, TargetType = "Multi", CastTime = 1.5f, IsInterruptible = false, EffectType = "Dark" },
            new BossSkillData { SkillName = "致命毒药", SkillId = "deadly_poison", Cooldown = 30f, Range = 20f, Damage = 300, TargetType = "Area", CastTime = 2f, IsInterruptible = true, EffectType = "Poison" }
        };
        shadowAssassin.EnrageTimers = new List<BossEnrageData>
        {
            new BossEnrageData { EnrageName = "影之终章", TriggerTime = 120f, AttackMultiplier = 2.5f, SpeedMultiplier = 2.0f, EnrageEffect = "速度和伤害大幅提升", VisualEffect = "全身被暗影覆盖" }
        };
        BossDatabase["shadow_assassin"] = shadowAssassin;
        
        // Ancient Golem - 远古泰坦
        var ancientGolem = new BossMechanicsData
        {
            BossId = "ancient_golem",
            BossName = "远古泰坦",
            BossLevel = 60,
            MaxHealth = 80000,
            Attack = 400,
            Defense = 500,
            Speed = 40,
            CanSummonMinions = false,
            HasEnrageMechanic = true,
            EnrageTime = 240f,
            WeaknessElement = "Lightning",
            WeaknessMultiplier = 1.8f,
            LootTable = new string[] { "golem_core", "ancient_ore", "earth_shield", "titan_fist" },
            LootWeights = new float[] { 35, 30, 20, 15 },
            MinLootCount = 2,
            MaxLootCount = 4
        };
        ancientGolem.Phases = new List<BossPhaseData>
        {
            new BossPhaseData { PhaseName = "石肤", PhaseNumber = 1, HealthPercentage = 1.0f, DefenseMultiplier = 2.0f, AttackMultiplier = 1.0f },
            new BossPhaseData { PhaseName = "大地之力", PhaseNumber = 2, HealthPercentage = 0.7f, DefenseMultiplier = 1.5f, AttackMultiplier = 1.5f, NewSkills = new List<string> { "earthquake" } },
            new BossPhaseData { PhaseName = "泰坦觉醒", PhaseNumber = 3, HealthPercentage = 0.4f, DefenseMultiplier = 1.0f, AttackMultiplier = 2.0f, SpeedMultiplier = 1.5f },
            new BossPhaseData { PhaseName = "最终形态", PhaseNumber = 4, HealthPercentage = 0.15f, DefenseMultiplier = 0.5f, AttackMultiplier = 3.0f, IsEnragePhase = true }
        };
        ancientGolem.Skills = new List<BossSkillData>
        {
            new BossSkillData { SkillName = "重拳", SkillId = "heavy_fist", Cooldown = 3f, Range = 12f, Damage = 700, TargetType = "Single", CastTime = 1f, IsInterruptible = false, EffectType = "Physical" },
            new BossSkillData { SkillName = "地震", SkillId = "earthquake", Cooldown = 25f, Range = 30f, Damage = 1000, TargetType = "Area", CastTime = 2.5f, IsInterruptible = false, EffectType = "Earth" },
            new BossSkillData { SkillName = "岩石护盾", SkillId = "rock_shield", Cooldown = 40f, Range = 0f, Damage = 0, TargetType = "Self", CastTime = 1f, IsInterruptible = true, EffectType = "Buff" },
            new BossSkillData { SkillName = "投掷巨石", SkillId = "throw_boulder", Cooldown = 15f, Range = 40f, Damage = 800, TargetType = "Single", CastTime = 2f, IsInterruptible = false, EffectType = "Earth" }
        };
        ancientGolem.EnrageTimers = new List<BossEnrageData>
        {
            new BossEnrageData { EnrageName = "泰坦之怒", TriggerTime = 240f, AttackMultiplier = 2.5f, SpeedMultiplier = 1.8f, EnrageEffect = "攻击大幅提升", VisualEffect = "身体开裂发光" }
        };
        BossDatabase["ancient_golem"] = ancientGolem;
        
        // Frost Wyrm - 冰霜巨龙
        var frostWyrm = new BossMechanicsData
        {
            BossId = "frost_wyrm",
            BossName = "冰霜巨龙",
            BossLevel = 55,
            MaxHealth = 45000,
            Attack = 550,
            Defense = 350,
            Speed = 90,
            CanSummonMinions = true,
            MaxMinionCount = 5,
            MinionTypes = new string[] { "ice_spirit", "frost_wolf" },
            MinionSpawnHealthPercent = 0.5f,
            HasEnrageMechanic = true,
            EnrageTime = 150f,
            WeaknessElement = "Fire",
            WeaknessMultiplier = 2.0f,
            LootTable = new string[] { "frost_scale", "ice_crystal", "wyrm_heart", "frozen_weapon" },
            LootWeights = new float[] { 40, 30, 20, 10 },
            MinLootCount = 2,
            MaxLootCount = 4
        };
        frostWyrm.Phases = new List<BossPhaseData>
        {
            new BossPhaseData { PhaseName = "冰封", PhaseNumber = 1, HealthPercentage = 1.0f, NewSkills = new List<string> { "ice_breath", "frost_nova" } },
            new BossPhaseData { PhaseName = "暴风雪", PhaseNumber = 2, HealthPercentage = 0.6f, AttackMultiplier = 1.4f, SpawnEnemies = new List<string> { "ice_spirit" }, SpawnCount = 3 },
            new BossPhaseData { PhaseName = "绝对零度", PhaseNumber = 3, HealthPercentage = 0.3f, AttackMultiplier = 2.0f, IsEnragePhase = true }
        };
        frostWyrm.Skills = new List<BossSkillData>
        {
            new BossSkillData { SkillName = "寒冰吐息", SkillId = "ice_breath", Cooldown = 12f, Range = 25f, Damage = 700, TargetType = "Area", CastTime = 2f, IsInterruptible = false, EffectType = "Ice" },
            new BossSkillData { SkillName = "冰霜新星", SkillId = "frost_nova", Cooldown = 20f, Range = 20f, Damage = 500, TargetType = "Area", CastTime = 1.5f, IsInterruptible = false, EffectType = "Ice" },
            new BossSkillData { SkillName = "绝对零度", SkillId = "absolute_zero", Cooldown = 60f, Range = 40f, Damage = 2000, TargetType = "Area", CastTime = 4f, IsInterruptible = true, EffectType = "Ice" }
        };
        frostWyrm.EnrageTimers = new List<BossEnrageData>
        {
            new BossEnrageData { EnrageName = "极寒", TriggerTime = 150f, AttackMultiplier = 2.0f, SpeedMultiplier = 1.3f, EnrageEffect = "攻击大幅提升", VisualEffect = "周围温度骤降" }
        };
        BossDatabase["frost_wyrm"] = frostWyrm;
        
        // Demon King - 恶魔之王
        var demonKing = new BossMechanicsData
        {
            BossId = "demon_king",
            BossName = "恶魔之王",
            BossLevel = 70,
            MaxHealth = 100000,
            Attack = 650,
            Defense = 400,
            Speed = 100,
            CanSummonMinions = true,
            MaxMinionCount = 8,
            MinionTypes = new string[] { "demon_soldier", "hell_hound", "dark_knight" },
            MinionSpawnHealthPercent = 0.5f,
            HasEnrageMechanic = true,
            EnrageTime = 200f,
            WeaknessElement = "Holy",
            WeaknessMultiplier = 2.5f,
            LootTable = new string[] { "demon_heart", "hellfire_sword", "dark_armor", "crown_of_madness" },
            LootWeights = new float[] { 30, 25, 25, 20 },
            MinLootCount = 3,
            MaxLootCount = 6
        };
        demonKing.Phases = new List<BossPhaseData>
        {
            new BossPhaseData { PhaseName = "黑暗之力", PhaseNumber = 1, HealthPercentage = 1.0f },
            new BossPhaseData { PhaseName = "恶魔大军", PhaseNumber = 2, HealthPercentage = 0.7f, AttackMultiplier = 1.3f, SpawnEnemies = new List<string> { "demon_soldier", "hell_hound" }, SpawnCount = 4 },
            new BossPhaseData { PhaseName = "炼狱之火", PhaseNumber = 3, HealthPercentage = 0.4f, AttackMultiplier = 1.8f, SpeedMultiplier = 1.3f },
            new BossPhaseData { PhaseName = "最终审判", PhaseNumber = 4, HealthPercentage = 0.15f, AttackMultiplier = 2.5f, SpeedMultiplier = 1.5f, IsEnragePhase = true }
        };
        demonKing.Skills = new List<BossSkillData>
        {
            new BossSkillData { SkillName = "暗影箭", SkillId = "shadow_bolt", Cooldown = 4f, Range = 30f, Damage = 400, TargetType = "Single", CastTime = 0.5f, IsInterruptible = false, EffectType = "Dark" },
            new BossSkillData { SkillName = "地狱火", SkillId = "hellfire", Cooldown = 30f, Range = 25f, Damage = 1200, TargetType = "Area", CastTime = 3f, IsInterruptible = false, EffectType = "Fire" },
            new BossSkillData { SkillName = "召唤恶魔", SkillId = "summon_demon", Cooldown = 45f, Range = 0f, Damage = 0, TargetType = "Self", CastTime = 3f, IsInterruptible = true, EffectType = "Summon" },
            new BossSkillData { SkillName = "恶魔之门", SkillId = "demon_gate", Cooldown = 90f, Range = 0f, Damage = 0, TargetType = "Self", CastTime = 5f, IsInterruptible = true, EffectType = "Summon" }
        };
        demonKing.EnrageTimers = new List<BossEnrageData>
        {
            new BossEnrageData { EnrageName = "真正形态", TriggerTime = 200f, AttackMultiplier = 2.5f, SpeedMultiplier = 1.5f, EnrageEffect = "完全解放恶魔之力", VisualEffect = "身体膨胀，火焰环绕" }
        };
        BossDatabase["demon_king"] = demonKing;
    }
    
    public BossMechanicsData GetBoss(string bossId)
    {
        if (BossDatabase.ContainsKey(bossId))
            return BossDatabase[bossId];
        return null;
    }
    
    public List<string> GetAllBossIds()
    {
        return new List<string>(BossDatabase.Keys);
    }
}
