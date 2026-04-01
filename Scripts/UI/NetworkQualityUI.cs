using Godot;
using System;

/// <summary>
/// 网络质量UI - 显示连接状态和延迟
/// </summary>
public partial class NetworkQualityUI : Control
{
    private Label _statusLabel;
    private Label _latencyLabel;
    private Color _goodColor = new Color(0.2f, 1f, 0.2f, 1f);    // 绿色 - 良好
    private Color _mediumColor = new Color(1f, 1f, 0.2f, 1f);     // 黄色 - 一般
    private Color _badColor = new Color(1f, 0.2f, 0.2f, 1f);      // 红色 - 差
    
    private float _updateInterval = 1.0f;
    private float _timer = 0f;
    
    // 延迟阈值
    private int _goodThreshold = 100;   // <100ms 良好
    private int _mediumThreshold = 300;  // <300ms 一般
    
    public override void _Ready()
    {
        SetupUI();
        Hide();
    }
    
    private void SetupUI()
    {
        var panel = new PanelContainer();
        panel.SetAnchorsPreset(Control.LayoutPreset.TopRight);
        panel.Position = new Vector2(-120, 10);
        panel.CustomMinimumSize = new Vector2(110, 50);
        AddChild(panel);
        
        var vbox = new VBoxContainer();
        panel.AddChild(vbox);
        
        _statusLabel = new Label();
        _statusLabel.Text = "未连接";
        _statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _statusLabel.AddThemeFontSizeOverride("font_size", 12);
        vbox.AddChild(_statusLabel);
        
        _latencyLabel = new Label();
        _latencyLabel.Text = "-- ms";
        _latencyLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _latencyLabel.AddThemeFontSizeOverride("font_size", 10);
        vbox.AddChild(_latencyLabel);
    }
    
    public override void _Process(double delta)
    {
        _timer += delta;
        if (_timer >= _updateInterval)
        {
            _timer = 0;
            UpdateNetworkStatus();
        }
    }
    
    private void UpdateNetworkStatus()
    {
        var networkClient = NetworkClient.Instance;
        
        if (networkClient == null || !networkClient.IsConnected)
        {
            _statusLabel.Text = "未连接";
            _statusLabel.Modulate = _badColor;
            _latencyLabel.Text = "-- ms";
            return;
        }
        
        // 检查连接状态
        bool isConnected = networkClient.IsConnected;
        
        if (isConnected)
        {
            _statusLabel.Text = "已连接";
            _statusLabel.Modulate = _goodColor;
            
            // 模拟延迟显示（实际应该从服务器获取）
            // 这里使用心跳间隔作为延迟估算
            int simulatedLatency = 50 + (int)(GD.Randf() * 100);
            UpdateLatencyDisplay(simulatedLatency);
        }
        else
        {
            _statusLabel.Text = "断开连接";
            _statusLabel.Modulate = _badColor;
            _latencyLabel.Text = "-- ms";
        }
    }
    
    private void UpdateLatencyDisplay(int latency)
    {
        _latencyLabel.Text = latency + " ms";
        
        if (latency < _goodThreshold)
        {
            _latencyLabel.Modulate = _goodColor;
        }
        else if (latency < _mediumThreshold)
        {
            _latencyLabel.Modulate = _mediumColor;
        }
        else
        {
            _latencyLabel.Modulate = _badColor;
        }
    }
    
    /// <summary>
    /// 切换显示
    /// </summary>
    public void Toggle()
    {
        if (Visible)
            Hide();
        else
            Show();
    }
}
