using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Statistics UI - displays player game statistics
    /// Press Z to open/close
    /// </summary>
    public partial class StatisticsUI : Control
    {
        private Panel _mainPanel;
        private Label _titleLabel;
        private VBoxContainer _statsContainer;
        private ScrollContainer _scrollContainer;
        private Button _closeButton;
        private Button _resetButton;
        
        private bool _isVisible = false;
        
        public override void _Ready()
        {
            CreateUI();
            Visible = false;
            
            // Subscribe to statistics updates
            StatisticsManager.Instance.OnStatisticsUpdated += UpdateDisplay;
            
            // Update display on open
            UpdateDisplay();
        }
        
        private void CreateUI()
        {
            // Main panel
            _mainPanel = new Panel
            {
                Name = "MainPanel",
                CustomMinimumSize = new Vector2(500, 600),
                AnchorsPreset = Control.LayoutPreset.Center
            };
            AddChild(_mainPanel);
            
            var panelStyle = new StyleBoxFlat();
            panelStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            panelStyle.BorderColor = new Color(0.3f, 0.3f, 0.4f);
            panelStyle.SetBorderWidthAll(2);
            panelStyle.SetCornerRadiusAll(8);
            _mainPanel.AddThemeStyleboxOverride("panel", panelStyle);
            
            // Title
            _titleLabel = new Label
            {
                Text = "📊 玩家统计",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Position = new Vector2(0, 10),
                Size = new Vector2(500, 40)
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.4f));
            _mainPanel.AddChild(_titleLabel);
            
            // Close button
            _closeButton = new Button
            {
                Text = "✕",
                Position = new Vector2(460, 10),
                Size = new Vector2(30, 30)
            };
            _closeButton.Pressed += () => ToggleVisibility();
            _mainPanel.AddChild(_closeButton);
            
            // Reset button
            _resetButton = new Button
            {
                Text = "重置统计",
                Position = new Vector2(180, 560),
                Size = new Vector2(140, 30)
            };
            _resetButton.Pressed += OnResetPressed;
            _mainPanel.AddChild(_resetButton);
            
            // Scroll container
            _scrollContainer = new ScrollContainer
            {
                Position = new Vector2(10, 60),
                Size = new Vector2(480, 490)
            };
            _mainPanel.AddChild(_scrollContainer);
            
            // Stats container
            _statsContainer = new VBoxContainer
            {
                Size = new Vector2(480, 490)
            };
            _statsContainer.AddThemeConstantOverride("separation", 8);
            _scrollContainer.AddChild(_statsContainer);
        }
        
        public override void _Input(InputEvent evt)
        {
            if (evt is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Z)
            {
                ToggleVisibility();
            }
        }
        
        private void ToggleVisibility()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible)
            {
                UpdateDisplay();
            }
        }
        
        private void UpdateDisplay()
        {
            if (!IsInstanceValid(_statsContainer)) return;
            
            // Clear existing
            foreach (Node child in _statsContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            var stats = StatisticsManager.Instance.Stats;
            
            // Combat section
            AddSectionHeader("⚔️ 战斗统计");
            AddStatRow("击杀总数", stats.TotalKills.ToString());
            AddStatRow("死亡次数", stats.TotalDeaths.ToString());
            AddStatRow("造成伤害", stats.TotalDamageDealt.ToString("N0"));
            AddStatRow("承受伤害", stats.TotalDamageTaken.ToString("N0"));
            AddStatRow("治疗量", stats.TotalHealing.ToString("N0"));
            AddStatRow("暴击次数", stats.CriticalHits.ToString());
            AddStatRow("完美格挡", stats.PerfectBlocks.ToString());
            AddStatRow("闪避次数", stats.Dodges.ToString());
            
            // Resources section
            AddSectionHeader("💰 资源统计");
            AddStatRow("获得金币", stats.GoldEarned.ToString("N0"));
            AddStatRow("消费金币", stats.GoldSpent.ToString("N0"));
            AddStatRow("获得经验", stats.ExperienceGained.ToString("N0"));
            AddStatRow("收集物品", stats.ItemsCollected.ToString());
            AddStatRow("合成物品", stats.ItemsCrafted.ToString());
            
            // Quest section
            AddSectionHeader("📜 任务统计");
            AddStatRow("完成任务", stats.QuestsCompleted.ToString());
            AddStatRow("放弃任务", stats.QuestsAbandoned.ToString());
            
            // Skills section
            AddSectionHeader("✨ 技能统计");
            AddStatRow("学习技能", stats.SkillsLearned.ToString());
            AddStatRow("使用技能", stats.SkillsUsed.ToString());
            
            // Exploration section
            AddSectionHeader("🗺️ 探索统计");
            AddStatRow("发现区域", stats.RegionsDiscovered.ToString());
            AddStatRow("遭遇敌人", stats.EnemiesEncountered.ToString());
            AddStatRow("击败Boss", stats.BossesDefeated.ToString());
            
            // Progression section
            AddSectionHeader("📈 成长统计");
            AddStatRow("最高等级", "Lv." + stats.HighestLevel);
            AddStatRow("最高连击", stats.HighestCombo.ToString() + "连击");
            AddStatRow("解锁成就", stats.AchievementsUnlocked.ToString());
            
            // Time section
            AddSectionHeader("⏱️ 游戏时间");
            int hours = (int)(stats.TotalPlayTime / 3600);
            int minutes = (int)((stats.TotalPlayTime % 3600) / 60);
            int seconds = (int)(stats.TotalPlayTime % 60);
            string playTime = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            AddStatRow("总游戏时间", playTime);
            
            // Kill/Death ratio
            if (stats.TotalDeaths > 0)
            {
                float ratio = (float)stats.TotalKills / stats.TotalDeaths;
                AddStatRow("击杀/死亡比", ratio.ToString("F2"));
            }
            else
            {
                AddStatRow("击杀/死亡比", stats.TotalKills.ToString());
            }
        }
        
        private void AddSectionHeader(string text)
        {
            var label = new Label
            {
                Text = text,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            label.AddThemeFontSizeOverride("font_size", 18);
            label.AddThemeColorOverride("font_color", new Color(0.9f, 0.75f, 0.4f));
            label.AddThemeConstantOverride("offset_top", 10);
            label.AddThemeConstantOverride("offset_bottom", 5);
            _statsContainer.AddChild(label);
            
            // Separator line
            var hSeparator = new HSeparator();
            hSeparator.AddThemeColorOverride("separator", new Color(0.3f, 0.3f, 0.35f));
            _statsContainer.AddChild(hSeparator);
        }
        
        private void AddStatRow(string label, string value)
        {
            var container = new HBoxContainer();
            container.AddThemeConstantOverride("separation", 10);
            _statsContainer.AddChild(container);
            
            var labelControl = new Label
            {
                Text = label + ":",
                HorizontalAlignment = HorizontalAlignment.Left,
                SizeFlagsHorizontal = SizeFlags.Expand
            };
            labelControl.AddThemeColorOverride("font_color", new Color(0.85f, 0.85f, 0.9f));
            container.AddChild(labelControl);
            
            var valueControl = new Label
            {
                Text = value,
                HorizontalAlignment = HorizontalAlignment.Right,
                SizeFlagsHorizontal = SizeFlags.ShrinkEnd
            };
            valueControl.AddThemeColorOverride("font_color", new Color(0.4f, 0.9f, 0.5f));
            container.AddChild(valueControl);
        }
        
        private void OnResetPressed()
        {
            var confirmDialog = new AcceptDialog
            {
                Title = "确认重置",
                DialogText = "确定要重置所有统计数据吗？此操作不可撤销。"
            };
            confirmDialog.OkButtonPressed += () => {
                StatisticsManager.Instance.ResetStatistics();
                UpdateDisplay();
            };
            AddChild(confirmDialog);
            confirmDialog.PopupCentered();
        }
        
        public override void _ExitTree()
        {
            if (StatisticsManager.Instance != null)
            {
                StatisticsManager.Instance.OnStatisticsUpdated -= UpdateDisplay;
            }
        }
    }
}
