using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.UI;
using SaveSystem = ClawRPG.Scripts.Framework.SaveSystem;

/// <summary>
/// Trade system that handles player-to-player item trading.
/// Supports trade offers, item exchange, and trade history tracking.
/// </summary>
public partial class TradeSystem : BaseSystem {
    /// <summary>
    /// Gets the singleton instance of the TradeSystem.
    /// </summary>
    public static TradeSystem Instance { get; private set; }

    /// <summary>
    /// Defines the current state of a trade.
    /// </summary>
    public enum TradeState {
        /// <summary>No active trade in progress.</summary>
        Idle,
        
        /// <summary>Trade offer is being prepared.</summary>
        Offering,
        
        /// <summary>Trade is actively being negotiated.</summary>
        Trading,
        
        /// <summary>Trade has been completed successfully.</summary>
        Completed,
        
        /// <summary>Trade was cancelled.</summary>
        Cancelled
    }

    /// <summary>
    /// Gets the current state of the trade system.
    /// </summary>
    /// <value>The current TradeState.</value>
    public TradeState CurrentState { get; private set; } = TradeState.Idle;

    /// <summary>
    /// Gets the current trade offer if one exists.
    /// </summary>
    /// <value>The current TradeOffer, or null if no offer exists.</value>
    public TradeOffer CurrentOffer { get; private set; }

    /// <summary>
    /// Gets the history of completed trades.
    /// </summary>
    /// <value>List of TradeRecord entries.</value>
    public List<TradeRecord> TradeHistory { get; private set; } = new List<TradeRecord>();

    // Signals
    
    /// <summary>
    /// Emitted when a new trade is started.
    /// </summary>
    public delegate void TradeStartedEventHandler();
    
    /// <summary>
    /// Emitted when a trade offer is updated.
    /// </summary>
    public delegate void OfferUpdatedEventHandler(TradeOffer offer);
    
    /// <summary>
    /// Emitted when a trade is accepted by both parties.
    /// </summary>
    public delegate void TradeAcceptedEventHandler();
    
    /// <summary>
    /// Emitted when a trade is completed successfully.
    /// </summary>
    public delegate void TradeCompletedEventHandler(TradeRecord record);
    
    /// <summary>
    /// Emitted when a trade is cancelled.
    /// </summary>
    public delegate void TradeCancelledEventHandler();
    
    /// <summary>
    /// Emitted when a trade fails.
    /// </summary>
    public delegate void TradeFailedEventHandler(string reason);

    public override void _Ready() {
        Instance = this;
        LoadTradeHistory();
    }

    // ===== Public Methods =====

    /// <summary>
    /// Starts a new trade with the specified player.
    /// </summary>
    /// <param name="targetPlayerId">Unique identifier of the target player.</param>
    /// <param name="targetPlayerName">Display name of the target player.</param>
    /// <returns>True if trade was started successfully, false otherwise.</returns>
    public bool StartTrade(string targetPlayerId, string targetPlayerName) {
        if (CurrentState != TradeState.Idle) {
            GD.PrintErr("当前已有进行中的交易");
            return false;
        }

        CurrentOffer = new TradeOffer {
            OfferId = Guid.NewGuid().ToString(),
            Player1Id = "player",
            Player1Name = "玩家",
            Player2Id = targetPlayerId,
            Player2Name = targetPlayerName,
            Player1Items = new List<ItemData>(),
            Player2Items = new List<ItemData>(),
            Player1Gold = 0,
            Player2Gold = 0,
            Player1Accepted = false,
            Player2Accepted = false,
            CreatedAt = DateTime.Now
        };

        CurrentState = TradeState.Offering;
        EmitSignal(SignalName.TradeStarted);
        return true;
    }

    /// <summary>
    /// Adds an item to the current trade offer.
    /// </summary>
    /// <param name="item">The item to add to the trade.</param>
    /// <param name="isPlayer1">True if adding for player 1, false for player 2.</param>
    /// <returns>True if item was added successfully, false otherwise.</returns>
    public bool AddItemToTrade(ItemData item, bool isPlayer1) {
        if (CurrentState != TradeState.Offering) {
            GD.PrintErr("当前不是报价状态");
            return false;
        }

        if (isPlayer1) {
            CurrentOffer.Player1Items.Add(item);
        } else {
            CurrentOffer.Player2Items.Add(item);
        }

        // 重置接受状态
        CurrentOffer.Player1Accepted = false; 
        CurrentOffer.Player2Accepted = false; 

        EmitSignal(SignalName.OfferUpdated, CurrentOffer);
        return true;
    }

    /// <summary>
    /// Removes an item from the current trade offer.
    /// </summary>
    /// <param name="item">The item to remove from the trade.</param>
    /// <param name="isPlayer1">True if removing for player 1, false for player 2.</param>
    /// <returns>True if item was removed successfully, false otherwise.</returns>
    public bool RemoveItemFromTrade(ItemData item, bool isPlayer1) {
        if (CurrentState != TradeState.Offering) {
            return false;
        }

        var items = isPlayer1 ? CurrentOffer.Player1Items : CurrentOffer.Player2Items;
        if (items.Remove(item)) {
            CurrentOffer.Player1Accepted = false; 
            CurrentOffer.Player2Accepted = false; 
            EmitSignal(SignalName.OfferUpdated, CurrentOffer);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Sets the gold amount for a player in the current trade.
    /// </summary>
    /// <param name="gold">Amount of gold to set.</param>
    /// <param name="isPlayer1">True if setting for player 1, false for player 2.</param>
    /// <returns>True if gold was set successfully, false otherwise.</returns>
    public bool SetGold(int gold, bool isPlayer1) {
        if (CurrentState != TradeState.Offering) {
            return false;
        }

        if (isPlayer1) {
            CurrentOffer.Player1Gold = gold;
        } else {
            CurrentOffer.Player2Gold = gold;
        }

        CurrentOffer.Player1Accepted = false; 
        CurrentOffer.Player2Accepted = false; 
        EmitSignal(SignalName.OfferUpdated, CurrentOffer);
        return true;
    }

    /// <summary>
    /// Accepts the current trade offer.
    /// </summary>
    /// <param name="isPlayer1">True if player 1 is accepting, false if player 2.</param>
    /// <returns>True if acceptance was recorded, false if not in offering state.</returns>
    public bool AcceptTrade(bool isPlayer1) {
        if (CurrentState != TradeState.Offering) {
            return false;
        }

        if (isPlayer1) {
            CurrentOffer.Player1Accepted = true;
        } else {
            CurrentOffer.Player2Accepted = true;
        }

        EmitSignal(SignalName.OfferUpdated, CurrentOffer);

        // 双方都接受
        if (CurrentOffer.Player1Accepted && CurrentOffer.Player2Accepted) {
            return CompleteTrade();
        }

        return true;
    }

    /// <summary>
    /// Completes the trade, executing the item and gold exchange.
    /// </summary>
    /// <returns>True if trade was completed successfully, false otherwise.</returns>
    private bool CompleteTrade() {
        if (CurrentOffer == null) {
            return false;
        }

        // 检查金币是否足够
        var playerGold = PlayerInventory.Instance.Gold;
        if (playerGold < CurrentOffer.Player1Gold) {
            EmitSignal(SignalName.TradeFailed, "金币不足");
            CancelTrade();
            return false;
        }

        // 执行交易
        // 玩家获得物品
        foreach (var item in CurrentOffer.Player2Items) {
            PlayerInventory.Instance.AddItem(item);
        }

        // 玩家获得金币
        if (CurrentOffer.Player2Gold > 0) {
            PlayerInventory.Instance.AddGold(CurrentOffer.Player2Gold);
        }

        // 玩家扣除物品
        foreach (var item in CurrentOffer.Player1Items) {
            PlayerInventory.Instance.RemoveItem(item);
        }

        // 玩家扣除金币
        if (CurrentOffer.Player1Gold > 0) {
            PlayerInventory.Instance.RemoveGold(CurrentOffer.Player1Gold);
        }

        // 记录交易
        var record = new TradeRecord {
            RecordId = Guid.NewGuid().ToString(),
            TradeOffer = CurrentOffer,
            CompletedAt = DateTime.Now
        };

        TradeHistory.Add(record);
        SaveTradeHistory();

        CurrentState = TradeState.Completed;
        EmitSignal(SignalName.TradeCompleted, record);
        EmitSignal(SignalName.TradeAccepted);

        // 重置状态
        CurrentState = TradeState.Idle;
        CurrentOffer = null;

        return true;
    }

    /// <summary>
    /// Cancels the current trade, resetting the trade state.
    /// </summary>
    public void CancelTrade() {
        if (CurrentState == TradeState.Idle) {
            return;
        }

        CurrentState = TradeState.Cancelled;
        CurrentOffer = null;
        CurrentState = TradeState.Idle;

        EmitSignal(SignalName.TradeCancelled);
    }

    /// <summary>
    /// Gets the trade history.
    /// </summary>
    /// <param name="count">Maximum number of records to return (default 20).</param>
    /// <returns>List of recent trade records.</returns>
    public List<TradeRecord> GetTradeHistory(int count = 20) {
        var result = new List<TradeRecord>();
        var start = Math.Max(0, TradeHistory.Count - count);
        for (int i = start; i < TradeHistory.Count; i++) {
            result.Add(TradeHistory[i]);
        }
        return result;
    }

    // 保存交易历史
    private void SaveTradeHistory() {
        var saveData = new Dictionary<string, object>();
        var records = new List<Dictionary<string, object>>();

        foreach (var record in TradeHistory) {
            records.Add(new Dictionary<string, object> {
                { "record_id", record.RecordId },
                { "completed_at", record.CompletedAt.ToString("o") }
            });
        }

        saveData["trade_history"] = records;

        var json = JSON.Stringify(saveData);
        FileAccess.WriteEncryptedString(GetSavePath(), json, SaveSystem.Instance.GetSaveKey());
    }

    // 加载交易历史
    private void LoadTradeHistory() {
        var path = GetSavePath();
        if (!FileAccess.FileExists(path)) {
            return;
        }

        try {
            var json = FileAccess.GetEncryptedStringAtPosition(path, 0, SaveSystem.Instance.GetSaveKey());
            var data = JSON.ParseString(json);
            if (data == null) return;

            var saveData = data.As<Dictionary<string, Variant>>();
            if (!saveData.ContainsKey("trade_history")) return;

            var records = saveData["trade_history"].As<Array>();
            foreach (var record in records) {
                var recordData = record.As<Dictionary<string, Variant>>();
                TradeHistory.Add(new TradeRecord {
                    RecordId = recordData["record_id"].ToString(),
                    CompletedAt = DateTime.Parse(recordData["completed_at"].ToString())
                });
            }
        } catch (Exception e) {
            GD.PrintErr("加载交易历史失败: " + e.Message);
        }
    }

    private string GetSavePath() {
        return SaveSystem.Instance.GetSaveDirectory() + "/trade_history.dat";
    }

    // ===== 持久化方法 =====

    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // 交易历史
        var records = new List<Dictionary>();
        foreach (var record in TradeHistory)
        {
            var recordDict = new Dictionary<string, object>();
            recordDict["record_id"] = record.RecordId;
            recordDict["completed_at"] = record.CompletedAt.ToString("o");
            
            // 交易详情（简化）
            if (record.TradeOffer != null)
            {
                var offerDict = new Dictionary<string, object>();
                offerDict["offer_id"] = record.TradeOffer.OfferId;
                offerDict["player1_name"] = record.TradeOffer.Player1Name;
                offerDict["player2_name"] = record.TradeOffer.Player2Name;
                offerDict["player1_gold"] = record.TradeOffer.Player1Gold;
                offerDict["player2_gold"] = record.TradeOffer.Player2Gold;
                offerDict["player1_items_count"] = record.TradeOffer.Player1Items.Count;
                offerDict["player2_items_count"] = record.TradeOffer.Player2Items.Count;
                recordDict["offer"] = offerDict;
            }
            
            records.Add(recordDict);
        }
        data["trade_history"] = records;
        
        // 当前交易状态
        data["current_state"] = (int)CurrentState;
        
        return data;
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        // 加载交易历史
        if (data.Contains("trade_history"))
        {
            TradeHistory.Clear();
            var records = (Array)data["trade_history"];
            foreach (Dictionary recordDict in records)
            {
                var record = new TradeRecord
                {
                    RecordId = recordDict["record_id"].ToString()
                };
                
                if (recordDict.Contains("completed_at"))
                {
                    DateTime.TryParse(recordDict["completed_at"].ToString(), out record.CompletedAt);
                }
                
                TradeHistory.Add(record);
            }
        }
        
        // 加载当前状态
        if (data.Contains("current_state"))
        {
            CurrentState = (TradeState)(int)data["current_state"];
        }
    }
}

// ===== Data Classes =====

/// <summary>
/// Represents a trade offer between two players.
/// </summary>
public class TradeOffer {
    /// <summary>Unique identifier for this trade offer.</summary>
    public string OfferId { get; set; } = "";
    
    /// <summary>ID of the first player in the trade.</summary>
    public string Player1Id { get; set; } = "";
    
    /// <summary>Name of the first player.</summary>
    public string Player1Name { get; set; } = "";
    
    /// <summary>ID of the second player in the trade.</summary>
    public string Player2Id { get; set; } = "";
    
    /// <summary>Name of the second player.</summary>
    public string Player2Name { get; set; } = "";
    
    /// <summary>List of items offered by player 1.</summary>
    public List<ItemData> Player1Items { get; set; } = new List<ItemData>();
    
    /// <summary>List of items offered by player 2.</summary>
    public List<ItemData> Player2Items { get; set; } = new List<ItemData>();
    
    /// <summary>Gold amount offered by player 1.</summary>
    public int Player1Gold { get; set; }
    
    /// <summary>Gold amount offered by player 2.</summary>
    public int Player2Gold { get; set; }
    
    /// <summary>Whether player 1 has accepted the current offer.</summary>
    public bool Player1Accepted { get; set; }
    
    /// <summary>Whether player 2 has accepted the current offer.</summary>
    public bool Player2Accepted { get; set; }
    
    /// <summary>Timestamp when this offer was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Calculates the total value of items and gold for a player.
    /// </summary>
    /// <param name="isPlayer1">True to calculate for player 1, false for player 2.</param>
    /// <returns>Total value in gold.</returns>
    public int GetTotalValue(bool isPlayer1) {
        var value = isPlayer1 ? Player1Gold : Player2Gold;
        var items = isPlayer1 ? Player1Items : Player2Items;
        foreach (var item in items) {
            value += ItemDatabase.GetItemValue(item.Id);
        }
        return value;
    }
}

/// <summary>
/// Represents a completed trade record for history tracking.
/// </summary>
public class TradeRecord {
    /// <summary>Unique identifier for this trade record.</summary>
    public string RecordId { get; set; } = "";
    
    /// <summary>The completed trade offer.</summary>
    public TradeOffer TradeOffer { get; set; }
    
    /// <summary>Timestamp when the trade was completed.</summary>
    public DateTime CompletedAt { get; set; } = DateTime.Now;
}
