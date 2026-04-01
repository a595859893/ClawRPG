using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 卡牌构建系统
    /// </summary>
    public partial class DeckBuildingSystem : BaseSystem
    {
        private DeckBuildingData _data;
        private DeckBuildingDatabase _database;
        private List<string> _drawPile = new List<string>();
        private List<string> _discardPile = new List<string>();
        private List<string> _hand = new List<string>();
        private int _currentEnergy = 3;
        private int _maxEnergy = 3;
        private int _strength = 0;
        private int _block = 0;
        
        public override void _Ready()
        {
            _data = new DeckBuildingData();
            _database = new DeckBuildingDatabase();
            ShuffleDeckIntoDrawPile();
        }
        
        /// <summary>
        /// 将套牌洗入抽牌堆
        /// </summary>
        public void ShuffleDeckIntoDrawPile()
        {
            _drawPile.Clear();
            _drawPile.AddRange(_data.CurrentDeck);
            _discardPile.Clear();
            _hand.Clear();
            ShuffleDrawPile();
            DrawCards(5);
        }
        
        /// <summary>
        /// 洗牌
        /// </summary>
        private void ShuffleDrawPile()
        {
            var random = new Random();
            int n = _drawPile.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                (_drawPile[k], _drawPile[n]) = (_drawPile[n], _drawPile[k]);
            }
        }
        
        /// <summary>
        /// 抽卡
        /// </summary>
        public void DrawCards(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_drawPile.Count == 0)
                {
                    if (_discardPile.Count > 0)
                    {
                        _drawPile.AddRange(_discardPile);
                        _discardPile.Clear();
                        ShuffleDrawPile();
                    }
                    else
                    {
                        break; // 没有牌了
                    }
                }
                
                if (_drawPile.Count > 0)
                {
                    var cardId = _drawPile[0];
                    _drawPile.RemoveAt(0);
                    _hand.Add(cardId);
                    _data.TotalCardsDrawn++;
                }
            }
        }
        
        /// <summary>
        /// 使用卡牌
        /// </summary>
        public bool PlayCard(string cardId, string targetId = "")
        {
            if (!_hand.Contains(cardId))
                return false;
            
            var card = _database.GetCard(cardId);
            if (card == null)
                return false;
            
            if (_currentEnergy < card.Cost)
                return false; // 能量不足
            
            // 消耗能量
            _currentEnergy -= card.Cost;
            
            // 从手牌移除
            _hand.Remove(cardId);
            
            // 应用卡牌效果
            ApplyCardEffects(card, targetId);
            
            // 加入弃牌堆
            _discardPile.Add(cardId);
            
            _data.TotalCardsPlayed++;
            
            return true;
        }
        
        /// <summary>
        /// 应用卡牌效果
        /// REQ-166: Evaluates conditional card effects before applying.
        /// </summary>
        private void ApplyCardEffects(CardData card, string targetId)
        {
            // REQ-166: Evaluate conditions to get effect multiplier
            float conditionMultiplier = CardConditionEvaluator.Instance.EvaluateConditions(card.Conditions);
            
            int finalDamage = (int)((card.Damage + _strength) * conditionMultiplier);
            int finalBlock = (int)(card.Block * conditionMultiplier);
            
            GD.Print($"Playing card: {card.Name}, Damage: {finalDamage} (x{conditionMultiplier:F1}), Block: {finalBlock}");
            
            if (card.Damage > 0)
            {
                _data.TotalDamageDealt += finalDamage;
                // 这里可以调用伤害系统
            }
            
            if (card.Block > 0)
            {
                _block += finalBlock;
            }
            
            if (card.Draw > 0)
            {
                DrawCards(card.Draw);
            }
            
            if (card.EnergyGain > 0)
            {
                _currentEnergy += card.EnergyGain;
            }
            
            // 处理特殊效果
            foreach (var effect in card.Effects)
            {
                ProcessEffect(effect);
            }
        }
        
        /// <summary>
        /// 处理特殊效果
        /// </summary>
        private void ProcessEffect(string effect)
        {
            switch (effect)
            {
                case "Vulnerable 1":
                    // 敌人易伤
                    break;
                case "Freeze 1":
                    // 冰冻敌人
                    break;
                case "AOE":
                    // 对所有敌人
                    break;
                case "Chain":
                    // 闪电链
                    break;
                case "Heal 8":
                    // 治疗
                    break;
                case "Demon Form":
                    // 恶魔形态 - 持续获得力量
                    _strength += 3;
                    break;
                case "Double Strength":
                    // 力量翻倍
                    _strength *= 2;
                    break;
                case "Double Attack":
                    // 本回合攻击两次
                    break;
                default:
                    GD.Print($"Unknown effect: {effect}");
                    break;
            }
        }
        
        /// <summary>
        /// 开始新回合
        /// </summary>
        public void StartTurn()
        {
            _currentEnergy = _maxEnergy;
            _block = 0;
            DrawCards(5);
            GD.Print("New turn started");
        }
        
        /// <summary>
        /// 结束回合
        /// </summary>
        public void EndTurn()
        {
            // 手牌全部放入弃牌堆
            foreach (var cardId in _hand)
            {
                _discardPile.Add(cardId);
            }
            _hand.Clear();
            
            GD.Print("Turn ended");
        }
        
        /// <summary>
        /// 获取当前手牌
        /// </summary>
        public List<string> GetHand() => _hand;
        
        /// <summary>
        /// 获取抽牌堆数量
        /// </summary>
        public int GetDrawPileCount() => _drawPile.Count;
        
        /// <summary>
        /// 获取弃牌堆数量
        /// </summary>
        public int GetDiscardPileCount() => _discardPile.Count;
        
        /// <summary>
        /// 获取当前能量
        /// </summary>
        public int GetCurrentEnergy() => _currentEnergy;
        
        /// <summary>
        /// 获取最大能量
        /// </summary>
        public int GetMaxEnergy() => _maxEnergy;
        
        /// <summary>
        /// 获取护甲值
        /// </summary>
        public int GetBlock() => _block;
        
        /// <summary>
        /// 获取力量值
        /// </summary>
        public int GetStrength() => _strength;
        
        /// <summary>
        /// 获取卡牌数据
        /// </summary>
        public CardData GetCardData(string cardId) => _database.GetCard(cardId);
        
        /// <summary>
        /// 获取套牌
        /// </summary>
        public List<string> GetCurrentDeck() => _data.CurrentDeck;
        
        /// <summary>
        /// 获取数据库
        /// </summary>
        public DeckBuildingDatabase GetDatabase() => _database;
        
        // ===== 持久化 =====
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // 保存当前套牌
            data["current_deck"] = new Array(_data.CurrentDeck);
            
            // 保存统计数据
            data["total_cards_played"] = _data.TotalCardsPlayed;
            data["total_cards_drawn"] = _data.TotalCardsDrawn;
            data["total_damage_dealt"] = _data.TotalDamageDealt;
            data["deck_wins"] = _data.DeckWins;
            data["deck_losses"] = _data.DeckLosses;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            // 恢复套牌
            if (data.ContainsKey("current_deck"))
            {
                _data.CurrentDeck.Clear();
                var deck = (Array)data["current_deck"];
                foreach (string cardId in deck)
                {
                    _data.CurrentDeck.Add(cardId);
                }
            }
            
            // 恢复统计数据
            if (data.ContainsKey("total_cards_played"))
                _data.TotalCardsPlayed = Convert.ToInt32(data["total_cards_played"]);
            if (data.ContainsKey("total_cards_drawn"))
                _data.TotalCardsDrawn = Convert.ToInt32(data["total_cards_drawn"]);
            if (data.ContainsKey("total_damage_dealt"))
                _data.TotalDamageDealt = Convert.ToInt32(data["total_damage_dealt"]);
            if (data.ContainsKey("deck_wins"))
                _data.DeckWins = Convert.ToInt32(data["deck_wins"]);
            if (data.ContainsKey("deck_losses"))
                _data.DeckLosses = Convert.ToInt32(data["deck_losses"]);
        }
        
        /// <summary>
        /// 获取统计数据
        /// </summary>
        public Dictionary<string, int> GetStatistics()
        {
            return new Dictionary<string, int>
            {
                { "TotalPlayed", _data.TotalCardsPlayed },
                { "TotalDamage", _data.TotalDamageDealt },
                { "TotalDrawn", _data.TotalCardsDrawn },
                { "Wins", _data.DeckWins },
                { "Losses", _data.DeckLosses }
            };
        }
        
        /// <summary>
        /// 添加卡牌到套牌
        /// </summary>
        public void AddCardToDeck(string cardId)
        {
            if (!_data.CurrentDeck.Contains(cardId))
            {
                _data.CurrentDeck.Add(cardId);
            }
        }

        /// <summary>
        /// 直接添加卡牌到手牌（用于沉积卡等外部系统注入）
        /// </summary>
        public void AddCardToHand(string cardId)
        {
            if (!_hand.Contains(cardId))
            {
                _hand.Add(cardId);
                _data.TotalCardsDrawn++;
            }
        }
        
        /// <summary>
        /// 从套牌移除卡牌
        /// </summary>
        public void RemoveCardFromDeck(string cardId)
        {
            if (_data.CurrentDeck.Contains(cardId))
            {
                _data.CurrentDeck.Remove(cardId);
            }
        }
        
        /// <summary>
        /// 记录胜利
        /// </summary>
        public void RecordWin()
        {
            _data.DeckWins++;
        }
        
        /// <summary>
        /// 记录失败
        /// </summary>
        public void RecordLoss()
        {
            _data.DeckLosses++;
        }
    }
}
