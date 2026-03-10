using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 多人游戏管理器
/// 房间管理、玩家同步、状态同步
/// </summary>
public class MultiplayerManager : Node
{
    public static MultiplayerManager Instance { get; private set; }

    // 房间信息
    public class RoomInfo
    {
        public string RoomId;
        public string RoomName;
        public int MaxPlayers;
        public int CurrentPlayers;
        public bool IsStarted;
    }

    // 玩家信息
    public class NetworkPlayer
    {
        public int PlayerId;
        public string PlayerName;
        public Vector2 Position;
        public Vector2 Velocity;
        public int Health;
        public int MaxHealth;
        public string CurrentState;
    }

    // 信号
    public delegate void RoomCreatedEvent(string roomId);
    public delegate void RoomJoinedEvent(string roomId);
    public delegate void RoomLeftEvent();
    public delegate void PlayerJoinedEvent(int playerId, string playerName);
    public delegate void PlayerLeftEvent(int playerId);
    public delegate void PlayerStateUpdateEvent(int playerId, NetworkPlayer player);
    public delegate void ConnectionFailedEvent(string reason);

    public event RoomCreatedEvent OnRoomCreated;
    public event RoomJoinedEvent OnRoomJoined;
    public event RoomLeftEvent OnRoomLeft;
    public event PlayerJoinedEvent OnPlayerJoined;
    public event PlayerLeftEvent OnPlayerLeft;
    public event PlayerStateUpdateEvent OnPlayerStateUpdate;
    public event ConnectionFailedEvent OnConnectionFailed;

    // 状态
    private bool _isHost = false;
    private string _currentRoomId = "";
    private int _localPlayerId = -1;
    private string _playerName = "Player";
    
    // 玩家列表
    private Dictionary<int, NetworkPlayer> _networkPlayers = new Dictionary<int, NetworkPlayer>();
    private readonly object _playersLock = new object();

    // 同步设置
    private float _syncInterval = 0.05f;  // 20Hz 同步频率
    private float _syncTimer = 0f;

    public bool IsInRoom => !string.IsNullOrEmpty(_currentRoomId);
    public bool IsHost => _isHost;
    public int LocalPlayerId => _localPlayerId;
    public string PlayerName
    {
        get => _playerName;
        set => _playerName = value;
    }

    public override void _Ready()
    {
        Instance = this;
        
        // 监听网络客户端事件
        if (NetworkClient.Instance != null)
        {
            NetworkClient.Instance.OnConnected += OnConnected;
            NetworkClient.Instance.OnDisconnected += OnDisconnected;
            NetworkClient.Instance.OnMessageReceived += OnMessageReceived;
            NetworkClient.Instance.OnError += OnError;
        }
    }

    public override void _Process(float delta)
    {
        if (!IsInRoom) return;

        // 同步玩家状态
        _syncTimer += delta;
        if (_syncTimer >= _syncInterval)
        {
            _syncTimer = 0;
            SyncLocalPlayerState();
        }
    }

    /// <summary>
    /// 连接到服务器
    /// </summary>
    public void ConnectToServer(string url)
    {
        if (NetworkClient.Instance != null)
        {
            NetworkClient.Instance.Connect(url);
        }
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public void Disconnect()
    {
        if (IsInRoom)
        {
            LeaveRoom();
        }
        
        if (NetworkClient.Instance != null)
        {
            NetworkClient.Instance.Disconnect();
        }
    }

    /// <summary>
    /// 创建房间
    /// </summary>
    public void CreateRoom(string roomName, int maxPlayers = 4)
    {
        if (!NetworkClient.Instance.IsConnected)
        {
            OnConnectionFailed?.Invoke("Not connected to server");
            return;
        }

        _isHost = true;
        var message = new Dictionary<string, object>
        {
            { "type", "create_room" },
            { "room_name", roomName },
            { "max_players", maxPlayers },
            { "player_name", _playerName }
        };
        
        NetworkClient.Instance.SendJson(message);
    }

    /// <summary>
    /// 加入房间
    /// </summary>
    public void JoinRoom(string roomId)
    {
        if (!NetworkClient.Instance.IsConnected)
        {
            OnConnectionFailed?.Invoke("Not connected to server");
            return;
        }

        _isHost = false;
        var message = new Dictionary<string, object>
        {
            { "type", "join_room" },
            { "room_id", roomId },
            { "player_name", _playerName }
        };
        
        NetworkClient.Instance.SendJson(message);
    }

    /// <summary>
    /// 离开房间
    /// </summary>
    public void LeaveRoom()
    {
        if (!IsInRoom) return;

        var message = new Dictionary<string, object>
        {
            { "type", "leave_room" },
            { "room_id", _currentRoomId },
            { "player_id", _localPlayerId }
        };
        
        NetworkClient.Instance.SendJson(message);
        
        _currentRoomId = "";
        _isHost = false;
        _localPlayerId = -1;
        
        lock (_playersLock)
        {
            _networkPlayers.Clear();
        }
        
        OnRoomLeft?.Invoke();
    }

    /// <summary>
    /// 同步本地玩家状态
    /// </summary>
    private void SyncLocalPlayerState()
    {
        var player = GetPlayerNode();
        if (player == null) return;

        var state = new Dictionary<string, object>
        {
            { "type", "player_state" },
            { "room_id", _currentRoomId },
            { "player_id", _localPlayerId },
            { "position", new Dictionary<string, float> { { "x", player.Position.X }, { "y", player.Position.Y } } },
            { "velocity", new Dictionary<string, float> { { "x", 0 }, { "y", 0 } } }, // 可从 player 获取
            { "health", player.Health },
            { "max_health", player.MaxHealth }
        };
        
        NetworkClient.Instance.SendJson(state);
    }

    private Node GetPlayerNode()
    {
        var root = GetTree().Root;
        foreach (Node child in root.GetChildren())
        {
            if (child is Player player)
            {
                return player;
            }
        }
        return null;
    }

    private void OnConnected()
    {
        GD.Print("[MultiplayerManager] Connected to server");
    }

    private void OnDisconnected(string reason)
    {
        GD.Print($"[MultiplayerManager] Disconnected: {reason}");
        
        if (IsInRoom)
        {
            _currentRoomId = "";
            _isHost = false;
            OnRoomLeft?.Invoke();
        }
    }

    private void OnMessageReceived(string rawMessage)
    {
        try
        {
            var json = Godot.JSON.Parse(rawMessage);
            if (json.Error != Error.Ok) return;
            
            var data = json.Result as Dictionary<string, object>;
            if (data == null || !data.ContainsKey("type")) return;
            
            string msgType = data["type"].ToString();
            
            switch (msgType)
            {
                case "room_created":
                    HandleRoomCreated(data);
                    break;
                case "room_joined":
                    HandleRoomJoined(data);
                    break;
                case "player_joined":
                    HandlePlayerJoined(data);
                    break;
                case "player_left":
                    HandlePlayerLeft(data);
                    break;
                case "player_state":
                    HandlePlayerState(data);
                    break;
                case "error":
                    HandleError(data);
                    break;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[MultiplayerManager] Message parse error: {ex.Message}");
        }
    }

    private void HandleRoomCreated(Dictionary<string, object> data)
    {
        if (data.ContainsKey("room_id"))
        {
            _currentRoomId = data["room_id"].ToString();
            _localPlayerId = 1; // 房主总是 ID 1
            
            GD.Print($"[MultiplayerManager] Room created: {_currentRoomId}");
            OnRoomCreated?.Invoke(_currentRoomId);
        }
    }

    private void HandleRoomJoined(Dictionary<string, object> data)
    {
        if (data.ContainsKey("room_id"))
        {
            _currentRoomId = data["room_id"].ToString();
            
            if (data.ContainsKey("player_id"))
            {
                _localPlayerId = Convert.ToInt32(data["player_id"]);
            }
            
            GD.Print($"[MultiplayerManager] Joined room: {_currentRoomId} as player {_localPlayerId}");
            OnRoomJoined?.Invoke(_currentRoomId);
        }
    }

    private void HandlePlayerJoined(Dictionary<string, object> data)
    {
        int playerId = data.ContainsKey("player_id") ? Convert.ToInt32(data["player_id"]) : -1;
        string playerName = data.ContainsKey("player_name") ? data["player_name"].ToString() : "Unknown";
        
        if (playerId > 0)
        {
            var networkPlayer = new NetworkPlayer
            {
                PlayerId = playerId,
                PlayerName = playerName
            };
            
            lock (_playersLock)
            {
                _networkPlayers[playerId] = networkPlayer;
            }
            
            GD.Print($"[MultiplayerManager] Player joined: {playerName} (ID: {playerId})");
            OnPlayerJoined?.Invoke(playerId, playerName);
        }
    }

    private void HandlePlayerLeft(Dictionary<string, object> data)
    {
        int playerId = data.ContainsKey("player_id") ? Convert.ToInt32(data["player_id"]) : -1;
        
        if (playerId > 0)
        {
            lock (_playersLock)
            {
                _networkPlayers.Remove(playerId);
            }
            
            GD.Print($"[MultiplayerManager] Player left: {playerId}");
            OnPlayerLeft?.Invoke(playerId);
        }
    }

    private void HandlePlayerState(Dictionary<string, object> data)
    {
        int playerId = data.ContainsKey("player_id") ? Convert.ToInt32(data["player_id"]) : -1;
        
        if (playerId <= 0 || playerId == _localPlayerId) return;
        
        var positionData = data["position"] as Dictionary<string, object>;
        var velocityData = data["velocity"] as Dictionary<string, object>;
        
        lock (_playersLock)
        {
            if (!_networkPlayers.ContainsKey(playerId))
            {
                _networkPlayers[playerId] = new NetworkPlayer { PlayerId = playerId };
            }
            
            var player = _networkPlayers[playerId];
            
            if (positionData != null)
            {
                player.Position = new Vector2(
                    Convert.ToSingle(positionData["x"]),
                    Convert.ToSingle(positionData["y"])
                );
            }
            
            if (velocityData != null)
            {
                player.Velocity = new Vector2(
                    Convert.ToSingle(velocityData["x"]),
                    Convert.ToSingle(velocityData["y"])
                );
            }
            
            if (data.ContainsKey("health"))
                player.Health = Convert.ToInt32(data["health"]);
            if (data.ContainsKey("max_health"))
                player.MaxHealth = Convert.ToInt32(data["max_health"]);
            
            OnPlayerStateUpdate?.Invoke(playerId, player);
        }
    }

    private void HandleError(Dictionary<string, object> data)
    {
        string error = data.ContainsKey("message") ? data["message"].ToString() : "Unknown error";
        GD.PrintErr($"[MultiplayerManager] Server error: {error}");
        OnConnectionFailed?.Invoke(error);
    }

    private void OnError(string error)
    {
        GD.PrintErr($"[MultiplayerManager] Connection error: {error}");
        OnConnectionFailed?.Invoke(error);
    }

    /// <summary>
    /// 获取所有网络玩家
    /// </summary>
    public List<NetworkPlayer> GetNetworkPlayers()
    {
        lock (_playersLock)
        {
            return new List<NetworkPlayer>(_networkPlayers.Values);
        }
    }

    /// <summary>
    /// 获取指定玩家
    /// </summary>
    public NetworkPlayer GetNetworkPlayer(int playerId)
    {
        lock (_playersLock)
        {
            return _networkPlayers.ContainsKey(playerId) ? _networkPlayers[playerId] : null;
        }
    }

    public override void _ExitTree()
    {
        Disconnect();
        Instance = null;
    }
}
