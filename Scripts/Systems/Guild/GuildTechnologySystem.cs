using System;
using System.Collections.Generic;
using Godot;

public partial class GuildTechnologySystem : BaseSystem
{
    private static GuildTechnologySystem _instance;
    public static GuildTechnologySystem Instance => _instance ??= new GuildTechnologySystem();

    protected override string SystemName => "GuildTechnologySystem";

    public GuildTechnologyData Data { get; private set; } = new GuildTechnologyData();
    public event Action<string, int> OnTechnologyLevelUp;
    public event Action<string> OnResearchComplete;

    public GuildTechnologySystem()
    {
        LoadData();
    }

    public void LoadData()
    {
        // 从存档加载数据
        var saveSystem = SaveSystem.Instance;
        if (saveSystem != null)
        {
            var loadedData = saveSystem.LoadGameData<GuildTechnologyData>("guild_technology_data");
            if (loadedData != null)
            {
                Data = loadedData;
            }
        }
    }

    public void SaveData()
    {
        var saveSystem = SaveSystem.Instance;
        if (saveSystem != null)
        {
            saveSystem.SaveGameData(Data, "guild_technology_data");
        }
    }

    // 开始研究科技
    public bool StartResearch(string techId)
    {
        var tech = GuildTechnologyDatabase.Instance.GetTechnology(techId);
        if (tech == null) return false;

        if (!Data.GuildTechs.TryGetValue(techId, out var progress))
        {
            progress = new GuildTechnologyData.GuildTechnologyProgress
            {
                TechId = techId,
                CurrentLevel = 0,
                IsResearching = false
            };
            Data.GuildTechs[techId] = progress;
        }

        // 检查是否已达最大等级
        if (progress.CurrentLevel >= tech.MaxLevel) return false;

        // 检查科技点数是否足够
        int cost = GetResearchCost(techId, progress.CurrentLevel + 1);
        if (Data.AvailablePoints < cost) return false;

        // 扣除科技点数并开始研究
        Data.AvailablePoints -= cost;
        progress.IsResearching = true;
        progress.ResearchStartTime = OS.GetSystemTimeMsecs();
        progress.TotalResearchTime = tech.ResearchTime;

        SaveData();
        return true;
    }

    // 取消研究
    public bool CancelResearch(string techId)
    {
        if (!Data.GuildTechs.TryGetValue(techId, out var progress)) return false;
        if (!progress.IsResearching) return false;

        // 退还一半科技点数
        var tech = GuildTechnologyDatabase.Instance.GetTechnology(techId);
        if (tech != null)
        {
            int cost = GetResearchCost(techId, progress.CurrentLevel + 1);
            Data.AvailablePoints += cost / 2;
        }

        progress.IsResearching = false;
        progress.ResearchStartTime = 0;
        SaveData();
        return true;
    }

    // 完成研究
    public void CompleteResearch(string techId)
    {
        if (!Data.GuildTechs.TryGetValue(techId, out var progress)) return;

        progress.IsResearching = false;
        progress.CurrentLevel++;
        progress.ResearchStartTime = 0;
        Data.TotalResearched++;

        OnTechnologyLevelUp?.Invoke(techId, progress.CurrentLevel);
        OnResearchComplete?.Invoke(techId);
        SaveData();
    }

    // 获取研究成本
    public int GetResearchCost(string techId, int level)
    {
        var tech = GuildTechnologyDatabase.Instance.GetTechnology(techId);
        if (tech == null) return 0;
        return (int)(tech.ResearchCost * Math.Pow(1.5, level - 1));
    }

    // 获取当前等级
    public int GetCurrentLevel(string techId)
    {
        if (Data.GuildTechs.TryGetValue(techId, out var progress))
        {
            return progress.CurrentLevel;
        }
        return 0;
    }

    // 获取研究进度
    public float GetResearchProgress(string techId)
    {
        if (!Data.GuildTechs.TryGetValue(techId, out var progress)) return 0;
        if (!progress.IsResearching) return 0;

        long elapsed = OS.GetSystemTimeMsecs() - progress.ResearchStartTime;
        float progress_ = (float)elapsed / (progress.TotalResearchTime * 1000);
        return Mathf.Clamp(progress_, 0, 1);
    }

    // 是否正在研究
    public bool IsResearching(string techId)
    {
        if (Data.GuildTechs.TryGetValue(techId, out var progress))
        {
            return progress.IsResearching;
        }
        return false;
    }

    // 获取科技加成
    public Dictionary<string, float> GetTotalBonuses()
    {
        Dictionary<string, float> totalBonuses = new Dictionary<string, float>();

        foreach (var kvp in Data.GuildTechs)
        {
            var tech = GuildTechnologyDatabase.Instance.GetTechnology(kvp.Key);
            if (tech == null) continue;

            int level = kvp.Value.CurrentLevel;
            if (level <= 0) continue;

            // 每级加成
            for (int i = 0; i < level; i++)
            {
                foreach (var bonus in tech.Bonuses)
                {
                    if (!totalBonuses.ContainsKey(bonus.Key))
                        totalBonuses[bonus.Key] = 0;
                    totalBonuses[bonus.Key] += bonus.Value;
                }
            }
        }

        return totalBonuses;
    }

    // 获取单个加成
    public float GetBonus(string bonusType)
    {
        var bonuses = GetTotalBonuses();
        return bonuses.GetValueOrDefault(bonusType, 0);
    }

    // 添加科技点数
    public void AddTechPoints(int points)
    {
        Data.AvailablePoints += points;
        SaveData();
    }

    // 获取所有科技数据
    public Dictionary<string, GuildTechnologyData.GuildTechnologyProgress> GetAllTechProgress()
    {
        return Data.GuildTechs;
    }

    // 获取科技数据
    public GuildTechnologyData.GuildTechnologyProgress GetTechProgress(string techId)
    {
        if (Data.GuildTechs.TryGetValue(techId, out var progress))
        {
            return progress;
        }
        return new GuildTechnologyData.GuildTechnologyProgress { TechId = techId, CurrentLevel = 0 };
    }

    // 更新研究进度
    public void UpdateResearch()
    {
        foreach (var kvp in Data.GuildTechs)
        {
            if (kvp.Value.IsResearching)
            {
                float progress = GetResearchProgress(kvp.Key);
                if (progress >= 1.0f)
                {
                    CompleteResearch(kvp.Key);
                }
            }
        }
    }

    // 获取正在研究的科技
    public string GetResearchingTech()
    {
        foreach (var kvp in Data.GuildTechs)
        {
            if (kvp.Value.IsResearching)
            {
                return kvp.Key;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Export save data (BaseSystem override)
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        data["availablePoints"] = Data.AvailablePoints;
        data["totalResearched"] = Data.TotalResearched;
        data["guildTechs"] = Data.GuildTechs;
        return data;
    }
    
    /// <summary>
    /// Import save data (BaseSystem override)
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.Contains("availablePoints"))
        {
            Data.AvailablePoints = (int)data["availablePoints"];
        }
        if (data.Contains("totalResearched"))
        {
            Data.TotalResearched = (int)data["totalResearched"];
        }
        if (data.Contains("guildTechs"))
        {
            Data.GuildTechs = (Dictionary<string, GuildTechnologyData.GuildTechnologyProgress>)data["guildTechs"];
        }
    }
}
