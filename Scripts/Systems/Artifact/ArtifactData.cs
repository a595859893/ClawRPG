// ============================================
// Artifact System - 神器系统
// 功能：神器收集、强化、合成、赐福
// ============================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Core.Systems
{
    // ==================== Data Structures ====================
    
    public enum ArtifactType
    {
        Weapon,      // 武器神器
        Armor,       // 护甲神器
        Accessory,   // 饰品神器
        Relic,       // 遗物神器
        Covenant,    // 契约神器
        Legendary    // 传说神器
    }

    public enum ArtifactRarity
    {
        Common = 1,
        Uncommon = 2,
        Rare = 3,
        Epic = 4,
        Legendary = 5,
        Mythic = 6
    }

    public enum ArtifactSlot
    {
        Primary,     // 主武器
        Secondary,   // 副武器
        Head,        // 头部
        Chest,       // 胸部
        Hands,       // 手部
        Legs,        // 腿部
        Feet,        // 脚部
        Ring1,       // 戒指1
        Ring2,       // 戒指2
        Amulet,      // 护符
        Relic1,      // 遗物1
        Relic2       // 遗物2
    }

    public enum ArtifactEffectType
    {
        DamageIncrease,
        CriticalRate,
        CriticalDamage,
        DefenseIncrease,
        HealthMax,
        ManaMax,
        HealthRegen,
        ManaRegen,
        MoveSpeed,
        AttackSpeed,
        CooldownReduction,
        LifeSteal,
        ManaSteal,
        DodgeRate,
        BlockRate,
        FireResistance,
        IceResistance,
        LightningResistance,
        PoisonResistance,
        AllAttributes,
        ExperienceGain,
        GoldGain,
        DropRate
    }

    public enum ArtifactSetBonusType
    {
        TwoPiece,
        ThreePiece,
        FourPiece,
        FullSet
    }

    public class ArtifactEffect
    {
        public ArtifactEffectType Type { get; set; }
        public float Value { get; set; }
        public string Description { get; set; }
    }

    public class Artifact
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ArtifactType Type { get; set; }
        public ArtifactRarity Rarity { get; set; }
        public ArtifactSlot Slot { get; set; }
        public List<ArtifactEffect> Effects { get; set; }
        public int Level { get; set; }
        public int EnhancementLevel { get; set; }
        public bool IsEquipped { get; set; }
        public string SetId { get; set; }
        public DateTime AcquiredTime { get; set; }
        public int UsageCount { get; set; }
    }

    public class ArtifactSet
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int PieceCount { get; set; }
        public Dictionary<ArtifactSetBonusType, List<ArtifactEffect>> SetBonuses { get; set; }
    }

    public class ArtifactForge
    {
        public string ArtifactId { get; set; }
        public int ForgeLevel { get; set; }
        public float SuccessRate { get; set; }
        public List<ArtifactEffect> BonusEffects { get; set; }
    }

    public class PlayerArtifactData
    {
        public List<Artifact> OwnedArtifacts { get; set; }
        public List<ArtifactSet> UnlockedSets { get; set; }
        public Dictionary<string, int> ArtifactStats { get; set; }
        public int TotalArtifactsCollected { get; set; }
        public int MythicArtifacts { get; set; }
    }

    public class ArtifactStatistics
    {
        public int TotalArtifacts { get; set; }
        public int RareArtifacts { get; set; }
        public int EpicArtifacts { get; set; }
        public int LegendaryArtifacts { get; set; }
        public int MythicArtifacts { get; set; }
        public int SetsCompleted { get; set; }
        public int BestForgeLevel { get; set; }
        public int SuccessfulForges { get; set; }
        public int FailedForges { get; set; }
    }
}
