using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

public class FriendUI : Control
{
    private FriendSystem _friendSystem;
    
    // UI 组件
    private TabContainer _tabContainer;
    private VBoxContainer _friendListContainer;
    private VBoxContainer _requestListContainer;
    private VBoxContainer _chatContainer;
    private LineEdit _searchEdit;
    private LineEdit _messageEdit;
    private Label _selectedFriendLabel;
    private RichTextLabel _chatDisplay;
    
    // 选中好友
    private string _selectedFriend = "";
    
    // 场景路径
    private const string FRIEND_CARD_SCENE = "res://ui/FriendCard.tscn";
    
    public override void _Ready()
    {
        _friendSystem = FriendSystem.Instance;
        
        SetupUI();
        ConnectSignals();
        RefreshFriendList();
        
        // 初始隐藏
        Visible = false;
    }

    private void SetupUI()
    {
        // 主容器
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainContainer.CustomMinimumSize = new Vector2(600, 500);
        AddChild(mainContainer);

        // 标题栏
        var titleBar = new HBoxContainer();
        mainContainer.AddChild(titleBar);
        
        var title = new Label();
        title.Text = "  好友系统";
        title.AddColorOverride("font_color", new Color(1, 0.9, 0.5));
        titleBar.AddChild(title);
        
        titleBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        var closeBtn = new Button();
        closeBtn.Text = "X";
        closeBtn.CustomMinimumSize = new Vector2(30, 30);
        closeBtnPressed += () => Visible = false;
        titleBar.AddChild(closeBtn);

        // TabContainer
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
        mainContainer.AddChild(_tabContainer);

        // 好友列表页
        var friendListTab = new Control();
        _tabContainer.AddChild(friendListTab);
        _tabContainer.SetTabTitle(0, "好友");
        
        SetupFriendListTab(friendListTab);

        // 好友申请页
        var requestTab = new Control();
        _tabContainer.AddChild(requestTab);
        _tabContainer.SetTabTitle(1, "申请");
        
        SetupRequestTab(requestTab);

        // 聊天页
        var chatTab = new Control();
        _tabContainer.AddChild(chatTab);
        _tabContainer.SetTabTitle(2, "聊天");
        
        SetupChatTab(chatTab);

        // 底部提示
        var hintLabel = new Label();
        hintLabel.Text = "  按 F 切换显示";
        hintLabel.AddColorOverride("font_color", new Color(0.6, 0.6, 0.6));
        mainContainer.AddChild(hintLabel);
    }

    private void SetupFriendListTab(Control tab)
    {
        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.MarginLeft = 10;
        vbox.MarginTop = 10;
        vbox.MarginRight = -10;
        vbox.MarginBottom = -10;
        tab.AddChild(vbox);

        // 搜索框
        _searchEdit = new LineEdit();
        _searchEdit.PlaceholderText = "搜索好友...";
        _searchEdit.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        _searchEdit.TextChanged += OnSearchTextChanged;
        vbox.AddChild(_searchEdit);

        // 好友列表
        _friendListContainer = new VBoxContainer();
        _friendListContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
        vbox.AddChild(_friendListContainer);

        // 添加好友按钮
        var addBtn = new Button();
        addBtn.Text = "添加好友";
        addBtn.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        addBtn.Pressed += OnAddFriendPressed;
        vbox.AddChild(addBtn);
    }

    private void SetupRequestTab(Control tab)
    {
        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.MarginLeft = 10;
        vbox.MarginTop = 10;
        vbox.MarginRight = -10;
        vbox.MarginBottom = -10;
        tab.AddChild(vbox);

        var title = new Label();
        title.Text = "好友申请";
        vbox.AddChild(title);

        _requestListContainer = new VBoxContainer();
        _requestListContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
        vbox.AddChild(_requestListContainer);
    }

    private void SetupChatTab(Control tab)
    {
        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.MarginLeft = 10;
        vbox.MarginTop = 10;
        vbox.MarginRight = -10;
        vbox.MarginBottom = -10;
        tab.AddChild(vbox);

        // 当前聊天好友
        _selectedFriendLabel = new Label();
        _selectedFriendLabel.Text = "选择好友开始聊天";
        vbox.AddChild(_selectedFriendLabel);

        // 聊天显示
        _chatDisplay = new RichTextLabel();
        _chatDisplay.SizeFlagsVertical = Control.SizeFlags.Expand;
        _chatDisplay.BbcodeEnabled = true;
        vbox.AddChild(_chatDisplay);

        // 消息输入
        _messageEdit = new LineEdit();
        _messageEdit.PlaceholderText = "输入消息...";
        _messageEdit.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        vbox.AddChild(_messageEdit);

        // 发送按钮
        var sendBtn = new Button();
        sendBtn.Text = "发送";
        sendBtn.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        sendBtn.Pressed += OnSendMessagePressed;
        vbox.AddChild(sendBtn);

        // 好友选择下拉
        var friendSelect = new OptionButton();
        friendSelect.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        friendSelect.ItemSelected += OnFriendSelected;
        vbox.AddChild(friendSelect);
        
        // 填充好友选项
        RefreshFriendSelect(friendSelect);
    }

    private void ConnectSignals()
    {
        if (_friendSystem != null)
        {
            _friendSystem.Connect(nameof(FriendSystem.FriendListUpdated), this, nameof(OnFriendListUpdated));
            _friendSystem.Connect(nameof(FriendSystem.FriendRequestReceived), this, nameof(OnFriendRequestReceived));
            _friendSystem.Connect(nameof(FriendSystem.ChatMessageReceived), this, nameof(OnChatMessageReceived));
        }
    }

    private void RefreshFriendList()
    {
        // 清空列表
        foreach (Node child in _friendListContainer.GetChildren())
        {
            child.QueueFree();
        }

        var friends = _friendSystem.GetFriends();
        foreach (var friend in friends)
        {
            var card = CreateFriendCard(friend);
            _friendListContainer.AddChild(card);
        }

        // 刷新申请列表
        RefreshRequestList();
        
        // 刷新聊天选择
        var chatTab = _tabContainer.GetTabControl(2);
        var optionBtn = chatTab.GetNode<OptionButton>(".");
        if (optionBtn != null)
        {
            RefreshFriendSelect(optionBtn);
        }
    }

    private void RefreshRequestList()
    {
        foreach (Node child in _requestListContainer.GetChildren())
        {
            child.QueueFree();
        }

        var requests = _friendSystem.GetPendingRequests();
        foreach (var requester in requests)
        {
            var hbox = new HBoxContainer();
            _requestListContainer.AddChild(hbox);

            var nameLabel = new Label();
            nameLabel.Text = requester;
            hbox.AddChild(nameLabel);

            var acceptBtn = new Button();
            acceptBtn.Text = "接受";
            acceptBtn.Pressed += () => OnAcceptRequest(requester);
            hbox.AddChild(acceptBtn);

            var declineBtn = new Button();
            declineBtn.Text = "拒绝";
            declineBtn.Pressed += () => OnDeclineRequest(requester);
            hbox.AddChild(declineBtn);
        }
        
        if (requests.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "暂无好友申请";
            _requestListContainer.AddChild(emptyLabel);
        }
    }

    private void RefreshFriendSelect(OptionButton optionBtn)
    {
        optionBtn.Clear();
        var friends = _friendSystem.GetFriends();
        int index = 0;
        foreach (var friend in friends)
        {
            optionBtn.AddItem(friend.playerName, index++);
        }
    }

    private Control CreateFriendCard(FriendData friend)
    {
        var hbox = new HBoxContainer();
        hbox.CustomMinimumSize = new Vector2(0, 50);

        // 状态指示
        var statusColor = new ColorRect();
        statusColor.CustomMinimumSize = new Vector2(10, 10);
        statusColor.Color = friend.isOnline ? Color.Green : Color.Gray;
        hbox.AddChild(statusColor);

        // 好友信息
        var infoVbox = new VBoxContainer();
        hbox.AddChild(infoVbox);

        var nameLabel = new Label();
        nameLabel.Text = friend.playerName;
        infoVbox.AddChild(nameLabel);

        var statusLabel = new Label();
        statusLabel.Text = friend.isOnline ? "在线" : "离线";
        statusLabel.AddColorOverride("font_color", new Color(0.6, 0.6, 0.6));
        infoVbox.AddChild(statusLabel);

        // 好感度
        var levelLabel = new Label();
        levelLabel.Text = $"Lv.{friend.friendshipLevel}";
        hbox.AddChild(levelLabel);

        // 聊天按钮
        var chatBtn = new Button();
        chatBtn.Text = "聊天";
        chatBtn.Pressed += () => OnChatWithFriend(friend.playerName);
        hbox.AddChild(chatBtn);

        // 删除按钮
        var removeBtn = new Button();
        removeBtn.Text = "删除";
        removeBtn.Pressed += () => OnRemoveFriend(friend.playerName);
        hbox.AddChild(removeBtn);

        // 点击选择
        var clickDetector = new Control();
        clickDetector.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        clickDetector.GuiInput += (inputEvent) => {
            if (inputEvent is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == Button.Left)
            {
                _selectedFriend = friend.playerName;
                _selectedFriendLabel.Text = $"正在与 {friend.playerName} 聊天";
                _tabContainer.CurrentTab = 2;
            }
        };
        hbox.AddChild(clickDetector);

        return hbox;
    }

    // 信号处理
    private void OnFriendListUpdated()
    {
        RefreshFriendList();
    }

    private void OnFriendRequestReceived(string fromPlayer, string message)
    {
        RefreshRequestList();
    }

    private void OnChatMessageReceived(string fromPlayer, string message)
    {
        if (_selectedFriend == fromPlayer)
        {
            RefreshChatDisplay();
        }
    }

    private void OnSearchTextChanged(string text)
    {
        // 过滤好友列表
        foreach (Node child in _friendListContainer.GetChildren())
        {
            if (child is HBoxContainer hbox)
            {
                var nameLabel = hbox.GetNode<Label>("./2"); // 简化处理
                if (nameLabel != null)
                {
                    child.Visible = nameLabel.Text.Contains(text);
                }
            }
        }
    }

    private void OnAddFriendPressed()
    {
        var dialog = new WindowDialog();
        dialog.Title = "添加好友";
        dialog.CustomMinimumSize = new Vector2(300, 150);
        AddChild(dialog);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.Center);
        vbox.MarginLeft = 20;
        vbox.MarginTop = 20;
        vbox.MarginRight = -20;
        vbox.MarginBottom = -20;
        dialog.AddChild(vbox);

        var nameEdit = new LineEdit();
        nameEdit.PlaceholderText = "玩家名称";
        vbox.AddChild(nameEdit);

        var msgEdit = new LineEdit();
        msgEdit.PlaceholderText = "验证消息（可选）";
        vbox.AddChild(msgEdit);

        var sendBtn = new Button();
        sendBtn.Text = "发送申请";
        sendBtn.Pressed += () => {
            if (_friendSystem.SendFriendRequest(nameEdit.Text, msgEdit.Text))
            {
                dialog.Hide();
            }
        };
        vbox.AddChild(sendBtn);

        dialog.PopupCentered();
    }

    private void OnAcceptRequest(string playerName)
    {
        _friendSystem.AcceptFriendRequest(playerName);
        RefreshFriendList();
    }

    private void OnDeclineRequest(string playerName)
    {
        _friendSystem.DeclineFriendRequest(playerName);
        RefreshRequestList();
    }

    private void OnChatWithFriend(string friendName)
    {
        _selectedFriend = friendName;
        _selectedFriendLabel.Text = $"正在与 {friendName} 聊天";
        _tabContainer.CurrentTab = 2;
        RefreshChatDisplay();
    }

    private void OnRemoveFriend(string playerName)
    {
        _friendSystem.RemoveFriend(playerName);
        RefreshFriendList();
    }

    private void OnFriendSelected(int index)
    {
        var friends = _friendSystem.GetFriends();
        if (index >= 0 && index < friends.Count)
        {
            _selectedFriend = friends[index].playerName;
            _selectedFriendLabel.Text = $"正在与 {friends[index].playerName} 聊天";
            RefreshChatDisplay();
        }
    }

    private void OnSendMessagePressed()
    {
        if (string.IsNullOrEmpty(_selectedFriend)) return;
        if (string.IsNullOrEmpty(_messageEdit.Text)) return;

        _friendSystem.SendMessage(_selectedFriend, _messageEdit.Text);
        _messageEdit.Text = "";
        RefreshChatDisplay();
    }

    private void RefreshChatDisplay()
    {
        if (string.IsNullOrEmpty(_selectedFriend)) return;

        _chatDisplay.Text = "";
        var messages = _friendSystem.GetChatHistory(_selectedFriend);
        foreach (var msg in messages)
        {
            var prefix = msg.fromPlayer == _friendSystem.GetPlayerName() ? "[我]" : $"[{msg.fromPlayer}]";
            _chatDisplay.AppendText($"{prefix}: {msg.message}\n");
        }
    }

    // 切换显示
    public void Toggle()
    {
        Visible = !Visible;
        if (Visible)
        {
            RefreshFriendList();
        }
    }

    // 输入处理
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            switch (keyEvent.Scancode)
            {
                case KeyList.F:
                    Toggle();
                    break;
                case KeyList.Escape:
                    Visible = false;
                    break;
            }
        }
    }

    private string GetPlayerName()
    {
        if (HasNode("/root/GameManager"))
        {
            var gameManager = GetNode("/root/GameManager");
            var playerName = gameManager.Get("player_name") as string;
            if (playerName != null) return playerName;
        }
        return "Player";
    }
}
