// Stub definitions for types referenced but not yet implemented
// Auto-generated - replace with real implementations

namespace ClawRPG.Systems.Pets.AI {
    public class DisagreementRecord {}
    public class PlayerActionRecord {}
    public class WorldAssessment {}
    public class AdversarialObserverState {}
    public class PlayerGoalInference {}
    public class TrajectoryPrediction {}
    // ObserverChallenge is defined in Scripts.Systems.Pets.AI
}

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

namespace ClawRPG.Systems {
    public class PartySystem {}
    public class InventoryManager {}
    public class WorldEventSystem {}
    public enum DungeonPhase { Exploration, Combat, Event, Boss, Victory }
}

namespace ClawRPG.Scripts.UI {
    public class ComboUI {}
    public class ComboCountUI {}
}

namespace Godot {
    // File class alias for Godot 3 compatibility
    public class File {
        public byte[] GetBuffer() { return System.Array.Empty<byte>(); }
    }
}

