using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Equipment;

public class EquipmentDurabilityUI : Control
{
    private Control _mainPanel;
    private VBoxContainer _equipmentList;
    private Label _statisticsLabel;
    private Button _repairAllButton;
    private Button _closeButton;
    private CheckButton _autoRepairCheck;
    
    private bool _isVisible = false;
    private int _selectedEquipmentId = -1;
    
    public override void _Ready()
    {
        SetupUI();
        Hide();
    }
    
    private void SetupUI()
    {
        // Main panel
        _mainPanel = new Panel();
        _mainPanel.SetSize(new Vector2(600, 500));
        _mainPanel.Position = new Vector2(100, 100);
        AddChild(_mainPanel);
        
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchorAndMargin(Control.LayoutMarginFull, 0);
        mainContainer.Margin = new ColorRect(10, 10, 10, 10);
        _mainPanel.AddChild(mainContainer);
        
        // Title
        var titleLabel = new Label();
        titleLabel.Text = "装备耐久度 / Equipment Durability";
        titleLabel.Align = Label.AlignEnum.Center;
        mainContainer.AddChild(titleLabel);
        
        // Equipment list (scrollable)
        var scrollContainer = new ScrollContainer();
        scrollContainer.SetCustomMinimumSize(new Vector2(0, 300));
        mainContainer.AddChild(scrollContainer);
        
        _equipmentList = new VBoxContainer();
        scrollContainer.AddChild(_equipmentList);
        
        // Statistics
        var statsTitle = new Label();
        statsTitle.Text = "统计 / Statistics";
        statsTitle.Align = Label.AlignEnum.Center;
        mainContainer.AddChild(statsTitle);
        
        _statisticsLabel = new Label();
        _statisticsLabel.Text = "加载中...";
        _statisticsLabel.Autowrap = true;
        mainContainer.AddChild(_statisticsLabel);
        
        // Auto repair toggle
        var autoRepairContainer = new HBoxContainer();
        mainContainer.AddChild(autoRepairContainer);
        
        var autoRepairLabel = new Label();
        autoRepairLabel.Text = "自动修理: ";
        autoRepairContainer.AddChild(autoRepairLabel);
        
        _autoRepairCheck = new CheckButton();
        _autoRepairCheck.Text = "启用";
        autoRepairContainer.AddChild(_autoRepairCheck);
        
        // Buttons
        var buttonContainer = new HBoxContainer();
        mainContainer.AddChild(buttonContainer);
        
        _repairAllButton = new Button();
        _repairAllButton.Text = "修理全部 (100 金币)";
        _repairAllButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _repairAllButton.Connect("pressed", this, nameof(OnRepairAllPressed));
        buttonContainer.AddChild(_repairAllButton);
        
        _closeButton = new Button();
        _closeButton.Text = "关闭 (ESC)";
        _closeButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _closeButton.Connect("pressed", this, nameof(OnClosePressed));
        buttonContainer.AddChild(_closeButton);
        
        // Setup input
        SetupInput();
    }
    
    private void SetupInput()
    {
        // This would integrate with the game's input system
    }
    
    public override void _Input(InputEvent evt)
    {
        if (evt is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Scancode == KeyList.Escape)
        {
            if (_isVisible)
            {
                ToggleVisibility();
            }
        }
    }
    
    public void ToggleVisibility()
    {
        if (_isVisible)
        {
            Hide();
        }
        else
        {
            Show();
            RefreshData();
        }
        _isVisible = !_isVisible;
    }
    
    private void RefreshData()
    {
        // Clear existing items
        foreach (Node child in _equipmentList.GetChildren())
        {
            child.QueueFree();
        }
        
        var durabilitySystem = EquipmentDurabilitySystem.Instance;
        if (durabilitySystem == null)
        {
            _statisticsLabel.Text = "系统未初始化";
            return;
        }
        
        var allDurability = durabilitySystem.GetAllDurability();
        
        if (allDurability.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "没有装备数据\nNo equipment data";
            _equipmentList.AddChild(emptyLabel);
        }
        else
        {
            foreach (var kvp in allDurability)
            {
                var equipmentPanel = CreateEquipmentPanel(kvp.Key, kvp.Value);
                _equipmentList.AddChild(equipmentPanel);
            }
        }
        
        // Update statistics
        var stats = durabilitySystem.GetStatistics();
        _statisticsLabel.Text = $"受损物品: {stats["TotalItemsDamaged"]}\n" +
                               $"损坏物品: {stats["TotalItemsBroken"]}\n" +
                               $"修理次数: {stats["TotalRepairsPerformed"]}\n" +
                               $"修理总花费: {stats["TotalRepairCosts"]} 金币";
        
        // Update repair all button
        _repairAllButton.Text = $"修理全部 ({CalculateTotalRepairCost()} 金币)";
    }
    
    private Control CreateEquipmentPanel(int equipmentId, DurabilityData data)
    {
        var panel = new PanelContainer();
        panel.SetCustomMinimumSize(new Vector2(0, 60));
        
        var hbox = new HBoxContainer();
        panel.AddChild(hbox);
        
        // Equipment icon/placeholder
        var icon = new TextureRect();
        icon.SetCustomMinimumSize(new Vector2(40, 40));
        icon.Modulate = GetStateColor(data.IntegrityPercent);
        hbox.AddChild(icon);
        
        // Equipment info
        var infoVBox = new VBoxContainer();
        infoVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hbox.AddChild(infoVBox);
        
        var nameLabel = new Label();
        nameLabel.Text = $"装备 #{equipmentId}";
        infoVBox.AddChild(nameLabel);
        
        var durabilityLabel = new Label();
        durabilityLabel.Text = $"{data.CurrentDurability}/{data.MaxDurability} ({data.IntegrityPercent * 100:F1}%)";
        infoVBox.AddChild(durabilityLabel);
        
        // Progress bar
        var progressBar = new ProgressBar();
        progressBar.Value = data.IntegrityPercent * 100;
        progressBar.MaxValue = 100;
        progressBar.Modulate = GetStateColor(data.IntegrityPercent);
        infoVBox.AddChild(progressBar);
        
        // State label
        var stateLabel = new Label();
        var state = GetStateText(data.IntegrityPercent);
        stateLabel.Text = state;
        stateLabel.Modulate = GetStateColor(data.IntegrityPercent);
        hbox.AddChild(stateLabel);
        
        // Repair button
        var repairButton = new Button();
        repairButton.Text = "修理";
        repairButton.Connect("pressed", this, nameof(OnRepairPressed), new Godot.Collections.Array { equipmentId });
        hbox.AddChild(repairButton);
        
        return panel;
    }
    
    private Color GetStateColor(float percent)
    {
        if (percent >= 0.75f) return new Color(0.2f, 0.8f, 0.2f); // Green
        if (percent >= 0.50f) return new Color(0.8f, 0.8f, 0.2f); // Yellow
        if (percent >= 0.25f) return new Color(0.8f, 0.5f, 0.2f); // Orange
        return new Color(0.8f, 0.2f, 0.2f); // Red
    }
    
    private string GetStateText(float percent)
    {
        if (percent >= 0.75f) return "完美";
        if (percent >= 0.50f) return "良好";
        if (percent >= 0.25f) return "磨损";
        if (percent > 0) return "临界";
        return "损坏";
    }
    
    private int CalculateTotalRepairCost()
    {
        var durabilitySystem = EquipmentDurabilitySystem.Instance;
        if (durabilitySystem == null) return 0;
        
        int totalCost = 0;
        var allDurability = durabilitySystem.GetAllDurability();
        
        foreach (var kvp in allDurability)
        {
            int amount = kvp.Value.MaxDurability - kvp.Value.CurrentDurability;
            if (amount > 0)
            {
                totalCost += durabilitySystem.CalculateRepairCost(kvp.Key, amount);
            }
        }
        
        return totalCost;
    }
    
    private void OnRepairPressed(int equipmentId)
    {
        var durabilitySystem = EquipmentDurabilitySystem.Instance;
        if (durabilitySystem == null) return;
        
        var data = durabilitySystem.GetDurabilityData(equipmentId);
        if (data == null) return;
        
        int amount = data.MaxDurability - data.CurrentDurability;
        if (amount <= 0) return;
        
        int cost = durabilitySystem.CalculateRepairCost(equipmentId, amount);
        
        // Check player gold
        // int playerGold = PlayerInventory.GetGold();
        // if (playerGold < cost)
        // {
        //     ShowMessage("金币不足!");
        //     return;
        // }
        
        bool success = durabilitySystem.RepairEquipment(equipmentId, amount);
        if (success)
        {
            GD.Print($"Repaired equipment {equipmentId} for {cost} gold");
            RefreshData();
        }
    }
    
    private void OnRepairAllPressed()
    {
        int totalCost = CalculateTotalRepairCost();
        
        // Check player gold
        // int playerGold = PlayerInventory.GetGold();
        // if (playerGold < totalCost)
        // {
        //     ShowMessage("金币不足!");
        //     return;
        // }
        
        var durabilitySystem = EquipmentDurabilitySystem.Instance;
        if (durabilitySystem == null) return;
        
        durabilitySystem.RepairAllEquipment();
        GD.Print($"Repaired all equipment for {totalCost} gold");
        RefreshData();
    }
    
    private void OnClosePressed()
    {
        ToggleVisibility();
    }
    
    // Public method to register equipment from other systems
    public void RegisterEquipment(int equipmentId, int maxDurability = 100)
    {
        var durabilitySystem = EquipmentDurabilitySystem.Instance;
        durabilitySystem?.RegisterEquipment(equipmentId, maxDurability);
    }
    
    // Handle equipment damage
    public void OnEquipmentUsed(int equipmentId)
    {
        var durabilitySystem = EquipmentDurabilitySystem.Instance;
        durabilitySystem?.ReduceFromSkillUse(equipmentId);
    }
    
    public void OnEquipmentCombatUse(int equipmentId, int damageDealt = 0)
    {
        var durabilitySystem = EquipmentDurabilitySystem.Instance;
        durabilitySystem?.ReduceFromCombat(equipmentId, damageDealt);
    }
}
