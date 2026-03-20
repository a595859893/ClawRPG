using System;

namespace ClawRPG.Scripts.Data
{
    /// <summary>
    /// 成就类型
    /// </summary>
    public enum AchievementType
    {
        Kill,
        LevelUp,
        Gold,
        Boss,
        Craft,
        Quest,
        Combo,
        Survival,
        Damage,
        Skill,
        Explore,
        EnrageKill,
        PerfectBlock,
        CounterAttack,
        NoHitBoss
    }

    /// <summary>
    /// 成就难度
    /// </summary>
    public enum AchievementDifficulty
    {
        Easy,
        Normal,
        Hard,
        Epic,
        Legendary
    }

    /// <summary>
    /// 成就数据
    /// </summary>
    public class Achievement
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AchievementType Type { get; set; }
        public AchievementDifficulty Difficulty { get; set; }
        public int RequiredValue { get; set; }
        public int RewardGold { get; set; }
        public int RewardExp { get; set; }
    }
}
