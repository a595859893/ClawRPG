using Godot;
using System;
using System.Collections.Generic;

public class BoonDatabase
{
    private static BoonDatabase _instance;
    public static BoonDatabase Instance => _instance ??= new BoonDatabase();
    
    public List<BoonData> AllBoons { get; private set; }
    public Dictionary<string, BoonData> BoonMap { get; private set; }
    public Dictionary<BoonRarity, float> RarityWeights { get; private set; }
    
    private BoonDatabase()
    {
        AllBoons = new List<BoonData>();
        BoonMap = new Dictionary<string, BoonData>();
        RarityWeights = new Dictionary<BoonRarity, float>
        {
            { BoonRarity.Common, 50f },
            { BoonRarity.Uncommon, 30f },
            { BoonRarity.Rare, 15f },
            { BoonRarity.Epic, 4f },
            { BoonRarity.Legendary, 1f }
        };
        InitializeBoons();
    }
    
    private void InitializeBoons()
    {
        // Attack Boons - Common
        AddBoon(new BoonData("attack_1", "锋利", "+10 攻击", BoonType.Attack, BoonRarity.Common) { AttackBonus = 10 });
        AddBoon(new BoonData("attack_2", "精准", "+5 攻击, +5% 暴击率", BoonType.Attack, BoonRarity.Common) { AttackBonus = 5, CritRateBonus = 0.05f });
        
        // Attack Boons - Uncommon
        AddBoon(new BoonData("attack_3", "猛击", "+25 攻击", BoonType.Attack, BoonRarity.Uncommon) { AttackBonus = 25 });
        AddBoon(new BoonData("attack_4", "撕裂", "+15 攻击, +10% 暴击率", BoonType.Attack, BoonRarity.Uncommon) { AttackBonus = 15, CritRateBonus = 0.10f });
        
        // Attack Boons - Rare
        AddBoon(new BoonData("attack_5", "破坏", "+40 攻击, +15% 暴击率", BoonType.Attack, BoonRarity.Rare) { AttackBonus = 40, CritRateBonus = 0.15f });
        AddBoon(new BoonData("attack_6", "斩裂", "+30 攻击, +20% 暴击伤害", BoonType.Attack, BoonRarity.Rare) { AttackBonus = 30, CritDamageBonus = 0.20f });
        
        // Attack Boons - Epic
        AddBoon(new BoonData("attack_7", "毁灭", "+60 攻击, +25% 暴击率", BoonType.Attack, BoonRarity.Epic) { AttackBonus = 60, CritRateBonus = 0.25f });
        
        // Attack Boons - Legendary
        AddBoon(new BoonData("attack_8", "弑神", "+100 攻击, +30% 暴击率, +50% 暴击伤害", BoonType.Attack, BoonRarity.Legendary) { AttackBonus = 100, CritRateBonus = 0.30f, CritDamageBonus = 0.50f });
        
        // Defense Boons - Common
        AddBoon(new BoonData("defense_1", "坚固", "+10 防御", BoonType.Defense, BoonRarity.Common) { DefenseBonus = 10 });
        AddBoon(new BoonData("defense_2", "护盾", "+5 防御, +5% 闪避", BoonType.Defense, BoonRarity.Common) { DefenseBonus = 5, DodgeBonus = 0.05f });
        
        // Defense Boons - Uncommon
        AddBoon(new BoonData("defense_3", "铁壁", "+25 防御", BoonType.Defense, BoonRarity.Uncommon) { DefenseBonus = 25 });
        AddBoon(new BoonData("defense_4", "闪避", "+15 防御, +10% 闪避", BoonType.Defense, BoonRarity.Uncommon) { DefenseBonus = 15, DodgeBonus = 0.10f });
        
        // Defense Boons - Rare
        AddBoon(new BoonData("defense_5", "坚韧", "+40 防御, +15% 闪避", BoonType.Defense, BoonRarity.Rare) { DefenseBonus = 40, DodgeBonus = 0.15f });
        
        // Defense Boons - Epic
        AddBoon(new BoonData("defense_6", "绝对防御", "+60 防御, +20% 闪避", BoonType.Defense, BoonRarity.Epic) { DefenseBonus = 60, DodgeBonus = 0.20f });
        
        // Defense Boons - Legendary
        AddBoon(new BoonData("defense_7", "不朽之躯", "+100 防御, +30% 闪避, 10% 生命偷取", BoonType.Defense, BoonRarity.Legendary) { DefenseBonus = 100, DodgeBonus = 0.30f, LifestealBonus = 0.10f });
        
        // Life Boons - Common
        AddBoon(new BoonData("life_1", "活力", "+50 生命", BoonType.Life, BoonRarity.Common) { HealthBonus = 50 });
        AddBoon(new BoonData("life_2", "恢复", "+30 生命, +3 生命恢复", BoonType.Life, BoonRarity.Common) { HealthBonus = 30 });
        
        // Life Boons - Uncommon
        AddBoon(new BoonData("life_3", "旺盛", "+100 生命", BoonType.Life, BoonRarity.Uncommon) { HealthBonus = 100 });
        AddBoon(new BoonData("life_4", "再生", "+75 生命, +5 生命恢复", BoonType.Life, BoonRarity.Uncommon) { HealthBonus = 75 });
        
        // Life Boons - Rare
        AddBoon(new BoonData("life_5", "巨魔之力", "+150 生命, +8 生命恢复", BoonType.Life, BoonRarity.Rare) { HealthBonus = 150 });
        
        // Life Boons - Epic
        AddBoon(new BoonData("life_6", "凤凰之息", "+200 生命, +12 生命恢复", BoonType.Life, BoonRarity.Epic) { HealthBonus = 200 });
        
        // Life Boons - Legendary
        AddBoon(new BoonData("life_7", "永恒生命", "+300 生命, +20 生命恢复, 15% 生命偷取", BoonType.Life, BoonRarity.Legendary) { HealthBonus = 300, LifestealBonus = 0.15f });
        
        // Magic Boons - Common
        AddBoon(new BoonData("magic_1", "魔力", "+10 魔法", BoonType.Magic, BoonRarity.Common) { MagicBonus = 10 });
        
        // Magic Boons - Uncommon
        AddBoon(new BoonData("magic_2", "奥术", "+25 魔法", BoonType.Magic, BoonRarity.Uncommon) { MagicBonus = 25 });
        
        // Magic Boons - Rare
        AddBoon(new BoonData("magic_3", "秘法", "+40 魔法, +10% 暴击率", BoonType.Magic, BoonRarity.Rare) { MagicBonus = 40, CritRateBonus = 0.10f });
        
        // Magic Boons - Epic
        AddBoon(new BoonData("magic_4", "元素大师", "+60 魔法, +15% 暴击率", BoonType.Magic, BoonRarity.Epic) { MagicBonus = 60, CritRateBonus = 0.15f });
        
        // Magic Boons - Legendary
        AddBoon(new BoonData("magic_5", "法神", "+100 魔法, +25% 暴击率, +30% 暴击伤害", BoonType.Magic, BoonRarity.Legendary) { MagicBonus = 100, CritRateBonus = 0.25f, CritDamageBonus = 0.30f });
        
        // Speed Boons - Common
        AddBoon(new BoonData("speed_1", "迅捷", "+5 速度", BoonType.Speed, BoonRarity.Common) { SpeedBonus = 5 });
        
        // Speed Boons - Uncommon
        AddBoon(new BoonData("speed_2", "疾风", "+10 速度", BoonType.Speed, BoonRarity.Uncommon) { SpeedBonus = 10 });
        
        // Speed Boons - Rare
        AddBoon(new BoonData("speed_3", "闪电", "+15 速度, +10% 闪避", BoonType.Speed, BoonRarity.Rare) { SpeedBonus = 15, DodgeBonus = 0.10f });
        
        // Speed Boons - Epic
        AddBoon(new BoonData("speed_4", "光速", "+20 速度, +15% 闪避", BoonType.Speed, BoonRarity.Epic) { SpeedBonus = 20, DodgeBonus = 0.15f });
        
        // Speed Boons - Legendary
        AddBoon(new BoonData("speed_5", "时间行者", "+30 速度, +25% 闪避, +20% 暴击率", BoonType.Speed, BoonRarity.Legendary) { SpeedBonus = 30, DodgeBonus = 0.25f, CritRateBonus = 0.20f });
        
        // Critical Boons - Uncommon
        AddBoon(new BoonData("crit_1", "敏锐", "+10% 暴击率", BoonType.Critical, BoonRarity.Uncommon) { CritRateBonus = 0.10f });
        
        // Critical Boons - Rare
        AddBoon(new BoonData("crit_2", "暴击", "+15% 暴击率, +20% 暴击伤害", BoonType.Critical, BoonRarity.Rare) { CritRateBonus = 0.15f, CritDamageBonus = 0.20f });
        
        // Critical Boons - Epic
        AddBoon(new BoonData("crit_3", "致命", "+25% 暴击率, +40% 暴击伤害", BoonType.Critical, BoonRarity.Epic) { CritRateBonus = 0.25f, CritDamageBonus = 0.40f });
        
        // Critical Boons - Legendary
        AddBoon(new BoonData("crit_4", "死神", "+35% 暴击率, +60% 暴击伤害, 10% 生命偷取", BoonType.Critical, BoonRarity.Legendary) { CritRateBonus = 0.35f, CritDamageBonus = 0.60f, LifestealBonus = 0.10f });
        
        // Utility Boons - Common
        AddBoon(new BoonData("utility_1", "幸运", "+10% 金币获取", BoonType.Utility, BoonRarity.Common) { GoldMultiplier = 10 });
        AddBoon(new BoonData("utility_2", "经验", "+10% 经验获取", BoonType.Utility, BoonRarity.Common) { ExpMultiplier = 10 });
        
        // Utility Boons - Uncommon
        AddBoon(new BoonData("utility_3", "财富", "+20% 金币获取", BoonType.Utility, BoonRarity.Uncommon) { GoldMultiplier = 20 });
        AddBoon(new BoonData("utility_4", "智慧", "+20% 经验获取", BoonType.Utility, BoonRarity.Uncommon) { ExpMultiplier = 20 });
        
        // Utility Boons - Rare
        AddBoon(new BoonData("utility_5", "富甲天下", "+35% 金币获取", BoonType.Utility, BoonRarity.Rare) { GoldMultiplier = 35 });
        AddBoon(new BoonData("utility_6", "博学", "+35% 经验获取", BoonType.Utility, BoonRarity.Rare) { ExpMultiplier = 35 });
        
        // Utility Boons - Epic
        AddBoon(new BoonData("utility_7", "金银满屋", "+50% 金币获取", BoonType.Utility, BoonRarity.Epic) { GoldMultiplier = 50 });
        AddBoon(new BoonData("utility_8", "学富五车", "+50% 经验获取", BoonType.Utility, BoonRarity.Epic) { ExpMultiplier = 50 });
        
        // Utility Boons - Legendary
        AddBoon(new BoonData("utility_9", "气运之子", "+75% 金币获取, +75% 经验获取", BoonType.Utility, BoonRarity.Legendary) { GoldMultiplier = 75, ExpMultiplier = 75 });
        
        // Special Boons - Rare
        AddBoon(new BoonData("special_1", "全能", "+20 攻击, +20 防御, +50 生命", BoonType.Special, BoonRarity.Rare) { AttackBonus = 20, DefenseBonus = 20, HealthBonus = 50 });
        
        // Special Boons - Epic
        AddBoon(new BoonData("special_2", "半神", "+40 攻击, +40 防御, +100 生命, +10 速度", BoonType.Special, BoonRarity.Epic) { AttackBonus = 40, DefenseBonus = 40, HealthBonus = 100, SpeedBonus = 10 });
        
        // Special Boons - Legendary
        AddBoon(new BoonData("special_3", "创世神", "+60 攻击, +60 防御, +150 生命, +20 速度, +15% 暴击率", BoonType.Special, BoonRarity.Legendary) { AttackBonus = 60, DefenseBonus = 60, HealthBonus = 150, SpeedBonus = 20, CritRateBonus = 0.15f });
    }
    
    private void AddBoon(BoonData boon)
    {
        AllBoons.Add(boon);
        BoonMap[boon.Id] = boon;
    }
    
    public BoonData GetBoon(string id)
    {
        return BoonMap.GetValueOrDefault(id, null);
    }
    
    public List<BoonData> GetBoonsByRarity(BoonRarity rarity)
    {
        var result = new List<BoonData>();
        foreach (var boon in AllBoons)
        {
            if (boon.Rarity == rarity)
                result.Add(boon);
        }
        return result;
    }
    
    public List<BoonData> GetBoonsByType(BoonType type)
    {
        var result = new List<BoonData>();
        foreach (var boon in AllBoons)
        {
            if (boon.Type == type)
                result.Add(boon);
        }
        return result;
    }
    
    public BoonData GetRandomBoon(BoonRarity? forcedRarity = null)
    {
        if (forcedRarity.HasValue)
        {
            var boons = GetBoonsByRarity(forcedRarity.Value);
            if (boons.Count > 0)
                return boons[GD.Rand() % boons.Count];
            return null;
        }
        
        // Weighted random based on rarity
        float totalWeight = 0;
        foreach (var kvp in RarityWeights)
            totalWeight += kvp.Value;
        
        float random = (float)GD.Randd() * totalWeight;
        float cumulative = 0;
        
        foreach (var kvp in RarityWeights)
        {
            cumulative += kvp.Value;
            if (random <= cumulative)
            {
                var boons = GetBoonsByRarity(kvp.Key);
                if (boons.Count > 0)
                    return boons[GD.Rand() % boons.Count];
            }
        }
        
        // Fallback
        if (AllBoons.Count > 0)
            return AllBoons[GD.Rand() % AllBoons.Count];
        return null;
    }
    
    public List<BoonData> GetRandomBoonPool(int count, BoonRarity? forcedRarity = null)
    {
        var pool = new List<BoonData>();
        var available = new List<BoonData>(AllBoons);
        
        for (int i = 0; i < count && available.Count > 0; i++)
        {
            int index;
            if (forcedRarity.HasValue)
            {
                var rarityBoons = GetBoonsByRarity(forcedRarity.Value);
                if (rarityBoons.Count == 0) break;
                index = GD.Rand() % rarityBoons.Count;
                pool.Add(rarityBoons[index]);
                available.Remove(rarityBoons[index]);
            }
            else
            {
                index = GD.Rand() % available.Count;
                pool.Add(available[index]);
                available.RemoveAt(index);
            }
        }
        
        return pool;
    }
    
    public static string GetRarityColor(BoonRarity rarity)
    {
        return rarity switch
        {
            BoonRarity.Common => "#FFFFFF",
            BoonRarity.Uncommon => "#1EFF00",
            BoonRarity.Rare => "#0070DD",
            BoonRarity.Epic => "#A335EE",
            BoonRarity.Legendary => "#FF8000",
            _ => "#FFFFFF"
        };
    }
    
    public static string GetTypeName(BoonType type)
    {
        return type switch
        {
            BoonType.Attack => "攻击",
            BoonType.Defense => "防御",
            BoonType.Life => "生命",
            BoonType.Magic => "魔法",
            BoonType.Speed => "速度",
            BoonType.Critical => "暴击",
            BoonType.Utility => "utility",
            BoonType.Special => "特殊",
            _ => "未知"
        };
    }
}
