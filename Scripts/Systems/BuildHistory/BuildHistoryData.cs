using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.BuildHistory
{
    /// <summary>
    /// 高光时刻类型
    /// </summary>
    public enum HighlightType
    {
        MaxCombo,          // 最大连击
        BossKill,          // Boss 击杀
        Clutch,            // 极限翻盘（低血量反杀）
        FirstComboUse,     // 首次使用某 combo
        ComboStreak,       // 连击连胜
        PerfectBlock,       // 完美格挡
        EnemyTypeKill      // 击杀特定类型敌人
    }

    /// <summary>
    /// 低谷时刻类型
    /// </summary>
    public enum LowlightType
    {
        ComboFailure,      // Combo 失败
        LossStreak,       // 连败
        NearDeath,         // 险死还生（并未翻盘）
        ForcedItem,        // 迫使用掉关键道具
        BossEscaped        // Boss 逃跑
    }

    /// <summary>
    /// 高光时刻记录
    /// </summary>
    [System.Serializable]
    public class HighlightMoment
    {
        public HighlightType Type;
        public string Title;          // 展示标题
        public string NarrativeText;  // 叙事文本
        public int Value;            // 数值（如 47 连击）
        public string Tag;          // 额外标签（如 combo id、boss id）
        public long Timestamp;       // 时间戳
    }

    /// <summary>
    /// 低谷时刻记录
    /// </summary>
    [System.Serializable]
    public class LowlightMoment
    {
        public LowlightType Type;
        public string Title;
        public string NarrativeText;
        public int Value;
        public string Tag;
        public long Timestamp;
    }

    /// <summary>
    /// 单次 run 的 Build 历史记录
    /// </summary>
    [System.Serializable]
    public class BuildHistoryEntry
    {
        public int RunIndex;
        public bool Victory;
        public long StartTime;
        public long EndTime;

        // 数值统计
        public int TotalEnemiesDefeated;
        public int TotalDamageDealt;
        public int TotalDamageTaken;
        public int MaxComboAchieved;
        public int BossesKilled;
        public int ComboFailures;
        public int CurrentWinStreak;
        public int CurrentLossStreak;
        public float FinalHealthPercent;

        // 叙事时刻
        public List<HighlightMoment> HighlightMoments = new List<HighlightMoment>();
        public List<LowlightMoment> LowlightMoments = new List<LowlightMoment>();

        // 标记：用于去重
        public HashSet<string> SeenComboIds = new HashSet<string>();
        public HashSet<string> SeenBossIds = new HashSet<string>();
    }

    /// <summary>
    /// Build 历史保存数据
    /// </summary>
    [System.Serializable]
    public class BuildHistorySaveData
    {
        public List<BuildHistoryEntry> HistoryEntries = new List<BuildHistoryEntry>();
        public int TotalRunsRecorded;
        public int AllTimeMaxCombo;
        public int AllTimeBestWinStreak;
    }

    /// <summary>
    /// Build 历史数据库 — 预写叙事模板
    /// </summary>
    public class BuildHistoryDatabase
    {
        private static BuildHistoryDatabase _instance;
        public static BuildHistoryDatabase Instance => _instance ??= new BuildHistoryDatabase();

        private BuildHistoryDatabase() { }

        #region Highlight Templates

        /// <summary>
        /// 生成高光叙事文本
        /// </summary>
        public string GenerateHighlightNarrative(HighlightMoment moment, int runIndex)
        {
            switch (moment.Type)
            {
                case HighlightType.MaxCombo:
                    return $"第 {runIndex} 次轮回，你在战斗中打出了 {moment.Value} 连击——这是你的最高光时刻。";

                case HighlightType.BossKill:
                    return $"在第 {runIndex} 次轮回中，你击败了 {moment.Tag}，证明了自己的实力。";

                case HighlightType.Clutch:
                    return $"第 {runIndex} 次轮回，你在 {moment.Tag} 的围攻中将血量从 {moment.Value}% 拉回——绝境中的反击。";

                case HighlightType.FirstComboUse:
                    return $"第 {runIndex} 次轮回，你首次尝试了 {moment.Tag} 套路——新的可能性被打开。";

                case HighlightType.ComboStreak:
                    return $"第 {runIndex} 次轮回，你保持了 {moment.Value} 连胜，这场战斗你无人能挡。";

                case HighlightType.PerfectBlock:
                    return $"第 {runIndex} 次轮回，你在 {moment.Tag} 的攻击中完美格挡——{moment.Value} 次无一失误。";

                case HighlightType.EnemyTypeKill:
                    return $"第 {runIndex} 次轮回，{moment.Tag} 倒在了你的手下，这个敌人不会再回来了。";

                default:
                    return $"第 {runIndex} 次轮回，发生了某件值得铭记的事。";
            }
        }

        /// <summary>
        /// 生成低谷叙事文本
        /// </summary>
        public string GenerateLowlightNarrative(LowlightMoment moment, int runIndex)
        {
            switch (moment.Type)
            {
                case LowlightType.ComboFailure:
                    return $"第 {runIndex} 次轮回，{moment.Tag} 在你手下失败了 {moment.Value} 次——它似乎在嘲笑你。";

                case LowlightType.LossStreak:
                    return $"第 {runIndex} 次轮回，你经历了 {moment.Value} 连败，这场轮回似乎从一开始就注定失败。";

                case LowlightType.NearDeath:
                    return $"第 {runIndex} 次轮回，你险些死去 {moment.Value} 次，每一次都惊险万分。";

                case LowlightType.ForcedItem:
                    return $"第 {runIndex} 次轮回，你被迫提前用掉了 {moment.Tag}——资源管理出了问题。";

                case LowlightType.BossEscaped:
                    return $"第 {runIndex} 次轮回，{moment.Tag} 从你手中逃走了——下次不会再有这样的机会。";

                default:
                    return $"第 {runIndex} 次轮回，发生了某些令人遗憾的事。";
            }
        }

        #endregion

        #region Run Summary Generation

        /// <summary>
        /// 生成 run 总结叙事文本
        /// </summary>
        public string GenerateRunSummaryNarrative(BuildHistoryEntry entry)
        {
            var lines = new List<string>();

            // 高光时刻总结
            if (entry.HighlightMoments.Count > 0)
            {
                var top = entry.HighlightMoments[0]; // 数值最高的一个
                lines.Add(GenerateHighlightNarrative(top, entry.RunIndex));
            }

            // 低谷时刻总结（如果存在）
            if (entry.LowlightMoments.Count > 0)
            {
                var worst = entry.LowlightMoments[0];
                lines.Add(GenerateLowlightNarrative(worst, entry.RunIndex));
            }

            // 整体感觉
            if (entry.Victory)
            {
                if (entry.MaxComboAchieved >= 20)
                    lines.Add($"最终，你带着 {entry.MaxComboAchieved} 连击的荣耀结束了这一轮回。");
                else
                    lines.Add("最终，你艰难地取得了胜利，这一轮回将被铭记。");
            }
            else
            {
                if (entry.CurrentLossStreak >= 3)
                    lines.Add($"你已经连败 {entry.CurrentLossStreak} 场，下一次轮回会不一样吗？");
                else
                    lines.Add("这一轮回结束了，但你还会回来。");
            }

            return string.Join("\n", lines);
        }

        #endregion
    }
}
