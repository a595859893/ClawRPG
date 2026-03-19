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
        [Signal] public delegate void RoomCreatedEventHandler(string roomId);
        [Signal] public delegate void RoomJoinedEventHandler(string roomId);
        [Signal] public delegate void RoomLeftEventHandler();
        [Signal] public delegate void ConnectionFailedEventHandler(string reason);
        
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

        public override Dictionary ExportSaveData() => new();
        public override void ImportSaveData(Dictionary data) { }
    }
}
