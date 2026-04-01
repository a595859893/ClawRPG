using Godot;
using System;
using System.Collections.Generic;

public partial class ItemSmeltingSystem : BaseSystem
{
    private ItemSmeltingData _data;
    private ItemSmeltingDatabase _database;
    
    // Current smelting state
    public string CurrentRecipeId = "";
    public int CurrentItemCount = 1;
    public bool IsSmelting = false;
    public float SmeltProgress = 0f;
    
    // Smelting duration (seconds)
    private const float SMELT_DURATION = 2.0f;
    
    public override void _Ready()
    {
        _data = GetNode<ItemSmeltingData>("/root/ItemSmeltingData");
        _database = GetNode<ItemSmeltingDatabase>("/root/ItemSmeltingDatabase");
        
        if (_data == null)
        {
            GD.PrintErr("ItemSmeltingData not found!");
            return;
        }
        
        if (_database == null)
        {
            GD.PrintErr("ItemSmeltingDatabase not found!");
            return;
        }
    }
    
    public void StartSmelting(string recipeId, int itemCount = 1)
    {
        if (IsSmelting) return;
        
        var recipe = _database.GetRecipe(recipeId);
        if (recipe == null)
        {
            GD.PrintErr("Recipe not found: " + recipeId);
            return;
        }
        
        // Check player gold (simplified - assume player has enough)
        int totalCost = recipe.GoldCost * itemCount;
        
        CurrentRecipeId = recipeId;
        CurrentItemCount = itemCount;
        IsSmelting = true;
        SmeltProgress = 0f;
        
        GD.Print($"Started smelting: {recipe.Name} x{itemCount}");
    }
    
    public override void _Process(double delta)
    {
        if (!IsSmelting) return;
        
        SmeltProgress += delta / SMELT_DURATION;
        
        if (SmeltProgress >= 1.0f)
        {
            CompleteSmelting();
        }
    }
    
    void CompleteSmelting()
    {
        var recipe = _database.GetRecipe(CurrentRecipeId);
        if (recipe == null)
        {
            IsSmelting = false;
            SmeltProgress = 0f;
            return;
        }
        
        // Calculate success
        bool success = GD.Randf() < recipe.SuccessRate;
        
        if (success)
        {
            // Generate materials
            Dictionary<string, int> generatedMaterials = new Dictionary<string, int>();
            
            foreach (var material in recipe.OutputMaterials)
            {
                int amount = material.Value * CurrentItemCount;
                
                // Apply equipment type multiplier
                float multiplier = _database.EquipmentSmeltMultipliers.ContainsKey(recipe.InputType) 
                    ? _database.EquipmentSmeltMultipliers[recipe.InputType] 
                    : 1.0f;
                
                amount = (int)(amount * multiplier);
                generatedMaterials[material.Key] = amount;
                
                _data.TotalMaterialsGenerated += amount;
            }
            
            // Update stats
            _data.TotalSmelts++;
            _data.TotalItemsSmelted += CurrentItemCount;
            _data.GoldSpent += recipe.GoldCost * CurrentItemCount;
            
            // Update recipe usage
            if (!_data.RecipeUsageCount.ContainsKey(CurrentRecipeId))
            {
                _data.RecipeUsageCount[CurrentRecipeId] = 0;
            }
            _data.RecipeUsageCount[CurrentRecipeId]++;
            
            // Record history
            SmeltingRecord record = new SmeltingRecord
            {
                RecipeId = CurrentRecipeId,
                ItemCount = CurrentItemCount,
                MaterialsGenerated = GetTotalMaterials(generatedMaterials),
                GoldSpent = recipe.GoldCost * CurrentItemCount,
                Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
            };
            _data.History.Insert(0, record);
            
            // Keep history limited
            if (_data.History.Count > 100)
            {
                _data.History.RemoveAt(_data.History.Count - 1);
            }
            
            // Save data
            _data.SaveData();
            
            GD.Print($"Smelting complete! Generated {GetTotalMaterials(generatedMaterials)} materials");
        }
        else
        {
            // Failed - lose items but not all materials
            _data.TotalSmelts++;
            _data.TotalItemsSmelted += CurrentItemCount;
            _data.GoldSpent += recipe.GoldCost * CurrentItemCount;
            
            // Record failed smelt
            SmeltingRecord record = new SmeltingRecord
            {
                RecipeId = CurrentRecipeId,
                ItemCount = CurrentItemCount,
                MaterialsGenerated = 0,
                GoldSpent = recipe.GoldCost * CurrentItemCount,
                Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
            };
            _data.History.Insert(0, record);
            
            _data.SaveData();
            
            GD.Print($"Smelting failed! Lost {CurrentItemCount} items and {recipe.GoldCost * CurrentItemCount} gold");
        }
        
        IsSmelting = false;
        SmeltProgress = 0f;
        CurrentRecipeId = "";
        CurrentItemCount = 1;
    }
    
    int GetTotalMaterials(Dictionary<string, int> materials)
    {
        int total = 0;
        foreach (var m in materials)
        {
            total += m.Value;
        }
        return total;
    }
    
    public void CancelSmelting()
    {
        if (IsSmelting)
        {
            IsSmelting = false;
            SmeltProgress = 0f;
            CurrentRecipeId = "";
            CurrentItemCount = 1;
            GD.Print("Smelting cancelled");
        }
    }
    
    public Dictionary<string, int> PreviewSmelting(string recipeId, int itemCount)
    {
        var recipe = _database.GetRecipe(recipeId);
        if (recipe == null) return new Dictionary<string, int>();
        
        Dictionary<string, int> preview = new Dictionary<string, int>();
        
        foreach (var material in recipe.OutputMaterials)
        {
            int amount = material.Value * itemCount;
            
            float multiplier = _database.EquipmentSmeltMultipliers.ContainsKey(recipe.InputType) 
                ? _database.EquipmentSmeltMultipliers[recipe.InputType] 
                : 1.0f;
            
            amount = (int)(amount * multiplier);
            preview[material.Key] = amount;
        }
        
        return preview;
    }
    
    public SmeltingStatistics GetStatistics()
    {
        return new SmeltingStatistics
        {
            TotalSmelts = _data.TotalSmelts,
            TotalItemsSmelted = _data.TotalItemsSmelted,
            TotalMaterialsGenerated = _data.TotalMaterialsGenerated,
            GoldSpent = _data.GoldSpent,
            AverageMaterialsPerSmelt = _data.TotalSmelts > 0 
                ? (float)_data.TotalMaterialsGenerated / _data.TotalSmelts 
                : 0f,
            SuccessRate = _data.TotalSmelts > 0 
                ? (_data.TotalItemsSmelted * 100f / _data.TotalSmelts) / _data.TotalSmelts 
                : 0f
        };
    }
    
    public List<SmeltingRecord> GetHistory(int limit = 20)
    {
        List<SmeltingRecord> result = new List<SmeltingRecord>();
        for (int i = 0; i < Mathf.Min(limit, _data.History.Count); i++)
        {
            result.Add(_data.History[i]);
        }
        return result;
    }

    #region Data Types

    public class SmeltingStatistics
    {
        public int TotalSmelts;
        public int TotalItemsSmelted;
        public int TotalMaterialsGenerated;
        public int GoldSpent;
        public float AverageMaterialsPerSmelt;
        public float SuccessRate;
    }

    #endregion

    #region Persistence

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();

        // 当前锻造状态
        data["current_recipe_id"] = CurrentRecipeId;
        data["current_item_count"] = CurrentItemCount;
        data["is_smelting"] = IsSmelting;
        data["smelt_progress"] = SmeltProgress;

        // 委托给数据层
        if (_data != null)
        {
            var dataData = _data.ExportSaveData();
            foreach (var kvp in dataData)
            {
                data[kvp.Key] = kvp.Value;
            }
        }

        return data;
    }

    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        // 当前锻造状态
        CurrentRecipeId = (string)data.GetValueOrDefault("current_recipe_id", "");
        CurrentItemCount = (int)data.GetValueOrDefault("current_item_count", 1);
        IsSmelting = (bool)data.GetValueOrDefault("is_smelting", false);
        SmeltProgress = (float)data.GetValueOrDefault("smelt_progress", 0f);

        // 委托给数据层
        if (_data != null)
        {
            _data.ImportSaveData(data);
        }
    }

    #endregion
}
