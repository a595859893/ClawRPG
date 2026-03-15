using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Combat {
    /// <summary>
    /// 战斗日志记录器 - 负责记录战斗事件
    /// </summary>
    public partial class CombatLogRecorder : BaseSystem {
        
        private List<CombatLogEntry> _logEntries = new();
        private int _maxEntries = 1000;
        
        public override void _Ready() {
            base._Ready();
        }
        
        /// <summary>
        /// 记录日志
        /// </summary>
        public void RecordLog(CombatLogType type, string message, Dictionary data = null) {
            var entry = new CombatLogEntry {
                Type = type,
                Message = message,
                Timestamp = Time.GetUnixTimeFromSystem(),
                Data = data ?? new Dictionary()
            };
            
            _logEntries.Add(entry);
            
            // 限制日志数量
            if (_logEntries.Count > _maxEntries) {
                _logEntries.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// 记录伤害
        /// </summary>
        public void RecordDamage(int sourceId, int targetId, int damage, string damageType) {
            RecordLog(CombatLogType.Damage, $"Deal {damage} {damageType} damage", new Dictionary {
                { "sourceId", sourceId },
                { "targetId", targetId },
                { "damage", damage },
                { "damageType", damageType }
            });
        }
        
        /// <summary>
        /// 记录治疗
        /// </summary>
        public void RecordHeal(int targetId, int amount) {
            RecordLog(CombatLogType.Heal, $"Heal {amount} HP", new Dictionary {
                { "targetId", targetId },
                { "amount", amount }
            });
        }
        
        /// <summary>
        /// 记录技能使用
        /// </summary>
        public void RecordSkillUse(int casterId, string skillId) {
            RecordLog(CombatLogType.Skill, $"Use skill {skillId}", new Dictionary {
                { "casterId", casterId },
                { "skillId", skillId }
            });
        }
        
        /// <summary>
        /// 记录死亡
        /// </summary>
        public void RecordDeath(int entityId) {
            RecordLog(CombatLogType.Death, $"Entity {entityId} died", new Dictionary {
                { "entityId", entityId }
            });
        }
        
        /// <summary>
        /// 获取所有日志
        /// </summary>
        public List<CombatLogEntry> GetAllLogs() {
            return new List<CombatLogEntry>(_logEntries);
        }
        
        /// <summary>
        /// 获取最近N条日志
        /// </summary>
        public List<CombatLogEntry> GetRecentLogs(int count) {
            var start = Math.Max(0, _logEntries.Count - count);
            var result = new List<CombatLogEntry>();
            
            for (int i = start; i < _logEntries.Count; i++) {
                result.Add(_logEntries[i]);
            }
            
            return result;
        }
        
        /// <summary>
        /// 清空日志
        /// </summary>
        public void ClearLogs() {
            _logEntries.Clear();
        }
        
        public override Dictionary ExportSaveData() {
            var data = new Dictionary();
            data["maxEntries"] = _maxEntries;
            return data;
        }
        
        public override void ImportSaveData(Dictionary data) {
            if (data.Contains("maxEntries")) {
                _maxEntries = (int)data["maxEntries"];
            }
        }
    }
}
