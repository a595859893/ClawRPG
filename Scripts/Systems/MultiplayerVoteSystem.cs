using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Modules.MultiplayerVote
{
    /// <summary>
    /// Core system for multiplayer voting and party management
    /// </summary>
    public partial class MultiplayerVoteSystem : BaseSystem
    {
        private static MultiplayerVoteSystem _instance;
        public static MultiplayerVoteSystem Instance => _instance;

        private MultiplayerVoteData _data = new MultiplayerVoteData();
        private MultiplayerVoteDatabase _database = MultiplayerVoteDatabase.Instance;
        
        // Signals for game events
        [Signal] public delegate void VoteStartedEventHandler(ActiveVote vote);
        [Signal] public delegate void VoteEndedEventHandler(ActiveVote vote, bool passed);
        [Signal] public delegate void VoteUpdatedEventHandler(ActiveVote vote);
        [Signal] public delegate void PartyCreatedEventHandler(Party party);
        [Signal] public delegate void PartyJoinedEventHandler(string partyId, PartyMember member);
        [Signal] public delegate void PartyLeftEventHandler(string partyId, string playerId);
        [Signal] public delegate void PartyMemberKickedEventHandler(string partyId, string playerId);
        [Signal] public delegate void PartyLeaderChangedEventHandler(string partyId, string newLeaderId);

        public override void _Ready()
        {
            _instance = this;
        }
        
        /// <summary>
        /// 系统名称
        /// </summary>
        protected override string SystemName => "MultiplayerVote";

        #region Party Management

        /// <summary>
        /// Create a new party
        /// </summary>
        public Party CreateParty(string leaderId, string leaderName, string partyName = "", bool isPublic = true, string password = "", string gameMode = "", int maxMembers = 4)
        {
            if (string.IsNullOrEmpty(partyName))
            {
                partyName = $"{leaderName}'s Party";
            }

            var party = new Party
            {
                PartyName = partyName,
                LeaderId = leaderId,
                IsPublic = isPublic,
                Password = password,
                GameMode = gameMode,
                MaxMembers = maxMembers > 0 ? maxMembers : _database.DefaultMaxMembers,
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

            _data.ActiveParties[party.PartyId] = party;

            // Initialize player data
            GetOrCreatePlayerPartyData(leaderId);
            _data.PlayerPartyData[leaderId].CurrentPartyId = party.PartyId;
            _data.PlayerPartyData[leaderId].TotalPartiesCreated++;
            GetOrCreatePlayerStatistics(leaderId).PartiesCreated++;

            EmitSignal(SignalName.PartyCreated, party);
            return party;
        }

        /// <summary>
        /// Join an existing party
        /// </summary>
        public bool JoinParty(string playerId, string playerName, int level, int power, string partyId, string password = "")
        {
            if (!_data.ActiveParties.ContainsKey(partyId))
                return false;

            var party = _data.ActiveParties[partyId];

            // Check restrictions
            if (party.Members.Count >= party.MaxMembers)
                return false;
            
            if (!party.IsPublic && party.Password != password)
                return false;
            
            if (level < party.MinLevel || level > party.MaxLevel)
                return false;

            // Check if already in party
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

            // Update player data
            GetOrCreatePlayerPartyData(playerId);
            _data.PlayerPartyData[playerId].CurrentPartyId = partyId;
            _data.PlayerPartyData[playerId].TotalPartiesJoined++;
            GetOrCreatePlayerStatistics(playerId).PartiesJoined++;

            EmitSignal(SignalName.PartyJoined, partyId, member);
            return true;
        }

        /// <summary>
        /// Leave current party
        /// </summary>
        public bool LeaveParty(string playerId)
        {
            var playerData = GetPlayerPartyData(playerId);
            if (playerData == null || string.IsNullOrEmpty(playerData.CurrentPartyId))
                return false;

            var partyId = playerData.CurrentPartyId;
            if (!_data.ActiveParties.ContainsKey(partyId))
                return false;

            var party = _data.ActiveParties[partyId];
            var member = party.Members.FirstOrDefault(m => m.PlayerId == playerId);
            
            if (member == null)
                return false;

            party.Members.Remove(member);

            // If leader left, promote new leader
            if (member.IsLeader && party.Members.Count > 0)
            {
                var newLeader = party.Members.First();
                newLeader.IsLeader = true;
                newLeader.Role = "Leader";
                party.LeaderId = newLeader.PlayerId;
                EmitSignal(SignalName.PartyLeaderChanged, partyId, newLeader.PlayerId);
            }

            // If party empty, remove it
            if (party.Members.Count == 0)
            {
                _data.ActiveParties.Remove(partyId);
            }

            playerData.CurrentPartyId = "";
            playerData.PastPartyIds.Add(partyId);

            EmitSignal(SignalName.PartyLeft, partyId, playerId);
            return true;
        }

        /// <summary>
        /// Kick a player from party
        /// </summary>
        public bool KickPlayer(string kickerId, string targetId)
        {
            var kickerData = GetPlayerPartyData(kickerId);
            if (kickerData == null || string.IsNullOrEmpty(kickerData.CurrentPartyId))
                return false;

            var party = _data.ActiveParties[kickerData.CurrentPartyId];
            
            // Only leader can kick
            if (party.LeaderId != kickerId)
                return false;

            var targetMember = party.Members.FirstOrDefault(m => m.PlayerId == targetId);
            if (targetMember == null || targetMember.IsLeader)
                return false;

            party.Members.Remove(targetMember);

            // Update target's party data
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
        /// Set player ready status
        /// </summary>
        public bool SetReady(string playerId, bool ready)
        {
            var playerData = GetPlayerPartyData(playerId);
            if (playerData == null || string.IsNullOrEmpty(playerData.CurrentPartyId))
                return false;

            var party = _data.ActiveParties[playerData.CurrentPartyId];
            var member = party.Members.FirstOrDefault(m => m.PlayerId == playerId);
            
            if (member == null)
                return false;

            member.IsReady = ready;
            return true;
        }

        /// <summary>
        /// Update party settings
        /// </summary>
        public bool UpdatePartySettings(string playerId, bool? isPublic = null, string password = null, string gameMode = null, int? maxMembers = null, int? minLevel = null, int? maxLevel = null)
        {
            var playerData = GetPlayerPartyData(playerId);
            if (playerData == null || string.IsNullOrEmpty(playerData.CurrentPartyId))
                return false;

            var party = _data.ActiveParties[playerData.CurrentPartyId];
            
            // Only leader can update settings
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

        #endregion

        #region Vote System

        /// <summary>
        /// Initiate a vote
        /// </summary>
        public ActiveVote InitiateVote(string initiatorId, VoteType voteType, string targetId = "", string targetName = "", string reason = "")
        {
            var initiatorData = GetPlayerPartyData(initiatorId);
            if (initiatorData == null || string.IsNullOrEmpty(initiatorData.CurrentPartyId))
                return null;

            var partyId = initiatorData.CurrentPartyId;
            if (!_data.ActiveParties.ContainsKey(partyId))
                return null;

            var party = _data.ActiveParties[partyId];

            // Check if too many active votes
            var partyVotes = _data.ActiveVotes.Values.Where(v => GetPartyIdByVote(v.VoteId) == partyId && v.Status == VoteStatus.Pending).ToList();
            if (partyVotes.Count >= _database.MaxActiveVotes)
                return null;

            var config = _database.GetVoteConfig(voteType);
            if (config == null)
                return null;

            var initiatorMember = party.Members.FirstOrDefault(m => m.PlayerId == initiatorId);
            if (initiatorMember == null)
                return null;

            int currentTime = OS.GetUnixTime();
            var vote = new ActiveVote
            {
                Type = voteType,
                InitiatorId = initiatorId,
                InitiatorName = initiatorMember.PlayerName,
                TargetId = targetId,
                TargetName = targetName,
                Reason = reason,
                StartTime = currentTime,
                EndTime = currentTime + config.DurationSeconds
            };

            // Add initiator's vote automatically
            vote.Votes.Add(new VoteRecord
            {
                PlayerId = initiatorId,
                PlayerName = initiatorMember.PlayerName,
                VotedYes = true,
                VoteTime = currentTime
            });

            _data.ActiveVotes[vote.VoteId] = vote;

            // Update statistics
            GetOrCreatePlayerStatistics(initiatorId).VotesInitiated++;

            EmitSignal(SignalName.VoteStarted, vote);
            return vote;
        }

        /// <summary>
        /// Cast a vote
        /// </summary>
        public bool CastVote(string voterId, string voteId, bool yes)
        {
            if (!_data.ActiveVotes.ContainsKey(voteId))
                return false;

            var vote = _data.ActiveVotes[voteId];
            if (vote.Status != VoteStatus.Pending)
                return false;

            // Check if vote expired
            if (OS.GetUnixTime() > vote.EndTime)
            {
                EndVote(voteId);
                return false;
            }

            // Check if voter is in the same party
            var voterData = GetPlayerPartyData(voterId);
            if (voterData == null || string.IsNullOrEmpty(voterData.CurrentPartyId))
                return false;

            var partyId = voterData.CurrentPartyId;
            if (GetPartyIdByVote(voteId) != partyId)
                return false;

            var party = _data.ActiveParties[partyId];
            var member = party.Members.FirstOrDefault(m => m.PlayerId == voterId);
            if (member == null)
                return false;

            // Check if already voted
            var existingVote = vote.Votes.FirstOrDefault(v => v.PlayerId == voterId);
            if (existingVote != null)
            {
                existingVote.VotedYes = yes;
                existingVote.VoteTime = OS.GetUnixTime();
            }
            else
            {
                vote.Votes.Add(new VoteRecord
                {
                    PlayerId = voterId,
                    PlayerName = member.PlayerName,
                    VotedYes = yes,
                    VoteTime = OS.GetUnixTime()
                });
            }

            // Update statistics
            GetOrCreatePlayerStatistics(voterId).VotesCast++;

            EmitSignal(SignalName.VoteUpdated, vote);
            
            // Check if all eligible voters have voted
            CheckVoteCompletion(voteId);
            
            return true;
        }

        /// <summary>
        /// Cancel a vote
        /// </summary>
        public bool CancelVote(string voteId, string cancellerId)
        {
            if (!_data.ActiveVotes.ContainsKey(voteId))
                return false;

            var vote = _data.ActiveVotes[voteId];
            if (vote.Status != VoteStatus.Pending)
                return false;

            // Only initiator or party leader can cancel
            var voterData = GetPlayerPartyData(cancellerId);
            if (voterData == null || string.IsNullOrEmpty(voterData.CurrentPartyId))
                return false;

            var party = _data.ActiveParties[voterData.CurrentPartyId];
            if (vote.InitiatorId != cancellerId && party.LeaderId != cancellerId)
                return false;

            vote.Status = VoteStatus.Cancelled;
            EmitSignal(SignalName.VoteEnded, vote, false);
            return true;
        }

        /// <summary>
        /// End a vote and check results
        /// </summary>
        private void EndVote(string voteId)
        {
            if (!_data.ActiveVotes.ContainsKey(voteId))
                return;

            var vote = _data.ActiveVotes[voteId];
            if (vote.Status != VoteStatus.Pending)
                return;

            var config = _database.GetVoteConfig(vote.Type);
            bool passed = false;

            // Calculate if vote passed
            if (config != null)
            {
                if (config.RequireMajority)
                {
                    passed = vote.YesPercentage >= config.PassThreshold;
                }
                else
                {
                    // Simple threshold check
                    passed = vote.YesPercentage >= config.PassThreshold;
                }
            }

            vote.Status = passed ? VoteStatus.Passed : VoteStatus.Failed;

            // Update statistics
            if (vote.InitiatorId != "")
            {
                var stats = GetOrCreatePlayerStatistics(vote.InitiatorId);
                if (passed)
                    stats.VotesPassed++;
                else
                    stats.VotesFailed++;
            }

            // Execute vote effects if passed
            if (passed)
            {
                ExecuteVoteEffects(vote);
            }

            EmitSignal(SignalName.VoteEnded, vote, passed);
        }

        /// <summary>
        /// Check if vote should complete early
        /// </summary>
        private void CheckVoteCompletion(string voteId)
        {
            if (!_data.ActiveVotes.ContainsKey(voteId))
                return;

            var vote = _data.ActiveVotes[voteId];
            var partyId = GetPartyIdByVote(voteId);
            if (partyId == null || !_data.ActiveParties.ContainsKey(partyId))
                return;

            var party = _data.ActiveParties[partyId];
            var config = _database.GetVoteConfig(vote.Type);

            // Check if everyone has voted
            if (vote.Votes.Count >= party.Members.Count)
            {
                EndVote(voteId);
                return;
            }

            // Check if vote already passed even if not all voted
            if (config != null && config.PassThreshold == 1.0f)  // 100% needed
            {
                int remaining = party.Members.Count - vote.Votes.Count;
                if (vote.NoCount > 0 || remaining > 0)
                {
                    // Won't pass without everyone
                    return;
                }
                EndVote(voteId);
            }
        }

        /// <summary>
        /// Execute effects of passed votes
        /// </summary>
        private void ExecuteVoteEffects(ActiveVote vote)
        {
            var partyId = GetPartyIdByVote(vote.VoteId);
            if (partyId == null || !_data.ActiveParties.ContainsKey(partyId))
                return;

            var party = _data.ActiveParties[partyId];

            switch (vote.Type)
            {
                case VoteType.KickPlayer:
                    KickPlayer(party.LeaderId, vote.TargetId);
                    break;
                    
                case VoteType.PromoteLeader:
                    // Promote new leader
                    var newLeader = party.Members.FirstOrDefault(m => m.PlayerId == vote.TargetId);
                    if (newLeader != null)
                    {
                        var oldLeader = party.Members.FirstOrDefault(m => m.IsLeader);
                        if (oldLeader != null)
                        {
                            oldLeader.IsLeader = false;
                            oldLeader.Role = "Member";
                        }
                        newLeader.IsLeader = true;
                        newLeader.Role = "Leader";
                        party.LeaderId = newLeader.PlayerId;
                        EmitSignal(SignalName.PartyLeaderChanged, partyId, newLeader.PlayerId);
                    }
                    break;
            }
        }

        #endregion

        #region Query Methods

        private string GetPartyIdByVote(string voteId)
        {
            // Find party by vote - we need to track this differently
            // For now, search through all parties
            foreach (var party in _data.ActiveParties.Values)
            {
                // Check if any vote in party matches
            }
            return null;
        }

        public Party GetParty(string partyId)
        {
            return _data.ActiveParties.ContainsKey(partyId) ? _data.ActiveParties[partyId] : null;
        }

        public Party GetPlayerParty(string playerId)
        {
            var playerData = GetPlayerPartyData(playerId);
            if (playerData == null || string.IsNullOrEmpty(playerData.CurrentPartyId))
                return null;

            return _data.ActiveParties.ContainsKey(playerData.CurrentPartyId) 
                ? _data.ActiveParties[playerData.CurrentPartyId] 
                : null;
        }

        public ActiveVote GetVote(string voteId)
        {
            return _data.ActiveVotes.ContainsKey(voteId) ? _data.ActiveVotes[voteId] : null;
        }

        public List<Party> GetPublicParties()
        {
            return _data.ActiveParties.Values
                .Where(p => p.IsPublic && p.Members.Count < p.MaxMembers)
                .OrderByDescending(p => p.Members.Count)
                .ToList();
        }

        public List<ActiveVote> GetPartyVotes(string partyId)
        {
            // Filter votes by checking if voter is in party
            var result = new List<ActiveVote>();
            foreach (var vote in _data.ActiveVotes.Values)
            {
                if (vote.Status == VoteStatus.Pending)
                {
                    // Check if vote belongs to this party
                    var initiatorData = GetPlayerPartyData(vote.InitiatorId);
                    if (initiatorData != null && initiatorData.CurrentPartyId == partyId)
                    {
                        result.Add(vote);
                    }
                }
            }
            return result;
        }

        public PlayerPartyData GetPlayerPartyData(string playerId)
        {
            return _data.PlayerPartyData.ContainsKey(playerId) ? _data.PlayerPartyData[playerId] : null;
        }

        public PartyStatistics GetPlayerStatistics(string playerId)
        {
            return _data.PlayerStatistics.ContainsKey(playerId) ? _data.PlayerStatistics[playerId] : null;
        }

        private PlayerPartyData GetOrCreatePlayerPartyData(string playerId)
        {
            if (!_data.PlayerPartyData.ContainsKey(playerId))
            {
                _data.PlayerPartyData[playerId] = new PlayerPartyData { PlayerId = playerId };
            }
            return _data.PlayerPartyData[playerId];
        }

        private PartyStatistics GetOrCreatePlayerStatistics(string playerId)
        {
            if (!_data.PlayerStatistics.ContainsKey(playerId))
            {
                _data.PlayerStatistics[playerId] = new PartyStatistics();
            }
            return _data.PlayerStatistics[playerId];
        }

        #endregion

        #region Update Loop

        public override void _Process(double delta)
        {
            int currentTime = OS.GetUnixTime();
            
            // Check for expired votes
            var expiredVotes = _data.ActiveVotes.Values
                .Where(v => v.Status == VoteStatus.Pending && currentTime > v.EndTime)
                .ToList();

            foreach (var vote in expiredVotes)
            {
                EndVote(vote.VoteId);
            }
        }

        #endregion

        #region Save/Load

        public Dictionary<string, object> ExportSaveData()
        {
            var saveData = new Dictionary<string, object>();
            
            // Export parties
            var partiesData = new List<Dictionary<string, object>>();
            foreach (var party in _data.ActiveParties.Values)
            {
                partiesData.Add(new Dictionary<string, object>
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
                    { "members", party.Members.Select(m => new Dictionary<string, object>
                    {
                        { "player_id", m.PlayerId },
                        { "player_name", m.PlayerName },
                        { "level", m.Level },
                        { "power", m.Power },
                        { "is_leader", m.IsLeader },
                        { "is_ready", m.IsReady },
                        { "role", m.Role },
                        { "join_time", m.JoinTime }
                    }).ToList() }
                });
            }
            saveData["parties"] = partiesData;

            // Export player data
            var playerDataList = new List<Dictionary<string, object>>();
            foreach (var pd in _data.PlayerPartyData.Values)
            {
                playerDataList.Add(new Dictionary<string, object>
                {
                    { "player_id", pd.PlayerId },
                    { "current_party_id", pd.CurrentPartyId },
                    { "pending_invites", pd.PendingInvites },
                    { "past_party_ids", pd.PastPartyIds },
                    { "total_parties_joined", pd.TotalPartiesJoined },
                    { "total_parties_created", pd.TotalPartiesCreated },
                    { "votes_cast", pd.VotesCast },
                    { "votes_initiated", pd.VotesInitiated }
                });
            }
            saveData["player_data"] = playerDataList;

            // Export statistics
            var statsData = new List<Dictionary<string, object>>();
            foreach (var stats in _data.PlayerStatistics)
            {
                statsData.Add(new Dictionary<string, object>
                {
                    { "player_id", stats.Key },
                    { "total_votes", stats.Value.TotalVotes },
                    { "votes_passed", stats.Value.VotesPassed },
                    { "votes_failed", stats.Value.VotesFailed },
                    { "parties_created", stats.Value.PartiesCreated },
                    { "parties_joined", stats.Value.PartiesJoined },
                    { "times_kicked", stats.Value.TimesKicked },
                    { "times_kicked_others", stats.Value.TimesKickedOthers }
                });
            }
            saveData["statistics"] = statsData;

            return saveData;
        }

        public void ImportSaveData(Dictionary<string, object> saveData)
        {
            if (saveData == null) return;

            _data = new MultiplayerVoteData();

            // Import parties
            if (saveData.ContainsKey("parties"))
            {
                var partiesData = saveData["parties"] as List<object>;
                if (partiesData != null)
                {
                    foreach (var partyObj in partiesData)
                    {
                        var pd = partyObj as Dictionary<string, object>;
                        if (pd == null) continue;

                        var party = new Party
                        {
                            PartyId = pd["party_id"].ToString(),
                            PartyName = pd["party_name"].ToString(),
                            LeaderId = pd["leader_id"].ToString(),
                            IsPublic = (bool)pd["is_public"],
                            Password = pd["password"].ToString(),
                            GameMode = pd["game_mode"].ToString(),
                            MaxMembers = Convert.ToInt32(pd["max_members"]),
                            MinLevel = Convert.ToInt32(pd["min_level"]),
                            MaxLevel = Convert.ToInt32(pd["max_level"]),
                            CreateTime = Convert.ToInt32(pd["create_time"])
                        };

                        var membersData = pd["members"] as List<object>;
                        if (membersData != null)
                        {
                            foreach (var memObj in membersData)
                            {
                                var md = memObj as Dictionary<string, object>;
                                if (md == null) continue;

                                party.Members.Add(new PartyMember
                                {
                                    PlayerId = md["player_id"].ToString(),
                                    PlayerName = md["player_name"].ToString(),
                                    Level = Convert.ToInt32(md["level"]),
                                    Power = Convert.ToInt32(md["power"]),
                                    IsLeader = (bool)md["is_leader"],
                                    IsReady = (bool)md["is_ready"],
                                    Role = md["role"].ToString(),
                                    JoinTime = Convert.ToInt32(md["join_time"])
                                });
                            }
                        }

                        _data.ActiveParties[party.PartyId] = party;
                    }
                }
            }

            // Import player data
            if (saveData.ContainsKey("player_data"))
            {
                var playerDataList = saveData["player_data"] as List<object>;
                if (playerDataList != null)
                {
                    foreach (var pdObj in playerDataList)
                    {
                        var pd = pdObj as Dictionary<string, object>;
                        if (pd == null) continue;

                        var playerData = new PlayerPartyData
                        {
                            PlayerId = pd["player_id"].ToString(),
                            CurrentPartyId = pd["current_party_id"].ToString()
                        };

                        var pendingInvites = pd["pending_invites"] as List<object>;
                        if (pendingInvites != null)
                            playerData.PendingInvites = pendingInvites.Select(i => i.ToString()).ToList();

                        var pastParties = pd["past_party_ids"] as List<object>;
                        if (pastParties != null)
                            playerData.PastPartyIds = pastParties.Select(i => i.ToString()).ToList();

                        playerData.TotalPartiesJoined = Convert.ToInt32(pd["total_parties_joined"]);
                        playerData.TotalPartiesCreated = Convert.ToInt32(pd["total_parties_created"]);
                        playerData.VotesCast = Convert.ToInt32(pd["votes_cast"]);
                        playerData.VotesInitiated = Convert.ToInt32(pd["votes_initiated"]);

                        _data.PlayerPartyData[playerData.PlayerId] = playerData;
                    }
                }
            }

            // Import statistics
            if (saveData.ContainsKey("statistics"))
            {
                var statsDataList = saveData["statistics"] as List<object>;
                if (statsDataList != null)
                {
                    foreach (var sdObj in statsDataList)
                    {
                        var sd = sdObj as Dictionary<string, object>;
                        if (sd == null) continue;

                        var stats = new PartyStatistics
                        {
                            TotalVotes = Convert.ToInt32(sd["total_votes"]),
                            VotesPassed = Convert.ToInt32(sd["votes_passed"]),
                            VotesFailed = Convert.ToInt32(sd["votes_failed"]),
                            PartiesCreated = Convert.ToInt32(sd["parties_created"]),
                            PartiesJoined = Convert.ToInt32(sd["parties_joined"]),
                            TimesKicked = Convert.ToInt32(sd["times_kicked"]),
                            TimesKickedOthers = Convert.ToInt32(sd["times_kicked_others"])
                        };

                        _data.PlayerStatistics[sd["player_id"].ToString()] = stats;
                    }
                }
            }
        }

        #endregion
    }
}
