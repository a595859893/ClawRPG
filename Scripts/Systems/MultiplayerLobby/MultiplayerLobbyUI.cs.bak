using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 多人游戏大厅UI
    /// 房间列表、创建/加入房间、准备状态、游戏模式选择
    /// </summary>
    public partial class MultiplayerLobbyUI : Control
    {
        public static MultiplayerLobbyUI Instance { get; private set; }
        
        // UI 组件
        private TabContainer _tabContainer;
        private VBoxContainer _createRoomPanel;
        private VBoxContainer _joinRoomPanel;
        private VBoxContainer _currentRoomPanel;
        private VBoxContainer _statisticsPanel;
        
        // 创建房间控件
        private LineEdit _roomNameEdit;
        private OptionButton _gameModeOption;
        private OptionButton _difficultyOption;
        private CheckBox _privateCheckBox;
        private LineEdit _passwordEdit;
        private Button _createButton;
        
        // 加入房间控件
        private OptionButton _roomListOption;
        private LineEdit _joinPasswordEdit;
        private Button _joinButton;
        private Button _refreshButton;
        
        // 当前房间控件
        private Label _currentRoomNameLabel;
        private Label _gameModeLabel;
        private Label _difficultyLabel;
        private VBoxContainer _playerListContainer;
        private Button _readyButton;
        private Button _leaveButton;
        private Button _startButton;
        
        // 统计面板
        private Label _roomsCreatedLabel;
        private Label _roomsJoinedLabel;
        private Label _gamesPlayedLabel;
        private Label _winsLabel;
        private Label _lossesLabel;
        private Label _winRateLabel;
        
        // 游戏模式数据
        private string[] _gameModes = { "CoopDungeon", "PvPBattle", "Racing", "BossRush", "TreasureHunt", "Survival" };
        private int[] _difficulties = { 1, 2, 3, 4, 5 };
        
        // 当前状态
        private bool _isInRoom = false;
        private bool _isHost = false;
        
        public override void _Ready()
        {
            Instance = this;
            Name = "MultiplayerLobbyUI";
            
            SetupUI();
            ConnectSignals();
            
            // 初始刷新房间列表
            RefreshRoomList();
            
            // 默认隐藏
            Visible = false;
        }
        
        private void SetupUI()
        {
            // 主容器
            var mainPanel = new PanelContainer
            {
                AnchorRight = 0.6f,
                AnchorBottom = 0.8f,
                AnchorLeft = 0.2f,
                AnchorTop = 0.1f,
                RectPosition = new Vector2(200, 100)
            };
            AddChild(mainPanel);
            
            var mainMargin = new MarginContainer { MarginLeft = 10, MarginTop = 10, MarginRight = 10, MarginBottom = 10 };
            mainPanel.AddChild(mainMargin);
            
            // 标题
            var titleLabel = new Label
            {
                Text = "=== Multiplayer Lobby ===",
                Align = Label.AlignEnum.Center
            };
            mainMargin.AddChild(titleLabel);
            
            // Tab 容器
            _tabContainer = new TabContainer
            {
                SizeFlagsVertical = SizeFlags.ExpandFill
            };
            mainMargin.AddChild(_tabContainer);
            
            // ===== 创建房间标签页 =====
            var createTab = new Control { Name = "Create" };
            _tabContainer.AddChild(createTab);
            _createRoomPanel = new VBoxContainer();
            createTab.AddChild(_createRoomPanel);
            
            var createTitle = new Label { Text = "Create Room", Align = Label.AlignEnum.Center };
            _createRoomPanel.AddChild(createTitle);
            _createRoomPanel.AddChild(new Control { RectMinSize = new Vector2(0, 10) }); // Spacer
            
            // 房间名称
            _createRoomPanel.AddChild(new Label { Text = "Room Name:" });
            _roomNameEdit = new LineEdit { PlaceholderText = "Enter room name..." };
            _createRoomPanel.AddChild(_roomNameEdit);
            
            // 游戏模式
            _createRoomPanel.AddChild(new Label { Text = "Game Mode:" });
            _gameModeOption = new OptionButton();
            for (int i = 0; i < _gameModes.Length; i++)
            {
                _gameModeOption.AddItem(_gameModes[i], i);
            }
            _createRoomPanel.AddChild(_gameModeOption);
            
            // 难度
            _createRoomPanel.AddChild(new Label { Text = "Difficulty:" });
            _difficultyOption = new OptionButton();
            for (int i = 0; i < _difficulties.Length; i++)
            {
                _difficultyOption.AddItem($"Difficulty {_difficulties[i]}", i);
            }
            _difficultyOption.Selected = 1; // Default to Normal
            _createRoomPanel.AddChild(_difficultyOption);
            
            // 私人房间选项
            _privateCheckBox = new CheckBox { Text = "Private Room" };
            _privateCheckBox.Toggled += OnPrivateToggled;
            _createRoomPanel.AddChild(_privateCheckBox);
            
            // 密码
            _passwordEdit = new LineEdit
            {
                PlaceholderText = "Password (if private)",
                Secret = true,
                Visible = false
            };
            _createRoomPanel.AddChild(_passwordEdit);
            
            _createRoomPanel.AddChild(new Control { RectMinSize = new Vector2(0, 20) }); // Spacer
            
            // 创建按钮
            _createButton = new Button { Text = "Create Room" };
            _createButton.Pressed += OnCreateRoomPressed;
            _createRoomPanel.AddChild(_createButton);
            
            // ===== 加入房间标签页 =====
            var joinTab = new Control { Name = "Join" };
            _tabContainer.AddChild(joinTab);
            _joinRoomPanel = new VBoxContainer();
            joinTab.AddChild(_joinRoomPanel);
            
            var joinTitle = new Label { Text = "Join Room", Align = Label.AlignEnum.Center };
            _joinRoomPanel.AddChild(joinTitle);
            _joinRoomPanel.AddChild(new Control { RectMinSize = new Vector2(0, 10) }); // Spacer
            
            // 房间列表
            _joinRoomPanel.AddChild(new Label { Text = "Available Rooms:" });
            _roomListOption = new OptionButton();
            _joinRoomPanel.AddChild(_roomListOption);
            
            // 刷新按钮
            _refreshButton = new Button { Text = "Refresh" };
            _refreshButton.Pressed += OnRefreshPressed;
            _joinRoomPanel.AddChild(_refreshButton);
            
            _joinRoomPanel.AddChild(new Control { RectMinSize = new Vector2(0, 10) }); // Spacer
            
            // 加入密码
            _joinRoomPanel.AddChild(new Label { Text = "Room Password (if required):" });
            _joinPasswordEdit = new LineEdit
            {
                PlaceholderText = "Password",
                Secret = true
            };
            _joinRoomPanel.AddChild(_joinPasswordEdit);
            
            _joinRoomPanel.AddChild(new Control { RectMinSize = new Vector2(0, 10) }); // Spacer
            
            // 加入按钮
            _joinButton = new Button { Text = "Join Room" };
            _joinButton.Pressed += OnJoinRoomPressed;
            _joinRoomPanel.AddChild(_joinButton);
            
            // ===== 当前房间标签页 =====
            var currentTab = new Control { Name = "CurrentRoom" };
            _tabContainer.AddChild(currentTab);
            _currentRoomPanel = new VBoxContainer();
            currentTab.AddChild(_currentRoomPanel);
            
            var roomTitle = new Label { Text = "Current Room", Align = Label.AlignEnum.Center };
            _currentRoomPanel.AddChild(roomTitle);
            
            _currentRoomNameLabel = new Label { Text = "Room: -", Align = Label.AlignEnum.Center };
            _currentRoomPanel.AddChild(_currentRoomNameLabel);
            
            _gameModeLabel = new Label { Text = "Mode: -" };
            _currentRoomPanel.AddChild(_gameModeLabel);
            
            _difficultyLabel = new Label { Text = "Difficulty: -" };
            _currentRoomPanel.AddChild(_difficultyLabel);
            
            _currentRoomPanel.AddChild(new Control { RectMinSize = new Vector2(0, 10) }); // Spacer
            
            // 玩家列表
            _currentRoomPanel.AddChild(new Label { Text = "Players:" });
            _playerListContainer = new VBoxContainer();
            _currentRoomPanel.AddChild(_playerListContainer);
            
            _currentRoomPanel.AddChild(new Control { RectMinSize = new Vector2(0, 10) }); // Spacer
            
            // 按钮行
            var buttonRow = new HBoxContainer();
            _currentRoomPanel.AddChild(buttonRow);
            
            _readyButton = new Button { Text = "Ready" };
            _readyButton.Pressed += OnReadyPressed;
            buttonRow.AddChild(_readyButton);
            
            _startButton = new Button { Text = "Start Game" };
            _startButton.Pressed += OnStartPressed;
            _startButton.Visible = false;
            buttonRow.AddChild(_startButton);
            
            _leaveButton = new Button { Text = "Leave Room" };
            _leaveButton.Pressed += OnLeavePressed;
            buttonRow.AddChild(_leaveButton);
            
            // ===== 统计标签页 =====
            var statsTab = new Control { Name = "Statistics" };
            _tabContainer.AddChild(statsTab);
            _statisticsPanel = new VBoxContainer();
            statsTab.AddChild(_statisticsPanel);
            
            var statsTitle = new Label { Text = "Statistics", Align = Label.AlignEnum.Center };
            _statisticsPanel.AddChild(statsTitle);
            _statisticsPanel.AddChild(new Control { RectMinSize = new Vector2(0, 10) }); // Spacer
            
            _roomsCreatedLabel = new Label { Text = "Rooms Created: 0" };
            _statisticsPanel.AddChild(_roomsCreatedLabel);
            
            _roomsJoinedLabel = new Label { Text = "Rooms Joined: 0" };
            _statisticsPanel.AddChild(_roomsJoinedLabel);
            
            _gamesPlayedLabel = new Label { Text = "Games Played: 0" };
            _statisticsPanel.AddChild(_gamesPlayedLabel);
            
            _winsLabel = new Label { Text = "Wins: 0" };
            _statisticsPanel.AddChild(_winsLabel);
            
            _lossesLabel = new Label { Text = "Losses: 0" };
            _statisticsPanel.AddChild(_lossesLabel);
            
            _winRateLabel = new Label { Text = "Win Rate: 0%" };
            _statisticsPanel.AddChild(_winRateLabel);
            
            // 更新统计
            UpdateStatistics();
            
            // 默认选中第一个标签
            _tabContainer.CurrentTab = 0;
        }
        
        private void ConnectSignals()
        {
            var lobbySystem = MultiplayerLobbySystem.Instance;
            if (lobbySystem != null)
            {
                lobbySystem.OnRoomCreated += OnRoomCreated;
                lobbySystem.OnRoomJoined += OnRoomJoined;
                lobbySystem.OnRoomLeft += OnRoomLeft;
                lobbySystem.OnRoomUpdated += OnRoomUpdated;
                lobbySystem.OnPlayerReady += OnPlayerReady;
                lobbySystem.OnGameStarted += OnGameStarted;
                lobbySystem.OnError += OnError;
            }
        }
        
        #region UI Event Handlers
        
        private void OnPrivateToggled(bool toggled)
        {
            _passwordEdit.Visible = toggled;
        }
        
        private void OnCreateRoomPressed()
        {
            string roomName = _roomNameEdit.Text;
            if (string.IsNullOrEmpty(roomName))
            {
                ShowMessage("Please enter a room name");
                return;
            }
            
            string gameMode = _gameModes[_gameModeOption.Selected];
            int difficulty = _difficulties[_difficultyOption.Selected];
            bool isPrivate = _privateCheckBox.Pressed;
            string password = _passwordEdit.Text;
            
            var lobbySystem = MultiplayerLobbySystem.Instance;
            if (lobbySystem != null)
            {
                string roomId = lobbySystem.CreateRoom(roomName, "Player1", gameMode, difficulty, isPrivate, password);
                if (!string.IsNullOrEmpty(roomId))
                {
                    _isInRoom = true;
                    _isHost = true;
                    UpdateCurrentRoomUI();
                    _tabContainer.CurrentTab = 2; // 切换到当前房间标签
                }
            }
        }
        
        private void OnRefreshPressed()
        {
            RefreshRoomList();
        }
        
        private void OnJoinRoomPressed()
        {
            if (_roomListOption.Selected < 0)
            {
                ShowMessage("Please select a room");
                return;
            }
            
            string selectedText = _roomListOption.GetItemText(_roomListOption.Selected);
            // 从选项文本提取房间ID (简化处理)
            string password = _joinPasswordEdit.Text;
            
            var lobbySystem = MultiplayerLobbySystem.Instance;
            if (lobbySystem != null)
            {
                var rooms = lobbySystem.GetAvailableRooms();
                if (_roomListOption.Selected < rooms.Count)
                {
                    var room = rooms[_roomListOption.Selected];
                    bool success = lobbySystem.JoinRoom(room.RoomId, "Player1", password);
                    if (success)
                    {
                        _isInRoom = true;
                        _isHost = false;
                        UpdateCurrentRoomUI();
                        _tabContainer.CurrentTab = 2; // 切换到当前房间标签
                    }
                }
            }
        }
        
        private void OnReadyPressed()
        {
            var lobbySystem = MultiplayerLobbySystem.Instance;
            if (lobbySystem != null)
            {
                var room = lobbySystem.GetCurrentRoom();
                if (room != null)
                {
                    bool currentReady = false;
                    foreach (var p in room.Players)
                    {
                        if (!p.IsHost)
                        {
                            currentReady = p.IsReady;
                            break;
                        }
                    }
                    lobbySystem.SetPlayerReady(2, !currentReady); // 假设玩家ID为2
                }
            }
        }
        
        private void OnStartPressed()
        {
            var lobbySystem = MultiplayerLobbySystem.Instance;
            if (lobbySystem != null)
            {
                lobbySystem.StartGame();
            }
        }
        
        private void OnLeavePressed()
        {
            var lobbySystem = MultiplayerLobbySystem.Instance;
            if (lobbySystem != null)
            {
                lobbySystem.LeaveRoom();
                _isInRoom = false;
                _isHost = false;
                RefreshRoomList();
            }
        }
        
        #endregion
        
        #region Signal Callbacks
        
        private void OnRoomCreated(string roomId, string roomName)
        {
            GD.Print($"UI: Room created - {roomName}");
            RefreshRoomList();
            UpdateCurrentRoomUI();
        }
        
        private void OnRoomJoined(string roomId)
        {
            GD.Print($"UI: Joined room - {roomId}");
            UpdateCurrentRoomUI();
        }
        
        private void OnRoomLeft()
        {
            GD.Print($"UI: Left room");
            _isInRoom = false;
            _isHost = false;
            RefreshRoomList();
        }
        
        private void OnRoomUpdated(string roomId)
        {
            if (_isInRoom)
            {
                UpdateCurrentRoomUI();
            }
        }
        
        private void OnPlayerReady(int playerId, bool isReady)
        {
            UpdateCurrentRoomUI();
        }
        
        private void OnGameStarted(string roomId)
        {
            ShowMessage("Game starting!");
        }
        
        private void OnError(string errorMessage)
        {
            ShowMessage($"Error: {errorMessage}");
        }
        
        #endregion
        
        #region UI Updates
        
        private void RefreshRoomList()
        {
            _roomListOption.Clear();
            
            var lobbySystem = MultiplayerLobbySystem.Instance;
            if (lobbySystem != null)
            {
                var rooms = lobbySystem.GetAvailableRooms();
                foreach (var room in rooms)
                {
                    string displayText = $"{room.RoomName} ({room.CurrentPlayers}/{room.MaxPlayers}) - {room.GameMode}";
                    _roomListOption.AddItem(displayText);
                }
            }
            
            if (_roomListOption.ItemCount == 0)
            {
                _roomListOption.AddItem("No rooms available");
                _joinButton.Disabled = true;
            }
            else
            {
                _joinButton.Disabled = false;
            }
        }
        
        private void UpdateCurrentRoomUI()
        {
            var lobbySystem = MultiplayerLobbySystem.Instance;
            if (lobbySystem == null)
            {
                return;
            }
            
            var room = lobbySystem.GetCurrentRoom();
            if (room == null)
            {
                _currentRoomNameLabel.Text = "Room: -";
                _gameModeLabel.Text = "Mode: -";
                _difficultyLabel.Text = "Difficulty: -";
                _playerListContainer.QueueFreeChildren();
                return;
            }
            
            _currentRoomNameLabel.Text = $"Room: {room.RoomName}";
            _gameModeLabel.Text = $"Mode: {room.GameMode}";
            _difficultyLabel.Text = $"Difficulty: {room.Difficulty}";
            
            // 更新玩家列表
            _playerListContainer.QueueFreeChildren();
            foreach (var player in room.Players)
            {
                string status = player.IsReady ? "[READY]" : "[NOT READY]";
                string hostTag = player.IsHost ? " (HOST)" : "";
                var playerLabel = new Label
                {
                    Text = $"{player.PlayerName}{hostTag} - {status} (Lv.{player.Level})"
                };
                _playerListContainer.AddChild(playerLabel);
            }
            
            // 更新按钮可见性
            _startButton.Visible = _isHost;
            _readyButton.Visible = !_isHost;
            
            UpdateStatistics();
        }
        
        private void UpdateStatistics()
        {
            var lobbySystem = MultiplayerLobbySystem.Instance;
            if (lobbySystem == null)
            {
                return;
            }
            
            var stats = lobbySystem.GetStatistics();
            
            _roomsCreatedLabel.Text = $"Rooms Created: {stats["TotalRoomsCreated"]}";
            _roomsJoinedLabel.Text = $"Rooms Joined: {stats["TotalRoomsJoined"]}";
            _gamesPlayedLabel.Text = $"Games Played: {stats["TotalGamesPlayed"]}";
            _winsLabel.Text = $"Wins: {stats["TotalWins"]}";
            _lossesLabel.Text = $"Losses: {stats["TotalLosses"]}";
            
            float winRate = stats["TotalGamesPlayed"] > 0 
                ? (float)stats["TotalWins"] / stats["TotalGamesPlayed"] * 100 
                : 0;
            _winRateLabel.Text = $"Win Rate: {winRate:F1}%";
        }
        
        private void ShowMessage(string message)
        {
            GD.Print($"MultiplayerLobby UI: {message}");
            // 可以添加一个消息弹窗
        }
        
        #endregion
        
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Scancode == KeyList.Escape)
                {
                    Visible = false;
                }
            }
        }
        
        public void Toggle()
        {
            Visible = !Visible;
            if (Visible)
            {
                RefreshRoomList();
                UpdateStatistics();
                if (_isInRoom)
                {
                    UpdateCurrentRoomUI();
                }
            }
        }
    }
}
