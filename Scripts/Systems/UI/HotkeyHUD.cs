using Godot;
using System;
using System.Collections.Generic;

public partial class HotkeyHUD : Control
{
	[Export] public Color backgroundColor = new Color(0, 0, 0, 0.85f);
	[Export] public Color textColor = new Color(1, 1, 1, 1f);
	[Export] public Color hotkeyColor = new Color(1, 0.8f, 0.2f, 1f);
	[Export] public Color categoryColor = new Color(0.4f, 0.8f, 1f, 1f);
	
	private bool isVisible = false;
	private VBoxContainer mainContainer;
	private Label titleLabel;
	private GridContainer hotkeyGrid;
	
	// Hotkey categories and bindings
	private readonly Dictionary<string, List<(string key, string action)>> hotkeyCategories = new()
	{
		{ "角色", new List<(string, string)>
			{
				("P", "玩家属性"),
				("I", "背包"),
				("K", "技能"),
				("T", "技能树"),
				("M", "技能大师"),
				("Ctrl+P", "声望转生"),
			}
		},
		{ "战斗", new List<(string, string)>
			{
				("[", "战斗状态"),
				("K", "技能冷却"),
				("Shift+D", "动态难度"),
				("V", "战斗特效"),
				("Ctrl+A", "竞技场"),
				("Shift+T", "锦标赛"),
			}
		},
		{ "宠物与坐骑", new List<(string, string)>
			{
				("J", "宠物"),
				("Ctrl+E", "宠物蛋"),
				("O", "宠物幻化"),
				("Ctrl+G", "宠物守卫"),
				("Shift+P", "宠物装备"),
				("Ctrl+B", "宠物繁殖"),
			}
		},
		{ "坐骑系统", new List<(string, string)>
			{
				("Ctrl+R", "坐骑远征"),
				("Shift+R", "坐骑竞赛"),
				("Ctrl+M", "坐骑战斗"),
				("J", "坐骑进化"),
				("K", "坐骑装备"),
			}
		},
		{ "社交与公会", new List<(string, string)>
			{
				("G", "公会"),
				("Shift+G", "公会任务"),
				("Ctrl+P", "队伍"),
			}
		},
		{ "经济与制作", new List<(string, string)>
			{
				("H", "商店"),
				("R", "装备回收"),
				("E", "附魔"),
				("L", "炼金"),
				("Shift+P", "烹饪"),
				("Shift+G", "采集"),
				("Ctrl+M", "制作大师"),
			}
		},
		{ "探索与活动", new List<(string, string)>
			{
				("D", "每日副本"),
				("Q", "每日任务"),
				("O", "随机事件"),
				("Shift+E", "季节活动"),
				("Ctrl+T", "封印之塔"),
				("W", "世界Boss"),
			}
		},
		{ "收藏与成就", new List<(string, string)>
			{
				("N", "称号"),
				("K", "百科全书"),
				("Ctrl+S", "秘密成就"),
				("Shift+M", "音乐收藏"),
				("B", "骰子大师"),
			}
		},
		{ "其他系统", new List<(string, string)>
			{
				("Y", "拍卖行"),
				("T", "交易"),
				("F", "宝石融合"),
				("Z", "宝石镶嵌"),
				("Ctrl+W", "天气"),
				("Ctrl+Shift+A", "无障碍设置"),
			}
		},
	};
	
	public override void _Ready()
	{
		SetupUI();
		Visible = false;
	}
	
	private void SetupUI()
	{
		// Main container
		mainContainer = new VBoxContainer();
		mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
		mainContainer.CustomMinimumSize = new Vector2(600, 500);
		mainContainer.Position = new Vector2(-300, -250);
		mainContainer.AddThemeConstantOverride("separation", 15);
		AddChild(mainContainer);
		
		// Title
		titleLabel = new Label();
		titleLabel.Text = "⌨️ 快捷键指南";
		titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		titleLabel.AddThemeFontSizeOverride("font_size", 24);
		titleLabel.AddThemeColorOverride("font_color", hotkeyColor);
		mainContainer.AddChild(titleLabel);
		
		// Scroll container for categories
		ScrollContainer scroll = new ScrollContainer();
		scroll.CustomMinimumSize = new Vector2(580, 450);
		scroll.VerticalScrollMode = ScrollContainer.ScrollMode.Enabled;
		mainContainer.AddChild(scroll);
		
		// Categories container
		VBoxContainer categoriesContainer = new VBoxContainer();
		categoriesContainer.AddThemeConstantOverride("separation", 20);
		scroll.AddChild(categoriesContainer);
		
		// Add each category
		foreach (var category in hotkeyCategories)
		{
			Control categoryPanel = CreateCategoryPanel(category.Key, category.Value);
			categoriesContainer.AddChild(categoryPanel);
		}
		
		// Hint text
		Label hintLabel = new Label();
		hintLabel.Text = "按 H 键切换显示";
		hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
		hintLabel.AddThemeFontSizeOverride("font_size", 14);
		hintLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f, 1f));
		mainContainer.AddChild(hintLabel);
	}
	
	private Control CreateCategoryPanel(string categoryName, List<(string key, string action)> hotkeys)
	{
		Control panel = new Control();
		panel.CustomMinimumSize = new Vector2(550, 0);
		
		VBoxContainer container = new VBoxContainer();
		container.AddThemeConstantOverride("separation", 5);
		panel.AddChild(container);
		
		// Category title
		Label categoryLabel = new Label();
		categoryLabel.Text = "▸ " + categoryName;
		categoryLabel.AddThemeFontSizeOverride("font_size", 16);
		categoryLabel.AddThemeColorOverride("font_color", categoryColor);
		container.AddChild(categoryLabel);
		
		// Hotkey grid
		GridContainer grid = new GridContainer();
		grid.Columns = 2;
		grid.AddThemeConstantOverride("h_separation", 20);
		grid.AddThemeConstantOverride("v_separation", 8);
		container.AddChild(grid);
		
		// Add hotkey entries
		foreach (var hotkey in hotkeys)
		{
			HBoxContainer hotkeyRow = new HBoxContainer();
			hotkeyRow.Alignment = BoxContainer.AlignmentMode.Center;
			hotkeyRow.CustomMinimumSize = new Vector2(250, 0);
			
			// Key label
			Label keyLabel = new Label();
			keyLabel.Text = hotkey.key;
			keyLabel.HorizontalAlignment = HorizontalAlignment.Center;
			keyLabel.CustomMinimumSize = new Vector2(60, 0);
			keyLabel.AddThemeFontSizeOverride("font_size", 14);
			keyLabel.AddThemeColorOverride("font_color", hotkeyColor);
			keyLabel.AddThemeStyleBoxOverride("normal", CreateKeyStyleBox());
			hotkeyRow.AddChild(keyLabel);
			
			// Action label
			Label actionLabel = new Label();
			actionLabel.Text = hotkey.action;
			actionLabel.AddThemeFontSizeOverride("font_size", 14);
			actionLabel.AddThemeColorOverride("font_color", textColor);
			actionLabel.CustomMinimumSize = new Vector2(180, 0);
			hotkeyRow.AddChild(actionLabel);
			
			grid.AddChild(hotkeyRow);
		}
		
		return panel;
	}
	
	private StyleBoxFlat CreateKeyStyleBox()
	{
		StyleBoxFlat style = new StyleBoxFlat();
		style.BgColor = new Color(0.2f, 0.2f, 0.2f, 1f);
		style.BorderWidthLeft = 2;
		style.BorderWidthRight = 2;
		style.BorderWidthTop = 2;
		style.BorderWidthBottom = 2;
		style.BorderColor = hotkeyColor;
		style.CornerRadiusTopLeft = 4;
		style.CornerRadiusTopRight = 4;
		style.CornerRadiusBottomLeft = 4;
		style.CornerRadiusBottomRight = 4;
		style.ContentMarginLeft = 8;
		style.ContentMarginRight = 8;
		style.ContentMarginTop = 4;
		style.ContentMarginBottom = 4;
		return style;
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			// Toggle with H key
			if (keyEvent.Keycode == Key.H)
			{
				Toggle();
			}
			// Close with Escape
			else if (keyEvent.Keycode == Key.Escape && isVisible)
			{
				Hide();
			}
		}
	}
	
	public void Toggle()
	{
		if (isVisible)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}
	
	public void Show()
	{
		Visible = true;
		isVisible = true;
		// Animate in
		Modulate = new Color(1, 1, 1, 0);
		Tween tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 1f, 0.2f);
	}
	
	public void Hide()
	{
		isVisible = false;
		// Animate out
		Tween tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 0f, 0.2f);
		tween.TweenCallback(Callable.From(() => Visible = false));
	}
	
	public bool IsVisible() => isVisible;
}
