using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class EconomicWarningData
{
    public List<WarningRecord> ActiveWarnings { get; set; } = new List<WarningRecord>();
    public List<WarningRecord> WarningHistory { get; set; } = new List<WarningRecord>();
    public EconomicStatistics Statistics { get; set; } = new EconomicStatistics();
    public List<string> AcknowledgedWarnings { get; set; } = new List<string>();
    public Dictionary<string, float> LastCheckTimes { get; set; } = new Dictionary<string, float>();
    public float LastFullCheckTime { get; set; } = 0f;
}

public class WarningRecord
{
    public string WarningId { get; set; }
    public string WarningType { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public WarningSeverity Severity { get; set; }
    public float Value { get; set; }
    public float Threshold { get; set; }
    public float Timestamp { get; set; }
    public bool IsActive { get; set; } = true;
    public string RecommendedAction { get; set; }
}

public enum WarningSeverity
{
    Info,
    Warning,
    Critical
}

public class EconomicStatistics
{
    public int TotalWarningsGenerated { get; set; }
    public int WarningsTriggered { get; set; }
    public int WarningsResolved { get; set; }
    public int CriticalWarnings { get; set; }
    public float AverageResolutionTime { get; set; }
    public Dictionary<string, int> WarningTypeCounts { get; set; } = new Dictionary<string, int>();
}
