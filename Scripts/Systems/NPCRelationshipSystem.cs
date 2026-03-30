using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// NPC好感度关系系统 - 管理玩家与NPC之间的关系
    /// 应用Game Narrative Design学习成果
    /// </summary>
    public class NPCRelationshipSystem : BaseSystem
    {
        public static NPCRelationshipSystem Instance { get; private set; }

        // 好感度等级枚举
        public enum RelationshipLevel
        {
            Stranger = 0,      // 陌生人
            Acquaintance = 1,  // 熟人
            Friend = 2,        // 朋友
            CloseFriend = 3,   // 亲密朋友
            BestFriend = 4,    // 挚友
            Soulmate = 5       // 灵魂伴侣
        }

        // 关系数据
        private Dictionary<string, NPCRelationship> _relationships = new Dictionary<string, NPCRelationship>();
        
        // 信号
        public delegate void RelationshipChangedDelegate(string npcId, RelationshipLevel oldLevel, RelationshipLevel newLevel);
        public event RelationshipChangedDelegate OnRelationshipChanged;

        public delegate void GiftGivenDelegate(string npcId, int favorAmount);
        public event GiftGivenDelegate OnGiftGiven;

        public override void _Ready()
        {
            Instance = this;
            LoadData();
        }

        public override void _ExitTree()
        {
            SaveData();
        }

        // 初始化NPC关系数据
        public void InitializeNPCs(List<string> npcIds)
        {
            foreach (var npcId in npcIds)
            {
                if (!_relationships.ContainsKey(npcId))
                {
                    _relationships[npcId] = new NPCRelationship {
                        NpcId = npcId,
                        Favor = 0,
                        TotalGiftsGiven = 0,
                        ConversationsHad = 0,
                        QuestsCompleted = 0,
                        LastInteractionTime = 0
                    };
                }
            }
        }

        // 获取好感度
        public int GetFavor(string npcId)
        {
            if (_relationships.ContainsKey(npcId))
                return _relationships[npcId].Favor;
            return 0;
        }

        // 获取关系等级
        public RelationshipLevel GetRelationshipLevel(string npcId)
        {
            int favor = GetFavor(npcId);
            return GetLevelFromFavor(favor);
        }

        // 从好感度计算等级
        public RelationshipLevel GetLevelFromFavor(int favor)
        {
            if (favor >= 1000) return RelationshipLevel.Soulmate;
            if (favor >= 500) return RelationshipLevel.BestFriend;
            if (favor >= 200) return RelationshipLevel.CloseFriend;
            if (favor >= 50) return RelationshipLevel.Friend;
            if (favor >= 10) return RelationshipLevel.Acquaintance;
            return RelationshipLevel.Stranger;
        }

        // 获取等级名称
        public string GetLevelName(RelationshipLevel level)
        {
            switch (level)
            {
                case RelationshipLevel.Stranger: return "陌生人";
                case RelationshipLevel.Acquaintance: return "熟人";
                case RelationshipLevel.Friend: return "朋友";
                case RelationshipLevel.CloseFriend: return "亲密朋友";
                case RelationshipLevel.BestFriend: return "挚友";
                case RelationshipLevel.Soulmate: return "灵魂伴侣";
                default: return "未知";
            }
        }

        // 获取等级的好感度要求
        public int GetFavorRequired(NPCRelationshipSystem.RelationshipLevel level)
        {
            switch (level)
            {
                case NPCRelationshipSystem.RelationshipLevel.Stranger: return 0;
                case NPCRelationshipSystem.RelationshipLevel.Acquaintance: return 10;
                case NPCRelationshipSystem.RelationshipLevel.Friend: return 50;
                case NPCRelationshipSystem.RelationshipLevel.CloseFriend: return 200;
                case NPCRelationshipSystem.RelationshipLevel.BestFriend: return 500;
                case NPCRelationshipSystem.RelationshipLevel.Soulmate: return 1000;
                default: return 0;
            }
        }

        // 获取好感度百分比
        public float GetFavorProgress(string npcId)
        {
            var currentLevel = GetRelationshipLevel(npcId);
            int currentFavor = GetFavor(npcId);
            int currentRequired = GetFavorRequired(currentLevel);
            
            if (currentLevel == RelationshipLevel.Soulmate)
                return 1.0f;
            
            int nextRequired = GetFavorRequired((RelationshipLevel)((int)currentLevel + 1));
            int favorInLevel = currentFavor - currentRequired;
            int favorNeeded = nextRequired - currentRequired;
            
            return (float)favorInLevel / favorNeeded;
        }

        // 送礼增加好感度
        public int GiveGift(string npcId, string itemId, int itemValue)
        {
            if (!_relationships.ContainsKey(npcId))
                InitializeNPCs(new List<string> { npcId });

            var relationship = _relationships[npcId];
            RelationshipLevel oldLevel = GetRelationshipLevel(npcId);
            
            // 基础好感度 = 物品价值的10%
            int baseFavor = Mathf.Max(1, itemValue / 10);
            
            // 随机波动 ±20%
            float randomFactor = 0.8f + (float)GD.RandDouble() * 0.4f;
            int favorGain = (int)(baseFavor * randomFactor);
            
            relationship.Favor += favorGain;
            relationship.TotalGiftsGiven += 1;
            relationship.LastInteractionTime = OS.GetSystemTimeMsecs();
            
            RelationshipLevel newLevel = GetRelationshipLevel(npcId);
            
            if (oldLevel != newLevel)
            {
                OnRelationshipChanged?.Invoke(npcId, oldLevel, newLevel);
            }
            
            OnGiftGiven?.Invoke(npcId, favorGain);
            
            return favorGain;
        }

        // 对话增加好感度
        public int HaveConversation(string npcId)
        {
            if (!_relationships.ContainsKey(npcId))
                InitializeNPCs(new List<string> { npcId });

            var relationship = _relationships[npcId];
            RelationshipLevel oldLevel = GetRelationshipLevel(npcId);
            
            // 每次对话增加1-3点好感度
            int favorGain = GD.RandI() % 3 + 1;
            
            relationship.Favor += favorGain;
            relationship.ConversationsHad += 1;
            relationship.LastInteractionTime = OS.GetSystemTimeMsecs();
            
            RelationshipLevel newLevel = GetRelationshipLevel(npcId);
            
            if (oldLevel != newLevel)
            {
                OnRelationshipChanged?.Invoke(npcId, oldLevel, newLevel);
            }
            
            return favorGain;
        }

        // 完成NPC任务增加好感度
        public int CompleteQuest(string npcId, int questDifficulty)
        {
            if (!_relationships.ContainsKey(npcId))
                InitializeNPCs(new List<string> { npcId });

            var relationship = _relationships[npcId];
            RelationshipLevel oldLevel = GetRelationshipLevel(npcId);
            
            // 任务难度系数 1-5
            int favorGain = questDifficulty * 5;
            
            relationship.Favor += favorGain;
            relationship.QuestsCompleted += 1;
            relationship.LastInteractionTime = OS.GetSystemTimeMsecs();
            
            RelationshipLevel newLevel = GetRelationshipLevel(npcId);
            
            if (oldLevel != newLevel)
            {
                OnRelationshipChanged?.Invoke(npcId, oldLevel, newLevel);
            }
            
            return favorGain;
        }

        // 获得商店折扣
        public float GetShopDiscount(string npcId)
        {
            var level = GetRelationshipLevel(npcId);
            switch (level)
            {
                case RelationshipLevel.Stranger: return 1.0f;
                case RelationshipLevel.Acquaintance: return 0.98f;
                case RelationshipLevel.Friend: return 0.95f;
                case RelationshipLevel.CloseFriend: return 0.90f;
                case RelationshipLevel.BestFriend: return 0.85f;
                case RelationshipLevel.Soulmate: return 0.80f;
                default: return 1.0f;
            }
        }

        // 解锁对话选项
        public List<string> GetUnlockedDialogueOptions(string npcId)
        {
            var level = GetRelationshipLevel(npcId);
            List<string> options = new List<string>();
            
            if (level >= RelationshipLevel.Acquaintance)
                options.Add("casual_chat");
            
            if (level >= RelationshipLevel.Friend)
                options.Add("personal_story");
            
            if (level >= RelationshipLevel.CloseFriend)
                options.Add("secret_info");
            
            if (level >= RelationshipLevel.BestFriend)
                options.Add("deep_secret");
            
            if (level >= RelationshipLevel.Soulmate)
                options.Add("life_bond");
            
            return options;
        }

        // 解锁特殊任务
        public bool IsSpecialQuestUnlocked(string npcId)
        {
            return GetRelationshipLevel(npcId) >= RelationshipLevel.CloseFriend;
        }

        // 获取关系数据
        public NPCRelationship GetRelationship(string npcId)
        {
            if (_relationships.ContainsKey(npcId))
                return _relationships[npcId];
            return null;
        }

        // 获取所有关系数据
        public Dictionary<string, NPCRelationship> GetAllRelationships()
        {
            return _relationships;
        }

        // 存档数据
        public Dictionary<string, object> GetSaveData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            List<Dictionary<string, object>> relationshipsData = new List<Dictionary<string, object>>();
            
            foreach (var kvp in _relationships)
            {
                relationshipsData.Add(new Dictionary<string, object> {
                    { "npcId", kvp.Value.NpcId },
                    { "favor", kvp.Value.Favor },
                    { "totalGiftsGiven", kvp.Value.TotalGiftsGiven },
                    { "conversationsHad", kvp.Value.ConversationsHad },
                    { "questsCompleted", kvp.Value.QuestsCompleted },
                    { "lastInteractionTime", kvp.Value.LastInteractionTime }
                });
            }
            
            data["relationships"] = relationshipsData;
            return data;
        }

        // 加载数据
        public void LoadFromSaveData(Dictionary<string, object> data)
        {
            if (data == null || !data.ContainsKey("relationships")) return;
            
            var relationshipsData = data["relationships"] as List<object>;
            if (relationshipsData == null) return;
            
            foreach (var relData in relationshipsData)
            {
                var dict = relData as Dictionary<string, object>;
                if (dict == null) continue;
                
                string npcId = dict["npcId"] as string;
                if (string.IsNullOrEmpty(npcId)) continue;
                
                _relationships[npcId] = new NPCRelationship {
                    NpcId = npcId,
                    Favor = Convert.ToInt32(dict["favor"]),
                    TotalGiftsGiven = Convert.ToInt32(dict["totalGiftsGiven"]),
                    ConversationsHad = Convert.ToInt32(dict["conversationsHad"]),
                    QuestsCompleted = Convert.ToInt32(dict["questsCompleted"]),
                    LastInteractionTime = Convert.ToInt64(dict["lastInteractionTime"])
                };
            }
        }
        
        #region Data Persistence
        
        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary(GetSaveData());
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            LoadFromSaveData(new Dictionary<string, object>(data));
        }
        
        #endregion

        private void LoadData()
        {
            // 从存档加载数据
            var saveSystem = GetNode("/root/SaveSystem");
            if (saveSystem != null)
            {
                // SaveSystem会自动调用LoadFromSaveData
            }
        }

        private void SaveData()
        {
            // 存档系统会自动获取数据
        }
    }

    /// <summary>
    /// NPC关系数据
    /// </summary>
    public class NPCRelationship
    {
        public string NpcId { get; set; }
        public int Favor { get; set; }
        public int TotalGiftsGiven { get; set; }
        public int ConversationsHad { get; set; }
        public int QuestsCompleted { get; set; }
        public long LastInteractionTime { get; set; }
    }
}
