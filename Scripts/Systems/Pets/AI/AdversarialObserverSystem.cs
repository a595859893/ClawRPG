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
        private static AdversarialObserverSystem _instance;
        public static AdversarialObserverSystem Instance => _instance ??= new AdversarialObserverSystem();

        // ===== 配置 =====
        
        private float _observationInterval = 1.0f;         // 观察间隔（秒）
        private float _confidenceThreshold = 0.65f;        // 只在置信度 > 此值时发声
        private float _silenceCooldownDuration = 8.0f;     // 提问后沉默 8 秒
        private int _actionHistorySize = 20;              // 保留最近 N 条动作记录
        private float _goalInferenceWindow = 10.0f;        // 目标推断时间窗口（秒）
        private float _narrativeCooldown = 15.0f;          // 两次叙事提问的最小间隔

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
            _instance = this;

            // 事件订阅
            if (EventBusManager.Instance != null)
            {
                EventBusManager.Instance.Subscribe(EventBusManager.Events.CombatStarted, OnCombatStarted);
                EventBusManager.Instance.Subscribe(EventBusManager.Events.CombatEnded, OnCombatEnded);
                EventBusManager.Instance.Subscribe(EventBusManager.Events.PlayerDied, OnPlayerDied);
            }

            // 加载持久化数据
            LoadPersistentState();

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
            _instance = this;
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

        /// <summary>
        /// 获取持久化数据（用于 SaveLoadSystem）
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            return new Dictionary
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
        public override void ImportSaveData(Dictionary data)
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

        // ===== 内部逻辑 =====

        private void OnCombatStarted()
        {
            _isInCombat = true;
            _actionHistory.Clear();
            _observationTimer = 0f;
            _decisionTickId = PetDecisionSystem.NextDecisionTick();
            GD.Print("[AdversarialObserverSystem] Combat started - beginning observation");
        }

        private void OnCombatEnded()
        {
            _isInCombat = false;
            ValidateLastPrediction();
            GD.Print("[AdversarialObserverSystem] Combat ended - validating prediction");
        }

        private void OnPlayerDied()
        {
            if (_currentTrajectory.PredictedTrajectory.Contains("死亡"))
            {
                _observerState.PersistentState.PredictionSuccessCount++;
            }
            else
            {
                _observerState.PredictionFailureCount++;
                // Observer 承认错误
                EmitSelfCorrection();
            }
        }

        /// <summary>
        /// 每次观察周期：评估 → 推断 → 预测 → 检测分歧 → 提问
        /// </summary>
        private void RunObservationCycle()
        {
            // Step 1: 评估当前世界局势
            AssessWorld();

            // Step 2: 基于历史推断玩家目标
            InferPlayerGoal();

            // Step 3: 预测玩家轨迹
            PredictTrajectory();

            // Step 4: 检测分歧
            var disagreement = DetectDisagreement();

            // Step 5: 如果有分歧且通过阈值和冷却检查，生成提问
            if (disagreement != null && ShouldChallenge())
            {
                GenerateAndEmitChallenge(disagreement);
            }
        }

        /// <summary>
        /// Step 1: 评估当前世界局势（Enemy HP, terrain, resources）
        /// </summary>
        private void AssessWorld()
        {
            var assessment = new WorldAssessment();

            if (_player == null)
            {
                _observerState.CurrentAssessment = assessment;
                return;
            }

            assessment.PlayerPosition = _player.GlobalPosition;
            assessment.PlayerHealthPercent = GetPlayerHealthPercent();

            // 获取敌人数量
            var enemies = GetTree().GetNodesInGroup("enemy");
            assessment.EnemyCount = enemies.Count;

            // 附近敌人
            int nearbyCount = 0;
            Vector2 keyEnemyPos = Vector2.Zero;
            float closestDist = float.MaxValue;

            foreach (Node enemy in enemies)
            {
                if (enemy is Node2D enemyNode)
                {
                    float dist = _player.GlobalPosition.DistanceTo(enemyNode.GlobalPosition);
                    if (dist < 300f)  // 视野范围
                    {
                        nearbyCount++;
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            keyEnemyPos = enemyNode.GlobalPosition;
                        }
                    }
                }
            }

            assessment.NearbyEnemyCount = nearbyCount;
            assessment.KeyEnemyPosition = keyEnemyPos;

            // 威胁等级
            if (assessment.PlayerHealthPercent < 0.2f && nearbyCount > 0)
                assessment.ThreatLevel = "危险";
            else if (assessment.EnemyCount > 3 || nearbyCount > 2)
                assessment.ThreatLevel = "高";
            else if (assessment.EnemyCount > 0)
                assessment.ThreatLevel = "中";
            else
                assessment.ThreatLevel = "低";

            // 玩家状态
            if (assessment.PlayerHealthPercent < 0.3f)
                assessment.PlayerStatus = "危险";
            else if (assessment.PlayerHealthPercent < 0.6f)
                assessment.PlayerStatus = "受伤";
            else
                assessment.PlayerStatus = "正常";

            // 当前动作（基于最近动作记录）
            if (_actionHistory.Count > 0)
                assessment.CurrentAction = _actionHistory[_actionHistory.Count - 1].ActionType;

            _observerState.CurrentAssessment = assessment;
        }

        /// <summary>
        /// Step 2: 推断玩家目标（基于最近 N 个动作）
        /// </summary>
        private void InferPlayerGoal()
        {
            if (_actionHistory.Count < 3)
            {
                _currentGoalInference = new PlayerGoalInference
                {
                    GoalType = "探索",
                    GoalDescription = "我还不确定你想做什么",
                    Confidence = 0.2f
                };
                return;
            }

            // 统计最近动作类型
            int attackCount = 0, healCount = 0, moveCount = 0, retreatCount = 0, collectCount = 0;
            string dominantTarget = "";

            foreach (var record in _actionHistory)
            {
                switch (record.ActionType)
                {
                    case "attack": attackCount++; break;
                    case "heal": healCount++; break;
                    case "move": moveCount++; break;
                    case "retreat": retreatCount++; break;
                    case "collect": collectCount++; break;
                }
                if (record.ActionTarget == record.ActionTarget)  // 简单重复检测
                    dominantTarget = record.ActionTarget;
            }

            int total = _actionHistory.Count;
            string goalType;
            string goalDesc;
            float confidence;

            // 主要行为模式判断
            if (attackCount > total * 0.5)
            {
                goalType = "进攻";
                goalDesc = $"你似乎在主动消灭敌人（{attackCount}次攻击）";
                confidence = Mathf.Min(0.9f, 0.5f + attackCount * 0.05f);
            }
            else if (healCount > total * 0.4)
            {
                goalType = "生存";
                goalDesc = $"你专注于恢复生命（{healCount}次治疗）";
                confidence = Mathf.Min(0.85f, 0.4f + healCount * 0.06f);
            }
            else if (retreatCount > total * 0.3)
            {
                goalType = "撤退";
                goalDesc = "你似乎在寻找出路";
                confidence = 0.7f;
            }
            else if (collectCount > total * 0.3)
            {
                goalType = "收集";
                goalDesc = $"你在收集周围资源（{collectCount}次）";
                confidence = 0.65f;
            }
            else
            {
                goalType = "探索";
                goalDesc = "你在这个区域四处探索";
                confidence = 0.5f;
            }

            _currentGoalInference = new PlayerGoalInference
            {
                GoalType = goalType,
                GoalDescription = goalDesc,
                Confidence = confidence,
                SupportingActionCount = Mathf.Max(attackCount, healCount, retreatCount, collectCount)
            };

            // 更新 Observer 持久化目标模型（平滑更新）
            float updateRate = 0.1f;
            _observerState.PersistentState.PlayerGoalModel = goalDesc;
            _observerState.PersistentState.Confidence = Mathf.Lerp(_observerState.PersistentState.Confidence, confidence, updateRate);
        }

        /// <summary>
        /// 增量更新目标推断（新动作加入时微调）
        /// </summary>
        private void UpdateGoalInferenceIncremental(PlayerActionRecord newRecord)
        {
            if (_currentGoalInference.Confidence < 0.4f)
                return;  // 置信度太低，不增量更新

            // 简单的贝叶斯更新：如果新动作符合当前推断，增强置信度
            bool consistent = false;
            switch (_currentGoalInference.GoalType)
            {
                case "进攻": consistent = newRecord.ActionType == "attack"; break;
                case "生存": consistent = newRecord.ActionType == "heal"; break;
                case "撤退": consistent = newRecord.ActionType == "retreat"; break;
                case "收集": consistent = newRecord.ActionType == "collect"; break;
            }

            float delta = consistent ? 0.02f : -0.03f;
            _currentGoalInference.Confidence = Mathf.Clamp(_currentGoalInference.Confidence + delta, 0.1f, 0.95f);
        }

        /// <summary>
        /// Step 3: 预测玩家轨迹
        /// </summary>
        private void PredictTrajectory()
        {
            if (_actionHistory.Count == 0)
            {
                _currentTrajectory = new TrajectoryPrediction
                {
                    PredictedTrajectory = "无预测数据",
                    Destination = ""
                };
                return;
            }

            var latest = _actionHistory[_actionHistory.Count - 1];
            var assessment = _observerState.CurrentAssessment;

            string trajectory, destination;

            switch (latest.ActionType)
            {
                case "attack":
                    if (assessment.NearbyEnemyCount > 0)
                    {
                        trajectory = "继续进攻，消耗血量";
                        destination = "战斗消耗";
                    }
                    else
                    {
                        trajectory = "寻找下一个敌人";
                        destination = "寻找敌人";
                    }
                    break;

                case "heal":
                    if (assessment.PlayerHealthPercent < 0.3f)
                    {
                        trajectory = "继续治疗，可能被围攻";
                        destination = "持续治疗";
                    }
                    else
                    {
                        trajectory = "血量恢复后可能转为进攻";
                        destination = "进攻准备";
                    }
                    break;

                case "retreat":
                    trajectory = "逃离当前区域";
                    destination = assessment.ThreatLevel == "危险" ? "安全区域" : "未知";
                    break;

                case "move":
                    trajectory = "转移位置";
                    destination = "新区域";
                    break;

                default:
                    trajectory = "当前状态持续";
                    destination = "维持现状";
                    break;
            }

            // 如果血量很低但还在进攻，预测可能死亡
            if (assessment.PlayerHealthPercent < 0.2f && latest.ActionType == "attack" && assessment.NearbyEnemyCount > 1)
            {
                trajectory += " → 可能倒下";
                destination = "危险轨迹";
            }

            _currentTrajectory = new TrajectoryPrediction
            {
                PredictedTrajectory = trajectory,
                Destination = destination,
                Accuracy = 1.0f,
                IsVerified = false
            };
        }

        /// <summary>
        /// Step 4: 检测分歧（玩家目标 vs 预测轨迹）
        /// </summary>
        private DisagreementRecord DetectDisagreement()
        {
            if (_actionHistory.Count < 3)
                return null;

            var assessment = _observerState.CurrentAssessment;
            var latest = _actionHistory[_actionHistory.Count - 1];
            var goal = _currentGoalInference;

            // 分歧检测规则

            // 1. GoalDrift: 血量危险但还在进攻
            if (assessment.PlayerHealthPercent < 0.25f && latest.ActionType == "attack" && assessment.NearbyEnemyCount > 1)
            {
                return new DisagreementRecord
                {
                    TickId = _decisionTickId,
                    Timestamp = Time.GetTicksMsec() / 1000f,
                    Type = DisagreementType.ThreatIgnored,
                    PlayerAction = $"血量{assessment.PlayerHealthPercent:P0}继续进攻{assessment.NearbyEnemyCount}个敌人",
                    ObserverPrediction = "这个行动路线可能导致倒下",
                    QuestionPrompt = "你的血已经不多了，身边还有敌人...你真的想继续吗？",
                    Confidence = 0.8f
                };
            }

            // 2. MissedOpportunity: 有治疗道具但没血却不治疗
            if (assessment.PlayerHealthPercent < 0.3f && latest.ActionType == "attack" && goal.GoalType != "生存")
            {
                return new DisagreementRecord
                {
                    TickId = _decisionTickId,
                    Timestamp = Time.GetTicksMsec() / 1000f,
                    Type = DisagreementType.MissedOpportunity,
                    PlayerAction = "血量危急但选择继续战斗",
                    ObserverPrediction = "如果先治疗会更有利",
                    QuestionPrompt = "你的生命在流逝...有没有想过先稳住阵脚？",
                    Confidence = 0.75f
                };
            }

            // 3. ThreatIgnored: 威胁很高但忽视
            if (assessment.ThreatLevel == "危险" && latest.ActionType != "retreat" && latest.ActionType != "heal")
            {
                return new DisagreementRecord
                {
                    TickId = _decisionTickId,
                    Timestamp = Time.GetTicksMsec() / 1000f,
                    Type = DisagreementType.ThreatIgnored,
                    PlayerAction = $"在{assessment.ThreatLevel}威胁下{latest.ActionType}",
                    ObserverPrediction = "应该优先处理威胁或撤退",
                    QuestionPrompt = "我感觉到了危险的气息...你确定要这样走吗？",
                    Confidence = 0.7f
                };
            }

            // 4. ResourceMismatch: 资源与环境不匹配
            if (goal.GoalType == "进攻" && assessment.EnemyCount == 0)
            {
                return new DisagreementRecord
                {
                    TickId = _decisionTickId,
                    Timestamp = Time.GetTicksMsec() / 1000f,
                    Type = DisagreementType.MissedOpportunity,
                    PlayerAction = "仍在寻找战斗",
                    ObserverPrediction = "周围没有敌人，这个方向可能没有出口",
                    QuestionPrompt = "前面已经没有敌人了...也许该换个方向？",
                    Confidence = 0.6f
                };
            }

            return null;
        }

        /// <summary>
        /// 检查是否应该发起挑战
        /// </summary>
        private bool ShouldChallenge()
        {
            if (_observerState.IsDisabled)
                return false;
            if (_silenceTimer > 0)
                return false;
            if (_narrativeTimer > 0)
                return false;
            if (_observerState.CurrentAssessment.PlayerHealthPercent <= 0)
                return false;
            return true;
        }

        /// <summary>
        /// 生成并发射挑战（叙事化提问）
        /// </summary>
        private void GenerateAndEmitChallenge(DisagreementRecord disagreement)
        {
            // 生成唯一签名用于去重
            string sig = $"{disagreement.Type}_{disagreement.PlayerAction.GetHashCode()}";
            if (_observerState.PersistentState.IssuedChallengeSignatures.Contains(sig))
                return;

            _observerState.PersistentState.IssuedChallengeSignatures.Add(sig);
            if (_observerState.PersistentState.IssuedChallengeSignatures.Count > 50)
                _observerState.PersistentState.IssuedChallengeSignatures.Clear();  // 防止内存泄漏

            // 构建挑战
            var challenge = new ObserverChallenge
            {
                TickId = disagreement.TickId,
                Type = disagreement.Type,
                PlayerAction = disagreement.PlayerAction,
                ObserverPrediction = disagreement.ObserverPrediction,
                QuestionPrompt = disagreement.QuestionPrompt,
                Confidence = disagreement.Confidence
            };

            // 设置冷却
            _silenceTimer = _silenceCooldownDuration;
            _narrativeTimer = _narrativeCooldown;

            // 发射信号（UI 订阅并显示气泡）
            OnObserverChallenge?.Invoke(challenge);

            GD.PrintRich($"[AdversarialObserver] [color=yellow]挑战[/color]: {challenge.QuestionPrompt}");
        }

        /// <summary>
        /// Observer 承认自己可能错了
        /// </summary>
        private void EmitSelfCorrection()
        {
            var corrections = new[]
            {
                "我可能看错了...",
                "也许我的判断太武断了。",
                "你的路，也许是对的。",
                "我收回刚才的话。"
            };

            var random = new Random();
            string msg = corrections[random.Next(corrections.Length)];

            GD.PrintRich($"[AdversarialObserver] [color=gray]{msg}[/color]");
        }

        /// <summary>
        /// 验证上次的预测
        /// </summary>
        private void ValidateLastPrediction()
        {
            if (_currentTrajectory.PredictedTrajectory.Contains("可能倒下") || 
                _currentTrajectory.PredictedTrajectory.Contains("危险"))
            {
                _observerState.PersistentState.PredictionSuccessCount++;
            }
            else
            {
                _observerState.PersistentState.PredictionFailureCount++;
            }

            float total = _observerState.PersistentState.PredictionSuccessCount + _observerState.PersistentState.PredictionFailureCount;
            if (total > 0)
            {
                float accuracy = _observerState.PersistentState.PredictionSuccessCount / total;
                _observerState.PersistentState.Confidence = Mathf.Lerp(_observerState.PersistentState.Confidence, accuracy, 0.2f);
                OnPredictionVerified?.Invoke(_currentTrajectory.PredictedTrajectory.Contains("危险"));
            }
        }

        // ===== 辅助方法 =====

        private float GetPlayerHealthPercent()
        {
            if (_player == null)
                return 1.0f;

            // 尝试从 HealthComponent 或类似组件获取
            var healthNode = _player.GetNodeOrNull<Node>("HealthComponent");
            if (healthNode != null)
            {
                var currentProp = healthNode.GetType().GetProperty("CurrentHealth");
                var maxProp = healthNode.GetType().GetProperty("MaxHealth");
                if (currentProp != null && maxProp != null)
                {
                    float current = Convert.ToSingle(currentProp.GetValue(healthNode));
                    float max = Convert.ToSingle(maxProp.GetValue(healthNode));
                    if (max > 0)
                        return current / max;
                }
            }

            // 备选：从 Stats 或类似节点
            var stats = _player.GetNodeOrNull<Node>("Stats");
            if (stats != null)
            {
                var hpProp = stats.GetType().GetProperty("HP");
                var maxHpProp = stats.GetType().GetProperty("MaxHP");
                if (hpProp != null && maxHpProp != null)
                {
                    float hp = Convert.ToSingle(hpProp.GetValue(stats));
                    float maxHp = Convert.ToSingle(maxHpProp.GetValue(stats));
                    if (maxHp > 0)
                        return hp / maxHp;
                }
            }

            return 1.0f;  // 默认满血
        }

        private void LoadPersistentState()
        {
            // 从 MainSaveLoad 请求加载数据
            // 这会在游戏启动时由 SaveLoadSystem 调用 ImportSaveData
        }

    }
}
