using Godot;
using System;
using System.Collections;

public partial class MultiplayerManager
{
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
    
    public override void _ExitTree()
    {
        Disconnect();
        Instance = null;
    }
    
    #endregion
}
