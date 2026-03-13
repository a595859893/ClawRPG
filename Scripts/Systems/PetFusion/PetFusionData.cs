using Godot;
using System;
using System.Collections.Generic;

public class PetFusionData : Resource {
    // 宠物融合数据
    public Dictionary<int, PetFusionRecord> FusionHistory { get; set; } = new Dictionary<int, PetFusionRecord>();
    public int TotalFusions { get; set; } = 0;
    public int SuccessfulFusions { get; set; } = 0;
    public int LegendaryFusions { get; set; } = 0;
    public int TotalGoldSpent { get; set; } = 0;
    public List<string> UnlockedFusionTypes { get; set; } = new List<string>();
    
    public PetFusionData() {
        // 初始化默认记录
    }
}

public class PetFusionRecord {
    public int Id { get; set; }
    public int ParentPet1Id { get; set; }
    public int ParentPet2Id { get; set; }
    public string Parent1Type { get; set; } = "";
    public string Parent2Type { get; set; } = "";
    public string ResultPetType { get; set; } = "";
    public string ResultRarity { get; set; } = "";
    public int GoldCost { get; set; }
    public bool WasSuccessful { get; set; }
    public long Timestamp { get; set; }
    
    public PetFusionRecord() {
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}

public enum PetFusionResult {
    Failure,
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
