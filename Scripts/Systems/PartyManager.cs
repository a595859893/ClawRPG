using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

/// <summary>
/// 队伍管理器 - 负责组队、离队、邀请等核心逻辑
/// </summary>
public class PartyManager : BaseSystem
{
    public static PartyManager Instance { get; private set; }

    // 状态
    private int _partyId = -1;
    private bool _isLeader = false;
    private int _localPlayerId = -1;
    private PartyData.PartyRole _currentRole = PartyData.PartyRole.DamageDealer;
    
    // 队伍成员
    private Dictionary<int, PartyData.PartyMember> _members = new Dictionary<int, PartyData.PartyMember>();
    private readonly object _membersLock = new object();

    // 事件信号
    public delegate void PartyCreatedEvent(int partyId);
    public delegate void PartyJoinedEvent(int partyId);
    public delegate void PartyLeftEvent();
    public delegate void MemberJoinedEvent(int playerId, string playerName);
    public delegate void MemberLeftEvent(int playerId);
    public delegate void RoleChangedEvent(int playerId, PartyData.PartyRole newRole);
    public delegate void LeaderChangedEvent(int newLeaderId);
    public delegate void MemberStateUpdateEvent(int playerId, PartyData.PartyMember member);

    public event PartyCreatedEvent OnPartyCreated;
    public event PartyJoinedEvent OnPartyJoined;
    public event PartyLeftEvent OnPartyLeft;
    public event MemberJoinedEvent OnMemberJoined;
    public event MemberLeftEvent OnMemberLeft;
    public event RoleChangedEvent OnRoleChanged;
    public event LeaderChangedEvent OnLeaderChanged;
    public event MemberStateUpdateEvent OnMemberStateUpdate;

    public bool IsInParty => _partyId > 0;
    public bool IsLeader => _isLeader;
    public int PartyId => _partyId;
    public PartyData.PartyRole CurrentRole => _currentRole;

    protected override void Initialize()
    {
        Instance = this;
        GD.Print("[PartyManager] Initialized");
    }

    public override void _Process(double delta)
    {
        if (!IsInParty) return;
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
        
        var member = new PartyData.PartyMember
        {
            PlayerId = playerId,
            PlayerName = GetPlayerName(playerId),
            Level = GetPlayerLevel(playerId),
            Role = PartyData.PartyRole.Leader,
            IsOnline = true,
            LastUpdate = OS.GetTicksMsec() / 1000f
        };
        
        lock (_membersLock)
        {
            _members[playerId] = member;
        }
        
        _currentRole = PartyData.PartyRole.Leader;
        
        GD.Print($"[PartyManager] Party created: {_partyId}");
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
        
        RequestPartyMembers();
        
        GD.Print($"[PartyManager] Joined party: {_partyId}");
        OnPartyJoined?.Invoke(_partyId);
    }

    /// <summary>
    /// 离开队伍
    /// </summary>
    public void LeaveParty()
    {
        if (!IsInParty) return;

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
        
        GD.Print("[PartyManager] Left party");
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
        if (memberId == _localPlayerId) return;

        lock (_membersLock)
        {
            if (_members.ContainsKey(memberId))
            {
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

        lock (_membersLock)
        {
            if (_members.ContainsKey(_localPlayerId))
            {
                _members[_localPlayerId].Role = PartyData.PartyRole.DamageDealer;
            }
            if (_members.ContainsKey(newLeaderId))
            {
                _members[newLeaderId].Role = PartyData.PartyRole.Leader;
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
    public void SetMemberRole(int memberId, PartyData.PartyRole role)
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
    public void SetRole(PartyData.PartyRole role)
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
    /// 获取队伍成员列表
    /// </summary>
    public List<PartyData.PartyMember> GetMembers()
    {
        lock (_membersLock)
        {
            return new List<PartyData.PartyMember>(_members.Values);
        }
    }

    /// <summary>
    /// 获取指定成员
    /// </summary>
    public PartyData.PartyMember GetMember(int playerId)
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
                var newMember = new PartyData.PartyMember
                {
                    PlayerId = memberId,
                    PlayerName = memberName,
                    Level = data.ContainsKey("level") ? Convert.ToInt32(data["level"]) : 1,
                    Role = PartyData.PartyRole.DamageDealer,
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
                        kvp.Value.Role = (kvp.Key == newLeader) ? PartyData.PartyRole.Leader : PartyData.PartyRole.DamageDealer;
                    }
                }
                OnLeaderChanged?.Invoke(newLeader);
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
                OnPartyLeft?.Invoke();
                break;
        }
    }

    // 辅助方法
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
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        data["party_id"] = _partyId;
        data["is_leader"] = _isLeader;
        data["local_player_id"] = _localPlayerId;
        data["current_role"] = (int)_currentRole;
        
        var members = new Godot.Collections.Array();
        lock (_membersLock)
        {
            foreach (var kvp in _members)
            {
                var member = new Dictionary<string, object>();
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
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.Contains("party_id")) _partyId = (int)data["party_id"];
        if (data.Contains("is_leader")) _isLeader = (bool)data["is_leader"];
        if (data.Contains("local_player_id")) _localPlayerId = (int)data["local_player_id"];
        if (data.Contains("current_role")) _currentRole = (PartyData.PartyRole)(int)data["current_role"];
        
        if (data.Contains("members"))
        {
            lock (_membersLock)
            {
                _members.Clear();
                var members = (Godot.Collections.Array)data["members"];
                foreach (Dictionary member in members)
                {
                    var pm = new PartyData.PartyMember
                    {
                        PlayerId = (int)member["player_id"],
                        PlayerName = (string)member["player_name"],
                        Level = (int)member["level"],
                        Health = (int)member["health"],
                        MaxHealth = (int)member["max_health"],
                        Role = (PartyData.PartyRole)(int)member["role"],
                        IsOnline = (bool)member["is_online"],
                        LastUpdate = OS.GetTicksMsec() / 1000f
                    };
                    _members[pm.PlayerId] = pm;
                }
            }
        }
    }
}
