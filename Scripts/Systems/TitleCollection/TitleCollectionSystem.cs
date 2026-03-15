using Godot;
using System;
using System.Collections.Generic;

public class TitleCollectionSystem : BaseSystem
{
    private TitleCollectionData _data = new TitleCollectionData();
    
    // 单例实例
    private static TitleCollectionSystem _instance;
    public static TitleCollectionSystem Instance
    {
        get => _instance;
    }
    
    public override void _Ready()
    {
        _instance = this;
        LoadData();
    }
    
    // 收集标题
    public bool CollectTitle(string titleId)
    {
        var titles = _data.CollectedTitles;
        if (titles.Contains(titleId))
        {
            return false; // 已经收集过
        }
        
        var titleConfig = TitleCollectionDatabase.GetTitleById(titleId);
        if (titleConfig == null)
        {
            return false;
        }
        
        // 添加到已收集
        titles[titleId] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        _data.CollectedTitles = titles;
        
        // 添加到历史
        var history = _data.TitleHistory;
        history.Add(new Godot.Collections.Dictionary {
            { "title_id", titleId },
            { "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        });
        _data.TitleHistory = history;
        
        // 更新统计
        _data.TotalTitlesCollected++;
        
        string rarity = (string)titleConfig["rarity"];
        if (rarity == "Legendary") _data.LegendaryTitles++;
        else if (rarity == "Epic") _data.EpicTitles++;
        else if (rarity == "Rare") _data.RareTitles++;
        
        SaveData();
        return true;
    }
    
    // 检查是否已收集
    public bool HasTitle(string titleId)
    {
        return _data.CollectedTitles.Contains(titleId);
    }
    
    // 获取所有已收集的标题ID
    public Godot.Collections.Array GetCollectedTitleIds()
    {
        Godot.Collections.Array result = new Godot.Collections.Array();
        var titles = _data.CollectedTitles;
        foreach (string key in titles.Keys)
        {
            result.Add(key);
        }
        return result;
    }
    
    // 设置显示的标题
    public void SetDisplayTitle(string titleId)
    {
        if (titleId == "" || HasTitle(titleId))
        {
            _data.CurrentDisplayTitle = titleId;
            SaveData();
        }
    }
    
    // 获取当前显示的标题
    public string GetDisplayTitle()
    {
        return _data.CurrentDisplayTitle;
    }
    
    // 获取显示标题的配置
    public Dictionary GetDisplayTitleConfig()
    {
        string currentTitle = _data.CurrentDisplayTitle;
        if (currentTitle == "")
        {
            return null;
        }
        return TitleCollectionDatabase.GetTitleById(currentTitle);
    }
    
    // 获取收集进度
    public float GetCollectionProgress()
    {
        int total = TitleCollectionDatabase.GetAllTitles().Count;
        int collected = _data.CollectedTitles.Count;
        return total > 0 ? (float)collected / total : 0;
    }
    
    // 获取统计信息
    public Dictionary GetStatistics()
    {
        return new Godot.Collections.Dictionary
        {
            { "total_collected", _data.TotalTitlesCollected },
            { "total_available", TitleCollectionDatabase.GetAllTitles().Count },
            { "legendary", _data.LegendaryTitles },
            { "epic", _data.EpicTitles },
            { "rare", _data.RareTitles },
            { "progress", GetCollectionProgress() }
        };
    }
    
    // 获取标题历史
    public Godot.Collections.Array GetTitleHistory()
    {
        return _data.TitleHistory;
    }
    
    // 根据类别获取收集的标题
    public Godot.Collections.Array GetCollectedByCategory(string category)
    {
        Godot.Collections.Array result = new Godot.Collections.Array();
        var collected = _data.CollectedTitles;
        
        foreach (string titleId in collected.Keys)
        {
            var config = TitleCollectionDatabase.GetTitleById(titleId);
            if (config != null && (string)config["category"] == category)
            {
                result.Add(titleId);
            }
        }
        return result;
    }
    
    // 根据稀有度获取收集的标题
    public Godot.Collections.Array GetCollectedByRarity(string rarity)
    {
        Godot.Collections.Array result = new Godot.Collections.Array();
        var collected = _data.CollectedTitles;
        
        foreach (string titleId in collected.Keys)
        {
            var config = TitleCollectionDatabase.GetTitleById(titleId);
            if (config != null && (string)config["rarity"] == rarity)
            {
                result.Add(titleId);
            }
        }
        return result;
    }
    
    // 保存数据
    public void SaveData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem != null)
        {
            var data = saveSystem.LoadGame();
            data["title_collection"] = _data.ToDict();
            saveSystem.SaveGame(data);
        }
    }
    
    // 加载数据
    private void LoadData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem != null)
        {
            var data = saveSystem.LoadGame();
            if (data.Contains("title_collection"))
            {
                _data.FromDict((Godot.Collections.Dictionary)data["title_collection"]);
            }
        }
    }
}
