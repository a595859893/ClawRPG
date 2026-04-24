using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 战利品掉落UI - 显示掉落统计和幸运值控制
/// 允许玩家调整幸运加成并查看掉落统计信息
/// </summary>
public partial class LootDropUI : Control
{
    private Label _titleLabel;
    private VBoxContainer _statsContainer;
    private VBoxContainer _rarityContainer;
    private VBoxContainer _typeContainer;
    private Button _closeButton;
    private HSlider _luckSlider;
    private Label _luckValueLabel;
    
    private bool _isVisible = false; 

    public override void _Ready()
    {
        SetupUI();
        Visible = false; 
    }

    private void SetupUI()
    {
        // Main panel
        var panel = new PanelContainer();
        panel.SetAnchor(AnchorPresets.Center);
        panel.OffsetLeft = -300;
        panel.OffsetRight = 300;
        panel.OffsetTop = -250;
        panel.OffsetBottom = 250;
        panel.Modulate = new Color(1, 1, 1, 0.95f);
        AddChild(panel);

        var mainVBox = new VBoxContainer();
        mainVBox.SetConstantSeparation(10);
        panel.AddChild(mainVBox);

        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "  📦 Loot Statistics  ";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainVBox.AddChild(_titleLabel);

        // Luck control
        var luckContainer = new HBoxContainer();
        mainVBox.AddChild(luckContainer);
        
        var luckLabel = new Label();
        luckLabel.Text = "Luck Bonus:";
        luckContainer.AddChild(luckLabel);
        
        _luckSlider = new HSlider();
        _luckSlider.MinValue = 0;
        _luckSlider.MaxValue = 10;
        _luckSlider.Step = 0.5f;
        _luckSlider.Value = LootDropSystem.Instance.LuckValue;
        _luckSlider.CustomMinimumSize = new Vector2(200, 0);
        _luckSlider.ValueChanged += OnLuckSliderChanged;
        luckContainer.AddChild(_luckSlider);
        
        _luckValueLabel = new Label();
        _luckValueLabel.Text = _luckSlider.Value.ToString("F1");
        _luckValueLabel.CustomMinimumSize = new Vector2(50, 0);
        luckContainer.AddChild(_luckValueLabel);

        // Stats tabs
        var statsHBox = new HBoxContainer();
        statsHBox.SetConstantSeparation(20);
        mainVBox.AddChild(statsHBox);

        // Rarity stats
        var rarityVBox = new VBoxContainer();
        rarityVBox.SetConstantSeparation(5);
        statsHBox.AddChild(rarityVBox);
        
        var rarityTitle = new Label();
        rarityTitle.Text = "Rarity Distribution";
        rarityTitle.AddThemeFontSizeOverride("font_size", 16);
        rarityVBox.AddChild(rarityTitle);
        
        _rarityContainer = new VBoxContainer();
        _rarityContainer.SetConstantSeparation(3);
        rarityVBox.AddChild(_rarityContainer);

        // Type stats
        var typeVBox = new VBoxContainer();
        typeVBox.SetConstantSeparation(5);
        statsHBox.AddChild(typeVBox);
        
        var typeTitle = new Label();
        typeTitle.Text = "Type Distribution";
        typeTitle.AddThemeFontSizeOverride("font_size", 16);
        typeVBox.AddChild(typeTitle);
        
        _typeContainer = new VBoxContainer();
        _typeContainer.SetConstantSeparation(3);
        typeVBox.AddChild(_typeContainer);

        // Overall stats
        _statsContainer = new VBoxContainer();
        _statsContainer.SetConstantSeparation(5);
        mainVBox.AddChild(_statsContainer);

        // Close button
        _closeButton = new Button();
        _closeButton.Text = "Close";
        _closeButton.CustomMinimumSize = new Vector2(100, 40);
        _closeButton.Pressed += OnClosePressed;
        
        var buttonContainer = new HBoxContainer();
        buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
        buttonContainer.AddChild(_closeButton);
        mainVBox.AddChild(buttonContainer);

        // Connect signals
        LootDropSystem.Instance.OnLootDropped += OnLootDropped;
    }

    private void OnLuckSliderChanged(float value)
    {
        LootDropSystem.Instance.ResetLuck();
        LootDropSystem.Instance.AddLuck(value);
        _luckValueLabel.Text = value.ToString("F1");
        RefreshStats();
    }

    private void OnLootDropped(LootDropData.LootEntry loot, int quantity)
    {
        // Auto-refresh when loot drops (if UI is open)
        if (Visible)
        {
            RefreshStats();
        }
    }

    private void RefreshStats()
    {
        // Clear existing
        foreach (var child in _rarityContainer.GetChildren())
        {
            child.QueueFree();
        }
        foreach (var child in _typeContainer.GetChildren())
        {
            child.QueueFree();
        }
        foreach (var child in _statsContainer.GetChildren())
        {
            child.QueueFree();
        }

        var stats = LootDropSystem.Instance.GetStatistics();
        var rarityDist = LootDropSystem.Instance.GetRarityDistribution();

        // Overall stats
        var totalLabel = new Label();
        totalLabel.Text = $"Total Drops: {stats.TotalDrops}";
        _statsContainer.AddChild(totalLabel);

        var luckyLabel = new Label();
        luckyLabel.Text = $"Lucky Drops: {stats.LuckyDrops}";
        _statsContainer.AddChild(luckyLabel);

        var criticalLabel = new Label();
        criticalLabel.Text = $"Critical Drops: {stats.CriticalDrops}";
        _statsContainer.AddChild(criticalLabel);

        var luckLabel = new Label();
        luckLabel.Text = $"Luck Items Used: {stats.LuckItems}";
        _statsContainer.AddChild(luckLabel);

        // Rarity distribution
        var rarityColors = new Dictionary<LootDropData.LootRarity, Color>
        {
            { LootDropData.LootRarity.Common, Colors.Gray },
            { LootDropData.LootRarity.Uncommon, Colors.Green },
            { LootDropData.LootRarity.Rare, Colors.Blue },
            { LootDropData.LootRarity.Epic, Colors.Purple },
            { LootDropData.LootRarity.Legendary, Colors.Orange }
        };

        foreach (var kvp in rarityDist)
        {
            var label = new Label();
            LootDropData.LootRarity rarity;
            if (Enum.TryParse<LootDropData.LootRarity>(kvp.Key, out rarity))
            {
                var color = rarityColors.ContainsKey(rarity) ? rarityColors[rarity] : Colors.White;
                label.Modulate = color;
            }
            label.Text = $"{kvp.Key}: {kvp.Value:F1}%";
            _rarityContainer.AddChild(label);
        }

        // Type distribution
        foreach (var kvp in stats.TypeDrops)
        {
            var label = new Label();
            float percentage = stats.TotalDrops > 0 ? (float)kvp.Value / stats.TotalDrops * 100f : 0;
            label.Text = $"{kvp.Key}: {percentage:F1}%";
            _typeContainer.AddChild(label);
        }
    }

    public void Toggle()
    {
        _isVisible = !_isVisible;
        Visible = _isVisible;
        
        if (_isVisible)
        {
            RefreshStats();
            // Update luck slider
            _luckSlider.Value = LootDropSystem.Instance.LuckValue;
            _luckValueLabel.Text = _luckSlider.Value.ToString("F1");
        }
    }

    private void OnClosePressed()
    {
        Toggle();
    }

    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.L)
        {
            Toggle();
        }
    }
}
