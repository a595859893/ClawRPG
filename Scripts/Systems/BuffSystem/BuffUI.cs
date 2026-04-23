using Godot;
using System;
using System.Collections.Generic;

public partial class BuffUI : Control
{
	private static BuffUI _instance;
	public static BuffUI Instance
	{
		get { return _instance; }
	}
	
	private PanelContainer _mainPanel;
	private VBoxContainer _buffListContainer;
	private Label _titleLabel;
	private Label _statsLabel;
	private ScrollContainer _scrollContainer;
	
	private bool _isVisible = false; 
	
	public override void _Ready()
	{
		_instance = this;
		SetupUI();
		Visible = false; 
		
		// 连接到BuffSystem信号
		if (BuffSystem.Instance != null)
		{
			BuffSystem.BuffListChanged += OnBuffListChanged;
			BuffSystem.BuffApplied += (b, s) => OnBuffApplied(b, s);
			BuffSystem.BuffRemoved += b => OnBuffRemoved(b);
		}
	}
	
	private void SetupUI()
	{
		// 主面板
		_mainPanel = new PanelContainer();
		_mainPanel.SetAnchorsPreset(Control.LayoutPreset.RightWide);
		_mainPanel.OffsetLeft = -320;
		_mainPanel.OffsetTop = 50;
		_mainPanel.OffsetRight = -20;
		_mainPanel.OffsetBottom = -50;
		_mainPanel.CustomMinimumSize = new Vector2(300, 0);
		
		// 样式
		StyleBoxFlat style = new StyleBoxFlat();
		style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
		style.BorderColor = new Color(0.3f, 0.3f, 0.4f, 1f);
		style.SetBorderWidthAll(2);
		style.SetCornerRadiusAll(8);
		style.SetContentMarginAll(10);
		_mainPanel.AddThemeStyleboxOverride("panel", style);
		
		// 垂直布局
		VBoxContainer mainVBox = new VBoxContainer();
		
		mainVBox.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		_mainPanel.AddChild(mainVBox);
		
		// 标题
		_titleLabel = new Label();
		_titleLabel.Text = "  状态效果";
		_titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.6f, 1f));
		_titleLabel.AddThemeFontOverride("font", GD.Load<Font>("res://Fonts/TitleFont.tres"));
		mainVBox.AddChild(_titleLabel);
		
		// 分隔线
		HSeparator separator = new HSeparator();
		separator.OffsetTop = 5;
		separator.OffsetBottom = -5;
		mainVBox.AddChild(separator);
		
		// Buff列表滚动容器
		_scrollContainer = new ScrollContainer();
		_scrollContainer.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
		_scrollContainer.VerticalScrollMode = ScrollContainer.ScrollMode.ShowAlways;
		_scrollContainer.OffsetTop = 10;
		_scrollContainer.OffsetBottom = -10;
		mainVBox.AddChild(_scrollContainer);
		
		// Buff列表容器
		_buffListContainer = new VBoxContainer();
		_buffListContainer.SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand;
		_scrollContainer.AddChild(_buffListContainer);
		
		// 统计信息
		_statsLabel = new Label();
		_statsLabel.OffsetTop = 10;
		_statsLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f, 1f));
		mainVBox.AddChild(_statsLabel);
		
		AddChild(_mainPanel);
	}
	
	private void OnBuffListChanged()
	{
		RefreshBuffList();
	}
	
	private void OnBuffApplied(string buffId, int stackCount)
	{
		RefreshBuffList();
	}
	
	private void OnBuffRemoved(string buffId)
	{
		RefreshBuffList();
	}
	
	private void RefreshBuffList()
	{
		// 清除现有项
		foreach (Node child in _buffListContainer.GetChildren())
		{
			child.QueueFree();
		}
		
		if (BuffSystem.Instance == null) return;
		
		List<ActiveBuff> buffs = BuffSystem.Instance.GetAllActiveBuffs();
		
		if (buffs.Count == 0)
		{
			Label emptyLabel = new Label();
			emptyLabel.Text = "  无状态效果";
			emptyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.6f, 1f));
			emptyLabel.HorizontalAlignment = HorizontalAlignment.Left;
			_buffListContainer.AddChild(emptyLabel);
		}
		else
		{
			foreach (var buff in buffs)
			{
				CreateBuffItem(buff);
			}
		}
		
		// 更新统计
		UpdateStats();
	}
	
	private void CreateBuffItem(ActiveBuff buff)
	{
		HBoxContainer itemContainer = new HBoxContainer();
		itemContainer.SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand;
		itemContainer.OffsetBottom = -5;
		
		// 图标背景
		Panel iconPanel = new Panel();
		iconPanel.CustomMinimumSize = new Vector2(32, 32);
		iconPanel.OffsetRight = -8;
		
		// 颜色根据buff类型
		StyleBoxFlat iconStyle = new StyleBoxFlat();
		if (buff.Info.IsDebuff)
		{
			iconStyle.BgColor = new Color(0.8f, 0.2f, 0.2f, 0.8f);  // 红色减益
		}
		else
		{
			iconStyle.BgColor = new Color(0.2f, 0.6f, 0.8f, 0.8f);  // 蓝色增益
		}
		iconStyle.SetCornerRadiusAll(4);
		iconPanel.AddThemeStyleboxOverride("panel", iconStyle);
		
		itemContainer.AddChild(iconPanel);
		
		// 信息容器
		VBoxContainer infoContainer = new VBoxContainer();
		infoContainer.SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand;
		
		// 名称和层数
		HBoxContainer nameRow = new HBoxContainer();
		
		Label nameLabel = new Label();
		nameLabel.Text = buff.Info.Name;
		nameLabel.AddThemeColorOverride("font_color", buff.Info.IsDebuff ? new Color(1f, 0.5f, 0.5f) : new Color(0.5f, 1f, 0.8f));
		nameRow.AddChild(nameLabel);
		
		if (buff.StackCount > 1)
		{
			Label stackLabel = new Label();
			stackLabel.Text = $" x{buff.StackCount}";
			stackLabel.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0.2f, 1f));
			nameRow.AddChild(stackLabel);
		}
		
		infoContainer.AddChild(nameRow);
		
		// 持续时间或进度
		ProgressBar timeBar = new ProgressBar();
		timeBar.SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand;
		timeBar.CustomMinimumSize = new Vector2(0, 8);
		
		float maxTime = buff.Info.Duration > 0 ? buff.Info.Duration : 1f;
		float progress = buff.TimeRemaining / maxTime;
		progress = Mathf.Clamp(progress, 0f, 1f);
		timeBar.Value = progress * 100;
		
		StyleBoxFlat barStyle = new StyleBoxFlat();
		barStyle.BgColor = new Color(0.2f, 0.2f, 0.3f, 0.8f);
		barStyle.SetCornerRadiusAll(2);
				timeBar.AddThemeStyleboxOverride("background", barStyle);
		
		StyleBoxFlat fillStyle = new StyleBoxFlat();
		fillStyle.BgColor = buff.Info.IsDebuff ? new Color(0.9f, 0.3f, 0.3f) : new Color(0.3f, 0.8f, 0.5f);
		fillStyle.SetCornerRadiusAll(2);
				timeBar.AddThemeStyleboxOverride("fill", fillStyle);
		
		infoContainer.AddChild(timeBar);
		
		itemContainer.AddChild(infoContainer);
		
		// 添加到列表
		_buffListContainer.AddChild(itemContainer);
	}
	
	private void UpdateStats()
	{
		if (BuffSystem.Instance == null)
		{
			_statsLabel.Text = "";
			return;
		}
		
		PlayerBuffData data = BuffSystem.Instance.GetBuffData();
		
		string stats = $"  增益: {data.TotalBuffsApplied} | 减益: {data.TotalDebuffsApplied}\n";
		stats += $"  活跃: {BuffSystem.Instance.GetUniqueBuffCount()} 个效果";
		
		_statsLabel.Text = stats;
	}
	
	// 切换显示
	public void ToggleBuffUI()
	{
		_isVisible = !_isVisible;
		Visible = _isVisible;
		
		if (_isVisible)
		{
			RefreshBuffList();
			
			// 显示动画
			Tween tween = CreateTween();
			tween.SetParallel(true);
			tween.TweenProperty(_mainPanel, "modulate:a", 1f, 0.3f);
			tween.TweenProperty(_mainPanel, "position:x", -320f, 0.3f).From(0f);
		}
		else
		{
			// 隐藏动画
			Tween tween = CreateTween();
			tween.SetParallel(true);
			tween.TweenProperty(_mainPanel, "modulate:a", 0f, 0.2f);
		}
	}
	
	public void ShowBuffUI()
	{
		if (!_isVisible)
			ToggleBuffUI();
	}
	
	public void HideBuffUI()
	{
		if (_isVisible)
			ToggleBuffUI();
	}
	
	// 输入处理
	public override void _Input(InputEvent eventObject)
	{
		if (eventObject is InputEventKey keyEvent && keyEvent.Pressed)
		{
			// V键切换buff界面
			if (keyEvent.Keycode == Key.V && !keyEvent.Echo)
			{
				ToggleBuffUI();
			}
		}
	}
}
