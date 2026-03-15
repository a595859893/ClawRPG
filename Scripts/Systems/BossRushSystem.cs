using Godot;
using System;
using System.Collections.Generic;

public class BossRushSystem : BaseSystem
{
    private BossRushData data;
    private BossRushState currentState = BossRushState.NotStarted;
    private string currentDifficulty = "Normal";
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    
    protected override void Initialize()
    {
        LoadData();
    }

    public override void _Ready()
    {
        base._Ready();
    }
    
    private void LoadData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        var savedData = saveSystem.LoadGame();
        
        if (savedData.Contains("BossRushData"))
        {
            var dict = (Godot.Dictionary)savedData["BossRushData"];
            data = new BossRushData();
            
            if (dict.Contains("CurrentStage")) data.CurrentStage = Convert.ToInt32(dict["CurrentStage"]);
            if (dict.Contains("CurrentBossIndex")) data.CurrentBossIndex = Convert.ToInt32(dict["CurrentBossIndex"]);
            if (dict.Contains("IsInRush")) data.IsInRush = Convert.ToBoolean(dict["IsInRush"]);
            if (dict.Contains("CurrentStreak")) data.CurrentStreak = Convert.ToInt32(dict["CurrentStreak"]);
            if (dict.Contains("BestStreak")) data.BestStreak = Convert.ToInt32(dict["BestStreak"]);
            if (dict.Contains("StartingHealth")) data.StartingHealth = Convert.ToSingle(dict["StartingHealth"]);
            if (dict.Contains("StartingAttack")) data.StartingAttack = Convert.ToSingle(dict["StartingAttack"]);
            if (dict.Contains("StartingDefense")) data.StartingDefense = Convert.ToSingle(dict["StartingDefense"]);
            if (dict.Contains("CurrentHealth")) data.CurrentHealth = Convert.ToSingle(dict["CurrentHealth"]);
            if (dict.Contains("GoldEarned")) data.GoldEarned = Convert.ToInt32(dict["GoldEarned"]);
            if (dict.Contains("ExpEarned")) data.ExpEarned = Convert.ToInt32(dict["ExpEarned"]);
            if (dict.Contains("BossesDefeated")) data.BossesDefeated = Convert.ToInt32(dict["BossesDefeated"]);
            if (dict.Contains("TotalRushAttempts")) data.TotalRushAttempts = Convert.ToInt32(dict["TotalRushAttempts"]);
            if (dict.Contains("TotalVictories")) data.TotalVictories = Convert.ToInt32(dict["TotalVictories"]);
            if (dict.Contains("TotalBossesDefeated")) data.TotalBossesDefeated = Convert.ToInt32(dict["TotalBossesDefeated"]);
            if (dict.Contains("HighestStageReached")) data.HighestStageReached = Convert.ToInt32(dict["HighestStageReached"]);
            if (dict.Contains("TotalGoldEarned")) data.TotalGoldEarned = Convert.ToInt32(dict["TotalGoldEarned"]);
            if (dict.Contains("TotalExpEarned")) data.TotalExpEarned = Convert.ToInt32(dict["TotalExpEarned"]);
        }
        else
        {
            data = new BossRushData();
        }
        
        rng.Randomize();
    }
    
    public override Dictionary ExportSaveData()
    {
        var dict = new Godot.Dictionary();
        dict["CurrentStage"] = data.CurrentStage;
        dict["CurrentBossIndex"] = data.CurrentBossIndex;
        dict["IsInRush"] = data.IsInRush;
        dict["CurrentStreak"] = data.CurrentStreak;
        dict["BestStreak"] = data.BestStreak;
        dict["StartingHealth"] = data.StartingHealth;
        dict["StartingAttack"] = data.StartingAttack;
        dict["StartingDefense"] = data.StartingDefense;
        dict["CurrentHealth"] = data.CurrentHealth;
        dict["GoldEarned"] = data.GoldEarned;
        dict["ExpEarned"] = data.ExpEarned;
        dict["BossesDefeated"] = data.BossesDefeated;
        dict["TotalRushAttempts"] = data.TotalRushAttempts;
        dict["TotalVictories"] = data.TotalVictories;
        dict["TotalBossesDefeated"] = data.TotalBossesDefeated;
        dict["HighestStageReached"] = data.HighestStageReached;
        dict["TotalGoldEarned"] = data.TotalGoldEarned;
        dict["TotalExpEarned"] = data.TotalExpEarned;
        
        return dict;
    }
    
    public override void ImportSaveData(Dictionary saveData)
    {
        if (saveData == null) return;
        
        if (saveData.Contains("CurrentStage")) data.CurrentStage = (int)saveData["CurrentStage"];
        if (saveData.Contains("CurrentBossIndex")) data.CurrentBossIndex = (int)saveData["CurrentBossIndex"];
        if (saveData.Contains("IsInRush")) data.IsInRush = (bool)saveData["IsInRush"];
        if (saveData.Contains("CurrentStreak")) data.CurrentStreak = (int)saveData["CurrentStreak"];
        if (saveData.Contains("BestStreak")) data.BestStreak = (int)saveData["BestStreak"];
        if (saveData.Contains("StartingHealth")) data.StartingHealth = (int)saveData["StartingHealth"];
        if (saveData.Contains("StartingAttack")) data.StartingAttack = (int)saveData["StartingAttack"];
        if (saveData.Contains("StartingDefense")) data.StartingDefense = (int)saveData["StartingDefense"];
        if (saveData.Contains("CurrentHealth")) data.CurrentHealth = (int)saveData["CurrentHealth"];
        if (saveData.Contains("GoldEarned")) data.GoldEarned = (int)saveData["GoldEarned"];
        if (saveData.Contains("ExpEarned")) data.ExpEarned = (int)saveData["ExpEarned"];
        if (saveData.Contains("BossesDefeated")) data.BossesDefeated = (int)saveData["BossesDefeated"];
        if (saveData.Contains("TotalRushAttempts")) data.TotalRushAttempts = (int)saveData["TotalRushAttempts"];
        if (saveData.Contains("TotalVictories")) data.TotalVictories = (int)saveData["TotalVictories"];
        if (saveData.Contains("TotalBossesDefeated")) data.TotalBossesDefeated = (int)saveData["TotalBossesDefeated"];
        if (saveData.Contains("HighestStageReached")) data.HighestStageReached = (int)saveData["HighestStageReached"];
        if (saveData.Contains("TotalGoldEarned")) data.TotalGoldEarned = (int)saveData["TotalGoldEarned"];
        if (saveData.Contains("TotalExpEarned")) data.TotalExpEarned = (int)saveData["TotalExpEarned"];
    }
    
    // Start a new boss rush
    public bool StartRush(string difficulty)
    {
        if (currentState == BossRushState.InProgress)
        {
            GD.Print("Already in a boss rush!");
            return false;
        }
        
        if (!BossRushDatabase.DifficultySettings.ContainsKey(difficulty))
        {
            GD.Print("Invalid difficulty: " + difficulty);
            return false;
        }
        
        currentDifficulty = difficulty;
        var player = GetNode("/root/Main/Player");
        
        data.CurrentStage = 1;
        data.CurrentBossIndex = 0;
        data.IsInRush = true;
        data.CurrentStreak = 0;
        data.StartingHealth = player.Get("max_health") != null ? Convert.ToSingle(player.Get("max_health")) : 1000f;
        data.StartingAttack = player.Get("attack") != null ? Convert.ToSingle(player.Get("attack")) : 100f;
        data.StartingDefense = player.Get("defense") != null ? Convert.ToSingle(player.Get("defense")) : 50f;
        data.CurrentHealth = data.StartingHealth;
        data.GoldEarned = 0;
        data.ExpEarned = 0;
        data.BossesDefeated = 0;
        
        data.TotalRushAttempts++;
        
        currentState = BossRushState.Preparing;
        SaveData();
        
        GD.Print($"Boss Rush started! Stage {data.CurrentStage}, Difficulty: {difficulty}");
        return true;
    }
    
    // Get current boss for the stage
    public BossRushBoss GetCurrentBoss()
    {
        if (!data.IsInRush || currentState == BossRushState.NotStarted)
            return null;
        
        int stageIndex = Math.Min(data.CurrentStage, 10);
        if (!BossRushDatabase.StageBosses.ContainsKey(stageIndex))
            return null;
        
        var bosses = BossRushDatabase.StageBosses[stageIndex];
        if (bosses.Count == 0)
            return null;
        
        // Random boss from the stage
        int bossIndex = rng.Randi() % bosses.Count;
        var boss = bosses[bossIndex];
        
        // Apply difficulty modifiers
        var diff = BossRushDatabase.DifficultySettings[currentDifficulty];
        
        var modifiedBoss = new BossRushBoss
        {
            Name = boss.Name,
            Health = boss.Health * diff.HealthMultiplier,
            Attack = boss.Attack * diff.AttackMultiplier,
            Defense = boss.Defense,
            Speed = boss.Speed,
            Experience = (int)(boss.Experience * diff.RewardMultiplier),
            Gold = (int)(boss.Gold * diff.RewardMultiplier),
            Element = boss.Element,
            Abilities = new List<string>(boss.Abilities)
        };
        
        return modifiedBoss;
    }
    
    // Record a boss defeat
    public void RecordBossDefeat(BossRushBoss boss)
    {
        data.BossesDefeated++;
        data.CurrentStreak++;
        data.GoldEarned += boss.Gold;
        data.ExpEarned += boss.Experience;
        
        if (data.CurrentStreak > data.BestStreak)
            data.CurrentStreak = data.BestStreak;
        
        if (data.CurrentStage > data.HighestStageReached)
            data.HighestStageReached = data.CurrentStage;
        
        GD.Print($"Boss defeated! Streak: {data.CurrentStreak}, Gold: {data.GoldEarned}, Exp: {data.ExpEarned}");
        
        SaveData();
    }
    
    // Record player damage taken
    public void RecordDamage(float damage)
    {
        data.CurrentHealth -= damage;
        
        if (data.CurrentHealth <= 0)
        {
            EndRush(false);
        }
    }
    
    // Advance to next stage
    public bool AdvanceStage()
    {
        if (!data.IsInRush)
            return false;
        
        data.CurrentStage++;
        data.CurrentBossIndex = 0;
        
        // Check if completed all stages
        if (data.CurrentStage > 10)
        {
            EndRush(true);
            return false;
        }
        
        // Grant stage completion rewards
        if (BossRushDatabase.StageRewards.ContainsKey(data.CurrentStage - 1))
        {
            var reward = BossRushDatabase.StageRewards[data.CurrentStage - 1];
            data.GoldEarned += (int)(reward.Gold * BossRushDatabase.DifficultySettings[currentDifficulty].RewardMultiplier);
            data.ExpEarned += (int)(reward.Experience * BossRushDatabase.DifficultySettings[currentDifficulty].RewardMultiplier);
        }
        
        SaveData();
        GD.Print($"Advanced to Stage {data.CurrentStage}!");
        return true;
    }
    
    // End the rush
    public void EndRush(bool victory)
    {
        if (victory)
        {
            currentState = BossRushState.Victory;
            data.TotalVictories++;
            data.IsInRush = false;
            GD.Print("Boss Rush Victory!");
        }
        else
        {
            currentState = BossRushState.Defeated;
            data.IsInRush = false;
            GD.Print("Boss Rush Defeated!");
        }
        
        // Update total statistics
        data.TotalBossesDefeated += data.BossesDefeated;
        data.TotalGoldEarned += data.GoldEarned;
        data.TotalExpEarned += data.ExpEarned;
        
        // Add to history
        var record = new BossRushRecord
        {
            Stage = data.CurrentStage,
            BossesDefeated = data.BossesDefeated,
            GoldEarned = data.GoldEarned,
            ExpEarned = data.ExpEarned,
            Victory = victory,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        
        data.RushHistory.Insert(0, record);
        if (data.RushHistory.Count > 50)
            data.RushHistory.RemoveAt(data.RushHistory.Count - 1);
        
        SaveData();
        
        // Grant rewards to player
        GrantRewards();
    }
    
    private void GrantRewards()
    {
        var player = GetNode("/root/Main/Player");
        if (player != null)
        {
            // Add gold
            var goldSystem = GetNode<GoldSystem>("/root/SaveSystem");
            if (goldSystem != null)
            {
                goldSystem.AddGold(data.GoldEarned);
            }
            
            // Add experience (simplified)
            GD.Print($"Rewards granted: {data.GoldEarned} Gold, {data.ExpEarned} Experience");
        }
    }
    
    // Pause/Resume
    public void PauseRush()
    {
        if (currentState == BossRushState.InProgress)
        {
            currentState = BossRushState.Paused;
            GD.Print("Boss Rush paused");
        }
    }
    
    public void ResumeRush()
    {
        if (currentState == BossRushState.Paused)
        {
            currentState = BossRushState.InProgress;
            GD.Print("Boss Rush resumed");
        }
    }
    
    // Quit rush
    public void QuitRush()
    {
        EndRush(false);
    }
    
    // Getters
    public BossRushData GetData() => data;
    public BossRushState GetState() => currentState;
    public string GetDifficulty() => currentDifficulty;
    public bool IsInRush() => data.IsInRush;
    
    public float GetCurrentHealthPercent()
    {
        if (data.StartingHealth <= 0) return 0;
        return Mathf.Clamp(data.CurrentHealth / data.StartingHealth, 0, 1);
    }
    
    public Dictionary GetStatistics()
    {
        var stats = new Godot.Dictionary();
        stats["total_attempts"] = data.TotalRushAttempts;
        stats["total_victories"] = data.TotalVictories;
        stats["total_bosses"] = data.TotalBossesDefeated;
        stats["highest_stage"] = data.HighestStageReached;
        stats["best_streak"] = data.BestStreak;
        stats["total_gold"] = data.TotalGoldEarned;
        stats["total_exp"] = data.TotalExpEarned;
        stats["win_rate"] = data.TotalRushAttempts > 0 ? (float)data.TotalVictories / data.TotalRushAttempts : 0;
        return stats;
    }
    
    public List<BossRushRecord> GetHistory(int count = 10)
    {
        if (data.RushHistory.Count <= count)
            return new List<BossRushRecord>(data.RushHistory);
        
        return data.RushHistory.GetRange(0, count);
    }
}
