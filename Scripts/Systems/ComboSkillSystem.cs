# Combo Skill System - 连击技能系统核心
## 连击技能系统核心逻辑

extends Node
class_name ComboSkillSystem

## 连击执行信号
signal combo_executed(combo_id: String)

## 连击完成信号
signal combo_completed(combo_id: String)

## 连击步骤触发信号
signal combo_step_triggered(combo_id: String, step: int)

## 连击取消信号
signal combo_cancelled(combo_id: String)

## 冷却更新信号
signal cooldown_updated(combo_id: String, remaining: float)
signal combo_unlocked(combo_id: String)

static var instance: ComboSkillSystem

# 玩家数据
var unlocked_combos: Array[String] = []
var equipped_combos: Array[PlayerComboSkill] = []
var execution_queue: Array[ComboExecutionState] = []

# 统计
var total_combos_executed: int = 0
var total_combos_completed: int = 0

func _ready():
	instance = self

static func get_instance():
	return instance

# ============ 解锁管理 ============

func unlock_combo(combo_id: String) -> bool:
	if unlocked_combos.has(combo_id):
		return false
	
	var combo = ComboSkillDatabase.get_instance().get_combo(combo_id)
	if combo == null:
		return false
	
	unlocked_combos.append(combo_id)
	combo_unlocked.emit(combo_id)
	return true

func is_unlocked(combo_id: String) -> bool:
	return unlocked_combos.has(combo_id)

func get_unlocked_combos() -> Array[String]:
	return unlocked_combos.duplicate()

# ============ 装备管理 ============

func equip_combo(combo_id: String) -> bool:
	if not unlocked_combos.has(combo_id):
		return false
	
	# 检查是否已装备
	for equipped in equipped_combos:
		if equipped.combo_id == combo_id:
			return true
	
	# 限制装备数量
	if equipped_combos.size() >= 5:
		return false
	
	var combo = ComboSkillDatabase.get_instance().get_combo(combo_id)
	if combo == null:
		return false
	
	var player_combo = PlayerComboSkill.new()
	player_combo.combo_id = combo_id
	player_combo.is_equipped = true
	player_combo.current_cooldown = 0.0
	player_combo.use_count = 0
	
	equipped_combos.append(player_combo)
	return true

func unequip_combo(combo_id: String) -> bool:
	for equipped in equipped_combos:
		if equipped.combo_id == combo_id:
			equipped.is_equipped = false
			equipped_combos.erase(equipped)
			return true
	return false

func is_equipped(combo_id: String) -> bool:
	for equipped in equipped_combos:
		if equipped.combo_id == combo_id:
			return equipped.is_equipped
	return false

func get_equipped_combos() -> Array[PlayerComboSkill]:
	return equipped_combos.duplicate()

# ============ 执行系统 ============

func execute_combo(combo_id: String) -> bool:
	if not unlocked_combos.has(combo_id):
		return false
	
	# 检查冷却
	var player_combo = _get_player_combo(combo_id)
	if player_combo != null and player_combo.current_cooldown > 0:
		return false
	
	# 检查是否正在执行
	for state in execution_queue:
		if state.combo_id == combo_id:
			return false
	
	var combo = ComboSkillDatabase.get_instance().get_combo(combo_id)
	if combo == null:
		return false
	
	# 创建执行状态
	var state = ComboExecutionState.new()
	state.combo_id = combo_id
	state.current_step = 0
	state.is_executing = true
	state.start_time = Time.get_unix_time_from_system()
	state.effects_applied = 0
	
	execution_queue.append(state)
	
	# 设置冷却
	if player_combo != null:
		player_combo.current_cooldown = combo.cooldown
		player_combo.use_count += 1
	
	total_combos_executed += 1
	combo_executed.emit(combo_id)
	
	# 开始执行第一步
	_execute_step(state)
	
	return true

func _execute_step(state: ComboExecutionState):
	if not state.is_executing:
		return
	
	var combo = ComboSkillDatabase.get_instance().get_combo(state.combo_id)
	if combo == null or state.current_step >= combo.steps.size():
		_complete_combo(state)
		return
	
	var step = combo.steps[state.current_step]
	
	# 延迟执行
	if step.delay > 0:
		await get_tree().create_timer(step.delay).timeout
	
	# 检查触发条件
	if not _check_condition(step):
		# 条件不满足，跳过此步骤
		state.current_step += 1
		_execute_step(state)
		return
	
	# 执行效果
	_apply_effects(step.effects)
	state.effects_applied += step.effects.size()
	combo_step_triggered.emit(state.combo_id, state.current_step)
	
	state.current_step += 1
	
	# 继续下一步或完成
	if state.current_step < combo.steps.size():
		_execute_step(state)
	else:
		_complete_combo(state)

func _check_condition(step: ComboStep) -> bool:
	match step.condition:
		TriggerCondition.Manual:
			return true
		TriggerCondition.OnHit:
			# 需要外部事件触发
			return true
		TriggerCondition.OnCritical:
			# 需要外部事件触发
			return false
		TriggerCondition.OnKill:
			return false
		TriggerCondition.OnHealthBelow:
			# 需要获取玩家生命值
			# var player_health = 100  # 从玩家系统获取
			# return player_health < step.condition_value
			return false
		TriggerCondition.OnManaBelow:
			return false
		_:
			return true

func _apply_effects(effects: Array):
	for effect in effects:
		match effect.effect_type:
			EffectType.Damage:
				_apply_damage(effect.value)
			EffectType.Heal:
				_apply_heal(effect.value)
			EffectType.Shield:
				_apply_shield(effect.value, effect.duration)
			EffectType.Buff:
				_apply_buff(effect.value, effect.duration)
			EffectType.Debuff:
				_apply_debuff(effect.value, effect.duration)

func _apply_damage(value: float):
	# 实际伤害应用需要敌人目标
	print("Combo Skill: Dealing %f damage" % value)

func _apply_heal(value: float):
	print("Combo Skill: Healing %f HP" % value)

func _apply_shield(value: float, duration: float):
	print("Combo Skill: Shield %f for %f seconds" % [value, duration])

func _apply_buff(value: float, duration: float):
	print("Combo Skill: Buff +%f for %f seconds" % [value, duration])

func _apply_debuff(value: float, duration: float):
	print("Combo Skill: Debuff %f for %f seconds" % [value, duration])

func _complete_combo(state: ComboExecutionState):
	state.is_executing = false
	execution_queue.erase(state)
	total_combos_completed += 1
	combo_completed.emit(state.combo_id)

func cancel_combo(combo_id: String) -> bool:
	for state in execution_queue:
		if state.combo_id == combo_id:
			state.is_executing = false
			execution_queue.erase(state)
			combo_cancelled.emit(combo_id)
			return true
	return false

# ============ 冷却管理 ============

func _process(delta: float):
	for equipped in equipped_combos:
		if equipped.current_cooldown > 0:
			equipped.current_cooldown -= delta
			if equipped.current_cooldown < 0:
				equipped.current_cooldown = 0
			cooldown_updated.emit(equipped.combo_id, equipped.current_cooldown)

func get_cooldown(combo_id: String) -> float:
	var player_combo = _get_player_combo(combo_id)
	if player_combo == null:
		return 0.0
	return player_combo.current_cooldown

func is_on_cooldown(combo_id: String) -> bool:
	return get_cooldown(combo_id) > 0

func _get_player_combo(combo_id: String) -> PlayerComboSkill:
	for pc in equipped_combos:
		if pc.combo_id == combo_id:
			return pc
	return null

# ============ 统计信息 ============

func get_statistics() -> Dictionary:
	return {
		"total_executed": total_combos_executed,
		"total_completed": total_combos_completed,
		"unlocked_count": unlocked_combos.size(),
		"equipped_count": equipped_combos.size()
	}

# ============ 存档支持 ============

func ExportSaveData() -> Dictionary:
	var data = {
		"unlocked_combos": unlocked_combos.duplicate(),
		"equipped": [],
		"statistics": {
			"total_executed": total_combos_executed,
			"total_completed": total_combos_completed
		}
	}
	
	for equipped in equipped_combos:
		data["equipped"].append({
			"combo_id": equipped.combo_id,
			"cooldown": equipped.current_cooldown,
			"use_count": equipped.use_count
		})
	
	return data

func ImportSaveData(data: Dictionary):
	if data.has("unlocked_combos"):
		unlocked_combos = data["unlocked_combos"]
	
	equipped_combos.clear()
	if data.has("equipped"):
		for eq_data in data["equipped"]:
			var pc = PlayerComboSkill.new()
			pc.combo_id = eq_data["combo_id"]
			pc.is_equipped = true
			pc.current_cooldown = eq_data.get("cooldown", 0.0)
			pc.use_count = eq_data.get("use_count", 0)
			equipped_combos.append(pc)
	
	if data.has("statistics"):
		var stats = data["statistics"]
		total_combos_executed = stats.get("total_executed", 0)
		total_combos_completed = stats.get("total_completed", 0)
