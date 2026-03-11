using Godot;
using System;
using System.Collections.Generic;

public enum PetMorphType
{
    Normal,
    Battle,
    Speed,
    Tank,
    Magic,
    Elite,
    Legendary,
    Mythical
}

public enum PetMorphState
{
    Inactive,
    Transforming,
    Active
}

[Serializable]
public class PetMorph
{
    public string MorphId;
    public string MorphName;
    public PetMorphType MorphType;
    public string Description;
    public int RequiredAffectionLevel;
    public int UnlockCost;
    
    // 属性加成
    public float AttackBonus;
    public float DefenseBonus;
    public float HealthBonus;
    public float SpeedBonus;
    public float CritRateBonus;
    public float CritDamageBonus;
    public float LifeStealBonus;
    
    // 特殊效果
    public string SpecialEffect;
    public float EffectValue;
    
    // 形态外观
    public string VisualEffect;
    public Color GlowColor;
}

[Serializable]
public class PetMorphInstance
{
    public string PetId;
    public string MorphId;
    public PetMorphState State;
    public float TransformProgress;
    public DateTime TransformStartTime;
    public DateTime MorphEndTime;
    public bool IsActive;
}

[Serializable]
public class PlayerMorphData
{
    public Dictionary<string, List<string>> UnlockedMorphs = new Dictionary<string, List<string>>();
    public Dictionary<string, string> ActiveMorphs = new Dictionary<string, string>();
    public Dictionary<string, List<string>> MorphHistory = new Dictionary<string, List<string>>();
    
    // 统计
    public int TotalTransformations;
    public int TotalMorphTime;
    public Dictionary<string, int> MorphUsageCount = new Dictionary<string, int>();
}

public class PetMorphData
{
    // 形态实例数据
    public Dictionary<string, PetMorphInstance> ActiveMorphs = new Dictionary<string, PetMorphInstance>();
    
    // 玩家形态数据
    public PlayerMorphData PlayerMorphData = new PlayerMorphData();
}
