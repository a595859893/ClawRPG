using System;
using System.Collections.Generic;

public class PetAchievementUI
{
    private PetAchievementSystem _system;
    private PetAchievementDatabase _database;
    private string _currentPetId = "";
    private int _currentTab = 0;
    private PetAchievementData.AchievementType? _filterType = null;
    private bool _showUnlockedOnly = false;
    
    public PetAchievementUI()
    {
        _system = PetAchievementSystem.Instance;
        _database = _system.Database;
    }
    
    public void ToggleUI()
    {
        if (_currentPetId == "")
        {
            // Show pet selection or first pet
            _currentPetId = "pet_1"; // Default, would be from player data
        }
        
        // Toggle visibility (in Godot, would control panel visibility)
        Console.WriteLine("=== Pet Achievement UI Toggled ===");
        ShowUI();
    }
    
    public void ShowUI()
    {
        if (_currentPetId == "")
        {
            Console.WriteLine("No pet selected!");
            return;
        }
        
        _system.InitializePetAchievements(_currentPetId);
        
        Console.WriteLine("\n=== PET ACHIEVEMENT SYSTEM ===");
        Console.WriteLine($"Pet: {_currentPetId}");
        Console.WriteLine($"Progress: {_system.GetProgressPercentage(_currentPetId):F1}%");
        
        ShowTab(_currentTab);
    }
    
    private void ShowTab(int tab)
    {
        switch (tab)
        {
            case 0:
                ShowOverview();
                break;
            case 1:
                ShowAchievements();
                break;
            case 2:
                ShowStatistics();
                break;
        }
    }
    
    private void ShowOverview()
    {
        Console.WriteLine("\n--- Overview ---");
        
        var achievements = _system.GetPetAchievements(_currentPetId);
        int unlocked = 0;
        int common = 0, uncommon = 0, rare = 0, epic = 0, legendary = 0;
        
        foreach (var a in achievements)
        {
            if (a.IsUnlocked)
            {
                unlocked++;
                switch (a.Rarity)
                {
                    case PetAchievementData.AchievementRarity.Common: common++; break;
                    case PetAchievementData.AchievementRarity.Uncommon: uncommon++; break;
                    case PetAchievementData.AchievementRarity.Rare: rare++; break;
                    case PetAchievementData.AchievementRarity.Epic: epic++; break;
                    case PetAchievementData.AchievementRarity.Legendary: legendary++; break;
                }
            }
        }
        
        Console.WriteLine($"Unlocked: {unlocked}/{achievements.Count}");
        Console.WriteLine($"Progress: {_system.GetProgressPercentage(_currentPetId):F1}%");
        Console.WriteLine($"\nRarity Breakdown:");
        Console.WriteLine($"  Common: {common}");
        Console.WriteLine($"  Uncommon: {uncommon}");
        Console.WriteLine($"  Rare: {rare}");
        Console.WriteLine($"  Epic: {epic}");
        Console.WriteLine($"  Legendary: {legendary}");
        
        // Show recent unlocks
        Console.WriteLine("\nRecent Achievements:");
        var unlockedList = _system.GetUnlockedAchievements(_currentPetId);
        int count = 0;
        for (int i = unlockedList.Count - 1; i >= 0 && count < 5; i--)
        {
            var a = unlockedList[i];
            Console.WriteLine($"  [{GetRarityIcon(a.Rarity)}] {a.Name} - {a.Description}");
            count++;
        }
        
        Console.WriteLine("\n[0] Overview | [1] Achievements | [2] Statistics");
        Console.WriteLine("[<] Previous Pet | [>] Next Pet | [ESC] Close");
    }
    
    private void ShowAchievements()
    {
        Console.WriteLine("\n--- All Achievements ---");
        
        var achievements = _system.GetPetAchievements(_currentPetId);
        
        // Filter by type if set
        if (_filterType.HasValue)
        {
            Console.WriteLine($"Filter: {_filterType.Value}");
        }
        
        int shown = 0;
        foreach (var a in achievements)
        {
            // Apply filters
            if (_filterType.HasValue && a.Type != _filterType.Value)
                continue;
            
            if (_showUnlockedOnly && !a.IsUnlocked)
                continue;
            
            string status = a.IsUnlocked ? "✓" : " ";
            string progress = $"{a.CurrentValue}/{a.RequiredValue}";
            string rarity = GetRarityIcon(a.Rarity);
            
            Console.WriteLine($"  {status} {rarity} {a.Name} ({a.Rarity})");
            Console.WriteLine($"      {a.Description}");
            Console.WriteLine($"      Progress: {progress}");
            Console.WriteLine();
            
            shown++;
            if (shown >= 20) break;
        }
        
        Console.WriteLine($"\nShowing {shown} achievements");
        Console.WriteLine("[T] Toggle Type Filter | [U] Toggle Unlocked Only");
        Console.WriteLine("[0] Overview | [1] Achievements | [2] Statistics");
    }
    
    private void ShowStatistics()
    {
        Console.WriteLine("\n--- Statistics ---");
        
        var stats = _system.GetStatistics();
        
        Console.WriteLine($"Total Achievements Unlocked: {stats.TotalAchievementsUnlocked}");
        Console.WriteLine($"Total Achievements: {stats.TotalAchievements}");
        Console.WriteLine($"Total Gold Earned: {stats.TotalGoldEarned}");
        
        Console.WriteLine("\nBy Rarity:");
        foreach (var kvp in stats.RarityBreakdown)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }
        
        Console.WriteLine("\n[0] Overview | [1] Achievements | [2] Statistics");
    }
    
    private string GetRarityIcon(PetAchievementData.AchievementRarity rarity)
    {
        switch (rarity)
        {
            case PetAchievementData.AchievementRarity.Common: return "⚪";
            case PetAchievementData.AchievementRarity.Uncommon: return "🟢";
            case PetAchievementData.AchievementRarity.Rare: return "🔵";
            case PetAchievementData.AchievementRarity.Epic: return "🟣";
            case PetAchievementData.AchievementRarity.Legendary: return "🟠";
            default: return "⚪";
        }
    }
    
    public void SetCurrentPet(string petId)
    {
        _currentPetId = petId;
    }
    
    public void NextTab()
    {
        _currentTab = (_currentTab + 1) % 3;
        ShowTab(_currentTab);
    }
    
    public void PreviousTab()
    {
        _currentTab = (_currentTab - 1 + 3) % 3;
        ShowTab(_currentTab);
    }
    
    public void ToggleTypeFilter(PetAchievementData.AchievementType type)
    {
        if (_filterType == type)
            _filterType = null;
        else
            _filterType = type;
        
        ShowAchievements();
    }
    
    public void ToggleUnlockedOnly()
    {
        _showUnlockedOnly = !_showUnlockedOnly;
        ShowAchievements();
    }
}
