using Godot;
using System;
using System.Collections.Generic;

public partial class AlchemyLaboratorySystem : BaseSystem
{
    public static AlchemyLaboratorySystem Instance { get; private set; }

    // Laboratory state
    public bool IsUnlocked { get; set; } = false;
    public int LaboratoryLevel { get; set; } = 1;
    public int MaxLaboratoryLevel { get; set; } = 10;
    
    // Research progress
    public Dictionary<string, AlchemyResearch> Researches { get; set; } = new Dictionary<string, AlchemyResearch>();
    
    // Discovered formulas
    public List<string> DiscoveredFormulas { get; set; } = new List<string>();
    
    // Statistics
    public int TotalResearchesCompleted { get; set; } = 0;
    public int TotalFormulasDiscovered { get; set; } = 0;
    public int TotalGoldInvested { get; set; } = 0;

    // Research types
    public enum ResearchType
    {
        Extraction,      // Extract essence from materials
        Synthesis,       // Combine multiple materials
        Transmutation,   // Transform one material to another
        Enhancement,     // Enhance potion effects
        Discovery        // Discover new formulas
    }

    // Research class
    public class AlchemyResearch
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ResearchType Type { get; set; }
        public int Level { get; set; }
        public int Progress { get; set; }
        public int MaxProgress { get; set; }
        public int GoldCost { get; set; }
        public bool IsCompleted { get; set; }
        public string Reward { get; set; }
        public List<string> RequiredMaterials { get; set; } = new List<string>();
    }

    public override void _Ready()
    {
        Instance = this;
    }

    public void UnlockLaboratory()
    {
        IsUnlocked = true;
        GD.Print("Alchemy Laboratory unlocked!");
    }

    public void StartResearch(string researchId)
    {
        if (!Researches.ContainsKey(researchId))
        {
            GD.PrintErr("Research not found: " + researchId);
            return;
        }

        var research = Researches[researchId];
        if (research.IsCompleted)
        {
            GD.Print("Research already completed: " + research.Name);
            return;
        }

        // Check gold
        if (Player.Instance.Gold < research.GoldCost)
        {
            GD.Print("Not enough gold for research");
            return;
        }

        // Deduct gold
        Player.Instance.Gold -= research.GoldCost;
        TotalGoldInvested += research.GoldCost;

        // Start research (instant for simplicity)
        CompleteResearch(researchId);
    }

    public void CompleteResearch(string researchId)
    {
        if (!Researches.ContainsKey(researchId))
            return;

        var research = Researches[researchId];
        research.IsCompleted = true;
        research.Progress = research.MaxProgress;
        TotalResearchesCompleted++;

        // Add discovered formula
        if (!string.IsNullOrEmpty(research.Reward) && !DiscoveredFormulas.Contains(research.Reward))
        {
            DiscoveredFormulas.Add(research.Reward);
            TotalFormulasDiscovered++;
        }

        GD.Print("Research completed: " + research.Name);
    }

    public void LevelUpLaboratory()
    {
        if (LaboratoryLevel >= MaxLaboratoryLevel)
        {
            GD.Print("Laboratory already at max level");
            return;
        }

        int upgradeCost = LaboratoryLevel * 5000;
        if (Player.Instance.Gold < upgradeCost)
        {
            GD.Print("Not enough gold to upgrade laboratory");
            return;
        }

        Player.Instance.Gold -= upgradeCost;
        LaboratoryLevel++;
        TotalGoldInvested += upgradeCost;
        
        // Unlock new researches
        GenerateNewResearches();
        
        GD.Print("Laboratory upgraded to level " + LaboratoryLevel);
    }

    public void GenerateNewResearches()
    {
        // Generate researches based on level
        string[] extractionResearches = { "herb_extraction", "mineral_extraction", "monster_part_extraction" };
        string[] synthesisResearches = { "health_synthesis", "mana_synthesis", "buff_synthesis" };
        string[] transmutationResearches = { "gold_transmutation", "material_transmutation" };
        string[] enhancementResearches = { "potency_enhancement", "duration_enhancement", "effect_enhancement" };
        string[] discoveryResearches = { "rare_formula_discovery", "epic_formula_discovery", "legendary_formula_discovery" };

        var newResearches = new List<string[]>();
        
        if (LaboratoryLevel >= 1)
            newResearches.Add(new string[] { "extraction_1", "Extraction Basics", "Extraction", "1", "100", "health_essence" });
        if (LaboratoryLevel >= 2)
            newResearches.Add(new string[] { "synthesis_1", "Basic Synthesis", "Synthesis", "1", "200", "basic_health_potion" });
        if (LaboratoryLevel >= 3)
            newResearches.Add(new string[] { "transmutation_1", "Material Transmutation", "Transmutation", "2", "500", "iron_ingot" });
        if (LaboratoryLevel >= 4)
            newResearches.Add(new string[] { "enhancement_1", "Potion Enhancement", "Enhancement", "2", "800", "enhanced_health_potion" });
        if (LaboratoryLevel >= 5)
            newResearches.Add(new string[] { "discovery_1", "Rare Formula Discovery", "Discovery", "3", "1500", "rare_potion_formula" });
        if (LaboratoryLevel >= 6)
            newResearches.Add(new string[] { "extraction_2", "Advanced Extraction", "Extraction", "3", "2000", "mana_essence" });
        if (LaboratoryLevel >= 7)
            newResearches.Add(new string[] { "synthesis_2", "Advanced Synthesis", "Synthesis", "4", "3000", "advanced_buff_potion" });
        if (LaboratoryLevel >= 8)
            newResearches.Add(new string[] { "transmutation_2", "Gold Transmutation", "Transmutation", "4", "5000", "gemstone" });
        if (LaboratoryLevel >= 9)
            newResearches.Add(new string[] { "enhancement_2", "Master Enhancement", "Enhancement", "5", "8000", "master_potion" });
        if (LaboratoryLevel >= 10)
            newResearches.Add(new string[] { "discovery_2", "Legendary Discovery", "Discovery", "5", "10000", "legendary_potion_formula" });

        foreach (var researchData in newResearches)
        {
            string id = researchData[0];
            if (!Researches.ContainsKey(id))
            {
                var research = new AlchemyResearch
                {
                    Id = id,
                    Name = researchData[1],
                    Type = Enum.Parse<ResearchType>(researchData[2]),
                    Level = int.Parse(researchData[3]),
                    Progress = 0,
                    MaxProgress = 100,
                    GoldCost = int.Parse(researchData[4]),
                    Reward = researchData[5],
                    IsCompleted = false
                };
                Researches[id] = research;
            }
        }
    }

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        data["is_unlocked"] = IsUnlocked;
        data["laboratory_level"] = LaboratoryLevel;
        data["total_researches_completed"] = TotalResearchesCompleted;
        data["total_formulas_discovered"] = TotalFormulasDiscovered;
        data["total_gold_invested"] = TotalGoldInvested;
        
        var researchList = new List<Dictionary<string, object>>();
        foreach (var kvp in Researches)
        {
            var r = new Dictionary<string, object>();
            r["id"] = kvp.Value.Id;
            r["is_completed"] = kvp.Value.IsCompleted;
            researchList.Add(r);
        }
        data["researches"] = researchList;
        data["discovered_formulas"] = DiscoveredFormulas;
        
        return data;
    }

    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.Contains("is_unlocked")) IsUnlocked = Convert.ToBoolean(data["is_unlocked"]);
        if (data.Contains("laboratory_level")) LaboratoryLevel = Convert.ToInt32(data["laboratory_level"]);
        if (data.Contains("total_researches_completed")) TotalResearchesCompleted = Convert.ToInt32(data["total_researches_completed"]);
        if (data.Contains("total_formulas_discovered")) TotalFormulasDiscovered = Convert.ToInt32(data["total_formulas_discovered"]);
        if (data.Contains("total_gold_invested")) TotalGoldInvested = Convert.ToInt32(data["total_gold_invested"]);
        
        if (data.Contains("discovered_formulas"))
        {
            DiscoveredFormulas = new List<string>((System.Collections.Generic.IEnumerable<string>)data["discovered_formulas"]);
        }
        
        // Generate researches
        GenerateNewResearches();
        
        // Load completed researches
        if (data.Contains("researches"))
        {
            foreach (Dictionary r in (System.Collections.ArrayList)data["researches"])
            {
                string id = Convert.ToString(r["id"]);
                bool completed = Convert.ToBoolean(r["is_completed"]);
                if (Researches.ContainsKey(id))
                {
                    Researches[id].IsCompleted = completed;
                    if (completed)
                        Researches[id].Progress = Researches[id].MaxProgress;
                }
            }
        }
    }
    
    // 兼容性方法（保留旧名称供外部调用）
    public Dictionary<string, object> GetSaveData()
    {
        var data = new Dictionary<string, object>();
        data["is_unlocked"] = IsUnlocked;
        data["laboratory_level"] = LaboratoryLevel;
        data["total_researches_completed"] = TotalResearchesCompleted;
        data["total_formulas_discovered"] = TotalFormulasDiscovered;
        data["total_gold_invested"] = TotalGoldInvested;
        
        var researchList = new List<Dictionary<string, object>>();
        foreach (var kvp in Researches)
        {
            var r = new Dictionary<string, object>();
            r["id"] = kvp.Value.Id;
            r["is_completed"] = kvp.Value.IsCompleted;
            researchList.Add(r);
        }
        data["researches"] = researchList;
        data["discovered_formulas"] = DiscoveredFormulas;
        
        return data;
    }

    // 兼容性方法 - 调用 ImportSaveData
    public void LoadFromData(Dictionary<string, object> data)
    {
        ImportSaveData(new Dictionary(data));
    }
}
