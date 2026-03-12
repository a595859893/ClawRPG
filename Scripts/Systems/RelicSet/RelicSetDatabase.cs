using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 遗物套装数据库配置
    /// </summary>
    public class RelicSetDatabase
    {
        private static RelicSetDatabase _instance;
        public static RelicSetDatabase Instance => _instance ??= new RelicSetDatabase();

        public Dictionary<string, RelicSetData.RelicSet> Sets { get; private set; } = new Dictionary<string, RelicSetData.RelicSet>();

        public RelicSetDatabase()
        {
            InitializeSets();
        }

        private void InitializeSets()
        {
            // 战士套装 - 力量与防御
            var warriorSet = new RelicSetData.RelicSet
            {
                Id = "warrior_legacy",
                Name = "战士传承",
                Description = "古老战士的遗留下的装备，蕴含着战斗的智慧",
                PieceCount = 4,
                RelicIds = new List<string> { "warrior_sword", "warrior_shield", "warrior_helmet", "warrior_armor" },
                SetBonuses = new Dictionary<string, float>
                {
                    { "2", 0.15f },  // 2件: 15% 攻击力
                    { "3", 0.10f },  // 3件: 10% 防御力
                    { "4", 0.25f }   // 4件: 25% 生命值
                },
                Icon = "⚔️"
            };
            Sets["warrior_legacy"] = warriorSet;

            // 法师套装 - 魔法与智慧
            var mageSet = new RelicSetData.RelicSet
            {
                Id = "mage_arcane",
                Name = "奥术法师",
                Description = "神秘法师的珍藏，蕴含强大的魔法能量",
                PieceCount = 4,
                RelicIds = new List<string> { "mage_staff", "mage_tome", "mage_amulet", "mage_robe" },
                SetBonuses = new Dictionary<string, float>
                {
                    { "2", 0.20f },  // 2件: 20% 魔法攻击
                    { "3", 0.15f },  // 3件: 15% 法力上限
                    { "4", 0.30f }   // 4件: 30% 技能冷却减少
                },
                Icon = "🔮"
            };
            Sets["mage_arcane"] = mageSet;

            // 盗贼套装 - 速度与暴击
            var rogueSet = new RelicSetData.RelicSet
            {
                Id = "rogue_shadow",
                Name = "暗影刺客",
                Description = "隐秘刺客的暗杀装备，致命而迅速",
                PieceCount = 4,
                RelicIds = new List<string> { "rogue_dagger", "rogue_cloak", "rogue_boots", "rogue_mask" },
                SetBonuses = new Dictionary<string, float>
                {
                    { "2", 0.18f },  // 2件: 18% 暴击率
                    { "3", 0.25f },  // 3件: 25% 攻击速度
                    { "4", 0.35f }   // 4件: 35% 闪避率
                },
                Icon = "🗡️"
            };
            Sets["rogue_shadow"] = rogueSet;

            // 圣骑士套装 - 神圣与防御
            var paladinSet = new RelicSetData.RelicSet
            {
                Id = "paladin_holy",
                Name = "圣光卫士",
                Description = "神圣骑士的圣洁装备，驱散一切邪恶",
                PieceCount = 4,
                RelicIds = new List<string> { "paladin_hammer", "paladin_shield_holy", "paladin_helm_holy", "paladin_armor_holy" },
                SetBonuses = new Dictionary<string, float>
                {
                    { "2", 0.15f },  // 2件: 15% 神圣伤害
                    { "3", 0.20f },  // 3件: 20% 治疗效果
                    { "4", 0.40f }   // 4件: 40% 伤害减免
                },
                Icon = "🛡️"
            };
            Sets["paladin_holy"] = paladinSet;

            // 游侠套装 - 远程与自然
            var rangerSet = new RelicSetData.RelicSet
            {
                Id = "ranger_nature",
                Name = "森林游侠",
                Description = "自然守护者的装备，与万物合一",
                PieceCount = 4,
                RelicIds = new List<string> { "ranger_bow", "ranger_quiver", "ranger_cloak_nature", "ranger_boots_nature" },
                SetBonuses = new Dictionary<string, float>
                {
                    { "2", 0.20f },  // 2件: 20% 远程伤害
                    { "3", 0.15f },  // 3件: 15% 移动速度
                    { "4", 0.25f }   // 4件: 25% 经验获取
                },
                Icon = "🏹"
            };
            Sets["ranger_nature"] = rangerSet;

            // 术士套装 - 暗影与火焰
            var warlockSet = new RelicSetData.RelicSet
            {
                Id = "warlock_dark",
                Name = "暗影术士",
                Description = "黑暗法师的禁忌装备，操控暗影之力",
                PieceCount = 4,
                RelicIds = new List<string> { "warlock_staff_dark", "warlock_tome_shadow", "warlock_ring_dark", "warlock_cloak_shadow" },
                SetBonuses = new Dictionary<string, float>
                {
                    { "2", 0.22f },  // 2件: 22% 暗影伤害
                    { "3", 0.18f },  // 3件: 18% 生命偷取
                    { "4", 0.35f }   // 4件: 35% 技能伤害
                },
                Icon = "🔥"
            };
            Sets["warlock_dark"] = warlockSet;

            // 德鲁伊套装 - 自然与变形
            var druidSet = new RelicSetData.RelicSet
            {
                Id = "druid_natural",
                Name = "自然德鲁伊",
                Description = "自然之力的化身，与动物沟通",
                PieceCount = 4,
                RelicIds = new List<string> { "druid_staff_nature", "druid_cloak_leaf", "druid_amulet_moon", "druid_boots_nature" },
                SetBonuses = new Dictionary<string, float>
                {
                    { "2", 0.15f },  // 2件: 15% 自然伤害
                    { "3", 0.25f },  // 3件: 25% 变形持续时间
                    { "4", 0.30f }   // 4件: 30% 动物伙伴属性
                },
                Icon = "🌿"
            };
            Sets["druid_natural"] = druidSet;

            // 吟游诗人套装 - 音乐与辅助
            var bardSet = new RelicSetData.RelicSet
            {
                Id = "bard_melodic",
                Name = "旋律诗人",
                Description = "音乐大师的乐器，治愈人心",
                PieceCount = 4,
                RelicIds = new List<string> { "bard_lute", "bard_flute", "bard_drum", "bard_lyre" },
                SetBonuses = new Dictionary<string, float>
                {
                    { "2", 0.18f },  // 2件: 18% 辅助技能效果
                    { "3", 0.20f },  // 3件: 20% 经验获取
                    { "4", 0.30f }   // 4件: 30% 团队属性加成
                },
                Icon = "🎵"
            };
            Sets["bard_melodic"] = bardSet;
        }

        public RelicSetData.RelicSet GetSet(string setId)
        {
            return Sets.ContainsKey(setId) ? Sets[setId] : null;
        }

        public List<RelicSetData.RelicSet> GetAllSets()
        {
            return new List<RelicSetData.RelicSet>(Sets.Values);
        }
    }
}
