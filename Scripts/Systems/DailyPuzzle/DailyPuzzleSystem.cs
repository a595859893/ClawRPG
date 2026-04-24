using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SaveSystem = ClawRPG.Scripts.Framework.SaveSystem;

public partial class DailyPuzzleSystem : BaseSystem
{
    private DailyPuzzleData _data;
    private Random _random = new Random();
    
    // Singleton instance for static-style access
    private static DailyPuzzleSystem _instance;
    public static DailyPuzzleSystem Instance => _instance;
    
    protected override void Initialize()
    {
        base.Initialize();
        _instance = this;
        _data = new DailyPuzzleData();
        
        // Load saved data
        var saveSystem = SaveSystem.Instance;
        if (saveSystem != null)
        {
            var savedData = saveSystem.LoadGame();
            if (savedData != null && savedData.ContainsKey("daily_puzzle"))
            {
                ImportSaveData((Dictionary)savedData["daily_puzzle"]);
            }
        }
        
        GD.Print("[DailyPuzzleSystem] Initialized");
    }
    
    // Get daily puzzle based on date
    public PuzzleConfig GetDailyPuzzle()
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
        }
        
        return DailyPuzzleDatabase.GetPuzzleConfig(_data.CurrentDailyPuzzleId);
    }
    
    // Get current puzzle ID
    public int GetCurrentPuzzleId()
    {
        return _data.CurrentDailyPuzzleId;
    }
    
    // Check if today's puzzle is solved
    public bool IsTodayPuzzleSolved()
    {
        return _data.SolvedPuzzles.ContainsKey(_data.CurrentDailyPuzzleId);
    }
    
    // Solve puzzle
    public PuzzleRecord SolvePuzzle(string answer, int timeTakenSeconds, int hintsUsed)
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
        }
        else
        {
            _data.TotalFailed++;
        }
        
        return record;
    }
    
    // Get hint for current puzzle
    public string GetHint()
    {
        PuzzleConfig puzzle = GetDailyPuzzle();
        if (puzzle == null)
            return "";
        
        return puzzle.Hint;
    }
    
    // Get statistics
    public DailyPuzzleData GetStatistics()
    {
        return _data;
    }
    
    // Get current streak
    public int GetCurrentStreak()
    {
        return _data.CurrentStreak;
    }
    
    // Get best streak
    public int GetBestStreak()
    {
        return _data.BestStreak;
    }
    
    // Get total solved
    public int GetTotalSolved()
    {
        return _data.TotalSolved;
    }
    
    // Get solved puzzle IDs
    public List<int> GetSolvedPuzzleIds()
    {
        return _data.SolvedPuzzleIds;
    }
    
    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        data["current_daily_puzzle_id"] = _data.CurrentDailyPuzzleId;
        data["last_puzzle_date"] = _data.LastPuzzleDate.ToString("o");
        data["current_streak"] = _data.CurrentStreak;
        data["best_streak"] = _data.BestStreak;
        data["total_solved"] = _data.TotalSolved;
        data["total_failed"] = _data.TotalFailed;
        data["hints_used"] = _data.HintsUsed;
        data["total_gold_earned"] = _data.TotalGoldEarned;
        data["total_exp_earned"] = _data.TotalExpEarned;
        
        // Export solved puzzles
        var solvedArray = new Godot.Array();
        foreach (var kvp in _data.SolvedPuzzles)
        {
            var record = kvp.Value;
            var puzzleRecord = new Godot.Collections.Dictionary();
            puzzleRecord["puzzle_id"] = record.PuzzleId;
            puzzleRecord["solved_date"] = record.SolvedDate.ToString("o");
            puzzleRecord["time_taken"] = record.TimeTakenSeconds;
            puzzleRecord["hints_used"] = record.HintsUsed;
            puzzleRecord["used_bonus_time"] = record.UsedBonusTime;
            puzzleRecord["gold_earned"] = record.GoldEarned;
            puzzleRecord["exp_earned"] = record.ExpEarned;
            solvedArray.Add(puzzleRecord);
        }
        data["solved_puzzles"] = solvedArray;
        
        // Export solved puzzle IDs list
        var solvedIdsArray = new Godot.Array();
        foreach (int puzzleId in _data.SolvedPuzzleIds)
        {
            solvedIdsArray.Add(puzzleId);
        }
        data["solved_puzzle_ids"] = solvedIdsArray;
        
        return data;
    }
    
    /// <summary>
    /// Import save data
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("current_daily_puzzle_id"))
            _data.CurrentDailyPuzzleId = (int)data["current_daily_puzzle_id"];
        
        if (data.ContainsKey("last_puzzle_date"))
            _data.LastPuzzleDate = DateTime.Parse((string)data["last_puzzle_date"]);
        
        if (data.ContainsKey("current_streak"))
            _data.CurrentStreak = (int)data["current_streak"];
        
        if (data.ContainsKey("best_streak"))
            _data.BestStreak = (int)data["best_streak"];
        
        if (data.ContainsKey("total_solved"))
            _data.TotalSolved = (int)data["total_solved"];
        
        if (data.ContainsKey("total_failed"))
            _data.TotalFailed = (int)data["total_failed"];
        
        if (data.ContainsKey("hints_used"))
            _data.HintsUsed = (int)data["hints_used"];
        
        if (data.ContainsKey("total_gold_earned"))
            _data.TotalGoldEarned = (int)data["total_gold_earned"];
        
        if (data.ContainsKey("total_exp_earned"))
            _data.TotalExpEarned = (int)data["total_exp_earned"];
        
        // Load solved puzzles
        if (data.ContainsKey("solved_puzzles"))
        {
            var solvedArray = (Godot.Array)data["solved_puzzles"];
            foreach (Godot.Collections.Dictionary puzzleRecord in solvedArray)
            {
                var record = new PuzzleRecord
                {
                    PuzzleId = (int)puzzleRecord["puzzle_id"],
                    SolvedDate = DateTime.Parse((string)puzzleRecord["solved_date"]),
                    TimeTakenSeconds = (int)puzzleRecord["time_taken"],
                    HintsUsed = (int)puzzleRecord["hints_used"],
                    UsedBonusTime = (bool)puzzleRecord["used_bonus_time"],
                    GoldEarned = (int)puzzleRecord["gold_earned"],
                    ExpEarned = (int)puzzleRecord["exp_earned"]
                };
                _data.SolvedPuzzles[record.PuzzleId] = record;
                
                if (!_data.SolvedPuzzleIds.Contains(record.PuzzleId))
                    _data.SolvedPuzzleIds.Add(record.PuzzleId);
            }
        }
    }
}
