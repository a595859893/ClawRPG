using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 鉴定系统 - 为未鉴装备生成随机属性
/// 支持多种鉴定方法（免费/标准/高级/ premium），不同方法产出不同数量和品质的属性的
/// </summary>
public class IdentificationSystem : BaseSystem
{
    /// <summary>
    /// 获取系统单例实例
    /// </summary>
    public static IdentificationSystem Instance { get; private set; }
    
    // 鉴定方法类型
    public enum IdentificationMethod
    {
        Basic,      // 免费鉴定 - 1-2个随机属性
        Standard,   // 标准鉴定 - 2-3个随机属性 (100金币)
        Advanced,   // 高级鉴定 - 3-4个随机属性 (500金币)
        Premium     // Premium鉴定 - 4-5个随机属性 (2000金币)
    }
    
    // 鉴定属性池
    private static readonly string[] AttributePool = new string[]
    {
        "attack", "defense", "health", "magic", "speed",
        "crit_rate", "crit_damage", "lifesteal", "dodge",
        "fire_resist", "ice_resist", "lightning_resist", "dark_resist", "holy_resist",
        "exp_bonus", "gold_bonus", "drop_bonus", "regen"
    };
    
    // 稀有度对应的属性数量和品质
    private static readonly Dictionary<string, int[]> RarityAttributeCount = new Dictionary<string, int[]>
    {
        {"Common", new int[] {1, 2}},
        {"Uncommon", new int[] {2, 3}},
        {"Rare", new int[] {2, 4}},
        {"Epic", new int[] {3, 4}},
        {"Legendary", new int[] {4, 5}},
        {"Mythical", new int[] {4, 6}}
    };
    
    // 玩家数据
    private int _totalIdentifications = 0;
    private int _basicIdentifications = 0;
    private int _standardIdentifications = 0;
    private int _advancedIdentifications = 0;
    private int _premiumIdentifications = 0;
    private int _highestRarityIdentified = 0; // 0=None, 1=Common, ..., 6=Mythical
    
    // 成本配置
    private static readonly Dictionary<IdentificationMethod, int> MethodCosts = new Dictionary<IdentificationMethod, int>
    {
        {IdentificationMethod.Basic, 0},
        {IdentificationMethod.Standard, 100},
        {IdentificationMethod.Advanced, 500},
        {IdentificationMethod.Premium, 2000}
    };
    
    // 属性范围
    private static readonly Dictionary<string, (int min, int max)> AttributeRanges = new Dictionary<string, (int, int)>
    {
        {"attack", (5, 50)},
        {"defense", (5, 40)},
        {"health", (20, 200)},
        {"magic", (5, 45)},
        {"speed", (2, 20)},
        {"crit_rate", (1, 10)},
        {"crit_damage", (5, 30)},
        {"lifesteal", (1, 8)},
        {"dodge", (1, 8)},
        {"fire_resist", (3, 15)},
        {"ice_resist", (3, 15)},
        {"lightning_resist", (3, 15)},
        {"dark_resist", (3, 15)},
        {"holy_resist", (3, 15)},
        {"exp_bonus", (5, 20)},
        {"gold_bonus", (5, 25)},
        {"drop_bonus", (3, 15)},
        {"regen", (1, 5)}
    };
    
    public override void _Ready()
    {
        Instance = this;
    }
    
    public static IdentificationSystem GetInstance() => Instance;
    
    // 鉴定装备
    public Dictionary<string, int> IdentifyEquipment(string itemRarity, IdentificationMethod method)
    {
        var identifiedAttributes = new Dictionary<string, int>();
        
        // 根据方法和稀有度确定属性数量
        int minAttrs = 1;
        int maxAttrs = 2;
        
        if (RarityAttributeCount.ContainsKey(itemRarity))
        {
            var range = RarityAttributeCount[itemRarity];
            minAttrs = range[0];
            maxAttrs = range[1];
        }
        
        // 根据鉴定方法提升属性数量
        switch (method)
        {
            case IdentificationMethod.Standard:
                minAttrs += 1;
                maxAttrs += 1;
                break;
            case IdentificationMethod.Advanced:
                minAttrs += 2;
                maxAttrs += 2;
                break;
            case IdentificationMethod.Premium:
                minAttrs += 3;
                maxAttrs += 3;
                break;
        }
        
        // 确保在有效范围内
        minAttrs = Mathf.Min(minAttrs, AttributePool.Length);
        maxAttrs = Mathf.Min(maxAttrs, AttributePool.Length);
        
        // 随机选择属性数量
        int attrCount = (int)(GD.Randf() * (maxAttrs - minAttrs + 1)) + minAttrs;
        
        // 随机选择属性
        var shuffledAttrs = ShuffleArray(AttributePool);
        for (int i = 0; i < attrCount && i < shuffledAttrs.Length; i++)
        {
            string attr = shuffledAttrs[i];
            var range = AttributeRanges[attr];
            int value = (int)(GD.Randf() * (range.max - range.min + 1)) + range.min;
            identifiedAttributes[attr] = value;
        }
        
        // 更新统计
        UpdateStatistics(method, itemRarity);
        
        return identifiedAttributes;
    }
    
    // 随机打乱数组
    private string[] ShuffleArray(string[] array)
    {
        var result = new string[array.Length];
        Array.Copy(array, result, array.Length);
        
        for (int i = result.Length - 1; i > 0; i--)
        {
            int j = (int)(GD.Randf() * (i + 1));
            string temp = result[i];
            result[i] = result[j];
            result[j] = temp;
        }
        
        return result;
    }
    
    // 更新统计
    private void UpdateStatistics(IdentificationMethod method, string rarity)
    {
        _totalIdentifications++;
        
        switch (method)
        {
            case IdentificationMethod.Basic:
                _basicIdentifications++;
                break;
            case IdentificationMethod.Standard:
                _standardIdentifications++;
                break;
            case IdentificationMethod.Advanced:
                _advancedIdentifications++;
                break;
            case IdentificationMethod.Premium:
                _premiumIdentifications++;
                break;
        }
        
        // 更新最高稀有度
        int rarityLevel = GetRarityLevel(rarity);
        if (rarityLevel > _highestRarityIdentified)
        {
            _highestRarityIdentified = rarityLevel;
        }
    }
    
    // 获取稀有度等级
    private int GetRarityLevel(string rarity)
    {
        switch (rarity)
        {
            case "Common": return 1;
            case "Uncommon": return 2;
            case "Rare": return 3;
            case "Epic": return 4;
            case "Legendary": return 5;
            case "Mythical": return 6;
            default: return 0;
        }
    }
    
    // 获取鉴定成本
    public static int GetIdentificationCost(IdentificationMethod method)
    {
        return MethodCosts.ContainsKey(method) ? MethodCosts[method] : 0;
    }
    
    // 获取统计信息
    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            {"total_identifications", _totalIdentifications},
            {"basic_identifications", _basicIdentifications},
            {"standard_identifications", _standardIdentifications},
            {"advanced_identifications", _advancedIdentifications},
            {"premium_identifications", _premiumIdentifications},
            {"highest_rarity", _highestRarityIdentified}
        };
    }
    
    // 保存数据
    public Dictionary<string, object> SaveData()
    {
        return new Dictionary<string, object>
        {
            {"total_identifications", _totalIdentifications},
            {"basic_identifications", _basicIdentifications},
            {"standard_identifications", _standardIdentifications},
            {"advanced_identifications", _advancedIdentifications},
            {"premium_identifications", _premiumIdentifications},
            {"highest_rarity_identified", _highestRarityIdentified}
        };
    }
    
    // 加载数据
    public void LoadData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("total_identifications"))
            _totalIdentifications = Convert.ToInt32(data["total_identifications"]);
        if (data.ContainsKey("basic_identifications"))
            _basicIdentifications = Convert.ToInt32(data["basic_identifications"]);
        if (data.ContainsKey("standard_identifications"))
            _standardIdentifications = Convert.ToInt32(data["standard_identifications"]);
        if (data.ContainsKey("advanced_identifications"))
            _advancedIdentifications = Convert.ToInt32(data["advanced_identifications"]);
        if (data.ContainsKey("premium_identifications"))
            _premiumIdentifications = Convert.ToInt32(data["premium_identifications"]);
        if (data.ContainsKey("highest_rarity_identified"))
            _highestRarityIdentified = Convert.ToInt32(data["highest_rarity_identified"]);
    }
    
    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary
        {
            { "total_identifications", _totalIdentifications },
            { "basic_identifications", _basicIdentifications },
            { "standard_identifications", _standardIdentifications },
            { "advanced_identifications", _advancedIdentifications },
            { "premium_identifications", _premiumIdentifications },
            { "highest_rarity_identified", _highestRarityIdentified }
        };
    }
    
    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.Contains("total_identifications")) _totalIdentifications = (int)data["total_identifications"];
        if (data.Contains("basic_identifications")) _basicIdentifications = (int)data["basic_identifications"];
        if (data.Contains("standard_identifications")) _standardIdentifications = (int)data["standard_identifications"];
        if (data.Contains("advanced_identifications")) _advancedIdentifications = (int)data["advanced_identifications"];
        if (data.Contains("premium_identifications")) _premiumIdentifications = (int)data["premium_identifications"];
        if (data.Contains("highest_rarity_identified")) _highestRarityIdentified = (int)data["highest_rarity_identified"];
    }
}
