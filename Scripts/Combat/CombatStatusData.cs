using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// Combat status data structures
    /// </summary>
    public class CombatStatusData
    {
        // Combat event types
        public enum CombatEventType
        {
            DamageDealt,
            DamageTaken,
            HealingDone,
            CriticalHit,
            Block,
            Dodge,
            EnemyKilled,
            BossDamage,
            SkillUsed,
            BuffApplied,
            DebuffApplied
        }

        // Single combat event
        public class CombatEvent
        {
            public CombatEventType Type;
            public float Value;
            public string Description;
            public double Timestamp;
            public bool IsCritical;
        }

        // Damage breakdown by type
        public class DamageBreakdown
        {
            public float PhysicalDamage;
            public float MagicDamage;
            public float FireDamage;
            public float IceDamage;
            public float LightningDamage;
            public float DarkDamage;
            public float HolyDamage;
            public float PoisonDamage;
            
            public float Total => PhysicalDamage + MagicDamage + FireDamage + IceDamage + 
                                 LightningDamage + DarkDamage + HolyDamage + PoisonDamage;
        }

        // Player combat status
        public class PlayerCombatStatus
        {
            public float TotalDamageDealt;
            public float TotalDamageTaken;
            public float TotalHealingDone;
            public float TotalHealingReceived;
            public int CriticalHits;
            public int Blocks;
            public int Dodges;
            public int EnemiesKilled;
            public int SkillsUsed;
            public int CurrentCombo;
            public int MaxCombo;
            public double CombatStartTime;
            public bool IsInCombat;
            public DamageBreakdown DamageDealtBreakdown;
            public DamageBreakdown DamageTakenBreakdown;
            public List<CombatEvent> RecentEvents;
            public int ActiveBuffs;
            public int ActiveDebuffs;

            public PlayerCombatStatus()
            {
                DamageDealtBreakdown = new DamageBreakdown();
                DamageTakenBreakdown = new DamageBreakdown();
                RecentEvents = new List<CombatEvent>();
            }
        }

        // Session statistics
        public class SessionStats
        {
            public int TotalCombats;
            public float TotalDamageDealt;
            public float TotalDamageTaken;
            public float TotalHealingDone;
            public int TotalCriticalHits;
            public int TotalEnemiesKilled;
            public double TotalCombatTime;
            public int BestCombo;
            public float HighestDPS;
            public DateTime SessionStart;

            public SessionStats()
            {
                SessionStart = DateTime.Now;
            }
        }

        // Combat rating grades
        public enum CombatGrade
        {
            D,  // Poor
            C,  // Fair
            B,  // Good
            A,  // Excellent
            S   // Perfect
        }
    }
}
