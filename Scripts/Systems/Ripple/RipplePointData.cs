using Godot;
using System;

namespace ClawRPG.Scripts.Systems.Ripple {
    /// <summary>
    /// Ripple Type - 涟漪类型，对应玩家不同类型的行为
    /// </summary>
    public enum RippleType {
        Loss,        // 放弃/combo 失败
        Abandon,     // 跳过/遗物未用
        Sacrifice,   // 宠物牺牲
        Desperation, // 连续失败
        Triumph,     // 完美胜利
        Forget       // 忽视/跳过冥想
    }

    /// <summary>
    /// RipplePointData - 涟漪点数据
    /// </summary>
    public partial class RipplePointData : Resource {
        [Export] public RippleType Type { get; set; }
        [Export] public int Points { get; set; }
        [Export] public int TotalEarned { get; set; }
        [Export] public double LastUpdated { get; set; }

        public RipplePointData() {
            Type = RippleType.Loss;
            Points = 0;
            TotalEarned = 0;
            LastUpdated = 0;
        }

        public RipplePointData(RippleType type) {
            Type = type;
            Points = 0;
            TotalEarned = 0;
            LastUpdated = OS.GetUnixTime();
        }

        public void AddPoints(int amount) {
            Points += amount;
            TotalEarned += amount;
            LastUpdated = OS.GetUnixTime();
        }

        /// <summary>
        /// 衰减点数，按比例衰减
        /// </summary>
        public void Decay(float decayRate) {
            int decayedAmount = (int)(Points * decayRate);
            Points = Mathf.Max(0, Points - decayedAmount);
            LastUpdated = OS.GetUnixTime();
        }

        /// <summary>
        /// 获取归一化权重（0.0 ~ 1.0）
        /// </summary>
        public float GetNormalizedWeight(int threshold) {
            if (threshold <= 0) return 0f;
            return Mathf.Clamp((float)Points / threshold, 0f, 1f);
        }

        /// <summary>
        /// 是否达到预兆阈值（70%）
        /// </summary>
        public bool IsHudHintVisible(int threshold) {
            if (threshold <= 0) return false;
            return (float)Points / threshold >= 0.7f;
        }
    }

    /// <summary>
    /// RippleSaveData - 涟漪点存档数据结构
    /// </summary>
    public class RippleSaveData {
        public int Points { get; set; }
        public int TotalEarned { get; set; }
        public double LastUpdated { get; set; }

        public RippleSaveData() {}

        public RippleSaveData(int points, int totalEarned, double lastUpdated) {
            Points = points;
            TotalEarned = totalEarned;
            LastUpdated = lastUpdated;
        }
    }
}
