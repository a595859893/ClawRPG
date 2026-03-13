using Godot;
using System.Collections.Generic;

public class PetLifeCycleSystem : Node
{
    private PetLifeCycleData _data;
    private PetLifeCycleDatabase _database;
    
    public override void _Ready()
    {
        _data = new PetLifeCycleData();
        _database = new PetLifeCycleDatabase();
        LoadData();
    }
    
    // 注册宠物到生命周期系统
    public void RegisterPet(int petId, string petName, string petType)
    {
        if (_data.PetLifeCycles.ContainsKey(petId))
            return;
        
        var config = PetLifeCycleDatabase.PetTypeConfigs.ContainsKey(petType) 
            ? PetLifeCycleDatabase.PetTypeConfigs[petType] 
            : PetLifeCycleDatabase.DefaultConfig;
        
        var entry = new PetLifeCycleEntry
        {
            PetId = petId,
            PetName = petName,
            CurrentAge = 0,
            MaxAge = config.BaseMaxAge,
            CurrentStage = LifeStage.Baby,
            IsImmortal = false,
            LifeExtensionUsed = 0,
            DaysSinceLastStageChange = 0
        };
        
        _data.PetLifeCycles[petId] = entry;
        _data.TotalLifeCycles++;
        SaveData();
    }
    
    // 每日更新 - 应该在每天结束时调用
    public void UpdateDaily(int petId)
    {
        if (!_data.PetLifeCycles.ContainsKey(petId))
            return;
        
        var pet = _data.PetLifeCycles[petId];
        if (pet.IsImmortal)
            return;
        
        pet.CurrentAge++;
        pet.DaysSinceLastStageChange++;
        
        // 检查阶段变化
        var oldStage = pet.CurrentStage;
        pet.CurrentStage = CalculateStage(pet.CurrentAge, pet.MaxAge);
        
        if (oldStage != pet.CurrentStage)
        {
            pet.DaysSinceLastStageChange = 0;
            OnStageChanged(petId, oldStage, pet.CurrentStage);
        }
        
        // 检查死亡
        if (pet.CurrentAge >= pet.MaxAge && !pet.IsImmortal)
        {
            OnPetDeath(petId);
        }
        
        SaveData();
    }
    
    // 计算当前阶段
    private LifeStage CalculateStage(int age, int maxAge)
    {
        float percentage = (float)age / maxAge;
        
        if (percentage < 0.1f) return LifeStage.Baby;
        if (percentage < 0.3f) return LifeStage.Young;
        if (percentage < 0.7f) return LifeStage.Adult;
        if (percentage < 0.9f) return LifeStage.Senior;
        if (percentage < 1.0f) return LifeStage.Final;
        return LifeStage.Immortal;
    }
    
    // 阶段变化事件
    private void OnStageChanged(int petId, LifeStage oldStage, LifeStage newStage)
    {
        var pet = _data.PetLifeCycles[petId];
        GD.Print($"[PetLifeCycle] {pet.PetName} 从 {oldStage} 阶段进入了 {newStage} 阶段");
        
        // 获取阶段事件消息
        if (PetLifeCycleDatabase.StageChangeEvents.ContainsKey(newStage))
        {
            var events = PetLifeCycleDatabase.StageChangeEvents[newStage];
            var randomIndex = (int)(GD.Rand() * events.Count);
            GD.Print($"[PetLifeCycle] {pet.PetName}: {events[randomIndex]}");
        }
    }
    
    // 宠物死亡
    private void OnPetDeath(int petId)
    {
        if (!_data.PetLifeCycles.ContainsKey(petId))
            return;
        
        var pet = _data.PetLifeCycles[petId];
        
        // 记录历史
        var historyEntry = new LifeCycleHistoryEntry
        {
            PetId = petId,
            PetName = pet.PetName,
            AgeAtDeath = pet.CurrentAge,
            StageAtDeath = pet.CurrentStage,
            WasExtended = pet.LifeExtensionUsed > 0,
            LifeExtensions = pet.LifeExtensionUsed,
            Timestamp = OS.GetUnixTime()
        };
        
        _data.History.Insert(0, historyEntry);
        if (_data.History.Count > 50)
            _data.History.RemoveAt(_data.History.Count - 1);
        
        _data.TotalDeaths++;
        if (pet.CurrentAge > _data.LongestLifeSpan)
            _data.LongestLifeSpan = pet.CurrentAge;
        
        GD.Print($"[PetLifeCycle] {pet.PetName} 在 {pet.CurrentAge} 天岁时离世...");
        
        // 从活跃列表移除
        _data.PetLifeCycles.Remove(petId);
        SaveData();
    }
    
    // 使用生命延续道具
    public bool UseLifeExtensionItem(int petId, string itemId)
    {
        if (!_data.PetLifeCycles.ContainsKey(petId))
            return false;
        
        var pet = _data.PetLifeCycles[petId];
        
        // 查找道具
        LifeExtensionItem item = null;
        foreach (var i in PetLifeCycleDatabase.LifeExtensionItems)
        {
            if (i.ItemId == itemId)
            {
                item = i;
                break;
            }
        }
        
        if (item == null)
            return false;
        
        // 检查是否不朽
        if (pet.IsImmortal)
            return false;
        
        // 延长生命
        if (item.DaysExtended >= 999)
        {
            pet.IsImmortal = true;
            pet.CurrentStage = LifeStage.Immortal;
            GD.Print($"[PetLifeCycle] {pet.PetName} 获得了不朽的生命!");
        }
        else
        {
            pet.MaxAge += item.DaysExtended;
            pet.LifeExtensionUsed++;
            _data.TotalLifeExtensions++;
            GD.Print($"[PetLifeCycle] {pet.PetName} 的生命延长了 {item.DaysExtended} 天");
        }
        
        SaveData();
        return true;
    }
    
    // 获取宠物当前阶段
    public LifeStage GetPetStage(int petId)
    {
        if (!_data.PetLifeCycles.ContainsKey(petId))
            return LifeStage.Adult;
        return _data.PetLifeCycles[petId].CurrentStage;
    }
    
    // 获取宠物属性加成
    public float GetStatBonus(int petId)
    {
        if (!_data.PetLifeCycles.ContainsKey(petId))
            return 1.0f;
        
        var pet = _data.PetLifeCycles[petId];
        var stageConfig = PetLifeCycleDatabase.StageConfigs[pet.CurrentStage];
        
        return stageConfig.OverallBonus;
    }
    
    // 获取阶段信息
    public string GetStageInfo(int petId)
    {
        if (!_data.PetLifeCycles.ContainsKey(petId))
            return "";
        
        var pet = _data.PetLifeCycles[petId];
        var stageConfig = PetLifeCycleDatabase.StageConfigs[pet.CurrentStage];
        
        return $"{stageConfig.StageName}: {stageConfig.Description}";
    }
    
    // 获取生命周期进度(0-100)
    public float GetLifeProgress(int petId)
    {
        if (!_data.PetLifeCycles.ContainsKey(petId))
            return 0;
        
        var pet = _data.PetLifeCycles[petId];
        if (pet.IsImmortal)
            return 100;
        
        return Mathf.Min(100, (float)pet.CurrentAge / pet.MaxAge * 100);
    }
    
    // 获取统计数据
    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            {"TotalLifeCycles", _data.TotalLifeCycles},
            {"TotalDeaths", _data.TotalDeaths},
            {"TotalLifeExtensions", _data.TotalLifeExtensions},
            {"LongestLifeSpan", _data.LongestLifeSpan},
            {"ActivePets", _data.PetLifeCycles.Count}
        };
    }
    
    // 存档
    private void SaveData()
    {
        // TODO: 实现存档逻辑
    }
    
    // 读档
    private void LoadData()
    {
        // TODO: 实现读档逻辑
    }
}
