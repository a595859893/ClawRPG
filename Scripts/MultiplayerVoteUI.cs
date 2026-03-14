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

        private void SetupUI()
        {
            // Main container
            var mainPanel = new PanelContainer
            {
                AnchorRight = 0.4f,
                AnchorBottom = 0.8f,
                OffsetLeft = 20,
                OffsetTop = 20,
                OffsetRight = -20,
                OffsetBottom = -20
            };
            AddChild(mainPanel);

            var mainMargin = new MarginContainer
            {
                MouseFilter = MouseFilterEnum.Stop
            };
            mainMargin.AddThemeConstantOverride("margin_left", 10);
            mainMargin.AddThemeConstantOverride("margin_right", 10);
            mainMargin.AddThemeConstantOverride("margin_top", 10);
            mainMargin.AddThemeConstantOverride("margin_bottom", 10);
            mainPanel.AddChild(mainMargin);

            // Title
            var titleLabel = new Label
            {
                Text = "🎮 Multiplayer Party & Vote",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                CustomMinimumSize = new Vector2(0, 40)
            };
            titleLabel.AddThemeFontSizeOverride("font_size", 20);
            mainMargin.AddChild(titleLabel);

            // Tab container
            _tabContainer = new TabContainer
            {
                SizeFlagsVertical = SizeFlags.ExpandFill,
                TabAlign = TabsAlign.Top
            };
            mainMargin.AddChild(_tabContainer);

            // Party tab
            _partyTab = new VBoxContainer { Name = "Party" };
            _tabContainer.AddChild(_partyTab);
            SetupPartyTab();

            // Vote tab
            _voteTab = new VBoxContainer { Name = "Vote" };
            _tabContainer.AddChild(_voteTab);
            SetupVoteTab();

            // Browse tab
            _browseTab = new VBoxContainer { Name = "Browse" };
            _tabContainer.AddChild(_browseTab);
            SetupBrowseTab();

            // Stats tab
            _statsTab = new VBoxContainer { Name = "Stats" };
            _tabContainer.AddChild(_statsTab);
            SetupStatsTab();

            // Close button
            var closeButton = new Button
            {
                Text = "✕ Close",
                CustomMinimumSize = new Vector2(0, 35),
                SizeFlagsHorizontal = SizeFlags.ShrinkEnd
            };
            closeButton.Pressed += () => Visible = false;
            mainMargin.AddChild(closeButton);
        }

        private void SetupPartyTab()
        {
            // Party info
            _partyNameLabel = new Label
            {
                Text = "No Party",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _partyNameLabel.AddThemeFontSizeOverride("font_size", 18);
            _partyTab.AddChild(_partyNameLabel);

            _partyInfoLabel = new Label
            {
                Text = "Create or join a party to play with others",
                HorizontalAlignment = HorizontalAlignment.Center,
                AutowrapMode = TextServer.WordWrap
            };
            _partyTab.AddChild(_partyInfoLabel);

            // Members list
            _membersContainer = new VBoxContainer;
            _membersContainer.AddThemeConstantOverride("separation", 5);
            _partyTab.AddChild(_membersContainer);

            // Create party button
            var createButton = new Button
            {
                Text = "📝 Create Party",
                CustomMinimumSize = new Vector2(0, 35)
            };
            createButton.Pressed += OnCreatePartyPressed;
            _partyTab.AddChild(createButton);

            // Action buttons
            var buttonContainer = new HBoxContainer;
            _partyTab.AddChild(buttonContainer);

            _readyButton = new Button
            {
                Text = "✅ Ready",
                SizeFlagsHorizontal = SizeFlags.Expand,
                CustomMinimumSize = new Vector2(0, 35)
            };
            _readyButton.Pressed += OnReadyPressed;
            buttonContainer.AddChild(_readyButton);

            _inviteButton = new Button
            {
                Text = "📤 Invite",
                SizeFlagsHorizontal = SizeFlags.Expand,
                CustomMinimumSize = new Vector2(0, 35)
            };
            _inviteButton.Pressed += OnInvitePressed;
            buttonContainer.AddChild(_inviteButton);

            var buttonContainer2 = new HBoxContainer;
            _partyTab.AddChild(buttonContainer2);

            _settingsButton = new Button
            {
                Text = "⚙️ Settings",
                SizeFlagsHorizontal = SizeFlags.Expand,
                CustomMinimumSize = new Vector2(0, 35)
            };
            _settingsButton.Pressed += OnSettingsPressed;
            buttonContainer2.AddChild(_settingsButton);

            _leaveButton = new Button
            {
                Text = "🚪 Leave Party",
                SizeFlagsHorizontal = SizeFlags.Expand,
                CustomMinimumSize = new Vector2(0, 35)
            };
            _leaveButton.Pressed += OnLeavePartyPressed;
            buttonContainer2.AddChild(_leaveButton);
        }

        private void SetupVoteTab()
        {
            // Active votes header
            var votesHeader = new Label
            {
                Text = "Active Votes",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            votesHeader.AddThemeFontSizeOverride("font_size", 16);
            _voteTab.AddChild(votesHeader);

            // Active votes list
            _activeVotesContainer = new VBoxContainer;
            _voteTab.AddChild(_activeVotesContainer);

            // Separator
            var separator = new HSeparator;
            _voteTab.AddChild(separator);

            // Initiate vote section
            var initiateHeader = new Label
            {
                Text = "Initiate Vote",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            initiateHeader.AddThemeFontSizeOverride("font_size", 16);
            _voteTab.AddChild(initiateHeader);

            // Vote type selector
            var typeLabel = new Label { Text = "Vote Type:" };
            _voteTab.AddChild(typeLabel);

            _voteTypeSelector = new OptionButton;
            PopulateVoteTypes();
            _voteTab.AddChild(_voteTypeSelector);

            // Target player input
            var targetLabel = new Label { Text = "Target Player (optional):" };
            _voteTab.AddChild(targetLabel);

            _targetPlayerInput = new LineEdit
            {
                PlaceholderText = "Player ID"
            };
            _voteTab.AddChild(_targetPlayerInput);

            // Reason input
            var reasonLabel = new Label { Text = "Reason:" };
            _voteTab.AddChild(reasonLabel);

            _reasonInput = new LineEdit
            {
                PlaceholderText = "Reason for vote"
            };
            _voteTab.AddChild(_reasonInput);

            // Initiate button
            _initiateVoteButton = new Button
            {
                Text = "🗳️ Initiate Vote",
                CustomMinimumSize = new Vector2(0, 40)
            };
            _initiateVoteButton.Pressed += OnInitiateVotePressed;
            _voteTab.AddChild(_initiateVoteButton);
        }

        private void SetupBrowseTab()
        {
            // Header
            var header = new Label
            {
                Text = "Public Parties",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            header.AddThemeFontSizeOverride("font_size", 16);
            _browseTab.AddChild(header);

            // Refresh button
            _refreshButton = new Button
            {
                Text = "🔄 Refresh",
                CustomMinimumSize = new Vector2(0, 35)
            };
            _refreshButton.Pressed += OnRefreshPressed;
            _browseTab.AddChild(_refreshButton);

            // Party list
            _partyListContainer = new VBoxContainer;
            _browseTab.AddChild(_partyListContainer);

            // Join section
            var joinHeader = new Label
            {
                Text = "Join by ID"
            };
            _browseTab.AddChild(joinHeader);

            var partyIdLabel = new Label { Text = "Party ID:" };
            _browseTab.AddChild(partyIdLabel);

            _partyIdInput = new LineEdit
            {
                PlaceholderText = "Enter Party ID"
            };
            _browseTab.AddChild(_partyIdInput);

            var passwordLabel = new Label { Text = "Password (if private):" };
            _browseTab.AddChild(passwordLabel);

            _passwordInput = new LineEdit
            {
                PlaceholderText = "Password",
                Secret = true
            };
            _browseTab.AddChild(_passwordInput);

            _joinButton = new Button
            {
                Text = "🚪 Join Party",
                CustomMinimumSize = new Vector2(0, 40)
            };
            _joinButton.Pressed += OnJoinPartyPressed;
            _browseTab.AddChild(_joinButton);
        }

        private void SetupStatsTab()
        {
            // Stats header
            var statsHeader = new Label
            {
                Text = "Your Statistics",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            statsHeader.AddThemeFontSizeOverride("font_size", 16);
            _statsTab.AddChild(statsHeader);

            // Votes cast
            _votesCastLabel = new Label { Text = "Votes Cast: 0" };
            _statsTab.AddChild(_votesCastLabel);

            // Votes initiated
            _votesInitiatedLabel = new Label { Text = "Votes Initiated: 0" };
            _statsTab.AddChild(_votesInitiatedLabel);

            // Votes passed
            _votesPassedLabel = new Label { Text = "Votes Passed: 0" };
            _statsTab.AddChild(_votesPassedLabel);

            // Parties created
            _partiesCreatedLabel = new Label { Text = "Parties Created: 0" };
            _statsTab.AddChild(_partiesCreatedLabel);

            // Parties joined
            _partiesJoinedLabel = new Label { Text = "Parties Joined: 0" };
            _statsTab.AddChild(_partiesJoinedLabel);
        }

        private void PopulateVoteTypes()
        {
            _voteTypeSelector.Clear();
            _voteTypeSelector.AddItem("Kick Player", (int)VoteType.KickPlayer);
            _voteTypeSelector.AddItem("Start Game", (int)VoteType.StartGame);
            _voteTypeSelector.AddItem("Pause Game", (int)VoteType.PauseGame);
            _voteTypeSelector.AddItem("Surrender", (int)VoteType.Surrender);
            _voteTypeSelector.AddItem("Map Vote", (int)VoteType.MapVote);
            _voteTypeSelector.AddItem("Difficulty Vote", (int)VoteType.DifficultyVote);
            _voteTypeSelector.AddItem("Ready Check", (int)VoteType.ReadyCheck);
            _voteTypeSelector.AddItem("Invite Player", (int)VoteType.InvitePlayer);
            _voteTypeSelector.AddItem("Promote Leader", (int)VoteType.PromoteLeader);
            _voteTypeSelector.AddItem("Cancel Match", (int)VoteType.CancelMatch);
        }

        private void RefreshAll()
        {
            RefreshPartyTab();
            RefreshVoteTab();
            RefreshBrowseTab();
            RefreshStatsTab();
        }

        private void RefreshPartyTab()
        {
            var party = _system.GetPlayerParty(_currentPlayerId);
            
            if (party == null)
            {
                _partyNameLabel.Text = "No Party";
                _partyInfoLabel.Text = "Create or join a party to play with others";
                _leaveButton.Disabled = true;
                _readyButton.Disabled = true;
                _settingsButton.Disabled = true;
                _inviteButton.Disabled = true;
            }
            else
            {
                _partyNameLabel.Text = $"📢 {party.PartyName}";
                _partyInfoLabel.Text = $"Leader: {party.Members.Find(m => m.IsLeader)?.PlayerName ?? "Unknown"}\n" +
                                      $"Members: {party.Members.Count}/{party.MaxMembers}\n" +
                                      $"Game Mode: {(string.IsNullOrEmpty(party.GameMode) ? "Any" : party.GameMode)}";
                _leaveButton.Disabled = false;
                _readyButton.Disabled = false;
                _settingsButton.Disabled = party.LeaderId != _currentPlayerId;
                _inviteButton.Disabled = party.LeaderId != _currentPlayerId;
            }

            // Refresh members list
            foreach (var child in _membersContainer.GetChildren())
            {
                child.QueueFree();
            }

            if (party != null)
            {
                foreach (var member in party.Members)
                {
                    var memberPanel = new PanelContainer;
                    var memberLabel = new Label
                    {
                        Text = $"{(member.IsLeader ? "👑 " : "👤 ")}{member.PlayerName} " +
                              $"(Lv.{member.Level}) {(member.IsReady ? "✅ Ready" : "⏳ Not Ready")}",
                        AutowrapMode = TextServer.WordWrap
                    };
                    memberPanel.AddChild(memberLabel);
                    _membersContainer.AddChild(memberPanel);
                }
            }
        }

        private void RefreshVoteTab()
        {
            foreach (var child in _activeVotesContainer.GetChildren())
            {
                child.QueueFree();
            }

            var playerData = _system.GetPlayerPartyData(_currentPlayerId);
            if (playerData == null || string.IsNullOrEmpty(playerData.CurrentPartyId))
            {
                var noPartyLabel = new Label
                {
                    Text = "Join a party to participate in votes",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                _activeVotesContainer.AddChild(noPartyLabel);
                _initiateVoteButton.Disabled = true;
                return;
            }

            var votes = _system.GetPartyVotes(playerData.CurrentPartyId);
            if (votes.Count == 0)
            {
                var noVotesLabel = new Label
                {
                    Text = "No active votes",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                _activeVotesContainer.AddChild(noVotesLabel);
            }
            else
            {
                foreach (var vote in votes)
                {
                    var votePanel = CreateVotePanel(vote);
                    _activeVotesContainer.AddChild(votePanel);
                }
            }

            _initiateVoteButton.Disabled = false;
        }

        private Control CreateVotePanel(ActiveVote vote)
        {
            var panel = new PanelContainer;
            var vbox = new VBoxContainer;
            panel.AddChild(vbox);

            var voteLabel = new Label
            {
                Text = $"📊 {vote.Type}\n" +
                      $"Initiated by: {vote.InitiatorName}\n" +
                      $"{(string.IsNullOrEmpty(vote.TargetName) ? "" : $"Target: {vote.TargetName}\n")}" +
                      $"{(string.IsNullOrEmpty(vote.Reason) ? "" : $"Reason: {vote.Reason}\n")}" +
                      $"Yes: {vote.YesCount} | No: {vote.NoCount} | " +
                      $"{(vote.EndTime - OS.GetUnixTime())}s remaining"
            };
            vbox.AddChild(voteLabel);

            // Add vote buttons if not yet voted
            var hasVoted = vote.Votes.Any(v => v.PlayerId == _currentPlayerId);
            if (!hasVoted)
            {
                var buttonContainer = new HBoxContainer;
                vbox.AddChild(buttonContainer);

                var yesButton = new Button
                {
                    Text = "✅ Yes",
                    SizeFlagsHorizontal = SizeFlags.Expand
                };
                yesButton.Pressed += () => _system.CastVote(_currentPlayerId, vote.VoteId, true);
                buttonContainer.AddChild(yesButton);

                var noButton = new Button
                {
                    Text = "❌ No",
                    SizeFlagsHorizontal = SizeFlags.Expand
                };
                noButton.Pressed += () => _system.CastVote(_currentPlayerId, vote.VoteId, false);
                buttonContainer.AddChild(noButton);
            }
            else
            {
                var votedLabel = new Label
                {
                    Text = "You have voted",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                vbox.AddChild(votedLabel);
            }

            return panel;
        }

        private void RefreshBrowseTab()
        {
            foreach (var child in _partyListContainer.GetChildren())
            {
                child.QueueFree();
            }

            var publicParties = _system.GetPublicParties();
            if (publicParties.Count == 0)
            {
                var noPartiesLabel = new Label
                {
                    Text = "No public parties available",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                _partyListContainer.AddChild(noPartiesLabel);
            }
            else
            {
                foreach (var party in publicParties)
                {
                    var partyPanel = new PanelContainer;
                    var partyLabel = new Label
                    {
                        Text = $"📢 {party.PartyName}\n" +
                              $"Leader: {party.Members.Find(m => m.IsLeader)?.PlayerName ?? "Unknown"}\n" +
                              $"Members: {party.Members.Count}/{party.MaxMembers}\n" +
                              $"Mode: {(string.IsNullOrEmpty(party.GameMode) ? "Any" : party.GameMode)}",
                        AutowrapMode = TextServer.WordWrap
                    };
                    partyPanel.AddChild(partyLabel);
                    _partyListContainer.AddChild(partyPanel);
                }
            }
        }

        private void RefreshStatsTab()
        {
            var stats = _system.GetPlayerStatistics(_currentPlayerId);
            
            if (stats == null)
            {
                _votesCastLabel.Text = "Votes Cast: 0";
                _votesInitiatedLabel.Text = "Votes Initiated: 0";
                _votesPassedLabel.Text = "Votes Passed: 0";
                _partiesCreatedLabel.Text = "Parties Created: 0";
                _partiesJoinedLabel.Text = "Parties Joined: 0";
                return;
            }

            _votesCastLabel.Text = $"Votes Cast: {stats.VotesCast}";
            _votesInitiatedLabel.Text = $"Votes Initiated: {stats.VotesInitiated}";
            _votesPassedLabel.Text = $"Votes Passed: {stats.VotesPassed} / {stats.VotesPassed + stats.VotesFailed}";
            _partiesCreatedLabel.Text = $"Parties Created: {stats.PartiesCreated}";
            _partiesJoinedLabel.Text = $"Parties Joined: {stats.PartiesJoined}";
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
