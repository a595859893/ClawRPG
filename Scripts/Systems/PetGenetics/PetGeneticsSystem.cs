using Godot;
using System;
using System.Collections.Generic;

public partial class PetGeneticsSystem : BaseSystem
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

    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, Variant>();

        if (_data == null) return data;

        // 保存活跃基因数据
        var activeGenesData = new Dictionary<string, List<Dictionary<string, Variant>>>();
        foreach (var kvp in _data.ActiveGenes)
        {
            var genesList = new List<Dictionary<string, Variant>>();
            foreach (var gene in kvp.Value)
            {
                genesList.Add(new Dictionary<string, Variant>
                {
                    ["gene_id"] = gene.GeneId ?? "",
                    ["gene_name"] = gene.GeneName ?? "",
                    ["gene_type"] = gene.GeneType ?? "",
                    ["rarity"] = gene.Rarity ?? "",
                    ["strength_bonus"] = gene.StrengthBonus,
                    ["vitality_bonus"] = gene.VitalityBonus,
                    ["agility_bonus"] = gene.AgilityBonus,
                    ["intelligence_bonus"] = gene.IntelligenceBonus,
                    ["luck_bonus"] = gene.LuckBonus,
                    ["special_effect"] = gene.SpecialEffect ?? ""
                });
            }
            activeGenesData[kvp.Key] = genesList;
        }
        data["active_genes"] = activeGenesData;

        // 保存已解锁的基因模板
        data["unlocked_gene_templates"] = new List<string>(_data.UnlockedGeneTemplates);

        // 保存基因修改历史
        var historyList = new List<Dictionary<string, Variant>>();
        foreach (var record in _data.ModificationHistory)
        {
            historyList.Add(new Dictionary<string, Variant>
            {
                ["pet_id"] = record.PetId ?? "",
                ["gene_id"] = record.GeneId ?? "",
                ["modification_type"] = record.ModificationType ?? "",
                ["timestamp"] = record.Timestamp,
                ["success"] = record.Success
            });
        }
        data["modification_history"] = historyList;

        // 保存统计数据
        data["total_modifications"] = _data.TotalModifications;
        data["legendary_genes_created"] = _data.LegendaryGenesCreated;
        data["epic_genes_created"] = _data.EpicGenesCreated;
        data["rare_genes_created"] = _data.RareGenesCreated;

        return data;
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null || _data == null) return;

        // 加载活跃基因数据
        if (data.TryGetValue("active_genes", out var activeGenesData))
        {
            _data.ActiveGenes = new Dictionary<string, List<PetGene>>();
            var genesDict = (Dictionary<string, Variant>)activeGenesData;
            foreach (var kvp in genesDict)
            {
                var genesList = new List<PetGene>();
                var genesVarList = (List<Variant>)kvp.Value;
                foreach (var geneVar in genesVarList)
                {
                    var geneDict = (Dictionary<string, Variant>)geneVar;
                    var gene = new PetGene("", "", "", "");

                    if (geneDict.TryGetValue("gene_id", out var geneId))
                        gene.GeneId = (string)geneId;
                    if (geneDict.TryGetValue("gene_name", out var geneName))
                        gene.GeneName = (string)geneName;
                    if (geneDict.TryGetValue("gene_type", out var geneType))
                        gene.GeneType = (string)geneType;
                    if (geneDict.TryGetValue("rarity", out var rarity))
                        gene.Rarity = (string)rarity;
                    if (geneDict.TryGetValue("strength_bonus", out var strBonus))
                        gene.StrengthBonus = (float)strBonus;
                    if (geneDict.TryGetValue("vitality_bonus", out var vitBonus))
                        gene.VitalityBonus = (float)vitBonus;
                    if (geneDict.TryGetValue("agility_bonus", out var agiBonus))
                        gene.AgilityBonus = (float)agiBonus;
                    if (geneDict.TryGetValue("intelligence_bonus", out var intBonus))
                        gene.IntelligenceBonus = (float)intBonus;
                    if (geneDict.TryGetValue("luck_bonus", out var luckBonus))
                        gene.LuckBonus = (float)luckBonus;
                    if (geneDict.TryGetValue("special_effect", out var specialEffect))
                        gene.SpecialEffect = (string)specialEffect;

                    genesList.Add(gene);
                }
                _data.ActiveGenes[kvp.Key] = genesList;
            }
        }

        // 加载已解锁的基因模板
        if (data.TryGetValue("unlocked_gene_templates", out var templatesData))
            _data.UnlockedGeneTemplates = new List<string>((List<string>)templatesData);

        // 加载基因修改历史
        if (data.TryGetValue("modification_history", out var historyData))
        {
            _data.ModificationHistory = new List<GeneModificationRecord>();
            var historyList = (List<Variant>)historyData;
            foreach (var recordVar in historyList)
            {
                var recordDict = (Dictionary<string, Variant>)recordVar;
                var record = new GeneModificationRecord();

                if (recordDict.TryGetValue("pet_id", out var petId))
                    record.PetId = (string)petId;
                if (recordDict.TryGetValue("gene_id", out var geneId))
                    record.GeneId = (string)geneId;
                if (recordDict.TryGetValue("modification_type", out var modType))
                    record.ModificationType = (string)modType;
                if (recordDict.TryGetValue("timestamp", out var timestamp))
                    record.Timestamp = (int)timestamp;
                if (recordDict.TryGetValue("success", out var success))
                    record.Success = (bool)success;

                _data.ModificationHistory.Add(record);
            }
        }

        // 加载统计数据
        if (data.TryGetValue("total_modifications", out var totalMods))
            _data.TotalModifications = (int)totalMods;
        if (data.TryGetValue("legendary_genes_created", out var legendaryGenes))
            _data.LegendaryGenesCreated = (int)legendaryGenes;
        if (data.TryGetValue("epic_genes_created", out var epicGenes))
            _data.EpicGenesCreated = (int)epicGenes;
        if (data.TryGetValue("rare_genes_created", out var rareGenes))
            _data.RareGenesCreated = (int)rareGenes;
    }
}
