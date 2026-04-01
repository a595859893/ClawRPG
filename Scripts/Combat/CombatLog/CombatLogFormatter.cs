using System;
using System.Collections.Generic;
using System.Text;
using Godot;
using Framework;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// CombatLogFormatter - 战斗日志格式化器
    /// 负责格式化日志消息，提供不同的样式、颜色、图标
    /// </summary>
    public partial class CombatLogFormatter : BaseSystem
    {
        private static CombatLogFormatter _instance;
        public static CombatLogFormatter Instance => _instance;

        /// <summary>
        /// Log format types
        /// </summary>
        public enum FormatType
        {
            Simple,      // 简单格式
            Detailed,    // 详细格式
            Compact,     // 紧凑格式
            Rich         // 富文本格式
        }

        // Format settings
        private FormatType _currentFormat = FormatType.Simple;
        private bool _showTimestamp = false;
        private bool _showType = false;
        private bool _useColor = true;
        private bool _useIcon = true;

        // Type-based colors (RRGGBB hex)
        private readonly Dictionary<CombatLogType, string> _typeColors = new Dictionary<CombatLogType, string>
        {
            { CombatLogType.Damage, "#ff6b6b" },
            { CombatLogType.Critical, "#ff0000" },
            { CombatLogType.Healing, "#51cf66" },
            { CombatLogType.Buff, "#4dabf7" },
            { CombatLogType.Debuff, "#ffa94d" },
            { CombatLogType.Kill, "#ffd43b" },
            { CombatLogType.Death, "#868e96" },
            { CombatLogType.SkillUsed, "#cc5de8" },
            { CombatLogType.ItemUsed, "#845ef7" },
            { CombatLogType.EnemySpawn, "#ff6b6b" },
            { CombatLogType.EnemyAggro, "#ffa94d" },
            { CombatLogType.Combo, "#ff922b" },
            { CombatLogType.Miss, "#adb5bd" },
            { CombatLogType.Block, "#74c0fc" },
            { CombatLogType.Dodge, "#20c997" },
            { CombatLogType.Parry, "#748ffc" },
            { CombatLogType.Shield, "#4dabf7" },
            { CombatLogType.Mana, "#9775fa" },
            { CombatLogType.Energy, "#ffd43b" },
            { CombatLogType.Experience, "#ffe066" },
            { CombatLogType.LevelUp, "#ff8787" },
            { CombatLogType.Info, "#ffffff" },
            { CombatLogType.Warning, "#ffa94d" },
            { CombatLogType.Error, "#ff6b6b" }
        };

        // Type-based icons
        private readonly Dictionary<CombatLogType, string> _typeIcons = new Dictionary<CombatLogType, string>
        {
            { CombatLogType.Damage, "⚔️" },
            { CombatLogType.Critical, "💥" },
            { CombatLogType.Healing, "💚" },
            { CombatLogType.Buff, "✨" },
            { CombatLogType.Debuff, "⛔" },
            { CombatLogType.Kill, "☠️" },
            { CombatLogType.Death, "💀" },
            { CombatLogType.SkillUsed, "🎯" },
            { CombatLogType.ItemUsed, "🎒" },
            { CombatLogType.EnemySpawn, "👹" },
            { CombatLogType.EnemyAggro, "👁️" },
            { CombatLogType.Combo, "🔥" },
            { CombatLogType.Miss, "❌" },
            { CombatLogType.Block, "🛡️" },
            { CombatLogType.Dodge, "💨" },
            { CombatLogType.Parry, "⚡" },
            { CombatLogType.Shield, "🔰" },
            { CombatLogType.Mana, "💎" },
            { CombatLogType.Energy, "⚡" },
            { CombatLogType.Experience, "⭐" },
            { CombatLogType.LevelUp, "🎉" },
            { CombatLogType.Info, "ℹ️" },
            { CombatLogType.Warning, "⚠️" },
            { CombatLogType.Error, "🚫" }
        };

        protected override void Initialize()
        {
            _instance = this;
            GD.Print("[CombatLogFormatter] Initialized");
        }

        #region Format Methods

        /// <summary>
        /// 设置格式类型
        /// </summary>
        public void SetFormatType(FormatType format)
        {
            _currentFormat = format;
        }

        /// <summary>
        /// 获取当前格式类型
        /// </summary>
        public FormatType GetFormatType()
        {
            return _currentFormat;
        }

        /// <summary>
        /// 格式化单条日志
        /// </summary>
        public string FormatLog(CombatLogEntry entry)
        {
            return _currentFormat switch
            {
                FormatType.Simple => FormatSimple(entry),
                FormatType.Detailed => FormatDetailed(entry),
                FormatType.Compact => FormatCompact(entry),
                FormatType.Rich => FormatRich(entry),
                _ => entry.Message
            };
        }

        /// <summary>
        /// 格式化多条日志
        /// </summary>
        public string FormatLogs(List<CombatLogEntry> entries)
        {
            var sb = new StringBuilder();

            foreach (var entry in entries)
            {
                sb.AppendLine(FormatLog(entry));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 简单格式 - 只返回消息
        /// </summary>
        private string FormatSimple(CombatLogEntry entry)
        {
            return entry.Message;
        }

        /// <summary>
        /// 详细格式 - 包含时间戳和类型
        /// </summary>
        private string FormatDetailed(CombatLogEntry entry)
        {
            var sb = new StringBuilder();

            if (_showTimestamp)
            {
                sb.Append($"[{entry.Timestamp:F1}s] ");
            }

            if (_showType)
            {
                sb.Append($"[{entry.Type}] ");
            }

            sb.Append(entry.Message);

            return sb.ToString();
        }

        /// <summary>
        /// 紧凑格式 - 包含图标和消息
        /// </summary>
        private string FormatCompact(CombatLogEntry entry)
        {
            if (_useIcon)
            {
                var icon = GetTypeIcon(entry.Type);
                return $"{icon} {entry.Message}";
            }
            return entry.Message;
        }

        /// <summary>
        /// 富文本格式 - 包含颜色和图标
        /// </summary>
        private string FormatRich(CombatLogEntry entry)
        {
            var sb = new StringBuilder();

            // Icon
            if (_useIcon)
            {
                var icon = GetTypeIcon(entry.Type);
                sb.Append(icon);
                sb.Append(" ");
            }

            // Message with color
            if (_useColor)
            {
                var color = GetTypeColor(entry.Type);
                sb.Append($"[color={color}]{entry.Message}[/color]");
            }
            else
            {
                sb.Append(entry.Message);
            }

            return sb.ToString();
        }

        #endregion

        #region Color and Icon Methods

        /// <summary>
        /// 获取类型对应的颜色
        /// </summary>
        public string GetTypeColor(CombatLogType type)
        {
            if (_typeColors.TryGetValue(type, out var color))
            {
                return color;
            }
            return "#ffffff";
        }

        /// <summary>
        /// 获取类型对应的图标
        /// </summary>
        public string GetTypeIcon(CombatLogType type)
        {
            if (_typeIcons.TryGetValue(type, out var icon))
            {
                return icon;
            }
            return "📝";
        }

        /// <summary>
        /// 设置自定义颜色
        /// </summary>
        public void SetTypeColor(CombatLogType type, string colorHex)
        {
            _typeColors[type] = colorHex;
        }

        /// <summary>
        /// 设置自定义图标
        /// </summary>
        public void SetTypeIcon(CombatLogType type, string icon)
        {
            _typeIcons[type] = icon;
        }

        #endregion

        #region Settings

        /// <summary>
        /// 设置是否显示时间戳
        /// </summary>
        public void SetShowTimestamp(bool show)
        {
            _showTimestamp = show;
        }

        /// <summary>
        /// 设置是否显示类型
        /// </summary>
        public void SetShowType(bool show)
        {
            _showType = show;
        }

        /// <summary>
        /// 设置是否使用颜色
        /// </summary>
        public void SetUseColor(bool use)
        {
            _useColor = use;
        }

        /// <summary>
        /// 设置是否使用图标
        /// </summary>
        public void SetUseIcon(bool use)
        {
            _useIcon = use;
        }

        /// <summary>
        /// 是否显示时间戳
        /// </summary>
        public bool IsShowTimestamp() => _showTimestamp;

        /// <summary>
        /// 是否显示类型
        /// </summary>
        public bool IsShowType() => _showType;

        /// <summary>
        /// 是否使用颜色
        /// </summary>
        public bool IsUseColor() => _useColor;

        /// <summary>
        /// 是否使用图标
        /// </summary>
        public bool IsUseIcon() => _useIcon;

        #endregion

        #region Data Persistence

        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();

            data["formatType"] = (int)_currentFormat;
            data["showTimestamp"] = _showTimestamp;
            data["showType"] = _showType;
            data["useColor"] = _useColor;
            data["useIcon"] = _useIcon;

            return data;
        }

        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            if (data.Contains("formatType"))
                _currentFormat = (FormatType)Convert.ToInt32(data["formatType"]);

            if (data.Contains("showTimestamp"))
                _showTimestamp = Convert.ToBoolean(data["showTimestamp"]);

            if (data.Contains("showType"))
                _showType = Convert.ToBoolean(data["showType"]);

            if (data.Contains("useColor"))
                _useColor = Convert.ToBoolean(data["useColor"]);

            if (data.Contains("useIcon"))
                _useIcon = Convert.ToBoolean(data["useIcon"]);

            GD.Print("[CombatLogFormatter] Save data imported successfully");
        }

        #endregion
    }
}
