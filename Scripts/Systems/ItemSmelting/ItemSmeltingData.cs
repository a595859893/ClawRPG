using Godot;
using System;
using System.Collections.Generic;

public partial class ItemSmeltingData : BaseSystem
{
    // Smelting recipes unlocked
    public HashSet<string> UnlockedRecipes = new HashSet<string>();
    
    // Smelting history
    public List<SmeltingRecord> History = new List<SmeltingRecord>();
    
    // Statistics
    public int TotalSmelts = 0;
    public int TotalItemsSmelted = 0;
    public int TotalMaterialsGenerated = 0;
    public int GoldSpent = 0;
    
    // Per recipe stats
    public Dictionary<string, int> RecipeUsageCount = new Dictionary<string, int>();
    
    public override void _Ready()
    {
        // Load data from save if exists
        LoadData();
    }
    
    public void LoadData()
    {
        if (FileAccess.FileExists("user://item_smelting_data.json"))
        {
            var file = FileAccess.Open("user://item_smelting_data.json", FileAccess.ModeFlags.Read);
            string json = file.GetAsText();
            file.Close();
            
            // Parse JSON (simplified)
            if (json.Contains("\"TotalSmelts\""))
            {
                // Load stats
                var data = Json.ParseString(json).AsGodotDictionary();
                if (data.Contains("TotalSmelts")) TotalSmelts = (int)data["TotalSmelts"];
                if (data.Contains("TotalItemsSmelted")) TotalItemsSmelted = (int)data["TotalItemsSmelted"];
                if (data.Contains("TotalMaterialsGenerated")) TotalMaterialsGenerated = (int)data["TotalMaterialsGenerated"];
                if (data.Contains("GoldSpent")) GoldSpent = (int)data["GoldSpent"];
            }
        }
    }
    
    public void SaveData()
    {
        var file = FileAccess.Open("user://item_smelting_data.json", FileAccess.ModeFlags.Write);
        string json = Json.Stringify(new Godot.Collections.Dictionary
        {
            ["TotalSmelts"] = TotalSmelts,
            ["TotalItemsSmelted"] = TotalItemsSmelted,
            ["TotalMaterialsGenerated"] = TotalMaterialsGenerated,
            ["GoldSpent"] = GoldSpent
        });
        file.StoreString(json);
        file.Close();
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary
        {
            { "total_smelts", TotalSmelts },
            { "total_items_smelted", TotalItemsSmelted },
            { "total_materials_generated", TotalMaterialsGenerated },
            { "gold_spent", GoldSpent },
            { "unlocked_recipes", new Godot.Array(UnlockedRecipes) },
            { "recipe_usage_count", new Dictionary(RecipeUsageCount) }
        };
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        TotalSmelts = data.GetValueOrDefault("total_smelts", 0);
        TotalItemsSmelted = data.GetValueOrDefault("total_items_smelted", 0);
        TotalMaterialsGenerated = data.GetValueOrDefault("total_materials_generated", 0);
        GoldSpent = data.GetValueOrDefault("gold_spent", 0);
        
        if (data.Contains("unlocked_recipes"))
        {
            var recipesArray = data["unlocked_recipes"] as Godot.Array;
            UnlockedRecipes = new HashSet<string>();
            foreach (string recipe in recipesArray)
            {
                UnlockedRecipes.Add(recipe);
            }
        }
        
        if (data.Contains("recipe_usage_count"))
        {
            var usageDict = data["recipe_usage_count"] as Dictionary;
            RecipeUsageCount = new Dictionary<string, int>();
            foreach (var kvp in usageDict)
            {
                RecipeUsageCount[kvp.Key] = (int)kvp.Value;
            }
        }
    }
}

public class SmeltingRecord
{
    public string RecipeId;
    public int ItemCount;
    public int MaterialsGenerated;
    public int GoldSpent;
    public long Timestamp;
}
