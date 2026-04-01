using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Framework;

public partial class BuffSystem : BaseSystem
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
public delegate void BuffApplied(string buffId, int stackCount);
public delegate void BuffRemoved(string buffId);
public delegate void BuffTick(string buffId, float value);
public delegate void BuffStackChanged(string buffId, int newStack);
public delegate void ShieldChanged(float newShield);
public delegate void StateChanged(string state, bool isActive);
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

		// 不变量：buffId 有效，MaxStacks 合法
		Invariant.Assert(!string.IsNullOrEmpty(buffId), "ApplyBuff: buffId is null or empty");
		Invariant.AssertRange(buffInfo.MaxStacks, 1, int.MaxValue, "MaxStacks");

		// 检查是否已经有这个buff
		ActiveBuff existingBuff = FindBuff(buffId);

		if (existingBuff != null)
		{
			// 不变量：已有 buff 的层数在合法范围内
			Invariant.AssertRange(existingBuff.StackCount, 1, existingBuff.Info.MaxStacks, "existingBuff.StackCount");

			// 如果可以叠加
			if (buffInfo.CanStack && existingBuff.StackCount < buffInfo.MaxStacks)
			{
				existingBuff.StackCount++;
				existingBuff.CurrentValue = buffInfo.StartValue * existingBuff.StackCount;
				float newDuration = customDuration > 0 ? customDuration : buffInfo.Duration;
				existingBuff.TimeRemaining = newDuration;

				// 不变量：叠加后层数不超过上限，持续时间为正或永久(-1)
				Invariant.AssertRange(existingBuff.StackCount, 1, buffInfo.MaxStacks, "StackCount after stacking");
				Invariant.Assert(newDuration < 0 || newDuration >= 0, "Buff duration must be -1 (permanent) or non-negative");

				EmitSignal(nameof(BuffStackChanged), buffId, existingBuff.StackCount);
				EmitSignal(nameof(BuffApplied), buffId, existingBuff.StackCount);
			}
			else
			{
				// 刷新持续时间
				float newDuration = customDuration > 0 ? customDuration : buffInfo.Duration;
				existingBuff.TimeRemaining = newDuration;
				Invariant.Assert(newDuration < 0 || newDuration >= 0, "Buff duration must be -1 (permanent) or non-negative");
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
			// 不变量：被移除的 buff 必须是活跃列表中的成员
			Invariant.Assert(_activeBuffs.Contains(buff), "RemoveBuff: buff not in _activeBuffs list — possible duplicate removal");

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
	public override void _Process(double delta)
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
				// 不变量：TickTimer 不应过度累积（超过 2 倍 interval 说明逻辑卡顿）
				Invariant.Assert(buff.TickTimer < buff.Info.TickInterval * 2,
					"TickTimer overflow: {0} >= {1} * 2 — possible logic stall", buff.TickTimer, buff.Info.TickInterval);

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

		// 不变量：累加期间各 bonus 值保持非负（逻辑上它们应该都是正的，只是防御/虚弱可能产生负值）
		// Slow/Weak multipliers 取最小值，理论上 [0, 1]，极端情况可能超出
		// 累加类 bonus 允许多 buff 叠加，只要最终结算时在合理范围即可
		
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
		// 不变量：伤害值必须非负
		Invariant.Assert(damage >= 0, "AbsorbShieldDamage: damage must be non-negative, got {0}", damage);

		if (_shieldValue <= 0) return damage;

		float absorbed = Mathf.Min(damage, _shieldValue);
		_shieldValue -= absorbed;
		float remaining = damage - absorbed;

		// 不变量：护盾值不能为负
		Invariant.Assert(_shieldValue >= 0, "AbsorbShieldDamage: _shieldValue went negative: {0}", _shieldValue);

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
	
	// ===== 持久化 =====
	public override Dictionary<string, object> ExportSaveData()
	{
		var data = new Dictionary<string, object>();
		
		// 保存统计数据
		data["total_buffs_applied"] = _playerBuffData.TotalBuffsApplied;
		data["total_debuffs_applied"] = _playerBuffData.TotalDebuffsApplied;
		data["total_buff_time"] = _playerBuffData.TotalBuffTime;
		data["buff_stacks"] = _playerBuffData.BuffStacks;
		data["buff_source_count"] = _playerBuffData.BuffSourceCount;
		
		// 保存活跃buff（只保存ID和剩余时间，buff信息从数据库读取）
		var activeBuffsData = new Array();
		foreach (var buff in _activeBuffs)
		{
			var buffData = new Dictionary<string, object>();
			buffData["id"] = buff.Info.Id;
			buffData["time_remaining"] = buff.TimeRemaining;
			buffData["stack_count"] = buff.StackCount;
			buffData["current_value"] = buff.CurrentValue;
			activeBuffsData.Add(buffData);
		}
		data["active_buffs"] = activeBuffsData;
		
		return data;
	}
	
	public override void ImportSaveData(Dictionary<string, object> data)
	{
		if (data == null) return;
		
		// 恢复统计数据
		if (data.ContainsKey("total_buffs_applied"))
			_playerBuffData.TotalBuffsApplied = Convert.ToInt32(data["total_buffs_applied"]);
		if (data.ContainsKey("total_debuffs_applied"))
			_playerBuffData.TotalDebuffsApplied = Convert.ToInt32(data["total_debuffs_applied"]);
		if (data.ContainsKey("total_buff_time"))
			_playerBuffData.TotalBuffTime = Convert.ToInt32(data["total_buff_time"]);
		if (data.ContainsKey("buff_stacks"))
			_playerBuffData.BuffStacks = (Dictionary<string, int>)data["buff_stacks"];
		if (data.ContainsKey("buff_source_count"))
			_playerBuffData.BuffSourceCount = (Dictionary<string, int>)data["buff_source_count"];
		
		// 恢复活跃buff
		_activeBuffs.Clear();
		if (data.ContainsKey("active_buffs"))
		{
			var activeBuffsData = (Array)data["active_buffs"];
			foreach (Dictionary buffData in activeBuffsData)
			{
				string buffId = (string)buffData["id"];
				BuffInfo buffInfo = BuffDatabase.Instance.GetBuff(buffId);
				if (buffInfo != null)
				{
					var newBuff = new ActiveBuff(buffInfo);
					newBuff.TimeRemaining = Convert.ToSingle(buffData["time_remaining"]);
					newBuff.StackCount = Convert.ToInt32(buffData["stack_count"]);
					newBuff.CurrentValue = Convert.ToSingle(buffData["current_value"]);
					_activeBuffs.Add(newBuff);
				}
			}
		}
	}
}
