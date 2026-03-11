using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public class PetEquipmentEnhancementUI : Control
{
    private Control _mainPanel;
    private VBoxContainer _equipmentList;
    private VBoxContainer _materialList;
    private Label _goldLabel;
    private Label _statsLabel;
    private Button _enhanceButton;
    private Label _resultLabel;
    
    private PetEquipmentSystem _petEquipmentSystem;
    private PetEquipmentEnhancementSystem _enhancementSystem;
    
    private string _selectedEquipmentId = "";
    private PetEquipmentEnhancementData.EnhancementTier _selectedTier = PetEquipmentEnhancementData.EnhancementTier.Basic;

    public override void _Ready()
    {
        _petEquipmentSystem = GetNode<PetEquipmentSystem>("/root/PetEquipmentSystem");
        _enhancementSystem = GetNode<PetEquipmentEnhancementSystem>("/root/PetEquipmentEnhancementSystem");
        
        SetupUI();
        ConnectSignals();
        RefreshUI();
    }

    private void SetupUI()
    {
        // Main panel
        _mainPanel = new Control();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(600, 500);
        AddChild(_mainPanel);

        // Background
        Panel bg = new Panel();
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        bg.Modulate = new Color(0, 0, 0, 0.8f);
        _mainPanel.AddChild(bg);

        // Title
        Label title = new Label();
        title.Text = "🐾 Pet Equipment Enhancement";
        title.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        title.Align = Label.AlignEnum.Center;
        title.Position = new Vector2(0, 10);
        title.AddThemeFontSizeOverride("font_size", 24);
        _mainPanel.AddChild(title);

        // Close button
        Button closeBtn = new Button();
        closeBtn.Text = "✕";
        closeBtn.Position = new Vector2(560, 10);
        closeBtn.Size = new Vector2(30, 30);
        closeBtn.Pressed += () => Hide();
        _mainPanel.AddChild(closeBtn);

        // Gold label
        _goldLabel = new Label();
        _goldLabel.Text = "Gold: 0";
        _goldLabel.Position = new Vector2(20, 50);
        _goldLabel.AddThemeFontSizeOverride("font_size", 18);
        _mainPanel.AddChild(_goldLabel);

        // Equipment scroll container
        ScrollContainer equipScroll = new ScrollContainer();
        equipScroll.Position = new Vector2(20, 80);
        equipScroll.Size = new Vector2(250, 300);
        _mainPanel.AddChild(equipScroll);

        _equipmentList = new VBoxContainer();
        _equipmentList.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        equipScroll.AddChild(_equipmentList);

        // Enhancement info panel
        VBoxContainer infoPanel = new VBoxContainer();
        infoPanel.Position = new Vector2(290, 80);
        infoPanel.Size = new Vector2(280, 300);
        _mainPanel.AddChild(infoPanel);

        // Selected equipment label
        Label selectedLabel = new Label();
        selectedLabel.Text = "Selected: None";
        selectedLabel.AddThemeFontSizeOverride("font_size", 16);
        infoPanel.AddChild(selectedLabel);

        // Current tier label
        Label tierLabel = new Label();
        tierLabel.Text = "Current Tier: None";
        tierLabel.AddThemeFontSizeOverride("font_size", 14);
        infoPanel.AddChild(tierLabel);

        // Material requirements
        Label materialTitle = new Label();
        materialTitle.Text = "Required Materials:";
        materialTitle.AddThemeFontSizeOverride("font_size", 14);
        materialTitle.Modulate = new Color(1, 0.9, 0.5);
        infoPanel.AddChild(materialTitle);

        _materialList = new VBoxContainer();
        infoPanel.AddChild(_materialList);

        // Enhancement tier selection
        Label tierSelectLabel = new Label();
        tierSelectLabel.Text = "Target Tier:";
        tierSelectLabel.AddThemeFontSizeOverride("font_size", 14);
        infoPanel.AddChild(tierSelectLabel);

        // Tier buttons
        HBoxContainer tierButtons = new HBoxContainer();
        infoPanel.AddChild(tierButtons);

        string[] tierNames = { "Basic", "Advanced", "Epic", "Legendary", "Mythic" };
        PetEquipmentEnhancementData.EnhancementTier[] tiers = {
            PetEquipmentEnhancementData.EnhancementTier.Basic,
            PetEquipmentEnhancementData.EnhancementTier.Advanced,
            PetEquipmentEnhancementData.EnhancementTier.Epic,
            PetEquipmentEnhancementData.EnhancementTier.Legendary,
            PetEquipmentEnhancementData.EnhancementTier.Mythic
        };

        for (int i = 0; i < tierNames.Length; i++)
        {
            Button tierBtn = new Button();
            tierBtn.Text = tierNames[i];
            tierBtn.Size = new Vector2(50, 30);
            int tierIndex = i;
            tierBtn.Pressed += () => SelectTier(tiers[tierIndex]);
            tierButtons.AddChild(tierBtn);
        }

        // Enhance button
        _enhanceButton = new Button();
        _enhanceButton.Text = "⚡ Enhance!";
        _enhanceButton.Position = new Vector2(290, 390);
        _enhanceButton.Size = new Vector2(280, 40);
        _enhanceButton.AddThemeFontSizeOverride("font_size", 20);
        _enhanceButton.Pressed += OnEnhancePressed;
        _mainPanel.AddChild(_enhanceButton);

        // Result label
        _resultLabel = new Label();
        _resultLabel.Text = "";
        _resultLabel.Position = new Vector2(290, 440);
        _resultLabel.Size = new Vector2(280, 40);
        _resultLabel.Align = Label.AlignEnum.Center;
        _resultLabel.AddThemeFontSizeOverride("font_size", 16);
        _mainPanel.AddChild(_resultLabel);

        // Stats panel
        Panel statsPanel = new Panel();
        statsPanel.Position = new Vector2(20, 390);
        statsPanel.Size = new Vector2(250, 90);
        _mainPanel.AddChild(statsPanel);

        _statsLabel = new Label();
        _statsLabel.Text = "Statistics:\nTotal: 0\nSuccess: 0\nCritical: 0\nFailed: 0";
        _statsLabel.Position = new Vector2(10, 10);
        _statsLabel.AddThemeFontSizeOverride("font_size", 12);
        statsPanel.AddChild(_statsLabel);

        // Update labels
        UpdateSelectedEquipment(selectedLabel, tierLabel);
        UpdateMaterials();
    }

    private void ConnectSignals()
    {
        if (_enhancementSystem != null)
        {
            _enhancementSystem.EnhancementSucceeded += OnEnhancementSucceeded;
            _enhancementSystem.EnhancementFailed += OnEnhancementFailed;
        }
    }

    private void SelectTier(PetEquipmentEnhancementData.EnhancementTier tier)
    {
        _selectedTier = tier;
        UpdateMaterials();
    }

    private void UpdateSelectedEquipment(Label selectedLabel, Label tierLabel)
    {
        if (string.IsNullOrEmpty(_selectedEquipmentId))
        {
            selectedLabel.Text = "Selected: None";
            tierLabel.Text = "Current Tier: None";
            return;
        }

        var equipment = _enhancementSystem.GetEquipmentEnhancement(_selectedEquipmentId);
        var item = PetEquipmentDatabase.GetPetEquipmentById(_selectedEquipmentId);
        
        selectedLabel.Text = $"Selected: {item?.Name ?? _selectedEquipmentId}";
        tierLabel.Text = $"Current Tier: {PetEquipmentEnhancementDatabase.GetTierName(equipment.Tier)}";
    }

    private void UpdateMaterials()
    {
        // Clear material list
        foreach (var child in _materialList.GetChildren())
        {
            child.QueueFree();
        }

        if (string.IsNullOrEmpty(_selectedEquipmentId))
        {
            return;
        }

        string equipmentType = GetEquipmentType(_selectedEquipmentId);
        var materials = PetEquipmentEnhancementDatabase.GetMaterialsForEnhancement(equipmentType, _selectedTier);

        int cost = PetEquipmentEnhancementDatabase.GetEnhancementCost(_selectedTier);
        float successRate = PetEquipmentEnhancementDatabase.GetSuccessRate(_selectedTier) * 100;
        
        foreach (var mat in materials)
        {
            int playerCount = InventoryManager.Instance.GetItemCount(mat.Id);
            bool hasEnough = playerCount >= mat.Quantity;
            
            Label matLabel = new Label();
            matLabel.Text = $"• {mat.Name}: {playerCount}/{mat.Quantity}";
            matLabel.Modulate = hasEnough ? new Color(0.5, 1, 0.5) : new Color(1, 0.5, 0.5);
            _materialList.AddChild(matLabel);
        }

        // Update gold display
        _goldLabel.Text = $"Gold: {Player.Instance.Gold} | Cost: {cost} | Success: {successRate:F0}%";
    }

    private string GetEquipmentType(string equipmentId)
    {
        if (equipmentId.Contains("collar")) return "collar";
        if (equipmentId.Contains("harness")) return "harness";
        if (equipmentId.Contains("armor")) return "armor";
        if (equipmentId.Contains("accessory")) return "accessory";
        if (equipmentId.Contains("toy")) return "toy";
        return "accessory";
    }

    private void RefreshUI()
    {
        // Clear equipment list
        foreach (var child in _equipmentList.GetChildren())
        {
            child.QueueFree();
        }

        // Get player's pet equipment
        var equipment = InventoryManager.Instance.GetItemsByType("pet_equipment");
        
        foreach (var item in equipment)
        {
            var eqData = item as ItemData;
            if (eqData == null) continue;

            Button eqButton = new Button();
            eqButton.Text = eqData.Name;
            eqButton.Size = new Vector2(230, 40);
            
            var enhancement = _enhancementSystem.GetEquipmentEnhancement(eqData.Id);
            string tierName = PetEquipmentEnhancementDatabase.GetTierName(enhancement.Tier);
            eqButton.Text = $"{eqData.Name}\n[{tierName}]";
            
            int eqTierIndex = (int)enhancement.Tier;
            switch (enhancement.Tier)
            {
                case PetEquipmentEnhancementData.EnhancementTier.Basic:
                    eqButton.Modulate = new Color(0.5, 1, 0.5);
                    break;
                case PetEquipmentEnhancementData.EnhancementTier.Advanced:
                    eqButton.Modulate = new Color(0.5, 0.8, 1);
                    break;
                case PetEquipmentEnhancementData.EnhancementTier.Epic:
                    eqButton.Modulate = new Color(0.6, 0.4, 0.9);
                    break;
                case PetEquipmentEnhancementData.EnhancementTier.Legendary:
                    eqButton.Modulate = new Color(1, 0.8, 0);
                    break;
                case PetEquipmentEnhancementData.EnhancementTier.Mythic:
                    eqButton.Modulate = new Color(1, 0.3, 0);
                    break;
            }
            
            string selectedId = eqData.Id;
            eqButton.Pressed += () => 
            {
                _selectedEquipmentId = selectedId;
                var selLabel = _mainPanel.GetNode<Label>("%selectedLabel");
                var tierLabel = _mainPanel.GetNode<Label>("%tierLabel");
                UpdateSelectedEquipment(selLabel, tierLabel);
                UpdateMaterials();
            };
            
            _equipmentList.AddChild(eqButton);
        }

        // Update stats
        var stats = _enhancementSystem.GetStatistics();
        _statsLabel.Text = $"Statistics:\n" +
            $"Total: {stats["totalEnhancements"]}\n" +
            $"Success: {stats["successCount"]} ({stats["successRate"]:P0})\n" +
            $"Critical: {stats["criticalCount"]}\n" +
            $"Failed: {stats["failureCount"]}\n" +
            $"Gold Spent: {stats["totalGoldSpent"]}";
    }

    private void OnEnhancePressed()
    {
        if (string.IsNullOrEmpty(_selectedEquipmentId))
        {
            _resultLabel.Text = "Please select equipment first!";
            _resultLabel.Modulate = new Color(1, 0.5, 0.5);
            return;
        }

        var result = _enhancementSystem.TryEnhance(_selectedEquipmentId, _selectedTier);
        
        switch (result)
        {
            case PetEquipmentEnhancementData.EnhancementResult.CriticalSuccess:
                _resultLabel.Text = "🎉 CRITICAL SUCCESS! +2 Tiers!";
                _resultLabel.Modulate = new Color(1, 0.8, 0);
                break;
            case PetEquipmentEnhancementData.EnhancementResult.Success:
                _resultLabel.Text = "✨ Enhancement Successful!";
                _resultLabel.Modulate = new Color(0.5, 1, 0.5);
                break;
            case PetEquipmentEnhancementData.EnhancementResult.Failure:
                _resultLabel.Text = "💥 Enhancement Failed!";
                _resultLabel.Modulate = new Color(1, 0.5, 0.5);
                break;
        }

        RefreshUI();
    }

    private void OnEnhancementSucceeded(string equipmentId, int newTier, bool isCritical)
    {
        GD.Print($"[PetEquipmentEnhancementUI] Enhancement succeeded: {equipmentId}, Tier: {newTier}, Critical: {isCritical}");
    }

    private void OnEnhancementFailed(string equipmentId, int currentTier)
    {
        GD.Print($"[PetEquipmentEnhancementUI] Enhancement failed: {equipmentId}, Current Tier: {currentTier}");
    }

    public void Show()
    {
        Visible = true;
        RefreshUI();
    }

    public void Hide()
    {
        Visible = false; 
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            Hide();
        }
    }
}
