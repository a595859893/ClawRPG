using Godot;
using System;
using System.Collections.Generic;

public class PetEvolutionSystem : Node
{
	private static PetEvolutionSystem _instance;
	public static PetEvolutionSystem Instance
	{
		get { return _instance; }
	}
	
	// Player's pet evolution data: pet_index -> evolution instance
	private Dictionary<int, PetEvolutionData.PetEvolutionInstance> _petEvolutions;
	
	// Evolution signals
	public static void EvolutionCompleted(int petIndex, string newPetId) { }
	public static void EvolutionAvailable(int petIndex) { }
	public static void EvolutionFailed(int petIndex, string reason) { }
	
	public override void _Ready()
	{
		_instance = this;
		_petEvolutions = new Dictionary<int, PetEvolutionData.PetEvolutionInstance>();
	}
	
	// Initialize a pet for evolution tracking
	public void InitializePet(int petIndex, string basePetId)
	{
		if (!_petEvolutions.ContainsKey(petIndex))
		{
			_petEvolutions[petIndex] = new PetEvolutionData.PetEvolutionInstance
			{
				PetId = basePetId + "_basic_nature",
				BasePetId = basePetId,
				Stage = PetEvolutionData.EvolutionStage.Basic,
				Type = PetEvolutionData.EvolutionType.Nature,
				CurrentExp = 0,
				BattleExp = 0,
				TotalKills = 0,
				EvolutionItemCount = 0,
				IsMaxStage = false
			};
		}
	}
	
	// Add battle experience to a pet
	public void AddBattleExp(int petIndex, int exp, int kills = 0)
	{
		if (!_petEvolutions.ContainsKey(petIndex))
		{
			// Auto-initialize if not exists
			InitializePet(petIndex, "wolf"); // Default base
		}
		
		var evolution = _petEvolutions[petIndex];
		evolution.BattleExp += exp;
		evolution.TotalKills += kills;
		
		// Check if evolution becomes available
		CheckEvolutionAvailability(petIndex);
	}
	
	// Add evolution item count
	public void AddEvolutionItem(int petIndex, string itemId)
	{
		if (!_petEvolutions.ContainsKey(petIndex))
		{
			InitializePet(petIndex, "wolf");
		}
		
		_petEvolutions[petIndex].EvolutionItemCount++;
		CheckEvolutionAvailability(petIndex);
	}
	
	// Check if evolution is available
	public bool CanEvolve(int petIndex)
	{
		if (!_petEvolutions.ContainsKey(petIndex))
			return false;
			
		var evolution = _petEvolutions[petIndex];
		
		if (evolution.IsMaxStage)
			return false;
		
		// Check database for next evolution
		var availableEvolutions = PetEvolutionDatabase.Instance.GetAvailableEvolutions(
			evolution.BasePetId, evolution.Stage, evolution.Type);
		
		foreach (var nextEvo in availableEvolutions)
		{
			if (CanMeetRequirement(evolution, nextEvo.Requirement))
			{
				return true;
			}
		}
		
		return false;
	}
	
	// Check if can meet requirement
	private bool CanMeetRequirement(PetEvolutionData.PetEvolutionInstance current, PetEvolutionData.EvolutionRequirement req)
	{
		// Check battle exp
		if (current.BattleExp < req.RequiredBattleExp)
			return false;
			
		// Check kills
		if (current.TotalKills < req.RequiredKills)
			return false;
			
		// Check item (simplified - just check count)
		if (current.EvolutionItemCount < req.RequiredItemCount)
			return false;
			
		return true;
	}
	
	// Get evolution progress
	public float GetEvolutionProgress(int petIndex)
	{
		if (!_petEvolutions.ContainsKey(petIndex))
			return 0.0f;
			
		var evolution = _petEvolutions[petIndex];
		
		if (evolution.IsMaxStage)
			return 1.0f;
		
		var availableEvolutions = PetEvolutionDatabase.Instance.GetAvailableEvolutions(
			evolution.BasePetId, evolution.Stage, evolution.Type);
		
		if (availableEvolutions.Count == 0)
			return 1.0f;
		
		// Get the first available evolution requirement
		var nextEvo = availableEvolutions[0];
		var req = nextEvo.Requirement;
		
		// Calculate progress based on battle exp
		float expProgress = (float)evolution.BattleExp / req.RequiredBattleExp;
		float killProgress = (float)evolution.TotalKills / req.RequiredKills;
		float itemProgress = (float)evolution.EvolutionItemCount / req.RequiredItemCount;
		
		// Average progress
		return Mathf.Clamp((expProgress + killProgress + itemProgress) / 3.0f, 0.0f, 1.0f);
	}
	
	// Get available evolution options
	public List<PetEvolutionData.PetEvolutionConfig> GetAvailableEvolutionOptions(int petIndex)
	{
		List<PetEvolutionData.PetEvolutionConfig> results = new List<PetEvolutionData.PetEvolutionConfig>();
		
		if (!_petEvolutions.ContainsKey(petIndex))
			return results;
			
		var evolution = _petEvolutions[petIndex];
		
		if (evolution.IsMaxStage)
			return results;
		
		var availableEvolutions = PetEvolutionDatabase.Instance.GetAvailableEvolutions(
			evolution.BasePetId, evolution.Stage, evolution.Type);
		
		foreach (var nextEvo in availableEvolutions)
		{
			if (CanMeetRequirement(evolution, nextEvo.Requirement))
			{
				results.Add(nextEvo);
			}
		}
		
		return results;
	}
	
	// Attempt to evolve pet
	public bool TryEvolve(int petIndex, PetEvolutionData.EvolutionType targetType)
	{
		if (!_petEvolutions.ContainsKey(petIndex))
		{
			GD.PrintErr("[PetEvolution] Pet not initialized: " + petIndex);
			return false;
		}
		
		var evolution = _petEvolutions[petIndex];
		
		if (evolution.IsMaxStage)
		{
			GD.PrintErr("[PetEvolution] Pet already at max stage");
			return false;
		}
		
		// Find matching evolution config
		var availableEvolutions = PetEvolutionDatabase.Instance.GetAvailableEvolutions(
			evolution.BasePetId, evolution.Stage, evolution.Type);
		
		PetEvolutionData.PetEvolutionConfig targetConfig = null;
		foreach (var evo in availableEvolutions)
		{
			if (evo.Type == targetType && CanMeetRequirement(evolution, evo.Requirement))
			{
				targetConfig = evo;
				break;
			}
		}
		
		if (targetConfig == null)
		{
			GD.PrintErr("[PetEvolution] No valid evolution available for type: " + targetType);
			return false;
		}
		
		// Consume items
		int itemCost = targetConfig.Requirement.RequiredItemCount;
		evolution.EvolutionItemCount -= itemCost;
		
		// Update evolution state
		evolution.PetId = targetConfig.PetId;
		evolution.Stage = targetConfig.Stage;
		evolution.Type = targetConfig.Type;
		
		// Check if max stage
		if (PetEvolutionDatabase.Instance.IsMaxStage(evolution.Stage))
		{
			evolution.IsMaxStage = true;
		}
		
		// Emit signal
		EvolutionCompleted(petIndex, evolution.PetId);
		GD.Print("[PetEvolution] Pet " + petIndex + " evolved to " + targetConfig.DisplayName);
		
		return true;
	}
	
	// Get pet evolution stats
	public Dictionary<string, object> GetPetEvolutionStats(int petIndex)
	{
		Dictionary<string, object> stats = new Dictionary<string, object>();
		
		if (!_petEvolutions.ContainsKey(petIndex))
		{
			stats["exists"] = false; 
			return stats;
		}
		
		var evolution = _petEvolutions[petIndex];
		var config = PetEvolutionDatabase.Instance.GetEvolutionConfig(
			evolution.BasePetId, evolution.Stage, evolution.Type);
		
		stats["exists"] = true;
		stats["pet_id"] = evolution.PetId;
		stats["base_pet_id"] = evolution.BasePetId;
		stats["stage"] = evolution.Stage.ToString();
		stats["type"] = evolution.Type.ToString();
		stats["battle_exp"] = evolution.BattleExp;
		stats["total_kills"] = evolution.TotalKills;
		stats["evolution_items"] = evolution.EvolutionItemCount;
		stats["is_max_stage"] = evolution.IsMaxStage;
		stats["progress"] = GetEvolutionProgress(petIndex);
		
		if (config != null)
		{
			stats["display_name"] = config.DisplayName;
			stats["description"] = config.Description;
			stats["base_attack"] = config.BaseAttack;
			stats["base_defense"] = config.BaseDefense;
			stats["base_health"] = config.BaseHealth;
			stats["base_speed"] = config.BaseSpeed;
			
			// Get next evolution requirement if available
			var nextEvolutions = GetAvailableEvolutionOptions(petIndex);
			if (nextEvolutions.Count > 0)
			{
				var next = nextEvolutions[0];
				stats["next_evolution"] = next.DisplayName;
				stats["required_battle_exp"] = next.Requirement.RequiredBattleExp;
				stats["required_kills"] = next.Requirement.RequiredKills;
				stats["required_items"] = next.Requirement.RequiredItemCount;
				stats["required_item_id"] = next.Requirement.RequiredItemId;
			}
		}
		
		return stats;
	}
	
	// Check evolution availability and emit signal
	private void CheckEvolutionAvailability(int petIndex)
	{
		if (CanEvolve(petIndex))
		{
			EvolutionAvailable(petIndex);
		}
	}
	
	// Save evolution data
	public Dictionary<string, object> Save()
	{
		Dictionary<string, object> data = new Dictionary<string, object>();
		List<Dictionary<string, object>> pets = new List<Dictionary<string, object>>();
		
		foreach (var kvp in _petEvolutions)
		{
			Dictionary<string, object> petData = new Dictionary<string, object>();
			petData["pet_index"] = kvp.Key;
			petData["pet_id"] = kvp.Value.PetId;
			petData["base_pet_id"] = kvp.Value.BasePetId;
			petData["stage"] = (int)kvp.Value.Stage;
			petData["type"] = (int)kvp.Value.Type;
			petData["battle_exp"] = kvp.Value.BattleExp;
			petData["total_kills"] = kvp.Value.TotalKills;
			petData["evolution_item_count"] = kvp.Value.EvolutionItemCount;
			petData["is_max_stage"] = kvp.Value.IsMaxStage;
			
			pets.Add(petData);
		}
		
		data["pets"] = pets;
		return data;
	}
	
	// Load evolution data
	public void Load(Dictionary<string, object> data)
	{
		if (data == null) return;
		
		_petEvolutions.Clear();
		
		if (data.ContainsKey("pets"))
		{
			var petsList = (Godot.Array)data["pets"];
			foreach (Dictionary<string, object> petData in petsList)
			{
				int petIndex = Convert.ToInt32(petData["pet_index"]);
				_petEvolutions[petIndex] = new PetEvolutionData.PetEvolutionInstance
				{
					PetId = (string)petData["pet_id"],
					BasePetId = (string)petData["base_pet_id"],
					Stage = (PetEvolutionData.EvolutionStage)Convert.ToInt32(petData["stage"]),
					Type = (PetEvolutionData.EvolutionType)Convert.ToInt32(petData["type"]),
					BattleExp = Convert.ToInt32(petData["battle_exp"]),
					TotalKills = Convert.ToInt32(petData["total_kills"]),
					EvolutionItemCount = Convert.ToInt32(petData["evolution_item_count"]),
					IsMaxStage = (bool)petData["is_max_stage"]
				};
			}
		}
		
		GD.Print("[PetEvolution] Loaded " + _petEvolutions.Count + " pet evolutions");
	}
}
