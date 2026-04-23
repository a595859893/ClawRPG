using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Collectible UI - displays collectible collection, details and progress
/// 重构自 REQ-075: 移除对 CollectibleSystem/CollectibleDatabase 的直接引用，改为事件驱动解耦
/// </summary>
public partial class CollectibleUI : Control
{
	// ===== 事件接口（UI → System 通信） =====
	// UI 层通过事件向外部发送操作请求，不直接持有 System 引用

	/// <summary>请求刷新收集品列表（外部/System 收到后调用 UpdateCollectibles）</summary>
	public Action OnRefreshRequested;

	/// <summary>请求查看收集品详情（外部/System 收到后调用 UpdateDetails）</summary>
	public Action<string> OnCollectibleSelected;

	/// <summary>请求关闭界面（外部/System 收到后处理）</summary>
	public Action OnCloseRequested;

	/// <summary>请求按分类筛选（外部/System 收到后调用 UpdateCollectibles）</summary>
	public Action<CollectibleData.CollectibleCategory?> OnCategoryFilterChanged;

	/// <summary>请求按稀有度筛选（外部/System 收到后调用 UpdateCollectibles）</summary>
	public Action<CollectibleData.CollectibleRarity?> OnRarityFilterChanged;

	// ===== UI 组件引用 (通过GetNode获取) =====
	private Label _titleLabel;
	private Label _progressLabel;
	private GridContainer _collectibleGrid;
	private OptionButton _categoryFilter;
	private OptionButton _rarityFilter;
	private VBoxContainer _detailPanel;
	private Label _detailName;
	private Label _detailDescription;
	private Label _detailCategory;
	private Label _detailRarity;
	private Label _detailRewards;
	private TextureRect _detailIcon;
	private Button _closeButton;

	// REQ-058-11: Migrated from Godot 3 .Connect() to C# event
	public event Action<string> OnCollectibleDiscoveredUI;

	private CollectibleData.CollectibleCategory? _currentCategoryFilter = null;
	private CollectibleData.CollectibleRarity? _currentRarityFilter = null;

	// ===== 生命周期 =====

	public override void _Ready()
	{
		SetupUI();
		ConnectSignals();
		// REQ-075 解耦：不再直接调用 RefreshCollectibles()，
		// 而是通过事件请求外部/System 提供数据
		OnRefreshRequested?.Invoke();
	}

	private void SetupUI()
	{
		// Main panel
		var mainPanel = new PanelContainer();
		mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
		mainPanel.CustomMinimumSize = new Vector2(900, 600);
		AddChild(mainPanel);

		var mainHBox = new HBoxContainer();
		mainPanel.AddChild(mainHBox);

		// Left panel - filters and grid
		var leftPanel = new VBoxContainer();
		leftPanel.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
		leftPanel.CustomMinimumSize = new Vector2(550, 0);
		mainHBox.AddChild(leftPanel);

		// Title
		_titleLabel = new Label();
		_titleLabel.Text = "📜 Collectible Encyclopedia";
		_titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_titleLabel.AddThemeFontSizeOverride("font_size", 24);
		leftPanel.AddChild(_titleLabel);

		// Progress
		_progressLabel = new Label();
		_progressLabel.HorizontalAlignment = HorizontalAlignment.Center;
		leftPanel.AddChild(_progressLabel);

		// Filters
		var filterBox = new HBoxContainer();
		leftPanel.AddChild(filterBox);

		_categoryFilter = new OptionButton();
		_categoryFilter.CustomMinimumSize = new Vector2(200, 0);
		_categoryFilter.Text = "All Categories";
		PopulateCategoryFilter();
		filterBox.AddChild(_categoryFilter);

		_rarityFilter = new OptionButton();
		_rarityFilter.CustomMinimumSize = new Vector2(150, 0);
		_rarityFilter.Text = "All Rarities";
		PopulateRarityFilter();
		filterBox.AddChild(_rarityFilter);

		// Collectible grid
		var scrollContainer = new ScrollContainer();
		scrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		leftPanel.AddChild(scrollContainer);

		_collectibleGrid = new GridContainer();
		_collectibleGrid.Columns = 5;
		_collectibleGrid.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
		_collectibleGrid.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		_collectibleGrid.AddThemeConstantOverride("h_separation", 10);
		_collectibleGrid.AddThemeConstantOverride("v_separation", 10);
		scrollContainer.AddChild(_collectibleGrid);

		// Right panel - details
		_detailPanel = new VBoxContainer();
		_detailPanel.CustomMinimumSize = new Vector2(300, 0);
		mainHBox.AddChild(_detailPanel);

		_detailName = new Label();
		_detailName.Text = "Select a collectible";
		_detailName.HorizontalAlignment = HorizontalAlignment.Center;
		_detailName.AddThemeFontSizeOverride("font_size", 20);
		_detailPanel.AddChild(_detailName);

		_detailIcon = new TextureRect();
		_detailIcon.CustomMinimumSize = new Vector2(100, 100);
		_detailIcon.SetAnchorsPreset(Control.LayoutPreset.Center);
		_detailPanel.AddChild(_detailIcon);

		_detailDescription = new Label();
		_detailDescription.HorizontalAlignment = HorizontalAlignment.Center;
		_detailDescription.AutowrapMode = TextServer.AutowrapMode.Word;
		_detailPanel.AddChild(_detailDescription);

		_detailCategory = new Label();
		_detailPanel.AddChild(_detailCategory);

		_detailRarity = new Label();
		_detailPanel.AddChild(_detailRarity);

		_detailRewards = new Label();
		_detailPanel.AddChild(_detailRewards);

		// Close button
		_closeButton = new Button();
		_closeButton.Text = "Close (K)";
		_closeButton.CustomMinimumSize = new Vector2(0, 50);
		_detailPanel.AddChild(_closeButton);

		// Animate in
		Modulate = new Color(1, 1, 1, 0);
		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(this, "modulate:a", 1.0, 0.3f).SetTrans(Tween.TransitionType.Back);
	}

	private void PopulateCategoryFilter()
	{
		_categoryFilter.Clear();
		_categoryFilter.AddItem("All Categories", 0);

		int index = 1;
		foreach (CollectibleData.CollectibleCategory category in Enum.GetValues(typeof(CollectibleData.CollectibleCategory)))
		{
			_categoryFilter.AddItem(category.ToString(), index++);
		}
	}

	private void PopulateRarityFilter()
	{
		_rarityFilter.Clear();
		_rarityFilter.AddItem("All Rarities", 0);

		int index = 1;
		foreach (CollectibleData.CollectibleRarity rarity in Enum.GetValues(typeof(CollectibleData.CollectibleRarity)))
		{
			_rarityFilter.AddItem(rarity.ToString(), index++);
		}
	}

	private void ConnectSignals()
	{
		_categoryFilter.ItemSelected += OnCategorySelected;
		_rarityFilter.ItemSelected += OnRaritySelected;
		_closeButton.Pressed += OnClosePressed;
		// REQ-075 解耦：不再直接连接 CollectibleSystem.Instance 信号
		// 而是通过 OnCollectibleSelected 事件请求外部处理
	}

	// ===== 公开更新接口（System → UI 通信） =====
	// REQ-075 解耦：UI 不再主动拉取数据，而是等待外部推送

	/// <summary>
	/// 更新收集品列表显示（由外部/System 调用）
	/// </summary>
	public void UpdateCollectibles(List<CollectibleData> collectibles, int discoveredCount, int totalCount)
	{
		// Update progress
		_progressLabel.Text = $"Discovered: {discoveredCount}/{totalCount} ({GetProgressPercent(discoveredCount, totalCount):F1}%)";

		// Clear grid
		foreach (Node child in _collectibleGrid.GetChildren())
		{
			child.QueueFree();
		}

		// Get collectibles based on filters
		foreach (var collectible in collectibles)
		{
			// Apply category filter
			if (_currentCategoryFilter.HasValue && collectible.Category != _currentCategoryFilter.Value)
				continue;

			// Apply rarity filter
			if (_currentRarityFilter.HasValue && collectible.Rarity != _currentRarityFilter.Value)
				continue;

			// Create item
			var item = CreateCollectibleItem(collectible, IsDiscovered(collectible.Id));
			_collectibleGrid.AddChild(item);
		}
	}

	/// <summary>
	/// 更新收集品详情显示（由外部/System 调用）
	/// </summary>
	public void UpdateDetails(CollectibleData collectible, bool isDiscovered)
	{
		_detailName.Text = isDiscovered ? collectible.Name : "???";
		_detailDescription.Text = isDiscovered ? collectible.Description : "Not yet discovered";
		_detailCategory.Text = $"Category: {(isDiscovered ? collectible.Category.ToString() : "???")}";
		_detailRarity.Text = $"Rarity: {(isDiscovered ? collectible.Rarity.ToString() : "???")}";
		_detailRarity.Modulate = isDiscovered ? GetRarityColor(collectible.Rarity) : Colors.Gray;

		if (isDiscovered && collectible.GoldReward > 0)
		{
			_detailRewards.Text = $"Rewards: +{collectible.GoldReward} Gold, +{collectible.ExpReward} EXP";
		}
		else
		{
			_detailRewards.Text = "Rewards: ???";
		}

		_detailIcon.Color = isDiscovered ? GetRarityColor(collectible.Rarity) : new Color(0.3f, 0.3f, 0.3f);
	}

	/// <summary>
	/// 通知收集品被发现（由外部/System 调用，触发刷新）
	/// </summary>
	public void NotifyCollectibleDiscovered(string collectibleId)
	{
		OnCollectibleDiscoveredUI?.Invoke(collectibleId);
		// 请求刷新
		OnRefreshRequested?.Invoke();
	}

	// ===== 内部辅助方法 =====

	private bool IsDiscovered(string collectibleId)
	{
		// REQ-075 注意：此方法需要外部通过 UpdateCollectibles 传入发现状态
		// 此处暂时保留直接调用，后续可移除
		return CollectibleSystem.Instance.IsDiscovered(collectibleId);
	}

	private Control CreateCollectibleItem(CollectibleData collectible, bool isDiscovered)
	{
		var container = new VBoxContainer();
		container.CustomMinimumSize = new Vector2(90, 100);

		// Icon (colored based on rarity)
		var iconBg = new ColorRect();
		iconBg.CustomMinimumSize = new Vector2(80, 80);
		iconBg.Color = isDiscovered ? GetRarityColor(collectible.Rarity) : new Color(0.2f, 0.2f, 0.2f, 0.5f);
		container.AddChild(iconBg);

		var iconLabel = new Label();
		iconLabel.Text = isDiscovered ? GetCategoryIcon(collectible.Category) : "?";
		iconLabel.SetAnchorsPreset(Control.LayoutPreset.Center);
		iconLabel.HorizontalAlignment = HorizontalAlignment.Center;
		iconLabel.VerticalAlignment = VerticalAlignment.Center;
		iconLabel.AddThemeFontSizeOverride("font_size", 32);
		iconBg.AddChild(iconLabel);

		// Name
		var nameLabel = new Label();
		nameLabel.Text = isDiscovered ? collectible.Name : "???";
		nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
		nameLabel.AutowrapMode = TextServer.AutowrapMode.Word;
		nameLabel.AddThemeFontSizeOverride("font_size", 11);
		container.AddChild(nameLabel);

		// Click to show details
		container.GuiInput += (evt) =>
		{
			if (evt is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
			{
				// REQ-075 解耦：通过事件请求外部处理
				OnCollectibleSelected?.Invoke(collectible.Id);
			}
		};

		return container;
	}

	private Color GetRarityColor(CollectibleData.CollectibleRarity rarity)
	{
		return rarity switch
		{
			CollectibleData.CollectibleRarity.Common => new Color(0.7f, 0.7f, 0.7f),
			CollectibleData.CollectibleRarity.Uncommon => new Color(0.3f, 0.8f, 0.3f),
			CollectibleData.CollectibleRarity.Rare => new Color(0.3f, 0.5f, 0.9f),
			CollectibleData.CollectibleRarity.Epic => new Color(0.7f, 0.4f, 0.9f),
			CollectibleData.CollectibleRarity.Legendary => new Color(1f, 0.7f, 0.2f),
			_ => Colors.White
		};
	}

	private string GetCategoryIcon(CollectibleData.CollectibleCategory category)
	{
		return category switch
		{
			CollectibleData.CollectibleCategory.Item => "🧪",
			CollectibleData.CollectibleCategory.Equipment => "⚔️",
			CollectibleData.CollectibleCategory.Enemy => "👹",
			CollectibleData.CollectibleCategory.Boss => "👺",
			CollectibleData.CollectibleCategory.Mount => "🐴",
			CollectibleData.CollectibleCategory.Pet => "🐕",
			CollectibleData.CollectibleCategory.Region => "🏔️",
			CollectibleData.CollectibleCategory.Material => "💎",
			CollectibleData.CollectibleCategory.Skill => "✨",
			CollectibleData.CollectibleCategory.Achievement => "🏆",
			_ => "📜"
		};
	}

	private float GetProgressPercent(int discovered, int total)
	{
		return total > 0 ? (float)discovered / total * 100f : 0f;
	}

	private void OnCategorySelected(long index)
	{
		if (index == 0)
		{
			_currentCategoryFilter = null;
		}
		else
		{
			var categories = Enum.GetValues(typeof(CollectibleData.CollectibleCategory));
			_currentCategoryFilter = (CollectibleData.CollectibleCategory)categories.GetValue((int)index - 1);
		}
		// REQ-075 解耦：通过事件请求外部处理
		OnCategoryFilterChanged?.Invoke(_currentCategoryFilter);
	}

	private void OnRaritySelected(long index)
	{
		if (index == 0)
		{
			_currentRarityFilter = null;
		}
		else
		{
			var rarities = Enum.GetValues(typeof(CollectibleData.CollectibleRarity));
			_currentRarityFilter = (CollectibleData.CollectibleRarity)rarities.GetValue((int)index - 1);
		}
		// REQ-075 解耦：通过事件请求外部处理
		OnRarityFilterChanged?.Invoke(_currentRarityFilter);
	}

	private void OnCollectibleDiscovered(string collectibleId)
	{
		// REQ-058-11: Invoke new event
		OnCollectibleDiscoveredUI?.Invoke(collectibleId);
		// REQ-075 解耦：请求外部刷新
		OnRefreshRequested?.Invoke();
	}

	private void OnClosePressed()
	{
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 0.0, 0.2f).SetTrans(Tween.TransitionType.Back);
		tween.TweenCallback(QueueFree);
		OnCloseRequested?.Invoke();
	}

	public override void _Input(InputEvent evt)
	{
		if (evt.IsActionPressed("ui_cancel") || evt.IsActionPressed("ui_collectible"))
		{
			OnClosePressed();
		}
	}
}
