using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Event Chain Database - 事件连锁配置数据库
    /// </summary>
    public class EventChainDatabase : BaseSystem {
        public static EventChainDatabase Instance { get; private set; }

        private Dictionary<string, EventChainData> chains = new Dictionary<string, EventChainData>();

        public override void _Ready() {
            Instance = this;
            InitializeChains();
        }

        private void InitializeChains() {
            // 冒险系列事件链
            AddChain(new EventChainData {
                chainId = "adventure_lost_treasure",
                chainName = "失落的宝藏",
                description = "探索古老遗迹，发现隐藏的宝藏",
                minChainLength = 3,
                maxChainLength = 4,
                triggerProbability = 0.25f,
                requiredEvents = new List<String> { "ancient_ruins_discovered" },
                followUpEvents = new List<String> { "treasure_guardian", "hidden_passage", "ancient_cursed" },
                reward = new EventChainReward {
                    goldBonus = 500,
                    expBonus = 200,
                    dropRateBonus = 1.5f,
                    bonusItems = new List<String> { "rare_gem", "ancient_artifact" }
                },
                category = EventChainCategory.Adventure
            });

            // 战斗系列事件链
            AddChain(new EventChainData {
                chainId = "combat_mercenary",
                chainName = "雇佣兵联盟",
                description = "遇到一支雇佣兵队伍，建立友谊",
                minChainLength = 2,
                maxChainLength = 3,
                triggerProbability = 0.35f,
                requiredEvents = new List<String> { "mercenary_encounter" },
                followUpEvents = new List<String> { "mercenary_battle", "mercenary_ally" },
                reward = new EventChainReward {
                    goldBonus = 300,
                    expBonus = 150,
                    dropRateBonus = 1.2f,
                    bonusItems = new List<String> { "mercenary_token" }
                },
                category = EventChainCategory.Combat
            });

            // 神秘系列事件链
            AddChain(new EventChainData {
                chainId = "mystery_prophecy",
                chainName = "古老预言",
                description = "揭开预示未来的神秘预言",
                minChainLength = 4,
                maxChainLength = 5,
                triggerProbability = 0.15f,
                requiredEvents = new List<String> { "strange_omen" },
                followUpEvents = new List<String> { "prophet_encounter", "prophecy_revealed", "fate_choice" },
                reward = new EventChainReward {
                    goldBonus = 1000,
                    expBonus = 500,
                    dropRateBonus = 2.0f,
                    bonusItems = new List<String> { "prophetic_orb", "fate_amulet" }
                },
                category = EventChainCategory.Mystery
            });

            // 传奇系列事件链
            AddChain(new EventChainData {
                chainId = "legend_dragon",
                chainName = "巨龙传说",
                description = "邂逅传说中的巨龙",
                minChainLength = 3,
                maxChainLength = 5,
                triggerProbability = 0.1f,
                requiredEvents = new List<String> { "dragon_sighting" },
                followUpEvents = new List<String> { "dragon_dialogue", "dragon_trial", "dragon_blessing" },
                reward = new EventChainReward {
                    goldBonus = 2000,
                    expBonus = 1000,
                    dropRateBonus = 3.0f,
                    bonusItems = new List<String> { "dragon_scale", "dragon_heart" }
                },
                category = EventChainCategory.Legend
            });

            // 浪漫系列事件链
            AddChain(new EventChainData {
                chainId = "romance_festival",
                chainName = "节日邂逅",
                description = "在节日庆典中遇到特别的人",
                minChainLength = 2,
                maxChainLength = 3,
                triggerProbability = 0.3f,
                requiredEvents = new List<String> { "festival_visit" },
                followUpEvents = new List<String> { "festival_date", "gift_exchange" },
                reward = new EventChainReward {
                    goldBonus = 200,
                    expBonus = 100,
                    dropRateBonus = 1.1f,
                    bonusItems = new List<String> { "love_letter" }
                },
                category = EventChainCategory.Romance
            });

            // 悲剧系列事件链
            AddChain(new EventChainData {
                chainId = "tragedy_village",
                chainName = "村庄的灾难",
                description = "帮助一个遭受灾难的村庄",
                minChainLength = 3,
                maxChainLength = 4,
                triggerProbability = 0.2f,
                requiredEvents = new List<String> { "village_attacked" },
                followUpEvents = new List<String> { "rescue_mission", "village_rebuild" },
                reward = new EventChainReward {
                    goldBonus = 400,
                    expBonus = 250,
                    dropRateBonus = 1.3f,
                    bonusItems = new List<String> { "gratitude_pendant" }
                },
                category = EventChainCategory.Tragedy
            });

            // 喜剧系列事件链
            AddChain(new EventChainData {
                chainId = "comedy_mistaken",
                chainName = "误会连连",
                description = "一系列有趣的误会",
                minChainLength = 2,
                maxChainLength = 3,
                triggerProbability = 0.35f,
                requiredEvents = new List<String> { "strange_encounter" },
                followUpEvents = new List<String> { "misunderstanding", "funny_revelation" },
                reward = new EventChainReward {
                    goldBonus = 150,
                    expBonus = 80,
                    dropRateBonus = 1.0f,
                    bonusItems = new List<String> { "joke_book" }
                },
                category = EventChainCategory.Comedy
            });

            // 战斗进阶事件链
            AddChain(new EventChainData {
                chainId = "combat_elite",
                chainName = "精英试炼",
                description = "通过精英敌人的试炼",
                minChainLength = 2,
                maxChainLength = 3,
                triggerProbability = 0.3f,
                requiredEvents = new List<String> { "elite_enemy_spotted" },
                followUpEvents = new List<String> { "elite_battle", "elite_victory" },
                reward = new EventChainReward {
                    goldBonus = 600,
                    expBonus = 300,
                    dropRateBonus = 1.8f,
                    bonusItems = new List<String> { "elite_badge" }
                },
                category = EventChainCategory.Combat
            });
        }

        private void AddChain(EventChainData chain) {
            chains[chain.chainId] = chain;
        }

        public EventChainData GetChain(string chainId) {
            return chains.ContainsKey(chainId) ? chains[chainId] : null;
        }

        public List<EventChainData> GetChainsByCategory(EventChainCategory category) {
            List<EventChainData> result = new List<EventChainData>();
            foreach (var chain in chains.Values) {
                if (chain.category == category) {
                    result.Add(chain);
                }
            }
            return result;
        }

        public List<EventChainData> GetAllChains() {
            return new List<EventChainData>(chains.Values);
        }

        public EventChainData GetRandomChain(float luckModifier = 1.0f) {
            List<EventChainData> available = new List<EventChainData>();
            foreach (var chain in chains.Values) {
                float adjustedProb = chain.triggerProbability * luckModifier;
                if (GD.Randf() < adjustedProb) {
                    available.Add(chain);
                }
            }
            if (available.Count == 0) return null;
            return available[GD.Randi() % available.Count];
        }

        /// <summary>
        /// EventChainDatabase is a pure configuration database that holds event chain templates.
        /// It contains no player-specific progress data, so no persistence is needed.
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            return new Dictionary();
        }

        /// <summary>
        /// Import save data from persistence
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            // No player-specific data to restore - this is a config-only database.
        }
    }
}
