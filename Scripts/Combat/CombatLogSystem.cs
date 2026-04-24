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
    public partial class CombatLogSystem : BaseSystem
    {
        private static CombatLogSystem _instance;
        public static CombatLogSystem Instance => _instance;

        // 子系统引用
        private NodePath _recorderPath = new NodePath("../CombatLog/CombatLogRecorder");
        private NodePath _formatterPath = new NodePath("../CombatLog/CombatLogFormatter");
        private NodePath _persistencePath = new NodePath("../CombatLog/CombatPersistenceSystem");

        private CombatLogRecorder _recorder;
        private CombatLogFormatter _formatter;
        private CombatPersistenceSystem _persistence;

        // 本地存储（仅协调用）
        private List<CombatLogEntry> _logEntries = new List<CombatLogEntry>();
        private List<CombatLogEntry> _filteredEntries = new List<CombatLogEntry>();

        // Configuration
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

            _recorder = GetNodeOrNull<CombatLogRecorder>(_recorderPath);
            _formatter = GetNodeOrNull<CombatLogFormatter>(_formatterPath);
            _persistence = GetNodeOrNull<CombatPersistenceSystem>(_persistencePath);

            if (_recorder == null) { _recorder = new CombatLogRecorder(); _recorder.Name = "CombatLogRecorder"; AddChild(_recorder); }
            if (_formatter == null) { _formatter = new CombatLogFormatter(); _formatter.Name = "CombatLogFormatter"; AddChild(_formatter); }
            if (_persistence == null) { _persistence = new CombatPersistenceSystem(); _persistence.Name = "CombatPersistenceSystem"; AddChild(_persistence); }

            GD.Print("[CombatLogSystem] Combat Log System initialized as coordinator");
        }

        public override void _Process(double delta)
        {
            _currentSessionTime += delta;
            ApplyFilters();

            if (_logEntries.Count > _maxEntries)
                _logEntries.RemoveAt(0);
        }

        // ── Entry Management ─────────────────────────────────────────────────

        private void AddEntry(CombatLogEntry entry)
        {
            _logEntries.Add(entry);
            EmitSignal(SignalNewEntry, entry);
            CheckComboMilestone();
        }

        private void ApplyFilters()
        {
            _filteredEntries.Clear();
            foreach (var entry in _logEntries)
            {
                if (_persistence != null && _persistence.ShouldIncludeEntry(entry))
                    _filteredEntries.Add(entry);
                else if (_persistence == null)
                    _filteredEntries.Add(entry);
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
                    AddEntry(new CombatLogEntry
                    {
                        Timestamp = _currentSessionTime,
                        Type = CombatLogType.Combo,
                        Message = $"💥 Combo x{milestone}!",
                        Value = milestone,
                        IsPlayerAction = true
                    });
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
                AddEntry(new CombatLogEntry
                {
                    Timestamp = _currentSessionTime,
                    Type = CombatLogType.Combo,
                    Message = $"🔥 击杀 streak x{streak}!",
                    Value = streak,
                    IsPlayerAction = true
                });
            }
        }

        // ── Getters ──────────────────────────────────────────────────────────

        public List<CombatLogEntry> GetAllEntries() => new List<CombatLogEntry>(_logEntries);
        public List<CombatLogEntry> GetFilteredEntries() => new List<CombatLogEntry>(_filteredEntries);

        public List<CombatLogEntry> GetRecentEntries(int count = 20)
        {
            int start = Math.Max(0, _filteredEntries.Count - count);
            int length = Math.Min(count, _filteredEntries.Count - start);
            if (length <= 0) return new List<CombatLogEntry>();
            return _filteredEntries.GetRange(start, length);
        }

        public List<CombatLogEntry> GetEntriesByType(CombatLogType type)
        {
            var result = new List<CombatLogEntry>();
            foreach (var entry in _logEntries)
                if (entry.Type == type) result.Add(entry);
            return result;
        }

        public int GetCurrentCombo() => _persistence != null ? _persistence.GetCurrentCombo() : 0;
        public int GetKillStreak() => _persistence != null ? _persistence.GetKillStreak() : 0;
        public CombatLogStatistics GetStatistics() => _persistence != null ? _persistence.GetStatistics() : new CombatLogStatistics();
        public float GetSessionTime() => _currentSessionTime;
        public List<string> GetRecentKills() => _persistence != null ? _persistence.GetRecentKills() : new List<string>();
        public CombatLogFormatter GetFormatter() => _formatter;

        // ── Filter Control ───────────────────────────────────────────────────

        public void SetShowDamage(bool show) => _persistence?.SetShowDamage(show);
        public void SetShowHealing(bool show) => _persistence?.SetShowHealing(show);
        public void SetShowBuffs(bool show) => _persistence?.SetShowBuffs(show);
        public void SetShowSkills(bool show) => _persistence?.SetShowSkills(show);
        public void SetShowCombat(bool show) => _persistence?.SetShowCombat(show);
        public void SetShowInfo(bool show) => _persistence?.SetShowInfo(show);
        public void SetPlayerOnly(bool playerOnly) => _persistence?.SetPlayerOnly(playerOnly);
        public void SetEnemyOnly(bool enemyOnly) => _persistence?.SetEnemyOnly(enemyOnly);
        public void ClearFilters() => _persistence?.ClearFilters();

        public void ClearLog()
        {
            _logEntries.Clear();
            _filteredEntries.Clear();
            _recorder?.ClearLog();
        }

        public void ResetStatistics() => _persistence?.ResetStatistics();

        public void ResetSession()
        {
            ClearLog();
            ResetStatistics();
            _currentSessionTime = 0f;
            _persistence?.FullReset();
        }

        // ── Persistence ──────────────────────────────────────────────────────

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>
            {
                ["sessionTime"] = _currentSessionTime,
                ["maxEntries"] = _maxEntries
            };
            if (_persistence != null) data["persistence"] = _persistence.ExportSaveData();
            if (_recorder != null) data["recorder"] = _recorder.ExportSaveData();
            if (_formatter != null) data["formatter"] = _formatter.ExportSaveData();
            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            if (data.ContainsKey("sessionTime")) _currentSessionTime = Convert.ToSingle(data["sessionTime"]);
            if (data.ContainsKey("maxEntries")) _maxEntries = Convert.ToInt32(data["maxEntries"]);

            if (data.ContainsKey("persistence") && _persistence != null)
                _persistence.ImportSaveData(data["persistence"] as Dictionary);
            if (data.ContainsKey("recorder") && _recorder != null)
                _recorder.ImportSaveData(data["recorder"] as Dictionary);
            if (data.ContainsKey("formatter") && _formatter != null)
                _formatter.ImportSaveData(data["formatter"] as Dictionary);

            if (_recorder != null) _logEntries = _recorder.GetAllEntries();
            ApplyFilters();
            GD.Print("[CombatLogSystem] Save data imported successfully");
        }
    }
}
