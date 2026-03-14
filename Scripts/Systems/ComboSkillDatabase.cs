# Combo Skill Database - 连击技能配置数据库

extends Node
class_name ComboSkillDatabase

static var instance: ComboSkillDatabase

var _combos: Dictionary = {}

func _ready():
	instance = self
	_init_combos()

static func get_instance(): 
	return instance

func get_combo(combo_id: String) -> ComboSkill:
	return _combos.get(combo_id, null)

func get_all_combos() -> Array[ComboSkill]:
	return _combos.values()

func get_combos_by_type(combo_type: ComboType) -> Array[ComboSkill]:
	var result: Array[ComboSkill] = []
	for combo in _combos.values():
		if combo.combo_type == combo_type:
			result.append(combo)
	return result

func get_combos_by_rarity(rarity: int) -> Array[ComboSkill]:
	var result: Array[ComboSkill] = []
	for combo in _combos.values():
		if combo.rarity == rarity:
			result.append(combo)
	return result

func get_available_combos(player_level: int) -> Array[ComboSkill]:
	var result: Array[ComboSkill] = []
	for combo in _combos.values():
		if combo.level_required <= player_level:
			result.append(combo)
	return result

func get_rarity_color(rarity: int) -> Color:
	match rarity:
		0: return Color.WHITE
		1: return Color.GREEN
		2: return Color(0.3, 0.5, 1.0)
		3: return Color(0.6, 0.2, 0.8)
		4: return Color(1.0, 0.6, 0.0)
	return Color.WHITE

func get_rarity_name(rarity: int) -> String:
	match rarity:
		0: return "普通"
		1: return "优秀"
		2: return "稀有"
		3: return "史诗"
		4: return "传说"
	return "未知"

func _init_combos():
	# === 顺序连击 ===
	_init_sequential_combos()
	# === 链式连击 ===
	_init_chain_combos()
	# === 并行连击 ===
	_init_parallel_combos()
	# === 条件连击 ===
	_init_conditional_combos()

func _init_sequential_combos():
	# 闪电连击 - 顺序触发
	var combo1 = ComboSkill.new()
	combo1.id = "combo_lightning"
	combo1.name = "闪电连击"
	combo1.description = "召唤三道闪电依次打击敌人"
	combo1.combo_type = ComboType.Sequential
	combo1.cooldown = 8.0
	combo1.mana_cost = 30.0
	combo1.level_required = 5
	combo1.rarity = 1
	combo1.steps = [
		_create_step("lightning_bolt", 0.0, [_create_effect(EffectType.Damage, 50.0, "闪电打击 50 伤害")]),
		_create_step("lightning_bolt", 0.5, [create_effect(EffectType.Damage, 50.0, "闪电打击 50 伤害")]),
		_create_step("lightning_bolt", 1.0, [create_effect(EffectType.Damage, 75.0, "终结闪电 75 伤害")])
	]
	combo1.total_time = 2.0
	_combos[combo1.id] = combo1

	# 治疗链 - 顺序治疗
	var combo2 = ComboSkill.new()
	combo2.id = "combo_healing_chain"
	combo2.name = "治疗链"
	combo2.description = "依次治疗目标三次"
	combo2.combo_type = ComboType.Sequential
	combo2.cooldown = 12.0
	combo2.mana_cost = 40.0
	combo2.level_required = 8
	combo2.rarity = 1
	combo2.steps = [
		_create_step("heal", 0.0, [create_effect(EffectType.Heal, 30.0, "治疗 30 HP")]),
		_create_step("heal", 0.8, [create_effect(EffectType.Heal, 30.0, "治疗 30 HP")]),
		_create_step("heal", 1.6, [create_effect(EffectType.Heal, 50.0, "强力治疗 50 HP")])
	]
	combo2.total_time = 2.5
	_combos[combo2.id] = combo2

	# 火焰风暴
	var combo3 = ComboSkill.new()
	combo3.id = "combo_fire_storm"
	combo3.name = "火焰风暴"
	combo3.description = "召唤火焰陨石轰炸区域"
	combo3.combo_type = ComboType.Sequential
	combo3.cooldown = 20.0
	combo3.mana_cost = 80.0
	combo3.level_required = 20
	combo3.rarity = 3
	combo3.steps = [
		_create_step("fire_meteor", 0.0, [create_effect(EffectType.Damage, 100.0, "陨石 100 伤害")]),
		_create_step("fire_meteor", 0.6, [create_effect(EffectType.Damage, 100.0, "陨石 100 伤害")]),
		_create_step("fire_meteor", 1.2, [create_effect(EffectType.Damage, 100.0, "陨石 100 伤害")]),
		_create_step("fire_explosion", 2.0, [create_effect(EffectType.Damage, 150.0, "爆炸 150 伤害")])
	]
	combo3.total_time = 3.0
	_combos[combo3.id] = combo3

func _init_chain_combos():
	# 暗影打击 - 链式触发
	var combo1 = ComboSkill.new()
	combo1.id = "combo_shadow_strike"
	combo1.name = "暗影打击"
	combo1.description = "穿梭于阴影中连续攻击"
	combo1.combo_type = ComboType.Chain
	combo1.cooldown = 10.0
	combo1.mana_cost = 35.0
	combo1.level_required = 12
	combo1.rarity = 2
	combo1.steps = [
		_create_step("shadow_strike", 0.0, TriggerCondition.Manual, 0.0, 
			[create_effect(EffectType.Damage, 40.0, "暗影斩 40 伤害")]),
		_create_step("shadow_strike", 0.3, TriggerCondition.OnHit, 0.0, 
			[create_effect(EffectType.Damage, 50.0, "穿刺 50 伤害")]),
		_create_step("shadow_strike", 0.3, TriggerCondition.OnHit, 0.0, 
			[create_effect(EffectType.Damage, 70.0, "终结 70 伤害")])
	]
	combo1.total_time = 2.0
	_combos[combo1.id] = combo1

	# 冰火两重天
	var combo2 = ComboSkill.new()
	combo2.id = "combo_fire_ice"
	combo2.name = "冰火两重天"
	combo2.description = "冰霜后接火焰，造成额外伤害"
	combo2.combo_type = ComboType.Chain
	combo2.cooldown = 15.0
	combo2.mana_cost = 50.0
	combo2.level_required = 15
	combo2.rarity = 2
	combo2.steps = [
		_create_step("ice_burst", 0.0, TriggerCondition.Manual, 0.0,
			[create_effect(EffectType.Damage, 60.0, "冰霜 60 伤害"), 
			 create_effect(EffectType.Debuff, 30.0, "减速 30%", 3.0)]),
		_create_step("fire_burst", 0.5, TriggerCondition.OnHit, 0.0,
			[create_effect(EffectType.Damage, 80.0, "火焰 80 伤害")])
	]
	combo2.total_time = 1.5
	_combos[combo2.id] = combo2

func _init_parallel_combos():
	# 全屏护盾 - 并行效果
	var combo1 = ComboSkill.new()
	combo1.id = "combo_shield_wall"
	combo1.name = "护盾壁垒"
	combo1.description = "同时施加多重护盾"
	combo1.combo_type = ComboType.Parallel
	combo1.cooldown = 25.0
	combo1.mana_cost = 60.0
	combo1.level_required = 10
	combo1.rarity = 2
	combo1.steps = [
		_create_step("shield", 0.0, [create_effect(EffectType.Shield, 100.0, "护盾 100")]),
		_create_step("buff", 0.0, [create_effect(EffectType.Buff, 20.0, "防御强化 20%", 10.0)]),
		_create_step("cleanse", 0.0, [create_effect(EffectType.ClearDebuffs, 1.0, "清除减益")])
	]
	combo1.total_time = 0.5
	_combos[combo1.id] = combo1

	# 元素爆发
	var combo2 = ComboSkill.new()
	combo2.id = "combo_elemental_burst"
	combo2.name = "元素爆发"
	combo2.description = "同时触发所有元素之力"
	combo2.combo_type = ComboType.Parallel
	combo2.cooldown = 30.0
	combo2.mana_cost = 100.0
	combo2.level_required = 25
	combo2.rarity = 4
	combo2.steps = [
		_create_step("fire", 0.0, [create_effect(EffectType.Damage, 120.0, "火 120 伤害")]),
		_create_step("ice", 0.0, [create_effect(EffectType.Damage, 100.0, "冰 100 伤害")]),
		_create_step("lightning", 0.0, [create_effect(EffectType.Damage, 80.0, "雷 80 伤害")])
	]
	combo2.total_time = 0.2
	_combos[combo2.id] = combo2

func _init_conditional_combos():
	# 绝地反击 - 条件触发
	var combo1 = ComboSkill.new()
	combo1.id = "combo_desperation"
	combo1.name = "绝地反击"
	combo1.description = "生命低于30%时触发强力反击"
	combo1.combo_type = ComboType.Conditional
	combo1.cooldown = 45.0
	combo1.mana_cost = 0.0
	combo1.level_required = 8
	combo1.rarity = 2
	combo1.steps = [
		_create_step("desperation_strike", 0.0, TriggerCondition.OnHealthBelow, 30.0,
			[create_effect(EffectType.Damage, 150.0, "反击 150 伤害"),
			 create_effect(EffectType.Heal, 50.0, "吸血 50 HP")])
	]
	combo1.total_time = 0.5
	_combos[combo1.id] = combo1

	# 暴击盛宴
	var combo2 = ComboSkill.new()
	combo2.id = "combo_critical_feast"
	combo2.name = "暴击盛宴"
	combo2.description = "暴击时触发连击"
	combo2.combo_type = ComboType.Conditional
	combo2.cooldown = 20.0
	combo2.mana_cost = 25.0
	combo2.level_required = 15
	combo2.rarity = 3
	combo2.steps = [
		_create_step("critical_strike", 0.0, TriggerCondition.OnCritical, 0.0,
			[create_effect(EffectType.Damage, 100.0, "暴击 100 伤害")]),
		_create_step("follow_up", 0.2, TriggerCondition.OnHit, 0.0,
			[create_effect(EffectType.Damage, 80.0, "追击 80 伤害")])
	]
	combo2.total_time = 1.0
	_combos[combo2.id] = combo2

	# 凤凰涅槃
	var combo3 = ComboSkill.new()
	combo3.id = "combo_phoenix"
	combo3.name = "凤凰涅槃"
	combo3.description = "死亡时复活并造成巨额伤害"
	combo3.combo_type = ComboType.Conditional
	combo3.cooldown = 120.0
	combo3.mana_cost = 0.0
	combo3.level_required = 30
	combo3.rarity = 4
	combo3.steps = [
		_create_step("resurrection", 0.0, TriggerCondition.OnHealthBelow, 0.0,
			[create_effect(EffectType.Heal, 100.0, "复活并恢复 100 HP"),
			 create_effect(EffectType.GrantInvulnerability, 1.0, "无敌 3秒", 3.0)]),
		_create_step("rebirth_damage", 0.5, TriggerCondition.Manual, 0.0,
			[create_effect(EffectType.Damage, 200.0, "涅槃之火 200 伤害")])
	]
	combo3.total_time = 2.0
	_combos[combo3.id] = combo3

# 辅助函数
func _create_step(skill_id: String, delay: float, effects: Array) -> ComboStep:
	var step = ComboStep.new()
	step.skill_id = skill_id
	step.delay = delay
	step.condition = TriggerCondition.Manual
	step.condition_value = 0.0
	step.effects = effects
	return step

func _create_step(skill_id: String, delay: float, condition: TriggerCondition, cond_value: float, effects: Array) -> ComboStep:
	var step = ComboStep.new()
	step.skill_id = skill_id
	step.delay = delay
	step.condition = condition
	step.condition_value = cond_value
	step.effects = effects
	return step

func create_effect(effect_type: EffectType, value: float, desc: String, duration: float = 0.0) -> ComboSkillEffect:
	var effect = ComboSkillEffect.new()
	effect.effect_type = effect_type
	effect.value = value
	effect.description = desc
	effect.duration = duration
	return effect
