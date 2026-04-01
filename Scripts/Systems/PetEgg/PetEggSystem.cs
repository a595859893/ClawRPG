using Godot;
using System;
using System.Collections.Generic;

public partial class PetEggSystem : BaseSystem
{
    public static PetEggSystem Instance;
    
    // Player's egg inventory
    private Dictionary<string, PetEgg> ownedEggs = new Dictionary<string, PetEgg>();
    
    // Statistics
    private int totalEggsHatched = 0;
    private int totalGoldSpent = 0;
    private Dictionary<int, int> hatchCountByRarity = new Dictionary<int, int>();
    private Dictionary<string, int> petTypeHatchCount = new Dictionary<string, int>();
    
    public override void _Ready()
    {
        Instance = this;
        PetEggDatabase.Initialize();
    }
    
    public void LoadData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        ownedEggs.Clear();
        if (data.ContainsKey("ownedEggs"))
        {
            var eggsList = (Dictionary<string, object>)data["ownedEggs"];
            foreach (var kvp in eggsList)
            {
                var eggData = (Dictionary<string, object>)kvp.Value;
                var egg = new PetEgg();
                egg.eggId = (string)eggData["eggId"];
                egg.acquireTime = (float)eggData["acquireTime"];
                egg.hatchStartTime = (float)eggData["hatchStartTime"];
                egg.isHatching = (bool)eggData["isHatching"];
                egg.isHatched = (bool)eggData["isHatched"];
                ownedEggs[kvp.Key] = egg;
            }
        }
        
        totalEggsHatched = data.ContainsKey("totalEggsHatched") ? (int)data["totalEggsHatched"] : 0;
        totalGoldSpent = data.ContainsKey("totalGoldSpent") ? (int)data["totalGoldSpent"] : 0;
        
        hatchCountByRarity.Clear();
        if (data.ContainsKey("hatchCountByRarity"))
        {
            var dict = (Dictionary<string, object>)data["hatchCountByRarity"];
            foreach (var kvp in dict)
            {
                hatchCountByRarity[int.Parse(kvp.Key)] = (int)kvp.Value;
            }
        }
        
        petTypeHatchCount.Clear();
        if (data.ContainsKey("petTypeHatchCount"))
        {
            var dict = (Dictionary<string, object>)data["petTypeHatchCount"];
            foreach (var kvp in dict)
            {
                petTypeHatchCount[kvp.Key] = (int)kvp.Value;
            }
        }
    }
    
    public Dictionary<string, object> SaveData()
    {
        var data = new Dictionary<string, object>();
        
        var eggsData = new Dictionary<string, object>();
        foreach (var kvp in ownedEggs)
        {
            var eggData = new Dictionary<string, object>();
            eggData["eggId"] = kvp.Value.eggId;
            eggData["acquireTime"] = kvp.Value.acquireTime;
            eggData["hatchStartTime"] = kvp.Value.hatchStartTime;
            eggData["isHatching"] = kvp.Value.isHatching;
            eggData["isHatched"] = kvp.Value.isHatched;
            eggsData[kvp.Key] = eggData;
        }
        data["ownedEggs"] = eggsData;
        data["totalEggsHatched"] = totalEggsHatched;
        data["totalGoldSpent"] = totalGoldSpent;
        
        var rarityData = new Dictionary<string, object>();
        foreach (var kvp in hatchCountByRarity)
        {
            rarityData[kvp.Key.ToString()] = kvp.Value;
        }
        data["hatchCountByRarity"] = rarityData;
        
        data["petTypeHatchCount"] = petTypeHatchCount;
        
        return data;
    }
    
    // Add egg to inventory
    public bool AddEgg(string eggId, int count = 1)
    {
        var eggData = PetEggDatabase.GetEgg(eggId);
        if (eggData == null) return false;
        
        for (int i = 0; i < count; i++)
        {
            string uniqueId = Guid.NewGuid().ToString();
            var egg = new PetEgg();
            egg.eggId = eggId;
            egg.acquireTime = OS.GetSystemTimeMsecs() / 1000f;
            egg.isHatching = false;
            egg.isHatched = false;
            ownedEggs[uniqueId] = egg;
        }
        
        return true;
    }
    
    // Start hatching an egg
    public bool StartHatching(string uniqueId)
    {
        if (!ownedEggs.ContainsKey(uniqueId)) return false;
        
        var egg = ownedEggs[uniqueId];
        var eggData = PetEggDatabase.GetEgg(egg.eggId);
        
        if (egg == null || eggData == null) return false;
        if (egg.isHatching || egg.isHatched) return false;
        
        // Check gold
        if (Player.Instance.Gold < eggData.goldCost)
        {
            return false;
        }
        
        // Deduct gold
        Player.Instance.Gold -= eggData.goldCost;
        totalGoldSpent += eggData.goldCost;
        
        // Start hatching
        egg.isHatching = true;
        egg.hatchStartTime = OS.GetSystemTimeMsecs() / 1000f;
        
        return true;
    }
    
    // Check if egg is ready to hatch
    public bool IsEggReadyToHatch(string uniqueId)
    {
        if (!ownedEggs.ContainsKey(uniqueId)) return false;
        
        var egg = ownedEggs[uniqueId];
        var eggData = PetEggDatabase.GetEgg(egg.eggId);
        
        if (egg == null || eggData == null) return false;
        if (!egg.isHatching || egg.isHatched) return false;
        
        float currentTime = OS.GetSystemTimeMsecs() / 1000f;
        float elapsed = currentTime - egg.hatchStartTime;
        
        return elapsed >= eggData.hatchTimeSeconds;
    }
    
    // Get hatch progress (0.0 to 1.0)
    public float GetHatchProgress(string uniqueId)
    {
        if (!ownedEggs.ContainsKey(uniqueId)) return 0f;
        
        var egg = ownedEggs[uniqueId];
        var eggData = PetEggDatabase.GetEgg(egg.eggId);
        
        if (egg == null || eggData == null || !egg.isHatching || egg.isHatched) return 0f;
        
        float currentTime = OS.GetSystemTimeMsecs() / 1000f;
        float elapsed = currentTime - egg.hatchStartTime;
        
        return Mathf.Clamp(elapsed / eggData.hatchTimeSeconds, 0f, 1f);
    }
    
    // Get remaining hatch time in seconds
    public int GetRemainingHatchTime(string uniqueId)
    {
        if (!ownedEggs.ContainsKey(uniqueId)) return 0;
        
        var egg = ownedEggs[uniqueId];
        var eggData = PetEggDatabase.GetEgg(egg.eggId);
        
        if (egg == null || eggData == null || !egg.isHatching || egg.isHatched) return 0;
        
        float currentTime = OS.GetSystemTimeMsecs() / 1000f;
        float elapsed = currentTime - egg.hatchStartTime;
        float remaining = eggData.hatchTimeSeconds - elapsed;
        
        return Mathf.Max(0, (int)remaining);
    }
    
    // Hatch the egg and get the pet
    public int? HatchEgg(string uniqueId)
    {
        if (!ownedEggs.ContainsKey(uniqueId)) return null;
        
        var egg = ownedEggs[uniqueId];
        var eggData = PetEggDatabase.GetEgg(egg.eggId);
        
        if (egg == null || eggData == null) return null;
        if (!egg.isHatching || egg.isHatched) return null;
        
        // Check if enough time has passed
        float currentTime = OS.GetSystemTimeMsecs() / 1000f;
        float elapsed = currentTime - egg.hatchStartTime;
        if (elapsed < eggData.hatchTimeSeconds) return null;
        
        // Select random pet based on weights
        int petId = SelectRandomPet(eggData);
        
        // Mark as hatched
        egg.isHatched = true;
        
        // Update statistics
        totalEggsHatched++;
        if (!hatchCountByRarity.ContainsKey(eggData.rarity))
            hatchCountByRarity[eggData.rarity] = 0;
        hatchCountByRarity[eggData.rarity]++;
        
        if (!petTypeHatchCount.ContainsKey(eggData.petType))
            petTypeHatchCount[eggData.petType] = 0;
        petTypeHatchCount[eggData.petType]++;
        
        // Remove egg from inventory
        ownedEggs.Remove(uniqueId);
        
        // Add pet to player's pet system
        if (PetManager.Instance != null)
        {
            PetManager.Instance.AddPet(petId);
        }
        
        return petId;
    }
    
    private int SelectRandomPet(PetEggData eggData)
    {
        if (eggData.possiblePetIds.Length == 0) return 1;
        if (eggData.possiblePetIds.Length == 1) return (int)eggData.possiblePetIds[0];
        
        float totalWeight = 0;
        foreach (float w in eggData.possiblePetWeights)
            totalWeight += w;
        
        float random = (float)GD.Rand() * totalWeight;
        float cumulative = 0;
        
        for (int i = 0; i < eggData.possiblePetIds.Length; i++)
        {
            cumulative += eggData.possiblePetWeights[i];
            if (random <= cumulative)
                return (int)eggData.possiblePetIds[i];
        }
        
        return (int)eggData.possiblePetIds[eggData.possiblePetIds.Length - 1];
    }
    
    // Get all owned eggs
    public Dictionary<string, PetEgg> GetOwnedEggs()
    {
        return ownedEggs;
    }
    
    // Get hatching eggs
    public List<KeyValuePair<string, PetEgg>> GetHatchingEggs()
    {
        List<KeyValuePair<string, PetEgg>> result = new List<KeyValuePair<string, PetEgg>>();
        foreach (var kvp in ownedEggs)
        {
            if (kvp.Value.isHatching && !kvp.Value.isHatched)
                result.Add(kvp);
        }
        return result;
    }
    
    // Get ready eggs
    public List<KeyValuePair<string, PetEgg>> GetReadyEggs()
    {
        List<KeyValuePair<string, PetEgg>> result = new List<KeyValuePair<string, PetEgg>>();
        foreach (var kvp in ownedEggs)
        {
            if (kvp.Value.isHatching && !kvp.Value.isHatched && IsEggReadyToHatch(kvp.Key))
                result.Add(kvp);
        }
        return result;
    }
    
    // Get egg info
    public PetEgg GetEgg(string uniqueId)
    {
        if (ownedEggs.ContainsKey(uniqueId))
            return ownedEggs[uniqueId];
        return null;
    }
    
    // Get statistics
    public int GetTotalEggsHatched() => totalEggsHatched;
    public int GetTotalGoldSpent() => totalGoldSpent;
    public int GetHatchCountByRarity(int rarity) => hatchCountByRarity.ContainsKey(rarity) ? hatchCountByRarity[rarity] : 0;
    public int GetPetTypeHatchCount(string petType) => petTypeHatchCount.ContainsKey(petType) ? petTypeHatchCount[petType] : 0;
    
    // Check if player has any eggs
    public bool HasEggs()
    {
        return ownedEggs.Count > 0;
    }
    
    // Get egg count
    public int GetEggCount()
    {
        return ownedEggs.Count;
    }

    #region Data Types

    public class PetEgg
    {
        public string eggId = "";
        public float acquireTime = 0;
        public float hatchStartTime = 0;
        public bool isHatching = false;
        public bool isHatched = false;
    }

    #endregion

    #region Persistence

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();

        // 拥有的蛋
        var eggsList = new Array();
        foreach (var kvp in ownedEggs)
        {
            var eggDict = new Dictionary
            {
                { "egg_id", kvp.Value.eggId },
                { "acquire_time", kvp.Value.acquireTime },
                { "hatch_start_time", kvp.Value.hatchStartTime },
                { "is_hatching", kvp.Value.isHatching },
                { "is_hatched", kvp.Value.isHatched }
            };
            eggsList.Add(eggDict);
        }
        data["owned_eggs"] = eggsList;

        // 统计数据
        data["total_eggs_hatched"] = totalEggsHatched;
        data["total_gold_spent"] = totalGoldSpent;

        return data;
    }

    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        // 拥有的蛋
        if (data.Contains("owned_eggs"))
        {
            ownedEggs = new Dictionary<string, PetEgg>();
            var eggsArray = (Array)data["owned_eggs"];
            foreach (Dictionary eggDict in eggsArray)
            {
                var egg = new PetEgg
                {
                    eggId = (string)eggDict["egg_id"],
                    acquireTime = (float)eggDict["acquire_time"],
                    hatchStartTime = (float)eggDict["hatch_start_time"],
                    isHatching = (bool)eggDict["is_hatching"],
                    isHatched = (bool)eggDict["is_hatched"]
                };
                ownedEggs[egg.eggId] = egg;
            }
        }

        // 统计数据
        totalEggsHatched = (int)data.GetValueOrDefault("total_eggs_hatched", 0);
        totalGoldSpent = (int)data.GetValueOrDefault("total_gold_spent", 0);
    }

    #endregion
}
