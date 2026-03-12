using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// NPC关系数据库 - 配置NPC关系相关数据
    /// </summary>
    public class NPCRelationshipDatabase
    {
        public static NPCRelationshipDatabase Instance { get; private set; }

        // NPC基础数据
        private Dictionary<string, NPCRelationshipData> _npcData = new Dictionary<string, NPCRelationshipData>();

        // 礼物好感度配置
        private Dictionary<string, int> _giftFavorValues = new Dictionary<string, int>();

        public NPCRelationshipDatabase()
        {
            Instance = this;
            InitializeNPCs();
            InitializeGiftValues();
        }

        private void InitializeNPCs()
        {
            // 战士导师
            _npcData["warrior_mentor"] = new NPCRelationshipData {
                NpcId = "warrior_mentor",
                DisplayName = "战士导师",
                Title = " noble warrior",
                Description = "一位经验丰富的战士，擅长武器训练和战斗技巧。",
                Location = "训练场",
                PreferredGifts = new List<string> { "weapon", "armor", "herb" },
                DislikedGifts = new List<string> { "magic_book", "fish" },
                SpecialQuestUnlocksAt = NPCRelationshipSystem.RelationshipLevel.BestFriend,
                UniqueDialogues = new Dictionary<NPCRelationshipSystem.RelationshipLevel, List<string>> {
                    { NPCRelationshipSystem.RelationshipLevel.Stranger, new List<string> { "欢迎，陌生人。" } },
                    { NPCRelationshipSystem.RelationshipLevel.Acquaintance, new List<string> { "是你啊，经常来训练场转转。" } },
                    { NPCRelationshipSystem.RelationshipLevel.Friend, new List<string> { "你的战斗技巧进步不小！" } },
                    { NPCRelationshipSystem.RelationshipLevel.CloseFriend, new List<string> { "我有一个重要的任务要委托给你..." } },
                    { NPCRelationshipSystem.RelationshipLevel.BestFriend, new List<string> { "你是我最信任的战士。" } },
                    { NPCRelationshipSystem.RelationshipLevel.Soulmate, new List<string> { "我们一起经历了太多，你是我真正的战友。" } }
                }
            };

            // 法师导师
            _npcData["mage_mentor"] = new NPCRelationshipData {
                NpcId = "mage_mentor",
                DisplayName = "法师导师",
                Title = "arcane scholar",
                Description = "一位博学的法师，精通各种魔法流派。",
                Location = "法师塔",
                PreferredGifts = new List<string> { "magic_book", "crystal", "potion" },
                DislikedGifts = new List<string> { "weapon", "meat" },
                SpecialQuestUnlocksAt = NPCRelationshipSystem.RelationshipLevel.BestFriend,
                UniqueDialogues = new Dictionary<NPCRelationshipSystem.RelationshipLevel, List<string>> {
                    { NPCRelationshipSystem.RelationshipLevel.Stranger, new List<string> { "你好，探索者。" } },
                    { NPCRelationshipSystem.RelationshipLevel.Acquaintance, new List<string> { "魔法是一门需要耐心的艺术。" } },
                    { NPCRelationshipSystem.RelationshipLevel.Friend, new List<string> { "你的魔法天赋令人惊讶。" } },
                    { NPCRelationshipSystem.RelationshipLevel.CloseFriend, new List<string> { "我观察到你有成为大法师的潜质。" } },
                    { NPCRelationshipSystem.RelationshipLevel.BestFriend, new List<string> { "我的毕生所学，终于找到了传人。" } },
                    { NPCRelationshipSystem.RelationshipLevel.Soulmate, new List<string> { "魔法将我们紧密联系在一起。" } }
                }
            };

            // 商店老板
            _npcData["shop_owner"] = new NPCRelationshipData {
                NpcId = "shop_owner",
                DisplayName = "商店老板",
                Title = "merchant",
                Description = "一位精明的商人，出售各种商品。",
                Location = "集市",
                PreferredGifts = new List<string> { "gem", "gold", "wine" },
                DislikedGifts = new List<string> { "herb", "fish" },
                SpecialQuestUnlocksAt = NPCRelationshipSystem.RelationshipLevel.CloseFriend,
                UniqueDialogues = new Dictionary<NPCRelationshipSystem.RelationshipLevel, List<string>> {
                    { NPCRelationshipSystem.RelationshipLevel.Stranger, new List<string> { "欢迎光临！" } },
                    { NPCRelationshipSystem.RelationshipLevel.Acquaintance, new List<string> { "又来了啊，今天想买点什么？" } },
                    { NPCRelationshipSystem.RelationshipLevel.Friend, new List<string> { "给你留了几件好货！" } },
                    { NPCRelationshipSystem.RelationshipLevel.CloseFriend, new List<string> { "我有个生意上的小麻烦..." } },
                    { NPCRelationshipSystem.RelationshipLevel.BestFriend, new List<string> { "你是我最好的顾客兼朋友。" } },
                    { NPCRelationshipSystem.RelationshipLevel.Soulmate, new List<string> { "我们的缘分不止于此。" } }
                }
            };

            // 铁匠
            _npcData["blacksmith"] = new NPCRelationshipData {
                NpcId = "blacksmith",
                DisplayName = "铁匠",
                Title = "master craftsman",
                Description = "技艺精湛的铁匠，能够打造优质装备。",
                Location = "铁匠铺",
                PreferredGifts = new List<string> { "ore", "coal", "weapon" },
                DislikedGifts = new List<string> { "magic_book", "potion" },
                SpecialQuestUnlocksAt = NPCRelationshipSystem.RelationshipLevel.CloseFriend,
                UniqueDialogues = new Dictionary<NPCRelationshipSystem.RelationshipLevel, List<string>> {
                    { NPCRelationshipSystem.RelationshipLevel.Stranger, new List<string> { "锻造需要力量与技巧。" } },
                    { NPCRelationshipSystem.RelationshipLevel.Acquaintance, new List<string> { "想要打造点什么？" } },
                    { NPCRelationshipSystem.RelationshipLevel.Friend, new List<string> { "你的武器手感不错！" } },
                    { NPCRelationshipSystem.RelationshipLevel.CloseFriend, new List<string> { "我需要一块特殊的矿石..." } },
                    { NPCRelationshipSystem.RelationshipLevel.BestFriend, new List<string> { "你是唯一理解锻造之美的人。" } },
                    { NPCRelationshipSystem.RelationshipLevel.Soulmate, new List<string> { "让我们一起打造传奇！" } }
                }
            };

            // 护士/治疗师
            _npcData["healer"] = new NPCRelationshipData {
                NpcId = "healer",
                DisplayName = "治疗师",
                Title = "holy healer",
                Description = "掌握神圣魔法的治疗者，救治伤患。",
                Location = "教堂",
                PreferredGifts = new List<string> { "herb", "flower", "potion" },
                DislikedGifts = new List<string> { "weapon", "meat" },
                SpecialQuestUnlocksAt = NPCRelationshipSystem.RelationshipLevel.BestFriend,
                UniqueDialogues = new Dictionary<NPCRelationshipSystem.RelationshipLevel, List<string>> {
                    { NPCRelationshipSystem.RelationshipLevel.Stranger, new List<string> { "愿神圣之光保佑你。" } },
                    { NPCRelationshipSystem.RelationshipLevel.Acquaintance, new List<string> { "你需要休息吗？" } },
                    { NPCRelationshipSystem.RelationshipLevel.Friend, new List<string> { "你的身体恢复得不错。" } },
                    { NPCRelationshipSystem.RelationshipLevel.CloseFriend, new List<string> { "有一个受伤的灵魂需要帮助..." } },
                    { NPCRelationshipSystem.RelationshipLevel.BestFriend, new List<string> { "你拥有治愈之心。" } },
                    { NPCRelationshipSystem.RelationshipLevel.Soulmate, new List<string> { "我们共同守护生命的奇迹。" } }
                }
            };

            // 吟游诗人
            _npcData["bard"] = new NPCRelationshipData {
                NpcId = "bard",
                DisplayName = "吟游诗人",
                Title = "wandering bard",
                Description = "游历四方的诗人，传唱英雄的事迹。",
                Location = "酒馆",
                PreferredGifts = new List<string> { "instrument", "wine", "book" },
                DislikedGifts = new List<string> { "ore", "weapon" },
                SpecialQuestUnlocksAt = NPCRelationshipSystem.RelationshipLevel.CloseFriend,
                UniqueDialogues = new Dictionary<NPCRelationshipSystem.RelationshipLevel, List<string>> {
                    { NPCRelationshipSystem.RelationshipLevel.Stranger, new List<string> { "想听一段故事吗？" } },
                    { NPCRelationshipSystem.RelationshipLevel.Acquaintance, new List<string> { "你的冒险值得被传唱。" } },
                    { NPCRelationshipSystem.RelationshipLevel.Friend, new List<string> { "让我为你弹奏一首！" } },
                    { NPCRelationshipSystem.RelationshipLevel.CloseFriend, new List<string> { "我有一首关于你的歌..." } },
                    { NPCRelationshipSystem.RelationshipLevel.BestFriend, new List<string> { "你是我最好的灵感来源。" } },
                    { NPCRelationshipSystem.RelationshipLevel.Soulmate, new List<string> { "我们的故事将永远流传。" } }
                }
            };

            // 盗贼工会联络人
            _npcData["thief_contact"] = new NPCRelationshipData {
                NpcId = "thief_contact",
                DisplayName = "影子商人",
                Title = "shadow broker",
                Description = "隐藏在暗处的神秘商人。",
                Location = "小巷",
                PreferredGifts = new List<string> { "gem", "gold", "map" },
                DislikedGifts = new List<string> { "flower", "herb" },
                SpecialQuestUnlocksAt = NPCRelationshipSystem.RelationshipLevel.Soulmate,
                UniqueDialogues = new Dictionary<NPCRelationshipSystem.RelationshipLevel, List<string>> {
                    { NPCRelationshipSystem.RelationshipLevel.Stranger, new List<string> { "...什么事？" } },
                    { NPCRelationshipSystem.RelationshipLevel.Acquaintance, new List<string> { "你是怎么找到这里的？" } },
                    { NPCRelationshipSystem.RelationshipLevel.Friend, new List<string> { "你的技巧越来越纯熟。" } },
                    { NPCRelationshipSystem.RelationshipLevel.CloseFriend, new List<string> { "有个高风险高回报的任务..." } },
                    { NPCRelationshipSystem.RelationshipLevel.BestFriend, new List<string> { "你是我最信任的伙伴。" } },
                    { NPCRelationshipSystem.RelationshipLevel.Soulmate, new List<string> { "我们是一路人。" } }
                }
            };

            // 城镇守卫队长
            _npcData["guard_captain"] = new NPCRelationshipData {
                NpcId = "guard_captain",
                DisplayName = "守卫队长",
                Title = "city guard captain",
                Description = "守护城镇安全的军官。",
                Location = "城门",
                PreferredGifts = new List<string> { "weapon", "armor", "wine" },
                DislikedGifts = new List<string> { "thief_tool", "map" },
                SpecialQuestUnlocksAt = NPCRelationshipSystem.RelationshipLevel.BestFriend,
                UniqueDialogues = new Dictionary<NPCRelationshipSystem.RelationshipLevel, List<string>> {
                    { NPCRelationshipSystem.RelationshipLevel.Stranger, new List<string> { "进城请遵守规矩。" } },
                    { NPCRelationshipSystem.RelationshipLevel.Acquaintance, new List<string> { "最近城里不太平。" } },
                    { NPCRelationshipSystem.RelationshipLevel.Friend, new List<string> { "有你在，我就放心了。" } },
                    { NPCRelationshipSystem.RelationshipLevel.CloseFriend, new List<string> { "我需要你的帮助来调查一件事..." } },
                    { NPCRelationshipSystem.RelationshipLevel.BestFriend, new List<string> { "你是我最可靠的战友。" } },
                    { NPCRelationshipSystem.RelationshipLevel.Soulmate, new List<string> { "让我们一起守护这座城市。" } }
                }
            };
        }

        private void InitializeGiftValues()
        {
            // 礼物好感度价值
            _giftFavorValues["gem"] = 100;
            _giftFavorValues["gold"] = 50;
            _giftFavorValues["crystal"] = 80;
            _giftFavorValues["weapon"] = 60;
            _giftFavorValues["armor"] = 60;
            _giftFavorValues["potion"] = 30;
            _giftFavorValues["magic_book"] = 70;
            _giftFavorValues["herb"] = 20;
            _giftFavorValues["ore"] = 25;
            _giftFavorValues["coal"] = 15;
            _giftFavorValues["flower"] = 15;
            _giftFavorValues["wine"] = 35;
            _giftFavorValues["instrument"] = 55;
            _giftFavorValues["book"] = 40;
            _giftFavorValues["fish"] = 10;
            _giftFavorValues["meat"] = 20;
            _giftFavorValues["map"] = 30;
            _giftFavorValues["thief_tool"] = 45;
        }

        // 获取NPC数据
        public NPCRelationshipData GetNPCData(string npcId)
        {
            if (_npcData.ContainsKey(npcId))
                return _npcData[npcId];
            return null;
        }

        // 获取所有NPC ID
        public List<string> GetAllNPCIds()
        {
            return new List<string>(_npcData.Keys);
        }

        // 获取礼物价值
        public int GetGiftValue(string itemId)
        {
            if (_giftFavorValues.ContainsKey(itemId))
                return _giftFavorValues[itemId];
            return 10; // 默认价值
        }

        // 检查NPC是否喜欢某礼物
        public bool DoesNPCLikeGift(string npcId, string itemId)
        {
            var npcData = GetNPCData(npcId);
            if (npcData == null) return false;
            return npcData.PreferredGifts.Contains(itemId);
        }

        // 检查NPC是否讨厌某礼物
        public bool DoesNPCDislikeGift(string npcId, string itemId)
        {
            var npcData = GetNPCData(npcId);
            if (npcData == null) return false;
            return npcData.DislikedGifts.Contains(itemId);
        }
    }

    /// <summary>
    /// NPC关系数据
    /// </summary>
    public class NPCRelationshipData
    {
        public string NpcId { get; set; }
        public string DisplayName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public List<string> PreferredGifts { get; set; }
        public List<string> DislikedGifts { get; set; }
        public NPCRelationshipSystem.RelationshipLevel SpecialQuestUnlocksAt { get; set; }
        public Dictionary<NPCRelationshipSystem.RelationshipLevel, List<string>> UniqueDialogues { get; set; }
    }
}
