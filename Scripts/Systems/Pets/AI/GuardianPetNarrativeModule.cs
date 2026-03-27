using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.Pets.AI;

namespace ClawRPG.Scripts.Systems.Pets.AI
{
    /// <summary>
    /// GuardianPetNarrativeModule — 叙事语言包装器（REQ-138-08）
    /// 职责：将 Observer 的发现转化为宠物的叙事语言
    /// 核心原则：提问 > 纠正，不用"你的HP很低"而是"我感觉到了危险"
    /// </summary>
    public class GuardianPetNarrativeModule
    {
        // ===== 宠物名字（可配置）=====
        
        private string _petName = "小家伙";
        
        // ===== 叙事模板 =====

        private readonly Dictionary<DisagreementType, List<string>> _narrativeTemplates = new Dictionary
        {
            [DisagreementType.ThreatIgnored] = new List<string>
            {
                "我有点担心...{0}",
                "这让我心里不安...{0}",
                "我能感觉到危险在靠近...{0}",
                "......{0}",
                "等等，我好像闻到了什么不对劲的味道。"
            },
            [DisagreementType.MissedOpportunity] = new List<string>
            {
                "嘿，{0}",
                "我有个想法...{0}",
                "不知道你有没有注意到...{0}",
                "有时候，退一步会看得更清楚。{0}",
                "你有没有想过...{0}"
            },
            [DisagreementType.GoalDrift] = new List<string>
            {
                "...{0}",
                "我想说点什么，但不知道该不该开口。{0}",
                "你的脚步...和以前不太一样了。{0}",
                "这是我的错觉吗...{0}",
                "我追随你很久了，你确定这是你想去的方向吗？"
            },
            [DisagreementType.ResourceMismatch] = new List<string>
            {
                "我有个疑问：{0}",
                "前方好像...不太对劲。{0}",
                "我能问问你为什么这样做吗？{0}",
                "这不是批评，只是...我想确认一下。{0}",
                "我注意到了一些奇怪的事情...{0}"
            }
        };

        private readonly List<string> _selfCorrectionLines = new List<string>
        {
            "我可能看错了...",
            "也许我的判断太武断了。",
            "你的路，也许是对的。",
            "我收回刚才的话。",
            "好吧，我承认这次我看走眼了。",
            "你比我更了解这片战场。"
        };

        private readonly List<string> _acknowledgmentLines = new List<string>
        {
            "明白了。",
            "好，我继续看着。",
            "我会留意的。",
            "......我懂了。",
            "嗯，我记住了。"
        };

        private readonly List<string> _quietObservationLines = new List<string>
        {
            "......",  // 沉默观察
            "我在看着。",
            "......嗯。",
            "......"   // 只有省略号，Observer 在安静地观察
        };

        // ===== 公开 API =====

        /// <summary>
        /// 设置宠物名字
        /// </summary>
        public void SetPetName(string name)
        {
            _petName = name;
        }

        /// <summary>
        /// 获取宠物名字
        /// </summary>
        public string GetPetName()
        {
            return _petName;
        }

        /// <summary>
        /// 将 ObserverChallenge 转化为叙事语言
        /// </summary>
        public string WrapChallenge(ObserverChallenge challenge)
        {
            if (challenge == null)
                return "";

            // 查找模板
            if (!_narrativeTemplates.TryGetValue(challenge.Type, out var templates))
                return challenge.QuestionPrompt;

            // 根据置信度选择：低置信度时更犹豫，高置信度时更直接
            string template;
            if (challenge.Confidence < 0.6f)
            {
                // 低置信度：加犹豫前缀
                var hesitantTemplates = new List<string>
                {
                    $"我不太确定，但是...{{0}}",
                    $"也许是我想多了，但是...{{0}}",
                    $"这可能只是我的直觉...{{0}}"
                };
                var hTemplate = hesitantTemplates[Math.Abs(challenge.QuestionPrompt.GetHashCode()) % hesitantTemplates.Count];
                return string.Format(hTemplate, challenge.QuestionPrompt);
            }
            else
            {
                // 正常置信度：直接使用模板
                template = templates[Math.Abs(challenge.QuestionPrompt.GetHashCode()) % templates.Count];
                return string.Format(template, challenge.QuestionPrompt);
            }
        }

        /// <summary>
        /// 生成 Observer 承认错误的叙事
        /// </summary>
        public string GetSelfCorrection()
        {
            var rng = new Random();
            return _selfCorrectionLines[rng.Next(_selfCorrectionLines.Count)];
        }

        /// <summary>
        /// 生成 Observer 确认收到了信息的叙事
        /// </summary>
        public string GetAcknowledgment()
        {
            var rng = new Random();
            return _acknowledgmentLines[rng.Next(_acknowledgmentLines.Count)];
        }

        /// <summary>
        /// 生成 Observer 安静观察的叙事（用于低置信度但仍需存在感）
        /// </summary>
        public string GetQuietObservation()
        {
            var rng = new Random();
            return _quietObservationLines[rng.Next(_quietObservationLines.Count)];
        }

        /// <summary>
        /// 构建完整的气泡文本（带宠物名）
        /// </summary>
        public string BuildBubbleText(string narrativeText)
        {
            return $"【{_petName}】{narrativeText}";
        }

        /// <summary>
        /// 叙事化描述世界评估（用于 UI 显示 Observer 的"视野"）
        /// </summary>
        public string DescribeWorldAssessment(WorldAssessment assessment)
        {
            if (assessment == null)
                return "我看不见周围的情况...";

            var lines = new List<string>();

            // 威胁描述
            switch (assessment.ThreatLevel)
            {
                case "危险":
                    lines.Add("四周弥漫着危险的气息...");
                    break;
                case "高":
                    lines.Add("前方有敌人出没。");
                    break;
                case "中":
                    lines.Add("周围还算平静。");
                    break;
                default:
                    lines.Add("这片区域很安静。");
                    break;
            }

            // 血量描述
            if (assessment.PlayerHealthPercent < 0.2f)
                lines.Add("你的气息很微弱...");
            else if (assessment.PlayerHealthPercent < 0.4f)
                lines.Add("你的身体在颤抖。");
            else if (assessment.PlayerHealthPercent < 0.6f)
                lines.Add("你有些疲惫。");
            else
                lines.Add("你的状态还不错。");

            // 敌人描述
            if (assessment.NearbyEnemyCount > 2)
                lines.Add($"附近有{assessment.NearbyEnemyCount}个敌人在徘徊。");
            else if (assessment.NearbyEnemyCount > 0)
                lines.Add("附近有敌人在窥视。");
            else
                lines.Add("眼前没有敌人。");

            return string.Join("\n", lines);
        }

        /// <summary>
        /// 叙事化描述目标推断（用于 UI 显示 Observer 看到的玩家目标）
        /// </summary>
        public string DescribeGoalInference(PlayerGoalInference goal)
        {
            if (goal == null || goal.Confidence < 0.3f)
                return "我还看不透你的意图...";

            string confidenceStr;
            if (goal.Confidence > 0.75f)
                confidenceStr = "我很确定";
            else if (goal.Confidence > 0.5f)
                confidenceStr = "我猜";
            else
                confidenceStr = "我隐约感觉";

            return $"{confidenceStr}...{goal.GoalDescription}";
        }

        /// <summary>
        /// 叙事化描述轨迹预测（用于 UI 显示 Observer 的预测）
        /// </summary>
        public string DescribeTrajectoryPrediction(TrajectoryPrediction trajectory)
        {
            if (trajectory == null || string.IsNullOrEmpty(trajectory.PredictedTrajectory))
                return "前方...我看不清。";

            return $"我的直觉告诉我：{trajectory.PredictedTrajectory}";
        }
    }
}
