using Godot;
using System;
using System.Collections.Generic;

public class PetTalentSystem : BaseSystem
{
    public static PetTalentSystem Instance { get; private set; }
    
    // Pet talent data storage (pet_id -> talent data)
    public Dictionary<int, PetTalentData> PetTalents { get; private set; }
    
    // Signal for talent changes
    public static Action<int> PetTalentUpdated;
    public static Action<int, string, int> TalentUnlocked;
    
    public override void _Ready()
    {
        Instance = this;
        PetTalents = new Dictionary<int, PetTalentData>();
    }
    
    public PetTalentData GetOrCreatePetTalentData(int petId)
    {
        if (!PetTalents.ContainsKey(petId))
        {
            PetTalents[petId] = new PetTalentData();
        }
        return PetTalents[petId];
    }
    
    public void UpdatePetLevel(int petId, int newLevel)
    {
        var data = GetOrCreatePetTalentData(petId);
        int newTotalPoints = PetTalentDatabase.Instance.GetTotalTalentPointsForLevel(newLevel);
        
        if (newTotalPoints > data.TotalPointsEarned)
        {
            int pointsToAdd = newTotalPoints - data.TotalPointsEarned;
            data.TotalPointsEarned = newTotalPoints;
            data.AvailablePoints += pointsToAdd;
            PetTalentUpdated?.Invoke(petId);
        }
    }
    
    public bool CanAllocateTalent(int petId, string talentId)
    {
        var data = GetOrCreatePetTalentData(petId);
        var talent = PetTalentDatabase.Instance.GetTalent(talentId);
        
        if (talent == null || data.AvailablePoints < talent.PointsPerLevel)
            return false;
        
        // Check if already at max level
        if (data.UnlockedTalents.ContainsKey(talentId))
        {
            if (data.UnlockedTalents[talentId] >= talent.MaxLevel)
                return false;
        }
        
        return true;
    }
    
    public bool AllocateTalent(int petId, string talentId)
    {
        if (!CanAllocateTalent(petId, talentId))
            return false;
        
        var data = GetOrCreatePetTalentData(petId);
        var talent = PetTalentDatabase.Instance.GetTalent(talentId);
        
        // Allocate talent
        if (!data.UnlockedTalents.ContainsKey(talentId))
        {
            data.UnlockedTalents[talentId] = 0;
        }
        
        data.UnlockedTalents[talentId]++;
        data.AvailablePoints -= talent.PointsPerLevel;
        data.TotalPointsSpent += talent.PointsPerLevel;
        
        // Update category allocated points
        if (!data.AllocatedPoints.ContainsKey(talent.Category))
        {
            data.AllocatedPoints[talent.Category] = 0;
        }
        data.AllocatedPoints[talent.Category] += talent.PointsPerLevel;
        
        TalentUnlocked?.Invoke(petId, talentId, data.UnlockedTalents[talentId]);
        PetTalentUpdated?.Invoke(petId);
        
        return true;
    }
    
    public bool CanResetTalent(int petId)
    {
        var data = GetOrCreatePetTalentData(petId);
        return data.TotalPointsSpent > 0;
    }
    
    public bool ResetTalent(int petId)
    {
        var data = GetOrCreatePetTalentData(petId);
        if (!CanResetTalent(petId))
            return false;
        
        // Refund all points
        data.AvailablePoints += data.TotalPointsSpent;
        data.TotalPointsSpent = 0;
        data.UnlockedTalents.Clear();
        data.AllocatedPoints.Clear();
        
        PetTalentUpdated?.Invoke(petId);
        return true;
    }
    
    public Dictionary<string, float> CalculateTalentBonuses(int petId)
    {
        var bonuses = new Dictionary<string, float>
        {
            { "attack", 0f },
            { "defense", 0f },
            { "health", 0f },
            { "speed", 0f },
            { "critRate", 0f },
            { "critDamage", 0f },
            { "lifeSteal", 0f },
            { "dodge", 0f }
        };
        
        var data = GetOrCreatePetTalentData(petId);
        
        foreach (var kvp in data.UnlockedTalents)
        {
            var talent = PetTalentDatabase.Instance.GetTalent(kvp.Key);
            if (talent != null)
            {
                int level = kvp.Value;
                bonuses["attack"] += talent.AttackBonus * level;
                bonuses["defense"] += talent.DefenseBonus * level;
                bonuses["health"] += talent.HealthBonus * level;
                bonuses["speed"] += talent.SpeedBonus * level;
                bonuses["critRate"] += talent.CritRateBonus * level;
                bonuses["critDamage"] += talent.CritDamageBonus * level;
                bonuses["lifeSteal"] += talent.LifeStealBonus * level;
                bonuses["dodge"] += talent.DodgeBonus * level;
            }
        }
        
        return bonuses;
    }
    
    public Dictionary<string, object> GetPetTalentInfo(int petId)
    {
        var data = GetOrCreatePetTalentData(petId);
        var bonuses = CalculateTalentBonuses(petId);
        
        return new Dictionary<string, object>
        {
            { "availablePoints", data.AvailablePoints },
            { "totalEarned", data.TotalPointsEarned },
            { "totalSpent", data.TotalPointsSpent },
            { "unlockedTalents", new Dictionary<string, int>(data.UnlockedTalents) },
            { "bonuses", bonuses }
        };
    }
    
    public Dictionary<string, int> GetCategoryPoints(int petId)
    {
        var data = GetOrCreatePetTalentData(petId);
        return new Dictionary<string, int>(data.AllocatedPoints);
    }
    
    // Save/Load
    public Dictionary<int, Dictionary<string, object>> SaveData()
    {
        var saveData = new Dictionary<int, Dictionary<string, object>>();
        
        foreach (var kvp in PetTalents)
        {
            var petData = new Dictionary<string, object>
            {
                { "availablePoints", kvp.Value.AvailablePoints },
                { "totalEarned", kvp.Value.TotalPointsEarned },
                { "totalSpent", kvp.Value.TotalPointsSpent },
                { "unlockedTalents", kvp.Value.UnlockedTalents },
                { "allocatedPoints", kvp.Value.AllocatedPoints }
            };
            saveData[kvp.Key] = petData;
        }
        
        return saveData;
    }
    
    public void LoadData(Dictionary<int, Dictionary<string, object>> saveData)
    {
        PetTalents.Clear();
        
        foreach (var kvp in saveData)
        {
            var petData = new PetTalentData
            {
                AvailablePoints = (int)kvp.Value["availablePoints"],
                TotalPointsEarned = (int)kvp.Value["totalEarned"],
                TotalPointsSpent = (int)kvp.Value["totalSpent"]
            };
            
            // Load unlocked talents
            var unlockedTalents = (Dictionary<string, object>)kvp.Value["unlockedTalents"];
            foreach (var talentKvp in unlockedTalents)
            {
                petData.UnlockedTalents[talentKvp.Key] = (int)talentKvp.Value;
            }
            
            // Load allocated points
            var allocatedPoints = (Dictionary<string, object>)kvp.Value["allocatedPoints"];
            foreach (var allocKvp in allocatedPoints)
            {
                petData.AllocatedPoints[allocKvp.Key] = (int)allocKvp.Value;
            }
            
            PetTalents[kvp.Key] = petData;
        }
    }

    public override Dictionary ExportSaveData()
    {
        var saveData = new Godot.Collections.Dictionary();

        foreach (var kvp in PetTalents)
        {
            var petData = new Godot.Collections.Dictionary
            {
                ["availablePoints"] = kvp.Value.AvailablePoints,
                ["totalEarned"] = kvp.Value.TotalPointsEarned,
                ["totalSpent"] = kvp.Value.TotalPointsSpent,
                ["unlockedTalents"] = new Godot.Collections.Dictionary(kvp.Value.UnlockedTalents),
                ["allocatedPoints"] = new Godot.Collections.Dictionary(kvp.Value.AllocatedPoints)
            };
            saveData[kvp.Key.ToString()] = petData;
        }

        return saveData;
    }

    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;

        PetTalents.Clear();

        foreach (var kvp in (Godot.Collections.Dictionary)data)
        {
            int petId = Convert.ToInt32(kvp.Key);
            var petDataDict = (Godot.Collections.Dictionary)kvp.Value;

            var petData = new PetTalentData
            {
                AvailablePoints = Convert.ToInt32(petDataDict.GetValueOrDefault("availablePoints", 0)),
                TotalPointsEarned = Convert.ToInt32(petDataDict.GetValueOrDefault("totalEarned", 0)),
                TotalPointsSpent = Convert.ToInt32(petDataDict.GetValueOrDefault("totalSpent", 0))
            };

            if (petDataDict.TryGetValue("unlockedTalents", out var unlockedObj) && unlockedObj is Godot.Collections.Dictionary unlocked)
            {
                foreach (string talentId in unlocked.Keys)
                {
                    petData.UnlockedTalents[talentId] = Convert.ToInt32(unlocked[talentId]);
                }
            }

            if (petDataDict.TryGetValue("allocatedPoints", out var allocatedObj) && allocatedObj is Godot.Collections.Dictionary allocated)
            {
                foreach (string category in allocated.Keys)
                {
                    petData.AllocatedPoints[category] = Convert.ToInt32(allocated[category]);
                }
            }

            PetTalents[petId] = petData;
        }
    }
    
    // 导出单个宠物的天赋数据（供 PetManager 使用）
    public Dictionary<string, object> ExportPetTalentData(string petId)
    {
        int key = petId.GetHashCode();
        if (PetTalents.TryGetValue(key, out var talentData))
        {
            return new Dictionary<string, object>
            {
                { "availablePoints", talentData.AvailablePoints },
                { "totalEarned", talentData.TotalPointsEarned },
                { "totalSpent", talentData.TotalPointsSpent },
                { "unlockedTalents", new Dictionary<string, int>(talentData.UnlockedTalents) },
                { "allocatedPoints", new Dictionary<string, int>(talentData.AllocatedPoints) }
            };
        }
        return null;
    }
    
    // 导入单个宠物的天赋数据（供 PetManager 使用）
    public void ImportPetTalentData(string petId, Dictionary talentData)
    {
        if (talentData == null) return;
        
        int key = petId.GetHashCode();
        var data = new PetTalentData
        {
            AvailablePoints = Convert.ToInt32(talentData.GetValueOrDefault("availablePoints", 0)),
            TotalPointsEarned = Convert.ToInt32(talentData.GetValueOrDefault("totalEarned", 0)),
            TotalPointsSpent = Convert.ToInt32(talentData.GetValueOrDefault("totalSpent", 0))
        };
        
        if (talentData.TryGetValue("unlockedTalents", out var unlockedObj) && unlockedObj is Dictionary unlocked)
        {
            foreach (string talentId in unlocked.Keys)
            {
                data.UnlockedTalents[talentId] = Convert.ToInt32(unlocked[talentId]);
            }
        }
        
        if (talentData.TryGetValue("allocatedPoints", out var allocatedObj) && allocatedObj is Dictionary allocated)
        {
            foreach (string category in allocated.Keys)
            {
                data.AllocatedPoints[category] = Convert.ToInt32(allocated[category]);
            }
        }
        
        PetTalents[key] = data;
    }

}
