using System;
using System.Collections.Generic;

[Serializable]
public class GameGuideData
{
    // 玩家已解锁的指南类别
    public Dictionary<string, bool> UnlockedCategories = new Dictionary<string, bool>();
    
    // 玩家已阅读的指南项
    public HashSet<string> ReadGuides = new HashSet<string>();
    
    // 玩家完成的教程步骤
    public HashSet<string> CompletedTutorials = new HashSet<string>();
    
    // 指南阅读历史
    public List<GuideReadHistory> ReadHistory = new List<GuideReadHistory>();
    
    // 统计数据
    public int TotalGuidesRead { get; set; }
    public int TotalTutorialsCompleted { get; set; }
    public DateTime LastReadTime { get; set; }
    public int CategoriesUnlocked { get; set; }
}

[Serializable]
public class GuideReadHistory
{
    public string GuideId { get; set; }
    public DateTime ReadTime { get; set; }
    public int ReadDuration { get; set; } // seconds
}

[Serializable]
public class GuideProgress
{
    public string GuideId { get; set; }
    public string Category { get; set; }
    public bool IsUnlocked { get; set; }
    public bool IsRead { get; set; }
    public int Priority { get; set; }
}
