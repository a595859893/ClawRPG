using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.FateWeaving {
    public class FateWeavingDatabase : Godot.Object {
        private static FateWeavingDatabase _instance;
        public static FateWeavingDatabase Instance {
            get {
                if (_instance == null) _instance = new FateWeavingDatabase();
                return _instance;
            }
        }

        public List<FatePathData> Paths { get; private set; }
        public List<FateChoice> Choices { get; private set; }

        public FateWeavingDatabase() {
            InitializePaths();
            InitializeChoices();
        }

        private void InitializePaths() {
            Paths = new List<FatePathData> {
                new FatePathData {
                    Type = FatePathType.Hero,
                    Name = "Hero's Path",
                    Description = "Walk the path of justice and protection. Your choices strengthen your resolve to help others.",
                    PathBonuses = new Dictionary<string, float> {
                        { "damage_vs_evil", 0.15f },
                        { "healing_received", 0.1f },
                        { "defense", 0.05f }
                    },
                    ExclusiveChoices = new List<string> { "village_raid_protect", "beggar_mercy" },
                    UnlockTier = 1
                },
                new FatePathData {
                    Type = FatePathType.AntiHero,
                    Name = "Anti-Hero's Path",
                    Description = "A darker road. You do what needs to be done, regardless of moral costs.",
                    PathBonuses = new Dictionary<string, float> {
                        { "critical_chance", 0.1f },
                        { "damage", 0.1f },
                        { "stealth", 0.1f }
                    },
                    ExclusiveChoices = new List<string> { "village_raid_tribute", "beggar_ignore" },
                    UnlockTier = 1
                },
                new FatePathData {
                    Type = FatePathType.Villain,
                    Name = "Villain's Path",
                    Description = "Embrace darkness. Power through fear and domination.",
                    PathBonuses = new Dictionary<string, float> {
                        { "intimidation", 0.2f },
                        { "loot_bonus", 0.15f },
                        { "fear_duration", 0.1f }
                    },
                    ExclusiveChoices = new List<string> { "village_raid_slaughter", "beggar_rob" },
                    UnlockTier = 3
                },
                new FatePathData {
                    Type = FatePathType.Mercenary,
                    Name = "Mercenary's Path",
                    Description = "True neutral. Gold is your only loyalty.",
                    PathBonuses = new Dictionary<string, float> {
                        { "trade_discount", 0.15f },
                        { "quest_reward", 0.1f },
                        { "negotiation", 0.1f }
                    },
                    ExclusiveChoices = new List<string> { "village_raid_negotiate", "beggar_business" },
                    UnlockTier = 2
                },
                new FatePathData {
                    Type = FatePathType.Legend,
                    Name = "Legend's Path",
                    Description = "Your deeds will be sung for generations. A path of great achievement.",
                    PathBonuses = new Dictionary<string, float> {
                        { "reputation", 0.2f },
                        { "drop_rate", 0.1f },
                        { "exp_bonus", 0.1f }
                    },
                    ExclusiveChoices = new List<string> { "legend_trial_sacrifice", "legend_trial_honor" },
                    UnlockTier = 5
                },
                new FatePathData {
                    Type = FatePathType.Myth,
                    Name = "Mythic Path",
                    Description = "Transcend mortal limitations. Become legend itself.",
                    PathBonuses = new Dictionary<string, float> {
                        { "all_stats", 0.05f },
                        { "mythical_drop", 0.1f },
                        { "special_ability", 1f }
                    },
                    ExclusiveChoices = new List<string> { "myth_trial_gods", "myth_trial_knowledge" },
                    UnlockTier = 10
                },
                new FatePathData {
                    Type = FatePathType.Chaos,
                    Name = "Chaos Path",
                    Description = "Embrace the unpredictable. Randomness is your ally.",
                    PathBonuses = new Dictionary<string, float> {
                        { "random_effects", 0.2f },
                        { "critical_damage", 0.15f },
                        { "spell_variation", 0.1f }
                    },
                    ExclusiveChoices = new List<string> { "chaos_trial_unpredictable", "chaos_trial_entropy" },
                    UnlockTier = 4
                },
                new FatePathData {
                    Type = FatePathType.Order,
                    Name = "Order Path",
                    Description = "Structure and discipline guide your journey.",
                    PathBonuses = new Dictionary<string, float> {
                        { "cooldown_reduction", 0.1f },
                        { "ability_consistency", 0.15f },
                        { "defense_bonus", 0.1f }
                    },
                    ExclusiveChoices = new List<string> { "order_trial_discipline", "order_trial_structure" },
                    UnlockTier = 4
                },
                new FatePathData {
                    Type = FatePathType.Shadow,
                    Name = "Shadow Path",
                    Description = "Walk in darkness. Strike from the unseen.",
                    PathBonuses = new Dictionary<string, float> {
                        { "backstab_damage", 0.2f },
                        { "stealth_effectiveness", 0.15f },
                        { "evasion", 0.1f }
                    },
                    ExclusiveChoices = new List<string> { "shadow_trial_silent", "shadow_trial_whisper" },
                    UnlockTier = 3
                },
                new FatePathData {
                    Type = FatePathType.Light,
                    Name = "Light Path",
                    Description = "Radiance guides your way. Purge darkness wherever you go.",
                    PathBonuses = new Dictionary<string, float> {
                        { "holy_damage", 0.15f },
                        { "undead_damage", 0.2f },
                        { "blessing_effectiveness", 0.15f }
                    },
                    ExclusiveChoices = new List<string> { "light_trial_purify", "light_trial_bless" },
                    UnlockTier = 3
                }
            };
        }

        private void InitializeChoices() {
            Choices = new List<FateChoice> {
                // Moral Choices
                new FateChoice {
                    Id = "village_raid_protect",
                    Title = "Defend the Village",
                    Description = "Bandits attack a small village. You can defend it.",
                    ChoiceType = FateChoiceType.Moral,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Hero, 15f },
                        { FatePathType.Legend, 5f },
                        { FatePathType.Light, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "strength", 2f },
                        { "charisma", 1f }
                    },
                    ConsequenceDescription = "The villagers praise your heroism. Word of your deeds spreads.",
                    IsSecret = false,
                    TierRequired = 1
                },
                new FateChoice {
                    Id = "village_raid_tribute",
                    Title = "Accept Tribute",
                    Description = "The village offers you gold to leave them to the bandits.",
                    ChoiceType = FateChoiceType.Moral,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.AntiHero, 10f },
                        { FatePathType.Mercenary, 15f },
                        { FatePathType.Villain, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "charisma", 2f },
                        { "luck", 1f }
                    },
                    ConsequenceDescription = "You take the gold and walk away. The bandits pillage the village.",
                    IsSecret = false,
                    TierRequired = 1
                },
                new FateChoice {
                    Id = "village_raid_slaughter",
                    Title = "Join the Slaughter",
                    Description = "You can join the bandits in their raid for great plunder.",
                    ChoiceType = FateChoiceType.Moral,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Villain, 20f },
                        { FatePathType.Chaos, 5f },
                        { FatePathType.Shadow, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "strength", 3f },
                        { "dexterity", 1f }
                    },
                    ConsequenceDescription = "The village burns. You gain wealth but earn a dark reputation.",
                    IsSecret = false,
                    TierRequired = 3
                },
                new FateChoice {
                    Id = "village_raid_negotiate",
                    Title = "Negotiate Peace",
                    Description = "You attempt to broker a deal between bandits and villagers.",
                    ChoiceType = FateChoiceType.Moral,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Mercenary, 15f },
                        { FatePathType.Order, 5f },
                        { FatePathType.Legend, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "wisdom", 2f },
                        { "charisma", 2f }
                    },
                    ConsequenceDescription = "A tense peace is negotiated. Both sides respect your cunning.",
                    IsSecret = false,
                    TierRequired = 2
                },
                new FateChoice {
                    Id = "beggar_mercy",
                    Title = "Give Generously",
                    Description = "A beggar asks for coin. You give freely.",
                    ChoiceType = FateChoiceType.Moral,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Hero, 10f },
                        { FatePathType.Light, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "charisma", 1f },
                        { "luck", 2f }
                    },
                    ConsequenceDescription = "The beggar reveals themselves as a disguised sage and grants you a blessing.",
                    IsSecret = true,
                    TierRequired = 1
                },
                new FateChoice {
                    Id = "beggar_ignore",
                    Title = "Walk Past",
                    Description = "You ignore the beggar's plea.",
                    ChoiceType = FateChoiceType.Moral,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.AntiHero, 5f },
                        { FatePathType.Mercenary, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "wisdom", 1f }
                    },
                    ConsequenceDescription = "You continue on your path, focused on your goals.",
                    IsSecret = false,
                    TierRequired = 1
                },
                new FateChoice {
                    Id = "beggar_rob",
                    Title = "Rob the Beggar",
                    Description = "The beggar looks wealthy despite their rags. You take what they have.",
                    ChoiceType = FateChoiceType.Moral,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Villain, 15f },
                        { FatePathType.Shadow, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "dexterity", 2f },
                        { "luck", 1f }
                    },
                    ConsequenceDescription = "You find a small fortune, but feel the weight of your actions.",
                    IsSecret = false,
                    TierRequired = 2
                },
                new FateChoice {
                    Id = "beggar_business",
                    Title = "Offer Employment",
                    Description = "You offer the beggar work in exchange for pay.",
                    ChoiceType = FateChoiceType.Moral,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Mercenary, 10f },
                        { FatePathType.Order, 5f },
                        { FatePathType.Legend, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "wisdom", 2f },
                        { "charisma", 1f }
                    },
                    ConsequenceDescription = "The beggar becomes a loyal contact in the underworld.",
                    IsSecret = false,
                    TierRequired = 2
                },
                
                // Combat Choices
                new FateChoice {
                    Id = "duel_honor",
                    Title = "Honorable Duel",
                    Description = "A warrior challenges you. You accept with honor.",
                    ChoiceType = FateChoiceType.Combat,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Hero, 10f },
                        { FatePathType.Legend, 10f },
                        { FatePathType.Order, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "strength", 2f },
                        { "dexterity", 2f }
                    },
                    ConsequenceDescription = "A glorious battle. Whether you win or lose, you gain respect.",
                    IsSecret = false,
                    TierRequired = 2
                },
                new FateChoice {
                    Id = "duel_dirty",
                    Title = "Use Any Means",
                    Description = "Victory at any cost. You fight dirty.",
                    ChoiceType = FateChoiceType.Combat,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Villain, 10f },
                        { FatePathType.Chaos, 5f },
                        { FatePathType.Shadow, 10f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "dexterity", 3f },
                        { "luck", 1f }
                    },
                    ConsequenceDescription = "You win, but observers frown upon your tactics.",
                    IsSecret = false,
                    TierRequired = 2
                },
                new FateChoice {
                    Id = "boss_spares",
                    Title = "Spare the Boss",
                    Description = "You defeat a powerful enemy but choose to spare their life.",
                    ChoiceType = FateChoiceType.Combat,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Hero, 15f },
                        { FatePathType.Light, 10f },
                        { FatePathType.Legend, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "wisdom", 3f },
                        { "charisma", 2f }
                    },
                    ConsequenceDescription = "The enemy becomes an ally. A powerful friend in need.",
                    IsSecret = false,
                    TierRequired = 4
                },
                new FateChoice {
                    Id = "boss_mercy_no",
                    Title = "Show No Mercy",
                    Description = "You eliminate the defeated enemy completely.",
                    ChoiceType = FateChoiceType.Combat,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Villain, 10f },
                        { FatePathType.Shadow, 5f },
                        { FatePathType.Chaos, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "strength", 3f },
                        { "intelligence", 1f }
                    },
                    ConsequenceDescription = "Fear follows your name. No one expects mercy from you.",
                    IsSecret = false,
                    TierRequired = 3
                },
                
                // Social Choices
                new FateChoice {
                    Id = "diplomacy_peace",
                    Title = "Seek Peace",
                    Description = "Two factions are at war. You work to broker peace.",
                    ChoiceType = FateChoiceType.Social,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Hero, 10f },
                        { FatePathType.Legend, 10f },
                        { FatePathType.Order, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "charisma", 3f },
                        { "wisdom", 2f }
                    },
                    ConsequenceDescription = "Your diplomatic success saves countless lives.",
                    IsSecret = false,
                    TierRequired = 3
                },
                new FateChoice {
                    Id = "diplomacy_war",
                    Title = "Fuel the Conflict",
                    Description = "You supply weapons to both sides, profiting from war.",
                    ChoiceType = FateChoiceType.Social,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Villain, 15f },
                        { FatePathType.Mercenary, 15f },
                        { FatePathType.Chaos, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "luck", 3f },
                        { "charisma", 2f }
                    },
                    ConsequenceDescription = "Blood money fills your coffers. Many suffer for your gain.",
                    IsSecret = false,
                    TierRequired = 4
                },
                new FateChoice {
                    Id = "social_lead",
                    Title = "Lead by Example",
                    Description = "You inspire others through your actions.",
                    ChoiceType = FateChoiceType.Social,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Legend, 15f },
                        { FatePathType.Hero, 5f },
                        { FatePathType.Light, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "charisma", 3f },
                        { "wisdom", 2f }
                    },
                    ConsequenceDescription = "Followers gather. You become a natural leader.",
                    IsSecret = false,
                    TierRequired = 3
                },
                
                // Economic Choices
                new FateChoice {
                    Id = "trade_honest",
                    Title = "Honest Deal",
                    Description = "A merchant offers a rigged deal. You refuse.",
                    ChoiceType = FateChoiceType.Economic,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Hero, 10f },
                        { FatePathType.Order, 10f },
                        { FatePathType.Legend, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "wisdom", 2f },
                        { "charisma", 1f }
                    },
                    ConsequenceDescription = "Your reputation as an honest trader spreads far and wide.",
                    IsSecret = false,
                    TierRequired = 1
                },
                new FateChoice {
                    Id = "trade_swindle",
                    Title = "Swindle the Mark",
                    Description = "You take advantage of the naive buyer.",
                    ChoiceType = FateChoiceType.Economic,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Villain, 10f },
                        { FatePathType.Shadow, 10f },
                        { FatePathType.Mercenary, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "intelligence", 2f },
                        { "luck", 2f }
                    },
                    ConsequenceDescription = "Quick gold, but your name becomes synonymous with deceit.",
                    IsSecret = false,
                    TierRequired = 2
                },
                new FateChoice {
                    Id = "trade_invest",
                    Title = "Invest in Future",
                    Description = "You invest your gold in a risky venture.",
                    ChoiceType = FateChoiceType.Economic,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Mercenary, 15f },
                        { FatePathType.Chaos, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "wisdom", 2f },
                        { "luck", 3f }
                    },
                    ConsequenceDescription = "Fortune favors the bold. The investment pays off handsomely.",
                    IsSecret = true,
                    TierRequired = 2
                },
                
                // Exploration Choices
                new FateChoice {
                    Id = "explore_danger",
                    Title = "Braved the Danger",
                    Description = "You explore a dangerous ancient ruin.",
                    ChoiceType = FateChoiceType.Exploration,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Legend, 10f },
                        { FatePathType.Myth, 5f },
                        { FatePathType.Chaos, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "dexterity", 2f },
                        { "wisdom", 2f }
                    },
                    ConsequenceDescription = "Ancient secrets revealed. Powerful artifacts claimed.",
                    IsSecret = false,
                    TierRequired = 3
                },
                new FateChoice {
                    Id = "explore_careful",
                    Title = "Plan Thoroughly",
                    Description = "You carefully map and plan before exploring.",
                    ChoiceType = FateChoiceType.Exploration,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Order, 15f },
                        { FatePathType.Mercenary, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "wisdom", 3f },
                        { "intelligence", 1f }
                    },
                    ConsequenceDescription = "Nothing missed. Every treasure secured with minimal risk.",
                    IsSecret = false,
                    TierRequired = 2
                },
                new FateChoice {
                    Id = "explore_sacrifice",
                    Title = "Sacrifice for Knowledge",
                    Description = "You give up something precious to unlock ancient knowledge.",
                    ChoiceType = FateChoiceType.Exploration,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Myth, 20f },
                        { FatePathType.Legend, 5f },
                        { FatePathType.Light, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "wisdom", 4f },
                        { "intelligence", 3f }
                    },
                    ConsequenceDescription = "The ancient ones reveal truths beyond mortal comprehension.",
                    IsSecret = true,
                    TierRequired = 5
                },
                
                // Mystery Choices
                new FateChoice {
                    Id = "mystery_truth",
                    Title = "Uncover the Truth",
                    Description = "A conspiracy lurks. You investigate despite the danger.",
                    ChoiceType = FateChoiceType.Mystery,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Hero, 10f },
                        { FatePathType.Shadow, 10f },
                        { FatePathType.Light, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "intelligence", 3f },
                        { "wisdom", 2f }
                    },
                    ConsequenceDescription = "The truth is revealed. Evil foiled, but powerful enemies made.",
                    IsSecret = false,
                    TierRequired = 3
                },
                new FateChoice {
                    Id = "mystery_secrets",
                    Title = "Keep the Secrets",
                    Description = "You discover dark secrets and choose to hide them.",
                    ChoiceType = FateChoiceType.Mystery,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Shadow, 15f },
                        { FatePathType.Villain, 5f },
                        { FatePathType.Chaos, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "wisdom", 2f },
                        { "charisma", 2f }
                    },
                    ConsequenceDescription = "You hold dangerous knowledge. Power, but at what cost?",
                    IsSecret = true,
                    TierRequired = 4
                },
                new FateChoice {
                    Id = "mystery_sacrifice_innocent",
                    Title = "Sacrifice the Innocent",
                    Description = "To stop a greater evil, you must sacrifice one who is innocent.",
                    ChoiceType = FateChoiceType.Moral,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.AntiHero, 20f },
                        { FatePathType.Villain, 10f },
                        { FatePathType.Chaos, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "strength", 3f },
                        { "intelligence", 3f }
                    },
                    ConsequenceDescription = "The greater evil is stopped, but your soul grows darker.",
                    IsSecret = false,
                    TierRequired = 8
                },
                
                // Legendary Choices
                new FateChoice {
                    Id = "legend_trial_sacrifice",
                    Title = "Trial by Sacrifice",
                    Description = "A legendary trial requires giving up something dear.",
                    ChoiceType = FateChoiceType.Mystery,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Legend, 25f },
                        { FatePathType.Hero, 10f },
                        { FatePathType.Light, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "all_stats", 5f }
                    },
                    ConsequenceDescription = "Sacrifice begets power. You become legend.",
                    IsSecret = false,
                    TierRequired = 10
                },
                new FateChoice {
                    Id = "legend_trial_honor",
                    Title = "Trial by Honor",
                    Description = "Face a legendary challenge with honor as your only shield.",
                    ChoiceType = FateChoiceType.Combat,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Legend, 25f },
                        { FatePathType.Hero, 15f },
                        { FatePathType.Order, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "all_stats", 4f },
                        { "charisma", 3f }
                    },
                    ConsequenceDescription = "Your honor shines brighter than any blade. Legend awaits.",
                    IsSecret = false,
                    TierRequired = 10
                },
                
                // Mythic Choices
                new FateChoice {
                    Id = "myth_trial_gods",
                    Title = "Challenge the Gods",
                    Description = "Defy the divine and claim godhood.",
                    ChoiceType = FateChoiceType.Combat,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Myth, 30f },
                        { FatePathType.Chaos, 15f },
                        { FatePathType.Villain, 10f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "all_stats", 10f }
                    },
                    ConsequenceDescription = "You ascend. Mortality behind you, eternity ahead.",
                    IsSecret = true,
                    TierRequired = 15
                },
                new FateChoice {
                    Id = "myth_trial_knowledge",
                    Title = "Seek Cosmic Truth",
                    Description = "Abandon all to understand the universe.",
                    ChoiceType = FateChoiceType.Mystery,
                    PathInfluence = new Dictionary<FatePathType, float> {
                        { FatePathType.Myth, 30f },
                        { FatePathType.Order, 10f },
                        { FatePathType.Light, 5f }
                    },
                    StatBonuses = new Dictionary<string, float> {
                        { "wisdom", 15f },
                        { "intelligence", 15f }
                    },
                    ConsequenceDescription = "The cosmos reveals its secrets. You become one with all.",
                    IsSecret = true,
                    TierRequired = 15
                }
            };
        }

        public FateChoice GetRandomChoice(int playerTier) {
            var availableChoices = new List<FateChoice>();
            foreach (var choice in Choices) {
                if (choice.TierRequired <= playerTier) {
                    availableChoices.Add(choice);
                }
            }
            
            if (availableChoices.Count == 0) return null;
            
            var random = new Random();
            return availableChoices[random.Next(availableChoices.Count)];
        }

        public List<FateChoice> GetAvailableChoices(int playerTier) {
            var result = new List<FateChoice>();
            foreach (var choice in Choices) {
                if (choice.TierRequired <= playerTier) {
                    result.Add(choice);
                }
            }
            return result;
        }

        public FatePathData GetPath(FatePathType type) {
            foreach (var path in Paths) {
                if (path.Type == type) return path;
            }
            return null;
        }
    }
}
