using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Data
{
    /// <summary>
    /// Mount training configuration data
    /// </summary>
    [Serializable]
    public class MountTrainingData
    {
        public string MountId;
        public int Level = 1;
        public int CurrentExperience = 0;
        public int BondLevel = 1;
        public int CurrentBondPoints = 0;
        public List<string> UnlockedSkills = new List<string>();
        public List<TrainingSession> TrainingHistory = new List<TrainingSession>();
        public Dictionary<string, int> AttributeBoosts = new Dictionary<string, int>();
        public DateTime LastTrainingTime;
        public int TrainingSessionsCompleted = 0;
    }

    [Serializable]
    public class TrainingSession
    {
        public string SessionId;
        public string TrainingType;
        public int DurationMinutes;
        public int ExperienceGained;
        public int BondPointsGained;
        public Dictionary<string, int> AttributeGains = new Dictionary<string, int>();
        public DateTime CompletedAt;
    }

    [Serializable]
    public class TrainingProject
    {
        public string ProjectId;
        public string ProjectName;
        public string Description;
        public TrainingCategory Category;
        public int RequiredLevel;
        public int DurationMinutes;
        public int ExperienceReward;
        public int BondPointsReward;
        public Dictionary<string, int> AttributeRewards = new Dictionary<string, int>();
        public List<string> RequiredSkills = new List<string>();
        public string UnlockSkillId;
        public int DailyLimit;
    }

    public enum TrainingCategory
    {
        Combat,
        Speed,
        Stamina,
        Intelligence,
        Bonding,
        Special
    }

    [Serializable]
    public class MountTrainingSaveData
    {
        public Dictionary<string, MountTrainingData> MountTrainings = new Dictionary<string, MountTrainingData>();
        public int TotalTrainingSessions = 0;
        public int TotalExperienceGained = 0;
        public DateTime LastSaveTime;
    }
}
