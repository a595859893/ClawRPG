using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 卡牌构建系统数据库
    /// </summary>
    public partial class DeckBuildingDatabase
    {
        private Dictionary<string, CardData> _cards = new Dictionary<string, CardData>();
        
        public DeckBuildingDatabase()
        {
            InitializeCards();
        }
        
        private void InitializeCards()
        {
            // ===== 基础攻击卡 =====
            AddCard(new CardData
            {
                Id = "Strike",
                Name = "打击",
                Description = "造成6点伤害",
                Type = CardType.Attack,
                Rarity = CardRarity.Common,
                Cost = 1,
                Damage = 6
            });
            
            AddCard(new CardData
            {
                Id = "Defend",
                Name = "防御",
                Description = "获得5点护甲",
                Type = CardType.Skill,
                Rarity = CardRarity.Common,
                Cost = 1,
                Block = 5
            });
            
            AddCard(new CardData
            {
                Id = "Bash",
                Name = "重击",
                Description = "造成8点伤害，敌人易伤1回合",
                Type = CardType.Attack,
                Rarity = CardRarity.Common,
                Cost = 2,
                Damage = 8,
                Effects = new List<string> { "Vulnerable 1" }
            });
            
            AddCard(new CardData
            {
                Id = "Shrug It Off",
                Name = "耸肩",
                Description = "获得8点护甲，抽1张牌",
                Type = CardType.Skill,
                Rarity = CardRarity.Common,
                Cost = 1,
                Block = 8,
                Draw = 1
            });
            
            AddCard(new CardData
            {
                Id = "Pommel Strike",
                Name = "柄击",
                Description = "造成9点伤害，抽1张牌",
                Type = CardType.Attack,
                Rarity = CardRarity.Common,
                Cost = 1,
                Damage = 9,
                Draw = 1
            });
            
            // ===== 技能卡 =====
            AddCard(new CardData
            {
                Id = "Fireball",
                Name = "火球术",
                Description = "对所有敌人造成10点伤害",
                Type = CardType.Attack,
                Rarity = CardRarity.Uncommon,
                Cost = 2,
                Damage = 10,
                Effects = new List<string> { "AOE" }
            });
            
            AddCard(new CardData
            {
                Id = "Ice Shield",
                Name = "冰盾",
                Description = "获得12点护甲，冰冻敌人1回合",
                Type = CardType.Skill,
                Rarity = CardRarity.Uncommon,
                Cost = 2,
                Block = 12,
                Effects = new List<string> { "Freeze 1" }
            });
            
            AddCard(new CardData
            {
                Id = "Lightning Bolt",
                Name = "闪电箭",
                Description = "造成15点伤害，闪电链",
                Type = CardType.Attack,
                Rarity = CardRarity.Uncommon,
                Cost = 2,
                Damage = 15,
                Effects = new List<string> { "Chain" }
            });
            
            AddCard(new CardData
            {
                Id = "Heal",
                Name = "治疗",
                Description = "恢复8点生命",
                Type = CardType.Skill,
                Rarity = CardRarity.Common,
                Cost = 1,
                Effects = new List<string> { "Heal 8" }
            });
            
            AddCard(new CardData
            {
                Id = "Fortify",
                Name = "加固",
                Description = "获得15点护甲，获得2能量",
                Type = CardType.Skill,
                Rarity = CardRarity.Uncommon,
                Cost = 0,
                Block = 15,
                EnergyGain = 2
            });
            
            // ===== 能力卡 =====
            AddCard(new CardData
            {
                Id = "Demon Form",
                Name = "恶魔形态",
                Description = "每回合获得3力量",
                Type = CardType.Power,
                Rarity = CardRarity.Rare,
                Cost = 3,
                Effects = new List<string> { "Demon Form" }
            });
            
            AddCard(new CardData
            {
                Id = "Limit Break",
                Name = "极限突破",
                Description = "力量翻倍，失去1生命上限",
                Type = CardType.Power,
                Rarity = CardRarity.Rare,
                Cost = 1,
                Effects = new List<string> { "Double Strength" }
            });
            
            AddCard(new CardData
            {
                Id = "Double Tap",
                Name = "双重打击",
                Description = "本回合攻击两次",
                Type = CardType.Power,
                Rarity = CardRarity.Uncommon,
                Cost = 1,
                Effects = new List<string> { "Double Attack" }
            });
            
            AddCard(new CardData
            {
                Id = "Spot Weakness",
                Name = "发现弱点",
                Description = "若敌人力量高于你，获得3力量",
                Type = CardType.Skill,
                Rarity = CardRarity.Uncommon,
                Cost = 1,
                Effects = new List<string> { "Conditional Strength" }
            });
            
            AddCard(new CardData
            {
                Id = "Catalyze",
                Name = "催化",
                Description = "本回合每使用一张卡，获得1力量",
                Type = CardType.Power,
                Rarity = CardRarity.Epic,
                Cost = 2,
                Effects = new List<string> { "Strength per Card" }
            });
            
            // ===== 高级解卡 =====
            AddCard(new CardData
            {
                Id = "Ghostly Armor",
                Name = "幽灵护甲",
                Description = "获得20点护甲，消耗敌人1能量",
                Type = CardType.Skill,
                Rarity = CardRarity.Rare,
                Cost = 2,
                Block = 20,
                Effects = new List<string> { "Energy Drain" }
            });
            
            AddCard(new CardData
            {
                Id = "Apotheosis",
                Name = "神化",
                Description = "本回合获得无限能量",
                Type = CardType.Power,
                Rarity = CardRarity.Legendary,
                Cost = 3,
                Effects = new List<string> { "Infinite Energy" }
            });
            
            AddCard(new CardData
            {
                Id = "Impenetrable",
                Name = "固若金汤",
                Description = "获得30点护甲，免疫下3次攻击",
                Type = CardType.Skill,
                Rarity = CardRarity.Epic,
                Cost = 2,
                Block = 30,
                Effects = new List<string> { "Block Shield 3" }
            });
            
            // ===== 稀有攻击卡 =====
            AddCard(new CardData
            {
                Id = "Carnage",
                Name = "大屠杀",
                Description = "造成20点伤害，获得1力量",
                Type = CardType.Attack,
                Rarity = CardRarity.Rare,
                Cost = 2,
                Damage = 20,
                Effects = new List<string> { "Strength 1" }
            });
            
            AddCard(new CardData
            {
                Id = "Sunder",
                Name = "碎裂",
                Description = "造成25点伤害，若敌人有护甲，改为造成3倍",
                Type = CardType.Attack,
                Rarity = CardRarity.Epic,
                Cost = 3,
                Damage = 25,
                Effects = new List<string> { "Armor Pierce" }
            });
        }
        
        private void AddCard(CardData card)
        {
            _cards[card.Id] = card;
        }
        
        public CardData GetCard(string cardId)
        {
            return _cards.ContainsKey(cardId) ? _cards[cardId] : null;
        }
        
        public List<CardData> GetCardsByRarity(CardRarity rarity)
        {
            var result = new List<CardData>();
            foreach (var card in _cards.Values)
            {
                if (card.Rarity == rarity)
                    result.Add(card);
            }
            return result;
        }
        
        public List<CardData> GetCardsByType(CardType type)
        {
            var result = new List<CardData>();
            foreach (var card in _cards.Values)
            {
                if (card.Type == type)
                    result.Add(card);
            }
            return result;
        }
        
        public Dictionary<string, CardData> GetAllCards() => _cards;
    }
}
