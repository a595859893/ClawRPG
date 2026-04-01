using Godot;
using System.Collections.Generic;

namespace ClawRPG.Systems.PetLifeCycle {
// DUPLICATE CLASS REMOVED
public partial class PetLifeCycleData : BaseSystem
{
    // 宠物生命周期数据
    public Dictionary<int, PetLifeCycleEntry> PetLifeCycles = new Dictionary<int, PetLifeCycleEntry>();

    // 统计数据
    public int TotalLifeCycles = 0;
    public int TotalDeaths = 0;
    public int TotalLifeExtensions = 0;
    public int LongestLifeSpan = 0; // 最长生命周期(游戏天数)

    // 历史记录
    public List<LifeCycleHistoryEntry> History = new List<LifeCycleHistoryEntry>();

    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, Variant>();

        // 保存宠物生命周期数据
        var cyclesData = new Dictionary<string, Dictionary<string, Variant>>();
        foreach (var kvp in PetLifeCycles)
        {
            cyclesData[kvp.Key.ToString()] = new Dictionary<string, Variant>
            {
                ["pet_id"] = kvp.Value.PetId,
                ["pet_name"] = kvp.Value.PetName ?? "",
                ["current_age"] = kvp.Value.CurrentAge,
                ["max_age"] = kvp.Value.MaxAge,
                ["current_stage"] = (int)kvp.Value.CurrentStage,
                ["is_immortal"] = kvp.Value.IsImmortal,
                ["life_extension_used"] = kvp.Value.LifeExtensionUsed,
                ["days_since_last_stage_change"] = kvp.Value.DaysSinceLastStageChange
            };
        }
        data["pet_lifecycles"] = cyclesData;

        // 保存统计数据
        data["total_lifecycles"] = TotalLifeCycles;
        data["total_deaths"] = TotalDeaths;
        data["total_life_extensions"] = TotalLifeExtensions;
        data["longest_lifespan"] = LongestLifeSpan;

        // 保存历史记录
        var historyList = new List<Dictionary<string, Variant>>();
        foreach (var entry in History)
        {
            historyList.Add(new Dictionary<string, Variant>
            {
                ["pet_id"] = entry.PetId,
                ["pet_name"] = entry.PetName ?? "",
                ["age_at_death"] = entry.AgeAtDeath,
                ["stage_at_death"] = (int)entry.StageAtDeath,
                ["was_extended"] = entry.WasExtended,
                ["life_extensions"] = entry.LifeExtensions,
                ["timestamp"] = entry.Timestamp
            });
        }
        data["history"] = historyList;

        return data;
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        // 加载宠物生命周期数据
        if (data.TryGetValue("pet_lifecycles", out var cyclesData))
        {
            PetLifeCycles = new Dictionary<int, PetLifeCycleEntry>();
            var cyclesDict = (Dictionary<string, Variant>)cyclesData;
            foreach (var kvp in cyclesDict)
            {
                if (int.TryParse(kvp.Key, out var petId))
                {
                    var entryData = (Dictionary<string, Variant>)kvp.Value;
                    var entry = new PetLifeCycleEntry();

                    if (entryData.TryGetValue("pet_id", out var id))
                        entry.PetId = (int)id;
                    if (entryData.TryGetValue("pet_name", out var name))
                        entry.PetName = (string)name;
                    if (entryData.TryGetValue("current_age", out var age))
                        entry.CurrentAge = (int)age;
                    if (entryData.TryGetValue("max_age", out var maxAge))
                        entry.MaxAge = (int)maxAge;
                    if (entryData.TryGetValue("current_stage", out var stage))
                        entry.CurrentStage = (LifeStage)(int)stage;
                    if (entryData.TryGetValue("is_immortal", out var isImmortal))
                        entry.IsImmortal = (bool)isImmortal;
                    if (entryData.TryGetValue("life_extension_used", out var extUsed))
                        entry.LifeExtensionUsed = (int)extUsed;
                    if (entryData.TryGetValue("days_since_last_stage_change", out var daysSince))
                        entry.DaysSinceLastStageChange = (int)daysSince;

                    PetLifeCycles[petId] = entry;
                }
            }
        }

        // 加载统计数据
        if (data.TryGetValue("total_lifecycles", out var totalCycles))
            TotalLifeCycles = (int)totalCycles;
        if (data.TryGetValue("total_deaths", out var totalDeaths))
            TotalDeaths = (int)totalDeaths;
        if (data.TryGetValue("total_life_extensions", out var totalExt))
            TotalLifeExtensions = (int)totalExt;
        if (data.TryGetValue("longest_lifespan", out var longestLife))
            LongestLifeSpan = (int)longestLife;

        // 加载历史记录
        if (data.TryGetValue("history", out var historyData))
        {
            History = new List<LifeCycleHistoryEntry>();
            var historyList = (List<Variant>)historyData;
            foreach (var entryVar in historyList)
            {
                var entryDict = (Dictionary<string, Variant>)entryVar;
                var entry = new LifeCycleHistoryEntry();

                if (entryDict.TryGetValue("pet_id", out var petId))
                    entry.PetId = (int)petId;
                if (entryDict.TryGetValue("pet_name", out var petName))
                    entry.PetName = (string)petName;
                if (entryDict.TryGetValue("age_at_death", out var ageAtDeath))
                    entry.AgeAtDeath = (int)ageAtDeath;
                if (entryDict.TryGetValue("stage_at_death", out var stageAtDeath))
                    entry.StageAtDeath = (LifeStage)(int)stageAtDeath;
                if (entryDict.TryGetValue("was_extended", out var wasExtended))
                    entry.WasExtended = (bool)wasExtended;
                if (entryDict.TryGetValue("life_extensions", out var lifeExtensions))
                    entry.LifeExtensions = (int)lifeExtensions;
                if (entryDict.TryGetValue("timestamp", out var timestamp))
                    entry.Timestamp = (long)timestamp;

                History.Add(entry);
            }
        }
    }
}
}
