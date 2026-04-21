using Godot;
using System;
using System.Collections.Generic;
using SaveSystem = ClawRPG.Scripts.Framework.SaveSystem;

public partial class ComboChainData : BaseSystem
{
    // 当前连击状态
    public int CurrentChain { get; set; } = 0;
    public int MaxChain { get; set; } = 0;
    public float ChainTimer { get; set; } = 0f;
    public bool IsChainActive { get; set; } = false;
    
    // 连击历史记录
    public List<ChainRecord> ChainHistory { get; set; } = new List<ChainRecord>();
    
    // 统计追踪
    public int TotalChains { get; set; } = 0;
    public int TotalChainHits { get; set; } = 0;
    public int MaxChainEver { get; set; } = 0;
    public int Chain10Count { get; set; } = 0;
    public int Chain25Count { get; set; } = 0;
    public int Chain50Count { get; set; } = 0;
    public int Chain100Count { get; set; } = 0;
    public float TotalChainDamage { get; set; } = 0f;
    public float ChainDamageBonus { get; set; } = 0f;
    
    // 连击记录结构
    public class ChainRecord
    {
        public int ChainLevel { get; set; }
        public float Damage { get; set; }
        public float BonusDamage { get; set; }
        public int ComboType { get; set; }
        public long Timestamp { get; set; }
    }
    
    public override void _Ready()
    {
        SaveSystem.Instance.RegisterSaveData(this);
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData() {
        var data = new Dictionary<string, object>();
        data["currentChain"] = CurrentChain;
        data["maxChain"] = MaxChain;
        data["isChainActive"] = IsChainActive;
        
        // 保存历史记录
        var historyList = new List<Dictionary>();
        foreach (var record in ChainHistory)
        {
            var recordDict = new Dictionary<string, object>();
            recordDict["chainLevel"] = record.ChainLevel;
            recordDict["damage"] = record.Damage;
            recordDict["bonusDamage"] = record.BonusDamage;
            recordDict["comboType"] = record.ComboType;
            recordDict["timestamp"] = record.Timestamp;
            historyList.Add(recordDict);
        }
        data["chainHistory"] = historyList;
        
        // 保存统计
        data["totalChains"] = TotalChains;
        data["totalChainHits"] = TotalChainHits;
        data["maxChainEver"] = MaxChainEver;
        data["chain10Count"] = Chain10Count;
        data["chain25Count"] = Chain25Count;
        data["chain50Count"] = Chain50Count;
        data["chain100Count"] = Chain100Count;
        data["totalChainDamage"] = TotalChainDamage;
        data["chainDamageBonus"] = ChainDamageBonus;
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.Contains("currentChain")) CurrentChain = (int)data["currentChain"];
        if (data.Contains("maxChain")) MaxChain = (int)data["maxChain"];
        if (data.Contains("isChainActive")) IsChainActive = (bool)data["isChainActive"];
        
        // 加载历史记录
        ChainHistory.Clear();
        if (data.Contains("chainHistory"))
        {
            var historyList = (Godot.Array)data["chainHistory"];
            foreach (Dictionary recordDict in historyList)
            {
                var record = new ChainRecord();
                record.ChainLevel = (int)recordDict["chainLevel"];
                record.Damage = (float)recordDict["damage"];
                record.BonusDamage = (float)recordDict["bonusDamage"];
                record.ComboType = (int)recordDict["comboType"];
                record.Timestamp = (long)recordDict["timestamp"];
                ChainHistory.Add(record);
            }
        }
        
        // 加载统计
        if (data.Contains("totalChains")) TotalChains = (int)data["totalChains"];
        if (data.Contains("totalChainHits")) TotalChainHits = (int)data["totalChainHits"];
        if (data.Contains("maxChainEver")) MaxChainEver = (int)data["maxChainEver"];
        if (data.Contains("chain10Count")) Chain10Count = (int)data["chain10Count"];
        if (data.Contains("chain25Count")) Chain25Count = (int)data["chain25Count"];
        if (data.Contains("chain50Count")) Chain50Count = (int)data["chain50Count"];
        if (data.Contains("chain100Count")) Chain100Count = (int)data["chain100Count"];
        if (data.Contains("totalChainDamage")) TotalChainDamage = (float)data["totalChainDamage"];
        if (data.Contains("chainDamageBonus")) ChainDamageBonus = (float)data["chainDamageBonus"];
    }
}
