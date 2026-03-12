using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems;

/// <summary>
/// 宠物变异数据
/// </summary>
public class PetMutationData
{
    public int PetId { get; set; }
    public List<PetMutation> Mutations { get; set; } = new();
    public int TotalMutations { get; set; }
    public int RareMutations { get; set; }
    public int LegendaryMutations { get; set; }
    public Dictionary<string, int> MutationTypeCounts { get; set; } = new();
}

/// <summary>
/// 宠物变异
/// </summary>
public class PetMutation
{
    public string MutationId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }  // Physical/Ability/Elemental/Stat/Special
    public string Rarity { get; set; }  // Common/Uncommon/Rare/Epic/Legendary
    public Dictionary<string, float> StatBonuses { get; set; } = new();
    public List<string> AddedAbilities { get; set; } = new();
    public string VisualEffect { get; set; }
    public DateTime MutatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// 变异数据库配置
/// </summary>
public class PetMutationDatabase
{
    public static Dictionary<string, Dictionary<string, object>> GetMutations()
    {
        return new Dictionary<string, Dictionary<string, object>>
        {
            // Physical 变异
            ["mutation_giant"] = new Dictionary<string, object>
            {
                ["name"] = "巨型化",
                ["description"] = "宠物体型增大，生命值大幅提升",
                ["type"] = "Physical",
                ["rarity"] = "Uncommon",
                ["stat_bonuses"] = new Dictionary<string, float> { ["health"] = 50f, ["defense"] = 10f },
                ["visual_effect"] = "size_up"
            },
            ["mutation_tiny"] = new Dictionary<string, object>
            {
                ["name"] = "迷你化",
                ["description"] = "宠物体型缩小，速度大幅提升",
                ["type"] = "Physical",
                ["rarity"] = "Uncommon",
                ["stat_bonuses"] = new Dictionary<string, float> { ["speed"] = 30f, ["dodge"] = 15f },
                ["visual_effect"] = "size_down"
            },
            ["mutation_spectral"] = new Dictionary<string, object>
            {
                ["name"] = "幽灵化",
                ["description"] = "宠物身体半透明，闪避率提升",
                ["type"] = "Physical",
                ["rarity"] = "Rare",
                ["stat_bonuses"] = new Dictionary<string, float> { ["dodge"] = 25f, ["magic_defense"] = 20f },
                ["visual_effect"] = "ghost_form"
            },
            ["mutation_crystalline"] = new Dictionary<string, object>
            {
                ["name"] = "晶石化",
                ["description"] = "宠物身体结晶化，防御力大幅提升",
                ["type"] = "Physical",
                ["rarity"] = "Rare",
                ["stat_bonuses"] = new Dictionary<string, float> { ["defense"] = 35f, ["magic_defense"] = 25f },
                ["visual_effect"] = "crystal_shine"
            },
            
            // Ability 变异
            ["mutation_teleport"] = new Dictionary<string, object>
            {
                ["name"] = "瞬移能力",
                ["description"] = "宠物获得瞬移技能",
                ["type"] = "Ability",
                ["rarity"] = "Epic",
                ["stat_bonuses"] = new Dictionary<string, float> { ["speed"] = 20f },
                ["added_abilities"] = new List<string> { "teleport" },
                ["visual_effect"] = "portal_spark"
            },
            ["mutation_phaseshift"] = new Dictionary<string, object>
            {
                ["name"] = "相位转移",
                ["description"] = "宠物可以穿过障碍物",
                ["type"] = "Ability",
                ["rarity"] = "Epic",
                ["stat_bonuses"] = new Dictionary<string, float> { ["dodge"] = 30f },
                ["added_abilities"] = new List<string> { "phase_shift" },
                ["visual_effect"] = "phase_aura"
            },
            ["mutation_timewarp"] = new Dictionary<string, object>
            {
                ["name"] = "时间扭曲",
                ["description"] = "宠物周围时间流速改变",
                ["type"] = "Ability",
                ["rarity"] = "Legendary",
                ["stat_bonuses"] = new Dictionary<string, float> { ["attack_speed"] = 40f, ["cooldown"] = -20f },
                ["added_abilities"] = new List<string> { "time_warp" },
                ["visual_effect"] = "time_spiral"
            },
            
            // Elemental 变异
            ["mutation_flame"] = new Dictionary<string, object>
            {
                ["name"] = "火焰亲和",
                ["description"] = "宠物获得火焰属性",
                ["type"] = "Elemental",
                ["rarity"] = "Rare",
                ["stat_bonuses"] = new Dictionary<string, float> { ["fire_damage"] = 30f, ["attack"] = 15f },
                ["visual_effect"] = "fire_aura"
            },
            ["mutation_ice"] = new Dictionary<string, object>
            {
                ["name"] = "冰霜亲和",
                ["description"] = "宠物获得冰霜属性",
                ["type"] = "Elemental",
                ["rarity"] = "Rare",
                ["stat_bonuses"] = new Dictionary<string, float> { ["ice_damage"] = 30f, ["defense"] = 15f },
                ["visual_effect"] = "ice_aura"
            },
            ["mutation_thunder"] = new Dictionary<string, object>
            {
                ["name"] = "雷电亲和",
                ["description"] = "宠物获得雷电属性",
                ["type"] = "Elemental",
                ["rarity"] = "Rare",
                ["stat_bonuses"] = new Dictionary<string, float> { ["thunder_damage"] = 30f, ["speed"] = 20f },
                ["visual_effect"] = "thunder_aura"
            },
            ["mutation_void"] = new Dictionary<string, object>
            {
                ["name"] = "虚空亲和",
                ["description"] = "宠物获得虚空属性",
                ["type"] = "Elemental",
                ["rarity"] = "Epic",
                ["stat_bonuses"] = new Dictionary<string, float> { ["void_damage"] = 40f, ["magic_attack"] = 25f },
                ["visual_effect"] = "void_aura"
            },
            ["mutation_chaos"] = new Dictionary<string, object>
            {
                ["name"] = "混沌亲和",
                ["description"] = "宠物获得混沌属性，所有伤害提升",
                ["type"] = "Elemental",
                ["rarity"] = "Legendary",
                ["stat_bonuses"] = new Dictionary<string, float> { ["all_damage"] = 25f, ["crit_damage"] = 20f },
                ["visual_effect"] = "chaos_aura"
            },
            
            // Stat 变异
            ["mutation_ferocious"] = new Dictionary<string, object>
            {
                ["name"] = "凶猛",
                ["description"] = "宠物变得凶猛，攻击力和暴击率提升",
                ["type"] = "Stat",
                ["rarity"] = "Uncommon",
                ["stat_bonuses"] = new Dictionary<string, float> { ["attack"] = 25f, ["crit_rate"] = 15f },
                ["visual_effect"] = "red_eyes"
            },
            ["mutation_swift"] = new Dictionary<string, object>
            {
                ["name"] = "迅捷",
                ["description"] = "宠物变得异常迅速",
                ["type"] = "Stat",
                ["rarity"] = "Uncommon",
                ["stat_bonuses"] = new Dictionary<string, float> { ["speed"] = 35f, ["attack_speed"] = 20f },
                ["visual_effect"] = "speed_lines"
            },
            ["mutation_tank"] = new Dictionary<string, object>
            {
                ["name"] = "坦克",
                ["description"] = "宠物变得非常抗打",
                ["type"] = "Stat",
                ["rarity"] = "Uncommon",
                ["stat_bonuses"] = new Dictionary<string, float> { ["health"] = 60f, ["defense"] = 20f },
                ["visual_effect"] = "armor_shine"
            },
            ["mutation_lucky"] = new Dictionary<string, object>
            {
                ["name"] = "幸运",
                ["description"] = "宠物变得非常幸运",
                ["type"] = "Stat",
                ["rarity"] = "Rare",
                ["stat_bonuses"] = new Dictionary<string, float> { ["luck"] = 40f, ["drop_rate"] = 25f },
                ["visual_effect"] = "star_aura"
            },
            ["mutation_vampiric"] = new Dictionary<string, object>
            {
                ["name"] = "吸血",
                ["description"] = "宠物获得吸血能力",
                ["type"] = "Stat",
                ["rarity"] = "Epic",
                ["stat_bonuses"] = new Dictionary<string, float> { ["lifesteal"] = 20f, ["attack"] = 15f },
                ["visual_effect"] = "blood_aura"
            },
            
            // Special 变异
            ["mutation_immortal"] = new Dictionary<string, object>
            {
                ["name"] = "不朽",
                ["description"] = "宠物获得一次免死机会",
                ["type"] = "Special",
                ["rarity"] = "Legendary",
                ["stat_bonuses"] = new Dictionary<string, float> { ["health"] = 100f },
                ["added_abilities"] = new List<string> { "extra_life" },
                ["visual_effect"] = "immortal_glow"
            },
            ["mutation_mythical"] = new Dictionary<string, object>
            {
                ["name"] = "神话",
                ["description"] = "宠物觉醒神话之力",
                ["type"] = "Special",
                ["rarity"] = "Legendary",
                ["stat_bonuses"] = new Dictionary<string, float> { ["all_stats"] = 15f },
                ["added_abilities"] = new List<string> { "mythical_blessing" },
                ["visual_effect"] = "mythical_sparkle"
            },
            ["mutation_mercurial"] = new Dictionary<string, object>
            {
                ["name"] = "易变",
                ["description"] = "宠物属性随机波动",
                ["type"] = "Special",
                ["rarity"] = "Epic",
                ["stat_bonuses"] = new Dictionary<string, float> { ["random_stat"] = 30f },
                ["added_abilities"] = new List<string> { "stat_fluctuation" },
                ["visual_effect"] = "rainbow_swirl"
            }
        };
    }
    
    public static Dictionary<string, float> GetRarityWeights()
    {
        return new Dictionary<string, float>
        {
            ["Common"] = 0f,
            ["Uncommon"] = 60f,
            ["Rare"] = 25f,
            ["Epic"] = 10f,
            ["Legendary"] = 5f
        };
    }
    
    public static string[] GetMutationTypes()
    {
        return new[] { "Physical", "Ability", "Elemental", "Stat", "Special" };
    }
}
