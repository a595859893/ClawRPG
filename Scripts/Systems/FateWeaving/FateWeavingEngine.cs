using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.FateWeaving {

    /// <summary>
    /// 子系统: 编织引擎
    /// 负责核心编织逻辑、卡牌生成、路径计算
    /// </summary>
    public partial class FateWeavingEngine : BaseSystem {

        private static FateWeavingEngine _instance;
        public static new FateWeavingEngine Instance {
            get {
                if (_instance == null) _instance = new FateWeavingEngine();
                return _instance;
            }
        }

        private FateCardDatabase _cardDatabase;

        /// <summary>
        /// 已排除的选择 ID（已被玩家使用）
        /// </summary>
        private HashSet<string> _excludedChoices = new HashSet<string>();

        public override void _Ready() {
            Instance = this;
            _cardDatabase = FateCardDatabase.Instance;
            base._Ready();
        }

        protected override void Initialize() {
            _cardDatabase = FateCardDatabase.Instance;
            base.Initialize();
        }

        /// <summary>
        /// 获取一个随机可用的选择
        /// </summary>
        /// <param name="playerTier">玩家当前层级</param>
        /// <param name="excludeUsed">是否排除已使用的选择</param>
        /// <returns>随机选择，如果无可用选择则返回 null</returns>
        public FateChoice GetRandomChoice(int playerTier, bool excludeUsed = true) {
            var availableChoices = new List<FateChoice>();
            foreach (var choice in _cardDatabase.Choices) {
                if (choice.TierRequired <= playerTier) {
                    if (!excludeUsed || !_excludedChoices.Contains(choice.Id)) {
                        availableChoices.Add(choice);
                    }
                }
            }

            if (availableChoices.Count == 0) return null;

            var random = new Random();
            return availableChoices[random.Next(availableChoices.Count)];
        }

        /// <summary>
        /// 获取所有当前可用的选择
        /// </summary>
        /// <param name="playerTier">玩家当前层级</param>
        /// <param name="excludeUsed">是否排除已使用的选择</param>
        /// <returns>可用选择列表</returns>
        public List<FateChoice> GetAvailableChoices(int playerTier, bool excludeUsed = true) {
            var result = new List<FateChoice>();
            foreach (var choice in _cardDatabase.Choices) {
                if (choice.TierRequired <= playerTier) {
                    if (!excludeUsed || !_excludedChoices.Contains(choice.Id)) {
                        result.Add(choice);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 计算当前的主导路径
        /// </summary>
        /// <param name="pathAffinity">路径亲和度字典</param>
        /// <returns>亲和度最高的路径类型</returns>
        public FatePathType CalculateDominantPath(Dictionary<FatePathType, float> pathAffinity) {
            float highestAffinity = 0f;
            FatePathType dominant = FatePathType.Hero;

            foreach (var affinity in pathAffinity) {
                if (affinity.Value > highestAffinity) {
                    highestAffinity = affinity.Value;
                    dominant = affinity.Key;
                }
            }

            return dominant;
        }

        /// <summary>
        /// 计算编织等级
        /// </summary>
        /// <param name="totalWeaves">已完成的编织次数</param>
        /// <returns>编织等级 (最高 20)</returns>
        public int CalculateWeaveLevel(int totalWeaves) {
            return Math.Min(20, 1 + totalWeaves / 5);
        }

        /// <summary>
        /// 计算到达下一级需要的编织次数
        /// </summary>
        /// <param name="currentLevel">当前等级</param>
        /// <returns>下一级需要的次数</returns>
        public int GetWeavesToNextLevel(int currentLevel) {
            return (currentLevel + 1) * 5;
        }

        /// <summary>
        /// 标记一个选择为已使用
        /// </summary>
        public void MarkChoiceUsed(string choiceId) {
            if (!string.IsNullOrEmpty(choiceId)) {
                _excludedChoices.Add(choiceId);
            }
        }

        /// <summary>
        /// 重置所有已使用的选择
        /// </summary>
        public void ResetExcludedChoices() {
            _excludedChoices.Clear();
        }

        /// <summary>
        /// 检查是否已使用过某个选择
        /// </summary>
        public bool IsChoiceUsed(string choiceId) {
            return _excludedChoices.Contains(choiceId);
        }

        /// <summary>
        /// 获取指定路径的定义数据
        /// </summary>
        public FatePathData GetPathData(FatePathType type) {
            return _cardDatabase.GetPath(type);
        }

        /// <summary>
        /// 计算路径亲和度变化
        /// </summary>
        /// <param name="currentAffinity">当前亲和度字典</param>
        /// <param name="choice">所做的选择</param>
        /// <returns>更新后的亲和度字典（新实例）</returns>
        public Dictionary<FatePathType, float> ApplyPathInfluence(
            Dictionary<FatePathType, float> currentAffinity,
            FateChoice choice) {
            var result = new Dictionary<FatePathType, float>(currentAffinity);
            foreach (var influence in choice.PathInfluence) {
                if (result.ContainsKey(influence.Key)) {
                    result[influence.Key] += influence.Value;
                } else {
                    result[influence.Key] = influence.Value;
                }
            }
            return result;
        }

        /// <summary>
        /// 计算属性加成变化
        /// </summary>
        /// <param name="currentStats">当前属性字典</param>
        /// <param name="choice">所做的选择</param>
        /// <returns>更新后的属性字典（新实例）</returns>
        public Dictionary<string, float> ApplyStatBonuses(
            Dictionary<string, float> currentStats,
            FateChoice choice) {
            var result = new Dictionary<string, float>(currentStats);
            foreach (var stat in choice.StatBonuses) {
                if (result.ContainsKey(stat.Key)) {
                    result[stat.Key] += stat.Value;
                } else {
                    result[stat.Key] = stat.Value;
                }
            }
            return result;
        }

        /// <summary>
        /// 计算经验进度
        /// </summary>
        /// <param name="totalWeaves">已完成的编织次数</param>
        /// <param name="weaveLevel">当前编织等级</param>
        /// <returns>0.0 到 1.0 之间的进度值</returns>
        public float CalculateExperienceProgress(int totalWeaves, int weaveLevel) {
            int weavesInCurrentLevel = totalWeaves % 5;
            int expNeeded = weaveLevel * 5;
            return (float)weavesInCurrentLevel / expNeeded;
        }

        /// <summary>
        /// 获取路径亲和度排名
        /// </summary>
        /// <param name="pathAffinity">路径亲和度字典</param>
        /// <returns>从高到低排序的路径列表</returns>
        public List<FatePathType> GetPathAffinityRanking(Dictionary<FatePathType, float> pathAffinity) {
            var sorted = new List<KeyValuePair<FatePathType, float>>(pathAffinity);
            sorted.Sort((a, b) => b.Value.CompareTo(a.Value));
            var result = new List<FatePathType>();
            foreach (var kvp in sorted) {
                result.Add(kvp.Key);
            }
            return result;
        }

        /// <summary>
        /// 重置引擎状态
        /// </summary>
        public override void Reset() {
            _excludedChoices.Clear();
            base.Reset();
        }

        public override Dictionary<string, object> ExportSaveData() {
            var data = new Dictionary<string, object>();
            var excludedList = new Godot.Collections.Array();
            foreach (var id in _excludedChoices) {
                excludedList.Add(id);
            }
            data["excluded_choices"] = excludedList;
            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data) {
            if (data == null) return;
            _excludedChoices.Clear();

            if (data.Contains("excluded_choices")) {
                var excludedList = data["excluded_choices"] as Godot.Array;
                foreach (var item in excludedList) {
                    _excludedChoices.Add(item.ToString());
                }
            }
        }
    }
}
