using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Characters;

namespace ClawRPG.Scripts.Systems.BossMechanics {
    /// <summary>
    /// Boss 攻击模式选择器
    /// REQ-156-02: 支持策略/狂暴两套攻击序列
    /// - Strategic: 按权重选择，保持节奏感
    /// - Enraged: 完全随机化，体现"乱拍"感
    /// </summary>
    public class BossAttackPatterns
    {
        private Random _random = new Random();

        // 策略模式攻击序列（按权重）
        private List<AttackPatternEntry> _strategicAttacks = new List<AttackPatternEntry>();
        // 狂暴模式攻击序列（完全随机）
        private List<string> _enragedAttacks = new List<string>();

        // 狂暴专属攻击
        private List<string> _enragedExclusiveAttacks = new List<string>();

        public BossAttackPatterns()
        {
            InitializeAttacks();
        }

        /// <summary>
        /// 初始化攻击序列
        /// </summary>
        private void InitializeAttacks()
        {
            // Strategic 模式攻击（带权重）
            _strategicAttacks.Add(new AttackPatternEntry("basic_attack", 30f, false));
            _strategicAttacks.Add(new AttackPatternEntry("fire_breath", 15f, true));
            _strategicAttacks.Add(new AttackPatternEntry("dark_bolt", 15f, false));
            _strategicAttacks.Add(new AttackPatternEntry("lightning_chain", 12f, true));
            _strategicAttacks.Add(new AttackPatternEntry("poison_cloud", 10f, true));
            _strategicAttacks.Add(new AttackPatternEntry("ground_slam", 8f, true));
            _strategicAttacks.Add(new AttackPatternEntry("heal", 5f, false));
            _strategicAttacks.Add(new AttackPatternEntry("teleport", 5f, false));

            // Enraged 模式可用攻击（完全随机）
            _enragedAttacks.Add("basic_attack");
            _enragedAttacks.Add("fire_breath");
            _enragedAttacks.Add("dark_bolt");
            _enragedAttacks.Add("lightning_chain");
            _enragedAttacks.Add("poison_cloud");
            _enragedAttacks.Add("ground_slam");
            _enragedAttacks.Add("magic_missile");
            _enragedAttacks.Add("ice_lance");
            _enragedAttacks.Add("fear_shout");
            _enragedAttacks.Add("bleed_wave");

            // 狂暴专属攻击（仅狂暴时可用）
            _enragedExclusiveAttacks.Add("enraged_burst");  // 连续冲刺
            _enragedExclusiveAttacks.Add("rapid_fire");      // 快速连击
            _enragedExclusiveAttacks.Add("desperate_strike"); // 孤注一掷
        }

        /// <summary>
        /// 策略模式：按权重选择攻击
        /// </summary>
        /// <param name="healthPercent">当前血量百分比</param>
        /// <param name="currentPhase">当前阶段</param>
        /// <param name="targetCount">目标数量</param>
        /// <returns>选择的攻击ID</returns>
        public string SelectStrategicAttack(float healthPercent, int currentPhase, int targetCount)
        {
            // 低血量时增加治疗优先级
            if (healthPercent < 0.3f)
            {
                foreach (var entry in _strategicAttacks)
                {
                    if (entry.AttackId == "heal")
                    {
                        entry.Weight = 20f; // 低血量时提高治疗权重
                        break;
                    }
                }
            }

            // 多目标时提高AoE权重
            if (targetCount > 1)
            {
                foreach (var entry in _strategicAttacks)
                {
                    if (entry.IsAoE)
                    {
                        entry.Weight *= 1.5f;
                    }
                }
            }

            // 后期阶段提高强力技能权重
            if (currentPhase >= 3)
            {
                foreach (var entry in _strategicAttacks)
                {
                    if (entry.AttackId == "fire_breath" || entry.AttackId == "dark_bolt")
                    {
                        entry.Weight *= 1.3f;
                    }
                }
            }

            // 加权随机选择
            float totalWeight = 0f;
            foreach (var entry in _strategicAttacks)
            {
                totalWeight += entry.Weight;
            }

            float roll = (float)_random.NextDouble() * totalWeight;
            float cumulative = 0f;

            foreach (var entry in _strategicAttacks)
            {
                cumulative += entry.Weight;
                if (roll <= cumulative)
                {
                    // 重置权重（避免影响下次选择）
                    ResetWeights();
                    GD.Print($"[BossAttackPatterns] Strategic: selected {entry.AttackId}");
                    return entry.AttackId;
                }
            }

            ResetWeights();
            return "basic_attack";
        }

        /// <summary>
        /// 狂暴模式：完全随机选择（REQ-156）
        /// 攻击类型和目标均随机，体现"乱拍"感
        /// </summary>
        /// <returns>选择的攻击ID</returns>
        public string SelectEnragedAttack()
        {
            // 70%几率从常规狂暴攻击池选择
            if (_random.NextDouble() < 0.7)
            {
                int idx = _random.Next(_enragedAttacks.Count);
                string attack = _enragedAttacks[idx];
                GD.Print($"[BossAttackPatterns] Enraged: random skill = {attack}");
                return attack;
            }
            else
            {
                // 30%几率使用狂暴专属攻击
                int idx = _random.Next(_enragedExclusiveAttacks.Count);
                string attack = _enragedExclusiveAttacks[idx];
                GD.Print($"[BossAttackPatterns] Enraged: EXCLUSIVE skill = {attack}");
                return attack;
            }
        }

        /// <summary>
        /// 获取狂暴专属攻击序列名称（用于UI显示）
        /// </summary>
        public string GetEnragedBurstName()
        {
            return "⚡ ENRAGED BURST";
        }

        /// <summary>
        /// 获取狂暴专属攻击序列描述
        /// </summary>
        public string GetEnragedBurstDescription()
        {
            return "连续冲刺攻击！";
        }

        /// <summary>
        /// 获取策略攻击列表（用于UI展示）
        /// </summary>
        public List<string> GetStrategicAttackList()
        {
            var list = new List<string>();
            foreach (var entry in _strategicAttacks)
            {
                list.Add(entry.AttackId);
            }
            return list;
        }

        /// <summary>
        /// 获取狂暴攻击列表（用于UI展示）
        /// </summary>
        public List<string> GetEnragedAttackList()
        {
            var list = new List<string>(_enragedAttacks);
            list.AddRange(_enragedExclusiveAttacks);
            return list;
        }

        /// <summary>
        /// 重置权重（每次选择后调用）
        /// </summary>
        private void ResetWeights()
        {
            foreach (var entry in _strategicAttacks)
            {
                switch (entry.AttackId)
                {
                    case "basic_attack": entry.Weight = 30f; break;
                    case "fire_breath": entry.Weight = 15f; break;
                    case "dark_bolt": entry.Weight = 15f; break;
                    case "lightning_chain": entry.Weight = 12f; break;
                    case "poison_cloud": entry.Weight = 10f; break;
                    case "ground_slam": entry.Weight = 8f; break;
                    case "heal": entry.Weight = 5f; break;
                    case "teleport": entry.Weight = 5f; break;
                }
            }
        }

        /// <summary>
        /// 攻击序列条目
        /// </summary>
        private class AttackPatternEntry
        {
            public string AttackId { get; set; }
            public float Weight { get; set; }
            public bool IsAoE { get; set; }

            public AttackPatternEntry(string attackId, float weight, bool isAoE)
            {
                AttackId = attackId;
                Weight = weight;
                IsAoE = isAoE;
            }
        }
    }
}
