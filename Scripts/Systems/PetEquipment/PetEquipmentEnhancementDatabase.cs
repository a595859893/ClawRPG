using System;
using System.Collections.Generic;
using PetEquipmentEnhancementData;

public class PetEquipmentEnhancementDatabase
{
    // Enhancement materials
    public class EnhancementMaterial
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public EnhancementTier RequiredTier { get; set; }
        public int Quantity { get; set; }
    }

    // Database of enhancement materials
    public static Dictionary<string, List<EnhancementMaterial>> MaterialsByTier = new Dictionary<string, List<EnhancementMaterial>>
    {
        { "collar", new List<EnhancementMaterial>
            {
                new EnhancementMaterial { Id = "leather_strip", Name = "Leather Strip", RequiredTier = EnhancementTier.Basic, Quantity = 3 },
                new EnhancementMaterial { Id = "iron_ingot", Name = "Iron Ingot", RequiredTier = EnhancementTier.Advanced, Quantity = 2 },
                new EnhancementMaterial { Id = "mithril_ingot", Name = "Mithril Ingot", RequiredTier = EnhancementTier.Epic, Quantity = 2 },
                new EnhancementMaterial { Id = "adamantite_ingot", Name = "Adamantite Ingot", RequiredTier = EnhancementTier.Legendary, Quantity = 2 },
                new EnhancementMaterial { Id = "dragon_scale", Name = "Dragon Scale", RequiredTier = EnhancementTier.Mythic, Quantity = 1 }
            }
        },
        { "harness", new List<EnhancementMaterial>
            {
                new EnhancementMaterial { Id = "leather_strip", Name = "Leather Strip", RequiredTier = EnhancementTier.Basic, Quantity = 3 },
                new EnhancementMaterial { Id = "steel_ingot", Name = "Steel Ingot", RequiredTier = EnhancementTier.Advanced, Quantity = 2 },
                new EnhancementMaterial { Id = "silver_ingot", Name = "Silver Ingot", RequiredTier = EnhancementTier.Epic, Quantity = 2 },
                new EnhancementMaterial { Id = "gold_ingot", Name = "Gold Ingot", RequiredTier = EnhancementTier.Legendary, Quantity = 2 },
                new EnhancementMaterial { Id = "phoenix_feather", Name = "Phoenix Feather", RequiredTier = EnhancementTier.Mythic, Quantity = 1 }
            }
        },
        { "armor", new List<EnhancementMaterial>
            {
                new EnhancementMaterial { Id = "iron_ingot", Name = "Iron Ingot", RequiredTier = EnhancementTier.Basic, Quantity = 3 },
                new EnhancementMaterial { Id = "steel_ingot", Name = "Steel Ingot", RequiredTier = EnhancementTier.Advanced, Quantity = 2 },
                new EnhancementMaterial { Id = "mithril_ingot", Name = "Mithril Ingot", RequiredTier = EnhancementTier.Epic, Quantity = 2 },
                new EnhancementMaterial { Id = "adamantite_ingot", Name = "Adamantite Ingot", RequiredTier = EnhancementTier.Legendary, Quantity = 2 },
                new EnhancementMaterial { Id = "titanium_ingot", Name = "Titanium Ingot", RequiredTier = EnhancementTier.Mythic, Quantity = 1 }
            }
        },
        { "accessory", new List<EnhancementMaterial>
            {
                new EnhancementMaterial { Id = "copper_gem", Name = "Copper Gem", RequiredTier = EnhancementTier.Basic, Quantity = 2 },
                new EnhancementMaterial { Id = "silver_gem", Name = "Silver Gem", RequiredTier = EnhancementTier.Advanced, Quantity = 2 },
                new EnhancementMaterial { Id = "gold_gem", Name = "Gold Gem", RequiredTier = EnhancementTier.Epic, Quantity = 2 },
                new EnhancementMaterial { Id = "diamond", Name = "Diamond", RequiredTier = EnhancementTier.Legendary, Quantity = 1 },
                new EnhancementMaterial { Id = "star_crystal", Name = "Star Crystal", RequiredTier = EnhancementTier.Mythic, Quantity = 1 }
            }
        },
        { "toy", new List<EnhancementMaterial>
            {
                new EnhancementMaterial { Id = "feather", Name = "Feather", RequiredTier = EnhancementTier.Basic, Quantity = 5 },
                new EnhancementMaterial { Id = "magic_dust", Name = "Magic Dust", RequiredTier = EnhancementTier.Advanced, Quantity = 3 },
                new EnhancementMaterial { Id = "enchanted_fabric", Name = "Enchanted Fabric", RequiredTier = EnhancementTier.Epic, Quantity = 2 },
                new EnhancementMaterial { Id = "soul_essence", Name = "Soul Essence", RequiredTier = EnhancementTier.Legendary, Quantity = 1 },
                new EnhancementMaterial { Id = "void_shard", Name = "Void Shard", RequiredTier = EnhancementTier.Mythic, Quantity = 1 }
            }
        }
    };

    // Get materials for equipment type and target tier
    public static List<EnhancementMaterial> GetMaterialsForEnhancement(string equipmentType, EnhancementTier targetTier)
    {
        if (MaterialsByTier.TryGetValue(equipmentType.ToLower(), out var materials))
        {
            List<EnhancementMaterial> result = new List<EnhancementMaterial>();
            foreach (var mat in materials)
            {
                if (mat.RequiredTier == targetTier)
                {
                    result.Add(mat);
                }
            }
            return result;
        }
        return new List<EnhancementMaterial>();
    }

    // Get enhancement cost for tier
    public static int GetEnhancementCost(EnhancementTier tier)
    {
        if (TierConfig.TryGetValue(tier, out var config))
        {
            return config.baseCost;
        }
        return 0;
    }

    // Get success rate for tier
    public static float GetSuccessRate(EnhancementTier tier)
    {
        if (TierConfig.TryGetValue(tier, out var config))
        {
            return config.successRate;
        }
        return 1.0f;
    }

    // Get critical rate for tier
    public static float GetCriticalRate(EnhancementTier tier)
    {
        if (TierConfig.TryGetValue(tier, out var config))
        {
            return config.criticalRate;
        }
        return 0f;
    }

    // Get bonus multiplier for tier
    public static float GetBonusMultiplier(EnhancementTier tier)
    {
        if (TierBonusMultiplier.TryGetValue(tier, out var bonus))
        {
            return bonus;
        }
        return 1.0f;
    }

    // Get tier name
    public static string GetTierName(EnhancementTier tier)
    {
        switch (tier)
        {
            case EnhancementTier.None: return "None";
            case EnhancementTier.Basic: return "Basic +1";
            case EnhancementTier.Advanced: return "Advanced +2";
            case EnhancementTier.Epic: return "Epic +3";
            case EnhancementTier.Legendary: return "Legendary +4";
            case EnhancementTier.Mythic: return "Mythic +5";
            default: return "Unknown";
        }
    }

    // Get tier color
    public static string GetTierColor(EnhancementTier tier)
    {
        switch (tier)
        {
            case EnhancementTier.None: return "#FFFFFF";
            case EnhancementTier.Basic: return "#90EE90";
            case EnhancementTier.Advanced: return "#87CEEB";
            case EnhancementTier.Epic: return "#9370DB";
            case EnhancementTier.Legendary: return "#FFD700";
            case EnhancementTier.Mythic: return "#FF4500";
            default: return "#FFFFFF";
        }
    }
}
