using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Skill Mastery Database - Configuration for skill mastery system
    /// </summary>
    public class SkillMasteryDatabase
    {
        private static SkillMasteryDatabase _instance;
        public static SkillMasteryDatabase Instance => _instance ?? new SkillMasteryDatabase();

        // Tier configurations
        public Dictionary<SkillMasteryData.MasteryTier, SkillMasteryData.MasteryTierInfo> TierConfigs { get; private set; }
        
        // Bonus configurations by skill type
        public Dictionary<SkillMasteryData.SkillType, List<SkillMasteryData.MasteryBonus>> BonusesByType { get; private set; }
        
        // Global bonuses unlocked at certain total mastery
        public List<SkillMasteryData.MasteryBonus> GlobalBonuses { get; private set; }

        public SkillMasteryDatabase()
        {
            _instance = this;
            InitializeTiers();
            InitializeBonuses();
            InitializeGlobalBonuses();
        }

        private void InitializeTiers()
        {
            TierConfigs = new Dictionary<SkillMasteryData.MasteryTier, SkillMasteryData.MasteryTierInfo>
            {
                { SkillMasteryData.MasteryTier.Novice, new SkillMasteryData.MasteryTierInfo
                {
                    Tier = SkillMasteryData.MasteryTier.Novice,
                    DisplayName = "Novice",
                    MinPoints = 0,
                    DamageBonus = 0f,
                    CooldownReduction = 0f,
                    ManaCostReduction = 0f
                }},
                { SkillMasteryData.MasteryTier.Apprentice, new SkillMasteryData.MasteryTierInfo
                {
                    Tier = SkillMasteryData.MasteryTier.Apprentice,
                    DisplayName = "Apprentice",
                    MinPoints = 100,
                    DamageBonus = 0.05f,
                    CooldownReduction = 0.02f,
                    ManaCostReduction = 0.02f
                }},
                { SkillMasteryData.MasteryTier.Journeyman, new SkillMasteryData.MasteryTierInfo
                {
                    Tier = SkillMasteryData.MasteryTier.Journeyman,
                    DisplayName = "Journeyman",
                    MinPoints = 500,
                    DamageBonus = 0.10f,
                    CooldownReduction = 0.05f,
                    ManaCostReduction = 0.05f
                }},
                { SkillMasteryData.MasteryTier.Expert, new SkillMasteryData.MasteryTierInfo
                {
                    Tier = SkillMasteryData.MasteryTier.Expert,
                    DisplayName = "Expert",
                    MinPoints = 2000,
                    DamageBonus = 0.15f,
                    CooldownReduction = 0.08f,
                    ManaCostReduction = 0.08f
                }},
                { SkillMasteryData.MasteryTier.Master, new SkillMasteryData.MasteryTierInfo
                {
                    Tier = SkillMasteryData.MasteryTier.Master,
                    DisplayName = "Master",
                    MinPoints = 10000,
                    DamageBonus = 0.20f,
                    CooldownReduction = 0.12f,
                    ManaCostReduction = 0.12f
                }},
                { SkillMasteryData.MasteryTier.GrandMaster, new SkillMasteryData.MasteryTierInfo
                {
                    Tier = SkillMasteryData.MasteryTier.GrandMaster,
                    DisplayName = "Grand Master",
                    MinPoints = 50000,
                    DamageBonus = 0.30f,
                    CooldownReduction = 0.20f,
                    ManaCostReduction = 0.20f
                }}
            };
        }

        private void InitializeBonuses()
        {
            BonusesByType = new Dictionary<SkillMasteryData.SkillType, List<SkillMasteryData.MasteryBonus>>();

            // Attack skill bonuses
            BonusesByType[SkillMasteryData.SkillType.Attack] = new List<SkillMasteryData.MasteryBonus>
            {
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "attack_crit_1",
                    Name = "Critical Strike I",
                    Description = "+5% Critical Hit Chance",
                    RequiredTier = SkillMasteryData.MasteryTier.Apprentice,
                    RequiredType = SkillMasteryData.SkillType.Attack,
                    RequiredPoints = 200,
                    BonusValue = 0.05f,
                    StatBonus = "crit_rate"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "attack_crit_2",
                    Name = "Critical Strike II",
                    Description = "+10% Critical Hit Chance",
                    RequiredTier = SkillMasteryData.MasteryTier.Expert,
                    RequiredType = SkillMasteryData.SkillType.Attack,
                    RequiredPoints = 3000,
                    BonusValue = 0.10f,
                    StatBonus = "crit_rate"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "attack_crit_dmg",
                    Name = "Lethal Precision",
                    Description = "+15% Critical Damage",
                    RequiredTier = SkillMasteryData.MasteryTier.Master,
                    RequiredType = SkillMasteryData.SkillType.Attack,
                    RequiredPoints = 15000,
                    BonusValue = 0.15f,
                    StatBonus = "crit_damage"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "attack_lifesteal",
                    Name = "Vampiric Touch",
                    Description = "+5% Life Steal",
                    RequiredTier = SkillMasteryData.MasteryTier.Journeyman,
                    RequiredType = SkillMasteryData.SkillType.Attack,
                    RequiredPoints = 1000,
                    BonusValue = 0.05f,
                    StatBonus = "lifesteal"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "attack_armor_pen",
                    Name = "Armor Breaking",
                    Description = "+10% Armor Penetration",
                    RequiredTier = SkillMasteryData.MasteryTier.Master,
                    RequiredType = SkillMasteryData.SkillType.Attack,
                    RequiredPoints = 20000,
                    BonusValue = 0.10f,
                    StatBonus = "armor_pen"
                }
            };

            // Defense skill bonuses
            BonusesByType[SkillMasteryData.SkillType.Defense] = new List<SkillMasteryData.MasteryBonus>
            {
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "defense_block_1",
                    Name = "Shield Bearer I",
                    Description = "+5% Block Chance",
                    RequiredTier = SkillMasteryData.MasteryTier.Apprentice,
                    RequiredType = SkillMasteryData.SkillType.Defense,
                    RequiredPoints = 200,
                    BonusValue = 0.05f,
                    StatBonus = "block_rate"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "defense_block_2",
                    Name = "Shield Bearer II",
                    Description = "+10% Block Chance",
                    RequiredTier = SkillMasteryData.MasteryTier.Expert,
                    RequiredType = SkillMasteryData.SkillType.Defense,
                    RequiredPoints = 3000,
                    BonusValue = 0.10f,
                    StatBonus = "block_rate"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "defense_dodge",
                    Name = "Evasion Master",
                    Description = "+8% Dodge Chance",
                    RequiredTier = SkillMasteryData.MasteryTier.Journeyman,
                    RequiredType = SkillMasteryData.SkillType.Defense,
                    RequiredPoints = 1500,
                    BonusValue = 0.08f,
                    StatBonus = "dodge"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "defense_thorns",
                    Name = "Thorn Armor",
                    Description = "Reflect 10% damage",
                    RequiredTier = SkillMasteryData.MasteryTier.Master,
                    RequiredType = SkillMasteryData.SkillType.Defense,
                    RequiredPoints = 15000,
                    BonusValue = 0.10f,
                    StatBonus = "reflect"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "defense_shield",
                    Name = "Iron Will",
                    Description = "+15% Shield Effectiveness",
                    RequiredTier = SkillMasteryData.MasteryTier.GrandMaster,
                    RequiredType = SkillMasteryData.SkillType.Defense,
                    RequiredPoints = 60000,
                    BonusValue = 0.15f,
                    StatBonus = "shield_effective"
                }
            };

            // Support skill bonuses
            BonusesByType[SkillMasteryData.SkillType.Support] = new List<SkillMasteryData.MasteryBonus>
            {
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "support_buff_1",
                    Name = "Inspiring Presence I",
                    Description = "+5% Buff Duration",
                    RequiredTier = SkillMasteryData.MasteryTier.Apprentice,
                    RequiredType = SkillMasteryData.SkillType.Support,
                    RequiredPoints = 200,
                    BonusValue = 0.05f,
                    StatBonus = "buff_duration"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "support_buff_2",
                    Name = "Inspiring Presence II",
                    Description = "+10% Buff Duration",
                    RequiredTier = SkillMasteryData.MasteryTier.Expert,
                    RequiredType = SkillMasteryData.SkillType.Support,
                    RequiredPoints = 3000,
                    BonusValue = 0.10f,
                    StatBonus = "buff_duration"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "support_aura",
                    Name = "Aura Mastery",
                    Description = "+20% Aura Range",
                    RequiredTier = SkillMasteryData.MasteryTier.Master,
                    RequiredType = SkillMasteryData.SkillType.Support,
                    RequiredPoints = 15000,
                    BonusValue = 0.20f,
                    StatBonus = "aura_range"
                }
            };

            // Magic skill bonuses
            BonusesByType[SkillMasteryData.SkillType.Magic] = new List<SkillMasteryData.MasteryBonus>
            {
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "magic_crit_1",
                    Name = "Arcane Precision I",
                    Description = "+5% Magic Critical",
                    RequiredTier = SkillMasteryData.MasteryTier.Apprentice,
                    RequiredType = SkillMasteryData.SkillType.Magic,
                    RequiredPoints = 200,
                    BonusValue = 0.05f,
                    StatBonus = "magic_crit"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "magic_crit_2",
                    Name = "Arcane Precision II",
                    Description = "+10% Magic Critical",
                    RequiredTier = SkillMasteryData.MasteryTier.Expert,
                    RequiredType = SkillMasteryData.SkillType.Magic,
                    RequiredPoints = 3000,
                    BonusValue = 0.10f,
                    StatBonus = "magic_crit"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "magic_pen",
                    Name = "Arcane Penetration",
                    Description = "+15% Magic Penetration",
                    RequiredTier = SkillMasteryData.MasteryTier.Master,
                    RequiredType = SkillMasteryData.SkillType.Magic,
                    RequiredPoints = 15000,
                    BonusValue = 0.15f,
                    StatBonus = "magic_pen"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "magic_cost",
                    Name = "Mana Efficiency",
                    Description = "-10% Mana Cost",
                    RequiredTier = SkillMasteryData.MasteryTier.Journeyman,
                    RequiredType = SkillMasteryData.SkillType.Magic,
                    RequiredPoints = 1000,
                    BonusValue = 0.10f,
                    StatBonus = "mana_cost"
                }
            };

            // Healing skill bonuses
            BonusesByType[SkillMasteryData.SkillType.Healing] = new List<SkillMasteryData.MasteryBonus>
            {
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "heal_power_1",
                    Name = "Healing Light I",
                    Description = "+5% Healing Power",
                    RequiredTier = SkillMasteryData.MasteryTier.Apprentice,
                    RequiredType = SkillMasteryData.SkillType.Healing,
                    RequiredPoints = 200,
                    BonusValue = 0.05f,
                    StatBonus = "heal_power"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "heal_power_2",
                    Name = "Healing Light II",
                    Description = "+10% Healing Power",
                    RequiredTier = SkillMasteryData.MasteryTier.Expert,
                    RequiredType = SkillMasteryData.SkillType.Healing,
                    RequiredPoints = 3000,
                    BonusValue = 0.10f,
                    StatBonus = "heal_power"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "heal_crit",
                    Name = "Divine Touch",
                    Description = "+15% Heal Critical Chance",
                    RequiredTier = SkillMasteryData.MasteryTier.Master,
                    RequiredType = SkillMasteryData.SkillType.Healing,
                    RequiredPoints = 15000,
                    BonusValue = 0.15f,
                    StatBonus = "heal_crit"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "heal_range",
                    Name = "Wide Embrace",
                    Description = "+20% Heal Range",
                    RequiredTier = SkillMasteryData.MasteryTier.Journeyman,
                    RequiredType = SkillMasteryData.SkillType.Healing,
                    RequiredPoints = 1000,
                    BonusValue = 0.20f,
                    StatBonus = "heal_range"
                }
            };

            // Utility skill bonuses
            BonusesByType[SkillMasteryData.SkillType.Utility] = new List<SkillMasteryData.MasteryBonus>
            {
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "utility_cd_1",
                    Name = "Swift Hands I",
                    Description = "+5% Cooldown Reduction",
                    RequiredTier = SkillMasteryData.MasteryTier.Apprentice,
                    RequiredType = SkillMasteryData.SkillType.Utility,
                    RequiredPoints = 200,
                    BonusValue = 0.05f,
                    StatBonus = "cdr"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "utility_cd_2",
                    Name = "Swift Hands II",
                    Description = "+10% Cooldown Reduction",
                    RequiredTier = SkillMasteryData.MasteryTier.Expert,
                    RequiredType = SkillMasteryData.SkillType.Utility,
                    RequiredPoints = 3000,
                    BonusValue = 0.10f,
                    StatBonus = "cdr"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "utility_range",
                    Name = "Extended Reach",
                    Description = "+15% Skill Range",
                    RequiredTier = SkillMasteryData.MasteryTier.Journeyman,
                    RequiredType = SkillMasteryData.SkillType.Utility,
                    RequiredPoints = 1000,
                    BonusValue = 0.15f,
                    StatBonus = "skill_range"
                }
            };
        }

        private void InitializeGlobalBonuses()
        {
            GlobalBonuses = new List<SkillMasteryData.MasteryBonus>
            {
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "global_polymath",
                    Name = "Polymath",
                    Description = "+5% All Stats for each skill type mastered",
                    RequiredTier = SkillMasteryData.MasteryTier.Master,
                    RequiredType = SkillMasteryData.SkillType.Attack, // Any type
                    RequiredPoints = 10000,
                    BonusValue = 0.05f,
                    StatBonus = "all_stats"
                },
                new SkillMasteryData.MasteryBonus
                {
                    BonusId = "global_mastery",
                    Name = "True Master",
                    Description = "+10% Damage when all skills at Expert+",
                    RequiredTier = SkillMasteryData.MasteryTier.GrandMaster,
                    RequiredType = SkillMasteryData.SkillType.Attack,
                    RequiredPoints = 50000,
                    BonusValue = 0.10f,
                    StatBonus = "damage"
                }
            };
        }

        public SkillMasteryData.MasteryTier GetTierForPoints(int points)
        {
            if (points >= 50001) return SkillMasteryData.MasteryTier.GrandMaster;
            if (points >= 10001) return SkillMasteryData.MasteryTier.Master;
            if (points >= 2001) return SkillMasteryData.MasteryTier.Expert;
            if (points >= 501) return SkillMasteryData.MasteryTier.Journeyman;
            if (points >= 101) return SkillMasteryData.MasteryTier.Apprentice;
            return SkillMasteryData.MasteryTier.Novice;
        }

        public SkillMasteryData.MasteryTierInfo GetTierInfo(SkillMasteryData.MasteryTier tier)
        {
            return TierConfigs.ContainsKey(tier) ? TierConfigs[tier] : TierConfigs[SkillMasteryData.MasteryTier.Novice];
        }

        public List<SkillMasteryData.MasteryBonus> GetAvailableBonuses(SkillMasteryData.SkillType type, int currentPoints, SkillMasteryData.MasteryTier currentTier)
        {
            if (!BonusesByType.ContainsKey(type)) return new List<SkillMasteryData.MasteryBonus>();

            var available = new List<SkillMasteryData.MasteryBonus>();
            foreach (var bonus in BonusesByType[type])
            {
                if (currentPoints >= bonus.RequiredPoints && currentTier >= bonus.RequiredTier)
                {
                    available.Add(bonus);
                }
            }
            return available;
        }
    }
}
