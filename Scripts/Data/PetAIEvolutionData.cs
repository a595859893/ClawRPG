using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Data
{
    /// <summary>
    /// Pet AI Evolution Types - how pets learn and improve
    /// </summary>
    public enum PetAIEvolutionType
    {
        AggressionMaster,      // Master of aggression
        DefenseExpert,         // Defense specialist
        SpeedDemon,            // Speed and evasion expert
        SupportMaster,         // Healing and support expert
        TacticalGenius,       // Smart positioning
        Survivalist,           // Survival expert
        Berserker,            // Low HP = high damage
        Guardian              // Protect others at all costs
    }

    /// <summary>
    /// Pet AI learning history
    /// </summary>
    [Serializable]
    public class PetAIEvolutionData
    {
        public string PetId;
        public List<PetAIEvolutionType> UnlockedEvolutions = new List<PetAIEvolutionType>();
        public Dictionary<PetAIEvolutionType, float> EvolutionProgress = new Dictionary<PetAIEvolutionType, float>();
        public int TotalBattlesFought;
        public int BattlesWon;
        public int TotalDamageDealt;
        public int TotalDamageTaken;
        public int TotalHealingDone;
        public int EnemiesDefeated;
        public float BestSurvivalRate; // Highest survival rate in a battle
        public int ComboKills; // Kills in quick succession
        public int HighestCombo;
        public DateTime LastEvolutionTime;
        public bool HasLearnedFromDefeat;
        
        // Learning thresholds
        public const float EvolutionThreshold = 100f; // Points needed to unlock
        public const int ComboThreshold = 3; // Kills needed for combo bonus
    }

    /// <summary>
    /// Evolution bonus configuration
    /// </summary>
    [Serializable]
    public class AIEvolutionBonus
    {
        public PetAIEvolutionType EvolutionType;
        public string EvolutionName;
        public string Description;
        
        // Stat bonuses when active
        public float DamageBonus = 1.0f;
        public float DefenseBonus = 1.0f;
        public float SpeedBonus = 1.0f;
        public float HealBonus = 1.0f;
        
        // Special abilities
        public bool CanCounterAttack;
        public bool CanLifeSteal;
        public bool CanEvade;
        public bool CanProtect;
        public bool CanRegenerate;
        
        // Thresholds to activate
        public float ActivationHPThreshold = 0.5f; // Below this HP%
        public float ActivationComboThreshold = 3; // Above this combo
    }

    /// <summary>
    /// Player's pet AI evolution records
    /// </summary>
    [Serializable]
    public class PlayerPetAIEvolutionData
    {
        public Dictionary<string, PetAIEvolutionData> PetEvolutions = new Dictionary<string, PetAIEvolutionData>();
        public int TotalEvolutionsUnlocked;
        public DateTime LastBattleTime;
    }
}
