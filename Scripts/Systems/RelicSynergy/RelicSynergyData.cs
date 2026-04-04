using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems;

/// <summary>
/// 遗物协同数据结构
/// </summary>
public class RelicSynergyEntry
{
    /// <summary>协同组合唯一ID</summary>
    public string SynergyId { get; set; }
    
    /// <summary>组成协同的遗物ID列表（2~3个）</summary>
    public string[] RelicIds { get; set; }
    
    /// <summary>协同名称</summary>
    public string SynergyName { get; set; }
    
    /// <summary>发现时显示的叙事文字</summary>
    public string DiscoveryMessage { get; set; }
    
    /// <summary>加成类型: damage | defense | special</summary>
    public string BonusType { get; set; }
    
    /// <summary>加成数值（百分比，如 0.15 表示 +15%）</summary>
    public float BonusValue { get; set; }
    
    /// <summary>稀有度: common | uncommon | rare | epic | legendary</summary>
    public string Rarity { get; set; }
}

/// <summary>
/// 玩家协同追踪数据（运行时）
/// </summary>
public class PlayerSynergyData
{
    /// <summary>本局已发现的synergyId集合</summary>
    public HashSet<string> DiscoveredThisRun { get; set; } = new();
    
    /// <summary>跨局次已解锁的synergyId集合</summary>
    public HashSet<string> AllTimeDiscoveries { get; set; } = new();
    
    /// <summary>本局激活的协同效果</summary>
    public List<string> ActiveSynergyIds { get; set; } = new();
}

/// <summary>
/// 遗物协同数据库 — 静态配置，预定义协同组合
/// </summary>
public static class RelicSynergyDatabase
{
    private static readonly Dictionary<string, RelicSynergyEntry> _synergies = new();
    private static bool _initialized = false;
    
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        
        // === 预设协同组合（示例 10 组，可扩展）===
        
        // 火焰系协同
        AddSynergy(new RelicSynergyEntry
        {
            SynergyId = "fire_burn_chain",
            RelicIds = new[] { "relic_flame_heart", "relic_burning_ring" },
            SynergyName = "火焰连锁",
            DiscoveryMessage = "You discovered: [Shattered Heart] + [Burning Ring] = Flame Chain\n   -> +15% damage vs burning enemies",
            BonusType = "damage",
            BonusValue = 0.15f,
            Rarity = "rare"
        });
        
        // 冰霜系协同
        AddSynergy(new RelicSynergyEntry
        {
            SynergyId = "ice_shatter_frost",
            RelicIds = new[] { "relic_ice_crystal", "relic_frost_armor" },
            SynergyName = "冰霜碎裂",
            DiscoveryMessage = "You discovered: [Ice Crystal] + [Frost Armor] = Shatter Frost\n   -> +20% crit rate vs frozen enemies",
            BonusType = "damage",
            BonusValue = 0.20f,
            Rarity = "uncommon"
        });
        
        // 雷电系协同
        AddSynergy(new RelicSynergyEntry
        {
            SynergyId = "lightning_charge_storm",
            RelicIds = new[] { "relic_thunder_orb", "relic_charge_core" },
            SynergyName = "雷霆充能",
            DiscoveryMessage = "You discovered: [Thunder Orb] + [Charge Core] = Lightning Charge\n   -> Static stacks on each attack, chains at 5 stacks",
            BonusType = "special",
            BonusValue = 0.10f,
            Rarity = "epic"
        });
        
        // 生命偷取协同
        AddSynergy(new RelicSynergyEntry
        {
            SynergyId = "lifesteal_drain_vamp",
            RelicIds = new[] { "relic_vampire_fang", "relic_drain_scepter" },
            SynergyName = "鲜血汲取",
            DiscoveryMessage = "Blood! You discovered: [Vampire Fang] + [Drain Scepter] = Crimson Drain\n   -> Lifesteal +25%",
            BonusType = "special",
            BonusValue = 0.25f,
            Rarity = "rare"
        });
        
        // 防御系协同
        AddSynergy(new RelicSynergyEntry
        {
            SynergyId = "defense_stone_wall",
            RelicIds = new[] { "relic_stone_guardian", "relic_wall_of_thorns" },
            SynergyName = "坚壁清野",
            DiscoveryMessage = "You discovered: [Stone Guardian] + [Wall of Thorns] = Barren Defense\n   -> Reflects 10% damage back to attacker",
            BonusType = "defense",
            BonusValue = 0.10f,
            Rarity = "uncommon"
        });
        
        // 暴击协同
        AddSynergy(new RelicSynergyEntry
        {
            SynergyId = "crit_precision_eagle",
            RelicIds = new[] { "relic_eagle_eye", "relic_precision_lens" },
            SynergyName = "精准狙击",
            DiscoveryMessage = "You discovered: [Eagle Eye] + [Precision Lens] = Precision Snipe\n   -> +30% critical damage",
            BonusType = "damage",
            BonusValue = 0.30f,
            Rarity = "epic"
        });
        
        // 三遗物协同（高级）
        AddSynergy(new RelicSynergyEntry
        {
            SynergyId = "holy_trinity_blessing",
            RelicIds = new[] { "relic_holy_grail", "relic_blessed_crown", "relic_angel_feather" },
            SynergyName = "神圣三位一体",
            DiscoveryMessage = "You discovered: [Holy Grail] + [Blessed Crown] + [Angel Feather] = Holy Trinity\n   -> All stats +10%, all status resist +50%",
            BonusType = "special",
            BonusValue = 0.10f,
            Rarity = "legendary"
        });
        
        // 速度协同
        AddSynergy(new RelicSynergyEntry
        {
            SynergyId = "speed_wind_feet",
            RelicIds = new[] { "relic_wind_boots", "relic_swift_gloves" },
            SynergyName = "疾风步",
            DiscoveryMessage = "You discovered: [Wind Boots] + [Swift Gloves] = Windwalk\n   -> +20% attack speed",
            BonusType = "special",
            BonusValue = 0.20f,
            Rarity = "uncommon"
        });
        
        // 毒系协同
        AddSynergy(new RelicSynergyEntry
        {
            SynergyId = "poison_viper_nettle",
            RelicIds = new[] { "relic_viper_scale", "relic_nettle_sting" },
            SynergyName = "毒蛇荨麻",
            DiscoveryMessage = "Serpent discovered: [Viper Scale] + [Nettle Sting] = Serpent's Nettle\n   -> Poison stack cap +3, duration +50%",
            BonusType = "special",
            BonusValue = 0.15f,
            Rarity = "rare"
        });
        
        // 经验协同
        AddSynergy(new RelicSynergyEntry
        {
            SynergyId = "exp_wisdom_tome",
            RelicIds = new[] { "relic_wisdom_tome", "relic_enlightenment_orb" },
            SynergyName = "智慧觉醒",
            DiscoveryMessage = "You discovered: [Wisdom Tome] + [Enlightenment Orb] = Enlightenment\n   -> +15% kill experience",
            BonusType = "special",
            BonusValue = 0.15f,
            Rarity = "uncommon"
        });
        
        GD.Print($"[RelicSynergyDatabase] Initialized {_synergies.Count} synergy combinations");
    }
    
    private static void AddSynergy(RelicSynergyEntry entry)
    {
        _synergies[entry.SynergyId] = entry;
    }
    
    /// <summary>
    /// 获取所有协同条目
    /// </summary>
    public static Dictionary<string, RelicSynergyEntry> GetAllSynergies()
    {
        return new Dictionary<string, RelicSynergyEntry>(_synergies);
    }
    
    /// <summary>
    /// 根据 synergyId 获取协同条目
    /// </summary>
    public static RelicSynergyEntry GetSynergy(string synergyId)
    {
        return _synergies.TryGetValue(synergyId, out var entry) ? entry : null;
    }
    
    /// <summary>
    /// 检查给定遗物ID列表是否形成某个协同
    /// </summary>
    public static RelicSynergyEntry CheckSynergy(IEnumerable<string> relicIds)
    {
        var relicSet = new HashSet<string>(relicIds);
        
        foreach (var entry in _synergies.Values)
        {
            var synergySet = new HashSet<string>(entry.RelicIds);
            
            // 所有协同遗物都在当前装备中
            bool allPresent = true;
            foreach (var rid in entry.RelicIds)
            {
                if (!relicSet.Contains(rid))
                {
                    allPresent = false;
                    break;
                }
            }
            
            if (allPresent)
                return entry;
        }
        
        return null;
    }
    
    /// <summary>
    /// 获取与指定遗物相关的所有协同
    /// </summary>
    public static List<RelicSynergyEntry> GetSynergiesForRelic(string relicId)
    {
        var result = new List<RelicSynergyEntry>();
        foreach (var entry in _synergies.Values)
        {
            foreach (var rid in entry.RelicIds)
            {
                if (rid == relicId)
                {
                    result.Add(entry);
                    break;
                }
            }
        }
        return result;
    }
}
