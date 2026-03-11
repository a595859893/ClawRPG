using Godot;
using System;
using System.Collections.Generic;

public class ArenaTournamentUI : Control
{
    private Button _closeButton;
    private TabContainer _tabContainer;
    private VBoxContainer _tournamentListContainer;
    private VBoxContainer _myTournamentsContainer;
    private VBoxContainer _statisticsContainer;
    private Label _titleLabel;
    
    private List<ArenaTournamentData.Tournament> _availableTournaments = new List<ArenaTournamentData.Tournament>();
    private ArenaTournamentData.Tournament _selectedTournament;
    
    public override void _Ready()
    {
        SetAnchor(AnchorPreset.FullRect);
        Modulate = new Color(1, 1, 1, 0);
        
        CreateUI();
        LoadTournaments();
        
        // Animate in
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 1f, 0.3f).SetTrans(Tween.TransitionType.Back).SetEasing(Tween.EasingFunction.EaseOut);
    }
    
    private void CreateUI()
    {
        // Background panel
        var bgPanel = new PanelContainer
        {
            SetAnchor(AnchorPreset.FullRect,
                new Vector2(0.1f, 0.1f),
                new Vector2(0.9f, 0.9f))
        };
        bgPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
        AddChild(bgPanel);
        
        // Main VBox
        var mainVBox = new VBoxContainer
        {
            SetAnchor(AnchorPreset.FullRect,
                new Vector2(0.05f, 0.05f),
                new Vector2(0.95f, 0.95f))
        };
        bgPanel.AddChild(mainVBox);
        
        // Title bar
        var titleBar = new HBoxContainer();
        mainVBox.AddChild(titleBar);
        
        _titleLabel = new Label
        {
            Text = "竞技场锦标赛",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        titleBar.AddChild(_titleLabel);
        
        titleBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        _closeButton = new Button
        {
            Text = "✕",
            CustomMinimumSize = new Vector2(40, 40)
        };
        _closeButton.Pressed += () => CloseUI();
        titleBar.AddChild(_closeButton);
        
        // Tab container
        _tabContainer = new TabContainer
        {
            SizeFlagsVertical = Control.SizeFlags.Expand
        };
        _tabContainer.AddThemeStyleboxOverride("panel", CreateTabPanelStyle());
        mainVBox.AddChild(_tabContainer);
        
        // Tournament list tab
        var tournamentListTab = new Control();
        tournamentListTab.Name = "锦标赛列表";
        _tabContainer.AddChild(tournamentListTab);
        
        var scroll1 = new ScrollContainer
        {
            SetAnchor(AnchorPreset.FullRect,
                new Vector2(0.05f, 0.05f),
                new Vector2(0.95f, 0.95f))
        };
        tournamentListTab.AddChild(scroll1);
        
        _tournamentListContainer = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.Expand
        };
        scroll1.AddChild(_tournamentListContainer);
        
        // My tournaments tab
        var myTournamentsTab = new Control();
        myTournamentsTab.Name = "我的报名";
        _tabContainer.AddChild(myTournamentsTab);
        
        var scroll2 = new ScrollContainer
        {
            SetAnchor(AnchorPreset.FullRect,
                new Vector2(0.05f, 0.05f),
                new Vector2(0.95f, 0.95f))
        };
        myTournamentsTab.AddChild(scroll2);
        
        _myTournamentsContainer = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.Expand
        };
        scroll2.AddChild(_myTournamentsContainer);
        
        // Statistics tab
        var statisticsTab = new Control();
        statisticsTab.Name = "我的战绩";
        _tabContainer.AddChild(statisticsTab);
        
        var scroll3 = new ScrollContainer
        {
            SetAnchor(AnchorPreset.FullRect,
                new Vector2(0.05f, 0.05f),
                new Vector2(0.95f, 0.95f))
        };
        statisticsTab.AddChild(scroll3);
        
        _statisticsContainer = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.Expand
        };
        scroll3.AddChild(_statisticsContainer);
    }
    
    private void LoadTournaments()
    {
        // Clear existing items
        foreach (var child in _tournamentListContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // Load available tournaments
        _availableTournaments = ArenaTournamentSystem.Instance.GetAllTournaments();
        
        foreach (var tournament in _availableTournaments)
        {
            var item = CreateTournamentItem(tournament);
            _tournamentListContainer.AddChild(item);
        }
        
        // Load my tournaments
        RefreshMyTournaments();
        
        // Load statistics
        RefreshStatistics();
    }
    
    private Control CreateTournamentItem(ArenaTournamentData.Tournament tournament)
    {
        var container = new PanelContainer
        {
            CustomMinimumSize = new Vector2(0, 80),
            Margin = new Margin(0, 0, 0, 5)
        };
        container.AddThemeStyleboxOverride("panel", CreateItemPanelStyle());
        
        var hbox = new HBoxContainer();
        container.AddChild(hbox);
        
        // Tournament info
        var infoVBox = new VBoxContainer;
        hbox.AddChild(infoVBox);
        
        var nameLabel = new Label
        {
            Text = tournament.Name,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        nameLabel.AddThemeFontSizeOverride("font_size", 18);
        infoVBox.AddChild(nameLabel);
        
        var descLabel = new Label
        {
            Text = $"{ArenaTournamentDatabase.Instance.GetTournamentTypeName(tournament.Type)} | {tournament.RegisteredPlayerIds.Count}/{tournament.MaxParticipants}人 | 报名费: {tournament.EntryFee}金",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        descLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        infoVBox.AddChild(descLabel);
        
        var prizeLabel = new Label
        {
            Text = $"奖池: {tournament.PrizePool}金",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        prizeLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f)); // Gold color
        infoVBox.AddChild(prizeLabel);
        
        hbox.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        // Status and register button
        var statusVBox = new VBoxContainer;
        hbox.AddChild(statusVBox);
        
        var stateLabel = new Label
        {
            Text = ArenaTournamentDatabase.Instance.GetStateName(tournament.State),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        
        // Color based on state
        switch (tournament.State)
        {
            case ArenaTournamentData.TournamentState.Registration:
                stateLabel.AddThemeColorOverride("font_color", new Color(0.2f, 0.8f, 0.2f));
                break;
            case ArenaTournamentData.TournamentState.InProgress:
                stateLabel.AddThemeColorOverride("font_color", new Color(1f, 0.6f, 0.2f));
                break;
            case ArenaTournamentData.TournamentState.Completed:
                stateLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
                break;
        }
        statusVBox.AddChild(stateLabel);
        
        if (tournament.State == ArenaTournamentData.TournamentState.Registration)
        {
            var registerButton = new Button
            {
                Text = "报名",
                CustomMinimumSize = new Vector2(80, 30)
            };
            registerButton.Pressed += () => TryRegister(tournament);
            statusVBox.AddChild(registerButton);
        }
        
        return container;
    }
    
    private void TryRegister(ArenaTournamentData.Tournament tournament)
    {
        if (Player.Main == null) return;
        
        var canRegister = ArenaTournamentSystem.Instance.CanRegister(
            tournament.Id,
            Player.Main.PlayerId,
            Player.Main.Level,
            Player.Main.Gold
        );
        
        if (canRegister)
        {
            int gold = Player.Main.Gold;
            if (ArenaTournamentSystem.Instance.RegisterPlayer(tournament.Id, Player.Main.PlayerId, ref gold))
            {
                Player.Main.Gold = gold;
                LoadTournaments();
                GD.Print($"[UI] Registered for tournament: {tournament.Name}");
            }
        }
        else
        {
            GD.Print("[UI] Cannot register for tournament");
        }
    }
    
    private void RefreshMyTournaments()
    {
        foreach (var child in _myTournamentsContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        if (Player.Main == null) return;
        
        var playerData = ArenaTournamentSystem.Instance.GetPlayerData(Player.Main.PlayerId);
        
        if (playerData.RegisteredTournamentIds.Count == 0)
        {
            var emptyLabel = new Label
            {
                Text = "还没有报名任何锦标赛",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            emptyLabel.AddThemeFontSizeOverride("font_size", 18);
            _myTournamentsContainer.AddChild(emptyLabel);
            return;
        }
        
        foreach (var tournamentId in playerData.RegisteredTournamentIds)
        {
            var tournament = ArenaTournamentSystem.Instance.GetTournament(tournamentId);
            if (tournament == null) continue;
            
            var item = CreateTournamentItem(tournament);
            _myTournamentsContainer.AddChild(item);
        }
    }
    
    private void RefreshStatistics()
    {
        foreach (var child in _statisticsContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        if (Player.Main == null) return;
        
        var playerData = ArenaTournamentSystem.Instance.GetPlayerData(Player.Main.PlayerId);
        
        // Statistics header
        var statsTitle = new Label
        {
            Text = "个人战绩",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        statsTitle.AddThemeFontSizeOverride("font_size", 20);
        _statisticsContainer.AddChild(statsTitle);
        
        _statisticsContainer.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });
        
        // Stats grid
        var statsGrid = new GridContainer
        {
            Columns = 2
        };
        _statisticsContainer.AddChild(statsGrid);
        
        AddStatRow(statsGrid, "获胜场次", playerData.Wins.ToString());
        AddStatRow(statsGrid, "失败场次", playerData.Losses.ToString());
        AddStatRow(statsGrid, "冠军次数", playerData.Championships.ToString());
        AddStatRow(statsGrid, "总收益", $"{playerData.TotalEarnings}金");
    }
    
    private void AddStatRow(GridContainer container, string label, string value)
    {
        var labelNode = new Label
        {
            Text = label,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        container.AddChild(labelNode);
        
        var valueNode = new Label
        {
            Text = value,
            HorizontalAlignment = HorizontalAlignment.Right,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        valueNode.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f));
        container.AddChild(valueNode);
    }
    
    private StyleBoxFlat CreatePanelStyle()
    {
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        style.BorderWidthLeft = 2;
        style.BorderWidthRight = 2;
        style.BorderWidthTop = 2;
        style.BorderWidthBottom = 2;
        style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        style.CornerRadiusTopLeft = 10;
        style.CornerRadiusTopRight = 10;
        style.CornerRadiusBottomLeft = 10;
        style.CornerRadiusBottomRight = 10;
        return style;
    }
    
    private StyleBoxFlat CreateTabPanelStyle()
    {
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.08f, 0.08f, 0.12f);
        style.CornerRadiusTopLeft = 5;
        style.CornerRadiusTopRight = 5;
        return style;
    }
    
    private StyleBoxFlat CreateItemPanelStyle()
    {
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.15f, 0.15f, 0.2f);
        style.BorderWidthLeft = 1;
        style.BorderWidthRight = 1;
        style.BorderWidthTop = 1;
        style.BorderWidthBottom = 1;
        style.BorderColor = new Color(0.25f, 0.25f, 0.35f);
        style.CornerRadiusTopLeft = 5;
        style.CornerRadiusTopRight = 5;
        style.CornerRadiusBottomLeft = 5;
        style.CornerRadiusBottomRight = 5;
        return style;
    }
    
    private void CloseUI()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0f, 0.2f).SetTrans(Tween.TransitionType.Back).SetEasing(Tween.EasingFunction.EaseIn);
        tween.TweenCallback(QueueFree);
    }
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            CloseUI();
        }
    }
}
