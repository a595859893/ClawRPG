using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Combat;
using Framework;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// Combat UI System - Manages all combat interface elements
    /// </summary>
    public partial class CombatUISystem : BaseSystem
    {
        private static CombatUISystem _instance;
        public static CombatUISystem Instance => _instance;
        
        // Database reference
        private CombatUIDatabase _database;
        
        // Active damage text instances
        private List<DamageTextData> _activeDamageTexts = new List<DamageTextData>();
        
        // Combat statistics
        private CombatStatistics _currentSessionStats = new CombatStatistics();
        
        // Combo tracking
        private ComboChainData _currentCombo = new ComboChainData();
        
        // UI preferences
        private UILayoutPreferences _uiPreferences = new UILayoutPreferences();
        
        // Combat state
        private PlayerCombatState _playerState = new PlayerCombatState();
        private List<EnemyCombatState> _enemyStates = new List<EnemyCombatState>();
        
        // Screen effects queue
        private Queue<ScreenEffectTrigger> _screenEffectQueue = new Queue<ScreenEffectTrigger>();
        
        // Signals
        public static string SignalDamageDealt = "damage_dealt";
        public static string SignalDamageTaken = "damage_taken";
        public static string SignalHealing = "healing";
        public static string SignalComboMilestone = "combo_milestone";
        public static string SignalKill = "enemy_kill";
        public static string SignalScreenEffect = "screen_effect";
        
        public override void _Ready()
        {
            _instance = this;
            _database = CombatUIDatabase.Instance;
            
            // Initialize default preferences
            _uiPreferences = new UILayoutPreferences
            {
                ShowDamageNumbers = true,
                ShowHealthBars = true,
                ShowComboCounter = true,
                ShowCombatIndicators = true,
                ShowDPS = false,
                UIScale = 1.0f,
                DamageNumberPosition = "above_target"
            };
            
            GD.Print("[CombatUISystem] Combat UI System initialized");
        }
        
        public override void _Process(double delta)
        {
            // Update combo timer
            if (_currentCombo.CurrentCombo > 0)
            {
                _currentCombo.ComboTimer -= delta;
                if (_currentCombo.ComboTimer <= 0)
                {
                    ResetCombo();
                }
            }
            
            // Update DPS calculation
            if (_currentSessionStats.SessionDuration > 0)
            {
                _currentSessionStats.DPS = _currentSessionStats.TotalDamageDealt / _currentSessionStats.SessionDuration;
            }
            
            // Process screen effects queue
            ProcessScreenEffects();
        }
    }
}
