using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Quests {
    /// <summary>
    /// NPC对话数据库
    /// </summary>
    public class DialogueDatabase {
        private static DialogueDatabase _instance;
        public static DialogueDatabase Instance {
            get {
                if (_instance == null) _instance = new DialogueDatabase();
                return _instance;
            }
        }

        private Dictionary<string, Dialogue> _dialogues;
        private Dictionary<string, List<string>> _npcDialogues;

        public DialogueDatabase() {
            _dialogues = new Dictionary<string, Dialogue>();
            _npcDialogues = new Dictionary<string, List<string>>();
            InitializeDialogues();
        }

        private void InitializeDialogues() {
            // 铁匠NPC对话
            CreateBlacksmithDialogues();
            // 商人NPC对话
            CreateMerchantDialogues();
            // 贤者NPC对话
            CreateSageDialogues();
            // 任务发布者对话
            CreateQuestGiverDialogues();
        }

        private void CreateBlacksmithDialogues() {
            var dialogue = new Dialogue {
                Id = "blacksmith_greeting",
                NpcId = "npc_blacksmith",
                NpcName = "铁匠大师",
                StartNodeId = "greeting",
                IsRepeatable = true
            };

            // 节点
            var greetingNode = new DialogueNode {
                Id = "greeting",
                SpeakerName = "铁匠大师",
                Text = "欢迎来到锻造铺！有什么武器需要打造或者修理吗？",
                NextNodeId = "offer"
            };
            greetingNode.Options.Add(new DialogueOption {
                Id = "view_weapons",
                Text = "我想看看武器",
                NextNodeId = "show_weapons"
            });
            greetingNode.Options.Add(new DialogueOption {
                Id = "repair",
                Text = "能帮我修理装备吗？",
                NextNodeId = "repair_info"
            });
            greetingNode.Options.Add(new DialogueOption {
                Id = "bye",
                Text = "我只是随便看看",
                NextNodeId = "farewell"
            });

            var showWeaponsNode = new DialogueNode {
                Id = "show_weapons",
                SpeakerName = "铁匠大师",
                Text = "这些都是我的得意之作！每一把武器都经过千锤百炼。",
                NextNodeId = "offer"
            };

            var repairInfoNode = new DialogueNode {
                Id = "repair_info",
                SpeakerName = "铁匠大师",
                Text = "修理费用取决于装备的耐久度损失。高品质的装备修理费用会更贵一些。",
                NextNodeId = "offer"
            };

            var offerNode = new DialogueNode {
                Id = "offer",
                SpeakerName = "铁匠大师",
                Text = "还需要什么帮助吗？",
                IsEndNode = false
            };
            offerNode.Options.Add(new DialogueOption {
                Id = "view_weapons_2",
                Text = "我想看看武器",
                NextNodeId = "show_weapons"
            });
            offerNode.Options.Add(new DialogueOption {
                Id = "bye_2",
                Text = "谢谢，我先离开了",
                NextNodeId = "farewell"
            });

            var farewellNode = new DialogueNode {
                Id = "farewell",
                SpeakerName = "铁匠大师",
                Text = "随时欢迎再来！锻造铺永远为勇者敞开大门。",
                IsEndNode = true
            };

            dialogue.Nodes.AddRange(new[] { greetingNode, showWeaponsNode, repairInfoNode, offerNode, farewellNode });
            AddDialogue(dialogue);
        }

        private void CreateMerchantDialogues() {
            var dialogue = new Dialogue {
                Id = "merchant_greeting",
                NpcId = "npc_merchant",
                NpcName = "商人托马",
                StartNodeId = "greeting",
                IsRepeatable = true
            };

            var greetingNode = new DialogueNode {
                Id = "greeting",
                SpeakerName = "商人托马",
                Text = "嘿！旅行者，看看我的货物吧！保证物美价廉！",
                NextNodeId = "shop"
            };
            greetingNode.Options.Add(new DialogueOption {
                Id = "view_goods",
                Text = "让我看看有什么好东西",
                NextNodeId = "show_goods"
            });
            greetingNode.Options.Add(new DialogueOption {
                Id = "sell",
                Text = "我想卖点东西",
                NextNodeId = "sell_info"
            });
            greetingNode.Options.Add(new DialogueOption {
                Id = "bye",
                Text = "下次再来",
                NextNodeId = "farewell"
            });

            var showGoodsNode = new DialogueNode {
                Id = "show_goods",
                SpeakerName = "商人托马",
                Text = "这边请！我有最新的药水、卷轴和稀有材料！",
                NextNodeId = "shop"
            };

            var sellInfoNode = new DialogueNode {
                Id = "sell_info",
                SpeakerName = "商人托马",
                Text = "当然！我收购各种战利品和材料。价格公道的很！",
                NextNodeId = "shop"
            };

            var shopNode = new DialogueNode {
                Id = "shop",
                SpeakerName = "商人托马",
                Text = "还需要什么？",
                IsEndNode = false
            };
            shopNode.Options.Add(new DialogueOption {
                Id = "view_goods_2",
                Text = "看看货物",
                NextNodeId = "show_goods"
            });
            shopNode.Options.Add(new DialogueOption {
                Id = "sell_2",
                Text = "卖点东西",
                NextNodeId = "sell_info"
            });
            shopNode.Options.Add(new DialogueOption {
                Id = "bye_2",
                Text = "再见了",
                NextNodeId = "farewell"
            });

            var farewellNode = new DialogueNode {
                Id = "farewell",
                SpeakerName = "商人托马",
                Text = "一路顺风！记得下次还来照顾我的生意！",
                IsEndNode = true
            };

            dialogue.Nodes.AddRange(new[] { greetingNode, showGoodsNode, sellInfoNode, shopNode, farewellNode });
            AddDialogue(dialogue);
        }

        private void CreateSageDialogues() {
            var dialogue = new Dialogue {
                Id = "sage_greeting",
                NpcId = "npc_sage",
                NpcName = "智慧贤者",
                StartNodeId = "greeting",
                IsRepeatable = false
            };

            var greetingNode = new DialogueNode {
                Id = "greeting",
                SpeakerName = "智慧贤者",
                Text = "年轻的勇者，我等你很久了。当你准备好接受试炼时，我会告诉你该做什么。",
                NextNodeId = "info"
            };
            greetingNode.Options.Add(new DialogueOption {
                Id = "what_trials",
                Text = "什么试炼？",
                NextNodeId = "trials_info"
            });
            greetingNode.Options.Add(new DialogueOption {
                Id = "ask_advice",
                Text = "请给我一些建议",
                NextNodeId = "advice"
            });
            greetingNode.Options.Add(new DialogueOption {
                Id = "farewell",
                Text = "我还会再来的",
                NextNodeId = "farewell_node"
            });

            var trialsInfoNode = new DialogueNode {
                Id = "trials_info",
                SpeakerName = "智慧贤者",
                Text = "这个世界的每个区域都隐藏着古老的试炼。完成它们，你将获得强大的力量和珍贵的宝藏。",
                NextNodeId = "info"
            };

            var adviceNode = new DialogueNode {
                Id = "advice",
                SpeakerName = "智慧贤者",
                Text = "记住：提升等级强化自身，学习技能增加手段，收集装备提升实力。勿急于求成，稳扎稳打方为上策。",
                NextNodeId = "info"
            };

            var infoNode = new DialogueNode {
                Id = "info",
                SpeakerName = "智慧贤者",
                Text = "你还有什么想知道的？",
                IsEndNode = false
            };
            infoNode.Options.Add(new DialogueOption {
                Id = "what_trials_2",
                Text = "关于试炼",
                NextNodeId = "trials_info"
            });
            infoNode.Options.Add(new DialogueOption {
                Id = "advice_2",
                Text = "建议",
                NextNodeId = "advice"
            });
            infoNode.Options.Add(new DialogueOption {
                Id = "farewell_2",
                Text = "告辞",
                NextNodeId = "farewell_node"
            });

            var farewellNode = new DialogueNode {
                Id = "farewell_node",
                SpeakerName = "智慧贤者",
                Text = "愿智慧之光指引你的道路，勇者。",
                IsEndNode = true,
                RewardQuestId = "quest_sage_blessing"
            };

            dialogue.Nodes.AddRange(new[] { greetingNode, trialsInfoNode, adviceNode, infoNode, farewellNode });
            AddDialogue(dialogue);
        }

        private void CreateQuestGiverDialogues() {
            // 森林任务发布者
            var forestQuest = new Dialogue {
                Id = "forest_quest_giver",
                NpcId = "npc_forest_guard",
                NpcName = "森林守卫",
                StartNodeId = "start",
                IsRepeatable = false
            };

            var startNode = new DialogueNode {
                Id = "start",
                SpeakerName = "森林守卫",
                Text = "勇者！森林里出现了大批哥布林，我们需要你的帮助！",
                NextNodeId = "details"
            };
            startNode.Options.Add(new DialogueOption {
                Id = "accept",
                Text = "我愿意帮助你们！",
                NextNodeId = "accept_quest",
                RequiredQuestState = "not_started"
            });
            startNode.Options.Add(new DialogueOption {
                Id = "details",
                Text = "告诉我更多详情",
                NextNodeId = "quest_details"
            });
            startNode.Options.Add(new DialogueOption {
                Id = "complete",
                Text = "我已经消灭了哥布林！",
                NextNodeId = "complete_quest",
                RequiredQuestId = "quest_goblin_trouble",
                RequiredQuestState = "completed"
            });

            var questDetailsNode = new DialogueNode {
                Id = "quest_details",
                SpeakerName = "森林守卫",
                Text = "森林东部的哥布林营地聚集了大量怪物。它们威胁着附近村庄的安全。请消灭20只哥布林。",
                NextNodeId = "details_ask"
            };

            var detailsAskNode = new DialogueNode {
                Id = "details_ask",
                SpeakerName = "森林守卫",
                Text = "你愿意接受这个任务吗？",
                IsEndNode = false
            };
            detailsAskNode.Options.Add(new DialogueOption {
                Id = "accept_2",
                Text = "我接受！",
                NextNodeId = "accept_quest",
                RequiredQuestState = "not_started"
            });
            detailsAskNode.Options.Add(new DialogueOption {
                Id = "later",
                Text = "让我想想",
                NextNodeId = "farewell"
            });

            var acceptQuestNode = new DialogueNode {
                Id = "accept_quest",
                SpeakerName = "森林守卫",
                Text = "太好了！这是你的任务凭证。消灭哥布林后回来找我领取奖励！",
                IsEndNode = true,
                RewardQuestId = "quest_goblin_trouble",
                RewardGold = 100,
                TriggerEvent = "quest_accepted"
            };

            var completeQuestNode = new DialogueNode {
                Id = "complete_quest",
                SpeakerName = "森林守卫",
                Text = "太感谢你了！森林终于恢复了平静。这是你应得的奖励！",
                IsEndNode = true,
                RewardGold = 200,
                RewardItemId = "item_health_potion_large",
                TriggerEvent = "quest_completed"
            };

            var farewellNode = new DialogueNode {
                Id = "farewell",
                SpeakerName = "森林守卫",
                Text = "随时可以来找我接受任务。",
                IsEndNode = true
            };

            forestQuest.Nodes.AddRange(new[] { startNode, questDetailsNode, detailsAskNode, acceptQuestNode, completeQuestNode, farewellNode });
            AddDialogue(forestQuest);

            // 洞穴任务发布者
            var caveQuest = new Dialogue {
                Id = "cave_quest_giver",
                NpcId = "npc_cave_explorer",
                NpcName = "洞穴探险家",
                StartNodeId = "start",
                IsRepeatable = false
            };

            var caveStartNode = new DialogueNode {
                Id = "start",
                SpeakerName = "洞穴探险家",
                Text = "我发现了幽暗洞穴的入口，但里面太危险了。我需要有人帮我找到里面的宝藏。",
                NextNodeId = "cave_ask"
            };
            caveStartNode.Options.Add(new DialogueOption {
                Id = "cave_accept",
                Text = "让我去探索！",
                NextNodeId = "cave_accept_quest",
                RequiredLevel = 3
            });
            caveStartNode.Options.Add(new DialogueOption {
                Id = "cave_info",
                Text = "洞穴里有什么？",
                NextNodeId = "cave_info_node"
            });

            var caveInfoNode = new DialogueNode {
                Id = "cave_info_node",
                SpeakerName = "洞穴探险家",
                Text = "根据我的调查，洞穴里有各种怪物，还有几率掉落稀有装备。但是需要一定的实力才能安全探索。",
                NextNodeId = "cave_ask"
            };

            var caveAskNode = new DialogueNode {
                Id = "cave_ask",
                SpeakerName = "洞穴探险家",
                Text = "你愿意接受这个探索任务吗？",
                IsEndNode = false
            };
            caveAskNode.Options.Add(new DialogueOption {
                Id = "cave_accept_2",
                Text = "我接受！",
                NextNodeId = "cave_accept_quest",
                RequiredLevel = 3
            });
            caveAskNode.Options.Add(new DialogueOption {
                Id = "cave_later",
                Text = "以后再说",
                NextNodeId = "cave_farewell"
            });

            var caveAcceptNode = new DialogueNode {
                Id = "cave_accept_quest",
                SpeakerName = "洞穴探险家",
                Text = "祝你平安归来！记得带回尽可能多的宝物！",
                IsEndNode = true,
                RewardQuestId = "quest_cave_exploration",
                RewardGold = 150,
                TriggerEvent = "quest_accepted"
            };

            var caveFarewellNode = new DialogueNode {
                Id = "cave_farewell",
                SpeakerName = "洞穴探险家",
                Text = "好吧，等你准备好了再来。",
                IsEndNode = true
            };

            caveQuest.Nodes.AddRange(new[] { caveStartNode, caveInfoNode, caveAskNode, caveAcceptNode, caveFarewellNode });
            AddDialogue(caveQuest);
        }

        public void AddDialogue(Dialogue dialogue) {
            _dialogues[dialogue.Id] = dialogue;
            
            if (!_npcDialogues.ContainsKey(dialogue.NpcId)) {
                _npcDialogues[dialogue.NpcId] = new List<string>();
            }
            _npcDialogues[dialogue.NpcId].Add(dialogue.Id);
        }

        public Dialogue GetDialogue(string dialogueId) {
            if (_dialogues.ContainsKey(dialogueId)) {
                return _dialogues[dialogueId];
            }
            return null;
        }

        public Dialogue GetDialogueByNpc(string npcId) {
            if (_npcDialogues.ContainsKey(npcId) && _npcDialogues[npcId].Count > 0) {
                return GetDialogue(_npcDialogues[npcId][0]);
            }
            return null;
        }

        public DialogueNode GetStartNode(string dialogueId) {
            var dialogue = GetDialogue(dialogueId);
            if (dialogue == null) return null;
            
            foreach (var node in dialogue.Nodes) {
                if (node.Id == dialogue.StartNodeId) {
                    return node;
                }
            }
            return null;
        }

        public DialogueNode GetNode(string dialogueId, string nodeId) {
            var dialogue = GetDialogue(dialogueId);
            if (dialogue == null) return null;
            
            foreach (var node in dialogue.Nodes) {
                if (node.Id == nodeId) {
                    return node;
                }
            }
            return null;
        }

        public List<string> GetDialogueIdsForNpc(string npcId) {
            if (_npcDialogues.ContainsKey(npcId)) {
                return _npcDialogues[npcId];
            }
            return new List<string>();
        }
    }
}
