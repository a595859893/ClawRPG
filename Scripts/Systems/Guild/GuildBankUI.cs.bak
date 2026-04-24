using Godot;
using System;
using System.Collections.Generic;

public partial class GuildBankUI : Control {
    private static GuildBankUI Instance;
    
    // UI组件
    private VBoxContainer mainContainer;
    private HBoxContainer headerContainer;
    private TabContainer tabContainer;
    private GridContainer itemGrid;
    private VBoxContainer transactionList;
    private Label goldLabel;
    private Label slotsLabel;
    private Label permissionLabel;
    private LineEdit goldInput;
    private CheckButton anyoneWithdrawCheck;
    private OptionButton minLevelOption;
    private Button depositGoldBtn;
    private Button withdrawGoldBtn;
    private Button refreshBtn;
    private Button closeBtn;
    
    // 颜色
    private Color rarityCommon = new Color(0.7f, 0.7f, 0.7f);
    private Color rarityUncommon = new Color(0.4f, 0.8f, 0.4f);
    private Color rarityRare = new Color(0.4f, 0.6f, 1.0f);
    private Color rarityEpic = new Color(0.6f, 0.4f, 0.8f);
    private Color rarityLegendary = new Color(1.0f, 0.7f, 0.3f);
    
    public override void _Ready() {
        Instance = this;
        SetupUI();
        ConnectSignals();
        Hide();
    }
    
    private void SetupUI() {
        // 主容器
        mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainContainer.CustomMinimumSize = new Vector2(800, 600);
        mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(mainContainer);
        
        // 标题栏
        headerContainer = new HBoxContainer();
        mainContainer.AddChild(headerContainer);
        
        var titleLabel = new Label();
        titleLabel.Text = "🏦 公会银行";
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        headerContainer.AddChild(titleLabel);
        
        headerContainer.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        closeBtn = new Button();
        closeBtn.Text = "✕";
        closeBtn.TooltipText = "关闭 (ESC)";
        closeBtn.Pressed += () => Hide();
        headerContainer.AddChild(closeBtn);
        
        // 信息栏
        var infoContainer = new HBoxContainer();
        mainContainer.AddChild(infoContainer);
        
        goldLabel = new Label();
        goldLabel.Text = "💰 金币: 0";
        goldLabel.AddThemeFontSizeOverride("font_size", 18);
        infoContainer.AddChild(goldLabel);
        
        infoContainer.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        slotsLabel = new Label();
        slotsLabel.Text = "📦 物品: 0/50";
        slotsLabel.AddThemeFontSizeOverride("font_size", 18);
        infoContainer.AddChild(slotsLabel);
        
        // 金币操作栏
        var goldContainer = new HBoxContainer();
        mainContainer.AddChild(goldContainer);
        
        goldContainer.AddChild(new Label() { Text = "💰 金币: " });
        
        goldInput = new LineEdit();
        goldInput.PlaceholderText = "数量";
        goldInput.CustomMinimumSize = new Vector2(120, 0);
        goldContainer.AddChild(goldInput);
        
        depositGoldBtn = new Button();
        depositGoldBtn.Text = "存款";
        depositGoldBtn.Pressed += OnDepositGold;
        goldContainer.AddChild(depositGoldBtn);
        
        withdrawGoldBtn = new Button();
        withdrawGoldBtn.Text = "取回";
        withdrawGoldBtn.Pressed += OnWithdrawGold;
        goldContainer.AddChild(withdrawGoldBtn);
        
        // 权限设置
        var permContainer = new HBoxContainer();
        mainContainer.AddChild(permContainer);
        
        anyoneWithdrawCheck = new CheckButton();
        anyoneWithdrawCheck.Text = "允许所有人取回";
        anyoneWithdrawCheck.Toggled += OnPermissionChanged;
        permContainer.AddChild(anyoneWithdrawCheck);
        
        permContainer.AddChild(new Label() { Text = "  最低等级: " });
        
        minLevelOption = new OptionButton();
        minLevelOption.AddItem("会员 (Member)", 0);
        minLevelOption.AddItem("官员 (Officer)", 1);
        minLevelOption.AddItem("副会长 (ViceLeader)", 2);
        minLevelOption.AddItem("会长 (Leader)", 3);
        minLevelOption.Selected = 1;
        minLevelOption.ItemSelected += OnMinLevelChanged;
        permContainer.AddChild(minLevelOption);
        
        // 标签页
        tabContainer = new TabContainer();
        tabContainer.SetVExpandFlags(Control.SizeFlags.Expand);
        mainContainer.AddChild(tabContainer);
        
        // 物品页
        var itemsTab = new VBoxContainer();
        itemsTab.Name = "物品";
        tabContainer.AddChild(itemsTab);
        
        var itemScroll = new ScrollContainer();
        itemScroll.SetVExpandFlags(Control.SizeFlags.Expand);
        itemsTab.AddChild(itemScroll);
        
        itemGrid = new GridContainer();
        itemGrid.Columns = 5;
        itemGrid.AddThemeConstantOverride("h_separation", 10);
        itemGrid.AddThemeConstantOverride("v_separation", 10);
        itemScroll.AddChild(itemGrid);
        
        // 交易记录页
        var transactionsTab = new VBoxContainer();
        transactionsTab.Name = "记录";
        tabContainer.AddChild(transactionsTab);
        
        var transScroll = new ScrollContainer();
        transScroll.SetVExpandFlags(Control.SizeFlags.Expand);
        transactionsTab.AddChild(transScroll);
        
        transactionList = new VBoxContainer();
        transactionList.AddThemeConstantOverride("separation", 5);
        transScroll.AddChild(transactionList);
        
        // 统计页
        var statsTab = new VBoxContainer();
        statsTab.Name = "统计";
        tabContainer.AddChild(statsTab);
        
        var statsScroll = new ScrollContainer();
        statsScroll.SetVExpandFlags(Control.SizeFlags.Expand);
        statsTab.AddChild(statsScroll);
        
        permissionLabel = new Label();
        permissionLabel.Text = "";
        permissionLabel.AddThemeFontSizeOverride("font_size", 16);
        statsScroll.AddChild(permissionLabel);
        
        // 刷新按钮
        refreshBtn = new Button();
        refreshBtn.Text = "🔄 刷新";
        refreshBtn.Pressed += RefreshDisplay;
        mainContainer.AddChild(refreshBtn);
        
        // 底部提示
        var tipLabel = new Label();
        tipLabel.Text = "按 ESC 键关闭";
        tipLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        mainContainer.AddChild(tipLabel);
    }
    
    private void ConnectSignals() {
        GuildBankSystem.Instance.ItemDeposited += OnBankUpdated;
        GuildBankSystem.Instance.ItemWithdrawn += OnBankUpdated;
        GuildBankSystem.Instance.GoldDeposited += OnBankUpdated;
        GuildBankSystem.Instance.GoldWithdrawn += OnBankUpdated;
    }
    
    public override void _Input(InputEvent e) {
        if (e.IsActionPressed("ui_cancel") && IsVisibleInTree()) {
            Hide();
        }
    }
    
    public void ShowBank() {
        if (GuildSystem.Instance?.CurrentGuild == null) {
            GD.PrintErr("玩家不在公会中");
            return;
        }
        
        RefreshDisplay();
        Show();
        GrabFocus();
    }
    
    private void RefreshDisplay() {
        var bank = GuildBankSystem.Instance;
        var data = bank.BankData;
        
        // 更新金币显示
        goldLabel.Text = $"💰 金币: {data.GoldDeposit:N0}";
        slotsLabel.Text = $"📦 物品: {data.Items.Count}/{bank.MaxSlots}";
        
        // 更新物品网格
        UpdateItemGrid();
        
        // 更新交易记录
        UpdateTransactionList();
        
        // 更新权限显示
        UpdatePermissionDisplay();
    }
    
    private void UpdateItemGrid() {
        // 清除现有物品
        foreach (var child in itemGrid.GetChildren()) {
            child.QueueFree();
        }
        
        var data = GuildBankSystem.Instance.BankData;
        
        // 添加物品槽
        for (int i = 0; i < data.Items.Count; i++) {
            var item = data.Items[i];
            var slot = CreateItemSlot(item, i);
            itemGrid.AddChild(slot);
        }
        
        // 添加空槽位
        int emptySlots = GuildBankSystem.Instance.MaxSlots - data.Items.Count;
        for (int i = 0; i < emptySlots && i < 10; i++) {
            var emptySlot = CreateEmptySlot();
            itemGrid.AddChild(emptySlot);
        }
    }
    
    private Control CreateItemSlot(GuildBankItem item, int index) {
        var container = new VBoxContainer();
        container.CustomMinimumSize = new Vector2(100, 100);
        
        // 物品图标背景
        var bg = new PanelContainer();
        bg.AddThemeStyleboxOverride("panel", CreateSlotStyle(item.Rarity));
        container.AddChild(bg);
        
        // 物品名称
        var nameLabel = new Label();
        nameLabel.Text = item.ItemName;
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        nameLabel.AddThemeFontSizeOverride("font_size", 12);
        nameLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        container.AddChild(nameLabel);
        
        // 数量
        var qtyLabel = new Label();
        qtyLabel.Text = $"x{item.Quantity}";
        qtyLabel.HorizontalAlignment = HorizontalAlignment.Center;
        qtyLabel.AddThemeFontSizeOverride("font_size", 14);
        container.AddChild(qtyLabel);
        
        // 存放者
        var depositorLabel = new Label();
        depositorLabel.Text = $"📤 {item.DepositorName}";
        depositorLabel.HorizontalAlignment = HorizontalAlignment.Center;
        depositorLabel.AddThemeFontSizeOverride("font_size", 10);
        depositorLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
        container.AddChild(depositorLabel);
        
        // 取回按钮
        var withdrawBtn = new Button();
        withdrawBtn.Text = "取回";
        withdrawBtn.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        withdrawBtn.Pressed += () => OnWithdrawItem(index);
        
        // 检查权限
        if (!GuildBankSystem.Instance.CanWithdraw()) {
            withdrawBtn.Disabled = true;
            withdrawBtn.TooltipText = "没有取回权限";
        }
        
        container.AddChild(withdrawBtn);
        
        return container;
    }
    
    private Control CreateEmptySlot() {
        var container = new VBoxContainer();
        container.CustomMinimumSize = new Vector2(100, 100);
        
        var bg = new PanelContainer();
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        style.BorderColor = new Color(0.3f, 0.3f, 0.3f);
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(4);
        bg.AddThemeStyleboxOverride("panel", style);
        container.AddChild(bg);
        
        var emptyLabel = new Label();
        emptyLabel.Text = "空";
        emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
        emptyLabel.AddThemeColorOverride("font_color", new Color(0.4f, 0.4f, 0.4f));
        container.AddChild(emptyLabel);
        
        return container;
    }
    
    private StyleBoxFlat CreateSlotStyle(string rarity) {
        var style = new StyleBoxFlat();
        style.SetCornerRadiusAll(8);
        style.SetBorderWidthAll(2);
        
        Color color;
        switch (rarity.ToLower()) {
            case "common": color = rarityCommon; break;
            case "uncommon": color = rarityUncommon; break;
            case "rare": color = rarityRare; break;
            case "epic": color = rarityEpic; break;
            case "legendary": color = rarityLegendary; break;
            default: color = rarityCommon; break;
        }
        
        style.BgColor = color * new Color(0.3f, 0.3f, 0.3f);
        style.BorderColor = color;
        
        return style;
    }
    
    private void UpdateTransactionList() {
        // 清除现有记录
        foreach (var child in transactionList.GetChildren()) {
            child.QueueFree();
        }
        
        var transactions = GuildBankSystem.Instance.GetRecentTransactions(20);
        
        foreach (var trans in transactions) {
            var label = new Label();
            
            string icon = trans.Type == "deposit" || trans.Type == "gold_deposit" ? "📥" : "📤";
            string timeStr = trans.Time.ToString("MM-dd HH:mm");
            
            label.Text = $"{icon} {timeStr} - {trans.PlayerName} {trans.Type}: {trans.ItemName} x{trans.Quantity}";
            label.AddThemeFontSizeOverride("font_size", 12);
            
            transactionList.AddChild(label);
        }
        
        if (transactions.Count == 0) {
            var emptyLabel = new Label();
            emptyLabel.Text = "暂无交易记录";
            emptyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
            transactionList.AddChild(emptyLabel);
        }
    }
    
    private void UpdatePermissionDisplay() {
        var data = GuildBankSystem.Instance.BankData;
        anyoneWithdrawCheck.ButtonPressed = data.AnyoneCanWithdraw;
        
        string levelName = data.MinWithdrawLevel switch {
            0 => "会员",
            1 => "官员",
            2 => "副会长",
            3 => "会长",
            _ => "未知"
        };
        
        permissionLabel.Text = $@"
📊 银行统计
═══════════════
物品数量: {data.Items.Count}
最大容量: {GuildBankSystem.Instance.MaxSlots}
当前存款: {data.GoldDeposit:N0}
历史总存款: {data.TotalDeposits:N0}
交易记录: {data.Transactions.Count}

⚙️ 权限设置
═══════════════
自由取回: {(data.AnyoneCanWithdraw ? "是" : "否")}
最低取回等级: {levelName}
";
        
        // 权限控制
        var playerLevel = GuildSystem.Instance.PlayerData.Level;
        bool isLeader = playerLevel == GuildLevel.Leader;
        
        anyoneWithdrawCheck.Visible = isLeader;
        minLevelOption.Visible = isLeader;
        withdrawGoldBtn.Disabled = !GuildBankSystem.Instance.CanWithdraw();
    }
    
    private void OnDepositGold() {
        if (int.TryParse(goldInput.Text, out int amount) && amount > 0) {
            if (GuildBankSystem.Instance.DepositGold(amount)) {
                goldInput.Text = "";
            }
        }
    }
    
    private void OnWithdrawGold() {
        if (int.TryParse(goldInput.Text, out int amount) && amount > 0) {
            if (GuildBankSystem.Instance.WithdrawGold(amount)) {
                goldInput.Text = "";
            }
        }
    }
    
    private void OnWithdrawItem(int slotIndex) {
        GuildBankSystem.Instance.WithdrawItem(slotIndex);
    }
    
    private void OnPermissionChanged(bool toggled) {
        if (GuildSystem.Instance.PlayerData.Level != GuildLevel.Leader) return;
        
        GuildBankSystem.Instance.SetWithdrawPermission(toggled, minLevelOption.GetSelectedId() + 1);
    }
    
    private void OnMinLevelChanged(long index) {
        if (GuildSystem.Instance.PlayerData.Level != GuildLevel.Leader) return;
        
        GuildBankSystem.Instance.SetWithdrawPermission(anyoneWithdrawCheck.ButtonPressed, (int)index + 1);
    }
    
    private void OnBankUpdated() {
        RefreshDisplay();
    }
    
    public static void Toggle() {
        if (Instance == null) return;
        
        if (Instance.IsVisibleInTree()) {
            Instance.Hide();
        } else {
            Instance.ShowBank();
        }
    }
}
