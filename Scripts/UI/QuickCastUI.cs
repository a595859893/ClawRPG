using Godot;
using System;
using System.Collections.Generic;

public partial class QuickCastUI : Control
{
    private static QuickCastUI _instance;
    public static QuickCastUI Instance => _instance;
    
    [Export] public Color ReadyColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
    [Export] public Color CooldownColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
    [Export] public Color EmptyColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [Export] public Color AssignedColor = new Color(0.2f, 0.6f, 1.0f, 0.9f);
    
    private VBoxContainer _mainContainer;
    private HBoxContainer _slotsContainer;
    private Label _titleLabel;
    private Label _statsLabel;
    private Label _instructionsLabel;
    private List<TextureRect> _slotIcons = new List<TextureRect>();
    private List<ProgressBar> _cooldownBars = new List<ProgressBar>();
    private List<Label> _slotLabels = new List<Label>();
    private List<Label> _keyLabels = new List<Label>();
    
    private bool _isVisible = false;
    private bool _isInitialized = false;
    
    public override void _Ready()
    {
        _instance = this;
        SetupUI();
        
        // Connect to QuickCastSystem
        if (QuickCastSystem.Instance != null)
        {
            QuickCastSystem.Instance._Ready();
        }
        
        Hide();
    }
    
    private void SetupUI()
    {
        // Main container
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainContainer.Position = new Vector2(-300, -250);
        _mainContainer.Size = new Vector2(600, 500);
        _mainContainer.Modulate = new Color(1, 1, 1, 0.95f);
        AddChild(_mainContainer);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "⚡ Quick Cast System";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _mainContainer.AddChild(_titleLabel);
        
        // Separator
        _mainContainer.AddChild(CreateSeparator());
        
        // Slots container
        _slotsContainer = new HBoxContainer();
        _slotsContainer.Alignment = BoxContainer.AlignmentMode.Center;
        _slotsContainer.CustomMinimumSize = new Vector2(0, 200);
        _mainContainer.AddChild(_slotsContainer);
        
        // Create 9 slots
        for (int i = 0; i < 9; i++)
        {
            var slotContainer = new VBoxContainer();
            slotContainer.CustomMinimumSize = new Vector2(60, 80);
            slotContainer.Alignment = BoxContainer.AlignmentMode.Center;
            
            // Key label (1-9)
            var keyLabel = new Label();
            keyLabel.Text = (i + 1).ToString();
            keyLabel.HorizontalAlignment = HorizontalAlignment.Center;
            keyLabel.AddThemeFontSizeOverride("font_size", 14);
            keyLabel.AddThemeColorOverride("font_color", new Color(1, 0.9f, 0.5f, 1));
            slotContainer.AddChild(keyLabel);
            _keyLabels.Add(keyLabel);
            
            // Slot icon
            var slotIcon = new TextureRect();
            slotIcon.CustomMinimumSize = new Vector2(50, 50);
            slotIcon.Modulate = EmptyColor;
            slotContainer.AddChild(slotIcon);
            _slotIcons.Add(slotIcon);
            
            // Cooldown bar
            var cooldownBar = new ProgressBar();
            cooldownBar.CustomMinimumSize = new Vector2(50, 6);
            cooldownBar.Value = 100;
            cooldownBar.ShowPercentage = false;
            cooldownBar.Modulate = new Color(0.3f, 0.8f, 1f, 0.8f);
            slotContainer.AddChild(cooldownBar);
            _cooldownBars.Add(cooldownBar);
            
            // Slot label (item name)
            var slotLabel = new Label();
            slotLabel.Text = "Empty";
            slotLabel.HorizontalAlignment = HorizontalAlignment.Center;
            slotLabel.AddThemeFontSizeOverride("font_size", 10);
            slotLabel.CustomMinimumSize = new Vector2(60, 20);
            slotContainer.AddChild(slotLabel);
            _slotLabels.Add(slotLabel);
            
            _slotsContainer.AddChild(slotContainer);
        }
        
        // Separator
        _mainContainer.AddChild(CreateSeparator());
        
        // Statistics
        _statsLabel = new Label();
        _statsLabel.Text = "Statistics: Casts: 0 | Success Rate: 0%";
        _statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _statsLabel.AddThemeFontSizeOverride("font_size", 14);
        _mainContainer.AddChild(_statsLabel);
        
        // Instructions
        _instructionsLabel = new Label();
        _instructionsLabel.Text = "Press 1-9 to use item | Auto-assign potions: [A] | Clear slot: [C + #]";
        _instructionsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _instructionsLabel.AddThemeFontSizeOverride("font_size", 12);
        _instructionsLabel.Modulate = new Color(0.7f, 0.7f, 0.7f, 1);
        _mainContainer.AddChild(_instructionsLabel);
        
        // Close hint
        var closeHint = new Label();
        closeHint.Text = "Press [ or ESC to close";
        closeHint.HorizontalAlignment = HorizontalAlignment.Center;
        closeHint.AddThemeFontSizeOverride("font_size", 11);
        closeHint.Modulate = new Color(0.5f, 0.5f, 0.5f, 1);
        _mainContainer.AddChild(closeHint);
        
        _isInitialized = true;
    }
    
    private Control CreateSeparator()
    {
        var separator = new Control();
        separator.CustomMinimumSize = new Vector2(0, 10);
        return separator;
    }
    
    public override void _Process(double delta)
    {
        if (!_isInitialized || !_isVisible) return;
        
        UpdateSlotDisplay();
        UpdateStatistics();
    }
    
    private void UpdateSlotDisplay()
    {
        var quickCast = QuickCastSystem.Instance;
        if (quickCast == null) return;
        
        for (int i = 0; i < 9; i++)
        {
            var slot = quickCast.GetSlot(i);
            var slotIcon = _slotIcons[i];
            var cooldownBar = _cooldownBars[i];
            var slotLabel = _slotLabels[i];
            
            if (slot != null && slot.IsAssigned && !string.IsNullOrEmpty(slot.ItemId))
            {
                // Slot has item assigned
                slotIcon.Modulate = AssignedColor;
                slotLabel.Text = slot.ItemName.Length > 8 ? slot.ItemName.Substring(0, 8) : slot.ItemName;
                
                // Update cooldown
                if (slot.CooldownRemaining > 0)
                {
                    cooldownBar.Value = (1 - slot.CooldownRemaining / slot.CooldownTime) * 100;
                    cooldownBar.Visible = true;
                    slotIcon.Modulate = CooldownColor;
                }
                else
                {
                    cooldownBar.Value = 100;
                    cooldownBar.Visible = false;
                    slotIcon.Modulate = ReadyColor;
                }
            }
            else
            {
                // Empty slot
                slotIcon.Modulate = EmptyColor;
                slotLabel.Text = "Empty";
                cooldownBar.Value = 100;
                cooldownBar.Visible = false;
            }
        }
    }
    
    private void UpdateStatistics()
    {
        var quickCast = QuickCastSystem.Instance;
        if (quickCast == null) return;
        
        int totalCasts = quickCast.GetTotalCasts();
        float successRate = quickCast.GetSuccessRate() * 100;
        string mostUsed = quickCast.GetMostUsedItem();
        
        _statsLabel.Text = $"Casts: {totalCasts} | Success: {successRate:F1}% | Most Used: {(string.IsNullOrEmpty(mostUsed) ? "None" : mostUsed)}";
    }
    
    public void Toggle()
    {
        if (_isVisible)
        {
            HideQuickCastUI();
        }
        else
        {
            ShowQuickCastUI();
        }
    }
    
    public void ShowQuickCastUI()
    {
        _isVisible = true;
        Show();
        UpdateSlotDisplay();
    }
    
    public void HideQuickCastUI()
    {
        _isVisible = false;
        Hide();
    }
    
    public override void _Input(InputEvent eventArgs)
    {
        if (eventArgs is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.BracketLeft || keyEvent.Keycode == Key.Escape)
            {
                if (_isVisible)
                {
                    HideQuickCastUI();
                }
            }
            else if (keyEvent.Keycode == Key.A)
            {
                // Auto-assign potions
                QuickCastSystem.Instance?.AutoAssignPotions();
            }
            else if (keyEvent.Keycode >= Key.Key1 && keyEvent.Keycode <= Key.Key9)
            {
                // Use slot directly
                int slotIndex = keyEvent.Keycode - Key.Key1;
                QuickCastSystem.Instance?.UseQuickCastSlot(slotIndex);
            }
        }
    }
}
