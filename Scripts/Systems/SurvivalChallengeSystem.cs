using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 生存挑战系统 - 管理生存挑战模式
    /// 支持：无尽波次、限时击杀、Boss Rush、竞技场生存、无尽地下城
    /// </summary>
    public partial class SurvivalChallengeSystem : BaseSystem
    {
        // ===== 核心属性 =====
        public static SurvivalChallengeSystem Instance { get; private set; }
        private SurvivalChallengeData.PlayerChallengeData _playerData = new();
        private SurvivalChallengeData.ActiveChallenge _currentChallenge;
        private List<Node2D> _activeEnemies = new();
        private Node2D _player;
        private string _saveKey = "survival_challenge_data";

        public bool IsChallengeActive => _currentChallenge != null &&
            _currentChallenge.State == SurvivalChallengeData.ChallengeState.InProgress;
        public SurvivalChallengeData.ActiveChallenge CurrentChallenge => _currentChallenge;

        // ===== 事件信号 =====
        public Action<SurvivalChallengeData.ChallengeResult> OnChallengeCompleted;
        public Action<SurvivalChallengeData.ActiveChallenge> OnChallengeStarted;
        public Action<int> OnWaveStarted;
        public Action<int> OnEnemyKilled;
        public Action<float> OnTimeUpdated;

        // ===== 生命周期 =====
        public override void _Ready()
        {
            Instance = this;
            base._Ready();
            Initialize();
        }

        protected override void Initialize()
        {
            LoadData();
            IsInitialized = true;
            GD.Print("生存挑战系统已初始化");
        }

        public override void _Process(double delta)
        {
            if (!IsChallengeActive) return;
            var config = SurvivalChallengeDatabase.GetChallenge(_currentChallenge.ConfigId);
            if (config == null) return;
            _currentChallenge.ElapsedTime += (float)delta;
            OnTimeUpdated?.Invoke(_currentChallenge.ElapsedTime);
            if (config.TimeLimit > 0 && _currentChallenge.ElapsedTime >= config.TimeLimit)
            {
                CompleteChallenge(false);
                return;
            }
            if (_currentChallenge.IsWaveInProgress && _currentChallenge.EnemiesRemaining <= 0)
            {
                _currentChallenge.CurrentWave++;
                _currentChallenge.IsWaveInProgress = false;
                if (config.WaveCount > 0 && _currentChallenge.CurrentWave > config.WaveCount)
                {
                    CompleteChallenge(true);
                    return;
                }
                _currentChallenge.EnemiesRemaining = config.EnemiesPerWave;
                _currentChallenge.IsWaveInProgress = true;
                SpawnWave(config);
                OnWaveStarted?.Invoke(_currentChallenge.CurrentWave);
            }
            CleanupDeadEnemies();
        }
    }
}
