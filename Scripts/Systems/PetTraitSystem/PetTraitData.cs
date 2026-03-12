using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Data
{
    /// <summary>
    /// Represents a pet trait that provides bonuses and effects.
    /// </summary>
    public enum TraitType
    {
        Battle,
        Economic,
        Exploration,
        Social,
        Special
    }

    public enum TraitRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    [System.Serializable]
    public class PetTrait
    {
        public string Id;
        public string Name;
        public string Description;
        public TraitType Type;
        public TraitRarity Rarity;
        
        // Attribute modifiers
        public float AttackBonus = 0f;
        public float DefenseBonus = 0f;
        public float HealthBonus = 0f;
        public float SpeedBonus = 0f;
        public float CriticalBonus = 0f;
        public float EvasionBonus = 0f;
        public float ExpBonus = 0f;
        public float GoldBonus = 0f;
        public float DropRateBonus = 0f;
        
        // Special effects
        public bool DoubleGoldChance = false;
        public bool DoubleExpChance = false;
        public bool RareDropChance = false;
        public bool StealChance = false;
        public bool RegenHealth = false;
        
        // Requirements
        public int MinPetLevel = 1;
        public string RequiredPetType = "";
    }

    [System.Serializable]
    public class PetTraitData
    {
        public Dictionary<string, bool> UnlockedTraits = new Dictionary<string, bool>();
        public Dictionary<string, int> TraitLevels = new Dictionary<string, int>();
        public List<string> ActiveTraits = new List<string>();
    }
}
