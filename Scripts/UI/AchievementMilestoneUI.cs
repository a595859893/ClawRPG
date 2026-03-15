using System;
using System.Collections.Generic;
using Godot;
using Godot.Controls;
using Godot.Collections;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// 成就里程碑系统 - UI
    /// </summary>
    public partial class AchievementMilestoneUI : Control
    {
        private AchievementMilestoneSystem _system;
        
        // 主容器
        private VBoxContainer _mainContainer;
        private TabContainer _tabContainer;
        
        // 成就标签页
        private VBoxContainer _achievementsTab;
        private ItemList _achievementList;
        private Label _achievementDetail;
        
        // 里程碑标签页
        private VBoxContainer _milestonesTab;
        private ItemList _milestoneList;
        private Label _milestoneDetail;
        
        // 统计标签页
        private VBoxContainer _statisticsTab;
        private Label _statsLabel;
        
        // 进度标签页
        private VBoxContainer _progressTab;
        private ProgressBar _overallProgress;
        private Label _progressLabel;
        
        // 当前选中
        private string _selectedAchievementId = "";
        private string _selectedMilestoneId = "";
        
        public override void _Ready()
        {
            _system = AchievementMilestoneSystem.Instance;
            
            SetupUI();
            ConnectSignals();
            RefreshAchievementList();
            RefreshMilestoneList();
            RefreshStatistics();
            RefreshProgress();
        }
        
        private void SetupUI()
        {
            // 主窗口设置
            WindowTitle = "成就与里程碑";
            Size = new Vector2(800, 600);
            Resizable = true;
            
            // 主容器
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _mainContainer.AddThemeConstantOverride("separation", 10);
            AddChild(_mainContainer);
            
            // 顶部进度条
            SetupProgressHeader();
            
            // 标签容器
            _tabContainer = new TabContainer();
            _tabContainer.SetSizeFlags(Control.SizeFlags.ExpandFill, Control.SizeFlags.Fill);
            _mainContainer.AddChild(_tabContainer);
            
            // 成就标签页
            SetupAchievementsTab();
            
            // 里程碑标签页
            SetupMilestonesTab();
            
            // 统计标签页
            SetupStatisticsTab();
            
            // 进度标签页
            SetupProgressTab();
        }
        
        private void SetupProgressHeader()
        {
            var headerContainer = new HBoxContainer();
            headerContainer.SetSizeFlags(Control.SizeFlags.ExpandFill, Control.SizeFlags.Fill);
            _mainContainer.AddChild(headerContainer);
            
            var titleLabel = new Label();
            titleLabel.Text = "🎯 成就与里程碑";
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            headerContainer.AddChild(titleLabel);
            
            headerContainer.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });
            
            _overallProgress = new ProgressBar();
            _overallProgress.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _overallProgress.CustomMinimumSize = new Vector2(200, 30);
            headerContainer.AddChild(_overallProgress);
            
            _progressLabel = new Label();
            _progressLabel.Text = "0%";
            _progressLabel.CustomMinimumSize = new Vector2(60, 0);
            headerContainer.AddChild(_progressLabel);
        }
        
        private void SetupAchievementsTab()
        {
            _achievementsTab = new VBoxContainer();
            _achievementsTab.SetTabTitle(0, "成就");
            _tabContainer.AddChild(_achievementsTab);
            
            var hbox = new HBoxContainer();
            hbox.SetSizeFlags(Control.SizeFlags.ExpandFill, Control.SizeFlags.Fill);
            _achievementsTab.AddChild(hbox);
            
            // 成就列表
            _achievementList = new ItemList();
            _achievementList.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _achievementList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _achievementList.CustomMinimumSize = new Vector2(300, 0);
            hbox.AddChild(_achievementList);
            
            // 成就详情
            _achievementDetail = new Label();
            _achievementDetail.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _achievementDetail.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _achievementDetail.Text = "选择一个成就查看详情";
            hbox.AddChild(_achievementDetail);
        }
        
        private void SetupMilestonesTab()
        {
            _milestonesTab = new VBoxContainer();
            _milestonesTab.SetTabTitle(1, "里程碑");
            _tabContainer.AddChild(_milestonesTab);
            
            var hbox = new HBoxContainer();
            hbox.SetSizeFlags(Control.SizeFlags.ExpandFill, Control.SizeFlags.Fill);
            _milestonesTab.AddChild(hbox);
            
            // 里程碑列表
            _milestoneList = new ItemList();
            _milestoneList.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _milestoneList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _milestoneList.CustomMinimumSize = new Vector2(300, 0);
            hbox.AddChild(_milestoneList);
            
            // 里程碑详情
            _milestoneDetail = new Label();
            _milestoneDetail.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _milestoneDetail.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _milestoneDetail.Text = "选择一个里程碑查看详情";
            hbox.AddChild(_milestoneDetail);
        }
        
        private void SetupStatisticsTab()
        {
            _statisticsTab = new VBoxContainer();
            _statisticsTab.SetTabTitle(2, "统计");
            _tabContainer.AddChild(_statisticsTab);
            
            var scroll = new ScrollContainer();
            scroll.SetSizeFlags(Control.SizeFlags.ExpandFill, Control.SizeFlags.Fill);
            _statisticsTab.AddChild(scroll);
            
            _statsLabel = new Label();
            _statsLabel.Text = "加载中...";
            scroll.AddChild(_statsLabel);
        }
        
        private void SetupProgressTab()
        {
            _progressTab = new VBoxContainer();
            _progressTab.SetTabTitle(3, "进度总览");
            _tabContainer.AddChild(_progressTab);
            
            var centerContainer = new VBoxContainer();
            centerContainer.Alignment = BoxContainer.AlignmentMode.Center;
            centerContainer.SetSizeFlags(Control.SizeFlags.Center, Control.SizeFlags.Fill);
            _progressTab.AddChild(centerContainer);
            
            var overallLabel = new Label();
            overallLabel.Text = "总体进度";
            overallLabel.AddThemeFontSizeOverride("font_size", 20);
            centerContainer.AddChild(overallLabel);
            
            var bigProgress = new ProgressBar();
            bigProgress.CustomMinimumSize = new Vector2(400, 50);
            bigProgress.SizeFlagsHorizontal = Control.SizeFlags.Center;
            centerContainer.AddChild(bigProgress);
            
            // 分类进度
            var categoryLabel = new Label();
            categoryLabel.Text = "\n分类进度";
            categoryLabel.AddThemeFontSizeOverride("font_size", 18);
            centerContainer.AddChild(categoryLabel);
            
            var categories = new[] { "战斗", "探索", "制作", "收集", "社交", "进度", "挑战", "特殊" };
            foreach (var cat in categories)
            {
                var catLabel = new Label();
                catLabel.Text = cat + ": 0/0";
                centerContainer.AddChild(catLabel);
            }
        }
        
        private void ConnectSignals()
        {
            _achievementList.ItemSelected += OnAchievementSelected;
            _milestoneList.ItemSelected += OnMilestoneSelected;
            
            if (_system != null)
            {
                _system.AchievementUnlocked += OnAchievementUnlocked;
                _system.MilestoneCompleted += OnMilestoneCompleted;
                _system.RewardClaimed += OnRewardClaimed;
            }
        }
        
        private void RefreshAchievementList()
        {
            _achievementList.Clear();
            
            var achievements = _system.GetAllAchievements();
            foreach (var ach in achievements.Values)
            {
                string displayText = GetAchievementDisplayText(ach);
                _achievementList.AddItem(displayText);
            }
        }
        
        private string GetAchievementDisplayText(Achievement ach)
        {
            string status = ach.IsUnlocked ? "✓" : " ";
            string rarity = GetRarityEmoji(ach.Rarity);
            return $"{status} {rarity} {ach.Name} ({ach.CurrentValue}/{ach.RequiredValue})";
        }
        
        private string GetRarityEmoji(AchievementRarity rarity)
        {
            return rarity switch
            {
                AchievementRarity.Common => "⚪",
                AchievementRarity.Uncommon => "🟢",
                AchievementRarity.Rare => "🔵",
                AchievementRarity.Epic => "🟣",
                AchievementRarity.Legendary => "🟡",
                _ => "⚪"
            };
        }
        
        private void RefreshMilestoneList()
        {
            _milestoneList.Clear();
            
            var milestones = _system.GetAllMilestones();
            foreach (var ms in milestones.Values)
            {
                string status = ms.IsCompleted ? "✓" : " ";
                string displayText = $"{status} {ms.Name} ({ms.CurrentCount}/{ms.RequiredCount})";
                _milestoneList.AddItem(displayText);
            }
        }
        
        private void RefreshStatistics()
        {
            if (_system == null) return;
            
            var stats = _system.GetStatistics();
            var playerData = _system.GetPlayerData();
            
            string text = "=== 成就统计 ===\n\n";
            text += $"总成就数: {playerData.TotalAchievements}\n";
            text += $"已解锁: {playerData.UnlockedAchievements}\n";
            text += $"已领取奖励: {playerData.ClaimedRewards}\n\n";
            
            text += "=== 里程碑统计 ===\n\n";
            text += $"总里程碑数: {playerData.TotalMilestones}\n";
            text += $"已完成: {playerData.CompletedMilestones}\n\n";
            
            text += "=== 奖励统计 ===\n\n";
            text += $"总奖励: {stats.TotalRewardsClaimed}\n";
            text += $"金币: {stats.GoldEarnedFromRewards}\n";
            text += $"经验: {stats.ExpEarnedFromRewards}\n";
            
            _statsLabel.Text = text;
        }
        
        private void RefreshProgress()
        {
            if (_system == null) return;
            
            float progress = _system.GetOverallProgress() * 100;
            _overallProgress.Value = progress;
            _progressLabel.Text = $"{progress:F1}%";
        }
        
        private void OnAchievementSelected(long index)
        {
            var achievements = _system.GetAllAchievements();
            int i = 0;
            foreach (var ach in achievements.Values)
            {
                if (i == index)
                {
                    _selectedAchievementId = ach.ID;
                    ShowAchievementDetail(ach);
                    break;
                }
                i++;
            }
        }
        
        private void ShowAchievementDetail(Achievement ach)
        {
            string text = $"=== {ach.Name} ===\n\n";
            text += $"描述: {ach.Description}\n";
            text += $"类型: {ach.Type}\n";
            text += $"稀有度: {ach.Rarity}\n";
            text += $"进度: {ach.CurrentValue}/{ach.RequiredValue} ({ach.Progress * 100:F1}%)\n";
            text += $"状态: {(ach.IsUnlocked ? "已解锁" : "未解锁")}\n";
            
            if (ach.IsUnlocked && ach.UnlockedTime.HasValue)
            {
                text += $"解锁时间: {ach.UnlockedTime.Value}\n";
            }
            
            text += $"\n奖励: ";
            foreach (var rewardId in ach.Rewards)
            {
                text += rewardId + " ";
            }
            
            if (ach.IsUnlocked && !ach.RewardsClaimed)
            {
                text += "\n\n[可领取奖励!]";
            }
            else if (ach.RewardsClaimed)
            {
                text += "\n\n[奖励已领取]";
            }
            
            _achievementDetail.Text = text;
        }
        
        private void OnMilestoneSelected(long index)
        {
            var milestones = _system.GetAllMilestones();
            int i = 0;
            foreach (var ms in milestones.Values)
            {
                if (i == index)
                {
                    _selectedMilestoneId = ms.ID;
                    ShowMilestoneDetail(ms);
                    break;
                }
                i++;
            }
        }
        
        private void ShowMilestoneDetail(Milestone ms)
        {
            string text = $"=== {ms.Name} ===\n\n";
            text += $"描述: {ms.Description}\n";
            text += $"进度: {ms.CurrentCount}/{ms.RequiredCount} ({ms.Progress * 100:F1}%)\n";
            text += $"状态: {(ms.IsCompleted ? "已完成" : "进行中")}\n";
            
            if (ms.IsCompleted && ms.CompletedTime.HasValue)
            {
                text += $"完成时间: {ms.CompletedTime.Value}\n";
            }
            
            text += $"\n奖励: ";
            foreach (var rewardId in ms.Rewards)
            {
                text += rewardId + " ";
            }
            
            if (ms.IsCompleted && !ms.RewardsClaimed)
            {
                text += "\n\n[可领取奖励!]";
            }
            else if (ms.RewardsClaimed)
            {
                text += "\n\n[奖励已领取]";
            }
            
            _milestoneDetail.Text = text;
        }
        
        private void OnAchievementUnlocked(string achievementId, Achievement achievement)
        {
            RefreshAchievementList();
            RefreshStatistics();
            RefreshProgress();
        }
        
        private void OnMilestoneCompleted(string milestoneId, Milestone milestone)
        {
            RefreshMilestoneList();
            RefreshStatistics();
            RefreshProgress();
        }
        
        private void OnRewardClaimed(string rewardId, int gold, int exp)
        {
            RefreshAchievementList();
            RefreshMilestoneList();
            RefreshStatistics();
            
            // 显示奖励通知
            GD.Print($"领取奖励: {rewardId}, 金币: {gold}, 经验: {exp}");
        }
        
        public void ToggleVisibility()
        {
            Visible = !Visible;
        }
    }
}
