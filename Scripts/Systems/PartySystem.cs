using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 队伍系统协调器 - 提供统一的队伍API入口
/// 内部委托给PartyManager、PartyBuffSystem、PartyLootSystem处理
/// </summary>
public class PartySystem : BaseSystem
{
    public static PartySystem Instance { get; private set; }

    // 事件信号 - 兼容旧API
    public delegate void PartyCreatedEvent(int partyId);
    public delegate void PartyJoinedEvent(int partyId);
    public delegate void PartyLeftEvent();
    public delegate void MemberJoinedEvent(int playerId, string playerName);
    public delegate void MemberLeftEvent(int playerId);
    public delegate void RoleChangedEvent(int playerId, PartyData.PartyRole newRole);
    public delegate void BuffAddedEvent(PartyData.PartyBuff buff);
    public delegate void BuffRemovedEvent(PartyData.PartyBuffType buffType);
    public delegate void LeaderChangedEvent(int newLeaderId);
    public delegate void MemberStateUpdateEvent(int playerId, PartyData.PartyMember member);

    public event PartyCreatedEvent OnPartyCreated;
    public event PartyJoinedEvent OnPartyJoined;
    public event PartyLeftEvent OnPartyLeft;
    public event MemberJoinedEvent OnMemberJoined;
    public event MemberLeftEvent OnMemberLeft;
    public event RoleChangedEvent OnRoleChanged;
    public event BuffAddedEvent OnBuffAdded;
    public event BuffRemovedEvent OnBuffRemoved;
    public event LeaderChangedEvent OnLeaderChanged;
    public event MemberStateUpdateEvent OnMemberStateUpdate;

    // 属性 - 兼容旧API
    public bool IsInParty => PartyManager.Instance != null && PartyManager.Instance.IsInParty;
    public bool IsLeader => PartyManager.Instance != null && PartyManager.Instance.IsLeader;
    public int PartyId => PartyManager.Instance != null ? PartyManager.Instance.PartyId : -1;
    public PartyData.PartyRole CurrentRole => PartyManager.Instance != null ? PartyManager.Instance.CurrentRole : PartyData.PartyRole.DamageDealer;
    public bool ShareExp => PartyLootSystem.Instance != null && PartyLootSystem.Instance.ShareExp;
    public bool ShareLoot => PartyLootSystem.Instance != null && PartyLootSystem.Instance.ShareLoot;

    // 内部系统引用
    private PartyManager _partyManager;
    private PartyBuffSystem _buffSystem;
    private PartyLootSystem _lootSystem;

    protected override void Initialize()
    {
        Instance = this;
        
        // 创建子系统（作为子节点）
        _partyManager = new PartyManager();
        _partyManager.Name = "PartyManager";
        AddChild(_partyManager);
        
        _buffSystem = new PartyBuffSystem();
        _buffSystem.Name = "PartyBuffSystem";
        AddChild(_buffSystem);
        
        _lootSystem = new PartyLootSystem();
        _lootSystem.Name = "PartyLootSystem";
        AddChild(_lootSystem);
        
        // 绑定事件
        BindEvents();
        
        GD.Print("[PartySystem] Initialized");
    }

    private void BindEvents()
    {
        if (_partyManager != null)
        {
            _partyManager.OnPartyCreated += (id) => OnPartyCreated?.Invoke(id);
            _partyManager.OnPartyJoined += (id) => OnPartyJoined?.Invoke(id);
            _partyManager.OnPartyLeft += () => OnPartyLeft?.Invoke();
            _partyManager.OnMemberJoined += (id, name) => OnMemberJoined?.Invoke(id, name);
            _partyManager.OnMemberLeft += (id) => OnMemberLeft?.Invoke(id);
            _partyManager.OnRoleChanged += (id, role) => OnRoleChanged?.Invoke(id, role);
            _partyManager.OnLeaderChanged += (id) => OnLeaderChanged?.Invoke(id);
            _partyManager.OnMemberStateUpdate += (id, member) => OnMemberStateUpdate?.Invoke(id, member);
        }
        
        if (_buffSystem != null)
        {
            _buffSystem.OnBuffAdded += (buff) => OnBuffAdded?.Invoke(buff);
            _buffSystem.OnBuffRemoved += (type) => OnBuffRemoved?.Invoke(type);
        }
    }

    // ============ 组队管理 API ============

    /// <summary>
    /// 创建队伍
    /// </summary>
    public void CreateParty(int playerId)
    {
        _partyManager?.CreateParty(playerId);
    }

    /// <summary>
    /// 加入队伍
    /// </summary>
    public void JoinParty(int partyId, int playerId)
    {
        _partyManager?.JoinParty(partyId, playerId);
    }

    /// <summary>
    /// 离开队伍
    /// </summary>
    public void LeaveParty()
    {
        _partyManager?.LeaveParty();
        _buffSystem?.ClearAllBuffs();
    }

    /// <summary>
    /// 邀请玩家加入队伍
    /// </summary>
    public void InvitePlayer(int targetPlayerId)
    {
        _partyManager?.InvitePlayer(targetPlayerId);
    }

    /// <summary>
    /// 踢出队伍成员
    /// </summary>
    public void KickMember(int memberId)
    {
        _partyManager?.KickMember(memberId);
    }

    /// <summary>
    /// 转让队长
    /// </summary>
    public void TransferLeadership(int newLeaderId)
    {
        _partyManager?.TransferLeadership(newLeaderId);
    }

    /// <summary>
    /// 设置成员角色
    /// </summary>
    public void SetMemberRole(int memberId, PartyData.PartyRole role)
    {
        _partyManager?.SetMemberRole(memberId, role);
    }

    /// <summary>
    /// 设置自己的角色
    /// </summary>
    public void SetRole(PartyData.PartyRole role)
    {
        _partyManager?.SetRole(role);
    }

    /// <summary>
    /// 获取队伍成员列表
    /// </summary>
    public List<PartyData.PartyMember> GetMembers()
    {
        return _partyManager?.GetMembers() ?? new List<PartyData.PartyMember>();
    }

    /// <summary>
    /// 获取指定成员
    /// </summary>
    public PartyData.PartyMember GetMember(int playerId)
    {
        return _partyManager?.GetMember(playerId);
    }

    /// <summary>
    /// 获取在线成员数
    /// </summary>
    public int GetOnlineMemberCount()
    {
        return _partyManager?.GetOnlineMemberCount() ?? 0;
    }

    /// <summary>
    /// 获取队伍平均等级
    /// </summary>
    public float GetAverageLevel()
    {
        return _partyManager?.GetAverageLevel() ?? 0;
    }

    // ============ Buff API ============

    /// <summary>
    /// 添加队伍Buff
    /// </summary>
    public void AddBuff(PartyData.PartyBuffType type, float value, float duration, int providerId)
    {
        _buffSystem?.AddBuff(type, value, duration, providerId);
    }

    /// <summary>
    /// 移除队伍Buff
    /// </summary>
    public void RemoveBuff(PartyData.PartyBuffType type)
    {
        _buffSystem?.RemoveBuff(type);
    }

    /// <summary>
    /// 获取队伍Buff效果
    /// </summary>
    public float GetBuffValue(PartyData.PartyBuffType type)
    {
        return _buffSystem?.GetBuffValue(type) ?? 0f;
    }

    /// <summary>
    /// 获取所有Buff效果
    /// </summary>
    public Dictionary<PartyData.PartyBuffType, float> GetAllBuffValues()
    {
        return _buffSystem?.GetAllBuffValues() ?? new Dictionary<PartyData.PartyBuffType, float>();
    }

    // ============ 战利品API ============

    /// <summary>
    /// 设置经验共享
    /// </summary>
    public void SetShareExp(bool share)
    {
        _lootSystem?.SetShareExp(share);
    }

    /// <summary>
    /// 设置战利品共享
    /// </summary>
    public void SetShareLoot(bool share)
    {
        _lootSystem?.SetShareLoot(share);
    }

    /// <summary>
    /// 设置自动接受邀请
    /// </summary>
    public void SetAutoAccept(bool autoAccept)
    {
        _lootSystem?.SetAutoAccept(autoAccept);
    }

    /// <summary>
    /// 设置经验分配模式
    /// </summary>
    public void SetExpDistributionMode(PartyData.ExpDistributionMode mode)
    {
        _lootSystem?.SetExpDistributionMode(mode);
    }

    /// <summary>
    /// 计算经验分配
    /// </summary>
    public Dictionary<int, int> CalculateExpDistribution(int totalExp)
    {
        return _lootSystem?.CalculateExpDistribution(totalExp) ?? new Dictionary<int, int>();
    }

    /// <summary>
    /// 获取经验倍率
    /// </summary>
    public float GetExpMultiplier()
    {
        return _lootSystem?.GetExpMultiplier() ?? 1f;
    }

    /// <summary>
    /// 获取金币倍率
    /// </summary>
    public float GetGoldMultiplier()
    {
        return _lootSystem?.GetGoldMultiplier() ?? 1f;
    }

    /// <summary>
    /// 获取掉落率倍率
    /// </summary>
    public float GetDropRateMultiplier()
    {
        return _lootSystem?.GetDropRateMultiplier() ?? 1f;
    }

    // ============ 消息处理 ============

    /// <summary>
    /// 处理服务器消息
    /// </summary>
    public void HandleMessage(Dictionary<string, object> data)
    {
        _partyManager?.HandleMessage(data);
        _buffSystem?.HandleMessage(data);
    }

    public override void _ExitTree()
    {
        _partyManager?.LeaveParty();
        Instance = null;
    }

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        if (_partyManager != null)
        {
            foreach (var kvp in _partyManager.ExportSaveData())
            {
                data[kvp.Key] = kvp.Value;
            }
        }
        
        if (_buffSystem != null)
        {
            foreach (var kvp in _buffSystem.ExportSaveData())
            {
                data[kvp.Key] = kvp.Value;
            }
        }
        
        if (_lootSystem != null)
        {
            foreach (var kvp in _lootSystem.ExportSaveData())
            {
                data[kvp.Key] = kvp.Value;
            }
        }
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        _partyManager?.ImportSaveData(data);
        _buffSystem?.ImportSaveData(data);
        _lootSystem?.ImportSaveData(data);
    }
}
