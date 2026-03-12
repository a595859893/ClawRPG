using Godot;
using System;
using System.Collections.Generic;

public class PetSynthesisData : Node
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
