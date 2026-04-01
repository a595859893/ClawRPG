// Stub definitions for types referenced but not yet implemented
// Auto-generated - replace with real implementations

namespace ClawRPG.Scripts.Data {
    public enum EmotionType { Happy, Sad, Angry, Fearful, Surprised, Neutral }
    public enum EmotionIntensity { Low, Medium, High, Extreme }
}

namespace ClawRPG.Scripts.Framework {
    public class SignalContainer {
        public class Signals {}
    }
    public class SignalContainer<T> {
        public void Emit(T value) {}
        public T Current { get; set; }
    }
}
