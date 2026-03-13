using Godot;
using System;
using System.Collections.Generic;

public class TutorialData : Resource
{
    public Dictionary<string, bool> CompletedTutorials { get; set; } = new Dictionary<string, bool>();
    public List<string> InProgressTutorials { get; set; } = new List<string>();
    public Dictionary<string, int> TutorialProgress { get; set; } = new Dictionary<string, int>();
    public int TotalTutorialsCompleted { get; set; } = 0;
    public int TotalTutorialsViewed { get; set; } = 0;
    public DateTime LastTutorialTime { get; set; } = DateTime.Now;
    public Dictionary<string, DateTime> TutorialCompletionTimes { get; set; } = new Dictionary<string, DateTime>();
    public int HintsUsed { get; set; } = 0;
    public Dictionary<string, int> StepSkips { get; set; } = new Dictionary<string, int>();
}
