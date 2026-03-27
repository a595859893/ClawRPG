using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Data
{
    /// <summary>
    /// Pet combat companion data - tracks combat coordination between pet and player
    /// </summary>
    public class PetCombatCompanionData
    {
        public Dictionary<string, PetCompanionState> PetStates { get; set; } = new Dictionary<string, PetCompanionState>();
        public Dictionary<string, List<CombatComboRecord>> ComboHistory { get; set; } = new Dictionary<string, List<CombatComboRecord>>();
        public Dictionary<string, PetLearningData> LearningData { get; set; } = new Dictionary<string, PetLearningData>();
        public int TotalCombos { get; set; }
        public float TotalComboDamage { get; set; }
        public int HighestComboChain { get; set; }

        // Active companion tracking (REQ-136)
        public string ActivePetId { get; set; } = "";
        public string CurrentRole { get; set; } = "Attacker";
        public float SyncLevel { get; set; } = 0.5f;
        public int ComboCount { get; set; }
        public int MaxComboCount { get; set; }
        public int TotalAttacksAssisted { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalEnemiesDefeated { get; set; }
        public List<string> LearnedSkills { get; set; } = new List<string>();
    }

    public class PetCompanionState
    {
        public string PetId { get; set; }
        public string CurrentRole { get; set; } = "Attacker"; // Attacker/Support/Tank/Scout
        public int ComboChain { get; set; }
        public float LastAttackTime { get; set; }
        public float ComboWindow { get; set; } = 2.0f;
        public bool IsInCombo { get; set; }
        public string LastPlayerAction { get; set; }
        public Vector2 LastPlayerPosition { get; set; }
        public float SyncLevel { get; set; } = 0.5f; // 0-1, how well pet coordinates with player
    }

    public class CombatComboRecord
    {
        public string PetId { get; set; }
        public string ComboType { get; set; }
        public float Damage { get; set; }
        public float Duration { get; set; }
        public int HitCount { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class PetLearningData
    {
        public string PetId { get; set; }
        public Dictionary<string, int> EnemyTypeKills { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, float> PlayerAttackPattern { get; set; } = new Dictionary<string, float>(); // Time between attacks
        public float AveragePlayerAttackInterval { get; set; } = 1.0f;
        public int SuccessfulDodges { get; set; }
        public int FailedDodges { get; set; }
        public float DodgeSuccessRate { get; set; }
        public List<string> PreferredBehaviors { get; set; } = new List<string>();
        public float AdaptationLevel { get; set; } = 0f; // 0-100
        public DateTime LastLearningUpdate { get; set; }
    }

    public enum PetCompanionRole
    {
        Attacker,
        Support,
        Tank,
        Scout
    }

    public enum ComboType
    {
        Basic,
        Chain,
        Counter,
        Support,
        Ultimate
    }
}
