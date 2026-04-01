using Godot;
using System;
using System.Collections.Generic;

public partial class SkillTreeUI : Control
{
    private VBoxContainer mainContainer;
    private HBoxContainer headerContainer;
    private Label titleLabel;
    private Label pointsLabel;
    private TabContainer categoryTabs;
    private Dictionary<string, Control> categoryPanels = new Dictionary<string, Control>();
    private Button closeButton;
    
    private SkillTreeSystem skillTreeSystem;
    private SkillTreeDatabase database;
    
    private Color unlockedColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    private Color availableColor = new Color(0.2f, 0.6f, 1f, 1f);
    private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    private Color parentLockedColor = new Color(0.5f, 0.3f, 0.3f, 0.5f);
    
    public override void _Ready()
    {
        skillTreeSystem = SkillTreeSystem.Instance;
        database = SkillTreeDatabase.Instance;
        
        skillTreeSystem.OnNodeUnlocked += OnNodeUnlocked;
        skillTreeSystem.OnSkillPointsChanged += UpdatePointsDisplay;
        
        SetupUI();
        Visible = false;
    }
    
    private void SetupUI()
    {
        // Main container
        mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(mainContainer);
        
        // Header
        headerContainer = new HBoxContainer();
        headerContainer.AddThemeConstantOverride("separation", 20);
        mainContainer.AddChild(headerContainer);
        
        titleLabel = new Label();
        titleLabel.Text = " Skill Tree ";
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        headerContainer.AddChild(titleLabel);
        
        pointsLabel = new Label();
        pointsLabel.Text = "Points: 10";
        pointsLabel.AddThemeFontSizeOverride("font_size", 18);
        headerContainer.AddChild(pointsLabel);
        
        var spacer = new Control();
        spacer.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        headerContainer.AddChild(spacer);
        
        closeButton = new Button();
        closeButton.Text = "X";
        closeButton.CustomMinimumSize = new Vector2(40, 40);
        closeButton.Pressed += () => ToggleUI();
        headerContainer.AddChild(closeButton);
        
        // Category tabs
        categoryTabs = new TabContainer();
        categoryTabs.SizeFlagsVertical = Control.SizeFlags.Expand;
        mainContainer.AddChild(categoryTabs);
        
        // Create tabs for each category
        foreach (var category in database.Categories.Values)
        {
            var panel = CreateCategoryPanel(category);
            categoryTabs.AddChild(panel);
            panel.Name = category.Name;
            categoryPanels[category.CategoryId] = panel;
        }
        
        UpdateAllNodes();
    }
    
    private Control CreateCategoryPanel(SkillTreeCategory category)
    {
        var scroll = new ScrollContainer();
        scroll.Name = category.CategoryId;
        
        var grid = new GridContainer();
        grid.Columns = 5;
        grid.AddThemeConstantOverride("h_separation", 10);
        grid.AddThemeConstantOverride("v_separation", 10);
        grid.AddThemeConstantOverride("margin_left", 20);
        grid.AddThemeConstantOverride("margin_top", 20);
        grid.AddThemeConstantOverride("margin_right", 20);
        grid.AddThemeConstantOverride("margin_bottom", 20);
        scroll.AddChild(grid);
        
        // Add category info at top
        var infoLabel = new Label();
        infoLabel.Text = $"{category.Name}: {skillTreeSystem.GetSpentPointsInCategory(category.CategoryId)}/{category.MaxPoints} points";
        infoLabel.Name = "info_label";
        grid.AddChild(infoLabel);
        
        // Add nodes
        var nodes = database.GetNodesByCategory(category.CategoryId);
        foreach (var node in nodes)
        {
            var nodeButton = CreateNodeButton(node);
            grid.AddChild(nodeButton);
        }
        
        return scroll;
    }
    
    private Control CreateNodeButton(SkillTreeNode node)
    {
        var container = new VBoxContainer();
        container.CustomMinimumSize = new Vector2(150, 100);
        
        var button = new Button();
        button.Text = $"{node.Name}\n\nCost: {node.Cost}\n\n{node.Description}";
        button.TooltipText = node.Description;
        button.CustomMinimumSize = new Vector2(150, 100);
        button.Pressed += () => OnNodeButtonPressed(node);
        button.Name = $"node_{node.NodeId}";
        container.AddChild(button);
        
        return container;
    }
    
    private void OnNodeButtonPressed(SkillTreeNode node)
    {
        if (skillTreeSystem.IsNodeUnlocked(node.NodeId))
        {
            GD.Print($"[SkillTreeUI] Node already unlocked: {node.Name}");
            return;
        }
        
        if (skillTreeSystem.UnlockNode(node.NodeId))
        {
            GD.Print($"[SkillTreeUI] Successfully unlocked: {node.Name}");
            UpdateAllNodes();
        }
        else
        {
            GD.Print($"[SkillTreeUI] Cannot unlock: {node.Name} - requirements not met");
        }
    }
    
    private void OnNodeUnlocked(string nodeId)
    {
        UpdateAllNodes();
    }
    
    private void UpdatePointsDisplay()
    {
        int available = skillTreeSystem.GetAvailableSkillPoints();
        int total = skillTreeSystem.PlayerData.TotalSkillPoints;
        pointsLabel.Text = $"Points: {available}/{total}";
        
        // Update category info labels
        foreach (var category in database.Categories.Values)
        {
            if (categoryPanels.ContainsKey(category.CategoryId))
            {
                var panel = categoryPanels[category.CategoryId];
                var scroll = panel as ScrollContainer;
                if (scroll != null && scroll.GetChildCount() > 0)
                {
                    var grid = scroll.GetChild(0) as GridContainer;
                    if (grid != null && grid.GetChildCount() > 0)
                    {
                        var infoLabel = grid.GetChild(0) as Label;
                        if (infoLabel != null && infoLabel.Name == "info_label")
                        {
                            infoLabel.Text = $"{category.Name}: {skillTreeSystem.GetSpentPointsInCategory(category.CategoryId)}/{category.MaxPoints} points";
                        }
                    }
                }
            }
        }
    }
    
    private void UpdateAllNodes()
    {
        foreach (var category in database.Categories.Values)
        {
            if (!categoryPanels.ContainsKey(category.CategoryId))
                continue;
                
            var panel = categoryPanels[category.CategoryId];
            var scroll = panel as ScrollContainer;
            if (scroll == null || scroll.GetChildCount() == 0)
                continue;
                
            var grid = scroll.GetChild(0) as GridContainer;
            if (grid == null)
                continue;
            
            // Update from index 1 (skip info label at index 0)
            for (int i = 1; i < grid.GetChildCount(); i++)
            {
                var container = grid.GetChild(i) as VBoxContainer;
                if (container == null || container.GetChildCount() == 0)
                    continue;
                    
                var button = container.GetChild(0) as Button;
                if (button == null)
                    continue;
                
                string nodeId = button.Name.Replace("node_", "");
                var node = database.GetNode(nodeId);
                if (node == null)
                    continue;
                
                UpdateNodeButtonAppearance(button, node);
            }
        }
        
        UpdatePointsDisplay();
    }
    
    private void UpdateNodeButtonAppearance(Button button, SkillTreeNode node)
    {
        bool isUnlocked = skillTreeSystem.IsNodeUnlocked(node.NodeId);
        bool canUnlock = skillTreeSystem.CanUnlockNode(node.NodeId);
        
        if (isUnlocked)
        {
            button.Modulate = unlockedColor;
            button.Text = $"✅ {node.Name}\n\nCost: {node.Cost}\n\n{node.Description}";
        }
        else if (canUnlock)
        {
            button.Modulate = availableColor;
            button.Text = $"🔓 {node.Name}\n\nCost: {node.Cost}\n\n{node.Description}";
        }
        else
        {
            // Check if parent is unlocked
            bool parentUnlocked = string.IsNullOrEmpty(node.ParentNodeId) || 
                skillTreeSystem.IsNodeUnlocked(node.ParentNodeId);
            
            if (!parentUnlocked)
            {
                button.Modulate = parentLockedColor;
                button.Text = $"🔒 {node.Name}\n\nCost: {node.Cost}\n\n(Requires: {node.ParentNodeId})";
            }
            else
            {
                button.Modulate = lockedColor;
                button.Text = $"🔒 {node.Name}\n\nCost: {node.Cost}\n\n{node.Description}";
            }
        }
    }
    
    public void ToggleUI()
    {
        Visible = !Visible;
        if (Visible)
        {
            UpdateAllNodes();
            GD.Print("[SkillTreeUI] Opened");
        }
        else
        {
            GD.Print("[SkillTreeUI] Closed");
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.T)
            {
                ToggleUI();
            }
            else if (keyEvent.Keycode == Key.Escape && Visible)
            {
                ToggleUI();
            }
        }
    }
    
    public override void _ExitTree()
    {
        if (skillTreeSystem != null)
        {
            skillTreeSystem.OnNodeUnlocked -= OnNodeUnlocked;
            skillTreeSystem.OnSkillPointsChanged -= UpdatePointsDisplay;
        }
    }
}
