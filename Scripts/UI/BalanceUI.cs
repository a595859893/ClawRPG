using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 游戏平衡调整UI - 允许玩家实时调整游戏平衡参数
    /// </summary>
    public partial class BalanceUI : Control {
        
        [Export] public Key keyToggle = Key.F11;
        
        private Control _mainPanel;
        private VBoxContainer _mainContainer;
        private Label _titleLabel;
        private Button _closeButton;
        private TabContainer _tabContainer;
        
        // Player settings
        private VBoxContainer _playerPanel;
        private HSlider _playerHealthSlider;
        private HSlider _playerManaSlider;
        private HSlider _playerAttackSlider;
        private HSlider _playerDefenseSlider;
        private HSlider _playerCritSlider;
        private HSlider _playerDodgeSlider;
        
        // Enemy settings
        private VBoxContainer _enemyPanel;
        private HSlider _enemyHealthSlider;
        private HSlider _enemyDamageSlider;
        private HSlider _enemyXPSlider;
        private HSlider _enemyDropRateSlider;
        
        // Combat settings
        private VBoxContainer _combatPanel;
        private HSlider _baseDamageSlider;
        private HSlider _critChanceSlider;
        private HSlider _critDamageSlider;
        private HSlider _dodgeChanceSlider;
        private HSlider _blockReductionSlider;
        private HSlider _counterDamageSlider;
        private HSlider _comboBonusSlider;
        
        // Economy settings
        private VBoxContainer _economyPanel;
        private HSlider _goldDropSlider;
        private HSlider _itemPriceSlider;
        private HSlider _questRewardSlider;
        
        // Boss settings
        private VBoxContainer _bossPanel;
        private HSlider _bossHealthSlider;
        private HSlider _bossDamageSlider;
        private HSlider _bossEnrageTimeSlider;
        
        // Buttons
        private Button _easyButton;
        private Button _normalButton;
        private Button _hardButton;
        private Button _nightmareButton;
        private Button _saveButton;
        private Button _resetButton;
        private Button _exportButton;
        private Button _importButton;
        
        private Label _previewLabel;
        
        private bool _isVisible = false; 
        
        public override void _Ready() {
            SetupUI();
            Hide();
            LoadCurrentValues();
        }
        
        private void SetupUI() {
            // Main panel
            _mainPanel = new Control();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(600, 700);
            AddChild(_mainPanel);
            
            var panel = new PanelContainer();
            panel.SetAnchorsPreset(Control.LayoutPreset.Center);
            panel.CustomMinimumSize = new Vector2(600, 700);
            _mainPanel.AddChild(panel);
            
            var margin = new MarginContainer();
            margin.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            margin.AddThemeConstantOverride("margin_left", 20);
            margin.AddThemeConstantOverride("margin_right", 20);
            margin.AddThemeConstantOverride("margin_top", 20);
            margin.AddThemeConstantOverride("margin_bottom", 20);
            panel.AddChild(margin);
            
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            margin.AddChild(_mainContainer);
            
            // Header
            var header = new HBoxContainer();
            _mainContainer.AddChild(header);
            
            _titleLabel = new Label();
            _titleLabel.Text = "⚙️ 游戏平衡调整";
            _titleLabel.AddThemeFontSizeOverride("font_size", 28);
            header.AddChild(_titleLabel);
            
            header.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlagsExpand });
            
            _closeButton = new Button();
            _closeButton.Text = "✕";
            _closeButton.TooltipText = "关闭 (F10)";
            _closeButton.Pressed += () => ToggleVisibility();
            header.AddChild(_closeButton);
            
            // Difficulty preset buttons
            var presetContainer = new HBoxContainer();
            _mainContainer.AddChild(presetContainer);
            presetContainer.AddThemeConstantOverride("separation", 10);
            
            _easyButton = new Button();
            _easyButton.Text = "简单";
            _easyButton.Pressed += () => ApplyPreset("easy");
            presetContainer.AddChild(_easyButton);
            
            _normalButton = new Button();
            _normalButton.Text = "普通";
            _normalButton.Pressed += () => ApplyPreset("normal");
            presetContainer.AddChild(_normalButton);
            
            _hardButton = new Button();
            _hardButton.Text = "困难";
            _hardButton.Pressed += () => ApplyPreset("hard");
            presetContainer.AddChild(_hardButton);
            
            _nightmareButton = new Button();
            _nightmareButton.Text = "噩梦";
            _nightmareButton.Pressed += () => ApplyPreset("nightmare");
            presetContainer.AddChild(_nightmareButton);
            
            // Tab container
            _tabContainer = new TabContainer();
            _tabContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _mainContainer.AddChild(_tabContainer);
            
            // Create tabs
            _playerPanel = CreatePlayerTab();
            _enemyPanel = CreateEnemyTab();
            _combatPanel = CreateCombatTab();
            _economyPanel = CreateEconomyTab();
            _bossPanel = CreateBossTab();
            
            _tabContainer.AddChild(_playerPanel);
            _tabContainer.AddChild(_enemyPanel);
            _tabContainer.AddChild(_combatPanel);
            _tabContainer.AddChild(_economyPanel);
            _tabContainer.AddChild(_bossPanel);
            
            // Preview label
            _previewLabel = new Label();
            _previewLabel.Text = "💡 调整将实时生效";
            _previewLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
            _mainContainer.AddChild(_previewLabel);
            
            // Action buttons
            var actionContainer = new HBoxContainer();
            _mainContainer.AddChild(actionContainer);
            actionContainer.AddThemeConstantOverride("separation", 10);
            
            _saveButton = new Button();
            _saveButton.Text = "💾 保存配置";
            _saveButton.Pressed += SaveConfig;
            actionContainer.AddChild(_saveButton);
            
            _resetButton = new Button();
            _resetButton.Text = "🔄 重置为默认";
            _resetButton.Pressed += ResetToDefault;
            actionContainer.AddChild(_resetButton);
            
            _exportButton = new Button();
            _exportButton.Text = "📤 导出配置";
            _exportButton.Pressed += ExportConfig;
            actionContainer.AddChild(_exportButton);
            
            _importButton = new Button();
            _importButton.Text = "📥 导入配置";
            _importButton.Pressed += ImportConfig;
            actionContainer.AddChild(_importButton);
            
            // Hotkey hint
            var hintLabel = new Label();
            hintLabel.Text = "提示: 按 F10 打开/关闭平衡调整面板";
            hintLabel.AddThemeFontSizeOverride("font_size", 12);
            hintLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
            _mainContainer.AddChild(hintLabel);
        }
        
        private VBoxContainer CreatePlayerTab() {
            var container = new VBoxContainer();
            container.Name = "玩家";
            
            var scroll = new ScrollContainer();
            scroll.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            container.AddChild(scroll);
            
            var vbox = new VBoxContainer();
            vbox.CustomMinimumSize = new Vector2(500, 400);
            scroll.AddChild(vbox);
            
            // Health
            vbox.AddChild(CreateSliderRow("❤️ 生命值乘数", 0.1f, 3.0f, 1.0f, (v) => {
                BalanceManager.Instance.SetPlayerMultiplier("health", v);
                UpdatePreview();
            }, out _playerHealthSlider));
            
            // Mana
            vbox.AddChild(CreateSliderRow("💧 法力值乘数", 0.1f, 3.0f, 1.0f, (v) => {
                BalanceManager.Instance.SetPlayerMultiplier("mana", v);
                UpdatePreview();
            }, out _playerManaSlider));
            
            // Attack
            vbox.AddChild(CreateSliderRow("⚔️ 攻击力乘数", 0.1f, 3.0f, 1.0f, (v) => {
                BalanceManager.Instance.SetPlayerMultiplier("attack", v);
                UpdatePreview();
            }, out _playerAttackSlider));
            
            // Defense
            vbox.AddChild(CreateSliderRow("🛡️ 防御力乘数", 0.1f, 3.0f, 1.0f, (v) => {
                BalanceManager.Instance.SetPlayerMultiplier("defense", v);
                UpdatePreview();
            }, out _playerDefenseSlider));
            
            // Crit
            vbox.AddChild(CreateSliderRow("⚡ 暴击率乘数", 0.1f, 3.0f, 1.0f, (v) => {
                BalanceManager.Instance.SetPlayerMultiplier("crit", v);
                UpdatePreview();
            }, out _playerCritSlider));
            
            // Dodge
            vbox.AddChild(CreateSliderRow("💨 闪避率乘数", 0.1f, 3.0f, 1.0f, (v) => {
                BalanceManager.Instance.SetPlayerMultiplier("dodge", v);
                UpdatePreview();
            }, out _playerDodgeSlider));
            
            return container;
        }
        
        private VBoxContainer CreateEnemyTab() {
            var container = new VBoxContainer();
            container.Name = "敌人";
            
            var scroll = new ScrollContainer();
            scroll.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            container.AddChild(scroll);
            
            var vbox = new VBoxContainer();
            vbox.CustomMinimumSize = new Vector2(500, 300);
            scroll.AddChild(vbox);
            
            // Health
            vbox.AddChild(CreateSliderRow("❤️ 敌人生命值乘数", 0.1f, 3.0f, 1.0f, (v) => {
                BalanceManager.Instance.SetEnemyMultiplier("health", v);
                UpdatePreview();
            }, out _enemyHealthSlider));
            
            // Damage
            vbox.AddChild(CreateSliderRow("⚔️ 敌人伤害乘数", 0.1f, 3.0f, 1.0f, (v) => {
                BalanceManager.Instance.SetEnemyMultiplier("damage", v);
                UpdatePreview();
            }, out _enemyDamageSlider));
            
            // XP
            vbox.AddChild(CreateSliderRow("⭐ 经验值乘数", 0.1f, 3.0f, 1.0f, (v) => {
                BalanceManager.Instance.SetEnemyMultiplier("xp", v);
                UpdatePreview();
            }, out _enemyXPSlider));
            
            // Drop rate
            vbox.AddChild(CreateSliderRow("💎 掉落率乘数", 0.1f, 3.0f, 1.0f, (v) => {
                BalanceManager.Instance.SetEnemyMultiplier("droprate", v);
                UpdatePreview();
            }, out _enemyDropRateSlider));
            
            return container;
        }
        
        private VBoxContainer CreateCombatTab() {
            var container = new VBoxContainer();
            container.Name = "战斗";
            
            var scroll = new ScrollContainer();
            scroll.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            container.AddChild(scroll);
            
            var vbox = new VBoxContainer();
            vbox.CustomMinimumSize = new Vector2(500, 400);
            scroll.AddChild(vbox);
            
            // Base damage
            vbox.AddChild(CreateSliderRow("💥 基础伤害乘数", 0.1f, 3.0f, 1.0f, (v) => {
                var config = BalanceManager.Instance.GetConfig();
                if (config != null) {
                    config.Combat.BaseDamageMultiplier = v;
                    BalanceManager.Instance.ApplyConfig();
                }
                UpdatePreview();
            }, out _baseDamageSlider));
            
            // Crit chance
            vbox.AddChild(CreateSliderRow("⚡ 暴击几率", 0.0f, 0.5f, 0.05f, (v) => {
                var config = BalanceManager.Instance.GetConfig();
                if (config != null) {
                    config.Combat.CritBaseChance = v;
                    BalanceManager.Instance.ApplyConfig();
                }
                UpdatePreview();
            }, out _critChanceSlider));
            
            // Crit damage
            vbox.AddChild(CreateSliderRow("💥 暴击伤害加成", 0.0f, 2.0f, 0.5f, (v) => {
                var config = BalanceManager.Instance.GetConfig();
                if (config != null) {
                    config.Combat.CritBonusDamage = v;
                    BalanceManager.Instance.ApplyConfig();
                }
                UpdatePreview();
            }, out _critDamageSlider));
            
            // Dodge chance
            vbox.AddChild(CreateSliderRow("💨 闪避几率", 0.0f, 0.5f, 0.05f, (v) => {
                var config = BalanceManager.Instance.GetConfig();
                if (config != null) {
                    config.Combat.DodgeBaseChance = v;
                    BalanceManager.Instance.ApplyConfig();
                }
                UpdatePreview();
            }, out _dodgeChanceSlider));
            
            // Block reduction
            vbox.AddChild(CreateSliderRow("🛡️ 格挡减伤比例", 0.0f, 1.0f, 0.5f, (v) => {
                var config = BalanceManager.Instance.GetConfig();
                if (config != null) {
                    config.Combat.BlockBaseReduction = v;
                    BalanceManager.Instance.ApplyConfig();
                }
                UpdatePreview();
            }, out _blockReductionSlider));
            
            // Counter damage
            vbox.AddChild(CreateSliderRow("🔄 反击伤害倍数", 0.5f, 3.0f, 1.5f, (v) => {
                var config = BalanceManager.Instance.GetConfig();
                if (config != null) {
                    config.Combat.CounterAttackDamage = v;
                    BalanceManager.Instance.ApplyConfig();
                }
                UpdatePreview();
            }, out _counterDamageSlider));
            
            // Combo bonus
            vbox.AddChild(CreateSliderRow("🔥 连击伤害加成", 0.0f, 0.5f, 0.1f, (v) => {
                var config = BalanceManager.Instance.GetConfig();
                if (config != null) {
                    config.Combat.ComboDamageBonus = v;
                    BalanceManager.Instance.ApplyConfig();
                }
                UpdatePreview();
            }, out _comboBonusSlider));
            
            return container;
        }
        
        private VBoxContainer CreateEconomyTab() {
            var container = new VBoxContainer();
            container.Name = "经济";
            
            var scroll = new ScrollContainer();
            scroll.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            container.AddChild(scroll);
            
            var vbox = new VBoxContainer();
            vbox.CustomMinimumSize = new Vector2(500, 200);
            scroll.AddChild(vbox);
            
            // Gold drop
            vbox.AddChild(CreateSliderRow("💰 金币掉落乘数", 0.1f, 5.0f, 1.0f, (v) => {
                var config = BalanceManager.Instance.GetConfig();
                if (config != null) {
                    config.Economy.GoldDropMultiplier = v;
                    BalanceManager.Instance.ApplyConfig();
                }
                UpdatePreview();
            }, out _goldDropSlider));
            
            // Item price
            vbox.AddChild(CreateSliderRow("🏪 物品价格乘数", 0.1f, 3.0f, 1.0f, (v) => {
                var config = BalanceManager.Instance.GetConfig();
                if (config != null) {
                    config.Economy.ItemPriceMultiplier = v;
                    BalanceManager.Instance.ApplyConfig();
                }
                UpdatePreview();
            }, out _itemPriceSlider));
            
            // Quest reward
            vbox.AddChild(CreateSliderRow("📜 任务奖励乘数", 0.1f, 3.0f, 1.0f, (v) => {
                var config = BalanceManager.Instance.GetConfig();
                if (config != null) {
                    config.Economy.QuestRewardMultiplier = v;
                    BalanceManager.Instance.ApplyConfig();
                }
                UpdatePreview();
            }, out _questRewardSlider));
            
            return container;
        }
        
        private VBoxContainer CreateBossTab() {
            var container = new VBoxContainer();
            container.Name = "Boss";
            
            var scroll = new ScrollContainer();
            scroll.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            container.AddChild(scroll);
            
            var vbox = new VBoxContainer();
            vbox.CustomMinimumSize = new Vector2(500, 200);
            scroll.AddChild(vbox);
            
            // Boss health
            vbox.AddChild(CreateSliderRow("❤️ Boss生命值乘数", 0.1f, 3.0f, 1.0f, (v) => {
                var config = BalanceManager.Instance.GetConfig();
                if (config != null) {
                    config.Boss.HealthMultiplier = v;
                    BalanceManager.Instance.ApplyConfig();
                }
                UpdatePreview();
            }, out _bossHealthSlider));
            
            // Boss damage
            vbox.AddChild(CreateSliderRow("⚔️ Boss伤害乘数", 0.1f, 3.0f, 1.0f, (v) => {
                var config = BalanceManager.Instance.GetConfig();
                if (config != null) {
                    config.Boss.DamageMultiplier = v;
                    BalanceManager.Instance.ApplyConfig();
                }
                UpdatePreview();
            }, out _bossDamageSlider));
            
            // Boss enrage time
            vbox.AddChild(CreateSliderRow("⏰ Boss狂暴时间乘数", 0.5f, 2.0f, 1.0f, (v) => {
                var config = BalanceManager.Instance.GetConfig();
                if (config != null) {
                    config.Boss.EnrageTimeMultiplier = v;
                    BalanceManager.Instance.ApplyConfig();
                }
                UpdatePreview();
            }, out _bossEnrageTimeSlider));
            
            return container;
        }
        
        private HBoxContainer CreateSliderRow(string label, float min, float max, float defaultValue, Action<float> onChange, out HSlider slider) {
            var container = new HBoxContainer();
            container.AddThemeConstantOverride("separation", 10);
            
            var labelControl = new Label();
            labelControl.Text = label;
            labelControl.CustomMinimumSize = new Vector2(180, 0);
            container.AddChild(labelControl);
            
            slider = new HSlider();
            slider.MinValue = min;
            slider.MaxValue = max;
            slider.Step = 0.1f;
            slider.Value = defaultValue;
            slider.SizeFlagsHorizontal = Control.SizeFlagsExpand;
            slider.ValueChanged += (v) => {
                onChange?.Invoke(v);
            };
            container.AddChild(slider);
            
            var valueLabel = new Label();
            valueLabel.Text = defaultValue.ToString("F1");
            valueLabel.CustomMinimumSize = new Vector2(40, 0);
            valueLabel.Name = "ValueLabel";
            container.AddChild(valueLabel);
            
            // Update value label when slider changes
            slider.ValueChanged += (v) => {
                var lbl = container.GetNode<Label>("ValueLabel");
                if (lbl != null) {
                    lbl.Text = v.ToString("F1");
                }
            };
            
            // Add reset button
            var resetBtn = new Button();
            resetBtn.Text = "⟲";
            resetBtn.TooltipText = "重置为默认值";
            resetBtn.Pressed += () => {
                slider.Value = defaultValue;
            };
            container.AddChild(resetBtn);
            
            return container;
        }
        
        private void LoadCurrentValues() {
            var config = BalanceManager.Instance.GetConfig();
            if (config == null) return;
            
            // Player
            if (_playerHealthSlider != null) _playerHealthSlider.Value = config.Player.HealthMultiplier;
            if (_playerManaSlider != null) _playerManaSlider.Value = config.Player.ManaMultiplier;
            if (_playerAttackSlider != null) _playerAttackSlider.Value = config.Player.AttackMultiplier;
            if (_playerDefenseSlider != null) _playerDefenseSlider.Value = config.Player.DefenseMultiplier;
            if (_playerCritSlider != null) _playerCritSlider.Value = config.Player.CritChanceMultiplier;
            if (_playerDodgeSlider != null) _playerDodgeSlider.Value = config.Player.DodgeMultiplier;
            
            // Enemy
            if (_enemyHealthSlider != null) _enemyHealthSlider.Value = config.Enemy.HealthMultiplier;
            if (_enemyDamageSlider != null) _enemyDamageSlider.Value = config.Enemy.DamageMultiplier;
            if (_enemyXPSlider != null) _enemyXPSlider.Value = config.Enemy.XPMultiplier;
            if (_enemyDropRateSlider != null) _enemyDropRateSlider.Value = config.Enemy.DropRateMultiplier;
            
            // Combat
            if (_baseDamageSlider != null) _baseDamageSlider.Value = config.Combat.BaseDamageMultiplier;
            if (_critChanceSlider != null) _critChanceSlider.Value = config.Combat.CritBaseChance;
            if (_critDamageSlider != null) _critDamageSlider.Value = config.Combat.CritBonusDamage;
            if (_dodgeChanceSlider != null) _dodgeChanceSlider.Value = config.Combat.DodgeBaseChance;
            if (_blockReductionSlider != null) _blockReductionSlider.Value = config.Combat.BlockBaseReduction;
            if (_counterDamageSlider != null) _counterDamageSlider.Value = config.Combat.CounterAttackDamage;
            if (_comboBonusSlider != null) _comboBonusSlider.Value = config.Combat.ComboDamageBonus;
            
            // Economy
            if (_goldDropSlider != null) _goldDropSlider.Value = config.Economy.GoldDropMultiplier;
            if (_itemPriceSlider != null) _itemPriceSlider.Value = config.Economy.ItemPriceMultiplier;
            if (_questRewardSlider != null) _questRewardSlider.Value = config.Economy.QuestRewardMultiplier;
            
            // Boss
            if (_bossHealthSlider != null) _bossHealthSlider.Value = config.Boss.HealthMultiplier;
            if (_bossDamageSlider != null) _bossDamageSlider.Value = config.Boss.DamageMultiplier;
            if (_bossEnrageTimeSlider != null) _bossEnrageTimeSlider.Value = config.Boss.EnrageTimeMultiplier;
        }
        
        private void ApplyPreset(string preset) {
            BalanceManager.Instance.ApplyDifficultyPreset(preset);
            LoadCurrentValues();
            UpdatePreview();
            ShowNotification($"已应用 {preset} 难度预设");
        }
        
        private void SaveConfig() {
            BalanceManager.Instance.SaveConfig();
            ShowNotification("✅ 配置已保存");
        }
        
        private void ResetToDefault() {
            BalanceManager.Instance.ApplyDifficultyPreset("normal");
            LoadCurrentValues();
            UpdatePreview();
            ShowNotification("🔄 已重置为默认配置");
        }
        
        private void ExportConfig() {
            var json = BalanceManager.Instance.ExportConfigAsJson();
            // Copy to clipboard (for Godot 4.x)
            DisplayServer.ClipboardSet(json);
            ShowNotification("📤 配置已复制到剪贴板");
        }
        
        private void ImportConfig() {
            var json = DisplayServer.ClipboardGet();
            if (!string.IsNullOrEmpty(json)) {
                BalanceManager.Instance.ImportConfigFromJson(json);
                LoadCurrentValues();
                UpdatePreview();
                ShowNotification("📥 配置已从剪贴板导入");
            }
        }
        
        private void UpdatePreview() {
            _previewLabel.Text = $"💡 调整已实时生效 - {DateTime.Now:HH:mm:ss}";
        }
        
        private void ShowNotification(string message) {
            GD.Print($"[BalanceUI] {message}");
            // Could add a toast notification here
        }
        
        public void ToggleVisibility() {
            if (_isVisible) {
                Hide();
                _isVisible = false; 
            } else {
                Show();
                LoadCurrentValues();
                _isVisible = true;
            }
        }
        
        public override void _Input(InputEvent evt) {
            if (evt is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == keyToggle) {
                ToggleVisibility();
            }
        }
        
        public bool IsVisible() => _isVisible;
    }
}
