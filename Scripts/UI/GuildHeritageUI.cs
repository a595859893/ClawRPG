using System;
using System.Collections.Generic;
using Godot;

public class GuildHeritageUI : Control
{
    private Label _titleLabel;
    private Label _guildNameLabel;
    private Label _pointsLabel;
    private VBoxContainer _heritageList;
    private VBoxContainer _contributorsList;
    private Button _refreshButton;
    private string _currentGuildId;
    
    public override void _Ready()
    {
        SetupUI();
    }

    private void SetupUI()
    {
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchorAndMargin(Control.LayoutPreset.FullRect, 0.05f);
        AddChild(mainContainer);

        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "🏛️ Guild Heritage System";
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 28);
        mainContainer.AddChild(_titleLabel);

        // Guild Info
        var infoContainer = new HBoxContainer();
        mainContainer.AddChild(infoContainer);

        _guildNameLabel = new Label();
        _guildNameLabel.Text = "Guild: None";
        _guildNameLabel.AddThemeFontSizeOverride("font_size", 20);
        infoContainer.AddChild(_guildNameLabel);

        var spacer = new Control();
        spacer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        infoContainer.AddChild(spacer);

        _pointsLabel = new Label();
        _pointsLabel.Text = "Heritage Points: 0";
        _pointsLabel.AddThemeFontSizeOverride("font_size", 20);
        infoContainer.AddChild(_pointsLabel);

        // Tab container for different views
        var tabContainer = new TabContainer();
        tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(tabContainer);

        // Heritages Tab
        var heritagesTab = new Control();
        tabContainer.AddChild(heritagesTab);
        tabContainer.SetTabTitle(0, "📜 Heritages");

        _heritageList = new VBoxContainer();
        _heritageList.SetAnchorAndMargin(Control.LayoutPreset.FullRect, 0.05f);
        heritagesTab.AddChild(_heritageList);

        // Contributors Tab
        var contributorsTab = new Control();
        tabContainer.AddChild(contributorsTab);
        tabContainer.SetTabTitle(1, "👥 Contributors");

        _contributorsList = new VBoxContainer();
        _contributorsList.SetAnchorAndMargin(Control.LayoutPreset.FullRect, 0.05f);
        contributorsTab.AddChild(_contributorsList);

        // Bottom buttons
        var buttonContainer = new HBoxContainer();
        mainContainer.AddChild(buttonContainer);

        _refreshButton = new Button();
        _refreshButton.Text = "🔄 Refresh";
        _refreshButton.Pressed += OnRefreshPressed;
        buttonContainer.AddChild(_refreshButton);

        var spacer2 = new Control();
        spacer2.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        buttonContainer.AddChild(spacer2);

        var closeButton = new Button();
        closeButton.Text = "✖ Close";
        closeButton.Pressed += OnClosePressed;
        buttonContainer.AddChild(closeButton);

        // Load initial data
        RefreshUI();
    }

    public void SetGuild(string guildId, string guildName)
    {
        _currentGuildId = guildId;
        _guildNameLabel.Text = $"Guild: {guildName}";
        RefreshUI();
    }

    private void RefreshUI()
    {
        // Clear existing
        foreach (var child in _heritageList.GetChildren())
        {
            child.QueueFree();
        }
        foreach (var child in _contributorsList.GetChildren())
        {
            child.QueueFree();
        }

        if (string.IsNullOrEmpty(_currentGuildId))
        {
            var noGuildLabel = new Label();
            noGuildLabel.Text = "No guild selected";
            noGuildLabel.Align = Label.AlignEnum.Center;
            _heritageList.AddChild(noGuildLabel);
            return;
        }

        var system = GuildHeritageSystem.Instance;
        
        // Update points
        if (system.GuildHeritages.ContainsKey(_currentGuildId))
        {
            var guild = system.GuildHeritages[_currentGuildId];
            _pointsLabel.Text = $"Heritage Points: {guild.TotalHeritagePoints}";

            // Show heritage types
            var types = Enum.GetValues(typeof(HeritageType));
            foreach (HeritageType type in types)
            {
                AddHeritageTypeRow(type, guild);
            }

            // Show contributors
            var contributors = system.GetTopContributors(_currentGuildId);
            int rank = 1;
            foreach (var kvp in contributors)
            {
                AddContributorRow(rank++, kvp.Key, kvp.Value);
            }
        }
        else
        {
            _pointsLabel.Text = "Heritage Points: 0 (Guild not registered)";
            
            // Show how to create
            var createLabel = new Label();
            createLabel.Text = "Your guild is not registered for Heritage System.\nStart contributing to unlock herITage bonuses!";
            createLabel.Align = Label.AlignEnum.Center;
            _heritageList.AddChild(createLabel);
        }
    }

    private void AddHeritageTypeRow(HeritageType type, GuildHeritage guild)
    {
        var container = new PanelContainer();
        container.SetMarginMargin(Margin.Top, 5);
        container.SetMarginMargin(Margin.Bottom, 5);
        _heritageList.AddChild(container);

        var vbox = new VBoxContainer();
        container.AddChild(vbox);

        var typeLabel = new Label();
        typeLabel.Text = $"◆ {GetHeritageTypeName(type)}";
        typeLabel.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(typeLabel);

        // Current tier
        var currentTier = GetCurrentTier(guild, type);
        var tierLabel = new Label();
        tierLabel.Text = $"  Current: {GetTierName(currentTier)}";
        tierLabel.Modulate = GetTierColor(currentTier);
        vbox.AddChild(tierLabel);

        // Next upgrade info
        var nextTier = GetNextTier(currentTier);
        if (nextTier != HeritageTier.Diamond)
        {
            var db = GuildHeritageDatabase.Instance;
            var nextId = db.TierMapping[nextTier][type];
            var nextHeritage = db.GetHeritage(nextId);
            
            if (nextHeritage != null)
            {
                var canUpgrade = guild.TotalHeritagePoints >= nextHeritage.RequiredPoints;
                var upgradeLabel = new Label();
                upgradeLabel.Text = canUpgrade ? $"  ✓ Can upgrade to {GetTierName(nextTier)} ({nextHeritage.RequiredPoints} pts)" : 
                    $"  🔒 Upgrade to {GetTierName(nextTier)}: {nextHeritage.RequiredPoints} pts";
                upgradeLabel.Modulate = canUpgrade ? new Color(0, 1, 0) : new Color(0.7f, 0.7f, 0.7f);
                vbox.AddChild(upgradeLabel);
            }
        }
        else
        {
            var maxLabel = new Label();
            maxLabel.Text = "  ⭐ MAX LEVEL";
            maxLabel.Modulate = new Color(1, 0.84f, 0);
            vbox.AddChild(maxLabel);
        }

        // Description of current bonus
        var bonusLabel = new Label();
        bonusLabel.Text = $"  Bonus: {GetCurrentBonusDescription(guild, type)}";
        bonusLabel.Modulate = new Color(0.8f, 0.8f, 0.8f);
        vbox.AddChild(bonusLabel);
    }

    private HeritageTier GetCurrentTier(GuildHeritage guild, HeritageType type)
    {
        var db = GuildHeritageDatabase.Instance;
        
        var tierOrder = new[] { HeritageTier.Diamond, HeritageTier.Platinum, HeritageTier.Gold, HeritageTier.Silver, HeritageTier.Bronze };
        
        foreach (var tier in tierOrder)
        {
            if (db.TierMapping.ContainsKey(tier) && db.TierMapping[tier].ContainsKey(type))
            {
                var id = db.TierMapping[tier][type];
                if (guild.UnlockedHeritages.ContainsKey(id))
                    return tier;
            }
        }
        
        return HeritageTier.None;
    }

    private HeritageTier GetNextTier(HeritageTier current)
    {
        var tierOrder = new[] { HeritageTier.None, HeritageTier.Bronze, HeritageTier.Silver, HeritageTier.Gold, HeritageTier.Platinum, HeritageTier.Diamond };
        var index = Array.IndexOf(tierOrder, current);
        if (index < tierOrder.Length - 1)
            return tierOrder[index + 1];
        return HeritageTier.Diamond;
    }

    private string GetCurrentBonusDescription(GuildHeritage guild, HeritageType type)
    {
        var db = GuildHeritageDatabase.Instance;
        var currentId = GetCurrentHeritageId(guild, type);
        
        if (currentId != null)
        {
            var heritage = db.GetHeritage(currentId);
            return heritage.Description;
        }
        
        return "No bonus active";
    }

    private string GetCurrentHeritageId(GuildHeritage guild, HeritageType type)
    {
        var db = GuildHeritageDatabase.Instance;
        
        if (db.HeritagesByType.ContainsKey(type))
        {
            foreach (var id in db.HeritagesByType[type])
            {
                if (guild.UnlockedHeritages.ContainsKey(id))
                    return id;
            }
        }
        
        return null;
    }

    private void AddContributorRow(int rank, string playerId, int points)
    {
        var container = new PanelContainer();
        container.SetMarginMargin(Margin.Top, 3);
        container.SetMarginMargin(Margin.Bottom, 3);
        _contributorsList.AddChild(container);

        var hbox = new HBoxContainer();
        container.AddChild(hbox);

        var rankLabel = new Label();
        rankLabel.Text = $"#{rank}";
        rankLabel.CustomMinimumSize = new Vector2(40, 0);
        rankLabel.Modulate = GetRankColor(rank);
        hbox.AddChild(rankLabel);

        var nameLabel = new Label();
        nameLabel.Text = playerId;
        nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hbox.AddChild(nameLabel);

        var pointsLabel = new Label();
        pointsLabel.Text = $"{points} pts";
        pointsLabel.Modulate = new Color(1, 0.84f, 0);
        hbox.AddChild(pointsLabel);
    }

    private Color GetRankColor(int rank)
    {
        return rank switch
        {
            1 => new Color(1, 0.84f, 0),    // Gold
            2 => new Color(0.75f, 0.75f, 0.75f), // Silver
            3 => new Color(0.8f, 0.5f, 0.2f),   // Bronze
            _ => Colors.White
        };
    }

    private Color GetTierColor(HeritageTier tier)
    {
        return tier switch
        {
            HeritageTier.Diamond => new Color(0.7f, 0.9f, 1f),
            HeritageTier.Platinum => new Color(0.9f, 0.9f, 0.95f),
            HeritageTier.Gold => new Color(1, 0.84f, 0),
            HeritageTier.Silver => new Color(0.75f, 0.75f, 0.75f),
            HeritageTier.Bronze => new Color(0.8f, 0.5f, 0.2f),
            _ => new Color(0.5f, 0.5f, 0.5f)
        };
    }

    private string GetHeritageTypeName(HeritageType type)
    {
        return type switch
        {
            HeritageType.BattleCry => "⚔️ Battle Cry",
            HeritageType.ArcaneSecrets => "🔮 Arcane Secrets",
            HeritageType.CraftingMastery => "🔨 Crafting Mastery",
            HeritageType.TradeProsperity => "💰 Trade Prosperity",
            HeritageType.DefenseFortification => "🛡️ Defense Fortification",
            HeritageType.Exploration => "🗺️ Exploration",
            HeritageType.Diplomacy => "🤝 Diplomacy",
            HeritageType.LegendaryHeroes => "🦸 Legendary Heroes",
            _ => type.ToString()
        };
    }

    private string GetTierName(HeritageTier tier)
    {
        return tier switch
        {
            HeritageTier.Diamond => "💎 Diamond",
            HeritageTier.Platinum => "💠 Platinum",
            HeritageTier.Gold => "🥇 Gold",
            HeritageTier.Silver => "🥈 Silver",
            HeritageTier.Bronze => "🥉 Bronze",
            _ => "None"
        };
    }

    private void OnRefreshPressed()
    {
        RefreshUI();
    }

    private void OnClosePressed()
    {
        QueueFree();
    }

    public static GuildHeritageUI ShowForGuild(string guildId, string guildName)
    {
        var ui = new GuildHeritageUI();
        ui.SetGuild(guildId, guildName);
        return ui;
    }
}
