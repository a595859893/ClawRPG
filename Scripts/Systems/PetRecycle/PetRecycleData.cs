using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.PetRecycle;

/// <summary>
/// 宠物回收系统数据
/// </summary>
public class PetRecycleData : Node
{
    // 回收记录
    public List<RecycleRecord> RecycleHistory { get; set; } = new();
    
    // 统计
    public int TotalRecycled { get; set; } = 0;
    public int TotalGoldEarned { get; set; } = 0;
    public int TotalMaterialsEarned { get; set; } = 0;
    public int RarePetsRecycled { get; set; } = 0;
    public int EpicPetsRecycled { get; set; } = 0;
    public int LegendaryPetsRecycled { get; set; } = 0;
    
    // 已解锁的回收配方
    public HashSet<string> UnlockedRecipes { get; set; } = new();
    
    public Dictionary<string, int> GetSaveData()
    {
        var data = new Dictionary<string, int>
        {
            { "total_recycled", TotalRecycled },
            { "total_gold_earned", TotalGoldEarned },
            { "total_materials_earned", TotalMaterialsEarned },
            { "rare_pets_recycled", RarePetsRecycled },
            { "epic_pets_recycled", EpicPetsRecycled },
            { "legendary_pets_recycled", LegendaryPetsRecycled }
        };
        return data;
    }
    
    public void LoadFromData(Dictionary<string, int> data)
    {
        if (data == null) return;
        TotalRecycled = data.GetValueOrDefault("total_recycled", 0);
        TotalGoldEarned = data.GetValueOrDefault("total_gold_earned", 0);
        TotalMaterialsEarned = data.GetValueOrDefault("total_materials_earned", 0);
        RarePetsRecycled = data.GetValueOrDefault("rare_pets_recycled", 0);
        EpicPetsRecycled = data.GetValueOrDefault("epic_pets_recycled", 0);
        LegendaryPetsRecycled = data.GetValueOrDefault("legendary_pets_recycled", 0);
    }
}

/// <summary>
/// 宠物回收记录
/// </summary>
public class RecycleRecord
{
    public string PetId { get; set; } = "";
    public string PetName { get; set; } = "";
    public string PetType { get; set; } = "";
    public string Rarity { get; set; } = "";
    public int Level { get; set; } = 1;
    public int GoldEarned { get; set; } = 0;
    public List<string> MaterialsEarned { get; set; } = new();
    public long Timestamp { get; set; } = 0;
}
