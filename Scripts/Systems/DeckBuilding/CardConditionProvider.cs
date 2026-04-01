using Godot;
using System;
using ClawRPG.Scripts.Combat;
using ClawRPG.Scripts.Systems;
using ClawRPG.Scripts.Systems.Combat;

namespace ClawRPG.Systems
{
    /// <summary>
    /// REQ-166: Connects CardConditionEvaluator to game state providers.
    /// Must be initialized when the game starts and when a combat begins.
    /// </summary>
    public partial class CardConditionProvider : Node
    {
        public override void _Ready()
        {
            // Initialize condition evaluator with current game state
            InitializeProviders();
            
            // Subscribe to combat events to update state
            CombatStatusSystem.OnCombatStarted += OnCombatStarted;
            
            GD.Print("[CardConditionProvider] Initialized");
        }
        
        private void InitializeProviders()
        {
            // Default providers (will be overridden when boss is available)
            CardConditionEvaluator.Instance.GetBossHealthRatio = () => GetBossHealthRatio();
            CardConditionEvaluator.Instance.IsBossEnraged = () => false;
            CardConditionEvaluator.Instance.IsBossCharging = () => IsBossCharging();
            CardConditionEvaluator.Instance.GetBossLastUsedAbility = () => _lastBossAbility;
            CardConditionEvaluator.Instance.GetBossAttacksThisTurn = () => _bossAttacksThisTurn;
            CardConditionEvaluator.Instance.BossWillAttackNext = () => WillBossAttackNext();
            CardConditionEvaluator.Instance.GetPlayerHealthRatio = () => GetPlayerHealthRatio();
            CardConditionEvaluator.Instance.IsFirstTurn = () => _isFirstTurn;
            CardConditionEvaluator.Instance.GetCurrentCombo = () => GetCurrentCombo();
        }
        
        private void OnCombatStarted()
        {
            _isFirstTurn = true;
            _bossAttacksThisTurn = 0;
            _lastBossAbility = "";
            
            // Update to non-first-turn after first action
            Callable.From(() => { _isFirstTurn = false; }).CallDeferred();
        }
        
        public void UpdateBossState(Godot.Node boss)
        {
            // Boss-specific state updates are handled by the BossDecisionMaker system
            // which calls OnBossUsedAbility and manages the charging state
        }
        
        public void OnBossUsedAbility(string abilityId)
        {
            _lastBossAbility = abilityId;
            _bossAttacksThisTurn++;
        }
        
        private float GetBossHealthRatio() => 1f;
        private bool IsBossCharging() => false;
        private bool WillBossAttackNext() => false;
        private float GetPlayerHealthRatio()
        {
            // Try to get player health from CombatStatusSystem
            var css = CombatStatusSystem.Instance;
            if (css == null) return 1f;
            
            var status = css.GetCurrentCombatStatus();
            if (status == null) return 1f;
            // CombatStatusData doesn't have player health directly
            return 1f;
        }
        
        private int GetCurrentCombo()
        {
            var css = CombatStatusSystem.Instance;
            if (css == null) return 0;
            
            var status = css.GetCurrentCombatStatus();
            if (status == null) return 0;
            return status.CurrentCombo;
        }
        
        private string _lastBossAbility = "";
        private int _bossAttacksThisTurn = 0;
        private bool _isFirstTurn = false;
    }
}
