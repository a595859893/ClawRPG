using Godot;
using System;
using System.Collections.Generic;

public class EquipmentEnhancementData
{
    public enum EnhancementType
    {
        Attack,
        Defense,
        Health,
        Magic,
        Speed,
        CriticalRate,
        CriticalDamage,
        LifeSteal,
        Dodge,
        Resilience
    }

    public enum EnhancementRarity
    {
        Normal = 0,
        Enhanced = 1,
        Superior = 2,
        Epic = 3,
        Legendary = 4
    }

    public enum EnhancementResult
    {
        Success,
        Failure,
        CriticalSuccess,
        CriticalFailure
    }

    [System.Serializable]
    public class EnhancementRecipe
    {
        public EnhancementType Type;
        public int Level;
        public int SuccessRate;
        public int CriticalRate;
        public int GoldCost;
        public List<int> MaterialIds = new List<int>();
        public List<int> MaterialCounts = new List<int>();
    }

    [System.Serializable]
    public class PlayerEnhancementData
    {
        public int TotalEnhancements = 0;
        public int SuccessfulEnhancements = 0;
        public int FailedEnhancements = 0;
        public int CriticalSuccesses = 0;
        public int CriticalFailures = 0;
        public Dictionary<int, int> EquipmentEnhancementLevels = new Dictionary<int, int>();
    }
}
