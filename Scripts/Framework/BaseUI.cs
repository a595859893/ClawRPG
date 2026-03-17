using Godot;
using System;

/// <summary>
/// 所有UI的基类
/// 提供统一的显示/隐藏/刷新接口
/// 支持单例模式 (通过 Instance 属性访问)
/// </summary>
public abstract class BaseUI : Control
{
    /// <summary>
    /// 单例实例 (子类需要实现)
    /// </summary>
    public static BaseUI Instance { get; protected set; }
    
    /// <summary>
    /// UI是否正在显示
    /// </summary>
    public bool IsVisible { get; private set; } = false;
    
    /// <summary>
    /// UI名称（用于日志）
    /// </summary>
    protected virtual string UIName => GetType().Name;
    
    public override void _Ready()
    {
        base._Ready();
        InitializeUI();
    }
    
    /// <summary>
    /// 初始化UI - 子类重写此方法进行初始化
    /// </summary>
    protected virtual void InitializeUI()
    {
        GD.Print($"[BaseUI] {UIName} initialized");
    }
    
    /// <summary>
    /// 显示UI并刷新数据
    /// </summary>
    public virtual void Show()
    {
        if (!IsVisible)
        {
            Visible = true;
            IsVisible = true;
            OnShow();
        }
    }
    
    /// <summary>
    /// 隐藏UI
    /// </summary>
    public virtual void Hide()
    {
        if (IsVisible)
        {
            Visible = false;
            IsVisible = false;
            OnHide();
        }
    }
    
    /// <summary>
    /// 刷新UI显示的数据 - 子类重写此方法更新UI内容
    /// </summary>
    public virtual void Refresh()
    {
        if (IsVisible)
        {
            OnRefresh();
        }
    }
    
    /// <summary>
    /// 显示时的回调 - 子类重写此方法处理显示逻辑
    /// </summary>
    protected virtual void OnShow()
    {
        Refresh();
    }
    
    /// <summary>
    /// 隐藏时的回调 - 子类重写此方法处理隐藏逻辑
    /// </summary>
    protected virtual void OnHide()
    {
    }
    
    /// <summary>
    /// 刷新时的回调 - 子类重写此方法更新具体UI元素
    /// </summary>
    protected virtual void OnRefresh()
    {
    }
    
    /// <summary>
    /// 切换UI显示状态
    /// </summary>
    public virtual void Toggle()
    {
        if (IsVisible)
            Hide();
        else
            Show();
    }
}
