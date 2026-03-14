// BossMechanicsDatabase.cs - Boss 机制系统配置数据库
using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.BossMechanics;

namespace ClawRPG.Scripts.BossMechanics {
    
    public class BossMechanicsDatabase {
        
        private static BossMechanicsDatabase _instance;
        public static BossMechanicsDatabase Instance {
            get {
                if (_instance == null) {
                    _instance = new BossMechanicsDatabase();
                }
                return _instance;
            }
        }
        
        // Boss 配置
        public Dictionary<string, BossState> BossConfigs { get; private set; }
        
        // Boss 技能库
        public Dictionary<string, BossSkill> SkillDatabase { get; private set; }
        
        // 战斗阶段配置
        public Dictionary<string, List<BattlePhase>> BossPhases { get; private set; }
        
        // Boss 掉落配置
        public Dictionary<string, List<BossLoot>> BossLootTables { get; private set; }
        
        // Boss AI 配置
        public Dictionary<BossAIBehavior, BossAIConfig> AIBehaviorConfigs { get; private set; }
        
        public BossMechanicsDatabase() {
            Initialize();
        }
        
        private void Initialize() {
            BossConfigs = new Dictionary<string, BossState>();
            SkillDatabase = new Dictionary<string, BossSkill>();
            BossPhases = new Dictionary<string, List<BattlePhase>>();
            BossLootTables = new Dictionary<string, List<BossLoot>>();
            AIBehaviorConfigs = new Dictionary<BossAIBehavior, BossAIConfig>();
            
            InitializeAIBehaviors();
            InitializeSkills();
            InitializeBosses();
            InitializeLootTables();
        }
        
        private void InitializeAIBehaviors() {
            // 激进型
            AIBehaviorConfigs[BossAIBehavior.Aggressive] = new BossAIConfig {
                Behavior = BossAIBehavior.Aggressive,
                AggressionLevel = 0.9f,
                DefensiveThreshold = 0.1f,
                SkillUsageRate = 0.8f,
                PriorityHealThreshold = 0.2f,
                UltimateAbilityThreshold = 0.3f,
                UseEnvironment = false,
                CallReinforcements = false,
                RetreatThreshold = 0.05f
            };
            
            // 防守型
            AIBehaviorConfigs[BossAIBehavior.Defensive] = new BossAIConfig {
                Behavior = BossAIBehavior.Defensive,
                AggressionLevel = 0.4f,
                DefensiveThreshold = 0.7f,
                SkillUsageRate = 0.6f,
                PriorityHealThreshold = 0.5f,
                UltimateAbilityThreshold = 0.4f,
                UseEnvironment = true,
                CallReinforcements = true,
                RetreatThreshold = 0.3f
            };
            
            // 平衡型
            AIBehaviorConfigs[BossAIBehavior.Balanced] = new BossAIConfig {
                Behavior = BossAIBehavior.Balanced,
                AggressionLevel = 0.6f,
                DefensiveThreshold = 0.4f,
                SkillUsageRate = 0.7f,
                PriorityHealThreshold = 0.35f,
                UltimateAbilityThreshold = 0.35f,
                UseEnvironment = true,
                CallReinforcements = false,
                RetreatThreshold = 0.2f
            };
            
            // 战术型
            AIBehaviorConfigs[BossAIBehavior.Tactical] = new BossAIConfig {
                Behavior = BossAIBehavior.Tactical,
                AggressionLevel = 0.5f,
                DefensiveThreshold = 0.5f,
                SkillUsageRate = 0.9f,
                PriorityHealThreshold = 0.4f,
                UltimateAbilityThreshold = 0.25f,
                UseEnvironment = true,
                CallReinforcements = true,
                RetreatThreshold = 0.15f
            };
            
            // 狂战士型
            AIBehaviorConfigs[BossAIBehavior.Berserker] = new BossAIConfig {
                Behavior = BossAIBehavior.Berserker,
                AggressionLevel = 1.0f,
                DefensiveThreshold = 0.05f,
                SkillUsageRate = 1.0f,
                PriorityHealThreshold = 0.1f,
                UltimateAbilityThreshold = 0.5f,
                UseEnvironment = false,
                CallReinforcements = false,
                RetreatThreshold = 0.0f
            };
            
            // 狡猾型
            AIBehaviorConfigs[BossAIBehavior.Cunning] = new BossAIConfig {
                Behavior = BossAIBehavior.Cunning,
                AggressionLevel = 0.55f,
                DefensiveThreshold = 0.6f,
                SkillUsageRate = 0.85f,
                PriorityHealThreshold = 0.45f,
                UltimateAbilityThreshold = 0.2f,
                UseEnvironment = true,
                CallReinforcements = true,
                RetreatThreshold = 0.25f
            };
        }
        
        private void InitializeSkills() {
            // 近战技能
            SkillDatabase["slash"] = new BossSkill {
                SkillId = "slash",
                SkillName = "劈砍",
                Type = BossSkillType.Melee,
                Damage = 50f,
                Cooldown = 2.0f,
                CastTime = 0.5f,
                Range = 3.0f,
                AreaRadius = 0f,
                Cost = 0,
                Description = "对目标造成物理伤害",
                Effects = new List<string> { "physical_damage" },
                PhaseUnlock = 1,
                Weight = 0.3f
            };
            
            SkillDatabase["heavy_strike"] = new BossSkill {
                SkillId = "heavy_strike",
                SkillName = "重击",
                Type = BossSkillType.Melee,
                Damage = 100f,
                Cooldown = 5.0f,
                CastTime = 1.0f,
                Range = 3.5f,
                AreaRadius = 0f,
                Cost = 20,
                Description = "强力打击，造成大量伤害",
                Effects = new List<string> { "physical_damage", "stun" },
                PhaseUnlock = 1,
                Weight = 0.15f
            };
            
            // 远程技能
            SkillDatabase["fireball"] = new BossSkill {
                SkillId = "fireball",
                SkillName = "火球术",
                Type = BossSkillType.Ranged,
                Damage = 60f,
                Cooldown = 3.0f,
                CastTime = 0.8f,
                Range = 15.0f,
                AreaRadius = 4.0f,
                Cost = 15,
                Description = "发射火球攻击目标区域",
                Effects = new List<string> { "fire_damage" },
                PhaseUnlock = 1,
                Weight = 0.2f
            };
            
            SkillDatabase["lightning_bolt"] = new BossSkill {
                SkillId = "lightning_bolt",
                SkillName = "闪电箭",
                Type = BossSkillType.Ranged,
                Damage = 45f,
                Cooldown = 2.5f,
                CastTime = 0.6f,
                Range = 20.0f,
                AreaRadius = 2.0f,
                Cost = 10,
                Description = "快速闪电攻击",
                Effects = new List<string> { "lightning_damage" },
                PhaseUnlock = 1,
                Weight = 0.25f
            };
            
            // AOE 技能
            SkillDatabase["earthquake"] = new BossSkill {
                SkillId = "earthquake",
                SkillName = "地震",
                Type = BossSkillType.AOE,
                Damage = 80f,
                Cooldown = 8.0f,
                CastTime = 1.5f,
                Range = 0f,
                AreaRadius = 10.0f,
                Cost = 30,
                Description = "引发地震，对范围内所有敌人造成伤害",
                Effects = new List<string> { "physical_damage", "knockback" },
                PhaseUnlock = 2,
                Weight = 0.1f
            };
            
            SkillDatabase["meteor"] = new BossSkill {
                SkillId = "meteor",
                SkillName = "陨石",
                Type = BossSkillType.AOE,
                Damage = 150f,
                Cooldown = 15.0f,
                CastTime = 2.0f,
                Range = 25.0f,
                AreaRadius = 6.0f,
                Cost = 50,
                Description = "召唤陨石砸向目标区域",
                Effects = new List<string> { "fire_damage", "stun" },
                PhaseUnlock = 3,
                Weight = 0.05f
            };
            
            // DoT 技能
            SkillDatabase["poison_cloud"] = new BossSkill {
                SkillId = "poison_cloud",
                SkillName = "毒云",
                Type = BossSkillType.DoT,
                Damage = 20f,
                Cooldown = 6.0f,
                CastTime = 1.0f,
                Range = 12.0f,
                AreaRadius = 5.0f,
                Cost = 20,
                Description = "释放毒云，持续伤害敌人",
                Effects = new List<string> { "poison_damage" },
                PhaseUnlock = 2,
                Weight = 0.1f
            };
            
            // 减益技能
            SkillDatabase["curse"] = new BossSkill {
                SkillId = "curse",
                SkillName = "诅咒",
                Type = BossSkillType.Debuff,
                Damage = 30f,
                Cooldown = 10.0f,
                CastTime = 1.2f,
                Range = 15.0f,
                AreaRadius = 0f,
                Cost = 25,
                Description = "诅咒敌人，降低防御",
                Effects = new List<string> { "dark_damage", "defense_reduction" },
                PhaseUnlock = 2,
                Weight = 0.1f
            };
            
            // 治疗技能
            SkillDatabase["self_heal"] = new BossSkill {
                SkillId = "self_heal",
                SkillName = "自我治疗",
                Type = BossSkillType.Heal,
                Damage = -80f,
                Cooldown = 12.0f,
                CastTime = 1.5f,
                Range = 0f,
                AreaRadius = 0f,
                Cost = 30,
                Description = "恢复自身生命值",
                Effects = new List<string> { "heal" },
                PhaseUnlock = 1,
                Weight = 0.15f
            };
            
            // 增益技能
            SkillDatabase["power_up"] = new BossSkill {
                SkillId = "power_up",
                SkillName = "强化",
                Type = BossSkillType.Buff,
                Damage = 0f,
                Cooldown = 20.0f,
                CastTime = 0.5f,
                Range = 0f,
                AreaRadius = 0f,
                Cost = 20,
                Description = "提升自身攻击力",
                Effects = new List<string> { "attack_boost" },
                PhaseUnlock = 1,
                Weight = 0.1f
            };
            
            // 召唤技能
            SkillDatabase["summon_minions"] = new BossSkill {
                SkillId = "summon_minions",
                SkillName = "召唤仆从",
                Type = BossSkillType.Summon,
                Damage = 0f,
                Cooldown = 25.0f,
                CastTime = 2.0f,
                Range = 0f,
                AreaRadius = 0f,
                Cost = 40,
                Description = "召唤小怪协助战斗",
                Effects = new List<string> { "summon" },
                PhaseUnlock = 2,
                Weight = 0.08f
            };
            
            // 终极技能
            SkillDatabase["doomsday"] = new BossSkill {
                SkillId = "doomsday",
                SkillName = "末日",
                Type = BossSkillType.Ultimate,
                Damage = 300f,
                Cooldown = 60.0f,
                CastTime = 3.0f,
                Range = 30.0f,
                AreaRadius = 15.0f,
                Cost = 100,
                Description = "毁灭性终极技能",
                Effects = new List<string> { "fire_damage", "dark_damage", "stun" },
                PhaseUnlock = 3,
                Weight = 0.02f
            };
        }
        
        private void InitializeBosses() {
            // 教程 Boss - 哥布林首领
            var goblinKing = new BossState {
                BossId = "goblin_king",
                BossName = "哥布林首领",
                Type = BossType.TutorialBoss,
                Element = BossElement.Physical,
                Level = 1,
                MaxHealth = 500f,
                CurrentHealth = 500f,
                AttackDamage = 25f,
                Defense = 5f,
                MoveSpeed = 4.0f,
                CurrentPhase = 1,
                IsEnraged = false,
                EnrageTimer = 0f
            };
            BossConfigs["goblin_king"] = goblinKing;
            InitializeBossPhases("goblin_king", 2);
            
            // 普通 Boss - 森林巨魔
            var forestTroll = new BossState {
                BossId = "forest_troll",
                BossName = "森林巨魔",
                Type = BossType.Normal,
                Element = BossElement.Physical,
                Level = 10,
                MaxHealth = 5000f,
                CurrentHealth = 5000f,
                AttackDamage = 150f,
                Defense = 50f,
                MoveSpeed = 3.5f,
                CurrentPhase = 1,
                IsEnraged = false,
                EnrageTimer = 180f
            };
            BossConfigs["forest_troll"] = forestTroll;
            InitializeBossPhases("forest_troll", 3);
            
            // 精英 Boss - 炎魔
            var fireDemon = new BossState {
                BossId = "fire_demon",
                BossName = "炎魔",
                Type = BossType.Elite,
                Element = BossElement.Fire,
                Level = 25,
                MaxHealth = 15000f,
                CurrentHealth = 15000f,
                AttackDamage = 300f,
                Defense = 100f,
                MoveSpeed = 4.0f,
                CurrentPhase = 1,
                IsEnraged = false,
                EnrageTimer = 120f
            };
            BossConfigs["fire_demon"] = fireDemon;
            InitializeBossPhases("fire_demon", 4);
            
            // 小 Boss - 冰霜巨龙
            var frostDrake = new BossState {
                BossId = "frost_drake",
                BossName = "冰霜巨龙",
                Type = BossType.MiniBoss,
                Element = BossElement.Ice,
                Level = 20,
                MaxHealth = 8000f,
                CurrentHealth = 8000f,
                AttackDamage = 200f,
                Defense = 80f,
                MoveSpeed = 5.0f,
                CurrentPhase = 1,
                IsEnraged = false,
                EnrageTimer = 150f
            };
            BossConfigs["frost_drake"] = frostDrake;
            InitializeBossPhases("frost_drake", 3);
            
            // 世界 Boss - 泰坦巨神
            var titan = new BossState {
                BossId = "titan",
                BossName = "泰坦巨神",
                Type = BossType.WorldBoss,
                Element = BossElement.Light,
                Level = 50,
                MaxHealth = 100000f,
                CurrentHealth = 100000f,
                AttackDamage = 500f,
                Defense = 200f,
                MoveSpeed = 2.0f,
                CurrentPhase = 1,
                IsEnraged = false,
                EnrageTimer = 300f
            };
            BossConfigs["titan"] = titan;
            InitializeBossPhases("titan", 5);
            
            // 副本 Boss - 暗影君王
            var shadowLord = new BossState {
                BossId = "shadow_lord",
                BossName = "暗影君王",
                Type = BossType.RaidBoss,
                Element = BossElement.Dark,
                Level = 40,
                MaxHealth = 50000f,
                CurrentHealth = 50000f,
                AttackDamage = 400f,
                Defense = 150f,
                MoveSpeed = 3.0f,
                CurrentPhase = 1,
                IsEnraged = false,
                EnrageTimer = 180f
            };
            BossConfigs["shadow_lord"] = shadowLord;
            InitializeBossPhases("shadow_lord", 4);
        }
        
        private void InitializeBossPhases(string bossId, int phaseCount) {
            var phases = new List<BattlePhase>();
            
            // 第一阶段 - 正常
            phases.Add(new BattlePhase {
                PhaseNumber = 1,
                Type = BattlePhaseType.Normal,
                PhaseName = "第一阶段",
                HealthThreshold = 1.0f,
                DamageMultiplier = 1.0f,
                DefenseMultiplier = 1.0f,
                SpeedMultiplier = 1.0f,
                UnlockedSkills = new List<string> { "slash", "heavy_strike", "self_heal" },
                PhaseBuffs = new List<string>(),
                PhaseDescription = "Boss 处于正常状态",
                Duration = 0
            });
            
            if (phaseCount >= 2) {
                // 第二阶段 - 狂暴
                phases.Add(new BattlePhase {
                    PhaseNumber = 2,
                    Type = BattlePhaseType.Enraged,
                    PhaseName = "狂暴阶段",
                    HealthThreshold = 0.7f,
                    DamageMultiplier = 1.5f,
                    DefenseMultiplier = 0.8f,
                    SpeedMultiplier = 1.3f,
                    UnlockedSkills = new List<string> { "slash", "heavy_strike", "fireball", "lightning_bolt", "power_up" },
                    PhaseBuffs = new List<string> { "enrage" },
                    PhaseDescription = "Boss 进入狂暴状态，攻击性增强",
                    Duration = 30
                });
            }
            
            if (phaseCount >= 3) {
                // 第三阶段 - 虚弱
                phases.Add(new BattlePhase {
                    PhaseNumber = 3,
                    Type = BattlePhaseType.Exhausted,
                    PhaseName = "虚弱阶段",
                    HealthThreshold = 0.4f,
                    DamageMultiplier = 0.7f,
                    DefenseMultiplier = 1.2f,
                    SpeedMultiplier = 0.8f,
                    UnlockedSkills = new List<string> { "slash", "earthquake", "poison_cloud", "self_heal" },
                    PhaseBuffs = new List<string> { "exhausted" },
                    PhaseDescription = "Boss 进入虚弱状态，防御增强但攻击减弱",
                    Duration = 20
                });
            }
            
            if (phaseCount >= 4) {
                // 第四阶段 - 变身
                phases.Add(new BattlePhase {
                    PhaseNumber = 4,
                    Type = BattlePhaseType.Transformation,
                    PhaseName = "变身阶段",
                    HealthThreshold = 0.2f,
                    DamageMultiplier = 2.0f,
                    DefenseMultiplier = 0.6f,
                    SpeedMultiplier = 1.5f,
                    UnlockedSkills = new List<string> { "meteor", "curse", "summon_minions", "doomsday" },
                    PhaseBuffs = new List<string> { "transformation" },
                    PhaseDescription = "Boss 变身为更强形态",
                    Duration = 0
                });
            }
            
            if (phaseCount >= 5) {
                // 第五阶段 - 特殊
                phases.Add(new BattlePhase {
                    PhaseNumber = 5,
                    Type = BattlePhaseType.Special,
                    PhaseName = "终极阶段",
                    HealthThreshold = 0.1f,
                    DamageMultiplier = 2.5f,
                    DefenseMultiplier = 1.0f,
                    SpeedMultiplier = 1.8f,
                    UnlockedSkills = new List<string> { "doomsday", "meteor", "summon_minions", "power_up" },
                    PhaseBuffs = new List<string> { "ultimate" },
                    PhaseDescription = "Boss 释放全部力量",
                    Duration = 0
                });
            }
            
            BossPhases[bossId] = phases;
        }
        
        private void InitializeLootTables() {
            // 哥布林首领掉落
            BossLootTables["goblin_king"] = new List<BossLoot> {
                new BossLoot { LootId = "goblin_sword", Type = LootType.Equipment, ItemId = "goblin_sword", ItemName = "哥布林之剑", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.15f, IsGuaranteed = false },
                new BossLoot { LootId = "gold_coins", Type = LootType.Currency, ItemId = "gold", ItemName = "金币", MinQuantity = 10, MaxQuantity = 50, DropRate = 1.0f, IsGuaranteed = true },
                new BossLoot { LootId = "goblin_ears", Type = LootType.Material, ItemId = "goblin_ear", ItemName = "哥布林耳朵", MinQuantity = 1, MaxQuantity = 3, DropRate = 0.5f, IsGuaranteed = true }
            };
            
            // 森林巨魔掉落
            BossLootTables["forest_troll"] = new List<BossLoot> {
                new BossLoot { LootId = "troll_hammer", Type = LootType.Equipment, ItemId = "troll_hammer", ItemName = "巨魔战锤", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.1f, IsGuaranteed = false },
                new BossLoot { LootId = "troll_hide", Type = LootType.Material, ItemId = "troll_hide", ItemName = "巨魔皮", MinQuantity = 2, MaxQuantity = 5, DropRate = 0.6f, IsGuaranteed = true },
                new BossLoot { LootId = "health_potion", Type = LootType.Consumable, ItemId = "health_potion", ItemName = "生命药水", MinQuantity = 3, MaxQuantity = 10, DropRate = 0.8f, IsGuaranteed = true },
                new BossLoot { LootId = "rare_gem", Type = LootType.Material, ItemId = "ruby", ItemName = "红宝石", MinQuantity = 1, MaxQuantity = 2, DropRate = 0.2f, IsGuaranteed = false }
            };
            
            // 炎魔掉落
            BossLootTables["fire_demon"] = new List<BossLoot> {
                new BossLoot { LootId = "fire_sword", Type = LootType.Equipment, ItemId = "fire_sword", ItemName = "烈焰之剑", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.08f, IsGuaranteed = false },
                new BossLoot { LootId = "fire_essence", Type = LootType.Material, ItemId = "fire_essence", ItemName = "火焰精华", MinQuantity = 1, MaxQuantity = 3, DropRate = 0.5f, IsGuaranteed = true },
                new BossLoot { LootId = "infernal_orb", Type = LootType.Material, ItemId = "infernal_orb", ItemName = "炼狱宝珠", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.15f, IsGuaranteed = false },
                new BossLoot { LootId = "fire_skill_book", Type = LootType.SkillBook, ItemId = "fire_breath", ItemName = "火焰吐息技能书", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.05f, IsGuaranteed = false }
            };
            
            // 冰霜巨龙掉落
            BossLootTables["frost_drake"] = new List<BossLoot> {
                new BossLoot { LootId = "frost_armor", Type = LootType.Equipment, ItemId = "frost_armor", ItemName = "冰霜护甲", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.1f, IsGuaranteed = false },
                new BossLoot { LootId = "dragon_scale", Type = LootType.Material, ItemId = "dragon_scale", ItemName = "龙鳞", MinQuantity = 3, MaxQuantity = 8, DropRate = 0.7f, IsGuaranteed = true },
                new BossLoot { LootId = "ice_crystal", Type = LootType.Material, ItemId = "ice_crystal", ItemName = "冰晶", MinQuantity = 2, MaxQuantity = 5, DropRate = 0.5f, IsGuaranteed = true },
                new BossLoot { LootId = "drake_egg", Type = LootType.Pet, ItemId = "frost_drake_egg", ItemName = "冰霜龙蛋", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.03f, IsGuaranteed = false }
            };
            
            // 泰坦巨神掉落
            BossLootTables["titan"] = new List<BossLoot> {
                new BossLoot { LootId = "titan_weapon", Type = LootType.Equipment, ItemId = "titan_gauntlet", ItemName = "泰坦护手", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.05f, IsGuaranteed = false },
                new BossLoot { LootId = "titan_heart", Type = LootType.Material, ItemId = "titan_heart", ItemName = "泰坦之心", MinQuantity = 1, MaxQuantity = 2, DropRate = 0.3f, IsGuaranteed = true },
                new BossLoot { LootId = "divine_orb", Type = LootType.Material, ItemId = "divine_orb", ItemName = "神圣宝珠", MinQuantity = 1, MaxQuantity = 3, DropRate = 0.2f, IsGuaranteed = true },
                new BossLoot { LootId = "legendary_gem", Type = LootType.Material, ItemId = "diamond", ItemName = "钻石", MinQuantity = 1, MaxQuantity = 5, DropRate = 0.4f, IsGuaranteed = true },
                new BossLoot { LootId = "titan_mount", Type = LootType.Mount, ItemId = "titan_golem", ItemName = "泰坦傀儡", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.02f, IsGuaranteed = false }
            };
            
            // 暗影君王掉落
            BossLootTables["shadow_lord"] = new List<BossLoot> {
                new BossLoot { LootId = "shadow_blade", Type = LootType.Equipment, ItemId = "shadow_blade", ItemName = "暗影之刃", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.08f, IsGuaranteed = false },
                new BossLoot { LootId = "shadow_crown", Type = LootType.Equipment, ItemId = "shadow_crown", ItemName = "暗影王冠", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.05f, IsGuaranteed = false },
                new BossLoot { LootId = "shadow_essence", Type = LootType.Material, ItemId = "shadow_essence", ItemName = "暗影精华", MinQuantity = 2, MaxQuantity = 6, DropRate = 0.6f, IsGuaranteed = true },
                new BossLoot { LootId = "dark_skill_book", Type = LootType.SkillBook, ItemId = "shadow_strike", ItemName = "暗影打击技能书", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.1f, IsGuaranteed = false },
                new BossLoot { LootId = "legendary_ring", Type = LootType.Equipment, ItemId = "ring_of_shadows", ItemName = "暗影之戒", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.03f, IsGuaranteed = false }
            };
        }
        
        // 获取 Boss 配置
        public BossState GetBossConfig(string bossId) {
            if (BossConfigs.ContainsKey(bossId)) {
                var boss = BossConfigs[bossId];
                return new BossState {
                    BossId = boss.BossId,
                    BossName = boss.BossName,
                    Type = boss.Type,
                    Element = boss.Element,
                    Level = boss.Level,
                    MaxHealth = boss.MaxHealth,
                    CurrentHealth = boss.MaxHealth,
                    AttackDamage = boss.AttackDamage,
                    Defense = boss.Defense,
                    MoveSpeed = boss.MoveSpeed,
                    CurrentPhase = 1,
                    IsEnraged = false,
                    EnrageTimer = boss.EnrageTimer
                };
            }
            return null;
        }
        
        // 获取 Boss 战斗阶段
        public List<BattlePhase> GetBossPhases(string bossId) {
            if (BossPhases.ContainsKey(bossId)) {
                return BossPhases[bossId];
            }
            return new List<BattlePhase>();
        }
        
        // 获取 Boss 掉落表
        public List<BossLoot> GetBossLootTable(string bossId) {
            if (BossLootTables.ContainsKey(bossId)) {
                return BossLootTables[bossId];
            }
            return new List<BossLoot>();
        }
        
        // 获取技能
        public BossSkill GetSkill(string skillId) {
            if (SkillDatabase.ContainsKey(skillId)) {
                return SkillDatabase[skillId];
            }
            return null;
        }
        
        // 获取所有 Boss ID
        public List<string> GetAllBossIds() {
            return new List<string>(BossConfigs.Keys);
        }
    }
}
