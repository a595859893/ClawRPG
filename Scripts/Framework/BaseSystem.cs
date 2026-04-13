namespace ClawRPG.Scripts.Framework
{

using Godot;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 所有游戏系统的基类
/// 提供统一的生命周期管理和数据持久化接口
/// 支持单例模式 (通过 Instance 属性访问)
/// </summary>
public abstract partial class BaseSystem : Node
{
    /// <summary>
    /// 单例实例 (子类需要实现)
    /// </summary>
    public static BaseSystem Instance { get; protected set; }
    
    /// <summary>
    /// 系统是否已初始化
    /// </summary>
    public bool IsInitialized { get; protected set; } = false;
    
    /// <summary>
    /// 系统名称（用于日志）
    /// </summary>
    protected virtual string SystemName => GetType().Name;
    
    public override void _Ready()
    {
        base._Ready();
        Initialize();
    }
    
    /// <summary>
    /// 初始化系统 - 子类重写此方法进行初始化
    /// </summary>
    protected virtual void Initialize()
    {
        GD.Print($"[BaseSystem] {SystemName} initialized");
        IsInitialized = true;
    }
    
    /// <summary>
    /// 导出保存数据 - 子类重写此方法实现数据持久化
    /// </summary>
    /// <returns>可序列化的字典数据</returns>
    public virtual Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary<string, object>();
    }
    
    /// <summary>
    /// 导入保存数据 - 子类重写此方法实现数据加载
    /// </summary>
    /// <param name="data">保存的字典数据</param>
    public virtual void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
    }
    
    /// <summary>
    /// 重置系统数据
    /// </summary>
    public virtual void Reset()
    {
        IsInitialized = false;
    }
    
    /// <summary>
    /// 获取系统唯一ID
    /// </summary>
    public virtual string GetId()
    {
        return SystemName;
    }
}

} // namespace ClawRPG.Scripts.Framework
