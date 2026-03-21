using Godot;
using System;
using System.Collections.Generic;

public partial class PartyUI
{
    /// <summary>
    /// 选择上一个成员
    /// </summary>
    private void SelectPreviousMember()
    {
        if (_memberList.GetChildCount() <= 0) return;
        
        int currentIndex = _selectedMemberId;
        _selectedMemberId = Math.Max(0, currentIndex - 1);
        HighlightSelectedMember();
    }

    /// <summary>
    /// 选择下一个成员
    /// </summary>
    private void SelectNextMember()
    {
        if (_memberList.GetChildCount() <= 0) return;
        
        int currentIndex = _selectedMemberId;
        _selectedMemberId = Math.Min(_memberList.GetChildCount() - 1, currentIndex + 1);
        HighlightSelectedMember();
    }

    /// <summary>
    /// 高亮选中成员
    /// </summary>
    private void HighlightSelectedMember()
    {
        for (int i = 0; i < _memberList.GetChildCount(); i++)
        {
            var child = _memberList.GetChild(i);
            if (child is PanelContainer panel)
            {
                panel.Modulate = (i == _selectedMemberId) ? new Color(1f, 1f, 0.5f) : new Color(1f, 1f, 1f);
            }
        }
    }

    /// <summary>
    /// 踢出选中成员
    /// </summary>
    private void KickSelectedMember()
    {
        if (!PartySystem.Instance.IsLeader || _selectedMemberId < 0) return;
        
        var members = PartySystem.Instance.GetMembers();
        if (_selectedMemberId < members.Count)
        {
            var member = members[_selectedMemberId];
            if (member.PlayerId != PartySystem.Instance.LocalPlayerId)
            {
                PartySystem.Instance.KickMember(member.PlayerId);
            }
        }
    }

    /// <summary>
    /// 转让队长给选中成员
    /// </summary>
    private void TransferLeadershipToSelected()
    {
        if (!PartySystem.Instance.IsLeader || _selectedMemberId < 0) return;
        
        var members = PartySystem.Instance.GetMembers();
        if (_selectedMemberId < members.Count)
        {
            var member = members[_selectedMemberId];
            if (member.PlayerId != PartySystem.Instance.LocalPlayerId)
            {
                PartySystem.Instance.TransferLeadership(member.PlayerId);
            }
        }
    }

    /// <summary>
    /// 创建/解散队伍按钮点击
    /// </summary>
    private void OnCreatePartyPressed()
    {
        if (PartySystem.Instance.IsInParty)
        {
            if (PartySystem.Instance.IsLeader)
            {
                // 解散队伍
                PartySystem.Instance.LeaveParty();
            }
        }
        else
        {
            // 创建队伍
            var root = GetTree().Root;
            foreach (Node child in root.GetChildren())
            {
                if (child is GameManager gm)
                {
                    PartySystem.Instance.CreateParty(gm.GetLocalPlayerId());
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 邀请按钮点击
    /// </summary>
    private void OnInvitePressed()
    {
        if (PartySystem.Instance != null && PartySystem.Instance.IsLeader)
        {
            // 显示玩家选择界面（模拟在线玩家列表）
            ShowPlayerSelectionUI();
        }
    }
    
    /// <summary>
    /// 玩家选择确认
    /// </summary>
    private void OnPlayerSelected(string listName)
    {
        // REQ-058-11: Invoke new event
        OnPlayerSelectedEvent?.Invoke(listName);
        var list = FindChild(listName, true, false) as ItemList;
        if (list != null && list.GetItemCount() > 0)
        {
            int selected = list.GetSelectedItems()[0];
            if (selected >= 0)
            {
                string playerInfo = list.GetItemText(selected);
                GD.Print($"[PartyUI] Selected player: {playerInfo}");
                
                // 从列表中提取玩家信息并邀请（模拟）
                // 实际应使用真实的玩家ID
                int playerId = selected + 1000; // 模拟ID
                PartySystem.Instance?.InvitePlayer(playerId);
            }
        }
        
        // 关闭弹窗
        foreach (var child in GetChildren())
        {
            if (child is WindowDialog popup)
            {
                popup.QueueFree();
            }
        }
    }
    
    /// <summary>
    /// 邀请取消
    /// </summary>
    private void OnInviteCancelled(string popupName)
    {
        // REQ-058-11: Invoke new event
        OnInviteCancelledEvent?.Invoke(popupName);
        var popup = FindChild(popupName, true, false) as WindowDialog;
        if (popup != null)
        {
            popup.QueueFree();
        }
        GD.Print("[PartyUI] Invite cancelled");
    }

    /// <summary>
    /// 离开队伍按钮点击
    /// </summary>
    private void OnLeavePartyPressed()
    {
        PartySystem.Instance?.LeaveParty();
    }

    /// <summary>
    /// 共享经验设置切换
    /// </summary>
    private void OnShareExpToggled(bool pressed)
    {
        if (PartySystem.Instance != null)
        {
            PartySystem.Instance.SetShareExp(pressed);
        }
    }

    /// <summary>
    /// 共享战利品设置切换
    /// </summary>
    private void OnShareLootToggled(bool pressed)
    {
        if (PartySystem.Instance != null)
        {
            PartySystem.Instance.SetShareLoot(pressed);
        }
    }

    // ===== 队伍系统事件处理 =====

    private void OnPartyCreated(int partyId)
    {
        RefreshUI();
    }

    private void OnPartyJoined(int partyId)
    {
        RefreshUI();
    }

    private void OnPartyLeft()
    {
        RefreshUI();
    }

    private void OnMemberJoined(int playerId, string playerName)
    {
        RefreshUI();
    }

    private void OnMemberLeft(int playerId)
    {
        RefreshUI();
    }

    private void OnBuffAdded(PartySystem.PartyBuff buff)
    {
        UpdateBuffDisplay();
    }

    private void OnBuffRemoved(PartySystem.PartyBuffType buffType)
    {
        UpdateBuffDisplay();
    }
}
