using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 队伍系统UI
/// 队伍管理界面
/// </summary>
public class PartyUI : Control
{
    private Label _titleLabel;
    private VBoxContainer _memberList;
    private Label _buffLabel;
    private HBoxContainer _buttonContainer;
    private Button _createButton;
    private Button _leaveButton;
    private Button _inviteButton;
    private Label _settingsLabel;
    private CheckBox _shareExpCheck;
    private CheckBox _shareLootCheck;
    
    private int _selectedMemberId = -1;
    private bool _isVisible = false;

    public override void _Ready()
    {
        Visible = false;
        _isVisible = false;
        
        // 背景面板
        var panel = new Panel
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            Color = new Color(0.1f, 0.1f, 0.15f, 0.95f)
        };
        AddChild(panel);
        
        // 标题
        _titleLabel = new Label
        {
            Text = "队伍系统",
            AnchorLeft = 0.5f,
            AnchorRight = 0.5f,
            AnchorTop = 0.02f,
            AnchorBottom = 0.08f,
            Align = Label.AlignEnum.Center,
            FontSize = 24
        };
        AddChild(_titleLabel);
        
        // 成员列表容器
        var scrollContainer = new ScrollContainer
        {
            AnchorLeft = 0.05f,
            AnchorRight = 0.95f,
            AnchorTop = 0.1f,
            AnchorBottom = 0.5f
        };
        AddChild(scrollContainer);
        
        _memberList = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(0, 300)
        };
        scrollContainer.AddChild(_memberList);
        
        // Buff显示
        _buffLabel = new Label
        {
            Text = "队伍加成: 无",
            AnchorLeft = 0.05f,
            AnchorRight = 0.95f,
            AnchorTop = 0.52f,
            AnchorBottom = 0.58f
        };
        AddChild(_buffLabel);
        
        // 设置
        _settingsLabel = new Label
        {
            Text = "队伍设置:",
            AnchorLeft = 0.05f,
            AnchorRight = 0.95f,
            AnchorTop = 0.60f,
            AnchorBottom = 0.64f
        };
        AddChild(_settingsLabel);
        
        _shareExpCheck = new CheckBox
        {
            Text = "共享经验",
            AnchorLeft = 0.05f,
            AnchorTop = 0.65f,
            Pressed = true
        };
        _shareExpCheck.Toggled += OnShareExpToggled;
        AddChild(_shareExpCheck);
        
        _shareLootCheck = new CheckBox
        {
            Text = "共享战利品",
            AnchorLeft = 0.25f,
            AnchorTop = 0.65f,
            Pressed = false
        };
        _shareLootCheck.Toggled += OnShareLootToggled;
        AddChild(_shareLootCheck);
        
        // 按钮容器
        _buttonContainer = new HBoxContainer
        {
            AnchorLeft = 0.05f,
            AnchorRight = 0.95f,
            AnchorTop = 0.72f,
            AnchorBottom = 0.82f
        };
        AddChild(_buttonContainer);
        
        _createButton = new Button
        {
            Text = "创建队伍",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        _createButton.Pressed += OnCreatePartyPressed;
        _buttonContainer.AddChild(_createButton);
        
        _inviteButton = new Button
        {
            Text = "邀请玩家",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        _inviteButton.Pressed += OnInvitePressed;
        _buttonContainer.AddChild(_inviteButton);
        
        _leaveButton = new Button
        {
            Text = "离开队伍",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        _leaveButton.Pressed += OnLeavePartyPressed;
        _buttonContainer.AddChild(_leaveButton);
        
        // 操作说明
        var helpLabel = new Label
        {
            Text = "快捷键: P | 操作: ↑/↓ 选择 | Enter 确认 | K 踢人 | L 转让队长",
            AnchorLeft = 0.05f,
            AnchorRight = 0.95f,
            AnchorTop = 0.85f,
            AnchorBottom = 0.92f,
            Modulate = new Color(0.7f, 0.7f, 0.7f)
        };
        AddChild(helpLabel);
        
        // 监听PartySystem事件
        if (PartySystem.Instance != null)
        {
            PartySystem.Instance.OnPartyCreated += OnPartyCreated;
            PartySystem.Instance.OnPartyJoined += OnPartyJoined;
            PartySystem.Instance.OnPartyLeft += OnPartyLeft;
            PartySystem.Instance.OnMemberJoined += OnMemberJoined;
            PartySystem.Instance.OnMemberLeft += OnMemberLeft;
            PartySystem.Instance.OnBuffAdded += OnBuffAdded;
            PartySystem.Instance.OnBuffRemoved += OnBuffRemoved;
        }
    }

    public override void _Input(InputEvent evt)
    {
        if (!_isVisible) return;
        
        if (evt is InputEventKey keyEvt && keyEvt.Pressed)
        {
            switch (keyEvt.Scancode)
            {
                case KeyList.P:
                case KeyList.Escape:
                    ToggleVisibility();
                    break;
                case KeyList.Up:
                    SelectPreviousMember();
                    break;
                case KeyList.Down:
                    SelectNextMember();
                    break;
                case KeyList.K:
                    KickSelectedMember();
                    break;
                case KeyList.L:
                    TransferLeadershipToSelected();
                    break;
            }
        }
    }

    /// <summary>
    /// 切换可见性
    /// </summary>
    public void ToggleVisibility()
    {
        _isVisible = !_isVisible;
        Visible = _isVisible;
        
        if (_isVisible)
        {
            RefreshUI();
        }
    }

    /// <summary>
    /// 刷新UI
    /// </summary>
    private void RefreshUI()
    {
        // 清空列表
        foreach (Node child in _memberList.GetChildren())
        {
            child.QueueFree();
        }
        
        if (PartySystem.Instance == null || !PartySystem.Instance.IsInParty)
        {
            _titleLabel.Text = "队伍系统 (未加入)";
            _createButton.Text = "创建队伍";
            _leaveButton.Text = "离开";
            _inviteButton.Disabled = true;
            _shareExpCheck.Disabled = true;
            _shareLootCheck.Disabled = true;
            
            // 显示提示
            var hint = new Label
            {
                Text = "未加入队伍",
                Align = Label.AlignEnum.Center
            };
            _memberList.AddChild(hint);
        }
        else
        {
            _titleLabel.Text = $"队伍系统 (ID: {PartySystem.Instance.PartyId})";
            _createButton.Text = PartySystem.Instance.IsLeader ? "解散队伍" : "";
            _leaveButton.Text = "离开队伍";
            _inviteButton.Disabled = !PartySystem.Instance.IsLeader;
            _shareExpCheck.Disabled = false;
            _shareLootCheck.Disabled = !PartySystem.Instance.IsLeader;
            _shareExpCheck.Pressed = PartySystem.Instance.ShareExp;
            _shareLootCheck.Pressed = PartySystem.Instance.ShareLoot;
            
            // 显示成员列表
            var members = PartySystem.Instance.GetMembers();
            foreach (var member in members)
            {
                var memberPanel = CreateMemberPanel(member);
                _memberList.AddChild(memberPanel);
            }
            
            // 更新Buff显示
            UpdateBuffDisplay();
        }
    }

    /// <summary>
    /// 创建成员面板
    /// </summary>
    private Control CreateMemberPanel(PartySystem.PartyMember member)
    {
        var panel = new PanelContainer
        {
            CustomMinimumSize = new Vector2(0, 50)
        };
        
        var hbox = new HBoxContainer();
        panel.AddChild(hbox);
        
        // 角色图标/颜色
        var roleLabel = new Label
        {
            Text = GetRoleIcon(member.Role),
            CustomMinimumSize = new Vector2(40, 0)
        };
        hbox.AddChild(roleLabel);
        
        // 玩家名
        var nameLabel = new Label
        {
            Text = member.PlayerName,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        if (member.PlayerId == PartySystem.Instance?.LocalPlayerId)
        {
            nameLabel.Modulate = new Color(1f, 0.8f, 0.4f); // 高亮自己的名字
        }
        hbox.AddChild(nameLabel);
        
        // 等级
        var levelLabel = new Label
        {
            Text = $"Lv.{member.Level}",
            CustomMinimumSize = new Vector2(60, 0)
        };
        hbox.AddChild(levelLabel);
        
        // 状态
        var statusLabel = new Label
        {
            Text = member.IsOnline ? "在线" : "离线",
            CustomMinimumSize = new Vector2(60, 0),
            Modulate = member.IsOnline ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.4f, 0.4f)
        };
        hbox.AddChild(statusLabel);
        
        // 角色
        var roleTextLabel = new Label
        {
            Text = GetRoleName(member.Role),
            CustomMinimumSize = new Vector2(80, 0)
        };
        hbox.AddChild(roleTextLabel);
        
        return panel;
    }

    /// <summary>
    /// 获取角色图标
    /// </summary>
    private string GetRoleIcon(PartySystem.PartyRole role)
    {
        switch (role)
        {
            case PartySystem.PartyRole.Leader: return "👑";
            case PartySystem.PartyRole.Tank: return "🛡";
            case PartySystem.PartyRole.Healer: return "💚";
            case PartySystem.PartyRole.DamageDealer: return "⚔";
            case PartySystem.PartyRole.Support: return "✨";
            case PartySystem.PartyRole.Scout: return "👁";
            default: return "•";
        }
    }

    /// <summary>
    /// 获取角色名称
    /// </summary>
    private string GetRoleName(PartySystem.PartyRole role)
    {
        switch (role)
        {
            case PartySystem.PartyRole.Leader: return "队长";
            case PartySystem.PartyRole.Tank: return "坦克";
            case PartySystem.PartyRole.Healer: return "治疗";
            case PartySystem.PartyRole.DamageDealer: return "输出";
            case PartySystem.PartyRole.Support: return "辅助";
            case PartySystem.PartyRole.Scout: return "侦察";
            default: return "成员";
        }
    }

    /// <summary>
    /// 更新Buff显示
    /// </summary>
    private void UpdateBuffDisplay()
    {
        if (PartySystem.Instance == null || !PartySystem.Instance.IsInParty)
        {
            _buffLabel.Text = "队伍加成: 无";
            return;
        }
        
        var buffs = PartySystem.Instance.GetAllBuffValues();
        if (buffs.Count == 0)
        {
            _buffLabel.Text = "队伍加成: 无";
            return;
        }
        
        string buffText = "队伍加成: ";
        foreach (var kvp in buffs)
        {
            buffText += $"{GetBuffName(kvp.Key)}+{kvp.Value:P0} ";
        }
        _buffLabel.Text = buffText;
    }

    /// <summary>
    /// 获取Buff名称
    /// </summary>
    private string GetBuffName(PartySystem.PartyBuffType type)
    {
        switch (type)
        {
            case PartySystem.PartyBuffType.ExperienceBoost: return "经验";
            case PartySystem.PartyBuffType.GoldBoost: return "金币";
            case PartySystem.PartyBuffType.DamageBoost: return "伤害";
            case PartySystem.PartyBuffType.DefenseBoost: return "防御";
            case PartySystem.PartyBuffType.HealthRegen: return "生命";
            case PartySystem.PartyBuffType.ManaRegen: return "法力";
            case PartySystem.PartyBuffType.LuckBoost: return "幸运";
            case PartySystem.PartyBuffType.DropRateBoost: return "掉落";
            default: return "未知";
        }
    }

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

    // 事件处理
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

    private void OnInvitePressed()
    {
        if (PartySystem.Instance != null && PartySystem.Instance.IsLeader)
        {
            // 显示玩家选择界面（模拟在线玩家列表）
            ShowPlayerSelectionUI();
        }
    }
    
    /// <summary>
    /// 显示玩家选择界面
    /// </summary>
    private void ShowPlayerSelectionUI()
    {
        // 创建玩家选择弹窗
        var popup = new WindowDialog();
        popup.Title = "邀请玩家";
        popup.RectMinSize = new Vector2(400, 300);
        AddChild(popup);
        
        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.Margin = new MarginContainer.Margin { Top = 40, Bottom = 40, Left = 20, Right = 20 };
        popup.AddChild(vbox);
        
        // 标题
        var titleLabel = new Label();
        titleLabel.Text = "选择要邀请的玩家:";
        titleLabel.Align = Label.AlignEnum.Center;
        vbox.AddChild(titleLabel);
        
        // 玩家列表（模拟数据，实际应从服务器获取）
        var playerList = new ItemList();
        playerList.RectMinSize = new Vector2(0, 200);
        
        // 模拟在线玩家（实际应从网络获取）
        var onlinePlayers = GetOnlinePlayers();
        foreach (var player in onlinePlayers)
        {
            playerList.AddItem($"{player.Value} (Lv.{player.Key})");
        }
        
        if (onlinePlayers.Count == 0)
        {
            playerList.AddItem("当前没有在线玩家");
        }
        
        vbox.AddChild(playerList);
        
        // 按钮容器
        var buttonBox = new HBoxContainer();
        buttonBox.Alignment = HBoxContainer.AlignmentMode.Center;
        vbox.AddChild(buttonBox);
        
        // 邀请按钮
        var inviteBtn = new Button();
        inviteBtn.Text = "邀请";
        inviteBtn.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        inviteBtn.Connect("pressed", this, nameof(OnPlayerSelected), new[] { playerList.Name });
        buttonBox.AddChild(inviteBtn);
        
        // 取消按钮
        var cancelBtn = new Button();
        cancelBtn.Text = "取消";
        cancelBtn.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        cancelBtn.Connect("pressed", this, nameof(OnInviteCancelled), new[] { popup.Name });
        buttonBox.AddChild(cancelBtn);
        
        popup.PopupCentered();
        GD.Print("[PartyUI] Player selection dialog opened");
    }
    
    /// <summary>
    /// 获取在线玩家列表（模拟实现）
    /// </summary>
    private Dictionary<int, string> GetOnlinePlayers()
    {
        var players = new Dictionary<int, string>();
        
        // 模拟一些在线玩家（实际应从网络获取）
        // 这里返回空列表，因为是单人游戏
        // 多人模式下应从NetworkClient获取在线玩家
        
        return players;
    }
    
    /// <summary>
    /// 玩家选择确认
    /// </summary>
    private void OnPlayerSelected(string listName)
    {
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
        var popup = FindChild(popupName, true, false) as WindowDialog;
        if (popup != null)
        {
            popup.QueueFree();
        }
        GD.Print("[PartyUI] Invite cancelled");
    }

    private void OnLeavePartyPressed()
    {
        PartySystem.Instance?.LeaveParty();
    }

    private void OnShareExpToggled(bool pressed)
    {
        if (PartySystem.Instance != null)
        {
            PartySystem.Instance.SetShareExp(pressed);
        }
    }

    private void OnShareLootToggled(bool pressed)
    {
        if (PartySystem.Instance != null)
        {
            PartySystem.Instance.SetShareLoot(pressed);
        }
    }

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

    public override void _ExitTree()
    {
        if (PartySystem.Instance != null)
        {
            PartySystem.Instance.OnPartyCreated -= OnPartyCreated;
            PartySystem.Instance.OnPartyJoined -= OnPartyJoined;
            PartySystem.Instance.OnPartyLeft -= OnPartyLeft;
            PartySystem.Instance.OnMemberJoined -= OnMemberJoined;
            PartySystem.Instance.OnMemberLeft -= OnMemberLeft;
            PartySystem.Instance.OnBuffAdded -= OnBuffAdded;
            PartySystem.Instance.OnBuffRemoved -= OnBuffRemoved;
        }
    }
}
