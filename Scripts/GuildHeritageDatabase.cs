using System;
using System.Collections.Generic;

public class GuildHeritageDatabase
{
    private static GuildHeritageDatabase _instance;
    public static GuildHeritageDatabase Instance => _instance ?? (_instance = new GuildHeritageDatabase());

    public Dictionary<string, HeritageBonus> Heritages { get; private set; }
    public Dictionary<HeritageType, List<string>> HeritagesByType { get; private set; }
    public Dictionary<HeritageTier, Dictionary<HeritageType, string>> TierMapping { get; private set; }

    public GuildHeritageDatabase()
    {
        Heritages = new Dictionary<string, HeritageBonus>();
        HeritagesByType = new Dictionary<HeritageType, List<string>>();
        TierMapping = new Dictionary<HeritageTier, Dictionary<HeritageType, string>>();
        InitializeHeritages();
        InitializeTierMapping();
    }

    private void InitializeHeritages()
    {
        // Battle Cry Heritages
        AddHeritage(new HeritageBonus
        {
            Id = "battle_cry_bronze",
            Name = "Ancient War Cry",
            Description = "Grants +5% damage to all guild members in combat",
            Type = HeritageType.BattleCry,
            Tier = HeritageTier.Bronze,
            RequiredPoints = 100,
            DamageBonus = 0.05f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "battle_cry_silver",
            Name = "Epic War Anthem",
            Description = "Grants +10% damage and +3% crit rate to all guild members",
            Type = HeritageType.BattleCry,
            Tier = HeritageTier.Silver,
            RequiredPoints = 500,
            DamageBonus = 0.10f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "battle_cry_gold",
            Name = "Legendary Battle Hymn",
            Description = "Grants +20% damage, +5% crit rate, +10% attack speed",
            Type = HeritageType.BattleCry,
            Tier = HeritageTier.Gold,
            RequiredPoints = 2000,
            DamageBonus = 0.20f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "battle_cry_platinum",
            Name = "Divine War Chant",
            Description = "Grants +30% damage, +10% crit rate, +15% attack speed, +5% lifesteal",
            Type = HeritageType.BattleCry,
            Tier = HeritageTier.Platinum,
            RequiredPoints = 5000,
            DamageBonus = 0.30f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "battle_cry_diamond",
            Name = "Mythic War Saga",
            Description = "Grants +50% damage, +15% crit rate, +20% attack speed, +10% lifesteal",
            Type = HeritageType.BattleCry,
            Tier = HeritageTier.Diamond,
            RequiredPoints = 10000,
            DamageBonus = 0.50f
        });

        // Arcane Secrets Heritages
        AddHeritage(new HeritageBonus
        {
            Id = "arcane_bronze",
            Name = "Mystic Insight",
            Description = "Grants +5% magic damage and +3% mana regen",
            Type = HeritageType.ArcaneSecrets,
            Tier = HeritageTier.Bronze,
            RequiredPoints = 100,
            MagicBonus = 0.05f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "arcane_silver",
            Name = "Arcane Wisdom",
            Description = "Grants +10% magic damage, +5% mana regen, +3% cooldown reduction",
            Type = HeritageType.ArcaneSecrets,
            Tier = HeritageTier.Silver,
            RequiredPoints = 500,
            MagicBonus = 0.10f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "arcane_gold",
            Name = "Sorcerer Legacy",
            Description = "Grants +20% magic damage, +10% mana regen, +5% cooldown reduction, +100 max mana",
            Type = HeritageType.ArcaneSecrets,
            Tier = HeritageTier.Gold,
            RequiredPoints = 2000,
            MagicBonus = 0.20f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "arcane_platinum",
            Name = "Archmage Heritage",
            Description = "Grants +30% magic damage, +15% mana regen, +10% cooldown reduction, +200 max mana",
            Type = HeritageType.ArcaneSecrets,
            Tier = HeritageTier.Platinum,
            RequiredPoints = 5000,
            MagicBonus = 0.30f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "arcane_diamond",
            Name = "Mythic Arcane Dynasty",
            Description = "Grants +50% magic damage, +20% mana regen, +15% cooldown reduction, +500 max mana",
            Type = HeritageType.ArcaneSecrets,
            Tier = HeritageTier.Diamond,
            RequiredPoints = 10000,
            MagicBonus = 0.50f
        });

        // Crafting Mastery Heritages
        AddHeritage(new HeritageBonus
        {
            Id = "crafting_bronze",
            Name = "Artisan Tradition",
            Description = "Grants +5% crafting success rate and +10% material yield",
            Type = HeritageType.CraftingMastery,
            Tier = HeritageTier.Bronze,
            RequiredPoints = 100
        });

        AddHeritage(new HeritageBonus
        {
            Id = "crafting_silver",
            Name = "Master Craftsman Guild",
            Description = "Grants +10% crafting success rate, +20% material yield, +5% experience",
            Type = HeritageType.CraftingMastery,
            Tier = HeritageTier.Silver,
            RequiredPoints = 500,
            ExpBonus = 0.05f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "crafting_gold",
            Name = "Legendary Forge",
            Description = "Grants +15% crafting success rate, +30% material yield, +10% experience, chance to auto-repair",
            Type = HeritageType.CraftingMastery,
            Tier = HeritageTier.Gold,
            RequiredPoints = 2000,
            ExpBonus = 0.10f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "crafting_platinum",
            Name = "Mythic Anvil Heritage",
            Description = "Grants +20% crafting success rate, +40% material yield, +15% experience, +10% item quality",
            Type = HeritageType.CraftingMastery,
            Tier = HeritageTier.Platinum,
            RequiredPoints = 5000,
            ExpBonus = 0.15f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "crafting_diamond",
            Name = "Divine Workshop Legacy",
            Description = "Grants +30% crafting success rate, +50% material yield, +20% experience, +20% item quality, guaranteed bonus属性",
            Type = HeritageType.CraftingMastery,
            Tier = HeritageTier.Diamond,
            RequiredPoints = 10000,
            ExpBonus = 0.20f
        });

        // Trade Prosperity Heritages
        AddHeritage(new HeritageBonus
        {
            Id = "trade_bronze",
            Name = "Merchant Network",
            Description = "Grants +5% gold from sales and +3% discount at NPC shops",
            Type = HeritageType.TradeProsperity,
            Tier = HeritageTier.Bronze,
            RequiredPoints = 100,
            GoldBonus = 0.05f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "trade_silver",
            Name = "Trade Guild Alliance",
            Description = "Grants +10% gold from sales, +5% discount, reduced transaction fees",
            Type = HeritageType.TradeProsperity,
            Tier = HeritageTier.Silver,
            RequiredPoints = 500,
            GoldBonus = 0.10f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "trade_gold",
            Name = "Commercial Empire",
            Description = "Grants +20% gold from sales, +10% discount, no transaction fees, daily gold bonus",
            Type = HeritageType.TradeProsperity,
            Tier = HeritageTier.Gold,
            RequiredPoints = 2000,
            GoldBonus = 0.20f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "trade_platinum",
            Name = "Mercantile Dynasty",
            Description = "Grants +30% gold from sales, +15% discount, investment returns, marketplace benefits",
            Type = HeritageType.TradeProsperity,
            Tier = HeritageTier.Platinum,
            RequiredPoints = 5000,
            GoldBonus = 0.30f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "trade_diamond",
            Name = "Legendary Trade Empire",
            Description = "Grants +50% gold from sales, +20% discount, passive income, exclusive items access",
            Type = HeritageType.TradeProsperity,
            Tier = HeritageTier.Diamond,
            RequiredPoints = 10000,
            GoldBonus = 0.50f
        });

        // Defense Fortification Heritages
        AddHeritage(new HeritageBonus
        {
            Id = "defense_bronze",
            Name = "Stone Walls",
            Description = "Grants +5% defense and +3% damage reduction for guild members",
            Type = HeritageType.DefenseFortification,
            Tier = HeritageTier.Bronze,
            RequiredPoints = 100,
            DefenseBonus = 0.05f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "defense_silver",
            Name = "Fortified Bastion",
            Description = "Grants +10% defense, +5% damage reduction, +50 HP",
            Type = HeritageType.DefenseFortification,
            Tier = HeritageTier.Silver,
            RequiredPoints = 500,
            DefenseBonus = 0.10f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "defense_gold",
            Name = "Impenetrable Fortress",
            Description = "Grants +20% defense, +10% damage reduction, +100 HP, +5% block chance",
            Type = HeritageType.DefenseFortification,
            Tier = HeritageTier.Gold,
            RequiredPoints = 2000,
            DefenseBonus = 0.20f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "defense_platinum",
            Name = "Legendary Citadel",
            Description = "Grants +30% defense, +15% damage reduction, +200 HP, +10% block chance, +10% evasion",
            Type = HeritageType.DefenseFortification,
            Tier = HeritageTier.Platinum,
            RequiredPoints = 5000,
            DefenseBonus = 0.30f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "defense_diamond",
            Name = "Mythic Fortress Legacy",
            Description = "Grants +50% defense, +20% damage reduction, +500 HP, +15% block chance, +15% evasion, temporary invulnerability",
            Type = HeritageType.DefenseFortification,
            Tier = HeritageTier.Diamond,
            RequiredPoints = 10000,
            DefenseBonus = 0.50f
        });

        // Exploration Heritages
        AddHeritage(new HeritageBonus
        {
            Id = "exploration_bronze",
            Name = "Pathfinder Guild",
            Description = "Grants +5% movement speed and +10% XP from exploration",
            Type = HeritageType.Exploration,
            Tier = HeritageTier.Bronze,
            RequiredPoints = 100,
            ExpBonus = 0.10f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "exploration_silver",
            Name = "Cartographer's Legacy",
            Description = "Grants +10% movement speed, +15% XP, +5% item drop rate from exploration",
            Type = HeritageType.Exploration,
            Tier = HeritageTier.Silver,
            RequiredPoints = 500,
            ExpBonus = 0.15f,
            DropRateBonus = 0.05f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "exploration_gold",
            Name = "World Explorer Society",
            Description = "Grants +15% movement speed, +25% XP, +10% item drop rate, reveals hidden paths",
            Type = HeritageType.Exploration,
            Tier = HeritageTier.Gold,
            RequiredPoints = 2000,
            ExpBonus = 0.25f,
            DropRateBonus = 0.10f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "exploration_platinum",
            Name = "Legendary Adventurer",
            Description = "Grants +20% movement speed, +35% XP, +15% item drop rate, auto-discover secrets",
            Type = HeritageType.Exploration,
            Tier = HeritageTier.Platinum,
            RequiredPoints = 5000,
            ExpBonus = 0.35f,
            DropRateBonus = 0.15f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "exploration_diamond",
            Name = "Mythic Explorer Heritage",
            Description = "Grants +30% movement speed, +50% XP, +25% item drop rate, legendary discovery chance",
            Type = HeritageType.Exploration,
            Tier = HeritageTier.Diamond,
            RequiredPoints = 10000,
            ExpBonus = 0.50f,
            DropRateBonus = 0.25f
        });

        // Diplomacy Heritages
        AddHeritage(new HeritageBonus
        {
            Id = "diplomacy_bronze",
            Name = "Friendly Relations",
            Description = "Grants +5% reputation gain and +10% NPC discount",
            Type = HeritageType.Diplomacy,
            Tier = HeritageTier.Bronze,
            RequiredPoints = 100
        });

        AddHeritage(new HeritageBonus
        {
            Id = "diplomacy_silver",
            Name = "Honored Allies",
            Description = "Grants +10% reputation gain, +15% NPC discount, +5% quest rewards",
            Type = HeritageType.Diplomacy,
            Tier = HeritageTier.Silver,
            RequiredPoints = 500
        });

        AddHeritage(new HeritageBonus
        {
            Id = "diplomacy_gold",
            Name = "Diplomatic Corps",
            Description = "Grants +15% reputation gain, +20% NPC discount, +10% quest rewards, unlock exclusive quests",
            Type = HeritageType.Diplomacy,
            Tier = HeritageTier.Gold,
            RequiredPoints = 2000
        });

        AddHeritage(new HeritageBonus
        {
            Id = "diplomacy_platinum",
            Name = "Legendary Emissaries",
            Description = "Grants +20% reputation gain, +25% NPC discount, +15% quest rewards, special faction access",
            Type = HeritageType.Diplomacy,
            Tier = HeritageTier.Platinum,
            RequiredPoints = 5000
        });

        AddHeritage(new HeritageBonus
        {
            Id = "diplomacy_diamond",
            Name = "Mythic Diplomatic Legacy",
            Description = "Grants +30% reputation gain, +30% NPC discount, +25% quest rewards, all factions revered",
            Type = HeritageType.Diplomacy,
            Tier = HeritageTier.Diamond,
            RequiredPoints = 10000
        });

        // Legendary Heroes Heritages
        AddHeritage(new HeritageBonus
        {
            Id = "heroes_bronze",
            Name = "Heroic Legacy",
            Description = "Grants +5% all stats and +10% experience in group content",
            Type = HeritageType.LegendaryHeroes,
            Tier = HeritageTier.Bronze,
            RequiredPoints = 200,
            DamageBonus = 0.05f,
            DefenseBonus = 0.05f,
            MagicBonus = 0.05f,
            ExpBonus = 0.10f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "heroes_silver",
            Name = "Champion's Heritage",
            Description = "Grants +10% all stats, +15% experience in group content, +5% movement speed",
            Type = HeritageType.LegendaryHeroes,
            Tier = HeritageTier.Silver,
            RequiredPoints = 800,
            DamageBonus = 0.10f,
            DefenseBonus = 0.10f,
            MagicBonus = 0.10f,
            ExpBonus = 0.15f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "heroes_gold",
            Name = "Legendary Heroes Guild",
            Description = "Grants +15% all stats, +20% experience in group content, +10% movement speed, +10% drop rate",
            Type = HeritageType.LegendaryHeroes,
            Tier = HeritageTier.Gold,
            RequiredPoints = 3000,
            DamageBonus = 0.15f,
            DefenseBonus = 0.15f,
            MagicBonus = 0.15f,
            ExpBonus = 0.20f,
            DropRateBonus = 0.10f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "heroes_platinum",
            Name = "Mythic Heroic Dynasty",
            Description = "Grants +25% all stats, +30% experience in group content, +15% movement speed, +15% drop rate, +10% crit chance",
            Type = HeritageType.LegendaryHeroes,
            Tier = HeritageTier.Platinum,
            RequiredPoints = 8000,
            DamageBonus = 0.25f,
            DefenseBonus = 0.25f,
            MagicBonus = 0.25f,
            ExpBonus = 0.30f,
            DropRateBonus = 0.15f
        });

        AddHeritage(new HeritageBonus
        {
            Id = "heroes_diamond",
            Name = "Divine Heroic Legacy",
            Description = "Grants +40% all stats, +50% experience in group content, +25% movement speed, +25% drop rate, +15% crit chance, +10% lifesteal",
            Type = HeritageType.LegendaryHeroes,
            Tier = HeritageTier.Diamond,
            RequiredPoints = 15000,
            DamageBonus = 0.40f,
            DefenseBonus = 0.40f,
            MagicBonus = 0.40f,
            ExpBonus = 0.50f,
            DropRateBonus = 0.25f
        });
    }

    private void AddHeritage(HeritageBonus heritage)
    {
        Heritages[heritage.Id] = heritage;
        
        if (!HeritagesByType.ContainsKey(heritage.Type))
        {
            HeritagesByType[heritage.Type] = new List<string>();
        }
        HeritagesByType[heritage.Type].Add(heritage.Id);
    }

    private void InitializeTierMapping()
    {
        TierMapping[HeritageTier.Bronze] = new Dictionary<HeritageType, string>
        {
            { HeritageType.BattleCry, "battle_cry_bronze" },
            { HeritageType.ArcaneSecrets, "arcane_bronze" },
            { HeritageType.CraftingMastery, "crafting_bronze" },
            { HeritageType.TradeProsperity, "trade_bronze" },
            { HeritageType.DefenseFortification, "defense_bronze" },
            { HeritageType.Exploration, "exploration_bronze" },
            { HeritageType.Diplomacy, "diplomacy_bronze" },
            { HeritageType.LegendaryHeroes, "heroes_bronze" }
        };

        TierMapping[HeritageTier.Silver] = new Dictionary<HeritageType, string>
        {
            { HeritageType.BattleCry, "battle_cry_silver" },
            { HeritageType.ArcaneSecrets, "arcane_silver" },
            { HeritageType.CraftingMastery, "crafting_silver" },
            { HeritageType.TradeProsperity, "trade_silver" },
            { HeritageType.DefenseFortification, "defense_silver" },
            { HeritageType.Exploration, "exploration_silver" },
            { HeritageType.Diplomacy, "diplomacy_silver" },
            { HeritageType.LegendaryHeroes, "heroes_silver" }
        };

        TierMapping[HeritageTier.Gold] = new Dictionary<HeritageType, string>
        {
            { HeritageType.BattleCry, "battle_cry_gold" },
            { HeritageType.ArcaneSecrets, "arcane_gold" },
            { HeritageType.CraftingMastery, "crafting_gold" },
            { HeritageType.TradeProsperity, "trade_gold" },
            { HeritageType.DefenseFortification, "defense_gold" },
            { HeritageType.Exploration, "exploration_gold" },
            { HeritageType.Diplomacy, "diplomacy_gold" },
            { HeritageType.LegendaryHeroes, "heroes_gold" }
        };

        TierMapping[HeritageTier.Platinum] = new Dictionary<HeritageType, string>
        {
            { HeritageType.BattleCry, "battle_cry_platinum" },
            { HeritageType.ArcaneSecrets, "arcane_platinum" },
            { HeritageType.CraftingMastery, "crafting_platinum" },
            { HeritageType.TradeProsperity, "trade_platinum" },
            { HeritageType.DefenseFortification, "defense_platinum" },
            { HeritageType.Exploration, "exploration_platinum" },
            { HeritageType.Diplomacy, "diplomacy_platinum" },
            { HeritageType.LegendaryHeroes, "heroes_platinum" }
        };

        TierMapping[HeritageTier.Diamond] = new Dictionary<HeritageType, string>
        {
            { HeritageType.BattleCry, "battle_cry_diamond" },
            { HeritageType.ArcaneSecrets, "arcane_diamond" },
            { HeritageType.CraftingMastery, "crafting_diamond" },
            { HeritageType.TradeProsperity, "trade_diamond" },
            { HeritageType.DefenseFortification, "defense_diamond" },
            { HeritageType.Exploration, "exploration_diamond" },
            { HeritageType.Diplomacy, "diplomacy_diamond" },
            { HeritageType.LegendaryHeroes, "heroes_diamond" }
        };
    }

    public HeritageBonus GetHeritage(string heritageId)
    {
        return Heritages.ContainsKey(heritageId) ? Heritages[heritageId] : null;
    }

    public List<HeritageBonus> GetHeritagesByType(HeritageType type)
    {
        var result = new List<HeritageBonus>();
        if (HeritagesByType.ContainsKey(type))
        {
            foreach (var id in HeritagesByType[type])
            {
                result.Add(Heritages[id]);
            }
        }
        return result;
    }

    public List<HeritageBonus> GetNextTier(HeritageType type, HeritageTier currentTier)
    {
        var herId = TierMapping[currentTier][type];
        return GetHeritagesByType(type);
    }

    public bool CanUpgrade(GuildHeritage guild, HeritageType type)
    {
        var herId = TierMapping[HeritageTier.Diamond][type];
        if (guild.UnlockedHeritages.ContainsKey(herId))
            return false;

        var tierOrder = new[] { HeritageTier.None, HeritageTier.Bronze, HeritageTier.Silver, HeritageTier.Gold, HeritageTier.Platinum, HeritageTier.Diamond };
        
        HeritageTier currentTier = HeritageTier.None;
        foreach (var id in HeritagesByType[type])
        {
            if (guild.UnlockedHeritages.ContainsKey(id))
            {
                var heritage = Heritages[id];
                currentTier = heritage.Tier;
            }
        }

        var nextTierIndex = Array.IndexOf(tierOrder, currentTier) + 1;
        if (nextTierIndex >= tierOrder.Length)
            return false;

        var nextTier = tierOrder[nextTierIndex];
        var nextHeritageId = TierMapping[nextTier][type];
        
        return guild.TotalHeritagePoints >= Heritages[nextHeritageId].RequiredPoints;
    }
}
