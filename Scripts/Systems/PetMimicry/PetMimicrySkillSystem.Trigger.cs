using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Systems.ProceduralDungeon;

namespace ClawRPG.Scripts.Systems.PetMimicry
{
    public partial class PetMimicrySkillSystem
    {
        // ══════════════════════════════════════════════════════════════
        // Trigger Evaluation — 触发条件评估 + 互斥组 + 环境检测
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// 获取当前环境最佳技能实例
        /// </summary>
        private MimicrySkillInstance GetBestSkillForCurrentEnvironment()
        {
            if (_database == null || _mimicryData == null) return null;

            var currentEnv = GetCurrentEnvironment();
            var def = _database.GetBestSkillForEnvironment(currentEnv, _mimicryData);
            if (def == null) return null;

            if (_skillInstances.TryGetValue(def.SourceBehavior, out var instance))
                return instance;
            return null;
        }

        /// <summary>
        /// 获取当前环境的房间类型
        /// </summary>
        private RoomEnvironmentType GetCurrentEnvironment()
        {
            try
            {
                var dungeon = ProceduralDungeonSystem.Instance?.CurrentDungeon;
                var room = dungeon?.CurrentRoom;
                return RoomEnvironmentClassifier.Classify(room);
            }
            catch
            {
                return RoomEnvironmentType.None;
            }
        }

        /// <summary>
        /// REQ-146: 评估所有技能的触发条件，返回满足条件的技能列表
        /// </summary>
        private List<MimicrySkillInstance> EvaluateTriggerConditions(RoomEnvironmentType currentEnv)
        {
            var candidates = new List<MimicrySkillInstance>();
            var petHp = GetPetHpPercent();
            var ownerHp = GetOwnerHpPercent();
            var nearbyEnemy = GetCurrentTarget();
            float timeSinceOwnerDamage = Time.GetTicksMsec() / 1000f - _lastOwnerDamageTime;

            foreach (var kvp in _skillInstances)
            {
                var inst = kvp.Value;
                var def = inst.Definition;
                var trigger = def.TriggerConfig;

                if (!inst.IsReady) continue;

                if (!string.IsNullOrEmpty(trigger.MutexGroup) && _activeMutexGroups.Contains(trigger.MutexGroup))
                    continue;

                if (_activeSkillEffects.ContainsKey(def.SkillType) &&
                    (def.SkillType == MimicrySkillType.DodgeMaster ||
                     def.SkillType == MimicrySkillType.LootInstinct ||
                     def.SkillType == MimicrySkillType.PuzzleInsight ||
                     def.SkillType == MimicrySkillType.SpecialMorph))
                    continue;

                bool triggered = false;
                switch (trigger.Trigger)
                {
                    case MimicryTriggerType.HpBelowThreshold:
                        triggered = petHp < trigger.Threshold;
                        break;

                    case MimicryTriggerType.OnOwnerDamaged:
                        triggered = timeSinceOwnerDamage < OWNER_DAMAGE_COOLDOWN;
                        break;

                    case MimicryTriggerType.OnEnemyNearby:
                        triggered = nearbyEnemy != null && IsEnemyInRange(nearbyEnemy, trigger.Range);
                        break;

                    case MimicryTriggerType.OnOwnerAttacking:
                        triggered = _ownerAttackCooldown > 0f && nearbyEnemy != null;
                        break;

                    case MimicryTriggerType.OnEnvironmentMatch:
                        triggered = (currentEnv & trigger.EnvironmentType) == trigger.EnvironmentType;
                        break;

                    case MimicryTriggerType.CooldownBased:
                        triggered = true;
                        break;

                    case MimicryTriggerType.ManualToggle:
                        triggered = false;
                        break;

                    case MimicryTriggerType.None:
                    default:
                        triggered = false;
                        break;
                }

                if (triggered)
                    candidates.Add(inst);
            }

            return candidates;
        }

        /// <summary>
        /// REQ-146: 从候选技能中选择最佳技能（最高优先级，互斥组去重）
        /// </summary>
        private MimicrySkillInstance SelectBestSkill(List<MimicrySkillInstance> candidates)
        {
            if (candidates.Count == 0) return null;
            if (candidates.Count == 1) return candidates[0];

            candidates.Sort((a, b) =>
                b.Definition.TriggerConfig.Priority.CompareTo(a.Definition.TriggerConfig.Priority));

            return candidates[0];
        }

        /// <summary>
        /// 检查敌人是否在指定范围内
        /// </summary>
        private bool IsEnemyInRange(Node2D enemy, float range)
        {
            if (enemy == null) return false;
            var petPos = GetPetPosition();
            return petPos.DistanceTo(enemy.GlobalPosition) <= range;
        }

        // ══════════════════════════════════════════════════════════════
        // Legacy trigger evaluation (kept for compatibility)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// 判断是否应该使用该技能（REX-146前向兼容）
        /// </summary>
        private bool ShouldUseSkill(MimicrySkillInstance skill, RoomEnvironmentType env)
        {
            var def = skill.Definition;

            switch (def.SkillType)
            {
                case MimicrySkillType.DodgeMaster:
                case MimicrySkillType.LootInstinct:
                case MimicrySkillType.PuzzleInsight:
                    if (_activeSkillEffects.ContainsKey(def.SkillType)) return false;
                    break;
            }

            var target = GetCurrentTarget();
            if (def.BaseDamage > 0f && target == null) return false;

            if (def.SkillType == MimicrySkillType.LastStand)
            {
                float hpPercent = GetPetHpPercent();
                if (hpPercent > 0.3f) return false;
            }

            return true;
        }
    }
}
