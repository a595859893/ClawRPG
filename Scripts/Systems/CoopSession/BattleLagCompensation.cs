using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.CoopSession {
    /// <summary>
    /// 战斗延迟补偿 - 处理网络延迟带来的同步问题
    /// </summary>
    public partial class BattleLagCompensation : BaseSystem {
        
        /// <summary>
        /// 延迟补偿模式
        /// </summary>
        public enum CompensationMode {
            None,
            Interpolation,
            Prediction,
            Rollback
        }
        
        private CompensationMode _currentMode = CompensationMode.Interpolation;
        private float _networkLatency = 0f;
        private float _interpolationDelay = 0.1f;
        private float _predictionThreshold = 0.2f;
        
        // 延迟历史记录
        private List<float> _latencyHistory = new();
        private int _maxLatencyHistorySize = 60;
        
        public override void _Ready() {
            base._Ready();
        }
        
        /// <summary>
        /// 设置补偿模式
        /// </summary>
        public void SetCompensationMode(CompensationMode mode) {
            _currentMode = mode;
            GD.Print($"[BattleLagCompensation] Mode set to {mode}");
        }
        
        /// <summary>
        /// 更新网络延迟
        /// </summary>
        public void UpdateLatency(float latency) {
            _networkLatency = latency;
            
            // 记录延迟历史
            _latencyHistory.Add(latency);
            if (_latencyHistory.Count > _maxLatencyHistorySize) {
                _latencyHistory.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// 获取平均延迟
        /// </summary>
        public float GetAverageLatency() {
            if (_latencyHistory.Count == 0) {
                return 0f;
            }
            
            float sum = 0f;
            foreach (var latency in _latencyHistory) {
                sum += latency;
            }
            return sum / _latencyHistory.Count;
        }
        
        /// <summary>
        /// 获取延迟抖动
        /// </summary>
        public float GetLatencyJitter() {
            if (_latencyHistory.Count < 2) {
                return 0f;
            }
            
            var avg = GetAverageLatency();
            float variance = 0f;
            
            foreach (var latency in _latencyHistory) {
                var diff = latency - avg;
                variance += diff * diff;
            }
            
            return Mathf.Sqrt(variance / _latencyHistory.Count);
        }
        
        /// <summary>
        /// 计算插值延迟
        /// </summary>
        public float CalculateInterpolationDelay() {
            var avgLatency = GetAverageLatency();
            return Mathf.Max(_interpolationDelay, avgLatency * 2);
        }
        
        /// <summary>
        /// 是否需要预测
        /// </summary>
        public bool ShouldPredict() {
            return _currentMode == CompensationMode.Prediction && 
                   _networkLatency > _predictionThreshold;
        }
        
        /// <summary>
        /// 应用延迟补偿到位置
        /// </summary>
        public Vector2 ApplyCompensation(Vector2 currentPosition, Vector2 targetPosition, float delta) {
            switch (_currentMode) {
                case CompensationMode.Interpolation:
                    var delay = CalculateInterpolationDelay();
                    return currentPosition.MoveToward(targetPosition, delta * (1f / delay));
                    
                case CompensationMode.Prediction:
                    if (ShouldPredict()) {
                        // 预测逻辑
                        var direction = (targetPosition - currentPosition).Normalized();
                        return currentPosition + direction * _networkLatency * 10f;
                    }
                    return currentPosition.MoveToward(targetPosition, delta * 10f);
                    
                case CompensationMode.Rollback:
                    // 回滚逻辑
                    return targetPosition;
                    
                default:
                    return currentPosition;
            }
        }
        
        /// <summary>
        /// 设置插值延迟
        /// </summary>
        public void SetInterpolationDelay(float delay) {
            _interpolationDelay = Mathf.Max(0.01f, delay);
        }
        
        /// <summary>
        /// 设置预测阈值
        /// </summary>
        public void SetPredictionThreshold(float threshold) {
            _predictionThreshold = Mathf.Max(0f, threshold);
        }
        
        public override Dictionary ExportSaveData() {
            var data = new Dictionary();
            data["mode"] = (int)_currentMode;
            data["interpolationDelay"] = _interpolationDelay;
            data["predictionThreshold"] = _predictionThreshold;
            return data;
        }
        
        public override void ImportSaveData(Dictionary data) {
            if (data.Contains("mode")) {
                _currentMode = (CompensationMode)(int)data["mode"];
            }
            if (data.Contains("interpolationDelay")) {
                _interpolationDelay = (float)data["interpolationDelay"];
            }
            if (data.Contains("predictionThreshold")) {
                _predictionThreshold = (float)data["predictionThreshold"];
            }
        }
    }
}
