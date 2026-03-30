using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Managers
{
    /// <summary>
    /// 玩家生成管理器 - 负责玩家的生成、位置管理和重生逻辑
    /// </summary>
    public class PlayerSpawnManager : BaseSystem
    {
        public static PlayerSpawnManager Instance { get; private set; }
        
        /// <summary>
        /// 优先级（数值越小越先初始化）
        /// </summary>
        public override int Priority => 20;
        
        // Player reference
        private Player _player;
        private PackedScene _playerScene;
        
        // Spawn configuration
        private Vector2 _defaultSpawnPosition = new Vector2(640, 360);
        private Vector2 _lastSafePosition;
        private bool _hasSpawned = false;
        
        // Spawn points
        private Dictionary<string, Vector2> _namedSpawnPoints = new Dictionary<string, Vector2>();
        private List<Vector2> _recentSpawnPositions = new List<Vector2>();
        private const int MaxRecentPositions = 5;
        
        // Player state
        private bool _isPlayerDead = false;
        private float _respawnTimer = 0f;
        private float _respawnDelay = 3f;
        
        // Events
        public event Action<Player> OnPlayerSpawned;
        public event Action OnPlayerDied;
        public event Action<Vector2> OnPlayerRespawned;
        
        public override void _Ready()
        {
            Instance = this;
            base._Ready();
            
            // Register default spawn point
            RegisterSpawnPoint("default", _defaultSpawnPosition);
            RegisterSpawnPoint("town", new Vector2(100, 200));
            RegisterSpawnPoint("dungeon_entrance", new Vector2(500, 300));
        }
        
        protected override void Initialize()
        {
            GD.Print("[PlayerSpawnManager] Initialized");
        }
        
        /// <summary>
        /// 设置玩家场景资源
        /// </summary>
        public void SetPlayerScene(PackedScene scene)
        {
            _playerScene = scene;
        }
        
        /// <summary>
        /// 生成玩家
        /// </summary>
        public Player SpawnPlayer(Vector2? position = null, bool preserveStats = true)
        {
            if (_playerScene == null)
            {
                GD.PrintErr("[PlayerSpawnManager] PlayerScene not set!");
                return null;
            }
            
            // If player already exists, just reposition
            if (_player != null && !_isPlayerDead)
            {
                var spawnPos = position ?? _defaultSpawnPosition;
                _player.GlobalPosition = spawnPos;
                SaveSafePosition(spawnPos);
                OnPlayerRespawned?.Invoke(spawnPos);
                return _player;
            }
            
            // Create new player instance
            _player = _playerScene.Instantiate<Player>();
            if (_player == null)
            {
                GD.PrintErr("[PlayerSpawnManager] Failed to instantiate player!");
                return null;
            }
            
            // Set position
            var spawnPosition = position ?? _defaultSpawnPosition;
            _player.GlobalPosition = spawnPosition;
            _player.AddToGroup("player");
            
            // Add to parent
            var mainNode = GetTree().CurrentScene;
            if (mainNode != null)
            {
                mainNode.AddChild(_player);
            }
            
            _hasSpawned = true;
            _isPlayerDead = false;
            SaveSafePosition(spawnPosition);
            
            GD.Print("[PlayerSpawnManager] Player spawned at " + spawnPosition);
            OnPlayerSpawned?.Invoke(_player);
            
            return _player;
        }
        
        /// <summary>
        /// 玩家死亡处理
        /// </summary>
        public void HandlePlayerDeath()
        {
            if (_isPlayerDead) return;
            
            _isPlayerDead = true;
            GD.Print("[PlayerSpawnManager] Player died");
            OnPlayerDied?.Invoke();
            
            // Start respawn timer
            _respawnTimer = _respawnDelay;
        }
        
        /// <summary>
        /// 立即重生玩家
        /// </summary>
        public void RespawnPlayer()
        {
            if (!_hasSpawned)
            {
                SpawnPlayer();
                return;
            }
            
            var respawnPos = GetLastSafePosition();
            SpawnPlayer(respawnPos);
            _isPlayerDead = false;
            _respawnTimer = 0f;
        }
        
        /// <summary>
        /// 强制玩家移动到指定位置
        /// </summary>
        public void TeleportPlayer(Vector2 position)
        {
            if (_player != null)
            {
                _player.GlobalPosition = position;
                SaveSafePosition(position);
                GD.Print("[PlayerSpawnManager] Player teleported to " + position);
            }
        }
        
        /// <summary>
        /// 注册命名生成点
        /// </summary>
        public void RegisterSpawnPoint(string name, Vector2 position)
        {
            _namedSpawnPoints[name] = position;
        }
        
        /// <summary>
        /// 从命名生成点生成玩家
        /// </summary>
        public void SpawnAtNamedPoint(string pointName)
        {
            if (_namedSpawnPoints.TryGetValue(pointName, out var position))
            {
                SpawnPlayer(position);
            }
            else
            {
                GD.PrintWarn($"[PlayerSpawnManager] Spawn point '{pointName}' not found!");
                SpawnPlayer();
            }
        }
        
        /// <summary>
        /// 保存当前位置为安全位置
        /// </summary>
        public void SaveSafePosition(Vector2 position)
        {
            _lastSafePosition = position;
            
            // Add to recent positions
            _recentSpawnPositions.Add(position);
            if (_recentSpawnPositions.Count > MaxRecentPositions)
            {
                _recentSpawnPositions.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// 获取最后的安全位置
        /// </summary>
        public Vector2 GetLastSafePosition()
        {
            return _lastSafePosition;
        }
        
        /// <summary>
        /// 获取最近的重生位置列表
        /// </summary>
        public List<Vector2> GetRecentSpawnPositions()
        {
            return new List<Vector2>(_recentSpawnPositions);
        }
        
        /// <summary>
        /// 设置重生延迟
        /// </summary>
        public void SetRespawnDelay(float delay)
        {
            _respawnDelay = Math.Max(0f, delay);
        }
        
        /// <summary>
        /// 获取当前玩家
        /// </summary>
        public Player GetPlayer()
        {
            return _player;
        }
        
        /// <summary>
        /// 检查玩家是否存活
        /// </summary>
        public bool IsPlayerAlive()
        {
            return _player != null && !_isPlayerDead;
        }
        
        /// <summary>
        /// 设置玩家引用（从外部设置）
        /// </summary>
        public void SetPlayer(Player player)
        {
            _player = player;
            if (player != null)
            {
                _hasSpawned = true;
                _isPlayerDead = false;
            }
        }
        
        // Getters
        public Vector2 GetDefaultSpawnPosition() => _defaultSpawnPosition;
        public bool HasPlayerSpawned() => _hasSpawned;
        public bool IsPlayerDead() => _isPlayerDead;
        public float GetRespawnDelay() => _respawnDelay;
        public Dictionary<string, Vector2> GetNamedSpawnPoints() => new Dictionary<string, Vector2>(_namedSpawnPoints);
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary
            {
                { "hasSpawned", _hasSpawned },
                { "defaultSpawnPositionX", _defaultSpawnPosition.X },
                { "defaultSpawnPositionY", _defaultSpawnPosition.Y },
                { "lastSafePositionX", _lastSafePosition.X },
                { "lastSafePositionY", _lastSafePosition.Y },
                { "respawnDelay", _respawnDelay },
                { "namedSpawnPoints", _namedSpawnPoints }
            };
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.Contains("hasSpawned")) _hasSpawned = (bool)data["hasSpawned"];
            if (data.Contains("defaultSpawnPositionX")) 
                _defaultSpawnPosition.x = Convert.ToSingle(data["defaultSpawnPositionX"]);
            if (data.Contains("defaultSpawnPositionY")) 
                _defaultSpawnPosition.y = Convert.ToSingle(data["defaultSpawnPositionY"]);
            if (data.Contains("lastSafePositionX")) 
                _lastSafePosition.x = Convert.ToSingle(data["lastSafePositionX"]);
            if (data.Contains("lastSafePositionY")) 
                _lastSafePosition.y = Convert.ToSingle(data["lastSafePositionY"]);
            if (data.Contains("respawnDelay")) _respawnDelay = Convert.ToSingle(data["respawnDelay"]);
            if (data.Contains("namedSpawnPoints")) 
                _namedSpawnPoints = data["namedSpawnPoints"] as Dictionary<string, Vector2>;
        }
    }
}
