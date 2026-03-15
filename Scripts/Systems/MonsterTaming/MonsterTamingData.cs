using Godot;
using System;
using System.Collections.Generic;

public class MonsterTamingData : BaseSystem
{
    // Tamed Monsters
    public Dictionary<int, TamedMonster> TamedMonsters = new Dictionary<int, TamedMonster>();
    
    // Capture Attempts
    public int TotalCaptureAttempts = 0;
    public int SuccessfulCaptures = 0;
    
    // Statistics
    public int TotalMonstersTamed = 0;
    public int LegendaryCaptures = 0;
    public int EpicCaptures = 0;
    public int RareCaptures = 0;
    public int UncommonCaptures = 0;
    public int CommonCaptures = 0;
    
    // Active Capture
    public bool IsCapturing = false;
    public int CapturingMonsterId = -1;
    public float CaptureProgress = 0f;
    
    public class TamedMonster
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Rarity { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int BondLevel { get; set; }
        public int BattlesWon { get; set; }
        public DateTime TamedAt { get; set; }
        public Dictionary<string, int> Stats { get; set; } = new Dictionary<string, int>();
    }
}
