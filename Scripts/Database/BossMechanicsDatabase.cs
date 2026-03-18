using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// Boss 机制数据库 - 存储所有 Boss 配置数据
    /// </summary>
    public class BossMechanicsDatabase : DatabaseBase
    {
        /// <summary>
        /// 静态实例引用（兼容原有访问模式）
        /// </summary>
        public static BossMechanicsDatabase Instance { get; private set; }

        public override object Instance => Instance;

        /// <summary>
        /// Boss 配置字典
        /// </summary>
        private Dictionary<string, BossConfigData> _bossConfigs = new Dictionary<string, BossConfigData>();

        /// <summary>
        /// Boss 技能配置字典
        /// </summary>
        private Dictionary<string, BossSkillData> _skillConfigs = new Dictionary<string, BossSkillData>();

        /// <summary>
        /// Boss 掉落表字典
        /// </summary>
        private Dictionary<string, List<BossLootData>> _lootTables = new Dictionary<string, List<BossLootData>>();

        /// <summary>
        /// Boss AI 行为配置
        /// </summary>
        private Dictionary<string, BossAIConfigData> _aiConfigs = new Dictionary<string, BossAIConfigData>();

        public BossMechanicsDatabase()
        {
            Instance = this;
            Initialize();
        }

        public override void Initialize()
        {
            InitializeSkills();
            InitializeAIConfigs();
            InitializeBossConfigs();
            InitializeLootTables();
        }

        public override bool ValidateData() => _bossConfigs.Count > 0;

        #region 初始化方法

        private void InitializeSkills()
        {
            // ========== 近战技能 ==========
            _skillConfigs["melee_slash"] = new BossSkillData
            {
                SkillId = "melee_slash",
                SkillName = "致命斩击",
                Description = "Boss挥动武器进行一次强力斩击",
                SkillType = BossSkillType.Melee,
                Damage = 150f,
                Range = 3f,
                Cooldown = 3f,
                CastTime = 0.5f,
                PhaseUnlock = 1
            };

            _skillConfigs["melee_heavy_strike"] = new BossSkillData
            {
                SkillId = "melee_heavy_strike",
                SkillName = "重击",
                Description = "强力打击，造成眩晕",
                SkillType = BossSkillType.Melee,
                Damage = 200f,
                Range = 3.5f,
                Cooldown = 5f,
                CastTime = 1f,
                StunDuration = 2f,
                PhaseUnlock = 1
            };

            _skillConfigs["spin_attack"] = new BossSkillData
            {
                SkillId = "spin_attack",
                SkillName = "旋转攻击",
                Description = "Boss原地旋转攻击周围所有目标",
                SkillType = BossSkillType.SpinAttack,
                Damage = 180f,
                AreaRadius = 5f,
                Cooldown = 8f,
                CastTime = 1f,
                Duration = 2f,
                PhaseUnlock = 2
            };

            _skillConfigs["charge"] = new BossSkillData
            {
                SkillId = "charge",
                SkillName = "冲锋",
                Description = "Boss向目标冲锋",
                SkillType = BossSkillType.Charge,
                Damage = 150f,
                Range = 15f,
                Cooldown = 10f,
                CastTime = 0.8f,
                KnockbackForce = 10f,
                PhaseUnlock = 2
            };

            // ========== 远程技能 ==========
            _skillConfigs["projectile_fireball"] = new BossSkillData
            {
                SkillId = "projectile_fireball",
                SkillName = "火球术",
                Description = "发射火球攻击目标区域",
                SkillType = BossSkillType.Projectile,
                Damage = 120f,
                Range = 20f,
                AreaRadius = 4f,
                Cooldown = 4f,
                CastTime = 0.8f,
                PhaseUnlock = 1
            };

            _skillConfigs["projectile_lightning"] = new BossSkillData
            {
                SkillId = "projectile_lightning",
                SkillName = "闪电箭",
                Description = "快速闪电攻击",
                SkillType = BossSkillType.Projectile,
                Damage = 100f,
                Range = 25f,
                AreaRadius = 2f,
                Cooldown = 2.5f,
                CastTime = 0.6f,
                PhaseUnlock = 1
            };

            _skillConfigs["aoe_fire_breath"] = new BossSkillData
            {
                SkillId = "aoe_fire_breath",
                SkillName = "火焰吐息",
                Description = "Boss喷出火焰锥形区域",
                SkillType = BossSkillType.AOE,
                Damage = 250f,
                AreaRadius = 8f,
                Range = 12f,
                Cooldown = 12f,
                CastTime = 1.5f,
                Duration = 3f,
                PhaseUnlock = 2
            };

            _skillConfigs["aoe_earthquake"] = new BossSkillData
            {
                SkillId = "aoe_earthquake",
                SkillName = "地震",
                Description = "引发地震，对范围内所有敌人造成伤害",
                SkillType = BossSkillType.AOE,
                Damage = 300f,
                AreaRadius = 10f,
                Cooldown = 15f,
                CastTime = 2f,
                KnockbackForce = 8f,
                PhaseUnlock = 3
            };

            _skillConfigs["aoe_meteor"] = new BossSkillData
            {
                SkillId = "aoe_meteor",
                SkillName = "陨石",
                Description = "召唤陨石砸向目标区域",
                SkillType = BossSkillType.AOE,
                Damage = 400f,
                AreaRadius = 6f,
                Range = 20f,
                Cooldown = 25f,
                CastTime = 3f,
                StunDuration = 2f,
                PhaseUnlock = 3
            };

            // ========== DoT 技能 ==========
            _skillConfigs["dot_poison_cloud"] = new BossSkillData
            {
                SkillId = "dot_poison_cloud",
                SkillName = "毒云",
                Description = "释放毒云，持续伤害敌人",
                SkillType = BossSkillType.DoT,
                Damage = 80f,
                Range = 12f,
                AreaRadius = 5f,
                Cooldown = 18f,
                CastTime = 1.5f,
                Duration = 8f,
                PhaseUnlock = 2
            };

            _skillConfigs["dot_curse"] = new BossSkillData
            {
                SkillId = "dot_curse",
                SkillName = "诅咒",
                Description = "诅咒敌人，降低防御并持续造成伤害",
                SkillType = BossSkillType.DoT,
                Damage = 50f,
                Range = 15f,
                Cooldown = 20f,
                CastTime = 1.2f,
                Duration = 10f,
                DefenseReduction = 0.3f,
                PhaseUnlock = 2
            };

            // ========== 治疗/辅助技能 ==========
            _skillConfigs["heal_self"] = new BossSkillData
            {
                SkillId = "heal_self",
                SkillName = "自我治疗",
                Description = "恢复自身生命值",
                SkillType = BossSkillType.Heal,
                HealAmount = 300f,
                Cooldown = 30f,
                CastTime = 1.5f,
                PhaseUnlock = 1
            };

            _skillConfigs["buff_power_up"] = new BossSkillData
            {
                SkillId = "buff_power_up",
                SkillName = "强化",
                Description = "提升自身攻击力",
                SkillType = BossSkillType.Buff,
                DamageMultiplier = 1.5f,
                Cooldown = 25f,
                CastTime = 0.5f,
                Duration = 15f,
                PhaseUnlock = 1
            };

            _skillConfigs["shield"] = new BossSkillData
            {
                SkillId = "shield",
                SkillName = "护盾",
                Description = "Boss获得护盾吸收伤害",
                SkillType = BossSkillType.Shield,
                ShieldAmount = 500f,
                Cooldown = 30f,
                CastTime = 0.5f,
                PhaseUnlock = 2
            };

            // ========== 召唤技能 ==========
            _skillConfigs["summon_minions"] = new BossSkillData
            {
                SkillId = "summon_minions",
                SkillName = "召唤仆从",
                Description = "Boss召唤小怪协助战斗",
                SkillType = BossSkillType.Summon,
                SummonCount = 4,
                Cooldown = 35f,
                CastTime = 2f,
                PhaseUnlock = 2
            };

            _skillConfigs["summon_elemental"] = new BossSkillData
            {
                SkillId = "summon_elemental",
                SkillName = "召唤元素",
                Description = "Boss召唤强大的元素生物",
                SkillType = BossSkillType.Summon,
                SummonCount = 1,
                Cooldown = 60f,
                CastTime = 3f,
                PhaseUnlock = 3
            };

            // ========== 终极技能 ==========
            _skillConfigs["ultimate_doomsday"] = new BossSkillData
            {
                SkillId = "ultimate_doomsday",
                SkillName = "末日",
                Description = "毁灭性终极技能",
                SkillType = BossSkillType.Ultimate,
                Damage = 500f,
                Range = 30f,
                AreaRadius = 15f,
                Cooldown = 120f,
                CastTime = 4f,
                StunDuration = 3f,
                PhaseUnlock = 3
            };

            _skillConfigs["ultimate_laser"] = new BossSkillData
            {
                SkillId = "ultimate_laser",
                SkillName = "激光射线",
                Description = "发射致命的激光束",
                SkillType = BossSkillType.Ultimate,
                Damage = 600f,
                Range = 30f,
                AreaRadius = 3f,
                Cooldown = 90f,
                CastTime = 3f,
                StunDuration = 2f,
                PhaseUnlock = 3
            };

            // ========== 控制技能 ==========
            _skillConfigs["control_stun"] = new BossSkillData
            {
                SkillId = "control_stun",
                SkillName = "震晕打击",
                Description = "重击并眩晕目标",
                SkillType = BossSkillType.Stun,
                Damage = 150f,
                Range = 4f,
                Cooldown = 15f,
                CastTime = 0.8f,
                StunDuration = 3f,
                KnockbackForce = 8f,
                PhaseUnlock = 1
            };

            _skillConfigs["teleport"] = new BossSkillData
            {
                SkillId = "teleport",
                SkillName = "传送",
                Description = "Boss传送到随机位置",
                SkillType = BossSkillType.Teleport,
                Cooldown = 20f,
                CastTime = 0.3f,
                PhaseUnlock = 1
            };
        }

        private void InitializeAIConfigs()
        {
            _aiConfigs["aggressive"] = new BossAIConfigData
            {
                Behavior = "Aggressive",
                AggressionLevel = 0.9f,
                DefensiveThreshold = 0.1f,
                SkillUsageRate = 0.8f,
                UltimateThreshold = 0.3f,
                RetreatThreshold = 0.05f
            };

            _aiConfigs["defensive"] = new BossAIConfigData
            {
                Behavior = "Defensive",
                AggressionLevel = 0.4f,
                DefensiveThreshold = 0.7f,
                SkillUsageRate = 0.6f,
                UltimateThreshold = 0.4f,
                RetreatThreshold = 0.3f
            };

            _aiConfigs["balanced"] = new BossAIConfigData
            {
                Behavior = "Balanced",
                AggressionLevel = 0.6f,
                DefensiveThreshold = 0.4f,
                SkillUsageRate = 0.7f,
                UltimateThreshold = 0.35f,
                RetreatThreshold = 0.2f
            };

            _aiConfigs["tactical"] = new BossAIConfigData
            {
                Behavior = "Tactical",
                AggressionLevel = 0.5f,
                DefensiveThreshold = 0.5f,
                SkillUsageRate = 0.9f,
                UltimateThreshold = 0.25f,
                RetreatThreshold = 0.15f
            };

            _aiConfigs["berserker"] = new BossAIConfigData
            {
                Behavior = "Berserker",
                AggressionLevel = 1.0f,
                DefensiveThreshold = 0.05f,
                SkillUsageRate = 1.0f,
                UltimateThreshold = 0.5f,
                RetreatThreshold = 0.0f
            };
        }

        private void InitializeBossConfigs()
        {
            // 教程 Boss - 哥布林首领
            var goblinKing = new BossConfigData
            {
                BossId = "goblin_king",
                BossName = "哥布林首领",
                Description = "哥布林部落的统治者",
                Type = BossType.Tutorial,
                Difficulty = Difficulty.Easy,
                MaxHealth = 500f,
                AttackDamage = 25f,
                Defense = 5f,
                MoveSpeed = 4.0f,
                Level = 1,
                PhaseCount = 2,
                EnrageThreshold = 0.3f,
                GoldReward = 100f,
                ExpReward = 50f,
                Skills = new List<string> { "melee_slash", "melee_heavy_strike", "control_stun" }
            };
            _bossConfigs["goblin_king"] = goblinKing;
            _dataStore["goblin_king"] = goblinKing;

            // 普通 Boss - 森林巨魔
            var forestTroll = new BossConfigData
            {
                BossId = "forest_troll",
                BossName = "森林巨魔",
                Description = "栖息在森林深处的巨大生物",
                Type = BossType.Normal,
                Difficulty = Difficulty.Normal,
                MaxHealth = 5000f,
                AttackDamage = 150f,
                Defense = 50f,
                MoveSpeed = 3.5f,
                Level = 10,
                PhaseCount = 3,
                EnrageThreshold = 0.3f,
                GoldReward = 500f,
                ExpReward = 300f,
                Skills = new List<string> { "melee_slash", "melee_heavy_strike", "spin_attack", "aoe_earthquake", "heal_self" }
            };
            _bossConfigs["forest_troll"] = forestTroll;
            _dataStore["forest_troll"] = forestTroll;

            // 精英 Boss - 炎魔领主
            var fireLord = new BossConfigData
            {
                BossId = "fire_lord",
                BossName = "炎魔领主",
                Description = "来自深渊的火焰恶魔",
                Type = BossType.Elite,
                Difficulty = Difficulty.Hard,
                MaxHealth = 15000f,
                AttackDamage = 300f,
                Defense = 100f,
                MoveSpeed = 4.0f,
                Level = 25,
                PhaseCount = 3,
                EnrageThreshold = 0.25f,
                GoldReward = 2000f,
                ExpReward = 1500f,
                Skills = new List<string> { "melee_slash", "charge", "projectile_fireball", "aoe_fire_breath", "dot_poison_cloud", "summon_minions", "ultimate_doomsday" }
            };
            _bossConfigs["fire_lord"] = fireLord;
            _dataStore["fire_lord"] = fireLord;

            // 小 Boss - 冰霜巨龙
            var frostDrake = new BossConfigData
            {
                BossId = "frost_drake",
                BossName = "冰霜巨龙",
                Description = "古老的冰霜巨龙",
                Type = BossType.MiniBoss,
                Difficulty = Difficulty.Hard,
                MaxHealth = 8000f,
                AttackDamage = 200f,
                Defense = 80f,
                MoveSpeed = 5.0f,
                Level = 20,
                PhaseCount = 3,
                EnrageThreshold = 0.25f,
                GoldReward = 1000f,
                ExpReward = 800f,
                Skills = new List<string> { "melee_slash", "projectile_lightning", "aoe_fire_breath", "teleport", "control_stun", "heal_self", "buff_power_up" }
            };
            _bossConfigs["frost_drake"] = frostDrake;
            _dataStore["frost_drake"] = frostDrake;

            // 世界 Boss - 泰坦巨神
            var titan = new BossConfigData
            {
                BossId = "titan",
                BossName = "泰坦巨神",
                Description = "远古的泰坦巨人",
                Type = BossType.World,
                Difficulty = Difficulty.Nightmare,
                MaxHealth = 100000f,
                AttackDamage = 500f,
                Defense = 200f,
                MoveSpeed = 2.0f,
                Level = 50,
                PhaseCount = 4,
                EnrageThreshold = 0.2f,
                GoldReward = 10000f,
                ExpReward = 8000f,
                Skills = new List<string> { "melee_slash", "spin_attack", "charge", "aoe_earthquake", "aoe_meteor", "summon_elemental", "shield", "ultimate_doomsday" }
            };
            _bossConfigs["titan"] = titan;
            _dataStore["titan"] = titan;

            // 副本 Boss - 暗影君王
            var shadowLord = new BossConfigData
            {
                BossId = "shadow_lord",
                BossName = "暗影君王",
                Description = "穿梭于虚实之间的暗影王者",
                Type = BossType.Raid,
                Difficulty = Difficulty.Legendary,
                MaxHealth = 50000f,
                AttackDamage = 400f,
                Defense = 150f,
                MoveSpeed = 3.0f,
                Level = 40,
                PhaseCount = 4,
                EnrageThreshold = 0.15f,
                GoldReward = 5000f,
                ExpReward = 4000f,
                Skills = new List<string> { "melee_slash", "melee_heavy_strike", "projectile_fireball", "dot_curse", "summon_minions", "shield", "teleport", "ultimate_doomsday", "ultimate_laser" }
            };
            _bossConfigs["shadow_lord"] = shadowLord;
            _dataStore["shadow_lord"] = shadowLord;

            // 深渊 Boss - 深渊恶魔
            var abyssDemon = new BossConfigData
            {
                BossId = "abyss_demon",
                BossName = "深渊恶魔",
                Description = "来自深渊的终极恶魔",
                Type = BossType.Raid,
                Difficulty = Difficulty.Legendary,
                MaxHealth = 200000f,
                AttackDamage = 600f,
                Defense = 250f,
                MoveSpeed = 3.5f,
                Level = 60,
                PhaseCount = 5,
                EnrageThreshold = 0.1f,
                GoldReward = 20000f,
                ExpReward = 15000f,
                Skills = new List<string> { "melee_slash", "spin_attack", "charge", "aoe_earthquake", "aoe_meteor", "dot_poison_cloud", "summon_elemental", "heal_self", "ultimate_doomsday", "ultimate_laser" }
            };
            _bossConfigs["abyss_demon"] = abyssDemon;
            _dataStore["abyss_demon"] = abyssDemon;
        }

        private void InitializeLootTables()
        {
            // 哥布林首领掉落
            _lootTables["goblin_king"] = new List<BossLootData>
            {
                new BossLootData { ItemId = "goblin_sword", ItemName = "哥布林之剑", DropRate = 0.15f, MinQuantity = 1, MaxQuantity = 1, LootType = LootType.Equipment },
                new BossLootData { ItemId = "gold", ItemName = "金币", DropRate = 1.0f, MinQuantity = 10, MaxQuantity = 50, LootType = LootType.Currency, IsGuaranteed = true },
                new BossLootData { ItemId = "goblin_ear", ItemName = "哥布林耳朵", DropRate = 0.5f, MinQuantity = 1, MaxQuantity = 3, LootType = LootType.Material, IsGuaranteed = true }
            };

            // 森林巨魔掉落
            _lootTables["forest_troll"] = new List<BossLootData>
            {
                new BossLootData { ItemId = "troll_hammer", ItemName = "巨魔战锤", DropRate = 0.1f, MinQuantity = 1, MaxQuantity = 1, LootType = LootType.Equipment },
                new BossLootData { ItemId = "troll_hide", ItemName = "巨魔皮", DropRate = 0.6f, MinQuantity = 2, MaxQuantity = 5, LootType = LootType.Material, IsGuaranteed = true },
                new BossLootData { ItemId = "health_potion", ItemName = "生命药水", DropRate = 0.8f, MinQuantity = 3, MaxQuantity = 10, LootType = LootType.Consumable, IsGuaranteed = true }
            };

            // 炎魔领主掉落
            _lootTables["fire_lord"] = new List<BossLootData>
            {
                new BossLootData { ItemId = "fire_sword", ItemName = "烈焰之剑", DropRate = 0.08f, MinQuantity = 1, MaxQuantity = 1, LootType = LootType.Equipment },
                new BossLootData { ItemId = "fire_essence", ItemName = "火焰精华", DropRate = 0.5f, MinQuantity = 1, MaxQuantity = 3, LootType = LootType.Material, IsGuaranteed = true },
                new BossLootData { ItemId = "infernal_orb", ItemName = "炼狱宝珠", DropRate = 0.15f, MinQuantity = 1, MaxQuantity = 1, LootType = LootType.Material }
            };

            // 冰霜巨龙掉落
            _lootTables["frost_drake"] = new List<BossLootData>
            {
                new BossLootData { ItemId = "frost_armor", ItemName = "冰霜护甲", DropRate = 0.1f, MinQuantity = 1, MaxQuantity = 1, LootType = LootType.Equipment },
                new BossLootData { ItemId = "dragon_scale", ItemName = "龙鳞", DropRate = 0.7f, MinQuantity = 3, MaxQuantity = 8, LootType = LootType.Material, IsGuaranteed = true },
                new BossLootData { ItemId = "ice_crystal", ItemName = "冰晶", DropRate = 0.5f, MinQuantity = 2, MaxQuantity = 5, LootType = LootType.Material, IsGuaranteed = true }
            };

            // 泰坦巨神掉落
            _lootTables["titan"] = new List<BossLootData>
            {
                new BossLootData { ItemId = "titan_gauntlet", ItemName = "泰坦护手", DropRate = 0.05f, MinQuantity = 1, MaxQuantity = 1, LootType = LootType.Equipment },
                new BossLootData { ItemId = "titan_heart", ItemName = "泰坦之心", DropRate = 0.3f, MinQuantity = 1, MaxQuantity = 2, LootType = LootType.Material, IsGuaranteed = true },
                new BossLootData { ItemId = "divine_orb", ItemName = "神圣宝珠", DropRate = 0.2f, MinQuantity = 1, MaxQuantity = 3, LootType = LootType.Material, IsGuaranteed = true }
            };

            // 暗影君王掉落
            _lootTables["shadow_lord"] = new List<BossLootData>
            {
                new BossLootData { ItemId = "shadow_blade", ItemName = "暗影之刃", DropRate = 0.08f, MinQuantity = 1, MaxQuantity = 1, LootType = LootType.Equipment },
                new BossLootData { ItemId = "shadow_crown", ItemName = "暗影王冠", DropRate = 0.05f, MinQuantity = 1, MaxQuantity = 1, LootType = LootType.Equipment },
                new BossLootData { ItemId = "shadow_essence", ItemName = "暗影精华", DropRate = 0.6f, MinQuantity = 2, MaxQuantity = 6, LootType = LootType.Material, IsGuaranteed = true }
            };

            // 深渊恶魔掉落
            _lootTables["abyss_demon"] = new List<BossLootData>
            {
                new BossLootData { ItemId = "demon_heart", ItemName = "恶魔之心", DropRate = 0.3f, MinQuantity = 1, MaxQuantity = 2, LootType = LootType.Material, IsGuaranteed = true },
                new BossLootData { ItemId = "abyss_artifact", ItemName = "深渊神器", DropRate = 0.05f, MinQuantity = 1, MaxQuantity = 1, LootType = LootType.Equipment },
                new BossLootData { ItemId = "void_shard", ItemName = "虚空碎片", DropRate = 0.4f, MinQuantity = 5, MaxQuantity = 15, LootType = LootType.Material, IsGuaranteed = true }
            };
        }

        #endregion

        #region 公共访问方法

        /// <summary>
        /// 通过 ID 获取 Boss 配置
        /// </summary>
        public BossConfigData GetBossConfig(string bossId)
        {
            return _bossConfigs.ContainsKey(bossId) ? _bossConfigs[bossId] : null;
        }

        /// <summary>
        /// 通过 ID 获取 Boss 配置（IDatabase 规范别名）
        /// </summary>
        public BossConfigData GetBossMechanics(string bossId) => GetBossConfig(bossId);

        /// <summary>
        /// 获取所有 Boss 配置
        /// </summary>
        public List<BossConfigData> GetAllBossConfigs()
        {
            return new List<BossConfigData>(_bossConfigs.Values);
        }

        /// <summary>
        /// 获取所有 Boss 配置（IDatabase 规范别名）
        /// </summary>
        public List<BossConfigData> GetAllBossMechanics() => GetAllBossConfigs();

        /// <summary>
        /// 获取所有 Boss ID
        /// </summary>
        public List<string> GetAllBossIds()
        {
            return new List<string>(_bossConfigs.Keys);
        }

        /// <summary>
        /// 通过 ID 获取技能配置
        /// </summary>
        public BossSkillData GetSkillConfig(string skillId)
        {
            return _skillConfigs.ContainsKey(skillId) ? _skillConfigs[skillId] : null;
        }

        /// <summary>
        /// 获取所有技能配置
        /// </summary>
        public Dictionary<string, BossSkillData> GetAllSkillConfigs()
        {
            return new Dictionary<string, BossSkillData>(_skillConfigs);
        }

        /// <summary>
        /// 通过 Boss ID 获取掉落表
        /// </summary>
        public List<BossLootData> GetLootTable(string bossId)
        {
            return _lootTables.ContainsKey(bossId) ? _lootTables[bossId] : new List<BossLootData>();
        }

        /// <summary>
        /// 获取 AI 配置
        /// </summary>
        public BossAIConfigData GetAIConfig(string behavior)
        {
            return _aiConfigs.ContainsKey(behavior) ? _aiConfigs[behavior] : null;
        }

        /// <summary>
        /// 按类型获取 Boss 列表
        /// </summary>
        public List<BossConfigData> GetBossesByType(BossType type)
        {
            var result = new List<BossConfigData>();
            foreach (var boss in _bossConfigs.Values)
            {
                if (boss.Type == type)
                    result.Add(boss);
            }
            return result;
        }

        /// <summary>
        /// 按难度获取 Boss 列表
        /// </summary>
        public List<BossConfigData> GetBossesByDifficulty(Difficulty difficulty)
        {
            var result = new List<BossConfigData>();
            foreach (var boss in _bossConfigs.Values)
            {
                if (boss.Difficulty == difficulty)
                    result.Add(boss);
            }
            return result;
        }

        #endregion

        #region IDatabase 覆盖实现

        public override T GetData<T>(string key)
        {
            if (_bossConfigs.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return null;
        }

        public override int GetDataCount() => _bossConfigs.Count;

        public override IEnumerable<string> GetAllKeys() => _bossConfigs.Keys;

        #endregion
    }

    // ==================== 数据类型定义 ====================

    /// <summary>
    /// Boss 类型
    /// </summary>
    public enum BossType
    {
        Tutorial,
        Normal,
        MiniBoss,
        Elite,
        World,
        Raid
    }

    /// <summary>
    /// Boss 难度
    /// </summary>
    public enum Difficulty
    {
        Easy,
        Normal,
        Hard,
        Nightmare,
        Legendary
    }

    /// <summary>
    /// Boss 技能类型
    /// </summary>
    public enum BossSkillType
    {
        Melee,
        SpinAttack,
        Charge,
        Projectile,
        AOE,
        DoT,
        Heal,
        Buff,
        Shield,
        Summon,
        Stun,
        Teleport,
        Ultimate
    }

    /// <summary>
    /// 掉落物类型
    /// </summary>
    public enum LootType
    {
        Equipment,
        Material,
        Currency,
        Consumable,
        SkillBook,
        Mount,
        Pet
    }

    /// <summary>
    /// Boss 配置数据
    /// </summary>
    public class BossConfigData
    {
        public string BossId { get; set; }
        public string BossName { get; set; }
        public string Description { get; set; }
        public BossType Type { get; set; }
        public Difficulty Difficulty { get; set; }
        public float MaxHealth { get; set; }
        public float AttackDamage { get; set; }
        public float Defense { get; set; }
        public float MoveSpeed { get; set; }
        public int Level { get; set; }
        public int PhaseCount { get; set; }
        public float EnrageThreshold { get; set; }
        public float GoldReward { get; set; }
        public float ExpReward { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
    }

    /// <summary>
    /// Boss 技能配置数据
    /// </summary>
    public class BossSkillData
    {
        public string SkillId { get; set; }
        public string SkillName { get; set; }
        public string Description { get; set; }
        public BossSkillType SkillType { get; set; }
        public float Damage { get; set; }
        public float Range { get; set; }
        public float AreaRadius { get; set; }
        public float Cooldown { get; set; }
        public float CastTime { get; set; }
        public float Duration { get; set; }
        public float HealAmount { get; set; }
        public float ShieldAmount { get; set; }
        public float DamageMultiplier { get; set; }
        public float KnockbackForce { get; set; }
        public float StunDuration { get; set; }
        public float DefenseReduction { get; set; }
        public int SummonCount { get; set; }
        public int PhaseUnlock { get; set; }
    }

    /// <summary>
    /// Boss AI 配置数据
    /// </summary>
    public class BossAIConfigData
    {
        public string Behavior { get; set; }
        public float AggressionLevel { get; set; }
        public float DefensiveThreshold { get; set; }
        public float SkillUsageRate { get; set; }
        public float UltimateThreshold { get; set; }
        public float RetreatThreshold { get; set; }
    }

    /// <summary>
    /// Boss 掉落数据
    /// </summary>
    public class BossLootData
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public float DropRate { get; set; }
        public int MinQuantity { get; set; }
        public int MaxQuantity { get; set; }
        public LootType LootType { get; set; }
        public bool IsGuaranteed { get; set; }
    }
}
