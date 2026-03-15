using Godot;
using System;
using System.Collections.Generic;

public class CharacterCreationUI : Control
{
    private CharacterCreationSystem _system;
    private CharacterCreationData _data;
    private CharacterCreationDatabase _database;
    
    // UI Elements
    private Label _titleLabel;
    private LineEdit _nameInput;
    private OptionButton _classSelector;
    private OptionButton _backgroundSelector;
    private OptionButton _hairStyleSelector;
    private OptionButton _skinColorSelector;
    private OptionButton _eyeColorSelector;
    
    // Attribute controls
    private Label _strengthLabel;
    private Label _agilityLabel;
    private Label _intelligenceLabel;
    private Label _vitalityLabel;
    private Label _luckLabel;
    private Label _pointsLabel;
    
    private Button _strengthMinus;
    private Button _strengthPlus;
    private Button _agilityMinus;
    private Button _agilityPlus;
    private Button _intelligenceMinus;
    private Button _intelligencePlus;
    private Button _vitalityMinus;
    private Button _vitalityPlus;
    private Button _luckMinus;
    private Button _luckPlus;
    
    // Class info
    private Label _classNameLabel;
    private Label _classDescriptionLabel;
    private Label _classStatsLabel;
    
    // Background info
    private Label _backgroundNameLabel;
    private Label _backgroundDescriptionLabel;
    private Label _backgroundBonusesLabel;
    
    // Buttons
    private Button _createButton;
    private Button _resetButton;
    
    // Stats
    private Label _statsLabel;
    
    private bool _isVisible = false;
    
    public override void _Ready()
    {
        _system = GetNode<CharacterCreationSystem>("/root/CharacterCreationSystem");
        _data = GetNode<CharacterCreationData>("/root/CharacterCreationData");
        _database = GetNode<CharacterCreationDatabase>("/root/CharacterCreationDatabase");
        
        SetupUI();
        UpdateUI();
        
        // Connect signals
        _system.Connect("AttributeChanged", this, "OnAttributeChanged");
        _system.Connect("ClassChanged", this, "OnClassChanged");
        _system.Connect("BackgroundChanged", this, "OnBackgroundChanged");
    }
    
    private void SetupUI()
    {
        // Main panel
        var mainPanel = new PanelContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            Visible = false
        };
        mainPanel.Name = "MainPanel";
        AddChild(mainPanel);
        
        // Create main container with scroll
        var scroll = new ScrollContainer()
        scroll.Name = "ScrollContainer";
        mainPanel.AddChild(scroll);
        
        var mainContainer = new VBoxContainer
        {
            AnchorRight = 1f,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        scroll.AddChild(mainContainer);
        
        // Title
        _titleLabel = new Label
        {
            Text = "⚔️ Character Creation ⚔️",
            Align = Label.AlignCenter,
            SizeFlagsHorizontal = Control.SizeFlags.Expand
        };
        _titleLabel.Set("custom_fonts/font", CreateTitleFont());
        mainContainer.AddChild(_titleLabel);
        
        // Name input
        var nameContainer = new HBoxContainer();
        mainContainer.AddChild(nameContainer);
        
        var nameLabel = new Label { Text = "Character Name: ", SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd };
        nameContainer.AddChild(nameLabel);
        
        _nameInput = new LineEdit
        {
            PlaceholderText = "Enter name...",
            SizeFlagsHorizontal = Control.SizeFlags.Expand
        };
        _nameInput.Connect("text_changed", this, "OnNameChanged");
        nameContainer.AddChild(_nameInput);
        
        // Class selection
        var classLabel = new Label { Text = "Class:", SizeFlagsVertical = Control.SizeFlags.ShrinkEnd };
        classLabel.Set("custom_fonts/font", CreateHeaderFont());
        mainContainer.AddChild(classLabel);
        
        _classSelector = new OptionButton
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        var classes = _system.GetAvailableClasses();
        foreach (var c in classes)
        {
            _classSelector.AddItem(c);
        }
        _classSelector.Connect("item_selected", this, "OnClassSelected");
        mainContainer.AddChild(_classSelector);
        
        // Class info
        _classNameLabel = new Label { Text = "", SizeFlagsVertical = Control.SizeFlags.ShrinkEnd };
        _classNameLabel.Set("custom_colors/font_color", new Color(1f, 0.8f, 0.2f));
        mainContainer.AddChild(_classNameLabel);
        
        _classDescriptionLabel = new Label { Text = "", SizeFlagsVertical = Control.SizeFlags.ShrinkEnd };
        mainContainer.AddChild(_classDescriptionLabel);
        
        _classStatsLabel = new Label { Text = "", SizeFlagsVertical = Control.SizeFlags.ShrinkEnd };
        _classStatsLabel.Set("custom_colors/font_color", new Color(0.7f, 0.7f, 0.7f));
        mainContainer.AddChild(_classStatsLabel);
        
        // Attributes section
        var attrHeader = new Label { Text = "\n📊 Attribute Points" };
        attrHeader.Set("custom_fonts/font", CreateHeaderFont());
        mainContainer.AddChild(attrHeader);
        
        _pointsLabel = new Label { Text = "Available Points: 20", SizeFlagsVertical = Control.SizeFlags.ShrinkEnd };
        _pointsLabel.Set("custom_colors/font_color", new Color(0.2f, 0.8f, 0.2f));
        mainContainer.AddChild(_pointsLabel);
        
        // Strength
        _strengthLabel = new Label { Text = "Strength: 10" };
        mainContainer.AddChild(CreateAttributeRow("Strength", _strengthLabel, out _strengthMinus, out _strengthPlus));
        
        // Agility
        _agilityLabel = new Label { Text = "Agility: 10" };
        mainContainer.AddChild(CreateAttributeRow("Agility", _agilityLabel, out _agilityMinus, out _agilityPlus));
        
        // Intelligence
        _intelligenceLabel = new Label { Text = "Intelligence: 10" };
        mainContainer.AddChild(CreateAttributeRow("Intelligence", _intelligenceLabel, out _intelligenceMinus, out _intelligencePlus));
        
        // Vitality
        _vitalityLabel = new Label { Text = "Vitality: 10" };
        mainContainer.AddChild(CreateAttributeRow("Vitality", _vitalityLabel, out _vitalityMinus, out _vitalityPlus));
        
        // Luck
        _luckLabel = new Label { Text = "Luck: 10" };
        mainContainer.AddChild(CreateAttributeRow("Luck", _luckLabel, out _luckMinus, out _luckPlus));
        
        // Background selection
        var bgHeader = new Label { Text = "\n📖 Background Story" };
        bgHeader.Set("custom_fonts/font", CreateHeaderFont());
        mainContainer.AddChild(bgHeader);
        
        _backgroundSelector = new OptionButton
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        var backgrounds = _system.GetAvailableBackgrounds();
        foreach (var b in backgrounds)
        {
            _backgroundSelector.AddItem(b);
        }
        _backgroundSelector.Connect("item_selected", this, "OnBackgroundSelected");
        mainContainer.AddChild(_backgroundSelector);
        
        _backgroundNameLabel = new Label { Text = "", SizeFlagsVertical = Control.SizeFlags.ShrinkEnd };
        _backgroundNameLabel.Set("custom_colors/font_color", new Color(1f, 0.8f, 0.2f));
        mainContainer.AddChild(_backgroundNameLabel);
        
        _backgroundDescriptionLabel = new Label { Text = "", SizeFlagsVertical = Control.SizeFlags.ShrinkEnd };
        mainContainer.AddChild(_backgroundDescriptionLabel);
        
        _backgroundBonusesLabel = new Label { Text = "", SizeFlagsVertical = Control.SizeFlags.ShrinkEnd };
        _backgroundBonusesLabel.Set("custom_colors/font_color", new Color(0.3f, 0.8f, 0.3f));
        mainContainer.AddChild(_backgroundBonusesLabel);
        
        // Appearance section
        var appearanceHeader = new Label { Text = "\n🎨 Appearance" };
        appearanceHeader.Set("custom_fonts/font", CreateHeaderFont());
        mainContainer.AddChild(appearanceHeader);
        
        var hairContainer = new HBoxContainer();
        mainContainer.AddChild(hairContainer);
        hairContainer.AddChild(new Label { Text = "Hair Style: ", SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd });
        
        _hairStyleSelector = new OptionButton { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        var hairStyles = _system.GetAvailableHairStyles();
        foreach (var h in hairStyles)
        {
            _hairStyleSelector.AddItem(h);
        }
        _hairStyleSelector.Connect("item_selected", this, "OnHairStyleSelected");
        hairContainer.AddChild(_hairStyleSelector);
        
        var skinContainer = new HBoxContainer();
        mainContainer.AddChild(skinContainer);
        skinContainer.AddChild(new Label { Text = "Skin Color: ", SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd });
        
        _skinColorSelector = new OptionButton { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        var skinColors = _system.GetAvailableSkinColors();
        foreach (var s in skinColors)
        {
            _skinColorSelector.AddItem(s);
        }
        _skinColorSelector.Connect("item_selected", this, "OnSkinColorSelected");
        skinContainer.AddChild(_skinColorSelector);
        
        var eyeContainer = new HBoxContainer();
        mainContainer.AddChild(eyeContainer);
        eyeContainer.AddChild(new Label { Text = "Eye Color: ", SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd });
        
        _eyeColorSelector = new OptionButton { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        var eyeColors = _system.GetAvailableEyeColors();
        foreach (var e in eyeColors)
        {
            _eyeColorSelector.AddItem(e);
        }
        _eyeColorSelector.Connect("item_selected", this, "OnEyeColorSelected");
        eyeContainer.AddChild(_eyeColorSelector);
        
        // Preview stats
        _statsLabel = new Label { Text = "\n📈 Preview Stats" };
        _statsLabel.Set("custom_fonts/font", CreateHeaderFont());
        mainContainer.AddChild(_statsLabel);
        
        UpdateStatsPreview();
        
        // Buttons
        var buttonContainer = new HBoxContainer();
        buttonContainer.Alignment = BoxContainer.AlignMode.Center;
        mainContainer.AddChild(buttonContainer);
        
        _createButton = new Button
        {
            Text = "✨ Create Character ✨",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        _createButton.Connect("pressed", this, "OnCreatePressed");
        buttonContainer.AddChild(_createButton);
        
        _resetButton = new Button
        {
            Text = "🔄 Reset"
        };
        _resetButton.Connect("pressed", this, "OnResetPressed");
        buttonContainer.AddChild(_resetButton);
        
        // Store reference
        mainPanel.Name = "MainPanel";
    }
    
    private Control CreateAttributeRow(string attrName, Label valueLabel, out Button minusBtn, out Button plusBtn)
    {
        var container = new HBoxContainer();
        
        valueLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        container.AddChild(valueLabel);
        
        minusBtn = new Button { Text = "-" };
        minusBtn.Connect("pressed", this, "OnMinusPressed", new Godot.Collections.Array { attrName });
        container.AddChild(minusBtn);
        
        plusBtn = new Button { Text = "+" };
        plusBtn.Connect("pressed", this, "OnPlusPressed", new Godot.Collections.Array { attrName });
        container.AddChild(plusBtn);
        
        return container;
    }
    
    private DynamicFont CreateTitleFont()
    {
        var font = new DynamicFont();
        font.FontData = GD.Load<DynamicFontData>("res://fonts/NormalFont.ttf");
        font.Size = 24;
        return font;
    }
    
    private DynamicFont CreateHeaderFont()
    {
        var font = new DynamicFont();
        font.FontData = GD.Load<DynamicFontData>("res://fonts/NormalFont.ttf");
        font.Size = 18;
        return font;
    }
    
    private void UpdateUI()
    {
        _nameInput.Text = _data.CharacterName;
        
        // Class
        var classes = _system.GetAvailableClasses();
        for (int i = 0; i < classes.Length; i++)
        {
            if (classes[i] == _data.SelectedClass)
            {
                _classSelector.Select(i);
                break;
            }
        }
        
        // Background
        var backgrounds = _system.GetAvailableBackgrounds();
        for (int i = 0; i < backgrounds.Length; i++)
        {
            if (backgrounds[i] == _data.SelectedBackground)
            {
                _backgroundSelector.Select(i);
                break;
            }
        }
        
        // Appearance
        _hairStyleSelector.Select(_data.HairStyle);
        _skinColorSelector.Select(_data.SkinColor);
        _eyeColorSelector.Select(_data.EyeColor);
        
        UpdateClassInfo();
        UpdateBackgroundInfo();
        UpdateAttributeLabels();
        UpdateStatsPreview();
    }
    
    private void UpdateClassInfo()
    {
        var classData = _system.GetClassData();
        _classNameLabel.Text = "⚔️ " + classData["name"];
        _classDescriptionLabel.Text = classData["description"].ToString();
        
        int hp = (int)classData["base_hp"];
        int atk = (int)classData["base_attack"];
        int def = (int)classData["base_defense"];
        int mag = (int)classData["base_magic"];
        int spd = (int)classData["base_speed"];
        
        _classStatsLabel.Text = $"HP: {hp} | ATK: {atk} | DEF: {def} | MAG: {mag} | SPD: {spd}";
    }
    
    private void UpdateBackgroundInfo()
    {
        var bgData = _system.GetBackgroundData();
        _backgroundNameLabel.Text = "📖 " + bgData["name"];
        _backgroundDescriptionLabel.Text = bgData["description"].ToString();
        
        // Parse bonuses
        var bonuses = bgData["bonuses"] as Dictionary<string, int>;
        string bonusText = "Bonuses: ";
        foreach (var b in bonuses)
        {
            bonusText += $"{b.Key} +{b.Value} ";
        }
        _backgroundBonusesLabel.Text = bonusText;
    }
    
    private void UpdateAttributeLabels()
    {
        _strengthLabel.Text = $"Strength: {_data.Strength}";
        _agilityLabel.Text = $"Agility: {_data.Agility}";
        _intelligenceLabel.Text = $"Intelligence: {_data.Intelligence}";
        _vitalityLabel.Text = $"Vitality: {_data.Vitality}";
        _luckLabel.Text = $"Luck: {_data.Luck}";
        
        int available = _system.GetAvailablePoints();
        _pointsLabel.Text = $"Available Points: {available}";
        
        if (available <= 0)
        {
            _pointsLabel.Set("custom_colors/font_color", new Color(0.8f, 0.2f, 0.2f));
        }
        else
        {
            _pointsLabel.Set("custom_colors/font_color", new Color(0.2f, 0.8f, 0.2f));
        }
    }
    
    private void UpdateStatsPreview()
    {
        var classData = _system.GetClassData();
        
        int hp = (int)classData["base_hp"] + (_data.Vitality - 10) * 10;
        int attack = (int)classData["base_attack"] + (_data.Strength - 10) * 2;
        int defense = (int)classData["base_defense"] + (_data.Vitality - 10) * 1 + (_data.Agility - 10) * 1;
        int magic = (int)classData["base_magic"] + (_data.Intelligence - 10) * 2;
        int speed = (int)classData["base_speed"] + (_data.Agility - 10) * 1 + (_data.Luck - 10) / 2;
        
        _statsLabel.Text = $"📈 Final Stats Preview:\nHP: {hp} | ATK: {attack} | DEF: {defense} | MAG: {magic} | SPD: {speed}";
    }
    
    // Signal handlers
    private void OnNameChanged(string text)
    {
        _system.SetCharacterName(text);
    }
    
    private void OnClassSelected(int index)
    {
        var classes = _system.GetAvailableClasses();
        if (index >= 0 && index < classes.Length)
        {
            _system.SetCharacterClass(classes[index]);
            UpdateUI();
        }
    }
    
    private void OnBackgroundSelected(int index)
    {
        var backgrounds = _system.GetAvailableBackgrounds();
        if (index >= 0 && index < backgrounds.Length)
        {
            _system.SetBackground(backgrounds[index]);
            UpdateUI();
        }
    }
    
    private void OnHairStyleSelected(int index)
    {
        _system.SetHairStyle(index);
    }
    
    private void OnSkinColorSelected(int index)
    {
        _system.SetSkinColor(index);
    }
    
    private void OnEyeColorSelected(int index)
    {
        _system.SetEyeColor(index);
    }
    
    private void OnMinusPressed(string attribute)
    {
        int current = _system.GetAttribute(attribute);
        if (current > 5) // Minimum 5
        {
            _system.SetAttribute(attribute, current - 1);
            UpdateUI();
        }
    }
    
    private void OnPlusPressed(string attribute)
    {
        int current = _system.GetAttribute(attribute);
        if (_system.GetAvailablePoints() > 0)
        {
            _system.SetAttribute(attribute, current + 1);
            UpdateUI();
        }
    }
    
    private void OnAttributeChanged(string attribute, int value)
    {
        UpdateAttributeLabels();
        UpdateStatsPreview();
    }
    
    private void OnClassChanged(string characterClass)
    {
        UpdateClassInfo();
        UpdateStatsPreview();
    }
    
    private void OnBackgroundChanged(string background)
    {
        UpdateBackgroundInfo();
    }
    
    private void OnCreatePressed()
    {
        if (_system.CanCreateCharacter())
        {
            var character = _system.CreateCharacter();
            if (character != null)
            {
                GD.Print("[CharacterCreationUI] Character created successfully!");
                Toggle();
            }
        }
        else
        {
            GD.Print("[CharacterCreationUI] Cannot create character - requirements not met");
        }
    }
    
    private void OnResetPressed()
    {
        _system.ResetCharacter();
        UpdateUI();
    }
    
    public void Toggle()
    {
        _isVisible = !_isVisible;
        var mainPanel = GetNode<Control>("MainPanel");
        if (mainPanel != null)
        {
            mainPanel.Visible = _isVisible;
        }
        
        if (_isVisible)
        {
            UpdateUI();
        }
    }
    
    public override void _Input(InputEvent ev)
    {
        if (ev.IsActionPressed("ui_cancel"))
        {
            Toggle();
        }
    }
}
