using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Collectible UI - displays collectible collection, details and progress
/// </summary>
public class CollectibleUI : Control
{
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
	
	// REQ-058-11: Migrated from Godot 3 .Connect() to C# event
	public event Action<string> OnCollectibleDiscoveredUI;
	private TextureRect _detailIcon;
	private Button _closeButton;

	private CollectibleData.CollectibleCategory? _currentCategoryFilter = null;
	private CollectibleData.CollectibleRarity? _currentRarityFilter = null;

	public override void _Ready()
	{
		SetupUI();
		ConnectSignals();
		RefreshCollectibles();
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
		leftPanel.SetHExpandFlags(Control.ExpandFlags.ExpandHorizontal);
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

		rarityFilter = new OptionButton();
		_rarityFilter.CustomMinimumSize = new Vector2(150, 0);
		_rarityFilter.Text = "All Rarities";
		PopulateRarityFilter();
		filterBox.AddChild(rarityFilter);

		// Collectible grid
		var scrollContainer = new ScrollContainer();
		scrollContainer.SetVExpandFlags(Control.ExpandFlags.ExpandFill);
		leftPanel.AddChild(scrollContainer);

		_collectibleGrid = new GridContainer();
		_collectibleGrid.Columns = 5;
		_collectibleGrid.SetHExpandFlags(Control.ExpandFlags.ExpandHorizontal);
		_collectibleGrid.SetVExpandFlags(Control.ExpandFlags.ExpandFill);
		_collectibleGrid.AddThemeConstantOverride("h_separation", 10);
		_collectibleGrid.AddThemeConstantOverride("v_separation", 10);
		scrollContainer.AddChild(_collectibleGrid);

		// Right panel - details
		_detailPanel = new VBoxContainer();
		_detailPanel.CustomMinimumSize = new Vector2(300, 0);
		mainHBox.AddChild(_detailPanel);

		var detailTitle = new Label();
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
		modulate = new Color(1, 1, 1, 0);
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

		// Connect system signals (REQ-058-11: migrated from Godot 3 .Connect() to C# event +=)
		CollectibleSystem.Instance.CollectibleDiscovered += OnCollectibleDiscovered;
	}

	private void RefreshCollectibles()
	{
		// Update progress
		int discovered = CollectibleSystem.Instance.GetDiscoveredCount();
		int total = CollectibleSystem.Instance.GetTotalCount();
		_progressLabel.Text = $"Discovered: {discovered}/{total} ({GetProgressPercent(discovered, total):F1}%)";

		// Clear grid
		foreach (Node child in _collectibleGrid.GetChildren())
		{
			child.QueueFree();
		}

		// Get collectibles based on filters
		var allCollectibles = CollectibleDatabase.Instance.AllCollectibles;

		foreach (var kvp in allCollectibles)
		{
			var collectible = kvp.Value;

			// Apply category filter
			if (_currentCategoryFilter.HasValue && collectible.Category != _currentCategoryFilter.Value)
				continue;

			// Apply rarity filter
			if (_currentRarityFilter.HasValue && collectible.Rarity != _currentRarityFilter.Value)
				continue;

			// Create item
			var item = CreateCollectibleItem(collectible);
			_collectibleGrid.AddChild(item);
		}
	}

	private Control CreateCollectibleItem(CollectibleData collectible)
	{
		var container = new VBoxContainer();
		container.CustomMinimumSize = new Vector2(90, 100);

		var isDiscovered = CollectibleSystem.Instance.IsDiscovered(collectible.Id);

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
				ShowDetails(collectible);
			}
		};

		return container;
	}

	private void ShowDetails(CollectibleData collectible)
	{
		bool isDiscovered = CollectibleSystem.Instance.IsDiscovered(collectible.Id);

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
		RefreshCollectibles();
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
		RefreshCollectibles();
	}

	private void OnCollectibleDiscovered(string collectibleId)
	{
		// REQ-058-11: Invoke new event
		OnCollectibleDiscoveredUI?.Invoke(collectibleId);
		RefreshCollectibles();
	}

	private void OnClosePressed()
	{
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 0.0, 0.2f).SetTrans(Tween.TransitionType.Back);
		tween.TweenCallback(QueueFree);
	}

	public override void _Input(InputEvent evt)
	{
		if (evt.IsActionPressed("ui_cancel") || evt.IsActionPressed("ui_collectible"))
		{
			OnClosePressed();
		}
	}
}
