using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using ClawRPG.Database;
using ClawRPG.Systems;

namespace ClawRPG.UI
{
    /// <summary>
    /// 坐骑远征界面
    /// </summary>
    public class MountExpeditionUI : Control
    {
        private Control _mainPanel;
        private TabContainer _tabContainer;
        private VBoxContainer _zoneList;
        private VBoxContainer _activeList;
        private VBoxContainer _historyList;
        private Label _statsLabel;
        
        private List<MountExpeditionData.ExpeditionZone> _zones;
        
        public override void _Ready()
        {
            _zones = MountExpeditionDatabase.GetAllZones();
            SetupUI();
            RefreshAll();
            
            // 连接信号
            MountExpeditionSystem.OnExpeditionStarted += OnExpeditionStarted;
            MountExpeditionSystem.OnExpeditionCompleted += OnExpeditionCompleted;
            MountExpeditionSystem.OnExpeditionFailed += OnExpeditionFailed;
        }
        
        private void SetupUI()
        {
            // 主面板
            _mainPanel = new Control
            {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                MouseFilter = Control.MouseFilterEnum.Stop
            };
            AddChild(_mainPanel);
            
            // 背景
            var bg = new ColorRect
            {
                Color = new Color(0, 0, 0, 0.7f),
                AnchorRight = 1f,
                AnchorBottom = 1f
            };
            _mainPanel.AddChild(bg);
            
            // 容器
            var container = new VBoxContainer
            {
                AnchorLeft = 0.5f,
                AnchorRight = 0.5f,
                AnchorTop = 0.5f,
                AnchorBottom = 0.5f,
                OffsetLeft = -400,
                OffsetRight = 400,
                OffsetTop = -300,
                OffsetBottom = 300,
                GrowHorizontal = Control.GrowDirection.Center,
                GrowVertical = Control.GrowDirection.Center
            };
            _mainPanel.AddChild(container);
            
            // 标题栏
            var titleBar = new HBoxContainer { };
            container.AddChild(titleBar);
            
            var title = new Label
            {
                Text = "坐骑远征",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                CustomMinimumSize = new Vector2(0, 50)
            };
            title.AddThemeFontSizeOverride("font_size", 24);
            titleBar.AddChild(title);
            
            var closeBtn = new Button { Text = " ✕" };
            closeBtn.Pressed += () => Hide();
            titleBar.AddChild(closeBtn);
            
            // 统计标签
            _statsLabel = new Label
            {
                Text = "统计数据加载中...",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            container.AddChild(_statsLabel);
            
            // 标签页
            _tabContainer = new TabContainer
            {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            container.AddChild(_tabContainer);
            
            // 区域页
            var zoneScroll = new ScrollContainer { Name = "区域" };
            _tabContainer.AddChild(zoneScroll);
            
            _zoneList = new VBoxContainer { };
            zoneScroll.AddChild(_zoneList);
            
            // 进行中页
            var activeScroll = new ScrollContainer { Name = "进行中" };
            _tabContainer.AddChild(activeScroll);
            
            _activeList = new VBoxContainer { };
            activeScroll.AddChild(_activeList);
            
            // 历史页
            var historyScroll = new ScrollContainer { Name = "历史" };
            _tabContainer.AddChild(historyScroll);
            
            _historyList = new VBoxContainer { };
            historyScroll.AddChild(_historyList);
            
            // 入场动画
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(_mainPanel, "modulate:a", 1f, 0.3f);
            tween.TweenProperty(container, "scale", Vector2.One, 0.3f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEasing(Tween.EasingFunction.EaseOut);
            
            _mainPanel.Modulate = Colors.Transparent;
            _mainPanel.Scale = Vector2.One * 0.9f;
        }
        
        private void RefreshAll()
        {
            RefreshZoneList();
            RefreshActiveList();
            RefreshHistoryList();
            RefreshStats();
        }
        
        private void RefreshZoneList()
        {
            // 清除现有项
            foreach (var child in _zoneList.GetChildren())
                child.QueueFree();
            
            foreach (var zone in _zones)
            {
                var item = CreateZoneItem(zone);
                _zoneList.AddChild(item);
            }
        }
        
        private Control CreateZoneItem(MountExpeditionData.ExpeditionZone zone)
        {
            var panel = new PanelContainer
            {
                CustomMinimumSize = new Vector2(0, 80)
            };
            
            var hbox = new HBoxContainer { };
            panel.AddChild(hbox);
            
            // 左侧信息
            var info = new VBoxContainer { };
            info.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            hbox.AddChild(info);
            
            var nameLabel = new Label
            {
                Text = zone.Name,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 18);
            info.AddChild(nameLabel);
            
            var descLabel = new Label
            {
                Text = zone.Description,
                HorizontalAlignment = HorizontalAlignment.Left,
                AutowrapMode = TextServer.AutowrapMode.Word
            };
            descLabel.AddThemeFontSizeOverride("font_size", 12);
            descLabel.Modulate = Colors.Gray;
            info.AddChild(descLabel);
            
            var statsLabel = new Label
            {
                Text = $"推荐等级: {zone.RecommendedLevel} | 时长: {zone.DurationMinutes}分钟 | 成功率: {zone.BaseSuccessRate:P0}",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            statsLabel.AddThemeFontSizeOverride("font_size", 12);
            statsLabel.Modulate = Colors.Yellow;
            info.AddChild(statsLabel);
            
            var rewardLabel = new Label
            {
                Text = $"奖励: {zone.MinGoldReward}-{zone.MaxGoldReward}金 | {zone.MinExpReward}-{zone.MaxExpReward}经验",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            rewardLabel.AddThemeFontSizeOverride("font_size", 12);
            rewardLabel.Modulate = Colors.Green;
            info.AddChild(rewardLabel);
            
            // 开始按钮
            var startBtn = new Button { Text = "开始远征" };
            startBtn.Pressed += () => OnStartExpedition(zone);
            hbox.AddChild(startBtn);
            
            return panel;
        }
        
        private void OnStartExpedition(MountExpeditionData.ExpeditionZone zone)
        {
            // 这里应该选择坐骑，暂时使用模拟坐骑ID
            string mountId = "mount_001";
            MountExpeditionSystem.Instance.StartExpedition(zone.Id, mountId);
            RefreshAll();
        }
        
        private void RefreshActiveList()
        {
            // 清除现有项
            foreach (var child in _activeList.GetChildren())
                child.QueueFree();
            
            var progress = MountExpeditionSystem.Instance.GetExpeditionProgress();
            
            if (progress.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "没有进行中的远征",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    CustomMinimumSize = new Vector2(0, 100)
                };
                emptyLabel.AddThemeFontSizeOverride("font_size", 16);
                emptyLabel.Modulate = Colors.Gray;
                _activeList.AddChild(emptyLabel);
                return;
            }
            
            foreach (var exp in progress)
            {
                var panel = new PanelContainer
                {
                    CustomMinimumSize = new Vector2(0, 100)
                };
                
                var vbox = new VBoxContainer { };
                panel.AddChild(vbox);
                
                var nameLabel = new Label
                {
                    Text = (string)exp["zone_name"],
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                nameLabel.AddThemeFontSizeOverride("font_size", 18);
                vbox.AddChild(nameLabel);
                
                var progressBar = new ProgressBar
                {
                    Value = (float)exp["progress"] * 100,
                    MaxValue = 100,
                    CustomMinimumSize = new Vector2(0, 20)
                };
                progressBar.ShowPercentage = false; 
                vbox.AddChild(progressBar);
                
                var statusLabel = new Label();
                if ((bool)exp["completed"])
                {
                    if (!(bool)exp["claimed"])
                    {
                        statusLabel.Text = "远征完成！可以领取奖励";
                        statusLabel.Modulate = Colors.Green;
                        
                        var claimBtn = new Button { Text = "领取奖励" };
                        claimBtn.Pressed += () =>
                        {
                            MountExpeditionSystem.Instance.ClaimReward((string)exp["expedition_id"]);
                            RefreshAll();
                        };
                        vbox.AddChild(claimBtn);
                    }
                    else
                    {
                        statusLabel.Text = "奖励已领取";
                        statusLabel.Modulate = Colors.Gray;
                    }
                }
                else
                {
                    statusLabel.Text = $"剩余时间: {(int)exp["remaining_minutes"]} 分钟";
                    statusLabel.Modulate = Colors.Yellow;
                    
                    var cancelBtn = new Button { Text = "取消" };
                    cancelBtn.Pressed += () =>
                    {
                        MountExpeditionSystem.Instance.CancelExpedition((string)exp["expedition_id"]);
                        RefreshAll();
                    };
                    vbox.AddChild(cancelBtn);
                }
                vbox.AddChild(statusLabel);
                
                _activeList.AddChild(panel);
            }
        }
        
        private void RefreshHistoryList()
        {
            // 清除现有项
            foreach (var child in _historyList.GetChildren())
                child.QueueFree();
            
            var stats = MountExpeditionSystem.Instance.GetStatistics();
            int historyCount = (int)stats["history_count"];
            
            if (historyCount == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "暂无远征历史",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    CustomMinimumSize = new Vector2(0, 100)
                };
                emptyLabel.AddThemeFontSizeOverride("font_size", 16);
                emptyLabel.Modulate = Colors.Gray;
                _historyList.AddChild(emptyLabel);
                return;
            }
            
            // 显示最近10条记录
            var history = MountExpeditionSystem.Instance.GetStatistics();
            for (int i = 0; i < Math.Min(10, historyCount); i++)
            {
                var panel = new PanelContainer
                {
                    CustomMinimumSize = new Vector2(0, 60)
                };
                
                var label = new Label
                {
                    Text = $"远征记录 #{i + 1}",
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                label.AddThemeFontSizeOverride("font_size", 14);
                panel.AddChild(label);
                
                _historyList.AddChild(panel);
            }
        }
        
        private void RefreshStats()
        {
            var stats = MountExpeditionSystem.Instance.GetStatistics();
            _statsLabel.Text = $"总远征次数: {stats["total_expeditions"]} | " +
                $"总获得金币: {stats["total_gold_earned"]} | " +
                $"总获得经验: {stats["total_exp_earned"]}";
        }
        
        private void OnExpeditionStarted()
        {
            RefreshAll();
        }
        
        private void OnExpeditionCompleted()
        {
            RefreshAll();
        }
        
        private void OnExpeditionFailed()
        {
            RefreshAll();
        }
        
        public void Show()
        {
            Visible = true;
            RefreshAll();
            
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(_mainPanel, "modulate:a", 1f, 0.3f);
            tween.TweenProperty(_mainPanel.Scale, Vector2.One, 0.3f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEasing(Tween.EasingFunction.EaseOut);
        }
        
        public void Hide()
        {
            var tween = CreateTween();
            tween.TweenProperty(_mainPanel, "modulate:a", 0f, 0.2f);
            tween.TweenProperty(_mainPanel.Scale, Vector2.One * 0.95f, 0.2f);
            tween.TweenCallback(Callable.From(() => Visible = false));
        }
        
        public void Toggle()
        {
            if (Visible)
                Hide();
            else
                Show();
        }
    }
}
