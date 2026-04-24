using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Systems.Pets.AI;
using ClawRPG.Scripts.Systems.ComboReplay;

namespace ClawRPG.Systems.Pets.AI
{
    public partial class AdversarialObserverSystem
    {
        // ===== 事件处理 =====

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
            if (_currentTrajectory.PredictedTrajectory.ContainsKey("死亡"))
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

        // ===== 初始化 =====

        private void InitializeBubbleUI()
        {
            // 尝试在 CanvasLayer 下创建 ObserverBubbleUI
            var tree = GetTree();
            if (tree == null) return;

            // 查找 CanvasLayer
            Node canvasLayer = null;
            var root = tree.GetRoot();
            if (root != null)
            {
                canvasLayer = root.FindChild("CanvasLayer", true, false);
            }

            if (canvasLayer == null)
            {
                // 如果没有 CanvasLayer，尝试在 Main 下
                var main = tree.CurrentScene;
                if (main != null)
                {
                    canvasLayer = main.FindChild("CanvasLayer", true, false);
                }
            }

            if (canvasLayer == null)
            {
                GD.Print("[AdversarialObserverSystem] CanvasLayer not found, ObserverBubbleUI will not be shown");
                return;
            }

            // 创建 ObserverBubbleUI
            var bubbleUI = new ObserverBubbleUI { Name = "ObserverBubbleUI" };
            bubbleUI.SetObserverPetName(GetPetNameFromPetSystem());
            canvasLayer.AddChild(bubbleUI);

            GD.Print("[AdversarialObserverSystem] ObserverBubbleUI instantiated");
        }

        private string GetPetNameFromPetSystem()
        {
            // 尝试从 PetCombatCompanionSystem 获取宠物名
            var companion = PetCombatCompanionSystem.Instance;
            if (companion != null)
            {
                var activePet = companion.GetActivePetId();
                if (!string.IsNullOrEmpty(activePet))
                {
                    // 尝试从宠物数据库获取名字
                    // 这里返回默认名字，实际可以从 PetStoryDatabase 获取
                }
            }
            return "小家伙";
        }

        // ===== 观察循环 =====

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

        // ===== Step 1: 世界评估 =====

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

        // ===== Step 2: 目标推断 =====

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
        private void UpdateGoalInferenceIncremental(ClawRPG.Scripts.Systems.Pets.AI.PlayerActionRecord newRecord)
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

        // ===== Step 3: 轨迹预测 =====

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

        // ===== Step 4: 分歧检测 =====

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
    }
}
