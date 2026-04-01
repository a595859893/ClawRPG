using Godot;
using System;
using System.Collections.Generic;

public partial class GuildHeritageUI : Control
{
    private Label _titleLabel;
    private Label _heritagePointsLabel;
    private Label _dailyLimitLabel;
    
    private TabContainer _tabContainer;
    
    // Transfer Tab
    private OptionButton _transferTypeOption;
    private SpinBox _goldSpinBox;
    private SpinBox _expSpinBox;
    private LineEdit _recipientEdit;
    private LineEdit _messageEdit;
    private Button _sendButton;
    private Label _transferInfoLabel;
    
    // Pending Tab
    private ItemList _pendingList;
    private Button _acceptButton;
    private Button _rejectButton;
    
    // History Tab
    private ItemList _historyList;
    
    // Statistics Tab
    private Label _statsLabel;
    
    // Reference to system
    private GuildHeritageSystem _system;
    
    public override void _Ready()
    {
        _system = GuildHeritageSystem.Instance;
        
        SetupUI();
        RefreshData();
    }
    
    private void SetupUI()
    {
        // Main Container
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainContainer.CustomMinimumSize = new Vector2(800, 600);
        AddChild(mainContainer);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "🏛️ Guild Heritage System";
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainContainer.AddChild(_titleLabel);
        
        // Heritage Points
        _heritagePointsLabel = new Label();
        _heritagePointsLabel.Text = "Heritage Points: 0";
        _heritagePointsLabel.Align = Label.AlignEnum.Center;
        mainContainer.AddChild(_heritagePointsLabel);
        
        // Daily Limit
        _dailyLimitLabel = new Label();
        _dailyLimitLabel.Text = "Transfers Today: 0/3";
        _dailyLimitLabel.Align = Label.AlignEnum.Center;
        mainContainer.AddChild(_dailyLimitLabel);
        
        // Tab Container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(_tabContainer);
        
        // Create tabs
        CreateTransferTab();
        CreatePendingTab();
        CreateHistoryTab();
        CreateStatisticsTab();
        
        // Close Button
        var closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        closeButton.Pressed += OnClosePressed;
        mainContainer.AddChild(closeButton);
    }
    
    private void CreateTransferTab()
    {
        var tab = new VBoxContainer();
        tab.Name = "Transfer";
        _tabContainer.AddChild(tab);
        
        // Transfer Type
        var typeLabel = new Label();
        typeLabel.Text = "Transfer Type:";
        tab.AddChild(typeLabel);
        
        _transferTypeOption = new OptionButton();
        _transferTypeOption.AddItem("🎁 Gift - Regular transfer", 0);
        _transferTypeOption.AddItem("🏛️ Inheritance - Senior to Junior", 1);
        _transferTypeOption.AddItem("📚 Teaching - Share experience", 2);
        _transferTypeOption.ItemSelected += OnTransferTypeChanged;
        tab.AddChild(_transferTypeOption);
        
        // Info Label
        _transferInfoLabel = new Label();
        _transferInfoLabel.Text = "Select a transfer type to see details";
        _transferInfoLabel.Autowrap = true;
        tab.AddChild(_transferInfoLabel);
        
        // Separator
        tab.AddChild(new Control());
        ((Control)tab.GetChild(tab.GetChildCount() - 1)).CustomMinimumSize = new Vector2(0, 20);
        
        // Recipient
        var recipientLabel = new Label();
        recipientLabel.Text = "Recipient Player Name:";
        tab.AddChild(recipientLabel);
        
        _recipientEdit = new LineEdit();
        _recipientEdit.Placeholder = "Enter player name";
        tab.AddChild(_recipientEdit);
        
        // Gold Amount
        var goldLabel = new Label();
        goldLabel.Text = "Gold Amount:";
        tab.AddChild(goldLabel);
        
        _goldSpinBox = new SpinBox();
        _goldSpinBox.MinValue = 0;
        _goldSpinBox.MaxValue = 100000;
        _goldSpinBox.Value = 0;
        tab.AddChild(_goldSpinBox);
        
        // Exp Amount
        var expLabel = new Label();
        expLabel.Text = "Experience Amount:";
        tab.AddChild(expLabel);
        
        _expSpinBox = new SpinBox();
        _expSpinBox.MinValue = 0;
        _expSpinBox.MaxValue = 50000;
        _expSpinBox.Value = 0;
        tab.AddChild(_expSpinBox);
        
        // Message
        var messageLabel = new Label();
        messageLabel.Text = "Message (optional):";
        tab.AddChild(messageLabel);
        
        _messageEdit = new LineEdit();
        _messageEdit.Placeholder = "Enter a message";
        tab.AddChild(_messageEdit);
        
        // Separator
        tab.AddChild(new Control());
        ((Control)tab.GetChild(tab.GetChildCount() - 1)).CustomMinimumSize = new Vector2(0, 20);
        
        // Send Button
        _sendButton = new Button();
        _sendButton.Text = "Send Transfer";
        _sendButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        _sendButton.Pressed += OnSendPressed;
        tab.AddChild(_sendButton);
    }
    
    private void CreatePendingTab()
    {
        var tab = new VBoxContainer();
        tab.Name = "Pending";
        _tabContainer.AddChild(tab);
        
        // Label
        var label = new Label();
        label.Text = "Incoming Transfer Requests:";
        tab.AddChild(label);
        
        // Pending List
        _pendingList = new ItemList();
        _pendingList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        tab.AddChild(_pendingList);
        
        // Buttons
        var buttonContainer = new HBoxContainer();
        buttonContainer.Alignment = BoxContainer.AlignMode.Center;
        tab.AddChild(buttonContainer);
        
        _acceptButton = new Button();
        _acceptButton.Text = "Accept";
        _acceptButton.Pressed += OnAcceptPressed;
        buttonContainer.AddChild(_acceptButton);
        
        _rejectButton = new Button();
        _rejectButton.Text = "Reject";
        _rejectButton.Pressed += OnRejectPressed;
        buttonContainer.AddChild(_rejectButton);
    }
    
    private void CreateHistoryTab()
    {
        var tab = new VBoxContainer();
        tab.Name = "History";
        _tabContainer.AddChild(tab);
        
        // Label
        var label = new Label();
        label.Text = "Transfer History:";
        tab.AddChild(label);
        
        // History List
        _historyList = new ItemList();
        _historyList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        tab.AddChild(_historyList);
    }
    
    private void CreateStatisticsTab()
    {
        var tab = new VBoxContainer();
        tab.Name = "Statistics";
        _tabContainer.AddChild(tab);
        
        // Stats Label
        _statsLabel = new Label();
        _statsLabel.Autowrap = true;
        _statsLabel.Text = "Loading statistics...";
        tab.AddChild(_statsLabel);
    }
    
    private void RefreshData()
    {
        // Refresh points and limits
        var stats = _system.GetStatistics();
        _heritagePointsLabel.Text = "Heritage Points: " + stats["heritage_points"];
        
        int remaining = _system.GetRemainingDailyTransfers();
        _dailyLimitLabel.Text = "Transfers Today: " + (3 - remaining) + "/3";
        
        // Refresh pending transfers
        RefreshPendingList();
        
        // Refresh history
        RefreshHistoryList();
        
        // Refresh statistics
        RefreshStatistics();
        
        // Update transfer info
        UpdateTransferInfo();
    }
    
    private void RefreshPendingList()
    {
        _pendingList.Clear();
        
        // In a real implementation, get actual player ID
        int playerId = 1;
        var pending = _system.GetPendingTransfers(playerId);
        
        foreach (var transfer in pending)
        {
            string text = $"[{transfer.TransferType}] {transfer.FromPlayerName} → {transfer.ToPlayerName}\n";
            text += $"Gold: {transfer.GoldAmount} | Exp: {transfer.ExpAmount}";
            if (transfer.Items.Count > 0)
            {
                text += $"\nItems: {transfer.Items.Count}";
            }
            _pendingList.AddItem(text);
        }
        
        if (pending.Count == 0)
        {
            _pendingList.AddItem("No pending transfers");
        }
    }
    
    private void RefreshHistoryList()
    {
        _historyList.Clear();
        
        var history = _system.Data.TransferHistory;
        
        foreach (var transfer in history)
        {
            string text = $"[{transfer.TransferType}] {transfer.FromPlayerName} → {transfer.ToPlayerName}\n";
            text += $"Gold: {transfer.GoldAmount} | Exp: {transfer.ExpAmount} | Status: {transfer.Status}";
            _historyList.AddItem(text);
        }
        
        if (history.Count == 0)
        {
            _historyList.AddItem("No transfer history");
        }
    }
    
    private void RefreshStatistics()
    {
        var stats = _system.GetStatistics();
        
        string text = "📊 Guild Heritage Statistics\n\n";
        text += $"Total Transfers: {stats["total_transfers"]}\n";
        text += $"Total Gold Transferred: {stats["total_gold"]:,}\n";
        text += $"Total Experience Transferred: {stats["total_exp"]:,}\n";
        text += $"Total Items Transferred: {stats["total_items"]}\n";
        text += $"Current Heritage Points: {stats["heritage_points"]}\n";
        text += $"Total Points Earned: {stats["total_points_earned"]}\n";
        text += $"Members Who Used System: {stats["members_used"]}";
        
        _statsLabel.Text = text;
    }
    
    private void UpdateTransferInfo()
    {
        int selected = _transferTypeOption.Selected;
        string[] types = { "gift", "inheritance", "teaching" };
        
        if (selected >= 0 && selected < types.Length)
        {
            var config = _system.Database.GetTransferType(types[selected]);
            _transferInfoLabel.Text = $"{config.Icon} {config.DisplayName}\n";
            _transferInfoLabel.Text += $"{config.Description}\n\n";
            _transferInfoLabel.Text += $"Max Gold: {config.MaxGold:,}\n";
            _transferInfoLabel.Text += $"Max Exp: {config.MaxExp:,}\n";
            _transferInfoLabel.Text += $"Max Items: {config.MaxItems}\n";
            _transferInfoLabel.Text += $"Tax Rate: {(config.TaxRate * 100):F0}%\n";
            _transferInfoLabel.Text += $"Cooldown: {config.CooldownSeconds / 3600}h\n";
            _transferInfoLabel.Text += $"Points Cost: {config.HeritagePointsCost}";
            
            // Update spinbox limits
            _goldSpinBox.MaxValue = config.MaxGold;
            _expSpinBox.MaxValue = config.MaxExp;
        }
    }
    
    // ==================== Signal Handlers ====================
    
    private void OnTransferTypeChanged(int index)
    {
        UpdateTransferInfo();
    }
    
    private void OnSendPressed()
    {
        string[] types = { "gift", "inheritance", "teaching" };
        string transferType = types[_transferTypeOption.Selected];
        
        string recipientName = _recipientEdit.Text;
        int goldAmount = (int)_goldSpinBox.Value;
        int expAmount = (int)_expSpinBox.Value;
        string message = _messageEdit.Text;
        
        if (string.IsNullOrEmpty(recipientName))
        {
            GD.PrintE("Please enter recipient name");
            return;
        }
        
        // In real implementation, get actual player IDs
        int fromPlayerId = 1;
        string fromPlayerName = "Player1";
        int toPlayerId = 2; // Would look up by name
        
        var transfer = _system.CreateTransfer(
            fromPlayerId,
            fromPlayerName,
            toPlayerId,
            recipientName,
            goldAmount,
            expAmount,
            new List<string>(),
            transferType,
            message
        );
        
        if (transfer != null)
        {
            GD.Print("Transfer created successfully!");
            RefreshData();
            
            // Clear inputs
            _recipientEdit.Text = "";
            _goldSpinBox.Value = 0;
            _expSpinBox.Value = 0;
            _messageEdit.Text = "";
        }
        else
        {
            GD.PrintE("Failed to create transfer");
        }
    }
    
    private void OnAcceptPressed()
    {
        int selected = _pendingList.GetSelectedItems()[0];
        if (selected >= 0)
        {
            int playerId = 1; // Would be actual player ID
            var pending = _system.GetPendingTransfers(playerId);
            
            if (selected < pending.Count)
            {
                _system.AcceptTransfer(pending[selected].TransferId, playerId);
                RefreshData();
            }
        }
    }
    
    private void OnRejectPressed()
    {
        int selected = _pendingList.GetSelectedItems()[0];
        if (selected >= 0)
        {
            int playerId = 1; // Would be actual player ID
            var pending = _system.GetPendingTransfers(playerId);
            
            if (selected < pending.Count)
            {
                _system.RejectTransfer(pending[selected].TransferId, playerId);
                RefreshData();
            }
        }
    }
    
    private void OnClosePressed()
    {
        Hide();
    }
    
    public override void _Input(InputEvent eventData)
    {
        if (eventData is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Scancode == KeyList.Escape)
        {
            Hide();
        }
    }
}
