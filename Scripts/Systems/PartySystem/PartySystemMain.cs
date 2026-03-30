using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.Systems.PartySystem
{

public partial class PartySystem : BaseSystem
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
    private Dictionary<int, string> _playerPartyCache = new Dictionary<int, string>();

    private int _nextPartyId = 1;

    private PartyOperations _partyOperations;
    private SocialRelations _socialRelations;
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
        _partyOperations = new PartyOperations(
            ref _parties, ref _pendingInvites, ref _playerData, ref _nextPartyId,
            PartyCreated, PartyDisbanded, PlayerJoinedParty, PlayerLeftParty,
            PlayerKicked, InviteSent, InviteAccepted, InviteDeclined, LeaderChanged, StateChanged
        );

        _socialRelations = new SocialRelations(
            ref _playerData, FriendAdded, FriendRemoved, PlayerBlocked, PlayerUnblocked
        );

        _partyPersistence = new PartyPersistenceManager(
            ref _parties, ref _pendingInvites, ref _playerData
        );

        GD.Print("[PartySystem] Initialized");
    }

    /// <summary>Rebuilds O(1) playerId→partyId cache from current _parties state.</summary>
    private void RebuildPartyCache()
    {
        _playerPartyCache.Clear();
        foreach (var kvp in _parties)
            foreach (var m in kvp.Value.Members)
                _playerPartyCache[m.PlayerId] = kvp.Key;
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

    public List<int> GetFriends(int playerId) => _socialRelations.GetFriends(playerId);

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

    public bool SetMemberReady(int playerId, bool ready)
    {
        if (!TryGetMember(playerId, out var party, out var member))
            return false;

        member.IsReady = ready;
        if (party.State == PartyData.PartyState.Forming
            && party.Members.All(m => m.IsReady)
            && party.Members.Count >= 2)
        {
            party.State = PartyData.PartyState.Ready;
            StateChanged.Emit(GetPlayerPartyId(playerId), (int)party.State);
        }
        return true;
    }

    public bool SetMemberRole(int playerId, PartyData.MemberRole role)
    {
        if (!TryGetMember(playerId, out _, out var member))
            return false;

        string partyId = GetPlayerPartyId(playerId);
        member.Role = role;
        RoleChanged.Emit(partyId, playerId, (int)role);
        return true;
    }

    public bool UpdateMemberPosition(int playerId, float x, float y)
    {
        if (!TryGetMember(playerId, out _, out var member))
            return false;

        member.PositionX = x;
        member.PositionY = y;
        member.LastUpdate = DateTime.Now;
        return true;
    }

    public bool UpdateMemberHealth(int playerId, float healthPercent)
    {
        if (!TryGetMember(playerId, out _, out var member))
            return false;

        member.HealthPercent = healthPercent;
        member.IsOnline = healthPercent > 0;
        member.LastUpdate = DateTime.Now;
        return true;
    }

    /// <summary>Returns partyId for playerId using O(1) cache; rebuilds cache on miss.</summary>
    public string GetPlayerPartyId(int playerId)
    {
        if (_playerPartyCache.TryGetValue(playerId, out var cached))
            return cached;

        RebuildPartyCache();
        return _playerPartyCache.TryGetValue(playerId, out cached) ? cached : "";
    }

    /// <summary>Helper: finds player's party and member. Rebuilds cache on miss. Returns false if not found.</summary>
    private bool TryGetMember(int playerId, out PartyData.Party party, out PartyData.PartyMember member)
    {
        string partyId = GetPlayerPartyId(playerId);
        party = null;
        member = null;
        if (string.IsNullOrEmpty(partyId) || !_parties.TryGetValue(partyId, out party))
            return false;

        member = party.Members.Find(m => m.PlayerId == playerId);
        return member != null;
    }

    public PartyData.Party GetParty(string partyId)
        => _parties.TryGetValue(partyId, out var p) ? p : null;

    public PartyData.Party GetPlayerParty(int playerId)
        => _parties.TryGetValue(GetPlayerPartyId(playerId), out var p) ? p : null;

    public List<PartyData.Party> GetAvailableParties()
    {
        var result = new List<PartyData.Party>();
        foreach (var p in _parties.Values)
            if (p.State == PartyData.PartyState.Forming && p.Members.Count < p.MaxMembers)
                result.Add(p);
        return result;
    }

    public bool HasPendingInvite(int playerId)
    {
        return _pendingInvites.ContainsKey(playerId);
    }

    public PartyData.PartyInvite GetPendingInvite(int playerId)
    {
        return _pendingInvites.ContainsKey(playerId) ? _pendingInvites[playerId] : null;
    }

    public float GetExpShareBonus(int playerId)    => GetPartyBonus(playerId, p => p.ExpShareBonus);
    public float GetDropRateBonus(int playerId)   => GetPartyBonus(playerId, p => p.DropRateBonus);
    public float GetPartyDamageBonus(int playerId) => GetPartyBonus(playerId, p => p.DamageBonus);
    public float GetPartyDefenseBonus(int playerId) => GetPartyBonus(playerId, p => p.DefenseBonus);

    private float GetPartyBonus(int playerId, Func<PartyData.Party, float> selector)
    {
        var party = GetPlayerParty(playerId);
        return party != null ? selector(party) : 0f;
    }

    public PartyData.PlayerPartyData GetPlayerPartyData(int playerId)
    {
        if (!_playerData.ContainsKey(playerId))
            _playerData[playerId] = new PartyData.PlayerPartyData();
        return _playerData[playerId];
    }

    /// <summary>
    /// 导出保存数据 - 继承自 BaseSystem
    /// </summary>
    public override Dictionary ExportSaveData() => _partyPersistence.ExportSaveData();
    public override void ImportSaveData(Dictionary data) => _partyPersistence.ImportSaveData(data);
}
}
