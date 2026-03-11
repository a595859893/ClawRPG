using Godot;
using System;
using System.Collections.Generic;

public class PetEvolutionData
{
	// Evolution stages
	public enum EvolutionStage
	{
		Basic,      // 基础
		Advanced,   // 进阶
		Elite,      // 精英
		Epic,       // 史诗
		Legendary   // 传说
	}
	
	// Evolution types
	public enum EvolutionType
	{
		Fire,       // 火焰
		Ice,        // 冰霜
		Lightning,  // 闪电
		Dark,       // 黑暗
		Holy,       // 神圣
		Nature      // 自然
	}
	
	// Single pet evolution data
	public class PetEvolutionInstance
	{
		public string PetId { get; set; }
		public string BasePetId { get; set; }
		public EvolutionStage Stage { get; set; }
		public EvolutionType Type { get; set; }
		public int CurrentExp { get; set; }
		public int BattleExp { get; set; }  // Battle experience
		public int TotalKills { get; set; }
		public int EvolutionItemCount { get; set; }
		public bool IsMaxStage { get; set; }
	}
	
	// Evolution requirement
	public class EvolutionRequirement
	{
		public EvolutionStage RequiredStage { get; set; }
		public int RequiredBattleExp { get; set; }
		public int RequiredKills { get; set; }
		public string RequiredItemId { get; set; }
		public int RequiredItemCount { get; set; }
		public EvolutionType? RequiredType { get; set; }  // Optional type requirement
	}
	
	// Evolution reward
	public class EvolutionReward
	{
		public int AttackBonus { get; set; }
		public int DefenseBonus { get; set; }
		public int HealthBonus { get; set; }
		public int SpeedBonus { get; set; }
		public float CritRateBonus { get; set; }
		public float CritDamageBonus { get; set; }
		public float LifestealBonus { get; set; }
		public List<string> UnlockedSkills { get; set; }
	}
	
	// Pet evolution config
	public class PetEvolutionConfig
	{
		public string PetId { get; set; }
		public EvolutionStage Stage { get; set; }
		public EvolutionType Type { get; set; }
		public string DisplayName { get; set; }
		public string Description { get; set; }
		public int BaseAttack { get; set; }
		public int BaseDefense { get; set; }
		public int BaseHealth { get; set; }
		public int BaseSpeed { get; set; }
		public float BaseCritRate { get; set; }
		public float BaseCritDamage { get; set; }
		public float BaseLifesteal { get; set; }
		public EvolutionRequirement Requirement { get; set; }
		public EvolutionReward Reward { get; set; }
	}
}
