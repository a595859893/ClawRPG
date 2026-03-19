using System;
using System.Collections.Generic;
using Godot;
using Yields;

public partial class PetEvolutionSystem : BaseSystem
{
    private static PetEvolutionSystem _instance;
    public static PetEvolutionSystem Instance
    {
        get
        {
            if (_instance == null) _instance = new PetEvolutionSystem();
            return _instance;
        }
    }

    protected override string SystemName => "PetEvolutionSystem";

    private PetEvolutionData _data;
    
    public PetEvolutionData Data
    {
        get { return _data; }
    }

    public PetEvolutionSystem()
    {
        _data = new PetEvolutionData();
    }

    // Initialize system
    public void Initialize()
    {
        LoadData();
        GD.Print("[PetEvolutionSystem] Initialized - Total Systems: " + GetTotalSystemCount());
    }

    // Get total system count
    public int GetTotalSystemCount()
    {
        int baseCount = 706; // Current milestone
        return baseCount + 1; // Adding Pet Evolution System
    }

    // Check if pet can evolve
    public bool CanEvolve(int petId, string petType)
    {
        if (!PetEvolutionDatabase.EvolutionChains.ContainsKey(petType))
            return false;

        if (!_data.EvolvedPets.ContainsKey(petId))
            return false;

        var record = _data.EvolvedPets[petId];
        var chain = PetEvolutionDatabase.EvolutionChains[petType];
        
        // Find current tier
        int currentTier = 0;
        for (int i = 0; i < chain.Count; i++)
        {
            if (chain[i].FormName == record.CurrentForm)
            {
                currentTier = chain[i].Tier;
                break;
            }
        }

        // Check if can evolve to next tier
        return currentTier < chain.Count && record.EvolutionPoints >= record.RequiredPoints;
    }

    // Evolve pet
    public EvolutionResult EvolvePet(int petId, string petType)
    {
        var result = new EvolutionResult { Success = false };

        if (!PetEvolutionDatabase.EvolutionChains.ContainsKey(petType))
        {
            result.Message = "Unknown pet type: " + petType;
            return result;
        }

        if (!_data.EvolvedPets.ContainsKey(petId))
        {
            // Initialize pet evolution record
            var chain = PetEvolutionDatabase.EvolutionChains[petType];
            var initialForm = chain[0];
            
            var record = new PetEvolutionRecord
            {
                PetId = petId,
                CurrentForm = initialForm.FormName,
                TargetForm = chain.Count > 1 ? chain[1].FormName : initialForm.FormName,
                EvolutionPoints = 0,
                RequiredPoints = chain.Count > 1 ? chain[1].RequiredPoints : 0,
                IsEvolved = false
            };
            
            _data.EvolvedPets[petId] = record;
            result.Message = "Pet evolution record initialized";
            return result;
        }

        var petRecord = _data.EvolvedPets[petId];
        var evolutionChain = PetEvolutionDatabase.EvolutionChains[petType];
        
        // Find current form index
        int currentIndex = -1;
        for (int i = 0; i < evolutionChain.Count; i++)
        {
            if (evolutionChain[i].FormName == petRecord.CurrentForm)
            {
                currentIndex = i;
                break;
            }
        }

        if (currentIndex < 0 || currentIndex >= evolutionChain.Count - 1)
        {
            result.Message = "Pet has reached maximum evolution";
            return result;
        }

        var nextForm = evolutionChain[currentIndex + 1];
        
        if (petRecord.EvolutionPoints < nextForm.RequiredPoints)
        {
            result.Message = $"Need {nextForm.RequiredPoints} evolution points, have {petRecord.EvolutionPoints}";
            return result;
        }

        // Perform evolution
        string originalForm = petRecord.CurrentForm;
        petRecord.CurrentForm = nextForm.FormName;
        petRecord.IsEvolved = nextForm.Tier >= evolutionChain.Count;
        
        if (!petRecord.IsEvolved && currentIndex + 2 < evolutionChain.Count)
        {
            petRecord.TargetForm = evolutionChain[currentIndex + 2].FormName;
            petRecord.RequiredPoints = evolutionChain[currentIndex + 2].RequiredPoints;
        }
        
        petRecord.LastEvolutionTime = DateTime.Now;
        
        // Update statistics
        _data.TotalEvolutions++;
        _data.TotalEvolutionPoints += petRecord.EvolutionPoints;
        
        switch (nextForm.Rarity)
        {
            case "Legendary":
                _data.LegendaryEvolutions++;
                break;
            case "Epic":
                _data.EpicEvolutions++;
                break;
            case "Rare":
                _data.RareEvolutions++;
                break;
        }

        // Add to history
        var historyEntry = new EvolutionHistoryEntry
        {
            PetId = petId,
            OriginalForm = originalForm,
            NewForm = nextForm.FormName,
            EvolutionType = nextForm.Rarity,
            EvolutionTime = DateTime.Now,
            PointsUsed = petRecord.EvolutionPoints
        };
        _data.EvolutionHistory.Add(historyEntry);

        result.Success = true;
        result.NewForm = nextForm.FormName;
        result.Rarity = nextForm.Rarity;
        result.Tier = nextForm.Tier;
        result.Message = $"{originalForm} evolved to {nextForm.FormName}!";
        
        SaveData();
        
        return result;
    }

    // Add evolution points
    public void AddEvolutionPoints(int petId, string petType, int points)
    {
        if (!PetEvolutionDatabase.EvolutionChains.ContainsKey(petType))
            return;

        if (!_data.EvolvedPets.ContainsKey(petId))
        {
            // Initialize record
            var chain = PetEvolutionDatabase.EvolutionChains[petType];
            var initialForm = chain[0];
            
            _data.EvolvedPets[petId] = new PetEvolutionRecord
            {
                PetId = petId,
                CurrentForm = initialForm.FormName,
                TargetForm = chain.Count > 1 ? chain[1].FormName : initialForm.FormName,
                EvolutionPoints = points,
                RequiredPoints = chain.Count > 1 ? chain[1].RequiredPoints : 0,
                IsEvolved = false
            };
        }
        else
        {
            var record = _data.EvolvedPets[petId];
            var chain = PetEvolutionDatabase.EvolutionChains[petType];
            
            // Check if already at max evolution
            int currentIndex = -1;
            for (int i = 0; i < chain.Count; i++)
            {
                if (chain[i].FormName == record.CurrentForm)
                {
                    currentIndex = i;
                    break;
                }
            }
            
            if (currentIndex >= 0 && currentIndex < chain.Count - 1)
            {
                record.EvolutionPoints += points;
            }
        }
        
        SaveData();
    }

    // Get evolution progress
    public EvolutionProgress GetEvolutionProgress(int petId, string petType)
    {
        var progress = new EvolutionProgress { CanEvolve = false };

        if (!PetEvolutionDatabase.EvolutionChains.ContainsKey(petType))
            return progress;

        if (!_data.EvolvedPets.ContainsKey(petId))
        {
            var chain = PetEvolutionDatabase.EvolutionChains[petType];
            if (chain.Count > 0)
            {
                progress.CurrentForm = chain[0].FormName;
                progress.NextForm = chain.Count > 1 ? chain[1].FormName : chain[0].FormName;
                progress.CurrentPoints = 0;
                progress.RequiredPoints = chain.Count > 1 ? chain[1].RequiredPoints : 0;
            }
            return progress;
        }

        var record = _data.EvolvedPets[petId];
        var evolutionChain = PetEvolutionDatabase.EvolutionChains[petType];

        progress.CurrentForm = record.CurrentForm;
        progress.CurrentPoints = record.EvolutionPoints;
        progress.RequiredPoints = record.RequiredPoints;
        
        int currentIndex = -1;
        for (int i = 0; i < evolutionChain.Count; i++)
        {
            if (evolutionChain[i].FormName == record.CurrentForm)
            {
                currentIndex = i;
                break;
            }
        }

        if (currentIndex >= 0 && currentIndex < evolutionChain.Count - 1)
        {
            progress.NextForm = evolutionChain[currentIndex + 1].FormName;
            progress.NextRarity = evolutionChain[currentIndex + 1].Rarity;
            progress.CanEvolve = record.EvolutionPoints >= record.RequiredPoints;
        }
        else
        {
            progress.NextForm = record.CurrentForm;
            progress.IsMaxEvolution = true;
        }

        progress.ProgressPercent = progress.RequiredPoints > 0 
            ? Mathf.Min(100, (int)((float)progress.CurrentPoints / progress.RequiredPoints * 100))
            : 100;

        return progress;
    }

    // Get evolution chain for pet type
    public List<EvolutionForm> GetEvolutionChain(string petType)
    {
        if (PetEvolutionDatabase.EvolutionChains.ContainsKey(petType))
            return PetEvolutionDatabase.EvolutionChains[petType];
        return new List<EvolutionForm>();
    }

    // Get all pet evolution records
    public Dictionary<int, PetEvolutionRecord> GetAllEvolutionRecords()
    {
        return _data.EvolvedPets;
    }

    // Get statistics
    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            { "total_evolutions", _data.TotalEvolutions },
            { "legendary_evolutions", _data.LegendaryEvolutions },
            { "epic_evolutions", _data.EpicEvolutions },
            { "rare_evolutions", _data.RareEvolutions },
            { "total_points", _data.TotalEvolutionPoints },
            { "history_count", _data.EvolutionHistory.Count }
        };
    }

    // Save data
    public void SaveData()
    {
        var saveSystem = SaveSystem.Instance;
        if (saveSystem == null) return;

        var data = saveSystem.LoadGame();
        if (data == null) data = new Godot.Dictionary();

        // Save evolved pets
        var evolvedPetsArray = new Godot.Array();
        foreach (var kvp in _data.EvolvedPets)
        {
            var petData = new Godot.Dictionary();
            petData["pet_id"] = kvp.Key;
            petData["current_form"] = kvp.Value.CurrentForm;
            petData["target_form"] = kvp.Value.TargetForm;
            petData["evolution_points"] = kvp.Value.EvolutionPoints;
            petData["required_points"] = kvp.Value.RequiredPoints;
            petData["is_evolved"] = kvp.Value.IsEvolved;
            if (kvp.Value.LastEvolutionTime != default(DateTime))
                petData["last_evolution"] = kvp.Value.LastEvolutionTime.ToString("o");
            evolvedPetsArray.Add(petData);
        }
        data["pet_evolution_pets"] = evolvedPetsArray;

        // Save evolution history (limit to last 50)
        var historyArray = new Godot.Array();
        var recentHistory = _data.EvolutionHistory.TakeLast(50).ToList();
        foreach (var entry in recentHistory)
        {
            var historyData = new Godot.Dictionary();
            historyData["pet_id"] = entry.PetId;
            historyData["original_form"] = entry.OriginalForm;
            historyData["new_form"] = entry.NewForm;
            historyData["evolution_type"] = entry.EvolutionType;
            historyData["time"] = entry.EvolutionTime.ToString("o");
            historyData["points_used"] = entry.PointsUsed;
            historyArray.Add(historyData);
        }
        data["pet_evolution_history"] = historyArray;

        // Save stats
        var stats = new Godot.Dictionary();
        stats["total_evolutions"] = _data.TotalEvolutions;
        stats["legendary_evolutions"] = _data.LegendaryEvolutions;
        stats["epic_evolutions"] = _data.EpicEvolutions;
        stats["rare_evolutions"] = _data.RareEvolutions;
        stats["total_points"] = _data.TotalEvolutionPoints;
        data["pet_evolution_stats"] = stats;

        saveSystem.SaveGame(data);
    }

    // Load data
    public void LoadData()
    {
        var saveSystem = SaveSystem.Instance;
        if (saveSystem == null) return;

        var data = saveSystem.LoadGame();
        if (data == null) return;

        // Load evolved pets
        if (data.Contains("pet_evolution_pets"))
        {
            var petsArray = (Godot.Array)data["pet_evolution_pets"];
            foreach (Dictionary petData in petsArray)
            {
                var record = new PetEvolutionRecord
                {
                    PetId = (int)petData["pet_id"],
                    CurrentForm = (string)petData["current_form"],
                    TargetForm = (string)petData["target_form"],
                    EvolutionPoints = (int)petData["evolution_points"],
                    RequiredPoints = (int)petData["required_points"],
                    IsEvolved = (bool)petData["is_evolved"]
                };
                if (petData.Contains("last_evolution"))
                    record.LastEvolutionTime = DateTime.Parse((string)petData["last_evolution"]);
                _data.EvolvedPets[record.PetId] = record;
            }
        }

        // Load evolution history
        if (data.Contains("pet_evolution_history"))
        {
            var historyArray = (Godot.Array)data["pet_evolution_history"];
            foreach (Dictionary historyData in historyArray)
            {
                var entry = new EvolutionHistoryEntry
                {
                    PetId = (int)historyData["pet_id"],
                    OriginalForm = (string)historyData["original_form"],
                    NewForm = (string)historyData["new_form"],
                    EvolutionType = (string)historyData["evolution_type"],
                    EvolutionTime = DateTime.Parse((string)historyData["time"]),
                    PointsUsed = (int)historyData["points_used"]
                };
                _data.EvolutionHistory.Add(entry);
            }
        }

        // Load stats
        if (data.Contains("pet_evolution_stats"))
        {
            var stats = (Godot.Dictionary)data["pet_evolution_stats"];
            _data.TotalEvolutions = (int)stats.Get("total_evolutions", 0);
            _data.LegendaryEvolutions = (int)stats.Get("legendary_evolutions", 0);
            _data.EpicEvolutions = (int)stats.Get("epic_evolutions", 0);
            _data.RareEvolutions = (int)stats.Get("rare_evolutions", 0);
            _data.TotalEvolutionPoints = (int)stats.Get("total_points", 0);
        }
    }
}

public class EvolutionResult
{
    public bool Success { get; set; }
    public string NewForm { get; set; } = "";
    public string Rarity { get; set; } = "";
    public int Tier { get; set; }
    public string Message { get; set; } = "";
}

public class EvolutionProgress
{
    public string CurrentForm { get; set; } = "";
    public string NextForm { get; set; } = "";
    public string NextRarity { get; set; } = "";
    public int CurrentPoints { get; set; }
    public int RequiredPoints { get; set; }
    public int ProgressPercent { get; set; }
    public bool CanEvolve { get; set; }
    public bool IsMaxEvolution { get; set; }
}

/// <summary>
/// PetEvolutionSystem partial class for BaseSystem integration
/// </summary>
public partial class PetEvolutionSystem
{
    /// <summary>
    /// Export save data (BaseSystem override)
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        data["evolvedPets"] = _data.EvolvedPets;
        data["totalEvolutions"] = _data.TotalEvolutions;
        data["legendaryEvolutions"] = _data.LegendaryEvolutions;
        data["epicEvolutions"] = _data.EpicEvolutions;
        data["rareEvolutions"] = _data.RareEvolutions;
        data["totalEvolutionPoints"] = _data.TotalEvolutionPoints;
        data["evolutionHistory"] = _data.EvolutionHistory;
        return data;
    }
    
    /// <summary>
    /// Import save data (BaseSystem override)
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("evolvedPets"))
        {
            _data.EvolvedPets = (Dictionary<int, PetEvolutionRecord>)data["evolvedPets"];
        }
        if (data.Contains("totalEvolutions"))
        {
            _data.TotalEvolutions = (int)data["totalEvolutions"];
        }
        if (data.Contains("legendaryEvolutions"))
        {
            _data.LegendaryEvolutions = (int)data["legendaryEvolutions"];
        }
        if (data.Contains("epicEvolutions"))
        {
            _data.EpicEvolutions = (int)data["epicEvolutions"];
        }
        if (data.Contains("rareEvolutions"))
        {
            _data.RareEvolutions = (int)data["rareEvolutions"];
        }
        if (data.Contains("totalEvolutionPoints"))
        {
            _data.TotalEvolutionPoints = (int)data["totalEvolutionPoints"];
        }
        if (data.Contains("evolutionHistory"))
        {
            _data.EvolutionHistory = (List<EvolutionHistoryEntry>)data["evolutionHistory"];
        }
    }
}
