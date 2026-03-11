using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// Pet AI behavior configuration database
    /// </summary>
    public static class PetAIDatabase
    {
        private static Dictionary<string, AIBehaviorConfig> _behaviors = new Dictionary<string, AIBehaviorConfig>();
        
        static PetAIDatabase()
        {
            InitializeBehaviors();
        }

        private static void InitializeBehaviors()
        {
            // Passive - Only attacks when commanded
            _behaviors["passive"] = new AIBehaviorConfig
            {
                BehaviorId = "passive",
                BehaviorName = "被动",
                BehaviorType = PetAIBehavior.Passive,
                AttackRange = 100f,
                ChaseRange = 150f,
                FleeThreshold = 0.1f,
                TargetSwitchTime = 10f,
                DodgeChance = 0.05f,
                BlockChance = 0.05f,
                UseSkills = false
            };

            // Defensive - Protect owner, prioritize threats
            _behaviors["defensive"] = new AIBehaviorConfig
            {
                BehaviorId = "defensive",
                BehaviorName = "防御",
                BehaviorType = PetAIBehavior.Defensive,
                AttackRange = 180f,
                ChaseRange = 250f,
                FleeThreshold = 0.15f,
                TargetSwitchTime = 5f,
                DodgeChance = 0.15f,
                BlockChance = 0.2f,
                UseSkills = true,
                SkillCooldownThreshold = 0.6f,
                SupportRange = 180f
            };

            // Aggressive - Always attack nearest enemy
            _behaviors["aggressive"] = new AIBehaviorConfig
            {
                BehaviorId = "aggressive",
                BehaviorName = "激进",
                BehaviorType = PetAIBehavior.Aggressive,
                AttackRange = 200f,
                ChaseRange = 400f,
                FleeThreshold = 0.05f,
                TargetSwitchTime = 2f,
                DodgeChance = 0.1f,
                BlockChance = 0.05f,
                UseSkills = true,
                SkillCooldownThreshold = 0.3f
            };

            // Tactical - Smart positioning and target selection
            _behaviors["tactical"] = new AIBehaviorConfig
            {
                BehaviorId = "tactical",
                BehaviorName = "战术",
                BehaviorType = PetAIBehavior.Tactical,
                AttackRange = 150f,
                ChaseRange = 300f,
                FleeThreshold = 0.25f,
                TargetSwitchTime = 4f,
                DodgeChance = 0.25f,
                BlockChance = 0.25f,
                UseSkills = true,
                SkillCooldownThreshold = 0.5f,
                SupportRange = 200f
            };

            // Support - Focus on healing/support
            _behaviors["support"] = new AIBehaviorConfig
            {
                BehaviorId = "support",
                BehaviorName = "辅助",
                BehaviorType = PetAIBehavior.Support,
                AttackRange = 120f,
                ChaseRange = 200f,
                FleeThreshold = 0.3f,
                TargetSwitchTime = 8f,
                DodgeChance = 0.1f,
                BlockChance = 0.15f,
                UseSkills = true,
                SkillCooldownThreshold = 0.7f,
                SupportRange = 250f
            };
        }

        public static AIBehaviorConfig GetBehavior(string behaviorId)
        {
            if (_behaviors.TryGetValue(behaviorId, out var config))
                return config;
            
            return _behaviors["aggressive"]; // Default
        }

        public static AIBehaviorConfig GetBehaviorByType(PetAIBehavior behaviorType)
        {
            foreach (var config in _behaviors.Values)
            {
                if (config.BehaviorType == behaviorType)
                    return config;
            }
            
            return _behaviors["aggressive"];
        }

        public static List<AIBehaviorConfig> GetAllBehaviors()
        {
            return new List<AIBehaviorConfig>(_behaviors.Values);
        }

        public static List<string> GetBehaviorIds()
        {
            return new List<string>(_behaviors.Keys);
        }
    }
}
