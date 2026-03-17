using System;
using System.Threading;

/// <summary>
/// 通用单例基类 - 提供线程安全的单例模式
/// 支持两种模式:
/// 1. 延迟初始化 (Lazy Singleton): 适用于普通类
/// 2. 节点单例 (Node Singleton): 适用于继承自 Node 的类
/// 
/// 使用方式:
/// - 普通类: public class MyClass : Singleton<MyClass> { }
/// - Node 类: public class MySystem : SingletonNode<MySystem> { }
/// </summary>
public abstract class Singleton<T> where T : class
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _isInitialized = false;
    
    /// <summary>
    /// 获取单例实例 (线程安全, 延迟初始化)
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        throw new InvalidOperationException(
                            $"Singleton instance of {typeof(T).Name} is not initialized. " +
                            $"Either call SetInstance() explicitly or inherit from SingletonNode for Node-based singletons.");
                    }
                }
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// 检查实例是否已初始化
    /// </summary>
    public static bool IsInitialized => _instance != null;
    
    /// <summary>
    /// 设置单例实例 (用于 Node 类型的单例)
    /// </summary>
    /// <param name="instance">实例</param>
    /// <param name="force">是否强制覆盖现有实例</param>
    protected static void SetInstance(T instance, bool force = false)
    {
        lock (_lock)
        {
            if (_instance != null && !force)
            {
                throw new InvalidOperationException(
                    $"Singleton instance of {typeof(T).Name} is already set. Use force=true to override.");
            }
            _instance = instance;
            _isInitialized = true;
        }
    }
    
    /// <summary>
    /// 清除单例实例 (主要用于测试)
    /// </summary>
    protected static void ClearInstance()
    {
        lock (_lock)
        {
            _instance = null;
            _isInitialized = false;
        }
    }
}

/// <summary>
/// Node 单例基类 - 适用于继承自 Godot Node 的类
/// 在 _Ready() 中自动设置单例实例
/// 
/// 使用方式:
/// public class MySystem : SingletonNode<MySystem>
/// {
///     public override void _Ready()
///     {
///         base._Ready();
///         // 初始化代码
///     }
/// }
/// </summary>
public abstract class SingletonNode<T> : Godot.Node where T : Godot.Node
{
    private static T _instance;
    private static readonly object _lock = new object();
    
    /// <summary>
    /// 获取单例实例
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GD.PrintErr($"[SingletonNode] {typeof(T).Name} instance is null. Make sure the node is in the scene tree.");
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// 检查实例是否已初始化
    /// </summary>
    public static bool IsInitialized => _instance != null;
    
    public override void _Ready()
    {
        base._Ready();
        
        lock (_lock)
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr($"[SingletonNode] {GetType().Name} instance already exists! Duplicate will be freed.");
                QueueFree();
                return;
            }
            
            _instance = (T)this;
            GD.Print($"[SingletonNode] {GetType().Name} initialized as singleton");
        }
    }
    
    /// <summary>
    /// 手动设置实例 (用于特殊情况)
    /// </summary>
    protected static void SetInstance(T instance)
    {
        lock (_lock)
        {
            _instance = instance;
        }
    }
}

/// <summary>
/// UI 单例基类 - 适用于继承自 Godot Control 的 UI 类
/// </summary>
public abstract class SingletonUI<T> : Godot.Control where T : Godot.Control
{
    private static T _instance;
    private static readonly object _lock = new object();
    
    /// <summary>
    /// 获取单例实例
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GD.PrintErr($"[SingletonUI] {typeof(T).Name} instance is null. Make sure the UI node is in the scene tree.");
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// 检查实例是否已初始化
    /// </summary>
    public static bool IsInitialized => _instance != null;
    
    public override void _Ready()
    {
        base._Ready();
        
        lock (_lock)
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr($"[SingletonUI] {GetType().Name} instance already exists! Duplicate will be freed.");
                QueueFree();
                return;
            }
            
            _instance = (T)this;
            GD.Print($"[SingletonUI] {GetType().Name} initialized as singleton");
        }
    }
}
