using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems;

/// <summary>
/// 元素伤害加成管理器 - 管理玩家的元素伤害加成
/// </summary>
public class ElementalDamageManager : Node
{
    public static ElementalDamageManager Instance { get; private set; }
    
    // 基础元素伤害加成 (百分比)
    private Dictionary<ElementType, float> _baseElementalDamage = new();
    
    // 元素反应伤害加成 (百分比)
    private Dictionary<ElementType, float> _reactionDamageBonus = new();
    
    // 元素抗性 (百分比)
    private Dictionary<ElementType, float> _elementalResistance = new();
    
    // 元素穿透 (百分比)
    private float _elementalPenetration = 0f;
    
    public override void _Ready()
    {
        Instance = this;
        InitializeElementalValues();
    }
    
    private void InitializeElementalValues()
    {
        // 初始化所有元素的基础伤害加成为0
        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            if (element != ElementType.None)
            {
                _baseElementalDamage[element] = 0f;
                _reactionDamageBonus[element] = 0f;
                _elementalResistance[element] = 0f;
            }
        }
    }
    
    /// <summary>
    /// 设置元素伤害加成
    /// </summary>
    public void SetElementalDamage(ElementType element, float damagePercent)
    {
        if (element != ElementType.None)
        {
            _baseElementalDamage[element] = damagePercent;
        }
    }
    
    /// <summary>
    /// 添加元素伤害加成
    /// </summary>
    public void AddElementalDamage(ElementType element, float damagePercent)
    {
        if (element != ElementType.None)
        {
            _baseElementalDamage[element] = _baseElementalDamage.GetValueOrDefault(element, 0f) + damagePercent;
        }
    }
    
    /// <summary>
    /// 获取元素伤害加成
    /// </summary>
    public float GetElementalDamage(ElementType element)
    {
        return _baseElementalDamage.GetValueOrDefault(element, 0f);
    }
    
    /// <summary>
    /// 设置元素抗性
    /// </summary>
    public void SetElementalResistance(ElementType element, float resistancePercent)
    {
        if (element != ElementType.None)
        {
            _elementalResistance[element] = Mathf.Clamp(resistancePercent, 0f, 95f);
        }
    }
    
    /// <summary>
    /// 添加元素抗性
    /// </summary>
    public void AddElementalResistance(ElementType element, float resistancePercent)
    {
        if (element != ElementType.None)
        {
            float current = _elementalResistance.GetValueOrDefault(element, 0f);
            _elementalResistance[element] = Mathf.Clamp(current + resistancePercent, 0f, 95f);
        }
    }
    
    /// <summary>
    /// 获取元素抗性
    /// </summary>
    public float GetElementalResistance(ElementType element)
    {
        return _elementalResistance.GetValueOrDefault(element, 0f);
    }
    
    /// <summary>
    /// 计算最终元素伤害
    /// </summary>
    public float CalculateElementalDamage(float baseDamage, ElementType element)
    {
        float damageBonus = _baseElementalDamage.GetValueOrDefault(element, 0f);
        float resistance = _elementalResistance.GetValueOrDefault(element, 0f);
        
        // 应用穿透
        float effectiveResistance = Mathf.Max(0f, resistance - _elementalPenetration);
        
        // 伤害 = 基础伤害 * (1 + 伤害加成) * (1 - 抗性)
        float finalDamage = baseDamage * (1f + damageBonus / 100f) * (1f - effectiveResistance / 100f);
        
        return finalDamage;
    }
    
    /// <summary>
    /// 设置元素穿透
    /// </summary>
    public void SetElementalPenetration(float penetration)
    {
        _elementalPenetration = Mathf.Clamp(penetration, 0f, 100f);
    }
    
    /// <summary>
    /// 获取元素穿透
    /// </summary>
    public float GetElementalPenetration()
    {
        return _elementalPenetration;
    }
    
    /// <summary>
    /// 获取所有元素伤害加成信息
    /// </summary>
    public Dictionary<ElementType, float> GetAllElementalDamage()
    {
        return new Dictionary<ElementType, float>(_baseElementalDamage);
    }
    
    /// <summary>
    /// 获取所有元素抗性信息
    /// </summary>
    public Dictionary<ElementType, float> GetAllElementalResistance()
    {
        return new Dictionary<ElementType, float>(_elementalResistance);
    }
    
    /// <summary>
    /// 重置所有加成
    /// </summary>
    public void ResetAll()
    {
        InitializeElementalValues();
        _elementalPenetration = 0f;
    }
}
