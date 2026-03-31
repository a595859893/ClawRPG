using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Systems.PetMimicry
{
    /// <summary>
    /// PetMimicryData — 条件触发性格分析
    /// </summary>
    public partial class PetMimicryData
    {
        /// <summary>
        /// REQ-149: 获取宠物的扩展性格分析结果
        /// 结合条件触发器（HP状态、环境专精、事件驱动等）加权计算
        /// </summary>
        public PersonalityAnalysisResult GetDominantBehaviorEx()
        {
            var result = new PersonalityAnalysisResult();

            var scores = new Dictionary<PlayerBehaviorType, float>();
            foreach (PlayerBehaviorType behavior in Enum.GetValues(typeof(PlayerBehaviorType)))
            {
                int level = GetHighestLevel(behavior);
                scores[behavior] = level * 1.0f;
            }

            EvaluateHpTriggers(scores, result.ActiveTriggers);
            EvaluateEnvironmentTriggers(scores, result.ActiveTriggers);
            EvaluateEventDrivenTriggers(scores, result.ActiveTriggers);
            EvaluateRecentBias(scores, result.ActiveTriggers);
            EvaluateSuppressedTriggers(scores, result.ActiveTriggers);

            float maxScore = 0f;
            PlayerBehaviorType? dominant = null;
            foreach (var kvp in scores)
            {
                if (kvp.Value > maxScore)
                {
                    maxScore = kvp.Value;
                    dominant = kvp.Key;
                }
            }

            result.DominantBehavior = dominant;
            result.DominantScore = maxScore;
            result.AllScores = scores;
            result.Description = BuildPersonalityDescription(result);
            return result;
        }

        private void EvaluateHpTriggers(Dictionary<PlayerBehaviorType, float> scores, List<PersonalityTrigger> triggers)
        {
            if (_currentHpPercent <= HP_LOW_THRESHOLD)
            {
                float intensity = 1f - (_currentHpPercent / HP_LOW_THRESHOLD);

                if (_currentHpPercent <= HP_CRITICAL_THRESHOLD)
                {
                    float bonus = HP_TRIGGER_WEIGHT * intensity * 2f;
                    AddTriggerScore(scores, PlayerBehaviorType.LowHPAggression, bonus, triggers,
                        new PersonalityTrigger(PersonalityTriggerType.HPRelated, PlayerBehaviorType.LowHPAggression,
                            bonus, true, $"HP危急({_currentHpPercent:P0})"));
                }

                float cautionBonus = HP_TRIGGER_WEIGHT * intensity;
                AddTriggerScore(scores, PlayerBehaviorType.QuickRetreat, cautionBonus, triggers,
                    new PersonalityTrigger(PersonalityTriggerType.HPRelated, PlayerBehaviorType.QuickRetreat,
                        cautionBonus, true, $"HP低({_currentHpPercent:P0})"));
                AddTriggerScore(scores, PlayerBehaviorType.DefensiveStance, cautionBonus * 0.7f, triggers,
                    new PersonalityTrigger(PersonalityTriggerType.HPRelated, PlayerBehaviorType.DefensiveStance,
                        cautionBonus * 0.7f, true, $"HP低({_currentHpPercent:P0})"));
            }
        }

        private void EvaluateEnvironmentTriggers(Dictionary<PlayerBehaviorType, float> scores, List<PersonalityTrigger> triggers)
        {
            if (_currentEnvironment == RoomEnvironmentType.None) return;

            var envImprints = GetImprintsForEnvironment(_currentEnvironment);
            foreach (var imprint in envImprints)
            {
                if (imprint.ImprintLevel > 0)
                {
                    float bonus = ENV_TRIGGER_WEIGHT * (imprint.ImprintLevel / 5f);
                    AddTriggerScore(scores, imprint.BehaviorType, bonus, triggers,
                        new PersonalityTrigger(PersonalityTriggerType.EnvironmentSpecialist, imprint.BehaviorType,
                            bonus, true, $"当前环境专精(Lv.{imprint.ImprintLevel})"));
                }
            }

            if (_currentEnvironment.HasFlag(RoomEnvironmentType.Boss))
            {
                float bossBonus = ENV_TRIGGER_WEIGHT * 1.2f;
                AddTriggerScore(scores, PlayerBehaviorType.FocusElite, bossBonus, triggers,
                    new PersonalityTrigger(PersonalityTriggerType.EnvironmentSpecialist, PlayerBehaviorType.FocusElite,
                        bossBonus, true, "Boss房间"));
            }

            if (_currentEnvironment.HasFlag(RoomEnvironmentType.Treasure))
            {
                float treasureBonus = ENV_TRIGGER_WEIGHT * 1.0f;
                AddTriggerScore(scores, PlayerBehaviorType.CollectLoot, treasureBonus, triggers,
                    new PersonalityTrigger(PersonalityTriggerType.EnvironmentSpecialist, PlayerBehaviorType.CollectLoot,
                        treasureBonus, true, "宝藏房间"));
            }

            if (_currentEnvironment.HasFlag(RoomEnvironmentType.TrapDense))
            {
                float trapBonus = ENV_TRIGGER_WEIGHT * 0.8f;
                AddTriggerScore(scores, PlayerBehaviorType.AvoidCombat, trapBonus, triggers,
                    new PersonalityTrigger(PersonalityTriggerType.EnvironmentSpecialist, PlayerBehaviorType.AvoidCombat,
                        trapBonus, true, "陷阱密集区"));
            }
        }

        private void EvaluateEventDrivenTriggers(Dictionary<PlayerBehaviorType, float> scores, List<PersonalityTrigger> triggers)
        {
            foreach (var kvp in _eventDrivenBonus)
            {
                if (kvp.Value > 0.01f)
                {
                    AddTriggerScore(scores, kvp.Key, kvp.Value * EVENT_TRIGGER_WEIGHT, triggers,
                        new PersonalityTrigger(PersonalityTriggerType.EventDriven, kvp.Key,
                            kvp.Value * EVENT_TRIGGER_WEIGHT, true, $"事件加成({kvp.Value:F1})"));
                }
            }
        }

        private void EvaluateRecentBias(Dictionary<PlayerBehaviorType, float> scores, List<PersonalityTrigger> triggers)
        {
            if (_mostRecentRecordTime == DateTime.MinValue) return;

            TimeSpan elapsed = DateTime.Now - _mostRecentRecordTime;
            if (elapsed.TotalSeconds > 300) return;

            BehaviorImprint recent = null;
            foreach (var imprint in _imprints)
            {
                if (recent == null || imprint.LastRecordedAt > recent.LastRecordedAt)
                    recent = imprint;
            }

            if (recent != null && recent.ImprintLevel > 0)
            {
                float recencyFactor = Mathf.Max(0f, 1f - (float)elapsed.TotalSeconds / 300f);
                float bonus = RECENT_TRIGGER_WEIGHT * recencyFactor * (recent.ImprintLevel / 5f);
                AddTriggerScore(scores, recent.BehaviorType, bonus, triggers,
                    new PersonalityTrigger(PersonalityTriggerType.RecentBias, recent.BehaviorType,
                        bonus, true, $"近期行为({elapsed.TotalSeconds:F0}s前)"));
            }
        }

        private void EvaluateSuppressedTriggers(Dictionary<PlayerBehaviorType, float> scores, List<PersonalityTrigger> triggers)
        {
            foreach (var imprint in _imprints)
            {
                if (imprint.ImprintLevel > 0 && imprint.LastRecordedAt != default)
                {
                    TimeSpan elapsed = DateTime.Now - imprint.LastRecordedAt;
                    if (elapsed.TotalSeconds > SUPPRESSION_THRESHOLD_SECONDS)
                    {
                        float suppressionFactor = Mathf.Min(1f, (float)(elapsed.TotalSeconds - SUPPRESSION_THRESHOLD_SECONDS) / 120f);
                        float penalty = SUPPRESSED_TRIGGER_WEIGHT * suppressionFactor * imprint.ImprintLevel;
                        AddTriggerScore(scores, imprint.BehaviorType, penalty, triggers,
                            new PersonalityTrigger(PersonalityTriggerType.Suppressed, imprint.BehaviorType,
                                penalty, true, $"久未使用({elapsed.TotalMinutes:F0}min)"));
                    }
                }
            }
        }

        private void AddTriggerScore(Dictionary<PlayerBehaviorType, float> scores, PlayerBehaviorType behavior, float delta, List<PersonalityTrigger> triggers, PersonalityTrigger trigger)
        {
            if (!scores.ContainsKey(behavior)) scores[behavior] = 0f;
            scores[behavior] += delta;
            if (trigger.IsActive) triggers.Add(trigger);
        }

        private string BuildPersonalityDescription(PersonalityAnalysisResult result)
        {
            if (result.DominantBehavior == null) return "无记录";

            var parts = new List<string>();
            parts.Add($"核心性格: {GetBehaviorDisplayName(result.DominantBehavior.Value)}");

            var sortedTriggers = result.ActiveTriggers.FindAll(t => t.IsActive && t.Weight > 0.1f);
            sortedTriggers.Sort((a, b) => b.Weight.CompareTo(a.Weight));

            if (sortedTriggers.Count > 0)
            {
                var activeReasons = new List<string>();
                foreach (var t in sortedTriggers.Take(3))
                {
                    if (!string.IsNullOrEmpty(t.Reason))
                        activeReasons.Add($"{GetBehaviorDisplayName(t.Behavior)}↑({t.Reason})");
                }
                if (activeReasons.Count > 0)
                    parts.Add("触发中: " + string.Join(", ", activeReasons));
            }

            return string.Join(" | ", parts);
        }
    }
}
