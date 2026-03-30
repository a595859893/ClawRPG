using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 装备强化系统 - 管理装备的强化操作和属性加成
/// 支持多种强化类型（攻击、防御、生命等），包含成功/暴击/失败机制
/// </summary>
public class EquipmentEnhancementSystem : BaseSystem
{
    private static EquipmentEnhancementSystem _instance;
    /// <summary>
    /// 获取系统单例实例
    /// </summary>
    public static EquipmentEnhancementSystem Instance
    {
        get
        {
            if (_instance == null) _instance = new EquipmentEnhancementSystem();
            return _instance;
        }
    }

    /// <summary>
    /// 玩家强化数据
    /// </summary>
    public EquipmentEnhancementData.PlayerEnhancementData PlayerData { get; private set; }

    // Signals
    /// <summary>
    /// 强化尝试结果信号 - 结果、强化等级、强化类型、加成值
    /// </summary>
    public Action<EquipmentEnhancementData.EnhancementResult, int, EquipmentEnhancementData.EnhancementType, int> OnEnhancementAttempt;
    /// <summary>
    /// 强化数据变更信号
    /// </summary>
    public Action OnEnhancementDataChanged;

    public EquipmentEnhancementSystem()
    {
        PlayerData = new EquipmentEnhancementData.PlayerEnhancementData();
    }

    /// <summary>
    /// 初始化强化系统
    /// </summary>
    public void Initialize()
    {
        GD.Print("[EquipmentEnhancementSystem] Initialized");
    }

    /// <summary>
    /// 检查是否可以进行强化
    /// </summary>
    /// <param name="type">强化类型</param>
    /// <param name="level">强化等级</param>
    /// <returns>是否可以强化</returns>
    public bool CanEnhance(EquipmentEnhancementData.EnhancementType type, int level)
    {
        var recipe = EquipmentEnhancementDatabase.Instance.GetRecipe(type, level);
        if (recipe == null) return false;

        // Check gold
        if (Player.Instance.Gold < recipe.GoldCost) return false;

        // Check materials
        for (int i = 0; i < recipe.MaterialIds.Count; i++)
        {
            int materialId = recipe.MaterialIds[i];
            int requiredCount = recipe.MaterialCounts[i];
            int playerCount = InventoryManager.Instance.GetItemCount(materialId);
            if (playerCount < requiredCount) return false;
        }

        return true;
    }

    /// <summary>
    /// 尝试进行装备强化
    /// </summary>
    /// <param name="type">强化类型</param>
    /// <param name="level">强化等级</param>
    /// <returns>强化结果</returns>
    public EquipmentEnhancementData.EnhancementResult TryEnhance(EquipmentEnhancementData.EnhancementType type, int level)
    {
        var recipe = EquipmentEnhancementDatabase.Instance.GetRecipe(type, level);
        if (recipe == null)
        {
            GD.PrintErr($"[EquipmentEnhancementSystem] Recipe not found for {type} level {level}");
            return EquipmentEnhancementData.EnhancementResult.Failure;
        }

        if (!CanEnhance(type, level))
        {
            GD.PrintErr("[EquipmentEnhancementSystem] Cannot enhance - insufficient resources");
            return EquipmentEnhancementData.EnhancementResult.Failure;
        }

        // Deduct gold
        Player.Instance.Gold -= recipe.GoldCost;

        // Deduct materials
        for (int i = 0; i < recipe.MaterialIds.Count; i++)
        {
            int materialId = recipe.MaterialIds[i];
            int requiredCount = recipe.MaterialCounts[i];
            InventoryManager.Instance.RemoveItem(materialId, requiredCount);
        }

        // Calculate result
        PlayerData.TotalEnhancements++;
        var random = new Random();
        int roll = random.Next(100);
        EquipmentEnhancementData.EnhancementResult result;

        if (roll < recipe.CriticalRate)
        {
            // Critical success
            result = EquipmentEnhancementData.EnhancementResult.CriticalSuccess;
            PlayerData.CriticalSuccesses++;
            GD.Print($"[EquipmentEnhancementSystem] Critical Success! {type} +{level}");
        }
        else if (roll < recipe.SuccessRate + recipe.CriticalRate)
        {
            // Normal success
            result = EquipmentEnhancementData.EnhancementResult.Success;
            PlayerData.SuccessfulEnhancements++;
            GD.Print($"[EquipmentEnhancementSystem] Success! {type} +{level}");
        }
        else if (roll < recipe.SuccessRate + recipe.CriticalRate + 10)
        {
            // Critical failure
            result = EquipmentEnhancementData.EnhancementResult.CriticalFailure;
            PlayerData.CriticalFailures++;
            GD.Print($"[EquipmentEnhancementSystem] Critical Failure! {type} +{level}");
        }
        else
        {
            // Normal failure
            result = EquipmentEnhancementData.EnhancementResult.Failure;
            PlayerData.FailedEnhancements++;
            GD.Print($"[EquipmentEnhancementSystem] Failure! {type} +{level}");
        }

        // Emit signal
        OnEnhancementAttempt?.Invoke(result, level, type, (int)GetEnhancementBonus(type, level));

        // Emit data changed signal
        OnEnhancementDataChanged?.Invoke();

        return result;
    }

    /// <summary>
    /// 获取指定强化等级的加成值
    /// </summary>
    /// <param name="type">强化类型</param>
    /// <param name="level">强化等级</param>
    /// <returns>加成百分比</returns>
    public float GetEnhancementBonus(EquipmentEnhancementData.EnhancementType type, int level)
    {
        return EquipmentEnhancementDatabase.Instance.GetEnhancementBonus(type, level);
    }

    /// <summary>
    /// 获取指定类型的总装备加成
    /// </summary>
    /// <param name="type">强化类型</param>
    /// <returns>总加成值</returns>
    public float GetTotalEquipmentBonus(EquipmentEnhancementData.EnhancementType type)
    {
        float total = 0f;
        if (PlayerData.EquipmentEnhancementLevels == null) return 0f;

        foreach (var kvp in PlayerData.EquipmentEnhancementLevels)
        {
            int level = kvp.Value;
            // This would need to map equipment ID to enhancement type
            // For now, we track enhancements separately
        }
        return total;
    }

    /// <summary>
    /// 获取玩家攻击加成
    /// </summary>
    /// <returns>攻击加成值</returns>
    public float GetPlayerAttackBonus()
    {
        if (PlayerData.EquipmentEnhancementLevels == null) return 0f;
        float total = 0f;
        foreach (var kvp in PlayerData.EquipmentEnhancementLevels)
        {
            total += GetEnhancementBonus(EquipmentEnhancementData.EnhancementType.Attack, kvp.Value);
        }
        return total;
    }

    /// <summary>
    /// 获取玩家防御加成
    /// </summary>
    /// <returns>防御加成值</returns>
    public float GetPlayerDefenseBonus()
    {
        if (PlayerData.EquipmentEnhancementLevels == null) return 0f;
        float total = 0f;
        foreach (var kvp in PlayerData.EquipmentEnhancementLevels)
        {
            total += GetEnhancementBonus(EquipmentEnhancementData.EnhancementType.Defense, kvp.Value);
        }
        return total;
    }

    /// <summary>
    /// 获取玩家生命加成
    /// </summary>
    /// <returns>生命加成值</returns>
    public float GetPlayerHealthBonus()
    {
        if (PlayerData.EquipmentEnhancementLevels == null) return 0f;
        float total = 0f;
        foreach (var kvp in PlayerData.EquipmentEnhancementLevels)
        {
            total += GetEnhancementBonus(EquipmentEnhancementData.EnhancementType.Health, kvp.Value);
        }
        return total;
    }

    /// <summary>
    /// 获取所有加成（用于UI显示）
    /// </summary>
    /// <returns>加成字典</returns>
    public Dictionary<string, float> GetAllBonuses()
    {
        var bonuses = new Dictionary<string, float>
        {
            { "Attack", GetPlayerAttackBonus() * 100f },
            { "Defense", GetPlayerDefenseBonus() * 100f },
            { "Health", GetPlayerHealthBonus() * 100f }
        };
        return bonuses;
    }

    /// <summary>
    /// 获取强化成功率
    /// </summary>
    /// <param name="type">强化类型</param>
    /// <param name="level">强化等级</param>
    /// <returns>成功率（百分比）</returns>
    public float GetSuccessRate(EquipmentEnhancementData.EnhancementType type, int level)
    {
        var recipe = EquipmentEnhancementDatabase.Instance.GetRecipe(type, level);
        return recipe != null ? recipe.SuccessRate : 0;
    }

    /// <summary>
    /// 获取暴击率
    /// </summary>
    /// <param name="type">强化类型</param>
    /// <param name="level">强化等级</param>
    /// <returns>暴击率（百分比）</returns>
    public float GetCriticalRate(EquipmentEnhancementData.EnhancementType type, int level)
    {
        var recipe = EquipmentEnhancementDatabase.Instance.GetRecipe(type, level);
        return recipe != null ? recipe.CriticalRate : 0;
    }

    /// <summary>
    /// 获取金币花费
    /// </summary>
    /// <param name="type">强化类型</param>
    /// <param name="level">强化等级</param>
    /// <returns>金币花费</returns>
    public int GetGoldCost(EquipmentEnhancementData.EnhancementType type, int level)
    {
        var recipe = EquipmentEnhancementDatabase.Instance.GetRecipe(type, level);
        return recipe != null ? recipe.GoldCost : 0;
    }

    /// <summary>
    /// 获取材料花费
    /// </summary>
    /// <param name="type">强化类型</param>
    /// <param name="level">强化等级</param>
    /// <returns>材料列表（物品ID，数量）</returns>
    public List<(int materialId, int count)> GetMaterialCost(EquipmentEnhancementData.EnhancementType type, int level)
    {
        var recipe = EquipmentEnhancementDatabase.Instance.GetRecipe(type, level);
        if (recipe == null) return new List<(int, int)>();

        var materials = new List<(int, int)>();
        for (int i = 0; i < recipe.MaterialIds.Count; i++)
        {
            materials.Add((recipe.MaterialIds[i], recipe.MaterialCounts[i]));
        }
        return materials;
    }

    /// <summary>
    /// 获取玩家强化数据
    /// </summary>
    /// <returns>玩家强化数据</returns>
    public EquipmentEnhancementData.PlayerEnhancementData GetPlayerData()
    {
        return PlayerData;
    }

    /// <summary>
    /// 获取强化统计信息
    /// </summary>
    /// <returns>统计字典</returns>
    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            { "TotalEnhancements", PlayerData.TotalEnhancements },
            { "SuccessfulEnhancements", PlayerData.SuccessfulEnhancements },
            { "FailedEnhancements", PlayerData.FailedEnhancements },
            { "CriticalSuccesses", PlayerData.CriticalSuccesses },
            { "CriticalFailures", PlayerData.CriticalFailures }
        };
    }

    /// <summary>
    /// 获取总体成功率
    /// </summary>
    /// <returns>成功率（百分比）</returns>
    public float GetSuccessRate()
    {
        if (PlayerData.TotalEnhancements == 0) return 0f;
        return (float)PlayerData.SuccessfulEnhancements / PlayerData.TotalEnhancements * 100f;
    }

    /// <summary>
    /// 保存强化数据
    /// </summary>
    /// <param name="node">数据节点</param>
    public void Save(DataNode node)
    {
        var enhancementNode = new DataNode("EquipmentEnhancement");
        enhancementNode.Set("TotalEnhancements", PlayerData.TotalEnhancements);
        enhancementNode.Set("SuccessfulEnhancements", PlayerData.SuccessfulEnhancements);
        enhancementNode.Set("FailedEnhancements", PlayerData.FailedEnhancements);
        enhancementNode.Set("CriticalSuccesses", PlayerData.CriticalSuccesses);
        enhancementNode.Set("CriticalFailures", PlayerData.CriticalFailures);

        // Save equipment enhancement levels
        if (PlayerData.EquipmentEnhancementLevels != null && PlayerData.EquipmentEnhancementLevels.Count > 0)
        {
            var levelsNode = new DataNode("EquipmentEnhancementLevels");
            foreach (var kvp in PlayerData.EquipmentEnhancementLevels)
            {
                levelsNode.Set(kvp.Key.ToString(), kvp.Value);
            }
            enhancementNode.AddChild(levelsNode);
        }

        node.AddChild(enhancementNode);
    }

    /// <summary>
    /// 加载强化数据
    /// </summary>
    /// <param name="node">数据节点</param>
    public void Load(DataNode node)
    {
        var enhancementNode = node.GetNode("EquipmentEnhancement");
        if (enhancementNode == null) return;

        PlayerData.TotalEnhancements = enhancementNode.Get("TotalEnhancements", 0);
        PlayerData.SuccessfulEnhancements = enhancementNode.Get("SuccessfulEnhancements", 0);
        PlayerData.FailedEnhancements = enhancementNode.Get("FailedEnhancements", 0);
        PlayerData.CriticalSuccesses = enhancementNode.Get("CriticalSuccesses", 0);
        PlayerData.CriticalFailures = enhancementNode.Get("CriticalFailures", 0);

        var levelsNode = enhancementNode.GetNode("EquipmentEnhancementLevels");
        if (levelsNode != null)
        {
            PlayerData.EquipmentEnhancementLevels = new Dictionary<int, int>();
            foreach (var key in levelsNode.GetKeys())
            {
                if (int.TryParse(key, out int equipId))
                {
                    PlayerData.EquipmentEnhancementLevels[equipId] = levelsNode.Get(key, 0);
                }
            }
        }

        GD.Print($"[EquipmentEnhancementSystem] Loaded {PlayerData.TotalEnhancements} enhancements");
    }

    public override Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary
        {
            { "total_enhancements", PlayerData.TotalEnhancements },
            { "successful_enhancements", PlayerData.SuccessfulEnhancements },
            { "failed_enhancements", PlayerData.FailedEnhancements },
            { "critical_successes", PlayerData.CriticalSuccesses },
            { "critical_failures", PlayerData.CriticalFailures },
            { "equipment_enhancement_levels", PlayerData.EquipmentEnhancementLevels }
        };
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        if (data.ContainsKey("total_enhancements"))
            PlayerData.TotalEnhancements = Convert.ToInt32(data["total_enhancements"]);
        if (data.ContainsKey("successful_enhancements"))
            PlayerData.SuccessfulEnhancements = Convert.ToInt32(data["successful_enhancements"]);
        if (data.ContainsKey("failed_enhancements"))
            PlayerData.FailedEnhancements = Convert.ToInt32(data["failed_enhancements"]);
        if (data.ContainsKey("critical_successes"))
            PlayerData.CriticalSuccesses = Convert.ToInt32(data["critical_successes"]);
        if (data.ContainsKey("critical_failures"))
            PlayerData.CriticalFailures = Convert.ToInt32(data["critical_failures"]);
        if (data.ContainsKey("equipment_enhancement_levels"))
        {
            var levelsData = data["equipment_enhancement_levels"] as Dictionary<object, object>;
            PlayerData.EquipmentEnhancementLevels = new Dictionary<int, int>();
            foreach (var kvp in levelsData)
            {
                PlayerData.EquipmentEnhancementLevels[Convert.ToInt32(kvp.Key)] = Convert.ToInt32(kvp.Value);
            }
        }
    }
}
