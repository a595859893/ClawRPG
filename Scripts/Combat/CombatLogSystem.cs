using System;
using System.Collections.Generic;
using Godot;
using Framework;
using ClawRPG.Scripts.Combat;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// Combat Log System - 战斗日志系统协调者
    /// 委托给子系统：Recorder（记录）、Formatter（格式化）、UI（显示）
    /// 保留核心功能：统计、过滤、连击、持久化
    /// </summary>
    public class CombatLogSystem : BaseSystem
    {
        private static CombatLogSystem _instance;
        public static CombatLogSystem Instance => _instance;

        // ========== 子系统引用 ==========
        // 使用 NodePath 在运行时获取子系统，避免直接 new
        private NodePath _recorderPath = new NodePath("../CombatLogRecorder");
        private NodePath _formatterPath = new NodePath("../CombatLogFormatter");
        
        private CombatLogRecorder _recorder;
        private CombatLogFormatter _formatter;
        
        // ========== 本地存储（协调数据） ==========
        private List<CombatLogEntry> _logEntries = new List<CombatLogEntry>();
        private List<CombatLogEntry> _filteredEntries = new List<CombatLogEntry>();

        // Configuration
        private int _maxEntries = 500;
        private float _autoClearTime = 300f; // 5 minutes
        private float _currentSessionTime = 0f;

        // Statistics
        private CombatLogStatistics _statistics = new CombatLogStatistics();

        // Filters
        private bool _showDamage = true;
        private bool _showHealing = true;
        private bool _showBuffs = true;
        private bool _showSkills = true;
        private bool _showCombat = true;
        private bool _showInfo = true;
        private bool _playerOnly = false;
        private bool _enemyOnly = false;

        // Combat state
        private int _currentCombo = 0;
        private float _comboTimer = 0f;
        private float _comboTimeWindow = 3f;

        // Recent kills tracking
        private List<string> _recentKills = new List<string>();
        private float _killStreakTimer = 0f;
        private int _killStreak = 0;

        // Signals
        public static string SignalNewEntry = "new_combat_log_entry";
        public static string SignalComboMilestone = "combo_milestone";
        public static string SignalKillStreak = "kill_streak";

        protected override void Initialize()
        {
            _instance = this;
            
            // 获取子系统引用
            _recorder = GetNodeOrNull<CombatLogRecorder>(_recorderPath);
            _formatter = GetNodeOrNull<CombatLogFormatter>(_formatterPath);
            
            if (_recorder == null)
            {
                _recorder = new CombatLogRecorder();
                AddChild(_recorder);
            }
            
            if (_formatter == null)
            {
                _formatter = new CombatLogFormatter();
                AddChild(_formatter);
            }
            
            GD.Print("[CombatLogSystem] Combat Log System initialized as coordinator");
        }

        public override void _Process(float delta)
        {
            _currentSessionTime += delta;
            _comboTimer -= delta;
            _killStreakTimer -= delta;

            // Update filtered entries
            ApplyFilters();

            // Auto-clear old entries
            if (_logEntries.Count > _maxEntries)
            {
                _logEntries.RemoveAt(0);
            }

            // Clear combo if timer expired
            if (_comboTimer <= 0 && _currentCombo > 0)
            {
                _currentCombo = 0;
            }

            // Reset kill streak if timer expired
            if (_killStreakTimer <= 0 && _killStreak > 0)
            {
                _killStreak = 0;
            }
        }

        #region Public API

        /// <summary>
        /// Log a damage event
        /// </summary>
        public void LogDamage(float damage, string source, string target, bool isCritical = false, bool isPlayerSource = true)
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

            // Update statistics
            _statistics.DamageEntries++;
            if (isCritical) _statistics.CriticalHits++;

            if (isPlayerSource)
            {
                _statistics.TotalDamageDealt += damage;
                AddCombo(1);
            }
            else
            {
                _statistics.TotalDamageTaken += damage;
            }
        }

        /// <summary>
        /// Log a healing event
        /// </summary>
        public void LogHealing(float amount, string source, string target, bool isPlayerSource = true)
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

            // Update statistics
            _statistics.HealingEntries++;
            _statistics.TotalHealing += amount;
        }

        /// <summary>
        /// Log a miss event
        /// </summary>
        public void LogMiss(string source, string target, string missType = "Miss", bool isPlayerSource = true)
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
            _statistics.Misses++;
        }

        /// <summary>
        /// Log a block event
        /// </summary>
        public void LogBlock(string source, string target, float blockedDamage, bool isPlayerSource = true)
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
            _statistics.Blocks++;
        }

        /// <summary>
        /// Log a dodge event
        /// </summary>
        public void LogDodge(string source, string target, bool isPlayerSource = true)
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
            _statistics.Dodges++;
        }

        /// <summary>
        /// Log a parry event
        /// </summary>
        public void LogParry(string source, string target, bool isPlayerSource = true)
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
        }

        /// <summary>
        /// Log a kill event
        /// </summary>
        public void LogKill(string killer, string target, bool isPlayerKiller = true)
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

            // Update statistics
            _statistics.KillEntries++;

            // Track kill streak
            if (isPlayerKiller)
            {
                _killStreak++;
                _killStreakTimer = 5f;

                if (_killStreak >= 3)
                {
                    EmitSignal(SignalKillStreak, _killStreak);
                    var streakEntry = new CombatLogEntry
                    {
                        Timestamp = _currentSessionTime,
                        Type = CombatLogType.Combo,
                        Message = $"🔥 击杀 streak x{_killStreak}!",
                        Value = _killStreak,
                        IsPlayerAction = true
                    };
                    AddEntry(streakEntry);
                }
            }

            _recentKills.Add(target);
            if (_recentKills.Count > 10)
            {
                _recentKills.RemoveAt(0);
            }
        }

        /// <summary>
        /// Log a death event
        /// </summary>
        public void LogDeath(string target, string killer)
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
        }

        /// <summary>
        /// Log a buff application
        /// </summary>
        public void LogBuff(string target, string buffName, float duration, bool isPlayerTarget = true)
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
        }

        /// <summary>
        /// Log a debuff application
        /// </summary>
        public void LogDebuff(string target, string debuffName, float duration, bool isPlayerTarget = true)
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
        }

        /// <summary>
        /// Log a skill use
        /// </summary>
        public void LogSkill(string skillName, string user, string target = "", bool isPlayerUser = true)
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
        }

        /// <summary>
        /// Log an item use
        /// </summary>
        public void LogItem(string itemName, string user, string effect = "", bool isPlayerUser = true)
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
        }

        /// <summary>
        /// Log mana/energy change
        /// </summary>
        public void LogResource(string resourceType, float amount, string target, bool isGain = true)
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
        }

        /// <summary>
        /// Log experience gain
        /// </summary>
        public void LogExperience(float amount, string target, string source = "战斗")
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
        }

        /// <summary>
        /// Log level up
        /// </summary>
        public void LogLevelUp(string target, int newLevel)
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
        }

        /// <summary>
        /// Log info message
        /// </summary>
        public void LogInfo(string message, bool isPlayerAction = true)
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.Info,
                Message = $"ℹ️ {message}",
                IsPlayerAction = isPlayerAction
            };

            AddEntry(entry);
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        public void LogWarning(string message, bool isPlayerAction = true)
        {
            var entry = new CombatLogEntry
            {
                Timestamp = _currentSessionTime,
                Type = CombatLogType.Warning,
                Message = $"⚠️ {message}",
                IsPlayerAction = isPlayerAction
            };

            AddEntry(entry);
        }

        /// <summary>
        /// Log enemy spawn
        /// </summary>
        public void LogEnemySpawn(string enemyName, int waveNumber)
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
        }

        /// <summary>
        /// Log enemy aggro
        /// </summary>
        public void LogEnemyAggro(string enemyName, string target)
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
        }

        #endregion

        #region Entry Management

        private void AddEntry(CombatLogEntry entry)
        {
            _logEntries.Add(entry);
            _statistics.TotalEntries++;

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
                bool include = true;

                // Type filter
                switch (entry.Type)
                {
                    case CombatLogType.Damage:
                    case CombatLogType.Critical:
                        include = _showDamage && _showCombat;
                        break;
                    case CombatLogType.Healing:
                        include = _showHealing;
                        break;
                    case CombatLogType.Buff:
                    case CombatLogType.Debuff:
                        include = _showBuffs;
                        break;
                    case CombatLogType.SkillUsed:
                    case CombatLogType.ItemUsed:
                        include = _showSkills;
                        break;
                    case CombatLogType.Kill:
                    case CombatLogType.Death:
                    case CombatLogType.Miss:
                    case CombatLogType.Block:
                    case CombatLogType.Dodge:
                    case CombatLogType.Parry:
                    case CombatLogType.Shield:
                        include = _showCombat;
                        break;
                    case CombatLogType.Experience:
                    case CombatLogType.LevelUp:
                        include = _showInfo;
                        break;
                    default:
                        include = _showInfo;
                        break;
                }

                // Player/Enemy filter
                if (include)
                {
                    if (_playerOnly && !entry.IsPlayerAction) include = false;
                    if (_enemyOnly && entry.IsPlayerAction) include = false;
                }

                if (include)
                {
                    _filteredEntries.Add(entry);
                }
            }
        }

        private void AddCombo(int hits)
        {
            _currentCombo += hits;
            _comboTimer = _comboTimeWindow;

            if (_currentCombo >= 5)
            {
                CheckComboMilestone();
            }
        }

        private void CheckComboMilestone()
        {
            int[] milestones = { 5, 10, 15, 20, 25, 30, 40, 50, 75, 100 };

            foreach (int milestone in milestones)
            {
                if (_currentCombo == milestone)
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
            return _currentCombo;
        }

        /// <summary>
        /// Get kill streak
        /// </summary>
        public int GetKillStreak()
        {
            return _killStreak;
        }

        /// <summary>
        /// Get statistics
        /// </summary>
        public CombatLogStatistics GetStatistics()
        {
            return _statistics;
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
            return new List<string>(_recentKills);
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Set damage filter
        /// </summary>
        public void SetShowDamage(bool show)
        {
            _showDamage = show;
        }

        /// <summary>
        /// Set healing filter
        /// </summary>
        public void SetShowHealing(bool show)
        {
            _showHealing = show;
        }

        /// <summary>
        /// Set buff filter
        /// </summary>
        public void SetShowBuffs(bool show)
        {
            _showBuffs = show;
        }

        /// <summary>
        /// Set skill filter
        /// </summary>
        public void SetShowSkills(bool show)
        {
            _showSkills = show;
        }

        /// <summary>
        /// Set combat filter
        /// </summary>
        public void SetShowCombat(bool show)
        {
            _showCombat = show;
        }

        /// <summary>
        /// Set info filter
        /// </summary>
        public void SetShowInfo(bool show)
        {
            _showInfo = show;
        }

        /// <summary>
        /// Set player only filter
        /// </summary>
        public void SetPlayerOnly(bool playerOnly)
        {
            _playerOnly = playerOnly;
            if (playerOnly) _enemyOnly = false;
        }

        /// <summary>
        /// Set enemy only filter
        /// </summary>
        public void SetEnemyOnly(bool enemyOnly)
        {
            _enemyOnly = enemyOnly;
            if (enemyOnly) _playerOnly = false;
        }

        /// <summary>
        /// Clear all filters
        /// </summary>
        public void ClearFilters()
        {
            _showDamage = true;
            _showHealing = true;
            _showBuffs = true;
            _showSkills = true;
            _showCombat = true;
            _showInfo = true;
            _playerOnly = false;
            _enemyOnly = false;
        }

        /// <summary>
        /// Clear all log entries
        /// </summary>
        public void ClearLog()
        {
            _logEntries.Clear();
            _filteredEntries.Clear();
            _currentCombo = 0;
            _killStreak = 0;
        }

        /// <summary>
        /// Reset statistics
        /// </summary>
        public void ResetStatistics()
        {
            _statistics.Reset();
        }

        /// <summary>
        /// Reset session
        /// </summary>
        public void ResetSession()
        {
            ClearLog();
            ResetStatistics();
            _currentSessionTime = 0f;
            _currentCombo = 0;
            _killStreak = 0;
        }

        #endregion
        
        #region 数据持久化

        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();

            // 会话时间
            data["sessionTime"] = _currentSessionTime;

            // 统计信息
            var stats = new Dictionary
            {
                { "totalEntries", _statistics.TotalEntries },
                { "damageEntries", _statistics.DamageEntries },
                { "healingEntries", _statistics.HealingEntries },
                { "killEntries", _statistics.KillEntries },
                { "criticalHits", _statistics.CriticalHits },
                { "misses", _statistics.Misses },
                { "blocks", _statistics.Blocks },
                { "dodges", _statistics.Dodges },
                { "totalDamageDealt", _statistics.TotalDamageDealt },
                { "totalDamageTaken", _statistics.TotalDamageTaken },
                { "totalHealing", _statistics.TotalHealing }
            };
            data["statistics"] = stats;

            // 筛选器设置
            var filters = new Dictionary
            {
                { "showDamage", _showDamage },
                { "showHealing", _showHealing },
                { "showBuffs", _showBuffs },
                { "showSkills", _showSkills },
                { "showCombat", _showCombat },
                { "showInfo", _showInfo },
                { "playerOnly", _playerOnly },
                { "enemyOnly", _enemyOnly }
            };
            data["filters"] = filters;

            // 连击状态
            var combo = new Dictionary
            {
                { "currentCombo", _currentCombo },
                { "comboTimer", _comboTimer }
            };
            data["combo"] = combo;

            // 击杀streak
            var killStreak = new Dictionary
            {
                { "killStreak", _killStreak },
                { "killStreakTimer", _killStreakTimer }
            };
            data["killStreak"] = killStreak;

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
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;

            // 恢复会话时间
            if (data.Contains("sessionTime"))
                _currentSessionTime = Convert.ToSingle(data["sessionTime"]);

            // 恢复统计信息
            if (data.Contains("statistics"))
            {
                var stats = data["statistics"] as Dictionary;
                if (stats != null)
                {
                    if (stats.Contains("totalEntries")) _statistics.TotalEntries = Convert.ToInt32(stats["totalEntries"]);
                    if (stats.Contains("damageEntries")) _statistics.DamageEntries = Convert.ToInt32(stats["damageEntries"]);
                    if (stats.Contains("healingEntries")) _statistics.HealingEntries = Convert.ToInt32(stats["healingEntries"]);
                    if (stats.Contains("killEntries")) _statistics.KillEntries = Convert.ToInt32(stats["killEntries"]);
                    if (stats.Contains("criticalHits")) _statistics.CriticalHits = Convert.ToInt32(stats["criticalHits"]);
                    if (stats.Contains("misses")) _statistics.Misses = Convert.ToInt32(stats["misses"]);
                    if (stats.Contains("blocks")) _statistics.Blocks = Convert.ToInt32(stats["blocks"]);
                    if (stats.Contains("dodges")) _statistics.Dodges = Convert.ToInt32(stats["dodges"]);
                    if (stats.Contains("totalDamageDealt")) _statistics.TotalDamageDealt = Convert.ToSingle(stats["totalDamageDealt"]);
                    if (stats.Contains("totalDamageTaken")) _statistics.TotalDamageTaken = Convert.ToSingle(stats["totalDamageTaken"]);
                    if (stats.Contains("totalHealing")) _statistics.TotalHealing = Convert.ToSingle(stats["totalHealing"]);
                }
            }

            // 恢复筛选器设置
            if (data.Contains("filters"))
            {
                var filters = data["filters"] as Dictionary;
                if (filters != null)
                {
                    if (filters.Contains("showDamage")) _showDamage = Convert.ToBoolean(filters["showDamage"]);
                    if (filters.Contains("showHealing")) _showHealing = Convert.ToBoolean(filters["showHealing"]);
                    if (filters.Contains("showBuffs")) _showBuffs = Convert.ToBoolean(filters["showBuffs"]);
                    if (filters.Contains("showSkills")) _showSkills = Convert.ToBoolean(filters["showSkills"]);
                    if (filters.Contains("showCombat")) _showCombat = Convert.ToBoolean(filters["showCombat"]);
                    if (filters.Contains("showInfo")) _showInfo = Convert.ToBoolean(filters["showInfo"]);
                    if (filters.Contains("playerOnly")) _playerOnly = Convert.ToBoolean(filters["playerOnly"]);
                    if (filters.Contains("enemyOnly")) _enemyOnly = Convert.ToBoolean(filters["enemyOnly"]);
                }
            }

            // 恢复连击状态
            if (data.Contains("combo"))
            {
                var combo = data["combo"] as Dictionary;
                if (combo != null)
                {
                    if (combo.Contains("currentCombo")) _currentCombo = Convert.ToInt32(combo["currentCombo"]);
                    if (combo.Contains("comboTimer")) _comboTimer = Convert.ToSingle(combo["comboTimer"]);
                }
            }

            // 恢复击杀streak
            if (data.Contains("killStreak"))
            {
                var killStreak = data["killStreak"] as Dictionary;
                if (killStreak != null)
                {
                    if (killStreak.Contains("killStreak")) _killStreak = Convert.ToInt32(killStreak["killStreak"]);
                    if (killStreak.Contains("killStreakTimer")) _killStreakTimer = Convert.ToSingle(killStreak["killStreakTimer"]);
                }
            }

            // 恢复日志条目
            if (data.Contains("logEntries"))
            {
                _logEntries.Clear();
                var entries = data["logEntries"] as ArrayList;
                if (entries != null)
                {
                    foreach (Dictionary entryData in entries)
                    {
                        var entry = new CombatLogEntry
                        {
                            Timestamp = entryData.Contains("timestamp") ? Convert.ToSingle(entryData["timestamp"]) : 0f,
                            Type = entryData.Contains("type") ? (CombatLogType)Convert.ToInt32(entryData["type"]) : CombatLogType.Info,
                            Message = entryData.Contains("message") ? entryData["message"].ToString() : "",
                            Value = entryData.Contains("value") ? Convert.ToSingle(entryData["value"]) : 0f,
                            Source = entryData.Contains("source") ? entryData["source"].ToString() : "",
                            Target = entryData.Contains("target") ? entryData["target"].ToString() : "",
                            IsPlayerAction = entryData.Contains("isPlayerAction") && Convert.ToBoolean(entryData["isPlayerAction"])
                        };
                        _logEntries.Add(entry);
                    }
                }
                // 重新应用筛选器
                ApplyFilters();
            }

            GD.Print("[CombatLogSystem] Save data imported successfully");
        }

        #endregion
    }
}
