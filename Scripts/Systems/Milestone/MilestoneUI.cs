using Godot;
using System;
using System.Collections.Generic;

public class MilestoneUI : Control
{
    private MilestoneSystem _system = MilestoneSystem.Instance;
    private MilestoneDatabase _database = MilestoneDatabase.Instance;
    
    private TabContainer _tabContainer;
    private VBoxContainer _overviewTab;
    private VBoxContainer _milestonesTab;
    private VBoxContainer _statisticsTab;
    
    private OptionButton _categoryFilter;
    private CheckButton _showUnlockedOnly;
    private ItemList _milestoneList;
    private Label _detailLabel;
    private ProgressBar _overallProgress;
    private Label _overallLabel;
    
    private Dictionary<string, MilestoneData.MilestoneEntry> _displayedMilestones = new Dictionary<string, MilestoneData.MilestoneEntry>();
    
    public override void _Ready()
    {
        SetupUI();
        ConnectSignals();
        RefreshMilestones();
        UpdateStatistics();
    }
    
    private void SetupUI()
    {
        // Main container
        var mainVBox = new VBoxContainer();
        mainVBox.SetAnchorsPreset(Control.LayoutPreset.WideRect);
        mainVBox.MarginLeft = 50;
        mainVBox.MarginTop = 50;
        mainVBox.MarginRight = -50;
        mainVBox.MarginBottom = -50;
        AddChild(mainVBox);
        
        // Title
        var title = new Label();
        title.Text = "Milestones";
        title.Align = Label.AlignEnum.Center;
        title.AddFontOverride("font_size", 24);
        mainVBox.AddChild(title);
        
        // Overall progress
        var progressContainer = new HBoxContainer();
        mainVBox.AddChild(progressContainer);
        
        _overallLabel = new Label();
        _overallLabel.Text = "Overall Progress: ";
        progressContainer.AddChild(_overallLabel);
        
        _overallProgress = new ProgressBar();
        _overallProgress.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        progressContainer.AddChild(_overallProgress);
        
        // Filter container
        var filterContainer = new HBoxContainer();
        mainVBox.AddChild(filterContainer);
        
        var categoryLabel = new Label();
        categoryLabel.Text = "Category: ";
        filterContainer.AddChild(categoryLabel);
        
        _categoryFilter = new OptionButton();
        _categoryFilter.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _categoryFilter.AddItem("All Categories", 0);
        int idx = 1;
        foreach (var category in _database.GetCategories())
        {
            _categoryFilter.AddItem(category, idx);
            idx++;
        }
        filterContainer.AddChild(_categoryFilter);
        
        _showUnlockedOnly = new CheckButton();
        _showUnlockedOnly.Text = "Show Unlocked Only";
        filterContainer.AddChild(_showUnlockedOnly);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainVBox.AddChild(_tabContainer);
        
        // Overview tab
        _overviewTab = new VBoxContainer();
        _overviewTab.Name = "Overview";
        _tabContainer.AddChild(_overviewTab);
        
        SetupOverviewTab();
        
        // Milestones tab
        _milestonesTab = new VBoxContainer();
        _milestonesTab.Name = "Milestones";
        _tabContainer.AddChild(_milestonesTab);
        
        SetupMilestonesTab();
        
        // Statistics tab
        _statisticsTab = new VBoxContainer();
        _statisticsTab.Name = "Statistics";
        _tabContainer.AddChild(_statisticsTab);
        
        SetupStatisticsTab();
        
        // Close button
        var closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.Align = Button.AlignEnum.Center;
        closeButton.Connect("pressed", this, nameof(OnClosePressed));
        mainVBox.AddChild(closeButton);
    }
    
    private void SetupOverviewTab()
    {
        var scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _overviewTab.AddChild(scroll);
        
        var content = new VBoxContainer();
        content.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        scroll.AddChild(content);
        
        // Category progress
        foreach (var category in _database.GetCategories())
        {
            var catLabel = new Label();
            catLabel.Text = category;
            catLabel.AddFontOverride("font_size", 18);
            content.AddChild(catLabel);
            
            var catProgress = new ProgressBar();
            catProgress.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            content.AddChild(catProgress);
            
            var milestones = _system.GetMilestonesByCategory(category);
            int unlocked = 0;
            foreach (var m in milestones)
            {
                if (m.Unlocked) unlocked++;
            }
            catProgress.MaxValue = milestones.Count;
            catProgress.Value = unlocked;
            catProgress.CustomMinimumSize = new Vector2(0, 20);
        }
    }
    
    private void SetupMilestonesTab()
    {
        var hbox = new HBoxContainer();
        hbox.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _milestonesTab.AddChild(hbox);
        
        // Milestone list
        var listContainer = new VBoxContainer();
        listContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hbox.AddChild(listContainer);
        
        _milestoneList = new ItemList();
        _milestoneList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _milestoneList.Connect("item_selected", this, nameof(OnMilestoneSelected));
        listContainer.AddChild(_milestoneList);
        
        // Detail panel
        var detailContainer = new VBoxContainer();
        detailContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        detailContainer.CustomMinimumSize = new Vector2(300, 0);
        hbox.AddChild(detailContainer);
        
        _detailLabel = new Label();
        _detailLabel.Text = "Select a milestone to view details";
        _detailLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        detailContainer.AddChild(_detailLabel);
    }
    
    private void SetupStatisticsTab()
    {
        var scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _statisticsTab.AddChild(scroll);
        
        var content = new VBoxContainer();
        content.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        scroll.AddChild(content);
        
        var stats = _system.GetStatistics();
        
        AddStatRow(content, "Total Milestones", stats.TotalMilestones.ToString());
        AddStatRow(content, "Unlocked Milestones", stats.UnlockedMilestones.ToString());
        AddStatRow(content, "Bronze Milestones", stats.BronzeMilestones.ToString());
        AddStatRow(content, "Silver Milestones", stats.SilverMilestones.ToString());
        AddStatRow(content, "Gold Milestones", stats.GoldMilestones.ToString());
        AddStatRow(content, "Platinum Milestones", stats.PlatinumMilestones.ToString());
        AddStatRow(content, "Diamond Milestones", stats.DiamondMilestones.ToString());
        AddStatRow(content, "Legendary Milestones", stats.LegendaryMilestones.ToString());
        AddStatRow(content, "Total Gold Earned", stats.TotalGoldEarned.ToString("N0"));
        AddStatRow(content, "Total EXP Earned", stats.TotalExpEarned.ToString("N0"));
    }
    
    private void AddStatRow(Control parent, string label, string value)
    {
        var hbox = new HBoxContainer();
        parent.AddChild(hbox);
        
        var labelNode = new Label();
        labelNode.Text = label + ": ";
        labelNode.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hbox.AddChild(labelNode);
        
        var valueNode = new Label();
        valueNode.Text = value;
        valueNode.Align = Label.AlignEnum.Right;
        hbox.AddChild(valueNode);
    }
    
    private void ConnectSignals()
    {
        _categoryFilter.Connect("item_selected", this, nameof(OnCategorySelected));
        _showUnlockedOnly.Connect("toggled", this, nameof(OnFilterToggled));
    }
    
    private void RefreshMilestones()
    {
        _milestoneList.Clear();
        _displayedMilestones.Clear();
        
        string selectedCategory = null;
        int selectedIdx = _categoryFilter.Selected;
        if (selectedIdx > 0)
        {
            var categories = _database.GetCategories();
            if (selectedIdx <= categories.Count)
                selectedCategory = categories[selectedIdx - 1];
        }
        
        bool showUnlockedOnly = _showUnlockedOnly.Pressed;
        
        List<MilestoneData.MilestoneEntry> milestones;
        if (selectedCategory != null)
            milestones = _system.GetMilestonesByCategory(selectedCategory);
        else
            milestones = _system.GetAllMilestones();
        
        foreach (var milestone in milestones)
        {
            if (showUnlockedOnly && !milestone.Unlocked)
                continue;
                
            string displayText = $"[{GetTierEmoji(milestone.Tier)}] {milestone.Name}";
            if (milestone.Unlocked)
                displayText += " ✓";
                
            _milestoneList.AddItem(displayText);
            _displayedMilestones[_milestoneList.ItemCount - 1] = milestone;
        }
        
        UpdateOverallProgress();
    }
    
    private string GetTierEmoji(MilestoneData.MilestoneTier tier)
    {
        switch (tier)
        {
            case MilestoneData.MilestoneTier.Bronze: return "🥉";
            case MilestoneData.MilestoneTier.Silver: return "🥈";
            case MilestoneData.MilestoneTier.Gold: return "🥇";
            case MilestoneData.MilestoneTier.Platinum: return "💎";
            case MilestoneData.MilestoneTier.Diamond: return "💠";
            case MilestoneData.MilestoneTier.Legendary: return "🌟";
            default: return "⬜";
        }
    }
    
    private void UpdateOverallProgress()
    {
        var allMilestones = _system.GetAllMilestones();
        int total = allMilestones.Count;
        int unlocked = 0;
        foreach (var m in allMilestones)
        {
            if (m.Unlocked) unlocked++;
        }
        
        _overallProgress.MaxValue = total > 0 ? total : 1;
        _overallProgress.Value = unlocked;
        _overallLabel.Text = $"Overall Progress: {unlocked}/{total} ({unlocked * 100 / Math.Max(total, 1)}%)";
    }
    
    private void UpdateStatistics()
    {
        // Statistics tab is rebuilt when selected
    }
    
    private void OnMilestoneSelected(int index)
    {
        if (!_displayedMilestones.ContainsKey(index))
            return;
            
        var milestone = _displayedMilestones[index];
        var config = _database.Milestones[milestone.Id];
        
        string detail = $"{milestone.Name}\n\n";
        detail += $"Category: {milestone.Category}\n";
        detail += $"Tier: {milestone.Tier}\n\n";
        detail += $"{milestone.Description}\n\n";
        
        if (milestone.Unlocked)
        {
            detail += $"✓ UNLOCKED!\n";
            if (milestone.UnlockTime.HasValue)
                detail += $"Unlocked on: {milestone.UnlockTime.Value}\n";
            detail += "Rewards:\n";
            foreach (var reward in config.Rewards)
            {
                detail += $"  - {reward.Key}: {reward.Value}\n";
            }
        }
        else
        {
            detail += $"Progress: {milestone.CurrentValue}/{milestone.RequiredValue}\n";
            detail += $"Reward: {config.Rewards["gold"]} gold, {config.Rewards["exp"]} exp";
        }
        
        _detailLabel.Text = detail;
    }
    
    private void OnCategorySelected(int index)
    {
        RefreshMilestones();
    }
    
    private void OnFilterToggled(bool pressed)
    {
        RefreshMilestones();
    }
    
    private void OnClosePressed()
    {
        QueueFree();
    }
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            QueueFree();
        }
    }
}
