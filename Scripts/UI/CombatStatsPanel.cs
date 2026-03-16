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
        
        #region Lifecycle
        
        public override void _Ready()
        {
            _instance = this;
            AddToGroup("CombatStatsPanel");
            SetupUI();
            ConnectSignals();
            Hide();
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
        
        #endregion
        
        #region Signal Connection
        
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
            
            // Connect combat signals
            ConnectCombatSignals();
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
        
        #endregion
        
        #region Combat Control
        
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
        
        #endregion
        
        #region Signal Handlers
        
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
        
        #endregion
        
        #region Public Properties
        
        public int TotalDamageDealt => _totalDamageDealt;
        public int TotalDamageTaken => _totalDamageTaken;
        public int TotalKills => _totalKills;
        public int TotalDodges => _totalDodges;
        public int TotalBlocks => _totalBlocks;
        public int TotalCrits => _totalCrits;
        public int MaxCombo => _maxCombo;
        public float CombatTime => (Time.GetTicksMsec() / 1000f) - _combatStartTime;
        
        #endregion
    }
}
