using Godot;
using System;
using ClawRPG.Scripts.Managers;

/// <summary>
/// 场景事件数据
/// </summary>
public class SceneEventData
{
    public string ScenePath { get; set; } = "";
    public string SceneName { get; set; } = "";
    public bool IsLoading { get; set; }
    public float LoadingProgress { get; set; }
    
    public SceneEventData() { }
    
    public SceneEventData(string path, bool isLoading = false, float progress = 0f)
    {
        ScenePath = path;
        SceneName = System.IO.Path.GetFileNameWithoutExtension(path);
        IsLoading = isLoading;
        LoadingProgress = progress;
    }
}

/// <summary>
/// 场景切换事件数据
/// </summary>
public class SceneChangedEventData
{
    public string OldScenePath { get; set; } = "";
    public string NewScenePath { get; set; } = "";
    public string OldSceneName { get; set; } = "";
    public string NewSceneName { get; set; } = "";
    
    public SceneChangedEventData() { }
    
    public SceneChangedEventData(string oldPath, string newPath)
    {
        OldScenePath = oldPath;
        NewScenePath = newPath;
        OldSceneName = System.IO.Path.GetFileNameWithoutExtension(oldPath);
        NewSceneName = System.IO.Path.GetFileNameWithoutExtension(newPath);
    }
}

/// <summary>
/// 游戏状态事件数据
/// </summary>
public class GameStateEventData
{
    public GameStateManager.GameState OldState { get; set; }
    public GameStateManager.GameState NewState { get; set; }
    
    public GameStateEventData() { }
    
    public GameStateEventData(GameStateManager.GameState oldState, GameStateManager.GameState newState)
    {
        OldState = oldState;
        NewState = newState;
    }
}

/// <summary>
/// 游戏暂停/恢复事件数据
/// </summary>
public class GamePauseEventData
{
    public bool IsPaused { get; set; }
    public GameStateManager.GameState CurrentState { get; set; }
    public float PlayTime { get; set; }
    
    public GamePauseEventData() { }
    
    public GamePauseEventData(bool paused, GameStateManager.GameState state, float playTime)
    {
        IsPaused = paused;
        CurrentState = state;
        PlayTime = playTime;
    }
}

/// <summary>
/// 游戏结束事件数据
/// </summary>
public class GameOverEventData
{
    public int TotalPlayTime { get; set; }
    public int KillCount { get; set; }
    public int DeathCount { get; set; }
    public int CurrentDay { get; set; }
    public string Cause { get; set; } = "unknown";
    
    public GameOverEventData() { }
    
    public GameOverEventData(int playTime, int kills, int deaths, int day, string cause = "unknown")
    {
        TotalPlayTime = playTime;
        KillCount = kills;
        DeathCount = deaths;
        CurrentDay = day;
        Cause = cause;
    }
}

/// <summary>
/// 游戏时间事件数据
/// </summary>
public class GameTimeEventData
{
    public int CurrentDay { get; set; }
    public float DayProgress { get; set; } // 0.0 - 1.0
    public float TotalPlayTime { get; set; }
    public float SessionPlayTime { get; set; }
    
    public GameTimeEventData() { }
    
    public GameTimeEventData(int day, float dayProgress, float totalTime, float sessionTime)
    {
        CurrentDay = day;
        DayProgress = dayProgress;
        TotalPlayTime = totalTime;
        SessionPlayTime = sessionTime;
    }
}
