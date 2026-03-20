using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.PartySystem
{

/// <summary>
/// PartySystem 队伍操作服务
/// 负责队伍创建、加入、离开、踢人、解散、邀请等核心逻辑
/// </summary>
public class PartyOperations
{
    private Dictionary<string, PartyData.Party> _parties;
    private Dictionary<int, PartyData.PartyInvite> _pendingInvites;
    private Dictionary<int, PartyData.PlayerPartyData> _playerData;
    private int _nextPartyId;

    // Signals proxied from PartySystem
    private Signal _partyCreated;
    private Signal _partyDisbanded;
    private Signal _playerJoinedParty;
    private Signal _playerLeftParty;
    private Signal _playerKicked;
    private Signal _inviteSent;
    private Signal _inviteAccepted;
    private Signal _inviteDeclined;
    private Signal _leaderChanged;
    private Signal _stateChanged;

    public PartyOperations(
        ref Dictionary<string, PartyData.Party> parties,
        ref Dictionary<int, PartyData.PartyInvite> pendingInvites,
        ref Dictionary<int, PartyData.PlayerPartyData> playerData,
        ref int nextPartyId,
        Signal partyCreated,
        Signal partyDisbanded,
        Signal playerJoinedParty,
        Signal playerLeftParty,
        Signal playerKicked,
        Signal inviteSent,
        Signal inviteAccepted,
        Signal inviteDeclined,
        Signal leaderChanged,
        Signal stateChanged)
    {
        _parties = parties;
        _pendingInvites = pendingInvites;
        _playerData = playerData;
        _nextPartyId = nextPartyId;
        _partyCreated = partyCreated;
        _partyDisbanded = partyDisbanded;
        _playerJoinedParty = playerJoinedParty;
        _playerLeftParty = playerLeftParty;
        _playerKicked = playerKicked;
        _inviteSent = inviteSent;
        _inviteAccepted = inviteAccepted;
        _inviteDeclined = inviteDeclined;
        _leaderChanged = leaderChanged;
        _stateChanged = stateChanged;
    }

    private void EnsurePlayerData(int playerId)
    {
        if (!_playerData.ContainsKey(playerId))
        {
            _playerData[playerId] = new PartyData.PlayerPartyData();
        }
    }

    private string GetPlayerPartyId(int playerId)
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

    public PartyData.Party CreateParty(int leaderId, string leaderName, PartyData.PartyType type, string partyName = "")
    {
        var party = new PartyData.Party
        {
            PartyId = "party_" + _nextPartyId++,
            PartyName = string.IsNullOrEmpty(partyName) ? $"{leaderName}的队伍" : partyName,
            Type = type,
            State = PartyData.PartyState.Forming,
            LeaderId = leaderId,
            MaxMembers = PartyDatabase.GetMaxMembers(type),
            ExpShareBonus = PartyDatabase.GetExpShareBonus(type),
            DropRateBonus = PartyDatabase.GetDropRateBonus(type),
            CreatedAt = DateTime.Now
        };

        var leader = new PartyData.PartyMember
        {
            PlayerId = leaderId,
            PlayerName = leaderName,
            Level = 1,
            ClassId = 0,
            Role = PartyData.MemberRole.Leader,
            IsReady = false,
            IsOnline = true,
            HealthPercent = 1.0f,
            LastUpdate = DateTime.Now
        };

        party.Members.Add(leader);
        _parties[party.PartyId] = party;

        EnsurePlayerData(leaderId);
        _playerData[leaderId].CurrentPartyId = party.PartyId;
        _playerData[leaderId].TotalPartiesJoined++;

        _partyCreated.Emit(party.PartyId);
        GD.Print($"[PartyOperations] Party created: {party.PartyId} by {leaderName}");

        return party;
    }

    public bool JoinParty(string partyId, int playerId, string playerName, int level, int classId)
    {
        if (!_parties.ContainsKey(partyId))
        {
            GD.Print($"[PartyOperations] Party not found: {partyId}");
            return false;
        }

        var party = _parties[partyId];

        if (party.Members.Count >= party.MaxMembers)
        {
            GD.Print($"[PartyOperations] Party is full: {partyId}");
            return false;
        }

        if (party.State != PartyData.PartyState.Forming && party.State != PartyData.PartyState.Ready)
        {
            GD.Print($"[PartyOperations] Party not joinable: {party.State}");
            return false;
        }

        var member = new PartyData.PartyMember
        {
            PlayerId = playerId,
            PlayerName = playerName,
            Level = level,
            ClassId = classId,
            Role = PartyData.MemberRole.Damage,
            IsReady = false,
            IsOnline = true,
            HealthPercent = 1.0f,
            LastUpdate = DateTime.Now
        };

        party.Members.Add(member);

        // Update party bonuses based on member count
        party.DamageBonus = PartyDatabase.GetDamageBonus(party.Type, party.Members.Count);
        party.DefenseBonus = PartyDatabase.GetDefenseBonus(party.Type, party.Members.Count);

        EnsurePlayerData(playerId);
        _playerData[playerId].CurrentPartyId = partyId;
        _playerData[playerId].TotalPartiesJoined++;

        _playerJoinedParty.Emit(partyId, playerId);
        GD.Print($"[PartyOperations] Player {playerName} joined party {partyId}");

        return true;
    }

    public bool LeaveParty(int playerId)
    {
        string partyId = GetPlayerPartyId(playerId);
        if (string.IsNullOrEmpty(partyId) || !_parties.ContainsKey(partyId))
            return false;

        var party = _parties[partyId];
        var member = party.Members.Find(m => m.PlayerId == playerId);
        if (member == null)
            return false;

        party.Members.Remove(member);

        // Record to history
        EnsurePlayerData(playerId);
        _playerData[playerId].History.Add(new PartyData.PartyRecord
        {
            PartyId = partyId,
            PartyName = party.PartyName,
            Type = party.Type,
            JoinedAt = DateTime.Now.AddHours(-1),
            LeftAt = DateTime.Now,
            WasLeader = member.Role == PartyData.MemberRole.Leader,
            WasVictory = party.State == PartyData.PartyState.InBattle
        });

        // If leader left, assign new leader or disband
        if (member.Role == PartyData.MemberRole.Leader)
        {
            if (party.Members.Count > 0)
            {
                party.LeaderId = party.Members[0].PlayerId;
                party.Members[0].Role = PartyData.MemberRole.Leader;
                _leaderChanged.Emit(partyId, party.LeaderId);
            }
            else
            {
                DisbandPartyInternal(partyId);
                return true;
            }
        }

        // Update party bonuses
        party.DamageBonus = PartyDatabase.GetDamageBonus(party.Type, party.Members.Count);
        party.DefenseBonus = PartyDatabase.GetDefenseBonus(party.Type, party.Members.Count);

        _playerData[playerId].CurrentPartyId = "";
        _playerLeftParty.Emit(partyId, playerId);
        GD.Print($"[PartyOperations] Player {playerId} left party {partyId}");

        return true;
    }

    public bool KickPlayer(int kickerId, int targetId)
    {
        string partyId = GetPlayerPartyId(kickerId);
        if (string.IsNullOrEmpty(partyId) || !_parties.ContainsKey(partyId))
            return false;

        var party = _parties[partyId];
        var kicker = party.Members.Find(m => m.PlayerId == kickerId);
        
        if (kicker == null || kicker.Role != PartyData.MemberRole.Leader)
        {
            GD.Print($"[PartyOperations] Only leader can kick players");
            return false;
        }

        var target = party.Members.Find(m => m.PlayerId == targetId);
        if (target == null)
            return false;

        if (target.Role == PartyData.MemberRole.Leader)
        {
            GD.Print($"[PartyOperations] Cannot kick leader");
            return false;
        }

        party.Members.Remove(target);

        // Record to history
        EnsurePlayerData(targetId);
        _playerData[targetId].History.Add(new PartyData.PartyRecord
        {
            PartyId = partyId,
            PartyName = party.PartyName,
            Type = party.Type,
            JoinedAt = DateTime.Now.AddHours(-1),
            LeftAt = DateTime.Now,
            WasLeader = false,
            WasVictory = party.State == PartyData.PartyState.InBattle
        });

        // Update party bonuses
        party.DamageBonus = PartyDatabase.GetDamageBonus(party.Type, party.Members.Count);
        party.DefenseBonus = PartyDatabase.GetDefenseBonus(party.Type, party.Members.Count);

        _playerData[targetId].CurrentPartyId = "";
        _playerKicked.Emit(partyId, targetId);
        GD.Print($"[PartyOperations] Player {targetId} kicked from party {partyId}");

        return true;
    }

    public void DisbandParty(string partyId)
    {
        DisbandPartyInternal(partyId);
    }

    private void DisbandPartyInternal(string partyId)
    {
        if (!_parties.ContainsKey(partyId))
            return;

        var party = _parties[partyId];

        // Record to all members' history
        foreach (var member in party.Members)
        {
            EnsurePlayerData(member.PlayerId);
            _playerData[member.PlayerId].History.Add(new PartyData.PartyRecord
            {
                PartyId = partyId,
                PartyName = party.PartyName,
                Type = party.Type,
                JoinedAt = party.CreatedAt,
                LeftAt = DateTime.Now,
                WasLeader = member.Role == PartyData.MemberRole.Leader,
                WasVictory = party.State == PartyData.PartyState.InBattle
            });

            if (member.Role == PartyData.MemberRole.Leader && member.PlayerId == party.LeaderId)
            {
                _playerData[member.PlayerId].TotalPartiesWon++;
            }

            _playerData[member.PlayerId].CurrentPartyId = "";
        }

        _parties.Remove(partyId);
        _partyDisbanded.Emit(partyId);
        GD.Print($"[PartyOperations] Party disbanded: {partyId}");
    }

    public bool SendInvite(int fromPlayerId, string fromPlayerName, int toPlayerId, PartyData.PartyType type)
    {
        if (_pendingInvites.ContainsKey(toPlayerId))
        {
            GD.Print($"[PartyOperations] Player {toPlayerId} already has pending invite");
            return false;
        }

        var invite = new PartyData.PartyInvite
        {
            FromPlayerId = fromPlayerId,
            FromPlayerName = fromPlayerName,
            ToPlayerId = toPlayerId,
            PartyType = type,
            SentAt = DateTime.Now
        };

        _pendingInvites[toPlayerId] = invite;
        EnsurePlayerData(fromPlayerId);
        _playerData[fromPlayerId].TotalPartyMembersInvited++;

        _inviteSent.Emit(fromPlayerId, toPlayerId);
        GD.Print($"[PartyOperations] Invite sent from {fromPlayerName} to {toPlayerId}");

        return true;
    }

    public bool AcceptInvite(int playerId, Func<int, string, PartyData.PartyType, PartyData.Party> createPartyFunc)
    {
        if (!_pendingInvites.ContainsKey(playerId))
            return false;

        var invite = _pendingInvites[playerId];
        _pendingInvites.Remove(playerId);

        string partyId = GetPlayerPartyId(invite.FromPlayerId);
        if (string.IsNullOrEmpty(partyId))
        {
            // Create new party if inviter doesn't have one
            var party = createPartyFunc(invite.FromPlayerId, invite.FromPlayerName, invite.PartyType);
            partyId = party.PartyId;
        }

        // Get player info (would normally come from PlayerManager)
        bool success = JoinParty(partyId, playerId, "Player_" + playerId, 1, 0);

        if (success)
        {
            _inviteAccepted.Emit(invite.FromPlayerId, playerId);
            GD.Print($"[PartyOperations] Player {playerId} accepted invite from {invite.FromPlayerId}");
        }

        return success;
    }

    public bool DeclineInvite(int playerId)
    {
        if (!_pendingInvites.ContainsKey(playerId))
            return false;

        var invite = _pendingInvites[playerId];
        _pendingInvites.Remove(playerId);

        _inviteDeclined.Emit(invite.FromPlayerId, playerId);
        GD.Print($"[PartyOperations] Player {playerId} declined invite from {invite.FromPlayerId}");

        return true;
    }

    public bool HasPendingInvite(int playerId)
    {
        return _pendingInvites.ContainsKey(playerId);
    }

    public PartyData.PartyInvite GetPendingInvite(int playerId)
    {
        return _pendingInvites.ContainsKey(playerId) ? _pendingInvites[playerId] : null;
    }
}
}
