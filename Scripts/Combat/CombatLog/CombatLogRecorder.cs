using System;
using System.Collections.Generic;
using Godot;
using Framework;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// CombatLogRecorder - 战斗日志记录器
    /// 负责记录各种战斗事件：伤害、治疗、击杀、技能等
    /// </summary>
    public partial class CombatLogRecorder : BaseSystem
    {
        private static CombatLogRecorder _instance;
        public static CombatLogRecorder Instance => _instance;

        // 日志存储
        private List<CombatLogEntry> _logEntries = new List<CombatLogEntry>();
        
        // 配置
        private int _maxEntries = 500;
        private float _currentSessionTime = 0f;
        
        // Signals
        public static string SignalNewEntry = "new_combat_log_entry_recorder";

        protected override void Initialize()
        {
            _instance = this;
            GD.Print("[CombatLogRecorder] Initialized");
        }

        public override void _Process(double delta)
        {
            _currentSessionTime += delta;
        }

        #region Log Methods - 委托给 CombatLogSystem

        /// <summary>
        /// Log a damage event
        /// </summary>
        public CombatLogEntry LogDamage(float damage, string source, string target, bool isCritical = false, bool isPlayerSource = true)
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = isCritical ? CombatLogType.Critical : CombatLogType.Damage,
                Message = isCritical ? $"暴击! {source} 对 {target} 造成 {damage:F0} 伤害" : $"{source} 对 {target} 造成 {damage:F0} 伤害",
                Value = damage,
                Source = source,
                Target = target,
                IsPlayerAction = isPlayerSource
            };

            AddEntry(entry);
            return entry;
        }

        /// <summary>
        /// Log a healing event
        /// </summary>
        public CombatLogEntry LogHealing(float amount, string source, string target, bool isPlayerSource = true)
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.Healing,
                Message = $"{source} 为 {target} 恢复 {amount:F0} 生命",
                Value = amount,
                Source = source,
                Target = target,
                IsPlayerAction = isPlayerSource
            };

            AddEntry(entry);
            return entry;
        }

        /// <summary>
        /// Log a miss event
        /// </summary>
        public CombatLogEntry LogMiss(string source, string target, string missType = "Miss", bool isPlayerSource = true)
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.Miss,
                Message = $"{source} 的攻击未命中 {target}",
                Source = source,
                Target = target,
                IsPlayerAction = isPlayerSource
            };

            AddEntry(entry);
            return entry;
        }

        /// <summary>
        /// Log a block event
        /// </summary>
        public CombatLogEntry LogBlock(string source, string target, float blockedDamage, bool isPlayerSource = true)
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.Block,
                Message = $"{source} 格挡了 {target} 的 {blockedDamage:F0} 伤害",
                Value = blockedDamage,
                Source = source,
                Target = target,
                IsPlayerAction = isPlayerSource
            };

            AddEntry(entry);
            return entry;
        }

        /// <summary>
        /// Log a dodge event
        /// </summary>
        public CombatLogEntry LogDodge(string source, string target, bool isPlayerSource = true)
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.Dodge,
                Message = $"{target} 闪避了 {source} 的攻击",
                Source = source,
                Target = target,
                IsPlayerAction = isPlayerSource
            };

            AddEntry(entry);
            return entry;
        }

        /// <summary>
        /// Log a parry event
        /// </summary>
        public CombatLogEntry LogParry(string source, string target, bool isPlayerSource = true)
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.Parry,
                Message = $"{source} 招架了 {target} 的攻击",
                Source = source,
                Target = target,
                IsPlayerAction = isPlayerSource
            };

            AddEntry(entry);
            return entry;
        }

        /// <summary>
        /// Log a kill event
        /// </summary>
        public CombatLogEntry LogKill(string killer, string target, bool isPlayerKiller = true)
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.Kill,
                Message = isPlayerKiller ? $"☠️ 击杀 {target}!" : $"你被 {target} 击败",
                Source = killer,
                Target = target,
                IsPlayerAction = isPlayerKiller
            };

            AddEntry(entry);
            return entry;
        }

        /// <summary>
        /// Log a death event
        /// </summary>
        public CombatLogEntry LogDeath(string target, string killer)
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.Death,
                Message = $"💀 {target} 被击败",
                Source = killer,
                Target = target,
                IsPlayerAction = false
            };

            AddEntry(entry);
            return entry;
        }

        /// <summary>
        /// Log a buff application
        /// </summary>
        public CombatLogEntry LogBuff(string target, string buffName, float duration, bool isPlayerTarget = true)
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.Buff,
                Message = $"✨ {target} 获得 buff: {buffName} ({duration:F1}秒)",
                Source = buffName,
                Target = target,
                IsPlayerAction = isPlayerTarget
            };

            AddEntry(entry);
            return entry;
        }

        /// <summary>
        /// Log a debuff application
        /// </summary>
        public CombatLogEntry LogDebuff(string target, string debuffName, float duration, bool isPlayerTarget = true)
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.Debuff,
                Message = $"⛔ {target} 受到 debuff: {debuffName} ({duration:F1}秒)",
                Source = debuffName,
                Target = target,
                IsPlayerAction = isPlayerTarget
            };

            AddEntry(entry);
            return entry;
        }

        /// <summary>
        /// Log a skill use
        /// </summary>
        public CombatLogEntry LogSkill(string skillName, string user, string target = "", bool isPlayerUser = true)
        {
            var message = string.IsNullOrEmpty(target)
                ? $"⚔️ {user} 使用 {skillName}"
                : $"⚔️ {user} 使用 {skillName} 对 {target}";

            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.SkillUsed,
                Message = message,
                Source = user,
                Target = target,
                IsPlayerAction = isPlayerUser
            };

            AddEntry(entry);
            return entry;
        }

        /// <summary>
        /// Log an item use
        /// </summary>
        public CombatLogEntry LogItem(string itemName, string user, string effect = "", bool isPlayerUser = true)
        {
            var message = string.IsNullOrEmpty(effect)
                ? $"🎒 {user} 使用 {itemName}"
                : $"🎒 {user} 使用 {itemName} - {effect}";

            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.ItemUsed,
                Message = message,
                Source = user,
                Target = effect,
                IsPlayerAction = isPlayerUser
            };

            AddEntry(entry);
            return entry;
        }

        /// <summary>
        /// Log mana/energy change
        /// </summary>
        public CombatLogEntry LogResource(string resourceType, float amount, string target, bool isGain = true)
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.Mana,
                Message = isGain
                    ? $"💎 {target} 恢复 {amount:F0} {resourceType}"
                    : $"💎 {target} 消耗 {amount:F0} {resourceType}",
                Value = amount,
                Source = resourceType,
                Target = target,
                IsPlayerAction = true
            };

            AddEntry(entry);
            return entry;
        }

        /// <summary>
        /// Log experience gain
        /// </summary>
        public CombatLogEntry LogExperience(float amount, string target, string source = "战斗")
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.Experience,
                Message = $"⭐ {target} 从 {source} 获得 {amount:F0} 经验",
                Value = amount,
                Source = source,
                Target = target,
                IsPlayerAction = true
            };

            AddEntry(entry);
            return entry;
        }

        /// <summary>
        /// Log level up
        /// </summary>
        public CombatLogEntry LogLevelUp(string target, int newLevel)
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.LevelUp,
                Message = $"🎉 {target} 升级到 {newLevel} 级!",
                Value = newLevel,
                Target = target,
                IsPlayerAction = true
            };

            AddEntry(entry);
            return entry;
        }

        /// <summary>
        /// Log info message
        /// </summary>
        public CombatLogEntry LogInfo(string message, bool isPlayerAction = true)
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.Info,
                Message = $"ℹ️ {message}",
                IsPlayerAction = isPlayerAction
            };

            AddEntry(entry);
            return entry;
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        public CombatLogEntry LogWarning(string message, bool isPlayerAction = true)
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.Warning,
                Message = $"⚠️ {message}",
                IsPlayerAction = isPlayerAction
            };

            AddEntry(entry);
            return entry;
        }

        /// <summary>
        /// Log enemy spawn
        /// </summary>
        public CombatLogEntry LogEnemySpawn(string enemyName, int waveNumber)
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.EnemySpawn,
                Message = $"👹 第 {waveNumber} 波: {enemyName} 出现!",
                Source = enemyName,
                Value = waveNumber,
                IsPlayerAction = false
            };

            AddEntry(entry);
            return entry;
        }

        /// <summary>
        /// Log enemy aggro
        /// </summary>
        public CombatLogEntry LogEnemyAggro(string enemyName, string target)
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.EnemyAggro,
                Message = $"👁️ {enemyName} 锁定 {target}",
                Source = enemyName,
                Target = target,
                IsPlayerAction = false
            };

            AddEntry(entry);
            return entry;
        }

        #endregion

        #region Entry Management

        private void AddEntry(CombatLogEntry entry)
        {
            _logEntries.Add(entry);

            // Auto-clear old entries
            if (_logEntries.Count > _maxEntries)
            {
                _logEntries.RemoveAt(0);
            }

            // Emit signal
            EmitSignal(SignalNewEntry, entry);
        }

        #endregion

        #region Getters

        /// <summary>
        /// Get all log entries
        /// </summary>
        public List<CombatLogEntry> GetAllEntries()
        {
            return new List<CombatLogEntry>(_logEntries);
        }

        /// <summary>
        /// Get recent entries (last n entries)
        /// </summary>
        public List<CombatLogEntry> GetRecentEntries(int count = 20)
        {
            int start = Math.Max(0, _logEntries.Count - count);
            int length = Math.Min(count, _logEntries.Count - start);

            if (length <= 0) return new List<CombatLogEntry>();

            return _logEntries.GetRange(start, length);
        }

        /// <summary>
        /// Get entries by type
        /// </summary>
        public List<CombatLogEntry> GetEntriesByType(CombatLogType type)
        {
            var result = new List<CombatLogEntry>();

            foreach (var entry in _logEntries)
            {
                if (entry.Type == type)
                {
                    result.Add(entry);
                }
            }

            return result;
        }

        /// <summary>
        /// Get session time
        /// </summary>
        public float GetSessionTime()
        {
            return _currentSessionTime;
        }

        /// <summary>
        /// Get entry count
        /// </summary>
        public int GetEntryCount()
        {
            return _logEntries.Count;
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Set max entries
        /// </summary>
        public void SetMaxEntries(int max)
        {
            _maxEntries = max;
        }

        /// <summary>
        /// Clear all log entries
        /// </summary>
        public void ClearLog()
        {
            _logEntries.Clear();
        }

        /// <summary>
        /// Reset session
        /// </summary>
        public void ResetSession()
        {
            _logEntries.Clear();
            _currentSessionTime = 0f;
        }

        #endregion

        #region Data Persistence

        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();

            // 会话时间
            data["sessionTime"] = _currentSessionTime;
            data["maxEntries"] = _maxEntries;

            // 日志条目（只保存最后100条，避免存档过大）
            var entries = new ArrayList();
            var startIndex = Math.Max(0, _logEntries.Count - 100);
            for (int i = startIndex; i < _logEntries.Count; i++)
            {
                var entry = _logEntries[i];
                entries.Add(new Dictionary
                {
                    { "timestamp", entry.Timestamp },
                    { "type", (int)entry.Type },
                    { "message", entry.Message ?? "" },
                    { "value", entry.Value },
                    { "source", entry.Source ?? "" },
                    { "target", entry.Target ?? "" },
                    { "isPlayerAction", entry.IsPlayerAction }
                });
            }
            data["logEntries"] = entries;

            return data;
        }

        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            // 恢复会话时间
            if (data.ContainsKey("sessionTime"))
                _currentSessionTime = Convert.ToSingle(data["sessionTime"]);

            // 恢复最大条目数
            if (data.ContainsKey("maxEntries"))
                _maxEntries = Convert.ToInt32(data["maxEntries"]);

            // 恢复日志条目
            if (data.ContainsKey("logEntries"))
            {
                _logEntries.Clear();
                var entries = data["logEntries"] as ArrayList;
                if (entries != null)
                {
                    foreach (Dictionary entryData in entries)
                    {
                        var entry = new CombatLogEntry
                        {
                            Timestamp = entryData.ContainsKey("timestamp") ? Convert.ToSingle(entryData["timestamp"]) : 0f,
                            Type = entryData.ContainsKey("type") ? (CombatLogType)Convert.ToInt32(entryData["type"]) : CombatLogType.Info,
                            Message = entryData.ContainsKey("message") ? entryData["message"].ToString() : "",
                            Value = entryData.ContainsKey("value") ? Convert.ToSingle(entryData["value"]) : 0f,
                            Source = entryData.ContainsKey("source") ? entryData["source"].ToString() : "",
                            Target = entryData.ContainsKey("target") ? entryData["target"].ToString() : "",
                            IsPlayerAction = entryData.ContainsKey("isPlayerAction") && Convert.ToBoolean(entryData["isPlayerAction"])
                        };
                        _logEntries.Add(entry);
                    }
                }
            }

            GD.Print("[CombatLogRecorder] Save data imported successfully");
        }

        #endregion
    }
}
