using Godot;
using System;
using System.Collections.Generic;

public class RoguelikeStatsDatabase
{
    // Character classes for tracking
    public static string[] CharacterClasses = new string[]
    {
        "Warrior", "Mage", "Rogue", "Ranger", "Paladin", "Necromancer", "Druid", "Bard"
    };
    
    // Build types
    public static string[] BuildTypes = new string[]
    {
        "Attack", "Defense", "Balanced", "Speed", "Magic", "Summoner", "Tank", "Hybrid"
    };
    
    // Death causes
    public static string[] DeathCauses = new string[]
    {
        "EnemyDamage", "BossDamage", "Poison", "Trap", "Fall", "Starvation", "Cursed", "Explosion"
    };
    
    // Victory rewards multiplier by floor
    public static Dictionary<int, float> FloorRewardMultiplier = new Dictionary<int, float>
    {
        { 10, 1.0f },
        { 20, 1.5f },
        { 30, 2.0f },
        { 40, 2.5f },
        { 50, 3.0f },
        { 60, 3.5f },
        { 70, 4.0f },
        { 80, 4.5f },
        { 90, 5.0f },
        { 100, 6.0f }
    };
    
    // Colors for UI
    public static Color VictoryColor = new Color(0.2f, 0.8f, 0.2f);
    public static Color DefeatColor = new Color(0.8f, 0.2f, 0.2f);
    public static Color GoldColor = new Color(1.0f, 0.84f, 0.0f);
    public static Color FloorColor = new Color(0.4f, 0.6f, 1.0f);
}
