using System;
using System.Collections.Generic;

[Serializable]
public class DailyPuzzleData
{
    // Puzzle types
    public enum PuzzleType
    {
        Math,
        Pattern,
        Memory,
        Word,
        Riddle
    }
    
    // Puzzle data
    public Dictionary<int, PuzzleRecord> SolvedPuzzles { get; set; } = new Dictionary<int, PuzzleRecord>();
    public int CurrentDailyPuzzleId { get; set; } = -1;
    public DateTime LastPuzzleDate { get; set; } = DateTime.MinValue;
    public int CurrentStreak { get; set; } = 0;
    public int BestStreak { get; set; } = 0;
    public int TotalSolved { get; set; } = 0;
    public int TotalFailed { get; set; } = 0;
    public int HintsUsed { get; set; } = 0;
    public int TotalGoldEarned { get; set; } = 0;
    public int TotalExpEarned { get; set; } = 0;
    public List<int> SolvedPuzzleIds { get; set; } = new List<int>();
}

[Serializable]
public class PuzzleRecord
{
    public int PuzzleId { get; set; }
    public DateTime SolvedDate { get; set; }
    public int TimeTakenSeconds { get; set; }
    public int HintsUsed { get; set; }
    public bool UsedBonusTime { get; set; }
    public int GoldEarned { get; set; }
    public int ExpEarned { get; set; }
}
