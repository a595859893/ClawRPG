using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Modules.MultiplayerVote
{
    /// <summary>
    /// Vote core system - handles all voting mechanics
    /// </summary>
    public partial class MultiplayerVoteCoreSystem : BaseSystem
    {
        private static MultiplayerVoteCoreSystem _instance;
        public static MultiplayerVoteCoreSystem Instance => _instance;

        private MultiplayerVoteData _data = new MultiplayerVoteData();
        private MultiplayerVoteDatabase _database = MultiplayerVoteDatabase.Instance;
        
        // Signals for vote events
        public delegate void VoteStartedEventHandler(ActiveVote vote);
        public delegate void VoteEndedEventHandler(ActiveVote vote, bool passed);
        public delegate void VoteUpdatedEventHandler(ActiveVote vote);

        public override void _Ready()
        {
            _instance = this;
        }
        
        /// <summary>
        /// System name
        /// </summary>
        protected override string SystemName => "MultiplayerVoteCore";

        /// <summary>
        /// Initialize the vote core system
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            GD.Print("[MultiplayerVoteCoreSystem] Initialized");
        }

        #region Data Access

        /// <summary>
        /// Get or create player party data (delegates to MultiplayerPartySystem)
        /// </summary>
        public PlayerPartyData GetOrCreatePlayerPartyData(string playerId)
        {
            return MultiplayerPartySystem.Instance?.GetOrCreatePlayerPartyData(playerId) 
                   ?? new PlayerPartyData { PlayerId = playerId };
        }

        /// <summary>
        /// Get player party data
        /// </summary>
        public PlayerPartyData GetPlayerPartyData(string playerId)
        {
            return MultiplayerPartySystem.Instance?.GetPlayerPartyData(playerId);
        }

        /// <summary>
        /// Get or create player statistics
        /// </summary>
        public PartyStatistics GetOrCreatePlayerStatistics(string playerId)
        {
            return MultiplayerPartySystem.Instance?.GetOrCreatePlayerStatistics(playerId)
                   ?? new PartyStatistics { PlayerId = playerId };
        }

        /// <summary>
        /// Get active vote by ID
        /// </summary>
        public ActiveVote GetVote(string voteId)
        {
            return _data.ActiveVotes.GetValueOrDefault(voteId);
        }

        /// <summary>
        /// Get all active votes
        /// </summary>
        public Dictionary<string, ActiveVote> GetAllVotes()
        {
            return _data.ActiveVotes;
        }

        /// <summary>
        /// Get party votes
        /// </summary>
        public List<ActiveVote> GetPartyVotes(string partyId)
        {
            return _data.ActiveVotes.Values
                .Where(v => GetPartyIdByVote(v.VoteId) == partyId)
                .ToList();
        }

        /// <summary>
        /// Get pending votes for player
        /// </summary>
        public List<ActiveVote> GetPlayerPendingVotes(string playerId)
        {
            var playerData = GetPlayerPartyData(playerId);
            if (playerData == null || string.IsNullOrEmpty(playerData.CurrentPartyId))
                return new List<ActiveVote>();

            var partyId = playerData.CurrentPartyId;
            return _data.ActiveVotes.Values
                .Where(v => GetPartyIdByVote(v.VoteId) == partyId && v.Status == VoteStatus.Pending)
                .ToList();
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
            var party = MultiplayerPartySystem.Instance?.GetParty(partyId);
            if (party == null)
                return null;

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

            var party = MultiplayerPartySystem.Instance?.GetParty(partyId);
            if (party == null)
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

            var party = MultiplayerPartySystem.Instance?.GetParty(voterData.CurrentPartyId);
            if (party == null)
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
        public void EndVote(string voteId)
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
            if (partyId == null)
                return;

            var party = MultiplayerPartySystem.Instance?.GetParty(partyId);
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
            if (config != null && config.PassThreshold == 1.0f)
            {
                int remaining = party.Members.Count - vote.Votes.Count;
                if (vote.NoCount > 0 || remaining > 0)
                {
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
            if (partyId == null)
                return;

            var party = MultiplayerPartySystem.Instance?.GetParty(partyId);
            if (party == null)
                return;

            switch (vote.Type)
            {
                case VoteType.KickPlayer:
                    MultiplayerPartySystem.Instance?.KickPlayer(party.LeaderId, vote.TargetId);
                    break;
                    
                case VoteType.PromoteLeader:
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
                        // Emit signal through party system
                    }
                    break;
            }
        }

        /// <summary>
        /// Get party ID by vote ID
        /// </summary>
        public string GetPartyIdByVote(string voteId)
        {
            // We need to track which vote belongs to which party
            // For now, check all parties
            if (MultiplayerPartySystem.Instance != null)
            {
                foreach (var party in MultiplayerPartySystem.Instance.GetAllParties().Values)
                {
                    var votes = _data.ActiveVotes.Values.Where(v => v.VoteId == voteId).ToList();
                    if (votes.Any())
                        return party.PartyId;
                }
            }
            return null;
        }

        /// <summary>
        /// Update vote timer (called from process)
        /// </summary>
        public void UpdateVotes()
        {
            var currentTime = OS.GetUnixTime();
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

        /// <summary>
        /// Export save data
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // Export active votes
            var votes = new List<Dictionary>();
            foreach (var vote in _data.ActiveVotes.Values)
            {
                var voteData = new Dictionary
                {
                    ["VoteId"] = vote.VoteId,
                    ["Type"] = (int)vote.Type,
                    ["InitiatorId"] = vote.InitiatorId,
                    ["InitiatorName"] = vote.InitiatorName,
                    ["TargetId"] = vote.TargetId,
                    ["TargetName"] = vote.TargetName,
                    ["Reason"] = vote.Reason,
                    ["Status"] = (int)vote.Status,
                    ["StartTime"] = vote.StartTime,
                    ["EndTime"] = vote.EndTime
                };

                var voteRecords = new List<Dictionary>();
                foreach (var record in vote.Votes)
                {
                    voteRecords.Add(new Dictionary
                    {
                        ["PlayerId"] = record.PlayerId,
                        ["PlayerName"] = record.PlayerName,
                        ["VotedYes"] = record.VotedYes,
                        ["VoteTime"] = record.VoteTime
                    });
                }
                voteData["Votes"] = voteRecords;
                votes.Add(voteData);
            }
            data["ActiveVotes"] = votes;

            return data;
        }

        /// <summary>
        /// Import save data
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            _data = new MultiplayerVoteData();

            // Import active votes
            if (data.Contains("ActiveVotes") && data["ActiveVotes"] is List<object> votes)
            {
                foreach (Dictionary voteDict in votes)
                {
                    var vote = new ActiveVote
                    {
                        VoteId = voteDict.GetValueOrDefault("VoteId", "").ToString(),
                        Type = (VoteType)Convert.ToInt32(voteDict.GetValueOrDefault("Type", 0)),
                        InitiatorId = voteDict.GetValueOrDefault("InitiatorId", "").ToString(),
                        InitiatorName = voteDict.GetValueOrDefault("InitiatorName", "").ToString(),
                        TargetId = voteDict.GetValueOrDefault("TargetId", "").ToString(),
                        TargetName = voteDict.GetValueOrDefault("TargetName", "").ToString(),
                        Reason = voteDict.GetValueOrDefault("Reason", "").ToString(),
                        Status = (VoteStatus)Convert.ToInt32(voteDict.GetValueOrDefault("Status", 0)),
                        StartTime = Convert.ToInt32(voteDict.GetValueOrDefault("StartTime", 0)),
                        EndTime = Convert.ToInt32(voteDict.GetValueOrDefault("EndTime", 0))
                    };

                    if (voteDict.Contains("Votes") && voteDict["Votes"] is List<object> voteRecords)
                    {
                        foreach (Dictionary recordDict in voteRecords)
                        {
                            vote.Votes.Add(new VoteRecord
                            {
                                PlayerId = recordDict.GetValueOrDefault("PlayerId", "").ToString(),
                                PlayerName = recordDict.GetValueOrDefault("PlayerName", "").ToString(),
                                VotedYes = Convert.ToBoolean(recordDict.GetValueOrDefault("VotedYes", false)),
                                VoteTime = Convert.ToInt32(recordDict.GetValueOrDefault("VoteTime", 0))
                            });
                        }
                    }

                    _data.ActiveVotes[vote.VoteId] = vote;
                }
            }

            GD.Print($"[MultiplayerVoteCoreSystem] Loaded {_data.ActiveVotes.Count} votes");
        }

        #endregion
    }
}
