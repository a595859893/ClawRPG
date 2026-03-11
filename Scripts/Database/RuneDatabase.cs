namespace ClawRPG.Scripts.Database
{
    using ClawRPG.Scripts.Data;

    /// <summary>
    /// 符文配置数据库
    /// </summary>
    public static class RuneDatabase
    {
        // 符文配置
        public static readonly Dictionary<string, Rune> Runes = new Dictionary<string, Rune>
        {
            // 攻击符文
            { "attack_common_1", new Rune { Id = "attack_common_1", Name = "锋利符文", Type = RuneType.Attack, Rarity = RuneRarity.Common, Level = 1, AttributeValue = 5f } },
            { "attack_common_2", new Rune { Id = "attack_common_2", Name = "锋利符文", Type = RuneType.Attack, Rarity = RuneRarity.Common, Level = 2, AttributeValue = 8f } },
            { "attack_uncommon_1", new Rune { Id = "attack_uncommon_1", Name = "穿刺符文", Type = RuneType.Attack, Rarity = RuneRarity.Uncommon, Level = 1, AttributeValue = 12f } },
            { "attack_uncommon_2", new Rune { Id = "attack_uncommon_2", Name = "穿刺符文", Type = RuneType.Attack, Rarity = RuneRarity.Uncommon, Level = 2, AttributeValue = 18f } },
            { "attack_rare_1", new Rune { Id = "attack_rare_1", Name = "锐利符文", Type = RuneType.Attack, Rarity = RuneRarity.Rare, Level = 1, AttributeValue = 25f } },
            { "attack_rare_2", new Rune { Id = "attack_rare_2", Name = "锐利符文", Type = RuneType.Attack, Rarity = RuneRarity.Rare, Level = 2, AttributeValue = 35f } },
            { "attack_epic_1", new Rune { Id = "attack_epic_1", Name = "破坏符文", Type = RuneType.Attack, Rarity = RuneRarity.Epic, Level = 1, AttributeValue = 50f } },
            { "attack_epic_2", new Rune { Id = "attack_epic_2", Name = "破坏符文", Type = RuneType.Attack, Rarity = RuneRarity.Epic, Level = 2, AttributeValue = 65f } },
            { "attack_legendary_1", new Rune { Id = "attack_legendary_1", Name = "弑神符文", Type = RuneType.Attack, Rarity = RuneRarity.Legendary, Level = 1, AttributeValue = 100f } },
            { "attack_legendary_2", new Rune { Id = "attack_legendary_2", Name = "弑神符文", Type = RuneType.Attack, Rarity = RuneRarity.Legendary, Level = 2, AttributeValue = 150f } },

            // 防御符文
            { "defense_common_1", new Rune { Id = "defense_common_1", Name = "护盾符文", Type = RuneType.Defense, Rarity = RuneRarity.Common, Level = 1, AttributeValue = 5f } },
            { "defense_common_2", new Rune { Id = "defense_common_2", Name = "护盾符文", Type = RuneType.Defense, Rarity = RuneRarity.Common, Level = 2, AttributeValue = 8f } },
            { "defense_uncommon_1", new Rune { Id = "defense_uncommon_1", Name = "钢铁符文", Type = RuneType.Defense, Rarity = RuneRarity.Uncommon, Level = 1, AttributeValue = 12f } },
            { "defense_uncommon_2", new Rune { Id = "defense_uncommon_2", Name = "钢铁符文", Type = RuneType.Defense, Rarity = RuneRarity.Uncommon, Level = 2, AttributeValue = 18f } },
            { "defense_rare_1", new Rune { Id = "defense_rare_1", Name = "钻石符文", Type = RuneType.Defense, Rarity = RuneRarity.Rare, Level = 1, AttributeValue = 25f } },
            { "defense_rare_2", new Rune { Id = "defense_rare_2", Name = "钻石符文", Type = RuneType.Defense, Rarity = RuneRarity.Rare, Level = 2, AttributeValue = 35f } },
            { "defense_epic_1", new Rune { Id = "defense_epic_1", Name = "绝对防御符文", Type = RuneType.Defense, Rarity = RuneRarity.Epic, Level = 1, AttributeValue = 50f } },
            { "defense_epic_2", new Rune { Id = "defense_epic_2", Name = "绝对防御符文", Type = RuneType.Defense, Rarity = RuneRarity.Epic, Level = 2, AttributeValue = 65f } },
            { "defense_legendary_1", new Rune { Id = "defense_legendary_1", Name = "不灭符文", Type = RuneType.Defense, Rarity = RuneRarity.Legendary, Level = 1, AttributeValue = 100f } },
            { "defense_legendary_2", new Rune { Id = "defense_legendary_2", Name = "不灭符文", Type = RuneType.Defense, Rarity = RuneRarity.Legendary, Level = 2, AttributeValue = 150f } },

            // 生命符文
            { "health_common_1", new Rune { Id = "health_common_1", Name = "生命符文", Type = RuneType.Health, Rarity = RuneRarity.Common, Level = 1, AttributeValue = 20f } },
            { "health_common_2", new Rune { Id = "health_common_2", Name = "生命符文", Type = RuneType.Health, Rarity = RuneRarity.Common, Level = 2, AttributeValue = 35f } },
            { "health_uncommon_1", new Rune { Id = "health_uncommon_1", Name = "活力符文", Type = RuneType.Health, Rarity = RuneRarity.Uncommon, Level = 1, AttributeValue = 50f } },
            { "health_uncommon_2", new Rune { Id = "health_uncommon_2", Name = "活力符文", Type = RuneType.Health, Rarity = RuneRarity.Uncommon, Level = 2, AttributeValue = 75f } },
            { "health_rare_1", new Rune { Id = "health_rare_1", Name = "再生符文", Type = RuneType.Health, Rarity = RuneRarity.Rare, Level = 1, AttributeValue = 100f } },
            { "health_rare_2", new Rune { Id = "health_rare_2", Name = "再生符文", Type = RuneType.Health, Rarity = RuneRarity.Rare, Level = 2, AttributeValue = 140f } },
            { "health_epic_1", new Rune { Id = "health_epic_1", Name = "不朽符文", Type = RuneType.Health, Rarity = RuneRarity.Epic, Level = 1, AttributeValue = 200f } },
            { "health_epic_2", new Rune { Id = "health_epic_2", Name = "不朽符文", Type = RuneType.Health, Rarity = RuneRarity.Epic, Level = 2, AttributeValue = 280f } },
            { "health_legendary_1", new Rune { Id = "health_legendary_1", Name = "永恒符文", Type = RuneType.Health, Rarity = RuneRarity.Legendary, Level = 1, AttributeValue = 400f } },
            { "health_legendary_2", new Rune { Id = "health_legendary_2", Name = "永恒符文", Type = RuneType.Health, Rarity = RuneRarity.Legendary, Level = 2, AttributeValue = 550f } },

            // 速度符文
            { "speed_common_1", new Rune { Id = "speed_common_1", Name = "迅捷符文", Type = RuneType.Speed, Rarity = RuneRarity.Common, Level = 1, AttributeValue = 2f } },
            { "speed_common_2", new Rune { Id = "speed_common_2", Name = "迅捷符文", Type = RuneType.Speed, Rarity = RuneRarity.Common, Level = 2, AttributeValue = 3f } },
            { "speed_uncommon_1", new Rune { Id = "speed_uncommon_1", Name = "疾风符文", Type = RuneType.Speed, Rarity = RuneRarity.Uncommon, Level = 1, AttributeValue = 5f } },
            { "speed_uncommon_2", new Rune { Id = "speed_uncommon_2", Name = "疾风符文", Type = RuneType.Speed, Rarity = RuneRarity.Uncommon, Level = 2, AttributeValue = 7f } },
            { "speed_rare_1", new Rune { Id = "speed_rare_1", Name = "闪电符文", Type = RuneType.Speed, Rarity = RuneRarity.Rare, Level = 1, AttributeValue = 10f } },
            { "speed_rare_2", new Rune { Id = "speed_rare_2", Name = "闪电符文", Type = RuneType.Speed, Rarity = RuneRarity.Rare, Level = 2, AttributeValue = 14f } },
            { "speed_epic_1", new Rune { Id = "speed_epic_1", Name = "音速符文", Type = RuneType.Speed, Rarity = RuneRarity.Epic, Level = 1, AttributeValue = 20f } },
            { "speed_epic_2", new Rune { Id = "speed_epic_2", Name = "音速符文", Type = RuneType.Speed, Rarity = RuneRarity.Epic, Level = 2, AttributeValue = 26f } },
            { "speed_legendary_1", new Rune { Id = "speed_legendary_1", Name = "光速符文", Type = RuneType.Speed, Rarity = RuneRarity.Legendary, Level = 1, AttributeValue = 35f } },
            { "speed_legendary_2", new Rune { Id = "speed_legendary_2", Name = "光速符文", Type = RuneType.Speed, Rarity = RuneRarity.Legendary, Level = 2, AttributeValue = 45f } },

            // 暴击符文
            { "critical_common_1", new Rune { Id = "critical_common_1", Name = "致命符文", Type = RuneType.Critical, Rarity = RuneRarity.Common, Level = 1, AttributeValue = 1f } },
            { "critical_common_2", new Rune { Id = "critical_common_2", Name = "致命符文", Type = RuneType.Critical, Rarity = RuneRarity.Common, Level = 2, AttributeValue = 2f } },
            { "critical_uncommon_1", new Rune { Id = "critical_uncommon_1", Name = "暴击符文", Type = RuneType.Critical, Rarity = RuneRarity.Uncommon, Level = 1, AttributeValue = 3f } },
            { "critical_uncommon_2", new Rune { Id = "critical_uncommon_2", Name = "暴击符文", Type = RuneType.Critical, Rarity = RuneRarity.Uncommon, Level = 2, AttributeValue = 4f } },
            { "critical_rare_1", new Rune { Id = "critical_rare_1", Name = "狩猎符文", Type = RuneType.Critical, Rarity = RuneRarity.Rare, Level = 1, AttributeValue = 6f } },
            { "critical_rare_2", new Rune { Id = "critical_rare_2", Name = "狩猎符文", Type = RuneType.Critical, Rarity = RuneRarity.Rare, Level = 2, AttributeValue = 8f } },
            { "critical_epic_1", new Rune { Id = "critical_epic_1", Name = "秒杀符文", Type = RuneType.Critical, Rarity = RuneRarity.Epic, Level = 1, AttributeValue = 12f } },
            { "critical_epic_2", new Rune { Id = "critical_epic_2", Name = "秒杀符文", Type = RuneType.Critical, Rarity = RuneRarity.Epic, Level = 2, AttributeValue = 15f } },
            { "critical_legendary_1", new Rune { Id = "critical_legendary_1", Name = "命运符文", Type = RuneType.Critical, Rarity = RuneRarity.Legendary, Level = 1, AttributeValue = 25f } },
            { "critical_legendary_2", new Rune { Id = "critical_legendary_2", Name = "命运符文", Type = RuneType.Critical, Rarity = RuneRarity.Legendary, Level = 2, AttributeValue = 32f } },

            // 魔法符文
            { "magic_common_1", new Rune { Id = "magic_common_1", Name = "魔法符文", Type = RuneType.Magic, Rarity = RuneRarity.Common, Level = 1, AttributeValue = 5f } },
            { "magic_common_2", new Rune { Id = "magic_common_2", Name = "魔法符文", Type = RuneType.Magic, Rarity = RuneRarity.Common, Level = 2, AttributeValue = 8f } },
            { "magic_uncommon_1", new Rune { Id = "magic_uncommon_1", Name = "奥术符文", Type = RuneType.Magic, Rarity = RuneRarity.Uncommon, Level = 1, AttributeValue = 12f } },
            { "magic_uncommon_2", new Rune { Id = "magic_uncommon_2", Name = "奥术符文", Type = RuneType.Magic, Rarity = RuneRarity.Uncommon, Level = 2, AttributeValue = 18f } },
            { "magic_rare_1", new Rune { Id = "magic_rare_1", Name = "秘法符文", Type = RuneType.Magic, Rarity = RuneRarity.Rare, Level = 1, AttributeValue = 25f } },
            { "magic_rare_2", new Rune { Id = "magic_rare_2", Name = "秘法符文", Type = RuneType.Magic, Rarity = RuneRarity.Rare, Level = 2, AttributeValue = 35f } },
            { "magic_epic_1", new Rune { Id = "magic_epic_1", Name = "元素符文", Type = RuneType.Magic, Rarity = RuneRarity.Epic, Level = 1, AttributeValue = 50f } },
            { "magic_epic_2", new Rune { Id = "magic_epic_2", Name = "元素符文", Type = RuneType.Magic, Rarity = RuneRarity.Epic, Level = 2, AttributeValue = 65f } },
            { "magic_legendary_1", new Rune { Id = "magic_legendary_1", Name = "神话符文", Type = RuneType.Magic, Rarity = RuneRarity.Legendary, Level = 1, AttributeValue = 100f } },
            { "magic_legendary_2", new Rune { Id = "magic_legendary_2", Name = "神话符文", Type = RuneType.Magic, Rarity = RuneRarity.Legendary, Level = 2, AttributeValue = 150f } },

            // 生命偷取符文
            { "lifesteal_common_1", new Rune { Id = "lifesteal_common_1", Name = "吸血符文", Type = RuneType.LifeSteal, Rarity = RuneRarity.Common, Level = 1, AttributeValue = 1f } },
            { "lifesteal_common_2", new Rune { Id = "lifesteal_common_2", Name = "吸血符文", Type = RuneType.LifeSteal, Rarity = RuneRarity.Common, Level = 2, AttributeValue = 2f } },
            { "lifesteal_uncommon_1", new Rune { Id = "lifesteal_uncommon_1", Name = "冥想符文", Type = RuneType.LifeSteal, Rarity = RuneRarity.Uncommon, Level = 1, AttributeValue = 3f } },
            { "lifesteal_uncommon_2", new Rune { Id = "lifesteal_uncommon_2", Name = "冥想符文", Type = RuneType.LifeSteal, Rarity = RuneRarity.Uncommon, Level = 2, AttributeValue = 4f } },
            { "lifesteal_rare_1", new Rune { Id = "lifesteal_rare_1", Name = "腐蚀符文", Type = RuneType.LifeSteal, Rarity = RuneRarity.Rare, Level = 1, AttributeValue = 6f } },
            { "lifesteal_rare_2", new Rune { Id = "lifesteal_rare_2", Name = "腐蚀符文", Type = RuneType.LifeSteal, Rarity = RuneRarity.Rare, Level = 2, AttributeValue = 8f } },
            { "lifesteal_epic_1", new Rune { Id = "lifesteal_epic_1", Name = "恶灵符文", Type = RuneType.LifeSteal, Rarity = RuneRarity.Epic, Level = 1, AttributeValue = 12f } },
            { "lifesteal_epic_2", new Rune { Id = "lifesteal_epic_2", Name = "恶灵符文", Type = RuneType.LifeSteal, Rarity = RuneRarity.Epic, Level = 2, AttributeValue = 15f } },
            { "lifesteal_legendary_1", new Rune { Id = "lifesteal_legendary_1", Name = "深渊符文", Type = RuneType.LifeSteal, Rarity = RuneRarity.Legendary, Level = 1, AttributeValue = 25f } },
            { "lifesteal_legendary_2", new Rune { Id = "lifesteal_legendary_2", Name = "深渊符文", Type = RuneType.LifeSteal, Rarity = RuneRarity.Legendary, Level = 2, AttributeValue = 32f } },

            // 闪避符文
            { "dodge_common_1", new Rune { Id = "dodge_common_1", Name = "闪避符文", Type = RuneType.Dodge, Rarity = RuneRarity.Common, Level = 1, AttributeValue = 1f } },
            { "dodge_common_2", new Rune { Id = "dodge_common_2", Name = "闪避符文", Type = RuneType.Dodge, Rarity = RuneRarity.Common, Level = 2, AttributeValue = 2f } },
            { "dodge_uncommon_1", new Rune { Id = "dodge_uncommon_1", Name = "幻影符文", Type = RuneType.Dodge, Rarity = RuneRarity.Uncommon, Level = 1, AttributeValue = 3f } },
            { "dodge_uncommon_2", new Rune { Id = "dodge_uncommon_2", Name = "幻影符文", Type = RuneType.Dodge, Rarity = RuneRarity.Uncommon, Level = 2, AttributeValue = 4f } },
            { "dodge_rare_1", new Rune { Id = "dodge_rare_1", Name = "幽影符文", Type = RuneType.Dodge, Rarity = RuneRarity.Rare, Level = 1, AttributeValue = 6f } },
            { "dodge_rare_2", new Rune { Id = "dodge_rare_2", Name = "幽影符文", Type = RuneType.Dodge, Rarity = RuneRarity.Rare, Level = 2, AttributeValue = 8f } },
            { "dodge_epic_1", new Rune { Id = "dodge_epic_1", Name = "鬼魅符文", Type = RuneType.Dodge, Rarity = RuneRarity.Epic, Level = 1, AttributeValue = 12f } },
            { "dodge_epic_2", new Rune { Id = "dodge_epic_2", Name = "鬼魅符文", Type = RuneType.Dodge, Rarity = RuneRarity.Epic, Level = 2, AttributeValue = 15f } },
            { "dodge_legendary_1", new Rune { Id = "dodge_legendary_1", Name = "虚无符文", Type = RuneType.Dodge, Rarity = RuneRarity.Legendary, Level = 1, AttributeValue = 25f } },
            { "dodge_legendary_2", new Rune { Id = "dodge_legendary_2", Name = "虚无符文", Type = RuneType.Dodge, Rarity = RuneRarity.Legendary, Level = 2, AttributeValue = 32f } },
        };

        // 符文套装配置
        public static readonly Dictionary<string, RuneSet> RuneSets = new Dictionary<string, RuneSet>
        {
            { "warrior", new RuneSet { Id = "warrior", Name = "战士套装", RuneTypeCounts = new int[] { 4, 2, 0, 0, 0, 0, 0, 0 }, BonusAttributes = new float[] { 10f, 25f, 50f } } },
            { "guardian", new RuneSet { Id = "guardian", Name = "守护者套装", RuneTypeCounts = new int[] { 0, 4, 2, 0, 0, 0, 0, 0 }, BonusAttributes = new float[] { 10f, 25f, 50f } } },
            { "assassin", new RuneSet { Id = "assassin", Name = "刺客套装", RuneTypeCounts = new int[] { 2, 0, 0, 2, 2, 0, 0, 0 }, BonusAttributes = new float[] { 10f, 25f, 50f } } },
            { "mage", new RuneSet { Id = "mage", Name = "法师套装", RuneTypeCounts = new int[] { 0, 0, 0, 0, 0, 4, 2, 0 }, BonusAttributes = new float[] { 10f, 25f, 50f } } },
            { "tank", new RuneSet { Id = "tank", Name = "坦克套装", RuneTypeCounts = new int[] { 0, 2, 4, 0, 0, 0, 0, 2 }, BonusAttributes = new float[] { 10f, 25f, 50f } } },
            { "hunter", new RuneSet { Id = "hunter", Name = "猎人套装", RuneTypeCounts = new int[] { 2, 0, 0, 2, 0, 0, 2, 0 }, BonusAttributes = new float[] { 10f, 25f, 50f } } },
            { "balanced", new RuneSet { Id = "balanced", Name = "均衡套装", RuneTypeCounts = new int[] { 1, 1, 1, 1, 1, 1, 1, 1 }, BonusAttributes = new float[] { 8f, 20f, 40f } } },
            { "berserker", new RuneSet { Id = "berserker", Name = "狂战士套装", RuneTypeCounts = new int[] { 4, 0, 0, 0, 2, 0, 2, 0 }, BonusAttributes = new float[] { 12f, 30f, 60f } } },
            { "healer", new RuneSet { Id = "healer", Name = "治疗者套装", RuneTypeCounts = new int[] { 0, 0, 4, 0, 0, 2, 2, 0 }, BonusAttributes = new float[] { 10f, 25f, 50f } } },
            { "legendary", new RuneSet { Id = "legendary", Name = "传奇套装", RuneTypeCounts = new int[] { 2, 2, 2, 2, 0, 0, 0, 0 }, BonusAttributes = new float[] { 15f, 35f, 70f } } },
        };

        // 稀有度颜色
        public static readonly Dictionary<RuneRarity, Color> RarityColors = new Dictionary<RuneRarity, Color>
        {
            { RuneRarity.Common, new Color(1f, 1f, 1f) },
            { RuneRarity.Uncommon, new Color(0f, 1f, 0f) },
            { RuneRarity.Rare, new Color(0f, 0.5f, 1f) },
            { RuneRarity.Epic, new Color(0.6f, 0.2f, 1f) },
            { RuneRarity.Legendary, new Color(1f, 0.6f, 0f) },
        };

        // 稀有度权重（用于随机生成）
        public static readonly Dictionary<RuneRarity, int> RarityWeights = new Dictionary<RuneRarity, int>
        {
            { RuneRarity.Common, 50 },
            { RuneRarity.Uncommon, 30 },
            { RuneRarity.Rare, 15 },
            { RuneRarity.Epic, 4 },
            { RuneRarity.Legendary, 1 },
        };

        /// <summary>
        /// 根据类型和稀有度获取符文
        /// </summary>
        public static Rune GetRune(RuneType type, RuneRarity rarity, int level)
        {
            string key = $"{type.ToString().ToLower()}_{rarity.ToString().ToLower()}_{level}";
            return Runes.ContainsKey(key) ? Runes[key] : null;
        }

        /// <summary>
        /// 获取随机符文
        /// </summary>
        public static Rune GetRandomRune()
        {
            var rand = new Random();
            var rarity = GetRandomRarity(rand);
            var type = (RuneType)rand.Next(0, 8);
            var level = rand.Next(1, 3);
            return GetRune(type, rarity, level);
        }

        /// <summary>
        /// 根据权重获取随机稀有度
        /// </summary>
        public static RuneRarity GetRandomRarity(Random rand)
        {
            int total = 0;
            foreach (var w in RarityWeights.Values) total += w;
            int roll = rand.Next(0, total);
            int cumulative = 0;
            foreach (var pair in RarityWeights)
            {
                cumulative += pair.Value;
                if (roll < cumulative) return pair.Key;
            }
            return RuneRarity.Common;
        }

        /// <summary>
        /// 获取符文类型名称
        /// </summary>
        public static string GetRuneTypeName(RuneType type)
        {
            return type switch
            {
                RuneType.Attack => "攻击",
                RuneType.Defense => "防御",
                RuneType.Health => "生命",
                RuneType.Speed => "速度",
                RuneType.Critical => "暴击",
                RuneType.Magic => "魔法",
                RuneType.LifeSteal => "生命偷取",
                RuneType.Dodge => "闪避",
                _ => "未知"
            };
        }

        /// <summary>
        /// 获取符文稀有度名称
        /// </summary>
        public static string GetRuneRarityName(RuneRarity rarity)
        {
            return rarity switch
            {
                RuneRarity.Common => "普通",
                RuneRarity.Uncommon => "优秀",
                RuneRarity.Rare => "稀有",
                RuneRarity.Epic => "史诗",
                RuneRarity.Legendary => "传说",
                _ => "未知"
            };
        }
    }
}
