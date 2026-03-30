using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// Fate Relic data structures for roguelike-style relic collection
    /// </summary>
    public class FateRelicRarity : GodotObject {
        public string Name { get; set; }
        public string Color { get; set; }
        public float DropRate { get; set; }
        
        public static FateRelicRarity Common = new FateRelicRarity { Name = "Common", Color = "#9E9E9E", DropRate = 0.50f };
        public static FateRelicRarity Uncommon = new FateRelicRarity { Name = "Uncommon", Color = "#4CAF50", DropRate = 0.25f };
        public static FateRelicRarity Rare = new FateRelicRarity { Name = "Rare", Color = "#2196F3", DropRate = 0.15f };
        public static FateRelicRarity Epic = new FateRelicRarity { Name = "Epic", Color = "#9C27B0", DropRate = 0.08f };
        public static FateRelicRarity Legendary = new FateRelicRarity { Name = "Legendary", Color = "#FF9800", DropRate = 0.02f };
        
        public static FateRelicRarity[] All = new[] { Common, Uncommon, Rare, Epic, Legendary };
    }
    
    public class FateRelicType : GodotObject {
        public string Name { get; set; }
        
        public static FateRelicType Combat = new FateRelicType { Name = "Combat" };
        public static FateRelicType Defense = new FateRelicType { Name = "Defense" };
        public static FateRelicType Utility = new FateRelicType { Name = "Utility" };
        public static FateRelicType Economic = new FateRelicType { Name = "Economic" };
        public static FateRelicType Special = new FateRelicType { Name = "Special" };
        
        public static FateRelicType[] All = new[] { Combat, Defense, Utility, Economic, Special };
    }
    
    public class FateRelicEffect : GodotObject {
        public string Stat { get; set; }
        public float Value { get; set; }
        public string Description { get; set; }
    }
    
    public class FateRelic : GodotObject {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public FateRelicRarity Rarity { get; set; }
        public FateRelicType Type { get; set; }
        public List<FateRelicEffect> Effects { get; set; }
        public bool IsEquipped { get; set; }
        public bool IsActive { get; set; }
        public int StackCount { get; set; }
        
        public FateRelic() {
            Effects = new List<FateRelicEffect>();
            IsEquipped = false;
            IsActive = true;
            StackCount = 1;
        }
    }
    
    public class PlayerFateRelicData : GodotObject {
        public List<string> OwnedRelicIds { get; set; }
        public Dictionary<string, bool> EquippedRelics { get; set; }
        public Dictionary<string, int> RelicStacks { get; set; }
        public int MaxRelicSlots { get; set; }
        public int GoldSpentOnRelics { get; set; }
        public int RelicsDiscovered { get; set; }
        public int RelicsCompleted { get; set; }
        
        public PlayerFateRelicData() {
            OwnedRelicIds = new List<string>();
            EquippedRelics = new Dictionary<string, bool>();
            RelicStacks = new Dictionary<string, int>();
            MaxRelicSlots = 3;
            GoldSpentOnRelics = 0;
            RelicsDiscovered = 0;
            RelicsCompleted = 0;
        }
    }
}
