using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.PetMood {
    public enum PetMoodType {
        Happy,
        Sad,
        Angry,
        Playful,
        Tired,
        Hungry,
        Excited,
        Calm,
        Affectionate,
        Neutral
    }

    public enum MoodIntensity {
        Low,
        Medium,
        High,
        Extreme
    }

    public class PetMoodData {
        public Dictionary<string, PetMood> Moods = new Dictionary<string, PetMood>();
        public int TotalInteractionCount = 0;
        public Dictionary<string, int> MoodChangesCount = new Dictionary<string, int>();
    }

    public class PetMood {
        public string PetId;
        public PetMoodType CurrentMood = PetMoodType.Neutral;
        public MoodIntensity Intensity = MoodIntensity.Medium;
        public float MoodValue = 0.5f; // 0-1 范围
        public double LastMoodChangeTime = 0;
        public int ConsecutiveMoodDuration = 0;
        public Dictionary<PetMoodType, float> MoodHistory = new Dictionary<PetMoodType, float>();
    }

    public class PetMoodEffect {
        public string EffectId;
        public string Description;
        public PetMoodType TriggerMood;
        public MoodIntensity RequiredIntensity;
        public float StatBonus = 0f;
        public float ExpBonus = 0f;
        public float DropRateBonus = 0f;
    }
}
