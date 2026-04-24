using Godot;
using ClawRPG.Scripts.Systems.Ripple;

namespace ClawRPG.Scripts.Systems.Ripple {
    /// <summary>
    /// RippleIntegration - 涟漪系统集成辅助类
    /// 提供各系统行为触发点的涟漪点添加接口
    /// </summary>
    public static class RippleIntegration {
        /// <summary>
        /// 添加涟漪点的安全包装（RippleSystem 未初始化时静默忽略）
        /// </summary>
        public static void AddRipple(RippleType type, int amount) {
            if (RippleSystem.Instance != null) {
                RippleSystem.Instance.AddRipple(type, amount);
            }
        }

        /// <summary>
        /// 衰减涟漪点（冥想/局次结束时调用）
        /// </summary>
        public static void DecayRipple(RippleType type, float multiplier = 1.0f) {
            if (RippleSystem.Instance != null) {
                RippleSystem.Instance.DecayRipple(type, multiplier);
            }
        }

        /// <summary>
        /// 衰减所有涟漪点
        /// </summary>
        public static void DecayAllRipples(float multiplier = 1.0f) {
            if (RippleSystem.Instance != null) {
                RippleSystem.Instance.DecayAll(multiplier);
            }
        }
    }
}
