using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Collectible data definitions - contains collectible structures, categories and rarity enums
/// </summary>
public class CollectibleData
{
	public enum CollectibleCategory
	{
		Item,
		Equipment,
		Enemy,
		Boss,
		Mount,
		Pet,
		Region,
		Material,
		Skill,
		Achievement
	}

	public enum CollectibleRarity
	{
		Common,
		Uncommon,
		Rare,
		Epic,
		Legendary
	}

	public string Id { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public CollectibleCategory Category { get; set; }
	public CollectibleRarity Rarity { get; set; }
	public int GoldReward { get; set; }
	public int ExpReward { get; set; }
	public string IconPath { get; set; }

	public CollectibleData(string id, string name, string description, CollectibleCategory category, 
		CollectibleRarity rarity, int goldReward = 0, int expReward = 0, string iconPath = "")
	{
		Id = id;
		Name = name;
		Description = description;
		Category = category;
		Rarity = rarity;
		GoldReward = goldReward;
		ExpReward = expReward;
		IconPath = iconPath;
	}
}

public class PlayerCollectibleData
{
	public Dictionary<string, bool> DiscoveredCollectibles { get; set; } = new Dictionary<string, bool>();
	public int TotalDiscovered { get; set; }
	public int TotalGoldEarned { get; set; }
	public int TotalExpEarned { get; set; }
	public Dictionary<string, int> CategoryDiscovered { get; set; } = new Dictionary<string, int>();

	public PlayerCollectibleData()
	{
		// Initialize category counters
		foreach (CollectibleData.CollectibleCategory category in Enum.GetValues(typeof(CollectibleData.CollectibleCategory)))
		{
			CategoryDiscovered[category.ToString()] = 0;
		}
	}
}
