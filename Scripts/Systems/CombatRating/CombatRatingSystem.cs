using Godot;
using System;
using System.Collections.Generic;

public class CombatRatingSystem : BaseSystem
{
	private CombatRatingData data = new CombatRatingData();
	private int battleIdCounter = 0;
	
	// Real-time tracking
	private int currentDamageDealt;
	private int currentDamageTaken;
	private int currentEnemiesDefeated;
	private int currentCriticalHits;
	private int currentPerfectDodges;
	private int currentComboCount;
	private int currentMaxCombo;
	private float battleStartTime;
	private bool battleActive;
	private bool hasTakenDamage;
	private bool hasBeenHit;
	
	// Score components
	private int damageScore;
	private int enemyScore;
	private int criticalScore;
	private int dodgeScore;
	private int timeScore;
	private int comboScore;
	private int bonusScore;
	
	public override void _Ready()
	{
		LoadData();
		GD.Print("Combat Rating System initialized");
	}
	
	// Start tracking a new battle
	public void StartBattle()
	{
		battleStartTime = Time.GetTicksMsec() / 1000f;
		battleActive = true;
		ResetCurrentBattle();
	}
	
	private void ResetCurrentBattle()
	{
		currentDamageDealt = 0;
		currentDamageTaken = 0;
		currentEnemiesDefeated = 0;
		currentCriticalHits = 0;
		currentPerfectDodges = 0;
		currentComboCount = 0;
		currentMaxCombo = 0;
		hasTakenDamage = false;
		hasBeenHit = false;
		
		damageScore = 0;
		enemyScore = 0;
		criticalScore = 0;
		dodgeScore = 0;
		timeScore = 0;
		comboScore = 0;
		bonusScore = 0;
	}
	
	// Record damage dealt
	public void RecordDamageDealt(int damage, bool isCritical = false)
	{
		if (!battleActive) return;
		
		currentDamageDealt += damage;
		damageScore += (int)(damage * CombatRatingDatabase.GetScoreMultiplier("damage_dealt"));
		
		if (isCritical)
		{
			currentCriticalHits++;
			criticalScore += (int)(CombatRatingDatabase.GetScoreMultiplier("critical_hit"));
		}
	}
	
	// Record damage taken
	public void RecordDamageTaken(int damage)
	{
		if (!battleActive) return;
		
		currentDamageTaken += damage;
		hasTakenDamage = true;
		hasBeenHit = true;
	}
	
	// Record enemy defeated
	public void RecordEnemyDefeated(bool isElite = false, bool isBoss = false)
	{
		if (!battleActive) return;
		
		currentEnemiesDefeated++;
		
		if (isBoss)
		{
			enemyScore += (int)CombatRatingDatabase.GetScoreMultiplier("boss_defeated");
		}
		else if (isElite)
		{
			enemyScore += (int)CombatRatingDatabase.GetScoreMultiplier("elite_defeated");
		}
		else
		{
			enemyScore += (int)CombatRatingDatabase.GetScoreMultiplier("enemy_defeated");
		}
	}
	
	// Record perfect dodge
	public void RecordPerfectDodge()
	{
		if (!battleActive) return;
		
		currentPerfectDodges++;
		dodgeScore += (int)CombatRatingDatabase.GetScoreMultiplier("perfect_dodge");
	}
	
	// Record combo
	public void RecordComboHit()
	{
		if (!battleActive) return;
		
		currentComboCount++;
		if (currentComboCount > currentMaxCombo)
			currentMaxCombo = currentComboCount;
		
		comboScore += (int)CombatRatingDatabase.GetScoreMultiplier("combo_bonus");
	}
	
	// Reset combo (when hit or timeout)
	public void ResetCombo()
	{
		currentComboCount = 0;
	}
	
	// Record elemental reaction
	public void RecordElementalReaction()
	{
		if (!battleActive) return;
		
		bonusScore += (int)CombatRatingDatabase.GetScoreMultiplier("elemental_reaction");
	}
	
	// End battle and calculate rating
	public CombatRatingData.CombatRatingRecord EndBattle()
	{
		if (!battleActive) return null;
		
		battleActive = false;
		float battleTime = Time.GetTicksMsec() / 1000f - battleStartTime;
		
		// Calculate total score
		int totalScore = damageScore + enemyScore + criticalScore + dodgeScore + timeScore + comboScore + bonusScore;
		
		// Calculate time bonus (faster = more points)
		if (battleTime < 60f)
		{
			timeScore = (int)((60f - battleTime) * CombatRatingDatabase.GetScoreMultiplier("fast_time_bonus"));
			totalScore += timeScore;
		}
		
		// Check for no-damage bonus
		bool noDamage = !hasTakenDamage;
		bool noHitsTaken = !hasBeenHit;
		
		if (noDamage)
		{
			bonusScore += (int)CombatRatingDatabase.GetScoreMultiplier("no_damage");
			data.noDamageCount++;
		}
		
		if (noHitsTaken)
		{
			bonusScore += (int)CombatRatingDatabase.GetScoreMultiplier("no_hits_taken");
		}
		
		totalScore += bonusScore;
		
		// Calculate stars
		int stars = CombatRatingDatabase.GetStarCount(totalScore);
		
		// Calculate grade
		CombatRatingData.RatingGrade grade = CombatRatingDatabase.CalculateGrade(
			totalScore, stars, battleTime, currentDamageTaken, noDamage, noHitsTaken);
		
		// Get grade requirements for rewards
		var requirement = CombatRatingDatabase.GetGradeRequirement(grade);
		
		// Calculate rewards
		int goldReward = requirement.goldBonus;
		int expReward = requirement.expBonus;
		
		// Bonus for high scores
		if (totalScore >= 5000) goldReward += 50;
		if (totalScore >= 8000) goldReward += 100;
		if (totalScore >= 10000) goldReward += 200;
		
		if (totalScore >= 5000) expReward += 25;
		if (totalScore >= 8000) expReward += 50;
		if (totalScore >= 10000) expReward += 100;
		
		// Create record
		CombatRatingData.CombatRatingRecord record = new CombatRatingData.CombatRatingRecord();
		record.battleId = ++battleIdCounter;
		record.grade = grade;
		record.score = totalScore;
		record.stars = stars;
		record.timeTaken = battleTime;
		record.damageDealt = currentDamageDealt;
		record.damageTaken = currentDamageTaken;
		record.enemiesDefeated = currentEnemiesDefeated;
		record.criticalHits = currentCriticalHits;
		record.perfectDodges = currentPerfectDodges;
		record.noDamage = noDamage;
		record.noHitsTaken = noHitsTaken;
		record.comboCount = currentMaxCombo;
		record.goldReward = goldReward;
		record.expReward = expReward;
		record.timestamp = Time.GetUnixTimeFromSystem();
		
		// Update statistics
		UpdateStatistics(record);
		
		// Add to history
		data.ratingHistory.Add(record);
		
		// Save data
		SaveData();
		
		return record;
	}
	
	private void UpdateStatistics(CombatRatingData.CombatRatingRecord record)
	{
		data.totalBattles++;
		data.totalScore += record.score;
		data.sessionBattles++;
		
		if (record.score > data.highestScore)
			data.highestScore = record.score;
		
		if (record.grade > data.highestGrade)
			data.highestGrade = record.grade;
		
		data.totalStars += record.stars;
		data.sessionScore += record.score;
		data.sessionStars += record.stars;
		
		if (record.grade == CombatRatingData.RatingGrade.SSS)
			data.sssCount++;
		
		// Calculate average grade
		float totalGradeValue = 0;
		foreach (var r in data.ratingHistory)
		{
			totalGradeValue += (int)r.grade;
		}
		data.averageGrade = totalGradeValue / data.ratingHistory.Count;
	}
	
	// Get current battle score (real-time)
	public int GetCurrentScore()
	{
		float battleTime = battleActive ? (Time.GetTicksMsec() / 1000f - battleStartTime) : 0;
		int timeBonus = battleActive && battleTime < 60f ? (int)((60f - battleTime) * CombatRatingDatabase.GetScoreMultiplier("fast_time_bonus")) : 0;
		
		int total = damageScore + enemyScore + criticalScore + dodgeScore + timeBonus + comboScore + bonusScore;
		
		if (!hasTakenDamage)
			total += (int)CombatRatingDatabase.GetScoreMultiplier("no_damage");
		if (!hasBeenHit)
			total += (int)CombatRatingDatabase.GetScoreMultiplier("no_hits_taken");
		
		return total;
	}
	
	// Get current stars (real-time)
	public int GetCurrentStars()
	{
		return CombatRatingDatabase.GetStarCount(GetCurrentScore());
	}
	
	// Get statistics
	public CombatRatingData GetStatistics()
	{
		return data;
	}
	
	// Get grade color
	public Color GetGradeColor(CombatRatingData.RatingGrade grade)
	{
		return CombatRatingDatabase.GetGradeColor(grade);
	}
	
	// Get grade name
	public string GetGradeName(CombatRatingData.RatingGrade grade)
	{
		return CombatRatingDatabase.GetGradeName(grade);
	}
	
	// Reset session statistics
	public void ResetSessionStats()
	{
		data.sessionBattles = 0;
		data.sessionScore = 0;
		data.sessionStars = 0;
	}
	
	// Clear all statistics
	public void ClearStatistics()
	{
		data = new CombatRatingData();
		SaveData();
	}
	
	// Save data to file
	private void SaveData()
	{
		string path = "user://combat_rating_data.json";
		FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
		if (file != null)
		{
			var json = new Json();
			string jsonString = json.Stringify(data.ToJson());
			file.StoreString(jsonString);
			file.Close();
		}
	}
	
	// Load data from file
	private void LoadData()
	{
		string path = "user://combat_rating_data.json";
		if (FileAccess.FileExists(path))
		{
			FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
			if (file != null)
			{
				string jsonString = file.GetAsText();
				var json = new Json();
				var result = json.Parse(jsonString);
				if (result == Error.Ok)
				{
					// Parse JSON data
					GD.Print("Combat rating data loaded");
				}
				file.Close();
			}
		}
	}
	
	// ===== 持久化 =====
	public override Dictionary ExportSaveData()
	{
		var exportData = new Dictionary();
		exportData["total_battles"] = data.totalBattles;
		exportData["total_score"] = data.totalScore;
		exportData["highest_score"] = data.highestScore;
		exportData["highest_grade"] = (int)data.highestGrade;
		exportData["total_stars"] = data.totalStars;
		exportData["sss_count"] = data.sssCount;
		exportData["no_damage_count"] = data.noDamageCount;
		exportData["average_grade"] = data.averageGrade;
		exportData["session_battles"] = data.sessionBattles;
		exportData["session_score"] = data.sessionScore;
		exportData["session_stars"] = data.sessionStars;
		
		// 保存历史记录（限制最近50条）
		var history = new Array();
		int count = 0;
		foreach (var record in data.ratingHistory)
		{
			if (count >= 50) break;
			var recordData = new Dictionary();
			recordData["battle_id"] = record.battleId;
			recordData["grade"] = (int)record.grade;
			recordData["score"] = record.score;
			recordData["stars"] = record.stars;
			recordData["time_taken"] = record.timeTaken;
			recordData["damage_dealt"] = record.damageDealt;
			recordData["damage_taken"] = record.damageTaken;
			recordData["enemies_defeated"] = record.enemiesDefeated;
			recordData["critical_hits"] = record.criticalHits;
			recordData["perfect_dodges"] = record.perfectDodges;
			recordData["no_damage"] = record.noDamage;
			recordData["no_hits_taken"] = record.noHitsTaken;
			recordData["combo_count"] = record.comboCount;
			recordData["gold_reward"] = record.goldReward;
			recordData["exp_reward"] = record.expReward;
			recordData["timestamp"] = record.timestamp;
			history.Add(recordData);
			count++;
		}
		exportData["rating_history"] = history;
		
		return exportData;
	}
	
	public override void ImportSaveData(Dictionary data)
	{
		if (data == null) return;
		
		if (data.ContainsKey("total_battles")) this.data.totalBattles = Convert.ToInt32(data["total_battles"]);
		if (data.ContainsKey("total_score")) this.data.totalScore = Convert.ToInt32(data["total_score"]);
		if (data.ContainsKey("highest_score")) this.data.highestScore = Convert.ToInt32(data["highest_score"]);
		if (data.ContainsKey("highest_grade")) this.data.highestGrade = (CombatRatingData.RatingGrade)Convert.ToInt32(data["highest_grade"]);
		if (data.ContainsKey("total_stars")) this.data.totalStars = Convert.ToInt32(data["total_stars"]);
		if (data.ContainsKey("sss_count")) this.data.sssCount = Convert.ToInt32(data["sss_count"]);
		if (data.ContainsKey("no_damage_count")) this.data.noDamageCount = Convert.ToInt32(data["no_damage_count"]);
		if (data.ContainsKey("average_grade")) this.data.averageGrade = Convert.ToSingle(data["average_grade"]);
		if (data.ContainsKey("session_battles")) this.data.sessionBattles = Convert.ToInt32(data["session_battles"]);
		if (data.ContainsKey("session_score")) this.data.sessionScore = Convert.ToInt32(data["session_score"]);
		if (data.ContainsKey("session_stars")) this.data.sessionStars = Convert.ToInt32(data["session_stars"]);
		
		// 恢复历史记录
		this.data.ratingHistory.Clear();
		if (data.ContainsKey("rating_history"))
		{
			var history = (Array)data["rating_history"];
			foreach (Dictionary recordData in history)
			{
				var record = new CombatRatingData.CombatRatingRecord();
				record.battleId = Convert.ToInt32(recordData["battle_id"]);
				record.grade = (CombatRatingData.RatingGrade)Convert.ToInt32(recordData["grade"]);
				record.score = Convert.ToInt32(recordData["score"]);
				record.stars = Convert.ToInt32(recordData["stars"]);
				record.timeTaken = Convert.ToSingle(recordData["time_taken"]);
				record.damageDealt = Convert.ToInt32(recordData["damage_dealt"]);
				record.damageTaken = Convert.ToInt32(recordData["damage_taken"]);
				record.enemiesDefeated = Convert.ToInt32(recordData["enemies_defeated"]);
				record.criticalHits = Convert.ToInt32(recordData["critical_hits"]);
				record.perfectDodges = Convert.ToInt32(recordData["perfect_dodges"]);
				record.noDamage = Convert.ToBoolean(recordData["no_damage"]);
				record.noHitsTaken = Convert.ToBoolean(recordData["no_hits_taken"]);
				record.comboCount = Convert.ToInt32(recordData["combo_count"]);
				record.goldReward = Convert.ToInt32(recordData["gold_reward"]);
				record.expReward = Convert.ToInt32(recordData["exp_reward"]);
				record.timestamp = Convert.ToInt64(recordData["timestamp"]);
				this.data.ratingHistory.Add(record);
			}
		}
	}
}
