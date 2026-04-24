using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Characters;
using ClawRPG.Scripts.Systems.Pets;

namespace ClawRPG.Scripts.Systems.PetMimicry
{
    public partial class PetMimicrySkillSystem
    {
        // ══════════════════════════════════════════════════════════════
        // Skill Execution — 技能执行 + 效果系统
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// 执行技能效果
        /// </summary>
        private bool ExecuteSkill(MimicrySkillInstance skill)
        {
            if (!skill.TryUse()) return false;

            var def = skill.Definition;
            var targets = GetSkillTargets(def.SkillType);

            switch (def.SkillType)
            {
                case MimicrySkillType.FireBreath:
                case MimicrySkillType.FrostBreath:
                case MimicrySkillType.ElectricArc:
                case MimicrySkillType.ShadowTear:
                case MimicrySkillType.HolySmite:
                case MimicrySkillType.NatureBind:
                case MimicrySkillType.DashStrike:
                case MimicrySkillType.FrenzyStrike:
                case MimicrySkillType.EliteSlayer:
                case MimicrySkillType.SynergyFangs:
                    ApplyDamageSkill(def.SkillType, def.GetDamage(skill.ImprintLevel), targets);
                    break;

                case MimicrySkillType.IronBulwark:
                    ApplyShieldSkill(def.GetDuration(skill.ImprintLevel));
                    break;

                case MimicrySkillType.LastStand:
                    ApplyBuffSkill(MimicrySkillType.LastStand, def.GetDuration(skill.ImprintLevel));
                    break;

                case MimicrySkillType.Rearguard:
                    ApplyBuffSkill(MimicrySkillType.Rearguard, def.GetDuration(skill.ImprintLevel));
                    break;

                case MimicrySkillType.DodgeMaster:
                    ApplyBuffSkill(def.SkillType, def.GetDuration(skill.ImprintLevel));
                    break;

                case MimicrySkillType.HealingLight:
                    ApplyHealingSkill(def.GetDamage(skill.ImprintLevel));
                    break;

                case MimicrySkillType.TrapSense:
                    ApplyBuffSkill(def.SkillType, def.GetDuration(skill.ImprintLevel));
                    break;

                case MimicrySkillType.LootInstinct:
                case MimicrySkillType.PuzzleInsight:
                case MimicrySkillType.SpecialMorph:
                    ApplyBuffSkill(def.SkillType, def.GetDuration(skill.ImprintLevel));
                    break;
            }

            PlaySkillVFX(def.SkillType, targets);
            return true;
        }

        // ── Target Acquisition ─────────────────────────────────────────────

        /// <summary>
        /// 获取宠物的当前攻击目标（优先使用 PetCombatAI 的目标）
        /// </summary>
        private Node2D GetCurrentTarget()
        {
            try
            {
                if (PetCombatAI.Instance != null)
                {
                    var aiTarget = PetCombatAI.Instance.GetCurrentTarget();
                    if (aiTarget != null) return aiTarget;
                }
                return FindNearestEnemy();
            }
            catch
            {
                return FindNearestEnemy();
            }
        }

        /// <summary>
        /// 查找最近的敌人
        /// </summary>
        private Node2D FindNearestEnemy()
        {
            var petPos = GetPetPosition();
            Node2D nearest = null;
            float nearestDist = float.MaxValue;

            try
            {
                var enemies = GetTree().GetNodesInGroup("enemy");
                foreach (Node node in enemies)
                {
                    if (node is Node2D enemy)
                    {
                        float dist = petPos.DistanceTo(enemy.GlobalPosition);
                        if (dist < nearestDist && dist <= ENEMY_SCAN_RANGE)
                        {
                            nearestDist = dist;
                            nearest = enemy;
                        }
                    }
                }
            }
            catch { }

            return nearest;
        }

        /// <summary>
        /// 获取技能目标列表（用于AOE技能）
        /// </summary>
        private List<Node2D> GetSkillTargets(MimicrySkillType skillType)
        {
            var targets = new List<Node2D>();
            var primary = GetCurrentTarget();
            if (primary == null) return targets;

            targets.Add(primary);

            if (skillType == MimicrySkillType.ElectricArc ||
                skillType == MimicrySkillType.FireBreath ||
                skillType == MimicrySkillType.NatureBind)
            {
                var petPos = GetPetPosition();
                var enemies = GetTree().GetNodesInGroup("enemy");
                foreach (Node node in enemies)
                {
                    if (node is Node2D enemy && enemy != primary)
                    {
                        float dist = petPos.DistanceTo(enemy.GlobalPosition);
                        if (dist <= ENEMY_SCAN_RANGE && targets.Count < MAX_SKILL_TARGETS)
                        {
                            targets.Add(enemy);
                        }
                    }
                }
            }

            return targets;
        }

        /// <summary>
        /// 获取宠物节点的世界坐标
        /// </summary>
        private Vector2 GetPetPosition()
        {
            try
            {
                if (PetCombatAI.Instance != null)
                {
                    var node = PetCombatAI.Instance.GetPetNode();
                    if (node != null) return node.GlobalPosition;
                }
                var players = GetTree().GetNodesInGroup("player");
                foreach (Node node in players)
                {
                    if (node is Node2D p) return p.GlobalPosition;
                }
            }
            catch { }
            return Vector2.Zero;
        }

        /// <summary>
        /// 获取宠物当前HP百分比
        /// </summary>
        private float GetPetHpPercent()
        {
            try
            {
                if (PetCombatAI.Instance != null)
                    return PetCombatAI.Instance.GetPetHpPercent();
            }
            catch { }
            return 1f;
        }

        /// <summary>
        /// 获取主人当前HP百分比
        /// </summary>
        private float GetOwnerHpPercent()
        {
            try
            {
                var players = GetTree().GetNodesInGroup("player");
                foreach (Node node in players)
                {
                    if (node is Character ch)
                    {
                        float maxHp = ch.MaxHealth;
                        if (maxHp > 0f)
                            return ch.Health / maxHp;
                    }
                }
            }
            catch { }
            return 1f;
        }

        // ── Skill Effects ───────────────────────────────────────────────────

        /// <summary>
        /// 应用伤害技能
        /// </summary>
        private void ApplyDamageSkill(MimicrySkillType skillType, float damage, List<Node2D> targets)
        {
            string dmgType = GetDamageType(skillType);

            foreach (var target in targets)
            {
                if (target == null) continue;

                var enemy = target as Enemy;
                if (enemy != null)
                {
                    int finalDamage = CalculateFinalDamage(damage, skillType, target);
                    enemy.TakeDamage(finalDamage);
                }

                GD.Print($"[PetMimicrySkillSystem] {skillType} dealt {damage:F0} ({dmgType}) to {(target.Name ?? "enemy")}");
            }

            if (PetCombatCompanionSystem.Instance != null)
            {
                PetCombatCompanionSystem.Instance.SynergyAttackTriggered?.Invoke(
                    "mimicry", skillType.ToString(), 1.0f);
            }
        }

        /// <summary>
        /// 计算最终伤害（含Buff加成）
        /// </summary>
        private int CalculateFinalDamage(float baseDamage, MimicrySkillType skillType, Node2D target)
        {
            float multiplier = baseDamage;

            float fidelity = GetCurrentFidelity();
            if (fidelity < 0.4f)
                multiplier *= 0.7f;
            else if (fidelity >= 0.7f)
                multiplier *= 1.0f;
            else
                multiplier *= (0.7f + (fidelity - 0.4f) / 0.3f * 0.3f);

            if (_activeSkillEffects.ContainsKey(MimicrySkillType.LastStand))
                multiplier *= 1.5f;

            if (PetCombatCompanionSystem.Instance != null)
            {
                float syncLevel = PetCombatCompanionSystem.Instance.GetCurrentSyncLevel();
                multiplier *= (1f + syncLevel * 0.1f);
            }

            return Mathf.RoundToInt(multiplier);
        }

        /// <summary>
        /// 获取当前技能的 fidelity 值
        /// </summary>
        private float GetCurrentFidelity()
        {
            if (_mimicryData == null) return 0.5f;
            float bestFidelity = 0.5f;
            foreach (var imprint in _mimicryData.GetAllImprints())
            {
                if (imprint.Fidelity > bestFidelity)
                    bestFidelity = imprint.Fidelity;
            }
            return bestFidelity;
        }

        /// <summary>
        /// 将技能类型映射为伤害类型字符串
        /// </summary>
        private string GetDamageType(MimicrySkillType skillType)
        {
            return skillType switch
            {
                MimicrySkillType.FireBreath => "fire",
                MimicrySkillType.FrostBreath => "ice",
                MimicrySkillType.ElectricArc => "electric",
                MimicrySkillType.ShadowTear => "shadow",
                MimicrySkillType.HolySmite => "holy",
                MimicrySkillType.NatureBind => "nature",
                MimicrySkillType.DashStrike => "physical",
                MimicrySkillType.FrenzyStrike => "physical",
                MimicrySkillType.EliteSlayer => "physical",
                MimicrySkillType.SynergyFangs => "physical",
                _ => "physical"
            };
        }

        /// <summary>
        /// 应用护盾技能
        /// </summary>
        private void ApplyShieldSkill(float duration)
        {
            float shieldAmount = 50f;
            try
            {
                if (PetCombatAI.Instance != null)
                    shieldAmount = PetCombatAI.Instance.GetPetMaxHp() * 0.2f;
            }
            catch { }

            _activeSkillEffects[MimicrySkillType.IronBulwark] = duration;
            GD.Print($"[PetMimicrySkillSystem] IronBulwark: +{shieldAmount:F0} shield for {duration:F1}s");
        }

        /// <summary>
        /// 应用Buff技能（持续性效果）
        /// </summary>
        private void ApplyBuffSkill(MimicrySkillType skillType, float duration)
        {
            _activeSkillEffects[skillType] = duration;
            GD.Print($"[PetMimicrySkillSystem] Buff {skillType} activated for {duration:F1}s");
        }

        /// <summary>
        /// 应用治疗技能
        /// </summary>
        private void ApplyHealingSkill(float healAmount)
        {
            float heal = Mathf.Abs(healAmount);
            try
            {
                if (PetCombatAI.Instance != null)
                    PetCombatAI.Instance.HealPet(Mathf.RoundToInt(heal));
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[PetMimicrySkillSystem] Failed to heal pet: {ex.Message}");
            }
        }

        // ── Visual Effects ─────────────────────────────────────────────────

        /// <summary>
        /// 播放技能视觉特效
        /// </summary>
        private void PlaySkillVFX(MimicrySkillType skillType, List<Node2D> targets)
        {
            if (targets.Count > 0 && targets[0] != null)
            {
                if (PetCombatAI.Instance != null)
                    PetCombatAI.Instance.EmitPetAttackedSignal(targets[0], 0);
            }

            EmitSignal(SignalName.MimicrySkillUsed,
                GetSkillBehaviorType(skillType),
                skillType,
                GetPetPosition());
        }

        /// <summary>
        /// 获取技能对应的行为类型
        /// </summary>
        private PlayerBehaviorType GetSkillBehaviorType(MimicrySkillType skillType)
        {
            if (_database == null) return PlayerBehaviorType.AggressiveAttack;
            var def = _database.GetSkill(skillType);
            return def?.SourceBehavior ?? PlayerBehaviorType.AggressiveAttack;
        }
    }
}
