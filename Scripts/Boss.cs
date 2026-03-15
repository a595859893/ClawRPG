using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.Characters {
    /// <summary>
    /// Boss enemy with multiple phases and special abilities - Refactored version
    /// </summary>
    public partial class Boss : Enemy
    {
        [ExportGroup("Boss Properties")]
        [Export] public string BossTitle = "Ancient Dragon";
        [Export] public int PhaseCount = 3;
        [Export] public float EnrageTime = 120f;
        
        [ExportGroup("Phase Settings")]
        [Export] public int[] PhaseHealthThresholds = { 66, 33 };
        
        [ExportGroup("Special Abilities")]
        [Export] public float AbilityCooldown = 10f;
        [Export] public string[] SpecialAbilities;
        
        // Boss subsystems
        private BossData _bossData;
        private BossAI _bossAI;
        private BossAbilities _bossAbilities;
        private BossPhase _bossPhase;
        
        // AI Decision Maker (Behavior Tree)
        private BossDecisionMaker _decisionMaker;
        
        // Events
        public event Action<int> OnPhaseChange;
        public event Action OnEnrage;
        public event Action<string> OnSpecialAbility;
        public event Action<string> OnAbilityWarmingUp;
        public event Action<BossAIState> OnAIStateChanged;
        
        public override void _Ready()
        {
            base._Ready();
            
            // Initialize subsystems
            InitializeBossData();
            
            // Initialize rage shader
            _bossPhase.InitializeRageShader();
            
            // Initialize abilities
            _bossAbilities.Initialize();
            
            // Connect ability warning signal to UI
            ConnectAbilityWarningToUI();
            
            // Initialize AI Decision Maker (Behavior Tree)
            _decisionMaker = new BossDecisionMaker(this);
            
            // Connect events
            ConnectEvents();
            
            // Reset boss damage tracking for no-hit achievement
            AchievementManager.Instance?.ResetBossDamageTaken();
            
            GD.Print($"Boss {BossTitle} spawned! Phase: {_bossData.CurrentPhase}, Enrage: {EnrageTime}s");
        }
        
        /// <summary>
        /// Initialize boss data
        /// </summary>
        private void InitializeBossData()
        {
            _bossData = new BossData
            {
                BossTitle = BossTitle,
                BossId = Name,
                PhaseCount = PhaseCount,
                EnrageTime = EnrageTime,
                PhaseHealthThresholds = PhaseHealthThresholds,
                AbilityCooldown = AbilityCooldown,
                SpecialAbilities = SpecialAbilities,
                EnrageTimer = EnrageTime
            };
            
            _bossData.InitializeAbilityDatabase();
            _bossData.InitializeAvailableAbilities(SpecialAbilities);
            
            _bossAI = new BossAI(this, _bossData);
            _bossAbilities = new BossAbilities(this, _bossData);
            _bossPhase = new BossPhase(this, _bossData);
        }
        
        /// <summary>
        /// Connect internal events
        /// </summary>
        private void ConnectEvents()
        {
            _bossAI.OnStateChanged += (state) => OnAIStateChanged?.Invoke(state);
            _bossPhase.OnPhaseChange += (phase) => OnPhaseChange?.Invoke(phase);
            _bossPhase.OnEnrage += () => OnEnrage?.Invoke();
            _bossAbilities.OnAbilityUsed += (ability) => OnSpecialAbility?.Invoke(ability);
            _bossAbilities.OnAbilityWarmingUp += (ability) => OnAbilityWarmingUp?.Invoke(ability);
        }
        
        /// <summary>
        /// Connect ability warning signal to UI
        /// </summary>
        private void ConnectAbilityWarningToUI()
        {
            CallDeferred(nameof(_ConnectAbilityWarningSignal));
        }
        
        private void _ConnectAbilityWarningSignal()
        {
            var warningUI = GetTree().GetCurrentScene().GetNodeOrNull<UI.BossAbilityWarningUI>("BossAbilityWarningUI");
            if (warningUI != null)
            {
                OnAbilityWarmingUp += (abilityId) => {
                    var target = GetTarget();
                    Vector2 targetPos = target != null ? target.GlobalPosition : GlobalPosition;
                    
                    // Get ability info
                    bool isAoE = false;
                    float aoeRadius = 0f;
                    var ability = _bossData.GetAbility(abilityId);
                    if (ability != null)
                    {
                        isAoE = ability.IsAoE;
                        aoeRadius = ability.AoERadius;
                    }
                    
                    warningUI.ShowAbilityWarning(abilityId, targetPos, 2f, isAoE, aoeRadius);
                };
                
                GD.Print($"Boss {BossTitle} connected ability warning to UI");
            }
            else
            {
                GD.PrintErr($"BossAbilityWarningUI not found for boss {BossTitle}");
            }
        }
        
        public override void _PhysicsProcess(double delta)
        {
            float dt = (float)delta;
            
            if (IsDead) return;
            
            // Update ability cooldowns
            _bossData.UpdateCooldowns(dt);
            
            // Update enrage timer
            _bossPhase.UpdateEnrage(dt);
            
            // Update AI
            _bossAI.Update(dt);
            
            // Update ability timer
            _bossData.AbilityTimer -= dt;
            if (_bossData.AbilityTimer <= 0)
            {
                TryUseDecisionMaker();
                _bossData.AbilityTimer = AbilityCooldown;
            }
            
            // Check phase transition
            _bossPhase.CheckPhaseTransition();
            
            base._PhysicsProcess(delta);
        }
        
        /// <summary>
        /// Use Behavior Tree decision maker for intelligent ability selection
        /// </summary>
        private void TryUseDecisionMaker()
        {
            if (_decisionMaker == null)
            {
                _bossAbilities.TryUseSpecialAbility();
                return;
            }
            
            var target = GetTarget();
            if (target == null)
            {
                _bossAbilities.TryUseSpecialAbility();
                return;
            }
            
            _decisionMaker.MakeDecision(target, 0f);
        }
        
        /// <summary>
        /// Force set AI state (for decision maker)
        /// </summary>
        public void ForceSetAIState(BossAIState newState)
        {
            _bossAI.ForceSetState(newState);
            GD.Print($"{BossTitle} state changed to {newState} via Decision Maker");
        }
        
        /// <summary>
        /// Try to use a specific ability (called by decision maker)
        /// </summary>
        public void TryUseAbility(string abilityId)
        {
            _bossAbilities.TryUseAbility(abilityId);
        }
        
        // Public accessors
        public int GetCurrentPhase() => _bossPhase.GetCurrentPhase();
        public bool IsEnraged() => _bossPhase.IsEnraged();
        public float GetEnrageTimeRemaining() => _bossPhase.GetEnrageTimeRemaining();
        public float GetEnragePercentage() => _bossPhase.GetEnragePercentage();
        public BossAIState GetAIState() => _bossAI.GetState();
        
        public float GetAbilityCooldown(string abilityId)
        {
            return _bossAbilities.GetAbilityCooldown(abilityId);
        }
        
        public Dictionary<string, BossAbility> GetAbilityDatabase() => _bossData.AbilityDatabase;
        
        public override void Die()
        {
            GD.Print($"*** BOSS DEFEATED: {BossTitle} ***");
            OnPhaseChange = null;
            OnEnrage = null;
            OnSpecialAbility = null;
            OnAIStateChanged = null;
            
            // Track boss bounty progress
            BountyManager.Instance?.UpdateBossKillProgress(BossId);
            
            // Track boss kill achievements
            AchievementManager.Instance?.TrackBossKill();
            
            // Track enrage kill achievements
            if (_bossPhase.IsEnraged())
            {
                AchievementManager.Instance?.TrackEnrageKill();
                GD.Print($"*** ENRAGE KILL! {_bossPhase.IsEnraged()} ***");
            }
            
            // Track no-hit boss achievement
            var player = GetTarget();
            if (player != null)
            {
                var stats = AchievementManager.Instance?.GetStatistics();
                if (stats != null && stats.ContainsKey("bossDamageTaken"))
                {
                    int bossDamageTaken = AchievementManager.Instance.GetBossDamageTaken();
                    if (bossDamageTaken <= 0)
                    {
                        AchievementManager.Instance?.TrackNoHitBoss(true);
                    }
                }
            }
            
            base.Die();
        }
    }
}
