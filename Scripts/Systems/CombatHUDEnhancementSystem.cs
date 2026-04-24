using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Combat;

/// <summary>
/// 战斗HUD增强系统 - 增强战斗界面显示效果
/// </summary>
public class CombatHUDEnhancementData
{
	public enum StatType
	{
		DamageDealt,
		DamageTaken,
		HealingDone,
		EnemiesKilled,
		CriticalHits,
		SkillsUsed,
		DodgeCount,
		BlockCount
	}
	
	public class CombatStat
	{
		public StatType Type { get; set; }
		public int Value { get; set; }
		public int ValuePerSecond { get; set; }
	}
	
	public class PlayerCombatStats
	{
		public int TotalDamageDealt { get; set; }
		public int TotalDamageTaken { get; set; }
		public int TotalHealingDone { get; set; }
		public int EnemiesKilled { get; set; }
		public int CriticalHits { get; set; }
		public int SkillsUsed { get; set; }
		public int DodgeCount { get; set; }
		public int BlockCount { get; set; }
		public float CombatDuration { get; set; }
		public int CurrentCombo { get; set; }
		public int MaxCombo { get; set; }
		public float DamagePerSecond => CombatDuration > 0 ? TotalDamageDealt / CombatDuration : 0;
		public float HealPerSecond => CombatDuration > 0 ? TotalHealingDone / CombatDuration : 0;
	}
	
	public class EnemyCombatInfo
	{
		public string EnemyId { get; set; }
		public string EnemyName { get; set; }
		public int DamageDealtToThis { get; set; }
		public float TimeSpentFighting { get; set; }
		public bool IsAlive { get; set; }
		public bool WasKilled { get; set; }
	}
	
	public class CombatSession
	{
		public PlayerCombatStats PlayerStats { get; set; } = new PlayerCombatStats();
		public List<EnemyCombatInfo> Enemies { get; set; } = new List<EnemyCombatInfo>();
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public bool IsActive { get; set; }
		public string CombatZone { get; set; }
	}
	
	public class CombatRating
	{
		public string Grade { get; set; } = "C";
		public float Score { get; set; }
		public float DamageEfficiency { get; set; }
		public float SurvivalRate { get; set; }
		public float SkillUsage { get; set; }
		public float CombatPace { get; set; }
	}
}

public partial class CombatHUDEnhancementSystem : BaseSystem
{
	public static CombatHUDEnhancementSystem Instance { get; private set; }
	
	private CombatHUDEnhancementData.CombatSession _currentSession;
	private Dictionary<string, CombatHUDEnhancementData.EnemyCombatInfo> _enemyInfoMap;
	private float _sessionStartTime;
	private int _currentCombo;
	private float _comboTimer;
	private const float COMBO_TIMEOUT = 3.0f;
	
	// Signals (Godot 4 compatible)
	public static event Action<string> CombatEnded;
	public static event Action<int> ComboChanged;
	public static event Action<string> MilestoneReached;
	
	public override void _Ready()
	{
		Instance = this;
		_enemyInfoMap = new Dictionary<string, CombatHUDEnhancementData.EnemyCombatInfo>();
		StartNewSession();
	}
	
	public void StartNewSession(string zone = "Unknown")
	{
		_currentSession = new CombatHUDEnhancementData.CombatSession
		{
			PlayerStats = new CombatHUDEnhancementData.PlayerCombatStats(),
			StartTime = DateTime.Now,
			IsActive = true,
			CombatZone = zone
		};
		_sessionStartTime = (float)Time.GetUnixTimeFromSystem();
		_enemyInfoMap.Clear();
		_currentCombo = 0;
		_comboTimer = 0;
	}
	
	public void EndSession()
	{
		if (_currentSession == null || !_currentSession.IsActive) return;
		
		_currentSession.EndTime = DateTime.Now;
		_currentSession.IsActive = false; 
		_currentSession.PlayerStats.CombatDuration = (float)(Time.GetUnixTimeFromSystem() - _sessionStartTime);
		_currentSession.PlayerStats.MaxCombo = Math.Max(_currentSession.PlayerStats.MaxCombo, _currentCombo);
		
			}
	
	public void RecordDamageDealt(int damage, bool isCritical, string enemyId, string enemyName)
	{
		if (_currentSession == null || !_currentSession.IsActive) return;
		
		_currentSession.PlayerStats.TotalDamageDealt += damage;
		if (isCritical)
		{
			_currentSession.PlayerStats.CriticalHits++;
		}
		
		// Update enemy info
		if (!_enemyInfoMap.ContainsKey(enemyId))
		{
			var enemyInfo = new CombatHUDEnhancementData.EnemyCombatInfo
			{
				EnemyId = enemyId,
				EnemyName = enemyName,
				IsAlive = true
			};
			_enemyInfoMap[enemyId] = enemyInfo;
			_currentSession.Enemies.Add(enemyInfo);
		}
		_enemyInfoMap[enemyId].DamageDealtToThis += damage;
		
		// Update combo
		UpdateCombo(damage);
	}
	
	public void RecordDamageTaken(int damage, string enemyId)
	{
		if (_currentSession == null || !_currentSession.IsActive) return;
		_currentSession.PlayerStats.TotalDamageTaken += damage;
	}
	
	public void RecordHealing(int healing)
	{
		if (_currentSession == null || !_currentSession.IsActive) return;
		_currentSession.PlayerStats.TotalHealingDone += healing;
	}
	
	public void RecordEnemyKilled(string enemyId)
	{
		if (_currentSession == null || !_currentSession.IsActive) return;
		
		_currentSession.PlayerStats.EnemiesKilled++;
		
		if (_enemyInfoMap.ContainsKey(enemyId))
		{
			_enemyInfoMap[enemyId].IsAlive = false; 
			_enemyInfoMap[enemyId].WasKilled = true;
		}
		
		// Check milestones
		CheckMilestones();
	}
	
	public void RecordSkillUsed()
	{
		if (_currentSession == null || !_currentSession.IsActive) return;
		_currentSession.PlayerStats.SkillsUsed++;
	}
	
	public void RecordDodge()
	{
		if (_currentSession == null || !_currentSession.IsActive) return;
		_currentSession.PlayerStats.DodgeCount++;
	}
	
	public void RecordBlock()
	{
		if (_currentSession == null || !_currentSession.IsActive) return;
		_currentSession.PlayerStats.BlockCount++;
	}
	
	private void UpdateCombo(int damage)
	{
		_comboTimer = COMBO_TIMEOUT;
		_currentCombo++;
		
		if (_currentCombo > _currentSession.PlayerStats.MaxCombo)
		{
			_currentSession.PlayerStats.MaxCombo = _currentCombo;
		}
		
		ComboChanged?.Invoke(_currentCombo);
		
		// Milestone combos
		if (_currentCombo == 10) MilestoneReached?.Invoke("Combo10");
		else if (_currentCombo == 25) MilestoneReached?.Invoke("Combo25");
		else if (_currentCombo == 50) MilestoneReached?.Invoke("Combo50");
		else if (_currentCombo == 100) MilestoneReached?.Invoke("Combo100");
	}
	
	private void CheckMilestones()
	{
		int kills = _currentSession.PlayerStats.EnemiesKilled;
		if (kills == 5) MilestoneReached?.Invoke("Kill5");
		else if (kills == 10) MilestoneReached?.Invoke("Kill10");
		else if (kills == 25) MilestoneReached?.Invoke("Kill25");
		else if (kills == 50) MilestoneReached.Emit("Kill50");
	}
	
	public override void _Process(double delta)
	{
		if (_currentSession == null || !_currentSession.IsActive) return;
		
		// Update combo timer
		if (_comboTimer > 0)
		{
			_comboTimer -= delta;
			if (_comboTimer <= 0)
			{
				_currentCombo = 0;
				ComboChanged.Emit(0);
			}
		}
		
		// Update combat duration
		_currentSession.PlayerStats.CombatDuration = (float)(Time.GetUnixTimeFromSystem() - _sessionStartTime);
	}
	
	private CombatHUDEnhancementData.CombatRating CalculateCombatRating()
	{
		var stats = _currentSession.PlayerStats;
		float score = 0;
		
		// Damage efficiency: damage dealt vs damage taken
		float damageEfficiency = stats.TotalDamageTaken > 0 
			? (float)stats.TotalDamageDealt / stats.TotalDamageTaken 
			: stats.TotalDamageDealt > 0 ? 5.0f : 0;
		damageEfficiency = Math.Min(damageEfficiency, 5.0f);
		
		// Survival rate: healing vs damage taken
		float survivalRate = stats.TotalDamageTaken > 0 
			? Math.Min((float)stats.TotalHealingDone / stats.TotalDamageTaken, 1.0f)
			: 1.0f;
		
		// Skill usage: skills used per enemy killed
		float skillUsage = stats.EnemiesKilled > 0 
			? (float)stats.SkillsUsed / stats.EnemiesKilled 
			: stats.SkillsUsed;
		skillUsage = Math.Min(skillUsage, 3.0f);
		
		// Combat pace: enemies killed per minute
		float combatPace = stats.CombatDuration > 0 
			? (stats.EnemiesKilled / stats.CombatDuration) * 60 
			: 0;
		combatPace = Math.Min(combatPace, 10.0f);
		
		// Calculate overall score
		score = damageEfficiency * 20 + survivalRate * 20 + skillUsage * 10 + combatPace * 5;
		score += stats.CriticalHits * 2;
		score += stats.DodgeCount;
		score += stats.BlockCount;
		
		// Determine grade
		string grade;
		if (score >= 150) grade = "S";
		else if (score >= 100) grade = "A";
		else if (score >= 70) grade = "B";
		else if (score >= 40) grade = "C";
		else grade = "D";
		
		return new CombatHUDEnhancementData.CombatRating
		{
			Grade = grade,
			Score = score,
			DamageEfficiency = damageEfficiency,
			SurvivalRate = survivalRate,
			SkillUsage = skillUsage,
			CombatPace = combatPace
		};
	}
	
	public CombatHUDEnhancementData.PlayerCombatStats GetCurrentStats()
	{
		return _currentSession?.PlayerStats ?? new CombatHUDEnhancementData.PlayerCombatStats();
	}
	
	public int GetCurrentCombo()
	{
		return _currentCombo;
	}
	
	public List<CombatHUDEnhancementData.EnemyCombatInfo> GetEnemyInfoList()
	{
		return _currentSession?.Enemies ?? new List<CombatHUDEnhancementData.EnemyCombatInfo>();
	}
	
	public bool IsInCombat()
	{
		return _currentSession?.IsActive ?? false;
	}
	
	/// <summary>
	/// Export save data for persistence
	/// </summary>
	public override Dictionary<string, object> ExportSaveData()
	{
		var data = new Dictionary<string, object>();
		data["session_start_time"] = _sessionStartTime;
		// Note: Current session data is runtime state, not persisted
		return data;
	}

	/// <summary>
	/// Import save data from persistence
	/// </summary>
	public override void ImportSaveData(Dictionary<string, object> data)
	{
		if (data == null) return;
		
		if (data.ContainsKey("session_start_time"))
		{
			_sessionStartTime = Convert.ToSingle(data["session_start_time"]);
		}
	}
	
	public Dictionary<string, object> GetSaveData()
	{
		var data = new Dictionary<string, object>();
		data["currentSession"] = _currentSession;
		data["sessionStartTime"] = _sessionStartTime;
		return data;
	}
	
	public void LoadSaveData(Dictionary<string, object> data)
	{
		if (data == null) return;
		
		if (data.ContainsKey("sessionStartTime"))
		{
			_sessionStartTime = Convert.ToSingle(data["sessionStartTime"]);
		}
	}
}
