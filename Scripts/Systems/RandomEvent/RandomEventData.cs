using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.RandomEvent;

/// <summary>
/// Random event types in the game
/// </summary>
public enum RandomEventType
{
    // Positive events
    LuckyFind,          // Found gold/items
    MysteriousBlessing, // Got a buff
    TraderVisit,        // A merchant appears
    HealingSpring,      // Restore health
    TreasureChest,     // Found a chest
    FriendlyEncounter,  // Met a friendly NPC
    AncientKnowledge,  // Gained experience/knowledge
    Windfall,          // Random bonus
    
    // Negative events
    Ambush,            // Enemy ambush
    Trap,              // Triggered a trap
    Curse,             // Got a debuff
    Bandits,           // Bandits attack
    BadWeather,        // Weather penalty
    Plague,            // Health penalty
    Theft,             // Lost items/gold
    MonsterAttack,     // Monster attacks
    
    // Neutral events
    Traveler,          // Met a traveler
    Landmark,         // Found a landmark
    RestSite,          // Safe place to rest
    Puzzle,            // Solve a puzzle
    Riddle,            // Answer a riddle
    Omen               // A mysterious omen
}

/// <summary>
/// Event rarity levels
/// </summary>
public enum EventRarity
{
    Common,     // 50% chance
    Uncommon,   // 30% chance  
    Rare,       // 15% chance
    Legendary   // 5% chance
}

/// <summary>
/// Data structure for random events
/// </summary>
[GlobalClass]
public partial class RandomEventData : Resource
{
    [Export] public string eventId;
    [Export] public string eventName;
    [Export] public string description;
    [Export] public RandomEventType eventType;
    [Export] public EventRarity rarity;
    [Export] public bool isPositive;
    [Export] public bool isNegative;
    
    // Rewards
    [Export] public int goldReward;
    [Export] public int experienceReward;
    [Export] public float healthRestore;
    [Export] public float attackBonus;
    [Export] public float defenseBonus;
    [Export] public float speedBonus;
    [Export] public float luckBonus;
    
    // Penalties (for negative events)
    [Export] public int goldPenalty;
    [Export] public float healthPenalty;
    [Export] public float attackPenalty;
    [Export] public float defensePenalty;
    [Export] public float speedPenalty;
    
    // Duration for temporary effects (in seconds, -1 for permanent)
    [Export] public float effectDuration = 300f;
    
    // Requirements
    [Export] public int minPlayerLevel = 1;
    [Export] public int maxPlayerLevel = 100;
    [Export] public float probability = 1.0f;
}

/// <summary>
/// Player's event history and stats
/// </summary>
public class RandomEventStats
{
    public int eventsEncountered;
    public int positiveEvents;
    public int negativeEvents;
    public int neutralEvents;
    public int totalGoldGained;
    public int totalGoldLost;
    public int totalExperienceGained;
    public Dictionary<string, int> eventCounts = new();
    public List<string> recentEvents = new();
    public Dictionary<string, DateTime> activeEffects = new();
}
