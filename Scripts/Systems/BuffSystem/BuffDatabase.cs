using Godot;
using System;
using System.Collections.Generic;

public class BuffDatabase
{
	private static BuffDatabase _instance;
	public static BuffDatabase Instance
	{
		get
		{
			if (_instance == null) _instance = new BuffDatabase();
			return _instance;
		}
	}
	
	public Dictionary<string, BuffInfo> AllBuffs = new Dictionary<string, BuffInfo>();
	
	public BuffDatabase()
	{
		InitializeBuffs();
	}
	
	private void InitializeBuffs()
	{
		// ===== 增益Buff (正面向) =====
		
		// 攻击强化
		BuffInfo attackBoost = new BuffInfo
		{
			Id = "attack_boost",
			Name = "攻击强化",
			Description = "攻击力提升",
			Type = BuffType.AttackBoost,
			Source = BuffSource.Skill,
			Duration = 30f,
			StartValue = 0.2f,
			EndValue = 0.2f,
			IsDebuff = false,
			CanStack = true,
			MaxStacks = 5,
			Priority = 10
		};
		AllBuffs["attack_boost"] = attackBoost;
		
		// 防御强化
		BuffInfo defenseBoost = new BuffInfo
		{
			Id = "defense_boost",
			Name = "防御强化",
			Description = "防御力提升",
			Type = BuffType.DefenseBoost,
			Source = BuffSource.Skill,
			Duration = 30f,
			StartValue = 0.2f,
			EndValue = 0.2f,
			IsDebuff = false,
			CanStack = true,
			MaxStacks = 5,
			Priority = 10
		};
		AllBuffs["defense_boost"] = defenseBoost;
		
		// 生命强化
		BuffInfo healthBoost = new BuffInfo
		{
			Id = "health_boost",
			Name = "生命强化",
			Description = "最大生命值提升",
			Type = BuffType.HealthBoost,
			Source = BuffSource.Skill,
			Duration = 60f,
			StartValue = 0.3f,
			EndValue = 0.3f,
			IsDebuff = false,
			CanStack = false,
			Priority = 15
		};
		AllBuffs["health_boost"] = healthBoost;
		
		// 魔法强化
		BuffInfo magicBoost = new BuffInfo
		{
			Id = "magic_boost",
			Name = "魔法强化",
			Description = "魔法攻击力提升",
			Type = BuffType.MagicBoost,
			Source = BuffSource.Skill,
			Duration = 30f,
			StartValue = 0.25f,
			EndValue = 0.25f,
			IsDebuff = false,
			CanStack = true,
			MaxStacks = 3,
			Priority = 10
		};
		AllBuffs["magic_boost"] = magicBoost;
		
		// 速度强化
		BuffInfo speedBoost = new BuffInfo
		{
			Id = "speed_boost",
			Name = "速度强化",
			Description = "移动速度提升",
			Type = BuffType.SpeedBoost,
			Source = BuffSource.Skill,
			Duration = 20f,
			StartValue = 0.3f,
			EndValue = 0.3f,
			IsDebuff = false,
			CanStack = false,
			Priority = 10
		};
		AllBuffs["speed_boost"] = speedBoost;
		
		// 暴击率提升
		BuffInfo critRateBoost = new BuffInfo
		{
			Id = "crit_rate_boost",
			Name = "暴击强化",
			Description = "暴击率提升",
			Type = BuffType.CritRateBoost,
			Source = BuffSource.Skill,
			Duration = 25f,
			StartValue = 0.15f,
			EndValue = 0.15f,
			IsDebuff = false,
			CanStack = true,
			MaxStacks = 3,
			Priority = 10
		};
		AllBuffs["crit_rate_boost"] = critRateBoost;
		
		// 暴击伤害提升
		BuffInfo critDamageBoost = new BuffInfo
		{
			Id = "crit_damage_boost",
			Name = "暴击伤害强化",
			Description = "暴击伤害提升",
			Type = BuffType.CritDamageBoost,
			Source = BuffSource.Skill,
			Duration = 25f,
			StartValue = 0.5f,
			EndValue = 0.5f,
			IsDebuff = false,
			CanStack = false,
			Priority = 10
		};
		AllBuffs["crit_damage_boost"] = critDamageBoost;
		
		// 生命偷取提升
		BuffInfo lifestealBoost = new BuffInfo
		{
			Id = "lifesteal_boost",
			Name = "生命偷取强化",
			Description = "生命偷取提升",
			Type = BuffType.LifeStealBoost,
			Source = BuffSource.Skill,
			Duration = 30f,
			StartValue = 0.2f,
			EndValue = 0.2f,
			IsDebuff = false,
			CanStack = false,
			Priority = 10
		};
		AllBuffs["lifesteal_boost"] = lifestealBoost;
		
		// 闪避提升
		BuffInfo dodgeBoost = new BuffInfo
		{
			Id = "dodge_boost",
			Name = "闪避强化",
			Description = "闪避率提升",
			Type = BuffType.DodgeBoost,
			Source = BuffSource.Skill,
			Duration = 20f,
			StartValue = 0.2f,
			EndValue = 0.2f,
			IsDebuff = false,
			CanStack = false,
			Priority = 10
		};
		AllBuffs["dodge_boost"] = dodgeBoost;
		
		// 护盾
		BuffInfo shield = new BuffInfo
		{
			Id = "shield",
			Name = "护盾",
			Description = "吸收伤害的护盾",
			Type = BuffType.Shield,
			Source = BuffSource.Skill,
			Duration = 15f,
			StartValue = 100f,
			EndValue = 0f,
			IsDebuff = false,
			CanStack = false,
			Priority = 20
		};
		AllBuffs["shield"] = shield;
		
		// 无敌
		BuffInfo invincible = new BuffInfo
		{
			Id = "invincible",
			Name = "无敌",
			Description = "完全无敌状态",
			Type = BuffType.Invincible,
			Source = BuffSource.Skill,
			Duration = 5f,
			StartValue = 1f,
			EndValue = 1f,
			IsDebuff = false,
			CanStack = false,
			Priority = 100
		};
		AllBuffs["invincible"] = invincible;
		
		// ===== 减益Buff (负面) =====
		
		// 中毒
		BuffInfo poison = new BuffInfo
		{
			Id = "poison",
			Name = "中毒",
			Description = "持续损失生命",
			Type = BuffType.Poison,
			Source = BuffSource.Enemy,
			Duration = 10f,
			TickInterval = 1f,
			TickDamage = 5f,
			IsDebuff = true,
			CanStack = true,
			MaxStacks = 5,
			Priority = 5
		};
		AllBuffs["poison"] = poison;
		
		// 流血
		BuffInfo bleed = new BuffInfo
		{
			Id = "bleed",
			Name = "流血",
			Description = "移动时损失更多生命",
			Type = BuffType.Bleed,
			Source = BuffSource.Enemy,
			Duration = 8f,
			TickInterval = 0.5f,
			TickDamage = 3f,
			IsDebuff = true,
			CanStack = true,
			MaxStacks = 3,
			Priority = 5
		};
		AllBuffs["bleed"] = bleed;
		
		// 燃烧
		BuffInfo burn = new BuffInfo
		{
			Id = "burn",
			Name = "燃烧",
			Description = "持续损失生命",
			Type = BuffType.Burn,
			Source = BuffSource.Enemy,
			Duration = 12f,
			TickInterval = 1f,
			TickDamage = 8f,
			IsDebuff = true,
			CanStack = true,
			MaxStacks = 3,
			Priority = 5
		};
		AllBuffs["burn"] = burn;
		
		// 冰冻
		BuffInfo freeze = new BuffInfo
		{
			Id = "freeze",
			Name = "冰冻",
			Description = "无法移动",
			Type = BuffType.Freeze,
			Source = BuffSource.Enemy,
			Duration = 3f,
			StartValue = 1f,
			EndValue = 1f,
			IsDebuff = true,
			CanStack = false,
			Priority = 50
		};
		AllBuffs["freeze"] = freeze;
		
		// 眩晕
		BuffInfo stun = new BuffInfo
		{
			Id = "stun",
			Name = "眩晕",
			Description = "无法行动",
			Type = BuffType.Stun,
			Source = BuffSource.Enemy,
			Duration = 2f,
			StartValue = 1f,
			EndValue = 1f,
			IsDebuff = true,
			CanStack = false,
			Priority = 50
		};
		AllBuffs["stun"] = stun;
		
		// 沉默
		BuffInfo silence = new BuffInfo
		{
			Id = "silence",
			Name = "沉默",
			Description = "无法使用技能",
			Type = BuffType.Silence,
			Source = BuffSource.Enemy,
			Duration = 5f,
			StartValue = 1f,
			EndValue = 1f,
			IsDebuff = true,
			CanStack = false,
			Priority = 30
		};
		AllBuffs["silence"] = silence;
		
		// 束缚
		BuffInfo root = new BuffInfo
		{
			Id = "root",
			Name = "束缚",
			Description = "无法移动",
			Type = BuffType.Root,
			Source = BuffSource.Enemy,
			Duration = 3f,
			StartValue = 1f,
			EndValue = 1f,
			IsDebuff = true,
			CanStack = false,
			Priority = 40
		};
		AllBuffs["root"] = root;
		
		// 减速
		BuffInfo slow = new BuffInfo
		{
			Id = "slow",
			Name = "减速",
			Description = "移动速度降低",
			Type = BuffType.Slow,
			Source = BuffSource.Enemy,
			Duration = 5f,
			StartValue = 0.5f,
			EndValue = 0.5f,
			IsDebuff = true,
			CanStack = true,
			MaxStacks = 3,
			Priority = 10
		};
		AllBuffs["slow"] = slow;
		
		// 虚弱
		BuffInfo weak = new BuffInfo
		{
			Id = "weak",
			Name = "虚弱",
			Description = "攻击力降低",
			Type = BuffType.Weak,
			Source = BuffSource.Enemy,
			Duration = 10f,
			StartValue = 0.3f,
			EndValue = 0.3f,
			IsDebuff = true,
			CanStack = true,
			MaxStacks = 3,
			Priority = 10
		};
		AllBuffs["weak"] = weak;
		
		// ===== 特殊Buff =====
		
		// 圣光祝福
		BuffInfo holyBlessing = new BuffInfo
		{
			Id = "holy_blessing",
			Name = "圣光祝福",
			Description = "每秒恢复生命",
			Type = BuffType.HealthBoost,
			Source = BuffSource.Skill,
			Duration = 20f,
			TickInterval = 1f,
			TickDamage = -10f,  // 负数表示治疗
			IsDebuff = false,
			CanStack = false,
			Priority = 15
		};
		AllBuffs["holy_blessing"] = holyBlessing;
		
		// 暗影之力
		BuffInfo shadowPower = new BuffInfo
		{
			Id = "shadow_power",
			Name = "暗影之力",
			Description = "攻击附带暗影伤害",
			Type = BuffType.AttackBoost,
			Source = BuffSource.Skill,
			Duration = 15f,
			StartValue = 0.5f,
			EndValue = 0.5f,
			IsDebuff = false,
			CanStack = false,
			Priority = 20
		};
		AllBuffs["shadow_power"] = shadowPower;
		
		// 雷霆之力
		BuffInfo thunderPower = new BuffInfo
		{
			Id = "thunder_power",
			Name = "雷霆之力",
			Description = "攻击附带雷电伤害",
			Type = BuffType.AttackBoost,
			Source = BuffSource.Skill,
			Duration = 15f,
			StartValue = 0.4f,
			EndValue = 0.4f,
			IsDebuff = false,
			CanStack = false,
			Priority = 20
		};
		AllBuffs["thunder_power"] = thunderPower;
	}
	
	public BuffInfo GetBuff(string buffId)
	{
		if (AllBuffs.ContainsKey(buffId))
			return AllBuffs[buffId];
		return null;
	}
	
	public List<BuffInfo> GetBuffsByType(BuffType type)
	{
		List<BuffInfo> result = new List<BuffInfo>();
		foreach (var buff in AllBuffs.Values)
		{
			if (buff.Type == type)
				result.Add(buff);
		}
		return result;
	}
	
	public List<BuffInfo> GetBuffsBySource(BuffSource source)
	{
		List<BuffInfo> result = new List<BuffInfo>();
		foreach (var buff in AllBuffs.Values)
		{
			if (buff.Source == source)
				result.Add(buff);
		}
		return result;
	}
	
	public List<BuffInfo> GetDebuffs()
	{
		List<BuffInfo> result = new List<BuffInfo>();
		foreach (var buff in AllBuffs.Values)
		{
			if (buff.IsDebuff)
				result.Add(buff);
		}
		return result;
	}
	
	public List<BuffInfo> GetPositiveBuffs()
	{
		List<BuffInfo> result = new List<BuffInfo>();
		foreach (var buff in AllBuffs.Values)
		{
			if (!buff.IsDebuff)
				result.Add(buff);
		}
		return result;
	}
}
