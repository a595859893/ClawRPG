using Godot;
using System;
using System.Collections.Generic;

public class CombatRatingData
{
	// Rating grades
	public enum RatingGrade
	{
		F, D, C, B, A, S, SS, SSS
	}
	
	// Combat rating record
	public class CombatRatingRecord
	{
		public int battleId;
		public RatingGrade grade;
		public int score;
		public int stars;
		public float timeTaken;
		public int damageDealt;
		public int damageTaken;
		public int enemiesDefeated;
		public int criticalHits;
		public int perfectDodges;
		public bool noDamage;
		public bool noHitsTaken;
		public int comboCount;
		public int goldReward;
		public int expReward;
		public long timestamp;
	}
	
	// Grade requirements
	public class GradeRequirement
	{
		public RatingGrade grade;
		public int minScore;
		public int minStars;
		public float maxTime;
		public float damageTakenMultiplier;
		public bool requireNoDamage;
		public bool requireNoHitsTaken;
		public int goldBonus;
		public int expBonus;
	}
	
	// Statistics
	public int totalBattles;
	public int totalScore;
	public int highestScore;
	public RatingGrade highestGrade;
	public int totalStars;
	public int sssCount;
	public int noDamageCount;
	public float averageGrade;
	
	// Rating history
	public List<CombatRatingRecord> ratingHistory = new List<CombatRatingRecord>();
	
	// Session stats
	public int sessionBattles;
	public int sessionScore;
	public int sessionStars;
}
