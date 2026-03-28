using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ClawRPG.Scripts.Systems.EventCardPool
{
    /// <summary>
    /// 事件卡类别
    /// </summary>
    public enum EventCardCategory
    {
        Resource,   // 资源类：生命/能量/金币变化
        Ally,       // 盟友类：临时NPC加入
        Terrain,    // 地形类：战场环境变化
        Curse,      // 诅咒类：负面效果
        Blessing    // 祝福类：正面效果
    }

    /// <summary>
    /// 事件卡稀有度
    /// </summary>
    public enum EventCardRarity
    {
        Common,     // 白色边框 50%
        Uncommon,   // 绿色边框 25%
        Rare,       // 蓝色边框 15%
        Epic,       // 紫色边框 7%
        Legendary   // 橙色边框 3%
    }

    /// <summary>
    /// 事件卡效果类型
    /// </summary>
    public enum EventCardEffectType
    {
        HealPlayer,         // 恢复玩家生命
        DamagePlayer,       // 伤害玩家
        EnergyBoost,        // 增加能量
        GoldChange,        // 金币变化
        TempAlly,          // 临时盟友
        TerrainEffect,     // 地形效果
        BuffEnemy,         // 敌人强化
        BuffPlayer,        // 玩家增益
        DebuffPlayer,      // 玩家debuff
        ShieldPlayer       // 护盾
    }

    /// <summary>
    /// 效果目标
    /// </summary>
    public enum EventCardEffectTarget
    {
        Player,
        AllEnemies,
        RandomEnemy,
        Terrain,
        None
    }

    /// <summary>
    /// 触发时机
    /// </summary>
    public enum EventCardTriggerTiming
    {
        OnDraw,         // 抽卡时立即触发
        OnCombatStart,  // 战斗开始时触发
        OnEnemySpawn,   // 敌人出现时触发
        OnPlayerHurt,   // 玩家受伤时触发
        OnHalfHealth    // 血量过半时触发
    }

    /// <summary>
    /// 单个效果定义
    /// </summary>
    public class EventCardEffect
    {
        [JsonPropertyName("effectType")]
        public EventCardEffectType EffectType { get; set; }

        [JsonPropertyName("amount")]
        public float Amount { get; set; }

        [JsonPropertyName("target")]
        public EventCardEffectTarget Target { get; set; }

        [JsonPropertyName("duration")]
        public float Duration { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        public EventCardEffect Clone()
        {
            return new EventCardEffect
            {
                EffectType = EffectType,
                Amount = Amount,
                Target = Target,
                Duration = Duration,
                Description = Description
            };
        }
    }

    /// <summary>
    /// 接受选项（重抽费用等）
    /// </summary>
    public class EventCardAcceptOption
    {
        [JsonPropertyName("rerollCost")]
        public int RerollCost { get; set; }

        [JsonPropertyName("acceptText")]
        public string AcceptText { get; set; } = "接受";

        [JsonPropertyName("rerollText")]
        public string RerollText { get; set; } = "重新抽卡";
    }

    /// <summary>
    /// 事件卡配置数据（JSON反序列化用）
    /// </summary>
    public class EventCardConfig
    {
        [JsonPropertyName("cardId")]
        public string CardId { get; set; } = "";

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("category")]
        public EventCardCategory Category { get; set; }

        [JsonPropertyName("rarity")]
        public EventCardRarity Rarity { get; set; }

        [JsonPropertyName("triggerTiming")]
        public EventCardTriggerTiming TriggerTiming { get; set; }

        [JsonPropertyName("effects")]
        public List<EventCardEffect> Effects { get; set; } = new List<EventCardEffect>();

        [JsonPropertyName("acceptOption")]
        public EventCardAcceptOption AcceptOption { get; set; } = new EventCardAcceptOption();

        [JsonPropertyName("minPlayerLevel")]
        public int MinPlayerLevel { get; set; } = 1;

        /// <summary>
        /// 效果展示文本
        /// </summary>
        public string GetEffectsText()
        {
            var lines = new List<string>();
            foreach (var effect in Effects)
            {
                if (!string.IsNullOrEmpty(effect.Description))
                {
                    lines.Add(effect.Description);
                }
                else
                {
                    string desc = effect.EffectType switch
                    {
                        EventCardEffectType.HealPlayer => $"恢复 {effect.Amount} 点生命",
                        EventCardEffectType.DamagePlayer => $"受到 {effect.Amount} 点伤害",
                        EventCardEffectType.EnergyBoost => $"获得 {effect.Amount} 点能量",
                        EventCardEffectType.GoldChange => $"金币 {(effect.Amount >= 0 ? "+" : "")}{effect.Amount}",
                        EventCardEffectType.TempAlly => $"临时盟友加入（持续 {effect.Duration} 秒）",
                        EventCardEffectType.TerrainEffect => $"地形效果：{effect.Description}",
                        EventCardEffectType.BuffEnemy => $"敌人获得 {(effect.Amount * 100):F0}% 攻击力提升",
                        EventCardEffectType.BuffPlayer => $"玩家获得 {(effect.Amount * 100):F0}% 增伤",
                        EventCardEffectType.DebuffPlayer => $"玩家获得负面状态",
                        EventCardEffectType.ShieldPlayer => $"获得 {effect.Amount} 点临时护盾",
                        _ => "未知效果"
                    };
                    lines.Add(desc);
                }
            }
            return string.Join("\n", lines);
        }

        /// <summary>
        /// 稀有度边框颜色 (Godot Color)
        /// </summary>
        public Color GetRarityColor()
        {
            return Rarity switch
            {
                EventCardRarity.Common => new Color(0.85f, 0.85f, 0.85f),     // 白色
                EventCardRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),      // 绿色
                EventCardRarity.Rare => new Color(0.3f, 0.5f, 1.0f),           // 蓝色
                EventCardRarity.Epic => new Color(0.6f, 0.2f, 0.9f),          // 紫色
                EventCardRarity.Legendary => new Color(1.0f, 0.6f, 0.1f),     // 橙色
                _ => Colors.White
            };
        }

        /// <summary>
        /// 稀有度权重（用于加权随机）
        /// </summary>
        public static float GetRarityWeight(EventCardRarity rarity)
        {
            return rarity switch
            {
                EventCardRarity.Common => 50f,
                EventCardRarity.Uncommon => 25f,
                EventCardRarity.Rare => 15f,
                EventCardRarity.Epic => 7f,
                EventCardRarity.Legendary => 3f,
                _ => 0f
            };
        }
    }

    /// <summary>
    /// 事件卡配置文件结构（JSON根对象）
    /// </summary>
    public class EventCardsConfigFile
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0";

        [JsonPropertyName("cards")]
        public List<EventCardConfig> Cards { get; set; } = new List<EventCardConfig>();
    }
}
