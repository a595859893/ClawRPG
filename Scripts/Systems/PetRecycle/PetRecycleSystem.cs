using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.PetRecycle;

public partial class PetRecycleSystem : Node
{
    public static PetRecycleSystem Instance { get; private set; }
    
    private PetRecycleData _data = new();
    private Random _random = new();
    
    // 信号
    public signal void RecycleCompleted(RecycleRecord record);
    public signal void StatisticsUpdated(PetRecycleData data);
    
    public override void _Ready()
    {
        Instance = this;
        LoadData();
    }
    
    /// <summary>
    /// 回收宠物
    /// </summary>
    public RecycleRecord RecyclePet(string petId, string petName, string petType, string rarity, int level)
    {
        // 获取配置
        var config = PetRecycleDatabase.GetConfig(petType);
        var rarityBonus = PetRecycleDatabase.GetRarityBonus(rarity);
        float levelBonus = PetRecycleDatabase.GetLevelBonus(level);
        
        // 计算金币
        int goldEarned = (int)(config.BaseGold * rarityBonus.GoldMultiplier * levelBonus);
        
        // 生成材料
        List<string> materialsEarned = new();
        foreach (var material in config.Materials)
        {
            // 根据权重随机
            if (_random.Next(100) < material.Weight)
            {
                int amount = _random.Next(material.AmountMin, material.AmountMax + 1);
                // 稀有度加成
                amount += rarityBonus.MaterialBonus;
                
                string materialStr = $"{material.Name} x{amount}";
                materialsEarned.Add(materialStr);
                
                // 添加到背包（如果有背包系统）
                AddMaterialToInventory(material.MaterialId, amount);
            }
        }
        
        // 添加金币
        AddGold(goldEarned);
        
        // 创建记录
        var record = new RecycleRecord
        {
            PetId = petId,
            PetName = petName,
            PetType = petType,
            Rarity = rarity,
            Level = level,
            GoldEarned = goldEarned,
            MaterialsEarned = materialsEarned,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        
        // 更新统计
        _data.RecycleHistory.Insert(0, record);
        _data.TotalRecycled++;
        _data.TotalGoldEarned += goldEarned;
        _data.TotalMaterialsEarned += materialsEarned.Count;
        
        // 稀有度统计
        switch (rarity)
        {
            case "Rare":
                _data.RarePetsRecycled++;
                break;
            case "Epic":
                _data.EpicPetsRecycled++;
                break;
            case "Legendary":
                _data.LegendaryPetsRecycled++;
                break;
        }
        
        // 保存数据
        SaveData();
        
        // 触发信号
        RecycleCompleted(record);
        StatisticsUpdated(_data);
        
        GD.Print($"[PetRecycle] Recycled {petName} ({rarity}) - Gold: {goldEarned}, Materials: {materialsEarned.Count}");
        
        return record;
    }
    
    /// <summary>
    /// 获取回收预览
    /// </summary>
    public Dictionary<string, object> GetRecyclePreview(string petType, string rarity, int level)
    {
        var config = PetRecycleDatabase.GetConfig(petType);
        var rarityBonus = PetRecycleDatabase.GetRarityBonus(rarity);
        float levelBonus = PetRecycleDatabase.GetLevelBonus(level);
        
        int goldEarned = (int)(config.BaseGold * rarityBonus.GoldMultiplier * levelBonus);
        
        List<Dictionary<string, object>> materials = new();
        foreach (var material in config.Materials)
        {
            int amount = _random.Next(material.AmountMin, material.AmountMax + 1) + rarityBonus.MaterialBonus;
            materials.Add(new Dictionary<string, object>
            {
                { "name", material.Name },
                { "amount", amount },
                { "weight", material.Weight }
            });
        }
        
        return new Dictionary<string, object>
        {
            { "gold", goldEarned },
            { "materials", materials }
        };
    }
    
    /// <summary>
    /// 获取统计
    /// </summary>
    public PetRecycleData GetStatistics()
    {
        return _data;
    }
    
    /// <summary>
    /// 获取回收历史
    /// </summary>
    public List<RecycleRecord> GetHistory(int limit = 20)
    {
        if (_data.RecycleHistory.Count <= limit)
            return _data.RecycleHistory;
        
        return _data.RecycleHistory.GetRange(0, limit);
    }
    
    /// <summary>
    /// 重置统计
    /// </summary>
    public void ResetStatistics()
    {
        _data.TotalRecycled = 0;
        _data.TotalGoldEarned = 0;
        _data.TotalMaterialsEarned = 0;
        _data.RarePetsRecycled = 0;
        _data.EpicPetsRecycled = 0;
        _data.LegendaryPetsRecycled = 0;
        _data.RecycleHistory.Clear();
        
        SaveData();
        StatisticsUpdated(_data);
        
        GD.Print("[PetRecycle] Statistics reset");
    }
    
    // 添加金币到玩家
    private void AddGold(int amount)
    {
        // 尝试获取经济系统
        var economySystem = GetTree().GetFirstNodeInGroup("EconomySystem");
        if (economySystem != null)
        {
            economySystem.Call("AddGold", amount);
        }
        else
        {
            // 直接修改 Player 数据
            var player = GetTree().GetFirstNodeInGroup("Player");
            if (player != null && player.HasMethod("AddGold"))
            {
                player.Call("AddGold", amount);
            }
        }
    }
    
    // 添加材料到背包
    private void AddMaterialToInventory(string materialId, int amount)
    {
        var inventorySystem = GetTree().GetFirstNodeInGroup("InventorySystem");
        if (inventorySystem != null)
        {
            inventorySystem.Call("AddItem", materialId, amount);
        }
    }
    
    // 保存数据
    private void SaveData()
    {
        var saveSystem = GetTree().GetFirstNodeInGroup("SaveSystem");
        if (saveSystem != null)
        {
            var saveData = _data.GetSaveData();
            saveSystem.Call("SaveSystemData", "PetRecycle", saveData);
        }
    }
    
    // 加载数据
    private void LoadData()
    {
        var saveSystem = GetTree().GetFirstNodeInGroup("SaveSystem");
        if (saveSystem != null)
        {
            var data = saveSystem.Call("LoadSystemData", "PetRecycle") as Dictionary<string, int>;
            if (data != null)
            {
                _data.LoadFromData(data);
            }
        }
        
        GD.Print($"[PetRecycle] Loaded - Total Recycled: {_data.TotalRecycled}, Gold: {_data.TotalGoldEarned}");
    }
}
