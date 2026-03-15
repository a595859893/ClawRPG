using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// MainDebug - 调试管理模块
/// 处理游戏内的调试功能和日志输出
/// </summary>
public partial class MainDebug : Node
{
    // 调试模式开关
    public static bool DebugMode { get; private set; } = false;
    
    // 日志级别
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
    
    private static LogLevel _currentLogLevel = LogLevel.Info;
    public static LogLevel CurrentLogLevel
    {
        get => _currentLogLevel;
        set => _currentLogLevel = value;
    }
    
    // 日志缓存
    private const int MaxLogCache = 100;
    private static List<string> _logCache = new List<string>();
    
    public override void _Ready()
    {
#if DEBUG
        DebugMode = true;
        _currentLogLevel = LogLevel.Debug;
#endif
    }
    
    /// <summary>
    /// 调试模式打印
    /// </summary>
    public static void DebugPrint(object message)
    {
        if (DebugMode)
        {
            GD.Print("[DEBUG] " + message);
            AddToCache("DEBUG", message.ToString());
        }
    }
    
    /// <summary>
    /// 普通信息打印
    /// </summary>
    public static void InfoPrint(object message)
    {
        if (_currentLogLevel <= LogLevel.Info)
        {
            GD.Print("[INFO] " + message);
            AddToCache("INFO", message.ToString());
        }
    }
    
    /// <summary>
    /// 警告打印
    /// </summary>
    public static void WarningPrint(object message)
    {
        if (_currentLogLevel <= LogLevel.Warning)
        {
            GD.PrintWarn("[WARN] " + message);
            AddToCache("WARN", message.ToString());
        }
    }
    
    /// <summary>
    /// 错误打印
    /// </summary>
    public static void ErrorPrint(object message)
    {
        if (_currentLogLevel <= LogLevel.Error)
        {
            GD.PrintErr("[ERROR] " + message);
            AddToCache("ERROR", message.ToString());
        }
    }
    
    /// <summary>
    /// 条件打印（仅在调试模式输出）
    /// </summary>
    public static void PrintIf(bool condition, object message)
    {
        if (condition)
        {
            DebugPrint(message);
        }
    }
    
    /// <summary>
    /// 添加日志到缓存
    /// </summary>
    private static void AddToCache(string level, string message)
    {
        string logEntry = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
        _logCache.Add(logEntry);
        
        if (_logCache.Count > MaxLogCache)
        {
            _logCache.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// 获取日志缓存
    /// </summary>
    public static List<string> GetLogCache()
    {
        return new List<string>(_logCache);
    }
    
    /// <summary>
    /// 清空日志缓存
    /// </summary>
    public static void ClearLogCache()
    {
        _logCache.Clear();
    }
    
    /// <summary>
    /// 设置调试模式
    /// </summary>
    public static void SetDebugMode(bool enabled)
    {
        DebugMode = enabled;
        if (enabled)
        {
            GD.Print("Debug mode enabled");
        }
    }
    
    /// <summary>
    /// 打印性能信息
    /// </summary>
    public static void PrintPerformanceInfo()
    {
        if (!DebugMode) return;
        
        GD.Print("=== Performance Info ===");
        GD.Print($"FPS: {Engine.GetFramesPerSecond()}");
        GD.Print($"Draw Calls: {RenderingServer.Server.GetRenderInfo(RenderingServer.RenderInfoObject.Active));
    }
    
    /// <summary>
    /// 打印节点树信息
    /// </summary>
    public static void PrintNodeTree(Node node, int depth = 0)
    {
        if (!DebugMode) return;
        
        string indent = new string(' ', depth * 2);
        GD.Print($"{indent}{node.Name} ({node.GetType().Name})");
        
        foreach (Node child in node.GetChildren())
        {
            PrintNodeTree(child, depth + 1);
        }
    }
}
