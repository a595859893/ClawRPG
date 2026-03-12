using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class MarketTrendUI : Control
{
    private MarketTrendSystem _system;
    private VBoxContainer _mainContainer;
    private OptionButton _categorySelector;
    private Label _sentimentLabel;
    private Label _trendLabel;
    private Label _predictionLabel;
    private VBoxContainer _historyContainer;
    private VBoxContainer _allTrendsContainer;
    private Label _statsLabel;
    
    // Colors
    private Color _risingColor = new Color(0.2f, 0.8f, 0.2f);
    private Color _fallingColor = new Color(0.8f, 0.2f, 0.2f);
    private Color _stableColor = new Color(0.8f, 0.8f, 0.2f);
    private Color _volatileColor = new Color(0.8f, 0.4f, 0.2f);
    
    public override void _Ready()
    {
        _system = GetNode<MarketTrendSystem>("/root/MarketTrendSystem");
        
        // Create main container
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchor(0, 0, 1, 1);
        _mainContainer.Margin = new Rect2(20, 20, -20, -20);
        AddChild(_mainContainer);
        
        CreateHeader();
        CreateMarketOverview();
        CreateCategorySection();
        CreateAllTrendsSection();
        CreateStatsSection();
        
        // Connect close key
        SetProcessInput(true);
    }
    
    private void CreateHeader()
    {
        HBoxContainer header = new HBoxContainer();
        _mainContainer.AddChild(header);
        
        Label title = new Label();
        title.Text = "📈 市场趋势分析";
        title.AddColorOverride("font_color", new Color(1, 0.9, 0.5f));
        title.Set("custom_fonts/font", CreateBoldFont(24));
        header.AddChild(title);
        
        Control spacer = new Control();
        spacer.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        header.AddChild(spacer);
        
        Button closeBtn = new Button();
        closeBtn.Text = "✕";
        closeBtn.RectMinSize = new Vector2(30, 30);
        closeBtn.Connect("pressed", this, nameof(OnClosePressed));
        header.AddChild(closeBtn);
        
        HSeparator sep = new HSeparator();
        _mainContainer.AddChild(sep);
    }
    
    private void CreateMarketOverview()
    {
        HBoxContainer overview = new HBoxContainer();
        _mainContainer.AddChild(overview);
        
        // Market Sentiment
        VBoxContainer sentimentBox = new VBoxContainer();
        overview.AddChild(sentimentBox);
        
        Label sentimentTitle = new Label();
        sentimentTitle.Text = "市场情绪";
        sentimentTitle.Align = Label.AlignEnum.Center;
        sentimentBox.AddChild(sentimentTitle);
        
        _sentimentLabel = new Label();
        _sentimentLabel.Align = Label.AlignEnum.Center;
        _sentimentLabel.Set("custom_fonts/font", CreateBoldFont(20));
        sentimentBox.AddChild(_sentimentLabel);
        
        // Hot Categories
        VBoxContainer hotBox = new VBoxContainer();
        overview.AddChild(hotBox);
        
        Label hotTitle = new Label();
        hotTitle.Text = "热门品类";
        hotTitle.Align = Label.AlignEnum.Center;
        hotBox.AddChild(hotTitle);
        
        Label hotLabel = new Label();
        hotLabel.Align = Label.AlignEnum.Center;
        hotBox.AddChild(hotLabel);
        
        // Investment Opportunities
        VBoxContainer investBox = new VBoxContainer();
        overview.AddChild(investBox);
        
        Label investTitle = new Label();
        investTitle.Text = "投资机会";
        investTitle.Align = Label.AlignEnum.Center;
        investBox.AddChild(investTitle);
        
        Label investLabel = new Label();
        investLabel.Align = Label.AlignEnum.Center;
        investBox.AddChild(investLabel);
        
        // Update overview
        UpdateMarketOverview(sentimentLabel, hotLabel, investLabel);
        
        HSeparator sep = new HSeparator();
        _mainContainer.AddChild(sep);
    }
    
    private void UpdateMarketOverview(Label sentiment, Label hot, Label invest)
    {
        // Sentiment
        int sentimentValue = _system.GetMarketSentiment();
        string sentimentText = _system.GetSentimentText();
        sentiment.Text = sentimentText;
        
        if (sentimentValue > 20)
            sentiment.AddColorOverride("font_color", _risingColor);
        else if (sentimentValue < -20)
            sentiment.AddColorOverride("font_color", _fallingColor);
        else
            sentiment.AddColorOverride("font_color", _stableColor);
        
        // Hot categories
        var hotCategories = _system.GetHotCategories(3);
        hot.Text = string.Join("\n", hotCategories);
        
        // Investment opportunities
        var opportunities = _system.GetInvestmentOpportunities(3);
        invest.Text = string.Join("\n", opportunities);
    }
    
    private void CreateCategorySection()
    {
        HBoxContainer selectorRow = new HBoxContainer();
        _mainContainer.AddChild(selectorRow);
        
        Label selectorLabel = new Label();
        selectorLabel.Text = "选择品类: ";
        selectorRow.AddChild(selectorLabel);
        
        _categorySelector = new OptionButton();
        foreach (string category in MarketTrendDatabase.ItemCategories)
        {
            _categorySelector.AddItem(category);
        }
        _categorySelector.Connect("item_selected", this, nameof(OnCategorySelected));
        selectorRow.AddChild(_categorySelector);
        
        // Trend info
        _trendLabel = new Label();
        _trendLabel.Text = "";
        _mainContainer.AddChild(_trendLabel);
        
        // Prediction info
        _predictionLabel = new Label();
        _predictionLabel.Text = "";
        _mainContainer.AddChild(_predictionLabel);
        
        HSeparator sep = new HSeparator();
        _mainContainer.AddChild(sep);
    }
    
    private void CreateAllTrendsSection()
    {
        Label sectionTitle = new Label();
        sectionTitle.Text = "所有品类趋势";
        sectionTitle.Set("custom_fonts/font", CreateBoldFont(16));
        _mainContainer.AddChild(sectionTitle);
        
        ScrollContainer scroll = new ScrollContainer();
        scroll.RectMinSize = new Vector2(0, 150);
        _mainContainer.AddChild(scroll);
        
        _allTrendsContainer = new VBoxContainer();
        scroll.AddChild(_allTrendsContainer);
        
        UpdateAllTrends();
    }
    
    private void UpdateAllTrends()
    {
        // Clear existing
        foreach (Node child in _allTrendsContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add trend for each category
        foreach (string category in MarketTrendDatabase.ItemCategories)
        {
            MarketTrend trend = _system.GetTrend(category);
            if (trend == null) continue;
            
            HBoxContainer trendRow = new HBoxContainer();
            _allTrendsContainer.AddChild(trendRow);
            
            // Category name
            Label catLabel = new Label();
            catLabel.Text = category;
            catLabel.RectMinSize = new Vector2(100, 0);
            trendRow.AddChild(catLabel);
            
            // Direction
            Label dirLabel = new Label();
            string dirText = "";
            Color dirColor = _stableColor;
            
            switch (trend.Direction)
            {
                case TrendDirection.Rising:
                    dirText = "↑ 上涨";
                    dirColor = _risingColor;
                    break;
                case TrendDirection.Falling:
                    dirText = "↓ 下跌";
                    dirColor = _fallingColor;
                    break;
                case TrendDirection.Volatile:
                    dirText = "⚡ 波动";
                    dirColor = _volatileColor;
                    break;
                default:
                    dirText = "→ 稳定";
                    break;
            }
            
            dirLabel.Text = dirText;
            dirLabel.AddColorOverride("font_color", dirColor);
            dirLabel.RectMinSize = new Vector2(80, 0);
            trendRow.AddChild(dirLabel);
            
            // Change percent
            Label changeLabel = new Label();
            string changeText = $"{trend.ChangePercent:+0.0;-0.0;0}%";
            changeLabel.Text = changeText;
            changeLabel.AddColorOverride("font_color", trend.ChangePercent >= 0 ? _risingColor : _fallingColor);
            changeLabel.RectMinSize = new Vector2(60, 0);
            trendRow.AddChild(changeLabel);
            
            // Strength bar
            ProgressBar strengthBar = new ProgressBar();
            strengthBar.RectMinSize = new Vector2(100, 16);
            strengthBar.Value = trend.TrendStrength;
            strengthBar.MaxValue = 100;
            trendRow.AddChild(strengthBar);
        }
    }
    
    private void CreateStatsSection()
    {
        HSeparator sep = new HSeparator();
        _mainContainer.AddChild(sep);
        
        Label statsTitle = new Label();
        statsTitle.Text = "📊 统计数据";
        statsTitle.Set("custom_fonts/font", CreateBoldFont(16));
        _mainContainer.AddChild(statsTitle);
        
        _statsLabel = new Label();
        _statsLabel.Text = "";
        _mainContainer.AddChild(_statsLabel);
        
        UpdateStats();
    }
    
    private void UpdateStats()
    {
        var stats = _system.GetStatistics();
        
        float accuracy = stats.ContainsKey("PredictionAccuracy") ? stats["PredictionAccuracy"] : 0;
        
        _statsLabel.Text = $"趋势更新: {stats["TotalUpdates"]}\n" +
                          $"预测总数: {stats["TotalPredictions"]}\n" +
                          $"预测准确率: {accuracy:F1}%";
    }
    
    private void OnCategorySelected(int index)
    {
        string category = MarketTrendDatabase.ItemCategories[index];
        
        MarketTrend trend = _system.GetTrend(category);
        MarketPrediction prediction = _system.GetPrediction(category);
        
        if (trend != null)
        {
            string dirText = "";
            Color dirColor = _stableColor;
            
            switch (trend.Direction)
            {
                case TrendDirection.Rising:
                    dirText = "上涨 📈";
                    dirColor = _risingColor;
                    break;
                case TrendDirection.Falling:
                    dirText = "下跌 📉";
                    dirColor = _fallingColor;
                    break;
                case TrendDirection.Volatile:
                    dirText = "波动 ⚡";
                    dirColor = _volatileColor;
                    break;
                default:
                    dirText = "稳定 →";
                    break;
            }
            
            _trendLabel.Text = $"趋势: {dirText} ({trend.ChangePercent:+0.0;-0.0;0}%) | 强度: {trend.TrendStrength:F0}% | 波动率: {trend.Volatility:P0}";
            _trendLabel.AddColorOverride("font_color", dirColor);
        }
        
        if (prediction != null)
        {
            string predDirText = "";
            switch (prediction.PredictedDirection)
            {
                case TrendDirection.Rising:
                    predDirText = "上涨";
                    break;
                case TrendDirection.Falling:
                    predDirText = "下跌";
                    break;
                default:
                    predDirText = "稳定";
                    break;
            }
            
            _predictionLabel.Text = $"预测: {predDirText} ({prediction.PredictedChange:+0.0;-0.0;0}%) | 置信度: {prediction.Confidence:F0}%";
        }
    }
    
    private void OnClosePressed()
    {
        QueueFree();
    }
    
    public override void _Input(InputEvent eventEvent)
    {
        if (eventEvent.IsActionPressed("ui_cancel"))
        {
            QueueFree();
        }
    }
    
    private Font CreateBoldFont(int size)
    {
        DynamicFont font = new DynamicFont();
        font.Size = size;
        return font;
    }
}
