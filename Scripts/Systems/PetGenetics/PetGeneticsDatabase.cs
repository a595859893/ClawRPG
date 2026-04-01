using Godot;
using System;
using System.Collections.Generic;

public partial class PetGeneticsDatabase : BaseSystem
{
    // Gene templates by type
    public Dictionary<string, List<GeneTemplate>> GeneTemplates = new Dictionary<string, List<GeneTemplate>>();
    
    // Rarity weights for gene generation
    public Dictionary<string, float> RarityWeights = new Dictionary<string, float>
    {
        {"Common", 50.0f},
        {"Uncommon", 30.0f},
        {"Rare", 15.0f},
        {"Epic", 4.0f},
        {"Legendary", 1.0f}
    };
    
    // Gene type categories
    public string[] GeneTypes = { "Physical", "Magical", "Support", "Utility" };
    
    public override void _Ready()
    {
        InitializeGeneTemplates();
    }
    
    private void InitializeGeneTemplates()
    {
        // Physical genes
        GeneTemplates["Physical"] = new List<GeneTemplate>
        {
            new GeneTemplate("gene_muscle", "Muscle Gene", "Physical", "Common", 0.1f, 0.05f, 0.0f, 0.0f, 0.0f, ""),
            new GeneTemplate("gene_power", "Power Gene", "Physical", "Uncommon", 0.15f, 0.08f, 0.05f, 0.0f, 0.0f, "FastAttack"),
            new GeneTemplate("gene_titan", "Titan Gene", "Physical", "Rare", 0.25f, 0.15f, 0.1f, 0.0f, 0.05f, "Tank"),
            new GeneTemplate("gene_beast", "Beast Gene", "Physical", "Epic", 0.35f, 0.2f, 0.15f, 0.05f, 0.1f, "Lifesteal"),
            new GeneTemplate("gene_atlas", "Atlas Gene", "Physical", "Legendary", 0.5f, 0.3f, 0.2f, 0.1f, 0.15f, "TitanForm")
        };
        
        // Magical genes
        GeneTemplates["Magical"] = new List<GeneTemplate>
        {
            new GeneTemplate("gene_mana", "Mana Gene", "Magical", "Common", 0.0f, 0.0f, 0.0f, 0.1f, 0.05f, ""),
            new GeneTemplate("gene_arcane", "Arcane Gene", "Magical", "Uncommon", 0.05f, 0.05f, 0.0f, 0.15f, 0.08f, "MagicBoost"),
            new GeneTemplate("gene_elemental", "Elemental Gene", "Magical", "Rare", 0.1f, 0.1f, 0.05f, 0.25f, 0.1f, "ElementalMastery"),
            new GeneTemplate("gene_celestial", "Celestial Gene", "Magical", "Epic", 0.15f, 0.15f, 0.1f, 0.35f, 0.15f, "MagicSurge"),
            new GeneTemplate("gene_cosmic", "Cosmic Gene", "Magical", "Legendary", 0.2f, 0.2f, 0.15f, 0.5f, 0.25f, "CosmicPower")
        };
        
        // Support genes
        GeneTemplates["Support"] = new List<GeneTemplate>
        {
            new GeneTemplate("gene_heal", "Healing Gene", "Support", "Common", 0.0f, 0.05f, 0.0f, 0.05f, 0.0f, "Regen"),
            new GeneTemplate("gene_guard", "Guard Gene", "Support", "Uncommon", 0.05f, 0.1f, 0.05f, 0.0f, 0.05f, "Shield"),
            new GeneTemplate("gene_aura", "Aura Gene", "Support", "Rare", 0.1f, 0.15f, 0.1f, 0.1f, 0.1f, "AuraBuff"),
            new GeneTemplate("gene_spirit", "Spirit Gene", "Support", "Epic", 0.15f, 0.2f, 0.15f, 0.15f, 0.15f, "SpiritBond"),
            new GeneTemplate("gene_divine", "Divine Gene", "Support", "Legendary", 0.2f, 0.3f, 0.2f, 0.2f, 0.2f, "DivineGrace")
        };
        
        // Utility genes
        GeneTemplates["Utility"] = new List<GeneTemplate>
        {
            new GeneTemplate("gene_lucky", "Lucky Gene", "Utility", "Common", 0.0f, 0.0f, 0.05f, 0.0f, 0.1f, ""),
            new GeneTemplate("gene_fortune", "Fortune Gene", "Utility", "Uncommon", 0.05f, 0.05f, 0.08f, 0.05f, 0.15f, "DoubleGold"),
            new GeneTemplate("gene_wisdom", "Wisdom Gene", "Utility", "Rare", 0.08f, 0.08f, 0.1f, 0.1f, 0.2f, "DoubleExp"),
            new GeneTemplate("gene_phantom", "Phantom Gene", "Utility", "Epic", 0.1f, 0.1f, 0.15f, 0.15f, 0.25f, "RareDrop"),
            new GeneTemplate("gene_chaos", "Chaos Gene", "Utility", "Legendary", 0.15f, 0.15f, 0.2f, 0.2f, 0.35f, "ChaosBlessing")
        };
    }
    
    public GeneTemplate GetRandomGeneTemplate(string geneType = "")
    {
        string type = geneType;
        if (string.IsNullOrEmpty(type))
        {
            type = GeneTypes[GD.Randi() % GeneTypes.Length];
        }
        
        if (!GeneTemplates.ContainsKey(type))
            type = "Physical";
        
        var templates = GeneTemplates[type];
        return templates[GD.Randi() % templates.Count];
    }
    
    public string GetRandomRarity()
    {
        float totalWeight = 0;
        foreach (var weight in RarityWeights.Values)
            totalWeight += weight;
        
        float random = (float)GD.Randf() * totalWeight;
        float cumulative = 0;
        
        foreach (var kvp in RarityWeights)
        {
            cumulative += kvp.Value;
            if (random <= cumulative)
                return kvp.Key;
        }
        
        return "Common";
    }
    
    public PetGene GenerateRandomGene(string geneType = "")
    {
        var template = GetRandomGeneTemplate(geneType);
        var rarity = GetRandomRarity();
        
        // Adjust bonuses based on actual rarity
        float rarityMultiplier = GetRarityMultiplier(rarity);
        
        var gene = new PetGene(
            template.GeneId + "_" + GD.Randi(),
            template.GeneName,
            template.GeneType,
            rarity
        );
        
        gene.StrengthBonus = template.BaseStrength * rarityMultiplier;
        gene.VitalityBonus = template.BaseVitality * rarityMultiplier;
        gene.AgilityBonus = template.BaseAgility * rarityMultiplier;
        gene.IntelligenceBonus = template.BaseIntelligence * rarityMultiplier;
        gene.LuckBonus = template.BaseLuck * rarityMultiplier;
        gene.SpecialEffect = template.SpecialEffect;
        
        return gene;
    }
    
    private float GetRarityMultiplier(string rarity)
    {
        switch (rarity)
        {
            case "Common": return 1.0f;
            case "Uncommon": return 1.5f;
            case "Rare": return 2.0f;
            case "Epic": return 3.0f;
            case "Legendary": return 5.0f;
            default: return 1.0f;
        }
    }
}

public class GeneTemplate
{
    public string GeneId;
    public string GeneName;
    public string GeneType;
    public string BaseRarity;
    public float BaseStrength;
    public float BaseVitality;
    public float BaseAgility;
    public float BaseIntelligence;
    public float BaseLuck;
    public string SpecialEffect;
    
    public GeneTemplate(string id, string name, string type, string rarity,
        float strength, float vitality, float agility, float intelligence, float luck, string effect)
    {
        GeneId = id;
        GeneName = name;
        GeneType = type;
        BaseRarity = rarity;
        BaseStrength = strength;
        BaseVitality = vitality;
        BaseAgility = agility;
        BaseIntelligence = intelligence;
        BaseLuck = luck;
        SpecialEffect = effect;
    }

        public Dictionary<string, object> ExportSaveData() => new();
        public void ImportSaveData(Dictionary<string, object> data) { }
}
