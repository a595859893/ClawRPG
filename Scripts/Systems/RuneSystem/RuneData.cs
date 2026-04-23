using Godot;
using System.Collections.Generic;

public partial class RuneData : Godot.Resource
{
    public Dictionary<string, int> UnlockedRunes = new Dictionary<string, int>();
    
    public Dictionary<string, string> EquippedRunes = new Dictionary<string, string>();
    
    public List<RuneHistoryEntry> RuneHistory = new List<RuneHistoryEntry>();
    
    public RuneStatistics Statistics = new RuneStatistics();
}

public class RuneHistoryEntry
{
    public string RuneId { get; set; }
    public string Action { get; set; }
    public int Level { get; set; }
    public long Timestamp { get; set; }
}

public class RuneStatistics
{
    public int TotalRunesUnlocked { get; set; }
    public int TotalRunesEquipped { get; set; }
    public int TotalGoldSpent { get; set; }
    public int TotalExpGained { get; set; }
    public int TimesEnhanced { get; set; }
    public int TimesRemoved { get; set; }
}
