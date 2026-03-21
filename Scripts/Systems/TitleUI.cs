using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Title UI - displays title collection, details and allows equipping titles
/// </summary>
public class TitleUI : Control
{
    // UI Elements
    private Label _titleLabel;
    private Label _equippedTitleLabel;
    private ItemList _titleList;
    private Button _equipButton;
    private Button _unequipButton;
    private Button _closeButton;
    private Label _statsLabel;
    private Label _detailLabel;
    
    // Filter
    private OptionButton _categoryFilter;
    private CheckButton _showUnlockedOnly;
    
    // Data
    private List<TitleData> _displayedTitles = new List<TitleData>();
    private TitleCategory _currentCategory = TitleCategory.Combat;
    private bool _showUnlockedOnlyFilter = false;

    // Toggle
    private bool _isVisible = false;

    public override void _Ready()
    {
        SetupUI();
        PopulateTitleList();
        
        // Connect signals
        if (_equipButton != null)
            _equipButton.Pressed += OnEquipButtonPressed;
        if (_unequipButton != null)
            _unequipButton.Pressed += OnUnequipButtonPressed;
        if (_closeButton != null)
            _closeButton.Pressed += OnCloseButtonPressed;
        if (_categoryFilter != null)
            _categoryFilter.ItemSelected += OnCategorySelected;
        if (_showUnlockedOnly != null)
            _showUnlockedOnly.Toggled += OnUnlockedOnlyToggled;
        if (_titleList != null)
            _titleList.ItemSelected += OnTitleSelected;

        // Input handling
        VisibilityChanged += OnVisibilityChanged;
    }

    private void SetupUI()
    {
        // Main container
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainContainer.Position = new Vector2(400, 150);
        mainContainer.CustomMinimumSize = new Vector2(800, 600);
        AddChild(mainContainer);

        // Title header
        _titleLabel = new Label();
        _titleLabel.Text = "Title System";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainContainer.AddChild(_titleLabel);

        // Equipped title display
        _equippedTitleLabel = new Label();
        _equippedTitleLabel.Text = "Equipped: None";
        _equippedTitleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _equippedTitleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f)); // Gold color
        mainContainer.AddChild(_equippedTitleLabel);

        // Filter container
        var filterContainer = new HBoxContainer();
        mainContainer.AddChild(filterContainer);

        // Category filter
        var categoryLabel = new Label();
        categoryLabel.Text = "Category: ";
        filterContainer.AddChild(categoryLabel);

        _categoryFilter = new OptionButton();
        _categoryFilter.AddItem("Combat", (int)TitleCategory.Combat);
        _categoryFilter.AddItem("Exploration", (int)TitleCategory.Exploration);
        _categoryFilter.AddItem("Collection", (int)TitleCategory.Collection);
        _categoryFilter.AddItem("Social", (int)TitleCategory.Social);
        _categoryFilter.AddItem("Economy", (int)TitleCategory.Economy);
        _categoryFilter.AddItem("Special", (int)TitleCategory.Special);
        _categoryFilter.AddItem("Seasonal", (int)TitleCategory.Seasonal);
        _categoryFilter.Selected = 0;
        filterContainer.AddChild(_categoryFilter);

        // Show unlocked only
        var unlockLabel = new Label();
        unlockLabel.Text = "  Show Unlocked Only: ";
        filterContainer.AddChild(unlockLabel);

        _showUnlockedOnly = new CheckButton();
        _showUnlockedOnly.Toggled += (pressed) => OnUnlockedOnlyToggled(pressed);
        filterContainer.AddChild(_showUnlockedOnly);

        // Title list
        _titleList = new ItemList();
        _titleList.CustomMinimumSize = new Vector2(760, 350);
        _titleList.MultipleSelection = false;
        mainContainer.AddChild(_titleList);

        // Detail panel
        _detailLabel = new Label();
        _detailLabel.Text = "Select a title to view details";
        _detailLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        mainContainer.AddChild(_detailLabel);

        // Button container
        var buttonContainer = new HBoxContainer();
        buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
        mainContainer.AddChild(buttonContainer);

        // Equip button
        _equipButton = new Button();
        _equipButton.Text = "Equip Title";
        _equipButton.CustomMinimumSize = new Vector2(150, 40);
        buttonContainer.AddChild(_equipButton);

        // Unequip button
        _unequipButton = new Button();
        _unequipButton.Text = "Unequip";
        _unequipButton.CustomMinimumSize = new Vector2(150, 40);
        buttonContainer.AddChild(_unequipButton);

        // Close button
        _closeButton = new Button();
        _closeButton.Text = "Close (ESC)";
        _closeButton.CustomMinimumSize = new Vector2(150, 40);
        buttonContainer.AddChild(_closeButton);

        // Stats label
        _statsLabel = new Label();
        _statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        UpdateStats();
        mainContainer.AddChild(_statsLabel);

        // Hide initially
        Visible = false;
    }

    private void PopulateTitleList()
    {
        if (_titleList == null) return;

        _titleList.Clear();
        _displayedTitles.Clear();

        var titles = TitleSystem.Instance.GetAllTitles();
        
        foreach (var title in titles)
        {
            // Apply filters
            if (title.Category != _currentCategory)
                continue;
            
            if (_showUnlockedOnlyFilter && !title.IsUnlocked)
                continue;

            _displayedTitles.Add(title);
            
            // Format display text
            string rarityColor = GetRarityColor(title.Rarity);
            string status = title.IsUnlocked ? " [Unlocked]" : " [Locked]";
            string displayText = $"{rarityColor}{title.TitleName}{status}";
            
            _titleList.AddItem(displayText);
            
            // Set item metadata
            int index = _displayedTitles.Count - 1;
            _titleList.SetItemMetadata(index, title.TitleId);
        }
    }

    private string GetRarityColor(TitleRarity rarity)
    {
        switch (rarity)
        {
            case TitleRarity.Common:
                return "[White]";
            case TitleRarity.Uncommon:
                return "[Green]";
            case TitleRarity.Rare:
                return "[Blue]";
            case TitleRarity.Epic:
                return "[Purple]";
            case TitleRarity.Legendary:
                return "[Gold]";
            default:
                return "";
        }
    }

    private void UpdateStats()
    {
        if (_statsLabel == null) return;

        int totalUnlocked = TitleSystem.Instance.GetTotalUnlockedCount();
        int totalTitles = TitleSystem.Instance.GetAllTitles().Count;
        
        _statsLabel.Text = $"Total Titles Unlocked: {totalUnlocked} / {totalTitles}";
    }

    private void UpdateEquippedDisplay()
    {
        if (_equippedTitleLabel == null) return;

        string equipped = TitleSystem.Instance.GetEquippedTitleName();
        if (string.IsNullOrEmpty(equipped))
        {
            _equippedTitleLabel.Text = "Equipped: None";
        }
        else
        {
            _equippedTitleLabel.Text = $"Equipped: {equipped}";
        }
    }

    private void OnCategorySelected(int index)
    {
        _currentCategory = (TitleCategory)index;
        PopulateTitleList();
    }

    private void OnUnlockedOnlyToggled(bool pressed)
    {
        _showUnlockedOnlyFilter = pressed;
        PopulateTitleList();
    }

    private void OnTitleSelected(int index)
    {
        if (index < 0 || index >= _displayedTitles.Count) return;

        var title = _displayedTitles[index];
        
        if (_detailLabel != null)
        {
            string rarityStr = title.Rarity.ToString();
            string categoryStr = title.Category.ToString();
            string status = title.IsUnlocked ? "Unlocked" : "Locked";
            string unlockTime = title.IsUnlocked ? title.UnlockTime.ToString() : "N/A";
            
            _detailLabel.Text = $"Title: {title.TitleName}\n" +
                $"Description: {title.Description}\n" +
                $"Category: {categoryStr}\n" +
                $"Rarity: {rarityStr}\n" +
                $"Requirement: {title.RequiredValue}\n" +
                $"Status: {status}\n" +
                $"Unlock Time: {unlockTime}";
        }

        // Update button states
        if (_equipButton != null)
        {
            _equipButton.Disabled = !title.IsUnlocked || TitleSystem.Instance.GetEquippedTitle() == title.TitleId;
        }
    }

    private void OnEquipButtonPressed()
    {
        var selectedItems = _titleList.GetSelectedItems();
        if (selectedItems.Length == 0) return;

        int index = selectedItems[0];
        if (index < 0 || index >= _displayedTitles.Count) return;

        var title = _displayedTitles[index];
        if (title.IsUnlocked)
        {
            TitleSystem.Instance.EquipTitle(title.TitleId);
            UpdateEquippedDisplay();
            PopulateTitleList();
        }
    }

    private void OnUnequipButtonPressed()
    {
        TitleSystem.Instance.UnequipTitle();
        UpdateEquippedDisplay();
        PopulateTitleList();
    }

    private void OnCloseButtonPressed()
    {
        ToggleVisibility();
    }

    public void ToggleVisibility()
    {
        _isVisible = !_isVisible;
        Visible = _isVisible;
        
        if (_isVisible)
        {
            PopulateTitleList();
            UpdateStats();
            UpdateEquippedDisplay();
        }
    }

    private void OnVisibilityChanged()
    {
        if (Visible)
        {
            PopulateTitleList();
            UpdateStats();
            UpdateEquippedDisplay();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // Toggle with T key
            if (keyEvent.Keycode == Key.T)
            {
                ToggleVisibility();
                GetTree().SetInputAsHandled();
            }
            // Close with Escape
            else if (keyEvent.Keycode == Key.Escape && Visible)
            {
                ToggleVisibility();
                GetTree().SetInputAsHandled();
            }
        }
    }
}
