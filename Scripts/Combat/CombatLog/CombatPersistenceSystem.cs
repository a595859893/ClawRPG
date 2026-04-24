using System;
using System.Collections.Generic;
using Godot;
using Framework;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// CombatPersistenceSystem - 战斗数据持久化系统
    /// 负责保存和加载战斗相关数据：统计、设置、历史记录
    /// </summary>
    public partial class CombatPersistenceSystem : BaseSystem
    {
        private static CombatPersistenceSystem _instance;
        public static CombatPersistenceSystem Instance => _instance;

        // Statistics
        private CombatLogStatistics _statistics = new CombatLogStatistics();

        // Filter settings
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

        // Configuration
        private int _maxEntries = 500;
        private float _autoClearTime = 300f;

        protected override void Initialize()
        {
            _instance = this;
            GD.Print("[CombatPersistenceSystem] Initialized");
        }

        public override void _Process(double delta)
        {
            _comboTimer -= delta;
            _killStreakTimer -= delta;

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

        #region Statistics Methods

        /// <summary>
        /// Record damage entry
        /// </summary>
        public void RecordDamage(float damage, bool isCritical, bool isPlayerSource)
        {
            _statistics.DamageEntries++;
            if (isCritical) _statistics.CriticalHits++;

            if (isPlayerSource)
            {
                _statistics.TotalDamageDealt += damage;
            }
            else
            {
                _statistics.TotalDamageTaken += damage;
            }
        }

        /// <summary>
        /// Record healing
        /// </summary>
        public void RecordHealing(float amount)
        {
            _statistics.HealingEntries++;
            _statistics.TotalHealing += amount;
        }

        /// <summary>
        /// Record kill
        /// </summary>
        public void RecordKill()
        {
            _statistics.KillEntries++;
        }

        /// <summary>
        /// Record miss
        /// </summary>
        public void RecordMiss()
        {
            _statistics.Misses++;
        }

        /// <summary>
        /// Record block
        /// </summary>
        public void RecordBlock()
        {
            _statistics.Blocks++;
        }

        /// <summary>
        /// Record dodge
        /// </summary>
        public void RecordDodge()
        {
            _statistics.Dodges++;
        }

        /// <summary>
        /// Increment total entries
        /// </summary>
        public void IncrementTotalEntries()
        {
            _statistics.TotalEntries++;
        }

        /// <summary>
        /// Get statistics
        /// </summary>
        public CombatLogStatistics GetStatistics()
        {
            return _statistics;
        }

        /// <summary>
        /// Reset statistics
        /// </summary>
        public void ResetStatistics()
        {
            _statistics.Reset();
        }

        #endregion

        #region Combo Methods

        /// <summary>
        /// Add combo hit
        /// </summary>
        public void AddCombo(int hits)
        {
            _currentCombo += hits;
            _comboTimer = _comboTimeWindow;
        }

        /// <summary>
        /// Get current combo
        /// </summary>
        public int GetCurrentCombo()
        {
            return _currentCombo;
        }

        /// <summary>
        /// Set combo time window
        /// </summary>
        public void SetComboTimeWindow(float window)
        {
            _comboTimeWindow = window;
        }

        /// <summary>
        /// Check if combo is active
        /// </summary>
        public bool IsComboActive()
        {
            return _currentCombo > 0 && _comboTimer > 0;
        }

        #endregion

        #region Kill Streak Methods

        /// <summary>
        /// Add kill to streak
        /// </summary>
        public void AddKillStreak(string target)
        {
            _killStreak++;
            _killStreakTimer = 5f;

            _recentKills.Add(target);
            if (_recentKills.Count > 10)
            {
                _recentKills.RemoveAt(0);
            }
        }

        /// <summary>
        /// Get kill streak
        /// </summary>
        public int GetKillStreak()
        {
            return _killStreak;
        }

        /// <summary>
        /// Get recent kills
        /// </summary>
        public List<string> GetRecentKills()
        {
            return new List<string>(_recentKills);
        }

        #endregion

        #region Filter Methods

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
        /// Get damage filter
        /// </summary>
        public bool IsShowDamage() => _showDamage;

        /// <summary>
        /// Get healing filter
        /// </summary>
        public bool IsShowHealing() => _showHealing;

        /// <summary>
        /// Get buff filter
        /// </summary>
        public bool IsShowBuffs() => _showBuffs;

        /// <summary>
        /// Get skill filter
        /// </summary>
        public bool IsShowSkills() => _showSkills;

        /// <summary>
        /// Get combat filter
        /// </summary>
        public bool IsShowCombat() => _showCombat;

        /// <summary>
        /// Get info filter
        /// </summary>
        public bool IsShowInfo() => _showInfo;

        /// <summary>
        /// Get player only filter
        /// </summary>
        public bool IsPlayerOnly() => _playerOnly;

        /// <summary>
        /// Get enemy only filter
        /// </summary>
        public bool IsEnemyOnly() => _enemyOnly;

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
        /// Get max entries
        /// </summary>
        public int GetMaxEntries()
        {
            return _maxEntries;
        }

        /// <summary>
        /// Set auto clear time
        /// </summary>
        public void SetAutoClearTime(float time)
        {
            _autoClearTime = time;
        }

        /// <summary>
        /// Get auto clear time
        /// </summary>
        public float GetAutoClearTime()
        {
            return _autoClearTime;
        }

        #endregion

        #region Filter Check

        /// <summary>
        /// Check if entry should be included based on filters
        /// </summary>
        public bool ShouldIncludeEntry(CombatLogEntry entry)
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

            return include;
        }

        #endregion

        #region Reset

        /// <summary>
        /// Full reset
        /// </summary>
        public void FullReset()
        {
            _statistics.Reset();
            ClearFilters();
            _currentCombo = 0;
            _comboTimer = 0f;
            _killStreak = 0;
            _killStreakTimer = 0f;
            _recentKills.Clear();
        }

        #endregion

        #region Data Persistence

        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();

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
                { "comboTimer", _comboTimer },
                { "comboTimeWindow", _comboTimeWindow }
            };
            data["combo"] = combo;

            // 击杀streak
            var killStreak = new Dictionary
            {
                { "killStreak", _killStreak },
                { "killStreakTimer", _killStreakTimer }
            };
            data["killStreak"] = killStreak;

            // 最近击杀
            data["recentKills"] = new ArrayList(_recentKills);

            // 配置
            var config = new Dictionary
            {
                { "maxEntries", _maxEntries },
                { "autoClearTime", _autoClearTime }
            };
            data["config"] = config;

            return data;
        }

        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            // 恢复统计信息
            if (data.ContainsKey("statistics"))
            {
                var stats = data["statistics"] as Dictionary;
                if (stats != null)
                {
                    if (stats.ContainsKey("totalEntries")) _statistics.TotalEntries = Convert.ToInt32(stats["totalEntries"]);
                    if (stats.ContainsKey("damageEntries")) _statistics.DamageEntries = Convert.ToInt32(stats["damageEntries"]);
                    if (stats.ContainsKey("healingEntries")) _statistics.HealingEntries = Convert.ToInt32(stats["healingEntries"]);
                    if (stats.ContainsKey("killEntries")) _statistics.KillEntries = Convert.ToInt32(stats["killEntries"]);
                    if (stats.ContainsKey("criticalHits")) _statistics.CriticalHits = Convert.ToInt32(stats["criticalHits"]);
                    if (stats.ContainsKey("misses")) _statistics.Misses = Convert.ToInt32(stats["misses"]);
                    if (stats.ContainsKey("blocks")) _statistics.Blocks = Convert.ToInt32(stats["blocks"]);
                    if (stats.ContainsKey("dodges")) _statistics.Dodges = Convert.ToInt32(stats["dodges"]);
                    if (stats.ContainsKey("totalDamageDealt")) _statistics.TotalDamageDealt = Convert.ToSingle(stats["totalDamageDealt"]);
                    if (stats.ContainsKey("totalDamageTaken")) _statistics.TotalDamageTaken = Convert.ToSingle(stats["totalDamageTaken"]);
                    if (stats.ContainsKey("totalHealing")) _statistics.TotalHealing = Convert.ToSingle(stats["totalHealing"]);
                }
            }

            // 恢复筛选器设置
            if (data.ContainsKey("filters"))
            {
                var filters = data["filters"] as Dictionary;
                if (filters != null)
                {
                    if (filters.ContainsKey("showDamage")) _showDamage = Convert.ToBoolean(filters["showDamage"]);
                    if (filters.ContainsKey("showHealing")) _showHealing = Convert.ToBoolean(filters["showHealing"]);
                    if (filters.ContainsKey("showBuffs")) _showBuffs = Convert.ToBoolean(filters["showBuffs"]);
                    if (filters.ContainsKey("showSkills")) _showSkills = Convert.ToBoolean(filters["showSkills"]);
                    if (filters.ContainsKey("showCombat")) _showCombat = Convert.ToBoolean(filters["showCombat"]);
                    if (filters.ContainsKey("showInfo")) _showInfo = Convert.ToBoolean(filters["showInfo"]);
                    if (filters.ContainsKey("playerOnly")) _playerOnly = Convert.ToBoolean(filters["playerOnly"]);
                    if (filters.ContainsKey("enemyOnly")) _enemyOnly = Convert.ToBoolean(filters["enemyOnly"]);
                }
            }

            // 恢复连击状态
            if (data.ContainsKey("combo"))
            {
                var combo = data["combo"] as Dictionary;
                if (combo != null)
                {
                    if (combo.ContainsKey("currentCombo")) _currentCombo = Convert.ToInt32(combo["currentCombo"]);
                    if (combo.ContainsKey("comboTimer")) _comboTimer = Convert.ToSingle(combo["comboTimer"]);
                    if (combo.ContainsKey("comboTimeWindow")) _comboTimeWindow = Convert.ToSingle(combo["comboTimeWindow"]);
                }
            }

            // 恢复击杀streak
            if (data.ContainsKey("killStreak"))
            {
                var killStreak = data["killStreak"] as Dictionary;
                if (killStreak != null)
                {
                    if (killStreak.ContainsKey("killStreak")) _killStreak = Convert.ToInt32(killStreak["killStreak"]);
                    if (killStreak.ContainsKey("killStreakTimer")) _killStreakTimer = Convert.ToSingle(killStreak["killStreakTimer"]);
                }
            }

            // 恢复最近击杀
            if (data.ContainsKey("recentKills"))
            {
                _recentKills.Clear();
                var kills = data["recentKills"] as ArrayList;
                if (kills != null)
                {
                    foreach (string kill in kills)
                    {
                        _recentKills.Add(kill);
                    }
                }
            }

            // 恢复配置
            if (data.ContainsKey("config"))
            {
                var config = data["config"] as Dictionary;
                if (config != null)
                {
                    if (config.ContainsKey("maxEntries")) _maxEntries = Convert.ToInt32(config["maxEntries"]);
                    if (config.ContainsKey("autoClearTime")) _autoClearTime = Convert.ToSingle(config["autoClearTime"]);
                }
            }

            GD.Print("[CombatPersistenceSystem] Save data imported successfully");
        }

        #endregion
    }
}
