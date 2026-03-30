using Godot;
using System;
using System.Collections.Generic;
using Framework;

/// <summary>
/// 动态难度系统 - 根据玩家水平调整游戏难度
/// </summary>
public class DynamicDifficultySystem : BaseSystem
{
    // 单例
    private static DynamicDifficultySystem _instance;
    public static DynamicDifficultySystem Instance
    {
        get
        {
            if (_instance == null)
            {
                GD.PrintErr("DynamicDifficultySystem not initialized!");
            }
            return _instance;
        }
    }

    // 玩家数据
    private DynamicDifficultyData.PlayerDynamicDifficultyData _playerData;

    // 数据库
    private DynamicDifficultyDatabase _database;

    // 信号
public delegate void DifficultyChanged(DynamicDifficultyData.DifficultyLevel newDifficulty, DynamicDifficultyData.DifficultyLevel oldDifficulty);
public delegate void SkillProfileUpdated(DynamicDifficultyData.PlayerSkillProfile profile);
public delegate void SessionStatsUpdated(DynamicDifficultyData.SessionStats stats);
public delegate void RecommendationChanged(DynamicDifficultyData.DifficultyLevel recommended);

    public override void _Ready()
    {
        _instance = this;
        _database = DynamicDifficultyDatabase.Instance;
        _playerData = new DynamicDifficultyData.PlayerDynamicDifficultyData();
        
        GD.Print("Dynamic Difficulty System initialized");
    }

    #region 公共方法

    // 获取当前难度
    public DynamicDifficultyData.DifficultyLevel GetCurrentDifficulty()
    {
        return _playerData.CurrentDifficulty;
    }

    // 获取建议难度
    public DynamicDifficultyData.DifficultyLevel GetRecommendedDifficulty()
    {
        return _playerData.RecommendedDifficulty;
    }

    // 获取玩家技能档案
    public DynamicDifficultyData.PlayerSkillProfile GetSkillProfile()
    {
        return _playerData.SkillProfile;
    }

    // 获取当前会话统计
    public DynamicDifficultyData.SessionStats GetCurrentSessionStats()
    {
        return _playerData.CurrentSession;
    }

    // 是否自动调整
    public bool IsAutoAdjustment()
    {
        return _playerData.IsAutoAdjustment;
    }

    // 设置自动调整
    public void SetAutoAdjustment(bool autoAdjust)
    {
        _playerData.IsAutoAdjustment = autoAdjust;
    }

    // 手动设置难度
    public bool SetDifficulty(DynamicDifficultyData.DifficultyLevel difficulty)
    {
        DynamicDifficultyData.DifficultyLevel oldDifficulty = _playerData.CurrentDifficulty;
        
        // 验证难度值
        if (difficulty < DynamicDifficultyData.DifficultyLevel.Easy || 
            difficulty > DynamicDifficultyData.DifficultyLevel.Legendary)
        {
            GD.PrintErr($"Invalid difficulty level: {difficulty}");
            return false;
        }

        _playerData.CurrentDifficulty = difficulty;
        
        // 发出信号
        EmitSignal(nameof(DifficultyChanged), difficulty, oldDifficulty);
        GD.Print($"Difficulty changed from {oldDifficulty} to {difficulty}");
        
        return true;
    }

    // 记录击杀敌人
    public void RecordEnemyKilled(bool isBoss = false)
    {
        _playerData.CurrentSession.EnemiesKilled++;
        if (isBoss)
        {
            _playerData.CurrentSession.BossesDefeated++;
        }
    }

    // 记录死亡
    public void RecordDeath()
    {
        _playerData.CurrentSession.TimesDied++;
    }

    // 记录获得物品
    public void RecordItemCollected(int count = 1)
    {
        _playerData.CurrentSession.ItemsCollected += count;
    }

    // 记录获得金币
    public void RecordGoldEarned(int gold)
    {
        _playerData.CurrentSession.GoldEarned += gold;
    }

    // 记录获得经验
    public void RecordExperienceGained(int exp)
    {
        _playerData.CurrentSession.ExperienceGained += exp;
    }

    // 记录使用药水
    public void RecordPotionUsed(int count = 1)
    {
        _playerData.CurrentSession.PotionsUsed += count;
    }

    // 记录暴击
    public void RecordCriticalHit(int count = 1)
    {
        _playerData.CurrentSession.CriticalHits += count;
    }

    // 记录闪避
    public void RecordDodge(int count = 1)
    {
        _playerData.CurrentSession.Dodges += count;
    }

    // 更新会话时间
    public void UpdateSessionTime(float delta)
    {
        _playerData.CurrentSession.SessionTime += delta;
    }

    // 会话结束 - 评估表现
    public void EndSession(bool victory)
    {
        // 更新胜率
        _playerData.SkillProfile.TotalSessions++;
        if (victory)
        {
            _playerData.SkillProfile.Wins++;
        }
        else
        {
            _playerData.SkillProfile.Losses++;
        }

        // 更新平均数据
        UpdateAverages();

        // 计算技能评分
        CalculateSkillScore();

        // 更新建议难度
        UpdateRecommendation();

        // 保存会话到历史
        SaveSessionToHistory();

        // 如果自动调整，检查是否需要调整难度
        if (_playerData.IsAutoAdjustment)
        {
            CheckForDifficultyAdjustment();
        }

        // 重置当前会话
        _playerData.CurrentSession.Reset();

        // 发出信号
        EmitSignal(nameof(SkillProfileUpdated), _playerData.SkillProfile);
    }

    // 获取难度修正值
    public DynamicDifficultyData.DifficultyModifiers GetDifficultyModifiers()
    {
        DynamicDifficultyData.DifficultyModifiers modifiers = new DynamicDifficultyData.DifficultyModifiers();
        modifiers.SetForDifficulty(_playerData.CurrentDifficulty);
        return modifiers;
    }

    // 获取敌人属性乘数
    public float GetEnemyHealthMultiplier() => GetDifficultyModifiers().EnemyHealthMultiplier;
    public float GetEnemyDamageMultiplier() => GetDifficultyModifiers().EnemyDamageMultiplier;
    public float GetEnemySpeedMultiplier() => GetDifficultyModifiers().EnemySpeedMultiplier;
    public float GetDropRateMultiplier() => GetDifficultyModifiers().DropRateMultiplier;
    public float GetExperienceMultiplier() => GetDifficultyModifiers().ExperienceMultiplier;
    public float GetGoldMultiplier() => GetDifficultyModifiers().GoldMultiplier;
    public float GetEnemyCountMultiplier() => GetDifficultyModifiers().EnemyCountMultiplier;
    public float GetBossHealthMultiplier() => GetDifficultyModifiers().BossHealthMultiplier;
    public float GetBossDamageMultiplier() => GetDifficultyModifiers().BossDamageMultiplier;

    #endregion

    #region 私有方法

    // 更新平均数据
    private void UpdateAverages()
    {
        DynamicDifficultyData.SessionStats current = _playerData.CurrentSession;
        DynamicDifficultyData.PlayerSkillProfile profile = _playerData.SkillProfile;

        int sessions = profile.TotalSessions;
        
        // 更新平均通关时间
        profile.AverageClearTime = (profile.AverageClearTime * (sessions - 1) + current.SessionTime) / sessions;
        
        // 更新平均死亡次数
        profile.AverageDeaths = (profile.AverageDeaths * (sessions - 1) + current.TimesDied) / sessions;
    }

    // 计算技能评分
    private void CalculateSkillScore()
    {
        DynamicDifficultyData.PlayerSkillProfile profile = _playerData.SkillProfile;
        DynamicDifficultyData.SessionStats current = _playerData.CurrentSession;

        // 更新胜率
        profile.WinRate = (float)profile.Wins / profile.TotalSessions;

        // 计算资源效率 (基于药水使用和死亡)
        float resourceScore = 1.0f;
        if (current.SessionTime > 0)
        {
            float potionPerMinute = current.PotionsUsed / (current.SessionTime / 60f);
            resourceScore = Mathf.Clamp(1.0f - (potionPerMinute * 0.1f), 0f, 1f);
        }
        profile.ResourceEfficiency = resourceScore;

        // 计算生存能力 (基于死亡次数和通关时间)
        float survivalScore = 1.0f;
        if (current.SessionTime > 0)
        {
            float deathsPerMinute = current.TimesDied / (current.SessionTime / 60f);
            survivalScore = Mathf.Clamp(1.0f - (deathsPerMinute * 0.2f), 0f, 1f);
        }
        profile.SurvivalAbility = survivalScore;

        // 计算输出能力 (基于击杀数和暴击)
        float damageScore = 0.5f;
        if (current.SessionTime > 0)
        {
            float killsPerMinute = current.EnemiesKilled / (current.SessionTime / 60f);
            damageScore = Mathf.Clamp(killsPerMinute / 10f, 0f, 1f);
            
            // 考虑暴击率
            if (current.EnemiesKilled > 0)
            {
                float critRate = (float)current.CriticalHits / current.EnemiesKilled;
                damageScore = (damageScore + Mathf.Clamp(critRate * 2f, 0f, 1f)) / 2f;
            }
        }
        profile.DamageOutput = damageScore;

        // 技术水平 (综合评估)
        float techScore = 0.5f;
        if (current.SessionTime > 0)
        {
            // 考虑闪避
            float dodgeRate = current.Dodges / (current.SessionTime / 60f);
            techScore = Mathf.Clamp(dodgeRate / 5f, 0f, 1f);
        }
        profile.TechnicalSkill = techScore;

        // 计算综合评分 (加权平均)
        DynamicDifficultyDatabase.SkillWeights weights = DynamicDifficultyDatabase.SkillWeights.Default;
        profile.OverallScore = 
            profile.WinRate * weights.WinRate +
            profile.ResourceEfficiency * weights.ResourceEfficiency +
            profile.SurvivalAbility * weights.SurvivalAbility +
            profile.DamageOutput * weights.DamageOutput +
            profile.TechnicalSkill * weights.TechnicalSkill;

        GD.Print($"Skill Score Updated: WinRate={profile.WinRate:F2}, Resource={profile.ResourceEfficiency:F2}, Survival={profile.SurvivalAbility:F2}, Damage={profile.DamageOutput:F2}, Tech={profile.TechnicalSkill:F2}, Overall={profile.OverallScore:F2}");
    }

    // 更新建议难度
    private void UpdateRecommendation()
    {
        DynamicDifficultyData.DifficultyLevel newRecommendation = 
            DynamicDifficultyDatabase.GetRecommendedDifficulty(_playerData.SkillProfile.OverallScore);

        if (newRecommendation != _playerData.RecommendedDifficulty)
        {
            _playerData.RecommendedDifficulty = newRecommendation;
            EmitSignal(nameof(RecommendationChanged), newRecommendation);
            GD.Print($"Recommended difficulty changed to: {newRecommendation}");
        }
    }

    // 保存会话到历史
    private void SaveSessionToHistory()
    {
        // 复制当前会话数据
        DynamicDifficultyData.SessionStats historySession = new DynamicDifficultyData.SessionStats
        {
            EnemiesKilled = _playerData.CurrentSession.EnemiesKilled,
            BossesDefeated = _playerData.CurrentSession.BossesDefeated,
            TimesDied = _playerData.CurrentSession.TimesDied,
            ItemsCollected = _playerData.CurrentSession.ItemsCollected,
            GoldEarned = _playerData.CurrentSession.GoldEarned,
            ExperienceGained = _playerData.CurrentSession.ExperienceGained,
            SessionTime = _playerData.CurrentSession.SessionTime,
            PotionsUsed = _playerData.CurrentSession.PotionsUsed,
            CriticalHits = _playerData.CurrentSession.CriticalHits,
            Dodges = _playerData.CurrentSession.Dodges
        };

        _playerData.SessionHistory.Add(historySession);

        // 限制历史记录数量
        if (_playerData.SessionHistory.Count > 50)
        {
            _playerData.SessionHistory.RemoveAt(0);
        }

        _playerData.SessionsSinceLastAdjustment++;
    }

    // 检查是否需要难度调整
    private void CheckForDifficultyAdjustment()
    {
        DynamicDifficultyDatabase.AdjustmentParams param = DynamicDifficultyDatabase.AdjustmentParams.Default;

        // 检查是否满足调整条件
        if (_playerData.SessionsSinceLastAdjustment < param.SessionsRequiredForAdjustment)
        {
            return;
        }

        DynamicDifficultyData.DifficultyLevel current = _playerData.CurrentDifficulty;
        float winRate = _playerData.SkillProfile.WinRate;
        float scoreChange = _playerData.SkillProfile.OverallScore - 0.5f; // 相对于基准的变化

        // 尝试升级难度
        if (winRate >= param.MinWinRateForUpgrade && scoreChange >= param.ScoreChangeThreshold)
        {
            int newDifficulty = (int)current + 1;
            if (newDifficulty <= (int)DynamicDifficultyData.DifficultyLevel.Legendary)
            {
                // 检查是否超过建议难度太多
                if (newDifficulty - (int)_playerData.RecommendedDifficulty <= 1)
                {
                    SetDifficulty((DynamicDifficultyData.DifficultyLevel)newDifficulty);
                    _playerData.SessionsSinceLastAdjustment = 0;
                    return;
                }
            }
        }

        // 尝试降级难度
        if (winRate <= param.MaxWinRateForDowngrade || scoreChange <= -param.ScoreChangeThreshold)
        {
            int newDifficulty = (int)current - 1;
            if (newDifficulty >= (int)DynamicDifficultyData.DifficultyLevel.Easy)
            {
                // 检查是否低于建议难度太多
                if ((int)_playerData.RecommendedDifficulty - newDifficulty <= 1)
                {
                    SetDifficulty((DynamicDifficultyData.DifficultyLevel)newDifficulty);
                    _playerData.SessionsSinceLastAdjustment = 0;
                }
            }
        }
    }

    #endregion

    #region 存档支持

    // 获取存档数据
    public override Dictionary<string, object> ExportSaveData()
    {
        Dictionary data = new Dictionary<string, object>();
        
        data["currentDifficulty"] = (int)_playerData.CurrentDifficulty;
        data["recommendedDifficulty"] = (int)_playerData.RecommendedDifficulty;
        data["isAutoAdjustment"] = _playerData.IsAutoAdjustment;
        data["sessionsSinceLastAdjustment"] = _playerData.SessionsSinceLastAdjustment;

        // 技能档案
        Dictionary profile = new Dictionary<string, object>();
        profile["totalSessions"] = _playerData.SkillProfile.TotalSessions;
        profile["wins"] = _playerData.SkillProfile.Wins;
        profile["losses"] = _playerData.SkillProfile.Losses;
        profile["averageClearTime"] = _playerData.SkillProfile.AverageClearTime;
        profile["averageDeaths"] = _playerData.SkillProfile.AverageDeaths;
        data["skillProfile"] = profile;

        return data;
    }

    // 加载存档数据
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        if (data.ContainsKey("currentDifficulty"))
            _playerData.CurrentDifficulty = (DynamicDifficultyData.DifficultyLevel)(int)data["currentDifficulty"];
        
        if (data.ContainsKey("recommendedDifficulty"))
            _playerData.RecommendedDifficulty = (DynamicDifficultyData.DifficultyLevel)(int)data["recommendedDifficulty"];
        
        if (data.ContainsKey("isAutoAdjustment"))
            _playerData.IsAutoAdjustment = (bool)data["isAutoAdjustment"];
        
        if (data.ContainsKey("sessionsSinceLastAdjustment"))
            _playerData.SessionsSinceLastAdjustment = (int)data["sessionsSinceLastAdjustment"];

        // 技能档案
        if (data.ContainsKey("skillProfile"))
        {
            Dictionary profile = (Dictionary)data["skillProfile"];
            if (profile.ContainsKey("totalSessions"))
                _playerData.SkillProfile.TotalSessions = (int)profile["totalSessions"];
            if (profile.ContainsKey("wins"))
                _playerData.SkillProfile.Wins = (int)profile["wins"];
            if (profile.ContainsKey("losses"))
                _playerData.SkillProfile.Losses = (int)profile["losses"];
            if (profile.ContainsKey("averageClearTime"))
                _playerData.SkillProfile.AverageClearTime = (float)profile["averageClearTime"];
            if (profile.ContainsKey("averageDeaths"))
                _playerData.SkillProfile.AverageDeaths = (float)profile["averageDeaths"];

            // 重新计算评分
            if (_playerData.SkillProfile.TotalSessions > 0)
            {
                CalculateSkillScore();
            }
        }

        GD.Print("Dynamic Difficulty data loaded");
    }
    
    // 旧的存档方法（保留兼容性）
    public Dictionary<string, object> GetSaveData()
    {
        var data = new Dictionary<string, object>();
        foreach (var key in ExportSaveData().Keys)
        {
            data[key.ToString()] = ExportSaveData()[key];
        }
        return data;
    }
    
    public void LoadSaveData(Dictionary<string, object> data)
    {
        ImportSaveData(new Dictionary(data));
    }

    #endregion
}
