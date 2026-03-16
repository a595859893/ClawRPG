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
        // 示例：连接按钮信号、输入框信号等
        // _confirmButton.Connect("pressed", this, nameof(_OnConfirmPressed));
        // _cancelButton.Connect("pressed", this, nameof(_OnCancelPressed));
        // _inputField.Connect("text_changed", this, nameof(_OnTextChanged));
        
        GD.Print($"[$CLASS_NAME$UI] Initialized");
    }
    
    /// <summary>
    /// 刷新UI
    /// </summary>
    protected override void OnRefresh()
    {
        // TODO: 更新UI显示
        // 示例：根据数据更新文本、进度条、图片等
        // _titleLabel.Text = _data.GetValueOrDefault("title", "");
        // _goldLabel.Text = $"金币: {_playerGold}";
        // _progressBar.Value = _currentProgress;
        // _avatarTexture.Texture = LoadTexture(_playerAvatarPath);
    }
    
    // === 信号处理方法 ===
    // private void _OnConfirmPressed()
    // {
    //     GD.Print("[$CLASS_NAME$UI] Confirm pressed");
    // }
    
    /// <summary>
    /// 每帧更新（UI动画、计时器等）
    /// </summary>
    public override void _Process(float delta)
    {
        // TODO: 添加每帧逻辑
        // 示例：更新动画、倒计时、动画播放等
        // _animationPlayer.Process(delta);
        // if (_countdown > 0) _countdown -= delta;
    }
}
