using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.SealedDungeon {
    public class SealedDungeonSystem : BaseSystem {
        private static SealedDungeonSystem _instance;
        public static SealedDungeonSystem Instance {
            get {
                if (_instance == null) {
                    _instance = new SealedDungeonSystem();
                }
                return _instance;
            }
        }

        private PlayerSealedDungeonData _playerData;
        private SealedDungeonData _currentDungeon;
        private int _elapsedTime;
        private bool _isInDungeon;
        private Timer _dungeonTimer;

        public signal EventHandler<SealedDungeonData> DungeonStarted;
        public signal EventHandler<SealedDungeonData> DungeonCompleted;
        public signal EventHandler<SealedDungeonData> DungeonFailed;
        public signal EventHandler<(int floor, bool success)> FloorCompleted;
        public signal EventHandler<DungeonZone> ZoneUnlocked;
        public signal EventHandler<DungeonReward> RewardClaimed;

        public PlayerSealedDungeonData PlayerData => _playerData;
        public SealedDungeonData CurrentDungeon => _currentDungeon;
        public bool IsInDungeon => _isInDungeon;

        public SealedDungeonSystem() {
            _instance = this;
            _playerData = new PlayerSealedDungeonData();
            _isInDungeon = false;
            _elapsedTime = 0;
        }

        public void Initialize() {
            _dungeonTimer = new Timer();
            AddChild(_dungeonTimer);
            _dungeonTimer.Connect("timeout", this, nameof(OnTimerTick));
            
            GD.Print("[SealedDungeonSystem] Initialized - Sealed Dungeon System ready");
        }

        public bool StartDungeon(int dungeonId) {
            var config = SealedDungeonDatabase.Instance.GetDungeonConfig(dungeonId);
            if (config == null) {
                GD.PrintErr($"[SealedDungeonSystem] Dungeon {dungeonId} not found");
                return false;
            }

            var dungeon = _playerData.Dungeons.Find(d => d.DungeonId == dungeonId);
            if (dungeon == null) {
                dungeon = new SealedDungeonData {
                    DungeonId = dungeonId,
                    DungeonName = config.Name,
                    CurrentZone = config.Zone,
                    State = SealedDungeonState.InProgress,
                    CurrentFloor = 1,
                    MaxFloors = config.MaxFloors
                };
                _playerData.Dungeons.Add(dungeon);
            }

            if (dungeon.State == SealedDungeonState.Completed) {
                GD.Print($"[SealedDungeonSystem] Dungeon {dungeonId} already completed");
                return false;
            }

            _currentDungeon = dungeon;
            _currentDungeon.State = SealedDungeonState.InProgress;
            _currentDungeon.Attempts++;
            _currentDungeon.LastAttemptTime = DateTime.Now;
            _currentDungeon.CurrentScore = 0;
            _currentDungeon.ClearedFloors = 0;
            
            _elapsedTime = 0;
            _isInDungeon = true;
            _dungeonTimer.Start(1.0f);

            DungeonStarted?.Invoke(this, _currentDungeon);
            
            GD.Print($"[SealedDungeonSystem] Started dungeon {config.Name} at floor {_currentDungeon.CurrentFloor}");
            return true;
        }

        public bool CompleteFloor(bool success) {
            if (!_isInDungeon || _currentDungeon == null) {
                GD.PrintErr("[SealedDungeonSystem] Not in dungeon");
                return false;
            }

            var reward = SealedDungeonDatabase.Instance.GetReward(_currentDungeon.CurrentFloor);
            
            if (success) {
                _currentDungeon.ClearedFloors++;
                _currentDungeon.CompletedFloors.Add(_currentDungeon.CurrentFloor);
                
                int baseScore = _currentDungeon.CurrentFloor * 100;
                var zoneConfig = SealedDungeonDatabase.Instance.GetZoneConfig(_currentDungeon.CurrentZone);
                if (zoneConfig != null) {
                    baseScore = (int)(baseScore * zoneConfig.ScoreMultiplier);
                }
                _currentDungeon.CurrentScore += baseScore;

                FloorCompleted?.Invoke(this, (_currentDungeon.CurrentFloor, true));
                
                GD.Print($"[SealedDungeonSystem] Floor {_currentDungeon.CurrentFloor} completed. Score: {_currentDungeon.CurrentScore}");

                if (_currentDungeon.CurrentFloor >= _currentDungeon.MaxFloors) {
                    CompleteDungeon(true);
                } else {
                    _currentDungeon.CurrentFloor++;
                }
            } else {
                FloorCompleted?.Invoke(this, (_currentDungeon.CurrentFloor, false));
                CompleteDungeon(false);
            }

            return success;
        }

        public void CompleteDungeon(bool success) {
            if (_currentDungeon == null) return;

            _isInDungeon = false;
            _dungeonTimer.Stop();

            if (success) {
                _currentDungeon.State = SealedDungeonState.Completed;
                _currentDungeon.Completions++;
                
                if (_currentDungeon.CurrentScore > _currentDungeon.BestScore) {
                    _currentDungeon.BestScore = _currentDungeon.CurrentScore;
                }
                
                if (_elapsedTime < _currentDungeon.BestTime) {
                    _currentDungeon.BestTime = _elapsedTime;
                }

                _playerData.Statistics.TotalCompletions++;
                _playerData.Statistics.TotalFloorsCleared += _currentDungeon.ClearedFloors;
                _playerData.Statistics.CurrentStreak++;
                
                if (_playerData.Statistics.CurrentStreak > _playerData.Statistics.LongestStreak) {
                    _playerData.Statistics.LongestStreak = _playerData.Statistics.CurrentStreak;
                }

                if (_currentDungeon.CurrentScore > _playerData.Statistics.BestScore) {
                    _playerData.Statistics.BestScore = _currentDungeon.CurrentScore;
                }

                var zoneConfig = SealedDungeonDatabase.Instance.GetZoneConfig(_currentDungeon.CurrentZone);
                if (zoneConfig != null) {
                    _playerData.Statistics.ZoneClearCount[_currentDungeon.CurrentZone]++;
                    
                    int nextZoneIndex = (int)_currentDungeon.CurrentZone + 1;
                    if (nextZoneIndex < Enum.GetValues(typeof(DungeonZone)).Length) {
                        var nextZone = (DungeonZone)nextZoneIndex;
                        if (!_playerData.UnlockedZones.Contains(nextZone)) {
                            _playerData.UnlockedZones.Add(nextZone);
                            _playerData.HighestZoneUnlocked = nextZoneIndex;
                            ZoneUnlocked?.Invoke(this, nextZone);
                        }
                    }
                }

                DungeonCompleted?.Invoke(this, _currentDungeon);
                GD.Print($"[SealedDungeonSystem] Dungeon {_currentDungeon.DungeonId} completed! Score: {_currentDungeon.CurrentScore}");
            } else {
                _currentDungeon.State = SealedDungeonState.Failed;
                _playerData.Statistics.CurrentStreak = 0;
                DungeonFailed?.Invoke(this, _currentDungeon);
                GD.Print($"[SealedDungeonSystem] Dungeon {_currentDungeon.DungeonId} failed at floor {_currentDungeon.CurrentFloor}");
            }
        }

        public DungeonReward ClaimFloorReward() {
            if (!_isInDungeon || _currentDungeon == null) return null;

            var reward = SealedDungeonDatabase.Instance.GetReward(_currentDungeon.CurrentFloor);
            if (reward == null) return null;

            var zoneConfig = SealedDungeonDatabase.Instance.GetZoneConfig(_currentDungeon.CurrentZone);
            if (zoneConfig != null) {
                reward.GoldReward = (int)(reward.GoldReward * zoneConfig.ScoreMultiplier);
                reward.ExperienceReward = (int)(reward.ExperienceReward * zoneConfig.ScoreMultiplier);
            }

            _playerData.Statistics.TotalGoldEarned += reward.GoldReward;
            _playerData.Statistics.TotalExperienceEarned += reward.ExperienceReward;

            RewardClaimed?.Invoke(this, reward);
            
            GD.Print($"[SealedDungeonSystem] Claimed reward: {reward.GoldReward} gold, {reward.ExperienceReward} XP");
            return reward;
        }

        public bool UnlockZone(DungeonZone zone) {
            if (_playerData.UnlockedZones.Contains(zone)) {
                GD.Print($"[SealedDungeonSystem] Zone {zone} already unlocked");
                return false;
            }

            var config = SealedDungeonDatabase.Instance.GetZoneConfig(zone);
            if (config == null) return false;

            // Check if player has enough resources (simplified - would integrate with player economy)
            // For now, just unlock
            _playerData.UnlockedZones.Add(zone);
            
            if ((int)zone > _playerData.HighestZoneUnlocked) {
                _playerData.HighestZoneUnlocked = (int)zone;
            }

            ZoneUnlocked?.Invoke(this, zone);
            GD.Print($"[SealedDungeonSystem] Unlocked zone: {zone}");
            return true;
        }

        public ZoneProgress GetZoneProgress(DungeonZone zone) {
            var progress = new ZoneProgress {
                Zone = zone,
                IsUnlocked = _playerData.UnlockedZones.Contains(zone),
                IsCompleted = false,
                BestTime = int.MaxValue,
                BestScore = 0,
                ClearCount = 0
            };

            var dungeon = _playerData.Dungeons.Find(d => d.CurrentZone == zone && d.State == SealedDungeonState.Completed);
            if (dungeon != null) {
                progress.IsCompleted = true;
                progress.BestTime = dungeon.BestTime;
                progress.BestScore = dungeon.BestScore;
                progress.ClearCount = dungeon.Completions;
            }

            return progress;
        }

        public int GetTotalStars() {
            int stars = 0;
            foreach (var dungeon in _playerData.Dungeons) {
                if (dungeon.State == SealedDungeonState.Completed) {
                    stars += dungeon.Completions;
                }
            }
            _playerData.TotalStars = stars;
            return stars;
        }

        private void OnTimerTick() {
            _elapsedTime++;
            
            var floorConfig = SealedDungeonDatabase.Instance.GetFloorConfig(_currentDungeon.CurrentFloor);
            if (floorConfig != null && _elapsedTime >= floorConfig.TimeLimit) {
                GD.Print($"[SealedDungeonSystem] Time limit reached on floor {_currentDungeon.CurrentFloor}");
                CompleteDungeon(false);
            }
        }

        public override Dictionary<string, object> ExportSaveData() {
            var data = new Dictionary<string, object>();

            var dungeons = new List<Dictionary<string, object>>();
            foreach (var dungeon in _playerData.Dungeons) {
                dungeons.Add(new Dictionary<string, object> {
                    { "DungeonId", dungeon.DungeonId },
                    { "DungeonName", dungeon.DungeonName },
                    { "CurrentZone", (int)dungeon.CurrentZone },
                    { "State", (int)dungeon.State },
                    { "CurrentFloor", dungeon.CurrentFloor },
                    { "MaxFloors", dungeon.MaxFloors },
                    { "ClearedFloors", dungeon.ClearedFloors },
                    { "BestTime", dungeon.BestTime },
                    { "CurrentScore", dungeon.CurrentScore },
                    { "BestScore", dungeon.BestScore },
                    { "Attempts", dungeon.Attempts },
                    { "Completions", dungeon.Completions },
                    { "UnlockedZones", dungeon.UnlockedZones },
                    { "CompletedFloors", dungeon.CompletedFloors }
                });
            }

            data["Dungeons"] = dungeons;

            var stats = new Dictionary<string, object> {
                { "TotalAttempts", _playerData.Statistics.TotalAttempts },
                { "TotalCompletions", _playerData.Statistics.TotalCompletions },
                { "TotalFloorsCleared", _playerData.Statistics.TotalFloorsCleared },
                { "TotalGoldEarned", _playerData.Statistics.TotalGoldEarned },
                { "TotalExperienceEarned", _playerData.Statistics.TotalExperienceEarned },
                { "LongestStreak", _playerData.Statistics.LongestStreak },
                { "CurrentStreak", _playerData.Statistics.CurrentStreak },
                { "BestScore", _playerData.Statistics.BestScore }
            };

            data["Statistics"] = stats;
            data["UnlockedZones"] = _playerData.UnlockedZones;
            data["HighestZoneUnlocked"] = _playerData.HighestZoneUnlocked;
            data["TotalStars"] = _playerData.TotalStars;

            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data) {
            if (data == null) return;

            _playerData = new PlayerSealedDungeonData();
            
            if (data.ContainsKey("Dungeons")) {
                var dungeons = (List<object>)data["Dungeons"];
                foreach (var d in dungeons) {
                    var dict = (Dictionary<string, object>)d;
                    var dungeon = new SealedDungeonData {
                        DungeonId = Convert.ToInt32(dict["DungeonId"]),
                        DungeonName = dict["DungeonName"].ToString(),
                        CurrentZone = (DungeonZone)Convert.ToInt32(dict["CurrentZone"]),
                        State = (SealedDungeonState)Convert.ToInt32(dict["State"]),
                        CurrentFloor = Convert.ToInt32(dict["CurrentFloor"]),
                        MaxFloors = Convert.ToInt32(dict["MaxFloors"]),
                        ClearedFloors = Convert.ToInt32(dict["ClearedFloors"]),
                        BestTime = Convert.ToInt32(dict["BestTime"]),
                        CurrentScore = Convert.ToInt32(dict["CurrentScore"]),
                        BestScore = Convert.ToInt32(dict["BestScore"]),
                        Attempts = Convert.ToInt32(dict["Attempts"]),
                        Completions = Convert.ToInt32(dict["Completions"])
                    };
                    
                    var unlockedZones = (List<object>)dict["UnlockedZones"];
                    dungeon.UnlockedZones = unlockedZones.ConvertAll(x => Convert.ToInt32(x));
                    
                    var completedFloors = (List<object>)dict["CompletedFloors"];
                    dungeon.CompletedFloors = completedFloors.ConvertAll(x => Convert.ToInt32(x));
                    
                    _playerData.Dungeons.Add(dungeon);
                }
            }
            
            if (data.ContainsKey("Statistics")) {
                var stats = (Dictionary<string, object>)data["Statistics"];
                _playerData.Statistics.TotalAttempts = Convert.ToInt32(stats["TotalAttempts"]);
                _playerData.Statistics.TotalCompletions = Convert.ToInt32(stats["TotalCompletions"]);
                _playerData.Statistics.TotalFloorsCleared = Convert.ToInt32(stats["TotalFloorsCleared"]);
                _playerData.Statistics.TotalGoldEarned = Convert.ToInt32(stats["TotalGoldEarned"]);
                _playerData.Statistics.TotalExperienceEarned = Convert.ToInt32(stats["TotalExperienceEarned"]);
                _playerData.Statistics.LongestStreak = Convert.ToInt32(stats["LongestStreak"]);
                _playerData.Statistics.CurrentStreak = Convert.ToInt32(stats["CurrentStreak"]);
                _playerData.Statistics.BestScore = Convert.ToInt32(stats["BestScore"]);
            }
            
            if (data.ContainsKey("UnlockedZones")) {
                var zones = (List<object>)data["UnlockedZones"];
                _playerData.UnlockedZones = zones.ConvertAll(x => (DungeonZone)Convert.ToInt32(x));
            }
            
            if (data.ContainsKey("HighestZoneUnlocked")) {
                _playerData.HighestZoneUnlocked = Convert.ToInt32(data["HighestZoneUnlocked"]);
            }
            
            if (data.ContainsKey("TotalStars")) {
                _playerData.TotalStars = Convert.ToInt32(data["TotalStars"]);
            }
            
            GD.Print("[SealedDungeonSystem] Save data imported");
        }

        public void AddTestData() {
            var dungeon = new SealedDungeonData {
                DungeonId = 1,
                DungeonName = "Ancient Seal",
                CurrentZone = DungeonZone.Entrance,
                State = SealedDungeonState.Available,
                CurrentFloor = 1,
                MaxFloors = 10,
                ClearedFloors = 0,
                Attempts = 0,
                Completions = 0,
                BestTime = int.MaxValue,
                BestScore = 0,
                CurrentScore = 0,
                UnlockedZones = new List<int> { 0, 1, 2 },
                CompletedFloors = new List<int>()
            };
            
            _playerData.Dungeons.Add(dungeon);
            _playerData.Statistics.TotalAttempts = 5;
            _playerData.Statistics.TotalCompletions = 2;
            _playerData.Statistics.TotalFloorsCleared = 15;
            _playerData.Statistics.TotalGoldEarned = 50000;
            _playerData.Statistics.TotalExperienceEarned = 25000;
            _playerData.Statistics.LongestStreak = 3;
            _playerData.Statistics.CurrentStreak = 1;
            _playerData.Statistics.BestScore = 2500;
            
            _playerData.UnlockedZones = new List<DungeonZone> {
                DungeonZone.Entrance,
                DungeonZone.WhisperingCorridor,
                DungeonZone.ForgottenChamber
            };
            _playerData.HighestZoneUnlocked = 2;
            _playerData.TotalStars = 10;
            
            GD.Print("[SealedDungeonSystem] Test data added");
        }
    }
}
