using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 遗物套装UI界面
    /// </summary>
    public class RelicSetUI : Control
    {
        private Control _mainPanel;
        private VBoxContainer _mainVBox;
        private TabContainer _tabContainer;
        
        // 套装列表
        private VBoxContainer _setsListPanel;
        private ScrollContainer _setsScroll;
        
        // 当前加成
        private VBoxContainer _currentBonusesPanel;
        
        // 统计面板
        private VBoxContainer _statsPanel;
        
        // 信号
        public event Action OnClose;

        public override void _Ready()
        {
            SetupUI();
            RefreshData();
            
            // 监听套装加成变化
            RelicSetSystem.Instance.OnSetBonusChanged += RefreshData;
        }

        private void SetupUI()
        {
            // 主面板
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(800, 600);
            AddChild(_mainPanel);

            _mainVBox = new VBoxContainer();
            _mainVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _mainVBox.AddThemeConstantOverride("separation", 10);
            _mainPanel.AddChild(_mainVBox);

            // 标题栏
            var titleLabel = new Label();
            titleLabel.Text = "  🗝️ 遗物套装";
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _mainVBox.AddChild(titleLabel);

            // Tab 容器
            _tabContainer = new TabContainer();
            _tabContainer.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlags.Fill);
            _mainVBox.AddChild(_tabContainer);

            // 标签页1: 套装列表
            _setsListPanel = new VBoxContainer();
            _setsListPanel.Name = "套装";
            _tabContainer.AddChild(_setsListPanel);
            
            SetupSetsListTab();

            // 标签页2: 当前加成
            _currentBonusesPanel = new VBoxContainer();
            _currentBonusesPanel.Name = "加成";
            _tabContainer.AddChild(_currentBonusesPanel);
            
            SetupBonusesTab();

            // 标签页3: 统计
            _statsPanel = new VBoxContainer();
            _statsPanel.Name = "统计";
            _tabContainer.AddChild(_statsPanel);
            
            SetupStatsTab();

            // 快捷键提示
            var hintLabel = new Label();
            hintLabel.Text = "按 ESC 关闭";
            hintLabel.AddThemeFontSizeOverride("font_size", 14);
            hintLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
            _mainVBox.AddChild(hintLabel);
        }

        private void SetupSetsListTab()
        {
            _setsScroll = new ScrollContainer();
            _setsScroll.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlags.Fill);
            _setsScroll.SetSizeFlags(Control.SizeFlags.Fill, Control.SizeFlags.Vertical);
            _setsListPanel.AddChild(_setsScroll);

            var scrollVBox = new VBoxContainer();
            scrollVBox.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlags.Fill);
            _setsScroll.AddChild(scrollVBox);

            // 动态添加套装卡片
            var sets = RelicSetDatabase.Instance.GetAllSets();
            foreach (var set in sets)
            {
                var card = CreateSetCard(set);
                scrollVBox.AddChild(card);
            }
        }

        private Control CreateSetCard(RelicSetData.RelicSet set)
        {
            var cardPanel = new PanelContainer();
            cardPanel.CustomMinimumSize = new Vector2(0, 100);
            cardPanel.AddThemeStyleboxOverride("panel", CreateCardStyle());

            var hBox = new HBoxContainer();
            hBox.AddThemeConstantOverride("separation", 15);
            cardPanel.AddChild(hBox);

            // 图标
            var iconLabel = new Label();
            iconLabel.Text = set.Icon;
            iconLabel.AddThemeFontSizeOverride("font_size", 32);
            hBox.AddChild(iconLabel);

            // 信息
            var infoVBox = new VBoxContainer();
            infoVBox.AddThemeConstantOverride("separation", 5);
            hBox.AddChild(infoVBox);

            var nameLabel = new Label();
            nameLabel.Text = set.Name;
            nameLabel.AddThemeFontSizeOverride("font_size", 18);
            infoVBox.AddChild(nameLabel);

            var descLabel = new Label();
            descLabel.Text = set.Description;
            descLabel.Modulate = new Color(0.8f, 0.8f, 0.8f);
            descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            infoVBox.AddChild(descLabel);

            // 进度
            var progressLabel = new Label();
            var equippedCount = RelicSetSystem.Instance.GetEquippedCount(set.Id);
            progressLabel.Text = $"已装备: {equippedCount}/{set.PieceCount}";
            progressLabel.Modulate = equippedCount >= set.PieceCount ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 1f, 0.3f);
            infoVBox.AddChild(progressLabel);

            // 进度条
            var progressBar = new ProgressBar();
            progressBar.MinValue = 0;
            progressBar.MaxValue = set.PieceCount;
            progressBar.Value = equippedCount;
            progressBar.CustomMinimumSize = new Vector2(200, 10);
            infoVBox.AddChild(progressBar);

            // 状态标签
            var statusLabel = new Label();
            var unlockedSets = RelicSetSystem.Instance.GetUnlockedSets();
            if (unlockedSets.Contains(set.Id))
            {
                statusLabel.Text = "✅ 已解锁";
                statusLabel.Modulate = new Color(0.3f, 1f, 0.3f);
            }
            else
            {
                statusLabel.Text = "🔒 未解锁";
                statusLabel.Modulate = new Color(0.6f, 0.6f, 0.6f);
            }
            hBox.AddChild(statusLabel);

            return cardPanel;
        }

        private StyleBoxFlat CreateCardStyle()
        {
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.15f, 0.15f, 0.2f, 0.9f);
            style.BorderWidthLeft = 2;
            style.BorderWidthRight = 2;
            style.BorderWidthTop = 2;
            style.BorderWidthBottom = 2;
            style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
            style.CornerRadiusTopLeft = 8;
            style.CornerRadiusTopRight = 8;
            style.CornerRadiusBottomLeft = 8;
            style.CornerRadiusBottomRight = 8;
            return style;
        }

        private void SetupBonusesTab()
        {
            var scroll = new ScrollContainer();
            scroll.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlags.Fill);
            _currentBonusesPanel.AddChild(scroll);

            var contentVBox = new VBoxContainer();
            contentVBox.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlags.Fill);
            scroll.AddChild(contentVBox);

            // 标题
            var titleLabel = new Label();
            titleLabel.Text = "当前套装加成";
            titleLabel.AddThemeFontSizeOverride("font_size", 20);
            contentVBox.AddChild(titleLabel);

            // 加成列表
            var bonuses = RelicSetSystem.Instance.GetAllSetBonuses();
            
            if (bonuses.Count == 0)
            {
                var emptyLabel = new Label();
                emptyLabel.Text = "暂无激活的套装加成\n\n装备遗物来激活套装效果！";
                emptyLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
                contentVBox.AddChild(emptyLabel);
            }
            else
            {
                foreach (var bonus in bonuses)
                {
                    var bonusPanel = new HBoxContainer();
                    contentVBox.AddChild(bonusPanel);

                    var iconLabel = new Label();
                    iconLabel.Text = GetBonusIcon(bonus.Key);
                    iconLabel.AddThemeFontSizeOverride("font_size", 24);
                    bonusPanel.AddChild(iconLabel);

                    var bonusLabel = new Label();
                    bonusLabel.Text = $"{GetBonusName(bonus.Key)}: +{bonus.Value*100:F0}%";
                    bonusLabel.Modulate = new Color(0.3f, 1f, 0.3f);
                    bonusPanel.AddChild(bonusLabel);
                }
            }
        }

        private string GetBonusIcon(string bonusType)
        {
            return bonusType switch
            {
                "attack" => "⚔️",
                "defense" => "🛡️",
                "health" => "❤️",
                "speed" => "⚡",
                "crit" => "💥",
                _ => "✨"
            };
        }

        private string GetBonusName(string bonusType)
        {
            return bonusType switch
            {
                "attack" => "攻击力",
                "defense" => "防御力",
                "health" => "生命值",
                "speed" => "移动速度",
                "crit" => "暴击率",
                _ => bonusType
            };
        }

        private void SetupStatsTab()
        {
            var scroll = new ScrollContainer();
            scroll.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlags.Fill);
            _statsPanel.AddChild(scroll);

            var contentVBox = new VBoxContainer();
            contentVBox.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlags.Fill);
            scroll.AddChild(contentVBox);

            // 标题
            var titleLabel = new Label();
            titleLabel.Text = "套装统计";
            titleLabel.AddThemeFontSizeOverride("font_size", 20);
            contentVBox.AddChild(titleLabel);

            // 统计项目
            var statsGrid = new GridContainer();
            statsGrid.Columns = 2;
            statsGrid.AddThemeConstantOverride("h_separation", 20);
            statsGrid.AddThemeConstantOverride("v_separation", 10);
            contentVBox.AddChild(statsGrid);

            // 已解锁套装数
            AddStatRow(statsGrid, "已解锁套装", $"{RelicSetSystem.Instance.GetUnlockedSets().Count}/8");
            
            // 已装备遗物数
            AddStatRow(statsGrid, "已装备遗物", $"{RelicSetSystem.Instance.GetEquippedRelics().Count}");
            
            // 完成的套装数
            AddStatRow(statsGrid, "套装完成次数", $"{RelicSetSystem.Instance.GetTotalSetsCompleted()}");
        }

        private void AddStatRow(GridContainer grid, string label, string value)
        {
            var labelNode = new Label();
            labelNode.Text = label;
            labelNode.Modulate = new Color(0.8f, 0.8f, 0.8f);
            grid.AddChild(labelNode);

            var valueNode = new Label();
            valueNode.Text = value;
            valueNode.Modulate = new Color(0.3f, 1f, 0.3f);
            grid.AddChild(valueNode);
        }

        private void RefreshData()
        {
            // 刷新套装列表
            foreach (var child in _setsListPanel.GetChildren())
            {
                child.QueueFree();
            }
            SetupSetsListTab();

            // 刷新加成面板
            foreach (var child in _currentBonusesPanel.GetChildren())
            {
                child.QueueFree();
            }
            SetupBonusesTab();

            // 刷新统计面板
            foreach (var child in _statsPanel.GetChildren())
            {
                child.QueueFree();
            }
            SetupStatsTab();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
            {
                OnClose?.Invoke();
                QueueFree();
            }
        }
    }
}
