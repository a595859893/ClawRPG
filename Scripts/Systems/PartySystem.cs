using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 多人游戏队伍系统
/// 队伍管理、Buff共享、经验加成
/// </summary>
public class PartySystem : BaseSystem
{
    public static PartySystem Instance { get; private set; }

    // 队伍成员
    public class PartyMember
    {
        public int PlayerId;
        public string PlayerName;
        public Vector2 Position;
        public int Level;
        public int Health;
        public int MaxHealth;
        public PartyRole Role;
        public bool IsOnline;
        public float LastUpdate;
    }

    // 队伍角色
    public enum PartyRole
    {
        Leader,      // 队长
        Tank,        // 坦克
        Healer,      // 治疗
        DamageDealer, // 输出
        Support,     // 辅助
        Scout        // 侦察
    }

    // 队伍Buff类型
    public enum PartyBuffType
    {
        ExperienceBoost,   // 经验加成
        GoldBoost,         // 金币加成
        DamageBoost,       // 伤害加成
        DefenseBoost,      // 防御加成
        HealthRegen,       // 生命恢复
        ManaRegen,         // 法力恢复
        LuckBoost,         // 幸运加成
        DropRateBoost      // 掉落率加成
    }

    // 队伍Buff
    public class PartyBuff
    {
        public PartyBuffType Type;
        public float Value;
        public float Duration;
        public float RemainingTime;
        public int ProviderId;
    }

    // 信号
    public delegate void PartyCreatedEvent(int partyId);
    public delegate void PartyJoinedEvent(int partyId);
    public delegate void PartyLeftEvent();
    public delegate void MemberJoinedEvent(int playerId, string playerName);
    public delegate void MemberLeftEvent(int playerId);
    public delegate void RoleChangedEvent(int playerId, PartyRole newRole);
    public delegate void BuffAddedEvent(PartyBuff buff);
    public delegate void BuffRemovedEvent(PartyBuffType buffType);
    public delegate void LeaderChangedEvent(int newLeaderId);
    public delegate void MemberStateUpdateEvent(int playerId, PartyMember member);

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

    // 状态
    private int _partyId = -1;
    private bool _isLeader = false;
    private int _localPlayerId = -1;
    private PartyRole _currentRole = PartyRole.DamageDealer;
    
    // 队伍成员
    private Dictionary<int, PartyMember> _members = new Dictionary<int, PartyMember>();
    private readonly object _membersLock = new object();

    // 队伍Buff
    private List<PartyBuff> _activeBuffs = new List<PartyBuff>();
    private readonly object _buffsLock = new object();

    // Buff配置
    private Dictionary<PartyBuffType, float> _buffDefaults = new Dictionary<PartyBuffType, float>
    {
        { PartyBuffType.ExperienceBoost, 0.10f },   // 10%
        { PartyBuffType.GoldBoost, 0.10f },         // 10%
        { PartyBuffType.DamageBoost, 0.05f },      // 5%
        { PartyBuffType.DefenseBoost, 0.05f },      // 5%
        { PartyBuffType.HealthRegen, 1.0f },        // 1hp/s
        { PartyBuffType.ManaRegen, 1.0f },          // 1mp/s
        { PartyBuffType.LuckBoost, 0.05f },         // 5%
        { PartyBuffType.DropRateBoost, 0.05f }      // 5%
    };

    // 队伍设置
    private bool _shareExp = true;
    private bool _shareLoot = false;
    private bool _autoAccept = false;

    // 经验分配模式
    private enum ExpDistributionMode
    {
        Equal,          // 平均分配
        BasedOnLevel,   // 按等级分配
        BasedOnDamage,  // 按伤害分配
        BasedOnHealing  // 按治疗分配
    }
    private ExpDistributionMode _expMode = ExpDistributionMode.Equal;

    public bool IsInParty => _partyId > 0;
    public bool IsLeader => _isLeader;
    public int PartyId => _partyId;
    public PartyRole CurrentRole => _currentRole;
    public bool ShareExp => _shareExp;
    public bool ShareLoot => _shareLoot;

    protected override void Initialize()
    {
        Instance = this;
    }

    public override void _Process(float delta)
    {
        if (!IsInParty) return;

        // 更新Buff时间
        UpdateBuffs(delta);
        
        // 同步成员状态
        SyncMemberState();
    }

    /// <summary>
    /// 创建队伍
    /// </summary>
    public void CreateParty(int playerId)
    {
        _localPlayerId = playerId;
        _partyId = (int)GD.Randi() % 10000 + 1;
        _isLeader = true;
        
        var member = new PartyMember
        {
            PlayerId = playerId,
            PlayerName = GetPlayerName(playerId),
            Level = GetPlayerLevel(playerId),
            Role = PartyRole.Leader,
            IsOnline = true,
            LastUpdate = OS.GetTicksMsec() / 1000f
        };
        
        lock (_membersLock)
        {
            _members[playerId] = member;
        }
        
        _currentRole = PartyRole.Leader;
        
        GD.Print($"[PartySystem] Party created: {_partyId}");
        OnPartyCreated?.Invoke(_partyId);
    }

    /// <summary>
    /// 加入队伍
    /// </summary>
    public void JoinParty(int partyId, int playerId)
    {
        _localPlayerId = playerId;
        _partyId = partyId;
        _isLeader = false;
        
        // 向服务器请求队伍成员列表
        RequestPartyMembers();
        
        GD.Print($"[PartySystem] Joined party: {_partyId}");
        OnPartyJoined?.Invoke(_partyId);
    }

    /// <summary>
    /// 离开队伍
    /// </summary>
    public void LeaveParty()
    {
        if (!IsInParty) return;

        // 通知服务器
        if (NetworkClient.Instance != null && NetworkClient.Instance.IsConnected)
        {
            var message = new Dictionary<string, object>
            {
                { "type", "party_leave" },
                { "party_id", _partyId },
                { "player_id", _localPlayerId }
            };
            NetworkClient.Instance.SendJson(message);
        }

        _partyId = -1;
        _isLeader = false;
        
        lock (_membersLock)
        {
            _members.Clear();
        }
        
        lock (_buffsLock)
        {
            _activeBuffs.Clear();
        }
        
        GD.Print("[PartySystem] Left party");
        OnPartyLeft?.Invoke();
    }

    /// <summary>
    /// 邀请玩家加入队伍
    /// </summary>
    public void InvitePlayer(int targetPlayerId)
    {
        if (!IsInParty || !_isLeader) return;

        if (NetworkClient.Instance != null && NetworkClient.Instance.IsConnected)
        {
            var message = new Dictionary<string, object>
            {
                { "type", "party_invite" },
                { "party_id", _partyId },
                { "inviter_id", _localPlayerId },
                { "target_id", targetPlayerId }
            };
            NetworkClient.Instance.SendJson(message);
        }
    }

    /// <summary>
    /// 踢出队伍成员
    /// </summary>
    public void KickMember(int memberId)
    {
        if (!IsInParty || !_isLeader) return;
        if (memberId == _localPlayerId) return; // 不能踢自己

        lock (_membersLock)
        {
            if (_members.ContainsKey(memberId))
            {
                var member = _members[memberId];
                
                // 通知服务器
                if (NetworkClient.Instance != null && NetworkClient.Instance.IsConnected)
                {
                    var message = new Dictionary<string, object>
                    {
                        { "type", "party_kick" },
                        { "party_id", _partyId },
                        { "kicker_id", _localPlayerId },
                        { "target_id", memberId }
                    };
                    NetworkClient.Instance.SendJson(message);
                }
                
                _members.Remove(memberId);
                OnMemberLeft?.Invoke(memberId);
            }
        }
    }

    /// <summary>
    /// 转让队长
    /// </summary>
    public void TransferLeadership(int newLeaderId)
    {
        if (!IsInParty || !_isLeader) return;
        if (!_members.ContainsKey(newLeaderId)) return;

        // 通知服务器
        if (NetworkClient.Instance != null && NetworkClient.Instance.IsConnected)
        {
            var message = new Dictionary<string, object>
            {
                { "type", "party_transfer_leader" },
                { "party_id", _partyId },
                { "current_leader_id", _localPlayerId },
                { "new_leader_id", newLeaderId }
            };
            NetworkClient.Instance.SendJson(message);
        }

        // 本地更新
        lock (_membersLock)
        {
            if (_members.ContainsKey(_localPlayerId))
            {
                _members[_localPlayerId].Role = PartyRole.DamageDealer;
            }
            if (_members.ContainsKey(newLeaderId))
            {
                _members[newLeaderId].Role = PartyRole.Leader;
            }
        }
        
        _isLeader = false;
        if (newLeaderId == _localPlayerId)
        {
            _isLeader = true;
        }
        
        OnLeaderChanged?.Invoke(newLeaderId);
    }

    /// <summary>
    /// 设置成员角色
    /// </summary>
    public void SetMemberRole(int memberId, PartyRole role)
    {
        if (!IsInParty || !_isLeader) return;
        
        lock (_membersLock)
        {
            if (_members.ContainsKey(memberId))
            {
                _members[memberId].Role = role;
                OnRoleChanged?.Invoke(memberId, role);
            }
        }
    }

    /// <summary>
    /// 设置自己的角色
    /// </summary>
    public void SetRole(PartyRole role)
    {
        if (!IsInParty) return;
        
        _currentRole = role;
        
        lock (_membersLock)
        {
            if (_members.ContainsKey(_localPlayerId))
            {
                _members[_localPlayerId].Role = role;
            }
        }
        
        // 通知服务器
        if (NetworkClient.Instance != null && NetworkClient.Instance.IsConnected)
        {
            var message = new Dictionary<string, object>
            {
                { "type", "party_set_role" },
                { "party_id", _partyId },
                { "player_id", _localPlayerId },
                { "role", role.ToString() }
            };
            NetworkClient.Instance.SendJson(message);
        }
    }

    /// <summary>
    /// 添加队伍Buff
    /// </summary>
    public void AddBuff(PartyBuffType type, float value, float duration, int providerId)
    {
        var buff = new PartyBuff
        {
            Type = type,
            Value = value,
            Duration = duration,
            RemainingTime = duration,
            ProviderId = providerId
        };
        
        lock (_buffsLock)
        {
            // 移除同类型的旧Buff
            _activeBuffs.RemoveAll(b => b.Type == type);
            _activeBuffs.Add(buff);
        }
        
        OnBuffAdded?.Invoke(buff);
        
        // 通知其他成员
        if (NetworkClient.Instance != null && NetworkClient.Instance.IsConnected)
        {
            var message = new Dictionary<string, object>
            {
                { "type", "party_add_buff" },
                { "party_id", _partyId },
                { "buff_type", type.ToString() },
                { "value", value },
                { "duration", duration },
                { "provider_id", providerId }
            };
            NetworkClient.Instance.SendJson(message);
        }
    }

    /// <summary>
    /// 移除队伍Buff
    /// </summary>
    public void RemoveBuff(PartyBuffType type)
    {
        lock (_buffsLock)
        {
            _activeBuffs.RemoveAll(b => b.Type == type);
        }
        
        OnBuffRemoved?.Invoke(type);
    }

    /// <summary>
    /// 获取队伍Buff效果
    /// </summary>
    public float GetBuffValue(PartyBuffType type)
    {
        lock (_buffsLock)
        {
            foreach (var buff in _activeBuffs)
            {
                if (buff.Type == type)
                {
                    return buff.Value;
                }
            }
        }
        return 0f;
    }

    /// <summary>
    /// 获取所有Buff效果
    /// </summary>
    public Dictionary<PartyBuffType, float> GetAllBuffValues()
    {
        var result = new Dictionary<PartyBuffType, float>();
        
        lock (_buffsLock)
        {
            foreach (var buff in _activeBuffs)
            {
                if (!result.ContainsKey(buff.Type))
                {
                    result[buff.Type] = 0f;
                }
                result[buff.Type] += buff.Value;
            }
        }
        
        return result;
    }

    /// <summary>
    /// 更新Buff时间
    /// </summary>
    private void UpdateBuffs(float delta)
    {
        lock (_buffsLock)
        {
            for (int i = _activeBuffs.Count - 1; i >= 0; i--)
            {
                var buff = _activeBuffs[i];
                buff.RemainingTime -= delta;
                
                if (buff.RemainingTime <= 0)
                {
                    _activeBuffs.RemoveAt(i);
                    OnBuffRemoved?.Invoke(buff.Type);
                }
            }
        }
    }

    /// <summary>
    /// 同步成员状态
    /// </summary>
    private void SyncMemberState()
    {
        if (_localPlayerId <= 0) return;
        
        lock (_membersLock)
        {
            if (_members.ContainsKey(_localPlayerId))
            {
                var member = _members[_localPlayerId];
                member.Position = GetPlayerPosition(_localPlayerId);
                member.Health = GetPlayerHealth(_localPlayerId);
                member.MaxHealth = GetPlayerMaxHealth(_localPlayerId);
                member.Level = GetPlayerLevel(_localPlayerId);
                member.LastUpdate = OS.GetTicksMsec() / 1000f;
                
                // 发送给服务器
                if (NetworkClient.Instance != null && NetworkClient.Instance.IsConnected)
                {
                    var message = new Dictionary<string, object>
                    {
                        { "type", "party_member_state" },
                        { "party_id", _partyId },
                        { "player_id", _localPlayerId },
                        { "position", new Dictionary<string, float> { { "x", member.Position.X }, { "y", member.Position.Y } } },
                        { "health", member.Health },
                        { "max_health", member.MaxHealth },
                        { "level", member.Level }
                    };
                    NetworkClient.Instance.SendJson(message);
                }
            }
        }
    }

    /// <summary>
    /// 请求队伍成员列表
    /// </summary>
    private void RequestPartyMembers()
    {
        if (NetworkClient.Instance != null && NetworkClient.Instance.IsConnected)
        {
            var message = new Dictionary<string, object>
            {
                { "type", "party_request_members" },
                { "party_id", _partyId },
                { "player_id", _localPlayerId }
            };
            NetworkClient.Instance.SendJson(message);
        }
    }

    /// <summary>
    /// 处理服务器消息
    /// </summary>
    public void HandleMessage(Dictionary<string, object> data)
    {
        if (!data.ContainsKey("type")) return;
        
        string msgType = data["type"].ToString();
        
        switch (msgType)
        {
            case "party_created":
                _partyId = Convert.ToInt32(data["party_id"]);
                _isLeader = true;
                _localPlayerId = Convert.ToInt32(data["player_id"]);
                OnPartyCreated?.Invoke(_partyId);
                break;
                
            case "party_joined":
                _partyId = Convert.ToInt32(data["party_id"]);
                _localPlayerId = Convert.ToInt32(data["player_id"]);
                OnPartyJoined?.Invoke(_partyId);
                break;
                
            case "party_member_joined":
                int memberId = Convert.ToInt32(data["player_id"]);
                string memberName = data["player_name"].ToString();
                var newMember = new PartyMember
                {
                    PlayerId = memberId,
                    PlayerName = memberName,
                    Level = data.ContainsKey("level") ? Convert.ToInt32(data["level"]) : 1,
                    Role = PartyRole.DamageDealer,
                    IsOnline = true,
                    LastUpdate = OS.GetTicksMsec() / 1000f
                };
                lock (_membersLock)
                {
                    _members[memberId] = newMember;
                }
                OnMemberJoined?.Invoke(memberId, memberName);
                break;
                
            case "party_member_left":
                int leftId = Convert.ToInt32(data["player_id"]);
                lock (_membersLock)
                {
                    _members.Remove(leftId);
                }
                OnMemberLeft?.Invoke(leftId);
                break;
                
            case "party_leader_changed":
                int newLeader = Convert.ToInt32(data["new_leader_id"]);
                _isLeader = (newLeader == _localPlayerId);
                lock (_membersLock)
                {
                    foreach (var kvp in _members)
                    {
                        kvp.Value.Role = (kvp.Key == newLeader) ? PartyRole.Leader : PartyRole.DamageDealer;
                    }
                }
                OnLeaderChanged?.Invoke(newLeader);
                break;
                
            case "party_buff_added":
                var buffType = Enum.Parse<PartyBuffType>(data["buff_type"].ToString());
                float buffValue = Convert.ToSingle(data["value"]);
                float buffDuration = Convert.ToSingle(data["duration"]);
                int provider = Convert.ToInt32(data["provider_id"]);
                AddBuff(buffType, buffValue, buffDuration, provider);
                break;
                
            case "party_member_state":
                int statePlayerId = Convert.ToInt32(data["player_id"]);
                lock (_membersLock)
                {
                    if (_members.ContainsKey(statePlayerId))
                    {
                        var member = _members[statePlayerId];
                        if (data.ContainsKey("position"))
                        {
                            var pos = data["position"] as Dictionary<string, object>;
                            member.Position = new Vector2(Convert.ToSingle(pos["x"]), Convert.ToSingle(pos["y"]));
                        }
                        if (data.ContainsKey("health"))
                            member.Health = Convert.ToInt32(data["health"]);
                        if (data.ContainsKey("max_health"))
                            member.MaxHealth = Convert.ToInt32(data["max_health"]);
                        if (data.ContainsKey("level"))
                            member.Level = Convert.ToInt32(data["level"]);
                        member.LastUpdate = OS.GetTicksMsec() / 1000f;
                        
                        OnMemberStateUpdate?.Invoke(statePlayerId, member);
                    }
                }
                break;
                
            case "party_disbanded":
                _partyId = -1;
                lock (_membersLock)
                {
                    _members.Clear();
                }
                lock (_buffsLock)
                {
                    _activeBuffs.Clear();
                }
                OnPartyLeft?.Invoke();
                break;
        }
    }

    /// <summary>
    /// 获取队伍成员列表
    /// </summary>
    public List<PartyMember> GetMembers()
    {
        lock (_membersLock)
        {
            return new List<PartyMember>(_members.Values);
        }
    }

    /// <summary>
    /// 获取指定成员
    /// </summary>
    public PartyMember GetMember(int playerId)
    {
        lock (_membersLock)
        {
            return _members.ContainsKey(playerId) ? _members[playerId] : null;
        }
    }

    /// <summary>
    /// 获取在线成员数
    /// </summary>
    public int GetOnlineMemberCount()
    {
        int count = 0;
        lock (_membersLock)
        {
            foreach (var member in _members.Values)
            {
                if (member.IsOnline) count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 获取队伍平均等级
    /// </summary>
    public float GetAverageLevel()
    {
        int totalLevel = 0;
        int count = 0;
        
        lock (_membersLock)
        {
            foreach (var member in _members.Values)
            {
                if (member.IsOnline)
                {
                    totalLevel += member.Level;
                    count++;
                }
            }
        }
        
        return count > 0 ? (float)totalLevel / count : 0;
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
    public void SetExpDistributionMode(ExpDistributionMode mode)
    {
        _expMode = mode;
    }

    /// <summary>
    /// 计算经验分配
    /// </summary>
    public Dictionary<int, int> CalculateExpDistribution(int totalExp)
    {
        var distribution = new Dictionary<int, int>();
        
        if (!_shareExp)
        {
            distribution[_localPlayerId] = totalExp;
            return distribution;
        }
        
        lock (_membersLock)
        {
            List<int> onlineMembers = new List<int>();
            foreach (var member in _members.Values)
            {
                if (member.IsOnline)
                    onlineMembers.Add(member.PlayerId);
            }
            
            int memberCount = onlineMembers.Count;
            if (memberCount == 0)
            {
                distribution[_localPlayerId] = totalExp;
                return distribution;
            }
            
            switch (_expMode)
            {
                case ExpDistributionMode.Equal:
                    int expPerMember = totalExp / memberCount;
                    foreach (var id in onlineMembers)
                        distribution[id] = expPerMember;
                    break;
                    
                case ExpDistributionMode.BasedOnLevel:
                    int totalLevel = 0;
                    Dictionary<int, int> memberLevels = new Dictionary<int, int>();
                    foreach (var id in onlineMembers)
                    {
                        int level = _members[id].Level;
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
        }
        
        return distribution;
    }

    // 辅助方法 - 需要从GameManager获取
    private string GetPlayerName(int playerId)
    {
        var root = GetTree().Root;
        foreach (Node child in root.GetChildren())
        {
            if (child is GameManager gm)
            {
                return gm.GetPlayerName(playerId);
            }
        }
        return "Player" + playerId;
    }

    private int GetPlayerLevel(int playerId)
    {
        var root = GetTree().Root;
        foreach (Node child in root.GetChildren())
        {
            if (child is GameManager gm)
            {
                return gm.GetPlayerLevel(playerId);
            }
        }
        return 1;
    }

    private Vector2 GetPlayerPosition(int playerId)
    {
        var root = GetTree().Root;
        foreach (Node child in root.GetChildren())
        {
            if (child is Player player && player.PlayerId == playerId)
            {
                return player.Position;
            }
        }
        return Vector2.Zero;
    }

    private int GetPlayerHealth(int playerId)
    {
        var root = GetTree().Root;
        foreach (Node child in root.GetChildren())
        {
            if (child is Player player && player.PlayerId == playerId)
            {
                return player.Health;
            }
        }
        return 100;
    }

    private int GetPlayerMaxHealth(int playerId)
    {
        var root = GetTree().Root;
        foreach (Node child in root.GetChildren())
        {
            if (child is Player player && player.PlayerId == playerId)
            {
                return player.MaxHealth;
            }
        }
        return 100;
    }

    public override void _ExitTree()
    {
        LeaveParty();
        Instance = null;
    }

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        data["party_id"] = _partyId;
        data["is_leader"] = _isLeader;
        data["local_player_id"] = _localPlayerId;
        data["current_role"] = (int)_currentRole;
        data["share_exp"] = _shareExp;
        data["share_loot"] = _shareLoot;
        data["auto_accept"] = _autoAccept;
        data["exp_mode"] = (int)_expMode;
        
        // 队伍成员
        var members = new Array();
        lock (_membersLock)
        {
            foreach (var kvp in _members)
            {
                var member = new Dictionary();
                member["player_id"] = kvp.Value.PlayerId;
                member["player_name"] = kvp.Value.PlayerName;
                member["level"] = kvp.Value.Level;
                member["health"] = kvp.Value.Health;
                member["max_health"] = kvp.Value.MaxHealth;
                member["role"] = (int)kvp.Value.Role;
                member["is_online"] = kvp.Value.IsOnline;
                members.Add(member);
            }
        }
        data["members"] = members;
        
        // 队伍Buff
        var buffs = new Array();
        lock (_buffsLock)
        {
            foreach (var buff in _activeBuffs)
            {
                var b = new Dictionary();
                b["type"] = (int)buff.Type;
                b["value"] = buff.Value;
                b["duration"] = buff.Duration;
                b["remaining_time"] = buff.RemainingTime;
                b["provider_id"] = buff.ProviderId;
                buffs.Add(b);
            }
        }
        data["active_buffs"] = buffs;
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("party_id")) _partyId = (int)data["party_id"];
        if (data.Contains("is_leader")) _isLeader = (bool)data["is_leader"];
        if (data.Contains("local_player_id")) _localPlayerId = (int)data["local_player_id"];
        if (data.Contains("current_role")) _currentRole = (PartyRole)(int)data["current_role"];
        if (data.Contains("share_exp")) _shareExp = (bool)data["share_exp"];
        if (data.Contains("share_loot")) _shareLoot = (bool)data["share_loot"];
        if (data.Contains("auto_accept")) _autoAccept = (bool)data["auto_accept"];
        if (data.Contains("exp_mode")) _expMode = (ExpDistributionMode)(int)data["exp_mode"];
        
        // 队伍成员
        if (data.Contains("members"))
        {
            lock (_membersLock)
            {
                _members.Clear();
                var members = (Array)data["members"];
                foreach (Dictionary member in members)
                {
                    var pm = new PartyMember
                    {
                        PlayerId = (int)member["player_id"],
                        PlayerName = (string)member["player_name"],
                        Level = (int)member["level"],
                        Health = (int)member["health"],
                        MaxHealth = (int)member["max_health"],
                        Role = (PartyRole)(int)member["role"],
                        IsOnline = (bool)member["is_online"],
                        LastUpdate = OS.GetTicksMsec() / 1000f
                    };
                    _members[pm.PlayerId] = pm;
                }
            }
        }
        
        // 队伍Buff
        if (data.Contains("active_buffs"))
        {
            lock (_buffsLock)
            {
                _activeBuffs.Clear();
                var buffs = (Array)data["active_buffs"];
                foreach (Dictionary b in buffs)
                {
                    var buff = new PartyBuff
                    {
                        Type = (PartyBuffType)(int)b["type"],
                        Value = (float)b["value"],
                        Duration = (float)b["duration"],
                        RemainingTime = (float)b["remaining_time"],
                        ProviderId = (int)b["provider_id"]
                    };
                    _activeBuffs.Add(buff);
                }
            }
        }
    }
}
