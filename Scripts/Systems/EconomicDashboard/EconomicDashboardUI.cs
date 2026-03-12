using Godot;
using System;
using System.Collections.Generic;

public class EconomicDashboardUI : Control
{
    private EconomicDashboardSystem system;
    private Label titleLabel;
    private TabContainer tabContainer;
    
    // 概览标签
    private Label totalGoldLabel;
    private Label earnedLabel;
    private Label spentLabel;
    private Label netChangeLabel;
    private Label healthLabel;
    private Label statusLabel;
    private Label goldPerMinuteLabel;
    private Label inflationLabel;
    
    // 收入标签
    private VBoxContainer earningsContainer;
    private Label combatEarningsLabel;
    private Label questEarningsLabel;
    private Label craftingEarningsLabel;
    private Label tradingEarningsLabel;
    private Label eventEarningsLabel;
    
    // 支出标签
    private VBoxContainer expensesContainer;
    private Label purchaseExpensesLabel;
    private Label repairExpensesLabel;
    private Label upgradeExpensesLabel;
    private Label craftingCostsLabel;
    private Label auctionFeesLabel;
    
    // 交易记录标签
    private VBoxContainer transactionsContainer;
    
    // 物品统计标签
    private Label itemsSoldLabel;
    private Label itemsPurchasedLabel;
    private Label itemsCraftedLabel;
    
    private bool isVisible = false;
    private int systemCount = 0;
    
    public override void _Ready()
    {
        system = GetNode<EconomicDashboardSystem>("/root/Main/EconomicDashboardSystem");
        if (system != null)
        {
            system.OnEconomicUpdate += OnEconomicUpdate;
        }
        
        SetupUI();
        UpdateDisplay();
    }
    
    private void SetupUI()
    {
        // 主容器
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchor(AnchorPreset.FullRect);
        mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(mainContainer);
        
        // 标题栏
        var header = new HBoxContainer();
        mainContainer.AddChild(header);
        
        titleLabel = new Label();
        titleLabel.Text = " 📊 经济监控面板";
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        header.AddChild(titleLabel);
        
        header.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlagsExpand });
        
        // 关闭按钮
        var closeBtn = new Button();
        closeBtn.Text = "✕";
        closeBtn.TooltipText = "关闭 (ESC)";
        closeBtn.Pressed += () => ToggleVisibility(false);
        header.AddChild(closeBtn);
        
        // Tab容器
        tabContainer = new TabContainer();
        tabContainer.SetVExpand(ExpandMode.Fill);
        tabContainer.SetHExpand(ExpandMode.Fill);
        mainContainer.AddChild(tabContainer);
        
        // ===== 概览标签 =====
        var overviewTab = new ScrollContainer();
        overviewTab.Name = "概览";
        tabContainer.AddChild(overviewTab);
        
        var overviewContainer = new VBoxContainer();
        overviewContainer.SetAnchor(AnchorPreset.FullRect);
        overviewContainer.AddThemeConstantOverride("separation", 15);
        overviewTab.AddChild(overviewContainer);
        
        // 经济状态卡片
        var healthCard = CreateStatCard("经济状态");
        statusLabel = new Label();
        statusLabel.Text = "经济健康";
        statusLabel.AddThemeFontSizeOverride("font_size", 28);
        healthCard.AddChild(statusLabel);
        
        healthLabel = new Label();
        healthLabel.Text = "健康度: 100%";
        healthLabel.AddThemeFontSizeOverride("font_size", 18);
        healthCard.AddChild(healthLabel);
        overviewContainer.AddChild(healthCard);
        
        // 金币流通卡片
        var goldCard = CreateStatCard("金币流通");
        totalGoldLabel = new Label();
        totalGoldLabel.Text = "流通总量: 0";
        totalGoldLabel.AddThemeFontSizeOverride("font_size", 20);
        goldCard.AddChild(totalGoldLabel);
        
        earnedLabel = new Label();
        earnedLabel.Text = "总收入: 0";
        earnedLabel.AddThemeFontSizeOverride("font_size", 18);
        goldCard.AddChild(earnedLabel);
        
        spentLabel = new Label();
        spentLabel.Text = "总支出: 0";
        spentLabel.AddThemeFontSizeOverride("font_size", 18);
        goldCard.AddChild(spentLabel);
        
        netChangeLabel = new Label();
        netChangeLabel.Text = "净变化: 0";
        netChangeLabel.AddThemeFontSizeOverride("font_size", 18);
        goldCard.AddChild(netChangeLabel);
        overviewContainer.AddChild(goldCard);
        
        // 统计卡片
        var statsCard = CreateStatCard("经济指标");
        goldPerMinuteLabel = new Label();
        goldPerMinuteLabel.Text = "金币/分钟: 0";
        goldPerMinuteLabel.AddThemeFontSizeOverride("font_size", 18);
        statsCard.AddChild(goldPerMinuteLabel);
        
        inflationLabel = new Label();
        inflationLabel.Text = "通胀率: 0%";
        inflationLabel.AddThemeFontSizeOverride("font_size", 18);
        statsCard.AddChild(inflationLabel);
        overviewContainer.AddChild(statsCard);
        
        // ===== 收入标签 =====
        var earningsTab = new ScrollContainer();
        earningsTab.Name = "收入";
        tabContainer.AddChild(earningsTab);
        
        earningsContainer = new VBoxContainer();
        earningsContainer.SetAnchor(AnchorPreset.FullRect);
        earningsContainer.AddThemeConstantOverride("separation", 10);
        earningsTab.AddChild(earningsContainer);
        
        combatEarningsLabel = CreateEarningRow("⚔️ 战斗收入", "0");
        questEarningsLabel = CreateEarningRow("📜 任务奖励", "0");
        craftingEarningsLabel = CreateEarningRow("🔨 制作收入", "0");
        tradingEarningsLabel = CreateEarningRow("💰 交易收入", "0");
        eventEarningsLabel = CreateEarningRow("🎉 活动奖励", "0");
        
        // ===== 支出标签 =====
        var expensesTab = new ScrollContainer();
        expensesTab.Name = "支出";
        tabContainer.AddChild(expensesTab);
        
        expensesContainer = new VBoxContainer();
        expensesContainer.SetAnchor(AnchorPreset.FullRect);
        expensesContainer.AddThemeConstantOverride("separation", 10);
        expensesTab.AddChild(expensesContainer);
        
        purchaseExpensesLabel = CreateEarningRow("🛒 购买支出", "0");
        repairExpensesLabel = CreateEarningRow("🔧 修理支出", "0");
        upgradeExpensesLabel = CreateEarningRow("⬆️ 升级支出", "0");
        craftingCostsLabel = CreateEarningRow("🔨 制作费用", "0");
        auctionFeesLabel = CreateEarningRow("📦 拍卖手续费", "0");
        
        // ===== 交易记录标签 =====
        var transactionsTab = new ScrollContainer();
        transactionsTab.Name = "交易记录";
        tabContainer.AddChild(transactionsTab);
        
        transactionsContainer = new VBoxContainer();
        transactionsContainer.SetAnchor(AnchorPreset.FullRect);
        transactionsContainer.AddThemeConstantOverride("separation", 5);
        transactionsTab.AddChild(transactionsContainer);
        
        // ===== 物品统计标签 =====
        var itemsTab = new ScrollContainer();
        itemsTab.Name = "物品统计";
        tabContainer.AddChild(itemsTab);
        
        var itemsContainer = new VBoxContainer();
        itemsContainer.SetAnchor(AnchorPreset.FullRect);
        itemsContainer.AddThemeConstantOverride("separation", 15);
        itemsTab.AddChild(itemsContainer);
        
        var itemsCard = CreateStatCard("物品交易统计");
        itemsSoldLabel = new Label();
        itemsSoldLabel.Text = "售出: 0";
        itemsSoldLabel.AddThemeFontSizeOverride("font_size", 20);
        itemsCard.AddChild(itemsSoldLabel);
        
        itemsPurchasedLabel = new Label();
        itemsPurchasedLabel.Text = "购买: 0";
        itemsPurchasedLabel.AddThemeFontSizeOverride("font_size", 20);
        itemsCard.AddChild(itemsPurchasedLabel);
        
        itemsCraftedLabel = new Label();
        itemsCraftedLabel.Text = "制作: 0";
        itemsCraftedLabel.AddThemeFontSizeOverride("font_size", 20);
        itemsCard.AddChild(itemsCraftedLabel);
        
        itemsContainer.AddChild(itemsCard);
        
        // 初始隐藏
        Visible = false;
    }
    
    private PanelContainer CreateStatCard(string title)
    {
        var card = new PanelContainer();
        card.AddThemeStyleboxOverride("panel", CreateCardStyle());
        
        var container = new VBoxContainer();
        container.AddThemeConstantOverride("separation", 10);
        
        var titleLabel = new Label();
        titleLabel.Text = title;
        titleLabel.AddThemeFontSizeOverride("font_size", 18);
        titleLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
        container.AddChild(titleLabel);
        
        card.AddChild(container);
        return card;
    }
    
    private Label CreateEarningRow(string name, string value)
    {
        var container = new HBoxContainer();
        container.AddThemeConstantOverride("separation", 20);
        
        var nameLabel = new Label();
        nameLabel.Text = name;
        nameLabel.SizeFlagsHorizontal = Control.SizeFlagsExpand;
        container.AddChild(nameLabel);
        
        var valueLabel = new Label();
        valueLabel.Text = value;
        valueLabel.HorizontalAlignment = HorizontalAlignment.Right;
        container.AddChild(valueLabel);
        
        if (earningsContainer != null)
            earningsContainer.AddChild(container);
        if (expensesContainer != null)
            expensesContainer.AddChild(container);
        
        return valueLabel;
    }
    
    private StyleBoxFlat CreateCardStyle()
    {
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
        style.SetContentMarginAll(15);
        return style;
    }
    
    public void ToggleVisibility(bool? forceVisible = null)
    {
        if (forceVisible.HasValue)
        {
            isVisible = forceVisible.Value;
        }
        else
        {
            isVisible = !isVisible;
        }
        
        Visible = isVisible;
        
        if (isVisible)
        {
            UpdateDisplay();
        }
    }
    
    private void UpdateDisplay()
    {
        if (system == null) return;
        
        var data = system.GetData();
        
        // 概览更新
        if (totalGoldLabel != null)
            totalGoldLabel.Text = $"流通总量: {FormatNumber(data.TotalGoldInCirculation)}";
        
        if (earnedLabel != null)
            earnedLabel.Text = $"总收入: {FormatNumber(data.TotalGoldEarned)}";
        
        if (spentLabel != null)
            spentLabel.Text = $"总支出: {FormatNumber(data.TotalGoldSpent)}";
        
        if (netChangeLabel != null)
        {
            string sign = data.NetGoldChange >= 0 ? "+" : "";
            netChangeLabel.Text = $"净变化: {sign}{FormatNumber(data.NetGoldChange)}";
            netChangeLabel.Modulate = data.NetGoldChange >= 0 ? new Color(0.3f, 0.9f, 0.3f) : new Color(0.9f, 0.3f, 0.3f);
        }
        
        if (healthLabel != null)
            healthLabel.Text = $"健康度: {data.EconomicHealth:F1}%";
        
        if (statusLabel != null)
        {
            statusLabel.Text = system.GetEconomicStatus();
            // 根据状态设置颜色
            if (data.EconomicHealth >= 80f)
                statusLabel.Modulate = new Color(0.3f, 0.9f, 0.3f);
            else if (data.EconomicHealth >= 60f)
                statusLabel.Modulate = new Color(0.9f, 0.9f, 0.3f);
            else if (data.EconomicHealth >= 40f)
                statusLabel.Modulate = new Color(0.9f, 0.7f, 0.3f);
            else
                statusLabel.Modulate = new Color(0.9f, 0.3f, 0.3f);
        }
        
        if (goldPerMinuteLabel != null)
            goldPerMinuteLabel.Text = $"金币/分钟: {data.GoldPerMinute:F1}";
        
        if (inflationLabel != null)
        {
            inflationLabel.Text = $"通胀率: {data.InflationRate:F2}%";
            inflationLabel.Modulate = data.InflationRate > 0 ? new Color(0.9f, 0.3f, 0.3f) : new Color(0.3f, 0.9f, 0.3f);
        }
        
        // 收入更新
        if (combatEarningsLabel != null)
            combatEarningsLabel.Text = FormatNumber(data.CombatEarnings);
        if (questEarningsLabel != null)
            questEarningsLabel.Text = FormatNumber(data.QuestRewards);
        if (craftingEarningsLabel != null)
            craftingEarningsLabel.Text = FormatNumber(data.CraftingEarnings);
        if (tradingEarningsLabel != null)
            tradingEarningsLabel.Text = FormatNumber(data.TradingEarnings);
        if (eventEarningsLabel != null)
            eventEarningsLabel.Text = FormatNumber(data.EventRewards);
        
        // 支出更新
        if (purchaseExpensesLabel != null)
            purchaseExpensesLabel.Text = FormatNumber(data.PurchaseExpenses);
        if (repairExpensesLabel != null)
            repairExpensesLabel.Text = FormatNumber(data.RepairExpenses);
        if (upgradeExpensesLabel != null)
            upgradeExpensesLabel.Text = FormatNumber(data.UpgradeExpenses);
        if (craftingCostsLabel != null)
            craftingCostsLabel.Text = FormatNumber(data.CraftingCosts);
        if (auctionFeesLabel != null)
            auctionFeesLabel.Text = FormatNumber(data.AuctionFees);
        
        // 物品统计更新
        if (itemsSoldLabel != null)
            itemsSoldLabel.Text = $"售出: {data.ItemsSold}";
        if (itemsPurchasedLabel != null)
            itemsPurchasedLabel.Text = $"购买: {data.ItemsPurchased}";
        if (itemsCraftedLabel != null)
            itemsCraftedLabel.Text = $"制作: {data.ItemsCrafted}";
        
        // 更新交易记录
        UpdateTransactionHistory();
    }
    
    private void UpdateTransactionHistory()
    {
        if (transactionsContainer == null || system == null) return;
        
        // 清除旧记录
        foreach (var child in transactionsContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        var transactions = system.GetRecentTransactions(30);
        
        foreach (var tx in transactions)
        {
            var container = new HBoxContainer();
            container.AddThemeConstantOverride("separation", 15);
            
            var typeLabel = new Label();
            typeLabel.Text = tx.Type == "收入" ? "💰" : "💸";
            typeLabel.SizeFlagsHorizontal = Control.SizeFlagsShrinkEnd;
            container.AddChild(typeLabel);
            
            var amountLabel = new Label();
            string sign = tx.Amount >= 0 ? "+" : "";
            amountLabel.Text = $"{sign}{FormatNumber(Mathf.Abs(tx.Amount))}";
            amountLabel.Modulate = tx.Amount >= 0 ? new Color(0.3f, 0.9f, 0.3f) : new Color(0.9f, 0.3f, 0.3f);
            amountLabel.SizeFlagsHorizontal = Control.SizeFlagsShrinkEnd;
            container.AddChild(amountLabel);
            
            var descLabel = new Label();
            descLabel.Text = tx.Description;
            descLabel.SizeFlagsHorizontal = Control.SizeFlagsExpand;
            container.AddChild(descLabel);
            
            var timeLabel = new Label();
            timeLabel.Text = FormatTimestamp(tx.Timestamp);
            timeLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
            timeLabel.SizeFlagsHorizontal = Control.SizeFlagsShrinkEnd;
            container.AddChild(timeLabel);
            
            transactionsContainer.AddChild(container);
        }
    }
    
    private string FormatNumber(long number)
    {
        if (Mathf.Abs(number) >= 1000000000)
            return $"{number / 1000000000.0:F2}B";
        else if (Mathf.Abs(number) >= 1000000)
            return $"{number / 1000000.0:F2}M";
        else if (Mathf.Abs(number) >= 1000)
            return $"{number / 1000.0:F1}K";
        else
            return number.ToString();
    }
    
    private string FormatTimestamp(long timestamp)
    {
        var dt = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        var now = DateTimeOffset.UtcNow;
        var diff = now - dt;
        
        if (diff.TotalMinutes < 1)
            return "刚刚";
        else if (diff.TotalHours < 1)
            return $"{(int)diff.TotalMinutes}分钟前";
        else if (diff.TotalDays < 1)
            return $"{(int)diff.TotalHours}小时前";
        else
            return dt.ToString("MM-dd HH:mm");
    }
    
    private void OnEconomicUpdate(EconomicDashboardData data)
    {
        if (isVisible)
        {
            UpdateDisplay();
        }
    }
    
    public void SetSystemCount(int count)
    {
        systemCount = count;
        if (titleLabel != null)
        {
            titleLabel.Text = $" 📊 经济监控面板 (系统 #{systemCount})";
        }
    }
    
    public override void _Input(InputEvent eventArgs)
    {
        if (eventArgs is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Escape && isVisible)
            {
                ToggleVisibility(false);
                GetTree().SetInputAsHandled();
            }
        }
    }
}
