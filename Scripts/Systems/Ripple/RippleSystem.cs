using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Ripple {
    /// <summary>
    /// RippleSystem - 涟漪点核心管理系统
    /// 「这片土地在回应你」
    /// </summary>
    public partial class RippleSystem : BaseSystem {
        public static RippleSystem Instance { get; private set; }

        // ========== 信号定义 ==========
        [Signal]
        public delegate void RippleAddedEventHandler(RippleType type, int amount, int newTotal);

        [Signal]
        public delegate void RippleDecayedEventHandler(RippleType type, int decayedAmount, int remaining);

        [Signal]
        public delegate void RippleHintVisibleEventHandler(RippleType type);

        [Signal]
        public delegate void RippleHintHiddenEventHandler(RippleType type);

        // ========== 配置 ==========
        [Export] private int defaultThreshold = 100;           // 默认触发阈值
        [Export] private float defaultDecayRate = 0.1f;       // 默认衰减率（10%）
        [Export] private float hudHintThreshold = 0.7f;        // 预兆显示阈值（70%）

        // ========== 涟漪数据 ==========
        private Dictionary<RippleType, RipplePointData> _ripplePoints = new Dictionary<RippleType, RipplePointData>();

        // ========== 阈值配置 ==========
        private Dictionary<RippleType, int> _thresholds = new Dictionary<RippleType, int>();
        private Dictionary<RippleType, float> _decayRates = new Dictionary<RippleType, float>();

        // ========== 预兆状态 ==========
        private Dictionary<RippleType, bool> _hudHintActive = new Dictionary<RippleType, bool>();

        public override void _Ready() {
            Instance = this;
            InitializeRippleTypes();
        }

        private void InitializeRippleTypes() {
            // 初始化每种涟漪类型
            foreach (RippleType type in Enum.GetValues(typeof(RippleType))) {
                _ripplePoints[type] = new RipplePointData(type);
                _thresholds[type] = defaultThreshold;
                _decayRates[type] = defaultDecayRate;
                _hudHintActive[type] = false;
            }

            // 配置各类型的阈值（根据行为稀有度调整）
            _thresholds[RippleType.Sacrifice] = 50;    // 宠物牺牲较稀有，低阈值
            _thresholds[RippleType.Triumph] = 150;     // 完美胜利较稀有，高阈值
            _thresholds[RippleType.Desperation] = 80;  // 连续失败容易积累，中等阈值
            _thresholds[RippleType.Loss] = 60;         // combo 放弃较频繁
            _thresholds[RippleType.Abandon] = 100;    // 跳过商人/遗物
            _thresholds[RippleType.Forget] = 120;     // 跳过冥想

            // 配置各类型的衰减率
            _decayRates[RippleType.Triumph] = 0.05f;    // 胜利荣耀持久
            _decayRates[RippleType.Sacrifice] = 0.08f;   // 牺牲记忆持久
            _decayRates[RippleType.Desperation] = 0.12f; // 失败感快速消退
            _decayRates[RippleType.Loss] = 0.15f;       // 放弃感较快消退
            _decayRates[RippleType.Abandon] = 0.10f;
            _decayRates[RippleType.Forget] = 0.10f;
        }

        // ========== 公开 API ==========

        /// <summary>
        /// 添加涟漪点
        /// </summary>
        public void AddRipple(RippleType type, int amount) {
            if (!_ripplePoints.ContainsKey(type)) {
                _ripplePoints[type] = new RipplePointData(type);
            }

            var ripple = _ripplePoints[type];
            ripple.AddPoints(amount);

            EmitSignal("RippleAdded", type, amount, ripple.Points);

            // 检查预兆显示
            CheckHudHint(type);

            GD.Print($"[Ripple] {type} +{amount} → {ripple.Points} pts (threshold: {_thresholds[type]})");
        }

        /// <summary>
        /// 衰减所有涟漪点
        /// </summary>
        public void DecayAll(float multiplier = 1.0f) {
            foreach (RippleType type in Enum.GetValues(typeof(RippleType))) {
                if (_ripplePoints.ContainsKey(type)) {
                    var ripple = _ripplePoints[type];
                    int oldPoints = ripple.Points;
                    float rate = _decayRates[type] * multiplier;
                    ripple.Decay(rate);
                    if (ripple.Points < oldPoints) {
                        EmitSignal("RippleDecayed", type, oldPoints - ripple.Points, ripple.Points);
                    }
                }
            }
        }

        /// <summary>
        /// 衰减指定类型的涟漪点
        /// </summary>
        public void DecayRipple(RippleType type, float multiplier = 1.0f) {
            if (!_ripplePoints.ContainsKey(type)) return;
            var ripple = _ripplePoints[type];
            int oldPoints = ripple.Points;
            ripple.Decay(_decayRates[type] * multiplier);
            if (ripple.Points < oldPoints) {
                EmitSignal("RippleDecayed", type, oldPoints - ripple.Points, ripple.Points);
                CheckHudHint(type);
            }
        }

        /// <summary>
        /// 获取涟漪加权权重（用于链触发概率计算）
        /// </summary>
        public float GetRippleWeight(RippleType type) {
            if (!_ripplePoints.ContainsKey(type)) return 0f;
            int threshold = _thresholds.ContainsKey(type) ? _thresholds[type] : defaultThreshold;
            return _ripplePoints[type].GetNormalizedWeight(threshold);
        }

        /// <summary>
        /// 获取涟漪点
        /// </summary>
        public int GetRipplePoints(RippleType type) {
            if (!_ripplePoints.ContainsKey(type)) return 0;
            return _ripplePoints[type].Points;
        }

        /// <summary>
        /// 获取所有涟漪点
        /// </summary>
        public Dictionary<RippleType, int> GetAllRipplePoints() {
            var result = new Dictionary<RippleType, int>();
            foreach (var kvp in _ripplePoints) {
                result[kvp.Key] = kvp.Value.Points;
            }
            return result;
        }

        /// <summary>
        /// 获取阈值
        /// </summary>
        public int GetThreshold(RippleType type) {
            return _thresholds.ContainsKey(type) ? _thresholds[type] : defaultThreshold;
        }

        /// <summary>
        /// 获取预兆是否应该显示
        /// </summary>
        public bool GetHudHint(RippleType type) {
            if (!_ripplePoints.ContainsKey(type)) return false;
            int threshold = _thresholds.ContainsKey(type) ? _thresholds[type] : defaultThreshold;
            return _ripplePoints[type].IsHudHintVisible(threshold);
        }

        /// <summary>
        /// 检查并更新预兆状态
        /// </summary>
        private void CheckHudHint(RippleType type) {
            bool shouldShow = GetHudHint(type);
            bool currentlyShowing = _hudHintActive.ContainsKey(type) && _hudHintActive[type];

            if (shouldShow && !currentlyShowing) {
                _hudHintActive[type] = true;
                EmitSignal("RippleHintVisible", type);
            } else if (!shouldShow && currentlyShowing) {
                _hudHintActive[type] = false;
                EmitSignal("RippleHintHidden", type);
            }
        }

        /// <summary>
        /// 获取当前活跃的预兆类型列表
        /// </summary>
        public List<RippleType> GetActiveHints() {
            var hints = new List<RippleType>();
            foreach (var kvp in _hudHintActive) {
                if (kvp.Value) hints.Add(kvp.Key);
            }
            return hints;
        }

        /// <summary>
        /// 获取涟漪点对应的 HUD 图标名称
        /// </summary>
        public string GetHintIcon(RippleType type) {
            switch (type) {
                case RippleType.Loss: return "combo_failed";
                case RippleType.Abandon: return "skipped";
                case RippleType.Sacrifice: return "pet_death";
                case RippleType.Desperation: return "low_health";
                case RippleType.Triumph: return "perfect_clear";
                case RippleType.Forget: return "meditation_skipped";
                default: return "generic";
            }
        }

        // ========== 存档支持 ==========

        public override Dictionary<string, object> ExportSaveData() {
            var rippleData = new Dictionary<string, Dictionary<string, object>>();
            foreach (var kvp in _ripplePoints) {
                rippleData[kvp.Key.ToString()] = new Dictionary<string, object> {
                    { "points", kvp.Value.Points },
                    { "totalEarned", kvp.Value.TotalEarned },
                    { "lastUpdated", kvp.Value.LastUpdated }
                };
            }
            return new Dictionary<string, object> {
                { "ripplePoints", rippleData }
            };
        }

        public override void ImportSaveData(Dictionary<string, object> data) {
            if (data == null || !data.ContainsKey("ripplePoints")) return;

            var rippleData = (Dictionary<string, object>)data["ripplePoints"];
            foreach (var kvp in rippleData) {
                RippleType type;
                if (Enum.TryParse<RippleType>(kvp.Key, out type)) {
                    var entry = (Dictionary<string, object>)kvp.Value;
                    if (!_ripplePoints.ContainsKey(type)) {
                        _ripplePoints[type] = new RipplePointData(type);
                    }
                    _ripplePoints[type].Points = (int)(long)entry["points"];
                    _ripplePoints[type].TotalEarned = (int)(long)entry["totalEarned"];
                    _ripplePoints[type].LastUpdated = (double)entry["lastUpdated"];
                }
            }

            // 重新检查预兆状态
            foreach (RippleType type in Enum.GetValues(typeof(RippleType))) {
                CheckHudHint(type);
            }
        }
    }
}
