using Godot;
using System;
using System.Collections.Generic;

public class FactionUI : Control
{
    // UI Elements
    private Label titleLabel;
    private Label reputationLabel;
    private GridContainer factionGrid;
    private VBoxContainer detailPanel;
    private Label factionNameLabel;
    private Label factionTypeLabel;
    private Label factionDescLabel;
    private Label factionRepLabel;
    private Label factionRepLevelLabel;
    private Label factionBonusLabel;
    private Label factionQuestsLabel;
    private Button closeButton;
    
    // Data
    private FactionSystem factionSystem;
    private List<Faction> factions;
    private Faction selectedFaction;
    
    // Colors
    private Color neutralColor = Colors.White;
    private Color friendlyColor = new Color(0.2f, 0.8f, 0.2f);
    private Color hostileColor = new Color(0.8f, 0.2f, 0.2f);
    private Color goldColor = new Color(1f, 0.84f, 0f);
    
    public override void _Ready()
    {
        factionSystem = FactionSystem.Instance;
        
        // Create UI
        CreateUI();
        
        // Connect signals
        if (factionSystem != null)
        {
            factionSystem.ReputationChanged += OnReputationChanged;
        }
        
        // Load factions
        RefreshFactionList();
        
        // Show UI
        Visible = false;
    }
    
    private void CreateUI()
    {
        // Main container
        MarginContainer mainContainer = new MarginContainer();
        mainContainer.SetAnchor(AnchorPresets.FullRect);
        mainContainer.AddChild(this);
        
        // Panel
        Panel panel = new Panel();
        panel.SetAnchor(AnchorPresets.FullRect);
        panel.Modulate = new Color(1, 1, 1, 0.9f);
        AddChild(panel);
        
        // Title
        titleLabel = new Label();
        titleLabel.Text = "Faction Reputation";
        titleLabel.SetAnchor(AnchorPresets.TopWide);
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        titleLabel.AddThemeColorOverride("font_color", goldColor);
        titleLabel.RectPosition = new Vector2(20, 20);
        AddChild(titleLabel);
        
        // Reputation summary
        reputationLabel = new Label();
        reputationLabel.Text = "";
        reputationLabel.SetAnchor(AnchorPresets.TopWide);
        reputationLabel.AddThemeFontSizeOverride("font_size", 14);
        reputationLabel.RectPosition = new Vector2(20, 55);
        AddChild(reputationLabel);
        
        // Faction grid
        factionGrid = new GridContainer();
        factionGrid.Columns = 2;
        factionGrid.SetAnchor(AnchorPresets.TopLeft);
        factionGrid.RectPosition = new Vector2(20, 90);
        factionGrid.RectSize = new Vector2(350, 400);
        AddChild(factionGrid);
        
        // Detail panel
        detailPanel = new VBoxContainer();
        detailPanel.SetAnchor(AnchorPresets.FullRect);
        detailPanel.RectPosition = new Vector2(400, 80);
        detailPanel.RectSize = new Vector2(380, 450);
        detailPanel.AddThemeConstantOverride("separation", 10);
        AddChild(detailPanel);
        
        // Faction name
        factionNameLabel = new Label();
        factionNameLabel.AddThemeFontSizeOverride("font_size", 20);
        factionNameLabel.AddThemeColorOverride("font_color", goldColor);
        detailPanel.AddChild(factionNameLabel);
        
        // Faction type
        factionTypeLabel = new Label();
        factionTypeLabel.AddThemeFontSizeOverride("font_size", 14);
        factionTypeLabel.AddThemeColorOverride("font_color", Colors.Gray);
        detailPanel.AddChild(factionTypeLabel);
        
        // Description
        factionDescLabel = new Label();
        factionDescLabel.AddThemeFontSizeOverride("font_size", 12);
        factionDescLabel.RectMinSize = new Vector2(0, 80);
        factionDescLabel.Autowrap = true;
        detailPanel.AddChild(factionDescLabel);
        
        // Reputation
        factionRepLabel = new Label();
        factionRepLabel.AddThemeFontSizeOverride("font_size", 16);
        detailPanel.AddChild(factionRepLabel);
        
        // Reputation level
        factionRepLevelLabel = new Label();
        factionRepLevelLabel.AddThemeFontSizeOverride("font_size", 18);
        detailPanel.AddChild(factionRepLevelLabel);
        
        // Bonus
        factionBonusLabel = new Label();
        factionBonusLabel.AddThemeFontSizeOverride("font_size", 14);
        detailPanel.AddChild(factionBonusLabel);
        
        // Quests
        factionQuestsLabel = new Label();
        factionQuestsLabel.AddThemeFontSizeOverride("font_size", 12);
        factionQuestsLabel.AddThemeColorOverride("font_color", Colors.LightGray);
        factionQuestsLabel.RectMinSize = new Vector2(0, 100);
        factionQuestsLabel.Autowrap = true;
        detailPanel.AddChild(factionQuestsLabel);
        
        // Close button
        closeButton = new Button();
        closeButton.Text = "Close";
        closeButton.RectPosition = new Vector2(650, 20);
        closeButton.RectSize = new Vector2(100, 30);
        closeButton.Pressed += OnClosePressed;
        AddChild(closeButton);
        
        // Update reputation summary
        UpdateReputationSummary();
    }
    
    private void RefreshFactionList()
    {
        // Clear existing
        foreach (Node child in factionGrid.GetChildren())
        {
            child.QueueFree();
        }
        
        // Get factions
        factions = factionSystem.GetAllFactions();
        
        // Create buttons
        foreach (Faction faction in factions)
        {
            Button factionButton = CreateFactionButton(faction);
            factionGrid.AddChild(factionButton);
        }
    }
    
    private Button CreateFactionButton(Faction faction)
    {
        Button button = new Button();
        button.Text = faction.Name;
        button.RectMinSize = new Vector2(160, 50);
        
        // Get reputation
        int rep = factionSystem.GetReputation(faction.Id);
        ReputationLevel level = factionSystem.GetReputationLevel(faction.Id);
        
        // Set color based on reputation
        Color repColor = GetReputationColor(level);
        button.Modulate = repColor;
        
        // Add info
        string tooltip = $"{faction.Description}\n\nReputation: {rep}\nLevel: {level}";
        button.TooltipText = tooltip;
        
        // Connect
        button.Pressed += () => OnFactionSelected(faction);
        
        return button;
    }
    
    private Color GetReputationColor(ReputationLevel level)
    {
        switch (level)
        {
            case ReputationLevel.Exalted: return new Color(1f, 0.84f, 0f); // Gold
            case ReputationLevel.Honored: return new Color(0.2f, 0.8f, 0.2f); // Green
            case ReputationLevel.Friendly: return new Color(0.4f, 0.9f, 0.4f); // Light green
            case ReputationLevel.Neutral: return Colors.White;
            case ReputationLevel.Unfriendly: return new Color(0.9f, 0.6f, 0.4f); // Light orange
            case ReputationLevel.Hostile: return new Color(0.9f, 0.4f, 0.2f); // Orange
            case ReputationLevel.Hated: return new Color(0.8f, 0.2f, 0.2f); // Red
            default: return Colors.White;
        }
    }
    
    private void OnFactionSelected(Faction faction)
    {
        selectedFaction = faction;
        UpdateDetailPanel();
    }
    
    private void UpdateDetailPanel()
    {
        if (selectedFaction == null) return;
        
        int rep = factionSystem.GetReputation(selectedFaction.Id);
        ReputationLevel level = factionSystem.GetReputationLevel(selectedFaction.Id);
        float bonus = factionSystem.GetFactionBonus(selectedFaction.Id);
        float discount = factionSystem.GetMerchantDiscount(selectedFaction.Id);
        List<string> quests = factionSystem.GetAvailableQuests(selectedFaction.Id);
        
        factionNameLabel.Text = selectedFaction.Name;
        factionTypeLabel.Text = $"Type: {selectedFaction.Type}";
        factionDescLabel.Text = selectedFaction.Description;
        
        factionRepLabel.Text = $"Reputation: {rep} / {FactionSystem.MAX_REPUTATION}";
        factionRepLabel.Modulate = GetReputationColor(level);
        
        factionRepLevelLabel.Text = $"Level: {level}";
        
        string bonusText = "Bonus: ";
        if (bonus > 1.0f)
            bonusText += $"+{(int)((bonus - 1.0f) * 100)}%";
        else if (bonus < 1.0f)
            bonusText += $"{(int)((bonus - 1.0f) * 100)}%";
        else
            bonusText += "None";
        
        if (discount != 0)
        {
            if (discount > 0)
                bonusText += $"\nMerchant Discount: -{(int)(discount * 100)}%";
            else
                bonusText += $"\nMerchant Surcharge: +{(int)(Math.Abs(discount) * 100)}%";
        }
        
        if (factionSystem.CanAccessVendor(selectedFaction.Id))
            bonusText += "\n✓ Vendor Access: Available";
        else
            bonusText += "\n✗ Vendor Access: Locked (Friendly required)";
        
        factionBonusLabel.Text = bonusText;
        
        // Quests
        string questText = "Available Quests:\n";
        if (quests.Count == 0)
            questText += "None";
        else
        {
            foreach (string quest in quests)
            {
                questText += "• " + FormatQuestName(quest) + "\n";
            }
        }
        factionQuestsLabel.Text = questText;
    }
    
    private string FormatQuestName(string questId)
    {
        string[] parts = questId.Split('_');
        if (parts.Length < 2) return questId;
        
        string questType = parts[0];
        string faction = parts[1];
        
        switch (questType)
        {
            case "faction_punish": return "Punish Enemies";
            case "faction_stealth": return "Stealth Mission";
            case "faction_earn": return "Earn Reputation";
            case "faction_intro": return "Introduction";
            case "faction_gather": return "Gather Resources";
            case "faction_deliver": return "Deliver Goods";
            case "faction_hunt": return "Hunt Targets";
            case "faction_elite": return "Elite Contract";
            case "faction_escort": return "Escort Mission";
            case "faction_legendary": return "Legendary Quest";
            case "faction_leader": return "Leader's Request";
            default: return questId;
        }
    }
    
    private void UpdateReputationSummary()
    {
        var reputations = factionSystem.GetAllReputations();
        
        int totalPositive = 0;
        int totalNegative = 0;
        
        foreach (var kvp in reputations)
        {
            if (kvp.Value > 0) totalPositive += kvp.Value;
            else if (kvp.Value < 0) totalNegative += kvp.Value;
        }
        
        reputationLabel.Text = $"Overall: +{totalPositive} / {totalNegative} (Positive/Negative Reputations)";
    }
    
    private void OnReputationChanged(string factionId, int newReputation)
    {
        RefreshFactionList();
        if (selectedFaction != null && selectedFaction.Id == factionId)
        {
            UpdateDetailPanel();
        }
        UpdateReputationSummary();
    }
    
    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("ui_cancel"))
        {
            OnClosePressed();
        }
    }
    
    private void OnClosePressed()
    {
        Visible = false;
    }
    
    public void Toggle()
    {
        Visible = !Visible;
        if (Visible)
        {
            RefreshFactionList();
            UpdateReputationSummary();
            if (selectedFaction != null)
            {
                UpdateDetailPanel();
            }
        }
    }
}
