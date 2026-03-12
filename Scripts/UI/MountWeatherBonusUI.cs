// 天气坐骑加成UI
// MountWeatherBonusUI.cs

using Godot;
using System;
using System.Collections.Generic;

public partial class MountWeatherBonusUI : Control
{
    private Label weatherLabel;
    private Label bonusLabel;
    private VBoxContainer bonusContainer;
    private Button closeButton;
    private Button refreshButton;
    
    private MountWeatherBonusSystem.WeatherType currentWeather;
    
    public override void _Ready()
    {
        // 创建UI
        SetupUI();
        
        // 初始化显示
        if (MountWeatherBonusSystem.Instance != null)
        {
            currentWeather = MountWeatherBonusSystem.Instance.GetCurrentWeather();
            UpdateDisplay();
        }
    }
    
    private void SetupUI()
    {
        // 主容器
        var mainPanel = new PanelContainer
        {
            AnchorPreset = ControlPreset.CenterRight,
            OffsetRight = -50,
            OffsetTop = 50,
            OffsetBottom = -50,
            CustomMinimumSize = new Vector2(300, 0)
        };
        AddChild(mainPanel);
        
        var mainVBox = new VBoxContainer();
        mainPanel.AddChild(mainVBox);
        
        // 标题
        var titleLabel = new Label
        {
            Text = "🌤️ 天气坐骑加成",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 20);
        mainVBox.AddChild(titleLabel);
        
        // 天气显示
        weatherLabel = new Label
        {
            Text = "",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        weatherLabel.AddThemeFontSizeOverride("font_size", 18);
        mainVBox.AddChild(weatherLabel);
        
        // 分隔线
        var separator = new HSeparator();
        mainVBox.AddChild(separator);
        
        // 加成容器
        bonusContainer = new VBoxContainer();
        mainVBox.AddChild(bonusContainer);
        
        // 刷新按钮
        refreshButton = new Button
        {
            Text = "🎲 随机天气"
        };
        refreshButton.Pressed += OnRefreshPressed;
        mainVBox.AddChild(refreshButton);
        
        // 关闭按钮
        closeButton = new Button
        {
            Text = "关闭"
        };
        closeButton.Pressed += OnClosePressed;
        mainVBox.AddChild(closeButton);
        
        // 设置样式
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(8);
        mainPanel.AddThemeStyleboxOverride("panel", style);
    }
    
    private void UpdateDisplay()
    {
        if (MountWeatherBonusSystem.Instance == null) return;
        
        // 显示天气
        string weatherIcon = MountWeatherBonusSystem.Instance.GetWeatherIcon(currentWeather);
        string weatherName = MountWeatherBonusSystem.Instance.GetWeatherName(currentWeather);
        weatherLabel.Text = $"{weatherIcon} {weatherName}";
        
        // 清空加成显示
        foreach (var child in bonusContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // 显示各类坐骑加成
        var categories = new List<MountWeatherBonusSystem.MountCategory>
        {
            MountWeatherBonusSystem.MountCategory.Land,
            MountWeatherBonusSystem.MountCategory.Flying,
            MountWeatherBonusSystem.MountCategory.Aquatic
        };
        
        var categoryNames = new Dictionary<MountWeatherBonusSystem.MountCategory, string>
        {
            { MountWeatherBonusSystem.MountCategory.Land, "🐴 陆地坐骑" },
            { MountWeatherBonusSystem.MountCategory.Flying, "🦅 飞行坐骑" },
            { MountWeatherBonusSystem.MountCategory.Aquatic, "🐳 水生坐骑" }
        };
        
        foreach (var category in categories)
        {
            var bonuses = MountWeatherBonusSystem.Instance.GetMountWeatherBonus("", category);
            
            // 类别标题
            var categoryLabel = new Label
            {
                Text = categoryNames[category]
            };
            categoryLabel.AddThemeFontSizeOverride("font_size", 16);
            bonusContainer.AddChild(categoryLabel);
            
            // 显示加成
            if (bonuses.Count == 0)
            {
                var noBonusLabel = new Label
                {
                    Text = "  无加成",
                    Modulate = new Color(0.6f, 0.6f, 0.6f)
                };
                bonusContainer.AddChild(noBonusLabel);
            }
            else
            {
                foreach (var bonus in bonuses)
                {
                    string bonusText = bonus.Value > 0 ? $"  +{bonus.Value * 100:F0}%" : $"  {bonus.Value * 100:F0}%";
                    string attrName = GetAttributeChineseName(bonus.Key);
                    
                    var bonusLabel = new Label
                    {
                        Text = $"{attrName}: {bonusText}",
                        Modulate = bonus.Value > 0 ? new Color(0.3f, 0.9f, 0.3f) : new Color(0.9f, 0.3f, 0.3f)
                    };
                    bonusContainer.AddChild(bonusLabel);
                }
            }
        }
    }
    
    private string GetAttributeChineseName(string attribute)
    {
        var nameMap = new Dictionary<string, string>
        {
            { "speed", "速度" },
            { "attack", "攻击" },
            { "defense", "防御" },
            { "health", "生命" },
            { "magic", "魔法" },
            { "dodge", "闪避" },
            { "stealth", "隐蔽" },
            { "fire_resist", "火抗" },
            { "ice_resist", "冰抗" },
            { "lightning_resist", "雷抗" },
            { "lightning_damage", "雷伤害" }
        };
        
        return nameMap.ContainsKey(attribute) ? nameMap[attribute] : attribute;
    }
    
    private void OnRefreshPressed()
    {
        if (MountWeatherBonusSystem.Instance != null)
        {
            MountWeatherBonusSystem.Instance.RandomWeather();
            currentWeather = MountWeatherBonusSystem.Instance.GetCurrentWeather();
            UpdateDisplay();
        }
    }
    
    private void OnClosePressed()
    {
        Hide();
    }
    
    public void ShowUI()
    {
        Show();
        if (MountWeatherBonusSystem.Instance != null)
        {
            currentWeather = MountWeatherBonusSystem.Instance.GetCurrentWeather();
            UpdateDisplay();
        }
    }
}
