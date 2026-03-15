using Godot;
using System;
using System.Collections.Generic;

public class PetGeneticsSystem : BaseSystem
{
    private PetGeneticsData _data;
    private PetGeneticsDatabase _database;
    
    // Maximum genes per pet
    private const int MAX_GENES_PER_PET = 5;
    
    // Gene modification cost
    private const int MODIFY_COST = 100;
    
    public override void _Ready()
    {
        _data = new PetGeneticsData();
        _database = new PetGeneticsDatabase();
        AddChild(_database);
        
        LoadData();
    }
    
    public void LoadData()
    {
        // Load from file
        string path = "user://pet_genetics_save.json";
        if (FileAccess.FileExists(path))
        {
            var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            string json = file.GetAsText();
            file.Close();
            
            var data = JSON.Parse(json).Result as Dictionary<string, object>;
            if (data != null)
            {
                _data.Load(data);
            }
        }
    }
    
    public void SaveData()
    {
        var file = FileAccess.Open("user://pet_genetics_save.json", FileAccess.ModeFlags.Write);
        var json = JSON.Stringify(_data.Save());
        file.StoreString(json);
        file.Close();
    }
    
    public bool AddGeneToPet(string petId, string geneType = "")
    {
        if (!_data.ActiveGenes.ContainsKey(petId))
        {
            _data.ActiveGenes[petId] = new List<PetGene>();
        }
        
        if (_data.ActiveGenes[petId].Count >= MAX_GENES_PER_PET)
        {
            GD.Print("Pet has maximum number of genes");
            return false;
        }
        
        var gene = _database.GenerateRandomGene(geneType);
        _data.ActiveGenes[petId].Add(gene);
        
        // Record modification
        var record = new GeneModificationRecord
        {
            PetId = petId,
            GeneId = gene.GeneId,
            ModificationType = "Add",
            Timestamp = OS.GetUnixTime(),
            Success = true
        };
        _data.ModificationHistory.Add(record);
        _data.TotalModifications++;
        
        if (gene.Rarity == "Legendary")
            _data.LegendaryGenesCreated++;
        else if (gene.Rarity == "Epic")
            _data.EpicGenesCreated++;
        else if (gene.Rarity == "Rare")
            _data.RareGenesCreated++;
        
        // Unlock template
        string templateId = gene.GeneId.Split('_')[0] + "_" + gene.GeneId.Split('_')[1];
        if (!_data.UnlockedGeneTemplates.Contains(templateId))
        {
            _data.UnlockedGeneTemplates.Add(templateId);
        }
        
        SaveData();
        return true;
    }
    
    public bool RemoveGeneFromPet(string petId, int geneIndex)
    {
        if (!_data.ActiveGenes.ContainsKey(petId))
            return false;
        
        if (geneIndex < 0 || geneIndex >= _data.ActiveGenes[petId].Count)
            return false;
        
        var gene = _data.ActiveGenes[petId][geneIndex];
        _data.ActiveGenes[petId].RemoveAt(geneIndex);
        
        var record = new GeneModificationRecord
        {
            PetId = petId,
            GeneId = gene.GeneId,
            ModificationType = "Remove",
            Timestamp = OS.GetUnixTime(),
            Success = true
        };
        _data.ModificationHistory.Add(record);
        _data.TotalModifications++;
        
        SaveData();
        return true;
    }
    
    public bool UpgradeGene(string petId, int geneIndex)
    {
        if (!_data.ActiveGenes.ContainsKey(petId))
            return false;
        
        if (geneIndex < 0 || geneIndex >= _data.ActiveGenes[petId].Count)
            return false;
        
        var gene = _data.ActiveGenes[petId][geneIndex];
        
        // Upgrade bonuses
        gene.StrengthBonus *= 1.25f;
        gene.VitalityBonus *= 1.25f;
        gene.AgilityBonus *= 1.25f;
        gene.IntelligenceBonus *= 1.25f;
        gene.LuckBonus *= 1.25f;
        
        var record = new GeneModificationRecord
        {
            PetId = petId,
            GeneId = gene.GeneId,
            ModificationType = "Upgrade",
            Timestamp = OS.GetUnixTime(),
            Success = true
        };
        _data.ModificationHistory.Add(record);
        _data.TotalModifications++;
        
        SaveData();
        return true;
    }
    
    public List<PetGene> GetPetGenes(string petId)
    {
        if (!_data.ActiveGenes.ContainsKey(petId))
            return new List<PetGene>();
        
        return _data.ActiveGenes[petId];
    }
    
    public Dictionary<string, float> CalculateGeneBonuses(string petId)
    {
        var bonuses = new Dictionary<string, float>
        {
            {"strength", 0},
            {"vitality", 0},
            {"agility", 0},
            {"intelligence", 0},
            {"luck", 0},
            {"attack", 0},
            {"defense", 0},
            {"health", 0},
            {"speed", 0},
            {"crit", 0},
            {"evasion", 0},
            {"goldBonus", 0},
            {"expBonus", 0}
        };
        
        var genes = GetPetGenes(petId);
        foreach (var gene in genes)
        {
            bonuses["strength"] += gene.StrengthBonus;
            bonuses["vitality"] += gene.VitalityBonus;
            bonuses["agility"] += gene.AgilityBonus;
            bonuses["intelligence"] += gene.IntelligenceBonus;
            bonuses["luck"] += gene.LuckBonus;
            
            // Apply to combat stats
            bonuses["attack"] += gene.StrengthBonus * 10;
            bonuses["defense"] += gene.VitalityBonus * 5;
            bonuses["health"] += gene.VitalityBonus * 20;
            bonuses["speed"] += gene.AgilityBonus * 2;
            bonuses["crit"] += gene.AgilityBonus * 0.5f;
            bonuses["evasion"] += gene.AgilityBonus * 0.5f;
            
            // Special effects
            if (gene.SpecialEffect == "DoubleGold")
                bonuses["goldBonus"] += 0.25f;
            else if (gene.SpecialEffect == "DoubleExp")
                bonuses["expBonus"] += 0.25f;
        }
        
        return bonuses;
    }
    
    public List<string> GetUnlockedTemplates()
    {
        return _data.UnlockedGeneTemplates;
    }
    
    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            {"TotalModifications", _data.TotalModifications},
            {"LegendaryGenes", _data.LegendaryGenesCreated},
            {"EpicGenes", _data.EpicGenesCreated},
            {"RareGenes", _data.RareGenesCreated},
            {"UnlockedTemplates", _data.UnlockedGeneTemplates.Count},
            {"ActivePets", _data.ActiveGenes.Count}
        };
    }
    
    public List<GeneModificationRecord> GetModificationHistory()
    {
        return _data.ModificationHistory;
    }

        public override Dictionary ExportSaveData() => new();
        public override void ImportSaveData(Dictionary data) { }
}
