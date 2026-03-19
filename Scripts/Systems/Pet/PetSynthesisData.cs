using Godot;
using System;
using System.Collections.Generic;

public class PetSynthesisData : BaseSystem
{
    // Synthesis records
    public Dictionary<int, List<PetSynthesisRecord>> SynthesisHistory = new Dictionary<int, List<PetSynthesisRecord>>();
    
    // Unlocked synthesis recipes
    public HashSet<string> UnlockedRecipes = new HashSet<string>();
    
    // Statistics
    public int TotalSyntheses = 0;
    public int SuccessfulSyntheses = 0;
    public int LegendarySyntheses = 0;
    public int TotalGoldSpent = 0;
    
    // Active synthesis
    public bool IsSynthesizing = false;
    public int SynthesisPet1Id = -1;
    public int SynthesisPet2Id = -1;
    public float SynthesisProgress = 0.0f;
    
    public override void _Ready()
    {
        // Initialize synthesis history for each pet
    }
    
    public Dictionary<string, object> Save()
    {
        var data = new Dictionary<string, object>();
        
        // Save synthesis history
        var historyList = new List<Dictionary<string, object>>();
        foreach (var kvp in SynthesisHistory)
        {
            foreach (var record in kvp.Value)
            {
                historyList.Add(record.Save());
            }
        }
        data["synthesis_history"] = historyList;
        
        // Save unlocked recipes
        var recipeList = new List<string>(UnlockedRecipes);
        data["unlocked_recipes"] = recipeList;
        
        // Save statistics
        data["total_syntheses"] = TotalSyntheses;
        data["successful_syntheses"] = SuccessfulSyntheses;
        data["legendary_syntheses"] = LegendarySyntheses;
        data["total_gold_spent"] = TotalGoldSpent;
        
        return data;
    }
    
    public void Load(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        // Load synthesis history
        if (data.ContainsKey("synthesis_history"))
        {
            var historyList = (List<Dictionary<string, object>>)data["synthesis_history"];
            foreach (var recordData in historyList)
            {
                var record = new PetSynthesisRecord();
                record.Load(recordData);
                int petId = record.Pet1Id;
                if (!SynthesisHistory.ContainsKey(petId))
                {
                    SynthesisHistory[petId] = new List<PetSynthesisRecord>();
                }
                SynthesisHistory[petId].Add(record);
            }
        }
        
        // Load unlocked recipes
        if (data.ContainsKey("unlocked_recipes"))
        {
            var recipeList = (List<string>)data["unlocked_recipes"];
            foreach (var recipe in recipeList)
            {
                UnlockedRecipes.Add(recipe);
            }
        }
        
        // Load statistics
        if (data.ContainsKey("total_syntheses"))
            TotalSyntheses = (int)data["total_syntheses"];
        if (data.ContainsKey("successful_syntheses"))
            SuccessfulSyntheses = (int)data["successful_syntheses"];
        if (data.ContainsKey("legendary_syntheses"))
            LegendarySyntheses = (int)data["legendary_syntheses"];
        if (data.ContainsKey("total_gold_spent"))
            TotalGoldSpent = (int)data["total_gold_spent"];
    }
}

public class PetSynthesisRecord
{
    public int Pet1Id { get; set; }
    public int Pet2Id { get; set; }
    public int ResultPetId { get; set; }
    public string ResultPetType { get; set; }
    public string ResultRarity { get; set; }
    public bool WasSuccessful { get; set; }
    public int GoldCost { get; set; }
    public long Timestamp { get; set; }
    
    public Dictionary<string, object> Save()
    {
        return new Dictionary<string, object>
        {
            ["pet1_id"] = Pet1Id,
            ["pet2_id"] = Pet2Id,
            ["result_pet_id"] = ResultPetId,
            ["result_pet_type"] = ResultPetType,
            ["result_rarity"] = ResultRarity,
            ["was_successful"] = WasSuccessful,
            ["gold_cost"] = GoldCost,
            ["timestamp"] = Timestamp
        };
    }
    
    public void Load(Dictionary<string, object> data)
    {
        Pet1Id = (int)data["pet1_id"];
        Pet2Id = (int)data["pet2_id"];
        ResultPetId = (int)data["result_pet_id"];
        ResultPetType = (string)data["result_pet_type"];
        ResultRarity = (string)data["result_rarity"];
        WasSuccessful = (bool)data["was_successful"];
        GoldCost = (int)data["gold_cost"];
        Timestamp = (long)data["timestamp"];
    }
}

public enum SynthesisResult
{
    Failure,
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum SynthesisType
{
    ElementalFusion,
    BeastFusion,
    MythicalFusion,
    ShadowFusion,
    CelestialFusion,
    ChaosFusion
}

        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary<string, Variant>();

            // 保存合成历史
            var historyList = new List<Dictionary<string, Variant>>();
            foreach (var kvp in SynthesisHistory)
            {
                foreach (var record in kvp.Value)
                {
                    historyList.Add(new Dictionary<string, Variant>
                    {
                        ["pet1_id"] = record.Pet1Id,
                        ["pet2_id"] = record.Pet2Id,
                        ["result_pet_id"] = record.ResultPetId,
                        ["result_pet_type"] = record.ResultPetType ?? "",
                        ["result_rarity"] = record.ResultRarity ?? "",
                        ["was_successful"] = record.WasSuccessful,
                        ["gold_cost"] = record.GoldCost,
                        ["timestamp"] = record.Timestamp
                    });
                }
            }
            data["synthesis_history"] = historyList;

            // 保存已解锁配方
            data["unlocked_recipes"] = new List<string>(UnlockedRecipes);

            // 保存统计
            data["total_syntheses"] = TotalSyntheses;
            data["successful_syntheses"] = SuccessfulSyntheses;
            data["legendary_syntheses"] = LegendarySyntheses;
            data["total_gold_spent"] = TotalGoldSpent;

            // 保存当前合成状态
            data["is_synthesizing"] = IsSynthesizing;
            data["synthesis_pet1_id"] = SynthesisPet1Id;
            data["synthesis_pet2_id"] = SynthesisPet2Id;
            data["synthesis_progress"] = SynthesisProgress;

            return data;
        }

        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;

            // 加载合成历史
            if (data.TryGetValue("synthesis_history", out var historyData))
            {
                var historyList = (List<Variant>)historyData;
                foreach (var recordVar in historyList)
                {
                    var recordDict = (Dictionary<string, Variant>)recordVar;
                    var record = new PetSynthesisRecord();

                    if (recordDict.TryGetValue("pet1_id", out var pet1Id))
                        record.Pet1Id = (int)pet1Id;
                    if (recordDict.TryGetValue("pet2_id", out var pet2Id))
                        record.Pet2Id = (int)pet2Id;
                    if (recordDict.TryGetValue("result_pet_id", out var resultPetId))
                        record.ResultPetId = (int)resultPetId;
                    if (recordDict.TryGetValue("result_pet_type", out var resultPetType))
                        record.ResultPetType = (string)resultPetType;
                    if (recordDict.TryGetValue("result_rarity", out var resultRarity))
                        record.ResultRarity = (string)resultRarity;
                    if (recordDict.TryGetValue("was_successful", out var wasSuccessful))
                        record.WasSuccessful = (bool)wasSuccessful;
                    if (recordDict.TryGetValue("gold_cost", out var goldCost))
                        record.GoldCost = (int)goldCost;
                    if (recordDict.TryGetValue("timestamp", out var timestamp))
                        record.Timestamp = (long)timestamp;

                    if (!SynthesisHistory.ContainsKey(record.Pet1Id))
                    {
                        SynthesisHistory[record.Pet1Id] = new List<PetSynthesisRecord>();
                    }
                    SynthesisHistory[record.Pet1Id].Add(record);
                }
            }

            // 加载已解锁配方
            if (data.TryGetValue("unlocked_recipes", out var recipesData))
            {
                var recipeList = (List<string>)recipesData;
                foreach (var recipe in recipeList)
                {
                    UnlockedRecipes.Add(recipe);
                }
            }

            // 加载统计
            if (data.TryGetValue("total_syntheses", out var totalSynth))
                TotalSyntheses = (int)totalSynth;
            if (data.TryGetValue("successful_syntheses", out var successSynth))
                SuccessfulSyntheses = (int)successSynth;
            if (data.TryGetValue("legendary_syntheses", out var legendarySynth))
                LegendarySyntheses = (int)legendarySynth;
            if (data.TryGetValue("total_gold_spent", out var goldSpent))
                TotalGoldSpent = (int)goldSpent;

            // 加载当前合成状态
            if (data.TryGetValue("is_synthesizing", out var isSynth))
                IsSynthesizing = (bool)isSynth;
            if (data.TryGetValue("synthesis_pet1_id", out var pet1Id))
                SynthesisPet1Id = (int)pet1Id;
            if (data.TryGetValue("synthesis_pet2_id", out var pet2Id))
                SynthesisPet2Id = (int)pet2Id;
            if (data.TryGetValue("synthesis_progress", out var progress))
                SynthesisProgress = (float)progress;
        }
}
