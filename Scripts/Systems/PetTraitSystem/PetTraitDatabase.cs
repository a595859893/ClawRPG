using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Database containing all pet trait configurations.
    /// </summary>
    public static class PetTraitDatabase
    {
        private static Dictionary<string, PetTrait> _traits = new Dictionary<string, PetTrait>();
        
        public static void Initialize()
        {
            RegisterTraits();
        }
        
        private static void RegisterTraits()
        {
            // Battle Traits (Common - Epic)
            RegisterTrait(new PetTrait
            {
                Id = "berserker",
                Name = "Berserker",
                Description = "Increases attack damage by 15% in battle",
                Type = TraitType.Battle,
                Rarity = TraitRarity.Common,
                AttackBonus = 0.15f,
                MinPetLevel = 1
            });
            
            RegisterTrait(new PetTrait
            {
                Id = "guardian",
                Name = "Guardian",
                Description = "Increases defense by 15%",
                Type = TraitType.Battle,
                Rarity = TraitRarity.Common,
                DefenseBonus = 0.15f,
                MinPetLevel = 1
            });
            
            RegisterTrait(new PetTrait
            {
                Id = "swift",
                Name = "Swift",
                Description = "Increases speed by 20%",
                Type = TraitType.Battle,
                Rarity = TraitRarity.Uncommon,
                SpeedBonus = 0.20f,
                MinPetLevel = 5
            });
            
            RegisterTrait(new PetTrait
            {
                Id = "lethal",
                Name = "Lethal",
                Description = "Increases critical hit chance by 10%",
                Type = TraitType.Battle,
                Rarity = TraitRarity.Rare,
                CriticalBonus = 0.10f,
                MinPetLevel = 10
            });
            
            RegisterTrait(new PetTrait
            {
                Id = "evasive",
                Name = "Evasive",
                Description = "Increases evasion by 15%",
                Type = TraitType.Battle,
                Rarity = TraitRarity.Rare,
                EvasionBonus = 0.15f,
                MinPetLevel = 10
            });
            
            RegisterTrait(new PetTrait
            {
                Id = "titan",
                Name = "Titan",
                Description = "Increases health by 25% and defense by 10%",
                Type = TraitType.Battle,
                Rarity = TraitRarity.Epic,
                HealthBonus = 0.25f,
                DefenseBonus = 0.10f,
                MinPetLevel = 20
            });
            
            // Economic Traits
            RegisterTrait(new PetTrait
            {
                Id = "merchant",
                Name = "Merchant",
                Description = "Increases gold earned by 20%",
                Type = TraitType.Economic,
                Rarity = TraitRarity.Uncommon,
                GoldBonus = 0.20f,
                MinPetLevel = 5
            });
            
            RegisterTrait(new PetTrait
            {
                Id = "treasure_hunter",
                Name = "Treasure Hunter",
                Description = "Increases drop rate by 15%",
                Type = TraitType.Economic,
                Rarity = TraitRarity.Rare,
                DropRateBonus = 0.15f,
                MinPetLevel = 10
            });
            
            RegisterTrait(new PetTrait
            {
                Id = "fortune",
                Name = "Fortune",
                Description = "10% chance to double gold earned",
                Type = TraitType.Economic,
                Rarity = TraitRarity.Epic,
                DoubleGoldChance = true,
                MinPetLevel = 15
            });
            
            RegisterTrait(new PetTrait
            {
                Id = "midas",
                Name = "Midas Touch",
                Description = "Increases gold by 50% and drop rate by 25%",
                Type = TraitType.Economic,
                Rarity = TraitRarity.Legendary,
                GoldBonus = 0.50f,
                DropRateBonus = 0.25f,
                MinPetLevel = 30
            });
            
            // Exploration Traits
            RegisterTrait(new PetTrait
            {
                Id = "explorer",
                Name = "Explorer",
                Description = "Increases experience gained by 15%",
                Type = TraitType.Exploration,
                Rarity = TraitRarity.Common,
                ExpBonus = 0.15f,
                MinPetLevel = 1
            });
            
            RegisterTrait(new PetTrait
            {
                Id = "scholar",
                Name = "Scholar",
                Description = "Increases experience by 25%",
                Type = TraitType.Exploration,
                Rarity = TraitRarity.Rare,
                ExpBonus = 0.25f,
                MinPetLevel = 10
            });
            
            RegisterTrait(new PetTrait
            {
                Id = "wisdom",
                Name = "Wisdom",
                Description = "10% chance to double experience",
                Type = TraitType.Exploration,
                Rarity = TraitRarity.Epic,
                DoubleExpChance = true,
                MinPetLevel = 20
            });
            
            RegisterTrait(new PetTrait
            {
                Id = "enlightened",
                Name = "Enlightened",
                Description = "Increases experience by 50% and critical chance by 5%",
                Type = TraitType.Exploration,
                Rarity = TraitRarity.Legendary,
                ExpBonus = 0.50f,
                CriticalBonus = 0.05f,
                MinPetLevel = 30
            });
            
            // Social Traits
            RegisterTrait(new PetTrait
            {
                Id = "charming",
                Name = "Charming",
                Description = "Increases NPC interaction rewards by 20%",
                Type = TraitType.Social,
                Rarity = TraitRarity.Uncommon,
                GoldBonus = 0.20f,
                MinPetLevel = 5
            });
            
            RegisterTrait(new PetTrait
            {
                Id = "diplomat",
                Name = "Diplomat",
                Description = "Increases guild reputation gain by 25%",
                Type = TraitType.Social,
                Rarity = TraitRarity.Rare,
                MinPetLevel = 15
            });
            
            // Special Traits
            RegisterTrait(new PetTrait
            {
                Id = "vampiric",
                Name = "Vampiric",
                Description = "Heals for 5% of damage dealt",
                Type = TraitType.Special,
                Rarity = TraitRarity.Epic,
                RegenHealth = true,
                MinPetLevel = 25
            });
            
            RegisterTrait(new PetTrait
            {
                Id = "thief",
                Name = "Thief",
                Description = "5% chance to steal item from enemies",
                Type = TraitType.Special,
                Rarity = TraitRarity.Epic,
                StealChance = true,
                MinPetLevel = 20
            });
            
            RegisterTrait(new PetTrait
            {
                Id = "lucky",
                Name = "Lucky",
                Description = "5% chance for rare drops",
                Type = TraitType.Special,
                Rarity = TraitRarity.Rare,
                RareDropChance = true,
                MinPetLevel = 15
            });
            
            RegisterTrait(new PetTrait
            {
                Id = "mythical",
                Name = "Mythical",
                Description = "All bonuses increased by 50%",
                Type = TraitType.Special,
                Rarity = TraitRarity.Legendary,
                AttackBonus = 0.25f,
                DefenseBonus = 0.25f,
                HealthBonus = 0.25f,
                SpeedBonus = 0.25f,
                ExpBonus = 0.25f,
                GoldBonus = 0.25f,
                DropRateBonus = 0.25f,
                MinPetLevel = 40
            });
        }
        
        private static void RegisterTrait(PetTrait trait)
        {
            _traits[trait.Id] = trait;
        }
        
        public static PetTrait GetTrait(string id)
        {
            if (_traits.ContainsKey(id))
                return _traits[id];
            return null;
        }
        
        public static List<PetTrait> GetAllTraits()
        {
            return new List<PetTrait>(_traits.Values);
        }
        
        public static List<PetTrait> GetTraitsByType(TraitType type)
        {
            List<PetTrait> result = new List<PetTrait>();
            foreach (var trait in _traits.Values)
            {
                if (trait.Type == type)
                    result.Add(trait);
            }
            return result;
        }
        
        public static List<PetTrait> GetTraitsByRarity(TraitRarity rarity)
        {
            List<PetTrait> result = new List<PetTrait>();
            foreach (var trait in _traits.Values)
            {
                if (trait.Rarity == rarity)
                    result.Add(trait);
            }
            return result;
        }
        
        public static List<PetTrait> GetUnlockableTraits(int petLevel, string petType = "")
        {
            List<PetTrait> result = new List<PetTrait>();
            foreach (var trait in _traits.Values)
            {
                if (petLevel >= trait.MinPetLevel)
                {
                    if (string.IsNullOrEmpty(trait.RequiredPetType) || trait.RequiredPetType == petType)
                        result.Add(trait);
                }
            }
            return result;
        }
        
        public static int GetTotalTraitCount() => _traits.Count;
        
        public static int GetTraitCountByRarity(TraitRarity rarity)
        {
            int count = 0;
            foreach (var trait in _traits.Values)
            {
                if (trait.Rarity == rarity)
                    count++;
            }
            return count;
        }
    }
}
