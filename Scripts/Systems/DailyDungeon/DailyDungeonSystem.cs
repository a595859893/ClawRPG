using Godot;
using System;
using System.Collections.Generic;

public class DailyDungeonSystem : BaseSystem
{
    public static DailyDungeonSystem Instance { get; private set; }

    // Signals
    public Signal DungeonStarted { get; }
    public Signal DungeonEnded { get; }
    public Signal FloorCleared { get; }
    public Signal FloorEntered { get; }
    public Signal DungeonCompleted { get; }
    public Signal DungeonFailed { get; }

    // Player dungeon data
    public Dictionary<string, PlayerDungeonData> PlayerDungeonData { get; private set; }

    // Current dungeon state
    private DailyDungeonData _currentDungeon;
    private int _currentFloor;
    private int _enemiesDefeated;
    private float _timeRemaining;
    private bool _isInDungeon;
    private DateTime _dungeonStartTime;

    // Daily reset
    private DateTime _lastDailyReset;
    private int _dailyChallengeCount;
    private bool _dailyCompleted;

    public override void _Ready()
    {
        Instance = this;
        PlayerDungeonData = new Dictionary<string, PlayerDungeonData>();
        LoadData();
    }

    public void LoadData()
    {
        // Load player dungeon data from save
        var saveData = SaveSystem.LoadProgress();
        if (saveData != null && saveData.ContainsKey("daily_dungeon"))
        {
            var dungeonData = saveData["daily_dungeon"] as Dictionary<string, object>;
            if (dungeonData != null)
            {
                _lastDailyReset = DateTime.Parse(dungeonData.GetValueOrDefault("last_reset", DateTime.Now.ToString()).ToString());
                _dailyChallengeCount = (int)(dungeonData.GetValueOrDefault("challenge_count", 0));
                _dailyCompleted = (bool)(dungeonData.GetValueOrDefault("daily_completed", false));

                // Load player dungeon progress
                var playerData = dungeonData.GetValueOrDefault("player_data", null) as Dictionary<string, object>;
                if (playerData != null)
                {
                    foreach (var kvp in playerData)
                    {
                        var data = kvp.Value as Dictionary<string, object>;
                        if (data != null)
                        {
                            var pdd = new PlayerDungeonData
                            {
                                BestFloor = (int)(data.GetValueOrDefault("best_floor", 0)),
                                TimesCompleted = (int)(data.GetValueOrDefault("times_completed", 0)),
                                TotalGoldEarned = (int)(data.GetValueOrDefault("total_gold", 0)),
                                TotalExpEarned = (int)(data.GetValueOrDefault("total_exp", 0)),
                                LastPlayedDate = DateTime.Parse(data.GetValueOrDefault("last_played", DateTime.Now.ToString()).ToString())
                            };
                            PlayerDungeonData[kvp.Key] = pdd;
                        }
                    }
                }
            }
        }

        CheckDailyReset();
    }

    public void SaveData()
    {
        var saveData = SaveSystem.LoadProgress() ?? new Dictionary<string, object>();
        
        var dungeonData = new Dictionary<string, object>
        {
            { "last_reset", _lastDailyReset.ToString() },
            { "challenge_count", _dailyChallengeCount },
            { "daily_completed", _dailyCompleted }
        };

        var playerData = new Dictionary<string, object>();
        foreach (var kvp in PlayerDungeonData)
        {
            playerData[kvp.Key] = new Dictionary<string, object>
            {
                { "best_floor", kvp.Value.BestFloor },
                { "times_completed", kvp.Value.TimesCompleted },
                { "total_gold", kvp.Value.TotalGoldEarned },
                { "total_exp", kvp.Value.TotalExpEarned },
                { "last_played", kvp.Value.LastPlayedDate.ToString() }
            };
        }
        dungeonData["player_data"] = playerData;

        saveData["daily_dungeon"] = dungeonData;
        SaveSystem.SaveProgress(saveData);
    }

    private void CheckDailyReset()
    {
        var now = DateTime.Now;
        if (now.Date > _lastDailyReset.Date)
        {
            // New day, reset daily challenges
            _lastDailyReset = now;
            _dailyChallengeCount = 0;
            _dailyCompleted = false; 
            SaveData();
        }
    }

    public bool CanEnterDungeon(string dungeonId)
    {
        if (_dailyCompleted)
        {
            GD.Print("Daily dungeon already completed today");
            return false;
        }

        var dungeon = DailyDungeonDatabase.GetDungeonById(dungeonId);
        if (dungeon == null)
        {
            GD.PrintErr("Dungeon not found: " + dungeonId);
            return false;
        }

        var player = GetTree().CurrentScene?.GetNode<Player>("Player");
        if (player == null)
        {
            GD.PrintErr("Player not found");
            return false;
        }

        if (player.Level < dungeon.RecommendedLevel)
        {
            GD.Print("Player level too low. Required: " + dungeon.RecommendedLevel);
            return false;
        }

        return true;
    }

    public bool StartDungeon(string dungeonId)
    {
        if (!CanEnterDungeon(dungeonId))
            return false;

        var dungeon = DailyDungeonDatabase.GetDungeonById(dungeonId);
        if (dungeon == null)
            return false;

        _currentDungeon = dungeon;
        _currentFloor = 1;
        _enemiesDefeated = 0;
        _timeRemaining = dungeon.TimeLimit;
        _isInDungeon = true;
        _dungeonStartTime = DateTime.Now;

        // Update daily challenge count
        _dailyChallengeCount++;
        SaveData();

        // Emit signal
        DungeonStarted?.Emit();

        GD.Print("Started dungeon: " + dungeon.Name + " Floor: " + _currentFloor);
        return true;
    }

    public void ExitDungeon()
    {
        if (!_isInDungeon)
            return;

        _isInDungeon = false; 
        _currentDungeon = null;
        _currentFloor = 0;
        _enemiesDefeated = 0;

        DungeonEnded?.Emit();
        GD.Print("Exited dungeon");
    }

    public void OnEnemyDefeated()
    {
        if (!_isInDungeon || _currentDungeon == null)
            return;

        _enemiesDefeated++;

        // Check if floor is cleared (every 3 enemies = 1 floor cleared)
        if (_enemiesDefeated >= 3)
        {
            _enemiesDefeated = 0;
            CompleteFloor();
        }
    }

    private void CompleteFloor()
    {
        if (_currentDungeon == null)
            return;

        _currentFloor++;

        if (_currentFloor > _currentDungeon.TotalFloors)
        {
            // Dungeon completed
            CompleteDungeon();
        }
        else
        {
            FloorCleared?.Emit();
            FloorEntered?.Emit();
            GD.Print("Floor cleared! Current floor: " + _currentFloor);
        }
    }

    private void CompleteDungeon()
    {
        if (_currentDungeon == null)
            return;

        var player = GetTree().CurrentScene?.GetNode<Player>("Player");
        if (player != null)
        {
            // Grant rewards
            player.AddGold(_currentDungeon.GoldReward);
            player.AddExp(_currentDungeon.ExpReward);

            // Add reward items
            foreach (var itemId in _currentDungeon.RewardItems)
            {
                InventoryManager.Instance.AddItem(itemId, 1);
            }
        }

        // Update player dungeon data
        var dungeonId = _currentDungeon.Id;
        if (!PlayerDungeonData.ContainsKey(dungeonId))
        {
            PlayerDungeonData[dungeonId] = new PlayerDungeonData();
        }

        var pdd = PlayerDungeonData[dungeonId];
        pdd.BestFloor = _currentDungeon.TotalFloors;
        pdd.TimesCompleted++;
        pdd.TotalGoldEarned += _currentDungeon.GoldReward;
        pdd.TotalExpEarned += _currentDungeon.ExpReward;
        pdd.LastPlayedDate = DateTime.Now;

        // Mark daily as completed if all dungeons done
        if (_dailyChallengeCount >= 5)
        {
            _dailyCompleted = true;
        }

        SaveData();

        _isInDungeon = false; 
        DungeonCompleted?.Emit();

        GD.Print("Dungeon completed! Rewards: " + _currentDungeon.GoldReward + " gold, " + _currentDungeon.ExpReward + " exp");
    }

    public void FailDungeon()
    {
        if (!_isInDungeon)
            return;

        // Record progress
        var dungeonId = _currentDungeon.Id;
        if (!PlayerDungeonData.ContainsKey(dungeonId))
        {
            PlayerDungeonData[dungeonId] = new PlayerDungeonData();
        }

        var pdd = PlayerDungeonData[dungeonId];
        if (_currentFloor > pdd.BestFloor)
        {
            pdd.BestFloor = _currentFloor;
        }
        pdd.LastPlayedDate = DateTime.Now;

        SaveData();

        _isInDungeon = false; 
        DungeonFailed?.Emit();

        GD.Print("Dungeon failed at floor: " + _currentFloor);
    }

    public void Update(float delta)
    {
        if (!_isInDungeon)
            return;

        _timeRemaining -= delta;
        if (_timeRemaining <= 0)
        {
            FailDungeon();
        }
    }

    // Getters
    public DailyDungeonData GetCurrentDungeon() => _currentDungeon;
    public int GetCurrentFloor() => _currentFloor;
    public float GetTimeRemaining() => _timeRemaining;
    public bool IsInDungeon() => _isInDungeon;
    public int GetDailyChallengeCount() => _dailyChallengeCount;
    public bool IsDailyCompleted() => _dailyCompleted;

    public PlayerDungeonData GetPlayerDungeonData(string dungeonId)
    {
        if (PlayerDungeonData.ContainsKey(dungeonId))
            return PlayerDungeonData[dungeonId];
        return null;
    }

    public Dictionary<string, PlayerDungeonData> GetAllPlayerDungeonData()
    {
        return PlayerDungeonData;
    }

    public List<DailyDungeonData> GetAvailableDungeons()
    {
        var player = GetTree().CurrentScene?.GetNode<Player>("Player");
        if (player == null)
            return new List<DailyDungeonData>();

        var available = new List<DailyDungeonData>();
        foreach (var dungeon in DailyDungeonDatabase.GetAllDungeons())
        {
            if (player.Level >= dungeon.RecommendedLevel - 5) // Show dungeons within 5 levels
            {
                available.Add(dungeon);
            }
        }
        return available;
    }

    // ===== 持久化方法 =====

    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        data["current_floor"] = _currentFloor;
        data["enemies_defeated"] = _enemiesDefeated;
        data["time_remaining"] = _timeRemaining;
        data["is_in_dungeon"] = _isInDungeon;
        data["dungeon_start_time"] = _dungeonStartTime.ToString("o");
        data["last_daily_reset"] = _lastDailyReset.ToString("o");
        data["daily_challenge_count"] = _dailyChallengeCount;
        data["daily_completed"] = _dailyCompleted;
        
        // 玩家地下城数据
        var playerDataList = new List<Dictionary>();
        foreach (var kvp in PlayerDungeonData)
        {
            var pd = new Dictionary();
            pd["dungeon_id"] = kvp.Key;
            pd["best_floor"] = kvp.Value.BestFloor;
            pd["times_completed"] = kvp.Value.TimesCompleted;
            pd["total_gold_earned"] = kvp.Value.TotalGoldEarned;
            pd["total_exp_earned"] = kvp.Value.TotalExpEarned;
            pd["last_played_date"] = kvp.Value.LastPlayedDate.ToString("o");
            playerDataList.Add(pd);
        }
        data["player_dungeon_data"] = playerDataList;
        
        return data;
    }

    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        _currentFloor = (int)(data.GetValueOrDefault("current_floor", 0));
        _enemiesDefeated = (int)(data.GetValueOrDefault("enemies_defeated", 0));
        _timeRemaining = (float)(data.GetValueOrDefault("time_remaining", 0f));
        _isInDungeon = (bool)(data.GetValueOrDefault("is_in_dungeon", false));
        
        if (data.Contains("dungeon_start_time"))
            _dungeonStartTime = DateTime.Parse(data["dungeon_start_time"].ToString());
        if (data.Contains("last_daily_reset"))
            _lastDailyReset = DateTime.Parse(data["last_daily_reset"].ToString());
        
        _dailyChallengeCount = (int)(data.GetValueOrDefault("daily_challenge_count", 0));
        _dailyCompleted = (bool)(data.GetValueOrDefault("daily_completed", false));
        
        // 恢复玩家地下城数据
        PlayerDungeonData.Clear();
        if (data.Contains("player_dungeon_data"))
        {
            var playerDataList = data["player_dungeon_data"] as List<Dictionary>;
            if (playerDataList != null)
            {
                foreach (var pd in playerDataList)
                {
                    var pdata = new PlayerDungeonData();
                    pdata.BestFloor = (int)(pd.GetValueOrDefault("best_floor", 0));
                    pdata.TimesCompleted = (int)(pd.GetValueOrDefault("times_completed", 0));
                    pdata.TotalGoldEarned = (int)(pd.GetValueOrDefault("total_gold_earned", 0));
                    pdata.TotalExpEarned = (int)(pd.GetValueOrDefault("total_exp_earned", 0));
                    if (pd.Contains("last_played_date"))
                        pdata.LastPlayedDate = DateTime.Parse(pd["last_played_date"].ToString());
                    
                    var dungeonId = pd["dungeon_id"].ToString();
                    PlayerDungeonData[dungeonId] = pdata;
                }
            }
        }
    }
}

public class PlayerDungeonData
{
    public int BestFloor { get; set; }
    public int TimesCompleted { get; set; }
    public int TotalGoldEarned { get; set; }
    public int TotalExpEarned { get; set; }
    public DateTime LastPlayedDate { get; set; }
}
