using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{

public partial class PartyUI : Control
{
    // Godot 4 C# event Actions (migrated from Godot 3 .Connect())
    public Action OnClosePressed;
    public Action OnRefreshPressed;
    public Action OnCreatePartyPressed;
    public Action OnInvitePressed;
    public Action OnLeavePressed;
    public Action OnDisbandPressed;
    public Action OnConfirmCreatePressed;
    public Action OnCancelCreatePressed;
    public Action OnSendInvitePressed;
    public Action OnCancelInvitePressed;
    public Action<string> OnJoinPartyPressed;

    // PartySystem signals
    public Action<string> OnPartyCreated;
    public Action<string> OnPartyDisbanded;
    public Action<string, int> OnPlayerJoinedParty;
    public Action<string, int> OnPlayerLeftParty;

    private PanelContainer _mainPanel;
    private VBoxContainer _mainVBox;
    private TabContainer _tabContainer;
    private Label _titleLabel;
    private Button _closeButton;

    // Party List Tab
    private VBoxContainer _partyListTab;
    private ScrollContainer _partyListScroll;
    private VBoxContainer _partyListContainer;
    private Button _createPartyButton;
    private Button _refreshButton;

    // My Party Tab
    private VBoxContainer _myPartyTab;
    private Label _currentPartyName;
    private Label _partyTypeLabel;
    private Label _partyStateLabel;
    private Label _bonusLabel;
    private ScrollContainer _memberListScroll;
    private VBoxContainer _memberListContainer;
    private Button _leaveButton;
    private Button _disbandButton;
    private Button _inviteButton;

    // Statistics Tab
    private VBoxContainer _statsTab;
    private Label _totalPartiesLabel;
    private Label _totalWinsLabel;
    private Label _winRateLabel;
    private Label _membersInvitedLabel;
    private ScrollContainer _historyScroll;
    private VBoxContainer _historyContainer;

    // Create Party Dialog
    private WindowDialog _createDialog;
    private OptionButton _partyTypeOption;
    private LineEdit _partyNameInput;
    private Button _confirmCreateButton;
    private Button _cancelCreateButton;

    // Invite Dialog
    private WindowDialog _inviteDialog;
    private LineEdit _playerIdInput;
    private Button _sendInviteButton;
    private Button _cancelInviteButton;

    private Color _primaryColor = new Color(0.2f, 0.4f, 0.8f);
    private Color _secondaryColor = new Color(0.3f, 0.3f, 0.35f);
    private Color _successColor = new Color(0.2f, 0.8f, 0.4f);
    private Color _warningColor = new Color(0.9f, 0.7f, 0.2f);
    private Color _dangerColor = new Color(0.9f, 0.3f, 0.3f);

    public override void _Ready()
    {
        Visible = false;
        SetupUI();
        ConnectSignals();
        PartySystem.Instance.Initialize();
    }

    private void SetupUI()
    {
        // Main Panel
        _mainPanel = new PanelContainer();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(600, 500);
        AddChild(_mainPanel);

        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        style.BorderColor = _primaryColor;
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(8);
        _mainPanel.AddThemeStyleboxOverride("panel", style);

        _mainVBox = new VBoxContainer();
        _mainVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _mainVBox.AddThemeConstantOverride("separation", 10);
        _mainPanel.AddChild(_mainVBox);

        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "组队系统";
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _titleLabel.AddThemeColorOverride("font_color", _primaryColor);
        _mainVBox.AddChild(_titleLabel);

        // Close Button
        _closeButton = new Button();
        _closeButton.Text = "×";
        _closeButton.CustomMinimumSize = new Vector2(30, 30);
        _closeButton.Pressed += OnClosePressed;
        _mainVBox.AddChild(_closeButton);

        // Tab Container
        _tabContainer = new TabContainer();
        _tabContainer.SetSizeFlagsHorizontal(Control.SizeFlags.ExpandFill);
        _tabContainer.SetSizeFlagsVertical(Control.SizeFlags.ExpandFill);
        _mainVBox.AddChild(_tabContainer);

        SetupPartyListTab();
        SetupMyPartyTab();
        SetupStatsTab();
        SetupCreateDialog();
        SetupInviteDialog();
    }

    private void SetupPartyListTab()
    {
        _partyListTab = new VBoxContainer();
        _partyListTab.Name = "队伍列表";
        _tabContainer.AddChild(_partyListTab);

        var header = new HBoxContainer();
        _partyListTab.AddChild(header);

        _refreshButton = new Button();
        _refreshButton.Text = "刷新";
        _refreshButton.Pressed += OnRefreshPressed;
        header.AddChild(_refreshButton);

        _createPartyButton = new Button();
        _createPartyButton.Text = "创建队伍";
        _createPartyButton.Pressed += OnCreatePartyPressed;
        header.AddChild(_createPartyButton);

        _partyListScroll = new ScrollContainer();
        _partyListScroll.SetSizeFlagsVertical(Control.SizeFlags.ExpandFill);
        _partyListTab.AddChild(_partyListScroll);

        _partyListContainer = new VBoxContainer();
        _partyListContainer.AddThemeConstantOverride("separation", 5);
        _partyListScroll.AddChild(_partyListContainer);
    }

    private void SetupMyPartyTab()
    {
        _myPartyTab = new VBoxContainer();
        _myPartyTab.Name = "我的队伍";
        _tabContainer.AddChild(_myPartyTab);

        var infoPanel = new VBoxContainer();
        infoPanel.AddThemeConstantOverride("separation", 5);
        _myPartyTab.AddChild(infoPanel);

        _currentPartyName = new Label();
        _currentPartyName.Text = "队伍名称: -";
        _currentPartyName.AddThemeFontSizeOverride("font_size", 18);
        infoPanel.AddChild(_currentPartyName);

        _partyTypeLabel = new Label();
        _partyTypeLabel.Text = "队伍类型: -";
        infoPanel.AddChild(_partyTypeLabel);

        _partyStateLabel = new Label();
        _partyStateLabel.Text = "状态: -";
        infoPanel.AddChild(_partyStateLabel);

        _bonusLabel = new Label();
        _bonusLabel.Text = "队伍加成:\n经验 +0%\n掉落 +0%\n伤害 +0%\n防御 +0%";
        _bonusLabel.AddThemeColorOverride("font_color", _successColor);
        infoPanel.AddChild(_bonusLabel);

        var memberLabel = new Label();
        memberLabel.Text = "成员列表:";
        memberLabel.AddThemeFontSizeOverride("font_size", 16);
        _myPartyTab.AddChild(memberLabel);

        _memberListScroll = new ScrollContainer();
        _memberListScroll.SetSizeFlagsVertical(Control.SizeFlags.ExpandFill);
        _myPartyTab.AddChild(_memberListScroll);

        _memberListContainer = new VBoxContainer();
        _memberListContainer.AddThemeConstantOverride("separation", 5);
        _memberListScroll.AddChild(_memberListContainer);

        var buttonPanel = new HBoxContainer();
        buttonPanel.AddThemeConstantOverride("separation", 10);
        _myPartyTab.AddChild(buttonPanel);

        _inviteButton = new Button();
        _inviteButton.Text = "邀请玩家";
        _inviteButton.Pressed += OnInvitePressed;
        buttonPanel.AddChild(_inviteButton);

        _leaveButton = new Button();
        _leaveButton.Text = "离开队伍";
        _leaveButton.Pressed += OnLeavePressed;
        buttonPanel.AddChild(_leaveButton);

        _disbandButton = new Button();
        _disbandButton.Text = "解散队伍";
        _disbandButton.Pressed += OnDisbandPressed;
        _disbandButton.Modulate = _dangerColor;
        buttonPanel.AddChild(_disbandButton);
    }

    private void SetupStatsTab()
    {
        _statsTab = new VBoxContainer();
        _statsTab.Name = "统计";
        _tabContainer.AddChild(_statsTab);

        var statsPanel = new VBoxContainer();
        statsPanel.AddThemeConstantOverride("separation", 8);
        _statsTab.AddChild(statsPanel);

        _totalPartiesLabel = new Label();
        _totalPartiesLabel.Text = "参加队伍次数: 0";
        statsPanel.AddChild(_totalPartiesLabel);

        _totalWinsLabel = new Label();
        _totalWinsLabel.Text = "胜利次数: 0";
        _totalWinsLabel.AddThemeColorOverride("font_color", _successColor);
        statsPanel.AddChild(_totalWinsLabel);

        _winRateLabel = new Label();
        _winRateLabel.Text = "胜率: 0%";
        statsPanel.AddChild(_winRateLabel);

        _membersInvitedLabel = new Label();
        _membersInvitedLabel.Text = "邀请成员数: 0";
        statsPanel.AddChild(_membersInvitedLabel);

        var historyLabel = new Label();
        historyLabel.Text = "历史记录:";
        historyLabel.AddThemeFontSizeOverride("font_size", 16);
        _statsTab.AddChild(historyLabel);

        _historyScroll = new ScrollContainer();
        _historyScroll.SetSizeFlagsVertical(Control.SizeFlags.ExpandFill);
        _statsTab.AddChild(_historyScroll);

        _historyContainer = new VBoxContainer();
        _historyContainer.AddThemeConstantOverride("separation", 3);
        _historyScroll.AddChild(_historyContainer);
    }

    private void SetupCreateDialog()
    {
        _createDialog = new WindowDialog();
        _createDialog.Title = "创建队伍";
        _createDialog.CustomMinimumSize = new Vector2(300, 200);
        AddChild(_createDialog);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 10);
        _createDialog.AddChild(vbox);

        var typeLabel = new Label();
        typeLabel.Text = "队伍类型:";
        vbox.AddChild(typeLabel);

        _partyTypeOption = new OptionButton();
        _partyTypeOption.AddItem("单人", 0);
        _partyTypeOption.AddItem("双人", 1);
        _partyTypeOption.AddItem("小队 (4人)", 2);
        _partyTypeOption.AddItem("团队 (8人)", 3);
        vbox.AddChild(_partyTypeOption);

        var nameLabel = new Label();
        nameLabel.Text = "队伍名称 (可选):";
        vbox.AddChild(nameLabel);

        _partyNameInput = new LineEdit();
        _partyNameInput.Placeholder = "默认: 玩家名+的队伍";
        vbox.AddChild(_partyNameInput);

        var buttonPanel = new HBoxContainer();
        buttonPanel.AddThemeConstantOverride("separation", 10);
        vbox.AddChild(buttonPanel);

        _confirmCreateButton = new Button();
        _confirmCreateButton.Text = "创建";
        _confirmCreateButton.Pressed += OnConfirmCreatePressed;
        buttonPanel.AddChild(_confirmCreateButton);

        _cancelCreateButton = new Button();
        _cancelCreateButton.Text = "取消";
        _cancelCreateButton.Pressed += OnCancelCreatePressed;
        buttonPanel.AddChild(_cancelCreateButton);
    }

    private void SetupInviteDialog()
    {
        _inviteDialog = new WindowDialog();
        _inviteDialog.Title = "邀请玩家";
        _inviteDialog.CustomMinimumSize = new Vector2(300, 150);
        AddChild(_inviteDialog);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 10);
        _inviteDialog.AddChild(vbox);

        var playerLabel = new Label();
        playerLabel.Text = "玩家ID:";
        vbox.AddChild(playerLabel);

        _playerIdInput = new LineEdit();
        _playerIdInput.Placeholder = "输入玩家ID";
        vbox.AddChild(_playerIdInput);

        var buttonPanel = new HBoxContainer();
        buttonPanel.AddThemeConstantOverride("separation", 10);
        vbox.AddChild(buttonPanel);

        _sendInviteButton = new Button();
        _sendInviteButton.Text = "发送邀请";
        _sendInviteButton.Pressed += OnSendInvitePressed;
        buttonPanel.AddChild(_sendInviteButton);

        _cancelInviteButton = new Button();
        _cancelInviteButton.Text = "取消";
        _cancelInviteButton.Pressed += OnCancelInvitePressed;
        buttonPanel.AddChild(_cancelInviteButton);
    }

    private void ConnectSignals()
    {
        // Button signal -> Action wiring
        OnClosePressed += HandleClosePressed;
        OnRefreshPressed += HandleRefreshPressed;
        OnCreatePartyPressed += HandleCreatePartyPressed;
        OnInvitePressed += HandleInvitePressed;
        OnLeavePressed += HandleLeavePressed;
        OnDisbandPressed += HandleDisbandPressed;
        OnConfirmCreatePressed += HandleConfirmCreatePressed;
        OnCancelCreatePressed += HandleCancelCreatePressed;
        OnSendInvitePressed += HandleSendInvitePressed;
        OnCancelInvitePressed += HandleCancelInvitePressed;

        // PartySystem signal -> Action wiring
        OnPartyCreated += HandlePartyCreated;
        OnPartyDisbanded += HandlePartyDisbanded;
        OnPlayerJoinedParty += HandlePlayerJoinedParty;
        OnPlayerLeftParty += HandlePlayerLeftParty;

        PartySystem.Instance.PartyCreated += OnPartyCreated;
        PartySystem.Instance.PartyDisbanded += OnPartyDisbanded;
        PartySystem.Instance.PlayerJoinedParty += OnPlayerJoinedParty;
        PartySystem.Instance.PlayerLeftParty += OnPlayerLeftParty;
    }

    public override void _Process(float delta)
    {
        if (Visible)
        {
            RefreshPartyList();
            RefreshMyParty();
            RefreshStats();
        }
    }

    private void RefreshPartyList()
    {
        foreach (var child in _partyListContainer.GetChildren())
        {
            child.QueueFree();
        }

        var parties = PartySystem.Instance.GetAvailableParties();
        foreach (var party in parties)
        {
            var partyPanel = CreatePartyListItem(party);
            _partyListContainer.AddChild(partyPanel);
        }

        if (parties.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "暂无可用队伍";
            emptyLabel.Align = Label.AlignEnum.Center;
            emptyLabel.AddThemeColorOverride("font_color", _secondaryColor);
            _partyListContainer.AddChild(emptyLabel);
        }
    }

    private Control CreatePartyListItem(PartyData.Party party)
    {
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(0, 60);

        var style = new StyleBoxFlat();
        style.BgColor = _secondaryColor;
        style.SetCornerRadiusAll(4);
        panel.AddThemeStyleboxOverride("panel", style);

        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 10);
        panel.AddChild(hbox);

        var infoVBox = new VBoxContainer();
        infoVBox.AddThemeConstantOverride("separation", 2);
        hbox.AddChild(infoVBox);

        var nameLabel = new Label();
        nameLabel.Text = party.PartyName;
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        infoVBox.AddChild(nameLabel);

        var detailLabel = new Label();
        string typeName = PartyDatabase.PartyTypeNames[(int)party.Type];
        detailLabel.Text = $"{typeName} | {party.Members.Count}/{party.MaxMembers}人 | 经验+{party.ExpShareBonus * 100:F0}%";
        detailLabel.AddThemeColorOverride("font_color", _successColor);
        infoVBox.AddChild(detailLabel);

        var joinButton = new Button();
        joinButton.Text = "加入";
        joinButton.Pressed += () => OnJoinPartyPressed(party.PartyId);
        hbox.AddChild(joinButton);

        return panel;
    }

    private void RefreshMyParty()
    {
        // Get current player ID (would normally come from PlayerManager)
        int playerId = 1; // Placeholder
        
        var party = PartySystem.Instance.GetPlayerParty(playerId);
        if (party == null)
        {
            _currentPartyName.Text = "队伍名称: 未加入队伍";
            _partyTypeLabel.Text = "队伍类型: -";
            _partyStateLabel.Text = "状态: -";
            _bonusLabel.Text = "队伍加成: -";
            
            _leaveButton.Disabled = true;
            _disbandButton.Disabled = true;
            _inviteButton.Disabled = true;

            foreach (var child in _memberListContainer.GetChildren())
            {
                child.QueueFree();
            }
            return;
        }

        _currentPartyName.Text = $"队伍名称: {party.PartyName}";
        _partyTypeLabel.Text = $"队伍类型: {PartyDatabase.PartyTypeNames[(int)party.Type]}";
        _partyStateLabel.Text = $"状态: {party.State}";
        _bonusLabel.Text = $"队伍加成:\n经验 +{party.ExpShareBonus * 100:F0}%\n掉落 +{party.DropRateBonus * 100:F0}%\n伤害 +{party.DamageBonus * 100:F0}%\n防御 +{party.DefenseBonus * 100:F0}%";

        var isLeader = party.LeaderId == playerId;
        _leaveButton.Disabled = false;
        _disbandButton.Disabled = !isLeader;
        _inviteButton.Disabled = party.Members.Count >= party.MaxMembers;

        foreach (var child in _memberListContainer.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var member in party.Members)
        {
            var memberPanel = CreateMemberItem(member, member.PlayerId == playerId);
            _memberListContainer.AddChild(memberPanel);
        }
    }

    private Control CreateMemberItem(PartyData.PartyMember member, bool isSelf)
    {
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(0, 50);

        var style = new StyleBoxFlat();
        style.BgColor = isSelf ? _primaryColor : _secondaryColor;
        style.SetCornerRadiusAll(4);
        panel.AddThemeStyleboxOverride("panel", style);

        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 10);
        panel.AddChild(hbox);

        var nameLabel = new Label();
        nameLabel.Text = member.PlayerName;
        nameLabel.AddThemeFontSizeOverride("font_size", 14);
        if (isSelf)
            nameLabel.AddThemeColorOverride("font_color", Colors.White);
        hbox.AddChild(nameLabel);

        var roleLabel = new Label();
        roleLabel.Text = $"[{PartyDatabase.RoleNames[(int)member.Role]}]";
        if (member.Role == PartyData.MemberRole.Leader)
            roleLabel.AddThemeColorOverride("font_color", _warningColor);
        hbox.AddChild(roleLabel);

        var levelLabel = new Label();
        levelLabel.Text = $"Lv.{member.Level}";
        hbox.AddChild(levelLabel);

        var readyLabel = new Label();
        readyLabel.Text = member.IsReady ? "✓" : "○";
        readyLabel.AddThemeColorOverride("font_color", member.IsReady ? _successColor : _secondaryColor);
        hbox.AddChild(readyLabel);

        var statusLabel = new Label();
        statusLabel.Text = member.IsOnline ? "🟢" : "🔴";
        hbox.AddChild(statusLabel);

        var healthLabel = new Label();
        healthLabel.Text = $"HP: {member.HealthPercent * 100:F0}%";
        if (member.HealthPercent < 0.3f)
            healthLabel.AddThemeColorOverride("font_color", _dangerColor);
        else if (member.HealthPercent < 0.6f)
            healthLabel.AddThemeColorOverride("font_color", _warningColor);
        hbox.AddChild(healthLabel);

        return panel;
    }

    private void RefreshStats()
    {
        int playerId = 1; // Placeholder
        var data = PartySystem.Instance.GetPlayerPartyData(playerId);

        _totalPartiesLabel.Text = $"参加队伍次数: {data.TotalPartiesJoined}";
        _totalWinsLabel.Text = $"胜利次数: {data.TotalPartiesWon}";
        
        float winRate = data.TotalPartiesJoined > 0 ? 
            (float)data.TotalPartiesWon / data.TotalPartiesJoined * 100 : 0;
        _winRateLabel.Text = $"胜率: {winRate:F1}%";

        _membersInvitedLabel.Text = $"邀请成员数: {data.TotalPartyMembersInvited}";

        foreach (var child in _historyContainer.GetChildren())
        {
            child.QueueFree();
        }

        var recentHistory = data.History.Count > 10 ? 
            data.GetRange(data.History.Count - 10, 10) : data.History;

        foreach (var record in recentHistory)
        {
            var recordLabel = new Label();
            recordLabel.Text = $"{record.PartyName} | {PartyDatabase.PartyTypeNames[(int)record.Type]} | " +
                $"{(record.WasVictory ? "胜利" : "失败")} | {record.LeftAt:MM-dd HH:mm}";
            recordLabel.AddThemeColorOverride("font_color", 
                record.WasVictory ? _successColor : _dangerColor);
            _historyContainer.AddChild(recordLabel);
        }
    }

    private void HandleClosePressed()
    {
        Visible = false;
    }

    private void HandleRefreshPressed()
    {
        RefreshPartyList();
    }

    private void HandleCreatePartyPressed()
    {
        _createDialog.PopupCentered();
    }

    private void HandleJoinPartyPressed(string partyId)
    {
        int playerId = 1; // Placeholder
        PartySystem.Instance.JoinParty(partyId, playerId, "Player_" + playerId, 1, 0);
    }

    private void HandleInvitePressed()
    {
        _inviteDialog.PopupCentered();
    }

    private void HandleLeavePressed()
    {
        int playerId = 1; // Placeholder
        PartySystem.Instance.LeaveParty(playerId);
    }

    private void HandleDisbandPressed()
    {
        int playerId = 1;
        var party = PartySystem.Instance.GetPlayerParty(playerId);
        if (party != null)
        {
            PartySystem.Instance.DisbandParty(party.PartyId);
        }
    }

    private void HandleConfirmCreatePressed()
    {
        int playerId = 1; // Placeholder
        var type = (PartyData.PartyType)_partyTypeOption.Selected;
        var name = _partyNameInput.Text;
        
        PartySystem.Instance.CreateParty(playerId, "Player_" + playerId, type, name);
        _createDialog.Hide();
    }

    private void HandleCancelCreatePressed()
    {
        _createDialog.Hide();
    }

    private void HandleSendInvitePressed()
    {
        int playerId = 1; // Placeholder
        if (int.TryParse(_playerIdInput.Text, out int targetId))
        {
            var party = PartySystem.Instance.GetPlayerParty(playerId);
            if (party != null)
            {
                PartySystem.Instance.SendInvite(playerId, "Player_" + playerId, targetId, party.Type);
            }
        }
        _inviteDialog.Hide();
    }

    private void HandleCancelInvitePressed()
    {
        _inviteDialog.Hide();
    }

    private void HandlePartyCreated(string partyId)
    {
        RefreshPartyList();
        RefreshMyParty();
    }

    private void HandlePartyDisbanded(string partyId)
    {
        RefreshPartyList();
        RefreshMyParty();
    }

    private void HandlePlayerJoinedParty(string partyId, int playerId)
    {
        RefreshPartyList();
        RefreshMyParty();
    }

    private void HandlePlayerLeftParty(string partyId, int playerId)
    {
        RefreshPartyList();
        RefreshMyParty();
    }

    public void Toggle()
    {
        Visible = !Visible;
        if (Visible)
        {
            RefreshPartyList();
            RefreshMyParty();
            RefreshStats();
        }
    }
}
}
