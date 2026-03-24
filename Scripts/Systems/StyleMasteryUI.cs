using Godot;
/// <summary>
/// 风格精通用户界面。
/// </summary>
using System;
using System.Collections.Generic;

public class StyleMasteryUI : Control
{
    private TabContainer tabContainer;
    private VBoxContainer overviewContainer;
    private VBoxContainer stylesContainer;
    private VBoxContainer statsContainer;
    
    private Label titleLabel;
    private Label activeStyleLabel;
    private Label styleInfoLabel;
    
    private StyleMasterySystem system;
    
    private string selectedStyle = "";
    
    public override void _Ready()
    {
        system = GetNode<StyleMasterySystem>("/root/StyleMasterySystem");
        
        SetupUI();
        RefreshUI();
    }
    
    private void SetupUI()
    {
        // Main panel
        var panel = new PanelContainer();
        panel.SetAnchorPreset(Control.Preset.FullRect);
        AddChild(panel);
        
        var mainVBox = new VBoxContainer();
        panel.AddChild(mainVBox);
        
        // Title
        titleLabel = new Label();
        titleLabel.Text = "⚔️ Style Mastery System";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainVBox.AddChild(titleLabel);
        
        // Active style display
        activeStyleLabel = new Label();
        activeStyleLabel.Text = "Active Style: None";
        activeStyleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        mainVBox.AddChild(activeStyleLabel);
        
        // Tab container
        tabContainer = new TabContainer();
        tabContainer.SetVExpandFlags(Control.VExpandFlags.ExpandFill);
        mainVBox.AddChild(tabContainer);
        
        // Overview tab
        overviewContainer = new VBoxContainer();
        overviewContainer.Name = "Overview";
        tabContainer.AddChild(overviewContainer);
        
        // Styles tab
        stylesContainer = new VBoxContainer();
        stylesContainer.Name = "Styles";
        tabContainer.AddChild(stylesContainer);
        
        // Statistics tab
        statsContainer = new VBoxContainer();
        statsContainer.Name = "Statistics";
        tabContainer.AddChild(statsContainer);
        
        // Close button
        var closeBtn = new Button();
        closeBtn.Text = "Close (ESC)";
        closeBtn.Pressed += OnClosePressed;
        mainVBox.AddChild(closeBtn);
        
        SetupOverviewTab();
        SetupStylesTab();
        SetupStatsTab();
    }
    
    private void SetupOverviewTab()
    {
        var scroll = new ScrollContainer();
        scroll.SetVExpandFlags(Control.VExpandFlags.ExpandFill);
        overviewContainer.AddChild(scroll);
        
        var content = new VBoxContainer();
        scroll.AddChild(content);
        
        // Current bonuses display
        var bonusesLabel = new Label();
        bonusesLabel.Text = "Current Style Bonuses";
        bonusesLabel.AddThemeFontSizeOverride("font_size", 18);
        content.AddChild(bonusesLabel);
        
        styleInfoLabel = new Label();
        styleInfoLabel.Text = "No active style";
        content.AddChild(styleInfoLabel);
        
        // Quick switch section
        var switchLabel = new Label();
        switchLabel.Text = "\nQuick Switch";
        switchLabel.AddThemeFontSizeOverride("font_size", 18);
        content.AddChild(switchLabel);
        
        var switchGrid = new GridContainer();
        switchGrid.Columns = 3;
        content.AddChild(switchGrid);
        
        // Add quick switch buttons for top styles
        var topStyles = new[] { "berserker", "guardian", "samurai" };
        foreach (var styleId in topStyles)
        {
            var btn = new Button();
            var config = system.GetStyleConfig(styleId);
            if (config != null)
            {
                btn.Text = config.Icon + " " + config.StyleName;
                btn.Pressed += () => OnStyleSelected(styleId);
                switchGrid.AddChild(btn);
            }
        }
    }
    
    private void SetupStylesTab()
    {
        var scroll = new ScrollContainer();
        scroll.SetVExpandFlags(Control.VExpandFlags.ExpandFill);
        stylesContainer.AddChild(scroll);
        
        var content = new VBoxContainer();
        scroll.AddChild(content);
        
        // Style list will be populated dynamically
        RefreshStyleList(content);
    }
    
    private void SetupStatsTab()
    {
        RefreshStatsDisplay();
    }
    
    private void RefreshStyleList(VBoxContainer container)
    {
        // Clear existing
        foreach (var child in container.GetChildren())
        {
            child.QueueFree();
        }
        
        var styles = system.GetAllStyles();
        
        foreach (var kvp in styles)
        {
            var style = kvp.Value;
            var record = system.GetStyle(style.StyleId);
            
            var stylePanel = new PanelContainer();
            stylePanel.CustomMinimumSize = new Vector2(0, 80);
            container.AddChild(stylePanel);
            
            var hbox = new HBoxContainer();
            stylePanel.AddChild(hbox);
            
            // Icon and name
            var iconLabel = new Label();
            iconLabel.Text = style.Icon + " " + style.StyleName;
            iconLabel.AddThemeFontSizeOverride("font_size", 16);
            hbox.AddChild(iconLabel);
            
            // Level
            var levelLabel = new Label();
            levelLabel.Text = " Lv." + (record != null ? record.MasteryLevel.ToString() : "1");
            levelLabel.HorizontalAlignment = HorizontalAlignment.Right;
            hbox.AddChild(levelLabel);
            
            // Category and unlock level info
            var infoLabel = new Label();
            infoLabel.Text = $"{style.Category} | Unlock Lv.{style.UnlockLevel}";
            infoLabel.AddThemeFontSizeOverride("font_size", 12);
            container.AddChild(infoLabel);
            
            // Description
            var descLabel = new Label();
            descLabel.Text = style.Description;
            descLabel.AddThemeFontSizeOverride("font_size", 12);
            container.AddChild(descLabel);
            
            // Action buttons
            var btnHBox = new HBoxContainer();
            container.AddChild(btnHBox);
            
            var selectBtn = new Button();
            selectBtn.Text = "Select";
            selectBtn.Pressed += () => OnStyleSelected(style.StyleId);
            btnHBox.AddChild(selectBtn);
            
            var infoBtn = new Button();
            infoBtn.Text = "Details";
            infoBtn.Pressed += () => ShowStyleDetails(style.StyleId);
            btnHBox.AddChild(infoBtn);
            
            // Separator
            var sep = new HSeparator();
            container.AddChild(sep);
        }
    }
    
    private void RefreshStatsDisplay()
    {
        // Clear existing
        foreach (var child in statsContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        var stats = system.GetStatistics();
        
        var titleLabel = new Label();
        titleLabel.Text = "Style Mastery Statistics";
        titleLabel.AddThemeFontSizeOverride("font_size", 18);
        statsContainer.AddChild(titleLabel);
        
        foreach (var kvp in stats)
        {
            var label = new Label();
            label.Text = $"{kvp.Key}: {kvp.Value}";
            statsContainer.AddChild(label);
        }
    }
    
    private void RefreshUI()
    {
        // Update active style display
        string activeStyle = system.GetActiveStyle();
        if (!string.IsNullOrEmpty(activeStyle))
        {
            var config = system.GetStyleConfig(activeStyle);
            if (config != null)
            {
                activeStyleLabel.Text = $"Active Style: {config.Icon} {config.StyleName}";
                
                // Update style info
                var info = system.GetStyleInfo(activeStyle);
                if (info.ContainsKey("bonuses"))
                {
                    var bonuses = (Dictionary<string, float>)info["bonuses"];
                    string bonusText = "Bonuses:\n";
                    foreach (var b in bonuses)
                    {
                        string sign = b.Value >= 0 ? "+" : "";
                        bonusText += $"  {b.Key}: {sign}{b.Value:F1}\n";
                    }
                    styleInfoLabel.Text = bonusText;
                }
            }
        }
        else
        {
            activeStyleLabel.Text = "Active Style: None";
            styleInfoLabel.Text = "No active style selected";
        }
        
        // Refresh style list
        if (stylesContainer.GetChildCount() > 0)
        {
            var scroll = stylesContainer.GetChild<ScrollContainer>(0);
            if (scroll != null && scroll.GetChildCount() > 0)
            {
                RefreshStyleList(scroll.GetChild<VBoxContainer>(0));
            }
        }
        
        // Refresh stats
        RefreshStatsDisplay();
    }
    
    private void OnStyleSelected(string styleId)
    {
        system.SwitchStyle(styleId);
        RefreshUI();
    }
    
    private void ShowStyleDetails(string styleId)
    {
        var info = system.GetStyleInfo(styleId);
        
        // Show details in a dialog or update the info panel
        string details = $"=== {info["name"]} ===\n";
        details += $"Category: {info["category"]}\n";
        details += $"Level: {info["level"]} (XP: {info["xp"]})\n";
        details += $"Enemies Defeated: {info["enemies_defeated"]}\n";
        details += $"Unlock Level: {info["unlock_level"]}\n";
        details += $"\n{info["description"]}\n\n";
        
        if (info.ContainsKey("bonuses"))
        {
            var bonuses = (Dictionary<string, float>)info["bonuses"];
            details += "Bonuses:\n";
            foreach (var b in bonuses)
            {
                string sign = b.Value >= 0 ? "+" : "";
                details += $"  {b.Key}: {sign}{b.Value:F1}\n";
            }
        }
        
        GD.Print(details);
    }
    
    private void OnClosePressed()
    {
        Hide();
    }
    
    public override void _Input(InputEvent ev)
    {
        if (ev is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            Hide();
        }
    }
}
}
