using System;
using System.Collections.Generic;
using Godot;
using Framework;
using ClawRPG.Scripts.Combat;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// Combat Log System - 战斗日志系统协调者
    /// 委托给子系统：
    ///   - CombatLogRecorder: 日志记录
    ///   - CombatLogFormatter: 格式化显示
    ///   - CombatPersistenceSystem: 持久化和统计
    /// 保留核心功能：协调、过滤、信号
    /// </summary>
    public class CombatLogSystem : BaseSystem
    {
        private static CombatLogSystem _instance;
        public static CombatLogSystem Instance => _instance;

        // ========== 子系统引用 ==========
        // 使用 NodePath 在运行时获取子系统
        private NodePath _recorderPath = new NodePath("../CombatLog/CombatLogRecorder");
        private NodePath _formatterPath = new NodePath("../CombatLog/CombatLogFormatter");
        private NodePath _persistencePath = new NodePath("../CombatLog/CombatPersistenceSystem");
        
        private CombatLogRecorder _recorder;
        private CombatLogFormatter _formatter;
        private CombatPersistenceSystem _persistence;
        
        // ========== 本地存储（仅协调用） ==========
        private List<CombatLogEntry> _logEntries = new List<CombatLogEntry>();
        private List<CombatLogEntry> _filteredEntries = new List<CombatLogEntry>();

        // Configuration (delegated to persistence)
        private int _maxEntries = 500;
        private float _autoClearTime = 300f;
        private float _currentSessionTime = 0f;

        // Signals
        public static string SignalNewEntry = "new_combat_log_entry";
        public static string SignalComboMilestone = "combo_milestone";
        public static string SignalKillStreak = "kill_streak";

        protected override void Initialize()
        {
            _instance = this;
            
            // 获取或创建子系统
            _recorder = GetNodeOrNull<CombatLogRecorder>(_recorderPath);
            _formatter = GetNodeOrNull<CombatLogFormatter>(_formatterPath);
            _persistence = GetNodeOrNull<CombatPersistenceSystem>(_persistencePath);
            
            if (_recorder == null)
            {
                _recorder = new CombatLogRecorder();
                _recorder.Name = "CombatLogRecorder";
                AddChild(_recorder);
            }
            
            if (_formatter == null)
            {
                _formatter = new CombatLogFormatter();
                _formatter.Name = "CombatLogFormatter";
                AddChild(_formatter);
            }
            
            if (_persistence == null)
            {
                _persistence = new CombatPersistenceSystem();
                _persistence.Name = "CombatPersistenceSystem";
                AddChild(_persistence);
            }
            
            GD.Print("[CombatLogSystem] Combat Log System initialized as coordinator");
        }

        public override void _Process(double delta)
        {
            _currentSessionTime += delta;

            // Update filtered entries
            ApplyFilters();

            // Auto-clear old entries
            if (_logEntries.Count > _maxEntries)
            {
                _logEntries.RemoveAt(0);
            }
        }

        #region Public API - 委托给 Recorder

        /// <summary>
        /// Log a damage event
        /// </summary>
        public void LogDamage(float damage, string source, string target, bool isCritical = false, bool isPlayerSource = true)
        {
            var entry = _recorder.LogDamage(damage, source, target, isCritical, isPlayerSource);
            AddEntry(entry);
            
            // Update persistence statistics
            _persistence.RecordDamage(damage, isCritical, isPlayerSource);
            
            if (isPlayerSource)
            {
                _persistence.AddCombo(1);
                CheckComboMilestone();
            }
        }

        /// <summary>
        /// Log a healing event
        /// </summary>
        public void LogHealing(float amount, string source, string target, bool isPlayerSource = true)
        {
            var entry = _recorder.LogHealing(amount, source, target, isPlayerSource);
            AddEntry(entry);
            
            // Update persistence statistics
            _persistence.RecordHealing(amount);
        }

        /// <summary>
        /// Log a miss event
        /// </summary>
        public void LogMiss(string source, string target, string missType = "Miss", bool isPlayerSource = true)
        {
            var entry = _recorder.LogMiss(source, target, missType, isPlayerSource);
            AddEntry(entry);
            
            _persistence.RecordMiss();
        }

        /// <summary>
        /// Log a block event
        /// </summary>
        public void LogBlock(string source, string target, float blockedDamage, bool isPlayerSource = true)
        {
            var entry = _recorder.LogBlock(source, target, blockedDamage, isPlayerSource);
            AddEntry(entry);
            
            _persistence.RecordBlock();
        }

        /// <summary>
        /// Log a dodge event
        /// </summary>
        public void LogDodge(string source, string target, bool isPlayerSource = true)
        {
            var entry = _recorder.LogDodge(source, target, isPlayerSource);
            AddEntry(entry);
            
            _persistence.RecordDodge();
        }

        /// <summary>
        /// Log a parry event
        /// </summary>
        public void LogParry(string source, string target, bool isPlayerSource = true)
        {
            var entry = _recorder.LogParry(source, target, isPlayerSource);
            AddEntry(entry);
        }

        /// <summary>
        /// Log a kill event
        /// </summary>
        public void LogKill(string killer, string target, bool isPlayerKiller = true)
        {
            var entry = _recorder.LogKill(killer, target, isPlayerKiller);
            AddEntry(entry);
            
            // Update persistence
            _persistence.RecordKill();
            
            if (isPlayerKiller)
            {
                _persistence.AddKillStreak(target);
                CheckKillStreak();
            }
        }

        /// <summary>
        /// Log a death event
        /// </summary>
        public void LogDeath(string target, string killer)
        {
            var entry = _recorder.LogDeath(target, killer);
            AddEntry(entry);
        }

        /// <summary>
        /// Log a buff application
        /// </summary>
        public void LogBuff(string target, string buffName, float duration, bool isPlayerTarget = true)
        {
            var entry = _recorder.LogBuff(target, buffName, duration, isPlayerTarget);
            AddEntry(entry);
        }

        /// <summary>
        /// Log a debuff application
        /// </summary>
        public void LogDebuff(string target, string debuffName, float duration, bool isPlayerTarget = true)
        {
            var entry = _recorder.LogDebuff(target, debuffName, duration, isPlayerTarget);
            AddEntry(entry);
        }

        /// <summary>
        /// Log a skill use
        /// </summary>
        public void LogSkill(string skillName, string user, string target = "", bool isPlayerUser = true)
        {
            var entry = _recorder.LogSkill(skillName, user, target, isPlayerUser);
            AddEntry(entry);
        }

        /// <summary>
        /// Log an item use
        /// </summary>
        public void LogItem(string itemName, string user, string effect = "", bool isPlayerUser = true)
        {
            var entry = _recorder.LogItem(itemName, user, effect, isPlayerUser);
            AddEntry(entry);
        }

        /// <summary>
        /// Log mana/energy change
        /// </summary>
        public void LogResource(string resourceType, float amount, string target, bool isGain = true)
        {
            var entry = _recorder.LogResource(resourceType, amount, target, isGain);
            AddEntry(entry);
        }

        /// <summary>
        /// Log experience gain
        /// </summary>
        public void LogExperience(float amount, string target, string source = "战斗")
        {
            var entry = _recorder.LogExperience(amount, target, source);
            AddEntry(entry);
        }

        /// <summary>
        /// Log level up
        /// </summary>
        public void LogLevelUp(string target, int newLevel)
        {
            var entry = _recorder.LogLevelUp(target, newLevel);
            AddEntry(entry);
        }

        /// <summary>
        /// Log info message
        /// </summary>
        public void LogInfo(string message, bool isPlayerAction = true)
        {
            var entry = _recorder.LogInfo(message, isPlayerAction);
            AddEntry(entry);
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        public void LogWarning(string message, bool isPlayerAction = true)
        {
            var entry = _recorder.LogWarning(message, isPlayerAction);
            AddEntry(entry);
        }

        /// <summary>
        /// Log enemy spawn
        /// </summary>
        public void LogEnemySpawn(string enemyName, int waveNumber)
        {
            var entry = _recorder.LogEnemySpawn(enemyName, waveNumber);
            AddEntry(entry);
        }

        /// <summary>
        /// Log enemy aggro
        /// </summary>
        public void LogEnemyAggro(string enemyName, string target)
        {
            var entry = _recorder.LogEnemyAggro(enemyName, target);
            AddEntry(entry);
        }

        #endregion

        #region Entry Management

        private void AddEntry(CombatLogEntry entry)
        {
            _logEntries.Add(entry);
            
            // Emit signal for UI update
            EmitSignal(SignalNewEntry, entry);

            // Check for combo milestones
            CheckComboMilestone();
        }

        private void ApplyFilters()
        {
            _filteredEntries.Clear();

            foreach (var entry in _logEntries)
            {
                if (_persistence != null)
                {
                    if (_persistence.ShouldIncludeEntry(entry))
                    {
                        _filteredEntries.Add(entry);
                    }
                }
                else
                {
                    // Fallback if persistence not available
                    _filteredEntries.Add(entry);
                }
            }
        }

        private void CheckComboMilestone()
        {
            int combo = _persistence != null ? _persistence.GetCurrentCombo() : 0;
            int[] milestones = { 5, 10, 15, 20, 25, 30, 40, 50, 75, 100 };

            foreach (int milestone in milestones)
            {
                if (combo == milestone)
                {
                    var entry = new CombatLogEntry
                    {
                        Timestamp = _currentSessionTime,
                        Type = CombatLogType.Combo,
                        Message = $"💥 Combo x{milestone}!",
                        Value = milestone,
                        IsPlayerAction = true
                    };
                    AddEntry(entry);

                    EmitSignal(SignalComboMilestone, milestone);
                    break;
                }
            }
        }

        private void CheckKillStreak()
        {
            if (_persistence == null) return;
            
            int streak = _persistence.GetKillStreak();
            if (streak >= 3)
            {
                EmitSignal(SignalKillStreak, streak);
                var entry = new CombatLogEntry
                {
                    Timestamp = _currentSessionTime,
                    Type = CombatLogType.Combo,
                    Message = $"🔥 击杀 streak x{streak}!",
                    Value = streak,
                    IsPlayerAction = true
                };
                AddEntry(entry);
            }
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
        /// Get filtered entries
        /// </summary>
        public List<CombatLogEntry> GetFilteredEntries()
        {
            return new List<CombatLogEntry>(_filteredEntries);
        }

        /// <summary>
        /// Get recent entries (last n entries)
        /// </summary>
        public List<CombatLogEntry> GetRecentEntries(int count = 20)
        {
            int start = Math.Max(0, _filteredEntries.Count - count);
            int length = Math.Min(count, _filteredEntries.Count - start);

            if (length <= 0) return new List<CombatLogEntry>();

            return _filteredEntries.GetRange(start, length);
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
        /// Get current combo count
        /// </summary>
        public int GetCurrentCombo()
        {
            return _persistence != null ? _persistence.GetCurrentCombo() : 0;
        }

        /// <summary>
        /// Get kill streak
        /// </summary>
        public int GetKillStreak()
        {
            return _persistence != null ? _persistence.GetKillStreak() : 0;
        }

        /// <summary>
        /// Get statistics
        /// </summary>
        public CombatLogStatistics GetStatistics()
        {
            return _persistence != null ? _persistence.GetStatistics() : new CombatLogStatistics();
        }

        /// <summary>
        /// Get session time
        /// </summary>
        public float GetSessionTime()
        {
            return _currentSessionTime;
        }

        /// <summary>
        /// Get recent kills
        /// </summary>
        public List<string> GetRecentKills()
        {
            return _persistence != null ? _persistence.GetRecentKills() : new List<string>();
        }

        /// <summary>
        /// Get formatter instance
        /// </summary>
        public CombatLogFormatter GetFormatter()
        {
            return _formatter;
        }

        #endregion

        #region Filter Control - 委托给 Persistence

        /// <summary>
        /// Set damage filter
        /// </summary>
        public void SetShowDamage(bool show)
        {
            _persistence?.SetShowDamage(show);
        }

        /// <summary>
        /// Set healing filter
        /// </summary>
        public void SetShowHealing(bool show)
        {
            _persistence?.SetShowHealing(show);
        }

        /// <summary>
        /// Set buff filter
        /// </summary>
        public void SetShowBuffs(bool show)
        {
            _persistence?.SetShowBuffs(show);
        }

        /// <summary>
        /// Set skill filter
        /// </summary>
        public void SetShowSkills(bool show)
        {
            _persistence?.SetShowSkills(show);
        }

        /// <summary>
        /// Set combat filter
        /// </summary>
        public void SetShowCombat(bool show)
        {
            _persistence?.SetShowCombat(show);
        }

        /// <summary>
        /// Set info filter
        /// </summary>
        public void SetShowInfo(bool show)
        {
            _persistence?.SetShowInfo(show);
        }

        /// <summary>
        /// Set player only filter
        /// </summary>
        public void SetPlayerOnly(bool playerOnly)
        {
            _persistence?.SetPlayerOnly(playerOnly);
        }

        /// <summary>
        /// Set enemy only filter
        /// </summary>
        public void SetEnemyOnly(bool enemyOnly)
        {
            _persistence?.SetEnemyOnly(enemyOnly);
        }

        /// <summary>
        /// Clear all filters
        /// </summary>
        public void ClearFilters()
        {
            _persistence?.ClearFilters();
        }

        /// <summary>
        /// Clear all log entries
        /// </summary>
        public void ClearLog()
        {
            _logEntries.Clear();
            _filteredEntries.Clear();
            _recorder?.ClearLog();
        }

        /// <summary>
        /// Reset statistics
        /// </summary>
        public void ResetStatistics()
        {
            _persistence?.ResetStatistics();
        }

        /// <summary>
        /// Reset session
        /// </summary>
        public void ResetSession()
        {
            ClearLog();
            ResetStatistics();
            _currentSessionTime = 0f;
            _persistence?.FullReset();
        }

        #endregion
        
        #region 数据持久化 - 委托给子系统

        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();

            // 会话时间
            data["sessionTime"] = _currentSessionTime;
            data["maxEntries"] = _maxEntries;

            // 委托给子系统
            if (_persistence != null)
            {
                data["persistence"] = _persistence.ExportSaveData();
            }
            
            if (_recorder != null)
            {
                data["recorder"] = _recorder.ExportSaveData();
            }
            
            if (_formatter != null)
            {
                data["formatter"] = _formatter.ExportSaveData();
            }

            return data;
        }

        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            // 恢复会话时间
            if (data.Contains("sessionTime"))
                _currentSessionTime = Convert.ToSingle(data["sessionTime"]);

            // 恢复最大条目数
            if (data.Contains("maxEntries"))
                _maxEntries = Convert.ToInt32(data["maxEntries"]);

            // 委托给子系统
            if (data.Contains("persistence") && _persistence != null)
            {
                _persistence.ImportSaveData(data["persistence"] as Dictionary);
            }
            
            if (data.Contains("recorder") && _recorder != null)
            {
                _recorder.ImportSaveData(data["recorder"] as Dictionary);
            }
            
            if (data.Contains("formatter") && _formatter != null)
            {
                _formatter.ImportSaveData(data["formatter"] as Dictionary);
            }

            // 重新加载日志
            if (_recorder != null)
            {
                _logEntries = _recorder.GetAllEntries();
            }
            
            ApplyFilters();

            GD.Print("[CombatLogSystem] Save data imported successfully");
        }

        #endregion
    }
}
