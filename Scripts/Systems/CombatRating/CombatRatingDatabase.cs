using Godot;
using System;
using System.Collections.Generic;

public class CombatRatingDatabase
{
	// Grade configurations
	private static readonly Dictionary<CombatRatingData.RatingGrade, CombatRatingData.GradeRequirement> GradeRequirements = new Dictionary<CombatRatingData.RatingGrade, CombatRatingData.GradeRequirement>
	{
		// SSS - Perfect (no damage, fast time, high combo)
		{
			CombatRatingData.RatingGrade.SSS, new CombatRatingData.GradeRequirement
			{
				grade = CombatRatingData.RatingGrade.SSS,
				minScore = 10000,
				minStars = 5,
				maxTime = 30f,
				damageTakenMultiplier = 0f,
				requireNoDamage = true,
				requireNoHitsTaken = true,
				goldBonus = 500,
				expBonus = 200
			}
		},
		// SS - Excellent (minimal damage, good time)
		{
			CombatRatingData.RatingGrade.SS, new CombatRatingData.GradeRequirement
			{
				grade = CombatRatingData.RatingGrade.SS,
				minScore = 8000,
				minStars = 4,
				maxTime = 45f,
				damageTakenMultiplier = 0.1f,
				requireNoDamage = false,
				requireNoHitsTaken = false,
				goldBonus = 300,
				expBonus = 150
			}
		},
		// S - Great (low damage, decent time)
		{
			CombatRatingData.RatingGrade.S, new CombatRatingData.GradeRequirement
			{
				grade = CombatRatingData.RatingGrade.S,
				minScore = 6000,
				minStars = 3,
				maxTime = 60f,
				damageTakenMultiplier = 0.25f,
				requireNoDamage = false,
				requireNoHitsTaken = false,
				goldBonus = 200,
				expBonus = 100
			}
		},
		// A - Good (moderate performance)
		{
			CombatRatingData.RatingGrade.A, new CombatRatingData.GradeRequirement
			{
				grade = CombatRatingData.RatingGrade.A,
				minScore = 4000,
				minStars = 3,
				maxTime = 90f,
				damageTakenMultiplier = 0.5f,
				requireNoDamage = false,
				requireNoHitsTaken = false,
				goldBonus = 100,
				expBonus = 50
			}
		},
		// B - Average (decent performance)
		{
			CombatRatingData.RatingGrade.B, new CombatRatingData.GradeRequirement
			{
				grade = CombatRatingData.RatingGrade.B,
				minScore = 2500,
				minStars = 2,
				maxTime = 120f,
				damageTakenMultiplier = 0.75f,
				requireNoDamage = false,
				requireNoHitsTaken = false,
				goldBonus = 50,
				expBonus = 25
			}
		},
		// C - Below Average
		{
			CombatRatingData.RatingGrade.C, new CombatRatingData.GradeRequirement
			{
				grade = CombatRatingData.RatingGrade.C,
				minScore = 1000,
				minStars = 1,
				maxTime = 180f,
				damageTakenMultiplier = 1.0f,
				requireNoDamage = false,
				requireNoHitsTaken = false,
				goldBonus = 20,
				expBonus = 10
			}
		},
		// D - Poor
		{
			CombatRatingData.RatingGrade.D, new CombatRatingData.GradeRequirement
			{
				grade = CombatRatingData.RatingGrade.D,
				minScore = 500,
				minStars = 0,
				maxTime = 240f,
				damageTakenMultiplier = 1.5f,
				requireNoDamage = false,
				requireNoHitsTaken = false,
				goldBonus = 10,
				expBonus = 5
			}
		},
		// F - Failed
		{
			CombatRatingData.RatingGrade.F, new CombatRatingData.GradeRequirement
			{
				grade = CombatRatingData.RatingGrade.F,
				minScore = 0,
				minStars = 0,
				maxTime = 999f,
				damageTakenMultiplier = 2.0f,
				requireNoDamage = false,
				requireNoHitsTaken = false,
				goldBonus = 0,
				expBonus = 0
			}
		}
	};
	
	// Score multipliers
	private static readonly Dictionary<string, float> ScoreMultipliers = new Dictionary<string, float>
	{
		{ "damage_dealt", 1.0f },
		{ "enemy_defeated", 50f },
		{ "critical_hit", 25f },
		{ "perfect_dodge", 30f },
		{ "no_damage", 500f },
		{ "no_hits_taken", 300f },
		{ "fast_time_bonus", 10f }, // per second under 60s
		{ "combo_bonus", 5f }, // per combo hit
		{ "elemental_reaction", 40f },
		{ "boss_defeated", 200f },
		{ "elite_defeated", 100f }
	};
	
	// Star thresholds
	private static readonly int[] StarThresholds = { 0, 500, 1500, 3000, 5000, 8000 };
	
	// Grade colors
	private static readonly Dictionary<CombatRatingData.RatingGrade, Color> GradeColors = new Dictionary<CombatRatingData.RatingGrade, Color>
	{
		{ CombatRatingData.RatingGrade.F, new Color(0.5f, 0.5f, 0.5f) },    // Gray
		{ CombatRatingData.RatingGrade.D, new Color(0.7f, 0.3f, 0.3f) },    // Dark Red
		{ CombatRatingData.RatingGrade.C, new Color(0.8f, 0.5f, 0.2f) },    // Orange
		{ CombatRatingData.RatingGrade.B, new Color(0.2f, 0.6f, 0.2f) },    // Green
		{ CombatRatingData.RatingGrade.A, new Color(0.2f, 0.4f, 0.8f) },    // Blue
		{ CombatRatingData.RatingGrade.S, new Color(0.6f, 0.2f, 0.8f) },    // Purple
		{ CombatRatingData.RatingGrade.SS, new Color(1f, 0.6f, 0f) },      // Gold
		{ CombatRatingData.RatingGrade.SSS, new Color(1f, 0.84f, 0f) }       // Bright Gold
	};
	
	// Grade names
	private static readonly Dictionary<CombatRatingData.RatingGrade, string> GradeNames = new Dictionary<CombatRatingData.RatingGrade, string>
	{
		{ CombatRatingData.RatingGrade.F, "Failed" },
		{ CombatRatingData.RatingGrade.D, "Poor" },
		{ CombatRatingData.RatingGrade.C, "Average" },
		{ CombatRatingData.RatingGrade.B, "Good" },
		{ CombatRatingData.RatingGrade.A, "Great" },
		{ CombatRatingData.RatingGrade.S, "Superb" },
		{ CombatRatingData.RatingGrade.SS, "Excellent" },
		{ CombatRatingData.RatingGrade.SSS, "Perfect" }
	};
	
	public static CombatRatingData.GradeRequirement GetGradeRequirement(CombatRatingData.RatingGrade grade)
	{
		if (GradeRequirements.ContainsKey(grade))
			return GradeRequirements[grade];
		return GradeRequirements[CombatRatingData.RatingGrade.F];
	}
	
	public static float GetScoreMultiplier(string type)
	{
		if (ScoreMultipliers.ContainsKey(type))
			return ScoreMultipliers[type];
		return 1.0f;
	}
	
	public static int GetStarCount(int score)
	{
		for (int i = StarThresholds.Length - 1; i >= 0; i--)
		{
			if (score >= StarThresholds[i])
				return i;
		}
		return 0;
	}
	
	public static Color GetGradeColor(CombatRatingData.RatingGrade grade)
	{
		if (GradeColors.ContainsKey(grade))
			return GradeColors[grade];
		return Colors.White;
	}
	
	public static string GetGradeName(CombatRatingData.RatingGrade grade)
	{
		if (GradeNames.ContainsKey(grade))
			return GradeNames[grade];
		return "Unknown";
	}
	
	public static CombatRatingData.RatingGrade CalculateGrade(int score, int stars, float time, int damageTaken, bool noDamage, bool noHitsTaken)
	{
		// Check from highest to lowest
		CombatRatingData.RatingGrade[] grades = {
			CombatRatingData.RatingGrade.SSS,
			CombatRatingData.RatingGrade.SS,
			CombatRatingData.RatingGrade.S,
			CombatRatingData.RatingGrade.A,
			CombatRatingData.RatingGrade.B,
			CombatRatingData.RatingGrade.C,
			CombatRatingData.RatingGrade.D,
			CombatRatingData.RatingGrade.F
		};
		
		foreach (var grade in grades)
		{
			var req = GetGradeRequirement(grade);
			if (score >= req.minScore && stars >= req.minStars && time <= req.maxTime)
			{
				if (req.requireNoDamage && !noDamage) continue;
				if (req.requireNoHitsTaken && !noHitsTaken) continue;
				return grade;
			}
		}
		
		return CombatRatingData.RatingGrade.F;
	}
}
