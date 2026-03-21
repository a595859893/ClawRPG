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
        
        // Rating system
        private PanelContainer _ratingPanel;
        private Label _ratingLabel;
        private Label _ratingDetailLabel;
        
        // Rating constants
        private const float RATING_S_THRESHOLD = 95f;  // S rank: top 5%
        private const float RATING_A_THRESHOLD = 85f;  // A rank: top 15%
        private const float RATING_B_THRESHOLD = 70f;  // B rank: top 30%
        private const float RATING_C_THRESHOLD = 50f;  // C rank: top 50%
        
        private float _lastUpdate = 0;
        private PanelContainer _mainPanel;
        private VBoxContainer _statsContainer;
        private Tween _pulseTween;
        
        // Display helper
        private CombatStatsPanelDisplay _display;
        
        public override void _Ready()
        {
            _instance = this;
            AddToGroup("CombatStatsPanel");
            SetupUI();
            ConnectSignals();
            Hide();
        }
        
        private void ConnectSignals()
        {
            // Connect to combat events via Main
            var main = GetNode("/root/Main");
            if (main != null)
            {
                main.EnemyDamaged += (damage, isCrit) => OnEnemyDamaged(damage, isCrit);
                main.PlayerDamaged += (damage) => OnPlayerDamaged(damage);
                main.EnemyKilled += OnEnemyKilled;
                main.PlayerDodged += OnPlayerDodged;
                main.PlayerBlocked += OnPlayerBlocked;
                main.PlayerCrit += OnPlayerCrit;
            }
            
            // Connect to combo system
            var comboSystem = GetTree().GetFirstNodeInGroup("ComboSystem");
            if (comboSystem != null)
            {
                comboSystem.OnComboMilestone += (combo, gold, exp) => OnComboMilestone(combo, gold, exp);
            }
            
            // Connect to player
            var player = GetTree().GetFirstNodeInGroup("Player");
            if (player != null)
            {
                player.DodgeSuccess += OnPlayerDodged;
                player.BlockSuccess += OnPlayerBlocked;
            }
        }
        
        private void ConnectCombatSignals()
        {
            // Try to connect to Player's damage methods
            var player = GetTree().GetFirstNodeInGroup("Player");
            if (player != null)
            {
                // Player damage received
                player.TookDamage += (damage) => OnPlayerDamaged(damage);
            }
            
            // Connect to enemy group
            GetTree().NodeAdded += OnNodeAdded;
        }
        
        private void OnNodeAdded(Node node)
        {
            if (node is Enemy enemy)
            {
                enemy.DamageReceived += (damage, isCrit) => OnEnemyDamaged(damage, isCrit);
                enemy.Died += OnEnemyDied;
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
                
                // Calculate and show rating
                if (_showRatingOnEnd && _totalKills > 0)
                {
                    ShowRating();
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
                PlayAppearAnimation();
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
