using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 多人游戏大厅核心系统
    /// 房间创建/加入/管理/邀请/准备状态
    /// </summary>
    public class MultiplayerLobbySystem : Node
    {
        public static MultiplayerLobbySystem Instance { get; private set; }
        
        [Export] private NodePath dataPath = new NodePath("../MultiplayerLobbyData");
        [Export] private NodePath databasePath = new NodePath("../MultiplayerLobbyDatabase");
        
        private MultiplayerLobbyData _data;
        private MultiplayerLobbyDatabase _database;
        
        // 信号
        public delegate void RoomCreatedEvent(string roomId, string roomName);
        public delegate void RoomJoinedEvent(string roomId);
        public delegate void RoomLeftEvent();
        public delegate void RoomUpdatedEvent(string roomId);
        public delegate void PlayerReadyEvent(int playerId, bool isReady);
        public delegate void GameStartedEvent(string roomId);
        public delegate void InviteReceivedEvent(string fromPlayer, string roomId);
        public delegate void ErrorEvent(string errorMessage);
        
        public event RoomCreatedEvent OnRoomCreated;
        public event RoomJoinedEvent OnRoomJoined;
        public event RoomLeftEvent OnRoomLeft;
        public event RoomUpdatedEvent OnRoomUpdated;
        public event PlayerReadyEvent OnPlayerReady;
        public event GameStartedEvent OnGameStarted;
        public event InviteReceivedEvent OnInviteReceived;
        public event ErrorEvent OnError;
        
        public override void _Ready()
        {
            Instance = this;
            
            // 获取数据节点
            _data = GetNode<MultiplayerLobbyData>(dataPath);
            _database = GetNode<MultiplayerLobbyDatabase>(databasePath);
            
            if (_data == null)
            {
                GD.PrintErr("MultiplayerLobbySystem: MultiplayerLobbyData not found!");
            }
            if (_database == null)
            {
                GD.PrintErr("MultiplayerLobbySystem: MultiplayerLobbyDatabase not found!");
            }
        }
        
        #region Room Management
        
        /// <summary>
        /// 创建房间
        /// </summary>
        public string CreateRoom(string roomName, string hostName, string gameMode, int difficulty, bool isPrivate, string password = "")
        {
            if (_data == null || _database == null)
            {
                NotifyError("System not initialized");
                return "";
            }
            
            // 验证游戏模式
            var modeConfig = _database.GetGameMode(gameMode);
            if (modeConfig == null)
            {
                NotifyError("Invalid game mode");
                return "";
            }
            
            // 生成房间ID
            string roomId = GenerateRoomId();
            
            // 创建房间
            var room = new MultiplayerLobbyData.LobbyRoom
            {
                RoomId = roomId,
                RoomName = roomName,
                HostName = hostName,
                GameMode = gameMode,
                Difficulty = difficulty,
                IsPrivate = isPrivate,
                Password = isPrivate ? password : "",
                MaxPlayers = modeConfig.MaxPlayers,
                CurrentPlayers = 1,
                State = MultiplayerLobbyData.RoomState.Waiting,
                CreatedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };
            
            // 添加主机玩家
            var hostPlayer = new MultiplayerLobbyData.LobbyPlayer
            {
                PlayerId = 1,
                PlayerName = hostName,
                IsHost = true,
                IsReady = true, // 主机默认准备
                Level = 1
            };
            room.Players.Add(hostPlayer);
            
            // 保存房间
            _data.ActiveRooms[roomId] = room;
            _data.CurrentRoomId = roomId;
            _data.TotalRoomsCreated++;
            
            GD.Print($"MultiplayerLobby: Room created - {roomName} ({roomId}) by {hostName}");
            
            OnRoomCreated?.Invoke(roomId, roomName);
            
            return roomId;
        }
        
        /// <summary>
        /// 加入房间
        /// </summary>
        public bool JoinRoom(string roomId, string playerName, string password = "")
        {
            if (_data == null || _database == null)
            {
                NotifyError("System not initialized");
                return false;
            }
            
            if (!_data.ActiveRooms.ContainsKey(roomId))
            {
                NotifyError("Room not found");
                return false;
            }
            
            var room = _data.ActiveRooms[roomId];
            
            // 检查房间是否已满
            if (room.CurrentPlayers >= room.MaxPlayers)
            {
                NotifyError("Room is full");
                return false;
            }
            
            // 检查房间是否已开始
            if (room.State != MultiplayerLobbyData.RoomState.Waiting)
            {
                NotifyError("Game already started");
                return false;
            }
            
            // 检查密码
            if (room.IsPrivate && room.Password != password)
            {
                NotifyError("Invalid password");
                return false;
            }
            
            // 添加玩家
            int newPlayerId = room.CurrentPlayers + 1;
            var newPlayer = new MultiplayerLobbyData.LobbyPlayer
            {
                PlayerId = newPlayerId,
                PlayerName = playerName,
                IsHost = false,
                IsReady = false,
                Level = 1
            };
            room.Players.Add(newPlayer);
            room.CurrentPlayers++;
            
            _data.CurrentRoomId = roomId;
            _data.TotalRoomsJoined++;
            
            GD.Print($"MultiplayerLobby: {playerName} joined room {roomId}");
            
            OnRoomJoined?.Invoke(roomId);
            OnRoomUpdated?.Invoke(roomId);
            
            return true;
        }
        
        /// <summary>
        /// 离开房间
        /// </summary>
        public void LeaveRoom()
        {
            if (_data == null || string.IsNullOrEmpty(_data.CurrentRoomId))
            {
                return;
            }
            
            string roomId = _data.CurrentRoomId;
            
            if (_data.ActiveRooms.ContainsKey(roomId))
            {
                var room = _data.ActiveRooms[roomId];
                
                // 如果是主机离开，解散房间
                if (room.Players.Count > 0 && room.Players[0].IsHost)
                {
                    _data.ActiveRooms.Remove(roomId);
                    GD.Print($"MultiplayerLobby: Room {roomId} disbanded (host left)");
                }
                else
                {
                    // 普通玩家离开
                    room.CurrentPlayers--;
                    if (room.CurrentPlayers <= 0)
                    {
                        _data.ActiveRooms.Remove(roomId);
                    }
                }
            }
            
            _data.CurrentRoomId = "";
            
            OnRoomLeft?.Invoke();
        }
        
        /// <summary>
        /// 设置玩家准备状态
        /// </summary>
        public void SetPlayerReady(int playerId, bool isReady)
        {
            if (_data == null || string.IsNullOrEmpty(_data.CurrentRoomId))
            {
                return;
            }
            
            string roomId = _data.CurrentRoomId;
            if (!_data.ActiveRooms.ContainsKey(roomId))
            {
                return;
            }
            
            var room = _data.ActiveRooms[roomId];
            var player = room.Players.Find(p => p.PlayerId == playerId);
            
            if (player != null)
            {
                player.IsReady = isReady;
                OnPlayerReady?.Invoke(playerId, isReady);
                OnRoomUpdated?.Invoke(roomId);
                
                // 检查是否所有玩家都准备
                CheckAllPlayersReady(roomId);
            }
        }
        
        /// <summary>
        /// 检查是否所有玩家都准备
        /// </summary>
        private void CheckAllPlayersReady(string roomId)
        {
            if (!_data.ActiveRooms.ContainsKey(roomId))
            {
                return;
            }
            
            var room = _data.ActiveRooms[roomId];
            
            // 主机默认准备，只需要检查其他玩家
            bool allReady = true;
            foreach (var player in room.Players)
            {
                if (!player.IsHost && !player.IsReady)
                {
                    allReady = false;
                    break;
                }
            }
            
            if (allReady && room.CurrentPlayers >= 1)
            {
                // 可以开始游戏
                StartGame(roomId);
            }
        }
        
        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartGame(string roomId = "")
        {
            if (_data == null)
            {
                return;
            }
            
            if (string.IsNullOrEmpty(roomId))
            {
                roomId = _data.CurrentRoomId;
            }
            
            if (!_data.ActiveRooms.ContainsKey(roomId))
            {
                NotifyError("Room not found");
                return;
            }
            
            var room = _data.ActiveRooms[roomId];
            
            // 检查是否有足够玩家
            var modeConfig = _database.GetGameMode(room.GameMode);
            if (room.CurrentPlayers < modeConfig.MinPlayers)
            {
                NotifyError($"Need at least {modeConfig.MinPlayers} players to start");
                return;
            }
            
            room.State = MultiplayerLobbyData.RoomState.Starting;
            
            GD.Print($"MultiplayerLobby: Starting game in room {roomId}");
            
            OnGameStarted?.Invoke(roomId);
            OnRoomUpdated?.Invoke(roomId);
        }
        
        /// <summary>
        /// 获取可用房间列表
        /// </summary>
        public List<MultiplayerLobbyData.LobbyRoom> GetAvailableRooms(string gameMode = "")
        {
            var rooms = new List<MultiplayerLobbyData.LobbyRoom>();
            
            if (_data == null)
            {
                return rooms;
            }
            
            foreach (var room in _data.ActiveRooms.Values)
            {
                // 过滤条件
                if (room.State != MultiplayerLobbyData.RoomState.Waiting)
                {
                    continue;
                }
                
                if (room.IsPrivate)
                {
                    continue; // 私人房间不显示在列表中
                }
                
                if (!string.IsNullOrEmpty(gameMode) && room.GameMode != gameMode)
                {
                    continue;
                }
                
                rooms.Add(room);
            }
            
            return rooms;
        }
        
        /// <summary>
        /// 获取当前房间信息
        /// </summary>
        public MultiplayerLobbyData.LobbyRoom GetCurrentRoom()
        {
            if (_data == null || string.IsNullOrEmpty(_data.CurrentRoomId))
            {
                return null;
            }
            
            return _data.ActiveRooms.ContainsKey(_data.CurrentRoomId) 
                ? _data.ActiveRooms[_data.CurrentRoomId] 
                : null;
        }
        
        #endregion
        
        #region Invite System
        
        /// <summary>
        /// 发送邀请
        /// </summary>
        public void SendInvite(string toPlayerName, string roomId = "")
        {
            if (_data == null)
            {
                return;
            }
            
            if (string.IsNullOrEmpty(roomId))
            {
                roomId = _data.CurrentRoomId;
            }
            
            // TODO: 通过好友系统发送邀请
            GD.Print($"MultiplayerLobby: Invite sent to {toPlayerName} for room {roomId}");
        }
        
        /// <summary>
        /// 接收邀请
        /// </summary>
        public void ReceiveInvite(string fromPlayer, string roomId)
        {
            if (_data == null)
            {
                return;
            }
            
            var invite = new MultiplayerLobbyData.LobbyInvite
            {
                RoomId = roomId,
                FromPlayer = fromPlayer,
                Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };
            
            _data.PendingInvites.Add(invite);
            
            OnInviteReceived?.Invoke(fromPlayer, roomId);
        }
        
        /// <summary>
        /// 接受邀请
        /// </summary>
        public bool AcceptInvite(int inviteIndex, string playerName)
        {
            if (_data == null || inviteIndex < 0 || inviteIndex >= _data.PendingInvites.Count)
            {
                return false;
            }
            
            var invite = _data.PendingInvites[inviteIndex];
            bool success = JoinRoom(invite.RoomId, playerName);
            
            if (success)
            {
                _data.PendingInvites.RemoveAt(inviteIndex);
            }
            
            return success;
        }
        
        /// <summary>
        /// 拒绝邀请
        /// </summary>
        public void DeclineInvite(int inviteIndex)
        {
            if (_data == null || inviteIndex < 0 || inviteIndex >= _data.PendingInvites.Count)
            {
                return;
            }
            
            _data.PendingInvites.RemoveAt(inviteIndex);
        }
        
        /// <summary>
        /// 获取待处理邀请
        /// </summary>
        public List<MultiplayerLobbyData.LobbyInvite> GetPendingInvites()
        {
            return _data?.PendingInvites ?? new List<MultiplayerLobbyData.LobbyInvite>();
        }
        
        #endregion
        
        #region Statistics
        
        /// <summary>
        /// 获取统计信息
        /// </summary>
        public Dictionary<string, int> GetStatistics()
        {
            var stats = new Dictionary<string, int>();
            
            if (_data != null)
            {
                stats["TotalRoomsCreated"] = _data.TotalRoomsCreated;
                stats["TotalRoomsJoined"] = _data.TotalRoomsJoined;
                stats["TotalGamesPlayed"] = _data.TotalGamesPlayed;
                stats["TotalWins"] = _data.TotalWins;
                stats["TotalLosses"] = _data.TotalLosses;
            }
            
            return stats;
        }
        
        /// <summary>
        /// 记录游戏结果
        /// </summary>
        public void RecordGameResult(bool won)
        {
            if (_data == null)
            {
                return;
            }
            
            _data.TotalGamesPlayed++;
            
            if (won)
            {
                _data.TotalWins++;
            }
            else
            {
                _data.TotalLosses++;
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private string GenerateRoomId()
        {
            return "room_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        }
        
        private void NotifyError(string message)
        {
            GD.PrintErr($"MultiplayerLobby Error: {message}");
            OnError?.Invoke(message);
        }
        
        #endregion
    }
}
