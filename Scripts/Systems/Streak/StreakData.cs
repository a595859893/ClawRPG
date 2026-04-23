using Godot;
using System;
using System.Collections.Generic;

public partial class StreakData : Resource
{
    [Export] public int LoginStreak { get; set; } = 0;
    [Export] public int BattleStreak { get; set; } = 0;
    [Export] public int QuestStreak { get; set; } = 0;
    [Export] public int DungeonStreak { get; set; } = 0;
    [Export] public int PetInteractionStreak { get; set; } = 0;
    
    [Export] public int BestLoginStreak { get; set; } = 0;
    [Export] public int BestBattleStreak { get; set; } = 0;
    [Export] public int BestQuestStreak { get; set; } = 0;
    [Export] public int BestDungeonStreak { get; set; } = 0;
    [Export] public int BestPetInteractionStreak { get; set; } = 0;
    
    [Export] public int TotalLoginDays { get; set; } = 0;
    [Export] public int TotalBattleDays { get; set; } = 0;
    [Export] public int TotalQuestDays { get; set; } = 0;
    [Export] public int TotalDungeonDays { get; set; } = 0;
    [Export] public int TotalPetInteractionDays { get; set; } = 0;
    
    [Export] public int StreakFreezeTokens { get; set; } = 1;
    [Export] public int TotalStreakFreezeUsed { get; set; } = 0;
    
    [Export] public long LastLoginTime { get; set; } = 0;
    [Export] public long LastBattleTime { get; set; } = 0;
    [Export] public long LastQuestTime { get; set; } = 0;
    [Export] public long LastDungeonTime { get; set; } = 0;
    [Export] public long LastPetInteractionTime { get; set; } = 0;
    
    // [Export] removed: List<long> not Godot-exportable
    public List<long> StreakHistory { get; set; } = new List<long>();
    
    [Export] public int TotalRewardsClaimed { get; set; } = 0;
    [Export] public int TotalGoldFromStreaks { get; set; } = 0;
    [Export] public int TotalExpFromStreaks { get; set; } = 0;
}

public enum StreakType
{
    Login,
    Battle,
    Quest,
    Dungeon,
    PetInteraction
}

public class StreakRecord
{
    public StreakType Type;
    public int CurrentStreak;
    public int BestStreak;
    public int TotalDays;
    public long LastTime;
    public bool ClaimedToday;
}
