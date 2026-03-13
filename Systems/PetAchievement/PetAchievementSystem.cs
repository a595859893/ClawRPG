using System;
using System.Collections.Generic;

public class PetAchievementSystem
{
    private static PetAchievementSystem _instance;
    public static PetAchievementSystem Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PetAchievementSystem();
            return _instance;
        }
    }
    
    public PetAchievementData Data { get; private set; }
    public PetAchievementDatabase Database { get; private set; }
    
    // Pet tracking counters (would be connected to actual game data)
    private Dictionary<string, int> _battleWins = new Dictionary<string, int>();
    private Dictionary<string, int> _enemyKills = new Dictionary<string, int>();
    private Dictionary<string, int> _bossKills = new Dictionary<string, int>();
    private Dictionary<string, int> _locationsVisited = new Dictionary<string, int>();
    private Dictionary<string, int> _floorReached = new Dictionary<string, int>();
    private Dictionary<string, int> _goldEarned = new Dictionary<string, int>();
    private Dictionary<string, int> _petLevel = new Dictionary<string, int>();
    private Dictionary<string, int> _itemsCollected = new Dictionary<string, int>();
    private Dictionary<string, int> _criticalHits = new Dictionary<string, int>();
    private Dictionary<string, int> _maxCombo = new Dictionary<string, int>();
    private Dictionary<string, int> _perfectBattles = new Dictionary<string, int>();
    private Dictionary<string, int> _survivalWins = new Dictionary<string, int>();
    private Dictionary<string, int> _noDamageWins = new Dictionary<string, int>();
    
    public event Action<string, PetAchievementData.Achievement> OnAchievementUnlocked;
    
    public PetAchievementSystem()
    {
        Data = new PetAchievementData();
        Database = new PetAchievementDatabase();
        InitializeAchievements();
    }
    
    private void InitializeAchievements()
    {
        // Initialize global achievements from database
        foreach (var achievementDef in Database.AllAchievements)
        {
            var achievement = new PetAchievementData.Achievement
            {
                Id = achievementDef.Id,
                Name = achievementDef.Name,
                Description = achievementDef.Description,
                Type = achievementDef.Type,
                Rarity = achievementDef.Rarity,
                RequiredValue = achievementDef.RequiredValue,
                CurrentValue = 0,
                IsUnlocked = false,
                UnlockedAt = null
            };
            Data.GlobalAchievements.Add(achievement);
            Data.TotalAchievements++;
        }
    }
    
    // Initialize achievements for a specific pet
    public void InitializePetAchievements(string petId)
    {
        if (!Data.PetAchievements.ContainsKey(petId))
        {
            Data.PetAchievements[petId] = new List<PetAchievementData.Achievement>();
            
            foreach (var achievementDef in Database.AllAchievements)
            {
                var achievement = new PetAchievementData.Achievement
                {
                    Id = achievementDef.Id,
                    Name = achievementDef.Name,
                    Description = achievementDef.Description,
                    Type = achievementDef.Type,
                    Rarity = achievementDef.Rarity,
                    RequiredValue = achievementDef.RequiredValue,
                    CurrentValue = 0,
                    IsUnlocked = false,
                    UnlockedAt = null
                };
                Data.PetAchievements[petId].Add(achievement);
                Data.TotalAchievements++;
            }
        }
        
        // Initialize counters
        if (!_battleWins.ContainsKey(petId)) _battleWins[petId] = 0;
        if (!_enemyKills.ContainsKey(petId)) _enemyKills[petId] = 0;
        if (!_bossKills.ContainsKey(petId)) _bossKills[petId] = 0;
        if (!_locationsVisited.ContainsKey(petId)) _locationsVisited[petId] = 0;
        if (!_floorReached.ContainsKey(petId)) _floorReached[petId] = 0;
        if (!_goldEarned.ContainsKey(petId)) _goldEarned[petId] = 0;
        if (!_petLevel.ContainsKey(petId)) _petLevel[petId] = 1;
        if (!_itemsCollected.ContainsKey(petId)) _itemsCollected[petId] = 0;
        if (!_criticalHits.ContainsKey(petId)) _criticalHits[petId] = 0;
        if (!_maxCombo.ContainsKey(petId)) _maxCombo[petId] = 0;
        if (!_perfectBattles.ContainsKey(petId)) _perfectBattles[petId] = 0;
        if (!_survivalWins.ContainsKey(petId)) _survivalWins[petId] = 0;
        if (!_noDamageWins.ContainsKey(petId)) _noDamageWins[petId] = 0;
    }
    
    // Update achievement progress
    public void UpdateProgress(string petId, string achievementId, int newValue)
    {
        InitializePetAchievements(petId);
        
        var achievements = Data.PetAchievements[petId];
        foreach (var achievement in achievements)
        {
            if (achievement.Id == achievementId && !achievement.IsUnlocked)
            {
                achievement.CurrentValue = newValue;
                
                if (achievement.CurrentValue >= achievement.RequiredValue)
                {
                    UnlockAchievement(petId, achievement);
                }
                break;
            }
        }
    }
    
    // Increment achievement progress
    public void IncrementProgress(string petId, string achievementId, int amount = 1)
    {
        InitializePetAchievements(petId);
        
        var achievements = Data.PetAchievements[petId];
        foreach (var achievement in achievements)
        {
            if (achievement.Id == achievementId && !achievement.IsUnlocked)
            {
                achievement.CurrentValue += amount;
                
                if (achievement.CurrentValue >= achievement.RequiredValue)
                {
                    UnlockAchievement(petId, achievement);
                }
                break;
            }
        }
    }
    
    // Unlock achievement
    private void UnlockAchievement(string petId, PetAchievementData.Achievement achievement)
    {
        achievement.IsUnlocked = true;
        achievement.UnlockedAt = DateTime.Now;
        
        Data.TotalAchievementsUnlocked++;
        if (Data.RarityBreakdown.ContainsKey(achievement.Rarity))
        {
            Data.RarityBreakdown[achievement.Rarity]++;
        }
        
        // Get reward
        var def = Database.GetAchievement(achievement.Id);
        if (def != null)
        {
            Data.TotalGoldEarned += def.GoldReward;
            // Gold would be added to player's wallet in actual implementation
        }
        
        OnAchievementUnlocked?.Invoke(petId, achievement);
    }
    
    // Convenience methods for common achievement updates
    public void RecordBattleVictory(string petId)
    {
        InitializePetAchievements(petId);
        
        int wins = ++_battleWins[petId];
        UpdateProgress(petId, "battle_first_victory", 1);
        UpdateProgress(petId, "battle_10_victories", wins);
        UpdateProgress(petId, "battle_50_victories", wins);
        UpdateProgress(petId, "battle_100_victories", wins);
        UpdateProgress(petId, "battle_500_victories", wins);
        UpdateProgress(petId, "battle_1000_victories", wins);
    }
    
    public void RecordEnemyKill(string petId)
    {
        InitializePetAchievements(petId);
        
        int kills = ++_enemyKills[petId];
        UpdateProgress(petId, "kills_10_enemies", kills);
        UpdateProgress(petId, "kills_50_enemies", kills);
        UpdateProgress(petId, "kills_100_enemies", kills);
        UpdateProgress(petId, "kills_500_enemies", kills);
    }
    
    public void RecordBossKill(string petId)
    {
        InitializePetAchievements(petId);
        
        int kills = ++_bossKills[petId];
        UpdateProgress(petId, "boss_first_kill", 1);
        UpdateProgress(petId, "boss_5_kills", kills);
        UpdateProgress(petId, "boss_25_kills", kills);
    }
    
    public void RecordLocationVisit(string petId)
    {
        InitializePetAchievements(petId);
        
        int locations = ++_locationsVisited[petId];
        UpdateProgress(petId, "explore_first_location", 1);
        UpdateProgress(petId, "explore_5_locations", locations);
        UpdateProgress(petId, "explore_10_locations", locations);
        UpdateProgress(petId, "explore_20_locations", locations);
    }
    
    public void RecordFloorReached(string petId, int floor)
    {
        InitializePetAchievements(petId);
        
        if (floor > _floorReached[petId])
        {
            _floorReached[petId] = floor;
            UpdateProgress(petId, "floor_5_reached", floor);
            UpdateProgress(petId, "floor_10_reached", floor);
            UpdateProgress(petId, "floor_25_reached", floor);
            UpdateProgress(petId, "floor_50_reached", floor);
        }
    }
    
    public void RecordGoldEarned(string petId, int gold)
    {
        InitializePetAchievements(petId);
        
        _goldEarned[petId] += gold;
        UpdateProgress(petId, "gold_100_earned", _goldEarned[petId]);
        UpdateProgress(petId, "gold_1000_earned", _goldEarned[petId]);
        UpdateProgress(petId, "gold_10000_earned", _goldEarned[petId]);
        UpdateProgress(petId, "gold_100000_earned", _goldEarned[petId]);
    }
    
    public void RecordLevelUp(string petId, int level)
    {
        InitializePetAchievements(petId);
        
        _petLevel[petId] = level;
        UpdateProgress(petId, "level_5_reached", level);
        UpdateProgress(petId, "level_10_reached", level);
        UpdateProgress(petId, "level_25_reached", level);
        UpdateProgress(petId, "level_50_reached", level);
        UpdateProgress(petId, "level_100_reached", level);
    }
    
    public void RecordItemCollected(string petId)
    {
        InitializePetAchievements(petId);
        
        int items = ++_itemsCollected[petId];
        UpdateProgress(petId, "collect_first_item", 1);
        UpdateProgress(petId, "collect_10_items", items);
        UpdateProgress(petId, "collect_50_items", items);
        UpdateProgress(petId, "collect_100_items", items);
    }
    
    public void RecordCriticalHit(string petId)
    {
        InitializePetAchievements(petId);
        
        int crits = ++_criticalHits[petId];
        UpdateProgress(petId, "crit_10_times", crits);
        UpdateProgress(petId, "crit_100_times", crits);
        UpdateProgress(petId, "crit_500_times", crits);
    }
    
    public void RecordCombo(string petId, int combo)
    {
        InitializePetAchievements(petId);
        
        if (combo > _maxCombo[petId])
        {
            _maxCombo[petId] = combo;
            UpdateProgress(petId, "combo_5", combo);
            UpdateProgress(petId, "combo_10", combo);
            UpdateProgress(petId, "combo_25", combo);
        }
    }
    
    public void RecordPerfectBattle(string petId)
    {
        InitializePetAchievements(petId);
        
        UpdateProgress(petId, "perfect_battle", 1);
    }
    
    public void RecordSurvivalWin(string petId)
    {
        InitializePetAchievements(petId);
        
        UpdateProgress(petId, "survival_expert", 1);
    }
    
    public void RecordNoDamageWin(string petId)
    {
        InitializePetAchievements(petId);
        
        int wins = ++_noDamageWins[petId];
        UpdateProgress(petId, "no_damage_run", wins);
    }
    
    public void RecordEvolution(string petId, int evolutionCount, bool isLegendary)
    {
        InitializePetAchievements(petId);
        
        UpdateProgress(petId, "evolve_first", evolutionCount);
        UpdateProgress(petId, "evolve_3_times", evolutionCount);
        
        if (isLegendary)
        {
            UpdateProgress(petId, "evolve_legendary", 1);
        }
    }
    
    // Get achievements for a pet
    public List<PetAchievementData.Achievement> GetPetAchievements(string petId)
    {
        InitializePetAchievements(petId);
        return Data.PetAchievements[petId];
    }
    
    // Get unlocked achievements
    public List<PetAchievementData.Achievement> GetUnlockedAchievements(string petId)
    {
        InitializePetAchievements(petId);
        
        var result = new List<PetAchievementData.Achievement>();
        foreach (var achievement in Data.PetAchievements[petId])
        {
            if (achievement.IsUnlocked)
                result.Add(achievement);
        }
        return result;
    }
    
    // Get achievement progress percentage
    public float GetProgressPercentage(string petId)
    {
        InitializePetAchievements(petId);
        
        int total = Data.PetAchievements[petId].Count;
        if (total == 0) return 0;
        
        int unlocked = 0;
        foreach (var achievement in Data.PetAchievements[petId])
        {
            if (achievement.IsUnlocked) unlocked++;
        }
        
        return (float)unlocked / total * 100f;
    }
    
    // Get statistics
    public PetAchievementData GetStatistics()
    {
        return Data;
    }
    
    // Check if achievement is unlocked
    public bool IsAchievementUnlocked(string petId, string achievementId)
    {
        InitializePetAchievements(petId);
        
        foreach (var achievement in Data.PetAchievements[petId])
        {
            if (achievement.Id == achievementId)
                return achievement.IsUnlocked;
        }
        return false;
    }
}
