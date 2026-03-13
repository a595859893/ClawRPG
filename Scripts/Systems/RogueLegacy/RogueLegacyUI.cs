using Godot;
using System.Collections.Generic;

public class RogueLegacyUI : Control
{
    private RogueLegacySystem _system;
    private RogueLegacyDatabase _database;
    
    // 主容器
    private VBoxContainer _mainContainer;
    private TabContainer _tabContainer;
    
    // 统计标签页
    private Label _statsLabel;
    
    // 升级标签页
    private VBoxContainer _upgradesContainer;
    private Label _pointsLabel;
    
    // 当前运行标签页
    private Label _runStatusLabel;
    private Label _floorLabel;
    private Label _goldLabel;
    private Label _expLabel;
    private Button _startRunButton;
    private Button _endRunButton;
    
    // 历史标签页
    private VBoxContainer _historyContainer;
    
    public override void _Ready()
    {
        _system = GetNode<RogueLegacySystem>("/root/RogueLegacySystem");
        _database = new RogueLegacyDatabase();
        
        SetupUI();
    }
    
    private void SetupUI()
    {
        // 主容器
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchorAndMargin(AnchorPreset.FullRect, 0);
        _mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(_mainContainer);
        
        // 标题
        var title = new Label();
        title.Text = "=== Rogue Legacy System ===";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        _mainContainer.AddChild(title);
        
        // 传承点数显示
        _pointsLabel = new Label();
        _pointsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _mainContainer.AddChild(_pointsLabel);
        
        // 标签页容器
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _mainContainer.AddChild(_tabContainer);
        
        // 创建标签页
        CreateOverviewTab();
        CreateUpgradesTab();
        CreateRunTab();
        CreateHistoryTab();
        
        // 关闭按钮
        var closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.Pressed += () => Hide();
        _mainContainer.AddChild(closeButton);
        
        // 更新UI
        UpdateUI();
    }
    
    private void CreateOverviewTab()
    {
        var scroll = new ScrollContainer();
        scroll.Name = "Overview";
        _tabContainer.AddChild(scroll);
        
        var container = new VBoxContainer();
        container.SetAnchorAndMargin(AnchorPreset.FullRect, 10);
        container.AddThemeConstantOverride("separation", 8);
        scroll.AddChild(container);
        
        // 统计信息
        var statsTitle = new Label();
        statsTitle.Text = "Statistics";
        statsTitle.AddThemeFontSizeOverride("font_size", 20);
        container.AddChild(statsTitle);
        
        _statsLabel = new Label();
        container.AddChild(_statsLabel);
        
        // 属性加成
        var bonusTitle = new Label();
        bonusTitle.Text = "\nPermanent Bonuses";
        bonusTitle.AddThemeFontSizeOverride("font_size", 20);
        container.AddChild(bonusTitle);
        
        var bonuses = _system.GetAttributeBonuses();
        var bonusLabel = new Label();
        bonusLabel.Text = $"Attack: +{bonuses["Attack"]}\n" +
                         $"Defense: +{bonuses["Defense"]}\n" +
                         $"Health: +{bonuses["Health"]}\n" +
                         $"Speed: +{bonuses["Speed"]}\n" +
                         $"Critical: +{bonuses["Critical"]}%";
        container.AddChild(bonusLabel);
    }
    
    private void CreateUpgradesTab()
    {
        var scroll = new ScrollContainer();
        scroll.Name = "Upgrades";
        _tabContainer.AddChild(scroll);
        
        _upgradesContainer = new VBoxContainer();
        _upgradesContainer.SetAnchorAndMargin(AnchorPreset.FullRect, 10);
        _upgradesContainer.AddThemeConstantOverride("separation", 10);
        scroll.AddChild(_upgradesContainer);
        
        // 分类升级
        string[] categories = { "Inheritance", "Attribute", "Special" };
        
        foreach (var category in categories)
        {
            var categoryLabel = new Label();
            categoryLabel.Text = $"=== {category} ===";
            categoryLabel.AddThemeFontSizeOverride("font_size", 18);
            _upgradesContainer.AddChild(categoryLabel);
            
            var upgrades = _system.GetUpgradesByCategory(category);
            foreach (var upgrade in upgrades)
            {
                var upgradePanel = CreateUpgradePanel(upgrade);
                _upgradesContainer.AddChild(upgradePanel);
            }
        }
    }
    
    private Control CreateUpgradePanel(InheritanceUpgrade upgrade)
    {
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(400, 80);
        
        var container = new VBoxContainer();
        container.AddThemeConstantOverride("separation", 5);
        panel.AddChild(container);
        
        // 升级名称和等级
        var nameLabel = new Label();
        int level = _system.GetUpgradeLevel(upgrade.Id);
        nameLabel.Text = $"{upgrade.Name} (Lv {level}/{upgrade.MaxLevel})";
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        container.AddChild(nameLabel);
        
        // 描述
        var descLabel = new Label();
        descLabel.Text = upgrade.Description;
        container.AddChild(descLabel);
        
        // 购买按钮
        var buttonContainer = new HBoxContainer();
        container.AddChild(buttonContainer);
        
        var cost = (int)(upgrade.BaseCost * Mathf.Pow(upgrade.CostScaling, level));
        var buyButton = new Button();
        buyButton.Text = $"Purchase ({cost} points)";
        
        if (level >= upgrade.MaxLevel)
        {
            buyButton.Text = "MAX";
            buyButton.Disabled = true;
        }
        
        buyButton.Pressed += () => 
        {
            if (_system.PurchaseUpgrade(upgrade.Id))
            {
                UpdateUI();
            }
        };
        
        buttonContainer.AddChild(buyButton);
        
        return panel;
    }
    
    private void CreateRunTab()
    {
        var container = new VBoxContainer();
        container.Name = "Current Run";
        container.SetAnchorAndMargin(AnchorPreset.FullRect, 10);
        container.AddThemeConstantOverride("separation", 10);
        _tabContainer.AddChild(container);
        
        // 运行状态
        var statusTitle = new Label();
        statusTitle.Text = "Run Status";
        statusTitle.AddThemeFontSizeOverride("font_size", 20);
        container.AddChild(statusTitle);
        
        _runStatusLabel = new Label();
        _runStatusLabel.Text = "Not in run";
        container.AddChild(_runStatusLabel);
        
        // 当前楼层
        var floorTitle = new Label();
        floorTitle.Text = "Floor Progress:";
        container.AddChild(floorTitle);
        
        _floorLabel = new Label();
        _floorLabel.Text = "1";
        container.AddChild(_floorLabel);
        
        // 当前金币
        var goldTitle = new Label();
        goldTitle.Text = "Gold Earned:";
        container.AddChild(goldTitle);
        
        _goldLabel = new Label();
        _goldLabel.Text = "0";
        container.AddChild(_goldLabel);
        
        // 当前经验
        var expTitle = new Label();
        expTitle.Text = "Experience Gained:";
        container.AddChild(expTitle);
        
        _expLabel = new Label();
        _expLabel.Text = "0";
        container.AddChild(_expLabel);
        
        // 继承预览
        var previewTitle = new Label();
        previewTitle.Text = "\nInheritance Preview:";
        container.AddChild(previewTitle);
        
        var previewLabel = new Label();
        var runData = _system.GetCurrentRunData();
        int goldInherit = (int)((int)runData["CurrentGold"] * (int)runData["GoldInheritancePercent"] / 100.0);
        int expInherit = (int)((int)runData["CurrentExperience"] * (int)runData["ExperienceInheritancePercent"] / 100.0);
        previewLabel.Text = $"Gold on death: {goldInherit}\n" +
                           $"Exp on death: {expInherit}";
        container.AddChild(previewLabel);
        
        // 按钮
        _startRunButton = new Button();
        _startRunButton.Text = "Start New Run";
        _startRunButton.Pressed += () => 
        {
            _system.StartRun();
            UpdateUI();
        };
        container.AddChild(_startRunButton);
        
        _endRunButton = new Button();
        _endRunButton.Text = "End Run (Death)";
        _endRunButton.Pressed += () => 
        {
            _system.EndRun(false);
            UpdateUI();
        };
        container.AddChild(_endRunButton);
        
        var completeButton = new Button();
        completeButton.Text = "Complete Run (Victory)";
        completeButton.Pressed += () => 
        {
            _system.EndRun(true);
            UpdateUI();
        };
        container.AddChild(completeButton);
    }
    
    private void CreateHistoryTab()
    {
        var scroll = new ScrollContainer();
        scroll.Name = "History";
        _tabContainer.AddChild(scroll);
        
        _historyContainer = new VBoxContainer();
        _historyContainer.SetAnchorAndMargin(AnchorPreset.FullRect, 10);
        _historyContainer.AddThemeConstantOverride("separation", 10);
        scroll.AddChild(_historyContainer);
    }
    
    public void UpdateUI()
    {
        // 更新点数显示
        var stats = _system.GetStatistics();
        _pointsLabel.Text = $"Legacy Points Available: {stats["LegacyPoints"]}";
        
        // 更新统计
        _statsLabel.Text = $"Total Deaths: {stats["TotalDeaths"]}\n" +
                          $"Runs Completed: {stats["RunsCompleted"]}\n" +
                          $"Total Points Earned: {stats["TotalPointsEarned"]}\n" +
                          $"Total Points Spent: {stats["TotalPointsSpent"]}\n" +
                          $"Highest Gold Inherited: {stats["HighestGoldInherited"]}\n" +
                          $"Highest Exp Inherited: {stats["HighestExpInherited"]}\n" +
                          $"Best Floor: {stats["BestFloor"]}\n" +
                          $"Longest Run: {stats["LongestRun"]}";
        
        // 更新运行状态
        var runData = _system.GetCurrentRunData();
        bool isActive = (bool)runData["IsActive"];
        
        if (isActive)
        {
            _runStatusLabel.Text = "Run in Progress!";
            _floorLabel.Text = $"Floor {runData["CurrentFloor"]}";
            _goldLabel.Text = runData["CurrentGold"].ToString();
            _expLabel.Text = runData["CurrentExperience"].ToString();
            _startRunButton.Disabled = true;
            _endRunButton.Disabled = false;
        }
        else
        {
            _runStatusLabel.Text = "Not in run";
            _floorLabel.Text = "-";
            _goldLabel.Text = "-";
            _expLabel.Text = "-";
            _startRunButton.Disabled = false;
            _endRunButton.Disabled = true;
        }
        
        // 更新历史
        UpdateHistory();
    }
    
    private void UpdateHistory()
    {
        // 清除旧内容
        foreach (Node child in _historyContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        var history = _system.GetRunHistory();
        
        var title = new Label();
        title.Text = "Run History";
        title.AddThemeFontSizeOverride("font_size", 18);
        _historyContainer.AddChild(title);
        
        if (history.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "No runs recorded yet.";
            _historyContainer.AddChild(emptyLabel);
            return;
        }
        
        foreach (var record in history)
        {
            var recordLabel = new Label();
            string status = record.Completed ? "✓ Completed" : "✗ Died";
            recordLabel.Text = $"Run #{record.RunNumber}: {status}\n" +
                              $"Floor: {record.FloorReached} | Gold: {record.GoldEarned} | XP: {record.ExperienceGained}\n" +
                              $"Inherited: {record.GoldInherited}g, {record.ExpInherited}xp | Points: +{record.LegacyPointsEarned}";
            _historyContainer.AddChild(recordLabel);
        }
    }
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            Hide();
        }
    }
}
