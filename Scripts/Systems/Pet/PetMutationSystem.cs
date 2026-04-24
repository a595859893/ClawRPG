using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems;

/// <summary>
/// 宠物变异系统 - 为宠物添加随机变异机制
/// </summary>
public partial class PetMutationSystem : BaseSystem
{
    private static PetMutationSystem _instance;
    public static PetMutationSystem Instance => _instance ??= new PetMutationSystem();
    
    protected override string SystemName => "PetMutationSystem";
    
    private Dictionary<int, PetMutationData> _petMutations = new();
    private Random _random = new();
    
    // 变异触发条件
    private int _totalMutationAttempts = 0;
    private int _successfulMutations = 0;
    private int _rerollUsed = 0;
    
    public event Action<int, PetMutation> OnMutationOccurred;
    public event Action<int> OnMutationRemoved;
    
    public void Initialize()
    {
        LoadData();
    }
    
    public void LoadData()
    {
        // 从存档加载数据
        var saveSystem = SaveLoadSystem.Instance;
        if (saveSystem != null && saveSystem.CurrentSave != null)
        {
            if (saveSystem.CurrentSave.Data.ContainsKey("pet_mutations"))
            {
                var data = saveSystem.CurrentSave.Data["pet_mutations"] as Dictionary<string, object>;
                if (data != null)
                {
                    foreach (var kvp in data)
                    {
                        if (int.TryParse(kvp.Key, out int petId))
                        {
                            var petData = DeserializePetMutationData(kvp.Value as Dictionary<string, object>);
                            if (petData != null)
                            {
                                _petMutations[petId] = petData;
                            }
                        }
                    }
                }
            }
            
            if (saveSystem.CurrentSave.Data.ContainsKey("mutation_stats"))
            {
                var stats = saveSystem.CurrentSave.Data["mutation_stats"] as Dictionary<string, object>;
                if (stats != null)
                {
                    if (stats.ContainsKey("total_attempts")) 
                        _totalMutationAttempts = Convert.ToInt32(stats["total_attempts"]);
                    if (stats.ContainsKey("successful")) 
                        _successfulMutations = Convert.ToInt32(stats["successful"]);
                    if (stats.ContainsKey("rerolls")) 
                        _rerollUsed = Convert.ToInt32(stats["rerolls"]);
                }
            }
        }
    }
    
    public void SaveData()
    {
        var saveSystem = SaveLoadSystem.Instance;
        if (saveSystem != null && saveSystem.CurrentSave != null)
        {
            var data = new Dictionary<string, object>();
            foreach (var kvp in _petMutations)
            {
                data[kvp.Key.ToString()] = SerializePetMutationData(kvp.Value);
            }
            saveSystem.CurrentSave.Data["pet_mutations"] = data;
            
            saveSystem.CurrentSave.Data["mutation_stats"] = new Dictionary<string, object>
            {
                ["total_attempts"] = _totalMutationAttempts,
                ["successful"] = _successfulMutations,
                ["rerolls"] = _rerollUsed
            };
        }
    }
    
    /// <summary>
    /// 尝试为宠物添加变异
    /// </summary>
    public bool TryMutatePet(int petId, int petLevel, float mutationChance = 0.15f)
    {
        _totalMutationAttempts++;
        
        // 基础变异概率
        float chance = mutationChance;
        
        // 等级越高，变异概率越高
        chance += petLevel * 0.01f;
        
        // 随机判定
        if (_random.NextDouble() > chance)
        {
            return false;
        }
        
        // 生成变异
        var mutation = GenerateRandomMutation();
        
        // 添加变异
        AddMutation(petId, mutation);
        _successfulMutations++;
        
        OnMutationOccurred?.Invoke(petId, mutation);
        return true;
    }
    
    /// <summary>
    /// 生成随机变异
    /// </summary>
    private PetMutation GenerateRandomMutation()
    {
        var database = PetMutationDatabase.GetMutations();
        var weights = PetMutationDatabase.GetRarityWeights();
        
        // 按稀有度权重选择
        double totalWeight = 0;
        foreach (var w in weights.Values) totalWeight += w;
        
        double roll = _random.NextDouble() * totalWeight;
        string selectedRarity = "Uncommon";
        double cumulative = 0;
        
        foreach (var kvp in weights)
        {
            cumulative += kvp.Value;
            if (roll <= cumulative)
            {
                selectedRarity = kvp.Key;
                break;
            }
        }
        
        // 筛选符合稀有度的变异
        var candidates = new List<string>();
        foreach (var kvp in database)
        {
            if (kvp.Value["rarity"].ToString() == selectedRarity)
            {
                candidates.Add(kvp.Key);
            }
        }
        
        // 如果没有该稀有度的，从低稀有度中选择
        if (candidates.Count == 0)
        {
            foreach (var kvp in database)
            {
                var rarity = kvp.Value["rarity"].ToString();
                if (rarity == "Uncommon" || rarity == "Common")
                {
                    candidates.Add(kvp.Key);
                }
            }
        }
        
        if (candidates.Count == 0)
        {
            candidates.Add("mutation_ferocious"); // 默认
        }
        
        // 随机选择
        string mutationId = candidates[_random.Next(candidates.Count)];
        var mutationData = database[mutationId];
        
        return new PetMutation
        {
            MutationId = mutationId,
            Name = mutationData["name"].ToString(),
            Description = mutationData["description"].ToString(),
            Type = mutationData["type"].ToString(),
            Rarity = mutationData["rarity"].ToString(),
            StatBonuses = ParseStatBonuses(mutationData["stat_bonuses"] as Dictionary<string, object>),
            AddedAbilities = ParseStringList(mutationData["added_abilities"]),
            VisualEffect = mutationData["visual_effect"].ToString(),
            MutatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }
    
    private Dictionary<string, float> ParseStatBonuses(Dictionary<string, object> data)
    {
        var result = new Dictionary<string, float>();
        if (data != null)
        {
            foreach (var kvp in data)
            {
                result[kvp.Key] = Convert.ToSingle(kvp.Value);
            }
        }
        return result;
    }
    
    private List<string> ParseStringList(object data)
    {
        var result = new List<string>();
        if (data is List<object> list)
        {
            foreach (var item in list)
            {
                result.Add(item.ToString());
            }
        }
        return result;
    }
    
    /// <summary>
    /// 添加变异到宠物
    /// </summary>
    public void AddMutation(int petId, PetMutation mutation)
    {
        if (!_petMutations.ContainsKey(petId))
        {
            _petMutations[petId] = new PetMutationData
            {
                PetId = petId,
                Mutations = new List<PetMutation>(),
                MutationTypeCounts = new Dictionary<string, int>()
            };
        }
        
        var petData = _petMutations[petId];
        petData.Mutations.Add(mutation);
        petData.TotalMutations++;
        
        if (mutation.Rarity == "Rare" || mutation.Rarity == "Epic")
            petData.RareMutations++;
        if (mutation.Rarity == "Legendary")
            petData.LegendaryMutations++;
        
        if (!petData.MutationTypeCounts.ContainsKey(mutation.Type))
            petData.MutationTypeCounts[mutation.Type] = 0;
        petData.MutationTypeCounts[mutation.Type]++;
    }
    
    /// <summary>
    /// 移除宠物的变异
    /// </summary>
    public bool RemoveMutation(int petId, int mutationIndex)
    {
        if (!_petMutations.ContainsKey(petId))
            return false;
            
        var petData = _petMutations[petId];
        if (mutationIndex < 0 || mutationIndex >= petData.Mutations.Count)
            return false;
        
        petData.Mutations.RemoveAt(mutationIndex);
        petData.TotalMutations--;
        
        OnMutationRemoved?.Invoke(petId);
        return true;
    }
    
    /// <summary>
    /// 获取宠物的所有变异
    /// </summary>
    public List<PetMutation> GetPetMutations(int petId)
    {
        if (!_petMutations.ContainsKey(petId))
            return new List<PetMutation>();
        return _petMutations[petId].Mutations;
    }
    
    /// <summary>
    /// 获取宠物变异数据
    /// </summary>
    public PetMutationData GetPetMutationData(int petId)
    {
        if (!_petMutations.ContainsKey(petId))
            return null;
        return _petMutations[petId];
    }
    
    /// <summary>
    /// 计算宠物的变异属性加成
    /// </summary>
    public Dictionary<string, float> CalculateMutationBonuses(int petId)
    {
        var bonuses = new Dictionary<string, float>();
        var mutations = GetPetMutations(petId);
        
        foreach (var mutation in mutations)
        {
            if (!mutation.IsActive) continue;
            
            foreach (var kvp in mutation.StatBonuses)
            {
                if (!bonuses.ContainsKey(kvp.Key))
                    bonuses[kvp.Key] = 0;
                bonuses[kvp.Key] += kvp.Value;
            }
        }
        
        return bonuses;
    }
    
    /// <summary>
    /// 花费金币重新随机变异（如果失败）
    /// </summary>
    public bool RerollMutation(int petId, int mutationIndex, int cost = 100)
    {
        if (!_petMutations.ContainsKey(petId))
            return false;
        
        var petData = _petMutations[petId];
        if (mutationIndex < 0 || mutationIndex >= petData.Mutations.Count)
            return false;
        
        // 移除旧变异
        petData.Mutations.RemoveAt(mutationIndex);
        petData.TotalMutations--;
        
        // 生成新变异
        var newMutation = GenerateRandomMutation();
        petData.Mutations.Add(newMutation);
        petData.TotalMutations++;
        
        _rerollUsed++;
        
        OnMutationOccurred?.Invoke(petId, newMutation);
        return true;
    }
    
    /// <summary>
    /// 获取变异统计
    /// </summary>
    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            ["total_attempts"] = _totalMutationAttempts,
            ["successful_mutations"] = _successfulMutations,
            ["success_rate"] = _totalMutationAttempts > 0 
                ? (float)_successfulMutations / _totalMutationAttempts * 100 
                : 0,
            ["rerolls_used"] = _rerollUsed,
            ["total_pets_mutated"] = _petMutations.Count,
            ["total_mutations_applied"] = _successfulMutations
        };
    }
    
    private Dictionary<string, object> SerializePetMutationData(PetMutationData data)
    {
        var mutations = new List<Dictionary<string, object>>();
        foreach (var m in data.Mutations)
        {
            mutations.Add(new Dictionary<string, object>
            {
                ["id"] = m.MutationId,
                ["name"] = m.Name,
                ["desc"] = m.Description,
                ["type"] = m.Type,
                ["rarity"] = m.Rarity,
                ["stats"] = m.StatBonuses,
                ["abilities"] = m.AddedAbilities,
                ["visual"] = m.VisualEffect,
                ["time"] = m.MutatedAt.ToString("O"),
                ["active"] = m.IsActive
            });
        }
        
        var typeCounts = new Dictionary<string, object>();
        foreach (var kvp in data.MutationTypeCounts)
        {
            typeCounts[kvp.Key] = kvp.Value;
        }
        
        return new Dictionary<string, object>
        {
            ["mutations"] = mutations,
            ["total"] = data.TotalMutations,
            ["rare"] = data.RareMutations,
            ["legendary"] = data.LegendaryMutations,
            ["type_counts"] = typeCounts
        };
    }
    
    private PetMutationData DeserializePetMutationData(Dictionary<string, object> data)
    {
        if (data == null) return null;
        
        var petData = new PetMutationData();
        
        if (data.ContainsKey("total"))
            petData.TotalMutations = Convert.ToInt32(data["total"]);
        if (data.ContainsKey("rare"))
            petData.RareMutations = Convert.ToInt32(data["rare"]);
        if (data.ContainsKey("legendary"))
            petData.LegendaryMutations = Convert.ToInt32(data["legendary"]);
        
        petData.Mutations = new List<PetMutation>();
        if (data.ContainsKey("mutations"))
        {
            var mutations = data["mutations"] as List<object>;
            if (mutations != null)
            {
                foreach (var m in mutations)
                {
                    var md = m as Dictionary<string, object>;
                    if (md == null) continue;
                    
                    petData.Mutations.Add(new PetMutation
                    {
                        MutationId = md["id"]?.ToString() ?? "",
                        Name = md["name"]?.ToString() ?? "",
                        Description = md["desc"]?.ToString() ?? "",
                        Type = md["type"]?.ToString() ?? "",
                        Rarity = md["rarity"]?.ToString() ?? "",
                        StatBonuses = ParseStatBonuses(md["stats"] as Dictionary<string, object>),
                        AddedAbilities = ParseStringList(md["abilities"]),
                        VisualEffect = md["visual"]?.ToString() ?? "",
                        MutatedAt = md.ContainsKey("time") ? DateTime.Parse(md["time"].ToString()) : DateTime.UtcNow,
                        IsActive = md.ContainsKey("active") ? Convert.ToBoolean(md["active"]) : true
                    });
                }
            }
        }
        
        petData.MutationTypeCounts = new Dictionary<string, int>();
        if (data.ContainsKey("type_counts"))
        {
            var typeCounts = data["type_counts"] as Dictionary<string, object>;
            if (typeCounts != null)
            {
                foreach (var kvp in typeCounts)
                {
                    petData.MutationTypeCounts[kvp.Key] = Convert.ToInt32(kvp.Value);
                }
            }
        }
        
        return petData;
    }
    
    /// <summary>
    /// Export save data (BaseSystem override)
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        var petMutationsData = new Dictionary<string, object>();
        foreach (var kvp in _petMutations)
        {
            petMutationsData[kvp.Key.ToString()] = SerializePetMutationData(kvp.Value);
        }
        data["petMutations"] = petMutationsData;
        data["totalMutationAttempts"] = _totalMutationAttempts;
        data["successfulMutations"] = _successfulMutations;
        data["rerollUsed"] = _rerollUsed;
        return data;
    }
    
    /// <summary>
    /// Import save data (BaseSystem override)
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("petMutations"))
        {
            var petMutationsData = data["petMutations"] as Dictionary<string, object>;
            if (petMutationsData != null)
            {
                foreach (var kvp in petMutationsData)
                {
                    if (int.TryParse(kvp.Key, out int petId))
                    {
                        var petData = DeserializePetMutationData(kvp.Value as Dictionary<string, object>);
                        if (petData != null)
                        {
                            _petMutations[petId] = petData;
                        }
                    }
                }
            }
        }
        if (data.ContainsKey("totalMutationAttempts"))
        {
            _totalMutationAttempts = (int)data["totalMutationAttempts"];
        }
        if (data.ContainsKey("successfulMutations"))
        {
            _successfulMutations = (int)data["successfulMutations"];
        }
        if (data.ContainsKey("rerollUsed"))
        {
            _rerollUsed = (int)data["rerollUsed"];
        }
    }
}
