using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Real-time combat statistics display panel
    /// Shows damage dealt, time, kills, damage taken, dodges, blocks, etc.
    /// Includes combat rating system for post-battle evaluation
    /// </summary>
    public partial class CombatStatsPanel : Control
    {
        private static CombatStatsPanel _instance;
        public static CombatStatsPanel Instance => _instance;
        
        [Export] private bool _autoShowInCombat = true;
        [Export] private float _updateInterval = 0.5f;
        [Export] private bool _showRatingOnEnd = true;
        
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
        
        // Components and Display helpers
        private CombatStatsPanelComponents _components;
        private CombatStatsPanelDisplay _display;
        
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
            
            // Initialize components helper
            _components = new CombatStatsPanelComponents(this);
            _components.SetupMainPanel(out _mainPanel, out _statsContainer);
            AddChild(_mainPanel);
            
            // Create stat rows
            _damageDealtLabel = _components.AddStatRow(_statsContainer, "造成伤害", "0", new Color(1f, 0.4f, 0.4f, 1f));
            _damageTakenLabel = _components.AddStatRow(_statsContainer, "受到伤害", "0", new Color(0.4f, 0.6f, 1f, 1f));
            _killsLabel = _components.AddStatRow(_statsContainer, "击杀敌人", "0", new Color(0.4f, 1f, 0.5f, 1f));
            _combatTimeLabel = _components.AddStatRow(_statsContainer, "战斗时间", "0:00", new Color(1f, 0.9f, 0.5f, 1f));
            _dodgesLabel = _components.AddStatRow(_statsContainer, "闪避次数", "0", new Color(0.5f, 0.8f, 1f, 1f));
            _blocksLabel = _components.AddStatRow(_statsContainer, "格挡次数", "0", new Color(0.8f, 0.6f, 1f, 1f));
            _critsLabel = _components.AddStatRow(_statsContainer, "暴击次数", "0", new Color(1f, 0.5f, 0.8f, 1f));
            _comboLabel = _components.AddStatRow(_statsContainer, "最高连击", "0", new Color(1f, 0.85f, 0.2f, 1f));
            
            // Initialize display helper
            _display = new CombatStatsPanelDisplay(this);
            _display.SetupRatingPanel();
            
            // Connect signals
            ConnectCombatSignals();
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
                    _display.PlayAppearAnimation(this);
                }
            }
        }
        
        public void EndCombat()
        {
            if (_inCombat)
            {
                _inCombat = false; 
                
                // Calculate and show rating
                if (_showRatingOnEnd && _totalKills > 0)
                {
                    _display.ShowRating(CalculateRating, GetRatingInfo);
                }
                
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
        
        /// <summary>
        /// Calculate combat rating based on performance metrics
        /// </summary>
        private float CalculateRating()
        {
            if (_totalKills == 0) return 0f;
            
            float score = 0f;
            
            // 1. Damage efficiency (40% weight)
            // Higher damage per kill = better
            float damagePerKill = _totalDamageDealt / (float)_totalKills;
            float damageScore = Math.Min(damagePerKill / 500f, 1f) * 40f;
            score += damageScore;
            
            // 2. Survival (30% weight)
            // Less damage taken = better
            float survivalScore = 0f;
            if (_totalDamageTaken == 0)
            {
                survivalScore = 30f; // Perfect survival
            }
            else
            {
                float damagePerKillTaken = _totalDamageDealt / (float)Math.Max(_totalDamageTaken, 1);
                survivalScore = Math.Min(damagePerKillTaken / 10f, 1f) * 30f;
            }
            score += survivalScore;
            
            // 3. Skill usage (20% weight)
            // Dodges, blocks, crits show player skill
            float totalSkillActions = _totalDodges + _totalBlocks + _totalCrits;
            float skillScore = Math.Min(totalSkillActions / (float)Math.Max(_totalKills, 1) * 2f, 1f) * 20f;
            score += skillScore;
            
            // 4. Combat efficiency (10% weight)
            // Fast kills = better
            float combatTime = (Time.GetTicksMsec() / 1000f) - _combatStartTime;
            if (combatTime > 0)
            {
                float killsPerSecond = _totalKills / combatTime;
                float efficiencyScore = Math.Min(killsPerSecond * 5f, 1f) * 10f;
                score += efficiencyScore;
            }
            
            return Math.Min(score, 100f);
        }
        
        /// <summary>
        /// Get rating letter based on score
        /// </summary>
        private (string letter, string detail, Color color) GetRatingInfo(float score)
        {
            const float RATING_S_THRESHOLD = 95f;  // S rank: top 5%
            const float RATING_A_THRESHOLD = 85f;  // A rank: top 15%
            const float RATING_B_THRESHOLD = 70f;  // B rank: top 30%
            const float RATING_C_THRESHOLD = 50f;  // C rank: top 50%
            
            if (score >= RATING_S_THRESHOLD)
                return ("S", "完美表现！", new Color(1f, 0.84f, 0f, 1f)); // Gold
            if (score >= RATING_A_THRESHOLD)
                return ("A", "出色发挥！", new Color(0.4f, 1f, 0.4f, 1f)); // Green
            if (score >= RATING_B_THRESHOLD)
                return ("B", "良好水平", new Color(0.4f, 0.8f, 1f, 1f)); // Blue
            if (score >= RATING_C_THRESHOLD)
                return ("C", "还需练习", new Color(1f, 0.7f, 0.4f, 1f)); // Orange
            return ("D", "继续努力", new Color(0.8f, 0.5f, 0.5f, 1f)); // Red
        }
        
        /// <summary>
        /// Hide rating panel
        /// </summary>
        public void HideRating()
        {
            _display?.HideRating();
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
            HideRating();
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
                HideRating();
            }
            else
            {
                Show();
                _display.PlayAppearAnimation(this);
            }
        }
        
        /// <summary>
        /// Get current combat rating (call after combat ends)
        /// </summary>
        public string GetCurrentRating()
        {
            float score = CalculateRating();
            var (letter, _, _) = GetRatingInfo(score);
            return letter;
        }
        
        /// <summary>
        /// Get detailed rating info
        /// </summary>
        public (string letter, string detail, float score) GetRatingDetails()
        {
            float score = CalculateRating();
            var (letter, detail, _) = GetRatingInfo(score);
            return (letter, detail, score);
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
