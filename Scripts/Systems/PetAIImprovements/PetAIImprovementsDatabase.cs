# Pet AI Improvements Database
# Configuration for personality types, behaviors, and learning parameters

class_name PetAIImprovementsDatabase extends Node

# Personality type configurations
var personality_configs: Dictionary = {
	0: {  # AGGRESSIVE
		"name": "Aggressive",
		"description": "Pets that focus on attacking enemies with high damage output",
		"aggression_modifier": 1.5,
		"defense_modifier": 0.8,
		"curiosity_level": 0.3,
		"energy_drain": 0.05,
		"preferred_tactics": ["attack", "chase", "pursue"]
	},
	1: {  # DEFENSIVE
		"name": "Defensive",
		"description": "Pets that protect the player and prioritize defense",
		"aggression_modifier": 0.7,
		"defense_modifier": 1.5,
		"curiosity_level": 0.4,
		"energy_drain": 0.02,
		"preferred_tactics": ["defend", "block", "protect"]
	},
	2: {  # SUPPORTIVE
		"name": "Supportive",
		"description": "Pets that heal and support the player",
		"aggression_modifier": 0.8,
		"defense_modifier": 1.0,
		"curiosity_level": 0.5,
		"energy_drain": 0.03,
		"preferred_tactics": ["heal", "buff", "support"]
	},
	3: {  # CURIOUS
		"name": "Curious",
		"description": "Pets that explore and discover hidden things",
		"aggression_modifier": 1.0,
		"defense_modifier": 1.0,
		"curiosity_level": 0.9,
		"energy_drain": 0.04,
		"preferred_tactics": ["explore", "discover", "investigate"]
	},
	4: {  # LAZY
		"name": "Lazy",
		"description": "Pets that prefer resting but can act when needed",
		"aggression_modifier": 0.5,
		"defense_modifier": 1.2,
		"curiosity_level": 0.2,
		"energy_drain": 0.01,
		"preferred_tactics": ["idle", "rest", "observe"]
	}
}

# Behavior state configurations
var behavior_configs: Dictionary = {
	0: {  # IDLE
		"name": "Idle",
		"action_delay": 2.0,
		"movement_speed": 0.0
	},
	1: {  # PATROL
		"name": "Patrol",
		"action_delay": 3.0,
		"movement_speed": 100.0
	},
	2: {  # CHASE
		"name": "Chase",
		"action_delay": 0.5,
		"movement_speed": 200.0
	},
	3: {  # ATTACK
		"name": "Attack",
		"action_delay": 0.3,
		"movement_speed": 150.0
	},
	4: {  # RETREAT
		"name": "Retreat",
		"action_delay": 1.0,
		"movement_speed": 180.0
	},
	5: {  # FOLLOW
		"name": "Follow",
		"action_delay": 0.5,
		"movement_speed": 120.0
	},
	6: {  # EXPLORE
		"name": "Explore",
		"action_delay": 2.0,
		"movement_speed": 80.0
	},
	7: {  # HEAL
		"name": "Heal",
		"action_delay": 1.5,
		"movement_speed": 50.0
	}
}

# Emotion configurations
var emotion_configs: Dictionary = {
	0: {  # HAPPY
		"name": "Happy",
		"stat_modifiers": {
			"damage": 1.1,
			"defense": 1.0,
			"speed": 1.0
		},
		"particle_color": Color(1.0, 0.9, 0.3, 1.0)
	},
	1: {  # SAD
		"name": "Sad",
		"stat_modifiers": {
			"damage": 0.8,
			"defense": 0.9,
			"speed": 0.8
		},
		"particle_color": Color(0.5, 0.5, 0.8, 1.0)
	},
	2: {  # ANGRY
		"name": "Angry",
		"stat_modifiers": {
			"damage": 1.3,
			"defense": 1.2,
			"speed": 1.1
		},
		"particle_color": Color(1.0, 0.3, 0.3, 1.0)
	},
	3: {  # EXCITED
		"name": "Excited",
		"stat_modifiers": {
			"damage": 1.2,
			"defense": 1.0,
			"speed": 1.3
		},
		"particle_color": Color(1.0, 0.8, 0.2, 1.0)
	},
	4: {  # SCARED
		"name": "Scared",
		"stat_modifiers": {
			"damage": 0.7,
			"defense": 0.8,
			"speed": 1.2
		},
		"particle_color": Color(0.7, 0.7, 0.9, 1.0)
	},
	5: {  # CALM
		"name": "Calm",
		"stat_modifiers": {
			"damage": 1.0,
			"defense": 1.1,
			"speed": 1.0
		},
		"particle_color": Color(0.5, 0.8, 1.0, 1.0)
	}
}

# Learning parameters
var learning_config: Dictionary = {
	"base_adaptation_rate": 0.01,
	"win_bonus": 0.05,
	"loss_penalty": 0.02,
	"combo_threshold": 3,
	"learning_decay": 0.001,
	"max_enemy_type_memory": 20,
	"response_time_weight": 0.1
}

# AI level thresholds
var level_thresholds: Array = [
	{"level": 1, "battles": 0, "adaptation": 0.0},
	{"level": 2, "battles": 10, "adaptation": 0.1},
	{"level": 3, "battles": 25, "adaptation": 0.2},
	{"level": 4, "battles": 40, "adaptation": 0.3},
	{"level": 5, "battles": 60, "adaptation": 0.4},
	{"level": 6, "battles": 85, "adaptation": 0.5},
	{"level": 7, "battles": 115, "adaptation": 0.6},
	{"level": 8, "battles": 150, "adaptation": 0.65},
	{"level": 9, "battles": 190, "adaptation": 0.7},
	{"level": 10, "battles": 235, "adaptation": 0.75},
	{"level": 11, "battles": 285, "adaptation": 0.8},
	{"level": 12, "battles": 340, "adaptation": 0.85},
	{"level": 13, "battles": 400, "adaptation": 0.9},
	{"level": 14, "battles": 465, "adaptation": 0.95},
	{"level": 15, "battles": 535, "adaptation": 1.0}
]

# Default decision weights
var default_decision_weights: Dictionary = {
	"attack": 1.0,
	"defend": 1.0,
	"support": 1.0,
	"retreat": 1.0,
	"explore": 1.0
}

func get_personality_config(type: int) -> Dictionary:
	return personality_configs.get(type, personality_configs[0])

func get_behavior_config(state: int) -> Dictionary:
	return behavior_configs.get(state, behavior_configs[0])

func get_emotion_config(emotion: int) -> Dictionary:
	return emotion_configs.get(emotion, emotion_configs[0])

func get_level_requirements(level: int) -> Dictionary:
	if level <= 0 or level > level_thresholds.size():
		return level_thresholds[level_thresholds.size() - 1]
	return level_thresholds[level - 1]

func calculate_ai_level(battles: int, adaptation: float, win_rate: float) -> int:
	var level: int = 1
	var requirements = level_thresholds[level_thresholds.size() - 1]
	
	for i in range(level_thresholds.size()):
		var req = level_thresholds[i]
		if battles >= req["battles"] and adaptation >= req["adaptation"]:
			level = req["level"]
	
	# Win rate bonus
	level += int(win_rate * 5)
	
	return clamp(level, 1, 15)

func get_stat_modifier_from_emotion(emotion: int, stat: String) -> float:
	var config = get_emotion_config(emotion)
	if config.has("stat_modifiers") and config["stat_modifiers"].has(stat):
		return config["stat_modifiers"][stat]
	return 1.0
