using Godot;
using System;
using System.Collections.Generic;

public class PetMorphSystem
{
    private static PetMorphSystem _instance;
    public static PetMorphSystem Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PetMorphSystem();
            return _instance;
        }
    }
    
    private PetMorphData _data = new PetMorphData();
    private bool _isInitialized = false;
    
    // 信号系统
    public signal void MorphUnlocked(string petId, string morphId);
    public signal void MorphActivated(string petId, string morphId);
    public signal void MorphDeactivated(string petId);
    public signal void MorphTransformed(string petId, string morphId);
    public signal void TransformationStarted(string petId, string morphId);
    public signal void TransformationCompleted(string petId, string morphId);
    public signal void TransformationFailed(string petId, string reason);
    
    public void Initialize()
    {
        if (_isInitialized) return;
        
        PetMorphDatabase.Initialize();
        _isInitialized = true;
        GD.Print("[PetMorphSystem] Initialized with " + PetMorphDatabase.GetAllMorphs().Count + " morphs");
    }
    
    public void SetData(PetMorphData data)
    {
        _data = data;
    }
    
    public PetMorphData GetData()
    {
        return _data;
    }
    
    // 解锁形态
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
    
    // 检查形态是否已解锁
    public bool IsMorphUnlocked(string petId, string morphId)
    {
        if (_data.PlayerMorphData.UnlockedMorphs.ContainsKey(petId))
        {
            return _data.PlayerMorphData.UnlockedMorphs[petId].Contains(morphId);
        }
        return false;
    }
    
    // 获取已解锁形态列表
    public List<string> GetUnlockedMorphs(string petId)
    {
        if (_data.PlayerMorphData.UnlockedMorphs.ContainsKey(petId))
        {
            return new List<string>(_data.PlayerMorphData.UnlockedMorphs[petId]);
        }
        return new List<string>();
    }
    
    // 激活形态
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
    
    // 完成形态转换
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
    
    // 取消形态
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
    
    // 检查是否有激活的形态
    public bool HasActiveMorph(string petId)
    {
        return _data.PlayerMorphData.ActiveMorphs.ContainsKey(petId);
    }
    
    // 获取当前激活的形态
    public string GetActiveMorph(string petId)
    {
        if (_data.PlayerMorphData.ActiveMorphs.ContainsKey(petId))
        {
            return _data.PlayerMorphData.ActiveMorphs[petId];
        }
        return null;
    }
    
    // 获取形态属性加成
    public float GetMorphAttackBonus(string petId)
    {
        return GetMorphStatBonus(petId, "attack");
    }
    
    public float GetMorphDefenseBonus(string petId)
    {
        return GetMorphStatBonus(petId, "defense");
    }
    
    public float GetMorphHealthBonus(string petId)
    {
        return GetMorphStatBonus(petId, "health");
    }
    
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
    
    // 应用/移除形态属性加成
    private void ApplyMorphBonuses(string petId, string morphId, bool apply)
    {
        var morph = PetMorphDatabase.GetMorph(morphId);
        if (morph == null) return;
        
        float multiplier = apply ? 1f : -1f;
        
        // 这里应该调用宠物系统来应用属性加成
        // 由于宠物系统可能有不同的实现，这里只是一个接口
        // 实际应用中需要与 PetSystem 集成
    }
    
    // 获取宠物好感度等级
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
    
    // 检查宠物是否有效
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
    
    // 获取可用形态列表
    public List<PetMorph> GetAvailableMorphsForPet(string petId)
    {
        int affectionLevel = GetPetAffectionLevel(petId);
        return PetMorphDatabase.GetAvailableMorphs(affectionLevel);
    }
    
    // 获取统计数据
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
    
    // 存档数据
    public Dictionary<string, object> GetSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // 序列化已解锁形态
        var unlockedMorphs = new Dictionary<string, List<string>>();
        foreach (var kvp in _data.PlayerMorphData.UnlockedMorphs)
        {
            unlockedMorphs[kvp.Key] = kvp.Value;
        }
        data["unlocked_morphs"] = unlockedMorphs;
        
        // 序列化激活形态
        var activeMorphs = new Dictionary<string, string>();
        foreach (var kvp in _data.PlayerMorphData.ActiveMorphs)
        {
            activeMorphs[kvp.Key] = kvp.Value;
        }
        data["active_morphs"] = activeMorphs;
        
        // 统计
        data["total_transformations"] = _data.PlayerMorphData.TotalTransformations;
        data["morph_usage_count"] = _data.PlayerMorphData.MorphUsageCount;
        
        return data;
    }
    
    public void LoadSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        Initialize();
        
        if (data.ContainsKey("unlocked_morphs"))
        {
            var unlockedMorphs = (Dictionary<string, List<string>>)data["unlocked_morphs"];
            _data.PlayerMorphData.UnlockedMorphs = unlockedMorphs;
        }
        
        if (data.ContainsKey("active_morphs"))
        {
            var activeMorphs = (Dictionary<string, string>)data["active_morphs"];
            _data.PlayerMorphData.ActiveMorphs = activeMorphs;
        }
        
        if (data.ContainsKey("total_transformations"))
        {
            _data.PlayerMorphData.TotalTransformations = (int)data["total_transformations"];
        }
        
        if (data.ContainsKey("morph_usage_count"))
        {
            _data.PlayerMorphData.MorphUsageCount = (Dictionary<string, int>)data["morph_usage_count"];
        }
        
        GD.Print("[PetMorphSystem] Save data loaded");
    }
}
