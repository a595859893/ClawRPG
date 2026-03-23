using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Event Chain System - 事件连锁核心系统
    /// 应用 PCG 学习成果 - 程序化内容生成
    /// </summary>
    public class EventChainSystem : BaseSystem {
        public static EventChainSystem Instance { get; private set; }

        // ========== 配置数据 ==========
        private Dictionary<string, EventChainData> chains = new Dictionary<string, EventChainData>();

        // ========== 运行时状态 ==========
        private Dictionary<string, ActiveEventChain> activeChains = new Dictionary<string, ActiveEventChain>();
        private List<string> completedChainIds = new List<string>();
        private List<string> failedChainIds = new List<string>();
        private Dictionary<string, int> chainCompletionCount = new Dictionary<string, int>();
        
        // 统计
        private int totalChainsStarted = 0;
        private int totalChainsCompleted = 0;
        private int totalChainsFailed = 0;
        private int totalGoldEarned = 0;
        private int totalExpEarned = 0;

        public override void _Ready() {
            Instance = this;
            InitializeChains();
        }

        // ========== 配置初始化 ==========
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
                category = EventChainCategory.Combat
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

        // ========== 配置查询方法 ==========
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

        // ========== 运行时方法 ==========
        /// <summary>
        /// 尝试开始一个新的事件链
        /// </summary>
        public bool TryStartChain(string eventId, float luckModifier = 1.0f) {
            if (EventChainSystem.Instance == null) return false;

            // 检查是否有可以触发的连锁
            var allChains = EventChainSystem.Instance.GetAllChains();
            foreach (var chain in allChains) {
                // 检查是否需要特定事件触发
                if (chain.requiredEvents.Contains(eventId)) {
                    // 检查是否已经完成过
                    if (completedChainIds.Contains(chain.chainId)) continue;
                    
                    // 随机触发检查
                    float adjustedProb = chain.triggerProbability * luckModifier;
                    if (GD.Randf() < adjustedProb) {
                        StartChain(chain);
                        return true;
                    }
                }
            }

            // 尝试随机开始一个连锁
            var randomChain = EventChainSystem.Instance.GetRandomChain(luckModifier);
            if (randomChain != null && !completedChainIds.Contains(randomChain.chainId)) {
                StartChain(randomChain);
                return true;
            }

            return false;
        }

        private void StartChain(EventChainData chain) {
            var activeChain = new ActiveEventChain {
                chainId = chain.chainId,
                currentStage = 0,
                totalStages = GD.RandRange(chain.minChainLength, chain.maxChainLength),
                progress = 0f,
                isCompleted = false,
                isFailed = false,
                startTime = OS.GetUnixTime()
            };

            activeChains[chain.chainId] = activeChain;
            totalChainsStarted++;
            
            GD.Print($"[EventChain] Started: {chain.chainName} - {activeChain.totalStages} stages");
        }

        /// <summary>
        /// 推进事件链到下一阶段
        /// </summary>
        public void AdvanceChain(string chainId, string nextEventId) {
            if (!activeChains.ContainsKey(chainId)) return;

            var chain = EventChainSystem.Instance.GetChain(chainId);
            if (chain == null) return;

            var activeChain = activeChains[chainId];
            activeChain.currentStage++;
            activeChain.progress = (float)activeChain.currentStage / activeChain.totalStages;

            // 检查是否跟随事件
            if (chain.followUpEvents.Contains(nextEventId)) {
                // 正常推进
            }

            // 检查是否完成
            if (activeChain.currentStage >= activeChain.totalStages) {
                CompleteChain(chainId);
            }

            GD.Print($"[EventChain] Advanced: {chain.chainName} - Stage {activeChain.currentStage}/{activeChain.totalStages}");
        }

        /// <summary>
        /// 完成事件链并发放奖励
        /// </summary>
        private void CompleteChain(string chainId) {
            if (!activeChains.ContainsKey(chainId)) return;

            var chain = EventChainSystem.Instance.GetChain(chainId);
            if (chain == null) return;

            var activeChain = activeChains[chainId];
            activeChain.isCompleted = true;

            completedChainIds.Add(chainId);
            totalChainsCompleted++;

            // 发放奖励
            if (chain.reward != null) {
                totalGoldEarned += chain.reward.goldBonus;
                totalExpEarned += chain.reward.expBonus;
                
                // 这里可以调用经济系统添加金币和经验
                // PlayerStats.AddGold(chain.reward.goldBonus);
                // PlayerStats.AddExp(chain.reward.expBonus);
                
                // 掉落奖励物品
                foreach (var item in chain.reward.bonusItems) {
                    GD.Print($"[EventChain] Reward: {item}");
                    // AddItemToInventory(item);
                }
            }

            // 更新统计
            if (!chainCompletionCount.ContainsKey(chainId)) {
                chainCompletionCount[chainId] = 0;
            }
            chainCompletionCount[chainId]++;

            GD.Print($"[EventChain] Completed: {chain.chainName} - Gold: {chain.reward?.goldBonus}, Exp: {chain.reward?.expBonus}");
        }

        /// <summary>
        /// 失败事件链
        /// </summary>
        public void FailChain(string chainId) {
            if (!activeChains.ContainsKey(chainId)) return;

            var chain = EventChainSystem.Instance.GetChain(chainId);
            var activeChain = activeChains[chainId];
            activeChain.isFailed = true;

            failedChainIds.Add(chainId);
            totalChainsFailed++;

            GD.Print($"[EventChain] Failed: {chain?.chainName ?? chainId}");
        }

        /// <summary>
        /// 获取当前活跃的事件链
        /// </summary>
        public List<ActiveEventChain> GetActiveChains() {
            return new List<ActiveEventChain>(activeChains.Values);
        }

        /// <summary>
        /// 获取特定事件链的状态
        /// </summary>
        public ActiveEventChain GetChainStatus(string chainId) {
            return activeChains.ContainsKey(chainId) ? activeChains[chainId] : null;
        }

        /// <summary>
        /// 获取系统统计
        /// </summary>
        public Dictionary<string, int> GetStatistics() {
            return new Dictionary<string, int> {
                { "total_chains_started", totalChainsStarted },
                { "total_chains_completed", totalChainsCompleted },
                { "total_chains_failed", totalChainsFailed },
                { "total_gold_earned", totalGoldEarned },
                { "total_exp_earned", totalExpEarned },
                { "active_chains", activeChains.Count },
                { "completed_unique_chains", completedChainIds.Count }
            };
        }

        /// <summary>
        /// 检查是否可以开始特定类型的事件链
        /// </summary>
        public bool CanStartChainOfCategory(EventChainCategory category) {
            if (EventChainSystem.Instance == null) return false;
            
            var chains = EventChainSystem.Instance.GetChainsByCategory(category);
            foreach (var chain in chains) {
                if (!completedChainIds.Contains(chain.chainId) && 
                    !activeChains.ContainsKey(chain.chainId)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取特定类别的所有事件链进度
        /// </summary>
        public Dictionary<string, float> GetCategoryProgress(EventChainCategory category) {
            Dictionary<string, float> progress = new Dictionary<string, float>();
            
            if (EventChainSystem.Instance == null) return progress;
            
            var chains = EventChainSystem.Instance.GetChainsByCategory(category);
            foreach (var chain in chains) {
                if (activeChains.ContainsKey(chain.chainId)) {
                    progress[chain.chainName] = activeChains[chain.chainId].progress;
                } else if (completedChainIds.Contains(chain.chainId)) {
                    progress[chain.chainName] = 1.0f;
                } else {
                    progress[chain.chainName] = 0.0f;
                }
            }
            
            return progress;
        }

        // ========== 存档支持 ==========
        public Dictionary<string, object> SaveData() {
            return new Dictionary<string, object> {
                { "completed_chain_ids", completedChainIds },
                { "failed_chain_ids", failedChainIds },
                { "chain_completion_count", chainCompletionCount },
                { "total_chains_started", totalChainsStarted },
                { "total_chains_completed", totalChainsCompleted },
                { "total_chains_failed", totalChainsFailed },
                { "total_gold_earned", totalGoldEarned },
                { "total_exp_earned", totalExpEarned }
            };
        }

        public void LoadData(Dictionary<string, object> data) {
            if (data == null) return;

            if (data.ContainsKey("completed_chain_ids")) {
                completedChainIds = new List<string>((List<string>)data["completed_chain_ids"]);
            }
            if (data.ContainsKey("failed_chain_ids")) {
                failedChainIds = new List<string>((List<string>)data["failed_chain_ids"]);
            }
            if (data.ContainsKey("chain_completion_count")) {
                chainCompletionCount = new Dictionary<string, int>((Dictionary<string, int>)data["chain_completion_count"]);
            }
            if (data.ContainsKey("total_chains_started")) {
                totalChainsStarted = (int)data["total_chains_started"];
            }
            if (data.ContainsKey("total_chains_completed")) {
                totalChainsCompleted = (int)data["total_chains_completed"];
            }
            if (data.ContainsKey("total_chains_failed")) {
                totalChainsFailed = (int)data["total_chains_failed"];
            }
            if (data.ContainsKey("total_gold_earned")) {
                totalGoldEarned = (int)data["total_gold_earned"];
            }
            if (data.ContainsKey("total_exp_earned")) {
                totalExpEarned = (int)data["total_exp_earned"];
            }
        }

        /// <summary>
        /// Export save data for persistence
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            return SaveData();
        }

        /// <summary>
        /// Import save data from persistence
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data != null)
            {
                LoadData(new Dictionary<string, object>(data));
            }
        }
    }
}
