using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// NetworkSyncSystem - 负责网络状态同步
    /// </summary>
    public partial class NetworkSyncSystem : BaseSystem
    {
        public static NetworkSyncSystem Instance { get; private set; }
        
        // 玩家信息
        public class NetworkPlayer
        {
            public int PlayerId { get; set; }
            public string PlayerName { get; set; }
            public Vector2 Position { get; set; }
            public Vector2 Velocity { get; set; }
            public int Health { get; set; }
            public int MaxHealth { get; set; }
            public string CurrentState { get; set; }
            public DateTime LastUpdate { get; set; }
        }
        
        // 玩家列表
        private Dictionary<int, NetworkPlayer> _networkPlayers = new Dictionary<int, NetworkPlayer>();
        private readonly object _playersLock = new object();
        
        // 同步设置
        private float _syncInterval = 0.05f;  // 20Hz 同步频率
        private float _syncTimer = 0f;
        
        // 玩家准备状态
        private Dictionary<int, bool> _playerReadyStates = new Dictionary<int, bool>();
        private bool _isReady = false;
        
        // 信号
        public delegate void PlayerJoinedEventHandler(int playerId, string playerName);
        public delegate void PlayerLeftEventHandler(int playerId);
        public delegate void PlayerStateUpdateEventHandler(int playerId, NetworkPlayer player);
        
        public bool IsReady => _isReady;
        public int PlayerCount => _networkPlayers.Count;
        
        public override void _Ready()
        {
            Instance = this;
        }
        
        public override void _Process(double delta)
        {
            _syncTimer += (float)delta;
            if (_syncTimer >= _syncInterval)
            {
                _syncTimer = 0f;
                // Sync logic would go here
            }
        }
        
        public void AddPlayer(int playerId, string playerName)
        {
            lock (_playersLock)
            {
                if (!_networkPlayers.ContainsKey(playerId))
                {
                    _networkPlayers[playerId] = new NetworkPlayer
                    {
                        PlayerId = playerId,
                        PlayerName = playerName,
                        Position = Vector2.Zero,
                        Velocity = Vector2.Zero,
                        Health = 100,
                        MaxHealth = 100,
                        CurrentState = "idle",
                        LastUpdate = DateTime.Now
                    };
                    
                    _playerReadyStates[playerId] = false;
                    
                    GD.Print($"[NetworkSync] Player joined: {playerName} (ID: {playerId})");
                    EmitSignal(SignalName.PlayerJoined, playerId, playerName);
                }
            }
        }
        
        public void RemovePlayer(int playerId)
        {
            lock (_playersLock)
            {
                if (_networkPlayers.ContainsKey(playerId))
                {
                    var playerName = _networkPlayers[playerId].PlayerName;
                    _networkPlayers.Remove(playerId);
                    _playerReadyStates.Remove(playerId);
                    
                    GD.Print($"[NetworkSync] Player left: {playerName} (ID: {playerId})");
                    EmitSignal(SignalName.PlayerLeft, playerId);
                }
            }
        }
        
        public void UpdatePlayerState(int playerId, Vector2 position, Vector2 velocity, string state)
        {
            lock (_playersLock)
            {
                if (_networkPlayers.ContainsKey(playerId))
                {
                    var player = _networkPlayers[playerId];
                    player.Position = position;
                    player.Velocity = velocity;
                    player.CurrentState = state;
                    player.LastUpdate = DateTime.Now;
                    
                    EmitSignal(SignalName.PlayerStateUpdateEvent, playerId, player);
                }
            }
        }
        
        public void UpdatePlayerHealth(int playerId, int health, int maxHealth)
        {
            lock (_playersLock)
            {
                if (_networkPlayers.ContainsKey(playerId))
                {
                    _networkPlayers[playerId].Health = health;
                    _networkPlayers[playerId].MaxHealth = maxHealth;
                }
            }
        }
        
        public NetworkPlayer GetPlayer(int playerId)
        {
            lock (_playersLock)
            {
                return _networkPlayers.ContainsKey(playerId) ? _networkPlayers[playerId] : null;
            }
        }
        
        public Dictionary<int, NetworkPlayer> GetAllPlayers()
        {
            lock (_playersLock)
            {
                return new Dictionary<int, NetworkPlayer>(_networkPlayers);
            }
        }
        
        public List<NetworkPlayer> GetPlayerList()
        {
            lock (_playersLock)
            {
                return new List<NetworkPlayer>(_networkPlayers.Values);
            }
        }
        
        public bool HasPlayer(int playerId)
        {
            lock (_playersLock)
            {
                return _networkPlayers.ContainsKey(playerId);
            }
        }
        
        public void SetPlayerReady(int playerId, bool ready)
        {
            _playerReadyStates[playerId] = ready;
            
            if (playerId == GetLocalPlayerId())
            {
                _isReady = ready;
            }
        }
        
        public bool IsPlayerReady(int playerId)
        {
            return _playerReadyStates.GetValueOrDefault(playerId, false);
        }
        
        public bool AreAllPlayersReady()
        {
            foreach (var state in _playerReadyStates.Values)
            {
                if (!state) return false;
            }
            return _playerReadyStates.Count > 0;
        }
        
        public Dictionary<int, bool> GetAllReadyStates()
        {
            return new Dictionary<int, bool>(_playerReadyStates);
        }
        
        public void ClearAllPlayers()
        {
            lock (_playersLock)
            {
                _networkPlayers.Clear();
                _playerReadyStates.Clear();
                _isReady = false;
            }
        }
        
        public void SetSyncInterval(float interval)
        {
            _syncInterval = Mathf.Max(0.01f, interval);
        }
        
        public float GetSyncInterval()
        {
            return _syncInterval;
        }
        
        private int GetLocalPlayerId()
        {
            // This would be retrieved from SessionManager
            return SessionManager.Instance?.LocalPlayerId ?? -1;
        }

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // 玩家准备状态
            var readyStates = new List<Dictionary<string, object>>();
            foreach (var kvp in _playerReadyStates)
            {
                readyStates.Add(new Dictionary<string, object>
                {
                    { "player_id", kvp.Key },
                    { "ready", kvp.Value }
                });
            }
            data["ready_states"] = readyStates;
            
            data["is_ready"] = _isReady;
            data["sync_interval"] = _syncInterval;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            // 玩家准备状态
            _playerReadyStates.Clear();
            if (data.ContainsKey("ready_states") && data["ready_states"] is List<object> statesList)
            {
                foreach (var item in statesList)
                {
                    if (item is Dictionary<string, object> stateDict)
                    {
                        var playerId = (int)stateDict["player_id"];
                        var ready = (bool)stateDict["ready"];
                        _playerReadyStates[playerId] = ready;
                    }
                }
            }
            
            _isReady = (bool)data.GetValueOrDefault("is_ready", false);
            _syncInterval = (float)data.GetValueOrDefault("sync_interval", 0.05f);
        }
    }
}
