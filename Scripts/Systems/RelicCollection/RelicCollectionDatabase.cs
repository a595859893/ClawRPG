// ============================================
// Relic Database - 遗物数据库
// 已废弃: 请使用 RelicConfigLoader
// ============================================

using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Relics
{
    /// <summary>
    /// 遗物数据库 (已废弃，请使用 RelicConfigLoader)
    /// 此类现在作为 RelicConfigLoader 的代理，提供向后兼容
    /// </summary>
    [Obsolete("请使用 RelicConfigLoader 替代 RelicCollectionDatabase")]
    public static class RelicCollectionDatabase
    {
        /// <summary>
        /// 获取所有遗物
        /// </summary>
        public static Dictionary<string, Relic> Relics => RelicConfigLoader.Relics;

        /// <summary>
        /// 获取所有套装
        /// </summary>
        public static Dictionary<string, RelicSet> RelicSets => RelicConfigLoader.RelicSets;

        /// <summary>
        /// 获取生成配置
        /// </summary>
        public static RelicGenerationConfig GenerationConfig => RelicConfigLoader.GenerationConfig;

        /// <summary>
        /// 获取遗物稀有度颜色
        /// </summary>
        public static string GetRarityColor(RelicRarity rarity)
        {
            return RelicConfigLoader.GetRarityColor(rarity);
        }

        /// <summary>
        /// 获取随机遗物
        /// </summary>
        public static Relic GetRandomRelic()
        {
            return RelicConfigLoader.GetRandomRelic();
        }
    }
}
