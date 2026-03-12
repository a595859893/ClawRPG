using Godot;
using System;
using System.Collections.Generic;

public class AlchemyLaboratoryUI : Control
{
    private AlchemyLaboratorySystem system;
    
    // UI Elements
    private Label titleLabel;
    private Label levelLabel;
    private Label goldLabel;
    private Label statusLabel;
    private VBoxContainer researchListContainer;
    private Button upgradeButton;
    private Button closeButton;
    
    // Research item scene reference
    private PackedScene researchItemScene;
    
    public override void _Ready()
    {
        system = AlchemyLaboratorySystem.Instance;
        
        SetupUI();
        ConnectSignals();
        RefreshUI();
    }

    private void SetupUI()
    {
        // Main panel
        var panel = new PanelContainer();
        panel.SetAnchorsPreset(Control.LayoutPreset.Center);
        panel.CustomMinimumSize = new Vector2(800, 600);
        AddChild(panel);
        
        var mainVBox = new VBoxContainer();
        panel.AddChild(mainVBox);
        
        // Header
        var headerHBox = new HBoxContainer();
        mainVBox.AddChild(headerHBox);
        
        titleLabel = new Label();
        titleLabel.Text = "⚗️ Alchemy Laboratory";
        titleLabel.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
        headerHBox.AddChild(titleLabel);
        
        closeButton = new Button();
        closeButton.Text = "✕";
        closeButton.CustomMinimumSize = new Vector2(40, 40);
        closeButton.Pressed += OnClosePressed;
        headerHBox.AddChild(closeButton);
        
        // Level and gold info
        var infoHBox = new HBoxContainer();
        mainVBox.AddChild(infoHBox);
        
        levelLabel = new Label();
        levelLabel.Text = "Laboratory Level: 1";
        infoHBox.AddChild(levelLabel);
        
        var spacer = new Control();
        spacer.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
        infoHBox.AddChild(spacer);
        
        goldLabel = new Label();
        goldLabel.Text = "Gold: 0";
        infoHBox.AddChild(goldLabel);
        
        // Upgrade button
        upgradeButton = new Button();
        upgradeButton.Text = "Upgrade Laboratory (5000g)";
        upgradeButton.Pressed += OnUpgradePressed;
        mainVBox.AddChild(upgradeButton);
        
        // Separator
        var hSeparator = new HSeparator();
        mainVBox.AddChild(hSeparator);
        
        // Status label
        statusLabel = new Label();
        statusLabel.Text = "Research Progress";
        mainVBox.AddChild(statusLabel);
        
        // Scroll container for research list
        var scrollContainer = new ScrollContainer();
        scrollContainer.SizeFlagsVertical = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
        mainVBox.AddChild(scrollContainer);
        
        researchListContainer = new VBoxContainer();
        researchListContainer.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
        scrollContainer.AddChild(researchListContainer);
        
        // Statistics
        var statsLabel = new Label();
        statsLabel.Text = "Total Researches: 0 | Formulas Discovered: 0 | Gold Invested: 0";
        mainVBox.AddChild(statsLabel);
    }

    private void ConnectSignals()
    {
        // Connect will be done in code
    }

    private void RefreshUI()
    {
        if (system == null) return;
        
        // Update level
        levelLabel.Text = "Laboratory Level: " + system.LaboratoryLevel;
        
        // Update gold
        goldLabel.Text = "Gold: " + Player.Instance.Gold;
        
        // Update upgrade button
        int upgradeCost = system.LaboratoryLevel * 5000;
        upgradeButton.Text = "Upgrade Laboratory (" + upgradeCost + "g)";
        
        // Clear and rebuild research list
        foreach (Node child in researchListContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add research items
        foreach (var kvp in system.Researches)
        {
            var research = kvp.Value;
            var item = CreateResearchItem(research);
            researchListContainer.AddChild(item);
        }
    }

    private Control CreateResearchItem(AlchemyLaboratorySystem.AlchemyResearch research)
    {
        var container = new HBoxContainer();
        container.CustomMinimumSize = new Vector2(0, 50);
        
        // Research info
        var infoVBox = new VBoxContainer();
        container.AddChild(infoVBox);
        
        var nameLabel = new Label();
        nameLabel.Text = research.Name + " (" + research.Type.ToString() + ")";
        infoVBox.AddChild(nameLabel);
        
        var progressLabel = new Label();
        progressLabel.Text = "Progress: " + research.Progress + "/" + research.MaxProgress;
        infoVBox.AddChild(progressLabel);
        
        // Spacer
        var spacer = new Control();
        spacer.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
        container.AddChild(spacer);
        
        // Cost and status
        if (research.IsCompleted)
        {
            var completedLabel = new Label();
            completedLabel.Text = "✓ Completed";
            completedLabel.Modulate = new Color(0, 1, 0);
            container.AddChild(completedLabel);
        }
        else
        {
            var costLabel = new Label();
            costLabel.Text = research.GoldCost + "g";
            container.AddChild(costLabel);
            
            var startButton = new Button();
            startButton.Text = "Research";
            startButton.Pressed += () => OnStartResearchPressed(research.Id);
            container.AddChild(startButton);
        }
        
        return container;
    }

    private void OnStartResearchPressed(string researchId)
    {
        system.StartResearch(researchId);
        RefreshUI();
    }

    private void OnUpgradePressed()
    {
        system.LevelUpLaboratory();
        RefreshUI();
    }

    private void OnClosePressed()
    {
        QueueFree();
    }

    public static void Toggle()
    {
        var existingUI = Engine.GetMainLoop().GetRoot().GetNode<AlchemyLaboratoryUI>("AlchemyLaboratoryUI");
        if (existingUI != null)
        {
            existingUI.QueueFree();
        }
        else
        {
            var ui = new AlchemyLaboratoryUI();
            ui.Name = "AlchemyLaboratoryUI";
            Engine.GetMainLoop().GetRoot().AddChild(ui);
        }
    }
}
