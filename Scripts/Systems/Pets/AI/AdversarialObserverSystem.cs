using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Managers;
using ClawRPG.Scripts.Systems.Pets;

namespace ClawRPG.Systems.Pets.AI
{
    /// <summary>
    /// 敌对观察者系统 - Strategic Critic（REQ-138）
    /// 职责：独立评估玩家策略，发现偏差时以叙事方式提问而非纠正
    /// 核心原则：提问 > 纠正。玩家永远保留决策权。
    /// </summary>
    public partial class AdversarialObserverSystem : BaseSystem
    {
        // ===== 配置 =====

        [Export] private float _observationInterval = 1.0f;
        [Export] private float _confidenceThreshold = 0.65f;
        [Export] private float _silenceCooldownDuration = 8.0f;
        [Export] private int _actionHistorySize = 20;
        [Export] private float _goalInferenceWindow = 10.0f;
        [Export] private float _narrativeCooldown = 15.0f;

        // ===== 状态 =====

        private AdversarialObserverState _observerState = new AdversarialObserverState();
        private List<PlayerActionRecord> _actionHistory = new List<PlayerActionRecord>();
        private PlayerGoalInference _currentGoalInference = new PlayerGoalInference();
        private TrajectoryPrediction _currentTrajectory = new TrajectoryPrediction();
        private float _observationTimer = 0f;
        private float _silenceTimer = 0f;
        private float _narrativeTimer = 0f;
        private bool _isInCombat = false;
        private int _decisionTickId = 0;

        // ===== 引用 =====

        private CharacterBody2D _player;
        private Node _playerTargeting;
        private Node _combatStatusNode;

        // ===== 信号 =====

        /// <summary>Observer 发现分歧并提问</summary>
        public Action<ObserverChallenge> OnObserverChallenge;

        /// <summary>Observer 置信度变化（用于 UI 显示观察者"心态"）</summary>
        public Action<float> OnConfidenceChanged;

        /// <summary>Observer 预测被验证（正确或错误）</summary>
        public Action<bool> OnPredictionVerified;

        // ===== 生命周期 =====

        public override void _Ready()
        {
            // 事件订阅
            if (EventBusManager.Instance != null)
            {
                EventBusManager.Instance.Subscribe(EventBusManager.Events.CombatStarted, OnCombatStarted);
                EventBusManager.Instance.Subscribe(EventBusManager.Events.CombatEnded, OnCombatEnded);
                EventBusManager.Instance.Subscribe(EventBusManager.Events.PlayerDied, OnPlayerDied);
            }

            // 加载持久化数据
            LoadPersistentState();

            // 初始化 ObserverBubbleUI
            InitializeBubbleUI();

            GD.Print("[AdversarialObserverSystem] Initialized - Strategic Critic ready");
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta;

            // 更新冷却
            if (_silenceTimer > 0)
                _silenceTimer -= dt;
            if (_narrativeTimer > 0)
                _narrativeTimer -= dt;

            if (!_isInCombat || _observerState.IsDisabled)
                return;

            // 定期观察
            _observationTimer += dt;
            if (_observationTimer >= _observationInterval)
            {
                _observationTimer = 0f;
                RunObservationCycle();
            }
        }

        // ===== 公开 API =====

        /// <summary>
        /// 初始化系统
        /// </summary>
        public void Initialize(CharacterBody2D player)
        {
            _player = player;
            GD.Print("[AdversarialObserverSystem] Initialized with player reference");
        }

        /// <summary>
        /// 记录玩家动作（由 SkillComboSystem / PlayerController 调用）
        /// </summary>
        public void RecordPlayerAction(string actionType, string actionTarget, Vector2 position, float healthPercent, float damageDealt = 0f, float damageTaken = 0f)
        {
            if (_actionHistory.Count >= _actionHistorySize)
                _actionHistory.RemoveAt(0);

            _decisionTickId = PetDecisionSystem.NextDecisionTick();

            var record = new PlayerActionRecord
            {
                TickId = _decisionTickId,
                Timestamp = Time.GetTicksMsec() / 1000f,
                ActionType = actionType,
                ActionTarget = actionTarget,
                Position = position,
                HealthPercent = healthPercent,
                DamageDealt = damageDealt,
                DamageTaken = damageTaken
            };

            _actionHistory.Add(record);

            // 立即更新目标推断（增量）
            UpdateGoalInferenceIncremental(record);
        }

        /// <summary>
        /// 获取当前世界评估（用于 UI 显示 Observer 的"视野"）
        /// </summary>
        public WorldAssessment GetCurrentAssessment()
        {
            return _observerState.CurrentAssessment;
        }

        /// <summary>
        /// 获取 Observer 状态（用于 UI 显示）
        /// </summary>
        public AdversarialObserverState GetObserverState()
        {
            return _observerState;
        }

        /// <summary>
        /// 获取当前目标推断
        /// </summary>
        public PlayerGoalInference GetCurrentGoalInference()
        {
            return _currentGoalInference;
        }

        /// <summary>
        /// 启用/禁用 Observer
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            _observerState.IsDisabled = !enabled;
            GD.Print($"[AdversarialObserverSystem] Observer {(enabled ? "enabled" : "disabled")}");
        }

        // ===== 持久化 =====

        /// <summary>
        /// 获取持久化数据（用于 SaveLoadSystem）
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary<string, object>
            {
                ["player_goal_model"] = _observerState.PersistentState.PlayerGoalModel,
                ["confidence"] = _observerState.PersistentState.Confidence,
                ["declared_goal"] = _observerState.PersistentState.DeclaredGoal,
                ["prediction_success"] = _observerState.PersistentState.PredictionSuccessCount,
                ["prediction_failure"] = _observerState.PersistentState.PredictionFailureCount,
                ["issued_signatures"] = new List<string>(_observerState.PersistentState.IssuedChallengeSignatures)
            };
        }

        /// <summary>
        /// 导入持久化数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            if (data.Contains("player_goal_model"))
                _observerState.PersistentState.PlayerGoalModel = data["player_goal_model"].ToString();
            if (data.Contains("confidence"))
                _observerState.PersistentState.Confidence = (float)Convert.ToDouble(data["confidence"]);
            if (data.Contains("declared_goal"))
                _observerState.PersistentState.DeclaredGoal = data["declared_goal"].ToString();
            if (data.Contains("prediction_success"))
                _observerState.PersistentState.PredictionSuccessCount = Convert.ToInt32(data["prediction_success"]);
            if (data.Contains("prediction_failure"))
                _observerState.PersistentState.PredictionFailureCount = Convert.ToInt32(data["prediction_failure"]);
            if (data.Contains("issued_signatures"))
            {
                var list = data["issued_signatures"] as System.Collections.IList;
                if (list != null)
                    foreach (var item in list)
                        _observerState.PersistentState.IssuedChallengeSignatures.Add(item.ToString());
            }

            GD.Print("[AdversarialObserverSystem] Loaded persistent state");
        }
    }
}
