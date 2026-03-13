using Godot;
using System;
using System.Collections.Generic;

public class RandomNameData : Node
{
    // Name history for uniqueness tracking
    public List<string> GeneratedNames = new List<string>();
    
    // Statistics
    public int TotalGenerated = 0;
    public Dictionary<string, int> FirstNameCount = new Dictionary<string, int>();
    public Dictionary<string, int> LastNameCount = new Dictionary<string, int>();
    public Dictionary<string, int> CultureCount = new Dictionary<string, int>();
}
