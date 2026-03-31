using Godot;
using System;

public partial class MultiplayerManager
{
    #region 连接管理
    
    /// <summary>
    /// 连接到服务器
    /// </summary>
    public void ConnectToServer(string url)
    {
        if (NetworkClient.Instance != null)
        {
            NetworkClient.Instance.Connect(url);
            GD.Print($"[MultiplayerManager] Connecting to server: {url}");
        }
    }
    
    /// <summary>
    /// 断开连接
    /// </summary>
    public void Disconnect()
    {
        if (NetworkClient.Instance != null)
        {
            NetworkClient.Instance.Disconnect();
        }
        Session?.LeaveRoom();
        NetworkSync?.ClearAllPlayers();
        Lobby?.ClearLobby();
        GD.Print("[MultiplayerManager] Disconnected");
    }
    
    #endregion
    
    #region 房间管理 (委托给 Session)
    
    /// <summary>
    /// 创建房间
    /// </summary>
    public void CreateRoom(string roomName, int maxPlayers = 4)
    {
        if (Session != null)
        {
            Session.CreateRoom(roomName, maxPlayers);
            Session.SetLocalPlayerId(1); // 房主总是 ID 1
            Session.SetIsHost(true);
            Lobby?.AddPlayer(1, PlayerName);
            Lobby?.SetPlayerReady(1, true);
        }
    }
    
    /// <summary>
    /// 加入房间
    /// </summary>
    public void JoinRoom(string roomId)
    {
        if (Session != null)
        {
            Session.JoinRoom(roomId);
            Lobby?.AddPlayer(LocalPlayerId, PlayerName);
        }
    }
    
    /// <summary>
    /// 离开房间
    /// </summary>
    public void LeaveRoom()
    {
        Session?.LeaveRoom();
        NetworkSync?.ClearAllPlayers();
        Lobby?.ClearLobby();
        EmitSignal(SignalName.RoomLeft);
    }
    
    /// <summary>
    /// 踢出玩家
    /// </summary>
    public void KickPlayer(int playerId)
    {
        if (Session != null && IsHost)
        {
            Session.KickPlayer(playerId);
            NetworkSync?.RemovePlayer(playerId);
            Lobby?.RemovePlayer(playerId);
            GD.Print($"[MultiplayerManager] Kicked player: {playerId}");
        }
    }
    
    /// <summary>
    /// 获取房间信息
    /// </summary>
    public SessionManager.RoomInfo GetRoomInfo()
    {
        return Session?.GetRoomInfo() ?? default;
    }
    
    /// <summary>
    /// 使用密码创建房间
    /// </summary>
    public void CreateRoomWithPassword(string roomName, string password, int maxPlayers = 4)
    {
        if (Session != null)
        {
            Session.CreateRoom(roomName, maxPlayers, password);
            Session.SetLocalPlayerId(1);
            Session.SetIsHost(true);
            Session.SetNeedsPassword(true);
            Lobby?.AddPlayer(1, PlayerName);
            GD.Print($"[MultiplayerManager] Room created with password: {roomName}");
        }
    }
    
    /// <summary>
    /// 使用密码加入房间
    /// </summary>
    public void JoinRoomWithPassword(string roomId, string password)
    {
        if (Session != null)
        {
            Session.JoinRoom(roomId, password);
            Lobby?.AddPlayer(LocalPlayerId, PlayerName);
        }
    }
    
    #endregion
}
