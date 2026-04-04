using Godot;
using System;

namespace ClawRPG.Systems;

/// <summary>
/// 遗物协同发现通知面板 — 屏幕角落动画提示
/// </summary>
public partial class RelicSynergyNotification : CanvasLayer
{
    private Label _titleLabel;
    private Label _bodyLabel;
    private PanelContainer _panel;
    private Tween _tween;
    
    public override void _Ready()
    {
        // 初始隐藏
        Visible = false;
        
        SetupUI();
        
        // 订阅协同发现信号
        if (RelicSynergySystem.Instance != null)
        {
            RelicSynergySystem.Instance.Connect("SynergyDiscovered", 
                Callable.From<string, string>(OnSynergyDiscovered));
        }
        else
        {
            // 延迟订阅
            var timer = new Godot.Timer { OneShot = true, WaitTime = 2.0f };
            AddChild(timer);
            timer.Timeout += () => {
                timer.QueueFree();
                if (RelicSynergySystem.Instance != null)
                {
                    RelicSynergySystem.Instance.Connect("SynergyDiscovered",
                        Callable.From<string, string>(OnSynergyDiscovered));
                }
            };
            timer.Start();
        }
    }
    
    private void SetupUI()
    {
        // 根面板
        _panel = new PanelContainer
        {
            AnchorLeft = 1.0f,
            AnchorRight = 1.0f,
            AnchorTop = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = -360,
            OffsetTop = -80,
            OffsetRight = -20,
            OffsetBottom = 80,
            ZIndex = 2000
        };
        
        // 背景样式
        var style = new StyleBoxFlat
        {
            BgColor = new Color(0.08f, 0.04f, 0.12f, 0.95f), // 深紫色背景
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2,
            BorderWidthBottom = 2,
            BorderColor = new Color(0.6f, 0.3f, 0.9f, 0.8f), // 紫色边框
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            ContentMarginLeft = 16,
            ContentMarginRight = 16,
            ContentMarginTop = 12,
            ContentMarginBottom = 12
        };
        _panel.AddThemeStyleboxOverride("panel", style);
        
        // VBox 容器
        var vbox = new VBoxContainer { }
;
        _panel.AddChild(vbox);
        
        // 标题行
        var hbox = new HBoxContainer { };
        vbox.AddChild(hbox);
        
        var iconLabel = new Label { Text = "✨", Align = Label.AlignModeEnum.Left };
        hbox.AddChild(iconLabel);
        
        _titleLabel = new Label
        {
            Text = "发现协同！",
            Align = Label.AlignModeEnum.Left,
            AutowrapMode = TextServer.AutowrapMode.Off
        };
        _titleLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.7f, 1.0f, 1.0f)); // 浅紫色
        _titleLabel.AddThemeFontSizeOverride("font_size", 18);
        hbox.AddChild(_titleLabel);
        
        // 分隔线
        var sep = new HSeparator { };
        sep.AddThemeConstantOverride("separation", 4);
        vbox.AddChild(sep);
        
        // 正文
        _bodyLabel = new Label
        {
            Text = "",
            Align = Label.AlignModeEnum.Left,
            AutowrapMode = TextServer.AutowrapMode.Word,
        };
        _bodyLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.85f, 0.95f, 1.0f));
        _bodyLabel.AddThemeFontSizeOverride("font_size", 14);
        vbox.AddChild(_bodyLabel);
        
        AddChild(_panel);
    }
    
    private void OnSynergyDiscovered(string synergyId, string message)
    {
        ShowNotification(message);
    }
    
    private void ShowNotification(string message)
    {
        _bodyLabel.Text = message;
        Visible = true;
        Modulate = new Color(1, 1, 1, 0);
        
        // 清理旧 Tween
        _tween?.Kill();
        
        _tween = CreateTween();
        _tween.SetParallel(true);
        
        // 淡入 + 缩放
        _tween.TweenProperty(this, "modulate:a", 1.0f, 0.3f);
        
        // 等待后淡出
        _tween.Chain();
        _tween.TweenInterval(3.5f);
        _tween.TweenProperty(this, "modulate:a", 0.0f, 0.5f);
        
        _tween.Finished += () => {
            Visible = false;
        };
    }
}
