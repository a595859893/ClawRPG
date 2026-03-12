using Godot;
using System;
using System.Collections.Generic;

public class GuildDiplomacyUI : Control
{
    // 外交界面
    private GuildDiplomacySystem diplomacySystem;
    private GuildSystem guildSystem;
    
    // UI 组件
    private VBoxContainer mainContainer;
    private TabContainer tabContainer;
    private VBoxContainer alliesContainer;
    private VBoxContainer enemiesContainer;
    private VBoxContainer neutralContainer;
    private Label goldBonusLabel;
    private Label expBonusLabel;
    
    // 预设公会列表（模拟）
    private List<string> sampleGuilds = new List<string>
    {
        "DragonSlayers", "PhoenixGuard", "ShadowKnights", "Silver Legion",
        "GoldenOrder", "CrystalMages", "IronForge", "StormBreakers"
    };
    
    public override void _Ready()
    {
        diplomacySystem = GetNode<GuildDiplomacySystem>("/root/Game/GuildDiplomacySystem");
        guildSystem = GetNode<GuildSystem>("/root/Game/GuildSystem");
        
        SetupUI();
        ConnectSignals();
        
        // 初始刷新
        RefreshDisplay();
    }
    
    private void SetupUI()
    {
        // 主容器
        mainContainer = new VBoxContainer();
        mainContainer.SetAnchor(0, 1, 0, 1);
        mainContainer.MarginLeft = 200;
        mainContainer.MarginTop = 100;
        mainContainer.MarginRight = -200;
        mainContainer.MarginBottom = -100;
        AddChild(mainContainer);
        
        // 标题
        var titleLabel = new Label();
        titleLabel.Text = "  🏛️ Guild Diplomacy";
        titleLabel.AddColorOverride("font_color", new Color(1, 0.9, 0.6));
        titleLabel.Align = Label.AlignEnum.Left;
        mainContainer.AddChild(titleLabel);
        
        // 统计面板
        var statsPanel = new HBoxContainer();
        mainContainer.AddChild(statsPanel);
        
        goldBonusLabel = new Label();
        goldBonusLabel.Text = "Gold Bonus: +0%";
        statsPanel.AddChild(goldBonusLabel);
        
        expBonusLabel = new Label();
        expBonusLabel.Text = "Exp Bonus: +0%";
        statsPanel.AddChild(expBonusLabel);
        
        // 分隔符
        var separator = new HSeparator();
        mainContainer.AddChild(separator);
        
        // Tab 容器
        tabContainer = new TabContainer();
        tabContainer.SetVExpand(true);
        mainContainer.AddChild(tabContainer);
        
        // 盟友标签页
        alliesContainer = new VBoxContainer();
        alliesContainer.Name = "Allies";
        tabContainer.AddChild(alliesContainer);
        
        // 敌对标签页
        enemiesContainer = new VBoxContainer();
        enemiesContainer.Name = "Enemies";
        tabContainer.AddChild(enemiesContainer);
        
        // 中立标签页
        neutralContainer = new VBoxContainer();
        neutralContainer.Name = "Neutral";
        tabContainer.AddChild(neutralContainer);
        
        // 操作面板
        var actionPanel = new HBoxContainer();
        mainContainer.AddChild(actionPanel);
        
        // 添加外交按钮
        var addButton = new Button();
        addButton.Text = "  📜 Propose Treaty";
        addButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        addButton.Connect("pressed", this, nameof(_OnProposeTreaty));
        actionPanel.AddChild(addButton);
        
        // 终止按钮
        var breakButton = new Button();
        breakButton.Text = "  ❌ Break Treaty";
        breakButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        breakButton.Connect("pressed", this, nameof(_OnBreakTreaty));
        actionPanel.AddChild(breakButton);
        
        // 关闭按钮
        var closeButton = new Button();
        closeButton.Text = "  ✖ Close (ESC)";
        closeButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        closeButton.Connect("pressed", this, nameof(_OnClose));
        actionPanel.AddChild(closeButton);
        
        // 按ESC关闭
        AddToGroup("ui");
    }
    
    private void ConnectSignals()
    {
        if (diplomacySystem != null)
        {
            diplomacySystem.Connect(nameof(GuildDiplomacySystem.RelationChanged), this, nameof(_OnRelationChanged));
        }
    }
    
    public void RefreshDisplay()
    {
        // 清除现有内容
        foreach (var child in alliesContainer.GetChildren()) child.QueueFree();
        foreach (var child in enemiesContainer.GetChildren()) child.QueueFree();
        foreach (var child in neutralContainer.GetChildren()) child.QueueFree();
        
        // 获取所有关系
        var relations = diplomacySystem.GetAllRelations();
        
        // 统计加成
        float totalGoldBonus = 0;
        float totalExpBonus = 0;
        
        // 显示盟友
        foreach (var kvp in relations)
        {
            var relation = kvp.Value;
            var card = CreateRelationCard(relation);
            
            if (relation.Type == GuildDiplomacyData.RelationType.Ally)
            {
                alliesContainer.AddChild(card);
                totalGoldBonus += 0.25f;
                totalExpBonus += 0.15f;
            }
            else if (relation.Type == GuildDiplomacyData.RelationType.Enemy)
            {
                enemiesContainer.AddChild(card);
            }
            else if (relation.Type == GuildDiplomacyData.RelationType.NonAggression)
            {
                neutralContainer.AddChild(card);
                totalGoldBonus += 0.10f;
            }
            else
            {
                neutralContainer.AddChild(card);
            }
        }
        
        // 如果没有关系，显示预设公会列表
        if (relations.Count == 0)
        {
            foreach (var guildName in sampleGuilds)
            {
                var card = CreateGuildCard(guildName, GuildDiplomacyData.RelationType.Neutral);
                neutralContainer.AddChild(card);
            }
        }
        
        // 更新统计显示
        goldBonusLabel.Text = $"Gold Bonus: +{(int)(totalGoldBonus * 100)}%";
        expBonusLabel.Text = $"Exp Bonus: +{(int)(totalExpBonus * 100)}%";
    }
    
    private Control CreateRelationCard(GuildDiplomacyData.GuildRelation relation)
    {
        var container = new VBoxContainer();
        
        var label = new Label();
        string typeStr = GetRelationTypeString(relation.Type);
        string trustStr = relation.Trust >= 0 ? $"+{relation.Trust}" : $"{relation.Trust}";
        label.Text = $"  {relation.GuildName} - {typeStr} (Trust: {trustStr})";
        
        Color labelColor = GetRelationColor(relation.Type);
        label.AddColorOverride("font_color", labelColor);
        
        container.AddChild(label);
        
        if (relation.TreatyTurns > 0)
        {
            var turnsLabel = new Label();
            turnsLabel.Text = $"    Treaty: {relation.TreatyTurns} turns remaining";
            container.AddChild(turnsLabel);
        }
        
        return container;
    }
    
    private Control CreateGuildCard(string guildName, GuildDiplomacyData.RelationType type)
    {
        var container = new VBoxContainer();
        
        var label = new Label();
        label.Text = $"  {guildName} - {GetRelationTypeString(type)}";
        label.AddColorOverride("font_color", GetRelationColor(type));
        
        container.AddChild(label);
        
        var button = new Button();
        button.Text = "  Send Proposal";
        button.Connect("pressed", this, nameof(_OnSendProposal), new Godot.Collections.Array { guildName });
        container.AddChild(button);
        
        return container;
    }
    
    private string GetRelationTypeString(GuildDiplomacyData.RelationType type)
    {
        switch (type)
        {
            case GuildDiplomacyData.RelationType.Ally: return "🤝 Ally";
            case GuildDiplomacyData.RelationType.Enemy: return "⚔️ Enemy";
            case GuildDiplomacyData.RelationType.NonAggression: return "🤐 Non-Aggression";
            default: return "⚪ Neutral";
        }
    }
    
    private Color GetRelationColor(GuildDiplomacyData.RelationType type)
    {
        switch (type)
        {
            case GuildDiplomacyData.RelationType.Ally: return new Color(0.4, 1, 0.4);
            case GuildDiplomacyData.RelationType.Enemy: return new Color(1, 0.3, 0.3);
            case GuildDiplomacyData.RelationType.NonAggression: return new Color(0.4, 0.7, 1);
            default: return new Color(0.7, 0.7, 0.7);
        }
    }
    
    private void _OnRelationChanged(string guildId, int newType)
    {
        RefreshDisplay();
    }
    
    private void _OnProposeTreaty()
    {
        // 随机选择一个未建立关系的公会建立联盟
        var relations = diplomacySystem.GetAllRelations();
        
        foreach (var guildName in sampleGuilds)
        {
            if (!relations.ContainsKey(guildName))
            {
                diplomacySystem.SetRelation(guildName, GuildDiplomacyData.RelationType.Ally, 10);
                RefreshDisplay();
                return;
            }
        }
    }
    
    private void _OnSendProposal(string guildName)
    {
        diplomacySystem.SetRelation(guildName, GuildDiplomacyData.RelationType.Ally, 10);
        RefreshDisplay();
    }
    
    private void _OnBreakTreaty()
    {
        var allies = diplomacySystem.GetAllies();
        if (allies.Count > 0)
        {
            diplomacySystem.BreakTreaty(allies[0]);
            RefreshDisplay();
        }
    }
    
    private void _OnClose()
    {
        Visible = false;
    }
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Scancode == KeyList.Escape)
        {
            if (Visible)
            {
                _OnClose();
            }
        }
    }
    
    public void Toggle()
    {
        Visible = !Visible;
        if (Visible)
        {
            RefreshDisplay();
        }
    }
}
