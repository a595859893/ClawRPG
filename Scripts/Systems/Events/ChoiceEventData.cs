using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Events
{
    /// <summary>
    /// 奖励物品数据
    /// </summary>
    public class RewardItem
    {
        public string Type { get; set; }       // Gold, Exp, Item, Buff
        public int Amount { get; set; }
        public float Chance { get; set; }
        public string Id { get; set; }          // For Item/Buff types
        
        public RewardItem()
        {
        }
        
        public RewardItem(string type, int amount, float chance = 1.0f, string id = "")
        {
            Type = type;
            Amount = amount;
            Chance = chance;
            Id = id;
        }
    }
    
    /// <summary>
    /// 惩罚物品数据
    /// </summary>
    public class PenaltyItem
    {
        public string Type { get; set; }       // Health, Debuff
        public int Amount { get; set; }
        public float Chance { get; set; }
        public string Id { get; set; }          // For Debuff type
        
        public PenaltyItem()
        {
        }
        
        public PenaltyItem(string type, int amount, float chance = 1.0f, string id = "")
        {
            Type = type;
            Amount = amount;
            Chance = chance;
            Id = id;
        }
    }
    
    /// <summary>
    /// 选择选项数据
    /// </summary>
    public class ChoiceOption
    {
        public string OptionId { get; set; }
        public string Text { get; set; }
        public string ResultText { get; set; }
        public List<RewardItem> Rewards { get; set; }
        public List<PenaltyItem> Penalties { get; set; }
        public float Weight { get; set; }
        public bool RequiresGold { get; set; }
        public int GoldCost { get; set; }
        
        public ChoiceOption()
        {
            Rewards = new List<RewardItem>();
            Penalties = new List<PenaltyItem>();
        }
    }
    
    /// <summary>
    /// 事件选择数据
    /// </summary>
    public class ChoiceEventData
    {
        public string EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int MinPlayerLevel { get; set; }
        public string RequiredRegion { get; set; }
        public List<ChoiceOption> Options { get; set; }
        
        public ChoiceEventData()
        {
            Options = new List<ChoiceOption>();
            RequiredRegion = "";
        }
    }
}
