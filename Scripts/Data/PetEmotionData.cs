using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Data
{
    /// <summary>
    /// Pet emotion data structure
    /// </summary>
    public class PetEmotionData
    {
        public enum EmotionType
        {
            Happy,
            Sad,
            Angry,
            Excited,
            Tired,
            Hungry,
            Playful,
            Affectionate,
            Scared,
            Neutral
        }

        public enum EmotionIntensity
        {
            Low,
            Medium,
            High,
            Extreme
        }

        public string PetId { get; set; }
        public Dictionary<EmotionType, float> CurrentEmotions { get; set; } = new Dictionary<EmotionType, float>();
        public PetEmotionData.EmotionType DominantEmotion { get; set; } = EmotionType.Neutral;
        public PetEmotionData.EmotionIntensity CurrentIntensity { get; set; } = EmotionIntensity.Low;
        public List<EmotionHistoryEntry> EmotionHistory { get; set; } = new List<EmotionHistoryEntry>();
        public int TotalEmotionChanges { get; set; }
        public DateTime LastEmotionChange { get; set; }
    }

    public class EmotionHistoryEntry
    {
        public PetEmotionData.EmotionType Emotion { get; set; }
        public PetEmotionData.EmotionIntensity Intensity { get; set; }
        public DateTime Timestamp { get; set; }
        public string Trigger { get; set; }
    }
}
