namespace ClawRPG.Scripts.Framework
{

using Godot;
using System.Collections;
using System.Linq;

/// <summary>
/// 游戏主管理器 - 统一管理所有系统
/// 负责系统的初始化、更新和数据持久化
/// </summary>
public class GameManager : BaseSystem
{
    /// <summary>
    /// 单例实例
    /// </summary>
    public static GameManager Instance { get; private set; }
    
    /// <summary>
    /// 所有注册的系统
    /// </summary>
    private readonly ArrayList _systems = new ArrayList();
    
    /// <summary>
    /// 所有注册的UI
    /// </summary>
    private readonly Dictionary<string, Control> _uis = new Dictionary<string, Control>();
    
    public override void _Ready()
    {
        base._Ready();
        
        if (Instance != null && Instance != this)
        {
            GD.PrintErr("[GameManager] Instance already exists!");
            QueueFree();
            return;
        }
        
        Instance = this;
        GD.Print("[GameManager] Initialized");
    }
    
    /// <summary>
    /// 注册系统
    /// </summary>
    public void RegisterSystem(BaseSystem system)
    {
        if (system == null) return;
        
        var systemName = system.SystemName;
        if (!_systems.Contains(system))
        {
            _systems.Add(system);
            GD.Print($"[GameManager] Registered system: {systemName}");
        }
    }
    
    /// <summary>
    /// 注销系统
    /// </summary>
    public void UnregisterSystem(BaseSystem system)
    {
        if (system == null) return;
        
        _systems.Remove(system);
        GD.Print($"[GameManager] Unregistered system: {system.SystemName}");
    }
    
    /// <summary>
    /// 注册UI
    /// </summary>
    public void RegisterUI(string name, Control ui)
    {
        if (ui == null || string.IsNullOrEmpty(name)) return;
        
        _uis[name] = ui;
        GD.Print($"[GameManager] Registered UI: {name}");
    }
    
    /// <summary>
    /// 获取UI
    /// </summary>
    public T GetUI<T>(string name) where T : Control
    {
        if (_uis.TryGetValue(name, out var ui))
        {
            return ui as T;
        }
        return null;
    }
    
    /// <summary>
    /// 显示UI
    /// </summary>
    public void ShowUI(string name)
    {
        if (_uis.TryGetValue(name, out var ui) && ui is BaseUI baseUi)
        {
            baseUi.Show();
        }
    }
    
    /// <summary>
    /// 隐藏UI
    /// </summary>
    public void HideUI(string name)
    {
        if (_uis.TryGetValue(name, out var ui) && ui is BaseUI baseUi)
        {
            baseUi.Hide();
        }
    }
    
    /// <summary>
    /// 导出所有系统数据
    /// </summary>
    public Dictionary ExportAllData()
    {
        var allData = new Dictionary();
        
        foreach (BaseSystem system in _systems)
        {
            var systemName = system.SystemName;
            try
            {
                allData[systemName] = system.ExportSaveData();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[GameManager] Failed to export {systemName}: {ex.Message}");
            }
        }
        
        return allData;
    }
    
    /// <summary>
    /// 导入所有系统数据
    /// </summary>
    public void ImportAllData(Dictionary data)
    {
        if (data == null) return;
        
        foreach (BaseSystem system in _systems)
        {
            var systemName = system.SystemName;
            if (data.Contains(systemName))
            {
                try
                {
                    system.ImportSaveData((Dictionary)data[systemName]);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameManager] Failed to import {systemName}: {ex.Message}");
                }
            }
        }
    }
    
    /// <summary>
    /// 重置所有系统
    /// </summary>
    public void ResetAll()
    {
        foreach (BaseSystem system in _systems)
        {
            system.Reset();
        }

        _uis.Clear();
        GD.Print("[GameManager] All systems reset");
    }

    public override Dictionary ExportSaveData() => new();
    public override void ImportSaveData(Dictionary data) { }
}

} // namespace ClawRPG.Scripts.Framework
