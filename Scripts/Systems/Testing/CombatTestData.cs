using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Testing
{
    /// <summary>
    /// 玩家战斗状态快照（用于模拟器输入）
    /// </summary>
    [Serializable]
    public class SimPlayerState
    {
        public int MaxHealth { get; set; } = 100;
        public int CurrentHealth { get; set; } = 100;
        public int Attack { get; set; } = 10;
        public int Defense { get; set; } = 5;
        public float CritChance { get; set; } = 0.1f;
        public float CritMultiplier { get; set; } = 1.5f;
        public float DodgeChance { get; set; } = 0.05f;
        public List<string> ActiveSkills { get; set; } = new();
    }

    /// <summary>
    /// 敌人战斗状态快照（用于模拟器输入）
    /// </summary>
    [Serializable]
    public class SimEnemyState
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "Enemy";
        public int MaxHealth { get; set; } = 30;
        public int CurrentHealth { get; set; } = 30;
        public int Attack { get; set; } = 8;
        public int Defense { get; set; } = 2;
        public float CritChance { get; set; } = 0.05f;
        public float CritMultiplier { get; set; } = 1.5f;
        public float DodgeChance { get; set; } = 0.0f;
        /// <summary>敌人速度（影响行动顺序）</summary>
        public int Speed { get; set; } = 10;
        /// <summary>精英/Boss 缩放因子</summary>
        public float EliteMultiplier { get; set; } = 1.0f;
    }

    /// <summary>
    /// 技能定义（用于模拟器）
    /// </summary>
    [Serializable]
    public class SimSkill
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int BaseDamage { get; set; } = 10;
        /// <summary>伤害类型：physical/magic/true</summary>
        public string DamageType { get; set; } = "physical";
        public int ManaCost { get; set; } = 0;
        public float Cooldown { get; set; } = 0f;
        public int TimesUsed { get; set; } = 0;
    }

    /// <summary>
    /// 战斗模拟结果
    /// </summary>
    [Serializable]
    public class CombatResult
    {
        public bool Victory { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalDamageTaken { get; set; }
        public int RoundsWon { get; set; }
        public int TotalRounds { get; set; }
        public int PlayerFinalHealth { get; set; }
        public int EnemyFinalHealth { get; set; }
        public Dictionary<string, int> SkillUsage { get; set; } = new();
        public int CriticalHitsDealt { get; set; }
        public int CriticalHitsTaken { get; set; }
        public int DodgesPerformed { get; set; }
        public int DodgesSuffered { get; set; }
        public List<string> CombatLog { get; set; } = new();
        public long ElapsedMs { get; set; }

        public float DamagePerRound => TotalRounds > 0 ? (float)TotalDamageDealt / TotalRounds : 0f;
        public float SurvivalRate => TotalRounds > 0 ? (float)PlayerFinalHealth / 100f : 0f;
    }

    /// <summary>
    /// 战斗测试用例（可作为 Godot Resource 加载）
    /// </summary>
    [Serializable]
    public class CombatTestCase
    {
        public string TestId { get; set; } = "";
        public string Description { get; set; } = "";
        public SimPlayerState Player { get; set; } = new();
        public List<SimEnemyState> Enemies { get; set; } = new();
        public List<SimSkill> AvailableSkills { get; set; } = new();
        /// <summary>预期胜利（null = 不检查）</summary>
        public bool? ExpectedVictory { get; set; }
        /// <summary>预期最小伤害</summary>
        public int? MinDamage { get; set; }
        /// <summary>预期最大伤害</summary>
        public int? MaxDamage { get; set; }
        /// <summary>最大模拟回合数（防死循环）</summary>
        public int MaxRounds { get; set; } = 100;
    }

    /// <summary>
    /// 测试套件（多个测试用例）
    /// </summary>
    [Serializable]
    public class CombatTestSuite
    {
        public string SuiteId { get; set; } = "";
        public string Description { get; set; } = "";
        public List<CombatTestCase> Cases { get; set; } = new();
    }
}
