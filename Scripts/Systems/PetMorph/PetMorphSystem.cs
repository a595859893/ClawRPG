using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Pet Morph System - 宠物变形系统
/// 允许宠物变换形态以获得属性加成和特殊效果
/// </summary>
public partial class PetMorphSystem : BaseSystem
{
    private static PetMorphSystem _instance;
    
    /// <summary>
    /// 获取单例实例
    /// </summary>
    public static PetMorphSystem Instance => _instance ??= new PetMorphSystem();
    
    private PetMorphData _data = new PetMorphData();
    private bool _isInitialized = false;
    
    // 信号系统 (Godot 4 compatible)
    [Signal]
    public delegate void MorphUnlockedDelegateEventHandlerEventHandler(string petId, string morphId);
    [Signal]
    public delegate void MorphActivatedDelegateEventHandlerEventHandler(string petId, string morphId);
    [Signal]
    public delegate void MorphDeactivatedDelegateEventHandlerEventHandler(string petId);
    [Signal]
    public delegate void MorphTransformedDelegateEventHandlerEventHandler(string petId, string morphId);
    [Signal]
    public delegate void TransformationStartedDelegateEventHandlerEventHandler(string petId, string morphId);
    [Signal]
    public delegate void TransformationCompletedDelegateEventHandlerEventHandler(string petId, string morphId);
    [Signal]
    public delegate void TransformationFailedDelegateEventHandlerEventHandler(string petId, string morphId);
    
    /// <summary>
    /// 初始化系统
    /// </summary>
    protected override void Initialize()
    {
        if (_isInitialized) return;
        
        PetMorphDatabase.Initialize();
        _isInitialized = true;
        IsInitialized = true;
        GD.Print("[PetMorphSystem] Initialized with " + PetMorphDatabase.GetAllMorphs().Count + " morphs");
    }
    
    /// <summary>
    /// 设置数据
    /// </summary>
    public void SetData(PetMorphData data)
    {
        _data = data;
    }
    
    /// <summary>
    /// 获取数据
    /// </summary>
    public PetMorphData GetData()
    {
        return _data;
    }
    
    /// <summary>
    /// 解锁形态
    /// </summary>
    public bool UnlockMorph(string petId, string morphId)
    {
        Initialize();
        
        var morph = PetMorphDatabase.GetMorph(morphId);
        if (morph == null)
        {
            GD.PrintErr("[PetMorphSystem] Morph not found: " + morphId);
            return false;
        }
        
        // 检查是否已解锁
        if (IsMorphUnlocked(petId, morphId))
        {
            GD.Print("[PetMorphSystem] Morph already unlocked: " + morphId);
            return false;
        }
        
        // 检查金币
        int playerGold = Player.Instance != null ? (int)Player.Instance.Gold : 0;
        if (playerGold < morph.UnlockCost)
        {
            GD.Print("[PetMorphSystem] Not enough gold to unlock morph: " + morphId);
            return false;
        }
        
        // 检查好感度
        int affectionLevel = GetPetAffectionLevel(petId);
        if (affectionLevel < morph.RequiredAffectionLevel)
        {
            GD.Print("[PetMorphSystem] Not enough affection level: " + morph.RequiredAffectionLevel + " needed, have " + affectionLevel);
            return false;
        }
        
        // 扣除金币
        if (Player.Instance != null)
        {
            Player.Instance.ModifyGold(-morph.UnlockCost);
        }
        
        // 解锁形态
        if (!_data.PlayerMorphData.UnlockedMorphs.ContainsKey(petId))
        {
            _data.PlayerMorphData.UnlockedMorphs[petId] = new List<string>();
        }
        _data.PlayerMorphData.UnlockedMorphs[petId].Add(morphId);
        
        MorphUnlocked.Call(petId, morphId);
        GD.Print("[PetMorphSystem] Unlocked morph: " + morphId + " for pet: " + petId);
        
        return true;
    }
    
    /// <summary>
    /// 检查形态是否已解锁
    /// </summary>
    public bool IsMorphUnlocked(string petId, string morphId)
    {
        if (_data.PlayerMorphData.UnlockedMorphs.ContainsKey(petId))
        {
            return _data.PlayerMorphData.UnlockedMorphs[petId].Contains(morphId);
        }
        return false;
    }
    
    /// <summary>
    /// 获取已解锁形态列表
    /// </summary>
    public List<string> GetUnlockedMorphs(string petId)
    {
        if (_data.PlayerMorphData.UnlockedMorphs.ContainsKey(petId))
        {
            return new List<string>(_data.PlayerMorphData.UnlockedMorphs[petId]);
        }
        return new List<string>();
    }
    
    /// <summary>
    /// 激活形态
    /// </summary>
    public bool ActivateMorph(string petId, string morphId)
    {
        Initialize();
        
        // 检查形态是否已解锁
        if (!IsMorphUnlocked(petId, morphId))
        {
            GD.Print("[PetMorphSystem] Morph not unlocked: " + morphId);
            return false;
        }
        
        // 检查宠物是否存在
        if (!IsValidPet(petId))
        {
            GD.Print("[PetMorphSystem] Invalid pet: " + petId);
            return false;
        }
        
        var morph = PetMorphDatabase.GetMorph(morphId);
        if (morph == null) return false;
        
        // 如果已有激活的形态，先取消
        if (_data.PlayerMorphData.ActiveMorphs.ContainsKey(petId))
        {
            DeactivateMorph(petId);
        }
        
        // 创建形态实例
        PetMorphInstance instance = new PetMorphInstance
        {
            PetId = petId,
            MorphId = morphId,
            State = PetMorphState.Transforming,
            TransformProgress = 0f,
            TransformStartTime = DateTime.Now,
            IsActive = false
        };
        
        _data.ActiveMorphs[petId] = instance;
        _data.PlayerMorphData.ActiveMorphs[petId] = morphId;
        
        TransformationStarted.Call(petId, morphId);
        
        // 启动变身动画（简化处理，立即完成）
        CompleteTransformation(petId, morphId);
        
        return true;
    }
    
    /// <summary>
    /// 完成形态转换
    /// </summary>
    private void CompleteTransformation(string petId, string morphId)
    {
        if (_data.ActiveMorphs.ContainsKey(petId))
        {
            var instance = _data.ActiveMorphs[petId];
            instance.State = PetMorphState.Active;
            instance.IsActive = true;
            instance.TransformProgress = 1f;
            
            _data.PlayerMorphData.TotalTransformations++;
            
            if (!_data.PlayerMorphData.MorphUsageCount.ContainsKey(morphId))
            {
                _data.PlayerMorphData.MorphUsageCount[morphId] = 0;
            }
            _data.PlayerMorphData.MorphUsageCount[morphId]++;
            
            // 应用形态属性加成
            ApplyMorphBonuses(petId, morphId, true);
            
            MorphTransformed.Call(petId, morphId);
            MorphActivated.Call(petId, morphId);
            TransformationCompleted.Call(petId, morphId);
            
            GD.Print("[PetMorphSystem] Morph transformation completed: " + morphId + " for pet: " + petId);
        }
    }
    
    /// <summary>
    /// 取消形态
    /// </summary>
    public bool DeactivateMorph(string petId)
    {
        if (_data.ActiveMorphs.ContainsKey(petId))
        {
            var instance = _data.ActiveMorphs[petId];
            string morphId = instance.MorphId;
            
            // 移除形态属性加成
            ApplyMorphBonuses(petId, morphId, false);
            
            instance.State = PetMorphState.Inactive;
            instance.IsActive = false;
            
            _data.PlayerMorphData.ActiveMorphs.Remove(petId);
            _data.ActiveMorphs.Remove(petId);
            
            MorphDeactivated.Call(petId);
            
            GD.Print("[PetMorphSystem] Morph deactivated for pet: " + petId);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 检查是否有激活的形态
    /// </summary>
    public bool HasActiveMorph(string petId)
    {
        return _data.PlayerMorphData.ActiveMorphs.ContainsKey(petId);
    }
    
    /// <summary>
    /// 获取当前激活的形态
    /// </summary>
    public string GetActiveMorph(string petId)
    {
        if (_data.PlayerMorphData.ActiveMorphs.ContainsKey(petId))
        {
            return _data.PlayerMorphData.ActiveMorphs[petId];
        }
        return null;
    }
    
    /// <summary>
    /// 获取形态攻击加成
    /// </summary>
    public float GetMorphAttackBonus(string petId)
    {
        return GetMorphStatBonus(petId, "attack");
    }
    
    /// <summary>
    /// 获取形态防御加成
    /// </summary>
    public float GetMorphDefenseBonus(string petId)
    {
        return GetMorphStatBonus(petId, "defense");
    }
    
    /// <summary>
    /// 获取形态生命加成
    /// </summary>
    public float GetMorphHealthBonus(string petId)
    {
        return GetMorphStatBonus(petId, "health");
    }
    
    /// <summary>
    /// 获取形态速度加成
    /// </summary>
    public float GetMorphSpeedBonus(string petId)
    {
        return GetMorphStatBonus(petId, "speed");
    }
    
    private float GetMorphStatBonus(string petId, string statType)
    {
        if (!HasActiveMorph(petId)) return 0f;
        
        string morphId = GetActiveMorph(petId);
        var morph = PetMorphDatabase.GetMorph(morphId);
        if (morph == null) return 0f;
        
        switch (statType)
        {
            case "attack": return morph.AttackBonus;
            case "defense": return morph.DefenseBonus;
            case "health": return morph.HealthBonus;
            case "speed": return morph.SpeedBonus;
            case "critrate": return morph.CritRateBonus;
            case "critdamage": return morph.CritDamageBonus;
            case "lifesteal": return morph.LifeStealBonus;
            default: return 0f;
        }
    }
    
    /// <summary>
    /// 应用/移除形态属性加成
    /// </summary>
    private void ApplyMorphBonuses(string petId, string morphId, bool apply)
    {
        var morph = PetMorphDatabase.GetMorph(morphId);
        if (morph == null) return;
        
        float multiplier = apply ? 1f : -1f;
        
        // 这里应该调用宠物系统来应用属性加成
        // 由于宠物系统可能有不同的实现，这里只是一个接口
        // 实际应用中需要与 PetSystem 集成
    }
    
    /// <summary>
    /// 获取宠物好感度等级
    /// </summary>
    private int GetPetAffectionLevel(string petId)
    {
        // 这里应该调用宠物好感度系统
        // 暂时返回默认值
        if (PetAffectionSystem.Instance != null)
        {
            var petAffection = PetAffectionSystem.Instance.GetPetAffection(petId);
            if (petAffection > 0)
            {
                // 好感度等级 = log2(好感度/100) + 1
                return Mathf.Max(1, (int)(Mathf.Log(petAffection / 100f + 1f) * 3f) + 1);
            }
        }
        return 1;
    }
    
    /// <summary>
    /// 检查宠物是否有效
    /// </summary>
    private bool IsValidPet(string petId)
    {
        // 检查宠物是否存在
        if (PetManager.Instance != null)
        {
            var pets = PetManager.Instance.GetOwnedPets();
            foreach (var pet in pets)
            {
                if (pet.Id == petId) return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// 获取可用形态列表
    /// </summary>
    public List<PetMorph> GetAvailableMorphsForPet(string petId)
    {
        int affectionLevel = GetPetAffectionLevel(petId);
        return PetMorphDatabase.GetAvailableMorphs(affectionLevel);
    }
    
    /// <summary>
    /// 获取统计数据
    /// </summary>
    public Dictionary<string, int> GetStatistics()
    {
        var stats = new Dictionary<string, int>();
        stats["total_transformations"] = _data.PlayerMorphData.TotalTransformations;
        stats["total_morph_time"] = _data.PlayerMorphData.TotalMorphTime;
        
        int uniqueMorphs = 0;
        foreach (var list in _data.PlayerMorphData.UnlockedMorphs.Values)
        {
            uniqueMorphs += list.Count;
        }
        stats["unique_morphs_unlocked"] = uniqueMorphs;
        
        return stats;
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // 序列化已解锁形态
        var unlockedMorphs = new Dictionary<string, object>();
        foreach (var kvp in _data.PlayerMorphData.UnlockedMorphs)
        {
            unlockedMorphs[kvp.Key] = kvp.Value;
        }
        data["unlocked_morphs"] = unlockedMorphs;
        
        // 序列化激活形态
        var activeMorphs = new Dictionary<string, object>();
        foreach (var kvp in _data.PlayerMorphData.ActiveMorphs)
        {
            activeMorphs[kvp.Key] = kvp.Value;
        }
        data["active_morphs"] = activeMorphs;
        
        // 统计
        data["total_transformations"] = _data.PlayerMorphData.TotalTransformations;
        data["morph_usage_count"] = _data.PlayerMorphData.MorphUsageCount;
        
        GD.Print("[PetMorphSystem] ExportSaveData called");
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        Initialize();
        
        if (data.ContainsKey("unlocked_morphs"))
        {
            var unlockedMorphs = (Dictionary)data["unlocked_morphs"];
            _data.PlayerMorphData.UnlockedMorphs = new Dictionary<string, List<string>>();
            foreach (var key in unlockedMorphs.Keys)
            {
                var list = (Godot.Collections.Array)unlockedMorphs[key];
                _data.PlayerMorphData.UnlockedMorphs[key.ToString()] = new List<string>();
                foreach (var item in list)
                {
                    _data.PlayerMorphData.UnlockedMorphs[key.ToString()].Add(item.ToString());
                }
            }
        }
        
        if (data.ContainsKey("active_morphs"))
        {
            var activeMorphs = (Dictionary)data["active_morphs"];
            _data.PlayerMorphData.ActiveMorphs = new Dictionary<string, string>();
            foreach (var kvp in activeMorphs)
            {
                _data.PlayerMorphData.ActiveMorphs[kvp.Key.ToString()] = kvp.Value.ToString();
            }
        }
        
        if (data.ContainsKey("total_transformations"))
        {
            _data.PlayerMorphData.TotalTransformations = Convert.ToInt32(data["total_transformations"]);
        }
        
        if (data.ContainsKey("morph_usage_count"))
        {
            var morphUsageCount = (Dictionary)data["morph_usage_count"];
            _data.PlayerMorphData.MorphUsageCount = new Dictionary<string, int>();
            foreach (var kvp in morphUsageCount)
            {
                _data.PlayerMorphData.MorphUsageCount[kvp.Key.ToString()] = Convert.ToInt32(kvp.Value);
            }
        }
        
        GD.Print("[PetMorphSystem] ImportSaveData called - Total transformations: " + _data.PlayerMorphData.TotalTransformations);
    }
}
