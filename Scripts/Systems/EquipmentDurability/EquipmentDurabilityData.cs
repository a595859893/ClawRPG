using Godot;
using System;
using System.Collections.Generic;

public class EquipmentDurabilityData
{
    public enum DurabilityState
    {
        Excellent,      // 100%-80%
        Good,           // 79%-50%
        Worn,           // 49%-20%
        Damaged,        // 19%-1%
        Broken          // 0%
    }

    public class EquipmentDurability
    {
        public string ItemId { get; set; }
        public int CurrentDurability { get; set; }
        public int MaxDurability { get; set; }
        
        public float DurabilityPercent => MaxDurability > 0 ? (float)CurrentDurability / MaxDurability : 0;
        
        public DurabilityState State
        {
            get
            {
                float percent = DurabilityPercent * 100;
                if (percent >= 80) return DurabilityState.Excellent;
                if (percent >= 50) return DurabilityState.Good;
                if (percent >= 20) return DurabilityState.Worn;
                if (percent >= 1) return DurabilityState.Damaged;
                return DurabilityState.Broken;
            }
        }
    }

    public class PlayerDurabilityData
    {
        public Dictionary<string, EquipmentDurability> EquippedDurability { get; set; } = new Dictionary<string, EquipmentDurability>();
        public int TotalRepairs { get; set; }
        public int TotalRepairCost { get; set; }
        public int TimesUsedRepairKit { get; set; }
    }
}
