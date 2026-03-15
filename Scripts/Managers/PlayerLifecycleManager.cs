using Godot;
using System;
using ClawRPG.Scripts.Managers;

/// <summary>
/// 玩家生命周期管理器 - 负责玩家的生成、死亡、重生和状态管理
/// 使用 EventBusManager 进行事件通信，减少系统耦合
/// </summary>
public class PlayerLifecycleManager : ManagerBase
{
    public static PlayerLifecycleManager Instance { get; private set; }
    
    /// <summary>
    /// 当前玩家节点
    /// </summary>
    public Player CurrentPlayer { get; private set; }
    
    /// <summary>
    /// 玩家是否存活
    /// </summary>
    public bool IsPlayerAlive { get; private set; } = true;
    
    /// <summary>
    /// 玩家是否正在重生中
    /// </summary>
    public bool IsRespawning { get; private set; } = false;
    
    /// <summary>
    /// 玩家死亡次数
    /// </summary>
    public int DeathCount { get; private set; } = 0;
    
    /// <summary>
    /// 玩家重生点
    /// </summary>
    public Vector3 RespawnPoint { get; set; } = Vector3.Zero;
    
    /// <summary>
    /// 重生延迟（秒）
    /// </summary>
    public float RespawnDelay { get; set; } = 3f;
    
    /// <summary>
    /// 当前重生计时器
    /// </summary>
    private float _respawnTimer = 0f;
    
    /// <summary>
    /// 玩家场景
    /// </summary>
    private PackedScene _playerScene;
    
    // 事件
    public event Action<Player> OnPlayerSpawned;
    public event Action<Player> OnPlayerDied;
    public event Action<Player> OnPlayerRespawned;
    public event Action<float> OnHealthChanged;
    public event Action<int> OnLevelUp;
    
    public override void _Ready()
    {
        Instance = this;
        base._Ready();
    }
    
    protected override void Initialize()
    {
        GD.Print("[PlayerLifecycleManager] Initialized");
        
        // 加载玩家场景
        LoadPlayerScene();
        
        // 如果有重生点设置，使用它
        if (RespawnPoint == Vector3.Zero)
        {
            RespawnPoint = new Vector3(0, 2, 0); // 默认重生点
        }
        
        NotifyInitialized();
    }
    
    /// <summary>
    /// 加载玩家场景
    /// </summary>
    private void LoadPlayerScene()
    {
        // 尝试从 Main 获取玩家场景
        var main = GetNode("/root/Main");
        if (main != null && main.HasMethod("GetPlayerScene"))
        {
            _playerScene = main.Call("GetPlayerScene") as PackedScene;
        }
        
        // 如果没有，尝试默认路径
        if (_playerScene == null)
        {
            _playerScene = GD.Load<PackedScene>("res://Player.tscn");
        }
    }
    
    /// <summary>
    /// 生成玩家
    /// </summary>
    public Player SpawnPlayer(Vector3? position = null)
    {
        if (_playerScene == null)
        {
            GD.PrintErr("[PlayerLifecycleManager] Player scene not loaded!");
            return null;
        }
        
        // 如果已有玩家，移除
        if (CurrentPlayer != null)
        {
            CurrentPlayer.QueueFree();
        }
        
        // 实例化玩家
        var player = _playerScene.Instance() as Player;
        if (player == null)
        {
            GD.PrintErr("[PlayerLifecycleManager] Failed to instantiate player!");
            return null;
        }
        
        // 设置位置
        var spawnPos = position ?? RespawnPoint;
        player.GlobalPosition = spawnPos;
        
        // 添加到场景
        var root = GetTree().Root;
        root.AddChild(player);
        
        CurrentPlayer = player;
        IsPlayerAlive = true;
        IsRespawning = false;
        
        GD.Print($"[PlayerLifecycleManager] Player spawned at {spawnPos}");
        
        // 触发本地事件
        OnPlayerSpawned?.Invoke(player);
        
        // 通过事件总线发布全局事件
        if (EventBusManager.Instance != null)
        {
            EventBusManager.Instance.Emit(EventBusManager.Events.PlayerSpawned, player);
        }
        
        return player;
    }
    
    /// <summary>
    /// 玩家死亡
    /// </summary>
    public void PlayerDied()
    {
        if (!IsPlayerAlive || IsRespawning) return;
        
        IsPlayerAlive = false;
        DeathCount++;
        
        GD.Print($"[PlayerLifecycleManager] Player died! Death count: {DeathCount}");
        
        // 触发本地事件
        OnPlayerDied?.Invoke(CurrentPlayer);
        
        // 通过事件总线发布全局事件
        if (EventBusManager.Instance != null)
        {
            EventBusManager.Instance.Emit(EventBusManager.Events.PlayerDied, CurrentPlayer);
        }
        
        // 开始重生计时
        StartRespawn();
    }
    
    /// <summary>
    /// 开始重生
    /// </summary>
    private void StartRespawn()
    {
        IsRespawning = true;
        _respawnTimer = RespawnDelay;
        
        GD.Print($"[PlayerLifecycleManager] Respawn in {RespawnDelay} seconds...");
    }
    
    /// <summary>
    /// 玩家重生
    /// </summary>
    public void RespawnPlayer()
    {
        if (CurrentPlayer != null)
        {
            CurrentPlayer.QueueFree();
        }
        
        SpawnPlayer(RespawnPoint);
        
        GD.Print("[PlayerLifecycleManager] Player respawned!");
        
        // 触发本地事件
        OnPlayerRespawned?.Invoke(CurrentPlayer);
        
        // 通过事件总线发布全局事件
        if (EventBusManager.Instance != null)
        {
            EventBusManager.Instance.Emit(EventBusManager.Events.PlayerRespawned, CurrentPlayer);
        }
    }
    
    /// <summary>
    /// 强制立即重生
    /// </summary>
    public void ForceRespawn()
    {
        IsRespawning = false;
        RespawnPlayer();
    }
    
    /// <summary>
    /// 设置重生点
    /// </summary>
    public void SetRespawnPoint(Vector3 point)
    {
        RespawnPoint = point;
        GD.Print($"[PlayerLifecycleManager] Respawn point set to {point}");
    }
    
    /// <summary>
    /// 获取玩家
    /// </summary>
    public Player GetPlayer()
    {
        return CurrentPlayer;
    }
    
    /// <summary>
    /// 检查玩家是否可用
    /// </summary>
    public bool IsPlayerValid()
    {
        return CurrentPlayer != null && IsInstanceValid(CurrentPlayer) && IsPlayerAlive;
    }
    
    public override void ManagerUpdate(double delta)
    {
        // 处理重生计时
        if (IsRespawning && !IsPlayerAlive)
        {
            _respawnTimer -= (float)delta;
            if (_respawnTimer <= 0)
            {
                RespawnPlayer();
            }
        }
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return new Dictionary
        {
            { "deathCount", DeathCount },
            { "respawnPointX", RespawnPoint.x },
            { "respawnPointY", RespawnPoint.y },
            { "respawnPointZ", RespawnPoint.z },
            { "respawnDelay", RespawnDelay }
        };
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("deathCount"))
            DeathCount = Convert.ToInt32(data["deathCount"]);
        
        if (data.Contains("respawnPointX") && data.Contains("respawnPointY") && data.Contains("respawnPointZ"))
        {
            RespawnPoint = new Vector3(
                Convert.ToSingle(data["respawnPointX"]),
                Convert.ToSingle(data["respawnPointY"]),
                Convert.ToSingle(data["respawnPointZ"])
            );
        }
        
        if (data.Contains("respawnDelay"))
            RespawnDelay = Convert.ToSingle(data["respawnDelay"]);
    }
}
