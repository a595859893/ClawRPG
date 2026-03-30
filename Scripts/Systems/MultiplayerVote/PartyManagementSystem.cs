using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Systems.MultiplayerVote
{
    /// <summary>
    /// 队伍管理系统 - 负责队伍的创建、加入、离开和管理
    /// 继承 BaseSystem 实现数据持久化
    /// </summary>
    public partial class PartyManagementSystem : BaseSystem
    {
        private static PartyManagementSystem _instance;
        public static PartyManagementSystem Instance => _instance;
        
        // 内部数据存储
        private Dictionary<string, Party> _activeParties = new Dictionary<string, Party>();
        private Dictionary<string, PlayerPartyData> _playerPartyData = new Dictionary<string, PlayerPartyData>();
        private Dictionary<string, PartyStatistics> _playerStatistics = new Dictionary<string, PartyStatistics>();
        
        // Signals - 转发到主系统
        public delegate void PartyCreatedEventHandler(Party party);
        public delegate void PartyJoinedEventHandler(string partyId, PartyMember member);
        public delegate void PartyLeftEventHandler(string partyId, string playerId);
        public delegate void PartyMemberKickedEventHandler(string partyId, string playerId);
        public delegate void PartyLeaderChangedEventHandler(string partyId, string newLeaderId);
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
        }
        
        protected override string SystemName => "PartyManagement";

        #region Party Management
        
        /// <summary>
        /// 创建队伍
        /// </summary>
        public Party CreateParty(string leaderId, string leaderName, string partyName = "", bool isPublic = true, string password = "", string gameMode = "", int maxMembers = 4)
        {
            if (string.IsNullOrEmpty(partyName))
            {
                partyName = $"{leaderName}'s Party";
            }

            var party = new Party
            {
                PartyId = Guid.NewGuid().ToString(),
                PartyName = partyName,
                LeaderId = leaderId,
                IsPublic = isPublic,
                Password = password,
                GameMode = gameMode,
                MaxMembers = maxMembers > 0 ? maxMembers : 4,
                CreateTime = OS.GetUnixTime()
            };

            var leaderMember = new PartyMember
            {
                PlayerId = leaderId,
                PlayerName = leaderName,
                IsLeader = true,
                IsReady = true,
                Role = "Leader",
                JoinTime = OS.GetUnixTime()
            };
            party.Members.Add(leaderMember);

            _activeParties[party.PartyId] = party;

            GetOrCreatePlayerPartyData(leaderId);
            _playerPartyData[leaderId].CurrentPartyId = party.PartyId;
            _playerPartyData[leaderId].TotalPartiesCreated++;
            GetOrCreatePlayerStatistics(leaderId).PartiesCreated++;

            EmitSignal(SignalName.PartyCreated, party);
            return party;
        }

        /// <summary>
        /// 加入队伍
        /// </summary>
        public bool JoinParty(string playerId, string playerName, int level, int power, string partyId, string password = "")
        {
            if (!_activeParties.ContainsKey(partyId))
                return false;

            var party = _activeParties[partyId];

            if (party.Members.Count >= party.MaxMembers)
                return false;
            
            if (!party.IsPublic && party.Password != password)
                return false;
            
            if (level < party.MinLevel || level > party.MaxLevel)
                return false;

            if (party.Members.Any(m => m.PlayerId == playerId))
                return false;

            var member = new PartyMember
            {
                PlayerId = playerId,
                PlayerName = playerName,
                Level = level,
                Power = power,
                IsLeader = false,
                IsReady = false,
                Role = "Member",
                JoinTime = OS.GetUnixTime()
            };

            party.Members.Add(member);

            GetOrCreatePlayerPartyData(playerId);
            _playerPartyData[playerId].CurrentPartyId = partyId;
            _playerPartyData[playerId].TotalPartiesJoined++;
            GetOrCreatePlayerStatistics(playerId).PartiesJoined++;

            EmitSignal(SignalName.PartyJoined, partyId, member);
            return true;
        }

        /// <summary>
        /// 离开队伍
        /// </summary>
        public bool LeaveParty(string playerId)
        {
            var playerData = GetPlayerPartyData(playerId);
            if (playerData == null || string.IsNullOrEmpty(playerData.CurrentPartyId))
                return false;

            var partyId = playerData.CurrentPartyId;
            if (!_activeParties.ContainsKey(partyId))
                return false;

            var party = _activeParties[partyId];
            var member = party.Members.FirstOrDefault(m => m.PlayerId == playerId);
            
            if (member == null)
                return false;

            party.Members.Remove(member);

            if (member.IsLeader && party.Members.Count > 0)
            {
                var newLeader = party.Members.First();
                newLeader.IsLeader = true;
                newLeader.Role = "Leader";
                party.LeaderId = newLeader.PlayerId;
                EmitSignal(SignalName.PartyLeaderChanged, partyId, newLeader.PlayerId);
            }

            if (party.Members.Count == 0)
            {
                _activeParties.Remove(partyId);
            }

            playerData.CurrentPartyId = "";
            playerData.PastPartyIds.Add(partyId);

            EmitSignal(SignalName.PartyLeft, partyId, playerId);
            return true;
        }

        /// <summary>
        /// 踢出玩家
        /// </summary>
        public bool KickPlayer(string kickerId, string targetId)
        {
            var kickerData = GetPlayerPartyData(kickerId);
            if (kickerData == null || string.IsNullOrEmpty(kickerData.CurrentPartyId))
                return false;

            var party = _activeParties[kickerData.CurrentPartyId];
            
            if (party.LeaderId != kickerId)
                return false;

            var targetMember = party.Members.FirstOrDefault(m => m.PlayerId == targetId);
            if (targetMember == null || targetMember.IsLeader)
                return false;

            party.Members.Remove(targetMember);

            var targetData = GetPlayerPartyData(targetId);
            if (targetData != null)
            {
                targetData.CurrentPartyId = "";
                targetData.PastPartyIds.Add(party.PartyId);
            }

            GetOrCreatePlayerStatistics(targetId).TimesKicked++;
            GetOrCreatePlayerStatistics(kickerId).TimesKickedOthers++;

            EmitSignal(SignalName.PartyMemberKicked, party.PartyId, targetId);
            return true;
        }

        /// <summary>
        /// 设置准备状态
        /// </summary>
        public bool SetReady(string playerId, bool ready)
        {
            var playerData = GetPlayerPartyData(playerId);
            if (playerData == null || string.IsNullOrEmpty(playerData.CurrentPartyId))
                return false;

            var party = _activeParties[playerData.CurrentPartyId];
            var member = party.Members.FirstOrDefault(m => m.PlayerId == playerId);
            
            if (member == null)
                return false;

            member.IsReady = ready;
            return true;
        }

        /// <summary>
        /// 更新队伍设置
        /// </summary>
        public bool UpdatePartySettings(string playerId, bool? isPublic = null, string password = null, string gameMode = null, int? maxMembers = null, int? minLevel = null, int? maxLevel = null)
        {
            var playerData = GetPlayerPartyData(playerId);
            if (playerData == null || string.IsNullOrEmpty(playerData.CurrentPartyId))
                return false;

            var party = _activeParties[playerData.CurrentPartyId];
            
            if (party.LeaderId != playerId)
                return false;

            if (isPublic.HasValue) party.IsPublic = isPublic.Value;
            if (password != null) party.Password = password;
            if (gameMode != null) party.GameMode = gameMode;
            if (maxMembers.HasValue) party.MaxMembers = Math.Max(2, Math.Min(maxMembers.Value, 10));
            if (minLevel.HasValue) party.MinLevel = minLevel.Value;
            if (maxLevel.HasValue) party.MaxLevel = maxLevel.Value;

            return true;
        }
        
        /// <summary>
        /// 转让队长
        /// </summary>
        public bool TransferLeadership(string currentLeaderId, string newLeaderId)
        {
            var leaderData = GetPlayerPartyData(currentLeaderId);
            if (leaderData == null || string.IsNullOrEmpty(leaderData.CurrentPartyId))
                return false;

            var party = _activeParties[leaderData.CurrentPartyId];
            if (party.LeaderId != currentLeaderId)
                return false;

            var newLeader = party.Members.FirstOrDefault(m => m.PlayerId == newLeaderId);
            if (newLeader == null)
                return false;

            var oldLeader = party.Members.FirstOrDefault(m => m.IsLeader);
            if (oldLeader != null)
            {
                oldLeader.IsLeader = false;
                oldLeader.Role = "Member";
            }

            newLeader.IsLeader = true;
            newLeader.Role = "Leader";
            party.LeaderId = newLeader.PlayerId;

            EmitSignal(SignalName.PartyLeaderChanged, party.PartyId, newLeader.PlayerId);
            return true;
        }

        #endregion

        #region Query Methods

        public Party GetParty(string partyId)
        {
            return _activeParties.ContainsKey(partyId) ? _activeParties[partyId] : null;
        }

        public Party GetPlayerParty(string playerId)
        {
            var playerData = GetPlayerPartyData(playerId);
            if (playerData == null || string.IsNullOrEmpty(playerData.CurrentPartyId))
                return null;

            return _activeParties.ContainsKey(playerData.CurrentPartyId) 
                ? _activeParties[playerData.CurrentPartyId] 
                : null;
        }

        public List<Party> GetPublicParties()
        {
            return _activeParties.Values
                .Where(p => p.IsPublic && p.Members.Count < p.MaxMembers)
                .OrderByDescending(p => p.Members.Count)
                .ToList();
        }

        public PlayerPartyData GetPlayerPartyData(string playerId)
        {
            return _playerPartyData.ContainsKey(playerId) ? _playerPartyData[playerId] : null;
        }

        public PartyStatistics GetPlayerStatistics(string playerId)
        {
            return _playerStatistics.ContainsKey(playerId) ? _playerStatistics[playerId] : null;
        }
        
        public Dictionary<string, Party> GetAllParties()
        {
            return _activeParties;
        }
        
        public Dictionary<string, PlayerPartyData> GetAllPlayerData()
        {
            return _playerPartyData;
        }
        
        public Dictionary<string, PartyStatistics> GetAllStatistics()
        {
            return _playerStatistics;
        }

        private PlayerPartyData GetOrCreatePlayerPartyData(string playerId)
        {
            if (!_playerPartyData.ContainsKey(playerId))
            {
                _playerPartyData[playerId] = new PlayerPartyData { PlayerId = playerId };
            }
            return _playerPartyData[playerId];
        }

        private PartyStatistics GetOrCreatePlayerStatistics(string playerId)
        {
            if (!_playerStatistics.ContainsKey(playerId))
            {
                _playerStatistics[playerId] = new PartyStatistics();
            }
            return _playerStatistics[playerId];
        }

        #endregion

        #region Save/Load

        public override Dictionary<string, object> ExportSaveData()
        {
            var saveData = new Dictionary<string, object>();
            
            // 导出队伍
            var partiesData = new List<Dictionary>();
            foreach (var party in _activeParties.Values)
            {
                var membersData = new List<Dictionary>();
                foreach (var member in party.Members)
                {
                    membersData.Add(new Dictionary
                    {
                        { "player_id", member.PlayerId },
                        { "player_name", member.PlayerName },
                        { "level", member.Level },
                        { "power", member.Power },
                        { "is_leader", member.IsLeader },
                        { "is_ready", member.IsReady },
                        { "role", member.Role },
                        { "join_time", member.JoinTime }
                    });
                }
                
                partiesData.Add(new Dictionary
                {
                    { "party_id", party.PartyId },
                    { "party_name", party.PartyName },
                    { "leader_id", party.LeaderId },
                    { "is_public", party.IsPublic },
                    { "password", party.Password },
                    { "game_mode", party.GameMode },
                    { "max_members", party.MaxMembers },
                    { "min_level", party.MinLevel },
                    { "max_level", party.MaxLevel },
                    { "create_time", party.CreateTime },
                    { "members", membersData }
                });
            }
            saveData["parties"] = partiesData;
            
            // 导出玩家数据
            var playerDataList = new List<Dictionary>();
            foreach (var kvp in _playerPartyData)
            {
                playerDataList.Add(new Dictionary
                {
                    { "player_id", kvp.Key },
                    { "current_party_id", kvp.Value.CurrentPartyId },
                    { "pending_invites", new List<string>(kvp.Value.PendingInvites) },
                    { "past_party_ids", new List<string>(kvp.Value.PastPartyIds) },
                    { "total_parties_joined", kvp.Value.TotalPartiesJoined },
                    { "total_parties_created", kvp.Value.TotalPartiesCreated }
                });
            }
            saveData["player_data"] = playerDataList;
            
            return saveData;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            // 导入队伍
            if (data.Contains("parties"))
            {
                var partiesData = (Godot.Collections.Array)data["parties"];
                foreach (Dictionary partyData in partiesData)
                {
                    var party = new Party
                    {
                        PartyId = partyData["party_id"].ToString(),
                        PartyName = partyData["party_name"].ToString(),
                        LeaderId = partyData["leader_id"].ToString(),
                        IsPublic = (bool)partyData["is_public"],
                        Password = partyData.Contains("password") ? partyData["password"].ToString() : "",
                        GameMode = partyData.Contains("game_mode") ? partyData["game_mode"].ToString() : "",
                        MaxMembers = Convert.ToInt32(partyData["max_members"]),
                        MinLevel = partyData.Contains("min_level") ? Convert.ToInt32(partyData["min_level"]) : 1,
                        MaxLevel = partyData.Contains("max_level") ? Convert.ToInt32(partyData["max_level"]) : 100,
                        CreateTime = Convert.ToInt32(partyData["create_time"])
                    };
                    
                    if (partyData.Contains("members"))
                    {
                        var membersData = (Godot.Collections.Array)partyData["members"];
                        foreach (Dictionary memberData in membersData)
                        {
                            var member = new PartyMember
                            {
                                PlayerId = memberData["player_id"].ToString(),
                                PlayerName = memberData["player_name"].ToString(),
                                Level = Convert.ToInt32(memberData["level"]),
                                Power = Convert.ToInt32(memberData["power"]),
                                IsLeader = (bool)memberData["is_leader"],
                                IsReady = (bool)memberData["is_ready"],
                                Role = memberData["role"].ToString(),
                                JoinTime = Convert.ToInt32(memberData["join_time"])
                            };
                            party.Members.Add(member);
                        }
                    }
                    
                    _activeParties[party.PartyId] = party;
                }
            }
            
            // 导入玩家数据
            if (data.Contains("player_data"))
            {
                var playerDataList = (Godot.Collections.Array)data["player_data"];
                foreach (Dictionary playerData in playerDataList)
                {
                    var pData = new PlayerPartyData
                    {
                        PlayerId = playerData["player_id"].ToString(),
                        CurrentPartyId = playerData["current_party_id"].ToString(),
                        TotalPartiesJoined = Convert.ToInt32(playerData["total_parties_joined"]),
                        TotalPartiesCreated = Convert.ToInt32(playerData["total_parties_created"])
                    };
                    
                    if (playerData.Contains("pending_invites"))
                    {
                        var invites = (Godot.Collections.Array)playerData["pending_invites"];
                        foreach (string invite in invites)
                        {
                            pData.PendingInvites.Add(invite);
                        }
                    }
                    
                    if (playerData.Contains("past_party_ids"))
                    {
                        var pastIds = (Godot.Collections.Array)playerData["past_party_ids"];
                        foreach (string pastId in pastIds)
                        {
                            pData.PastPartyIds.Add(pastId);
                        }
                    }
                    
                    _playerPartyData[pData.PlayerId] = pData;
                }
            }
        }

        #endregion
    }
}
