using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.FateWeaving;

public class FateWeavingUI : Control {
    private FateWeavingSystem _system;
    private FateWeavingDatabase _database;
    
    private TabContainer _tabContainer;
    private Label _titleLabel;
    private Label _weaveLevelLabel;
    private ProgressBar _levelProgress;
    private Label _dominantPathLabel;
    
    private VBoxContainer _choicesContainer;
    private VBoxContainer _pathsContainer;
    private VBoxContainer _statsContainer;
    
    private FateChoice _currentChoice;
    private TextureRect _choiceBackground;
    private Label _choiceTitle;
    private Label _choiceDescription;
    private Button _confirmButton;
    private Label _pathInfluenceLabel;
    
    public bool IsVisible { get; private set; }
    
    public override void _Ready() {
        _system = FateWeavingSystem.Instance;
        _database = FateWeavingDatabase.Instance;
        
        SetupUI();
        ConnectSignals();
        RefreshUI();
    }
    
    private void SetupUI() {
        // Main Panel
        var mainPanel = new Panel {
            AnchorRight = 0.8f,
            AnchorBottom = 0.8f,
            AnchorLeft = 0.1f,
            AnchorTop = 0.1f,
            SelfModulate = new Color(0.1f, 0.1f, 0.15f, 0.95f)
        };
        AddChild(mainPanel);
        
        // Title Bar
        var titleBar = new HBoxContainer {
            AnchorRight = 1f,
            CustomMinimumHeight = 60
        };
        mainPanel.AddChild(titleBar);
        
        _titleLabel = new Label {
            Text = "✧ Fate Weaving ✧",
            Align = Label.AlignEnum.Center,
            Valign = Label.VAlign.Center,
            CustomFonts = { "font", GetThemeFont("title", "Label") }
        };
        _titleLabel.SetAnchorsPreset(Control.LayoutPreset.Center);
        titleBar.AddChild(_titleLabel);
        
        var closeButton = new Button {
            Text = "✕",
            CustomMinimumSize = new Vector2(40, 40),
            AnchorLeft = 1f,
            AnchorRight = 1f,
            MarginLeft = -40
        };
        closeButton.Connect("pressed", this, nameof(Hide));
        titleBar.AddChild(closeButton);
        
        // Level Info
        var levelPanel = new HBoxContainer {
            AnchorTop = 60f,
            AnchorRight = 1f,
            CustomMinimumHeight = 50,
            Margin = new Margin(20, 70, 20, 0)
        };
        mainPanel.AddChild(levelPanel);
        
        _weaveLevelLabel = new Label {
            Text = "Weave Level: 1",
            CustomFonts = { "font", GetThemeFont("bold", "Label") }
        };
        levelPanel.AddChild(_weaveLevelLabel);
        
        levelPanel.AddChild(new Control { CustomMinimumWidth = 20 });
        
        _levelProgress = new ProgressBar {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            CustomMinimumHeight = 20
        };
        levelPanel.AddChild(_levelProgress);
        
        levelPanel.AddChild(new Control { CustomMinimumWidth = 20 });
        
        _dominantPathLabel = new Label {
            Text = "Path: Hero",
            CustomColors = new Label.LabelSettings { FontColor = new Color(1f, 0.85f, 0.3f) }
        };
        levelPanel.AddChild(_dominantPathLabel);
        
        // Tab Container
        _tabContainer = new TabContainer {
            AnchorTop = 130f,
            AnchorRight = 1f,
            AnchorBottom = 1f,
            Margin = new Margin(20, 140, 20, 20)
        };
        mainPanel.AddChild(_tabContainer);
        
        // Choices Tab
        var choicesTab = new Control { Name = "Choices" };
        _tabContainer.AddChild(choicesTab);
        SetupChoicesTab(choicesTab);
        
        // Paths Tab
        var pathsTab = new Control { Name = "Paths" };
        _tabContainer.AddChild(pathsTab);
        SetupPathsTab(pathsTab);
        
        // Statistics Tab
        var statsTab = new Control { Name = "Statistics" };
        _tabContainer.AddChild(statsTab);
        SetupStatsTab(statsTab);
        
        // Choice Modal
        SetupChoiceModal(mainPanel);
    }
    
    private void SetupChoicesTab(Control parent) {
        var scroll = new ScrollContainer {
            AnchorRight = 1f,
            AnchorBottom = 1f
        };
        parent.AddChild(scroll);
        
        _choicesContainer = new VBoxContainer {
            CustomMinimumSize = new Vector2(0, 400)
        };
        scroll.AddChild(_choicesContainer);
    }
    
    private void SetupPathsTab(Control parent) {
        var scroll = new ScrollContainer {
            AnchorRight = 1f,
            AnchorBottom = 1f
        };
        parent.AddChild(scroll);
        
        _pathsContainer = new VBoxContainer {
            CustomMinimumSize = new Vector2(0, 400)
        };
        scroll.AddChild(_pathsContainer);
    }
    
    private void SetupStatsTab(Control parent) {
        var scroll = new ScrollContainer {
            AnchorRight = 1f,
            AnchorBottom = 1f
        };
        parent.AddChild(scroll);
        
        _statsContainer = new VBoxContainer {
            CustomMinimumSize = new Vector2(0, 400)
        };
        scroll.AddChild(_statsContainer);
    }
    
    private void SetupChoiceModal(Panel parent) {
        _choiceBackground = new TextureRect {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            SelfModulate = new Color(0f, 0f, 0f, 0.8f),
            Visible = false
        };
        parent.AddChild(_choiceBackground);
        
        var modalPanel = new PanelContainer {
            AnchorLeft = 0.2f,
            AnchorTop = 0.2f,
            AnchorRight = 0.8f,
            AnchorBottom = 0.8f,
            SelfModulate = new Color(0.15f, 0.15f, 0.2f, 0.98f)
        };
        _choiceBackground.AddChild(modalPanel);
        
        var modalVBox = new VBoxContainer {
            CustomMinimumSize = new Vector2(400, 300),
            Margin = new Margin(30, 30, 30, 30)
        };
        modalPanel.AddChild(modalVBox);
        
        _choiceTitle = new Label {
            Text = "Choice Title",
            CustomFonts = { "font", GetThemeFont("title", "Label") },
            Align = Label.AlignEnum.Center
        };
        modalVBox.AddChild(_choiceTitle);
        
        var separator = new HSeparator { CustomMinimumHeight = 10 };
        modalVBox.AddChild(separator);
        
        _choiceDescription = new Label {
            Text = "Choice description goes here...",
            Autowrap = true,
            CustomMinimumSize = new Vector2(0, 100)
        };
        modalVBox.AddChild(_choiceDescription);
        
        _pathInfluenceLabel = new Label {
            Text = "",
            CustomColors = new Label.LabelSettings { FontColor = new Color(0.7f, 0.7f, 0.8f) }
        };
        modalVBox.AddChild(_pathInfluenceLabel);
        
        modalVBox.AddChild(new Control { CustomMinimumHeight = 20 });
        
        var buttonHBox = new HBoxContainer {
            Alignment = BoxContainer.AlignMode.Center
        };
        modalVBox.AddChild(buttonHBox);
        
        var cancelButton = new Button {
            Text = "Cancel",
            CustomMinimumSize = new Vector2(100, 40)
        };
        cancelButton.Connect("pressed", this, nameof(CloseChoiceModal));
        buttonHBox.AddChild(cancelButton);
        
        buttonHBox.AddChild(new Control { CustomMinimumWidth = 20 });
        
        _confirmButton = new Button {
            Text = "Weave Fate",
            CustomMinimumSize = new Vector2(150, 40),
            CustomColors = new Button.ButtonTextures { FontColor = new Color(1f, 0.85f, 0.3f) }
        };
        _confirmButton.Connect("pressed", this, nameof(ConfirmChoice));
        buttonHBox.AddChild(_confirmButton);
    }
    
    private Font GetThemeFont(string type, string themeType) {
        var label = new Label();
        if (type == "title") {
            label.AddThemeFontSize("font_size", 24);
        } else if (type == "bold") {
            label.AddThemeFontSize("font_size", 18);
        }
        return label.GetThemeDefaultFont();
    }
    
    private void ConnectSignals() {
        _system.OnChoiceMade += OnChoiceMade;
        _system.OnWeaveLevelChanged += OnWeaveLevelChanged;
    }
    
    private void RefreshUI() {
        // Update level
        _weaveLevelLabel.Text = $"Weave Level: {_system.GetWeaveLevel()}";
        
        var expProgress = _system.GetExperienceProgress();
        _levelProgress.Value = expProgress * 100;
        
        var dominantPath = _system.GetDominantPathData();
        if (dominantPath != null) {
            _dominantPathLabel.Text = $"Path: {dominantPath.Name}";
        }
        
        // Refresh choices list
        RefreshChoicesList();
        
        // Refresh paths list
        RefreshPathsList();
        
        // Refresh statistics
        RefreshStatsList();
    }
    
    private void RefreshChoicesList() {
        foreach (var child in _choicesContainer.GetChildren()) {
            child.QueueFree();
        }
        
        var choices = _system.GetAvailableChoices();
        foreach (var choice in choices) {
            if (_system.HasChosen(choice.Id)) continue;
            
            var choicePanel = CreateChoicePanel(choice);
            _choicesContainer.AddChild(choicePanel);
        }
        
        if (_choicesContainer.GetChildCount() == 0) {
            var noChoices = new Label {
                Text = "No new fate choices available.\nLevel up to unlock more!",
                Align = Label.AlignEnum.Center,
                CustomColors = new Label.LabelSettings { FontColor = new Color(0.6f, 0.6f, 0.7f) }
            };
            _choicesContainer.AddChild(noChoices);
        }
    }
    
    private Control CreateChoicePanel(FateChoice choice) {
        var panel = new PanelContainer {
            CustomMinimumSize = new Vector2(0, 80),
            Margin = new Margin(0, 0, 0, 10)
        };
        
        var hbox = new HBoxContainer {
            Margin = new Margin(10, 10, 10, 10)
        };
        panel.AddChild(hbox);
        
        var iconLabel = new Label {
            Text = GetChoiceTypeIcon(choice.ChoiceType),
            CustomMinimumSize = new Vector2(40, 0)
        };
        hbox.AddChild(iconLabel);
        
        var textVBox = new VBoxContainer {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        hbox.AddChild(textVBox);
        
        var titleLabel = new Label {
            Text = choice.Title,
            CustomFonts = { "font", GetThemeFont("bold", "Label") }
        };
        textVBox.AddChild(titleLabel);
        
        var descLabel = new Label {
            Text = choice.Description,
            Autowrap = true,
            CustomColors = new Label.LabelSettings { FontColor = new Color(0.7f, 0.7f, 0.8f) }
        };
        textVBox.AddChild(descLabel);
        
        var selectButton = new Button {
            Text = "Select",
            CustomMinimumSize = new Vector2(80, 30)
        };
        selectButton.Connect("pressed", this, nameof(OnSelectChoice), new Godot.Collections.Array { choice });
        hbox.AddChild(selectButton);
        
        if (choice.IsSecret) {
            var secretIcon = new Label {
                Text = "🔮",
                CustomColors = new Label.LabelSettings { FontColor = new Color(0.8f, 0.6f, 1f) }
            };
            hbox.AddChild(secretIcon);
        }
        
        return panel;
    }
    
    private string GetChoiceTypeIcon(FateChoiceType type) {
        switch (type) {
            case FateChoiceType.Moral: return "⚖️";
            case FateChoiceType.Combat: return "⚔️";
            case FateChoiceType.Social: return "🗣️";
            case FateChoiceType.Economic: return "💰";
            case FateChoiceType.Exploration: return "🗺️";
            case FateChoiceType.Mystery: return "🔮";
            default: return "✧";
        }
    }
    
    private void RefreshPathsList() {
        foreach (var child in _pathsContainer.GetChildren()) {
            child.QueueFree();
        }
        
        var affinities = _system.GetAllPathAffinities();
        
        foreach (var pathData in _database.Paths) {
            float affinity = affinities.ContainsKey(pathData.Type) ? affinities[pathData.Type] : 0f;
            
            var pathPanel = new PanelContainer {
                CustomMinimumSize = new Vector2(0, 70),
                Margin = new Margin(0, 0, 0, 8)
            };
            _pathsContainer.AddChild(pathPanel);
            
            var hbox = new HBoxContainer {
                Margin = new Margin(10, 8, 10, 8)
            };
            pathPanel.AddChild(hbox);
            
            var pathIcon = new Label {
                Text = GetPathIcon(pathData.Type),
                CustomMinimumSize = new Vector2(30, 0)
            };
            hbox.AddChild(pathIcon);
            
            var textVBox = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            hbox.AddChild(textVBox);
            
            var nameLabel = new Label {
                Text = pathData.Name,
                CustomFonts = { "font", GetThemeFont("bold", "Label") }
            };
            textVBox.AddChild(nameLabel);
            
            var descLabel = new Label {
                Text = pathData.Description,
                Autowrap = true,
                CustomColors = new Label.LabelSettings { FontColor = new Color(0.7f, 0.7f, 0.8f) },
                CustomMinimumSize = new Vector2(0, 30)
            };
            textVBox.AddChild(descLabel);
            
            var affinityBar = new ProgressBar {
                Value = affinity,
                CustomMinimumSize = new Vector2(100, 15)
            };
            hbox.AddChild(affinityBar);
            
            var affinityLabel = new Label {
                Text = $"{affinity:F1}",
                CustomColors = new Label.LabelSettings { FontColor = new Color(1f, 0.85f, 0.3f) }
            };
            hbox.AddChild(affinityLabel);
        }
    }
    
    private string GetPathIcon(FatePathType type) {
        switch (type) {
            case FatePathType.Hero: return "🛡️";
            case FatePathType.AntiHero: return "🗡️";
            case FatePathType.Villain: return "💀";
            case FatePathType.Mercenary: return "💰";
            case FatePathType.Legend: return "⭐";
            case FatePathType.Myth: return "🌟";
            case FatePathType.Chaos: return "🎲";
            case FatePathType.Order: return "📐";
            case FatePathType.Shadow: return "🌑";
            case FatePathType.Light: return "☀️";
            default: return "✧";
        }
    }
    
    private void RefreshStatsList() {
        foreach (var child in _statsContainer.GetChildren()) {
            child.QueueFree();
        }
        
        var stats = _system.Data;
        var statistics = _system.Statistics;
        
        AddStatRow("Total Fate Weaves", statistics.TotalChoicesMade.ToString());
        AddStatRow("Current Weave Level", _system.GetWeaveLevel().ToString());
        AddStatRow("Highest Path Affinity", $"{statistics.HighestPathAffinity:F1}");
        
        AddSectionHeader("Choices by Type");
        AddStatRow("Moral Choices", statistics.MoralChoices.ToString());
        AddStatRow("Combat Choices", statistics.CombatChoices.ToString());
        AddStatRow("Social Choices", statistics.SocialChoices.ToString());
        AddStatRow("Economic Choices", statistics.EconomicChoices.ToString());
        AddStatRow("Exploration Choices", statistics.ExplorationChoices.ToString());
        AddStatRow("Mystery Choices", statistics.MysteryChoices.ToString());
        
        AddSectionHeader("Stat Bonuses");
        var statBonuses = _system.GetAllStatBonuses();
        foreach (var bonus in statBonuses) {
            if (bonus.Value > 0) {
                AddStatRow(bonus.Key, $"+{bonus.Value:F1}");
            }
        }
    }
    
    private void AddStatRow(string label, string value) {
        var hbox = new HBoxContainer {
            Margin = new Margin(0, 2, 0, 2)
        };
        _statsContainer.AddChild(hbox);
        
        var labelControl = new Label {
            Text = label + ":",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            CustomColors = new Label.LabelSettings { FontColor = new Color(0.7f, 0.7f, 0.8f) }
        };
        hbox.AddChild(labelControl);
        
        var valueControl = new Label {
            Text = value,
            CustomColors = new Label.LabelSettings { FontColor = new Color(1f, 0.85f, 0.3f) }
        };
        hbox.AddChild(valueControl);
    }
    
    private void AddSectionHeader(string title) {
        var header = new Label {
            Text = "━━━ " + title + " ━━━",
            CustomFonts = { "font", GetThemeFont("bold", "Label") },
            CustomColors = new Label.LabelSettings { FontColor = new Color(0.5f, 0.5f, 0.6f) },
            Margin = new Margin(0, 15, 0, 5)
        };
        _statsContainer.AddChild(header);
    }
    
    private void OnSelectChoice(FateChoice choice) {
        _currentChoice = choice;
        _choiceBackground.Visible = true;
        
        _choiceTitle.Text = choice.Title;
        _choiceDescription.Text = choice.Description;
        
        // Build influence text
        var influenceText = "Path Influence:\n";
        foreach (var influence in choice.PathInfluence) {
            influenceText += $"{GetPathIcon(influence.Key)} {influence.Key}: +{influence.Value:F1}\n";
        }
        influenceText += "\nStat Bonuses:\n";
        foreach (var stat in choice.StatBonuses) {
            influenceText += $"{stat.Key}: +{stat.Value:F1}\n";
        }
        influenceText += $"\n{choice.ConsequenceDescription}";
        
        _pathInfluenceLabel.Text = influenceText;
    }
    
    private void CloseChoiceModal() {
        _choiceBackground.Visible = false;
        _currentChoice = null;
    }
    
    private void ConfirmChoice() {
        if (_currentChoice != null) {
            _system.MakeChoice(_currentChoice);
            CloseChoiceModal();
        }
    }
    
    private void OnChoiceMade(FateChoice choice) {
        RefreshUI();
    }
    
    private void OnWeaveLevelChanged(int newLevel) {
        RefreshUI();
    }
    
    public void Toggle() {
        IsVisible = !IsVisible;
        Visible = IsVisible;
        if (IsVisible) {
            RefreshUI();
        }
    }
    
    public void Show() {
        IsVisible = true;
        Visible = true;
        RefreshUI();
    }
    
    public void Hide() {
        IsVisible = false;
        Visible = false;
    }
    
    public override void _Input(InputEvent @event) {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
            if (keyEvent.Scancode == (int)KeyList.F) {
                Toggle();
            } else if (keyEvent.Scancode == (int)KeyList.Escape && IsVisible) {
                Hide();
            }
        }
    }
}
