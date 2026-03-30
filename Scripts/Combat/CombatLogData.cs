using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// Combat log entry types
    /// </summary>
    public enum CombatLogType
    {
        Damage,
        Healing,
        Buff,
        Debuff,
        Kill,
        Death,
        SkillUsed,
        ItemUsed,
        EnemySpawn,
        EnemyAggro,
        Combo,
        Critical,
        Miss,
        Block,
        Dodge,
        Parry,
        Shield,
        Mana,
        Energy,
        Experience,
        LevelUp,
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Single combat log entry
    /// </summary>
    public class CombatLogEntry
    {
        public float Timestamp { get; set; }
        public CombatLogType Type { get; set; }
        public string Message { get; set; }
        public float Value { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public bool IsPlayerAction { get; set; }
        public Dictionary Data { get; set; }

        public CombatLogEntry()
        {
            Timestamp = 0f;
            Type = CombatLogType.Info;
            Message = "";
            Value = 0f;
            Source = "";
            Target = "";
            IsPlayerAction = true;
            Data = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Combat log statistics
    /// </summary>
    public class CombatLogStatistics
    {
        public int TotalEntries { get; set; }
        public int DamageEntries { get; set; }
        public int HealingEntries { get; set; }
        public int KillEntries { get; set; }
        public int CriticalHits { get; set; }
        public int Misses { get; set; }
        public int Blocks { get; set; }
        public int Dodges { get; set; }
        public float TotalDamageDealt { get; set; }
        public float TotalDamageTaken { get; set; }
        public float TotalHealing { get; set; }

        public CombatLogStatistics()
        {
            Reset();
        }

        public void Reset()
        {
            TotalEntries = 0;
            DamageEntries = 0;
            HealingEntries = 0;
            KillEntries = 0;
            CriticalHits = 0;
            Misses = 0;
            Blocks = 0;
            Dodges = 0;
            TotalDamageDealt = 0;
            TotalDamageTaken = 0;
            TotalHealing = 0;
        }
    }
}
