using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 多人游戏队伍系统
/// 队伍管理、权限、buff共享
/// </summary>
public class TeamSystem : BaseSystem
{
    public static TeamSystem Instance { get; private set; }

    // 队伍成员
    public class TeamMember
    {
        public int PlayerId;
        public string PlayerName;
        public bool IsReady;
        public bool IsHost;
        public Vector2 Position;
        public int Health;
        public int MaxHealth;
        public float DamageDealt;      // 本场战斗伤害
        public float HealingDone;      // 本场战斗治疗
        public int EnemiesKilled;      // 本场战斗击杀

        public TeamMember(int playerId, string playerName, bool isHost = false)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            IsHost = isHost;
            IsReady = false; 
            Position = Vector2.Zero;
            Health = 100;
            MaxHealth = 100;
            DamageDealt = 0;
            HealingDone = 0;
            EnemiesKilled = 0;
        }
    }

    // 队伍信息
    public class TeamInfo
    {
        public string TeamId;
        public string TeamName;
        public List<TeamMember> Members = new List<TeamMember>();
        public int MaxMembers = 4;
        public bool IsPublic = true;
        public float ShareRange = 500f;  // 经验/掉落共享范围
        public bool ShareLoot = true;     // 掉落共享
        public bool ShareExp = true;      // 经验共享

        public TeamInfo(string teamId, string teamName)
        {
            TeamId = teamId;
            TeamName = teamName;
        }
    }

    // 信号
    public delegate void TeamCreatedEvent(TeamInfo team);
    public delegate void TeamJoinedEvent(TeamInfo team);
    public delegate void TeamLeftEvent();
    public delegate void MemberJoinedEvent(TeamMember member);
    public delegate void MemberLeftEvent(int playerId);
    public delegate void MemberReadyEvent(int playerId, bool isReady);
    public delegate void MemberKickedEvent(int playerId);
    public delegate void HostChangedEvent(int newHostId);

    public event TeamCreatedEvent OnTeamCreated;
    public event TeamJoinedEvent OnTeamJoined;
    public event TeamLeftEvent OnTeamLeft;
    public event MemberJoinedEvent OnMemberJoined;
    public event MemberLeftEvent OnMemberLeft;
    public event MemberReadyEvent OnMemberReady;
    public event MemberKickedEvent OnMemberKicked;
    public event HostChangedEvent OnHostChanged;

    // 状态
    private TeamInfo _currentTeam;
    private int _localPlayerId = -1;
    private bool _isInTeam = false; 

    public bool IsInTeam => _isInTeam;
    public TeamInfo CurrentTeam => _currentTeam;
    public bool IsHost => _currentTeam != null && _currentTeam.Members.Find(m => m.PlayerId == _localPlayerId)?.IsHost ?? false;

    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary>
    /// 创建队伍
    /// </summary>
    public void CreateTeam(string teamName)
    {
        _localPlayerId = MultiplayerManager.Instance.LocalPlayerId;
        var playerName = MultiplayerManager.Instance.PlayerName;

        _currentTeam = new TeamInfo(Guid.NewGuid().ToString("N")[..8], teamName);
        
        var hostMember = new TeamMember(_localPlayerId, playerName, true);
        _currentTeam.Members.Add(hostMember);
        
        _isInTeam = true;

        // 发送到服务器
        var data = new Dictionary<string, object>
        {
            { "type", "create_team" },
            { "team_name", teamName },
            { "player_id", _localPlayerId },
            { "player_name", playerName }
        };
        NetworkClient.Instance.SendJson(data);

        OnTeamCreated?.Invoke(_currentTeam);
    }

    /// <summary>
    /// 加入队伍
    /// </summary>
    public void JoinTeam(string teamId)
    {
        _localPlayerId = MultiplayerManager.Instance.LocalPlayerId;
        var playerName = MultiplayerManager.Instance.PlayerName;

        var data = new Dictionary<string, object>
        {
            { "type", "join_team" },
            { "team_id", teamId },
            { "player_id", _localPlayerId },
            { "player_name", playerName }
        };
        NetworkClient.Instance.SendJson(data);
    }

    /// <summary>
    /// 离开队伍
    /// </summary>
    public void LeaveTeam()
    {
        if (!_isInTeam || _currentTeam == null) return;

        var data = new Dictionary<string, object>
        {
            { "type", "leave_team" },
            { "team_id", _currentTeam.TeamId },
            { "player_id", _localPlayerId }
        };
        NetworkClient.Instance.SendJson(data);

        _currentTeam = null;
        _isInTeam = false; 

        OnTeamLeft?.Invoke();
    }

    /// <summary>
    /// 踢出队员（仅房主）
    /// </summary>
    public void KickMember(int playerId)
    {
        if (!IsHost) return;

        var data = new Dictionary<string, object>
        {
            { "type", "kick_member" },
            { "team_id", _currentTeam.TeamId },
            { "target_id", playerId },
            { "kicker_id", _localPlayerId }
        };
        NetworkClient.Instance.SendJson(data);
    }

    /// <summary>
    /// 转移房主
    /// </summary>
    public void TransferHost(int newHostId)
    {
        if (!IsHost) return;

        var data = new Dictionary<string, object>
        {
            { "type", "transfer_host" },
            { "team_id", _currentTeam.TeamId },
            { "current_host_id", _localPlayerId },
            { "new_host_id", newHostId }
        };
        NetworkClient.Instance.SendJson(data);
    }

    /// <summary>
    /// 设置准备状态
    /// </summary>
    public void SetReady(bool ready)
    {
        if (!_isInTeam || _currentTeam == null) return;

        var member = _currentTeam.Members.Find(m => m.PlayerId == _localPlayerId);
        if (member != null)
        {
            member.IsReady = ready;

            var data = new Dictionary<string, object>
            {
                { "type", "team_ready" },
                { "team_id", _currentTeam.TeamId },
                { "player_id", _localPlayerId },
                { "ready", ready }
            };
            NetworkClient.Instance.SendJson(data);

            OnMemberReady?.Invoke(_localPlayerId, ready);
        }
    }

    /// <summary>
    /// 处理队伍创建响应
    /// </summary>
    public void HandleTeamCreated(Dictionary<string, object> data)
    {
        if (!data.ContainsKey("team_id")) return;

        _localPlayerId = MultiplayerManager.Instance.LocalPlayerId;
        
        _currentTeam = new TeamInfo(
            data["team_id"].ToString(),
            data.ContainsKey("team_name") ? data["team_name"].ToString() : "Team"
        );

        if (data.ContainsKey("members"))
        {
            var members = data["members"] as Dictionary<string, object>;
            foreach (var kvp in members)
            {
                var m = kvp.Value as Dictionary<string, object>;
                var member = new TeamMember(
                    (int)m["player_id"],
                    m["player_name"].ToString(),
                    (bool)m["is_host"]
                );
                _currentTeam.Members.Add(member);
            }
        }

        _isInTeam = true;
        OnTeamCreated?.Invoke(_currentTeam);
    }

    /// <summary>
    /// 处理加入队伍响应
    /// </summary>
    public void HandleTeamJoined(Dictionary<string, object> data)
    {
        if (!data.ContainsKey("team_id")) return;

        _currentTeam = new TeamInfo(
            data["team_id"].ToString(),
            data.ContainsKey("team_name") ? data["team_name"].ToString() : "Team"
        );

        if (data.ContainsKey("members"))
        {
            var members = data["members"] as Dictionary<string, object>;
            foreach (var kvp in members)
            {
                var m = kvp.Value as Dictionary<string, object>;
                var member = new TeamMember(
                    (int)m["player_id"],
                    m["player_name"].ToString(),
                    (bool)m["is_host"]
                );
                _currentTeam.Members.Add(member);
            }
        }

        _isInTeam = true;
        OnTeamJoined?.Invoke(_currentTeam);
    }

    /// <summary>
    /// 处理成员加入
    /// </summary>
    public void HandleMemberJoined(Dictionary<string, object> data)
    {
        if (_currentTeam == null) return;

        var member = new TeamMember(
            (int)data["player_id"],
            data["player_name"].ToString(),
            false
        );

        _currentTeam.Members.Add(member);
        OnMemberJoined?.Invoke(member);
    }

    /// <summary>
    /// 处理成员离开
    /// </summary>
    public void HandleMemberLeft(Dictionary<string, object> data)
    {
        if (_currentTeam == null) return;

        var playerId = (int)data["player_id"];
        _currentTeam.Members.RemoveAll(m => m.PlayerId == playerId);

        // 如果离开的是房主，转移给下一位
        if (data.ContainsKey("new_host_id"))
        {
            var newHostId = (int)data["new_host_id"];
            foreach (var m in _currentTeam.Members)
            {
                m.IsHost = (m.PlayerId == newHostId);
            }
            OnHostChanged?.Invoke(newHostId);
        }

        OnMemberLeft?.Invoke(playerId);
    }

    /// <summary>
    /// 处理成员被踢
    /// </summary>
    public void HandleMemberKicked(Dictionary<string, object> data)
    {
        var playerId = (int)data["player_id"];

        if (playerId == _localPlayerId)
        {
            // 被踢的是本地玩家
            _currentTeam = null;
            _isInTeam = false; 
            OnTeamLeft?.Invoke();
        }
        else if (_currentTeam != null)
        {
            _currentTeam.Members.RemoveAll(m => m.PlayerId == playerId);
            OnMemberKicked?.Invoke(playerId);
        }
    }

    /// <summary>
    /// 获取队伍成员数量
    /// </summary>
    public int MemberCount => _currentTeam?.Members.Count ?? 0;

    /// <summary>
    /// 是否所有成员都准备好
    /// </summary>
    public bool AllMembersReady
    {
        get
        {
            if (_currentTeam == null || _currentTeam.Members.Count == 0) return false;
            foreach (var m in _currentTeam.Members)
            {
                if (!m.IsReady) return false;
            }
            return true;
        }
    }

    /// <summary>
    /// 获取成员统计
    /// </summary>
    public float GetTotalDamage() 
    {
        if (_currentTeam == null) return 0;
        float total = 0;
        foreach (var m in _currentTeam.Members)
        {
            total += m.DamageDealt;
        }
        return total;
    }

    /// <summary>
    /// 获取成员治疗
    /// </summary>
    public float GetTotalHealing()
    {
        if (_currentTeam == null) return 0;
        float total = 0;
        foreach (var m in _currentTeam.Members)
        {
            total += m.HealingDone;
        }
        return total;
    }
    
    // ===== 持久化 =====
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // 保存队伍状态
        data["is_in_team"] = _isInTeam;
        data["local_player_id"] = _localPlayerId;
        
        // 保存队伍信息
        if (_currentTeam != null)
        {
            data["team_id"] = _currentTeam.TeamId;
            data["team_name"] = _currentTeam.TeamName;
            data["team_is_public"] = _currentTeam.IsPublic;
            data["share_range"] = _currentTeam.ShareRange;
            data["share_loot"] = _currentTeam.ShareLoot;
            data["share_exp"] = _currentTeam.ShareExp;
            
            // 保存成员信息
            var membersData = new Array();
            foreach (var member in _currentTeam.Members)
            {
                var memberData = new Dictionary<string, object>();
                memberData["player_id"] = member.PlayerId;
                memberData["player_name"] = member.PlayerName;
                memberData["is_ready"] = member.IsReady;
                memberData["is_host"] = member.IsHost;
                memberData["position_x"] = member.Position.x;
                memberData["position_y"] = member.Position.y;
                memberData["health"] = member.Health;
                memberData["max_health"] = member.MaxHealth;
                memberData["damage_dealt"] = member.DamageDealt;
                memberData["healing_done"] = member.HealingDone;
                memberData["enemies_killed"] = member.EnemiesKilled;
                membersData.Add(memberData);
            }
            data["members"] = membersData;
        }
        
        return data;
    }
    
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        // 恢复队伍状态
        if (data.ContainsKey("is_in_team"))
            _isInTeam = Convert.ToBoolean(data["is_in_team"]);
        if (data.ContainsKey("local_player_id"))
            _localPlayerId = Convert.ToInt32(data["local_player_id"]);
        
        // 恢复队伍信息
        if (data.ContainsKey("team_id") && data.ContainsKey("team_name"))
        {
            _currentTeam = new TeamInfo(
                data["team_id"].ToString(),
                data["team_name"].ToString()
            );
            
            if (data.ContainsKey("team_is_public"))
                _currentTeam.IsPublic = Convert.ToBoolean(data["team_is_public"]);
            if (data.ContainsKey("share_range"))
                _currentTeam.ShareRange = Convert.ToSingle(data["share_range"]);
            if (data.ContainsKey("share_loot"))
                _currentTeam.ShareLoot = Convert.ToBoolean(data["share_loot"]);
            if (data.ContainsKey("share_exp"))
                _currentTeam.ShareExp = Convert.ToBoolean(data["share_exp"]);
            
            // 恢复成员信息
            if (data.ContainsKey("members"))
            {
                var membersData = (Array)data["members"];
                foreach (Dictionary memberData in membersData)
                {
                    var member = new TeamMember(
                        Convert.ToInt32(memberData["player_id"]),
                        memberData["player_name"].ToString(),
                        Convert.ToBoolean(memberData["is_host"])
                    );
                    member.IsReady = Convert.ToBoolean(memberData["is_ready"]);
                    member.Position = new Vector2(
                        Convert.ToSingle(memberData["position_x"]),
                        Convert.ToSingle(memberData["position_y"])
                    );
                    member.Health = Convert.ToInt32(memberData["health"]);
                    member.MaxHealth = Convert.ToInt32(memberData["max_health"]);
                    member.DamageDealt = Convert.ToSingle(memberData["damage_dealt"]);
                    member.HealingDone = Convert.ToSingle(memberData["healing_done"]);
                    member.EnemiesKilled = Convert.ToInt32(memberData["enemies_killed"]);
                    _currentTeam.Members.Add(member);
                }
            }
        }
    }
}
