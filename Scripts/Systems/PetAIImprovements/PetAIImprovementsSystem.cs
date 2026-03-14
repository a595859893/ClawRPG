# Pet AI Improvements System
# Enhanced pet AI with personality, learning, and adaptive behaviors

const AI_STATE_CHANGED = "ai_state_changed"
const AI_DECISION_MADE = "ai_decision_made"
const AI_LEARNING_UPDATE = "ai_learning_update"
const AI_EMOTION_CHANGED = "ai_emotion_changed"
const AI_LEVEL_UP = "ai_level_up"

class_name PetAIImprovementsSystem extends Node

var data: PetAIImprovementsData
var is_active: bool = false
var current_target: Node = null
var owner_pet_id: String = ""

# 配置参数
var decision_interval: float = 0.5  # 决策间隔(秒)
var emotion_decay_rate: float = 0.1  # 情绪衰减率
var learning_threshold: int = 10    # 学习阈值
var max_ai_level: int = 15

# 状态追踪
var decision_timer: float = 0.0
var last_ai_level: int = 1

func _init():
	data = PetAIImprovementsData.new()

func initialize(pet_id: String, personality_type: int = 0):
	owner_pet_id = pet_id
	data.personality = PetAIPersonality.new(personality_type)
	is_active = true
	last_ai_level = 1
	print("[PetAI] Initialized for pet: ", pet_id, " with personality: ", data.personality.get_state_name())

func _process(delta: float):
	if not is_active:
		return
	
	# 更新决策计时器
	decision_timer += delta
	if decision_timer >= decision_interval:
		decision_timer = 0.0
		make_ai_decision()
	
	# 更新情绪状态
	update_emotion_state(delta)

func make_ai_decision():
	# 收集当前情况
	var situation: Dictionary = collect_situation()
	
	# 计算最佳决策
	var decision: String = data.decision.calculate_decision(
		data.personality, 
		data.learning, 
		situation
	)
	
	# 执行决策
	execute_decision(decision, situation)
	
	# 发出信号
	emit_signal(AI_DECISION_MADE, decision, situation)

func collect_situati
on() -> Dictionary:
	var situation: Dictionary = {}
	
	# 玩家血量
	var player = get_tree().get_first_node_in_group("player")
	if player and player.has_method("get_health_percent"):
		situation["player_health"] = player.get_health_percent()
	
	# 敌人数量
	var enemies = get_tree().get_nodes_in_group("enemy")
	situation["enemy_count"] = enemies.size()
	
	# 最近敌人
	if enemies.size() > 0:
		situation["nearest_enemy_distance"] = 9999.0
		for enemy in enemies:
			if enemy.has_method("get_global_position"):
				var dist = enemy.get_global_position().distance_to(get_global_position())
				if dist < situation["nearest_enemy_distance"]:
					situation["nearest_enemy_distance"] = dist
	
	# 宠物血量
	situation["pet_health"] = 1.0  # 默认满血
	
	# 能量水平
	situation["energy"] = data.personality.energy_level
	
	return situation

func execute_decision(decision: String, situation: Dictionary):
	match decision:
		"attack":
			data.behavior.current_state = data.behavior.BehaviorState.ATTACK
			perform_attack_action(situation)
		"defend":
			data.behavior.current_state = data.behavior.BehaviorState.IDLE
			perform_defend_action(situation)
		"support":
			data.behavior.current_state = data.behavior.BehaviorState.FOLLOW
			perform_support_action(situation)
		"retreat":
			data.behavior.current_state = data.behavior.BehaviorState.RETREAT
			perform_retreat_action(situation)
		"explore":
			data.behavior.current_state = data.behavior.BehaviorState.EXPLORE
			perform_explore_action(situation)
	
	emit_signal(AI_STATE_CHANGED, data.behavior.get_state_name())

func perform_attack_action(situation: Dictionary):
	# 攻击行为逻辑
	if situation.has("enemy_count") and situation["enemy_count"] > 0:
		data.learning.record_battle_result(true)
		data.personality.energy_level = max(0.0, data.personality.energy_level - 0.05)

func perform_defend_action(situation: Dictionary):
	# 防御行为逻辑
	data.total_damage_prevented += 10.0
	data.learning.record_block()
	data.personality.energy_level = min(1.0, data.personality.energy_level + 0.02)

func perform_support_action(situation: Dictionary):
	# 支援行为逻辑
	data.total_healing_done += 5.0
	data.learning.record_heal()
	data.personality.energy_level = max(0.0, data.personality.energy_level - 0.03)

func perform_retreat_action(situation: Dictionary):
	# 撤退行为逻辑
	data.learning.record_dodge()
	data.personality.energy_level = min(1.0, data.personality.energy_level + 0.05)

func perform_explore_action(situation: Dictionary):
	# 探索行为逻辑 - 好奇心强的宠物
	if data.personality.personality_type == PetAIPersonality.PersonalityType.CURIOUS:
		# 发现隐藏物品/区域
		pass

func update_emotion_state(delta: float):
	data.emotion.mood_timer += delta
	
	# 情绪随时间衰减
	if data.emotion.mood_timer > 10.0:
		data.emotion.update_emotion(PetAIEmotionalState.Emotion.CALM, 0.5)

func record_battle_event(enemy_type: String, won: bool, damage_dealt: float, 
						 damage_prevented: float, healing_done: float):
	data.learning.record_battle_result(won)
	data.learning.record_enemy_killed(enemy_type)
	data.total_damage_dealt += damage_dealt
	data.total_damage_prevented += damage_prevented
	data.total_healing_done += healing_done
	
	# 更新情绪
	data.emotion.update_emotion_from_battle(won, 0.0)
	
	# 检查升级
	check_ai_level_up()

func record_critical_hit():
	data.learning.record_combo(data.learning.learning_data["best_combo"] + 1)
	data.critical_hits += 1

func record_perfect_dodge():
	data.learning.record_dodge()
	data.perfect_dodges += 1

func check_ai_level_up():
	var new_level: int = data.get_ai_level()
	if new_level > last_ai_level:
		last_ai_level = new_level
		emit_signal(AI_LEVEL_UP, new_level)
		print("[PetAI] AI Level up! New level: ", new_level)

func set_personality_type(personality_type: int):
	data.personality = PetAIPersonality.new(personality_type)

func get_ai_state() -> String:
	return data.behavior.get_state_name()

func get_ai_level() -> int:
	return data.get_ai_level()

func get_current_emotion() -> String:
	return data.emotion.get_emotion_name()

func get_personality_type() -> String:
	match data.personality.personality_type:
		PetAIPersonality.PersonalityType.AGGRESSIVE: return "Aggressive"
		PetAIPersonality.PersonalityType.DEFENSIVE: return "Defensive"
		PetAIPersonality.PersonalityType.SUPPORTIVE: return "Supportive"
		PetAIPersonality.PersonalityType.CURIOUS: return "Curious"
		PetAIPersonality.PersonalityType.LAZY: return "Lazy"
	return "Unknown"

func get_learning_stats() -> Dictionary:
	return {
		"adaptation_level": data.learning.adaptation_level,
		"win_rate": data.learning.get_win_rate(),
		"total_battles": data.learning.learning_data["total_battles"],
		"best_combo": data.learning.learning_data["best_combo"],
		"most_killed_enemy": data.learning.get_most_killed_enemy()
	}

func get_combat_stats() -> Dictionary:
	return {
		"total_damage_dealt": data.total_damage_dealt,
		"total_damage_prevented": data.total_damage_prevented,
		"total_healing_done": data.total_healing_done,
		"critical_hits": data.critical_hits,
		"perfect_dodges": data.perfect_dodges
	}

func export_save_data() -> Dictionary:
	return {
		"personality_type": data.personality.personality_type,
		"curiosity_level": data.personality.curiosity_level,
		"energy_level": data.personality.energy_level,
		"loyalty_level": data.personality.loyalty_level,
		"adaptation_level": data.learning.adaptation_level,
		"total_battles": data.learning.learning_data["total_battles"],
		"wins": data.learning.learning_data["wins"],
		"losses": data.learning.learning_data["losses"],
		"best_combo": data.learning.learning_data["best_combo"],
		"enemy_type_kills": data.learning.enemy_type_kills,
		"current_emotion": data.emotion.current_emotion,
		"emotion_intensity": data.emotion.emotion_intensity,
		"total_damage_dealt": data.total_damage_dealt,
		"total_damage_prevented": data.total_damage_prevented,
		"total_healing_done": data.total_healing_done,
		"critical_hits": data.critical_hits,
		"perfect_dodges": data.perfect_dodges
	}

func import_save_data(save_data: Dictionary):
	if save_data.has("personality_type"):
		data.personality = PetAIPersonality.new(save_data["personality_type"])
		data.personality.curiosity_level = save_data.get("curiosity_level", 0.5)
		data.personality.energy_level = save_data.get("energy_level", 1.0)
		data.personality.loyalty_level = save_data.get("loyalty_level", 0.5)
	
	if save_data.has("adaptation_level"):
		data.learning.adaptation_level = save_data["adaptation_level"]
		data.learning.learning_data["total_battles"] = save_data.get("total_battles", 0)
		data.learning.learning_data["wins"] = save_data.get("wins", 0)
		data.learning.learning_data["losses"] = save_data.get("losses", 0)
		data.learning.learning_data["best_combo"] = save_data.get("best_combo", 0)
	
	if save_data.has("enemy_type_kills"):
		data.learning.enemy_type_kills = save_data["enemy_type_kills"]
	
	if save_data.has("current_emotion"):
		data.emotion.current_emotion = save_data["current_emotion"]
		data.emotion.emotion_intensity = save_data.get("emotion_intensity", 0.5)
	
	data.total_damage_dealt = save_data.get("total_damage_dealt", 0.0)
	data.total_damage_prevented = save_data.get("total_damage_prevented", 0.0)
	data.total_healing_done = save_data.get("total_healing_done", 0.0)
	data.critical_hits = save_data.get("critical_hits", 0)
	data.perfect_dodges = save_data.get("perfect_dodges", 0)
	
	last_ai_level = data.get_ai_level()

func deactivate():
	is_active = false
