using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 卡牌构建系统 UI
    /// </summary>
    public partial class DeckBuildingUI : Control
    {
        private DeckBuildingSystem _system;
        private VBoxContainer _mainContainer;
        private HBoxContainer _handContainer;
        private HBoxContainer _deckInfoContainer;
        private Label _energyLabel;
        private Label _blockLabel;
        private Label _strengthLabel;
        private Label _pileInfoLabel;
        private TabContainer _tabContainer;
        
        public override void _Ready()
        {
            _system = GetNode<DeckBuildingSystem>("/root/Main/DeckBuildingSystem");
            if (_system == null)
            {
                GD.PrintErr("DeckBuildingSystem not found!");
                return;
            }
            
            SetupUI();
            RefreshUI();
        }
        
        private void SetupUI()
        {
            // 主容器
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(_mainContainer);
            
            // 标题
            var titleLabel = new Label();
            titleLabel.Text = "🎴 卡牌构建系统";
            titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _mainContainer.AddChild(titleLabel);
            
            // 状态栏
            var statusBar = new HBoxContainer();
            _mainContainer.AddChild(statusBar);
            
            _energyLabel = new Label();
            _energyLabel.Text = "⚡ 能量: 3/3";
            statusBar.AddChild(_energyLabel);
            
            var spacer = new Control();
            spacer.CustomMinimumSize = new Vector2(50, 0);
            statusBar.AddChild(spacer);
            
            _blockLabel = new Label();
            _blockLabel.Text = "🛡️ 护甲: 0";
            statusBar.AddChild(_blockLabel);
            
            var spacer2 = new Control();
            spacer2.CustomMinimumSize = new Vector2(50, 0);
            statusBar.AddChild(spacer2);
            
            _strengthLabel = new Label();
            _strengthLabel.Text = "💪 力量: 0";
            statusBar.AddChild(_strengthLabel);
            
            // 卡牌堆信息
            _pileInfoLabel = new Label();
            _pileInfoLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _mainContainer.AddChild(_pileInfoLabel);
            
            // 标签页容器
            _tabContainer = new TabContainer();
            _tabContainer.CustomMinimumSize = new Vector2(800, 450);
            _mainContainer.AddChild(_tabContainer);
            
            // 手牌标签页
            var handTab = new ScrollContainer();
            handTab.Name = "Hand";
            _tabContainer.AddChild(handTab);
            
            _handContainer = new HBoxContainer();
            _handContainer.Alignment = BoxContainer.AlignmentMode.Center;
            handTab.AddChild(_handContainer);
            
            // 套牌标签页
            var deckTab = new ScrollContainer();
            deckTab.Name = "Deck";
            _tabContainer.AddChild(deckTab);
            
            var deckList = new FlowContainer();
            deckList.Name = "DeckList";
            deckTab.AddChild(deckList);
            PopulateDeckList(deckList);
            
            // 收藏标签页
            var collectionTab = new ScrollContainer();
            collectionTab.Name = "Collection";
            _tabContainer.AddChild(collectionTab);
            
            var collectionList = new FlowContainer();
            collectionList.Name = "CollectionList";
            collectionTab.AddChild(collectionList);
            PopulateCollectionList(collectionList);
            
            // 统计标签页
            var statsTab = new VBoxContainer();
            statsTab.Name = "Statistics";
            _tabContainer.AddChild(statsTab);
            PopulateStatsTab(statsTab);
            
            // 控制按钮
            var buttonBar = new HBoxContainer();
            _mainContainer.AddChild(buttonBar);
            
            var drawButton = new Button();
            drawButton.Text = "抽卡 (5)";
            drawButton.Pressed += () => _system.DrawCards(5);
            buttonBar.AddChild(drawButton);
            
            var startTurnButton = new Button();
            startTurnButton.Text = "开始回合";
            startTurnButton.Pressed += () => _system.StartTurn();
            buttonBar.AddChild(startTurnButton);
            
            var endTurnButton = new Button();
            endTurnButton.Text = "结束回合";
            endTurnButton.Pressed += () => _system.EndTurn();
            buttonBar.AddChild(endTurnButton);
            
            var closeButton = new Button();
            closeButton.Text = "关闭 (ESC)";
            closeButton.Pressed += OnClosePressed;
            buttonBar.AddChild(closeButton);
        }
        
        private void PopulateDeckList(FlowContainer container)
        {
            foreach (var child in container.GetChildren())
            {
                child.QueueFree();
            }
            
            var deck = _system.GetCurrentDeck();
            var counts = new Dictionary<string, int>();
            foreach (var cardId in deck)
            {
                if (!counts.ContainsKey(cardId))
                    counts[cardId] = 0;
                counts[cardId]++;
            }
            
            foreach (var kvp in counts)
            {
                var card = _system.GetCardData(kvp.Key);
                if (card != null)
                {
                    var cardPanel = CreateCardPanel(card, kvp.Value);
                    container.AddChild(cardPanel);
                }
            }
        }
        
        private void PopulateCollectionList(FlowContainer container)
        {
            foreach (var child in container.GetChildren())
            {
                child.QueueFree();
            }
            
            var db = _system.GetDatabase();
            var allCards = db.GetAllCards();
            
            foreach (var kvp in allCards)
            {
                var cardPanel = CreateCardPanel(kvp.Value, 0);
                container.AddChild(cardPanel);
            }
        }
        
        private void PopulateStatsTab(VBoxContainer container)
        {
            var stats = _system.GetStatistics();
            
            var playedLabel = new Label();
            playedLabel.Text = $"总使用卡牌: {stats["TotalPlayed"]}";
            container.AddChild(playedLabel);
            
            var damageLabel = new Label();
            damageLabel.Text = $"总伤害: {stats["TotalDamage"]}";
            container.AddChild(damageLabel);
            
            var drawnLabel = new Label();
            drawnLabel.Text = $"总抽卡: {stats["TotalDrawn"]}";
            container.AddChild(drawnLabel);
            
            var winsLabel = new Label();
            winsLabel.Text = $"胜利: {stats["Wins"]}";
            container.AddChild(winsLabel);
            
            var lossesLabel = new Label();
            lossesLabel.Text = $"失败: {stats["Losses"]}";
            container.AddChild(lossesLabel);
        }
        
        private Control CreateCardPanel(CardData card, int count)
        {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(120, 160);
            
            var vbox = new VBoxContainer();
            panel.AddChild(vbox);
            
            var nameLabel = new Label();
            nameLabel.Text = count > 0 ? $"{card.Name} x{count}" : card.Name;
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            nameLabel.AddThemeFontSizeOverride("font_size", 12);
            vbox.AddChild(nameLabel);
            
            var typeLabel = new Label();
            typeLabel.Text = $"[{card.Type}]";
            typeLabel.HorizontalAlignment = HorizontalAlignment.Center;
            typeLabel.AddThemeFontSizeOverride("font_size", 10);
            vbox.AddChild(typeLabel);
            
            var costLabel = new Label();
            costLabel.Text = $"⚡ {card.Cost}";
            costLabel.HorizontalAlignment = HorizontalAlignment.Center;
            costLabel.AddThemeColorOverride("font_color", new Color(1, 0.8, 0.2));
            vbox.AddChild(costLabel);
            
            var descLabel = new Label();
            descLabel.Text = card.Description;
            descLabel.HorizontalAlignment = HorizontalAlignment.Center;
            descLabel.AddThemeFontSizeOverride("font_size", 9);
            descLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
            vbox.AddChild(descLabel);
            
            // 颜色区分
            Color rarityColor = card.Rarity switch
            {
                CardRarity.Common => Colors.Gray,
                CardRarity.Uncommon => Colors.Green,
                CardRarity.Rare => Colors.Blue,
                CardRarity.Epic => Colors.Purple,
                CardRarity.Legendary => Colors.Orange,
                _ => Colors.White
            };
            panel.AddThemeStyleboxOverride("panel", CreateRarityStyle(rarityColor));
            
            return panel;
        }
        
        private StyleBoxFlat CreateRarityStyle(Color color)
        {
            var style = new StyleBoxFlat();
            style.BorderWidthLeft = 2;
            style.BorderWidthRight = 2;
            style.BorderWidthTop = 2;
            style.BorderWidthBottom = 2;
            style.BorderColor = color;
            style.SetContentMarginAll(5);
            return style;
        }
        
        public override void _Process(double delta)
        {
            if (_system == null) return;
            
            // 更新状态显示
            _energyLabel.Text = $"⚡ 能量: {_system.GetCurrentEnergy()}/{_system.GetMaxEnergy()}";
            _blockLabel.Text = $"🛡️ 护甲: {_system.GetBlock()}";
            _strengthLabel.Text = $"💪 力量: {_system.GetStrength()}";
            _pileInfoLabel.Text = $"抽牌堆: {_system.GetDrawPileCount()} | 弃牌堆: {_system.GetDiscardPileCount()} | 手牌: {_system.GetHand().Count}";
            
            // 更新手牌显示
            RefreshHandDisplay();
        }
        
        private void RefreshHandDisplay()
        {
            foreach (var child in _handContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            var hand = _system.GetHand();
            foreach (var cardId in hand)
            {
                var card = _system.GetCardData(cardId);
                if (card != null)
                {
                    var cardPanel = CreateCardPanel(card, 1);
                    _handContainer.AddChild(cardPanel);
                }
            }
        }
        
        private void RefreshUI()
        {
            RefreshHandDisplay();
        }
        
        private void OnClosePressed()
        {
            Hide();
            QueueFree();
        }
        
        public override void _Input(InputEvent e)
        {
            if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
            {
                Hide();
                QueueFree();
            }
        }
    }
}
