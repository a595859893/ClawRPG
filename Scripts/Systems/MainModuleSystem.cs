using Godot;
using System;
using ClawRPG.Scripts.Managers;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// MainModuleSystem - 负责 Main 的模块组件初始化
    /// </summary>
    public partial class MainModuleSystem : BaseSystem
    {
        private Main _main;
        
        // 模块化组件
        private MainInput _mainInput;
        private MainUI _mainUI;
        private MainGame _mainGame;
        private MainMenu _mainMenu;
        private MainLobby _mainLobby;
        private MainNetwork _mainNetwork;

        public void Initialize(Main main)
        {
            _main = main;
        }

        /// <summary>
        /// 初始化模块组件
        /// </summary>
        public void InitializeModules(PlayerSpawnManager playerSpawnManager, EnemySpawnManager enemySpawnManager)
        {
            // 输入处理
            _mainInput = new MainInput { Name = "MainInput" };
            _mainInput.Initialize(_main);
            _main.AddChild(_mainInput);

            // UI 控制
            _mainUI = new MainUI { Name = "MainUI" };
            _mainUI.Initialize(_main);
            _main.AddChild(_mainUI);

            // 游戏循环
            _mainGame = new MainGame { Name = "MainGame" };
            _mainGame.Initialize(_main);
            _mainGame.SetPlayerSpawnManager(playerSpawnManager);
            _mainGame.SetEnemySpawnManager(enemySpawnManager);
            _main.AddChild(_mainGame);

            // 主菜单
            _mainMenu = new MainMenu { Name = "MainMenu" };
            _mainMenu.Initialize(_main);
            _main.AddChild(_mainMenu);

            // 大厅
            _mainLobby = new MainLobby { Name = "MainLobby" };
            _mainLobby.Initialize(_main);
            _main.AddChild(_mainLobby);

            // 网络
            _mainNetwork = new MainNetwork { Name = "MainNetwork" };
            _main.AddChild(_mainNetwork);
            _mainNetwork.InitializeNetwork();

            GD.Print("Modular components initialized");
        }

        /// <summary>
        /// 处理输入
        /// </summary>
        public void ProcessInput(float delta)
        {
            _mainInput?.ProcessInput(delta);
        }

        /// <summary>
        /// 处理游戏逻辑
        /// </summary>
        public void ProcessGame(double delta)
        {
            _mainGame?.ProcessGame(delta);
        }

        /// <summary>
        /// 更新玩家UI
        /// </summary>
        public void UpdatePlayerUI()
        {
            _mainGame?.UpdatePlayerUI();
        }

        /// <summary>
        /// 处理网络
        /// </summary>
        public void ProcessNetwork(double delta)
        {
            _mainNetwork?.ProcessNetwork(delta);
        }

        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            // MainModuleSystem 主要负责模块组件初始化，无持久化状态
            return data;
        }

        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            base.ImportSaveData(data);
            // MainModuleSystem 主要负责模块组件初始化，无持久化状态
        }
    }
}
