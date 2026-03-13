using Godot;
using System;
using System.Collections.Generic;

public class PetGeneticsData : Node
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
