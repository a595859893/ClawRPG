using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.UI;

/// <summary>
/// 遗物协同面板 — 基地/SafeHouse 查看所有已发现协同
/// </summary>
public partial class RelicSynergyPanel : Control
{
    private VBoxContainer _synergyList;
    private Label _emptyLabel;
    private ScrollContainer _scrollContainer;
    private int _displayedCount = 0;
    
    public override void _Ready()
    {
        // 初始化UI
        SetupUI();
        
        // 订阅协同发现信号，刷新列表
        if (RelicSynergySystem.Instance != null)
        {
            RelicSynergySystem.Instance.Connect("SynergyDiscovered",
                Callable.From<string, string>(OnSynergyDiscovered));
        }
    }
    
    private void SetupUI()
    {
        // 标题
        var title = new Label
        {
            Text = "✦ 已发现遗物协同",
            Align = Label.AlignModeEnum.Left,
            AutowrapMode = TextServer.AutowrapMode.Off
        };
        title.AddThemeColorOverride("font_color", new Color(0.9f, 0.75f, 1.0f, 1.0f));
        title.AddThemeFontSizeOverride("font_size", 16);
        AddChild(title);
        
        // 空状态提示
        _emptyLabel = new Label
        {
            Text = "尚未发现任何协同...\n多装备遗物也许会有惊喜",
            Align = Label.AlignModeEnum.Center,
            AutowrapMode = TextServer.AutowrapMode.Word
        };
        _emptyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.6f, 1.0f));
        _emptyLabel.AddThemeFontSizeOverride("font_size", 13);
        _emptyLabel.Visible = false;
        AddChild(_emptyLabel);
        
        // 滚动区域
        _scrollContainer = new ScrollContainer
        {
            VerticalScrollBarRestricted = ScrollContainer.ScrollBarMode.Auto,
            HorizontalScrollBarRestricted = ScrollContainer.ScrollBarMode.Disabled
        };
        _scrollContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_scrollContainer);
        
        _synergyList = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(0, 0)
        };
        _scrollContainer.AddChild(_synergyList);
        
        RefreshDisplay();
    }
    
    private void OnSynergyDiscovered(string synergyId, string message)
    {
        RefreshDisplay();
    }
    
    /// <summary>
    /// 刷新协同列表显示
    /// </summary>
    public void RefreshDisplay()
    {
        if (_synergyList == null) return;
        
        // 清空现有项
        foreach (var child in _synergyList.GetChildren())
            child.QueueFree();
        
        var discoveries = RelicSynergySystem.Instance?.GetAllTimeDiscoveries();
        
        if (discoveries == null || discoveries.Count == 0)
        {
            _emptyLabel.Visible = true;
            return;
        }
        
        _emptyLabel.Visible = false;
        
        foreach (var synergyId in discoveries)
        {
            var entry = RelicSynergySystem.Instance?.GetSynergyDetails(synergyId);
            if (entry == null) continue;
            
            var item = CreateSynergyItem(entry);
            _synergyList.AddChild(item);
        }
    }
    
    private Control CreateSynergyItem(RelicSynergyEntry entry)
    {
        var container = new PanelContainer
        {
            CustomMinimumSize = new Vector2(0, 64),
            ZIndex = 10
        };
        
        // 稀有度颜色
        var borderColor = GetRarityColor(entry.Rarity);
        var bgStyle = new StyleBoxFlat
        {
            BgColor = new Color(borderColor.R, borderColor.G, borderColor.B, 0.08f),
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 1,
            BorderWidthBottom = 1,
            BorderColor = new Color(borderColor.R, borderColor.G, borderColor.B, 0.5f),
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6,
            ContentMarginLeft = 12,
            ContentMarginRight = 12,
            ContentMarginTop = 8,
            ContentMarginBottom = 8
        };
        container.AddThemeStyleboxOverride("panel", bgStyle);
        
        var hbox = new HBoxContainer { };
        container.AddChild(hbox);
        
        // 稀有度标签
        var rarityLabel = new Label
        {
            Text = GetRarityIcon(entry.Rarity) + " ",
            Align = Label.AlignModeEnum.Center
        };
        hbox.AddChild(rarityLabel);
        
        // 协同名
        var nameLabel = new Label
        {
            Text = entry.SynergyName,
            Align = Label.AlignModeEnum.Left
        };
        nameLabel.AddThemeColorOverride("font_color", borderColor);
        nameLabel.AddThemeFontSizeOverride("font_size", 14);
        hbox.AddChild(nameLabel);
        
        // 加成标签
        var bonusLabel = new Label
        {
            Text = $" +{entry.BonusValue:P0} {entry.BonusType}",
            Align = Label.AlignModeEnum.Right
        };
        bonusLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.9f, 0.7f, 1.0f));
        bonusLabel.AddThemeFontSizeOverride("font_size", 12);
        hbox.AddChild(bonusLabel);
        
        // Tooltip 显示遗物组合
        container.GetTooltipText = () => $"遗物组合: {string.Join(" + ", entry.RelicIds)}\n\"{entry.DiscoveryMessage}\"";
        
        return container;
    }
    
    private Color GetRarityColor(string rarity)
    {
        return rarity switch
        {
            "common" => new Color(0.6f, 0.6f, 0.6f),
            "uncommon" => new Color(0.2f, 0.8f, 0.3f),
            "rare" => new Color(0.3f, 0.5f, 1.0f),
            "epic" => new Color(0.6f, 0.3f, 0.9f),
            "legendary" => new Color(1.0f, 0.6f, 0.1f),
            _ => new Color(0.7f, 0.7f, 0.7f)
        };
    }
    
    private string GetRarityIcon(string rarity)
    {
        return rarity switch
        {
            "common" => "◇",
            "uncommon" => "◆",
            "rare" => "★",
            "epic" => "★★",
            "legendary" => "★★★",
            _ => "•"
        };
    }
}
