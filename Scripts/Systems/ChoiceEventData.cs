using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 事件选择数据
    /// </summary>
    [System.Serializable]
    public class ChoiceEventData {
        public string EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } // Combat/Exploration/Mystery/Blessing/Curse
        public List<ChoiceOption> Options { get; set; }
        public int MinPlayerLevel { get; set; }
        public string RequiredRegion { get; set; }
        
        public ChoiceEventData() {
            Options = new List<ChoiceOption>();
        }
    }
    
    [System.Serializable]
    public class ChoiceOption {
        public string OptionId { get; set; }
        public string Text { get; set; }
        public string ResultText { get; set; }
        public List<RewardItem> Rewards { get; set; }
        public List<PenaltyItem> Penalties { get; set; }
        public float Weight { get; set; } // 选择权重
        public bool RequiresGold { get; set; }
        public int GoldCost { get; set; }
        
        public ChoiceOption() {
            Rewards = new List<RewardItem>();
            Penalties = new List<PenaltyItem>();
        }
    }
    
    [System.Serializable]
    public class RewardItem {
        public string Type { get; set; } // Gold/Exp/Item/Buff
        public string Id { get; set; }
        public int Amount { get; set; }
        public float Chance { get; set; }
    }
    
    [System.Serializable]
    public class PenaltyItem {
        public string Type { get; set; } // Gold/Health/Buff/Debuff
        public string Id { get; set; }
        public int Amount { get; set; }
    }
    
    /// <summary>
    /// 玩家事件选择数据
    /// </summary>
    [System.Serializable]
    public class PlayerChoiceEventData {
        public List<string> CompletedEventIds { get; set; }
        public Dictionary<string, List<string>> ChosenOptions { get; set; } // EventId -> List of chosen option IDs
        public int TotalChoicesMade { get; set; }
        public int TotalGoldEarned { get; set; }
        public int TotalExpEarned { get; set; }
        
        public PlayerChoiceEventData() {
            CompletedEventIds = new List<string>();
            ChosenOptions = new Dictionary<string, List<string>>();
        }
    }
}
