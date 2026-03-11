using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Mounts;

public partial class MountEvolutionUI : Control {
    private PanelContainer _mainPanel;
    private VBoxContainer _contentBox;
    private Label _titleLabel;
    
    // 坐骑列表
    private OptionButton _mountSelect;
    private Label _mountInfoLabel;
    
    // 进化信息
    private Label _currentStageLabel;
    private Label _currentTypeLabel;
    private Label _battleExpLabel;
    private ProgressBar _evolutionProgress;
    private Label _progressLabel;
    
    // 属性加成
    private VBoxContainer _bonusesContainer;
    
    // 进化按钮
    private Button _evolveButton;
    private OptionButton _typeSelect;
    private Label _costLabel;
    
    // 统计
    private Label _statsLabel;
    
    // 关闭按钮
    private Button _closeButton;
    
    private bool _isVisible = false;
    
    public override void _Ready() {
        SetupUI();
        ConnectSignals();
        Visible = false;
        GD.Print("[MountEvolutionUI] Initialized");
    }
    
    private void SetupUI() {
        // 主面板
        _mainPanel = new PanelContainer();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(600, 500);
        AddChild(_mainPanel);
        
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        styleBox.BorderWidthLeft = 2f;
        styleBox.BorderWidthRight = 2f;
        styleBox.BorderWidthTop = 2f;
        styleBox.BorderWidthBottom = 2f;
        styleBox.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        styleBox.CornerRadiusTopLeft = 8f;
        styleBox.CornerRadiusTopRight = 8f;
        styleBox.CornerRadiusBottomLeft = 8f;
        styleBox.CornerRadiusBottomRight = 8f;
        _mainPanel.AddThemeStyleboxOverride("panel", styleBox);
        
        // 内容容器
        _contentBox = new VBoxContainer();
        _contentBox.SetThemeConstant("separation", 10);
        _mainPanel.AddChild(_contentBox);
        
        // 标题
        _titleLabel = new Label();
        _titleLabel.Text = "🐎 坐骑进化系统";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _contentBox.AddChild(_titleLabel);
        
        // 坐骑选择
        var mountLabel = new Label();
        mountLabel.Text = "选择坐骑:";
        _contentBox.AddChild(mountLabel);
        
        _mountSelect = new OptionButton();
        _mountSelect.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _contentBox.AddChild(_mountSelect);
        
        // 坐骑信息
        _mountInfoLabel = new Label();
        _mountInfoLabel.Text = "请选择坐骑";
        _mountInfoLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _contentBox.AddChild(_mountInfoLabel);
        
        // 分隔线
        _contentBox.AddChild(CreateSeparator());
        
        // 当前进化阶段
        var stageContainer = new HBoxContainer();
        _contentBox.AddChild(stageContainer);
        
        _currentStageLabel = new Label();
        _currentStageLabel.Text = "当前阶段: 基础";
        _currentStageLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        stageContainer.AddChild(_currentStageLabel);
        
        _currentTypeLabel = new Label();
        _currentTypeLabel.Text = "元素类型: 自然";
        _currentTypeLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        stageContainer.AddChild(_currentTypeLabel);
        
        // 战斗经验
        _battleExpLabel = new Label();
        _battleExpLabel.Text = "战斗经验: 0";
        _contentBox.AddChild(_battleExpLabel);
        
        // 进化进度条
        var progressContainer = new VBoxContainer();
        _contentBox.AddChild(progressContainer);
        
        _progressLabel = new Label();
        _progressLabel.Text = "进化进度: 0%";
        _progressLabel.HorizontalAlignment = HorizontalAlignment.Center;
        progressContainer.AddChild(_progressLabel);
        
        _evolutionProgress = new ProgressBar();
        _evolutionProgress.CustomMinimumSize = new Vector2(0, 20);
        _evolutionProgress.ShowPercentage = false;
        progressContainer.AddChild(_evolutionProgress);
        
        // 属性加成
        var bonusTitle = new Label();
        bonusTitle.Text = "属性加成:";
        bonusTitle.AddThemeFontSizeOverride("font_size", 16);
        _contentBox.AddChild(bonusTitle);
        
        _bonusesContainer = new VBoxContainer();
        _contentBox.AddChild(_bonusesContainer);
        UpdateBonusDisplay();
        
        // 分隔线
        _contentBox.AddChild(CreateSeparator());
        
        // 进化选项
        var evolveContainer = new VBoxContainer();
        _contentBox.AddChild(evolveContainer);
        
        var typeLabel = new Label();
        typeLabel.Text = "选择进化类型:";
        evolveContainer.AddChild(typeLabel);
        
        _typeSelect = new OptionButton();
        _typeSelect.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        
        // 添加进化类型选项
        var types = new[] { "火焰", "冰霜", "闪电", "黑暗", "神圣", "自然" };
        for (int i = 0; i < types.Length; i++) {
            _typeSelect.AddItem(types[i], i);
        }
        evolveContainer.AddChild(_typeSelect);
        
        _costLabel = new Label();
        _costLabel.Text = "消耗: -";
        _costLabel.HorizontalAlignment = HorizontalAlignment.Center;
        evolveContainer.AddChild(_costLabel);
        
        _evolveButton = new Button();
        _evolveButton.Text = "🎯 进化坐骑";
        _evolveButton.CustomMinimumSize = new Vector2(0, 40);
        evolveContainer.AddChild(_evolveButton);
        
        // 分隔线
        _contentBox.AddChild(CreateSeparator());
        
        // 统计信息
        _statsLabel = new Label();
        _statsLabel.Text = "总进化次数: 0 | 总战斗经验: 0";
        _statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _contentBox.AddChild(_statsLabel);
        
        // 关闭按钮
        _closeButton = new Button();
        _closeButton.Text = "✕ 关闭";
        _closeButton.CustomMinimumSize = new Vector2(0, 35);
        _contentBox.AddChild(_closeButton);
        
        // 居中面板
        _mainPanel.Position = (GetViewportRect().Size - _mainPanel.CustomMinimumSize) / 2;
    }
    
    private HSeparator CreateSeparator() {
        var separator = new HSeparator();
        separator.AddThemeConstantOverride("separation", 10);
        return separator;
    }
    
    private void ConnectSignals() {
        _mountSelect.ItemSelected += OnMountSelected;
        _typeSelect.ItemSelected += OnTypeSelected;
        _evolveButton.Pressed += OnEvolvePressed;
        _closeButton.Pressed += OnClosePressed;
    }
    
    public override void _Process(float delta) {
        if (!Visible) return;
        
        UpdateMountList();
        UpdateEvolutionInfo();
    }
    
    private void UpdateMountList() {
        if (MountManager.Instance == null) return;
        
        var previousCount = _mountSelect.ItemCount;
        var mounts = MountManager.Instance.GetOwnedMounts();
        
        if (mounts.Count != previousCount - 1) { // -1 for "选择坐骑" item
            _mountSelect.Clear();
            _mountSelect.AddItem("选择坐骑", 0);
            
            int index = 1;
            foreach (var mount in mounts) {
                _mountSelect.AddItem($"{mount.Value.Name} (Lv.{mount.Value.Level})", index++);
            }
        }
    }
    
    private void UpdateEvolutionInfo() {
        if (MountManager.Instance == null || MountEvolutionSystem.Instance == null) return;
        
        var selectedIndex = _mountSelect.Selected;
        if (selectedIndex <= 0) return;
        
        var mounts = MountManager.Instance.GetOwnedMounts();
        var mountList = new List<string>(mounts.Keys);
        
        if (selectedIndex - 1 >= mountList.Count) return;
        
        var mountId = mountList[selectedIndex - 1];
        var evolution = MountEvolutionSystem.Instance.GetMountEvolution(mountId);
        
        if (evolution == null) {
            // 初始化进化数据
            // 默认使用第一个进化链
            MountEvolutionSystem.Instance.InitializeMountEvolution(mountId, MountEvolutionChain.Horse);
            evolution = MountEvolutionSystem.Instance.GetMountEvolution(mountId);
        }
        
        if (evolution != null) {
            // 更新阶段和类型显示
            _currentStageLabel.Text = $"当前阶段: {MountEvolutionSystem.Instance.GetStageName(evolution.CurrentStage)}";
            _currentTypeLabel.Text = $"元素类型: {MountEvolutionSystem.Instance.GetTypeName(evolution.CurrentType)}";
            
            // 更新战斗经验
            _battleExpLabel.Text = $"战斗经验: {evolution.BattleExp}";
            
            // 更新进度条
            var progress = MountEvolutionSystem.Instance.GetEvolutionProgress(mountId);
            _evolutionProgress.Value = progress * 100;
            _progressLabel.Text = $"进化进度: {progress * 100:F1}%";
            
            // 更新属性加成显示
            UpdateBonusDisplay();
            
            // 更新进化按钮状态
            var canEvolve = MountEvolutionSystem.Instance.CanEvolve(mountId);
            _evolveButton.Disabled = !canEvolve;
            
            if (!canEvolve && evolution.CurrentStage == MountEvolutionStage.Legendary) {
                _costLabel.Text = "已达到最高阶段!";
            } else if (!canEvolve) {
                var nextStage = MountEvolutionDatabase.GetNextStage(evolution.CurrentStage);
                var nextConfig = MountEvolutionDatabase.GetStageConfig(nextStage);
                _costLabel.Text = $"还需要 {nextConfig.RequiredExp - evolution.BattleExp} 经验";
            } else {
                var nextStage = MountEvolutionDatabase.GetNextStage(evolution.CurrentStage);
                var goldCost = MountEvolutionDatabase.GetEvolutionGoldCost(nextStage);
                var materialName = MountEvolutionDatabase.GetEvolutionMaterialName(nextStage);
                var materialCount = MountEvolutionDatabase.GetStageConfig(nextStage).RequiredItems;
                _costLabel.Text = $"消耗: {goldCost}金币 + {materialCount}个{materialName}";
            }
        }
        
        // 更新统计
        var stats = MountEvolutionSystem.Instance.GetStatistics();
        _statsLabel.Text = $"总进化次数: {stats.TotalEvolutions} | 总战斗经验: {stats.TotalBattleExp}";
    }
    
    private void UpdateBonusDisplay() {
        // 清空现有显示
        foreach (var child in _bonusesContainer.GetChildren()) {
            child.QueueFree();
        }
        
        var selectedIndex = _mountSelect.Selected;
        if (selectedIndex <= 0) return;
        
        var mounts = MountManager.Instance.GetOwnedMounts();
        var mountList = new List<string>(mounts.Keys);
        
        if (selectedIndex - 1 >= mountList.Count) return;
        
        var mountId = mountList[selectedIndex - 1];
        var bonuses = MountEvolutionSystem.Instance.GetMountEvolutionBonuses(mountId);
        
        foreach (var kvp in bonuses) {
            if (kvp.Value > 0) {
                var bonusLabel = new Label();
                var bonusName = kvp.Key.Replace("Bonus", "");
                bonusLabel.Text = $"  {bonusName}: +{kvp.Value:F1}%";
                _bonusesContainer.AddChild(bonusLabel);
            }
        }
        
        if (_bonusesContainer.GetChildCount() == 0) {
            var noBonusLabel = new Label();
            noBonusLabel.Text = "  无属性加成";
            _bonusesContainer.AddChild(noBonusLabel);
        }
    }
    
    private void OnMountSelected(int index) {
        UpdateEvolutionInfo();
    }
    
    private void OnTypeSelected(int index) {
        // 类型选择变化
    }
    
    private void OnEvolvePressed() {
        var selectedIndex = _mountSelect.Selected;
        if (selectedIndex <= 0) {
            ShowMessage("请先选择坐骑!");
            return;
        }
        
        var mounts = MountManager.Instance.GetOwnedMounts();
        var mountList = new List<string>(mounts.Keys);
        
        if (selectedIndex - 1 >= mountList.Count) return;
        
        var mountId = mountList[selectedIndex - 1];
        var targetType = (MountEvolutionType)_typeSelect.Selected;
        
        var result = MountEvolutionSystem.Instance.TryEvolveMount(mountId, targetType);
        
        switch (result) {
            case EvolutionResult.Success:
                ShowMessage("🎉 进化成功!");
                break;
            case EvolutionResult.MaxStage:
                ShowMessage("已达到最高进化阶段!");
                break;
            case EvolutionResult.InsufficientExp:
                ShowMessage("经验不足!");
                break;
            case EvolutionResult.InsufficientItems:
                ShowMessage("材料不足!");
                break;
            default:
                ShowMessage("进化失败!");
                break;
        }
        
        UpdateEvolutionInfo();
    }
    
    private void OnClosePressed() {
        ToggleUI();
    }
    
    public void ToggleUI() {
        _isVisible = !_isVisible;
        Visible = _isVisible;
        
        if (_isVisible) {
            UpdateMountList();
            UpdateEvolutionInfo();
        }
    }
    
    private void ShowMessage(string message) {
        // 简单的消息显示
        GD.Print($"[MountEvolutionUI] {message}");
    }
    
    public override void _Input(InputEvent eventEvent) {
        if (eventEvent.IsActionPressed("ui_cancel") && Visible) {
            ToggleUI();
        }
    }
}
