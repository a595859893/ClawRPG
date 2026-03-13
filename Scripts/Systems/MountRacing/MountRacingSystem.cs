using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    public partial class MountRacingSystem : Node {
        private MountRacingData _data;
        private MountRacingDatabase _database;
        
        public event Action<RaceResult> OnRaceCompleted;
        
        public void Initialize(MountRacingData data, MountRacingDatabase database) {
            _data = data;
            _database = database;
        }
        
        public string[] GetAvailableTracks() {
            var tracks = new List<string>();
            foreach (var track in _database.Tracks) {
                if (_data.UnlockedTracks.Contains(track.Key)) {
                    tracks.Add(track.Key);
                }
            }
            return tracks.ToArray();
        }
        
        public string[] GetAllTracks() {
            var tracks = new List<string>();
            foreach (var track in _database.Tracks) {
                tracks.Add(track.Key);
            }
            return tracks.ToArray();
        }
        
        public TrackConfig GetTrackConfig(string trackId) {
            return _database.Tracks.ContainsKey(trackId) ? _database.Tracks[trackId] : null;
        }
        
        public bool UnlockTrack(string trackId) {
            if (_database.Tracks.ContainsKey(trackId) && !_data.UnlockedTracks.Contains(trackId)) {
                _data.UnlockedTracks.Add(trackId);
                return true;
            }
            return false;
        }
        
        public RaceResult SimulateRace(string trackId, string mountId, int mountSpeed, int mountStamina) {
            var track = GetTrackConfig(trackId);
            if (track == null) return null;
            
            var difficulty = _database.DifficultySettings[track.Difficulty];
            
            // Calculate base time based on track length and mount speed
            float baseTime = track.Length / (float)mountSpeed;
            
            // Apply difficulty modifier
            baseTime /= difficulty.SpeedMod;
            
            // Add obstacle penalties
            float obstaclePenalty = 0;
            var random = new Random();
            for (int i = 0; i < track.Obstacles.Length; i++) {
                if (random.NextDouble() < difficulty.ObstacleChance) {
                    obstaclePenalty += random.Next(3, 10);
                }
            }
            
            // Stamina affects consistency
            float staminaMod = mountStamina / 100f;
            staminaMod = Math.Clamp(staminaMod, 0.7f, 1.3f);
            
            // Calculate final time
            int finalTime = (int)(baseTime + obstaclePenalty / staminaMod);
            
            // Simulate AI opponents
            int playerRank = 1;
            for (int i = 1; i < track.MinPlayers; i++) {
                int aiTime = finalTime + random.Next(-15, 20);
                if (aiTime < finalTime) {
                    playerRank++;
                }
            }
            
            // Calculate rewards
            var rankReward = _database.RankRewards.ContainsKey(playerRank) 
                ? _database.RankRewards[playerRank] 
                : _database.RankRewards[8];
            
            int goldReward = (int)(track.BaseReward * rankReward.GoldMultiplier);
            int expReward = (int)(track.BaseReward * rankReward.ExpMultiplier);
            
            // Update records
            UpdateRecords(trackId, mountId, finalTime, playerRank, goldReward, expReward);
            
            return new RaceResult {
                TrackId = trackId,
                MountId = mountId,
                Time = finalTime,
                Rank = playerRank,
                GoldReward = goldReward,
                ExpReward = expReward,
                Title = rankReward.Title,
                IsNewBestTime = _data.GetBestTime(trackId) == -1 || finalTime < _data.GetBestTime(trackId)
            };
        }
        
        private void UpdateRecords(string trackId, string mountId, int time, int rank, int gold, int exp) {
            // Update best time
            int currentBest = _data.GetBestTime(trackId);
            if (currentBest == -1 || time < currentBest) {
                _data.BestTimes[trackId] = time;
            }
            
            // Update total races
            if (!_data.TotalRaces.ContainsKey(trackId)) {
                _data.TotalRaces[trackId] = 0;
            }
            _data.TotalRaces[trackId]++;
            
            // Update wins
            if (rank == 1) {
                if (!_data.TotalWins.ContainsKey(trackId)) {
                    _data.TotalWins[trackId] = 0;
                }
                _data.TotalWins[trackId]++;
            }
            
            // Update totals
            _data.TotalGoldEarned += gold;
            _data.TotalExpEarned += exp;
            
            // Add to history
            var record = new MountRacingRecord {
                TrackId = trackId,
                MountId = mountId,
                Time = time,
                Rank = rank,
                Timestamp = DateTime.Now,
                GoldReward = gold,
                ExpReward = exp
            };
            _data.RacingHistory[DateTime.Now.ToString("yyyyMMddHHmmss")] = record;
            
            OnRaceCompleted?.Invoke(new RaceResult {
                TrackId = trackId,
                MountId = mountId,
                Time = time,
                Rank = rank,
                GoldReward = gold,
                ExpReward = exp,
                Title = "",
                IsNewBestTime = currentBest == -1 || time < currentBest
            });
        }
        
        public RacingStatistics GetStatistics() {
            int totalRaces = 0;
            int totalWins = 0;
            foreach (var races in _data.TotalRaces.Values) {
                totalRaces += races;
            }
            foreach (var wins in _data.TotalWins.Values) {
                totalWins += wins;
            }
            
            return new RacingStatistics {
                TotalRaces = totalRaces,
                TotalWins = totalWins,
                WinRate = totalRaces > 0 ? (float)totalWins / totalRaces * 100 : 0,
                TotalGoldEarned = _data.TotalGoldEarned,
                TotalExpEarned = _data.TotalExpEarned,
                TracksUnlocked = _data.UnlockedTracks.Count,
                BestTimes = new Dictionary<string, int>(_data.BestTimes)
            };
        }
        
        public int GetBestTime(string trackId) {
            return _data.GetBestTime(trackId);
        }
        
        public int GetWinCount(string trackId) {
            return _data.GetWinCount(trackId);
        }
    }
    
    public class RaceResult {
        public string TrackId;
        public string MountId;
        public int Time;
        public int Rank;
        public int GoldReward;
        public int ExpReward;
        public string Title;
        public bool IsNewBestTime;
    }
    
    public class RacingStatistics {
        public int TotalRaces;
        public int TotalWins;
        public float WinRate;
        public int TotalGoldEarned;
        public int TotalExpEarned;
        public int TracksUnlocked;
        public Dictionary<string, int> BestTimes;
    }
}
