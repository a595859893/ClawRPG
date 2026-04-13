using Godot;
using System;
using System.Collections.Generic;

public partial class ArtifactFusionUI : Control
{
    private TabContainer tabContainer;
    private VBoxContainer recipeContainer;
    private VBoxContainer historyContainer;
    private VBoxContainer statsContainer;
    
    // 标签页
    private Control recipesTab;
    private Control historyTab;
    private Control statsTab;
    
    // 统计标签
    private Label totalFusionsLabel;
    private Label successRateLabel;
    private Label legendaryFusionsLabel;
    private Label goldSpentLabel;
    private Label recipesUnlockedLabel;
    
    private Button closeButton;
    
    public override void _Ready()
    {
        SetupUI();
        PopulateData();
        
        // 连接信号
        if (ArtifactFusionSystem.Instance != null)
        {
            ArtifactFusionSystem.Instance.OnFusionCompleted += OnFusionCompleted;
        }
        
        GD.Print("[ArtifactFusionUI] Initialized");
    }
    
    private void SetupUI()
    {
        // 主容器
        var mainPanel = new PanelContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetLeft = 200,
            OffsetTop = 100,
            OffsetRight = -200,
            OffsetBottom = -100
        };
        AddChild(mainPanel);
        
        var mainVBox = new VBoxContainer();
        mainPanel.AddChild(mainVBox);
        
        // 标题栏
        var titleBar = new HBoxContainer();
        mainVBox.AddChild(titleBar);
        
        var titleLabel = new Label
        {
            Text = "Artifact Fusion System",
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.Expand
        };
        titleBar.AddChild(titleLabel);
        
        closeButton = new Button
        {
            Text = "X",
            SizeFlagsHorizontal = SizeFlags.ShrinkEnd
        };
        closeButton.Pressed += OnClosePressed;
        titleBar.AddChild(closeButton);
        
        // TabContainer
        tabContainer = new TabContainer
        {
            SizeFlagsVertical = SizeFlags.Expand
        };
        mainVBox.AddChild(tabContainer);
        
        // 配方标签页
        recipesTab = new Control();
        recipesTab.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        tabContainer.AddChild(recipesTab);
        tabContainer.SetTabTitle(0, "Recipes");
        
        var recipesScroll = new ScrollContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetLeft = 10,
            OffsetTop = 10,
            OffsetRight = -10,
            OffsetBottom = -10
        };
        recipesTab.AddChild(recipesScroll);
        
        recipeContainer = new VBoxContainer();
        recipeContainer.SizeFlagsHorizontal = SizeFlags.Expand;
        recipesScroll.AddChild(recipeContainer);
        
        // 历史标签页
        historyTab = new Control();
        historyTab.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        tabContainer.AddChild(historyTab);
        tabContainer.SetTabTitle(1, "History");
        
        var historyScroll = new ScrollContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetLeft = 10,
            OffsetTop = 10,
            OffsetRight = -10,
            OffsetBottom = -10
        };
        historyTab.AddChild(historyScroll);
        
        historyContainer = new VBoxContainer();
        historyContainer.SizeFlagsHorizontal = SizeFlags.Expand;
        historyScroll.AddChild(historyContainer);
        
        // 统计标签页
        statsTab = new Control();
        statsTab.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        tabContainer.AddChild(statsTab);
        tabContainer.SetTabTitle(2, "Statistics");
        
        var statsVBox = new VBoxContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetLeft = 20,
            OffsetTop = 20,
            OffsetRight = -20,
            OffsetBottom = -20
        };
        statsTab.AddChild(statsVBox);
        
        totalFusionsLabel = new Label { Text = "Total Fusions: 0" };
        statsVBox.AddChild(totalFusionsLabel);
        
        successRateLabel = new Label { Text = "Success Rate: 0%" };
        statsVBox.AddChild(successRateLabel);
        
        legendaryFusionsLabel = new Label { Text = "Legendary Fusions: 0" };
        statsVBox.AddChild(legendaryFusionsLabel);
        
        goldSpentLabel = new Label { Text = "Total Gold Spent: 0" };
        statsVBox.AddChild(goldSpentLabel);
        
        recipesUnlockedLabel = new Label { Text = "Recipes Unlocked: 0" };
        statsVBox.AddChild(recipesUnlockedLabel);
        
        var resetButton = new Button
        {
            Text = "Reset Statistics",
            OffsetTop = 30
        };
        resetButton.Pressed += OnResetPressed;
        statsVBox.AddChild(resetButton);
        
        statsContainer = statsVBox;
    }
    
    private void PopulateData()
    {
        // 填充配方列表
        PopulateRecipes();
        
        // 填充历史记录
        PopulateHistory();
        
        // 填充统计数据
        UpdateStatistics();
    }
    
    private void PopulateRecipes()
    {
        foreach (var recipe in ArtifactFusionDatabase.Recipes)
        {
            var recipePanel = CreateRecipeCard(recipe);
            recipeContainer.AddChild(recipePanel);
        }
    }
    
    private Control CreateRecipeCard(ArtifactFusionDatabase.FusionRecipe recipe)
    {
        var panel = new PanelContainer { OffsetBottom = 10 };
        
        var vbox = new VBoxContainer();
        panel.AddChild(vbox);
        
        var nameLabel = new Label
        {
            Text = $"{recipe.Name} ({recipe.ResultRarity})",
            SizeFlagsHorizontal = SizeFlags.Expand
        };
        vbox.AddChild(nameLabel);
        
        var descLabel = new Label
        {
            Text = recipe.Description,
            Modulate = new Color(0.7f, 0.7f, 0.7f)
        };
        vbox.AddChild(descLabel);
        
        var infoLabel = new Label
        {
            Text = $"Requires: {recipe.Artifact1} + {recipe.Artifact2}",
            Modulate = new Color(0.8f, 0.8f, 0.8f)
        };
        vbox.AddChild(infoLabel);
        
        var costLabel = new Label
        {
            Text = $"Cost: {recipe.GoldCost} gold | Success Rate: {recipe.SuccessRate * 100:F1}% | Level: {recipe.RequiredLevel}",
            Modulate = new Color(1f, 0.9f, 0.5f)
        };
        vbox.AddChild(costLabel);
        
        // 融合按钮
        var fuseButton = new Button
        {
            Text = "Fuse",
            OffsetTop = 5
        };
        var recipeCopy = recipe;
        fuseButton.Pressed += () => OnFuseButtonPressed(recipeCopy.Id);
        vbox.AddChild(fuseButton);
        
        return panel;
    }
    
    private void PopulateHistory()
    {
        var history = ArtifactFusionSystem.Instance.GetFusionHistory(20);
        
        if (history.Count == 0)
        {
            var emptyLabel = new Label
            {
                Text = "No fusion history yet.",
                Modulate = new Color(0.5f, 0.5f, 0.5f)
            };
            historyContainer.AddChild(emptyLabel);
            return;
        }
        
        foreach (var record in history)
        {
            var recordPanel = CreateHistoryCard(record);
            historyContainer.AddChild(recordPanel);
        }
    }
    
    private Control CreateHistoryCard(FusionRecord record)
    {
        var panel = new PanelContainer { OffsetBottom = 5 };
        
        var hbox = new HBoxContainer();
        panel.AddChild(hbox);
        
        var statusIcon = new Label
        {
            Text = record.Success ? "✓" : "✗",
            Modulate = record.Success ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.2f),
            SizeFlagsHorizontal = SizeFlags.ShrinkStart,
            OffsetRight = 10
        };
        hbox.AddChild(statusIcon);
        
        var infoLabel = new Label
        {
            Text = $"{record.Artifact1} + {record.Artifact2} → {record.ResultArtifact} ({record.GoldSpent}g)",
            SizeFlagsHorizontal = SizeFlags.Expand
        };
        hbox.AddChild(infoLabel);
        
        return panel;
    }
    
    private void UpdateStatistics()
    {
        var stats = ArtifactFusionSystem.Instance.GetStatistics();
        float successRate = ArtifactFusionSystem.Instance.GetSuccessRate();
        
        totalFusionsLabel.Text = $"Total Fusions: {stats["total_fusions"]}";
        successRateLabel.Text = $"Success Rate: {successRate * 100:F1}%";
        legendaryFusionsLabel.Text = $"Legendary Fusions: {stats["legendary_fusions"]}";
        goldSpentLabel.Text = $"Total Gold Spent: {stats["total_gold_spent"]}";
        recipesUnlockedLabel.Text = $"Recipes Unlocked: {stats["recipes_unlocked"]}";
    }
    
    private void OnFuseButtonPressed(string recipeId)
    {
        GD.Print($"[ArtifactFusionUI] Fuse button pressed for recipe: {recipeId}");
        
        var result = ArtifactFusionSystem.Instance.PerformFusion(recipeId);
        
        if (result != null)
        {
            // 刷新UI
            RefreshUI();
        }
    }
    
    private void OnFusionCompleted(FusionRecord record)
    {
        RefreshUI();
    }
    
    private void RefreshUI()
    {
        // 清除并重新填充历史
        foreach (var child in historyContainer.GetChildren())
        {
            child.QueueFree();
        }
        PopulateHistory();
        
        // 更新统计
        UpdateStatistics();
    }
    
    private void OnClosePressed()
    {
        Hide();
        if (ArtifactFusionSystem.Instance != null)
        {
            ArtifactFusionSystem.Instance.SaveData();
        }
    }
    
    private void OnResetPressed()
    {
        ArtifactFusionSystem.Instance.ResetStatistics();
        UpdateStatistics();
        
        // 刷新历史
        foreach (var child in historyContainer.GetChildren())
        {
            child.QueueFree();
        }
        PopulateHistory();
    }
    
    public void Toggle()
    {
        if (Visible)
        {
            Hide();
        }
        else
        {
            Show();
            RefreshUI();
        }
    }
    
    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            if (ArtifactFusionSystem.Instance != null)
            {
                ArtifactFusionSystem.Instance.OnFusionCompleted -= OnFusionCompleted;
            }
        }
    }
}
