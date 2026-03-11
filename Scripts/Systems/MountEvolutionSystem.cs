using Godot;
using System;
using System.Collections.Generic;

public class MountEvolutionSystem
{
	private static MountEvolutionSystem _instance;
	public static MountEvolutionSystem Instance
	{
		get
		{
			if (_instance == null)
				_instance = new MountEvolutionSystem();
			return _instance;
		}
		private set { _instance = value; }
	}
	
	private MountEvolutionData.PlayerMountEvolutionData _playerData;
	private Dictionary<int, MountEvolutionData.MountEvolutionInstance> _activeEvolutions;
	
	public static signal void EvolutionStarted(int mountId, int configId);
	public static signal void EvolutionCompleted(int mountId, int configId);
	public static signal void ExpGained(int mountId, int amount, int totalExp);
	public static signal void SkillUnlocked(int mountId, string skillName);
	public static signal void EvolutionFailed(int mountId, string reason);
	
	public void Initialize()
	{
		_playerData = new MountEvolutionData.PlayerMountEvolutionData();
		_activeEvolutions = new Dictionary<int, MountEvolutionData.MountEvolutionInstance>();
		GD.Print("[MountEvolutionSystem] Initialized");
	}
	
	public void SetPlayerData(MountEvolutionData.PlayerMountEvolutionData data)
	{
		_playerData = data;
		_activeEvolutions = data.ActiveEvolutions;
	}
	
	public MountEvolutionData.PlayerMountEvolutionData GetPlayerData()
	{
		return _playerData;
	}
	
	public bool CanEvolve(int mountId, int configId)
	{
		var config = MountEvolutionDatabase.GetConfigById(configId);
		if (config == null)
		{
			EvolutionFailed?.Emit(mountId, "Invalid evolution config");
			return false;
		}
		
		var player = GetPlayer();
		if (player == null) return false;
		
		// Check level requirement
		int mountLevel = GetMountLevel(mountId);
		if (mountLevel < config.RequiredLevel)
		{
			EvolutionFailed?.Emit(mountId, $"Mount level {config.RequiredLevel} required");
			return false;
		}
		
		// Check gold
		if (player.Gold < config.GoldCost)
		{
			EvolutionFailed?.Emit(mountId, $"Not enough gold. Need {config.GoldCost}");
			return false;
		}
		
		// Check items
		if (config.RequiredItems != null && config.RequiredItems.Count > 0)
		{
			var inventory = GetPlayerInventory();
			if (inventory != null)
			{
				foreach (var itemName in config.RequiredItems)
				{
					if (!HasItem(inventory, itemName))
					{
						EvolutionFailed?.Emit(mountId, $"Missing required item: {itemName}");
						return false;
					}
				}
			}
		}
		
		return true;
	}
	
	public bool TryEvolve(int mountId, int configId)
	{
		if (!CanEvolve(mountId, configId))
			return false;
			
		var config = MountEvolutionDatabase.GetConfigById(configId);
		var player = GetPlayer();
		
		// Deduct gold
		player.Gold -= config.GoldCost;
		
		// Deduct items
		if (config.RequiredItems != null && config.RequiredItems.Count > 0)
		{
			var inventory = GetPlayerInventory();
			if (inventory != null)
			{
				foreach (var itemName in config.RequiredItems)
				{
					RemoveItem(inventory, itemName);
				}
			}
		}
		
		// Create evolution instance
		var instance = new MountEvolutionData.MountEvolutionInstance
		{
			MountId = mountId,
			EvolutionConfigId = configId,
			CurrentExp = 0,
			IsEvolved = false,
			LastExpGain = DateTime.Now
		};
		
		_activeEvolutions[mountId] = instance;
		
		EvolutionStarted?.Emit(mountId, configId);
		
		// Apply initial bonuses
		ApplyEvolutionBonuses(mountId, config);
		
		GD.Print($"[MountEvolutionSystem] Evolution started for mount {mountId} with config {configId}");
		return true;
	}
	
	public void AddExp(int mountId, int amount)
	{
		if (!_activeEvolutions.TryGetValue(mountId, out var instance))
			return;
			
		var config = MountEvolutionDatabase.GetConfigById(instance.EvolutionConfigId);
		if (config == null) return;
		
		instance.CurrentExp += amount;
		instance.LastExpGain = DateTime.Now;
		
		_playerData.TotalExpGained += amount;
		
		ExpGained?.Emit(mountId, amount, instance.CurrentExp);
		
		// Check for evolution
		if (!instance.IsEvolved && instance.CurrentExp >= config.RequiredExp)
		{
			CompleteEvolution(mountId);
		}
		
		SaveData();
	}
	
	private void CompleteEvolution(int mountId)
	{
		if (!_activeEvolutions.TryGetValue(mountId, out var instance))
			return;
			
		var config = MountEvolutionDatabase.GetConfigById(instance.EvolutionConfigId);
		if (config == null) return;
		
		instance.IsEvolved = true;
		_playerData.TotalEvolutions++;
		
		EvolutionCompleted?.Emit(mountId, config.Id);
		
		// Check for skill unlock
		if (!string.IsNullOrEmpty(config.SkillUnlocked))
		{
			SkillUnlocked?.Emit(mountId, config.SkillUnlocked);
		}
		
		// Add to history
		if (!_playerData.EvolutionHistory.ContainsKey(mountId))
			_playerData.EvolutionHistory[mountId] = new List<int>();
		_playerData.EvolutionHistory[mountId].Add(config.Id);
		
		GD.Print($"[MountEvolutionSystem] Evolution completed for mount {mountId}: {config.Name}");
	}
	
	public void ApplyEvolutionBonuses(int mountId, MountEvolutionData.EvolutionConfig config)
	{
		var player = GetPlayer();
		if (player == null) return;
		
		// Apply stat bonuses
		player.AddHealthBonus(config.HealthBonus);
		player.AddAttackBonus(config.AttackBonus);
		player.AddDefenseBonus(config.DefenseBonus);
		player.AddSpeedBonus(config.SpeedBonus);
		player.AddCritRateBonus(config.CritRateBonus);
		player.AddCritDamageBonus(config.CritDamageBonus);
	}
	
	public int GetEvolutionProgress(int mountId)
	{
		if (!_activeEvolutions.TryGetValue(mountId, out var instance))
			return 0;
			
		var config = MountEvolutionDatabase.GetConfigById(instance.EvolutionConfigId);
		if (config == null || config.RequiredExp == 0)
			return 100;
			
		return (int)((float)instance.CurrentExp / config.RequiredExp * 100);
	}
	
	public MountEvolutionData.EvolutionConfig GetEvolutionConfig(int mountId)
	{
		if (!_activeEvolutions.TryGetValue(mountId, out var instance))
			return null;
			
		return MountEvolutionDatabase.GetConfigById(instance.EvolutionConfigId);
	}
	
	public MountEvolutionData.EvolutionConfig GetNextEvolutionConfig(int mountId)
	{
		if (!_activeEvolutions.TryGetValue(mountId, out var instance))
			return null;
			
		return MountEvolutionDatabase.GetNextEvolution(instance.EvolutionConfigId);
	}
	
	public bool HasActiveEvolution(int mountId)
	{
		return _activeEvolutions.ContainsKey(mountId);
	}
	
	public bool IsEvolved(int mountId)
	{
		if (!_activeEvolutions.TryGetValue(mountId, out var instance))
			return false;
		return instance.IsEvolved;
	}
	
	public Dictionary<int, int> GetEvolutionStatistics()
	{
		var stats = new Dictionary<int, int>();
		stats["totalEvolutions"] = _playerData.TotalEvolutions;
		stats["totalExpGained"] = _playerData.TotalExpGained;
		stats["activeEvolutions"] = _activeEvolutions.Count;
		
		int legendaryCount = 0;
		int epicCount = 0;
		int eliteCount = 0;
		
		foreach (var instance in _activeEvolutions.Values)
		{
			var config = MountEvolutionDatabase.GetConfigById(instance.EvolutionConfigId);
			if (config != null)
			{
				switch (config.Stage)
				{
					case MountEvolutionData.EvolutionStage.Legendary:
						legendaryCount++;
						break;
					case MountEvolutionData.EvolutionStage.Epic:
						epicCount++;
						break;
					case MountEvolutionData.EvolutionStage.Elite:
						eliteCount++;
						break;
				}
			}
		}
		
		stats["legendaryEvolutions"] = legendaryCount;
		stats["epicEvolutions"] = epicCount;
		stats["eliteEvolutions"] = eliteCount;
		
		return stats;
	}
	
	public List<MountEvolutionData.EvolutionConfig> GetAvailableEvolutions(MountEvolutionData.EvolutionChain chain)
	{
		return MountEvolutionDatabase.GetConfigsByChain(chain);
	}
	
	public Dictionary<string, object> GetSaveData()
	{
		var data = new Dictionary<string, object>();
		data["activeEvolutions"] = new List<Dictionary<string, object>>();
		
		foreach (var kvp in _activeEvolutions)
		{
			var instance = kvp.Value;
			var instanceData = new Dictionary<string, object>();
			instanceData["mountId"] = instance.MountId;
			instanceData["configId"] = instance.EvolutionConfigId;
			instanceData["currentExp"] = instance.CurrentExp;
			instanceData["isEvolved"] = instance.IsEvolved;
			instanceData["lastExpGain"] = instance.LastExpGain.ToString("o");
			((List<Dictionary<string, object>>)data["activeEvolutions"]).Add(instanceData);
		}
		
		data["totalEvolutions"] = _playerData.TotalEvolutions;
		data["totalExpGained"] = _playerData.TotalExpGained;
		
		data["evolutionHistory"] = new Dictionary<string, List<int>>();
		foreach (var kvp in _playerData.EvolutionHistory)
		{
			((Dictionary<string, List<int>>)data["evolutionHistory"])[kvp.Key.ToString()] = kvp.Value;
		}
		
		return data;
	}
	
	public void LoadSaveData(Dictionary<string, object> data)
	{
		if (data == null) return;
		
		_activeEvolutions.Clear();
		
		if (data.ContainsKey("activeEvolutions"))
		{
			var evolutionList = (List<Dictionary<string, object>>)data["activeEvolutions"];
			foreach (var instanceData in evolutionList)
			{
				var instance = new MountEvolutionData.MountEvolutionInstance();
				instance.MountId = Convert.ToInt32(instanceData["mountId"]);
				instance.EvolutionConfigId = Convert.ToInt32(instanceData["configId"]);
				instance.CurrentExp = Convert.ToInt32(instanceData["currentExp"]);
				instance.IsEvolved = Convert.ToBoolean(instanceData["isEvolved"]);
				
				if (instanceData.ContainsKey("lastExpGain"))
				{
					DateTime.TryParse(instanceData["lastExpGain"].ToString(), out var lastExp);
					instance.LastExpGain = lastExp;
				}
				
				_activeEvolutions[instance.MountId] = instance;
			}
		}
		
		_playerData.TotalEvolutions = data.ContainsKey("totalEvolutions") ? Convert.ToInt32(data["totalEvolutions"]) : 0;
		_playerData.TotalExpGained = data.ContainsKey("totalExpGained") ? Convert.ToInt32(data["totalExpGained"]) : 0;
		
		_playerData.EvolutionHistory.Clear();
		if (data.ContainsKey("evolutionHistory"))
		{
			var history = (Dictionary<string, List<int>>)data["evolutionHistory"];
			foreach (var kvp in history)
			{
				if (int.TryParse(kvp.Key, out int mountId))
				{
					_playerData.EvolutionHistory[mountId] = kvp.Value;
				}
			}
		}
		
		GD.Print($"[MountEvolutionSystem] Loaded {_activeEvolutions.Count} active evolutions");
	}
	
	private void SaveData()
	{
		// Trigger save through SaveSystem
		if (HasMethod("SaveSystem", "SaveGame"))
		{
			// SaveSystem.SaveGame();
		}
	}
	
	// Helper methods to get player data
	private Player GetPlayer()
	{
		var player = GetTree().GetFirstNodeInGroup("player");
		if (player is Player p)
			return p;
			
		// Try to find Player node
		var nodes = GetTree().GetNodesInGroup("player");
		if (nodes.Count > 0)
			return nodes[0] as Player;
			
		return null;
	}
	
	private List<InventorySlot> GetPlayerInventory()
	{
		var player = GetPlayer();
		if (player is Player p && p.HasMethod("GetInventory"))
		{
			return p.GetInventory();
		}
		return null;
	}
	
	private int GetMountLevel(int mountId)
	{
		// Placeholder - would integrate with MountManager
		return 1;
	}
	
	private bool HasItem(List<InventorySlot> inventory, string itemName)
	{
		if (inventory == null) return false;
		foreach (var slot in inventory)
		{
			if (slot.Item != null && slot.Item.Name == itemName && slot.Quantity > 0)
				return true;
		}
		return false;
	}
	
	private void RemoveItem(List<InventorySlot> inventory, string itemName)
	{
		if (inventory == null) return;
		foreach (var slot in inventory)
		{
			if (slot.Item != null && slot.Item.Name == itemName && slot.Quantity > 0)
			{
				slot.Quantity--;
				if (slot.Quantity <= 0)
					slot.Item = null;
				break;
			}
		}
	}
	
	private bool HasMethod(object obj, string methodName)
	{
		var type = obj.GetType();
		return type.GetMethod(methodName) != null;
	}
}
