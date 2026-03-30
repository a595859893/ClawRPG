using Godot;
using System;
using System.Collections.Generic;

public class PetSynthesisSystem : BaseSystem
{
    private static PetSynthesisSystem _instance;
    public static PetSynthesisSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PetSynthesisSystem();
            }
            return _instance;
        }
    }
    
    private PetSynthesisData _data;
    private PetManager _petManager;
    private Player _player;
    
    // Signals
    public static string SignalSynthesisStarted = "synthesis_started";
    public static string SignalSynthesisCompleted = "synthesis_completed";
    public static string SignalSynthesisFailed = "synthesis_failed";
    
    public override void _Ready()
    {
        _instance = this;
        _data = new PetSynthesisData();
        
        // Get references
        _petManager = GetNode<PetManager>("/root/Main/PetManager");
        _player = GetNode<Player>("/root/Main/Player");
    }
    
    public void Initialize()
    {
        _data = GetNode<PetSynthesisData>("/root/Main/PetSynthesisData");
        if (_data == null)
        {
            _data = new PetSynthesisData();
            AddChild(_data);
        }
    }
    
    #region Synthesis Operations
    
    public bool CanSynthesize(int pet1Id, int pet2Id)
    {
        if (_petManager == null || _player == null)
            return false;
            
        var pet1 = _petManager.GetPet(pet1Id);
        var pet2 = _petManager.GetPet(pet2Id);
        
        if (pet1 == null || pet2 == null)
            return false;
            
        // Check if both pets exist
        if (pet1.Id < 0 || pet2.Id < 0)
            return false;
            
        // Check player level
        var recipe = PetSynthesisDatabase.Instance.GetRecipe(pet1.PetType, pet2.PetType);
        if (recipe != null && _player.Level < recipe.RequiredLevel)
            return false;
            
        // Check gold
        int cost = recipe != null ? recipe.GoldCost : 500;
        if (_player.Gold < cost)
            return false;
            
        return true;
    }
    
    public SynthesisResult StartSynthesis(int pet1Id, int pet2Id)
    {
        if (!CanSynthesize(pet1Id, pet2Id))
            return SynthesisResult.Failure;
            
        var pet1 = _petManager.GetPet(pet1Id);
        var pet2 = _petManager.GetPet(pet2Id);
        
        // Get recipe or generate random
        var recipe = PetSynthesisDatabase.Instance.GetRecipe(pet1.PetType, pet2.PetType);
        
        int goldCost = recipe != null ? recipe.GoldCost : 500;
        float successRate = recipe != null ? recipe.SuccessRate : 0.5f;
        var synthesisType = recipe != null ? recipe.SynthesisType : GetRandomSynthesisType(pet1, pet2);
        
        // Deduct gold
        _player.Gold -= goldCost;
        _data.TotalGoldSpent += goldCost;
        
        // Start synthesis
        _data.IsSynthesizing = true;
        _data.SynthesisPet1Id = pet1Id;
        _data.SynthesisPet2Id = pet2Id;
        _data.SynthesisProgress = 0.0f;
        
        // Emit signal
        EmitSignal(SignalSynthesisStarted, pet1Id, pet2Id);
        
        // Perform synthesis
        var result = PerformSynthesis(pet1, pet2, recipe, synthesisType, successRate);
        
        // Update statistics
        _data.TotalSyntheses++;
        if (result != SynthesisResult.Failure)
        {
            _data.SuccessfulSyntheses++;
            if (result == SynthesisResult.Legendary)
                _data.LegendarySyntheses++;
        }
        
        // Reset synthesis state
        _data.IsSynthesizing = false;
        _data.SynthesisPet1Id = -1;
        _data.SynthesisPet2Id = -1;
        _data.SynthesisProgress = 0.0f;
        
        // Record synthesis
        var record = new PetSynthesisRecord
        {
            Pet1Id = pet1Id,
            Pet2Id = pet2Id,
            ResultPetId = result != SynthesisResult.Failure ? _petManager.GetNextPetId() : -1,
            ResultPetType = result != SynthesisResult.Failure ? (recipe != null ? recipe.ResultPetType : "Unknown") : "None",
            ResultRarity = result.ToString(),
            WasSuccessful = result != SynthesisResult.Failure,
            GoldCost = goldCost,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        
        if (!_data.SynthesisHistory.ContainsKey(pet1Id))
            _data.SynthesisHistory[pet1Id] = new List<PetSynthesisRecord>();
        _data.SynthesisHistory[pet1Id].Add(record);
        
        // Emit result signal
        if (result != SynthesisResult.Failure)
        {
            EmitSignal(SignalSynthesisCompleted, record.ResultPetId, result.ToString());
        }
        else
        {
            EmitSignal(SignalSynthesisFailed);
        }
        
        return result;
    }
    
    private SynthesisResult PerformSynthesis(object pet1, object pet2, PetSynthesisRecipe recipe, SynthesisType type, float successRate)
    {
        // Roll for rarity
        var resultRarity = PetSynthesisDatabase.Instance.RollRarity(successRate);
        
        if (resultRarity == SynthesisResult.Failure)
        {
            // Remove pets on failure
            if (_petManager != null)
            {
                _petManager.RemovePet(_data.SynthesisPet1Id);
                _petManager.RemovePet(_data.SynthesisPet2Id);
            }
            return SynthesisResult.Failure;
        }
        
        // Determine result pet
        string resultPetType;
        if (recipe != null)
        {
            resultPetType = recipe.ResultPetType;
        }
        else
        {
            resultPetType = PetSynthesisDatabase.Instance.GetRandomResultPet(resultRarity, type);
        }
        
        // Remove source pets
        if (_petManager != null)
        {
            _petManager.RemovePet(_data.SynthesisPet1Id);
            _petManager.RemovePet(_data.SynthesisPet2Id);
            
            // Create new pet
            var newPet = _petManager.CreatePet(resultPetType, resultRarity.ToString());
            if (newPet != null)
            {
                // Add bonus stats based on rarity
                ApplyRarityBonus(newPet, resultRarity);
            }
        }
        
        // Unlock recipe if exists
        if (recipe != null)
        {
            _data.UnlockedRecipes.Add(recipe.Id);
        }
        
        return resultRarity;
    }
    
    private SynthesisType GetRandomSynthesisType(object pet1, object pet2)
    {
        var random = new Random();
        var types = Enum.GetValues(typeof(SynthesisType));
        return (SynthesisType)types.GetValue(random.Next(types.Length));
    }
    
    private void ApplyRarityBonus(object pet, SynthesisResult rarity)
    {
        // Apply stat bonuses based on rarity
        float bonusMultiplier = 1.0f;
        
        switch (rarity)
        {
            case SynthesisResult.Common:
                bonusMultiplier = 1.0f;
                break;
            case SynthesisResult.Uncommon:
                bonusMultiplier = 1.2f;
                break;
            case SynthesisResult.Rare:
                bonusMultiplier = 1.4f;
                break;
            case SynthesisResult.Epic:
                bonusMultiplier = 1.7f;
                break;
            case SynthesisResult.Legendary:
                bonusMultiplier = 2.0f;
                break;
        }
        
        // Note: Actual stat bonus application depends on Pet class implementation
    }
    
    #endregion
    
    #region Getters
    
    public PetSynthesisData GetData()
    {
        return _data;
    }
    
    public List<PetSynthesisRecord> GetSynthesisHistory(int petId)
    {
        if (_data.SynthesisHistory.ContainsKey(petId))
            return _data.SynthesisHistory[petId];
        return new List<PetSynthesisRecord>();
    }
    
    public List<PetSynthesisRecord> GetAllSynthesisHistory()
    {
        var allHistory = new List<PetSynthesisRecord>();
        foreach (var kvp in _data.SynthesisHistory)
        {
            allHistory.AddRange(kvp.Value);
        }
        return allHistory;
    }
    
    public List<PetSynthesisRecipe> GetAvailableRecipes()
    {
        if (_player == null)
            return new List<PetSynthesisRecipe>();
        return PetSynthesisDatabase.Instance.GetAvailableRecipes(_player.Level);
    }
    
    public bool IsRecipeUnlocked(string recipeId)
    {
        return _data.UnlockedRecipes.Contains(recipeId);
    }
    
    public int GetTotalSyntheses()
    {
        return _data.TotalSyntheses;
    }
    
    public int GetSuccessfulSyntheses()
    {
        return _data.SuccessfulSyntheses;
    }
    
    public int GetLegendarySyntheses()
    {
        return _data.LegendarySyntheses;
    }
    
    public float GetSuccessRate()
    {
        if (_data.TotalSyntheses == 0)
            return 0f;
        return (float)_data.SuccessfulSyntheses / _data.TotalSyntheses;
    }
    
    #endregion
    
    #region Save/Load
    
    public Dictionary<string, object> Save()
    {
        return _data.Save();
    }
    
    public void Load(Dictionary<string, object> data)
    {
        if (_data != null)
            _data.Load(data);
    }
    
    #endregion

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, Variant>();
            
            // 保存合成统计
            data["totalSyntheses"] = _data.TotalSyntheses;
            data["successfulSyntheses"] = _data.SuccessfulSyntheses;
            data["legendarySyntheses"] = _data.LegendarySyntheses;
            data["totalGoldSpent"] = _data.TotalGoldSpent;
            
            // 保存已解锁的配方
            data["unlockedRecipes"] = new List<Variant>(_data.UnlockedRecipes);
            
            // 保存合成历史
            var historyData = new Dictionary<string, Variant>();
            foreach (var kvp in _data.SynthesisHistory)
            {
                var records = new List<Dictionary<string, Variant>>();
                foreach (var record in kvp.Value)
                {
                    records.Add(new Dictionary<string, Variant>
                    {
                        { "pet1Id", record.Pet1Id },
                        { "pet2Id", record.Pet2Id },
                        { "resultPetId", record.ResultPetId },
                        { "resultPetType", record.ResultPetType },
                        { "resultRarity", record.ResultRarity },
                        { "wasSuccessful", record.WasSuccessful },
                        { "goldCost", record.GoldCost },
                        { "timestamp", record.Timestamp }
                    });
                }
                historyData[kvp.Key.ToString()] = records;
            }
            data["synthesisHistory"] = historyData;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            // 加载合成统计
            if (data.TryGetValue("totalSyntheses", out var totalSyntheses))
                _data.TotalSyntheses = (int)totalSyntheses;
            if (data.TryGetValue("successfulSyntheses", out var successfulSyntheses))
                _data.SuccessfulSyntheses = (int)successfulSyntheses;
            if (data.TryGetValue("legendarySyntheses", out var legendarySyntheses))
                _data.LegendarySyntheses = (int)legendarySyntheses;
            if (data.TryGetValue("totalGoldSpent", out var totalGoldSpent))
                _data.TotalGoldSpent = (int)totalGoldSpent;
            
            // 加载已解锁的配方
            if (data.TryGetValue("unlockedRecipes", out var unlockedRecipes))
                _data.UnlockedRecipes = new HashSet<string>((IEnumerable<string>)unlockedRecipes);
            
            // 加载合成历史
            if (data.TryGetValue("synthesisHistory", out var historyData))
            {
                var hd = (Dictionary<string, Variant>)historyData;
                foreach (var kvp in hd)
                {
                    if (int.TryParse(kvp.Key, out var pet1Id))
                    {
                        var records = new List<PetSynthesisRecord>();
                        var recordsList = (List<Variant>)kvp.Value;
                        foreach (var recordVar in recordsList)
                        {
                            var rData = (Dictionary<string, Variant>)recordVar;
                            var record = new PetSynthesisRecord
                            {
                                Pet1Id = (int)rData["pet1Id"],
                                Pet2Id = (int)rData["pet2Id"],
                                ResultPetId = (int)rData["resultPetId"],
                                ResultPetType = (string)rData["resultPetType"],
                                ResultRarity = (string)rData["resultRarity"],
                                WasSuccessful = (bool)rData["wasSuccessful"],
                                GoldCost = (int)rData["goldCost"],
                                Timestamp = (long)rData["timestamp"]
                            };
                            records.Add(record);
                        }
                        _data.SynthesisHistory[pet1Id] = records;
                    }
                }
            }
        }
}
