using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ProceduralWeaponGeneration {
    /// <summary>
    /// Data structure for procedural weapon generation system
    /// </summary>
    public class ProceduralWeaponData : BaseSystem {
        
        // Weapon generation history
        public List<WeaponGenerationRecord> GenerationHistory { get; set; } = new List<WeaponGenerationRecord>();
        
        // Statistics tracking
        public int TotalWeaponsGenerated { get; set; } = 0;
        public int LegendaryWeapons { get; set; } = 0;
        public int EpicWeapons { get; set; } = 0;
        public int RareWeapons { get; set; } = 0;
        public int TotalGoldSpent { get; set; } = 0;
        public int TotalMaterialsUsed { get; set; } = 0;
        
        // Unlocked weapon types
        public List<string> UnlockedWeaponTypes { get; set; } = new List<string>();
        
        // Unlocked prefixes
        public List<string> UnlockedPrefixes { get; set; } = new List<string>();
        
        // Unlocked suffixes
        public List<string> UnlockedSuffixes { get; set; } = new List<string>();
        
        // Total generation count per rarity
        public Dictionary<string, int> RarityGenerationCount { get; set; } = new Dictionary<string, int>();
        
        public ProceduralWeaponData() {
            // Initialize rarity counts
            RarityGenerationCount["Common"] = 0;
            RarityGenerationCount["Uncommon"] = 0;
            RarityGenerationCount["Rare"] = 0;
            RarityGenerationCount["Epic"] = 0;
            RarityGenerationCount["Legendary"] = 0;
        }
    }
    
    /// <summary>
    /// Record of a single weapon generation
    /// </summary>
    public class WeaponGenerationRecord {
        public string WeaponName { get; set; } = "";
        public string WeaponType { get; set; } = "";
        public string Rarity { get; set; } = "";
        public int Level { get; set; } = 1;
        public int Attack { get; set; } = 0;
        public int Defense { get; set; } = 0;
        public int Speed { get; set; } = 0;
        public List<string> SpecialEffects { get; set; } = new List<string>();
        public DateTime GenerationTime { get; set; } = DateTime.Now;
        public int GoldCost { get; set; } = 0;
        public bool IsReroll { get; set; } = false;
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        // 统计数据
        data["total_weapons_generated"] = TotalWeaponsGenerated;
        data["legendary_weapons"] = LegendaryWeapons;
        data["epic_weapons"] = EpicWeapons;
        data["rare_weapons"] = RareWeapons;
        data["total_gold_spent"] = TotalGoldSpent;
        data["total_materials_used"] = TotalMaterialsUsed;
        
        // 稀有度生成计数
        data["rarity_generation_count"] = new Dictionary(RarityGenerationCount);
        
        // 解锁的类型
        data["unlocked_weapon_types"] = new Array(UnlockedWeaponTypes);
        
        // 解锁的前缀
        data["unlocked_prefixes"] = new Array(UnlockedPrefixes);
        
        // 解锁的后缀
        data["unlocked_suffixes"] = new Array(UnlockedSuffixes);
        
        // 生成历史（只保存最近50条）
        var historyList = new Array();
        var recentHistory = GenerationHistory.Count > 50 ? 
            GenerationHistory.GetRange(GenerationHistory.Count - 50, 50) : 
            GenerationHistory;
        
        foreach (var record in recentHistory)
        {
            var recordDict = new Dictionary
            {
                { "weapon_name", record.WeaponName },
                { "weapon_type", record.WeaponType },
                { "rarity", record.Rarity },
                { "level", record.Level },
                { "attack", record.Attack },
                { "defense", record.Defense },
                { "speed", record.Speed },
                { "generation_time", record.GenerationTime.ToString("o") },
                { "gold_cost", record.GoldCost },
                { "is_reroll", record.IsReroll }
            };
            
            var effects = new Array();
            foreach (var effect in record.SpecialEffects)
            {
                effects.Add(effect);
            }
            recordDict["special_effects"] = effects;
            
            historyList.Add(recordDict);
        }
        data["generation_history"] = historyList;
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        // 统计数据
        TotalWeaponsGenerated = (int)data.GetValueOrDefault("total_weapons_generated", 0);
        LegendaryWeapons = (int)data.GetValueOrDefault("legendary_weapons", 0);
        EpicWeapons = (int)data.GetValueOrDefault("epic_weapons", 0);
        RareWeapons = (int)data.GetValueOrDefault("rare_weapons", 0);
        TotalGoldSpent = (int)data.GetValueOrDefault("total_gold_spent", 0);
        TotalMaterialsUsed = (int)data.GetValueOrDefault("total_materials_used", 0);
        
        // 稀有度生成计数
        if (data.Contains("rarity_generation_count"))
        {
            var rarityDict = (Dictionary)data["rarity_generation_count"];
            RarityGenerationCount = new Dictionary<string, int>();
            foreach (var kvp in rarityDict)
            {
                RarityGenerationCount[kvp.Key] = (int)kvp.Value;
            }
        }
        
        // 解锁的类型
        if (data.Contains("unlocked_weapon_types"))
        {
            var typesArray = (Array)data["unlocked_weapon_types"];
            UnlockedWeaponTypes = new List<string>();
            foreach (string type in typesArray)
            {
                UnlockedWeaponTypes.Add(type);
            }
        }
        
        // 解锁的前缀
        if (data.Contains("unlocked_prefixes"))
        {
            var prefixesArray = (Array)data["unlocked_prefixes"];
            UnlockedPrefixes = new List<string>();
            foreach (string prefix in prefixesArray)
            {
                UnlockedPrefixes.Add(prefix);
            }
        }
        
        // 解锁的后缀
        if (data.Contains("unlocked_suffixes"))
        {
            var suffixesArray = (Array)data["unlocked_suffixes"];
            UnlockedSuffixes = new List<string>();
            foreach (string suffix in suffixesArray)
            {
                UnlockedSuffixes.Add(suffix);
            }
        }
        
        // 生成历史
        GenerationHistory = new List<WeaponGenerationRecord>();
        if (data.Contains("generation_history"))
        {
            var historyArray = (Array)data["generation_history"];
            foreach (Dictionary recordDict in historyArray)
            {
                var record = new WeaponGenerationRecord
                {
                    WeaponName = (string)recordDict["weapon_name"],
                    WeaponType = (string)recordDict["weapon_type"],
                    Rarity = (string)recordDict["rarity"],
                    Level = (int)recordDict["level"],
                    Attack = (int)recordDict["attack"],
                    Defense = (int)recordDict["defense"],
                    Speed = (int)recordDict["speed"],
                    GoldCost = (int)recordDict["gold_cost"],
                    IsReroll = (bool)recordDict["is_reroll"]
                };
                
                if (recordDict.Contains("generation_time"))
                {
                    if (DateTime.TryParse(recordDict["generation_time"].ToString(), out var time))
                    {
                        record.GenerationTime = time;
                    }
                }
                
                if (recordDict.Contains("special_effects"))
                {
                    var effectsArray = (Array)recordDict["special_effects"];
                    record.SpecialEffects = new List<string>();
                    foreach (string effect in effectsArray)
                    {
                        record.SpecialEffects.Add(effect);
                    }
                }
                
                GenerationHistory.Add(record);
            }
        }
    }
}
