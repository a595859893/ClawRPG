using Godot;
using System;
using System.Collections.Generic;

public class PetTalentSystem : Node
{
    public static PetTalentSystem Instance { get; private set; }
    
    // Pet talent data storage (pet_id -> talent data)
    public Dictionary<int, PetTalentData> PetTalents { get; private set; }
    
    // Signal for talent changes
    public static signal PetTalentUpdated;
    public static signal TalentUnlocked;
    
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
            PetTalentUpdated?.Emit(petId);
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
        
        TalentUnlocked?.Emit(petId, talentId, data.UnlockedTalents[talentId]);
        PetTalentUpdated?.Emit(petId);
        
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
        
        PetTalentUpdated?.Emit(petId);
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
}
