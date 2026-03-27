using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.ProceduralDungeon;

namespace ClawRPG.Scripts.Systems.PetMimicry
{
    /// <summary>
    /// 房间环境分类器 — 将 DungeonRoom + RoomType 映射为 RoomEnvironmentType
    /// 可扩展：未来可接入房间元数据、敌人类型、装饰物标签等做更精细分类
    /// </summary>
    public static class RoomEnvironmentClassifier
    {
        /// <summary>
        /// 根据房间对象和房间类型获取环境标签
        /// </summary>
        public static RoomEnvironmentType Classify(DungeonRoom room)
        {
            if (room == null)
                return RoomEnvironmentType.Normal;

            return ClassifyByRoomType(room.Type);
        }

        /// <summary>
        /// 根据 RoomType 进行基础分类
        /// </summary>
        public static RoomEnvironmentType ClassifyByRoomType(RoomType roomType)
        {
            return roomType switch
            {
                RoomType.Boss     => RoomEnvironmentType.Boss,
                RoomType.Treasure => RoomEnvironmentType.Treasure,
                RoomType.Rest     => RoomEnvironmentType.Rest,
                RoomType.Puzzle   => RoomEnvironmentType.Puzzle,
                RoomType.Elite    => RoomEnvironmentType.Elite,
                RoomType.Secret   => RoomEnvironmentType.Treasure | RoomEnvironmentType.Escape,
                RoomType.Trap     => RoomEnvironmentType.TrapDense,
                RoomType.Combat   => RoomEnvironmentType.Combat,
                RoomType.Entrance => RoomEnvironmentType.Entrance,
                RoomType.Corridor => RoomEnvironmentType.None,
                RoomType.Event    => RoomEnvironmentType.None,
                RoomType.Merchant => RoomEnvironmentType.Treasure,
                _                 => RoomEnvironmentType.Normal
            };
        }

        /// <summary>
        /// 根据名称/标签关键词推断环境类型（用于场景装饰物检测）
        /// </summary>
        public static RoomEnvironmentType ClassifyByKeyword(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return RoomEnvironmentType.None;

            var lower = keyword.ToLowerInvariant();
            if (lower.Contains("fire") || lower.Contains("lava") || lower.Contains("flame"))
                return RoomEnvironmentType.Fire;
            if (lower.Contains("ice") || lower.Contains("frost") || lower.Contains("snow"))
                return RoomEnvironmentType.Ice;
            if (lower.Contains("trap") || lower.Contains("spike") || lower.Contains("dart"))
                return RoomEnvironmentType.TrapDense;
            if (lower.Contains("boss"))
                return RoomEnvironmentType.Boss;
            if (lower.Contains("treasure") || lower.Contains("chest") || lower.Contains("gold"))
                return RoomEnvironmentType.Treasure;
            if (lower.Contains("rest") || lower.Contains("campfire") || lower.Contains("heal"))
                return RoomEnvironmentType.Rest;
            if (lower.Contains("puzzle"))
                return RoomEnvironmentType.Puzzle;
            if (lower.Contains("elite"))
                return RoomEnvironmentType.Elite;
            if (lower.Contains("escape") || lower.Contains("exit") || lower.Contains("retreat"))
                return RoomEnvironmentType.Escape;
            if (lower.Contains("poison") || lower.Contains("toxic"))
                return RoomEnvironmentType.Poison;
            if (lower.Contains("electric") || lower.Contains("lightning") || lower.Contains("coil"))
                return RoomEnvironmentType.Electric;
            if (lower.Contains("shadow") || lower.Contains("void") || lower.Contains("dark"))
                return RoomEnvironmentType.Shadow;
            if (lower.Contains("holy") || lower.Contains("sacred"))
                return RoomEnvironmentType.Holy;
            if (lower.Contains("nature") || lower.Contains("vine") || lower.Contains("moss"))
                return RoomEnvironmentType.Nature;

            return RoomEnvironmentType.None;
        }

        /// <summary>
        /// 获取环境类型的显示名称
        /// </summary>
        public static string GetDisplayName(RoomEnvironmentType type)
        {
            return type switch
            {
                RoomEnvironmentType.Fire      => "🔥 火系",
                RoomEnvironmentType.Ice      => "❄️ 冰系",
                RoomEnvironmentType.TrapDense => "⚠️ 陷阱区",
                RoomEnvironmentType.Boss      => "☠️ Boss房",
                RoomEnvironmentType.Escape    => "🚪 撤退区",
                RoomEnvironmentType.Treasure  => "💎 宝藏房",
                RoomEnvironmentType.Rest      => "🏕️ 休息区",
                RoomEnvironmentType.Puzzle     => "🧩 谜题房",
                RoomEnvironmentType.Elite     => "⚔️ 精英区",
                RoomEnvironmentType.Combat    => "⚔️ 战斗区",
                RoomEnvironmentType.Entrance   => "🚪 入口",
                RoomEnvironmentType.Poison    => "☠️ 毒系",
                RoomEnvironmentType.Electric  => "⚡ 电系",
                RoomEnvironmentType.Shadow    => "👁️ 暗系",
                RoomEnvironmentType.Holy      => "✨ 神圣系",
                RoomEnvironmentType.Nature    => "🌿 自然系",
                RoomEnvironmentType.None      => "普通",
                _                              => type.ToString()
            };
        }
    }
}
