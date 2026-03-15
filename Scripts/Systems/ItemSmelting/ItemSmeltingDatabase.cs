using Godot;
using System;
using System.Collections.Generic;

public class ItemSmeltingDatabase : BaseSystem
{
    // Recipe configurations
    public Dictionary<string, SmeltingRecipe> Recipes = new Dictionary<string, SmeltingRecipe>();
    
    // Material types
    public Dictionary<string, MaterialInfo> Materials = new Dictionary<string, MaterialInfo>();
    
    // Equipment type configs
    public Dictionary<string, float> EquipmentSmeltMultipliers = new Dictionary<string, float>();
    
    public override void _Ready()
    {
        InitializeRecipes();
        InitializeMaterials();
        InitializeMultipliers();
    }
    
    void InitializeRecipes()
    {
        // Common recipes
        Recipes["common_weapon"] = new SmeltingRecipe
        {
            Id = "common_weapon",
            Name = "Common Weapon Smelting",
            InputType = "Weapon",
            InputRarity = "Common",
            OutputMaterials = new Dictionary<string, int>
            {
                ["iron_scrap"] = 1,
                ["common_essence"] = 1
            },
            GoldCost = 10,
            SuccessRate = 0.95f,
            RequiredLevel = 1
        };
        
        Recipes["common_armor"] = new SmeltingRecipe
        {
            Id = "common_armor",
            Name = "Common Armor Smelting",
            InputType = "Armor",
            InputRarity = "Common",
            OutputMaterials = new Dictionary<string, int>
            {
                ["iron_scrap"] = 2,
                ["common_essence"] = 1
            },
            GoldCost = 10,
            SuccessRate = 0.95f,
            RequiredLevel = 1
        };
        
        Recipes["common_accessory"] = new SmeltingRecipe
        {
            Id = "common_accessory",
            Name = "Common Accessory Smelting",
            InputType = "Accessory",
            InputRarity = "Common",
            OutputMaterials = new Dictionary<string, int>
            {
                ["common_essence"] = 2
            },
            GoldCost = 5,
            SuccessRate = 0.95f,
            RequiredLevel = 1
        };
        
        // Uncommon recipes
        Recipes["uncommon_weapon"] = new SmeltingRecipe
        {
            Id = "uncommon_weapon",
            Name = "Uncommon Weapon Smelting",
            InputType = "Weapon",
            InputRarity = "Uncommon",
            OutputMaterials = new Dictionary<string, int>
            {
                ["steel_scrap"] = 2,
                ["uncommon_essence"] = 2,
                ["gem_fragment"] = 1
            },
            GoldCost = 25,
            SuccessRate = 0.90f,
            RequiredLevel = 5
        };
        
        Recipes["uncommon_armor"] = new SmeltingRecipe
        {
            Id = "uncommon_armor",
            Name = "Uncommon Armor Smelting",
            InputType = "Armor",
            InputRarity = "Uncommon",
            OutputMaterials = new Dictionary<string, int>
            {
                ["steel_scrap"] = 3,
                ["uncommon_essence"] = 2,
                ["gem_fragment"] = 1
            },
            GoldCost = 25,
            SuccessRate = 0.90f,
            RequiredLevel = 5
        };
        
        // Rare recipes
        Recipes["rare_weapon"] = new SmeltingRecipe
        {
            Id = "rare_weapon",
            Name = "Rare Weapon Smelting",
            InputType = "Weapon",
            InputRarity = "Rare",
            OutputMaterials = new Dictionary<string, int>
            {
                ["mithril_scrap"] = 2,
                ["rare_essence"] = 3,
                ["gem_shard"] = 2,
                ["magic_crystal"] = 1
            },
            GoldCost = 50,
            SuccessRate = 0.85f,
            RequiredLevel = 15
        };
        
        Recipes["rare_armor"] = new SmeltingRecipe
        {
            Id = "rare_armor",
            Name = "Rare Armor Smelting",
            InputType = "Armor",
            InputRarity = "Rare",
            OutputMaterials = new Dictionary<string, int>
            {
                ["mithril_scrap"] = 3,
                ["rare_essence"] = 3,
                ["gem_shard"] = 2,
                ["magic_crystal"] = 1
            },
            GoldCost = 50,
            SuccessRate = 0.85f,
            RequiredLevel = 15
        };
        
        // Epic recipes
        Recipes["epic_weapon"] = new SmeltingRecipe
        {
            Id = "epic_weapon",
            Name = "Epic Weapon Smelting",
            InputType = "Weapon",
            InputRarity = "Epic",
            OutputMaterials = new Dictionary<string, int>
            {
                ["adamantite_scrap"] = 3,
                ["epic_essence"] = 4,
                ["gem_gem"] = 3,
                ["arcane_crystal"] = 2,
                ["dragon_scale"] = 1
            },
            GoldCost = 100,
            SuccessRate = 0.75f,
            RequiredLevel = 30
        };
        
        Recipes["epic_armor"] = new SmeltingRecipe
        {
            Id = "epic_armor",
            Name = "Epic Armor Smelting",
            InputType = "Armor",
            InputRarity = "Epic",
            OutputMaterials = new Dictionary<string, int>
            {
                ["adamantite_scrap"] = 4,
                ["epic_essence"] = 4,
                ["gem_gem"] = 3,
                ["arcane_crystal"] = 2
            },
            GoldCost = 100,
            SuccessRate = 0.75f,
            RequiredLevel = 30
        };
        
        // Legendary recipes
        Recipes["legendary_weapon"] = new SmeltingRecipe
        {
            Id = "legendary_weapon",
            Name = "Legendary Weapon Smelting",
            InputType = "Weapon",
            InputRarity = "Legendary",
            OutputMaterials = new Dictionary<string, int>
            {
                ["divine_scrap"] = 5,
                ["legendary_essence"] = 5,
                ["celestial_gem"] = 3,
                ["prismatic_crystal"] = 3,
                ["phoenix_feather"] = 2,
                ["void_shard"] = 1
            },
            GoldCost = 200,
            SuccessRate = 0.60f,
            RequiredLevel = 50
        };
        
        Recipes["legendary_armor"] = new SmeltingRecipe
        {
            Id = "legendary_armor",
            Name = "Legendary Armor Smelting",
            InputType = "Armor",
            InputRarity = "Legendary",
            OutputMaterials = new Dictionary<string, int>
            {
                ["divine_scrap"] = 5,
                ["legendary_essence"] = 5,
                ["celestial_gem"] = 3,
                ["prismatic_crystal"] = 3,
                ["phoenix_feather"] = 1
            },
            GoldCost = 200,
            SuccessRate = 0.60f,
            RequiredLevel = 50
        };
        
        // Universal recipes (any rarity)
        Recipes["universal_scrap"] = new SmeltingRecipe
        {
            Id = "universal_scrap",
            Name = "Universal Scrap Smelting",
            InputType = "Any",
            InputRarity = "Common",
            OutputMaterials = new Dictionary<string, int>
            {
                ["iron_scrap"] = 1
            },
            GoldCost = 5,
            SuccessRate = 0.98f,
            RequiredLevel = 1
        };
        
        Recipes["essence_extraction"] = new SmeltingRecipe
        {
            Id = "essence_extraction",
            Name = "Essence Extraction",
            InputType = "Any",
            InputRarity = "Uncommon",
            OutputMaterials = new Dictionary<string, int>
            {
                ["uncommon_essence"] = 1
            },
            GoldCost = 15,
            SuccessRate = 0.92f,
            RequiredLevel = 3
        };
    }
    
    void InitializeMaterials()
    {
        // Scrap materials
        Materials["iron_scrap"] = new MaterialInfo { Id = "iron_scrap", Name = "Iron Scrap", Category = "Scrap", BaseValue = 1 };
        Materials["steel_scrap"] = new MaterialInfo { Id = "steel_scrap", Name = "Steel Scrap", Category = "Scrap", BaseValue = 5 };
        Materials["mithril_scrap"] = new MaterialInfo { Id = "mithril_scrap", Name = "Mithril Scrap", Category = "Scrap", BaseValue = 15 };
        Materials["adamantite_scrap"] = new MaterialInfo { Id = "adamantite_scrap", Name = "Adamantite Scrap", Category = "Scrap", BaseValue = 50 };
        Materials["divine_scrap"] = new MaterialInfo { Id = "divine_scrap", Name = "Divine Scrap", Category = "Scrap", BaseValue = 200 };
        
        // Essence materials
        Materials["common_essence"] = new MaterialInfo { Id = "common_essence", Name = "Common Essence", Category = "Essence", BaseValue = 2 };
        Materials["uncommon_essence"] = new MaterialInfo { Id = "uncommon_essence", Name = "Uncommon Essence", Category = "Essence", BaseValue = 8 };
        Materials["rare_essence"] = new MaterialInfo { Id = "rare_essence", Name = "Rare Essence", Category = "Essence", BaseValue = 25 };
        Materials["epic_essence"] = new MaterialInfo { Id = "epic_essence", Name = "Epic Essence", Category = "Essence", BaseValue = 80 };
        Materials["legendary_essence"] = new MaterialInfo { Id = "legendary_essence", Name = "Legendary Essence", Category = "Essence", BaseValue = 300 };
        
        // Gem materials
        Materials["gem_fragment"] = new MaterialInfo { Id = "gem_fragment", Name = "Gem Fragment", Category = "Gem", BaseValue = 3 };
        Materials["gem_shard"] = new MaterialInfo { Id = "gem_shard", Name = "Gem Shard", Category = "Gem", BaseValue = 12 };
        Materials["gem_gem"] = new MaterialInfo { Id = "gem_gem", Name = "Polished Gem", Category = "Gem", BaseValue = 40 };
        Materials["celestial_gem"] = new MaterialInfo { Id = "celestial_gem", Name = "Celestial Gem", Category = "Gem", BaseValue = 150 };
        
        // Crystal materials
        Materials["magic_crystal"] = new MaterialInfo { Id = "magic_crystal", Name = "Magic Crystal", Category = "Crystal", BaseValue = 20 };
        Materials["arcane_crystal"] = new MaterialInfo { Id = "arcane_crystal", Name = "Arcane Crystal", Category = "Crystal", BaseValue = 60 };
        Materials["prismatic_crystal"] = new MaterialInfo { Id = "prismatic_crystal", Name = "Prismatic Crystal", Category = "Crystal", BaseValue = 250 };
        
        // Special materials
        Materials["dragon_scale"] = new MaterialInfo { Id = "dragon_scale", Name = "Dragon Scale", Category = "Special", BaseValue = 100 };
        Materials["phoenix_feather"] = new MaterialInfo { Id = "phoenix_feather", Name = "Phoenix Feather", Category = "Special", BaseValue = 200 };
        Materials["void_shard"] = new MaterialInfo { Id = "void_shard", Name = "Void Shard", Category = "Special", BaseValue = 500 };
    }
    
    void InitializeMultipliers()
    {
        EquipmentSmeltMultipliers["Weapon"] = 1.2f;
        EquipmentSmeltMultipliers["Armor"] = 1.5f;
        EquipmentSmeltMultipliers["Accessory"] = 1.0f;
        EquipmentSmeltMultipliers["Helmet"] = 1.0f;
        EquipmentSmeltMultipliers["Boots"] = 0.8f;
        EquipmentSmeltMultipliers["Gloves"] = 0.8f;
    }
    
    public SmeltingRecipe GetRecipe(string recipeId)
    {
        return Recipes.ContainsKey(recipeId) ? Recipes[recipeId] : null;
    }
    
    public List<SmeltingRecipe> GetAvailableRecipes(int playerLevel)
    {
        List<SmeltingRecipe> available = new List<SmeltingRecipe>();
        foreach (var recipe in Recipes.Values)
        {
            if (recipe.RequiredLevel <= playerLevel)
            {
                available.Add(recipe);
            }
        }
        return available;
    }
}

public class SmeltingRecipe
{
    public string Id;
    public string Name;
    public string InputType;
    public string InputRarity;
    public Dictionary<string, int> OutputMaterials = new Dictionary<string, int>();
    public int GoldCost;
    public float SuccessRate;
    public int RequiredLevel;
}

public class MaterialInfo
{
    public string Id;
    public string Name;
    public string Category;
    public int BaseValue;
}

/// <summary>
/// 导出保存数据
/// </summary>
public override Dictionary ExportSaveData()
{
    var data = new Dictionary();
    // ItemSmeltingDatabase 是静态配置数据，不需要持久化
    return data;
}

/// <summary>
/// 导入保存数据
/// </summary>
public override void ImportSaveData(Dictionary data)
{
    if (data == null) return;
    // ItemSmeltingDatabase 是静态配置数据，不需要持久化
}
