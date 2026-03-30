using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Prestige System - allows players to reset their progress for special rewards
    /// Based on Player Progression & Leveling System Design learning
    /// </summary>
    public partial class PrestigeSystem : BaseSystem
    {
        public static PrestigeSystem Instance { get; private set; }
        
        // Prestige levels (0 = not prestiged yet)
        public int PrestigeLevel { get; private set; } = 0;
        
        // Prestige currency earned
        public int PrestigePoints { get; private set; } = 0;
        
        // Total prestige points earned (lifetime)
        public int TotalPrestigePointsEarned { get; private set; } = 0;
        
        // Number of times prestiged
        public int TimesPrestiged { get; private set; } = 0;
        
        // Experience multiplier per prestige level
        private const float EXP_MULTIPLIER_PER_LEVEL = 0.1f;
        
        // Gold drop multiplier per prestige level
        private const float GOLD_MULTIPLIER_PER_LEVEL = 0.15f;
        
        // Required level to prestige
        private const int REQUIRED_LEVEL = 100;
        
        // Points earned per prestige (base)
        private const int BASE_PRESTIGE_POINTS = 100;
        
        // Points multiplier per prestige level
        private const float POINTS_MULTIPLIER_PER_LEVEL = 1.5f;
        
        // Attribute bonus per prestige level
        private const int ATTRIBUTE_BONUS_PER_LEVEL = 5;
        
        // Max prestige level
        public const int MAX_PRESTIGE_LEVEL = 20;
        
        // Prestige thresholds
        private static readonly int[] LevelThresholds = { 0, 1, 2, 3, 4, 5, 7, 10, 15, 20, 25, 30, 40, 50, 60, 75, 90, 110, 130, 150, 200 };
        
        public override void _Ready()
        {
            Instance = this;
            GD.Print("=== Prestige System Initialized ===");
        }
        
        /// <summary>
        /// Calculate the prestige points reward for a prestige
        /// </summary>
        public int CalculatePrestigePointsReward()
        {
            return (int)(BASE_PRESTIGE_POINTS * Math.Pow(POINTS_MULTIPLIER_PER_LEVEL, PrestigeLevel));
        }
        
        /// <summary>
        /// Check if player can prestige
        /// </summary>
        public bool CanPrestige(int playerLevel)
        {
            return playerLevel >= REQUIRED_LEVEL && PrestigeLevel < MAX_PRESTIGE_LEVEL;
        }
        
        /// <summary>
        /// Get the required level for next prestige
        /// </summary>
        public int GetRequiredLevelForPrestige()
        {
            if (PrestigeLevel >= MAX_PRESTIGE_LEVEL)
                return -1;
            return LevelThresholds[Math.Min(PrestigeLevel, LevelThresholds.Length - 1)];
        }
        
        /// <summary>
        /// Perform prestige reset
        /// </summary>
        public bool PerformPrestige(int playerLevel, int playerExp, int playerGold)
        {
            if (!CanPrestige(playerLevel))
                return false;
            
            // Calculate rewards
            int pointsEarned = CalculatePrestigePointsReward();
            
            // Add prestige points
            PrestigePoints += pointsEarned;
            TotalPrestigePointsEarned += pointsEarned;
            PrestigeLevel++;
            TimesPrestiged++;
            
            GD.Print($"Prestige performed! Level: {PrestigeLevel}, Points earned: {pointsEarned}");
            GD.Print($"Total Prestige Points: {PrestigePoints}");
            
            return true;
        }
        
        /// <summary>
        /// Get experience multiplier based on prestige level
        /// </summary>
        public float GetExperienceMultiplier()
        {
            return 1.0f + (PrestigeLevel * EXP_MULTIPLIER_PER_LEVEL);
        }
        
        /// <summary>
        /// Get gold drop multiplier based on prestige level
        /// </summary>
        public float GetGoldMultiplier()
        {
            return 1.0f + (PrestigeLevel * GOLD_MULTIPLIER_PER_LEVEL);
        }
        
        /// <summary>
        /// Get attribute bonus based on prestige level
        /// </summary>
        public int GetAttributeBonus()
        {
            return PrestigeLevel * ATTRIBUTE_BONUS_PER_LEVEL;
        }
        
        /// <summary>
        /// Get prestige tier name
        /// </summary>
        public string GetPrestigeTierName()
        {
            if (PrestigeLevel == 0)
                return "None";
            else if (PrestigeLevel <= 3)
                return "Bronze";
            else if (PrestigeLevel <= 6)
                return "Silver";
            else if (PrestigeLevel <= 10)
                return "Gold";
            else if (PrestigeLevel <= 15)
                return "Platinum";
            else if (PrestigeLevel <= 19)
                return "Diamond";
            else
                return "Legendary";
        }
        
        /// <summary>
        /// Get prestige tier color
        /// </summary>
        public string GetPrestigeTierColor()
        {
            if (PrestigeLevel == 0)
                return "#FFFFFF";
            else if (PrestigeLevel <= 3)
                return "#CD7F32"; // Bronze
            else if (PrestigeLevel <= 6)
                return "#C0C0C0"; // Silver
            else if (PrestigeLevel <= 10)
                return "#FFD700"; // Gold
            else if (PrestigeLevel <= 15)
                return "#E5E4E2"; // Platinum
            else if (PrestigeLevel <= 19)
                return "#B9F2FF"; // Diamond
            else
                return "#FF6B6B"; // Legendary
        }
        
        /// <summary>
        /// Get prestige progress to next tier
        /// </summary>
        public float GetPrestigeProgress()
        {
            if (PrestigeLevel >= MAX_PRESTIGE_LEVEL)
                return 1.0f;
            
            // Calculate progress within current tier
            int currentThreshold = LevelThresholds[Math.Min(PrestigeLevel, LevelThresholds.Length - 1)];
            int nextThreshold = LevelThresholds[Math.Min(PrestigeLevel + 1, LevelThresholds.Length - 1)];
            
            if (nextThreshold <= currentThreshold)
                return 1.0f;
            
            // For now, return a simple progress based on prestige level
            return (float)PrestigeLevel / MAX_PRESTIGE_LEVEL;
        }
        
        /// <summary>
        /// Check if player has enough prestige points for a purchase
        /// </summary>
        public bool CanPurchase(int cost)
        {
            return PrestigePoints >= cost;
        }
        
        /// <summary>
        /// Spend prestige points
        /// </summary>
        public bool SpendPrestigePoints(int amount)
        {
            if (!CanPurchase(amount))
                return false;
            
            PrestigePoints -= amount;
            return true;
        }
        
        /// <summary>
        /// Get all prestige bonuses as a dictionary
        /// </summary>
        public Dictionary<string, Variant> GetAllBonuses()
        {
            var bonuses = new Dictionary<string, Variant>();
            bonuses["exp_multiplier"] = GetExperienceMultiplier();
            bonuses["gold_multiplier"] = GetGoldMultiplier();
            bonuses["attribute_bonus"] = GetAttributeBonus();
            bonuses["prestige_tier"] = GetPrestigeTierName();
            return bonuses;
        }
        
        /// <summary>
        /// Save prestige data
        /// </summary>
        public Dictionary<string, Variant> SaveData()
        {
            var data = new Dictionary<string, Variant>();
            data["prestige_level"] = PrestigeLevel;
            data["prestige_points"] = PrestigePoints;
            data["total_prestige_points_earned"] = TotalPrestigePointsEarned;
            data["times_prestiged"] = TimesPrestiged;
            return data;
        }
        
        /// <summary>
        /// Load prestige data
        /// </summary>
        public void LoadData(Dictionary<string, Variant> data)
        {
            if (data == null) return;
            
            if (data.ContainsKey("prestige_level"))
                PrestigeLevel = (int)data["prestige_level"];
            if (data.ContainsKey("prestige_points"))
                PrestigePoints = (int)data["prestige_points"];
            if (data.ContainsKey("total_prestige_points_earned"))
                TotalPrestigePointsEarned = (int)data["total_prestige_points_earned"];
            if (data.ContainsKey("times_prestiged"))
                TimesPrestiged = (int)data["times_prestiged"];
            
            GD.Print($"Prestige data loaded: Level {PrestigeLevel}, Points {PrestigePoints}");
        }
        
        // ===== 持久化 =====
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["prestige_level"] = PrestigeLevel;
            data["prestige_points"] = PrestigePoints;
            data["total_prestige_points_earned"] = TotalPrestigePointsEarned;
            data["times_prestiged"] = TimesPrestiged;
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.ContainsKey("prestige_level"))
                PrestigeLevel = Convert.ToInt32(data["prestige_level"]);
            if (data.ContainsKey("prestige_points"))
                PrestigePoints = Convert.ToInt32(data["prestige_points"]);
            if (data.ContainsKey("total_prestige_points_earned"))
                TotalPrestigePointsEarned = Convert.ToInt32(data["total_prestige_points_earned"]);
            if (data.ContainsKey("times_prestiged"))
                TimesPrestiged = Convert.ToInt32(data["times_prestiged"]);
        }
    }
}
