using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Characters {
    /// <summary>
    /// Boss data container - holds all boss configuration and state data
    /// </summary>
    public class BossData
    {
        // Boss Properties
        public string BossTitle { get; set; } = "Ancient Dragon";
        public string BossId { get; set; } = "";
        public int PhaseCount { get; set; } = 3;
        public float EnrageTime { get; set; } = 120f;
        
        // Phase Settings
        public int[] PhaseHealthThresholds { get; set; } = { 66, 33 };
        
        // Special Abilities
        public float AbilityCooldown { get; set; } = 10f;
        public string[] SpecialAbilities { get; set; }
        
        // Ability Database
        public Dictionary<string, BossAbility> AbilityDatabase { get; private set; }
        public Dictionary<string, float> AbilityCurrentCooldowns { get; private set; }
        public List<string> AvailableAbilities { get; private set; }
        
        // Current State
        public int CurrentPhase { get; set; } = 1;
        public float AbilityTimer { get; set; }
        public float EnrageTimer { get; set; }
        public bool IsEnraged { get; set; }
        public bool PhaseTransitioning { get; set; }
        
        // Rage shader
        public ShaderMaterial RageMaterial { get; set; }
        
        public BossData()
        {
            AbilityDatabase = new Dictionary<string, BossAbility>();
            AbilityCurrentCooldowns = new Dictionary<string, float>();
            AvailableAbilities = new List<string>();
            AbilityTimer = 5f;
        }
        
        /// <summary>
        /// Initialize ability database with default abilities
        /// </summary>
        public void InitializeAbilityDatabase()
        {
            AbilityDatabase.Clear();
            AbilityCurrentCooldowns.Clear();
            
            // Offensive abilities
            AbilityDatabase["fire_breath"] = new BossAbility("fire_breath", "火焰吐息", "喷射火焰造成持续伤害", 12f, 1.5f, 250f)
            {
                ApplyStatus = StatusEffectType.Burning,
                StatusChance = 0.8f,
                IsAoE = true,
                AoERadius = 100f,
                Duration = 5f
            };
            
            AbilityDatabase["lightning_chain"] = new BossAbility("lightning_chain", "闪电链", "连锁攻击多个目标", 15f, 1.2f, 200f)
            {
                IsAoE = true,
                AoERadius = 150f
            };
            
            AbilityDatabase["poison_cloud"] = new BossAbility("poison_cloud", "毒云", "释放毒云持续伤害", 18f, 0.8f, 180f)
            {
                ApplyStatus = StatusEffectType.Poisoned,
                StatusChance = 1f,
                IsAoE = true,
                AoERadius = 120f,
                Duration = 8f
            };
            
            AbilityDatabase["ice_lance"] = new BossAbility("ice_lance", "寒冰长矛", "快速冰冻攻击", 8f, 1.0f, 300f)
            {
                ApplyStatus = StatusEffectType.Frozen,
                StatusChance = 0.6f,
                Duration = 2f
            };
            
            AbilityDatabase["dark_bolt"] = new BossAbility("dark_bolt", "暗影箭", "暗影属性强力攻击", 10f, 1.8f, 200f)
            {
                ApplyStatus = StatusEffectType.Cursed,
                StatusChance = 0.5f,
                Duration = 5f
            };
            
            AbilityDatabase["ground_slam"] = new BossAbility("ground_slam", "地震猛击", "强力范围攻击", 14f, 2.0f, 100f)
            {
                IsAoE = true,
                AoERadius = 200f,
                StatusChance = 0.3f,
                ApplyStatus = StatusEffectType.Stunned,
                Duration = 1f
            };
            
            AbilityDatabase["fear_shout"] = new BossAbility("fear_shout", "恐惧咆哮", "使敌人恐惧逃跑", 20f, 0.5f, 250f)
            {
                IsAoE = true,
                AoERadius = 250f,
                ApplyStatus = StatusEffectType.Frozen,
                StatusChance = 0.7f,
                Duration = 3f
            };
            
            AbilityDatabase["bleed_wave"] = new BossAbility("bleed_wave", "鲜血波纹", "造成出血效果", 16f, 1.3f, 150f)
            {
                ApplyStatus = StatusEffectType.Bleeding,
                StatusChance = 0.9f,
                Duration = 6f,
                IsAoE = true,
                AoERadius = 180f
            };
            
            AbilityDatabase["magic_missile"] = new BossAbility("magic_missile", "奥术飞弹", "追踪魔法攻击", 6f, 0.9f, 250f);
            
            AbilityDatabase["heal"] = new BossAbility("heal", "自我治疗", "恢复生命值", 25f, 0f);
            
            AbilityDatabase["teleport"] = new BossAbility("teleport", "闪现", "瞬间移动位置", 18f, 0f);
            
            AbilityDatabase["summon_minions"] = new BossAbility("summon_minions", "召唤小怪", "召唤助手作战", 30f, 0f)
            {
                IsAoE = true,
                AoERadius = 100f
            };
            
            // Initialize cooldowns
            foreach (var ability in AbilityDatabase.Keys)
            {
                AbilityCurrentCooldowns[ability] = 0f;
            }
        }
        
        /// <summary>
        /// Initialize available abilities from export
        /// </summary>
        public void InitializeAvailableAbilities(string[] abilities)
        {
            AvailableAbilities.Clear();
            if (abilities != null)
            {
                AvailableAbilities.AddRange(abilities);
            }
        }
        
        /// <summary>
        /// Update ability cooldowns
        /// </summary>
        public void UpdateCooldowns(float dt)
        {
            foreach (var ability in AbilityCurrentCooldowns.Keys)
            {
                if (AbilityCurrentCooldowns[ability] > 0)
                {
                    AbilityCurrentCooldowns[ability] -= dt;
                }
            }
        }
        
        /// <summary>
        /// Get list of abilities ready to use
        /// </summary>
        public List<string> GetReadyAbilities()
        {
            List<string> readyAbilities = new List<string>();
            foreach (string ability in AvailableAbilities)
            {
                if (AbilityCurrentCooldowns.ContainsKey(ability) && AbilityCurrentCooldowns[ability] <= 0)
                {
                    readyAbilities.Add(ability);
                }
            }
            return readyAbilities;
        }
        
        /// <summary>
        /// Set ability on cooldown
        /// </summary>
        public void SetAbilityOnCooldown(string abilityId)
        {
            if (AbilityDatabase.ContainsKey(abilityId))
            {
                AbilityCurrentCooldowns[abilityId] = AbilityDatabase[abilityId].Cooldown;
            }
        }
        
        /// <summary>
        /// Get ability by ID
        /// </summary>
        public BossAbility GetAbility(string abilityId)
        {
            if (AbilityDatabase.TryGetValue(abilityId, out var ability))
            {
                return ability;
            }
            return null;
        }
        
        /// <summary>
        /// Export save data
        /// </summary>
        public Dictionary ExportSaveData()
        {
            return new Dictionary
            {
                { "currentPhase", CurrentPhase },
                { "isEnraged", IsEnraged },
                { "enrageTimer", EnrageTimer },
                { "abilityTimer", AbilityTimer }
            };
        }
        
        /// <summary>
        /// Import save data
        /// </summary>
        public void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            if (data.Contains("currentPhase"))
                CurrentPhase = (int)data["currentPhase"];
            if (data.Contains("isEnraged"))
                IsEnraged = (bool)data["isEnraged"];
            if (data.Contains("enrageTimer"))
                EnrageTimer = (float)data["enrageTimer"];
            if (data.Contains("abilityTimer"))
                AbilityTimer = (float)data["abilityTimer"];
        }
    }
}
