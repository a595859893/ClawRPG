using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Combat;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// Combat status UI - displays real-time combat statistics
    /// </summary>
    public partial class CombatStatusUI : Control
    {
        private Control _mainPanel;
        private VBoxContainer _statsContainer;
        private VBoxContainer _eventsContainer;
        
        // Stats labels
        private Label _dpsLabel;
        private Label _damageDealtLabel;
        private Label _damageTakenLabel;
        private Label _healingLabel;
        private Label _comboLabel;
        private Label _critLabel;
        private Label _killsLabel;
        private Label _gradeLabel;
        
        // Buff display
        private HBoxContainer _buffContainer;
        
        // Toggle visibility
        private bool _isVisible = false; 
        
        // Auto-hide timer
        private float _autoHideTimer = 0;
        private const float AUTO_HIDE_TIME = 10.0f;
        
        public override void _Ready()
        {
            SetupUI();
            ConnectSignals();
            Hide();
        }

        private void SetupUI()
        {
            // Main panel
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchor(AnchorPresets.RightWide);
            _mainPanel.OffsetLeft = -320;
            _mainPanel.OffsetTop = 150;
            _mainPanel.OffsetRight = -20;
            _mainPanel.OffsetBottom = -150;
            _mainPanel.CustomMinimumSize = new Vector2(300, 0);
            AddChild(_mainPanel);
            
            // Create style
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            style.BorderWidthLeft = 2;
            style.BorderWidthTop = 2;
            style.BorderWidthRight = 2;
            style.BorderWidthBottom = 2;
            style.BorderColor = new Color(0.3f, 0.5f, 0.8f, 0.8f);
            style.CornerRadiusTopLeft = 8;
            style.CornerRadiusTopRight = 8;
            style.CornerRadiusBottomLeft = 8;
            style.CornerRadiusBottomRight = 8;
            _mainPanel.AddThemeStyleboxOverride("panel", style);
            
            // Main VBox
            var mainVBox = new VBoxContainer();
            mainVBox.AddThemeConstantOverride("separation", 8);
            _mainPanel.AddChild(mainVBox);
            
            // Title
            var titleLabel = new Label();
            titleLabel.Text = "⚔️ 战斗状态";
            titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            titleLabel.AddThemeFontSizeOverride("font_size", 18);
            titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.4f));
            mainVBox.AddChild(titleLabel);
            
            // Stats container
            _statsContainer = new VBoxContainer();
            _statsContainer.AddThemeConstantOverride("separation", 4);
            mainVBox.AddChild(_statsContainer);
            
            // DPS
            _dpsLabel = CreateStatLabel("DPS: 0", new Color(1f, 0.6f, 0.2f));
            _statsContainer.AddChild(_dpsLabel);
            
            // Damage dealt
            _damageDealtLabel = CreateStatLabel("伤害输出: 0", new Color(0.3f, 1f, 0.3f));
            _statsContainer.AddChild(_damageDealtLabel);
            
            // Damage taken
            _damageTakenLabel = CreateStatLabel("伤害承受: 0", new Color(1f, 0.3f, 0.3f));
            _statsContainer.AddChild(_damageTakenLabel);
            
            // Healing
            _healingLabel = CreateStatLabel("治疗量: 0", new Color(0.3f, 0.8f, 1f));
            _statsContainer.AddChild(_healingLabel);
            
            // Combo
            _comboLabel = CreateStatLabel("连击: 0", new Color(1f, 0.85f, 0.3f));
            _statsContainer.AddChild(_comboLabel);
            
            // Critical hits
            _critLabel = CreateStatLabel("暴击: 0", new Color(1f, 0.5f, 0.8f));
            _statsContainer.AddChild(_critLabel);
            
            // Kills
            _killsLabel = CreateStatLabel("击杀: 0", new Color(0.8f, 0.6f, 1f));
            _statsContainer.AddChild(_killsLabel);
            
            // Grade
            _gradeLabel = CreateStatLabel("评价: -", new Color(1f, 1f, 1f));
            _gradeLabel.AddThemeFontSizeOverride("font_size", 20);
            _statsContainer.AddChild(_gradeLabel);
            
            // Separator
            var separator = new HSeparator();
            separator.AddThemeColorOverride("separator", new Color(0.3f, 0.3f, 0.4f));
            mainVBox.AddChild(separator);
            
            // Buff container
            var buffLabel = new Label();
            buffLabel.Text = "状态效果";
            buffLabel.AddThemeFontSizeOverride("font_size", 14);
            buffLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
            mainVBox.AddChild(buffLabel);
            
            _buffContainer = new HBoxContainer();
            _buffContainer.AddThemeConstantOverride("separation", 4);
            mainVBox.AddChild(_buffContainer);
            
            // Separator
            var separator2 = new HSeparator();
            separator2.AddThemeColorOverride("separator", new Color(0.3f, 0.3f, 0.4f));
            mainVBox.AddChild(separator2);
            
            // Recent events
            var eventsLabel = new Label();
            eventsLabel.Text = "最近事件";
            eventsLabel.AddThemeFontSizeOverride("font_size", 14);
            eventsLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
            mainVBox.AddChild(eventsLabel);
            
            // Events scroll
            var scrollContainer = new ScrollContainer();
            scrollContainer.CustomMinimumSize = new Vector2(0, 100);
            mainVBox.AddChild(scrollContainer);
            
            _eventsContainer = new VBoxContainer();
            _eventsContainer.AddThemeConstantOverride("separation", 2);
            scrollContainer.AddChild(_eventsContainer);
            
            // Close hint
            var hintLabel = new Label();
            hintLabel.Text = "按 [ 键关闭";
            hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
            hintLabel.AddThemeFontSizeOverride("font_size", 12);
            hintLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.6f));
            mainVBox.AddChild(hintLabel);
        }

        private Label CreateStatLabel(string text, Color color)
        {
            var label = new Label();
            label.Text = text;
            label.AddThemeFontSizeOverride("font_size", 14);
            label.AddThemeColorOverride("font_color", color);
            return label;
        }

        private void ConnectSignals()
        {
            CombatStatusSystem.OnStatsUpdated += UpdateStats;
            CombatStatusSystem.OnComboChanged += UpdateCombo;
            CombatStatusSystem.OnCombatEvent += AddCombatEvent;
            CombatStatusSystem.OnCombatEnded += OnCombatEnded;
            // REQ-173: Combo milestone celebrations
            CombatStatusSystem.OnComboMilestone += OnComboMilestone;
            // REQ-173: Personal best notifications
            CombatStatusSystem.OnPersonalBestBroken += OnPersonalBestBroken;
        }

        public override void _Input(InputEvent e)
        {
            if (e is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.Bracketleft || keyEvent.Keycode == Key.Bracketright)
                {
                    ToggleVisibility();
                }
            }
        }

        public override void _Process(double delta)
        {
            var system = CombatStatusSystem.Instance;
            
            // Update DPS display
            float currentDPS = system.GetCurrentDPS();
            _dpsLabel.Text = $"DPS: {currentDPS:F1}";
            
            // Auto-hide when not in combat
            var status = system.GetCurrentCombatStatus();
            if (!status.IsInCombat && _isVisible)
            {
                _autoHideTimer += (float)delta;
                if (_autoHideTimer >= AUTO_HIDE_TIME)
                {
                    Hide();
                }
            }
            else
            {
                _autoHideTimer = 0;
            }
        }

        private void ToggleVisibility()
        {
            if (_isVisible)
            {
                Hide();
            }
            else
            {
                Show();
                UpdateStats();
            }
        }

        private void Show()
        {
            _isVisible = true;
            _autoHideTimer = 0;
            Visible = true;
        }

        private void Hide()
        {
            _isVisible = false; 
            Visible = false; 
        }

        private void UpdateStats()
        {
            var system = CombatStatusSystem.Instance;
            var status = system.GetCurrentCombatStatus();
            
            _damageDealtLabel.Text = $"伤害输出: {status.TotalDamageDealt:F0}";
            _damageTakenLabel.Text = $"伤害承受: {status.TotalDamageTaken:F0}";
            _healingLabel.Text = $"治疗量: {status.TotalHealingDone:F0}";
            _critLabel.Text = $"暴击: {status.CriticalHits}";
            _killsLabel.Text = $"击杀: {status.EnemiesKilled}";
            
            // Update grade
            var grade = system.CalculateCombatGrade();
            Color gradeColor = grade switch
            {
                CombatStatusData.CombatGrade.S => new Color(1f, 0.85f, 0.3f),
                CombatStatusData.CombatGrade.A => new Color(0.3f, 1f, 0.5f),
                CombatStatusData.CombatGrade.B => new Color(0.3f, 0.8f, 1f),
                CombatStatusData.CombatGrade.C => new Color(0.8f, 0.6f, 0.3f),
                _ => new Color(0.6f, 0.6f, 0.6f)
            };
            
            _gradeLabel.Text = $"评价: {grade}";
            _gradeLabel.AddThemeColorOverride("font_color", gradeColor);
        }

        private void UpdateCombo()
        {
            var status = CombatStatusSystem.Instance.GetCurrentCombatStatus();
            _comboLabel.Text = $"连击: {status.CurrentCombo}x";
            
            // REQ-173: Specific milestone celebration effects
            if (status.CurrentCombo == 10 || status.CurrentCombo == 25 || 
                status.CurrentCombo == 50 || status.CurrentCombo == 100) {
                // Large pulse effect for major milestones
                var tween = CreateTween();
                tween.TweenProperty(_comboLabel, "scale", new Vector2(1.8f, 1.8f), 0.15f);
                tween.TweenProperty(_comboLabel, "scale", new Vector2(1f, 1f), 0.25f);
                
                // Color flash
                var origColor = _comboLabel.GetThemeColor("font_color");
                _comboLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.3f, 1f));
                tween.TweenCallback(Callable.From(() => {
                    _comboLabel.AddThemeColorOverride("font_color", origColor);
                }));
            }
        }
        
        /// <summary>
        /// REQ-173: Combo milestone celebration — called at specific combo values (10x, 25x, 50x).
        /// </summary>
        private void OnComboMilestone(int combo) {
            // Big screen flash effect for milestones
            var flash = new ColorRect();
            flash.Color = new Color(1f, 0.9f, 0.5f, 0.15f);
            flash.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(flash);
            
            var tween = CreateTween();
            tween.TweenProperty(flash, "color:a", 0f, 0.4f);
            tween.TweenCallback(Callable.From(() => flash.QueueFree()));
            
            GD.Print($"[CombatStatusUI] Combo milestone: {combo}x!");
        }
        
        /// <summary>
        /// REQ-173: Personal best broken notification.
        /// </summary>
        private void OnPersonalBestBroken(string statName, float newValue, float previousValue) {
            // Create a floating "NEW RECORD!" notification
            var notification = new Label();
            notification.Text = $"新纪录! {statName}: {newValue:F0}";
            notification.GlobalPosition = new Vector2(200, 100);
            notification.AddThemeFontSizeOverride("font_size", 24);
            notification.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0.2f, 1f));
            GetTree().Root.AddChild(notification);
            
            var tween = CreateTween();
            tween.TweenProperty(notification, "position:y", notification.Position.Y - 80, 1.5f);
            tween.TweenProperty(notification, "modulate:a", 0f, 0.5f);
            tween.TweenCallback(Callable.From(() => notification.QueueFree()));
            
            GD.Print($"[CombatStatusUI] Personal best broken: {statName} = {newValue:F0} (prev: {previousValue:F0})");
        }

        private void AddCombatEvent(CombatStatusData.CombatEvent combatEvent)
        {
            if (!_isVisible) return;
            
            // Add event to list
            var eventLabel = new Label();
            eventLabel.Text = combatEvent.Description;
            eventLabel.AddThemeFontSizeOverride("font_size", 12);
            
            // Color based on event type
            Color eventColor = combatEvent.Type switch
            {
                CombatStatusData.CombatEventType.DamageDealt => new Color(0.6f, 1f, 0.6f),
                CombatStatusData.CombatEventType.DamageTaken => new Color(1f, 0.6f, 0.6f),
                CombatStatusData.CombatEventType.HealingDone => new Color(0.6f, 0.9f, 1f),
                CombatStatusData.CombatEventType.CriticalHit => new Color(1f, 0.8f, 0.4f),
                CombatStatusData.CombatEventType.EnemyKilled => new Color(0.8f, 0.7f, 1f),
                CombatStatusData.CombatEventType.BossDamage => new Color(1f, 0.3f, 0.3f),
                CombatStatusData.CombatEventType.Dodge => new Color(0.5f, 1f, 0.5f),
                CombatStatusData.CombatEventType.Block => new Color(0.7f, 0.7f, 1f),
                _ => new Color(0.8f, 0.8f, 0.8f)
            };
            
            if (combatEvent.IsCritical)
            {
                eventColor = new Color(1f, 0.6f, 0.2f);
                eventLabel.AddThemeFontSizeOverride("font_size", 13);
            }
            
            eventLabel.AddThemeColorOverride("font_color", eventColor);
            
            _eventsContainer.AddChild(eventLabel);
            
            // Keep only last 10 events
            while (_eventsContainer.GetChildCount() > 10)
            {
                _eventsContainer.GetChild(0).QueueFree();
            }
            
            // Scroll to bottom
            var scroll = _eventsContainer.GetParent() as ScrollContainer;
            if (scroll != null)
            {
                scroll.ScrollVertical = (int)scroll.GetVScrollBar().MaxValue;
            }
            
            // Show panel when event occurs
            if (!_isVisible)
            {
                Show();
            }
            _autoHideTimer = 0;
        }

        private void OnCombatEnded()
        {
            // Show final stats briefly
            UpdateStats();
            
            // Flash effect
            var tween = CreateTween();
            tween.TweenProperty(_mainPanel, "modulate", new Color(1f, 1f, 1f, 0.5f), 0.1f);
            tween.TweenProperty(_mainPanel, "modulate", new Color(1f, 1f, 1f, 1f), 0.2f);
        }

        public override void _ExitTree()
        {
            CombatStatusSystem.OnStatsUpdated -= UpdateStats;
            CombatStatusSystem.OnComboChanged -= UpdateCombo;
            CombatStatusSystem.OnCombatEvent -= AddCombatEvent;
            CombatStatusSystem.OnCombatEnded -= OnCombatEnded;
            // REQ-173
            CombatStatusSystem.OnComboMilestone -= OnComboMilestone;
            CombatStatusSystem.OnPersonalBestBroken -= OnPersonalBestBroken;
        }
    }
}
