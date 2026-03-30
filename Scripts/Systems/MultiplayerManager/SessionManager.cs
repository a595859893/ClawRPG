using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// SessionManager - 负责多人游戏会话管理
    /// </summary>
    public partial class SessionManager : BaseSystem
    {
        public static SessionManager Instance { get; private set; }
        
        // 房间信息
        public class RoomInfo
        {
            public string RoomId { get; set; }
            public string RoomName { get; set; }
            public int MaxPlayers { get; set; }
            public int CurrentPlayers { get; set; }
            public bool IsStarted { get; set; }
            public string HostPlayerId { get; set; }
            public string Password { get; set; }
            public DateTime CreatedTime { get; set; }
        }
        
        // 状态
        private bool _isHost = false;
        private string _currentRoomId = "";
        private int _localPlayerId = -1;
        private string _playerName = "Player";
        private string _roomPassword = "";
        private bool _needsPassword = false;
        
        // 当前房间
        private RoomInfo _currentRoom;
        
        // 踢人记录
        private HashSet<int> _kickedPlayers = new HashSet<int>();
        
        // 信号
        public delegate void RoomCreatedEventHandler(string roomId);
        public delegate void RoomJoinedEventHandler(string roomId);
        public delegate void RoomLeftEventHandler();
        public delegate void ConnectionFailedEventHandler(string reason);
        
        public bool IsInRoom => !string.IsNullOrEmpty(_currentRoomId);
        public bool IsHost => _isHost;
        public bool NeedsPassword => _needsPassword;
        public int LocalPlayerId => _localPlayerId;
        public string PlayerName => _playerName;
        
        public override void _Ready()
        {
            Instance = this;
        }
        
        public void CreateRoom(string roomName, int maxPlayers, string password = "")
        {
            _currentRoomId = Guid.NewGuid().ToString();
            _currentRoom = new RoomInfo
            {
                RoomId = _currentRoomId,
                RoomName = roomName,
                MaxPlayers = maxPlayers,
                CurrentPlayers = 1,
                IsStarted = false,
                HostPlayerId = _localPlayerId.ToString(),
                Password = password,
                CreatedTime = DateTime.Now
            };
            
            _isHost = true;
            _roomPassword = password;
            _needsPassword = !string.IsNullOrEmpty(password);
            
            GD.Print($"[SessionManager] Room created: {roomName}");
            EmitSignal(SignalName.RoomCreated, _currentRoomId);
        }
        
        public void JoinRoom(string roomId, string password = "")
        {
            _currentRoomId = roomId;
            _isHost = false;
            _roomPassword = password;
            _needsPassword = false;
            
            if (_currentRoom != null)
            {
                _currentRoom.CurrentPlayers++;
            }
            
            GD.Print($"[SessionManager] Joined room: {roomId}");
            EmitSignal(SignalName.RoomJoined, roomId);
        }
        
        public void LeaveRoom()
        {
            if (_currentRoom != null && _currentRoom.CurrentPlayers > 0)
            {
                _currentRoom.CurrentPlayers--;
            }
            
            _currentRoomId = "";
            _isHost = false;
            _currentRoom = null;
            
            GD.Print("[SessionManager] Left room");
            EmitSignal(SignalName.RoomLeft);
        }
        
        public RoomInfo GetCurrentRoom()
        {
            return _currentRoom;
        }
        
        public void SetLocalPlayerId(int playerId)
        {
            _localPlayerId = playerId;
        }
        
        public void SetPlayerName(string name)
        {
            _playerName = name;
        }
        
        public void KickPlayer(int playerId)
        {
            _kickedPlayers.Add(playerId);
            GD.Print($"[SessionManager] Player {playerId} kicked");
        }
        
        public bool IsPlayerKicked(int playerId)
        {
            return _kickedPlayers.Contains(playerId);
        }
        
        public void ClearKickedPlayers()
        {
            _kickedPlayers.Clear();
        }
        
        public void SetRoomStarted()
        {
            if (_currentRoom != null)
            {
                _currentRoom.IsStarted = true;
            }
        }

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            data["is_host"] = _isHost;
            data["current_room_id"] = _currentRoomId;
            data["local_player_id"] = _localPlayerId;
            data["player_name"] = _playerName;
            data["needs_password"] = _needsPassword;
            
            // 当前房间信息
            if (_currentRoom != null)
            {
                var roomData = new Dictionary<string, object>
                {
                    { "room_id", _currentRoom.RoomId ?? "" },
                    { "room_name", _currentRoom.RoomName ?? "" },
                    { "max_players", _currentRoom.MaxPlayers },
                    { "current_players", _currentRoom.CurrentPlayers },
                    { "is_started", _currentRoom.IsStarted },
                    { "host_player_id", _currentRoom.HostPlayerId ?? "" }
                };
                data["current_room"] = roomData;
            }
            
            // 踢人记录
            var kickedList = new List<int>();
            foreach (var id in _kickedPlayers)
            {
                kickedList.Add(id);
            }
            data["kicked_players"] = kickedList;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            _isHost = (bool)data.GetValueOrDefault("is_host", false);
            _currentRoomId = (string)data.GetValueOrDefault("current_room_id", "");
            _localPlayerId = (int)data.GetValueOrDefault("local_player_id", -1);
            _playerName = (string)data.GetValueOrDefault("player_name", "Player");
            _needsPassword = (bool)data.GetValueOrDefault("needs_password", false);
            
            // 当前房间信息
            if (data.ContainsKey("current_room") && data["current_room"] is Dictionary<string, object> roomData)
            {
                _currentRoom = new RoomInfo
                {
                    RoomId = (string)roomData.GetValueOrDefault("room_id", ""),
                    RoomName = (string)roomData.GetValueOrDefault("room_name", ""),
                    MaxPlayers = (int)roomData.GetValueOrDefault("max_players", 4),
                    CurrentPlayers = (int)roomData.GetValueOrDefault("current_players", 1),
                    IsStarted = (bool)roomData.GetValueOrDefault("is_started", false),
                    HostPlayerId = (string)roomData.GetValueOrDefault("host_player_id", "")
                };
            }
            
            // 踢人记录
            _kickedPlayers.Clear();
            if (data.ContainsKey("kicked_players") && data["kicked_players"] is List<object> kickedList)
            {
                foreach (var item in kickedList)
                {
                    _kickedPlayers.Add((int)item);
                }
            }
        }
    }
}
