using Godot;
using System;
using System.Collections.Generic;

public partial class ArtifactFusionSystem : BaseSystem
{
    public static ArtifactFusionSystem Instance { get; private set; }
    
    public ArtifactFusionData Data { get; private set; }
    public bool IsInitialized { get; private set; } = false;
    
    // 信号
    public delegate void FusionCompletedEventHandler(FusionRecord record);
    public event FusionCompletedEventHandler OnFusionCompleted;
    
    public delegate void RecipeUnlockedEventHandler(string recipeId);
    public event RecipeUnlockedEventHandler OnRecipeUnlocked;
    
    public override void _Ready()
    {
        Instance = this;
        Initialize();
    }
    
    private void Initialize()
    {
        Data = new ArtifactFusionData();
        LoadData();
        IsInitialized = true;
        GD.Print("[ArtifactFusionSystem] Initialized");
    }
    
    public FusionRecord PerformFusion(string recipeId)
    {
        var recipe = ArtifactFusionDatabase.GetRecipe(recipeId);
        if (recipe == null)
        {
            GD.PrintErr($"[ArtifactFusionSystem] Recipe not found: {recipeId}");
            return null;
        }
        
        // 检查玩家金币是否足够
        int playerGold = GetPlayerGold();
        if (playerGold < recipe.GoldCost)
        {
            GD.PrintErr($"[ArtifactFusionSystem] Not enough gold. Required: {recipe.GoldCost}, Has: {playerGold}");
            return null;
        }
        
        // 扣除金币
        DeductGold(recipe.GoldCost);
        
        // 创建融合记录
        FusionRecord record = new FusionRecord
        {
            Artifact1 = recipe.Artifact1,
            Artifact2 = recipe.Artifact2,
            GoldSpent = recipe.GoldCost,
            Timestamp = OS.GetUnixTime()
        };
        
        // 计算融合结果
        var random = new Random();
        float roll = (float)random.NextDouble();
        
        bool success = roll < recipe.SuccessRate;
        record.Success = success;
        
        if (success)
        {
            if (recipe.IsRandomResult)
            {
                record.ResultArtifact = ArtifactFusionDatabase.GetRandomArtifactByRarity(recipe.ResultRarity);
            }
            else
            {
                record.ResultArtifact = recipe.ResultArtifact;
            }
            
            // 解锁配方
            if (!Data.UnlockedRecipes.Contains(recipeId))
            {
                Data.UnlockedRecipes.Add(recipeId);
                OnRecipeUnlocked?.Invoke(recipeId);
            }
            
            Data.SuccessfulFusions++;
            if (recipe.ResultRarity == "Legendary")
            {
                Data.LegendaryFusions++;
            }
            
            GD.Print($"[ArtifactFusionSystem] Fusion success! Result: {record.ResultArtifact}");
        }
        else
        {
            record.ResultArtifact = "Fusion Failed";
            GD.Print($"[ArtifactFusionSystem] Fusion failed!");
        }
        
        // 更新统计
        Data.TotalFusions++;
        Data.TotalGoldSpent += recipe.GoldCost;
        
        // 添加到历史记录
        Data.FusionHistory.Insert(0, record);
        if (Data.FusionHistory.Count > 100)
        {
            Data.FusionHistory.RemoveAt(Data.FusionHistory.Count - 1);
        }
        
        // 保存数据
        SaveData();
        
        // 触发事件
        OnFusionCompleted?.Invoke(record);
        
        return record;
    }
    
    public List<ArtifactFusionDatabase.FusionRecipe> GetAvailableRecipes()
    {
        List<ArtifactFusionDatabase.FusionRecipe> available = new List<ArtifactFusionDatabase.FusionRecipe>();
        foreach (var recipe in ArtifactFusionDatabase.Recipes)
        {
            if (recipe.RequiredLevel <= GetPlayerLevel())
            {
                available.Add(recipe);
            }
        }
        return available;
    }
    
    public List<ArtifactFusionDatabase.FusionRecipe> GetUnlockedRecipes()
    {
        List<ArtifactFusionDatabase.FusionRecipe> unlocked = new List<ArtifactFusionDatabase.FusionRecipe>();
        foreach (var recipeId in Data.UnlockedRecipes)
        {
            var recipe = ArtifactFusionDatabase.GetRecipe(recipeId);
            if (recipe != null)
            {
                unlocked.Add(recipe);
            }
        }
        return unlocked;
    }
    
    public List<FusionRecord> GetFusionHistory(int count = 20)
    {
        int limit = Mathf.Min(count, Data.FusionHistory.Count);
        return Data.FusionHistory.GetRange(0, limit);
    }
    
    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            { "total_fusions", Data.TotalFusions },
            { "successful_fusions", Data.SuccessfulFusions },
            { "legendary_fusions", Data.LegendaryFusions },
            { "total_gold_spent", Data.TotalGoldSpent },
            { "recipes_unlocked", Data.UnlockedRecipes.Count }
        };
    }
    
    public float GetSuccessRate()
    {
        if (Data.TotalFusions == 0) return 0f;
        return (float)Data.SuccessfulFusions / Data.TotalFusions;
    }
    
    // 模拟玩家数据获取 (需要根据实际游戏数据调整)
    private int GetPlayerGold()
    {
        // 这里应该从玩家数据获取金币
        // 暂时返回默认值
        if (HasNode("/root/Main/Player"))
        {
            var player = GetNode("/root/Main/Player");
            var goldProperty = player.Get("gold");
            if (goldProperty != null)
                return (int)goldProperty;
        }
        return 10000; // 默认金币
    }
    
    private int GetPlayerLevel()
    {
        // 这里应该从玩家数据获取等级
        if (HasNode("/root/Main/Player"))
        {
            var player = GetNode("/root/Main/Player");
            var levelProperty = player.Get("level");
            if (levelProperty != null)
                return (int)levelProperty;
        }
        return 50; // 默认等级
    }
    
    private void DeductGold(int amount)
    {
        // 这里应该扣除玩家金币
        if (HasNode("/root/Main/Player"))
        {
            var player = GetNode("/root/Main/Player");
            var currentGold = (int)player.Get("gold");
            player.Set("gold", currentGold - amount);
        }
    }
    
    public void SaveData()
    {
        // 保存到文件
        var saveSystem = GetTree().Root.GetNode("Main/SaveSystem") as SaveSystem;
        if (saveSystem != null)
        {
            var key = "artifact_fusion_data";
            saveSystem.SaveGameData(key, Data.Save());
        }
    }
    
    public void LoadData()
    {
        var saveSystem = GetTree().Root.GetNode("Main/SaveSystem") as SaveSystem;
        if (saveSystem != null)
        {
            var key = "artifact_fusion_data";
            var data = saveSystem.LoadGameData(key);
            if (data != null)
            {
                Data.Load(data);
                GD.Print($"[ArtifactFusionSystem] Loaded {Data.TotalFusions} fusion records");
            }
        }
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        data["total_fusions"] = Data.TotalFusions;
        data["successful_fusions"] = Data.SuccessfulFusions;
        data["legendary_fusions"] = Data.LegendaryFusions;
        data["total_gold_spent"] = Data.TotalGoldSpent;
        data["unlocked_recipes"] = new Godot.Array(Data.UnlockedRecipes);
        
        // Serialize fusion history
        var historyArray = new Godot.Array();
        foreach (var record in Data.FusionHistory)
        {
            var recordDict = new Dictionary<string, object>();
            recordDict["artifact1"] = record.Artifact1;
            recordDict["artifact2"] = record.Artifact2;
            recordDict["result_artifact"] = record.ResultArtifact;
            recordDict["success"] = record.Success;
            recordDict["gold_spent"] = record.GoldSpent;
            recordDict["timestamp"] = record.Timestamp;
            historyArray.Add(recordDict);
        }
        data["fusion_history"] = historyArray;
        
        return data;
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.Contains("total_fusions")) Data.TotalFusions = (int)data["total_fusions"];
        if (data.Contains("successful_fusions")) Data.SuccessfulFusions = (int)data["successful_fusions"];
        if (data.Contains("legendary_fusions")) Data.LegendaryFusions = (int)data["legendary_fusions"];
        if (data.Contains("total_gold_spent")) Data.TotalGoldSpent = (int)data["total_gold_spent"];
        
        if (data.Contains("unlocked_recipes"))
        {
            Data.UnlockedRecipes.Clear();
            var recipes = (Godot.Array)data["unlocked_recipes"];
            foreach (string recipe in recipes)
            {
                Data.UnlockedRecipes.Add(recipe);
            }
        }
    }
    
    public void ResetStatistics()
    {
        Data.TotalFusions = 0;
        Data.SuccessfulFusions = 0;
        Data.LegendaryFusions = 0;
        Data.TotalGoldSpent = 0;
        Data.FusionHistory.Clear();
        SaveData();
        GD.Print("[ArtifactFusionSystem] Statistics reset");
    }
}
