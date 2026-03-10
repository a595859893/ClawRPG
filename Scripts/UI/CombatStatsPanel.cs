using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Real-time combat statistics display panel
    /// Shows damage dealt, time, kills, damage taken, dodges, blocks, etc.
    /// </summary>
    public partial class CombatStatsPanel : Control
    {
        private static CombatStatsPanel _instance;
        public static CombatStatsPanel Instance => _instance;
        
        [Export] private bool _autoShowInCombat = true;
        [Export] private float _updateInterval = 0.5f;
        
        // Stats labels
        private Label _damageDealtLabel;
        private Label _damageTakenLabel;
        private Label _killsLabel;
        private Label _combatTimeLabel;
        private Label _dodgesLabel;
        private Label _blocksLabel;
        private Label _critsLabel;
        private Label _comboLabel;
        
        // Combat tracking
        private int _totalDamageDealt = 0;
        private int _totalDamageTaken = 0;
        private int _totalKills = 0;
        private int _totalDodges = 0;
        private int _totalBlocks = 0;
        private int _totalCrits = 0;
        private int _maxCombo = 0;
        private float _combatStartTime = 0;
        private bool _inCombat = false;
        
        private float _lastUpdate = 0;
        private PanelContainer _mainPanel;
        private VBoxContainer _statsContainer;
        private Tween _pulseTween;
        
        public override void _Ready()
        {
            _instance = this;
            AddToGroup("CombatStatsPanel");
            SetupUI();
            ConnectSignals();
            Hide();
        }
        
        private void SetupUI()
        {
            Name = "CombatStatsPanel";
            AnchorRight = 0f;
            AnchorBottom = 0f;
            OffsetLeft = 20;
            OffsetTop = 300;
            OffsetRight = 220;
            OffsetBottom = 550;
            
            // Main panel
            _mainPanel = new PanelContainer
            {
                Name = "MainPanel",
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 0,
                OffsetTop = 0,
                OffsetRight = 0,
                OffsetBottom = 0
            };
            AddChild(_mainPanel);
            
            // Style
            var panelStyle = new StyleBoxFlat();
            panelStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            panelStyle.CornerRadiusTopLeft = 8;
            panelStyle.CornerRadiusTopRight = 8;
            panelStyle.CornerRadiusBottomLeft = 8;
            panelStyle.CornerRadiusBottomRight = 8;
            panelStyle.BorderWidthLeft = 2;
            panelStyle.BorderWidthTop = 2;
            panelStyle.BorderWidthRight = 2;
            panelStyle.BorderWidthBottom = 2;
            panelStyle.BorderColor = new Color(0.4f, 0.3f, 0.2f, 0.8f);
            _mainPanel.AddThemeStyleBoxOverride("panel", panelStyle);
            
            // Stats container
            _statsContainer = new VBoxContainer
            {
                Name = "StatsContainer",
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = -10,
                OffsetBottom = -10,
                Theme = CreateTheme()
            };
            _mainPanel.AddChild(_statsContainer);
            
            // Title
            var titleLabel = new Label
            {
                Text = "⚔️ 战斗统计",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            titleLabel.AddThemeFontSizeOverride("font_size", 18);
            titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.4f, 1f));
            _statsContainer.AddChild(titleLabel);
            
            // Separator
            AddSeparator();
            
            // Create stat rows
            _damageDealtLabel = AddStatRow("造成伤害", "0", new Color(1f, 0.4f, 0.4f, 1f));
            _damageTakenLabel = AddStatRow("受到伤害", "0", new Color(0.4f, 0.6f, 1f, 1f));
            _killsLabel = AddStatRow("击杀敌人", "0", new Color(0.4f, 1f, 0.5f, 1f));
            _combatTimeLabel = AddStatRow("战斗时间", "0:00", new Color(1f, 0.9f, 0.5f, 1f));
            _dodgesLabel = AddStatRow("闪避次数", "0", new Color(0.5f, 0.8f, 1f, 1f));
            _blocksLabel = AddStatRow("格挡次数", "0", new Color(0.8f, 0.6f, 1f, 1f));
            _critsLabel = AddStatRow("暴击次数", "0", new Color(1f, 0.5f, 0.8f, 1f));
            _comboLabel = AddStatRow("最高连击", "0", new Color(1f, 0.85f, 0.2f, 1f));
            
            // Connect signals
            ConnectCombatSignals();
        }
        
        private Theme CreateTheme()
        {
            var theme = new Theme();
            theme.SetFontSize("font_size", 14);
            return theme;
        }
        
        private Label AddStatRow(string label, string value, Color valueColor)
        {
            var container = new HBoxContainer
            {
                Alignment = BoxContainer.AlignmentMode.Center,
                CustomMinimumHeight = 24
            };
            _statsContainer.AddChild(container);
            
            var labelControl = new Label
            {
                Text = label + ":",
                HorizontalAlignment = HorizontalAlignment.Left,
                SizeFlagsHorizontal = SizeFlags.Expand
            };
            labelControl.AddThemeFontSizeOverride("font_size", 13);
            labelControl.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f, 1f));
            container.AddChild(labelControl);
            
            var valueControl = new Label
            {
                Text = value,
                HorizontalAlignment = HorizontalAlignment.Right,
                SizeFlagsHorizontal = SizeFlags.ShrinkEnd
            };
            valueControl.AddThemeFontSizeOverride("font_size", 14);
            valueControl.AddThemeColorOverride("font_color", valueColor);
            container.AddChild(valueControl);
            
            return valueControl;
        }
        
        private void AddSeparator()
        {
            var separator = new HSeparator
            {
                Modulate = new Color(0.4f, 0.3f, 0.2f, 0.5f),
                CustomMinimumHeight = 1
            };
            _statsContainer.AddChild(separator);
        }
        
        private void ConnectSignals()
        {
            // Connect to combat events via Main
            var main = GetNode("/root/Main");
            if (main != null)
            {
                main.Connect("enemy_damaged", new Callable(this, nameof(OnEnemyDamaged)));
                main.Connect("player_damaged", new Callable(this, nameof(OnPlayerDamaged)));
                main.Connect("enemy_killed", new Callable(this, nameof(OnEnemyKilled)));
                main.Connect("player_dodged", new Callable(this, nameof(OnPlayerDodged)));
                main.Connect("player_blocked", new Callable(this, nameof(OnPlayerBlocked)));
                main.Connect("player_crit", new Callable(this, nameof(OnPlayerCrit)));
            }
            
            // Connect to combo system
            var comboSystem = GetTree().GetFirstNodeInGroup("ComboSystem");
            if (comboSystem != null)
            {
                comboSystem.Connect("OnComboMilestone", new Callable(this, nameof(OnComboMilestone)));
            }
            
            // Connect to player
            var player = GetTree().GetFirstNodeInGroup("Player");
            if (player != null)
            {
                player.Connect("dodge_success", new Callable(this, nameof(OnPlayerDodged)));
                player.Connect("block_success", new Callable(this, nameof(OnPlayerBlocked)));
            }
        }
        
        private void ConnectCombatSignals()
        {
            // Try to connect to Player's damage methods
            var player = GetTree().GetFirstNodeInGroup("Player");
            if (player != null)
            {
                // Player damage received
                player.Connect("took_damage", new Callable(this, nameof(OnPlayerDamaged)));
            }
            
            // Connect to enemy group
            GetTree().Connect("node_added", new Callable(this, nameof(OnNodeAdded)));
        }
        
        private void OnNodeAdded(Node node)
        {
            if (node is Enemy enemy)
            {
                enemy.Connect("damage_received", new Callable(this, nameof(OnEnemyDamaged)));
                enemy.Connect("died", new Callable(this, nameof(OnEnemyDied)));
            }
        }
        
        public void StartCombat()
        {
            if (!_inCombat)
            {
                _inCombat = true;
                _combatStartTime = Time.GetTicksMsec() / 1000f;
                
                if (_autoShowInCombat)
                {
                    Show();
                    PlayAppearAnimation();
                }
            }
        }
        
        public void EndCombat()
        {
            if (_inCombat)
            {
                _inCombat = false;
                
                if (_autoShowInCombat)
                {
                    // Keep showing for a moment
                    GetTree().CreateTimer(3.0f).Timeout += () => {
                        if (!_inCombat)
                        {
                            Hide();
                        }
                    };
                }
            }
        }
        
        public void ResetStats()
        {
            _totalDamageDealt = 0;
            _totalDamageTaken = 0;
            _totalKills = 0;
            _totalDodges = 0;
            _totalBlocks = 0;
            _totalCrits = 0;
            _maxCombo = 0;
            _combatStartTime = Time.GetTicksMsec() / 1000f;
            UpdateDisplay();
        }
        
        // Signal handlers
        private void OnEnemyDamaged(int damage, bool isCrit)
        {
            _totalDamageDealt += damage;
            PulseLabel(_damageDealtLabel);
            UpdateDisplay();
        }
        
        private void OnPlayerDamaged(int damage)
        {
            _totalDamageTaken += damage;
            PulseLabel(_damageTakenLabel);
            UpdateDisplay();
        }
        
        private void OnEnemyKilled()
        {
            _totalKills++;
            PulseLabel(_killsLabel);
            UpdateDisplay();
        }
        
        private void OnEnemyDied()
        {
            _totalKills++;
            PulseLabel(_killsLabel);
            UpdateDisplay();
        }
        
        private void OnPlayerDodged()
        {
            _totalDodges++;
            PulseLabel(_dodgesLabel);
            UpdateDisplay();
        }
        
        private void OnPlayerBlocked()
        {
            _totalBlocks++;
            PulseLabel(_blocksLabel);
            UpdateDisplay();
        }
        
        private void OnPlayerCrit()
        {
            _totalCrits++;
            PulseLabel(_critsLabel);
            UpdateDisplay();
        }
        
        private void OnComboMilestone(int comboLevel, int goldReward, int expReward)
        {
            if (comboLevel > _maxCombo)
            {
                _maxCombo = comboLevel;
                PulseLabel(_comboLabel);
                UpdateDisplay();
            }
        }
        
        private void PulseLabel(Label label)
        {
            _pulseTween?.Kill();
            _pulseTween = CreateTween();
            _pulseTween.TweenProperty(label, "modulate", new Color(1.5f, 1.5f, 1.5f, 1f), 0.1f);
            _pulseTween.TweenProperty(label, "modulate", Colors.White, 0.2f);
        }
        
        private void UpdateDisplay()
        {
            _damageDealtLabel.Text = _totalDamageDealt.ToString("N0");
            _damageTakenLabel.Text = _totalDamageTaken.ToString("N0");
            _killsLabel.Text = _totalKills.ToString();
            _dodgesLabel.Text = _totalDodges.ToString();
            _blocksLabel.Text = _totalBlocks.ToString();
            _critsLabel.Text = _totalCrits.ToString();
            _comboLabel.Text = _maxCombo.ToString();
            
            // Update combat time
            float elapsed = (Time.GetTicksMsec() / 1000f) - _combatStartTime;
            int minutes = (int)(elapsed / 60);
            int seconds = (int)(elapsed % 60);
            _combatTimeLabel.Text = $"{minutes}:{seconds:D2}";
        }
        
        private void PlayAppearAnimation()
        {
            Modulate = new Color(1f, 1f, 1f, 0f);
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 1f, 0.3f);
        }
        
        public override void _Process(double delta)
        {
            if (_inCombat)
            {
                _lastUpdate += (float)delta;
                if (_lastUpdate >= _updateInterval)
                {
                    _lastUpdate = 0;
                    UpdateDisplay();
                }
            }
        }
        
        public void Toggle()
        {
            if (Visible)
            {
                Hide();
            }
            else
            {
                Show();
                PlayAppearAnimation();
            }
        }
        
        // Public getters for external access
        public int TotalDamageDealt => _totalDamageDealt;
        public int TotalDamageTaken => _totalDamageTaken;
        public int TotalKills => _totalKills;
        public int TotalDodges => _totalDodges;
        public int TotalBlocks => _totalBlocks;
        public int TotalCrits => _totalCrits;
        public int MaxCombo => _maxCombo;
        public float CombatTime => (Time.GetTicksMsec() / 1000f) - _combatStartTime;
    }
}
