using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PetExpeditionUI : Control
{
    private Control _mainPanel;
    private VBoxContainer _zoneContainer;
    private VBoxContainer _activeContainer;
    private VBoxContainer _historyContainer;
    private Label _statsLabel;
    private TabContainer _tabContainer;
    
    private Button _closeButton;
    private Label _titleLabel;
    
    // 当前显示的数据
    private string _selectedZoneId;
    private string _selectedPetId;
    
    public override void _Ready()
    {
        // 设置界面
        SetupUI();
        Visible = false; 
        
        // 连接到信号
        if (PetExpeditionSystem.Instance != null)
        {
            PetExpeditionSystem.Instance.OnExpeditionStarted += OnExpeditionStarted;
            PetExpeditionSystem.Instance.OnExpeditionCompleted += OnExpeditionCompleted;
        }
        
        // 添加到场景
        var main = GetNode("/root/Main/UI");
        if (main != null)
        {
            main.AddChild(this);
            RectPosition = new Vector2(200, 100);
            RectMinSize = new Vector2(800, 600);
        }
        
        GD.Print("Pet Expedition UI initialized");
    }
    
    private void SetupUI()
    {
        // 主背景
        var bg = new PanelContainer
        {
            AnchorRight = 1,
            AnchorBottom = 1,
            SelfModulate = new Color(0, 0, 0, 0.85f)
        };
        AddChild(bg);
        
        // 主容器
        _mainPanel = new VBoxContainer
        {
            AnchorRight = 1,
            AnchorBottom = 1,
            CustomConstants.Separation = 10
        };
        bg.AddChild(_mainPanel);
        
        // 标题栏
        var titleBar = new HBoxContainer
        {
            CustomConstants.Separation = 10
        };
        _mainPanel.AddChild(titleBar);
        
        _titleLabel = new Label
        {
            Text = "宠物远征系统",
            CustomFonts.Font = GD.Load<DynamicFont>("res://fonts/msyh.ttc"),
            CustomFontSizes.FontSize = 24
        };
        _titleLabel.AddColorOverride("font_color", new Color(1, 0.84f, 0));
        titleBar.AddChild(_titleLabel);
        
        titleBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        // 统计显示
        _statsLabel = new Label
        {
            Text = "总远征: 0 | 总金币: 0 | 总经验: 0",
            CustomFontSizes.FontSize = 14
        };
        _statsLabel.AddColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
        _mainPanel.AddChild(_statsLabel);
        
        // 分割线
        var separator = new HSeparator();
        _mainPanel.AddChild(separator);
        
        // 标签页容器
        _tabContainer = new TabContainer
        {
            SizeFlagsVertical = Control.SizeFlags.Expand
        };
        _mainPanel.AddChild(_tabContainer);
        
        // 创建标签页
        CreateZonesTab();
        CreateActiveTab();
        CreateHistoryTab();
        
        // 关闭按钮
        _closeButton = new Button
        {
            Text = "关闭",
            RectMinSize = new Vector2(100, 40)
        };
        _closeButton.Pressed += OnClosePressed;
        _mainPanel.AddChild(_closeButton);
        
        // 打开动画
        Show();
    }
    
    private void CreateZonesTab()
    {
        var scroll = new ScrollContainer
        {
            Name = "Zones"
        };
        _tabContainer.AddChild(scroll);
        
        var container = new VBoxContainer
        {
            CustomConstants.Separation = 10
        };
        container.SetAnchorAndMargin(AnchorRight, 1);
        container.SetAnchorAndMargin(AnchorBottom, 1);
        scroll.AddChild(container);
        
        _zoneContainer = container;
        
        RefreshZones();
    }
    
    private void CreateActiveTab()
    {
        var scroll = new ScrollContainer
        {
            Name = "Active"
        };
        _tabContainer.AddChild(scroll);
        
        var container = new VBoxContainer
        {
            CustomConstants.Separation = 10
        };
        container.SetAnchorAndMargin(AnchorRight, 1);
        container.SetAnchorAndMargin(AnchorBottom, 1);
        scroll.AddChild(container);
        
        _activeContainer = container;
        
        RefreshActiveExpeditions();
    }
    
    private void CreateHistoryTab()
    {
        var scroll = new ScrollContainer
        {
            Name = "History"
        };
        _tabContainer.AddChild(scroll);
        
        var container = new VBoxContainer
        {
            CustomConstants.Separation = 10
        };
        container.SetAnchorAndMargin(AnchorRight, 1);
        container.SetAnchorAndMargin(AnchorBottom, 1);
        scroll.AddChild(container);
        
        _historyContainer = container;
        
        RefreshHistory();
    }
    
    private void RefreshZones()
    {
        // 清除现有内容
        foreach (var child in _zoneContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        var player = GetNode<Player>("/root/Main/Player");
        int playerLevel = player != null ? player.Level : 1;
        
        var zones = PetExpeditionDatabase.Instance.GetZonesByLevel(playerLevel);
        
        foreach (var zone in zones)
        {
            var panel = CreateZonePanel(zone);
            _zoneContainer.AddChild(panel);
        }
    }
    
    private Control CreateZonePanel(GameSystems.ExpeditionZone zone)
    {
        var panel = new PanelContainer
        {
            CustomConstants/separation = 10
        };
        
        var hbox = new HBoxContainer
        {
            CustomConstants.Separation = 10
        };
        panel.AddChild(hbox);
        
        // 区域信息
        var info = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.Expand
        };
        hbox.AddChild(info);
        
        var nameLabel = new Label
        {
            Text = zone.Name,
            CustomFontSizes.FontSize = 18
        };
        nameLabel.AddColorOverride("font_color", new Color(1, 0.84f, 0));
        info.AddChild(nameLabel);
        
        var descLabel = new Label
        {
            Text = zone.Description,
            CustomFontSizes.FontSize = 14
        };
        descLabel.AddColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
        info.AddChild(descLabel);
        
        var statsLabel = new Label
        {
            Text = $"推荐等级: {zone.RecommendedLevel} | 时长: {zone.DurationMinutes}分钟 | 所需宠物: {zone.PetSlotsRequired}只",
            CustomFontSizes.FontSize = 12
        };
        statsLabel.AddColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
        info.AddChild(statsLabel);
        
        var rewardLabel = new Label
        {
            Text = $"金币: {zone.MinGoldReward}-{zone.MaxGoldReward} | 经验: {zone.MinExpReward}-{zone.MaxExpReward}",
            CustomFontSizes.FontSize = 12
        };
        rewardLabel.AddColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
        info.AddChild(rewardLabel);
        
        // 远征按钮
        var btnContainer = new VBoxContainer();
        hbox.AddChild(btnContainer);
        
        var startBtn = new Button
        {
            Text = "开始远征",
            RectMinSize = new Vector2(120, 40)
        };
        startBtn.Pressed += () => OnStartExpedition(zone.Id);
        
        // 检查是否可以开始
        var petManager = GetNode<PetManager>("/root/Main/PetManager");
        int availablePets = petManager != null ? petManager.GetAllPets().Count(p => 
            PetExpeditionSystem.Instance.GetPetExpedition(p.Id) == null) : 0;
        
        if (availablePets < zone.PetSlotsRequired)
        {
            startBtn.Disabled = true;
            startBtn.Text = "宠物不足";
        }
        
        btnContainer.AddChild(startBtn);
        
        // 完成次数
        var completions = 0;
        if (PetExpeditionSystem.Instance != null)
        {
            PetExpeditionSystem.Instance.PlayerData.ZoneCompletions.TryGetValue(zone.Id, out completions);
        }
        
        var completeLabel = new Label
        {
            Text = $"完成: {completions}次",
            CustomFontSizes.FontSize = 12,
            Alignment = Alignment.Center
        };
        completeLabel.AddColorOverride("font_color", new Color(0.5f, 0.8f, 0.5f));
        btnContainer.AddChild(completeLabel);
        
        return panel;
    }
    
    private void RefreshActiveExpeditions()
    {
        // 清除现有内容
        foreach (var child in _activeContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        if (PetExpeditionSystem.Instance == null) return;
        
        var active = PetExpeditionSystem.Instance.PlayerData.ActiveExpeditions.Where(e => !e.Completed).ToList();
        
        if (active.Count == 0)
        {
            var emptyLabel = new Label
            {
                Text = "没有进行中的远征",
                CustomFontSizes.FontSize = 16,
                Alignment = Alignment.Center
            };
            emptyLabel.AddColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            _activeContainer.AddChild(emptyLabel);
            return;
        }
        
        foreach (var expedition in active)
        {
            var zone = PetExpeditionDatabase.Instance.GetZone(expedition.ZoneId);
            var panel = new PanelContainer();
            _activeContainer.AddChild(panel);
            
            var hbox = new HBoxContainer
            {
                CustomConstants.Separation = 10
            };
            panel.AddChild(hbox);
            
            var info = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            hbox.AddChild(info);
            
            var nameLabel = new Label
            {
                Text = zone != null ? zone.Name : expedition.ZoneId,
                CustomFontSizes.FontSize = 16
            };
            nameLabel.AddColorOverride("font_color", new Color(1, 0.84f, 0));
            info.AddChild(nameLabel);
            
            var progress = PetExpeditionSystem.Instance.GetExpeditionProgress(expedition.ExpeditionId);
            var progressBar = new ProgressBar
            {
                Value = progress * 100,
                RectMinSize = new Vector2(200, 20)
            };
            progressBar.AddColorOverride("font_color", new Color(1, 1, 1));
            progressBar.AddColorOverride("font_color_disabled", new Color(1, 1, 1));
            info.AddChild(progressBar);
            
            var timeLabel = new Label
            {
                Text = $"{progress * 100:F0}%",
                CustomFontSizes.FontSize = 12
            };
            timeLabel.AddColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            info.AddChild(timeLabel);
            
            // 取消按钮
            var cancelBtn = new Button
            {
                Text = "取消",
                RectMinSize = new Vector2(80, 40)
            };
            cancelBtn.Pressed += () => OnCancelExpedition(expedition.ExpeditionId);
            hbox.AddChild(cancelBtn);
        }
    }
    
    private void RefreshHistory()
    {
        // 清除现有内容
        foreach (var child in _historyContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        if (PetExpeditionSystem.Instance == null) return;
        
        var history = PetExpeditionSystem.Instance.PlayerData.History;
        
        if (history.Count == 0)
        {
            var emptyLabel = new Label
            {
                Text = "没有远征历史",
                CustomFontSizes.FontSize = 16,
                Alignment = Alignment.Center
            };
            emptyLabel.AddColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            _historyContainer.AddChild(emptyLabel);
            return;
        }
        
        foreach (var result in history.Take(20))
        {
            var zone = PetExpeditionDatabase.Instance.GetZone(result.ZoneId);
            var panel = new PanelContainer();
            _historyContainer.AddChild(panel);
            
            var hbox = new HBoxContainer
            {
                CustomConstants.Separation = 10
            };
            panel.AddChild(hbox);
            
            var info = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            hbox.AddChild(info);
            
            var nameLabel = new Label
            {
                Text = (zone != null ? zone.Name : result.ZoneId) + (result.Success ? " ✓" : " ✗"),
                CustomFontSizes.FontSize = 14
            };
            nameLabel.AddColorOverride("font_color", result.Success ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.5f, 0.5f));
            info.AddChild(nameLabel);
            
            var rewardLabel = new Label
            {
                Text = $"金币: {result.GoldEarned} | 经验: {result.ExpEarned}" + 
                       (result.ItemsEarned.Count > 0 ? $" | 物品: {string.Join(", ", result.ItemsEarned)}" : ""),
                CustomFontSizes.FontSize = 12
            };
            rewardLabel.AddColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            info.AddChild(rewardLabel);
        }
    }
    
    private void RefreshStats()
    {
        if (PetExpeditionSystem.Instance == null) return;
        
        var stats = PetExpeditionSystem.Instance.GetStatistics();
        _statsLabel.Text = $"总远征: {stats["total_expeditions"]} | 总金币: {stats["total_gold_earned"]} | 总经验: {stats["total_exp_earned"]}";
    }
    
    public void Refresh()
    {
        RefreshZones();
        RefreshActiveExpeditions();
        RefreshHistory();
        RefreshStats();
    }
    
    private void OnStartExpedition(string zoneId)
    {
        if (PetExpeditionSystem.Instance == null) return;
        
        // 获取一个可用的宠物
        var petManager = GetNode<PetManager>("/root/Main/PetManager");
        if (petManager == null) return;
        
        var availablePet = petManager.GetAllPets().FirstOrDefault(p => 
            PetExpeditionSystem.Instance.GetPetExpedition(p.Id) == null);
        
        if (availablePet == null)
        {
            GD.Print("No available pets for expedition");
            return;
        }
        
        // 检查是否有坐骑
        var mountManager = GetNode<MountManager>("/root/Main/MountManager");
        if (mountManager != null)
        {
            // 有坐骑时尝试使用坐骑远征
            var mounts = mountManager.GetAllMounts();
            if (mounts.Count > 0)
            {
                var mount = mounts.FirstOrDefault();
                if (mount != null)
                {
                    // 使用坐骑远征
                    GD.Print($"Starting expedition with mount: {mount.Name}");
                }
            }
        }
        
        if (PetExpeditionSystem.Instance.StartExpedition(zoneId, availablePet.Id))
        {
            Refresh();
        }
    }
    
    private void OnCancelExpedition(string expeditionId)
    {
        if (PetExpeditionSystem.Instance == null) return;
        
        if (PetExpeditionSystem.Instance.CancelExpedition(expeditionId))
        {
            Refresh();
        }
    }
    
    private void OnExpeditionStarted(string expeditionId, string zoneId)
    {
        Refresh();
    }
    
    private void OnExpeditionCompleted(GameSystems.ExpeditionResult result)
    {
        Refresh();
    }
    
    private void OnClosePressed()
    {
        Visible = false; 
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Scancode == KeyList.Escape)
        {
            Visible = false; 
        }
    }
}
