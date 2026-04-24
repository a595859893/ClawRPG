using Godot;
using ClawRPG.Systems.PetLifeCycle;
using System.Collections.Generic;
using SaveSystem = ClawRPG.Scripts.Framework.SaveSystem;

public partial class PetLifeCycleSystem : BaseSystem
{
    private PetLifeCycleData _data;
    private PetLifeCycleDatabase _database;
    
    public override void _Ready()
    {
        _data = new PetLifeCycleData();
        _database = new PetLifeCycleDatabase();
        LoadData();
    }
    
    // 获取数据
    public PetLifeCycleData GetData() => _data;
    
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
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem == null) return;

        var data = saveSystem.LoadGame();
        if (data == null) data = new Godot.Collections.Dictionary();

        // 保存宠物生命周期数据
        var petCyclesArray = new Godot.Array();
        foreach (var kvp in _data.PetLifeCycles)
        {
            var petData = new Godot.Collections.Dictionary();
            petData["pet_id"] = kvp.Key;
            petData["pet_name"] = kvp.Value.PetName;
            petData["current_age"] = kvp.Value.CurrentAge;
            petData["max_age"] = kvp.Value.MaxAge;
            petData["current_stage"] = (int)kvp.Value.CurrentStage;
            petData["is_immortal"] = kvp.Value.IsImmortal;
            petData["life_extension_used"] = kvp.Value.LifeExtensionUsed;
            petData["days_since_stage_change"] = kvp.Value.DaysSinceLastStageChange;
            petCyclesArray.Add(petData);
        }
        data["pet_life_cycle_pets"] = petCyclesArray;

        // 保存统计数据
        var stats = new Godot.Collections.Dictionary();
        stats["total_life_cycles"] = _data.TotalLifeCycles;
        stats["total_deaths"] = _data.TotalDeaths;
        stats["total_life_extensions"] = _data.TotalLifeExtensions;
        stats["longest_life_span"] = _data.LongestLifeSpan;
        data["pet_life_cycle_stats"] = stats;

        // 保存历史记录 (限制最近50条)
        var historyArray = new Godot.Array();
        var recentHistory = _data.History.TakeLast(50).ToList();
        foreach (var record in recentHistory)
        {
            var historyData = new Godot.Collections.Dictionary();
            historyData["pet_id"] = record.PetId;
            historyData["pet_name"] = record.PetName;
            historyData["age_at_death"] = record.AgeAtDeath;
            historyData["stage_at_death"] = (int)record.StageAtDeath;
            historyData["was_extended"] = record.WasExtended;
            historyData["life_extensions"] = record.LifeExtensions;
            historyData["timestamp"] = record.Timestamp;
            historyArray.Add(historyData);
        }
        data["pet_life_cycle_history"] = historyArray;

        saveSystem.SaveGame(data);
    }
    
    // 读档
    private void LoadData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem == null) return;

        var data = saveSystem.LoadGame();
        if (data == null) return;

        // 加载宠物生命周期数据
        if (data.ContainsKey("pet_life_cycle_pets"))
        {
            var petCyclesArray = (Godot.Array)data["pet_life_cycle_pets"];
            foreach (Godot.Collections.Dictionary petData in petCyclesArray)
            {
                var entry = new PetLifeCycleEntry
                {
                    PetId = (int)petData["pet_id"],
                    PetName = (string)petData["pet_name"],
                    CurrentAge = (int)petData["current_age"],
                    MaxAge = (int)petData["max_age"],
                    CurrentStage = (LifeStage)(int)petData["current_stage"],
                    IsImmortal = (bool)petData["is_immortal"],
                    LifeExtensionUsed = (int)petData["life_extension_used"],
                    DaysSinceLastStageChange = (int)petData["days_since_stage_change"]
                };
                _data.PetLifeCycles[entry.PetId] = entry;
            }
        }

        // 加载统计数据
        if (data.ContainsKey("pet_life_cycle_stats"))
        {
            var stats = (Godot.Collections.Dictionary)data["pet_life_cycle_stats"];
            _data.TotalLifeCycles = (int)stats["total_life_cycles"];
            _data.TotalDeaths = (int)stats["total_deaths"];
            _data.TotalLifeExtensions = (int)stats["total_life_extensions"];
            _data.LongestLifeSpan = (int)stats["longest_life_span"];
        }

        // 加载历史记录
        if (data.ContainsKey("pet_life_cycle_history"))
        {
            var historyArray = (Godot.Array)data["pet_life_cycle_history"];
            foreach (Godot.Collections.Dictionary historyData in historyArray)
            {
                var record = new LifeCycleHistoryEntry
                {
                    PetId = (int)historyData["pet_id"],
                    PetName = (string)historyData["pet_name"],
                    AgeAtDeath = (int)historyData["age_at_death"],
                    StageAtDeath = (LifeStage)(int)historyData["stage_at_death"],
                    WasExtended = (bool)historyData["was_extended"],
                    LifeExtensions = (int)historyData["life_extensions"],
                    Timestamp = (long)historyData["timestamp"]
                };
                _data.History.Add(record);
            }
        }
    }

    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, Variant>();

        if (_data == null) return data;

        // 保存宠物生命周期数据
        var cyclesData = new Dictionary<string, Dictionary<string, Variant>>();
        foreach (var kvp in _data.PetLifeCycles)
        {
            cyclesData[kvp.Key.ToString()] = new Dictionary<string, Variant>
            {
                ["pet_id"] = kvp.Value.PetId,
                ["pet_name"] = kvp.Value.PetName ?? "",
                ["current_age"] = kvp.Value.CurrentAge,
                ["max_age"] = kvp.Value.MaxAge,
                ["current_stage"] = (int)kvp.Value.CurrentStage,
                ["is_immortal"] = kvp.Value.IsImmortal,
                ["life_extension_used"] = kvp.Value.LifeExtensionUsed,
                ["days_since_last_stage_change"] = kvp.Value.DaysSinceLastStageChange
            };
        }
        data["pet_lifecycles"] = cyclesData;

        // 保存统计数据
        data["total_lifecycles"] = _data.TotalLifeCycles;
        data["total_deaths"] = _data.TotalDeaths;
        data["total_life_extensions"] = _data.TotalLifeExtensions;
        data["longest_lifespan"] = _data.LongestLifeSpan;

        // 保存历史记录
        var historyList = new List<Dictionary<string, Variant>>();
        foreach (var entry in _data.History)
        {
            historyList.Add(new Dictionary<string, Variant>
            {
                ["pet_id"] = entry.PetId,
                ["pet_name"] = entry.PetName ?? "",
                ["age_at_death"] = entry.AgeAtDeath,
                ["stage_at_death"] = (int)entry.StageAtDeath,
                ["was_extended"] = entry.WasExtended,
                ["life_extensions"] = entry.LifeExtensions,
                ["timestamp"] = entry.Timestamp
            });
        }
        data["history"] = historyList;

        return data;
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null || _data == null) return;

        // 加载宠物生命周期数据
        if (data.TryGetValue("pet_lifecycles", out var cyclesData))
        {
            _data.PetLifeCycles = new Dictionary<int, PetLifeCycleEntry>();
            var cyclesDict = (Dictionary<string, Variant>)cyclesData;
            foreach (var kvp in cyclesDict)
            {
                if (int.TryParse(kvp.Key, out var petId))
                {
                    var entryData = (Dictionary<string, Variant>)kvp.Value;
                    var entry = new PetLifeCycleEntry();

                    if (entryData.TryGetValue("pet_id", out var id))
                        entry.PetId = (int)id;
                    if (entryData.TryGetValue("pet_name", out var name))
                        entry.PetName = (string)name;
                    if (entryData.TryGetValue("current_age", out var age))
                        entry.CurrentAge = (int)age;
                    if (entryData.TryGetValue("max_age", out var maxAge))
                        entry.MaxAge = (int)maxAge;
                    if (entryData.TryGetValue("current_stage", out var stage))
                        entry.CurrentStage = (LifeStage)(int)stage;
                    if (entryData.TryGetValue("is_immortal", out var isImmortal))
                        entry.IsImmortal = (bool)isImmortal;
                    if (entryData.TryGetValue("life_extension_used", out var extUsed))
                        entry.LifeExtensionUsed = (int)extUsed;
                    if (entryData.TryGetValue("days_since_last_stage_change", out var daysSince))
                        entry.DaysSinceLastStageChange = (int)daysSince;

                    _data.PetLifeCycles[petId] = entry;
                }
            }
        }

        // 加载统计数据
        if (data.TryGetValue("total_lifecycles", out var totalCycles))
            _data.TotalLifeCycles = (int)totalCycles;
        if (data.TryGetValue("total_deaths", out var totalDeaths))
            _data.TotalDeaths = (int)totalDeaths;
        if (data.TryGetValue("total_life_extensions", out var totalExt))
            _data.TotalLifeExtensions = (int)totalExt;
        if (data.TryGetValue("longest_lifespan", out var longestLife))
            _data.LongestLifeSpan = (int)longestLife;

        // 加载历史记录
        if (data.TryGetValue("history", out var historyData))
        {
            _data.History = new List<LifeCycleHistoryEntry>();
            var historyList = (List<Variant>)historyData;
            foreach (var entryVar in historyList)
            {
                var entryDict = (Dictionary<string, Variant>)entryVar;
                var entry = new LifeCycleHistoryEntry();

                if (entryDict.TryGetValue("pet_id", out var petId))
                    entry.PetId = (int)petId;
                if (entryDict.TryGetValue("pet_name", out var petName))
                    entry.PetName = (string)petName;
                if (entryDict.TryGetValue("age_at_death", out var ageAtDeath))
                    entry.AgeAtDeath = (int)ageAtDeath;
                if (entryDict.TryGetValue("stage_at_death", out var stageAtDeath))
                    entry.StageAtDeath = (LifeStage)(int)stageAtDeath;
                if (entryDict.TryGetValue("was_extended", out var wasExtended))
                    entry.WasExtended = (bool)wasExtended;
                if (entryDict.TryGetValue("life_extensions", out var lifeExtensions))
                    entry.LifeExtensions = (int)lifeExtensions;
                if (entryDict.TryGetValue("timestamp", out var timestamp))
                    entry.Timestamp = (long)timestamp;

                _data.History.Add(entry);
            }
        }
    }
}
