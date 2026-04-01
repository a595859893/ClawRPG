using Godot;
using System;
using System.Collections.Generic;

public partial class ArtifactUI : Control
{
    private ArtifactSystem _artifactSystem;
    
    // UI 组件
    private Label _titleLabel;
    private Label _statsLabel;
    private GridContainer _artifactGrid;
    private VBoxContainer _detailPanel;
    private Label _detailName;
    private Label _detailDescription;
    private Label _detailStats;
    private Label _detailType;
    private Label _detailRarity;
    private Button _equipButton;
    private Button _unequipButton;
    private Button _closeButton;
    
    // 当前选中的神器
    private string _selectedArtifact = "";
    
    // 颜色配置
    private Color rarityColorCommon = new Color(0.7f, 0.7f, 0.7f);
    private Color rarityColorUncommon = new Color(0.2f, 0.8f, 0.2f);
    private Color rarityColorRare = new Color(0.2f, 0.5f, 1.0f);
    private Color rarityColorEpic = new Color(0.6f, 0.3f, 0.9f);
    private Color rarityColorLegendary = new Color(1.0f, 0.6f, 0.0f);
    
    public override void _Ready()
    {
        // 获取系统
        _artifactSystem = GetNode<ArtifactSystem>("/root/Main/ArtifactSystem");
        
        SetupUI();
        ConnectSignals();
        
        // 初始刷新
        RefreshArtifactList();
    }
    
    private void SetupUI()
    {
        // 主容器
        var mainContainer = new HBoxContainer();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        mainContainer.AddThemeConstantOverride("separation", 20);
        AddChild(mainContainer);
        
        // 左侧：神器列表
        var leftPanel = new VBoxContainer();
        leftPanel.CustomMinimumSize = new Vector2(500, 0);
        mainContainer.AddChild(leftPanel);
        
        // 标题
        var titleContainer = new HBoxContainer();
        leftPanel.AddChild(titleContainer);
        
        _titleLabel = new Label();
        _titleLabel.Text = "  🗝️ Artifact System  神器系统";
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        titleContainer.AddChild(_titleLabel);
        
        // 统计信息
        _statsLabel = new Label();
        _statsLabel.Text = "Loading...";
        _statsLabel.AddThemeFontSizeOverride("font_size", 14);
        leftPanel.AddChild(_statsLabel);
        
        // 神器网格
        _artifactGrid = new GridContainer();
        _artifactGrid.Columns = 3;
        _artifactGrid.SizeFlagsVertical = Control.SizeFlags.Expand;
        _artifactGrid.AddThemeConstantOverride("h_separation", 10);
        _artifactGrid.AddThemeConstantOverride("v_separation", 10);
        
        var scrollContainer = new ScrollContainer();
        scrollContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
        leftPanel.AddChild(scrollContainer);
        scrollContainer.AddChild(_artifactGrid);
        
        // 右侧：详情面板
        _detailPanel = new VBoxContainer();
        _detailPanel.CustomMinimumSize = new Vector2(350, 0);
        mainContainer.AddChild(_detailPanel);
        
        // 详情标题
        var detailTitle = new Label();
        detailTitle.Text = "  Artifact Details 神器详情";
        detailTitle.AddThemeFontSizeOverride("font_size", 18);
        _detailPanel.AddChild(detailTitle);
        
        // 详细信息
        _detailName = new Label();
        _detailName.Text = "Select an artifact";
        _detailName.AddThemeFontSizeOverride("font_size", 20);
        _detailPanel.AddChild(_detailName);
        
        _detailRarity = new Label();
        _detailRarity.Text = "";
        _detailRarity.AddThemeFontSizeOverride("font_size", 14);
        _detailPanel.AddChild(_detailRarity);
        
        _detailType = new Label();
        _detailType.Text = "";
        _detailType.AddThemeFontSizeOverride("font_size", 14);
        _detailPanel.AddChild(_detailType);
        
        var spacer1 = new Control();
        spacer1.CustomMinimumSize = new Vector2(0, 20);
        _detailPanel.AddChild(spacer1);
        
        _detailDescription = new Label();
        _detailDescription.Text = "";
        _detailDescription.AutowrapMode = TextServer.AutowrapMode.Word;
        _detailDescription.AddThemeFontSizeOverride("font_size", 14);
        _detailPanel.AddChild(_detailDescription);
        
        var spacer2 = new Control();
        spacer2.CustomMinimumSize = new Vector2(0, 20);
        _detailPanel.AddChild(spacer2);
        
        _detailStats = new Label();
        _detailStats.Text = "";
        _detailStats.AddThemeFontSizeOverride("font_size", 14);
        _detailPanel.AddChild(_detailStats);
        
        // 装备按钮
        var buttonContainer = new HBoxContainer();
        buttonContainer.AddThemeConstantOverride("separation", 10);
        _detailPanel.AddChild(buttonContainer);
        
        _equipButton = new Button();
        _equipButton.Text = "  Equip  装备 ";
        _equipButton.CustomMinimumSize = new Vector2(150, 40);
        _equipButton.Pressed += OnEquipPressed;
        buttonContainer.AddChild(_equipButton);
        
        _unequipButton = new Button();
        _unequipButton.Text = "  Unequip  卸下 ";
        _unequipButton.CustomMinimumSize = new Vector2(150, 40);
        _unequipButton.Pressed += OnUnequipPressed;
        buttonContainer.AddChild(_unequipButton);
        
        // 关闭按钮
        _closeButton = new Button();
        _closeButton.Text = "  Close  关闭 (ESC) ";
        _closeButton.CustomMinimumSize = new Vector2(200, 40);
        _closeButton.Pressed += OnClosePressed;
        
        var closeContainer = new HBoxContainer();
        closeContainer.AddThemeConstantOverride("separation", 10);
        _detailPanel.AddChild(closeContainer);
        closeContainer.AddChild(_closeButton);
        
        // 添加一些空白让按钮靠下
        var spacer3 = new Control();
        spacer3.SizeFlagsVertical = Control.SizeFlags.Expand;
        _detailPanel.AddChild(spacer3);
    }
    
    private void ConnectSignals()
    {
        // 连接信号
        if (_artifactSystem != null)
        {
            _artifactSystem.ArtifactUnlocked += OnArtifactUnlocked;
            _artifactSystem.ArtifactEquipped += OnArtifactEquipped;
            _artifactSystem.ArtifactUnequipped += OnArtifactUnequipped;
        }
        
        // 输入处理
        SetProcessInput(true);
    }
    
    public override void _Input(InputEvent eventEvent)
    {
        if (eventEvent is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Escape)
            {
                OnClosePressed();
            }
        }
    }
    
    private void RefreshArtifactList()
    {
        // 清空网格
        foreach (Node child in _artifactGrid.GetChildren())
        {
            child.QueueFree();
        }
        
        var allArtifacts = _artifactSystem.GetAllArtifacts();
        
        // 添加所有神器卡片
        foreach (var kvp in allArtifacts)
        {
            var artifact = kvp.Value;
            var card = CreateArtifactCard(artifact);
            _artifactGrid.AddChild(card);
        }
        
        // 更新统计
        var stats = _artifactSystem.GetStats();
        _statsLabel.Text = $"Unlocked: {stats["unlocked_count"]}/{stats["total_artifacts"]} | " +
            $"Legendary: {stats["legendary"]} | Epic: {stats["epic"]} | Rare: {stats["rare"]}";
        
        // 更新详情面板
        UpdateDetailPanel();
    }
    
    private Control CreateArtifactCard(ArtifactData artifact)
    {
        var card = new Button();
        card.CustomMinimumSize = new Vector2(150, 80);
        
        var cardContainer = new VBoxContainer();
        card.AddChild(cardContainer);
        
        // 名称
        var nameLabel = new Label();
        nameLabel.Text = artifact.displayName;
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        nameLabel.AddThemeFontSizeOverride("font_size", 12);
        cardContainer.AddChild(nameLabel);
        
        // 类型
        var typeLabel = new Label();
        typeLabel.Text = artifact.type.ToString();
        typeLabel.HorizontalAlignment = HorizontalAlignment.Center;
        typeLabel.AddThemeFontSizeOverride("font_size", 10);
        cardContainer.AddChild(typeLabel);
        
        // 稀有度颜色
        Color rarityColor = GetRarityColor(artifact.rarity);
        
        // 样式
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = rarityColor * new Color(0.3f, 0.3f, 0.3f);
        styleBox.BorderColor = rarityColor;
        styleBox.SetBorderWidthAll(2);
        styleBox.SetCornerRadiusAll(8);
        card.AddThemeStyleboxOverride("normal", styleBox);
        
        var hoverStyle = new StyleBoxFlat();
        hoverStyle.BgColor = rarityColor * new Color(0.5f, 0.5f, 0.5f);
        hoverStyle.BorderColor = rarityColor;
        hoverStyle.SetBorderWidthAll(3);
        hoverStyle.SetCornerRadiusAll(8);
        card.AddThemeStyleboxOverride("hover", hoverStyle);
        
        var pressedStyle = new StyleBoxFlat();
        pressedStyle.BgColor = rarityColor * new Color(0.4f, 0.4f, 0.4f);
        pressedStyle.BorderColor = rarityColor;
        pressedStyle.SetBorderWidthAll(2);
        pressedStyle.SetCornerRadiusAll(8);
        card.AddThemeStyleboxOverride("pressed", pressedStyle);
        
        // 解锁状态
        bool isUnlocked = _artifactSystem.IsUnlocked(artifact.id);
        
        if (!isUnlocked)
        {
            // 未解锁样式
            var lockedStyle = new StyleBoxFlat();
            lockedStyle.BgColor = new Color(0.2f, 0.2f, 0.2f);
            lockedStyle.BorderColor = new Color(0.3f, 0.3f, 0.3f);
            lockedStyle.SetBorderWidthAll(1);
            lockedStyle.SetCornerRadiusAll(8);
            card.AddThemeStyleboxOverride("normal", lockedStyle);
            
            nameLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
            typeLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
        }
        
        // 装备状态
        if (_artifactSystem.IsEquipped(artifact.id))
        {
            var equippedStyle = new StyleBoxFlat();
            equippedStyle.BgColor = rarityColor * new Color(0.6f, 0.6f, 0.6f);
            equippedStyle.BorderColor = new Color(1.0f, 0.8f, 0.0f);
            equippedStyle.SetBorderWidthAll(3);
            equippedStyle.SetCornerRadiusAll(8);
            card.AddThemeStyleboxOverride("normal", equippedStyle);
        }
        
        // 点击事件
        card.Pressed += () => OnArtifactSelected(artifact.id);
        
        return card;
    }
    
    private void OnArtifactSelected(string artifactId)
    {
        _selectedArtifact = artifactId;
        UpdateDetailPanel();
    }
    
    private void UpdateDetailPanel()
    {
        if (_selectedArtifact == "" || _artifactSystem == null)
        {
            _detailName.Text = "Select an artifact";
            _detailDescription.Text = "";
            _detailStats.Text = "";
            _detailType.Text = "";
            _detailRarity.Text = "";
            _equipButton.Disabled = true;
            _unequipButton.Disabled = true;
            return;
        }
        
        var artifact = _artifactSystem.GetArtifact(_selectedArtifact);
        if (artifact == null) return;
        
        bool isUnlocked = _artifactSystem.IsUnlocked(_selectedArtifact);
        bool isEquipped = _artifactSystem.IsEquipped(_selectedArtifact);
        
        // 名称和颜色
        _detailName.Text = artifact.displayName;
        _detailName.Modulate = GetRarityColor(artifact.rarity);
        
        _detailRarity.Text = "Rarity: " + artifact.rarity.ToString();
        _detailRarity.Modulate = GetRarityColor(artifact.rarity);
        
        _detailType.Text = "Type: " + artifact.type.ToString();
        
        // 描述
        if (isUnlocked)
        {
            _detailDescription.Text = artifact.description;
        }
        else
        {
            _detailDescription.Text = "[ Locked ]\nDiscover this artifact to reveal its secrets.";
        }
        
        // 属性
        string statsText = "";
        if (isUnlocked)
        {
            statsText = $"Power: {artifact.power}\n";
            statsText += $"Attack: +{artifact.attack}\n";
            statsText += $"Defense: +{artifact.defense}\n";
            statsText += $"Health: +{artifact.health}\n";
            statsText += $"Speed: +{artifact.speed}\n";
            statsText += $"Crit Chance: +{artifact.critChance:P1}\n";
            statsText += $"Crit Damage: +{artifact.critDamage:P1}";
        }
        else
        {
            statsText = "???";
        }
        _detailStats.Text = statsText;
        
        // 按钮状态
        _equipButton.Disabled = !isUnlocked || isEquipped;
        _unequipButton.Disabled = !isEquipped;
    }
    
    private Color GetRarityColor(ArtifactRarity rarity)
    {
        switch (rarity)
        {
            case ArtifactRarity.Common: return rarityColorCommon;
            case ArtifactRarity.Uncommon: return rarityColorUncommon;
            case ArtifactRarity.Rare: return rarityColorRare;
            case ArtifactRarity.Epic: return rarityColorEpic;
            case ArtifactRarity.Legendary: return rarityColorLegendary;
            default: return Color.White;
        }
    }
    
    private void OnEquipPressed()
    {
        if (_selectedArtifact != "")
        {
            _artifactSystem.EquipArtifact(_selectedArtifact);
            RefreshArtifactList();
        }
    }
    
    private void OnUnequipPressed()
    {
        _artifactSystem.UnequipArtifact();
        RefreshArtifactList();
    }
    
    private void OnClosePressed()
    {
        QueueFree();
    }
    
    private void OnArtifactUnlocked(string artifactId)
    {
        RefreshArtifactList();
    }
    
    private void OnArtifactEquipped(string artifactId)
    {
        RefreshArtifactList();
    }
    
    private void OnArtifactUnequipped(string artifactId)
    {
        RefreshArtifactList();
    }
    
    // 切换可见性（供 Main.cs 调用）
    public void ToggleVisible()
    {
        Visible = !Visible;
        if (Visible)
        {
            RefreshArtifactList();
        }
    }
}
