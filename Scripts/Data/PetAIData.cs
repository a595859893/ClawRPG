using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Data
{
    /// <summary>
    /// Pet AI behavior configuration data
    /// </summary>
    public enum PetAIBehavior
    {
        Passive,        // Only attack when commanded
        Defensive,     // Protect owner, prioritize threats
        Aggressive,    // Always attack nearest enemy
        Tactical,      // Smart positioning and target selection
        Support        // Focus on healing/support
    }

    public enum PetAIState
    {
        Idle,
        Patrolling,
        Chasing,
        Attacking,
        Fleeing,
        Supporting,
        Returning
    }

    [Serializable]
    public class AIBehaviorConfig
    {
        public string BehaviorId;
        public string BehaviorName;
        public PetAIBehavior BehaviorType;
        public float AttackRange = 150f;
        public float ChaseRange = 300f;
        public float FleeThreshold = 0.2f; // HP percentage to start fleeing
        public float TargetSwitchTime = 3f; // Time before switching targets
        public float DodgeChance = 0.1f;
        public float BlockChance = 0.1f;
        public bool UseSkills = true;
        public float SkillCooldownThreshold = 0.5f; // Use skill when HP below this
        public float SupportRange = 200f;
    }

    [Serializable]
    public class PetAIData
    {
        public string PetId;
        public PetAIBehavior CurrentBehavior = PetAIBehavior.Aggressive;
        public PetAIState CurrentState = PetAIState.Idle;
        public Vector2 LastPosition;
        public Vector2 TargetPosition;
        public string TargetEnemyId;
        public float StateTimer;
        public float TargetSwitchTimer;
        public int EnemiesAttacked;
        public int DodgesSuccessful;
        public int BlocksSuccessful;
        public float TotalDamageDealt;
        public float TotalDamageAvoided;
    }

    [Serializable]
    public partial class PlayerPetAIData
    {
        public Dictionary<string, PetAIData> PetAIStates = new Dictionary<string, PetAIData>();
        public int TotalEnemiesDefeated;
        public int TotalDodges;
        public int TotalBlocks;
        public float TotalDamageAvoided;
    }
}
