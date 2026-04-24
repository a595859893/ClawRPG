using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ClawRPG.Scripts.Systems.EventCardPool
{
    /// <summary>
    /// 事件卡池运行时数据（单例，每局游戏一份）
    /// </summary>
    public partial class EventCardPoolData : Node
    {
        private static EventCardPoolData _instance;
        public static EventCardPoolData Instance => _instance;

        // ========== 卡池配置 ==========
        private EventCardsConfigFile _configFile;
        private List<EventCardConfig> _allCards = new List<EventCardConfig>();
        private bool _isLoaded = false;

        // ========== 当前局状态 ==========
        private string _currentDrawnCardId = "";
        private List<string> _usedCardIds = new List<string>();  // 本局已出现过的卡
        private int _rerollCount = 0;
        private bool _cardAccepted = false;

        // ========== 信号 ==========
        public Action<string> OnCardDrawn;          // cardId
        public Action<string> OnCardAccepted;      // cardId
        public Action<string> OnCardReplaced;     // oldCardId
        public Action<string, string> OnCardDiscarded; // oldCardId, reason

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }
            _instance = this;
            LoadCardPool();
        }

        // ========== 卡池加载 ==========
        public bool LoadCardPool()
        {
            string configPath = "res://Resources/Config/event_cards_config.json";
            if (!FileAccess.FileExists(configPath))
            {
                GD.PrintErr($"[EventCardPoolData] 卡池配置文件不存在: {configPath}");
                return false;
            }

            try
            {
                using var file = FileAccess.Open(configPath, FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    GD.PrintErr($"[EventCardPoolData] 无法打开配置文件: {configPath}");
                    return false;
                }
                string json = file.GetAsText();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                _configFile = JsonSerializer.Deserialize<EventCardsConfigFile>(json, options);

                if (_configFile == null || _configFile.Cards == null)
                {
                    GD.PrintErr("[EventCardPoolData] 卡池配置解析失败");
                    return false;
                }

                _allCards = _configFile.Cards;
                _isLoaded = true;
                GD.Print($"[EventCardPoolData] 成功加载 {_allCards.Count} 张事件卡");
                return true;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[EventCardPoolData] 卡池加载异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 热重载卡池（修改JSON后调用）
        /// </summary>
        public void ReloadCardPool()
        {
            _usedCardIds.Clear();
            _currentDrawnCardId = "";
            _rerollCount = 0;
            _cardAccepted = false;
            LoadCardPool();
            GD.Print("[EventCardPoolData] 卡池热重载完成");
        }

        // ========== 抽卡逻辑 ==========
        /// <summary>
        /// 抽取一张随机事件卡（按稀有度加权）
        /// </summary>
        public string DrawCard()
        {
            if (!_isLoaded || _allCards.Count == 0)
            {
                GD.PrintErr("[EventCardPoolData] 卡池未加载，无法抽卡");
                return "";
            }

            // 过滤可用卡（未使用 + 等级满足）
            var availableCards = GetAvailableCards();
            if (availableCards.Count == 0)
            {
                // 所有卡都用过了，清空已用列表重新开始
                _usedCardIds.Clear();
                availableCards = GetAvailableCards();
                if (availableCards.Count == 0)
                {
                    GD.PrintWarn("[EventCardPoolData] 卡池为空");
                    return "";
                }
            }

            // 加权随机
            string drawnId = WeightedRandomDraw(availableCards);
            if (string.IsNullOrEmpty(drawnId)) return "";

            _currentDrawnCardId = drawnId;
            _usedCardIds.Add(drawnId);
            _cardAccepted = false;

            GD.Print($"[EventCardPoolData] 抽卡: {GetCardById(drawnId)?.Title} ({drawnId})");
            OnCardDrawn?.Invoke(drawnId);
            return drawnId;
        }

        /// <summary>
        /// 重新抽卡（消耗资源）
        /// </summary>
        public string ReDrawCard()
        {
            if (string.IsNullOrEmpty(_currentDrawnCardId))
            {
                return DrawCard();
            }

            string oldCardId = _currentDrawnCardId;
            string newCardId = DrawCard();

            if (!string.IsNullOrEmpty(newCardId))
            {
                _rerollCount++;
                OnCardReplaced?.Invoke(oldCardId);
            }

            return newCardId;
        }

        /// <summary>
        /// 接受当前抽中的卡
        /// </summary>
        public void AcceptCurrentCard()
        {
            if (string.IsNullOrEmpty(_currentDrawnCardId)) return;

            _cardAccepted = true;
            OnCardAccepted?.Invoke(_currentDrawnCardId);
            GD.Print($"[EventCardPoolData] 接受事件卡: {_currentDrawnCardId}");
        }

        /// <summary>
        /// 获取当前抽中的卡
        /// </summary>
        public EventCardConfig GetCurrentCard()
        {
            if (string.IsNullOrEmpty(_currentDrawnCardId)) return null;
            return GetCardById(_currentDrawnCardId);
        }

        /// <summary>
        /// 根据ID获取卡配置
        /// </summary>
        public EventCardConfig GetCardById(string cardId)
        {
            return _allCards.Find(c => c.CardId == cardId);
        }

        /// <summary>
        /// 获取可用卡列表（未使用 + 等级满足）
        /// </summary>
        public List<EventCardConfig> GetAvailableCards()
        {
            int playerLevel = GetPlayerLevel();
            var available = new List<EventCardConfig>();
            foreach (var card in _allCards)
            {
                if (!_usedCardIds.Contains(card.CardId) && card.MinPlayerLevel <= playerLevel)
                {
                    available.Add(card);
                }
            }
            return available;
        }

        /// <summary>
        /// 加权随机抽取
        /// </summary>
        private string WeightedRandomDraw(List<EventCardConfig> cards)
        {
            float totalWeight = 0f;
            foreach (var card in cards)
            {
                totalWeight += EventCardConfig.GetRarityWeight(card.Rarity);
            }

            if (totalWeight <= 0f) return "";

            float roll = (float)GD.Randd() * totalWeight;
            float cumulative = 0f;

            foreach (var card in cards)
            {
                cumulative += EventCardConfig.GetRarityWeight(card.Rarity);
                if (roll <= cumulative)
                {
                    return card.CardId;
                }
            }

            return cards[cards.Count - 1].CardId;
        }

        private int GetPlayerLevel()
        {
            // TODO: 从玩家数据获取等级，临时返回1
            return 1;
        }

        // ========== 查询 API ==========
        public bool IsCardAccepted => _cardAccepted;
        public int RerollCount => _rerollCount;
        public bool IsLoaded => _isLoaded;
        public int TotalCards => _allCards.Count;
        public int AvailableCards => GetAvailableCards().Count;
        public int UsedCards => _usedCardIds.Count;

        // ========== 持久化 ==========
        public Dictionary ExportSaveData()
        {
            return new Dictionary {
                { "currentDrawnCardId", _currentDrawnCardId },
                { "usedCardIds", _usedCardIds },
                { "rerollCount", _rerollCount },
                { "cardAccepted", _cardAccepted }
            };
        }

        public void ImportSaveData(Dictionary data)
        {
            if (data == null) return;

            if (data.ContainsKey("currentDrawnCardId"))
                _currentDrawnCardId = (string)data["currentDrawnCardId"];
            if (data.ContainsKey("usedCardIds"))
                _usedCardIds = new List<string>((Godot.Collections.Array)data["usedCardIds"]);
            if (data.ContainsKey("rerollCount"))
                _rerollCount = (int)(long)data["rerollCount"];
            if (data.ContainsKey("cardAccepted"))
                _cardAccepted = (bool)data["cardAccepted"];

            GD.Print($"[EventCardPoolData] Import: card={_currentDrawnCardId}, used={_usedCardIds.Count}, rerolls={_rerollCount}");
        }

        /// <summary>
        /// 重置本局状态（新游戏开始时调用）
        /// </summary>
        public void ResetRunState()
        {
            _currentDrawnCardId = "";
            _usedCardIds.Clear();
            _rerollCount = 0;
            _cardAccepted = false;
            GD.Print("[EventCardPoolData] 局状态已重置");
        }
    }
}
