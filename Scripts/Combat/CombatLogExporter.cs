using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Godot;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// Combat log exporter - handles exporting combat logs to various formats
    /// </summary>
    public partial class CombatLogExporter
    {
        private CombatLogSystem _logSystem;
        
        public CombatLogExporter(CombatLogSystem logSystem)
        {
            _logSystem = logSystem;
        }
        
        /// <summary>
        /// Export format types
        /// </summary>
        public enum ExportFormat
        {
            Text,
            CSV,
            JSON,
            HTML
        }
        
        /// <summary>
        /// Export logs to string
        /// </summary>
        public string ExportToString(ExportFormat format, int maxEntries = -1)
        {
            var entries = _logSystem.GetAllEntries();
            if (maxEntries > 0 && entries.Count > maxEntries)
            {
                entries = entries.GetRange(entries.Count - maxEntries, maxEntries);
            }
            
            return format switch
            {
                ExportFormat.Text => ExportToText(entries),
                ExportFormat.CSV => ExportToCSV(entries),
                ExportFormat.JSON => ExportToJSON(entries),
                ExportFormat.HTML => ExportToHTML(entries),
                _ => ExportToText(entries)
            };
        }
        
        /// <summary>
        /// Export logs to file
        /// </summary>
        public bool ExportToFile(string filePath, ExportFormat format)
        {
            try
            {
                var content = ExportToString(format);
                System.IO.File.WriteAllText(filePath, content);
                GD.Print($"[CombatLogExporter] Exported to {filePath}");
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"[CombatLogExporter] Export failed: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Export to plain text format
        /// </summary>
        private string ExportToText(List<CombatLogEntry> entries)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Combat Log ===");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            
            foreach (var entry in entries)
            {
                sb.AppendLine($"[{entry.Timestamp:F1}s] {entry.Message}");
            }
            
            // Add statistics
            var stats = _logSystem.GetStatistics();
            sb.AppendLine();
            sb.AppendLine("=== Statistics ===");
            sb.AppendLine($"Total Entries: {stats.TotalEntries}");
            sb.AppendLine($"Damage Dealt: {stats.TotalDamageDealt:F0}");
            sb.AppendLine($"Damage Taken: {stats.TotalDamageTaken:F0}");
            sb.AppendLine($"Healing: {stats.TotalHealing:F0}");
            sb.AppendLine($"Kills: {stats.KillEntries}");
            sb.AppendLine($"Critical Hits: {stats.CriticalHits}");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Export to CSV format
        /// </summary>
        private string ExportToCSV(List<CombatLogEntry> entries)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Timestamp,Type,Message,Value,Source,Target,IsPlayerAction");
            
            foreach (var entry in entries)
            {
                sb.AppendLine($"{entry.Timestamp:F1},{entry.Type},\"{EscapeCSV(entry.Message)}\",{entry.Value},\"{EscapeCSV(entry.Source)}\",\"{EscapeCSV(entry.Target)}\",{entry.IsPlayerAction}");
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Export to JSON format
        /// </summary>
        private string ExportToJSON(List<CombatLogEntry> entries)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"combatLog\": {");
            sb.AppendLine($"    \"exportTime\": \"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\",");
            sb.AppendLine("    \"entries\": [");
            
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                sb.AppendLine("      {");
                sb.AppendLine($"        \"timestamp\": {entry.Timestamp:F1},");
                sb.AppendLine($"        \"type\": \"{entry.Type}\",");
                sb.AppendLine($"        \"message\": \"{EscapeJSON(entry.Message)}\",");
                sb.AppendLine($"        \"value\": {entry.Value},");
                sb.AppendLine($"        \"source\": \"{EscapeJSON(entry.Source)}\",");
                sb.AppendLine($"        \"target\": \"{EscapeJSON(entry.Target)}\",");
                sb.AppendLine($"        \"isPlayerAction\": {entry.IsPlayerAction.ToString().ToLower()}");
                sb.Append("      }");
                if (i < entries.Count - 1) sb.AppendLine(",");
                else sb.AppendLine();
            }
            
            sb.AppendLine("    ],");
            
            // Add statistics
            var stats = _logSystem.GetStatistics();
            sb.AppendLine("    \"statistics\": {");
            sb.AppendLine($"      \"totalEntries\": {stats.TotalEntries},");
            sb.AppendLine($"      \"damageEntries\": {stats.DamageEntries},");
            sb.AppendLine($"      \"healingEntries\": {stats.HealingEntries},");
            sb.AppendLine($"      \"killEntries\": {stats.KillEntries},");
            sb.AppendLine($"      \"criticalHits\": {stats.CriticalHits},");
            sb.AppendLine($"      \"totalDamageDealt\": {stats.TotalDamageDealt:F1},");
            sb.AppendLine($"      \"totalDamageTaken\": {stats.TotalDamageTaken:F1},");
            sb.AppendLine($"      \"totalHealing\": {stats.TotalHealing:F1}");
            sb.AppendLine("    }");
            sb.AppendLine("  }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Export to HTML format
        /// </summary>
        private string ExportToHTML(List<CombatLogEntry> entries)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head>");
            sb.AppendLine("<meta charset=\"utf-8\">");
            sb.AppendLine("<title>Combat Log</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: monospace; background: #1a1a1a; color: #eee; padding: 20px; }");
            sb.AppendLine("table { border-collapse: collapse; width: 100%; }");
            sb.AppendLine("th, td { border: 1px solid #444; padding: 8px; text-align: left; }");
            sb.AppendLine("th { background: #333; }");
            sb.AppendLine(".damage { color: #ff6b6b; }");
            sb.AppendLine(".healing { color: #51cf66; }");
            sb.AppendLine(".critical { color: #ffd43b; }");
            sb.AppendLine(".info { color: #74c0fc; }");
            sb.AppendLine("</style></head><body>");
            sb.AppendLine("<h1>Combat Log</h1>");
            sb.AppendLine($"<p>Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Time</th><th>Type</th><th>Message</th><th>Value</th></tr>");
            
            foreach (var entry in entries)
            {
                string cssClass = GetTypeCSSClass(entry.Type);
                sb.AppendLine($"<tr><td>{entry.Timestamp:F1}s</td><td>{entry.Type}</td><td class=\"{cssClass}\">{System.Web.HttpUtility.HtmlEncode(entry.Message)}</td><td>{entry.Value:F0}</td></tr>");
            }
            
            sb.AppendLine("</table>");
            sb.AppendLine("</body></html>");
            
            return sb.ToString();
        }
        
        private string GetTypeCSSClass(CombatLogType type)
        {
            return type switch
            {
                CombatLogType.Damage or CombatLogType.Critical => "damage",
                CombatLogType.Healing => "healing",
                CombatLogType.Critical => "critical",
                _ => "info"
            };
        }
        
        private string EscapeCSV(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Replace("\"", "\"\"");
        }
        
        private string EscapeJSON(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r");
        }
    }
}
