using Godot;
using System;
using System.Collections.Generic;

public class CombatSkillCooldownData
{
	/// <summary>
	/// 技能冷却数据
	/// </summary>
	public class SkillCooldown
	{
		public string SkillId { get; set; }
		public string SkillName { get; set; }
		public float MaxCooldown { get; set; }
		public float CurrentCooldown { get; set; }
		public bool IsReady => CurrentCooldown <= 0;
		public float CooldownPercent => MaxCooldown > 0 ? 1.0f - (CurrentCooldown / MaxCooldown) : 1.0f;
	}
	
	/// <summary>
	/// 玩家技能冷却数据
	/// </summary>
	public class PlayerSkillCooldownData
	{
		public Dictionary<string, SkillCooldown> ActiveCooldowns { get; set; } = new Dictionary<string, SkillCooldown>();
		public int TotalSkillsUsed { get; set; }
		public int TotalCooldownTime { get; set; }
		public Dictionary<string, int> SkillUsageCount { get; set; } = new Dictionary<string, int>();
	}
}
