using Godot;
using System;
using System.Collections.Generic;
using PartySystemReal = ClawRPG.Scripts.Systems.PartySystem;

/// <summary>
/// 队伍系统UI
/// 队伍管理界面
/// </summary>
public partial class PartyUI : Control
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
        if (PartySystemReal.Instance != null)
        {
            PartySystemReal.Instance.OnPartyCreated += OnPartyCreated;
            PartySystemReal.Instance.OnPartyJoined += OnPartyJoined;
            PartySystemReal.Instance.OnPartyLeft += OnPartyLeft;
            PartySystemReal.Instance.OnMemberJoined += OnMemberJoined;
            PartySystemReal.Instance.OnMemberLeft += OnMemberLeft;
            PartySystemReal.Instance.OnBuffAdded += OnBuffAdded;
            PartySystemReal.Instance.OnBuffRemoved += OnBuffRemoved;
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
        
        if (PartySystemReal.Instance == null || !PartySystemReal.Instance.IsInParty)
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
            _titleLabel.Text = $"队伍系统 (ID: {PartySystemReal.Instance.PartyId})";
            _createButton.Text = PartySystemReal.Instance.IsLeader ? "解散队伍" : "";
            _leaveButton.Text = "离开队伍";
            _inviteButton.Disabled = !PartySystemReal.Instance.IsLeader;
            _shareExpCheck.Disabled = false;
            _shareLootCheck.Disabled = !PartySystemReal.Instance.IsLeader;
            _shareExpCheck.Pressed = PartySystemReal.Instance.ShareExp;
            _shareLootCheck.Pressed = PartySystemReal.Instance.ShareLoot;
            
            // 显示成员列表
            var members = PartySystemReal.Instance.GetMembers();
            foreach (var member in members)
            {
                var memberPanel = CreateMemberPanel(member);
                _memberList.AddChild(memberPanel);
            }
            
            // 更新Buff显示
            UpdateBuffDisplay();
        }
    }

    public override void _ExitTree()
    {
        if (PartySystemReal.Instance != null)
        {
            PartySystemReal.Instance.OnPartyCreated -= OnPartyCreated;
            PartySystemReal.Instance.OnPartyJoined -= OnPartyJoined;
            PartySystemReal.Instance.OnPartyLeft -= OnPartyLeft;
            PartySystemReal.Instance.OnMemberJoined -= OnMemberJoined;
            PartySystemReal.Instance.OnMemberLeft -= OnMemberLeft;
            PartySystemReal.Instance.OnBuffAdded -= OnBuffAdded;
            PartySystemReal.Instance.OnBuffRemoved -= OnBuffRemoved;
        }
    }
}
