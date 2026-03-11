using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Crafting;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.UI;

/// <summary>
/// 钓鱼用户界面
/// </summary>
public class FishingUI : Control
{
    private Control _mainPanel;
    private Label _titleLabel;
    private Label _skillLabel;
    private ProgressBar _expProgress;
    private Label _expLabel;
    private VBoxContainer _fishList;
    private Label _statusLabel;
    private Button _startButton;
    private Button _reelButton;
    private Button _cancelButton;
    
    // 钓鱼状态显示
    private Label _stateLabel;
    private Label _rodLabel;
    private Label _durabilityLabel;
    private Label _timerLabel;
    
    // 钓鱼统计
    private Label _statsLabel;
    
    private bool _isVisible;
    
    public override void _Ready()
    {
        SetupUI();
        ConnectSignals();
        Hide();
    }
    
    private void SetupUI()
    {
        // 主面板
        _mainPanel = new Panel
        {
            AnchorRight = 0.4f,
            AnchorBottom = 0.8f,
            OffsetLeft = 50,
            OffsetTop = 50,
            OffsetRight = -50,
            OffsetBottom = -50
        };
        AddChild(_mainPanel);
        
        var mainStyle = new StyleBoxFlat();
        mainStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        mainStyle.BorderWidthLeft = 2;
        mainStyle.BorderWidthTop = 2;
        mainStyle.BorderWidthRight = 2;
        mainStyle.BorderWidthBottom = 2;
        mainStyle.BorderColor = new Color(0.3f, 0.6f, 0.9f);
        mainStyle.CornerRadiusTopLeft = 10;
        mainStyle.CornerRadiusTopRight = 10;
        mainStyle.CornerRadiusBottomLeft = 10;
        mainStyle.CornerRadiusBottomRight = 10;
        _mainPanel.AddThemeStyleboxOverride("panel", mainStyle);
        
        var vbox = new VBoxContainer
        {
            OffsetLeft = 15,
            OffsetTop = 15,
            OffsetRight = -15,
            OffsetBottom = -15
        };
        _mainPanel.AddChild(vbox);
        
        // 标题
        _titleLabel = new Label
        {
            Text = "🎣 钓鱼系统",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        vbox.AddChild(_titleLabel);
        
        vbox.AddChild(new HSeparator());
        
        // 技能信息
        var skillBox = new HBoxContainer();
        vbox.AddChild(skillBox);
        
        _skillLabel = new Label
        {
            Text = "等级: 1",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        _skillLabel.AddThemeFontSizeOverride("font_size", 18);
        skillBox.AddChild(_skillLabel);
        
        // 经验条
        _expProgress = new ProgressBar
        {
            MinValue = 0,
            MaxValue = 100,
            Value = 0,
            CustomMinimumSize = new Vector2(0, 20)
        };
        vbox.AddChild(_expProgress);
        
        _expLabel = new Label
        {
            Text = "经验: 0 / 100",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        vbox.AddChild(_expLabel);
        
        vbox.AddChild(new HSeparator());
        
        // 当前鱼竿信息
        var rodBox = new HBoxContainer();
        vbox.AddChild(rodBox);
        
        _rodLabel = new Label
        {
            Text = "鱼竿: 木质鱼竿",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        rodBox.AddChild(_rodLabel);
        
        _durabilityLabel = new Label
        {
            Text = "耐久度: 100/100",
            HorizontalAlignment = HorizontalAlignment.Right
        };
        rodBox.AddChild(_durabilityLabel);
        
        vbox.AddChild(new HSeparator());
        
        // 钓鱼状态
        _stateLabel = new Label
        {
            Text = "状态: 空闲",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _stateLabel.AddThemeFontSizeOverride("font_size", 16);
        vbox.AddChild(_stateLabel);
        
        _timerLabel = new Label
        {
            Text = "",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        vbox.AddChild(_timerLabel);
        
        // 按钮区域
        var buttonBox = new HBoxContainer();
        vbox.AddChild(buttonBox);
        
        _startButton = new Button
        {
            Text = "开始钓鱼",
            CustomMinimumSize = new Vector2(120, 40)
        };
        _startButton.Pressed += OnStartPressed;
        buttonBox.AddChild(_startButton);
        
        _reelButton = new Button
        {
            Text = "提竿!",
            CustomMinimumSize = new Vector2(120, 40),
            Disabled = true
        };
        _reelButton.Pressed += OnReelPressed;
        buttonBox.AddChild(_reelButton);
        
        _cancelButton = new Button
        {
            Text = "取消",
            CustomMinimumSize = new Vector2(100, 40)
        };
        _cancelButton.Pressed += OnCancelPressed;
        buttonBox.AddChild(_cancelButton);
        
        vbox.AddChild(new HSeparator());
        
        // 可捕获的鱼类预览
        var fishTitle = new Label
        {
            Text = "可捕获的鱼类:",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        vbox.AddChild(fishTitle);
        
        _fishList = new VBoxContainer();
        _fishList.CustomMinimumSize = new Vector2(0, 150);
        vbox.AddChild(_fishList);
        
        // 刷新鱼类列表
        RefreshFishList();
        
        vbox.AddChild(new HSeparator());
        
        // 统计信息
        _statsLabel = new Label
        {
            Text = "钓鱼统计",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        vbox.AddChild(_statsLabel);
        
        // 关闭按钮
        var closeButton = new Button
        {
            Text = "关闭 (ESC)",
            CustomMinimumSize = new Vector2(0, 35)
        };
        closeButton.Pressed += OnClosePressed;
        vbox.AddChild(closeButton);
    }
    
    private void ConnectSignals()
    {
        if (FishingSystem.Instance == null) return;
        
        FishingSystem.Instance.FishingStateChanged += OnStateChanged;
        FishingSystem.Instance.FishCaught += OnFishCaught;
        FishingSystem.Instance.FishMissed += OnFishMissed;
        FishingSystem.Instance.LevelUp += OnLevelUp;
    }
    
    private void RefreshFishList()
    {
        // 清除旧列表
        foreach (var child in _fishList.GetChildren())
        {
            child.QueueFree();
        }
        
        var skill = FishingSystem.Instance.PlayerSkill;
        var availableFish = FishingDatabase.Instance.GetAvailableFish(skill.Level, skill.LuckBonus * 10);
        
        foreach (var fish in availableFish)
        {
            var label = new Label
            {
                Text = $"• {fish.Name} ({GetRarityColor(fish.Rarity)}{fish.Rarity}{GetColorEnd()}) - {fish.ExperienceReward}经验"
            };
            _fishList.AddChild(label);
        }
    }
    
    private string GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => "[color=#ffffff]",
            ItemRarity.Uncommon => "[color=#00ff00]",
            ItemRarity.Rare => "[color=#0080ff]",
            ItemRarity.Epic => "[color=#8000ff]",
            ItemRarity.Legendary => "[color=#ff8000]",
            _ => "[color=#ffffff]"
        };
    }
    
    private string GetColorEnd() => "[/color]";
    
    public override void _Process(float delta)
    {
        if (!_isVisible) return;
        
        UpdateSkillInfo();
        UpdateFishingUI();
    }
    
    private void UpdateSkillInfo()
    {
        var skill = FishingSystem.Instance.PlayerSkill;
        _skillLabel.Text = $"等级: {skill.Level}";
        _expProgress.MaxValue = skill.ExperienceToNextLevel;
        _expProgress.Value = skill.Experience;
        _expLabel.Text = $"经验: {skill.Experience} / {skill.ExperienceToNextLevel}";
        
        var rod = FishingDatabase.Instance.GetFishingRod(FishingSystem.Instance.CurrentRodId);
        _rodLabel.Text = $"鱼竿: {rod?.Name ?? "未知"}";
        _durabilityLabel.Text = $"耐久度: {FishingSystem.Instance.CurrentRodDurability}/{rod?.Durability ?? 100}";
    }
    
    private void UpdateFishingUI()
    {
        var state = FishingSystem.Instance.State;
        
        switch (state)
        {
            case FishingState.Idle:
                _startButton.Disabled = false;
                _reelButton.Disabled = true;
                _cancelButton.Disabled = true;
                _timerLabel.Text = "";
                break;
                
            case FishingState.Casting:
                _startButton.Disabled = true;
                _reelButton.Disabled = true;
                _cancelButton.Disabled = false;
                _timerLabel.Text = "抛竿中...";
                break;
                
            case FishingState.Waiting:
                _startButton.Disabled = true;
                _reelButton.Disabled = true;
                _cancelButton.Disabled = false;
                _timerLabel.Text = "等待鱼咬钩...";
                break;
                
            case FishingState.Biting:
                _startButton.Disabled = true;
                _reelButton.Disabled = false;
                _cancelButton.Disabled = false;
                _timerLabel.Text = "鱼咬钩了！快提竿！";
                _timerLabel.Modulate = new Color(1, 0.3f, 0.3f);
                break;
                
            case FishingState.Reeling:
                _startButton.Disabled = true;
                _reelButton.Disabled = true;
                _cancelButton.Disabled = true;
                _timerLabel.Text = "收线中...";
                break;
        }
    }
    
    private void OnStateChanged(FishingState newState)
    {
        _stateLabel.Text = $"状态: {newState}";
    }
    
    private void OnFishCaught(FishingData fish, int quantity)
    {
        _statusLabel.Text = $"钓到了 {fish.Name} x{quantity}!";
    }
    
    private void OnFishMissed()
    {
        _statusLabel.Text = "鱼跑掉了...";
    }
    
    private void OnLevelUp(int newLevel)
    {
        RefreshFishList();
    }
    
    private void OnStartPressed()
    {
        var player = GetTree().GetFirstNodeInGroup("player");
        if (player != null)
        {
            FishingSystem.Instance.StartFishing(player.GlobalPosition);
        }
    }
    
    private void OnReelPressed()
    {
        FishingSystem.Instance.Reel();
    }
    
    private void OnCancelPressed()
    {
        FishingSystem.Instance.CancelFishing();
    }
    
    private void OnClosePressed()
    {
        HideFishingUI();
    }
    
    public void ShowFishingUI()
    {
        Show();
        _isVisible = true;
        UpdateSkillInfo();
    }
    
    public void HideFishingUI()
    {
        Hide();
        _isVisible = false;
    }
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
        {
            if (_isVisible)
            {
                HideFishingUI();
            }
        }
    }
}
