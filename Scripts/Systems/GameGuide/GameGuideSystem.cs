using System;
using System.Collections.Generic;
using Godot;

public partial class GameGuideSystem : BaseSystem
{
    private static GameGuideData _data;
    private static GameGuideSystem _instance;
    
    protected override string SystemName => "GameGuideSystem";
    
    public static GameGuideSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameGuideSystem();
            }
            return _instance;
        }
    }
    
    public GameGuideData Data
    {
        get
        {
            if (_data == null)
            {
                _data = new GameGuideData();
            }
            return _data;
        }
        set => _data = value;
    }
    
    public void Initialize()
    {
        // 初始化默认解锁的类别
        foreach (var category in GameGuideDatabase.Categories.Values)
        {
            if (category.IsDefaultUnlocked && !Data.UnlockedCategories.ContainsKey(category.Id))
            {
                Data.UnlockedCategories[category.Id] = true;
            }
        }
    }
    
    // 解锁类别
    public bool UnlockCategory(string categoryId)
    {
        if (Data.UnlockedCategories.ContainsKey(categoryId) && Data.UnlockedCategories[categoryId])
        {
            return true; // 已经解锁
        }
        
        // 检查前置条件
        var category = GameGuideDatabase.Categories.GetValueOrDefault(categoryId);
        if (category == null) return false;
        
        if (!string.IsNullOrEmpty(category.UnlockRequirement))
        {
            // 需要先完成前置指南
            if (!Data.ReadGuides.Contains(category.UnlockRequirement))
            {
                return false;
            }
        }
        
        Data.UnlockedCategories[categoryId] = true;
        Data.CategoriesUnlocked = CountUnlockedCategories();
        return true;
    }
    
    // 检查类别是否解锁
    public bool IsCategoryUnlocked(string categoryId)
    {
        return Data.UnlockedCategories.GetValueOrDefault(categoryId, false);
    }
    
    // 读取指南
    public void ReadGuide(string guideId)
    {
        if (!Data.ReadGuides.Contains(guideId))
        {
            Data.ReadGuides.Add(guideId);
            Data.TotalGuidesRead++;
            Data.LastReadTime = DateTime.Now;
            
            // 添加到历史
            var guide = GameGuideDatabase.Guides.GetValueOrDefault(guideId);
            if (guide != null)
            {
                Data.ReadHistory.Add(new GuideReadHistory
                {
                    GuideId = guideId,
                    ReadTime = DateTime.Now,
                    ReadDuration = guide.ReadTime
                });
                
                // 保持历史记录不超过100条
                if (Data.ReadHistory.Count > 100)
                {
                    Data.ReadHistory.RemoveAt(0);
                }
                
                // 尝试解锁该类别
                UnlockCategory(guide.Category);
            }
        }
    }
    
    // 检查指南是否已读
    public bool IsGuideRead(string guideId)
    {
        return Data.ReadGuides.Contains(guideId);
    }
    
    // 获取类别下的指南
    public List<GuideConfig> GetGuidesByCategory(string categoryId)
    {
        var result = new List<GuideConfig>();
        foreach (var guide in GameGuideDatabase.Guides.Values)
        {
            if (guide.Category == categoryId && IsCategoryUnlocked(categoryId))
            {
                result.Add(guide);
            }
        }
        result.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        return result;
    }
    
    // 获取所有解锁的类别
    public List<GuideCategory> GetUnlockedCategories()
    {
        var result = new List<GuideCategory>();
        foreach (var category in GameGuideDatabase.Categories.Values)
        {
            if (IsCategoryUnlocked(category.Id))
            {
                result.Add(category);
            }
        }
        result.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
        return result;
    }
    
    // 获取推荐指南（未读的优先级高的指南）
    public List<GuideConfig> GetRecommendedGuides(int count = 5)
    {
        var result = new List<GuideConfig>();
        var unlockedCategories = GetUnlockedCategories();
        
        foreach (var category in unlockedCategories)
        {
            foreach (var guide in GameGuideDatabase.Guides.Values)
            {
                if (guide.Category == category.Id && !IsGuideRead(guide.Id))
                {
                    result.Add(guide);
                }
            }
        }
        
        result.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        
        if (result.Count > count)
        {
            return result.GetRange(0, count);
        }
        return result;
    }
    
    // 统计
    public int CountUnlockedCategories()
    {
        int count = 0;
        foreach (var unlocked in Data.UnlockedCategories.Values)
        {
            if (unlocked) count++;
        }
        return count;
    }
    
    public int GetTotalGuidesCount()
    {
        return GameGuideDatabase.Guides.Count;
    }
    
    public int GetReadGuidesCount()
    {
        return Data.ReadGuides.Count;
    }
    
    // 获取指南详情
    public GuideConfig GetGuide(string guideId)
    {
        return GameGuideDatabase.Guides.GetValueOrDefault(guideId);
    }
    
    // 获取类别详情
    public GuideCategory GetCategory(string categoryId)
    {
        return GameGuideDatabase.Categories.GetValueOrDefault(categoryId);
    }
    
    /// <summary>
    /// Export save data (BaseSystem override)
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var saveData = new Dictionary();
        saveData["unlocked_categories"] = Data.UnlockedCategories;
        saveData["read_guides"] = new List<string>(Data.ReadGuides);
        saveData["completed_tutorials"] = new List<string>(Data.CompletedTutorials);
        saveData["read_history"] = Data.ReadHistory;
        saveData["total_guides_read"] = Data.TotalGuidesRead;
        saveData["total_tutorials_completed"] = Data.TotalTutorialsCompleted;
        saveData["categories_unlocked"] = Data.CategoriesUnlocked;
        return saveData;
    }
    
    /// <summary>
    /// Import save data (BaseSystem override)
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("unlocked_categories"))
        {
            Data.UnlockedCategories = (Dictionary<string, bool>)data["unlocked_categories"];
        }
        
        if (data.Contains("read_guides"))
        {
            Data.ReadGuides = new HashSet<string>((List<string>)data["read_guides"]);
        }
        
        if (data.Contains("completed_tutorials"))
        {
            Data.CompletedTutorials = new HashSet<string>((List<string>)data["completed_tutorials"]);
        }
        
        if (data.Contains("read_history"))
        {
            Data.ReadHistory = (List<GuideReadHistory>)data["read_history"];
        }
        
        if (data.Contains("total_guides_read"))
        {
            Data.TotalGuidesRead = (int)data["total_guides_read"];
        }
        
        if (data.Contains("total_tutorials_completed"))
        {
            Data.TotalTutorialsCompleted = (int)data["total_tutorials_completed"];
        }
        
        if (data.Contains("categories_unlocked"))
        {
            Data.CategoriesUnlocked = (int)data["categories_unlocked"];
        }
        
        Initialize();
    }
}
