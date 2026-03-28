using Godot;
using System;
using System.Collections.Generic;

namespace Framework
{
    /// <summary>
    /// Combo 重新发现通知 — 比首次发现更简洁
    /// 显示 "COMBO REMEMBERED" 而不是完整的发现面板
    /// </summary>
    public class ComboRediscoveredNotification : Control
    {
        private CanvasLayer _canvasLayer;
        private VBoxContainer _container;
        private Queue<string> _pendingComboIds = new();
        private bool _isShowing;
        private Timer _displayTimer;
        private Label _comboNameLabel;
        
        [Export] private float _displayDuration = 2.0f;
        [Export] private float _slideInDuration = 0.25f;
        [Export] private float _slideOutDuration = 0.3f;
        
        public override void _Ready()
        {
            Visible = false;
            
            _canvasLayer = new CanvasLayer();
            _canvasLayer.Layer = 195;
            AddChild(_canvasLayer);
            
            _container = new VBoxContainer();
            _container.SetAnchorsPreset(Control.LayoutPreset.Center);
            _container.Position = new Vector2(-160, -40);
            _container.CustomMinimumSize = new Vector2(320, 0);
            _canvasLayer.AddChild(_container);
            
            _displayTimer = new Timer();
            _displayTimer.OneShot = true;
            _displayTimer.Timeout += OnDisplayTimerTimeout;
            _canvasLayer.AddChild(_displayTimer);
            
            // 订阅重发现事件
            ComboForgetData.ComboRediscovered += OnComboRediscovered;
        }
        
        public override void _ExitTree()
        {
            ComboForgetData.ComboRediscovered -= OnComboRediscovered;
        }
        
        private void OnComboRediscovered(string comboId)
        {
            _pendingComboIds.Enqueue(comboId);
            if (!_isShowing)
            {
                ShowNextNotification();
            }
        }
        
        private void ShowNextNotification()
        {
            if (_pendingComboIds.Count == 0)
            {
                _isShowing = false;
                Visible = false;
                return;
            }
            
            _isShowing = true;
            Visible = true;
            string comboId = _pendingComboIds.Dequeue();
            
            // 清除旧内容
            foreach (var child in _container.GetChildren())
            {
                child.QueueFree();
            }
            
            var panel = CreateRediscoverPanel(comboId);
            _container.AddChild(panel);
            
            // 入场动画：淡入
            var tween = CreateTween();
            panel.Modulate = new Color(1, 1, 1, 0);
            tween.TweenProperty(panel, "modulate:a", 1f, _slideInDuration);
            
            // 启动计时器
            _displayTimer.Stop();
            _displayTimer.Start(_displayDuration);
        }
        
        private void OnDisplayTimerTimeout()
        {
            if (_container.GetChildCount() > 0)
            {
                var notification = _container.GetChild(0);
                var tween = CreateTween();
                tween.TweenProperty(notification, "modulate:a", 0f, _slideOutDuration);
                tween.Callback(Callable.From(() =>
                {
                    foreach (var child in _container.GetChildren())
                    {
                        child.QueueFree();
                    }
                    ShowNextNotification();
                }));
            }
            else
            {
                ShowNextNotification();
            }
        }
        
        private Control CreateRediscoverPanel(string comboId)
        {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(320, 60);
            
            var style = new StyleBoxFlat();
            style.BorderWidthLeft = 2;
            style.BorderWidthRight = 2;
            style.BorderWidthTop = 2;
            style.BorderWidthBottom = 2;
            style.BorderColor = new Color(0.4f, 0.7f, 1f, 0.8f); // 蓝色边框，表示重新激活
            style.BgColor = new Color(0.03f, 0.08f, 0.15f, 0.9f);
            style.CornerRadiusTopLeft = 8;
            style.CornerRadiusTopRight = 8;
            style.CornerRadiusBottomLeft = 8;
            style.CornerRadiusBottomRight = 8;
            style.ContentMarginLeft = 16;
            style.ContentMarginTop = 10;
            style.ContentMarginRight = 16;
            style.ContentMarginBottom = 10;
            panel.AddThemeStyleboxOverride("panel", style);
            
            var hbox = new HBoxContainer();
            hbox.Alignment = BoxContainer.AlignmentMode.Center;
            panel.AddChild(hbox);
            
            // 图标
            var iconLabel = new Label();
            iconLabel.Text = "🔄";
            iconLabel.AddThemeFontSizeOverride("font_size", 20);
            hbox.AddChild(iconLabel);
            
            // "REMEMBERED" 标签
            var titleLabel = new Label();
            titleLabel.Text = "COMBO REMEMBERED  ";
            titleLabel.AddThemeFontSizeOverride("font_size", 12);
            titleLabel.Modulate = new Color(0.5f, 0.7f, 1f);
            hbox.AddChild(titleLabel);
            
            // Combo 名称
            _comboNameLabel = new Label();
            _comboNameLabel.Text = GetComboDisplayName(comboId);
            _comboNameLabel.AddThemeFontSizeOverride("font_size", 16);
            _comboNameLabel.Modulate = new Color(0.7f, 0.9f, 1f);
            hbox.AddChild(_comboNameLabel);
            
            return panel;
        }
        
        private string GetComboDisplayName(string comboId)
        {
            // 从 ComboSystem 获取 combo 名称
            try
            {
                var system = ComboSystem.Instance;
                if (system != null)
                {
                    var combos = system.GetAllCombos();
                    if (combos.TryGetValue(comboId, out var combo))
                    {
                        return combo.comboName ?? comboId;
                    }
                }
            }
            catch { }
            return comboId;
        }
    }
}
