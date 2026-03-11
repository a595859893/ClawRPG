using System;
using System.Collections.Generic;
using Godot;

public class PetEquipmentEnhancementData
{
    // Enhancement tier definitions
    public enum EnhancementTier
    {
        None = 0,
        Basic = 1,
        Advanced = 2,
        Epic = 3,
        Legendary = 4,
        Mythic = 5
    }

    // Enhancement result types
    public enum EnhancementResult
    {
        Success,
        CriticalSuccess,
        Failure,
        CriticalFailure
    }

    // Enhancement configuration for each equipment
    public class EquipmentEnhancement
    {
        public string EquipmentId { get; set; }
        public int CurrentTier { get; set; }
        public EnhancementTier Tier { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public DateTime LastEnhanceTime { get; set; }
    }

    // Player enhancement data
    public class PlayerEnhancementData
    {
        public int TotalEnhancements { get; set; }
        public int SuccessCount { get; set; }
        public int CriticalCount { get; set; }
        public int FailureCount { get; set; }
        public int TotalGoldSpent { get; set; }
        public List<EquipmentEnhancement> EquipmentList { get; set; } = new List<EquipmentEnhancement>();
    }

    // Enhancement tier configuration
    public static Dictionary<EnhancementTier, (int baseCost, float successRate, float criticalRate)> TierConfig = new Dictionary<EnhancementTier, (int, float, float)>
    {
        { EnhancementTier.Basic, (100, 0.90f, 0.05f) },
        { EnhancementTier.Advanced, (500, 0.75f, 0.04f) },
        { EnhancementTier.Epic, (2000, 0.60f, 0.03f) },
        { EnhancementTier.Legendary, (8000, 0.45f, 0.02f) },
        { EnhancementTier.Mythic, (30000, 0.30f, 0.01f) }
    };

    // Attribute bonus per tier
    public static Dictionary<EnhancementTier, float> TierBonusMultiplier = new Dictionary<EnhancementTier, float>
    {
        { EnhancementTier.Basic, 1.15f },
        { EnhancementTier.Advanced, 1.30f },
        { EnhancementTier.Epic, 1.50f },
        { EnhancementTier.Legendary, 1.75f },
        { EnhancementTier.Mythic, 2.00f }
    };
}
