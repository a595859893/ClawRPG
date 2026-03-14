using Godot;
using System;

/// <summary>
/// $UI_NAME$ 界面
/// 描述: $DESCRIPTION$
/// </summary>
public class $CLASS_NAME$UI : BaseUI
{
    // UI 组件引用
    // [Export] private Label _titleLabel;
    // [Export] private Button _confirmButton;
    
    /// <summary>
    /// UI名称
    /// </summary>
    protected override string UIName => "$CLASS_NAME$";
    
    /// <summary>
    /// 初始化UI
    /// </summary>
    protected override void InitializeUI()
    {
        base.InitializeUI();
        
        // TODO: 连接信号
        // _confirmButton.Connect("pressed", this, nameof(_OnConfirmPressed));
        
        GD.Print($"[$CLASS_NAME$UI] Initialized");
    }
    
    /// <summary>
    /// 刷新UI
    /// </summary>
    protected override void OnRefresh()
    {
        // TODO: 更新UI显示
    }
    
    // === 信号处理方法 ===
    // private void _OnConfirmPressed()
    // {
    //     GD.Print("[$CLASS_NAME$UI] Confirm pressed");
    // }
}
