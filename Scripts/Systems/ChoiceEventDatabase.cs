using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 事件选择数据库 - 管理所有随机事件
    /// </summary>
    public class ChoiceEventDatabase : DatabaseBase
    {
        private static ChoiceEventDatabase _instance;
        public static ChoiceEventDatabase Instance => _instance ??= new ChoiceEventDatabase();

        private Dictionary<string, ChoiceEventData> _events = new Dictionary<string, ChoiceEventData>();

        // 玩家选择记录（按玩家ID索引）
        private Dictionary<string, PlayerEventRecord> _playerRecords = new Dictionary<string, PlayerEventRecord>();

        // 事件冷却数据（按玩家ID索引）
        private Dictionary<string, Dictionary<string, DateTime>> _eventCooldowns = new Dictionary<string, Dictionary<string, DateTime>>();

        public override object Instance => Instance;

        public override void Initialize()
        {
            InitializeEvents();
        }

        public override bool ValidateData()
        {
            return _events != null && _events.Count > 0;
        }

        #region 玩家数据管理

        /// <summary>
        /// 记录玩家选择
        /// </summary>
        public void RecordPlayerChoice(string playerId, string eventId, string optionId)
        {
            if (!_playerRecords.ContainsKey(playerId))
            {
                _playerRecords[playerId] = new PlayerEventRecord { PlayerId = playerId };
            }

            var record = _playerRecords[playerId];
            record.ChoicesMade++;

            if (!record.EventChoiceHistory.ContainsKey(eventId))
            {
                record.EventChoiceHistory[eventId] = new List<string>();
            }
            record.EventChoiceHistory[eventId].Add(optionId);
        }

        /// <summary>
        /// 解锁事件
        /// </summary>
        public void UnlockEvent(string playerId, string eventId)
        {
            if (!_playerRecords.ContainsKey(playerId))
            {
                _playerRecords[playerId] = new PlayerEventRecord { PlayerId = playerId };
            }

            if (!_playerRecords[playerId].UnlockedEvents.Contains(eventId))
            {
                _playerRecords[playerId].UnlockedEvents.Add(eventId);
            }
        }

        /// <summary>
        /// 检查事件是否解锁
        /// </summary>
        public bool IsEventUnlocked(string playerId, string eventId)
        {
            if (_playerRecords.TryGetValue(playerId, out var record))
            {
                return record.UnlockedEvents.Contains(eventId);
            }
            return false;
        }

        /// <summary>
        /// 设置事件冷却
        /// </summary>
        public void SetEventCooldown(string playerId, string eventId, TimeSpan cooldown)
        {
            if (!_eventCooldowns.ContainsKey(playerId))
            {
                _eventCooldowns[playerId] = new Dictionary<string, DateTime>();
            }

            _eventCooldowns[playerId][eventId] = DateTime.Now + cooldown;
        }

        /// <summary>
        /// 检查事件是否在冷却中
        /// </summary>
        public bool IsEventOnCooldown(string playerId, string eventId)
        {
            if (_eventCooldowns.TryGetValue(playerId, out var cooldowns))
            {
                if (cooldowns.TryGetValue(eventId, out var cooldownEnd))
                {
                    return DateTime.Now < cooldownEnd;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取玩家事件记录
        /// </summary>
        public PlayerEventRecord GetPlayerRecord(string playerId)
        {
            if (_playerRecords.TryGetValue(playerId, out var record))
            {
                return record;
            }
            return null;
        }

        #endregion

        #region 持久化

        protected override void OnExportSaveData(Godot.Collections.Dictionary saveData)
        {
            // 导出玩家选择记录
            var playerRecordsData = new Godot.Collections.Dictionary();
            foreach (var kvp in _playerRecords)
            {
                var recordDict = new Godot.Collections.Dictionary
                {
                    ["playerId"] = kvp.Value.PlayerId,
                    ["choicesMade"] = kvp.Value.ChoicesMade,
                    ["eventChoiceHistory"] = new Godot.Collections.Dictionary()
                };

                var historyDict = (Godot.Collections.Dictionary)recordDict["eventChoiceHistory"];
                foreach (var historyKvp in kvp.Value.EventChoiceHistory)
                {
                    historyDict[historyKvp.Key] = new Godot.Collections.Array(historyKvp.Value);
                }

                recordDict["unlockedEvents"] = new Godot.Collections.Array(kvp.Value.UnlockedEvents);
                playerRecordsData[kvp.Key] = recordDict;
            }
            saveData["playerRecords"] = playerRecordsData;

            // 导出事件冷却数据
            var cooldownsData = new Godot.Collections.Dictionary();
            foreach (var playerKvp in _eventCooldowns)
            {
                var playerCooldowns = new Godot.Collections.Dictionary();
                foreach (var cooldownKvp in playerKvp.Value)
                {
                    playerCooldowns[cooldownKvp.Key] = cooldownKvp.Value.Ticks;
                }
                cooldownsData[playerKvp.Key] = playerCooldowns;
            }
            saveData["eventCooldowns"] = cooldownsData;
        }

        protected override void OnImportSaveData(Godot.Collections.Dictionary saveData)
        {
            // 导入玩家选择记录
            if (saveData.TryGetValue("playerRecords", out var recordsObj) && recordsObj is Godot.Collections.Dictionary recordsData)
            {
                foreach (var playerKvp in recordsData)
                {
                    if (playerKvp.Value is Godot.Collections.Dictionary recordDict)
                    {
                        var playerId = playerKvp.Key.ToString();
                        var record = new PlayerEventRecord
                        {
                            PlayerId = playerId
                        };

                        if (recordDict.TryGetValue("choicesMade", out var choicesMade))
                            record.ChoicesMade = Convert.ToInt32(choicesMade);

                        if (recordDict.TryGetValue("unlockedEvents", out var unlockedObj) && unlockedObj is Godot.Collections.Array unlockedArray)
                        {
                            foreach (var item in unlockedArray)
                                record.UnlockedEvents.Add(item.ToString());
                        }

                        if (recordDict.TryGetValue("eventChoiceHistory", out var historyObj) && historyObj is Godot.Collections.Dictionary historyDict)
                        {
                            foreach (var historyKvp in historyDict)
                            {
                                if (historyKvp.Value is Godot.Collections.Array choiceArray)
                                {
                                    var choices = new List<string>();
                                    foreach (var choice in choiceArray)
                                        choices.Add(choice.ToString());
                                    record.EventChoiceHistory[historyKvp.Key.ToString()] = choices;
                                }
                            }
                        }

                        _playerRecords[playerId] = record;
                    }
                }
            }

            // 导入事件冷却数据
            if (saveData.TryGetValue("eventCooldowns", out var cooldownsObj) && cooldownsObj is Godot.Collections.Dictionary cooldownsData)
            {
                foreach (var playerKvp in cooldownsData)
                {
                    if (playerKvp.Value is Godot.Collections.Dictionary playerCooldowns)
                    {
                        var playerId = playerKvp.Key.ToString();
                        var cooldownDict = new Dictionary<string, DateTime>();

                        foreach (var cooldownKvp in playerCooldowns)
                        {
                            if (Convert.ToInt64(cooldownKvp.Value) > DateTime.Now.Ticks)
                            {
                                cooldownDict[cooldownKvp.Key.ToString()] = new DateTime(Convert.ToInt64(cooldownKvp.Value));
                            }
                        }

                        _eventCooldowns[playerId] = cooldownDict;
                    }
                }
            }
        }

        #endregion

        private void InitializeEvents()
        {
            // === 战斗类事件 ===
            AddEvent(new ChoiceEventData {
                EventId = "ambush",
                Title = "遭遇伏击",
                Description = "你在探索时遭遇了一伙盗贼的伏击！",
                Category = "Combat",
                MinPlayerLevel = 1,
                Options = new List<ChoiceOption> {
                    new ChoiceOption {
                        OptionId = "fight",
                        Text = "战斗到底",
                        ResultText = "你击败了盗贼，获得了战利品！",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Gold", Amount = 50, Chance = 1.0f },
                            new RewardItem { Type = "Exp", Amount = 30, Chance = 1.0f }
                        },
                        Weight = 0.4f
                    },
                    new ChoiceOption {
                        OptionId = "negotiate",
                        Text = "谈判和解",
                        ResultText = "你支付了一些金币换取安全通过",
                        RequiresGold = true,
                        GoldCost = 30,
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Exp", Amount = 10, Chance = 1.0f }
                        },
                        Weight = 0.3f
                    },
                    new ChoiceOption {
                        OptionId = "flee",
                        Text = "快速逃跑",
                        ResultText = "你成功逃跑了，但没有获得任何奖励",
                        Weight = 0.3f
                    }
                }
            });

            AddEvent(new ChoiceEventData {
                EventId = "monster_den",
                Title = "怪物巢穴",
                Description = "你发现了一个怪物巢穴，里面似乎有宝贝...",
                Category = "Combat",
                MinPlayerLevel = 5,
                Options = new List<ChoiceOption> {
                    new ChoiceOption {
                        OptionId = "attack",
                        Text = "发起攻击",
                        ResultText = "你消灭了怪物，发现了宝藏！",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Gold", Amount = 100, Chance = 1.0f },
                            new RewardItem { Type = "Exp", Amount = 50, Chance = 1.0f },
                            new RewardItem { Type = "Item", Id = "material_001", Amount = 2, Chance = 0.5f }
                        },
                        Weight = 0.5f
                    },
                    new ChoiceOption {
                        OptionId = "stealth",
                        Text = "悄悄潜入",
                        ResultText = "你偷偷拿了一些财宝然后离开",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Gold", Amount = 60, Chance = 1.0f }
                        },
                        Weight = 0.3f
                    },
                    new ChoiceOption {
                        OptionId = "ignore",
                        Text = "绕道而行",
                        ResultText = "你选择了安全离开",
                        Weight = 0.2f
                    }
                }
            });

            // === 探索类事件 ===
            AddEvent(new ChoiceEventData {
                EventId = "ancient_ruins",
                Title = "古代遗迹",
                Description = "你在探索中发现了一座神秘的古代遗迹",
                Category = "Exploration",
                MinPlayerLevel = 3,
                Options = new List<ChoiceOption> {
                    new ChoiceOption {
                        OptionId = "explore",
                        Text = "深入探索",
                        ResultText = "你发现了古老的宝藏！",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Gold", Amount = 150, Chance = 1.0f },
                            new RewardItem { Type = "Exp", Amount = 80, Chance = 1.0f },
                            new RewardItem { Type = "Item", Id = "ancient_coin", Amount = 1, Chance = 0.3f }
                        },
                        Weight = 0.4f
                    },
                    new ChoiceOption {
                        OptionId = "research",
                        Text = "研究碑文",
                        ResultText = "你从碑文学到了古老的知识",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Exp", Amount = 100, Chance = 1.0f }
                        },
                        Weight = 0.3f
                    },
                    new ChoiceOption {
                        OptionId = "leave",
                        Text = "离开",
                        ResultText = "你决定不打扰这片宁静",
                        Weight = 0.3f
                    }
                }
            });

            AddEvent(new ChoiceEventData {
                EventId = "hidden_chest",
                Title = "隐藏宝箱",
                Description = "你注意到墙壁上有一个隐蔽的开关...",
                Category = "Exploration",
                MinPlayerLevel = 1,
                Options = new List<ChoiceOption> {
                    new ChoiceOption {
                        OptionId = "open",
                        Text = "打开开关",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Gold", Amount = 80, Chance = 0.7f },
                            new RewardItem { Type = "Item", Id = "gem_001", Amount = 1, Chance = 0.3f }
                        },
                        Weight = 0.6f
                    },
                    new ChoiceOption {
                        OptionId = "ignore",
                        Text = "不冒险",
                        Weight = 0.4f
                    }
                }
            });

            // === 神秘类事件 ===
            AddEvent(new ChoiceEventData {
                EventId = "mysterious_merchant",
                Title = "神秘商人",
                Description = "一个神秘的商人出现在你面前，想要交易",
                Category = "Mystery",
                MinPlayerLevel = 1,
                Options = new List<ChoiceOption> {
                    new ChoiceOption {
                        OptionId = "buy",
                        Text = "购买物品",
                        RequiresGold = true,
                        GoldCost = 100,
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Item", Id = "rare_potion", Amount = 3, Chance = 1.0f }
                        },
                        Weight = 0.4f
                    },
                    new ChoiceOption {
                        OptionId = "trade",
                        Text = "以物易物",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Item", Id = "mystic_trade", Amount = 1, Chance = 0.5f }
                        },
                        Weight = 0.3f
                    },
                    new ChoiceOption {
                        OptionId = "refuse",
                        Text = "拒绝交易",
                        ResultText = "你礼貌地离开了",
                        Weight = 0.3f
                    }
                }
            });

            AddEvent(new ChoiceEventData {
                EventId = "strange_portal",
                Title = "奇怪传送门",
                Description = "地面上有一个散发诡异光芒的传送门",
                Category = "Mystery",
                MinPlayerLevel = 10,
                Options = new List<ChoiceOption> {
                    new ChoiceOption {
                        OptionId = "enter",
                        Text = "进入传送门",
                        ResultText = "你被传送到了未知的地方...",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Gold", Amount = 200, Chance = 0.6f },
                            new RewardItem { Type = "Exp", Amount = 150, Chance = 0.8f },
                            new RewardItem { Type = "Item", Id = "portal_shard", Amount = 1, Chance = 0.2f }
                        },
                        Penalties = new List<PenaltyItem> {
                            new PenaltyItem { Type = "Health", Amount = 20, Chance = 0.3f }
                        },
                        Weight = 0.4f
                    },
                    new ChoiceOption {
                        OptionId = "study",
                        Text = "研究传送门",
                        ResultText = "你学到了空间魔法的奥秘",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Exp", Amount = 50, Chance = 1.0f }
                        },
                        Weight = 0.3f
                    },
                    new ChoiceOption {
                        OptionId = "avoid",
                        Text = "绕开它",
                        ResultText = "你安全地绕开了传送门",
                        Weight = 0.3f
                    }
                }
            });

            // === 祝福类事件 ===
            AddEvent(new ChoiceEventData {
                EventId = "shrine",
                Title = "神圣 shrine",
                Description = "你发现了一个古老的神殿，里面供奉着神灵",
                Category = "Blessing",
                MinPlayerLevel = 1,
                Options = new List<ChoiceOption> {
                    new ChoiceOption {
                        OptionId = "pray",
                        Text = "祈祷祈福",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Buff", Id = "blessing", Amount = 1, Chance = 0.7f },
                            new RewardItem { Type = "Exp", Amount = 20, Chance = 1.0f }
                        },
                        Weight = 0.5f
                    },
                    new ChoiceOption {
                        OptionId = "sacrifice",
                        Text = "献祭物品",
                        RequiresGold = true,
                        GoldCost = 50,
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Buff", Id = "blessing", Amount = 1, Chance = 0.9f },
                            new RewardItem { Type = "Exp", Amount = 40, Chance = 1.0f }
                        },
                        Weight = 0.3f
                    },
                    new ChoiceOption {
                        OptionId = "ignore",
                        Text = "离开",
                        Weight = 0.2f
                    }
                }
            });

            AddEvent(new ChoiceEventData {
                EventId = "healing_spring",
                Title = "治愈之泉",
                Description = "你发现了一处散发温和光芒的泉水",
                Category = "Blessing",
                MinPlayerLevel = 1,
                Options = new List<ChoiceOption> {
                    new ChoiceOption {
                        OptionId = "drink",
                        Text = "饮用泉水",
                        ResultText = "泉水恢复了你的体力！",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Buff", Id = "healing", Amount = 1, Chance = 1.0f }
                        },
                        Weight = 0.6f
                    },
                    new ChoiceOption {
                        OptionId = "collect",
                        Text = "收集泉水",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Item", Id = "spring_water", Amount = 3, Chance = 1.0f }
                        },
                        Weight = 0.4f
                    }
                }
            });

            // === 诅咒类事件 ===
            AddEvent(new ChoiceEventData {
                EventId = "cursed_artifact",
                Title = "受诅咒的神器",
                Description = "你发现了一个散发不祥气息的古老神器",
                Category = "Curse",
                MinPlayerLevel = 5,
                Options = new List<ChoiceOption> {
                    new ChoiceOption {
                        OptionId = "take",
                        Text = "拿走神器",
                        ResultText = "你拿走了神器，但受到了诅咒...",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Gold", Amount = 300, Chance = 1.0f },
                            new RewardItem { Type = "Item", Id = "cursed_artifact", Amount = 1, Chance = 1.0f }
                        },
                        Penalties = new List<PenaltyItem> {
                            new PenaltyItem { Type = "Debuff", Id = "curse", Amount = 1 }
                        },
                        Weight = 0.3f
                    },
                    new ChoiceOption {
                        OptionId = "destroy",
                        Text = "摧毁神器",
                        ResultText = "你摧毁了神器，获得了功德",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Exp", Amount = 60, Chance = 1.0f }
                        },
                        Weight = 0.3f
                    },
                    new ChoiceOption {
                        OptionId = "leave",
                        Text = "不打扰",
                        ResultText = "你快速离开了这个危险的地方",
                        Weight = 0.4f
                    }
                }
            });

            AddEvent(new ChoiceEventData {
                EventId = "dark_ritual",
                Title = "黑暗仪式",
                Description = "你意外闯入了正在进行中的黑暗仪式",
                Category = "Curse",
                MinPlayerLevel = 8,
                Options = new List<ChoiceOption> {
                    new ChoiceOption {
                        OptionId = "interrupt",
                        Text = "打断仪式",
                        ResultText = "你成功打断了仪式，获得了战利品",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Gold", Amount = 150, Chance = 1.0f },
                            new RewardItem { Type = "Exp", Amount = 80, Chance = 1.0f }
                        },
                        Weight = 0.4f
                    },
                    new ChoiceOption {
                        OptionId = "join",
                        Text = "加入仪式",
                        ResultText = "你获得了黑暗力量，但也受到了影响...",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Buff", Id = "dark_power", Amount = 1, Chance = 1.0f }
                        },
                        Penalties = new List<PenaltyItem> {
                            new PenaltyItem { Type = "Debuff", Id = "shadow_mark", Amount = 1 }
                        },
                        Weight = 0.2f
                    },
                    new ChoiceOption {
                        OptionId = "escape",
                        Text = "偷偷离开",
                        ResultText = "你成功逃走了",
                        Weight = 0.4f
                    }
                }
            });

            // === 更多探索事件 ===
            AddEvent(new ChoiceEventData {
                EventId = "abandoned_camp",
                Title = "废弃营地",
                Description = "你发现了一个看起来已经废弃的冒险者营地",
                Category = "Exploration",
                MinPlayerLevel = 2,
                Options = new List<ChoiceOption> {
                    new ChoiceOption {
                        OptionId = "search",
                        Text = "搜索营地",
                        ResultText = "你找到了一些有用的物品！",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Gold", Amount = 40, Chance = 0.8f },
                            new RewardItem { Type = "Item", Id = "potion_001", Amount = 2, Chance = 0.6f },
                            new RewardItem { Type = "Item", Id = "material_002", Amount = 1, Chance = 0.4f }
                        },
                        Weight = 0.5f
                    },
                    new ChoiceOption {
                        OptionId = "rest",
                        Text = "休息一下",
                        ResultText = "你休息了一会儿，恢复了一些体力",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Buff", Id = "rest", Amount = 1, Chance = 1.0f }
                        },
                        Weight = 0.3f
                    },
                    new ChoiceOption {
                        OptionId = "ignore",
                        Text = "继续前进",
                        Weight = 0.2f
                    }
                }
            });

            AddEvent(new ChoiceEventData {
                EventId = "traveling_merchant",
                Title = "旅行商人",
                Description = "你遇到了一位旅行商人，他的货物很神秘...",
                Category = "Mystery",
                MinPlayerLevel = 1,
                Options = new List<ChoiceOption> {
                    new ChoiceOption {
                        OptionId = "browse",
                        Text = "浏览货物",
                        RequiresGold = true,
                        GoldCost = 80,
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Item", Id = "mystery_box", Amount = 1, Chance = 1.0f }
                        },
                        Weight = 0.5f
                    },
                    new ChoiceOption {
                        OptionId = "ask",
                        Text = "询问情报",
                        ResultText = "商人告诉你一些有用的信息",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Exp", Amount = 25, Chance = 1.0f }
                        },
                        Weight = 0.3f
                    },
                    new ChoiceOption {
                        OptionId = "leave",
                        Text = "离开",
                        Weight = 0.2f
                    }
                }
            });

            // === 战斗类事件2 ===
            AddEvent(new ChoiceEventData {
                EventId = "bandit_leader",
                Title = "土匪头目",
                Description = "你遇到了土匪头目，他威胁要抢劫你",
                Category = "Combat",
                MinPlayerLevel = 8,
                Options = new List<ChoiceOption> {
                    new ChoiceOption {
                        OptionId = "defeat",
                        Text = "击败他",
                        ResultText = "你战胜了土匪头目，获得了丰厚的奖励！",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Gold", Amount = 200, Chance = 1.0f },
                            new RewardItem { Type = "Exp", Amount = 100, Chance = 1.0f },
                            new RewardItem { Type = "Item", Id = "rare_equipment", Amount = 1, Chance = 0.3f }
                        },
                        Weight = 0.4f
                    },
                    new ChoiceOption {
                        OptionId = "bribe",
                        Text = "贿赂",
                        RequiresGold = true,
                        GoldCost = 150,
                        ResultText = "你给了贿赂金，土匪放你走了",
                        Weight = 0.3f
                    },
                    new ChoiceOption {
                        OptionId = "trick",
                        Text = "欺骗",
                        ResultText = "你用计谋甩开了土匪",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Exp", Amount = 30, Chance = 1.0f }
                        },
                        Weight = 0.3f
                    }
                }
            });

            // === 神秘事件2 ===
            AddEvent(new ChoiceEventData {
                EventId = "dream_world",
                Title = "梦境世界",
                Description = "你突然被传送到了一个奇异的梦境世界",
                Category = "Mystery",
                MinPlayerLevel = 15,
                Options = new List<ChoiceOption> {
                    new ChoiceOption {
                        OptionId = "explore_dream",
                        Text = "探索梦境",
                        ResultText = "你在梦中获得了神秘的启示！",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Exp", Amount = 200, Chance = 1.0f },
                            new RewardItem { Type = "Buff", Id = "dream_blessing", Amount = 1, Chance = 0.5f }
                        },
                        Weight = 0.5f
                    },
                    new ChoiceOption {
                        OptionId = "wake",
                        Text = "努力醒来",
                        ResultText = "你醒来了，但感觉很疲惫",
                        Penalties = new List<PenaltyItem> {
                            new PenaltyItem { Type = "Health", Amount = 10 }
                        },
                        Weight = 0.3f
                    },
                    new ChoiceOption {
                        OptionId = "accept",
                        Text = "接受梦境",
                        ResultText = "你在梦境中获得了强大的力量",
                        Rewards = new List<RewardItem> {
                            new RewardItem { Type = "Buff", Id = "dream_power", Amount = 1, Chance = 1.0f }
                        },
                        Weight = 0.2f
                    }
                }
            });
        }

        private void AddEvent(ChoiceEventData eventData)
        {
            _events[eventData.EventId] = eventData;
        }

        /// <summary>
        /// 获取所有事件
        /// </summary>
        public Dictionary<string, ChoiceEventData> GetAllEvents()
        {
            return new Dictionary<string, ChoiceEventData>(_events);
        }

        /// <summary>
        /// 根据ID获取事件
        /// </summary>
        public ChoiceEventData GetEvent(string eventId)
        {
            if (_events.ContainsKey(eventId))
            {
                return _events[eventId];
            }
            return null;
        }

        /// <summary>
        /// 获取随机事件（基于玩家等级和区域）
        /// </summary>
        public ChoiceEventData GetRandomEvent(int playerLevel, string region = "")
        {
            var validEvents = new List<ChoiceEventData>();

            foreach (var evt in _events.Values)
            {
                if (evt.MinPlayerLevel <= playerLevel)
                {
                    if (string.IsNullOrEmpty(evt.RequiredRegion) || evt.RequiredRegion == region)
                    {
                        validEvents.Add(evt);
                    }
                }
            }

            if (validEvents.Count == 0) return null;

            // 加权随机选择
            return GetWeightedRandomEvent(validEvents);
        }

        /// <summary>
        /// 根据类别获取随机事件
        /// </summary>
        public ChoiceEventData GetRandomEventByCategory(string category, int playerLevel)
        {
            var validEvents = new List<ChoiceEventData>();

            foreach (var evt in _events.Values)
            {
                if (evt.Category == category && evt.MinPlayerLevel <= playerLevel)
                {
                    validEvents.Add(evt);
                }
            }

            if (validEvents.Count == 0) return null;

            return GetWeightedRandomEvent(validEvents);
        }

        /// <summary>
        /// 加权随机选择
        /// </summary>
        private ChoiceEventData GetWeightedRandomEvent(List<ChoiceEventData> events)
        {
            if (events.Count == 0) return null;
            if (events.Count == 1) return events[0];

            float totalWeight = 0;
            foreach (var evt in events)
            {
                totalWeight += 1.0f; // 简单等权重
            }

            float randomValue = (float)GD.RandDouble() * totalWeight;
            float currentWeight = 0;

            foreach (var evt in events)
            {
                currentWeight += 1.0f;
                if (randomValue <= currentWeight)
                {
                    return evt;
                }
            }

            return events[0];
        }
    }

    /// <summary>
    /// 玩家事件记录（用于持久化）
    /// </summary>
    public class PlayerEventRecord
    {
        public string PlayerId { get; set; }
        public int ChoicesMade { get; set; }
        public Dictionary<string, List<string>> EventChoiceHistory { get; set; }  // eventId -> chosen optionIds
        public List<string> UnlockedEvents { get; set; }

        public PlayerEventRecord()
        {
            EventChoiceHistory = new Dictionary<string, List<string>>();
            UnlockedEvents = new List<string>();
        }
    }
}
