using Godot;
using System;
using System.Collections.Generic;

public class GuildHeritageSystem : BaseSystem
{
    private static GuildHeritageSystem _instance;
    public static GuildHeritageSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GuildHeritageSystem();
            }
            return _instance;
        }
    }
    
    private GuildHeritageData _data;
    private GuildHeritageDatabase _database;
    
    public GuildHeritageData Data => _data;
    public GuildHeritageDatabase Database => _database;
    
    public GuildHeritageSystem()
    {
        _data = new GuildHeritageData();
        _database = GuildHeritageDatabase.Instance;
    }
    
    public void Initialize()
    {
        GD.Print("[GuildHeritageSystem] Initialized");
        LoadData();
    }
    
    // ==================== Transfer Operations ====================
    
    public bool CanTransfer(int playerId, string transferType, int goldAmount, int expAmount, int itemCount)
    {
        var config = _database.GetTransferType(transferType);
        
        // Check daily limit
        CheckDailyReset();
        if (_data.TransfersToday >= _data.DailyTransferLimit)
        {
            GD.PrintW("[GuildHeritageSystem] Daily transfer limit reached");
            return false;
        }
        
        // Check cooldown
        string cooldownKey = playerId + "_" + transferType;
        if (_data.TransferCooldowns.ContainsKey(cooldownKey))
        {
            int cooldownEnd = _data.TransferCooldowns[cooldownKey];
            if (OS.GetUnixTime() < cooldownEnd)
            {
                GD.PrintW("[GuildHeritageSystem] Transfer on cooldown");
                return false;
            }
        }
        
        // Check amount limits
        if (goldAmount > config.MaxGold || goldAmount < 0)
        {
            GD.PrintW("[GuildHeritageSystem] Gold amount exceeds limit");
            return false;
        }
        
        if (expAmount > config.MaxExp || expAmount < 0)
        {
            GD.PrintW("[GuildHeritageSystem] Exp amount exceeds limit");
            return false;
        }
        
        if (itemCount > config.MaxItems)
        {
            GD.PrintW("[GuildHeritageSystem] Item count exceeds limit");
            return false;
        }
        
        // Check heritage points
        if (_data.HeritagePoints < config.HeritagePointsCost)
        {
            GD.PrintW("[GuildHeritageSystem] Not enough heritage points");
            return false;
        }
        
        return true;
    }
    
    public HeritageTransfer CreateTransfer(
        int fromPlayerId, 
        string fromPlayerName,
        int toPlayerId, 
        string toPlayerName,
        int goldAmount,
        int expAmount,
        List<string> items,
        string transferType,
        string message)
    {
        if (!CanTransfer(fromPlayerId, transferType, goldAmount, expAmount, items.Count))
        {
            GD.PrintE("[GuildHeritageSystem] Cannot create transfer");
            return null;
        }
        
        var config = _database.GetTransferType(transferType);
        
        // Calculate tax
        int goldTax = (int)(goldAmount * config.TaxRate);
        int expTax = (int)(expAmount * config.TaxRate);
        
        int finalGold = goldAmount - goldTax;
        int finalExp = expAmount - expTax;
        
        // Create transfer
        var transfer = new HeritageTransfer
        {
            TransferId = _data.PendingTransfers.Count + 1,
            FromPlayerId = fromPlayerId,
            FromPlayerName = fromPlayerName,
            ToPlayerId = toPlayerId,
            ToPlayerName = toPlayerName,
            GoldAmount = finalGold,
            ExpAmount = finalExp,
            Items = new List<string>(items),
            HeritagePointsCost = config.HeritagePointsCost,
            Message = message,
            TransferType = transferType
        };
        
        // Deduct heritage points
        _data.HeritagePoints -= config.HeritagePointsCost;
        
        // Add to pending transfers
        _data.PendingTransfers.Add(transfer);
        
        // Update cooldown
        string cooldownKey = fromPlayerId + "_" + transferType;
        _data.TransferCooldowns[cooldownKey] = OS.GetUnixTime() + config.CooldownSeconds;
        
        // Increment daily counter
        _data.TransfersToday++;
        
        SaveData();
        
        GD.Print("[GuildHeritageSystem] Transfer created: " + transfer.TransferId);
        return transfer;
    }
    
    public bool AcceptTransfer(int transferId, int playerId)
    {
        HeritageTransfer transfer = null;
        foreach (var t in _data.PendingTransfers)
        {
            if (t.TransferId == transferId)
            {
                transfer = t;
                break;
            }
        }
        
        if (transfer == null)
        {
            GD.PrintE("[GuildHeritageSystem] Transfer not found");
            return false;
        }
        
        if (transfer.ToPlayerId != playerId)
        {
            GD.PrintE("[GuildHeritageSystem] Not the intended recipient");
            return false;
        }
        
        if (transfer.Status != "pending")
        {
            GD.PrintE("[GuildHeritageSystem] Transfer is not pending");
            return false;
        }
        
        // Accept transfer
        transfer.Status = "accepted";
        
        // Update statistics
        _data.TotalTransfers++;
        _data.TotalGoldTransferred += transfer.GoldAmount;
        _data.TotalExpTransferred += transfer.ExpAmount;
        _data.TotalItemsTransferred += transfer.Items.Count;
        
        // Award heritage points to receiver
        string receiveKey = "receive_" + transfer.TransferType;
        int receivePoints = _database.GetHeritagePointReward(receiveKey);
        _data.HeritagePoints += receivePoints;
        _data.TotalHeritagePointsEarned += receivePoints;
        
        // Award points to giver
        int giverPoints = _database.GetHeritagePointReward(transfer.TransferType);
        _data.HeritagePoints += giverPoints;
        _data.TotalHeritagePointsEarned += giverPoints;
        
        // Add to history
        var record = new HeritageRecord
        {
            RecordId = _data.HeritageRecords.Count + 1,
            FromPlayerId = transfer.FromPlayerId,
            FromPlayerName = transfer.FromPlayerName,
            ToPlayerId = transfer.ToPlayerId,
            ToPlayerName = transfer.ToPlayerName,
            GoldAmount = transfer.GoldAmount,
            ExpAmount = transfer.ExpAmount,
            Items = new List<string>(transfer.Items),
            HeritagePoints = giverPoints + receivePoints,
            TransferType = transfer.TransferType
        };
        _data.HeritageRecords[record.RecordId] = record;
        _data.TransferHistory.Add(transfer);
        
        // Remove from pending
        _data.PendingTransfers.Remove(transfer);
        
        SaveData();
        
        GD.Print("[GuildHeritageSystem] Transfer accepted: " + transferId);
        return true;
    }
    
    public bool RejectTransfer(int transferId, int playerId)
    {
        HeritageTransfer transfer = null;
        foreach (var t in _data.PendingTransfers)
        {
            if (t.TransferId == transferId)
            {
                transfer = t;
                break;
            }
        }
        
        if (transfer == null)
        {
            GD.PrintE("[GuildHeritageSystem] Transfer not found");
            return false;
        }
        
        if (transfer.ToPlayerId != playerId)
        {
            GD.PrintE("[GuildHeritageSystem] Not the intended recipient");
            return false;
        }
        
        // Reject transfer - return points to sender
        _data.HeritagePoints += transfer.HeritagePointsCost;
        
        transfer.Status = "rejected";
        _data.TransferHistory.Add(transfer);
        _data.PendingTransfers.Remove(transfer);
        
        SaveData();
        
        GD.Print("[GuildHeritageSystem] Transfer rejected: " + transferId);
        return true;
    }
    
    // ==================== Heritage Points ====================
    
    public void EarnHeritagePoints(int playerId, int goldAmount, int expAmount, int itemCount)
    {
        int points = 0;
        
        // Points for contributions
        points += (goldAmount / 1000) * _database.PointsPerGold1000;
        points += (expAmount / 1000) * _database.PointsPerExp1000;
        points += itemCount * _database.PointsPerItem;
        
        _data.HeritagePoints += points;
        _data.TotalHeritagePointsEarned += points;
        
        SaveData();
    }
    
    // ==================== Query Operations ====================
    
    public List<HeritageTransfer> GetPendingTransfers(int playerId)
    {
        List<HeritageTransfer> result = new List<HeritageTransfer>();
        foreach (var t in _data.PendingTransfers)
        {
            if (t.ToPlayerId == playerId && t.Status == "pending")
            {
                result.Add(t);
            }
        }
        return result;
    }
    
    public List<HeritageTransfer> GetSentTransfers(int playerId)
    {
        List<HeritageTransfer> result = new List<HeritageTransfer>();
        foreach (var t in _data.PendingTransfers)
        {
            if (t.FromPlayerId == playerId)
            {
                result.Add(t);
            }
        }
        return result;
    }
    
    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            { "total_transfers", _data.TotalTransfers },
            { "total_gold", _data.TotalGoldTransferred },
            { "total_exp", _data.TotalExpTransferred },
            { "total_items", _data.TotalItemsTransferred },
            { "heritage_points", _data.HeritagePoints },
            { "total_points_earned", _data.TotalHeritagePointsEarned },
            { "members_used", _data.MembersUsed },
            { "transfers_today", _data.TransfersToday }
        };
    }
    
    public int GetRemainingDailyTransfers()
    {
        CheckDailyReset();
        return _data.DailyTransferLimit - _data.TransfersToday;
    }
    
    // ==================== Private Methods ====================
    
    private void CheckDailyReset()
    {
        var now = DateTime.UtcNow;
        string today = now.ToString("yyyy-MM-dd");
        
        if (_data.LastTransferDate != today)
        {
            _data.TransfersToday = 0;
            _data.LastTransferDate = today;
            SaveData();
        }
    }
    
    private void SaveData()
    {
        // Save to file
        string path = "user://guild_heritage_save.json";
        var file = new File();
        
        try
        {
            if (file.Open(path, File.ModeFlags.Write))
            {
                string json = Godot.Json.Serialize(_data);
                file.StoreString(json);
                file.Close();
            }
        }
        catch (Exception e)
        {
            GD.PrintE("[GuildHeritageSystem] Save error: " + e.Message);
        }
    }
    
    private void LoadData()
    {
        string path = "user://guild_heritage_save.json";
        var file = new File();
        
        try
        {
            if (file.FileExists(path) && file.Open(path, File.ModeFlags.Read))
            {
                string json = file.GetAsText();
                file.Close();
                
                var result = (Godot.Collections.Dictionary)Godot.Json.Parse(json);
                if (result != null)
                {
                    // Load data
                    GD.Print("[GuildHeritageSystem] Data loaded successfully");
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintE("[GuildHeritageSystem] Load error: " + e.Message);
        }
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary
        {
            { "heritage_points", _data.HeritagePoints },
            { "total_transfers", _data.TotalTransfers },
            { "total_gold_transferred", _data.TotalGoldTransferred },
            { "total_exp_transferred", _data.TotalExpTransferred },
            { "total_items_transferred", _data.TotalItemsTransferred },
            { "total_heritage_points_earned", _data.TotalHeritagePointsEarned },
            { "members_used", _data.MembersUsed },
            { "transfers_today", _data.TransfersToday },
            { "last_transfer_date", _data.LastTransferDate }
        };
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        _data.HeritagePoints = data.GetValueOrDefault("heritage_points", 0);
        _data.TotalTransfers = data.GetValueOrDefault("total_transfers", 0);
        _data.TotalGoldTransferred = data.GetValueOrDefault("total_gold_transferred", 0);
        _data.TotalExpTransferred = data.GetValueOrDefault("total_exp_transferred", 0);
        _data.TotalItemsTransferred = data.GetValueOrDefault("total_items_transferred", 0);
        _data.TotalHeritagePointsEarned = data.GetValueOrDefault("total_heritage_points_earned", 0);
        _data.MembersUsed = data.GetValueOrDefault("members_used", 0);
        _data.TransfersToday = data.GetValueOrDefault("transfers_today", 0);
        _data.LastTransferDate = data.GetValueOrDefault("last_transfer_date", "");
    }
}
