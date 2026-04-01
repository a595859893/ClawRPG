using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 战斗视觉特效 UI - 显示特效系统设置和统计信息
    /// </summary>
    public partial class CombatVFXUI : Control {
        public static CombatVFXUI Instance { get; private set; }
        
        private CombatVFXSystem vfxSystem;
        private bool isVisible = false;
        
        // UI 组件
        private PanelContainer mainPanel;
        private VBoxContainer mainVBox;
        private TabContainer tabContainer;
        
        // 设置面板组件
        private CheckBox enableScreenEffectsCheck;
        private CheckBox enableCriticalGlowCheck;
        private CheckBox enableComboEffectsCheck;
        private Slider damageNumberLifetimeSlider;
        private Label damageNumberLifetimeLabel;
        private Slider maxDamageNumbersSlider;
        private Label maxDamageNumbersLabel;
        
        // 统计面板组件
        private Label totalDamageNumbersLabel;
        private Label criticalHitsLabel;
        private Label healsLabel;
        private Label blocksLabel;
        private Label dodgesLabel;
        private Label maxComboLabel;
        private Label screenEffectsLabel;
        private Label vfxPlayedLabel;
        
        public override void _Ready() {
            Instance = this;
            vfxSystem = CombatVFXSystem.Instance;
            
            SetupUI();
            Hide();
        }
        
        private void SetupUI() {
            // 主面板
            mainPanel = new PanelContainer();
            mainPanel.SetAnchorsPreset(Control.LayoutPreset.CenterRight);
            mainPanel.OffsetLeft = -400;
            mainPanel.OffsetRight = -50;
            mainPanel.OffsetTop = 50;
            mainPanel.OffsetBottom = -50;
            mainPanel.CustomMinimumSize = new Vector2(350, 0);
            
            // 样式
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(8);
            mainPanel.AddThemeStyleboxOverride("panel", style);
            
            AddChild(mainPanel);
            
            // 主容器
            mainVBox = new VBoxContainer();
            mainVBox.Setanchorspreset(Control.LayoutPreset.FullRect);
            mainVBox.AddThemeConstantOverride("separation", 10);
            mainPanel.AddChild(mainVBox);
            
            // 标题
            var title = new Label();
            title.Text = "⚔️ Combat VFX Settings";
            title.AddThemeFontSizeOverride("font_size", 20);
            title.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.7f));
            title.HorizontalAlignment = HorizontalAlignment.Center;
            mainVBox.AddChild(title);
            
            // 分隔线
            var hsep1 = new HSeparator();
            hsep1.AddThemeColorOverride("separator", new Color(0.4f, 0.4f, 0.5f));
            mainVBox.AddChild(hsep1);
            
            // Tab 容器
            tabContainer = new TabContainer();
            tabContainer.Settabsposition(TabContainer.TabsPosition.Top);
            tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            mainVBox.AddChild(tabContainer);
            
            // 创建设置和统计标签页
            CreateSettingsTab();
            CreateStatisticsTab();
            
            // 快捷键提示
            var hintLabel = new Label();
            hintLabel.Text = "Press V or ESC to close";
            hintLabel.AddThemeFontSizeOverride("font_size", 12);
            hintLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.7f));
            hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
            mainVBox.AddChild(hintLabel);
        }
        
        private void CreateSettingsTab() {
            var settingsScroll = new ScrollContainer();
            settingsScroll.Name = "Settings";
            tabContainer.AddChild(settingsScroll);
            
            var settingsVBox = new VBoxContainer();
            settingsVBox.AddThemeConstantOverride("separation", 15);
            settingsScroll.AddChild(settingsVBox);
            
            // 屏幕特效开关
            enableScreenEffectsCheck = new CheckBox();
            enableScreenEffectsCheck.Text = "Enable Screen Effects";
            enableScreenEffectsCheck.ButtonPressed = vfxSystem != null && vfxSystem.EnableScreenEffects;
            enableScreenEffectsCheck.Toggled += OnEnableScreenEffectsToggled;
            settingsVBox.AddChild(enableScreenEffectsCheck);
            
            // 暴击光效开关
            enableCriticalGlowCheck = new CheckBox();
            enableCriticalGlowCheck.Text = "Enable Critical Glow";
            enableCriticalGlowCheck.ButtonPressed = vfxSystem != null && vfxSystem.EnableCriticalGlow;
            enableCriticalGlowCheck.Toggled += OnEnableCriticalGlowToggled;
            settingsVBox.AddChild(enableCriticalGlowCheck);
            
            // 连击特效开关
            enableComboEffectsCheck = new CheckBox();
            enableComboEffectsCheck.Text = "Enable Combo Effects";
            enableComboEffectsCheck.ButtonPressed = vfxSystem != null && vfxSystem.EnableComboEffects;
            enableComboEffectsCheck.Toggled += OnEnableComboEffectsToggled;
            settingsVBox.AddChild(enableComboEffectsCheck);
            
            // 分隔
            var sep1 = new HSeparator();
            sep1.AddThemeColorOverride("separator", new Color(0.3f, 0.3f, 0.4f));
            settingsVBox.AddChild(sep1);
            
            // 伤害数字持续时间
            var lifetimeLabel = new Label();
            lifetimeLabel.Text = "Damage Number Lifetime:";
            lifetimeLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1f));
            settingsVBox.AddChild(lifetimeLabel);
            
            damageNumberLifetimeSlider = new HSlider();
            damageNumberLifetimeSlider.MinValue = 0.5f;
            damageNumberLifetimeSlider.MaxValue = 3f;
            damageNumberLifetimeSlider.Step = 0.1f;
            damageNumberLifetimeSlider.Value = vfxSystem != null ? vfxSystem.DamageNumberLifetime : 1.5f;
            damageNumberLifetimeSlider.ValueChanged += OnDamageNumberLifetimeChanged;
            settingsVBox.AddChild(damageNumberLifetimeSlider);
            
            damageNumberLifetimeLabel = new Label();
            damageNumberLifetimeLabel.Text = $"{damageNumberLifetimeSlider.Value:F1}s";
            damageNumberLifetimeLabel.HorizontalAlignment = HorizontalAlignment.Center;
            settingsVBox.AddChild(damageNumberLifetimeLabel);
            
            // 最大伤害数字数量
            var maxLabel = new Label();
            maxLabel.Text = "Max Damage Numbers:";
            maxLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1f));
            settingsVBox.AddChild(maxLabel);
            
            maxDamageNumbersSlider = new HSlider();
            maxDamageNumbersSlider.MinValue = 10;
            maxDamageNumbersSlider.MaxValue = 100;
            maxDamageNumbersSlider.Step = 5;
            maxDamageNumbersSlider.Value = vfxSystem != null ? vfxSystem.MaxDamageNumbers : 50;
            maxDamageNumbersSlider.ValueChanged += OnMaxDamageNumbersChanged;
            settingsVBox.AddChild(maxDamageNumbersSlider);
            
            maxDamageNumbersLabel = new Label();
            maxDamageNumbersLabel.Text = maxDamageNumbersSlider.Value.ToString();
            maxDamageNumbersLabel.HorizontalAlignment = HorizontalAlignment.Center;
            settingsVBox.AddChild(maxDamageNumbersLabel);
            
            // 重置按钮
            var resetButton = new Button();
            resetButton.Text = "Reset to Defaults";
            resetButton.Pressed += OnResetPressed;
            settingsVBox.AddChild(resetButton);
        }
        
        private void CreateStatisticsTab() {
            var statsScroll = new ScrollContainer();
            statsScroll.Name = "Statistics";
            tabContainer.AddChild(statsScroll);
            
            var statsVBox = new VBoxContainer();
            statsVBox.AddThemeConstantOverride("separation", 10);
            statsScroll.AddChild(statsVBox);
            
            // 标题
            var statsTitle = new Label();
            statsTitle.Text = "⚔️ Combat VFX Statistics";
            statsTitle.AddThemeFontSizeOverride("font_size", 18);
            statsTitle.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.7f));
            statsTitle.HorizontalAlignment = HorizontalAlignment.Center;
            statsVBox.AddChild(statsTitle);
            
            // 分隔线
            var hsep = new HSeparator();
            hsep.AddThemeColorOverride("separator", new Color(0.4f, 0.4f, 0.5f));
            statsVBox.AddChild(hsep);
            
            // 统计数据标签
            totalDamageNumbersLabel = CreateStatLabel("Total Damage Numbers:", "0", statsVBox);
            criticalHitsLabel = CreateStatLabel("Critical Hits:", "0", statsVBox);
            healsLabel = CreateStatLabel("Heals:", "0", statsVBox);
            blocksLabel = CreateStatLabel("Blocks:", "0", statsVBox);
            dodgesLabel = CreateStatLabel("Dodges:", "0", statsVBox);
            maxComboLabel = CreateStatLabel("Max Combo:", "0", statsVBox);
            screenEffectsLabel = CreateStatLabel("Screen Effects:", "0", statsVBox);
            vfxPlayedLabel = CreateStatLabel("VFX Played:", "0", statsVBox);
            
            // 更新按钮
            var updateButton = new Button();
            updateButton.Text = "Refresh Statistics";
            updateButton.Pressed += OnRefreshStatsPressed;
            statsVBox.AddChild(updateButton);
            
            // 清除统计按钮
            var clearButton = new Button();
            clearButton.Text = "Clear Statistics";
            clearButton.Pressed += OnClearStatsPressed;
            statsVBox.AddChild(clearButton);
        }
        
        private Label CreateStatLabel(string labelText, string valueText, VBoxContainer parent) {
            var hbox = new HBoxContainer();
            
            var label = new Label();
            label.Text = labelText;
            label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            label.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
            hbox.AddChild(label);
            
            var value = new Label();
            value.Text = valueText;
            value.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.6f));
            value.HorizontalAlignment = HorizontalAlignment.Right;
            hbox.AddChild(value);
            
            parent.AddChild(hbox);
            
            return value;
        }
        
        public override void _Process(double delta) {
            if (isVisible && vfxSystem != null) {
                UpdateStatistics();
            }
        }
        
        private void UpdateStatistics() {
            var stats = vfxSystem.GetStatistics();
            
            totalDamageNumbersLabel.Text = stats.GetValueOrDefault("TotalDamageNumbers", 0).ToString();
            criticalHitsLabel.Text = stats.GetValueOrDefault("CriticalHits", 0).ToString();
            healsLabel.Text = stats.GetValueOrDefault("Heals", 0).ToString();
            blocksLabel.Text = stats.GetValueOrDefault("Blocks", 0).ToString();
            dodgesLabel.Text = stats.GetValueOrDefault("Dodges", 0).ToString();
            maxComboLabel.Text = stats.GetValueOrDefault("MaxCombo", 0).ToString();
            screenEffectsLabel.Text = stats.GetValueOrDefault("ScreenEffects", 0).ToString();
            vfxPlayedLabel.Text = stats.GetValueOrDefault("VFXPlayed", 0).ToString();
        }
        
        #region 信号处理
        
        private void OnEnableScreenEffectsToggled(bool toggledOn) {
            if (vfxSystem != null) {
                vfxSystem.EnableScreenEffects = toggledOn;
            }
        }
        
        private void OnEnableCriticalGlowToggled(bool toggledOn) {
            if (vfxSystem != null) {
                vfxSystem.EnableCriticalGlow = toggledOn;
            }
        }
        
        private void OnEnableComboEffectsToggled(bool toggledOn) {
            if (vfxSystem != null) {
                vfxSystem.EnableComboEffects = toggledOn;
            }
        }
        
        private void OnDamageNumberLifetimeChanged(double value) {
            damageNumberLifetimeLabel.Text = $"{value:F1}s";
            if (vfxSystem != null) {
                vfxSystem.DamageNumberLifetime = (float)value;
            }
        }
        
        private void OnMaxDamageNumbersChanged(double value) {
            maxDamageNumbersLabel.Text = value.ToString();
            if (vfxSystem != null) {
                vfxSystem.MaxDamageNumbers = (int)value;
            }
        }
        
        private void OnResetPressed() {
            enableScreenEffectsCheck.ButtonPressed = true;
            enableCriticalGlowCheck.ButtonPressed = true;
            enableComboEffectsCheck.ButtonPressed = true;
            damageNumberLifetimeSlider.Value = 1.5f;
            maxDamageNumbersSlider.Value = 50;
        }
        
        private void OnRefreshStatsPressed() {
            UpdateStatistics();
        }
        
        private void OnClearStatsPressed() {
            // 清除统计数据（重新创建系统实例会重置）
            if (vfxSystem != null) {
                vfxSystem.PlayerData = new Combat.PlayerCombatVFXData();
            }
            UpdateStatistics();
        }
        
        #endregion
        
        #region 显示/隐藏
        
        public override void _Input(InputEvent e) {
            if (e is InputEventKey keyEvent && keyEvent.Pressed) {
                if (keyEvent.Keycode == Key.V || keyEvent.Keycode == Key.Escape) {
                    Toggle();
                }
            }
        }
        
        public void Toggle() {
            if (isVisible) {
                Hide();
            } else {
                Show();
            }
        }
        
        public void OnVisibleToggled(bool visibleNode) {
            isVisible = visibleNode;
        }
        
        #endregion
    }
    
    // 扩展 CombatVFXSystem 的属性访问
    public partial class CombatVFXSystem {
        public bool EnableScreenEffects {
            get => enableScreenEffects;
            set => enableScreenEffects = value;
        }
        
        public bool EnableCriticalGlow {
            get => enableCriticalGlow;
            set => enableCriticalGlow = value;
        }
        
        public bool EnableComboEffects {
            get => enableComboEffects;
            set => enableComboEffects = value;
        }
        
        public float DamageNumberLifetime {
            get => damageNumberLifetime;
            set => damageNumberLifetime = value;
        }
        
        public int MaxDamageNumbers {
            get => maxDamageNumbers;
            set => maxDamageNumbers = value;
        }
    }
}
