using Godot;
using System;
using System.Collections.Generic;

public partial class GuildBankData : Resource {
    // 银行数据
    public string GuildId { get; set; } = "";
    public List<GuildBankItem> Items { get; set; } = new List<GuildBankItem>();
    public int GoldDeposit { get; set; } = 0;
    public int TotalDeposits { get; set; } = 0;
    
    // 存款记录
    public List<GuildBankTransaction> Transactions { get; set; } = new List<GuildBankTransaction>();
    
    // 权限设置
    public bool AnyoneCanWithdraw { get; set; } = false;
    public int MinWithdrawLevel { get; set; } = 2; // Officer+
    
    public GuildBankData() { }
    
    public GuildBankData(string guildId) {
        GuildId = guildId;
    }
}

public class GuildBankItem {
    public string ItemId { get; set; } = "";
    public string ItemName { get; set; } = "";
    public int Quantity { get; set; } = 1;
    public string Rarity { get; set; } = "Common";
    public string DepositorName { get; set; } = "";
    public DateTime DepositTime { get; set; } = DateTime.Now;
    public string IconPath { get; set; } = "";
}

public class GuildBankTransaction {
    public string TransactionId { get; set; } = "";
    public string Type { get; set; } = ""; // "deposit" or "withdraw"
    public string ItemName { get; set; } = "";
    public int Quantity { get; set; } = 0;
    public string PlayerName { get; set; } = "";
    public DateTime Time { get; set; } = DateTime.Now;
}
