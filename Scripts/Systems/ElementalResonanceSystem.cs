using Godot;
/// <summary>
/// 元素共鸣系统。
/// </summary>
using System;
using System.Collections.Generic;

/// <summary>
/// 元素共鸣系统 - 管理元素共鸣加成
/// </summary>
public class ElementalResonanceSystem : BaseSystem
{
    public static ElementalResonanceSystem Instance { get; private set; }

    // Element types
    public enum Element { Fire, Water, Ice, Lightning, Dark, Holy, Earth, Wind, Poison, None }

    // Resonance types
    public enum ResonanceType
    {
        None, Explosion, Steam, Melt, Freeze, Shock, Void, Light, Storm, Nature, 
        Biohazard, Magma, Blizzard, ThunderStorm, Judgement, Corruption, Chaos
    }

    // Active elements on target
    private Dictionary<int, List<Element>> targetElements = new Dictionary<int, List<Element>>();
    private Dictionary<int, float> elementTimers = new Dictionary<int, float>();

    // Resonance damage bonuses
    private float baseResonanceDamage = 1.5f;
    private float elementDuration = 3.0f;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(float delta)
    {
        // Update element timers
        List<int> expiredTargets = new List<int>();
        foreach (var kvp in elementTimers)
        {
            elementTimers[kvp.Key] -= delta;
            if (elementTimers[kvp.Key] <= 0)
            {
                expiredTargets.Add(kvp.Key);
            }
        }

        foreach (int targetId in expiredTargets)
        {
            RemoveTarget(targetId);
        }
    }

    public void ApplyElement(int targetId, Element element)
    {
        if (!targetElements.ContainsKey(targetId))
        {
            targetElements[targetId] = new List<Element>();
            elementTimers[targetId] = elementDuration;
        }

        if (!targetElements[targetId].Contains(element))
        {
            targetElements[targetId].Add(element);
            elementTimers[targetId] = elementDuration;
        }
    }

    public void RemoveTarget(int targetId)
    {
        if (targetElements.ContainsKey(targetId))
        {
            targetElements.Remove(targetId);
        }
        if (elementTimers.ContainsKey(targetId))
        {
            elementTimers.Remove(targetId);
        }
    }

    public ResonanceType CheckResonance(int targetId)
    {
        if (!targetElements.ContainsKey(targetId) || targetElements[targetId].Count < 2)
            return ResonanceType.None;

        List<Element> elements = targetElements[targetId];
        
        // Check all combinations for resonances
        foreach (Element e1 in elements)
        {
            foreach (Element e2 in elements)
            {
                if (e1 == e2) continue;
                
                ResonanceType resonance = GetResonanceType(e1, e2);
                if (resonance != ResonanceType.None)
                    return resonance;
            }
        }

        return ResonanceType.None;
    }

    private ResonanceType GetResonanceType(Element e1, Element e2)
    {
        // Fire combinations
        if ((e1 == Element.Fire && e2 == Element.Fire) || (e1 == Element.Fire && e2 == Element.Fire))
            return ResonanceType.Explosion;
        if ((e1 == Element.Fire && e2 == Element.Water) || (e1 == Element.Water && e2 == Element.Fire))
            return ResonanceType.Steam;
        if ((e1 == Element.Fire && e2 == Element.Ice) || (e1 == Element.Ice && e2 == Element.Fire))
            return ResonanceType.Melt;
        if ((e1 == Element.Fire && e2 == Element.Earth) || (e1 == Element.Earth && e2 == Element.Fire))
            return ResonanceType.Magma;

        // Ice combinations
        if ((e1 == Element.Ice && e2 == Element.Water) || (e1 == Element.Water && e2 == Element.Ice))
            return ResonanceType.Freeze;
        if ((e1 == Element.Ice && e2 == Element.Lightning) || (e1 == Element.Lightning && e2 == Element.Ice))
            return ResonanceType.Blizzard;
        if ((e1 == Element.Ice && e2 == Element.Wind) || (e1 == Element.Wind && e2 == Element.Ice))
            return ResonanceType.Blizzard;

        // Lightning combinations
        if ((e1 == Element.Lightning && e2 == Element.Water) || (e1 == Element.Water && e2 == Element.Lightning))
            return ResonanceType.Shock;
        if ((e1 == Element.Lightning && e2 == Element.Wind) || (e1 == Element.Wind && e2 == Element.Lightning))
            return ResonanceType.ThunderStorm;

        // Dark combinations
        if ((e1 == Element.Dark && e2 == Element.Fire) || (e1 == Element.Fire && e2 == Element.Dark))
            return ResonanceType.Void;
        if ((e1 == Element.Dark && e2 == Element.Holy) || (e1 == Element.Holy && e2 == Element.Dark))
            return ResonanceType.Judgement;
        if ((e1 == Element.Dark && e2 == Element.Poison) || (e1 == Element.Poison && e2 == Element.Dark))
            return ResonanceType.Corruption;

        // Holy combinations
        if ((e1 == Element.Holy && e2 == Element.Fire) || (e1 == Element.Fire && e2 == Element.Holy))
            return ResonanceType.Judgement;
        if ((e1 == Element.Holy && e2 == Element.Water) || (e1 == Element.Water && e2 == Element.Holy))
            return ResonanceType.Light;

        // Nature combinations
        if ((e1 == Element.Earth && e2 == Element.Water) || (e1 == Element.Water && e2 == Element.Earth))
            return ResonanceType.Nature;
        if ((e1 == Element.Earth && e2 == Element.Poison) || (e1 == Element.Poison && e2 == Element.Earth))
            return ResonanceType.Biohazard;
        if ((e1 == Element.Wind && e2 == Element.Poison) || (e1 == Element.Poison && e2 == Element.Wind))
            return ResonanceType.Biohazard;

        // Three elements
        if (elements.Count >= 3)
            return ResonanceType.Chaos;

        return ResonanceType.None;
    }

    public float GetResonanceDamageMultiplier(ResonanceType resonance)
    {
        switch (resonance)
        {
            case ResonanceType.Explosion: return 2.0f;
            case ResonanceType.Steam: return 1.8f;
            case ResonanceType.Melt: return 1.7f;
            case ResonanceType.Freeze: return 1.8f;
            case ResonanceType.Shock: return 1.7f;
            case ResonanceType.Void: return 2.2f;
            case ResonanceType.Light: return 1.6f;
            case ResonanceType.Storm: return 1.9f;
            case ResonanceType.Nature: return 1.5f;
            case ResonanceType.Biohazard: return 1.8f;
            case ResonanceType.Magma: return 2.1f;
            case ResonanceType.Blizzard: return 1.9f;
            case ResonanceType.ThunderStorm: return 2.0f;
            case ResonanceType.Judgement: return 2.3f;
            case ResonanceType.Corruption: return 1.7f;
            case ResonanceType.Chaos: return 2.5f;
            default: return 1.0f;
        }
    }

    public string GetResonanceName(ResonanceType resonance)
    {
        switch (resonance)
        {
            case ResonanceType.Explosion: return "烈焰爆发";
            case ResonanceType.Steam: return "蒸汽爆炸";
            case ResonanceType.Melt: return "融化";
            case ResonanceType.Freeze: return "冰冻";
            case ResonanceType.Shock: return "电击";
            case ResonanceType.Void: return "虚空";
            case ResonanceType.Light: return "圣光";
            case ResonanceType.Storm: return "风暴";
            case ResonanceType.Nature: return "自然";
            case ResonanceType.Biohazard: return "剧毒";
            case ResonanceType.Magma: return "熔岩";
            case ResonanceType.Blizzard: return "暴风雪";
            case ResonanceType.ThunderStorm: return "雷暴";
            case ResonanceType.Judgement: return "审判";
            case ResonanceType.Corruption: return "腐蚀";
            case ResonanceType.Chaos: return "混沌";
            default: return "";
        }
    }

    public Color GetResonanceColor(ResonanceType resonance)
    {
        switch (resonance)
        {
            case ResonanceType.Explosion: return new Color(1f, 0.3f, 0f);
            case ResonanceType.Steam: return new Color(0.8f, 0.8f, 0.9f);
            case ResonanceType.Melt: return new Color(1f, 0.5f, 0f);
            case ResonanceType.Freeze: return new Color(0.5f, 0.8f, 1f);
            case ResonanceType.Shock: return new Color(1f, 1f, 0.2f);
            case ResonanceType.Void: return new Color(0.3f, 0f, 0.5f);
            case ResonanceType.Light: return new Color(1f, 1f, 0.8f);
            case ResonanceType.Storm: return new Color(0.6f, 0.6f, 0.9f);
            case ResonanceType.Nature: return new Color(0.2f, 0.8f, 0.2f);
            case ResonanceType.Biohazard: return new Color(0.4f, 0.8f, 0.2f);
            case ResonanceType.Magma: return new Color(0.9f, 0.2f, 0f);
            case ResonanceType.Blizzard: return new Color(0.7f, 0.9f, 1f);
            case ResonanceType.ThunderStorm: return new Color(0.7f, 0.7f, 1f);
            case ResonanceType.Judgement: return new Color(1f, 0.9f, 0.3f);
            case ResonanceType.Corruption: return new Color(0.5f, 0f, 0.5f);
            case ResonanceType.Chaos: return new Color(0.8f, 0f, 0.8f);
            default: return Colors.White;
        }
    }

    public int GetTargetElementCount(int targetId)
    {
        if (!targetElements.ContainsKey(targetId))
            return 0;
        return targetElements[targetId].Count;
    }

    public List<Element> GetTargetElements(int targetId)
    {
        if (!targetElements.ContainsKey(targetId))
            return new List<Element>();
        return new List<Element>(targetElements[targetId]);
    }

    public float GetElementTimeRemaining(int targetId)
    {
        if (!elementTimers.ContainsKey(targetId))
            return 0;
        return elementTimers[targetId];
    }
}
