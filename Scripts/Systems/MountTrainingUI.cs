using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Data;

/// <summary>
/// 坐骑训练界面 - 显示和管理坐骑训练项目的UI
/// </summary>
public partial class MountTrainingUI : Control
{
    private MountTrainingSystem system;
    private MountTrainingDatabase database;
    
    // UI Elements
    private Control mainContainer;
    private TabContainer tabContainer;
    
    // Mount selection
    private OptionButton mountSelector;
    private Label mountLevelLabel;
    private Label mountBondLabel;
    private ProgressBar experienceBar;
    private ProgressBar bondBar;
    
    // Project lists
    private ItemList combatList;
    private ItemList speedList;
    private ItemList staminaList;
    private ItemList intelligenceList;
    private ItemList bondingList;
    private ItemList specialList;
    
    // Project details
    private Label projectNameLabel;
    private Label projectDescLabel;
    private Label projectRequirementsLabel;
    private Label projectRewardsLabel;
    private Label dailyLimitLabel;
    private Button trainButton;
    
    // Statistics
    private Label totalSessionsLabel;
    private Label totalExpLabel;
    private Label avgLevelLabel;
    private Label avgBondLabel;
    
    // Skills panel
    private ItemList skillsList;
    
    // Current selection
    private string currentMountId = "";
    private TrainingProject selectedProject;
    
    public override void _Ready()
    {
        system = MountTrainingSystem.Instance;
        database = MountTrainingDatabase.Instance;
        
        SetupUI();
        RefreshMountList();
    }
    
    private void SetupUI()
    {
        // Main container
        mainContainer = new Control();
        mainContainer.SetAnchor(0, 0, 1, 1);
        AddChild(mainContainer);
        
        // Background panel
        Panel bgPanel = new Panel();
        bgPanel.SetAnchor(0, 0, 1, 1);
        bgPanel.Modulate = new Color(1, 1, 1, 0.9f);
        mainContainer.AddChild(bgPanel);
        
        // Title
        Label title = new Label();
        title.Text = "Mount Training System";
        title.SetAnchor(0, 0, 0, 0);
        title.SetOffset(0, 10, 300, 50);
        title.Align = Label.AlignEnum.Center;
        title.AddColorOverride("font_color", new Color(1, 0.9, 0.5f));
        mainContainer.AddChild(title);
        
        // Mount selector
        Label mountLabel = new Label();
        mountLabel.Text = "Select Mount:";
        mountLabel.SetAnchor(0, 0, 0, 0);
        mountLabel.SetOffset(20, 60, 150, 90);
        mainContainer.AddChild(mountLabel);
        
        mountSelector = new OptionButton();
        mountSelector.SetAnchor(0, 0, 0, 0);
        mountSelector.SetOffset(130, 55, 350, 95);
        mountSelector.ItemSelected += _on_mount_selected;
        mainContainer.AddChild(mountSelector);
        
        // Level display
        mountLevelLabel = new Label();
        mountLevelLabel.Text = "Level: 1";
        mountLevelLabel.SetAnchor(0, 0, 0, 0);
        mountLevelLabel.SetOffset(370, 60, 500, 90);
        mainContainer.AddChild(mountLevelLabel);
        
        // Bond display
        mountBondLabel = new Label();
        mountBondLabel.Text = "Bond: Lv.1";
        mountBondLabel.SetAnchor(0, 0, 0, 0);
        mountBondLabel.SetOffset(520, 60, 650, 90);
        mainContainer.AddChild(mountBondLabel);
        
        // Experience bar
        Label expLabel = new Label();
        expLabel.Text = "Experience:";
        expLabel.SetAnchor(0, 0, 0, 0);
        expLabel.SetOffset(20, 100, 120, 130);
        mainContainer.AddChild(expLabel);
        
        experienceBar = new ProgressBar();
        experienceBar.SetAnchor(0, 0, 0, 0);
        experienceBar.SetOffset(120, 105, 350, 125);
        experienceBar.MinValue = 0;
        experienceBar.MaxValue = 100;
        experienceBar.Value = 0;
        mainContainer.AddChild(experienceBar);
        
        // Bond bar
        Label bondLabel = new Label();
        bondLabel.Text = "Bond:";
        bondLabel.SetAnchor(0, 0, 0, 0);
        bondLabel.SetOffset(370, 100, 430, 130);
        mainContainer.AddChild(bondLabel);
        
        bondBar = new ProgressBar();
        bondBar.SetAnchor(0, 0, 0, 0);
        bondBar.SetOffset(420, 105, 650, 125);
        bondBar.MinValue = 0;
        bondBar.MaxValue = 100;
        bondBar.Value = 0;
        mainContainer.AddChild(bondBar);
        
        // Tab container
        tabContainer = new TabContainer();
        tabContainer.SetAnchor(0, 0, 0, 0);
        tabContainer.SetOffset(20, 140, 680, 530);
        mainContainer.AddChild(tabContainer);
        
        // Training tabs
        SetupTrainingTab("Combat", TrainingCategory.Combat);
        SetupTrainingTab("Speed", TrainingCategory.Speed);
        SetupTrainingTab("Stamina", TrainingCategory.Stamina);
        SetupTrainingTab("Intelligence", TrainingCategory.Intelligence);
        SetupTrainingTab("Bonding", TrainingCategory.Bonding);
        SetupTrainingTab("Special", TrainingCategory.Special);
        
        // Project details tab
        SetupProjectDetailsTab();
        
        // Skills tab
        SetupSkillsTab();
        
        // Statistics tab
        SetupStatisticsTab();
        
        // Train button
        trainButton = new Button();
        trainButton.Text = "Start Training";
        trainButton.SetAnchor(0, 0, 0, 0);
        trainButton.SetOffset(550, 545, 650, 585);
        trainButton.Pressed += _on_train_pressed;
        mainContainer.AddChild(trainButton);
        
        // Close button
        Button closeBtn = new Button();
        closeBtn.Text = "Close";
        closeBtn.SetAnchor(0, 0, 0, 0);
        closeBtn.SetOffset(20, 545, 120, 585);
        closeBtn.Pressed += _on_close_pressed;
        mainContainer.AddChild(closeBtn);
        
        Visible = false;
    }
    
    private void SetupTrainingTab(string tabName, TrainingCategory category)
    {
        ScrollContainer scroll = new ScrollContainer();
        scroll.Name = tabName;
        tabContainer.AddChild(scroll);
        
        ItemList list = new ItemList();
        list.SetAnchor(0, 0, 1, 1);
        list.SetOffset(10, 10, -10, -10);
        list.ItemSelected += _on_project_selected;
        scroll.AddChild(list);
        
        switch (category)
        {
            case TrainingCategory.Combat: combatList = list; break;
            case TrainingCategory.Speed: speedList = list; break;
            case TrainingCategory.Stamina: staminaList = list; break;
            case TrainingCategory.Intelligence: intelligenceList = list; break;
            case TrainingCategory.Bonding: bondingList = list; break;
            case TrainingCategory.Special: specialList = list; break;
        }
    }
    
    private void SetupProjectDetailsTab()
    {
        Control detailsTab = new Control();
        detailsTab.Name = "Details";
        tabContainer.AddChild(detailsTab);
        
        projectNameLabel = new Label();
        projectNameLabel.Text = "Select a training project";
        projectNameLabel.SetAnchor(0, 0, 0, 0);
        projectNameLabel.SetOffset(20, 20, 400, 50);
        projectNameLabel.AddColorOverride("font_color", new Color(1, 0.9, 0.5f));
        detailsTab.AddChild(projectNameLabel);
        
        projectDescLabel = new Label();
        projectDescLabel.Text = "";
        projectDescLabel.SetAnchor(0, 0, 0, 0);
        projectDescLabel.SetOffset(20, 60, 600, 100);
        projectDescLabel.Autowrap = true;
        detailsTab.AddChild(projectDescLabel);
        
        projectRequirementsLabel = new Label();
        projectRequirementsLabel.Text = "";
        projectRequirementsLabel.SetAnchor(0, 0, 0, 0);
        projectRequirementsLabel.SetOffset(20, 110, 600, 160);
        detailsTab.AddChild(projectRequirementsLabel);
        
        projectRewardsLabel = new Label();
        projectRewardsLabel.Text = "";
        projectRewardsLabel.SetAnchor(0, 0, 0, 0);
        projectRewardsLabel.SetOffset(20, 170, 600, 250);
        projectRewardsLabel.AddColorOverride("font_color", new Color(0.5f, 1, 0.5f));
        detailsTab.AddChild(projectRewardsLabel);
        
        dailyLimitLabel = new Label();
        dailyLimitLabel.Text = "";
        dailyLimitLabel.SetAnchor(0, 0, 0, 0);
        dailyLimitLabel.SetOffset(20, 260, 400, 290);
        dailyLimitLabel.AddColorOverride("font_color", new Color(1, 0.5f, 0.5f));
        detailsTab.AddChild(dailyLimitLabel);
    }
    
    private void SetupSkillsTab()
    {
        Control skillsTab = new Control();
        skillsTab.Name = "Skills";
        tabContainer.AddChild(skillsTab);
        
        Label skillsTitle = new Label();
        skillsTitle.Text = "Unlocked Skills:";
        skillsTitle.SetAnchor(0, 0, 0, 0);
        skillsTitle.SetOffset(20, 20, 200, 50);
        skillsTab.AddChild(skillsTitle);
        
        skillsList = new ItemList();
        skillsList.SetAnchor(0, 0, 0, 0);
        skillsList.SetOffset(20, 60, 640, 350);
        skillsTab.AddChild(skillsList);
    }
    
    private void SetupStatisticsTab()
    {
        Control statsTab = new Control();
        statsTab.Name = "Statistics";
        tabContainer.AddChild(statsTab);
        
        totalSessionsLabel = new Label();
        totalSessionsLabel.Text = "Total Training Sessions: 0";
        totalSessionsLabel.SetAnchor(0, 0, 0, 0);
        totalSessionsLabel.SetOffset(20, 20, 400, 50);
        statsTab.AddChild(totalSessionsLabel);
        
        totalExpLabel = new Label();
        totalExpLabel.Text = "Total Experience Gained: 0";
        totalExpLabel.SetAnchor(0, 0, 0, 0);
        totalExpLabel.SetOffset(20, 60, 400, 90);
        statsTab.AddChild(totalExpLabel);
        
        avgLevelLabel = new Label();
        avgLevelLabel.Text = "Average Mount Level: 0";
        avgLevelLabel.SetAnchor(0, 0, 0, 0);
        avgLevelLabel.SetOffset(20, 100, 400, 130);
        statsTab.AddChild(avgLevelLabel);
        
        avgBondLabel = new Label();
        avgBondLabel.Text = "Average Bond Level: 0";
        avgBondLabel.SetAnchor(0, 0, 0, 0);
        avgBondLabel.SetOffset(20, 140, 400, 170);
        statsTab.AddChild(avgBondLabel);
        
        UpdateStatisticsDisplay();
    }
    
    private void RefreshMountList()
    {
        mountSelector.Clear();
        
        // Add some default mounts for demonstration
        string[] demoMounts = { "ThunderSteed", "ShadowPhoenix", "FrostWyvern", "InfernoLion", "AzureDragon" };
        
        for (int i = 0; i < demoMounts.Length; i++)
        {
            mountSelector.AddItem(demoMounts[i], i);
        }
        
        if (demoMounts.Length > 0)
        {
            mountSelector.Select(0);
            currentMountId = demoMounts[0];
            RefreshTrainingLists();
            UpdateMountDisplay();
        }
    }
    
    private void RefreshTrainingLists()
    {
        if (string.IsNullOrEmpty(currentMountId)) return;
        
        ClearAllLists();
        
        var combatProjects = system.GetProjectsByCategory(currentMountId, TrainingCategory.Combat);
        foreach (var p in combatProjects)
            combatList.AddItem($"{p.ProjectName} (Lv.{p.RequiredLevel})");
        
        var speedProjects = system.GetProjectsByCategory(currentMountId, TrainingCategory.Speed);
        foreach (var p in speedProjects)
            speedList.AddItem($"{p.ProjectName} (Lv.{p.RequiredLevel})");
        
        var staminaProjects = system.GetProjectsByCategory(currentMountId, TrainingCategory.Stamina);
        foreach (var p in staminaProjects)
            staminaList.AddItem($"{p.ProjectName} (Lv.{p.RequiredLevel})");
        
        var intProjects = system.GetProjectsByCategory(currentMountId, TrainingCategory.Intelligence);
        foreach (var p in intProjects)
            intelligenceList.AddItem($"{p.ProjectName} (Lv.{p.RequiredLevel})");
        
        var bondingProjects = system.GetProjectsByCategory(currentMountId, TrainingCategory.Bonding);
        foreach (var p in bondingProjects)
            bondingList.AddItem($"{p.ProjectName} (Lv.{p.RequiredLevel})");
        
        var specialProjects = system.GetProjectsByCategory(currentMountId, TrainingCategory.Special);
        foreach (var p in specialProjects)
            specialList.AddItem($"{p.ProjectName} (Lv.{p.RequiredLevel})");
    }
    
    private void ClearAllLists()
    {
        combatList.Clear();
        speedList.Clear();
        staminaList.Clear();
        intelligenceList.Clear();
        bondingList.Clear();
        specialList.Clear();
    }
    
    private void UpdateMountDisplay()
    {
        if (string.IsNullOrEmpty(currentMountId)) return;
        
        int level = system.GetMountLevel(currentMountId);
        int bondLevel = system.GetMountBondLevel(currentMountId);
        int expProgress = system.GetExperienceProgress(currentMountId);
        int bondProgress = system.GetBondProgress(currentMountId);
        
        mountLevelLabel.Text = $"Level: {level}";
        mountBondLabel.Text = $"Bond: Lv.{bondLevel}";
        experienceBar.Value = expProgress;
        bondBar.Value = bondProgress;
        
        // Update skills list
        skillsList.Clear();
        var skills = system.GetUnlockedSkills(currentMountId);
        foreach (string skill in skills)
            skillsList.AddItem(skill);
        
        UpdateStatisticsDisplay();
    }
    
    private void UpdateStatisticsDisplay()
    {
        var stats = system.GetStatistics();
        
        totalSessionsLabel.Text = $"Total Training Sessions: {stats["TotalTrainingSessions"]}";
        totalExpLabel.Text = $"Total Experience Gained: {stats["TotalExperienceGained"]}";
        
        if (stats.ContainsKey("AverageLevel"))
        {
            avgLevelLabel.Text = $"Average Mount Level: {stats["AverageLevel"]}";
            avgBondLabel.Text = $"Average Bond Level: {stats["AverageBondLevel"]}";
        }
    }
    
    private void UpdateProjectDetails(TrainingProject project)
    {
        if (project == null)
        {
            projectNameLabel.Text = "Select a training project";
            projectDescLabel.Text = "";
            projectRequirementsLabel.Text = "";
            projectRewardsLabel.Text = "";
            dailyLimitLabel.Text = "";
            trainButton.Disabled = true;
            return;
        }
        
        projectNameLabel.Text = project.ProjectName;
        projectDescLabel.Text = project.Description;
        
        string reqs = $"Required Level: {project.RequiredLevel}\nDuration: {project.DurationMinutes} minutes";
        if (project.RequiredSkills.Count > 0)
            reqs += $"\nRequired Skills: {string.Join(", ", project.RequiredSkills)}";
        projectRequirementsLabel.Text = reqs;
        
        string rewards = $"Experience: +{project.ExperienceReward}\nBond Points: +{project.BondPointsReward}";
        if (project.AttributeRewards.Count > 0)
        {
            rewards += "\nAttributes:";
            foreach (var attr in project.AttributeRewards)
                rewards += $"\n  {attr.Key}: +{attr.Value}";
        }
        projectRewardsLabel.Text = rewards;
        
        int remaining = system.GetRemainingDailyTraining(currentMountId, project.ProjectId);
        dailyLimitLabel.Text = $"Daily Uses Remaining: {remaining}/{project.DailyLimit}";
        
        trainButton.Disabled = remaining <= 0;
    }
    
    private void _on_mount_selected(int index)
    {
        currentMountId = mountSelector.GetItemText(index);
        RefreshTrainingLists();
        UpdateMountDisplay();
    }
    
    private void _on_project_selected(int index)
    {
        ItemList selectedList = null;
        
        // Determine which list was selected based on current tab
        int tabIndex = tabContainer.CurrentTab;
        string[] tabNames = { "Combat", "Speed", "Stamina", "Intelligence", "Bonding", "Special" };
        
        if (tabIndex >= 0 && tabIndex < tabNames.Length)
        {
            string currentTab = tabContainer.GetTabControl(tabIndex).Name;
            switch (currentTab)
            {
                case "Combat": selectedList = combatList; break;
                case "Speed": selectedList = speedList; break;
                case "Stamina": selectedList = staminaList; break;
                case "Intelligence": selectedList = intelligenceList; break;
                case "Bonding": selectedList = bondingList; break;
                case "Special": selectedList = specialList; break;
            }
        }
        
        if (selectedList == null) return;
        
        var projects = system.GetProjectsByCategory(currentMountId, 
            (TrainingCategory)tabIndex);
        
        if (index >= 0 && index < projects.Count)
        {
            selectedProject = projects[index];
            UpdateProjectDetails(selectedProject);
            tabContainer.CurrentTab = 6; // Details tab
        }
    }
    
    private void _on_train_pressed()
    {
        if (selectedProject == null || string.IsNullOrEmpty(currentMountId)) return;
        
        bool success = system.StartTraining(currentMountId, selectedProject.ProjectId);
        
        if (success)
        {
            GD.Print($"Training started: {selectedProject.ProjectName}");
            RefreshTrainingLists();
            UpdateMountDisplay();
            UpdateProjectDetails(selectedProject);
        }
        else
        {
            GD.PrintErr("Training failed!");
        }
    }
    
    private void _on_close_pressed()
    {
        Visible = false;
    }
    
    public void Toggle()
    {
        Visible = !Visible;
        if (Visible)
        {
            RefreshMountList();
            RefreshTrainingLists();
            UpdateMountDisplay();
        }
    }
}
