using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;
    using Title = ClawRPG.Scripts.Systems.Title;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 称号系统 UI
    /// </summary>
    public partial class TitleUI : Control {
        private Control container;
        private VBoxContainer titleListContainer;
        private Label currentTitleLabel;
        private Label titleCountLabel;
        
        // 按钮
        private Button closeButton;
        private Button allButton;
        private Button levelButton;
        private Button combatButton;
        private Button questButton;
        private Button collectionButton;
        private Button specialButton;
        
        // 当前筛选类型
        private TitleCategory? currentFilter = null;
        
        // 预设颜色
        private Color commonColor = new Color(0.7f, 0.7f, 0.7f);
        private Color uncommonColor = new Color(0.2f, 0.8f, 0.2f);
        private Color rareColor = new Color(0.2f, 0.5f, 1.0f);
        private Color epicColor = new Color(0.6f, 0.3f, 0.9f);
        private Color legendaryColor = new Color(1.0f, 0.6f, 0.0f);
        
        public override void _Ready() {
            InitializeUI();
            PopulateTitleList();
            Hide();
        }
        
        private void InitializeUI() {
            // 主容器
            container = new Control();
            container.SetAnchorsAndMarginsPreset(Control.Preset.FullRect);
            AddChild(container);
            
            // 背景面板
            Panel backgroundPanel = new Panel();
            backgroundPanel.SetAnchorsAndMarginsPreset(Control.Preset.FullRect);
            backgroundPanel.Modulate = new Color(0, 0, 0, 0.7f);
            container.AddChild(backgroundPanel);
            
            // 标题
            Label titleLabel = new Label();
            titleLabel.Text = "  称号系统";
            titleLabel.AddThemeFontSizeOverride("font_size", 28);
            titleLabel.SetAnchorsPreset(Control.Preset.TopWide);
            titleLabel.SetMargins(0, 20, 0, 0);
            container.AddChild(titleLabel);
            
            // 关闭按钮
            closeButton = new Button();
            closeButton.Text = "✕";
            closeButton.SetAnchorsPreset(Control.Preset.TopRight);
            closeButton.SetMargins(-60, 20, -20, 50);
            closeButton.Pressed += () => Hide();
            container.AddChild(closeButton);
            
            // 当前称号显示
            Panel currentPanel = new Panel();
            currentPanel.SetAnchorsPreset(Control.Preset.TopWide);
            currentPanel.SetMargins(40, 70, -40, 120);
            container.AddChild(currentPanel);
            
            VBoxContainer currentVBox = new VBoxContainer();
            currentVBox.SetAnchorsAndMarginsPreset(Control.Preset.FullRect);
            currentVBox.AddThemeConstantOverride("separation", 10);
            currentPanel.AddChild(currentVBox);
            
            Label currentTitleTitle = new Label();
            currentTitleTitle.Text = "当前称号:";
            currentTitleTitle.AddThemeFontSizeOverride("font_size", 18);
            currentVBox.AddChild(currentTitleTitle);
            
            currentTitleLabel = new Label();
            currentTitleLabel.Text = "暂无称号";
            currentTitleLabel.AddThemeFontSizeOverride("font_size", 24);
            currentVBox.AddChild(currentTitleLabel);
            
            // 称号计数
            titleCountLabel = new Label();
            titleCountLabel.Text = "已解锁: 0/0";
            titleCountLabel.AddThemeFontSizeOverride("font_size", 16);
            titleCountLabel.Modulate = new Color(0.8f, 0.8f, 0.8f);
            currentVBox.AddChild(titleCountLabel);
            
            // 筛选按钮容器
            HBoxContainer filterContainer = new HBoxContainer();
            filterContainer.SetAnchorsPreset(Control.Preset.TopWide);
            filterContainer.SetMargins(40, 165, -40, 0);
            filterContainer.AddThemeConstantOverride("separation", 10);
            container.AddChild(filterContainer);
            
            // 筛选按钮
            allButton = CreateFilterButton("全部", filterContainer);
            allButton.Pressed += () => SetFilter(null);
            
            levelButton = CreateFilterButton("等级", filterContainer);
            levelButton.Pressed += () => SetFilter(TitleCategory.Combat);
            
            combatButton = CreateFilterButton("战斗", filterContainer);
            combatButton.Pressed += () => SetFilter(TitleCategory.Combat);
            
            questButton = CreateFilterButton("任务", filterContainer);
            questButton.Pressed += () => SetFilter(TitleCategory.Exploration);
            
            collectionButton = CreateFilterButton("收集", filterContainer);
            collectionButton.Pressed += () => SetFilter(TitleCategory.Collection);
            
            specialButton = CreateFilterButton("特殊", filterContainer);
            specialButton.Pressed += () => SetFilter(TitleCategory.Special);
            
            // 称号列表容器 (使用 ScrollContainer)
            ScrollContainer scrollContainer = new ScrollContainer();
            scrollContainer.SetAnchorsPreset(Control.Preset.BottomWide);
            scrollContainer.SetMargins(40, 210, -40, 40);
            container.AddChild(scrollContainer);
            
            titleListContainer = new VBoxContainer();
            titleListContainer.SetAnchorsAndMarginsPreset(Control.Preset.FullRect);
            titleListContainer.AddThemeConstantOverride("separation", 8);
            scrollContainer.AddChild(titleListContainer);
            
            // 设置 ScrollContainer 的子节点
            scrollContainer.AddChild(titleListContainer);
            scrollContainer.SetHScrollEnabled(false);
        }
        
        private Button CreateFilterButton(string text, HBoxContainer parent) {
            Button button = new Button();
            button.Text = text;
            button.CustomMinimumSize = new Vector2(80, 35);
            parent.AddChild(button);
            return button;
        }
        
        private void SetFilter(TitleCategory? type) {
            currentFilter = type;
            
            // 更新按钮状态
            allButton.Modulate = type == null ? Colors.Yellow : Colors.White;
            levelButton.Modulate = type == TitleCategory.Combat ? Colors.Yellow : Colors.White;
            combatButton.Modulate = type == TitleCategory.Combat ? Colors.Yellow : Colors.White;
            questButton.Modulate = type == TitleCategory.Exploration ? Colors.Yellow : Colors.White;
            collectionButton.Modulate = type == TitleCategory.Collection ? Colors.Yellow : Colors.White;
            specialButton.Modulate = type == TitleCategory.Special ? Colors.Yellow : Colors.White;
            
            PopulateTitleList();
        }
        
        private void PopulateTitleList() {
            // 清除现有列表
            foreach (Node child in titleListContainer.GetChildren()) {
                child.QueueFree();
            }
            
            var titleSystem = TitleSystem.Instance;
            var allTitleData = titleSystem.GetAllTitles();
            var allTitles = allTitleData.ConvertAll(t => new Title(t));
            var unlockedTitles = titleSystem.GetUnlockedTitles();
            
            int totalCount = allTitles.Count;
            int unlockedCount = unlockedTitles.Count;
            
            // 更新计数
            titleCountLabel.Text = $"已解锁: {unlockedCount}/{totalCount}";
            
            // 更新当前称号显示
            string currentTitleName = titleSystem.GetEquippedTitleName();
            if (!string.IsNullOrEmpty(currentTitleName)) {
                currentTitleLabel.Text = currentTitleName;
                currentTitleLabel.Modulate = titleSystem.GetCurrentTitleColor();
            } else {
                currentTitleLabel.Text = "暂无称号";
                currentTitleLabel.Modulate = Colors.Gray;
            }
            
            // 筛选并显示称号
            List<Title> filteredTitles = new List<Title>();
            if (currentFilter.HasValue) {
                var raw = titleSystem.GetTitlesByType(currentFilter.Value);
                filteredTitles = raw.ConvertAll(t => new Title(t));
            } else {
                filteredTitles = allTitles.ConvertAll(t => new Title(t));
            }
            
            foreach (var title in filteredTitles) {
                Control titleItem = CreateTitleItem(title);
                titleListContainer.AddChild(titleItem);
            }
        }
        
        private Control CreateTitleItem(Title title) {
            var titleSystem = TitleSystem.Instance;
            
            // 称号项目容器
            Panel itemPanel = new Panel();
            itemPanel.CustomMinimumSize = new Vector2(0, 60);
            
            HBoxContainer hbox = new HBoxContainer();
            hbox.SetAnchorsAndMarginsPreset(Control.Preset.FullRect);
            hbox.AddThemeConstantOverride("separation", 15);
            itemPanel.AddChild(hbox);
            
            // 锁定/解锁图标
            Label statusIcon = new Label();
            statusIcon.Text = title.IsUnlocked ? "✓" : "✗";
            statusIcon.AddThemeFontSizeOverride("font_size", 20);
            statusIcon.Modulate = title.IsUnlocked ? Colors.Green : Colors.Gray;
            statusIcon.SetAnchorsPreset(Control.Preset.LeftWide);
            statusIcon.SetMargins(15, 0, 0, 0);
            hbox.AddChild(statusIcon);
            
            // 称号信息
            VBoxContainer infoVBox = new VBoxContainer();
            infoVBox.AddThemeConstantOverride("separation", 5);
            hbox.AddChild(infoVBox);
            
            // 称号名称
            Label nameLabel = new Label();
            nameLabel.Text = title.Name;
            nameLabel.AddThemeFontSizeOverride("font_size", 18);
            nameLabel.Modulate = title.IsUnlocked ? titleSystem.GetRarityColor(title.Rarity) : Colors.Gray;
            infoVBox.AddChild(nameLabel);
            
            // 称号描述
            Label descLabel = new Label();
            descLabel.Text = title.Description;
            descLabel.AddThemeFontSizeOverride("font_size", 14);
            descLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
            infoVBox.AddChild(descLabel);
            
            // 稀有度标签
            Label rarityLabel = new Label();
            rarityLabel.Text = GetRarityName(title.Rarity);
            rarityLabel.AddThemeFontSizeOverride("font_size", 12);
            rarityLabel.Modulate = title.IsUnlocked ? titleSystem.GetRarityColor(title.Rarity) : Colors.Gray;
            rarityLabel.SetAnchorsPreset(Control.Preset.RightWide);
            rarityLabel.SetMargins(0, 0, 15, 0);
            hbox.AddChild(rarityLabel);
            
            // 如果未解锁，显示选择按钮
            if (!title.IsUnlocked) {
                // 显示进度
                // 这里可以添加进度条显示
            } else {
                // 如果已解锁且不是当前称号，显示设置按钮
                if (TitleSystem.Instance.CurrentTitleId != title.Id) {
                    Button setButton = new Button();
                    setButton.Text = "设为称号";
                    setButton.SetAnchorsPreset(Control.Preset.RightWide);
                    setButton.SetMargins(0, 0, 15, 0);
                    setButton.Pressed += () => {
                        TitleSystem.Instance.SetCurrentTitle(title.Id);
                        PopulateTitleList();
                    };
                    hbox.AddChild(setButton);
                }
            }
            
            return itemPanel;
        }
        
        private string GetRarityName(TitleRarity rarity) {
            switch (rarity) {
                case TitleRarity.Common: return "普通";
                case TitleRarity.Uncommon: return "优秀";
                case TitleRarity.Rare: return "稀有";
                case TitleRarity.Epic: return "史诗";
                case TitleRarity.Legendary: return "传说";
                default: return "";
            }
        }
        
        public override void _Input(InputEvent @event) {
            if (@event.IsActionPressed("ui_cancel")) {
                Hide();
            }
        }
    }
}
