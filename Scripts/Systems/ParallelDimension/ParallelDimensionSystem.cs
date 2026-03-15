using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.ParallelDimension;

namespace ClawRPG.Scripts.Systems.ParallelDimension {
    
    public class ParallelDimensionSystem : BaseSystem {
        
        private static ParallelDimensionSystem _instance;
        public static ParallelDimensionSystem Instance {
            get {
                if (_instance == null) {
                    GD.Print("[ParallelDimensionSystem] Instance is null!");
                }
                return _instance;
            }
        }
        
        private PlayerDimensionData _playerData;
        private DimensionEntry _currentDimension;
        private bool _isInDimension = false;
        private int _currentFloorEnemiesDefeated = 0;
        private int _currentScore = 0;
        private DateTime _dimensionStartTime;
        
        public Signal DimensionEntered { get; } = new Signal();
        public Signal DimensionLeft { get; } = new Signal();
        public Signal FloorCompleted { get; } = new Signal();
        public Signal DimensionCompleted { get; } = new Signal();
        public Signal DimensionMastered { get; } = new Signal();
        public Signal ScoreUpdated { get; } = new Signal();
        
        public override void _Ready() {
            _instance = this;
            ParallelDimensionDatabase.Initialize();
            _playerData = new PlayerDimensionData {
                PlayerId = 1
            };
            GD.Print("[ParallelDimensionSystem] Initialized - Parallel Dimension System ready!");
        }
        
        public bool EnterDimension(int dimensionId, int playerLevel, int playerGold) {
            var dimension = ParallelDimensionDatabase.GetDimension(dimensionId);
            if (dimension == null) {
                GD.PrintErr($"[ParallelDimensionSystem] Dimension {dimensionId} not found!");
                return false;
            }
            
            if (dimension.State == DimensionState.Locked) {
                GD.Print($"[ParallelDimensionSystem] Dimension {dimensionId} is locked. Required level: {dimension.RequiredLevel}");
                return false;
            }
            
            if (dimension.State == DimensionState.InProgress) {
                GD.Print($"[ParallelDimensionSystem] Already in dimension {dimensionId}!");
                return false;
            }
            
            if (playerGold < dimension.EntryCost) {
                GD.Print($"[ParallelDimensionSystem] Not enough gold! Need {dimension.EntryCost}, have {playerGold}");
                return false;
            }
            
            _currentDimension = dimension;
            _currentDimension.State = DimensionState.InProgress;
            _currentDimension.LastEntered = DateTime.Now;
            _isInDimension = true;
            _currentFloorEnemiesDefeated = 0;
            _currentScore = 0;
            _dimensionStartTime = DateTime.Now;
            
            DimensionEntered.Call();
            GD.Print($"[ParallelDimensionSystem] Entered {dimension.DimensionName} - Floor {dimension.CurrentFloor}/{dimension.MaxFloors}");
            return true;
        }
        
        public void ExitDimension() {
            if (!_isInDimension || _currentDimension == null) return;
            
            var timeInDimension = (DateTime.Now - _dimensionStartTime).TotalSeconds;
            var finalScore = CalculateScore(timeInDimension);
            
            if (_currentDimension.BestScore < finalScore) {
                _currentDimension.BestScore = finalScore;
            }
            if (_currentDimension.BestTime == 0 || timeInDimension < _currentDimension.BestTime) {
                _currentDimension.BestTime = (int)timeInDimension;
            }
            
            _currentDimension.State = DimensionState.Available;
            _isInDimension = false;
            
            DimensionLeft.Call();
            GD.Print($"[ParallelDimensionSystem] Exited {_currentDimension.DimensionName}. Score: {finalScore}, Time: {timeInDimension:F1}s");
            _currentDimension = null;
        }
        
        public void OnEnemyDefeated(int baseExp, int baseGold) {
            if (!_isInDimension || _currentDimension == null) return;
            
            var rules = _currentDimension.Rules;
            var expReward = (int)(baseExp * rules.ExpMultiplier);
            var goldReward = (int)(baseGold * rules.DropMultiplier);
            
            _currentScore += expReward + goldReward;
            _currentFloorEnemiesDefeated++;
            
            ScoreUpdated.Call();
            
            EmitSignal(nameof(ScoreUpdated));
        }
        
        public void CompleteFloor() {
            if (!_isInDimension || _currentDimension == null) return;
            
            var floor = _currentDimension.CurrentFloor;
            var rewards = ParallelDimensionDatabase.GetFloorRewards(_currentDimension.DimensionId, floor);
            
            foreach (var reward in rewards) {
                _currentScore += reward.GoldReward + reward.ExpReward;
            }
            
            if (_currentDimension.CurrentFloor >= _currentDimension.MaxFloors) {
                CompleteDimension();
            } else {
                _currentDimension.CurrentFloor++;
                _currentFloorEnemiesDefeated = 0;
                FloorCompleted.Call();
                GD.Print($"[ParallelDimensionSystem] Floor {floor} completed! Now on floor {_currentDimension.CurrentFloor}");
            }
            
            ScoreUpdated.Call();
        }
        
        private void CompleteDimension() {
            if (_currentDimension == null) return;
            
            _currentDimension.TimesCompleted++;
            _currentDimension.State = DimensionState.Completed;
            
            if (_currentDimension.TimesCompleted >= 10) {
                _currentDimension.State = DimensionState.Mastered;
                _playerData.DimensionsMastered++;
                DimensionMastered.Call();
                GD.Print($"[ParallelDimensionSystem] {_currentDimension.DimensionName} MASTERED!");
            }
            
            var finalScore = CalculateScore((DateTime.Now - _dimensionStartTime).TotalSeconds);
            if (_playerData.DimensionHighScores.ContainsKey(_currentDimension.DimensionId)) {
                if (_playerData.DimensionHighScores[_currentDimension.DimensionId] < finalScore) {
                    _playerData.DimensionHighScores[_currentDimension.DimensionId] = finalScore;
                }
            } else {
                _playerData.DimensionHighScores[_currentDimension.DimensionId] = finalScore;
            }
            
            if (_playerData.DimensionCompletions.ContainsKey(_currentDimension.DimensionId)) {
                _playerData.DimensionCompletions[_currentDimension.DimensionId]++;
            } else {
                _playerData.DimensionCompletions[_currentDimension.DimensionId] = 1;
            }
            
            _playerData.TotalDimensionScore += finalScore;
            
            DimensionCompleted.Call();
            GD.Print($"[ParallelDimensionSystem] Dimension {_currentDimension.DimensionName} completed! Final Score: {finalScore}");
        }
        
        private int CalculateScore(double timeInSeconds) {
            var baseScore = _currentScore;
            var timeBonus = (int)((_currentDimension.MaxFloors * 60 - timeInSeconds) * 10);
            if (timeBonus < 0) timeBonus = 0;
            
            var floorBonus = _currentDimension.CurrentFloor * 100;
            var completionBonus = _currentDimension.TimesCompleted * 50;
            
            return baseScore + timeBonus + floorBonus + completionBonus;
        }
        
        public bool UnlockNextDimension(int playerLevel) {
            var nextId = _currentDimension != null ? _currentDimension.DimensionId + 1 : 2;
            return ParallelDimensionDatabase.UnlockDimension(nextId, playerLevel);
        }
        
        public DimensionEntry GetCurrentDimension() {
            return _currentDimension;
        }
        
        public bool IsInDimension() {
            return _isInDimension;
        }
        
        public int GetCurrentScore() {
            return _currentScore;
        }
        
        public PlayerDimensionData GetPlayerData() {
            return _playerData;
        }
        
        public List<DimensionEntry> GetAvailableDimensions() {
            return ParallelDimensionDatabase.GetUnlockedDimensions();
        }
        
        public Dictionary<string, Variant> ExportSaveData() {
            var data = new Dictionary<string, Variant>();
            
            var dimensionStates = new List<Dictionary<string, Variant>>();
            foreach (var dim in ParallelDimensionDatabase.GetAllDimensions()) {
                dimensionStates.Add(new Dictionary<string, Variant> {
                    ["dimension_id"] = dim.DimensionId,
                    ["state"] = (int)dim.State,
                    ["current_floor"] = dim.CurrentFloor,
                    ["best_score"] = dim.BestScore,
                    ["best_time"] = dim.BestTime,
                    ["times_completed"] = dim.TimesCompleted,
                    ["last_entered"] = dim.LastEntered.ToString("o")
                });
            }
            
            data["dimension_states"] = dimensionStates;
            data["total_score"] = _playerData.TotalDimensionScore;
            data["dimensions_mastered"] = _playerData.DimensionsMastered;
            
            return data;
        }
        
        public void ImportSaveData(Dictionary<string, Variant> data) {
            if (!data.Contains("dimension_states")) return;
            
            var dimensionStates = (Godot.Collections.Array)data["dimension_states"];
            foreach (Dictionary<string, Variant> dimData in dimensionStates) {
                var dimId = (int)dimData["dimension_id"];
                var dim = ParallelDimensionDatabase.GetDimension(dimId);
                if (dim != null) {
                    dim.State = (DimensionState)(int)dimData["state"];
                    dim.CurrentFloor = (int)dimData["current_floor"];
                    dim.BestScore = (int)dimData["best_score"];
                    dim.BestTime = (int)dimData["best_time"];
                    dim.TimesCompleted = (int)dimData["times_completed"];
                    
                    var lastEnteredStr = (string)dimData["last_entered"];
                    if (DateTime.TryParse(lastEnteredStr, out var lastEntered)) {
                        dim.LastEntered = lastEntered;
                    }
                }
            }
            
            _playerData.TotalDimensionScore = (int)data["total_score"];
            _playerData.DimensionsMastered = (int)data["dimensions_mastered"];
            
            GD.Print("[ParallelDimensionSystem] Save data imported successfully!");
        }
        
        public void ResetProgress() {
            foreach (var dim in ParallelDimensionDatabase.GetAllDimensions()) {
                dim.State = DimensionState.Locked;
                dim.CurrentFloor = 1;
                dim.BestScore = 0;
                dim.BestTime = 0;
                dim.TimesCompleted = 0;
            }
            
            ParallelDimensionDatabase.GetDimension(1).State = DimensionState.Available;
            
            _playerData = new PlayerDimensionData {
                PlayerId = 1
            };
            
            GD.Print("[ParallelDimensionSystem] Progress reset!");
        }
    }
}
