using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ClawRPG.Modules.MultiplayerParty;

namespace ClawRPG.Modules.MultiplayerVote
{
    /// <summary>
    /// MultiplayerVote System - Main integration system
    /// Delegates to MultiplayerPartySystem and MultiplayerVoteCoreSystem
    /// </summary>
    public partial class MultiplayerVoteSystem : BaseSystem
    {
        private static MultiplayerVoteSystem _instance;
        public static MultiplayerVoteSystem Instance => _instance;

        // Reference to other systems
        private MultiplayerPartySystem _partySystem;
        private MultiplayerVoteCoreSystem _voteCoreSystem;
        
        // Signals for game events (forwarded from core)
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
            
            // Get system references
            _partySystem = MultiplayerPartySystem.Instance;
            _voteCoreSystem = MultiplayerVoteCoreSystem.Instance;
            
            // Connect signals from subsystems
            ConnectSubsystemSignals();
        }
        
        /// <summary>
        /// System name
        /// </summary>
        protected override string SystemName => "MultiplayerVote";

        /// <summary>
        /// Initialize the vote system
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            GD.Print("[MultiplayerVoteSystem] Initialized");
        }

        /// <summary>
        /// Connect signals from party and vote core systems
        /// </summary>
        private void ConnectSubsystemSignals()
        {
            if (_partySystem != null)
            {
                _partySystem.Connect(SignalName.PartyCreated, Callable.From<Party>(OnPartyCreated));
                _partySystem.Connect(SignalName.PartyJoined, Callable.From<string, PartyMember>(OnPartyJoined));
                _partySystem.Connect(SignalName.PartyLeft, Callable.From<string, string>(OnPartyLeft));
                _partySystem.Connect(SignalName.PartyMemberKicked, Callable.From<string, string>(OnPartyMemberKicked));
                _partySystem.Connect(SignalName.PartyLeaderChanged, Callable.From<string, string>(OnPartyLeaderChanged));
            }
            
            if (_voteCoreSystem != null)
            {
                _voteCoreSystem.Connect(SignalName.VoteStarted, Callable.From<ActiveVote>(OnVoteStarted));
                _voteCoreSystem.Connect(SignalName.VoteEnded, Callable.From<ActiveVote, bool>(OnVoteEnded));
                _voteCoreSystem.Connect(SignalName.VoteUpdated, Callable.From<ActiveVote>(OnVoteUpdated));
            }
        }

        #region Signal Handlers

        private void OnPartyCreated(Party party) => EmitSignal(SignalName.PartyCreated, party);
        private void OnPartyJoined(string partyId, PartyMember member) => EmitSignal(SignalName.PartyJoined, partyId, member);
        private void OnPartyLeft(string partyId, string playerId) => EmitSignal(SignalName.PartyLeft, partyId, playerId);
        private void OnPartyMemberKicked(string partyId, string playerId) => EmitSignal(SignalName.PartyMemberKicked, partyId, playerId);
        private void OnPartyLeaderChanged(string partyId, string newLeaderId) => EmitSignal(SignalName.PartyLeaderChanged, partyId, newLeaderId);
        private void OnVoteStarted(ActiveVote vote) => EmitSignal(SignalName.VoteStarted, vote);
        private void OnVoteEnded(ActiveVote vote, bool passed) => EmitSignal(SignalName.VoteEnded, vote, passed);
        private void OnVoteUpdated(ActiveVote vote) => EmitSignal(SignalName.VoteUpdated, vote);

        #endregion

        #region Party Operations (Delegate to PartySystem)

        /// <summary>
        /// Create a new party
        /// </summary>
        public Party CreateParty(string leaderId, string leaderName, string partyName = "", bool isPublic = true, string password = "", string gameMode = "", int maxMembers = 4)
        {
            return _partySystem?.CreateParty(leaderId, leaderName, partyName, isPublic, password, gameMode, maxMembers);
        }

        /// <summary>
        /// Join an existing party
        /// </summary>
        public bool JoinParty(string playerId, string playerName, int level, int power, string partyId, string password = "")
        {
            return _partySystem?.JoinParty(playerId, playerName, level, power, partyId, password) ?? false;
        }

        /// <summary>
        /// Leave current party
        /// </summary>
        public bool LeaveParty(string playerId)
        {
            return _partySystem?.LeaveParty(playerId) ?? false;
        }

        /// <summary>
        /// Kick a player from party
        /// </summary>
        public bool KickPlayer(string kickerId, string targetId)
        {
            return _partySystem?.KickPlayer(kickerId, targetId) ?? false;
        }

        /// <summary>
        /// Promote a party member to leader
        /// </summary>
        public bool PromoteLeader(string oldLeaderId, string newLeaderId)
        {
            return _partySystem?.PromoteLeader(oldLeaderId, newLeaderId) ?? false;
        }

        /// <summary>
        /// Get player's current party
        /// </summary>
        public Party GetPlayerParty(string playerId)
        {
            return _partySystem?.GetPlayerParty(playerId);
        }

        /// <summary>
        /// Get party by ID
        /// </summary>
        public Party GetParty(string partyId)
        {
            return _partySystem?.GetParty(partyId);
        }

        /// <summary>
        /// Get public parties list
        /// </summary>
        public List<Party> GetPublicParties()
        {
            return _partySystem?.GetPublicParties() ?? new List<Party>();
        }

        /// <summary>
        /// Check if player is in a party
        /// </summary>
        public bool IsPlayerInParty(string playerId)
        {
            return _partySystem?.IsPlayerInParty(playerId) ?? false;
        }

        /// <summary>
        /// Check if player is party leader
        /// </summary>
        public bool IsPlayerPartyLeader(string playerId)
        {
            return _partySystem?.IsPlayerPartyLeader(playerId) ?? false;
        }

        /// <summary>
        /// Set player ready status
        /// </summary>
        public bool SetReady(string playerId, bool ready)
        {
            return _partySystem?.SetReady(playerId, ready) ?? false;
        }

        #endregion

        #region Vote Operations (Delegate to VoteCoreSystem)

        /// <summary>
        /// Initiate a vote
        /// </summary>
        public ActiveVote InitiateVote(string initiatorId, VoteType voteType, string targetId = "", string targetName = "", string reason = "")
        {
            return _voteCoreSystem?.InitiateVote(initiatorId, voteType, targetId, targetName, reason);
        }

        /// <summary>
        /// Cast a vote
        /// </summary>
        public bool CastVote(string voterId, string voteId, bool yes)
        {
            return _voteCoreSystem?.CastVote(voterId, voteId, yes) ?? false;
        }

        /// <summary>
        /// Cancel a vote
        /// </summary>
        public bool CancelVote(string voteId, string cancellerId)
        {
            return _voteCoreSystem?.CancelVote(voteId, cancellerId) ?? false;
        }

        /// <summary>
        /// Get vote by ID
        /// </summary>
        public ActiveVote GetVote(string voteId)
        {
            return _voteCoreSystem?.GetVote(voteId);
        }

        /// <summary>
        /// Get party pending votes
        /// </summary>
        public List<ActiveVote> GetPartyVotes(string partyId)
        {
            return _voteCoreSystem?.GetPartyVotes(partyId) ?? new List<ActiveVote>();
        }

        /// <summary>
        /// Update vote timers
        /// </summary>
        public void UpdateVotes()
        {
            _voteCoreSystem?.UpdateVotes();
        }

        #endregion

        #region Update Loop

        public override void _Process(double delta)
        {
            // Update vote timers
            UpdateVotes();
        }

        #endregion

        #region Save/Load

        /// <summary>
        /// Export save data (delegates to subsystems)
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var saveData = new Dictionary();
            
            // Export party data
            if (_partySystem != null)
            {
                var partyData = _partySystem.ExportSaveData();
                foreach (var key in partyData.Keys)
                {
                    saveData[key] = partyData[key];
                }
            }
            
            // Export vote data
            if (_voteCoreSystem != null)
            {
                var voteData = _voteCoreSystem.ExportSaveData();
                foreach (var key in voteData.Keys)
                {
                    saveData[key] = voteData[key];
                }
            }
            
            return saveData;
        }

        /// <summary>
        /// Import save data (delegates to subsystems)
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;

            // Import party data
            if (_partySystem != null)
            {
                _partySystem.ImportSaveData(data);
            }
            
            // Import vote data
            if (_voteCoreSystem != null)
            {
                _voteCoreSystem.ImportSaveData(data);
            }
        }

        #endregion
    }
}
