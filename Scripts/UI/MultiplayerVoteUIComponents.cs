using Godot;

namespace ClawRPG.Modules.MultiplayerVote
{
    /// <summary>
    /// UI Components - MultiplayerVoteUI 面板组件
    /// </summary>
    public partial class MultiplayerVoteUI
    {
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
    }
}
