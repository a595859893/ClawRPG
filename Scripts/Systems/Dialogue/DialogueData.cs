using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Dialogue {
    /// <summary>
    /// Represents a single dialogue node in a dialogue tree.
    /// </summary>
    public class DialogueNode {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Speaker { get; set; }
        public string SpeakerPortrait { get; set; }
        public List<DialogueChoice> Choices { get; set; }
        public string NextNodeId { get; set; }  // For linear dialogue
        public DialogueAction Action { get; set; }  // Triggered when node is shown
        public DialogueCondition Condition { get; set; }  // Show this node only if condition met
        public string Animation { get; set; }  // Speaker animation
        public float TextSpeed { get; set; } = 0.05f;  // Text reveal speed
        
        public DialogueNode() {
            Choices = new List<DialogueChoice>();
        }
        
        public bool HasChoices => Choices != null && Choices.Count > 0;
        public bool IsEndNode => string.IsNullOrEmpty(NextNodeId) && !HasChoices;
    }
    
    /// <summary>
    /// Represents a choice the player can make in a dialogue.
    /// </summary>
    public class DialogueChoice {
        public string Id { get; set; }
        public string Text { get; set; }
        public string NextNodeId { get; set; }
        public DialogueCondition Condition { get; set; }  // Show choice only if condition met
        public DialogueAction Action { get; set; }  // Triggered when chosen
        public string RequiredItem { get; set; }  // Require item to choose
        public int RequiredGold { get; set; }  // Require gold to choose
        public string RequiredQuest { get; set; }  // Require quest to choose
        public string RequiredQuestState { get; set; }  // Require quest in specific state
    }
    
    /// <summary>
    /// Action to trigger when a dialogue node is reached or choice is made.
    /// </summary>
    public class DialogueAction {
        public enum ActionType {
            None,
            StartQuest,
            CompleteQuest,
            AddItem,
            RemoveItem,
            AddGold,
            RemoveGold,
            AddReputation,
            RemoveReputation,
            TriggerEvent,
            OpenShop,
            HealPlayer,
            Teleport,
            ChangeRelationship,
            SetFlag,
            UnlockAchievement
        }
        
        public ActionType Type { get; set; }
        public string Value { get; set; }
        public int Amount { get; set; }
    }
    
    /// <summary>
    /// Condition for showing a dialogue node or choice.
    /// </summary>
    public class DialogueCondition {
        public enum ConditionType {
            None,
            HasItem,
            HasGold,
            QuestState,
            QuestNotStarted,
            QuestCompleted,
            RelationshipLevel,
            Level,
            HasFlag,
            TimeOfDay,
            DayCount
        }
        
        public ConditionType Type { get; set; }
        public string Value { get; set; }
        public int Amount { get; set; }
        public string Comparison { get; set; }  // "==", "!=", ">", "<", ">=", "<="
    }
    
    /// <summary>
    /// Manages a complete dialogue tree for an NPC.
    /// </summary>
    public class DialogueTree {
        public string Id { get; set; }
        public string NpcId { get; set; }
        public string StartNodeId { get; set; }
        public Dictionary<string, DialogueNode> Nodes { get; set; }
        public Dictionary<string, string> Flags { get; set; }  // Local dialogue flags
        
        public DialogueTree() {
            Nodes = new Dictionary<string, DialogueNode>();
            Flags = new Dictionary<string, string>();
        }
        
        public DialogueNode GetNode(string nodeId) {
            if (Nodes.ContainsKey(nodeId)) {
                return Nodes[nodeId];
            }
            return null;
        }
        
        public DialogueNode GetStartNode() {
            return GetNode(StartNodeId);
        }
        
        public void SetFlag(string key, string value) {
            Flags[key] = value;
        }
        
        public string GetFlag(string key, string defaultValue = "") {
            return Flags.ContainsKey(key) ? Flags[key] : defaultValue;
        }
    }
}
