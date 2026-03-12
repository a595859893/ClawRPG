using Godot;
using System;
using System.Collections.Generic;

public class EnemyWeaknessSystem : Node
{
    // 引用
    private EnemyWeaknessData _data;
    private EnemyWeaknessDatabase _database;

    // 当前战斗中的敌人弱点
    private Dictionary<int, List<EnemyWeaknessDatabase.WeaknessConfig>> _activeWeaknesses = new Dictionary<int, List<EnemyWeaknessDatabase.WeaknessConfig>>();

    // 信号
    [Signal]
    public void WeaknessActivated(int entityId, string weaknessId, float bonusDamage);

    [Signal]
    public void ResistanceTriggered(int entityId, string resistanceId, float reducedDamage);

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
}
