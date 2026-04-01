using System;
using System.Collections.Generic;

namespace ClawRPG.Systems
{
    /// <summary>
    /// REQ-166: Conditional Card Effect System
    /// 
    /// Condition triggers that modify card effect values based on game state.
    /// Conditions are evaluated at card-play time, not at deck-building time.
    /// 
    /// Condition multiplier rules:
    /// - 0.0 = card has no effect (played but deals no damage/block)
    /// - 1.0 = normal effect
    /// - >1.0 = enhanced effect (conditional upgrade)
    /// 
    /// Multiple conditions use OR logic (any match triggers the multiplier).
    /// </summary>
    public enum CardConditionType
    {
        None,
        
        // Boss health conditions
        BossHealthAbove,   // Boss HP > threshold (e.g., 50%)
        BossHealthBelow,    // Boss HP < threshold
        
        // Boss state conditions  
        BossIsEnraged,     // Boss in enrage mode
        BossIsCharging,     // Boss currently charging an ability (REQ-164)
        BossJustUsedAbility,// Boss used ability X this turn
        
        // Boss attack conditions
        BossAttackedThisTurn,  // Boss attacked >= N times this turn
        BossWillAttackNext,    // Boss intent shows attack
        
        // Player conditions
        PlayerHealthBelow,   // Player HP < threshold (danger)
        PlayerIsFullHealth,   // Player at full HP
        
        // Combat conditions
        IsFirstTurnOfCombat, // First turn of current combat
        ComboActive,          // Combo is currently active (>= 3)
    }
    
    /// <summary>
    /// REQ-166: A single condition attached to a card.
    /// </summary>
    [Serializable]
    public class CardCondition
    {
        public CardConditionType Type = CardConditionType.None;
        
        // For threshold-based conditions (e.g., BossHealthAbove with threshold=0.5)
        public float Threshold = 0f;
        
        // For ability-based conditions (e.g., BossJustUsedAbility)
        public string AbilityId = "";
        
        // The multiplier applied when this condition is met
        public float Multiplier = 1.0f;
        
        // Description shown when condition is active (for UI)
        public string ActiveText = "";
        
        // Description shown when condition is NOT active
        public string InactiveText = "";
        
        public CardCondition() { }
        
        public CardCondition(CardConditionType type, float multiplier, string activeText = "", string inactiveText = "")
        {
            Type = type;
            Multiplier = multiplier;
            ActiveText = activeText;
            InactiveText = inactiveText;
        }
        
        public override string ToString()
        {
            return $"[{Type} x{Multiplier}]";
        }
    }
    
    /// <summary>
    /// REQ-166: Evaluates card conditions against current game state.
    /// Uses singleton pattern - state is injected from external sources.
    /// </summary>
    public class CardConditionEvaluator
    {
        public static readonly CardConditionEvaluator Instance = new CardConditionEvaluator();
        
        // State providers — set by external systems
        public Func<float> GetBossHealthRatio { get; set; } = () => 1f;
        public Func<bool> IsBossEnraged { get; set; } = () => false;
        public Func<bool> IsBossCharging { get; set; } = () => false;
        public Func<string> GetBossLastUsedAbility { get; set; } = () => "";
        public Func<int> GetBossAttacksThisTurn { get; set; } = () => 0;
        public Func<bool> BossWillAttackNext { get; set; } = () => false;
        public Func<float> GetPlayerHealthRatio { get; set; } = () => 1f;
        public Func<bool> IsFirstTurn { get; set; } = () => false;
        public Func<int> GetCurrentCombo { get; set; } = () => 0;
        
        /// <summary>
        /// Evaluate a single condition against current state.
        /// Returns true if condition is met.
        /// </summary>
        public bool Evaluate(CardCondition condition)
        {
            return condition.Type switch
            {
                CardConditionType.None => false,
                
                CardConditionType.BossHealthAbove => 
                    GetBossHealthRatio() > condition.Threshold,
                
                CardConditionType.BossHealthBelow => 
                    GetBossHealthRatio() < condition.Threshold,
                
                CardConditionType.BossIsEnraged => 
                    IsBossEnraged(),
                
                CardConditionType.BossIsCharging => 
                    IsBossCharging(),
                
                CardConditionType.BossJustUsedAbility => 
                    GetBossLastUsedAbility() == condition.AbilityId,
                
                CardConditionType.BossAttackedThisTurn => 
                    GetBossAttacksThisTurn() >= (int)condition.Threshold,
                
                CardConditionType.BossWillAttackNext => 
                    BossWillAttackNext(),
                
                CardConditionType.PlayerHealthBelow => 
                    GetPlayerHealthRatio() < condition.Threshold,
                
                CardConditionType.PlayerIsFullHealth => 
                    GetPlayerHealthRatio() >= 1f,
                
                CardConditionType.IsFirstTurnOfCombat => 
                    IsFirstTurn(),
                
                CardConditionType.ComboActive => 
                    GetCurrentCombo() >= (int)condition.Threshold,
                
                _ => false
            };
        }
        
        /// <summary>
        /// Evaluate all conditions on a card.
        /// Uses OR logic: if ANY condition is met, the corresponding multiplier applies.
        /// If multiple conditions are met, the HIGHEST multiplier wins.
        /// </summary>
        public float EvaluateConditions(System.Collections.Generic.List<CardCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0)
                return 1.0f; // No conditions = always normal
            
            float bestMultiplier = 1.0f;
            
            foreach (var condition in conditions)
            {
                if (Evaluate(condition))
                {
                    if (condition.Multiplier > bestMultiplier)
                        bestMultiplier = condition.Multiplier;
                }
            }
            
            return bestMultiplier;
        }
        
        /// <summary>
        /// Get the text to display for this card given current conditions.
        /// Shows active text if condition is met, inactive text otherwise.
        /// </summary>
        public string GetConditionText(CardCondition condition)
        {
            if (condition == null || condition.Type == CardConditionType.None)
                return "";
            
            if (Evaluate(condition))
                return condition.ActiveText;
            else
                return condition.InactiveText;
        }
    }
}
