using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Meditation
{
    /// <summary>
    /// Meditation database configuration
    /// </summary>
    public partial class MeditationDatabase : BaseSystem
    {
        public static MeditationDatabase Instance { get; private set; }
        
        // Meditation type configurations
        public Dictionary<MeditationType, MeditationTypeConfig> TypeConfigs { get; private set; } = new Dictionary<MeditationType, MeditationTypeConfig>();
        
        // Benefit configurations
        public List<MeditationBenefit> Benefits { get; private set; } = new List<MeditationBenefit>();
        
        // Unlock requirements
        public Dictionary<string, int> FocusToUnlock { get; private set; } = new Dictionary<string, int>();
        
        public override void _Ready()
        {
            Instance = this;
            InitializeDatabase();
        }
        
        private void InitializeDatabase()
        {
            // Initialize meditation type configs
            InitializeTypeConfigs();
            
            // Initialize benefits
            InitializeBenefits();
            
            // Initialize unlock requirements
            InitializeUnlockRequirements();
        }
        
        private void InitializeTypeConfigs()
        {
            TypeConfigs[MeditationType.Focus] = new MeditationTypeConfig
            {
                Type = MeditationType.Focus,
                DisplayName = "Focus Meditation",
                Description = "Increases concentration and focus",
                BaseFocusGain = 15,
                MinDuration = 30,
                MaxDuration = 300,
                Cooldown = 60,
                Color = new Color(0.2f, 0.6f, 1.0f),
                IconPath = "res://icons/focus.png"
            };
            
            TypeConfigs[MeditationType.Healing] = new MeditationTypeConfig
            {
                Type = MeditationType.Healing,
                DisplayName = "Healing Meditation",
                Description = "Restores health through inner peace",
                BaseFocusGain = 10,
                MinDuration = 60,
                MaxDuration = 180,
                Cooldown = 120,
                Color = new Color(0.2f, 0.9f, 0.4f),
                IconPath = "res://icons/healing.png"
            };
            
            TypeConfigs[MeditationType.Clarity] = new MeditationTypeConfig
            {
                Type = MeditationType.Clarity,
                DisplayName = "Clarity Meditation",
                Description = "Clears negative effects and mental fog",
                BaseFocusGain = 12,
                MinDuration = 45,
                MaxDuration = 150,
                Cooldown = 180,
                Color = new Color(0.9f, 0.9f, 0.3f),
                IconPath = "res://icons/clarity.png"
            };
            
            TypeConfigs[MeditationType.Strength] = new MeditationTypeConfig
            {
                Type = MeditationType.Strength,
                DisplayName = "Strength Meditation",
                Description = "Channel inner strength for combat",
                BaseFocusGain = 8,
                MinDuration = 30,
                MaxDuration = 120,
                Cooldown = 300,
                Color = new Color(0.9f, 0.3f, 0.2f),
                IconPath = "res://icons/strength.png"
            };
            
            TypeConfigs[MeditationType.Defense] = new MeditationTypeConfig
            {
                Type = MeditationType.Defense,
                DisplayName = "Defense Meditation",
                Description = "Strengthen mental defenses",
                BaseFocusGain = 8,
                MinDuration = 30,
                MaxDuration = 120,
                Cooldown = 300,
                Color = new Color(0.3f, 0.5f, 0.9f),
                IconPath = "res://icons/defense.png"
            };
            
            TypeConfigs[MeditationType.Speed] = new MeditationTypeConfig
            {
                Type = MeditationType.Speed,
                DisplayName = "Speed Meditation",
                Description = "Accelerate mental processes",
                BaseFocusGain = 8,
                MinDuration = 30,
                MaxDuration = 120,
                Cooldown = 300,
                Color = new Color(0.9f, 0.7f, 0.2f),
                IconPath = "res://icons/speed.png"
            };
            
            TypeConfigs[MeditationType.Wisdom] = new MeditationTypeConfig
            {
                Type = MeditationType.Wisdom,
                DisplayName = "Wisdom Meditation",
                Description = "Gain insight for enhanced learning",
                BaseFocusGain = 10,
                MinDuration = 60,
                MaxDuration = 240,
                Cooldown = 180,
                Color = new Color(0.6f, 0.3f, 0.9f),
                IconPath = "res://icons/wisdom.png"
            };
            
            TypeConfigs[MeditationType.Endurance] = new MeditationTypeConfig
            {
                Type = MeditationType.Endurance,
                DisplayName = "Endurance Meditation",
                Description = "Push beyond physical limits",
                BaseFocusGain = 8,
                MinDuration = 45,
                MaxDuration = 150,
                Cooldown = 300,
                Color = new Color(0.8f, 0.4f, 0.3f),
                IconPath = "res://icons/endurance.png"
            };
            
            TypeConfigs[MeditationType.Spirit] = new MeditationTypeConfig
            {
                Type = MeditationType.Spirit,
                DisplayName = "Spirit Meditation",
                Description = "Restore magical energy",
                BaseFocusGain = 10,
                MinDuration = 45,
                MaxDuration = 180,
                Cooldown = 120,
                Color = new Color(0.4f, 0.8f, 0.9f),
                IconPath = "res://icons/spirit.png"
            };
            
            TypeConfigs[MeditationType.Balance] = new MeditationTypeConfig
            {
                Type = MeditationType.Balance,
                DisplayName = "Balance Meditation",
                Description = "Achieve harmony in all aspects",
                BaseFocusGain = 5,
                MinDuration = 120,
                MaxDuration = 300,
                Cooldown = 600,
                Color = new Color(0.7f, 0.7f, 0.7f),
                IconPath = "res://icons/balance.png"
            };
        }
        
        private void InitializeBenefits()
        {
            // Focus benefits
            Benefits.Add(new MeditationBenefit
            {
                BenefitId = "focus_master",
                BenefitName = "Focus Master",
                Description = "Maximum focus increased",
                Type = MeditationType.Focus,
                MinDuration = 180,
                EffectMultiplier = 1.5f,
                StatAffected = "FocusGain",
                BaseValue = 5,
                Duration = -1 // Permanent
            });
            
            Benefits.Add(new MeditationBenefit
            {
                BenefitId = "concentration",
                BenefitName = "Concentration",
                Description = "Critical hit chance increased",
                Type = MeditationType.Focus,
                MinDuration = 60,
                EffectMultiplier = 1.0f,
                StatAffected = "CriticalChance",
                BaseValue = 0.05f,
                Duration = 300
            });
            
            // Healing benefits
            Benefits.Add(new MeditationBenefit
            {
                BenefitId = "healing_blessing",
                BenefitName = "Healing Blessing",
                Description = "Health restored",
                Type = MeditationType.Healing,
                MinDuration = 60,
                EffectMultiplier = 1.0f,
                StatAffected = "Health",
                BaseValue = 50,
                Duration = -1
            });
            
            Benefits.Add(new MeditationBenefit
            {
                BenefitId = "regeneration",
                BenefitName = "Regeneration",
                Description = "Health regeneration increased",
                Type = MeditationType.Healing,
                MinDuration = 120,
                EffectMultiplier = 1.2f,
                StatAffected = "HealthRegen",
                BaseValue = 2f,
                Duration = 600
            });
            
            // Clarity benefits
            Benefits.Add(new MeditationBenefit
            {
                BenefitId = "clear_mind",
                BenefitName = "Clear Mind",
                Description = "All negative effects removed",
                Type = MeditationType.Clarity,
                MinDuration = 45,
                EffectMultiplier = 1.0f,
                StatAffected = "RemoveDebuffs",
                BaseValue = 1,
                Duration = -1
            });
            
            // Strength benefits
            Benefits.Add(new MeditationBenefit
            {
                BenefitId = "inner_strength",
                BenefitName = "Inner Strength",
                Description = "Attack power increased",
                Type = MeditationType.Strength,
                MinDuration = 30,
                EffectMultiplier = 1.0f,
                StatAffected = "AttackPower",
                BaseValue = 0.15f,
                Duration = 180
            });
            
            // Defense benefits
            Benefits.Add(new MeditationBenefit
            {
                BenefitId = "iron_will",
                BenefitName = "Iron Will",
                Description = "Defense increased",
                Type = MeditationType.Defense,
                MinDuration = 30,
                EffectMultiplier = 1.0f,
                StatAffected = "Defense",
                BaseValue = 0.15f,
                Duration = 180
            });
            
            // Speed benefits
            Benefits.Add(new MeditationBenefit
            {
                BenefitId = "swift_mind",
                BenefitName = "Swift Mind",
                Description = "Action speed increased",
                Type = MeditationType.Speed,
                MinDuration = 30,
                EffectMultiplier = 1.0f,
                StatAffected = "ActionSpeed",
                BaseValue = 0.1f,
                Duration = 180
            });
            
            // Wisdom benefits
            Benefits.Add(new MeditationBenefit
            {
                BenefitId = "insight",
                BenefitName = "Insight",
                Description = "Experience gain increased",
                Type = MeditationType.Wisdom,
                MinDuration = 60,
                EffectMultiplier = 1.0f,
                StatAffected = "ExperienceGain",
                BaseValue = 0.2f,
                Duration = 300
            });
            
            // Endurance benefits
            Benefits.Add(new MeditationBenefit
            {
                BenefitId = "titan_form",
                BenefitName = "Titan Form",
                Description = "Maximum health increased",
                Type = MeditationType.Endurance,
                MinDuration = 45,
                EffectMultiplier = 1.0f,
                StatAffected = "MaxHealth",
                BaseValue = 0.1f,
                Duration = 300
            });
            
            // Spirit benefits
            Benefits.Add(new MeditationBenefit
            {
                BenefitId = "mana_flow",
                BenefitName = "Mana Flow",
                Description = "Magic energy restored",
                Type = MeditationType.Spirit,
                MinDuration = 45,
                EffectMultiplier = 1.0f,
                StatAffected = "Mana",
                BaseValue = 30,
                Duration = -1
            });
            
            // Balance benefits
            Benefits.Add(new MeditationBenefit
            {
                BenefitId = "harmony",
                BenefitName = "Harmony",
                Description = "All stats increased slightly",
                Type = MeditationType.Balance,
                MinDuration = 120,
                EffectMultiplier = 1.0f,
                StatAffected = "AllStats",
                BaseValue = 0.05f,
                Duration = 240
            });
        }
        
        private void InitializeUnlockRequirements()
        {
            FocusToUnlock["Clarity"] = 20;
            FocusToUnlock["Strength"] = 30;
            FocusToUnlock["Defense"] = 30;
            FocusToUnlock["Speed"] = 40;
            FocusToUnlock["Wisdom"] = 50;
            FocusToUnlock["Endurance"] = 60;
            FocusToUnlock["Spirit"] = 70;
            FocusToUnlock["Balance"] = 100;
        }
        
        public MeditationTypeConfig GetTypeConfig(MeditationType type)
        {
            return TypeConfigs.ContainsKey(type) ? TypeConfigs[type] : null;
        }
        
        public List<MeditationBenefit> GetBenefitsForType(MeditationType type, int duration)
        {
            var result = new List<MeditationBenefit>();
            foreach (var benefit in Benefits)
            {
                if (benefit.Type == type && duration >= benefit.MinDuration)
                {
                    result.Add(benefit);
                }
            }
            return result;
        }
        
        public bool IsMeditationUnlocked(MeditationType type, int currentFocus)
        {
            string typeName = type.ToString();
            return !FocusToUnlock.ContainsKey(typeName) || currentFocus >= FocusToUnlock[typeName];
        }
    }
    
    /// <summary>
    /// Configuration for each meditation type
    /// </summary>
    public class MeditationTypeConfig
    {
        public MeditationType Type { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public int BaseFocusGain { get; set; }
        public int MinDuration { get; set; }
        public int MaxDuration { get; set; }
        public int Cooldown { get; set; }
        public Color Color { get; set; }
        public string IconPath { get; set; }

        public Dictionary<string, object> ExportSaveData() => new();
        public void ImportSaveData(Dictionary<string, object> data) { }
    }
} // namespace ClawRPG.Systems.Meditation
