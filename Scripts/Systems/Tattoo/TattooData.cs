using Godot;
using System;
using System.Collections.Generic;

public partial class TattooData : Resource
{
    [Export] public Dictionary<string, bool> UnlockedTattoos = new Dictionary<string, bool>();
    [Export] public Dictionary<string, string> AppliedTattoos = new Dictionary<string, string>(); // slot -> tattoo_id
    [Export] public List<string> TattooHistory = new List<string>();
    [Export] public int TotalTattoosApplied = 0;
    [Export] public int TotalGoldSpent = 0;
    [Export] public Dictionary<string, int> TattooUsageCount = new Dictionary<string, int>(); // tattoo_id -> count
}
