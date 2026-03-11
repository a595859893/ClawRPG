using Godot;
using System;
using System.Collections.Generic;

public class PetEvolutionDatabase
{
	private static PetEvolutionDatabase _instance;
	public static PetEvolutionDatabase Instance
	{
		get
		{
			if (_instance == null)
				_instance = new PetEvolutionDatabase();
			return _instance;
		}
	}
	
	// Pet evolution configs: base pet -> stage -> type
	private Dictionary<string, Dictionary<PetEvolutionData.EvolutionStage, Dictionary<PetEvolutionData.EvolutionType, PetEvolutionData.PetEvolutionConfig>>> _evolutionConfigs;
	
	// Evolution item definitions
	private Dictionary<string, Dictionary<PetEvolutionData.EvolutionType, string>> _evolutionItems;
	
	public PetEvolutionDatabase()
	{
		_evolutionConfigs = new Dictionary<string, Dictionary<PetEvolutionData.EvolutionStage, Dictionary<PetEvolutionData.EvolutionType, PetEvolutionData.PetEvolutionConfig>>>();
		_evolutionItems = new Dictionary<string, Dictionary<PetEvolutionData.EvolutionType, string>>();
		InitializeEvolutionConfigs();
		InitializeEvolutionItems();
	}
	
	private void InitializeEvolutionItems()
	{
		// Evolution items for each type
		_evolutionItems["fire_stone"] = new Dictionary<PetEvolutionData.EvolutionType, string>
		{
			{ PetEvolutionData.EvolutionType.Fire, "fire_evolution_stone" }
		};
		
		_evolutionItems["ice_stone"] = new Dictionary<PetEvolutionData.EvolutionType, string>
		{
			{ PetEvolutionData.EvolutionType.Ice, "ice_evolution_stone" }
		};
		
		_evolutionItems["lightning_stone"] = new Dictionary<PetEvolutionData.EvolutionType, string>
		{
			{ PetEvolutionData.EvolutionType.Lightning, "lightning_evolution_stone" }
		};
		
		_evolutionItems["dark_stone"] = new Dictionary<PetEvolutionData.EvolutionType, string>
		{
			{ PetEvolutionData.EvolutionType.Dark, "dark_evolution_stone" }
		};
		
		_evolutionItems["holy_stone"] = new Dictionary<PetEvolutionData.EvolutionType, string>
		{
			{ PetEvolutionData.EvolutionType.Holy, "holy_evolution_stone" }
		};
		
		_evolutionItems["nature_stone"] = new Dictionary<PetEvolutionData.EvolutionType, string>
		{
			{ PetEvolutionData.EvolutionType.Nature, "nature_evolution_stone" }
		};
	}
	
	private void InitializeEvolutionConfigs()
	{
		// Wolf evolution chain
		AddWolfEvolutions();
		
		// Bear evolution chain
		AddBearEvolutions();
		
		// Eagle evolution chain
		AddEagleEvolutions();
		
		// Fox evolution chain
		AddFoxEvolutions();
		
		// Dragon evolution chain (special - starts at Elite)
		AddDragonEvolutions();
	}
	
	private void AddWolfEvolutions()
	{
		string basePetId = "wolf";
		
		// Basic -> Advanced
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Basic, PetEvolutionData.EvolutionType.Nature,
			"Wild Wolf", "A wild wolf companion", 50, 30, 200, 80, 0.05f, 1.5f, 0.0f,
			PetEvolutionData.EvolutionStage.Advanced, 100, 20, "nature_evolution_stone", 1, null,
			10, 5, 30, 5, 0.02f, 0.1f, 0.0f, new List<string>());
		
		// Advanced -> Elite
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Advanced, PetEvolutionData.EvolutionType.Nature,
			"Dire Wolf", "A powerful dire wolf", 70, 45, 300, 100, 0.08f, 1.6f, 0.02f,
			PetEvolutionData.EvolutionStage.Elite, 300, 50, "nature_evolution_stone", 3, null,
			15, 8, 50, 10, 0.03f, 0.15f, 0.02f, new List<string> { "howl" });
		
		// Elite -> Epic
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Elite, PetEvolutionData.EvolutionType.Nature,
			"Spectral Wolf", "A wolf with ethereal powers", 100, 65, 450, 130, 0.12f, 1.8f, 0.04f,
			PetEvolutionData.EvolutionStage.Epic, 800, 100, "nature_evolution_stone", 5, null,
			20, 12, 80, 15, 0.05f, 0.2f, 0.03f, new List<string> { "howl", "phase_shift" });
		
		// Elite -> Epic (Dark type)
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Elite, PetEvolutionData.EvolutionType.Dark,
			"Shadow Wolf", "A wolf of pure darkness", 110, 70, 420, 140, 0.14f, 1.9f, 0.03f,
			PetEvolutionData.EvolutionStage.Epic, 800, 100, "dark_evolution_stone", 5, PetEvolutionData.EvolutionType.Dark,
			22, 14, 70, 18, 0.06f, 0.22f, 0.02f, new List<string> { "dark_howl", "shadow_step" });
		
		// Epic -> Legendary
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Epic, PetEvolutionData.EvolutionType.Nature,
			"Alpha Wolf", "The leader of all wolves", 150, 100, 650, 180, 0.18f, 2.2f, 0.08f,
			PetEvolutionData.EvolutionStage.Legendary, 2000, 200, "nature_evolution_stone", 10, null,
			30, 20, 120, 25, 0.08f, 0.3f, 0.05f, new List<string> { "howl", "pack_leader", "nature_blessing" });
		
		// Epic -> Legendary (Dark)
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Epic, PetEvolutionData.EvolutionType.Dark,
			"Nightmare Wolf", "A wolf born from nightmares", 160, 110, 600, 190, 0.20f, 2.4f, 0.06f,
			PetEvolutionData.EvolutionStage.Legendary, 2000, 200, "dark_evolution_stone", 10, PetEvolutionData.EvolutionType.Dark,
			35, 22, 100, 30, 0.10f, 0.35f, 0.04f, new List<string> { "dark_howl", "nightmare_ride", "void_embrace" });
	}
	
	private void AddBearEvolutions()
	{
		string basePetId = "bear";
		
		// Basic -> Advanced
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Basic, PetEvolutionData.EvolutionType.Nature,
			"Brown Bear", "A sturdy bear companion", 80, 60, 400, 50, 0.03f, 1.4f, 0.0f,
			PetEvolutionData.EvolutionStage.Advanced, 150, 25, "nature_evolution_stone", 1, null,
			15, 12, 60, 8, 0.02f, 0.1f, 0.0f, new List<string>());
		
		// Advanced -> Elite
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Advanced, PetEvolutionData.EvolutionType.Nature,
			"Grizzly Bear", "A powerful grizzly bear", 110, 85, 550, 70, 0.05f, 1.5f, 0.03f,
			PetEvolutionData.EvolutionStage.Elite, 400, 60, "nature_evolution_stone", 3, null,
			20, 18, 90, 12, 0.03f, 0.15f, 0.02f, new List<string> { "maul" });
		
		// Elite -> Epic
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Elite, PetEvolutionData.EvolutionType.Nature,
			"Cave Bear", "A massive cave dwelling bear", 150, 120, 750, 95, 0.08f, 1.7f, 0.05f,
			PetEvolutionData.EvolutionStage.Epic, 1000, 120, "nature_evolution_stone", 5, null,
			28, 25, 130, 18, 0.05f, 0.2f, 0.03f, new List<string> { "maul", "roar" });
		
		// Elite -> Epic (Fire)
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Elite, PetEvolutionData.EvolutionType.Fire,
			"Magma Bear", "A bear born from volcanic fire", 165, 110, 700, 100, 0.10f, 1.8f, 0.04f,
			PetEvolutionData.EvolutionStage.Epic, 1000, 120, "fire_evolution_stone", 5, PetEvolutionData.EvolutionType.Fire,
			30, 22, 110, 20, 0.06f, 0.22f, 0.03f, new List<string> { "flame_maul", "magma_skin" });
		
		// Epic -> Legendary
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Epic, PetEvolutionData.EvolutionType.Nature,
			"Ancient Bear", "An ancient guardian of the forest", 220, 170, 1000, 130, 0.12f, 2.0f, 0.10f,
			PetEvolutionData.EvolutionStage.Legendary, 2500, 250, "nature_evolution_stone", 10, null,
			40, 35, 180, 28, 0.08f, 0.3f, 0.06f, new List<string> { "maul", "ancient_roar", "forest_guardian" });
		
		// Epic -> Legendary (Fire)
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Epic, PetEvolutionData.EvolutionType.Fire,
			"Volcanic Bear", "A bear forged in volcanic flames", 240, 160, 950, 140, 0.14f, 2.2f, 0.08f,
			PetEvolutionData.EvolutionStage.Legendary, 2500, 250, "fire_evolution_stone", 10, PetEvolutionData.EvolutionType.Fire,
			45, 30, 150, 32, 0.10f, 0.35f, 0.05f, new List<string> { "flame_maul", "eruption", "lava_body" });
	}
	
	private void AddEagleEvolutions()
	{
		string basePetId = "eagle";
		
		// Basic -> Advanced
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Basic, PetEvolutionData.EvolutionType.Nature,
			"Golden Eagle", "A majestic eagle", 40, 25, 150, 120, 0.08f, 1.6f, 0.0f,
			PetEvolutionData.EvolutionStage.Advanced, 100, 20, "nature_evolution_stone", 1, null,
			8, 5, 25, 15, 0.02f, 0.1f, 0.0f, new List<string>());
		
		// Advanced -> Elite
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Advanced, PetEvolutionData.EvolutionType.Nature,
			"Storm Eagle", "A eagle that controls storms", 55, 35, 220, 160, 0.12f, 1.8f, 0.02f,
			PetEvolutionData.EvolutionStage.Elite, 300, 50, "nature_evolution_stone", 3, null,
			12, 8, 40, 22, 0.03f, 0.15f, 0.02f, new List<string> { "swoop" });
		
		// Advanced -> Elite (Lightning)
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Advanced, PetEvolutionData.EvolutionType.Lightning,
			"Thunder Eagle", "A eagle made of lightning", 60, 32, 200, 175, 0.14f, 1.9f, 0.01f,
			PetEvolutionData.EvolutionStage.Elite, 300, 50, "lightning_evolution_stone", 3, PetEvolutionData.EvolutionType.Lightning,
			14, 7, 35, 25, 0.04f, 0.18f, 0.01f, new List<string> { "thunder_swoop", "static_discharge" });
		
		// Elite -> Epic
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Elite, PetEvolutionData.EvolutionType.Nature,
			"Sky Sovereign", "Ruler of the skies", 80, 50, 320, 210, 0.16f, 2.0f, 0.04f,
			PetEvolutionData.EvolutionStage.Epic, 800, 100, "nature_evolution_stone", 5, null,
			18, 12, 60, 30, 0.05f, 0.22f, 0.03f, new List<string> { "swoop", "wind_blade" });
		
		// Elite -> Epic (Lightning)
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Elite, PetEvolutionData.EvolutionType.Lightning,
			"Tempest Lord", "Master of lightning and storm", 90, 48, 290, 230, 0.18f, 2.2f, 0.03f,
			PetEvolutionData.EvolutionStage.Epic, 800, 100, "lightning_evolution_stone", 5, PetEvolutionData.EvolutionType.Lightning,
			20, 10, 50, 35, 0.06f, 0.25f, 0.02f, new List<string> { "thunder_swoop", "lightning_strike", "storm_call" });
		
		// Epic -> Legendary
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Epic, PetEvolutionData.EvolutionType.Nature,
			"Celestial Eagle", "An eagle descended from the heavens", 120, 75, 450, 280, 0.22f, 2.5f, 0.06f,
			PetEvolutionData.EvolutionStage.Legendary, 2000, 200, "nature_evolution_stone", 10, null,
			28, 18, 90, 45, 0.08f, 0.32f, 0.04f, new List<string> { "swoop", "wind_blade", "divine_wings" });
		
		// Epic -> Legendary (Lightning)
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Epic, PetEvolutionData.EvolutionType.Lightning,
			"Thunderlord Eagle", "The supreme lightning predator", 135, 70, 400, 300, 0.25f, 2.8f, 0.05f,
			PetEvolutionData.EvolutionStage.Legendary, 2000, 200, "lightning_evolution_stone", 10, PetEvolutionData.EvolutionType.Lightning,
			32, 15, 75, 50, 0.10f, 0.38f, 0.03f, new List<string> { "thunder_swoop", "lightning_strike", "thunderlord_blessing" });
	}
	
	private void AddFoxEvolutions()
	{
		string basePetId = "fox";
		
		// Basic -> Advanced
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Basic, PetEvolutionData.EvolutionType.Nature,
			"Red Fox", "A clever fox companion", 35, 20, 120, 100, 0.10f, 1.7f, 0.0f,
			PetEvolutionData.EvolutionStage.Advanced, 80, 15, "nature_evolution_stone", 1, null,
			7, 4, 20, 12, 0.02f, 0.1f, 0.0f, new List<string>());
		
		// Advanced -> Elite
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Advanced, PetEvolutionData.EvolutionType.Nature,
			"Arctic Fox", "A fox of the frozen lands", 48, 28, 180, 130, 0.14f, 1.9f, 0.02f,
			PetEvolutionData.EvolutionStage.Elite, 250, 40, "ice_evolution_stone", 3, null,
			10, 6, 30, 18, 0.03f, 0.15f, 0.02f, new List<string> { "ice_shard" });
		
		// Advanced -> Elite (Dark)
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Advanced, PetEvolutionData.EvolutionType.Dark,
			"Shadow Fox", "A fox of pure shadow", 52, 25, 160, 140, 0.16f, 2.0f, 0.01f,
			PetEvolutionData.EvolutionStage.Elite, 250, 40, "dark_evolution_stone", 3, PetEvolutionData.EvolutionType.Dark,
			11, 5, 25, 20, 0.04f, 0.18f, 0.01f, new List<string> { "shadow_strike", "night_fade" });
		
		// Elite -> Epic
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Elite, PetEvolutionData.EvolutionType.Ice,
			"Frost Phantom", "A phantom of eternal winter", 70, 42, 280, 175, 0.20f, 2.2f, 0.04f,
			PetEvolutionData.EvolutionStage.Epic, 700, 90, "ice_evolution_stone", 5, PetEvolutionData.EvolutionType.Ice,
			15, 9, 50, 25, 0.05f, 0.22f, 0.03f, new List<string> { "ice_shard", "frost_nova", "winter_cocoon" });
		
		// Elite -> Epic (Dark)
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Elite, PetEvolutionData.EvolutionType.Dark,
			"Void Walker", "A fox that walks between dimensions", 78, 38, 240, 190, 0.22f, 2.4f, 0.03f,
			PetEvolutionData.EvolutionStage.Epic, 700, 90, "dark_evolution_stone", 5, PetEvolutionData.EvolutionType.Dark,
			17, 8, 42, 28, 0.06f, 0.25f, 0.02f, new List<string> { "shadow_strike", "void_portal", "dimensional_shift" });
		
		// Epic -> Legendary (Ice)
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Epic, PetEvolutionData.EvolutionType.Ice,
			"Winter Spirit", "The spirit of eternal winter", 105, 65, 400, 230, 0.28f, 2.7f, 0.06f,
			PetEvolutionData.EvolutionStage.Legendary, 1800, 180, "ice_evolution_stone", 10, PetEvolutionData.EvolutionType.Ice,
			23, 14, 75, 38, 0.08f, 0.32f, 0.04f, new List<string> { "ice_shard", "frost_nova", "blizzard_summon", "absolute_zero" });
		
		// Epic -> Legendary (Dark)
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Epic, PetEvolutionData.EvolutionType.Dark,
			"Abyss Fox", "A fox from the void itself", 118, 58, 350, 250, 0.30f, 2.9f, 0.05f,
			PetEvolutionData.EvolutionStage.Legendary, 1800, 180, "dark_evolution_stone", 10, PetEvolutionData.EvolutionType.Dark,
			26, 12, 60, 42, 0.10f, 0.38f, 0.03f, new List<string> { "shadow_strike", "void_portal", "abyss_touch", "void_annihilation" });
	}
	
	private void AddDragonEvolutions()
	{
		string basePetId = "dragon";
		
		// Elite -> Epic (start directly at Elite for dragons)
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Elite, PetEvolutionData.EvolutionType.Fire,
			"Young Fire Dragon", "A young dragon of flame", 180, 140, 800, 120, 0.10f, 1.8f, 0.05f,
			PetEvolutionData.EvolutionStage.Epic, 1200, 150, "fire_evolution_stone", 5, PetEvolutionData.EvolutionType.Fire,
			30, 25, 120, 20, 0.05f, 0.2f, 0.04f, new List<string> { "fire_breath" });
		
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Elite, PetEvolutionData.EvolutionType.Ice,
			"Young Ice Dragon", "A young dragon of frost", 160, 150, 850, 110, 0.08f, 1.7f, 0.06f,
			PetEvolutionData.EvolutionStage.Epic, 1200, 150, "ice_evolution_stone", 5, PetEvolutionData.EvolutionType.Ice,
			28, 28, 130, 18, 0.04f, 0.18f, 0.05f, new List<string> { "ice_breath" });
		
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Elite, PetEvolutionData.EvolutionType.Lightning,
			"Young Storm Dragon", "A young dragon of storms", 170, 120, 700, 150, 0.15f, 2.0f, 0.03f,
			PetEvolutionData.EvolutionStage.Epic, 1200, 150, "lightning_evolution_stone", 5, PetEvolutionData.EvolutionType.Lightning,
			32, 22, 100, 25, 0.06f, 0.25f, 0.02f, new List<string> { "lightning_breath" });
		
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Elite, PetEvolutionData.EvolutionType.Dark,
			"Young Shadow Dragon", "A young dragon of darkness", 185, 130, 720, 130, 0.12f, 1.9f, 0.04f,
			PetEvolutionData.EvolutionStage.Epic, 1200, 150, "dark_evolution_stone", 5, PetEvolutionData.EvolutionType.Dark,
			35, 24, 105, 22, 0.05f, 0.22f, 0.03f, new List<string> { "shadow_breath" });
		
		// Epic -> Legendary
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Epic, PetEvolutionData.EvolutionType.Fire,
			"Elder Fire Dragon", "An ancient dragon of flame", 260, 200, 1100, 160, 0.15f, 2.2f, 0.08f,
			PetEvolutionData.EvolutionStage.Legendary, 3000, 300, "fire_evolution_stone", 10, PetEvolutionData.EvolutionType.Fire,
			45, 35, 170, 28, 0.08f, 0.3f, 0.06f, new List<string> { "fire_breath", "inferno", "fire_aura" });
		
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Epic, PetEvolutionData.EvolutionType.Ice,
			"Elder Ice Dragon", "An ancient dragon of frost", 230, 220, 1200, 150, 0.12f, 2.0f, 0.10f,
			PetEvolutionData.EvolutionStage.Legendary, 3000, 300, "ice_evolution_stone", 10, PetEvolutionData.EvolutionType.Ice,
			40, 40, 190, 25, 0.06f, 0.25f, 0.08f, new List<string> { "ice_breath", "blizzard", "frost_aura" });
		
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Epic, PetEvolutionData.EvolutionType.Lightning,
			"Elder Storm Dragon", "An ancient dragon of storms", 250, 175, 950, 200, 0.20f, 2.5f, 0.05f,
			PetEvolutionData.EvolutionStage.Legendary, 3000, 300, "lightning_evolution_stone", 10, PetEvolutionData.EvolutionType.Lightning,
			48, 32, 140, 35, 0.10f, 0.35f, 0.04f, new List<string> { "lightning_breath", "thunderstorm", "storm_aura" });
		
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Epic, PetEvolutionData.EvolutionType.Dark,
			"Elder Shadow Dragon", "An ancient dragon of darkness", 270, 190, 980, 175, 0.18f, 2.3f, 0.06f,
			PetEvolutionData.EvolutionStage.Legendary, 3000, 300, "dark_evolution_stone", 10, PetEvolutionData.EvolutionType.Dark,
			52, 34, 145, 30, 0.08f, 0.32f, 0.05f, new List<string> { "shadow_breath", "dark_void", "shadow_aura" });
		
		// Legendary -> Mythical (special)
		AddEvolutionConfig(basePetId, PetEvolutionData.EvolutionStage.Legendary, PetEvolutionData.EvolutionType.Holy,
			"Divine Dragon", "A dragon ascended to divinity", 350, 280, 1500, 220, 0.22f, 2.8f, 0.12f,
			PetEvolutionData.EvolutionStage.Legendary, 5000, 500, "holy_evolution_stone", 10, PetEvolutionData.EvolutionType.Holy,
			60, 48, 230, 40, 0.12f, 0.4f, 0.08f, new List<string> { "divine_breath", "heavenly_light", "divine_aura", "resurrection" });
	}
	
	private void AddEvolutionConfig(
		string petId,
		PetEvolutionData.EvolutionStage stage,
		PetEvolutionData.EvolutionType type,
		string displayName,
		string description,
		int baseAttack, int baseDefense, int baseHealth, int baseSpeed,
		float baseCritRate, float baseCritDamage, float baseLifesteal,
		PetEvolutionData.EvolutionStage requiredStage,
		int requiredBattleExp, int requiredKills,
		string requiredItemId, int requiredItemCount,
		PetEvolutionData.EvolutionType? requiredType,
		int attackBonus, int defenseBonus, int healthBonus, int speedBonus,
		float critRateBonus, float critDamageBonus, float lifestealBonus,
		List<string> unlockedSkills)
	{
		var config = new PetEvolutionData.PetEvolutionConfig
		{
			PetId = petId + "_" + stage.ToString().ToLower() + "_" + type.ToString().ToLower(),
			Stage = stage,
			Type = type,
			DisplayName = displayName,
			Description = description,
			BaseAttack = baseAttack,
			BaseDefense = baseDefense,
			BaseHealth = baseHealth,
			BaseSpeed = baseSpeed,
			BaseCritRate = baseCritRate,
			BaseCritDamage = baseCritDamage,
			BaseLifesteal = baseLifesteal,
			Requirement = new PetEvolutionData.EvolutionRequirement
			{
				RequiredStage = requiredStage,
				RequiredBattleExp = requiredBattleExp,
				RequiredKills = requiredKills,
				RequiredItemId = requiredItemId,
				RequiredItemCount = requiredItemCount,
				RequiredType = requiredType
			},
			Reward = new PetEvolutionData.EvolutionReward
			{
				AttackBonus = attackBonus,
				DefenseBonus = defenseBonus,
				HealthBonus = healthBonus,
				SpeedBonus = speedBonus,
				CritRateBonus = critRateBonus,
				CritDamageBonus = critDamageBonus,
				LifestealBonus = lifestealBonus,
				UnlockedSkills = unlockedSkills
			}
		};
		
		if (!_evolutionConfigs.ContainsKey(petId))
		{
			_evolutionConfigs[petId] = new Dictionary<PetEvolutionData.EvolutionStage, Dictionary<PetEvolutionData.EvolutionType, PetEvolutionData.PetEvolutionConfig>>();
		}
		
		if (!_evolutionConfigs[petId].ContainsKey(stage))
		{
			_evolutionConfigs[petId][stage] = new Dictionary<PetEvolutionData.EvolutionType, PetEvolutionData.PetEvolutionConfig>();
		}
		
		_evolutionConfigs[petId][stage][type] = config;
	}
	
	// Get evolution config
	public PetEvolutionData.PetEvolutionConfig GetEvolutionConfig(string petId, PetEvolutionData.EvolutionStage stage, PetEvolutionData.EvolutionType type)
	{
		if (_evolutionConfigs.ContainsKey(petId) &&
			_evolutionConfigs[petId].ContainsKey(stage) &&
			_evolutionConfigs[petId][stage].ContainsKey(type))
		{
			return _evolutionConfigs[petId][stage][type];
		}
		return null;
	}
	
	// Get all available evolution configs for a pet
	public List<PetEvolutionData.PetEvolutionConfig> GetAvailableEvolutions(string petId, PetEvolutionData.EvolutionStage currentStage, PetEvolutionData.EvolutionType currentType)
	{
		List<PetEvolutionData.PetEvolutionConfig> results = new List<PetEvolutionData.PetEvolutionConfig>();
		
		// Find next stage
		PetEvolutionData.EvolutionStage nextStage = GetNextStage(currentStage);
		if (nextStage == currentStage) return results; // Already max stage
		
		if (_evolutionConfigs.ContainsKey(petId) && _evolutionConfigs[petId].ContainsKey(nextStage))
		{
			foreach (var kvp in _evolutionConfigs[petId][nextStage])
			{
				var config = kvp.Value;
				// Check if this evolution path is available (some require specific type)
				if (config.Requirement.RequiredType == null || config.Requirement.RequiredType == currentType)
				{
					results.Add(config);
				}
			}
		}
		
		return results;
	}
	
	// Get next stage
	public PetEvolutionData.EvolutionStage GetNextStage(PetEvolutionData.EvolutionStage currentStage)
	{
		switch (currentStage)
		{
			case PetEvolutionData.EvolutionStage.Basic: return PetEvolutionData.EvolutionStage.Advanced;
			case PetEvolutionData.EvolutionStage.Advanced: return PetEvolutionData.EvolutionStage.Elite;
			case PetEvolutionData.EvolutionStage.Elite: return PetEvolutionData.EvolutionStage.Epic;
			case PetEvolutionData.EvolutionStage.Epic: return PetEvolutionData.EvolutionStage.Legendary;
			case PetEvolutionData.EvolutionStage.Legendary: return PetEvolutionData.EvolutionStage.Legendary; // Max
			default: return currentStage;
		}
	}
	
	// Check if stage is max
	public bool IsMaxStage(PetEvolutionData.EvolutionStage stage)
	{
		return stage == PetEvolutionData.EvolutionStage.Legendary;
	}
	
	// Get all base pets
	public List<string> GetAllBasePets()
	{
		return new List<string>(_evolutionConfigs.Keys);
	}
	
	// Get evolution items (for shop)
	public List<string> GetAllEvolutionItems()
	{
		List<string> items = new List<string>
		{
			"fire_evolution_stone",
			"ice_evolution_stone", 
			"lightning_evolution_stone",
			"dark_evolution_stone",
			"holy_evolution_stone",
			"nature_evolution_stone"
		};
		return items;
	}
}
