using Godot;
using System;
using System.Collections.Generic;

public class BuffSystem : Node
{
	private static BuffSystem _instance;
	public static BuffSystem Instance
	{
		get { return _instance; }
	}
	
	// 当前活跃的Buff列表
	private List<ActiveBuff> _activeBuffs = new List<ActiveBuff>();
	
	// 玩家属性加成缓存
	private float _attackBonus = 0f;
	private float _defenseBonus = 0f;
	private float _healthBonus = 0f;
	private float _magicBonus = 0f;
	private float _speedBonus = 0f;
	private float _critRateBonus = 0f;
	private float _critDamageBonus = 0f;
	private float _lifestealBonus = 0f;
	private float _dodgeBonus = 0f;
	private float _shieldValue = 0f;
	
	// 状态标志
	private bool _isInvincible = false; 
	private bool _isFrozen = false; 
	private bool _isStunned = false; 
	private bool _isSilenced = false; 
	private bool _isRooted = false; 
	private float _slowMultiplier = 1f;
	private float _weakMultiplier = 1f;
	
	// 统计
	private PlayerBuffData _playerBuffData = new PlayerBuffData();
	
	// 信号
	[Signal]
	public delegate void BuffApplied(string buffId, int stackCount);
	[Signal]
	public delegate void BuffRemoved(string buffId);
	[Signal]
	public delegate void BuffTick(string buffId, float value);
	[Signal]
	public delegate void BuffStackChanged(string buffId, int newStack);
	[Signal]
	public delegate void ShieldChanged(float newShield);
	[Signal]
	public delegate void StateChanged(string state, bool isActive);
	[Signal]
	public delegate void BuffListChanged();
	
	public override void _Ready()
	{
		_instance = this;
	}
	
	// 应用Buff
	public void ApplyBuff(string buffId, object caster = null, float customDuration = -1f, float customValue = -1f)
	{
		BuffInfo buffInfo = BuffDatabase.Instance.GetBuff(buffId);
		if (buffInfo == null)
		{
			GD.PrintErr($"BuffSystem: Buff {buffId} not found!");
			return;
		}
		
		// 检查是否已经有这个buff
		ActiveBuff existingBuff = FindBuff(buffId);
		
		if (existingBuff != null)
		{
			// 如果可以叠加
			if (buffInfo.CanStack && existingBuff.StackCount < buffInfo.MaxStacks)
			{
				existingBuff.StackCount++;
				existingBuff.CurrentValue = buffInfo.StartValue * existingBuff.StackCount;
				existingBuff.TimeRemaining = customDuration > 0 ? customDuration : buffInfo.Duration;
				
				EmitSignal(nameof(BuffStackChanged), buffId, existingBuff.StackCount);
				EmitSignal(nameof(BuffApplied), buffId, existingBuff.StackCount);
			}
			else
			{
				// 刷新持续时间
				existingBuff.TimeRemaining = customDuration > 0 ? customDuration : buffInfo.Duration;
			}
		}
		else
		{
			// 创建新buff
			ActiveBuff newBuff = new ActiveBuff(buffInfo, caster);
			if (customDuration > 0) newBuff.TimeRemaining = customDuration;
			if (customValue > 0) newBuff.CurrentValue = customValue;
			
			_activeBuffs.Add(newBuff);
			
			// 更新统计
			if (buffInfo.IsDebuff)
				_playerBuffData.TotalDebuffsApplied++;
			else
				_playerBuffData.TotalBuffsApplied++;
			
			if (!_playerBuffData.BuffStacks.ContainsKey(buffId))
				_playerBuffData.BuffStacks[buffId] = 0;
			_playerBuffData.BuffStacks[buffId]++;
			
			string sourceKey = buffInfo.Source.ToString();
			if (!_playerBuffData.BuffSourceCount.ContainsKey(sourceKey))
				_playerBuffData.BuffSourceCount[sourceKey] = 0;
			_playerBuffData.BuffSourceCount[sourceKey]++;
			
			EmitSignal(nameof(BuffApplied), buffId, 1);
		}
		
		// 更新属性加成
		UpdateBuffBonuses();
		EmitSignal(nameof(BuffListChanged));
	}
	
	// 移除Buff
	public void RemoveBuff(string buffId)
	{
		ActiveBuff buff = FindBuff(buffId);
		if (buff != null)
		{
			_activeBuffs.Remove(buff);
			EmitSignal(nameof(BuffRemoved), buffId);
			
			// 更新统计
			if (_playerBuffData.BuffStacks.ContainsKey(buffId))
			{
				_playerBuffData.BuffStacks[buffId]--;
				if (_playerBuffData.BuffStacks[buffId] <= 0)
					_playerBuffData.BuffStacks.Remove(buffId);
			}
			
			// 更新属性加成
			UpdateBuffBonuses();
			EmitSignal(nameof(BuffListChanged));
		}
	}
	
	// 移除所有指定类型的buff
	public void RemoveBuffsByType(BuffType type)
	{
		List<ActiveBuff> toRemove = new List<ActiveBuff>();
		foreach (var buff in _activeBuffs)
		{
			if (buff.Info.Type == type)
				toRemove.Add(buff);
		}
		
		foreach (var buff in toRemove)
		{
			RemoveBuff(buff.Info.Id);
		}
	}
	
	// 移除所有减益buff
	public void RemoveAllDebuffs()
	{
		List<ActiveBuff> toRemove = new List<ActiveBuff>();
		foreach (var buff in _activeBuffs)
		{
			if (buff.Info.IsDebuff)
				toRemove.Add(buff);
		}
		
		foreach (var buff in toRemove)
		{
			RemoveBuff(buff.Info.Id);
		}
	}
	
	// 移除所有增益buff
	public void RemoveAllPositiveBuffs()
	{
		List<ActiveBuff> toRemove = new List<ActiveBuff>();
		foreach (var buff in _activeBuffs)
		{
			if (!buff.Info.IsDebuff)
				toRemove.Add(buff);
		}
		
		foreach (var buff in toRemove)
		{
			RemoveBuff(buff.Info.Id);
		}
	}
	
	// 清除所有buff
	public void ClearAllBuffs()
	{
		_activeBuffs.Clear();
		UpdateBuffBonuses();
		EmitSignal(nameof(BuffListChanged));
	}
	
	// 查找buff
	private ActiveBuff FindBuff(string buffId)
	{
		foreach (var buff in _activeBuffs)
		{
			if (buff.Info.Id == buffId)
				return buff;
		}
		return null;
	}
	
	// 检查是否有指定buff
	public bool HasBuff(string buffId)
	{
		return FindBuff(buffId) != null;
	}
	
	// 检查是否有指定类型的buff
	public bool HasBuffType(BuffType type)
	{
		foreach (var buff in _activeBuffs)
		{
			if (buff.Info.Type == type)
				return true;
		}
		return false;
	}
	
	// 获取buff层数
	public int GetBuffStack(string buffId)
	{
		ActiveBuff buff = FindBuff(buffId);
		return buff != null ? buff.StackCount : 0;
	}
	
	// 获取所有活跃buff
	public List<ActiveBuff> GetAllActiveBuffs()
	{
		return new List<ActiveBuff>(_activeBuffs);
	}
	
	// 获取增益buff
	public List<ActiveBuff> GetPositiveBuffs()
	{
		List<ActiveBuff> result = new List<ActiveBuff>();
		foreach (var buff in _activeBuffs)
		{
			if (!buff.Info.IsDebuff)
				result.Add(buff);
		}
		return result;
	}
	
	// 获取减益buff
	public List<ActiveBuff> GetDebuffs()
	{
		List<ActiveBuff> result = new List<ActiveBuff>();
		foreach (var buff in _activeBuffs)
		{
			if (buff.Info.IsDebuff)
				result.Add(buff);
		}
		return result;
	}
	
	// 每帧更新
	public override void _Process(float delta)
	{
		List<ActiveBuff> toRemove = new List<ActiveBuff>();
		
		foreach (var buff in _activeBuffs)
		{
			if (!buff.IsActive) continue;
			
			// 更新持续时间
			buff.TimeElapsed += delta;
			buff.TimeRemaining -= delta;
			
			// 周期伤害/治疗
			if (buff.Info.TickDamage != 0 && buff.Info.TickInterval > 0)
			{
				buff.TickTimer += delta;
				if (buff.TickTimer >= buff.Info.TickInterval)
				{
					buff.TickTimer = 0;
					ApplyTickEffect(buff);
				}
			}
			
			// 检查是否过期
			if (buff.TimeRemaining <= 0)
			{
				toRemove.Add(buff);
			}
		}
		
		// 移除过期的buff
		foreach (var buff in toRemove)
		{
			RemoveBuff(buff.Info.Id);
		}
	}
	
	// 应用周期效果
	private void ApplyTickEffect(ActiveBuff buff)
	{
		float effectValue = buff.Info.TickDamage * buff.StackCount;
		
		if (effectValue < 0)
		{
			// 治疗效果
			Player player = GetTree().GetFirstNodeInGroup("Player") as Player;
			if (player != null)
			{
				player.Heal(Mathf.Abs(effectValue));
			}
		}
		else
		{
			// 伤害效果
			Player player = GetTree().GetFirstNodeInGroup("Player") as Player;
			if (player != null)
			{
				player.TakeDamage(effectValue, null, false);
			}
		}
		
		EmitSignal(nameof(BuffTick), buff.Info.Id, effectValue);
	}
	
	// 更新属性加成
	private void UpdateBuffBonuses()
	{
		// 重置所有加成
		_attackBonus = 0f;
		_defenseBonus = 0f;
		_healthBonus = 0f;
		_magicBonus = 0f;
		_speedBonus = 0f;
		_critRateBonus = 0f;
		_critDamageBonus = 0f;
		_lifestealBonus = 0f;
		_dodgeBonus = 0f;
		_shieldValue = 0f;
		
		_isInvincible = false; 
		_isFrozen = false; 
		_isStunned = false; 
		_isSilenced = false; 
		_isRooted = false; 
		_slowMultiplier = 1f;
		_weakMultiplier = 1f;
		
		// 累加所有buff的效果
		foreach (var buff in _activeBuffs)
		{
			float value = buff.CurrentValue;
			
			switch (buff.Info.Type)
			{
				case BuffType.AttackBoost:
					_attackBonus += value;
					break;
				case BuffType.DefenseBoost:
					_defenseBonus += value;
					break;
				case BuffType.HealthBoost:
					_healthBonus += value;
					break;
				case BuffType.MagicBoost:
					_magicBonus += value;
					break;
				case BuffType.SpeedBoost:
					_speedBonus += value;
					break;
				case BuffType.CritRateBoost:
					_critRateBonus += value;
					break;
				case BuffType.CritDamageBoost:
					_critDamageBonus += value;
					break;
				case BuffType.LifeStealBoost:
					_lifestealBonus += value;
					break;
				case BuffType.DodgeBoost:
					_dodgeBonus += value;
					break;
				case BuffType.Shield:
					_shieldValue += value;
					break;
				case BuffType.Invincible:
					_isInvincible = true;
					break;
				case BuffType.Freeze:
					_isFrozen = true;
					break;
				case BuffType.Stun:
					_isStunned = true;
					break;
				case BuffType.Silence:
					_isSilenced = true;
					break;
				case BuffType.Root:
					_isRooted = true;
					break;
				case BuffType.Slow:
					_slowMultiplier = Mathf.Min(_slowMultiplier, value);
					break;
				case BuffType.Weak:
					_weakMultiplier = Mathf.Min(_weakMultiplier, value);
					break;
			}
		}
		
		// 发送状态变化信号
		EmitSignal(nameof(StateChanged), "Invincible", _isInvincible);
		EmitSignal(nameof(StateChanged), "Frozen", _isFrozen);
		EmitSignal(nameof(StateChanged), "Stunned", _isStunned);
		EmitSignal(nameof(StateChanged), "Silenced", _isSilenced);
		EmitSignal(nameof(StateChanged), "Rooted", _isRooted);
		
		if (_shieldValue > 0)
			EmitSignal(nameof(ShieldChanged), _shieldValue);
	}
	
	// ===== 属性获取方法 =====
	
	public float GetAttackBonus() => _attackBonus;
	public float GetDefenseBonus() => _defenseBonus;
	public float GetHealthBonus() => _healthBonus;
	public float GetMagicBonus() => _magicBonus;
	public float GetSpeedBonus() => _speedBonus;
	public float GetCritRateBonus() => _critRateBonus;
	public float GetCritDamageBonus() => _critDamageBonus;
	public float GetLifestealBonus() => _lifestealBonus;
	public float GetDodgeBonus() => _dodgeBonus;
	public float GetShieldValue() => _shieldValue;
	
	public bool IsInvincible() => _isInvincible;
	public bool IsFrozen() => _isFrozen;
	public bool IsStunned() => _isStunned;
	public bool IsSilenced() => _isSilenced;
	public bool IsRooted() => _isRooted;
	public float GetSlowMultiplier() => _slowMultiplier;
	public float GetWeakMultiplier() => _weakMultiplier;
	
	// 护盾吸收伤害
	public float AbsorbShieldDamage(float damage)
	{
		if (_shieldValue <= 0) return damage;
		
		float absorbed = Mathf.Min(damage, _shieldValue);
		_shieldValue -= absorbed;
		float remaining = damage - absorbed;
		
		EmitSignal(nameof(ShieldChanged), _shieldValue);
		
		return remaining;
	}
	
	// 获取统计数据
	public PlayerBuffData GetBuffData()
	{
		return _playerBuffData;
	}
	
	// 获取buff数量
	public int GetBuffCount()
	{
		return _activeBuffs.Count;
	}
	
	// 获取活跃buff数量（按ID去重）
	public int GetUniqueBuffCount()
	{
		HashSet<string> uniqueIds = new HashSet<string>();
		foreach (var buff in _activeBuffs)
		{
			uniqueIds.Add(buff.Info.Id);
		}
		return uniqueIds.Count;
	}
}
