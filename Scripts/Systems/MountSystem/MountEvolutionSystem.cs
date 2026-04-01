using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public class MountEvolutionSystem : BaseSystem
{
    private static MountEvolutionSystem _instance;
    public static MountEvolutionSystem Instance => _instance ??= new MountEvolutionSystem();

    // 玩家坐骑进化数据: mountId -> PlayerMountEvolution
    private System.Collections.Generic.Dictionary<string, MountEvolutionData.PlayerMountEvolution> _playerEvolutions;

    // 进化事件信号
    public static Action<string, string, MountEvolutionData.EvolutionStage> OnMountEvolved;
    public static Action<string, MountEvolutionData.EvolutionStage> OnEvolutionReady;
    public static Action<string, int> OnBattleExpGained;

    public MountEvolutionSystem()
    {
        _playerEvolutions = new System.Collections.Generic.Dictionary<string, MountEvolutionData.PlayerMountEvolution>();
    }

    protected override void Initialize()
    {
        GD.Print("[MountEvolutionSystem] Initialized");
        IsInitialized = true;
    }

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override System.Collections.Generic.Dictionary<string, object> ExportSaveData()
    {
        var data = new System.Collections.Generic.Dictionary<string, object>();
        
        foreach (var kvp in _playerEvolutions)
        {
            var evolution = kvp.Value;
            data[kvp.Key] = new Dictionary
            {
                { "currentStage", (int)evolution.CurrentStage },
                { "evolvedType", (int)evolution.EvolvedType },
                { "totalBattleExp", evolution.TotalBattleExp },
                { "evolutionCount", evolution.EvolutionCount },
                { "lastEvolutionTime", evolution.LastEvolutionTime.ToString("o") }
            };
        }
        
        return data;
    }

    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(System.Collections.Generic.Dictionary<string, object> data)
    {
        if (data == null) return;
        
        _playerEvolutions.Clear();
        
        foreach (var key in data.Keys)
        {
            var mountId = key.ToString();
            var mountData = data[key] as Dictionary;
            
            if (mountData == null) continue;
            
            var evolution = new MountEvolutionData.PlayerMountEvolution
            {
                MountId = mountId,
                CurrentStage = (MountEvolutionData.EvolutionStage)Convert.ToInt32(mountData["currentStage"]),
                EvolvedType = (MountEvolutionData.EvolutionType)Convert.ToInt32(mountData["evolvedType"]),
                TotalBattleExp = Convert.ToInt32(mountData["totalBattleExp"]),
                EvolutionCount = Convert.ToInt32(mountData["evolutionCount"])
            };
            
            if (mountData.ContainsKey("lastEvolutionTime"))
            {
                DateTime.TryParse(mountData["lastEvolutionTime"].ToString(), out var lastTime);
                evolution.LastEvolutionTime = lastTime;
            }
            
            _playerEvolutions[mountId] = evolution;
        }
        
        GD.Print($"[MountEvolutionSystem] Loaded evolution data for {_playerEvolutions.Count} mounts");
    }

    /// <summary>
    /// 获取玩家坐骑进化数据
    /// </summary>
    public MountEvolutionData.PlayerMountEvolution GetEvolution(string mountId)
    {
        if (!_playerEvolutions.ContainsKey(mountId))
        {
            _playerEvolutions[mountId] = new MountEvolutionData.PlayerMountEvolution
            {
                MountId = mountId
            };
        }
        return _playerEvolutions[mountId];
    }

    /// <summary>
    /// 获取当前进化阶段
    /// </summary>
    public MountEvolutionData.EvolutionStage GetCurrentStage(string mountId)
    {
        return GetEvolution(mountId).CurrentStage;
    }

    /// <summary>
    /// 获取进化后类型
    /// </summary>
    public MountEvolutionData.EvolutionType GetEvolvedType(string mountId)
    {
        return GetEvolution(mountId).EvolvedType;
    }

    /// <summary>
    /// 获取进化名称
    /// </summary>
    public string GetEvolutionName(string mountId)
    {
        var evolution = GetEvolution(mountId);
        var config = MountEvolutionDatabase.Instance.GetEvolutionByStage(mountId, evolution.CurrentStage);
        
        if (config != null && evolution.CurrentStage > MountEvolutionData.EvolutionStage.Basic)
            return config.EvolutionName;
        
        // 返回基础名称
        return GetBaseMountName(mountId);
    }

    /// <summary>
    /// 获取基础坐骑名称
    /// </summary>
    public string GetBaseMountName(string mountId)
    {
        var config = MountEvolutionDatabase.Instance.GetEvolutionByStage(mountId, MountEvolutionData.EvolutionStage.Basic);
        if (config != null)
            return config.BaseMountName;
        
        // 默认映射
        switch (mountId)
        {
            case "white_horse": return "白马";
            case "black_horse": return "黑马";
            case "snow_wolf": return "雪狼";
            case "shadow_wolf": return "暗影狼";
            case "brown_bear": return "棕熊";
            case "golden_eagle": return "金鹰";
            case "red_dragon": return "红龙";
            case "blue_dragon": return "蓝龙";
            case "qilin": return "麒麟";
            default: return mountId;
        }
    }

    /// <summary>
    /// 获取进化描述
    /// </summary>
    public string GetEvolutionDescription(string mountId)
    {
        var evolution = GetEvolution(mountId);
        var config = MountEvolutionDatabase.Instance.GetEvolutionByStage(mountId, evolution.CurrentStage);
        
        if (config != null && evolution.CurrentStage > MountEvolutionData.EvolutionStage.Basic)
            return config.Description;
        
        return "未进化的基础形态";
    }

    /// <summary>
    /// 获取总战斗经验
    /// </summary>
    public int GetTotalBattleExp(string mountId)
    {
        return GetEvolution(mountId).TotalBattleExp;
    }

    /// <summary>
    /// 检查是否可以进化到下一阶段
    /// </summary>
    public bool CanEvolve(string mountId)
    {
        var evolution = GetEvolution(mountId);
        var nextConfig = MountEvolutionDatabase.Instance.GetNextEvolution(mountId, evolution.CurrentStage);
        
        if (nextConfig == null) return false;
        
        return evolution.CanEvolve(nextConfig);
    }

    /// <summary>
    /// 获取进化需求信息
    /// </summary>
    public MountEvolutionData.MountEvolutionConfig GetEvolutionRequirements(string mountId)
    {
        var evolution = GetEvolution(mountId);
        return MountEvolutionDatabase.Instance.GetNextEvolution(mountId, evolution.CurrentStage);
    }

    /// <summary>
    /// 执行进化
    /// </summary>
    public bool Evolve(string mountId)
    {
        var evolution = GetEvolution(mountId);
        var nextConfig = MountEvolutionDatabase.Instance.GetNextEvolution(mountId, evolution.CurrentStage);
        
        if (nextConfig == null)
        {
            GD.Print($"[MountEvolutionSystem] No more evolutions available for {mountId}");
            return false;
        }
        
        if (!evolution.CanEvolve(nextConfig))
        {
            GD.Print($"[MountEvolutionSystem] Cannot meet evolution requirements for {mountId}");
            return false;
        }

        // 扣除物品
        if (nextConfig.RequiredItemId > 0)
        {
            InventorySystem.Instance.RemoveItem(nextConfig.RequiredItemId, nextConfig.RequiredItemCount);
        }

        // 扣除金币
        Player.Instance.Gold -= nextConfig.GoldCost;

        // 执行进化
        evolution.CurrentStage = nextConfig.Stage;
        evolution.EvolvedType = nextConfig.Type;
        evolution.EvolutionCount++;
        evolution.LastEvolutionTime = DateTime.Now;

        // 解锁技能
        foreach (var skillId in nextConfig.UnlockSkills)
        {
            UnlockMountSkill(mountId, skillId);
        }

        GD.Print($"[MountEvolutionSystem] {mountId} evolved to {nextConfig.EvolutionName}!");
        
        // 触发事件
        OnMountEvolved?.Invoke(mountId, nextConfig.EvolutionName, nextConfig.Stage);
        
        return true;
    }

    /// <summary>
    /// 解锁坐骑技能
    /// </summary>
    private void UnlockMountSkill(string mountId, string skillId)
    {
        // 技能解锁逻辑 - 可以在此处添加
        GD.Print($"[MountEvolutionSystem] Unlocked mount skill: {skillId} for {mountId}");
    }

    /// <summary>
    /// 添加战斗经验
    /// </summary>
    public void AddBattleExp(string mountId, int exp)
    {
        var evolution = GetEvolution(mountId);
        evolution.TotalBattleExp += exp;
        
        OnBattleExpGained?.Invoke(mountId, exp);
        
        // 检查是否可以进化
        var nextConfig = MountEvolutionDatabase.Instance.GetNextEvolution(mountId, evolution.CurrentStage);
        if (nextConfig != null && evolution.TotalBattleExp >= nextConfig.RequiredBattleExp)
        {
            OnEvolutionReady?.Invoke(mountId, nextConfig.Stage);
        }
        
        GD.Print($"[MountEvolutionSystem] {mountId} gained {exp} battle exp, total: {evolution.TotalBattleExp}");
    }

    /// <summary>
    /// 获取属性加成
    /// </summary>
    public float GetAttributeBonus(string mountId, string attributeType)
    {
        var evolution = GetEvolution(mountId);
        var config = MountEvolutionDatabase.Instance.GetEvolutionByStage(mountId, evolution.CurrentStage);
        
        if (config == null) return 0f;
        
        switch (attributeType.ToLower())
        {
            case "health":
            case "hp":
                return config.HealthBonus;
            case "attack":
            case "damage":
                return config.AttackBonus;
            case "defense":
                return config.DefenseBonus;
            case "speed":
                return config.SpeedBonus;
            default:
                return 0f;
        }
    }

    /// <summary>
    /// 获取进化后的颜色
    /// </summary>
    public Color GetEvolutionTintColor(string mountId)
    {
        var evolution = GetEvolution(mountId);
        
        if (evolution.CurrentStage == MountEvolutionData.EvolutionStage.Basic)
            return Colors.White;
        
        var config = MountEvolutionDatabase.Instance.GetEvolutionByStage(mountId, evolution.CurrentStage);
        return config?.TintColor ?? Colors.White;
    }

    /// <summary>
    /// 获取进化进度百分比
    /// </summary>
    public float GetEvolutionProgress(string mountId)
    {
        var evolution = GetEvolution(mountId);
        var nextConfig = MountEvolutionDatabase.Instance.GetNextEvolution(mountId, evolution.CurrentStage);
        
        if (nextConfig == null)
            return 1f; // 已达到最高进化
        
        if (nextConfig.RequiredBattleExp <= 0)
            return 0f;
        
        return Mathf.Clamp((float)evolution.TotalBattleExp / nextConfig.RequiredBattleExp, 0f, 1f);
    }

    /// <summary>
    /// 获取所有可进化的坐骑
    /// </summary>
    public List<string> GetEvolvableMounts()
    {
        return MountEvolutionDatabase.Instance.GetEvolvableMounts();
    }

    /// <summary>
    /// 检查是否有可进化的坐骑
    /// </summary>
    public bool HasEvolvableMounts()
    {
        foreach (var mountId in GetEvolvableMounts())
        {
            if (CanEvolve(mountId))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 存档数据
    /// </summary>
    public System.Collections.Generic.Dictionary<string, System.Collections.Generic.System.Collections.Generic.Dictionary<string, object>> SaveData()
    {
        var data = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.System.Collections.Generic.Dictionary<string, object>>();
        
        foreach (var kvp in _playerEvolutions)
        {
            var evolution = kvp.Value;
            data[kvp.Key] = new System.Collections.Generic.System.Collections.Generic.Dictionary<string, object>
            {
                { "currentStage", (int)evolution.CurrentStage },
                { "evolvedType", (int)evolution.EvolvedType },
                { "totalBattleExp", evolution.TotalBattleExp },
                { "evolutionCount", evolution.EvolutionCount },
                { "lastEvolutionTime", evolution.LastEvolutionTime.ToString("o") }
            };
        }
        
        return data;
    }

    /// <summary>
    /// 加载存档数据
    /// </summary>
    public void LoadData(System.Collections.Generic.Dictionary<string, System.Collections.Generic.System.Collections.Generic.Dictionary<string, object>> data)
    {
        if (data == null) return;
        
        _playerEvolutions.Clear();
        
        foreach (var kvp in data)
        {
            var mountId = kvp.Key;
            var mountData = kvp.Value;
            
            var evolution = new MountEvolutionData.PlayerMountEvolution
            {
                MountId = mountId,
                CurrentStage = (MountEvolutionData.EvolutionStage)Convert.ToInt32(mountData["currentStage"]),
                EvolvedType = (MountEvolutionData.EvolutionType)Convert.ToInt32(mountData["evolvedType"]),
                TotalBattleExp = Convert.ToInt32(mountData["totalBattleExp"]),
                EvolutionCount = Convert.ToInt32(mountData["evolutionCount"])
            };
            
            if (mountData.ContainsKey("lastEvolutionTime"))
            {
                DateTime.TryParse(mountData["lastEvolutionTime"].ToString(), out var lastTime);
                evolution.LastEvolutionTime = lastTime;
            }
            
            _playerEvolutions[mountId] = evolution;
        }
        
        GD.Print($"[MountEvolutionSystem] Loaded evolution data for {_playerEvolutions.Count} mounts");
    }


}
