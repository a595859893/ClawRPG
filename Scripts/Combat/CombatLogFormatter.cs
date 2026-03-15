using Godot;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClawRPG.Scripts.Combat {
    /// <summary>
    /// 战斗日志格式化器 - 负责格式化日志输出
    /// </summary>
    public partial class CombatLogFormatter : BaseSystem {
        
        /// <summary>
        /// 日志格式类型
        /// </summary>
        public enum FormatType {
            Simple,
            Detailed,
            Compact,
            Rich
        }
        
        private FormatType _currentFormat = FormatType.Simple;
        private bool _showTimestamp = true;
        private bool _showType = true;
        
        public override void _Ready() {
            base._Ready();
        }
        
        /// <summary>
        /// 设置格式类型
        /// </summary>
        public void SetFormatType(FormatType format) {
            _currentFormat = format;
        }
        
        /// <summary>
        /// 格式化单条日志
        /// </summary>
        public string FormatLog(CombatLogEntry entry) {
            switch (_currentFormat) {
                case FormatType.Simple:
                    return FormatSimple(entry);
                case FormatType.Detailed:
                    return FormatDetailed(entry);
                case FormatType.Compact:
                    return FormatCompact(entry);
                case FormatType.Rich:
                    return FormatRich(entry);
                default:
                    return entry.Message;
            }
        }
        
        /// <summary>
        /// 格式化多条日志
        /// </summary>
        public string FormatLogs(List<CombatLogEntry> entries) {
            var sb = new StringBuilder();
            
            foreach (var entry in entries) {
                sb.AppendLine(FormatLog(entry));
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 简单格式
        /// </summary>
        private string FormatSimple(CombatLogEntry entry) {
            return entry.Message;
        }
        
        /// <summary>
        /// 详细格式
        /// </summary>
        private string FormatDetailed(CombatLogEntry entry) {
            var sb = new StringBuilder();
            
            if (_showTimestamp) {
                var time = Time.GetDatetimeStringFromUnixTime((long)entry.Timestamp);
                sb.Append($"[{time}] ");
            }
            
            if (_showType) {
                sb.Append($"[{entry.Type}] ");
            }
            
            sb.Append(entry.Message);
            
            if (entry.Data != null && entry.Data.Count > 0) {
                sb.Append($" | {FormatData(entry.Data)}");
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 紧凑格式
        /// </summary>
        private string FormatCompact(CombatLogEntry entry) {
            var typeIcon = GetTypeIcon(entry.Type);
            return $"{typeIcon} {entry.Message}";
        }
        
        /// <summary>
        /// 富文本格式
        /// </summary>
        private string FormatRich(CombatLogEntry entry) {
            var color = GetTypeColor(entry.Type);
            var typeIcon = GetTypeIcon(entry.Type);
            return $"[color={color}]{typeIcon} {entry.Message}[/color]";
        }
        
        /// <summary>
        /// 格式化数据
        /// </summary>
        private string FormatData(Dictionary data) {
            var parts = new List<string>();
            
            foreach (var key in data.Keys) {
                parts.Add($"{key}={data[key]}");
            }
            
            return string.Join(", ", parts);
        }
        
        /// <summary>
        /// 获取类型图标
        /// </summary>
        private string GetTypeIcon(CombatLogType type) {
            return type switch {
                CombatLogType.Damage => "⚔️",
                CombatLogType.Heal => "💚",
                CombatLogType.Skill => "✨",
                CombatLogType.Death => "💀",
                CombatLogType.System => "⚙️",
                CombatLogType.Buff => "🔰",
                CombatLogType.Debuff => "❌",
                CombatLogType.Victory => "🏆",
                CombatLogType.Defeat => "😵",
                _ => "📝"
            };
        }
        
        /// <summary>
        /// 获取类型颜色
        /// </summary>
        private string GetTypeColor(CombatLogType type) {
            return type switch {
                CombatLogType.Damage => "#ff6b6b",
                CombatLogType.Heal => "#51cf66",
                CombatLogType.Skill => "#cc5de8",
                CombatLogType.Death => "#868e96",
                CombatLogType.System => "#ced4da",
                CombatLogType.Buff => "#4dabf7",
                CombatLogType.Debuff => "#ffa94d",
                CombatLogType.Victory => "#ffd43b",
                CombatLogType.Defeat => "#ff8787",
                _ => "#ffffff"
            };
        }
        
        /// <summary>
        /// 设置是否显示时间戳
        /// </summary>
        public void SetShowTimestamp(bool show) {
            _showTimestamp = show;
        }
        
        /// <summary>
        /// 设置是否显示类型
        /// </summary>
        public void SetShowType(bool show) {
            _showType = show;
        }
        
        public override Dictionary ExportSaveData() {
            var data = new Dictionary();
            data["formatType"] = (int)_currentFormat;
            data["showTimestamp"] = _showTimestamp;
            data["showType"] = _showType;
            return data;
        }
        
        public override void ImportSaveData(Dictionary data) {
            if (data.Contains("formatType")) {
                _currentFormat = (FormatType)(int)data["formatType"];
            }
            if (data.Contains("showTimestamp")) {
                _showTimestamp = (bool)data["showTimestamp"];
            }
            if (data.Contains("showType")) {
                _showType = (bool)data["showType"];
            }
        }
    }
}
