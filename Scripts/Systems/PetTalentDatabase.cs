using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 宠物天赋数据库 - 存储所有宠物天赋定义和获取方法
/// </summary>
public class PetTalentDatabase
{
    private static PetTalentDatabase _instance;
    public static PetTalentDatabase Instance
    {
        get
        {
            if (_instance == null) _instance = new PetTalentDatabase();
            return _instance;
        }
    }

    public Dictionary<string, PetTalentData> AllTalents { get; private set; }
    public List<PetTalentData> CommonTalents { get; private set; }
    public List<PetTalentData> UncommonTalents { get; private set; }
    public List<PetTalentData> RareTalents { get; private set; }
    public List<PetTalentData> EpicTalents { get; private set; }
    public List<PetTalentData> LegendaryTalents { get; private set; }

    private Random _random = new Random();

    public PetTalentDatabase()
    {
        AllTalents = new Dictionary<string, PetTalentData>();
        CommonTalents = new List<PetTalentData>();
        UncommonTalents = new List<PetTalentData>();
        RareTalents = new List<PetTalentData>();
        EpicTalents = new List<PetTalentData>();
        LegendaryTalents = new List<PetTalentData>();
        InitializeTalents();
    }

    private void InitializeTalents()
    {
        // ===== 普通天赋 (Common) =====
        AddTalent(new PetTalentData("attack_boost_1", "力量增强 I", "+5% 攻击力", 
            PetTalentData.TalentType.Attack, PetTalentData.TalentRarity.Common, 0.05f, "attack"));
        AddTalent(new PetTalentData("defense_boost_1", "防御增强 I", "+5% 防御力", 
            PetTalentData.TalentType.Defense, PetTalentData.TalentRarity.Common, 0.05f, "defense"));
        AddTalent(new PetTalentData("health_boost_1", "生命增强 I", "+5% 最大生命", 
            PetTalentData.TalentType.Defense, PetTalentData.TalentRarity.Common, 0.05f, "health"));
        AddTalent(new PetTalentData("speed_boost_1", "速度增强 I", "+5% 移动速度", 
            PetTalentData.TalentType.Utility, PetTalentData.TalentRarity.Common, 0.05f, "speed"));
        AddTalent(new PetTalentData("crit_boost_1", "暴击增强 I", "+3% 暴击率", 
            PetTalentData.TalentType.Attack, PetTalentData.TalentRarity.Common, 0.03f, "crit_rate"));
        AddTalent(new PetTalentData("exp_boost_1", "经验获取 I", "+5% 经验获取", 
            PetTalentData.TalentType.Support, PetTalentData.TalentRarity.Common, 0.05f, "exp"));
        AddTalent(new PetTalentData("gold_boost_1", "金币获取 I", "+5% 金币获取", 
            PetTalentData.TalentType.Utility, PetTalentData.TalentRarity.Common, 0.05f, "gold"));
        AddTalent(new PetTalentData("drop_boost_1", "掉落增强 I", "+5% 物品掉落率", 
            PetTalentData.TalentType.Utility, PetTalentData.TalentRarity.Common, 0.05f, "drop"));

        // ===== 优秀天赋 (Uncommon) =====
        AddTalent(new PetTalentData("attack_boost_2", "力量增强 II", "+10% 攻击力", 
            PetTalentData.TalentType.Attack, PetTalentData.TalentRarity.Uncommon, 0.10f, "attack"));
        AddTalent(new PetTalentData("defense_boost_2", "防御增强 II", "+10% 防御力", 
            PetTalentData.TalentType.Defense, PetTalentData.TalentRarity.Uncommon, 0.10f, "defense"));
        AddTalent(new PetTalentData("health_boost_2", "生命增强 II", "+10% 最大生命", 
            PetTalentData.TalentType.Defense, PetTalentData.TalentRarity.Uncommon, 0.10f, "health"));
        AddTalent(new PetTalentData("speed_boost_2", "速度增强 II", "+10% 移动速度", 
            PetTalentData.TalentType.Utility, PetTalentData.TalentRarity.Uncommon, 0.10f, "speed"));
        AddTalent(new PetTalentData("crit_boost_2", "暴击增强 II", "+6% 暴击率", 
            PetTalentData.TalentType.Attack, PetTalentData.TalentRarity.Uncommon, 0.06f, "crit_rate"));
        AddTalent(new PetTalentData("crit_damage_1", "暴击伤害 I", "+10% 暴击伤害", 
            PetTalentData.TalentType.Attack, PetTalentData.TalentRarity.Uncommon, 0.10f, "crit_damage"));
        AddTalent(new PetTalentData("lifesteal_1", "生命偷取 I", "+5% 生命偷取", 
            PetTalentData.TalentType.Attack, PetTalentData.TalentRarity.Uncommon, 0.05f, "lifesteal"));
        AddTalent(new PetTalentData("dodge_1", "闪避 I", "+5% 闪避率", 
            PetTalentData.TalentType.Defense, PetTalentData.TalentRarity.Uncommon, 0.05f, "dodge"));

        // ===== 稀有天赋 (Rare) =====
        AddTalent(new PetTalentData("attack_boost_3", "力量增强 III", "+15% 攻击力", 
            PetTalentData.TalentType.Attack, PetTalentData.TalentRarity.Rare, 0.15f, "attack"));
        AddTalent(new PetTalentData("defense_boost_3", "防御增强 III", "+15% 防御力", 
            PetTalentData.TalentType.Defense, PetTalentData.TalentRarity.Rare, 0.15f, "defense"));
        AddTalent(new PetTalentData("health_boost_3", "生命增强 III", "+15% 最大生命", 
            PetTalentData.TalentType.Defense, PetTalentData.TalentRarity.Rare, 0.15f, "health"));
        AddTalent(new PetTalentData("crit_boost_3", "暴击增强 III", "+9% 暴击率", 
            PetTalentData.TalentType.Attack, PetTalentData.TalentRarity.Rare, 0.09f, "crit_rate"));
        AddTalent(new PetTalentData("crit_damage_2", "暴击伤害 II", "+20% 暴击伤害", 
            PetTalentData.TalentType.Attack, PetTalentData.TalentRarity.Rare, 0.20f, "crit_damage"));
        AddTalent(new PetTalentData("lifesteal_2", "生命偷取 II", "+10% 生命偷取", 
            PetTalentData.TalentType.Attack, PetTalentData.TalentRarity.Rare, 0.10f, "lifesteal"));
        AddTalent(new PetTalentData("dodge_2", "闪避 II", "+10% 闪避率", 
            PetTalentData.TalentType.Defense, PetTalentData.TalentRarity.Rare, 0.10f, "dodge"));
        AddTalent(new PetTalentData("tenacity_1", "韧性 I", "+10% 韧性", 
            PetTalentData.TalentType.Defense, PetTalentData.TalentRarity.Rare, 0.10f, "tenacity"));

        // ===== 史诗天赋 (Epic) =====
        AddTalent(new PetTalentData("attack_boost_4", "力量增强 IV", "+20% 攻击力", 
            PetTalentData.TalentType.Attack, PetTalentData.TalentRarity.Epic, 0.20f, "attack"));
        AddTalent(new PetTalentData("defense_boost_4", "防御增强 IV", "+20% 防御力", 
            PetTalentData.TalentType.Defense, PetTalentData.TalentRarity.Epic, 0.20f, "defense"));
        AddTalent(new PetTalentData("health_boost_4", "生命增强 IV", "+20% 最大生命", 
            PetTalentData.TalentType.Defense, PetTalentData.TalentRarity.Epic, 0.20f, "health"));
        AddTalent(new PetTalentData("crit_boost_4", "暴击增强 IV", "+12% 暴击率", 
            PetTalentData.TalentType.Attack, PetTalentData.TalentRarity.Epic, 0.12f, "crit_rate"));
        AddTalent(new PetTalentData("crit_damage_3", "暴击伤害 III", "+30% 暴击伤害", 
            PetTalentData.TalentType.Attack, PetTalentData.TalentRarity.Epic, 0.30f, "crit_damage"));
        AddTalent(new PetTalentData("lifesteal_3", "生命偷取 III", "+15% 生命偷取", 
            PetTalentData.TalentType.Attack, PetTalentData.TalentRarity.Epic, 0.15f, "lifesteal"));
        AddTalent(new PetTalentData("dodge_3", "闪避 III", "+15% 闪避率", 
            PetTalentData.TalentType.Defense, PetTalentData.TalentRarity.Epic, 0.15f, "dodge"));
        AddTalent(new PetTalentData("tenacity_2", "韧性 II", "+20% 韧性", 
            PetTalentData.TalentType.Defense, PetTalentData.TalentRarity.Epic, 0.20f, "tenacity"));

        // ===== 传说天赋 (Legendary) =====
        AddTalent(new PetTalentData("legendary_power", "传奇之力", "+30% 全部属性", 
            PetTalentData.TalentType.Special, PetTalentData.TalentRarity.Legendary, 0.30f, "all"));
        AddTalent(new PetTalentData("legendary_frenzy", "狂暴之力", "+25% 攻击 + 暴击率", 
            PetTalentData.TalentType.Attack, PetTalentData.TalentRarity.Legendary, 0.25f, "frenzy"));
        AddTalent(new PetTalentData("legendary_guard", "守护之力", "+25% 防御 + 闪避", 
            PetTalentData.TalentType.Defense, PetTalentData.TalentRarity.Legendary, 0.25f, "guard"));
        AddTalent(new PetTalentData("legendary_blessing", "祝福之力", "+20% 经验 + 金币 + 掉落", 
            PetTalentData.TalentType.Support, PetTalentData.TalentRarity.Legendary, 0.20f, "blessing"));
        AddTalent(new PetTalentData("legendary_swift", "极速之力", "+25% 移动速度 + 闪避", 
            PetTalentData.TalentType.Utility, PetTalentData.TalentRarity.Legendary, 0.25f, "swift"));
    }

    private void AddTalent(PetTalentData talent)
    {
        AllTalents[talent.Id] = talent;
        
        switch (talent.Rarity)
        {
            case PetTalentData.TalentRarity.Common:
                CommonTalents.Add(talent);
                break;
            case PetTalentData.TalentRarity.Uncommon:
                UncommonTalents.Add(talent);
                break;
            case PetTalentData.TalentRarity.Rare:
                RareTalents.Add(talent);
                break;
            case PetTalentData.TalentRarity.Epic:
                EpicTalents.Add(talent);
                break;
            case PetTalentData.TalentRarity.Legendary:
                LegendaryTalents.Add(talent);
                break;
        }
    }

    public PetTalentData GetTalent(string talentId)
    {
        if (AllTalents.ContainsKey(talentId))
            return AllTalents[talentId];
        return null;
    }

    public List<PetTalentData> GetTalentsByRarity(PetTalentData.TalentRarity rarity)
    {
        switch (rarity)
        {
            case PetTalentData.TalentRarity.Common: return CommonTalents;
            case PetTalentData.TalentRarity.Uncommon: return UncommonTalents;
            case PetTalentData.TalentRarity.Rare: return RareTalents;
            case PetTalentData.TalentRarity.Epic: return EpicTalents;
            case PetTalentData.TalentRarity.Legendary: return LegendaryTalents;
            default: return CommonTalents;
        }
    }

    public PetTalentData GetRandomTalentByRarity(PetTalentData.TalentRarity rarity)
    {
        var talents = GetTalentsByRarity(rarity);
        if (talents.Count == 0) return null;
        return talents[_random.Next(talents.Count)];
    }

    public PetTalentData GenerateRandomTalent()
    {
        // 稀有度权重: Common 50%, Uncommon 30%, Rare 15%, Epic 4%, Legendary 1%
        double roll = _random.NextDouble() * 100;
        
        if (roll < 50) return GetRandomTalentByRarity(PetTalentData.TalentRarity.Common);
        else if (roll < 80) return GetRandomTalentByRarity(PetTalentData.TalentRarity.Uncommon);
        else if (roll < 95) return GetRandomTalentByRarity(PetTalentData.TalentRarity.Rare);
        else if (roll < 99) return GetRandomTalentByRarity(PetTalentData.TalentRarity.Epic);
        else return GetRandomTalentByRarity(PetTalentData.TalentRarity.Legendary);
    }

    public List<PetTalentData> GenerateTalentSet(int count)
    {
        List<PetTalentData> talents = new List<PetTalentData>();
        HashSet<string> usedIds = new HashSet<string>();
        
        for (int i = 0; i < count; i++)
        {
            PetTalentData talent = GenerateRandomTalent();
            if (talent != null && !usedIds.Contains(talent.Id))
            {
                talents.Add(talent);
                usedIds.Add(talent.Id);
            }
        }
        return talents;
    }
}
