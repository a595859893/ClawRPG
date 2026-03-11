using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.BossMechanics;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Boss 机制 UI - 显示 Boss 战斗状态和统计数据
    /// </summary>
    public class BossMechanicsUI : Control {
        private static BossMechanicsUI _instance;
        public static BossMechanicsUI Instance {
            get => _instance;
        }

        // UI 组件
        private PanelContainer _mainPanel;
        private VBoxContainer _contentBox;
        private Label _titleLabel;
        private TabContainer _tabContainer;
        
        // 当前战斗标签页
        private VBoxContainer _activeFightTab;
        private Label _bossNameLabel;
        private Label _phaseLabel;
        private ProgressBar _healthBar;
        private Label _healthLabel;
        private Label _timeLabel;
        private Label _comboLabel;
        private Label _multiplierLabel;
        
        // 统计标签页
        private VBoxContainer _statsTab;
        private VBoxContainer _statsContainer;
        
        // 设置标签页
        private VBoxContainer _settingsTab;
        private CheckButton _showNotificationsCheck;
        
        private bool _isVisible = false; 
        private bool _showNotifications = true;

        public override void _Ready() {
            _instance = this;
            SetupUI();
            ConnectSignals();
            Visible = false; 
        }

        /// <summary>
        /// 设置 UI
        /// </summary>
        private void SetupUI() {
            // 主面板
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(600, 500);
            AddChild(_mainPanel);

            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            styleBox.BorderColor = new Color(0.3f, 0.3f, 0.4f);
            styleBox.SetBorderWidthAll(2);
            styleBox.SetCornerRadiusAll(8);
            _mainPanel.AddThemeStyleboxOverride("panel", styleBox);

            // 内容盒子
            _contentBox = new VBoxContainer();
            _contentBox.SetMeta("theme_constants", new Dictionary<string, int> {
                { "separation", 10 }
            });
            _mainPanel.AddChild(_contentBox);

            // 标题
            _titleLabel = new Label();
            _titleLabel.Text = "  ⚔️ Boss 机制系统";
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _contentBox.AddChild(_titleLabel);

            // 分隔线
            var separator = new HSeparator();
            _contentBox.AddChild(separator);

            // 标签容器
            _tabContainer = new TabContainer();
            _tabContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _contentBox.AddChild(_tabContainer);

            // === 当前战斗标签页 ===
            _activeFightTab = new VBoxContainer();
            _activeFightTab.SetMeta("theme_constants", new Dictionary<string, int> {
                { "separation", 8 }
            });
            _tabContainer.AddChild(_activeFightTab);
            _tabContainer.SetTabTitle(_activeFightTab, "当前战斗");

            // Boss 名称
            _bossNameLabel = new Label();
            _bossNameLabel.Text = "等待 Boss 战斗开始...";
            _bossNameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _bossNameLabel.AddThemeFontSizeOverride("font_size", 20);
            _activeFightTab.AddChild(_bossNameLabel);

            // 阶段
            _phaseLabel = new Label();
            _phaseLabel.Text = "阶段: -";
            _phaseLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _phaseLabel.AddThemeFontSizeOverride("font_size", 16);
            _activeFightTab.AddChild(_phaseLabel);

            // 血量条
            var healthContainer = new VBoxContainer();
            _activeFightTab.AddChild(healthContainer);

            var healthLabelTitle = new Label();
            healthLabelTitle.Text = "生命值";
            healthContainer.AddChild(healthLabelTitle);

            _healthBar = new ProgressBar();
            _healthBar.CustomMinimumSize = new Vector2(500, 30);
            _healthBar.MaxValue = 100;
            _healthBar.Value = 100;
            _healthBar.ShowPercentage = false; 
            healthContainer.AddChild(_healthBar);

            _healthLabel = new Label();
            _healthLabel.Text = "100%";
            _healthLabel.HorizontalAlignment = HorizontalAlignment.Center;
            healthContainer.AddChild(_healthLabel);

            // 战斗时间
            _timeLabel = new Label();
            _timeLabel.Text = "战斗时间: 00:00";
            _timeLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _activeFightTab.AddChild(_timeLabel);

            // 连击数
            _comboLabel = new Label();
            _comboLabel.Text = "连击数: 0";
            _comboLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _activeFightTab.AddChild(_comboLabel);

            // 属性乘数
            _multiplierLabel = new Label();
            _multiplierLabel.Text = "伤害乘数: 1.0x | 速度乘数: 1.0x";
            _multiplierLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _activeFightTab.AddChild(_multiplierLabel);

            // === 统计标签页 ===
            _statsTab = new VBoxContainer();
            _statsTab.SetMeta("theme_constants", new Dictionary<string, int> {
                { "separation", 5 }
            });
            _tabContainer.AddChild(_statsTab);
            _tabContainer.SetTabTitle(_statsTab, "战斗统计");

            var statsTitle = new Label();
            statsTitle.Text = "Boss 战斗统计";
            statsTitle.HorizontalAlignment = HorizontalAlignment.Center;
            statsTitle.AddThemeFontSizeOverride("font_size", 18);
            _statsTab.AddChild(statsTitle);

            var statsSeparator = new HSeparator();
            _statsTab.AddChild(statsSeparator);

            _statsContainer = new VBoxContainer();
            _statsContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _statsTab.AddChild(_statsContainer);

            // === 设置标签页 ===
            _settingsTab = new VBoxContainer();
            _settingsTab.SetMeta("theme_constants", new Dictionary<string, int> {
                { "separation", 10 }
            });
            _tabContainer.AddChild(_settingsTab);
            _tabContainer.SetTabTitle(_settingsTab, "设置");

            var settingsTitle = new Label();
            settingsTitle.Text = "Boss 机制设置";
            settingsTitle.HorizontalAlignment = HorizontalAlignment.Center;
            settingsTitle.AddThemeFontSizeOverride("font_size", 18);
            _settingsTab.AddChild(settingsTitle);

            _showNotificationsCheck = new CheckButton();
            _showNotificationsCheck.Text = "显示战斗通知";
            _showNotificationsCheck.ButtonPressed = _showNotifications;
            _showNotificationsCheck.Toggled += OnShowNotificationsToggled;
            _settingsTab.AddChild(_showNotificationsCheck);

            // 更新统计显示
            UpdateStatsDisplay();
        }

        /// <summary>
        /// 连接信号
        /// </summary>
        private void ConnectSignals() {
            BossMechanicsSystem.BossPhaseChanged += OnBossPhaseChanged;
            BossMechanicsSystem.BossEnraged += OnBossEnraged;
            BossMechanicsSystem.BossSpecialMechanicTriggered += OnBossSpecialMechanicTriggered;
        }

        /// <summary>
        /// 切换显示
        /// </summary>
        public void Toggle() {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible) {
                UpdateDisplay();
            }
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateDisplay() {
            UpdateActiveFightDisplay();
            UpdateStatsDisplay();
        }

        /// <summary>
        /// 更新当前战斗显示
        /// </summary>
        private void UpdateActiveFightDisplay() {
            // 获取所有活跃战斗
            var system = BossMechanicsSystem.Instance;
            
            // 显示第一个活跃战斗（或创建一个示例显示）
            var bossIds = new List<string> { "forest_boss", "fire_boss", "ice_boss", "shadow_boss", "holy_boss" };
            bool hasActiveFight = false; 
            
            foreach (var bossId in bossIds) {
                var fight = system.GetBossFightStatus(bossId);
                if (fight != null) {
                    hasActiveFight = true;
                    
                    // 更新 Boss 名称
                    _bossNameLabel.Text = $"🔥 {fight.bossName}";
                    
                    // 更新阶段
                    var phase = system.GetCurrentPhaseConfig(bossId);
                    if (phase != null) {
                        _phaseLabel.Text = $"阶段: {phase.phaseName}";
                        
                        // 阶段颜色
                        switch (phase.phaseType) {
                            case BossPhaseType.Normal:
                                _phaseLabel.Modulate = new Color(1f, 1f, 1f);
                                break;
                            case BossPhaseType.Enraged:
                                _phaseLabel.Modulate = new Color(1f, 0.5f, 0f);
                                break;
                            case BossPhaseType.Final:
                                _phaseLabel.Modulate = new Color(1f, 0.2f, 0.2f);
                                break;
                        }
                    }
                    
                    // 更新血量
                    float healthPercent = (fight.currentHealth / fight.maxHealth) * 100f;
                    _healthBar.MaxValue = fight.maxHealth;
                    _healthBar.Value = fight.currentHealth;
                    _healthLabel.Text = $"{fight.currentHealth:F0} / {fight.maxHealth:F0} ({healthPercent:F1}%)";
                    
                    // 更新战斗时间
                    int minutes = (int)(fight.timeInCombat / 60);
                    int seconds = (int)(fight.timeInCombat % 60);
                    _timeLabel.Text = $"战斗时间: {minutes:D2}:{seconds:D2}";
                    
                    // 更新连击
                    _comboLabel.Text = $"连击数: {fight.currentCombo}";
                    
                    // 更新乘数
                    float damageMult = system.GetDamageMultiplier(bossId);
                    float speedMult = system.GetSpeedMultiplier(bossId);
                    _multiplierLabel.Text = $"伤害乘数: {damageMult:F1}x | 速度乘数: {speedMult:F1}x";
                    
                    break;
                }
            }
            
            if (!hasActiveFight) {
                _bossNameLabel.Text = "等待 Boss 战斗开始...";
                _phaseLabel.Text = "阶段: -";
                _phaseLabel.Modulate = new Color(1f, 1f, 1f);
                _healthBar.Value = 0;
                _healthLabel.Text = "0 / 0 (0%)";
                _timeLabel.Text = "战斗时间: 00:00";
                _comboLabel.Text = "连击数: 0";
                _multiplierLabel.Text = "伤害乘数: 1.0x | 速度乘数: 1.0x";
            }
        }

        /// <summary>
        /// 更新统计显示
        /// </summary>
        private void UpdateStatsDisplay() {
            // 清除现有内容
            foreach (var child in _statsContainer.GetChildren()) {
                child.QueueFree();
            }
            
            var system = BossMechanicsSystem.Instance;
            var records = system.GetAllPlayerRecords();
            
            if (records.Count == 0) {
                var noDataLabel = new Label();
                noDataLabel.Text = "暂无战斗记录";
                noDataLabel.HorizontalAlignment = HorizontalAlignment.Center;
                _statsContainer.AddChild(noDataLabel);
                return;
            }
            
            // Boss 名称映射
            var bossNames = new Dictionary<string, string> {
                { "forest_boss", "森林之王" },
                { "fire_boss", "炎魔领主" },
                { "ice_boss", "冰霜巨龙" },
                { "shadow_boss", "暗影君王" },
                { "holy_boss", "光明主教" }
            };
            
            foreach (var kvp in records) {
                var record = kvp.Value;
                var bossName = bossNames.ContainsKey(record.bossId) ? bossNames[record.bossId] : record.bossId;
                
                var recordPanel = new PanelContainer();
                recordPanel.CustomMinimumSize = new Vector2(0, 100);
                _statsContainer.AddChild(recordPanel);
                
                var recordBox = new VBoxContainer();
                recordBox.SetMeta("theme_constants", new Dictionary<string, int> { { "separation", 5 } });
                recordPanel.AddChild(recordBox);
                
                // Boss 名称
                var nameLabel = new Label();
                nameLabel.Text = $"⚔️ {bossName}";
                nameLabel.AddThemeFontSizeOverride("font_size", 16);
                recordBox.AddChild(nameLabel);
                
                // 战斗次数
                var timesLabel = new Label();
                timesLabel.Text = $"   战斗次数: {record.timesFought} | 胜利: {record.timesDefeated}";
                recordBox.AddChild(timesLabel);
                
                // 最佳时间
                var bestTimeStr = record.bestTime < float.MaxValue 
                    ? $"{(int)(record.bestTime / 60)}:{(int)(record.bestTime % 60):D2}" 
                    : "--:--";
                var bestTimeLabel = new Label();
                bestTimeLabel.Text = $"   最佳时间: {bestTimeStr}";
                recordBox.AddChild(bestTimeLabel);
                
                // 总伤害
                var damageLabel = new Label();
                damageLabel.Text = $"   总造成伤害: {record.totalDamageDealt:F0}";
                recordBox.AddChild(damageLabel);
                
                // 最佳连击
                var comboLabel = new Label();
                comboLabel.Text = $"   最佳连击: {record.bestCombo}";
                recordBox.AddChild(comboLabel);
            }
        }

        /// <summary>
        /// Boss 阶段改变回调
        /// </summary>
        private void OnBossPhaseChanged() {
            if (_showNotifications && Visible) {
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Boss 狂暴回调
        /// </summary>
        private void OnBossEnraged(string message) {
            if (_showNotifications) {
                GD.Print($"[BossMechanicsUI] {message}");
            }
            if (Visible) {
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Boss 特殊机制触发回调
        /// </summary>
        private void OnBossSpecialMechanicTriggered(string mechanicName) {
            if (_showNotifications) {
                GD.Print($"[BossMechanicsUI] Boss 使用技能: {mechanicName}");
            }
            if (Visible) {
                UpdateDisplay();
            }
        }

        /// <summary>
        /// 通知开关切换
        /// </summary>
        private void OnShowNotificationsToggled(bool toggled) {
            _showNotifications = toggled;
        }

        public override void _Process(float delta) {
            if (Visible) {
                UpdateActiveFightDisplay();
            }
        }

        /// <summary>
        /// 输入处理
        /// </summary>
        public override void _UnhandledInput(InputEvent @event) {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
                if (keyEvent.Keycode == Key.B) {
                    Toggle();
                }
            }
        }

        /// <summary>
        /// 获取存档数据
        /// </summary>
        public Dictionary<string, Variant> GetSaveData() {
            var data = new Dictionary<string, Variant> {
                { "show_notifications", _showNotifications }
            };
            return data;
        }

        /// <summary>
        /// 加载存档数据
        /// </summary>
        public void LoadSaveData(Dictionary<string, Variant> data) {
            if (data.ContainsKey("show_notifications")) {
                _showNotifications = (bool)data["show_notifications"];
                _showNotificationsCheck.ButtonPressed = _showNotifications;
            }
        }
    }
}
