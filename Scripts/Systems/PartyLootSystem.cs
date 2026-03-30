using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

/// <summary>
/// 队伍战利品系统 - 管理经验分配和掉落规则
/// </summary>
public class PartyLootSystem : BaseSystem
{
    public static PartyLootSystem Instance { get; private set; }

    // 队伍设置
    private bool _shareExp = true;
    private bool _shareLoot = false;
    private bool _autoAccept = false;
    private PartyData.ExpDistributionMode _expMode = PartyData.ExpDistributionMode.Equal;

    public bool ShareExp => _shareExp;
    public bool ShareLoot => _shareLoot;
    public bool AutoAccept => _autoAccept;
    public PartyData.ExpDistributionMode ExpMode => _expMode;

    protected override void Initialize()
    {
        Instance = this;
        GD.Print("[PartyLootSystem] Initialized");
    }

    /// <summary>
    /// 设置经验共享
    /// </summary>
    public void SetShareExp(bool share)
    {
        _shareExp = share;
    }

    /// <summary>
    /// 设置战利品共享
    /// </summary>
    public void SetShareLoot(bool share)
    {
        _shareLoot = share;
    }

    /// <summary>
    /// 设置自动接受邀请
    /// </summary>
    public void SetAutoAccept(bool autoAccept)
    {
        _autoAccept = autoAccept;
    }

    /// <summary>
    /// 设置经验分配模式
    /// </summary>
    public void SetExpDistributionMode(PartyData.ExpDistributionMode mode)
    {
        _expMode = mode;
    }

    /// <summary>
    /// 计算经验分配
    /// </summary>
    public Dictionary<int, int> CalculateExpDistribution(int totalExp)
    {
        var distribution = new Dictionary<int, int>();
        
        if (!_shareExp || PartyManager.Instance == null || !PartyManager.Instance.IsInParty)
        {
            if (PartyManager.Instance != null)
            {
                distribution[PartyManager.Instance.IsInParty ? PartyManager.Instance.PartyId : 0] = totalExp;
            }
            else
            {
                distribution[0] = totalExp;
            }
            return distribution;
        }
        
        var members = PartyManager.Instance.GetMembers();
        List<int> onlineMembers = new List<int>();
        foreach (var member in members)
        {
            if (member.IsOnline)
                onlineMembers.Add(member.PlayerId);
        }
        
        int memberCount = onlineMembers.Count;
        if (memberCount == 0)
        {
            distribution[PartyManager.Instance.PartyId] = totalExp;
            return distribution;
        }
        
        switch (_expMode)
        {
            case PartyData.ExpDistributionMode.Equal:
                int expPerMember = totalExp / memberCount;
                foreach (var id in onlineMembers)
                    distribution[id] = expPerMember;
                break;
                
            case PartyData.ExpDistributionMode.BasedOnLevel:
                int totalLevel = 0;
                Dictionary<int, int> memberLevels = new Dictionary<int, int>();
                foreach (var id in onlineMembers)
                {
                    var member = PartyManager.Instance.GetMember(id);
                    int level = member != null ? member.Level : 1;
                    memberLevels[id] = level;
                    totalLevel += level;
                }
                foreach (var id in onlineMembers)
                {
                    float ratio = (float)memberLevels[id] / totalLevel;
                    distribution[id] = (int)(totalExp * ratio);
                }
                break;
                
            default:
                int equalExp = totalExp / memberCount;
                foreach (var id in onlineMembers)
                    distribution[id] = equalExp;
                break;
        }
        
        return distribution;
    }

    /// <summary>
    /// 计算掉落分配 - 返回获得战利品的玩家ID
    /// </summary>
    public int CalculateLootAssignment(int[] candidateIds, string itemRarity)
    {
        if (!_shareLoot || PartyManager.Instance == null || !PartyManager.Instance.IsInParty)
        {
            // 不共享时随机分配给候选人
            if (candidateIds.Length > 0)
            {
                return candidateIds[GD.Randi() % candidateIds.Length];
            }
            return -1;
        }
        
        var members = PartyManager.Instance.GetMembers();
        
        // 基于稀有度调整概率
        float luckBonus = 0f;
        if (PartyBuffSystem.Instance != null)
        {
            luckBonus = PartyBuffSystem.Instance.GetBuffValue(PartyData.PartyBuffType.LuckBoost);
        }
        
        // 简单随机分配，后续可扩展更复杂的规则
        List<int> onlineMembers = new List<int>();
        foreach (var member in members)
        {
            if (member.IsOnline)
                onlineMembers.Add(member.PlayerId);
        }
        
        if (onlineMembers.Count == 0)
        {
            return candidateIds.Length > 0 ? candidateIds[GD.Randi() % candidateIds.Length] : -1;
        }
        
        // 随机分配
        return onlineMembers[GD.Randi() % onlineMembers.Count];
    }

    /// <summary>
    /// 获取经验加成
    /// </summary>
    public float GetExpMultiplier()
    {
        if (PartyBuffSystem.Instance != null)
        {
            return 1f + PartyBuffSystem.Instance.GetBuffValue(PartyData.PartyBuffType.ExperienceBoost);
        }
        return 1f;
    }

    /// <summary>
    /// 获取金币加成
    /// </summary>
    public float GetGoldMultiplier()
    {
        if (PartyBuffSystem.Instance != null)
        {
            return 1f + PartyBuffSystem.Instance.GetBuffValue(PartyData.PartyBuffType.GoldBoost);
        }
        return 1f;
    }

    /// <summary>
    /// 获取掉落率加成
    /// </summary>
    public float GetDropRateMultiplier()
    {
        if (PartyBuffSystem.Instance != null)
        {
            return 1f + PartyBuffSystem.Instance.GetBuffValue(PartyData.PartyBuffType.DropRateBoost);
        }
        return 1f;
    }

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        data["share_exp"] = _shareExp;
        data["share_loot"] = _shareLoot;
        data["auto_accept"] = _autoAccept;
        data["exp_mode"] = (int)_expMode;
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.Contains("share_exp")) _shareExp = (bool)data["share_exp"];
        if (data.Contains("share_loot")) _shareLoot = (bool)data["share_loot"];
        if (data.Contains("auto_accept")) _autoAccept = (bool)data["auto_accept"];
        if (data.Contains("exp_mode")) _expMode = (PartyData.ExpDistributionMode)(int)data["exp_mode"];
    }
}
