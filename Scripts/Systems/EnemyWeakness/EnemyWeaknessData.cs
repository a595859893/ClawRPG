using Godot;
using System;
using System.Collections.Generic;

public class EnemyWeaknessData : Node
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
}
