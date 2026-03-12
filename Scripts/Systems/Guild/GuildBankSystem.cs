using Godot;
using System;
using System.Collections.Generic;

public partial class GuildBankSystem : Node {
    public static GuildBankSystem Instance { get; private set; }
    
    // 当前公会银行数据
    public GuildBankData BankData { get; private set; } = new GuildBankData();
    
    // 银行容量
    public int MaxSlots { get; private set; } = 50;
    public int MaxGoldPerDeposit { get; set; } = 100000;
    
    // 信号
    [Signal] public delegate void ItemDepositedEventHandler(GuildBankItem item);
    [Signal] public delegate void ItemWithdrawnEventHandler(GuildBankItem item);
    [Signal] public delegate void GoldDepositedEventHandler(int amount);
    [Signal] public delegate void GoldWithdrawnEventHandler(int amount);
    [Signal] public delegate void BankUpdatedEventHandler();
    [Signal] public delegate void PermissionChangedEventHandler();
    
    public override void _Ready() {
        Instance = this;
    }
    
    // 初始化银行（公会创建时调用）
    public void InitializeBank(string guildId) {
        BankData = new GuildBankData(guildId);
        GD.Print($"公会银行初始化: {guildId}");
    }
    
    // 加载银行数据
    public void LoadBankData(GuildBankData data) {
        if (data != null) {
            BankData = data;
            GD.Print($"公会银行数据加载: {BankData.Items.Count} 物品");
        }
    }
    
    // 存款物品
    public bool DepositItem(string itemId, string itemName, int quantity, string rarity, string iconPath = "") {
        if (GuildSystem.Instance?.CurrentGuild == null) {
            GD.PrintErr("玩家不在公会中");
            return false;
        }
        
        if (BankData.Items.Count >= MaxSlots) {
            GD.PrintErr("公会银行已满");
            return false;
        }
        
        var player = GetTree().CurrentScene.GetNode<Player>("Player");
        string playerName = player?.PlayerName ?? "Player";
        
        var item = new GuildBankItem {
            ItemId = itemId,
            ItemName = itemName,
            Quantity = quantity,
            Rarity = rarity,
            DepositorName = playerName,
            DepositTime = DateTime.Now,
            IconPath = iconPath
        };
        
        BankData.Items.Add(item);
        
        // 记录交易
        var transaction = new GuildBankTransaction {
            TransactionId = Guid.NewGuid().ToString(),
            Type = "deposit",
            ItemName = itemName,
            Quantity = quantity,
            PlayerName = playerName,
            Time = DateTime.Now
        };
        BankData.Transactions.Insert(0, transaction);
        
        // 限制交易记录数量
        if (BankData.Transactions.Count > 100) {
            BankData.Transactions.RemoveAt(BankData.Transactions.Count - 1);
        }
        
        EmitSignal(SignalName.ItemDeposited, item);
        EmitSignal(SignalName.BankUpdated);
        
        GD.Print($"物品存入公会银行: {itemName} x{quantity}");
        return true;
    }
    
    // 取回物品
    public bool WithdrawItem(int slotIndex) {
        if (GuildSystem.Instance?.CurrentGuild == null) {
            GD.PrintErr("玩家不在公会中");
            return false;
        }
        
        if (slotIndex < 0 || slotIndex >= BankData.Items.Count) {
            GD.PrintErr("无效的物品槽位");
            return false;
        }
        
        // 检查权限
        if (!CanWithdraw()) {
            GD.PrintErr("没有取回物品的权限");
            return false;
        }
        
        var item = BankData.Items[slotIndex];
        
        // 添加到玩家背包
        var inventory = Inventory.Instance;
        if (inventory != null) {
            if (!inventory.AddItem(item.ItemId, item.ItemName, item.Quantity, item.Rarity)) {
                GD.PrintErr("背包已满");
                return false;
            }
        }
        
        var player = GetTree().CurrentScene.GetNode<Player>("Player");
        string playerName = player?.PlayerName ?? "Player";
        
        // 记录交易
        var transaction = new GuildBankTransaction {
            TransactionId = Guid.NewGuid().ToString(),
            Type = "withdraw",
            ItemName = item.ItemName,
            Quantity = item.Quantity,
            PlayerName = playerName,
            Time = DateTime.Now
        };
        BankData.Transactions.Insert(0, transaction);
        
        // 限制交易记录数量
        if (BankData.Transactions.Count > 100) {
            BankData.Transactions.RemoveAt(BankData.Transactions.Count - 1);
        }
        
        // 移除物品
        BankData.Items.RemoveAt(slotIndex);
        
        EmitSignal(SignalName.ItemWithdrawn, item);
        EmitSignal(SignalName.BankUpdated);
        
        GD.Print($"物品从公会银行取出: {item.ItemName} x{item.Quantity}");
        return true;
    }
    
    // 存款金币
    public bool DepositGold(int amount) {
        if (GuildSystem.Instance?.CurrentGuild == null) {
            GD.PrintErr("玩家不在公会中");
            return false;
        }
        
        if (amount <= 0 || amount > MaxGoldPerDeposit) {
            GD.PrintErr($"无效的金币数量 (最大: {MaxGoldPerDeposit})");
            return false;
        }
        
        var player = GetTree().CurrentScene.GetNode<Player>("Player");
        if (player == null) return false;
        
        if (player.Gold < amount) {
            GD.PrintErr("金币不足");
            return false;
        }
        
        player.Gold -= amount;
        BankData.GoldDeposit += amount;
        BankData.TotalDeposits += amount;
        
        // 记录交易
        var transaction = new GuildBankTransaction {
            TransactionId = Guid.NewGuid().ToString(),
            Type = "gold_deposit",
            ItemName = "金币",
            Quantity = amount,
            PlayerName = player.PlayerName,
            Time = DateTime.Now
        };
        BankData.Transactions.Insert(0, transaction);
        
        // 限制交易记录数量
        if (BankData.Transactions.Count > 100) {
            BankData.Transactions.RemoveAt(BankData.Transactions.Count - 1);
        }
        
        EmitSignal(SignalName.GoldDeposited, amount);
        EmitSignal(SignalName.BankUpdated);
        
        GD.Print($"金币存入公会银行: {amount}");
        return true;
    }
    
    // 取回金币
    public bool WithdrawGold(int amount) {
        if (GuildSystem.Instance?.CurrentGuild == null) {
            GD.PrintErr("玩家不在公会中");
            return false;
        }
        
        if (!CanWithdraw()) {
            GD.PrintErr("没有取回金币的权限");
            return false;
        }
        
        if (amount <= 0 || amount > BankData.GoldDeposit) {
            GD.PrintErr("无效的金币数量");
            return false;
        }
        
        var player = GetTree().CurrentScene.GetNode<Player>("Player");
        if (player == null) return false;
        
        player.Gold += amount;
        BankData.GoldDeposit -= amount;
        
        // 记录交易
        var transaction = new GuildBankTransaction {
            TransactionId = Guid.NewGuid().ToString(),
            Type = "gold_withdraw",
            ItemName = "金币",
            Quantity = amount,
            PlayerName = player.PlayerName,
            Time = DateTime.Now
        };
        BankData.Transactions.Insert(0, transaction);
        
        // 限制交易记录数量
        if (BankData.Transactions.Count > 100) {
            BankData.Transactions.RemoveAt(BankData.Transactions.Count - 1);
        }
        
        EmitSignal(SignalName.GoldWithdrawn, amount);
        EmitSignal(SignalName.BankUpdated);
        
        GD.Print($"金币从公会银行取出: {amount}");
        return true;
    }
    
    // 检查是否可以取回
    public bool CanWithdraw() {
        if (BankData.AnyoneCanWithdraw) return true;
        
        var guildSystem = GuildSystem.Instance;
        if (guildSystem?.PlayerData.Level >= GuildLevel.Officer) return true;
        
        return false;
    }
    
    // 设置取回权限
    public void SetWithdrawPermission(bool anyoneCanWithdraw, int minLevel = 2) {
        BankData.AnyoneCanWithdraw = anyoneCanWithdraw;
        BankData.MinWithdrawLevel = minLevel;
        
        EmitSignal(SignalName.PermissionChanged);
        EmitSignal(SignalName.BankUpdated);
        
        GD.Print($"公会银行权限已更新: 自由取回={anyoneCanWithdraw}, 最低等级={minLevel}");
    }
    
    // 获取银行统计
    public Dictionary GetStats() {
        return new Dictionary {
            { "total_items", BankData.Items.Count },
            { "max_slots", MaxSlots },
            { "gold_deposit", BankData.GoldDeposit },
            { "total_deposits", BankData.TotalDeposits },
            { "transaction_count", BankData.Transactions.Count }
        };
    }
    
    // 获取交易历史
    public List<GuildBankTransaction> GetRecentTransactions(int count = 20) {
        int total = Mathf.Min(count, BankData.Transactions.Count);
        return BankData.Transactions.GetRange(0, total);
    }
}
