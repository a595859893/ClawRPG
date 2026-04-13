using Godot;
using System;

/// <summary>
/// 玩家事件数据
/// </summary>
public class PlayerEventData
{
    public Player Player { get; set; }
    public Vector3 Position { get; set; }
    public float Health { get; set; }
    public float MaxHealth { get; set; }
    public float HealthPercentage => MaxHealth > 0 ? (float)Health / MaxHealth : 0f;
    
    public PlayerEventData() { }
    
    public PlayerEventData(Player player)
    {
        Player = player;
        if (player != null)
        {
            Position = new Vector3(player.GlobalPosition.X, player.GlobalPosition.Y, 0);
            Health = player.CurrentHealth;
            MaxHealth = player.MaxHealth;
        }
    }
}

/// <summary>
/// 玩家死亡事件数据
/// </summary>
public class PlayerDiedEventData
{
    public Player Player { get; set; }
    public int DeathCount { get; set; }
    public Vector3 DeathPosition { get; set; }
    public string CauseOfDeath { get; set; } = "unknown";
    
    public PlayerDiedEventData() { }
    
    public PlayerDiedEventData(Player player, int deathCount, Vector3 position = default)
    {
        Player = player;
        DeathCount = deathCount;
        DeathPosition = position;
    }
}

/// <summary>
/// 玩家重生事件数据
/// </summary>
public class PlayerRespawnEventData
{
    public Player Player { get; set; }
    public Vector3 RespawnPosition { get; set; }
    public int CurrentDeathCount { get; set; }
    
    public PlayerRespawnEventData() { }
    
    public PlayerRespawnEventData(Player player, Vector3 position, int deathCount)
    {
        Player = player;
        RespawnPosition = position;
        CurrentDeathCount = deathCount;
    }
}

/// <summary>
/// 玩家生命值变化事件数据
/// </summary>
public class PlayerHealthChangedEventData
{
    public Player Player { get; set; }
    public int OldHealth { get; set; }
    public int NewHealth { get; set; }
    public float MaxHealth { get; set; }
    public int Delta => NewHealth - OldHealth;
    public float OldPercentage => MaxHealth > 0 ? (float)OldHealth / MaxHealth : 0f;
    public float NewPercentage => MaxHealth > 0 ? (float)NewHealth / MaxHealth : 0f;
    
    public PlayerHealthChangedEventData() { }
    
    public PlayerHealthChangedEventData(Player player, int oldHealth, int newHealth, int maxHealth)
    {
        Player = player;
        OldHealth = oldHealth;
        NewHealth = newHealth;
        MaxHealth = maxHealth;
    }
}

/// <summary>
/// 玩家升级事件数据
/// </summary>
public class PlayerLevelUpEventData
{
    public Player Player { get; set; }
    public int OldLevel { get; set; }
    public int NewLevel { get; set; }
    public int Delta => NewLevel - OldLevel;
    
    public PlayerLevelUpEventData() { }
    
    public PlayerLevelUpEventData(Player player, int oldLevel, int newLevel)
    {
        Player = player;
        OldLevel = oldLevel;
        NewLevel = newLevel;
    }
}
