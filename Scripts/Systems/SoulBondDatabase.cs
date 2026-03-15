using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 灵魂绑定数据库。存储所有灵魂绑定配置信息。
/// </summary>
public class SoulBondDatabase
{
    /// <summary>
    /// 获取数据库单例实例。
    /// </summary>
    private static SoulBondDatabase _instance;
    public static SoulBondDatabase Instance => _instance ??= new SoulBondDatabase();

    public Dictionary<string, BondConfig> BondConfigs { get; private set; }
    public Dictionary<BondLevel, BondLevelConfig> LevelConfigs { get; private set; }
    public Dictionary<string, BondAbility> Abilities { get; private set; }

    public SoulBondDatabase()
    {
        InitializeBondConfigs();
        InitializeLevelConfigs();
        InitializeAbilities();
    }

    private void InitializeBondConfigs()
    {
        BondConfigs = new Dictionary<string, BondConfig>
        {
            // Weapon bonds
            ["sword_1"] = new BondConfig { ItemId = "sword_1", BondType = BondType.Weapon, BasePointsRequired = 100, PointMultiplier = 1.0f },
            ["sword_2"] = new BondConfig { ItemId = "sword_2", BondType = BondType.Weapon, BasePointsRequired = 150, PointMultiplier = 1.2f },
            ["sword_3"] = new BondConfig { ItemId = "sword_3", BondType = BondType.Weapon, BasePointsRequired = 200, PointMultiplier = 1.5f },
            ["axe_1"] = new BondConfig { ItemId = "axe_1", BondType = BondType.Weapon, BasePointsRequired = 100, PointMultiplier = 1.0f },
            ["staff_1"] = new BondConfig { ItemId = "staff_1", BondType = BondType.Weapon, BasePointsRequired = 120, PointMultiplier = 1.1f },
            ["bow_1"] = new BondConfig { ItemId = "bow_1", BondType = BondType.Weapon, BasePointsRequired = 100, PointMultiplier = 1.0f },

            // Armor bonds
            ["armor_1"] = new BondConfig { ItemId = "armor_1", BondType = BondType.Armor, BasePointsRequired = 100, PointMultiplier = 1.0f },
            ["armor_2"] = new BondConfig { ItemId = "armor_2", BondType = BondType.Armor, BasePointsRequired = 150, PointMultiplier = 1.2f },
            ["helmet_1"] = new BondConfig { ItemId = "helmet_1", BondType = BondType.Armor, BasePointsRequired = 80, PointMultiplier = 0.9f },
            ["boots_1"] = new BondConfig { ItemId = "boots_1", BondType = BondType.Armor, BasePointsRequired = 80, PointMultiplier = 0.9f },

            // Accessory bonds
            ["ring_1"] = new BondConfig { ItemId = "ring_1", BondType = BondType.Accessory, BasePointsRequired = 80, PointMultiplier = 0.8f },
            ["amulet_1"] = new BondConfig { ItemId = "amulet_1", BondType = BondType.Accessory, BasePointsRequired = 100, PointMultiplier = 1.0f },
            ["cloak_1"] = new BondConfig { ItemId = "cloak_1", BondType = BondType.Accessory, BasePointsRequired = 90, PointMultiplier = 0.9f },

            // Pet bonds
            ["pet_dragon"] = new BondConfig { ItemId = "pet_dragon", BondType = BondType.Pet, BasePointsRequired = 200, PointMultiplier = 1.5f },
            ["pet_phoenix"] = new BondConfig { ItemId = "pet_phoenix", BondType = BondType.Pet, BasePointsRequired = 200, PointMultiplier = 1.5f },
            ["pet_wolf"] = new BondConfig { ItemId = "pet_wolf", BondType = BondType.Pet, BasePointsRequired = 150, PointMultiplier = 1.2f },
            ["pet_slime"] = new BondConfig { ItemId = "pet_slime", BondType = BondType.Pet, BasePointsRequired = 100, PointMultiplier = 1.0f },
            ["pet_ghost"] = new BondConfig { ItemId = "pet_ghost", BondType = BondType.Pet, BasePointsRequired = 180, PointMultiplier = 1.4f }
        };
    }

    private void InitializeLevelConfigs()
    {
        LevelConfigs = new Dictionary<BondLevel, BondLevelConfig>
        {
            [BondLevel.Awakening] = new BondLevelConfig
            {
                Level = BondLevel.Awakening,
                PointsRequired = 0,
                StatBonus = new Dictionary<string, float> { ["attack"] = 0.05f },
                AbilityUnlock = "awakening_passive"
            },
            [BondLevel.Manifestation] = new BondLevelConfig
            {
                Level = BondLevel.Manifestation,
                PointsRequired = 100,
                StatBonus = new Dictionary<string, float> { ["attack"] = 0.1f, ["defense"] = 0.05f },
                AbilityUnlock = "manifest_skill"
            },
            [BondLevel.Convergence] = new BondLevelConfig
            {
                Level = BondLevel.Convergence,
                PointsRequired = 300,
                StatBonus = new Dictionary<string, float> { ["attack"] = 0.15f, ["defense"] = 0.1f, ["speed"] = 0.05f },
                AbilityUnlock = "convergence_aura"
            },
            [BondLevel.Transcendence] = new BondLevelConfig
            {
                Level = BondLevel.Transcendence,
                PointsRequired = 600,
                StatBonus = new Dictionary<string, float> { ["attack"] = 0.2f, ["defense"] = 0.15f, ["speed"] = 0.1f, ["critical"] = 0.05f },
                AbilityUnlock = "transcendence_skill"
            },
            [BondLevel.Nirvana] = new BondLevelConfig
            {
                Level = BondLevel.Nirvana,
                PointsRequired = 1000,
                StatBonus = new Dictionary<string, float> { ["attack"] = 0.25f, ["defense"] = 0.2f, ["speed"] = 0.15f, ["critical"] = 0.1f, ["luck"] = 0.1f },
                AbilityUnlock = "nirvana_blessing"
            }
        };
    }

    private void InitializeAbilities()
    {
        Abilities = new Dictionary<string, BondAbility>
        {
            ["awakening_passive"] = new BondAbility
            {
                Id = "awakening_passive",
                Name = "Awakened Spirit",
                Description = "Increases base stats by 5%",
                Type = "passive",
                StatModifiers = new Dictionary<string, float> { ["all"] = 0.05f }
            },
            ["manifest_skill"] = new BondAbility
            {
                Id = "manifest_skill",
                Name = "Manifest Soul",
                Description = "Active: Deal 150% damage once",
                Type = "active",
                Cooldown = 30f,
                Effect = "damage_150"
            },
            ["convergence_aura"] = new BondAbility
            {
                Id = "convergence_aura",
                Name = "Soul Convergence",
                Description = "Passive: Allies within range gain 10% attack",
                Type = "passive",
                AuraRange = 10f,
                StatModifiers = new Dictionary<string, float> { ["attack"] = 0.1f }
            },
            ["transcendence_skill"] = new BondAbility
            {
                Id = "transcendence_skill",
                Name = "Transcend Limits",
                Description = "Active: Remove all debuffs and gain 50% damage reduction for 5s",
                Type = "active",
                Cooldown = 60f,
                Effect = "cleanse_and_shield"
            },
            ["nirvana_blessing"] = new BondAbility
            {
                Id = "nirvana_blessing",
                Name = "Nirvana Blessing",
                Description = "Ultimate: Revive once with 50% HP on death (once per battle)",
                Type = "ultimate",
                Cooldown = 300f,
                Effect = "revive_once"
            }
        };
    }

    public BondConfig GetBondConfig(string itemId)
    {
        return BondConfigs.ContainsKey(itemId) ? BondConfigs[itemId] : null;
    }

    public BondLevelConfig GetLevelConfig(BondLevel level)
    {
        return LevelConfigs.ContainsKey(level) ? LevelConfigs[level] : null;
    }

    public BondAbility GetAbility(string abilityId)
    {
        return Abilities.ContainsKey(abilityId) ? Abilities[abilityId] : null;
    }
}

public class BondConfig
{
    public string ItemId { get; set; }
    public BondType BondType { get; set; }
    public int BasePointsRequired { get; set; }
    public float PointMultiplier { get; set; }
}

public class BondLevelConfig
{
    public BondLevel Level { get; set; }
    public int PointsRequired { get; set; }
    public Dictionary<string, float> StatBonus { get; set; }
    public string AbilityUnlock { get; set; }
}

public class BondAbility
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; } // passive, active, ultimate
    public float Cooldown { get; set; }
    public string Effect { get; set; }
    public float AuraRange { get; set; }
    public Dictionary<string, float> StatModifiers { get; set; }
}
