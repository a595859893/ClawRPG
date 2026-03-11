using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 声望等级枚举
    /// </summary>
    public enum ReputationTier {
        Hated = 0,      // 仇恨
        Hostile = 1,    // 敌对
        Unfriendly = 2, // 不友好
        Neutral = 3,    // 中立
        Friendly = 4,    // 友好
        Honored = 5,    // 尊敬
        Revered = 6,    // 崇敬
        Exalted = 7     // 传说
    }

    /// <summary>
    /// 阵营数据
    /// </summary>
    public class Faction {
        public string Id;
        public string Name;
        public string Description;
        public string Icon;
        public int StartingReputation;
        public List<FactionReward> Rewards;
        public Dictionary<string, int> EnemyFactions;
        
        public Faction() {
            Rewards = new List<FactionReward>();
            EnemyFactions = new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// 阵营奖励
    /// </summary>
    public class FactionReward {
        public ReputationTier RequiredTier;
        public int Gold;
        public int Experience;
        public string ItemId;
        public int ItemAmount;
        public string Title;
    }

    /// <summary>
    /// 玩家阵营声望数据
    /// </summary>
    public class PlayerFactionData {
        public string FactionId;
        public int Reputation;
        public ReputationTier Tier;
        public bool RewardClaimed;
        
        public PlayerFactionData() {
            FactionId = "";
            Reputation = 0;
            Tier = ReputationTier.Neutral;
            RewardClaimed = false; 
        }
    }

    /// <summary>
    /// 声望系统 - 管理玩家与阵营的声望关系
    /// </summary>
    public class ReputationSystem {
        private static ReputationSystem _instance;
        public static ReputationSystem Instance {
            get {
                if (_instance == null) _instance = new ReputationSystem();
                return _instance;
            }
        }

        // 信号系统
        public Signal1<string> ReputationChanged { get; }
        public Signal2<string, ReputationTier> TierChanged { get; }
        public Signal2<string, FactionReward> RewardAvailable { get; }
        public Signal1<string> FactionJoined { get; }
        public Signal1<string> FactionLeft { get; }

        private Dictionary<string, Faction> _factions;
        private Dictionary<string, PlayerFactionData> _playerFactions;
        private bool _isInitialized;

        public bool IsInitialized => _isInitialized;

        public ReputationSystem() {
            ReputationChanged = new Signal1<string>();
            TierChanged = new Signal2<string, ReputationTier>();
            RewardAvailable = new Signal2<string, FactionReward>();
            FactionJoined = new Signal1<string>();
            FactionLeft = new Signal1<string>();
            _factions = new Dictionary<string, PlayerFactionData>();
            _playerFactions = new Dictionary<string, PlayerFactionData>();
        }

        /// <summary>
        /// 初始化声望系统
        /// </summary>
        public void Initialize() {
            if (_isInitialized) return;
            
            InitializeFactions();
            _isInitialized = true;
            GD.Print("[ReputationSystem] Initialized");
        }

        /// <summary>
        /// 初始化所有阵营
        /// </summary>
        private void InitializeFactions() {
            _factions = new Dictionary<string, Faction>();
            
            // 战士公会
            var warriors = new Faction {
                Id = "warriors",
                Name = "战士公会",
                Description = "由勇敢的战士组成的古老公会",
                StartingReputation = 0,
                EnemyFactions = new Dictionary<string, int> {
                    { "bandits", 10 }
                },
                Rewards = new List<FactionReward> {
                    new FactionReward { RequiredTier = ReputationTier.Friendly, Gold = 100, Title = "战士之友" },
                    new FactionReward { RequiredTier = ReputationTier.Honored, Experience = 500, ItemId = "steel_sword", ItemAmount = 1 },
                    new FactionReward { RequiredTier = ReputationTier.Exalted, ItemId = "warrior_badge", ItemAmount = 1, Title = "传奇战士" }
                }
            };
            _factions["warriors"] = warriors;

            // 法师议会
            var mages = new Faction {
                Id = "mages",
                Name = "法师议会",
                Description = "掌握奥术魔法的神秘组织",
                StartingReputation = 0,
                EnemyFactions = new Dictionary<string, int> {
                    { "cultists", 15 }
                },
                Rewards = new List<FactionReward> {
                    new FactionReward { RequiredTier = ReputationTier.Friendly, Experience = 200, Title = "法师之友" },
                    new FactionReward { RequiredTier = ReputationTier.Honored, ItemId = "magic_staff", ItemAmount = 1 },
                    new FactionReward { RequiredTier = ReputationTier.Exalted, ItemId = "archmage_robe", ItemAmount = 1, Title = "大法师" }
                }
            };
            _factions["mages"] = mages;

            // 盗贼公会
            var thieves = new Faction {
                Id = "thieves",
                Name = "盗贼公会",
                Description = "暗中活动的隐秘组织",
                StartingReputation = -1000,
                EnemyFactions = new Dictionary<string, int> {
                    { "guards", 5 }
                },
                Rewards = new List<FactionReward> {
                    new FactionReward { RequiredTier = ReputationTier.Friendly, ItemId = "lockpick", ItemAmount = 5 },
                    new FactionReward { RequiredTier = ReputationTier.Honored, ItemId = "shadow_cloak", ItemAmount = 1 },
                    new FactionReward { RequiredTier = ReputationTier.Exalted, ItemId = "master_key", ItemAmount = 1, Title = "影子之手" }
                }
            };
            _factions["thieves"] = thieves;

            // 商人联盟
            var merchants = new Faction {
                Id = "merchants",
                Name = "商人联盟",
                Description = "掌握大陆贸易的富商组织",
                StartingReputation = 0,
                Rewards = new List<FactionReward> {
                    new FactionReward { RequiredTier = ReputationTier.Friendly, Gold = 50, Title = "贵宾" },
                    new FactionReward { RequiredTier = ReputationTier.Honored, Gold = 200, Experience = 100 },
                    new FactionReward { RequiredTier = ReputationTier.Exalted, ItemId = "trade_permit", ItemAmount = 1, Title = "商业大亨" }
                }
            };
            _factions["merchants"] = merchants;

            // 光明教会
            var church = new Faction {
                Id = "church",
                Name = "光明教会",
                Description = "信仰光明之神的神圣组织",
                StartingReputation = 0,
                EnemyFactions = new Dictionary<string, int> {
                    { "demons", 20 },
                    { "cultists", 10 }
                },
                Rewards = new List<FactionReward> {
                    new FactionReward { RequiredTier = ReputationTier.Friendly, Experience = 150, Title = "信徒" },
                    new FactionReward { RequiredTier = ReputationTier.Honored, ItemId = "holy_symbol", ItemAmount = 1 },
                    new FactionReward { RequiredTier = ReputationTier.Exalted, ItemId = "divine_blade", ItemAmount = 1, Title = "圣殿骑士" }
                }
            };
            _factions["church"] = church;

            GD.Print($"[ReputationSystem] Initialized {_factions.Count} factions");
        }

        /// <summary>
        /// 获取所有阵营
        /// </summary>
        public Dictionary<string, Faction> GetAllFactions() {
            return _factions;
        }

        /// <summary>
        /// 获取玩家在指定阵营的声望数据
        /// </summary>
        public PlayerFactionData GetFactionData(string factionId) {
            if (_playerFactions.ContainsKey(factionId)) {
                return _playerFactions[factionId];
            }
            
            // 如果没有数据，创建新的
            if (_factions.ContainsKey(factionId)) {
                var data = new PlayerFactionData {
                    FactionId = factionId,
                    Reputation = _factions[factionId].StartingReputation,
                    Tier = GetTierFromReputation(_factions[factionId].StartingReputation)
                };
                _playerFactions[factionId] = data;
                return data;
            }
            
            return null;
        }

        /// <summary>
        /// 修改玩家声望
        /// </summary>
        public void ModifyReputation(string factionId, int amount) {
            if (!_factions.ContainsKey(factionId)) {
                GD.PrintErr($"[ReputationSystem] Unknown faction: {factionId}");
                return;
            }

            var data = GetFactionData(factionId);
            var oldTier = data.Tier;
            
            data.Reputation += amount;
            data.Tier = GetTierFromReputation(data.Reputation);
            
            ReputationChanged.Call(factionId);
            
            // 检查阵营敌对关系
            var faction = _factions[factionId];
            if (faction.EnemyFactions != null) {
                foreach (var enemy in faction.EnemyFactions) {
                    ModifyReputation(enemy.Key, -enemy.Value);
                }
            }
            
            // 检查是否升级
            if (data.Tier != oldTier) {
                TierChanged.Call(factionId, data.Tier);
                GD.Print($"[ReputationSystem] Player faction tier changed to {data.Tier} for {factionId}");
            }
            
            // 检查是否有可领取奖励
            CheckForRewards(factionId);
        }

        /// <summary>
        /// 根据声望值获取等级
        /// </summary>
        private ReputationTier GetTierFromReputation(int reputation) {
            if (reputation >= 10000) return ReputationTier.Exalted;
            if (reputation >= 5000) return ReputationTier.Revered;
            if (reputation >= 2000) return ReputationTier.Honored;
            if (reputation >= 500) return ReputationTier.Friendly;
            if (reputation >= 0) return ReputationTier.Neutral;
            if (reputation >= -500) return ReputationTier.Unfriendly;
            if (reputation >= -2000) return ReputationTier.Hostile;
            return ReputationTier.Hated;
        }

        /// <summary>
        /// 检查是否有可领取的奖励
        /// </summary>
        private void CheckForRewards(string factionId) {
            if (!_factions.ContainsKey(factionId)) return;
            
            var data = GetFactionData(factionId);
            var faction = _factions[factionId];
            
            foreach (var reward in faction.Rewards) {
                if (data.Tier >= reward.RequiredTier && !data.RewardClaimed) {
                    RewardAvailable.Call(factionId, reward);
                    break;
                }
            }
        }

        /// <summary>
        /// 领取阵营奖励
        /// </summary>
        public bool ClaimReward(string factionId) {
            if (!_factions.ContainsKey(factionId)) return false;
            
            var data = GetFactionData(factionId);
            var faction = _factions[factionId];
            
            foreach (var reward in faction.Rewards) {
                if (data.Tier >= reward.RequiredTier && !data.RewardClaimed) {
                    // 发放奖励
                    var player = GetPlayer();
                    if (player != null) {
                        if (reward.Gold > 0) {
                            player.Call("AddGold", reward.Gold);
                        }
                        if (reward.Experience > 0) {
                            player.Call("AddExperience", reward.Experience);
                        }
                        if (!string.IsNullOrEmpty(reward.ItemId)) {
                            var inventory = InventoryManager.Instance;
                            inventory.AddItem(reward.ItemId, reward.ItemAmount);
                        }
                        if (!string.IsNullOrEmpty(reward.Title)) {
                            var titleSystem = TitleSystem.Instance;
                            if (titleSystem != null && titleSystem.HasMethod("AddTitle")) {
                                titleSystem.Call("AddTitle", reward.Title);
                            }
                        }
                    }
                    
                    data.RewardClaimed = true;
                    GD.Print($"[ReputationSystem] Claimed reward for {factionId}: {reward.Title ?? "reward"}");
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// 获取当前声望等级名称
        /// </summary>
        public string GetTierName(ReputationTier tier) {
            switch (tier) {
                case ReputationTier.Hated: return "仇恨";
                case ReputationTier.Hostile: return "敌对";
                case ReputationTier.Unfriendly: return "不友好";
                case ReputationTier.Neutral: return "中立";
                case ReputationTier.Friendly: return "友好";
                case ReputationTier.Honored: return "尊敬";
                case ReputationTier.Revered: return "崇敬";
                case ReputationTier.Exalted: return "传说";
                default: return "未知";
            }
        }

        /// <summary>
        /// 获取声望进度百分比
        /// </summary>
        public float GetTierProgress(string factionId) {
            var data = GetFactionData(factionId);
            if (data == null) return 0f;
            
            var currentTier = (int)data.Tier;
            var nextTierValue = GetTierThreshold(currentTier + 1);
            var currentTierValue = GetTierThreshold(currentTier);
            
            if (nextTierValue == currentTierValue) return 1f;
            
            return (float)(data.Reputation - currentTierValue) / (nextTierValue - currentTierValue);
        }

        /// <summary>
        /// 获取等级阈值
        /// </summary>
        private int GetTierThreshold(int tierIndex) {
            int[] thresholds = { -10000, -2000, -500, 0, 500, 2000, 5000, 10000 };
            if (tierIndex < 0) return thresholds[0];
            if (tierIndex >= thresholds.Length) return thresholds[thresholds.Length - 1];
            return thresholds[tierIndex];
        }

        /// <summary>
        /// 获取玩家
        /// </summary>
        private Node GetPlayer() {
            var tree = Engine.GetMainLoop();
            if (tree is SceneTree sceneTree) {
                return sceneTree.GetFirstNodeInGroup("player");
            }
            return null;
        }

        /// <summary>
        /// 重置声望数据
        /// </summary>
        public void Reset() {
            _playerFactions.Clear();
            _isInitialized = false; 
        }

        /// <summary>
        /// 加载声望数据
        /// </summary>
        public void LoadData(Dictionary<string, object> data) {
            if (data == null) return;
            
            foreach (var kvp in data) {
                if (kvp.Value is Dictionary<string, object> factionData) {
                    var playerData = new PlayerFactionData {
                        FactionId = kvp.Key,
                        Reputation = factionData.ContainsKey("reputation") ? Convert.ToInt32(factionData["reputation"]) : 0,
                        Tier = factionData.ContainsKey("tier") ? (ReputationTier)Convert.ToInt32(factionData["tier"]) : ReputationTier.Neutral,
                        RewardClaimed = factionData.ContainsKey("reward_claimed") && Convert.ToBoolean(factionData["reward_claimed"])
                    };
                    _playerFactions[kvp.Key] = playerData;
                }
            }
            _isInitialized = true;
        }

        /// <summary>
        /// 保存声望数据
        /// </summary>
        public Dictionary<string, object> SaveData() {
            var data = new Dictionary<string, object>();
            
            foreach (var kvp in _playerFactions) {
                data[kvp.Key] = new Dictionary<string, object> {
                    { "reputation", kvp.Value.Reputation },
                    { "tier", (int)kvp.Value.Tier },
                    { "reward_claimed", kvp.Value.RewardClaimed }
                };
            }
            
            return data;
        }
    }
}
