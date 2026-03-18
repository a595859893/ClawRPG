using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.FateWeaving {

    /// <summary>
    /// 子系统: 结果计算器
    /// 负责结果计算、奖励/惩罚计算
    /// </summary>
    public class FateOutcomeCalculator : BaseSystem {

        private static FateOutcomeCalculator _instance;
        public static new FateOutcomeCalculator Instance {
            get {
                if (_instance == null) _instance = new FateOutcomeCalculator();
                return _instance;
            }
        }

        private FateCardDatabase _cardDatabase;
        private FateWeavingEngine _engine;

        public override void _Ready() {
            Instance = this;
            _cardDatabase = FateCardDatabase.Instance;
            _engine = FateWeavingEngine.Instance;
            base._Ready();
        }

        protected override void Initialize() {
            _cardDatabase = FateCardDatabase.Instance;
            _engine = FateWeavingEngine.Instance;
            base.Initialize();
        }

        /// <summary>
        /// 计算选择的结果数据
        /// </summary>
        /// <param name="choice">所做的选择</param>
        /// <param name="currentAffinity">当前路径亲和度</param>
        /// <param name="currentStats">当前属性值</param>
        /// <returns>结果数据</returns>
        public ChoiceOutcome CalculateChoiceOutcome(
            FateChoice choice,
            Dictionary<FatePathType, float> currentAffinity,
            Dictionary<string, float> currentStats) {

            var outcome = new ChoiceOutcome {
                ChoiceId = choice.Id,
                ChoiceTitle = choice.Title,
                ConsequenceDescription = choice.ConsequenceDescription
            };

            // 计算路径亲和度变化
            outcome.PathAffinityChanges = new Dictionary<FatePathType, float>();
            foreach (var influence in choice.PathInfluence) {
                outcome.PathAffinityChanges[influence.Key] = influence.Value;
            }

            // 应用路径亲和度变化
            outcome.NewAffinity = _engine.ApplyPathInfluence(currentAffinity, choice);

            // 计算新的主导路径
            outcome.NewDominantPath = _engine.CalculateDominantPath(outcome.NewAffinity);

            // 计算属性加成变化
            outcome.StatBonusChanges = new Dictionary<string, float>();
            foreach (var stat in choice.StatBonuses) {
                outcome.StatBonusChanges[stat.Key] = stat.Value;
            }

            // 应用属性加成变化
            outcome.NewStats = _engine.ApplyStatBonuses(currentStats, choice);

            // 计算总属性加成
            outcome.TotalStatBonus = 0f;
            foreach (var stat in outcome.NewStats.Values) {
                outcome.TotalStatBonus += stat;
            }

            // 计算路径加成
            outcome.PathBonuses = new Dictionary<string, float>();
            var dominantPathData = _cardDatabase.GetPath(outcome.NewDominantPath);
            if (dominantPathData != null && dominantPathData.PathBonuses != null) {
                foreach (var bonus in dominantPathData.PathBonuses) {
                    outcome.PathBonuses[bonus.Key] = bonus.Value;
                }
            }

            // 判断是否有路径转变
            outcome.PathChanged = true;
            foreach (var kvp in currentAffinity) {
                if (Math.Abs(outcome.NewAffinity[kvp.Key] - kvp.Value) > 0.001f) {
                    outcome.PathChanged = false;
                    break;
                }
            }

            // 判断是否有主导路径变化
            FatePathType currentDominant = _engine.CalculateDominantPath(currentAffinity);
            outcome.DominantPathChanged = (currentDominant != outcome.NewDominantPath);

            // 判断是否触发等级提升
            outcome.LeveledUp = false;

            return outcome;
        }

        /// <summary>
        /// 计算最终路径报告
        /// </summary>
        /// <param name="finalAffinity">最终路径亲和度</param>
        /// <param name="madeChoices">已做的选择 ID 列表</param>
        /// <returns>路径报告</returns>
        public PathReport CalculateFinalPathReport(
            Dictionary<FatePathType, float> finalAffinity,
            List<string> madeChoices) {

            var report = new PathReport {
                DominantPath = _engine.CalculateDominantPath(finalAffinity),
                PathRanking = _engine.GetPathAffinityRanking(finalAffinity),
                TotalChoices = madeChoices.Count,
                MadeChoiceIds = new List<string>(madeChoices)
            };

            // 统计各类型选择的数量
            report.ChoicesByType = new Dictionary<FateChoiceType, int>();
            foreach (FateChoiceType type in Enum.GetValues(typeof(FateChoiceType))) {
                report.ChoicesByType[type] = 0;
            }

            foreach (var choiceId in madeChoices) {
                var choice = _cardDatabase.GetChoiceById(choiceId);
                if (choice != null && report.ChoicesByType.ContainsKey(choice.ChoiceType)) {
                    report.ChoicesByType[choice.ChoiceType]++;
                }
            }

            // 获取主导路径数据
            var dominantPathData = _cardDatabase.GetPath(report.DominantPath);
            if (dominantPathData != null) {
                report.DominantPathName = dominantPathData.Name;
                report.DominantPathDescription = dominantPathData.Description;
                report.DominantPathBonuses = dominantPathData.PathBonuses;
            }

            // 计算最高亲和度值
            report.HighestAffinity = 0f;
            foreach (var affinity in finalAffinity.Values) {
                if (affinity > report.HighestAffinity) {
                    report.HighestAffinity = affinity;
                }
            }

            // 计算路径平衡度（亲和度分布的均匀程度）
            float sum = 0f;
            foreach (var affinity in finalAffinity.Values) {
                sum += affinity;
            }
            float avg = sum / finalAffinity.Count;
            float variance = 0f;
            foreach (var affinity in finalAffinity.Values) {
                float diff = affinity - avg;
                variance += diff * diff;
            }
            report.PathBalance = 1f - (float)Math.Sqrt(variance / finalAffinity.Count) / Math.Max(1f, avg);

            return report;
        }

        /// <summary>
        /// 计算路径加成效果
        /// </summary>
        /// <param name="dominantPath">主导路径类型</param>
        /// <param name="bonusType">加成类型</param>
        /// <returns>加成数值</returns>
        public float GetPathBonusValue(FatePathType dominantPath, string bonusType) {
            var pathData = _cardDatabase.GetPath(dominantPath);
            if (pathData != null && pathData.PathBonuses != null &&
                pathData.PathBonuses.ContainsKey(bonusType)) {
                return pathData.PathBonuses[bonusType];
            }
            return 0f;
        }

        /// <summary>
        /// 计算某路径的解锁状态
        /// </summary>
        /// <param name="pathType">路径类型</param>
        /// <param name="weaveLevel">当前编织等级</param>
        /// <returns>是否已解锁</returns>
        public bool IsPathUnlocked(FatePathType pathType, int weaveLevel) {
            var pathData = _cardDatabase.GetPath(pathType);
            if (pathData == null) return false;
            return weaveLevel >= pathData.UnlockTier;
        }

        /// <summary>
        /// 计算某选择所需的最小等级
        /// </summary>
        /// <param name="choice">选择</param>
        /// <returns>所需最小等级</returns>
        public int GetRequiredLevel(FateChoice choice) {
            return choice != null ? choice.TierRequired : 0;
        }

        /// <summary>
        /// 计算总路径加成值（所有已解锁路径的加成总和）
        /// </summary>
        /// <param name="unlockedPaths">已解锁的路径类型列表</param>
        /// <returns>总加成值</returns>
        public float CalculateTotalPathBonus(List<FatePathType> unlockedPaths) {
            float total = 0f;
            foreach (var pathType in unlockedPaths) {
                var pathData = _cardDatabase.GetPath(pathType);
                if (pathData != null && pathData.PathBonuses != null) {
                    foreach (var bonus in pathData.PathBonuses.Values) {
                        total += bonus;
                    }
                }
            }
            return total;
        }

        /// <summary>
        /// 计算选择的一致性评分（与主导路径的契合度）
        /// </summary>
        /// <param name="choice">选择</param>
        /// <param name="dominantPath">主导路径</param>
        /// <returns>一致性评分，0.0 到 1.0</returns>
        public float CalculateConsistencyScore(FateChoice choice, FatePathType dominantPath) {
            if (choice == null || !choice.PathInfluence.ContainsKey(dominantPath)) {
                return 0f;
            }

            float dominantInfluence = choice.PathInfluence[dominantPath];
            float totalInfluence = 0f;
            foreach (var influence in choice.PathInfluence.Values) {
                totalInfluence += influence;
            }

            return totalInfluence > 0 ? dominantInfluence / totalInfluence : 0f;
        }

        public override Dictionary ExportSaveData() {
            // 结果计算器无持久化状态（纯计算）
            return new Dictionary();
        }

        public override void ImportSaveData(Dictionary data) {
            // 无状态，无需处理
        }

        public override void Reset() {
            base.Reset();
        }
    }

    /// <summary>
    /// 选择结果数据
    /// </summary>
    public class ChoiceOutcome {
        public string ChoiceId { get; set; }
        public string ChoiceTitle { get; set; }
        public string ConsequenceDescription { get; set; }
        public Dictionary<FatePathType, float> PathAffinityChanges { get; set; }
        public Dictionary<FatePathType, float> NewAffinity { get; set; }
        public FatePathType NewDominantPath { get; set; }
        public Dictionary<string, float> StatBonusChanges { get; set; }
        public Dictionary<string, float> NewStats { get; set; }
        public float TotalStatBonus { get; set; }
        public Dictionary<string, float> PathBonuses { get; set; }
        public bool PathChanged { get; set; }
        public bool DominantPathChanged { get; set; }
        public bool LeveledUp { get; set; }
    }

    /// <summary>
    /// 最终路径报告
    /// </summary>
    public class PathReport {
        public FatePathType DominantPath { get; set; }
        public string DominantPathName { get; set; }
        public string DominantPathDescription { get; set; }
        public Dictionary<string, float> DominantPathBonuses { get; set; }
        public List<FatePathType> PathRanking { get; set; }
        public float HighestAffinity { get; set; }
        public float PathBalance { get; set; }
        public int TotalChoices { get; set; }
        public List<string> MadeChoiceIds { get; set; }
        public Dictionary<FateChoiceType, int> ChoicesByType { get; set; }
    }
}
