using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

/// <summary>
/// 多人游戏管理器 - 协调者
/// 协调 SessionManager, LobbySystem, NetworkSyncSystem 三个子系统
/// </summary>
public partial class MultiplayerManager : BaseSystem
{
    public static MultiplayerManager Instance { get; private set; }

    // 子系统引用
    private SessionManager _sessionManager;
    private LobbySystem _lobbySystem;
    private NetworkSyncSystem _networkSyncSystem;

    // 信号 - 委托到子系统
    [Signal] public delegate void RoomCreatedEventHandler(string roomId);
    [Signal] public delegate void RoomJoinedEventHandler(string roomId);
    [Signal] public delegate void RoomLeftEventHandler();
    [Signal] public delegate void PlayerJoinedEventHandler(int playerId, string playerName);
    [Signal] public delegate void PlayerLeftEventHandler(int playerId);
    [Signal] public delegate void ConnectionFailedEventHandler(string reason);

    // 属性委托
    public bool IsInRoom => _sessionManager?.IsInRoom ?? false;
    public bool IsHost => _sessionManager?.IsHost ?? false;
    public bool IsReady => _networkSyncSystem?.IsReady ?? false;
    public bool NeedsPassword => _sessionManager?.NeedsPassword ?? false;
    public int LocalPlayerId => _sessionManager?.LocalPlayerId ?? -1;
    public string PlayerName
    {
        get => _sessionManager?.PlayerName ?? "Player";
        set
        {
            if (_sessionManager != null)
                _sessionManager.SetPlayerName(value);
        }
    }

    public override void _Ready()
    {
        Instance = this;
        
        // 获取子系统引用
        _sessionManager = GetNode<SessionManager>("SessionManager");
        _lobbySystem = GetNode<LobbySystem>("LobbySystem");
        _networkSyncSystem = GetNode<NetworkSyncSystem>("NetworkSyncSystem");
        
        // 连接子系统信号
        ConnectSubsystemSignals();
        
        GD.Print("[MultiplayerManager] Initialized as coordinator");
    }

    private void ConnectSubsystemSignals()
    {
        // SessionManager signals
        if (_sessionManager != null)
        {
            _sessionManager.Connect(SignalName.RoomCreated, Callable.From<string>(OnRoomCreated));
            _sessionManager.Connect(SignalName.RoomJoined, Callable.From<string>(OnRoomJoined));
            _sessionManager.Connect(SignalName.RoomLeft, Callable.From(OnRoomLeft));
            _sessionManager.Connect(SignalName.ConnectionFailed, Callable.From<string>(OnConnectionFailed));
        }
        
        // LobbySystem signals
        if (_lobbySystem != null)
        {
            _lobbySystem.Connect(SignalName.LobbyJoined, Callable.From<string>(OnLobbyJoined));
            _lobbySystem.Connect(SignalName.LobbyLeft, Callable.From(OnLobbyLeft));
            _lobbySystem.Connect(SignalName.PlayerJoinedLobby, Callable.From<int, string>(OnPlayerJoinedLobby));
            _lobbySystem.Connect(SignalName.PlayerLeftLobby, Callable.From<int>(OnPlayerLeftLobby));
            _lobbySystem.Connect(SignalName.PlayerReady, Callable.From<int, bool>(OnPlayerReady));
        }
        
        // NetworkSyncSystem signals
        if (_networkSyncSystem != null)
        {
            _networkSyncSystem.Connect(SignalName.PlayerJoined, Callable.From<int, string>(OnNetworkPlayerJoined));
            _networkSyncSystem.Connect(SignalName.PlayerLeft, Callable.From<int>(OnNetworkPlayerLeft));
        }
    }

    #region Connection Management

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

    #endregion

    #region Room Management (Delegate to SessionManager)

    /// <summary>
    /// 创建房间
    /// </summary>
    public void CreateRoom(string roomName, int maxPlayers = 4, string password = "")
    {
        _sessionManager?.CreateRoom(roomName, maxPlayers, password);
    }

    /// <summary>
    /// 加入房间
    /// </summary>
    public void JoinRoom(string roomId, string password = "")
    {
        _sessionManager?.JoinRoom(roomId, password);
    }

    /// <summary>
    /// 离开房间
    /// </summary>
    public void LeaveRoom()
    {
        _sessionManager?.LeaveRoom();
        _lobbySystem?.LeaveLobby();
        _networkSyncSystem?.ClearAllPlayers();
    }

    /// <summary>
    /// 踢出玩家
    /// </summary>
    public void KickPlayer(int playerId)
    {
        _sessionManager?.KickPlayer(playerId);
    }

    /// <summary>
    /// 获取当前房间信息
    /// </summary>
    public SessionManager.RoomInfo GetCurrentRoom()
    {
        return _sessionManager?.GetCurrentRoom();
    }

    #endregion

    #region Ready System (Delegate to NetworkSyncSystem)

    /// <summary>
    /// 设置准备状态
    /// </summary>
    public void SetReady(bool ready)
    {
        _networkSyncSystem?.SetPlayerReady(LocalPlayerId, ready);
    }

    /// <summary>
    /// 切换准备状态
    /// </summary>
    public void ToggleReady()
    {
        if (_networkSyncSystem != null)
        {
            SetReady(!_networkSyncSystem.IsReady);
        }
    }

    /// <summary>
    /// 检查是否所有玩家都准备好
    /// </summary>
    public bool AreAllPlayersReady()
    {
        return _networkSyncSystem?.AreAllPlayersReady() ?? false;
    }

    #endregion

    #region Lobby Management (Delegate to LobbySystem)

    /// <summary>
    /// 加入大堂
    /// </summary>
    public void JoinLobby(string lobbyId)
    {
        _lobbySystem?.JoinLobby(lobbyId);
    }

    /// <summary>
    /// 离开大堂
    /// </summary>
    public void LeaveLobby()
    {
        _lobbySystem?.LeaveLobby();
    }

    /// <summary>
    /// 获取大堂玩家列表
    /// </summary>
    public List<LobbySystem.LobbyPlayerInfo> GetLobbyPlayers()
    {
        if (_lobbySystem == null) return new List<LobbySystem.LobbyPlayerInfo>();
        
        var players = _lobbySystem.GetAllPlayers();
        return new List<LobbySystem.LobbyPlayerInfo>(players.Values);
    }

    /// <summary>
    /// 更新房间列表
    /// </summary>
    public void UpdateRoomList(List<LobbySystem.LobbyRoomInfo> rooms)
    {
        _lobbySystem?.UpdateRoomList(rooms);
    }

    /// <summary>
    /// 获取可用房间列表
    /// </summary>
    public List<LobbySystem.LobbyRoomInfo> GetAvailableRooms()
    {
        return _lobbySystem?.GetAvailableRooms() ?? new List<LobbySystem.LobbyRoomInfo>();
    }

    #endregion

    #region Player Management (Delegate to NetworkSyncSystem)

    /// <summary>
    /// 获取所有网络玩家
    /// </summary>
    public List<NetworkSyncSystem.NetworkPlayer> GetNetworkPlayers()
    {
        return _networkSyncSystem?.GetPlayerList() ?? new List<NetworkSyncSystem.NetworkPlayer>();
    }

    /// <summary>
    /// 获取指定玩家
    /// </summary>
    public NetworkSyncSystem.NetworkPlayer GetNetworkPlayer(int playerId)
    {
        return _networkSyncSystem?.GetPlayer(playerId);
    }

    #endregion

    #region Signal Handlers

    private void OnRoomCreated(string roomId)
    {
        EmitSignal(SignalName.RoomCreated, roomId);
    }

    private void OnRoomJoined(string roomId)
    {
        EmitSignal(SignalName.RoomJoined, roomId);
    }

    private void OnRoomLeft()
    {
        EmitSignal(SignalName.RoomLeft);
    }

    private void OnConnectionFailed(string reason)
    {
        EmitSignal(SignalName.ConnectionFailed, reason);
    }

    private void OnLobbyJoined(string lobbyId)
    {
        GD.Print($"[MultiplayerManager] Lobby joined: {lobbyId}");
    }

    private void OnLobbyLeft()
    {
        GD.Print("[MultiplayerManager] Lobby left");
    }

    private void OnPlayerJoinedLobby(int playerId, string playerName)
    {
        GD.Print($"[MultiplayerManager] Player joined lobby: {playerName}");
    }

    private void OnPlayerLeftLobby(int playerId)
    {
        GD.Print($"[MultiplayerManager] Player left lobby: {playerId}");
    }

    private void OnPlayerReady(int playerId, bool isReady)
    {
        GD.Print($"[MultiplayerManager] Player {playerId} ready: {isReady}");
    }

    private void OnNetworkPlayerJoined(int playerId, string playerName)
    {
        EmitSignal(SignalName.PlayerJoined, playerId, playerName);
    }

    private void OnNetworkPlayerLeft(int playerId)
    {
        EmitSignal(SignalName.PlayerLeft, playerId);
    }

    #endregion

    #region Persistence

    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        if (_sessionManager != null)
        {
            data["session"] = _sessionManager.ExportSaveData();
        }
        
        if (_lobbySystem != null)
        {
            data["lobby"] = _lobbySystem.ExportSaveData();
        }
        
        if (_networkSyncSystem != null)
        {
            data["network_sync"] = _networkSyncSystem.ExportSaveData();
        }
        
        return data;
    }

    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("session") && _sessionManager != null)
        {
            _sessionManager.ImportSaveData((Dictionary)data["session"]);
        }
        
        if (data.Contains("lobby") && _lobbySystem != null)
        {
            _lobbySystem.ImportSaveData((Dictionary)data["lobby"]);
        }
        
        if (data.Contains("network_sync") && _networkSyncSystem != null)
        {
            _networkSyncSystem.ImportSaveData((Dictionary)data["network_sync"]);
        }
    }

    #endregion

    public override void _ExitTree()
    {
        Disconnect();
        Instance = null;
    }
}
