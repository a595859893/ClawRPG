using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// LobbySystem - 负责大堂管理（房间列表、玩家大厅等）
    /// </summary>
    public partial class LobbySystem : BaseSystem
    {
        public static LobbySystem Instance { get; private set; }
        
        // 大堂玩家信息
        public class LobbyPlayerInfo
        {
            public int PlayerId { get; set; }
            public string PlayerName { get; set; }
            public bool IsReady { get; set; }
            public bool IsHost { get; set; }
            public int TeamId { get; set; }
            public DateTime JoinedTime { get; set; }
        }
        
        // 大堂房间信息
        public class LobbyRoomInfo
        {
            public string RoomId { get; set; }
            public string RoomName { get; set; }
            public string HostName { get; set; }
            public int PlayerCount { get; set; }
            public int MaxPlayers { get; set; }
            public bool HasPassword { get; set; }
            public string GameMode { get; set; }
            public int Ping { get; set; }
        }
        
        // 状态
        private bool _isInLobby = false;
        private string _lobbyId = "";
        private Dictionary<int, LobbyPlayerInfo> _lobbyPlayers = new Dictionary<int, LobbyPlayerInfo>();
        private List<LobbyRoomInfo> _availableRooms = new List<LobbyRoomInfo>();
        
        // 信号
        [Signal] public delegate void LobbyJoinedEventHandler(string lobbyId);
        [Signal] public delegate void LobbyLeftEventHandler();
        [Signal] public delegate void PlayerJoinedLobbyEventHandler(int playerId, string playerName);
        [Signal] public delegate void PlayerLeftLobbyEventHandler(int playerId);
        [Signal] public delegate void PlayerReadyEventHandler(int playerId, bool isReady);
        [Signal] public delegate void RoomListUpdatedEventHandler();
        [Signal] public delegate void LobbyChatMessageEventHandler(int playerId, string message);
        
        public bool IsInLobby => _isInLobby;
        public string LobbyId => _lobbyId;
        public int PlayerCount => _lobbyPlayers.Count;
        
        public override void _Ready()
        {
            Instance = this;
        }
        
        #region 大堂管理
        
        public void JoinLobby(string lobbyId)
        {
            _lobbyId = lobbyId;
            _isInLobby = true;
            _lobbyPlayers.Clear();
            
            GD.Print($"[LobbySystem] Joined lobby: {lobbyId}");
            EmitSignal(SignalName.LobbyJoined, lobbyId);
        }
        
        public void LeaveLobby()
        {
            _lobbyId = "";
            _isInLobby = false;
            _lobbyPlayers.Clear();
            
            GD.Print("[LobbySystem] Left lobby");
            EmitSignal(SignalName.LobbyLeft);
        }
        
        #endregion
        
        #region 玩家管理
        
        public void AddPlayer(int playerId, string playerName, bool isHost = false)
        {
            var playerInfo = new LobbyPlayerInfo
            {
                PlayerId = playerId,
                PlayerName = playerName,
                IsReady = false,
                IsHost = isHost,
                TeamId = 0,
                JoinedTime = DateTime.Now
            };
            
            _lobbyPlayers[playerId] = playerInfo;
            
            GD.Print($"[LobbySystem] Player joined: {playerName} (ID: {playerId})");
            EmitSignal(SignalName.PlayerJoinedLobby, playerId, playerName);
        }
        
        public void RemovePlayer(int playerId)
        {
            if (_lobbyPlayers.ContainsKey(playerId))
            {
                var playerName = _lobbyPlayers[playerId].PlayerName;
                _lobbyPlayers.Remove(playerId);
                
                GD.Print($"[LobbySystem] Player left: {playerName} (ID: {playerId})");
                EmitSignal(SignalName.PlayerLeftLobby, playerId);
            }
        }
        
        public void SetPlayerReady(int playerId, bool isReady)
        {
            if (_lobbyPlayers.ContainsKey(playerId))
            {
                _lobbyPlayers[playerId].IsReady = isReady;
                
                GD.Print($"[LobbySystem] Player {playerId} ready: {isReady}");
                EmitSignal(SignalName.PlayerReady, playerId, isReady);
            }
        }
        
        public void SetPlayerTeam(int playerId, int teamId)
        {
            if (_lobbyPlayers.ContainsKey(playerId))
            {
                _lobbyPlayers[playerId].TeamId = teamId;
            }
        }
        
        public LobbyPlayerInfo GetPlayerInfo(int playerId)
        {
            return _lobbyPlayers.GetValueOrDefault(playerId);
        }
        
        public Dictionary<int, LobbyPlayerInfo> GetAllPlayers()
        {
            return new Dictionary<int, LobbyPlayerInfo>(_lobbyPlayers);
        }
        
        public bool AreAllPlayersReady()
        {
            foreach (var player in _lobbyPlayers.Values)
            {
                if (!player.IsHost && !player.IsReady)
                {
                    return false;
                }
            }
            return _lobbyPlayers.Count > 0;
        }
        
        #endregion
        
        #region 房间列表
        
        public void UpdateRoomList(List<LobbyRoomInfo> rooms)
        {
            _availableRooms = rooms;
            
            GD.Print($"[LobbySystem] Room list updated: {rooms.Count} rooms");
            EmitSignal(SignalName.RoomListUpdated);
        }
        
        public void AddRoom(LobbyRoomInfo room)
        {
            _availableRooms.Add(room);
            EmitSignal(SignalName.RoomListUpdated);
        }
        
        public void RemoveRoom(string roomId)
        {
            _availableRooms.RemoveAll(r => r.RoomId == roomId);
            EmitSignal(SignalName.RoomListUpdated);
        }
        
        public List<LobbyRoomInfo> GetAvailableRooms()
        {
            return new List<LobbyRoomInfo>(_availableRooms);
        }
        
        public LobbyRoomInfo GetRoomInfo(string roomId)
        {
            return _availableRooms.Find(r => r.RoomId == roomId);
        }
        
        #endregion
        
        #region 大堂聊天
        
        public void SendChatMessage(int playerId, string message)
        {
            GD.Print($"[LobbySystem] Chat from {playerId}: {message}");
            EmitSignal(SignalName.LobbyChatMessage, playerId, message);
        }
        
        #endregion
        
        #region 持久化
        
        public Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            data["is_in_lobby"] = _isInLobby;
            data["lobby_id"] = _lobbyId;
            
            // 保存玩家信息
            var playersData = new Array();
            foreach (var player in _lobbyPlayers.Values)
            {
                var playerDict = new Dictionary();
                playerDict["player_id"] = player.PlayerId;
                playerDict["player_name"] = player.PlayerName;
                playerDict["is_ready"] = player.IsReady;
                playerDict["is_host"] = player.IsHost;
                playerDict["team_id"] = player.TeamId;
                playersData.Add(playerDict);
            }
            data["lobby_players"] = playersData;
            
            return data;
        }
        
        public void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            _isInLobby = Convert.ToBoolean(data.GetValueOrDefault("is_in_lobby", false));
            _lobbyId = data.GetValueOrDefault("lobby_id", "")?.ToString() ?? "";
            
            // 恢复玩家信息
            _lobbyPlayers.Clear();
            if (data.Contains("lobby_players"))
            {
                var playersData = data["lobby_players"] as Array;
                if (playersData != null)
                {
                    foreach (Dictionary playerDict in playersData)
                    {
                        var player = new LobbyPlayerInfo
                        {
                            PlayerId = Convert.ToInt32(playerDict.GetValueOrDefault("player_id", 0)),
                            PlayerName = playerDict.GetValueOrDefault("player_name", "")?.ToString() ?? "",
                            IsReady = Convert.ToBoolean(playerDict.GetValueOrDefault("is_ready", false)),
                            IsHost = Convert.ToBoolean(playerDict.GetValueOrDefault("is_host", false)),
                            TeamId = Convert.ToInt32(playerDict.GetValueOrDefault("team_id", 0))
                        };
                        _lobbyPlayers[player.PlayerId] = player;
                    }
                }
            }
        }
        
        #endregion
    }
}
