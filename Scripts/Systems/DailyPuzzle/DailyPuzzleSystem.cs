using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Random = System.Random;

/// <summary>
/// 每日谜题系统 - 提供每日挑战玩法
/// </summary>
public class DailyPuzzleSystem : BaseSystem
{
    private static DailyPuzzleData _data;
    private static Random _random = new Random();
    
    // Initialize the system
    public static void Initialize()
    {
        _data = new DailyPuzzleData();
        LoadData();
    }
    
    // BaseSystem Initialize override
    protected override void Initialize()
    {
        _data = new DailyPuzzleData();
        LoadData();
        IsInitialized = true;
        GD.Print("[DailyPuzzleSystem] Initialized");
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
    
    #region 数据持久化接口
    
    /// <summary>
    /// 导出保存数据 - 实现 BaseSystem 接口
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        if (_data == null) return data;
        
        data["current_daily_puzzle_id"] = _data.CurrentDailyPuzzleId;
        data["last_puzzle_date"] = _data.LastPuzzleDate.ToString("o");
        data["current_streak"] = _data.CurrentStreak;
        data["best_streak"] = _data.BestStreak;
        data["total_solved"] = _data.TotalSolved;
        data["total_failed"] = _data.TotalFailed;
        data["hints_used"] = _data.HintsUsed;
        data["total_gold_earned"] = _data.TotalGoldEarned;
        data["total_exp_earned"] = _data.TotalExpEarned;
        
        // Save solved puzzles
        var solvedArray = new Godot.Array();
        if (_data.SolvedPuzzles != null)
        {
            foreach (var kvp in _data.SolvedPuzzles)
            {
                var record = kvp.Value;
                var puzzleRecord = new Godot.Dictionary();
                puzzleRecord["puzzle_id"] = record.PuzzleId;
                puzzleRecord["solved_date"] = record.SolvedDate.ToString("o");
                puzzleRecord["time_taken"] = record.TimeTakenSeconds;
                puzzleRecord["hints_used"] = record.HintsUsed;
                puzzleRecord["used_bonus_time"] = record.UsedBonusTime;
                puzzleRecord["gold_earned"] = record.GoldEarned;
                puzzleRecord["exp_earned"] = record.ExpEarned;
                solvedArray.Add(puzzleRecord);
            }
        }
        data["solved_puzzles"] = solvedArray;
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据 - 实现 BaseSystem 接口
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null || _data == null) return;
        
        if (data.Contains("current_daily_puzzle_id"))
            _data.CurrentDailyPuzzleId = (int)data["current_daily_puzzle_id"];
        
        if (data.Contains("last_puzzle_date"))
            _data.LastPuzzleDate = DateTime.Parse((string)data["last_puzzle_date"]);
        
        if (data.Contains("current_streak"))
            _data.CurrentStreak = (int)data["current_streak"];
        
        if (data.Contains("best_streak"))
            _data.BestStreak = (int)data["best_streak"];
        
        if (data.Contains("total_solved"))
            _data.TotalSolved = (int)data["total_solved"];
        
        if (data.Contains("total_failed"))
            _data.TotalFailed = (int)data["total_failed"];
        
        if (data.Contains("hints_used"))
            _data.HintsUsed = (int)data["hints_used"];
        
        if (data.Contains("total_gold_earned"))
            _data.TotalGoldEarned = (int)data["total_gold_earned"];
        
        if (data.Contains("total_exp_earned"))
            _data.TotalExpEarned = (int)data["total_exp_earned"];
        
        // Load solved puzzles
        if (data.Contains("solved_puzzles"))
        {
            var solvedArray = (Godot.Array)data["solved_puzzles"];
            foreach (Godot.Dictionary puzzleRecord in solvedArray)
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
    
    #endregion
    
    // Load data from file (legacy, 兼容旧调用)
    private static void LoadData()
    {
        var saveSystem = SaveSystem.Instance;
        if (saveSystem == null) return;

        var data = saveSystem.LoadGame();
        if (data == null || data.Count == 0) return;

        // Load daily puzzle data
        if (data.Contains("daily_puzzle"))
        {
            var puzzleData = (Godot.Dictionary)data["daily_puzzle"];
            
            if (puzzleData.Contains("current_daily_puzzle_id"))
                _data.CurrentDailyPuzzleId = (int)puzzleData["current_daily_puzzle_id"];
            
            if (puzzleData.Contains("last_puzzle_date"))
                _data.LastPuzzleDate = DateTime.Parse((string)puzzleData["last_puzzle_date"]);
            
            if (puzzleData.Contains("current_streak"))
                _data.CurrentStreak = (int)puzzleData["current_streak"];
            
            if (puzzleData.Contains("best_streak"))
                _data.BestStreak = (int)puzzleData["best_streak"];
            
            if (puzzleData.Contains("total_solved"))
                _data.TotalSolved = (int)puzzleData["total_solved"];
            
            if (puzzleData.Contains("total_failed"))
                _data.TotalFailed = (int)puzzleData["total_failed"];
            
            if (puzzleData.Contains("hints_used"))
                _data.HintsUsed = (int)puzzleData["hints_used"];
            
            if (puzzleData.Contains("total_gold_earned"))
                _data.TotalGoldEarned = (int)puzzleData["total_gold_earned"];
            
            if (puzzleData.Contains("total_exp_earned"))
                _data.TotalExpEarned = (int)puzzleData["total_exp_earned"];
            
            // Load solved puzzles
            if (puzzleData.Contains("solved_puzzles"))
            {
                var solvedArray = (Godot.Array)puzzleData["solved_puzzles"];
                foreach (Godot.Dictionary puzzleRecord in solvedArray)
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
    
    // Save data to file (legacy, 兼容旧调用)
    private static void SaveData()
    {
        var saveSystem = SaveSystem.Instance;
        if (saveSystem == null) return;

        var data = saveSystem.LoadGame();
        if (data == null) data = new Godot.Dictionary();

        // Save daily puzzle data
        var puzzleData = new Godot.Dictionary();
        puzzleData["current_daily_puzzle_id"] = _data.CurrentDailyPuzzleId;
        puzzleData["last_puzzle_date"] = _data.LastPuzzleDate.ToString("o");
        puzzleData["current_streak"] = _data.CurrentStreak;
        puzzleData["best_streak"] = _data.BestStreak;
        puzzleData["total_solved"] = _data.TotalSolved;
        puzzleData["total_failed"] = _data.TotalFailed;
        puzzleData["hints_used"] = _data.HintsUsed;
        puzzleData["total_gold_earned"] = _data.TotalGoldEarned;
        puzzleData["total_exp_earned"] = _data.TotalExpEarned;

        // Save solved puzzles
        var solvedArray = new Godot.Array();
        foreach (var kvp in _data.SolvedPuzzles)
        {
            var record = kvp.Value;
            var puzzleRecord = new Godot.Dictionary();
            puzzleRecord["puzzle_id"] = record.PuzzleId;
            puzzleRecord["solved_date"] = record.SolvedDate.ToString("o");
            puzzleRecord["time_taken"] = record.TimeTakenSeconds;
            puzzleRecord["hints_used"] = record.HintsUsed;
            puzzleRecord["used_bonus_time"] = record.UsedBonusTime;
            puzzleRecord["gold_earned"] = record.GoldEarned;
            puzzleRecord["exp_earned"] = record.ExpEarned;
            solvedArray.Add(puzzleRecord);
        }
        puzzleData["solved_puzzles"] = solvedArray;

        data["daily_puzzle"] = puzzleData;
        saveSystem.SaveGame(data);
    }
}
