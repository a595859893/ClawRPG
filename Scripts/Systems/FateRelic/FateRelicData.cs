using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// Fate Relic data structures for roguelike-style relic collection
    /// </summary>
    public class RelicRarity : Godot.Object {
        public string Name { get; set; }
        public string Color { get; set; }
        public float DropRate { get; set; }
        
        public static RelicRarity Common = new RelicRarity { Name = "Common", Color = "#9E9E9E", DropRate = 0.50f };
        public static RelicRarity Uncommon = new RelicRarity { Name = "Uncommon", Color = "#4CAF50", DropRate = 0.25f };
        public static RelicRarity Rare = new RelicRarity { Name = "Rare", Color = "#2196F3", DropRate = 0.15f };
        public static RelicRarity Epic = new RelicRarity { Name = "Epic", Color = "#9C27B0", DropRate = 0.08f };
        public static RelicRarity Legendary = new RelicRarity { Name = "Legendary", Color = "#FF9800", DropRate = 0.02f };
        
        public static RelicRarity[] All = new[] { Common, Uncommon, Rare, Epic, Legendary };
    }
    
    public class RelicType : Godot.Object {
        public string Name { get; set; }
        
        public static RelicType Combat = new RelicType { Name = "Combat" };
        public static RelicType Defense = new RelicType { Name = "Defense" };
        public static RelicType Utility = new RelicType { Name = "Utility" };
        public static RelicType Economic = new RelicType { Name = "Economic" };
        public static RelicType Special = new RelicType { Name = "Special" };
        
        public static RelicType[] All = new[] { Combat, Defense, Utility, Economic, Special };
    }
    
    public class RelicEffect : Godot.Object {
        public string Stat { get; set; }
        public float Value { get; set; }
        public string Description { get; set; }
    }
    
    public class FateRelic : Godot.Object {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public RelicRarity Rarity { get; set; }
        public RelicType Type { get; set; }
        public List<RelicEffect> Effects { get; set; }
        public bool IsEquipped { get; set; }
        public bool IsActive { get; set; }
        public int StackCount { get; set; }
        
        public FateRelic() {
            Effects = new List<RelicEffect>();
            IsEquipped = false;
            IsActive = true;
            StackCount = 1;
        }
    }
    
    public class PlayerFateRelicData : Godot.Object {
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
