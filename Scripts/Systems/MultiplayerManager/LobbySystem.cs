using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// LobbySystem - 负责大堂管理
    /// 房间创建、加入、离开、玩家列表管理等
    /// </summary>
    public partial class LobbySystem : BaseSystem
    {
        public static LobbySystem Instance { get; private set; }
        
        // 玩家信息
        public class LobbyPlayer
        {
            public int PlayerId { get; set; }
            public string PlayerName { get; set; }
            public bool IsReady { get; set; }
            public bool IsHost { get; set; }
            public int Ping { get; set; }
            public DateTime JoinTime { get; set; }
        }
        
        // 房间信息
        public class LobbyRoomInfo
        {
            public string RoomId { get; set; }
            public string RoomName { get; set; }
            public string HostName { get; set; }
            public int MaxPlayers { get; set; }
            public int CurrentPlayers { get; set; }
            public bool IsPasswordProtected { get; set; }
            public bool IsStarted { get; set; }
            public string GameMode { get; set; }
            public DateTime CreatedTime { get; set; }
        }
        
        // 大堂状态
        private bool _isInLobby = false;
        private string _currentLobbyId = "";
        private bool _isLobbyHost = false;
        
        // 当前大堂的玩家列表
        private Dictionary<int, LobbyPlayer> _lobbyPlayers = new Dictionary<int, LobbyPlayer>();
        private readonly object _playersLock = new object();
        
        // 本地玩家信息
        private int _localPlayerId = -1;
        private string _localPlayerName = "Player";
        
        // 信号
        public delegate void LobbyJoinedEventHandler(string lobbyId);
        public delegate void LobbyLeftEventHandler();
        public delegate void PlayerJoinedLobbyEventHandler(int playerId, string playerName);
        public delegate void PlayerLeftLobbyEventHandler(int playerId);
        public delegate void PlayerReadyEventHandler(int playerId, bool isReady);
        public delegate void HostChangedEventHandler(int newHostId);
        public delegate void LobbyFullEventHandler();
        public delegate void LobbyErrorEventHandler(string error);
        
        public bool IsInLobby => _isInLobby;
        public bool IsHost => _isLobbyHost;
        public int LocalPlayerId => _localPlayerId;
        public string LocalPlayerName => _localPlayerName;
        public int PlayerCount => _lobbyPlayers.Count;
        
        public override void _Ready()
        {
            Instance = this;
        }
        
        #region 公共接口
        
        /// <summary>
        /// 创建大堂
        /// </summary>
        public void CreateLobby(string lobbyName, int maxPlayers = 4, string password = "", string gameMode = "survival")
        {
            _currentLobbyId = Guid.NewGuid().ToString();
            _isLobbyHost = true;
            _isInLobby = true;
            
            // 添加房主到玩家列表
            var hostPlayer = new LobbyPlayer
            {
                PlayerId = _localPlayerId,
                PlayerName = _localPlayerName,
                IsReady = false,
                IsHost = true,
                Ping = 0,
                JoinTime = DateTime.Now
            };
            
            lock (_playersLock)
            {
                _lobbyPlayers[_localPlayerId] = hostPlayer;
            }
            
            GD.Print($"[LobbySystem] Lobby created: {lobbyName}, ID: {_currentLobbyId}");
            EmitSignal(SignalName.LobbyJoined, _currentLobbyId);
        }
        
        /// <summary>
        /// 加入大堂
        /// </summary>
        public void JoinLobby(string lobbyId, string password = "")
        {
            _currentLobbyId = lobbyId;
            _isLobbyHost = false;
            _isInLobby = true;
            
            GD.Print($"[LobbySystem] Joined lobby: {lobbyId}");
            EmitSignal(SignalName.LobbyJoined, lobbyId);
        }
        
        /// <summary>
        /// 离开大堂
        /// </summary>
        public void LeaveLobby()
        {
            if (!_isInLobby) return;
            
            lock (_playersLock)
            {
                _lobbyPlayers.Clear();
            }
            
            string leftLobbyId = _currentLobbyId;
            _currentLobbyId = "";
            _isInLobby = false;
            _isLobbyHost = false;
            
            GD.Print($"[LobbySystem] Left lobby: {leftLobbyId}");
            EmitSignal(SignalName.LobbyLeft);
        }
        
        /// <summary>
        /// 设置玩家准备状态
        /// </summary>
        public void SetPlayerReady(int playerId, bool ready)
        {
            lock (_playersLock)
            {
                if (_lobbyPlayers.ContainsKey(playerId))
                {
                    _lobbyPlayers[playerId].IsReady = ready;
                    GD.Print($"[LobbySystem] Player {playerId} ready: {ready}");
                    EmitSignal(SignalName.PlayerReady, playerId, ready);
                }
            }
        }
        
        /// <summary>
        /// 切换准备状态（本地玩家）
        /// </summary>
        public void ToggleReady()
        {
            if (_localPlayerId <= 0) return;
            
            bool currentReady = false;
            lock (_playersLock)
            {
                if (_lobbyPlayers.ContainsKey(_localPlayerId))
                {
                    currentReady = _lobbyPlayers[_localPlayerId].IsReady;
                }
            }
            
            SetPlayerReady(_localPlayerId, !currentReady);
        }
        
        /// <summary>
        /// 添加玩家到大堂
        /// </summary>
        public void AddPlayer(int playerId, string playerName)
        {
            lock (_playersLock)
            {
                if (!_lobbyPlayers.ContainsKey(playerId))
                {
                    _lobbyPlayers[playerId] = new LobbyPlayer
                    {
                        PlayerId = playerId,
                        PlayerName = playerName,
                        IsReady = false,
                        IsHost = false,
                        Ping = 0,
                        JoinTime = DateTime.Now
                    };
                    
                    GD.Print($"[LobbySystem] Player joined: {playerName} (ID: {playerId})");
                    EmitSignal(SignalName.PlayerJoinedLobby, playerId, playerName);
                }
            }
        }
        
        /// <summary>
        /// 从大堂移除玩家
        /// </summary>
        public void RemovePlayer(int playerId)
        {
            lock (_playersLock)
            {
                if (_lobbyPlayers.ContainsKey(playerId))
                {
                    string playerName = _lobbyPlayers[playerId].PlayerName;
                    bool wasHost = _lobbyPlayers[playerId].IsHost;
                    _lobbyPlayers.Remove(playerId);
                    
                    GD.Print($"[LobbySystem] Player left: {playerName} (ID: {playerId})");
                    EmitSignal(SignalName.PlayerLeftLobby, playerId);
                    
                    // 如果离开的是房主，转移房主权限
                    if (wasHost && _lobbyPlayers.Count > 0)
                    {
                        int newHostId = GetNextHost();
                        if (newHostId > 0)
                        {
                            _lobbyPlayers[newHostId].IsHost = true;
                            _isLobbyHost = true;
                            GD.Print($"[LobbySystem] New host: {newHostId}");
                            EmitSignal(SignalName.HostChanged, newHostId);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取玩家信息
        /// </summary>
        public LobbyPlayer GetPlayer(int playerId)
        {
            lock (_playersLock)
            {
                return _lobbyPlayers.ContainsKey(playerId) ? _lobbyPlayers[playerId] : null;
            }
        }
        
        /// <summary>
        ///获取所有玩家列表
        /// </summary>
        public List<LobbyPlayer> GetPlayerList()
        {
            lock (_playersLock)
            {
                return new List<LobbyPlayer>(_lobbyPlayers.Values);
            }
        }
        
        /// <summary>
        /// 获取玩家数量
        /// </summary>
        public int GetPlayerCount()
        {
            lock (_playersLock)
            {
                return _lobbyPlayers.Count;
            }
        }
        
        /// <summary>
        /// 检查玩家是否在大堂中
        /// </summary>
        public bool HasPlayer(int playerId)
        {
            lock (_playersLock)
            {
                return _lobbyPlayers.ContainsKey(playerId);
            }
        }
        
        /// <summary>
        /// 检查是否所有玩家都准备完毕
        /// </summary>
        public bool AreAllPlayersReady()
        {
            lock (_playersLock)
            {
                if (_lobbyPlayers.Count <= 1) return true;
                
                foreach (var player in _lobbyPlayers.Values)
                {
                    if (!player.IsReady) return false;
                }
                return true;
            }
        }
        
        /// <summary>
        /// 设置本地玩家ID
        /// </summary>
        public void SetLocalPlayerId(int playerId)
        {
            _localPlayerId = playerId;
        }
        
        /// <summary>
        /// 设置本地玩家名称
        /// </summary>
        public void SetLocalPlayerName(string name)
        {
            _localPlayerName = name;
            
            lock (_playersLock)
            {
                if (_lobbyPlayers.ContainsKey(_localPlayerId))
                {
                    _lobbyPlayers[_localPlayerId].PlayerName = name;
                }
            }
        }
        
        /// <summary>
        /// 更新玩家延迟
        /// </summary>
        public void UpdatePlayerPing(int playerId, int ping)
        {
            lock (_playersLock)
            {
                if (_lobbyPlayers.ContainsKey(playerId))
                {
                    _lobbyPlayers[playerId].Ping = ping;
                }
            }
        }
        
        /// <summary>
        /// 清空大堂
        /// </summary>
        public void ClearLobby()
        {
            lock (_playersLock)
            {
                _lobbyPlayers.Clear();
            }
            _currentLobbyId = "";
            _isInLobby = false;
            _isLobbyHost = false;
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 获取下一个房主人选
        /// </summary>
        private int GetNextHost()
        {
            lock (_playersLock)
            {
                foreach (var player in _lobbyPlayers.Values)
                {
                    if (!player.IsHost)
                    {
                        return player.PlayerId;
                    }
                }
            }
            return -1;
        }
        
        #endregion
        
        #region 数据持久化
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            data["is_in_lobby"] = _isInLobby;
            data["current_lobby_id"] = _currentLobbyId;
            data["is_lobby_host"] = _isLobbyHost;
            data["local_player_id"] = _localPlayerId;
            data["local_player_name"] = _localPlayerName;
            
            lock (_playersLock)
            {
                var playersData = new Dictionary();
                foreach (var kvp in _lobbyPlayers)
                {
                    playersData[kvp.Key.ToString()] = new Dictionary
                    {
                        ["player_name"] = kvp.Value.PlayerName,
                        ["is_ready"] = kvp.Value.IsReady,
                        ["is_host"] = kvp.Value.IsHost,
                        ["ping"] = kvp.Value.Ping
                    };
                }
                data["players"] = playersData;
            }
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            _isInLobby = Convert.ToBoolean(data.GetValueOrDefault("is_in_lobby", false));
            _currentLobbyId = data.GetValueOrDefault("current_lobby_id", "")?.ToString() ?? "";
            _isLobbyHost = Convert.ToBoolean(data.GetValueOrDefault("is_lobby_host", false));
            _localPlayerId = Convert.ToInt32(data.GetValueOrDefault("local_player_id", -1));
            _localPlayerName = data.GetValueOrDefault("local_player_name", "Player")?.ToString() ?? "Player";
            
            lock (_playersLock)
            {
                _lobbyPlayers.Clear();
                
                if (data.Contains("players"))
                {
                    var playersData = (Dictionary)data["players"];
                    foreach (string key in playersData.Keys)
                    {
                        int playerId = int.Parse(key);
                        var playerData = (Dictionary)playersData[key];
                        
                        _lobbyPlayers[playerId] = new LobbyPlayer
                        {
                            PlayerId = playerId,
                            PlayerName = playerData.GetValueOrDefault("player_name", "")?.ToString() ?? "",
                            IsReady = Convert.ToBoolean(playerData.GetValueOrDefault("is_ready", false)),
                            IsHost = Convert.ToBoolean(playerData.GetValueOrDefault("is_host", false)),
                            Ping = Convert.ToInt32(playerData.GetValueOrDefault("ping", 0)),
                            JoinTime = DateTime.Now
                        };
                    }
                }
            }
        }
        
        #endregion
    }
}
