using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.MountBattle {
    /// <summary>
    /// 坐骑战斗UI - Mount Battle UI
    /// 显示坐骑战斗界面
    /// </summary>
    public partial class MountBattleUI : Control {
        private MountBattleSystem _battleSystem;
        private bool _isVisible = false;
        
        // UI 组件
        private Label _titleLabel;
        private Label _levelLabel;
        private Label _rankLabel;
        private Label _pointsLabel;
        private Label _streakLabel;
        private Label _winsLabel;
        private Label _lossesLabel;
        
        // 战斗状态
        private ProgressBar _healthBar;
        private ProgressBar _manaBar;
        private Label _healthLabel;
        private Label _manaLabel;
        
        // 技能栏
        private GridContainer _skillGrid;
        private List<Button> _skillButtons = new List<Button>();
        
        // 统计面板
        private Label _totalKillsLabel;
        private Label _totalDamageLabel;
        private Label _bestStreakLabel;
        
        // 战斗历史
        private VBoxContainer _battleHistoryContainer;
        
        public override void _Ready() {
            SetupUI();
            SetupInput();
            
            // 连接到信号
            _battleSystem = GetNode<MountBattleSystem>("/root/MountBattleSystem");
            if (_battleSystem != null) {
                _battleSystem.OnHealthChange += UpdateHealthBar;
                _battleSystem.OnManaChange += UpdateManaBar;
                _battleSystem.OnLevelUp += OnLevelUp;
                _battleSystem.OnRankChange += OnRankChange;
                _battleSystem.OnBattleEnd += OnBattleEnd;
            }
        }
        
        private void SetupUI() {
            // 主容器
            var mainContainer = new VBoxContainer {
                AnchorRight = AnchorEnd.Float(1),
                AnchorBottom = AnchorEnd.Float(1),
                CustomMinimumSize = new Vector2(800, 600)
            };
            AddChild(mainContainer);
            
            // 标题栏
            var header = new HBoxContainer();
            mainContainer.AddChild(header);
            
            _titleLabel = new Label {
                Text = "🐎 坐骑战斗",
                Align = Label.AlignEnum.Left
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            header.AddChild(_titleLabel);
            
            header.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });
            
            var closeButton = new Button { Text = "✕" };
            closeButton.Pressed += ToggleVisibility;
            header.AddChild(closeButton);
            
            // 信息栏
            var infoPanel = new HBoxContainer();
            mainContainer.AddChild(infoPanel);
            
            _levelLabel = new Label { Text = "等级: 1" };
            infoPanel.AddChild(_levelLabel);
            
            _rankLabel = new Label { Text = "段位: 青铜" };
            infoPanel.AddChild(_rankLabel);
            
            _pointsLabel = new Label { Text = "积分: 0" };
            infoPanel.AddChild(_pointsLabel);
            
            _streakLabel = new Label { Text = "连胜: 0" };
            infoPanel.AddChild(_streakLabel);
            
            _winsLabel = new Label { Text = "胜利: 0" };
            infoPanel.AddChild(_winsLabel);
            
            _lossesLabel = new Label { Text = "失败: 0" };
            infoPanel.AddChild(_lossesLabel);
            
            // 战斗状态区域
            var battlePanel = new HBoxContainer { SizeFlagsVertical = Control.SizeFlags.Expand };
            mainContainer.AddChild(battlePanel);
            
            // 左侧 - 战斗状态
            var leftPanel = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.Expand };
            battlePanel.AddChild(leftPanel);
            
            // 生命值
            var healthContainer = new VBoxContainer();
            leftPanel.AddChild(healthContainer);
            
            var healthTitle = new Label { Text = "生命值" };
            healthContainer.AddChild(healthTitle);
            
            _healthBar = new ProgressBar {
                MinValue = 0,
                MaxValue = 100,
                Value = 100,
                ShowPercentage = false
            };
            _healthBar.CustomMinimumSize = new Vector2(300, 30);
            healthContainer.AddChild(_healthBar);
            
            _healthLabel = new Label { Text = "100 / 100" };
            healthContainer.AddChild(_healthLabel);
            
            // 魔法值
            var manaContainer = new VBoxContainer();
            leftPanel.AddChild(manaContainer);
            
            var manaTitle = new Label { Text = "魔法值" };
            manaContainer.AddChild(manaTitle);
            
            _manaBar = new ProgressBar {
                MinValue = 0,
                MaxValue = 100,
                Value = 100,
                ShowPercentage = false
            };
            _manaBar.CustomMinimumSize = new Vector2(300, 30);
            manaContainer.AddChild(_manaBar);
            
            _manaLabel = new Label { Text = "100 / 100" };
            manaContainer.AddChild(_manaLabel);
            
            // 技能栏
            var skillTitle = new Label { Text = "战斗技能" };
            leftPanel.AddChild(skillTitle);
            
            _skillGrid = new GridContainer {
                Columns = 4
            };
            leftPanel.AddChild(_skillGrid);
            
            // 初始化技能按钮
            InitializeSkillButtons();
            
            // 右侧 - 统计面板
            var rightPanel = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.Expand };
            battlePanel.AddChild(rightPanel);
            
            var statsTitle = new Label { Text = "战斗统计" };
            statsTitle.AddThemeFontSizeOverride("font_size", 18);
            rightPanel.AddChild(statsTitle);
            
            _totalKillsLabel = new Label { Text = "总击杀: 0" };
            rightPanel.AddChild(_totalKillsLabel);
            
            _totalDamageLabel = new Label { Text = "总伤害: 0" };
            rightPanel.AddChild(_totalDamageLabel);
            
            _bestStreakLabel = new Label { Text = "最高连胜: 0" };
            rightPanel.AddChild(_bestStreakLabel);
            
            // 按钮区域
            var buttonPanel = new HBoxContainer { SizeFlagsVertical = Control.SizeFlags.ShrinkEnd };
            mainContainer.AddChild(buttonPanel);
            
            var startNormalButton = new Button { Text = "⚔️ 普通战斗" };
            startNormalButton.Pressed += () => StartBattle(MountBattleType.Normal);
            buttonPanel.AddChild(startNormalButton);
            
            var startRankedButton = new Button { Text = "🏆 排位赛" };
            startRankedButton.Pressed += () => StartBattle(MountBattleType.Ranked);
            buttonPanel.AddChild(startRankedButton);
            
            var startDuelButton = new Button { Text = "🎯 对决" };
            startDuelButton.Pressed += () => StartBattle(MountBattleType.Duel);
            buttonPanel.AddChild(startDuelButton);
            
            var enableButton = new Button { Text = "启用/禁用 坐骑战斗" };
            enableButton.Pressed += ToggleMountBattle;
            buttonPanel.AddChild(enableButton);
            
            // 底部提示
            var tipLabel = new Label {
                Text = "提示: 按 M 键切换显示 | 使用数字键 1-4 释放技能",
                Align = Label.AlignEnum.Center
            };
            mainContainer.AddChild(tipLabel);
            
            // 默认隐藏
            Hide();
        }
        
        private void InitializeSkillButtons() {
            var skills = MountBattleDatabase.MountSkills;
            
            foreach (var skill in skills) {
                var button = new Button {
                    Text = $"{skill.Value.Name}\n({skill.Key})",
                    TooltipText = skill.Value.Description
                };
                button.Pressed += () => OnSkillButtonPressed(skill.Key);
                _skillGrid.AddChild(button);
                _skillButtons.Add(button);
            }
        }
        
        private void SetupInput() {
            // 在实际游戏中，这里会连接输入事件
        }
        
        private void OnSkillButtonPressed(string skillId) {
            if (_battleSystem != null) {
                float damage = _battleSystem.DealDamage(50f); // 基础伤害
                _battleSystem.UseSkill(skillId, damage);
            }
        }
        
        private void StartBattle(MountBattleType battleType) {
            if (_battleSystem != null) {
                _battleSystem.StartBattle(battleType);
            }
        }
        
        private void ToggleMountBattle() {
            if (_battleSystem != null) {
                var data = _battleSystem.GetData();
                if (data.IsMountBattleEnabled) {
                    _battleSystem.DisableMountBattle();
                } else {
                    _battleSystem.EnableMountBattle();
                }
                UpdateDisplay();
            }
        }
        
        private void UpdateHealthBar(float current, float max) {
            if (_healthBar != null) {
                _healthBar.MaxValue = max;
                _healthBar.Value = current;
            }
            if (_healthLabel != null) {
                _healthLabel.Text = $"{(int)current} / {(int)max}";
            }
        }
        
        private void UpdateManaBar(float current, float max) {
            if (_manaBar != null) {
                _manaBar.MaxValue = max;
                _manaBar.Value = current;
            }
            if (_manaLabel != null) {
                _manaLabel.Text = $"{(int)current} / {(int)max}";
            }
        }
        
        private void OnLevelUp(int newLevel) {
            UpdateDisplay();
        }
        
        private void OnRankChange(string newRank) {
            UpdateDisplay();
        }
        
        private void OnBattleEnd(MountBattleRecord record) {
            UpdateDisplay();
            
            string message = record.Victory ? "战斗胜利!" : "战斗失败";
            GD.Print($"[MountBattle UI] {message}, 获得 {record.EarnedPoints} 积分, {record.EarnedExp} 经验");
        }
        
        private void UpdateDisplay() {
            if (_battleSystem == null) return;
            
            var data = _battleSystem.GetData();
            
            if (_levelLabel != null) {
                _levelLabel.Text = $"等级: {data.CurrentMountCombatLevel}";
            }
            if (_rankLabel != null) {
                _rankLabel.Text = $"段位: {data.SeasonRank}";
            }
            if (_pointsLabel != null) {
                _pointsLabel.Text = $"积分: {data.SeasonPoints}";
            }
            if (_streakLabel != null) {
                _streakLabel.Text = $"连胜: {data.CurrentStreak}";
            }
            if (_winsLabel != null) {
                _winsLabel.Text = $"胜利: {data.Wins}";
            }
            if (_lossesLabel != null) {
                _lossesLabel.Text = $"失败: {data.Losses}";
            }
            
            if (_totalKillsLabel != null) {
                _totalKillsLabel.Text = $"总击杀: {data.TotalMountKills}";
            }
            if (_totalDamageLabel != null) {
                _totalDamageLabel.Text = $"总伤害: {data.TotalMountDamageDealt}";
            }
            if (_bestStreakLabel != null) {
                _bestStreakLabel.Text = $"最高连胜: {data.BestStreak}";
            }
            
            // 更新生命值和魔法值
            if (_battleSystem.GetCurrentState() == MountBattleState.InBattle) {
                UpdateHealthBar(_battleSystem.GetCurrentMountHealth(), _battleSystem.GetMaxMountHealth());
                UpdateManaBar(_battleSystem.GetCurrentMountMana(), _battleSystem.GetMaxMountMana());
            } else {
                UpdateHealthBar(_battleSystem.GetMaxMountHealth(), _battleSystem.GetMaxMountHealth());
                UpdateManaBar(_battleSystem.GetMaxMountMana(), _battleSystem.GetMaxMountMana());
            }
        }
        
        public void ToggleVisibility() {
            _isVisible = !_isVisible;
            
            if (_isVisible) {
                Show();
                UpdateDisplay();
            } else {
                Hide();
            }
        }
        
        public bool IsVisible() => _isVisible;
        
        public override void _Input(InputEvent @event) {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
                // M 键切换显示
                if (keyEvent.Keycode == Key.M) {
                    ToggleVisibility();
                }
                
                // 数字键 1-4 释放技能
                if (keyEvent.Keycode >= Key.Key1 && keyEvent.Keycode <= Key.Key4) {
                    int skillIndex = keyEvent.Keycode - Key.Key1;
                    if (skillIndex < _skillButtons.Count) {
                        _skillButtons[skillIndex].Pressed -= () => OnSkillButtonPressed(MountBattleDatabase.MountSkills.Keys.GetHashCode(skillIndex).ToString());
                    }
                }
                
                // ESC 关闭
                if (keyEvent.Keycode == Key.Escape && _isVisible) {
                    ToggleVisibility();
                }
            }
        }
    }
}
