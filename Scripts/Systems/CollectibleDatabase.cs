using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Collectible database - stores all collectible templates and provides lookup functionality
/// </summary>
public class CollectibleDatabase
{
	private static CollectibleDatabase _instance;
	public static CollectibleDatabase Instance => _instance ??= new CollectibleDatabase();

	public Dictionary<string, CollectibleData> AllCollectibles { get; private set; } = new Dictionary<string, CollectibleData>();
	public Dictionary<CollectibleData.CollectibleCategory, List<CollectibleData>> ByCategory { get; private set; } = new Dictionary<CollectibleData.CollectibleCategory, List<CollectibleData>>();
	public Dictionary<CollectibleData.CollectibleRarity, List<CollectibleData>> ByRarity { get; private set; } = new Dictionary<CollectibleData.CollectibleRarity, List<CollectibleData>>();

	private CollectibleDatabase()
	{
		InitializeDatabase();
	}

	private void InitializeDatabase()
	{
		// Initialize category lists
		foreach (CollectibleData.CollectibleCategory category in Enum.GetValues(typeof(CollectibleData.CollectibleCategory)))
		{
			ByCategory[category] = new List<CollectibleData>();
		}

		foreach (CollectibleData.CollectibleRarity rarity in Enum.GetValues(typeof(CollectibleData.CollectibleRarity)))
		{
			ByRarity[rarity] = new List<CollectibleData>();
		}

		// Items (30 collectibles)
		AddCollectible(new CollectibleData("item_potion_health", "Health Potion", "Restores health", CollectibleData.CollectibleCategory.Item, CollectibleData.CollectibleRarity.Common, 10, 5));
		AddCollectible(new CollectibleData("item_potion_mana", "Mana Potion", "Restores mana", CollectibleData.CollectibleCategory.Item, CollectibleData.CollectibleRarity.Common, 10, 5));
		AddCollectible(new CollectibleData("item_potion_speed", "Speed Potion", "Increases movement speed", CollectibleData.CollectibleCategory.Item, CollectibleData.CollectibleRarity.Uncommon, 25, 15));
		AddCollectible(new CollectibleData("item_potion_strength", "Strength Potion", "Increases attack power", CollectibleData.CollectibleCategory.Item, CollectibleData.CollectibleRarity.Rare, 50, 30));
		AddCollectible(new CollectibleData("item_scroll_fire", "Fire Scroll", "Casts fireball", CollectibleData.CollectibleCategory.Item, CollectibleData.CollectibleRarity.Uncommon, 30, 20));
		AddCollectible(new CollectibleData("item_scroll_ice", "Ice Scroll", "Casts frost bolt", CollectibleData.CollectibleCategory.Item, CollectibleData.CollectibleRarity.Uncommon, 30, 20));
		AddCollectible(new CollectibleData("item_material_herb", "Healing Herb", "Crafting material", CollectibleData.CollectibleCategory.Material, CollectibleData.CollectibleRarity.Common, 5, 3));
		AddCollectible(new CollectibleData("item_material_ore", "Iron Ore", "Smithing material", CollectibleData.CollectibleCategory.Material, CollectibleData.CollectibleRarity.Common, 5, 3));
		AddCollectible(new CollectibleData("item_material_crystal", "Magic Crystal", "Enchantment material", CollectibleData.CollectibleCategory.Material, CollectibleData.CollectibleRarity.Uncommon, 25, 15));
		AddCollectible(new CollectibleData("item_material_dragon_scale", "Dragon Scale", "Rare crafting material", CollectibleData.CollectibleCategory.Material, CollectibleData.CollectibleRarity.Epic, 100, 50));
		AddCollectible(new CollectibleData("item_material_phoenix_feather", "Phoenix Feather", "Legendary material", CollectibleData.CollectibleCategory.Material, CollectibleData.CollectibleRarity.Legendary, 200, 100));

		// Equipment (20 collectibles)
		AddCollectible(new CollectibleData("equip_sword_iron", "Iron Sword", "Basic weapon", CollectibleData.CollectibleCategory.Equipment, CollectibleData.CollectibleRarity.Common, 20, 10));
		AddCollectible(new CollectibleData("equip_sword_flame", "Flame Sword", "Fire enchanted weapon", CollectibleData.CollectibleCategory.Equipment, CollectibleData.CollectibleRarity.Rare, 80, 40));
		AddCollectible(new CollectibleData("equip_sword_legend", "Excalibur", "Legendary sword", CollectibleData.CollectibleCategory.Equipment, CollectibleData.CollectibleRarity.Legendary, 500, 250));
		AddCollectible(new CollectibleData("equip_armor_leather", "Leather Armor", "Basic armor", CollectibleData.CollectibleCategory.Equipment, CollectibleData.CollectibleRarity.Common, 20, 10));
		AddCollectible(new CollectibleData("equip_armor_dragon", "Dragon Scale Armor", "Epic armor", CollectibleData.CollectibleCategory.Equipment, CollectibleData.CollectibleRarity.Epic, 150, 75));
		AddCollectible(new CollectibleData("equip_shield_holy", "Holy Shield", "Legendary shield", CollectibleData.CollectibleCategory.Equipment, CollectibleData.CollectibleRarity.Legendary, 400, 200));

		// Enemies (25 collectibles)
		AddCollectible(new CollectibleData("enemy_goblin", "Goblin", "Weak but numerous", CollectibleData.CollectibleCategory.Enemy, CollectibleData.CollectibleRarity.Common, 5, 3));
		AddCollectible(new CollectibleData("enemy_skeleton", "Skeleton", "Undead warrior", CollectibleData.CollectibleCategory.Enemy, CollectibleData.CollectibleRarity.Common, 8, 5));
		AddCollectible(new CollectibleData("enemy_orc", "Orc", "Fierce warrior", CollectibleData.CollectibleCategory.Enemy, CollectibleData.CollectibleRarity.Uncommon, 20, 12));
		AddCollectible(new CollectibleData("enemy_troll", "Troll", "Regenerating brute", CollectibleData.CollectibleCategory.Enemy, CollectibleData.CollectibleRarity.Rare, 40, 25));
		AddCollectible(new CollectibleData("enemy_wraith", "Wraith", "Ghostly assassin", CollectibleData.CollectibleCategory.Enemy, CollectibleData.CollectibleRarity.Rare, 50, 30));
		AddCollectible(new CollectibleData("enemy_elemental_fire", "Fire Elemental", "Living flame", CollectibleData.CollectibleCategory.Enemy, CollectibleData.CollectibleRarity.Rare, 45, 28));
		AddCollectible(new CollectibleData("enemy_elemental_ice", "Ice Elemental", "Frozen guardian", CollectibleData.CollectibleCategory.Enemy, CollectibleData.CollectibleRarity.Rare, 45, 28));
		AddCollectible(new CollectibleData("enemy_dark_knight", "Dark Knight", "Corrupted warrior", CollectibleData.CollectibleCategory.Enemy, CollectibleData.CollectibleRarity.Epic, 80, 50));
		AddCollectible(new CollectibleData("enemy_vampire", "Vampire", "Bloodsucking noble", CollectibleData.CollectibleCategory.Enemy, CollectibleData.CollectibleRarity.Epic, 90, 55));

		// Bosses (15 collectibles)
		AddCollectible(new CollectibleData("boss_goblin_king", "Goblin King", "Leader of the goblins", CollectibleData.CollectibleCategory.Boss, CollectibleData.CollectibleRarity.Rare, 100, 60));
		AddCollectible(new CollectibleData("boss_skeleton_lord", "Skeleton Lord", "Undead monarch", CollectibleData.CollectibleCategory.Boss, CollectibleData.CollectibleRarity.Rare, 120, 70));
		AddCollectible(new CollectibleData("boss_troll_chief", "Troll Chief", "Alpha of the trolls", CollectibleData.CollectibleCategory.Boss, CollectibleData.CollectibleRarity.Rare, 150, 80));
		AddCollectible(new CollectibleData("boss_wraith_king", "Wraith King", "Ghostly sovereign", CollectibleData.CollectibleCategory.Boss, CollectibleData.CollectibleRarity.Epic, 200, 100));
		AddCollectible(new CollectibleData("boss_dragon_red", "Red Dragon", "Ancient fire drake", CollectibleData.CollectibleCategory.Boss, CollectibleData.CollectibleRarity.Epic, 250, 125));
		AddCollectible(new CollectibleData("boss_dragon_blue", "Blue Dragon", "Lightning wyrm", CollectibleData.CollectibleCategory.Boss, CollectibleData.CollectibleRarity.Epic, 250, 125));
		AddCollectible(new CollectibleData("boss_lich", "Lich", "Undead mage lord", CollectibleData.CollectibleCategory.Boss, CollectibleData.CollectibleRarity.Epic, 300, 150));
		AddCollectible(new CollectibleData("boss_demon_lord", "Demon Lord", "Ruler of the abyss", CollectibleData.CollectibleCategory.Boss, CollectibleData.CollectibleRarity.Legendary, 500, 250));
		AddCollectible(new CollectibleData("boss_ancient_dragon", "Ancient Dragon", "Primordial beast", CollectibleData.CollectibleCategory.Boss, CollectibleData.CollectibleRarity.Legendary, 800, 400));

		// Mounts (15 collectibles)
		AddCollectible(new CollectibleData("mount_horse", "Horse", "Basic mount", CollectibleData.CollectibleCategory.Mount, CollectibleData.CollectibleRarity.Common, 30, 15));
		AddCollectible(new CollectibleData("mount_unicorn", "Unicorn", "Magical horse", CollectibleData.CollectibleCategory.Mount, CollectibleData.CollectibleRarity.Rare, 100, 50));
		AddCollectible(new CollectibleData("mount_pegasus", "Pegasus", "Winged horse", CollectibleData.CollectibleCategory.Mount, CollectibleData.CollectibleRarity.Epic, 200, 100));
		AddCollectible(new CollectibleData("mount_griffon", "Griffon", "Lion-eagle hybrid", CollectibleData.CollectibleCategory.Mount, CollectibleData.CollectibleRarity.Epic, 180, 90));
		AddCollectible(new CollectibleData("mount_phoenix", "Phoenix", "Fire bird mount", CollectibleData.CollectibleCategory.Mount, CollectibleData.CollectibleRarity.Legendary, 400, 200));
		AddCollectible(new CollectibleData("mount_dragon", "Dragon", "Ultimate mount", CollectibleData.CollectibleCategory.Mount, CollectibleData.CollectibleRarity.Legendary, 500, 250));

		// Pets (15 collectibles)
		AddCollectible(new CollectibleData("pet_cat", "Cat", "Cute companion", CollectibleData.CollectibleCategory.Pet, CollectibleData.CollectibleRarity.Common, 20, 10));
		AddCollectible(new CollectibleData("pet_dog", "Dog", "Loyal friend", CollectibleData.CollectibleCategory.Pet, CollectibleData.CollectibleRarity.Common, 20, 10));
		AddCollectible(new CollectibleData("pet_owl", "Owl", "Wise bird", CollectibleData.CollectibleCategory.Pet, CollectibleData.CollectibleRarity.Uncommon, 40, 20));
		AddCollectible(new CollectibleData("pet_wolf", "Wolf", "Fierce companion", CollectibleData.CollectibleCategory.Pet, CollectibleData.CollectibleRarity.Rare, 80, 40));
		AddCollectible(new CollectibleData("pet_fenix", "Phoenix", "Fire bird pet", CollectibleData.CollectibleCategory.Pet, CollectibleData.CollectibleRarity.Epic, 150, 75));
		AddCollectible(new CollectibleData("pet_dragon", "Dragon", "Legendary companion", CollectibleData.CollectibleCategory.Pet, CollectibleData.CollectibleRarity.Legendary, 300, 150));

		// Regions (10 collectibles)
		AddCollectible(new CollectibleData("region_plains", "Green Plains", "Peaceful starting area", CollectibleData.CollectibleCategory.Region, CollectibleData.CollectibleRarity.Common, 15, 8));
		AddCollectible(new CollectibleData("region_forest", "Dark Forest", "Mysterious woods", CollectibleData.CollectibleCategory.Region, CollectibleData.CollectibleRarity.Uncommon, 30, 15));
		AddCollectible(new CollectibleData("region_mountain", "Rocky Mountains", "Dangerous peaks", CollectibleData.CollectibleCategory.Region, CollectibleData.CollectibleRarity.Uncommon, 35, 18));
		AddCollectible(new CollectibleData("region_swamp", "Cursed Swamp", "Toxic marshlands", CollectibleData.CollectibleCategory.Region, CollectibleData.CollectibleRarity.Rare, 50, 25));
		AddCollectible(new CollectibleData("region_volcano", "Volcanic Realm", "Molten mountains", CollectibleData.CollectibleCategory.Region, CollectibleData.CollectibleRarity.Epic, 100, 50));
		AddCollectible(new CollectibleData("region_ice", "Frozen Wasteland", "Endless ice", CollectibleData.CollectibleCategory.Region, CollectibleData.CollectibleRarity.Epic, 100, 50));
		AddCollectible(new CollectibleData("region_shadow", "Shadow Realm", "Dark dimension", CollectibleData.CollectibleCategory.Region, CollectibleData.CollectibleRarity.Legendary, 200, 100));

		// Skills (15 collectibles)
		AddCollectible(new CollectibleData("skill_fireball", "Fireball", "Fire magic skill", CollectibleData.CollectibleCategory.Skill, CollectibleData.CollectibleRarity.Uncommon, 40, 20));
		AddCollectible(new CollectibleData("skill_ice_lance", "Ice Lance", "Ice magic skill", CollectibleData.CollectibleCategory.Skill, CollectibleData.CollectibleRarity.Uncommon, 40, 20));
		AddCollectible(new CollectibleData("skill_thunder", "Thunder Strike", "Lightning skill", CollectibleData.CollectibleCategory.Skill, CollectibleData.CollectibleRarity.Rare, 60, 30));
		AddCollectible(new CollectibleData("skill_heal", "Holy Heal", "Healing skill", CollectibleData.CollectibleCategory.Skill, CollectibleData.CollectibleRarity.Rare, 60, 30));
		AddCollectible(new CollectibleData("skill_meteor", "Meteor Strike", "Ultimate fire skill", CollectibleData.CollectibleCategory.Skill, CollectibleData.CollectibleRarity.Epic, 120, 60));
		AddCollectible(new CollectibleData("skill_ultimate", "Divine Wrath", "Legendary skill", CollectibleData.CollectibleCategory.Skill, CollectibleData.CollectibleRarity.Legendary, 250, 125));

		GD.Print($"[CollectibleDatabase] Initialized {AllCollectibles.Count} collectibles");
	}

	private void AddCollectible(CollectibleData collectible)
	{
		AllCollectibles[collectible.Id] = collectible;
		ByCategory[collectible.Category].Add(collectible);
		ByRarity[collectible.Rarity].Add(collectible);
	}

	public CollectibleData GetCollectible(string id)
	{
		return AllCollectibles.ContainsKey(id) ? AllCollectibles[id] : null;
	}

	public List<CollectibleData> GetByCategory(CollectibleData.CollectibleCategory category)
	{
		return ByCategory.ContainsKey(category) ? ByCategory[category] : new List<CollectibleData>();
	}

	public List<CollectibleData> GetByRarity(CollectibleData.CollectibleRarity rarity)
	{
		return ByRarity.ContainsKey(rarity) ? ByRarity[rarity] : new List<CollectibleData>();
	}

	public int GetTotalCount() => AllCollectibles.Count;

	public int GetCategoryCount(CollectibleData.CollectibleCategory category)
	{
		return ByCategory.ContainsKey(category) ? ByCategory[category].Count : 0;
	}
}
