using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Skill Mastery System - Players gain mastery points by using skills
    /// </summary>
    public class SkillMasteryData
    {
        public enum MasteryTier
        {
            Novice,      // 0-100 points
            Apprentice,  // 101-500 points
            Journeyman,  // 501-2000 points
            Expert,      // 2001-10000 points
            Master,      // 10001-50000 points
            GrandMaster  // 50001+ points
        }

        public enum SkillType
        {
            Attack,
            Defense,
            Support,
            Magic,
            Healing,
            Utility
        }

        [System.Serializable]
        public class SkillMastery
        {
            public string SkillId;
            public string SkillName;
            public SkillType Type;
            public int TotalUses;
            public int MasteryPoints;
            public MasteryTier Tier;
            public List<string> UnlockedBonuses = new List<string>();
            public DateTime LastUsed;
        }

        [System.Serializable]
        public class PlayerSkillMasteryData
        {
            public Dictionary<string, SkillMastery> Skills = new Dictionary<string, SkillMastery>();
            public int TotalMasteryPoints;
            public int HighestTierCount;
            public DateTime FirstMastery;
            public DateTime LastMastery;
        }

        [System.Serializable]
        public class MasteryBonus
        {
            public string BonusId;
            public string Name;
            public string Description;
            public MasteryTier RequiredTier;
            public SkillType RequiredType;
            public int RequiredPoints;
            public float BonusValue;
            public string StatBonus; // attack, defense, health, etc.
        }

        [System.Serializable]
        public class MasteryTierInfo
        {
            public MasteryTier Tier;
            public string DisplayName;
            public int MinPoints;
            public float DamageBonus;
            public float CooldownReduction;
            public float ManaCostReduction;
        }
    }
}
