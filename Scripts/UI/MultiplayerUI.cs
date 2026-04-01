using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 多人游戏 UI
/// 连接服务器、创建/加入房间
/// </summary>
public partial class MultiplayerUI : Control
{
    // UI 元素
    private Button _connectButton;
    private Button _createRoomButton;
    private Button _leaveRoomButton;
    private LineEdit _serverUrlInput;
    private LineEdit _roomNameInput;
    private LineEdit _playerNameInput;
    private Label _statusLabel;
    private Label _roomInfoLabel;
    private VBoxContainer _playerListContainer;
    private Panel _mainPanel;
    private VBoxContainer _menuContainer;
    
    // 状态
    private bool _isVisible = false; 

    public override void _Ready()
    {
        SetupUI();
        SetupSignals();
        Hide();
    }

    private void SetupUI()
    {
        // 主面板
        _mainPanel = new Panel();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(400, 500);
        AddChild(_mainPanel);
        
        var mainVBox = new VBoxContainer();
        mainVBox.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainVBox.Position = new Vector2(100, 50);
        mainVBox.Size = new Vector2(200, 400);
        _mainPanel.AddChild(mainVBox);
        
        // 标题
        var titleLabel = new Label();
        titleLabel.Text = "多人游戏";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainVBox.AddChild(titleLabel);
        
        mainVBox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });
        
        // 服务器 URL
        var serverLabel = new Label();
        serverLabel.Text = "服务器地址:";
        mainVBox.AddChild(serverLabel);
        
        _serverUrlInput = new LineEdit();
        _serverUrlInput.PlaceholderText = "ws://localhost:8080";
        _serverUrlInput.Text = "ws://localhost:8080";
        mainVBox.AddChild(_serverUrlInput);
        
        mainVBox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });
        
        // 玩家名称
        var playerNameLabel = new Label();
        playerNameLabel.Text = "你的名称:";
        mainVBox.AddChild(playerNameLabel);
        
        _playerNameInput = new LineEdit();
        _playerNameInput.PlaceholderText = "输入你的名字";
        _playerNameInput.Text = "Player";
        mainVBox.AddChild(_playerNameInput);
        
        mainVBox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });
        
        // 连接按钮
        _connectButton = new Button();
        _connectButton.Text = "连接服务器";
        _connectButton.CustomMinimumSize = new Vector2(0, 40);
        mainVBox.AddChild(_connectButton);
        
        mainVBox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });
        
        // 房间名称输入
        var roomNameLabel = new Label();
        roomNameLabel.Text = "房间名称:";
        mainVBox.AddChild(roomNameLabel);
        
        _roomNameInput = new LineEdit();
        _roomNameInput.PlaceholderText = "输入房间名称";
        _roomNameInput.Text = "我的房间";
        mainVBox.AddChild(_roomNameInput);
        
        mainVBox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });
        
        // 创建房间按钮
        _createRoomButton = new Button();
        _createRoomButton.Text = "创建房间";
        _createRoomButton.CustomMinimumSize = new Vector2(0, 40);
        _createRoomButton.Disabled = true;
        mainVBox.AddChild(_createRoomButton);
        
        mainVBox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });
        
        // 状态标签
        _statusLabel = new Label();
        _statusLabel.Text = "未连接";
        _statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
        mainVBox.AddChild(_statusLabel);
        
        mainVBox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });
        
        // 房间信息
        _roomInfoLabel = new Label();
        _roomInfoLabel.Text = "";
        _roomInfoLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _roomInfoLabel.Visible = false; 
        mainVBox.AddChild(_roomInfoLabel);
        
        // 玩家列表
        _playerListContainer = new VBoxContainer();
        _playerListContainer.Visible = false; 
        mainVBox.AddChild(_playerListContainer);
        
        mainVBox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });
        
        // 离开房间按钮
        _leaveRoomButton = new Button();
        _leaveRoomButton.Text = "离开房间";
        _leaveRoomButton.CustomMinimumSize = new Vector2(0, 40);
        _leaveRoomButton.Visible = false; 
        mainVBox.AddChild(_leaveRoomButton);
    }

    private void SetupSignals()
    {
        _connectButton.Pressed += OnConnectPressed;
        _createRoomButton.Pressed += OnCreateRoomPressed;
        _leaveRoomButton.Pressed += OnLeaveRoomPressed;
        
        if (MultiplayerManager.Instance != null)
        {
            MultiplayerManager.Instance.OnConnected += OnServerConnected;
            MultiplayerManager.Instance.OnDisconnected += OnServerDisconnected;
            MultiplayerManager.Instance.OnRoomCreated += OnRoomCreated;
            MultiplayerManager.Instance.OnRoomJoined += OnRoomJoined;
            MultiplayerManager.Instance.OnRoomLeft += OnRoomLeft;
            MultiplayerManager.Instance.OnPlayerJoined += OnPlayerJoined;
            MultiplayerManager.Instance.OnPlayerLeft += OnPlayerLeft;
            MultiplayerManager.Instance.OnConnectionFailed += OnConnectionFailed;
        }
    }

    private void OnConnectPressed()
    {
        string serverUrl = _serverUrlInput.Text;
        string playerName = _playerNameInput.Text;
        
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Player";
        }
        
        if (MultiplayerManager.Instance != null)
        {
            MultiplayerManager.Instance.PlayerName = playerName;
            MultiplayerManager.Instance.ConnectToServer(serverUrl);
            _statusLabel.Text = "连接中...";
            _connectButton.Disabled = true;
        }
    }

    private void OnCreateRoomPressed()
    {
        string roomName = _roomNameInput.Text;
        
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "我的房间";
        }
        
        if (MultiplayerManager.Instance != null)
        {
            MultiplayerManager.Instance.CreateRoom(roomName);
        }
    }

    private void OnLeaveRoomPressed()
    {
        if (MultiplayerManager.Instance != null)
        {
            MultiplayerManager.Instance.LeaveRoom();
        }
    }

    private void OnServerConnected()
    {
        _statusLabel.Text = "已连接到服务器";
        _createRoomButton.Disabled = false; 
    }

    private void OnServerDisconnected(string reason)
    {
        _statusLabel.Text = $"断开连接: {reason}";
        _connectButton.Disabled = false; 
        _createRoomButton.Disabled = true;
        _roomInfoLabel.Visible = false; 
        _playerListContainer.Visible = false; 
    }

    private void OnRoomCreated(string roomId)
    {
        _roomInfoLabel.Text = $"房间已创建: {roomId}";
        _roomInfoLabel.Visible = true;
        _createRoomButton.Disabled = true;
        _leaveRoomButton.Visible = true;
        _playerListContainer.Visible = true;
        
        // 显示房主
        UpdatePlayerList();
    }

    private void OnRoomJoined(string roomId)
    {
        _roomInfoLabel.Text = $"已加入房间: {roomId}";
        _roomInfoLabel.Visible = true;
        _createRoomButton.Disabled = true;
        _leaveRoomButton.Visible = true;
        _playerListContainer.Visible = true;
        
        UpdatePlayerList();
    }

    private void OnRoomLeft()
    {
        _roomInfoLabel.Text = "";
        _roomInfoLabel.Visible = false; 
        _createRoomButton.Disabled = false; 
        _leaveRoomButton.Visible = false; 
        _playerListContainer.Visible = false; 
        
        // 清空玩家列表
        foreach (Node child in _playerListContainer.GetChildren())
        {
            child.QueueFree();
        }
    }

    private void OnPlayerJoined(int playerId, string playerName)
    {
        UpdatePlayerList();
    }

    private void OnPlayerLeft(int playerId)
    {
        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        // 清空列表
        foreach (Node child in _playerListContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        if (MultiplayerManager.Instance == null) return;
        
        var header = new Label();
        header.Text = "玩家列表:";
        _playerListContainer.AddChild(header);
        
        var players = MultiplayerManager.Instance.GetNetworkPlayers();
        
        foreach (var player in players)
        {
            var playerLabel = new Label();
            string hostTag = player.PlayerId == 1 ? " (房主)" : "";
            playerLabel.Text = $"• {player.PlayerName}{hostTag}";
            _playerListContainer.AddChild(playerLabel);
        }
    }

    private void OnConnectionFailed(string reason)
    {
        _statusLabel.Text = $"连接失败: {reason}";
        _connectButton.Disabled = false; 
    }

    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // M 键切换多人游戏界面
            if (keyEvent.Keycode == Key.M && !keyEvent.Echo)
            {
                Toggle();
            }
        }
    }

    public void Toggle()
    {
        if (_isVisible)
        {
            Hide();
            _isVisible = false; 
        }
        else
        {
            Show();
            _isVisible = true;
        }
    }

    public override void _ExitTree()
    {
        if (MultiplayerManager.Instance != null)
        {
            MultiplayerManager.Instance.OnConnected -= OnServerConnected;
            MultiplayerManager.Instance.OnDisconnected -= OnServerDisconnected;
            MultiplayerManager.Instance.OnRoomCreated -= OnRoomCreated;
            MultiplayerManager.Instance.OnRoomJoined -= OnRoomJoined;
            MultiplayerManager.Instance.OnRoomLeft -= OnRoomLeft;
            MultiplayerManager.Instance.OnPlayerJoined -= OnPlayerJoined;
            MultiplayerManager.Instance.OnPlayerLeft -= OnPlayerLeft;
            MultiplayerManager.Instance.OnConnectionFailed -= OnConnectionFailed;
        }
    }
}
