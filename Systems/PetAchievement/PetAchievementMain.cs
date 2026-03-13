using System;

// Main entry point for Pet Achievement System integration
public class PetAchievementMain
{
    private static PetAchievementSystem _achievementSystem;
    private static PetAchievementUI _achievementUI;
    
    public static void Initialize()
    {
        _achievementSystem = PetAchievementSystem.Instance;
        _achievementUI = new PetAchievementUI();
        
        // Subscribe to achievement unlock events
        _achievementSystem.OnAchievementUnlocked += OnAchievementUnlocked;
        
        Console.WriteLine("[PetAchievementSystem] Initialized");
    }
    
    private static void OnAchievementUnlocked(string petId, PetAchievementData.Achievement achievement)
    {
        Console.WriteLine($"[Achievement Unlocked!] {petId}: {achievement.Name}");
        Console.WriteLine($"  {achievement.Description}");
        
        var def = _achievementSystem.Database.GetAchievement(achievement.Id);
        if (def != null)
        {
            Console.WriteLine($"  Reward: {def.GoldReward} Gold, {def.ExpReward} EXP");
        }
    }
    
    public static void TogglePetAchievementUI()
    {
        if (_achievementUI == null)
        {
            _achievementUI = new PetAchievementUI();
        }
        _achievementUI.ToggleUI();
    }
    
    public static PetAchievementSystem GetSystem()
    {
        return _achievementSystem;
    }
    
    public static PetAchievementUI GetUI()
    {
        return _achievementUI;
    }
}

// Input handling helper (would be connected to Godot input system)
public partial class Main
{
    private static bool _petAchievementUIVisible = false;
    
    // Called when 'A' key is pressed (Pet Achievement toggle)
    public static void TogglePetAchievementUI()
    {
        _petAchievementUIVisible = !_petAchievementUIVisible;
        
        if (_petAchievementUIVisible)
        {
            PetAchievementMain.Initialize();
            PetAchievementMain.TogglePetAchievementUI();
        }
        else
        {
            // Hide UI (in Godot, would hide the panel)
            Console.WriteLine("[PetAchievement] UI Hidden");
        }
    }
}
