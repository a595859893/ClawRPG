using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 成就里程碑UI界面
    /// </summary>
    public class AchievementMilestoneUI : Control
    {
        private Control _mainPanel;
        private VBoxContainer _milestoneList;
        private VBoxContainer _statsPanel;
        private VBoxContainer _historyPanel;
        private TabContainer _tabContainer;
        
        // 信号
        public event Action OnClose;

        public override void _Ready()
        {
            SetupUI();
            RefreshData();
        }

        private void SetupUI()
        {
            // 主面板
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(800, 600);
            AddChild(_mainPanel);

            var mainVBox = new VBoxContainer();
            mainVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            mainVBox.AddThemeConstantOverride("separation", 10);
            _mainPanel.AddChild(mainVBox);

            // 标题栏
            var titleLabel = new Label();
            titleLabel.Text = "  🏆 成就里程碑";
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            mainVBox.AddChild(titleLabel);

            // Tab 容器
            _tabContainer = new TabContainer();
            _tabContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            mainVBox.AddChild(_tabContainer);

            // 里程碑列表页
            var milestoneTab = new ScrollContainer();
            milestoneTab.Name = "Milestones";
            _milestoneList = new VBoxContainer();
            _milestoneList.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _milestoneList.AddThemeConstantOverride("separation", 5);
            milestoneTab.AddChild(_milestoneList);
            _tabContainer.AddChild(milestoneTab);

            // 统计面板页
            var statsTab = new ScrollContainer();
            statsTab.Name = "Statistics";
            _statsPanel = new VBoxContainer();
            _statsPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _statsPanel.AddThemeConstantOverride("separation", 10);
            statsTab.AddChild(_statsPanel);
            _tabContainer.AddChild(statsTab);

            // 历史记录页
            var historyTab = new ScrollContainer();
            historyTab.Name = "History";
            _historyPanel = new VBoxContainer();
            _historyPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _historyPanel.AddThemeConstantOverride("separation", 5);
            historyTab.AddChild(_historyPanel);
            _tabContainer.AddChild(historyTab);

            // 关闭按钮
            var closeButton = new Button();
            closeButton.Text = "  关闭  ";
            closeButton.Pressed += () => OnClose?.Invoke();
            mainVBox.AddChild(closeButton);

            // 键盘关闭
            var input = new InputEventKey();
            input.Keycode = Key.Escape;
            input.Pressed = true;
        }

        private void RefreshData()
        {
            RefreshMilestones();
            RefreshStatistics();
            RefreshHistory();
        }

        private void RefreshMilestones()
        {
            // 清除现有内容
            foreach (var child in _milestoneList.GetChildren())
            {
                child.QueueFree();
            }

            var system = AchievementMilestoneSystem.Instance;
            var database = AchievementMilestoneDatabase.Instance;

            // 标题
            var headerLabel = new Label();
            headerLabel.Text = "成就里程碑进度";
            headerLabel.AddThemeFontSizeOverride("font_size", 18);
            _milestoneList.AddChild(headerLabel);

            // 遍历所有里程碑配置
            foreach (var kvp in database.Milestones)
            {
                var achievementId = kvp.Key;
                var milestones = kvp.Value;

                // 成就名称
                var nameLabel = new Label();
                nameLabel.Text = GetAchievementDisplayName(achievementId);
                nameLabel.AddThemeFontSizeOverride("font_size", 16);
                _milestoneList.AddChild(nameLabel);

                // 当前里程碑
                int currentLevel = system.GetMilestoneLevel(achievementId);
                var currentLabel = new Label();
                currentLabel.Text = $"  当前里程碑: Lv.{currentLevel}";
                currentLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.5f));
                _milestoneList.AddChild(currentLabel);

                // 进度条
                var progressBar = new ProgressBar();
                progressBar.CustomMinimumSize = new Vector2(600, 20);
                progressBar.ShowPercentage = true;
                
                if (milestones.Count > 0 && currentLevel < milestones.Count)
                {
                    progressBar.MaxValue = 100;
                    progressBar.Value = currentLevel * 100 / milestones.Count;
                }
                else
                {
                    progressBar.MaxValue = 100;
                    progressBar.Value = 100;
                }
                _milestoneList.AddChild(progressBar);

                // 里程碑详情
                foreach (var milestone in milestones)
                {
                    var detailLabel = new Label();
                    bool isReached = milestone.Level <= currentLevel;
                    string statusIcon = isReached ? "✅" : "⬜";
                    detailLabel.Text = $"  {statusIcon} Lv.{milestone.Level}: {milestone.Threshold} ({milestone.Reward})";
                    detailLabel.AddThemeColorOverride("font_color", isReached ? new Color(0.5f, 1f, 0.5f) : new Color(0.7f, 0.7f, 0.7f));
                    _milestoneList.AddChild(detailLabel);
                }

                // 分隔线
                var separator = new HSeparator();
                _milestoneList.AddChild(separator);
            }
        }

        private void RefreshStatistics()
        {
            // 清除现有内容
            foreach (var child in _statsPanel.GetChildren())
            {
                child.QueueFree();
            }

            var system = AchievementMilestoneSystem.Instance;
            var stats = system.GetStatistics();

            // 标题
            var titleLabel = new Label();
            titleLabel.Text = "里程碑统计";
            titleLabel.AddThemeFontSizeOverride("font_size", 20);
            _statsPanel.AddChild(titleLabel);

            // 总里程碑数
            var totalLabel = new Label();
            totalLabel.Text = $"总达成里程碑: {stats["total_milestones"]}";
            totalLabel.AddThemeFontSizeOverride("font_size", 16);
            _statsPanel.AddChild(totalLabel);

            // 最高等级
            var highestLabel = new Label();
            highestLabel.Text = $"最高里程碑等级: {stats["highest_level"]}";
            highestLabel.AddThemeFontSizeOverride("font_size", 16);
            _statsPanel.AddChild(highestLabel);

            // 有里程碑的成就数
            var achievementsLabel = new Label();
            achievementsLabel.Text = $"已解锁里程碑的成就数: {stats["achievements_with_milestones"]}";
            achievementsLabel.AddThemeFontSizeOverride("font_size", 16);
            _statsPanel.AddChild(achievementsLabel);

            // 历史记录数
            var historyLabel = new Label();
            historyLabel.Text = $"历史记录数: {stats["history_count"]}";
            historyLabel.AddThemeFontSizeOverride("font_size", 16);
            _statsPanel.AddChild(historyLabel);
        }

        private void RefreshHistory()
        {
            // 清除现有内容
            foreach (var child in _historyPanel.GetChildren())
            {
                child.QueueFree();
            }

            var system = AchievementMilestoneSystem.Instance;
            var history = system.GetHistory(20);

            // 标题
            var titleLabel = new Label();
            titleLabel.Text = "里程碑达成历史";
            titleLabel.AddThemeFontSizeOverride("font_size", 20);
            _historyPanel.AddChild(titleLabel);

            if (history.Count == 0)
            {
                var emptyLabel = new Label();
                emptyLabel.Text = "暂无里程碑记录";
                emptyLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
                _historyPanel.AddChild(emptyLabel);
                return;
            }

            // 历史记录列表
            foreach (var entry in history)
            {
                var entryPanel = new PanelContainer();
                entryPanel.CustomMinimumSize = new Vector2(0, 40);

                var entryHBox = new HBoxContainer();
                entryPanel.AddChild(entryHBox);

                var nameLabel = new Label();
                nameLabel.Text = entry.AchievementName;
                nameLabel.SizeFlagsHorizontal = Control.SizeFlags.Expand;
                entryHBox.AddChild(nameLabel);

                var levelLabel = new Label();
                levelLabel.Text = $"Lv.{entry.MilestoneLevel}";
                levelLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.5f));
                entryHBox.AddChild(levelLabel);

                // 时间戳
                var date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(entry.Timestamp);
                var timeLabel = new Label();
                timeLabel.Text = date.ToString("yyyy-MM-dd HH:mm");
                timeLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
                entryHBox.AddChild(timeLabel);

                _historyPanel.AddChild(entryPanel);
            }
        }

        private string GetAchievementDisplayName(string achievementId)
        {
            return achievementId switch
            {
                "kill_enemies" => "击杀敌人",
                "reach_level" => "达到等级",
                "earn_gold" => "赚取金币",
                "kill_bosses" => "击杀Boss",
                "reach_dungeon_floor" => "地下城楼层",
                "collect_pets" => "收集宠物",
                "collect_equipment" => "收集装备",
                "unlock_skill_nodes" => "解锁技能节点",
                _ => achievementId
            };
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
            {
                OnClose?.Invoke();
            }
        }
    }
}
