using Godot;
using System;
using System.Collections.Generic;

public partial class TradeSystem : Node {
    public static TradeSystem Instance { get; private set; }

    // 交易状态
    public enum TradeState {
        Idle,
        Offering,
        Trading,
        Completed,
        Cancelled
    }

    // 当前交易状态
    public TradeState CurrentState { get; private set; } = TradeState.Idle;

    // 玩家交易提议
    public TradeOffer CurrentOffer { get; private set; }

    // 交易历史
    public List<TradeRecord> TradeHistory { get; private set; } = new List<TradeRecord>();

    // 信号
    [Signal] public delegate void TradeStartedEventHandler();
    [Signal] public delegate void OfferUpdatedEventHandler(TradeOffer offer);
    [Signal] public delegate void TradeAcceptedEventHandler();
    [Signal] public delegate void TradeCompletedEventHandler(TradeRecord record);
    [Signal] public delegate void TradeCancelledEventHandler();
    [Signal] public delegate void TradeFailedEventHandler(string reason);

    public override void _Ready() {
        Instance = this;
        LoadTradeHistory();
    }

    // 开始交易
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

    // 添加物品到交易
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

    // 移除物品从交易
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

    // 设置交易金币
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

    // 接受交易提议
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

    // 完成交易
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

    // 取消交易
    public void CancelTrade() {
        if (CurrentState == TradeState.Idle) {
            return;
        }

        CurrentState = TradeState.Cancelled;
        CurrentOffer = null;
        CurrentState = TradeState.Idle;

        EmitSignal(SignalName.TradeCancelled);
    }

    // 获取交易历史
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
}

// 交易提议
public class TradeOffer {
    public string OfferId { get; set; } = "";
    public string Player1Id { get; set; } = "";
    public string Player1Name { get; set; } = "";
    public string Player2Id { get; set; } = "";
    public string Player2Name { get; set; } = "";
    public List<ItemData> Player1Items { get; set; } = new List<ItemData>();
    public List<ItemData> Player2Items { get; set; } = new List<ItemData>();
    public int Player1Gold { get; set; }
    public int Player2Gold { get; set; }
    public bool Player1Accepted { get; set; }
    public bool Player2Accepted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int GetTotalValue(bool isPlayer1) {
        var value = isPlayer1 ? Player1Gold : Player2Gold;
        var items = isPlayer1 ? Player1Items : Player2Items;
        foreach (var item in items) {
            value += ItemDatabase.GetItemValue(item.Id);
        }
        return value;
    }
}

// 交易记录
public class TradeRecord {
    public string RecordId { get; set; } = "";
    public TradeOffer TradeOffer { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.Now;
}
