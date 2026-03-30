using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Quests;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 任务追踪器 UI - 在屏幕左上角显示当前任务进度
    /// 应用数据驱动设计模式：从 insights.md 学习
    /// </summary>
    public partial class QuestTrackerUI : Control
    {
        // 节点引用
        private PanelContainer mainPanel;
        private VBoxContainer questList;
        private Label noQuestLabel;
        
        // 任务显示数据
        private List<QuestDisplayItem> questDisplays = new List<QuestDisplayItem>();
        
        // 信号系统
        public static event Action OnQuestTrackerOpened;
        public static event Action OnQuestTrackerClosed;
        
        // 数据驱动：任务数据
        private List<Quest> activeQuests = new List<Quest>();
        
        // 配置
        private int maxVisibleQuests = 5;
        private float updateInterval = 0.5f;
        private float timer = 0f;
        
        // 品质颜色
        private Color mainQuestColor = new Color(1f, 0.84f, 0f, 1f); // 金色
        private Color sideQuestColor = new Color(0.3f, 0.7f, 1f, 1f); // 蓝色
        private Color dailyQuestColor = new Color(0.5f, 1f, 0.5f, 1f); // 绿色
        private Color completedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f); // 灰色
        
        public override void _Ready()
        {
            SetupUI();
            ConnectSignals();
            Hide();
        }
        
        private void SetupUI()
        {
            // 主面板 - 左上角位置
            mainPanel = new PanelContainer();
            mainPanel.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
            mainPanel.Position = new Vector2(20, 20);
            mainPanel.CustomMinimumSize = new Vector2(300, 0);
            AddChild(mainPanel);
            
            // 样式
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0f, 0f, 0f, 0.7f);
            style.BorderColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(8);
            mainPanel.AddThemeStyleboxOverride("panel", style);
            
            // 任务列表容器
            questList = new VBoxContainer();
            questList.AddThemeConstantOverride("separation", 8);
            mainPanel.AddChild(questList);
            
            // 无任务提示
            noQuestLabel = new Label();
            noQuestLabel.Text = "当前没有任务";
            noQuestLabel.HorizontalAlignment = HorizontalAlignment.Center;
            noQuestLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f, 1f));
            questList.AddChild(noQuestLabel);
        }
        
        private void ConnectSignals()
        {
            // 连接到任务管理器信号
            QuestManager.OnQuestAccepted += OnQuestAccepted;
            QuestManager.OnQuestCompleted += OnQuestCompleted;
            QuestManager.OnQuestObjectiveUpdated += OnQuestObjectiveUpdated;
            QuestManager.OnQuestTurnedIn += OnQuestTurnedIn;
        }
        
        public override void _Process(float delta)
        {
            timer += delta;
            if (timer >= updateInterval)
            {
                timer = 0;
                RefreshQuests();
            }
        }
        
        private void RefreshQuests()
        {
            // 从任务管理器获取活跃任务
            var questManager = new QuestManager();
            var quests = questManager.GetActiveQuests();
            
            // 检查是否需要更新显示
            bool needsUpdate = false; 
            if (quests.Count != activeQuests.Count)
            {
                needsUpdate = true;
            }
            else
            {
                for (int i = 0; i < quests.Count; i++)
                {
                    if (quests[i].Id != activeQuests[i].Id)
                    {
                        needsUpdate = true;
                        break;
                    }
                    // 检查目标进度
                    foreach (var obj in quests[i].Objectives)
                    {
                        var oldObj = activeQuests[i].Objectives.Find(o => o.Description == obj.Description);
                        if (oldObj == null || oldObj.CurrentAmount != obj.CurrentAmount)
                        {
                            needsUpdate = true;
                            break;
                        }
                    }
                }
            }
            
            if (needsUpdate)
            {
                activeQuests = new List<Quest>(quests);
                UpdateQuestDisplay();
            }
        }
        
        private void UpdateQuestDisplay()
        {
            // 清除现有显示
            foreach (var display in questDisplays)
            {
                display.Container.QueueFree();
            }
            questDisplays.Clear();
            
            // 显示无任务提示
            noQuestLabel.Visible = activeQuests.Count == 0;
            
            // 显示任务（限制数量）
            int displayCount = Math.Min(activeQuests.Count, maxVisibleQuests);
            for (int i = 0; i < displayCount; i++)
            {
                var quest = activeQuests[i];
                var display = CreateQuestDisplay(quest);
                questDisplays.Add(display);
                questList.AddChild(display.Container);
            }
        }
        
        private QuestDisplayItem CreateQuestDisplay(Quest quest)
        {
            var container = new VBoxContainer();
            container.AddThemeConstantOverride("separation", 4);
            
            // 任务名称
            var nameLabel = new Label();
            nameLabel.Text = $"◆ {quest.Name}";
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            
            // 根据任务类型设置颜色
            Color questColor;
            switch (quest.Type)
            {
                case Quest.QuestType.Main:
                    questColor = mainQuestColor;
                    break;
                case Quest.QuestType.Daily:
                    questColor = dailyQuestColor;
                    break;
                default:
                    questColor = sideQuestColor;
                    break;
            }
            nameLabel.AddThemeColorOverride("font_color", questColor);
            container.AddChild(nameLabel);
            
            // 目标列表
            foreach (var objective in quest.Objectives)
            {
                var objectiveLabel = new Label();
                string statusIcon = objective.IsComplete ? "✓" : "○";
                string progress = $"{objective.CurrentAmount}/{objective.RequiredAmount}";
                objectiveLabel.Text = $"  {statusIcon} {objective.Description} {progress}";
                
                if (objective.IsComplete)
                {
                    objectiveLabel.AddThemeColorOverride("font_color", completedColor);
                }
                else
                {
                    objectiveLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f, 1f));
                }
                
                container.AddChild(objectiveLabel);
            }
            
            return new QuestDisplayItem 
            { 
                Container = container,
                QuestId = quest.Id 
            };
        }
        
        // 信号处理
        private void OnQuestAccepted(Quest quest)
        {
            RefreshQuests();
            Show();
        }
        
        private void OnQuestCompleted(Quest quest)
        {
            RefreshQuests();
        }
        
        private void OnQuestObjectiveUpdated(Quest quest, QuestObjective objective)
        {
            RefreshQuests();
        }
        
        private void OnQuestTurnedIn(Quest quest)
        {
            RefreshQuests();
            if (activeQuests.Count == 0)
            {
                // 可选：延迟隐藏
                // Hide();
            }
        }
        
        public void Toggle()
        {
            if (Visible)
            {
                Hide();
                OnQuestTrackerClosed?.Invoke();
            }
            else
            {
                Show();
                RefreshQuests();
                OnQuestTrackerOpened?.Invoke();
            }
        }
        
        public void ShowTracker()
        {
            Show();
            RefreshQuests();
            OnQuestTrackerOpened?.Invoke();
        }
        
        public void HideTracker()
        {
            Hide();
            OnQuestTrackerClosed?.Invoke();
        }
        
        // 内部类：任务显示项
        private class QuestDisplayItem
        {
            public Control Container { get; set; }
            public int QuestId { get; set; }
        }
    }
}
