using Godot;
using System;
using System.Collections.Generic;

public partial class PetGeneticsData : BaseSystem
{
    // Gene pool for each pet type
    public Dictionary<string, List<PetGene>> ActiveGenes = new Dictionary<string, List<PetGene>>();
    
    // Discovered gene templates
    public List<string> UnlockedGeneTemplates = new List<string>();
    
    // Gene modification history
    public List<GeneModificationRecord> ModificationHistory = new List<GeneModificationRecord>();
    
    // Statistics
    public int TotalModifications = 0;
    public int LegendaryGenesCreated = 0;
    public int EpicGenesCreated = 0;
    public int RareGenesCreated = 0;
    
    public override void _Ready()
    {
        // Initialize default unlocked genes
        if (UnlockedGeneTemplates.Count == 0)
        {
            UnlockedGeneTemplates.Add("Strength");
            UnlockedGeneTemplates.Add("Vitality");
            UnlockedGeneTemplates.Add("Agility");
        }
    }
    
    public Dictionary<string, object> Save()
    {
        return new Dictionary<string, object>
        {
            {"ActiveGenes", ActiveGenes},
            {"UnlockedGeneTemplates", UnlockedGeneTemplates},
            {"ModificationHistory", ModificationHistory},
            {"TotalModifications", TotalModifications},
            {"LegendaryGenesCreated", LegendaryGenesCreated},
            {"EpicGenesCreated", EpicGenesCreated},
            {"RareGenesCreated", RareGenesCreated}
        };
    }
    
    public void Load(Dictionary<string, object> data)
    {
        if (data.ContainsKey("ActiveGenes"))
            ActiveGenes = (Dictionary<string, List<PetGene>>)data["ActiveGenes"];
        if (data.ContainsKey("UnlockedGeneTemplates"))
            UnlockedGeneTemplates = (List<string>)data["UnlockedGeneTemplates"];
        if (data.ContainsKey("ModificationHistory"))
            ModificationHistory = (List<GeneModificationRecord>)data["ModificationHistory"];
        if (data.ContainsKey("TotalModifications"))
            TotalModifications = (int)data["TotalModifications"];
        if (data.ContainsKey("LegendaryGenesCreated"))
            LegendaryGenesCreated = (int)data["LegendaryGenesCreated"];
        if (data.ContainsKey("EpicGenesCreated"))
            EpicGenesCreated = (int)data["EpicGenesCreated"];
        if (data.ContainsKey("RareGenesCreated"))
            RareGenesCreated = (int)data["RareGenesCreated"];
    }

    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, Variant>();

        // 保存活跃基因数据
        var activeGenesData = new Dictionary<string, List<Dictionary<string, Variant>>>();
        foreach (var kvp in ActiveGenes)
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
        data["unlocked_gene_templates"] = new List<string>(UnlockedGeneTemplates);

        // 保存基因修改历史
        var historyList = new List<Dictionary<string, Variant>>();
        foreach (var record in ModificationHistory)
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
        data["total_modifications"] = TotalModifications;
        data["legendary_genes_created"] = LegendaryGenesCreated;
        data["epic_genes_created"] = EpicGenesCreated;
        data["rare_genes_created"] = RareGenesCreated;

        return data;
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        // 加载活跃基因数据
        if (data.TryGetValue("active_genes", out var activeGenesData))
        {
            ActiveGenes = new Dictionary<string, List<PetGene>>();
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
                ActiveGenes[kvp.Key] = genesList;
            }
        }

        // 加载已解锁的基因模板
        if (data.TryGetValue("unlocked_gene_templates", out var templatesData))
            UnlockedGeneTemplates = new List<string>((List<string>)templatesData);

        // 加载基因修改历史
        if (data.TryGetValue("modification_history", out var historyData))
        {
            ModificationHistory = new List<GeneModificationRecord>();
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

                ModificationHistory.Add(record);
            }
        }

        // 加载统计数据
        if (data.TryGetValue("total_modifications", out var totalMods))
            TotalModifications = (int)totalMods;
        if (data.TryGetValue("legendary_genes_created", out var legendaryGenes))
            LegendaryGenesCreated = (int)legendaryGenes;
        if (data.TryGetValue("epic_genes_created", out var epicGenes))
            EpicGenesCreated = (int)epicGenes;
        if (data.TryGetValue("rare_genes_created", out var rareGenes))
            RareGenesCreated = (int)rareGenes;
    }
}

public class PetGene
{
    public string GeneId;
    public string GeneName;
    public string GeneType; // Physical/Magical/Support/Utility
    public string Rarity; // Common/Uncommon/Rare/Epic/Legendary
    public float StrengthBonus;
    public float VitalityBonus;
    public float AgilityBonus;
    public float IntelligenceBonus;
    public float LuckBonus;
    public string SpecialEffect; // DoubleGold/DoubleExp/FastAttack/Tank/Regen
    
    public PetGene(string id, string name, string type, string rarity)
    {
        GeneId = id;
        GeneName = name;
        GeneType = type;
        Rarity = rarity;
    }
}

public class GeneModificationRecord
{
    public string PetId;
    public string GeneId;
    public string ModificationType; // Add/Remove/Upgrade
    public int Timestamp;
    public bool Success;
}
