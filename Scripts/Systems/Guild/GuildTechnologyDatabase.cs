using System;
using System.Collections.Generic;
using Godot;

public class GuildTechnologyDatabase
{
    private static GuildTechnologyDatabase _instance;
    public static GuildTechnologyDatabase Instance => _instance ??= new GuildTechnologyDatabase();

    public Dictionary<string, GuildTechnologyData.Technology> Technologies { get; private set; } = new Dictionary<string, GuildTechnologyData.Technology>();

    public GuildTechnologyDatabase()
    {
        InitializeTechnologies();
    }

    private void InitializeTechnologies()
    {
        // 战斗类科技
        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "combat_power",
            Name = "战斗之力",
            Description = "提升公会成员的攻击力",
            Category = GuildTechnologyData.TechCategory.Combat,
            Level = GuildTechnologyData.TechLevel.Basic,
            ResearchCost = 100,
            ResearchTime = 300,
            MaxLevel = 5,
            Bonuses = new Dictionary<string, float> { { "attack", 0.05f } }
        });

        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "combat_endurance",
            Name = "战斗耐力",
            Description = "提升公会成员的生命值",
            Category = GuildTechnologyData.TechCategory.Combat,
            Level = GuildTechnologyData.TechLevel.Basic,
            ResearchCost = 100,
            ResearchTime = 300,
            MaxLevel = 5,
            Bonuses = new Dictionary<string, float> { { "health", 0.05f } }
        });

        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "war_training",
            Name = "战争训练",
            Description = "提升公会成员的暴击率",
            Category = GuildTechnologyData.TechCategory.Combat,
            Level = GuildTechnologyData.TechLevel.Advanced,
            ResearchCost = 200,
            ResearchTime = 600,
            MaxLevel = 5,
            Bonuses = new Dictionary<string, float> { { "crit_rate", 0.02f } }
        });

        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "tactical_advantage",
            Name = "战术优势",
            Description = "提升公会成员的暴击伤害",
            Category = GuildTechnologyData.TechCategory.Combat,
            Level = GuildTechnologyData.TechLevel.Master,
            ResearchCost = 400,
            ResearchTime = 1200,
            MaxLevel = 3,
            Bonuses = new Dictionary<string, float> { { "crit_damage", 0.05f } }
        });

        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "legendary_warriors",
            Name = "传奇战士",
            Description = "大幅提升所有战斗属性",
            Category = GuildTechnologyData.TechCategory.Combat,
            Level = GuildTechnologyData.TechLevel.Legendary,
            ResearchCost = 1000,
            ResearchTime = 3600,
            MaxLevel = 1,
            Bonuses = new Dictionary<string, float> { { "attack", 0.1f }, { "health", 0.1f }, { "crit_rate", 0.05f } }
        });

        // 经济类科技
        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "trade_network",
            Name = "贸易网络",
            Description = "提升金币获取",
            Category = GuildTechnologyData.TechCategory.Economy,
            Level = GuildTechnologyData.TechLevel.Basic,
            ResearchCost = 100,
            ResearchTime = 300,
            MaxLevel = 5,
            Bonuses = new Dictionary<string, float> { { "gold_bonus", 0.05f } }
        });

        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "market_influence",
            Name = "市场影响力",
            Description = "商店购物折扣",
            Category = GuildTechnologyData.TechCategory.Economy,
            Level = GuildTechnologyData.TechLevel.Advanced,
            ResearchCost = 200,
            ResearchTime = 600,
            MaxLevel = 3,
            Bonuses = new Dictionary<string, float> { { "shop_discount", 0.03f } }
        });

        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "treasure_mapping",
            Name = "宝藏Mapping",
            Description = "提升宝藏获取概率",
            Category = GuildTechnologyData.TechCategory.Economy,
            Level = GuildTechnologyData.TechLevel.Master,
            ResearchCost = 400,
            ResearchTime = 1200,
            MaxLevel = 3,
            Bonuses = new Dictionary<string, float> { { "treasure_chance", 0.05f } }
        });

        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "economic_empire",
            Name = "经济帝国",
            Description = "大幅提升所有经济收益",
            Category = GuildTechnologyData.TechCategory.Economy,
            Level = GuildTechnologyData.TechLevel.Legendary,
            ResearchCost = 1000,
            ResearchTime = 3600,
            MaxLevel = 1,
            Bonuses = new Dictionary<string, float> { { "gold_bonus", 0.15f }, { "shop_discount", 0.1f } }
        });

        // 生产类科技
        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "crafting_efficiency",
            Name = "制作效率",
            Description = "提升制作成功率",
            Category = GuildTechnologyData.TechCategory.Production,
            Level = GuildTechnologyData.TechLevel.Basic,
            ResearchCost = 100,
            ResearchTime = 300,
            MaxLevel = 5,
            Bonuses = new Dictionary<string, float> { { "craft_success", 0.03f } }
        });

        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "material_knowledge",
            Name = "材料知识",
            Description = "提升材料掉落率",
            Category = GuildTechnologyData.TechCategory.Production,
            Level = GuildTechnologyData.TechLevel.Basic,
            ResearchCost = 100,
            ResearchTime = 300,
            MaxLevel = 5,
            Bonuses = new Dictionary<string, float> { { "material_drop", 0.05f } }
        });

        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "advanced_tools",
            Name = "高级工具",
            Description = "提升采集效率",
            Category = GuildTechnologyData.TechCategory.Production,
            Level = GuildTechnologyData.TechLevel.Advanced,
            ResearchCost = 200,
            ResearchTime = 600,
            MaxLevel = 3,
            Bonuses = new Dictionary<string, float> { { "gathering_speed", 0.1f } }
        });

        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "master_craftsmanship",
            Name = "大师工艺",
            Description = "提升制作暴击概率",
            Category = GuildTechnologyData.TechCategory.Production,
            Level = GuildTechnologyData.TechLevel.Master,
            ResearchCost = 400,
            ResearchTime = 1200,
            MaxLevel = 3,
            Bonuses = new Dictionary<string, float> { { "craft_crit", 0.05f } }
        });

        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "legendary_artisans",
            Name = "传奇工匠",
            Description = "大幅提升所有制作属性",
            Category = GuildTechnologyData.TechCategory.Production,
            Level = GuildTechnologyData.TechLevel.Legendary,
            ResearchCost = 1000,
            ResearchTime = 3600,
            MaxLevel = 1,
            Bonuses = new Dictionary<string, float> { { "craft_success", 0.1f }, { "material_drop", 0.1f } }
        });

        // 社交类科技
        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "member_benefits",
            Name = "会员福利",
            Description = "提升经验获取",
            Category = GuildTechnologyData.TechCategory.Social,
            Level = GuildTechnologyData.TechLevel.Basic,
            ResearchCost = 100,
            ResearchTime = 300,
            MaxLevel = 5,
            Bonuses = new Dictionary<string, float> { { "exp_bonus", 0.05f } }
        });

        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "guild_unity",
            Name = "公会团结",
            Description = "提升公会任务奖励",
            Category = GuildTechnologyData.TechCategory.Social,
            Level = GuildTechnologyData.TechLevel.Advanced,
            ResearchCost = 200,
            ResearchTime = 600,
            MaxLevel = 3,
            Bonuses = new Dictionary<string, float> { { "quest_reward", 0.1f } }
        });

        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "reputation_boost",
            Name = "声望提升",
            Description = "提升声望获取速度",
            Category = GuildTechnologyData.TechCategory.Social,
            Level = GuildTechnologyData.TechLevel.Master,
            ResearchCost = 400,
            ResearchTime = 1200,
            MaxLevel = 3,
            Bonuses = new Dictionary<string, float> { { "reputation_gain", 0.1f } }
        });

        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "alliance_network",
            Name = "联盟网络",
            Description = "解锁更多公会社交功能",
            Category = GuildTechnologyData.TechCategory.Social,
            Level = GuildTechnologyData.TechLevel.Legendary,
            ResearchCost = 1000,
            ResearchTime = 3600,
            MaxLevel = 1,
            Bonuses = new Dictionary<string, float> { { "exp_bonus", 0.1f }, { "quest_reward", 0.15f } }
        });

        // 防御类科技
        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "defensive_walls",
            Name = "防御城墙",
            Description = "提升防御力",
            Category = GuildTechnologyData.TechCategory.Defense,
            Level = GuildTechnologyData.TechLevel.Basic,
            ResearchCost = 100,
            ResearchTime = 300,
            MaxLevel = 5,
            Bonuses = new Dictionary<string, float> { { "defense", 0.05f } }
        });

        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "early_warning",
            Name = "预警系统",
            Description = "提升闪避率",
            Category = GuildTechnologyData.TechCategory.Defense,
            Level = GuildTechnologyData.TechLevel.Basic,
            ResearchCost = 100,
            ResearchTime = 300,
            MaxLevel = 5,
            Bonuses = new Dictionary<string, float> { { "dodge", 0.02f } }
        });

        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "fortress_design",
            Name = "堡垒设计",
            Description = "提升公会战防御",
            Category = GuildTechnologyData.TechCategory.Defense,
            Level = GuildTechnologyData.TechLevel.Advanced,
            ResearchCost = 200,
            ResearchTime = 600,
            MaxLevel = 3,
            Bonuses = new Dictionary<string, float> { { "guild_war_defense", 0.1f } }
        });

        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "emergency_protocols",
            Name = "应急协议",
            Description = "减少死亡惩罚",
            Category = GuildTechnologyData.TechCategory.Defense,
            Level = GuildTechnologyData.TechLevel.Master,
            ResearchCost = 400,
            ResearchTime = 1200,
            MaxLevel = 3,
            Bonuses = new Dictionary<string, float> { { "death_penalty_reduction", 0.1f } }
        });

        AddTechnology(new GuildTechnologyData.Technology
        {
            Id = "impenetrable_fortress",
            Name = "坚不可摧",
            Description = "大幅提升所有防御属性",
            Category = GuildTechnologyData.TechCategory.Defense,
            Level = GuildTechnologyData.TechLevel.Legendary,
            ResearchCost = 1000,
            ResearchTime = 3600,
            MaxLevel = 1,
            Bonuses = new Dictionary<string, float> { { "defense", 0.1f }, { "dodge", 0.05f }, { "health", 0.1f } }
        });
    }

    private void AddTechnology(GuildTechnologyData.Technology tech)
    {
        Technologies[tech.Id] = tech;
    }

    public GuildTechnologyData.Technology GetTechnology(string id)
    {
        return Technologies.GetValueOrDefault(id);
    }

    public List<GuildTechnologyData.Technology> GetTechnologiesByCategory(GuildTechnologyData.TechCategory category)
    {
        List<GuildTechnologyData.Technology> result = new List<GuildTechnologyData.Technology>();
        foreach (var tech in Technologies.Values)
        {
            if (tech.Category == category)
            {
                result.Add(tech);
            }
        }
        return result;
    }

    public List<GuildTechnologyData.Technology> GetAllTechnologies()
    {
        return new List<GuildTechnologyData.Technology>(Technologies.Values);
    }
}
