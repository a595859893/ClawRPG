using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{

/// <summary>
/// PartySystem 持久化服务
/// 负责所有存档数据的导入/导出逻辑
/// </summary>
public class PartyPersistenceManager
{
    private Dictionary<string, PartyData.Party> _parties;
    private Dictionary<int, PartyData.PartyInvite> _pendingInvites;
    private Dictionary<int, PartyData.PlayerPartyData> _playerData;

    public PartyPersistenceManager(
        ref Dictionary<string, PartyData.Party> parties,
        ref Dictionary<int, PartyData.PartyInvite> pendingInvites,
        ref Dictionary<int, PartyData.PlayerPartyData> playerData)
    {
        _parties = parties;
        _pendingInvites = pendingInvites;
        _playerData = playerData;
    }

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public Dictionary ExportSaveData()
    {
        var data = new Dictionary<string, object>();

        // 保存队伍数据
        var partiesList = new Godot.Collections.Array();
        foreach (var party in _parties.Values)
        {
            var partyData = new Dictionary
            {
                { "party_id", party.PartyId },
                { "party_name", party.PartyName },
                { "type", (int)party.Type },
                { "state", (int)party.State },
                { "leader_id", party.LeaderId },
                { "max_members", party.MaxMembers },
                { "exp_share_bonus", party.ExpShareBonus },
                { "drop_rate_bonus", party.DropRateBonus },
                { "damage_bonus", party.DamageBonus },
                { "defense_bonus", party.DefenseBonus },
                { "created_at", party.CreatedAt.ToString("o") }
            };

            var membersList = new Godot.Collections.Array();
            foreach (var member in party.Members)
            {
                membersList.Add(new Dictionary
                {
                    { "player_id", member.PlayerId },
                    { "player_name", member.PlayerName },
                    { "level", member.Level },
                    { "class_id", member.ClassId },
                    { "role", (int)member.Role },
                    { "is_ready", member.IsReady },
                    { "is_online", member.IsOnline },
                    { "health_percent", member.HealthPercent }
                });
            }
            partyData["members"] = membersList;
            partiesList.Add(partyData);
        }
        data["parties"] = partiesList;

        // 保存待处理的邀请
        var invitesList = new Godot.Collections.Array();
        foreach (var invite in _pendingInvites.Values)
        {
            invitesList.Add(new Dictionary
            {
                { "from_player_id", invite.FromPlayerId },
                { "from_player_name", invite.FromPlayerName },
                { "to_player_id", invite.ToPlayerId },
                { "party_type", (int)invite.PartyType },
                { "sent_at", invite.SentAt.ToString("o") }
            });
        }
        data["pending_invites"] = invitesList;

        // 保存玩家数据
        var playerDataDict = new Dictionary<string, object>();
        foreach (var kvp in _playerData)
        {
            var pd = new Dictionary
            {
                { "current_party_id", kvp.Value.CurrentPartyId },
                { "total_parties_joined", kvp.Value.TotalPartiesJoined },
                { "total_parties_won", kvp.Value.TotalPartiesWon },
                { "total_members_invited", kvp.Value.TotalPartyMembersInvited }
            };
            playerDataDict[kvp.Key.ToString()] = pd;
        }
        data["player_data"] = playerDataDict;

        return data;
    }

    /// <summary>
    /// 导入保存数据
    /// </summary>
    public void ImportSaveData(Dictionary data)
    {
        if (data == null)
            return;

        _parties.Clear();
        _playerData.Clear();
        _pendingInvites.Clear();

        if (data.ContainsKey("parties"))
        {
            var partiesList = (Godot.Collections.Array)data["parties"];
            foreach (var partyObj in partiesList)
            {
                var partyData = (Dictionary)partyObj;
                var party = new PartyData.Party
                {
                    PartyId = partyData["party_id"].ToString(),
                    PartyName = partyData["party_name"].ToString(),
                    Type = (PartyData.PartyType)(int)partyData["type"],
                    State = (PartyData.PartyState)(int)partyData["state"],
                    LeaderId = (int)partyData["leader_id"],
                    MaxMembers = (int)partyData["max_members"],
                    ExpShareBonus = (float)partyData["exp_share_bonus"],
                    DropRateBonus = (float)partyData["drop_rate_bonus"],
                    DamageBonus = (float)partyData["damage_bonus"],
                    DefenseBonus = (float)partyData["defense_bonus"],
                    CreatedAt = DateTime.Parse(partyData["created_at"].ToString())
                };

                if (partyData.ContainsKey("members"))
                {
                    var membersList = (Godot.Collections.Array)partyData["members"];
                    foreach (var memberObj in membersList)
                    {
                        var memberData = (Dictionary)memberObj;
                        party.Members.Add(new PartyData.PartyMember
                        {
                            PlayerId = (int)memberData["player_id"],
                            PlayerName = memberData["player_name"].ToString(),
                            Level = (int)memberData["level"],
                            ClassId = (int)memberData["class_id"],
                            Role = (PartyData.MemberRole)(int)memberData["role"],
                            IsReady = (bool)memberData["is_ready"],
                            IsOnline = (bool)memberData["is_online"],
                            HealthPercent = (float)memberData["health_percent"],
                            LastUpdate = DateTime.Now
                        });
                    }
                }

                _parties[party.PartyId] = party;
            }
        }

        // 加载待处理的邀请
        if (data.ContainsKey("pending_invites"))
        {
            var invitesList = (Godot.Collections.Array)data["pending_invites"];
            foreach (var inviteObj in invitesList)
            {
                var inviteData = (Dictionary)inviteObj;
                var invite = new PartyData.PartyInvite
                {
                    FromPlayerId = (int)inviteData["from_player_id"],
                    FromPlayerName = inviteData["from_player_name"].ToString(),
                    ToPlayerId = (int)inviteData["to_player_id"],
                    PartyType = (PartyData.PartyType)(int)inviteData["party_type"],
                    SentAt = DateTime.Parse(inviteData["sent_at"].ToString())
                };
                _pendingInvites[invite.ToPlayerId] = invite;
            }
        }

        if (data.ContainsKey("player_data"))
        {
            var playerDataDict = (Dictionary)data["player_data"];
            foreach (var kvp in playerDataDict)
            {
                var pd = (Dictionary)kvp.Value;
                int playerId = int.Parse(kvp.Key.ToString());
                _playerData[playerId] = new PartyData.PlayerPartyData
                {
                    CurrentPartyId = pd["current_party_id"].ToString(),
                    TotalPartiesJoined = (int)pd["total_parties_joined"],
                    TotalPartiesWon = (int)pd["total_parties_won"],
                    TotalPartyMembersInvited = (int)pd["total_members_invited"]
                };
            }
        }

        GD.Print($"[PartyPersistenceManager] Loaded {_parties.Count} parties, {_pendingInvites.Count} invites");
    }
}
}
