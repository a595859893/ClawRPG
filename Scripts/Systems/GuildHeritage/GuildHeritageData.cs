using Godot;
using System;
using System.Collections.Generic;

public class GuildHeritageData : Godot.Object
{
    // Heritage Records - tracks what has been passed down
    public Dictionary<int, HeritageRecord> HeritageRecords { get; set; } = new Dictionary<int, HeritageRecord>();
    
    // Pending Transfers - transfers waiting to be accepted
    public List<HeritageTransfer> PendingTransfers { get; set; } = new List<HeritageTransfer>();
    
    // Transfer History
    public List<HeritageTransfer> TransferHistory { get; set; } = new List<HeritageTransfer>();
    
    // Statistics
    public int TotalTransfers { get; set; } = 0;
    public int TotalGoldTransferred { get; set; } = 0;
    public int TotalExpTransferred { get; set; } = 0;
    public int TotalItemsTransferred { get; set; } = 0;
    public int MembersUsed { get; set; } = 0;
    
    // Heritage Points - earned by contributing to guild
    public int HeritagePoints { get; set; } = 0;
    public int TotalHeritagePointsEarned { get; set; } = 0;
    
    // Cooldowns
    public Dictionary<string, int> TransferCooldowns { get; set; } = new Dictionary<string, int>();
    public int DailyTransferLimit { get; set; } = 3;
    public int TransfersToday { get; set; } = 0;
    public string LastTransferDate { get; set; } = "";
    
    // Guild Level Requirement
    public int MinGuildLevelForHeritage { get; set; } = 3;
    
    public GuildHeritageData()
    {
    }
}

public class HeritageRecord : Godot.Object
{
    public int RecordId { get; set; }
    public int FromPlayerId { get; set; }
    public string FromPlayerName { get; set; }
    public int ToPlayerId { get; set; }
    public string ToPlayerName { get; set; }
    public int GoldAmount { get; set; }
    public int ExpAmount { get; set; }
    public List<string> Items { get; set; } = new List<string>();
    public int HeritagePoints { get; set; }
    public long Timestamp { get; set; }
    public string TransferType { get; set; } // "gift", "inheritance", "teaching"
    
    public HeritageRecord()
    {
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}

public class HeritageTransfer : Godot.Object
{
    public int TransferId { get; set; }
    public int FromPlayerId { get; set; }
    public string FromPlayerName { get; set; }
    public int ToPlayerId { get; set; }
    public string ToPlayerName { get; set; }
    public int GoldAmount { get; set; }
    public int ExpAmount { get; set; }
    public List<string> Items { get; set; } = new List<string>();
    public int HeritagePointsCost { get; set; }
    public string Message { get; set; }
    public long Timestamp { get; set; }
    public string Status { get; set; } // "pending", "accepted", "rejected", "expired"
    public string TransferType { get; set; } // "gift", "inheritance", "teaching"
    
    public HeritageTransfer()
    {
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Status = "pending";
    }
}
