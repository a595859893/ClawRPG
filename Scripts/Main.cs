using Godot;
using System;
using ClawRPG.Scripts.Managers;
using ClawRPG.Scripts.Systems;
using ClawRPG.Scripts.UI;

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

        // 核心协调系统
        private MainCoordinatorSystem _coordinator;
        private MainModuleSystem _modules;
        private MainEventSystem _events;
        private MainSaveLoadSystem _saveLoad;

        // 向后兼容
        public static bool IsPaused => GameStateManager.IsPaused;
        public static int CurrentDay => GameStateManager.Instance?.GetCurrentDay() ?? 1;

        public void SetGameState(Main.GameState state)
        {
            var gsm = GetNodeOrNull<GameStateManager>("GameStateManager");
            if (gsm == null)
            {
                GD.PrintErr("SetGameState: GameStateManager node not found!");
                return;
            }
            gsm?.SetState(state);
        }

        public Main.GameState GetGameState()
        {
            var gsm = GetNodeOrNull<GameStateManager>("GameStateManager");
            if (gsm == null)
            {
                GD.PrintErr("GetGameState: GameStateManager node not found!");
                return Main.GameState.Playing;
            }
            return gsm?.GetState() ?? Main.GameState.Playing;
        }

        public Player GetPlayer()
        {
            var psm = GetNodeOrNull<PlayerSpawnManager>("PlayerSpawnManager");
            if (psm == null)
            {
                GD.PrintErr("GetPlayer: PlayerSpawnManager node not found!");
                return _player;
            }
            return psm?.GetPlayer() ?? _player;
        }

        public override void _Ready()
        {
            try
            {
                GD.Print("=== ClawRPG Starting ===");

                // 初始化协调系统
                InitializeSystems();

                // 生成玩家
                SpawnPlayer();

                // 加载游戏数据
                LoadGameData();

                GD.Print("Game initialized successfully!");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"_Ready: Exception during initialization: {ex.Message}");
                GD.PrintErr($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 初始化所有系统
        /// </summary>
        private void InitializeSystems()
        {
            // 1. 创建节点结构
            _coordinator = new MainCoordinatorSystem { Name = "MainCoordinatorSystem" };
            _coordinator.PlayerScene = PlayerScene;
            _coordinator.EnemyScene = EnemyScene;
            _coordinator.Initialize(this);
            AddChild(_coordinator);
            _coordinator.CreateNodeStructure();

            // 2. 初始化核心管理器
            _coordinator.InitializeCoreManagers();

            // 3. 初始化其他管理器
            _coordinator.InitializeAdditionalManagers();

            // 4. 初始化模块组件
            _modules = new MainModuleSystem { Name = "MainModuleSystem" };
            _modules.Initialize(this);
            AddChild(_modules);
            _modules.InitializeModules(_coordinator.GetPlayerSpawnManager(), _coordinator.GetEnemySpawnManager());

            // 5. 初始化系统
            InitializeGameSystems();

            // 6. 初始化 UI
            InitializeUI();

            // 7. 连接事件
            _events = new MainEventSystem { Name = "MainEventSystem" };
            _events.Initialize(this);
            AddChild(_events);
            _events.ConnectSignals();

            // 8. 初始化存档系统
            _saveLoad = new MainSaveLoadSystem { Name = "MainSaveLoadSystem" };
            _saveLoad.Initialize(this);
            AddChild(_saveLoad);

            var gsm = GetNode<GameStateManager>("GameStateManager");
            var sim = GetNode<SystemInitializationManager>("SystemInitializationManager");
            var uim = GetNode<UIManager>("UIManager");
            _saveLoad.SetManagers(gsm, sim, uim);
        }

        /// <summary>
        /// 初始化游戏系统
        /// </summary>
        private void InitializeGameSystems()
        {
            LootDropSystem.Instance.Initialize();
            GD.Print("Systems initialized");
        }

        /// <summary>
        /// 初始化 UI
        /// </summary>
        private void InitializeUI()
        {
            GD.Print("UI initialized via UIManager");
        }

        /// <summary>
        /// 生成玩家
        /// </summary>
        private void SpawnPlayer()
        {
            _player = _coordinator.SpawnPlayer();
            GD.Print("Player spawned");
        }

        /// <summary>
        /// 加载游戏数据
        /// </summary>
        private void LoadGameData()
        {
            _saveLoad.LoadGameData();
        }

        public override void _Process(double delta)
        {
            try
            {
                float dt = (float)delta;

                // 处理模块组件
                _modules?.ProcessInput(dt);
                _modules?.ProcessGame(delta);
                _modules?.UpdatePlayerUI();
                _modules?.ProcessNetwork(delta);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"_Process: Exception during process: {ex.Message}");
                GD.PrintErr($"Stack trace: {ex.StackTrace}");
            }
        }

        public override void _Input(InputEvent @event)
        {
            try
            {
                // Pass input to modules for handling
                _modules?.ProcessInputEvent(@event);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"_Input: Exception during input handling: {ex.Message}");
                GD.PrintErr($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 导出所有游戏数据
        /// </summary>
        public Dictionary ExportAllData()
        {
            return _saveLoad?.ExportAllData() ?? new Dictionary();
        }

        /// <summary>
        /// 导入所有游戏数据
        /// </summary>
        public void ImportAllData(Dictionary data)
        {
            _saveLoad?.ImportAllData(data);
        }
    }
}
