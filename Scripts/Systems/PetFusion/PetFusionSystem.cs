using Godot;
using System;
using System.Collections.Generic;

public partial class PetFusionSystem : BaseSystem
{
    private PetFusionData _data;
    private const string SAVE_PATH = "user://pet_fusion_save.json";
    
    protected override string SystemName => "PetFusionSystem";
    private PetFusionData _data;
    private const string SAVE_PATH = "user://pet_fusion_save.json";
    
    public PetFusionSystem() {
        _data = new PetFusionData();
        LoadData();
    }
    
    // 获取数据
    public PetFusionData GetData() {
        return _data;
    }
    
    // 执行宠物融合
    public PetFusionResult FusionPets(string pet1Type, string pet2Type, int pet1Level, int pet2Level, int playerGold) {
        string rarity = PetFusionDatabase.GetRandomRarity();
        int cost = PetFusionDatabase.CalculateFusionCost(rarity, pet1Level, pet2Level);
        
        // 检查金币是否足够
        if (playerGold < cost) {
            return PetFusionResult.Failure;
        }
        
        // 计算成功率
        float successRate = PetFusionDatabase.CalculateSuccessRate(rarity);
        bool success = GD.RandDouble() < successRate;
        
        // 创建融合记录
        PetFusionRecord record = new PetFusionRecord {
            Parent1Type = pet1Type,
            Parent2Type = pet2Type,
            ResultRarity = rarity,
            GoldCost = cost,
            WasSuccessful = success,
            ParentPet1Id = pet1Level,
            ParentPet2Id = pet2Level
        };
        
        if (success) {
            // 获取结果宠物类型
            string resultType = GetFusionResultType(pet1Type, pet2Type, rarity);
            record.ResultPetType = resultType;
            
            // 更新统计数据
            _data.TotalFusions++;
            _data.SuccessfulFusions++;
            _data.TotalGoldSpent += cost;
            
            if (rarity == "Legendary") {
                _data.LegendaryFusions++;
            }
            
            // 解锁融合类型
            string fusionTypeKey = pet1Type + "+" + pet2Type;
            if (!_data.UnlockedFusionTypes.Contains(fusionTypeKey)) {
                _data.UnlockedFusionTypes.Add(fusionTypeKey);
            }
            
            // 根据稀有度返回结果
            switch (rarity) {
                case "Legendary": return PetFusionResult.Legendary;
                case "Epic": return PetFusionResult.Epic;
                case "Rare": return PetFusionResult.Rare;
                case "Uncommon": return PetFusionResult.Uncommon;
                default: return PetFusionResult.Common;
            }
        } else {
            // 融合失败
            record.ResultPetType = "None";
            _data.TotalFusions++;
            _data.TotalGoldSpent += cost / 2; // 失败只花费一半
            
            return PetFusionResult.Failure;
        }
    }
    
    // 获取融合结果宠物类型
    private string GetFusionResultType(string pet1Type, string pet2Type, string rarity) {
        // 尝试从配方中获取
        var recipes = PetFusionDatabase.GetRecipesForPetType(pet1Type);
        foreach (var recipe in recipes) {
            if (recipe.RequiredTypes.Contains(pet2Type) && recipe.Rarity == rarity) {
                return recipe.ResultType;
            }
        }
        
        // 尝试反向查找
        recipes = PetFusionDatabase.GetRecipesForPetType(pet2Type);
        foreach (var recipe in recipes) {
            if (recipe.RequiredTypes.Contains(pet1Type) && recipe.Rarity == rarity) {
                return recipe.ResultType;
            }
        }
        
        // 根据元素组合生成默认名称
        string element = GetCombinedElement(pet1Type, pet2Type);
        switch (rarity) {
            case "Legendary": return element + " Primordial";
            case "Epic": return element + " Ancient";
            case "Rare": return element + " Elite";
            case "Uncommon": return element + " Hybrid";
            default: return element + " Pet";
        }
    }
    
    // 获取组合元素
    private string GetCombinedElement(string pet1Type, string pet2Type) {
        // 元素组合映射
        var elements = new List<string> { pet1Type, pet2Type };
        
        if (elements.Contains("Fire") && elements.Contains("Water")) return "Steam";
        if (elements.Contains("Fire") && elements.Contains("Ice")) return "Steam";
        if (elements.Contains("Fire") && elements.Contains("Lightning")) return "Plasma";
        if (elements.Contains("Water") && elements.Contains("Ice")) return "Frost";
        if (elements.Contains("Shadow") && elements.Contains("Holy")) return "Balance";
        if (elements.Contains("Fire") && elements.Contains("Shadow")) return "Infernal";
        
        // 默认返回第一个元素
        return pet1Type;
    }
    
    // 预览融合结果
    public Dictionary<string, object> PreviewFusion(string pet1Type, string pet2Type, int pet1Level, int pet2Level) {
        string rarity = PetFusionDatabase.GetRandomRarity();
        int cost = PetFusionDatabase.CalculateFusionCost(rarity, pet1Level, pet2Level);
        float successRate = PetFusionDatabase.CalculateSuccessRate(rarity);
        
        return new Dictionary<string, object> {
            { "estimatedRarity", rarity },
            { "estimatedCost", cost },
            { "successRate", successRate * 100 },
            { "resultType", GetFusionResultType(pet1Type, pet2Type, rarity) }
        };
    }
    
    // 获取统计信息
    public Dictionary<string, object> GetStatistics() {
        return new Dictionary<string, object> {
            { "totalFusions", _data.TotalFusions },
            { "successfulFusions", _data.SuccessfulFusions },
            { "legendaryFusions", _data.LegendaryFusions },
            { "totalGoldSpent", _data.TotalGoldSpent },
            { "successRate", _data.TotalFusions > 0 ? (_data.SuccessfulFusions * 100.0 / _data.TotalFusions) : 0 },
            { "unlockedTypes", _data.UnlockedFusionTypes.Count }
        };
    }
    
    // 获取融合历史
    public List<PetFusionRecord> GetFusionHistory() {
        var history = new List<PetFusionRecord>(_data.FusionHistory.Values);
        history.Sort((a, b) => b.Timestamp.CompareTo(a.Timestamp));
        return history;
    }
    
    // 保存数据
    public void SaveData() {
        var saveJson = new Godot.Collections.Dictionary();
        
        var fusionHistory = new Godot.Collections.Array();
        foreach (var record in _data.FusionHistory.Values) {
            var recordDict = new Godot.Collections.Dictionary();
            recordDict["id"] = record.Id;
            recordDict["parent1_type"] = record.Parent1Type;
            recordDict["parent2_type"] = record.Parent2Type;
            recordDict["result_type"] = record.ResultPetType;
            recordDict["result_rarity"] = record.ResultRarity;
            recordDict["gold_cost"] = record.GoldCost;
            recordDict["was_successful"] = record.WasSuccessful;
            recordDict["timestamp"] = record.Timestamp;
            fusionHistory.Add(recordDict);
        }
        
        saveJson["fusion_history"] = fusionHistory;
        saveJson["total_fusions"] = _data.TotalFusions;
        saveJson["successful_fusions"] = _data.SuccessfulFusions;
        saveJson["legendary_fusions"] = _data.LegendaryFusions;
        saveJson["total_gold_spent"] = _data.TotalGoldSpent;
        saveJson["unlocked_fusion_types"] = new Godot.Collections.Array(_data.UnlockedFusionTypes);
        
        using (var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write)) {
            file.StoreString(JSON.Stringify(saveJson));
        }
    }
    
    // 加载数据
    public void LoadData() {
        if (!FileAccess.FileExists(SAVE_PATH)) {
            return;
        }
        
        using (var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Read)) {
            var jsonString = file.GetAsText();
            var json = JSON.ParseString(jsonString);
            
            if (json.Error != Error.Ok) {
                return;
            }
            
            var saveJson = (Godot.Collections.Dictionary)json.Result;
            
            // 加载融合历史
            if (saveJson.Contains("fusion_history")) {
                var fusionHistory = (Godot.Collections.Array)saveJson["fusion_history"];
                foreach (Godot.Collections.Dictionary recordDict in fusionHistory) {
                    var record = new PetFusionRecord {
                        Id = (int)recordDict["id"],
                        Parent1Type = (string)recordDict["parent1_type"],
                        Parent2Type = (string)recordDict["parent2_type"],
                        ResultPetType = (string)recordDict["result_type"],
                        ResultRarity = (string)recordDict["result_rarity"],
                        GoldCost = (int)recordDict["gold_cost"],
                        WasSuccessful = (bool)recordDict["was_successful"],
                        Timestamp = (long)recordDict["timestamp"]
                    };
                    _data.FusionHistory[record.Id] = record;
                }
            }
            
            // 加载统计数据
            if (saveJson.Contains("total_fusions")) {
                _data.TotalFusions = (int)saveJson["total_fusions"];
            }
            if (saveJson.Contains("successful_fusions")) {
                _data.SuccessfulFusions = (int)saveJson["successful_fusions"];
            }
            if (saveJson.Contains("legendary_fusions")) {
                _data.LegendaryFusions = (int)saveJson["legendary_fusions"];
            }
            if (saveJson.Contains("total_gold_spent")) {
                _data.TotalGoldSpent = (int)saveJson["total_gold_spent"];
            }
            if (saveJson.Contains("unlocked_fusion_types")) {
                var unlockedTypes = (Godot.Collections.Array)saveJson["unlocked_fusion_types"];
                foreach (string type in unlockedTypes) {
                    _data.UnlockedFusionTypes.Add(type);
                }
            }
        }
    }
    
    /// <summary>
    /// Export save data (BaseSystem override)
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        data["totalFusions"] = _data.TotalFusions;
        data["successfulFusions"] = _data.SuccessfulFusions;
        data["legendaryFusions"] = _data.LegendaryFusions;
        data["totalGoldSpent"] = _data.TotalGoldSpent;
        data["unlockedFusionTypes"] = _data.UnlockedFusionTypes.ToList();
        data["fusionHistory"] = _data.FusionHistory.Values.ToList();
        return data;
    }
    
    /// <summary>
    /// Import save data (BaseSystem override)
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("totalFusions"))
        {
            _data.TotalFusions = (int)data["totalFusions"];
        }
        if (data.Contains("successfulFusions"))
        {
            _data.SuccessfulFusions = (int)data["successfulFusions"];
        }
        if (data.Contains("legendaryFusions"))
        {
            _data.LegendaryFusions = (int)data["legendaryFusions"];
        }
        if (data.Contains("totalGoldSpent"))
        {
            _data.TotalGoldSpent = (int)data["totalGoldSpent"];
        }
        if (data.Contains("unlockedFusionTypes"))
        {
            _data.UnlockedFusionTypes = new HashSet<string>((List<string>)data["unlockedFusionTypes"]);
        }
    }
}
