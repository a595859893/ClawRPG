using Godot;
using System;
using System.Collections.Generic;

public class CombatSkillCooldownSystem : BaseSystem
{
	private static CombatSkillCooldownSystem _instance;
	public static new CombatSkillCooldownSystem Instance
	{
		get => _instance;
		private set => _instance = value;
	}
	
	// 技能冷却数据
	private CombatSkillCooldownData.PlayerSkillCooldownData _playerCooldownData;
	
	// 信号系统
	[Signal]
    public delegate void CooldownStarted(string skillId, string skillName, float cooldownTime);
	[Signal]
    public delegate void CooldownUpdated(string skillId, float remainingTime);
	[Signal]
    public delegate void CooldownReady(string skillId, string skillName);
	[Signal]
    public delegate void SkillUsed(string skillId, string skillName);
	
	public override void _Ready()
	{
		base._Ready();
		Instance = this;
		_playerCooldownData = new CombatSkillCooldownData.PlayerSkillCooldownData();
		LoadData();
	}
	
	protected override void Initialize()
	{
		GD.Print("[CombatSkillCooldownSystem] Initialized");
	}
	
	/// <summary>
	/// Export save data
	/// </summary>
	public override Dictionary ExportSaveData()
	{
		return GetSaveData();
	}
	
	/// <summary>
	/// Import save data
	/// </summary>
	public override void ImportSaveData(Dictionary data)
	{
		if (data != null)
		{
			LoadSaveData(data);
		}
	}
	
	/// <summary>
	/// 开始技能冷却
	/// </summary>
	public void StartCooldown(string skillId, string skillName, float cooldownTime)
	{
		if (_playerCooldownData.ActiveCooldowns.ContainsKey(skillId))
		{
			// 更新现有冷却
			_playerCooldownData.ActiveCooldowns[skillId].CurrentCooldown = cooldownTime;
			_playerCooldownData.ActiveCooldowns[skillId].MaxCooldown = cooldownTime;
		}
		else
		{
			// 创建新冷却
			var cooldown = new CombatSkillCooldownData.SkillCooldown
			{
				SkillId = skillId,
				SkillName = skillName,
				MaxCooldown = cooldownTime,
				CurrentCooldown = cooldownTime
			};
			_playerCooldownData.ActiveCooldowns[skillId] = cooldown;
		}
		
		// 统计
		if (!_playerCooldownData.SkillUsageCount.ContainsKey(skillId))
		{
			_playerCooldownData.SkillUsageCount[skillId] = 0;
		}
		_playerCooldownData.SkillUsageCount[skillId]++;
		_playerCooldownData.TotalSkillsUsed++;
		_playerCooldownData.TotalCooldownTime += (int)cooldownTime;
		
		CooldownStarted?.Invoke(skillId, skillName, cooldownTime);
	}
	
	/// <summary>
	/// 玩家使用技能（手动调用）
	/// </summary>
	public void UseSkill(string skillId, string skillName, float cooldownTime)
	{
		StartCooldown(skillId, skillName, cooldownTime);
		SkillUsed?.Invoke(skillId, skillName);
	}
	
	/// <summary>
	/// 更新冷却时间
	/// </summary>
	public void _Process(float delta)
	{
		var readySkills = new List<string>();
		
		foreach (var kvp in _playerCooldownData.ActiveCooldowns)
		{
			if (kvp.Value.CurrentCooldown > 0)
			{
				kvp.Value.CurrentCooldown -= delta;
				if (kvp.Value.CurrentCooldown <= 0)
				{
					kvp.Value.CurrentCooldown = 0;
					readySkills.Add(kvp.Key);
				}
				else
				{
					CooldownUpdated?.Invoke(kvp.Key, kvp.Value.CurrentCooldown);
				}
			}
		}
		
		// 触发技能就绪信号
		foreach (var skillId in readySkills)
		{
			var skill = _playerCooldownData.ActiveCooldowns[skillId];
			CooldownReady?.Invoke(skillId, skill.SkillName);
		}
	}
	
	/// <summary>
	/// 获取技能冷却状态
	/// </summary>
	public CombatSkillCooldownData.SkillCooldown GetSkillCooldown(string skillId)
	{
		if (_playerCooldownData.ActiveCooldowns.TryGetValue(skillId, out var cooldown))
		{
			return cooldown;
		}
		return null;
	}
	
	/// <summary>
	/// 获取所有技能冷却状态
	/// </summary>
	public Dictionary<string, CombatSkillCooldownData.SkillCooldown> GetAllCooldowns()
	{
		return _playerCooldownData.ActiveCooldowns;
	}
	
	/// <summary>
	/// 检查技能是否就绪
	/// </summary>
	public bool IsSkillReady(string skillId)
	{
		if (_playerCooldownData.ActiveCooldowns.TryGetValue(skillId, out var cooldown))
		{
			return cooldown.IsReady;
		}
		return true; // 未使用过的技能视为就绪
	}
	
	/// <summary>
	/// 获取统计信息
	/// </summary>
	public Dictionary<string, object> GetStatistics()
	{
		return new Dictionary<string, object>
		{
			{ "totalSkillsUsed", _playerCooldownData.TotalSkillsUsed },
			{ "totalCooldownTime", _playerCooldownData.TotalCooldownTime },
			{ "activeCooldowns", _playerCooldownData.ActiveCooldowns.Count },
			{ "readySkills", GetReadySkillCount() }
		};
	}
	
	/// <summary>
	/// 获取就绪技能数量
	/// </summary>
	public int GetReadySkillCount()
	{
		int count = 0;
		foreach (var kvp in _playerCooldownData.ActiveCooldowns)
		{
			if (kvp.Value.IsReady) count++;
		}
		return count;
	}
	
	/// <summary>
	/// 清除冷却数据
	/// </summary>
	public void ClearCooldowns()
	{
		_playerCooldownData.ActiveCooldowns.Clear();
	}
	
	/// <summary>
	/// 存档数据
	/// </summary>
	public Dictionary<string, object> GetSaveData()
	{
		var data = new Dictionary<string, object>();
		
		var cooldownList = new List<Dictionary<string, object>>();
		foreach (var kvp in _playerCooldownData.ActiveCooldowns)
		{
			cooldownList.Add(new Dictionary<string, object>
			{
				{ "skillId", kvp.Value.SkillId },
				{ "skillName", kvp.Value.SkillName },
				{ "maxCooldown", kvp.Value.MaxCooldown },
				{ "currentCooldown", kvp.Value.CurrentCooldown }
			});
		}
		
		data["cooldowns"] = cooldownList;
		data["totalSkillsUsed"] = _playerCooldownData.TotalSkillsUsed;
		data["totalCooldownTime"] = _playerCooldownData.TotalCooldownTime;
		
		var usageList = new List<Dictionary<string, object>>();
		foreach (var kvp in _playerCooldownData.SkillUsageCount)
		{
			usageList.Add(new Dictionary<string, object>
			{
				{ "skillId", kvp.Key },
				{ "count", kvp.Value }
			});
		}
		data["skillUsage"] = usageList;
		
		return data;
	}
	
	/// <summary>
	/// 加载存档数据
	/// </summary>
	public void LoadSaveData(Dictionary<string, object> data)
	{
		if (data == null) return;
		
		ClearCooldowns();
		
		if (data.ContainsKey("cooldowns"))
		{
			var cooldownList = (List<object>)data["cooldowns"];
			foreach (Dictionary<string, object> cooldownData in cooldownList)
			{
				var cooldown = new CombatSkillCooldownData.SkillCooldown
				{
					SkillId = cooldownData["skillId"].ToString(),
					SkillName = cooldownData["skillName"].ToString(),
					MaxCooldown = (float)cooldownData["maxCooldown"],
					CurrentCooldown = (float)cooldownData["currentCooldown"]
				};
				_playerCooldownData.ActiveCooldowns[cooldown.SkillId] = cooldown;
			}
		}
		
		if (data.ContainsKey("totalSkillsUsed"))
		{
			_playerCooldownData.TotalSkillsUsed = (int)data["totalSkillsUsed"];
		}
		
		if (data.ContainsKey("totalCooldownTime"))
		{
			_playerCooldownData.TotalCooldownTime = (int)data["totalCooldownTime"];
		}
		
		if (data.ContainsKey("skillUsage"))
		{
			var usageList = (List<object>)data["skillUsage"];
			foreach (Dictionary<string, object> usageData in usageList)
			{
				_playerCooldownData.SkillUsageCount[usageData["skillId"].ToString()] = (int)usageData["count"];
			}
		}
	}
}
