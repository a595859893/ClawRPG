using Godot;
using System;
using System.Collections.Generic;

public class ArtifactFusionData : BaseSystem
{
    // 融合记录
    public List<FusionRecord> FusionHistory { get; set; } = new List<FusionRecord>();
    
    // 已解锁的融合配方
    public List<string> UnlockedRecipes { get; set; } = new List<string>();
    
    // 统计数据
    public int TotalFusions { get; set; } = 0;
    public int SuccessfulFusions { get; set; } = 0;
    public int LegendaryFusions { get; set; } = 0;
    public int TotalGoldSpent { get; set; } = 0;
    
    // 融合槽位
    public string SelectedArtifact1 { get; set; } = "";
    public string SelectedArtifact2 { get; set; } = "";
    
    public ArtifactFusionData()
    {
    }
    
    public Dictionary<string, object> Save()
    {
        return new Dictionary<string, object>
        {
            { "fusion_history", FusionHistory.ConvertAll(r => r.Save()) },
            { "unlocked_recipes", UnlockedRecipes },
            { "total_fusions", TotalFusions },
            { "successful_fusions", SuccessfulFusions },
            { "legendary_fusions", LegendaryFusions },
            { "total_gold_spent", TotalGoldSpent },
            { "selected_artifact1", SelectedArtifact1 },
            { "selected_artifact2", SelectedArtifact2 }
        };
    }
    
    public void Load(Dictionary<string, object> data)
    {
        if (data.ContainsKey("fusion_history"))
        {
            var history = data["fusion_history"] as List<object>;
            FusionHistory = new List<FusionRecord>();
            foreach (var h in history)
            {
                var record = new FusionRecord();
                record.Load((Dictionary<string, object>)h);
                FusionHistory.Add(record);
            }
        }
        
        if (data.ContainsKey("unlocked_recipes"))
        {
            UnlockedRecipes = new List<string>((List<object>)data["unlocked_recipes"]);
        }
        
        TotalFusions = data.GetValueOrDefault("total_fusions", 0);
        SuccessfulFusions = data.GetValueOrDefault("successful_fusions", 0);
        LegendaryFusions = data.GetValueOrDefault("legendary_fusions", 0);
        TotalGoldSpent = data.GetValueOrDefault("total_gold_spent", 0);
        SelectedArtifact1 = data.GetValueOrDefault("selected_artifact1", "");
        SelectedArtifact2 = data.GetValueOrDefault("selected_artifact2", "");
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return new Dictionary
        {
            { "fusion_history", FusionHistory.ConvertAll(r => r.Save()) },
            { "unlocked_recipes", UnlockedRecipes },
            { "total_fusions", TotalFusions },
            { "successful_fusions", SuccessfulFusions },
            { "legendary_fusions", LegendaryFusions },
            { "total_gold_spent", TotalGoldSpent },
            { "selected_artifact1", SelectedArtifact1 },
            { "selected_artifact2", SelectedArtifact2 }
        };
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("fusion_history"))
        {
            var history = data["fusion_history"] as Godot.Array;
            FusionHistory = new List<FusionRecord>();
            foreach (Dictionary h in history)
            {
                var record = new FusionRecord();
                record.Load(new Dictionary<string, object>(h));
                FusionHistory.Add(record);
            }
        }
        
        if (data.Contains("unlocked_recipes"))
        {
            var recipes = data["unlocked_recipes"] as Godot.Array;
            UnlockedRecipes = new List<string>();
            foreach (string r in recipes)
            {
                UnlockedRecipes.Add(r);
            }
        }
        
        TotalFusions = data.GetValueOrDefault("total_fusions", 0);
        SuccessfulFusions = data.GetValueOrDefault("successful_fusions", 0);
        LegendaryFusions = data.GetValueOrDefault("legendary_fusions", 0);
        TotalGoldSpent = data.GetValueOrDefault("total_gold_spent", 0);
        SelectedArtifact1 = data.GetValueOrDefault("selected_artifact1", "");
        SelectedArtifact2 = data.GetValueOrDefault("selected_artifact2", "");
    }
}

public class FusionRecord
{
    public string Artifact1 { get; set; } = "";
    public string Artifact2 { get; set; } = "";
    public string ResultArtifact { get; set; } = "";
    public bool Success { get; set; } = false;
    public int GoldSpent { get; set; } = 0;
    public float Timestamp { get; set; } = 0;
    
    public Dictionary<string, object> Save()
    {
        return new Dictionary<string, object>
        {
            { "artifact1", Artifact1 },
            { "artifact2", Artifact2 },
            { "result_artifact", ResultArtifact },
            { "success", Success },
            { "gold_spent", GoldSpent },
            { "timestamp", Timestamp }
        };
    }
    
    public void Load(Dictionary<string, object> data)
    {
        Artifact1 = data.GetValueOrDefault("artifact1", "");
        Artifact2 = data.GetValueOrDefault("artifact2", "");
        ResultArtifact = data.GetValueOrDefault("result_artifact", "");
        Success = data.GetValueOrDefault("success", false);
        GoldSpent = data.GetValueOrDefault("gold_spent", 0);
        Timestamp = data.GetValueOrDefault("timestamp", 0f);
    }
}

public enum FusionResult
{
    Failure,
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum FusionType
{
    Weapon,
    Armor,
    Accessory,
    Ring,
    Amulet,
    Mixed
}

/// <summary>
/// 导出保存数据
/// </summary>
public override Dictionary ExportSaveData()
{
    var data = new Dictionary();
    
    // 融合历史
    var historyList = new Array();
    foreach (var record in FusionHistory)
    {
        historyList.Add(record.Save());
    }
    data["fusion_history"] = historyList;
    
    // 已解锁的配方
    data["unlocked_recipes"] = new Array(UnlockedRecipes);
    
    // 统计数据
    data["total_fusions"] = TotalFusions;
    data["successful_fusions"] = SuccessfulFusions;
    data["legendary_fusions"] = LegendaryFusions;
    data["total_gold_spent"] = TotalGoldSpent;
    
    // 融合槽位
    data["selected_artifact1"] = SelectedArtifact1;
    data["selected_artifact2"] = SelectedArtifact2;
    
    return data;
}

/// <summary>
/// 导入保存数据
/// </summary>
public override void ImportSaveData(Dictionary data)
{
    if (data == null) return;
    
    // 融合历史
    if (data.Contains("fusion_history"))
    {
        var historyArray = (Array)data["fusion_history"];
        FusionHistory = new List<FusionRecord>();
        foreach (Dictionary recordDict in historyArray)
        {
            var record = new FusionRecord();
            record.Load(recordDict);
            FusionHistory.Add(record);
        }
    }
    
    // 已解锁的配方
    if (data.Contains("unlocked_recipes"))
    {
        var recipesArray = (Array)data["unlocked_recipes"];
        UnlockedRecipes = new List<string>();
        foreach (string recipe in recipesArray)
        {
            UnlockedRecipes.Add(recipe);
        }
    }
    
    // 统计数据
    TotalFusions = (int)data.GetValueOrDefault("total_fusions", 0);
    SuccessfulFusions = (int)data.GetValueOrDefault("successful_fusions", 0);
    LegendaryFusions = (int)data.GetValueOrDefault("legendary_fusions", 0);
    TotalGoldSpent = (int)data.GetValueOrDefault("total_gold_spent", 0);
    
    // 融合槽位
    SelectedArtifact1 = (string)data.GetValueOrDefault("selected_artifact1", "");
    SelectedArtifact2 = (string)data.GetValueOrDefault("selected_artifact2", "");
}
