using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Meditation
{
    /// <summary>
    /// Meditation session data
    /// </summary>
    public class MeditationSession
    {
        public string SessionId { get; set; }
        public string PlayerId { get; set; }
        public MeditationType Type { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Duration { get; set; } // seconds
        public bool Completed { get; set; }
        public List<string> AchievedBenefits { get; set; } = new List<string>();
        public int FocusGained { get; set; }
        
        public MeditationSession()
        {
            SessionId = Guid.NewGuid().ToString();
            StartTime = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Types of meditation
    /// </summary>
    public enum MeditationType
    {
        Focus,         // 专注冥想 - 增加专注力
        Healing,        // 治愈冥想 - 恢复生命值
        Clarity,       // 清晰冥想 - 清除负面效果
        Strength,      // 力量冥想 - 临时攻击力提升
        Defense,       // 防御冥想 - 临时防御力提升
        Speed,         // 速度冥想 - 临时速度提升
        Wisdom,        // 智慧冥想 - 经验加成
        Endurance,     // 耐力冥想 - 最大生命值临时提升
        Spirit,        // 精神冥想 - 魔法值恢复
        Balance        // 平衡冥想 - 全属性小幅提升
    }
    
    /// <summary>
    /// Meditation benefit data
    /// </summary>
    public class MeditationBenefit
    {
        public string BenefitId { get; set; }
        public string BenefitName { get; set; }
        public string Description { get; set; }
        public MeditationType Type { get; set; }
        public int MinDuration { get; set; } // minimum seconds to achieve
        public float EffectMultiplier { get; set; }
        public string StatAffected { get; set; }
        public float BaseValue { get; set; }
        public int Duration { get; set; } // seconds, -1 for permanent
    }
    
    /// <summary>
    /// Player meditation progress
    /// </summary>
    public class MeditationProgress
    {
        public string PlayerId { get; set; }
        public int TotalSessions { get; set; }
        public int TotalMeditationTime { get; set; } // seconds
        public Dictionary<MeditationType, int> SessionsByType { get; set; } = new Dictionary<MeditationType, int>();
        public int CurrentFocus { get; set; }
        public int MaxFocus { get; set; } = 100;
        public List<string> UnlockedAbilities { get; set; } = new List<string>();
        public List<MeditationSession> RecentSessions { get; set; } = new List<MeditationSession>();
        public DateTime LastMeditationTime { get; set; }
        public int DailySessions { get; set; }
        public DateTime DailyResetTime { get; set; }
        
        public MeditationProgress()
        {
            foreach (MeditationType type in Enum.GetValues(typeof(MeditationType)))
            {
                SessionsByType[type] = 0;
            }
            DailyResetTime = DateTime.Today.AddDays(1);
        }
    }
    
    /// <summary>
    /// Active meditation buff on player
    /// </summary>
    public class MeditationBuff
    {
        public string BuffId { get; set; }
        public MeditationType Type { get; set; }
        public string StatAffected { get; set; }
        public float Value { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; } // seconds
        public bool IsPermanent { get; set; }
    }
}
