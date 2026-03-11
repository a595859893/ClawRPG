using Godot;
using System;
using System.Collections.Generic;

public class TitleDatabase
{
    private static TitleDatabase _instance;
    public static TitleDatabase Instance
    {
        get
        {
            if (_instance == null)
                _instance = new TitleDatabase();
            return _instance;
        }
    }
    
    public Dictionary<string, TitleDefinition> AllTitles = new Dictionary<string, TitleDefinition>();
    public Dictionary<TitleCategory, List<string>> TitlesByCategory = new Dictionary<TitleCategory, List<string>>();
    public Dictionary<TitleRarity, List<string>> TitlesByRarity = new Dictionary<TitleRarity, List<string>>();
    
    public TitleDatabase()
    {
        InitializeTitles();
    }
    
    private void InitializeTitles()
    {
        // 初始化类别索引
        foreach (TitleCategory cat in Enum.GetValues(typeof(TitleCategory)))
        {
            TitlesByCategory[cat] = new List<string>();
        }
        foreach (TitleRarity rar in Enum.GetValues(typeof(TitleRarity)))
        {
            TitlesByRarity[rar] = new List<string>();
        }
        
        // ========== 战斗称号 ==========
        AddTitle(new TitleDefinition
        {
            Id = "combat_novice",
            Name = "初战告捷",
            Description = "击败第一个敌人",
            Category = TitleCategory.Combat,
            Rarity = TitleRarity.Common,
            AttributeBonuses = new Dictionary<string, float> { { "attack", 1 } }
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "combat_veteran",
            Name = "战斗老兵",
            Description = "累计击败100个敌人",
            Category = TitleCategory.Combat,
            Rarity = TitleRarity.Rare,
            AttributeBonuses = new Dictionary<string, float> { { "attack", 3 }, { "defense", 2 } }
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "boss_slayer",
            Name = "Boss杀手",
            Description = "击败10个Boss",
            Category = TitleCategory.Combat,
            Rarity = TitleRarity.Epic,
            AttributeBonuses = new Dictionary<string, float> { { "attack", 5 }, { "crit_rate", 2 } }
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "dragon_slayer",
            Name = "屠龙者",
            Description = "击败巨龙Boss",
            Category = TitleCategory.Combat,
            Rarity = TitleRarity.Legendary,
            AttributeBonuses = new Dictionary<string, float> { { "attack", 10 }, { "crit_damage", 15 }, { "life", 100 } }
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "unstoppable",
            Name = "势不可挡",
            Description = "完成10连杀",
            Category = TitleCategory.Combat,
            Rarity = TitleRarity.Rare,
            AttributeBonuses = new Dictionary<string, float> { { "attack", 4 }, { "attack_speed", 3 } }
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "critical_master",
            Name = "暴击大师",
            Description = "累计暴击100次",
            Category = TitleCategory.Combat,
            Rarity = TitleRarity.Rare,
            AttributeBonuses = new Dictionary<string, float> { { "crit_rate", 5 }, { "crit_damage", 10 } }
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "tank_master",
            Name = "铜墙铁壁",
            Description = "受到1000点伤害但未死亡",
            Category = TitleCategory.Combat,
            Rarity = TitleRarity.Rare,
            AttributeBonuses = new Dictionary<string, float> { { "defense", 5 }, { "max_health", 50 } }
        });
        
        // ========== 采集称号 ==========
        AddTitle(new TitleDefinition
        {
            Id = "fisherman",
            Name = "渔夫",
            Description = "成功钓鱼10次",
            Category = TitleCategory.Gathering,
            Rarity = TitleRarity.Common,
            AttributeBonuses = new Dictionary<string, float> { { "fishing_skill", 2 } }
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "master_fisher",
            Name = "钓鱼大师",
            Description = "钓鱼技能达到满级",
            Category = TitleCategory.Gathering,
            Rarity = TitleRarity.Epic,
            AttributeBonuses = new Dictionary<string, float> { { "fishing_skill", 10 }, { "luck", 5 } }
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "alchemist",
            Name = "炼金师",
            Description = "成功制作50次药水",
            Category = TitleCategory.Gathering,
            Rarity = TitleRarity.Rare,
            AttributeBonuses = new Dictionary<string, float> { { "alchemy_skill", 3 } }
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "master_alchemist",
            Name = "大宗师",
            Description = "炼金等级达到满级",
            Category = TitleCategory.Gathering,
            Rarity = TitleRarity.Legendary,
            AttributeBonuses = new Dictionary<string, float> { { "alchemy_skill", 10 }, { "max_mana", 50 } }
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "miner",
            Name = "矿工",
            Description = "挖掘矿石50次",
            Category = TitleCategory.Gathering,
            Rarity = TitleRarity.Common,
            AttributeBonuses = new Dictionary<string, float> { { "mining_skill", 2 } }
        });
        
        // ========== 探索称号 ==========
        AddTitle(new TitleDefinition
        {
            Id = "explorer",
            Name = "探索者",
            Description = "进入5个不同区域",
            Category = TitleCategory.Exploration,
            Rarity = TitleRarity.Common,
            AttributeBonuses = new Dictionary<string, float> { { "movement_speed", 2 } }
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "collector",
            Name = "收藏家",
            Description = "收集100件不同物品",
            Category = TitleCategory.Exploration,
            Rarity = TitleRarity.Rare,
            AttributeBonuses = new Dictionary<string, float> { { "luck", 5 }, { "gold_gain", 3 } }
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "world_conqueror",
            Name = "世界征服者",
            Description = "通关所有区域",
            Category = TitleCategory.Exploration,
            Rarity = TitleRarity.Epic,
            AttributeBonuses = new Dictionary<string, float> { { "attack", 5 }, { "defense", 5 }, { "max_health", 100 } }
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "treasure_hunter",
            Name = "寻宝猎人",
            Description = "找到50个收藏点",
            Category = TitleCategory.Exploration,
            Rarity = TitleRarity.Rare,
            AttributeBonuses = new Dictionary<string, float> { { "luck", 8 }, { "item_drop", 5 } }
        });
        
        // ========== 社交称号 ==========
        AddTitle(new TitleDefinition
        {
            Id = "team_leader",
            Name = "团队领袖",
            Description = "组建队伍并完成10次副本",
            Category = TitleCategory.Social,
            Rarity = TitleRarity.Rare,
            AttributeBonuses = new Dictionary<string, float> { { "leadership", 5 }, { "exp_gain", 3 } }
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "mentor",
            Name = "导师",
            Description = "帮助其他玩家升级10次",
            Category = TitleCategory.Social,
            Rarity = TitleRarity.Epic,
            AttributeBonuses = new Dictionary<string, float> { { "exp_gain", 8 }, { "luck", 3 } }
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "merchant",
            Name = "商人",
            Description = "在拍卖行卖出100件物品",
            Category = TitleCategory.Social,
            Rarity = TitleRarity.Rare,
            AttributeBonuses = new Dictionary<string, float> { { "gold_gain", 5 }, { "trade_discount", 3 } }
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "social_butterfly",
            Name = "社交蝴蝶",
            Description = "添加50个好友",
            Category = TitleCategory.Social,
            Rarity = TitleRarity.Common,
            AttributeBonuses = new Dictionary<string, float> { { "charisma", 3 } }
        });
        
        // ========== 特殊称号 ==========
        AddTitle(new TitleDefinition
        {
            Id = "champion",
            Name = "冠军",
            Description = "获得竞技场第一名",
            Category = TitleCategory.Special,
            Rarity = TitleRarity.Legendary,
            AttributeBonuses = new Dictionary<string, float> { { "attack", 10 }, { "defense", 10 }, { "crit_rate", 5 } },
            IsSecret = false
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "legend",
            Name = "传奇",
            Description = "角色等级达到满级",
            Category = TitleCategory.Special,
            Rarity = TitleRarity.Legendary,
            AttributeBonuses = new Dictionary<string, float> { { "attack", 15 }, { "defense", 15 }, { "max_health", 200 }, { "max_mana", 100 } },
            IsSecret = false
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "millionaire",
            Name = "百万富翁",
            Description = "拥有100万金币",
            Category = TitleCategory.Special,
            Rarity = TitleRarity.Epic,
            AttributeBonuses = new Dictionary<string, float> { { "gold_gain", 10 }, { "trade_discount", 5 } }
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "perfectionist",
            Name = "完美主义者",
            Description = "解锁所有其他称号",
            Category = TitleCategory.Special,
            Rarity = TitleRarity.Legendary,
            AttributeBonuses = new Dictionary<string, float> { { "attack", 5 }, { "defense", 5 }, { "luck", 10 }, { "all_attributes", 5 } },
            IsSecret = true
        });
        
        AddTitle(new TitleDefinition
        {
            Id = "first_blood",
            Name = "先驱者",
            Description = "创建第一个角色",
            Category = TitleCategory.Special,
            Rarity = TitleRarity.Common,
            AttributeBonuses = new Dictionary<string, float> { { "exp_gain", 2 } }
        });
    }
    
    private void AddTitle(TitleDefinition title)
    {
        AllTitles[title.Id] = title;
        TitlesByCategory[title.Category].Add(title.Id);
        TitlesByRarity[title.Rarity].Add(title.Id);
    }
    
    public TitleDefinition GetTitle(string id)
    {
        if (AllTitles.ContainsKey(id))
            return AllTitles[id];
        return null;
    }
    
    public List<TitleDefinition> GetTitlesByCategory(TitleCategory category)
    {
        List<TitleDefinition> result = new List<TitleDefinition>();
        if (TitlesByCategory.ContainsKey(category))
        {
            foreach (string id in TitlesByCategory[category])
            {
                result.Add(AllTitles[id]);
            }
        }
        return result;
    }
    
    public List<TitleDefinition> GetTitlesByRarity(TitleRarity rarity)
    {
        List<TitleDefinition> result = new List<TitleDefinition>();
        if (TitlesByRarity.ContainsKey(rarity))
        {
            foreach (string id in TitlesByRarity[rarity])
            {
                result.Add(AllTitles[id]);
            }
        }
        return result;
    }
    
    public List<TitleDefinition> GetAllTitles()
    {
        return new List<TitleDefinition>(AllTitles.Values);
    }
    
    public string GetRarityColor(TitleRarity rarity)
    {
        switch (rarity)
        {
            case TitleRarity.Common: return "#FFFFFF";
            case TitleRarity.Rare: return "#00FF00";
            case TitleRarity.Epic: return "#FF00FF";
            case TitleRarity.Legendary: return "#FFA500";
            default: return "#FFFFFF";
        }
    }
}
