using Godot;
using System;
using System.Collections.Generic;

public class SkillSynergyData : BaseSystem
{
    // 技能协同记录
    public class SynergyRecord
    {
        public string SynergyId { get; set; }
        public string SynergyName { get; set; }
        public int TriggerCount { get; set; }
        public int MaxStacks { get; set; }
        public float Duration { get; set; }
        public float CurrentDuration { get; set; }
    }

    // 协同效果
    public class SynergyEffect
    {
        public string StatType { get; set; }  // attack/defense/health/speed/critical/etc
        public float Value { get; set; }
        public bool IsPercentage { get; set; }
    }

    // 技能协同数据
    public Dictionary<string, SynergyRecord> ActiveSynergies { get; set; } = new Dictionary<string, SynergyRecord>();
    public Dictionary<string, int> SynergyUnlockProgress { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> SynergyTriggerHistory { get; set; } = new Dictionary<string, int>();

    // 统计
    public int TotalSynergiesTriggered { get; set; }
    public int UniqueSynergiesDiscovered { get; set; }
    public int MaxComboChain { get; set; }
    public float TotalBonusDamage { get; set; }
    public float TotalBonusHealing { get; set; }

    public SkillSynergyData()
    {
        ActiveSynergies = new Dictionary<string, SynergyRecord>();
        SynergyUnlockProgress = new Dictionary<string, int>();
        SynergyTriggerHistory = new Dictionary<string, int>();
    }

    public Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>
        {
            { "active_synergies", SerializeSynergies() },
            { "synergy_unlock_progress", SynergyUnlockProgress },
            { "synergy_trigger_history", SynergyTriggerHistory },
            { "total_synergies_triggered", TotalSynergiesTriggered },
            { "unique_synergies_discovered", UniqueSynergiesDiscovered },
            { "max_combo_chain", MaxComboChain },
            { "total_bonus_damage", TotalBonusDamage },
            { "total_bonus_healing", TotalBonusHealing }
        };
    }

    private List<Dictionary<string, object>> SerializeSynergies()
    {
        var list = new List<Dictionary<string, object>>();
        foreach (var kvp in ActiveSynergies)
        {
            list.Add(new Dictionary<string, object>
            {
                { "synergy_id", kvp.Value.SynergyId },
                { "synergy_name", kvp.Value.SynergyName },
                { "trigger_count", kvp.Value.TriggerCount },
                { "max_stacks", kvp.Value.MaxStacks },
                { "duration", kvp.Value.Duration },
                { "current_duration", kvp.Value.CurrentDuration }
            });
        }
        return list;
    }

    public void Deserialize(Dictionary<string, object> data)
    {
        if (data.ContainsKey("active_synergies"))
        {
            var list = (List<object>)data["active_synergies"];
            foreach (var item in list)
            {
                var dict = (Dictionary<string, object>)item;
                var record = new SynergyRecord
                {
                    SynergyId = (string)dict["synergy_id"],
                    SynergyName = (string)dict["synergy_name"],
                    TriggerCount = (int)dict["trigger_count"],
                    MaxStacks = (int)dict["max_stacks"],
                    Duration = (float)dict["duration"],
                    CurrentDuration = (float)dict["current_duration"]
                };
                ActiveSynergies[record.SynergyId] = record;
            }
        }

        if (data.ContainsKey("synergy_unlock_progress"))
            SynergyUnlockProgress = new Dictionary<string, int>((Dictionary<string, object>)data["synergy_unlock_progress"]);

        if (data.ContainsKey("synergy_trigger_history"))
            SynergyTriggerHistory = new Dictionary<string, int>((Dictionary<string, object>)data["synergy_trigger_history"]);

        if (data.ContainsKey("total_synergies_triggered"))
            TotalSynergiesTriggered = (int)data["total_synergies_triggered"];

        if (data.ContainsKey("unique_synergies_discovered"))
            UniqueSynergiesDiscovered = (int)data["unique_synergies_discovered"];

        if (data.ContainsKey("max_combo_chain"))
            MaxComboChain = (int)data["max_combo_chain"];

        if (data.ContainsKey("total_bonus_damage"))
            TotalBonusDamage = (float)data["total_bonus_damage"];

        if (data.ContainsKey("total_bonus_healing"))
            TotalBonusHealing = (float)data["total_bonus_healing"];
    }

    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary<string, Variant>();

        // 保存激活的协同效果
        var synergiesList = new List<Dictionary<string, Variant>>();
        foreach (var kvp in ActiveSynergies)
        {
            synergiesList.Add(new Dictionary<string, Variant>
            {
                ["synergy_id"] = kvp.Value.SynergyId ?? "",
                ["synergy_name"] = kvp.Value.SynergyName ?? "",
                ["trigger_count"] = kvp.Value.TriggerCount,
                ["max_stacks"] = kvp.Value.MaxStacks,
                ["duration"] = kvp.Value.Duration,
                ["current_duration"] = kvp.Value.CurrentDuration
            });
        }
        data["active_synergies"] = synergiesList;

        // 保存解锁进度
        var unlockProgress = new Dictionary<string, int>();
        foreach (var kvp in SynergyUnlockProgress)
        {
            unlockProgress[kvp.Key] = kvp.Value;
        }
        data["synergy_unlock_progress"] = unlockProgress;

        // 保存触发历史
        var triggerHistory = new Dictionary<string, int>();
        foreach (var kvp in SynergyTriggerHistory)
        {
            triggerHistory[kvp.Key] = kvp.Value;
        }
        data["synergy_trigger_history"] = triggerHistory;

        // 保存统计数据
        data["total_synergies_triggered"] = TotalSynergiesTriggered;
        data["unique_synergies_discovered"] = UniqueSynergiesDiscovered;
        data["max_combo_chain"] = MaxComboChain;
        data["total_bonus_damage"] = TotalBonusDamage;
        data["total_bonus_healing"] = TotalBonusHealing;

        return data;
    }

    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;

        // 加载激活的协同效果
        if (data.TryGetValue("active_synergies", out var synergiesData))
        {
            ActiveSynergies = new Dictionary<string, SynergyRecord>();
            var synergiesList = (List<Variant>)synergiesData;
            foreach (var synergyVar in synergiesList)
            {
                var synergyDict = (Dictionary<string, Variant>)synergyVar;
                var record = new SynergyRecord();

                if (synergyDict.TryGetValue("synergy_id", out var synergyId))
                    record.SynergyId = (string)synergyId;
                if (synergyDict.TryGetValue("synergy_name", out var synergyName))
                    record.SynergyName = (string)synergyName;
                if (synergyDict.TryGetValue("trigger_count", out var triggerCount))
                    record.TriggerCount = (int)triggerCount;
                if (synergyDict.TryGetValue("max_stacks", out var maxStacks))
                    record.MaxStacks = (int)maxStacks;
                if (synergyDict.TryGetValue("duration", out var duration))
                    record.Duration = (float)duration;
                if (synergyDict.TryGetValue("current_duration", out var currentDuration))
                    record.CurrentDuration = (float)currentDuration;

                if (!string.IsNullOrEmpty(record.SynergyId))
                    ActiveSynergies[record.SynergyId] = record;
            }
        }

        // 加载解锁进度
        if (data.TryGetValue("synergy_unlock_progress", out var unlockData))
        {
            SynergyUnlockProgress = new Dictionary<string, int>();
            var unlockDict = (Dictionary<string, Variant>)unlockData;
            foreach (var kvp in unlockDict)
            {
                SynergyUnlockProgress[kvp.Key] = (int)kvp.Value;
            }
        }

        // 加载触发历史
        if (data.TryGetValue("synergy_trigger_history", out var historyData))
        {
            SynergyTriggerHistory = new Dictionary<string, int>();
            var historyDict = (Dictionary<string, Variant>)historyData;
            foreach (var kvp in historyDict)
            {
                SynergyTriggerHistory[kvp.Key] = (int)kvp.Value;
            }
        }

        // 加载统计数据
        if (data.TryGetValue("total_synergies_triggered", out var totalTriggered))
            TotalSynergiesTriggered = (int)totalTriggered;
        if (data.TryGetValue("unique_synergies_discovered", out var uniqueDiscovered))
            UniqueSynergiesDiscovered = (int)uniqueDiscovered;
        if (data.TryGetValue("max_combo_chain", out var maxCombo))
            MaxComboChain = (int)maxCombo;
        if (data.TryGetValue("total_bonus_damage", out var bonusDamage))
            TotalBonusDamage = (float)bonusDamage;
        if (data.TryGetValue("total_bonus_healing", out var bonusHealing))
            TotalBonusHealing = (float)bonusHealing;
    }
}
