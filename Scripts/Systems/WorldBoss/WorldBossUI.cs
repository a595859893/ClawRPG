using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Systems.WorldBoss;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// World boss UI for displaying and interacting with world bosses
    /// </summary>
    public partial class WorldBossUI : Control
    {
        private static WorldBossUI _instance;
        public static WorldBossUI Instance => _instance;
        
        // UI Components
        private PanelContainer _mainPanel;
        private VBoxContainer _mainContainer;
        private TabContainer _tabContainer;
        
        // Boss list tab
        private VBoxContainer _bossListTab;
        private ScrollContainer _bossListScroll;
        private VBoxContainer _bossListContainer;
        
        // Active bosses tab
        private VBoxContainer _activeTab;
        private ScrollContainer _activeScroll;
        private VBoxContainer _activeContainer;
        
        // Statistics tab
        private VBoxContainer _statsTab;
        private VBoxContainer _statsContainer;
        
        // Timers tab
        private VBoxContainer _timersTab;
        private ScrollContainer _timersScroll;
        private VBoxContainer _timersContainer;
        
        // Visibility
        private bool _isVisible = false; 
        
        public override void _Ready()
        {
            _instance = this;
            SetupUI();
            Hide();
        }
        
        private void SetupUI()
        {
            // Main panel
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(800, 600);
            AddChild(_mainPanel);
            
            // Style
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(8);
            _mainPanel.AddThemeStyleboxOverride("panel", style);
            
            // Main container
            _mainContainer = new VBoxContainer();
            _mainContainer.SetSeparation(10);
            _mainPanel.AddChild(_mainContainer);
            
            // Title
            var title = new Label();
            title.Text = "🌍 世界首领系统";
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.AddThemeFontSizeOverride("font_size", 24);
            _mainContainer.AddChild(title);
            
            // Tab container
            _tabContainer = new TabContainer();
            _tabContainer.SetVExpand(true);
            _mainContainer.AddChild(_tabContainer);
            
            SetupBossListTab();
            SetupActiveTab();
            SetupStatsTab();
            SetupTimersTab();
            
            // Close button
            var closeBtn = new Button();
            closeBtn.Text = " 关闭 (W) ";
            closeBtn.Pressed += () => ToggleVisibility();
            _mainContainer.AddChild(closeBtn);
        }
        
        private void SetupBossListTab()
        {
            _bossListTab = new VBoxContainer();
            _bossListTab.SetSeparation(5);
            _tabContainer.AddChild(_bossListTab);
            _tabContainer.SetTabTitle(_bossListTab, "首领列表");
            
            var title = new Label();
            title.Text = "世界首领一览";
            title.AddThemeFontSizeOverride("font_size", 18);
            _bossListTab.AddChild(title);
            
            _bossListScroll = new ScrollContainer();
            _bossListScroll.SetVExpand(true);
            _bossListTab.AddChild(_bossListScroll);
            
            _bossListContainer = new VBoxContainer();
            _bossListContainer.SetSeparation(5);
            _bossListScroll.AddChild(_bossListContainer);
            
            RefreshBossList();
        }
        
        private void SetupActiveTab()
        {
            _activeTab = new VBoxContainer();
            _activeTab.SetSeparation(5);
            _tabContainer.AddChild(_activeTab);
            _tabContainer.SetTabTitle(_activeTab, "进行中");
            
            var title = new Label();
            title.Text = "当前活跃的世界首领";
            title.AddThemeFontSizeOverride("font_size", 18);
            _activeTab.AddChild(title);
            
            _activeScroll = new ScrollContainer();
            _activeScroll.SetVExpand(true);
            _activeTab.AddChild(_activeScroll);
            
            _activeContainer = new VBoxContainer();
            _activeContainer.SetSeparation(5);
            _activeScroll.AddChild(_activeContainer);
            
            RefreshActiveBosses();
        }
        
        private void SetupStatsTab()
        {
            _statsTab = new VBoxContainer();
            _statsTab.SetSeparation(5);
            _tabContainer.AddChild(_statsTab);
            _tabContainer.SetTabTitle(_statsTab, "统计");
            
            var title = new Label();
            title.Text = "击杀统计";
            title.AddThemeFontSizeOverride("font_size", 18);
            _statsTab.AddChild(title);
            
            _statsContainer = new VBoxContainer();
            _statsContainer.SetSeparation(5);
            _statsTab.AddChild(_statsContainer);
            
            RefreshStats();
        }
        
        private void SetupTimersTab()
        {
            _timersTab = new VBoxContainer();
            _timersTab.SetSeparation(5);
            _tabContainer.AddChild(_timersTab);
            _tabContainer.SetTabTitle(_timersTab, "刷新时间");
            
            var title = new Label();
            title.Text = "首领刷新倒计时";
            title.AddThemeFontSizeOverride("font_size", 18);
            _timersTab.AddChild(title);
            
            _timersScroll = new ScrollContainer();
            _timersScroll.SetVExpand(true);
            _timersTab.AddChild(_timersScroll);
            
            _timersContainer = new VBoxContainer();
            _timersContainer.SetSeparation(5);
            _timersScroll.AddChild(_timersContainer);
            
            RefreshTimers();
        }
        
        public override void _Process(double delta)
        {
            if (!_isVisible) return;
            
            // Update timers display every second
            if (Engine.GetProcessFrames() % 60 == 0)
            {
                RefreshTimers();
                RefreshActiveBosses();
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel") || @event.IsActionPressed("ui_toggle_world_boss"))
            {
                ToggleVisibility();
            }
        }
        
        public void ToggleVisibility()
        {
            if (_isVisible)
            {
                Hide();
                _isVisible = false; 
            }
            else
            {
                Show();
                RefreshAll();
                _isVisible = true;
            }
        }
        
        private void RefreshAll()
        {
            RefreshBossList();
            RefreshActiveBosses();
            RefreshStats();
            RefreshTimers();
        }
        
        private void RefreshBossList()
        {
            // Clear existing
            foreach (var child in _bossListContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            var bosses = WorldBossDatabase.GetAllBosses();
            
            // Group by rarity
            var grouped = new Dictionary<WorldBossData.BossRarity, List<WorldBossData.WorldBoss>>();
            foreach (var boss in bosses)
            {
                if (!grouped.ContainsKey(boss.Rarity))
                    grouped[boss.Rarity] = new List<WorldBossData.WorldBoss>();
                grouped[boss.Rarity].Add(boss);
            }
            
            foreach (var group in grouped)
            {
                var rarityLabel = new Label();
                rarityLabel.Text = $"=== {GetRarityName(group.Key)} ===";
                rarityLabel.AddThemeColorOverride("font_color", GetRarityColor(group.Key));
                rarityLabel.AddThemeFontSizeOverride("font_size", 16);
                _bossListContainer.AddChild(rarityLabel);
                
                foreach (var boss in group.Value)
                {
                    var bossPanel = CreateBossInfoPanel(boss);
                    _bossListContainer.AddChild(bossPanel);
                }
            }
        }
        
        private Control CreateBossInfoPanel(WorldBossData.WorldBoss boss)
        {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(700, 80);
            
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.15f, 0.15f, 0.2f);
            style.BorderColor = GetRarityColor(boss.Rarity);
            style.SetBorderWidthAll(1);
            style.SetCornerRadiusAll(4);
            panel.AddThemeStyleboxOverride("panel", style);
            
            var hbox = new HBoxContainer();
            hbox.SetSeparation(20);
            panel.AddChild(hbox);
            
            // Boss name and rarity
            var infoBox = new VBoxContainer();
            infoBox.SetSeparation(2);
            hbox.AddChild(infoBox);
            
            var nameLabel = new Label();
            nameLabel.Text = $"[{GetRarityName(boss.Rarity)}] {boss.Name}";
            nameLabel.AddThemeColorOverride("font_color", GetRarityColor(boss.Rarity));
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            infoBox.AddChild(nameLabel);
            
            var descLabel = new Label();
            descLabel.Text = boss.Description;
            descLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            infoBox.AddChild(descLabel);
            
            // Stats
            var statsBox = new VBoxContainer();
            statsBox.SetSeparation(2);
            hbox.AddChild(statsBox);
            
            var levelLabel = new Label();
            levelLabel.Text = $"等级: {boss.Level}";
            statsBox.AddChild(levelLabel);
            
            var healthLabel = new Label();
            healthLabel.Text = $"生命: {boss.Health:N0}";
            statsBox.AddChild(healthLabel);
            
            var rewardLabel = new Label();
            rewardLabel.Text = $"奖励: {boss.GoldReward:N0}金 / {boss.ExpReward:N0}经验";
            rewardLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.3f));
            statsBox.AddChild(rewardLabel);
            
            return panel;
        }
        
        private void RefreshActiveBosses()
        {
            // Clear existing
            foreach (var child in _activeContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            var system = WorldBossSystem.Instance;
            if (system == null)
            {
                var noBossLabel = new Label();
                noBossLabel.Text = "当前没有活跃的世界首领";
                noBossLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
                _activeContainer.AddChild(noBossLabel);
                return;
            }
            
            var activeBosses = system.GetActiveBosses();
            bool hasActive = false; 
            
            foreach (var boss in activeBosses)
            {
                if (boss.IsDefeated) continue;
                hasActive = true;
                
                var bossPanel = CreateActiveBossPanel(boss);
                _activeContainer.AddChild(bossPanel);
            }
            
            if (!hasActive)
            {
                var noBossLabel = new Label();
                noBossLabel.Text = "当前没有活跃的世界首领";
                noBossLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
                _activeContainer.AddChild(noBossLabel);
            }
        }
        
        private Control CreateActiveBossPanel(WorldBossData.ActiveWorldBoss boss)
        {
            var system = WorldBossSystem.Instance;
            
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(700, 120);
            
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.2f, 0.15f, 0.15f);
            style.BorderColor = GetRarityColor(boss.Rarity);
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(4);
            panel.AddThemeStyleboxOverride("panel", style);
            
            var vbox = new VBoxContainer();
            vbox.SetSeparation(5);
            panel.AddChild(vbox);
            
            // Boss name
            var nameLabel = new Label();
            nameLabel.Text = $"🔥 {boss.BossName} [活跃中]";
            nameLabel.AddThemeColorOverride("font_color", GetRarityColor(boss.Rarity));
            nameLabel.AddThemeFontSizeOverride("font_size", 18);
            vbox.AddChild(nameLabel);
            
            // Health bar
            var healthBox = new HBoxContainer();
            vbox.AddChild(healthBox);
            
            var healthLabel = new Label();
            healthLabel.Text = "生命: ";
            healthBox.AddChild(healthLabel);
            
            var healthBar = new ProgressBar();
            healthBar.CustomMinimumSize = new Vector2(400, 20);
            healthBar.MinValue = 0;
            healthBar.MaxValue = boss.MaxHealth;
            healthBar.Value = boss.CurrentHealth;
            healthBar.ShowPercentage = false; 
            healthBox.AddChild(healthBar);
            
            var healthValue = new Label();
            healthValue.Text = $" {boss.CurrentHealth:N0} / {boss.MaxHealth:N0}";
            healthBox.AddChild(healthValue);
            
            // Time remaining
            var elapsed = DateTime.Now - boss.SpawnTime;
            var remaining = TimeSpan.FromMinutes(boss.LifeTimeMinutes) - elapsed;
            var timeLabel = new Label();
            timeLabel.Text = $"剩余时间: {remaining.Minutes:D2}:{remaining.Seconds:D2} | 已造成伤害: {boss.TotalDamageDealt:N0}";
            vbox.AddChild(timeLabel);
            
            // Damage records
            var damageRecords = system.GetDamageRecords(boss.InstanceId);
            if (damageRecords.Count > 0)
            {
                var damageLabel = new Label();
                damageLabel.Text = "伤害排名:";
                damageLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
                vbox.AddChild(damageLabel);
                
                int rank = 1;
                foreach (var record in damageRecords)
                {
                    if (rank > 5) break; // Top 5
                    var recordLabel = new Label();
                    recordLabel.Text = $"  {rank}. {record.PlayerName}: {record.DamageDealt:N0} ({record.DamagePercent:F1}%)";
                    vbox.AddChild(recordLabel);
                    rank++;
                }
            }
            
            return panel;
        }
        
        private void RefreshStats()
        {
            // Clear existing
            foreach (var child in _statsContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            var system = WorldBossSystem.Instance;
            if (system == null) return;
            
            var totalKilled = system.GetTotalBossesKilled();
            var totalLabel = new Label();
            totalLabel.Text = $"总计击败首领: {totalKilled}";
            totalLabel.AddThemeFontSizeOverride("font_size", 18);
            _statsContainer.AddChild(totalLabel);
            
            // Kill history
            var history = system.GetKillHistory();
            if (history.Count > 0)
            {
                var historyTitle = new Label();
                historyTitle.Text = "最近击败:";
                historyTitle.AddThemeFontSizeOverride("font_size", 14);
                _statsContainer.AddChild(historyTitle);
                
                int count = 0;
                for (int i = history.Count - 1; i >= 0 && count < 10; i--)
                {
                    var record = history[i];
                    var recordLabel = new Label();
                    recordLabel.Text = $"  [{GetRarityName(record.Rarity)}] {record.BossName} - {record.KillTime:MM-dd HH:mm}";
                    recordLabel.AddThemeColorOverride("font_color", GetRarityColor(record.Rarity));
                    _statsContainer.AddChild(recordLabel);
                    count++;
                }
            }
        }
        
        private void RefreshTimers()
        {
            // Clear existing
            foreach (var child in _timersContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            var system = WorldBossSystem.Instance;
            if (system == null) return;
            
            var timers = system.GetAllSpawnTimers();
            var bosses = WorldBossDatabase.GetAllBosses();
            
            foreach (var boss in bosses)
            {
                var nextSpawn = timers.ContainsKey(boss.Id) ? timers[boss.Id] : DateTime.Now;
                var remaining = nextSpawn - DateTime.Now;
                
                var timerPanel = new PanelContainer();
                var style = new StyleBoxFlat();
                style.BgColor = new Color(0.15f, 0.15f, 0.2f);
                style.BorderColor = GetRarityColor(boss.Rarity);
                style.SetBorderWidthAll(1);
                style.SetCornerRadiusAll(4);
                timerPanel.AddThemeStyleboxOverride("panel", style);
                timerPanel.CustomMinimumSize = new Vector2(650, 50);
                
                var hbox = new HBoxContainer();
                hbox.SetSeparation(20);
                timerPanel.AddChild(hbox);
                
                var nameLabel = new Label();
                nameLabel.Text = $"[{GetRarityName(boss.Rarity)}] {boss.Name}";
                nameLabel.AddThemeColorOverride("font_color", GetRarityColor(boss.Rarity));
                nameLabel.CustomMinimumSize = new Vector2(200, 0);
                hbox.AddChild(nameLabel);
                
                var timeLabel = new Label();
                if (remaining.TotalSeconds > 0)
                {
                    timeLabel.Text = $"刷新: {remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
                    timeLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.8f, 0.3f));
                }
                else
                {
                    timeLabel.Text = "即将刷新!";
                    timeLabel.AddThemeColorOverride("font_color", new Color(1f, 0.3f, 0.3f));
                }
                hbox.AddChild(timeLabel);
                
                var levelLabel = new Label();
                levelLabel.Text = $"推荐等级: {boss.Level}";
                hbox.AddChild(levelLabel);
                
                _timersContainer.AddChild(timerPanel);
            }
        }
        
        private string GetRarityName(WorldBossData.BossRarity rarity)
        {
            switch (rarity)
            {
                case WorldBossData.BossRarity.Elite: return "精英";
                case WorldBossData.BossRarity.Rare: return "稀有";
                case WorldBossData.BossRarity.Epic: return "史诗";
                case WorldBossData.BossRarity.Legendary: return "传说";
                case WorldBossData.BossRarity.Mythic: return "神级";
                default: return "未知";
            }
        }
        
        private Color GetRarityColor(WorldBossData.BossRarity rarity)
        {
            var colorValue = WorldBossDatabase.GetRarityColor(rarity);
            return new Color(
                ((colorValue >> 16) & 0xFF) / 255f,
                ((colorValue >> 8) & 0xFF) / 255f,
                (colorValue & 0xFF) / 255f
            );
        }
    }
}
