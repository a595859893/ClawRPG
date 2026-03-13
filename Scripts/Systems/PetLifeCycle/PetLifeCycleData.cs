using Godot;
using System.Collections.Generic;

public class PetLifeCycleData : Node
{
    // 宠物生命周期数据
    public Dictionary<int, PetLifeCycleEntry> PetLifeCycles = new Dictionary<int, PetLifeCycleEntry>();
    
    // 统计数据
    public int TotalLifeCycles = 0;
    public int TotalDeaths = 0;
    public int TotalLifeExtensions = 0;
    public int LongestLifeSpan = 0; // 最长生命周期(游戏天数)
    
    // 历史记录
    public List<LifeCycleHistoryEntry> History = new List<LifeCycleHistoryEntry>();
}

public class PetLifeCycleEntry
{
    public int PetId;
    public string PetName;
    public int CurrentAge = 0; // 当前年龄(天)
    public int MaxAge = 100; // 最大年龄(天)
    public LifeStage CurrentStage = LifeStage.Adult;
    public bool IsImmortal = false;
    public int LifeExtensionUsed = 0;
    public int DaysSinceLastStageChange = 0;
}

public enum LifeStage
{
    Baby,      // 婴儿期 (0-10天)
    Young,    // 幼年期 (11-30天)
    Adult,    // 成年期 (31-70天)
    Senior,   // 老年期 (71-90天)
    Final,    // 临终期 (91-100天)
    Immortal  // 不朽 (使用生命延续后)
}

public class LifeCycleHistoryEntry
{
    public int PetId;
    public string PetName;
    public int AgeAtDeath;
    public LifeStage StageAtDeath;
    public bool WasExtended;
    public int LifeExtensions;
    public long Timestamp;
}
