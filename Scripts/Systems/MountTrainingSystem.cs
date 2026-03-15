using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Data;

/// <summary>
/// 坐骑训练系统 - 管理坐骑的训练、经验和亲密度提升
/// </summary>
public class MountTrainingSystem : BaseSystem
{
    public static MountTrainingSystem Instance { get; private set; }
    
    private MountTrainingDatabase database;
    private Dictionary<string, MountTrainingData> mountTrainings = new Dictionary<string, MountTrainingData>();
    private Dictionary<string, List<TrainingSession>> dailyTrainingSessions = new Dictionary<string, List<TrainingSession>>();
    
    // Statistics
    private int totalTrainingSessions = 0;
    private int totalExperienceGained = 0;
    
    // Signals
    public static void TrainingCompleted(string mountId, TrainingSession session) { }
    public static void LevelUp(string mountId, int newLevel) { }
    public static void BondLevelUp(string mountId, int newBondLevel) { }
    public static void SkillUnlocked(string mountId, string skillId) { }
    
    public override void _Ready()
    {
        Instance = this;
        database = new MountTrainingDatabase();
        LoadTrainingData();
    }
    
    private void LoadTrainingData()
    {
        // Load from save file if exists
        // For now, initialize with empty data
        GD.Print("[MountTrainingSystem] System initialized");
    }
    
    public MountTrainingData GetOrCreateTrainingData(string mountId)
    {
        if (!mountTrainings.ContainsKey(mountId))
        {
            mountTrainings[mountId] = new MountTrainingData
            {
                MountId = mountId,
                Level = 1,
                CurrentExperience = 0,
                BondLevel = 1,
                CurrentBondPoints = 0,
                UnlockedSkills = new List<string>(),
                AttributeBoosts = new Dictionary<string, int>(),
                LastTrainingTime = DateTime.Now
            };
        }
        return mountTrainings[mountId];
    }
    
    public bool StartTraining(string mountId, string projectId)
    {
        var trainingData = GetOrCreateTrainingData(mountId);
        var project = database.GetTrainingProject(projectId);
        
        if (project == null)
        {
            GD.PrintErr($"[MountTrainingSystem] Project not found: {projectId}");
            return false;
        }
        
        if (trainingData.Level < project.RequiredLevel)
        {
            GD.PrintErr($"[MountTrainingSystem] Mount level {trainingData.Level} too low for project (required: {project.RequiredLevel})");
            return false;
        }
        
        // Check daily limit
        string dailyKey = mountId + "_" + projectId;
        if (!dailyTrainingSessions.ContainsKey(dailyKey))
            dailyTrainingSessions[dailyKey] = new List<TrainingSession>();
        
        int todayCount = 0;
        DateTime today = DateTime.Today;
        foreach (var session in dailyTrainingSessions[dailyKey])
        {
            if (session.CompletedAt.Date == today)
                todayCount++;
        }
        
        if (todayCount >= project.DailyLimit)
        {
            GD.PrintErr($"[MountTrainingSystem] Daily limit reached for project: {projectId}");
            return false;
        }
        
        // Check required skills
        foreach (string requiredSkill in project.RequiredSkills)
        {
            if (!trainingData.UnlockedSkills.Contains(requiredSkill))
            {
                GD.PrintErr($"[MountTrainingSystem] Required skill not unlocked: {requiredSkill}");
                return false;
            }
        }
        
        // Create training session
        TrainingSession session = new TrainingSession
        {
            SessionId = Guid.NewGuid().ToString(),
            TrainingType = projectId,
            DurationMinutes = project.DurationMinutes,
            ExperienceReward = project.ExperienceReward,
            BondPointsReward = project.BondPointsReward,
            AttributeGains = new Dictionary<string, int>(project.AttributeRewards),
            CompletedAt = DateTime.Now.AddMinutes(project.DurationMinutes)
        };
        
        // Apply rewards immediately (instant training for simplicity)
        ApplyTrainingRewards(mountId, session);
        
        // Record session
        trainingData.TrainingHistory.Add(session);
        trainingData.TrainingSessionsCompleted++;
        dailyTrainingSessions[dailyKey].Add(session);
        
        totalTrainingSessions++;
        totalExperienceGained += session.ExperienceGained;
        
        // Check for skill unlock
        if (!string.IsNullOrEmpty(project.UnlockSkillId) && !trainingData.UnlockedSkills.Contains(project.UnlockSkillId))
        {
            trainingData.UnlockedSkills.Add(project.UnlockSkillId);
            GD.Print($"[MountTrainingSystem] Skill unlocked: {project.UnlockSkillId}");
        }
        
        TrainingCompleted(mountId, session);
        
        GD.Print($"[MountTrainingSystem] Training completed for mount {mountId}: {project.ProjectName}");
        return true;
    }
    
    private void ApplyTrainingRewards(string mountId, TrainingSession session)
    {
        var trainingData = mountTrainings[mountId];
        
        // Add experience
        trainingData.CurrentExperience += session.ExperienceGained;
        
        // Check level up
        while (trainingData.CurrentExperience >= GetRequiredExperience(trainingData.Level + 1))
        {
            trainingData.CurrentExperience -= GetRequiredExperience(trainingData.Level + 1);
            trainingData.Level++;
            GD.Print($"[MountTrainingSystem] Mount {mountId} leveled up to {trainingData.Level}");
            LevelUp(mountId, trainingData.Level);
        }
        
        // Add bond points
        trainingData.CurrentBondPoints += session.BondPointsReward;
        
        // Check bond level up
        while (trainingData.CurrentBondPoints >= database.GetRequiredBondPoints(trainingData.BondLevel + 1))
        {
            trainingData.CurrentBondPoints -= database.GetRequiredBondPoints(trainingData.BondLevel + 1);
            trainingData.BondLevel++;
            GD.Print($"[MountTrainingSystem] Mount {mountId} bond level up to {trainingData.BondLevel}");
            BondLevelUp(mountId, trainingData.BondLevel);
            
            // Check for special unlock
            var bondReward = database.GetBondReward(trainingData.BondLevel);
            if (bondReward != null && bondReward.UnlocksSpecial && !trainingData.UnlockedSkills.Contains(bondReward.SpecialUnlock))
            {
                trainingData.UnlockedSkills.Add(bondReward.SpecialUnlock);
                SkillUnlocked(mountId, bondReward.SpecialUnlock);
            }
        }
        
        // Add attribute boosts
        foreach (var attr in session.AttributeGains)
        {
            if (!trainingData.AttributeBoosts.ContainsKey(attr.Key))
                trainingData.AttributeBoosts[attr.Key] = 0;
            trainingData.AttributeBoosts[attr.Key] += attr.Value;
        }
        
        trainingData.LastTrainingTime = DateTime.Now;
    }
    
    public int GetMountLevel(string mountId)
    {
        var data = GetOrCreateTrainingData(mountId);
        return data.Level;
    }
    
    public int GetMountBondLevel(string mountId)
    {
        var data = GetOrCreateTrainingData(mountId);
        return data.BondLevel;
    }
    
    public float GetBondMultiplier(string mountId)
    {
        var data = GetOrCreateTrainingData(mountId);
        var reward = database.GetBondReward(data.BondLevel);
        return reward != null ? reward.BonusMultiplier : 1.0f;
    }
    
    public Dictionary<string, int> GetAttributeBoosts(string mountId)
    {
        var data = GetOrCreateTrainingData(mountId);
        return data.AttributeBoosts;
    }
    
    public List<string> GetUnlockedSkills(string mountId)
    {
        var data = GetOrCreateTrainingData(mountId);
        return data.UnlockedSkills;
    }
    
    public List<TrainingProject> GetAvailableProjects(string mountId)
    {
        var data = GetOrCreateTrainingData(mountId);
        return database.GetAvailableProjects(data.Level);
    }
    
    public List<TrainingProject> GetProjectsByCategory(string mountId, TrainingCategory category)
    {
        var data = GetOrCreateTrainingData(mountId);
        var projects = database.GetProjectsByCategory(category);
        List<TrainingProject> available = new List<TrainingProject>();
        
        foreach (var project in projects)
        {
            if (project.RequiredLevel <= data.Level)
                available.Add(project);
        }
        
        return available;
    }
    
    public int GetProjectDailyUsage(string mountId, string projectId)
    {
        string dailyKey = mountId + "_" + projectId;
        if (!dailyTrainingSessions.ContainsKey(dailyKey))
            return 0;
        
        int count = 0;
        DateTime today = DateTime.Today;
        foreach (var session in dailyTrainingSessions[dailyKey])
        {
            if (session.CompletedAt.Date == today)
                count++;
        }
        return count;
    }
    
    public int GetRemainingDailyTraining(string mountId, string projectId)
    {
        var project = database.GetTrainingProject(projectId);
        if (project == null) return 0;
        
        int used = GetProjectDailyUsage(mountId, projectId);
        return Math.Max(0, project.DailyLimit - used);
    }
    
    public int GetExperienceProgress(string mountId)
    {
        var data = GetOrCreateTrainingData(mountId);
        int required = GetRequiredExperience(data.Level + 1);
        return required > 0 ? (int)((float)data.CurrentExperience / required * 100) : 100;
    }
    
    public int GetBondProgress(string mountId)
    {
        var data = GetOrCreateTrainingData(mountId);
        int required = database.GetRequiredBondPoints(data.BondLevel + 1);
        return required > 0 ? (int)((float)data.CurrentBondPoints / required * 100) : 100;
    }
    
    // === 数据持久化接口 ===
    
    public override Dictionary ExportSaveData()
    {
        var saveData = GetSaveData();
        var data = new Dictionary<string, object>
        {
            ["mount_trainings"] = saveData.MountTrainings,
            ["total_sessions"] = saveData.TotalTrainingSessions,
            ["total_exp"] = saveData.TotalExperienceGained
        };
        return new Dictionary(data);
    }
    
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("mount_trainings"))
            mountTrainings = new Dictionary<string, MountTrainingData>((Dictionary)data["mount_trainings"]);
        if (data.ContainsKey("total_sessions"))
            totalTrainingSessions = Convert.ToInt32(data["total_sessions"]);
        if (data.ContainsKey("total_exp"))
            totalExperienceGained = Convert.ToInt32(data["total_exp"]);
    }
    
    public MountTrainingSaveData GetSaveData()
    {
        var saveData = new MountTrainingSaveData
        {
            MountTrainings = new Dictionary<string, MountTrainingData>(mountTrainings),
            TotalTrainingSessions = totalTrainingSessions,
            TotalExperienceGained = totalExperienceGained,
            LastSaveTime = DateTime.Now
        };
        return saveData;
    }
    
    public void LoadSaveData(MountTrainingSaveData data)
    {
        if (data == null) return;
        
        mountTrainings = new Dictionary<string, MountTrainingData>(data.MountTrainings);
        totalTrainingSessions = data.TotalTrainingSessions;
        totalExperienceGained = data.TotalExperienceGained;
        
        GD.Print($"[MountTrainingSystem] Loaded {mountTrainings.Count} mount training records");
    }
    
    public Dictionary<string, object> GetStatistics()
    {
        Dictionary<string, object> stats = new Dictionary<string, object>
        {
            { "TotalTrainingSessions", totalTrainingSessions },
            { "TotalExperienceGained", totalExperienceGained },
            { "TotalMounts", mountTrainings.Count }
        };
        
        int totalLevel = 0;
        int totalBondLevel = 0;
        foreach (var data in mountTrainings.Values)
        {
            totalLevel += data.Level;
            totalBondLevel += data.BondLevel;
        }
        
        if (mountTrainings.Count > 0)
        {
            stats["AverageLevel"] = totalLevel / mountTrainings.Count;
            stats["AverageBondLevel"] = totalBondLevel / mountTrainings.Count;
        }
        
        return stats;
    }
}
