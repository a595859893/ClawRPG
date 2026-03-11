using Godot;
using System;
using System.Collections.Generic;

public class PetMorphUI : Control
{
    private Control _mainContainer;
    private VBoxContainer _morphListContainer;
    private Label _titleLabel;
    private Label _statsLabel;
    private PetMorphSystem _morphSystem;
    private string _selectedPetId = "";
    private List<PetMorph> _currentMorphs = new List<PetMorph>();
    
    // 资源预加载
    private PackedScene _morphButtonScene;
    
    public override void _Ready()
    {
        _morphSystem = PetMorphSystem.Instance;
        _morphSystem.Initialize();
        
        SetupUI();
        SetupSignals();
        
        // 默认隐藏
        Visible = false;
        
        GD.Print("[PetMorphUI] Ready");
    }
    
    private void SetupUI()
    {
        // 主容器
        _mainContainer = new Control();
        _mainContainer.SetAnchorsPreset(Control.AnchorsPreset.Center);
        _mainContainer.CustomMinimumSize = new Vector2(600, 500);
        AddChild(_mainContainer);
        
        // 背景面板
        Panel background = new Panel();
        background.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
        background.Modulate = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        _mainContainer.AddChild(background);
        
        // 标题
        _titleLabel = new Label();
        _titleLabel.Text = "宠物形态系统";
        _titleLabel.SetAnchorsPreset(Control.AnchorsPreset.TopWide);
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.Position = new Vector2(0, 10);
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _mainContainer.AddChild(_titleLabel);
        
        // 关闭按钮
        Button closeBtn = new Button();
        closeBtn.Text = "X";
        closeBtn.Position = new Vector2(560, 10);
        closeBtn.Size = new Vector2(30, 30);
        closeBtn.Pressed += () => HideUI();
        _mainContainer.AddChild(closeBtn);
        
        // 宠物选择
        Label petLabel = new Label();
        petLabel.Text = "选择宠物:";
        petLabel.Position = new Vector2(20, 60);
        _mainContainer.AddChild(petLabel);
        
        // 宠物列表容器
        ScrollContainer petScroll = new ScrollContainer();
        petScroll.Position = new Vector2(20, 85);
        petScroll.Size = new Vector2(200, 60);
        _mainContainer.AddChild(petScroll);
        
        HBoxContainer petList = new HBoxContainer();
        petScroll.AddChild(petList);
        
        // 添加宠物选择按钮
        UpdatePetList(petList);
        
        // 形态列表标题
        Label morphTitle = new Label();
        morphTitle.Text = "可用形态:";
        morphTitle.Position = new Vector2(20, 160);
        _mainContainer.AddChild(morphTitle);
        
        // 形态列表
        ScrollContainer morphScroll = new ScrollContainer();
        morphScroll.Position = new Vector2(20, 185);
        morphScroll.Size = new Vector2(350, 250);
        _mainContainer.AddChild(morphScroll);
        
        _morphListContainer = new VBoxContainer();
        _morphListContainer.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
        morphScroll.AddChild(_morphListContainer);
        
        // 统计面板
        Panel statsPanel = new Panel();
        statsPanel.Position = new Vector2(390, 185);
        statsPanel.Size = new Vector2(180, 250);
        statsPanel.Modulate = new Color(0.15f, 0.15f, 0.2f, 0.9f);
        _mainContainer.AddChild(statsPanel);
        
        _statsLabel = new Label();
        _statsLabel.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
        _statsLabel.Position = new Vector2(10, 10);
        _statsLabel.AddThemeFontSizeOverride("font_size", 16);
        _statsLabel.Text = "统计信息\n\n---\n\n";
        statsPanel.AddChild(_statsLabel);
        
        // 说明文字
        Label helpLabel = new Label();
        helpLabel.Text = "选择宠物后，点击形态进行解锁或激活\n形态激活后宠物获得属性加成";
        helpLabel.Position = new Vector2(20, 455);
        helpLabel.AddThemeFontSizeOverride("font_size", 12);
        helpLabel.Modulate = new Color(0.7f, 0.7f, 0.7f, 1f);
        _mainContainer.AddChild(helpLabel);
    }
    
    private void UpdatePetList(HBoxContainer container)
    {
        // 清除现有按钮
        foreach (var child in container.GetChildren())
        {
            child.QueueFree();
        }
        
        if (PetManager.Instance == null) return;
        
        var pets = PetManager.Instance.GetOwnedPets();
        foreach (var pet in pets)
        {
            Button petBtn = new Button();
            petBtn.Text = pet.Name;
            petBtn.Size = new Vector2(80, 40);
            petBtn.Pressed += () => SelectPet(pet.Id);
            container.AddChild(petBtn);
        }
    }
    
    private void SelectPet(string petId)
    {
        _selectedPetId = petId;
        UpdateMorphList();
        UpdateStats();
    }
    
    private void UpdateMorphList()
    {
        // 清除现有按钮
        foreach (var child in _morphListContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        if (string.IsNullOrEmpty(_selectedPetId))
        {
            Label noPet = new Label();
            noPet.Text = "请先选择宠物";
            noPet.Modulate = new Color(0.8f, 0.8f, 0.8f, 1f);
            _morphListContainer.AddChild(noPet);
            return;
        }
        
        var morphs = _morphSystem.GetAvailableMorphsForPet(_selectedPetId);
        _currentMorphs = morphs;
        
        foreach (var morph in morphs)
        {
            CreateMorphButton(morph);
        }
    }
    
    private void CreateMorphButton(PetMorph morph)
    {
        Panel buttonPanel = new Panel();
        buttonPanel.CustomMinimumSize = new Vector2(320, 80);
        buttonPanel.Modulate = new Color(0.2f, 0.2f, 0.25f, 0.9f);
        
        // 根据稀有度设置颜色
        Color rarityColor = GetRarityColor(morph.MorphType);
        Color borderColor = rarityColor;
        
        // 标题行
        HBoxContainer titleRow = new HBoxContainer();
        titleRow.SetAnchorsPreset(Control.AnchorsPreset.TopWide);
        titleRow.Position = new Vector2(10, 5);
        buttonPanel.AddChild(titleRow);
        
        Label nameLabel = new Label();
        nameLabel.Text = morph.MorphName;
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        nameLabel.Modulate = rarityColor;
        titleRow.AddChild(nameLabel);
        
        // 状态标签
        bool isUnlocked = _morphSystem.IsMorphUnlocked(_selectedPetId, morph.MorphId);
        bool isActive = _morphSystem.GetActiveMorph(_selectedPetId) == morph.MorphId;
        
        Label statusLabel = new Label();
        if (isActive)
        {
            statusLabel.Text = "[已激活]";
            statusLabel.Modulate = new Color(0.2f, 1f, 0.2f, 1f);
        }
        else if (isUnlocked)
        {
            statusLabel.Text = "[已解锁]";
            statusLabel.Modulate = new Color(0.3f, 0.7f, 1f, 1f);
        }
        else
        {
            statusLabel.Text = $"[未解锁 {morph.UnlockCost}金]";
            statusLabel.Modulate = new Color(0.6f, 0.6f, 0.6f, 1f);
        }
        statusLabel.HorizontalAlignment = HorizontalAlignment.Right;
        titleRow.AddChild(statusLabel);
        
        // 描述
        Label descLabel = new Label();
        descLabel.Text = morph.Description;
        descLabel.Position = new Vector2(10, 30);
        descLabel.Modulate = new Color(0.7f, 0.7f, 0.7f, 1f);
        descLabel.AddThemeFontSizeOverride("font_size", 12);
        buttonPanel.AddChild(descLabel);
        
        // 属性加成
        Label statsLabel = new Label();
        string statsText = "属性: ";
        if (morph.AttackBonus > 0) statsText += $"攻击+{morph.AttackBonus} ";
        if (morph.DefenseBonus > 0) statsText += $"防御+{morph.DefenseBonus} ";
        if (morph.HealthBonus > 0) statsText += $"生命+{morph.HealthBonus} ";
        if (morph.SpeedBonus > 0) statsText += $"速度+{morph.SpeedBonus} ";
        if (morph.CritRateBonus > 0) statsText += $"暴击+{morph.CritRateBonus}% ";
        statsLabel.Text = statsText;
        statsLabel.Position = new Vector2(10, 50);
        statsLabel.Modulate = new Color(0.5f, 0.8f, 0.5f, 1f);
        statsLabel.AddThemeFontSizeOverride("font_size", 11);
        buttonPanel.AddChild(statsLabel);
        
        // 按钮
        Button actionBtn = new Button();
        actionBtn.Text = isActive ? "取消激活" : (isUnlocked ? "激活" : "解锁");
        actionBtn.Position = new Vector2(220, 45);
        actionBtn.Size = new Vector2(90, 30);
        
        if (isActive)
        {
            actionBtn.Pressed += () => DeactivateMorph(morph.MorphId);
        }
        else if (isUnlocked)
        {
            actionBtn.Pressed += () => ActivateMorph(morph.MorphId);
        }
        else
        {
            actionBtn.Pressed += () => UnlockMorph(morph.MorphId, morph.UnlockCost);
        }
        
        buttonPanel.AddChild(actionBtn);
        
        _morphListContainer.AddChild(buttonPanel);
        
        // 添加间距
        Control spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(0, 5);
        _morphListContainer.AddChild(spacer);
    }
    
    private Color GetRarityColor(PetMorphType type)
    {
        switch (type)
        {
            case PetMorphType.Normal: return new Color(0.8f, 0.8f, 0.8f);
            case PetMorphType.Battle: return new Color(1f, 0.5f, 0.3f);
            case PetMorphType.Speed: return new Color(0.3f, 0.7f, 1f);
            case PetMorphType.Tank: return new Color(0.3f, 0.5f, 0.8f);
            case PetMorphType.Magic: return new Color(0.7f, 0.4f, 0.9f);
            case PetMorphType.Elite: return new Color(1f, 0.6f, 0.2f);
            case PetMorphType.Legendary: return new Color(1f, 0.8f, 0.2f);
            case PetMorphType.Mythical: return new Color(1f, 0.5f, 0.8f);
            default: return new Color(1f, 1f, 1f);
        }
    }
    
    private void UnlockMorph(string morphId, int cost)
    {
        if (_morphSystem.UnlockMorph(_selectedPetId, morphId))
        {
            UpdateMorphList();
            UpdateStats();
        }
    }
    
    private void ActivateMorph(string morphId)
    {
        if (_morphSystem.ActivateMorph(_selectedPetId, morphId))
        {
            UpdateMorphList();
            UpdateStats();
        }
    }
    
    private void DeactivateMorph(string morphId)
    {
        if (_morphSystem.DeactivateMorph(_selectedPetId))
        {
            UpdateMorphList();
            UpdateStats();
        }
    }
    
    private void UpdateStats()
    {
        var stats = _morphSystem.GetStatistics();
        string statsText = "统计信息\n\n";
        statsText += $"总变形次数: {stats["total_transformations"]}\n";
        statsText += $"解锁形态数: {stats["unique_morphs_unlocked"]}\n";
        statsText += $"总形态时间: {stats["total_morph_time"]}秒\n";
        
        _statsLabel.Text = statsText;
    }
    
    private void SetupSignals()
    {
        _morphSystem.MorphUnlocked += (petId, morphId) => {
            if (petId == _selectedPetId)
            {
                UpdateMorphList();
                UpdateStats();
            }
        };
        
        _morphSystem.MorphActivated += (petId, morphId) => {
            if (petId == _selectedPetId)
            {
                UpdateMorphList();
                UpdateStats();
            }
        };
        
        _morphSystem.MorphDeactivated += (petId) => {
            if (petId == _selectedPetId)
            {
                UpdateMorphList();
                UpdateStats();
            }
        };
    }
    
    public void ShowUI()
    {
        Visible = true;
        UpdateMorphList();
        UpdateStats();
        
        // 显示动画
        Tween tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(_mainContainer, "modulate:a", 1f, 0.3f);
    }
    
    public void HideUI()
    {
        // 隐藏动画
        Tween tween = CreateTween();
        tween.TweenProperty(_mainContainer, "modulate:a", 0f, 0.2f);
        tween.TweenCallback(Callable.From(() => Visible = false));
    }
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            if (Visible)
            {
                HideUI();
            }
        }
        
        // M 键切换显示
        if (e is InputEventKey keyEvent2 && keyEvent2.Pressed && keyEvent2.Keycode == Key.M)
        {
            if (Visible)
            {
                HideUI();
            }
            else
            {
                ShowUI();
            }
        }
    }
}
