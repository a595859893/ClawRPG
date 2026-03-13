using System;
using System.Collections.Generic;

public class DailyPuzzleDatabase
{
    // Puzzle configuration
    public static readonly Dictionary<int, PuzzleConfig> PuzzleConfigs = new Dictionary<int, PuzzleConfig>
    {
        // Math Puzzles (1-10)
        { 1, new PuzzleConfig { Id = 1, Type = DailyPuzzleData.PuzzleType.Math, Difficulty = 1, TimeLimit = 60, GoldReward = 50, ExpReward = 25, Question = "5 + 7 = ?", Answer = "12", Hint = "Add the numbers together" } },
        { 2, new PuzzleConfig { Id = 2, Type = DailyPuzzleData.PuzzleType.Math, Difficulty = 1, TimeLimit = 60, GoldReward = 50, ExpReward = 25, Question = "10 - 3 = ?", Answer = "7", Hint = "Subtract 3 from 10" } },
        { 3, new PuzzleConfig { Id = 3, Type = DailyPuzzleData.PuzzleType.Math, Difficulty = 2, TimeLimit = 45, GoldReward = 75, ExpReward = 40, Question = "8 × 6 = ?", Answer = "48", Hint = "Multiply 8 by 6" } },
        { 4, new PuzzleConfig { Id = 4, Type = DailyPuzzleData.PuzzleType.Math, Difficulty = 2, TimeLimit = 45, GoldReward = 75, ExpReward = 40, Question = "72 ÷ 8 = ?", Answer = "9", Hint = "Divide 72 by 8" } },
        { 5, new PuzzleConfig { Id = 5, Type = DailyPuzzleData.PuzzleType.Math, Difficulty = 3, TimeLimit = 30, GoldReward = 100, ExpReward = 60, Question = "15 × 15 = ?", Answer = "225", Hint = "15 squared" } },
        
        // Pattern Puzzles (11-20)
        { 11, new PuzzleConfig { Id = 11, Type = DailyPuzzleData.PuzzleType.Pattern, Difficulty = 1, TimeLimit = 60, GoldReward = 50, ExpReward = 25, Question = "2, 4, 6, 8, ?", Answer = "10", Hint = "Even numbers" } },
        { 12, new PuzzleConfig { Id = 12, Type = DailyPuzzleData.PuzzleType.Pattern, Difficulty = 1, TimeLimit = 60, GoldReward = 50, ExpReward = 25, Question = "1, 1, 2, 3, 5, ?", Answer = "8", Hint = "Fibonacci sequence" } },
        { 13, new PuzzleConfig { Id = 13, Type = DailyPuzzleData.PuzzleType.Pattern, Difficulty = 2, TimeLimit = 45, GoldReward = 75, ExpReward = 40, Question = "A, C, E, G, ?", Answer = "I", Hint = "Every other letter" } },
        { 14, new PuzzleConfig { Id = 14, Type = DailyPuzzleData.PuzzleType.Pattern, Difficulty = 2, TimeLimit = 45, GoldReward = 75, ExpReward = 40, Question = "1, 4, 9, 16, ?", Answer = "25", Hint = "Square numbers" } },
        { 15, new PuzzleConfig { Id = 15, Type = DailyPuzzleData.PuzzleType.Pattern, Difficulty = 3, TimeLimit = 30, GoldReward = 100, ExpReward = 60, Question = "O, T, T, F, F, S, ?", Answer = "S", Hint = "First letters of numbers" } },
        
        // Memory Puzzles (21-30)
        { 21, new PuzzleConfig { Id = 21, Type = DailyPuzzleData.PuzzleType.Memory, Difficulty = 1, TimeLimit = 30, GoldReward = 50, ExpReward = 25, Question = "Remember the sequence: Red, Blue, Green", Answer = "red blue green", Hint = "Watch the colors carefully" } },
        { 22, new PuzzleConfig { Id = 22, Type = DailyPuzzleData.PuzzleType.Memory, Difficulty = 1, TimeLimit = 30, GoldReward = 50, ExpReward = 25, Question = "Remember: Sword, Shield, Potion", Answer = "sword shield potion", Hint = "Three items" } },
        { 23, new PuzzleConfig { Id = 23, Type = DailyPuzzleData.PuzzleType.Memory, Difficulty = 2, TimeLimit = 45, GoldReward = 75, ExpReward = 40, Question = "Sequence: Fire, Water, Earth, Wind", Answer = "fire water earth wind", Hint = "Four elements" } },
        { 24, new PuzzleConfig { Id = 24, Type = DailyPuzzleData.PuzzleType.Memory, Difficulty = 2, TimeLimit = 45, GoldReward = 75, ExpReward = 40, Question = "Numbers: 7, 3, 9, 1", Answer = "7 3 9 1", Hint = "Remember the order" } },
        { 25, new PuzzleConfig { Id = 25, Type = DailyPuzzleData.PuzzleType.Memory, Difficulty = 3, TimeLimit = 60, GoldReward = 100, ExpReward = 60, Question = "Code: Dragon - Phoenix - Golem - Elemental", Answer = "dragon phoenix golem elemental", Hint = "Four mythic creatures" } },
        
        // Word Puzzles (31-40)
        { 31, new PuzzleConfig { Id = 31, Type = DailyPuzzleData.PuzzleType.Word, Difficulty = 1, TimeLimit = 60, GoldReward = 50, ExpReward = 25, Question = "Opposite of Light", Answer = "dark", Hint = "The absence of light" } },
        { 32, new PuzzleConfig { Id = 32, Type = DailyPuzzleData.PuzzleType.Word, Difficulty = 1, TimeLimit = 60, GoldReward = 50, ExpReward = 25, Question = "Synonym of Happy", Answer = "joyful", Hint = "Another word for happy" } },
        { 33, new PuzzleConfig { Id = 33, Type = DailyPuzzleData.PuzzleType.Word, Difficulty = 2, TimeLimit = 45, GoldReward = 75, ExpReward = 40, Question = "Unscramble: L P A E", Answer = "LEAP", Hint = "A jumping motion" } },
        { 34, new PuzzleConfig { Id = 34, Type = DailyPuzzleData.PuzzleType.Word, Difficulty = 2, TimeLimit = 45, GoldReward = 75, ExpReward = 40, Question = "Unscramble: D O O R G", Answer = "GOOD", Hint = "The opposite of bad" } },
        { 35, new PuzzleConfig { Id = 35, Type = DailyPuzzleData.PuzzleType.Word, Difficulty = 3, TimeLimit = 30, GoldReward = 100, ExpReward = 60, Question = "Unscramble: N I G H T M A R E", Answer = "NIGHTMARE", Hint = "A scary dream" } },
        
        // Riddle Puzzles (41-50)
        { 41, new PuzzleConfig { Id = 41, Type = DailyPuzzleData.PuzzleType.Riddle, Difficulty = 1, TimeLimit = 90, GoldReward = 50, ExpReward = 25, Question = "I have cities, but no houses. I have mountains, but no trees. I have water, but no fish. What am I?", Answer = "map", Hint = "It's something you look at" } },
        { 42, new PuzzleConfig { Id = 42, Type = DailyPuzzleData.PuzzleType.Riddle, Difficulty = 1, TimeLimit = 90, GoldReward = 50, ExpReward = 25, Question = "The more you take, the more you leave behind. What am I?", Answer = "footsteps", Hint = "Think about walking" } },
        { 43, new PuzzleConfig { Id = 43, Type = DailyPuzzleData.PuzzleType.Riddle, Difficulty = 2, TimeLimit = 60, GoldReward = 75, ExpReward = 40, Question = "I speak without a mouth and hear without ears. What am I?", Answer = "echo", Hint = "You hear this in mountains" } },
        { 44, new PuzzleConfig { Id = 44, Type = DailyPuzzleData.PuzzleType.Riddle, Difficulty = 2, TimeLimit = 60, GoldReward = 75, ExpReward = 40, Question = "I am not alive, but I grow; I don't have lungs, but I need air. What am I?", Answer = "fire", Hint = "Be careful around this" } },
        { 45, new PuzzleConfig { Id = 45, Type = DailyPuzzleData.PuzzleType.Riddle, Difficulty = 3, TimeLimit = 45, GoldReward = 100, ExpReward = 60, Question = "I have keys but no locks. I have space but no room. You can enter but can't go inside. What am I?", Answer = "keyboard", Hint = "Used with a computer" } },
    };
    
    // Difficulty names
    public static readonly string[] DifficultyNames = { "Easy", "Medium", "Hard" };
    
    // Puzzle type names
    public static readonly string[] PuzzleTypeNames = { "Math", "Pattern", "Memory", "Word", "Riddle" };
    
    // Get puzzle config by ID
    public static PuzzleConfig GetPuzzleConfig(int id)
    {
        if (PuzzleConfigs.ContainsKey(id))
            return PuzzleConfigs[id];
        return null;
    }
    
    // Get puzzles by type
    public static List<PuzzleConfig> GetPuzzlesByType(DailyPuzzleData.PuzzleType type)
    {
        List<PuzzleConfig> result = new List<PuzzleConfig>();
        foreach (var config in PuzzleConfigs.Values)
        {
            if (config.Type == type)
                result.Add(config);
        }
        return result;
    }
    
    // Get puzzles by difficulty
    public static List<PuzzleConfig> GetPuzzlesByDifficulty(int difficulty)
    {
        List<PuzzleConfig> result = new List<PuzzleConfig>();
        foreach (var config in PuzzleConfigs.Values)
        {
            if (config.Difficulty == difficulty)
                result.Add(config);
        }
        return result;
    }
}

public class PuzzleConfig
{
    public int Id { get; set; }
    public DailyPuzzleData.PuzzleType Type { get; set; }
    public int Difficulty { get; set; }
    public int TimeLimit { get; set; }
    public int GoldReward { get; set; }
    public int ExpReward { get; set; }
    public string Question { get; set; }
    public string Answer { get; set; }
    public string Hint { get; set; }
}
