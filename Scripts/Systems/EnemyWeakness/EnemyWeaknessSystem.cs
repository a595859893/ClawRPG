using Godot;
using System;
using System.Collections.Generic;

public partial class EnemyWeaknessSystem : BaseSystem
{
    // 引用
    private EnemyWeaknessData _data;
    private EnemyWeaknessDatabase _database;

    // 当前战斗中的敌人弱点
    private Dictionary<int, List<EnemyWeaknessDatabase.WeaknessConfig>> _activeWeaknesses = new Dictionary<int, List<EnemyWeaknessDatabase.WeaknessConfig>>();

    // 信号

    public override void _Ready()
    {
        _data = GetNode<EnemyWeaknessData>("/root/EnemyWeaknessData");
        _database = GetNode<EnemyWeaknessDatabase>("/root/EnemyWeaknessDatabase");
    }

    /// <summary>
    /// 为敌人初始化弱点
    /// </summary>
    public void InitializeEnemyWeakness(int entityId, string enemyType)
    {
        var weaknesses = _database.GetEnemyWeaknesses(enemyType);
        if (weaknesses.Count > 0)
        {
            _activeWeaknesses[entityId] = weaknesses;
        }
    }

    /// <summary>
    /// 计算伤害加成
    /// </summary>
    public float CalculateDamageBonus(int entityId, EnemyWeaknessDatabase.ElementType element, EnemyWeaknessDatabase.WeaknessType weaknessType)
    {
        if (!_activeWeaknesses.ContainsKey(entityId))
            return 1.0f;

        float totalBonus = 1.0f;
        bool foundWeakness = false;

        foreach (var weakness in _activeWeaknesses[entityId])
        {
            if (weakness.Element == element || weakness.Type == weaknessType)
            {
                totalBonus *= weakness.DamageMultiplier;
                foundWeakness = true;

                // 记录弱点激活
                RecordWeaknessActivation(entityId, weakness);
            }
        }

        if (foundWeakness)
        {
            EmitSignal(nameof(WeaknessActivated), entityId, "", totalBonus);
        }

        return totalBonus;
    }

    /// <summary>
    /// 计算元素伤害
    /// </summary>
    public float CalculateElementalDamage(int entityId, float baseDamage, EnemyWeaknessDatabase.ElementType element)
    {
        return baseDamage * CalculateDamageBonus(entityId, element, EnemyWeaknessDatabase.WeaknessType.Elemental);
    }

    /// <summary>
    /// 计算物理伤害
    /// </summary>
    public float CalculatePhysicalDamage(int entityId, float baseDamage, string physicalType)
    {
        return baseDamage * CalculateDamageBonus(entityId, EnemyWeaknessDatabase.ElementType.Physical, EnemyWeaknessDatabase.WeaknessType.Physical);
    }

    /// <summary>
    /// 获取敌人弱点描述
    /// </summary>
    public List<string> GetEnemyWeaknessDescriptions(int entityId)
    {
        var descriptions = new List<string>();

        if (!_activeWeaknesses.ContainsKey(entityId))
            return descriptions;

        foreach (var weakness in _activeWeaknesses[entityId])
        {
            descriptions.Add(weakness.Description);
        }

        return descriptions;
    }

    /// <summary>
    /// 获取敌人抗性描述
    /// </summary>
    public List<string> GetEnemyResistanceDescriptions(int entityId, string enemyType)
    {
        var descriptions = new List<string>();
        var config = _database.GetEnemyWeaknessConfig(enemyType);

        if (config == null) return descriptions;

        foreach (var resistanceId in config.ResistanceIDs)
        {
            var resistance = _database.GetWeaknessConfig(resistanceId);
            if (resistance != null)
            {
                descriptions.Add(resistance.Description);
            }
        }

        return descriptions;
    }

    /// <summary>
    /// 获取敌人弱点提示
    /// </summary>
    public string GetWeaknessHint(string enemyType)
    {
        var config = _database.GetEnemyWeaknessConfig(enemyType);
        if (config != null)
            return config.CriticalSpotHint;
        return "";
    }

    /// <summary>
    /// 记录弱点激活
    /// </summary>
    private void RecordWeaknessActivation(int entityId, EnemyWeaknessDatabase.WeaknessConfig weakness)
    {
        if (_data == null) return;

        _data.TotalWeaknessActivations++;
        _data.TotalBonusDamage += (int)(weakness.DamageMultiplier * 100);

        string weaknessKey = weakness.Type.ToString();
        if (!_data.WeaknessTypeUsage.ContainsKey(weaknessKey))
            _data.WeaknessTypeUsage[weaknessKey] = 0;
        _data.WeaknessTypeUsage[weaknessKey]++;

        string elementKey = weakness.Element.ToString();
        if (!_data.ElementUsage.ContainsKey(elementKey))
            _data.ElementUsage[elementKey] = 0;
        _data.ElementUsage[elementKey]++;

        // 记录到历史
        var record = new EnemyWeaknessData.WeaknessActivationRecord
        {
            EnemyType = "",
            WeaknessType = weakness.Type,
            Element = weakness.Element,
            DamageBonus = weakness.DamageMultiplier,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        _data.ActivationHistory.Add(record);

        // 限制历史记录数量
        if (_data.ActivationHistory.Count > 100)
            _data.ActivationHistory.RemoveAt(0);
    }

    /// <summary>
    /// 清除敌人弱点
    /// </summary>
    public void ClearEnemyWeakness(int entityId)
    {
        if (_activeWeaknesses.ContainsKey(entityId))
        {
            _activeWeaknesses.Remove(entityId);
        }
    }

    /// <summary>
    /// 获取统计信息
    /// </summary>
    public Dictionary<string, int> GetStatistics()
    {
        var stats = new Dictionary<string, int>();

        if (_data != null)
        {
            stats["TotalActivations"] = _data.TotalWeaknessActivations;
            stats["TotalBonusDamage"] = _data.TotalBonusDamage;
        }

        return stats;
    }

    /// <summary>
    /// 测试弱点系统
    /// </summary>
    public void TestWeaknessSystem()
    {
        GD.Print("=== Enemy Weakness System Test ===");

        // 测试敌人弱点初始化
        InitializeEnemyWeakness(1, "FireElemental");
        InitializeEnemyWeakness(2, "IceElemental");
        InitializeEnemyWeakness(3, "Mechanical");

        // 测试伤害计算
        float damage1 = CalculateElementalDamage(1, 100f, EnemyWeaknessDatabase.ElementType.Ice);
        GD.Print($"FireElemental takes Ice damage: {damage1} (base: 100)");

        float damage2 = CalculateElementalDamage(2, 100f, EnemyWeaknessDatabase.ElementType.Fire);
        GD.Print($"IceElemental takes Fire damage: {damage2} (base: 100)");

        float damage3 = CalculateElementalDamage(3, 100f, EnemyWeaknessDatabase.ElementType.Lightning);
        GD.Print($"Mechanical takes Lightning damage: {damage3} (base: 100)");

        // 测试弱点描述
        var desc1 = GetEnemyWeaknessDescriptions(1);
        GD.Print($"FireElemental weaknesses: {string.Join(", ", desc1)}");

        var hint = GetWeaknessHint("FireElemental");
        GD.Print($"FireElemental hint: {hint}");

        // 获取统计
        var stats = GetStatistics();
        GD.Print($"Stats: {stats.Count} entries");
    }
    
    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // 敌人弱点追踪数据
        var enemyWeaknessesData = new Godot.Collections.Array();
        foreach (var kvp in _data.EnemyWeaknesses)
        {
            var enemyData = new Dictionary<string, object>();
            enemyData["enemy_type"] = kvp.Key;
            
            var weaknessesArray = new Godot.Collections.Array();
            foreach (var weakness in kvp.Value.Weaknesses)
            {
                weaknessesArray.Add(new Dictionary<string, object>
                {
                    { "type", (int)weakness.Type },
                    { "element", (int)weakness.Element },
                    { "damage_multiplier", weakness.DamageMultiplier },
                    { "resistance_multiplier", weakness.ResistanceMultiplier },
                    { "description", weakness.Description }
                });
            }
            enemyData["weaknesses"] = weaknessesArray;
            enemyWeaknessesData.Add(enemyData);
        }
        data["enemy_weaknesses"] = enemyWeaknessesData;
        
        // 弱点激活历史
        var historyArray = new Godot.Collections.Array();
        foreach (var record in _data.ActivationHistory)
        {
            historyArray.Add(new Dictionary<string, object>
            {
                { "enemy_type", record.EnemyType },
                { "weakness_type", (int)record.WeaknessType },
                { "element", (int)record.Element },
                { "damage_bonus", record.DamageBonus },
                { "timestamp", record.Timestamp }
            });
        }
        data["activation_history"] = historyArray;
        
        // 统计
        data["total_weakness_activations"] = _data.TotalWeaknessActivations;
        data["total_bonus_damage"] = _data.TotalBonusDamage;
        
        var weaknessTypeUsageData = new Dictionary<string, object>();
        foreach (var kvp in _data.WeaknessTypeUsage)
        {
            weaknessTypeUsageData[kvp.Key] = kvp.Value;
        }
        data["weakness_type_usage"] = weaknessTypeUsageData;
        
        var elementUsageData = new Dictionary<string, object>();
        foreach (var kvp in _data.ElementUsage)
        {
            elementUsageData[kvp.Key] = kvp.Value;
        }
        data["element_usage"] = elementUsageData;
        
        return data;
    }
    
    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        // 敌人弱点追踪数据
        _data.EnemyWeaknesses.Clear();
        if (data.Contains("enemy_weaknesses"))
        {
            var enemyWeaknessesData = (Array)data["enemy_weaknesses"];
            foreach (Dictionary enemyData in enemyWeaknessesData)
            {
                string enemyType = (string)enemyData["enemy_type"];
                var record = new EnemyWeaknessData.EnemyWeaknessRecord { EnemyType = enemyType };
                
                if (enemyData.Contains("weaknesses"))
                {
                    var weaknessesArray = (Array)enemyData["weaknesses"];
                    foreach (Dictionary weaknessData in weaknessesArray)
                    {
                        record.Weaknesses.Add(new EnemyWeaknessData.Weakness
                        {
                            Type = (EnemyWeaknessData.WeaknessType)(int)weaknessData["type"],
                            Element = (EnemyWeaknessData.ElementType)(int)weaknessData["element"],
                            DamageMultiplier = (float)weaknessData["damage_multiplier"],
                            ResistanceMultiplier = (float)weaknessData["resistance_multiplier"],
                            Description = (string)weaknessData["description"]
                        });
                    }
                }
                _data.EnemyWeaknesses[enemyType] = record;
            }
        }
        
        // 弱点激活历史
        _data.ActivationHistory.Clear();
        if (data.Contains("activation_history"))
        {
            var historyArray = (Array)data["activation_history"];
            foreach (Dictionary recordData in historyArray)
            {
                _data.ActivationHistory.Add(new EnemyWeaknessData.WeaknessActivationRecord
                {
                    EnemyType = (string)recordData["enemy_type"],
                    WeaknessType = (EnemyWeaknessData.WeaknessType)(int)recordData["weakness_type"],
                    Element = (EnemyWeaknessData.ElementType)(int)recordData["element"],
                    DamageBonus = (float)recordData["damage_bonus"],
                    Timestamp = (long)recordData["timestamp"]
                });
            }
        }
        
        // 统计
        if (data.Contains("total_weakness_activations")) _data.TotalWeaknessActivations = (int)data["total_weakness_activations"];
        if (data.Contains("total_bonus_damage")) _data.TotalBonusDamage = (int)data["total_bonus_damage"];
        
        _data.WeaknessTypeUsage.Clear();
        if (data.Contains("weakness_type_usage"))
        {
            var weaknessTypeUsageData = (Dictionary)data["weakness_type_usage"];
            foreach (var kvp in weaknessTypeUsageData)
            {
                _data.WeaknessTypeUsage[kvp.Key] = (int)kvp.Value;
            }
        }
        
        _data.ElementUsage.Clear();
        if (data.Contains("element_usage"))
        {
            var elementUsageData = (Dictionary)data["element_usage"];
            foreach (var kvp in elementUsageData)
            {
                _data.ElementUsage[kvp.Key] = (int)kvp.Value;
            }
        }
    }
}
