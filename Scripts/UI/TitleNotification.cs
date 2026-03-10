using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 称号解锁通知弹窗
    /// </summary>
    public class TitleNotification : Control {
        private static TitleNotification _instance;
        public static TitleNotification Instance {
            get {
                if (_instance == null) {
                    _instance = new TitleNotification();
                }
                return _instance;
            }
        }
        
        // 通知队列
        private Queue<TitleSystem.Title> _notificationQueue = new();
        private bool _isShowing = false;
        private const int MaxVisible = 3;
        
        // 通知节点列表
        private List<Control> _activeNotifications = new();
        
        public TitleNotification() {
            _instance = this;
            SetAnchorsAndMarginsPreset(Control.Preset.FullRect);
            Name = "TitleNotification";
        }
        
        public override void _Ready() {
            // 订阅称号解锁事件
            if (TitleSystem.Instance != null) {
                TitleSystem.Instance.OnTitleUnlocked += ShowTitleNotification;
            }
        }
        
        /// <summary>
        /// 显示称号解锁通知
        /// </summary>
        public void ShowTitleNotification(TitleSystem.Title title) {
            _notificationQueue.Enqueue(title);
            ProcessQueue();
        }
        
        private void ProcessQueue() {
            if (_isShowing || _notificationQueue.Count == 0) return;
            
            // 如果已有足够数量的通知显示，等待
            if (_activeNotifications.Count >= MaxVisible) return;
            
            _isShowing = true;
            var title = _notificationQueue.Dequeue();
            CreateNotificationPopup(title);
        }
        
        private void CreateNotificationPopup(TitleSystem.Title title) {
            var titleSystem = TitleSystem.Instance;
            
            // 创建通知面板
            Panel panel = new Panel();
            panel.CustomMinimumSize = new Vector2(300, 70);
            panel.SetAnchorsPreset(Control.Preset.RightWide);
            panel.SetMargins(-320, 20 + _activeNotifications.Count * 80, -10, 0);
            AddChild(panel);
            _activeNotifications.Add(panel);
            
            // 背景颜色
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            styleBox.BorderWidthBottom = 3;
            styleBox.BorderColor = titleSystem.GetRarityColor(title.Rarity);
            styleBox.CornerRadiusTopLeft = 8;
            styleBox.CornerRadiusTopRight = 8;
            styleBox.CornerRadiusBottomLeft = 8;
            styleBox.CornerRadiusBottomRight = 8;
            panel.AddThemeStyleboxOverride("panel", styleBox);
            
            // 内容容器
            HBoxContainer hbox = new HBoxContainer();
            hbox.SetAnchorsAndMarginsPreset(Control.Preset.FullRect);
            hbox.AddThemeConstantOverride("separation", 10);
            panel.AddChild(hbox);
            
            // 图标
            Label iconLabel = new Label();
            iconLabel.Text = "🏆";
            iconLabel.AddThemeFontSizeOverride("font_size", 28);
            iconLabel.SetAnchorsPreset(Control.Preset.LeftWide);
            iconLabel.SetMargins(15, 0, 0, 0);
            hbox.AddChild(iconLabel);
            
            // 文本内容
            VBoxContainer textVBox = new VBoxContainer();
            textVBox.AddThemeConstantOverride("separation", 5);
            hbox.AddChild(textVBox);
            
            // 标题
            Label titleLabel = new Label();
            titleLabel.Text = "称号解锁!";
            titleLabel.AddThemeFontSizeOverride("font_size", 14);
            titleLabel.AddThemeColorOverride("font_color", new Color(1, 0.9, 0.5));
            textVBox.AddChild(titleLabel);
            
            // 称号名称
            Label nameLabel = new Label();
            nameLabel.Text = title.Name;
            nameLabel.AddThemeFontSizeOverride("font_size", 18);
            nameLabel.AddThemeColorOverride("font_color", titleSystem.GetRarityColor(title.Rarity));
            textVBox.AddChild(nameLabel);
            
            // 稀有度标签
            Label rarityLabel = new Label();
            rarityLabel.Text = GetRarityName(title.Rarity);
            rarityLabel.AddThemeFontSizeOverride("font_size", 12);
            rarityLabel.AddThemeColorOverride("font_color", titleSystem.GetRarityColor(title.Rarity));
            rarityLabel.SetAnchorsPreset(Control.Preset.RightWide);
            rarityLabel.SetMargins(0, 0, 15, 0);
            hbox.AddChild(rarityLabel);
            
            // 动画效果
            panel.Modulate = new Color(1, 1, 1, 0);
            var tween = CreateTween();
            tween.TweenProperty(panel, "modulate:a", 1.0f, 0.3f);
            tween.TweenInterval(3.0f);
            tween.TweenProperty(panel, "modulate:a", 0.0f, 0.5f);
            tween.TweenCallback(() => {
                if (panel != null) {
                    panel.QueueFree();
                    _activeNotifications.Remove(panel);
                    _isShowing = false;
                    // 重新调整其他通知的位置
                    RepositionNotifications();
                    // 处理队列中的下一个
                    ProcessQueue();
                }
            });
        }
        
        private void RepositionNotifications() {
            for (int i = 0; i < _activeNotifications.Count; i++) {
                var panel = _activeNotifications[i];
                panel.SetMargins(-320, 20 + i * 80, -10, 0);
            }
        }
        
        private string GetRarityName(TitleSystem.TitleRarity rarity) {
            switch (rarity) {
                case TitleSystem.TitleRarity.Common: return "普通";
                case TitleSystem.TitleRarity.Uncommon: return "优秀";
                case TitleSystem.TitleRarity.Rare: return "稀有";
                case TitleSystem.TitleRarity.Epic: return "史诗";
                case TitleSystem.TitleRarity.Legendary: return "传说";
                default: return "";
            }
        }
    }
}
