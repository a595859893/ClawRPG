using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// Pet AI Evolution database - configuration for pet learning system
    /// </summary>
    public static class PetAIEvolutionDatabase
    {
        private static Dictionary<PetAIEvolutionType, AIEvolutionBonus> _evolutions = new Dictionary<PetAIEvolutionType, AIEvolutionBonus>();
        
        static PetAIEvolutionDatabase()
        {
            InitializeEvolutions();
        }

        private static void InitializeEvolutions()
        {
            // AggressionMaster - Damage dealer specialization
            _evolutions[PetAIEvolutionType.AggressionMaster] = new AIEvolutionBonus
            {
                EvolutionType = PetAIEvolutionType.AggressionMaster,
                EvolutionName = "攻击大师",
                Description = "专注于造成更多伤害",
                DamageBonus = 1.5f,
                SpeedBonus = 1.1f,
                CanCounterAttack = true
            };

            // DefenseExpert - Tank specialization
            _evolutions[PetAIEvolutionType.DefenseExpert] = new AIEvolutionBonus
            {
                EvolutionType = PetAIEvolutionType.DefenseExpert,
                EvolutionName = "防御专家",
                Description = "专注于减少受到的伤害",
                DefenseBonus = 1.5f,
                CanProtect = true,
                CanRegenerate = true,
                ActivationHPThreshold = 0.4f
            };

            // SpeedDemon - Evasion specialist
            _evolutions[PetAIEvolutionType.SpeedDemon] = new AIEvolutionBonus
            {
                EvolutionType = PetAIEvolutionType.SpeedDemon,
                EvolutionName = "速度恶魔",
                Description = "闪避和速度专家",
                SpeedBonus = 1.5f,
                CanEvade = true,
                DamageBonus = 1.2f
            };

            // SupportMaster - Healing specialization
            _evolutions[PetAIEvolutionType.SupportMaster] = new AIEvolutionBonus
            {
                EvolutionType = PetAIEvolutionType.SupportMaster,
                EvolutionName = "辅助大师",
                Description = "治疗和辅助专家",
                HealBonus = 1.5f,
                CanLifeSteal = true,
                CanProtect = true
            };

            // TacticalGenius - Smart positioning
            _evolutions[PetAIEvolutionType.TacticalGenius] = new AIEvolutionBonus
            {
                EvolutionType = PetAIEvolutionType.TacticalGenius,
                EvolutionName = "战术天才",
                Description = "智能走位和目标选择",
                DamageBonus = 1.2f,
                DefenseBonus = 1.2f,
                SpeedBonus = 1.1f,
                CanEvade = true,
                CanCounterAttack = true
            };

            // Survivalist - Survival specialist
            _evolutions[PetAIEvolutionType.Survivalist] = new AIEvolutionBonus
            {
                EvolutionType = PetAIEvolutionType.Survivalist,
                EvolutionName = "生存专家",
                Description = "极限生存能力",
                DefenseBonus = 1.3f,
                CanRegenerate = true,
                CanEvade = true,
                ActivationHPThreshold = 0.3f
            };

            // Berserker - Low HP = high damage
            _evolutions[PetAIEvolutionType.Berserker] = new AIEvolutionBonus
            {
                EvolutionType = PetAIEvolutionType.Berserker,
                EvolutionName = "狂战士",
                Description = "血量越低，伤害越高",
                DamageBonus = 2.0f,
                SpeedBonus = 1.2f,
                CanCounterAttack = true,
                ActivationHPThreshold = 0.25f
            };

            // Guardian - Protect others
            _evolutions[PetAIEvolutionType.Guardian] = new AIEvolutionBonus
            {
                EvolutionType = PetAIEvolutionType.Guardian,
                EvolutionName = "守护者",
                Description = "随时保护主人和其他宠物",
                DefenseBonus = 1.4f,
                CanProtect = true,
                CanRegenerate = true,
                ActivationHPThreshold = 0.5f
            };
        }

        public static AIEvolutionBonus GetEvolution(PetAIEvolutionType type)
        {
            if (_evolutions.TryGetValue(type, out var evolution))
                return evolution;
            
            return null;
        }

        public static AIEvolutionBonus GetEvolutionById(string typeId)
        {
            if (Enum.TryParse<PetAIEvolutionType>(typeId, true, out var type))
                return GetEvolution(type);
            
            return null;
        }

        public static List<AIEvolutionBonus> GetAllEvolutions()
        {
            return new List<AIEvolutionBonus>(_evolutions.Values);
        }

        public static List<AIEvolutionBonus> GetAvailableEvolutions(PetAIEvolutionData evolutionData)
        {
            var available = new List<AIEvolutionBonus>();
            
            foreach (var evolution in _evolutions.Values)
            {
                // Already unlocked
                if (evolutionData.UnlockedEvolutions.Contains(evolution.EvolutionType))
                    continue;
                
                available.Add(evolution);
            }
            
            return available;
        }

        /// <summary>
        /// Calculate which evolution to prioritize based on battle stats
        /// </summary>
        public static PetAIEvolutionType CalculateBestEvolution(PetAIEvolutionData data)
        {
            if (data.TotalBattlesFought < 10)
                return PetAIEvolutionType.TacticalGenius; // Default for new pets
            
            float damageRatio = data.TotalDamageDealt > 0 ? (float)data.TotalDamageTaken / data.TotalDamageDealt : 1f;
            float winRate = data.TotalBattlesFought > 0 ? (float)data.BattlesWon / data.TotalBattlesFought : 0f;
            float survivalRate = data.BestSurvivalRate;
            
            // Decision logic
            if (data.ComboKills >= 10 || data.HighestCombo >= 5)
                return PetAIEvolutionType.AggressionMaster;
            
            if (damageRatio > 2.0f && survivalRate < 0.5f)
                return PetAIEvolutionType.DefenseExpert;
            
            if (survivalRate < 0.3f)
                return PetAIEvolutionType.Survivalist;
            
            if (data.TotalHealingDone > data.TotalDamageDealt * 0.3f)
                return PetAIEvolutionType.SupportMaster;
            
            if (winRate > 0.8f && damageRatio < 1.0f)
                return PetAIEvolutionType.TacticalGenius;
            
            if (damageRatio > 1.5f)
                return PetAIEvolutionType.Berserker;
            
            return PetAIEvolutionType.Guardian;
        }
    }
}
