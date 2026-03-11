using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 队伍技能UI
/// 显示队伍技能快捷键、冷却、激活状态
/// </summary>
public class TeamSkillUI : Control
{
    private VBoxContainer _skillContainer;
    private Label _titleLabel;
    private Dictionary<TeamSkillSystem.TeamSkillType, Label> _skillLabels = new Dictionary<TeamSkillSystem.TeamSkillType, Label>();
    private Dictionary<TeamSkillSystem.TeamSkillType, float> _cooldownTimers = new Dictionary<TeamSkillSystem.TeamSkillType, float>();
    private bool _isVisible = false; 

    public override void _Ready()
    {
        Visible = false; 
        SetupUI();
        ConnectSignals();
    }

    /// <summary>
    /// 设置UI布局
    /// </summary>
    private void SetupUI()
    {
        // 主容器
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.BottomRight);
        mainContainer.Position = new Vector2(-320, -20);
        mainContainer.CustomMinimumSize = new Vector2(300, 0);
        AddChild(mainContainer);

        // 标题
        _titleLabel = new Label();
        _titleLabel.Text = "队伍技能 (T)";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        mainContainer.AddChild(_titleLabel);

        // 技能容器
        _skillContainer = new VBoxContainer();
        mainContainer.AddChild(_skillContainer);

        // 创建技能按钮
        var skills = TeamSkillSystem.Instance.GetAllSkills();
        foreach (var skill in skills)
        {
            CreateSkillButton(skill);
        }
    }

    /// <summary>
    /// 创建技能按钮
    /// </summary>
    private void CreateSkillButton(TeamSkillSystem.TeamSkill skill)
    {
        var hbox = new HBoxContainer();
        _skillContainer.AddChild(hbox);

        // 技能名称
        var nameLabel = new Label();
        nameLabel.Text = skill.Name;
        nameLabel.CustomMinimumSize = new Vector2(100, 0);
        hbox.AddChild(nameLabel);

        // 描述
        var descLabel = new Label();
        descLabel.Text = skill.Description;
        descLabel.SizeFlagsHorizontal = SizeFlags.Expand;
        hbox.AddChild(descLabel);

        // 冷却标签
        var cooldownLabel = new Label();
        cooldownLabel.Text = skill.Cooldown > 0 ? $"{skill.Cooldown}s" : "Ready";
        cooldownLabel.CustomMinimumSize = new Vector2(60, 0);
        hbox.AddChild(cooldownLabel);

        _skillLabels[skill.Type] = cooldownLabel;
        _cooldownTimers[skill.Type] = 0;
    }

    /// <summary>
    /// 连接信号
    /// </summary>
    private void ConnectSignals()
    {
        if (TeamSkillSystem.Instance == null) return;

        TeamSkillSystem.Instance.OnSkillActivated += (skill) => {
            UpdateSkillDisplay(skill.Type, true);
        };

        TeamSkillSystem.Instance.OnSkillExpired += (skill) => {
            UpdateSkillDisplay(skill.Type, false);
        };
    }

    /// <summary>
    /// 更新技能显示
    /// </summary>
    private void UpdateSkillDisplay(TeamSkillSystem.TeamSkillType type, bool isActive)
    {
        if (_skillLabels.TryGetValue(type, out var label))
        {
            var skill = TeamSkillSystem.Instance.GetAllSkills().Find(s => s.Type == type);
            if (skill != null)
            {
                label.Text = isActive ? "Active" : (skill.CurrentCooldown > 0 ? $"{skill.CurrentCooldown:F1}s" : "Ready");
            }
        }
    }

    /// <summary>
    /// 切换显示
    /// </summary>
    public void Toggle()
    {
        _isVisible = !_isVisible;
        Visible = _isVisible;
    }

    /// <summary>
    /// 显示/隐藏
    /// </summary>
    public void ShowUI()
    {
        _isVisible = true;
        Visible = true;
    }

    public void HideUI()
    {
        _isVisible = false; 
        Visible = false; 
    }

    public bool IsVisible() => _isVisible;

    /// <summary>
    /// 处理输入
    /// </summary>
    public override void _Process(float delta)
    {
        // 更新冷却显示
        var skills = TeamSkillSystem.Instance.GetAllSkills();
        foreach (var skill in skills)
        {
            if (_skillLabels.TryGetValue(skill.Type, out var label))
            {
                if (skill.IsActive)
                {
                    label.Text = "Active";
                }
                else if (skill.CurrentCooldown > 0)
                {
                    label.Text = $"{skill.CurrentCooldown:F1}s";
                }
                else
                {
                    label.Text = "[T-" + GetKeyForSkill(skill.Type) + "]";
                }
            }
        }
    }

    /// <summary>
    /// 获取技能对应的快捷键
    /// </summary>
    private string GetKeyForSkill(TeamSkillSystem.TeamSkillType type)
    {
        switch (type)
        {
            case TeamSkillSystem.TeamSkillType.HealingRain: return "1";
            case TeamSkillSystem.TeamSkillType.ShieldWall: return "2";
            case TeamSkillSystem.TeamSkillType.DamageAura: return "3";
            case TeamSkillSystem.TeamSkillType.DefenseAura: return "4";
            case TeamSkillSystem.TeamSkillType.SpeedAura: return "5";
            case TeamSkillSystem.TeamSkillType.ManaRegen: return "6";
            case TeamSkillSystem.TeamSkillType.CritAura: return "7";
            case TeamSkillSystem.TeamSkillType.LifeSteal: return "8";
            case TeamSkillSystem.TeamSkillType.Invincibility: return "9";
            case TeamSkillSystem.TeamSkillType.Resurrection: return "0";
            case TeamSkillSystem.TeamSkillType.ElementalResist: return "-";
            case TeamSkillSystem.TeamSkillType.ExpBoost: return "=";
            case TeamSkillSystem.TeamSkillType.LootBoost: return "]";
            default: return "?";
        }
    }
}
