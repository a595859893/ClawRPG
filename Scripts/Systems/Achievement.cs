using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Achievement data class - defines achievement structure
    /// </summary>
    public class Achievement
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AchievementType Type { get; set; }
        public AchievementDifficulty Difficulty { get; set; }
        public int RequiredValue { get; set; }
        public int CurrentValue { get; set; }
        public bool IsUnlocked { get; set; }
        public DateTime? UnlockedTime { get; set; }
        public int RewardGold { get; set; }
        public int RewardExp { get; set; }
        
        public float Progress => RequiredValue > 0 ? (float)CurrentValue / RequiredValue : 0f;
        public bool CanUnlock => !IsUnlocked && CurrentValue >= RequiredValue;
        
        public void Unlock()
        {
            if (!IsUnlocked && CurrentValue >= RequiredValue)
            {
                IsUnlocked = true;
                UnlockedTime = DateTime.Now;
            }
        }
        
        public void AddProgress(int amount)
        {
            if (!IsUnlocked)
            {
                CurrentValue = Mathf.Min(CurrentValue + amount, RequiredValue);
                if (CurrentValue >= RequiredValue)
                {
                    Unlock();
                }
            }
        }
    }
    
    public enum AchievementType
    {
        Kill,
        Collect,
        Explore,
        Craft,
        LevelUp,
        Quest,
        Skill,
        Boss,
        Survival,
        Combo,
        Damage,
        Gold
    }
    
    public enum AchievementDifficulty
    {
        Easy,
        Normal,
        Hard,
        Epic,
        Legendary
    }
}
