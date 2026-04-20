using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 卡牌构建系统数据
    /// </summary>
    [GlobalClass]
    public partial class DeckBuildingData : Resource
    {
        [Export] public Godot.Collections.Dictionary UnlockedCards = new Godot.Collections.Dictionary();
        [Export] public Godot.Collections.Array CurrentDeck = new Godot.Collections.Array();
        [Export] public Godot.Collections.Dictionary CardCollection = new Godot.Collections.Dictionary();
        [Export] public int TotalCardsPlayed = 0;
        [Export] public int TotalDamageDealt = 0;
        [Export] public int TotalCardsDrawn = 0;
        [Export] public int DeckWins = 0;
        [Export] public int DeckLosses = 0;
        
        public DeckBuildingData()
        {
            InitializeDefaultCards();
        }
        
        private void InitializeDefaultCards()
        {
            // 基础攻击卡
            var basicAttacks = new[]
            {
                "Strike", "Defend", "Bash", "Shrug It Off", "Pommel Strike",
                "Body Slam", "Heavy Blade", "Twin Strike", "Carnage", "Sunder"
            };
            
            // 技能卡
            var skillCards = new[]
            {
                "Fireball", "Ice Shield", "Lightning Bolt", "Heal", "Fortify",
                "Focus", "Meditate", "Rage", "Grace", "Valor"
            };
            
            // 能力卡
            var powerCards = new[]
            {
                "Demon Form", "Limit Break", "Double Tap", "Spot Weakness", "Catalyze",
                "Pressure Points", "Master of Strategy", "Berserk", "Corruption", "Omniscience"
            };
            
            // 解卡
            var defenseCards = new[]
            {
                "Ghostly Armor", "Panacea", "Apotheosis", "Foresight", "Impervious",
                "Intimidate", "Shockwave", "Whirlwind", "Battle Trance", "Enlightenment"
            };
            
            // 解锁所有卡
            foreach (var card in basicAttacks)
                UnlockedCards[card] = true;
            foreach (var card in skillCards)
                UnlockedCards[card] = true;
            foreach (var card in powerCards)
                UnlockedCards[card] = false; // 需要解锁
            foreach (var card in defenseCards)
                UnlockedCards[card] = false;
                
            // 默认套牌
            CurrentDeck = new Godot.Collections.Array { "Strike", "Strike", "Strike", "Defend", "Defend", "Defend", "Bash" };
        }
    }
    
    /// <summary>
    /// 卡牌数据结构
    /// </summary>
    public class CardData
    {
        public string Id = "";
        public string Name = "";
        public string Description = "";
        public CardType Type = CardType.Attack;
        public CardRarity Rarity = CardRarity.Common;
        public int Cost = 1;
        public int Damage = 0;
        public int Block = 0;
        public int Draw = 0;
        public int EnergyGain = 0;
        public List<string> Effects = new List<string>();
        public bool IsUpgraded = false;
        
        // REQ-166: Conditional card effects
        public List<CardCondition> Conditions = new List<CardCondition>();
        
        public string GetDisplayText()
        {
            var text = $"{Name}\n";
            text += $"[Cost: {Cost}] ";
            
            if (Damage > 0) text += $"Deal {Damage} damage. ";
            if (Block > 0) text += $"Gain {Block} block. ";
            if (Draw > 0) text += $"Draw {Draw} card. ";
            if (EnergyGain > 0) text += $"Gain {EnergyGain} energy. ";
            
            foreach (var effect in Effects)
            {
                text += effect + ". ";
            }
            
            return text;
        }
    }
    
    public enum CardType
    {
        Attack,
        Skill,
        Power,
        Status,
        Curse
    }
    
    public enum CardRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}
