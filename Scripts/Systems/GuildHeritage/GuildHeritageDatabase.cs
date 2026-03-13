using Godot;
using System;
using System.Collections.Generic;

public class GuildHeritageDatabase : Godot.Object
{
    // Transfer Type Configurations
    public Dictionary<string, TransferTypeConfig> TransferTypes { get; set; } = new Dictionary<string, TransferTypeConfig>();
    
    // Heritage Point Rewards
    public Dictionary<string, int> HeritagePointRewards { get; set; } = new Dictionary<string, int>();
    
    // Guild Level Requirements
    public Dictionary<int, int> GuildLevelRequirements { get; set; } = new Dictionary<int, int>();
    
    // Daily Limits
    public int MaxDailyTransfers { get; set; } = 3;
    public int MaxPendingTransfers { get; set; } = 5;
    
    // Tax Rate for Transfers
    public float GoldTaxRate { get; set; } = 0.05f; // 5% tax
    public float ExpTaxRate { get; set; } = 0.10f; // 10% tax
    
    // Cooldowns (in seconds)
    public int TransferCooldown { get; set; } = 3600; // 1 hour
    public int DailyResetHour { get; set; } = 0; // Midnight UTC
    
    // Heritage Point Costs
    public int FreeTransferCost { get; set; } = 10; // Points cost for free transfer
    public int InheritanceCost { get; set; } = 50; // Points cost for inheritance
    public int TeachingCost { get; set; } = 25; // Points cost for teaching
    
    // Transfer Limits
    public int MaxGoldPerTransfer { get; set; } = 100000;
    public int MaxExpPerTransfer { get; set; } = 50000;
    public int MaxItemsPerTransfer { get; set; } = 3;
    
    // Minimum Guild Level for Heritage
    public int MinGuildLevel { get; set; } = 3;
    
    // Points per Contribution
    public int PointsPerGold1000 { get; set; } = 1;
    public int PointsPerExp1000 { get; set; } = 1;
    public int PointsPerItem { get; set; } = 5;
    
    public GuildHeritageDatabase()
    {
        InitializeTransferTypes();
        InitializeHeritagePointRewards();
        InitializeGuildLevelRequirements();
    }
    
    private void InitializeTransferTypes()
    {
        // Gift - regular transfer between members
        TransferTypes["gift"] = new TransferTypeConfig
        {
            TypeId = "gift",
            DisplayName = "Gift",
            Description = "Transfer resources to a fellow guild member as a gift",
            Icon = "🎁",
            MaxGold = 50000,
            MaxExp = 25000,
            MaxItems = 2,
            TaxRate = 0.05f,
            RequiresLevel = 1,
            CooldownSeconds = 1800, // 30 minutes
            HeritagePointsCost = 5
        };
        
        // Inheritance - transfer from senior to junior member
        TransferTypes["inheritance"] = new TransferTypeConfig
        {
            TypeId = "inheritance",
            DisplayName = "Inheritance",
            Description = "Pass down resources from a senior member to a junior",
            Icon = "🏛️",
            MaxGold = 100000,
            MaxExp = 50000,
            MaxItems = 3,
            TaxRate = 0.02f,
            RequiresLevel = 5,
            CooldownSeconds = 86400, // 24 hours
            HeritagePointsCost = 20
        };
        
        // Teaching - transfer knowledge (exp) with bonus
        TransferTypes["teaching"] = new TransferTypeConfig
        {
            TypeId = "teaching",
            DisplayName = "Teaching",
            Description = "Share experience with fellow members",
            Icon = "📚",
            MaxGold = 10000,
            MaxExp = 50000,
            MaxItems = 1,
            TaxRate = 0.0f, // No tax for teaching
            RequiresLevel = 3,
            CooldownSeconds = 3600, // 1 hour
            HeritagePointsCost = 10
        };
    }
    
    private void InitializeHeritagePointRewards()
    {
        // Points earned by the giver for each transfer type
        HeritagePointRewards["gift"] = 5;
        HeritagePointRewards["inheritance"] = 15;
        HeritagePointRewards["teaching"] = 10;
        
        // Points earned for receiving
        HeritagePointRewards["receive_gift"] = 2;
        HeritagePointRewards["receive_inheritance"] = 5;
        HeritagePointRewards["receive_teaching"] = 3;
    }
    
    private void InitializeGuildLevelRequirements()
    {
        // Guild level required for each transfer type
        GuildLevelRequirements[1] = 1; // Gift - any level
        GuildLevelRequirements[3] = 2; // Teaching - guild level 2
        GuildLevelRequirements[5] = 3; // Inheritance - guild level 3
    }
    
    public TransferTypeConfig GetTransferType(string typeId)
    {
        if (TransferTypes.ContainsKey(typeId))
            return TransferTypes[typeId];
        return TransferTypes["gift"];
    }
    
    public int GetHeritagePointReward(string transferType)
    {
        if (HeritagePointRewards.ContainsKey(transferType))
            return HeritagePointRewards[transferType];
        return 5;
    }
    
    public int GetRequiredGuildLevel(string transferType)
    {
        if (GuildLevelRequirements.ContainsKey(GetTransferTypeOrder(transferType)))
            return GuildLevelRequirements[GetTransferTypeOrder(transferType)];
        return 1;
    }
    
    private int GetTransferTypeOrder(string typeId)
    {
        switch(typeId)
        {
            case "gift": return 1;
            case "teaching": return 3;
            case "inheritance": return 5;
            default: return 1;
        }
    }
}

public class TransferTypeConfig : Godot.Object
{
    public string TypeId { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public int MaxGold { get; set; }
    public int MaxExp { get; set; }
    public int MaxItems { get; set; }
    public float TaxRate { get; set; }
    public int RequiresLevel { get; set; }
    public int CooldownSeconds { get; set; }
    public int HeritagePointsCost { get; set; }
}
