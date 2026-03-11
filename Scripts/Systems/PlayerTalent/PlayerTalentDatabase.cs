using System;
using System.Collections.Generic;
using Godot;

public class PlayerTalentDatabase
{
    private static PlayerTalentDatabase _instance;
    public static PlayerTalentDatabase Instance
    {
        get
        {
            if (_instance == null) _instance = new PlayerTalentDatabase();
            return _instance;
        }
    }
    
    public Dictionary<string, PlayerTalentData.TalentNode> AllTalents { get; private set; }
    
    public PlayerTalentDatabase()
    {
        AllTalents = new Dictionary<string, PlayerTalentData.TalentNode>();
        InitializeTalents();
    }
    
    private void InitializeTalents()
    {
        // ========== 战斗型天赋树 ==========
        // Tier 1
        AddTalent("combat_1_1", "战斗直觉", "战斗时获得额外经验", 
            PlayerTalentData.TalentTree.Combat, PlayerTalentData.TalentRarity.Basic, 1, 1,
            new Dictionary<string, float> { { "exp_bonus", 0.05f } }, null);
        
        AddTalent("combat_1_2", "力量训练", "提升基础攻击力", 
            PlayerTalentData.TalentTree.Combat, PlayerTalentData.TalentRarity.Basic, 1, 1,
            new Dictionary<string, float> { { "attack_flat", 5f } }, null);
        
        // Tier 2
        AddTalent("combat_2_1", "暴击专精", "提升暴击率", 
            PlayerTalentData.TalentTree.Combat, PlayerTalentData.TalentRarity.Advanced, 2, 2,
            new Dictionary<string, float> { { "crit_rate", 0.03f } }, new List<string> { "combat_1_1" });
        
        AddTalent("combat_2_2", "致命打击", "提升暴击伤害", 
            PlayerTalentData.TalentTree.Combat, PlayerTalentData.TalentRarity.Advanced, 2, 2,
            new Dictionary<string, float> { { "crit_damage", 0.10f } }, new List<string> { "combat_1_2" });
        
        // Tier 3
        AddTalent("combat_3_1", "战斗狂热", "攻击时有一定几率提升攻击速度", 
            PlayerTalentData.TalentTree.Combat, PlayerTalentData.TalentRarity.Expert, 3, 3,
            new Dictionary<string, float> { { "attack_speed", 0.10f }, { "attack_speed_chance", 0.10f } }, new List<string> { "combat_2_1" });
        
        AddTalent("combat_3_2", "斩击", "提升对高血量敌人的伤害", 
            PlayerTalentData.TalentTree.Combat, PlayerTalentData.TalentRarity.Expert, 3, 3,
            new Dictionary<string, float> { { "damage_above_50hp", 0.15f } }, new List<string> { "combat_2_2" });
        
        // Tier 4
        AddTalent("combat_4_1", "无双", "大幅度提升攻击力", 
            PlayerTalentData.TalentTree.Combat, PlayerTalentData.TalentRarity.Master, 4, 5,
            new Dictionary<string, float> { { "attack_flat", 25f }, { "attack_percent", 0.10f } }, new List<string> { "combat_3_1" });
        
        AddTalent("combat_4_2", "收割者", "击杀敌人时恢复生命", 
            PlayerTalentData.TalentTree.Combat, PlayerTalentData.TalentRarity.Master, 4, 5,
            new Dictionary<string, float> { { "lifesteal", 0.05f } }, new List<string> { "combat_3_2" });
        
        // Tier 5
        AddTalent("combat_5_1", "战争之王", "所有战斗属性大幅提升", 
            PlayerTalentData.TalentTree.Combat, PlayerTalentData.TalentRarity.Master, 5, 8,
            new Dictionary<string, float> { { "attack_percent", 0.20f }, { "crit_rate", 0.05f }, { "crit_damage", 0.15f } }, new List<string> { "combat_4_1", "combat_4_2" });
        
        // ========== 防御型天赋树 ==========
        // Tier 1
        AddTalent("defense_1_1", "铁壁", "提升基础防御力", 
            PlayerTalentData.TalentTree.Defense, PlayerTalentData.TalentRarity.Basic, 1, 1,
            new Dictionary<string, float> { { "defense_flat", 5f } }, null);
        
        AddTalent("defense_1_2", "生命强化", "提升最大生命值", 
            PlayerTalentData.TalentTree.Defense, PlayerTalentData.TalentRarity.Basic, 1, 1,
            new Dictionary<string, float> { { "health_flat", 50f } }, null);
        
        // Tier 2
        AddTalent("defense_2_1", "闪避专精", "提升闪避率", 
            PlayerTalentData.TalentTree.Defense, PlayerTalentData.TalentRarity.Advanced, 2, 2,
            new Dictionary<string, float> { { "dodge", 0.03f } }, new List<string> { "defense_1_1" });
        
        AddTalent("defense_2_2", "再生", "提升生命恢复速度", 
            PlayerTalentData.TalentTree.Defense, PlayerTalentData.TalentRarity.Advanced, 2, 2,
            new Dictionary<string, float> { { "health_regen", 0.10f } }, new List<string> { "defense_1_2" });
        
        // Tier 3
        AddTalent("defense_3_1", "荆棘", "受到攻击时反弹伤害", 
            PlayerTalentData.TalentTree.Defense, PlayerTalentData.TalentRarity.Expert, 3, 3,
            new Dictionary<string, float> { { "thorns", 0.10f } }, new List<string> { "defense_2_1" });
        
        AddTalent("defense_3_2", "护盾", "获得额外护盾效果", 
            PlayerTalentData.TalentTree.Defense, PlayerTalentData.TalentRarity.Expert, 3, 3,
            new Dictionary<string, float> { { "shield_bonus", 0.15f } }, new List<string> { "defense_2_2" });
        
        // Tier 4
        AddTalent("defense_4_1", "不动如山", "大幅提升防御力", 
            PlayerTalentData.TalentTree.Defense, PlayerTalentData.TalentRarity.Master, 4, 5,
            new Dictionary<string, float> { { "defense_flat", 25f }, { "defense_percent", 0.15f } }, new List<string> { "defense_3_1" });
        
        AddTalent("defense_4_2", "不死之身", "大幅提升生命值和恢复", 
            PlayerTalentData.TalentTree.Defense, PlayerTalentData.TalentRarity.Master, 4, 5,
            new Dictionary<string, float> { { "health_flat", 200f }, { "health_regen", 0.20f } }, new List<string> { "defense_3_2" });
        
        // Tier 5
        AddTalent("defense_5_1", "守护天使", "所有防御属性大幅提升", 
            PlayerTalentData.TalentTree.Defense, PlayerTalentData.TalentRarity.Master, 5, 8,
            new Dictionary<string, float> { { "defense_percent", 0.25f }, { "health_percent", 0.15f }, { "dodge", 0.05f } }, new List<string> { "defense_4_1", "defense_4_2" });
        
        // ========== 辅助型天赋树 ==========
        // Tier 1
        AddTalent("support_1_1", "经验加成", "获得额外经验", 
            PlayerTalentData.TalentTree.Support, PlayerTalentData.TalentRarity.Basic, 1, 1,
            new Dictionary<string, float> { { "exp_bonus", 0.10f } }, null);
        
        AddTalent("support_1_2", "金币加成", "获得额外金币", 
            PlayerTalentData.TalentTree.Support, PlayerTalentData.TalentRarity.Basic, 1, 1,
            new Dictionary<string, float> { { "gold_bonus", 0.10f } }, null);
        
        // Tier 2
        AddTalent("support_2_1", "掉落提升", "提升物品掉落率", 
            PlayerTalentData.TalentTree.Support, PlayerTalentData.TalentRarity.Advanced, 2, 2,
            new Dictionary<string, float> { { "drop_rate", 0.10f } }, new List<string> { "support_1_1" });
        
        AddTalent("support_2_2", "交易大师", "商店出售价格更高", 
            PlayerTalentData.TalentTree.Support, PlayerTalentData.TalentRarity.Advanced, 2, 2,
            new Dictionary<string, float> { { "sell_price", 0.15f } }, new List<string> { "support_1_2" });
        
        // Tier 3
        AddTalent("support_3_1", "幸运之星", "提升稀有掉落几率", 
            PlayerTalentData.TalentTree.Support, PlayerTalentData.TalentRarity.Expert, 3, 3,
            new Dictionary<string, float> { { "rare_drop", 0.05f } }, new List<string> { "support_2_1" });
        
        AddTalent("support_3_2", "锻造大师", "强化成功率提升", 
            PlayerTalentData.TalentTree.Support, PlayerTalentData.TalentRarity.Expert, 3, 3,
            new Dictionary<string, float> { { "enhance_success", 0.10f } }, new List<string> { "support_2_2" });
        
        // Tier 4
        AddTalent("support_4_1", "聚宝盆", "大幅提升金币获取", 
            PlayerTalentData.TalentTree.Support, PlayerTalentData.TalentRarity.Master, 4, 5,
            new Dictionary<string, float> { { "gold_bonus", 0.25f }, { "drop_rate", 0.15f } }, new List<string> { "support_3_1" });
        
        AddTalent("support_4_2", "大师学者", "大幅提升经验获取", 
            PlayerTalentData.TalentTree.Support, PlayerTalentData.TalentRarity.Master, 4, 5,
            new Dictionary<string, float> { { "exp_bonus", 0.25f } }, new List<string> { "support_3_2" });
        
        // Tier 5
        AddTalent("support_5_1", "富甲天下", "所有经济属性大幅提升", 
            PlayerTalentData.TalentTree.Support, PlayerTalentData.TalentRarity.Master, 5, 8,
            new Dictionary<string, float> { { "gold_bonus", 0.40f }, { "exp_bonus", 0.30f }, { "drop_rate", 0.20f }, { "rare_drop", 0.10f } }, new List<string> { "support_4_1", "support_4_2" });
        
        // ========== 敏捷型天赋树 ==========
        // Tier 1
        AddTalent("agility_1_1", "速度强化", "提升移动速度", 
            PlayerTalentData.TalentTree.Agility, PlayerTalentData.TalentRarity.Basic, 1, 1,
            new Dictionary<string, float> { { "move_speed", 0.05f } }, null);
        
        AddTalent("agility_1_2", "灵巧", "提升攻击速度", 
            PlayerTalentData.TalentTree.Agility, PlayerTalentData.TalentRarity.Basic, 1, 1,
            new Dictionary<string, float> { { "attack_speed", 0.05f } }, null);
        
        // Tier 2
        AddTalent("agility_2_1", "疾跑", "提升闪避和移动", 
            PlayerTalentData.TalentTree.Agility, PlayerTalentData.TalentRarity.Advanced, 2, 2,
            new Dictionary<string, float> { { "move_speed", 0.10f }, { "dodge", 0.02f } }, new List<string> { "agility_1_1" });
        
        AddTalent("agility_2_2", "连击", "提升攻击速度", 
            PlayerTalentData.TalentTree.Agility, PlayerTalentData.TalentRarity.Advanced, 2, 2,
            new Dictionary<string, float> { { "attack_speed", 0.10f } }, new List<string> { "agility_1_2" });
        
        // Tier 3
        AddTalent("agility_3_1", "疾风", "大幅提升移动速度", 
            PlayerTalentData.TalentTree.Agility, PlayerTalentData.TalentRarity.Expert, 3, 3,
            new Dictionary<string, float> { { "move_speed", 0.20f } }, new List<string> { "agility_2_1" });
        
        AddTalent("agility_3_2", "闪电打击", "大幅提升攻击速度", 
            PlayerTalentData.TalentTree.Agility, PlayerTalentData.TalentRarity.Expert, 3, 3,
            new Dictionary<string, float> { { "attack_speed", 0.20f } }, new List<string> { "agility_2_2" });
        
        // Tier 4
        AddTalent("agility_4_1", "风行者", "移动时无敌", 
            PlayerTalentData.TalentTree.Agility, PlayerTalentData.TalentRarity.Master, 4, 5,
            new Dictionary<string, float> { { "move_dodge_chance", 0.05f } }, new List<string> { "agility_3_1" });
        
        AddTalent("agility_4_2", "幻影", "攻击有一定几率触发额外攻击", 
            PlayerTalentData.TalentTree.Agility, PlayerTalentData.TalentRarity.Master, 4, 5,
            new Dictionary<string, float> { { "extra_attack_chance", 0.08f } }, new List<string> { "agility_3_2" });
        
        // Tier 5
        AddTalent("agility_5_1", "闪电侠", "所有敏捷属性大幅提升", 
            PlayerTalentData.TalentTree.Agility, PlayerTalentData.TalentRarity.Master, 5, 8,
            new Dictionary<string, float> { { "move_speed", 0.35f }, { "attack_speed", 0.30f }, { "dodge", 0.05f } }, new List<string> { "agility_4_1", "agility_4_2" });
    }
    
    private void AddTalent(string id, string name, string desc, PlayerTalentData.TalentTree tree, 
        PlayerTalentData.TalentRarity rarity, int tier, int cost, 
        Dictionary<string, float> bonuses, List<string> requires)
    {
        AllTalents[id] = new PlayerTalentData.TalentNode
        {
            Id = id,
            Name = name,
            Description = desc,
            Tree = tree,
            Rarity = rarity,
            Tier = tier,
            Cost = cost,
            Bonuses = bonuses,
            Requires = requires ?? new List<string>()
        };
    }
    
    public List<PlayerTalentData.TalentNode> GetTalentsByTree(PlayerTalentData.TalentTree tree)
    {
        List<PlayerTalentData.TalentNode> result = new List<PlayerTalentData.TalentNode>();
        foreach (var talent in AllTalents.Values)
        {
            if (talent.Tree == tree)
                result.Add(talent);
        }
        result.Sort((a, b) => a.Tier.CompareTo(b.Tier));
        return result;
    }
    
    public PlayerTalentData.TalentNode GetTalent(string id)
    {
        return AllTalents.ContainsKey(id) ? AllTalents[id] : null;
    }
}
