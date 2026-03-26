using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Managers;
using ClawRPG.Scripts.Events;

/// <summary>
/// 敌人生命周期管理器 - 负责敌人的生成、AI 更新、死亡和状态管理
/// 使用 EventBusManager 进行事件通信，减少系统耦合
/// </summary>
public class EnemyLifecycleManager : ManagerBase
{
    public static EnemyLifecycleManager Instance { get; private set; }
    
    /// <summary>
    /// 优先级（数值越小越先初始化）
    /// </summary>
    public override int Priority => 30;
    
    /// <summary>
    /// 所有活跃敌人列表
    /// </summary>
    public List<Enemy> ActiveEnemies { get; private set; } = new List<Enemy>();
    
    /// <summary>
    /// 敌人最大数量
    /// </summary>
    public int MaxEnemies { get; set; } = 50;
    
    /// <summary>
    /// 敌人生成间隔（秒）
    /// </summary>
    public float SpawnInterval { get; set; } = 5f;
    
    /// <summary>
    /// 敌人击杀计数
    /// </summary>
    public int KillCount { get; private set; } = 0;
    
    /// <summary>
    /// 当前敌人生成计时器
    /// </summary>
    private float _spawnTimer = 0f;
    
    /// <summary>
    /// 敌人场景
    /// </summary>
    private PackedScene _enemyScene;
    
    /// <summary>
    /// 敌人出生点列表
    /// </summary>
    private List<Vector3> _spawnPoints = new List<Vector3>();
    
    // 事件
    public event Action<Enemy> OnEnemySpawned;
    public event Action<Enemy> OnEnemyDied;
    public event Action<Enemy> OnEnemyDamaged;
    public event Action<int> OnKillCountChanged;
    
    public override void _Ready()
    {
        Instance = this;
        base._Ready();
    }
    
    protected override void Initialize()
    {
        GD.Print("[EnemyLifecycleManager] Initialized");
        
        // 加载敌人场景
        LoadEnemyScene();
        
        // 添加默认出生点
        _spawnPoints.Add(new Vector3(10, 0, 0));
        _spawnPoints.Add(new Vector3(-10, 0, 0));
        _spawnPoints.Add(new Vector3(0, 0, 10));
        _spawnPoints.Add(new Vector3(0, 0, -10));
        
        NotifyInitialized();
    }
    
    /// <summary>
    /// 加载敌人场景
    /// </summary>
    private void LoadEnemyScene()
    {
        var main = GetNode("/root/Main");
        if (main != null && main.HasMethod("GetEnemyScene"))
        {
            _enemyScene = main.Call("GetEnemyScene") as PackedScene;
        }
        
        if (_enemyScene == null)
        {
            _enemyScene = GD.Load<PackedScene>("res://Enemies/Enemy.tscn");
        }
    }
    
    /// <summary>
    /// 生成敌人
    /// </summary>
    public Enemy SpawnEnemy(Vector3? position = null, string enemyType = "default")
    {
        if (ActiveEnemies.Count >= MaxEnemies)
        {
            GD.PrintWarn("[EnemyLifecycleManager] Max enemies reached!");
            return null;
        }
        
        if (_enemyScene == null)
        {
            GD.PrintErr("[EnemyLifecycleManager] Enemy scene not loaded!");
            return null;
        }
        
        // 实例化敌人
        var enemy = _enemyScene.Instance() as Enemy;
        if (enemy == null)
        {
            GD.PrintErr("[EnemyLifecycleManager] Failed to instantiate enemy!");
            return null;
        }
        
        // 设置位置
        var spawnPos = position ?? GetRandomSpawnPoint();
        enemy.GlobalPosition = spawnPos;
        
        // 添加到场景
        GetTree().CurrentScene?.AddChild(enemy);
        
        // 添加到活跃列表
        ActiveEnemies.Add(enemy);
        

        
        GD.Print($"[EnemyLifecycleManager] Enemy spawned at {spawnPos}");
        
        // 触发本地事件
        OnEnemySpawned?.Invoke(enemy);
        
        // 通过事件总线发布全局事件 (REQ-112-05: 使用 EventData 封装)
        if (EventBusManager.Instance != null)
        {
            var spawnData = new EnemySpawnedEventData(enemy, new Vector3(spawnPos.X, spawnPos.Y, 0), ActiveEnemies.Count);
            EventBusManager.Instance.Emit(EventBusManager.Events.EnemySpawned, spawnData);
        }
        
        return enemy;
    }
    
    /// <summary>
    /// 批量生成敌人
    /// </summary>
    public void SpawnEnemies(int count, Vector3? center = null)
    {
        for (int i = 0; i < count; i++)
        {
            var offset = new Vector3(
                GD.Randf() * 20 - 10,
                0,
                GD.Randf() * 20 - 10
            );
            var pos = center.HasValue ? center.Value + offset : offset;
            SpawnEnemy(pos);
        }
    }
    
    /// <summary>
    /// 移除敌人
    /// </summary>
    public void RemoveEnemy(Enemy enemy)
    {
        if (enemy == null) return;
        
        if (ActiveEnemies.Contains(enemy))
        {
            ActiveEnemies.Remove(enemy);
        }
        
        // Emit EnemyDied to EventBusManager (REQ-112-05: 事件驱动集成)
        var pos = enemy.GlobalPosition;
        var diedData = new EnemyDiedEventData(enemy, ActiveEnemies.Count + 1, new Vector3(pos.X, pos.Y, 0));
        EventBusManager.Instance?.Emit(EventBusManager.Events.EnemyDied, diedData);
        
        if (IsInstanceValid(enemy))
        {
            enemy.QueueFree();
        }
    }
    
    /// <summary>
    /// 清除所有敌人
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (var enemy in ActiveEnemies.ToArray())
        {
            if (IsInstanceValid(enemy))
            {
                enemy.QueueFree();
            }
        }
        ActiveEnemies.Clear();
        GD.Print("[EnemyLifecycleManager] All enemies cleared");
    }
    
    /// <summary>
    /// 获取随机出生点
    /// </summary>
    private Vector3 GetRandomSpawnPoint()
    {
        if (_spawnPoints.Count == 0) return Vector3.Zero;
        return _spawnPoints[GD.Randi() % _spawnPoints.Count];
    }
    
    /// <summary>
    /// 添加出生点
    /// </summary>
    public void AddSpawnPoint(Vector3 point)
    {
        if (!_spawnPoints.Contains(point))
        {
            _spawnPoints.Add(point);
        }
    }
    
    /// <summary>
    /// 移除出生点
    /// </summary>
    public void RemoveSpawnPoint(Vector3 point)
    {
        _spawnPoints.Remove(point);
    }
    
    /// <summary>
    /// 获取最近的敌人
    /// </summary>
    public Enemy GetNearestEnemy(Vector3 fromPosition)
    {
        Enemy nearest = null;
        float nearestDist = float.MaxValue;
        
        foreach (var enemy in ActiveEnemies)
        {
            if (!IsInstanceValid(enemy)) continue;
            
            var dist = fromPosition.DistanceTo(enemy.GlobalPosition);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = enemy;
            }
        }
        
        return nearest;
    }
    
    /// <summary>
    /// 获取范围内的所有敌人
    /// </summary>
    public List<Enemy> GetEnemiesInRange(Vector3 position, float range)
    {
        var enemies = new List<Enemy>();
        
        foreach (var enemy in ActiveEnemies)
        {
            if (!IsInstanceValid(enemy)) continue;
            
            if (position.DistanceTo(enemy.GlobalPosition) <= range)
            {
                enemies.Add(enemy);
            }
        }
        
        return enemies;
    }
    

    /// <summary>
    /// 更新敌人 AI
    /// </summary>
    public void UpdateEnemyAI(double delta)
    {
        // 移除无效的敌人
        ActiveEnemies.RemoveAll(e => !IsInstanceValid(e));
        
        // 更新每个敌人的 AI
        foreach (var enemy in ActiveEnemies)
        {
            if (!IsInstanceValid(enemy)) continue;
            
            // 调用敌人的 AI 更新方法（如果存在）
            if (enemy.HasMethod("UpdateAI"))
            {
                enemy.Call("UpdateAI", delta);
            }
        }
    }
    
    /// <summary>
    /// 敌人生成计时更新
    /// </summary>
    public override void ManagerUpdate(double delta)
    {
        // 自动生成敌人（可选功能）
        if (SpawnInterval > 0 && ActiveEnemies.Count < MaxEnemies)
        {
            _spawnTimer += (float)delta;
            if (_spawnTimer >= SpawnInterval)
            {
                _spawnTimer = 0f;
                SpawnEnemy();
            }
        }
        
        // 更新敌人 AI
        UpdateEnemyAI(delta);
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return new Dictionary
        {
            { "killCount", KillCount },
            { "maxEnemies", MaxEnemies },
            { "spawnInterval", SpawnInterval }
        };
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("killCount"))
            KillCount = Convert.ToInt32(data["killCount"]);
        if (data.Contains("maxEnemies"))
            MaxEnemies = Convert.ToInt32(data["maxEnemies"]);
        if (data.Contains("spawnInterval"))
            SpawnInterval = Convert.ToSingle(data["spawnInterval"]);
    }
}
