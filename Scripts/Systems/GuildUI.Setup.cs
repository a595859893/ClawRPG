using Godot;
using System;
using System.Collections.Generic;

public partial class GuildUI {
    public override void _Ready() {
        SetupUI();
        RefreshUI();
    }

    private void SetupUI() {
        // 主面板
        var mainPanel = new Panel();
        mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainPanel.CustomMinimumSize = new Vector2(800, 600);
        AddChild(mainPanel);

        var mainVBox = new VBoxContainer();
        mainVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        mainVBox.AddThemeConstantOverride("separation", 10);
        mainPanel.AddChild(mainVBox);

        // 标题栏
        var titleBar = new HBoxContainer();
        titleBar.AddThemeConstantOverride("separation", 10);
        mainVBox.AddChild(titleBar);

        var titleLabel = new Label();
        titleLabel.Text = " 公会系统 ";
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        titleBar.AddChild(titleLabel);

        titleBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlagsExpand });

        var closeButton = new Button();
        closeButton.Text = "✕";
        closeButton.TooltipText = "关闭 (ESC)";
        closeButton.Pressed += () => Hide();
        titleBar.AddChild(closeButton);

        // 创建/加入按钮
        var buttonBar = new HBoxContainer();
        buttonBar.AddThemeConstantOverride("separation", 10);
        mainVBox.AddChild(buttonBar);

        createButton = new Button();
        createButton.Text = "创建公会";
        createButton.CustomMinimumSize = new Vector2(120, 40);
        createButton.Pressed += OnCreateButtonPressed;
        buttonBar.AddChild(createButton);

        joinButton = new Button();
        joinButton.Text = "加入公会";
        joinButton.CustomMinimumSize = new Vector2(120, 40);
        joinButton.Pressed += OnJoinButtonPressed;
        buttonBar.AddChild(joinButton);

        leaveButton = new Button();
        leaveButton.Text = "离开公会";
        leaveButton.CustomMinimumSize = new Vector2(120, 40);
        leaveButton.Pressed += OnLeaveButtonPressed;
        buttonBar.AddChild(leaveButton);

        disbandButton = new Button();
        disbandButton.Text = "解散公会";
        disbandButton.CustomMinimumSize = new Vector2(120, 40);
        disbandButton.Pressed += OnDisbandButtonPressed;
        buttonBar.AddChild(disbandButton);

        buttonBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlagsExpand });

        // 标签页容器
        var tabContainer = new TabContainer();
        tabContainer.SizeFlagsVertical = Control.SizeFlagsExpand;
        mainVBox.AddChild(tabContainer);

        // 信息标签页
        infoTab = CreateInfoTab();
        tabContainer.AddChild(infoTab);
        tabContainer.SetTabTitle(0, "公会信息");

        // 成员标签页
        membersTab = CreateMembersTab();
        tabContainer.AddChild(membersTab);
        tabContainer.SetTabTitle(1, "成员");

        // 建筑标签页
        buildingsTab = CreateBuildingsTab();
        tabContainer.AddChild(buildingsTab);
        tabContainer.SetTabTitle(2, "建筑");

        // 技能标签页
        skillsTab = CreateSkillsTab();
        tabContainer.AddChild(skillsTab);
        tabContainer.SetTabTitle(3, "技能");

        // 申请标签页
        applicationsTab = CreateApplicationsTab();
        tabContainer.AddChild(applicationsTab);
        tabContainer.SetTabTitle(4, "申请");
    }

    private Control CreateInfoTab() {
        var scroll = new ScrollContainer();
        scroll.AddThemeConstantOverride("h_separation", 10);
        scroll.AddThemeConstantOverride("v_separation", 10);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 15);
        scroll.AddChild(vbox);

        // 公会名称
        var nameRow = new HBoxContainer();
        vbox.AddChild(nameRow);

        var nameLabel = new Label();
        nameLabel.Text = "公会名称:";
        nameLabel.CustomMinimumSize = new Vector2(100, 0);
        nameRow.AddChild(nameLabel);

        guildNameLabel = new Label();
        guildNameLabel.Text = "未加入公会";
        nameRow.AddChild(guildNameLabel);

        // 等级
        var levelRow = new HBoxContainer();
        vbox.AddChild(levelRow);

        var levelLabel = new Label();
        levelLabel.Text = "公会等级:";
        levelLabel.CustomMinimumSize = new Vector2(100, 0);
        levelRow.AddChild(levelLabel);

        guildLevelLabel = new Label();
        guildLevelLabel.Text = "-";
        levelRow.AddChild(guildLevelLabel);

        // 成员数
        var memberRow = new HBoxContainer();
        vbox.AddChild(memberRow);

        var memberLabel = new Label();
        memberLabel.Text = "成员数量:";
        memberLabel.CustomMinimumSize = new Vector2(100, 0);
        memberRow.AddChild(memberLabel);

        memberCountLabel = new Label();
        memberCountLabel.Text = "-";
        memberRow.AddChild(memberCountLabel);

        // 贡献度
        var contribRow = new HBoxContainer();
        vbox.AddChild(contribRow);

        var contribLabel = new Label();
        contribLabel.Text = "我的贡献:";
        contribLabel.CustomMinimumSize = new Vector2(100, 0);
        contribRow.AddChild(contribLabel);

        contributionLabel = new Label();
        contributionLabel.Text = "-";
        contribRow.AddChild(contributionLabel);

        // 公告
        var noticeTitle = new Label();
        noticeTitle.Text = "公会公告:";
        vbox.AddChild(noticeTitle);

        noticeLabel = new Label();
        noticeLabel.Text = "暂无公告";
        vbox.AddChild(noticeLabel);

        // 公告输入
        var noticeInputRow = new HBoxContainer();
        vbox.AddChild(noticeInputRow);

        noticeInput = new LineEdit();
        noticeInput.PlaceholderText = "输入新公告...";
        noticeInput.SizeFlagsHorizontal = Control.SizeFlagsExpand;
        noticeInput.CustomMinimumSize = new Vector2(300, 0);
        noticeInputRow.AddChild(noticeInput);

        var updateNoticeButton = new Button();
        updateNoticeButton.Text = "更新公告";
        updateNoticeButton.Pressed += OnUpdateNoticePressed;
        noticeInputRow.AddChild(updateNoticeButton);

        // 搜索公会
        var searchRow = new HBoxContainer();
        vbox.AddChild(searchRow);

        var searchLabel = new Label();
        searchLabel.Text = "搜索公会:";
        searchLabel.CustomMinimumSize = new Vector2(100, 0);
        searchRow.AddChild(searchLabel);

        var searchInput = new LineEdit();
        searchInput.PlaceholderText = "输入公会名称...";
        searchInput.SizeFlagsHorizontal = Control.SizeFlagsExpand;
        searchInput.TextSubmitted += OnSearchSubmitted;
        searchRow.AddChild(searchInput);

        var searchButton = new Button();
        searchButton.Text = "搜索";
        searchButton.Pressed += OnSearchPressed;
        searchRow.AddChild(searchButton);

        // 公会列表
        var listLabel = new Label();
        listLabel.Text = "可加入的公会:";
        vbox.AddChild(listLabel);

        guildList = new ItemList();
        guildList.CustomMinimumSize = new Vector2(0, 200);
        guildList.SizeFlagsVertical = Control.SizeFlagsExpand;
        guildList.ItemSelected += OnGuildSelected;
        vbox.AddChild(guildList);

        return scroll;
    }

    private Control CreateMembersTab() {
        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 10);

        // 成员列表
        memberList = new ItemList();
        memberList.CustomMinimumSize = new Vector2(300, 0);
        memberList.SizeFlagsVertical = Control.SizeFlagsExpand;
        memberList.ItemSelected += OnMemberSelected;
        hbox.AddChild(memberList);

        // 详情面板
        var detailPanel = new VBoxContainer();
        detailPanel.AddThemeConstantOverride("separation", 10);
        hbox.AddChild(detailPanel);

        var detailNameLabel = new Label();
        detailNameLabel.Text = "选择成员查看详情";
        detailPanel.AddChild(detailNameLabel);

        var promoteButton = new Button();
        promoteButton.Text = "晋升";
        promoteButton.Pressed += OnPromotePressed;
        detailPanel.AddChild(promoteButton);

        var demoteButton = new Button();
        demoteButton.Text = "降职";
        demoteButton.Pressed += OnDemotePressed;
        detailPanel.AddChild(demoteButton);

        var kickButton = new Button();
        kickButton.Text = "踢出";
        kickButton.Pressed += OnKickPressed;
        detailPanel.AddChild(kickButton);

        var transferButton = new Button();
        transferButton.Text = "转让会长";
        transferButton.Pressed += OnTransferPressed;
        detailPanel.AddChild(transferButton);

        return hbox;
    }

    private Control CreateBuildingsTab() {
        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 10);

        // 建筑列表
        buildingList = new ItemList();
        buildingList.CustomMinimumSize = new Vector2(300, 0);
        buildingList.SizeFlagsVertical = Control.SizeFlagsExpand;
        buildingList.ItemSelected += OnBuildingSelected;
        hbox.AddChild(buildingList);

        // 详情面板
        var detailPanel = new VBoxContainer();
        detailPanel.AddThemeConstantOverride("separation", 10);
        hbox.AddChild(detailPanel);

        var detailLabel = new Label();
        detailLabel.Text = "选择建筑查看详情";
        detailPanel.AddChild(detailLabel);

        var upgradeButton = new Button();
        upgradeButton.Text = "升级建筑";
        upgradeButton.Pressed += OnUpgradeBuildingPressed;
        detailPanel.AddChild(upgradeButton);

        return hbox;
    }

    private Control CreateSkillsTab() {
        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 10);

        // 技能列表
        skillList = new ItemList();
        skillList.CustomMinimumSize = new Vector2(300, 0);
        skillList.SizeFlagsVertical = Control.SizeFlagsExpand;
        skillList.ItemSelected += OnSkillSelected;
        hbox.AddChild(skillList);

        // 详情面板
        var detailPanel = new VBoxContainer();
        detailPanel.AddThemeConstantOverride("separation", 10);
        hbox.AddChild(detailPanel);

        var detailLabel = new Label();
        detailLabel.Text = "选择技能查看详情";
        detailPanel.AddChild(detailLabel);

        var learnButton = new Button();
        learnButton.Text = "学习/升级技能";
        learnButton.Pressed += OnLearnSkillPressed;
        detailPanel.AddChild(learnButton);

        return hbox;
    }

    private Control CreateApplicationsTab() {
        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 10);

        // 申请列表
        applicationList = new ItemList();
        applicationList.CustomMinimumSize = new Vector2(300, 0);
        applicationList.SizeFlagsVertical = Control.SizeFlagsExpand;
        applicationList.ItemSelected += OnApplicationSelected;
        hbox.AddChild(applicationList);

        // 按钮面板
        var buttonPanel = new VBoxContainer();
        buttonPanel.AddThemeConstantOverride("separation", 10);
        hbox.AddChild(buttonPanel);

        var acceptButton = new Button();
        acceptButton.Text = "接受申请";
        acceptButton.Pressed += OnAcceptApplicationPressed;
        buttonPanel.AddChild(acceptButton);

        var rejectButton = new Button();
        rejectButton.Text = "拒绝申请";
        rejectButton.Pressed += OnRejectApplicationPressed;
        buttonPanel.AddChild(rejectButton);

        return hbox;
    }

    private void RefreshUI() {
        var guild = GuildSystem.Instance.CurrentGuild;
        bool hasGuild = guild != null;

        // 按钮状态
        createButton.Visible = !hasGuild;
        joinButton.Visible = !hasGuild;
        leaveButton.Visible = hasGuild;
        disbandButton.Visible = hasGuild && GuildSystem.Instance.PlayerData.Level == GuildLevel.Leader;

        if (hasGuild) {
            guildNameLabel.Text = guild.Name;
            guildLevelLabel.Text = GuildDatabase.GetGuildLevelName(guild.Level);
            memberCountLabel.Text = $"{guild.CurrentMembers}/{guild.MaxMembers}";
            contributionLabel.Text = GuildSystem.Instance.PlayerData.Contribution.ToString();
            noticeLabel.Text = guild.Notice;

            RefreshMemberList();
            RefreshBuildingList();
            RefreshSkillList();
            RefreshApplicationList();
        } else {
            guildNameLabel.Text = "未加入公会";
            guildLevelLabel.Text = "-";
            memberCountLabel.Text = "-";
            contributionLabel.Text = "-";
            noticeLabel.Text = "暂无公告";

            RefreshGuildList();
        }
    }

    private void RefreshGuildList() {
        guildList.Clear();
        foreach (var guild in GuildSystem.Instance.AvailableGuilds) {
            string text = $"{guild.Name} [L{guild.Level}] - {guild.CurrentMembers}/{guild.MaxMembers}人";
            guildList.AddItem(text);
        }
    }

    private void RefreshMemberList() {
        memberList.Clear();
        if (GuildSystem.Instance.CurrentGuild == null) return;

        foreach (var member in GuildSystem.Instance.CurrentGuild.Members) {
            string status = member.IsOnline ? "🟢" : "⚪";
            string text = $"{status} {member.PlayerName} [{GuildDatabase.GetLevelName(member.Level)}] - 贡献:{member.Contribution}";
            memberList.AddItem(text);
        }
    }

    private void RefreshBuildingList() {
        buildingList.Clear();
        if (GuildSystem.Instance.CurrentGuild == null) return;

        foreach (var building in GuildSystem.Instance.CurrentGuild.Buildings.Values) {
            string text = $"{building.Name} [L{building.Level}]";
            buildingList.AddItem(text);
        }
    }

    private void RefreshSkillList() {
        skillList.Clear();
        if (GuildSystem.Instance.CurrentGuild == null) return;

        foreach (var skill in GuildSystem.Instance.CurrentGuild.Skills.Values) {
            string text = $"{skill.Name} [L{skill.Level}/{skill.MaxLevel}]";
            if (skill.Level == 0) text += " (未解锁)";
            skillList.AddItem(text);
        }
    }

    private void RefreshApplicationList() {
        applicationList.Clear();
        if (GuildSystem.Instance.CurrentGuild?.Applications != null) {
            foreach (var app in GuildSystem.Instance.CurrentGuild.Applications) {
                string status = app.IsAccepted ? "✓ 已接受" : "待处理";
                applicationList.AddItem($"{app.PlayerName} - Lv.{app.PlayerLevel} [{status}]");
            }
        }
    }
}
