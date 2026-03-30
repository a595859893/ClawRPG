using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Systems.Dialogue;

public partial class DialogueSystem : BaseSystem {
    public static DialogueSystem Instance { get; private set; }
    
    // All dialogue trees indexed by NPC ID
    private Dictionary<string, DialogueTree> _dialogueTrees = new();
    private DialogueTree _currentTree;
    private DialogueNode _currentNode;
    private bool _isActive;
    
    // Signals
public delegate void DialogueStarted(string npcId);
public delegate void DialogueEnded();
public delegate void NodeChanged(DialogueNode node);
public delegate void ChoiceMade(DialogueChoice choice);
    
    public override void _Ready() {
        Instance = this;
        LoadDialogueTrees();
    }
    
    public bool IsActive => _isActive;
    public DialogueTree CurrentTree => _currentTree;
    public DialogueNode CurrentNode => _currentNode;
    
    /// <summary>
    /// Load all dialogue trees from resources.
    /// </summary>
    private void LoadDialogueTrees() {
        // Load dialogue data from JSON files or create default dialogues
        // For now, create some sample dialogues
        CreateSampleDialogues();
    }
    
    /// <summary>
    /// Start a dialogue with an NPC.
    /// </summary>
    public void StartDialogue(string npcId) {
        if (!_dialogueTrees.ContainsKey(npcId)) {
            GD.Print($"[DialogueSystem] No dialogue found for NPC: {npcId}");
            return;
        }
        
        _currentTree = _dialogueTrees[npcId];
        _currentNode = _currentTree.GetStartNode();
        _isActive = true;
        
        DialogueStarted?.Invoke(npcId);
        
        if (_currentNode != null) {
            ExecuteNodeAction(_currentNode);
            NodeChanged?.Invoke(_currentNode);
        }
    }
    
    /// <summary>
    /// Advance to the next node in linear dialogue.
    /// </summary>
    public void AdvanceDialogue() {
        if (_currentNode == null || !_isActive) return;
        
        if (_currentNode.HasChoices) {
            // Wait for player to make a choice
            return;
        }
        
        if (!string.IsNullOrEmpty(_currentNode.NextNodeId)) {
            _currentNode = _currentTree.GetNode(_currentNode.NextNodeId);
            if (_currentNode != null) {
                ExecuteNodeAction(_currentNode);
                NodeChanged?.Invoke(_currentNode);
                
                if (_currentNode.IsEndNode) {
                    EndDialogue();
                }
            }
        } else {
            EndDialogue();
        }
    }
    
    /// <summary>
    /// Make a choice in the dialogue.
    /// </summary>
    public void MakeChoice(DialogueChoice choice) {
        if (_currentNode == null || !_isActive) return;
        
        ChoiceMade?.Invoke(choice);
        
        // Execute choice action
        if (choice.Action != null) {
            ExecuteAction(choice.Action);
        }
        
        // Move to next node
        if (!string.IsNullOrEmpty(choice.NextNodeId)) {
            _currentNode = _currentTree.GetNode(choice.NextNodeId);
            if (_currentNode != null) {
                ExecuteNodeAction(_currentNode);
                NodeChanged?.Invoke(_currentNode);
                
                if (_currentNode.IsEndNode) {
                    EndDialogue();
                }
            }
        } else {
            EndDialogue();
        }
    }
    
    /// <summary>
    /// Make a choice by index.
    /// </summary>
    public void MakeChoiceByIndex(int index) {
        if (_currentNode == null || !_currentNode.HasChoices) return;
        if (index < 0 || index >= _currentNode.Choices.Count) return;
        
        var choice = _currentNode.Choices[index];
        if (CanSelectChoice(choice)) {
            MakeChoice(choice);
        }
    }
    
    /// <summary>
    /// Check if a choice can be selected.
    /// </summary>
    public bool CanSelectChoice(DialogueChoice choice) {
        // Check item requirement
        if (!string.IsNullOrEmpty(choice.RequiredItem)) {
            if (!PlayerInventory.Instance.HasItem(choice.RequiredItem)) {
                return false;
            }
        }
        
        // Check gold requirement
        if (choice.RequiredGold > 0) {
            if (PlayerStats.Instance.Gold < choice.RequiredGold) {
                return false;
            }
        }
        
        // Check quest requirement
        if (!string.IsNullOrEmpty(choice.RequiredQuest)) {
            var quest = QuestSystem.Instance.GetQuest(choice.RequiredQuest);
            if (quest == null) return false;
            
            if (!string.IsNullOrEmpty(choice.RequiredQuestState)) {
                if (quest.State.ToString() != choice.RequiredQuestState) {
                    return false;
                }
            }
        }
        
        // Check condition
        if (choice.Condition != null) {
            return EvaluateCondition(choice.Condition);
        }
        
        return true;
    }
    
    /// <summary>
    /// Get available choices for current node.
    /// </summary>
    public List<DialogueChoice> GetAvailableChoices() {
        if (_currentNode == null || !_currentNode.HasChoices) {
            return new List<DialogueChoice>();
        }
        
        var available = new List<DialogueChoice>();
        foreach (var choice in _currentNode.Choices) {
            // Check condition
            if (choice.Condition != null) {
                if (!EvaluateCondition(choice.Condition)) {
                    continue;
                }
            }
            available.Add(choice);
        }
        return available;
    }
    
    /// <summary>
    /// End the current dialogue.
    /// </summary>
    public void EndDialogue() {
        _isActive = false; 
        _currentTree = null;
        _currentNode = null;
        DialogueEnded?.Invoke();
    }
    
    /// <summary>
    /// Register a dialogue tree.
    /// </summary>
    public void RegisterDialogueTree(DialogueTree tree) {
        _dialogueTrees[tree.NpcId] = tree;
    }
    
    /// <summary>
    /// Get dialogue tree for an NPC.
    /// </summary>
    public DialogueTree GetDialogueTree(string npcId) {
        return _dialogueTrees.ContainsKey(npcId) ? _dialogueTrees[npcId] : null;
    }
    
    /// <summary>
    /// Execute action when entering a node.
    /// </summary>
    private void ExecuteNodeAction(DialogueNode node) {
        if (node.Action != null) {
            ExecuteAction(node.Action);
        }
    }
    
    /// <summary>
    /// Execute a dialogue action.
    /// </summary>
    private void ExecuteAction(DialogueAction action) {
        switch (action.Type) {
            case DialogueAction.ActionType.AddItem:
                PlayerInventory.Instance.AddItem(action.Value, action.Amount);
                break;
            case DialogueAction.ActionType.RemoveItem:
                PlayerInventory.Instance.RemoveItem(action.Value, action.Amount);
                break;
            case DialogueAction.ActionType.AddGold:
                PlayerStats.Instance.AddGold(action.Amount);
                break;
            case DialogueAction.ActionType.RemoveGold:
                PlayerStats.Instance.RemoveGold(action.Amount);
                break;
            case DialogueAction.ActionType.StartQuest:
                QuestSystem.Instance.StartQuest(action.Value);
                break;
            case DialogueAction.ActionType.CompleteQuest:
                QuestSystem.Instance.CompleteQuest(action.Value);
                break;
            case DialogueAction.ActionType.AddReputation:
                ReputationSystem.Instance.AddReputation(action.Value, action.Amount);
                break;
            case DialogueAction.ActionType.RemoveReputation:
                ReputationSystem.Instance.AddReputation(action.Value, -action.Amount);
                break;
            case DialogueAction.ActionType.HealPlayer:
                PlayerStats.Instance.Heal(PlayerStats.Instance.MaxHealth);
                break;
            case DialogueAction.ActionType.UnlockAchievement:
                AchievementManager.Instance.UnlockAchievement(action.Value);
                break;
            case DialogueAction.ActionType.SetFlag:
                if (_currentTree != null) {
                    var parts = action.Value.Split(':');
                    if (parts.Length == 2) {
                        _currentTree.SetFlag(parts[0], parts[1]);
                    }
                }
                break;
        }
    }
    
    /// <summary>
    /// Evaluate a dialogue condition.
    /// </summary>
    private bool EvaluateCondition(DialogueCondition condition) {
        if (condition == null || condition.Type == DialogueCondition.ConditionType.None) {
            return true;
        }
        
        switch (condition.Type) {
            case DialogueCondition.ConditionType.HasItem:
                return PlayerInventory.Instance.HasItem(condition.Value);
            case DialogueCondition.ConditionType.HasGold:
                return CompareValues(PlayerStats.Instance.Gold, condition.Amount, condition.Comparison);
            case DialogueCondition.ConditionType.QuestState:
                var quest = QuestSystem.Instance.GetQuest(condition.Value);
                return quest != null && quest.State.ToString() == condition.Comparison;
            case DialogueCondition.ConditionType.QuestCompleted:
                return QuestSystem.Instance.IsQuestCompleted(condition.Value);
            case DialogueCondition.ConditionType.Level:
                return CompareValues(PlayerStats.Instance.Level, condition.Amount, condition.Comparison);
            case DialogueCondition.ConditionType.TimeOfDay:
                // Check current time
                return true; // Simplified
            case DialogueCondition.ConditionType.DayCount:
                return CompareValues(1, condition.Amount, condition.Comparison); // Simplified
            default:
                return true;
        }
    }
    
    private bool CompareValues(int actual, int expected, string comparison) {
        return comparison switch {
            "==" => actual == expected,
            "!=" => actual != expected,
            ">" => actual > expected,
            "<" => actual < expected,
            ">=" => actual >= expected,
            "<=" => actual <= expected,
            _ => actual == expected
        };
    }
    
    /// <summary>
    /// Create sample dialogue trees.
    /// </summary>
    private void CreateSampleDialogues() {
        // Create Village Elder dialogue
        var elderTree = new DialogueTree {
            Id = "village_elder",
            NpcId = "village_elder",
            StartNodeId = "greeting"
        };
        
        elderTree.Nodes["greeting"] = new DialogueNode {
            Id = "greeting",
            Text = "Welcome, brave adventurer! Welcome to our humble village.",
            Speaker = "Village Elder",
            NextNodeId = "ask_help"
        };
        
        elderTree.Nodes["ask_help"] = new DialogueNode {
            Id = "ask_help",
            Text = "Our village has been plagued by monsters from the dark forest. Could you help us?",
            Speaker = "Village Elder",
            Choices = new List<DialogueChoice> {
                new DialogueChoice {
                    Id = "accept",
                    Text = "I'll help you!",
                    NextNodeId = "accept_quest",
                    Action = new DialogueAction {
                        Type = DialogueAction.ActionType.StartQuest,
                        Value = "village_monsters"
                    }
                },
                new DialogueChoice {
                    Id = "need_info",
                    Text = "Tell me more about the monsters.",
                    NextNodeId = "monster_info"
                },
                new DialogueChoice {
                    Id = "later",
                    Text = "Maybe later.",
                    NextNodeId = "goodbye"
                }
            }
        };
        
        elderTree.Nodes["monster_info"] = new DialogueNode {
            Id = "monster_info",
            Text = "Terrible wolves and goblins have been attacking our livestock. They come from the forest at night.",
            Speaker = "Village Elder",
            NextNodeId = "ask_help"
        };
        
        elderTree.Nodes["accept_quest"] = new DialogueNode {
            Id = "accept_quest",
            Text = "Thank you, brave hero! May the light be with you!",
            Speaker = "Village Elder",
            NextNodeId = "goodbye"
        };
        
        elderTree.Nodes["goodbye"] = new DialogueNode {
            Id = "goodbye",
            Text = "Safe travels, adventurer!",
            Speaker = "Village Elder"
        };
        
        RegisterDialogueTree(elderTree);
        
        // Create Merchant dialogue
        var merchantTree = new DialogueTree {
            Id = "village_merchant",
            NpcId = "village_merchant",
            StartNodeId = "greeting"
        };
        
        merchantTree.Nodes["greeting"] = new DialogueNode {
            Id = "greeting",
            Text = "Welcome! Look at my wares. Gold is welcome here!",
            Speaker = "Merchant",
            Choices = new List<DialogueChoice> {
                new DialogueChoice {
                    Id = "buy",
                    Text = "I want to buy something.",
                    NextNodeId = "shop",
                    Action = new DialogueAction {
                        Type = DialogueAction.ActionType.OpenShop,
                        Value = "general_shop"
                    }
                },
                new DialogueChoice {
                    Id = "sell",
                    Text = "I have items to sell.",
                    NextNodeId = "sell"
                },
                new DialogueChoice {
                    Id = "bye",
                    Text = "Goodbye.",
                    NextNodeId = "goodbye"
                }
            }
        };
        
        merchantTree.Nodes["shop"] = new DialogueNode {
            Id = "shop",
            Text = "Take your time!",
            Speaker = "Merchant",
            NextNodeId = "goodbye"
        };
        
        merchantTree.Nodes["sell"] = new DialogueNode {
            Id = "sell",
            Text = "Let's see what you have.",
            Speaker = "Merchant",
            NextNodeId = "goodbye"
        };
        
        merchantTree.Nodes["goodbye"] = new DialogueNode {
            Id = "goodbye",
            Text = "Come back anytime!",
            Speaker = "Merchant"
        };
        
        RegisterDialogueTree(merchantTree);
        
        // Create Quest Giver dialogue
        var questGiverTree = new DialogueTree {
            Id = "quest_giver",
            NpcId = "quest_giver",
            StartNodeId = "check_quest"
        };
        
        questGiverTree.Nodes["check_quest"] = new DialogueNode {
            Id = "check_quest",
            Text = "Ah, it's you again!",
            Speaker = "Quest Giver",
            NextNodeId = "quest_status"  // This would dynamically check quest state
        };
        
        questGiverTree.Nodes["quest_status"] = new DialogueNode {
            Id = "quest_status",
            Text = "Have you completed the task?",
            Speaker = "Quest Giver",
            Choices = new List<DialogueChoice> {
                new DialogueChoice {
                    Id = "complete",
                    Text = "Yes, I've completed it!",
                    NextNodeId = "complete_quest",
                    RequiredQuest = "village_monsters",
                    RequiredQuestState = "Completed"
                },
                new DialogueChoice {
                    Id = "not_done",
                    Text = "Not yet.",
                    NextNodeId = "encourage"
                }
            }
        };
        
        questGiverTree.Nodes["complete_quest"] = new DialogueNode {
            Id = "complete_quest",
            Text = "Incredible! Here's your reward.",
            Speaker = "Quest Giver",
            Action = new DialogueAction {
                Type = DialogueAction.ActionType.CompleteQuest,
                Value = "village_monsters"
            },
            Action = new DialogueAction {
                Type = DialogueAction.ActionType.AddGold,
                Amount = 100
            },
            NextNodeId = "thanks"
        };
        
        questGiverTree.Nodes["encourage"] = new DialogueNode {
            Id = "encourage",
            Text = "Please hurry! We need your help!",
            Speaker = "Quest Giver",
            NextNodeId = "goodbye"
        };
        
        questGiverTree.Nodes["thanks"] = new DialogueNode {
            Id = "thanks",
            Text = "You're a true hero!",
            Speaker = "Quest Giver",
            NextNodeId = "goodbye"
        };
        
        questGiverTree.Nodes["goodbye"] = new DialogueNode {
            Id = "goodbye",
            Text = "Until next time!",
            Speaker = "Quest Giver"
        };
        
        RegisterDialogueTree(questGiverTree);
        
        GD.Print($"[DialogueSystem] Loaded {_dialogueTrees.Count} dialogue trees");
    }

    // ===== 持久化方法 =====

    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        // 对话树通常从数据库加载，不需要保存
        // 只保存当前对话状态
        data["is_active"] = _isActive;
        
        if (_currentTree != null)
        {
            data["current_tree_id"] = _currentTree.Id;
        }
        
        if (_currentNode != null)
        {
            data["current_node_id"] = _currentNode.Id;
        }
        
        return data;
    }

    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        _isActive = (bool)(data.GetValueOrDefault("is_active", false));
        
        // 恢复对话树和节点
        if (data.Contains("current_tree_id"))
        {
            var treeId = data["current_tree_id"].ToString();
            if (_dialogueTrees.ContainsKey(treeId))
            {
                _currentTree = _dialogueTrees[treeId];
            }
        }
        
        if (data.Contains("current_node_id") && _currentTree != null)
        {
            var nodeId = data["current_node_id"].ToString();
            if (_currentTree.Nodes.ContainsKey(nodeId))
            {
                _currentNode = _currentTree.Nodes[nodeId];
            }
        }
    }
}
