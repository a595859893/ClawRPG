using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Quests {
    /// <summary>
    /// 对话选项数据
    /// </summary>
    public class DialogueOption {
        public string Id { get; set; }
        public string Text { get; set; }
        public string NextNodeId { get; set; }
        public string RequiredQuestId { get; set; }
        public string RequiredQuestState { get; set; }
        public int RequiredLevel { get; set; }
        public string RewardItemId { get; set; }
        public int RewardGold { get; set; }
        public string RewardQuestId { get; set; }
        public string TriggerEvent { get; set; }
    }

    /// <summary>
    /// 对话节点数据
    /// </summary>
    public class DialogueNode {
        public string Id { get; set; }
        public string SpeakerName { get; set; }
        public string SpeakerPortrait { get; set; }
        public string Text { get; set; }
        public List<DialogueOption> Options { get; set; }
        public string NextNodeId { get; set; }
        public bool IsEndNode { get; set; }
        public string RequiredQuestId { get; set; }
        public string RequiredQuestState { get; set; }
        public int RequiredLevel { get; set; }

        public DialogueNode() {
            Options = new List<DialogueOption>();
        }
    }

    /// <summary>
    /// 对话数据
    /// </summary>
    public class Dialogue {
        public string Id { get; set; }
        public string NpcId { get; set; }
        public string NpcName { get; set; }
        public List<DialogueNode> Nodes { get; set; }
        public string StartNodeId { get; set; }
        public bool IsRepeatable { get; set; }

        public Dialogue() {
            Nodes = new List<DialogueNode>();
        }
    }
}
