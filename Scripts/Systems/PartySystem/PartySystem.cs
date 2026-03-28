using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.PartySystem;

namespace ClawRPG.Scripts.Systems
{

public class PartySystem : BaseSystem
{
    private static PartySystem _instance;
    public static PartySystem Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PartySystem();
            return _instance;
        }
        private set { _instance = value; }
    }

    private Dictionary<string, PartyData.Party> _parties = new Dictionary<string, PartyData.Party>();
    private Dictionary<int, PartyData.PartyInvite> _pendingInvites = new Dictionary<int, PartyData.PartyInvite>();
    private Dictionary<int, PartyData.PlayerPartyData> _playerData = new Dictionary<int, PartyData.PlayerPartyData>();
    
    private int _nextPartyId = 1;

    // PartyOperations service for party creation/join/leave/invite logic
    private PartyOperations _partyOperations;

    // SocialRelations service for friend/blacklist/social data logic
    private SocialRelations _socialRelations;

    // PartyPersistence service for save/load logic
    private PartyPersistenceManager _partyPersistence;

    public Signal PartyCreated { get; } = new Signal();
    public Signal PartyDisbanded { get; } = new Signal();
    public Signal PlayerJoinedParty { get; } = new Signal();
    public Signal PlayerLeftParty { get; } = new Signal();
    public Signal PlayerKicked { get; } = new Signal();
    public Signal InviteSent { get; } = new Signal();
    public Signal InviteAccepted { get; } = new Signal();
    public Signal InviteDeclined { get; } = new Signal();
    public Signal LeaderChanged { get; } = new Signal();
    public Signal RoleChanged { get; } = new Signal();
    public Signal StateChanged { get; } = new Signal();
    public Signal FriendAdded { get; } = new Signal();
    public Signal FriendRemoved { get; } = new Signal();
    public Signal PlayerBlocked { get; } = new Signal();
    public Signal PlayerUnblocked { get; } = new Signal();

    public void Initialize()
    {
        // Initialize PartyOperations with data references and signals
        _partyOperations = new PartyOperations(
            ref _parties,
            ref _pendingInvites,
            ref _playerData,
            ref _nextPartyId,
            PartyCreated,
            PartyDisbanded,
            PlayerJoinedParty,
            PlayerLeftParty,
            PlayerKicked,
            InviteSent,
            InviteAccepted,
            InviteDeclined,
            LeaderChanged,
            StateChanged
        );

        // Initialize SocialRelations with data references and signals
        _socialRelations = new SocialRelations(
            ref _playerData,
            FriendAdded,
            FriendRemoved,
            PlayerBlocked,
            PlayerUnblocked
        );

        // Initialize PartyPersistence with data references
        _partyPersistence = new PartyPersistenceManager(
            ref _parties,
            ref _pendingInvites,
            ref _playerData
        );

        GD.Print("[PartySystem] Initialized");
    }

    public PartyData.Party CreateParty(int leaderId, string leaderName, PartyData.PartyType type, string partyName = "")
    {
        return _partyOperations.CreateParty(leaderId, leaderName, type, partyName);
    }

    public bool JoinParty(string partyId, int playerId, string playerName, int level, int classId)
    {
        return _partyOperations.JoinParty(partyId, playerId, playerName, level, classId);
    }

    public bool LeaveParty(int playerId)
    {
        return _partyOperations.LeaveParty(playerId);
    }

    public bool KickPlayer(int kickerId, int targetId)
    {
        return _partyOperations.KickPlayer(kickerId, targetId);
    }

    public void DisbandParty(string partyId)
    {
        _partyOperations.DisbandParty(partyId);
    }

    public bool SendInvite(int fromPlayerId, string fromPlayerName, int toPlayerId, PartyData.PartyType type)
    {
        return _partyOperations.SendInvite(fromPlayerId, fromPlayerName, toPlayerId, type);
    }

    public bool AcceptInvite(int playerId)
    {
        return _partyOperations.AcceptInvite(playerId, CreateParty);
    }

    public bool DeclineInvite(int playerId)
    {
        return _partyOperations.DeclineInvite(playerId);
    }

    // ==================== Social Relations ====================

    public bool AddFriend(int playerId, int friendId)
    {
        return _socialRelations.AddFriend(playerId, friendId);
    }

    public bool RemoveFriend(int playerId, int friendId)
    {
        return _socialRelations.RemoveFriend(playerId, friendId);
    }

    public bool IsFriend(int playerId, int otherId)
    {
        return _socialRelations.IsFriend(playerId, otherId);
    }

    public List<int> GetFriends(int playerId)
    {
        return _socialRelations.GetFriends(playerId);
    }

    public bool BlockPlayer(int playerId, int blockedId)
    {
        return _socialRelations.BlockPlayer(playerId, blockedId);
    }

    public bool UnblockPlayer(int playerId, int unblockedId)
    {
        return _socialRelations.UnblockPlayer(playerId, unblockedId);
    }

    public bool IsBlocked(int playerId, int otherId)
    {
        return _socialRelations.IsBlocked(playerId, otherId);
    }

    public List<int> GetBlacklist(int playerId)
    {
        return _socialRelations.GetBlacklist(playerId);
    }

    public int GetRelationStatus(int playerId, int otherId)
    {
        return _socialRelations.GetRelationStatus(playerId, otherId);
    }

    public int GetFriendCount(int playerId)
    {
        return _socialRelations.GetFriendCount(playerId);
    }

    public int GetBlacklistCount(int playerId)
    {
        return _socialRelations.GetBlacklistCount(playerId);
    }

    public void ClearSocialData(int playerId)
    {
        _socialRelations.ClearSocialData(playerId);
    }

    // ==================== Member Status ====================

    public bool SetMemberReady(int playerId, bool ready)
    {
        string partyId = GetPlayerPartyId(playerId);
        if (string.IsNullOrEmpty(partyId) || !_parties.ContainsKey(partyId))
            return false;

        var party = _parties[partyId];
        var member = party.Members.Find(m => m.PlayerId == playerId);
        if (member == null)
            return false;

        member.IsReady = ready;

        // Check if all members are ready
        if (party.State == PartyData.PartyState.Forming)
        {
            bool allReady = true;
            foreach (var m in party.Members)
            {
                if (!m.IsReady)
                {
                    allReady = false;
                    break;
                }
            }

            if (allReady && party.Members.Count >= 2)
            {
                party.State = PartyData.PartyState.Ready;
                StateChanged.Emit(partyId, (int)party.State);
            }
        }

        return true;
    }

    public bool SetMemberRole(int playerId, PartyData.MemberRole role)
    {
        string partyId = GetPlayerPartyId(playerId);
        if (string.IsNullOrEmpty(partyId) || !_parties.ContainsKey(partyId))
            return false;

        var party = _parties[partyId];
        var member = party.Members.Find(m => m.PlayerId == playerId);
        if (member == null)
            return false;

        member.Role = role;
        RoleChanged.Emit(partyId, playerId, (int)role);

        return true;
    }

    public bool UpdateMemberPosition(int playerId, float x, float y)
    {
        string partyId = GetPlayerPartyId(playerId);
        if (string.IsNullOrEmpty(partyId) || !_parties.ContainsKey(partyId))
            return false;

        var party = _parties[partyId];
        var member = party.Members.Find(m => m.PlayerId == playerId);
        if (member == null)
            return false;

        member.PositionX = x;
        member.PositionY = y;
        member.LastUpdate = DateTime.Now;

        return true;
    }

    public bool UpdateMemberHealth(int playerId, float healthPercent)
    {
        string partyId = GetPlayerPartyId(playerId);
        if (string.IsNullOrEmpty(partyId) || !_parties.ContainsKey(partyId))
            return false;

        var party = _parties[partyId];
        var member = party.Members.Find(m => m.PlayerId == playerId);
        if (member == null)
            return false;

        member.HealthPercent = healthPercent;
        member.IsOnline = healthPercent > 0;
        member.LastUpdate = DateTime.Now;

        return true;
    }

    public string GetPlayerPartyId(int playerId)
    {
        foreach (var kvp in _parties)
        {
            foreach (var member in kvp.Value.Members)
            {
                if (member.PlayerId == playerId)
                    return kvp.Key;
            }
        }
        return "";
    }

    public PartyData.Party GetParty(string partyId)
    {
        return _parties.ContainsKey(partyId) ? _parties[partyId] : null;
    }

    public PartyData.Party GetPlayerParty(int playerId)
    {
        string partyId = GetPlayerPartyId(playerId);
        return string.IsNullOrEmpty(partyId) ? null : GetParty(partyId);
    }

    public List<PartyData.Party> GetAvailableParties()
    {
        var available = new List<PartyData.Party>();
        foreach (var party in _parties.Values)
        {
            if (party.State == PartyData.PartyState.Forming && party.Members.Count < party.MaxMembers)
            {
                available.Add(party);
            }
        }
        return available;
    }

    public bool HasPendingInvite(int playerId)
    {
        return _pendingInvites.ContainsKey(playerId);
    }

    public PartyData.PartyInvite GetPendingInvite(int playerId)
    {
        return _pendingInvites.ContainsKey(playerId) ? _pendingInvites[playerId] : null;
    }

    public float GetExpShareBonus(int playerId)
    {
        var party = GetPlayerParty(playerId);
        return party != null ? party.ExpShareBonus : 0f;
    }

    public float GetDropRateBonus(int playerId)
    {
        var party = GetPlayerParty(playerId);
        return party != null ? party.DropRateBonus : 0f;
    }

    public float GetPartyDamageBonus(int playerId)
    {
        var party = GetPlayerParty(playerId);
        return party != null ? party.DamageBonus : 0f;
    }

    public float GetPartyDefenseBonus(int playerId)
    {
        var party = GetPlayerParty(playerId);
        return party != null ? party.DefenseBonus : 0f;
    }

    public PartyData.PlayerPartyData GetPlayerPartyData(int playerId)
    {
        EnsurePlayerData(playerId);
        return _playerData[playerId];
    }

    private void EnsurePlayerData(int playerId)
    {
        if (!_playerData.ContainsKey(playerId))
        {
            _playerData[playerId] = new PartyData.PlayerPartyData();
        }
    }

    /// <summary>
    /// 导出保存数据 - 继承自 BaseSystem
    /// </summary>
    public override Dictionary ExportSaveData() => _partyPersistence.ExportSaveData();

    /// <summary>
    /// 导入保存数据 - 继承自 BaseSystem
    /// </summary>
    public override void ImportSaveData(Dictionary data) => _partyPersistence.ImportSaveData(data);
}
}
