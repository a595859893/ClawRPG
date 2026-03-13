using System;
using System.Collections.Generic;
using System.Linq;
using System.Random = System.Random;

public class DailyPuzzleSystem
{
    private static DailyPuzzleData _data;
    private static Random _random = new Random();
    
    // Initialize the system
    public static void Initialize()
    {
        _data = new DailyPuzzleData();
        LoadData();
    }
    
    // Get daily puzzle based on date
    public static PuzzleConfig GetDailyPuzzle()
    {
        DateTime today = DateTime.Today;
        
        // Check if we need a new daily puzzle
        if (_data.LastPuzzleDate.Date != today)
        {
            // Generate new daily puzzle ID based on date
            int dayOfYear = today.DayOfYear;
            int year = today.Year;
            int puzzleId = (dayOfYear + year) % 45 + 1; // 45 puzzles in database
            
            // Make sure puzzle ID is valid
            if (puzzleId < 1) puzzleId = 1;
            if (puzzleId > 45) puzzleId = 45;
            
            _data.CurrentDailyPuzzleId = puzzleId;
            _data.LastPuzzleDate = today;
            
            // Check streak
            if (_data.LastPuzzleDate.Date == today.AddDays(-1).Date && _data.SolvedPuzzles.ContainsKey(_data.CurrentDailyPuzzleId))
            {
                // Continue streak
            }
            else if (_data.LastPuzzleDate.Date != today.AddDays(-1).Date)
            {
                // Reset streak if missed a day
                _data.CurrentStreak = 0;
            }
            
            SaveData();
        }
        
        return DailyPuzzleDatabase.GetPuzzleConfig(_data.CurrentDailyPuzzleId);
    }
    
    // Get current puzzle ID
    public static int GetCurrentPuzzleId()
    {
        return _data.CurrentDailyPuzzleId;
    }
    
    // Check if today's puzzle is solved
    public static bool IsTodayPuzzleSolved()
    {
        return _data.SolvedPuzzles.ContainsKey(_data.CurrentDailyPuzzleId);
    }
    
    // Solve puzzle
    public static PuzzleRecord SolvePuzzle(string answer, int timeTakenSeconds, int hintsUsed)
    {
        PuzzleConfig puzzle = GetDailyPuzzle();
        if (puzzle == null)
            return null;
        
        // Check if already solved
        if (_data.SolvedPuzzles.ContainsKey(puzzle.Id))
            return _data.SolvedPuzzles[puzzle.Id];
        
        // Normalize answer
        string normalizedAnswer = answer.Trim().ToLower();
        string correctAnswer = puzzle.Answer.Trim().ToLower();
        
        bool isCorrect = normalizedAnswer == correctAnswer;
        
        PuzzleRecord record = new PuzzleRecord
        {
            PuzzleId = puzzle.Id,
            SolvedDate = DateTime.Now,
            TimeTakenSeconds = timeTakenSeconds,
            HintsUsed = hintsUsed,
            UsedBonusTime = timeTakenSeconds > puzzle.TimeLimit,
            GoldEarned = 0,
            ExpEarned = 0
        };
        
        if (isCorrect)
        {
            // Calculate rewards
            int goldReward = puzzle.GoldReward;
            int expReward = puzzle.ExpReward;
            
            // Bonus for fast completion
            if (timeTakenSeconds < puzzle.TimeLimit * 0.5)
            {
                goldReward = (int)(goldReward * 1.5);
                expReward = (int)(expReward * 1.5);
            }
            
            // Bonus for no hints
            if (hintsUsed == 0)
            {
                goldReward = (int)(goldReward * 1.25);
                expReward = (int)(expReward * 1.25);
            }
            
            record.GoldEarned = goldReward;
            record.ExpEarned = expReward;
            
            // Update statistics
            _data.SolvedPuzzles[puzzle.Id] = record;
            _data.TotalSolved++;
            _data.HintsUsed += hintsUsed;
            _data.TotalGoldEarned += goldReward;
            _data.TotalExpEarned += expReward;
            
            // Update streak
            _data.CurrentStreak++;
            if (_data.CurrentStreak > _data.BestStreak)
                _data.BestStreak = _data.CurrentStreak;
            
            if (!_data.SolvedPuzzleIds.Contains(puzzle.Id))
                _data.SolvedPuzzleIds.Add(puzzle.Id);
            
            // Add rewards (would integrate with GoldSystem in real implementation)
            // GoldSystem.AddGold(goldReward);
            // Player.AddExp(expReward);
            
            SaveData();
        }
        else
        {
            _data.TotalFailed++;
            SaveData();
        }
        
        return record;
    }
    
    // Get hint for current puzzle
    public static string GetHint()
    {
        PuzzleConfig puzzle = GetDailyPuzzle();
        if (puzzle == null)
            return "";
        
        return puzzle.Hint;
    }
    
    // Get statistics
    public static DailyPuzzleData GetStatistics()
    {
        return _data;
    }
    
    // Get current streak
    public static int GetCurrentStreak()
    {
        return _data.CurrentStreak;
    }
    
    // Get best streak
    public static int GetBestStreak()
    {
        return _data.BestStreak;
    }
    
    // Get total solved
    public static int GetTotalSolved()
    {
        return _data.TotalSolved;
    }
    
    // Get solved puzzle IDs
    public static List<int> GetSolvedPuzzleIds()
    {
        return _data.SolvedPuzzleIds;
    }
    
    // Load data from file
    private static void LoadData()
    {
        // TODO: Load from file
    }
    
    // Save data to file
    private static void SaveData()
    {
        // TODO: Save to file
    }
}
