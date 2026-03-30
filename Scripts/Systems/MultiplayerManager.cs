using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.CoopSession;

/// <summary>
/// 多人游戏管理器 - 协调者
/// 委托给三个子系统：
/// - SessionManager: 会话管理
/// - NetworkSyncSystem: 网络状态同步
/// - LobbySystem: 大堂管理
/// </summary>
public class MultiplayerManager : BaseSystem
{
    public static MultiplayerManager Instance { get; private set; }
    
    // 三个子系统
    public SessionManager Session { get; private set; }
    public NetworkSyncSystem NetworkSync { get; private set; }
    public LobbySystem Lobby { get; private set; }
    
    // 信号 - 委托给子系统
    public delegate void RoomCreatedEventHandler(string roomId);
    public delegate void RoomJoinedEventHandler(string roomId);
    public delegate void RoomLeftEventHandler();
    public delegate void ConnectionFailedEventHandler(string reason);
    
    // 兼容旧接口的属性
    public bool IsInRoom => Session?.IsInRoom ?? false;
    public bool IsHost => Session?.IsHost ?? false;
    public bool IsReady => NetworkSync?.IsReady ?? false;
    public bool NeedsPassword => Session?.NeedsPassword ?? false;
    public int LocalPlayerId => Session?.LocalPlayerId ?? -1;
    public string PlayerName
    {
        get => Session?.PlayerName ?? "Player";
        set 
        {
            if (Session != null) Session.SetPlayerName(value);
            if (Lobby != null) Lobby.SetLocalPlayerName(value);
        }
    }
    
    public override void _Ready()
    {
        Instance = this;
        
        // 创建三个子系统
        Session = new SessionManager();
        NetworkSync = new NetworkSyncSystem();
        Lobby = new LobbySystem();
        
        // 添加为子节点
        AddChild(Session);
        AddChild(NetworkSync);
        AddChild(Lobby);
        
        // 初始化子系统
        Session._Ready();
        NetworkSync._Ready();
        Lobby._Ready();
        
        // 转发子系统的信号
        ConnectSubsystemSignals();
        
        // 监听网络客户端事件
        if (NetworkClient.Instance != null)
        {
            NetworkClient.Instance.OnConnected += OnConnected;
            NetworkClient.Instance.OnDisconnected += OnDisconnected;
            NetworkClient.Instance.OnMessageReceived += OnMessageReceived;
            NetworkClient.Instance.OnError += OnError;
        }
        
        GD.Print("[MultiplayerManager] Initialized with 3 subsystems");
    }
    
    private void ConnectSubsystemSignals()
    {
        // Session 信号转发
        if (Session != null)
        {
            Session.RoomCreated += (roomId) => EmitSignal(SignalName.RoomCreated, roomId);
            Session.RoomJoined += (roomId) => EmitSignal(SignalName.RoomJoined, roomId);
            Session.RoomLeft += () => EmitSignal(SignalName.RoomLeft);
            Session.ConnectionFailed += (reason) => EmitSignal(SignalName.ConnectionFailed, reason);
        }
    }
    
    public override void _Process(double delta)
    {
        // 子系统各自处理自己的逻辑
    }
    
    #region 连接管理
    
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
        
        // 清空所有子系统
        Session?.ClearKickedPlayers();
        NetworkSync?.ClearAllPlayers();
        Lobby?.ClearLobby();
    }
    
    #endregion
    
    #region 房间管理 (委托给 Session)
    
    /// <summary>
    /// 创建房间
    /// </summary>
    public void CreateRoom(string roomName, int maxPlayers = 4)
    {
        if (!NetworkClient.Instance.IsConnected)
        {
            EmitSignal(SignalName.ConnectionFailed, "Not connected to server");
            return;
        }
        
        var message = new Dictionary<string, object>
        {
            { "type", "create_room" },
            { "room_name", roomName },
            { "max_players", maxPlayers },
            { "player_name", PlayerName }
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
            EmitSignal(SignalName.ConnectionFailed, "Not connected to server");
            return;
        }
        
        var message = new Dictionary<string, object>
        {
            { "type", "join_room" },
            { "room_id", roomId },
            { "player_name", PlayerName }
        };
        
        NetworkClient.Instance.SendJson(message);
    }
    
    /// <summary>
    /// 离开房间
    /// </summary>
    public void LeaveRoom()
    {
        if (!IsInRoom) return;
        
        var roomId = Session?.GetCurrentRoom()?.RoomId ?? "";
        
        var message = new Dictionary<string, object>
        {
            { "type", "leave_room" },
            { "room_id", roomId },
            { "player_id", LocalPlayerId }
        };
        
        NetworkClient.Instance.SendJson(message);
        
        Session?.LeaveRoom();
        NetworkSync?.ClearAllPlayers();
        Lobby?.ClearLobby();
        
        EmitSignal(SignalName.RoomLeft);
    }
    
    /// <summary>
    /// 踢出玩家（仅房主可用）
    /// </summary>
    public void KickPlayer(int playerId)
    {
        if (!IsHost || !IsInRoom) return;
        
        var roomId = Session?.GetCurrentRoom()?.RoomId ?? "";
        
        var message = new Dictionary<string, object>
        {
            { "type", "kick_player" },
            { "room_id", roomId },
            { "target_player_id", playerId },
            { "kicker_id", LocalPlayerId }
        };
        
        NetworkClient.Instance.SendJson(message);
        Session?.KickPlayer(playerId);
    }
    
    /// <summary>
    /// 获取房间信息
    /// </summary>
    public SessionManager.RoomInfo GetRoomInfo()
    {
        return Session?.GetCurrentRoom();
    }
    
    /// <summary>
    /// 创建密码房间
    /// </summary>
    public void CreateRoomWithPassword(string roomName, string password, int maxPlayers = 4)
    {
        if (!NetworkClient.Instance.IsConnected)
        {
            EmitSignal(SignalName.ConnectionFailed, "Not connected to server");
            return;
        }
        
        var message = new Dictionary<string, object>
        {
            { "type", "create_room" },
            { "room_name", roomName },
            { "password", password },
            { "max_players", maxPlayers },
            { "player_name", PlayerName }
        };
        
        NetworkClient.Instance.SendJson(message);
    }
    
    /// <summary>
    /// 加入密码房间
    /// </summary>
    public void JoinRoomWithPassword(string roomId, string password)
    {
        if (!NetworkClient.Instance.IsConnected)
        {
            EmitSignal(SignalName.ConnectionFailed, "Not connected to server");
            return;
        }
        
        var message = new Dictionary<string, object>
        {
            { "type", "join_room" },
            { "room_id", roomId },
            { "password", password },
            { "player_name", PlayerName }
        };
        
        NetworkClient.Instance.SendJson(message);
    }
    
    #endregion
    
    #region 准备状态管理 (委托给 NetworkSync)
    
    /// <summary>
    /// 设置准备状态
    /// </summary>
    public void SetReady(bool ready)
    {
        if (!IsInRoom) return;
        
        NetworkSync?.SetPlayerReady(LocalPlayerId, ready);
        
        var roomId = Session?.GetCurrentRoom()?.RoomId ?? "";
        var message = new Dictionary<string, object>
        {
            { "type", "player_ready" },
            { "room_id", roomId },
            { "player_id", LocalPlayerId },
            { "ready", ready }
        };
        
        NetworkClient.Instance.SendJson(message);
    }
    
    /// <summary>
    /// 切换准备状态
    /// </summary>
    public void ToggleReady()
    {
        SetReady(!IsReady);
    }
    
    /// <summary>
    /// 检查是否所有玩家都准备好
    /// </summary>
    public bool AreAllPlayersReady()
    {
        return NetworkSync?.AreAllPlayersReady() ?? false;
    }
    
    #endregion
    
    #region 玩家数据访问
    
    /// <summary>
    /// 获取所有网络玩家
    /// </summary>
    public List<NetworkSyncSystem.NetworkPlayer> GetNetworkPlayers()
    {
        if (NetworkSync == null) return new List<NetworkSyncSystem.NetworkPlayer>();
        return NetworkSync.GetPlayerList();
    }
    
    /// <summary>
    /// 获取指定玩家
    /// </summary>
    public NetworkSyncSystem.NetworkPlayer GetNetworkPlayer(int playerId)
    {
        return NetworkSync?.GetPlayer(playerId);
    }
    
    #endregion
    
    #region 网络事件处理
    
    private void OnConnected()
    {
        GD.Print("[MultiplayerManager] Connected to server");
    }
    
    private void OnDisconnected(string reason)
    {
        GD.Print($"[MultiplayerManager] Disconnected: {reason}");
        
        if (IsInRoom)
        {
            Session?.LeaveRoom();
            NetworkSync?.ClearAllPlayers();
            Lobby?.ClearLobby();
            EmitSignal(SignalName.RoomLeft);
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
                case "player_kicked":
                    HandlePlayerKicked(data);
                    break;
                case "player_ready_update":
                    HandlePlayerReadyUpdate(data);
                    break;
                case "room_locked":
                    HandleRoomLocked(data);
                    break;
                case "battle_action":
                    HandleBattleAction(data);
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
            string roomId = data["room_id"].ToString();
            int playerId = 1; // 房主总是 ID 1
            
            Session?.CreateRoom(data.ContainsKey("room_name") ? data["room_name"].ToString() : "Room", 
                data.ContainsKey("max_players") ? Convert.ToInt32(data["max_players"]) : 4);
            Session?.SetLocalPlayerId(playerId);
            
            // 添加到 Lobby
            Lobby?.AddPlayer(playerId, PlayerName);
            
            GD.Print($"[MultiplayerManager] Room created: {roomId}");
            EmitSignal(SignalName.RoomCreated, roomId);
        }
    }
    
    private void HandleRoomJoined(Dictionary<string, object> data)
    {
        if (data.ContainsKey("room_id"))
        {
            string roomId = data["room_id"].ToString();
            int playerId = data.ContainsKey("player_id") ? Convert.ToInt32(data["player_id"]) : -1;
            
            Session?.JoinRoom(roomId);
            if (playerId > 0)
            {
                Session?.SetLocalPlayerId(playerId);
                Lobby?.AddPlayer(playerId, PlayerName);
            }
            
            GD.Print($"[MultiplayerManager] Joined room: {roomId} as player {playerId}");
            EmitSignal(SignalName.RoomJoined, roomId);
        }
    }
    
    private void HandlePlayerJoined(Dictionary<string, object> data)
    {
        int playerId = data.ContainsKey("player_id") ? Convert.ToInt32(data["player_id"]) : -1;
        string playerName = data.ContainsKey("player_name") ? data["player_name"].ToString() : "Unknown";
        
        if (playerId > 0)
        {
            NetworkSync?.AddPlayer(playerId, playerName);
            Lobby?.AddPlayer(playerId, playerName);
            
            GD.Print($"[MultiplayerManager] Player joined: {playerName} (ID: {playerId})");
        }
    }
    
    private void HandlePlayerLeft(Dictionary<string, object> data)
    {
        int playerId = data.ContainsKey("player_id") ? Convert.ToInt32(data["player_id"]) : -1;
        
        if (playerId > 0)
        {
            NetworkSync?.RemovePlayer(playerId);
            Lobby?.RemovePlayer(playerId);
            
            GD.Print($"[MultiplayerManager] Player left: {playerId}");
        }
    }
    
    private void HandlePlayerState(Dictionary<string, object> data)
    {
        int playerId = data.ContainsKey("player_id") ? Convert.ToInt32(data["player_id"]) : -1;
        
        if (playerId <= 0 || playerId == LocalPlayerId) return;
        
        var positionData = data["position"] as Dictionary<string, object>;
        var velocityData = data["velocity"] as Dictionary<string, object>;
        
        Vector2 position = Vector2.Zero;
        Vector2 velocity = Vector2.Zero;
        
        if (positionData != null)
        {
            position = new Vector2(
                Convert.ToSingle(positionData.GetValueOrDefault("x", 0f)),
                Convert.ToSingle(positionData.GetValueOrDefault("y", 0f))
            );
        }
        
        if (velocityData != null)
        {
            velocity = new Vector2(
                Convert.ToSingle(velocityData.GetValueOrDefault("x", 0f)),
                Convert.ToSingle(velocityData.GetValueOrDefault("y", 0f))
            );
        }
        
        string state = data.ContainsKey("state") ? data["state"].ToString() : "idle";
        NetworkSync?.UpdatePlayerState(playerId, position, velocity, state);
        
        if (data.ContainsKey("health"))
        {
            int health = Convert.ToInt32(data["health"]);
            int maxHealth = Convert.ToInt32(data.GetValueOrDefault("max_health", 100));
            NetworkSync?.UpdatePlayerHealth(playerId, health, maxHealth);
        }
    }
    
    private void HandleError(Dictionary<string, object> data)
    {
        string error = data.ContainsKey("message") ? data["message"].ToString() : "Unknown error";
        GD.PrintErr($"[MultiplayerManager] Server error: {error}");
        EmitSignal(SignalName.ConnectionFailed, error);
    }
    
    private void HandlePlayerKicked(Dictionary<string, object> data)
    {
        int kickedId = data.ContainsKey("player_id") ? Convert.ToInt32(data["player_id"]) : -1;
        
        if (kickedId == LocalPlayerId)
        {
            GD.Print("[MultiplayerManager] You were kicked from the room");
            Session?.LeaveRoom();
            NetworkSync?.ClearAllPlayers();
            Lobby?.ClearLobby();
            EmitSignal(SignalName.RoomLeft);
        }
        else
        {
            NetworkSync?.RemovePlayer(kickedId);
            Lobby?.RemovePlayer(kickedId);
            GD.Print($"[MultiplayerManager] Player {kickedId} was kicked");
        }
    }
    
    private void HandlePlayerReadyUpdate(Dictionary<string, object> data)
    {
        int playerId = data.ContainsKey("player_id") ? Convert.ToInt32(data["player_id"]) : -1;
        bool isReady = data.ContainsKey("ready") && Convert.ToBoolean(data["ready"]);
        
        if (playerId > 0)
        {
            NetworkSync?.SetPlayerReady(playerId, isReady);
            Lobby?.SetPlayerReady(playerId, isReady);
            GD.Print($"[MultiplayerManager] Player {playerId} ready: {isReady}");
        }
    }
    
    private void HandleRoomLocked(Dictionary<string, object> data)
    {
        string reason = data.ContainsKey("reason") ? data["reason"].ToString() : "Unknown reason";
        GD.Print($"[MultiplayerManager] Room locked: {reason}");
        EmitSignal(SignalName.ConnectionFailed, reason);
    }
    
    private void HandleBattleAction(Dictionary<string, object> data)
    {
        try
        {
            if (!data.ContainsKey("actions")) return;
            
            var actions = data["actions"] as ArrayList;
            if (actions == null) return;
            
            foreach (Dictionary<string, object> actionData in actions)
            {
                var action = new BattleSyncData.BattleAction
                {
                    ActionId = actionData.ContainsKey("actionId") ? actionData["actionId"].ToString() : "",
                    PlayerId = actionData.ContainsKey("playerId") ? Convert.ToInt32(actionData["playerId"]) : 0,
                    PlayerName = actionData.ContainsKey("playerName") ? actionData["playerName"].ToString() : "",
                    SkillId = actionData.ContainsKey("skillId") ? actionData["skillId"].ToString() : "",
                    Value = actionData.ContainsKey("value") ? Convert.ToSingle(actionData["value"]) : 0f,
                    TargetX = actionData.ContainsKey("targetX") ? Convert.ToSingle(actionData["targetX"]) : 0f,
                    TargetY = actionData.ContainsKey("targetY") ? Convert.ToSingle(actionData["targetY"]) : 0f,
                    TargetId = actionData.ContainsKey("targetId") ? Convert.ToInt32(actionData["targetId"]) : 0,
                    IsCritical = actionData.ContainsKey("isCritical") && Convert.ToBoolean(actionData["isCritical"]),
                    Timestamp = actionData.ContainsKey("timestamp") ? Convert.ToInt64(actionData["timestamp"]) : 0
                };
                
                if (actionData.ContainsKey("type"))
                {
                    if (Enum.TryParse<BattleActionType>(actionData["type"].ToString(), out var actionType))
                    {
                        action.Type = actionType;
                    }
                }
                
                if (BattleSyncSystem.Instance != null)
                {
                    BattleSyncSystem.Instance.ReceiveRemoteAction(action);
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[MultiplayerManager] HandleBattleAction error: {ex.Message}");
        }
    }
    
    private void OnError(string error)
    {
        GD.PrintErr($"[MultiplayerManager] Connection error: {error}");
        EmitSignal(SignalName.ConnectionFailed, error);
    }
    
    #endregion
    
    public override void _ExitTree()
    {
        Disconnect();
        Instance = null;
    }
    
    #region 数据持久化
    
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        if (Session != null) data["session"] = Session.ExportSaveData();
        if (NetworkSync != null) data["network_sync"] = NetworkSync.ExportSaveData();
        if (Lobby != null) data["lobby"] = Lobby.ExportSaveData();
        
        return data;
    }
    
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("session") && Session != null)
        {
            Session.ImportSaveData((Dictionary)data["session"]);
        }
        
        if (data.Contains("network_sync") && NetworkSync != null)
        {
            NetworkSync.ImportSaveData((Dictionary)data["network_sync"]);
        }
        
        if (data.Contains("lobby") && Lobby != null)
        {
            Lobby.ImportSaveData((Dictionary)data["lobby"]);
        }
    }
    
    #endregion
}
