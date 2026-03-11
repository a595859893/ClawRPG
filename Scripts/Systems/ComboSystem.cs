using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Enhanced combo system with visual feedback and rewards
    /// </summary>
    public partial class ComboSystem : Node
    {
        private static ComboSystem _instance;
        public static ComboSystem Instance => _instance;
        
        [Export] private int _maxCombo = 100;
        [Export] private float _comboDecayTime = 3.0f; // seconds without hit before combo resets
        [Export] private float _comboMultiplierBase = 0.1f; // 10% bonus per combo level
        
        private int _currentCombo = 0;
        private float _comboTimer = 0f;
        private int _totalComboHits = 0;
        private int _highestCombo = 0;
        private bool _hasTriggeredFirstCombo = false;
        
        // Combo milestone rewards
        private Dictionary<int, (int gold, int exp)> _milestoneRewards = new()
        {
            { 10, (10, 5) },
            { 25, (25, 15) },
            { 50, (50, 30) },
            { 75, (100, 50) },
            { 100, (200, 100) }
        };
        
        // Signals
        [Signal]
    public delegate void OnComboChanged(int newCombo, int maxCombo);
        [Signal]
    public delegate void OnComboMilestone(int comboLevel, int goldReward, int expReward);
        [Signal]
    public delegate void OnComboBroken();
        
        public int CurrentCombo => _currentCombo;
        public int MaxCombo => _maxCombo;
        public float ComboMultiplier => 1f + (_currentCombo * _comboMultiplierBase);
        
        public override void _Ready()
        {
            _instance = this;
            AddToGroup("ComboSystem");
        }
        
        public override void _Process(double delta)
        {
            if (_currentCombo > 0)
            {
                _comboTimer += (float)delta;
                if (_comboTimer >= _comboDecayTime)
                {
                    BreakCombo();
                }
            }
        }
        
        /// <summary>
        /// Register a hit to build combo
        /// </summary>
        public void RegisterHit(int damage = 0)
        {
            _currentCombo = Mathf.Min(_currentCombo + 1, _maxCombo);
            _comboTimer = 0f;
            _totalComboHits++;
            
            if (_currentCombo > _highestCombo)
            {
                _highestCombo = _currentCombo;
            }
            
            OnComboChanged?.Invoke(_currentCombo, _maxCombo);
            
            // Check for milestone rewards
            if (_milestoneRewards.TryGetValue(_currentCombo, out var rewards))
            {
                GrantMilestoneReward(rewards.gold, rewards.exp);
            }
            
            // Trigger visual effect
            TriggerComboEffect();
            
            // Trigger tutorial for first combo
            if (!_hasTriggeredFirstCombo && _currentCombo >= 3)
            {
                _hasTriggeredFirstCombo = true;
                TutorialSystem.Trigger(TutorialTrigger.FirstCombo);
            }
        }
        
        /// <summary>
        /// Break the combo
        /// </summary>
        public void BreakCombo()
        {
            if (_currentCombo > 5)
            {
                OnComboBroken?.Invoke();
            }
            
            _currentCombo = 0;
            _comboTimer = 0f;
            OnComboChanged?.Invoke(_currentCombo, _maxCombo);
        }
        
        /// <summary>
        /// Grant milestone reward
        /// </summary>
        private void GrantMilestoneReward(int gold, int exp)
        {
            var player = GetTree().GetFirstNodeInGroup("Player") as Player;
            if (player != null)
            {
                player.AddGold(gold);
                player.AddExperience(exp);
            }
            
            OnComboMilestone?.Invoke(_currentCombo, gold, exp);
            
            // Show notification
            var messageSystem = GetTree().GetFirstNodeInGroup("GameMessage") as Node;
            messageSystem?.Call("ShowPositive", $"Combo x{_currentCombo}! +{gold} Gold, +{exp} XP");
        }
        
        /// <summary>
        /// Trigger combo visual effect
        /// </summary>
        private void TriggerComboEffect()
        {
            var screenEffect = GetTree().GetFirstNodeInGroup("DynamicScreenEffect") as Node;
            screenEffect?.Call("TriggerComboPulse", _currentCombo);
            
            // Screen flash for high combos
            if (_currentCombo >= 25)
            {
                screenEffect?.Call("FlashScreen", new Color(1f, 0.8f, 0.2f, 0.15f), 0.2f);
            }
        }
        
        /// <summary>
        /// Get combo damage bonus
        /// </summary>
        public float GetComboDamageBonus()
        {
            return ComboMultiplier;
        }
        
        /// <summary>
        /// Serialize combo data for save
        /// </summary>
        public Dictionary<string, Variant> Serialize()
        {
            return new Dictionary<string, Variant>
            {
                { "totalComboHits", _totalComboHits },
                { "highestCombo", _highestCombo }
            };
        }
        
        /// <summary>
        /// Deserialize combo data from save
        /// </summary>
        public void Deserialize(Dictionary<string, Variant> data)
        {
            if (data.TryGetValue("totalComboHits", out var hits))
                _totalComboHits = (int)hits;
            if (data.TryGetValue("highestCombo", out var highest))
                _highestCombo = (int)highest;
        }
    }
}
