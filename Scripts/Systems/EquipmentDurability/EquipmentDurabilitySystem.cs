using Godot;
using System;
using System.Collections.Generic;

public class EquipmentDurabilitySystem : BaseSystem
{
    public static EquipmentDurabilitySystem Instance { get; private set; }

    private EquipmentDurabilityData.PlayerDurabilityData _playerData = new EquipmentDurabilityData.PlayerDurabilityData();
    
    // 耐久度配置
    private Dictionary<string, int> _baseDurability = new Dictionary<string, int>()
    {
        // 武器
        { "sword_001", 100 }, { "sword_002", 120 }, { "sword_003", 150 },
        { "axe_001", 110 }, { "axe_002", 130 }, { "axe_003", 160 },
        { "bow_001", 90 }, { "bow_002", 110 }, { "bow_003", 140 },
        { "staff_001", 80 }, { "staff_002", 100 }, { "staff_003", 130 },
        
        // 防具
        { "helmet_001", 80 }, { "helmet_002", 100 }, { "helmet_003", 130 },
        { "chest_001", 120 }, { "chest_002", 150 }, { "chest_003", 180 },
        { "leggings_001", 100 }, { "leggings_002", 120 }, { "leggings_003", 150 },
        { "boots_001", 80 }, { "boots_002", 100 }, { "boots_003", 130 },
        { "shield_001", 150 }, { "shield_002", 180 }, { "shield_003", 220 },
        
        // 饰品
        { "ring_001", 50 }, { "ring_002", 70 }, { "ring_003", 90 },
        { "amulet_001", 50 }, { "amulet_002", 70 }, { "amulet_003", 90 },
    };

    // 每次攻击/受到伤害时装备耐久度损失
    private int _damageOnAttack = 1;
    private int _damageOnHit = 2;
    private int _damageOnCriticalHit = 3;

    public override void _Ready()
    {
        Instance = this;
        
        // Load saved durability data
        LoadData();
    }
    
    private void LoadData()
    {
        try
        {
            var saveSystem = SaveSystem.Instance;
            if (saveSystem != null)
            {
                var data = saveSystem.LoadEquipmentDurabilityData();
                if (data != null && data.Count > 0)
                {
                    LoadSaveData(data);
                    GD.Print("[EquipmentDurabilitySystem] Loaded durability data");
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintErr("[EquipmentDurabilitySystem] Failed to load data: " + e.Message);
        }
    }
    
    private void SaveData()
    {
        try
        {
            var saveSystem = SaveSystem.Instance;
            if (saveSystem != null)
            {
                saveSystem.SaveEquipmentDurabilityData(GetSaveData());
            }
        }
        catch (Exception e)
        {
            GD.PrintErr("[EquipmentDurabilitySystem] Failed to save data: " + e.Message);
        }
    }

    public EquipmentDurabilityData.EquipmentDurability GetEquipmentDurability(string itemId)
    {
        if (_playerData.EquippedDurability.ContainsKey(itemId))
        {
            return _playerData.EquippedDurability[itemId];
        }
        return null;
    }

    public void InitializeEquipmentDurability(string itemId, int rarity)
    {
        int baseDur = 50;
        if (_baseDurability.ContainsKey(itemId))
        {
            baseDur = _baseDurability[itemId];
        }
        
        // 稀有度加成：优秀x1.2, 稀有x1.5, 史诗x2, 传说x3
        float rarityMultiplier = rarity switch
        {
            1 => 1.2f,  // 优秀
            2 => 1.5f,  // 稀有
            3 => 2.0f,  // 史诗
            4 => 3.0f,  // 传说
            _ => 1.0f   // 普通
        };

        int maxDur = (int)(baseDur * rarityMultiplier);

        var durability = new EquipmentDurabilityData.EquipmentDurability
        {
            ItemId = itemId,
            CurrentDurability = maxDur,
            MaxDurability = maxDur
        };

        _playerData.EquippedDurability[itemId] = durability;
    }

    public void OnPlayerAttack()
    {
        // 玩家攻击时，主武器损失耐久度
        if (Player.Instance != null && Player.Instance.Equipment != null)
        {
            var weapon = Player.Instance.Equipment.GetCurrentWeapon();
            if (weapon != null)
            {
                ReduceDurability(weapon.Id, _damageOnAttack);
            }
        }
    }

    public void OnPlayerHit()
    {
        // 玩家受到攻击时，防具损失耐久度
        if (Player.Instance != null && Player.Instance.Equipment != null)
        {
            var helmet = Player.Instance.Equipment.GetEquipment("helmet");
            var chest = Player.Instance.Equipment.GetEquipment("chest");
            var leggings = Player.Instance.Equipment.GetEquipment("leggings");
            var boots = Player.Instance.Equipment.GetEquipment("boots");
            var shield = Player.Instance.Equipment.GetEquipment("shield");

            if (helmet != null) ReduceDurability(helmet.Id, _damageOnHit);
            if (chest != null) ReduceDurability(chest.Id, _damageOnHit);
            if (leggings != null) ReduceDurability(leggings.Id, _damageOnHit);
            if (boots != null) ReduceDurability(boots.Id, _damageOnHit);
            if (shield != null) ReduceDurability(shield.Id, _damageOnHit);
        }
    }

    public void OnPlayerCriticalHit()
    {
        // 暴击时耐久度损失更多
        if (Player.Instance != null && Player.Instance.Equipment != null)
        {
            var weapon = Player.Instance.Equipment.GetCurrentWeapon();
            if (weapon != null)
            {
                ReduceDurability(weapon.Id, _damageOnCriticalHit);
            }
        }
    }

    public void ReduceDurability(string itemId, int amount)
    {
        if (!_playerData.EquippedDurability.ContainsKey(itemId))
        {
            // 自动创建耐久度数据
            InitializeEquipmentDurability(itemId, 0);
        }

        var durability = _playerData.EquippedDurability[itemId];
        durability.CurrentDurability = Mathf.Max(0, durability.CurrentDurability - amount);

        // 发送信号更新UI
        EmitSignal(nameof(DurabilityChanged), itemId, durability.CurrentDurability, durability.MaxDurability);
        
        // 如果装备损坏，发送信号
        if (durability.CurrentDurability == 0)
        {
            EmitSignal(nameof(EquipmentBroken), itemId);
        }
        
        // 自动保存
        SaveData();
    }

    public bool RepairEquipment(string itemId, int cost)
    {
        if (!_playerData.EquippedDurability.ContainsKey(itemId))
        {
            return false;
        }

        var durability = _playerData.EquippedDurability[itemId];
        
        if (Player.Instance != null && Player.Instance.Gold >= cost)
        {
            Player.Instance.Gold -= cost;
            durability.CurrentDurability = durability.MaxDurability;
            
            _playerData.TotalRepairs++;
            _playerData.TotalRepairCost += cost;

            EmitSignal(nameof(EquipmentRepaired), itemId, cost);
            return true;
        }
        
        return false;
    }

    public bool RepairAllEquipment(int costPerItem)
    {
        if (Player.Instance == null) return false;

        int totalCost = 0;
        int repairedCount = 0;

        foreach (var kvp in _playerData.EquippedDurability)
        {
            if (kvp.Value.CurrentDurability < kvp.Value.MaxDurability)
            {
                int cost = (int)(costPerItem * (1.0f - kvp.Value.DurabilityPercent));
                totalCost += cost;
                repairedCount++;
            }
        }

        if (totalCost > 0 && Player.Instance.Gold >= totalCost)
        {
            Player.Instance.Gold -= totalCost;
            
            foreach (var kvp in _playerData.EquippedDurability)
            {
                if (kvp.Value.CurrentDurability < kvp.Value.MaxDurability)
                {
                    kvp.Value.CurrentDurability = kvp.Value.MaxDurability;
                    EmitSignal(nameof(EquipmentRepaired), kvp.Key, costPerItem);
                }
            }
            
            _playerData.TotalRepairs += repairedCount;
            _playerData.TotalRepairCost += totalCost;
            
            return true;
        }
        
        return false;
    }

    public int GetRepairCost(string itemId)
    {
        if (!_playerData.EquippedDurability.ContainsKey(itemId))
        {
            return 0;
        }

        var durability = _playerData.EquippedDurability[itemId];
        float percentMissing = 1.0f - durability.DurabilityPercent;
        return (int)(100 * percentMissing); // 每1%耐久度需要1金币
    }

    public int GetTotalRepairCost()
    {
        int total = 0;
        foreach (var kvp in _playerData.EquippedDurability)
        {
            total += GetRepairCost(kvp.Key);
        }
        return total;
    }

    public Dictionary<string, EquipmentDurabilityData.EquipmentDurability> GetAllDurability()
    {
        return _playerData.EquippedDurability;
    }

    public Dictionary<string, object> GetSaveData()
    {
        var data = new Dictionary<string, object>();
        var durabilityList = new List<Dictionary<string, object>>();

        foreach (var kvp in _playerData.EquippedDurability)
        {
            durabilityList.Add(new Dictionary<string, object>
            {
                { "item_id", kvp.Key },
                { "current", kvp.Value.CurrentDurability },
                { "max", kvp.Value.MaxDurability }
            });
        }

        data["equipped_durability"] = durabilityList;
        data["total_repairs"] = _playerData.TotalRepairs;
        data["total_repair_cost"] = _playerData.TotalRepairCost;
        data["times_used_repair_kit"] = _playerData.TimesUsedRepairKit;

        return data;
    }

    public void LoadSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        _playerData.EquippedDurability.Clear();

        if (data.ContainsKey("equipped_durability"))
        {
            var durabilityList = (List<object>)data["equipped_durability"];
            foreach (Dictionary<string, object> item in durabilityList)
            {
                var durability = new EquipmentDurabilityData.EquipmentDurability
                {
                    ItemId = (string)item["item_id"],
                    CurrentDurability = (int)item["current"],
                    MaxDurability = (int)item["max"]
                };
                _playerData.EquippedDurability[durability.ItemId] = durability;
            }
        }

        if (data.ContainsKey("total_repairs"))
            _playerData.TotalRepairs = (int)data["total_repairs"];
        if (data.ContainsKey("total_repair_cost"))
            _playerData.TotalRepairCost = (int)data["total_repair_cost"];
        if (data.ContainsKey("times_used_repair_kit"))
            _playerData.TimesUsedRepairKit = (int)data["times_used_repair_kit"];
    }

    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            { "total_repairs", _playerData.TotalRepairs },
            { "total_repair_cost", _playerData.TotalRepairCost },
            { "times_used_repair_kit", _playerData.TimesUsedRepairKit },
            { "total_equipped", _playerData.EquippedDurability.Count }
        };
    }

    // 信号
    [Signal]
    public delegate void DurabilityChangedEventHandler(string itemId, int current, int max);
    
    [Signal]
    public delegate void EquipmentBrokenEventHandler(string itemId);
    
    [Signal]
    public delegate void EquipmentRepairedEventHandler(string itemId, int cost);
    
    // ===== 持久化 =====
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        // 保存装备耐久度
        var durabilityList = new Array();
        foreach (var kvp in _playerData.EquippedDurability)
        {
            var itemData = new Dictionary();
            itemData["item_id"] = kvp.Key;
            itemData["current"] = kvp.Value.CurrentDurability;
            itemData["max"] = kvp.Value.MaxDurability;
            durabilityList.Add(itemData);
        }
        data["equipped_durability"] = durabilityList;
        
        // 保存统计数据
        data["total_repairs"] = _playerData.TotalRepairs;
        data["total_repair_cost"] = _playerData.TotalRepairCost;
        data["times_used_repair_kit"] = _playerData.TimesUsedRepairKit;
        
        return data;
    }
    
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        // 恢复装备耐久度
        _playerData.EquippedDurability.Clear();
        if (data.ContainsKey("equipped_durability"))
        {
            var durabilityList = (Array)data["equipped_durability"];
            foreach (Dictionary itemData in durabilityList)
            {
                var durability = new EquipmentDurabilityData.EquipmentDurability
                {
                    ItemId = (string)itemData["item_id"],
                    CurrentDurability = Convert.ToInt32(itemData["current"]),
                    MaxDurability = Convert.ToInt32(itemData["max"])
                };
                _playerData.EquippedDurability[durability.ItemId] = durability;
            }
        }
        
        // 恢复统计数据
        if (data.ContainsKey("total_repairs"))
            _playerData.TotalRepairs = Convert.ToInt32(data["total_repairs"]);
        if (data.ContainsKey("total_repair_cost"))
            _playerData.TotalRepairCost = Convert.ToInt32(data["total_repair_cost"]);
        if (data.ContainsKey("times_used_repair_kit"))
            _playerData.TimesUsedRepairKit = Convert.ToInt32(data["times_used_repair_kit"]);
    }
}
