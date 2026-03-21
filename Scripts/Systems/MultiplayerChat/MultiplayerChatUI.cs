using Godot;
using System.Collections.Generic;

public class MultiplayerChatUI : Control
{
    private MultiplayerChatSystem _chatSystem;
    private MultiplayerChatData _chatData;
    private MultiplayerChatDatabase _database;
    
    // UI Elements
    private VBoxContainer _mainContainer;
    private ScrollContainer _scrollContainer;
    private VBoxContainer _messagesContainer;
    private HBoxContainer _inputContainer;
    private LineEdit _messageInput;
    private Button _sendButton;
    private OptionButton _channelSelector;
    private Button _emoteButton;
    private MenuButton _settingsMenu;
    
    // Tab containers
    private TabContainer _tabContainer;
    private VBoxContainer _chatTab;
    private VBoxContainer _emotesTab;
    private VBoxContainer _settingsTab;
    private VBoxContainer _statsTab;
    
    // Emote grid
    private GridContainer _emoteGrid;
    
    // Settings toggles
    private CheckButton _timestampToggle;
    private CheckButton _emoteToggle;
    private CheckButton _profanityToggle;
    
    // Stats labels
    private Label _totalMessagesLabel;
    private Label _totalEmotesLabel;
    private Label _channelStatsLabel;
    
    // Message display
    private RichTextLabel _chatDisplay;
    private Color _defaultColor = new Color(1, 1, 1);
    private Color _emoteColor = new Color(1, 0.67, 0);
    private Color _systemColor = new Color(1, 1, 0);
    
    public override void _Ready()
    {
        // Get system nodes
        _chatSystem = GetNode<MultiplayerChatSystem>("../../MultiplayerChatSystem");
        _chatData = GetNode<MultiplayerChatData>("../../MultiplayerChatData");
        _database = GetNode<MultiplayerChatDatabase>("../../MultiplayerChatDatabase");
        
        SetupUI();
        ConnectSignals();
        
        GD.Print("MultiplayerChatUI initialized");
    }
    
    private void SetupUI()
    {
        // Main container
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _mainContainer.MarginBottom = -50; // Leave space for input
        AddChild(_mainContainer);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _mainContainer.AddChild(_tabContainer);
        
        // Chat tab
        _chatTab = new VBoxContainer();
        _chatTab.Name = "Chat";
        _tabContainer.AddChild(_chatTab);
        
        // Chat display
        _chatDisplay = new RichTextLabel();
        _chatDisplay.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _chatDisplay.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _chatDisplay.BbcodeEnabled = true;
        _chatDisplay.ScrollFollowing = true;
        _chatDisplay.Name = "ChatDisplay";
        _chatTab.AddChild(_chatDisplay);
        
        // Channel selector
        HBoxContainer channelRow = new HBoxContainer();
        _chatTab.AddChild(channelRow);
        
        Label channelLabel = new Label();
        channelLabel.Text = "Channel:";
        channelRow.AddChild(channelLabel);
        
        _channelSelector = new OptionButton();
        _channelSelector.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
        _channelSelector.CustomMinimumWidth = 120;
        channelRow.AddChild(_channelSelector);
        
        // Populate channel selector
        foreach (MultiplayerChatData.ChatChannel channel in System.Enum.GetValues(typeof(MultiplayerChatData.ChatChannel)))
        {
            _channelSelector.AddItem(_database.GetChannelName(channel), (int)channel);
        }
        _channelSelector.Select((int)_chatSystem.GetCurrentChannel());
        
        // Input container
        _inputContainer = new HBoxContainer();
        _mainContainer.AddChild(_inputContainer);
        
        _messageInput = new LineEdit();
        _messageInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _messageInput.PlaceholderText = "Type a message...";
        _messageInput.MaxLength = 300;
        _messageInput.AcceptEvent = true;
        _inputContainer.AddChild(_messageInput);
        
        _sendButton = new Button();
        _sendButton.Text = "Send";
        _sendButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
        _inputContainer.AddChild(_sendButton);
        
        _emoteButton = new Button();
        _emoteButton.Text = "😀";
        _emoteButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
        _emoteButton.CustomMinimumWidth = 40;
        _inputContainer.AddChild(_emoteButton);
        
        // Emotes tab
        _emotesTab = new VBoxContainer();
        _emotesTab.Name = "Emotes";
        _tabContainer.AddChild(_emotesTab);
        
        Label emoteInfoLabel = new Label();
        emoteInfoLabel.Text = "Click an emote to use it in chat with /me command";
        _emotesTab.AddChild(emoteInfoLabel);
        
        _emoteGrid = new GridContainer();
        _emoteGrid.Columns = 5;
        _emoteGrid.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _emoteGrid.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _emotesTab.AddChild(_emoteGrid);
        
        // Populate emote grid
        var emotes = _chatSystem.GetEmotes();
        foreach (var emote in emotes)
        {
            Button emoteBtn = new Button();
            emoteBtn.Text = emote.Id;
            emoteBtn.TooltipText = $"/me {emote.Id} - {emote.Name}";
            emoteBtn.Pressed += () => OnEmotePressed(emote.Id);
            _emoteGrid.AddChild(emoteBtn);
        }
        
        // Settings tab
        _settingsTab = new VBoxContainer();
        _settingsTab.Name = "Settings";
        _tabContainer.AddChild(_settingsTab);
        
        _timestampToggle = new CheckButton();
        _timestampToggle.Text = "Show Timestamps";
        _timestampToggle.Pressed = true;
        _settingsTab.AddChild(_timestampToggle);
        
        _emoteToggle = new CheckButton();
        _emoteToggle.Text = "Enable Emotes";
        _emoteToggle.Pressed = true;
        _settingsTab.AddChild(_emoteToggle);
        
        _profanityToggle = new CheckButton();
        _profanityToggle.Text = "Profanity Filter";
        _profanityToggle.Pressed = false;
        _settingsTab.AddChild(_profanityToggle);
        
        Button clearButton = new Button();
        clearButton.Text = "Clear Chat History";
        clearButton.Pressed += OnClearPressed;
        _settingsTab.AddChild(clearButton);
        
        // Stats tab
        _statsTab = new VBoxContainer();
        _statsTab.Name = "Statistics";
        _tabContainer.AddChild(_statsTab);
        
        _totalMessagesLabel = new Label();
        _totalMessagesLabel.Text = "Total Messages: 0";
        _statsTab.AddChild(_totalMessagesLabel);
        
        _totalEmotesLabel = new Label();
        _totalEmotesLabel.Text = "Total Emotes: 0";
        _statsTab.AddChild(_totalEmotesLabel);
        
        _channelStatsLabel = new Label();
        _channelStatsLabel.Text = "Messages per Channel:";
        _statsTab.AddChild(_channelStatsLabel);
        
        UpdateStatistics();
        
        // Set minimum size
        CustomMinimumSize = new Vector2(400, 300);
    }
    
    private void ConnectSignals()
    {
        _sendButton.Pressed += OnSendPressed;
        _messageInput.TextSubmitted += OnMessageEntered;
        _channelSelector.ItemSelected += OnChannelSelected;
        _emoteButton.Pressed += OnEmoteButtonPressed;
        
        // Settings
        _timestampToggle.Toggled += OnTimestampToggled;
        _emoteToggle.Toggled += OnEmoteToggled;
        _profanityToggle.Toggled += OnProfanityToggled;
        
        // Chat system signals
        _chatSystem.MessageReceived += OnMessageReceived;
        _chatSystem.ClearRequested += OnClearRequested;
    }
    
    private void OnSendPressed()
    {
        string message = _messageInput.Text.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            _chatSystem.SendMessage(message);
            _messageInput.Text = "";
        }
    }
    
    private void OnMessageEntered(string text)
    {
        OnSendPressed();
    }
    
    private void OnChannelSelected(int index)
    {
        var channel = (MultiplayerChatData.ChatChannel)index;
        _chatSystem.SwitchChannel(channel.ToString().ToLower());
        RefreshChat();
    }
    
    private void OnEmoteButtonPressed()
    {
        _tabContainer.CurrentTab = 1; // Switch to emotes tab
    }
    
    private void OnEmotePressed(string emoteId)
    {
        _chatSystem.SendEmote(emoteId);
        _tabContainer.CurrentTab = 0; // Switch back to chat tab
        _messageInput.GrabFocus();
    }
    
    private void OnMessageReceived(string message, MultiplayerChatData.ChatChannel channel, bool isEmote = false)
    {
        AddMessageToDisplay(message, channel, isEmote);
    }
    
    private void AddMessageToDisplay(string message, MultiplayerChatData.ChatChannel channel, bool isEmote)
    {
        string color = _database.GetChannelColor(channel);
        
        if (isEmote)
        {
            _chatDisplay.AddText($"[color={_emoteColor.ToHtml()}]{message}[/color]\n");
        }
        else if (channel == MultiplayerChatData.ChatChannel.System)
        {
            _chatDisplay.AddText($"[color={_systemColor.ToHtml()}]{message}[/color]\n");
        }
        else
        {
            _chatDisplay.AddText($"[color={color}]{message}[/color]\n");
        }
        
        // Auto-scroll to bottom
        _chatDisplay.ScrollToLine(_chatDisplay.GetLineCount() - 1);
    }
    
    private void OnClearPressed()
    {
        _chatDisplay.Clear();
    }
    
    private void OnClearRequested()
    {
        _chatDisplay.Clear();
    }
    
    private void OnTimestampToggled(bool pressed)
    {
        _chatSystem.SetShowTimestamps(pressed);
    }
    
    private void OnEmoteToggled(bool pressed)
    {
        _chatSystem.SetEmoteEnabled(pressed);
    }
    
    private void OnProfanityToggled(bool pressed)
    {
        _chatSystem.SetProfanityFilter(pressed);
    }
    
    private void RefreshChat()
    {
        _chatDisplay.Clear();
        var messages = _chatSystem.GetRecentMessages(50);
        foreach (var msg in messages)
        {
            AddMessageToDisplay($"{msg.SenderName}: {msg.Content}", msg.Channel, msg.IsEmote);
        }
    }
    
    private void UpdateStatistics()
    {
        var stats = _chatSystem.GetStatistics();
        
        if (stats.ContainsKey("TotalMessages"))
            _totalMessagesLabel.Text = $"Total Messages: {stats["TotalMessages"]}";
        
        if (stats.ContainsKey("TotalEmotes"))
            _totalEmotesLabel.Text = $"Total Emotes: {stats["TotalEmotes"]}";
        
        string channelStats = "Messages per Channel:\n";
        foreach (var kvp in stats)
        {
            if (kvp.Key.EndsWith("Messages"))
            {
                channelStats += $"  {kvp.Key.Replace("Messages", "")}: {kvp.Value}\n";
            }
        }
        _channelStatsLabel.Text = channelStats;
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // Ctrl+Enter to send
            if (keyEvent.Control && keyEvent.Scancode == KeyList.Return)
            {
                OnSendPressed();
                GetTree().SetInputAsHandled();
            }
        }
    }
}
