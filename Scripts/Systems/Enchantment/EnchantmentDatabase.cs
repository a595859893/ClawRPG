using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems.Enchantment
{
    /// <summary>
    /// 附魔配置数据库
    /// </summary>
    public class EnchantmentDatabase : DatabaseBase
    {
        private static EnchantmentDatabase _instance;
        public static EnchantmentDatabase Instance => _instance ??= new EnchantmentDatabase();
        public override object Instance => _instance ??= new EnchantmentDatabase();

        // 附魔记录缓存
        private Dictionary<string, EnchantmentRecord> _enchantments;
        private Dictionary<EnchantmentTier, List<string>> _tierCache;
        private Dictionary<EnchantmentType, List<string>> _typeCache;

        /// <summary>
        /// 初始化附魔数据库
        /// </summary>
        public override void Initialize()
        {
            _enchantments = new Dictionary<string, EnchantmentRecord>();
            _tierCache = new Dictionary<EnchantmentTier, List<string>>();
            _typeCache = new Dictionary<EnchantmentType, List<string>>();

            InitializeTierCache();
            InitializeTypeCache();
            LoadDefaultEnchantments();
        }

        public bool ValidateData()
        {
            return _enchantments != null && _enchantments.Count > 0;
        }

        private void InitializeTierCache()
        {
            _tierCache[EnchantmentTier.Common] = new List<string>();
            _tierCache[EnchantmentTier.Uncommon] = new List<string>();
            _tierCache[EnchantmentTier.Rare] = new List<string>();
            _tierCache[EnchantmentTier.Epic] = new List<string>();
            _tierCache[EnchantmentTier.Legendary] = new List<string>();
        }

        private void InitializeTypeCache()
        {
            _typeCache[EnchantmentType.Weapon] = new List<string>();
            _typeCache[EnchantmentType.Armor] = new List<string>();
            _typeCache[EnchantmentType.Accessory] = new List<string>();
            _typeCache[EnchantmentType.Universal] = new List<string>();
        }

        /// <summary>
        /// 加载默认附魔配置
        /// </summary>
        private void LoadDefaultEnchantments()
        {
            // 武器附魔 - 普通
            AddEnchantment(new EnchantmentRecord
            {
                Id = "weapon_sharp_1",
                Name = "锋利 I",
                Type = EnchantmentType.Weapon,
                PrimaryEffect = EnchantmentEffect.Damage,
                PrimaryEffectValue = 5f,
                Tier = EnchantmentTier.Common,
                RequiredLevel = 1,
                EnchantmentCost = 100,
                SuccessRate = 90f,
                Description = "增加5点攻击力",
                IconName = "sword"
            });

            AddEnchantment(new EnchantmentRecord
            {
                Id = "weapon_sharp_2",
                Name = "锋利 II",
                Type = EnchantmentType.Weapon,
                PrimaryEffect = EnchantmentEffect.Damage,
                PrimaryEffectValue = 10f,
                Tier = EnchantmentTier.Uncommon,
                RequiredLevel = 10,
                EnchantmentCost = 250,
                SuccessRate = 80f,
                Description = "增加10点攻击力",
                IconName = "sword"
            });

            AddEnchantment(new EnchantmentRecord
            {
                Id = "weapon_sharp_3",
                Name = "锋利 III",
                Type = EnchantmentType.Weapon,
                PrimaryEffect = EnchantmentEffect.Damage,
                PrimaryEffectValue = 18f,
                Tier = EnchantmentTier.Rare,
                RequiredLevel = 25,
                EnchantmentCost = 500,
                SuccessRate = 70f,
                Description = "增加18点攻击力",
                IconName = "sword"
            });

            // 武器附魔 - 暴击
            AddEnchantment(new EnchantmentRecord
            {
                Id = "weapon_crit_1",
                Name = "致命 I",
                Type = EnchantmentType.Weapon,
                PrimaryEffect = EnchantmentEffect.CriticalRate,
                PrimaryEffectValue = 3f,
                Tier = EnchantmentTier.Common,
                RequiredLevel = 5,
                EnchantmentCost = 150,
                SuccessRate = 85f,
                Description = "增加3%暴击率",
                IconName = "skull"
            });

            AddEnchantment(new EnchantmentRecord
            {
                Id = "weapon_crit_2",
                Name = "致命 II",
                Type = EnchantmentType.Weapon,
                PrimaryEffect = EnchantmentEffect.CriticalRate,
                PrimaryEffectValue = 6f,
                Tier = EnchantmentTier.Uncommon,
                RequiredLevel = 15,
                EnchantmentCost = 350,
                SuccessRate = 75f,
                Description = "增加6%暴击率",
                IconName = "skull"
            });

            AddEnchantment(new EnchantmentRecord
            {
                Id = "weapon_crit_3",
                Name = "致命 III",
                Type = EnchantmentType.Weapon,
                PrimaryEffect = EnchantmentEffect.CriticalRate,
                PrimaryEffectValue = 10f,
                Tier = EnchantmentTier.Rare,
                RequiredLevel = 30,
                EnchantmentCost = 700,
                SuccessRate = 65f,
                Description = "增加10%暴击率",
                IconName = "skull"
            });

            // 武器附魔 - 暴伤
            AddEnchantment(new EnchantmentRecord
            {
                Id = "weapon_critdmg_1",
                Name = "强击 I",
                Type = EnchantmentType.Weapon,
                PrimaryEffect = EnchantmentEffect.CriticalDamage,
                PrimaryEffectValue = 15f,
                Tier = EnchantmentTier.Common,
                RequiredLevel = 8,
                EnchantmentCost = 200,
                SuccessRate = 85f,
                Description = "增加15%暴击伤害",
                IconName = "explosion"
            });

            AddEnchantment(new EnchantmentRecord
            {
                Id = "weapon_critdmg_2",
                Name = "强击 II",
                Type = EnchantmentType.Weapon,
                PrimaryEffect = EnchantmentEffect.CriticalDamage,
                PrimaryEffectValue = 30f,
                Tier = EnchantmentTier.Uncommon,
                RequiredLevel = 20,
                EnchantmentCost = 450,
                SuccessRate = 75f,
                Description = "增加30%暴击伤害",
                IconName = "explosion"
            });

            // 武器附魔 - 攻击速度
            AddEnchantment(new EnchantmentRecord
            {
                Id = "weapon_atkspd_1",
                Name = "急速 I",
                Type = EnchantmentType.Weapon,
                PrimaryEffect = EnchantmentEffect.AttackSpeed,
                PrimaryEffectValue = 5f,
                Tier = EnchantmentTier.Common,
                RequiredLevel = 12,
                EnchantmentCost = 250,
                SuccessRate = 80f,
                Description = "增加5%攻击速度",
                IconName = "lightning"
            });

            // 武器附魔 - 生命偷取
            AddEnchantment(new EnchantmentRecord
            {
                Id = "weapon_lifesteal_1",
                Name = "吸血 I",
                Type = EnchantmentType.Weapon,
                PrimaryEffect = EnchantmentEffect.LifeSteal,
                PrimaryEffectValue = 3f,
                Tier = EnchantmentTier.Rare,
                RequiredLevel = 25,
                EnchantmentCost = 800,
                SuccessRate = 60f,
                Description = "3%生命偷取",
                IconName = "drop"
            });

            AddEnchantment(new EnchantmentRecord
            {
                Id = "weapon_lifesteal_2",
                Name = "吸血 II",
                Type = EnchantmentType.Weapon,
                PrimaryEffect = EnchantmentEffect.LifeSteal,
                PrimaryEffectValue = 5f,
                Tier = EnchantmentTier.Epic,
                RequiredLevel = 40,
                EnchantmentCost = 1500,
                SuccessRate = 50f,
                Description = "5%生命偷取",
                IconName = "drop"
            });

            // 武器附魔 - 传奇
            AddEnchantment(new EnchantmentRecord
            {
                Id = "weapon_legend_1",
                Name = "弑神",
                Type = EnchantmentType.Weapon,
                PrimaryEffect = EnchantmentEffect.Damage,
                PrimaryEffectValue = 50f,
                SecondaryEffect = EnchantmentEffect.CriticalRate,
                SecondaryEffectValue = 15f,
                Tier = EnchantmentTier.Legendary,
                RequiredLevel = 50,
                EnchantmentCost = 5000,
                SuccessRate = 30f,
                Description = "增加50点攻击力和15%暴击率",
                IconName = "crown"
            });

            // 护甲附魔 - 防御
            AddEnchantment(new EnchantmentRecord
            {
                Id = "armor_defense_1",
                Name = "坚固 I",
                Type = EnchantmentType.Armor,
                PrimaryEffect = EnchantmentEffect.Defense,
                PrimaryEffectValue = 8f,
                Tier = EnchantmentTier.Common,
                RequiredLevel = 1,
                EnchantmentCost = 100,
                SuccessRate = 90f,
                Description = "增加8点防御力",
                IconName = "shield"
            });

            AddEnchantment(new EnchantmentRecord
            {
                Id = "armor_defense_2",
                Name = "坚固 II",
                Type = EnchantmentType.Armor,
                PrimaryEffect = EnchantmentEffect.Defense,
                PrimaryEffectValue = 15f,
                Tier = EnchantmentTier.Uncommon,
                RequiredLevel = 12,
                EnchantmentCost = 300,
                SuccessRate = 80f,
                Description = "增加15点防御力",
                IconName = "shield"
            });

            AddEnchantment(new EnchantmentRecord
            {
                Id = "armor_defense_3",
                Name = "坚固 III",
                Type = EnchantmentType.Armor,
                PrimaryEffect = EnchantmentEffect.Defense,
                PrimaryEffectValue = 25f,
                Tier = EnchantmentTier.Rare,
                RequiredLevel = 28,
                EnchantmentCost = 600,
                SuccessRate = 70f,
                Description = "增加25点防御力",
                IconName = "shield"
            });

            // 护甲附魔 - 生命
            AddEnchantment(new EnchantmentRecord
            {
                Id = "armor_health_1",
                Name = "生命 I",
                Type = EnchantmentType.Armor,
                PrimaryEffect = EnchantmentEffect.Health,
                PrimaryEffectValue = 50f,
                Tier = EnchantmentTier.Common,
                RequiredLevel = 5,
                EnchantmentCost = 150,
                SuccessRate = 85f,
                Description = "增加50点最大生命",
                IconName = "heart"
            });

            AddEnchantment(new EnchantmentRecord
            {
                Id = "armor_health_2",
                Name = "生命 II",
                Type = EnchantmentType.Armor,
                PrimaryEffect = EnchantmentEffect.Health,
                PrimaryEffectValue = 100f,
                Tier = EnchantmentTier.Uncommon,
                RequiredLevel = 18,
                EnchantmentCost = 400,
                SuccessRate = 75f,
                Description = "增加100点最大生命",
                IconName = "heart"
            });

            // 护甲附魔 - 抗性
            AddEnchantment(new EnchantmentRecord
            {
                Id = "armor_fire_1",
                Name = "火焰抗性 I",
                Type = EnchantmentType.Armor,
                PrimaryEffect = EnchantmentEffect.FireResistance,
                PrimaryEffectValue = 10f,
                Tier = EnchantmentTier.Common,
                RequiredLevel = 8,
                EnchantmentCost = 200,
                SuccessRate = 85f,
                Description = "增加10%火焰抗性",
                IconName = "fire"
            });

            AddEnchantment(new EnchantmentRecord
            {
                Id = "armor_ice_1",
                Name = "冰霜抗性 I",
                Type = EnchantmentType.Armor,
                PrimaryEffect = EnchantmentEffect.IceResistance,
                PrimaryEffectValue = 10f,
                Tier = EnchantmentTier.Common,
                RequiredLevel = 8,
                EnchantmentCost = 200,
                SuccessRate = 85f,
                Description = "增加10%冰霜抗性",
                IconName = "snowflake"
            });

            AddEnchantment(new EnchantmentRecord
            {
                Id = "armor_lightning_1",
                Name = "闪电抗性 I",
                Type = EnchantmentType.Armor,
                PrimaryEffect = EnchantmentEffect.LightningResistance,
                PrimaryEffectValue = 10f,
                Tier = EnchantmentTier.Common,
                RequiredLevel = 8,
                EnchantmentCost = 200,
                SuccessRate = 85f,
                Description = "增加10%闪电抗性",
                IconName = "bolt"
            });

            // 护甲附魔 - 闪避
            AddEnchantment(new EnchantmentRecord
            {
                Id = "armor_dodge_1",
                Name = "闪避 I",
                Type = EnchantmentType.Armor,
                PrimaryEffect = EnchantmentEffect.Dodge,
                PrimaryEffectValue = 3f,
                Tier = EnchantmentTier.Rare,
                RequiredLevel = 22,
                EnchantmentCost = 550,
                SuccessRate = 65f,
                Description = "增加3%闪避率",
                IconName = "dodge"
            });

            // 护甲附魔 - 传奇
            AddEnchantment(new EnchantmentRecord
            {
                Id = "armor_legend_1",
                Name = "不朽",
                Type = EnchantmentType.Armor,
                PrimaryEffect = EnchantmentEffect.Health,
                PrimaryEffectValue = 300f,
                SecondaryEffect = EnchantmentEffect.Defense,
                SecondaryEffectValue = 50f,
                Tier = EnchantmentTier.Legendary,
                RequiredLevel = 50,
                EnchantmentCost = 5000,
                SuccessRate = 30f,
                Description = "增加300点生命和50点防御",
                IconName = "crown"
            });

            // 饰品附魔 - 全属性
            AddEnchantment(new EnchantmentRecord
            {
                Id = "accessory_allattr_1",
                Name = "全能 I",
                Type = EnchantmentType.Accessory,
                PrimaryEffect = EnchantmentEffect.AllAttributes,
                PrimaryEffectValue = 5f,
                Tier = EnchantmentTier.Uncommon,
                RequiredLevel = 15,
                EnchantmentCost = 350,
                SuccessRate = 75f,
                Description = "增加5点所有属性",
                IconName = "star"
            });

            AddEnchantment(new EnchantmentRecord
            {
                Id = "accessory_allattr_2",
                Name = "全能 II",
                Type = EnchantmentType.Accessory,
                PrimaryEffect = EnchantmentEffect.AllAttributes,
                PrimaryEffectValue = 10f,
                Tier = EnchantmentTier.Rare,
                RequiredLevel = 30,
                EnchantmentCost = 700,
                SuccessRate = 65f,
                Description = "增加10点所有属性",
                IconName = "star"
            });

            // 饰品附魔 - 幸运
            AddEnchantment(new EnchantmentRecord
            {
                Id = "accessory_luck_1",
                Name = "幸运 I",
                Type = EnchantmentType.Accessory,
                PrimaryEffect = EnchantmentEffect.Luck,
                PrimaryEffectValue = 5f,
                Tier = EnchantmentTier.Rare,
                RequiredLevel = 20,
                EnchantmentCost = 500,
                SuccessRate = 70f,
                Description = "增加5点幸运值",
                IconName = "clover"
            });

            // 饰品附魔 - 法力
            AddEnchantment(new EnchantmentRecord
            {
                Id = "accessory_mana_1",
                Name = "法力 I",
                Type = EnchantmentType.Accessory,
                PrimaryEffect = EnchantmentEffect.Mana,
                PrimaryEffectValue = 30f,
                Tier = EnchantmentTier.Common,
                RequiredLevel = 3,
                EnchantmentCost = 120,
                SuccessRate = 88f,
                Description = "增加30点最大法力",
                IconName = "mana"
            });

            AddEnchantment(new EnchantmentRecord
            {
                Id = "accessory_manaregen_1",
                Name = "回蓝 I",
                Type = EnchantmentType.Accessory,
                PrimaryEffect = EnchantmentEffect.ManaRegen,
                PrimaryEffectValue = 2f,
                Tier = EnchantmentTier.Uncommon,
                RequiredLevel = 15,
                EnchantmentCost = 300,
                SuccessRate = 80f,
                Description = "增加2点/秒法力回复",
                IconName = "mana_regen"
            });

            // 通用附魔 - 速度
            AddEnchantment(new EnchantmentRecord
            {
                Id = "universal_speed_1",
                Name = "敏捷 I",
                Type = EnchantmentType.Universal,
                PrimaryEffect = EnchantmentEffect.Speed,
                PrimaryEffectValue = 3f,
                Tier = EnchantmentTier.Common,
                RequiredLevel = 1,
                EnchantmentCost = 80,
                SuccessRate = 92f,
                Description = "增加3%移动速度",
                IconName = "boot"
            });

            // 通用附魔 - 力量
            AddEnchantment(new EnchantmentRecord
            {
                Id = "universal_str_1",
                Name = "力量 I",
                Type = EnchantmentType.Universal,
                PrimaryEffect = EnchantmentEffect.Strength,
                PrimaryEffectValue = 8f,
                Tier = EnchantmentTier.Common,
                RequiredLevel = 1,
                EnchantmentCost = 100,
                SuccessRate = 90f,
                Description = "增加8点力量",
                IconName = "muscle"
            });

            AddEnchantment(new EnchantmentRecord
            {
                Id = "universal_str_2",
                Name = "力量 II",
                Type = EnchantmentType.Universal,
                PrimaryEffect = EnchantmentEffect.Strength,
                PrimaryEffectValue = 15f,
                Tier = EnchantmentTier.Uncommon,
                RequiredLevel = 15,
                EnchantmentCost = 300,
                SuccessRate = 80f,
                Description = "增加15点力量",
                IconName = "muscle"
            });

            // 通用附魔 - 智力
            AddEnchantment(new EnchantmentRecord
            {
                Id = "universal_int_1",
                Name = "智慧 I",
                Type = EnchantmentType.Universal,
                PrimaryEffect = EnchantmentEffect.Intelligence,
                PrimaryEffectValue = 8f,
                Tier = EnchantmentTier.Common,
                RequiredLevel = 1,
                EnchantmentCost = 100,
                SuccessRate = 90f,
                Description = "增加8点智力",
                IconName = "brain"
            });

            // 通用附魔 - 敏捷
            AddEnchantment(new EnchantmentRecord
            {
                Id = "universal_dex_1",
                Name = "灵巧 I",
                Type = EnchantmentType.Universal,
                PrimaryEffect = EnchantmentEffect.Dexterity,
                PrimaryEffectValue = 8f,
                Tier = EnchantmentTier.Common,
                RequiredLevel = 1,
                EnchantmentCost = 100,
                SuccessRate = 90f,
                Description = "增加8点敏捷",
                IconName = "agility"
            });

            // 通用附魔 - 体力
            AddEnchantment(new EnchantmentRecord
            {
                Id = "universal_vit_1",
                Name = "体质 I",
                Type = EnchantmentType.Universal,
                PrimaryEffect = EnchantmentEffect.Vitality,
                PrimaryEffectValue = 8f,
                Tier = EnchantmentTier.Common,
                RequiredLevel = 1,
                EnchantmentCost = 100,
                SuccessRate = 90f,
                Description = "增加8点体力",
                IconName = "health"
            });
        }

        /// <summary>
        /// 添加附魔配置
        /// </summary>
        private void AddEnchantment(EnchantmentRecord record)
        {
            _enchantments[record.Id] = record;
            _tierCache[record.Tier].Add(record.Id);
            _typeCache[record.Type].Add(record.Id);
        }

        /// <summary>
        /// 获取所有附魔
        /// </summary>
        public List<EnchantmentRecord> GetAllEnchantments()
        {
            return new List<EnchantmentRecord>(_enchantments.Values);
        }

        /// <summary>
        /// 根据ID获取附魔
        /// </summary>
        public EnchantmentRecord GetEnchantmentById(string id)
        {
            if (_enchantments.ContainsKey(id))
                return _enchantments[id];
            return null;
        }

        /// <summary>
        /// 根据类型获取附魔
        /// </summary>
        public List<EnchantmentRecord> GetEnchantmentsByType(EnchantmentType type)
        {
            List<EnchantmentRecord> result = new List<EnchantmentRecord>();
            if (_typeCache.ContainsKey(type))
            {
                foreach (var id in _typeCache[type])
                {
                    result.Add(_enchantments[id]);
                }
            }
            return result;
        }

        /// <summary>
        /// 根据等级获取附魔
        /// </summary>
        public List<EnchantmentRecord> GetEnchantmentsByTier(EnchantmentTier tier)
        {
            List<EnchantmentRecord> result = new List<EnchantmentRecord>();
            if (_tierCache.ContainsKey(tier))
            {
                foreach (var id in _tierCache[tier])
                {
                    result.Add(_enchantments[id]);
                }
            }
            return result;
        }

        /// <summary>
        /// 根据玩家等级获取可用附魔
        /// </summary>
        public List<EnchantmentRecord> GetAvailableEnchantments(int playerLevel)
        {
            List<EnchantmentRecord> result = new List<EnchantmentRecord>();
            foreach (var enchantment in _enchantments.Values)
            {
                if (enchantment.RequiredLevel <= playerLevel)
                {
                    result.Add(enchantment);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取附魔总数
        /// </summary>
        public int GetTotalCount()
        {
            return _enchantments.Count;
        }

        protected override void OnExportSaveData(Godot.Collections.Dictionary saveData)
        {
            // EnchantmentDatabase 是静态配置数据库，无玩家状态需持久化
        }

        protected override void OnImportSaveData(Godot.Collections.Dictionary saveData)
        {
            // EnchantmentDatabase 是静态配置数据库，无玩家状态需恢复
        }
    }
}
