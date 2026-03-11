using Godot;
using System;
using System.Collections.Generic;

namespace 悬疑RPG
{
    /// <summary>
    /// 每日登录奖励界面
    /// </summary>
    public partial class DailyLoginRewardUI : Control
    {
        private static DailyLoginRewardUI Instance { get; set; }

        // UI组件
        private Label titleLabel;
        private Label consecutiveDaysLabel;
        private Label totalDaysLabel;
        private HBoxContainer rewardsContainer;
        private Button closeButton;
        private Label statusLabel;

        // 奖励天数按钮
        private List<Button> dayButtons = new List<Button>();
        private List<Label> dayLabels = new List<Label>();

        // 当前选中的天数
        private int selectedDay = 1;

        public override void _Ready()
        {
            Instance = this;
            SetupUI();
            Visible = false;
            
            // 连接信号
            DailyLoginRewardSystem.Instance.Connect(DailyLoginRewardSystem.SignalName.LoginDaysUpdated, Callable.From(OnLoginDaysUpdated));
            DailyLoginRewardSystem.Instance.Connect(DailyLoginRewardSystem.SignalName.RewardClaimed, Callable.From(OnRewardClaimed));
            DailyLoginRewardSystem.Instance.Connect(DailyLoginRewardSystem.SignalName.NewDayAvailable, Callable.From(OnNewDayAvailable));

            // 按键处理
            GetTree().Root.Connect("size_changed", Callable.From(OnWindowResized));
        }

        /// <summary>
        /// 设置UI
        /// </summary>
        private void SetupUI()
        {
            // 主容器
            var mainPanel = new PanelContainer
            {
                AnchorLeft = 0.5f,
                AnchorTop = 0.5f,
                AnchorRight = 0.5f,
                AnchorBottom = 0.5f,
                OffsetLeft = -300,
                OffsetTop = -250,
                OffsetRight = 300,
                OffsetBottom = 250,
                GrowHorizontal = Control.GrowDirection.Center,
                GrowVertical = Control.GrowDirection.Center
            };
            AddChild(mainPanel);

            var mainVBox = new VBoxContainer { CustomMinimumSize = new Vector2(600, 500) };
            mainPanel.AddChild(mainVBox);

            // 标题栏
            var titleBar = new HBoxContainer { CustomMinimumSize = new Vector2(0, 50) };
            mainVBox.AddChild(titleBar);

            titleLabel = new Label
            {
                Text = "每日登录奖励",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill
            };
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            titleBar.AddChild(titleLabel);

            closeButton = new Button { Text = "×", CustomMinimumSize = new Vector2(40, 40) };
            closeButton.Pressed += () => Hide();
            titleBar.AddChild(closeButton);

            // 登录天数信息
            var infoBar = new HBoxContainer { CustomMinimumSize = new Vector2(0, 40) };
            mainVBox.AddChild(infoBar);

            consecutiveDaysLabel = new Label { Text = "连续登录: 0 天", SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill };
            consecutiveDaysLabel.AddThemeFontSizeOverride("font_size", 18);
            infoBar.AddChild(consecutiveDaysLabel);

            totalDaysLabel = new Label { Text = "累计登录: 0 天", SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill };
            totalDaysLabel.AddThemeFontSizeOverride("font_size", 18);
            infoBar.AddChild(totalDaysLabel);

            // 分割线
            var separator = new HSeparator();
            mainVBox.AddChild(separator);

            // 奖励显示区域
            var scrollContainer = new ScrollContainer { CustomMinimumSize = new Vector2(0, 300), SizeFlagsVertical = Control.SizeFlags.Expand | Control.SizeFlags.Fill };
            mainVBox.AddChild(scrollContainer);

            var gridContainer = new GridContainer { Columns = 7 };
            gridContainer.AddThemeConstantOverride("h_separation", 10);
            gridContainer.AddThemeConstantOverride("v_separation", 10);
            scrollContainer.AddChild(gridContainer);

            // 创建7天的奖励显示
            var rewards = DailyLoginRewardSystem.Instance.GetAllRewards();
            for (int i = 0; i < rewards.Count; i++)
            {
                CreateDayCard(gridContainer, i + 1, rewards[i]);
            }

            // 状态标签
            statusLabel = new Label
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                CustomMinimumSize = new Vector2(0, 30)
            };
            statusLabel.AddThemeFontSizeOverride("font_size", 16);
            mainVBox.AddChild(statusLabel);

            // 更新显示
            UpdateDisplay();
        }

        /// <summary>
        /// 创建单天奖励卡片
        /// </summary>
        private void CreateDayCard(GridContainer parent, int day, DailyLoginReward reward)
        {
            var cardContainer = new VBoxContainer { CustomMinimumSize = new Vector2(80, 120) };
            parent.AddChild(cardContainer);

            // 天数标签
            var dayLabel = new Label
            {
                Text = $"第{day}天",
                HorizontalAlignment = HorizontalAlignment.Center,
                CustomMinimumSize = new Vector2(0, 25)
            };
            dayLabel.AddThemeFontSizeOverride("font_size", 14);
            cardContainer.AddChild(dayLabel);
            dayLabels.Add(dayLabel);

            // 按钮
            var dayButton = new Button
            {
                Text = GetRewardIcon(day),
                CustomMinimumSize = new Vector2(70, 70),
                TooltipText = GetRewardDescription(day, reward)
            };
            dayButton.Pressed += () => OnDayButtonPressed(day);
            cardContainer.AddChild(dayButton);
            dayButtons.Add(dayButton);

            // 状态标签
            var stateLabel = new Label
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                CustomMinimumSize = new Vector2(0, 20)
            };
            stateLabel.AddThemeFontSizeOverride("font_size", 12);
            cardContainer.AddChild(stateLabel);
        }

        /// <summary>
        /// 获取奖励图标
        /// </summary>
        private string GetRewardIcon(int day)
        {
            return day switch
            {
                1 => "🎁",
                2 => "💰",
                3 => "💎",
                4 => "🧪",
                5 => "📜",
                6 => "✨",
                7 => "🏆",
                _ => "📦"
            };
        }

        /// <summary>
        /// 获取奖励描述
        /// </summary>
        private string GetRewardDescription(int day, DailyLoginReward reward)
        {
            string desc = $"第{day}天奖励:\n";
            
            for (int i = 0; i < reward.ItemRewards.Count; i++)
            {
                desc += $"• {reward.ItemRewards[i]} x{reward.ItemCounts[i]}\n";
            }
            
            if (reward.ExpReward > 0)
            {
                desc += $"• 经验 +{reward.ExpReward}\n";
            }
            
            return desc;
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateDisplay()
        {
            int consecutiveDays = DailyLoginRewardSystem.Instance.GetConsecutiveLoginDays();
            int totalDays = DailyLoginRewardSystem.Instance.GetTotalLoginDays();
            int currentDay = DailyLoginRewardSystem.Instance.GetCurrentCycleDay();

            consecutiveDaysLabel.Text = $"连续登录: {consecutiveDays} 天";
            totalDaysLabel.Text = $"累计登录: {totalDays} 天";

            // 更新每个天数的状态
            for (int i = 0; i < dayButtons.Count; i++)
            {
                int day = i + 1;
                bool isClaimed = DailyLoginRewardSystem.Instance.IsDayClaimed(day);
                bool isCurrentDay = (day == currentDay);

                // 更新按钮状态
                if (isClaimed)
                {
                    dayButtons[i].Disabled = true;
                    dayLabels[i].Modulate = new Color(0.5f, 0.5f, 0.5f);
                }
                else if (isCurrentDay)
                {
                    dayButtons[i].Disabled = false;
                    dayLabels[i].Modulate = new Color(1f, 0.8f, 0.2f); // 金色高亮
                }
                else
                {
                    dayButtons[i].Disabled = true;
                    dayLabels[i].Modulate = new Color(0.5f, 0.5f, 0.5f);
                }
            }

            // 更新状态文字
            if (DailyLoginRewardSystem.Instance.HasUnclaimedReward())
            {
                statusLabel.Text = $"🎉 第{currentDay}天奖励可领取！点击领取";
                statusLabel.Modulate = new Color(1f, 0.8f, 0.2f);
            }
            else
            {
                statusLabel.Text = "✅ 今日奖励已全部领取，明天再来吧！";
                statusLabel.Modulate = new Color(0.5f, 1f, 0.5f);
            }
        }

        /// <summary>
        /// 天数按钮点击
        /// </summary>
        private void OnDayButtonPressed(int day)
        {
            if (DailyLoginRewardSystem.Instance.IsDayClaimed(day))
            {
                return;
            }

            // 尝试领取
            if (DailyLoginRewardSystem.Instance.ClaimReward(day))
            {
                // 领取成功
                UpdateDisplay();
            }
        }

        /// <summary>
        /// 登录天数更新
        /// </summary>
        private void OnLoginDaysUpdated(int consecutiveDays, int totalDays)
        {
            UpdateDisplay();
        }

        /// <summary>
        /// 奖励领取
        /// </summary>
        private void OnRewardClaimed(int day, List<string> items, List<int> counts, int gold, int exp)
        {
            string msg = $"🎁 领取成功！\n";
            for (int i = 0; i < items.Count; i++)
            {
                msg += $"• {items[i]} x{counts[i]}\n";
            }
            if (exp > 0)
            {
                msg += $"• 经验 +{exp}\n";
            }
            
            NotificationManager.Instance.ShowNotification(msg);
        }

        /// <summary>
        /// 新一天可领取
        /// </summary>
        private void OnNewDayAvailable()
        {
            if (Visible)
            {
                UpdateDisplay();
            }
        }

        /// <summary>
        /// 窗口大小改变
        /// </summary>
        private void OnWindowResized()
        {
            // 可以在这里调整UI大小
        }

        /// <summary>
        /// 显示界面
        /// </summary>
        public void Show()
        {
            UpdateDisplay();
            Visible = true;
        }

        /// <summary>
        /// 隐藏界面
        /// </summary>
        public void Hide()
        {
            Visible = false;
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
}
