using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts;
using ClawRPG.Scripts.Skills;
using ClawRPG.Systems;

namespace ClawRPG.Systems;

/// <summary>
/// 元素技能管理器 - 集成元素反应到技能系统
/// </summary>
public class ElementalSkillManager
{
    private static ElementalSkillManager _instance;
    public static ElementalSkillManager Instance => _instance ??= new ElementalSkillManager();
    
    private ElementalReactionManager _reactionManager;
    private ElementalDamageManager _damageManager;
    
    public ElementalSkillManager()
    {
        // 延迟初始化，等待其他系统就绪
    }
    
    /// <summary>
    /// 初始化管理器引用
    /// </summary>
    public void Initialize()
    {
        _reactionManager = ElementalReactionManager.Instance;
        _damageManager = ElementalDamageManager.Instance;
    }
    
    /// <summary>
    /// 检查技能是否有元素类型
    /// </summary>
    public bool HasElement(SkillData skill)
    {
        return skill != null && skill.Element != ElementType.None;
    }
    
    /// <summary>
    /// 获取技能的元素类型
    /// </summary>
    public ElementType GetSkillElement(SkillData skill)
    {
        return skill?.Element ?? ElementType.None;
    }
    
    /// <summary>
    /// 释放元素技能 - 应用元素附着和可能的元素反应
    /// </summary>
    /// <param name="caster">释放者</param>
    /// <param name="target">目标</param>
    /// <param name="skill">使用的技能</param>
    /// <param name="baseDamage">基础伤害</param>
    /// <returns>元素反应结果（如果有）</returns>
    public ElementalReactionResult CastElementalSkill(Node caster, Node target, SkillData skill, float baseDamage)
    {
        if (skill == null || target == null || skill.Element == ElementType.None)
        {
            return null;
        }
        
        // 计算元素伤害
        float elementalDamage = baseDamage;
        if (_damageManager != null)
        {
            elementalDamage = _damageManager.CalculateElementalDamage(baseDamage, skill.Element);
        }
        
        // 对目标应用元素附着
        if (_reactionManager != null)
        {
            return _reactionManager.ApplyElementalEffect(caster, target, skill.Element, elementalDamage, 5f, 1f);
        }
        
        return null;
    }
    
    /// <summary>
    /// 释放元素技能（无目标版本，用于AOE技能）
    /// </summary>
    public void CastElementalSkillAOE(Node caster, Vector2 position, float radius, SkillData skill, float baseDamage)
    {
        if (skill == null || skill.Element == ElementType.None)
        {
            return;
        }
        
        // 计算元素伤害
        float elementalDamage = baseDamage;
        if (_damageManager != null)
        {
            elementalDamage = _damageManager.CalculateElementalDamage(baseDamage, skill.Element);
        }
        
        // 对范围内的所有敌人应用元素附着
        if (_reactionManager != null && caster.HasMethod("GetTree"))
        {
            var enemies = GetEnemiesInArea(caster, position, radius);
            foreach (var enemy in enemies)
            {
                _reactionManager.ApplyElementalEffect(caster, enemy, skill.Element, elementalDamage, 5f, 1f);
            }
        }
    }
    
    /// <summary>
    /// 获取范围内的敌人
    /// </summary>
    private Array GetEnemiesInArea(Node caster, Vector2 position, float radius)
    {
        var enemies = new Array();
        
        if (caster.HasMethod("GetTree"))
        {
            var tree = caster.GetTree();
            if (tree != null)
            {
                var groups = tree.GetGroups();
                foreach (var group in groups)
                {
                    if (group is string groupName && groupName == "enemies")
                    {
                        var nodes = tree.GetNodesInGroup("enemies");
                        foreach (Node node in nodes)
                        {
                            if (node is Node2D node2D)
                            {
                                float dist = position.DistanceTo(node2D.GlobalPosition);
                                if (dist <= radius)
                                {
                                    enemies.Add(node);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        return enemies;
    }
    
    /// <summary>
    /// 获取元素的颜色表示（用于UI显示）
    /// </summary>
    public Color GetElementColor(ElementType element)
    {
        return element switch
        {
            ElementType.Fire => new Color(1f, 0.3f, 0.1f),      // 橙红色
            ElementType.Water => new Color(0.2f, 0.5f, 1f),     // 蓝色
            ElementType.Ice => new Color(0.7f, 0.9f, 1f),       // 浅蓝色
            ElementType.Thunder => new Color(0.9f, 0.9f, 0.2f), // 黄色
            ElementType.Wind => new Color(0.7f, 0.9f, 0.7f),    // 浅绿色
            ElementType.Earth => new Color(0.6f, 0.5f, 0.3f),  // 棕色
            ElementType.Light => new Color(1f, 1f, 0.8f),       // 亮白色
            ElementType.Dark => new Color(0.4f, 0.2f, 0.5f),    // 暗紫色
            _ => Colors.White
        };
    }
    
    /// <summary>
    /// 获取元素的图标路径
    /// </summary>
    public string GetElementIconPath(ElementType element)
    {
        return element switch
        {
            ElementType.Fire => "res://Icons/element_fire.png",
            ElementType.Water => "res://Icons/element_water.png",
            ElementType.Ice => "res://Icons/element_ice.png",
            ElementType.Thunder => "res://Icons/element_thunder.png",
            ElementType.Wind => "res://Icons/element_wind.png",
            ElementType.Earth => "res://Icons/element_earth.png",
            ElementType.Light => "res://Icons/element_light.png",
            ElementType.Dark => "res://Icons/element_dark.png",
            _ => ""
        };
    }
    
    /// <summary>
    /// 获取元素的显示名称
    /// </summary>
    public string GetElementName(ElementType element)
    {
        return element switch
        {
            ElementType.Fire => "火",
            ElementType.Water => "水",
            ElementType.Ice => "冰",
            ElementType.Thunder => "雷",
            ElementType.Wind => "风",
            ElementType.Earth => "土",
            ElementType.Light => "光",
            ElementType.Dark => "暗",
            _ => "无"
        };
    }
    
    /// <summary>
    /// 获取元素的描述
    /// </summary>
    public string GetElementDescription(ElementType element)
    {
        return element switch
        {
            ElementType.Fire => "造成火焰伤害，可与冰产生融化，与水产生蒸发",
            ElementType.Water => "造成水元素伤害，可与火产生蒸发，与冰产生冻结",
            ElementType.Ice => "冰冻敌人，可与火产生融化，与雷产生超导",
            ElementType.Thunder => "雷电伤害，可与冰产生超导，与火产生超载",
            ElementType.Wind => "风元素伤害，可扩散任意元素",
            ElementType.Earth => "土元素伤害，可与水/冰/雷/火产生结晶",
            ElementType.Light => "神圣伤害，对暗属性敌人有额外效果",
            ElementType.Dark => "暗影伤害，对光属性敌人有额外效果",
            _ => "无元素属性"
        };
    }
}

/// <summary>
/// 元素反应结果
/// </summary>
public class ElementalReactionResult
{
    public ElementalReaction Reaction { get; set; }
    public float Damage { get; set; }
    public float Duration { get; set; }
    public bool HasControlEffect { get; set; }
    public string ControlEffectName { get; set; } = "";
    public Node Source { get; set; }
    public Node Target { get; set; }
}
