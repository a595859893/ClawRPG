using System;
using System.Collections.Generic;
using Godot;

public enum MythicPlusDifficulty
{
    Mythic0 = 0,
    Mythic2 = 2,
    Mythic5 = 5,
    Mythic10 = 10,
    Mythic15 = 15,
    Mythic20 = 20,
    MythicPlus = 100  // Unlimited
}

public enum MythicAffix
{
    Teeming,        // 涌流 - 更多敌人
    Volcanic,       // 火山 - 地面AOE
    Necrotic,       // 坏死 - 持续伤害光环
    Afflicted,      // 折磨 - 召唤虚弱敌人
    SpittingImage,  // 分身 - 敌人镜像
    GrievingWound,  // 重伤 - 治疗减少
    Explosive,      // 爆炸 - 击杀炸弹
    Quaking,        // 地震 - 周围AOE
    Sanguine,       // 血池 - 击杀生成血池
    Bolstering,     // 加强 - 击杀增强敌人
    Raging,         // 狂暴 - 血量低时狂暴
    Tyrannical,     // 暴君 - Boss增强
    Fortified,      // 强化 - 小怪增强
    Bursting,       // 爆发 - 死亡爆炸
    Infested,       // 感染 - 小怪召唤
    Skittish,       // 惊慌 - 仇恨不稳定
    Inspiring,      // 激励 - 附近敌人增益
    Prideful,       // 骄傲 - 击杀召唤护卫
    Storming,       // 风暴 - 视野减少
    Entangling,     // 缠绕 - 减速
    Discording,     // 混乱 - 伤害反射
    Molten,         // 熔火 - 周期性AOE
    Shrouding,      // 笼罩 - 隐身
    Elitist,        // 精英 - 增强敌人
    Reaping,        // 收割 - 死亡鬼魂
    Explosive,      // 爆炸 - 炸弹
    Incorporeal,    // 虚化 - 敌人虚化
    Awakened,       // 觉醒 - 召唤Boss
    Dueling,        // 决斗 - 敌人联手
    Felburst,       // 邪能爆发
    Grenade,        // 手雷
    Thundering,     // 雷鸣
    FelReaver,      // 邪能破坏者
    Reflective,     // 反射
    Empowered,      // 强化
    Shielding,      // 护盾
    Unyielding,     // 不屈
    Swelling,       // 肿胀
    Telegating,     // 传送
    Staggering,     // 蹒跚
    Frustrated,     // 挫折
    Augmented,      // 增强
    Arcane,         // 奥术
    Frost,          // 冰霜
    Fel,            // 邪能
    Shadow,         // 暗影
    Holy,           // 神圣
    Nature,         // 自然
    Chaos,          // 混沌
}

public class MythicPlusRun
{
    public int RunId { get; set; }
    public int DungeonLevel { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool Completed { get; set; }
    public bool Failed { get; set; }
    public int EnemiesKilled { get; set; }
    public int BossesDefeated { get; set; }
    public int Deaths { get; set; }
    public int TimeBonus { get; set; }
    public int AffixBonus { get; set; }
    public int CompletionTimeSeconds { get; set; }
    public MythicPlusDifficulty Difficulty { get; set; }
    public List<MythicAffix> ActiveAffixes { get; set; }
    public int Score { get; set; }
    public int RewardGold { get; set; }
    public int RewardExp { get; set; }
    public List<string> RewardItems { get; set; }
    
    public MythicPlusRun()
    {
        ActiveAffixes = new List<MythicAffix>();
        RewardItems = new List<string>();
    }
    
    public int CalculateScore()
    {
        if (!Completed || Failed) return 0;
        
        int baseScore = DungeonLevel * 100;
        int timeScore = Math.Max(0, 3000 - CompletionTimeSeconds) / 10;
        int affixScore = ActiveAffixes.Count * (DungeonLevel * 5);
        int killScore = EnemiesKilled / 10;
        int deathPenalty = Deaths * 50;
        
        return Math.Max(0, baseScore + timeScore + affixScore + killScore - deathPenalty);
    }
}

public class MythicPlusAffixGroup
{
    public int Season { get; set; }
    public int WeekNumber { get; set; }
    public List<MythicAffix> Affixes { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    public MythicPlusAffixGroup()
    {
        Affixes = new List<MythicAffix>();
    }
}

public class MythicPlusDungeonConfig
{
    public string DungeonId { get; set; }
    public string Name { get; set; }
    public int BaseLevel { get; set; }
    public int RecommendedItemLevel { get; set; }
    public int MinItemLevel { get; set; }
    public int BossCount { get; set; }
    public int EnemyCount { get; set; }
    public int EstimatedTimeMinutes { get; set; }
    public string Biome { get; set; }
    public List<string> EnemyTypes { get; set; }
    public Dictionary<string, int> Rewards { get; set; }
    
    public MythicPlusDungeonConfig()
    {
        EnemyTypes = new List<string>();
        Rewards = new Dictionary<string, int>();
    }
}

public class MythicPlusProgress
{
    public int PlayerId { get; set; }
    public int BestLevel { get; set; }
    public int TotalRuns { get; set; }
    public int CompletedRuns { get; set; }
    public int FailedRuns { get; set; }
    public int HighestScore { get; set; }
    public int TotalTimePlayed { get; set; }
    public int TotalEnemiesKilled { get; set; }
    public int TotalDeaths { get; set; }
    public Dictionary<int, int> LevelCompletionCount { get; set; }
    public Dictionary<int, int> LevelBestTime { get; set; }
    public List<int> WeeklyBestLevels { get; set; }
    public DateTime LastRunTime { get; set; }
    public int ConsecutiveCompletions { get; set; }
    
    public MythicPlusProgress()
    {
        LevelCompletionCount = new Dictionary<int, int>();
        LevelBestTime = new Dictionary<int, int>();
        WeeklyBestLevels = new List<int>();
    }
}

public class MythicPlusReward
{
    public int Level { get; set; }
    public int Gold { get; set; }
    public int Experience { get; set; }
    public List<string> Items { get; set; }
    public int ScoreBonus { get; set; }
    public string Title { get; set; }
    public List<string> Unlocks { get; set; }
    
    public MythicPlusReward()
    {
        Items = new List<string>();
        Unlocks = new List<string>();
    }
}

public class MythicPlusLeaderboard
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; }
    public int Level { get; set; }
    public int Score { get; set; }
    public int TimeSeconds { get; set; }
    public DateTime CompletionTime { get; set; }
    public bool IsWeekly { get; set; }
    
    public MythicPlusLeaderboard()
    {
        PlayerName = "";
    }
}
