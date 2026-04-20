using Godot;
using System;

public partial class GuildUI {
    // ===== 按钮事件 =====

    private void OnCreateButtonPressed() {
        var popup = new AcceptDialog();
        popup.Title = "创建公会";
        popup.SetAnchorsPreset(Control.LayoutPreset.Center);
        popup.CustomMinimumSize = new Vector2(400, 200);
        GetTree().CurrentScene.AddChild(popup);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 10);
        popup.AddChild(vbox);

        var input = new LineEdit();
        input.PlaceholderText = "输入公会名称...";
        vbox.AddChild(input);

        popup.OkButtonPressed += () => {
            if (GuildSystem.Instance.CreateGuild(input.Text)) {
                RefreshUI();
            }
        };
        popup.CancelButtonPressed += () => popup.QueueFree();
        popup.PopupCentered();
    }

    private void OnJoinButtonPressed() {
        if (selectedGuildId == "") {
            GD.Print("请先选择一个公会");
            return;
        }
        GuildSystem.Instance.JoinGuild(selectedGuildId);
        RefreshUI();
    }

    private void OnLeaveButtonPressed() {
        var confirm = new ConfirmDialog();
        confirm.Title = "确认离开";
        confirm.DialogText = "确定要离开当前公会吗？";
        GetTree().CurrentScene.AddChild(confirm);
        confirm.Confirmed += () => {
            GuildSystem.Instance.LeaveGuild();
            RefreshUI();
        };
        confirm.PopupCentered();
    }

    private void OnDisbandButtonPressed() {
        var confirm = new ConfirmDialog();
        confirm.Title = "确认解散";
        confirm.DialogText = "确定要解散公会吗？此操作不可恢复！";
        GetTree().CurrentScene.AddChild(confirm);
        confirm.Confirmed += () => {
            GuildSystem.Instance.DisbandGuild();
            RefreshUI();
        };
        confirm.PopupCentered();
    }

    private void OnUpdateNoticePressed() {
        if (noticeInput.Text.Length > 0) {
            GuildSystem.Instance.UpdateNotice(noticeInput.Text);
            RefreshUI();
        }
    }

    private void OnSearchSubmitted(string text) {
        OnSearchPressed();
    }

    private void OnSearchPressed() {
        // 注：单人模式按名称筛选公会
        RefreshGuildList();
    }

    // ===== 列表选择事件 =====

    private void OnGuildSelected(int index) {
        if (index >= 0 && index < GuildSystem.Instance.AvailableGuilds.Count) {
            selectedGuildId = GuildSystem.Instance.AvailableGuilds[index].GuildId;
        }
    }

    private void OnMemberSelected(int index) {
        if (GuildSystem.Instance.CurrentGuild != null &&
            index >= 0 && index < GuildSystem.Instance.CurrentGuild.Members.Count) {
            selectedMemberId = GuildSystem.Instance.CurrentGuild.Members[index].PlayerId;
        }
    }

    private void OnBuildingSelected(int index) {
        if (GuildSystem.Instance.CurrentGuild != null) {
            int i = 0;
            foreach (var building in GuildSystem.Instance.CurrentGuild.Buildings.Values) {
                if (i == index) {
                    selectedBuildingId = building.BuildingId;
                    break;
                }
                i++;
            }
        }
    }

    private void OnSkillSelected(int index) {
        if (GuildSystem.Instance.CurrentGuild != null) {
            int i = 0;
            foreach (var skill in GuildSystem.Instance.CurrentGuild.Skills.Values) {
                if (i == index) {
                    selectedSkillId = skill.SkillId;
                    break;
                }
                i++;
            }
        }
    }

    private void OnApplicationSelected(int index) {
        // 注：单人模式选择申请（可选功能）
    }

    // ===== 成员管理事件 =====

    private void OnPromotePressed() {
        if (selectedMemberId != "") {
            GuildSystem.Instance.PromoteMember(selectedMemberId);
            RefreshUI();
        }
    }

    private void OnDemotePressed() {
        if (selectedMemberId != "") {
            GuildSystem.Instance.DemoteMember(selectedMemberId);
            RefreshUI();
        }
    }

    private void OnKickPressed() {
        if (selectedMemberId != "") {
            GuildSystem.Instance.KickMember(selectedMemberId);
            RefreshUI();
        }
    }

    private void OnTransferPressed() {
        if (selectedMemberId != "") {
            GuildSystem.Instance.TransferLeadership(selectedMemberId);
            RefreshUI();
        }
    }

    private void OnUpgradeBuildingPressed() {
        if (selectedBuildingId != "") {
            GuildSystem.Instance.UpgradeBuilding(selectedBuildingId);
            RefreshUI();
        }
    }

    private void OnLearnSkillPressed() {
        if (selectedSkillId != "") {
            GuildSystem.Instance.LearnSkill(selectedSkillId);
            RefreshUI();
        }
    }

    private void OnAcceptApplicationPressed() {
        GD.Print("申请处理功能：点击申请列表中的项目然后处理");
    }

    private void OnRejectApplicationPressed() {
        GD.Print("申请处理功能：点击申请列表中的项目然后处理");
    }

    // ===== 公共方法 =====

    public void Toggle() {
        if (Visible) {
            Hide();
        } else {
            Show();
            RefreshUI();
        }
    }
}
