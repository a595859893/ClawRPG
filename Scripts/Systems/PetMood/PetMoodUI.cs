using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.PetMood;

public partial class PetMoodUI : Control {
    private PetMoodSystem _moodSystem;
    private Control _mainPanel;
    private Label _titleLabel;
    private VBoxContainer _petListContainer;
    private PetMoodType _selectedFilter = PetMoodType.Neutral;
    private string _selectedPetId = "";
    
    // 快捷键
    private bool _isVisible = false;
    
    public override void _Ready() {
        _moodSystem = GetNode<PetMoodSystem>("/root/PetMoodSystem");
        if (_moodSystem == null) {
            GD.PrintErr("PetMoodSystem not found!");
            return;
        }
        
        SetupUI();
        ConnectSignals();
        
        // 初始隐藏
        Visible = false;
    }
    
    private void SetupUI() {
        // 主面板
        _mainPanel = new PanelContainer();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(600, 500);
        AddChild(_mainPanel);
        
        var mainVBox = new VBoxContainer();
        mainVBox.SetanchorsAndOffsetsPreset(Control.LayoutPreset.Center);
        _mainPanel.AddChild(mainVBox);
        
        // 标题栏
        var titleBar = new HBoxContainer();
        mainVBox.AddChild(titleBar);
        
        _titleLabel = new Label();
        _titleLabel.Text = "🐾 宠物心情系统";
        _titleLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        titleBar.AddChild(_titleLabel);
        
        var closeButton = new Button();
        closeButton.Text = "✕";
        closeButton.Pressed += () => ToggleUI();
        titleBar.AddChild(closeButton);
        
        // 筛选栏
        var filterBar = new HBoxContainer();
        mainVBox.AddChild(filterBar);
        
        var filterLabel = new Label();
        filterLabel.Text = "心情筛选: ";
        filterBar.AddChild(filterLabel);
        
        var moodTypes = Enum.GetValues(typeof(PetMoodType));
        foreach (PetMoodType mood in moodTypes) {
            var btn = new Button();
            btn.Text = PetMoodDatabase.Instance.MoodEmojis[mood];
            btn.TooltipText = mood.ToString();
            btn.Pressed += () => OnFilterPressed(mood);
            filterBar.AddChild(btn);
        }
        
        // 全部按钮
        var allBtn = new Button();
        allBtn.Text = "全部";
        allBtn.Pressed += () => OnFilterPressed(PetMoodType.Neutral);
        filterBar.AddChild(allBtn);
        
        // 内容区域
        var contentHBox = new HBoxContainer();
        contentHBox.SizeFlagsVertical = SizeFlags.ExpandFill;
        mainVBox.AddChild(contentHBox);
        
        // 左侧 - 宠物列表
        var leftPanel = new VBoxContainer();
        leftPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        contentHBox.AddChild(leftPanel);
        
        var listLabel = new Label();
        listLabel.Text = "宠物列表";
        leftPanel.AddChild(listLabel);
        
        var scrollContainer = new ScrollContainer();
        scrollContainer.SizeFlagsVertical = SizeFlags.ExpandFill;
        leftPanel.AddChild(scrollContainer);
        
        _petListContainer = new VBoxContainer();
        scrollContainer.AddChild(_petListContainer);
        
        // 右侧 - 详情面板
        var rightPanel = new VBoxContainer();
        rightPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        contentHBox.AddChild(rightPanel);
        
        var detailLabel = new Label();
        detailLabel.Text = "心情详情";
        rightPanel.AddChild(detailLabel);
        
        var detailScroll = new ScrollContainer();
        detailScroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        rightPanel.AddChild(detailScroll);
        
        var detailContainer = new VBoxContainer();
        detailScroll.AddChild(detailContainer);
        
        // 操作按钮
        var actionBar = new HBoxContainer();
        mainVBox.AddChild(actionBar);
        
        var interactBtn = new Button();
        interactBtn.Text = "互动";
        interactBtn.Pressed += () => OnInteractPressed("pet");
        actionBar.AddChild(interactBtn);
        
        var feedBtn = new Button();
        feedBtn.Text = "喂食";
        feedBtn.Pressed += () => _moodSystem?.FeedPet(_selectedPetId);
        actionBar.AddChild(feedBtn);
        
        var playBtn = new Button();
        playBtn.Text = "玩耍";
        playBtn.Pressed += () => OnInteractPressed("play");
        actionBar.AddChild(playBtn);
        
        // 统计按钮
        var statsBtn = new Button();
        statsBtn.Text = "统计";
        statsBtn.Pressed += () => ShowStatistics();
        actionBar.AddChild(statsBtn);
        
        RefreshPetList();
    }
    
    private void ConnectSignals() {
        PetMoodSystem.PetMoodChanged += OnMoodChanged;
    }
    
    private void OnMoodChanged(string petId, PetMoodType newMood) {
        RefreshPetList();
    }
    
    private void OnFilterPressed(PetMoodType mood) {
        _selectedFilter = mood;
        RefreshPetList();
    }
    
    private void RefreshPetList() {
        // 清空列表
        foreach (var child in _petListContainer.GetChildren()) {
            child.QueueFree();
        }
        
        // 获取所有宠物
        var petManager = GetNode<("/root/PetManager") as PetManager;
        if (petManager == null) return;
        
        var pets = petManager.GetPets();
        
        foreach (var pet in pets) {
            var petId = pet.Get("pet_id")?.ToString() ?? "";
            var petName = pet.Get("name")?.ToString() ?? "Unknown";
            
            var mood = _moodSystem?.GetPetMood(petId);
            if (mood == null) continue;
            
            // 应用筛选
            if (_selectedFilter != PetMoodType.Neutral && mood.CurrentMood != _selectedFilter) {
                continue;
            }
            
            var petButton = new Button();
            petButton.Text = $"{PetMoodDatabase.Instance.MoodEmojis[mood.CurrentMood]} {petName}";
            petButton.Pressed += () => OnPetSelected(petId, petName);
            _petListContainer.AddChild(petButton);
        }
    }
    
    private void OnPetSelected(string petId, string petName) {
        _selectedPetId = petId;
        
        var mood = _moodSystem?.GetPetMood(petId);
        if (mood == null) return;
        
        // 显示详情
        ShowPetDetails(petId, petName, mood);
    }
    
    private void ShowPetDetails(string petId, string petName, PetMood mood) {
        // 找到右侧容器并清空
        var contentHBox = _mainPanel.GetChild<VBoxContainer>(0).GetChild<HBoxContainer>(2);
        var rightPanel = contentHBox.GetChild<VBoxContainer>(1);
        
        foreach (var child in rightPanel.GetChildren()) {
            if (child is ScrollContainer scroll) {
                var container = scroll.GetChild<VBoxContainer>(0);
                foreach (var c in container.GetChildren()) {
                    c.QueueFree();
                }
                
                // 添加详情
                var nameLabel = new Label();
                nameLabel.Text = $"🐾 {petName}";
                container.AddChild(nameLabel);
                
                var moodLabel = new Label();
                var moodEmoji = PetMoodDatabase.Instance.MoodEmojis[mood.CurrentMood];
                var moodColor = PetMoodDatabase.Instance.MoodColors[mood.CurrentMood];
                moodLabel.Text = $"心情: {moodEmoji} {mood.CurrentMood}";
                moodLabel.Modulate = moodColor;
                container.AddChild(moodLabel);
                
                var intensityLabel = new Label();
                intensityLabel.Text = $"强度: {mood.Intensity}";
                container.AddChild(intensityLabel);
                
                var valueLabel = new Label();
                valueLabel.Text = $"心情值: {mood.MoodValue:F2}";
                container.AddChild(valueLabel);
                
                // 效果
                var effects = PetMoodDatabase.Instance.GetEffectsForMood(mood);
                if (effects.Count > 0) {
                    var effectsLabel = new Label();
                    effectsLabel.Text = "当前效果:";
                    container.AddChild(effectsLabel);
                    
                    foreach (var effect in effects) {
                        var effectLabel = new Label();
                        effectLabel.Text = $"  • {effect.Description}";
                        container.AddChild(effectLabel);
                    }
                }
            }
        }
    }
    
    private void OnInteractPressed(string interactionType) {
        if (string.IsNullOrEmpty(_selectedPetId)) {
            GD.Print("请先选择宠物");
            return;
        }
        _moodSystem?.InteractWithPet(_selectedPetId, interactionType);
    }
    
    private void ShowStatistics() {
        var stats = _moodSystem?.GetMoodStatistics();
        var total = _moodSystem?.GetTotalInteractionCount() ?? 0;
        
        GD.Print("=== 宠物心情统计 ===");
        GD.Print($"总互动次数: {total}");
        
        if (stats != null) {
            foreach (var kvp in stats) {
                GD.Print($"{kvp.Key}: {kvp.Value}次");
            }
        }
    }
    
    public void ToggleUI() {
        _isVisible = !_isVisible;
        Visible = _isVisible;
        
        if (_isVisible) {
            RefreshPetList();
        }
    }
    
    public override void _Input(InputEvent @event) {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
            if (keyEvent.Keycode == Key.M) {
                ToggleUI();
            } else if (keyEvent.Keycode == Key.Escape && _isVisible) {
                ToggleUI();
            }
        }
    }
}
