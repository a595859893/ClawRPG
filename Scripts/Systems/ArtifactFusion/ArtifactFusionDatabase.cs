using Godot;
using System;
using System.Collections.Generic;

public class ArtifactFusionDatabase : BaseSystem
{
    // 融合配方
    public static List<FusionRecipe> Recipes { get; private set; } = new List<FusionRecipe>();
    
    // 运行时数据（非静态）
    public Dictionary<string, int> RecipeUsageCount { get; set; } = new Dictionary<string, int>();
    public List<string> RecentlyUsedRecipes { get; set; } = new List<string>();
    
    // 神器类型权重
    public static Dictionary<string, float> ArtifactWeights { get; private set; } = new Dictionary<string, float>
    {
        { "Common", 40f },
        { "Uncommon", 30f },
        { "Rare", 18f },
        { "Epic", 8f },
        { "Legendary", 4f }
    };
    
    // 稀有度颜色
    public static Dictionary<string, Color> RarityColors { get; private set; } = new Dictionary<string, Color>
    {
        { "Common", new Color(0.7f, 0.7f, 0.7f) },
        { "Uncommon", new Color(0.2f, 0.8f, 0.2f) },
        { "Rare", new Color(0.2f, 0.5f, 1.0f) },
        { "Epic", new Color(0.6f, 0.2f, 0.8f) },
        { "Legendary", new Color(1.0f, 0.6f, 0.0f) }
    };
    
    public override void _Ready()
    {
        InitializeRecipes();
    }
    
    private void InitializeRecipes()
    {
        Recipes.Clear();
        
        // 武器融合配方
        Recipes.Add(new FusionRecipe
        {
            Id = "sword_fire",
            Name = "火焰剑",
            Description = "将火焰之力注入武器",
            Artifact1 = "Fire Sword",
            Artifact2 = "Flame Amulet",
            ResultArtifact = "Inferno Blade",
            ResultRarity = "Epic",
            SuccessRate = 0.35f,
            RequiredLevel = 15,
            GoldCost = 500
        });
        
        Recipes.Add(new FusionRecipe
        {
            Id = "blade_shadow",
            Name = "暗影之刃",
            Description = "融合暗影能量的致命武器",
            Artifact1 = "Shadow Dagger",
            Artifact2 = "Dark Essence",
            ResultArtifact = "Void Reaper",
            ResultRarity = "Legendary",
            SuccessRate = 0.15f,
            RequiredLevel = 30,
            GoldCost = 2000
        });
        
        // 护甲融合配方
        Recipes.Add(new FusionRecipe
        {
            Id = "armor_ice",
            Name = "冰霜护甲",
            Description = "寒冰之力守护穿戴者",
            Artifact1 = "Frost Plate",
            Artifact2 = "Ice Crystal",
            ResultArtifact = "Glacial Armor",
            ResultRarity = "Epic",
            SuccessRate = 0.30f,
            RequiredLevel = 20,
            GoldCost = 800
        });
        
        Recipes.Add(new FusionRecipe
        {
            Id = "shield_light",
            Name = "光明圣盾",
            Description = "圣光加持的防御屏障",
            Artifact1 = "Holy Shield",
            Artifact2 = "Light Orb",
            ResultArtifact = "Divine Aegis",
            ResultRarity = "Legendary",
            SuccessRate = 0.12f,
            RequiredLevel = 35,
            GoldCost = 3000
        });
        
        // 饰品融合配方
        Recipes.Add(new FusionRecipe
        {
            Id = "ring_thunder",
            Name = "雷鸣戒指",
            Description = "雷电之力的凝聚",
            Artifact1 = "Lightning Ring",
            Artifact2 = "Thunder Stone",
            ResultArtifact = "Storm Lord's Ring",
            ResultRarity = "Epic",
            SuccessRate = 0.28f,
            RequiredLevel = 18,
            GoldCost = 600
        });
        
        Recipes.Add(new FusionRecipe
        {
            Id = "amulet_life",
            Name = "生命护符",
            Description = "生命能量的结晶",
            Artifact1 = "Health Pendant",
            Artifact2 = "Life Crystal",
            ResultArtifact = "Soul Anchor",
            ResultRarity = "Rare",
            SuccessRate = 0.45f,
            RequiredLevel = 10,
            GoldCost = 300
        });
        
        Recipes.Add(new FusionRecipe
        {
            Id = "necklace_dragon",
            Name = "巨龙之证",
            Description = "龙族力量的传承",
            Artifact1 = "Dragon Scale",
            Artifact2 = "Dragon Heart",
            ResultArtifact = "Dragon Lord Pendant",
            ResultRarity = "Legendary",
            SuccessRate = 0.10f,
            RequiredLevel = 40,
            GoldCost = 5000
        });
        
        // 混合类型配方
        Recipes.Add(new FusionRecipe
        {
            Id = "chaos_orb",
            Name = "混沌宝珠",
            Description = "融合多种元素的神秘宝珠",
            Artifact1 = "Fire Orb",
            Artifact2 = "Ice Orb",
            ResultArtifact = "Chaos Orb",
            ResultRarity = "Epic",
            SuccessRate = 0.20f,
            RequiredLevel = 25,
            GoldCost = 1500
        });
        
        Recipes.Add(new FusionRecipe
        {
            Id = "phoenix_wings",
            Name = "凤凰之翼",
            Description = "浴火重生的翅膀",
            Artifact1 = "Angel Wings",
            Artifact2 = "Phoenix Feather",
            ResultArtifact = "Phoenix Wings",
            ResultRarity = "Legendary",
            SuccessRate = 0.08f,
            RequiredLevel = 45,
            GoldCost = 8000
        });
        
        Recipes.Add(new FusionRecipe
        {
            Id = "void_armor",
            Name = "虚空战甲",
            Description = "来自虚空的终极防御",
            Artifact1 = "Dark Armor",
            Artifact2 = "Void Essence",
            ResultArtifact = "Void Lord Armor",
            ResultRarity = "Legendary",
            SuccessRate = 0.06f,
            RequiredLevel = 50,
            GoldCost = 10000
        });
        
        // 普通融合配方 (随机结果)
        Recipes.Add(new FusionRecipe
        {
            Id = "random_legendary",
            Name = "传奇融合",
            Description = "有几率获得传奇神器",
            Artifact1 = "Ancient Relic",
            Artifact2 = "Mystical Crystal",
            ResultArtifact = "",
            ResultRarity = "Legendary",
            SuccessRate = 0.05f,
            RequiredLevel = 35,
            GoldCost = 5000,
            IsRandomResult = true
        });
        
        Recipes.Add(new FusionRecipe
        {
            Id = "random_epic",
            Name = "史诗融合",
            Description = "有几率获得史诗神器",
            Artifact1 = "Mystic Artifact",
            Artifact2 = "Enchanted Gem",
            ResultArtifact = "",
            ResultRarity = "Epic",
            SuccessRate = 0.15f,
            RequiredLevel = 20,
            GoldCost = 1500,
            IsRandomResult = true
        });
        
        Recipes.Add(new FusionRecipe
        {
            Id = "random_rare",
            Name = "稀有融合",
            Description = "有几率获得稀有神器",
            Artifact1 = "Common Relic",
            Artifact2 = "Gemstone",
            ResultArtifact = "",
            ResultRarity = "Rare",
            SuccessRate = 0.30f,
            RequiredLevel = 10,
            GoldCost = 500,
            IsRandomResult = true
        });
        
        // 武器普通融合
        Recipes.Add(new FusionRecipe
        {
            Id = "weapon_common",
            Name = "普通武器融合",
            Description = "随机获得武器",
            Artifact1 = "Iron Sword",
            Artifact2 = "Steel Dagger",
            ResultArtifact = "",
            ResultRarity = "Common",
            SuccessRate = 0.60f,
            RequiredLevel = 1,
            GoldCost = 100,
            IsRandomResult = true,
            FusionType = FusionType.Weapon
        });
        
        // 护甲普通融合
        Recipes.Add(new FusionRecipe
        {
            Id = "armor_common",
            Name = "普通护甲融合",
            Description = "随机获得护甲",
            Artifact1 = "Leather Armor",
            Artifact2 = "Chain Mail",
            ResultArtifact = "",
            ResultRarity = "Common",
            SuccessRate = 0.60f,
            RequiredLevel = 1,
            GoldCost = 100,
            IsRandomResult = true,
            FusionType = FusionType.Armor
        });
        
        // 饰品普通融合
        Recipes.Add(new FusionRecipe
        {
            Id = "accessory_common",
            Name = "普通饰品融合",
            Description = "随机获得饰品",
            Artifact1 = "Copper Ring",
            Artifact2 = "Silver Amulet",
            ResultArtifact = "",
            ResultRarity = "Common",
            SuccessRate = 0.60f,
            RequiredLevel = 1,
            GoldCost = 100,
            IsRandomResult = true,
            FusionType = FusionType.Accessory
        });
    }
    
    public static FusionRecipe GetRecipe(string id)
    {
        foreach (var recipe in Recipes)
        {
            if (recipe.Id == id)
                return recipe;
        }
        return null;
    }
    
    public static List<FusionRecipe> GetRecipesByRarity(string rarity)
    {
        List<FusionRecipe> result = new List<FusionRecipe>();
        foreach (var recipe in Recipes)
        {
            if (recipe.ResultRarity == rarity)
                result.Add(recipe);
        }
        return result;
    }
    
    public static List<FusionRecipe> GetRecipesByType(FusionType type)
    {
        List<FusionRecipe> result = new List<FusionRecipe>();
        foreach (var recipe in Recipes)
        {
            if (recipe.FusionType == type)
                result.Add(recipe);
        }
        return result;
    }
    
    public static string GetRandomArtifactByRarity(string rarity)
    {
        // 根据稀有度返回随机神器名称
        var random = new Random();
        
        switch (rarity)
        {
            case "Common":
                string[] common = { "Iron Sword", "Leather Armor", "Copper Ring", "Wooden Shield", "Basic Amulet" };
                return common[random.Next(common.Length)];
            case "Uncommon":
                string[] uncommon = { "Steel Sword", "Chain Mail", "Silver Ring", "Bronze Shield", "Magic Amulet" };
                return uncommon[random.Next(uncommon.Length)];
            case "Rare":
                string[] rare = { "Mithril Blade", "Dragon Scale Mail", "Golden Ring", "Enchanted Shield", "Ruby Amulet" };
                return rare[random.Next(rare.Length)];
            case "Epic":
                string[] epic = { "Demonic Blade", "Void Armor", "Ethereal Ring", "Sacred Shield", "Sapphire Amulet" };
                return epic[random.Next(epic.Length)];
            case "Legendary":
                string[] legendary = { "Excalibur", "Godly Plate", "Infinity Ring", "Divine Shield", "Phoenix Amulet" };
                return legendary[random.Next(legendary.Length)];
            default:
                return "Unknown Artifact";
        }
    }

    #region Data Types

    public class FusionRecipe
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Artifact1 { get; set; } = "";
        public string Artifact2 { get; set; } = "";
        public string ResultArtifact { get; set; } = "";
        public string ResultRarity { get; set; } = "Common";
        public float SuccessRate { get; set; } = 0.5f;
        public int RequiredLevel { get; set; } = 1;
        public int GoldCost { get; set; } = 100;
        public bool IsRandomResult { get; set; } = false;
        public FusionType FusionType { get; set; } = FusionType.Mixed;
    }

    #endregion

    #region Persistence

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        // ArtifactFusionDatabase 是静态配置数据，不需要持久化
        return data;
    }

    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        // ArtifactFusionDatabase 是静态配置数据，不需要持久化
    }

    #endregion
}
