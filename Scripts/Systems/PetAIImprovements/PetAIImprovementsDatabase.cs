// Pet AI Improvements Database
// Configuration for personality types, behaviors, and learning parameters

using Godot;
using System.Collections.Generic;

#pragma warning disable CS8618 // Non-nullable field is uninitialized

public partial class PetAIImprovementsDatabase : Resource
{
	// Personality type configurations
	public Dictionary<int, Dictionary> PersonalityConfigs { get; set; } = new Dictionary<int, Dictionary>
	{
		{ 0, new Dictionary {  // AGGRESSIVE
			{ "name", "Aggressive" },
			{ "description", "Pets that focus on attacking enemies with high damage output" },
			{ "aggression_modifier", 1.5 },
			{ "defense_modifier", 0.8 },
			{ "curiosity_level", 0.3 },
			{ "energy_drain", 0.05 },
			{ "preferred_tactics", new string[] { "attack", "chase", "pursue" } }
		} },
		{ 1, new Dictionary {  // DEFENSIVE
			{ "name", "Defensive" },
			{ "description", "Pets that protect the player and prioritize defense" },
			{ "aggression_modifier", 0.7 },
			{ "defense_modifier", 1.5 },
			{ "curiosity_level", 0.4 },
			{ "energy_drain", 0.02 },
			{ "preferred_tactics", new string[] { "defend", "block", "protect" } }
		} },
		{ 2, new Dictionary {  // SUPPORTIVE
			{ "name", "Supportive" },
			{ "description", "Pets that heal and support the player" },
			{ "aggression_modifier", 0.8 },
			{ "defense_modifier", 1.0 },
			{ "curiosity_level", 0.5 },
			{ "energy_drain", 0.03 },
			{ "preferred_tactics", new string[] { "heal", "buff", "support" } }
		} },
		{ 3, new Dictionary {  // CURIOUS
			{ "name", "Curious" },
			{ "description", "Pets that explore and discover hidden things" },
			{ "aggression_modifier", 1.0 },
			{ "defense_modifier", 1.0 },
			{ "curiosity_level", 0.9 },
			{ "energy_drain", 0.04 },
			{ "preferred_tactics", new string[] { "explore", "discover", "investigate" } }
		} },
		{ 4, new Dictionary {  // LAZY
			{ "name", "Lazy" },
			{ "description", "Pets that prefer resting but can act when needed" },
			{ "aggression_modifier", 0.5 },
			{ "defense_modifier", 1.2 },
			{ "curiosity_level", 0.2 },
			{ "energy_drain", 0.01 },
			{ "preferred_tactics", new string[] { "idle", "rest", "observe" } }
		} }
	};

	// Behavior state configurations
	public Dictionary<int, Dictionary> BehaviorConfigs { get; set; } = new Dictionary<int, Dictionary>
	{
		{ 0, new Dictionary {  // IDLE
			{ "name", "Idle" },
			{ "action_delay", 2.0 },
			{ "movement_speed", 0.0 }
		} },
		{ 1, new Dictionary {  // PATROL
			{ "name", "Patrol" },
			{ "action_delay", 3.0 },
			{ "movement_speed", 100.0 }
		} },
		{ 2, new Dictionary {  // CHASE
			{ "name", "Chase" },
			{ "action_delay", 0.5 },
			{ "movement_speed", 200.0 }
		} },
		{ 3, new Dictionary {  // ATTACK
			{ "name", "Attack" },
			{ "action_delay", 0.3 },
			{ "movement_speed", 150.0 }
		} },
		{ 4, new Dictionary {  // RETREAT
			{ "name", "Retreat" },
			{ "action_delay", 1.0 },
			{ "movement_speed", 180.0 }
		} },
		{ 5, new Dictionary {  // FOLLOW
			{ "name", "Follow" },
			{ "action_delay", 0.5 },
			{ "movement_speed", 120.0 }
		} },
		{ 6, new Dictionary {  // EXPLORE
			{ "name", "Explore" },
			{ "action_delay", 2.0 },
			{ "movement_speed", 80.0 }
		} },
		{ 7, new Dictionary {  // HEAL
			{ "name", "Heal" },
			{ "action_delay", 1.5 },
			{ "movement_speed", 50.0 }
		} }
	};

	// Emotion configurations
	public Dictionary<int, Dictionary> EmotionConfigs { get; set; } = new Dictionary<int, Dictionary>
	{
		{ 0, new Dictionary {  // HAPPY
			{ "name", "Happy" },
			{ "stat_modifiers", new Dictionary<string, float> {
				{ "damage", 1.1f },
				{ "defense", 1.0f },
				{ "speed", 1.0f }
			} },
			{ "particle_color", new Color(1.0f, 0.9f, 0.3f, 1.0f) }
		} },
		{ 1, new Dictionary {  // SAD
			{ "name", "Sad" },
			{ "stat_modifiers", new Dictionary<string, float> {
				{ "damage", 0.8f },
				{ "defense", 0.9f },
				{ "speed", 0.8f }
			} },
			{ "particle_color", new Color(0.5f, 0.5f, 0.8f, 1.0f) }
		} },
		{ 2, new Dictionary {  // ANGRY
			{ "name", "Angry" },
			{ "stat_modifiers", new Dictionary<string, float> {
				{ "damage", 1.3f },
				{ "defense", 1.2f },
				{ "speed", 1.1f }
			} },
			{ "particle_color", new Color(1.0f, 0.3f, 0.3f, 1.0f) }
		} },
		{ 3, new Dictionary {  // EXCITED
			{ "name", "Excited" },
			{ "stat_modifiers", new Dictionary<string, float> {
				{ "damage", 1.2f },
				{ "defense", 1.0f },
				{ "speed", 1.3f }
			} },
			{ "particle_color", new Color(1.0f, 0.8f, 0.2f, 1.0f) }
		} },
		{ 4, new Dictionary {  // SCARED
			{ "name", "Scared" },
			{ "stat_modifiers", new Dictionary<string, float> {
				{ "damage", 0.7f },
				{ "defense", 0.8f },
				{ "speed", 1.2f }
			} },
			{ "particle_color", new Color(0.7f, 0.7f, 0.9f, 1.0f) }
		} },
		{ 5, new Dictionary {  // CALM
			{ "name", "Calm" },
			{ "stat_modifiers", new Dictionary<string, float> {
				{ "damage", 1.0f },
				{ "defense", 1.1f },
				{ "speed", 1.0f }
			} },
			{ "particle_color", new Color(0.5f, 0.8f, 1.0f, 1.0f) }
		} }
	};

	// Learning parameters
	public Dictionary<string, float> LearningConfig { get; set; } = new Dictionary<string, float>
	{
		{ "base_adaptation_rate", 0.01f },
		{ "win_bonus", 0.05f },
		{ "loss_penalty", 0.02f },
		{ "combo_threshold", 3.0f },
		{ "learning_decay", 0.001f },
		{ "max_enemy_type_memory", 20.0f },
		{ "response_time_weight", 0.1f }
	};

	// AI level thresholds
	public List<Dictionary> LevelThresholds { get; set; } = new List<Dictionary>
	{
		new Dictionary { { "level", 1 }, { "battles", 0 }, { "adaptation", 0.0f } },
		new Dictionary { { "level", 2 }, { "battles", 10 }, { "adaptation", 0.1f } },
		new Dictionary { { "level", 3 }, { "battles", 25 }, { "adaptation", 0.2f } },
		new Dictionary { { "level", 4 }, { "battles", 40 }, { "adaptation", 0.3f } },
		new Dictionary { { "level", 5 }, { "battles", 60 }, { "adaptation", 0.4f } },
		new Dictionary { { "level", 6 }, { "battles", 85 }, { "adaptation", 0.5f } },
		new Dictionary { { "level", 7 }, { "battles", 115 }, { "adaptation", 0.6f } },
		new Dictionary { { "level", 8 }, { "battles", 150 }, { "adaptation", 0.65f } },
		new Dictionary { { "level", 9 }, { "battles", 190 }, { "adaptation", 0.7f } },
		new Dictionary { { "level", 10 }, { "battles", 235 }, { "adaptation", 0.75f } },
		new Dictionary { { "level", 11 }, { "battles", 285 }, { "adaptation", 0.8f } },
		new Dictionary { { "level", 12 }, { "battles", 340 }, { "adaptation", 0.85f } },
		new Dictionary { { "level", 13 }, { "battles", 400 }, { "adaptation", 0.9f } },
		new Dictionary { { "level", 14 }, { "battles", 465 }, { "adaptation", 0.95f } },
		new Dictionary { { "level", 15 }, { "battles", 535 }, { "adaptation", 1.0f } }
	};

	// Default decision weights
	public Dictionary<string, float> DefaultDecisionWeights { get; set; } = new Dictionary<string, float>
	{
		{ "attack", 1.0f },
		{ "defend", 1.0f },
		{ "support", 1.0f },
		{ "retreat", 1.0f },
		{ "explore", 1.0f }
	};

	public Dictionary GetPersonalityConfig(int type)
	{
		return PersonalityConfigs.GetValueOrDefault(type, PersonalityConfigs[0]);
	}

	public Dictionary GetBehaviorConfig(int state)
	{
		return BehaviorConfigs.GetValueOrDefault(state, BehaviorConfigs[0]);
	}

	public Dictionary GetEmotionConfig(int emotion)
	{
		return EmotionConfigs.GetValueOrDefault(emotion, EmotionConfigs[0]);
	}

	public Dictionary GetLevelRequirements(int level)
	{
		if (level <= 0 || level > LevelThresholds.Count)
		{
			return LevelThresholds[LevelThresholds.Count - 1];
		}
		return LevelThresholds[level - 1];
	}

	public int CalculateAiLevel(int battles, float adaptation, float winRate)
	{
		int level = 1;
		
		foreach (var req in LevelThresholds)
		{
			if (battles >= (int)req["battles"] && adaptation >= (float)req["adaptation"])
			{
				level = (int)req["level"];
			}
		}
		
		// Win rate bonus
		level += (int)(winRate * 5);
		
		return Mathf.Clamp(level, 1, 15);
	}

	public float GetStatModifierFromEmotion(int emotion, string stat)
	{
		var config = GetEmotionConfig(emotion);
		if (config.Contains("stat_modifiers"))
		{
			var statModifiers = config["stat_modifiers"] as Dictionary<string, float>;
			if (statModifiers != null && statModifiers.ContainsKey(stat))
			{
				return statModifiers[stat];
			}
		}
		return 1.0f;
	}
}
