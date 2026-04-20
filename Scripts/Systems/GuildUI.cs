using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 公会UI - 管理公会界面显示
/// </summary>
public partial class GuildUI : Control {
    // 标签页
    private Control infoTab;
    private Control membersTab;
    private Control buildingsTab;
    private Control skillsTab;
    private Control applicationsTab;

    // 按钮
    private Button createButton;
    private Button joinButton;
    private Button leaveButton;
    private Button disbandButton;

    // 列表
    private ItemList guildList;
    private ItemList memberList;
    private ItemList buildingList;
    private ItemList skillList;
    private ItemList applicationList;

    // 详情面板
    private Label guildNameLabel;
    private Label guildLevelLabel;
    private Label memberCountLabel;
    private Label contributionLabel;
    private Label noticeLabel;

    // 输入
    private LineEdit noticeInput;

    // 当前选中
    private string selectedGuildId = "";
    private string selectedMemberId = "";
    private string selectedBuildingId = "";
    private string selectedSkillId = "";
}
