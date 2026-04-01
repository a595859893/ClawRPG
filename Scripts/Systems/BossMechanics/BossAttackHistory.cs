using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.BossMechanics {
    /// <summary>
    /// REQ-158: Boss 攻击历史热力图
    /// 记录每个 Boss 战斗实例最近 20 次攻击类型，支持 UI 热力图展示。
    /// </summary>
    
    /// <summary>
    /// 单次攻击记录
    /// </summary>
    public class BossAttackRecord {
        public BossSkillType AttackType { get; set; }
        public string AbilityName { get; set; }
        public int Damage { get; set; }
        public float RelativeTime { get; set; }

        public BossAttackRecord(BossSkillType type, string name, int damage, float relativeTime)
        {
            AttackType = type;
            AbilityName = name;
            Damage = damage;
            RelativeTime = relativeTime;
        }
    }

    /// <summary>
    /// REQ-158: Boss 攻击历史管理器（按战斗实例隔离）
    /// </summary>
    public static class BossAttackHistory {
        private const int MaxHistorySize = 20;

        private static readonly Dictionary<string, List<BossAttackRecord>> _histories = new Dictionary<string, List<BossAttackRecord>>();
        private static readonly Dictionary<string, float> _battleStartTimes = new Dictionary<string, float>();

        /// <summary>
        /// 开始追踪一场 Boss 战（历史清空）
        /// </summary>
        public static void BeginBattle(string instanceId, float battleStartTime) {
            _histories[instanceId] = new List<BossAttackRecord>();
            _battleStartTimes[instanceId] = battleStartTime;
        }

        /// <summary>
        /// 记录一次攻击
        /// </summary>
        public static void RecordAttack(string instanceId, BossSkillType type, string abilityName, int damage, float timestamp) {
            if (!_histories.TryGetValue(instanceId, out var history)) {
                history = new List<BossAttackRecord>();
                _histories[instanceId] = history;
            }

            float relativeTime = timestamp;
            if (_battleStartTimes.TryGetValue(instanceId, out var startTime)) {
                relativeTime = timestamp - startTime;
            }

            history.Add(new BossAttackRecord(type, abilityName, damage, relativeTime));

            // 固定窗口，新出旧弃
            while (history.Count > MaxHistorySize) {
                history.RemoveAt(0);
            }
        }

        /// <summary>
        /// 获取指定战斗实例的攻击历史（最近全部或指定数量）
        /// </summary>
        public static List<BossAttackRecord> GetHistory(string instanceId, int count = -1) {
            if (!_histories.TryGetValue(instanceId, out var history))
                return new List<BossAttackRecord>();

            if (count < 0 || count >= history.Count)
                return new List<BossAttackRecord>(history);

            return new List<BossAttackRecord>(history.GetRange(history.Count - count, count));
        }

        /// <summary>
        /// 清理指定战斗的历史（战斗结束时调用）
        /// </summary>
        public static void EndBattle(string instanceId) {
            _histories.Remove(instanceId);
            _battleStartTimes.Remove(instanceId);
        }
    }
}
