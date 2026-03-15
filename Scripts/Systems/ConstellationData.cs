using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 星座数据 - 存储星座配置信息
/// </summary>
public class ConstellationData : Node
{
    // Constellation types
    public enum ConstellationType
    {
        Fire,      // Aries, Leo, Sagittarius
        Water,     // Cancer, Scorpio, Pisces
        Earth,     // Taurus, Virgo, Capricorn
        Air,       // Gemini, Libra, Aquarius
        Light,     // Orion, Phoenix, Sirius
        Dark       // Shadow, Void, Eclipse
    }
    
    // Rarity levels
    public enum ConstellationRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    // Single constellation data
    public class Constellation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ConstellationType Type { get; set; }
        public ConstellationRarity Rarity { get; set; }
        public int Stars { get; set; } // Number of stars in constellation
        public float AttackBonus { get; set; }
        public float DefenseBonus { get; set; }
        public float HealthBonus { get; set; }
        public float SpeedBonus { get; set; }
        public float CriticalBonus { get; set; }
        public float EvasionBonus { get; set; }
        public float GoldBonus { get; set; }
        public float ExpBonus { get; set; }
        public int UnlockCost { get; set; }
        public int RequiredLevel { get; set; }
    }
    
    // Player's constellation progress
    public class ConstellationProgress
    {
        public string ConstellationId { get; set; }
        public bool Unlocked { get; set; }
        public int ActivatedStars { get; set; }
        public int TotalStars { get; set; }
        public DateTime UnlockTime { get; set; }
    }
    
    // Data storage
    public Dictionary<string, ConstellationProgress> UnlockedConstellations { get; set; } = new Dictionary<string, ConstellationProgress>();
    public int TotalActivationPoints { get; set; }
    public int UsedActivationPoints { get; set; }
    public int ConstellationFragments { get; set; }
    
    // Statistics
    public int TotalConstellationsUnlocked { get; set; }
    public int TotalStarsActivated { get; set; }
    public int GoldSpentOnConstellations { get; set; }
    public int FragmentsCollected { get; set; }
    
    public override void _Ready()
    {
        // Initialize data
    }
    
    public Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>
        {
            { "unlocked_constellations", UnlockedConstellations },
            { "total_activation_points", TotalActivationPoints },
            { "used_activation_points", UsedActivationPoints },
            { "constellation_fragments", ConstellationFragments },
            { "total_constellations_unlocked", TotalConstellationsUnlocked },
            { "total_stars_activated", TotalStarsActivated },
            { "gold_spent_on_constellations", GoldSpentOnConstellations },
            { "fragments_collected", FragmentsCollected }
        };
    }
    
    public void Deserialize(Dictionary<string, object> data)
    {
        if (data.ContainsKey("unlocked_constellations"))
            UnlockedConstellations = (Dictionary<string, ConstellationProgress>)data["unlocked_constellations"];
        if (data.ContainsKey("total_activation_points"))
            TotalActivationPoints = (int)data["total_activation_points"];
        if (data.ContainsKey("used_activation_points"))
            UsedActivationPoints = (int)data["used_activation_points"];
        if (data.ContainsKey("constellation_fragments"))
            ConstellationFragments = (int)data["constellation_fragments"];
        if (data.ContainsKey("total_constellations_unlocked"))
            TotalConstellationsUnlocked = (int)data["total_constellations_unlocked"];
        if (data.ContainsKey("total_stars_activated"))
            TotalStarsActivated = (int)data["total_stars_activated"];
        if (data.ContainsKey("gold_spent_on_constellations"))
            GoldSpentOnConstellations = (int)data["gold_spent_on_constellations"];
        if (data.ContainsKey("fragments_collected"))
            FragmentsCollected = (int)data["fragments_collected"];
    }
}
