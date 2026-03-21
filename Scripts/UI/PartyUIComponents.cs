using Godot;
using System;
using System.Collections.Generic;

public partial class PartyUI
{
    // REQ-058-11: Migrated from Godot 3 .Connect() to C# event
    public event Action<string> OnPlayerSelectedEvent;
    public event Action<string> OnInviteCancelledEvent;
    
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
        
        // 邀请按钮 (REQ-058-11: migrated from Godot 3 .Connect() to C# event +=)
        var inviteBtn = new Button();
        inviteBtn.Text = "邀请";
        inviteBtn.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        inviteBtn.Pressed += () => OnPlayerSelected(playerList.Name); // NEW
        inviteBtn.Connect("pressed", this, nameof(OnPlayerSelected), new[] { playerList.Name }); // TODO: Remove after migration
        buttonBox.AddChild(inviteBtn);
        
        // 取消按钮 (REQ-058-11: migrated from Godot 3 .Connect() to C# event +=)
        var cancelBtn = new Button();
        cancelBtn.Text = "取消";
        cancelBtn.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        cancelBtn.Pressed += () => OnInviteCancelled(popup.Name); // NEW
        cancelBtn.Connect("pressed", this, nameof(OnInviteCancelled), new[] { popup.Name }); // TODO: Remove after migration
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
}
