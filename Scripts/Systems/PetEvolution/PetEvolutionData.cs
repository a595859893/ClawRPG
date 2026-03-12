using System;
using System.Collections.Generic;

public class PetEvolutionData
{
    // Pet evolution data tracking
    public Dictionary<int, PetEvolutionRecord> EvolvedPets { get; set; } = new Dictionary<int, PetEvolutionRecord>();
    
    // Evolution history
    public List<EvolutionHistoryEntry> EvolutionHistory { get; set; } = new List<EvolutionHistoryEntry>();
    
    // Statistics
    public int TotalEvolutions { get; set; }
    public int LegendaryEvolutions { get; set; }
    public int EpicEvolutions { get; set; }
    public int RareEvolutions { get; set; }
    public int TotalEvolutionPoints { get; set; }
}

public class PetEvolutionRecord
{
    public int PetId { get; set; }
    public string CurrentForm { get; set; } = "";
    public string TargetForm { get; set; } = "";
    public int EvolutionPoints { get; set; }
    public int RequiredPoints { get; set; }
    public bool IsEvolved { get; set; }
    public DateTime LastEvolutionTime { get; set; }
}

public class EvolutionHistoryEntry
{
    public int PetId { get; set; }
    public string OriginalForm { get; set; } = "";
    public string NewForm { get; set; } = "";
    public string EvolutionType { get; set; } = "";
    public DateTime EvolutionTime { get; set; }
    public int PointsUsed { get; set; }
}
