using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.BossMechanics {
    /// <summary>
    /// Boss AI - Handles boss decision making and behavior
    /// Part of BossMechanicsSystem refactoring
    /// REQ-156: Boss 狂暴姿态 = 真实 AI 行为模式变化
    /// </summary>
    public partial class BossAI : BaseSystem
    {
        /// <summary>
        /// Boss AI 行为模式 (REQ-156)
        /// Strategic: 策略模式，攻击有节奏、可预判
        /// Enraged:   狂暴模式，攻击间隔缩短50%，攻击随机化
        /// </summary>
        public enum BossMode
        {
            Strategic,  // 默认策略模式
            Enraged    // 狂暴模式
        }

        private BossMechanicsSystem _bossSystem;
        private BossAbilityDatabase _abilityDb;
        private BossPhaseManager _phaseManager;
        
        // AI configuration
        private float _decisionInterval = 1.0f;  // Make decisions every second
        private float _decisionTimer = 0f;
        
        // Random for AI decisions
        private Random _random = new Random();

        // REQ-156: 模式状态
        private BossMode _currentMode = BossMode.Strategic;
        
        // REQ-156: 狂暴时攻击间隔缩减倍率（0.5 = 缩短50%）
        private float _enragedAttackIntervalMultiplier = 0.5f;
        
        public BossAI(BossMechanicsSystem bossSystem, BossAbilityDatabase abilityDb, BossPhaseManager phaseManager)
        {
            _bossSystem = bossSystem;
            _abilityDb = abilityDb;
            _phaseManager = phaseManager;
        }

        // ========================
        // REQ-156: 模式切换 API
        // ========================

        /// <summary>
        /// 模式切换时发射的信号 (REQ-156)
        /// 参数: (oldMode, newMode)
        /// </summary>
public delegate void BossModeChangedEventHandler(BossMode oldMode, BossMode newMode);

        /// <summary>
        /// 切换 Boss AI 行为模式 (REQ-156)
        /// Strategic: 正常决策间隔，权重选择
        /// Enraged:   间隔缩短50%，攻击完全随机化
        /// </summary>
        public void SetMode(BossMode mode)
        {
            if (_currentMode == mode) return;

            BossMode oldMode = _currentMode;
            _currentMode = mode;
            
            GD.Print($"[BossAI] Mode changed: {oldMode} → {mode}");
            EmitSignal(nameof(BossModeChanged), oldMode, mode);
        }

        /// <summary>
        /// 获取当前行为模式 (REQ-156)
        /// </summary>
        public BossMode GetMode() => _currentMode;

        /// <summary>
        /// 是否处于狂暴模式 (REQ-156)
        /// </summary>
        public bool IsEnraged => _currentMode == BossMode.Enraged;

        /// <summary>
        /// 获取狂暴模式下攻击间隔倍率 (REQ-156)
        /// 返回 0.5 表示狂暴时攻击间隔缩短为正常的一半
        /// </summary>
        public float GetEnragedAttackIntervalMultiplier() => _enragedAttackIntervalMultiplier;

        // ========================
        // 原有公开方法
        // ========================

        /// <summary>
        /// Set AI decision interval
        /// </summary>
        public void SetDecisionInterval(float interval)
        {
            _decisionInterval = interval;
        }
        
        /// <summary>
        /// Update AI decision making (REQ-156)
        /// Enraged 模式下决策频率提升（间隔缩短50%）
        /// </summary>
        public void Update(BossBattleState state, BossMechanicsData bossData, float delta)
        {
            if (state == null || bossData == null) return;
            
            // REQ-156: Enraged 模式下决策间隔缩短
            float effectiveInterval = _decisionInterval;
            if (_currentMode == BossMode.Enraged)
            {
                effectiveInterval *= _enragedAttackIntervalMultiplier;
            }
            
            _decisionTimer += delta;
            
            if (_decisionTimer >= effectiveInterval)
            {
                _decisionTimer = 0f;
                MakeDecision(state, bossData);
            }
        }
        
        /// <summary>
        /// Make AI decision
        /// </summary>
        private void MakeDecision(BossBattleState state, BossMechanicsData bossData)
        {
            // Check phase transitions
            _phaseManager.CheckPhaseTransition(state, bossData);
            
            // Check enrage
            CheckEnrage(state, bossData);
            
            // Decide whether to use a skill
            if (bossData.Skills != null && bossData.Skills.Count > 0)
            {
                TryUseSkill(state, bossData);
            }
            
            // Check minion spawning
            TrySpawnMinions(state, bossData);
        }
        
        /// <summary>
        /// Try to use a skill
        /// </summary>
        private void TryUseSkill(BossBattleState state, BossMechanicsData bossData)
        {
            // Get available skills
            var availableSkills = _abilityDb.GetAvailableSkills(state, bossData);
            if (availableSkills.Count == 0) return;
            
            // Select skill based on AI personality
            string selectedSkill = SelectSkill(state, bossData, availableSkills);
            
            if (!string.IsNullOrEmpty(selectedSkill))
            {
                _abilityDb.UseSkill(state, bossData, selectedSkill);
            }
        }
        
        /// <summary>
        /// Select skill based on boss AI personality (REQ-156)
        /// Enraged 模式：攻击完全随机化，不再按权重选择
        /// Strategic 模式：原有权重选择逻辑
        /// </summary>
        private string SelectSkill(BossBattleState state, BossMechanicsData bossData, List<string> availableSkills)
        {
            if (availableSkills.Count == 0) return null;

            // REQ-156: Enraged 模式下攻击完全随机化
            if (_currentMode == BossMode.Enraged)
            {
                int idx = _random.Next(availableSkills.Count);
                string enragedSkill = availableSkills[idx];
                GD.Print($"[BossAI] Enraged mode: random skill selected = {enragedSkill}");
                return enragedSkill;
            }

            // Strategic mode: 原有权重选择逻辑
            float healthPercent = state.CurrentHealth / state.MaxHealth;
            
            // In low health, prefer defensive/healing skills if available
            if (healthPercent < 0.3f)
            {
                foreach (var skillId in availableSkills)
                {
                    var skill = _abilityDb.GetSkillData(bossData, skillId);
                    if (skill != null && skill.SkillName.ToLower().Contains("heal"))
                    {
                        return skillId;
                    }
                }
            }
            
            // Use ability database's selection
            return _abilityDb.SelectBestSkill(state, bossData);
        }
        
        /// <summary>
        /// Check and trigger enrage mechanic (REQ-156)
        /// 狂暴触发时切换到 Enraged 模式，后续 SelectSkill 会切换为随机选择
        /// </summary>
        private void CheckEnrage(BossBattleState state, BossMechanicsData bossData)
        {
            if (bossData == null || !bossData.HasEnrageMechanic) return;
            
            if (!state.IsEnraged && state.BattleTime >= bossData.EnrageTime)
            {
                state.IsEnraged = true;
                GD.Print($"[BossAI] Boss {bossData.BossName} is ENRAGED!");
                
                // REQ-156: 触发模式切换为 Enraged
                SetMode(BossMode.Enraged);
            }
        }
        
        /// <summary>
        /// Try to spawn minions
        /// </summary>
        private void TrySpawnMinions(BossBattleState state, BossMechanicsData bossData)
        {
            if (bossData == null || !bossData.CanSummonMinions) return;
            
            float healthPercent = state.CurrentHealth / state.MaxHealth;
            
            // Spawn based on health threshold
            if (bossData.MinionSpawnHealthPercent > 0 && healthPercent <= bossData.MinionSpawnHealthPercent)
            {
                if (state.ActiveMinionCount < bossData.MaxMinionCount)
                {
                    // Random chance per update when in range
                    if (_random.NextDouble() < 0.01) // 1% chance per decision interval
                    {
                        _bossSystem.SpawnMinions(state.BossId, bossData.MinionTypes, 1);
                    }
                }
            }
        }
        
        /// <summary>
        /// Calculate attack multiplier based on current state
        /// </summary>
        public float GetAttackMultiplier(BossBattleState state, BossMechanicsData bossData)
        {
            if (state == null || bossData == null) return 1.0f;
            
            float multiplier = 1.0f;
            
            // Phase multiplier
            float phaseMultiplier = _phaseManager.GetPhaseAttackMultiplier(state, bossData);
            multiplier *= phaseMultiplier;
            
            // Enrage multiplier
            if (state.IsEnraged && bossData.EnrageTimers != null && bossData.EnrageTimers.Count > 0)
            {
                multiplier *= bossData.EnrageTimers[0].AttackMultiplier;
            }
            
            return multiplier;
        }
        
        /// <summary>
        /// Generate loot on boss defeat
        /// </summary>
        public List<string> GenerateLoot(BossMechanicsData bossData)
        {
            List<string> loot = new List<string>();
            
            if (bossData.LootTable == null || bossData.LootTable.Length == 0)
                return loot;
            
            int lootCount = bossData.MinLootCount + _random.Next(bossData.MaxLootCount - bossData.MinLootCount + 1);
            
            for (int i = 0; i < lootCount; i++)
            {
                float roll = (float)_random.NextDouble() * 100f;
                float cumulative = 0f;
                
                for (int j = 0; j < bossData.LootTable.Length && j < bossData.LootWeights.Length; j++)
                {
                    cumulative += bossData.LootWeights[j];
                    if (roll <= cumulative)
                    {
                        loot.Add(bossData.LootTable[j]);
                        break;
                    }
                }
            }
            
            return loot;
        }
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["decisionInterval"] = _decisionInterval;
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data.Contains("decisionInterval")) {
                _decisionInterval = Convert.ToSingle(data["decisionInterval"]);
            }
        }
    }
}
