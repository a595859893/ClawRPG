using Godot;
using System;
using ClawRPG.Scripts.Managers;
using ClawRPG.Scripts.Systems;
using ClawRPG.Scripts.UI;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// MainCoordinatorSystem - 负责 Main 的初始化协调和节点结构创建
    /// </summary>
    public partial class MainCoordinatorSystem : BaseSystem
    {
        private Main _main;
        private Node2D _enemies;
        private Node2D _items;
        private PlayerSpawnManager _playerSpawnManager;
        private ClawRPG.Scripts.Managers.EnemySpawnManager _enemySpawnManager;
        
        public PackedScene PlayerScene { get; set; }
        public PackedScene EnemyScene { get; set; }

        public void Initialize(Main main)
        {
            _main = main;
        }

        /// <summary>
        /// 创建节点结构
        /// </summary>
        public Node2D CreateNodeStructure()
        {
            _enemies = new Node2D { Name = "Enemies" };
            _main.AddChild(_enemies);

            _items = new Node2D { Name = "Items" };
            _main.AddChild(_items);
            
            return _enemies;
        }

        /// <summary>
        /// 初始化核心管理器
        /// </summary>
        public void InitializeCoreManagers()
        {
            // 游戏状态管理器
            var gameStateManager = new GameStateManager { Name = "GameStateManager" };
            _main.AddChild(gameStateManager);

            // 玩家生成管理器
            _playerSpawnManager = new PlayerSpawnManager { Name = "PlayerSpawnManager" };
            _playerSpawnManager.SetPlayerScene(PlayerScene);
            _main.AddChild(_playerSpawnManager);

            // 敌人生成管理器
            _enemySpawnManager = new ClawRPG.Scripts.Managers.EnemySpawnManager { Name = "EnemySpawnManager" };
            _enemySpawnManager.SetDefaultEnemyScene(EnemyScene);
            _main.AddChild(_enemySpawnManager);

            // 游戏初始化管理器
            var initializationManager = new GameInitializationManager { Name = "GameInitializationManager" };
            _main.AddChild(initializationManager);

            // 系统初始化管理器
            var systemInitializationManager = new SystemInitializationManager { Name = "SystemInitializationManager" };
            _main.AddChild(systemInitializationManager);

            // UI 管理器
            var uiManager = new UIManager { Name = "UIManager" };
            _main.AddChild(uiManager);

            GD.Print("Core managers initialized");
        }

        /// <summary>
        /// 初始化其他管理器
        /// </summary>
        public void InitializeAdditionalManagers()
        {
            // 场景管理器
            var sceneManager = new SceneManager { Name = "SceneManager" };
            _main.AddChild(sceneManager);

            // 存档管理器
            var saveLoadManager = new SaveLoadManager { Name = "SaveLoadManager" };
            _main.AddChild(saveLoadManager);

            // 事件总线管理器
            var eventBusManager = new EventBusManager { Name = "EventBusManager" };
            _main.AddChild(eventBusManager);

            // 玩家生命周期管理器
            var playerLifecycleManager = new PlayerLifecycleManager { Name = "PlayerLifecycleManager" };
            _main.AddChild(playerLifecycleManager);

            // 敌人生命周期管理器
            var enemyLifecycleManager = new EnemyLifecycleManager { Name = "EnemyLifecycleManager" };
            _main.AddChild(enemyLifecycleManager);

            // 战斗前Combo预览系统
            var combatPreloadComboSystem = new CombatPreloadComboSystem { Name = "CombatPreloadComboSystem" };
            _main.AddChild(combatPreloadComboSystem);

            GD.Print("Additional managers initialized");
        }

        /// <summary>
        /// 生成玩家
        /// </summary>
        public Player SpawnPlayer()
        {
            if (_playerSpawnManager != null)
            {
                return _playerSpawnManager.SpawnPlayer(null, true);
            }
            else if (PlayerScene != null)
            {
                var player = PlayerScene.Instantiate<Player>();
                player.AddToGroup("player");
                player.GlobalPosition = new Vector2(640, 360);
                _main.AddChild(player);
                return player;
            }
            
            GD.PrintErr("PlayerScene not set!");
            return null;
        }

        /// <summary>
        /// 获取 PlayerSpawnManager
        /// </summary>
        public PlayerSpawnManager GetPlayerSpawnManager() => _playerSpawnManager;

        /// <summary>
        /// 获取 EnemySpawnManager
        /// </summary>
        public ClawRPG.Scripts.Managers.EnemySpawnManager GetEnemySpawnManager() => _enemySpawnManager;

        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["PlayerScene"] = PlayerScene?.ResourcePath ?? "";
            data["EnemyScene"] = EnemyScene?.ResourcePath ?? "";
            return data;
        }

        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            base.ImportSaveData(data);
            
            if (data.Contains("PlayerScene"))
            {
                var path = data["PlayerScene"] as string;
                if (!string.IsNullOrEmpty(path))
                {
                    PlayerScene = GD.Load<PackedScene>(path);
                }
            }
            
            if (data.Contains("EnemyScene"))
            {
                var path = data["EnemyScene"] as string;
                if (!string.IsNullOrEmpty(path))
                {
                    EnemyScene = GD.Load<PackedScene>(path);
                }
            }
        }
    }
}
