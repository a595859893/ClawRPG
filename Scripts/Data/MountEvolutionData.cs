using Godot;
using System;
using System.Collections.Generic;

public class MountEvolutionData
{
	public enum EvolutionStage
	{
		Basic,
		Advanced,
		Elite,
		Epic,
		Legendary
	}
	
	public enum EvolutionType
	{
		Fire,
		Ice,
		Lightning,
		Dark,
		Holy,
		Nature
	}
	
	public enum EvolutionChain
	{
		Horse,
		Wolf,
		Bear,
		Eagle,
		Dragon,
		Phoenix,
		Griffin,
		Unicorn
	}
	
	public class EvolutionConfig
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public EvolutionChain Chain { get; set; }
		public EvolutionStage Stage { get; set; }
		public EvolutionType Type { get; set; }
		public int RequiredExp { get; set; }
		public int RequiredLevel { get; set; }
		public int GoldCost { get; set; }
		public List<string> RequiredItems { get; set; }
		public float HealthBonus { get; set; }
		public float AttackBonus { get; set; }
		public float DefenseBonus { get; set; }
		public float SpeedBonus { get; set; }
		public float CritRateBonus { get; set; }
		public float CritDamageBonus { get; set; }
		public string SkillUnlocked { get; set; }
		public string NextEvolutionName { get; set; }
	}
	
	public class MountEvolutionInstance
	{
		public int MountId { get; set; }
		public int EvolutionConfigId { get; set; }
		public int CurrentExp { get; set; }
		public bool IsEvolved { get; set; }
		public DateTime LastExpGain { get; set; }
	}
	
	public class PlayerMountEvolutionData
	{
		public Dictionary<int, MountEvolutionInstance> ActiveEvolutions { get; set; }
		public Dictionary<int, List<int>> EvolutionHistory { get; set; }
		public int TotalEvolutions { get; set; }
		public int TotalExpGained { get; set; }
		
		public PlayerMountEvolutionData()
		{
			ActiveEvolutions = new Dictionary<int, MountEvolutionInstance>();
			EvolutionHistory = new Dictionary<int, List<int>>();
		}
	}
}
