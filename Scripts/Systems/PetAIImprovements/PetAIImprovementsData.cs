# Pet AI Improvements Data
# Enhanced pet AI behaviors and learning systems

class PetAIPersonality extends Node:
	enum PersonalityType:
		AGGRESSIVE = 0  # 主动攻击型
		DEFENSIVE = 1   # 防御保护型
		SUPPORTIVE = 2  # 辅助支援型
		CURIOUS = 3     # 探索好奇型
		LAZY = 4        # 懒散休息型
	
	var personality_type: int = PersonalityType.AGGRESSIVE
	var curiosity_level: float = 0.5  # 0.0 - 1.0
	var energy_level: float = 1.0    # 0.0 - 1.0
	var loyalty_level: float = 0.5   # 0.0 - 1.0
	var aggression_modifier: float = 1.0
	var defense_modifier: float = 1.0
	
	func _init(type: int = PersonalityType.AGGRESSIVE):
		personality_type = type
		match type:
			PersonalityType.AGGRESSIVE:
				aggression_modifier = 1.5
				defense_modifier = 0.8
			PersonalityType.DEFENSIVE:
				aggression_modifier = 0.7
				defense_modifier = 1.5
			PersonalityType.SUPPORTIVE:
				aggression_modifier = 0.8
				defense_modifier = 1.0
			PersonalityType.CURIOUS:
				curiosity_level = 0.9
				aggression_modifier = 1.0
			PersonalityType.LAZY:
				energy_level = 0.3
				aggression_modifier = 0.5

class PetAIBehavior extends Node:
	enum BehaviorState:
		IDLE = 0
		PATROL = 1
		CHASE = 2
		ATTACK = 3
		RETREAT = 4
		FOLLOW = 5
		EXPLORE = 6
		HEAL = 7
	
	var current_state: int = BehaviorState.IDLE
	var target_position: Vector2 = Vector2.ZERO
	var target_entity: Node = null
	var state_timer: float = 0.0
	var behavior_priority: int = 0
	
	func get_state_name() -> String:
		match current_state:
			BehaviorState.IDLE: return "Idle"
			BehaviorState.PATROL: return "Patrol"
			BehaviorState.CHASE: return "Chase"
			BehaviorState.ATTACK: return "Attack"
			BehaviorState.RETREAT: return "Retreat"
			BehaviorState.FOLLOW: return "Follow"
			BehaviorState.EXPLORE: return "Explore"
			BehaviorState.HEAL: return "Heal"
		return "Unknown"

class PetAILearning extends Node:
	var learning_data: Dictionary = {}
	var enemy_type_kills: Dictionary = {}  # EnemyType -> kill_count
	var player_action_mimic: Array = []      # 记录玩家行为用于学习
	var adaptation_level: float = 0.0        # 0.0 - 1.0
	var preferred_tactics: Array = []        # 偏好的战术
	
	func _init():
		learning_data = {
			"total_battles": 0,
			"wins": 0,
			"losses": 0,
			"dodge_count": 0,
			"block_count": 0,
			"heal_count": 0,
			"combo_count": 0,
			"best_combo": 0,
			"average_response_time": 0.0,
			"preferred_enemy_types": [],
			"weak_against": []
		}
	
	func record_battle_result(win: bool):
		learning_data["total_battles"] += 1
		if win:
			learning_data["wins"] += 1
		else:
			learning_data["losses"] += 1
		update_adaptation_level()
	
	func record_enemy_killed(enemy_type: String):
		if not enemy_type_kills.has(enemy_type):
			enemy_type_kills[enemy_type] = 0
		enemy_type_kills[enemy_type] += 1
	
	func record_dodge():
		learning_data["dodge_count"] += 1
	
	func record_block():
		learning_data["block_count"] += 1
	
	func record_heal():
		learning_data["heal_count"] += 1
	
	func record_combo(combo_size: int):
		learning_data["combo_count"] += 1
		if combo_size > learning_data["best_combo"]:
			learning_data["best_combo"] = combo_size
	
	func update_adaptation_level():
		var total: float = float(learning_data["total_battles"])
		if total > 0:
			adaptation_level = min(1.0, total / 100.0)
	
	func get_win_rate() -> float:
		var total: int = learning_data["total_battles"]
		if total == 0:
			return 0.0
		return float(learning_data["wins"]) / float(total)
	
	func get_most_killed_enemy() -> String:
		var max_kills: int = 0
		var result: String = ""
		for enemy_type in enemy_type_kills:
			if enemy_type_kills[enemy_type] > max_kills:
				max_kills = enemy_type_kills[enemy_type]
				result = enemy_type
		return result

class PetAIDecision extends Node:
	var decision_weights: Dictionary = {
		"attack": 1.0,
		"defend": 1.0,
		"support": 1.0,
		"retreat": 1.0,
		"explore": 1.0
	}
	var personality_influence: float = 0.5
	var learning_influence: float = 0.3
	var situation_influence: float = 0.2
	
	func calculate_decision(personality: PetAIPersonality, learning: PetAILearning, 
							situation: Dictionary) -> String:
		var weights = decision_weights.duplicate()
		
		# 性格影响
		match personality.personality_type:
			PetAIPersonality.PersonalityType.AGGRESSIVE:
				weights["attack"] *= personality.aggression_modifier * personality_influence * 2.0
				weights["retreat"] *= 0.5
			PetAIPersonality.PersonalityType.DEFENSIVE:
				weights["defend"] *= personality.defense_modifier * personality_influence * 2.0
				weights["attack"] *= 0.7
			PetAIPersonality.PersonalityType.SUPPORTIVE:
				weights["support"] *= personality_influence * 2.0
				weights["attack"] *= 0.8
			PetAIPersonality.PersonalityType.CURIOUS:
				weights["explore"] *= personality.curiosity_level * personality_influence * 2.0
		
		# 学习影响
		var win_rate: float = learning.get_win_rate()
		if win_rate > 0.7:
			weights["attack"] *= (1.0 + learning_influence)
		elif win_rate < 0.4:
			weights["defend"] *= (1.0 + learning_influence)
			weights["retreat"] *= (1.0 + learning_influence * 0.5)
		
		# 情况影响
		if situation.has("player_health"):
			var player_health: float = situation["player_health"]
			if player_health < 0.3:
				weights["support"] *= situation_influence * 3.0
				weights["defend"] *= situation_influence * 2.0
		
		if situation.has("enemy_count"):
			var enemy_count: int = situation["enemy_count"]
			if enemy_count > 3:
				weights["attack"] *= 0.7
				weights["defend"] *= 1.5
		
		# 选择最高权重
		var best_decision: String = "attack"
		var best_weight: float = 0.0
		for decision in weights:
			if weights[decision] > best_weight:
				best_weight = weights[decision]
				best_decision = decision
		
		return best_decision

class PetAIEmotionalState extends Node:
	enum Emotion:
		HAPPY = 0
		SAD = 1
		ANGRY = 2
		EXCITED = 3
		SCARED = 4
		CALM = 5
	
	var current_emotion: int = Emotion.HAPPY
	var emotion_intensity: float = 0.5  # 0.0 - 1.0
	var mood_timer: float = 0.0
	var emotion_history: Array = []
	
	func _init():
		emotion_history = []
	
	func update_emotion(new_emotion: int, intensity: float = 0.5):
		current_emotion = new_emotion
		emotion_intensity = clamp(intensity, 0.0, 1.0)
		mood_timer = 0.0
		
		emotion_history.append({
			"emotion": new_emotion,
			"intensity": intensity,
			"time": Time.get_unix_time_from_system()
		})
		
		# 保持历史在最近20条
		if emotion_history.size() > 20:
			emotion_history.pop_front()
	
	func get_emotion_name() -> String:
		match current_emotion:
			Emotion.HAPPY: return "Happy"
			Emotion.SAD: return "Sad"
			Emotion.ANGRY: return "Angry"
			Emotion.EXCITED: return "Excited"
			Emotion.SCARED: return "Scared"
			Emotion.CALM: return "Calm"
		return "Unknown"
	
	func update_emotion_from_battle(win: bool, player_health_change: float):
		if win:
			update_emotion(Emotion.EXCITED, 0.8)
		elif player_health_change < -0.3:
			update_emotion(Emotion.SCARED, 0.7)
		elif player_health_change > 0.3:
			update_emotion(Emotion.HAPPY, 0.6)
		else:
			update_emotion(Emotion.CALM, 0.5)

class PetAIImprovementsData extends Node:
	var personality: PetAIPersonality
	var behavior: PetAIBehavior
	var learning: PetAILearning
	var decision: PetAIDecision
	var emotion: PetAIEmotionalState
	
	# 战斗统计
	var total_damage_dealt: float = 0.0
	var total_damage_prevented: float = 0.0
	var total_healing_done: float = 0.0
	var critical_hits: int = 0
	var perfect_dodges: int = 0
	
	func _init():
		personality = PetAIPersonality.new()
		behavior = PetAIBehavior.new()
		learning = PetAILearning.new()
		decision = PetAIDecision.new()
		emotion = PetAIEmotionalState.new()
	
	func reset():
		total_damage_dealt = 0.0
		total_damage_prevented = 0.0
		total_healing_done = 0.0
		critical_hits = 0
		perfect_dodges = 0
	
	func get_ai_level() -> int:
		# 基于学习数据和适应等级计算AI等级
		var level: float = 1.0
		level += learning.adaptation_level * 9.0  # 1-10级
		level += learning.get_win_rate() * 5.0    # 额外加分
		return int(clamp(level, 1, 15))

# Export variables for Godot
extends Node
