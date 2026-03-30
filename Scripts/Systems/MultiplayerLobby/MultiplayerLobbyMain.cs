using Godot;
using System;

namespace ClawRPG.Systems
{
    /// <summary>
    /// Multiplayer Lobby Main Entry
    /// 多人游戏大厅系统主入口
    /// 
    /// 功能:
    /// - 房间创建/加入/管理
    /// - 游戏模式选择 (Co-op Dungeon, PvP Battle, Racing, Boss Rush, Treasure Hunt, Survival)
    /// - 难度选择 (Easy/Normal/Hard/Nightmare/Legendary)
    /// - 私人房间密码保护
    /// - 玩家准备状态
    /// - 邀请系统
    /// - 游戏统计追踪
    /// 
    /// 快捷键: Ctrl+Shift+L
    /// </summary>
    public class MultiplayerLobbyMain : BaseSystem
    {
        public static MultiplayerLobbyMain Instance { get; private set; }
        
        private MultiplayerLobbyData _data;
        private MultiplayerLobbyDatabase _database;
        
        // REQ-058-11: Migrated from Godot 3 .Connect() to C# event
        public event Action OnDatabaseReadyUI;
        private MultiplayerLobbySystem _system;
        private MultiplayerLobbyUI _ui;
        
        public override void _Ready()
        {
            Instance = this;
            Name = "MultiplayerLobby";
            
            // 初始化数据节点
            _data = new MultiplayerLobbyData();
            AddChild(_data);
            
            // 初始化数据库
            _database = new MultiplayerLobbyDatabase();
            AddChild(_database);
            
            // 等待数据库初始化完成 (REQ-058-11: migrated from Godot 3 .Connect() to C# event +=)
            _database.Ready += OnDatabaseReady;
        }
        
        private void OnDatabaseReady()
        {
            // REQ-058-11: Invoke new event
            OnDatabaseReadyUI?.Invoke();
            // 初始化系统
            _system = new MultiplayerLobbySystem();
            AddChild(_system);
            
            // 初始化UI
            _ui = new MultiplayerLobbyUI();
            AddChild(_ui);
            
            GD.Print("MultiplayerLobby System initialized");
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // Ctrl+Shift+L 切换UI
                if (keyEvent.Control && keyEvent.Shift && keyEvent.Scancode == KeyList.L)
                {
                    ToggleUI();
                }
            }
        }
        
        public void ToggleUI()
        {
            if (_ui != null)
            {
                _ui.Toggle();
            }
        }
        
        public static void Show()
        {
            if (Instance != null && Instance._ui != null)
            {
                Instance._ui.Visible = true;
            }
        }
        
        public static void Hide()
        {
            if (Instance != null && Instance._ui != null)
            {
                Instance._ui.Visible = false;
            }
        }
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            // MultiplayerLobbyMain 是容器系统，无持久化状态
            return data;
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            // MultiplayerLobbyMain 是容器系统，无持久化状态
        }
    }
}
