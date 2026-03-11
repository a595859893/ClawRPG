using Godot;
using System;
using System.Collections.Generic;

namespace Game
{
    public enum ArtifactRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
        Mythical = 5
    }

    public enum ArtifactType
    {
        Weapon,
        Armor,
        Accessory,
        Relic
    }

    public enum ArtifactEffectType
    {
        StatBoost,
        SkillBoost,
        CombatBonus,
        EconomicBonus,
        Utility,
        Special
    }

    [System.Serializable]
    public class ArtifactEffect
    {
        public ArtifactEffectType EffectType { get; set; }
        public string StatName { get; set; }
        public float Value { get; set; }
        public string Description { get; set; }
    }

    [System.Serializable]
    public class Artifact
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ArtifactType Type { get; set; }
        public ArtifactRarity Rarity { get; set; }
        public List<ArtifactEffect> Effects { get; set; }
        public string SetId { get; set; }
        public int SetCount { get; set; }
        public string Lore { get; set; }
        public string Origin { get; set; }
        public float DropRate { get; set; }
    }

    [System.Serializable]
    public class PlayerArtifactData
    {
        public List<string> UnlockedArtifactIds { get; set; }
        public List<string> EquippedArtifactIds { get; set; }
        public Dictionary<string, int> ArtifactCount { get; set; }
        public int TotalArtifacts { get; set; }
        public int LegendaryFound { get; set; }
        public int MythicalFound { get; set; }

        public PlayerArtifactData()
        {
            UnlockedArtifactIds = new List<string>();
            EquippedArtifactIds = new List<string>();
            ArtifactCount = new Dictionary<string, int>();
        }
    }

    [System.Serializable]
    public class ActiveArtifactBuff
    {
        public string ArtifactId { get; set; }
        public ArtifactEffect Effect { get; set; }
        public float Duration { get; set; }
        public float RemainingTime { get; set; }
    }
}
