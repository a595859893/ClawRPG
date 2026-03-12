using Godot;
using System;
using System.Collections.Generic;

public class EnchantmentSystem : Node
{
    private static EnchantmentSystem _instance;
    public static EnchantmentSystem Instance => _instance ?? (_instance = new EnchantmentSystem());

    // 玩家已解锁的附魔
    private HashSet<string> _unlockedEnchantments;

    // 当前应用的附魔实例 (equipment_id -> list of enchantments)
    private Dictionary<string, List<EnchantmentInstance>> _appliedEnchantments;

    // 附魔统计
    private int _totalEnchantments;
    private int _successfulEnchantments;
    private int _failedEnchantments;
    private int _totalGoldSpent;
    private Dictionary<string, int> _enchantmentUsageCount;

    // 附魔经验曲线
    private int[] _levelExperience = { 0, 100, 250, 500, 1000, 2000 };

    [Export]
    public bool IsEnabled { get; set; } = true;

    public override void _Ready()
    {
        _instance = this;
        _unlockedEnchantments = new HashSet<string>();
        _appliedEnchantments = new Dictionary<string, List<EnchantmentInstance>>();
        _enchantmentUsageCount = new Dictionary<string, int>();

        // 解锁基础附魔
        UnlockEnchantment("enchant_steel_armor");
        UnlockEnchantment("enchant_fire_weapon");
        UnlockEnchantment("enchant_ice_weapon");
        UnlockEnchantment("enchant_lucky_accessory");
        UnlockEnchantment("enchant_swift_boots");
    }

    // 解锁附魔
    public void UnlockEnchantment(string enchantmentId)
    {
        if (!_unlockedEnchantments.Contains(enchantmentId))
        {
            _unlockedEnchantments.Add(enchantmentId);
            GD.Print($"[Enchantment] Unlocked: {enchantmentId}");
        }
    }

    public bool IsUnlocked(string enchantmentId)
    {
        return _unlockedEnchantments.Contains(enchantmentId);
    }

    public List<EnchantmentData> GetUnlockedEnchantments()
    {
        var result = new List<EnchantmentData>();
        foreach (var id in _unlockedEnchantments)
        {
            var data = EnchantmentDatabase.Instance.GetEnchantment(id);
            if (data != null)
            {
                result.Add(data);
            }
        }
        return result;
    }

    // 应用附魔到装备
    public bool ApplyEnchantment(string equipmentId, string enchantmentId, int playerGold)
    {
        if (!IsEnabled) return false;

        var enchantment = EnchantmentDatabase.Instance.GetEnchantment(enchantmentId);
        if (enchantment == null)
        {
            GD.PrintErr($"[Enchantment] Enchantment not found: {enchantmentId}");
            return false;
        }

        if (!_unlockedEnchantments.Contains(enchantmentId))
        {
            GD.PrintErr($"[Enchantment] Enchantment not unlocked: {enchantmentId}");
            return false;
        }

        int cost = CalculateEnchantmentCost(enchantment);
        if (playerGold < cost)
        {
            GD.PrintErr($"[Enchantment] Not enough gold. Need: {cost}, Have: {playerGold}");
            return false;
        }

        // 检查是否已有该类型附魔
        if (_appliedEnchantments.ContainsKey(equipmentId))
        {
            var existing = _appliedEnchantments[equipmentId];
            bool hasSameType = false;
            foreach (var inst in existing)
            {
                var instData = EnchantmentDatabase.Instance.GetEnchantment(inst.TemplateId);
                if (instData != null && instData.Type == enchantment.Type)
                {
                    hasSameType = true;
                    break;
                }
            }
            if (hasSameType)
            {
                GD.PrintErr($"[Enchantment] Equipment already has {enchantment.Type} enchantment");
                return false;
            }
        }

        // 成功率计算
        float successRate = CalculateSuccessRate(enchantment);
        bool success = GD.Randf() < successRate;

        _totalEnchantments++;
        _totalGoldSpent += cost;

        if (_enchantmentUsageCount.ContainsKey(enchantmentId))
            _enchantmentUsageCount[enchantmentId]++;
        else
            _enchantmentUsageCount[enchantmentId] = 1;

        if (success)
        {
            _successfulEnchantments++;

            var instance = new EnchantmentInstance
            {
                TemplateId = enchantmentId,
                CurrentLevel = 1,
                Experience = 0
            };

            if (!_appliedEnchantments.ContainsKey(equipmentId))
            {
                _appliedEnchantments[equipmentId] = new List<EnchantmentInstance>();
            }
            _appliedEnchantments[equipmentId].Add(instance);

            GD.Print($"[Enchantment] Success! Applied {enchantment.Name} to {equipmentId}");
            return true;
        }
        else
        {
            _failedEnchantments++;
            GD.Print($"[Enchantment] Failed! {enchantment.Name} failed on {equipmentId}");
            return false;
        }
    }

    // 移除附魔
    public bool RemoveEnchantment(string equipmentId, string enchantmentInstanceId)
    {
        if (!_appliedEnchantments.ContainsKey(equipmentId))
            return false;

        var list = _appliedEnchantments[equipmentId];
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i].Id == enchantmentInstanceId)
            {
                list.RemoveAt(i);
                GD.Print($"[Enchantment] Removed enchantment {enchantmentInstanceId} from {equipmentId}");
                return true;
            }
        }
        return false;
    }

    // 获取装备的附魔
    public List<EnchantmentInstance> GetEnchantmentsForEquipment(string equipmentId)
    {
        return _appliedEnchantments.ContainsKey(equipmentId)
            ? _appliedEnchantments[equipmentId]
            : new List<EnchantmentInstance>();
    }

    // 获取附魔属性加成
    public Dictionary<EnchantmentData.PropertyType, float> GetTotalPropertyBonuses(string equipmentId)
    {
        var result = new Dictionary<EnchantmentData.PropertyType, float>();

        if (!_appliedEnchantments.ContainsKey(equipmentId))
            return result;

        foreach (var instance in _appliedEnchantments[equipmentId])
        {
            var data = EnchantmentDatabase.Instance.GetEnchantment(instance.TemplateId);
            if (data == null) continue;

            foreach (var prop in data.Properties)
            {
                // 等级加成
                float levelBonus = 1f + (instance.CurrentLevel - 1) * 0.2f;
                float value = prop.Value * levelBonus;

                if (result.ContainsKey(prop.Key))
                    result[prop.Key] += value;
                else
                    result[prop.Key] = value;
            }
        }

        return result;
    }

    // 计算附魔费用
    public int CalculateEnchantmentCost(EnchantmentData enchantment)
    {
        int baseCost = enchantment.BaseCost;

        // 稀有度加成
        float rarityMultiplier = 1f;
        switch (enchantment.RarityLevel)
        {
            case EnchantmentData.Rarity.Uncommon: rarityMultiplier = 1.5f; break;
            case EnchantmentData.Rarity.Rare: rarityMultiplier = 2.5f; break;
            case EnchantmentData.Rarity.Epic: rarityMultiplier = 4f; break;
            case EnchantmentData.Rarity.Legendary: rarityMultiplier = 8f; break;
        }

        return (int)(baseCost * rarityMultiplier);
    }

    // 计算成功率
    public float CalculateSuccessRate(EnchantmentData enchantment)
    {
        float baseRate = enchantment.SuccessRate;

        // 稀有度降低成功率
        float rarityPenalty = 0f;
        switch (enchantment.RarityLevel)
        {
            case EnchantmentData.Rarity.Rare: rarityPenalty = 0.05f; break;
            case EnchantmentData.Rarity.Epic: rarityPenalty = 0.1f; break;
            case EnchantmentData.Rarity.Legendary: rarityPenalty = 0.15f; break;
        }

        return Math.Max(0.1f, baseRate - rarityPenalty);
    }

    // 升级附魔
    public bool UpgradeEnchantment(string equipmentId, string enchantmentInstanceId, int playerGold)
    {
        if (!_appliedEnchantments.ContainsKey(equipmentId))
            return false;

        EnchantmentInstance targetInstance = null;
        foreach (var inst in _appliedEnchantments[equipmentId])
        {
            if (inst.Id == enchantmentInstanceId)
            {
                targetInstance = inst;
                break;
            }
        }

        if (targetInstance == null)
            return false;

        var data = EnchantmentDatabase.Instance.GetEnchantment(targetInstance.TemplateId);
        if (data == null)
            return false;

        if (targetInstance.CurrentLevel >= data.MaxLevel)
        {
            GD.Print($"[Enchantment] Already at max level: {data.MaxLevel}");
            return false;
        }

        int upgradeCost = CalculateEnchantmentCost(data) * targetInstance.CurrentLevel;
        if (playerGold < upgradeCost)
        {
            GD.PrintErr($"[Enchantment] Not enough gold for upgrade. Need: {upgradeCost}");
            return false;
        }

        // 升级成功率 = 基础成功率 * (1 - 等级 * 0.1)
        float upgradeSuccessRate = data.SuccessRate * (1f - targetInstance.CurrentLevel * 0.1f);

        if (GD.Randf() < upgradeSuccessRate)
        {
            targetInstance.CurrentLevel++;
            targetInstance.Experience = 0;
            _totalGoldSpent += upgradeCost;
            GD.Print($"[Enchantment] Upgraded to level {targetInstance.CurrentLevel}");
            return true;
        }
        else
        {
            _failedEnchantments++;
            _totalGoldSpent += upgradeCost;
            GD.Print($"[Enchantment] Upgrade failed");
            return false;
        }
    }

    // 获取统计
    public Dictionary<string, object> GetStatistics()
    {
        var stats = new Dictionary<string, object>
        {
            { "total_enchantments", _totalEnchantments },
            { "successful_enchantments", _successfulEnchantments },
            { "failed_enchantments", _failedEnchantments },
            { "total_gold_spent", _totalGoldSpent },
            { "success_rate", _totalEnchantments > 0 ? (float)_successfulEnchantments / _totalEnchantments * 100f : 0f },
            { "unlocked_count", _unlockedEnchantments.Count },
            { "applied_count", _appliedEnchantments.Count }
        };

        // 使用最多的附魔
        string mostUsed = "";
        int maxCount = 0;
        foreach (var kvp in _enchantmentUsageCount)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                mostUsed = kvp.Key;
            }
        }
        stats["most_used_enchantment"] = mostUsed;

        return stats;
    }

    // 随机解锁附魔
    public bool DiscoverRandomEnchantment(int playerLevel)
    {
        var available = EnchantmentDatabase.Instance.GetAvailableEnchantments(playerLevel);

        // 过滤掉已解锁的
        var undiscovered = available.FindAll(e => !_unlockedEnchantments.Contains(e.Id));

        if (undiscovered.Count == 0)
            return false;

        // 稀有度权重
        int totalWeight = 0;
        foreach (var e in undiscovered)
        {
            totalWeight += EnchantmentDatabase.Instance.GetRarityWeight(e.RarityLevel);
        }

        int random = GD.Randi() % totalWeight;
        int cumulative = 0;

        foreach (var e in undiscovered)
        {
            cumulative += EnchantmentDatabase.Instance.GetRarityWeight(e.RarityLevel);
            if (random < cumulative)
            {
                UnlockEnchantment(e.Id);
                GD.Print($"[Enchantment] Discovered new enchantment: {e.Name} ({e.RarityLevel})");
                return true;
            }
        }

        return false;
    }

    // 存档支持
    public Dictionary<string, object> SaveData()
    {
        var data = new Dictionary<string, object>
        {
            { "unlocked_enchantments", new List<string>(_unlockedEnchantments) },
            { "total_enchantments", _totalEnchantments },
            { "successful_enchantments", _successfulEnchantments },
            { "failed_enchantments", _failedEnchantments },
            { "total_gold_spent", _totalGoldSpent }
        };

        // 保存应用的附魔
        var appliedData = new List<Dictionary<string, object>>();
        foreach (var kvp in _appliedEnchantments)
        {
            foreach (var inst in kvp.Value)
            {
                appliedData.Add(new Dictionary<string, object>
                {
                    { "equipment_id", kvp.Key },
                    { "instance_id", inst.Id },
                    { "template_id", inst.TemplateId },
                    { "level", inst.CurrentLevel },
                    { "experience", inst.Experience },
                    { "is_active", inst.IsActive }
                });
            }
        }
        data["applied_enchantments"] = appliedData;

        return data;
    }

    public void LoadData(Dictionary<string, object> data)
    {
        if (data == null) return;

        if (data.ContainsKey("unlocked_enchantments"))
        {
            var unlocked = (List<string>)data["unlocked_enchantments"];
            _unlockedEnchantments = new HashSet<string>(unlocked);
        }

        _totalEnchantments = data.ContainsKey("total_enchantments") ? (int)data["total_enchantments"] : 0;
        _successfulEnchantments = data.ContainsKey("successful_enchantments") ? (int)data["successful_enchantments"] : 0;
        _failedEnchantments = data.ContainsKey("failed_enchantments") ? (int)data["failed_enchantments"] : 0;
        _totalGoldSpent = data.ContainsKey("total_gold_spent") ? (int)data["total_gold_spent"] : 0;

        // 加载应用的附魔
        if (data.ContainsKey("applied_enchantments"))
        {
            var appliedList = (List<object>)data["applied_enchantments"];
            foreach (var item in appliedList)
            {
                var dict = (Dictionary<string, object>)item;
                string equipmentId = (string)dict["equipment_id"];

                var instance = new EnchantmentInstance
                {
                    TemplateId = (string)dict["template_id"],
                    CurrentLevel = (int)dict["level"],
                    Experience = (int)dict["experience"],
                    IsActive = (bool)dict["is_active"]
                };

                if (!_appliedEnchantments.ContainsKey(equipmentId))
                {
                    _appliedEnchantments[equipmentId] = new List<EnchantmentInstance>();
                }
                _appliedEnchantments[equipmentId].Add(instance);
            }
        }

        GD.Print($"[Enchantment] Loaded {_unlockedEnchantments.Count} unlocked, {_appliedEnchantments.Count} applied");
    }
}
