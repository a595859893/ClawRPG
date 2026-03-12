using Godot;
using System;
using System.Collections.Generic;

public class TradeRouteUI : Control
{
    private TabContainer _tabContainer;
    private Label _goldLabel;
    private Label _profitLabel;
    private Label _tradeCountLabel;
    private VBoxContainer _routesContainer;
    private VBoxContainer _historyContainer;
    private VBoxContainer _marketContainer;

    private Color _rareColor = new Color(1, 0.84f, 0);
    private Color _epicColor = new Color(0.64f, 0.21f, 0.94f);
    private Color _legendaryColor = new Color(1, 0.5f, 0);
    private Color _commonColor = new Color(0.7f, 0.7f, 0.7f);
    private Color _uncommonColor = new Color(0.3f, 1f, 0.3f);

    public override void _Ready()
    {
        Visible = false;
        SetupUI();
        
        TradeRouteSystem.Instance.OnTradeCompleted += OnTradeCompleted;
        TradeRouteSystem.Instance.OnMarketUpdate += RefreshMarketDisplay;
    }

    private void SetupUI()
    {
        var bg = new Panel();
        bg.SetAnchor(AnchorPreset.FullRect);
        bg.Modulate = new Color(0, 0, 0, 0.85f);
        AddChild(bg);

        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchor(AnchorPreset.FullRect);
        mainContainer.AddThemeConstantOverride("separation", 20);
        AddChild(mainContainer);

        // Header
        var header = new HBoxContainer();
        header.AddThemeConstantOverride("separation", 20);
        mainContainer.AddChild(header);

        var title = new Label();
        title.Text = "🚚 Trade Route System";
        title.AddThemeFontSizeOverride("font_size", 24);
        header.AddChild(title);

        var spacer = new Control();
        spacer.SizeFlagsHorizontal = Control.SizeFlagsExpandFill;
        header.AddChild(spacer);

        _goldLabel = new Label();
        _goldLabel.Text = $"Gold: {Player.Instance.Gold}";
        _goldLabel.AddThemeFontSizeOverride("font_size", 18);
        header.AddChild(_goldLabel);

        // Stats
        var statsContainer = new HBoxContainer();
        statsContainer.AddThemeConstantOverride("separation", 40);
        mainContainer.AddChild(statsContainer);

        _profitLabel = new Label();
        _profitLabel.Text = $"Total Profit: 0";
        _profitLabel.AddThemeFontSizeOverride("font_size", 16);
        statsContainer.AddChild(_profitLabel);

        _tradeCountLabel = new Label();
        _tradeCountLabel.Text = $"Trades: 0 | Success Rate: 0%";
        _tradeCountLabel.AddThemeFontSizeOverride("font_size", 16);
        statsContainer.AddChild(_tradeCountLabel);

        // Tab Container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsVertical = Control.SizeFlagsExpandFill;
        mainContainer.AddChild(_tabContainer);

        // Routes Tab
        var routesTab = new Control();
        routesTab.Name = "Routes";
        _tabContainer.AddChild(routesTab);
        SetupRoutesTab(routesTab);

        // Market Tab
        var marketTab = new Control();
        marketTab.Name = "Market";
        _tabContainer.AddChild(marketTab);
        SetupMarketTab(marketTab);

        // History Tab
        var historyTab = new Control();
        historyTab.Name = "History";
        _tabContainer.AddChild(historyTab);
        SetupHistoryTab(historyTab);

        // Footer
        var footer = new HBoxContainer();
        footer.AddThemeConstantOverride("separation", 10);
        mainContainer.AddChild(footer);

        var refreshBtn = new Button();
        refreshBtn.Text = "Refresh Market (Free)";
        refreshBtn.Pressed += () => TradeRouteSystem.Instance.RefreshMarket();
        footer.AddChild(refreshBtn);

        var spacer2 = new Control();
        spacer2.SizeFlagsHorizontal = Control.SizeFlagsExpandFill;
        footer.AddChild(spacer2);

        var closeBtn = new Button();
        closeBtn.Text = "Close (ESC)";
        closeBtn.Pressed += ToggleVisibility;
        footer.AddChild(closeBtn);

        RefreshDisplay();
    }

    private void SetupRoutesTab(Control parent)
    {
        var scroll = new ScrollContainer();
        scroll.SetAnchor(AnchorPreset.FullRect);
        scroll.AddThemeConstantOverride("h_separation", 10);
        parent.AddChild(scroll);

        _routesContainer = new VBoxContainer();
        _routesContainer.AddThemeConstantOverride("separation", 10);
        scroll.AddChild(_routesContainer);

        RefreshRoutesDisplay();
    }

    private void SetupMarketTab(Control parent)
    {
        var scroll = new ScrollContainer();
        scroll.SetAnchor(AnchorPreset.FullRect);
        parent.AddChild(scroll);

        _marketContainer = new VBoxContainer();
        _marketContainer.AddThemeConstantOverride("separation", 5);
        scroll.AddChild(_marketContainer);

        RefreshMarketDisplay();
    }

    private void SetupHistoryTab(Control parent)
    {
        var scroll = new ScrollContainer();
        scroll.SetAnchor(AnchorPreset.FullRect);
        parent.AddChild(scroll);

        _historyContainer = new VBoxContainer();
        _historyContainer.AddThemeConstantOverride("separation", 5);
        scroll.AddChild(_historyContainer);

        RefreshHistoryDisplay();
    }

    public void RefreshDisplay()
    {
        RefreshStats();
        RefreshRoutesDisplay();
        RefreshMarketDisplay();
        RefreshHistoryDisplay();
    }

    private void RefreshStats()
    {
        _goldLabel.Text = $"Gold: {Player.Instance.Gold}";
        _profitLabel.Text = $"Total Profit: {TradeRouteSystem.Instance.GetTotalProfit():N0}";
        
        int trades = TradeRouteSystem.Instance.GetTotalTrades();
        float rate = TradeRouteSystem.Instance.GetSuccessRate() * 100;
        _tradeCountLabel.Text = $"Trades: {trades} | Success Rate: {rate:F1}%";
    }

    private void RefreshRoutesDisplay()
    {
        foreach (var child in _routesContainer.GetChildren())
        {
            child.QueueFree();
        }

        var templates = TradeRouteSystem.Instance.GetAvailableRouteTemplates();
        
        foreach (var template in templates)
        {
            var card = CreateRouteCard(template);
            _routesContainer.AddChild(card);
        }
    }

    private Control CreateRouteCard(TradeRouteDatabase.RouteTemplate template)
    {
        var card = new PanelContainer();
        card.CustomMinimumSize = new Vector2(0, 120);
        
        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 15);
        card.AddChild(hbox);

        // Route Info
        var info = new VBoxContainer();
        info.AddThemeConstantOverride("separation", 5);
        hbox.AddChild(info);

        var nameLabel = new Label();
        nameLabel.Text = template.RouteName;
        nameLabel.AddThemeFontSizeOverride("font_size", 18);
        info.AddChild(nameLabel);

        var descLabel = new Label();
        descLabel.Text = template.Description;
        descLabel.AddThemeFontSizeOverride("font_size", 12);
        descLabel.Modulate = new Color(0.8f, 0.8f, 0.8f);
        info.AddChild(descLabel);

        var reqLabel = new Label();
        reqLabel.Text = $"Level Req: {template.MinLevel} | Distance: {template.BaseDistance}";
        reqLabel.AddThemeFontSizeOverride("font_size", 12);
        reqLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
        info.AddChild(reqLabel);

        // Spacer
        var spacer = new Control();
        spacer.SizeFlagsHorizontal = Control.SizeFlagsExpandFill;
        hbox.AddChild(spacer);

        // Actions
        var actions = new VBoxContainer();
        actions.AddThemeConstantOverride("separation", 5);
        hbox.AddChild(actions);

        var isUnlocked = TradeRouteSystem.Instance.GetUnlockedRoutes().ContainsKey(template.RouteId);
        
        if (!isUnlocked)
        {
            var unlockBtn = new Button();
            unlockBtn.Text = $"Unlock ({template.BaseInvestment} Gold)";
            unlockBtn.Pressed += () => UnlockRoute(template.RouteId, template.BaseInvestment);
            actions.AddChild(unlockBtn);
        }
        else
        {
            var route = TradeRouteSystem.Instance.GetOrCreateRoute(template.RouteId);
            var investLabel = new Label();
            investLabel.Text = $"Invested: {route.CurrentInvestment}/{template.BaseInvestment * 10}";
            actions.AddChild(investLabel);

            var investBtn = new Button();
            investBtn.Text = "Invest 500 Gold";
            investBtn.Pressed += () => InvestRoute(template.RouteId, 500);
            actions.AddChild(investBtn);

            if (route.IsActive)
            {
                var tradeBtn = new Button();
                tradeBtn.Text = "Execute Trade";
                tradeBtn.Pressed += () => ExecuteTrade(template.RouteId);
                actions.AddChild(tradeBtn);

                var deactivateBtn = new Button();
                deactivateBtn.Text = "Deactivate";
                deactivateBtn.Pressed += () => TradeRouteSystem.Instance.DeactivateRoute(template.RouteId);
                actions.AddChild(deactivateBtn);
            }
            else
            {
                var activateBtn = new Button();
                activateBtn.Text = "Activate Route";
                activateBtn.Pressed += () => TradeRouteSystem.Instance.ActivateRoute(template.RouteId);
                actions.AddChild(activateBtn);
            }
        }

        return card;
    }

    private void UnlockRoute(string routeId, int cost)
    {
        if (Player.Instance.Gold >= cost)
        {
            Player.Instance.Gold -= cost;
            TradeRouteSystem.Instance.UnlockRoute(routeId);
            RefreshDisplay();
        }
    }

    private void InvestRoute(string routeId, int amount)
    {
        if (TradeRouteSystem.Instance.InvestInRoute(routeId, amount))
        {
            RefreshDisplay();
        }
    }

    private void ExecuteTrade(string routeId)
    {
        var record = TradeRouteSystem.Instance.ExecuteTrade(routeId);
        if (record != null)
        {
            RefreshDisplay();
        }
    }

    private void RefreshMarketDisplay()
    {
        foreach (var child in _marketContainer.GetChildren())
        {
            child.QueueFree();
        }

        var header = new Label();
        header.Text = "📊 Current Market Prices";
        header.AddThemeFontSizeOverride("font_size", 16);
        _marketContainer.AddChild(header);

        var market = TradeRouteSystem.Instance.GetMarketData();
        
        foreach (var kvp in market)
        {
            var goods = TradeRouteDatabase.Instance.GetGoods(kvp.Key);
            if (goods == null) continue;

            var hbox = new HBoxContainer();
            hbox.AddThemeConstantOverride("separation", 10);
            _marketContainer.AddChild(hbox);

            var nameLabel = new Label();
            nameLabel.Text = goods.Name;
            nameLabel.CustomMinimumSize = new Vector2(150, 0);
            hbox.AddChild(nameLabel);

            var categoryLabel = new Label();
            categoryLabel.Text = $"[{goods.Category}]";
            categoryLabel.Modulate = new Color(0.6f, 0.6f, 0.6f);
            categoryLabel.CustomMinimumSize = new Vector2(80, 0);
            hbox.AddChild(categoryLabel);

            var priceLabel = new Label();
            priceLabel.Text = $"{kvp.Value.CurrentPrice} Gold";
            priceLabel.CustomMinimumSize = new Vector2(100, 0);
            
            if (kvp.Value.Trend > 0) priceLabel.Modulate = new Color(0.3f, 1f, 0.3f);
            else if (kvp.Value.Trend < 0) priceLabel.Modulate = new Color(1f, 0.3f, 0.3f);
            else priceLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
            
            hbox.AddChild(priceLabel);

            var regionLabel = new Label();
            regionLabel.Text = $"Source: {goods.SourceRegions[0]}";
            regionLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
            hbox.AddChild(regionLabel);
        }
    }

    private void RefreshHistoryDisplay()
    {
        foreach (var child in _historyContainer.GetChildren())
        {
            child.QueueFree();
        }

        var header = new Label();
        header.Text = "📜 Trade History";
        header.AddThemeFontSizeOverride("font_size", 16);
        _historyContainer.AddChild(header);

        var history = TradeRouteSystem.Instance.GetTradeHistory();
        
        if (history.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "No trades yet. Activate a route and start trading!";
            emptyLabel.Modulate = new Color(0.6f, 0.6f, 0.6f);
            _historyContainer.AddChild(emptyLabel);
            return;
        }

        foreach (var record in history)
        {
            var hbox = new HBoxContainer();
            hbox.AddThemeConstantOverride("separation", 10);
            _historyContainer.AddChild(hbox);

            var routeLabel = new Label();
            routeLabel.Text = record.RouteName;
            routeLabel.CustomMinimumSize = new Vector2(200, 0);
            hbox.AddChild(routeLabel);

            var goodsLabel = new Label();
            goodsLabel.Text = record.GoodsName;
            goodsLabel.CustomMinimumSize = new Vector2(120, 0);
            hbox.AddChild(goodsLabel);

            var profitLabel = new Label();
            profitLabel.Text = $"+{record.Profit} Gold";
            profitLabel.Modulate = record.Profit > 0 ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.3f, 0.3f);
            profitLabel.CustomMinimumSize = new Vector2(100, 0);
            hbox.AddChild(profitLabel);
        }
    }

    private void OnTradeCompleted(TradeRouteData.TradeRecord record)
    {
        RefreshDisplay();
    }

    public void ToggleVisibility()
    {
        Visible = !Visible;
        if (Visible)
        {
            RefreshDisplay();
        }
    }

    public override void _Input(InputEvent evt)
    {
        if (evt is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            if (Visible)
            {
                ToggleVisibility();
            }
        }
    }
}
