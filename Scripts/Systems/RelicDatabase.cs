using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems;

/// <summary>
/// 遗物数据库
/// </summary>
public static class RelicDatabase
{
    private static readonly Dictionary<string, RelicData> _relics = new();
    private static bool _initialized = false; 
    
    public static void Initialize()
    {
        if (_initialized) return;
        
        // 攻击型遗物 (Attack)
        AddRelic(new RelicData("relic_sword_fragment", "断剑碎片", "增加10%攻击力", 
            RelicType.Attack, RelicRarity.Common, 1, 100) 
        { AttributeBonuses = new Dictionary<string, float> { { "attack", 0.10f } } });
        
        AddRelic(new RelicData("relic_blood_amulet", "血之护符", "增加5%生命偷取", 
            RelicType.Attack, RelicRarity.Uncommon, 1, 300) 
        { AttributeBonuses = new Dictionary<string, float> { { "lifesteal", 0.05f } } });
        
        AddRelic(new RelicData("relic_crimson_orb", "深红宝珠", "增加15%暴击率和20%暴击伤害", 
            RelicType.Attack, RelicRarity.Rare, 2, 800) 
        { AttributeBonuses = new Dictionary<string, float> { { "crit_rate", 0.15f }, { "crit_damage", 0.20f } } });
        
        AddRelic(new RelicData("relic_dragon_scale", "龙鳞", "攻击时有10%几率造成300%伤害", 
            RelicType.Attack, RelicRarity.Epic, 3, 2000) 
        { SpecialEffect = "dragon_strike" });
        
        AddRelic(new RelicData("relic_phantom_blade", "幽灵之刃", "普通攻击有30%几率触发额外一次攻击", 
            RelicType.Attack, RelicRarity.Legendary, 3, 5000) 
        { SpecialEffect = "phantom_strike" });
        
        // 防御型遗物 (Defense)
        AddRelic(new RelicData("relic_iron_shield", "铁盾", "增加10%防御力", 
            RelicType.Defense, RelicRarity.Common, 1, 100) 
        { AttributeBonuses = new Dictionary<string, float> { { "defense", 0.10f } } });
        
        AddRelic(new RelicData("relic_healing_crystal", "治愈水晶", "每5秒恢复1%最大生命值", 
            RelicType.Defense, RelicRarity.Uncommon, 1, 300) 
        { SpecialEffect = "regeneration" });
        
        AddRelic(new RelicData("relic_thorn_armor", "荆棘之甲", "受到攻击时反弹15%伤害", 
            RelicType.Defense, RelicRarity.Rare, 2, 800) 
        { SpecialEffect = "thorns" });
        
        AddRelic(new RelicData("relic_phoenix_feather", "凤凰羽毛", "每场战斗最多抵挡一次致命伤害", 
            RelicType.Defense, RelicRarity.Epic, 3, 2000) 
        { SpecialEffect = "phoenix_blessing" });
        
        AddRelic(new RelicData("relic_immortal_ring", "不朽之戒", "生命值低于20%时获得3秒无敌", 
            RelicType.Defense, RelicRarity.Legendary, 3, 5000) 
        { SpecialEffect = "last_stand" });
        
        // 辅助型遗物 (Support)
        AddRelic(new RelicData("relic_mana_stone", "魔力石", "增加10%最大法力值", 
            RelicType.Support, RelicRarity.Common, 1, 100) 
        { AttributeBonuses = new Dictionary<string, float> { { "max_mana", 0.10f } } });
        
        AddRelic(new RelicData("relic_experience_book", "经验之书", "增加10%经验获取", 
            RelicType.Support, RelicRarity.Uncommon, 1, 300) 
        { SpecialEffect = "exp_boost" });
        
        AddRelic(new RelicData("relic_fortune_coin", "幸运硬币", "增加10%掉落率", 
            RelicType.Support, RelicRarity.Rare, 2, 800) 
        { SpecialEffect = "drop_boost" });
        
        AddRelic(new RelicData("relic_golden_compass", "黄金罗盘", "增加20%任务奖励", 
            RelicType.Support, RelicRarity.Epic, 3, 2000) 
        { SpecialEffect = "quest_reward_boost" });
        
        AddRelic(new RelicData("relic_time_watch", "时之手表", "技能冷却速度加快15%", 
            RelicType.Support, RelicRarity.Legendary, 3, 5000) 
        { SpecialEffect = "cooldown_reduction" });
        
        // 特殊型遗物 (Special)
        AddRelic(new RelicData("relic_ancient_coin", "古钱币", "金币获取增加5%", 
            RelicType.Special, RelicRarity.Common, 1, 100) 
        { SpecialEffect = "gold_boost" });
        
        AddRelic(new RelicData("relic_mystery_box", "神秘盒子", "每天随机获得一个增益效果", 
            RelicType.Special, RelicRarity.Uncommon, 1, 300) 
        { SpecialEffect = "daily_blessing" });
        
        AddRelic(new RelicData("relic_shadow_cloak", "暗影斗篷", "增加10%闪避率", 
            RelicType.Special, RelicRarity.Rare, 2, 800) 
        { AttributeBonuses = new Dictionary<string, float> { { "dodge", 0.10f } } });
        
        AddRelic(new RelicData("relic_soul_jar", "灵魂罐", "击杀敌人时有5%几率获得额外灵魂", 
            RelicType.Special, RelicRarity.Epic, 3, 2000) 
        { SpecialEffect = "soul_gather" });
        
        AddRelic(new RelicData("relic_universe_orb", "宇宙宝珠", "所有属性增加5%", 
            RelicType.Special, RelicRarity.Legendary, 3, 5000) 
        { AttributeBonuses = new Dictionary<string, float> { 
            { "attack", 0.05f }, 
            { "defense", 0.05f },
            { "health", 0.05f },
            { "speed", 0.05f }
        }});
        
        // 工具型遗物 (Utility)
        AddRelic(new RelicData("relic_teleport_stone", "传送石", "可以随时传送到城镇", 
            RelicType.Utility, RelicRarity.Uncommon, 1, 300) 
        { SpecialEffect = "teleport" });
        
        AddRelic(new RelicData("relic_merchant_bag", "商人钱包", "商店物品价格降低5%", 
            RelicType.Utility, RelicRarity.Rare, 2, 800) 
        { SpecialEffect = "discount" });
        
        AddRelic(new RelicData("relic_identify_glass", "鉴定镜", "可以查看物品真实属性", 
            RelicType.Utility, RelicRarity.Epic, 3, 2000) 
        { SpecialEffect = "identify" });
        
        _initialized = true;
        GD.Print($"[RelicDatabase] Initialized with {_relics.Count} relics");
    }
    
    private static void AddRelic(RelicData relic)
    {
        _relics[relic.Id] = relic;
    }
    
    public static RelicData GetRelic(string id)
    {
        if (_relics.TryGetValue(id, out var relic))
            return relic;
        return null;
    }
    
    public static List<RelicData> GetAllRelics()
    {
        return new List<RelicData>(_relics.Values);
    }
    
    public static List<RelicData> GetRelicsByType(RelicType type)
    {
        var result = new List<RelicData>();
        foreach (var relic in _relics.Values)
        {
            if (relic.Type == type)
                result.Add(relic);
        }
        return result;
    }
    
    public static List<RelicData> GetRelicsByRarity(RelicRarity rarity)
    {
        var result = new List<RelicData>();
        foreach (var relic in _relics.Values)
        {
            if (relic.Rarity == rarity)
                result.Add(relic);
        }
        return result;
    }
    
    public static List<RelicData> GetShopRelics()
    {
        var result = new List<RelicData>();
        foreach (var relic in _relics.Values)
        {
            if (relic.Price > 0)
                result.Add(relic);
        }
        return result;
    }
    
    public static RelicData GetRandomRelic(RelicRarity rarity)
    {
        var relics = GetRelicsByRarity(rarity);
        if (relics.Count == 0) return null;
        return relics[GD.Rand() % relics.Count];
    }
}
