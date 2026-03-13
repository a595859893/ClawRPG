using Godot;
using System;

public class StreakMain : Node
{
    private static StreakSystem _streakSystem;
    private static StreakUI _streakUI;
    private static bool _initialized = false;
    
    public override void _Ready()
    {
        if (!_initialized)
        {
            Initialize();
        }
    }
    
    public static void Initialize()
    {
        if (_streakSystem == null)
        {
            _streakSystem = new StreakSystem();
            _streakSystem.Name = "StreakSystem";
            
            var root = Engine.GetRootMetas("root");
            if (root != null)
            {
                root.AddChild(_streakSystem);
            }
            
            // Connect signals
            StreakSystem.OnStreakUpdated += OnStreakUpdated;
            StreakSystem.OnRewardClaimed += OnRewardClaimed;
            StreakSystem.OnStreakBroken += OnStreakBroken;
            StreakSystem.OnMilestoneReached += OnMilestoneReached;
            
            _initialized = true;
            
            GD.Print("StreakSystem initialized");
        }
    }
    
    private static void OnStreakUpdated(StreakType type, int newStreak)
    {
        GD.Print($"Streak updated: {type} = {newStreak} days");
    }
    
    private static void OnRewardClaimed(StreakType type, int streak, StreakReward reward)
    {
        GD.Print($"Reward claimed: {type} (streak: {streak}) - {reward.Gold} gold, {reward.Exp} exp");
    }
    
    private static void OnStreakBroken(StreakType type)
    {
        GD.Print($"Streak broken: {type}");
    }
    
    private static void OnMilestoneReached(StreakType type, int milestone)
    {
        GD.Print($"Milestone reached: {type} at {milestone} days!");
    }
    
    public static StreakSystem GetStreakSystem() => _streakSystem;
    
    public static void ToggleStreakUI()
    {
        if (_streakSystem == null)
        {
            Initialize();
        }
        
        if (_streakUI != null && IsInstanceValid(_streakUI))
        {
            _streakUI.QueueFree();
            _streakUI = null;
            return;
        }
        
        _streakUI = new StreakUI();
        _streakUI.Name = "StreakUI";
        
        var root = Engine.GetRootMetas("root");
        if (root != null)
        {
            root.AddChild(_streakUI);
        }
    }
    
    public static void RecordLogin()
    {
        if (_streakSystem != null)
        {
            _streakSystem.OnPlayerLogin();
        }
    }
    
    public static void RecordBattle()
    {
        if (_streakSystem != null)
        {
            _streakSystem.OnBattleComplete();
        }
    }
    
    public static void RecordQuest()
    {
        if (_streakSystem != null)
        {
            _streakSystem.OnQuestComplete();
        }
    }
    
    public static void RecordDungeon()
    {
        if (_streakSystem != null)
        {
            _streakSystem.OnDungeonComplete();
        }
    }
    
    public static void RecordPetInteraction()
    {
        if (_streakSystem != null)
        {
            _streakSystem.OnPetInteraction();
        }
    }
}
