using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Collectible system - manages player collectibles, discovery and category completion
/// </summary>
public class CollectibleSystem
{
	private static CollectibleSystem _instance;
	public static CollectibleSystem Instance => _instance ??= new CollectibleSystem();

	public PlayerCollectibleData Data { get; private set; } = new PlayerCollectibleData();

	// Signals
	public Signal1<string> CollectibleDiscovered { get; } = new Signal1<string>();
	public Signal1<string> CategoryCompleted { get; } = new Signal1<string>();
	public Signal1<string> AllCollectiblesCompleted { get; } = new Signal1<string>();

	private CollectibleSystem() { }

	public void Initialize()
	{
		GD.Print("[CollectibleSystem] Initialized");
	}

	/// <summary>
	/// 发现收藏品
	/// </summary>
	/// <param name="collectibleId">收藏品ID</param>
	/// <returns>是否成功发现</returns>
	public bool DiscoverCollectible(string collectibleId)
	{
		var collectible = CollectibleDatabase.Instance.GetCollectible(collectibleId);
		if (collectible == null)
		{
			GD.Warning($"[CollectibleSystem] Collectible not found: {collectibleId}");
			return false;
		}

		// Check if already discovered
		if (Data.DiscoveredCollectibles.ContainsKey(collectibleId) && Data.DiscoveredCollectibles[collectibleId])
		{
			return false;
		}

		// Mark as discovered
		Data.DiscoveredCollectibles[collectibleId] = true;
		Data.TotalDiscovered++;

		// Update category count
		string categoryName = collectible.Category.ToString();
		if (Data.CategoryDiscovered.ContainsKey(categoryName))
		{
			Data.CategoryDiscovered[categoryName]++;
		}

		// Award rewards
		Data.TotalGoldEarned += collectible.GoldReward;
		Data.TotalExpEarned += collectible.ExpReward;

		// Add gold and exp to player
		if (collectible.GoldReward > 0)
		{
			Player.Instance.AddGold(collectible.GoldReward);
		}
		if (collectible.ExpReward > 0)
		{
			Player.Instance.AddExp(collectible.ExpReward);
		}

		// Check if category completed
		int categoryTotal = CollectibleDatabase.Instance.GetCategoryCount(collectible.Category);
		int categoryDiscovered = Data.CategoryDiscovered[categoryName];
		if (categoryDiscovered >= categoryTotal)
		{
			CategoryCompleted.Emit(categoryName);
		}

		// Check if all completed
		if (Data.TotalDiscovered >= CollectibleDatabase.Instance.GetTotalCount())
		{
			AllCollectiblesCompleted.Emit("All");
		}

		GD.Print($"[CollectibleSystem] Discovered: {collectible.Name} (+{collectible.GoldReward} gold, +{collectible.ExpReward} exp)");
		CollectibleDiscovered.Emit(collectibleId);

		// Auto-save
		SaveSystem.Instance.SaveGame();

		return true;
	}

	/// <summary>
	/// 检查收藏品是否已发现
	/// </summary>
	/// <param name="collectibleId">收藏品ID</param>
	/// <returns>是否已发现</returns>
	public bool IsDiscovered(string collectibleId)
	{
		return Data.DiscoveredCollectibles.ContainsKey(collectibleId) && Data.DiscoveredCollectibles[collectibleId];
	}

	/// <summary>
	/// 获取发现进度
	/// </summary>
	/// <param name="category">收藏品分类（可选）</param>
	/// <returns>进度百分比</returns>
	public float GetDiscoveryProgress(CollectibleData.CollectibleCategory? category = null)
	{
		if (category == null)
		{
			// Overall progress
			int total = CollectibleDatabase.Instance.GetTotalCount();
			return total > 0 ? (float)Data.TotalDiscovered / total : 0f;
		}
		else
		{
			// Category progress
			int categoryTotal = CollectibleDatabase.Instance.GetCategoryCount(category.Value);
			string categoryName = category.Value.ToString();
			int categoryDiscovered = Data.CategoryDiscovered.ContainsKey(categoryName) ? Data.CategoryDiscovered[categoryName] : 0;
			return categoryTotal > 0 ? (float)categoryDiscovered / categoryTotal : 0f;
		}
	}

	/// <summary>
	/// 获取已发现数量
	/// </summary>
	/// <param name="category">收藏品分类（可选）</param>
	/// <returns>已发现数量</returns>
	public int GetDiscoveredCount(CollectibleData.CollectibleCategory? category = null)
	{
		if (category == null)
		{
			return Data.TotalDiscovered;
		}
		else
		{
			string categoryName = category.Value.ToString();
			return Data.CategoryDiscovered.ContainsKey(categoryName) ? Data.CategoryDiscovered[categoryName] : 0;
		}
	}

	/// <summary>
	/// 获取总数量
	/// </summary>
	/// <param name="category">收藏品分类（可选）</param>
	/// <returns>总数量</returns>
	public int GetTotalCount(CollectibleData.CollectibleCategory? category = null)
	{
		if (category == null)
		{
			return CollectibleDatabase.Instance.GetTotalCount();
		}
		else
		{
			return CollectibleDatabase.Instance.GetCategoryCount(category.Value);
		}
	}

	/// <summary>
	/// 获取已发现的收藏品列表
	/// </summary>
	/// <param name="category">收藏品分类（可选）</param>
	/// <returns>收藏品列表</returns>
	public List<CollectibleData> GetDiscoveredCollectibles(CollectibleData.CollectibleCategory? category = null)
	{
		var result = new List<CollectibleData>();

		if (category == null)
		{
			// Get all discovered
			foreach (var kvp in Data.DiscoveredCollectibles)
			{
				if (kvp.Value)
				{
					var collectible = CollectibleDatabase.Instance.GetCollectible(kvp.Key);
					if (collectible != null)
					{
						result.Add(collectible);
					}
				}
			}
		}
		else
		{
			// Get by category
			var categoryCollectibles = CollectibleDatabase.Instance.GetByCategory(category.Value);
			foreach (var collectible in categoryCollectibles)
			{
				if (IsDiscovered(collectible.Id))
				{
					result.Add(collectible);
				}
			}
		}

		return result;
	}

	// Convenience methods for common discovery triggers
	public void OnItemCollected(string itemId) => DiscoverCollectible($"item_{itemId}");
	public void OnEquipmentCollected(string equipId) => DiscoverCollectible($"equip_{equipId}");
	public void OnEnemyKilled(string enemyId) => DiscoverCollectible($"enemy_{enemyId}");
	public void OnBossDefeated(string bossId) => DiscoverCollectible($"boss_{bossId}");
	public void OnMountUnlocked(string mountId) => DiscoverCollectible($"mount_{mountId}");
	public void OnPetObtained(string petId) => DiscoverCollectible($"pet_{petId}");
	public void OnRegionEntered(string regionId) => DiscoverCollectible($"region_{regionId}");
	public void OnSkillLearned(string skillId) => DiscoverCollectible($"skill_{skillId}");

	// Save/Load
	public Dictionary<string, object> GetSaveData()
	{
		return new Dictionary<string, object>
		{
			{ "discovered", Data.DiscoveredCollectibles },
			{ "total_discovered", Data.TotalDiscovered },
			{ "total_gold_earned", Data.TotalGoldEarned },
			{ "total_exp_earned", Data.TotalExpEarned },
			{ "category_discovered", Data.CategoryDiscovered }
		};
	}

	public void LoadSaveData(Dictionary<string, object> data)
	{
		if (data == null) return;

		if (data.ContainsKey("discovered"))
		{
			Data.DiscoveredCollectibles = new Dictionary<string, bool>();
			var discovered = data["discovered"] as Dictionary<string, bool>;
			if (discovered != null)
			{
				foreach (var kvp in discovered)
				{
					Data.DiscoveredCollectibles[kvp.Key] = kvp.Value;
				}
			}
		}

		if (data.ContainsKey("total_discovered"))
			Data.TotalDiscovered = Convert.ToInt32(data["total_discovered"]);

		if (data.ContainsKey("total_gold_earned"))
			Data.TotalGoldEarned = Convert.ToInt32(data["total_gold_earned"]);

		if (data.ContainsKey("total_exp_earned"))
			Data.TotalExpEarned = Convert.ToInt32(data["total_exp_earned"]);

		if (data.ContainsKey("category_discovered"))
		{
			Data.CategoryDiscovered = new Dictionary<string, int>();
			var categoryData = data["category_discovered"] as Dictionary<string, int>;
			if (categoryData != null)
			{
				foreach (var kvp in categoryData)
				{
					Data.CategoryDiscovered[kvp.Key] = kvp.Value;
				}
			}
		}

		GD.Print($"[CollectibleSystem] Loaded {Data.TotalDiscovered} discovered collectibles");
	}
}
