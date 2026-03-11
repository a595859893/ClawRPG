// RandomWorldEventUI.cs - 随机世界事件UI
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.Systems {
    public partial class RandomWorldEventUI : Control {
        private VBoxContainer _mainContainer;
        private Label _titleLabel;
        private ScrollContainer _eventListContainer;
        private VBoxContainer _eventList;
        private Label _statsLabel;
        private Button _closeButton;
        
        // 事件按钮映射
        private Dictionary<string, Button> _eventButtons = new Dictionary<string, Button>();
        
        public override void _Ready() {
            SetupUI();
            ConnectSignals();
            RefreshEventList();
        }

        private void SetupUI() {
            // 背景面板
            var bgPanel = new Panel {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                Modulate = new Color(1, 1, 1, 0.85f)
            };
            AddChild(bgPanel);
            
            // 主容器
            _mainContainer = new VBoxContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 50,
                OffsetTop = 50,
                OffsetRight = -50,
                OffsetBottom = -50
            };
            AddChild(_mainContainer);
            
            // 标题
            _titleLabel = new Label {
                Text = "🌍 随机世界事件",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                CustomMinimumSize = new Vector2(0, 60)
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 32);
            _mainContainer.AddChild(_titleLabel);
            
            // 统计信息
            _statsLabel = new Label {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _statsLabel.AddThemeFontSizeOverride("font_size", 16);
            _mainContainer.AddChild(_statsLabel);
            
            // 事件列表容器
            _eventListContainer = new ScrollContainer {
                VscrollVisible = true,
                CustomMinimumSize = new Vector2(0, 400)
            };
            _eventListContainer.SetHExpandFlags(Control.ExpandFlags.IgnoreSize);
            _mainContainer.AddChild(_eventListContainer);
            
            // 事件列表
            _eventList = new VBoxContainer {
                CustomMinimumSize = new Vector2(500, 0)
            };
            _eventListContainer.AddChild(_eventList);
            
            // 关闭按钮
            _closeButton = new Button {
                Text = "关闭",
                CustomMinimumSize = new Vector2(200, 50)
            };
            _closeButton.Pressed += OnClosePressed;
            _mainContainer.AddChild(_closeButton);
            
            // 默认隐藏
            Visible = false; 
        }

        private void ConnectSignals() {
            if (RandomWorldEventSystem.Instance != null) {
                RandomWorldEventSystem.Instance.Connect(
                    RandomWorldEventSystem.SignalName.EventTriggered,
                    Callable.From<WorldEventConfig>(OnEventTriggered)
                );
                RandomWorldEventSystem.Instance.Connect(
                    RandomWorldEventSystem.SignalName.EventExpired,
                    Callable.From<string>(OnEventExpired)
                );
            }
        }

        /// <summary>
        /// 刷新事件列表
        /// </summary>
        public void RefreshEventList() {
            // 清除现有按钮
            foreach (var btn in _eventButtons.Values) {
                btn.QueueFree();
            }
            _eventButtons.Clear();
            
            // 获取活跃事件
            var activeEvents = RandomWorldEventSystem.Instance?.GetActiveEvents() ?? new List<WorldEventConfig>();
            
            // 更新统计
            var playerData = RandomWorldEventSystem.Instance?.GetPlayerEventData();
            if (playerData != null) {
                _statsLabel.Text = $"总触发次数: {playerData.TotalEventsTriggered} | 见证传说: {playerData.LegendaryEventsWitnessed} | 当前活跃: {activeEvents.Count}";
            }
            
            if (activeEvents.Count == 0) {
                var noEventLabel = new Label {
                    Text = "当前没有活跃的世界事件",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    CustomMinimumSize = new Vector2(0, 100)
                };
                noEventLabel.AddThemeFontSizeOverride("font_size", 18);
                _eventList.AddChild(noEventLabel);
                return;
            }
            
            // 创建事件按钮
            foreach (var config in activeEvents) {
                CreateEventButton(config);
            }
        }

        /// <summary>
        /// 创建事件按钮
        /// </summary>
        private void CreateEventButton(WorldEventConfig config) {
            var eventData = config.Event;
            
            var eventPanel = new HBoxContainer {
                CustomMinimumSize = new Vector2(0, 80)
            };
            
            // 稀有度颜色
            Color rarityColor = GetRarityColor(eventData.Rarity);
            
            // 事件图标
            var iconLabel = new Label {
                Text = GetEventTypeIcon(eventData.EventType),
                CustomMinimumSize = new Vector2(60, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            iconLabel.AddThemeFontSizeOverride("font_size", 32);
            eventPanel.AddChild(iconLabel);
            
            // 事件信息容器
            var infoContainer = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            
            // 事件名称
            var nameLabel = new Label {
                Text = eventData.EventName,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 20);
            nameLabel.Modulate = rarityColor;
            infoContainer.AddChild(nameLabel);
            
            // 描述
            var descLabel = new Label {
                Text = eventData.Description,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            descLabel.AddThemeFontSizeOverride("font_size", 14);
            infoContainer.AddChild(descLabel);
            
            // 奖励信息
            var rewardText = "";
            if (eventData.GoldReward > 0) rewardText += $"💰 {eventData.GoldReward} ";
            if (eventData.ExpReward > 0) rewardText += $"✨ {eventData.ExpReward} ";
            if (eventData.ItemRewards.Count > 0) rewardText += $"🎁 {eventData.ItemRewards.Count}种物品";
            
            if (!string.IsNullOrEmpty(rewardText)) {
                var rewardLabel = new Label {
                    Text = rewardText,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                rewardLabel.AddThemeFontSizeOverride("font_size", 14);
                rewardLabel.Modulate = new Color(1, 0.9, 0.5);
                infoContainer.AddChild(rewardLabel);
            }
            
            eventPanel.AddChild(infoContainer);
            
            // 领取按钮
            var claimButton = new Button {
                Text = "领取奖励",
                CustomMinimumSize = new Vector2(120, 0)
            };
            claimButton.Pressed += () => OnClaimRewardPressed(config.Event.EventId);
            eventPanel.AddChild(claimButton);
            
            // 倒计时
            var timeLeft = (config.ExpireTime - DateTime.Now).TotalSeconds;
            var countdownLabel = new Label {
                Text = $"{(int)(timeLeft / 60)}:{(int)(timeLeft % 60):D2}",
                CustomMinimumSize = new Vector2(60, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            countdownLabel.AddThemeFontSizeOverride("font_size", 18);
            countdownLabel.Modulate = timeLeft < 60 ? Colors.Red : Colors.White;
            eventPanel.AddChild(countdownLabel);
            
            _eventList.AddChild(eventPanel);
            _eventButtons[config.Event.EventId] = claimButton;
        }

        /// <summary>
        /// 获取稀有度颜色
        /// </summary>
        private Color GetRarityColor(EventRarity rarity) {
            return rarity switch {
                EventRarity.Common => Colors.White,
                EventRarity.Uncommon => Colors.Green,
                EventRarity.Rare => new Color(0.3f, 0.5f, 1f),
                EventRarity.Epic => new Color(0.6f, 0.3f, 0.8f),
                EventRarity.Legendary => new Color(1f, 0.6f, 0f),
                _ => Colors.White
            };
        }

        /// <summary>
        /// 获取事件类型图标
        /// </summary>
        private string GetEventTypeIcon(WorldEventType type) {
            return type switch {
                WorldEventType.ResourceSpawn => "🌿",
                WorldEventType.EnemyInvasion => "⚔️",
                WorldEventType.TreasureChest => "📦",
                WorldEventType.MerchantArrival => "🏪",
                WorldEventType.WeatherChange => "🌤️",
                WorldEventType.LuckyMoment => "🍀",
                WorldEventType.CurseEvent => "💀",
                WorldEventType.BlessingEvent => "✨",
                WorldEventType.HiddenChest => "🎁",
                WorldEventType.RARE_DragonAttack => "🐉",
                _ => "❓"
            };
        }

        /// <summary>
        /// 事件触发回调
        /// </summary>
        private void OnEventTriggered(WorldEventConfig config) {
            RefreshEventList();
            
            // 显示通知
            ShowEventNotification(config.Event);
        }

        /// <summary>
        /// 事件过期回调
        /// </summary>
        private void OnEventExpired(string eventId) {
            RefreshEventList();
        }

        /// <summary>
        /// 领取奖励按钮按下
        /// </summary>
        private void OnClaimRewardPressed(string eventId) {
            RandomWorldEventSystem.Instance?.ClaimEventReward(eventId);
            RefreshEventList();
        }

        /// <summary>
        /// 显示事件通知
        /// </summary>
        private void ShowEventNotification(WorldEventData eventData) {
            // 可以在此处添加通知动画
            GD.Print($"[RandomWorldEventUI] 新事件通知: {eventData.EventName}");
        }

        /// <summary>
        /// 关闭按钮按下
        /// </summary>
        private void OnClosePressed() {
            Visible = false; 
        }

        /// <summary>
        /// 切换显示
        /// </summary>
        public void ToggleVisibility() {
            Visible = !Visible;
            if (Visible) {
                RefreshEventList();
            }
        }

        /// <summary>
        /// 打开UI
        /// </summary>
        public void Open() {
            Visible = true;
            RefreshEventList();
        }

        /// <summary>
        /// 关闭UI
        /// </summary>
        public void Close() {
            Visible = false; 
        }
    }
}
