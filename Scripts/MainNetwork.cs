using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Systems;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// MainNetwork - Handles network initialization, connection state, and room management
    /// This is a partial class that extends Main with networking capabilities
    /// </summary>
    public partial class MainNetwork : Node
    {
        #region Connection State
        
        /// <summary>
        /// Network connection states
        /// </summary>
        public enum ConnectionState
        {
            Disconnected,
            Connecting,
            Connected,
            Reconnecting,
            Error
        }
        
        private ConnectionState _connectionState = ConnectionState.Disconnected;
        private string _lastError = "";
        private float _connectionTimer = 0f;
        private int _reconnectAttempts = 0;
        private const int MaxReconnectAttempts = 5;
        
        #endregion
        
        #region Room Management
        
        /// <summary>
        /// Room info for external access
        /// </summary>
        public class RoomInfo
        {
            public string RoomId;
            public string RoomName;
            public string HostName;
            public int CurrentPlayers;
            public int MaxPlayers;
            public string GameMode;
            public int Difficulty;
            public bool IsPrivate;
        }
        
        private List<RoomInfo> _cachedRoomList = new List<RoomInfo>();
        private float _roomListRefreshTimer = 0f;
        private const float RoomListRefreshInterval = 10f;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize network systems - called from Main._Ready
        /// </summary>
        public void InitializeNetwork()
        {
            GD.Print("MainNetwork: Initializing network systems...");
            
            // Initialize network quality monitoring
            InitializeNetworkQuality();
            
            // Set initial connection state
            _connectionState = ConnectionState.Disconnected;
            
            // Initialize multiplayer systems if available
            InitializeMultiplayerSystems();
            
            GD.Print("MainNetwork: Network initialization complete");
        }
        
        /// <summary>
        /// Initialize network quality monitoring
        /// </summary>
        private void InitializeNetworkQuality()
        {
            // Network quality is handled by NetworkQualityUI
            // This method can be extended for actual network quality monitoring
            GD.Print("MainNetwork: Network quality monitoring initialized");
        }
        
        /// <summary>
        /// Initialize multiplayer systems
        /// </summary>
        private void InitializeMultiplayerSystems()
        {
            // Multiplayer systems are already initialized in Main._Ready
            // This method provides additional network-level setup if needed
        }
        
        #endregion
        
        #region Connection Management
        
        /// <summary>
        /// Get current connection state
        /// </summary>
        public ConnectionState GetConnectionState()
        {
            return _connectionState;
        }
        
        /// <summary>
        /// Get last error message
        /// </summary>
        public string GetLastError()
        {
            return _lastError;
        }
        
        /// <summary>
        /// Connect to server (placeholder for actual network connection)
        /// </summary>
        public void Connect(string serverAddress, int port)
        {
            GD.Print($"MainNetwork: Connecting to {serverAddress}:{port}...");
            
            _connectionState = ConnectionState.Connecting;
            _lastError = "";
            _reconnectAttempts = 0;
            
            // In a real implementation, this would establish actual network connection
            // For now, we simulate a successful connection
            _connectionState = ConnectionState.Connected;
            
            GD.Print("MainNetwork: Connected successfully");
        }
        
        /// <summary>
        /// Disconnect from server
        /// </summary>
        public void Disconnect()
        {
            GD.Print("MainNetwork: Disconnecting...");
            
            _connectionState = ConnectionState.Disconnected;
            
            // Leave any active room
            LeaveRoom();
            
            GD.Print("MainNetwork: Disconnected");
        }
        
        /// <summary>
        /// Attempt to reconnect
        /// </summary>
        public void Reconnect()
        {
            if (_reconnectAttempts >= MaxReconnectAttempts)
            {
                _lastError = "Max reconnection attempts reached";
                _connectionState = ConnectionState.Error;
                GD.PrintErr($"MainNetwork: {_lastError}");
                return;
            }
            
            _reconnectAttempts++;
            _connectionState = ConnectionState.Reconnecting;
            
            GD.Print($"MainNetwork: Reconnecting... Attempt {_reconnectAttempts}/{MaxReconnectAttempts}");
            
            // Simulate reconnection
            _connectionState = ConnectionState.Connected;
        }
        
        /// <summary>
        /// Process network updates - called from Main._Process
        /// </summary>
        public void ProcessNetwork(double delta)
        {
            float dt = (float)delta;
            
            // Update connection timer
            if (_connectionState == ConnectionState.Connecting || 
                _connectionState == ConnectionState.Reconnecting)
            {
                _connectionTimer += dt;
                
                // Timeout after 30 seconds
                if (_connectionTimer > 30f)
                {
                    _lastError = "Connection timeout";
                    _connectionState = ConnectionState.Error;
                    GD.PrintErr($"MainNetwork: {_lastError}");
                }
            }
            else
            {
                _connectionTimer = 0f;
            }
            
            // Refresh room list periodically
            _roomListRefreshTimer += dt;
            if (_roomListRefreshTimer >= RoomListRefreshInterval)
            {
                _roomListRefreshTimer = 0f;
                RefreshRoomListInternal();
            }
        }
        
        #endregion
        
        #region Room Management
        
        /// <summary>
        /// Create a new room
        /// </summary>
        public string CreateRoom(string roomName, string hostName, string gameMode = "Classic", 
            int difficulty = 1, bool isPrivate = false, string password = "")
        {
            GD.Print($"MainNetwork: Creating room - {roomName}");
            
            var lobbySystem = MultiplayerLobbySystem.Instance;
            if (lobbySystem == null)
            {
                _lastError = "Lobby system not available";
                GD.PrintErr($"MainNetwork: {_lastError}");
                return "";
            }
            
            string roomId = lobbySystem.CreateRoom(roomName, hostName, gameMode, difficulty, isPrivate, password);
            
            if (!string.IsNullOrEmpty(roomId))
            {
                GD.Print($"MainNetwork: Room created successfully - {roomId}");
            }
            else
            {
                _lastError = "Failed to create room";
                GD.PrintErr($"MainNetwork: {_lastError}");
            }
            
            return roomId;
        }
        
        /// <summary>
        /// Join an existing room
        /// </summary>
        public bool JoinRoom(string roomId, string playerName, string password = "")
        {
            GD.Print($"MainNetwork: Joining room - {roomId}");
            
            var lobbySystem = MultiplayerLobbySystem.Instance;
            if (lobbySystem == null)
            {
                _lastError = "Lobby system not available";
                GD.PrintErr($"MainNetwork: {_lastError}");
                return false;
            }
            
            bool success = lobbySystem.JoinRoom(roomId, playerName, password);
            
            if (success)
            {
                GD.Print($"MainNetwork: Joined room successfully - {roomId}");
            }
            else
            {
                _lastError = "Failed to join room";
                GD.PrintErr($"MainNetwork: {_lastError}");
            }
            
            return success;
        }
        
        /// <summary>
        /// Leave current room
        /// </summary>
        public void LeaveRoom()
        {
            GD.Print("MainNetwork: Leaving room...");
            
            var lobbySystem = MultiplayerLobbySystem.Instance;
            if (lobbySystem == null)
            {
                return;
            }
            
            lobbySystem.LeaveRoom();
            
            GD.Print("MainNetwork: Left room");
        }
        
        /// <summary>
        /// Get list of available rooms
        /// </summary>
        public List<RoomInfo> GetRoomList(string gameMode = "")
        {
            // Return cached room list
            return new List<RoomInfo>(_cachedRoomList);
        }
        
        /// <summary>
        /// Refresh room list from server
        /// </summary>
        public void RefreshRoomList()
        {
            RefreshRoomListInternal();
        }
        
        /// <summary>
        /// Internal room list refresh
        /// </summary>
        private void RefreshRoomListInternal()
        {
            _cachedRoomList.Clear();
            
            var lobbySystem = MultiplayerLobbySystem.Instance;
            if (lobbySystem == null)
            {
                return;
            }
            
            var rooms = lobbySystem.GetAvailableRooms();
            
            foreach (var room in rooms)
            {
                var info = new RoomInfo
                {
                    RoomId = room.RoomId,
                    RoomName = room.RoomName,
                    HostName = room.HostName,
                    CurrentPlayers = room.CurrentPlayers,
                    MaxPlayers = room.MaxPlayers,
                    GameMode = room.GameMode,
                    Difficulty = room.Difficulty,
                    IsPrivate = room.IsPrivate
                };
                
                _cachedRoomList.Add(info);
            }
            
            GD.Print($"MainNetwork: Room list refreshed - {_cachedRoomList.Count} rooms available");
        }
        
        /// <summary>
        /// Get current room info
        /// </summary>
        public RoomInfo GetCurrentRoomInfo()
        {
            var lobbySystem = MultiplayerLobbySystem.Instance;
            if (lobbySystem == null)
            {
                return null;
            }
            
            var room = lobbySystem.GetCurrentRoom();
            if (room == null)
            {
                return null;
            }
            
            return new RoomInfo
            {
                RoomId = room.RoomId,
                RoomName = room.RoomName,
                HostName = room.HostName,
                CurrentPlayers = room.CurrentPlayers,
                MaxPlayers = room.MaxPlayers,
                GameMode = room.GameMode,
                Difficulty = room.Difficulty,
                IsPrivate = room.IsPrivate
            };
        }
        
        /// <summary>
        /// Check if player is in a room
        /// </summary>
        public bool IsInRoom()
        {
            var roomInfo = GetCurrentRoomInfo();
            return roomInfo != null;
        }
        
        #endregion
        
        #region Network Quality
        
        /// <summary>
        /// Get estimated network latency (ms)
        /// </summary>
        public int GetLatency()
        {
            // In a real implementation, this would measure actual latency
            // For now, return a placeholder value
            return 50;
        }
        
        /// <summary>
        /// Get network quality rating (0-100)
        /// </summary>
        public int GetNetworkQuality()
        {
            int latency = GetLatency();
            
            if (latency < 50) return 100;
            if (latency < 100) return 80;
            if (latency < 200) return 60;
            if (latency < 500) return 40;
            return 20;
        }
        
        #endregion
    }
}
