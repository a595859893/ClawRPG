using Godot;
using System;
using System.Collections.Generic;

public enum BuffType
{
	AttackBoost,      // 攻击强化
	DefenseBoost,     // 防御强化
	HealthBoost,     // 生命强化
	MagicBoost,      // 魔法强化
	SpeedBoost,      // 速度强化
	CritRateBoost,   // 暴击率提升
	CritDamageBoost, // 暴击伤害提升
	LifeStealBoost,  // 生命偷取提升
	DodgeBoost,     // 闪避提升
	Shield,          // 护盾
	Invincible,      // 无敌
	Poison,          // 中毒
	Bleed,           // 流血
	Burn,            // 燃烧
	Freeze,          // 冰冻
	Stun,            // 眩晕
	Silence,         // 沉默
	Root,            // 束缚
	Slow,            // 减速
	Weak,            // 虚弱
}

public enum BuffSource
{
	Skill,      // 技能
	Potion,     // 药水
	Equipment,  // 装备
	Environment, // 环境
	Enemy,      // 敌人
	Special,    // 特殊
}

public class BuffInfo
{
	public string Id;
	public string Name;
	public string Description;
	public BuffType Type;
	public BuffSource Source;
	public float Duration;      // 持续时间（秒），-1表示永久
	public float TickInterval;  // 周期触发间隔（秒）
	public float TickDamage;    // 周期伤害/治疗
	public float StartValue;    // 起始值
	public float EndValue;      // 结束值（用于衰减）
	public bool IsPermanent;    // 是否永久
	public bool IsDebuff;      // 是否减益
	public bool CanStack;      // 是否可以叠加
	public int MaxStacks;      // 最大叠加层数
	public int Priority;       // 优先级（高优先级buff覆盖低优先级）
	
	public BuffInfo()
	{
		Duration = -1f;
		TickInterval = 1f;
		IsPermanent = false;
		CanStack = false;
		MaxStacks = 1;
		Priority = 0;
	}
}

public class ActiveBuff
{
	public string Id;
	public BuffInfo Info;
	public float TimeRemaining;  // 剩余时间
	public float TimeElapsed;   // 已经过时间
	public int StackCount;      // 当前层数
	public float CurrentValue;  // 当前值
	public float TickTimer;     // 周期计时器
	public bool IsActive;
	public object Caster;        // 施加者引用
	
	public ActiveBuff(BuffInfo info, object caster = null)
	{
		Id = info.Id + "_" + Guid.NewGuid().ToString().Substring(0, 8);
		Info = info;
		TimeRemaining = info.Duration;
		TimeElapsed = 0;
		StackCount = 1;
		CurrentValue = info.StartValue;
		TickTimer = 0;
		IsActive = true;
		Caster = caster;
	}
}

public class PlayerBuffData
{
	public Dictionary<string, int> BuffStacks = new Dictionary<string, int>();
	public int TotalBuffsApplied;
	public int TotalDebuffsApplied;
	public int TotalBuffTime;
	public Dictionary<string, int> BuffSourceCount = new Dictionary<string, int>();
	
	public PlayerBuffData()
	{
	}
}
