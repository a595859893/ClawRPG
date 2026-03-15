using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 装备强化数据库 - 管理所有强化配方和加成计算
/// </summary>
public class EquipmentEnhancementDatabase
{
    private static EquipmentEnhancementDatabase _instance;
    /// <summary>
    /// 获取数据库单例实例
    /// </summary>
    public static EquipmentEnhancementDatabase Instance
    {
        get
        {
            if (_instance == null) _instance = new EquipmentEnhancementDatabase();
            return _instance;
        }
    }

    /// <summary>
    /// 所有强化配方列表
    /// </summary>
    public List<EquipmentEnhancementData.EnhancementRecipe> Recipes = new List<EquipmentEnhancementData.EnhancementRecipe>();

    public EquipmentEnhancementDatabase()
    {
        InitializeRecipes();
    }

    /// <summary>
    /// 初始化所有强化配方
    /// </summary>
    private void InitializeRecipes()
    {
        // Attack enhancements (10 levels)
        for (int i = 1; i <= 10; i++)
        {
            Recipes.Add(CreateRecipe(EquipmentEnhancementData.EnhancementType.Attack, i));
        }

        // Defense enhancements (10 levels)
        for (int i = 1; i <= 10; i++)
        {
            Recipes.Add(CreateRecipe(EquipmentEnhancementData.EnhancementType.Defense, i));
        }

        // Health enhancements (10 levels)
        for (int i = 1; i <= 10; i++)
        {
            Recipes.Add(CreateRecipe(EquipmentEnhancementData.EnhancementType.Health, i));
        }

        // Magic enhancements (10 levels)
        for (int i = 1; i <= 10; i++)
        {
            Recipes.Add(CreateRecipe(EquipmentEnhancementData.EnhancementType.Magic, i));
        }

        // Speed enhancements (10 levels)
        for (int i = 1; i <= 10; i++)
        {
            Recipes.Add(CreateRecipe(EquipmentEnhancementData.EnhancementType.Speed, i));
        }

        // Critical Rate enhancements (10 levels)
        for (int i = 1; i <= 10; i++)
        {
            Recipes.Add(CreateRecipe(EquipmentEnhancementData.EnhancementType.CriticalRate, i));
        }

        // Critical Damage enhancements (10 levels)
        for (int i = 1; i <= 10; i++)
        {
            Recipes.Add(CreateRecipe(EquipmentEnhancementData.EnhancementType.CriticalDamage, i));
        }

        // LifeSteal enhancements (10 levels)
        for (int i = 1; i <= 10; i++)
        {
            Recipes.Add(CreateRecipe(EquipmentEnhancementData.EnhancementType.LifeSteal, i));
        }

        // Dodge enhancements (10 levels)
        for (int i = 1; i <= 10; i++)
        {
            Recipes.Add(CreateRecipe(EquipmentEnhancementData.EnhancementType.Dodge, i));
        }

        // Resilience enhancements (10 levels)
        for (int i = 1; i <= 10; i++)
        {
            Recipes.Add(CreateRecipe(EquipmentEnhancementData.EnhancementType.Resilience, i));
        }
    }

    /// <summary>
    /// 创建强化配方
    /// </summary>
    /// <param name="type">强化类型</param>
    /// <param name="level">强化等级</param>
    /// <returns>强化配方</returns>
    private EquipmentEnhancementData.EnhancementRecipe CreateRecipe(EquipmentEnhancementData.EnhancementType type, int level)
    {
        var recipe = new EquipmentEnhancementData.EnhancementRecipe
        {
            Type = type,
            Level = level,
            SuccessRate = Math.Max(10, 100 - level * 8),
            CriticalRate = Math.Min(50, level * 3),
            GoldCost = level * level * 100
        };

        // Add material requirements based on level
        int materialId = GetMaterialIdForType(type);
        recipe.MaterialIds.Add(materialId);
        recipe.MaterialCounts.Add(level * 2);

        // Higher levels require additional materials
        if (level >= 5)
        {
            recipe.MaterialIds.Add(1001); // Enhancement stone
            recipe.MaterialCounts.Add(level - 3);
        }

        if (level >= 8)
        {
            recipe.MaterialIds.Add(1002); // Mythril
            recipe.MaterialCounts.Add(level - 6);
        }

        return recipe;
    }

    /// <summary>
    /// 获取强化类型对应的材料ID
    /// </summary>
    /// <param name="type">强化类型</param>
    /// <returns>材料ID</returns>
    private int GetMaterialIdForType(EquipmentEnhancementData.EnhancementType type)
    {
        switch (type)
        {
            case EquipmentEnhancementData.EnhancementType.Attack:
                return 1001; // Attack crystal
            case EquipmentEnhancementData.EnhancementType.Defense:
                return 1002; // Defense crystal
            case EquipmentEnhancementData.EnhancementType.Health:
                return 1003; // Health crystal
            case EquipmentEnhancementData.EnhancementType.Magic:
                return 1004; // Magic crystal
            case EquipmentEnhancementData.EnhancementType.Speed:
                return 1005; // Speed crystal
            case EquipmentEnhancementData.EnhancementType.CriticalRate:
                return 1006; // Critical crystal
            case EquipmentEnhancementData.EnhancementType.CriticalDamage:
                return 1007; // Damage crystal
            case EquipmentEnhancementData.EnhancementType.LifeSteal:
                return 1008; // Life steal crystal
            case EquipmentEnhancementData.EnhancementType.Dodge:
                return 1009; // Dodge crystal
            case EquipmentEnhancementData.EnhancementType.Resilience:
                return 1010; // Resilience crystal
            default:
                return 1001;
        }
    }

    /// <summary>
    /// 获取指定类型和等级的强化配方
    /// </summary>
    /// <param name="type">强化类型</param>
    /// <param name="level">强化等级</param>
    /// <returns>强化配方，不存在则返回null</returns>
    public EquipmentEnhancementData.EnhancementRecipe GetRecipe(EquipmentEnhancementData.EnhancementType type, int level)
    {
        foreach (var recipe in Recipes)
        {
            if (recipe.Type == type && recipe.Level == level)
                return recipe;
        }
        return null;
    }

    /// <summary>
    /// 获取强化加成值
    /// </summary>
    /// <param name="type">强化类型</param>
    /// <param name="level">强化等级</param>
    /// <returns>加成值（百分比）</returns>
    public float GetEnhancementBonus(EquipmentEnhancementData.EnhancementType type, int level)
    {
        float baseBonus = level * 0.05f; // 5% per level
        float multiplier = 1.0f + (level - 1) * 0.1f;
        return baseBonus * multiplier;
    }

    /// <summary>
    /// 获取强化类型的中文名称
    /// </summary>
    /// <param name="type">强化类型</param>
    /// <returns>类型名称</returns>
    public string GetEnhancementTypeName(EquipmentEnhancementData.EnhancementType type)
    {
        switch (type)
        {
            case EquipmentEnhancementData.EnhancementType.Attack:
                return "Attack";
            case EquipmentEnhancementData.EnhancementType.Defense:
                return "Defense";
            case EquipmentEnhancementData.EnhancementType.Health:
                return "Health";
            case EquipmentEnhancementData.EnhancementType.Magic:
                return "Magic";
            case EquipmentEnhancementData.EnhancementType.Speed:
                return "Speed";
            case EquipmentEnhancementData.EnhancementType.CriticalRate:
                return "Critical Rate";
            case EquipmentEnhancementData.EnhancementType.CriticalDamage:
                return "Critical Damage";
            case EquipmentEnhancementData.EnhancementType.LifeSteal:
                return "Life Steal";
            case EquipmentEnhancementData.EnhancementType.Dodge:
                return "Dodge";
            case EquipmentEnhancementData.EnhancementType.Resilience:
                return "Resilience";
            default:
                return "Unknown";
        }
    }

    /// <summary>
    /// 获取强化类型的描述
    /// </summary>
    /// <param name="type">强化类型</param>
    /// <returns>类型描述</returns>
    public string GetEnhancementTypeDescription(EquipmentEnhancementData.EnhancementType type)
    {
        switch (type)
        {
            case EquipmentEnhancementData.EnhancementType.Attack:
                return "Increases attack damage";
            case EquipmentEnhancementData.EnhancementType.Defense:
                return "Increases defense power";
            case EquipmentEnhancementData.EnhancementType.Health:
                return "Increases maximum health";
            case EquipmentEnhancementData.EnhancementType.Magic:
                return "Increases magic power";
            case EquipmentEnhancementData.EnhancementType.Speed:
                return "Increases movement speed";
            case EquipmentEnhancementData.EnhancementType.CriticalRate:
                return "Increases critical hit chance";
            case EquipmentEnhancementData.EnhancementType.CriticalDamage:
                return "Increases critical hit damage";
            case EquipmentEnhancementData.EnhancementType.LifeSteal:
                return "Increases life steal percentage";
            case EquipmentEnhancementData.EnhancementType.Dodge:
                return "Increases dodge chance";
            case EquipmentEnhancementData.EnhancementType.Resilience:
                return "Increases damage reduction";
            default:
                return "Unknown enhancement";
        }
    }

    /// <summary>
    /// 获取强化类型的颜色（用于UI显示）
    /// </summary>
    /// <param name="type">强化类型</param>
    /// <returns>颜色值</returns>
    public Godot.Color GetEnhancementTypeColor(EquipmentEnhancementData.EnhancementType type)
    {
        switch (type)
        {
            case EquipmentEnhancementData.EnhancementType.Attack:
                return new Godot.Color(1f, 0.3f, 0.3f); // Red
            case EquipmentEnhancementData.EnhancementType.Defense:
                return new Godot.Color(0.3f, 0.3f, 1f); // Blue
            case EquipmentEnhancementData.EnhancementType.Health:
                return new Godot.Color(0.3f, 1f, 0.3f); // Green
            case EquipmentEnhancementData.EnhancementType.Magic:
                return new Godot.Color(0.6f, 0.3f, 1f); // Purple
            case EquipmentEnhancementData.EnhancementType.Speed:
                return new Godot.Color(1f, 1f, 0.3f); // Yellow
            case EquipmentEnhancementData.EnhancementType.CriticalRate:
                return new Godot.Color(1f, 0.6f, 0f); // Orange
            case EquipmentEnhancementData.EnhancementType.CriticalDamage:
                return new Godot.Color(1f, 0f, 0.5f); // Pink
            case EquipmentEnhancementData.EnhancementType.LifeSteal:
                return new Godot.Color(0.8f, 0.2f, 0.2f); // Dark Red
            case EquipmentEnhancementData.EnhancementType.Dodge:
                return new Godot.Color(0.3f, 0.8f, 0.8f); // Cyan
            case EquipmentEnhancementData.EnhancementType.Resilience:
                return new Godot.Color(0.5f, 0.5f, 0.5f); // Gray
            default:
                return new Godot.Color(1f, 1f, 1f);
        }
    }
}
