using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Pets.AI
{
    public partial class AdversarialObserverSystem
    {
        // ===== 挑战生成 =====

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
