using Godot;
using System;
using System.Collections.Generic;
using MountEvolutionDataSpace;

public class MountEvolutionUI : Control
{
    private static MountEvolutionUI _instance;
    public static MountEvolutionUI Instance => _instance;

    // UI Components
    private Panel _mainPanel;
    private VBoxContainer _mountListContainer;
    private VBoxContainer _evolutionInfoContainer;
    private Label _titleLabel;
    private Label _mountNameLabel;
    private Label _stageLabel;
    private Label _typeLabel;
    private Label _descriptionLabel;
    private Label _expLabel;
    private ProgressBar _expProgressBar;
    private Label _progressLabel;
    private Button _evolveButton;
    private Label _costLabel;
    private RichTextLabel _requirementsLabel;
    private Button _closeButton;

    // Data
    private List<string> _availableMounts;
    private string _selectedMountId;
    private bool _isVisible = false;

    public override void _Ready()
    {
        _instance = this;
        SetupUI();
        Visible = false;
    }

    private void SetupUI()
    {
        // 主面板
        _mainPanel = new Panel
        {
            AnchorLeft = 0.5f,
            AnchorTop = 0.5f,
            AnchorRight = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = -400f,
            OffsetTop = -300f,
            OffsetRight = 400f,
            OffsetBottom = 300f,
            CustomMinimumSize = new Vector2(800, 600)
        };
        AddChild(_mainPanel);

        // 标题
        _titleLabel = new Label
        {
            Text = "坐骑进化系统",
            AnchorLeft = 0.5f,
            AnchorRight = 0.5f,
            OffsetTop = 10f,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _mainPanel.AddChild(_titleLabel);

        // 关闭按钮
        _closeButton = new Button
        {
            Text = "×",
            AnchorLeft = 1f,
            AnchorRight = 1f,
            OffsetLeft = -40f,
            OffsetTop = 5f,
            OffsetRight = -5f,
            OffsetBottom = 35f
        };
        _closeButton.Pressed += OnCloseButtonPressed;
        _mainPanel.AddChild(_closeButton);

        // 坐骑列表容器
        _mountListContainer = new VBoxContainer
        {
            AnchorLeft = 0f,
            AnchorTop = 0.1f,
            AnchorBottom = 1f,
            OffsetLeft = 20f,
            OffsetTop = 50f,
            OffsetRight = 200f,
            OffsetBottom = -20f
        };
        _mainPanel.AddChild(_mountListContainer);

        // 进化信息容器
        _evolutionInfoContainer = new VBoxContainer
        {
            AnchorLeft = 0.3f,
            AnchorTop = 0.1f,
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetLeft = 250f,
            OffsetTop = 50f,
            OffsetRight = -20f,
            OffsetBottom = -20f
        };
        _mainPanel.AddChild(_evolutionInfoContainer);

        // 坐骑名称
        _mountNameLabel = new Label
        {
            Text = "选择坐骑",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _mountNameLabel.AddThemeFontSizeOverride("font_size", 20);
        _mountListContainer.AddChild(_mountNameLabel);

        // 刷新坐骑列表
        RefreshMountList();
    }

    private void RefreshMountList()
    {
        // 清除现有按钮
        foreach (var child in _mountListContainer.GetChildren())
        {
            if (child is Button btn && btn != _mountNameLabel)
                btn.QueueFree();
        }

        // 获取可进化的坐骑列表
        _availableMounts = MountEvolutionSystem.Instance.GetEvolvableMounts();

        // 添加坐骑按钮
        foreach (var mountId in _availableMounts)
        {
            var mountName = MountEvolutionSystem.Instance.GetBaseMountName(mountId);
            var stage = MountEvolutionSystem.Instance.GetCurrentStage(mountId);
            var evolutionName = MountEvolutionSystem.Instance.GetEvolutionName(mountId);

            var btn = new Button
            {
                Text = $"{mountName}\n({GetStageName(stage)})",
                CustomMinimumSize = new Vector2(160, 60)
            };
            btn.Pressed += () => OnMountSelected(mountId);
            _mountListContainer.AddChild(btn);
        }

        // 如果有坐骑但没有选中，默认选中第一个
        if (_availableMounts.Count > 0 && string.IsNullOrEmpty(_selectedMountId))
        {
            OnMountSelected(_availableMounts[0]);
        }
    }

    private void OnMountSelected(string mountId)
    {
        _selectedMountId = mountId;
        UpdateEvolutionInfo();
    }

    private void UpdateEvolutionInfo()
    {
        // 清除现有内容
        foreach (var child in _evolutionInfoContainer.GetChildren())
        {
            child.QueueFree();
        }

        if (string.IsNullOrEmpty(_selectedMountId)) return;

        var baseName = MountEvolutionSystem.Instance.GetBaseMountName(_selectedMountId);
        var currentStage = MountEvolutionSystem.Instance.GetCurrentStage(_selectedMountId);
        var evolutionName = MountEvolutionSystem.Instance.GetEvolutionName(_selectedMountId);
        var description = MountEvolutionSystem.Instance.GetEvolutionDescription(_selectedMountId);
        var totalExp = MountEvolutionSystem.Instance.GetTotalBattleExp(_selectedMountId);
        var progress = MountEvolutionSystem.Instance.GetEvolutionProgress(_selectedMountId);
        var canEvolve = MountEvolutionSystem.Instance.CanEvolve(_selectedMountId);
        var requirements = MountEvolutionSystem.Instance.GetEvolutionRequirements(_selectedMountId);

        // 坐骑名称
        var nameLabel = new Label
        {
            Text = $"坐骑: {evolutionName}",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        nameLabel.AddThemeFontSizeOverride("font_size", 22);
        _evolutionInfoContainer.AddChild(nameLabel);

        // 当前阶段
        var stageLabel = new Label
        {
            Text = $"当前阶段: {GetStageName(currentStage)}",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        stageLabel.AddThemeFontSizeOverride("font_size", 16);
        _evolutionInfoContainer.AddChild(stageLabel);

        // 经验值
        _expLabel = new Label
        {
            Text = $"战斗经验: {totalExp}",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _expLabel.AddThemeFontSizeOverride("font_size", 14);
        _evolutionInfoContainer.AddChild(_expLabel);

        // 经验进度条
        _expProgressBar = new ProgressBar
        {
            Value = progress * 100f,
            CustomMinimumSize = new Vector2(400, 20)
        };
        _expProgressBar.AddThemeStyleboxOverride("fill", CreateProgressStyle());
        _evolutionInfoContainer.AddChild(_expProgressBar);

        // 进度百分比
        _progressLabel = new Label
        {
            Text = $"进化进度: {progress * 100f:F1}%",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _expProgressBar.AddChild(_progressLabel);

        // 描述
        var descLabel = new Label
        {
            Text = $"描述: {description}"
        };
        descLabel.AddThemeFontSizeOverride("font_size", 14);
        _evolutionInfoContainer.AddChild(descLabel);

        // 分割线
        _evolutionInfoContainer.AddChild(new HSeparator());

        // 进化需求
        if (requirements != null)
        {
            var reqTitle = new Label
            {
                Text = "下一阶段进化需求:",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            reqTitle.AddThemeFontSizeOverride("font_size", 16);
            _evolutionInfoContainer.AddChild(reqTitle);

            var reqLabel = new Label
            {
                Text = $"• 等级需求: {requirements.RequiredLevel}\n" +
                       $"• 战斗经验: {requirements.RequiredBattleExp}\n" +
                       $"• 金币: {requirements.GoldCost}",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            reqLabel.AddThemeFontSizeOverride("font_size", 14);
            _evolutionInfoContainer.AddChild(reqLabel);

            // 物品需求
            if (requirements.RequiredItemId > 0)
            {
                var itemName = GetItemName(requirements.RequiredItemId);
                var itemCount = requirements.RequiredItemCount;
                var hasItem = HasItem(requirements.RequiredItemId, requirements.RequiredItemCount);
                
                var itemLabel = new Label
                {
                    Text = $"• 物品需求: {itemName} x{itemCount} {(hasItem ? "✓" : "✗")}",
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                itemLabel.AddThemeFontSizeOverride("font_size", 14);
                if (!hasItem)
                    itemLabel.Modulate = new Color(1f, 0.5f, 0.5f);
                _evolutionInfoContainer.AddChild(itemLabel);
            }

            // 属性加成预览
            var bonusTitle = new Label
            {
                Text = "进化后属性加成:",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            bonusTitle.AddThemeFontSizeOverride("font_size", 16);
            _evolutionInfoContainer.AddChild(bonusTitle);

            var bonusText = "";
            if (requirements.HealthBonus > 0) bonusText += $"• 生命 +{requirements.HealthBonus * 100f:F0}%\n";
            if (requirements.AttackBonus > 0) bonusText += $"• 攻击 +{requirements.AttackBonus * 100f:F0}%\n";
            if (requirements.DefenseBonus > 0) bonusText += $"• 防御 +{requirements.DefenseBonus * 100f:F0}%\n";
            if (requirements.SpeedBonus > 0) bonusText += $"• 速度 +{requirements.SpeedBonus * 100f:F0}%\n";

            var bonusLabel = new Label
            {
                Text = bonusText,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            bonusLabel.AddThemeFontSizeOverride("font_size", 14);
            _evolutionInfoContainer.AddChild(bonusLabel);

            // 进化按钮
            _evolveButton = new Button
            {
                Text = canEvolve ? "进化" : "条件不足",
                Disabled = !canEvolve,
                CustomMinimumSize = new Vector2(200, 50)
            };
            _evolveButton.AddThemeFontSizeOverride("font_size", 18);
            if (canEvolve)
            {
                _evolveButton.Pressed += OnEvolveButtonPressed;
            }
            _evolutionInfoContainer.AddChild(_evolveButton);
        }
        else
        {
            var maxLabel = new Label
            {
                Text = "已达成最高进化阶段!",
                HorizontalAlignment = HorizontalAlignment.Center,
                Modulate = new Color(1f, 0.8f, 0.2f)
            };
            maxLabel.AddThemeFontSizeOverride("font_size", 18);
            _evolutionInfoContainer.AddChild(maxLabel);
        }
    }

    private StyleBoxFlat CreateProgressStyle()
    {
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.2f, 0.4f, 0.2f);
        style.CornerRadiusTopLeft = 5f;
        style.CornerRadiusTopRight = 5f;
        style.CornerRadiusBottomLeft = 5f;
        style.CornerRadiusBottomRight = 5f;
        return style;
    }

    private void OnEvolveButtonPressed()
    {
        if (string.IsNullOrEmpty(_selectedMountId)) return;

        var success = MountEvolutionSystem.Instance.Evolve(_selectedMountId);
        if (success)
        {
            UpdateEvolutionInfo();
            
            // 显示成功提示
            var notification = new Label
            {
                Text = "进化成功!",
                HorizontalAlignment = HorizontalAlignment.Center,
                Modulate = new Color(0.5f, 1f, 0.5f)
            };
            notification.AddThemeFontSizeOverride("font_size", 24);
            _evolutionInfoContainer.AddChild(notification);
            
            // 延迟刷新
            var timer = GetTree().CreateTimer(2f);
            timer.Timeout += () => 
            {
                notification.QueueFree();
                UpdateEvolutionInfo();
            };
        }
    }

    private void OnCloseButtonPressed()
    {
        ToggleUI();
    }

    public void ToggleUI()
    {
        _isVisible = !_isVisible;
        Visible = _isVisible;
        
        if (_isVisible)
        {
            RefreshMountList();
            UpdateEvolutionInfo();
        }
    }

    public static string GetStageName(MountEvolutionData.EvolutionStage stage)
    {
        switch (stage)
        {
            case MountEvolutionData.EvolutionStage.Basic: return "基础";
            case MountEvolutionData.EvolutionStage.Advanced: return "进阶";
            case MountEvolutionData.EvolutionStage.Elite: return "精英";
            case MountEvolutionData.EvolutionStage.Epic: return "史诗";
            case MountEvolutionData.EvolutionStage.Legendary: return "传奇";
            default: return "未知";
        }
    }

    private string GetItemName(int itemId)
    {
        // 物品ID到名称的映射
        switch (itemId)
        {
            case 1001: return "圣光之羽";
            case 1002: return "天使之羽";
            case 1003: return "光明神印";
            case 1011: return "暗影之石";
            case 1012: return "深渊魔晶";
            case 1013: return "毁灭本源";
            case 1021: return "冰晶";
            case 1022: return "永恒冰晶";
            case 1023: return "冰封王座";
            case 1031: return "强化钢板";
            case 1032: return "山岳之心";
            case 1033: return "自然之源";
            case 1041: return "雷电精华";
            case 1042: return "苍穹之雷";
            case 1043: return "雷霆本源";
            case 1051: return "火焰之心";
            case 1052: return "熔岩核心";
            case 1053: return "炎帝之印";
            case 1097: return "炎狱核心";
            case 1098: return "灭世之源";
            case 1099: return "创世神晶";
            default: return $"物品{itemId}";
        }
    }

    private bool HasItem(int itemId, int count)
    {
        var item = InventorySystem.Instance.GetItem(itemId);
        return item != null && item.Quantity >= count;
    }

    public override void _Input(InputEvent event2)
    {
        if (event2 is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // J键切换显示
            if (keyEvent.Keycode == Key.J)
            {
                ToggleUI();
            }
        }
    }
}
