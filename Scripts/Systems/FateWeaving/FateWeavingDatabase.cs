using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.FateWeaving {

    /// <summary>
    /// FateWeavingDatabase - 命运编织协调者 (Facade/Coordinator)
    /// 委托给三个子系统:
    ///   - FateCardDatabase: 卡牌数据存储
    ///   - FateWeavingEngine: 编织逻辑与路径计算
    ///   - FateOutcomeCalculator: 结果计算与奖励/惩罚计算
    ///
    /// 保留向后兼容的 public API，底层委托给相应子系统
    /// </summary>
    public class FateWeavingDatabase : GodotObject {

        private static FateWeavingDatabase _instance;
        public static FateWeavingDatabase Instance {
            get {
                if (_instance == null) _instance = new FateWeavingDatabase();
                return _instance;
            }
        }

        // 子系统引用
        private FateCardDatabase _cardDatabase;
        private FateWeavingEngine _engine;
        private FateOutcomeCalculator _calculator;

        /// <summary>
        /// 所有命运路径的定义数据（委托给 FateCardDatabase）
        /// </summary>
        public List<FatePathData> Paths {
            get { return _cardDatabase?.Paths ?? new List<FatePathData>(); }
        }

        /// <summary>
        /// 所有选择的定义数据（委托给 FateCardDatabase）
        /// </summary>
        public List<FateChoice> Choices {
            get { return _cardDatabase?.Choices ?? new List<FateChoice>(); }
        }

        /// <summary>
        /// FateCardDatabase 子系统实例
        /// </summary>
        public FateCardDatabase CardDatabase => _cardDatabase;

        /// <summary>
        /// FateWeavingEngine 子系统实例
        /// </summary>
        public FateWeavingEngine Engine => _engine;

        /// <summary>
        /// FateOutcomeCalculator 子系统实例
        /// </summary>
        public FateOutcomeCalculator Calculator => _calculator;

        public FateWeavingDatabase() {
            // 初始化三个子系统（单例延迟初始化）
            _cardDatabase = FateCardDatabase.Instance;
            _engine = FateWeavingEngine.Instance;
            _calculator = FateOutcomeCalculator.Instance;

            // 如果子系统尚未初始化，触发初始化
            if (!_cardDatabase.IsInitialized) {
                _cardDatabase.Initialize();
            }
            if (!_engine.IsInitialized) {
                _engine.Initialize();
            }
            if (!_calculator.IsInitialized) {
                _calculator.Initialize();
            }
        }

        // ==================== 向后兼容的 API ====================
        // 以下方法保持原有签名，委托给相应子系统

        /// <summary>
        /// 获取随机可用的选择
        /// </summary>
        /// <param name="playerTier">玩家当前层级</param>
        /// <returns>随机选择</returns>
        public FateChoice GetRandomChoice(int playerTier) {
            return _engine?.GetRandomChoice(playerTier, true);
        }

        /// <summary>
        /// 获取所有当前可用的选择
        /// </summary>
        /// <param name="playerTier">玩家当前层级</param>
        /// <returns>可用选择列表</returns>
        public List<FateChoice> GetAvailableChoices(int playerTier) {
            return _engine?.GetAvailableChoices(playerTier, true) ?? new List<FateChoice>();
        }

        /// <summary>
        /// 根据路径类型获取路径数据
        /// </summary>
        /// <param name="type">路径类型</param>
        /// <returns>路径数据</returns>
        public FatePathData GetPath(FatePathType type) {
            return _cardDatabase?.GetPath(type);
        }

        // ==================== 扩展 API（委托给子系统） ====================

        /// <summary>
        /// 获取指定类型的所有选择（委托给 FateCardDatabase）
        /// </summary>
        public List<FateChoice> GetChoicesByType(FateChoiceType type) {
            return _cardDatabase?.GetChoicesByType(type) ?? new List<FateChoice>();
        }

        /// <summary>
        /// 根据 ID 获取选择（委托给 FateCardDatabase）
        /// </summary>
        public FateChoice GetChoiceById(string id) {
            return _cardDatabase?.GetChoiceById(id);
        }

        /// <summary>
        /// 计算选择结果（委托给 FateOutcomeCalculator）
        /// </summary>
        public ChoiceOutcome CalculateChoiceOutcome(
            FateChoice choice,
            Dictionary<FatePathType, float> currentAffinity,
            Dictionary<string, float> currentStats) {
            return _calculator?.CalculateChoiceOutcome(choice, currentAffinity, currentStats);
        }

        /// <summary>
        /// 计算最终路径报告（委托给 FateOutcomeCalculator）
        /// </summary>
        public PathReport CalculateFinalPathReport(
            Dictionary<FatePathType, float> finalAffinity,
            List<string> madeChoices) {
            return _calculator?.CalculateFinalPathReport(finalAffinity, madeChoices);
        }

        /// <summary>
        /// 获取路径加成值（委托给 FateOutcomeCalculator）
        /// </summary>
        public float GetPathBonusValue(FatePathType dominantPath, string bonusType) {
            return _calculator?.GetPathBonusValue(dominantPath, bonusType) ?? 0f;
        }

        /// <summary>
        /// 计算路径是否解锁（委托给 FateOutcomeCalculator）
        /// </summary>
        public bool IsPathUnlocked(FatePathType pathType, int weaveLevel) {
            return _calculator?.IsPathUnlocked(pathType, weaveLevel) ?? false;
        }

        /// <summary>
        /// 获取一致性评分（委托给 FateOutcomeCalculator）
        /// </summary>
        public float CalculateConsistencyScore(FateChoice choice, FatePathType dominantPath) {
            return _calculator?.CalculateConsistencyScore(choice, dominantPath) ?? 0f;
        }

        /// <summary>
        /// 计算编织等级（委托给 FateWeavingEngine）
        /// </summary>
        public int CalculateWeaveLevel(int totalWeaves) {
            return _engine?.CalculateWeaveLevel(totalWeaves) ?? 1;
        }

        /// <summary>
        /// 获取路径亲和度排名（委托给 FateWeavingEngine）
        /// </summary>
        public List<FatePathType> GetPathAffinityRanking(Dictionary<FatePathType, float> pathAffinity) {
            return _engine?.GetPathAffinityRanking(pathAffinity) ?? new List<FatePathType>();
        }

        /// <summary>
        /// 重置所有子系统数据
        /// </summary>
        public void ResetAll() {
            _cardDatabase?.Reset();
            _engine?.Reset();
            _calculator?.Reset();
        }
    }
}
