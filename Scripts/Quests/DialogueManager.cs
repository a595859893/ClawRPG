using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Items;

namespace ClawRPG.Scripts.Quests {
    /// <summary>
    /// 对话管理器 - 处理对话流程
    /// </summary>
    public class DialogueManager {
        private static DialogueManager _instance;
        public static DialogueManager Instance {
            get {
                if (_instance == null) _instance = new DialogueManager();
                return _instance;
            }
        }

        // 信号系统
        public Signal0 DialogueStarted { get; }
        public Signal0 DialogueEnded { get; }
        public Signal1<string> NodeChanged { get; }
        public Signal1<DialogueOption> OptionSelected { get; }
        public Signal1<DialogueReward> RewardGranted { get; }

        private Dialogue _currentDialogue;
        private DialogueNode _currentNode;
        private string _currentNpcId;
        private bool _isInDialogue;
        private HashSet<string> _completedDialogues;

        public Dialogue CurrentDialogue => _currentDialogue;
        public DialogueNode CurrentNode => _currentNode;
        public string CurrentNpcId => _currentNpcId;
        public bool IsInDialogue => _isInDialogue;

        public DialogueManager() {
            DialogueStarted = new Signal0();
            DialogueEnded = new Signal0();
            NodeChanged = new Signal1<string>();
            OptionSelected = new Signal1<DialogueOption>();
            RewardGranted = new Signal1<DialogueReward>();
            _completedDialogues = new HashSet<string>();
        }

        /// <summary>
        /// 开始与NPC的对话
        /// </summary>
        public bool StartDialogue(string npcId) {
            var dialogue = DialogueDatabase.Instance.GetDialogueByNpc(npcId);
            if (dialogue == null) {
                GD.Print($"[DialogueManager] No dialogue found for NPC: {npcId}");
                return false;
            }

            // 检查是否已完成且不可重复
            if (_completedDialogues.Contains(dialogue.Id) && !dialogue.IsRepeatable) {
                GD.Print($"[DialogueManager] Dialogue {dialogue.Id} is not repeatable");
                return false;
            }

            _currentDialogue = dialogue;
            _currentNpcId = npcId;
            _currentNode = DialogueDatabase.Instance.GetStartNode(dialogue.Id);
            
            // 检查节点解锁条件
            if (!CheckNodeUnlock(_currentNode)) {
                // 找到第一个解锁的节点
                _currentNode = FindFirstUnlockedNode(dialogue);
            }

            _isInDialogue = true;
            DialogueStarted.Call();
            NodeChanged.Call(_currentNode.Id);
            
            GD.Print($"[DialogueManager] Started dialogue: {dialogue.Id} with NPC: {npcId}");
            return true;
        }

        /// <summary>
        /// 选择对话选项
        /// </summary>
        public void SelectOption(DialogueOption option) {
            if (_currentDialogue == null || _currentNode == null) return;

            OptionSelected.Call(option);

            // 发放奖励
            if (option.RewardGold > 0 || !string.IsNullOrEmpty(option.RewardItemId)) {
                var reward = new DialogueReward {
                    Gold = option.RewardGold,
                    ItemId = option.RewardItemId,
                    QuestId = option.RewardQuestId
                };
                GrantReward(reward);
            }

            // 触发事件
            if (!string.IsNullOrEmpty(option.TriggerEvent)) {
                TriggerDialogueEvent(option.TriggerEvent);
            }

            // 跳转到下一个节点
            if (!string.IsNullOrEmpty(option.NextNodeId)) {
                var nextNode = DialogueDatabase.Instance.GetNode(_currentDialogue.Id, option.NextNodeId);
                if (nextNode != null) {
                    if (CheckNodeUnlock(nextNode)) {
                        _currentNode = nextNode;
                        NodeChanged.Call(_currentNode.Id);
                        
                        if (_currentNode.IsEndNode) {
                            EndDialogue();
                        }
                    } else {
                        // 节点未解锁，尝试找到下一个可用的节点
                        var availableNode = FindNextAvailableNode(nextNode);
                        if (availableNode != null) {
                            _currentNode = availableNode;
                            NodeChanged.Call(_currentNode.Id);
                            
                            if (_currentNode.IsEndNode) {
                                EndDialogue();
                            }
                        } else {
                            EndDialogue();
                        }
                    }
                } else {
                    EndDialogue();
                }
            } else {
                EndDialogue();
            }
        }

        /// <summary>
        /// 继续到下一节点（无选项时）
        /// </summary>
        public void Continue() {
            if (_currentDialogue == null || _currentNode == null) return;

            if (!string.IsNullOrEmpty(_currentNode.NextNodeId)) {
                var nextNode = DialogueDatabase.Instance.GetNode(_currentDialogue.Id, _currentNode.NextNodeId);
                if (nextNode != null && CheckNodeUnlock(nextNode)) {
                    _currentNode = nextNode;
                    NodeChanged.Call(_currentNode.Id);
                    
                    if (_currentNode.IsEndNode) {
                        EndDialogue();
                    }
                } else {
                    EndDialogue();
                }
            } else if (!_currentNode.IsEndNode) {
                // 如果没有下一个节点且不是结束节点，检查是否有可用选项
                if (_currentNode.Options.Count == 0) {
                    EndDialogue();
                }
            }
        }

        /// <summary>
        /// 结束对话
        /// </summary>
        public void EndDialogue() {
            if (_currentDialogue != null && _currentNode != null && _currentNode.IsEndNode) {
                _completedDialogues.Add(_currentDialogue.Id);
            }

            _isInDialogue = false; 
            _currentDialogue = null;
            _currentNode = null;
            _currentNpcId = null;
            
            DialogueEnded.Call();
            GD.Print("[DialogueManager] Dialogue ended");
        }

        /// <summary>
        /// 检查节点是否解锁
        /// </summary>
        private bool CheckNodeUnlock(DialogueNode node) {
            if (node == null) return false;

            // 检查任务条件
            if (!string.IsNullOrEmpty(node.RequiredQuestId)) {
                var questState = GetQuestState(node.RequiredQuestId);
                if (questState != node.RequiredQuestState) {
                    return false;
                }
            }

            // 检查等级条件
            if (node.RequiredLevel > 0) {
                var playerLevel = GetPlayerLevel();
                if (playerLevel < node.RequiredLevel) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 找到第一个解锁的节点
        /// </summary>
        private DialogueNode FindFirstUnlockedNode(Dialogue dialogue) {
            if (dialogue == null) return null;

            foreach (var node in dialogue.Nodes) {
                if (CheckNodeUnlock(node)) {
                    return node;
                }
            }
            return null;
        }

        /// <summary>
        /// 找到下一个可用的节点
        /// </summary>
        private DialogueNode FindNextAvailableNode(DialogueNode fromNode) {
            if (_currentDialogue == null || fromNode == null) return null;

            // 递归查找
            foreach (var node in _currentDialogue.Nodes) {
                if (CheckNodeUnlock(node)) {
                    return node;
                }
            }
            return null;
        }

        /// <summary>
        /// 发放对话奖励
        /// </summary>
        private void GrantReward(DialogueReward reward) {
            // 添加金币
            if (reward.Gold > 0) {
                var player = GetPlayer();
                if (player != null) {
                    player.AddGold(reward.Gold);
                }
            }

            // 添加物品
            if (!string.IsNullOrEmpty(reward.ItemId)) {
                var itemSystem = GetItemSystem();
                if (itemSystem != null) {
                    itemSystem.AddItem(reward.ItemId, 1);
                }
            }

            // 接受任务
            if (!string.IsNullOrEmpty(reward.QuestId)) {
                var questSystem = GetQuestSystem();
                if (questSystem != null) {
                    questSystem.AcceptQuest(reward.QuestId);
                }
            }

            RewardGranted.Call(reward);
        }

        /// <summary>
        /// 触发对话事件
        /// </summary>
        private void TriggerDialogueEvent(string eventName) {
            GD.Print($"[DialogueManager] Dialogue event triggered: {eventName}");
            
            switch (eventName) {
                case "quest_accepted":
                    // 可以在这里添加额外的处理
                    break;
                case "quest_completed":
                    // 任务完成的额外处理
                    break;
                case "shop_open":
                    // 打开商店UI
                    break;
                case "teleport":
                    // 传送到特定位置
                    break;
            }
        }

        // 玩家引用（通过 Initialize 设置）
        private Node _playerNode;
        
        /// <summary>
        /// 初始化对话框管理器（由 Main 调用）
        /// </summary>
        public void Initialize(Node playerNode) {
            _playerNode = playerNode;
        }
        
        // 获取玩家相关数据的接口方法
        private Node GetPlayer() {
            if (_playerNode != null) return _playerNode;
            
            // 尝试从场景树获取玩家节点
            var tree = Engine.GetMainLoop();
            if (tree is SceneTree sceneTree) {
                return sceneTree.GetFirstNodeInGroup("player");
            }
            return null;
        }

        private int GetPlayerLevel() {
            var player = GetPlayer();
            if (player != null && player.HasMethod("GetLevel")) {
                return (int)player.Call("GetLevel");
            }
            return 1;
        }

        private string GetQuestState(string questId) {
            var questSystem = QuestSystem.Instance;
            if (questSystem != null && questSystem.HasMethod("GetQuestState")) {
                return (string)questSystem.Call("GetQuestState", questId);
            }
            return "not_started";
        }

        private Node GetQuestSystem() {
            return QuestSystem.Instance;
        }

        private Node GetItemSystem() {
            return InventoryManager.Instance;
        }

        /// <summary>
        /// 检查对话选项是否可用
        /// </summary>
        public bool IsOptionAvailable(DialogueOption option) {
            if (option == null) return false;

            // 检查任务条件
            if (!string.IsNullOrEmpty(option.RequiredQuestId)) {
                var questState = GetQuestState(option.RequiredQuestId);
                if (questState != option.RequiredQuestState) {
                    return false;
                }
            }

            // 检查等级条件
            if (option.RequiredLevel > 0) {
                var playerLevel = GetPlayerLevel();
                if (playerLevel < option.RequiredLevel) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 重置对话状态（用于新游戏）
        /// </summary>
        public void Reset() {
            _completedDialogues.Clear();
            _isInDialogue = false; 
            _currentDialogue = null;
            _currentNode = null;
            _currentNpcId = null;
        }

        /// <summary>
        /// 获取可用选项列表
        /// </summary>
        public List<DialogueOption> GetAvailableOptions() {
            var availableOptions = new List<DialogueOption>();
            
            if (_currentNode == null || _currentNode.Options == null) {
                return availableOptions;
            }

            foreach (var option in _currentNode.Options) {
                if (IsOptionAvailable(option)) {
                    availableOptions.Add(option);
                }
            }

            return availableOptions;
        }
    }

    /// <summary>
    /// 对话奖励数据结构
    /// </summary>
    public class DialogueReward {
        public int Gold { get; set; }
        public string ItemId { get; set; }
        public string QuestId { get; set; }
    }
}
