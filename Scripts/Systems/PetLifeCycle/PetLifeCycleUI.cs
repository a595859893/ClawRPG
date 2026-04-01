using Godot;
using System.Collections.Generic;

public partial class PetLifeCycleUI : Control
{
    private PetLifeCycleSystem _system;
    private TabContainer _tabContainer;
    private VBoxContainer _overviewTab;
    private VBoxContainer _petsTab;
    private VBoxContainer _historyTab;
    private VBoxContainer _statisticsTab;
    
    // 宠物列表
    private Tree _petsTree;
    
    public override void _Ready()
    {
        _system = GetNode<PetLifeCycleSystem>("/root/PetLifeCycleSystem");
        
        SetupUI();
    }
    
    private void SetupUI()
    {
        // 主容器
        var mainPanel = new PanelContainer();
        mainPanel.SetAnchorsPreset(Control.Preset.Center);
        mainPanel.CustomMinimumSize = new Vector2(800, 600);
        AddChild(mainPanel);
        
        var mainVBox = new VBoxContainer();
        mainPanel.AddChild(mainVBox);
        
        // 标题
        var title = new Label();
        title.Text = "  🐾 宠物生命周期系统  🐾  ";
        title.Align = Label.AlignEnum.Center;
        title.AddThemeFontSizeOverride("font_size", 24);
        mainVBox.AddChild(title);
        
        // Tab容器
        _tabContainer = new TabContainer();
        _tabContainer.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlags.Vertical);
        mainVBox.AddChild(_tabContainer);
        
        // 创建标签页
        CreateOverviewTab();
        CreatePetsTab();
        CreateHistoryTab();
        CreateStatisticsTab();
        
        // 底部按钮
        var buttonBox = new HBoxContainer();
        mainVBox.AddChild(buttonBox);
        
        var closeButton = new Button();
        closeButton.Text = "  关闭 (ESC)  ";
        closeButton.Pressed += OnClosePressed;
        buttonBox.AddChild(closeButton);
        
        var refreshButton = new Button();
        refreshButton.Text = "  刷新  ";
        refreshButton.Pressed += OnRefreshPressed;
        buttonBox.AddChild(refreshButton);
    }
    
    private void CreateOverviewTab()
    {
        _overviewTab = new VBoxContainer();
        _overviewTab.Name = "概览";
        _tabContainer.AddChild(_overviewTab);
        
        var title = new Label();
        title.Text = "宠物生命周期概览";
        title.Align = Label.AlignEnum.Center;
        title.AddThemeFontSizeOverride("font_size", 20);
        _overviewTab.AddChild(title);
        
        _overviewTab.AddChild(new HSeparator());
        
        // 统计信息
        var stats = _system.GetStatistics();
        
        var statsLabel = new Label();
        statsLabel.Text = $"""
        📊 生命周期统计:
        
        • 注册宠物总数: {stats["TotalLifeCycles"]}
        • 已离世宠物: {stats["TotalDeaths"]}
        • 生命延续次数: {stats["TotalLifeExtensions"]}
        • 最长生命周期: {stats["LongestLifeSpan"]} 天
        • 当前活跃宠物: {stats["ActivePets"]}
        """;
        _overviewTab.AddChild(statsLabel);
        
        _overviewTab.AddChild(new HSeparator());
        
        // 说明
        var infoLabel = new Label();
        infoLabel.Text = """
        📖 生命周期说明:
        
        宠物会经历以下阶段:
        • 🐣 婴儿期 (0-10%): 属性降低,需要照顾
        • 🌱 幼年期 (10-30%): 成长中,学习能力强
        • 💪 成年期 (30-70%): 巅峰状态
        • 🐕 老年期 (70-90%): 能力开始下降
        • 🌅 临终期 (90-100%): 最后的时光
        • ✨ 不朽: 超越生死
        
        生命延续:
        • 使用生命道具可延长宠物寿命
        • 不朽精华可以让宠物永生
        """;
        _overviewTab.AddChild(infoLabel);
    }
    
    private void CreatePetsTab()
    {
        _petsTab = new VBoxContainer();
        _petsTab.Name = "宠物";
        _tabContainer.AddChild(_petsTab);
        
        var title = new Label();
        title.Text = "宠物生命周期状态";
        title.Align = Label.AlignEnum.Center;
        title.AddThemeFontSizeOverride("font_size", 20);
        _petsTab.AddChild(title);
        
        _petsTab.AddChild(new HSeparator());
        
        // 宠物树形列表
        _petsTree = new Tree();
        _petsTree.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlags.Vertical);
        _petsTree.HideRoot = true;
        _petsTab.AddChild(_petsTree);
        
        // 添加测试宠物按钮
        var testButton = new Button();
        testButton.Text = "  添加测试宠物  ";
        testButton.Pressed += OnAddTestPetPressed;
        _petsTab.AddChild(testButton);
    }
    
    private void CreateHistoryTab()
    {
        _historyTab = new VBoxContainer();
        _historyTab.Name = "历史";
        _tabContainer.AddChild(_historyTab);
        
        var title = new Label();
        title.Text = "宠物生命周期历史";
        title.Align = Label.AlignEnum.Center;
        title.AddThemeFontSizeOverride("font_size", 20);
        _historyTab.AddChild(title);
        
        _historyTab.AddChild(new HSeparator());
        
        var historyLabel = new Label();
        historyLabel.Text = "宠物死亡历史记录将显示在这里";
        _historyTab.AddChild(historyLabel);
    }
    
    private void CreateStatisticsTab()
    {
        _statisticsTab = new VBoxContainer();
        _statisticsTab.Name = "统计";
        _tabContainer.AddChild(_statisticsTab);
        
        var title = new Label();
        title.Text = "详细统计数据";
        title.Align = Label.AlignEnum.Center;
        title.AddThemeFontSizeOverride("font_size", 20);
        _statisticsTab.AddChild(title);
        
        _statisticsTab.AddChild(new HSeparator());
        
        RefreshStatistics();
    }
    
    private void RefreshStatistics()
    {
        // 清除现有内容
        foreach (var child in _statisticsTab.GetChildren())
        {
            if (child is Label || child is HSeparator)
                child.QueueFree();
        }
        
        var stats = _system.GetStatistics();
        
        var statsText = $"""
        📈 详细统计:
        
        生命周期:
        • 注册总数: {stats["TotalLifeCycles"]}
        • 当前活跃: {stats["ActivePets"]}
        
        死亡统计:
        • 总死亡数: {stats["TotalDeaths"]}
        • 生命延续: {stats["TotalLifeExtensions"]}
        
        记录:
        • 最长生命: {stats["LongestLifeSpan"]} 天
        
        平均生命: {(stats["TotalDeaths"] > 0 ? (float)stats["LongestLifeSpan"] / stats["TotalDeaths"] : 0):F1} 天
        """;
        
        var label = new Label();
        label.Text = statsText;
        _statisticsTab.AddChild(label);
    }
    
    private void RefreshPetsList()
    {
        _petsTree.Clear();
        
        var root = _petsTree.CreateItem();
        root.SetText(0, "宠物列表");
        
        // 从系统获取宠物列表并显示
        if (_system != null && _system.GetData() != null)
        {
            var petCycles = _system.GetData().PetLifeCycles;
            if (petCycles != null && petCycles.Count > 0)
            {
                foreach (var kvp in petCycles)
                {
                    var pet = kvp.Value;
                    var petItem = _petsTree.CreateItem(root);
                    string stageName = pet.CurrentStage.ToString();
                    petItem.SetText(0, $"宠物 (ID: {pet.PetId}) - {stageName}");
                }
            }
            else
            {
                var emptyItem = _petsTree.CreateItem(root);
                emptyItem.SetText(0, "暂无宠物数据");
            }
        }
        else
        {
            var testPet = _petsTree.CreateItem(root);
            testPet.SetText(0, "测试宠物 (ID: 1) - 成年期");
        }
    }
    
    private void OnClosePressed()
    {
        QueueFree();
    }
    
    private void OnRefreshPressed()
    {
        RefreshStatistics();
        RefreshPetsList();
    }
    
    private void OnAddTestPetPressed()
    {
        // 添加测试宠物
        _system.RegisterPet(1, "测试宠物", "Dog");
        RefreshPetsList();
        GD.Print("[PetLifeCycleUI] 添加了测试宠物");
    }
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Scancode == KeyList.Escape)
        {
            QueueFree();
        }
    }
}
