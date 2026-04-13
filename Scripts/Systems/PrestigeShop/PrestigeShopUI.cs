using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.PrestigeShop;
using PrestigeShopItem = ClawRPG.Scripts.Systems.PrestigeShop.ShopItem;
using ShopSystemItem = ClawRPG.Scripts.Systems.ShopItem;

/// <summary>
/// Prestige Shop UI panel - displays all cosmetic items available for purchase or auto-unlock.
/// Purely cosmetic, zero gameplay balance impact.
/// </summary>
public partial class PrestigeShopUI : Control
{
    // ===== Constants =====
    private const int PANEL_W = 900;
    private const int PANEL_H = 650;
    private const int ITEM_CARD_H = 90;

    // ===== Core references =====
    private PrestigeShopSystem _shop;
    private PrestigeSystem _prestige;

    // ===== UI Components =====
    private Panel _mainPanel;
    private Label _titleLabel;
    private Label _pointsLabel;
    private Label _tierBadge;
    private TabContainer _tabContainer;
    private Button _closeButton;
    private Button _refreshButton;

    // Tab containers
    private ScrollContainer[] _tabScrolls;
    private VBoxContainer[] _tabVBoxes;
    private ShopCategory[] _tabCategories;

    // Item cache
    private Dictionary<string, Control> _itemCards = new Dictionary<string, Control>();

    public override void _Ready()
    {
        _shop = PrestigeShopSystem.Instance;
        _prestige = PrestigeSystem.Instance;

        if (_shop == null)
        {
            GD.PrintErr("[PrestigeShopUI] PrestigeShopSystem not found!");
            return;
        }

        // Subscribe to signals
        _shop.ItemPurchased += OnItemPurchased;
        _shop.ItemUnlocked += OnItemUnlocked;
        _shop.TierAutoUnlocked += OnTierAutoUnlocked;

        BuildUI();
        RefreshAllTabs();
    }

    public override void _ExitTree()
    {
        if (_shop != null)
        {
            _shop.ItemPurchased -= OnItemPurchased;
            _shop.ItemUnlocked -= OnItemUnlocked;
            _shop.TierAutoUnlocked -= OnTierAutoUnlocked;
        }
    }

    // ===== UI Construction =====

    private void BuildUI()
    {
        // Semi-transparent background overlay
        var bg = new ColorRect();
        bg.Color = new Color(0, 0, 0, 0.6f);
        bg.SetAnchorAndMargin(Control.LayoutPreset.FullRect, 0);
        AddChild(bg);

        // Main panel
        _mainPanel = new Panel();
        _mainPanel.SetSize(new Vector2(PANEL_W, PANEL_H));
        _mainPanel.Position = new Vector2((GetViewportRect().Size.x - PANEL_W) / 2,
                                           (GetViewportRect().Size.y - PANEL_H) / 2);
        _mainPanel.Modulate = new Color(1, 1, 1, 0.97f);
        AddChild(_mainPanel);

        var mainVBox = new VBoxContainer();
        mainVBox.SetAnchorAndMargin(Control.LayoutPreset.FullRect, 0);
        mainVBox.AddThemeConstantOverride("separation", 0);
        _mainPanel.AddChild(mainVBox);

        // === HEADER ===
        var header = BuildHeader();
        mainVBox.AddChild(header);

        // === TAB CONTAINER ===
        _tabContainer = new TabContainer();
        _tabContainer.SetHSizeFlags(Control.SizeFlags.ExpandFill);
        _tabContainer.SetCustomMinimumSize(new Vector2(0, PANEL_H - 140));
        _tabContainer.TabChanged += OnTabChanged;
        mainVBox.AddChild(_tabContainer);

        // Build tabs
        _tabCategories = new[] { ShopCategory.Title, ShopCategory.PetAura, ShopCategory.PortalEffect, ShopCategory.FarewellFx };
        string[] tabNames = { "🏅 Titles", "✨ Pet Auras", "🌀 Portals", "🎆 Farewell FX" };
        _tabScrolls = new ScrollContainer[_tabCategories.Length];
        _tabVBoxes = new VBoxContainer[_tabCategories.Length];

        for (int i = 0; i < _tabCategories.Length; i++)
        {
            var tabPanel = new Control();
            tabPanel.SetHSizeFlags(Control.SizeFlags.ExpandFill | Control.SizeFlags.ShrinkBegin);
            _tabContainer.AddChild(tabPanel);

            var tabName = tabNames[i];

            var scroll = new ScrollContainer();
            scroll.SetAnchorAndMargin(Control.LayoutPreset.FullRect, 0);
            scroll.HScrollEnabled = false;
            tabPanel.AddChild(scroll);
            _tabScrolls[i] = scroll;

            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 8);
            scroll.AddChild(vbox);
            _tabVBoxes[i] = vbox;

            _tabContainer.SetTabTitle(i, tabName);
        }

        // Populate all tabs
        for (int i = 0; i < _tabCategories.Length; i++)
        {
            PopulateCategoryTab(i, _tabCategories[i]);
        }
    }

    private HBoxContainer BuildHeader()
    {
        var header = new HBoxContainer();
        header.SetCustomMinimumSize(new Vector2(0, 70));
        header.AddThemeConstantOverride("separation", 10);
        header.AddThemeStyleboxOverride("panel", new StyleBoxEmpty());

        // Left: Title
        _titleLabel = new Label();
        _titleLabel.Text = "⭐ Prestige Shop";
        _titleLabel.AddThemeFontSizeOverride("font_size", 26);
        _titleLabel.VerticalAlignment = VerticalAlignment.Center;
        header.AddChild(_titleLabel);

        // Spacer
        var spacer1 = new Control();
        spacer1.SetHSizeFlags(Control.SizeFlags.ExpandFill);
        header.AddChild(spacer1);

        // Tier badge
        _tierBadge = new Label();
        _tierBadge.VerticalAlignment = VerticalAlignment.Center;
        _tierBadge.HorizontalAlignment = HorizontalAlignment.Center;
        _tierBadge.AddThemeStyleboxOverride("normal", TierBadgeStylebox());
        _tierBadge.AddThemeFontSizeOverride("font_size", 16);
        _tierBadge.SetCustomMinimumSize(new Vector2(120, 36));
        header.AddChild(_tierBadge);

        // Points display
        var pointsBox = new HBoxContainer();
        pointsBox.VerticalAlignment = VerticalAlignment.Center;
        pointsBox.AddThemeConstantOverride("separation", 6);
        header.AddChild(pointsBox);

        var ptsIcon = new Label();
        ptsIcon.Text = "💎";
        ptsIcon.AddThemeFontSizeOverride("font_size", 20);
        ptsIcon.VerticalAlignment = VerticalAlignment.Center;
        pointsBox.AddChild(ptsIcon);

        _pointsLabel = new Label();
        _pointsLabel.AddThemeFontSizeOverride("font_size", 22);
        _pointsLabel.VerticalAlignment = VerticalAlignment.Center;
        pointsBox.AddChild(_pointsLabel);

        // Refresh button
        _refreshButton = new Button();
        _refreshButton.Text = "🔄";
        _refreshButton.SetCustomMinimumSize(new Vector2(40, 36));
        _refreshButton.Pressed += OnRefreshPressed;
        header.AddChild(_refreshButton);

        // Close button
        _closeButton = new Button();
        _closeButton.Text = "✕";
        _closeButton.SetCustomMinimumSize(new Vector2(44, 44));
        _closeButton.Pressed += OnClosePressed;
        _closeButton.AddThemeFontSizeOverride("font_size", 20);
        header.AddChild(_closeButton);

        return header;
    }

    private StyleBoxFlat TierBadgeStylebox()
    {
        var s = new StyleBoxFlat();
        s.BgColor = new Color(0.2f, 0.15f, 0.3f, 0.9f);
        s.CornerRadiusTopLeft = 8;
        s.CornerRadiusTopRight = 8;
        s.CornerRadiusBottomLeft = 8;
        s.CornerRadiusBottomRight = 8;
        s.ContentMarginLeft = 12;
        s.ContentMarginRight = 12;
        s.ContentMarginTop = 6;
        s.ContentMarginBottom = 6;
        return s;
    }

    // ===== Tab Population =====

    private void PopulateCategoryTab(int tabIndex, ShopCategory category)
    {
        var items = PrestigeShopDatabase.GetByCategory(category);
        var vbox = _tabVBoxes[tabIndex];

        foreach (var item in items)
        {
            var card = BuildItemCard(item);
            vbox.AddChild(card);
            _itemCards[item.ItemId] = card;
        }
    }

    private Control BuildItemCard(PrestigeShopItem item)
    {
        bool isUnlocked = _shop.IsUnlocked(item.ItemId);
        bool isPurchased = _shop.IsPurchased(item.ItemId);
        int currentPoints = _shop.GetPrestigePoints();
        bool canAfford = currentPoints >= item.Cost;
        bool isAutoTier = item.UnlockType == UnlockType.AutoTier;

        // Card panel
        var card = new Panel();
        card.SetCustomMinimumSize(new Vector2(0, ITEM_CARD_H));
        card.SetHSizeFlags(Control.SizeFlags.ExpandFill);
        card.AddThemeStyleboxOverride("panel", ItemCardStylebox(isUnlocked, isPurchased));

        var hbox = new HBoxContainer();
        hbox.SetAnchorAndMargin(Control.LayoutPreset.FullRect, 0);
        hbox.AddThemeConstantOverride("separation", 12);
        card.AddChild(hbox);

        // Left: Icon
        var iconLabel = new Label();
        iconLabel.Text = item.IconEmoji;
        iconLabel.HorizontalAlignment = HorizontalAlignment.Center;
        iconLabel.VerticalAlignment = VerticalAlignment.Center;
        iconLabel.SetCustomMinimumSize(new Vector2(56, 0));
        iconLabel.AddThemeFontSizeOverride("font_size", 32);
        hbox.AddChild(iconLabel);

        // Center: Name + Description + unlock info
        var infoVBox = new VBoxContainer();
        infoVBox.SetHSizeFlags(Control.SizeFlags.ExpandFill);
        infoVBox.AddThemeConstantOverride("separation", 4);
        hbox.AddChild(infoVBox);

        var nameLabel = new Label();
        nameLabel.Text = item.DisplayName;
        nameLabel.HorizontalAlignment = HorizontalAlignment.Left;
        nameLabel.AddThemeFontSizeOverride("font_size", 17);
        nameLabel.AddThemeColorOverride("font_color", isUnlocked ? new Color(0.9f, 1f, 0.8f) : new Color(0.9f, 0.9f, 0.9f));
        infoVBox.AddChild(nameLabel);

        var descLabel = new Label();
        descLabel.Text = item.Description;
        descLabel.HorizontalAlignment = HorizontalAlignment.Left;
        descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        descLabel.AddThemeFontSizeOverride("font_size", 13);
        descLabel.AddThemeColorOverride("font_color", new Color(0.65f, 0.65f, 0.7f));
        descLabel.SetCustomMaximumSize(new Vector2(520, 50));
        infoVBox.AddChild(descLabel);

        // Unlock hint
        var hintLabel = new Label();
        if (isUnlocked)
        {
            hintLabel.Text = isPurchased ? $"✅ Purchased (Tier {item.RequiredTier})" : $"🎖️ {item.TierName} Tier Unlocked";
        }
        else if (isAutoTier)
        {
            int currentTier = _shop.GetPrestigeLevel();
            if (currentTier >= item.RequiredTier)
                hintLabel.Text = "🎖️ Auto-unlock available!";
            else
                hintLabel.Text = $"🔒 Unlocks at {item.TierName} Tier ({item.RequiredTier})";
        }
        else
        {
            hintLabel.Text = $"💎 {item.Cost} points";
        }
        hintLabel.HorizontalAlignment = HorizontalAlignment.Left;
        hintLabel.AddThemeFontSizeOverride("font_size", 12);
        hintLabel.AddThemeColorOverride("font_color", isUnlocked ? new Color(0.4f, 0.9f, 0.5f) : new Color(0.5f, 0.5f, 0.6f));
        infoVBox.AddChild(hintLabel);

        // Right: Action button or status
        var actionContainer = new VBoxContainer();
        actionContainer.SetHSizeFlags(Control.SizeFlags.ShrinkEnd);
        actionContainer.VerticalAlignment = VerticalAlignment.Center;
        actionContainer.AddThemeConstantOverride("separation", 6);
        hbox.AddChild(actionContainer);

        if (isUnlocked)
        {
            var ownedLabel = new Label();
            ownedLabel.Text = "✅ Owned";
            ownedLabel.HorizontalAlignment = HorizontalAlignment.Center;
            ownedLabel.AddThemeFontSizeOverride("font_size", 15);
            ownedLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.85f, 0.4f));
            ownedLabel.SetCustomMinimumSize(new Vector2(100, 36));
            ownedLabel.VerticalAlignment = VerticalAlignment.Center;
            actionContainer.AddChild(ownedLabel);
        }
        else if (isAutoTier)
        {
            // Check if we can auto-claim
            int currentTier = _shop.GetPrestigeLevel();
            if (currentTier >= item.RequiredTier)
            {
                var claimBtn = new Button();
                claimBtn.Text = "Claim!";
                claimBtn.SetCustomMinimumSize(new Vector2(90, 36));
                claimBtn.Pressed += () => OnAutoClaimPressed(item);
                actionContainer.AddChild(claimBtn);
            }
            else
            {
                var tierReqLabel = new Label();
                tierReqLabel.Text = $"Tier {item.RequiredTier}";
                tierReqLabel.HorizontalAlignment = HorizontalAlignment.Center;
                tierReqLabel.AddThemeFontSizeOverride("font_size", 14);
                tierReqLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.55f));
                tierReqLabel.SetCustomMinimumSize(new Vector2(100, 36));
                tierReqLabel.VerticalAlignment = VerticalAlignment.Center;
                actionContainer.AddChild(tierReqLabel);
            }
        }
        else
        {
            var buyBtn = new Button();
            buyBtn.Text = canAfford ? $"Buy" : "Not enough";
            buyBtn.SetCustomMinimumSize(new Vector2(100, 36));
            buyBtn.Disabled = !canAfford;
            buyBtn.Pressed += () => OnPurchasePressed(item);
            actionContainer.AddChild(buyBtn);

            if (canAfford)
            {
                var costLabel = new Label();
                costLabel.Text = $"💎 {item.Cost}";
                costLabel.HorizontalAlignment = HorizontalAlignment.Center;
                costLabel.AddThemeFontSizeOverride("font_size", 14);
                costLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.8f, 1f));
                actionContainer.AddChild(costLabel);
            }
        }

        return card;
    }

    private StyleBoxFlat ItemCardStylebox(bool isUnlocked, bool isPurchased)
    {
        var s = new StyleBoxFlat();
        if (isUnlocked && isPurchased)
            s.BgColor = new Color(0.1f, 0.25f, 0.12f, 0.85f);
        else if (isUnlocked)
            s.BgColor = new Color(0.12f, 0.18f, 0.28f, 0.85f);
        else
            s.BgColor = new Color(0.15f, 0.15f, 0.2f, 0.8f);

        s.CornerRadiusTopLeft = 10;
        s.CornerRadiusTopRight = 10;
        s.CornerRadiusBottomLeft = 10;
        s.CornerRadiusBottomRight = 10;
        s.ContentMarginLeft = 14;
        s.ContentMarginRight = 14;
        s.ContentMarginTop = 10;
        s.ContentMarginBottom = 10;

        // Border
        if (isUnlocked && isPurchased)
            s.BorderColor = new Color(0.2f, 0.6f, 0.3f, 0.5f);
        else if (isUnlocked)
            s.BorderColor = new Color(0.3f, 0.5f, 0.8f, 0.4f);
        else
            s.BorderColor = new Color(0.25f, 0.25f, 0.3f, 0.3f);
        s.BorderWidthLeft = 1;
        s.BorderWidthRight = 1;
        s.BorderWidthTop = 1;
        s.BorderWidthBottom = 1;
        return s;
    }

    // ===== Event Handlers =====

    private void OnPurchasePressed(PrestigeShopItem item)
    {
        bool success = _shop.PurchaseItem(item.ItemId);
        if (success)
        {
            RefreshAllTabs();
            UpdateHeader();
        }
    }

    private void OnAutoClaimPressed(PrestigeShopItem item)
    {
        // Auto-tier items are claimed by checking and updating tier
        _shop.OnPrestigeTierChanged(_shop.GetPrestigeLevel(), _shop.GetPrestigeTierName());
        RefreshAllTabs();
    }

    private void OnItemPurchased(string itemId, PrestigeShopItem item, int cost)
    {
        RefreshAllTabs();
        UpdateHeader();
        ShowNotification($"Purchased: {item.DisplayName}!");
    }

    private void OnItemUnlocked(string itemId, PrestigeShopItem item)
    {
        RefreshAllTabs();
    }

    private void OnTierAutoUnlocked(string tierName, int tierLevel)
    {
        RefreshAllTabs();
        ShowNotification($"🎖️ Reached {tierName} Tier! New rewards unlocked!");
    }

    private void OnTabChanged(long tabIndex)
    {
        // Tab switched - scroll to top
        if (tabIndex >= 0 && tabIndex < _tabScrolls.Length && _tabScrolls[tabIndex] != null)
            _tabScrolls[tabIndex].ScrollVertical = 0;
    }

    private void OnRefreshPressed()
    {
        RefreshAllTabs();
        UpdateHeader();
    }

    private void OnClosePressed()
    {
        HideShop();
    }

    // ===== Refresh =====

    private void RefreshAllTabs()
    {
        UpdateHeader();

        for (int i = 0; i < _tabCategories.Length; i++)
        {
            var vbox = _tabVBoxes[i];
            var category = _tabCategories[i];

            // Remove old cards
            foreach (var child in vbox.GetChildren())
            {
                child.QueueFree();
            }

            // Rebuild cards
            var items = PrestigeShopDatabase.GetByCategory(category);
            foreach (var item in items)
            {
                var card = BuildItemCard(item);
                vbox.AddChild(card);
                _itemCards[item.ItemId] = card;
            }
        }
    }

    private void UpdateHeader()
    {
        if (_shop == null) return;

        int points = _shop.GetPrestigePoints();
        string tierName = _shop.GetPrestigeTierName();
        int tierLevel = _shop.GetPrestigeLevel();

        _pointsLabel.Text = $"{points:N0}";
        _tierBadge.Text = $"  {tierName}  ";
        _tierBadge.AddThemeColorOverride("font_color", TierColor(tierLevel));
    }

    private Color TierColor(int level)
    {
        if (level <= 3) return new Color(0.8f, 0.5f, 0.2f);       // Bronze
        if (level <= 6) return new Color(0.75f, 0.75f, 0.75f);    // Silver
        if (level <= 10) return new Color(1f, 0.84f, 0f);          // Gold
        if (level <= 15) return new Color(0.9f, 0.9f, 0.95f);     // Platinum
        if (level <= 19) return new Color(0.73f, 0.95f, 1f);      // Diamond
        return new Color(1f, 0.42f, 0.42f);                        // Legendary
    }

    // ===== Notification =====

    private Timer _notificationTimer;

    private void ShowNotification(string text)
    {
        // Remove existing notification
        if (HasNode("NotificationLabel"))
            GetNode("NotificationLabel").QueueFree();

        var label = new Label();
        label.Name = "NotificationLabel";
        label.Text = text;
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.AddThemeFontSizeOverride("font_size", 18);
        label.AddThemeColorOverride("font_color", new Color(1f, 0.95f, 0.5f));
        label.SetAnchorsAndMarginsPreset(Control.LayoutPreset.CenterBottom, (int)Control.LayoutPresetMode.Preserve);
        label.Position = new Vector2(GetViewportRect().Size.x / 2 - 200, GetViewportRect().Size.y - 120);
        label.SetCustomMinimumSize(new Vector2(400, 50));
        label.ZIndex = 100;
        AddChild(label);

        // Auto-dismiss
        _notificationTimer = new Timer();
        _notificationTimer.WaitTime = 2.5f;
        _notificationTimer.OneShot = true;
        _notificationTimer.Timeout += () =>
        {
            if (label != null && label.IsInsideTree())
            {
                var tween = CreateTween();
                tween.TweenProperty(label, "modulate:a", 0f, 0.5f);
                tween.TweenCallback(Callable.From(() => label.QueueFree()));
            }
        };
        AddChild(_notificationTimer);
        _notificationTimer.Start();
    }

    // ===== Show/Hide API =====

    public void ShowShop()
    {
        Visible = true;
        RefreshAllTabs();
        UpdateHeader();
    }

    public void HideShop()
    {
        Visible = false;
    }

    public void ToggleShop()
    {
        if (Visible)
            HideShop();
        else
            ShowShop();
    }
}
