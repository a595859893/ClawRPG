using Godot;
using System;
using System.Collections.Generic;

public class TitleUI : Control
{
    private static TitleUI _instance;
    public static TitleUI Instance
    {
        get
        {
            return _instance;
        }
    }
    
    // UI 组件
    private PanelContainer _mainPanel;
    private VBoxContainer _mainVBox;
    private HBoxContainer _categoryTabs;
    private ScrollContainer _titleListScroll;
    private VBoxContainer _titleList;
    private PanelContainer _detailPanel;
    private Label _detailName;
    private Label _detailDescription;
    private Label _detailRarity;
    private Label _detailCategory;
    private VBoxContainer _detailAttributes;
    private Button _activateButton;
    private Label _activeTitleLabel;
    
    // 状态
    private TitleCategory _currentCategory = TitleCategory.Combat;
    private TitleDefinition _selectedTitle;
    private bool _isVisible = false; 
    
    // 颜色
    private Color _commonColor = new Color(1, 1, 1);
    private Color _rareColor = new Color(0, 1, 0);
    private Color _epicColor = new Color(1, 0, 1);
    private Color _legendaryColor = new Color(1, 0.65, 0);
    
    public override void _Ready()
    {
        _instance = this;
        Visible = false; 
        SetupUI();
    }
    
    private void SetupUI()
    {
        // 主面板
        _mainPanel = new PanelContainer();
        _mainPanel.SetAnchor(AnchorPresets.FullRect);
        _mainPanel.MarginLeft = 200;
        _mainPanel.MarginTop = 100;
        _mainPanel.MarginRight = -200;
        _mainPanel.MarginBottom = -100;
        _mainPanel.Modulate = new Color(1, 1, 1, 0.95f);
        AddChild(_mainPanel);
        
        // 主容器
        _mainVBox = new VBoxContainer();
        _mainVBox.SetAnchor(AnchorPresets.FullRect);
        _mainPanel.AddChild(_mainVBox);
        
        // 标题栏
        var titleBar = new HBoxContainer();
        titleBar.Alignment = BoxContainer.AlignMode.Center;
        _mainVBox.AddChild(titleBar);
        
        var titleLabel = new Label();
        titleLabel.Text = "  玩家称号  ";
        titleLabel.AddColorOverride("font_color", new Color(1, 0.84, 0));
        titleLabel.RectMinSize = new Vector2(0, 40);
        titleLabel.Align = Label.AlignEnum.Center;
        titleBar.AddChild(titleLabel);
        
        // 当前激活称号显示
        _activeTitleLabel = new Label();
        _activeTitleLabel.Text = "当前称号: 无";
        _activeTitleLabel.AddColorOverride("font_color", new Color(0.7, 0.7, 0.7));
        _activeTitleLabel.RectMinSize = new Vector2(0, 30);
        titleBar.AddChild(_activeTitleLabel);
        
        var closeButton = new Button();
        closeButton.Text = "X";
        closeButton.RectMinSize = new Vector2(30, 30);
        closeButton.Connect("pressed", this, nameof(OnClosePressed));
        titleBar.AddChild(closeButton);
        
        // 分类标签
        _categoryTabs = new HBoxContainer();
        _categoryTabs.Alignment = BoxContainer.AlignMode.Center;
        _categoryTabs.RectMinSize = new Vector2(0, 45);
        _mainVBox.AddChild(_categoryTabs);
        
        CreateCategoryTab("战斗", TitleCategory.Combat);
        CreateCategoryTab("采集", TitleCategory.Gathering);
        CreateCategoryTab("探索", TitleCategory.Exploration);
        CreateCategoryTab("社交", TitleCategory.Social);
        CreateCategoryTab("特殊", TitleCategory.Special);
        
        // 内容区域
        var contentHBox = new HBoxContainer();
        contentHBox.SetAnchor(AnchorPresets.FullRect);
        contentHBox.MarginLeft = 10;
        contentHBox.MarginTop = 10;
        contentHBox.MarginRight = -10;
        contentHBox.MarginBottom = -10;
        contentHBox.MouseFilter = Control.MouseFilterEnum.Stop;
        _mainVBox.AddChild(contentHBox);
        
        // 称号列表
        _titleListScroll = new ScrollContainer();
        _titleListScroll.SetAnchor(AnchorPresets.LeftWide, true);
        _titleListScroll.RectMinSize = new Vector2(300, 0);
        _titleListScroll.MouseFilter = Control.MouseFilterEnum.Stop;
        contentHBox.AddChild(_titleListScroll);
        
        _titleList = new VBoxContainer();
        _titleList.SetAnchor(AnchorPresets.FullRect);
        _titleList.RectMinSize = new Vector2(280, 0);
        _titleListScroll.AddChild(_titleList);
        
        // 详情面板
        _detailPanel = new PanelContainer();
        _detailPanel.SetAnchor(AnchorPresets.RightWide, true);
        _detailPanel.RectMinSize = new Vector2(250, 0);
        _detailPanel.MouseFilter = Control.MouseFilterEnum.Stop;
        contentHBox.AddChild(_detailPanel);
        
        var detailVBox = new VBoxContainer();
        detailVBox.SetAnchor(AnchorPresets.FullRect);
        detailVBox.MarginLeft = 10;
        detailVBox.MarginTop = 10;
        detailVBox.MarginRight = -10;
        detailVBox.MarginBottom = -10;
        _detailPanel.AddChild(detailVBox);
        
        _detailName = new Label();
        _detailName.Text = "选择称号";
        _detailName.Align = Label.AlignEnum.Center;
        _detailName.AddColorOverride("font_color", new Color(1, 0.84, 0));
        _detailName.RectMinSize = new Vector2(0, 30);
        detailVBox.AddChild(_detailName);
        
        _detailDescription = new Label();
        _detailDescription.Text = "";
        _detailDescription.Autowrap = true;
        _detailDescription.Align = Label.AlignEnum.Center;
        _detailDescription.RectMinSize = new Vector2(0, 40);
        detailVBox.AddChild(_detailDescription);
        
        _detailRarity = new Label();
        _detailRarity.Text = "";
        _detailRarity.Align = Label.AlignEnum.Center;
        _detailRarity.RectMinSize = new Vector2(0, 25);
        detailVBox.AddChild(_detailRarity);
        
        _detailCategory = new Label();
        _detailCategory.Text = "";
        _detailCategory.Align = Label.AlignEnum.Center;
        _detailCategory.RectMinSize = new Vector2(0, 25);
        detailVBox.AddChild(_detailCategory);
        
        var attrTitle = new Label();
        attrTitle.Text = "属性加成:";
        attrTitle.Align = Label.AlignEnum.Center;
        attrTitle.RectMinSize = new Vector2(0, 25);
        detailVBox.AddChild(attrTitle);
        
        _detailAttributes = new VBoxContainer();
        detailVBox.AddChild(_detailAttributes);
        
        _activateButton = new Button();
        _activateButton.Text = "激活称号";
        _activateButton.RectMinSize = new Vector2(0, 40);
        _activateButton.Disabled = true;
        _activateButton.Connect("pressed", this, nameof(OnActivatePressed));
        detailVBox.AddChild(_activateButton);
        
        UpdateTitleList();
    }
    
    private void CreateCategoryTab(string text, TitleCategory category)
    {
        var button = new Button();
        button.Text = text;
        button.ToggleMode = true;
        button.Pressed = _currentCategory == category;
        button.Connect("pressed", this, nameof(OnCategoryPressed), new Godot.Collections.Array { category });
        _categoryTabs.AddChild(button);
    }
    
    private void OnCategoryPressed(TitleCategory category)
    {
        _currentCategory = category;
        
        // 更新标签状态
        foreach (var child in _categoryTabs.GetChildren())
        {
            if (child is Button btn)
            {
                btn.Pressed = btn.Text == GetCategoryName(category);
            }
        }
        
        UpdateTitleList();
    }
    
    private string GetCategoryName(TitleCategory category)
    {
        switch (category)
        {
            case TitleCategory.Combat: return "战斗";
            case TitleCategory.Gathering: return "采集";
            case TitleCategory.Exploration: return "探索";
            case TitleCategory.Social: return "社交";
            case TitleCategory.Special: return "特殊";
            default: return "";
        }
    }
    
    private void UpdateTitleList()
    {
        // 清空列表
        foreach (var child in _titleList.GetChildren())
        {
            child.QueueFree();
        }
        
        var titles = TitleDatabase.Instance.GetTitlesByCategory(_currentCategory);
        
        foreach (var title in titles)
        {
            var titleData = TitleSystem.Instance.GetTitleData(title.Id);
            bool isUnlocked = titleData != null && titleData.IsUnlocked;
            bool isActive = titleData != null && titleData.IsActive;
            
            var itemPanel = new PanelContainer();
            itemPanel.RectMinSize = new Vector2(0, 45);
            itemPanel.MouseFilter = Control.MouseFilterEnum.Stop;
            _titleList.AddChild(itemPanel);
            
            var itemHBox = new HBoxContainer();
            itemPanel.AddChild(itemHBox);
            
            // 状态图标
            var statusLabel = new Label();
            if (isActive)
            {
                statusLabel.Text = "★";
                statusLabel.AddColorOverride("font_color", new Color(1, 0.84, 0));
            }
            else if (isUnlocked)
            {
                statusLabel.Text = "✓";
                statusLabel.AddColorOverride("font_color", new Color(0, 1, 0));
            }
            else
            {
                statusLabel.Text = "?";
                statusLabel.AddColorOverride("font_color", new Color(0.5, 0.5, 0.5));
            }
            statusLabel.RectMinSize = new Vector2(25, 0);
            itemHBox.AddChild(statusLabel);
            
            // 称号名称
            var nameLabel = new Label();
            string displayName = isUnlocked || !title.IsSecret ? title.Name : "???";
            nameLabel.Text = displayName;
            nameLabel.AddColorOverride("font_color", GetRarityColor(title.Rarity));
            nameLabel.RectMinSize = new Vector2(180, 0);
            nameLabel.Align = Label.AlignEnum.Left;
            itemHBox.AddChild(nameLabel);
            
            // 点击事件
            var clickDetector = new Control();
            clickDetector.RectMinSize = new Vector2(220, 40);
            clickDetector.MouseFilter = Control.MouseFilterEnum.Stop;
            clickDetector.Connect("gui_input", this, nameof(OnTitleItemPressed), new Godot.Collections.Array { title });
            itemHBox.AddChild(clickDetector);
        }
    }
    
    private Color GetRarityColor(TitleRarity rarity)
    {
        switch (rarity)
        {
            case TitleRarity.Common: return _commonColor;
            case TitleRarity.Rare: return _rareColor;
            case TitleRarity.Epic: return _epicColor;
            case TitleRarity.Legendary: return _legendaryColor;
            default: return _commonColor;
        }
    }
    
    private void OnTitleItemPressed(InputEvent @event, TitleDefinition title)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == Godot.ButtonList.Left)
        {
            _selectedTitle = title;
            UpdateDetailPanel();
        }
    }
    
    private void UpdateDetailPanel()
    {
        if (_selectedTitle == null) return;
        
        var titleData = TitleSystem.Instance.GetTitleData(_selectedTitle.Id);
        bool isUnlocked = titleData != null && titleData.IsUnlocked;
        bool isActive = titleData != null && titleData.IsActive;
        
        // 名称
        string displayName = isUnlocked || !_selectedTitle.IsSecret ? _selectedTitle.Name : "???";
        _detailName.Text = displayName;
        _detailName.AddColorOverride("font_color", GetRarityColor(_selectedTitle.Rarity));
        
        // 描述
        _detailDescription.Text = isUnlocked ? _selectedTitle.Description : "未解锁";
        
        // 稀有度
        string rarityText = "";
        switch (_selectedTitle.Rarity)
        {
            case TitleRarity.Common: rarityText = "普通"; break;
            case TitleRarity.Rare: rarityText = "稀有"; break;
            case TitleRarity.Epic: rarityText = "史诗"; break;
            case TitleRarity.Legendary: rarityText = "传说"; break;
        }
        _detailRarity.Text = "稀有度: " + rarityText;
        _detailRarity.AddColorOverride("font_color", GetRarityColor(_selectedTitle.Rarity));
        
        // 分类
        _detailCategory.Text = "分类: " + GetCategoryName(_selectedTitle.Category);
        
        // 属性加成
        foreach (var child in _detailAttributes.GetChildren())
        {
            child.QueueFree();
        }
        
        if (isUnlocked && _selectedTitle.AttributeBonuses != null)
        {
            foreach (var kvp in _selectedTitle.AttributeBonuses)
            {
                var attrLabel = new Label();
                attrLabel.Text = $"  {kvp.Key}: +{kvp.Value}";
                attrLabel.AddColorOverride("font_color", new Color(0.7, 0.9, 0.7));
                _detailAttributes.AddChild(attrLabel);
            }
        }
        else
        {
            var lockLabel = new Label();
            lockLabel.Text = "  (无属性加成)";
            lockLabel.AddColorOverride("font_color", new Color(0.5, 0.5, 0.5));
            _detailAttributes.AddChild(lockLabel);
        }
        
        // 激活按钮
        if (isUnlocked)
        {
            _activateButton.Disabled = false; 
            if (isActive)
            {
                _activateButton.Text = "取消激活";
            }
            else
            {
                _activateButton.Text = "激活称号";
            }
        }
        else
        {
            _activateButton.Disabled = true;
            _activateButton.Text = "未解锁";
        }
    }
    
    private void OnActivatePressed()
    {
        if (_selectedTitle == null) return;
        
        var titleData = TitleSystem.Instance.GetTitleData(_selectedTitle.Id);
        if (titleData == null || !titleData.IsUnlocked) return;
        
        if (titleData.IsActive)
        {
            TitleSystem.Instance.DeactivateTitle(_selectedTitle.Id);
        }
        else
        {
            TitleSystem.Instance.ActivateTitle(_selectedTitle.Id);
        }
        
        UpdateTitleList();
        UpdateDetailPanel();
        UpdateActiveTitleLabel();
    }
    
    private void UpdateActiveTitleLabel()
    {
        var activeTitle = TitleSystem.Instance.GetActiveTitle();
        if (activeTitle != null)
        {
            _activeTitleLabel.Text = "当前称号: " + activeTitle.Name;
        }
        else
        {
            _activeTitleLabel.Text = "当前称号: 无";
        }
    }
    
    private void OnClosePressed()
    {
        ToggleUI();
    }
    
    public void ToggleUI()
    {
        _isVisible = !_isVisible;
        Visible = _isVisible;
        
        if (_isVisible)
        {
            UpdateTitleList();
            UpdateActiveTitleLabel();
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Scancode == Godot.KeyList.N)
            {
                ToggleUI();
            }
            else if (keyEvent.Scancode == Godot.KeyList.Escape && _isVisible)
            {
                ToggleUI();
            }
        }
    }
}
