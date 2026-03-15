using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Connection Recovery System - Handles network disconnection and reconnection
    /// Based on Multiplayer Networking Patterns learning
    /// </summary>
    public class ConnectionRecoveryData {
        public bool IsConnected { get; set; } = true;
        public bool IsReconnecting { get; set; } = false;
        public int ConnectionAttempts { get; set; } = 0;
        public int MaxConnectionAttempts { get; set; } = 5;
        public float HeartbeatInterval { get; set; } = 5.0f;
        public float LastHeartbeatTime { get; set; } = 0f;
        public float ReconnectDelay { get; set; } = 2.0f;
        public float LastReconnectAttempt { get; set; } = 0f;
        public int OfflineModeEnabled { get; set; } = 0;
        public Dictionary<string, byte[]> SavedGameState { get; set; } = new Dictionary<string, byte[]>();
        public float LastPing { get; set; } = 0f;
        public float AveragePing { get; set; } = 0f;
        public List<float> PingHistory { get; set; } = new List<float>();
        public ConnectionState State { get; set; } = ConnectionState.Connected;
        public DateTime DisconnectTime { get; set; } = DateTime.MinValue;
        public DateTime LastConnectedTime { get; set; } = DateTime.Now;
    }

    public enum ConnectionState {
        Connected,
        Connecting,
        Disconnected,
        Reconnecting,
        OfflineMode
    }

    public class ConnectionRecoverySystem : BaseSystem {
        private static ConnectionRecoveryData _data;
        private static ConnectionRecoverySystem _instance;
        
        public static ConnectionRecoverySystem Instance {
            get {
                if (_instance == null) {
                    _instance = new ConnectionRecoverySystem();
                    _data = new ConnectionRecoveryData();
                }
                return _instance;
            }
        }

        public ConnectionRecoveryData Data => _data;

        /// <summary>
        /// Initialize the connection recovery system
        /// </summary>
        public void Initialize() {
            _data = new ConnectionRecoveryData();
            _data.State = ConnectionState.Connected;
            _data.LastConnectedTime = DateTime.Now;
            GD.Print("[ConnectionRecovery] System initialized");
        }

        /// <summary>
        /// Called when connection is lost
        /// </summary>
        public void OnConnectionLost() {
            _data.IsConnected = false;
            _data.State = ConnectionState.Disconnected;
            _data.DisconnectTime = DateTime.Now;
            _data.ConnectionAttempts = 0;
            GD.Print("[ConnectionRecovery] Connection lost at " + _data.DisconnectTime);
        }

        /// <summary>
        /// Attempt to reconnect to the server
        /// </summary>
        public void AttemptReconnect() {
            if (_data.ConnectionAttempts >= _data.MaxConnectionAttempts) {
                GD.Print("[ConnectionRecovery] Max connection attempts reached");
                EnableOfflineMode();
                return;
            }

            _data.IsReconnecting = true;
            _data.State = ConnectionState.Reconnecting;
            _data.ConnectionAttempts++;
            _data.LastReconnectAttempt = Time.GetTicksMsec() / 1000f;
            
            GD.Print($"[ConnectionRecovery] Reconnect attempt {_data.ConnectionAttempts}/{_data.MaxConnectionAttempts}");
            
            // Simulate reconnection attempt
            // In a real implementation, this would attempt to reconnect to the server
            bool success = SimulateReconnect();
            
            if (success) {
                OnReconnectSuccess();
            } else {
                _data.IsReconnecting = false;
                _data.State = ConnectionState.Disconnected;
            }
        }

        /// <summary>
        /// Simulate reconnection (placeholder for real implementation)
        /// </summary>
        private bool SimulateReconnect() {
            // In a real game, this would attempt actual network reconnection
            // For now, simulate a successful reconnect after delay
            return _data.ConnectionAttempts >= 1;
        }

        /// <summary>
        /// Called when reconnection is successful
        /// </summary>
        public void OnReconnectSuccess() {
            _data.IsConnected = true;
            _data.IsReconnecting = false;
            _data.State = ConnectionState.Connected;
            _data.LastConnectedTime = DateTime.Now;
            _data.ConnectionAttempts = 0;
            
            GD.Print("[ConnectionRecovery] Reconnection successful");
            
            // Sync game state after reconnection
            SyncGameState();
        }

        /// <summary>
        /// Enable offline mode when all reconnect attempts fail
        /// </summary>
        public void EnableOfflineMode() {
            _data.OfflineModeEnabled = 1;
            _data.State = ConnectionState.OfflineMode;
            _data.IsConnected = false;
            _data.IsReconnecting = false;
            
            GD.Print("[ConnectionRecovery] Offline mode enabled");
        }

        /// <summary>
        /// Disable offline mode and attempt to reconnect
        /// </summary>
        public void DisableOfflineMode() {
            _data.OfflineModeEnabled = 0;
            _data.ConnectionAttempts = 0;
            AttemptReconnect();
        }

        /// <summary>
        /// Send heartbeat to check connection status
        /// </summary>
        public void SendHeartbeat() {
            float currentTime = Time.GetTicksMsec() / 1000f;
            
            if (currentTime - _data.LastHeartbeatTime >= _data.HeartbeatInterval) {
                _data.LastHeartbeatTime = currentTime;
                
                // In a real implementation, send actual ping to server
                float ping = MeasurePing();
                UpdatePingStats(ping);
                
                if (ping < 0) {
                    OnConnectionLost();
                }
            }
        }

        /// <summary>
        /// Measure current ping (placeholder)
        /// </summary>
        private float MeasurePing() {
            // In a real implementation, measure actual network latency
            // Return -1 to indicate connection lost
            return 50f + (new Random().Next(-20, 20));
        }

        /// <summary>
        /// Update ping statistics
        /// </summary>
        private void UpdatePingStats(float ping) {
            _data.LastPing = ping;
            _data.PingHistory.Add(ping);
            
            // Keep only last 10 ping values
            if (_data.PingHistory.Count > 10) {
                _data.PingHistory.RemoveAt(0);
            }
            
            // Calculate average
            float sum = 0;
            foreach (float p in _data.PingHistory) {
                sum += p;
            }
            _data.AveragePing = sum / _data.PingHistory.Count;
        }

        /// <summary>
        /// Get connection quality based on ping
        /// </summary>
        public string GetConnectionQuality() {
            if (_data.AveragePing < 50) return "Excellent";
            if (_data.AveragePing < 100) return "Good";
            if (_data.AveragePing < 200) return "Fair";
            return "Poor";
        }

        /// <summary>
        /// Save current game state for recovery
        /// </summary>
        public void SaveGameState(string key, byte[] state) {
            _data.SavedGameState[key] = state;
        }

        /// <summary>
        /// Get saved game state
        /// </summary>
        public byte[] GetSavedGameState(string key) {
            if (_data.SavedGameState.ContainsKey(key)) {
                return _data.SavedGameState[key];
            }
            return null;
        }

        /// <summary>
        /// Sync game state after reconnection
        /// </summary>
        public void SyncGameState() {
            GD.Print("[ConnectionRecovery] Syncing game state after reconnection");
            
            // In a real implementation:
            // 1. Send last sync timestamp to server
            // 2. Receive any updates that occurred while disconnected
            // 3. Apply delta changes to local state
            // 4. Resolve any conflicts
        }

        /// <summary>
        /// Get reconnection progress (0-100)
        /// </summary>
        public int GetReconnectionProgress() {
            if (_data.MaxConnectionAttempts == 0) return 100;
            return (_data.ConnectionAttempts * 100) / _data.MaxConnectionAttempts;
        }

        /// <summary>
        /// Get disconnection duration in seconds
        /// </summary>
        public float GetDisconnectionDuration() {
            if (_data.DisconnectTime == DateTime.MinValue) return 0;
            return (float)(DateTime.Now - _data.DisconnectTime).TotalSeconds;
        }

        /// <summary>
        /// Process reconnection logic
        /// </summary>
        public void Process(float delta) {
            SendHeartbeat();
            
            if (_data.State == ConnectionState.Disconnected || _data.State == ConnectionState.Reconnecting) {
                float currentTime = Time.GetTicksMsec() / 1000f;
                
                if (currentTime - _data.LastReconnectAttempt >= _data.ReconnectDelay) {
                    if (!_data.IsReconnecting && _data.ConnectionAttempts < _data.MaxConnectionAttempts) {
                        AttemptReconnect();
                    }
                }
            }
        }

        /// <summary>
        /// Get current connection state as string
        /// </summary>
        public string GetStateString() {
            return _data.State.ToString();
        }

        /// <summary>
        /// Check if currently in offline mode
        /// </summary>
        public bool IsOfflineMode() {
            return _data.State == ConnectionState.OfflineMode;
        }

        /// <summary>
        /// Check if connection is stable
        /// </summary>
        public bool IsConnectionStable() {
            return _data.State == ConnectionState.Connected && _data.AveragePing < 150;
        }

        /// <summary>
        /// Reset the system
        /// </summary>
        public void Reset() {
            _data = new ConnectionRecoveryData();
            GD.Print("[ConnectionRecovery] System reset");
        }

        /// <summary>
        /// Export save data for persistence
        /// </summary>
        public Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["is_connected"] = _data.IsConnected;
            data["connection_attempts"] = _data.ConnectionAttempts;
            data["offline_mode_enabled"] = _data.OfflineModeEnabled;
            data["last_ping"] = _data.LastPing;
            data["average_ping"] = _data.AveragePing;
            data["state"] = (int)_data.State;
            return data;
        }

        /// <summary>
        /// Import save data from persistence
        /// </summary>
        public void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.ContainsKey("is_connected"))
                _data.IsConnected = (bool)data["is_connected"];
            if (data.ContainsKey("connection_attempts"))
                _data.ConnectionAttempts = (int)data["connection_attempts"];
            if (data.ContainsKey("offline_mode_enabled"))
                _data.OfflineModeEnabled = (int)data["offline_mode_enabled"];
            if (data.ContainsKey("last_ping"))
                _data.LastPing = Convert.ToSingle(data["last_ping"]);
            if (data.ContainsKey("average_ping"))
                _data.AveragePing = Convert.ToSingle(data["average_ping"]);
            if (data.ContainsKey("state"))
                _data.State = (ConnectionState)(int)data["state"];
        }
    }
}
