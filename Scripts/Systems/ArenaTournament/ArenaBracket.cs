using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ArenaTournament {
    /// <summary>
    /// 竞技场锦标赛赛程 - 管理锦标赛赛程
    /// </summary>
    public partial class ArenaBracket : BaseSystem {
        
        /// <summary>
        /// 锦标赛阶段
        /// </summary>
        public enum TournamentPhase {
            Registration,
            RoundOf16,
            QuarterFinals,
            SemiFinals,
            Finals,
            Finished
        }
        
        /// <summary>
        /// 比赛结果
        /// </summary>
        public class MatchResult {
            public int Round;
            public int Player1Id;
            public int Player2Id;
            public int WinnerId;
            public bool IsBye;
        }
        
        private TournamentPhase _currentPhase = TournamentPhase.Registration;
        private List<int> _registeredPlayers = new();
        private List<MatchResult> _matchResults = new();
        private int _currentRound = 0;
        private int _totalRounds = 4;
        
        public override void _Ready() {
            base._Ready();
        }
        
        /// <summary>
        /// 注册玩家
        /// </summary>
        public bool RegisterPlayer(int playerId) {
            if (_currentPhase != TournamentPhase.Registration) {
                return false;
            }
            
            if (!_registeredPlayers.Contains(playerId)) {
                _registeredPlayers.Add(playerId);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 开始锦标赛
        /// </summary>
        public void StartTournament() {
            if (_registeredPlayers.Count < 2) {
                GD.PrintErr("[ArenaBracket] Not enough players to start");
                return;
            }
            
            _currentRound = 1;
            _currentPhase = GetPhaseForRound(_currentRound);
            GenerateMatches();
            
            GD.Print($"[ArenaBracket] Tournament started with {_registeredPlayers.Count} players");
        }
        
        /// <summary>
        /// 生成比赛对阵
        /// </summary>
        private void GenerateMatches() {
            _matchResults.Clear();
            
            // 简单配对：相邻玩家对战
            for (int i = 0; i < _registeredPlayers.Count - 1; i += 2) {
                _matchResults.Add(new MatchResult {
                    Round = _currentRound,
                    Player1Id = _registeredPlayers[i],
                    Player2Id = _registeredPlayers[i + 1],
                    WinnerId = -1,
                    IsBye = false
                });
            }
            
            // 检查轮空
            if (_registeredPlayers.Count % 2 == 1) {
                var lastPlayer = _registeredPlayers[_registeredPlayers.Count - 1];
                _matchResults.Add(new MatchResult {
                    Round = _currentRound,
                    Player1Id = lastPlayer,
                    Player2Id = -1,
                    WinnerId = lastPlayer,
                    IsBye = true
                });
            }
        }
        
        /// <summary>
        /// 记录比赛结果
        /// </summary>
        public void RecordResult(int player1Id, int player2Id, int winnerId) {
            foreach (var match in _matchResults) {
                if (match.Round == _currentRound && 
                    match.Player1Id == player1Id && 
                    match.Player2Id == player2Id) {
                    match.WinnerId = winnerId;
                    break;
                }
            }
        }
        
        /// <summary>
        /// 进入下一轮
        /// </summary>
        public void NextRound() {
            _currentRound++;
            
            if (_currentRound > _totalRounds) {
                _currentPhase = TournamentPhase.Finished;
            } else {
                _currentPhase = GetPhaseForRound(_currentRound);
                GenerateMatches();
            }
        }
        
        /// <summary>
        /// 获取当前阶段
        /// </summary>
        public TournamentPhase GetCurrentPhase() {
            return _currentPhase;
        }
        
        /// <summary>
        /// 获取当前轮次
        /// </summary>
        public int GetCurrentRound() {
            return _currentRound;
        }
        
        /// <summary>
        /// 获取比赛结果
        /// </summary>
        public List<MatchResult> GetMatchResults() {
            return new List<MatchResult>(_matchResults);
        }
        
        /// <summary>
        /// 获取阶段名称
        /// </summary>
        private TournamentPhase GetPhaseForRound(int round) {
            return round switch {
                1 => TournamentPhase.RoundOf16,
                2 => TournamentPhase.QuarterFinals,
                3 => TournamentPhase.SemiFinals,
                4 => TournamentPhase.Finals,
                _ => TournamentPhase.Finished
            };
        }
        
        public override Dictionary<string, object> ExportSaveData() {
            var data = new Dictionary<string, object>();
            data["currentPhase"] = (int)_currentPhase;
            data["currentRound"] = _currentRound;
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data) {
            if (data.Contains("currentPhase")) {
                _currentPhase = (TournamentPhase)(int)data["currentPhase"];
            }
            if (data.Contains("currentRound")) {
                _currentRound = (int)data["currentRound"];
            }
        }
    }
}
