using Godot;
using System;
using System.Collections.Generic;
using Game.Systems.Pets;

public partial class PetEquipmentUI : Control
{
    private PanelContainer _mainPanel;
    private VBoxContainer _mainVBox;
    
    // 顶部信息
    private HBoxContainer _topBar;
    private Label _titleLabel;
    private Label _goldLabel;
    private Button _closeButton;
    
    // 主内容区域
    private HBoxContainer _contentArea;
    
    // 左侧：宠物列表
    private VBoxContainer _petListContainer;
    private ScrollContainer _petScroll;
    private VBoxContainer _petList;
    
    // 中间：装备商店/背包
    private VBoxContainer _equipmentContainer;
    private TabContainer _equipmentTabs;
    private ScrollContainer _shopScroll;
    private VBoxContainer _shopList;
    private ScrollContainer _inventoryScroll;
    private VBoxContainer _inventoryList;
    
    // 右侧：详情面板
    private VBoxContainer _detailPanel;
    private Label _detailNameLabel;
    private RichTextLabel _detailDescLabel;
    private VBoxContainer _statsContainer;
    private Label _statsLabel;
    private Button _buyButton;
    private Button _equipButton;
    private Button _unequipButton;
    
    // 当前选择
    private string _selectedEquipmentId = "";
    private PetEquipmentType _selectedType = PetEquipmentType.Collar;
    
    // 宠物相关
    private PetManager _petManager;
    private List<Pet> _pets = new List<Pet>();
    private string _selectedPetId = "";
    
    // 玩家金币
    private int _playerGold = 0;
    
    // UI 颜色
    private Color _commonColor = new Color(0.7f, 0.7f, 0.7f);
    private Color _uncommonColor = new Color(0.2f, 0.8f, 0.2f);
    private Color _rareColor = new Color(0.2f, 0.5f, 1.0f);
    private Color _epicColor = new Color(0.6f, 0.3f, 0.9f);
    private Color _legendaryColor = new Color(1.0f, 0.6f, 0.0f);
    
    public override void _Ready()
    {
        SetupUI();
        ConnectSignals();
        RefreshUI();
    }
    
    private void SetupUI()
    {
        // 主面板
        _mainPanel = new PanelContainer();
        _mainPanel.AnchorRight = 1.0f;
        _mainPanel.AnchorBottom = 1.0f;
        _mainPanel.Modulate = new Color(1, 1, 1, 0.95f);
        AddChild(_mainPanel);
        
        _mainVBox = new VBoxContainer();
        _mainVBox.SetAnchorAndMargin(AnchorPreset.FullRect, 0);
        _mainVBox.MarginLeft = 20;
        _mainVBox.MarginTop = 20;
        _mainVBox.MarginRight = 20;
        _mainVBox.MarginBottom = 20;
        _mainPanel.AddChild(_mainVBox);
        
        // 顶部栏
        _topBar = new HBoxContainer();
        _topBar.CustomMinimumHeight = 50;
        _mainVBox.AddChild(_topBar);
        
        _titleLabel = new Label();
        _titleLabel.Text = "  宠物装备";
        _titleLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _topBar.AddChild(_titleLabel);
        
        _goldLabel = new Label();
        _goldLabel.Text = "金币: 0";
        _goldLabel.HorizontalAlignment = HorizontalAlignment.Right;
        _topBar.AddChild(_goldLabel);
        
        _closeButton = new Button();
        _closeButton.Text = "✕";
        _closeButton.CustomMinimumWidth = 40;
        _closeButton.Pressed += () => Toggle();
        _topBar.AddChild(_closeButton);
        
        // 分割线
        HSeparator sep = new HSeparator();
        sep.Modulate = new Color(1, 1, 1, 0.3f);
        _mainVBox.AddChild(sep);
        
        // 主内容区域
        _contentArea = new HBoxContainer();
        _contentArea.SetAnchorAndMargin(AnchorPreset.FullRect, 0);
        _contentArea.MarginTop = 10;
        _contentArea.MarginRight = 10;
        _contentArea.MarginBottom = 10;
        _mainVBox.AddChild(_contentArea);
        
        // 左侧：宠物选择
        _petListContainer = new VBoxContainer();
        _petListContainer.CustomMinimumWidth = 150;
        _contentArea.AddChild(_petListContainer);
        
        Label petListTitle = new Label();
        petListTitle.Text = "选择宠物";
        petListTitle.HorizontalAlignment = HorizontalAlignment.Center;
        _petListContainer.AddChild(petListTitle);
        
        _petScroll = new ScrollContainer();
        _petScroll.CustomMinimumHeight = 300;
        _petListContainer.AddChild(_petScroll);
        
        _petList = new VBoxContainer();
        _petScroll.AddChild(_petList);
        
        // 中间：装备列表
        _equipmentContainer = new VBoxContainer();
        _equipmentContainer.SetAnchorAndMargin(AnchorPreset.FullRect, 0);
        _equipmentContainer.CustomMinimumWidth = 400;
        _contentArea.AddChild(_equipmentContainer);
        
        // 类型筛选
        HBoxContainer typeFilter = new HBoxContainer();
        typeFilter.CustomMinimumHeight = 40;
        _equipmentContainer.AddChild(typeFilter);
        
        string[] typeNames = { "项圈", "马具", "护甲", "配饰", "玩具" };
        for (int i = 0; i < typeNames.Length; i++)
        {
            Button typeBtn = new Button();
            typeBtn.Text = typeNames[i];
            typeBtn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            typeBtn.Pressed += () => OnTypeButtonPressed(typeBtn);
            typeBtn.Metadata = i;
            typeFilter.AddChild(typeBtn);
        }
        
        // 标签页
        _equipmentTabs = new TabContainer();
        _equipmentTabs.SetAnchorAndMargin(AnchorPreset.FullRect, 0);
        _equipmentTabs.SizeFlagsVertical = SizeFlags.ExpandFill;
        _equipmentContainer.AddChild(_equipmentTabs);
        
        // 商店标签页
        Control shopTab = new Control();
        shopTab.Name = "商店";
        _equipmentTabs.AddChild(shopTab);
        
        _shopScroll = new ScrollContainer();
        _shopScroll.SetAnchorAndMargin(AnchorPreset.FullRect, 0);
        shopTab.AddChild(_shopScroll);
        
        _shopList = new VBoxContainer();
        _shopScroll.AddChild(_shopList);
        
        // 背包标签页
        Control inventoryTab = new Control();
        inventoryTab.Name = "背包";
        _equipmentTabs.AddChild(inventoryTab);
        
        _inventoryScroll = new ScrollContainer();
        _inventoryScroll.SetAnchorAndMargin(AnchorPreset.FullRect, 0);
        inventoryTab.AddChild(_inventoryScroll);
        
        _inventoryList = new VBoxContainer();
        _inventoryScroll.AddChild(_inventoryList);
        
        // 右侧：详情面板
        _detailPanel = new VBoxContainer();
        _detailPanel.CustomMinimumWidth = 200;
        _contentArea.AddChild(_detailPanel);
        
        Label detailTitle = new Label();
        detailTitle.Text = "装备详情";
        detailTitle.HorizontalAlignment = HorizontalAlignment.Center;
        _detailPanel.AddChild(detailTitle);
        
        _detailNameLabel = new Label();
        _detailNameLabel.Text = "未选择";
        _detailNameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _detailNameLabel.CustomMinimumHeight = 30;
        _detailPanel.AddChild(_detailNameLabel);
        
        _detailDescLabel = new RichTextLabel();
        _detailDescLabel.BbcodeEnabled = true;
        _detailDescLabel.CustomMinimumHeight = 80;
        _detailDescLabel.SizeFlagsVertical = SizeFlags.ExpandFill;
        _detailPanel.AddChild(_detailDescLabel);
        
        _statsLabel = new Label();
        _statsLabel.Text = "";
        _statsLabel.CustomMinimumHeight = 120;
        _detailPanel.AddChild(_statsLabel);
        
        _buyButton = new Button();
        _buyButton.Text = "购买";
        _buyButton.Pressed += OnBuyPressed;
        _buyButton.Visible = false; 
        _detailPanel.AddChild(_buyButton);
        
        _equipButton = new Button();
        _equipButton.Text = "装备";
        _equipButton.Pressed += OnEquipPressed;
        _equipButton.Visible = false; 
        _detailPanel.AddChild(_equipButton);
        
        _unequipButton = new Button();
        _unequipButton.Text = "卸下";
        _unequipButton.Pressed += OnUnequipPressed;
        _unequipButton.Visible = false; 
        _detailPanel.AddChild(_unequipButton);
    }
    
    private void ConnectSignals()
    {
        if (PetEquipmentSystem.Instance != null)
        {
            PetEquipmentSystem.Instance.EquipmentPurchased += OnEquipmentChanged;
            PetEquipmentSystem.Instance.EquipmentEquipped += OnEquipmentChanged;
            PetEquipmentSystem.Instance.EquipmentUnequipped += OnEquipmentChanged;
            PetEquipmentSystem.Instance.DataLoaded += RefreshUI;
        }
    }
    
    private void OnTypeButtonPressed(Button btn)
    {
        int typeIndex = (int)btn.Metadata;
        _selectedType = (PetEquipmentType)typeIndex;
        RefreshEquipmentList();
    }
    
    private void OnEquipmentChanged(string _ = "", string __ = "")
    {
        RefreshUI();
    }
    
    public void RefreshUI()
    {
        // 获取玩家金币
        var player = GetNode<Player>("/root/Main/Player");
        if (player != null)
        {
            _playerGold = player.Gold;
            _goldLabel.Text = $"金币: {_playerGold}";
        }
        
        // 获取宠物列表
        RefreshPetList();
        
        // 刷新装备列表
        RefreshEquipmentList();
        
        // 刷新详情
        RefreshDetailPanel();
    }
    
    private void RefreshPetList()
    {
        // 清除现有
        foreach (Node child in _petList.GetChildren())
        {
            child.QueueFree();
        }
        
        // 获取宠物管理器
        _petManager = PetManager.Instance;
        if (_petManager == null) return;
        
        _pets = _petManager.GetAllPets();
        
        foreach (var pet in _pets)
        {
            Button petBtn = new Button();
            petBtn.Text = pet.PetName;
            petBtn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            petBtn.Pressed += () => OnPetSelected(pet.PetId);
            
            if (pet.PetId == _selectedPetId)
            {
                petBtn.Modulate = new Color(1, 0.8f, 0.4f);
            }
            
            _petList.AddChild(petBtn);
        }
        
        // 默认选择第一个宠物
        if (_selectedPetId == "" && _pets.Count > 0)
        {
            _selectedPetId = _pets[0].PetId;
        }
    }
    
    private void OnPetSelected(string petId)
    {
        _selectedPetId = petId;
        RefreshPetList();
        RefreshDetailPanel();
    }
    
    private void RefreshEquipmentList()
    {
        // 清除现有
        foreach (Node child in _shopList.GetChildren())
        {
            child.QueueFree();
        }
        foreach (Node child in _inventoryList.GetChildren())
        {
            child.QueueFree();
        }
        
        // 获取商店列表
        var shopEquipment = PetEquipmentDatabase.GetEquipmentByType(_selectedType);
        foreach (var equip in shopEquipment)
        {
            CreateEquipmentButton(equip, _shopList, true);
        }
        
        // 获取背包列表
        var ownedEquipment = PetEquipmentSystem.Instance.GetOwnedEquipmentByType(_selectedType);
        foreach (var equipId in ownedEquipment)
        {
            var equip = PetEquipmentDatabase.GetEquipment(equipId);
            if (equip != null)
            {
                CreateEquipmentButton(equip, _inventoryList, false);
            }
        }
    }
    
    private void CreateEquipmentButton(PetEquipment equip, VBoxContainer container, bool isShop)
    {
        Button btn = new Button();
        btn.Text = $"[{GetRarityName(equip.Rarity)}] {equip.Name} - {equip.Price}金币";
        btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        btn.Modulate = GetRarityColor(equip.Rarity);
        btn.Pressed += () => OnEquipmentSelected(equip.Id, isShop);
        
        container.AddChild(btn);
    }
    
    private void OnEquipmentSelected(string equipmentId, bool isShop)
    {
        _selectedEquipmentId = equipmentId;
        
        // 切换到对应标签页
        _equipmentTabs.CurrentTab = isShop ? 0 : 1;
        
        RefreshDetailPanel();
    }
    
    private void RefreshDetailPanel()
    {
        if (string.IsNullOrEmpty(_selectedEquipmentId))
        {
            _detailNameLabel.Text = "未选择";
            _detailDescLabel.Text = "";
            _statsLabel.Text = "";
            _buyButton.Visible = false; 
            _equipButton.Visible = false; 
            _unequipButton.Visible = false; 
            return;
        }
        
        var equip = PetEquipmentDatabase.GetEquipment(_selectedEquipmentId);
        if (equip == null) return;
        
        _detailNameLabel.Text = $"[{GetRarityName(equip.Rarity)}] {equip.Name}";
        _detailNameLabel.Modulate = GetRarityColor(equip.Rarity);
        
        _detailDescLabel.Text = equip.Description;
        
        // 属性
        string stats = "\n";
        if (equip.AttackBonus > 0) stats += $"攻击 +{equip.AttackBonus}\n";
        if (equip.DefenseBonus > 0) stats += $"防御 +{equip.DefenseBonus}\n";
        if (equip.HealthBonus > 0) stats += $"生命 +{equip.HealthBonus}\n";
        if (equip.SpeedBonus > 0) stats += $"速度 +{equip.SpeedBonus}\n";
        if (equip.CritRateBonus > 0) stats += $"暴击率 +{(equip.CritRateBonus * 100):F1}%\n";
        if (equip.CritDamageBonus > 0) stats += $"暴击伤害 +{(equip.CritDamageBonus * 100):F1}%\n";
        if (equip.LifeStealBonus > 0) stats += $"生命偷取 +{equip.LifeStealBonus}%\n";
        
        _statsLabel.Text = stats;
        
        // 按钮状态
        bool owned = PetEquipmentSystem.Instance.HasEquipment(_selectedEquipmentId);
        string currentEquipped = PetEquipmentSystem.Instance.GetEquippedEquipment(_selectedPetId);
        bool isEquipped = currentEquipped == _selectedEquipmentId;
        
        _buyButton.Visible = !owned && _equipmentTabs.CurrentTab == 0;
        _equipButton.Visible = owned && !isEquipped && !string.IsNullOrEmpty(_selectedPetId);
        _unequipButton.Visible = isEquipped;
    }
    
    private void OnBuyPressed()
    {
        var equip = PetEquipmentDatabase.GetEquipment(_selectedEquipmentId);
        if (equip == null) return;
        
        var player = GetNode<Player>("/root/Main/Player");
        if (player == null) return;
        
        if (player.Gold < equip.Price)
        {
            GD.Print("[PetEquipmentUI] Not enough gold!");
            return;
        }
        
        bool success = PetEquipmentSystem.Instance.PurchaseEquipment(_selectedEquipmentId, player.Gold);
        if (success)
        {
            player.Gold -= equip.Price;
            _goldLabel.Text = $"金币: {player.Gold}";
            RefreshUI();
        }
    }
    
    private void OnEquipPressed()
    {
        if (string.IsNullOrEmpty(_selectedPetId)) return;
        
        PetEquipmentSystem.Instance.EquipToPet(_selectedPetId, _selectedEquipmentId);
        RefreshUI();
    }
    
    private void OnUnequipPressed()
    {
        if (string.IsNullOrEmpty(_selectedPetId)) return;
        
        PetEquipmentSystem.Instance.UnequipFromPet(_selectedPetId);
        RefreshUI();
    }
    
    private string GetRarityName(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "普通";
            case ItemRarity.Uncommon: return "优秀";
            case ItemRarity.Rare: return "稀有";
            case ItemRarity.Epic: return "史诗";
            case ItemRarity.Legendary: return "传说";
            default: return "未知";
        }
    }
    
    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return _commonColor;
            case ItemRarity.Uncommon: return _uncommonColor;
            case ItemRarity.Rare: return _rareColor;
            case ItemRarity.Epic: return _epicColor;
            case ItemRarity.Legendary: return _legendaryColor;
            default: return _commonColor;
        }
    }
    
    public void Toggle()
    {
        if (Visible)
        {
            Hide();
        }
        else
        {
            Show();
            RefreshUI();
        }
    }
}
