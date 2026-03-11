using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// Pet affection data structures - tracks player-pet relationship
    /// </summary>
    public class PetAffectionData {
        public enum AffectionLevel {
            Stranger = 1,      // 1-100
            Acquaintance = 2,  // 100-500
            Friend = 3,        // 500-1500
            CloseFriend = 4,   // 1500-3500
            BestFriend = 5,    // 3500-7000
            Beloved = 6,       // 7000-12000
            Devoted = 7,       // 12000-20000
            Soulmate = 8,      // 20000-35000
            Legend = 9,        // 35000-60000
            Mythic = 10        // 60000+
        }

        public string PetId { get; set; }
        public int CurrentAffection { get; set; }
        public int TotalInteractionCount { get; set; }
        public int FeedCount { get; set; }
        public int PlayCount { get; set; }
        public int BattleCount { get; set; }
        public int LastInteractionTime { get; set; }
        
        public AffectionLevel GetAffectionLevel() {
            if (CurrentAffection >= 60000) return AffectionLevel.Mythic;
            if (CurrentAffection >= 35000) return AffectionLevel.Legend;
            if (CurrentAffection >= 20000) return AffectionLevel.Soulmate;
            if (CurrentAffection >= 12000) return AffectionLevel.Devoted;
            if (CurrentAffection >= 7000) return AffectionLevel.Beloved;
            if (CurrentAffection >= 3500) return AffectionLevel.BestFriend;
            if (CurrentAffection >= 1500) return AffectionLevel.CloseFriend;
            if (CurrentAffection >= 500) return AffectionLevel.Friend;
            if (CurrentAffection >= 100) return AffectionLevel.Acquaintance;
            return AffectionLevel.Stranger;
        }

        public float GetAffectionBonus() {
            var level = GetAffectionLevel();
            return (int)level * 0.05f; // 5% per level
        }

        public int GetAffectionLevelNumber() {
            return (int)GetAffectionLevel();
        }

        public string GetAffectionTitle() {
            return GetAffectionLevel().ToString();
        }
    }

    /// <summary>
    /// Player's pet affection data
    /// </summary>
    public class PlayerAffectionData {
        public Dictionary<string, PetAffectionData> PetAffection { get; set; } = new Dictionary<string, PetAffectionData>();
    }
}
