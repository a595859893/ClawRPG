using Godot;
using System;

/// <summary>
/// 所有管理器的基础类
/// 提供统一的生命周期管理和通用接口
/// </summary>
public abstract partial class ManagerBase : BaseSystem
{
    /// <summary>
    /// 管理器是否已准备好
    /// </summary>
    public bool IsReady { get; protected set; } = false;
    
    /// <summary>
    /// 管理器优先级（数值越小越先初始化）
    /// </summary>
    public virtual int Priority => 100;
    
    /// <summary>
    /// 初始化完成回调
    /// </summary>
    public event Action<ManagerBase> OnInitialized;
    
    /// <summary>
    /// 管理器更新（每帧调用）
    /// </summary>
    public virtual void ManagerUpdate(double delta) { }
    
    /// <summary>
    /// 物理更新
    /// </summary>
    public virtual void ManagerPhysicsUpdate(double delta) { }
    
    /// <summary>
    /// 延迟更新（每秒调用）
    /// </summary>
    public virtual void ManagerLateUpdate(double delta) { }
    
    /// <summary>
    /// 销毁管理器
    /// </summary>
    public virtual void Shutdown()
    {
        GD.Print($"[{GetType().Name}] Shutdown");
    }
    
    /// <summary>
    /// 通知初始化完成
    /// </summary>
    protected void NotifyInitialized()
    {
        IsReady = true;
        OnInitialized?.Invoke(this);
    }

    public override Dictionary<string, object> ExportSaveData() => new();
    public override void ImportSaveData(Dictionary<string, object> data) { }
}
