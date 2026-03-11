using Godot;
using System;
using System.Collections.Generic;

public partial class TradeUI : Control {
    private static TradeUI Instance { get; set; }

    // UI 组件
    private Label titleLabel;
    private Label statusLabel;
    private Label player1NameLabel;
    private Label player2NameLabel;
    private Label player1GoldLabel;
    private Label player2GoldLabel;
    private Label player1ValueLabel;
    private Label player2ValueLabel;
    private Button acceptButton;
    private Button cancelButton;
    private Button closeButton;
    private HBoxContainer player1ItemsContainer;
    private HBoxContainer player2ItemsContainer;
    private VBoxContainer player1Panel;
    private VBoxContainer player2Panel;

    // 交易物品网格
    private GridContainer player1Grid;
    private GridContainer player2Grid;

    public override void _Ready() {
        Instance = this;
        SetupUI();
        ConnectSignals();
        Visible = false;

        TradeSystem.Instance.Connect(TradeSystem.SignalName.TradeStarted, Callable.From(OnTradeStarted));
        TradeSystem.Instance.Connect(TradeSystem.SignalName.OfferUpdated, Callable.From(OnOfferUpdated));
        TradeSystem.Instance.Connect(TradeSystem.SignalName.TradeCompleted, Callable.From(OnTradeCompleted));
        TradeSystem.Instance.Connect(TradeSystem.SignalName.TradeCancelled, Callable.From(OnTradeCancelled));
    }

    private void SetupUI() {
        // 主容器
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainContainer.CustomMinimumSize = new Vector2(800, 500);
        AddChild(mainContainer);

        // 标题栏
        var titleBar = new HBoxContainer();
        mainContainer.AddChild(titleBar);

        titleLabel = new Label();
        titleLabel.Text = "  交易系统  ";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        titleBar.AddChild(titleLabel);

        closeButton = new Button();
        closeButton.Text = "X";
        closeButton.CustomMinimumSize = new Vector2(40, 40);
        closeButton.Pressed += OnClosePressed;
        titleBar.AddChild(closeButton);

        // 状态标签
        statusLabel = new Label();
        statusLabel.Text = "请发起交易";
        statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
        mainContainer.AddChild(statusLabel);

        // 交易面板
        var tradePanels = new HBoxContainer();
        tradePanels.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(tradePanels);

        // 玩家1面板（本地玩家）
        player1Panel = new VBoxContainer();
        player1Panel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        player1Panel.Modulate = new Color(0.8, 1, 0.8);
        tradePanels.AddChild(player1Panel);

        player1NameLabel = new Label();
        player1NameLabel.Text = "玩家";
        player1NameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        player1Panel.AddChild(player1NameLabel);

        player1GoldLabel = new Label();
        player1GoldLabel.Text = "金币: 0";
        player1GoldLabel.HorizontalAlignment = HorizontalAlignment.Center;
        player1Panel.AddChild(player1GoldLabel);

        player1ValueLabel = new Label();
        player1ValueLabel.Text = "交易价值: 0";
        player1ValueLabel.HorizontalAlignment = HorizontalAlignment.Center;
        player1Panel.AddChild(player1ValueLabel);

        var scroll1 = new ScrollContainer();
        scroll1.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        player1Panel.AddChild(scroll1);

        player1Grid = new GridContainer();
        player1Grid.Columns = 4;
        scroll1.AddChild(player1Grid);

        // 交易中间区域
        var centerPanel = new VBoxContainer();
        centerPanel.CustomMinimumSize = new Vector2(150, 0);
        tradePanels.AddChild(centerPanel);

        var spacer1 = new Control();
        spacer1.CustomMinimumSize = new Vector2(0, 50);
        centerPanel.AddChild(spacer1);

        var versusLabel = new Label();
        versusLabel.Text = "⟷";
        versusLabel.HorizontalAlignment = HorizontalAlignment.Center;
        versusLabel.FontSize = 32;
        centerPanel.AddChild(versusLabel);

        var spacer2 = new Control();
        spacer2.CustomMinimumSize = new Vector2(0, 50);
        centerPanel.AddChild(spacer2);

        // 玩家2面板（交易对手）
        player2Panel = new VBoxContainer();
        player2Panel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        player2Panel.Modulate = new Color(0.8, 0.8, 1);
        tradePanels.AddChild(player2Panel);

        player2NameLabel = new Label();
        player2NameLabel.Text = "交易对手";
        player2NameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        player2Panel.AddChild(player2NameLabel);

        player2GoldLabel = new Label();
        player2GoldLabel.Text = "金币: 0";
        player2GoldLabel.HorizontalAlignment = HorizontalAlignment.Center;
        player2Panel.AddChild(player2GoldLabel);

        player2ValueLabel = new Label();
        player2ValueLabel.Text = "交易价值: 0";
        player2ValueLabel.HorizontalAlignment = HorizontalAlignment.Center;
        player2Panel.AddChild(player2ValueLabel);

        var scroll2 = new ScrollContainer();
        scroll2.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        player2Panel.AddChild(scroll2);

        player2Grid = new GridContainer();
        player2Grid.Columns = 4;
        scroll2.AddChild(player2Grid);

        // 按钮栏
        var buttonBar = new HBoxContainer();
        mainContainer.AddChild(buttonBar);

        var spacer3 = new Control();
        spacer3.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        buttonBar.AddChild(spacer3);

        acceptButton = new Button();
        acceptButton.Text = "  接受交易  ";
        acceptButton.CustomMinimumSize = new Vector2(150, 50);
        acceptButton.Pressed += OnAcceptPressed;
        buttonBar.AddChild(acceptButton);

        cancelButton = new Button();
        cancelButton.Text = "  取消交易  ";
        cancelButton.CustomMinimumSize = new Vector2(150, 50);
        cancelButton.Pressed += OnCancelPressed;
        buttonBar.AddChild(cancelButton);

        var spacer4 = new Control();
        spacer4.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        buttonBar.AddChild(spacer4);
    }

    private void ConnectSignals() {
        // 信号连接在 _Ready 中处理
    }

    public override void _Input(InputEvent e) {
        if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.T) {
            if (Visible) {
                HideTrade();
            } else {
                // 模拟开始与 NPC 交易
                StartNPCTrade();
            }
        }
    }

    private void StartNPCTrade() {
        // 与 NPC 商人进行模拟交易
        TradeSystem.Instance.StartTrade("npc_merchant", "商人");
        UpdateUI();
        ShowTrade();
    }

    private void ShowTrade() {
        Visible = true;
        UpdateUI();
    }

    private void HideTrade() {
        Visible = false;
    }

    private void UpdateUI() {
        var offer = TradeSystem.Instance.CurrentOffer;
        if (offer == null) {
            statusLabel.Text = "请发起交易 (T键)";
            return;
        }

        player1NameLabel.Text = offer.Player1Name;
        player2NameLabel.Text = offer.Player2Name;
        player1GoldLabel.Text = "金币: " + offer.Player1Gold;
        player2GoldLabel.Text = "金币: " + offer.Player2Gold;
        player1ValueLabel.Text = "交易价值: " + offer.GetTotalValue(true);
        player2ValueLabel.Text = "交易价值: " + offer.GetTotalValue(false);

        // 更新物品网格
        UpdateItemGrid(player1Grid, offer.Player1Items, true);
        UpdateItemGrid(player2Grid, offer.Player2Items, false);

        // 更新状态
        if (offer.Player1Accepted && offer.Player2Accepted) {
            statusLabel.Text = "交易已完成！";
            acceptButton.Text = "  完成  ";
        } else if (offer.Player1Accepted) {
            statusLabel.Text = "等待对方接受...";
            acceptButton.Text = "  取消接受  ";
        } else if (offer.Player2Accepted) {
            statusLabel.Text = "等待您接受...";
            acceptButton.Text = "  接受交易  ";
        } else {
            statusLabel.Text = "正在报价...";
            acceptButton.Text = "  接受交易  ";
        }
    }

    private void UpdateItemGrid(GridContainer grid, List<ItemData> items, bool isPlayer1) {
        // 清除现有项
        foreach (var child in grid.GetChildren()) {
            child.QueueFree();
        }

        // 添加物品项
        foreach (var item in items) {
            var itemButton = CreateItemButton(item, isPlayer1);
            grid.AddChild(itemButton);
        }

        // 添加空白占位
        var emptySlots = 8 - items.Count;
        for (int i = 0; i < Math.Max(0, emptySlots); i++) {
            var placeholder = new Control();
            placeholder.CustomMinimumSize = new Vector2(60, 60);
            grid.AddChild(placeholder);
        }
    }

    private Button CreateItemButton(ItemData item, bool isPlayer1) {
        var button = new Button();
        button.CustomMinimumSize = new Vector2(60, 60);
        button.Text = item.Id.Length > 6 ? item.Id.Substring(0, 6) : item.Id;
        button.TooltipText = item.Id + "\n数量: " + item.Quantity;

        if (item.Rarity == ItemRarity.Legendary) {
            button.Modulate = new Color(1, 0.84f, 0);
        } else if (item.Rarity == ItemRarity.Epic) {
            button.Modulate = new Color(0.9, 0.5, 0.9);
        } else if (item.Rarity == ItemRarity.Rare) {
            button.Modulate = new Color(0.5, 0.8, 1);
        } else if (item.Rarity == ItemRarity.Uncommon) {
            button.Modulate = new Color(0.5, 1, 0.5);
        }

        button.Pressed += () => OnItemClicked(item, isPlayer1);
        return button;
    }

    private void OnItemClicked(ItemData item, bool isPlayer1) {
        if (TradeSystem.Instance.CurrentState != TradeSystem.TradeState.Offering) {
            return;
        }

        // 显示物品操作菜单
        var menu = GetTree().CurrentScene.GetNode<Control>("CanvasLayer/ItemActionMenu");
        if (menu != null) {
            menu.Visible = true;
        }
    }

    private void OnTradeStarted() {
        UpdateUI();
        ShowTrade();
    }

    private void OnOfferUpdated(TradeOffer offer) {
        UpdateUI();
    }

    private void OnTradeCompleted(TradeRecord record) {
        statusLabel.Text = "交易成功！";
        GD.Print("交易完成: " + record.RecordId);
    }

    private void OnTradeCancelled() {
        HideTrade();
    }

    private void OnAcceptPressed() {
        TradeSystem.Instance.AcceptTrade(true);
    }

    private void OnCancelPressed() {
        TradeSystem.Instance.CancelTrade();
    }

    private void OnClosePressed() {
        TradeSystem.Instance.CancelTrade();
    }

    public static void ToggleTrade() {
        if (Instance == null) return;

        if (Instance.Visible) {
            Instance.HideTrade();
        } else {
            Instance.StartNPCTrade();
        }
    }
}
