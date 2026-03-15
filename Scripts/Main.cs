using Godot;
using System;
using ClawRPG.Scripts.Managers;
using ClawRPG.Scripts.Systems;
using ClawRPG.Scripts.UI;
using ClawRPG.Scripts.Events;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// Main game manager - 游戏的入口和协调器
    /// 负责管理器的初始化和协调，不直接处理具体业务逻辑
    /// </summary>
    public partial class Main : Node2D
    {
        [Export] public PackedScene PlayerScene;
        [Export] public PackedScene EnemyScene;

        private Player _player;
        private Node2D _enemies;
        private Node2D _items;

        // Game state
        public enum GameState
        {
            TitleScreen,
            Playing,
            Paused,
            GameOver
        }

        // 核心管理器
        private GameStateManager _gameStateManager;
        private PlayerSpawnManager _playerSpawnManager;
        private EnemySpawnManager _enemySpawnManager;
        private GameInitializationManager _initializationManager;
        private SystemInitializationManager _systemInitializationManager;
        private UIManager _uiManager;

        // 模块化组件
        private MainInput _mainInput;
        private MainUI _mainUI;
        private MainGame _mainGame;
        private MainMenu _mainMenu;
        private MainLobby _mainLobby;
        private MainNetwork _mainNetwork;

        // 向后兼容
        public static bool IsPaused => GameStateManager.IsPaused;
        public static int CurrentDay => GameStateManager.Instance?.GetCurrentDay() ?? 1;

        public void SetGameState(GameState state)
        {
            _gameStateManager?.SetState(state);
        }

        public GameState GetGameState() => _gameStateManager?.GetState() ?? GameState.Playing;

        public Player GetPlayer() => _playerSpawnManager?.GetPlayer() ?? _player;

        public override void _Ready()
        {
            GD.Print("=== ClawRPG Starting ===");

            // 创建节点结构
            CreateNodeStructure();

            // 初始化核心管理器
            InitializeCoreManagers();

            // 初始化模块组件
            InitializeModules();

            // 初始化系统（通过 SystemInitializationManager）
            InitializeSystems();

            // 初始化 UI（通过 UIManager）
            InitializeUI();

            // 连接信号
            ConnectSignals();

            // 生成玩家
            SpawnPlayer();

            // 加载游戏数据
            LoadGameData();

            GD.Print("Game initialized successfully!");
        }

        /// <summary>
        /// 创建节点结构
        /// </summary>
        private void CreateNodeStructure()
        {
            _enemies = new Node2D { Name = "Enemies" };
            AddChild(_enemies);

            _items = new Node2D { Name = "Items" };
            AddChild(_items);
        }

        /// <summary>
        /// 初始化核心管理器
        /// </summary>
        private void InitializeCoreManagers()
        {
            // 游戏状态管理器
            _gameStateManager = new GameStateManager { Name = "GameStateManager" };
            AddChild(_gameStateManager);

            // 玩家生成管理器
            _playerSpawnManager = new PlayerSpawnManager { Name = "PlayerSpawnManager" };
            _playerSpawnManager.SetPlayerScene(PlayerScene);
            AddChild(_playerSpawnManager);

            // 敌人生成管理器
            _enemySpawnManager = new EnemySpawnManager { Name = "EnemySpawnManager" };
            _enemySpawnManager.SetDefaultEnemyScene(EnemyScene);
            AddChild(_enemySpawnManager);

            // 游戏初始化管理器
            _initializationManager = new GameInitializationManager { Name = "GameInitializationManager" };
            AddChild(_initializationManager);

            // 系统初始化管理器
            _systemInitializationManager = new SystemInitializationManager { Name = "SystemInitializationManager" };
            AddChild(_systemInitializationManager);

            // UI 管理器
            _uiManager = new UIManager { Name = "UIManager" };
            AddChild(_uiManager);

            // 其他管理器
            InitializeAdditionalManagers();

            GD.Print("Core managers initialized");
        }

        /// <summary>
        /// 初始化其他管理器
        /// </summary>
        private void InitializeAdditionalManagers()
        {
            // 场景管理器
            var sceneManager = new SceneManager { Name = "SceneManager" };
            AddChild(sceneManager);

            // 存档管理器
            var saveLoadManager = new SaveLoadManager { Name = "SaveLoadManager" };
            AddChild(saveLoadManager);

            // 事件总线管理器
            var eventBusManager = new EventBusManager { Name = "EventBusManager" };
            AddChild(eventBusManager);

            // 玩家生命周期管理器
            var playerLifecycleManager = new PlayerLifecycleManager { Name = "PlayerLifecycleManager" };
            AddChild(playerLifecycleManager);

            // 敌人生命周期管理器
            var enemyLifecycleManager = new EnemyLifecycleManager { Name = "EnemyLifecycleManager" };
            AddChild(enemyLifecycleManager);

            GD.Print("Additional managers initialized");
        }

        /// <summary>
        /// 初始化模块组件
        /// </summary>
        private void InitializeModules()
        {
            // 输入处理
            _mainInput = new MainInput { Name = "MainInput" };
            _mainInput.Initialize(this);
            AddChild(_mainInput);

            // UI 控制
            _mainUI = new MainUI { Name = "MainUI" };
            _mainUI.Initialize(this);
            AddChild(_mainUI);

            // 游戏循环
            _mainGame = new MainGame { Name = "MainGame" };
            _mainGame.Initialize(this);
            _mainGame.SetPlayerSpawnManager(_playerSpawnManager);
            _mainGame.SetEnemySpawnManager(_enemySpawnManager);
            AddChild(_mainGame);

            // 主菜单
            _mainMenu = new MainMenu { Name = "MainMenu" };
            _mainMenu.Initialize(this);
            AddChild(_mainMenu);

            // 大厅
            _mainLobby = new MainLobby { Name = "MainLobby" };
            _mainLobby.Initialize(this);
            AddChild(_mainLobby);

            // 网络
            _mainNetwork = new MainNetwork { Name = "MainNetwork" };
            AddChild(_mainNetwork);
            _mainNetwork.InitializeNetwork();

            GD.Print("Modular components initialized");
        }

        /// <summary>
        /// 初始化系统（使用 SystemInitializationManager）
        /// </summary>
        private void InitializeSystems()
        {
            // 系统初始化由 SystemInitializationManager 处理
            // 这里可以添加一些特殊的系统初始化
            
            // 初始化 LootDropSystem
            LootDropSystem.Instance.Initialize();

            GD.Print("Systems initialized via SystemInitializationManager");
        }

        /// <summary>
        /// 初始化 UI（使用 UIManager）
        /// </summary>
        private void InitializeUI()
        {
            // UI 初始化由 UIManager 处理
            GD.Print("UI initialized via UIManager");
        }

        /// <summary>
        /// 连接信号
        /// </summary>
        private void ConnectSignals()
        {
            // 成就解锁声音
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementUnlocked += achievement =>
                {
                    SoundEffectSystem.Instance?.PlayAchievementUnlock();
                };
            }

            // 称号解锁声音
            if (TitleSystem.Instance != null)
            {
                TitleSystem.Instance.OnTitleUnlocked += title =>
                {
                    SoundEffectSystem.Instance?.PlayTitleUnlock();
                };
            }

            // 任务完成声音
            QuestSystem.OnQuestCompleted += quest =>
            {
                SoundEffectSystem.Instance?.PlayQuestComplete();
            };

            // 通过 EventBus 订阅游戏事件（事件驱动架构）
            ConnectEventBusSignals();

            GD.Print("Signals connected");
        }
        
        /// <summary>
        /// 连接事件总线信号
        /// </summary>
        private void ConnectEventBusSignals()
        {
            if (EventBusManager.Instance == null) return;

            // 玩家死亡事件
            EventBusManager.Instance.Subscribe<PlayerDiedEventData>(EventBusManager.Events.PlayerDied, OnPlayerDied);
            
            // 敌人击杀事件
            EventBusManager.Instance.Subscribe<EnemyDiedEventData>(EventBusManager.Events.EnemyDied, OnEnemyDied);
            
            // 场景切换事件
            EventBusManager.Instance.Subscribe<string>(EventBusManager.Events.SceneChanged, OnSceneChanged);
            
            // 游戏暂停/恢复事件
            EventBusManager.Instance.Subscribe<GamePauseEventData>(EventBusManager.Events.GamePaused, OnGamePaused);
            EventBusManager.Instance.Subscribe<GamePauseEventData>(EventBusManager.Events.GameResumed, OnGameResumed);
            
            // 游戏结束事件
            EventBusManager.Instance.Subscribe<GameOverEventData>(EventBusManager.Events.GameOver, OnGameOver);
            
            GD.Print("[Main] EventBus signals connected");
        }
        
        /// <summary>
        /// 处理玩家死亡事件
        /// </summary>
        private void OnPlayerDied(PlayerDiedEventData data)
        {
            GD.Print($"[Main] Player died! Death count: {data.DeathCount}");
            // 可以在这里添加玩家死亡后的全局逻辑，如：
            // - 显示死亡界面
            // - 更新统计
            // - 触发成就等
        }
        
        /// <summary>
        /// 处理敌人击杀事件
        /// </summary>
        private void OnEnemyDied(EnemyDiedEventData data)
        {
            GD.Print($"[Main] Enemy killed! Total kills: {data.KillCount}");
            // 可以在这里添加敌人死亡后的全局逻辑，如：
            // - 更新击杀统计
            // - 检查成就
            // - 掉落物品处理等
        }
        
        /// <summary>
        /// 处理场景切换事件
        /// </summary>
        private void OnSceneChanged(string scenePath)
        {
            GD.Print($"[Main] Scene changed to: {scenePath}");
            // 可以在这里添加场景切换后的全局逻辑
        }
        
        /// <summary>
        /// 处理游戏暂停事件
        /// </summary>
        private void OnGamePaused(GamePauseEventData data)
        {
            GD.Print($"[Main] Game paused at playtime: {data.PlayTime}");
        }
        
        /// <summary>
        /// 处理游戏恢复事件
        /// </summary>
        private void OnGameResumed(GamePauseEventData data)
        {
            GD.Print("[Main] Game resumed");
        }
        
        /// <summary>
        /// 处理游戏结束事件
        /// </summary>
        private void OnGameOver(GameOverEventData data)
        {
            GD.Print($"[Main] Game Over! Play time: {data.TotalPlayTime}s, Kills: {data.KillCount}, Deaths: {data.DeathCount}");
            // 可以在这里添加游戏结束后的全局逻辑
        }

        /// <summary>
        /// 生成玩家
        /// </summary>
        private void SpawnPlayer()
        {
            if (_playerSpawnManager != null)
            {
                _player = _playerSpawnManager.SpawnPlayer(null, true);
            }
            else if (PlayerScene != null)
            {
                _player = PlayerScene.Instantiate<Player>();
                _player.AddToGroup("player");
                _player.GlobalPosition = new Vector2(640, 360);
                AddChild(_player);
            }
            else
            {
                GD.PrintErr("PlayerScene not set!");
            }

            GD.Print("Player spawned");
        }

        /// <summary>
        /// 加载游戏数据
        /// </summary>
        private void LoadGameData()
        {
            var mainSaveLoad = GetNodeOrNull<MainSaveLoad>("MainSaveLoad");
            mainSaveLoad?.LoadGameData();
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta;

            // 处理模块组件
            _mainInput?.ProcessInput(dt);
            _mainGame?.ProcessGame(delta);
            _mainGame?.UpdatePlayerUI();
            _mainNetwork?.ProcessNetwork(delta);

            // 其他游戏逻辑已委托给各 Manager 和 MainGame 处理
            // - 自动保存: SaveLoadManager.ManagerUpdate()
            // - 统计更新: MainGame.ProcessGame()
            // - 药水效果: MainGame.ProcessGame()
        }

        /// <summary>
        /// 导出所有游戏数据（供存档使用）
        /// </summary>
        public Dictionary ExportAllData()
        {
            var allData = new Dictionary();

            // 从各管理器收集数据
            if (_gameStateManager != null)
            {
                allData["gameState"] = _gameStateManager.ExportSaveData();
            }

            if (_systemInitializationManager != null)
            {
                allData["systemInit"] = _systemInitializationManager.ExportSaveData();
            }

            if (_uiManager != null)
            {
                allData["ui"] = _uiManager.ExportSaveData();
            }

            return allData;
        }

        /// <summary>
        /// 导入所有游戏数据（供读档使用）
        /// </summary>
        public void ImportAllData(Dictionary data)
        {
            if (data == null) return;

            if (data.Contains("gameState"))
            {
                _gameStateManager?.ImportSaveData(data["gameState"] as Dictionary);
            }

            if (data.Contains("systemInit"))
            {
                _systemInitializationManager?.ImportSaveData(data["systemInit"] as Dictionary);
            }

            if (data.Contains("ui"))
            {
                _uiManager?.ImportSaveData(data["ui"] as Dictionary);
            }
        }
    }
}
