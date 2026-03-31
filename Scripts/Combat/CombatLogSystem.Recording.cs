using System;
using System.Collections.Generic;
using Godot;
using Framework;
using ClawRPG.Scripts.Combat;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// CombatLogSystem — 所有 LogXXX 记录方法
    /// 每个方法：recorder.LogXxx() → AddEntry() → persistence.RecordXxx()
    /// </summary>
    public partial class CombatLogSystem
    {
        // ── 战斗事件 ────────────────────────────────────────────────────────

        public void LogDamage(float damage, string source, string target, bool isCritical = false, bool isPlayerSource = true)
        {
            var entry = _recorder.LogDamage(damage, source, target, isCritical, isPlayerSource);
            AddEntry(entry);
            _persistence.RecordDamage(damage, isCritical, isPlayerSource);
            if (isPlayerSource)
            {
                _persistence.AddCombo(1);
                CheckComboMilestone();
            }
        }

        public void LogHealing(float amount, string source, string target, bool isPlayerSource = true)
        {
            var entry = _recorder.LogHealing(amount, source, target, isPlayerSource);
            AddEntry(entry);
            _persistence.RecordHealing(amount);
        }

        public void LogMiss(string source, string target, string missType = "Miss", bool isPlayerSource = true)
        {
            var entry = _recorder.LogMiss(source, target, missType, isPlayerSource);
            AddEntry(entry);
            _persistence.RecordMiss();
        }

        public void LogBlock(string source, string target, float blockedDamage, bool isPlayerSource = true)
        {
            var entry = _recorder.LogBlock(source, target, blockedDamage, isPlayerSource);
            AddEntry(entry);
            _persistence.RecordBlock();
        }

        public void LogDodge(string source, string target, bool isPlayerSource = true)
        {
            var entry = _recorder.LogDodge(source, target, isPlayerSource);
            AddEntry(entry);
            _persistence.RecordDodge();
        }

        public void LogParry(string source, string target, bool isPlayerSource = true)
        {
            var entry = _recorder.LogParry(source, target, isPlayerSource);
            AddEntry(entry);
        }

        public void LogKill(string killer, string target, bool isPlayerKiller = true)
        {
            var entry = _recorder.LogKill(killer, target, isPlayerKiller);
            AddEntry(entry);
            _persistence.RecordKill();
            if (isPlayerKiller)
            {
                _persistence.AddKillStreak(target);
                CheckKillStreak();
            }
        }

        public void LogDeath(string target, string killer)
        {
            var entry = _recorder.LogDeath(target, killer);
            AddEntry(entry);
        }

        // ── Buff/Debuff ──────────────────────────────────────────────────────

        public void LogBuff(string target, string buffName, float duration, bool isPlayerTarget = true)
        {
            AddEntry(_recorder.LogBuff(target, buffName, duration, isPlayerTarget));
        }

        public void LogDebuff(string target, string debuffName, float duration, bool isPlayerTarget = true)
        {
            AddEntry(_recorder.LogDebuff(target, debuffName, duration, isPlayerTarget));
        }

        // ── 技能/物品 ────────────────────────────────────────────────────────

        public void LogSkill(string skillName, string user, string target = "", bool isPlayerUser = true)
        {
            AddEntry(_recorder.LogSkill(skillName, user, target, isPlayerUser));
        }

        public void LogItem(string itemName, string user, string effect = "", bool isPlayerUser = true)
        {
            AddEntry(_recorder.LogItem(itemName, user, effect, isPlayerUser));
        }

        // ── 资源/经验 ────────────────────────────────────────────────────────

        public void LogResource(string resourceType, float amount, string target, bool isGain = true)
        {
            AddEntry(_recorder.LogResource(resourceType, amount, target, isGain));
        }

        public void LogExperience(float amount, string target, string source = "战斗")
        {
            AddEntry(_recorder.LogExperience(amount, target, source));
        }

        public void LogLevelUp(string target, int newLevel)
        {
            AddEntry(_recorder.LogLevelUp(target, newLevel));
        }

        // ── 信息 ────────────────────────────────────────────────────────────

        public void LogInfo(string message, bool isPlayerAction = true)
        {
            AddEntry(_recorder.LogInfo(message, isPlayerAction));
        }

        public void LogWarning(string message, bool isPlayerAction = true)
        {
            AddEntry(_recorder.LogWarning(message, isPlayerAction));
        }

        public void LogEnemySpawn(string enemyName, int waveNumber)
        {
            AddEntry(_recorder.LogEnemySpawn(enemyName, waveNumber));
        }

        public void LogEnemyAggro(string enemyName, string target)
        {
            AddEntry(_recorder.LogEnemyAggro(enemyName, target));
        }
    }
}
