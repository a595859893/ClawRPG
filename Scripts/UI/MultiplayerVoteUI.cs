using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Modules.MultiplayerVote
{
    /// <summary>
    /// UI for multiplayer voting and party management
    /// </summary>
    public partial class MultiplayerVoteUI : Control
    {
        private static MultiplayerVoteUI _instance;
        public static MultiplayerVoteUI Instance => _instance;

        private MultiplayerVoteSystem _system;
        
        // UI Elements
        private TabContainer _tabContainer;
        private VBoxContainer _partyTab;
        private VBoxContainer _voteTab;
        private VBoxContainer _browseTab;
        private VBoxContainer _statsTab;
        
        // Party tab elements
        private Label _partyNameLabel;
        private Label _partyInfoLabel;
        private VBoxContainer _membersContainer;
        private Button _leaveButton;
        private Button _readyButton;
        private Button _settingsButton;
        private Button _inviteButton;
        
        // Vote tab elements
        private VBoxContainer _activeVotesContainer;
        private OptionButton _voteTypeSelector;
        private LineEdit _targetPlayerInput;
        private LineEdit _reasonInput;
        private Button _initiateVoteButton;
        
        // Browse tab elements
        private VBoxContainer _partyListContainer;
        private Button _refreshButton;
        private LineEdit _partyIdInput;
        private LineEdit _passwordInput;
        private Button _joinButton;
        
        // Stats tab elements
        private Label _votesCastLabel;
        private Label _votesInitiatedLabel;
        private Label _votesPassedLabel;
        private Label _partiesCreatedLabel;
        private Label _partiesJoinedLabel;
        
        private string _currentPlayerId = "player_1";  // Would come from game state
        private string _currentPlayerName = "Player";

        public override void _Ready()
        {
            _instance = this;
            _system = MultiplayerVoteSystem.Instance;
            
            SetupUI();
            RefreshAll();
        }

        #region Event Handlers

        private void OnCreatePartyPressed()
        {
            var partyName = $" {_currentPlayerName}'s Party";
            _system.CreateParty(_currentPlayerId, _currentPlayerName, partyName, true, "", "PvE", 4);
            RefreshAll();
        }

        private void OnLeavePartyPressed()
        {
            _system.LeaveParty(_currentPlayerId);
            RefreshAll();
        }

        private void OnReadyPressed()
        {
            var party = _system.GetPlayerParty(_currentPlayerId);
            if (party == null) return;

            var member = party.Members.FirstOrDefault(m => m.PlayerId == _currentPlayerId);
            if (member == null) return;

            _system.SetReady(_currentPlayerId, !member.IsReady);
            RefreshPartyTab();
        }

        private void OnInvitePressed()
        {
            var party = _system.GetPlayerParty(_currentPlayerId);
            if (party == null) return;

            GD.Print($"Invite link: clawrpg://party/{party.PartyId}");
        }

        private void OnSettingsPressed()
        {
            // Toggle party visibility
            var party = _system.GetPlayerParty(_currentPlayerId);
            if (party != null)
            {
                _system.UpdatePartySettings(_currentPlayerId, isPublic: !party.IsPublic);
                RefreshPartyTab();
            }
        }

        private void OnInitiateVotePressed()
        {
            var voteType = (VoteType)_voteTypeSelector.GetSelectedId();
            var targetId = _targetPlayerInput.Text;
            var reason = _reasonInput.Text;

            _system.InitiateVote(_currentPlayerId, voteType, targetId, "", reason);
            _targetPlayerInput.Text = "";
            _reasonInput.Text = "";
            RefreshVoteTab();
        }

        private void OnRefreshPressed()
        {
            RefreshBrowseTab();
        }

        private void OnJoinPartyPressed()
        {
            var partyId = _partyIdInput.Text;
            var password = _passwordInput.Text;

            if (!string.IsNullOrEmpty(partyId))
            {
                var joined = _system.JoinParty(_currentPlayerId, _currentPlayerName, 1, 100, partyId, password);
                if (joined)
                {
                    _partyIdInput.Text = "";
                    _passwordInput.Text = "";
                    RefreshAll();
                }
            }
        }

        #endregion

        public void SetCurrentPlayer(string playerId, string playerName)
        {
            _currentPlayerId = playerId;
            _currentPlayerName = playerName;
            RefreshAll();
        }

        public void Toggle()
        {
            Visible = !Visible;
            if (Visible)
            {
                RefreshAll();
            }
        }
    }
}
