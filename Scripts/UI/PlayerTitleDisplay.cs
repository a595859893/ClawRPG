using Godot;
using Godot.Collections;
using System;

namespace ClawRPG.Scripts.UI;

/// <summary>
/// 玩家头顶称号显示组件
/// 显示玩家当前设置的称号
/// </summary>
public class PlayerTitleDisplay : Node2D
{
    private Label _titleLabel;
    private Player _player;
    private Camera2D _camera;
    private bool _isVisible = true;
    
    // 称号显示偏移
    private Vector2 _offset = new Vector2(0, -60);
    
    public override void _Ready()
    {
        // 创建标签
        _titleLabel = new Label();
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.Valign = Label.VAlign.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 14);
        AddChild(_titleLabel);
        
        // 获取玩家节点
        _player = GetParent() as Player;
        
        // 查找相机
        CallDeferred(nameof(FindCamera));
        
        // 监听称号变化
        if (TitleSystem.Instance != null)
        {
            TitleSystem.Instance.OnCurrentTitleChanged += OnCurrentTitleChanged;
        }
        
        // 初始更新
        UpdateTitleDisplay();
    }
    
    private void FindCamera()
    {
        var players = GetTree().GetNodesInGroup("Player");
        if (players.Count > 0)
        {
            _player = players[0] as Player;
            if (_player != null && _player.GetNode("Camera2D") is Camera2D cam)
            {
                _camera = cam;
            }
        }
    }
    
    public override void _Process(double delta)
    {
        if (_player == null || !_isVisible) return;
        
        // 跟随玩家位置
        GlobalPosition = _player.GlobalPosition + _offset;
        
        // 根据相机位置调整Z-index，保持在玩家上方但在UI下方
        if (_camera != null)
        {
            ZIndex = _camera.ZIndex - 1;
        }
    }
    
    private void OnCurrentTitleChanged(string titleId)
    {
        UpdateTitleDisplay();
    }
    
    private void UpdateTitleDisplay()
    {
        if (TitleSystem.Instance == null || _titleLabel == null) return;
        
        var currentTitle = TitleSystem.Instance.GetCurrentTitle();
        if (currentTitle != null)
        {
            _titleLabel.Text = currentTitle.Name;
            
            // 根据稀有度设置颜色
            Color rarityColor = GetRarityColor(currentTitle.Rarity);
            _titleLabel.AddThemeColorOverride("font_color", rarityColor);
            
            // 设置描边效果（通过OutlineSize和OutlineColor）
            _titleLabel.AddThemeConstantOverride("outline_size", 2);
            _titleLabel.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 1));
            
            _titleLabel.Visible = true;
        }
        else
        {
            _titleLabel.Visible = false; 
        }
    }
    
    private Color GetRarityColor(TitleRarity rarity)
    {
        return rarity switch
        {
            TitleRarity.Common => new Color(0.7f, 0.7f, 0.7f),      // 灰色
            TitleRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),   // 绿色
            TitleRarity.Rare => new Color(0.2f, 0.5f, 1.0f),        // 蓝色
            TitleRarity.Epic => new Color(0.6f, 0.2f, 0.8f),        // 紫色
            TitleRarity.Legendary => new Color(1f, 0.6f, 0f),      // 金色
            _ => new Color(1f, 1f, 1f)
        };
    }
    
    public void SetVisible(bool visible)
    {
        _isVisible = visible;
        if (_titleLabel != null)
        {
            _titleLabel.Visible = visible && TitleSystem.Instance?.GetCurrentTitle() != null;
        }
    }
    
    public override void _ExitTree()
    {
        if (TitleSystem.Instance != null)
        {
            TitleSystem.Instance.OnCurrentTitleChanged -= OnCurrentTitleChanged;
        }
    }
}
