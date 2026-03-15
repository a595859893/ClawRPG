// ============================================
// Relic Collection System - 遗物收集系统
// ============================================

namespace ClawRPG.Systems.Relics
{
    // 遗物稀有度
    public enum RelicRarity
    {
        Common = 1,      // 普通
        Uncommon = 2,    // 优秀
        Rare = 3,        // 稀有
        Epic = 4,        // 史诗
        Legendary = 5,   // 传说
        Mythic = 6       // 神器
    }

    // 遗物类型
    public enum RelicType
    {
        Weapon,      // 武器
        Armor,       // 护甲
        Accessory,   // 饰品
        Passive,     // 被遗物
        Trigger,     // 触发遗物
        Set          // 套装遗物
    }

    // 遗物效果类型
    public enum RelicEffectType
    {
        DamageIncrease,      // 伤害增加
        DamageReduction,    // 伤害减免
        CriticalRate,       // 暴击率
        CriticalDamage,     // 暴击伤害
        AttackSpeed,        // 攻击速度
        MoveSpeed,          // 移动速度
        HealthMax,          // 最大生命
        ManaMax,            // 最大法力
        HealthRegen,        // 生命恢复
        ManaRegen,          // 法力恢复
        LifeSteal,          // 生命偷取
        CooldownReduction,  // 冷却缩减
        ElementalDamage,    // 元素伤害
        ElementalResist,    // 元素抗性
        GoldGain,           // 金币获取
        ExperienceGain,     // 经验获取
        DropRate,           // 掉落率
        EnemyScale,         // 敌人规模
        RoomReward          // 房间奖励
    }

    // 遗物数据
    public class Relic
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public RelicType Type { get; set; }
        public RelicRarity Rarity { get; set; }
        public RelicEffectType PrimaryEffect { get; set; }
        public double PrimaryEffectValue { get; set; }
        public RelicEffectType? SecondaryEffect { get; set; }
        public double? SecondaryEffectValue { get; set; }
        public string SetId { get; set; }
        public int Level { get; set; }
    }

    // 遗物套装
    public class RelicSet
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RequiredCount { get; set; }
        public RelicEffectType SetEffect { get; set; }
        public double SetEffectValue { get; set; }
    }

    // 玩家遗物数据
    public class PlayerRelicData
    {
        public string RelicId { get; set; }
        public bool Unlocked { get; set; }
        public int CurrentLevel { get; set; }
        public int MaxLevel { get; set; }
        public bool Equipped { get; set; }
    }

    // 玩家遗物收集数据
    public class PlayerRelicCollection
    {
        public Dictionary<string, PlayerRelicData> Relics { get; set; }
        public List<string> EquippedRelics { get; set; }
        public Dictionary<string, int> SetCompletions { get; set; }
        public int TotalRelicsUnlocked { get; set; }
    }

    // 遗物统计
    public class RelicStatistics
    {
        public int TotalRelicsUnlocked { get; set; }
        public int TotalRelicsEquipped { get; set; }
        public Dictionary<RelicRarity, int> UnlockedByRarity { get; set; }
        public Dictionary<RelicType, int> UnlockedByType { get; set; }
        public int SetsCompleted { get; set; }
        public int TotalRelicLevels { get; set; }
    }

    // 遗物生成配置
    public class RelicGenerationConfig
    {
        public int MinRelicsPerFloor { get; set; }
        public int MaxRelicsPerFloor { get; set; }
        public double CommonChance { get; set; }
        public double UncommonChance { get; set; }
        public double RareChance { get; set; }
        public double EpicChance { get; set; }
        public double LegendaryChance { get; set; }
        public double MythicChance { get; set; }
    }
}
