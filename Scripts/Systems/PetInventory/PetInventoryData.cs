using Godot;
using System;
using System.Collections.Generic;

public class PetInventoryItem {
    public string ItemId { get; set; }
    public string ItemName { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
    public string Rarity { get; set; }
    public string Category { get; set; }
    public int Value { get; set; }
    public Dictionary<string, float> Stats { get; set; }
    public string SpecialEffect { get; set; }
    public DateTime AcquiredAt { get; set; }
}

public class PetInventoryData : BaseSystem {
    public Dictionary<string, List<PetInventoryItem>> PetInventories { get; set; }
    public int TotalSlots { get; set; }
    public Dictionary<string, int> GoldByPet { get; set; }
    public List<string> UnlockedCategories { get; set; }
    public Dictionary<string, int> ItemUsageCount { get; set; }
    public int TotalItemsCollected { get; set; }
    public int TotalItemsUsed { get; set; }
    public int TotalGoldSpent { get; set; }
    
    public override void _Ready() {
        base._Ready();
        Initialize();
    }
    
    public void Initialize() {
        if (PetInventories == null) {
            PetInventories = new Dictionary<string, List<PetInventoryItem>>();
        }
        if (GoldByPet == null) {
            GoldByPet = new Dictionary<string, int>();
        }
        if (UnlockedCategories == null) {
            UnlockedCategories = new List<string> { "All" };
        }
        if (ItemUsageCount == null) {
            ItemUsageCount = new Dictionary<string, int>();
        }
        if (TotalSlots == 0) {
            TotalSlots = 50;
        }
    }

        public override Dictionary ExportSaveData() => new();
        public override void ImportSaveData(Dictionary data) { }
}
