using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    public partial class MountRacingMain : BaseSystem {
        private MountRacingSystem _mountRacingSystem;
        private MountRacingDatabase _mountRacingDatabase;
        private MountRacingData _mountRacingData;
        private MountRacingUI _mountRacingUI;
        
        public override void _Ready() {
            InitializeSystems();
        }
        
        private void InitializeSystems() {
            // Initialize database
            _mountRacingDatabase = new MountRacingDatabase();
            
            // Initialize data (load from save in production)
            _mountRacingData = new MountRacingData();
            
            // Initialize system
            _mountRacingSystem = new MountRacingSystem();
            _mountRacingSystem.Initialize(_mountRacingData, _mountRacingDatabase);
            
            // Initialize UI
            _mountRacingUI = new MountRacingUI();
            _mountRacingUI.Initialize(_mountRacingSystem, _mountRacingDatabase, _mountRacingData);
            _mountRacingUI.Visible = false;
            AddChild(_mountRacingUI);
        }
        
        public void ToggleMountRacingUI() {
            _mountRacingUI.Visible = !_mountRacingUI.Visible;
        }
        
        public override void _UnhandledInput(InputEvent e) {
            if (e is InputEventKey key && key.Pressed) {
                // R key for Mount Racing
                if (key.Keycode == Key.R) {
                    ToggleMountRacingUI();
                }
            }
        }
        
        /// <summary>
        /// Exports mount racing data for persistence.
        /// </summary>
        public override Dictionary<string, object> ExportSaveData() {
            var data = new Dictionary<string, object>();
            
            if (_mountRacingData != null) {
                data["racing_history"] = SerializeRacingHistory(_mountRacingData.RacingHistory);
                data["unlocked_tracks"] = _mountRacingData.UnlockedTracks;
                data["best_times"] = _mountRacingData.BestTimes;
                data["total_races"] = _mountRacingData.TotalRaces;
                data["total_wins"] = _mountRacingData.TotalWins;
                data["total_gold_earned"] = _mountRacingData.TotalGoldEarned;
                data["total_exp_earned"] = _mountRacingData.TotalExpEarned;
            }
            
            return data;
        }
        
        /// <summary>
        /// Imports mount racing data from persistence.
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data) {
            if (data == null || _mountRacingData == null) return;
            
            if (data.Contains("racing_history")) {
                _mountRacingData.RacingHistory = DeserializeRacingHistory(data["racing_history"] as Dictionary);
            }
            
            if (data.Contains("unlocked_tracks")) {
                _mountRacingData.UnlockedTracks = data["unlocked_tracks"] as List<string> ?? new List<string>();
            }
            
            if (data.Contains("best_times")) {
                _mountRacingData.BestTimes = data["best_times"] as Dictionary ?? new Dictionary<string, int>();
            }
            
            if (data.Contains("total_races")) {
                _mountRacingData.TotalRaces = data["total_races"] as Dictionary ?? new Dictionary<string, int>();
            }
            
            if (data.Contains("total_wins")) {
                _mountRacingData.TotalWins = data["total_wins"] as Dictionary ?? new Dictionary<string, int>();
            }
            
            if (data.Contains("total_gold_earned")) {
                _mountRacingData.TotalGoldEarned = data["total_gold_earned"] as int? ?? 0;
            }
            
            if (data.Contains("total_exp_earned")) {
                _mountRacingData.TotalExpEarned = data["total_exp_earned"] as int? ?? 0;
            }
        }
        
        private Dictionary SerializeRacingHistory(Dictionary<string, MountRacingRecord> history) {
            var result = new Dictionary<string, object>();
            foreach (var kvp in history) {
                var record = new Dictionary {
                    ["track_id"] = kvp.Value.TrackId,
                    ["mount_id"] = kvp.Value.MountId,
                    ["time"] = kvp.Value.Time,
                    ["rank"] = kvp.Value.Rank,
                    ["timestamp"] = kvp.Value.Timestamp.ToString("o"),
                    ["gold_reward"] = kvp.Value.GoldReward,
                    ["exp_reward"] = kvp.Value.ExpReward
                };
                result[kvp.Key] = record;
            }
            return result;
        }
        
        private Dictionary<string, MountRacingRecord> DeserializeRacingHistory(Dictionary data) {
            var result = new Dictionary<string, MountRacingRecord>();
            if (data == null) return result;
            
            foreach (var key in data.Keys) {
                var recordDict = data[key] as Dictionary;
                if (recordDict == null) continue;
                
                var record = new MountRacingRecord {
                    TrackId = recordDict["track_id"] as string ?? "",
                    MountId = recordDict["mount_id"] as string ?? "",
                    Time = recordDict["time"] as int? ?? 0,
                    Rank = recordDict["rank"] as int? ?? 0,
                    Timestamp = DateTime.TryParse(recordDict["timestamp"] as string, out var ts) ? ts : DateTime.Now,
                    GoldReward = recordDict["gold_reward"] as int? ?? 0,
                    ExpReward = recordDict["exp_reward"] as int? ?? 0
                };
                result[key as string ?? ""] = record;
            }
            return result;
        }
    }
}
