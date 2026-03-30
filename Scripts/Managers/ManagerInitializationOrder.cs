using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Managers
{
    /// <summary>
    /// 管理器初始化顺序管理器 - 负责管理所有 Manager 的初始化顺序
    /// </summary>
    public class ManagerInitializationOrder : ManagerBase
    {
        public static ManagerInitializationOrder Instance { get; private set; }
        
        /// <summary>
        /// 优先级（数值越小越先初始化）
        /// </summary>
        public override int Priority => 1;
        
        /// <summary>
        /// 已初始化的管理器列表
        /// </summary>
        private List<ManagerBase> _initializedManagers = new List<ManagerBase>();
        
        /// <summary>
        /// 待初始化的管理器列表
        /// </summary>
        private List<ManagerBase> _pendingManagers = new List<ManagerBase>();
        
        /// <summary>
        /// 初始化顺序定义
        /// </summary>
        private readonly Dictionary<Type, int> _initializationOrder = new Dictionary<Type, int>
        {
            { typeof(EventBusManager), 1 },
            { typeof(GameStateManager), 2 },
            { typeof(SaveLoadManager), 3 },
            { typeof(SceneManager), 4 },
            { typeof(PlayerSpawnManager), 5 },
            { typeof(EnemySpawnManager), 6 },
            { typeof(PlayerLifecycleManager), 7 },
            { typeof(EnemyLifecycleManager), 8 },
            { typeof(GameInitializationManager), 9 },
            { typeof(SystemInitializationManager), 10 },
            { typeof(UIManager), 50 }
        };
        
        public override void _Ready()
        {
            Instance = this;
            base._Ready();
        }
        
        protected override void Initialize()
        {
            GD.Print("[ManagerInitializationOrder] Starting manager initialization...");
            
            // 按优先级排序并初始化所有 Manager
            InitializeAllManagers();
            
            GD.Print($"[ManagerInitializationOrder] All managers initialized: {_initializedManagers.Count}");
            NotifyInitialized();
        }
        
        /// <summary>
        /// 初始化所有管理器
        /// </summary>
        private void InitializeAllManagers()
        {
            var main = GetParent<Main>();
            if (main == null) return;
            
            // 获取所有 ManagerBase 子类
            var managers = new List<ManagerBase>();
            foreach (Node child in main.GetChildren())
            {
                if (child is ManagerBase manager)
                {
                    managers.Add(manager);
                }
            }
            
            // 按优先级排序
            managers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            
            // 初始化每个 Manager
            foreach (var manager in managers)
            {
                if (!manager.IsReady)
                {
                    GD.Print($"[ManagerInitializationOrder] Initializing: {manager.GetType().Name}");
                    _initializedManagers.Add(manager);
                }
            }
        }
        
        /// <summary>
        /// 获取管理器初始化优先级
        /// </summary>
        public int GetManagerPriority(Type managerType)
        {
            if (_initializationOrder.TryGetValue(managerType, out int priority))
            {
                return priority;
            }
            return 100; // 默认优先级
        }
        
        /// <summary>
        /// 获取已初始化的管理器
        /// </summary>
        public List<ManagerBase> GetInitializedManagers()
        {
            return new List<ManagerBase>(_initializedManagers);
        }
        
        /// <summary>
        /// 获取特定类型的已初始化管理器
        /// </summary>
        public T GetManager<T>() where T : ManagerBase
        {
            foreach (var manager in _initializedManagers)
            {
                if (manager is T typedManager)
                {
                    return typedManager;
                }
            }
            return null;
        }
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary
            {
                { "initializedCount", _initializedManagers.Count }
            };
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            // Manager 状态通常不需要持久化
        }
    }
}
