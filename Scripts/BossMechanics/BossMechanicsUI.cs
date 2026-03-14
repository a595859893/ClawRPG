// BossMechanicsUI.cs - Boss 机制系统 UI
using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.BossMechanics;

namespace ClawRPG.Scripts.BossMechanics {
    
    public class BossMechanicsUI : Control {
        
        // UI 组件
        private PanelContainer _mainPanel;
        private VBoxContainer _contentBox;
        
        // 标签页
        private TabContainer _tabContainer;
        private Control _bossListTab;
        private Control _battleTab;
        private Control _statisticsTab;
        
        // Boss 列表
        private ItemList _bossList;
        private Button _startBattleBtn;
        
        // 战斗界面
        private Label _bossNameLabel;
        private ProgressBar _healthBar;
        private Label _healthLabel;
        private Label _phaseLabel;
        private Label _timerLabel;
        private Label _enrageLabel;
        private VBoxContainer _skillsContainer;
        
        // 统计界面
        private Label _totalBattlesLabel;
        private Label _victoriesLabel;
        private Label _winRateLabel;
        private Label _totalDamageLabel;
        private Label _highestPhaseLabel;
        
        // 系统引用
        private BossMechanicsSystem _bossSystem;
        
        // 当前显示的 Boss
        private string _selectedBossId;
        
        public override void _Ready() {
            base._Ready();
            
            _bossSystem = BossMechanicsSystem.Instance;
            
            SetupUI();
            ConnectSignals();
            RefreshBossList();
            RefreshStatistics();
            
            // 初始隐藏战斗界面
            _battleTab.Visible = false;
        }
        
        private void SetupUI() {
            // 主面板
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(_mainPanel);
            
            _contentBox = new VBoxContainer();
            _contentBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _contentBox.AddThemeConstantOverride("separation", 10);
            _mainPanel.AddChild(_contentBox);
            
            // 标题
            var titleLabel = new Label();
            titleLabel.Text = "🏆 Boss 战斗系统";
            titleLabel.Align = Label.AlignEnum.Center;
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _contentBox.AddChild(titleLabel);
            
            // 标签页容器
            _tabContainer = new TabContainer();
            _tabContainer.SetSizeFlags(Control.SizeFlags.ExpandAndFill, Control.SizeFlags.ExpandAndFill);
            _contentBox.AddChild(_tabContainer);
            
            // Boss 列表标签页
            _bossListTab = new Control();
            _bossListTab.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _tabContainer.AddChild(_bossListTab);
            _tabContainer.SetTabTitle(_bossListTab, "Boss 列表");
            
            SetupBossListTab();
            
            // 战斗标签页
            _battleTab = new Control();
            _battleTab.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _tabContainer.AddChild(_battleTab);
            _tabContainer.SetTabTitle(_battleTab, "战斗");
            
            SetupBattleTab();
            
            // 统计标签页
            _statisticsTab = new Control();
            _statisticsTab.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _tabContainer.AddChild(_statisticsTab);
            _tabContainer.SetTabTitle(_statisticsTab, "统计");
            
            SetupStatisticsTab();
            
            // 关闭按钮
            var closeBtn = new Button();
            closeBtn.Text = "关闭 (B)";
            closeBtn.Pressed += () => Hide();
            _contentBox.AddChild(closeBtn);
        }
        
        private void SetupBossListTab() {
            var vbox = new VBoxContainer();
            vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            vbox.AddThemeConstantOverride("separation", 10);
            _bossListTab.AddChild(vbox);
            
            var listLabel = new Label();
            listLabel.Text = "选择 Boss:";
            vbox.AddChild(listLabel);
            
            _bossList = new ItemList();
            _bossList.SetSizeFlags(Control.SizeFlags.ExpandAndFill, Control.SizeFlags.ExpandAndFill);
            _bossList.ItemSelected += OnBossSelected;
            vbox.AddChild(_bossList);
            
            var buttonBox = new HBoxContainer();
            buttonBox.Alignment = BoxContainer.AlignmentMode.Center;
            vbox.AddChild(buttonBox);
            
            _startBattleBtn = new Button();
            _startBattleBtn.Text = "开始战斗";
            _startBattleBtn.Pressed += OnStartBattlePressed;
            _startBattleBtn.Disabled = true;
            buttonBox.AddChild(_startBattleBtn);
            
            var refreshBtn = new Button();
            refreshBtn.Text = "刷新";
            refreshBtn.Pressed += RefreshBossList;
            buttonBox.AddChild(refreshBtn);
        }
        
        private void SetupBattleTab() {
            var vbox = new VBoxContainer();
            vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            vbox.AddThemeConstantOverride("separation", 15);
            _battleTab.AddChild(vbox);
            
            // Boss 名称
            _bossNameLabel = new Label();
            _bossNameLabel.Text = "等待开始...";
            _bossNameLabel.Align = Label.AlignEnum.Center;
            _bossNameLabel.AddThemeFontSizeOverride("font_size", 28);
            vbox.AddChild(_bossNameLabel);
            
            // 生命条
            var healthBox = new VBoxContainer();
            vbox.AddChild(healthBox);
            
            var healthTitle = new Label();
            healthTitle.Text = "生命值:";
            healthBox.AddChild(healthTitle);
            
            _healthBar = new ProgressBar();
            _healthBar.SetSizeFlags(Control.SizeFlags.ExpandAndFill, Control.SizeFlags.ShrinkCenter);
            _healthBar.MinValue = 0;
            _healthBar.MaxValue = 100;
            _healthBar.Value = 100;
            healthBox.AddChild(_healthBar);
            
            _healthLabel = new Label();
            _healthLabel.Text = "0 / 0";
            _healthLabel.Align = Label.AlignEnum.Center;
            vbox.AddChild(_healthLabel);
            
            // 阶段信息
            _phaseLabel = new Label();
            _phaseLabel.Text = "阶段: 1";
            _phaseLabel.Align = Label.AlignEnum.Center;
            vbox.AddChild(_phaseLabel);
            
            // 计时器
            _timerLabel = new Label();
            _timerLabel.Text = "战斗时间: 00:00";
            _timerLabel.Align = Label.AlignEnum.Center;
            vbox.AddChild(_timerLabel);
            
            // 狂暴提示
            _enrageLabel = new Label();
            _enrageLabel.Text = "⚠️ BOSS 狂暴中!";
            _enrageLabel.Modulate = new Color(1, 0.3, 0.3);
            _enrageLabel.Align = Label.AlignEnum.Center;
            _enrageLabel.Visible = false;
            vbox.AddChild(_enrageLabel);
            
            // 技能列表
            var skillsLabel = new Label();
            skillsLabel.Text = "Boss 技能:";
            vbox.AddChild(skillsLabel);
            
            _skillsContainer = new VBoxContainer();
            _skillsContainer.SetSizeFlags(Control.SizeFlags.ExpandAndFill, Control.SizeFlags.ExpandAndFill);
            vbox.AddChild(_skillsContainer);
            
            // 放弃按钮
            var forfeitBtn = new Button();
            forfeitBtn.Text = "放弃战斗";
            forfeitBtn.Pressed += OnForfeitPressed;
            vbox.AddChild(forfeitBtn);
        }
        
        private void SetupStatisticsTab() {
            var vbox = new VBoxContainer();
            vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            vbox.AddThemeConstantOverride("separation", 10);
            vbox.MarginLeft = 20;
            vbox.MarginTop = 20;
            _statisticsTab.AddChild(vbox);
            
            var title = new Label();
            title.Text = "战斗统计";
            title.AddThemeFontSizeOverride("font_size", 22);
            vbox.AddChild(title);
            
            _totalBattlesLabel = new Label();
            _totalBattlesLabel.Text = "总战斗次数: 0";
            vbox.AddChild(_totalBattlesLabel);
            
            _victoriesLabel = new Label();
            _victoriesLabel.Text = "胜利次数: 0";
            vbox.AddChild(_victoriesLabel);
            
            _winRateLabel = new Label();
            _winRateLabel.Text = "胜率: 0%";
            vbox.AddChild(_winRateLabel);
            
            _totalDamageLabel = new Label();
            _totalDamageLabel.Text = "总伤害: 0";
            vbox.AddChild(_totalDamageLabel);
            
            _highestPhaseLabel = new Label();
            _highestPhaseLabel.Text = "最高阶段: 0";
            vbox.AddChild(_highestPhaseLabel);
            
            var refreshBtn = new Button();
            refreshBtn.Text = "刷新统计";
            refreshBtn.Pressed += RefreshStatistics;
            vbox.AddChild(refreshBtn);
        }
        
        private void ConnectSignals() {
            if (_bossSystem != null) {
                _bossSystem.Connect(nameof(BossMechanicsSystem.BossDamaged), this, nameof(OnBossDamaged));
                _bossSystem.Connect(nameof(BossMechanicsSystem.PhaseChanged), this, nameof(OnPhaseChanged));
                _bossSystem.Connect(nameof(BossMechanicsSystem.BossDefeated), this, nameof(OnBossDefeated));
                _bossSystem.Connect(nameof(BossMechanicsSystem.SkillUsed), this, nameof(OnSkillUsed));
                _bossSystem.Connect(nameof(BossMechanicsSystem.LootDropped), this, nameof(OnLootDropped));
                _bossSystem.Connect(nameof(BossMechanicsSystem.EnrageActivated), this, nameof(OnEnrageActivated));
                _bossSystem.Connect(nameof(BossMechanicsSystem.BattleStarted), this, nameof(OnBattleStarted));
            }
        }
        
        private void OnBossSelected(int index) {
            if (index >= 0) {
                var bosses = _bossSystem.GetAvailableBosses();
                if (index < bosses.Count) {
                    _selectedBossId = bosses[index];
                    _startBattleBtn.Disabled = false;
                }
            }
        }
        
        private void OnStartBattlePressed() {
            if (!string.IsNullOrEmpty(_selectedBossId)) {
                _bossSystem.StartBattle(_selectedBossId);
                _tabContainer.CurrentTab = 1; // 切换到战斗标签页
            }
        }
        
        private void OnForfeitPressed() {
            _bossSystem.ForfeitBattle();
            _battleTab.Visible = false;
            _tabContainer.CurrentTab = 0; // 切换回 Boss 列表
        }
        
        private void OnBossDamaged(float damage, string damageType) {
            UpdateBattleUI();
        }
        
        private void OnPhaseChanged(int newPhase, BattlePhaseType phaseType) {
            UpdateBattleUI();
        }
        
        private void OnBossDefeated(string bossId, bool isVictory, int starsEarned) {
            var message = isVictory ? $"胜利! 获得 {starsEarned} ⭐" : "失败...";
            
            var dialog = new AcceptDialog();
            dialog.Title = "战斗结束";
            dialog.DialogText = message;
            dialog.Connect("confirmed", this, nameof(OnDialogConfirmed));
            AddChild(dialog);
            dialog.PopupCentered();
            
            _battleTab.Visible = false;
        }
        
        private void OnDialogConfirmed() {
            _tabContainer.CurrentTab = 0;
            RefreshBossList();
            RefreshStatistics();
        }
        
        private void OnSkillUsed(string skillId, string skillName) {
            GD.Print($"UI: Boss used {skillName}");
        }
        
        private void OnLootDropped(string lootId, string itemName, int quantity) {
            GD.Print($"UI: Loot dropped - {itemName} x{quantity}");
        }
        
        private void OnEnrageActivated() {
            _enrageLabel.Visible = true;
        }
        
        private void OnBattleStarted(string bossId, string bossName) {
            _battleTab.Visible = true;
            _bossNameLabel.Text = bossName;
            _enrageLabel.Visible = false;
            UpdateBattleUI();
            UpdateSkillsList();
        }
        
        private void UpdateBattleUI() {
            var boss = _bossSystem.GetCurrentBoss();
            if (boss == null) return;
            
            _healthBar.MaxValue = boss.MaxHealth;
            _healthBar.Value = boss.CurrentHealth;
            _healthLabel.Text = $"{(int)boss.CurrentHealth} / {(int)boss.MaxHealth} ({(int)(boss.HealthPercentage * 100)}%)";
            
            _phaseLabel.Text = $"阶段: {boss.CurrentPhase}";
            
            float battleTime = _bossSystem.GetBattleTime();
            int minutes = (int)(battleTime / 60);
            int seconds = (int)(battleTime % 60);
            _timerLabel.Text = $"战斗时间: {minutes:D2}:{seconds:D2}";
            
            _enrageLabel.Visible = boss.IsEnraged;
        }
        
        private void UpdateSkillsList() {
            foreach (var child in _skillsContainer.GetChildren()) {
                child.QueueFree();
            }
            
            var db = BossMechanicsDatabase.Instance;
            var boss = _bossSystem.GetCurrentBoss();
            if (boss == null) return;
            
            var phases = db.GetBossPhases(boss.BossId);
            foreach (var phase in phases) {
                if (phase.PhaseNumber <= boss.CurrentPhase) {
                    foreach (var skillId in phase.UnlockedSkills) {
                        var skill = db.GetSkill(skillId);
                        if (skill != null) {
                            var skillLabel = new Label();
                            float cooldown = _bossSystem.GetSkillCooldown(skillId);
                            string cooldownText = cooldown > 0 ? $" (冷却: {cooldown:F1}s)" : " [可用]";
                            skillLabel.Text = $"• {skill.SkillName}: {skill.Description}{cooldownText}";
                            _skillsContainer.AddChild(skillLabel);
                        }
                    }
                    break;
                }
            }
        }
        
        private void RefreshBossList() {
            _bossList.Clear();
            
            var bosses = _bossSystem.GetAvailableBosses();
            var db = BossMechanicsDatabase.Instance;
            
            foreach (var bossId in bosses) {
                var bossConfig = db.BossConfigs[bossId];
                string displayText = $"[{bossConfig.Type}] {bossConfig.BossName} (Lv.{bossConfig.Level})";
                _bossList.AddItem(displayText);
            }
            
            _startBattleBtn.Disabled = true;
            _selectedBossId = null;
        }
        
        private void RefreshStatistics() {
            var stats = _bossSystem.GetStatistics();
            
            _totalBattlesLabel.Text = $"总战斗次数: {stats.TotalBattles}";
            _victoriesLabel.Text = $"胜利次数: {stats.Victories}";
            _winRateLabel.Text = $"胜率: {stats.WinRate:F1}%";
            _totalDamageLabel.Text = $"总伤害: {stats.TotalDamageDealt}";
            _highestPhaseLabel.Text = $"最高阶段: {stats.HighestPhaseReached}";
        }
        
        public override void _Input(InputEvent @event) {
            base._Input(@event);
            
            if (@event.IsActionPressed("ui_cancel") || @event.IsActionPressed("boss_toggle")) {
                if (Visible) {
                    Hide();
                } else {
                    Show();
                }
            }
        }
        
        public void Show() {
            Visible = true;
            RefreshBossList();
            RefreshStatistics();
        }
        
        public void Hide() {
            Visible = false;
        }
    }
}
