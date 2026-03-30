namespace ClawRPG.Scripts.Systems.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Godot;

    /// <summary>
    /// Compares two combat logs and generates structured diff reports.
    /// Used to detect numerical regressions between code versions.
    /// </summary>
    public class CombatDiffReporter
    {
        private readonly float _alertThresholdPercent;

        public CombatDiffReporter(float alertThresholdPercent = 5.0f)
        {
            _alertThresholdPercent = alertThresholdPercent;
        }

        /// <summary>
        /// Generate a full diff report between two combat log versions.
        /// </summary>
        public CombatDiffReport GenerateDiff(CombatLogger oldLog, CombatLogger newLog)
        {
            var report = new CombatDiffReport
            {
                OldVersion = oldLog?.Version ?? "unknown",
                NewVersion = newLog?.Version ?? "unknown",
                Timestamp = DateTime.UtcNow.ToString("o")
            };

            if (oldLog == null || newLog == null)
            {
                report.Alerts.Add(new DiffAlert
                {
                    Severity = "error",
                    Field = "log",
                    Message = "One or both combat logs are null"
                });
                return report;
            }

            // Compare outcomes
            CompareOutcome(report, oldLog.Outcome, newLog.Outcome);

            // Compare events field by field
            CompareEvents(report, oldLog.Events, newLog.Events);

            // Compare actors
            CompareActors(report, oldLog.Player, newLog.Player, "player");
            CompareEnemyLists(report, oldLog.Enemies, newLog.Enemies);

            return report;
        }

        private void CompareOutcome(CombatDiffReport report, CombatOutcome oldOutcome, CombatOutcome newOutcome)
        {
            if (oldOutcome == null || newOutcome == null) return;

            AddFieldDiff(report, "outcome.won", oldOutcome.Won ? 1 : 0, newOutcome.Won ? 1 : 0);
            AddFieldDiff(report, "outcome.damage_taken", oldOutcome.DamageTaken, newOutcome.DamageTaken);
            AddFieldDiff(report, "outcome.rounds", oldOutcome.Rounds, newOutcome.Rounds);
        }

        private void CompareActors(CombatDiffReport report, CombatActorSnapshot oldActor, CombatActorSnapshot newActor, string prefix)
        {
            if (oldActor == null && newActor == null) return;
            if (oldActor == null || newActor == null)
            {
                report.Alerts.Add(new DiffAlert
                {
                    Severity = "warning",
                    Field = $"{prefix}.hp",
                    Message = $"Actor {prefix} missing in one version"
                });
                return;
            }

            AddFieldDiff(report, $"{prefix}.hp", oldActor.Hp, newActor.Hp);
            AddFieldDiff(report, $"{prefix}.attack", oldActor.Attack, newActor.Attack);
        }

        private void CompareEnemyLists(CombatDiffReport report, List<CombatActorSnapshot> oldEnemies, List<CombatActorSnapshot> newEnemies)
        {
            int maxCount = Math.Max(oldEnemies?.Count ?? 0, newEnemies?.Count ?? 0);
            for (int i = 0; i < maxCount; i++)
            {
                var oldE = i < (oldEnemies?.Count ?? 0) ? oldEnemies[i] : null;
                var newE = i < (newEnemies?.Count ?? 0) ? newEnemies[i] : null;
                string prefix = $"enemy[{i}]";
                CompareActors(report, oldE, newE, prefix);
            }
        }

        private void CompareEvents(CombatDiffReport report, List<CombatEvent> oldEvents, List<CombatEvent> newEvents)
        {
            int oldCount = oldEvents?.Count ?? 0;
            int newCount = newEvents?.Count ?? 0;
            AddFieldDiff(report, "events.count", oldCount, newCount);

            int maxEvents = Math.Max(oldCount, newCount);
            for (int i = 0; i < maxEvents; i++)
            {
                var oldE = i < oldCount ? oldEvents[i] : null;
                var newE = i < newCount ? newEvents[i] : null;
                CompareSingleEvent(report, oldE, newE, i);
            }
        }

        private void CompareSingleEvent(CombatDiffReport report, CombatEvent oldEvent, CombatEvent newEvent, int index)
        {
            string prefix = $"event[{index}]";

            if (oldEvent == null || newEvent == null)
            {
                report.Diffs.Add(new FieldDiff
                {
                    Field = $"{prefix}.exists",
                    OldValue = oldEvent != null ? 1 : 0,
                    NewValue = newEvent != null ? 1 : 0,
                    PercentChange = 100f
                });
                return;
            }

            if (oldEvent.Type != newEvent.Type)
            {
                report.Diffs.Add(new FieldDiff
                {
                    Field = $"{prefix}.type",
                    OldValue = oldEvent.Type?.GetHashCode() ?? 0,
                    NewValue = newEvent.Type?.GetHashCode() ?? 0,
                    PercentChange = 0f
                });
            }

            AddFieldDiff(report, $"{prefix}.damage", oldEvent.Damage, newEvent.Damage);
            AddFieldDiff(report, $"{prefix}.frame", oldEvent.Frame, newEvent.Frame);
            AddFieldDiff(report, $"{prefix}.duration", oldEvent.Duration, newEvent.Duration);
        }

        private void AddFieldDiff(CombatDiffReport report, string field, int oldVal, int newVal)
        {
            float pct = oldVal != 0 ? (float)(newVal - oldVal) / Math.Abs(oldVal) * 100f : (newVal != 0 ? 100f : 0f);

            report.Diffs.Add(new FieldDiff
            {
                Field = field,
                OldValue = oldVal,
                NewValue = newVal,
                PercentChange = pct
            });

            if (Math.Abs(pct) >= _alertThresholdPercent)
            {
                report.Alerts.Add(new DiffAlert
                {
                    Severity = Math.Abs(pct) >= _alertThresholdPercent * 2 ? "error" : "warning",
                    Field = field,
                    PercentChange = pct,
                    Message = $"{field} changed by {pct:+0.0;-0.0;0}% ({oldVal} → {newVal})"
                });
            }
        }

        /// <summary>
        /// Export report as a serializable dictionary.
        /// </summary>
        public Dictionary<string, object> ExportReport(CombatDiffReport report)
        {
            return new Dictionary<string, object>
            {
                { "old_version", report.OldVersion },
                { "new_version", report.NewVersion },
                { "timestamp", report.Timestamp },
                { "diff", report.Diffs.ConvertAll(d => d.Export()) },
                { "alerts", report.Alerts.ConvertAll(a => a.Export()) }
            };
        }

        /// <summary>
        /// Quick sanity check — returns true if any alerts with severity >= warning exist.
        /// </summary>
        public bool HasRegressions(CombatDiffReport report)
        {
            return report.Alerts.Any(a => a.Severity == "warning" || a.Severity == "error");
        }
    }

    /// <summary>
    /// The generated diff report.
    /// </summary>
    public class CombatDiffReport
    {
        public string OldVersion { get; set; }
        public string NewVersion { get; set; }
        public string Timestamp { get; set; }
        public List<FieldDiff> Diffs { get; set; } = new List<FieldDiff>();
        public List<DiffAlert> Alerts { get; set; } = new List<DiffAlert>();
    }

    /// <summary>
    /// A single field difference.
    /// </summary>
    public class FieldDiff
    {
        public string Field { get; set; }
        public int OldValue { get; set; }
        public int NewValue { get; set; }
        public float PercentChange { get; set; }

        public Dictionary<string, object> Export()
        {
            return new Dictionary<string, object>
            {
                { "field", Field ?? "" },
                { "old", OldValue },
                { "new", NewValue },
                { "pct", Math.Round(PercentChange, 1) }
            };
        }
    }

    /// <summary>
    /// An alert raised when a change exceeds the threshold.
    /// </summary>
    public class DiffAlert
    {
        public string Severity { get; set; }
        public string Field { get; set; }
        public float PercentChange { get; set; }
        public string Message { get; set; }

        public Dictionary<string, object> Export()
        {
            return new Dictionary<string, object>
            {
                { "severity", Severity ?? "" },
                { "field", Field ?? "" },
                { "pct", Math.Round(PercentChange, 1) },
                { "message", Message ?? "" }
            };
        }
    }
}
