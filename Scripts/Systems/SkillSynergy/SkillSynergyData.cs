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

    public override Dictionary ExportSaveData() => new();
    public override void ImportSaveData(Dictionary data) { }

}
