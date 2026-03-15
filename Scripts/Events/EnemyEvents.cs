using Godot;
using System;

/// <summary>
/// 敌人事件数据
/// </summary>
public class EnemyEventData
{
    public Enemy Enemy { get; set; }
    public Vector3 Position { get; set; }
    public string EnemyType { get; set; } = "default";
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    
    public EnemyEventData() { }
    
    public EnemyEventData(Enemy enemy)
    {
        Enemy = enemy;
        if (enemy != null)
        {
            Position = enemy.GlobalPosition;
            // 尝试获取敌人类型和生命值
            if (enemy.HasMethod("GetEnemyType"))
            {
                EnemyType = enemy.Call("GetEnemyType") as string ?? "default";
            }
        }
    }
}

/// <summary>
/// 敌人死亡事件数据
/// </summary>
public class EnemyDiedEventData
{
    public Enemy Enemy { get; set; }
    public Vector3 DeathPosition { get; set; }
    public string EnemyType { get; set; } = "default";
    public int KillCount { get; set; }
    public float DamageDealt { get; set; }
    public Player Killer { get; set; }
    
    public EnemyDiedEventData() { }
    
    public EnemyDiedEventData(Enemy enemy, int killCount, Vector3 position = default, float damage = 0f)
    {
        Enemy = enemy;
        KillCount = killCount;
        DeathPosition = position;
        DamageDealt = damage;
        
        if (enemy != null && enemy.HasMethod("GetEnemyType"))
        {
            EnemyType = enemy.Call("GetEnemyType") as string ?? "default";
        }
    }
}

/// <summary>
/// 敌人受伤事件数据
/// </summary>
public class EnemyDamagedEventData
{
    public Enemy Enemy { get; set; }
    public int OldHealth { get; set; }
    public int NewHealth { get; set; }
    public int Damage { get; set; }
    public Vector3 DamageSourcePosition { get; set; }
    public string DamageType { get; set; } = "physical";
    public Player Attacker { get; set; }
    
    public EnemyDamagedEventData() { }
    
    public EnemyDamagedEventData(Enemy enemy, int oldHealth, int newHealth, int damage)
    {
        Enemy = enemy;
        OldHealth = oldHealth;
        NewHealth = newHealth;
        Damage = damage;
    }
}

/// <summary>
/// 敌人生成事件数据
/// </summary>
public class EnemySpawnedEventData
{
    public Enemy Enemy { get; set; }
    public Vector3 SpawnPosition { get; set; }
    public string EnemyType { get; set; } = "default";
    public int ActiveEnemyCount { get; set; }
    
    public EnemySpawnedEventData() { }
    
    public EnemySpawnedEventData(Enemy enemy, Vector3 position, int activeCount)
    {
        Enemy = enemy;
        SpawnPosition = position;
        ActiveEnemyCount = activeCount;
        
        if (enemy != null && enemy.HasMethod("GetEnemyType"))
        {
            EnemyType = enemy.Call("GetEnemyType") as string ?? "default";
        }
    }
}
