using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ClawRPG.Modules.MultiplayerParty;

namespace ClawRPG.Modules.MultiplayerVote
{
    /// <summary>
    /// Core system for multiplayer voting
    /// </summary>
    public partial class MultiplayerVoteSystem : BaseSystem
    {
        private static MultiplayerVoteSystem _instance;
        public static MultiplayerVoteSystem Instance => _instance;

        private MultiplayerVoteData _data = new MultiplayerVoteData();
        private MultiplayerVoteDatabase _database = MultiplayerVoteDatabase.Instance;
        
        // Reference to party system
        private MultiplayerPartySystem PartySystem => MultiplayerPartySystem.Instance;
        
        // Signals for game events
        [Signal] public delegate void VoteStartedEventHandler(ActiveVote vote);
        [Signal] public delegate void VoteEndedEventHandler(ActiveVote vote, bool passed);
        [Signal] public delegate void VoteUpdatedEventHandler(ActiveVote vote);

        public override void _Ready()
        {
            _instance = this;
        }
        
        /// <summary>
        /// 系统名称
        /// </summary>
        protected override string SystemName => "MultiplayerVote";

        #region Vote System

        /// <summary>
        /// Initiate a vote
        /// </summary>
        public ActiveVote InitiateVote(string initiatorId, VoteType voteType, string targetId = "", string targetName = "", string reason = "")
        {
            var party = PartySystem.GetPlayerParty(initiatorId);
            if (party == null)
                return null;

            // Check if too many active votes
            var partyVotes = _data.ActiveVotes.Values.Where(v => v.PartyId == party.PartyId && v.Status == VoteStatus.Pending).ToList();
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
                VoteId = Guid.NewGuid().ToString(),
                PartyId = party.PartyId,
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

            // Update vote statistics
            GetOrCreateVoteStatistics(initiatorId).VotesInitiated++;

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
            var party = PartySystem.GetPlayerParty(voterId);
            if (party == null || party.PartyId != vote.PartyId)
                return false;

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

            // Update vote statistics
            GetOrCreateVoteStatistics(voterId).VotesCast++;

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
            var party = PartySystem.GetPlayerParty(cancellerId);
            if (party == null || party.PartyId != vote.PartyId)
                return false;

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
                passed = vote.YesPercentage >= config.PassThreshold;
            }

            vote.Status = passed ? VoteStatus.Passed : VoteStatus.Failed;

            // Update vote statistics
            if (vote.InitiatorId != "")
            {
                var stats = GetOrCreateVoteStatistics(vote.InitiatorId);
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
            var party = PartySystem.GetParty(vote.PartyId);
            if (party == null)
                return;

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
            var party = PartySystem.GetParty(vote.PartyId);
            if (party == null)
                return;

            switch (vote.Type)
            {
                case VoteType.KickPlayer:
                    PartySystem.KickPlayer(party.LeaderId, vote.TargetId);
                    break;
                    
                case VoteType.PromoteLeader:
                    PartySystem.PromoteLeader(party.LeaderId, vote.TargetId);
                    break;
                    
                case VoteType.StartGame:
                    // Handle start game vote
                    break;
                    
                case VoteType.Surrender:
                    // Handle surrender vote
                    break;
            }
        }

        #endregion

        #region Query Methods

        public ActiveVote GetVote(string voteId)
        {
            return _data.ActiveVotes.ContainsKey(voteId) ? _data.ActiveVotes[voteId] : null;
        }

        public List<ActiveVote> GetPartyVotes(string partyId)
        {
            return _data.ActiveVotes.Values
                .Where(v => v.Status == VoteStatus.Pending && v.PartyId == partyId)
                .ToList();
        }

        private VoteStatistics GetOrCreateVoteStatistics(string playerId)
        {
            if (!_data.VoteStatistics.ContainsKey(playerId))
            {
                _data.VoteStatistics[playerId] = new VoteStatistics();
            }
            return _data.VoteStatistics[playerId];
        }

        public VoteStatistics GetPlayerVoteStatistics(string playerId)
        {
            return _data.VoteStatistics.ContainsKey(playerId) ? _data.VoteStatistics[playerId] : null;
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

        public override Dictionary ExportSaveData()
        {
            var saveData = new Dictionary<string, object>();
            
            // Export votes
            var votesData = new List<Dictionary<string, object>>();
            foreach (var vote in _data.ActiveVotes.Values)
            {
                votesData.Add(new Dictionary<string, object>
                {
                    { "vote_id", vote.VoteId },
                    { "party_id", vote.PartyId },
                    { "type", (int)vote.Type },
                    { "initiator_id", vote.InitiatorId },
                    { "initiator_name", vote.InitiatorName },
                    { "target_id", vote.TargetId },
                    { "target_name", vote.TargetName },
                    { "reason", vote.Reason },
                    { "status", (int)vote.Status },
                    { "start_time", vote.StartTime },
                    { "end_time", vote.EndTime },
                    { "votes", vote.Votes.Select(v => new Dictionary<string, object>
                    {
                        { "player_id", v.PlayerId },
                        { "player_name", v.PlayerName },
                        { "voted_yes", v.VotedYes },
                        { "vote_time", v.VoteTime }
                    }).ToList() }
                });
            }
            saveData["votes"] = votesData;

            // Export vote statistics
            var statsData = new List<Dictionary<string, object>>();
            foreach (var stats in _data.VoteStatistics)
            {
                statsData.Add(new Dictionary<string, object>
                {
                    { "player_id", stats.Key },
                    { "votes_initiated", stats.Value.VotesInitiated },
                    { "votes_cast", stats.Value.VotesCast },
                    { "votes_passed", stats.Value.VotesPassed },
                    { "votes_failed", stats.Value.VotesFailed }
                });
            }
            saveData["vote_statistics"] = statsData;

            return saveData;
        }

        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;

            _data = new MultiplayerVoteData();

            // Import votes
            if (data.Contains("votes"))
            {
                var votesData = data["votes"] as Godot.Collections.Array;
                if (votesData != null)
                {
                    foreach (var voteObj in votesData)
                    {
                        var vd = voteObj as Godot.Collections.Dictionary;
                        if (vd == null) continue;

                        var vote = new ActiveVote
                        {
                            VoteId = vd["vote_id"].ToString(),
                            PartyId = vd["party_id"].ToString(),
                            Type = (VoteType)Convert.ToInt32(vd["type"]),
                            InitiatorId = vd["initiator_id"].ToString(),
                            InitiatorName = vd["initiator_name"].ToString(),
                            TargetId = vd["target_id"].ToString(),
                            TargetName = vd["target_name"].ToString(),
                            Reason = vd["reason"].ToString(),
                            Status = (VoteStatus)Convert.ToInt32(vd["status"]),
                            StartTime = Convert.ToInt32(vd["start_time"]),
                            EndTime = Convert.ToInt32(vd["end_time"])
                        };

                        var voteRecords = vd["votes"] as Godot.Collections.Array;
                        if (voteRecords != null)
                        {
                            foreach (var vrObj in voteRecords)
                            {
                                var vrd = vrObj as Godot.Collections.Dictionary;
                                if (vrd == null) continue;

                                vote.Votes.Add(new VoteRecord
                                {
                                    PlayerId = vrd["player_id"].ToString(),
                                    PlayerName = vrd["player_name"].ToString(),
                                    VotedYes = (bool)vrd["voted_yes"],
                                    VoteTime = Convert.ToInt32(vrd["vote_time"])
                                });
                            }
                        }

                        _data.ActiveVotes[vote.VoteId] = vote;
                    }
                }
            }

            // Import vote statistics
            if (data.Contains("vote_statistics"))
            {
                var statsDataList = data["vote_statistics"] as Godot.Collections.Array;
                if (statsDataList != null)
                {
                    foreach (var sdObj in statsDataList)
                    {
                        var sd = sdObj as Godot.Collections.Dictionary;
                        if (sd == null) continue;

                        var stats = new VoteStatistics
                        {
                            VotesInitiated = Convert.ToInt32(sd["votes_initiated"]),
                            VotesCast = Convert.ToInt32(sd["votes_cast"]),
                            VotesPassed = Convert.ToInt32(sd["votes_passed"]),
                            VotesFailed = Convert.ToInt32(sd["votes_failed"])
                        };

                        _data.VoteStatistics[sd["player_id"].ToString()] = stats;
                    }
                }
            }
        }

        #endregion
    }
}
