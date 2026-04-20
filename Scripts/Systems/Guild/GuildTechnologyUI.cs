using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class GuildTechnologyUI : Control
{
    private VBoxContainer mainContainer;
    private HBoxContainer headerContainer;
    private Label titleLabel;
    private Label pointsLabel;
    private TabContainer tabContainer;

    // 科技列表
    private System.Collections.Generic.Dictionary<GuildTechnologyData.TechCategory, ItemList> categoryLists = new System.Collections.Generic.Dictionary<GuildTechnologyData.TechCategory, ItemList>();
    private System.Collections.Generic.Dictionary<GuildTechnologyData.TechCategory, Godot.Collections.Array> categoryTechs = new System.Collections.Generic.Dictionary<GuildTechnologyData.TechCategory, Godot.Collections.Array>();

    // 详情面板
    private Panel detailPanel;
    private Label detailNameLabel;
    private Label detailDescLabel;
    private Label detailLevelLabel;
    private Label detailBonusLabel;
    private Label detailCostLabel;
    private Label detailTimeLabel;
    private ProgressBar researchProgressBar;
    private Button researchButton;
    private Button cancelButton;

    // 当前选中的科技
    private string selectedTechId;
    private GuildTechnologyData.TechCategory currentCategory = GuildTechnologyData.TechCategory.Combat;

    // 刷新计时器
    private float refreshTimer = 0f;

    public override void _Ready()
    {
        SetupUI();
        ConnectSignals();
        RefreshTechList();
    }

    private void SetupUI()
    {
        // 主容器
        mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(FullRect);
        mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(mainContainer);

        // 标题栏
        headerContainer = new HBoxContainer();
        headerContainer.AddThemeConstantOverride("separation", 20);
        mainContainer.AddChild(headerContainer);

        titleLabel = new Label();
        titleLabel.Text = " 公会科技 ";
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        headerContainer.AddChild(titleLabel);

        headerContainer.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });

        pointsLabel = new Label();
        pointsLabel.Text = "可用科技点数: 0";
        pointsLabel.AddThemeFontSizeOverride("font_size", 18);
        headerContainer.AddChild(pointsLabel);

        // Tab 容器
        tabContainer = new TabContainer();
        tabContainer.SetHExpand(true);
        tabContainer.SetVExpand(true);
        tabContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
        mainContainer.AddChild(tabContainer);

        // 创建各类别的标签页
        CreateCategoryTab(GuildTechnologyData.TechCategory.Combat, "战斗");
        CreateCategoryTab(GuildTechnologyData.TechCategory.Economy, "经济");
        CreateCategoryTab(GuildTechnologyData.TechCategory.Production, "生产");
        CreateCategoryTab(GuildTechnologyData.TechCategory.Social, "社交");
        CreateCategoryTab(GuildTechnologyData.TechCategory.Defense, "防御");

        // 详情面板
        detailPanel = new Panel();
        detailPanel.SetHExpand(true);
        detailPanel.CustomMinimumSize = new Vector2(0, 200);
        mainContainer.AddChild(detailPanel);

        SetupDetailPanel();

        // 初始选择第一页
        tabContainer.CurrentTab = 0;
    }

    private void CreateCategoryTab(GuildTechnologyData.TechCategory category, string name)
    {
        ScrollContainer scroll = new ScrollContainer();
        scroll.Name = name;
        tabContainer.AddChild(scroll);

        VBoxContainer container = new VBoxContainer();
        container.AddThemeConstantOverride("separation", 5);
        scroll.AddChild(container);

        ItemList list = new ItemList();
        list.SetVExpand(true);
        list.SetHExpand(true);
        list.ItemSelected += OnItemSelected;
        container.AddChild(list);

        categoryLists[category] = list;

        // 存储科技数据
        var techs = GuildTechnologyDatabase.Instance.GetTechnologiesByCategory(category);
        Array techArray = new Godot.Collections.Array();
        foreach (var t in techs) techArray.Add(t);
        categoryTechs[category] = techArray;
    }

    private void SetupDetailPanel()
    {
        VBoxContainer container = new VBoxContainer();
        container.SetAnchorsPreset(FullRect);
        container.AddThemeConstantOverride("separation", 10);
        container.AddThemeConstantOverride("margin_left", 20);
        container.AddThemeConstantOverride("margin_top", 10);
        container.AddThemeConstantOverride("margin_right", -20);
        container.AddThemeConstantOverride("margin_bottom", -10);
        detailPanel.AddChild(container);

        // 名称
        detailNameLabel = new Label();
        detailNameLabel.AddThemeFontSizeOverride("font_size", 20);
        container.AddChild(detailNameLabel);

        // 描述
        detailDescLabel = new Label();
        detailDescLabel.AddThemeFontSizeOverride("font_size", 14);
        container.AddChild(detailDescLabel);

        // 等级
        detailLevelLabel = new Label();
        container.AddChild(detailLevelLabel);

        // 加成
        detailBonusLabel = new Label();
        container.AddChild(detailBonusLabel);

        // 成本
        detailCostLabel = new Label();
        container.AddChild(detailCostLabel);

        // 时间
        detailTimeLabel = new Label();
        container.AddChild(detailTimeLabel);

        // 研究进度条
        researchProgressBar = new ProgressBar();
        researchProgressBar.SetHExpand(true);
        researchProgressBar.Visible = false;
        container.AddChild(researchProgressBar);

        // 按钮容器
        HBoxContainer buttonContainer = new HBoxContainer();
        buttonContainer.AddThemeConstantOverride("separation", 10);
        container.AddChild(buttonContainer);

        researchButton = new Button();
        researchButton.Text = "开始研究";
        researchButton.Pressed += OnResearchPressed;
        buttonContainer.AddChild(researchButton);

        cancelButton = new Button();
        cancelButton.Text = "取消研究";
        cancelButton.Pressed += OnCancelPressed;
        cancelButton.Visible = false;
        buttonContainer.AddChild(cancelButton);
    }

    private void ConnectSignals()
    {
        GuildTechnologySystem.Instance.OnTechnologyLevelUp += OnTechLevelUp;
        GuildTechnologySystem.Instance.OnResearchComplete += OnResearchCompleted;
    }

    private void OnItemSelected(long index)
    {
        var techs = categoryTechs[currentCategory];
        if (index < 0 || index >= techs.Count) return;

        var tech = techs[index] as GuildTechnologyData.Technology;
        if (tech == null) return;

        selectedTechId = tech.Id;
        UpdateDetailPanel(tech);
    }

    private void UpdateDetailPanel(GuildTechnologyData.Technology tech)
    {
        var progress = GuildTechnologySystem.Instance.GetTechProgress(tech.Id);
        int currentLevel = progress.CurrentLevel;
        bool isMaxLevel = currentLevel >= tech.MaxLevel;
        bool isResearching = GuildTechnologySystem.Instance.IsResearching(tech.Id);

        detailNameLabel.Text = tech.Name;
        detailDescLabel.Text = tech.Description;
        detailLevelLabel.Text = $"当前等级: {currentLevel} / {tech.MaxLevel}";
        
        // 显示加成
        string bonusText = "加成: ";
        foreach (var bonus in tech.Bonuses)
        {
            float totalBonus = bonus.Value * currentLevel;
            bonusText += $"{bonus.Key} +{totalBonus*100:F1}% ";
        }
        detailBonusLabel.Text = bonusText;

        // 显示成本和时间
        if (!isMaxLevel)
        {
            int cost = GuildTechnologySystem.Instance.GetResearchCost(tech.Id, currentLevel + 1);
            detailCostLabel.Text = $"研究成本: {cost} 科技点数";
            detailTimeLabel.Text = $"研究时间: {tech.ResearchTime} 秒";
        }
        else
        {
            detailCostLabel.Text = "已满级";
            detailTimeLabel.Text = "";
        }

        // 更新按钮状态
        researchButton.Disabled = isMaxLevel || isResearching;
        researchButton.Text = isMaxLevel ? "已满级" : (isResearching ? "研究中..." : "开始研究");
        
        cancelButton.Visible = isResearching;
        researchProgressBar.Visible = isResearching;

        if (isResearching)
        {
            float progress_ = GuildTechnologySystem.Instance.GetResearchProgress(tech.Id);
            researchProgressBar.Value = progress_ * 100;
        }
    }

    private void OnResearchPressed()
    {
        if (string.IsNullOrEmpty(selectedTechId)) return;
        GuildTechnologySystem.Instance.StartResearch(selectedTechId);
        RefreshTechList();
    }

    private void OnCancelPressed()
    {
        if (string.IsNullOrEmpty(selectedTechId)) return;
        GuildTechnologySystem.Instance.CancelResearch(selectedTechId);
        RefreshTechList();
    }

    private void OnTechLevelUp(string techId, int newLevel)
    {
        RefreshTechList();
    }

    private void OnResearchCompleted(string techId)
    {
        RefreshTechList();
    }

    public override void _Process(double delta)
    {
        refreshTimer += delta;
        if (refreshTimer >= 1.0f)
        {
            refreshTimer = 0;
            RefreshPointsDisplay();
            UpdateResearchProgress();
        }
    }

    private void UpdateResearchProgress()
    {
        if (string.IsNullOrEmpty(selectedTechId)) return;

        var tech = GuildTechnologyDatabase.Instance.GetTechnology(selectedTechId);
        if (tech == null) return;

        bool isResearching = GuildTechnologySystem.Instance.IsResearching(selectedTechId);
        if (isResearching)
        {
            float progress = GuildTechnologySystem.Instance.GetResearchProgress(selectedTechId);
            researchProgressBar.Value = progress * 100;
        }
    }

    private void RefreshPointsDisplay()
    {
        pointsLabel.Text = $"可用科技点数: {GuildTechnologySystem.Instance.Data.AvailablePoints}";
    }

    private void RefreshTechList()
    {
        foreach (var kvp in categoryLists)
        {
            var category = kvp.Key;
            var list = kvp.Value;
            list.Clear();

            var techs = categoryTechs[category];
            foreach (GuildTechnologyData.Technology tech in techs)
            {
                int currentLevel = GuildTechnologySystem.Instance.GetCurrentLevel(tech.Id);
                string displayText = $"{tech.Name} (Lv.{currentLevel}/{tech.MaxLevel})";
                list.AddItem(displayText);
            }
        }

        // 更新详情面板
        if (!string.IsNullOrEmpty(selectedTechId))
        {
            var tech = GuildTechnologyDatabase.Instance.GetTechnology(selectedTechId);
            if (tech != null)
            {
                UpdateDetailPanel(tech);
            }
        }

        RefreshPointsDisplay();
    }

    public void Refresh()
    {
        RefreshTechList();
    }

    private void OnTabChanged(int tab)
    {
        currentCategory = (GuildTechnologyData.TechCategory)tab;
    }

    public override void _Input(InputEvent evt)
    {
        if (evt is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            Hide();
        }
    }
}
