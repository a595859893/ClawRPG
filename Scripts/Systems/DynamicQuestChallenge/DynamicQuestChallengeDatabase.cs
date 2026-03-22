using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.DynamicQuestChallenge
{
    /// <summary>
    /// DynamicQuestChallengeDatabase - 动态任务挑战数据库
    /// 提供挑战模板和生成逻辑
    /// </summary>
    public partial class DynamicQuestChallengeDatabase : BaseSystem
    {
        /// <summary>
        /// Challenge templates organized by type
        /// </summary>
        private Dictionary<string, List<Dictionary>> _challengeTemplates;

        public override void _Ready()
        {
            base._Ready();
            InitializeTemplates();
            GD.Print($"[DynamicQuestChallengeDatabase] Initialized with {_challengeTemplates.Count} challenge types");
        }

        /// <summary>
        /// Initialize challenge templates
        /// </summary>
        private void InitializeTemplates()
        {
            _challengeTemplates = new Dictionary<string, List<Dictionary>>
            {
                {
                    "Combat", new List<Dictionary>
                    {
                        new Dictionary
                        {
                            { "template_id", "combat_kill_enemies" },
                            { "name", "Enemy Slayer" },
                            { "description", "Defeat %d enemies" },
                            { "type", "Combat" },
                            { "category", "Battle" },
                            { "target_type", "kill_count" },
                            { "base_target", 20 },
                            { "duration", 300 },
                            { "difficulty_scales", new Dictionary<string, float>
                                {
                                    { "Easy", 1.0f },
                                    { "Medium", 1.5f },
                                    { "Hard", 2.0f },
                                    { "Epic", 3.0f },
                                    { "Legendary", 5.0f }
                                }
                            },
                            { "rewards", new Dictionary<string, int>
                                {
                                    { "gold", 100 },
                                    { "experience", 50 }
                                }
                            }
                        },
                        new Dictionary
                        {
                            { "template_id", "combat_boss_defeat" },
                            { "name", "Boss Hunter" },
                            { "description", "Defeat %d boss enemies" },
                            { "type", "Combat" },
                            { "category", "Boss" },
                            { "target_type", "boss_kill" },
                            { "base_target", 3 },
                            { "duration", 600 },
                            { "difficulty_scales", new Dictionary<string, float>
                                {
                                    { "Easy", 1.0f },
                                    { "Medium", 1.5f },
                                    { "Hard", 2.0f },
                                    { "Epic", 3.0f },
                                    { "Legendary", 5.0f }
                                }
                            },
                            { "rewards", new Dictionary<string, int>
                                {
                                    { "gold", 500 },
                                    { "experience", 200 }
                                }
                            }
                        },
                        new Dictionary
                        {
                            { "template_id", "combat_damage_dealt" },
                            { "name", "Damage Dealer" },
                            { "description", "Deal %d total damage" },
                            { "type", "Combat" },
                            { "category", "Damage" },
                            { "target_type", "damage_dealt" },
                            { "base_target", 1000 },
                            { "duration", 300 },
                            { "difficulty_scales", new Dictionary<string, float>
                                {
                                    { "Easy", 1.0f },
                                    { "Medium", 1.5f },
                                    { "Hard", 2.0f },
                                    { "Epic", 3.0f },
                                    { "Legendary", 5.0f }
                                }
                            },
                            { "rewards", new Dictionary<string, int>
                                {
                                    { "gold", 150 },
                                    { "experience", 75 }
                                }
                            }
                        }
                    }
                },
                {
                    "Collection", new List<Dictionary>
                    {
                        new Dictionary
                        {
                            { "template_id", "collect_items" },
                            { "name", "Collector" },
                            { "description", "Collect %d items" },
                            { "type", "Collection" },
                            { "category", "Gathering" },
                            { "target_type", "item_collect" },
                            { "base_target", 15 },
                            { "duration", 400 },
                            { "difficulty_scales", new Dictionary<string, float>
                                {
                                    { "Easy", 1.0f },
                                    { "Medium", 1.5f },
                                    { "Hard", 2.0f },
                                    { "Epic", 3.0f },
                                    { "Legendary", 5.0f }
                                }
                            },
                            { "rewards", new Dictionary<string, int>
                                {
                                    { "gold", 80 },
                                    { "experience", 40 }
                                }
                            }
                        },
                        new Dictionary
                        {
                            { "template_id", "collect_gold" },
                            { "name", "Treasure Hunter" },
                            { "description", "Collect %d gold from loot" },
                            { "type", "Collection" },
                            { "category", "Wealth" },
                            { "target_type", "gold_collect" },
                            { "base_target", 500 },
                            { "duration", 300 },
                            { "difficulty_scales", new Dictionary<string, float>
                                {
                                    { "Easy", 1.0f },
                                    { "Medium", 1.5f },
                                    { "Hard", 2.0f },
                                    { "Epic", 3.0f },
                                    { "Legendary", 5.0f }
                                }
                            },
                            { "rewards", new Dictionary<string, int>
                                {
                                    { "gold", 50 },
                                    { "experience", 25 }
                                }
                            }
                        }
                    }
                },
                {
                    "Exploration", new List<Dictionary>
                    {
                        new Dictionary
                        {
                            { "template_id", "explore_dungeons" },
                            { "name", "Dungeon Explorer" },
                            { "description", "Complete %d dungeon floors" },
                            { "type", "Exploration" },
                            { "category", "Dungeon" },
                            { "target_type", "dungeon_floor" },
                            { "base_target", 5 },
                            { "duration", 600 },
                            { "difficulty_scales", new Dictionary<string, float>
                                {
                                    { "Easy", 1.0f },
                                    { "Medium", 1.5f },
                                    { "Hard", 2.0f },
                                    { "Epic", 3.0f },
                                    { "Legendary", 5.0f }
                                }
                            },
                            { "rewards", new Dictionary<string, int>
                                {
                                    { "gold", 200 },
                                    { "experience", 100 }
                                }
                            }
                        },
                        new Dictionary
                        {
                            { "template_id", "explore_areas" },
                            { "name", "World Traveler" },
                            { "description", "Visit %d new areas" },
                            { "type", "Exploration" },
                            { "category", "World" },
                            { "target_type", "area_visit" },
                            { "base_target", 10 },
                            { "duration", 500 },
                            { "difficulty_scales", new Dictionary<string, float>
                                {
                                    { "Easy", 1.0f },
                                    { "Medium", 1.5f },
                                    { "Hard", 2.0f },
                                    { "Epic", 3.0f },
                                    { "Legendary", 5.0f }
                                }
                            },
                            { "rewards", new Dictionary<string, int>
                                {
                                    { "gold", 120 },
                                    { "experience", 60 }
                                }
                            }
                        }
                    }
                },
                {
                    "Social", new List<Dictionary>
                    {
                        new Dictionary
                        {
                            { "template_id", "social_friends" },
                            { "name", "Social Butterfly" },
                            { "description", "Add %d new friends" },
                            { "type", "Social" },
                            { "category", "Friends" },
                            { "target_type", "friend_add" },
                            { "base_target", 3 },
                            { "duration", 600 },
                            { "difficulty_scales", new Dictionary<string, float>
                                {
                                    { "Easy", 1.0f },
                                    { "Medium", 1.5f },
                                    { "Hard", 2.0f },
                                    { "Epic", 3.0f },
                                    { "Legendary", 5.0f }
                                }
                            },
                            { "rewards", new Dictionary<string, int>
                                {
                                    { "gold", 100 },
                                    { "experience", 50 }
                                }
                            }
                        },
                        new Dictionary
                        {
                            { "template_id", "social_guild" },
                            { "name", "Guild Member" },
                            { "description", "Complete %d guild quests" },
                            { "type", "Social" },
                            { "category", "Guild" },
                            { "target_type", "guild_quest" },
                            { "base_target", 5 },
                            { "duration", 600 },
                            { "difficulty_scales", new Dictionary<string, float>
                                {
                                    { "Easy", 1.0f },
                                    { "Medium", 1.5f },
                                    { "Hard", 2.0f },
                                    { "Epic", 3.0f },
                                    { "Legendary", 5.0f }
                                }
                            },
                            { "rewards", new Dictionary<string, int>
                                {
                                    { "gold", 150 },
                                    { "experience", 75 }
                                }
                            }
                        }
                    }
                },
                {
                    "Economy", new List<Dictionary>
                    {
                        new Dictionary
                        {
                            { "template_id", "economy_trade" },
                            { "name", "Merchant" },
                            { "description", "Complete %d trades in auction house" },
                            { "type", "Economy" },
                            { "category", "Trading" },
                            { "target_type", "trade_complete" },
                            { "base_target", 10 },
                            { "duration", 600 },
                            { "difficulty_scales", new Dictionary<string, float>
                                {
                                    { "Easy", 1.0f },
                                    { "Medium", 1.5f },
                                    { "Hard", 2.0f },
                                    { "Epic", 3.0f },
                                    { "Legendary", 5.0f }
                                }
                            },
                            { "rewards", new Dictionary<string, int>
                                {
                                    { "gold", 200 },
                                    { "experience", 50 }
                                }
                            }
                        },
                        new Dictionary
                        {
                            { "template_id", "economy_earn" },
                            { "name", "Wealth Builder" },
                            { "description", "Earn %d total gold" },
                            { "type", "Economy" },
                            { "category", "Wealth" },
                            { "target_type", "gold_earn" },
                            { "base_target", 1000 },
                            { "duration", 500 },
                            { "difficulty_scales", new Dictionary<string, float>
                                {
                                    { "Easy", 1.0f },
                                    { "Medium", 1.5f },
                                    { "Hard", 2.0f },
                                    { "Epic", 3.0f },
                                    { "Legendary", 5.0f }
                                }
                            },
                            { "rewards", new Dictionary<string, int>
                                {
                                    { "gold", 100 },
                                    { "experience", 30 }
                                }
                            }
                        }
                    }
                },
                {
                    "Pet", new List<Dictionary>
                    {
                        new Dictionary
                        {
                            { "template_id", "pet_battle" },
                            { "name", "Pet Battler" },
                            { "description", "Win %d pet battles" },
                            { "type", "Pet" },
                            { "category", "Battle" },
                            { "target_type", "pet_battle_win" },
                            { "base_target", 10 },
                            { "duration", 400 },
                            { "difficulty_scales", new Dictionary<string, float>
                                {
                                    { "Easy", 1.0f },
                                    { "Medium", 1.5f },
                                    { "Hard", 2.0f },
                                    { "Epic", 3.0f },
                                    { "Legendary", 5.0f }
                                }
                            },
                            { "rewards", new Dictionary<string, int>
                                {
                                    { "gold", 100 },
                                    { "experience", 50 }
                                }
                            }
                        },
                        new Dictionary
                        {
                            { "template_id", "pet_interact" },
                            { "name", "Pet Lover" },
                            { "description", "Interact with your pet %d times" },
                            { "type", "Pet" },
                            { "category", "Interaction" },
                            { "target_type", "pet_interact" },
                            { "base_target", 20 },
                            { "duration", 400 },
                            { "difficulty_scales", new Dictionary<string, float>
                                {
                                    { "Easy", 1.0f },
                                    { "Medium", 1.5f },
                                    { "Hard", 2.0f },
                                    { "Epic", 3.0f },
                                    { "Legendary", 5.0f }
                                }
                            },
                            { "rewards", new Dictionary<string, int>
                                {
                                    { "gold", 50 },
                                    { "experience", 30 }
                                }
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Get all challenge types
        /// </summary>
        public List<string> GetChallengeTypes()
        {
            return new List<string>(_challengeTemplates.Keys);
        }

        /// <summary>
        /// Get challenges by type
        /// </summary>
        public List<Dictionary> GetChallengesByType(string challengeType)
        {
            if (_challengeTemplates.ContainsKey(challengeType))
            {
                return _challengeTemplates[challengeType];
            }
            return new List<Dictionary>();
        }

        /// <summary>
        /// Generate a random challenge of the specified type
        /// </summary>
        public Dictionary GenerateChallenge(string challengeType, string difficulty, int playerLevel, string playerClass)
        {
            var challenges = GetChallengesByType(challengeType);
            if (challenges.Count == 0)
            {
                return new Dictionary();
            }

            var random = new Random();
            var template = challenges[random.Next(challenges.Count)];
            var scale = GetDifficultyScale(template, difficulty);
            var target = (int)((int)template["base_target"] * scale);

            // Scale by player level
            target = (int)(target * (1.0 + playerLevel * 0.05));

            // Calculate rewards
            var rewards = new Dictionary<string, int>
            {
                { "gold", (int)((int)((Dictionary)template["rewards"])["gold"] * scale * (1.0 + playerLevel * 0.02)) },
                { "experience", (int)((int)((Dictionary)template["rewards"])["experience"] * scale * (1.0 + playerLevel * 0.03)) }
            };

            // Calculate duration based on difficulty
            int durationBase = (int)template["duration"];
            float durationMultiplier = difficulty == "Easy" ? 1.0f : (difficulty == "Legendary" ? 0.8f : 1.0f);
            int duration = (int)(durationBase * durationMultiplier);

            return new Dictionary
            {
                { "template_id", template["template_id"] },
                { "name", template["name"] },
                { "description", string.Format((string)template["description"], target) },
                { "type", template["type"] },
                { "category", template["category"] },
                { "target_type", template["target_type"] },
                { "target_amount", target },
                { "difficulty", difficulty },
                { "duration", duration },
                { "rewards", rewards },
                { "player_level", playerLevel },
                { "player_class", playerClass }
            };
        }

        /// <summary>
        /// Get difficulty scale from template
        /// </summary>
        private float GetDifficultyScale(Dictionary template, string difficulty)
        {
            var scales = (Dictionary)template["difficulty_scales"];
            if (scales.Contains(difficulty))
            {
                return (float)scales[difficulty];
            }
            return 1.0f;
        }

        /// <summary>
        /// Database class typically doesn't need save data
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            return new Dictionary();
        }

        /// <summary>
        /// Database class typically doesn't need load data
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            // No data to load for database class
        }
    }
}
