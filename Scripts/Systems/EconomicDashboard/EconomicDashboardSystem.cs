using Godot;
using System;
using System.Collections.Generic;

public class EconomicDashboardData
{
    // 金币统计
    public long TotalGoldInCirculation { get; set; }      // 流通金币总量
    public long TotalGoldEarned { get; set; }             // 总获得金币
    public long TotalGoldSpent { get; set; }             // 总支出金币
    public long NetGoldChange { get; set; }               // 净变化
    
    // 收入来源分类
    public long CombatEarnings { get; set; }              // 战斗收入
    public long QuestRewards { get; set; }                 // 任务奖励
    public long CraftingEarnings { get; set; }            // 制作收入
    public long TradingEarnings { get; set; }             // 交易收入
    public long EventRewards { get; set; }                // 活动奖励
    public long OtherEarnings { get; set; }               // 其他收入
    
    // 支出分类
    public long PurchaseExpenses { get; set; }             // 购买支出
    public long RepairExpenses { get; set; }               // 修理支出
    public long UpgradeExpenses { get; set; }              // 升级支出
    public long CraftingCosts { get; set; }                // 制作费用
    public long AuctionFees { get; set; }                 // 拍卖手续费
    public long OtherExpenses { get; set; }                // 其他支出
    
    // 物品交易统计
    public int ItemsSold { get; set; }                    // 售出物品数
    public int ItemsPurchased { get; set; }               // 购买物品数
    public int ItemsCrafted { get; set; }                 // 制作物品数
    
    // 经济健康度
    public float EconomicHealth { get; set; }             // 经济健康度 (0-100)
    public float InflationRate { get; set; }              // 通胀率
    public float GoldPerMinute { get; set; }              // 每分钟金币流动
    
    // 时间戳
    public long LastUpdateTime { get; set; }
    
    public EconomicDashboardData()
    {
        TotalGoldInCirculation = 0;
        TotalGoldEarned = 0;
        TotalGoldSpent = 0;
        NetGoldChange = 0;
        CombatEarnings = 0;
        QuestRewards = 0;
        CraftingEarnings = 0;
        TradingEarnings = 0;
        EventRewards = 0;
        OtherEarnings = 0;
        PurchaseExpenses = 0;
        RepairExpenses = 0;
        UpgradeExpenses = 0;
        CraftingCosts = 0;
        AuctionFees = 0;
        OtherExpenses = 0;
        ItemsSold = 0;
        ItemsPurchased = 0;
        ItemsCrafted = 0;
        EconomicHealth = 100f;
        InflationRate = 0f;
        GoldPerMinute = 0f;
        LastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}

// 交易记录项
public class TransactionRecord
{
    public string Type { get; set; }                      // 交易类型
    public long Amount { get; set; }                      // 金币数量
    public string Description { get; set; }               // 描述
    public long Timestamp { get; set; }                    // 时间戳
    
    public TransactionRecord(string type, long amount, string description)
    {
        Type = type;
        Amount = amount;
        Description = description;
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}

public class EconomicDashboardSystem : BaseSystem
{
    private EconomicDashboardData data = new EconomicDashboardData();
    private List<TransactionRecord> recentTransactions = new List<TransactionRecord>();
    private int maxTransactionHistory = 100;
    
    // 事件
    public delegate void EconomicEventHandler(EconomicDashboardData data);
    public event EconomicEventHandler OnEconomicUpdate;
    
    public override void _Ready()
    {
        LoadData();
        UpdateEconomicHealth();
    }
    
    // 记录收入
    public void RecordEarning(string category, long amount)
    {
        if (amount <= 0) return;
        
        data.TotalGoldEarned += amount;
        data.NetGoldChange += amount;
        
        switch (category.ToLower())
        {
            case "combat":
                data.CombatEarnings += amount;
                break;
            case "quest":
                data.QuestRewards += amount;
                break;
            case "crafting":
                data.CraftingEarnings += amount;
                break;
            case "trading":
                data.TradingEarnings += amount;
                break;
            case "event":
                data.EventRewards += amount;
                break;
            default:
                data.OtherEarnings += amount;
                break;
        }
        
        AddTransaction("收入", amount, category);
        UpdateEconomicHealth();
        NotifyUpdate();
    }
    
    // 记录支出
    public void RecordExpense(string category, long amount)
    {
        if (amount <= 0) return;
        
        data.TotalGoldSpent += amount;
        data.NetGoldChange -= amount;
        
        switch (category.ToLower())
        {
            case "purchase":
                data.PurchaseExpenses += amount;
                break;
            case "repair":
                data.RepairExpenses += amount;
                break;
            case "upgrade":
                data.UpgradeExpenses += amount;
                break;
            case "crafting":
                data.CraftingCosts += amount;
                break;
            case "auction":
                data.AuctionFees += amount;
                break;
            default:
                data.OtherExpenses += amount;
                break;
        }
        
        AddTransaction("支出", -amount, category);
        UpdateEconomicHealth();
        NotifyUpdate();
    }
    
    // 记录物品交易
    public void RecordItemSold()
    {
        data.ItemsSold++;
    }
    
    public void RecordItemPurchased()
    {
        data.ItemsPurchased++;
    }
    
    public void RecordItemCrafted()
    {
        data.ItemsCrafted++;
    }
    
    // 更新流通金币总量
    public void UpdateTotalGoldInCirculation(long total)
    {
        data.TotalGoldInCirculation = total;
        NotifyUpdate();
    }
    
    // 添加交易记录
    private void AddTransaction(string type, long amount, string category)
    {
        var record = new TransactionRecord(type, amount, category);
        recentTransactions.Insert(0, record);
        
        if (recentTransactions.Count > maxTransactionHistory)
        {
            recentTransactions.RemoveAt(recentTransactions.Count - 1);
        }
    }
    
    // 更新经济健康度
    private void UpdateEconomicHealth()
    {
        // 计算收入/支出比率
        float incomeExpenseRatio = 1.0f;
        if (data.TotalGoldSpent > 0)
        {
            incomeExpenseRatio = (float)data.TotalGoldEarned / data.TotalGoldSpent;
        }
        
        // 理想比率是1.0左右（收入≈支出）
        // 比率越高说明收入越多，经济越活跃
        // 比率越低说明支出大于收入，可能有通胀风险
        float healthScore = Mathf.Clamp(incomeExpenseRatio * 50f, 0f, 100f);
        
        // 考虑净变化
        if (data.NetGoldChange > 0)
        {
            // 净收入为正，略微提高健康度
            healthScore = Mathf.Min(healthScore + 5f, 100f);
        }
        else if (data.NetGoldChange < 0)
        {
            // 净支出为负，略微降低健康度
            healthScore = Mathf.Max(healthScore - 5f, 0f);
        }
        
        data.EconomicHealth = healthScore;
        
        // 估算通胀率（基于净变化）
        if (data.TotalGoldInCirculation > 0)
        {
            data.InflationRate = (float)data.NetGoldChange / data.TotalGoldInCirculation * 100f;
        }
        
        data.LastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
    
    // 计算每分钟金币流动
    public void CalculateGoldPerMinute(long sessionDurationMinutes)
    {
        if (sessionDurationMinutes > 0)
        {
            data.GoldPerMinute = (data.TotalGoldEarned + data.TotalGoldSpent) / (float)sessionDurationMinutes;
        }
    }
    
    // 获取数据
    public EconomicDashboardData GetData()
    {
        return data;
    }
    
    // 获取交易历史
    public List<TransactionRecord> GetRecentTransactions(int count = 20)
    {
        int actualCount = Mathf.Min(count, recentTransactions.Count);
        return recentTransactions.GetRange(0, actualCount);
    }
    
    // 获取收入来源分布（百分比）
    public Dictionary<string, float> GetEarningDistribution()
    {
        var distribution = new Dictionary<string, float>();
        
        if (data.TotalGoldEarned <= 0) return distribution;
        
        distribution["战斗"] = (float)data.CombatEarnings / data.TotalGoldEarned * 100f;
        distribution["任务"] = (float)data.QuestRewards / data.TotalGoldEarned * 100f;
        distribution["制作"] = (float)data.CraftingEarnings / data.TotalGoldEarned * 100f;
        distribution["交易"] = (float)data.TradingEarnings / data.TotalGoldEarned * 100f;
        distribution["活动"] = (float)data.EventRewards / data.TotalGoldEarned * 100f;
        distribution["其他"] = (float)data.OtherEarnings / data.TotalGoldEarned * 100f;
        
        return distribution;
    }
    
    // 获取支出分布（百分比）
    public Dictionary<string, float> GetExpenseDistribution()
    {
        var distribution = new Dictionary<string, float>();
        
        if (data.TotalGoldSpent <= 0) return distribution;
        
        distribution["购买"] = (float)data.PurchaseExpenses / data.TotalGoldSpent * 100f;
        distribution["修理"] = (float)data.RepairExpenses / data.TotalGoldSpent * 100f;
        distribution["升级"] = (float)data.UpgradeExpenses / data.TotalGoldSpent * 100f;
        distribution["制作"] = (float)data.CraftingCosts / data.TotalGoldSpent * 100f;
        distribution["拍卖"] = (float)data.AuctionFees / data.TotalGoldSpent * 100f;
        distribution["其他"] = (float)data.OtherExpenses / data.TotalGoldSpent * 100f;
        
        return distribution;
    }
    
    // 获取经济状态描述
    public string GetEconomicStatus()
    {
        if (data.EconomicHealth >= 80f)
            return "经济繁荣";
        else if (data.EconomicHealth >= 60f)
            return "经济健康";
        else if (data.EconomicHealth >= 40f)
            return "经济平稳";
        else if (data.EconomicHealth >= 20f)
            return "经济衰退";
        else
            return "经济危机";
    }
    
    // 通知更新
    private void NotifyUpdate()
    {
        OnEconomicUpdate?.Invoke(data);
    }
    
    // 保存数据
    public void SaveData()
    {
        // 保存到存档系统
        if (GetTree().CurrentScene is Main main)
        {
            var saveData = new Dictionary<string, object>
            {
                ["total_gold_earned"] = data.TotalGoldEarned,
                ["total_gold_spent"] = data.TotalGoldSpent,
                ["combat_earnings"] = data.CombatEarnings,
                ["quest_rewards"] = data.QuestRewards,
                ["crafting_earnings"] = data.CraftingEarnings,
                ["trading_earnings"] = data.TradingEarnings,
                ["event_rewards"] = data.EventRewards,
                ["purchase_expenses"] = data.PurchaseExpenses,
                ["repair_expenses"] = data.RepairExpenses,
                ["upgrade_expenses"] = data.UpgradeExpenses,
                ["crafting_costs"] = data.CraftingCosts,
                ["auction_fees"] = data.AuctionFees,
                ["items_sold"] = data.ItemsSold,
                ["items_purchased"] = data.ItemsPurchased,
                ["items_crafted"] = data.ItemsCrafted
            };
            
            main.SaveSystem.SaveData("economic_dashboard", saveData);
        }
    }
    
    // 加载数据
    public void LoadData()
    {
        if (GetTree().CurrentScene is Main main)
        {
            var saveData = main.SaveSystem.LoadData("economic_dashboard");
            if (saveData != null)
            {
                if (saveData.ContainsKey("total_gold_earned"))
                    data.TotalGoldEarned = Convert.ToInt64(saveData["total_gold_earned"]);
                if (saveData.ContainsKey("total_gold_spent"))
                    data.TotalGoldSpent = Convert.ToInt64(saveData["total_gold_spent"]);
                if (saveData.ContainsKey("combat_earnings"))
                    data.CombatEarnings = Convert.ToInt64(saveData["combat_earnings"]);
                if (saveData.ContainsKey("quest_rewards"))
                    data.QuestRewards = Convert.ToInt64(saveData["quest_rewards"]);
                if (saveData.ContainsKey("crafting_earnings"))
                    data.CraftingEarnings = Convert.ToInt64(saveData["crafting_earnings"]);
                if (saveData.ContainsKey("trading_earnings"))
                    data.TradingEarnings = Convert.ToInt64(saveData["trading_earnings"]);
                if (saveData.ContainsKey("event_rewards"))
                    data.EventRewards = Convert.ToInt64(saveData["event_rewards"]);
                if (saveData.ContainsKey("purchase_expenses"))
                    data.PurchaseExpenses = Convert.ToInt64(saveData["purchase_expenses"]);
                if (saveData.ContainsKey("repair_expenses"))
                    data.RepairExpenses = Convert.ToInt64(saveData["repair_expenses"]);
                if (saveData.ContainsKey("upgrade_expenses"))
                    data.UpgradeExpenses = Convert.ToInt64(saveData["upgrade_expenses"]);
                if (saveData.ContainsKey("crafting_costs"))
                    data.CraftingCosts = Convert.ToInt64(saveData["crafting_costs"]);
                if (saveData.ContainsKey("auction_fees"))
                    data.AuctionFees = Convert.ToInt64(saveData["auction_fees"]);
                if (saveData.ContainsKey("items_sold"))
                    data.ItemsSold = Convert.ToInt32(saveData["items_sold"]);
                if (saveData.ContainsKey("items_purchased"))
                    data.ItemsPurchased = Convert.ToInt32(saveData["items_purchased"]);
                if (saveData.ContainsKey("items_crafted"))
                    data.ItemsCrafted = Convert.ToInt32(saveData["items_crafted"]);
                
                data.NetGoldChange = data.TotalGoldEarned - data.TotalGoldSpent;
                UpdateEconomicHealth();
            }
        }
    }
}
