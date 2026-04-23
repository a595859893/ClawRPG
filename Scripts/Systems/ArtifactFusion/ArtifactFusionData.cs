using Godot;
using System;
using System.Collections.Generic;

public class ArtifactFusionData
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
                record.Load((Godot.Collections.Dictionary)h);
                FusionHistory.Add(record);
            }
        }
        
        if (data.ContainsKey("unlocked_recipes"))
        {
            var recipesArr = (Godot.Collections.Array)data["unlocked_recipes"];
            UnlockedRecipes = new List<string>();
            foreach (var r in recipesArr) UnlockedRecipes.Add((string)r);
        }
        
        TotalFusions = Convert.ToInt32(data.GetValueOrDefault("total_fusions", 0) ?? 0);
        SuccessfulFusions = Convert.ToInt32(data.GetValueOrDefault("successful_fusions", 0) ?? 0);
        LegendaryFusions = Convert.ToInt32(data.GetValueOrDefault("legendary_fusions", 0) ?? 0);
        TotalGoldSpent = Convert.ToInt32(data.GetValueOrDefault("total_gold_spent", 0) ?? 0);
        SelectedArtifact1 = (string)(data.GetValueOrDefault("selected_artifact1", "") ?? "");
        SelectedArtifact2 = (string)(data.GetValueOrDefault("selected_artifact2", "") ?? "");
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public Dictionary ExportSaveData()
    {
        var result = new Dictionary();
        var historyArray = new Godot.Collections.Array();
        foreach (var r in FusionHistory)
            historyArray.Add(r.Save());
        result.Add("fusion_history", historyArray);
        var recipesArray = new Godot.Collections.Array();
        foreach (var r in UnlockedRecipes)
            recipesArray.Add(r);
        result.Add("unlocked_recipes", recipesArray);
        result.Add("total_fusions", TotalFusions);
        result.Add("successful_fusions", SuccessfulFusions);
        result.Add("legendary_fusions", LegendaryFusions);
        result.Add("total_gold_spent", TotalGoldSpent);
        result.Add("selected_artifact1", SelectedArtifact1);
        result.Add("selected_artifact2", SelectedArtifact2);
        return result;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("fusion_history"))
        {
            var history = (Godot.Collections.Array)data["fusion_history"];
            FusionHistory = new List<FusionRecord>();
            foreach (var h in history)
            {
                var record = new FusionRecord();
                record.Load((Godot.Collections.Dictionary)h);
                FusionHistory.Add(record);
            }
        }
        
        if (data.ContainsKey("unlocked_recipes"))
        {
            var recipes = (Godot.Collections.Array)data["unlocked_recipes"];
            UnlockedRecipes = new List<string>();
            foreach (var r in recipes)
                UnlockedRecipes.Add((string)r);
        }
        
        TotalFusions = data.ContainsKey("total_fusions") ? Convert.ToInt32(data["total_fusions"]) : 0;
        SuccessfulFusions = data.ContainsKey("successful_fusions") ? Convert.ToInt32(data["successful_fusions"]) : 0;
        LegendaryFusions = data.ContainsKey("legendary_fusions") ? Convert.ToInt32(data["legendary_fusions"]) : 0;
        TotalGoldSpent = data.ContainsKey("total_gold_spent") ? Convert.ToInt32(data["total_gold_spent"]) : 0;
        SelectedArtifact1 = data.ContainsKey("selected_artifact1") ? (string)data["selected_artifact1"] : "";
        SelectedArtifact2 = data.ContainsKey("selected_artifact2") ? (string)data["selected_artifact2"] : "";
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
    
    public Godot.Collections.Dictionary Save()
    {
        return new Godot.Collections.Dictionary
        {
            { "artifact1", Artifact1 },
            { "artifact2", Artifact2 },
            { "result_artifact", ResultArtifact },
            { "success", Success },
            { "gold_spent", GoldSpent },
            { "timestamp", Timestamp }
        };
    }
    
    public void Load(Godot.Collections.Dictionary data)
    {
        if (data == null) return;
        Artifact1 = data.ContainsKey("artifact1") ? (string)data["artifact1"] : "";
        Artifact2 = data.ContainsKey("artifact2") ? (string)data["artifact2"] : "";
        ResultArtifact = data.ContainsKey("result_artifact") ? (string)data["result_artifact"] : "";
        Success = data.ContainsKey("success") && (bool)data["success"];
        GoldSpent = data.ContainsKey("gold_spent") ? Convert.ToInt32(data["gold_spent"]) : 0;
        Timestamp = data.ContainsKey("timestamp") ? Convert.ToSingle(data["timestamp"]) : 0f;
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
