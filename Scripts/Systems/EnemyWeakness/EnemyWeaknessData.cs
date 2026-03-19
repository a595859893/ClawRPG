using Godot;
using System;
using System.Collections.Generic;

public class EnemyWeaknessData : BaseSystem
{
    // 弱点类型
    public enum WeaknessType
    {
        Elemental,      // 元素弱点
        Physical,       // 物理弱点
        StatusEffect,  // 状态异常弱点
        CriticalSpot   // 部位破坏弱点
    }

    // 元素类型
    public enum ElementType
    {
        Fire,       // 火
        Ice,        // 冰
        Lightning,  // 雷
        Water,      // 水
        Holy,       // 圣
        Dark,       // 暗
        Physical,   // 物理
        Nature,     // 自然
        Wind        // 风
    }

    // 弱点数据结构
    [System.Serializable]
    public class Weakness
    {
        public WeaknessType Type;
        public ElementType Element;
        public float DamageMultiplier = 1.0f;      // 伤害倍率
        public float ResistanceMultiplier = 1.0f;   // 抗性倍率
        public string Description = "";
    }

    // 敌人弱点记录
    [System.Serializable]
    public class EnemyWeaknessRecord
    {
        public string EnemyType;
        public List<Weakness> Weaknesses = new List<Weakness>();
    }

    // 敌人弱点追踪
    public Dictionary<string, EnemyWeaknessRecord> EnemyWeaknesses = new Dictionary<string, EnemyWeaknessRecord>();

    // 弱点激活历史
    public List<WeaknessActivationRecord> ActivationHistory = new List<WeaknessActivationRecord>();

    // 弱点激活记录
    [System.Serializable]
    public class WeaknessActivationRecord
    {
        public string EnemyType;
        public WeaknessType WeaknessType;
        public ElementType Element;
        public float DamageBonus;
        public long Timestamp;
    }

    // 统计追踪
    public int TotalWeaknessActivations = 0;
    public int TotalBonusDamage = 0;
    public Dictionary<string, int> WeaknessTypeUsage = new Dictionary<string, int>();
    public Dictionary<string, int> ElementUsage = new Dictionary<string, int>();

    public override void _Ready()
    {
        LoadData();
    }

    public void LoadData()
    {
        if (FileAccess.FileExists("user://enemy_weakness_data.json"))
        {
            var file = FileAccess.Open("user://enemy_weakness_data.json", FileAccess.ModeFlags.Read);
            string content = file.GetAsText();
            file.Close();
            // 简单解析 - 这里应该用 JSON 解析库
        }
    }

    public void SaveData()
    {
        // 保存数据到 JSON
    }

    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary<string, Variant>();

        // 保存敌人弱点数据
        var enemyWeaknessesData = new Dictionary<string, Dictionary<string, Variant>>();
        foreach (var kvp in EnemyWeaknesses)
        {
            var weaknessesList = new List<Dictionary<string, Variant>>();
            foreach (var weakness in kvp.Value.Weaknesses)
            {
                weaknessesList.Add(new Dictionary<string, Variant>
                {
                    ["type"] = (int)weakness.Type,
                    ["element"] = (int)weakness.Element,
                    ["damage_multiplier"] = weakness.DamageMultiplier,
                    ["resistance_multiplier"] = weakness.ResistanceMultiplier,
                    ["description"] = weakness.Description ?? ""
                });
            }
            enemyWeaknessesData[kvp.Key] = new Dictionary<string, Variant>
            {
                ["enemy_type"] = kvp.Value.EnemyType ?? "",
                ["weaknesses"] = weaknessesList
            };
        }
        data["enemy_weaknesses"] = enemyWeaknessesData;

        // 保存激活历史
        var activationHistory = new List<Dictionary<string, Variant>>();
        foreach (var record in ActivationHistory)
        {
            activationHistory.Add(new Dictionary<string, Variant>
            {
                ["enemy_type"] = record.EnemyType ?? "",
                ["weakness_type"] = (int)record.WeaknessType,
                ["element"] = (int)record.Element,
                ["damage_bonus"] = record.DamageBonus,
                ["timestamp"] = record.Timestamp
            });
        }
        data["activation_history"] = activationHistory;

        // 保存统计
        data["total_weakness_activations"] = TotalWeaknessActivations;
        data["total_bonus_damage"] = TotalBonusDamage;

        // 保存弱点类型使用统计
        var weaknessTypeUsage = new Dictionary<string, int>();
        foreach (var kvp in WeaknessTypeUsage)
        {
            weaknessTypeUsage[kvp.Key] = kvp.Value;
        }
        data["weakness_type_usage"] = weaknessTypeUsage;

        // 保存元素使用统计
        var elementUsage = new Dictionary<string, int>();
        foreach (var kvp in ElementUsage)
        {
            elementUsage[kvp.Key] = kvp.Value;
        }
        data["element_usage"] = elementUsage;

        return data;
    }

    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;

        // 加载敌人弱点数据
        if (data.TryGetValue("enemy_weaknesses", out var enemyData))
        {
            EnemyWeaknesses = new Dictionary<string, EnemyWeaknessRecord>();
            var enemyDict = (Dictionary<string, Variant>)enemyData;
            foreach (var kvp in enemyDict)
            {
                var recordData = (Dictionary<string, Variant>)kvp.Value;
                var record = new EnemyWeaknessRecord { EnemyType = (string)recordData["enemy_type"] };

                if (recordData.TryGetValue("weaknesses", out var weaknessesData))
                {
                    record.Weaknesses = new List<Weakness>();
                    var weaknessesList = (List<Variant>)weaknessesData;
                    foreach (var wVar in weaknessesList)
                    {
                        var wDict = (Dictionary<string, Variant>)wVar;
                        var weakness = new Weakness();

                        if (wDict.TryGetValue("type", out var type))
                            weakness.Type = (WeaknessType)(int)type;
                        if (wDict.TryGetValue("element", out var element))
                            weakness.Element = (ElementType)(int)element;
                        if (wDict.TryGetValue("damage_multiplier", out var dmgMult))
                            weakness.DamageMultiplier = (float)dmgMult;
                        if (wDict.TryGetValue("resistance_multiplier", out var resMult))
                            weakness.ResistanceMultiplier = (float)resMult;
                        if (wDict.TryGetValue("description", out var desc))
                            weakness.Description = (string)desc;

                        record.Weaknesses.Add(weakness);
                    }
                }

                EnemyWeaknesses[kvp.Key] = record;
            }
        }

        // 加载激活历史
        if (data.TryGetValue("activation_history", out var historyData))
        {
            ActivationHistory = new List<WeaknessActivationRecord>();
            var historyList = (List<Variant>)historyData;
            foreach (var recordVar in historyList)
            {
                var recordDict = (Dictionary<string, Variant>)recordVar;
                var record = new WeaknessActivationRecord();

                if (recordDict.TryGetValue("enemy_type", out var enemyType))
                    record.EnemyType = (string)enemyType;
                if (recordDict.TryGetValue("weakness_type", out var weaknessType))
                    record.WeaknessType = (WeaknessType)(int)weaknessType;
                if (recordDict.TryGetValue("element", out var element))
                    record.Element = (ElementType)(int)element;
                if (recordDict.TryGetValue("damage_bonus", out var damageBonus))
                    record.DamageBonus = (float)damageBonus;
                if (recordDict.TryGetValue("timestamp", out var timestamp))
                    record.Timestamp = (long)timestamp;

                ActivationHistory.Add(record);
            }
        }

        // 加载统计
        if (data.TryGetValue("total_weakness_activations", out var totalActivations))
            TotalWeaknessActivations = (int)totalActivations;
        if (data.TryGetValue("total_bonus_damage", out var totalBonus))
            TotalBonusDamage = (float)totalBonus;

        // 加载弱点类型使用统计
        if (data.TryGetValue("weakness_type_usage", out var usageData))
        {
            WeaknessTypeUsage = new Dictionary<string, int>();
            var usageDict = (Dictionary<string, Variant>)usageData;
            foreach (var kvp in usageDict)
            {
                WeaknessTypeUsage[kvp.Key] = (int)kvp.Value;
            }
        }

        // 加载元素使用统计
        if (data.TryGetValue("element_usage", out var elemData))
        {
            ElementUsage = new Dictionary<string, int>();
            var elemDict = (Dictionary<string, Variant>)elemData;
            foreach (var kvp in elemDict)
            {
                ElementUsage[kvp.Key] = (int)kvp.Value;
            }
        }
    }
}
