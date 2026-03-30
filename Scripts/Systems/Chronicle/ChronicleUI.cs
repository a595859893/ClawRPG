using Godot;
using System;
using System.Collections.Generic;

public class ChronicleUI : Control
{
    private VBoxContainer mainContainer;
    private TabContainer tabContainer;
    private Label titleLabel;
    private Label chapterLabel;
    private Label descriptionLabel;
    private ProgressBar progressBar;
    private VBoxContainer chroniclesList;
    private VBoxContainer loreList;
    private VBoxContainer flagsList;
    private Button closeButton;
    
    private Color defaultColor = new Color(1, 1, 1, 1);
    private Color completedColor = new Color(0.3f, 0.8f, 0.3f, 1);
    private Color inProgressColor = new Color(0.3f, 0.6f, 0.9f, 1);
    
    public override void _Ready()
    {
        Visible = false;
        SetupUI();
        
        // Connect input
        VisibilityChanged += OnVisibilityChanged;
    }
    
    private void SetupUI()
    {
        // Main panel
        var panel = new PanelContainer();
        panel.SetAnchorPreset(ControlPreset.FullRect);
        AddChild(panel);
        
        var panelStyle = new StyleBoxFlat();
        panelStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        panelStyle.CornerRadiusTopLeft = 10;
        panelStyle.CornerRadiusTopRight = 10;
        panelStyle.CornerRadiusBottomLeft = 10;
        panelStyle.CornerRadiusBottomRight = 10;
        panelStyle.SetBorderWidthAll(2);
        panelStyle.BorderColor = new Color(0.4f, 0.3f, 0.2f, 1);
        panel.AddThemeStyleboxOverride("panel", panelStyle);
        
        mainContainer = new VBoxContainer();
        mainContainer.SetAnchorPreset(ControlPreset.FullRect);
        mainContainer.AddThemeConstantOverride("separation", 15);
        panel.AddChild(mainContainer);
        
        // Title header
        var headerContainer = new HBoxContainer();
        mainContainer.AddChild(headerContainer);
        
        titleLabel = new Label();
        titleLabel.Text = " ⚔ Chronicle Quest Journal";
        titleLabel.AddThemeFontSizeOverride("font_size", 28);
        titleLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.75f, 0.4f, 1));
        headerContainer.AddChild(titleLabel);
        
        headerContainer.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        closeButton = new Button();
        closeButton.Text = "✕";
        closeButton.TooltipText = "Close (Esc)";
        closeButton.Pressed += OnClosePressed;
        headerContainer.AddChild(closeButton);
        
        // Chapter info
        var chapterContainer = new VBoxContainer();
        mainContainer.AddChild(chapterContainer);
        
        chapterLabel = new Label();
        chapterLabel.Text = "Chapter: Prologue";
        chapterLabel.AddThemeFontSizeOverride("font_size", 20);
        chapterLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.85f, 1f, 1));
        chapterContainer.AddChild(chapterLabel);
        
        descriptionLabel = new Label();
        descriptionLabel.Text = "Your adventure begins...";
        descriptionLabel.AddThemeFontSizeOverride("font_size", 14);
        descriptionLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f, 1));
        chapterContainer.AddChild(descriptionLabel);
        
        // Progress bar
        progressBar = new ProgressBar();
        progressBar.MinValue = 0;
        progressBar.MaxValue = 100;
        progressBar.Value = 0;
        progressBar.CustomMinimumSize = new Vector2(0, 20);
        
        var progressStyle = new StyleBoxFlat();
        progressStyle.BgColor = new Color(0.2f, 0.2f, 0.25f, 1);
        progressStyle.CornerRadiusTopLeft = 5;
        progressStyle.CornerRadiusTopRight = 5;
        progressStyle.CornerRadiusBottomLeft = 5;
        progressStyle.CornerRadiusBottomRight = 5;
        progressBar.AddThemeStyleboxOverride("background", progressStyle);
        
        var progressFill = new StyleBoxFlat();
        progressFill.BgColor = new Color(0.2f, 0.6f, 0.9f, 1);
        progressFill.CornerRadiusTopLeft = 5;
        progressFill.CornerRadiusTopRight = 5;
        progressFill.CornerRadiusBottomLeft = 5;
        progressFill.CornerRadiusBottomRight = 5;
        progressBar.AddThemeStyleboxOverride("fill", progressFill);
        
        mainContainer.AddChild(progressBar);
        
        // Tab container
        tabContainer = new TabContainer();
        tabContainer.SetAnchorPreset(ControlPreset.FullRect);
        tabContainer.CustomMinimumSize = new Vector2(0, 400);
        mainContainer.AddChild(tabContainer);
        
        // Quests tab
        var questsTab = new ScrollContainer();
        questsTab.Name = "Quests";
        tabContainer.AddChild(questsTab);
        
        chroniclesList = new VBoxContainer();
        chroniclesList.AddThemeConstantOverride("separation", 10);
        questsTab.AddChild(chroniclesList);
        
        // Lore tab
        var loreTab = new ScrollContainer();
        loreTab.Name = "Lore";
        tabContainer.AddChild(loreTab);
        
        loreList = new VBoxContainer();
        loreList.AddThemeConstantOverride("separation", 10);
        loreTab.AddChild(loreList);
        
        // Story Flags tab
        var flagsTab = new ScrollContainer();
        flagsTab.Name = "Story Flags";
        tabContainer.AddChild(flagsTab);
        
        flagsList = new VBoxContainer();
        flagsList.AddThemeConstantOverride("separation", 5);
        flagsTab.AddChild(flagsList);
        
        // Stats footer
        var footerContainer = new HBoxContainer();
        mainContainer.AddChild(footerContainer);
        
        var loreCountLabel = new Label();
        loreCountLabel.Text = "Lore Discovered: 0/50";
        loreCountLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f, 1));
        footerContainer.AddChild(loreCountLabel);
        
        footerContainer.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        var hintLabel = new Label();
        hintLabel.Text = "Press J to toggle";
        hintLabel.AddThemeFontSizeOverride("font_size", 12);
        hintLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f, 1));
        footerContainer.AddChild(hintLabel);
    }
    
    public override void _Process(double delta)
    {
        if (Visible)
        {
            RefreshDisplay();
        }
    }
    
    private void RefreshDisplay()
    {
        if (ChronicleSystem.Instance == null) return;
        
        // Update chapter info
        var chapter = ChronicleSystem.Instance.GetCurrentChapter();
        if (chapter != null)
        {
            chapterLabel.Text = $"📖 {chapter.title}";
            descriptionLabel.Text = chapter.description;
        }
        
        // Update progress
        float totalProgress = 0;
        float maxTotal = 0;
        
        var chronicles = ChronicleSystem.Instance.GetChronicles();
        foreach (var entry in chronicles.Values)
        {
            totalProgress += entry.progress;
            maxTotal += entry.maxProgress;
        }
        
        progressBar.Value = maxTotal > 0 ? (totalProgress / maxTotal) * 100 : 0;
        
        // Refresh chronicles list
        RefreshChroniclesList(chronicles);
        
        // Refresh lore list
        RefreshLoreList();
        
        // Refresh flags list
        RefreshFlagsList();
    }
    
    private void RefreshChroniclesList(Dictionary<string, ChronicleEntry> chronicles)
    {
        // Clear existing
        foreach (var child in chroniclesList.GetChildren())
        {
            child.QueueFree();
        }
        
        foreach (var entry in chronicles.Values)
        {
            var entryContainer = new VBoxContainer();
            
            var titleLabel = new Label();
            titleLabel.Text = $"{(entry.isCompleted ? "✅" : "📋")} {entry.title}";
            titleLabel.AddThemeFontSizeOverride("font_size", 16);
            titleLabel.AddThemeColorOverride("font_color", entry.isCompleted ? completedColor : inProgressColor);
            entryContainer.AddChild(titleLabel);
            
            var descLabel = new Label();
            descLabel.Text = $"{entry.description} ({entry.progress}/{entry.maxProgress})";
            descLabel.AddThemeFontSizeOverride("font_size", 12);
            descLabel.AddThemeColorOverride("font_color", defaultColor);
            entryContainer.AddChild(descLabel);
            
            var progressBar = new ProgressBar();
            progressBar.MinValue = 0;
            progressBar.MaxValue = entry.maxProgress;
            progressBar.Value = entry.progress;
            progressBar.CustomMinimumSize = new Vector2(0, 10);
            entryContainer.AddChild(progressBar);
            
            chroniclesList.AddChild(entryContainer);
        }
    }
    
    private void RefreshLoreList()
    {
        // Clear existing
        foreach (var child in loreList.GetChildren())
        {
            child.QueueFree();
        }
        
        var lore = ChronicleSystem.Instance.GetDiscoveredLore();
        
        if (lore.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "No lore discovered yet. Explore the world to find ancient knowledge!";
            emptyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f, 1));
            loreList.AddChild(emptyLabel);
            return;
        }
        
        foreach (var entry in lore)
        {
            var entryContainer = new VBoxContainer();
            
            var titleLabel = new Label();
            titleLabel.Text = $"📜 {entry.title}";
            titleLabel.AddThemeFontSizeOverride("font_size", 14);
            titleLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.8f, 0.5f, 1));
            entryContainer.AddChild(titleLabel);
            
            var contentLabel = new Label();
            contentLabel.Text = entry.content;
            contentLabel.AddThemeFontSizeOverride("font_size", 12);
            contentLabel.AddThemeColorOverride("font_color", defaultColor);
            entryContainer.AddChild(contentLabel);
            
            loreList.AddChild(entryContainer);
        }
    }
    
    private void RefreshFlagsList()
    {
        // Clear existing
        foreach (var child in flagsList.GetChildren())
        {
            child.QueueFree();
        }
        
        var flags = ChronicleSystem.Instance.GetStoryFlags();
        
        foreach (var flag in flags)
        {
            var flagLabel = new Label();
            flagLabel.Text = $"{(flag.Value ? "✅" : "⬜")} {flag.Key.Replace("_", " ")}";
            flagLabel.AddThemeFontSizeOverride("font_size", 12);
            flagLabel.AddThemeColorOverride("font_color", flag.Value ? completedColor : new Color(0.5f, 0.5f, 0.5f, 1));
            flagsList.AddChild(flagLabel);
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
    
    private void OnClosePressed()
    {
        Visible = false;
    }
    
    private void OnVisibilityChanged()
    {
        if (Visible)
        {
            RefreshDisplay();
        }
    }
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.J)
        {
            Toggle();
        }
    }
}
