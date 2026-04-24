using Godot;
using System;
using System.Collections.Generic;

public partial class RandomBoonUI : Control
{
    private PanelContainer _mainPanel;
    private VBoxContainer _mainVBox;
    private HBoxContainer _header;
    private Label _titleLabel;
    private Label _statsLabel;
    private TabContainer _tabContainer;
    
    // Owned boons tab
    private ScrollContainer _ownedScroll;
    private GridContainer _ownedGrid;
    
    // Active boons tab
    private ScrollContainer _activeScroll;
    private GridContainer _activeGrid;
    
    // Offer panel (for selecting boons)
    private PanelContainer _offerPanel;
    private VBoxContainer _offerVBox;
    private Label _offerTitle;
    private HBoxContainer _offerContainer;
    private Button _closeOfferButton;
    
    // Current mode
    private bool _isOfferMode;
    
    // Boon button scene path
    private PackedScene _boonButtonScene;
    
    public override void _Ready()
    {
        _boonButtonScene = GD.Load<PackedScene>("res://UI/BoonButton.tscn");
        SetupUI();
        SetupSignals();
        RefreshUI();
        
        Visible = false; 
    }
    
    private void SetupUI()
    {
        // Main panel
        _mainPanel = new PanelContainer();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(800, 600);
        AddChild(_mainPanel);
        
        _mainVBox = new VBoxContainer();
        _mainPanel.AddChild(_mainVBox);
        
        // Header
        _header = new HBoxContainer();
        _header.Alignment = BoxContainer.AlignmentMode.Center;
        _mainVBox.AddChild(_header);
        
        _titleLabel = new Label();
        _titleLabel.Text = "  随机祝福系统  ";
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _header.AddChild(_titleLabel);
        
        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(100, 0);
        _header.AddChild(spacer);
        
        _statsLabel = new Label();
        _statsLabel.Text = "激活: 0/3";
        _header.AddChild(_statsLabel);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _mainVBox.AddChild(_tabContainer);
        
        // Owned tab
        var ownedTab = new Control();
        ownedTab.Name = "已拥有";
        _tabContainer.AddChild(ownedTab);
        
        _ownedScroll = new ScrollContainer();
        _ownedScroll.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _ownedScroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        ownedTab.AddChild(_ownedScroll);
        
        _ownedGrid = new GridContainer();
        _ownedGrid.Columns = 4;
        _ownedGrid.AddThemeConstantOverride("separation", 10);
        _ownedScroll.AddChild(_ownedGrid);
        
        // Active tab
        var activeTab = new Control();
        activeTab.Name = "已激活";
        _tabContainer.AddChild(activeTab);
        
        _activeScroll = new ScrollContainer();
        _activeScroll.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _activeScroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        activeTab.AddChild(_activeScroll);
        
        _activeGrid = new GridContainer();
        _activeGrid.Columns = 4;
        _activeGrid.AddThemeConstantOverride("separation", 10);
        _activeScroll.AddChild(_activeGrid);
        
        // Offer panel (hidden by default)
        SetupOfferPanel();
        
        // Close button
        var closeButton = new Button();
        closeButton.Text = "  关闭 (B)  ";
        closeButton.Pressed += () => ToggleUI();
        _mainVBox.AddChild(closeButton);
    }
    
    private void SetupOfferPanel()
    {
        _offerPanel = new PanelContainer();
        _offerPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _offerPanel.CustomMinimumSize = new Vector2(600, 400);
        _offerPanel.Visible = false; 
        AddChild(_offerPanel);
        
        _offerVBox = new VBoxContainer();
        _offerPanel.AddChild(_offerVBox);
        
        _offerTitle = new Label();
        _offerTitle.Text = "选择你的祝福";
        _offerTitle.AddThemeFontSizeOverride("font_size", 20);
        _offerTitle.Alignment = HorizontalAlignment.Center;
        _offerVBox.AddChild(_offerTitle);
        
        var hint = new Label();
        hint.Text = "选择一个祝福激活";
        hint.Alignment = HorizontalAlignment.Center;
        _offerVBox.AddChild(hint);
        
        _offerContainer = new HBoxContainer();
        _offerContainer.Alignment = BoxContainer.AlignmentMode.Center;
        _offerContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _offerVBox.AddChild(_offerContainer);
        
        var offerSpacer = new Control();
        offerSpacer.CustomMinimumSize = new Vector2(0, 20);
        _offerVBox.AddChild(offerSpacer);
        
        _closeOfferButton = new Button();
        _closeOfferButton.Text = "  暂时放弃  ";
        _closeOfferButton.Pressed += () =>
        {
            RandomBoonSystem.Instance.CancelOffer();
            HideOfferPanel();
        };
        _offerVBox.AddChild(_closeOfferButton);
    }
    
    private void SetupSignals()
    {
        if (RandomBoonSystem.Instance == null) return;
        
        RandomBoonSystem.Instance.BoonOffered += OnBoonOffered;
        RandomBoonSystem.Instance.BoonActivated += OnBoonActivated;
        RandomBoonSystem.Instance.BoonRemoved += OnBoonRemoved;
    }
    
    private void OnBoonOffered(List<BoonData> boons)
    {
        ShowOfferPanel(boons);
    }
    
    private void OnBoonActivated(BoonData boon)
    {
        RefreshUI();
    }
    
    private void OnBoonRemoved(string boonId)
    {
        RefreshUI();
    }
    
    private void ShowOfferPanel(List<BoonData> boons)
    {
        _isOfferMode = true;
        _offerPanel.Visible = true;
        
        // Clear old buttons
        foreach (var child in _offerContainer.GetChildren())
            child.QueueFree();
        
        // Create buttons for each offered boon
        foreach (var boon in boons)
        {
            var button = CreateBoonButton(boon, true);
            _offerContainer.AddChild(button);
        }
        
        // Show main panel if hidden
        _mainPanel.Visible = true;
    }
    
    private void HideOfferPanel()
    {
        _isOfferMode = false; 
        _offerPanel.Visible = false; 
        RefreshUI();
    }
    
    private void RefreshUI()
    {
        // Refresh owned boons
        RefreshOwnedBoons();
        
        // Refresh active boons
        RefreshActiveBoons();
        
        // Update stats
        var stats = RandomBoonSystem.Instance.GetStatistics();
        _statsLabel.Text = $"激活: {stats["active_count"]}/{stats["max_active"]}";
    }
    
    private void RefreshOwnedBoons()
    {
        foreach (var child in _ownedGrid.GetChildren())
            child.QueueFree();
        
        var ownedBoons = RandomBoonSystem.Instance.GetOwnedBoons();
        foreach (var boon in ownedBoons)
        {
            var button = CreateBoonButton(boon, false);
            _ownedGrid.AddChild(button);
        }
        
        if (ownedBoons.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "暂无祝福";
            emptyLabel.Alignment = HorizontalAlignment.Center;
            _ownedGrid.AddChild(emptyLabel);
        }
    }
    
    private void RefreshActiveBoons()
    {
        foreach (var child in _activeGrid.GetChildren())
            child.QueueFree();
        
        var activeBoons = RandomBoonSystem.Instance.GetActiveBoons();
        foreach (var boon in activeBoons)
        {
            var button = CreateBoonButton(boon, false);
            button.Modulate = new Color(1f, 1f, 1f, 0.8f);
            _activeGrid.AddChild(button);
        }
        
        if (activeBoons.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "暂无激活的祝福";
            emptyLabel.Alignment = HorizontalAlignment.Center;
            _activeGrid.AddChild(emptyLabel);
        }
    }
    
    private Button CreateBoonButton(BoonData boon, bool isOfferMode)
    {
        var button = new Button();
        button.CustomMinimumSize = new Vector2(140, 80);
        
        var vbox = new VBoxContainer();
        button.AddChild(vbox);
        
        // Rarity color
        var rarityColor = Color.FromHex(BoonDatabase.GetRarityColor(boon.Rarity));
        
        // Name
        var nameLabel = new Label();
        nameLabel.Text = boon.Name;
        nameLabel.AddThemeFontSizeOverride("font_size", 14);
        nameLabel.Modulate = rarityColor;
        nameLabel.Alignment = HorizontalAlignment.Center;
        vbox.AddChild(nameLabel);
        
        // Type
        var typeLabel = new Label();
        typeLabel.Text = BoonDatabase.GetTypeName(boon.Type);
        typeLabel.AddThemeFontSizeOverride("font_size", 10);
        typeLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
        typeLabel.Alignment = HorizontalAlignment.Center;
        vbox.AddChild(typeLabel);
        
        // Description
        var descLabel = new Label();
        descLabel.Text = boon.Description;
        descLabel.AddThemeFontSizeOverride("font_size", 10);
        descLabel.Modulate = new Color(0.6f, 0.6f, 0.6f);
        descLabel.Alignment = HorizontalAlignment.Center;
        descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        vbox.AddChild(descLabel);
        
        // Click handler
        if (isOfferMode)
        {
            button.Pressed += () => OnOfferButtonPressed(boon);
        }
        else
        {
            button.Pressed += () => OnBoonButtonPressed(boon);
        }
        
        return button;
    }
    
    private void OnOfferButtonPressed(BoonData boon)
    {
        var offer = RandomBoonSystem.Instance.GetCurrentOffer();
        if (offer == null) return;
        
        int index = offer.IndexOf(boon);
        if (index >= 0)
        {
            RandomBoonSystem.Instance.SelectBoon(index);
            HideOfferPanel();
        }
    }
    
    private void OnBoonButtonPressed(BoonData boon)
    {
        // Toggle activation
        if (RandomBoonSystem.Instance.GetActiveBoons().Contains(boon))
        {
            RandomBoonSystem.Instance.DeactivateBoon(boon.Id);
        }
        else
        {
            RandomBoonSystem.Instance.ActivateBoon(boon.Id);
        }
    }
    
    public void ToggleUI()
    {
        Visible = !Visible;
        
        if (Visible)
        {
            RefreshUI();
            
            // Check if there's an offer waiting
            if (RandomBoonSystem.Instance.IsOffering())
            {
                var offer = RandomBoonSystem.Instance.GetCurrentOffer();
                if (offer != null && offer.Count > 0)
                {
                    ShowOfferPanel(offer);
                }
            }
        }
    }
    
    public override void _Input(InputEvent evt)
    {
        if (evt.IsActionPressed("ui_cancel") || evt.IsActionPressed("ui_boon"))
        {
            ToggleUI();
            GetViewport().SetInputAsHandled();
        }
    }
}
