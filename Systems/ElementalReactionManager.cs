using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems;

/// <summary>
/// 元素类型枚举
/// </summary>
public enum ElementType
{
    None = 0,
    Fire = 1,    // 火
    Water = 2,  // 水
    Ice = 3,    // 冰
    Thunder = 4, // 雷
    Wind = 5,   // 风
    Earth = 6,  // 土
    Light = 7,  // 光
    Dark = 8    // 暗
}

/// <summary>
/// 元素反应类型
/// </summary>
public enum ElementalReaction
{
    None = 0,
    Freeze = 1,        // 冻结 - 冰+水
    Melt = 2,          // 融化 - 火+冰
    Vaporize = 3,      // 蒸发 - 火+水
    Superconduct = 4,  // 超导 - 冰+雷
    Overload = 5,      // 超载 - 火+雷
    ElectroCharged = 6, // 感电 - 水+雷
    Swirl = 7,         // 扩散 - 风+任意
    Crystallize = 8,   // 结晶 - 土+水/冰/雷/火
    Burning = 9,       // 燃烧 - 火+风
    Bloom = 10,        // 绽放 - 水+火
    Quicken = 11,      // 激化 - 雷+草(如有)/雷+雷
    Shatter = 12       // 碎冰 - 冰+物理
}

/// <summary>
/// 元素附着数据
/// </summary>
public class ElementalAura
{
    public ElementType Element { get; set; }
    public float Duration { get; set; }
    public float Intensity { get; set; }
    public Node Source { get; set; }
    
    public ElementalAura(ElementType element, float duration, float intensity = 1.0f, Node source = null)
    {
        Element = element;
        Duration = duration;
        Intensity = intensity;
        Source = source;
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
    public string Description { get; set; }
    public bool ApplyControlEffect { get; set; }
    public float ControlDuration { get; set; }
    
    public ElementalReactionResult()
    {
        Reaction = ElementalReaction.None;
        Damage = 0;
        Duration = 0;
        Description = "";
        ApplyControlEffect = false;
        ControlDuration = 0;
    }
}

/// <summary>
/// 元素反应管理器 - 管理元素反应系统
/// </summary>
public class ElementalReactionManager : Node
{
    public static ElementalReactionManager Instance { get; private set; }
    
    // 元素反应配置
    private Dictionary<ElementalReaction, float> _reactionBaseDamage = new();
    private Dictionary<ElementalReaction, float> _reactionControlDuration = new();
    private Dictionary<ElementalReaction, string> _reactionDescriptions = new();
    
    // 玩家/敌人元素附着
    private Dictionary<Node, List<ElementalAura>> _playerAuras = new();
    private Dictionary<Node, List<ElementalAura>> _enemyAuras = new();
    
    // 冷却防止反应过于频繁
    private Dictionary<Node2D, float> _reactionCooldowns = new();
    private float _reactionCooldownTime = 0.5f;
    
    public override void _Ready()
    {
        Instance = this;
        InitializeReactionData();
    }
    
    private void InitializeReactionData()
    {
        // 基础伤害倍率 (相对于元素伤害)
        _reactionBaseDamage[ElementalReaction.Freeze] = 1.5f;
        _reactionBaseDamage[ElementalReaction.Melt] = 2.0f;
        _reactionBaseDamage[ElementalReaction.Vaporize] = 2.0f;
        _reactionBaseDamage[ElementalReaction.Superconduct] = 1.5f;
        _reactionBaseDamage[ElementalReaction.Overload] = 1.8f;
        _reactionBaseDamage[ElementalReaction.ElectroCharged] = 1.2f;
        _reactionBaseDamage[ElementalReaction.Swirl] = 1.2f;
        _reactionBaseDamage[ElementalReaction.Crystallize] = 1.5f;
        _reactionBaseDamage[ElementalReaction.Burning] = 1.5f;
        _reactionBaseDamage[ElementalReaction.Bloom] = 1.5f;
        _reactionBaseDamage[ElementalReaction.Quick] = 1.8f;
        _reactionBaseDamage[ElementalReaction.Shatter] = 2.0f;
        
        // 控制效果持续时间
        _reactionControlDuration[ElementalReaction.Freeze] = 2.5f;
        _reactionControlDuration[ElementalReaction.Overload] = 0.8f;
        _reactionControlDuration[ElementalReaction.ElectroCharged] = 1.0f;
        
        // 反应描述
        _reactionDescriptions[ElementalReaction.Freeze] = "冻结";
        _reactionDescriptions[ElementalReaction.Melt] = "融化";
        _reactionDescriptions[ElementalReaction.Vaporize] = "蒸发";
        _reactionDescriptions[ElementalReaction.Superconduct] = "超导";
        _reactionDescriptions[ElementalReaction.Overload] = "超载";
        _reactionDescriptions[ElementalReaction.ElectroCharged] = "感电";
        _reactionDescriptions[ElementalReaction.Swirl] = "扩散";
        _reactionDescriptions[ElementalReaction.Crystallize] = "结晶";
        _reactionDescriptions[ElementalReaction.Burning] = "燃烧";
        _reactionDescriptions[ElementalReaction.Bloom] = "绽放";
        _reactionDescriptions[ElementalReaction.Quicken] = "激化";
        _reactionDescriptions[ElementalReaction.Shatter] = "碎冰";
    }
    
    /// <summary>
    /// 触发元素伤害并检查反应
    /// </summary>
    public ElementalReactionResult ApplyElementalDamage(
        Node target, 
        ElementType element, 
        float baseDamage, 
        Node source = null)
    {
        var result = new ElementalReactionResult();
        
        // 获取目标当前的元素附着
        var auras = target is Player ? GetPlayerAuras(target) : GetEnemyAuras(target);
        
        // 检查是否可以触发反应
        if (auras.Count > 0)
        {
            var reaction = CheckReaction(element, auras[0].Element);
            if (reaction != ElementalReaction.None)
            {
                // 计算反应伤害
                float damageMultiplier = _reactionBaseDamage.GetValueOrDefault(reaction, 1.5f);
                result.Damage = baseDamage * damageMultiplier;
                result.Reaction = reaction;
                result.Duration = _reactionControlDuration.GetValueOrDefault(reaction, 0);
                result.Description = _reactionDescriptions.GetValueOrDefault(reaction, "");
                
                // 检查是否有控制效果
                if (_reactionControlDuration.ContainsKey(reaction))
                {
                    result.ApplyControlEffect = true;
                    result.ControlDuration = _reactionControlDuration[reaction];
                }
                
                // 清除被反应的元素附着
                if (auras.Count > 0)
                {
                    RemoveAura(target, auras[0]);
                }
                
                // 触发反应视觉效果
                TriggerReactionEffect(target, reaction);
                
                GD.Print($"[Elemental] {reaction} triggered! Damage: {result.Damage}");
            }
        }
        
        // 添加元素附着
        if (result.Reaction == ElementalReaction.None)
        {
            AddAura(target, new ElementalAura(element, 8.0f, 1.0f, source));
        }
        
        return result;
    }
    
    /// <summary>
    /// 检查元素反应
    /// </summary>
    private ElementalReaction CheckReaction(ElementType attacker, ElementType defender)
    {
        // 冻结: 冰 + 水
        if ((attacker == ElementType.Ice && defender == ElementType.Water) ||
            (attacker == ElementType.Water && defender == ElementType.Ice))
            return ElementalReaction.Freeze;
        
        // 融化: 火 + 冰
        if ((attacker == ElementType.Fire && defender == ElementType.Ice) ||
            (attacker == ElementType.Ice && defender == ElementType.Fire))
            return ElementalReaction.Melt;
        
        // 蒸发: 火 + 水
        if ((attacker == ElementType.Fire && defender == ElementType.Water) ||
            (attacker == ElementType.Water && defender == ElementType.Fire))
            return ElementalReaction.Vaporize;
        
        // 超导: 冰 + 雷
        if ((attacker == ElementType.Ice && defender == ElementType.Thunder) ||
            (attacker == ElementType.Thunder && defender == ElementType.Ice))
            return ElementalReaction.Superconduct;
        
        // 超载: 火 + 雷
        if ((attacker == ElementType.Fire && defender == ElementType.Thunder) ||
            (attacker == ElementType.Thunder && defender == ElementType.Fire))
            return ElementalReaction.Overload;
        
        // 感电: 水 + 雷
        if ((attacker == ElementType.Water && defender == ElementType.Thunder) ||
            (attacker == ElementType.Thunder && defender == ElementType.Water))
            return ElementalReaction.ElectroCharged;
        
        // 扩散: 风 + 任意元素
        if (attacker == ElementType.Wind)
        {
            if (defender == ElementType.Fire) return ElementalReaction.Burning;
            if (defender == ElementType.Water) return ElementalReaction.Swirl;
            if (defender == ElementType.Ice) return ElementalReaction.Swirl;
            if (defender == ElementType.Thunder) return ElementalReaction.Swirl;
        }
        
        // 结晶: 土 + 水/冰/雷/火
        if (attacker == ElementType.Earth)
        {
            if (defender == ElementType.Water || defender == ElementType.Ice || 
                defender == ElementType.Thunder || defender == ElementType.Fire)
                return ElementalReaction.Crystallize;
        }
        
        // 燃烧: 火 + 风
        if ((attacker == ElementType.Fire && defender == ElementType.Wind) ||
            (attacker == ElementType.Wind && defender == ElementType.Fire))
            return ElementalReaction.Burning;
        
        // 绽放: 水 + 火 (反向蒸发)
        if ((attacker == ElementType.Water && defender == ElementType.Fire))
            return ElementalReaction.Bloom;
        
        // 碎冰: 冰 + 物理
        if (attacker == ElementType.Ice && defender == ElementType.None)
            return ElementalReaction.Shatter;
        
        return ElementalReaction.None;
    }
    
    /// <summary>
    /// 添加元素附着
    /// </summary>
    public void AddAura(Node target, ElementalAura aura)
    {
        var auras = target is Player ? GetPlayerAuras(target) : GetEnemyAuras(target);
        
        // 如果已有相同元素，刷新时间
        foreach (var existing in auras)
        {
            if (existing.Element == aura.Element)
            {
                existing.Duration = Mathf.Max(existing.Duration, aura.Duration);
                existing.Intensity = Mathf.Max(existing.Intensity, aura.Intensity);
                return;
            }
        }
        
        // 添加新附着
        auras.Add(aura);
        
        // 触发附着视觉效果
        TriggerAuraVisual(target, aura.Element);
    }
    
    /// <summary>
    /// 移除元素附着
    /// </summary>
    public void RemoveAura(Node target, ElementalAura aura)
    {
        var auras = target is Player ? GetPlayerAuras(target) : GetEnemyAuras(target);
        auras.Remove(aura);
    }
    
    /// <summary>
    /// 获取玩家元素附着
    /// </summary>
    private List<ElementalAura> GetPlayerAuras(Node player)
    {
        if (!_playerAuras.ContainsKey(player))
            _playerAuras[player] = new List<ElementalAura>();
        return _playerAuras[player];
    }
    
    /// <summary>
    /// 获取敌人元素附着
    /// </summary>
    private List<ElementalAura> GetEnemyAuras(Node enemy)
    {
        if (!_enemyAuras.ContainsKey(enemy))
            _enemyAuras[enemy] = new List<ElementalAura>();
        return _enemyAuras[enemy];
    }
    
    /// <summary>
    /// 触发反应视觉效果
    /// </summary>
    private void TriggerReactionEffect(Node target, ElementalReaction reaction)
    {
        if (target is Node2D node2D)
        {
            // 根据反应类型触发不同效果
            var position = node2D.GlobalPosition;
            
            switch (reaction)
            {
                case ElementalReaction.Freeze:
                    // 冻结特效 - 蓝白色粒子
                    GameManager.Instance?.SpawnEffect(position, "freeze_effect");
                    break;
                case ElementalReaction.Melt:
                    // 融化特效 - 橙红色
                    GameManager.Instance?.SpawnEffect(position, "melt_effect");
                    break;
                case ElementalReaction.Vaporize:
                    // 蒸发特效 - 蒸汽
                    GameManager.Instance?.SpawnEffect(position, "vaporize_effect");
                    break;
                case ElementalReaction.Overload:
                    // 超载特效 - 爆炸
                    GameManager.Instance?.SpawnEffect(position, "overload_effect");
                    break;
                case ElementalReaction.ElectroCharged:
                    // 感电特效 - 电弧
                    GameManager.Instance?.SpawnEffect(position, "shock_effect");
                    break;
                case ElementalReaction.Burning:
                    // 燃烧特效 - 火焰
                    GameManager.Instance?.SpawnEffect(position, "burn_effect");
                    break;
            }
        }
    }
    
    /// <summary>
    /// 触发元素附着视觉效果
    /// </summary>
    private void TriggerAuraVisual(Node target, ElementType element)
    {
        if (target is Node2D node2D)
        {
            // 根据元素类型显示不同颜色光环
            Color auraColor = element switch
            {
                ElementType.Fire => new Color(1f, 0.3f, 0f),
                ElementType.Water => new Color(0.2f, 0.5f, 1f),
                ElementType.Ice => new Color(0.8f, 0.9f, 1f),
                ElementType.Thunder => new Color(1f, 1f, 0.2f),
                ElementType.Wind => new Color(0.8f, 1f, 0.8f),
                ElementType.Earth => new Color(0.6f, 0.5f, 0.3f),
                ElementType.Light => new Color(1f, 1f, 0.9f),
                ElementType.Dark => new Color(0.4f, 0.2f, 0.6f),
                _ => Colors.White
            };
            
            // 这里可以添加实际的光环效果节点
            GD.Print($"[Elemental] Aura applied: {element} ({auraColor})");
        }
    }
    
    /// <summary>
    /// 获取目标当前元素附着
    /// </summary>
    public List<ElementalAura> GetTargetAuras(Node target)
    {
        return target is Player ? GetPlayerAuras(target) : GetEnemyAuras(target);
    }
    
    /// <summary>
    /// 清除目标所有元素附着
    /// </summary>
    public void ClearAuras(Node target)
    {
        if (target is Player)
            _playerAuras[target]?.Clear();
        else
            _enemyAuras[target]?.Clear();
    }
    
    /// <summary>
    /// 获取元素对应的颜色
    /// </summary>
    public Color GetElementColor(ElementType element)
    {
        return element switch
        {
            ElementType.Fire => new Color(1f, 0.3f, 0f),
            ElementType.Water => new Color(0.2f, 0.5f, 1f),
            ElementType.Ice => new Color(0.8f, 0.9f, 1f),
            ElementType.Thunder => new Color(1f, 1f, 0.2f),
            ElementType.Wind => new Color(0.8f, 1f, 0.8f),
            ElementType.Earth => new Color(0.6f, 0.5f, 0.3f),
            ElementType.Light => new Color(1f, 1f, 0.9f),
            ElementType.Dark => new Color(0.4f, 0.2f, 0.6f),
            _ => Colors.White
        };
    }
    
    public override void _Process(float delta)
    {
        // 更新元素附着持续时间
        UpdateAuras(delta, _playerAuras);
        UpdateAuras(delta, _enemyAuras);
        
        // 更新反应冷却
        UpdateCooldowns(delta);
    }
    
    private void UpdateAuras(float delta, Dictionary<Node, List<ElementalAura>> auraDict)
    {
        foreach (var kvp in auraDict)
        {
            var auras = kvp.Value;
            for (int i = auras.Count - 1; i >= 0; i--)
            {
                auras[i].Duration -= delta;
                if (auras[i].Duration <= 0)
                {
                    auras.RemoveAt(i);
                }
            }
        }
    }
    
    private void UpdateCooldowns(float delta)
    {
        var keys = new List<Node2D>(_reactionCooldowns.Keys);
        foreach (var key in keys)
        {
            _reactionCooldowns[key] -= delta;
            if (_reactionCooldowns[key] <= 0)
            {
                _reactionCooldowns.Remove(key);
            }
        }
    }
}
